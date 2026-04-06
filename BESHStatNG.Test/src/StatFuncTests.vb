Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.StatFunc
Imports BESHStatNG.Matrix

<TestClass()>
Public Class StatFunc_Tests

    Private Const TOL As Double = 1.0E-12

    ' ============================================================
    ' Basic math helpers
    ' ============================================================

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(0, 0#)>
    <DataRow(5, 4.7874917427820458#)>
    <DataRow(10, 15.104412573075516#)>
    <DataRow(25, 58.003605222980518#)>
    Public Sub LogFactorial_matches_reference(y As Integer, expected As Double)
        Dim actual As Double = LogFactorial(y)
        Assert.AreEqual(expected, actual, 0.0000000001)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Minimum_generic_matches_reference()
        Assert.AreEqual(1, Minimum(5, 2, 9, 1, 7))
        Assert.AreEqual(-3, Minimum(-3, 0, 4))
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Minimum_generic_empty_throws()
        Assert.ThrowsException(Of ArgumentException)(Sub() Minimum(Of Integer)())
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(5.0#, 2.0#, 10.0#)>
    <DataRow(52.0#, 5.0#, 2598960.0#)>
    Public Sub Combin_matches_reference(n As Double, k As Double, expected As Double)
        Dim actual As Double = Combin(n, k)
        Assert.AreEqual(expected, actual, 0.0#)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(52, 5, 14.770621922970371#)>
    Public Sub LogCombin_matches_reference(n As Integer, k As Integer, expected As Double)
        Dim actual As Double = LogCombin(n, k)
        Assert.AreEqual(expected, actual, 1.0E-12)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(180.0#, 3.1415926535897931#)>
    <DataRow(45.0#, 0.78539816339744828#)>
    Public Sub Radians_matches_reference(deg As Double, expected As Double)
        Dim actual As Double = Radians(deg)
        Assert.AreEqual(expected, actual, 1.0E-12)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(0.5#, 0.54930614433405489#)>
    <DataRow(-0.2#, -0.20273255405408214#)>
    Public Sub Atanh_matches_reference(x As Double, expected As Double)
        Dim actual As Double = Atanh(x)
        Assert.AreEqual(expected, actual, 1.0E-12)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(3.14159#, 2, 3.14#)>
    <DataRow(-3.14159#, 2, -3.14#)>
    <DataRow(123.0#, 0, 123.0#)>
    Public Sub RoundDown_matches_excel_style(number As Double, digits As Integer, expected As Double)
        Dim actual As Double = RoundDown(number, digits)
        Assert.AreEqual(expected, actual, 0.0#)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(3.14159#, 2, 3.15#)>
    <DataRow(-3.14159#, 2, -3.15#)>
    <DataRow(123.0#, 0, 123.0#)>
    Public Sub RoundUp_matches_excel_style(number As Double, digits As Integer, expected As Double)
        Dim actual As Double = RoundUp(number, digits)
        Assert.AreEqual(expected, actual, 0.0#)
    End Sub

    ' ============================================================
    ' Regression / correlation
    ' ============================================================

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Slope_and_Intercept_match_reference()
        Dim x() As Double = {1, 2, 3, 4}
        Dim y() As Double = {3, 5, 7, 9} ' y = 2x + 1

        Dim m As Double = Slope(y, x)
        Dim b As Double = Intercept(y, x)

        Assert.AreEqual(2.0#, m, 1.0E-12)
        Assert.AreEqual(1.0#, b, 1.0E-12)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Correl_matches_reference()
        Dim x() As Double = {1, 2, 3, 4}
        Dim y() As Double = {10, 20, 30, 40}
        Assert.AreEqual(1.0#, Correl(x, y), 1.0E-12)

        Dim y2() As Double = {40, 30, 20, 10}
        Assert.AreEqual(-1.0#, Correl(x, y2), 1.0E-12)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Correl_constant_series_returns_nan()
        Dim x() As Double = {1, 1, 1}
        Dim y() As Double = {1, 2, 3}
        Assert.IsTrue(Double.IsNaN(Correl(x, y)))
    End Sub

    ' ============================================================
    ' Array helpers (2D)
    ' ============================================================

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Sum2D_and_Average2D_match_reference()
        Dim m(,) As Integer = New Integer(,) {{1, 2}, {3, 4}}
        Assert.AreEqual(10.0#, Sum2D(m), 0.0#)
        Assert.AreEqual(2.5#, Average2D(m), 0.0#)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Minimum2D_and_Maximum2D_match_reference()
        Dim m(,) As Double = New Double(,) {{-1.0#, 2.0#}, {3.5#, 0.0#}}
        Assert.AreEqual(-1.0#, Minimum2D(m), 0.0#)
        Assert.AreEqual(3.5#, Maximum2D(m), 0.0#)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub SumSq_1D_and_2D_match_reference()
        Dim v() As Integer = {1, 2, 3}
        Assert.AreEqual(14.0#, SumSq(v), 0.0#)

        Dim m(,) As Integer = New Integer(,) {{1, 2}, {3, 4}}
        Assert.AreEqual(30.0#, SumSq(m), 0.0#)
    End Sub

    ' ============================================================
    ' Descriptive statistics
    ' ============================================================

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Variance_and_StDev_match_reference()
        Dim x() As Double = {1, 2, 3, 4}
        Dim v As Double = variance(x) ' sample variance
        Dim s As Double = stDev(x)

        Assert.AreEqual(1.6666666666666667#, v, 1.0E-12)
        Assert.AreEqual(1.2909944487358056#, s, 1.0E-12)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub DevSq_matches_reference()
        Dim x() As Double = {1, 2, 3}
        Assert.AreEqual(2.0#, DevSq(x), 1.0E-12)
        Assert.AreEqual(2.0#, DevSq(1.0#, 2.0#, 3.0#), 1.0E-12)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Median_and_QuartilesComp_match_reference()
        Dim x() As Double = {1, 2, 3, 4}
        Assert.AreEqual(2.5#, Median(x), 1.0E-12)

        Dim q As udQuartiles = QuartilesComp(x)
        Assert.AreEqual(1.5#, q.Q1, 1.0E-12)
        Assert.AreEqual(2.5#, q.Median, 1.0E-12)
        Assert.AreEqual(3.5#, q.Q3, 1.0E-12)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Skewness_and_Kurtosis_match_reference_for_symmetric_data()
        ' Symmetric around mean => skewness ~ 0
        Dim x() As Double = {-2, -1, 0, 1, 2}
        Assert.AreEqual(0.0#, Skewness(x), 1.0E-12)

        ' Kurtosis depends on definition (excess vs non-excess), so check it's finite
        Dim k As Double = Kurtosis(x)
        Assert.IsFalse(Double.IsNaN(k))
        Assert.IsFalse(Double.IsInfinity(k))
    End Sub

    ' ============================================================
    ' Special functions
    ' ============================================================

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(1.0#, -0.57721566490153287#)>
    <DataRow(2.5#, 0.70315664064524319#)>
    Public Sub Digamma_matches_reference(x As Double, expected As Double)
        Dim actual As Double = digamma(x)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(1.0#, 1.6449340668482266#)>
    <DataRow(2.5#, 0.49035775610023491#)>
    Public Sub Trigamma_matches_reference(x As Double, expected As Double)
        Dim actual As Double = trigamma(x)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(0.5#, 0.57236494292469997#)>
    <DataRow(5.0#, 3.1780538303479458#)>
    <DataRow(10.2#, 13.254266744235549#)>
    Public Sub LogGamma_matches_reference(x As Double, expected As Double)
        Dim actual As Double = LogGamma(x)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(2.5#, 1.0#, 0.15085496391539038#)>
    <DataRow(5.0#, 10.0#, 0.97074731192303887#)>
    Public Sub LowerIncompleteGamma_matches_reference(a As Double, x As Double, expected As Double)
        Dim actual As Double = LowerIncompleteGamma(a, x)
        Assert.AreEqual(expected, actual, 1.0E-10)
    End Sub

    ' ============================================================
    ' F test (two-tailed) and percentile (exclusive)
    ' ============================================================

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub FTest_matches_reference()
        Dim a1() As Double = {1.2, 0.9, 1.1, 1.0, 1.3}
        Dim a2() As Double = {1.8, 2.0, 1.6, 1.9, 2.1}
        Dim actual As Double = FTest(a1, a2)
        Assert.AreEqual(0.71330267530462221#, actual, 1.0E-10)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    <DataRow(0.25#, 2.75#)>
    <DataRow(0.5#, 5.5#)>
    <DataRow(0.75#, 8.25#)>
    Public Sub PercentileExc_matches_reference(k As Double, expected As Double)
        Dim data() As Double = {1,2,3,4,5,6,7,8,9,10}
        Dim actual As Double = Percentile_Exc(data, k)
        Assert.AreEqual(expected, actual, 1.0E-12)
    End Sub

    ' ============================================================
    ' Covariance / correlation helpers
    ' ============================================================

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub Cov2Corr_and_Corr2Cov_roundtrip()
        Dim corr(,) As Double = New Double(,) {{1.0#, 0.5#}, {0.5#, 1.0#}}
        Dim std() As Double = {2.0#, 3.0#}

        Dim cov(,) As Double = corr2cov(corr, std)
        Assert.AreEqual(4.0#, cov(0,0), 1.0E-12)
        Assert.AreEqual(3.0#, cov(0,1), 1.0E-12)
        Assert.AreEqual(3.0#, cov(1,0), 1.0E-12)
        Assert.AreEqual(9.0#, cov(1,1), 1.0E-12)

        Dim stdOut() As Double = Nothing
        Dim corr2(,) As Double = cov2corr(cov, stdOut)

        Assert.AreEqual(2.0#, stdOut(0), 1.0E-12)
        Assert.AreEqual(3.0#, stdOut(1), 1.0E-12)
        Assert.AreEqual(1.0#, corr2(0,0), 1.0E-12)
        Assert.AreEqual(0.5#, corr2(0,1), 1.0E-12)
        Assert.AreEqual(0.5#, corr2(1,0), 1.0E-12)
        Assert.AreEqual(1.0#, corr2(1,1), 1.0E-12)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub CorrClipped_makes_matrix_psd()
        ' Not a valid correlation matrix (off-diagonal > 1) -> has a negative eigenvalue
        Dim corr(,) As Double = New Double(,) {{1.0#, 1.2#}, {1.2#, 1.0#}}
        Dim fixed(,) As Double = corrClipped(corr, 0.0#)

        ' Should normalize back to a valid correlation matrix; this case clips to perfect correlation
        Assert.AreEqual(1.0#, fixed(0,0), 1.0E-12)
        Assert.AreEqual(1.0#, fixed(1,1), 1.0E-12)
        Assert.AreEqual(1.0#, fixed(0,1), 1.0E-12)
        Assert.AreEqual(1.0#, fixed(1,0), 1.0E-12)

        ' And eigenvalues should be >= 0
        Dim ei = EIGEN_JK(fixed)
        Dim minEval As Double = ei.Item1.Min()
        Assert.IsTrue(minEval >= -1.0E-10)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub CovNearest_produces_psd_matrix()
        ' Indefinite covariance matrix (one negative eigenvalue)
        Dim cov(,) As Double = New Double(,) {{4.0#, 7.0#}, {7.0#, 9.0#}}
        Dim fixed(,) As Double = CovNearest(cov, 0.0#)

        ' Symmetry
        Assert.AreEqual(fixed(0,1), fixed(1,0), 1.0E-12)

        ' PSD check via eigenvalues
        Dim ei = EIGEN_JK(fixed)
        Dim minEval As Double = ei.Item1.Min()
        Assert.IsTrue(minEval >= -1.0E-10)
    End Sub


    ' ============================================================
    ' Studentized range (AS 190)
    ' ============================================================

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub PRTRNG_matches_reference()
        Dim ifault As Integer = 0
        Dim q As Double = 4.0#
        Dim v As Double = 10.0#
        Dim r As Double = 5.0#
        Dim actual As Double = distributions.PRTRNG(q, v, r, ifault)
        Assert.AreEqual(0, ifault)
        Assert.AreEqual(0.89804509472586513#, actual, 1.0E-6)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub QTRNG_matches_reference()
        Dim ifault As Integer = 0
        Dim p As Double = 0.95#
        Dim v As Double = 10.0#
        Dim r As Double = 5.0#
        Dim actual As Double = distributions.QTRNG(p, v, r, ifault)
        Assert.AreEqual(0, ifault)
        Assert.AreEqual(4.6542929978545375#, actual, 0.0005)
    End Sub

    <TestCategory("StatFunc")>
    <TestMethod()>
    Public Sub QTRNG0_is_monotone_in_p()
        Dim v As Double = 10.0#
        Dim r As Double = 5.0#
        Dim q90 As Double = distributions.QTRNG0(0.9#, v, r)
        Dim q95 As Double = distributions.QTRNG0(0.95#, v, r)
        Assert.IsTrue(q95 > q90)
    End Sub

End Class
