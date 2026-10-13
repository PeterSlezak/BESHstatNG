Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Checks the existing Windows chart/UDF helper signatures still forward to Core.</summary>
<TestClass>
Public Class HistogramBinningFacadeTests
    <TestMethod>
    <TestCategory("Histogram-Compatibility")>
    Public Sub HistogramBinsComputation_ForwardsSameBinTable()
        Dim sample As Double() = {0, 1, 2, 3, 4, 5, 6, 7}
        Dim actual As Object(,) = graphics.ChartingFunc.HistogramBinsComputation(DirectCast(sample.Clone(), Double()), "(Sturges)")
        Dim expected As Object(,) = HistogramBinningCore.HistogramBinsComputation(DirectCast(sample.Clone(), Double()), "(Sturges)")
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0))
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1))
        For i As Integer = 0 To actual.GetLength(0) - 1
            Assert.AreEqual(expected(i, 0), actual(i, 0))
            Assert.AreEqual(expected(i, 1), actual(i, 1))
        Next
    End Sub

    <TestMethod>
    <TestCategory("Histogram-Compatibility")>
    Public Sub GaussOverlayComputation_ForwardsSameCoordinates()
        Dim sample As Double() = {1, 2, 3, 4, 5, 6, 7, 8}
        Dim mids As Double() = {1, 3, 5, 7}
        Dim actual As Double(,) = graphics.ChartingFunc.GaussOverlayComputation(DirectCast(sample.Clone(), Double()), mids)
        Dim expected As Double(,) = HistogramBinningCore.GaussOverlayComputation(DirectCast(sample.Clone(), Double()), mids)
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0))
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1))
        For i As Integer = 0 To expected.GetLength(0) - 1
            Assert.AreEqual(expected(i, 0), actual(i, 0), 0.000000000001R)
            Assert.AreEqual(expected(i, 1), actual(i, 1), 0.000000000001R)
        Next
    End Sub
End Class
