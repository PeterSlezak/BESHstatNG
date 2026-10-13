Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Framework API forwarding tests; no Excel process or chart required.</summary>
<TestClass>
Public Class PolarPlotFacadeTests
    <TestMethod>
    <TestCategory("Polar-Compatibility")>
    Public Sub ConstructorAndCompute_ForwardToPortableEngine()
        Dim radii As Double() = {2, Double.NaN, 5}
        Dim angles As Double() = {0, 90, 180}
        Dim options As New PolarPlotOptions With {
            .RadialMinimum = 0, .RadialMaximum = 10,
            .ZeroAngle = PolarZeroAngle.North,
            .Rotation = PolarRotation.Clockwise}
        Dim actual As PolarPlotResult = New PolarPlot(radii, angles, options).Compute()
        Dim expected As PolarPlotResult = New PolarPlotCore(radii, angles, options).Compute()
        Assert.AreEqual(expected.ValidPointCount, actual.ValidPointCount)
        Assert.AreEqual(expected.MissingPointCount, actual.MissingPointCount)
        Assert.AreEqual(expected.GroupCount, actual.GroupCount)
        Assert.AreEqual(expected.Spokes.Length, actual.Spokes.Length)
        For i As Integer = 0 To radii.Length - 1
            Assert.AreEqual(expected.Points(i).IsMissing, actual.Points(i).IsMissing)
            If actual.Points(i).IsPlottable Then
                Assert.AreEqual(expected.Points(i).X, actual.Points(i).X, 0.0000000001R)
                Assert.AreEqual(expected.Points(i).Y, actual.Points(i).Y, 0.0000000001R)
            End If
        Next
    End Sub

    <TestMethod>
    <TestCategory("Polar-Compatibility")>
    Public Sub CreateGrouped_PreservesReturnTypeAndGroupGeometry()
        Dim radius As Double() = {1, 2, 3, 4}
        Dim angle As Double() = {0, 90, 180, 270}
        Dim groups As Object() = {"B", "A", "B", "A"}
        Dim actualModel As PolarPlot = PolarPlot.CreateGrouped(radius, angle, groups)
        Dim expectedModel As PolarPlotCore = PolarPlotCore.CreateGrouped(radius, angle, groups)
        Dim actual As PolarPlotResult = actualModel.Compute()
        Dim expected As PolarPlotResult = expectedModel.Compute()
        Assert.AreEqual(2, actual.GroupCount)
        Assert.AreEqual(expected.GroupCount, actual.GroupCount)
        For i As Integer = 0 To 1
            Assert.AreEqual(expected.GroupSeries(i).Name, actual.GroupSeries(i).Name)
            CollectionAssert.AreEqual(expected.GroupSeries(i).SourceIndices,
                                      actual.GroupSeries(i).SourceIndices)
        Next
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() PolarPlot.CreateGrouped(radius, angle, Nothing))
    End Sub
End Class
