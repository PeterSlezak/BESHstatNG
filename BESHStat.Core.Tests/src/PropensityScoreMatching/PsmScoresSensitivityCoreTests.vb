Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.CausalInference

<TestClass>
Public Class PsmScoresSensitivityCoreTests

    Private Const Tolerance As Double = 0.0000000001R

    Private Shared Function FourRowInput() As PsmInputData
        Return New PsmInputData With {
            .Treatment = New Double() {1.0R, 0.0R, 1.0R, 0.0R},
            .Outcome = New Double() {8.0R, 3.0R, 7.0R, 4.0R},
            .Covariates = New Double(,) {{1.0R}, {2.0R}, {3.0R}, {4.0R}},
            .CovariateNames = New String() {"X"},
            .SuppliedPropensityScores = New Double() {0.8R, 0.25R, 0.6R, 0.4R}
        }
    End Function

    Private Shared Function WeightOptions(estimand As PsmEstimand) As PsmOptions
        Return New PsmOptions With {
            .ScoreMethod = PsmScoreMethod.Supplied,
            .Estimand = estimand,
            .NormalizeWeightsToSampleSize = False
        }
    End Function

    Private Shared Function TwentyFourRowInput() As PsmInputData
        Dim treatment(23) As Double
        Dim covariates(23, 1) As Double
        For i As Integer = 0 To 23
            treatment(i) = If(i Mod 5 = 0 OrElse i Mod 5 = 2, 1.0R, 0.0R)
            covariates(i, 0) = 1.0R + 0.35R * i
            covariates(i, 1) = Convert.ToDouble((i * 7) Mod 11) / 3.0R
        Next
        Return New PsmInputData With {
            .Treatment = treatment,
            .Covariates = covariates,
            .CovariateNames = New String() {"Trend", "Cyclic"}
        }
    End Function

    Private Shared Function MatchedInput() As PsmInputData
        Return New PsmInputData With {
            .Treatment = New Double() {1, 0, 1, 0, 1, 0, 1, 0},
            .Outcome = New Double() {3, 1, 4, 2, 5, 3, 6, 4},
            .Covariates = New Double(,) {{1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}}
        }
    End Function

    Private Shared Function FourPairs() As List(Of PsmMatchLink)
        Return New List(Of PsmMatchLink) From {
            New PsmMatchLink With {.SetId = 1, .TreatedRowIndex = 0, .ControlRowIndex = 1},
            New PsmMatchLink With {.SetId = 2, .TreatedRowIndex = 2, .ControlRowIndex = 3},
            New PsmMatchLink With {.SetId = 3, .TreatedRowIndex = 4, .ControlRowIndex = 5},
            New PsmMatchLink With {.SetId = 4, .TreatedRowIndex = 6, .ControlRowIndex = 7}
        }
    End Function

    <TestMethod>
    <TestCategory("PSM-Scores")>
    Public Sub SuppliedScores_PreserveOrderAndReturnFiniteLogits()
        Dim result = PsmPropensityEstimator.Estimate(FourRowInput(), WeightOptions(PsmEstimand.ATT))
        Assert.AreEqual(PsmScoreMethod.Supplied, result.Method)
        Assert.IsTrue(result.Converged)
        Assert.AreEqual(0, result.Iterations)
        Assert.AreEqual(4, result.Scores.Length)
        Assert.AreEqual(0.8R, result.Scores(0), Tolerance)
        Assert.AreEqual(0.4R, result.Scores(3), Tolerance)
        Assert.AreEqual(Math.Log(4.0R), result.LinearPredictor(0), Tolerance)
        Assert.IsTrue(Double.IsNaN(result.LogLikelihood))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Scores")>
    Public Sub SuppliedScores_ValidateInputAndRequiredScoreVector()
        Dim input = FourRowInput()
        input.SuppliedPropensityScores = Nothing
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmPropensityEstimator.Estimate(input, WeightOptions(PsmEstimand.ATT)))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() PsmPropensityEstimator.Estimate(Nothing, WeightOptions(PsmEstimand.ATT)))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() PsmPropensityEstimator.Estimate(FourRowInput(), Nothing))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Scores")>
    Public Sub LogisticEstimation_UsesPortableGlmAndReturnsAlignedPredictions()
        Dim input = TwentyFourRowInput()
        Dim options As New PsmOptions With {
            .ScoreMethod = PsmScoreMethod.LogisticRegression,
            .LogisticRidgePenalty = 0.000001R,
            .LogisticMaxIterations = 150
        }
        Dim result = PsmPropensityEstimator.Estimate(input, options)
        Assert.AreEqual(PsmScoreMethod.LogisticRegression, result.Method)
        Assert.AreEqual(input.RowCount, result.Scores.Length)
        Assert.AreEqual(input.RowCount, result.LinearPredictor.Length)
        Assert.AreEqual(3, result.Coefficients.Length)
        Assert.AreEqual(3, result.VariableNames.Length)
        Assert.AreEqual("Intercept", result.VariableNames(0))
        Assert.AreEqual("Trend", result.VariableNames(1))
        Assert.AreEqual("Cyclic", result.VariableNames(2))
        For i As Integer = 0 To result.Scores.Length - 1
            Assert.IsTrue(result.Scores(i) > 0 AndAlso result.Scores(i) < 1, "Scores must be probabilities.")
            Assert.IsFalse(Double.IsNaN(result.LinearPredictor(i)) OrElse Double.IsInfinity(result.LinearPredictor(i)))
        Next
    End Sub

    <TestMethod>
    <TestCategory("PSM-Weights")>
    Public Sub AteWeights_AreInverseProbabilityWeights()
        Dim actual = PsmWeightEngine.ComputeBalancingWeights(FourRowInput(), New Double() {0.8, 0.25, 0.6, 0.4}, WeightOptions(PsmEstimand.ATE))
        Assert.AreEqual(1.25R, actual(0), Tolerance)
        Assert.AreEqual(4.0R / 3.0R, actual(1), Tolerance)
        Assert.AreEqual(5.0R / 3.0R, actual(2), Tolerance)
        Assert.AreEqual(5.0R / 3.0R, actual(3), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Weights")>
    Public Sub AttWeights_KeepTreatedAtOneAndReweightControls()
        Dim actual = PsmWeightEngine.ComputeBalancingWeights(FourRowInput(), New Double() {0.8, 0.25, 0.6, 0.4}, WeightOptions(PsmEstimand.ATT))
        Assert.AreEqual(1.0R, actual(0), Tolerance)
        Assert.AreEqual(1.0R / 3.0R, actual(1), Tolerance)
        Assert.AreEqual(1.0R, actual(2), Tolerance)
        Assert.AreEqual(2.0R / 3.0R, actual(3), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Weights")>
    Public Sub AtcWeights_KeepControlsAtOneAndReweightTreated()
        Dim actual = PsmWeightEngine.ComputeBalancingWeights(FourRowInput(), New Double() {0.8, 0.25, 0.6, 0.4}, WeightOptions(PsmEstimand.ATC))
        Assert.AreEqual(0.25R, actual(0), Tolerance)
        Assert.AreEqual(1.0R, actual(1), Tolerance)
        Assert.AreEqual(2.0R / 3.0R, actual(2), Tolerance)
        Assert.AreEqual(1.0R, actual(3), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Weights")>
    Public Sub AtoWeights_AreOverlapWeights()
        Dim actual = PsmWeightEngine.ComputeBalancingWeights(FourRowInput(), New Double() {0.8, 0.25, 0.6, 0.4}, WeightOptions(PsmEstimand.ATO))
        Assert.AreEqual(0.2R, actual(0), Tolerance)
        Assert.AreEqual(0.25R, actual(1), Tolerance)
        Assert.AreEqual(0.4R, actual(2), Tolerance)
        Assert.AreEqual(0.4R, actual(3), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Weights")>
    Public Sub Trimming_ZeroesWeightsOutsidePropensityRange()
        Dim options = WeightOptions(PsmEstimand.ATT)
        options.TrimPropensityLower = 0.3R
        options.TrimPropensityUpper = 0.7R
        Dim actual = PsmWeightEngine.ComputeBalancingWeights(FourRowInput(), New Double() {0.8, 0.25, 0.6, 0.4}, options)
        Assert.AreEqual(0.0R, actual(0))
        Assert.AreEqual(0.0R, actual(1))
        Assert.AreEqual(1.0R, actual(2))
        Assert.AreEqual(2.0R / 3.0R, actual(3), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Weights")>
    Public Sub GroupNormalization_PreservesGroupSampleSizes()
        Dim options = WeightOptions(PsmEstimand.ATT)
        options.NormalizeWeightsToSampleSize = True
        Dim weights = PsmWeightEngine.ComputeBalancingWeights(FourRowInput(), New Double() {0.8, 0.25, 0.6, 0.4}, options)
        Assert.AreEqual(2.0R, weights(0) + weights(2), Tolerance)
        Assert.AreEqual(2.0R, weights(1) + weights(3), Tolerance)
        Assert.AreEqual(2.0R / 3.0R, weights(1), Tolerance)
        Assert.AreEqual(4.0R / 3.0R, weights(3), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Weights")>
    Public Sub WeightEngine_RejectsMissingOrMismatchedScores()
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmWeightEngine.ComputeBalancingWeights(FourRowInput(), New Double() {0.4}, WeightOptions(PsmEstimand.ATT)))
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmWeightEngine.ComputeBalancingWeights(FourRowInput(), Nothing, WeightOptions(PsmEstimand.ATT)))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() PsmWeightEngine.ComputeBalancingWeights(Nothing, New Double() {0.1}, WeightOptions(PsmEstimand.ATT)))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Sensitivity")>
    Public Sub RosenbaumSignTest_GammaOneEqualsExactTwoSidedBinomial()
        Dim result = PsmSensitivityAnalysis.RosenbaumMatchedPairs(MatchedInput(), FourPairs(), 1.0R, 0.1R, 0.05R)
        Assert.AreEqual(4, result.InformativePairs)
        Assert.AreEqual(4, result.PositiveDifferences)
        Assert.AreEqual(0, result.NegativeDifferences)
        Assert.AreEqual(0, result.TiedDifferences)
        Assert.AreEqual(2.0R, result.MedianDifference, Tolerance)
        Assert.AreEqual(1, result.Rows.Count)
        Assert.AreEqual(0.125R, result.Rows(0).LowerBoundPValue, Tolerance)
        Assert.AreEqual(0.125R, result.Rows(0).UpperBoundPValue, Tolerance)
        Assert.AreEqual(1.0R, result.TippingPointGamma, Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Sensitivity")>
    Public Sub RosenbaumGrid_IsFiniteAndContainsRequestedEndpoint()
        Dim result = PsmSensitivityAnalysis.RosenbaumMatchedPairs(MatchedInput(), FourPairs(), 2.0R, 0.5R, 0.05R, PsmSensitivityAlternative.Greater)
        Assert.AreEqual(PsmSensitivityAlternative.Greater, result.Alternative)
        Assert.AreEqual(3, result.Rows.Count)
        Assert.AreEqual(1.0R, result.Rows(0).Gamma, Tolerance)
        Assert.AreEqual(1.5R, result.Rows(1).Gamma, Tolerance)
        Assert.AreEqual(2.0R, result.Rows(2).Gamma, Tolerance)
        For Each entry In result.Rows
            Assert.IsTrue(entry.LowerBoundPValue >= 0.0R AndAlso entry.LowerBoundPValue <= 1.0R)
            Assert.IsTrue(entry.UpperBoundPValue >= 0.0R AndAlso entry.UpperBoundPValue <= 1.0R)
        Next
    End Sub

    <TestMethod>
    <TestCategory("PSM-Sensitivity")>
    Public Sub RosenbaumNoValidPairs_ReturnsWarningNotException()
        Dim links = FourPairs()
        links(1).SetId = 1
        links(2).SetId = 1
        links(3).SetId = 1
        Dim result = PsmSensitivityAnalysis.RosenbaumMatchedPairs(MatchedInput(), links)
        Assert.AreEqual(0, result.Rows.Count)
        Assert.IsTrue(result.Warnings.Count > 0)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Sensitivity")>
    Public Sub RosenbaumAllTied_ReturnsNoInformativeComparisons()
        Dim input = MatchedInput()
        input.Outcome = New Double() {1, 1, 2, 2, 3, 3, 4, 4}
        Dim result = PsmSensitivityAnalysis.RosenbaumMatchedPairs(input, FourPairs())
        Assert.AreEqual(4, result.TiedDifferences)
        Assert.AreEqual(0, result.InformativePairs)
        Assert.AreEqual(0, result.Rows.Count)
        Assert.IsTrue(result.Warnings.Count > 0)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Sensitivity")>
    Public Sub RosenbaumInvalidArguments_AreValidated()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() PsmSensitivityAnalysis.RosenbaumMatchedPairs(Nothing, FourPairs()))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() PsmSensitivityAnalysis.RosenbaumMatchedPairs(MatchedInput(), Nothing))
        Dim input = MatchedInput()
        input.Outcome = Nothing
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmSensitivityAnalysis.RosenbaumMatchedPairs(input, FourPairs()))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Sensitivity")>
    Public Sub EffectSensitivity_ReturnsNormalApproximationAndConfidenceCrossing()
        Dim effect As New PsmEffectResult With {
            .Method = "ATT matching", .Estimand = PsmEstimand.ATT,
            .Estimate = 2.0R, .StandardError = 1.0R,
            .LowerConfidenceLimit = -0.1R, .UpperConfidenceLimit = 4.1R
        }
        Dim actual = PsmSensitivityAnalysis.SummarizeEffectSensitivity(effect)
        Assert.AreEqual(2.0R, actual.ZStatistic, Tolerance)
        Assert.IsTrue(actual.TwoSidedPValue > 0.04R AndAlso actual.TwoSidedPValue < 0.05R)
        Assert.IsTrue(actual.EffectCrossesZero)
        Assert.AreEqual("ATT matching", actual.Method)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Sensitivity")>
    Public Sub EffectSensitivity_HandlesUnavailableStandardError()
        Dim effect As New PsmEffectResult With {.Estimate = 2.0R, .StandardError = 0.0R}
        Dim result = PsmSensitivityAnalysis.SummarizeEffectSensitivity(effect)
        Assert.IsTrue(Double.IsNaN(result.ZStatistic))
        Assert.IsTrue(Double.IsNaN(result.TwoSidedPValue))
        Assert.IsFalse(result.EffectCrossesZero)
        Assert.IsFalse(String.IsNullOrWhiteSpace(result.Warning))
        Assert.IsNull(PsmSensitivityAnalysis.SummarizeEffectSensitivity(Nothing))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Sensitivity")>
    Public Sub SensitivityTableBuilders_ProduceExcelNeutralObjectArrays()
        Dim result = PsmSensitivityAnalysis.RosenbaumMatchedPairs(MatchedInput(), FourPairs(), 1.0R)
        Dim detail As Object(,) = PsmSensitivityTables.RosenbaumTable(result)
        Assert.AreEqual(2, detail.GetLength(0))
        Assert.AreEqual(8, detail.GetLength(1))
        Assert.AreEqual("Gamma", detail(0, 0))
        Assert.AreEqual(1.0R, Convert.ToDouble(detail(1, 0)), Tolerance)
        Dim summary As Object(,) = PsmSensitivityTables.RosenbaumSummaryTable(result)
        Assert.AreEqual(9, summary.GetLength(0))
        Assert.AreEqual(2, summary.GetLength(1))
        Assert.AreEqual("Metric", summary(0, 0))
        Dim effect As New PsmEffectSensitivitySummary With {.Method = "IPW", .TwoSidedPValue = 0.03R}
        Dim effectTable As Object(,) = PsmSensitivityTables.EffectSensitivitySummaryTable(effect)
        Assert.AreEqual(2, effectTable.GetLength(0))
        Assert.AreEqual(9, effectTable.GetLength(1))
        Assert.AreEqual("p-value", effectTable(0, 5))
        Assert.AreEqual(0.03R, Convert.ToDouble(effectTable(1, 5)), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Sensitivity")>
    Public Sub EmptySensitivityTables_UseExistingCoreResultContract()
        Assert.IsNotNull(PsmSensitivityTables.RosenbaumTable(Nothing))
        Assert.IsNotNull(PsmSensitivityTables.RosenbaumSummaryTable(Nothing))
        Assert.IsNotNull(PsmSensitivityTables.EffectSensitivitySummaryTable(Nothing))
    End Sub

End Class
