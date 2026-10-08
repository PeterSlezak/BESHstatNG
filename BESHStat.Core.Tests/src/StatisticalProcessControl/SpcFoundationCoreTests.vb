Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.StatisticalProcessControl

<TestClass>
Public Class SpcFoundationCoreTests

    Private Const TightTolerance As Double = 0.0000000001R
    Private Const StandardTolerance As Double = 0.0000001R

    <TestMethod>
    <TestCategory("SPC-Core-Models")>
    Public Sub SpecificationHistoricalAndChartParameters_ValidateAndSnapshotValues()
        Dim specs As New SpcSpecificationLimits(1.0R, 2.0R, 3.0R)
        Assert.IsTrue(specs.HasAnyValue)
        AssertClose(1.0R, specs.LowerSpecificationLimit.Value)
        AssertClose(2.0R, specs.Target.Value)
        AssertClose(3.0R, specs.UpperSpecificationLimit.Value)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcSpecificationLimits(3.0R, 2.0R, 1.0R)
            End Sub)

        Dim history As New SpcHistoricalParameters(
            stageId:=" S1 ", processMean:=10.0R, processSigma:=2.0R)
        Assert.AreEqual("S1", history.StageId)
        Assert.AreEqual(2, history.ParameterCount)
        Assert.IsFalse(history.AppliesToAllStages)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcHistoricalParameters()
            End Sub)

        Dim parameters As New SpcChartParameters(
            ewmaLambda:=0.25R,
            cusumReferenceValue:=0.5R,
            cusumDecisionInterval:=4.0R,
            headStart:=1.0R,
            movingAverageSpan:=5,
            useSteadyStateLimits:=True)
        AssertClose(0.25R, parameters.EwmaLambda.Value)
        Assert.AreEqual(5, parameters.MovingAverageSpan.Value)
        Assert.IsTrue(parameters.UseSteadyStateLimits)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcChartParameters(ewmaLambda:=0.0R)
            End Sub)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcChartParameters(movingAverageSpan:=1)
            End Sub)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Core-Models")>
    Public Sub InputFactories_PreserveMetadataDefensively()
        Dim values As Double() = {1.0R, 2.0R, 3.0R}
        Dim labels As String() = {"A", "B", "C"}
        Dim sourceRows As Integer() = {10, 11, 12}
        Dim data As SpcInputData = SpcInputData.FromIndividualSequence(
            values, labels, {101.0R, 102.0R, 103.0R}, sourceRows, "Response")

        values(0) = 999.0R
        labels(0) = "changed"
        sourceRows(0) = 999

        Assert.AreEqual(SpcDataLayout.IndividualSequence, data.Layout)
        Assert.AreEqual(3, data.RowCount)
        Assert.AreEqual(1, data.MeasurementColumnCount)
        Assert.AreEqual("Response", data.GetMeasurementColumnName(0))
        AssertClose(1.0R, data.Measurements(0, 0))
        Assert.AreEqual("A", data.Labels(0))
        Assert.AreEqual(10, data.SourceRowIndices(0))

        Dim returned As Double(,) = data.Measurements
        returned(0, 0) = -20.0R
        AssertClose(1.0R, data.Measurements(0, 0), TightTolerance,
                    "The immutable input must return a defensive matrix copy.")
    End Sub

    <TestMethod>
    <TestCategory("SPC-Core-Models")>
    Public Sub FitRequest_SnapshotsMutableOptionGraph()
        Dim options As SpcAnalysisOptions = CreateOptions(SpcRulePreset.Nelson)
        options.ControlLimits.SigmaMultiplier = 2.5R
        options.Stages = {
            New SpcStageDefinition("Baseline", 0, 1, SpcPhase.PhaseI,
                                   SpcStageLimitMode.EstimateFromStageData),
            New SpcStageDefinition("Monitoring", 2, 3, SpcPhase.PhaseII,
                                   SpcStageLimitMode.UseReferenceStage,
                                   referenceStageId:="Baseline")
        }

        Dim request As New SpcFitRequest(
            SpcChartType.Individuals,
            SpcInputData.FromIndividualSequence({9.8R, 10.1R, 10.0R, 10.2R}),
            analysisOptions:=options,
            requestLabel:=" Snapshot ")

        options.ControlLimits.SigmaMultiplier = 9.0R
        options.Rules.Preset = SpcRulePreset.None
        options.Stages = Array.Empty(Of SpcStageDefinition)()

        AssertClose(2.5R, request.AnalysisOptions.ControlLimits.SigmaMultiplier)
        Assert.AreEqual(SpcRulePreset.Nelson, request.AnalysisOptions.Rules.Preset)
        Assert.AreEqual(2, request.Stages.Length)
        Assert.AreEqual("Snapshot", request.RequestLabel)

        Dim returned As SpcAnalysisOptions = request.AnalysisOptions
        returned.ControlLimits.SigmaMultiplier = 11.0R
        AssertClose(2.5R, request.AnalysisOptions.ControlLimits.SigmaMultiplier)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Core-Models")>
    Public Sub ModelConstructors_RejectInvalidShapesAndIndices()
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcInputData(SpcDataLayout.IndividualSequence)
            End Sub)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcInputData.FromIndividualSequence({1.0R, 2.0R}, labels:={"only one"}))
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcInputData.FromIndividualSequence({1.0R, Double.PositiveInfinity}))
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
                Dim unused = New SpcPointResult(-1, 1.0R, 0.0R, -3.0R, 3.0R)
            End Sub)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Core-Statistics")>
    Public Sub CalculateSubgroupAndWideSubgroups_ReturnKnownStatisticsAndMissingPolicies()
        Dim stats As SpcSubgroupStatistics = SpcStatistics.CalculateSubgroup({1.0R, 2.0R, 3.0R, 4.0R})
        Assert.AreEqual(4, stats.Count)
        AssertClose(2.5R, stats.Mean)
        AssertClose(3.0R, stats.Range)
        AssertClose(Math.Sqrt(5.0R / 3.0R), stats.SampleStandardDeviation)

        Dim wide As Double(,) = {{1.0R, 2.0R, 3.0R}, {4.0R, Double.NaN, 6.0R}}
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcStatistics.CalculateWideSubgroups(wide, SpcMissingValuePolicy.Reject))
        Dim omitted As SpcSubgroupStatistics() =
            SpcStatistics.CalculateWideSubgroups(wide, SpcMissingValuePolicy.OmitPoint)
        Assert.AreEqual(1, omitted.Length)
        Dim available As SpcSubgroupStatistics() =
            SpcStatistics.CalculateWideSubgroups(wide, SpcMissingValuePolicy.UseAvailableMeasurements)
        Assert.AreEqual(2, available.Length)
        Assert.AreEqual(2, available(1).Count)
        AssertClose(5.0R, available(1).Mean)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Core-Statistics")>
    Public Sub ControlChartConstants_C4AndRangeConstantsMatchReferences()
        AssertClose(0.797884560802865R, SpcStatistics.C4(2), 0.000000000001R)
        Dim constants As SpcControlChartConstants = SpcStatistics.GetControlChartConstants(5)
        Assert.AreEqual(5, constants.SubgroupSize)
        AssertClose(2.326R, constants.D2, TightTolerance)
        AssertClose(0.864R, constants.D3, TightTolerance)
        AssertClose(0.94R, constants.C4, 0.001R)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() SpcStatistics.GetControlChartConstants(26))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Core-Statistics")>
    Public Sub MovingRangesAndSigmaEstimators_ExerciseClassicalAndRobustPaths()
        Dim values As Double() = {1.0R, 2.0R, 4.0R, 7.0R, 11.0R}
        Dim original As Double() = CType(values.Clone(), Double())

        CollectionAssert.AreEqual(New Double() {1.0R, 2.0R, 3.0R, 4.0R},
                                  SpcStatistics.MovingRanges(values, 2))
        AssertClose(4.0R, SpcStatistics.Median(values))
        AssertClose(3.0R, SpcStatistics.MedianAbsoluteDeviation(values))
        CollectionAssert.AreEqual(original, values)

        Dim meanMr As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromIndividuals(
            values, SpcWithinSigmaEstimator.MovingRange, 2, True)
        AssertClose(2.5R / 1.128R, meanMr.Value)
        Assert.AreEqual(4, meanMr.ContributingPointCount)

        Dim medianMr As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromIndividuals(
            values, SpcWithinSigmaEstimator.MedianMovingRange, 2, True)
        AssertClose(2.5R / (Math.Sqrt(2.0R) * 0.67448975019608171R), medianMr.Value)

        Dim sampleSd As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromIndividuals(
            values, SpcWithinSigmaEstimator.SampleStandardDeviation, 2, False)
        AssertClose(SpcStatistics.CalculateSubgroup(values).SampleStandardDeviation,
                    sampleSd.Value)

        Dim mad As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromIndividuals(
            values, SpcWithinSigmaEstimator.MedianAbsoluteDeviation, 2, True)
        AssertClose(SpcStatistics.MedianAbsoluteDeviation(values) / 0.67448975019608171R,
                    mad.Value)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcStatistics.EstimateSigmaFromIndividuals(
                values, SpcWithinSigmaEstimator.AverageRange, 2, True))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Core-Statistics")>
    Public Sub SubgroupSigmaEstimators_ExerciseRangeAverageSdAndPooledPaths()
        Dim groups As SpcSubgroupStatistics() = {
            SpcStatistics.CalculateSubgroup({1.0R, 2.0R, 3.0R}),
            SpcStatistics.CalculateSubgroup({2.0R, 4.0R, 6.0R}),
            SpcStatistics.CalculateSubgroup({3.0R, 6.0R, 9.0R})
        }
        Dim range As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromSubgroups(
            groups, SpcWithinSigmaEstimator.AverageRange, True)
        AssertClose((2.0R + 4.0R + 6.0R) / 3.0R / 1.693R, range.Value)

        Dim averageSd As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromSubgroups(
            groups, SpcWithinSigmaEstimator.AverageStandardDeviation, False)
        AssertClose((1.0R + 2.0R + 3.0R) / 3.0R, averageSd.Value)

        Dim pooled As SpcSigmaEstimate = SpcStatistics.EstimateSigmaFromSubgroups(
            groups, SpcWithinSigmaEstimator.PooledStandardDeviation, False)
        AssertClose(Math.Sqrt((2.0R * 1.0R + 2.0R * 4.0R + 2.0R * 9.0R) / 6.0R),
                    pooled.Value)
    End Sub

    Private Shared Function CreateOptions(preset As SpcRulePreset) As SpcAnalysisOptions
        Return New SpcAnalysisOptions With {
            .MissingValuePolicy = SpcMissingValuePolicy.Reject,
            .ControlLimits = New SpcControlLimitOptions With {
                .ParameterSource = SpcParameterSource.EstimateFromPhaseI,
                .Method = SpcControlLimitMethod.ShewhartSigma,
                .SigmaMultiplier = 3.0R,
                .WithinSigmaEstimator = SpcWithinSigmaEstimator.Automatic,
                .NaturalLimitPolicy = SpcNaturalLimitPolicy.ClipToFeasibleRange,
                .MovingRangeLength = 2,
                .UseBiasCorrection = True
            },
            .Rules = New SpcRuleOptions With {
                .preset = preset,
                .PhaseScope = SpcRulePhaseScope.All,
                .GapBehavior = SpcSequenceGapBehavior.BreakSequence,
                .MarkingMode = SpcSignalMarkingMode.TerminalPointOnly
            }
        }
    End Function

    Private Shared Sub AssertClose(expected As Double,
                                   actual As Double,
                                   Optional tolerance As Double = StandardTolerance,
                                   Optional message As String = Nothing)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) OrElse
           Math.Abs(expected - actual) > tolerance Then
            Assert.Fail(If(message, "Values differ.") &
                        " Expected " & expected.ToString("R") &
                        ", actual " & actual.ToString("R") & ".")
        End If
    End Sub

    Private Shared Function AssertThrowsAssignable(Of TException As Exception)(
        action As Action,
        Optional message As String = Nothing) As TException

        If action Is Nothing Then Throw New ArgumentNullException(NameOf(action))

        Try
            action()
        Catch ex As Exception
            If TypeOf ex Is TException Then Return DirectCast(ex, TException)
            Assert.Fail(If(message, "Unexpected exception type.") &
                        " Expected a type assignable to " & GetType(TException).FullName &
                        ", but " & ex.GetType().FullName & " was thrown.")
        End Try

        Assert.Fail(If(message, "Expected an exception.") &
                    " Expected a type assignable to " & GetType(TException).FullName & ".")
        Return Nothing
    End Function
End Class
