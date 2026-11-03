Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG.regression

Namespace ExcelCommon
    Public NotInheritable Class RegressionClassificationReport
        Private ReadOnly _calibration As List(Of CalibrationBinSummary)
        Private ReadOnly _tables As List(Of ResultTable)
        Friend Sub New(tables As List(Of ResultTable), calibration As List(Of CalibrationBinSummary))
            _tables = tables
            _calibration = If(calibration Is Nothing, Nothing, New List(Of CalibrationBinSummary)(calibration))
            Dim models As New List(Of ResultTableOutputModel)
            For Each t In tables : models.Add(t.ToOutputModel()) : Next
            Document = New AnalysisResultDocument("regression.classification", models, Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing)
        End Sub
        Public ReadOnly Property Document As AnalysisResultDocument
        ''' <summary>Fresh list for the existing Windows writer. Tables are Core-owned legacy output;
        ''' the immutable Document is the portable snapshot. Caller must not mutate table contents.</summary>
        Public Function LegacyTables() As List(Of ResultTable)
            Return New List(Of ResultTable)(_tables)
        End Function
        Public ReadOnly Property CalibrationRows As List(Of CalibrationBinSummary)
            Get
                Return If(_calibration Is Nothing, Nothing, New List(Of CalibrationBinSummary)(_calibration))
            End Get
        End Property
    End Class

    ''' <summary>Optional-section orchestration; computations remain BinaryClassificationCore.
    ''' Windows supplies the legacy weighted event rate; ROC and chart work never enter Common.</summary>
    Public NotInheritable Class RegressionClassificationPolicy
        Private Sub New()
        End Sub
        Public Shared Function Compose(y() As Double, p() As Double, weights() As Double, options As RegressionOptions,
                                       Optional eventRate As Double = Double.NaN,
                                       Optional label As String = "GLM Binary Classification") As RegressionClassificationReport
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
            If Not options.Classification Then Return New RegressionClassificationReport(New List(Of ResultTable), Nothing)
            If Not options.Family.HasValue OrElse options.Family.Value <> RegressionFamily.Binomial Then Throw New ArgumentException("regression.options.classification: Classification requires a binomial GLM.")
            If Not RegressionRequestPolicy.Finite(options.Threshold) OrElse options.Threshold < 0 OrElse options.Threshold > 1 Then Throw New ArgumentException("regression.options.threshold: Classification threshold must be between 0 and 1.")
            If options.Calibration AndAlso options.CalibrationBins < 2 Then Throw New ArgumentException("regression.options.calibrationBins: Calibration bin count must be at least 2.")
            BinaryClassificationCore.ValidateBinaryInputs(y, p, weights)
            Dim summary = BinaryClassificationCore.ComputeBinarySummary(y, p, options.Threshold, weights)
            Dim thresholds As List(Of BinaryThresholdRow) = Nothing, calibration As List(Of CalibrationBinSummary) = Nothing
            If options.ThresholdTable Then thresholds = BinaryClassificationCore.BuildThresholdTable(y, p, Nothing, weights)
            If options.Calibration Then calibration = BinaryClassificationCore.BuildCalibrationBins(y, p, options.CalibrationBins, weights, "quantile")
            Dim brier = Double.NaN
            If options.Brier Then brier = BinaryClassificationCore.ComputeBrierScore(y, p, weights)
            Return New RegressionClassificationReport(BinaryClassificationCore.WrapResults(summary, thresholds, calibration, brier, If(options.Brier, eventRate, Double.NaN), label), calibration)
        End Function
    End Class
End Namespace
