Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Checks the legacy Windows-facing forwarding API after Core migration.</summary>
<TestClass>
Public Class BulletChartFacadeTests

    <TestMethod>
    <TestCategory("Bullet-Compatibility")>
    Public Sub ComputeBasic_UsesSharedCorePayloadType()
        Dim labels As String() = {"Revenue"}
        Dim actual As Double() = {120.0R}
        Dim target As Double() = {150.0R}
        Dim boundaries As Double(,) = {{100.0R, 140.0R, 200.0R}}
        Dim throughHost As BulletChartResult = BulletChart.ComputeBasic(labels, actual, target, boundaries)
        Dim directCore As BulletChartResult = BulletChartCore.ComputeBasic(labels, actual, target, boundaries)
        Assert.AreEqual(1, throughHost.ItemCount)
        Assert.AreEqual(directCore.Items(0).ScaleMaximum, throughHost.Items(0).ScaleMaximum)
        Assert.AreEqual(directCore.Items(0).ActualNormalized, throughHost.Items(0).ActualNormalized)
        Assert.AreEqual(directCore.Items(0).Bands(2).UpperBound, throughHost.Items(0).Bands(2).UpperBound)
        Assert.AreEqual(BulletChartCore.MinimumQualitativeRanges, BulletChart.MinimumQualitativeRanges)
        Assert.AreEqual(BulletChartCore.MaximumQualitativeRanges, BulletChart.MaximumQualitativeRanges)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Compatibility")>
    Public Sub Compute_ForwardsNamedOptionalParameters()
        Dim result As BulletChartResult = BulletChart.Compute(
            New String() {"Metric"}, New Double() {15.0R}, New Double() {18.0R},
            New Double(,) {{10.0R, 20.0R}},
            subtitles:=New String() {" units "},
            scaleMaximums:=New Double() {25.0R},
            majorIntervals:=New Double() {5.0R},
            directions:=New Object() {False})
        Assert.AreEqual("units", result.Items(0).Subtitle)
        Assert.AreEqual(BulletChartDirection.LowerIsBetter, result.Items(0).Direction)
        Assert.AreEqual(25.0R, result.Items(0).ScaleMaximum)
        Assert.AreEqual(5.0R, result.Items(0).MajorInterval)
    End Sub
End Class
