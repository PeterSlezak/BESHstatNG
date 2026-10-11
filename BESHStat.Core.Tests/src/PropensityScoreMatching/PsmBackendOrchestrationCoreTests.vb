Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.CausalInference

<TestClass()>
Public Class PsmBackendOrchestrationCoreTests

    Private Const TOL As Double = 0.000000001

    Private Shared Function MakeContractInput() As PsmInputData
        Return New PsmInputData With {
            .Ids = New String() {"T1", "C1", "T2", "C2", "T3", "C3", "T4", "C4", "T5", "C5", "T6", "C6"},
            .Treatment = New Double() {1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0},
            .Outcome = New Double() {10, 7, 9, 6, 11, 8, 8, 6, 12, 8, 7, 5},
            .Covariates = New Double(,) {
                {0.00, 0.1},
                {0.05, 0.2},
                {0.2, 0.25},
                {0.25, 0.3},
                {0.4, 0.45},
                {0.45, 0.5},
                {0.6, 0.65},
                {0.65, 0.7},
                {0.8, 0.85},
                {0.85, 0.9},
                {1.0, 1.05},
                {1.05, 1.1}
            },
            .CovariateNames = New String() {"x1", "x2"},
            .SuppliedPropensityScores = New Double() {0.15, 0.14, 0.25, 0.24, 0.4, 0.39, 0.58, 0.57, 0.76, 0.75, 0.9, 0.89},
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

    <TestMethod>
    <TestCategory("PSM-Core-Backend")>
    Public Sub StandaloneBackend_SuppliedScoresProducesWeightsAndBalance()
        Dim input As PsmInputData = MakeContractInput()
        Dim options As PsmOptions = BaseOptions(PsmEstimand.ATE)
        options.MatchingMethod = PsmMatchingMethod.None
        Dim fit As PsmResult = PsmBackend.Fit(input, options)
        Assert.IsNotNull(fit.ScoreModel)
        Assert.AreEqual(input.RowCount, fit.ScoreModel.Scores.Length)
        Assert.AreEqual(input.RowCount, fit.BalancingWeights.Length)
        Assert.IsNotNull(fit.WeightedEffect)
        Assert.IsTrue(fit.Balance.Any(Function(b) b.Sample = PsmBalanceSample.Before))
        Assert.IsTrue(fit.Balance.Any(Function(b) b.Sample = PsmBalanceSample.AfterWeighting))
        Assert.AreEqual(input.RowCount, fit.SampleSize.TotalRows)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Backend")>
    Public Sub ComprehensiveBackend_WeightingDiagnosticsAndWarningsAreNeutral()
        Dim input As PsmInputData = MakeContractInput()
        Dim options As PsmOptions = BaseOptions(PsmEstimand.ATE)
        options.MatchingMethod = PsmMatchingMethod.None
        Dim request As New PsmComprehensiveFitOptions With {
            .StandardOptions = options,
            .RunMethod = PsmBackendRunMethod.WeightingOnly,
            .IncludeDoublyRobustEstimate = False,
            .OverlapBinCount = 6
        }
        Dim fit As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, request)
        Assert.IsNotNull(fit.Result)
        Assert.IsNotNull(fit.Result.WeightedEffect)
        Assert.IsNotNull(fit.OverlapDiagnostics)
        Assert.IsTrue(fit.WeightDiagnostics.Count > 0)
        Assert.IsTrue(fit.LovePlotRows.Count > 0)
        Assert.AreEqual(PsmBackendRunMethod.WeightingOnly, fit.RunMethod)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Backend")>
    Public Sub CaliperTuning_RejectsInvalidCandidatesAndReturnsWarning()
        Dim fit As PsmCaliperTuningResult = PsmCaliperTuner.TuneNearestNeighbor(
            MakeContractInput(), BaseOptions(), New Double() {Double.NaN, -0.1R},
            PsmCaliperScale.RawPropensityScore, 0.5R)
        Assert.AreEqual(2, fit.Rows.Count)
        Assert.IsTrue(fit.Rows.All(Function(r) Not r.Accepted))
        Assert.IsNull(fit.SelectedRow)
        Assert.IsTrue(fit.Warnings.Count > 0)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Backend")>
    Public Sub CaliperTuning_ValidCandidateReturnsSelectedOptions()
        Dim fit As PsmCaliperTuningResult = PsmCaliperTuner.TuneNearestNeighbor(
            MakeContractInput(), BaseOptions(), New Double() {1.0R},
            PsmCaliperScale.RawPropensityScore, 0.5R)
        Assert.AreEqual(1, fit.Rows.Count)
        Assert.IsTrue(fit.Rows(0).Accepted)
        Assert.IsNotNull(fit.SelectedOptions)
        Assert.AreEqual(1.0R, fit.SelectedOptions.Caliper)
        Assert.AreEqual(PsmCaliperScale.RawPropensityScore, fit.SelectedOptions.CaliperScale)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Backend")>
    Public Sub CloneOptions_PreservesSettingsWithoutSharingInstance()
        Dim original As PsmOptions = BaseOptions()
        original.Caliper = 0.25R
        original.MatchingRatio = 2
        Dim cloned As PsmOptions = PsmComprehensiveBackend.CloneOptions(original)
        Assert.AreNotSame(original, cloned)
        Assert.AreEqual(0.25R, cloned.Caliper)
        Assert.AreEqual(2, cloned.MatchingRatio)
        cloned.MatchingRatio = 1
        Assert.AreEqual(2, original.MatchingRatio)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Backend")>
    Public Sub MethodCapabilities_ExposeCorrectControlApplicability()
        Assert.IsTrue(PsmMethodCapabilities.UsesMatchingControls(PsmBackendRunMethod.StandardNearestNeighbor))
        Assert.IsTrue(PsmMethodCapabilities.UsesOptimalPairOptions(PsmBackendRunMethod.OptimalPairMatching))
        Assert.IsTrue(PsmMethodCapabilities.UsesSubclassificationControls(PsmBackendRunMethod.StandardSubclassification))
        Assert.IsTrue(PsmMethodCapabilities.UsesCemControls(PsmBackendRunMethod.CoarsenedExactMatching))
        Assert.IsFalse(PsmMethodCapabilities.UsesCaliper(PsmBackendRunMethod.WeightingOnly))
        Assert.IsTrue(PsmMethodCapabilities.SupportsDoublyRobust(PsmBackendRunMethod.WeightingOnly, PsmEstimand.ATO))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Bootstrap")>
    Public Sub Bootstrap_RejectsInsufficientReplications()
        Dim options As PsmOptions = BaseOptions(PsmEstimand.ATE)
        options.MatchingMethod = PsmMatchingMethod.None
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() PsmBootstrapRunner.BootstrapEffect(MakeContractInput(), options, PsmBootstrapTarget.WeightedEffect, 1))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Bootstrap")>
    Public Sub Bootstrap_WeightedEffectIsDeterministicWithFixedSeed()
        Dim input As PsmInputData = MakeContractInput()
        Dim options As PsmOptions = BaseOptions(PsmEstimand.ATE)
        options.MatchingMethod = PsmMatchingMethod.None
        Dim first As PsmBootstrapResult = PsmBootstrapRunner.BootstrapEffect(
            input, options, PsmBootstrapTarget.WeightedEffect, 24, 9123)
        Dim second As PsmBootstrapResult = PsmBootstrapRunner.BootstrapEffect(
            input, options, PsmBootstrapTarget.WeightedEffect, 24, 9123)
        Assert.AreEqual(24, first.ReplicationsRequested)
        Assert.IsTrue(first.ReplicationsUsed > 0)
        Assert.AreEqual(first.ReplicationsUsed, first.ReplicateEstimates.Length)
        Assert.AreEqual(first.ReplicationsUsed, second.ReplicationsUsed)
        Assert.AreEqual(first.EstimateOriginal, second.EstimateOriginal)
        CollectionAssert.AreEqual(first.ReplicateEstimates, second.ReplicateEstimates)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Tables")>
    Public Sub BackendTables_ProduceScoreBalanceMatchAndEffectSections()
        Dim input As PsmInputData = MakeContractInput()
        Dim fit As PsmResult = PsmBackend.Fit(input, BaseOptions())
        Dim score As Object(,) = PsmBackendTables.ScoreModelTable(fit)
        Dim balance As Object(,) = PsmBackendTables.BalanceTable(fit)
        Dim matches As Object(,) = PsmBackendTables.MatchesTable(fit, input)
        Dim effects As Object(,) = PsmBackendTables.EffectTable(fit)
        Assert.AreEqual("Term", Convert.ToString(score(0, 0)))
        Assert.AreEqual("Sample", Convert.ToString(balance(0, 0)))
        Assert.AreEqual("Set", Convert.ToString(matches(0, 0)))
        Assert.AreEqual("Method", Convert.ToString(effects(0, 0)))
        Assert.AreEqual(fit.Matches.Count + 1, matches.GetLength(0))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Tables")>
    Public Sub FrontEndTables_PreserveImportedSourceRowIds()
        Dim input As PsmInputData = MakeContractInput()
        Dim options As PsmOptions = BaseOptions(PsmEstimand.ATE)
        options.MatchingMethod = PsmMatchingMethod.None
        Dim fit As PsmResult = PsmBackend.Fit(input, options)
        Dim rowIds As Integer() = Enumerable.Range(100, input.RowCount).ToArray()
        Dim audit As Object(,) = PsmFrontEndTables.RowLevelAuditTable(input, fit, rowIds)
        Assert.AreEqual(input.RowCount + 1, audit.GetLength(0))
        Assert.AreEqual("Source Row", Convert.ToString(audit(0, 0)))
        Assert.AreEqual(100, Convert.ToInt32(audit(1, 0)))
        Assert.AreEqual(101, Convert.ToInt32(audit(2, 0)))
        Assert.AreEqual(input.GetId(0), Convert.ToString(audit(1, 2)))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Tables")>
    Public Sub FormattedTables_PreservePValueColumnMetadata()
        Dim matrix As Object(,) = New Object(,) {{"Method", "p-value"}, {"Example", 0.003R}}
        Dim result As Global.BESHStatNG.ResultTable = PsmFormattedResultTables.MatrixToResultTable("Test", matrix)
        Assert.AreEqual(1, result.HeadersTopCount)
        Assert.IsTrue(result.PvalColumns.Contains(2))
        Assert.IsTrue(result.TotalRows >= 3)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Tables")>
    Public Sub ComprehensiveTables_PreserveMethodAndWarningSummaries()
        Dim input As PsmInputData = MakeContractInput()
        Dim request As New PsmComprehensiveFitOptions With {
            .StandardOptions = BaseOptions(),
            .RunMethod = PsmBackendRunMethod.StandardNearestNeighbor,
            .IncludeDoublyRobustEstimate = False
        }
        Dim fit As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, request)
        Dim summary As Object(,) = PsmComprehensiveTables.RunSummaryTable(fit)
        Assert.AreEqual("Item", Convert.ToString(summary(0, 0)))
        Assert.IsTrue(summary.GetLength(0) > 2)
        fit.Warnings.Add("deliberate test warning")
        Dim warnings As Object(,) = PsmComprehensiveTables.WarningsTable(fit)
        Assert.AreEqual("Warning", Convert.ToString(warnings(0, 0)))
        Assert.AreEqual("deliberate test warning", Convert.ToString(warnings(warnings.GetUpperBound(0), 0)))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Core-Tables")>
    Public Sub FormattedSections_UsePortableResultTableContracts()
        Dim input As PsmInputData = MakeContractInput()
        Dim request As New PsmComprehensiveFitOptions With {
            .StandardOptions = BaseOptions(),
            .RunMethod = PsmBackendRunMethod.StandardNearestNeighbor,
            .IncludeDoublyRobustEstimate = False
        }
        Dim fit As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, request)
        Dim sections As List(Of Global.BESHStatNG.ResultTable) = PsmFormattedResultTables.GeneralResultTables(
            fit, request, New Object(,) {{"Metric", "Value"}, {"Rows", 12}})
        Assert.AreEqual(8, sections.Count)
        Assert.IsTrue(sections.All(Function(t) t.TotalRows > 0 AndAlso t.TotalCols > 0))
        Dim audit As List(Of Global.BESHStatNG.ResultTable) = PsmFormattedResultTables.RowAuditTables(input, fit)
        Assert.AreEqual(1, audit.Count)
        Assert.IsTrue(audit(0).TotalRows >= input.RowCount)
    End Sub

End Class
