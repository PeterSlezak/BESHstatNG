Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.StatisticalProcessControl

<TestClass>
Public Class SpcModelsStatisticsEngineTests

    <TestMethod>
    <TestCategory("SPC-Models")>
    Public Sub SpecificationHistoricalAndChartParameters_ValidateAndSnapshotValues()
        Dim specs As New SpcSpecificationLimits(1.0, 2.0, 3.0)
        Assert.IsTrue(specs.HasAnyValue)
        AssertClose(1.0, specs.LowerSpecificationLimit.Value)
        AssertClose(2.0, specs.Target.Value)
        AssertClose(3.0, specs.UpperSpecificationLimit.Value)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcSpecificationLimits(3.0, 2.0, 1.0)
            End Sub)

        Dim history As New SpcHistoricalParameters(
            stageId:=" S1 ", processMean:=10.0, processSigma:=2.0)
        Assert.AreEqual("S1", history.StageId)
        Assert.AreEqual(2, history.ParameterCount)
        Assert.IsFalse(history.AppliesToAllStages)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcHistoricalParameters()
            End Sub)

        Dim parameters As New SpcChartParameters(
            ewmaLambda:=0.25,
            cusumReferenceValue:=0.5,
            cusumDecisionInterval:=4.0,
            headStart:=1.0,
            movingAverageSpan:=5,
            useSteadyStateLimits:=True)
        AssertClose(0.25, parameters.EwmaLambda.Value)
        Assert.AreEqual(5, parameters.MovingAverageSpan.Value)
        Assert.IsTrue(parameters.UseSteadyStateLimits)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcChartParameters(ewmaLambda:=0.0)
            End Sub)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcChartParameters(movingAverageSpan:=1)
            End Sub)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Models")>
    Public Sub InputFactories_PreserveMetadataDefensively()
        Dim values As Double() = {1.0, 2.0, 3.0}
        Dim labels As String() = {"A", "B", "C"}
        Dim sourceRows As Integer() = {10, 11, 12}
        Dim data As SpcInputData = SpcInputData.FromIndividualSequence(
            values, labels, {101.0, 102.0, 103.0}, sourceRows, "Response")

        values(0) = 999.0
        labels(0) = "changed"
        sourceRows(0) = 999

        Assert.AreEqual(SpcDataLayout.IndividualSequence, data.Layout)
        Assert.AreEqual(3, data.RowCount)
        Assert.AreEqual(1, data.MeasurementColumnCount)
        Assert.AreEqual("Response", data.GetMeasurementColumnName(0))
        AssertClose(1.0, data.Measurements(0, 0))
        Assert.AreEqual("A", data.Labels(0))
        Assert.AreEqual(10, data.SourceRowIndices(0))

        Dim returned As Double(,) = data.Measurements
        returned(0, 0) = -20.0
        AssertClose(1.0, data.Measurements(0, 0), TightTolerance,
                    "The immutable input must return a defensive matrix copy.")
    End Sub

    <TestMethod>
    <TestCategory("SPC-Models")>
    Public Sub FitRequest_SnapshotsMutableOptionGraph()
        Dim options As SpcAnalysisOptions = OptionsWithRules(SpcRulePreset.Nelson)
        options.ControlLimits.SigmaMultiplier = 2.5
        options.Stages = BaselineMonitoringStages(30, 20)
        Dim request As New SpcFitRequest(
            SpcChartType.Individuals,
            SpcInputData.FromIndividualSequence(IndividualsData()),
            analysisOptions:=options,
            requestLabel:=" Snapshot ")

        options.ControlLimits.SigmaMultiplier = 9.0
        options.Rules.Preset = SpcRulePreset.None
        options.Stages = Array.Empty(Of SpcStageDefinition)()

        AssertClose(2.5, request.AnalysisOptions.ControlLimits.SigmaMultiplier)
        Assert.AreEqual(SpcRulePreset.Nelson, request.AnalysisOptions.Rules.Preset)
        Assert.AreEqual(2, request.Stages.Length)
        Assert.AreEqual("Snapshot", request.RequestLabel)

        Dim returned As SpcAnalysisOptions = request.AnalysisOptions
        returned.ControlLimits.SigmaMultiplier = 11.0
        AssertClose(2.5, request.AnalysisOptions.ControlLimits.SigmaMultiplier)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Models")>
    Public Sub ModelConstructors_RejectInvalidShapesAndIndices()
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcInputData(SpcDataLayout.IndividualSequence)
            End Sub)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcInputData.FromIndividualSequence({1.0, 2.0}, labels:={"only one"}))
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcInputData.FromIndividualSequence({1.0, Double.PositiveInfinity}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcStageDefinition(
                    "S", -1, 3, SpcPhase.PhaseI,
                    SpcStageLimitMode.EstimateFromStageData)
            End Sub)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcStageDefinition(
                    "S", 0, 3, SpcPhase.PhaseI,
                    SpcStageLimitMode.UseReferenceStage,
                    referenceStageId:="S")
            End Sub)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcExclusionDefinition(0, SpcExclusionScope.None)
            End Sub)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcPointResult(-1, 1.0, 0.0, -3.0, 3.0)
            End Sub)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Statistics")>
    Public Sub CalculateSubgroupAndWideSubgroups_ReturnKnownStatisticsAndMissingPolicies()
        Dim stats As SpcSubgroupStatistics = SpcStatistics.CalculateSubgroup({1.0, 2.0, 3.0, 4.0})
        Assert.AreEqual(4, stats.Count)
        AssertClose(2.5, stats.Mean)
        AssertClose(3.0, stats.Range)
        AssertClose(Math.Sqrt(5.0 / 3.0), stats.SampleStandardDeviation)

        Dim wide As Double(,) = {{1.0, 2.0, 3.0}, {4.0, Double.NaN, 6.0}}
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcStatistics.CalculateWideSubgroups(wide, SpcMissingValuePolicy.Reject))
        Dim omitted As SpcSubgroupStatistics() =
            SpcStatistics.CalculateWideSubgroups(wide, SpcMissingValuePolicy.OmitPoint)
        Assert.AreEqual(1, omitted.Length)
        Dim available As SpcSubgroupStatistics() =
            SpcStatistics.CalculateWideSubgroups(wide, SpcMissingValuePolicy.UseAvailableMeasurements)
        Assert.AreEqual(2, available.Length)
        Assert.AreEqual(2, available(1).Count)
        AssertClose(5.0, available(1).Mean)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Statistics")>
    Public Sub ControlChartConstants_C4AndRangeConstantsMatchReferences()
        AssertClose(0.797884560802865, SpcStatistics.C4(2), 0.000000000001)
        Dim constants As SpcControlChartConstants = SpcStatistics.GetControlChartConstants(5)
        Assert.AreEqual(5, constants.SubgroupSize)
        AssertClose(2.326, constants.D2, TightTolerance)
        AssertClose(0.864, constants.D3, TightTolerance)
        AssertClose(0.94, constants.C4, 0.001)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() SpcStatistics.GetControlChartConstants(26))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Statistics")>
    Public Sub MovingRangesAndRobustStatistics_ReturnKnownValuesWithoutMutatingInput()
        Dim values As Double() = {1.0, 4.0, 2.0, 8.0}
        Dim copy As Double() = CType(values.Clone(), Double())
        CollectionAssert.AreEqual(New Double() {3.0, 2.0, 6.0},
                                  SpcStatistics.MovingRanges(values, 2))
        CollectionAssert.AreEqual(New Double() {3.0, 6.0},
                                  SpcStatistics.MovingRanges(values, 3))
        AssertClose(3.0, SpcStatistics.Median(values))
        AssertClose(1.5, SpcStatistics.MedianAbsoluteDeviation(values))
        CollectionAssert.AreEqual(copy, values)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Statistics")>
    Public Sub IndividualSigmaEstimators_ExerciseClassicalAndRobustPaths()
        Dim values As Double() = {1.0, 2.0, 4.0, 7.0, 11.0}
        Dim meanMr As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromIndividuals(
            values, SpcWithinSigmaEstimator.MovingRange, 2, True)
        AssertClose(2.5 / 1.128, meanMr.Value)
        Assert.AreEqual(4, meanMr.ContributingPointCount)

        Dim medianMr As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromIndividuals(
            values, SpcWithinSigmaEstimator.MedianMovingRange, 2, True)
        AssertClose(2.5 / (Math.Sqrt(2.0) * 0.6744897501960817), medianMr.Value)

        Dim sampleSd As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromIndividuals(
            values, SpcWithinSigmaEstimator.SampleStandardDeviation, 2, False)
        AssertClose(SpcStatistics.CalculateSubgroup(values).SampleStandardDeviation,
                    sampleSd.Value)

        Dim mad As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromIndividuals(
            values, SpcWithinSigmaEstimator.MedianAbsoluteDeviation, 2, True)
        AssertClose(SpcStatistics.MedianAbsoluteDeviation(values) / 0.6744897501960817,
                    mad.Value)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcStatistics.EstimateSigmaFromIndividuals(
                values, SpcWithinSigmaEstimator.AverageRange, 2, True))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Statistics")>
    Public Sub SubgroupSigmaEstimators_ExerciseRangeAverageSdAndPooledPaths()
        Dim groups As SpcSubgroupStatistics() = {
            SpcStatistics.CalculateSubgroup({1.0, 2.0, 3.0}),
            SpcStatistics.CalculateSubgroup({2.0, 4.0, 6.0}),
            SpcStatistics.CalculateSubgroup({3.0, 6.0, 9.0})
        }
        Dim range As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromSubgroups(
            groups, SpcWithinSigmaEstimator.AverageRange, True)
        AssertClose((2.0 + 4.0 + 6.0) / 3.0 / 1.693, range.Value)

        Dim averageSd As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromSubgroups(
            groups, SpcWithinSigmaEstimator.AverageStandardDeviation, False)
        AssertClose((1.0 + 2.0 + 3.0) / 3.0, averageSd.Value)

        Dim pooled As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromSubgroups(
            groups, SpcWithinSigmaEstimator.PooledStandardDeviation, False)
        AssertClose(Math.Sqrt((2.0 * 1.0 + 2.0 * 4.0 + 2.0 * 9.0) / 6.0),
                    pooled.Value)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Engine")>
    Public Sub EngineRegistry_ReportsEveryBundledCalculatorAndPublicFamily()
        Dim implemented As SpcChartType() = SpcEngine.GetImplementedChartTypes()
        Dim expected As SpcChartType() = {
            SpcChartType.Individuals, SpcChartType.MovingRange,
            SpcChartType.IndividualsMovingRange, SpcChartType.XBar,
            SpcChartType.SubgroupRange, SpcChartType.SubgroupStandardDeviation,
            SpcChartType.XBarR, SpcChartType.XBarS,
            SpcChartType.PChart, SpcChartType.NpChart,
            SpcChartType.CChart, SpcChartType.UChart,
            SpcChartType.Cusum, SpcChartType.Ewma, SpcChartType.MovingAverage
        }
        CollectionAssert.AreEquivalent(expected, implemented)
        Assert.IsFalse(SpcEngine.IsImplemented(SpcChartType.RunChart))
        Assert.IsFalse(SpcEngine.IsImplemented(SpcChartType.LaneyPPrime))
        Assert.IsFalse(SpcEngine.IsImplemented(SpcChartType.GChart))
        Assert.IsFalse(SpcEngine.IsImplemented(SpcChartType.ShortRunZMovingRange))

        Assert.AreEqual(SpcChartFamily.Run, SpcEngine.GetChartFamily(SpcChartType.RunChart))
        Assert.AreEqual(SpcChartFamily.ShewhartVariables,
                        SpcEngine.GetChartFamily(SpcChartType.XBarR))
        Assert.AreEqual(SpcChartFamily.ShewhartAttributes,
                        SpcEngine.GetChartFamily(SpcChartType.LaneyPPrime))
        Assert.AreEqual(SpcChartFamily.RareEvent,
                        SpcEngine.GetChartFamily(SpcChartType.GChart))
        Assert.AreEqual(SpcChartFamily.TimeWeighted,
                        SpcEngine.GetChartFamily(SpcChartType.Ewma))
        Assert.AreEqual(SpcChartFamily.Multivariate,
                        SpcEngine.GetChartFamily(SpcChartType.PcaQ))
        Assert.AreEqual(SpcChartFamily.Specialized,
                        SpcEngine.GetChartFamily(SpcChartType.ProfileChart))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Engine")>
    Public Sub EngineValidation_RejectsBadLayoutsStagesHistoryAndAttributeValues()
        Dim individual As SpcInputData = SpcInputData.FromIndividualSequence({1.0, 2.0, 3.0})
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcEngine.Validate(New SpcFitRequest(SpcChartType.PChart, individual)))

        Dim options As SpcAnalysisOptions = NoRulesOptions()
        options.Stages = {
            New SpcStageDefinition("A", 0, 1, SpcPhase.PhaseI,
                                   SpcStageLimitMode.EstimateFromStageData),
            New SpcStageDefinition("B", 1, 2, SpcPhase.PhaseII,
                                   SpcStageLimitMode.UseReferenceStage, "A")
        }
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcEngine.Validate(New SpcFitRequest(SpcChartType.Individuals,
                                                       individual, options)))

        options = NoRulesOptions(parameterSource:=SpcParameterSource.UseHistoricalParameters)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcEngine.Validate(New SpcFitRequest(SpcChartType.Individuals,
                                                       individual, options)))

        Dim invalidCounts As SpcInputData =
            SpcInputData.FromAggregatedCounts({2.5, 1.0}, {10.0, 10.0})
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcEngine.Validate(New SpcFitRequest(SpcChartType.PChart,
                                                       invalidCounts)))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Engine")>
    Public Sub EngineCancellationAndUnsupportedChart_AreReportedDeterministically()
        Dim request As New SpcFitRequest(
            SpcChartType.Individuals,
            SpcInputData.FromIndividualSequence(IndividualsData()),
            NoRulesOptions())
        Assert.ThrowsException(Of OperationCanceledException)(
            Sub() SpcEngine.Fit(request, Function() True))

        Dim unsupported As New SpcFitRequest(
            SpcChartType.RunChart,
            SpcInputData.FromIndividualSequence(IndividualsData()),
            NoRulesOptions())
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() SpcEngine.Fit(unsupported))
    End Sub

End Class
