Option Strict On
Option Explicit On

Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class CategoricalLogitCommonTests

    <TestMethod>
    Public Sub LogSumExpBaselineZero_IsStableForLargePredictors()
        Dim eta() As Double = {1000.0R, 999.0R}
        Dim actual As Double = CategoricalLogitUtils.LogSumExpBaselineZero(eta)
        Dim expected As Double = 1000.0R + Math.Log(1.0R + Math.Exp(-1.0R) + Math.Exp(-1000.0R))

        Assert.AreEqual(expected, actual, 0.000000000001R)
        Assert.IsFalse(Double.IsInfinity(actual))
    End Sub

    <TestMethod>
    Public Sub MatTimesVec_MultipliesSquareMatrix()
        Dim matrix(,) As Double = {{2.0R, 1.0R}, {-1.0R, 3.0R}}
        Dim vector() As Double = {4.0R, 5.0R}

        CollectionAssert.AreEqual(New Double() {13.0R, 11.0R}, CategoricalLogitUtils.MatTimesVec(matrix, vector))
    End Sub

    <TestMethod>
    Public Sub MaxAbs_And_ArgMax_ReturnExpectedValues()
        Assert.AreEqual(7.0R, CategoricalLogitUtils.MaxAbs(New Double() {-7.0R, 2.0R, 6.0R}), 0.0R)
        Assert.AreEqual(1, CategoricalLogitUtils.ArgMax(New Double() {1.0R, 4.0R, 4.0R}, True))
    End Sub

    <TestMethod>
    Public Sub SharedDtos_AreAvailableFromCore()
        Dim counts(1, 1) As Double
        Dim table As New ClassificationCrosstab With {
            .Categories = New Integer() {1, 2},
            .counts = counts,
            .OverallAccuracy = 0.75R
        }
        Dim residuals As New MultinomialResiduals With {
            .Categories = New Integer() {1, 2}
        }

        Assert.AreEqual(ReferenceCategory.Last, CType(1, ReferenceCategory))
        Assert.AreEqual(ResidualColumnType.PearsonResidual, CType(4, ResidualColumnType))
        Assert.AreEqual(0.75R, table.OverallAccuracy, 0.0R)
        CollectionAssert.AreEqual(New Integer() {1, 2}, residuals.Categories)
    End Sub
End Class
