Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Direct regression tests for portable histogram bins and Gaussian overlays.</summary>
<TestClass>
Public Class HistogramBinningCoreTests
    Private Shared Function Run(rule As String, ParamArray values() As Double) As Object(,)
        Return HistogramBinningCore.HistogramBinsComputation(values, rule)
    End Function

    Private Shared Function CountTotal(bins As Object(,)) As Integer
        Dim total As Integer = 0
        For row As Integer = 0 To bins.GetLength(0) - 1
            total += Convert.ToInt32(bins(row, 1))
        Next
        Return total
    End Function

    Private Shared Sub CheckShapeAndCounts(bins As Object(,), expectedN As Integer)
        Assert.AreEqual(0, bins.GetLowerBound(0))
        Assert.AreEqual(0, bins.GetLowerBound(1))
        Assert.AreEqual(2, bins.GetLength(1))
        Assert.IsTrue(bins.GetLength(0) > 0)
        Assert.AreEqual(expectedN, CountTotal(bins))
        For row As Integer = 0 To bins.GetLength(0) - 1
            Assert.IsInstanceOfType(bins(row, 0), GetType(Double))
            Assert.IsInstanceOfType(bins(row, 1), GetType(Integer))
            If row > 0 Then
                Assert.IsTrue(Convert.ToDouble(bins(row, 0)) > Convert.ToDouble(bins(row - 1, 0)))
            End If
        Next
    End Sub

    <TestMethod>
    Public Sub Sturges_KnownBreaksMidpointsAndFrequencies()
        Dim bins = Run("(Sturges)", 0, 1, 2, 3, 4, 5, 6, 7)
        Assert.AreEqual(4, bins.GetLength(0))
        For i As Integer = 0 To 3
            Assert.AreEqual(1.0R + 2.0R * i, Convert.ToDouble(bins(i, 0)), 0.000000000001R)
            Assert.AreEqual(2, Convert.ToInt32(bins(i, 1)))
        Next
    End Sub

    <TestMethod>
    Public Sub Sturges_RightMostBoundaryIncluded()
        Dim bins = Run("(Sturges)", 0, 2, 4, 6, 8, 10, 12, 14)
        Assert.AreEqual(3, bins.GetLength(0))
        CollectionAssert.AreEqual(New Integer() {3, 2, 3},
                                  Enumerable.Range(0, bins.GetLength(0)).Select(Function(i) Convert.ToInt32(bins(i, 1))).ToArray())
    End Sub

    <TestMethod>
    Public Sub Sturges_OneValueAndZeroValueRanges()
        CheckShapeAndCounts(Run("(Sturges)", 5, 5, 5, 5, 5, 5, 5, 5), 8)
        Dim zeroBins = Run("(Sturges)", 0, 0, 0, 0)
        CheckShapeAndCounts(zeroBins, 4)
        Assert.AreEqual(0.0R, Convert.ToDouble(zeroBins(0, 0)), 0.000000000001R)
    End Sub

    <TestMethod>
    Public Sub UnknownRule_RetainsOneTargetBinFallback()
        Dim bins = Run("", 0, 1, 2, 3, 4, 5, 6, 7)
        CheckShapeAndCounts(bins, 8)
        Assert.AreEqual(1, bins.GetLength(0))
        Assert.AreEqual(5.0R, Convert.ToDouble(bins(0, 0)), 0.000000000001R)
    End Sub

    <TestMethod>
    Public Sub FourRules_AcceptSkewedFiniteObservations()
        Dim values As Double() = {1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 9, 20, 50}
        For Each rule As String In New String() {"(Sturges)", "(Doane)", "(Scott)", "(Freedman-Diaconis)"}
            CheckShapeAndCounts(HistogramBinningCore.HistogramBinsComputation(DirectCast(values.Clone(), Double()), rule), values.Length)
        Next
    End Sub

    <TestMethod>
    Public Sub FreedmanDiaconis_PreservesQuartileSorterSideEffect()
        Dim observations As Double() = {8, 7, 6, 5, 4, 3, 2, 1}
        CheckShapeAndCounts(HistogramBinningCore.HistogramBinsComputation(observations, "(Freedman-Diaconis)"), 8)
        CollectionAssert.AreEqual(New Double() {1, 2, 3, 4, 5, 6, 7, 8}, observations)
    End Sub

    <TestMethod>
    Public Sub DecimalStep_ProducesRoundedFiniteMidpoints()
        Dim values As Double() = {0, 0.05R, 0.1R, 0.15R, 0.2R, 0.25R, 0.3R, 0.35R}
        Dim bins = HistogramBinningCore.HistogramBinsComputation(values, "(Sturges)")
        CheckShapeAndCounts(bins, 8)
        Assert.AreEqual(0.05R, Convert.ToDouble(bins(0, 0)), 0.00000000001R)
    End Sub

    <TestMethod>
    Public Sub NegativeValues_AreCounted()
        Dim bins = Run("(Sturges)", -7, -6, -5, -4, -3, -2, -1, 0)
        CheckShapeAndCounts(bins, 8)
        Assert.IsTrue(Convert.ToDouble(bins(0, 0)) < 0.0R)
    End Sub

    <TestMethod>
    Public Sub NullInput_RetainsException()
        Assert.ThrowsException(Of NullReferenceException)(Sub() HistogramBinningCore.HistogramBinsComputation(Nothing, "(Sturges)"))
    End Sub

    <TestMethod>
    Public Sub EmptyInput_RetainsException()
        Assert.ThrowsException(Of InvalidOperationException)(Sub() HistogramBinningCore.HistogramBinsComputation(Array.Empty(Of Double)(), "(Sturges)"))
    End Sub

    <TestMethod>
    Public Sub GaussianOverlay_HasLegacyHundredByTwoShapeAndEndpoints()
        Dim data As Double() = {1, 2, 3, 4, 5, 6, 7, 8}
        Dim overlay = HistogramBinningCore.GaussOverlayComputation(data, New Double() {1, 3, 5, 7})
        Assert.AreEqual(100, overlay.GetLength(0))
        Assert.AreEqual(2, overlay.GetLength(1))
        Assert.AreEqual(1.0R, overlay(0, 0), 0.00000000001R)
        Assert.AreEqual(8.0R, overlay(99, 0), 0.00000000001R)
        Assert.IsTrue(overlay(50, 1) > 0)
        For i As Integer = 0 To 99
            Assert.IsFalse(Double.IsNaN(overlay(i, 1)))
            Assert.IsFalse(Double.IsInfinity(overlay(i, 1)))
        Next
    End Sub

    <TestMethod>
    Public Sub GaussianOverlay_FrequencyScalingUsesDistinctSortedBinWidths()
        Dim a = HistogramBinningCore.GaussOverlayComputation(New Double() {1, 2, 3, 4, 5, 6, 7, 8},
                                                            New Double() {7, 3, 3, 1, 5})
        Dim b = HistogramBinningCore.GaussOverlayComputation(New Double() {1, 2, 3, 4, 5, 6, 7, 8},
                                                            New Double() {1, 2, 3, 4})
        For i As Integer = 0 To 99
            Assert.AreEqual(a(i, 0), b(i, 0), 0.000000000001R)
            Assert.AreEqual(2.0R * b(i, 1), a(i, 1), 0.00000000001R)
        Next
    End Sub

    <TestMethod>
    Public Sub GaussianOverlay_OneMidpointUsesUnitWidth()
        Dim a = HistogramBinningCore.GaussOverlayComputation(New Double() {1, 2, 3, 4, 5, 6, 7, 8},
                                                            New Double() {3})
        Dim b = HistogramBinningCore.GaussOverlayComputation(New Double() {1, 2, 3, 4, 5, 6, 7, 8},
                                                            New Double() {1, 2})
        Assert.AreEqual(b(50, 1), a(50, 1), 0.00000000001R)
    End Sub

    <TestMethod>
    Public Sub GaussianOverlay_PreservesInputSortingSideEffect()
        Dim values As Double() = {8, 2, 5, 3, 7, 1, 4, 6}
        Dim result = HistogramBinningCore.GaussOverlayComputation(values, New Double() {1, 3, 5})
        CollectionAssert.AreEqual(New Double() {1, 2, 3, 4, 5, 6, 7, 8}, values)
        Assert.AreEqual(100, result.GetLength(0))
    End Sub

    <TestMethod>
    Public Sub GaussianOverlay_ConstantSampleYieldsNonfiniteDensitiesAsBefore()
        Dim result = HistogramBinningCore.GaussOverlayComputation(New Double() {4, 4, 4, 4}, New Double() {4})
        Assert.AreEqual(100, result.GetLength(0))
        Assert.IsTrue(Double.IsNaN(result(0, 1)))
    End Sub

    <TestMethod>
    Public Sub GaussianOverlay_NullSampleRetainsException()
        Assert.ThrowsException(Of NullReferenceException)(Sub() HistogramBinningCore.GaussOverlayComputation(Nothing, New Double() {1, 2}))
    End Sub
End Class
