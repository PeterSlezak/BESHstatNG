Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression

<TestClass>
Public Class BinaryClassificationCoreTests

    Private Shared Sub AssertClose(expected As Double, actual As Double, tol As Double, Optional message As String = "")
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail($"{message} expected {expected:R} but got {actual:R}.")
        End If
        If Math.Abs(expected - actual) > tol Then
            Assert.Fail($"{message} expected {expected:R} but got {actual:R}.")
        End If
    End Sub

    <TestMethod>
    Public Sub Summary_and_Brier_match_reference_values()
        Dim y() As Double = {1.0R, 1.0R, 0.0R, 0.0R}
        Dim p() As Double = {0.9R, 0.4R, 0.8R, 0.1R}

        Dim s As BinaryClassificationSummary = BinaryClassificationCore.ComputeBinarySummary(y, p, 0.5R)
        AssertClose(1.0R, s.TP, 0.000000000001R, "TP")
        AssertClose(1.0R, s.FN, 0.000000000001R, "FN")
        AssertClose(1.0R, s.FP, 0.000000000001R, "FP")
        AssertClose(1.0R, s.TN, 0.000000000001R, "TN")
        AssertClose(0.5R, s.Sensitivity, 0.000000000001R, "Sensitivity")
        AssertClose(0.5R, s.Specificity, 0.000000000001R, "Specificity")
        AssertClose(0.5R, s.Accuracy, 0.000000000001R, "Accuracy")
        AssertClose(0.255R, BinaryClassificationCore.ComputeBrierScore(y, p), 0.000000000001R, "Brier")
    End Sub

    <TestMethod>
    Public Sub Weighted_threshold_calibration_and_result_tables_are_consistent()
        Dim y() As Double = {1.0R, 1.0R, 0.0R, 0.0R}
        Dim p() As Double = {0.9R, 0.4R, 0.8R, 0.1R}
        Dim w() As Double = {1.0R, 2.0R, 1.0R, 1.0R}

        Dim s = BinaryClassificationCore.ComputeBinarySummary(y, p, 0.5R, w)
        AssertClose(5.0R, s.N, 0.000000000001R, "N")
        AssertClose(1.0R / 3.0R, s.Sensitivity, 0.000000000001R, "Sensitivity")

        Dim thresholds() As Double = {0.25R, 0.5R, 0.75R}
        Dim thresholdRows = BinaryClassificationCore.BuildThresholdTable(y, p, thresholds, w)
        Assert.AreEqual(3, thresholdRows.Count)

        Dim calibrationRows = BinaryClassificationCore.BuildCalibrationBins(y, p, 2, w, "quantile")
        Assert.AreEqual(2, calibrationRows.Count)
        AssertClose(5.0R, calibrationRows.Sum(Function(r) r.N), 0.000000000001R, "Calibration N")

        Dim brier = BinaryClassificationCore.ComputeBrierScore(y, p, w)
        AssertClose(0.276R, brier, 0.000000000001R, "Weighted Brier")

        Dim wrapped As List(Of ResultTable) = BinaryClassificationCore.WrapResults(s, thresholdRows, calibrationRows, brier, 0.6R, "Core Test")
        Assert.AreEqual(5, wrapped.Count)
        Assert.IsTrue(wrapped.All(Function(t) t IsNot Nothing AndAlso t.TotalRows > 0 AndAlso t.TotalCols > 0))
    End Sub

    <TestMethod>
    Public Sub Default_thresholds_are_sorted_bounded_and_thinned_deterministically()
        Dim p() As Double = {0.8R, 0.2R, 0.2R, 0.6R, 0.4R}
        Dim thresholds() As Double = BinaryClassificationCore.GetDefaultThresholds(p, maxRows:=4, includeEndpoints:=True)

        Assert.IsTrue(thresholds.Length <= 4)
        Assert.AreEqual(0.0R, thresholds.First(), 0.0R)
        Assert.AreEqual(1.0R, thresholds.Last(), 0.0R)
        For i = 1 To thresholds.Length - 1
            Assert.IsTrue(thresholds(i) > thresholds(i - 1))
        Next
    End Sub

    <TestMethod>
    Public Sub Percentile_cutpoint_bins_collapse_tied_cutpoints_and_cover_all_rows()
        Dim p() As Double = {0.1R, 0.1R, 0.1R, 0.5R, 0.5R, 0.9R, 0.9R, 0.9R}
        Dim bins = BinaryClassificationCore.BuildPercentileCutpointBins(p, 10)

        Assert.IsTrue(bins.Count < 10)
        Assert.AreEqual(p.Length, bins.Sum(Function(b) b.Indices.Length))
        Assert.IsTrue(bins.All(Function(b) b.Indices.Length > 0))
    End Sub

    <TestMethod>
    Public Sub Validation_rejects_nonbinary_outcomes_invalid_probabilities_and_negative_weights()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() BinaryClassificationCore.ValidateBinaryInputs(New Double() {0.0R, 2.0R}, New Double() {0.2R, 0.8R}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() BinaryClassificationCore.ValidateBinaryInputs(New Double() {0.0R, 1.0R}, New Double() {0.2R, 1.2R}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() BinaryClassificationCore.ValidateBinaryInputs(New Double() {0.0R, 1.0R}, New Double() {0.2R, 0.8R}, New Double() {1.0R, -1.0R}))
    End Sub

End Class
