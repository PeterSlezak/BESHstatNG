Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression

<TestClass>
Public Class MixedModelResultsPostEstimationCoreTests

    Private Const Tol As Double = 0.000000001R

    <TestMethod>
    Public Sub PostEstimationHelpers_AverageProfilesAndDirectedDifferences()
        Dim values() As Double = {2.0R, Double.NaN, 1.0R, Double.PositiveInfinity, 2.0R + 0.0000000001R, -1.0R}
        Dim unique() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(values)
        CollectionAssert.AreEqual(New Double() {-1.0R, 1.0R, 2.0R}, unique)

        Dim x(,) As Double = {
            {1.0R, 8.0R, 0.0R, 0.0R},
            {1.0R, 8.0R, 1.0R, 8.0R},
            {1.0R, 10.0R, 0.0R, 0.0R},
            {1.0R, 10.0R, 1.0R, 10.0R},
            {1.0R, 11.0R, 1.0R, 11.0R},
            {1.0R, 12.0R, 1.0R, 12.0R}
        }
        Dim visit() As Double = {1.0R, 1.0R, 2.0R, 2.0R, 2.0R, 3.0R}
        Dim group() As Double = {0.0R, 1.0R, 0.0R, 1.0R, 1.0R, 1.0R}

        Dim row() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(
            x, visit, group, 2.0R, 1.0R, Nothing)
        Assert.IsNotNull(row)
        Assert.AreEqual(1.0R, row(0), Tol)
        Assert.AreEqual(10.5R, row(1), Tol)
        Assert.AreEqual(1.0R, row(2), Tol)
        Assert.AreEqual(10.5R, row(3), Tol)
        Assert.AreEqual(2, MixedModelPostEstimation.CountProfileRows(visit, group, 2.0R, 1.0R, Nothing))

        Dim diff() As Double = MixedModelPostEstimation.MakeDirectedDifference(
            {1.0R, 2.0R, 3.0R}, {0.5R, 1.0R, 1.5R},
            "Treatment - control", "Treatment - control", "Control - treatment")
        Assert.AreEqual(0.5R, diff(0), Tol)
        Assert.AreEqual(1.0R, diff(1), Tol)
        Assert.AreEqual(1.5R, diff(2), Tol)
    End Sub

    <TestMethod>
    Public Sub Result_SatterthwaiteAndWrapResults_ArePortable()
        Dim workspace As New MixedModelInferenceWorkspace With {
            .P = 1,
            .K = 1,
            .VarBeta = New Double(,) {{4.0R}},
            .ThetaCovariance = New Double(,) {{1.0R}}
        }
        Dim gradient(0, 0, 0) As Double
        gradient(0, 0, 0) = 0.2R
        workspace.VarBetaGradient = gradient

        Dim result As New MixedModelResult With {
            .Converged = True,
            .P = 1,
            .Nobs = 12,
            .NoSubjects = 6,
            .FixedEffectNames = New String() {"Intercept"},
            .Beta = New Double() {5.0R},
            .VarBeta = workspace.VarBeta,
            .BetaSE = New Double() {2.0R},
            .BetaZ = New Double() {2.5R},
            .BetaP = New Double() {0.02R},
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.Satterthwaite,
            .BetaDF = New Double() {800.0R},
            .BetaStatistic = New Double() {2.5R},
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)",
            .InferenceWorkspace = workspace,
            .ThetaRNames = New String() {"Residual variance"},
            .ThetaR = New Double() {1.0R},
            .Theta = New Double() {1.0R}
        }

        Dim df As Double = Double.NaN
        Assert.IsTrue(result.TrySatterthwaiteDFForLinearCombination(New Double() {1.0R}, df))
        Assert.AreEqual(800.0R, df, 0.000001R)

        Dim tables As List(Of ResultTable) = result.wrapResults(
            includeOptimizerTrace:=False,
            includeKenwardRogerTermTests:=False,
            includeDiagnostics:=False)
        Assert.IsNotNull(tables)
        Assert.IsTrue(tables.Count >= 4)
        Assert.IsTrue(tables(0).PvalColumns.Count > 0, "Fixed-effects table should mark its p-value column.")
    End Sub

    <TestMethod>
    Public Sub HypothesisBuilder_GroupsTermsAndBuildsSelectors()
        Dim names() As String = {
            "(Intercept)", "visit=2", "visit=3", "treatment_active",
            "treatment_active:visit=2", "treatment_active:visit=3", "age"
        }
        Dim hypotheses As List(Of MixedModelMultiDfHypothesis) =
            MixedModelHypothesisBuilder.BuildTermHypotheses(names, includeIntercept:=False)

        Assert.AreEqual(4, hypotheses.Count)
        Assert.AreEqual("visit", hypotheses(0).Label)
        Assert.AreEqual(2, hypotheses(0).L.GetLength(0))
        Assert.AreEqual(1.0R, hypotheses(0).L(0, 1), 0.0R)
        Assert.AreEqual(1.0R, hypotheses(0).L(1, 2), 0.0R)
        Assert.AreEqual("treatment_active:visit",
                        MixedModelHypothesisBuilder.NormalizeTermKey("visit=2:treatment_active"))
    End Sub

    <TestMethod>
    Public Sub ReferenceGrid_EqualWeightingBuildsExpectedRows()
        Dim result As MixedModelResult = BuildSyntheticReferenceGridResult()
        Dim treatment() As Double = {0, 0, 1, 1, 1, 0, 1, 0}
        Dim site() As Double = {0, 1, 0, 1, 2, 2, 2, 0}
        Dim age() As Double = {7, 8, 9, 10, 11, 12, 13, 14}

        Dim spec As New MixedModelReferenceGridSpec With {
            .FixedEffectNames = result.FixedEffectNames,
            .Weighting = MixedModelReferenceGridWeighting.EqualCells,
            .MultiplicityAdjustment = MixedModelMultiplicityAdjustment.None,
            .Alpha = 0.05R
        }
        spec.AddByFactor("treatment_active", New Double() {0, 1}, treatment)
        spec.AddMarginalFactor("site_code", New Double() {0, 1, 2}, site)
        spec.AddCovariateValue("age_centered_8", age, 10.0R)

        Dim rows As List(Of MixedModelReferenceGridRow) = MixedModelReferenceGridService.BuildReferenceGridRows(spec)
        Assert.AreEqual(2, rows.Count)
        AssertVector(New Double() {1.0R, 0.0R, 1.0R, 10.0R, 0.0R}, rows(0).L)
        AssertVector(New Double() {1.0R, 1.0R, 1.0R, 10.0R, 10.0R}, rows(1).L)
        Assert.AreEqual(16.0R, MixedModelPostEstimation.LinearEstimate(rows(0).L, result.Beta), Tol)
        Assert.AreEqual(19.0R, MixedModelPostEstimation.LinearEstimate(rows(1).L, result.Beta), Tol)
    End Sub

    <TestMethod>
    Public Sub ReferenceGrid_MultiplicityAdjustmentsMatchExpectedValues()
        Dim p() As Double = {0.01R, 0.02R, 0.2R}
        Dim bonf() As Double = MixedModelReferenceGridService.AdjustPValues(p, MixedModelMultiplicityAdjustment.Bonferroni)
        Dim holm() As Double = MixedModelReferenceGridService.AdjustPValues(p, MixedModelMultiplicityAdjustment.Holm)
        Dim sidak() As Double = MixedModelReferenceGridService.AdjustPValues(New Double() {0.05R}, MixedModelMultiplicityAdjustment.Sidak)

        Assert.AreEqual(0.03R, bonf(0), Tol)
        Assert.AreEqual(0.06R, bonf(1), Tol)
        Assert.AreEqual(0.6R, bonf(2), Tol)
        Assert.AreEqual(0.03R, holm(0), Tol)
        Assert.AreEqual(0.04R, holm(1), Tol)
        Assert.AreEqual(0.2R, holm(2), Tol)
        Assert.AreEqual(0.05R, sidak(0), Tol)
    End Sub

    Private Shared Function BuildSyntheticReferenceGridResult() As MixedModelResult
        Return New MixedModelResult With {
            .P = 5,
            .Beta = New Double() {10.0R, 2.0R, 1.0R, 0.5R, 0.1R},
            .VarBeta = New Double(,) {
                {0.04R, 0.0R, 0.0R, 0.0R, 0.0R},
                {0.0R, 0.04R, 0.0R, 0.0R, 0.0R},
                {0.0R, 0.0R, 0.04R, 0.0R, 0.0R},
                {0.0R, 0.0R, 0.0R, 0.01R, 0.0R},
                {0.0R, 0.0R, 0.0R, 0.0R, 0.01R}
            },
            .FixedEffectNames = New String() {"Intercept", "treatment_active", "site_code", "age_centered_8", "treatment_active:age_centered_8"},
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.ResidualDF,
            .BetaDF = New Double() {20, 20, 20, 20, 20},
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)"
        }
    End Function

    Private Shared Sub AssertVector(expected() As Double, actual() As Double)
        Assert.IsNotNull(actual)
        Assert.AreEqual(expected.Length, actual.Length)
        For i As Integer = 0 To expected.Length - 1
            Assert.AreEqual(expected(i), actual(i), Tol, "Mismatch at index " & i.ToString())
        Next
    End Sub

End Class
