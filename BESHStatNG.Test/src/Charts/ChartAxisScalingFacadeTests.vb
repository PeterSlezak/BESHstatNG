Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.graphics

''' <summary>Preserves the Framework chart scaling API and mutable Excel-scale contract.</summary>
<TestClass>
Public Class ChartAxisScalingFacadeTests
    Private Shared Sub CompareFacade(minimum As Double, maximum As Double)
        Dim expected As ChartAxisScaleResult = ChartAxisScalingCore.Compute(minimum, maximum)
        Dim actual As CHARTscale = ChartingFunc.ChartScaling(minimum, maximum)
        Assert.AreEqual(expected.Min, actual.Min)
        Assert.AreEqual(expected.Max, actual.Max)
        Assert.AreEqual(expected.Scale, actual.Scale)
    End Sub

    <TestMethod>
    <TestCategory("ChartAxisScaling-Compatibility")>
    Public Sub WindowsChartScaling_ForwardsMixedAndReversedIntervals()
        CompareFacade(-5.0R, 13.0R)
        CompareFacade(13.0R, -5.0R)
        CompareFacade(-13.0R, -5.0R)
    End Sub

    <TestMethod>
    <TestCategory("ChartAxisScaling-Compatibility")>
    Public Sub WindowsChartScaling_ExposesMutableLegacyResult()
        Dim actual As CHARTscale = ChartingFunc.ChartScaling(0.0R, 0.0R)
        Assert.AreEqual(0.0R, actual.Min)
        Assert.AreEqual(1.2R, actual.Max, 0.000000001R)
        actual.Min = -100.0R
        Assert.AreEqual(-100.0R, actual.Min)
        CompareFacade(10.0R, 10.0R)
    End Sub
End Class
