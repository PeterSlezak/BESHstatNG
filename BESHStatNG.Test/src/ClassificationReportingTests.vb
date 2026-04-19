Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression
Imports Udfs = BESHStatNG.BESHStatNG.WorksheetFunctions

Friend Module ClassificationReportingTestData

    Private ReadOnly Invariant As CultureInfo = CultureInfo.InvariantCulture

    Public Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory

        Dim candidates As New List(Of String) From {
            Path.Combine(baseDir, fileName),
            Path.Combine(baseDir, "TestData", fileName),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "TestData", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "TestData", fileName))
        }

        For Each p In candidates
            If File.Exists(p) Then Return p
        Next

        Throw New FileNotFoundException($"Could not locate test data file '{fileName}'. Searched: {String.Join(" ; ", candidates)}")
    End Function

    Private Function ParseDoubleInvariant(text As String) As Double
        Return Double.Parse(text.Trim(), NumberStyles.Float Or NumberStyles.AllowThousands, Invariant)
    End Function

    Public Function ToColumnWithHeader(header As String, values() As Double) As Object(,)
        Dim out(values.Length, 0) As Object
        out(0, 0) = header
        For i As Integer = 0 To values.Length - 1
            out(i + 1, 0) = values(i)
        Next
        Return out
    End Function

    Public Function ToRowWithHeader(header As String, values() As Double) As Object(,)
        Dim out(0, values.Length) As Object
        out(0, 0) = header
        For i As Integer = 0 To values.Length - 1
            out(0, i + 1) = values(i)
        Next
        Return out
    End Function

    Public Function ToColumn(values() As Double) As Object(,)
        Dim out(values.Length - 1, 0) As Object
        For i As Integer = 0 To values.Length - 1
            out(i, 0) = values(i)
        Next
        Return out
    End Function

    Public Function ToRow(values() As Double) As Object(,)
        Dim out(0, values.Length - 1) As Object
        For i As Integer = 0 To values.Length - 1
            out(0, i) = values(i)
        Next
        Return out
    End Function

    Public Sub LoadGlmUdfArgs(fileName As String,
                              ByRef y As Object(,),
                              ByRef x As Object(,))
        Dim lines() As String = File.ReadAllLines(GetTestDataPath(fileName))
        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must contain header + rows.")

        Dim header() As String = lines(0).Split(","c).Select(Function(z) z.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next

        Dim n As Integer = lines.Length - 1
        y = New Object(n - 1, 0) {}
        x = New Object(n - 1, 1) {}

        For r As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(r).Split(","c)
            y(r - 1, 0) = ParseDoubleInvariant(parts(idx("y")))
            x(r - 1, 0) = ParseDoubleInvariant(parts(idx("x1")))
            x(r - 1, 1) = ParseDoubleInvariant(parts(idx("x2")))
        Next
    End Sub

    Public Sub LoadGeeUdfArgs(fileName As String,
                              ByRef y As Object(,),
                              ByRef x As Object(,),
                              ByRef clusterId As Object(,),
                              ByRef time As Object(,))
        Dim lines() As String = File.ReadAllLines(GetTestDataPath(fileName))
        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must contain header + rows.")

        Dim header() As String = lines(0).Split(","c).Select(Function(z) z.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next

        Dim n As Integer = lines.Length - 1
        y = New Object(n - 1, 0) {}
        x = New Object(n - 1, 1) {}
        clusterId = New Object(n - 1, 0) {}
        time = New Object(n - 1, 0) {}

        For r As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(r).Split(","c)
            y(r - 1, 0) = ParseDoubleInvariant(parts(idx("y")))
            x(r - 1, 0) = ParseDoubleInvariant(parts(idx("x1")))
            x(r - 1, 1) = ParseDoubleInvariant(parts(idx("x2")))
            clusterId(r - 1, 0) = parts(idx("cluster")).Trim()
            time(r - 1, 0) = ParseDoubleInvariant(parts(idx("time")))
        Next
    End Sub

End Module

<TestClass>
Public Class BinaryClassificationReportingTests

    Private Shared Sub AssertClose(expected As Double, actual As Double, tol As Double, Optional message As String = "")
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail($"{message} expected {expected:R} but got {actual:R}.")
        End If
        If Math.Abs(expected - actual) > tol Then
            Assert.Fail($"{message} expected {expected:R} but got {actual:R}.")
        End If
    End Sub

    <TestMethod>
    <TestCategory("Classification")>
    Public Sub ComputeBinarySummary_and_Brier_return_expected_unweighted_values()
        Dim y() As Double = {1.0R, 1.0R, 0.0R, 0.0R}
        Dim p() As Double = {0.9R, 0.4R, 0.8R, 0.1R}

        Dim s As BinaryClassificationSummary = BinaryClassificationReporting.ComputeBinarySummary(y, p, 0.5R)

        AssertClose(1.0R, s.TP, 0.000000000001R, "TP")
        AssertClose(1.0R, s.FN, 0.000000000001R, "FN")
        AssertClose(1.0R, s.FP, 0.000000000001R, "FP")
        AssertClose(1.0R, s.TN, 0.000000000001R, "TN")
        AssertClose(0.5R, s.Sensitivity, 0.000000000001R, "Sensitivity")
        AssertClose(0.5R, s.Specificity, 0.000000000001R, "Specificity")
        AssertClose(0.5R, s.Precision, 0.000000000001R, "Precision")
        AssertClose(0.5R, s.Recall, 0.000000000001R, "Recall")
        AssertClose(0.5R, s.NPV, 0.000000000001R, "NPV")
        AssertClose(0.5R, s.Accuracy, 0.000000000001R, "Accuracy")
        AssertClose(0.5R, s.BalancedAccuracy, 0.000000000001R, "BalancedAccuracy")
        AssertClose(0.0R, s.YoudenJ, 0.000000000001R, "YoudenJ")
        AssertClose(0.5R, s.F1, 0.000000000001R, "F1")
        AssertClose(0.5R, s.Prevalence, 0.000000000001R, "Prevalence")
        AssertClose(4.0R, s.N, 0.000000000001R, "N")

        Dim brier As Double = BinaryClassificationReporting.ComputeBrierScore(y, p)
        AssertClose(0.255R, brier, 0.000000000001R, "Brier")
    End Sub

    <TestMethod>
    <TestCategory("Classification")>
    Public Sub Weighted_summary_threshold_table_calibration_and_wrapresults_are_consistent()
        Dim y() As Double = {1.0R, 1.0R, 0.0R, 0.0R}
        Dim p() As Double = {0.9R, 0.4R, 0.8R, 0.1R}
        Dim w() As Double = {1.0R, 2.0R, 1.0R, 1.0R}

        Dim s As BinaryClassificationSummary = BinaryClassificationReporting.ComputeBinarySummary(y, p, 0.5R, w)
        AssertClose(1.0R, s.TP, 0.000000000001R, "weighted TP")
        AssertClose(2.0R, s.FN, 0.000000000001R, "weighted FN")
        AssertClose(1.0R, s.FP, 0.000000000001R, "weighted FP")
        AssertClose(1.0R, s.TN, 0.000000000001R, "weighted TN")
        AssertClose(5.0R, s.N, 0.000000000001R, "weighted N")
        AssertClose(1.0R / 3.0R, s.Sensitivity, 0.000000000001R, "weighted sensitivity")
        AssertClose(0.5R, s.Specificity, 0.000000000001R, "weighted specificity")
        AssertClose(0.4R, s.Accuracy, 0.000000000001R, "weighted accuracy")

        Dim thresholds() As Double = {0.25R, 0.5R, 0.75R}
        Dim thrRows As List(Of BinaryThresholdRow) = BinaryClassificationReporting.BuildThresholdTable(y, p, thresholds, w)
        Assert.AreEqual(3, thrRows.Count)
        AssertClose(0.25R, thrRows(0).Threshold, 0.000000000001R)
        AssertClose(0.5R, thrRows(1).Threshold, 0.000000000001R)
        AssertClose(0.75R, thrRows(2).Threshold, 0.000000000001R)

        Dim calibRows As List(Of CalibrationBinSummary) = BinaryClassificationReporting.BuildCalibrationBins(y, p, 2, w, "quantile")
        Assert.AreEqual(2, calibRows.Count)
        AssertClose(5.0R, calibRows.Sum(Function(r) r.N), 0.000000000001R, "calibration weighted N")
        Assert.IsTrue(calibRows.All(Function(r) r.MeanPredicted >= 0.0R AndAlso r.MeanPredicted <= 1.0R))
        Assert.IsTrue(calibRows.All(Function(r) r.ObservedRate >= 0.0R AndAlso r.ObservedRate <= 1.0R))

        Dim brier As Double = BinaryClassificationReporting.ComputeBrierScore(y, p, w)
        AssertClose(0.276R, brier, 0.000000000001R, "weighted Brier")

        Dim wrapped As List(Of ResultTable) = BinaryClassificationReporting.WrapResults(s, thrRows, calibRows, brier, eventRate:=0.6R, analysisLabel:="Unit Test")
        Assert.AreEqual(5, wrapped.Count, "Expected summary, calibration, brier, and threshold tables.")
        Assert.IsTrue(wrapped.All(Function(t) t IsNot Nothing AndAlso t.TotalRows > 0 AndAlso t.TotalCols > 0))
    End Sub

End Class

<TestClass>
Public Class ClassificationReportGenericUdfTests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub CLASS_generic_udfs_and_CALIB_POINTS_return_expected_tables()
        Dim y() As Double = {1.0R, 1.0R, 0.0R, 0.0R}
        Dim p() As Double = {0.9R, 0.4R, 0.8R, 0.1R}
        Dim w() As Double = {1.0R, 2.0R, 1.0R, 1.0R}

        Dim yCol As Object(,) = ClassificationReportingTestData.ToColumnWithHeader("y", y)
        Dim pCol As Object(,) = ClassificationReportingTestData.ToColumnWithHeader("p", p)
        Dim wCol As Object(,) = ClassificationReportingTestData.ToColumnWithHeader("w", w)
        Dim thrRow As Object(,) = ClassificationReportingTestData.ToRow(New Double() {0.25R, 0.5R, 0.75R})

        Dim confusion As Object(,) = UdfAssert.AsTable(Udfs.ClassificationReportUDFs.CLASS_CONFUSION(yCol, pCol, 0.5R, wCol, True))
        Assert.AreEqual("Observed \ Predicted", CStr(confusion(0, 0)))
        Assert.AreEqual(0.0R, Convert.ToDouble(confusion(1, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual(1.0R, Convert.ToDouble(confusion(2, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual(50.0R, Convert.ToDouble(confusion(1, 3), CultureInfo.InvariantCulture), 0.0000001R)
        Assert.AreEqual(33.3333333333333R, Convert.ToDouble(confusion(2, 3), CultureInfo.InvariantCulture), 0.0001R)
        Assert.AreEqual(0.5R, Convert.ToDouble(confusion(4, 1), CultureInfo.InvariantCulture), 0.0000001R)

        Dim thresh As Object(,) = UdfAssert.AsTable(Udfs.ClassificationReportUDFs.CLASS_THRESH(yCol, pCol, thrRow, wCol, True))
        Assert.AreEqual("Threshold", CStr(thresh(0, 0)))
        Assert.AreEqual(4, thresh.GetLength(0))
        Assert.AreEqual(14, thresh.GetLength(1))
        Assert.AreEqual(0.25R, Convert.ToDouble(thresh(1, 0), CultureInfo.InvariantCulture), 0.0000001R)
        Assert.AreEqual(0.5R, Convert.ToDouble(thresh(2, 0), CultureInfo.InvariantCulture), 0.0000001R)
        Assert.AreEqual(0.75R, Convert.ToDouble(thresh(3, 0), CultureInfo.InvariantCulture), 0.0000001R)

        Dim calib As Object(,) = UdfAssert.AsTable(Udfs.ClassificationReportUDFs.CLASS_CALIB(yCol, pCol, 2, "quantile", wCol, True))
        Assert.AreEqual("Bin", CStr(calib(0, 0)))
        Assert.AreEqual("MeanPredicted", CStr(calib(0, 2)))
        Assert.AreEqual(3, calib.GetLength(0))
        Assert.AreEqual(6, calib.GetLength(1))

        Dim brierTbl As Object(,) = UdfAssert.AsTable(Udfs.ClassificationReportUDFs.CLASS_BRIER(yCol, pCol, wCol, True))
        Assert.AreEqual("Item", CStr(brierTbl(0, 0)))
        Assert.AreEqual("BrierScore", CStr(brierTbl(1, 0)))
        Assert.AreEqual(0.276R, Convert.ToDouble(brierTbl(1, 1), CultureInfo.InvariantCulture), 0.0000001R)
        Assert.AreEqual(5.0R, Convert.ToDouble(brierTbl(2, 1), CultureInfo.InvariantCulture), 0.0000001R)
        Assert.AreEqual(0.6R, Convert.ToDouble(brierTbl(3, 1), CultureInfo.InvariantCulture), 0.0000001R)

        Dim calibPoints As Object(,) = UdfAssert.AsTable(Udfs.PlotDataUDFs.CALIB_POINTS(yCol, pCol, 2, "quantile", wCol))
        Assert.AreEqual("Bin", CStr(calibPoints(0, 0)))
        Assert.AreEqual("ErrorMinus", CStr(calibPoints(0, 6)))
        Assert.AreEqual("ErrorPlus", CStr(calibPoints(0, 7)))
        Assert.AreEqual(3, calibPoints.GetLength(0))
        Assert.AreEqual(8, calibPoints.GetLength(1))
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub CLASS_generic_udfs_accept_row_vectors_with_headers_and_reject_invalid_threshold()
        Dim yRow As Object(,) = ClassificationReportingTestData.ToRowWithHeader("y", New Double() {1.0R, 0.0R, 1.0R, 0.0R})
        Dim pRow As Object(,) = ClassificationReportingTestData.ToRowWithHeader("p", New Double() {0.9R, 0.2R, 0.6R, 0.4R})

        Dim confusion As Object(,) = UdfAssert.AsTable(Udfs.ClassificationReportUDFs.CLASS_CONFUSION(yRow, pRow, 0.5R, Nothing, True))
        Assert.AreEqual("Observed \ Predicted", CStr(confusion(0, 0)))
        Assert.AreEqual(100.0R, Convert.ToDouble(confusion(1, 3), CultureInfo.InvariantCulture), 0.0000001R)
        Assert.AreEqual(100.0R, Convert.ToDouble(confusion(2, 3), CultureInfo.InvariantCulture), 0.0000001R)

        UdfAssert.IsExcelError(Udfs.ClassificationReportUDFs.CLASS_CONFUSION(yRow, pRow, 1.5R, Nothing, True), "ExcelErrorValue")
    End Sub

End Class

<TestClass>
Public Class ModelReportingHandleUdfTests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub GLM_binomial_handle_exposes_classification_reporting_udfs()
        Dim y As Object(,) = Nothing
        Dim x As Object(,) = Nothing
        ClassificationReportingTestData.LoadGlmUdfArgs("glm_binomial_full.csv", y, x)

        Dim handleObj As Object = Udfs.GLMUDFs.GLM_FIT(y, x, "x1,x2", "binomial", "logit")
        Assert.IsInstanceOfType(handleObj, GetType(String), $"GLM_FIT returned unexpected value: {If(handleObj, "<Nothing>")}")

        Dim handle As String = CStr(handleObj)
        Dim classTbl As Object(,) = UdfAssert.AsTable(Udfs.GLMUDFs.GLM_CLASS(handle, 0.5R, True))
        Assert.AreEqual("Observed \ Predicted", CStr(classTbl(0, 0)))

        Dim threshTbl As Object(,) = UdfAssert.AsTable(Udfs.GLMUDFs.GLM_THRESH(handle, ClassificationReportingTestData.ToRow(New Double() {0.25R, 0.5R, 0.75R}), True))
        Assert.AreEqual("Threshold", CStr(threshTbl(0, 0)))
        Assert.AreEqual(4, threshTbl.GetLength(0))

        Dim calibTbl As Object(,) = UdfAssert.AsTable(Udfs.GLMUDFs.GLM_CALIB(handle, 5, "quantile", True))
        Assert.AreEqual("Bin", CStr(calibTbl(0, 0)))
        Assert.AreEqual("ObservedRate", CStr(calibTbl(0, 3)))

        Dim brierTbl As Object(,) = UdfAssert.AsTable(Udfs.GLMUDFs.GLM_BRIER(handle, True))
        Assert.AreEqual("Item", CStr(brierTbl(0, 0)))
        Assert.AreEqual("BrierScore", CStr(brierTbl(1, 0)))
        Dim brier As Double = Convert.ToDouble(brierTbl(1, 1), CultureInfo.InvariantCulture)
        Assert.IsTrue(brier >= 0.0R AndAlso brier <= 1.0R)

        Assert.AreEqual(True, Udfs.GLMUDFs.GLM_DROP(handle))
        UdfAssert.IsExcelError(Udfs.GLMUDFs.GLM_CLASS(handle, 0.5R, True), "ExcelErrorNA")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub GEE_binomial_handle_exposes_classification_reporting_udfs()
        Dim y As Object(,) = Nothing
        Dim x As Object(,) = Nothing
        Dim clusterId As Object(,) = Nothing
        Dim time As Object(,) = Nothing
        ClassificationReportingTestData.LoadGeeUdfArgs("gee_binomial_logit_full.csv", y, x, clusterId, time)

        Dim handleObj As Object = Udfs.GEEUDFs.GEE_FIT(y, x, clusterId, time, "x1,x2", "binomial", "logit", "independence", "robust")
        Assert.IsInstanceOfType(handleObj, GetType(String), $"GEE_FIT returned unexpected value: {If(handleObj, "<Nothing>")}")

        Dim handle As String = CStr(handleObj)
        Dim classTbl As Object(,) = UdfAssert.AsTable(Udfs.GEEUDFs.GEE_CLASS(handle, 0.5R, True))
        Assert.AreEqual("Observed \ Predicted", CStr(classTbl(0, 0)))

        Dim threshTbl As Object(,) = UdfAssert.AsTable(Udfs.GEEUDFs.GEE_THRESH(handle, ClassificationReportingTestData.ToRow(New Double() {0.25R, 0.5R, 0.75R}), True))
        Assert.AreEqual("Threshold", CStr(threshTbl(0, 0)))
        Assert.AreEqual(4, threshTbl.GetLength(0))

        Dim calibTbl As Object(,) = UdfAssert.AsTable(Udfs.GEEUDFs.GEE_CALIB(handle, 5, "quantile", True))
        Assert.AreEqual("Bin", CStr(calibTbl(0, 0)))
        Assert.AreEqual("UpperCI", CStr(calibTbl(0, 5)))

        Dim brierTbl As Object(,) = UdfAssert.AsTable(Udfs.GEEUDFs.GEE_BRIER(handle, True))
        Assert.AreEqual("Item", CStr(brierTbl(0, 0)))
        Assert.AreEqual("BrierScore", CStr(brierTbl(1, 0)))
        Dim brier As Double = Convert.ToDouble(brierTbl(1, 1), CultureInfo.InvariantCulture)
        Assert.IsTrue(brier >= 0.0R AndAlso brier <= 1.0R)

        Assert.AreEqual(True, Udfs.GEEUDFs.GEE_DROP(handle))
        UdfAssert.IsExcelError(Udfs.GEEUDFs.GEE_CLASS(handle, 0.5R, True), "ExcelErrorNA")
    End Sub

End Class
