Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.survival

Namespace ExcelCommon
    ''' <summary>Exact Core tie enum; no alternative estimator or alias parser.</summary>
    Public NotInheritable Class CoxOptions
        Public Property Ties As TieMethod = TieMethod.Breslow
        Public Property Alpha As Double = 0.05
        Public Property MaximumIterations As Integer = 50
        Public Property Epsilon As Double = 0.000001
        Public Property RobustVariance As Boolean
        Public Property ReturnCovariance As Boolean
        Public Property AllResiduals As Boolean
        Public Property PHScoreTest As Boolean
        Public Property Trace As Boolean
        Public Property IterationDetails As Boolean
        Public Function Copy() As CoxOptions
            Return DirectCast(MemberwiseClone(), CoxOptions)
        End Function
    End Class

    Public NotInheritable Class CoxWorkflowIntent
        Private ReadOnly _options As CoxOptions
        Public Sub New(input As SurvivalInput, expanded As RegressionComponent, Optional options As CoxOptions = Nothing)
            Me.Input = input : Me.Expanded = expanded : _options = If(options, New CoxOptions()).Copy()
            Validation = CoxWorkflowPolicy.Validate(input, expanded, _options)
        End Sub
        Public ReadOnly Property Input As SurvivalInput
        Public ReadOnly Property Expanded As RegressionComponent
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property Options As CoxOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public Function CreateCoreModel() As CoxPH
            If Not Validation.IsValid Then Throw New ArgumentException("survival.cox.request.invalid")
            Return CreateAcquiredModel(CoxWorkflowPolicy.BuildExpandedRecords(Input, Expanded.Data))
        End Function
        ''' <summary>Existing Windows validation/conversion remains authoritative for shipping
        ''' refusal timing. No new IsValid gate, exception handling or Fit call is inserted.</summary>
        Public Function CreateAcquiredModel(records As List(Of SurvivalRecord)) As CoxPH
            Dim fit As New CoxPH(SurvivalWorkflowPolicy.CopyRecords(records), Expanded.Names, _options.MaximumIterations, _options.Epsilon)
            fit.startParams = Expanded.InitialValues
            fit.bRobustVariance = _options.RobustVariance : fit.bReturnCov = _options.ReturnCovariance
            fit.bComputeAllResiduals = _options.AllResiduals : fit.bComputePHScoreTest = _options.PHScoreTest
            fit.bIterationDetails = _options.IterationDetails : fit.bTrace = _options.Trace
            Return fit
        End Function
    End Class

    Public NotInheritable Class CoxWorkflowPolicy
        Private Sub New()
        End Sub
        Public Shared ReadOnly Property OmitCategoricalReference As Boolean
            Get
                Return True
            End Get
        End Property
        Public Shared Function InitialValuesCountMatches(values() As Double, expandedParameterCount As Integer) As Boolean
            Return values Is Nothing OrElse values.Length = expandedParameterCount
        End Function
        Public Shared Function Validate(input As SurvivalInput, expanded As RegressionComponent, options As CoxOptions) As RegressionValidation
            options = If(options, New CoxOptions())
            Dim issues = SurvivalWorkflowPolicy.Validate(SurvivalAnalysisMode.Cox, input, New SurvivalOptions With {.Alpha = options.Alpha}).Diagnostics.ToList()
            If Not [Enum].IsDefined(GetType(TieMethod), options.Ties) Then SurvivalWorkflowPolicy.Issue(issues, "cox.ties", "Unknown Cox tie method.")
            If options.MaximumIterations <= 0 Then SurvivalWorkflowPolicy.Issue(issues, "cox.iterations", "Maximum iterations must be positive.")
            If Not SurvivalWorkflowPolicy.Finite(options.Epsilon) OrElse options.Epsilon <= 0 Then SurvivalWorkflowPolicy.Issue(issues, "cox.epsilon", "Epsilon must be positive and finite.")
            If expanded Is Nothing OrElse expanded.Data Is Nothing OrElse expanded.Names Is Nothing OrElse expanded.RowIds Is Nothing Then
                SurvivalWorkflowPolicy.Issue(issues, "cox.predictors", "Expanded predictors, names and source rows are required.")
                Return New RegressionValidation(issues, 0, 0, {})
            End If
            Dim x = expanded.Data, names = expanded.Names, rows = expanded.RowIds, p = x.GetLength(1)
            If p = 0 OrElse names.Length <> p OrElse names.Any(Function(n) String.IsNullOrWhiteSpace(n)) OrElse names.Distinct(StringComparer.Ordinal).Count() <> names.Length Then SurvivalWorkflowPolicy.Issue(issues, "cox.names", "Expanded coefficient names must be present, distinct and aligned with columns.")
            If input Is Nothing OrElse input.Time Is Nothing OrElse input.Time.RowIds Is Nothing OrElse Not rows.SequenceEqual(input.Time.RowIds) OrElse rows.Length <> x.GetLength(0) Then SurvivalWorkflowPolicy.Issue(issues, "cox.rows", "Expanded design and survival roles must retain exactly the same row identities/order.")
            For Each value In x
                If Not SurvivalWorkflowPolicy.Finite(value) Then SurvivalWorkflowPolicy.Issue(issues, "cox.predictors.finite", "Expanded predictors must be finite.") : Exit For
            Next
            Dim initial = expanded.InitialValues
            If Not InitialValuesCountMatches(initial, p) Then SurvivalWorkflowPolicy.Issue(issues, "cox.starts.count", "Supplied starts must match the expanded coefficient count.")
            If initial IsNot Nothing AndAlso initial.Any(Function(v) Not SurvivalWorkflowPolicy.Finite(v)) Then SurvivalWorkflowPolicy.Issue(issues, "cox.starts.finite", "Supplied starts must be finite.")
            Return New RegressionValidation(issues, p, 0, {})
        End Function
        ''' <summary>Same positional expansion bridge as the original form. Core design authoring
        ''' has already expanded the selected TermSpecs; this function does not parse effects.</summary>
        Public Shared Function BuildExpandedRecords(input As SurvivalInput, x(,) As Double) As List(Of SurvivalRecord)
            Dim times = input.Time.Values, status = input.Status.Values, ids = input.Time.RowIds
            Dim strata = If(input.Strata Is Nothing, Nothing, input.Strata.Values), records As New List(Of SurvivalRecord)
            For i = 0 To times.Length - 1
                Dim row = i
                Dim values = If(x Is Nothing, Array.Empty(Of Double)(), Enumerable.Range(0, x.GetLength(1)).Select(Function(j) x(row, j)).ToArray())
                records.Add(New SurvivalRecord With {.Time = times(i), .Censorship = CInt(status(i)), .Index = ids(i),
                    .Stratum = If(strata Is Nothing, "0", strata(i)), .Covariates = values})
            Next
            Return records
        End Function
    End Class
End Namespace
