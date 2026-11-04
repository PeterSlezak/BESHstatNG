Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.Multivariate

Namespace ExcelCommon
    Public NotInheritable Class MultivariateDiscriminantObservation
        Friend Sub New(inputOrdinal As Integer, sourceRowId As Integer?, activeOrdinal As Integer?, removedOrdinal As Integer?, sourceLabel As String, displayLabel As String, actualClass As String)
            Me.InputOrdinal = inputOrdinal : Me.SourceRowId = sourceRowId : Me.ActiveOrdinal = activeOrdinal
            Me.RemovedOrdinal = removedOrdinal : Me.SourceLabel = sourceLabel : Me.DisplayLabel = displayLabel : Me.ActualClass = actualClass
        End Sub
        Public ReadOnly Property InputOrdinal As Integer
        Public ReadOnly Property SourceRowId As Integer?
        Public ReadOnly Property ActiveOrdinal As Integer?
        Public ReadOnly Property RemovedOrdinal As Integer?
        Public ReadOnly Property SourceLabel As String
        Public ReadOnly Property DisplayLabel As String
        Public ReadOnly Property ActualClass As String
    End Class
    Public NotInheritable Class MultivariateDiscriminantClass
        Friend Sub New(index As Integer, value As DiscriminantGroupStatistics)
            Me.Index = index : Label = value.GroupLabel : Count = value.Count : Prior = value.PriorProbability
            CovarianceShape = MultivariateArrayShape.Matrix(value.CovarianceWorking)
            InverseCovarianceShape = MultivariateArrayShape.Matrix(value.InverseCovarianceWorking)
            RegularizationUsed = value.RegularizationUsed
        End Sub
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property Label As String
        Public ReadOnly Property Count As Integer
        Public ReadOnly Property Prior As Double
        Public ReadOnly Property RegularizationUsed As Double
        Public ReadOnly Property CovarianceShape As MultivariateArrayShape
        Public ReadOnly Property InverseCovarianceShape As MultivariateArrayShape
    End Class
    Public NotInheritable Class MultivariateDiscriminantCase
        Friend Sub New(index As Integer, ordinal As Integer, sourceRowId As Integer?, label As String, actual As String, predicted As String, fold As Integer?, assignedPosterior As Double)
            Me.Index = index : Me.CoreInputOrdinal = ordinal : Me.SourceRowId = sourceRowId
            Me.DisplayLabel = label : Me.ActualClass = actual : Me.PredictedClass = predicted
            Me.Fold = fold : Me.AssignedPosterior = assignedPosterior
        End Sub
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property CoreInputOrdinal As Integer
        Public ReadOnly Property SourceRowId As Integer?
        Public ReadOnly Property DisplayLabel As String
        Public ReadOnly Property ActualClass As String
        Public ReadOnly Property PredictedClass As String
        Public ReadOnly Property Fold As Integer?
        Public ReadOnly Property AssignedPosterior As Double
    End Class
    ''' <summary>Actual Core confusion snapshot: observed rows, predicted columns.
    ''' Percentages, totals and counts are copied, never recomputed or transposed.</summary>
    Public NotInheritable Class MultivariateDiscriminantConfusion
        Private ReadOnly _counts As Double(,), _rowTotals As Double(), _columnTotals As Double(), _recall As Double(), _precision As Double()
        Friend Sub New(value As DiscriminantConfusionMatrix)
            ObservedClasses = Array.AsReadOnly(DirectCast(value.ClassLabels.Clone(), String()))
            PredictedClasses = Array.AsReadOnly(DirectCast(value.ClassLabels.Clone(), String()))
            _counts = DirectCast(value.Counts.Clone(), Double(,))
            _rowTotals = DirectCast(value.RowTotals.Clone(), Double()) : _columnTotals = DirectCast(value.ColumnTotals.Clone(), Double())
            _recall = DirectCast(value.RecallPct.Clone(), Double()) : _precision = DirectCast(value.PrecisionPct.Clone(), Double())
            AccuracyFraction = value.OverallAccuracy : AccuracyPercent = value.OverallAccuracyPct
            TableShape = MultivariateArrayShape.Matrix(value.ToObjectTable())
        End Sub
        Public ReadOnly Property ObservedClasses As IReadOnlyList(Of String)
        Public ReadOnly Property PredictedClasses As IReadOnlyList(Of String)
        Public ReadOnly Property TableShape As MultivariateArrayShape
        Public ReadOnly Property AccuracyFraction As Double
        Public ReadOnly Property AccuracyPercent As Double
        Public ReadOnly Property Counts As Double(,)
            Get
                Return DirectCast(_counts.Clone(), Double(,))
            End Get
        End Property
        Public ReadOnly Property RowTotals As Double()
            Get
                Return DirectCast(_rowTotals.Clone(), Double())
            End Get
        End Property
        Public ReadOnly Property ColumnTotals As Double()
            Get
                Return DirectCast(_columnTotals.Clone(), Double())
            End Get
        End Property
        Public ReadOnly Property RecallPercent As Double()
            Get
                Return DirectCast(_recall.Clone(), Double())
            End Get
        End Property
        Public ReadOnly Property PrecisionPercent As Double()
            Get
                Return DirectCast(_precision.Clone(), Double())
            End Get
        End Property
    End Class
    Public NotInheritable Class MultivariateDiscriminantPrediction
        Friend Sub New(input As MultivariateInputIntent, classes() As String, value As DiscriminantPredictionResult, training As Boolean)
            Me.IsTrainingResubstitution = training : Me.ValidationMode = value.ValidationMode
            ScoreClasses = Array.AsReadOnly(DirectCast(classes.Clone(), String()))
            PosteriorShape = MultivariateArrayShape.Matrix(value.PosteriorProbabilities)
            RawScoreShape = MultivariateArrayShape.Matrix(value.RawScores)
            DistanceShape = MultivariateArrayShape.Matrix(value.SquaredDistances)
            CasewiseShape = MultivariateArrayShape.Matrix(value.ToCasewiseTable(classes))
            Confusion = New MultivariateDiscriminantConfusion(value.Confusion)
            Dim rows = input.SourceRowIds, cases As New List(Of MultivariateDiscriminantCase)
            For i = 0 To value.RowIndices.Length - 1
                Dim ordinal = value.RowIndices(i)
                cases.Add(New MultivariateDiscriminantCase(i, ordinal,
                    If(rows IsNot Nothing AndAlso ordinal > 0 AndAlso ordinal <= rows.Length, CType(rows(ordinal - 1), Integer?), Nothing),
                    value.RowLabels(i), value.ActualGroupLabels(i), value.PredictedGroupLabels(i),
                    If(value.FoldAssignments Is Nothing, Nothing, CType(value.FoldAssignments(i), Integer?)), value.AssignedPosteriorProbability(i)))
            Next
            Me.Cases = cases.AsReadOnly()
        End Sub
        Public ReadOnly Property IsTrainingResubstitution As Boolean
        Public ReadOnly Property ValidationMode As DiscriminantValidationMode
        Public ReadOnly Property ScoreClasses As IReadOnlyList(Of String)
        Public ReadOnly Property Cases As IReadOnlyList(Of MultivariateDiscriminantCase)
        Public ReadOnly Property PosteriorShape As MultivariateArrayShape
        Public ReadOnly Property RawScoreShape As MultivariateArrayShape
        Public ReadOnly Property DistanceShape As MultivariateArrayShape
        Public ReadOnly Property CasewiseShape As MultivariateArrayShape
        Public ReadOnly Property Confusion As MultivariateDiscriminantConfusion
    End Class
    Public NotInheritable Class MultivariateDiscriminantProjection
        Friend Sub New(request As MultivariateDiscriminantRequest, validation As MultivariateDiscriminantValidationRequest,
                       priors As MultivariateUserPriorIntent, model As DiscriminantAnalysis)
            Me.Request = request : Me.ValidationRequest = validation : Me.UserPriors = priors
            Classes = Array.AsReadOnly(DirectCast(model.GroupLabels.Clone(), String()))
            GroupStatistics = model.GroupStatistics.Select(Function(g, i) New MultivariateDiscriminantClass(i, g)).ToList().AsReadOnly()
            CanonicalFunctions = Enumerable.Range(0, If(model.CanonicalEigenvalues Is Nothing, 0, model.CanonicalEigenvalues.Length)).Select(Function(i) New MultivariateAxisItem(i, Nothing)).ToList().AsReadOnly()
            Dim prepared = model.PreparedData, columns = request.Predictors
            Variables = prepared.VariableNames.Select(Function(v, i) New MultivariateAxisItem(i, v, sourcePosition:=i + 1, sourceKey:=If(i < columns.Count, columns(i).SourceKey, Nothing))).ToList().AsReadOnly()
            WorkingDataShape = MultivariateArrayShape.Matrix(prepared.WorkingData)
            ActiveOriginalShape = MultivariateArrayShape.Matrix(prepared.ActiveOriginalData)
            Dim active As New Dictionary(Of Integer, Integer), removed As New Dictionary(Of Integer, Integer)
            For i = 0 To prepared.ActiveOriginalIndices.Length - 1 : active(prepared.ActiveOriginalIndices(i)) = i : Next
            For i = 0 To prepared.RemovedOriginalIndices.Length - 1 : removed(prepared.RemovedOriginalIndices(i)) = i : Next
            Dim rows = request.Input.SourceRowIds, labels = request.Input.RowLabels, observations As New List(Of MultivariateDiscriminantObservation)
            For ordinal = 1 To request.Input.RowCount
                Dim a = If(active.ContainsKey(ordinal), active(ordinal), -1), r = If(removed.ContainsKey(ordinal), removed(ordinal), -1)
                observations.Add(New MultivariateDiscriminantObservation(ordinal,
                    If(rows IsNot Nothing AndAlso ordinal <= rows.Length, CType(rows(ordinal - 1), Integer?), Nothing),
                    If(a >= 0, CType(a + 1, Integer?), Nothing), If(r >= 0, CType(r + 1, Integer?), Nothing),
                    If(labels IsNot Nothing AndAlso ordinal <= labels.Length, labels(ordinal - 1), Nothing),
                    If(a >= 0, prepared.ActiveRowLabels(a), If(r >= 0, prepared.RemovedRowLabels(r), Nothing)),
                    If(a >= 0, prepared.ActiveGroupLabels(a), Nothing)))
            Next
            Me.Observations = observations.AsReadOnly()
            Training = New MultivariateDiscriminantPrediction(request.Input, model.GroupLabels, model.TrainingClassification, True)
            If model.ValidationClassification IsNot Nothing Then Me.Validation = New MultivariateDiscriminantPrediction(request.Input, model.GroupLabels, model.ValidationClassification, False)
            Plots = MultivariateReportPolicy.PlotPlan(MultivariateAnalysisKind.Discriminant)
            Dim diagnostics As New List(Of RegressionDiagnostic)
            If observations.Any(Function(o) o.SourceRowId.HasValue AndAlso o.SourceRowId.Value <> o.InputOrdinal) Then diagnostics.Add(New RegressionDiagnostic("C20-DA-ROWID-DISPLAY-01", "rows", "Core casewise Row uses input ordinals, not physical source RowIds. No output remapping is activated.", blocking:=False))
            Me.Diagnostics = New RegressionValidation(diagnostics, 0, 0, {})
        End Sub
        Public ReadOnly Property Request As MultivariateDiscriminantRequest
        Public ReadOnly Property ValidationRequest As MultivariateDiscriminantValidationRequest
        Public ReadOnly Property UserPriors As MultivariateUserPriorIntent
        Public ReadOnly Property Classes As IReadOnlyList(Of String)
        Public ReadOnly Property GroupStatistics As IReadOnlyList(Of MultivariateDiscriminantClass)
        Public ReadOnly Property Variables As IReadOnlyList(Of MultivariateAxisItem)
        Public ReadOnly Property CanonicalFunctions As IReadOnlyList(Of MultivariateAxisItem)
        Public ReadOnly Property Observations As IReadOnlyList(Of MultivariateDiscriminantObservation)
        Public ReadOnly Property WorkingDataShape As MultivariateArrayShape
        Public ReadOnly Property ActiveOriginalShape As MultivariateArrayShape
        Public ReadOnly Property Training As MultivariateDiscriminantPrediction
        Public ReadOnly Property Validation As MultivariateDiscriminantPrediction
        Public ReadOnly Property Plots As IReadOnlyList(Of MultivariatePlotIntent)
        Public ReadOnly Property Diagnostics As RegressionValidation
        Public Function ProjectReport(tables As IEnumerable(Of ResultTable)) As MultivariateModelReport
            Return New MultivariateModelReport(Request.Input, tables)
        End Function
    End Class
    Public NotInheritable Class MultivariateDiscriminantProjectionPolicy
        Private Sub New()
        End Sub
        Public Shared Function Project(request As MultivariateDiscriminantRequest, validation As MultivariateDiscriminantValidationRequest,
                                       priors As MultivariateUserPriorIntent, model As DiscriminantAnalysis) As MultivariateDiscriminantProjection
            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            If validation Is Nothing Then Throw New ArgumentNullException(NameOf(validation))
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            Return New MultivariateDiscriminantProjection(request, validation, priors, model)
        End Function
    End Class
End Namespace
