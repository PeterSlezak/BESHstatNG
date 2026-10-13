Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Checks the existing Windows entry point forwards correctly to Core.</summary>
<TestClass>
Public Class CategoricalHistogramFacadeTests
    <TestMethod>
    <TestCategory("CategoricalHistogram-Compatibility")>
    Public Sub Compute_ForwardsGroupedCountsAndPlotOptions()
        Dim values As Double() = {0, 1, 2, 3, 4, 5, 6, 7}
        Dim groups As Object() = {"A", "B", "A", "B", "A", "B", "A", "B"}
        Dim options As New CategoricalHistogramOptions With {
            .PlotType = CategoricalHistogramPlotType.StackedBar,
            .BinningRule = CategoricalHistogramBinningRule.Sturges}
        Dim actual = CategoricalHistogram.Compute(values, groups, options)
        Dim expected = CategoricalHistogramCore.Compute(values, groups, options)
        Assert.AreEqual(expected.GroupCount, actual.GroupCount)
        Assert.AreEqual(expected.BinCount, actual.BinCount)
        Assert.AreEqual(expected.BinWidth, actual.BinWidth)
        CollectionAssert.AreEqual(expected.BinMidpoints, actual.BinMidpoints)
        For i As Integer = 0 To expected.GroupCount - 1
            Assert.AreEqual(expected.Series(i).Name, actual.Series(i).Name)
            CollectionAssert.AreEqual(expected.Series(i).PlotValues, actual.Series(i).PlotValues)
        Next
    End Sub

    <TestMethod>
    <TestCategory("CategoricalHistogram-Compatibility")>
    Public Sub Compute_ForwardsMissingAndComparativeOptions()
        Dim values As Double() = {0, 1, Double.NaN, 3, 4}
        Dim groups As Object() = {"A", "B", "A", Nothing, "B"}
        Dim options As New CategoricalHistogramOptions With {
            .PlotType = CategoricalHistogramPlotType.Comparative,
            .ComparativeOrientation = CategoricalHistogramComparativeOrientation.Horizontal}
        Dim actual = CategoricalHistogram.Compute(values, groups, options)
        Dim expected = CategoricalHistogramCore.Compute(values, groups, options)
        Assert.AreEqual(expected.ExcludedObservationCount, actual.ExcludedObservationCount)
        Assert.AreEqual(expected.UsableObservationCount, actual.UsableObservationCount)
        Assert.AreEqual(expected.Options.ComparativeOrientation, actual.Options.ComparativeOrientation)
        CollectionAssert.AreEqual(expected.Series(1).PerGroupPercent, actual.Series(1).PerGroupPercent)
    End Sub
End Class
