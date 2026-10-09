Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.CausalInference

<TestClass>
Public Class PsmFoundationCoreTests

    Private Const Tolerance As Double = 0.0000000001R

    Private Shared Function ValidInput() As PsmInputData
        Return New PsmInputData With {
            .Ids = New String() {"T1", "C1", "T2", "C2"},
            .Treatment = New Double() {1.0R, 0.0R, 1.0R, 0.0R},
            .Outcome = New Double() {5.0R, 3.0R, 7.0R, 4.0R},
            .Covariates = New Double(,) {{1.0R, 2.0R}, {2.0R, 3.0R}, {3.0R, 4.0R}, {4.0R, 5.0R}},
            .CovariateNames = New String() {"Age", "Marker"},
            .SuppliedPropensityScores = New Double() {0.7R, 0.3R, 0.8R, 0.2R},
            .ExactGroupLabels = New String() {"A", "A", "B", "B"}
        }
    End Function

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub DefaultOptions_AreValidAndRetainExistingContract()
        Dim options As New PsmOptions()
        options.Validate()
        Assert.AreEqual(PsmScoreMethod.LogisticRegression, options.ScoreMethod)
        Assert.AreEqual(PsmMatchingMethod.NearestNeighbor, options.MatchingMethod)
        Assert.AreEqual(PsmEstimand.ATT, options.Estimand)
        Assert.AreEqual(1, options.MatchingRatio)
        Assert.AreEqual(PsmCaliperScale.None, options.CaliperScale)
        Assert.IsTrue(Double.IsNaN(options.Caliper))
        Assert.AreEqual(12345, options.RandomSeed)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub OptionsValidation_RejectsInvalidMatchingSettings()
        Dim options As New PsmOptions With {.MatchingRatio = 0}
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() options.Validate())
        options.MatchingRatio = 1
        options.DistanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper
        Assert.ThrowsException(Of ArgumentException)(Sub() options.Validate())
        options.CaliperScale = PsmCaliperScale.RawPropensityScore
        options.Caliper = 0.1R
        options.Validate()
        options.Caliper = -0.1R
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() options.Validate())
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub OptionsValidation_RejectsInvalidNumericalAndTrimmingSettings()
        Dim options As New PsmOptions With {.LogisticMaxIterations = 0}
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() options.Validate())
        options.LogisticMaxIterations = 100
        options.LogisticRidgePenalty = -1.0R
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() options.Validate())
        options.LogisticRidgePenalty = 0.0R
        options.TrimPropensityLower = 0.5R
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() options.Validate())
        options.TrimPropensityLower = 0.0R
        options.TrimPropensityUpper = 0.5R
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() options.Validate())
        options.TrimPropensityUpper = 1.0R
        options.SubclassificationStrata = 1
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() options.Validate())
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub ValidInput_ExposesAlignedRowsColumnsAndNames()
        Dim input As PsmInputData = ValidInput()
        input.Validate(New PsmOptions With {.ScoreMethod = PsmScoreMethod.Supplied})
        Assert.AreEqual(4, input.RowCount)
        Assert.AreEqual(2, input.CovariateCount)
        Assert.AreEqual("Age", input.GetCovariateName(0))
        Assert.AreEqual("Marker", input.GetCovariateName(1))
        Assert.AreEqual("X3", input.GetCovariateName(2))
        Assert.AreEqual("T1", input.GetId(0))
        Assert.AreEqual("5", input.GetId(4))
        input.Ids = Nothing
        input.CovariateNames = Nothing
        Assert.AreEqual("1", input.GetId(0))
        Assert.AreEqual("X1", input.GetCovariateName(0))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub InputValidation_RejectsMisalignedArraysAndMissingSuppliedScores()
        Dim input As PsmInputData = ValidInput()
        input.Covariates = New Double(,) {{1.0R, 2.0R}, {3.0R, 4.0R}, {5.0R, 6.0R}}
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(New PsmOptions()))
        input = ValidInput()
        input.Ids = New String() {"one"}
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(New PsmOptions()))
        input = ValidInput()
        input.SuppliedPropensityScores = Nothing
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(New PsmOptions With {.ScoreMethod = PsmScoreMethod.Supplied}))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub InputValidation_RejectsNonBinaryAndSingleGroupTreatment()
        Dim input As PsmInputData = ValidInput()
        input.Treatment(1) = 0.5R
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(New PsmOptions()))
        input = ValidInput()
        input.Treatment = New Double() {1.0R, 1.0R, 1.0R, 1.0R}
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(New PsmOptions()))
        input = ValidInput()
        input.Treatment(1) = Double.NaN
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(New PsmOptions()))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub InputValidation_RejectsNonFiniteCovariatesOutcomesAndInvalidScores()
        Dim input As PsmInputData = ValidInput()
        input.Covariates(1, 1) = Double.PositiveInfinity
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(New PsmOptions()))
        input = ValidInput()
        input.Outcome(1) = Double.NaN
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(New PsmOptions()))
        input = ValidInput()
        input.SuppliedPropensityScores(1) = 1.0R
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(New PsmOptions With {.ScoreMethod = PsmScoreMethod.Supplied}))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub WeightedMoments_RespectSampleCorrection()
        Dim values As Double() = {1.0R, 3.0R, 5.0R}
        Dim equalWeights As Double() = {1.0R, 1.0R, 1.0R}
        Assert.AreEqual(3.0R, PsmMath.WeightedMean(values, equalWeights), Tolerance)
        Assert.AreEqual(4.0R, PsmMath.WeightedVariance(values, equalWeights), Tolerance)
        Assert.AreEqual(8.0R / 3.0R, PsmMath.WeightedVariance(values, equalWeights, False), Tolerance)
        Assert.AreEqual(17.0R / 6.0R, PsmMath.WeightedMean(New Double() {1.0R, 2.0R, 4.0R}, New Double() {1.0R, 2.0R, 3.0R}), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub WeightedMoments_IgnoreNonFiniteAndNonPositiveWeights()
        Dim values As Double() = {2.0R, 100.0R, 4.0R, Double.NaN}
        Dim weights As Double() = {1.0R, 0.0R, 1.0R, 5.0R}
        Assert.AreEqual(3.0R, PsmMath.WeightedMean(values, weights), Tolerance)
        Assert.AreEqual(2.0R, PsmMath.WeightedVariance(values, weights), Tolerance)
        Assert.IsTrue(Double.IsNaN(PsmMath.WeightedMean(New Double() {1.0R}, New Double() {0.0R})))
        Assert.IsTrue(Double.IsNaN(PsmMath.WeightedVariance(New Double() {1.0R}, New Double() {1.0R})))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub EffectiveSampleSize_IgnoresInvalidWeights()
        Assert.AreEqual(36.0R / 14.0R, PsmMath.EffectiveSampleSize(New Double() {1.0R, 2.0R, 3.0R}), Tolerance)
        Assert.AreEqual(2.0R, PsmMath.EffectiveSampleSize(New Double() {1.0R, -4.0R, 1.0R, Double.NaN}), Tolerance)
        Assert.AreEqual(0.0R, PsmMath.EffectiveSampleSize(Nothing), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub VarianceAndStandardDeviation_SkipNonFiniteObservations()
        Dim values As Double() = {1.0R, 3.0R, 5.0R, Double.NaN, Double.PositiveInfinity}
        Assert.AreEqual(4.0R, PsmMath.Variance(values), Tolerance)
        Assert.AreEqual(8.0R / 3.0R, PsmMath.Variance(values, False), Tolerance)
        Assert.AreEqual(2.0R, PsmMath.StandardDeviation(values), Tolerance)
        Assert.IsTrue(Double.IsNaN(PsmMath.Variance(Nothing)))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub StandardizedBalanceMetrics_UsePooledScale()
        Assert.AreEqual(2.0R, PsmMath.PooledStandardDeviation(4.0R, 4.0R), Tolerance)
        Assert.AreEqual(1.0R, PsmMath.StandardizedMeanDifference(5.0R, 3.0R, 4.0R, 4.0R), Tolerance)
        Assert.AreEqual(2.0R, PsmMath.VarianceRatio(4.0R, 2.0R), Tolerance)
        Assert.IsTrue(Double.IsNaN(PsmMath.PooledStandardDeviation(-1.0R, 4.0R)))
        Assert.IsTrue(Double.IsNaN(PsmMath.VarianceRatio(4.0R, 0.0R)))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub Quantile_UsesInterpolatedSortedFiniteValues()
        Dim values As Double() = {8.0R, Double.NaN, 2.0R, 6.0R, 4.0R}
        Assert.AreEqual(2.0R, PsmMath.Quantile(values, 0.0R), Tolerance)
        Assert.AreEqual(3.5R, PsmMath.Quantile(values, 0.25R), Tolerance)
        Assert.AreEqual(5.0R, PsmMath.Quantile(values, 0.5R), Tolerance)
        Assert.AreEqual(8.0R, PsmMath.Quantile(values, 1.0R), Tolerance)
        Assert.AreEqual(8.0R, PsmMath.Quantile(values, 2.0R), Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub EcdfDifference_ReturnsMeanAndMaximumAbsoluteDistances()
        Dim distances As Tuple(Of Double, Double) = PsmMath.EcdfDifference(
            New Double() {1.0R, 3.0R}, New Double() {1.0R, 1.0R},
            New Double() {2.0R, 4.0R}, New Double() {1.0R, 1.0R})
        Assert.AreEqual(0.25R, distances.Item1, Tolerance)
        Assert.AreEqual(0.5R, distances.Item2, Tolerance)
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub MathHelpers_ClampLogitAndCopyAreStable()
        Assert.AreEqual(0.0R, PsmMath.Clamp(-4.0R, 0.0R, 1.0R), Tolerance)
        Assert.AreEqual(1.0R, PsmMath.Clamp(4.0R, 0.0R, 1.0R), Tolerance)
        Assert.AreEqual(0.0R, PsmMath.SafeLogit(0.5R), Tolerance)
        Dim original As Double() = {2.0R, 4.0R}
        Dim copied As Double() = PsmMath.CopyVector(original)
        CollectionAssert.AreEqual(original, copied)
        copied(0) = 100.0R
        Assert.AreEqual(2.0R, original(0), Tolerance)
        Assert.IsNull(PsmMath.CopyVector(Nothing))
    End Sub

    <TestMethod>
    <TestCategory("PSM-Foundation")>
    Public Sub ResultContracts_KeepCollectionsAndNeutralTableShape()
        Dim result As New PsmResult()
        Assert.IsNotNull(result.Matches)
        Assert.IsNotNull(result.Balance)
        Assert.IsNotNull(result.Subclasses)
        Assert.IsNotNull(result.Warnings)
        Dim table As Object(,) = PsmResult.EmptyTable("No matches")
        Assert.AreEqual(1, table.GetLength(0))
        Assert.AreEqual(1, table.GetLength(1))
        Assert.AreEqual("No matches", Convert.ToString(table(0, 0)))
    End Sub

End Class
