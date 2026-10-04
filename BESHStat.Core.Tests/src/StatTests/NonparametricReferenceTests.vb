Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Reference values were checked independently against standard published formulas
' and SciPy implementations where the same statistic/convention is available.

<TestClass()>
Public Class NonparametricReferenceTests

    Private Const Tol As Double = 0.000000001

    Private Shared Sub AssertClose(expected As Double, actual As Double, Optional tolerance As Double = Tol)
        Assert.IsFalse(Double.IsNaN(actual), "Actual value is NaN.")
        Assert.IsFalse(Double.IsInfinity(actual), "Actual value is infinite.")
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

    <TestMethod()>
    Public Sub MannWhitney_MatchesReference()
        Dim x() As Double = {1, 2, 3, 4, 5}
        Dim y() As Double = {6, 7, 8, 9, 10}
        Dim test As New BESHStatNG.nonparametric.MannWhitney(New Double()() {x, y}, "X", "Y")
        Dim result As BESHStatNG.TestResult = test.Compute()

        AssertClose(0.0, result.TestStatistics1)
        AssertClose(25.0, result.TestStatistics2)
        AssertClose(0.012185780355344813, result.Pvalue)
        Assert.IsTrue(result.bExactAvailable)
        AssertClose(0.0079365079365079361, result.PvalueExact, 0.000000000001)
    End Sub

    <TestMethod()>
    Public Sub WilcoxonSignedRank_MatchesReference()
        Dim data(,) As Double = {{2, 1}, {4, 2}, {6, 3}, {8, 4}, {10, 5}}
        Dim test As New BESHStatNG.nonparametric.WilcoxonTest(data, "Before", "After")
        Dim result As BESHStatNG.TestResult = test.Compute()

        AssertClose(1.8877596148970779, result.TestStatistics1)
        AssertClose(0.059058229090536707, result.Pvalue)
        Assert.IsTrue(result.bExactAvailable)
        AssertClose(0.0625, result.PvalueExact, 0.000000000001)
    End Sub

    <TestMethod()>
    Public Sub SpearmanRho_MatchesReference()
        Dim x() As Double = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}
        Dim y() As Double = {2, 1, 4, 3, 6, 5, 8, 7, 10, 9, 12, 11}
        Dim test As New BESHStatNG.nonparametric.SpearmanRho(x, y, "X", "Y")
        Dim result As BESHStatNG.TestResult = test.Compute()

        AssertClose(0.95804195804195813, result.TestStatistics1, 0.000000000001)
        AssertClose(0.00000095435818268384024, result.Pvalue)
    End Sub

    <TestMethod()>
    Public Sub KendallTauB_WithTies_MatchesReference()
        Dim x() As Double = {1, 2, 2, 3, 4, 4}
        Dim y() As Double = {1, 1, 2, 3, 3, 4}
        Dim test As New BESHStatNG.nonparametric.KendallsTau(x, y, "X", "Y")
        Dim result As BESHStatNG.TestResult = test.compute()

        AssertClose(0.84615384615384615, result.TestStatistics1, 0.000000000001)
        Assert.IsTrue(result.bExactAvailable)
        AssertClose(0.027777777777777776, result.PvalueExact, 0.000000000001)
    End Sub

    <TestMethod()>
    Public Sub KruskalWallis_WithTies_MatchesReference()
        Dim groups As Double()() = {
            New Double() {1, 2, 2, 3},
            New Double() {2, 3, 4, 4},
            New Double() {4, 5, 5, 6}
        }
        Dim test As New BESHStatNG.nonparametric.KruskallWalis(groups, New String() {"A", "B", "C"})
        Dim result As BESHStatNG.TestResult = test.compute()

        AssertClose(7.875, result.TestStatistics1, 0.000000000001)
        AssertClose(8.1603260869565215, result.TestStatistics2, 0.000000000001)
        AssertClose(0.019496896108597995, result.Pvalue)
        AssertClose(0.016904709225411534, result.Pvalue2)
    End Sub

    <TestMethod()>
    Public Sub Friedman_MatchesReference()
        Dim data(,) As Double = {
            {1, 2, 3},
            {2, 1, 4},
            {3, 4, 2},
            {4, 3, 5},
            {5, 6, 1}
        }
        Dim test As New BESHStatNG.nonparametric.Friedman(data, New String() {"A", "B", "C"})
        Dim result As BESHStatNG.TestResult = test.compute()

        AssertClose(0.4, result.TestStatistics1, 0.000000000001)
        AssertClose(0.16666666666666666, result.TestStatistics2, 0.000000000001)
        AssertClose(0.81873075307798182, result.Pvalue)
        AssertClose(0.84934656, result.Pvalue2)
    End Sub

    <TestMethod()>
    Public Sub TheilSen_MatchesReference()
        Dim data(,) As Double = {{3, 1}, {5, 2}, {7, 3}, {9, 4}, {11, 5}, {13, 6}}
        Dim test As New BESHStatNG.nonparametric.TheilSen(data, New String() {"Y", "X"})
        Dim result As BESHStatNG.nonparametric.TheilSenResults = test.compute(0.05)

        AssertClose(2.0, result.MedianSlope, 0.000000000001)
        AssertClose(2.0, result.LLslope, 0.000000000001)
        AssertClose(2.0, result.ULslope, 0.000000000001)
        AssertClose(1.0, result.Intercept, 0.000000000001)
        Assert.AreEqual(0, result.lNoTies)
        AssertClose(0.05, result.alpha, 0.000000000001)

        Dim plotData As BESHStatNG.nonparametric.TheilSenPlotData = test.GetPlotData()
        Assert.AreEqual(6, plotData.XValues.Length)
        Assert.AreEqual(6, plotData.YValues.Length)
        Assert.AreEqual("X", plotData.XName)
        Assert.AreEqual("Y", plotData.YName)
        AssertClose(1.0, plotData.MinX, 0.000000000001)
        AssertClose(6.0, plotData.MaxX, 0.000000000001)
        AssertClose(3.0, plotData.FittedYAtMinX, 0.000000000001)
        AssertClose(13.0, plotData.FittedYAtMaxX, 0.000000000001)
    End Sub

    <TestMethod()>
    Public Sub SkillingsMack_CompleteBlocks_MatchesFriedmanReference()
        Dim data(,) As Double = {
            {1, 2, 3},
            {2, 1, 4},
            {3, 4, 2},
            {4, 3, 5},
            {5, 6, 1}
        }

        Dim result As BESHStatNG.TestResult = BESHStatNG.nonparametric.Nonparametric.SkillingsMack(data)

        AssertClose(0.4, result.TestStatistics1, 0.000000000001)
        AssertClose(0.81873075307798182, result.Pvalue)
    End Sub

End Class
