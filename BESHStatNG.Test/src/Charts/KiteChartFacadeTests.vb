Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Windows API compatibility checks without Excel automation.</summary>
<TestClass>
Public Class KiteChartFacadeTests
    <TestMethod>
    <TestCategory("Kite-Compatibility")>
    Public Sub Compute_ForwardsGeometryAndResults()
        Dim source As Double(,) = {{2, 4}, {4, 2}}
        Dim actual As KiteChartResult = KiteChart.Compute(source)
        Dim direct As KiteChartResult = KiteChartCore.Compute(source)
        Assert.AreEqual(direct.PlottedCategoryCount, actual.PlottedCategoryCount)
        Assert.AreEqual(direct.Series.Length, actual.Series.Length)
        For index As Integer = 0 To actual.Series.Length - 1
            CollectionAssert.AreEqual(direct.Series(index).HalfWidths, actual.Series(index).HalfWidths)
            CollectionAssert.AreEqual(direct.Series(index).UpperBoundary, actual.Series(index).UpperBoundary)
        Next
    End Sub

    <TestMethod>
    <TestCategory("Kite-Compatibility")>
    Public Sub Compute_PreservesOptionalParameterNamesAndTypes()
        Dim optionSet As New KiteChartOptions With {
            .ScaleMode = KiteScaleMode.PerSeriesMaximum,
            .ValueTransform = KiteValueTransform.SquareRoot,
            .AddZeroEndpoints = False}
        Dim actual As KiteChartResult = KiteChart.Compute(
            New Double(,) {{1, 4}, {9, 16}},
            seriesNames:=New String() {"First", "Second"},
            categoryLabels:=New String() {"before", "after"},
            options:=optionSet)
        Assert.AreEqual(2, actual.PlottedCategoryCount)
        Assert.AreEqual("First", actual.Series(0).Name)
        Assert.AreEqual("before", actual.Categories(0))
        Assert.AreEqual(KiteValueTransform.SquareRoot, actual.Options.ValueTransform)
        Assert.AreEqual(2, actual.Series.Length)
    End Sub
End Class
