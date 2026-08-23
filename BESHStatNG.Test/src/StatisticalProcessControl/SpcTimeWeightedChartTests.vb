Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.StatisticalProcessControl

<TestClass>
Public Class SpcTimeWeightedChartTests

    Private Shared Function HistoricalOptions(
        Optional gap As SpcSequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence,
        Optional missingPolicy As SpcMissingValuePolicy = SpcMissingValuePolicy.Reject) As SpcAnalysisOptions
        Dim options As SpcAnalysisOptions = NoRulesOptions(
            missingPolicy:=missingPolicy,
            parameterSource:=SpcParameterSource.UseHistoricalParameters)
        options.Rules.GapBehavior = gap
        Return options
    End Function

    Private Shared Function HistoricalRequest(chartType As SpcChartType,
                                              values As Double(),
                                              parameters As SpcChartParameters,
                                              Optional options As SpcAnalysisOptions = Nothing) As SpcFitRequest
        If options Is Nothing Then options = HistoricalOptions()
        Return New SpcFitRequest(
            chartType,
            SpcInputData.FromIndividualSequence(values),
            options,
            {New SpcHistoricalParameters(processMean:=10.0, processSigma:=2.0)},
            chartParameters:=parameters)
    End Function

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub Ewma_ReproducesRecursiveStatisticAndDynamicStartupLimits()
        Dim result As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.Ewma,
            {10.0, 12.0, 11.0},
            New SpcChartParameters(ewmaLambda:=0.25)))
        Dim panel As SpcPanelResult = result.GetPanel(SpcPanelType.Ewma)
        Assert.AreEqual(3, panel.PointCount)
        AssertClose(10.0, panel.Points(0).Value)
        AssertClose(10.5, panel.Points(1).Value)
        AssertClose(10.625, panel.Points(2).Value)

        Dim lambda As Double = 0.25
        For i As Integer = 0 To panel.PointCount - 1
            Dim age As Double = CDbl(i + 1)
            Dim varianceFactor As Double = lambda / (2.0 - lambda) *
                (1.0 - Math.Pow(1.0 - lambda, 2.0 * age))
            Dim expectedSe As Double = 2.0 * Math.Sqrt(varianceFactor)
            AssertClose(expectedSe, panel.Points(i).StandardError)
            AssertClose(10.0 - 3.0 * expectedSe, panel.Points(i).LowerControlLimit)
            AssertClose(10.0 + 3.0 * expectedSe, panel.Points(i).UpperControlLimit)
        Next
    End Sub

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub Ewma_SteadyStateLimitsAreConstantFromFirstPoint()
        Dim result As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.Ewma,
            {10.0, 12.0, 11.0},
            New SpcChartParameters(ewmaLambda:=0.25, useSteadyStateLimits:=True)))
        Dim points As SpcPointResult() = result.GetPanel(SpcPanelType.Ewma).Points
        Dim expectedSe As Double = 2.0 * Math.Sqrt(0.25 / 1.75)
        For Each point As SpcPointResult In points
            AssertClose(expectedSe, point.StandardError)
            AssertClose(points(0).UpperControlLimit, point.UpperControlLimit)
        Next
    End Sub

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub Cusum_ReproducesTabularRecursionAndIntrinsicDecisionSignals()
        Dim result As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.Cusum,
            {10.0, 12.0, 14.0, 8.0},
            New SpcChartParameters(cusumReferenceValue:=0.5,
                                   cusumDecisionInterval:=2.0,
                                   headStart:=1.0)))
        Dim upper As SpcPanelResult = result.GetPanel(SpcPanelType.UpperCusum)
        Dim lower As SpcPanelResult = result.GetPanel(SpcPanelType.LowerCusum)
        CollectionAssert.AreEqual(New Double() {0.5, 1.0, 2.5, 1.0},
                                  upper.Points.Select(Function(p) p.Value).ToArray())
        CollectionAssert.AreEqual(New Double() {-0.5, 0.0, 0.0, -0.5},
                                  lower.Points.Select(Function(p) p.Value).ToArray())
        Assert.AreEqual(1, upper.SignalCount)
        Assert.AreEqual(2, upper.Signals(0).TerminalPointIndex)
        Assert.AreEqual("CUSUM-U", upper.Signals(0).RuleCode)
        Assert.AreEqual(0, lower.SignalCount)
        Assert.AreEqual(1, result.SignalCount,
                        "Intrinsic signals must remain active when the preset is None.")
    End Sub

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub MovingAverage_DynamicStartupUsesPartialWindowsAndCorrectSourceRows()
        Dim result As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.MovingAverage,
            {10.0, 12.0, 11.0, 13.0},
            New SpcChartParameters(movingAverageSpan:=3)))
        Dim points As SpcPointResult() = result.GetPanel(SpcPanelType.MovingAverage).Points
        CollectionAssert.AreEqual(New Double() {10.0, 11.0, 11.0, 12.0},
                                  points.Select(Function(p) p.Value).ToArray())
        CollectionAssert.AreEqual(New Double() {1.0, 2.0, 3.0, 3.0},
                                  points.Select(Function(p) p.EffectiveSampleSize).ToArray())
        AssertClose(2.0, points(0).StandardError)
        AssertClose(2.0 / Math.Sqrt(2.0), points(1).StandardError)
        AssertClose(2.0 / Math.Sqrt(3.0), points(2).StandardError)
        CollectionAssert.AreEqual(New Integer() {1, 2, 3}, points(3).SourceRowIndices)
    End Sub

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub MovingAverage_SteadyStateRetainsUndefinedStartupPoints()
        Dim result As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.MovingAverage,
            {10.0, 12.0, 11.0, 13.0},
            New SpcChartParameters(movingAverageSpan:=3, useSteadyStateLimits:=True)))
        Dim points As SpcPointResult() = result.GetPanel(SpcPanelType.MovingAverage).Points
        Assert.IsFalse(points(0).HasFiniteValue)
        Assert.IsFalse(points(1).HasFiniteValue)
        Assert.IsTrue(points(2).HasFiniteValue)
        AssertClose(11.0, points(2).Value)
        Assert.IsTrue(result.Warnings.Any(
            Function(w) w.IndexOf("startup", StringComparison.OrdinalIgnoreCase) >= 0))
    End Sub

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub RuleExcludedPoint_BreaksStateAndCannotContaminateAnyTimeWeightedChart()
        Dim values As Double() = {10.0, 10.0, 100.0, 10.0, 10.0, 10.0}
        Dim chartTypes As SpcChartType() = {
            SpcChartType.Ewma, SpcChartType.Cusum, SpcChartType.MovingAverage
        }
        For Each chartType As SpcChartType In chartTypes
            Dim options As SpcAnalysisOptions = HistoricalOptions(
                SpcSequenceGapBehavior.BreakSequence)
            options.Exclusions = {
                New SpcExclusionDefinition(2, SpcExclusionScope.RuleEvaluation, "Synthetic spike")
            }
            Dim parameters As SpcChartParameters
            Select Case chartType
                Case SpcChartType.Ewma
                    parameters = New SpcChartParameters(ewmaLambda:=0.2)
                Case SpcChartType.Cusum
                    parameters = New SpcChartParameters(cusumReferenceValue:=0.5,
                                                        cusumDecisionInterval:=5.0)
                Case Else
                    parameters = New SpcChartParameters(movingAverageSpan:=3)
            End Select

            Dim result As SpcFitResult = SpcEngine.Fit(
                HistoricalRequest(chartType, values, parameters, options))
            Select Case chartType
                Case SpcChartType.Ewma
                    AssertClose(10.0,
                                result.GetPanel(SpcPanelType.Ewma).GetPoint(3).Value,
                                message:="EWMA must restart after the excluded point.")
                Case SpcChartType.Cusum
                    AssertClose(0.0,
                                result.GetPanel(SpcPanelType.UpperCusum).GetPoint(3).Value,
                                message:="CUSUM must restart after the excluded point.")
                Case SpcChartType.MovingAverage
                    AssertClose(10.0,
                                result.GetPanel(SpcPanelType.MovingAverage).GetPoint(3).Value,
                                message:="Moving-average window must restart after the excluded point.")
                    Dim movingAveragePoint As SpcPointResult =
                        result.GetPanel(SpcPanelType.MovingAverage).GetPoint(3)
                    AssertClose(1.0,
                                movingAveragePoint.EffectiveSampleSize)
            End Select
        Next
    End Sub

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub SkipPointAndContinue_PreservesPreGapStateButIgnoresExcludedValue()
        Dim values As Double() = {10.0, 12.0, 100.0, 10.0}
        Dim options As SpcAnalysisOptions = HistoricalOptions(
            SpcSequenceGapBehavior.SkipPointAndContinue)
        options.Exclusions = {
            New SpcExclusionDefinition(2, SpcExclusionScope.RuleEvaluation)
        }
        Dim result As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.Ewma,
            values,
            New SpcChartParameters(ewmaLambda:=0.5),
            options))
        Dim panel As SpcPanelResult = result.GetPanel(SpcPanelType.Ewma)
        AssertClose(11.0, panel.GetPoint(1).Value)
        Assert.IsFalse(panel.GetPoint(2).IncludedInRuleEvaluation)
        AssertClose(10.5, panel.GetPoint(3).Value,
                    message:="Point 3 must use the pre-gap state of 11, not the excluded spike.")
    End Sub

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub MissingPointGapBehavior_BreaksOrContinuesEwmaStateAsSelected()
        Dim values As Double() = {10.0, 12.0, Double.NaN, 10.0}
        Dim breakOptions As SpcAnalysisOptions = HistoricalOptions(
            SpcSequenceGapBehavior.BreakSequence,
            SpcMissingValuePolicy.OmitPoint)
        Dim breakResult As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.Ewma, values, New SpcChartParameters(ewmaLambda:=0.5), breakOptions))
        AssertClose(10.0, breakResult.GetPanel(SpcPanelType.Ewma).GetPoint(3).Value)

        Dim skipOptions As SpcAnalysisOptions = HistoricalOptions(
            SpcSequenceGapBehavior.SkipPointAndContinue,
            SpcMissingValuePolicy.OmitPoint)
        Dim skipResult As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.Ewma, values, New SpcChartParameters(ewmaLambda:=0.5), skipOptions))
        AssertClose(10.5, skipResult.GetPanel(SpcPanelType.Ewma).GetPoint(3).Value)
    End Sub

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub StageBoundary_ResetsRecursionWhileReferenceLimitsRemainFrozen()
        Dim values As Double() = {10.0, 12.0, 12.0, 12.0}
        Dim options As SpcAnalysisOptions = HistoricalOptions()
        options.ControlLimits.ParameterSource = SpcParameterSource.DefinedByStage
        options.Stages = {
            New SpcStageDefinition("A", 0, 1, SpcPhase.PhaseI,
                                   SpcStageLimitMode.UseHistoricalParameters),
            New SpcStageDefinition("B", 2, 3, SpcPhase.PhaseII,
                                   SpcStageLimitMode.UseReferenceStage, "A")
        }
        Dim result As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.Ewma,
            values,
            New SpcChartParameters(ewmaLambda:=0.5),
            options))
        Dim points As SpcPointResult() = result.GetPanel(SpcPanelType.Ewma).Points
        AssertClose(11.0, points(1).Value)
        AssertClose(11.0, points(2).Value,
                    message:="The second stage must restart from the referenced center.")
        AssertClose(points(0).UpperControlLimit, points(2).UpperControlLimit)
    End Sub

    <TestMethod>
    <TestCategory("SPC-TimeWeighted")>
    Public Sub TimeWeightedValidation_RejectsIncompatibleLimitsAndParametersAndWarnsForInapplicableRules()
        Dim options As SpcAnalysisOptions = HistoricalOptions()
        options.Rules.Preset = SpcRulePreset.Nelson
        Dim inapplicable As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.Ewma, {10.0, 11.0},
            New SpcChartParameters(ewmaLambda:=0.2), options))
        Assert.IsTrue(inapplicable.Warnings.Any(
            Function(w) w.IndexOf("No selected", StringComparison.OrdinalIgnoreCase) >= 0))

        options = HistoricalOptions()
        options.ControlLimits.Method = SpcControlLimitMethod.ExactProbability
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() SpcEngine.Fit(HistoricalRequest(
                SpcChartType.Ewma, {10.0, 11.0},
                New SpcChartParameters(ewmaLambda:=0.2), options)))

        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() SpcEngine.Fit(HistoricalRequest(
                SpcChartType.Cusum, {10.0, 11.0},
                New SpcChartParameters(cusumDecisionInterval:=1.0,
                                       headStart:=1.0))))

        Dim warningResult As SpcFitResult = SpcEngine.Fit(HistoricalRequest(
            SpcChartType.Cusum, {10.0, 11.0},
            New SpcChartParameters(cusumReferenceValue:=1.0,
                                   cusumDecisionInterval:=0.5)))
        Assert.IsTrue(warningResult.Warnings.Any(
            Function(w) w.IndexOf("reference value", StringComparison.OrdinalIgnoreCase) >= 0))

        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcEngine.Fit(HistoricalRequest(
                SpcChartType.MovingAverage, {10.0, 11.0},
                New SpcChartParameters(movingAverageSpan:=5,
                                       useSteadyStateLimits:=True))))
    End Sub

End Class
