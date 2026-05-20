Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.CausalInference

<TestClass()>
Public Class PsmCapabilityContractTests

    Private Const TOL As Double = 0.000000001

    Private Shared Function MakeContractInput() As PsmInputData
        Return New PsmInputData With {
            .Ids = New String() {"T1", "C1", "T2", "C2", "T3", "C3", "T4", "C4", "T5", "C5", "T6", "C6"},
            .Treatment = New Double() {1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0},
            .Outcome = New Double() {10, 7, 9, 6, 11, 8, 8, 6, 12, 8, 7, 5},
            .Covariates = New Double(,) {
                {0.00, 0.10},
                {0.05, 0.20},
                {0.20, 0.25},
                {0.25, 0.30},
                {0.40, 0.45},
                {0.45, 0.50},
                {0.60, 0.65},
                {0.65, 0.70},
                {0.80, 0.85},
                {0.85, 0.90},
                {1.00, 1.05},
                {1.05, 1.10}
            },
            .CovariateNames = New String() {"x1", "x2"},
            .SuppliedPropensityScores = New Double() {0.15, 0.14, 0.25, 0.24, 0.40, 0.39, 0.58, 0.57, 0.76, 0.75, 0.90, 0.89},
            .ExactGroupLabels = New String() {"", "", "", "", "", "", "", "", "", "", "", ""}
        }
    End Function

    Private Shared Function BaseOptions(Optional estimand As PsmEstimand = PsmEstimand.ATT) As PsmOptions
        Return New PsmOptions With {
            .ScoreMethod = PsmScoreMethod.Supplied,
            .MatchingMethod = PsmMatchingMethod.NearestNeighbor,
            .Estimand = estimand,
            .DistanceMetric = PsmDistanceMetric.PropensityScore,
            .MatchingRatio = 1,
            .WithReplacement = False,
            .CaliperScale = PsmCaliperScale.None,
            .Caliper = Double.NaN,
            .CommonSupport = PsmCommonSupportMode.None,
            .TrimPropensityLower = 0.0,
            .TrimPropensityUpper = 1.0,
            .MatchingOrder = PsmMatchingOrder.PropensityDescending
        }
    End Function

    Private Shared Sub AssertEstimands(runMethod As PsmBackendRunMethod, ParamArray expected() As PsmEstimand)
        Dim actual As PsmEstimand() = PsmMethodCapabilities.SupportedEstimands(runMethod)
        CollectionAssert.AreEqual(expected.Select(Function(e) e.ToString()).ToArray(),
                                  actual.Select(Function(e) e.ToString()).ToArray(),
                                  "Unexpected supported estimands for " & runMethod.ToString())
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub CapabilityContract_ExposesOnlyImplementedEstimands()
        AssertEstimands(PsmBackendRunMethod.StandardNearestNeighbor, PsmEstimand.ATT, PsmEstimand.ATC)
        AssertEstimands(PsmBackendRunMethod.OptimalPairMatching, PsmEstimand.ATT, PsmEstimand.ATC)
        AssertEstimands(PsmBackendRunMethod.StandardSubclassification, PsmEstimand.ATT, PsmEstimand.ATC, PsmEstimand.ATE, PsmEstimand.ATO)
        AssertEstimands(PsmBackendRunMethod.WeightingOnly, PsmEstimand.ATT, PsmEstimand.ATC, PsmEstimand.ATE, PsmEstimand.ATO)
        AssertEstimands(PsmBackendRunMethod.CoarsenedExactMatching, PsmEstimand.ATT, PsmEstimand.ATC, PsmEstimand.ATE, PsmEstimand.ATO)
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub CapabilityContract_RejectsUnsupportedRunMethodEstimandCombinations()
        Dim nnAte As New PsmComprehensiveFitOptions With {
            .StandardOptions = BaseOptions(PsmEstimand.ATE),
            .RunMethod = PsmBackendRunMethod.StandardNearestNeighbor
        }
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmMethodCapabilities.ValidateFitOptions(nnAte))

        Dim optAto As New PsmComprehensiveFitOptions With {
            .StandardOptions = BaseOptions(PsmEstimand.ATO),
            .RunMethod = PsmBackendRunMethod.OptimalPairMatching
        }
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmMethodCapabilities.ValidateFitOptions(optAto))
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub StandaloneBackend_NearestNeighborAteThrowsInsteadOfWarningFallback()
        Dim input As PsmInputData = MakeContractInput()
        Dim options As PsmOptions = BaseOptions(PsmEstimand.ATE)
        options.MatchingMethod = PsmMatchingMethod.NearestNeighbor

        Assert.ThrowsException(Of ArgumentException)(Sub() PsmBackend.Fit(input, options))
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub CapabilityContract_RejectsOptimalPairReplacementAndRatio()
        Dim options As PsmOptions = BaseOptions(PsmEstimand.ATT)
        options.WithReplacement = True

        Dim fitOptions As New PsmComprehensiveFitOptions With {
            .StandardOptions = options,
            .RunMethod = PsmBackendRunMethod.OptimalPairMatching
        }
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmMethodCapabilities.ValidateFitOptions(fitOptions))

        options.WithReplacement = False
        options.MatchingRatio = 2
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmMethodCapabilities.ValidateFitOptions(fitOptions))
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub MahalanobisWithinPropensityCaliper_RequiresActualCaliper()
        Dim options As PsmOptions = BaseOptions(PsmEstimand.ATT)
        options.DistanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper
        options.CaliperScale = PsmCaliperScale.None
        options.Caliper = Double.NaN

        Assert.ThrowsException(Of ArgumentException)(Sub() options.Validate())

        options.CaliperScale = PsmCaliperScale.StandardizedLogitPropensityScore
        options.Caliper = 0.2
        options.Validate()
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub CemDefaultCovariateBins_AffectsCoarseningWhenNoPerVariableBinsAreProvided()
        Dim input As PsmInputData = MakeContractInput()
        Dim scores As Double() = input.SuppliedPropensityScores

        Dim twoBins As PsmCoarsenedExactResult = PsmAdvancedMatching.BuildCoarsenedExactWeights(
            input,
            scores,
            New PsmCoarseningSpec With {
                .DefaultCovariateBins = 2,
                .IncludePropensityScore = False,
                .Estimand = PsmEstimand.ATT
            })

        Dim fiveBins As PsmCoarsenedExactResult = PsmAdvancedMatching.BuildCoarsenedExactWeights(
            input,
            scores,
            New PsmCoarseningSpec With {
                .DefaultCovariateBins = 5,
                .IncludePropensityScore = False,
                .Estimand = PsmEstimand.ATT
            })

        Assert.IsTrue(twoBins.Strata.Count > 0, "Two-bin CEM should produce strata.")
        Assert.IsTrue(fiveBins.Strata.Count > 0, "Five-bin CEM should produce strata.")
        Assert.AreNotEqual(twoBins.Strata.Count, fiveBins.Strata.Count, "DefaultCovariateBins should change the generated CEM strata when BinCounts are not supplied.")
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub AipwAto_ReturnsFiniteOverlapTargetedEstimate()
        Dim input As PsmInputData = MakeContractInput()
        Dim scores As Double() = input.SuppliedPropensityScores
        Dim options As PsmOptions = BaseOptions(PsmEstimand.ATO)
        options.MatchingMethod = PsmMatchingMethod.None

        Dim aipw As PsmDoublyRobustResult = PsmDoublyRobustEstimator.EstimateAipw(input, scores, options)

        Assert.IsNotNull(aipw)
        Assert.IsNotNull(aipw.Effect)
        Assert.IsFalse(Double.IsNaN(aipw.Effect.Estimate) OrElse Double.IsInfinity(aipw.Effect.Estimate), "AIPW ATO estimate should be finite.")
        Assert.AreEqual(PsmEstimand.ATO, aipw.Effect.Estimand)
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub ComprehensiveBackend_AllGuiExposedRunMethodsProduceExpectedPrimaryOutput()
        Dim input As PsmInputData = MakeContractInput()

        Dim nn As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, New PsmComprehensiveFitOptions With {
            .StandardOptions = BaseOptions(PsmEstimand.ATT),
            .RunMethod = PsmBackendRunMethod.StandardNearestNeighbor
        })
        Assert.IsTrue(nn.Result.Matches.Count > 0, "Nearest-neighbor run should produce matches.")

        Dim subclassOptions As PsmOptions = BaseOptions(PsmEstimand.ATE)
        subclassOptions.MatchingMethod = PsmMatchingMethod.Subclassification
        subclassOptions.CaliperScale = PsmCaliperScale.None
        subclassOptions.SubclassificationStrata = 3
        Dim subclass As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, New PsmComprehensiveFitOptions With {
            .StandardOptions = subclassOptions,
            .RunMethod = PsmBackendRunMethod.StandardSubclassification
        })
        Assert.IsNotNull(subclass.Result.SubclassificationEffect, "Subclassification run should produce a subclassification effect.")

        Dim weightingOptions As PsmOptions = BaseOptions(PsmEstimand.ATO)
        weightingOptions.MatchingMethod = PsmMatchingMethod.None
        Dim weighting As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, New PsmComprehensiveFitOptions With {
            .StandardOptions = weightingOptions,
            .RunMethod = PsmBackendRunMethod.WeightingOnly
        })
        Assert.IsNotNull(weighting.Result.WeightedEffect, "Weighting run should produce a weighted effect.")

        Dim optimalOptions As PsmOptions = BaseOptions(PsmEstimand.ATC)
        optimalOptions.MatchingRatio = 1
        optimalOptions.WithReplacement = False
        Dim optimal As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, New PsmComprehensiveFitOptions With {
            .StandardOptions = optimalOptions,
            .RunMethod = PsmBackendRunMethod.OptimalPairMatching
        })
        Assert.IsTrue(optimal.Result.Matches.Count > 0, "Optimal pair matching should produce matches.")

        Dim cemOptions As PsmOptions = BaseOptions(PsmEstimand.ATE)
        cemOptions.MatchingMethod = PsmMatchingMethod.None
        Dim cem As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, New PsmComprehensiveFitOptions With {
            .StandardOptions = cemOptions,
            .RunMethod = PsmBackendRunMethod.CoarsenedExactMatching,
            .CoarseningSpec = New PsmCoarseningSpec With {
                .DefaultCovariateBins = 2,
                .Estimand = PsmEstimand.ATE
            }
        })
        Assert.IsNotNull(cem.CoarsenedExactResult, "CEM run should return CEM details.")
        Assert.IsNotNull(cem.Result.WeightedEffect, "CEM run should produce a weighted effect.")
    End Sub

End Class
