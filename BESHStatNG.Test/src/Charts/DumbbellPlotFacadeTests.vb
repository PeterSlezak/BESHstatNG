Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Windows compatibility tests for the Core-forwarding API.</summary>
<TestClass>
Public Class DumbbellPlotFacadeTests
    <TestMethod>
    <TestCategory("Dumbbell-Compatibility")>
    Public Sub Compute_ForwardsSortAndUsesCorePayload()
        Dim options As New DumbbellPlotOptions With {
            .SortMode = DumbbellPlotSortMode.FirstAscending,
            .ReverseCategoryOrder = True}
        Dim cats As String() = {"B", "A"}
        Dim first As Double() = {2, 1}
        Dim second As Double() = {6, 4}
        Dim actual As DumbbellPlotResult = DumbbellPlot.Compute(cats, first, second, options)
        Dim expected As DumbbellPlotResult = DumbbellPlotCore.Compute(cats, first, second, options)
        Assert.AreEqual(expected.CategoryCount, actual.CategoryCount)
        Assert.AreEqual(expected.Categories(0).CategoryKey, actual.Categories(0).CategoryKey)
        Assert.AreEqual(expected.Categories(0).YPosition, actual.Categories(0).YPosition)
        Assert.AreEqual(expected.Categories(1).Difference, actual.Categories(1).Difference)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Compatibility")>
    Public Sub ComputeWithAuxiliary_ForwardsPositionalAndJitterOptions()
        Dim options As New DumbbellPlotOptions With {
            .AuxiliaryJitterFraction = 0.2R, .AuxiliaryJitterSeed = 123}
        Dim actual As DumbbellPlotResult = DumbbellPlot.ComputeWithAuxiliary(
            New String() {"A", "B"}, New Double() {1, 2}, New Double() {3, 4},
            New String() {"B", "B", "A"}, New Double() {2.5R, 3.5R, 1.5R}, options)
        Dim expected As DumbbellPlotResult = DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A", "B"}, New Double() {1, 2}, New Double() {3, 4},
            New String() {"B", "B", "A"}, New Double() {2.5R, 3.5R, 1.5R}, options)
        Assert.AreEqual(3, actual.UsableAuxiliaryObservationCount)
        Assert.AreEqual(expected.AuxiliaryObservations(0).CategoryDisplayIndex,
                        actual.AuxiliaryObservations(0).CategoryDisplayIndex)
        Assert.AreEqual(expected.AuxiliaryObservations(1).JitterOffset,
                        actual.AuxiliaryObservations(1).JitterOffset)
        Assert.AreEqual(expected.MaximumValue, actual.MaximumValue)
    End Sub
End Class
