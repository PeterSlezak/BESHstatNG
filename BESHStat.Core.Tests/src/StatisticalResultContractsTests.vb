Option Explicit On
Option Strict On

Imports System
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

<TestClass>
Public Class StatisticalResultContractsTests

    <TestMethod>
    Public Sub TestResult_Defaults_MatchLegacyContract()
        Dim result As New BESHStatNG.TestResult()

        Assert.AreEqual(0.0R, result.Pvalue, 0.0R)
        Assert.AreEqual(0.0R, result.PvalueLowerSide, 0.0R)
        Assert.AreEqual(0.0R, result.PvalueUpperSide, 0.0R)
        Assert.AreEqual(0.0R, result.Pvalue2, 0.0R)
        Assert.AreEqual(0.0R, result.PvalueExact, 0.0R)
        Assert.AreEqual(0.0R, result.pValueExactLowerSide, 0.0R)
        Assert.AreEqual(0.0R, result.pValueExactUpperSide, 0.0R)
        Assert.AreEqual(0.0R, result.TestStatistics1, 0.0R)
        Assert.AreEqual(0.0R, result.TestStatistics2, 0.0R)
        Assert.AreEqual(0.0R, result.DF1, 0.0R)
        Assert.AreEqual(0.0R, result.DF2, 0.0R)
        Assert.IsNull(result.strSpecialInformation)
        Assert.IsFalse(result.bExactAvailable)
    End Sub

    <TestMethod>
    Public Sub TestResult_PublicFields_RetainAssignedValues()
        Dim result As New BESHStatNG.TestResult With {
            .Pvalue = 0.01R,
            .PvalueLowerSide = 0.005R,
            .PvalueUpperSide = 0.995R,
            .Pvalue2 = 0.02R,
            .PvalueExact = 0.03R,
            .pValueExactLowerSide = 0.015R,
            .pValueExactUpperSide = 0.985R,
            .TestStatistics1 = 2.5R,
            .TestStatistics2 = 3.5R,
            .DF1 = 10.0R,
            .DF2 = 20.0R,
            .strSpecialInformation = "details",
            .bExactAvailable = True
        }

        Assert.AreEqual(0.01R, result.Pvalue, 0.0R)
        Assert.AreEqual(0.005R, result.PvalueLowerSide, 0.0R)
        Assert.AreEqual(0.995R, result.PvalueUpperSide, 0.0R)
        Assert.AreEqual(0.02R, result.Pvalue2, 0.0R)
        Assert.AreEqual(0.03R, result.PvalueExact, 0.0R)
        Assert.AreEqual(0.015R, result.pValueExactLowerSide, 0.0R)
        Assert.AreEqual(0.985R, result.pValueExactUpperSide, 0.0R)
        Assert.AreEqual(2.5R, result.TestStatistics1, 0.0R)
        Assert.AreEqual(3.5R, result.TestStatistics2, 0.0R)
        Assert.AreEqual(10.0R, result.DF1, 0.0R)
        Assert.AreEqual(20.0R, result.DF2, 0.0R)
        Assert.AreEqual("details", result.strSpecialInformation)
        Assert.IsTrue(result.bExactAvailable)
    End Sub


    <TestMethod>
    Public Sub CIformat_NumericValues_RemainStable()
        Assert.AreEqual(0, Convert.ToInt32(CIformat.E_p_LL_to_UL_p))
        Assert.AreEqual(1, Convert.ToInt32(CIformat.LL_to_UL))
        Assert.AreEqual(2, Convert.ToInt32(CIformat.p_LL_to_UL_p))
    End Sub

    <TestMethod>
    Public Sub ConfidenceInterval_Defaults_MatchLegacyContract()
        Dim ci As New ConfidenceIntervalResult()

        Assert.AreEqual(0.0R, ci.Estimate, 0.0R)
        Assert.AreEqual(0.0R, ci.LowerLimit, 0.0R)
        Assert.AreEqual(0.0R, ci.UpperLimit, 0.0R)
        Assert.AreEqual(0.0R, ci.StdErr, 0.0R)
        Assert.AreEqual(0.05R, ci.alpha, 0.0R)
        Assert.AreEqual("95% Confidence Interval", ci.CIlabel)
    End Sub

    <TestMethod>
    Public Sub ConfidenceInterval_DefaultFormat_MatchesLegacyFormatting()
        Dim ci As New ConfidenceIntervalResult With {
            .Estimate = 1.25R,
            .LowerLimit = 1.0R,
            .UpperLimit = 1.5R
        }

        Dim estimateText As String = Convert.ToSingle(1.25R).ToString(CultureInfo.CurrentCulture)
        Dim lowerText As String = Convert.ToSingle(1.0R).ToString(CultureInfo.CurrentCulture)
        Dim upperText As String = Convert.ToSingle(1.5R).ToString(CultureInfo.CurrentCulture)

        Assert.AreEqual($"{estimateText} ({lowerText} to {upperText})", ci.strConfidenceInterval)
    End Sub

    <TestMethod>
    Public Sub ConfidenceInterval_AlternativeFormats_MatchLegacyContract()
        Dim rangeOnly As New ConfidenceIntervalResult With {
            .Estimate = 1.25R,
            .LowerLimit = 1.0R,
            .UpperLimit = 1.5R
        }
        Dim parenthesized As New ConfidenceIntervalResult With {
            .Estimate = 1.25R,
            .LowerLimit = 1.0R,
            .UpperLimit = 1.5R
        }

        Dim lowerText As String = Convert.ToSingle(1.0R).ToString(CultureInfo.CurrentCulture)
        Dim upperText As String = Convert.ToSingle(1.5R).ToString(CultureInfo.CurrentCulture)

        Assert.AreEqual($"{lowerText} to {upperText}", rangeOnly.strConfidenceInterval(CIformat.LL_to_UL))
        Assert.AreEqual($"({lowerText} to {upperText})", parenthesized.strConfidenceInterval(CIformat.p_LL_to_UL_p))
    End Sub

    <TestMethod>
    Public Sub ConfidenceInterval_SpecialFloatingPointValues_UseLegacyTokens()
        Dim ci As New ConfidenceIntervalResult With {
            .Estimate = Double.NaN,
            .LowerLimit = Double.NegativeInfinity,
            .UpperLimit = Double.PositiveInfinity
        }

        Assert.AreEqual("#N/A (#Ninf to #Pinf)", ci.strConfidenceInterval)
    End Sub

    <TestMethod>
    Public Sub ConfidenceInterval_ExplicitTextOverride_IsPreserved()
        Dim ci As New ConfidenceIntervalResult()
        ci.strConfidenceInterval = "custom interval"

        Assert.AreEqual("custom interval", ci.strConfidenceInterval)
        Assert.AreEqual("custom interval", ci.strConfidenceInterval(CIformat.LL_to_UL))
    End Sub

    <TestMethod>
    Public Sub ConfidenceInterval_Label_UsesCurrentAlpha()
        Dim ci As New ConfidenceIntervalResult With {.alpha = 0.01R}

        Assert.AreEqual("99% Confidence Interval", ci.CIlabel)
    End Sub

    <TestMethod>
    Public Sub ConfidenceInterval_FormatValue_IsCached_AsInLegacyContract()
        Dim ci As New ConfidenceIntervalResult With {
            .Estimate = 2.0R,
            .LowerLimit = 1.0R,
            .UpperLimit = 3.0R
        }

        Dim first As String = ci.strConfidenceInterval(CIformat.LL_to_UL)
        Dim second As String = ci.strConfidenceInterval(CIformat.E_p_LL_to_UL_p)

        Assert.AreEqual(first, second)
    End Sub

End Class
