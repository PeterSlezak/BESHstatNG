Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.AppInfrastructure

''' <summary>
''' Contract tests for NumericGuards after it is moved into BESHStat.Core.
'''
''' NumericGuards members are Friend, so BESHStat.Core must expose its internals to
''' BESHStat.Core.Tests, for example:
'''
'''   &lt;Assembly: Runtime.CompilerServices.InternalsVisibleTo("BESHStat.Core.Tests")&gt;
'''
''' These tests intentionally exercise the existing public behavior contract rather than
''' changing visibility or validation semantics as part of the migration.
''' </summary>
<TestClass>
Public Class NumericGuardsTests

    <TestMethod>
    Public Sub IsFinite_RejectsNaNAndInfinities()

        Assert.IsTrue(NumericGuards.IsFinite(0.0R))
        Assert.IsTrue(NumericGuards.IsFinite(-123.456R))
        Assert.IsTrue(NumericGuards.IsFinite(Double.MaxValue))
        Assert.IsTrue(NumericGuards.IsFinite(Double.MinValue))

        Assert.IsFalse(NumericGuards.IsFinite(Double.NaN))
        Assert.IsFalse(NumericGuards.IsFinite(Double.PositiveInfinity))
        Assert.IsFalse(NumericGuards.IsFinite(Double.NegativeInfinity))

    End Sub


    <TestMethod>
    Public Sub UnitIntervalPredicates_HaveExpectedEndpointSemantics()

        Assert.IsTrue(NumericGuards.IsClosedUnitInterval(0.0R))
        Assert.IsTrue(NumericGuards.IsClosedUnitInterval(0.5R))
        Assert.IsTrue(NumericGuards.IsClosedUnitInterval(1.0R))
        Assert.IsFalse(NumericGuards.IsClosedUnitInterval(-Double.Epsilon))
        Assert.IsFalse(NumericGuards.IsClosedUnitInterval(1.000000000000001R))
        Assert.IsFalse(NumericGuards.IsClosedUnitInterval(Double.NaN))

        Assert.IsFalse(NumericGuards.IsOpenUnitInterval(0.0R))
        Assert.IsTrue(NumericGuards.IsOpenUnitInterval(0.5R))
        Assert.IsFalse(NumericGuards.IsOpenUnitInterval(1.0R))
        Assert.IsFalse(NumericGuards.IsOpenUnitInterval(Double.PositiveInfinity))

        Assert.IsTrue(NumericGuards.IsHalfOpenUnitInterval(0.0R))
        Assert.IsTrue(NumericGuards.IsHalfOpenUnitInterval(0.5R))
        Assert.IsFalse(NumericGuards.IsHalfOpenUnitInterval(1.0R))
        Assert.IsFalse(NumericGuards.IsHalfOpenUnitInterval(Double.NegativeInfinity))

    End Sub


    <TestMethod>
    Public Sub NormalizeAlpha_PreservesValidAlphaAndFallsBackForInvalidValues()

        Assert.AreEqual(0.01R, NumericGuards.NormalizeAlpha(0.01R), 0.0R)
        Assert.AreEqual(0.5R, NumericGuards.NormalizeAlpha(0.5R), 0.0R)
        Assert.AreEqual(0.999R, NumericGuards.NormalizeAlpha(0.999R), 0.0R)

        Assert.AreEqual(0.05R, NumericGuards.NormalizeAlpha(0.0R), 0.0R)
        Assert.AreEqual(0.05R, NumericGuards.NormalizeAlpha(1.0R), 0.0R)
        Assert.AreEqual(0.05R, NumericGuards.NormalizeAlpha(-0.1R), 0.0R)
        Assert.AreEqual(0.05R, NumericGuards.NormalizeAlpha(1.1R), 0.0R)
        Assert.AreEqual(0.05R, NumericGuards.NormalizeAlpha(Double.NaN), 0.0R)
        Assert.AreEqual(0.05R, NumericGuards.NormalizeAlpha(Double.PositiveInfinity), 0.0R)
        Assert.AreEqual(0.05R, NumericGuards.NormalizeAlpha(Double.NegativeInfinity), 0.0R)

    End Sub


    <TestMethod>
    Public Sub ClampProbability_ClampsFiniteAndInfiniteBoundsAndPreservesNaN()

        Assert.AreEqual(0.0R, NumericGuards.ClampProbability(-2.0R), 0.0R)
        Assert.AreEqual(0.0R, NumericGuards.ClampProbability(0.0R), 0.0R)
        Assert.AreEqual(0.25R, NumericGuards.ClampProbability(0.25R), 0.0R)
        Assert.AreEqual(1.0R, NumericGuards.ClampProbability(1.0R), 0.0R)
        Assert.AreEqual(1.0R, NumericGuards.ClampProbability(2.0R), 0.0R)
        Assert.AreEqual(0.0R, NumericGuards.ClampProbability(Double.NegativeInfinity), 0.0R)
        Assert.AreEqual(1.0R, NumericGuards.ClampProbability(Double.PositiveInfinity), 0.0R)
        Assert.IsTrue(Double.IsNaN(NumericGuards.ClampProbability(Double.NaN)))

    End Sub


    <TestMethod>
    Public Sub ValidateFinite_AcceptsFiniteAndRejectsNonFiniteWithParameterName()

        NumericGuards.ValidateFinite(0.0R, "value")
        NumericGuards.ValidateFinite(Double.MaxValue, "value")

        For Each invalidValue As Double In
            New Double() {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}

            Dim ex As ArgumentOutOfRangeException =
                Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                    Sub()
                        NumericGuards.ValidateFinite(invalidValue, "value")
                    End Sub)

            Assert.AreEqual("value", ex.ParamName)
            StringAssert.Contains(ex.Message, "Value must be finite.")
        Next

    End Sub


    <TestMethod>
    Public Sub ValidateOpenUnitInterval_EnforcesStrictEndpoints()

        NumericGuards.ValidateOpenUnitInterval(0.25R, "p")
        NumericGuards.ValidateOpenUnitInterval(0.999R, "p")

        For Each invalidValue As Double In
            New Double() {0.0R, 1.0R, -0.1R, 1.1R, Double.NaN, Double.PositiveInfinity}

            Dim ex As ArgumentOutOfRangeException =
                Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                    Sub()
                        NumericGuards.ValidateOpenUnitInterval(invalidValue, "p")
                    End Sub)

            Assert.AreEqual("p", ex.ParamName)
        Next

    End Sub


    <TestMethod>
    Public Sub ValidateClosedUnitInterval_AcceptsBothEndpoints()

        NumericGuards.ValidateClosedUnitInterval(0.0R, "p")
        NumericGuards.ValidateClosedUnitInterval(0.5R, "p")
        NumericGuards.ValidateClosedUnitInterval(1.0R, "p")

        For Each invalidValue As Double In
            New Double() {-0.1R, 1.1R, Double.NaN, Double.NegativeInfinity}

            Dim ex As ArgumentOutOfRangeException =
                Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                    Sub()
                        NumericGuards.ValidateClosedUnitInterval(invalidValue, "p")
                    End Sub)

            Assert.AreEqual("p", ex.ParamName)
        Next

    End Sub


    <TestMethod>
    Public Sub ValidateUnitIntervalExcludingOne_AcceptsZeroButRejectsOne()

        NumericGuards.ValidateUnitIntervalExcludingOne(0.0R, "p")
        NumericGuards.ValidateUnitIntervalExcludingOne(0.999R, "p")

        For Each invalidValue As Double In
            New Double() {-0.1R, 1.0R, 1.1R, Double.NaN, Double.PositiveInfinity}

            Dim ex As ArgumentOutOfRangeException =
                Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                    Sub()
                        NumericGuards.ValidateUnitIntervalExcludingOne(invalidValue, "p")
                    End Sub)

            Assert.AreEqual("p", ex.ParamName)
        Next

    End Sub


    <TestMethod>
    Public Sub ValidatePositive_RequiresFiniteStrictlyPositiveValue()

        NumericGuards.ValidatePositive(Double.Epsilon, "x")
        NumericGuards.ValidatePositive(10.0R, "x")

        For Each invalidValue As Double In
            New Double() {0.0R, -1.0R, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}

            Dim ex As ArgumentOutOfRangeException =
                Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                    Sub()
                        NumericGuards.ValidatePositive(invalidValue, "x")
                    End Sub)

            Assert.AreEqual("x", ex.ParamName)
            StringAssert.Contains(ex.Message, "Value must be positive.")
        Next

    End Sub


    <TestMethod>
    Public Sub ValidateAlpha_UsesOpenUnitIntervalAndExpectedMessage()

        NumericGuards.ValidateAlpha(0.05R)
        NumericGuards.ValidateAlpha(0.95R, "customAlpha")

        Dim exDefault As ArgumentOutOfRangeException =
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub()
                    NumericGuards.ValidateAlpha(0.0R)
                End Sub)

        Assert.AreEqual("alpha", exDefault.ParamName)
        StringAssert.Contains(exDefault.Message, "Alpha must lie in the open interval (0, 1).")

        Dim exCustom As ArgumentOutOfRangeException =
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub()
                    NumericGuards.ValidateAlpha(1.0R, "customAlpha")
                End Sub)

        Assert.AreEqual("customAlpha", exCustom.ParamName)

    End Sub


    <TestMethod>
    Public Sub ValidateAlphaOneSided_RequiresAlphaBelowOneHalf()

        NumericGuards.ValidateAlphaOneSided(0.025R, "alphaOneSided")
        NumericGuards.ValidateAlphaOneSided(0.499999R, "alphaOneSided")

        Dim endpoint As ArgumentOutOfRangeException =
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub()
                    NumericGuards.ValidateAlphaOneSided(0.5R, "alphaOneSided")
                End Sub)

        Assert.AreEqual("alphaOneSided", endpoint.ParamName)
        StringAssert.Contains(endpoint.Message, "one-sided alpha must be less than 0.5")

        Dim zero As ArgumentOutOfRangeException =
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub()
                    NumericGuards.ValidateAlphaOneSided(0.0R, "alphaOneSided")
                End Sub)

        Assert.AreEqual("alphaOneSided", zero.ParamName)

    End Sub


    <TestMethod>
    Public Sub ValidateProbability_AllowsClosedUnitIntervalOnly()

        NumericGuards.ValidateProbability(0.0R, "probability")
        NumericGuards.ValidateProbability(0.5R, "probability")
        NumericGuards.ValidateProbability(1.0R, "probability")

        Dim ex As ArgumentOutOfRangeException =
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub()
                    NumericGuards.ValidateProbability(1.0001R, "probability")
                End Sub)

        Assert.AreEqual("probability", ex.ParamName)
        StringAssert.Contains(ex.Message, "Probability must lie in the closed interval [0,1].")

    End Sub


    <TestMethod>
    Public Sub ValidatePositiveReplicates_EnforcesInclusiveMinimum()

        NumericGuards.ValidatePositiveReplicates(10, "replicates", 10)
        NumericGuards.ValidatePositiveReplicates(11, "replicates", 10)

        Dim ex As ArgumentOutOfRangeException =
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub()
                    NumericGuards.ValidatePositiveReplicates(9, "replicates", 10)
                End Sub)

        Assert.AreEqual("replicates", ex.ParamName)
        StringAssert.Contains(ex.Message, "Value must be >= 10.")

    End Sub


    <TestMethod>
    Public Sub ValidatePositiveLong_EnforcesInclusiveMinimum()

        NumericGuards.ValidatePositiveLong(100L, "count", 100L)
        NumericGuards.ValidatePositiveLong(101L, "count", 100L)

        Dim ex As ArgumentOutOfRangeException =
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub()
                    NumericGuards.ValidatePositiveLong(99L, "count", 100L)
                End Sub)

        Assert.AreEqual("count", ex.ParamName)
        StringAssert.Contains(ex.Message, "Value must be >= 100.")

    End Sub


    <TestMethod>
    Public Sub ValidateFiniteStatistics_AcceptsFiniteArray()

        NumericGuards.ValidateFiniteStatistics(
            New Double() {-10.0R, 0.0R, 1.5R, Double.MaxValue},
            "statistics")

    End Sub


    <TestMethod>
    Public Sub ValidateFiniteStatistics_RejectsNothingEmptyAndNonFiniteArrays()

        Dim nullEx As ArgumentNullException =
            Assert.ThrowsException(Of ArgumentNullException)(
                Sub()
                    NumericGuards.ValidateFiniteStatistics(Nothing, "statistics")
                End Sub)

        Assert.AreEqual("statistics", nullEx.ParamName)

        Dim emptyEx As ArgumentException =
            Assert.ThrowsException(Of ArgumentException)(
                Sub()
                    NumericGuards.ValidateFiniteStatistics(
                        Array.Empty(Of Double)(),
                        "statistics")
                End Sub)

        Assert.AreEqual("statistics", emptyEx.ParamName)
        StringAssert.Contains(emptyEx.Message, "At least one statistic is required.")

        For Each invalidValue As Double In
            New Double() {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}

            Dim nonFiniteEx As ArgumentException =
                Assert.ThrowsException(Of ArgumentException)(
                    Sub()
                        NumericGuards.ValidateFiniteStatistics(
                            New Double() {1.0R, invalidValue, 2.0R},
                            "statistics")
                    End Sub)

            Assert.AreEqual("statistics", nonFiniteEx.ParamName)
            StringAssert.Contains(
                nonFiniteEx.Message,
                "Resampling statistics must contain only finite values.")
        Next

    End Sub


    <TestMethod>
    Public Sub ValidationMethods_PreserveCustomMessages()

        Const customMessage As String = "Custom validation message."

        Dim finiteEx As ArgumentOutOfRangeException =
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub()
                    NumericGuards.ValidateFinite(Double.NaN, "x", customMessage)
                End Sub)

        StringAssert.Contains(finiteEx.Message, customMessage)

        Dim positiveEx As ArgumentOutOfRangeException =
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub()
                    NumericGuards.ValidatePositive(0.0R, "x", customMessage)
                End Sub)

        StringAssert.Contains(positiveEx.Message, customMessage)

        Dim statisticsEx As ArgumentException =
            Assert.ThrowsException(Of ArgumentException)(
                Sub()
                    NumericGuards.ValidateFiniteStatistics(
                        Array.Empty(Of Double)(),
                        "statistics",
                        emptyMessage:=customMessage)
                End Sub)

        StringAssert.Contains(statisticsEx.Message, customMessage)

    End Sub

End Class
