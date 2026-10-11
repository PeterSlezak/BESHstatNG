Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.CausalInference

<TestClass>
Public Class PsmMatchingCoreTests

    Private Const Tolerance As Double = 0.00000001R

    Private Shared Function InputSix(Optional exactGroups As Boolean = False) As PsmInputData
        Return New PsmInputData With {
            .Treatment = New Double() {1, 0, 1, 0, 1, 0},
            .Outcome = New Double() {10, 2, 20, 4, 30, 6},
            .Covariates = New Double(,) {{0}, {0}, {1}, {1}, {2}, {2}},
            .CovariateNames = New String() {"X"},
            .ExactGroupLabels = If(exactGroups, New String() {"A", "A", "B", "B", "C", "C"}, Nothing)
        }
    End Function

    Private Shared Function ScoresSix() As Double()
        Return New Double() {0.2R, 0.21R, 0.5R, 0.51R, 0.8R, 0.81R}
    End Function

    Private Shared Function Observations(input As PsmInputData, options As PsmOptions) As List(Of PsmObservation)
        Return PsmMatchingEngine.BuildObservations(input, New PsmScoreModelResult With {.Scores = ScoresSix()}, options)
    End Function

    <TestMethod>
    <TestCategory("PSM-Matching")>
    Public Sub ObservationConstruction_PreservesRowIdsAndFallbackLogits()
        Dim input = InputSix(True)
        Dim obs = Observations(input, New PsmOptions())
        Assert.AreEqual(6, obs.Count)
        Assert.AreEqual("1", obs(0).Id)
        Assert.AreEqual("A", obs(1).ExactGroupLabel)
        Assert.AreEqual(10.0R, obs(0).Outcome, Tolerance)
        Assert.AreEqual(1, obs(2).Covariates.Length)
        Assert.AreEqual(Math.Log(0.2R / 0.8R), obs(0).LogitPropensityScore, Tolerance)
        Assert.IsTrue(obs(0).Treated)
        Assert.IsFalse(obs(1).Treated)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Matching")>
    Public Sub ObservationConstruction_RespectsTrimmingAndOverlap()
        Dim input = InputSix()
        Dim options As New PsmOptions With {.TrimPropensityLower = 0.25R, .TrimPropensityUpper = 0.75R}
        Dim trimmed = Observations(input, options)
        Assert.AreEqual(2, trimmed.Where(Function(o) o.IncludedByTrimming).Count())
        Assert.IsTrue(trimmed(2).IncludedByTrimming)
        Assert.IsTrue(trimmed(3).IncludedByTrimming)
        options = New PsmOptions With {.CommonSupport = PsmCommonSupportMode.DropOutsideOverlapRange}
        Dim overlap = Observations(input, options)
        Assert.IsFalse(overlap(0).IncludedByCommonSupport)
        Assert.IsTrue(overlap(2).IncludedByCommonSupport)
        Assert.IsFalse(overlap(5).IncludedByCommonSupport)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Matching")>
    Public Sub NearestNeighbor_ATT_UsesExactGroupsAndUnitWeights()
        Dim input = InputSix(True)
        Dim options As New PsmOptions With {.Estimand = PsmEstimand.ATT, .MatchingRatio = 1}
        Dim matches = PsmMatchingEngine.MatchNearestNeighbors(input, Observations(input, options), options)
        Assert.AreEqual(3, matches.Count)
        For Each link In matches
            Assert.AreEqual(link.TreatedRowIndex + 1, link.ControlRowIndex)
            Assert.AreEqual(1.0R, link.MatchedWeight, Tolerance)
            Assert.AreEqual(0.01R, link.PropensityDistance, Tolerance)
        Next
        Dim weights = PsmMatchingEngine.ComputeMatchingWeights(input, matches, options)
        Assert.IsTrue(weights.All(Function(w) Math.Abs(w - 1.0R) < Tolerance))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Matching")>
    Public Sub NearestNeighbor_ATC_ReversesFocalAndPreservesTreatedControlIndices()
        Dim input = InputSix(True)
        Dim options As New PsmOptions With {.Estimand = PsmEstimand.ATC, .MatchingRatio = 1}
        Dim matches = PsmMatchingEngine.MatchNearestNeighbors(input, Observations(input, options), options)
        Assert.AreEqual(3, matches.Count)
        For Each link In matches
            Assert.AreEqual(link.ControlRowIndex, link.FocalRowIndex)
            Assert.IsTrue(link.TreatedRowIndex Mod 2 = 0)
        Next
        Assert.AreEqual(6.0R, PsmMatchingEngine.ComputeMatchingWeights(input, matches, options).Sum(), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Matching")>
    Public Sub NearestNeighbor_RawCaliperExcludesOutOfRangePairs()
        Dim input = InputSix(True)
        Dim options As New PsmOptions With {
            .Estimand = PsmEstimand.ATT,
            .CaliperScale = PsmCaliperScale.RawPropensityScore,
            .Caliper = 0.005R
        }
        Dim matches = PsmMatchingEngine.MatchNearestNeighbors(input, Observations(input, options), options)
        Assert.AreEqual(0, matches.Count)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Matching")>
    Public Sub NearestNeighbor_ReplacementAndOneToManyPreserveTotalFocalWeight()
        Dim input = InputSix()
        Dim options As New PsmOptions With {.MatchingRatio = 2, .WithReplacement = True}
        Dim matches = PsmMatchingEngine.MatchNearestNeighbors(input, Observations(input, options), options)
        Assert.AreEqual(6, matches.Count)
        Assert.AreEqual(3, matches.Select(Function(m) m.SetId).Distinct().Count())
        Assert.IsTrue(matches.All(Function(m) Math.Abs(m.MatchedWeight - 0.5R) < Tolerance))
        Dim weights = PsmMatchingEngine.ComputeMatchingWeights(input, matches, options)
        Assert.AreEqual(3.0R, weights.Where(Function(w, idx) idx Mod 2 = 0).Sum(), Tolerance)
        Assert.AreEqual(3.0R, weights.Where(Function(w, idx) idx Mod 2 = 1).Sum(), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Matching")>
    Public Sub NearestNeighbor_MahalanobisUsesCoreMatrixAndProducesFiniteDistance()
        Dim input = InputSix(True)
        Dim options As New PsmOptions With {.DistanceMetric = PsmDistanceMetric.Mahalanobis}
        Dim matches = PsmMatchingEngine.MatchNearestNeighbors(input, Observations(input, options), options)
        Assert.AreEqual(3, matches.Count)
        Assert.IsTrue(matches.All(Function(m) Not Double.IsNaN(m.MahalanobisDistance)))
        Assert.IsTrue(matches.All(Function(m) Math.Abs(m.MahalanobisDistance) < Tolerance))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Matching")>
    Public Sub NearestNeighbor_RejectsUnsupportedEstimands()
        Dim input = InputSix()
        Dim options As New PsmOptions With {.Estimand = PsmEstimand.ATE}
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmMatchingEngine.MatchNearestNeighbors(input, Observations(input, options), options))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Subclassification")>
    Public Sub Subclassification_SplitsScoreDistributionAndAveragesEffects()
        Dim input = InputSix()
        Dim options As New PsmOptions With {.SubclassificationStrata = 3, .Estimand = PsmEstimand.ATE}
        Dim rows = PsmSubclassificationEngine.BuildSubclasses(input, ScoresSix(), options)
        Assert.AreEqual(3, rows.Count)
        Assert.IsTrue(rows.All(Function(r) r.TreatedN = 1 AndAlso r.ControlN = 1))
        Assert.AreEqual(8.0R, rows(0).Effect, Tolerance)
        Assert.AreEqual(16.0R, rows(1).Effect, Tolerance)
        Assert.AreEqual(24.0R, rows(2).Effect, Tolerance)
        Dim effect = PsmSubclassificationEngine.EstimateEffect(rows, options)
        Assert.IsNotNull(effect)
        Assert.AreEqual("Subclassification", effect.Method)
        Assert.AreEqual(PsmEstimand.ATE, effect.Estimand)
        Assert.AreEqual(16.0R, effect.Estimate, Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Subclassification")>
    Public Sub Subclassification_RequiresOutcomesAndAlignedScores()
        Dim input = InputSix()
        Dim options As New PsmOptions With {.SubclassificationStrata = 3}
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmSubclassificationEngine.BuildSubclasses(input, New Double() {0.1R}, options))
        input.Outcome = Nothing
        Assert.AreEqual(0, PsmSubclassificationEngine.BuildSubclasses(input, ScoresSix(), options).Count)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Advanced")>
    Public Sub OptimalPairs_AttRespectsExactGroupsAndUsesUniqueControls()
        Dim input = InputSix(True)
        Dim options As New PsmOptions With {.Estimand = PsmEstimand.ATT}
        Dim matches = PsmAdvancedMatching.MatchOptimalPairs(input, Observations(input, options), options)
        Assert.AreEqual(3, matches.Count)
        Assert.AreEqual(3, matches.Select(Function(m) m.ControlRowIndex).Distinct().Count())
        Assert.IsTrue(matches.All(Function(m) m.TreatedRowIndex Mod 2 = 0 AndAlso m.ControlRowIndex Mod 2 = 1))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Advanced")>
    Public Sub OptimalPairs_MahalanobisRetainsFiniteDistances()
        Dim input = InputSix(True)
        Dim options As New PsmOptions With {.Estimand = PsmEstimand.ATC, .DistanceMetric = PsmDistanceMetric.Mahalanobis}
        Dim matches = PsmAdvancedMatching.MatchOptimalPairs(input, Observations(input, options), options)
        Assert.AreEqual(3, matches.Count)
        Assert.IsTrue(matches.All(Function(m) Not Double.IsNaN(m.MahalanobisDistance)))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Advanced")>
    Public Sub OptimalPairs_RejectsUnsupportedEstimands()
        Dim input = InputSix()
        Dim options As New PsmOptions With {.Estimand = PsmEstimand.ATO}
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmAdvancedMatching.MatchOptimalPairs(input, Observations(input, options), options))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Advanced")>
    Public Sub Cem_ExplicitCutPointsRetainPairedStrataAndReturnWeights()
        Dim input = InputSix()
        Dim spec As New PsmCoarseningSpec With {
            .CutPoints = New Double()() {New Double() {0.5R, 1.5R}},
            .NormalizeWeightsToSampleSize = False,
            .Estimand = PsmEstimand.ATT
        }
        Dim cem = PsmAdvancedMatching.BuildCoarsenedExactWeights(input, Nothing, spec)
        Assert.AreEqual(3, cem.Strata.Count)
        Assert.AreEqual(6, cem.RetainedRows)
        Assert.AreEqual(0, cem.DroppedRows)
        Assert.IsTrue(cem.Strata.All(Function(s) s.Retained AndAlso s.TreatedN = 1 AndAlso s.ControlN = 1))
        Assert.IsTrue(cem.Weights.All(Function(w) Math.Abs(w - 1.0R) < Tolerance))
        Dim strataTable = PsmAdvancedMatchingTables.CoarsenedExactStrataTable(cem)
        Dim weightsTable = PsmAdvancedMatchingTables.CoarsenedExactWeightsTable(input, cem)
        Assert.AreEqual(4, strataTable.GetLength(0))
        Assert.AreEqual(8, strataTable.GetLength(1))
        Assert.AreEqual(7, weightsTable.GetLength(0))
        Assert.AreEqual(6, weightsTable.GetLength(1))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Advanced")>
    Public Sub Cem_PropensityScoreBinsRequireAlignedScores()
        Dim input = InputSix()
        Dim spec As New PsmCoarseningSpec With {.IncludePropensityScore = True}
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmAdvancedMatching.BuildCoarsenedExactWeights(input, Nothing, spec))
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmAdvancedMatching.BuildCoarsenedExactWeights(input, New Double() {0.2R}, spec))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Advanced")>
    Public Sub Cem_EffectCallsNowPortableWeightedOutcomeAnalysis()
        Dim input = InputSix()
        Dim spec As New PsmCoarseningSpec With {
            .CutPoints = New Double()() {New Double() {0.5R, 1.5R}},
            .NormalizeWeightsToSampleSize = False
        }
        Dim cem = PsmAdvancedMatching.BuildCoarsenedExactWeights(input, Nothing, spec)
        Dim effect = PsmAdvancedMatching.EstimateCoarsenedExactEffect(input, cem, PsmEstimand.ATT)
        Assert.IsNotNull(effect)
        Assert.AreEqual("Coarsened Exact Matching", effect.Method)
        Assert.AreEqual(PsmOutcomeType.Continuous, effect.OutcomeType)
        Assert.AreEqual(16.0R, effect.Estimate, Tolerance)
        Assert.AreEqual(PsmEstimand.ATT, effect.Estimand)
        Assert.AreEqual(3.0R, effect.EffectiveTreatedN, Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Effects")>
    Public Sub OutcomeAnalysis_MatchedAndWeightedMethodsStayCallable()
        Dim input = InputSix(True)
        Dim options As New PsmOptions With {.Estimand = PsmEstimand.ATT}
        Dim matches = PsmMatchingEngine.MatchNearestNeighbors(input, Observations(input, options), options)
        Dim matched = PsmOutcomeAnalysis.EstimateMatchedEffect(input, matches, options)
        Dim weighted = PsmOutcomeAnalysis.EstimateWeightedEffect(input, New Double() {1, 1, 1, 1, 1, 1}, options)
        Assert.IsNotNull(matched)
        Assert.IsNotNull(weighted)
        Assert.AreEqual(16.0R, matched.Estimate, Tolerance)
        Assert.AreEqual(16.0R, weighted.Estimate, Tolerance)
        Assert.AreEqual(PsmOutcomeType.Binary, PsmOutcomeAnalysis.InferOutcomeType(New Double() {0, 1, 0, 1}))
        Assert.AreEqual(PsmOutcomeType.Continuous, PsmOutcomeAnalysis.InferOutcomeType(input.Outcome))
    End Sub
End Class
