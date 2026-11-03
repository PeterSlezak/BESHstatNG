Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.survival

Namespace ExcelCommon
    Public NotInheritable Class SurvivalReport
        Friend Sub New(id As String, tables As IEnumerable(Of ResultTable), warnings As IEnumerable(Of AnalysisWarning), Optional fault As AnalysisFault = Nothing)
            Document = New AnalysisResultDocument(id, If(tables, Enumerable.Empty(Of ResultTable)()).Select(Function(t) t.ToOutputModel()),
                Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing, warnings,
                If(fault Is Nothing, Array.Empty(Of AnalysisFault)(), New AnalysisFault() {fault}))
        End Sub
        Public ReadOnly Property Document As AnalysisResultDocument
        ''' <summary>Reuses the accepted table bridge including nulls, p-value cells and footnotes.</summary>
        Public Function LegacyTables() As List(Of ResultTable)
            Return Document.Tables.Select(Function(t) LinearBinomialReport.Rehydrate(t.Model)).ToList()
        End Function
        Public ReadOnly Property BlankRowsBetweenTables As Integer
            Get
                Return 1
            End Get
        End Property
    End Class

    ''' <summary>Snapshot of the original Core chart input, including any malformed legacy axes.
    ''' The Windows renderer still owns clipping, series indices, style and COM. No estimator here.</summary>
    Public NotInheritable Class KaplanMeierCurvePayload
        Private ReadOnly _data As KaplanMeierPlotData
        Private ReadOnly _options As SurvivalOptions
        Friend Sub New(data As KaplanMeierPlotData, options As SurvivalOptions)
            _data = Clone(data) : _options = options.Copy()
            Dim warnings As New List(Of AnalysisWarning)
            If data.CensoredCountByGroup.Length <> data.GroupLabels.Length Then warnings.Add(New AnalysisWarning("survival.plot.censor-axis", "Core censor-count axis omits groups without censoring; legacy native renderer compatibility is retained."))
            Diagnostics = warnings.AsReadOnly()
        End Sub
        Public ReadOnly Property Diagnostics As IReadOnlyList(Of AnalysisWarning)
        Public ReadOnly Property Options As SurvivalOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public ReadOnly Property SeriesOrder As IReadOnlyList(Of String)
            Get
                Dim names As New List(Of String)(_data.GroupLabels)
                names.AddRange(_data.GroupLabels.Select(Function(g) "Censored " & g))
                If _options.PlotConfidenceIntervals Then
                    Dim prefix = $"{100.0 * (1.0 - _data.Alpha):0.##}% CI "
                    names.AddRange(_data.GroupLabels.Select(Function(g) prefix & g))
                    names.AddRange(_data.GroupLabels.Select(Function(g) prefix & g))
                End If
                Return names.AsReadOnly()
            End Get
        End Property
        Public Function ToCoreData() As KaplanMeierPlotData
            Return Clone(_data)
        End Function
        Private Shared Function Clone(p As KaplanMeierPlotData) As KaplanMeierPlotData
            Return New KaplanMeierPlotData With {.Alpha = p.Alpha, .GroupLabels = DirectCast(p.GroupLabels.Clone(), String()),
                .MaximumTimeByGroup = DirectCast(p.MaximumTimeByGroup.Clone(), Double()), .CensoredCountByGroup = DirectCast(p.CensoredCountByGroup.Clone(), Integer()),
                .SurvivalTime = DirectCast(p.SurvivalTime.Clone(), Double()), .SurvivalProbability = DirectCast(p.SurvivalProbability.Clone(), Double(,)),
                .LowerConfidenceLimit = DirectCast(p.LowerConfidenceLimit.Clone(), Double(,)), .UpperConfidenceLimit = DirectCast(p.UpperConfidenceLimit.Clone(), Double(,)),
                .CensorMarkerTime = DirectCast(p.CensorMarkerTime.Clone(), Double(,)), .CensorMarkerProbability = DirectCast(p.CensorMarkerProbability.Clone(), Double(,))}
        End Function
    End Class

    Public NotInheritable Class SurvivalReportPolicy
        Private Sub New()
        End Sub
        Public Shared Function Compose(intent As SurvivalWorkflowIntent, tables As IEnumerable(Of ResultTable)) As SurvivalReport
            Return New SurvivalReport(If(intent.Mode = SurvivalAnalysisMode.LogRank, "survival.logrank", "survival.kaplan-meier"), tables, Warnings(intent.Validation))
        End Function
        Friend Shared Function Warnings(validation As RegressionValidation) As IEnumerable(Of AnalysisWarning)
            ' Strict portable issues are surfaced as metadata; never injected as new legacy table rows/refusals.
            Return validation.Diagnostics.Select(Function(d) New AnalysisWarning(d.Code, d.Message, d.FieldPath))
        End Function
        Public Shared Function Plot(model As Survival_KM_LR, alpha As Double, requestedPresentation As SurvivalOptions) As KaplanMeierCurvePayload
            Return New KaplanMeierCurvePayload(model.GetKaplanMeierPlotData(alpha), requestedPresentation)
        End Function
        ''' <summary>The existing two-group wrapper emits a heading-only fixed-time table
        ''' even when calculation is unchecked. This metadata distinguishes that from computation.</summary>
        Public Shared Function FixedTimeDisposition(intent As SurvivalWorkflowIntent) As String
            If intent.GroupCount <> 2 Then Return "NOT_APPLICABLE"
            Return If(intent.RunFixedTimeTest, "CORE_COMPUTED", "LEGACY_HEADER_ONLY")
        End Function
    End Class

    Public NotInheritable Class CoxReport
        Private ReadOnly _coefficients As Double()
        Friend Sub New(report As SurvivalReport, result As CoxResult)
            Me.Report = report
            If result IsNot Nothing Then
                Converged = result.Converged : Iterations = result.Iterations
                _coefficients = DirectCast(result.Coefficients.Clone(), Double())
            End If
        End Sub
        Public ReadOnly Property Report As SurvivalReport
        Public ReadOnly Property Converged As Nullable(Of Boolean)
        Public ReadOnly Property Iterations As Nullable(Of Integer)
        Public ReadOnly Property Coefficients As Double()
            Get
                Return If(_coefficients Is Nothing, Nothing, DirectCast(_coefficients.Clone(), Double()))
            End Get
        End Property
        Public ReadOnly Property SheetOrder As IReadOnlyList(Of String)
            Get
                Return Array.AsReadOnly({"Cox Regression", "Adjusted Curves", "Residuals"})
            End Get
        End Property
    End Class

    ''' <summary>One Core baseline and its existing step projection. Literal stratum label and
    ''' order are supplied by ComputeBaseline; never sorted/relabelled by Common.</summary>
    Public NotInheritable Class CoxCurveData
        Private ReadOnly _baseline, _steps As Double(,)
        Friend Sub New(stratum As String, baseline(,) As Double, steps(,) As Double)
            Me.Stratum = stratum : _baseline = DirectCast(baseline.Clone(), Double(,)) : _steps = DirectCast(steps.Clone(), Double(,))
        End Sub
        Public ReadOnly Property Stratum As String
        Public ReadOnly Property ColumnNames As String()
            Get
                Return {$"Time_{Stratum}", $"Baseline Survival_{Stratum}", $"Baseline Cumulative Hazard_{Stratum}"}
            End Get
        End Property
        Public ReadOnly Property Baseline As Double(,)
            Get
                Return DirectCast(_baseline.Clone(), Double(,))
            End Get
        End Property
        Public ReadOnly Property StepValues As Double(,)
            Get
                Return DirectCast(_steps.Clone(), Double(,))
            End Get
        End Property
        Public ReadOnly Property Times As Double()
            Get
                Return Enumerable.Range(0, _steps.GetLength(0)).Select(Function(i) _steps(i, 0)).ToArray()
            End Get
        End Property
        Public ReadOnly Property Survival As Double()
            Get
                Return Enumerable.Range(0, _steps.GetLength(0)).Select(Function(i) _steps(i, 1)).ToArray()
            End Get
        End Property
        Public ReadOnly Property AdjustedSeriesIntent As String
            Get
                Return "S0(t) raised to exp(sum of coefficient times user covariate); first stratum is the Windows default. Dynamic edits and stratum changes require the native host."
            End Get
        End Property
    End Class

    Public NotInheritable Class CoxResidualData
        Private ReadOnly _blocks As List(Of Object(,))
        Private ReadOnly _rows As Integer()
        Private ReadOnly _time As Double()
        Private ReadOnly _status As Integer()
        Private ReadOnly _x As Double(,)
        Private ReadOnly _names As String()
        Friend Sub New(intent As CoxWorkflowIntent, blocks As IEnumerable(Of Object(,)))
            _rows = intent.Input.Time.RowIds : _time = intent.Input.Time.Values
            _status = intent.Input.Status.Values.Select(Function(v) CInt(v)).ToArray()
            _x = intent.Expanded.Data : _names = intent.Expanded.Names
            _blocks = blocks.Select(Function(b) DirectCast(b.Clone(), Object(,))).ToList()
        End Sub
        Public ReadOnly Property RowIds As Integer()
            Get
                Return DirectCast(_rows.Clone(), Integer())
            End Get
        End Property
        Public ReadOnly Property Time As Double()
            Get
                Return DirectCast(_time.Clone(), Double())
            End Get
        End Property
        Public ReadOnly Property Status As Integer()
            Get
                Return DirectCast(_status.Clone(), Integer())
            End Get
        End Property
        Public ReadOnly Property Predictors As Double(,)
            Get
                Return DirectCast(_x.Clone(), Double(,))
            End Get
        End Property
        Public ReadOnly Property Names As String()
            Get
                Return DirectCast(_names.Clone(), String())
            End Get
        End Property
        ''' <summary>Exact original two-header-row Core blocks; no new residual plot is implied.</summary>
        Public Function LegacyBlocks() As List(Of Object(,))
            Return _blocks.Select(Function(b) DirectCast(b.Clone(), Object(,))).ToList()
        End Function
    End Class

    Public NotInheritable Class CoxReportPolicy
        Private Sub New()
        End Sub
        Public Shared Function Compose(intent As CoxWorkflowIntent, tables As IEnumerable(Of ResultTable), result As CoxResult) As CoxReport
            Return New CoxReport(New SurvivalReport("survival.cox", tables, SurvivalReportPolicy.Warnings(intent.Validation)), result)
        End Function
        Public Shared Function Fault(intent As CoxWorkflowIntent, code As String, message As String) As CoxReport
            Return New CoxReport(New SurvivalReport("survival.cox", Nothing, SurvivalReportPolicy.Warnings(intent.Validation), New AnalysisFault(code, message)), Nothing)
        End Function
        Public Shared Function Curve(model As CoxPH, stratum As String, baseline(,) As Double) As CoxCurveData
            Return New CoxCurveData(stratum, baseline, model.BaseSurvivalForPloting(baseline))
        End Function
        Public Shared Function Residuals(intent As CoxWorkflowIntent, blocks As IEnumerable(Of Object(,))) As CoxResidualData
            Return New CoxResidualData(intent, blocks)
        End Function
    End Class
End Namespace
