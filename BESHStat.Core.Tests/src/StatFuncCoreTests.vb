Option Explicit On
Option Strict On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SF = BESHStatNG.StatFunc

<TestClass>
Public Class StatFuncCoreTests

    <TestMethod>
    Public Sub BasicCombinatorialAndPercentileHelpers_MatchReferenceValues()
        Assert.AreEqual(Math.Log(120.0R), SF.LogFactorial(5), 0.000000000000001R)
        Assert.IsTrue(Double.IsNaN(SF.LogFactorial(-1)))
        Assert.AreEqual(-3, SF.Minimum(5, 2, 9, -3))

        Dim values() As Double = {1.0R, 2.0R, 3.0R, 4.0R, 5.0R}
        Assert.AreEqual(3.0R, SF.Percentile_Exc(values, 0.5R), 0.0R)
        Assert.AreEqual(1.5R, SF.Percentile_Exc(values, 0.25R), 0.0R)
        Assert.AreEqual(210.0R, SF.Combin(10.0R, 4.0R), 0.0R)
        Assert.AreEqual(Math.Log(210.0R), SF.LogCombin(10, 4), 0.000000000000001R)
    End Sub

    <TestMethod>
    Public Sub CorrelationAndSimpleRegression_MatchReferenceValues()
        Dim x() As Double = {1.0R, 2.0R, 3.0R, 4.0R}
        Dim y() As Double = {3.0R, 5.0R, 7.0R, 9.0R}

        Assert.AreEqual(1.0R, SF.Correl(x, y), 0.000000000000001R)
        Assert.AreEqual(2.0R, SF.Slope(y, x), 0.000000000000001R)
        Assert.AreEqual(1.0R, SF.Intercept(y, x), 0.000000000000001R)
        Assert.AreEqual(1.0R, SF.FTest(x, x), 0.000000000001R)
    End Sub

    <TestMethod>
    Public Sub ScalarAndRoundingHelpers_PreserveLegacySemantics()
        Assert.AreEqual(Math.PI, SF.Radians(180.0R), 0.000000000000001R)
        Assert.AreEqual(0.54930614433405489R, SF.Atanh(0.5R), 0.000000000000001R)
        Assert.IsTrue(Double.IsNaN(SF.Atanh(1.0R)))
        Assert.AreEqual(3.14R, SF.RoundDown(3.14159R, 2), 0.0R)
        Assert.AreEqual(-3.14R, SF.RoundDown(-3.14159R, 2), 0.0R)
        Assert.AreEqual(3.15R, SF.RoundUp(3.14159R, 2), 0.0R)
        Assert.AreEqual(-3.15R, SF.RoundUp(-3.14159R, 2), 0.0R)
    End Sub

    <TestMethod>
    Public Sub TwoDimensionalHelpers_WorkForNumericArrays()
        Dim values(,) As Integer = {
            {1, 2, 3},
            {4, 5, 6}
        }
        Dim doubles(,) As Double = {
            {-2.0R, 4.0R},
            {7.0R, 1.0R}
        }

        Assert.AreEqual(21.0R, SF.Sum2D(values), 0.0R)
        Assert.AreEqual(3.5R, SF.Average2D(values), 0.0R)
        Assert.AreEqual(-2.0R, SF.Minimum2D(doubles), 0.0R)
        Assert.AreEqual(7.0R, SF.Maximum2D(doubles), 0.0R)
    End Sub

    <TestMethod>
    Public Sub CovarianceCorrelationHelpers_UseCoreMatrixFoundation()
        Dim covariance(,) As Double = {
            {4.0R, 2.0R},
            {2.0R, 9.0R}
        }
        Dim std() As Double = Nothing

        Dim correlation(,) As Double = SF.cov2corr(covariance, std)

        Assert.AreEqual(2.0R, std(0), 0.000000000000001R)
        Assert.AreEqual(3.0R, std(1), 0.000000000000001R)
        Assert.AreEqual(1.0R, correlation(0, 0), 0.000000000000001R)
        Assert.AreEqual(1.0R / 3.0R, correlation(0, 1), 0.000000000000001R)
        Assert.AreEqual(1.0R, correlation(1, 1), 0.000000000000001R)

        Dim roundTrip(,) As Double = SF.corr2cov(correlation, std)
        For row As Integer = 0 To 1
            For column As Integer = 0 To 1
                Assert.AreEqual(covariance(row, column), roundTrip(row, column), 0.000000000000001R)
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub EigenvalueClipping_UsesCoreEigensystemAndMatrixArithmetic()
        Dim indefinite(,) As Double = {
            {1.0R, 2.0R},
            {2.0R, 1.0R}
        }
        Dim clipped As Boolean = False

        Dim repaired(,) As Double = SF.clipEvals(indefinite, clipped, 0.0R)

        Assert.IsTrue(clipped)
        Assert.AreEqual(1.5R, repaired(0, 0), 0.000000001R)
        Assert.AreEqual(1.5R, repaired(0, 1), 0.000000001R)
        Assert.AreEqual(repaired(0, 1), repaired(1, 0), 0.000000001R)
        Assert.AreEqual(1.5R, repaired(1, 1), 0.000000001R)
    End Sub

    <TestMethod>
    Public Sub SpecialFunctionsAndMoments_MatchReferenceValues()
        Assert.AreEqual(-0.57721566490153287R, SF.digamma(1.0R), 0.0000000001R)
        Assert.AreEqual(Math.PI * Math.PI / 6.0R, SF.trigamma(1.0R), 0.0000000001R)
        Assert.AreEqual(Math.Log(24.0R), SF.LogGamma(5.0R), 0.000000000001R)
        Assert.AreEqual(0.80085172652854419R, SF.LowerIncompleteGamma(2.0R, 3.0R), 0.000000000001R)

        Dim symmetric() As Double = {1.0R, 2.0R, 3.0R, 4.0R, 5.0R}
        Assert.AreEqual(0.0R, SF.Skewness(symmetric), 0.000000000000001R)
        Assert.AreEqual(1.7R, SF.Kurtosis(symmetric), 0.000000000000001R)
    End Sub

    <TestMethod>
    Public Sub VarianceQuartilesWhiskersAndSumSquares_MatchReferenceValues()
        Dim values() As Double = {1.0R, 2.0R, 3.0R, 4.0R}
        Assert.AreEqual(5.0R / 3.0R, SF.variance(values), 0.000000000000001R)
        Assert.AreEqual(Math.Sqrt(5.0R / 3.0R), SF.stDev(values), 0.000000000000001R)
        Assert.AreEqual(5.0R, SF.DevSq(values), 0.000000000000001R)
        Assert.AreEqual(2.5R, SF.Median(values), 0.0R)
        Assert.AreEqual(30.0R, SF.SumSq(values), 0.0R)

        Dim quartileInput() As Double = {8.0R, 1.0R, 6.0R, 3.0R, 5.0R, 2.0R, 7.0R, 4.0R}
        Dim quartiles As BESHStatNG.udQuartiles = SF.QuartilesComp(quartileInput)
        Assert.AreEqual(2.5R, quartiles.Q1, 0.0R)
        Assert.AreEqual(4.5R, quartiles.Median, 0.0R)
        Assert.AreEqual(6.5R, quartiles.Q3, 0.0R)

        Dim whiskerValues() As Double = {1.0R, 2.0R, 3.0R, 4.0R, 5.0R, 20.0R}
        Dim whiskerLow As Double
        Dim whiskerHigh As Double
        SF.ResolveTukeyWhiskers(whiskerValues, 2.0R, 5.0R, whiskerLow, whiskerHigh)
        Assert.AreEqual(1.0R, whiskerLow, 0.0R)
        Assert.AreEqual(5.0R, whiskerHigh, 0.0R)
    End Sub

End Class
