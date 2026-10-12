Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Framework compatibility checks for the existing LadderPlot entry points.</summary>
<TestClass>
Public Class LadderPlotFacadeTests
    <TestMethod>
    <TestCategory("Ladder-Compatibility")>
    Public Sub Compute_ForwardsDTOAndJitter()
        Dim first As Double() = {2, 2, 5}
        Dim second As Double() = {4, 6, 7}
        Dim options As New LadderPlotOptions With {.HorizontalJitterFraction = 0.1R}
        Dim actual As LadderPlotResult = LadderPlot.Compute(first, second, options)
        Dim expected As LadderPlotResult = LadderPlotCore.Compute(first, second, options)
        Assert.AreEqual(expected.ObservationCount, actual.ObservationCount)
        Assert.AreEqual(expected.FirstCollisionGroupCount, actual.FirstCollisionGroupCount)
        Assert.AreEqual(expected.Observations(0).FirstXPosition, actual.Observations(0).FirstXPosition)
        Assert.AreEqual(expected.MaximumValue, actual.MaximumValue)
        Assert.AreEqual(LadderPlotCore.FirstBaseXPosition, LadderPlot.FirstBaseXPosition)
    End Sub

    <TestMethod>
    <TestCategory("Ladder-Compatibility")>
    Public Sub ComputeWithGroups_AndLabelsForwardUnchanged()
        Dim first As Double() = {1, 2}
        Dim second As Double() = {4, 5}
        Dim labels As String() = {"row A", "row B"}
        Dim groups As String() = {"X", "Y"}
        Dim actual As LadderPlotResult = LadderPlot.ComputeWithGroups(first, second, labels, groups)
        Dim expected As LadderPlotResult = LadderPlotCore.ComputeWithGroups(first, second, labels, groups)
        Assert.AreEqual(expected.GroupCount, actual.GroupCount)
        Assert.AreEqual(expected.GroupLevels(1).GroupKey, actual.GroupLevels(1).GroupKey)
        Assert.AreEqual(expected.Observations(0).RecordLabel, actual.Observations(0).RecordLabel)
        Assert.AreEqual("row B", LadderPlot.ComputeWithLabels(first, second, labels).Observations(1).RecordLabel)
    End Sub
End Class
