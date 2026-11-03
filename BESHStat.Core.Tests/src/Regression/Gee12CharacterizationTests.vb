Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Gee12CoreCharacterizationTests
    Private Shared Function Model(Optional irregular As Boolean = False, Optional interleaved As Boolean = False) As GEE
        Dim data(23, 1) As Double, ids(23) As Integer, clusters(23) As Object, times(23) As Double
        For i = 0 To 23
            Dim g = If(interleaved, i Mod 8, i \ 3), visit = If(interleaved, i \ 8, i Mod 3)
            Dim x = g * 0.2 + visit * 0.3
            data(i, 0) = 2 + 0.4 * x + 0.1 * Math.Sin(g + 2 * visit)
            data(i, 1) = x : ids(i) = 99 - 3 * i : clusters(i) = "subject-" & g.ToString()
            times(i) = If(irregular, New Double() {0, 2, 9}(visit), CDbl(visit))
        Next
        Dim fit As New GEE(New Gaussian, New Identity, New Autoregressive)
        fit.data(data, clusters, ids, time:=times) : fit.setVarNames({"y", "x"}, "subject", strTimeName:="visit")
        fit.settingInputs(0.05, 50, 0.000001, False) : fit.bComputeResiduals = True
        Return fit
    End Function

    <TestMethod>
    Public Sub ExistingArOneUsesPositionRatherThanNumericVisitGaps()
        Dim regular = Model(), irregular = Model(True)
        regular.Fit(False) : irregular.Fit(False)
        CollectionAssert.AreEqual(regular.results.Coeffs_est, irregular.results.Coeffs_est)
        CollectionAssert.AreEqual(regular.results.Coeffs_SEs, irregular.results.Coeffs_SEs)
        CollectionAssert.AreEqual(regular.WorkingCorrelationMatrix.Cast(Of Double)().ToArray(), irregular.WorkingCorrelationMatrix.Cast(Of Double)().ToArray())
        CollectionAssert.AreEqual(New Double() {0, 2, 9}, irregular.UniqueTimesDict.Keys.OrderBy(Function(v) v).ToArray())
    End Sub

    <TestMethod>
    Public Sub ExistingPredictionAndObservedAxesDifferForInterleavedClusters()
        Dim fit = Model(interleaved:=True)
        fit.Fit(False)
        Dim predicted = fit.PredictedResponses, input = fit.LinearPredictors, k = 0
        For g = 0 To 7
            For visit = 0 To 2
                Assert.AreEqual(input(g + 8 * visit), predicted(k), 0.000000000001)
                k += 1
            Next
        Next
        Assert.AreEqual(24, fit.ObservedResponses.Length)
        Assert.AreEqual(25, fit.AllResiduals.GetLength(0))
    End Sub

    <TestMethod>
    Public Sub SuppliedStartsLeaveExistingQicIndependenceReferenceUnavailable()
        Dim fit = Model() : fit.startParams = {2.0, 0.4}
        Dim ex = Assert.ThrowsException(Of NullReferenceException)(Sub() fit.Fit(True))
        Assert.IsTrue(ex.StackTrace.Contains("EstimateQIC"))
        Dim nextFit = Model() : nextFit.Fit(False)
        Assert.IsNotNull(nextFit.wrapResults())
    End Sub
End Class
