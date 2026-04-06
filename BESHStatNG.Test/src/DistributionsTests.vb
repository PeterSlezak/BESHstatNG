Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.Distributions

<TestClass()>
Public Class Distributions_Tests

    Private Const TOL As Double = 1.0E-12

    ' ============================================================
    ' Normal distribution
    ' ============================================================

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.0#, 0.0#, 1.0#, 0.3989422804014327#)>
    <DataRow(1.0#, 0.0#, 1.0#, 0.24197072451914337#)>
    <DataRow(5.0#, 4.0#, 2.0#, 0.17603266338214973#)>
    Public Sub DNorm_matches_reference(x As Double, mean As Double, sd As Double, expected As Double)
        Dim actual As Double = DNorm(x, mean, sd)
        Assert.AreEqual(expected, actual, TOL)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    Public Sub DNorm_sd_nonpositive_returns_NaN()
        Assert.IsTrue(Double.IsNaN(DNorm(0.0#, 0.0#, 0.0#)))
        Assert.IsTrue(Double.IsNaN(DNorm(0.0#, 0.0#, -1.0#)))
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.0#, 0.0#, 1.0#, 0.5#)>
    <DataRow(1.0#, 0.0#, 1.0#, 0.8413447460685429#)>
    <DataRow(5.0#, 4.0#, 2.0#, 0.6914624612740131#)>
    Public Sub PNorm_matches_reference(q As Double, mean As Double, sd As Double, expected As Double)
        Dim actual As Double = PNorm(q, mean, sd)
        Assert.AreEqual(expected, actual, TOL)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.975#, 0.0#, 1.0#, 1.959963984540054#)>
    <DataRow(0.9#, 4.0#, 2.0#, 6.563103131089201#)>
    Public Sub QNorm_matches_reference(p As Double, mean As Double, sd As Double, expected As Double)
        Dim actual As Double = QNorm(p, mean, sd)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    Public Sub QNorm_at_0_and_1_returns_infinities()
        Assert.IsTrue(Double.IsNegativeInfinity(QNorm(0.0#, 0.0#, 1.0#)))
        Assert.IsTrue(Double.IsPositiveInfinity(QNorm(1.0#, 0.0#, 1.0#)))
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.975#, 1.959963984540054#)>
    <DataRow(0.5#, 0.0#)>
    Public Sub NormSInv_matches_reference(p As Double, expected As Double)
        Dim actual As Double = NormSInv(p)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    Public Sub NormSInv_invalid_p_throws()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() NormSInv(0.0#))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() NormSInv(1.0#))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() NormSInv(-0.1#))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() NormSInv(1.1#))
    End Sub

    ' ============================================================
    ' Chi-square distribution
    ' ============================================================

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(2.0#, 4.0#, 0.18393972058572117#)>
    Public Sub ChiSquarePDF_matches_reference(x As Double, df As Double, expected As Double)
        Dim actual As Double = ChiSquarePDF(x, df)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(2.0#, 4.0#, 0.2642411176571153#)>
    Public Sub ChiSquareCDF_matches_reference(x As Double, df As Double, expected As Double)
        Dim actual As Double = ChiSquareCDF(x, df)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.95#, 4.0#, 9.487729036781154#)>
    Public Sub ChiSquareInv_matches_reference(p As Double, df As Double, expected As Double)
        Dim actual As Double = ChiSquareInv(p, df)
        Assert.AreEqual(expected, actual, 1.0E-8)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    Public Sub ChiSquareInv_edge_cases()
        Assert.AreEqual(0.0#, ChiSquareInv(0.0#, 4.0#), 0.0#)
        Assert.IsTrue(Double.IsPositiveInfinity(ChiSquareInv(1.0#, 4.0#)))
        Assert.IsTrue(Double.IsNaN(ChiSquareInv(0.5#, -1.0#)))
    End Sub

    ' ============================================================
    ' Student t distribution
    ' ============================================================

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(1.5#, 10.0#, 0.1274447942870917#)>
    Public Sub T_PDF_matches_reference(x As Double, df As Double, expected As Double)
        Dim actual As Double = T_PDF(x, df)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(1.5#, 10.0#, 0.9177463367772799#)>
    Public Sub T_CDF_matches_reference(x As Double, df As Double, expected As Double)
        Dim actual As Double = T_CDF(x, df)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(1.5#, 10.0#, 0.0822536632227201#)>
    Public Sub T_RT_matches_reference(x As Double, df As Double, expected As Double)
        Dim actual As Double = T_RT(x, df)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(1.5#, 10.0#, 0.1645073264454402#)>
    Public Sub T_2T_matches_reference(x As Double, df As Double, expected As Double)
        Dim actual As Double = T_2T(x, df)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.975#, 10.0#, 2.2281388519649385#)>
    Public Sub T_Inv_matches_reference(p As Double, df As Double, expected As Double)
        Dim actual As Double = T_Inv(p, df)
        Assert.AreEqual(expected, actual, 1.0E-8)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.05#, 10.0#, 2.2281388519649385#)>
    Public Sub T_Inv_2T_matches_reference(p As Double, df As Double, expected As Double)
        Dim actual As Double = T_Inv_2T(p, df)
        Assert.AreEqual(expected, actual, 1.0E-8)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    Public Sub T_Inv_edge_cases()
        Assert.IsTrue(Double.IsNegativeInfinity(T_Inv(0.0#, 10.0#)))
        Assert.IsTrue(Double.IsPositiveInfinity(T_Inv(1.0#, 10.0#)))
        Assert.IsTrue(Double.IsNaN(T_Inv(0.5#, 0.0#)))
    End Sub

    ' ============================================================
    ' Incomplete beta (core special functions)
    ' ============================================================

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.3#, 2.5#, 1.2#, 0.06417089830947949#)>
    Public Sub RegularizedIncompleteBeta_matches_reference(x As Double, a As Double, b As Double, expected As Double)
        Dim actual As Double = RegularizedIncompleteBeta(x, a, b)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.7#, 2.5#, 1.2#, 0.8265761454536257#)>
    Public Sub InverseRegularizedIncompleteBeta_matches_reference(p As Double, a As Double, b As Double, expected As Double)
        Dim actual As Double = InverseRegularizedIncompleteBeta(p, a, b)
        Assert.AreEqual(expected, actual, 1.0E-8)
    End Sub

    ' ============================================================
    ' F distribution
    ' ============================================================

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(3.2#, 5.0#, 10.0#, 0.045828457479904425#)>
    Public Sub F_PDF_matches_reference(x As Double, df1 As Double, df2 As Double, expected As Double)
        Dim actual As Double = F_PDF(x, df1, df2)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(3.2#, 5.0#, 10.0#, 0.9445706301030101#)>
    Public Sub F_CDF_matches_reference(x As Double, df1 As Double, df2 As Double, expected As Double)
        Dim actual As Double = F_CDF(x, df1, df2)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(3.2#, 5.0#, 10.0#, 0.055429369896989926#)>
    Public Sub F_RT_matches_reference(x As Double, df1 As Double, df2 As Double, expected As Double)
        Dim actual As Double = F_RT(x, df1, df2)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(3.2#, 5.0#, 10.0#, 0.11085873979397985#)>
    Public Sub F_2T_matches_reference(x As Double, df1 As Double, df2 As Double, expected As Double)
        Dim actual As Double = F_2T(x, df1, df2)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.95#, 5.0#, 10.0#, 3.3258345304130112#)>
    Public Sub F_Inv_matches_reference(p As Double, df1 As Double, df2 As Double, expected As Double)
        Dim actual As Double = F_Inv(p, df1, df2)
        Assert.AreEqual(expected, actual, 1.0E-8)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(0.05#, 5.0#, 10.0#, 3.3258345304130112#)>
    Public Sub F_Inv_RT_matches_reference(p As Double, df1 As Double, df2 As Double, expected As Double)
        Dim actual As Double = F_Inv_RT(p, df1, df2)
        Assert.AreEqual(expected, actual, 1.0E-8)
    End Sub

    ' ============================================================
    ' Poisson distribution
    ' ============================================================

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(3.7#, 2.0#, 0.18044704431548358#)>
    Public Sub PoissonPMF_truncates_x_and_matches_reference(x As Double, lambda As Double, expected As Double)
        Dim actual As Double = PoissonPMF(x, lambda)
        Assert.AreEqual(expected, actual, 1.0E-12)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(3.7#, 2.0#, 0.857123460498547#)>
    Public Sub PoissonCDF_truncates_x_and_matches_reference(x As Double, lambda As Double, expected As Double)
        Dim actual As Double = PoissonCDF(x, lambda)
        Assert.AreEqual(expected, actual, 1.0E-12)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(3.7#, 2.0#, 0.14287653950145296#)>
    Public Sub PoissonUpperTail_matches_reference(x As Double, lambda As Double, expected As Double)
        Dim actual As Double = PoissonUpperTail(x, lambda)
        Assert.AreEqual(expected, actual, 1.0E-12)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    Public Sub PoissonInv_matches_reference_and_edge_cases()
        Assert.AreEqual(3, PoissonInv(0.8#, 2.0#))
        Assert.AreEqual(0, PoissonInv(0.0#, 2.0#))
        Assert.AreEqual(Integer.MaxValue, PoissonInv(1.0#, 2.0#))
        Assert.AreEqual(Integer.MinValue, PoissonInv(-0.1#, 2.0#))
        Assert.AreEqual(Integer.MinValue, PoissonInv(0.5#, -1.0#))
    End Sub

    ' ============================================================
    ' Binomial distribution
    ' ============================================================

    <TestCategory("Distributions")>
    <TestMethod()>
    <DataRow(3, 10, 0.2#, False, 0.20132659199999992#)>
    <DataRow(3, 10, 0.2#, True, 0.8791261183999999#)>
    Public Sub BinomDist_matches_reference(x As Integer, n As Integer, p As Double, cumulative As Boolean, expected As Double)
        Dim actual As Double = BinomDist(x, n, p, cumulative)
        Assert.AreEqual(expected, actual, 1.0E-12)
    End Sub

    <TestCategory("Distributions")>
    <TestMethod()>
    Public Sub BinomDist_invalid_inputs_return_NaN()
        Assert.IsTrue(Double.IsNaN(BinomDist(-1, 10, 0.2#, True)))
        Assert.IsTrue(Double.IsNaN(BinomDist(11, 10, 0.2#, True)))
        Assert.IsTrue(Double.IsNaN(BinomDist(3, 10, -0.1#, True)))
        Assert.IsTrue(Double.IsNaN(BinomDist(3, 10, 1.1#, False)))
    End Sub

End Class
