Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Anchored regression checks for the portable legacy axis-scaling algorithm.</summary>
<TestClass>
Public Class ChartAxisScalingCoreTests
    Private Shared Sub ExpectScale(minimum As Double, maximum As Double,
                                   expectedMinimum As Double, expectedMaximum As Double,
                                   expectedMajorUnit As Double)
        Dim actual As ChartAxisScaleResult = ChartAxisScalingCore.Compute(minimum, maximum)
        CloseTo(expectedMinimum, actual.Min)
        CloseTo(expectedMaximum, actual.Max)
        CloseTo(expectedMajorUnit, actual.Scale)
    End Sub

    Private Shared Sub CloseTo(expected As Double, actual As Double)
        Dim tolerance As Double = Math.Max(1.0E-12R, Math.Abs(expected) * 1.0E-12R)
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

    <TestMethod>
    Public Sub ZeroToZero_PreservesLegacySpecialCase()
        ExpectScale(0.0R, 0.0R, 0.0R, 1.2R, 0.2R)
    End Sub

    <TestMethod>
    Public Sub PositiveIntervals_HaveEstablishedMajorUnits()
        ExpectScale(1.0R, 9.0R, 0.0R, 10.0R, 2.0R)
        ExpectScale(0.0R, 100.0R, -20.0R, 120.0R, 20.0R)
        ExpectScale(1.0R, 2.0R, 1.0R, 2.2R, 0.2R)
    End Sub

    <TestMethod>
    Public Sub NegativeIntervals_UseFloorRatherThanTruncation()
        ExpectScale(-9.0R, -1.0R, -10.0R, 0.0R, 2.0R)
        ExpectScale(-1.0R, 0.0R, -1.2R, 0.0R, 0.2R)
    End Sub

    <TestMethod>
    Public Sub MixedSignIntervals_EncloseZero()
        ExpectScale(-3.0R, 7.0R, -4.0R, 8.0R, 2.0R)
        ExpectScale(-100.0R, 100.0R, -120.0R, 120.0R, 20.0R)
    End Sub

    <TestMethod>
    Public Sub ReversedIntervals_ProduceSameScaling()
        Dim ordered As ChartAxisScaleResult = ChartAxisScalingCore.Compute(-3.0R, 7.0R)
        Dim reversed As ChartAxisScaleResult = ChartAxisScalingCore.Compute(7.0R, -3.0R)
        Assert.AreEqual(ordered.Min, reversed.Min)
        Assert.AreEqual(ordered.Max, reversed.Max)
        Assert.AreEqual(ordered.Scale, reversed.Scale)
    End Sub

    <TestMethod>
    Public Sub EqualPositiveLimits_ExpandNarrowly()
        ExpectScale(1.0R, 1.0R, 0.99R, 1.012R, 0.002R)
        ExpectScale(10.0R, 10.0R, 9.9R, 10.12R, 0.02R)
    End Sub

    <TestMethod>
    Public Sub EqualNegativeLimits_ExpandNarrowly()
        ExpectScale(-10.0R, -10.0R, -10.12R, -9.9R, 0.02R)
    End Sub

    <TestMethod>
    Public Sub SmallDecimals_AreNotRoundedToWholeNumbers()
        ExpectScale(0.001R, 0.002R, 0.001R, 0.0022R, 0.0002R)
    End Sub

    <TestMethod>
    Public Sub LargeValues_RetainScalePrecision()
        ExpectScale(1000000.0R, 1200000.0R, 1000000.0R, 1220000.0R, 20000.0R)
    End Sub

    <TestMethod>
    Public Sub StableObjectResultAndFreshInstance()
        Dim first As ChartAxisScaleResult = ChartAxisScalingCore.Compute(1.0R, 9.0R)
        Dim second As ChartAxisScaleResult = ChartAxisScalingCore.Compute(1.0R, 9.0R)
        Assert.AreNotSame(first, second)
        Assert.AreEqual(0.0R, first.Min)
        Assert.AreEqual(10.0R, first.Max)
        Assert.AreEqual(2.0R, first.Scale)
    End Sub

    <TestMethod>
    Public Sub NegativeZero_EqualsPositiveZero()
        Dim first As ChartAxisScaleResult = ChartAxisScalingCore.Compute(-0.0R, -0.0R)
        Dim second As ChartAxisScaleResult = ChartAxisScalingCore.Compute(0.0R, 0.0R)
        Assert.AreEqual(second.Min, first.Min)
        Assert.AreEqual(second.Max, first.Max)
        Assert.AreEqual(second.Scale, first.Scale)
    End Sub
End Class
