Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Immutable Core response snapshot. Core prediction and input/residual axes
    ''' remain explicit. No model, callbacks, Excel cells or native chart is retained.</summary>
    Public NotInheritable Class GeeReport
        Private ReadOnly _observed, _weights, _corePredictions As Double()
        Private ReadOnly _correlation As Double(,)
        Friend Sub New(intent As GeeWorkflowIntent, report As RegressionReportPlan, corePredictions() As Double,
                       observed() As Double, weights() As Double, correlation(,) As Double)
            Me.Intent = intent : MeanReport = report
            _observed = Copy(observed) : _weights = Copy(weights) : _corePredictions = Copy(corePredictions)
            _correlation = If(correlation Is Nothing, Nothing, DirectCast(correlation.Clone(), Double(,)))
            Dim warnings = report.Document.Warnings.Concat(intent.Validation.Diagnostics.Where(Function(d) Not d.Blocking).
                Select(Function(d) New AnalysisWarning(d.Code, d.Message, d.FieldPath)))
            Dim faults = If(report.Document.Tables.Count = 0 AndAlso corePredictions Is Nothing,
                New AnalysisFault() {New AnalysisFault("gee.fit.unavailable", "Core supplied no usable fit/report payload.")}, Array.Empty(Of AnalysisFault)())
            Document = New AnalysisResultDocument("regression.gee", report.Document.Tables.Select(Function(t) t.Model),
                Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing, warnings, faults)
        End Sub
        Private Shared Function Copy(values() As Double) As Double()
            Return If(values Is Nothing, Nothing, DirectCast(values.Clone(), Double()))
        End Function
        Public ReadOnly Property Intent As GeeWorkflowIntent
        Public ReadOnly Property MeanReport As RegressionReportPlan
        Public ReadOnly Property Document As AnalysisResultDocument
        Public ReadOnly Property Rows As RegressionRowOutput
            Get
                Return MeanReport.Rows
            End Get
        End Property
        Public ReadOnly Property Convergence As RegressionConvergence
            Get
                Return MeanReport.Convergence
            End Get
        End Property
        Public ReadOnly Property CorePredictionRowIds As Integer()
            Get
                Return Intent.Order.CorePredictionRowIds
            End Get
        End Property
        Public ReadOnly Property CorePredictions As Double()
            Get
                Return Copy(_corePredictions)
            End Get
        End Property
        Public ReadOnly Property Observed As Double()
            Get
                Return Copy(_observed)
            End Get
        End Property
        Public ReadOnly Property ObservationWeights As Double()
            Get
                Return Copy(_weights)
            End Get
        End Property
        Public ReadOnly Property WorkingCorrelation As Double(,)
            Get
                Return If(_correlation Is Nothing, Nothing, DirectCast(_correlation.Clone(), Double(,)))
            End Get
        End Property
        Public Function LegacyTables() As List(Of ResultTable)
            Return Document.Tables.Select(Function(t) LinearBinomialReport.Rehydrate(t.Model)).ToList()
        End Function
    End Class

    Public NotInheritable Class GeeClassificationPlan
        Private ReadOnly _options As RegressionOptions
        Private ReadOnly _rowIds As Integer()
        Private ReadOnly _calibration As List(Of regression.CalibrationBinSummary)
        Friend Sub New(rows() As Integer, options As RegressionOptions, classification As RegressionClassificationReport)
            _rowIds = DirectCast(rows.Clone(), Integer()) : _options = options.Copy()
            _calibration = classification.CalibrationRows : Document = classification.Document
        End Sub
        Public ReadOnly Property RowIds As Integer()
            Get
                Return DirectCast(_rowIds.Clone(), Integer())
            End Get
        End Property
        Public ReadOnly Property Options As RegressionOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public ReadOnly Property Document As AnalysisResultDocument
        Public ReadOnly Property CalibrationRows As List(Of regression.CalibrationBinSummary)
            Get
                Return If(_calibration Is Nothing, Nothing, New List(Of regression.CalibrationBinSummary)(_calibration))
            End Get
        End Property
        Public ReadOnly Property ChartHooks As IReadOnlyList(Of String)
            Get
                If Document.Tables.Count = 0 Then Return Array.Empty(Of String)()
                Return If(_calibration Is Nothing, Array.AsReadOnly({"ROC"}), Array.AsReadOnly({"ROC", "Calibration"}))
            End Get
        End Property
        Public Function LegacyTables() As List(Of ResultTable)
            Return Document.Tables.Select(Function(t) LinearBinomialReport.Rehydrate(t.Model)).ToList()
        End Function
    End Class

    Public NotInheritable Class GeeReportPolicy
        Private Sub New()
        End Sub
        ''' <summary>Core predictions are cluster-concatenated, unlike observed/residual arrays.
        ''' This explicit map is identity for the actual sorted Windows import. No inverse link.</summary>
        Public Shared Function Predictions(intent As GeeWorkflowIntent, corePredictions() As Double) As Double()
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            If corePredictions Is Nothing Then Return Nothing
            Dim rows = intent.MeanRequest.Primary.RowIds, permutation = intent.Order.ClusteredToInputIndices
            If rows Is Nothing OrElse corePredictions.Length <> rows.Length OrElse permutation.Length <> rows.Length OrElse permutation.Distinct().Count() <> rows.Length Then Throw New ArgumentException("gee.output.prediction.alignment: Actual Core predictions and cluster/source axes must agree.")
            Dim aligned(rows.Length - 1) As Double
            For i = 0 To rows.Length - 1 : aligned(permutation(i)) = corePredictions(i) : Next
            Return aligned
        End Function
        Public Shared Function Residuals(intent As GeeWorkflowIntent, residual(,) As Object) As Object(,)
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            If residual Is Nothing Then Return Nothing
            If residual.GetLowerBound(0) <> 0 OrElse residual.GetLowerBound(1) <> 0 OrElse residual.GetLength(0) <> intent.MeanRequest.Primary.RowIds.Length + 1 OrElse residual.GetLength(1) <> 6 Then Throw New ArgumentException("gee.output.residuals.axes: Core GEE residuals require one explicit header row and six columns in input order.")
            For c = 0 To 5
                If Not TypeOf residual(0, c) Is String Then Throw New ArgumentException("gee.output.residuals.headers: Core residual headers are not observation rows.")
            Next
            Return DirectCast(residual.Clone(), Object(,))
        End Function
        Public Shared Function Compose(intent As GeeWorkflowIntent, tables As IEnumerable(Of ResultTable), corePredictions() As Double,
                                       residuals(,) As Object, observed() As Double, weights() As Double,
                                       Optional workingCorrelation(,) As Double = Nothing) As GeeReport
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            Dim ids = intent.MeanRequest.Primary.RowIds, predicted = Predictions(intent, corePredictions)
            If observed IsNot Nothing AndAlso observed.Length <> ids.Length Then Throw New ArgumentException("gee.output.observed.alignment")
            If weights IsNot Nothing AndAlso weights.Length <> ids.Length Then Throw New ArgumentException("gee.output.weights.alignment")
            Dim matrix As Double(,) = Nothing
            If predicted IsNot Nothing Then
                ReDim matrix(predicted.Length - 1, 0)
                For i = 0 To predicted.Length - 1 : matrix(i, 0) = predicted(i) : Next
            End If
            Dim rows As New RegressionRowOutput(ids, matrix, GeeReportPolicy.Residuals(intent, residuals))
            Dim report = RegressionReportPolicy.Compose(intent.MeanRequest, tables, rows)
            Return New GeeReport(intent, report, corePredictions, observed, weights, workingCorrelation)
        End Function
        ''' <summary>Call only at the original post-fit UI read boundary. Request-time controls
        ''' are not reused; Windows supplies its weighted-event-rate helper and owns charts.</summary>
        Public Shared Function Classification(report As GeeReport, postFitOptions As RegressionOptions,
                                              Optional eventRate As Double = Double.NaN) As GeeClassificationPlan
            If report Is Nothing OrElse postFitOptions Is Nothing Then Throw New ArgumentNullException()
            Dim o = postFitOptions.Copy()
            ' Classification belongs to the actual fitted family. A post-fit UI change
            ' is not evidence that a different model was fitted. Windows quarantines
            ' its legacy mismatch path separately pending exact native timing authority.
            o.Family = report.Intent.MeanRequest.Options.Family
            If o.Family <> RegressionFamily.Binomial OrElse report.Rows.Fitted Is Nothing OrElse report.Observed Is Nothing Then o.Classification = False
            Dim p As Double() = Nothing
            If report.Rows.Fitted IsNot Nothing Then
                Dim values = report.Rows.Fitted
                p = Enumerable.Range(0, values.GetLength(0)).Select(Function(i) values(i, 0)).ToArray()
            End If
            Return New GeeClassificationPlan(report.Rows.RowIds, o,
                RegressionClassificationPolicy.Compose(report.Observed, p, report.ObservationWeights, o, eventRate, "GEE Binary Classification"))
        End Function
    End Class
End Namespace
