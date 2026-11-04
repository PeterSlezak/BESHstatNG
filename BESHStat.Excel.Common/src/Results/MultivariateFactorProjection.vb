Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.Multivariate

Namespace ExcelCommon
    Public NotInheritable Class MultivariateFactorObservation
        Friend Sub New(activeOrdinal As Integer, coreRowId As Integer, inputOrdinal As Integer?, sourceRowId As Integer?, state As MultivariateMappingState)
            Me.ActiveOrdinal = activeOrdinal : Me.CoreRowId = coreRowId : Me.InputOrdinal = inputOrdinal
            Me.SourceRowId = sourceRowId : Me.State = state
        End Sub
        Public ReadOnly Property ActiveOrdinal As Integer
        Public ReadOnly Property CoreRowId As Integer
        Public ReadOnly Property InputOrdinal As Integer?
        Public ReadOnly Property SourceRowId As Integer?
        Public ReadOnly Property State As MultivariateMappingState
    End Class
    ''' <summary>Post-Calculate interpretation. No eigensolver, rotation, scoring,
    ''' missing-value filtering or numerical payload copying is implemented here.</summary>
    Public NotInheritable Class MultivariateFactorProjection
        Friend Sub New(request As MultivariateFactorRequest, model As FactorAnalysis)
            Me.Request = request
            ActualFactorCount = model.NumberOfFactors : RemovedRowCount = model.RemovedRowCount
            ExtractionConverged = model.ExtractionConverged : RotationConverged = model.RotationConverged
            ExtractionIterations = model.ExtractionIterations : RotationIterations = model.RotationIterations
            Factors = model.FactorNames().Select(Function(v, i) New MultivariateAxisItem(i, v)).ToList().AsReadOnly()
            Eigenvalues = Enumerable.Range(0, If(model.InitialEigenvalues Is Nothing, 0, model.InitialEigenvalues.Length)).Select(Function(i) New MultivariateAxisItem(i, Nothing)).ToList().AsReadOnly()
            Dim columns = request.Input.Columns
            Variables = model.VariableNames.Select(Function(v, i) New MultivariateAxisItem(i, v, sourcePosition:=i, sourceKey:=If(i < columns.Count, columns(i).SourceKey, Nothing))).ToList().AsReadOnly()
            Dim rows = request.Input.SourceRowIds, ids = model.AnalysisRowIds
            Dim physical = rows IsNot Nothing AndAlso rows.Length = request.Input.RowCount
            Dim positions As New Dictionary(Of Integer, List(Of Integer))
            For i = 0 To request.Input.RowCount - 1
                Dim key = If(physical, rows(i), i + 1)
                If Not positions.ContainsKey(key) Then positions.Add(key, New List(Of Integer))
                positions(key).Add(i + 1)
            Next
            Dim observations As New List(Of MultivariateFactorObservation), known As New HashSet(Of Integer)
            For i = 0 To ids.Length - 1
                Dim ordinal As Integer? = Nothing, state = MultivariateMappingState.Unavailable
                If positions.ContainsKey(ids(i)) Then
                    Dim matches = positions(ids(i))
                    If matches.Count = 1 Then
                        ordinal = matches(0) : state = MultivariateMappingState.Known
                    ElseIf ids.Length = request.Input.RowCount AndAlso i < request.Input.RowCount AndAlso If(physical, rows(i), i + 1) = ids(i) Then
                        ordinal = i + 1 : state = MultivariateMappingState.Known
                    Else
                        state = MultivariateMappingState.Ambiguous
                    End If
                End If
                If ordinal.HasValue Then known.Add(ordinal.Value)
                observations.Add(New MultivariateFactorObservation(i + 1, ids(i), ordinal,
                    If(physical AndAlso positions.ContainsKey(ids(i)), CType(ids(i), Integer?), Nothing), state))
            Next
            ActiveObservations = observations.AsReadOnly()
            ' Unknown/ambiguous identity must never be guessed into the removed set.
            RemovedInputOrdinals = If(observations.All(Function(o) o.State = MultivariateMappingState.Known),
                Enumerable.Range(1, request.Input.RowCount).Where(Function(i) Not known.Contains(i)).ToList(), New List(Of Integer)).AsReadOnly()
            Dim arrays As New List(Of MultivariateModelArray)
            For Each item In New KeyValuePair(Of String, Array)() {
                New KeyValuePair(Of String, Array)("UnrotatedLoadings", model.UnrotatedLoadings),
                New KeyValuePair(Of String, Array)("PatternMatrix", model.PatternMatrix),
                New KeyValuePair(Of String, Array)("StructureMatrix", model.StructureMatrix),
                New KeyValuePair(Of String, Array)("ScoreCoefficientMatrix", model.ScoreCoefficientMatrix)}
                arrays.Add(New MultivariateModelArray(item.Key, item.Value, MultivariateModelAxis.InputVariables, MultivariateModelAxis.Factors))
            Next
            arrays.Add(New MultivariateModelArray("Scores", model.Scores, MultivariateModelAxis.ActiveObservations, MultivariateModelAxis.Factors))
            arrays.Add(New MultivariateModelArray("AnalysisData", model.AnalysisData, MultivariateModelAxis.ActiveObservations, MultivariateModelAxis.InputVariables))
            arrays.Add(New MultivariateModelArray("CenteredData", model.CenteredData, MultivariateModelAxis.ActiveObservations, MultivariateModelAxis.InputVariables))
            arrays.Add(New MultivariateModelArray("StandardizedData", model.StandardizedData, MultivariateModelAxis.ActiveObservations, MultivariateModelAxis.InputVariables))
            arrays.Add(New MultivariateModelArray("Phi", model.FactorCorrelationMatrix, MultivariateModelAxis.Factors, MultivariateModelAxis.Factors))
            arrays.Add(New MultivariateModelArray("RotationTransform", model.RotationTransform, MultivariateModelAxis.Factors, MultivariateModelAxis.Factors))
            For Each item In New KeyValuePair(Of String, Array)() {
                New KeyValuePair(Of String, Array)("WorkingMatrix", model.WorkingMatrix),
                New KeyValuePair(Of String, Array)("CorrelationMatrix", model.CorrelationMatrix),
                New KeyValuePair(Of String, Array)("AntiImageCorrelation", model.AntiImageCorrelationMatrix),
                New KeyValuePair(Of String, Array)("CommonVariance", model.CommonVarianceMatrix),
                New KeyValuePair(Of String, Array)("Reproduced", model.ReproducedMatrix),
                New KeyValuePair(Of String, Array)("Residual", model.ResidualMatrix)}
                arrays.Add(New MultivariateModelArray(item.Key, item.Value, MultivariateModelAxis.InputVariables, MultivariateModelAxis.InputVariables))
            Next
            Me.Arrays = arrays.AsReadOnly()
            Plots = MultivariateReportPolicy.PlotPlan(MultivariateAnalysisKind.Factor).Select(Function(p) New MultivariateModelPlot(p,
                If(p.Kind = MultivariatePlotKind.Scree, New MultivariateArrayShape(Eigenvalues.Count, 1), MultivariateArrayShape.Matrix(model.PatternMatrix)))).ToList().AsReadOnly()
            Dim diagnostics As New List(Of RegressionDiagnostic)
            If observations.Any(Function(o) o.State <> MultivariateMappingState.Known) Then diagnostics.Add(New RegressionDiagnostic("C20-FA-ROWID-AMBIGUOUS-01", "rows", "Core score RowIds cannot uniquely identify all source input positions; no deleted-row identity is guessed.", blocking:=False))
            Validation = New RegressionValidation(diagnostics, 0, 0, {})
        End Sub
        Public ReadOnly Property Request As MultivariateFactorRequest
        Public ReadOnly Property ActualFactorCount As Integer
        Public ReadOnly Property Factors As IReadOnlyList(Of MultivariateAxisItem)
        Public ReadOnly Property Eigenvalues As IReadOnlyList(Of MultivariateAxisItem)
        Public ReadOnly Property Variables As IReadOnlyList(Of MultivariateAxisItem)
        Public ReadOnly Property ActiveObservations As IReadOnlyList(Of MultivariateFactorObservation)
        Public ReadOnly Property RemovedInputOrdinals As IReadOnlyList(Of Integer)
        Public ReadOnly Property RemovedRowCount As Integer
        Public ReadOnly Property ExtractionConverged As Boolean
        Public ReadOnly Property RotationConverged As Boolean
        Public ReadOnly Property ExtractionIterations As Integer
        Public ReadOnly Property RotationIterations As Integer
        Public ReadOnly Property Arrays As IReadOnlyList(Of MultivariateModelArray)
        Public ReadOnly Property Plots As IReadOnlyList(Of MultivariateModelPlot)
        Public ReadOnly Property Validation As RegressionValidation
        Public Function ProjectReport(tables As IEnumerable(Of ResultTable)) As MultivariateModelReport
            Return New MultivariateModelReport(Request.Input, tables)
        End Function
    End Class
    Public NotInheritable Class MultivariateFactorProjectionPolicy
        Private Sub New()
        End Sub
        Public Shared Function Project(request As MultivariateFactorRequest, model As FactorAnalysis) As MultivariateFactorProjection
            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            Return New MultivariateFactorProjection(request, model)
        End Function
    End Class
End Namespace
