Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.survival

Namespace ExcelCommon
    Public Enum SurvivalAnalysisMode
        KaplanMeier
        LogRank
        Cox
    End Enum
    Public Enum SurvivalWeight
        Logrank
        TaroneWare
        GehanBreslow
        Peto
        ModifiedPeto
    End Enum
    Public Enum SurvivalOutputTarget
        NewWorksheet
        NewWorkbook
        SelectedRange
    End Enum
    Public Enum SurvivalRole
        Time
        Status
        Group
        Stratum
    End Enum

    ''' <summary>Logical columns in the original joined transport, not worksheet columns.</summary>
    Public NotInheritable Class SurvivalRoleColumns
        Public Sub New(time As Integer, status As Integer, Optional group As Integer = -1, Optional stratum As Integer = -1)
            Me.Time = time : Me.Status = status : Me.Group = group : Me.Stratum = stratum
        End Sub
        Public ReadOnly Property Time As Integer
        Public ReadOnly Property Status As Integer
        Public ReadOnly Property Group As Integer
        Public ReadOnly Property Stratum As Integer
        Public Function Column(role As SurvivalRole) As Integer
            Select Case role
                Case SurvivalRole.Time : Return Time
                Case SurvivalRole.Status : Return Status
                Case SurvivalRole.Group : Return Group
                Case SurvivalRole.Stratum : Return Stratum
                Case Else : Return -1
            End Select
        End Function
    End Class

    ''' <summary>Literal ordinal labels, with the same retained row axis as the numeric roles.</summary>
    Public NotInheritable Class SurvivalLabels
        Private ReadOnly _values As String()
        Private ReadOnly _rows As Integer()
        Public Sub New(key As String, values() As String, rows() As Integer)
            Me.Key = key
            _values = If(values Is Nothing, Nothing, DirectCast(values.Clone(), String()))
            _rows = If(rows Is Nothing, Nothing, DirectCast(rows.Clone(), Integer()))
        End Sub
        Public ReadOnly Property Key As String
        Public ReadOnly Property Values As String()
            Get
                Return If(_values Is Nothing, Nothing, DirectCast(_values.Clone(), String()))
            End Get
        End Property
        Public ReadOnly Property RowIds As Integer()
            Get
                Return If(_rows Is Nothing, Nothing, DirectCast(_rows.Clone(), Integer()))
            End Get
        End Property
    End Class

    ''' <summary>Post-acquisition snapshot. Status stays numeric until validation, so fractional
    ''' codes cannot be rounded into events. No filtering, trimming, sorting or invented coordinates.
    ''' SpreadsheetSurvivalInput's Boolean event cannot represent invalid status diagnostics.</summary>
    Public NotInheritable Class SurvivalInput
        Public Sub New(time As RegressionVector, status As RegressionVector, Optional groups As SurvivalLabels = Nothing,
                       Optional strata As SurvivalLabels = Nothing, Optional provenance As SpreadsheetTransportFrame = Nothing,
                       Optional roleColumns As SurvivalRoleColumns = Nothing)
            Me.Time = time : Me.Status = status : Me.Groups = groups : Me.Strata = strata : Me.Provenance = provenance
            Me.RoleColumns = roleColumns
        End Sub
        Public ReadOnly Property Time As RegressionVector
        Public ReadOnly Property Status As RegressionVector
        Public ReadOnly Property Groups As SurvivalLabels
        Public ReadOnly Property Strata As SurvivalLabels
        ''' <summary>The accepted transport frame retains each original area/column; never a first-area rectangle.</summary>
        Public ReadOnly Property Provenance As SpreadsheetTransportFrame
        Public ReadOnly Property RoleColumns As SurvivalRoleColumns
        Public Function SourceAt(sourceRowId As Integer, role As SurvivalRole) As SpreadsheetTransportCoordinate
            If Provenance Is Nothing OrElse RoleColumns Is Nothing Then Return Nothing
            Dim column = RoleColumns.Column(role), index = Provenance.RowIds.ToList().IndexOf(sourceRowId)
            If index < 0 OrElse column < 0 OrElse column >= Provenance.Data.ColumnCount Then Return Nothing
            Return Provenance.SourceAt(index, column)
        End Function
        Public ReadOnly Property GroupLabels As String()
            Get
                Return If(Groups Is Nothing, New String() {"0"}, If(Groups.Values, Array.Empty(Of String)()).Distinct(StringComparer.Ordinal).ToArray())
            End Get
        End Property
        Public ReadOnly Property StratumLabels As String()
            Get
                Return If(Strata Is Nothing, New String() {"0"}, If(Strata.Values, Array.Empty(Of String)()).Distinct(StringComparer.Ordinal).ToArray())
            End Get
        End Property
    End Class

    ''' <summary>Caller-owned builder; workflow and plot snapshots return independent copies.
    ''' Chart intent never instantiates a chart or resolves an output range.</summary>
    Public NotInheritable Class SurvivalOptions
        Public Property Alpha As Double = 0.05
        Public Property Weight As SurvivalWeight = SurvivalWeight.Logrank
        Public Property MedianConfidenceInterval As Boolean = True
        Public Property DetailedTable As Boolean = True
        Public Property EqualityOfMedians As Boolean = True
        Public Property FixedTimeComparison As Boolean = True
        Public Property PlotConfidenceIntervals As Boolean = True
        Public Property ShowLegend As Boolean = True
        Public Property DisplayTitle As Boolean
        Public Property Title As String = "Kaplan–Meier Plot"
        Public Property XUnits As String = "days"
        Public Property OutputTarget As SurvivalOutputTarget = SurvivalOutputTarget.NewWorksheet
        Public Function Copy() As SurvivalOptions
            Return DirectCast(MemberwiseClone(), SurvivalOptions)
        End Function
    End Class

    Public NotInheritable Class SurvivalWorkflowIntent
        Private ReadOnly _options As SurvivalOptions
        Public Sub New(mode As SurvivalAnalysisMode, input As SurvivalInput, Optional options As SurvivalOptions = Nothing)
            Me.Mode = mode : Me.Input = input : _options = If(options, New SurvivalOptions()).Copy()
            Validation = SurvivalWorkflowPolicy.Validate(mode, input, _options)
        End Sub
        Public ReadOnly Property Mode As SurvivalAnalysisMode
        Public ReadOnly Property Input As SurvivalInput
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property Options As SurvivalOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public ReadOnly Property GroupCount As Integer
            Get
                Return If(Input Is Nothing, 0, Input.GroupLabels.Length)
            End Get
        End Property
        Public ReadOnly Property RunWeightedTest As Boolean
            Get
                Return GroupCount > 1
            End Get
        End Property
        Public ReadOnly Property RunMedianTest As Boolean
            Get
                Return _options.EqualityOfMedians AndAlso GroupCount > 1
            End Get
        End Property
        Public ReadOnly Property RunFixedTimeTest As Boolean
            Get
                Return _options.FixedTimeComparison AndAlso GroupCount = 2
            End Get
        End Property
        Public Function Execute() As Survival_KM_LR
            If Not Validation.IsValid Then Throw New ArgumentException("survival.request.invalid: Resolve portable diagnostics before execution.")
            Return ExecuteAcquiredRecords(SurvivalWorkflowPolicy.CreateRecords(Input, Mode))
        End Function
        ''' <summary>Shipping compatibility seam: existing host conversions/Core error paths
        ''' have already run. Diagnostics are observational here, never a new Windows refusal.</summary>
        Public Function ExecuteAcquiredRecords(records As List(Of SurvivalRecord)) As Survival_KM_LR
            If Mode = SurvivalAnalysisMode.Cox Then Throw New InvalidOperationException("survival.mode.cox: Use CoxWorkflowIntent for Cox fitting.")
            Dim fit As New Survival_KM_LR(SurvivalWorkflowPolicy.CopyRecords(records))
            Dim method As String = Nothing
            SurvivalWorkflowPolicy.TryWeightName(_options.Weight, method)
            If RunWeightedTest Then fit.WeightedLogRankTest(If(method, String.Empty), _options.Alpha)
            If _options.MedianConfidenceInterval Then fit.BrookmeyerCrowleyMedianSurvivalCI(_options.Alpha)
            If _options.DetailedTable Then fit.SurvivalCurveTabularOutput(_options.Alpha)
            If RunMedianTest Then fit.EqualityOfMedianTest()
            If RunFixedTimeTest Then fit.CompareCurveFixTimePoint()
            Return fit
        End Function
    End Class

    Public NotInheritable Class SurvivalWorkflowPolicy
        Private Sub New()
        End Sub
        Public Const MaximumDiagnostics As Integer = 128
        Public Shared Function TryWeightName(weight As SurvivalWeight, ByRef name As String) As Boolean
            name = Nothing
            Select Case weight
                Case SurvivalWeight.Logrank : name = "Logrank"
                Case SurvivalWeight.TaroneWare : name = "Tarone-Ware"
                Case SurvivalWeight.GehanBreslow : name = "Gehan-Breslow"
                Case SurvivalWeight.Peto : name = "Peto"
                Case SurvivalWeight.ModifiedPeto : name = "modified Peto"
            End Select
            Return name IsNot Nothing
        End Function
        Friend Shared Function Finite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function
        Friend Shared Sub Issue(list As List(Of RegressionDiagnostic), field As String, message As String,
                                Optional row As Nullable(Of Integer) = Nothing, Optional blocking As Boolean = True)
            If list.Count < MaximumDiagnostics Then list.Add(New RegressionDiagnostic("survival." & field, field, message, row, blocking:=blocking))
        End Sub
        Public Shared Function Validate(mode As SurvivalAnalysisMode, input As SurvivalInput, options As SurvivalOptions) As RegressionValidation
            Dim issues As New List(Of RegressionDiagnostic)
            If Not [Enum].IsDefined(GetType(SurvivalAnalysisMode), mode) Then Issue(issues, "mode", "Unknown survival mode.")
            If options Is Nothing Then options = New SurvivalOptions()
            If Not Finite(options.Alpha) OrElse options.Alpha <= 0 OrElse options.Alpha >= 1 Then Issue(issues, "alpha", "Alpha must be finite and between 0 and 1.")
            Dim name As String = Nothing
            If Not TryWeightName(options.Weight, name) Then Issue(issues, "weight", "Unsupported weighted log-rank option.")
            If Not [Enum].IsDefined(GetType(SurvivalOutputTarget), options.OutputTarget) Then Issue(issues, "outputTarget", "Unknown output target metadata.")
            If input Is Nothing OrElse input.Time Is Nothing OrElse input.Status Is Nothing Then
                Issue(issues, "input.required", "Time and binary status roles are required.")
                Return New RegressionValidation(issues, 0, 0, {})
            End If
            Dim times = input.Time.Values, status = input.Status.Values, rows = input.Time.RowIds
            If String.IsNullOrWhiteSpace(input.Time.Key) OrElse String.IsNullOrWhiteSpace(input.Status.Key) Then Issue(issues, "roles", "Time and status role names are required.")
            If times Is Nothing OrElse status Is Nothing OrElse rows Is Nothing Then
                Issue(issues, "shape", "Time, status and source row axes are required.")
                Return New RegressionValidation(issues, 0, 0, {})
            End If
            If times.Length = 0 Then Issue(issues, "empty", "No observations supplied.")
            If times.Length <> status.Length OrElse times.Length <> rows.Length OrElse input.Status.RowIds Is Nothing OrElse Not rows.SequenceEqual(input.Status.RowIds) Then Issue(issues, "shape", "Time, status and source row axes must agree exactly.")
            If rows.Distinct().Count() <> rows.Length Then Issue(issues, "rows.duplicate", "Source row identities must be distinct.")
            For Each labels In {input.Groups, input.Strata}
                If labels Is Nothing Then Continue For
                Dim values = labels.Values, ids = labels.RowIds
                If values Is Nothing OrElse ids Is Nothing OrElse values.Length <> times.Length OrElse Not rows.SequenceEqual(ids) Then
                    Issue(issues, "labels.shape", "Selected labels must align with the original source rows.")
                ElseIf values.Any(Function(v) String.IsNullOrWhiteSpace(v)) OrElse String.IsNullOrWhiteSpace(labels.Key) Then
                    Issue(issues, "labels.missing", "Selected labels and role names cannot be missing; labels are never trimmed or recoded.")
                End If
            Next
            If mode = SurvivalAnalysisMode.LogRank AndAlso input.Groups Is Nothing Then Issue(issues, "group.required", "Log-rank mode requires a group role.")
            If mode = SurvivalAnalysisMode.KaplanMeier AndAlso input.Strata IsNot Nothing Then Issue(issues, "strata.mode", "The KM dialog has no stratum role; use stratified log-rank mode.")
            If mode = SurvivalAnalysisMode.Cox AndAlso input.Groups IsNot Nothing Then Issue(issues, "group.mode", "Cox has strata and predictors, not KM comparison groups.")
            For i = 0 To times.Length - 1
                If Not Finite(times(i)) OrElse times(i) < 0 Then Issue(issues, "time", "Time must be finite and nonnegative.", If(i < rows.Length, CType(rows(i), Nullable(Of Integer)), Nothing))
                If issues.Count >= MaximumDiagnostics Then Exit For
            Next
            For i = 0 To status.Length - 1
                If Not Finite(status(i)) OrElse (status(i) <> 0 AndAlso status(i) <> 1) Then Issue(issues, "status", "Status must be exactly 0 (censored) or 1 (event).", If(i < rows.Length, CType(rows(i), Nullable(Of Integer)), Nothing))
                If issues.Count >= MaximumDiagnostics Then Exit For
            Next
            If status.Length > 0 AndAlso Not status.Any(Function(s) s = 1) Then Issue(issues, "events.none", "No events: inference/median/baseline outputs may be undefined in Core.", blocking:=False)
            If input.Provenance IsNot Nothing AndAlso rows.Any(Function(id) Not input.Provenance.RowIds.Contains(id)) Then Issue(issues, "provenance", "A retained source row is absent from the transport frame.")
            Return New RegressionValidation(issues, 0, 0, {})
        End Function
        Public Shared Function CreateRecords(input As SurvivalInput, mode As SurvivalAnalysisMode) As List(Of SurvivalRecord)
            Dim validation = Validate(mode, input, New SurvivalOptions())
            If Not validation.IsValid Then Throw New ArgumentException("survival.input.invalid")
            Dim t = input.Time.Values, ids = input.Time.RowIds, err = String.Empty
            Dim groups = If(input.Groups Is Nothing, Enumerable.Repeat("0", t.Length).ToArray(), input.Groups.Values)
            Dim strata = If(input.Strata Is Nothing, Enumerable.Repeat("0", t.Length).ToArray(), input.Strata.Values)
            Dim records = Survival.CreatSurvivalData(t, input.Status.Values.Select(Function(s) CInt(s)).ToArray(), groups, strata, err)
            If records Is Nothing Then Throw New ArgumentException(err)
            For i = 0 To records.Count - 1
                Dim r = records(i) : r.Index = ids(i)
                If mode = SurvivalAnalysisMode.Cox Then r.Stratum = strata(i)
                records(i) = r
            Next
            Return records
        End Function
        Public Shared Function CopyRecords(records As IEnumerable(Of SurvivalRecord)) As List(Of SurvivalRecord)
            If records Is Nothing Then Throw New ArgumentNullException(NameOf(records))
            Return records.Select(Function(r)
                                      r.Covariates = If(r.Covariates Is Nothing, Nothing, DirectCast(r.Covariates.Clone(), Double()))
                                      Return r
                                  End Function).ToList()
        End Function
    End Class
End Namespace
