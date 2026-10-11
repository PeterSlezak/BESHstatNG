Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.CausalInference

<TestClass>
Public Class PsmDiagnosticsInferenceCoreTests
    Private Const Tolerance As Double = 0.000001R

    Private Shared Function ExampleInput() As PsmInputData
        Dim treatment(11) As Double
        Dim outcome(11) As Double
        Dim covariates(11, 0) As Double
        Dim scores(11) As Double
        For i As Integer = 0 To 11
            Dim treated As Boolean = (i Mod 2 = 0)
            treatment(i) = If(treated, 1.0R, 0.0R)
            covariates(i, 0) = Convert.ToDouble(i)
            outcome(i) = 2.0R + 0.5R * Convert.ToDouble(i) + 3.0R * treatment(i)
            scores(i) = If(treated, 0.35R + 0.035R * i, 0.25R + 0.04R * i)
        Next
        Return New PsmInputData With {
            .Treatment = treatment,
            .Outcome = outcome,
            .Covariates = covariates,
            .CovariateNames = New String() {"X"},
            .SuppliedPropensityScores = scores
        }
    End Function

    Private Shared Function Options(Optional estimand As PsmEstimand = PsmEstimand.ATE) As PsmOptions
        Return New PsmOptions With {
            .ScoreMethod = PsmScoreMethod.Supplied,
            .MatchingMethod = PsmMatchingMethod.None,
            .Estimand = estimand,
            .LogisticRidgePenalty = 0.000001R
        }
    End Function

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub BalanceDefaultsToUnitWeights_AndProducesCorrectMoments()
        Dim rows = PsmBalanceDiagnostics.ComputeBalance(ExampleInput(), Nothing, PsmBalanceSample.Before, Options())
        Assert.AreEqual(1, rows.Count)
        Dim r = rows(0)
        Assert.AreEqual("X", r.VariableName)
        Assert.AreEqual(PsmBalanceSample.Before, r.Sample)
        Assert.AreEqual(5.0R, r.TreatedMean, Tolerance)
        Assert.AreEqual(6.0R, r.ControlMean, Tolerance)
        Assert.AreEqual(14.0R, r.TreatedVariance, Tolerance)
        Assert.AreEqual(14.0R, r.ControlVariance, Tolerance)
        Assert.AreEqual(1.0R, r.VarianceRatio, Tolerance)
        Assert.AreEqual(-1.0R / Math.Sqrt(14.0R), r.StandardizedMeanDifference, Tolerance)
        Assert.AreEqual(6.0R, r.TreatedN, Tolerance)
        Assert.AreEqual(6.0R, r.ControlN, Tolerance)
        StringAssert.Contains(r.Flag, "SMD")
    End Sub

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub BalanceRejectsInvalidWeightLength()
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmBalanceDiagnostics.ComputeBalance(ExampleInput(), New Double() {1}, PsmBalanceSample.Before, Options()))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() PsmBalanceDiagnostics.ComputeBalance(Nothing, Nothing, PsmBalanceSample.Before, Options()))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub BalanceWithZeroWeights_ExcludesObservations()
        Dim weights(11) As Double
        weights(0) = 1.0R
        weights(1) = 1.0R
        Dim rows = PsmBalanceDiagnostics.ComputeBalance(ExampleInput(), weights, PsmBalanceSample.AfterMatching, Options())
        Dim row = rows(0)
        Assert.AreEqual(1.0R, row.TreatedN, Tolerance)
        Assert.AreEqual(1.0R, row.ControlN, Tolerance)
        Assert.AreEqual(0.0R, row.TreatedMean, Tolerance)
        Assert.AreEqual(1.0R, row.ControlMean, Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub WeightDiagnostics_GroupCountsAndEffectiveSize()
        Dim weights = Enumerable.Repeat(1.0R, 12).ToArray()
        Dim rows = PsmAdvancedDiagnostics.ComputeWeightDiagnostics(ExampleInput(), weights)
        Assert.AreEqual(3, rows.Count)
        Assert.AreEqual("All", rows(0).GroupName)
        Assert.AreEqual("Treated", rows(1).GroupName)
        Assert.AreEqual("Control", rows(2).GroupName)
        Assert.AreEqual(12.0R, rows(0).EffectiveSampleSize, Tolerance)
        Assert.AreEqual(6.0R, rows(1).SumWeights, Tolerance)
        Assert.AreEqual(6, rows(2).NonZeroN)
        Assert.AreEqual("OK", rows(0).Flag)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub WeightDiagnostics_ReportsExtremeAndNoPositiveWeights()
        Dim weights(11) As Double
        weights(0) = 25.0R
        Dim rows = PsmAdvancedDiagnostics.ComputeWeightDiagnostics(ExampleInput(), weights, "Weighted", 10.0R)
        Assert.AreEqual(1, rows(0).ExtremeWeightN)
        StringAssert.Contains(rows(0).Flag, "Extreme weights")
        Assert.AreEqual("No non-zero weights", rows(2).Flag)
        Assert.AreEqual(0.0R, rows(2).EffectiveSampleSize, Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub WeightDiagnostics_RejectsMismatchedWeights()
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmAdvancedDiagnostics.ComputeWeightDiagnostics(ExampleInput(), New Double() {1}))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub OverlapSummaryAndBins_PreserveTotals()
        Dim input = ExampleInput()
        Dim result = PsmAdvancedDiagnostics.ComputeOverlapDiagnostics(input, input.SuppliedPropensityScores, Nothing, 5)
        Assert.AreEqual(2, result.Summary.Count)
        Assert.AreEqual(5, result.Bins.Count)
        Assert.AreEqual(6, result.Summary(0).N)
        Assert.AreEqual(6, result.Summary(1).N)
        Assert.AreEqual(6, result.Bins.Sum(Function(r) r.TreatedN))
        Assert.AreEqual(6, result.Bins.Sum(Function(r) r.ControlN))
        Assert.AreEqual(6.0R, result.Bins.Sum(Function(r) r.TreatedWeight), Tolerance)
        Assert.AreEqual(6.0R, result.Bins.Sum(Function(r) r.ControlWeight), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub OverlapDetectsDisjointScoreRanges()
        Dim input = ExampleInput()
        Dim scores(11) As Double
        For i As Integer = 0 To 11
            scores(i) = If(i Mod 2 = 0, 0.85R, 0.15R)
        Next
        Dim result = PsmAdvancedDiagnostics.ComputeOverlapDiagnostics(input, scores, Nothing, 4)
        Assert.IsTrue(result.Warnings.Any(Function(w) w.Contains("do not overlap")))
        Assert.AreEqual(4, result.Bins.Count)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub OverlapRejectsMismatchedScoreOrWeightVectors()
        Dim input = ExampleInput()
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmAdvancedDiagnostics.ComputeOverlapDiagnostics(input, New Double() {0.5R}))
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmAdvancedDiagnostics.ComputeOverlapDiagnostics(input, input.SuppliedPropensityScores, New Double() {1.0R}))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Diagnostics")>
    Public Sub LovePlot_RanksMaximumImbalanceAndUsesConfiguredThreshold()
        Dim result As New PsmResult With {.Options = New PsmOptions With {.BalanceSmdThreshold = 0.15R}}
        result.Balance.Add(New PsmBalanceRow With {.VariableName = "X", .Sample = PsmBalanceSample.Before, .StandardizedMeanDifference = 0.5R})
        result.Balance.Add(New PsmBalanceRow With {.VariableName = "X", .Sample = PsmBalanceSample.AfterMatching, .StandardizedMeanDifference = 0.25R})
        result.Balance.Add(New PsmBalanceRow With {.VariableName = "Z", .Sample = PsmBalanceSample.AfterWeighting, .StandardizedMeanDifference = 0.05R})
        Dim rows = PsmAdvancedDiagnostics.BuildLovePlotRows(result)
        Assert.AreEqual(2, rows.Count)
        Assert.AreEqual("X", rows(0).VariableName)
        Assert.AreEqual(0.25R, rows(0).MaxAbsAfter, Tolerance)
        Assert.AreEqual(0.15R, rows(0).Threshold, Tolerance)
        Assert.AreEqual("Review", rows(0).Flag)
        Assert.AreEqual("OK", rows(1).Flag)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Inference")>
    Public Sub AipwAte_PerfectLinearOutcomeRecoversTreatmentDifference()
        Dim input = ExampleInput()
        Dim result = PsmDoublyRobustEstimator.EstimateAipw(input, input.SuppliedPropensityScores, Options(PsmEstimand.ATE))
        Assert.IsNotNull(result.Effect)
        Assert.AreEqual(3.0R, result.Effect.Estimate, 0.00001R)
        Assert.AreEqual(PsmEstimand.ATE, result.Effect.Estimand)
        Assert.AreEqual(12, result.IndividualContributions.Length)
        Assert.AreEqual(6, result.TreatedOutcomeModel.N)
        Assert.AreEqual(6, result.ControlOutcomeModel.N)
        Assert.AreEqual(2, result.TreatedOutcomeModel.Coefficients.Length)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Inference")>
    Public Sub AipwOverlap_ATT_ATC_ReturnFiniteEstimates()
        Dim input = ExampleInput()
        For Each target As PsmEstimand In New PsmEstimand() {PsmEstimand.ATO, PsmEstimand.ATT, PsmEstimand.ATC}
            Dim result = PsmDoublyRobustEstimator.EstimateAipw(input, input.SuppliedPropensityScores, Options(target))
            Assert.IsNotNull(result.Effect)
            Assert.IsFalse(Double.IsNaN(result.Effect.Estimate) OrElse Double.IsInfinity(result.Effect.Estimate))
            Assert.AreEqual(target, result.Effect.Estimand)
            Assert.AreEqual(3.0R, result.Effect.Estimate, 0.0001R)
        Next
    End Sub

    <TestMethod>
    <TestCategory("PSM-Inference")>
    Public Sub AipwRequiresOutcomeAndScoreAlignment()
        Dim input = ExampleInput()
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmDoublyRobustEstimator.EstimateAipw(input, New Double() {0.5R}))
        input.Outcome = Nothing
        Assert.ThrowsException(Of ArgumentException)(Sub() PsmDoublyRobustEstimator.EstimateAipw(input, input.SuppliedPropensityScores))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Tables")>
    Public Sub NeutralDiagnosticTables_HaveExpectedHeadersAndDimensions()
        Dim input = ExampleInput()
        Dim wd = PsmAdvancedDiagnostics.ComputeWeightDiagnostics(input, Enumerable.Repeat(1.0R, 12).ToArray())
        Dim ov = PsmAdvancedDiagnostics.ComputeOverlapDiagnostics(input, input.SuppliedPropensityScores, Nothing, 5)
        Dim weightTable = PsmAdvancedTables.WeightDiagnosticsTable(wd)
        Dim summaryTable = PsmAdvancedTables.OverlapSummaryTable(ov)
        Dim binsTable = PsmAdvancedTables.OverlapBinsTable(ov)
        Assert.AreEqual(4, weightTable.GetLength(0))
        Assert.AreEqual(13, weightTable.GetLength(1))
        Assert.AreEqual("Sample", weightTable(0, 0))
        Assert.AreEqual(3, summaryTable.GetLength(0))
        Assert.AreEqual(6, binsTable.GetLength(0))
        Assert.AreEqual("Treated N", binsTable(0, 3))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Tables")>
    Public Sub DoublyRobustAndLoveTables_ContainNeutralResults()
        Dim input = ExampleInput()
        Dim dr = PsmDoublyRobustEstimator.EstimateAipw(input, input.SuppliedPropensityScores, Options())
        Dim effectTable = PsmAdvancedTables.DoublyRobustEffectTable(dr)
        Assert.AreEqual(2, effectTable.GetLength(0))
        Assert.AreEqual("Doubly robust AIPW", effectTable(1, 0))
        Assert.AreEqual(dr.Effect.Estimate, Convert.ToDouble(effectTable(1, 2)), Tolerance)
        Dim love = PsmAdvancedTables.LovePlotTable(New List(Of PsmLovePlotRow) From {
            New PsmLovePlotRow With {.VariableName = "X", .SmdBefore = 0.5R, .MaxAbsAfter = 0.2R, .Flag = "Review"}
        })
        Assert.AreEqual("X", love(1, 0))
        Assert.AreEqual("Variable", love(0, 0))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Tables")>
    Public Sub BootstrapContractTables_ArePortableEvenWhenRunnerStaysHostSide()
        Dim bootstrap As New PsmBootstrapResult With {
            .Target = PsmBootstrapTarget.WeightedEffect,
            .EstimateOriginal = 1.3R,
            .BootstrapMean = 1.2R,
            .BootstrapStandardError = 0.2R,
            .ReplicationsUsed = 9,
            .ReplicationsRequested = 10
        }
        Dim table = PsmAdvancedTables.BootstrapTable(bootstrap)
        Assert.AreEqual(2, table.GetLength(0))
        Assert.AreEqual(8, table.GetLength(1))
        Assert.AreEqual("WeightedEffect", table(1, 0))
        Assert.AreEqual(9, table(1, 6))
        Assert.AreEqual(10, table(1, 7))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Tables")>
    Public Sub EmptyNeutralTables_ReturnDescriptiveMessages()
        Dim weightTable = PsmAdvancedTables.WeightDiagnosticsTable(Nothing)
        Dim overlapSummary = PsmAdvancedTables.OverlapSummaryTable(Nothing)
        Dim overlapBins = PsmAdvancedTables.OverlapBinsTable(Nothing)
        Dim loveTable = PsmAdvancedTables.LovePlotTable(Nothing)
        Dim effectTable = PsmAdvancedTables.DoublyRobustEffectTable(Nothing)
        Dim bootstrapTable = PsmAdvancedTables.BootstrapTable(Nothing)
        Assert.AreEqual("No weight diagnostics available", weightTable(0, 0))
        Assert.AreEqual("No overlap summary available", overlapSummary(0, 0))
        Assert.AreEqual("No overlap bins available", overlapBins(0, 0))
        Assert.AreEqual("No Love plot data available", loveTable(0, 0))
        Assert.AreEqual("No doubly robust effect estimate available", effectTable(0, 0))
        Assert.AreEqual("No bootstrap result available", bootstrapTable(0, 0))
    End Sub
End Class
