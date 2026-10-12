Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Framework integration checks for the existing violin calculation entry point.</summary>
<TestClass>
Public Class ViolinPlotFacadeTests
    <TestMethod>
    <TestCategory("Violin-Compatibility")>
    Public Sub Compute_ForwardsGroupingAndNumericalOptions()
        Dim values As Double() = {1, 2, 4, 3, 8}
        Dim groups As Object() = {"B", "A", "B", "A", "B"}
        Dim options As New ViolinPlotOptions With {
            .BandwidthRule = ViolinBandwidthRule.Manual, .ManualBandwidth = 0.7R,
            .GridPoints = 64, .ScaleMode = ViolinScaleMode.Count}
        Dim actual As ViolinPlotResult = ViolinPlot.Compute(values, groups, options)
        Dim expected As ViolinPlotResult = ViolinPlotCore.Compute(values, groups, options)
        Assert.AreEqual(expected.GroupCount, actual.GroupCount)
        Assert.AreEqual(expected.Series(0).Name, actual.Series(0).Name)
        Assert.AreEqual(expected.Series(1).Name, actual.Series(1).Name)
        Assert.AreEqual(expected.Series(0).Bandwidth, actual.Series(0).Bandwidth)
        CollectionAssert.AreEqual(expected.Series(0).ScaledHalfWidths, actual.Series(0).ScaledHalfWidths)
    End Sub

    <TestMethod>
    <TestCategory("Violin-Compatibility")>
    Public Sub Compute_ForwarderPreservesMissingRowBehavior()
        Dim values As Double() = {1, Double.NaN, 5, 7}
        Dim groups As Object() = {"A", "A", Nothing, "B"}
        Dim actual As ViolinPlotResult = ViolinPlot.Compute(values, groups)
        Dim expected As ViolinPlotResult = ViolinPlotCore.Compute(values, groups)
        Assert.AreEqual(expected.ExcludedObservationCount, actual.ExcludedObservationCount)
        Assert.AreEqual(expected.UsableObservationCount, actual.UsableObservationCount)
        Assert.AreEqual(expected.GroupCount, actual.GroupCount)
    End Sub
End Class
