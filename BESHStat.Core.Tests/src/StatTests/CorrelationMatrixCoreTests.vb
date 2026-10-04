Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.Matrix

<TestClass()>
Public Class CorrelationMatrixCoreTests

    <TestMethod()>
    Public Sub PearsonMatrix_PreservesTriangleContractAndLegacyPValueFormula()
        Dim data(,) As Double = {
            {1.0R, 2.0R},
            {2.0R, 1.0R},
            {3.0R, 4.0R},
            {4.0R, 3.0R},
            {5.0R, 6.0R},
            {6.0R, 5.0R}
        }

        Dim result(,) As Double = CorrelationMatrixCore.Compute(data, "r")
        Dim x() As Double = {1.0R, 2.0R, 3.0R, 4.0R, 5.0R, 6.0R}
        Dim y() As Double = {2.0R, 1.0R, 4.0R, 3.0R, 6.0R, 5.0R}
        Dim coefficient As Double = BESHStatNG.StatFunc.Correl(x, y)
        Dim statistic As Double = Math.Abs(coefficient * Math.Sqrt(data.GetLength(0) - 2) /
                                                (1.0R - Math.Sqrt(coefficient ^ 2)))
        Dim pValue As Double = BESHStatNG.distributions.T_2T(statistic, data.GetLength(0))

        Assert.AreEqual(1.0R, result(0, 0), 0.000000000001R)
        Assert.AreEqual(1.0R, result(1, 1), 0.000000000001R)
        Assert.AreEqual(coefficient, result(0, 1), 0.000000000001R)
        Assert.AreEqual(pValue, result(1, 0), 0.000000000001R)
    End Sub

    <TestMethod()>
    Public Sub SpearmanMatrix_MatchesDirectSpearmanCalculation()
        Dim x() As Double = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}
        Dim y() As Double = {2, 1, 4, 3, 6, 5, 8, 7, 10, 9, 12, 11}
        Dim data(x.Length - 1, 1) As Double
        For i As Integer = 0 To x.Length - 1
            data(i, 0) = x(i)
            data(i, 1) = y(i)
        Next

        Dim direct As New BESHStatNG.nonparametric.SpearmanRho(x, y, "X", "Y")
        Dim directResult As BESHStatNG.TestResult = direct.Compute()
        Dim result(,) As Double = CorrelationMatrixCore.Compute(data, "rho")

        Assert.AreEqual(1.0R, result(0, 0), 0.000000000001R)
        Assert.AreEqual(1.0R, result(1, 1), 0.000000000001R)
        Assert.AreEqual(directResult.TestStatistics1, result(0, 1), 0.000000000001R)
        Assert.AreEqual(directResult.Pvalue, result(1, 0), 0.000000000001R)
    End Sub

    <TestMethod()>
    Public Sub KendallMatrix_MatchesDirectKendallCalculation()
        Dim x() As Double = {1, 2, 2, 3, 4, 4}
        Dim y() As Double = {1, 1, 2, 3, 3, 4}
        Dim data(x.Length - 1, 1) As Double
        For i As Integer = 0 To x.Length - 1
            data(i, 0) = x(i)
            data(i, 1) = y(i)
        Next

        Dim direct As New BESHStatNG.nonparametric.KendallsTau(x, y, "X", "Y")
        Dim directResult As BESHStatNG.TestResult = direct.compute()
        Dim result(,) As Double = CorrelationMatrixCore.Compute(data, "tau")

        Assert.AreEqual(1.0R, result(0, 0), 0.000000000001R)
        Assert.AreEqual(1.0R, result(1, 1), 0.000000000001R)
        Assert.AreEqual(directResult.TestStatistics1, result(0, 1), 0.000000000001R)
        Assert.AreEqual(directResult.Pvalue, result(1, 0), 0.000000000001R)
    End Sub

End Class
