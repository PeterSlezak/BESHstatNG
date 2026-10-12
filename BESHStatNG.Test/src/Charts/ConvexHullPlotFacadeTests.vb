Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Framework-side regression checks for the existing ConvexHullPlot public entry points.</summary>
<TestClass>
Public Class ConvexHullPlotFacadeTests
    <TestMethod>
    <TestCategory("ConvexHull-Compatibility")>
    Public Sub Compute_ForwardsOptionsAndGeometry()
        Dim x As Double() = {0, 2, 2, 0}
        Dim y As Double() = {0, 0, 2, 2}
        Dim options As New ConvexHullPlotOptions With {.PaddingPercentX = 5}
        Dim actual As ConvexHullPlotResult = ConvexHullPlot.Compute(x, y, options)
        Dim expected As ConvexHullPlotResult = ConvexHullPlotCore.Compute(x, y, options)
        Assert.AreEqual(expected.Groups.Length, actual.Groups.Length)
        Assert.AreEqual(expected.Groups(0).Area, actual.Groups(0).Area)
        CollectionAssert.AreEqual(expected.Groups(0).ClosedHullX, actual.Groups(0).ClosedHullX)
    End Sub

    <TestMethod>
    <TestCategory("ConvexHull-Compatibility")>
    Public Sub ComputeGrouped_RetainsGroupOrderAndMissingBehavior()
        Dim x As Double() = {0, 1, 2, 3}
        Dim y As Double() = {0, 1, 2, 3}
        Dim categories As Object() = {"B", "A", "B", Nothing}
        Dim actual As ConvexHullPlotResult = ConvexHullPlot.ComputeGrouped(x, y, categories)
        Dim expected As ConvexHullPlotResult = ConvexHullPlotCore.ComputeGrouped(x, y, categories)
        Assert.AreEqual(expected.OmittedObservationCount, actual.OmittedObservationCount)
        Assert.AreEqual(expected.Groups.Length, actual.Groups.Length)
        Assert.AreEqual(expected.Groups(0).Name, actual.Groups(0).Name)
        Assert.AreEqual(expected.Groups(1).Name, actual.Groups(1).Name)
    End Sub
End Class
