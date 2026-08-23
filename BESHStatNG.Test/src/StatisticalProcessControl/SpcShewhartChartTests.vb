Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.StatisticalProcessControl

<TestClass>
Public Class SpcShewhartChartTests

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub IndividualsMovingRange_ReproducesIndependentPhaseIFormulas()
        Dim values As Double() = IndividualsData()
        Dim options As SpcAnalysisOptions = NoRulesOptions()
        options.Stages = BaselineMonitoringStages(values.Length, 20)
        Dim result As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.IndividualsMovingRange,
            SpcInputData.FromIndividualSequence(values, Labels(values.Length), Sequence(values.Length),
                                                valueName:="Measurement"),
            options))

        Assert.AreEqual(2, result.PanelCount)
        Dim individuals As SpcPanelResult = result.GetPanel(SpcPanelType.IndividualValue)
        Dim movingRange As SpcPanelResult = result.GetPanel(SpcPanelType.MovingRange)
        Assert.IsNotNull(individuals)
        Assert.IsNotNull(movingRange)
        Assert.AreEqual(values.Length, individuals.PointCount)
        Assert.AreEqual(values.Length, movingRange.PointCount)
        Assert.IsFalse(movingRange.Points(0).HasFiniteValue)

        Dim expectedMean As Double = values.Take(20).Average()
        Dim baseline(values.Length - 1) As Double
        For i As Integer = 0 To baseline.Length - 1
            baseline(i) = If(i < 20, values(i), Double.NaN)
        Next
        Dim ranges As Double() = SpcStatistics.MovingRanges(baseline, 2)
        Dim expectedMrBar As Double = ranges.Average()
        Dim expectedSigma As Double = expectedMrBar / 1.128

        Dim first As SpcPointResult = individuals.Points(0)
        AssertClose(expectedMean, first.CenterLine)
        AssertClose(expectedSigma, first.StandardError)
        AssertClose(expectedMean - 3.0 * expectedSigma, first.LowerControlLimit)
        AssertClose(expectedMean + 3.0 * expectedSigma, first.UpperControlLimit)
        AssertClose(expectedMrBar, movingRange.Points(1).CenterLine)

        Dim monitoring As SpcPointResult = individuals.Points(25)
        Assert.AreEqual("Monitoring", monitoring.StageId)
        Assert.AreEqual(SpcPhase.PhaseII, monitoring.Phase)
        AssertClose(first.CenterLine, monitoring.CenterLine)
        AssertClose(first.UpperControlLimit, monitoring.UpperControlLimit)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub EveryExposedVariableChart_ReturnsItsExpectedPanelSet()
        Dim wide As Double(,) = WideSubgroups()
        Dim options As SpcAnalysisOptions = NoRulesOptions()
        options.Stages = BaselineMonitoringStages(wide.GetLength(0), 12)

        Dim cases As New Dictionary(Of SpcChartType, SpcPanelType()) From {
            {SpcChartType.XBar, {SpcPanelType.SubgroupMean}},
            {SpcChartType.SubgroupRange, {SpcPanelType.SubgroupRange}},
            {SpcChartType.SubgroupStandardDeviation, {SpcPanelType.SubgroupStandardDeviation}},
            {SpcChartType.XBarR, {SpcPanelType.SubgroupMean, SpcPanelType.SubgroupRange}},
            {SpcChartType.XBarS, {SpcPanelType.SubgroupMean, SpcPanelType.SubgroupStandardDeviation}}
        }

        For Each item As KeyValuePair(Of SpcChartType, SpcPanelType()) In cases
            Dim result As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
                item.Key,
                SpcInputData.FromWideSubgroups(wide, Labels(wide.GetLength(0), "G")),
                options))
            Assert.AreEqual(item.Value.Length, result.PanelCount, item.Key.ToString())
            For Each panelType As SpcPanelType In item.Value
                Assert.IsNotNull(result.GetPanel(panelType),
                                 item.Key.ToString() & " did not return " & panelType.ToString())
            Next
        Next
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub XBarR_UsesAverageRangeConstantsAndFrozenReferenceStageLimits()
        Dim wide As Double(,) = WideSubgroups()
        Dim options As SpcAnalysisOptions = NoRulesOptions()
        options.Stages = BaselineMonitoringStages(wide.GetLength(0), 12)
        Dim result As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.XBarR,
            SpcInputData.FromWideSubgroups(wide),
            options))

        Dim xbar As SpcPanelResult = result.GetPanel(SpcPanelType.SubgroupMean)
        Dim range As SpcPanelResult = result.GetPanel(SpcPanelType.SubgroupRange)
        Dim baselineMeans As New List(Of Double)()
        Dim baselineRanges As New List(Of Double)()
        For i As Integer = 0 To 11
            Dim row(4) As Double
            For j As Integer = 0 To 4
                row(j) = wide(i, j)
            Next
            Dim stats As SpcSubgroupStatistics = SpcStatistics.CalculateSubgroup(row)
            baselineMeans.Add(stats.Mean)
            baselineRanges.Add(stats.Range)
        Next
        Dim grandMean As Double = baselineMeans.Average()
        Dim rBar As Double = baselineRanges.Average()
        Dim sigma As Double = rBar / SpcStatistics.GetControlChartConstants(5).D2
        Dim standardError As Double = sigma / Math.Sqrt(5.0)

        AssertClose(grandMean, xbar.Points(0).CenterLine)
        AssertClose(standardError, xbar.Points(0).StandardError)
        AssertClose(grandMean + 3.0 * standardError, xbar.Points(0).UpperControlLimit)
        AssertClose(rBar, range.Points(0).CenterLine)
        AssertClose(xbar.Points(0).CenterLine, xbar.Points(20).CenterLine)
        AssertClose(range.Points(0).UpperControlLimit, range.Points(20).UpperControlLimit)
        Assert.IsTrue(xbar.ParameterEstimates.Any(Function(p) p.ParameterName = "ProcessMean"))
        Assert.IsTrue(xbar.ParameterEstimates.Any(Function(p) p.ParameterName = "ProcessSigma"))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub XBarS_UsesAverageBiasCorrectedSubgroupStandardDeviation()
        Dim wide As Double(,) = WideSubgroups()
        Dim options As SpcAnalysisOptions = NoRulesOptions(
            estimator:=SpcWithinSigmaEstimator.AverageStandardDeviation)
        options.Stages = BaselineMonitoringStages(wide.GetLength(0), 12)
        Dim result As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.XBarS,
            SpcInputData.FromWideSubgroups(wide),
            options))
        Dim sPanel As SpcPanelResult = result.GetPanel(SpcPanelType.SubgroupStandardDeviation)

        Dim sValues As New List(Of Double)()
        For i As Integer = 0 To 11
            Dim row(4) As Double
            For j As Integer = 0 To 4
                row(j) = wide(i, j)
            Next
            sValues.Add(SpcStatistics.CalculateSubgroup(row).SampleStandardDeviation)
        Next
        AssertClose(sValues.Average(), sPanel.Points(0).CenterLine)
        Assert.IsTrue(sPanel.Points.All(Function(p) p.LowerControlLimit >= 0.0))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub StackedAndWideSubgroupLayouts_ProduceEquivalentStatisticsAndSourceAudits()
        Dim wide As Double(,) = WideSubgroups()
        Dim stacked As Double() = Nothing
        Dim ids As String() = Nothing
        Dim labels As String() = Nothing
        Dim sequence As Double() = Nothing
        StackWide(wide, stacked, ids, labels, sequence)

        Dim wideOptions As SpcAnalysisOptions = NoRulesOptions()
        wideOptions.Stages = BaselineMonitoringStages(wide.GetLength(0), 12)
        Dim stackedOptions As SpcAnalysisOptions = NoRulesOptions()
        stackedOptions.Stages = BaselineMonitoringStages(wide.GetLength(0), 12)

        Dim wideResult As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.XBarR, SpcInputData.FromWideSubgroups(wide), wideOptions))
        Dim stackedResult As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.XBarR,
            SpcInputData.FromStackedObservations(stacked, ids, labels, sequence),
            stackedOptions))

        For Each panelType As SpcPanelType In {SpcPanelType.SubgroupMean, SpcPanelType.SubgroupRange}
            Dim left As SpcPointResult() = wideResult.GetPanel(panelType).Points
            Dim right As SpcPointResult() = stackedResult.GetPanel(panelType).Points
            Assert.AreEqual(left.Length, right.Length)
            For i As Integer = 0 To left.Length - 1
                AssertClose(left(i).Value, right(i).Value)
                AssertClose(left(i).CenterLine, right(i).CenterLine)
                AssertClose(left(i).LowerControlLimit, right(i).LowerControlLimit)
                AssertClose(left(i).UpperControlLimit, right(i).UpperControlLimit)
            Next
        Next
        Dim firstStackedPoint As SpcPointResult =
            stackedResult.GetPanel(SpcPanelType.SubgroupMean).Points(0)
        Assert.AreEqual(5, firstStackedPoint.SourceRowIndices.Length)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub MissingValuePolicies_RejectOmitOrUseAvailableAsConfigured()
        Dim wide As Double(,) = WideSubgroups()
        wide(3, 2) = Double.NaN

        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcEngine.Fit(New SpcFitRequest(
                SpcChartType.XBarR,
                SpcInputData.FromWideSubgroups(wide),
                NoRulesOptions(SpcMissingValuePolicy.Reject))))

        Dim omitted As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.XBarR,
            SpcInputData.FromWideSubgroups(wide),
            NoRulesOptions(SpcMissingValuePolicy.OmitPoint)))
        Assert.AreEqual(wide.GetLength(0) - 1, omitted.ChartPointCount)

        Dim available As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.XBarR,
            SpcInputData.FromWideSubgroups(wide),
            NoRulesOptions(SpcMissingValuePolicy.UseAvailableMeasurements)))
        Assert.AreEqual(wide.GetLength(0), available.ChartPointCount)
        AssertClose(4.0,
                    available.GetPanel(SpcPanelType.SubgroupMean).Points(3).EffectiveSampleSize)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub AttributeCharts_ReproducePooledPhaseIParameters()
        Dim counts As Double() = AttributeCounts()
        Dim sizes As Double() = SampleSizes()
        Dim exposures As Double() = ExposuresARRAY()
        Dim options As SpcAnalysisOptions = NoRulesOptions()
        options.Stages = BaselineMonitoringStages(counts.Length, 20)

        Dim p As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.PChart,
            SpcInputData.FromAggregatedCounts(counts, sizes), options))
        Dim np As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.NpChart,
            SpcInputData.FromAggregatedCounts(counts, ConstantSampleSizes(counts.Length)), options))
        Dim c As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.CChart,
            SpcInputData.FromAggregatedCounts(counts), options))
        Dim u As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.UChart,
            SpcInputData.FromAggregatedCounts(counts, exposures:=exposures), options))

        Dim pBar As Double = counts.Take(20).Sum() / sizes.Take(20).Sum()
        Dim cBar As Double = counts.Take(20).Average()
        Dim uBar As Double = counts.Take(20).Sum() / exposures.Take(20).Sum()
        AssertClose(pBar, p.GetPanel(SpcPanelType.Proportion).Points(0).CenterLine)
        AssertClose(pBar * 100.0,
                    np.GetPanel(SpcPanelType.NumberNonconforming).Points(0).CenterLine)
        AssertClose(cBar, c.GetPanel(SpcPanelType.DefectCount).Points(0).CenterLine)
        AssertClose(uBar, u.GetPanel(SpcPanelType.DefectRate).Points(0).CenterLine)
        Assert.IsTrue(p.GetPanel(SpcPanelType.Proportion).Points.All(
            Function(point) point.LowerControlLimit >= 0.0 AndAlso point.UpperControlLimit <= 1.0))
        Assert.IsTrue(c.GetPanel(SpcPanelType.DefectCount).Points.All(
            Function(point) point.LowerControlLimit >= 0.0))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub ExactProbabilityLimits_AreAvailableForEveryImplementedAttributeChart()
        Dim counts As Double() = AttributeCounts()
        Dim options As SpcAnalysisOptions = NoRulesOptions(
            method:=SpcControlLimitMethod.ExactProbability)
        Dim cases As New Dictionary(Of SpcChartType, Tuple(Of SpcInputData, SpcPanelType)) From {
            {SpcChartType.PChart,
             Tuple.Create(SpcInputData.FromAggregatedCounts(counts, SampleSizes()),
                          SpcPanelType.Proportion)},
            {SpcChartType.NpChart,
             Tuple.Create(SpcInputData.FromAggregatedCounts(counts,
                                                            ConstantSampleSizes(counts.Length)),
                          SpcPanelType.NumberNonconforming)},
            {SpcChartType.CChart,
             Tuple.Create(SpcInputData.FromAggregatedCounts(counts),
                          SpcPanelType.DefectCount)},
            {SpcChartType.UChart,
             Tuple.Create(SpcInputData.FromAggregatedCounts(counts,
                                                            exposures:=ExposuresARRAY()),
                          SpcPanelType.DefectRate)}
        }
        For Each item As KeyValuePair(Of SpcChartType, Tuple(Of SpcInputData, SpcPanelType)) In cases
            Dim result As SpcFitResult = SpcEngine.Fit(
                New SpcFitRequest(item.Key, item.Value.Item1, options))
            Dim point As SpcPointResult = result.GetPanel(item.Value.Item2).Points(0)
            AssertFinite(point.LowerControlLimit, item.Key.ToString() & " LCL")
            AssertFinite(point.UpperControlLimit, item.Key.ToString() & " UCL")
            Assert.IsTrue(Double.IsNaN(point.LowerOneSigmaLimit))
            Assert.IsTrue(Double.IsNaN(point.UpperTwoSigmaLimit))
        Next
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub NaturalLimitPolicy_ClipsOrRetainsMathematicalLimitsAsRequested()
        Dim data As SpcInputData =
            SpcInputData.FromAggregatedCounts({0.0, 0.0, 0.0, 1.0},
                                              {10.0, 10.0, 10.0, 10.0})
        Dim clippedOptions As SpcAnalysisOptions = NoRulesOptions()
        Dim clippedResult As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.PChart, data, clippedOptions))
        Dim clipped As SpcPointResult =
            clippedResult.GetPanel(SpcPanelType.Proportion).Points(0)
        AssertClose(0.0, clipped.LowerControlLimit)

        Dim retainedOptions As SpcAnalysisOptions = NoRulesOptions()
        retainedOptions.ControlLimits.NaturalLimitPolicy =
            SpcNaturalLimitPolicy.RetainCalculatedLimits
        Dim retainedResult As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.PChart, data, retainedOptions))
        Dim retained As SpcPointResult =
            retainedResult.GetPanel(SpcPanelType.Proportion).Points(0)
        Assert.IsTrue(retained.LowerControlLimit < 0.0)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub ExactProbabilityRuleOne_UsesDisplayedDiscreteLimits()
        Dim counts(20) As Double
        Dim sizes(20) As Double
        For i As Integer = 0 To 19
            counts(i) = 2.0
            sizes(i) = 100.0
        Next
        For i As Integer = 17 To 19
            counts(i) = 3.0
        Next
        counts(20) = 6.0
        sizes(20) = 84.0

        Dim exactOptions As SpcAnalysisOptions = OptionsWithRules(SpcRulePreset.RuleOneOnly)
        exactOptions.ControlLimits.Method = SpcControlLimitMethod.ExactProbability
        exactOptions.Stages = BaselineMonitoringStages(21, 20)
        Dim exactResult As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.PChart,
            SpcInputData.FromAggregatedCounts(counts, sizes),
            exactOptions))
        Dim exactPoint As SpcPointResult = exactResult.GetPanel(SpcPanelType.Proportion).Points(20)
        Assert.IsTrue(exactPoint.Value < exactPoint.UpperControlLimit)
        Assert.IsTrue(exactPoint.StandardizedValue > 3.0)
        Assert.IsFalse(exactPoint.IsSignalled,
                       "Rule 1 must follow the displayed exact probability limit.")

        Dim sigmaOptions As SpcAnalysisOptions = OptionsWithRules(SpcRulePreset.RuleOneOnly)
        sigmaOptions.Stages = BaselineMonitoringStages(21, 20)
        Dim sigmaResult As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.PChart,
            SpcInputData.FromAggregatedCounts(counts, sizes),
            sigmaOptions))
        Assert.IsTrue(sigmaResult.GetPanel(SpcPanelType.Proportion).Points(20).IsSignalled)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub HistoricalParametersAndStageOverrides_AreAppliedPerStage()
        Dim values As Double() = IndividualsData()
        Dim options As SpcAnalysisOptions = NoRulesOptions(
            parameterSource:=SpcParameterSource.DefinedByStage)
        options.Stages = {
            New SpcStageDefinition("Historical", 0, 14, SpcPhase.PhaseI,
                                   SpcStageLimitMode.UseHistoricalParameters),
            New SpcStageDefinition("Estimated", 15, values.Length - 1, SpcPhase.PhaseI,
                                   SpcStageLimitMode.EstimateFromStageData)
        }
        Dim history As SpcHistoricalParameters() = {
            New SpcHistoricalParameters("Historical", processMean:=10.0, processSigma:=0.5)
        }
        Dim result As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.Individuals,
            SpcInputData.FromIndividualSequence(values),
            options,
            history))
        Dim panel As SpcPanelResult = result.GetPanel(SpcPanelType.IndividualValue)
        AssertClose(10.0, panel.Points(0).CenterLine)
        AssertClose(0.5, panel.Points(0).StandardError)
        Assert.AreNotEqual(panel.Points(0).CenterLine, panel.Points(20).CenterLine)
        Assert.IsTrue(panel.ParameterEstimates.Any(
            Function(p) p.StageId = "Historical" AndAlso
                        p.LimitMode = SpcStageLimitMode.UseHistoricalParameters))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Shewhart")>
    Public Sub ExclusionsControlEstimationAndRulesButRemainInPanelAudit()
        Dim values As Double() = {10.0, 10.1, 9.9, 10.0, 50.0, 10.2, 9.8, 10.0, 10.1, 9.9}
        Dim options As SpcAnalysisOptions = OptionsWithRules(SpcRulePreset.RuleOneOnly)
        options.Exclusions = {
            New SpcExclusionDefinition(4, SpcExclusionScope.EstimationAndRules, "Setup event")
        }
        Dim result As SpcFitResult = SpcEngine.Fit(New SpcFitRequest(
            SpcChartType.Individuals,
            SpcInputData.FromIndividualSequence(values),
            options))
        Dim point As SpcPointResult = result.GetPanel(SpcPanelType.IndividualValue).GetPoint(4)
        Assert.IsNotNull(point)
        Assert.IsTrue(point.IsExplicitlyExcluded)
        Assert.IsFalse(point.IncludedInParameterEstimation)
        Assert.IsFalse(point.IncludedInRuleEvaluation)
        Assert.AreEqual("Setup event", point.ExclusionReason)
        Assert.IsFalse(point.IsSignalled)
        AssertClose(10.0, point.CenterLine, 0.05)
    End Sub

End Class
