Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Namespace ExcelCommon

    ''' <summary>One subject at risk over (entry, time], with an explicit event/censor indicator.</summary>
    Public NotInheritable Class SpreadsheetSurvivalObservation
        Public Sub New(time As SpreadsheetNumericObservation, entry As SpreadsheetNumericObservation,
                       isEvent As Boolean, stratum As SpreadsheetCategoryValue)
            If time Is Nothing Then Throw New ArgumentNullException(NameOf(time))
            Me.Time = time
            Me.EntryTime = entry
            Me.IsEvent = isEvent
            Me.Stratum = stratum
        End Sub
        Public ReadOnly Property Time As SpreadsheetNumericObservation
        Public ReadOnly Property EntryTime As SpreadsheetNumericObservation
        Public ReadOnly Property IsEvent As Boolean
        ''' <summary>Nothing when no stratum column was selected.</summary>
        Public ReadOnly Property Stratum As SpreadsheetCategoryValue
        Public ReadOnly Property SelectedRow As Integer
            Get
                Return Time.SelectedRow
            End Get
        End Property
        Public ReadOnly Property WorksheetRow As Integer
            Get
                Return Time.WorksheetRow
            End Get
        End Property
    End Class

    Public NotInheritable Class SpreadsheetSurvivalInput
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly _observations As ReadOnlyCollection(Of SpreadsheetSurvivalObservation)
        Public Sub New(observations As IEnumerable(Of SpreadsheetSurvivalObservation))
            If observations Is Nothing Then Throw New ArgumentNullException(NameOf(observations))
            Dim copied As New List(Of SpreadsheetSurvivalObservation)()
            For Each observation As SpreadsheetSurvivalObservation In observations
                If observation Is Nothing Then Throw New ArgumentException("Survival rows cannot be null.", NameOf(observations))
                copied.Add(observation)
            Next
            _observations = copied.AsReadOnly()
        End Sub
        Public ReadOnly Property Observations As IReadOnlyList(Of SpreadsheetSurvivalObservation)
            Get
                Return _observations
            End Get
        End Property
        Public ReadOnly Property Count As Integer
            Get
                Return _observations.Count
            End Get
        End Property
        Public ReadOnly Property EventCount As Integer
            Get
                Dim count As Integer = 0
                For Each row As SpreadsheetSurvivalObservation In _observations
                    If row.IsEvent Then count += 1
                Next
                Return count
            End Get
        End Property
    End Class

    ''' <summary>One original row of subject/visit/response data; group is optional.</summary>
    Public NotInheritable Class SpreadsheetRepeatedObservation
        Public Sub New(subject As SpreadsheetCategoryValue, visit As SpreadsheetCategoryValue,
                       visitTime As Nullable(Of Double), visitDate As Nullable(Of DateTime),
                       response As SpreadsheetNumericObservation, treatmentGroup As SpreadsheetCategoryValue)
            If subject Is Nothing Then Throw New ArgumentNullException(NameOf(subject))
            If response Is Nothing Then Throw New ArgumentNullException(NameOf(response))
            If visitTime.HasValue AndAlso (Double.IsNaN(visitTime.Value) OrElse Double.IsInfinity(visitTime.Value)) Then
                Throw New ArgumentException("Visit time must be finite.", NameOf(visitTime))
            End If
            Me.Subject = subject
            Me.Visit = visit
            Me.VisitTime = visitTime
            Me.VisitDate = visitDate
            Me.Response = response
            Me.TreatmentGroup = treatmentGroup
        End Sub
        Public ReadOnly Property Subject As SpreadsheetCategoryValue
        ''' <summary>Nothing only in mixed-model mode with VisitColumnIndex = -1.</summary>
        Public ReadOnly Property Visit As SpreadsheetCategoryValue
        ''' <summary>Raw finite numeric visit coordinate when supplied, otherwise Nothing.</summary>
        Public ReadOnly Property VisitTime As Nullable(Of Double)
        ''' <summary>Calendar date only when the caller explicitly requests date interpretation.</summary>
        Public ReadOnly Property VisitDate As Nullable(Of DateTime)
        Public ReadOnly Property Response As SpreadsheetNumericObservation
        Public ReadOnly Property TreatmentGroup As SpreadsheetCategoryValue
        Public ReadOnly Property SelectedRow As Integer
            Get
                Return Response.SelectedRow
            End Get
        End Property
        Public ReadOnly Property WorksheetRow As Integer
            Get
                Return Response.WorksheetRow
            End Get
        End Property
    End Class

    Public NotInheritable Class SpreadsheetRepeatedSubjectInput
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly _observations As ReadOnlyCollection(Of SpreadsheetRepeatedObservation)
        Private ReadOnly _subjects As ReadOnlyCollection(Of SpreadsheetCategoryValue)
        Public Sub New(observations As IEnumerable(Of SpreadsheetRepeatedObservation))
            If observations Is Nothing Then Throw New ArgumentNullException(NameOf(observations))
            Dim copied As New List(Of SpreadsheetRepeatedObservation)()
            Dim subjects As New List(Of SpreadsheetCategoryValue)()
            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
            For Each observation As SpreadsheetRepeatedObservation In observations
                If observation Is Nothing Then Throw New ArgumentException("Repeated rows cannot be null.", NameOf(observations))
                copied.Add(observation)
                If seen.Add(observation.Subject.Key) Then subjects.Add(observation.Subject)
            Next
            _observations = copied.AsReadOnly()
            _subjects = subjects.AsReadOnly()
        End Sub
        Public ReadOnly Property Observations As IReadOnlyList(Of SpreadsheetRepeatedObservation)
            Get
                Return _observations
            End Get
        End Property
        Public ReadOnly Property Subjects As IReadOnlyList(Of SpreadsheetCategoryValue)
            Get
                Return _subjects
            End Get
        End Property
        Public ReadOnly Property Count As Integer
            Get
                Return _observations.Count
            End Get
        End Property
    End Class

    ''' <summary>Mixed-model row with raw, type-preserving fixed-effect cells and random-group levels.</summary>
    Public NotInheritable Class SpreadsheetMixedModelObservation
        Private ReadOnly _covariates As ReadOnlyCollection(Of SpreadsheetFrameCell)
        Private ReadOnly _randomGroups As ReadOnlyCollection(Of SpreadsheetCategoryValue)
        Public Sub New(repeated As SpreadsheetRepeatedObservation,
                       covariates As IEnumerable(Of SpreadsheetFrameCell),
                       randomGroups As IEnumerable(Of SpreadsheetCategoryValue))
            If repeated Is Nothing Then Throw New ArgumentNullException(NameOf(repeated))
            If covariates Is Nothing Then Throw New ArgumentNullException(NameOf(covariates))
            If randomGroups Is Nothing Then Throw New ArgumentNullException(NameOf(randomGroups))
            Dim c As New List(Of SpreadsheetFrameCell)(covariates)
            Dim g As New List(Of SpreadsheetCategoryValue)(randomGroups)
            If c.Contains(Nothing) OrElse g.Contains(Nothing) Then Throw New ArgumentException("Mixed-model row contains null values.")
            Me.Repeated = repeated
            _covariates = c.AsReadOnly()
            _randomGroups = g.AsReadOnly()
        End Sub
        Public ReadOnly Property Repeated As SpreadsheetRepeatedObservation
        Public ReadOnly Property Covariates As IReadOnlyList(Of SpreadsheetFrameCell)
            Get
                Return _covariates
            End Get
        End Property
        Public ReadOnly Property RandomGroups As IReadOnlyList(Of SpreadsheetCategoryValue)
            Get
                Return _randomGroups
            End Get
        End Property
    End Class

    Public NotInheritable Class SpreadsheetMixedModelInput
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly _rows As ReadOnlyCollection(Of SpreadsheetMixedModelObservation)
        Private ReadOnly _covariateNames As ReadOnlyCollection(Of String)
        Private ReadOnly _randomGroupNames As ReadOnlyCollection(Of String)
        Public Sub New(rows As IEnumerable(Of SpreadsheetMixedModelObservation),
                       covariateNames As IEnumerable(Of String), randomGroupNames As IEnumerable(Of String))
            If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))
            If covariateNames Is Nothing Then Throw New ArgumentNullException(NameOf(covariateNames))
            If randomGroupNames Is Nothing Then Throw New ArgumentNullException(NameOf(randomGroupNames))
            Dim copied As New List(Of SpreadsheetMixedModelObservation)(rows)
            Dim cov As New List(Of String)(covariateNames)
            Dim groups As New List(Of String)(randomGroupNames)
            If copied.Contains(Nothing) Then Throw New ArgumentException("Null mixed-model row.", NameOf(rows))
            For Each item As SpreadsheetMixedModelObservation In copied
                If item.Covariates.Count <> cov.Count OrElse item.RandomGroups.Count <> groups.Count Then
                    Throw New ArgumentException("Mixed-model row schema mismatch.", NameOf(rows))
                End If
            Next
            _rows = copied.AsReadOnly()
            _covariateNames = cov.AsReadOnly()
            _randomGroupNames = groups.AsReadOnly()
        End Sub
        Public ReadOnly Property Rows As IReadOnlyList(Of SpreadsheetMixedModelObservation)
            Get
                Return _rows
            End Get
        End Property
        Public ReadOnly Property CovariateNames As IReadOnlyList(Of String)
            Get
                Return _covariateNames
            End Get
        End Property
        Public ReadOnly Property RandomGroupNames As IReadOnlyList(Of String)
            Get
                Return _randomGroupNames
            End Get
        End Property
    End Class

    ''' <summary>Column indices are zero-based in the selected rectangular range; -1 means absent.</summary>
    Public NotInheritable Class SpreadsheetSurvivalParseOptions
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
        Public Property TimeColumnIndex As Integer = 0
        Public Property StatusColumnIndex As Integer = 1
        Public Property EntryColumnIndex As Integer = -1
        Public Property StratumColumnIndex As Integer = -1
        Public Property MinimumObservations As Integer = 1
        Public Property RequirePositiveTime As Boolean = True
        Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
        Public Property AllowBooleanStatus As Boolean = False
        ''' <summary>Only opt-in text spellings are accepted as status labels.</summary>
        Public Property EventText As String = Nothing
        Public Property CensorText As String = Nothing
        Public Property IgnoreStatusCase As Boolean = False
        Public Property IgnoreCategoryCase As Boolean = False
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
    End Class

    Public Class SpreadsheetRepeatedParseOptions
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
        Public Property SubjectColumnIndex As Integer = 0
        Public Property VisitColumnIndex As Integer = 1
        Public Property ResponseColumnIndex As Integer = 2
        Public Property GroupColumnIndex As Integer = -1
        Public Property MinimumObservations As Integer = 1
        Public Property MinimumSubjects As Integer = 1
        Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
        Public Property RejectDuplicateSubjectVisits As Boolean = True
        ''' <summary>When enabled visits must be finite numeric and strictly increasing within each subject in source order.</summary>
        Public Property RequireAscendingVisits As Boolean = False
        ''' <summary>Requires a declared Excel date system or host-supplied DateSerialOrigin.</summary>
        Public Property InterpretVisitAsDate As Boolean = False
        Public Property IgnoreCategoryCase As Boolean = False
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
    End Class

    Public NotInheritable Class SpreadsheetMixedModelParseOptions
        Inherits SpreadsheetRepeatedParseOptions
        ''' <summary>VisitColumnIndex = -1 is permitted for cross-sectional LMM rows.</summary>
        Public Property FixedEffectColumnIndices As Integer() = Array.Empty(Of Integer)()
        Public Property RandomGroupColumnIndices As Integer() = Array.Empty(Of Integer)()
    End Class
End Namespace
