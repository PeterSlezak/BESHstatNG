Option Explicit On
Option Strict On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Dist = BESHStatNG.distributions.Distributions
Imports Special = BESHStatNG.StatisticalSpecialFunctions

<TestClass>
Public Class DistributionCoreTests

    <TestMethod>
    <DataRow(0.0R, 0.0R, 1.0R, 0.3989422804014327R)>
    <DataRow(1.0R, 0.0R, 1.0R, 0.24197072451914337R)>
    <DataRow(5.0R, 4.0R, 2.0R, 0.17603266338214973R)>
    Public Sub DNorm_MatchesReference(x As Double, mean As Double, sd As Double, expected As Double)
        Assert.AreEqual(expected, Dist.DNorm(x, mean, sd), 0.000000000001)
    End Sub

    <TestMethod>
    Public Sub NormalFunctions_HandleBoundariesAndTails()
        Assert.AreEqual(0.5R, Dist.PNorm(0.0R), 0.0R)
        Assert.AreEqual(0.000000000000000622096057427174R, Dist.PNorm(-8.0R), 1.0E-27R)
        Assert.AreEqual(-4.7534243088228987R, Dist.NormSInv(0.000001R), 0.0000000001R)
        Assert.AreEqual(1.959963984540054R, Dist.ZCritTwoSided(0.05R), 0.0000000001R)
        Assert.IsTrue(Double.IsNegativeInfinity(Dist.QNorm(0.0R, 0.0R, 1.0R)))
        Assert.IsTrue(Double.IsPositiveInfinity(Dist.QNorm(1.0R, 0.0R, 1.0R)))
        Assert.IsTrue(Double.IsNaN(Dist.QNorm(0.5R, 0.0R, 0.0R)))
    End Sub

    <TestMethod>
    Public Sub NormSInv_InvalidProbability_Throws()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Dist.NormSInv(0.0R))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Dist.NormSInv(1.0R))
    End Sub

    <TestMethod>
    Public Sub ZCritTwoSided_InvalidAlpha_Throws()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Dist.ZCritTwoSided(0.0R))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Dist.ZCritTwoSided(1.0R))
    End Sub

    <TestMethod>
    Public Sub ChiSquareFunctions_MatchReferenceAndBoundaries()
        Assert.AreEqual(0.18393972058572117R, Dist.ChiSquarePDF(2.0R, 4.0R), 0.0000000001R)
        Assert.AreEqual(0.26424111765711528R, Dist.ChiSquareCDF(2.0R, 4.0R), 0.0000000001R)
        Assert.AreEqual(9.487729036781154R, Dist.ChiSquareInv(0.95R, 4.0R), 0.00000001R)
        Assert.AreEqual(0.0R, Dist.ChiSquareCDF(0.0R, 4.0R), 0.0R)
        Assert.AreEqual(1.0R, Dist.ChiSquareCDF(Double.PositiveInfinity, 4.0R), 0.0R)
        Assert.IsTrue(Double.IsNaN(Dist.ChiSquareCDF(1.0R, 0.0R)))
    End Sub

    <TestMethod>
    Public Sub StudentTFunctions_MatchReference()
        Assert.AreEqual(0.12744479428709171R, Dist.T_PDF(1.5R, 10.0R), 0.0000000001R)
        Assert.AreEqual(0.9177463367772799R, Dist.T_CDF(1.5R, 10.0R), 0.0000000001R)
        Assert.AreEqual(0.0822536632227201R, Dist.T_RT(1.5R, 10.0R), 0.0000000001R)
        Assert.AreEqual(0.16450732644544019R, Dist.T_2T(1.5R, 10.0R), 0.0000000001R)
        Assert.AreEqual(2.2281388519649385R, Dist.T_Inv(0.975R, 10.0R), 0.00000001R)
        Assert.AreEqual(2.2281388519649385R, Dist.T_Inv_2T(0.05R, 10.0R), 0.00000001R)
    End Sub

    <TestMethod>
    Public Sub IncompleteBetaFunctions_MatchReference()
        Assert.AreEqual(0.064170898309479488R,
                        Dist.RegularizedIncompleteBeta(0.3R, 2.5R, 1.2R),
                        0.0000000001R)
        Assert.AreEqual(0.82657614545362568R,
                        Dist.InverseRegularizedIncompleteBeta(0.7R, 2.5R, 1.2R),
                        0.00000001R)
    End Sub

    <TestMethod>
    Public Sub FDistributionFunctions_MatchReference()
        Assert.AreEqual(0.045828457479904425R, Dist.F_PDF(3.2R, 5.0R, 10.0R), 0.0000000001R)
        Assert.AreEqual(0.94457063010301012R, Dist.F_CDF(3.2R, 5.0R, 10.0R), 0.0000000001R)
        Assert.AreEqual(0.055429369896989926R, Dist.F_RT(3.2R, 5.0R, 10.0R), 0.0000000001R)
        Assert.AreEqual(3.3258345304130112R, Dist.F_Inv(0.95R, 5.0R, 10.0R), 0.00000001R)
        Assert.AreEqual(3.3258345304130112R, Dist.F_Inv_RT(0.05R, 5.0R, 10.0R), 0.00000001R)
    End Sub

    <TestMethod>
    Public Sub PoissonFunctions_MatchReferenceAndEdgeCases()
        Assert.AreEqual(0.18044704431548358R, Dist.PoissonPMF(3.7R, 2.0R), 0.000000000001R)
        Assert.AreEqual(0.857123460498547R, Dist.PoissonCDF(3.7R, 2.0R), 0.000000000001R)
        Assert.AreEqual(0.14287653950145296R, Dist.PoissonUpperTail(3.7R, 2.0R), 0.000000000001R)
        Assert.AreEqual(3, Dist.PoissonInv(0.8R, 2.0R))
        Assert.AreEqual(0, Dist.PoissonInv(0.0R, 2.0R))
        Assert.AreEqual(Integer.MaxValue, Dist.PoissonInv(1.0R, 2.0R))
        Assert.AreEqual(Integer.MinValue, Dist.PoissonInv(0.5R, -1.0R))
    End Sub

    <TestMethod>
    Public Sub BinomialDistribution_MatchesReferenceAndRejectsInvalidInputs()
        Assert.AreEqual(0.20132659199999992R, Dist.BinomDist(3, 10, 0.2R, False), 0.000000000001R)
        Assert.AreEqual(0.87912611839999988R, Dist.BinomDist(3, 10, 0.2R, True), 0.000000000001R)
        Assert.IsTrue(Double.IsNaN(Dist.BinomDist(-1, 10, 0.2R, True)))
        Assert.IsTrue(Double.IsNaN(Dist.BinomDist(11, 10, 0.2R, True)))
        Assert.IsTrue(Double.IsNaN(Dist.BinomDist(3, 10, -0.1R, True)))
    End Sub

    <TestMethod>
    Public Sub StudentizedRangeFunctions_MatchReference()
        Dim iFault As Integer = 0
        Dim cdf As Double = Dist.PRTRNG(4.0R, 10.0R, 5.0R, iFault)
        Assert.AreEqual(0, iFault)
        Assert.AreEqual(0.89804509472586513R, cdf, 0.000001R)

        iFault = 0
        Dim quantile As Double = Dist.QTRNG(0.95R, 10.0R, 5.0R, iFault)
        Assert.AreEqual(0, iFault)
        Assert.AreEqual(4.6542929978545375R, quantile, 0.0005R)
        Assert.IsTrue(Dist.QTRNG0(0.95R, 10.0R, 5.0R) > Dist.QTRNG0(0.9R, 10.0R, 5.0R))
    End Sub

    <TestMethod>
    Public Sub StudentizedRange_BoundaryAndInvalidInputs_PreserveLegacyFaultCodes()
        Dim iFault As Integer = 0

        ' Legacy AS 190 behavior: q <= 0 returns zero without flagging a fault.
        Assert.AreEqual(0.0R, Dist.PRTRNG(-1.0R, 10.0R, 5.0R, iFault), 0.0R)
        Assert.AreEqual(0, iFault)

        iFault = 0
        Assert.AreEqual(0.0R, Dist.PRTRNG(1.0R, 0.0R, 5.0R, iFault), 0.0R)
        Assert.AreEqual(1, iFault)

        iFault = 0
        Assert.AreEqual(0.0R, Dist.QTRNG(0.5R, 10.0R, 5.0R, iFault), 0.0R)
        Assert.AreEqual(2, iFault)
    End Sub

    <TestMethod>
    Public Sub CoreSpecialFunctions_MatchLegacyReferences()
        Assert.AreEqual(14.770621922970371R, Special.LogCombin(52, 5), 0.000000000001R)
        Assert.AreEqual(0.5723649429247R, Special.LogGamma(0.5R), 0.0000000001R)
        Assert.AreEqual(3.1780538303479458R, Special.LogGamma(5.0R), 0.0000000001R)
        Assert.AreEqual(0.15085496391539038R, Special.LowerIncompleteGamma(2.5R, 1.0R), 0.0000000001R)
        Assert.AreEqual(0.97074731192303887R, Special.LowerIncompleteGamma(5.0R, 10.0R), 0.0000000001R)
    End Sub

End Class
