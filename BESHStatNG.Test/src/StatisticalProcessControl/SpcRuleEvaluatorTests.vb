Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.StatisticalProcessControl

<TestClass>
Public Class SpcRuleEvaluatorTests

    Private Shared Function EvaluateCustom(zValues As Double(),
                                           rule As SpcRuleDefinition,
                                           Optional marking As SpcSignalMarkingMode = SpcSignalMarkingMode.TerminalPointOnly,
                                           Optional gap As SpcSequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence,
                                           Optional panel As SpcPanelResult = Nothing) As SpcRuleEvaluationResult
        Dim options As New SpcRuleOptions With {
            .Preset = SpcRulePreset.Custom,
            .CustomRules = {rule},
            .PhaseScope = SpcRulePhaseScope.All,
            .GapBehavior = gap,
            .MarkingMode = marking
        }
        Return SpcRuleEvaluator.Evaluate(
            {If(panel, StandardizedPanel(zValues))}, options)
    End Function

    <TestMethod>
    <TestCategory("SPC-Rules")>
    Public Sub NamedPresets_HaveExpectedStableDefinitionsAndDefensiveArrays()
        Assert.AreEqual(0, SpcRuleCatalog.GetRules(SpcRulePreset.None).Length)
        Assert.AreEqual(1, SpcRuleCatalog.GetRules(SpcRulePreset.RuleOneOnly).Length)
        Assert.AreEqual(4, SpcRuleCatalog.GetRules(SpcRulePreset.WesternElectric).Length)
        Assert.AreEqual(8, SpcRuleCatalog.GetRules(SpcRulePreset.Nelson).Length)
        Assert.AreEqual(8, SpcRuleCatalog.GetRules(SpcRulePreset.PaperMontgomeryEightRules).Length)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcRuleCatalog.GetRules(SpcRulePreset.Custom))

        Dim first As SpcRuleDefinition() = SpcRuleCatalog.GetRules(SpcRulePreset.Nelson)
        Dim second As SpcRuleDefinition() = SpcRuleCatalog.GetRules(SpcRulePreset.Nelson)
        first(0) = Nothing
        Assert.IsNotNull(second(0))
        Assert.AreEqual("N1", second(0).RuleCode)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Rules")>
    Public Sub CustomRuleValidation_RejectsDuplicatesAndInvalidPatternShapes()
        Dim duplicateA As New SpcRuleDefinition("A", 1, SpcRuleKind.BeyondSigma, 1, 1, 3.0)
        Dim duplicateB As New SpcRuleDefinition("a", 2, SpcRuleKind.BeyondSigma, 1, 1, 2.0)
        Dim options As New SpcRuleOptions With {
            .Preset = SpcRulePreset.Custom,
            .CustomRules = {duplicateA, duplicateB}
        }
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcRuleCatalog.ResolveRules(options))

        options.CustomRules = {
            New SpcRuleDefinition("BAD-RUN", 1, SpcRuleKind.RunOnOneSide, 8, 7, 0.0)
        }
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcRuleCatalog.ResolveRules(options))

        options.CustomRules = Array.Empty(Of SpcRuleDefinition)()
        AssertThrowsAssignable(Of ArgumentException)(
            Sub() SpcRuleCatalog.ResolveRules(options))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Rules")>
    Public Sub BeyondSigma_DetectsUpperAndLowerTerminalPoints()
        Dim rule As New SpcRuleDefinition("T1", 1, SpcRuleKind.BeyondSigma,
                                          1, 1, 3.0)
        Dim evaluated As SpcRuleEvaluationResult =
            EvaluateCustom({0.0, 3.1, 0.0, -3.2}, rule)
        Assert.AreEqual(2, evaluated.SignalCount)
        Assert.AreEqual(SpcRuleSide.UpperSideOnly, evaluated.Signals(0).TriggeredSide)
        Assert.AreEqual(1, evaluated.Signals(0).TerminalPointIndex)
        Assert.AreEqual(SpcRuleSide.LowerSideOnly, evaluated.Signals(1).TriggeredSide)
        Assert.AreEqual(3, evaluated.Signals(1).TerminalPointIndex)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Rules")>
    Public Sub KOfMConsecutiveBeyondSigma_RequiresQualifyingPointsOnSameSide()
        Dim rule As New SpcRuleDefinition("T2", 2,
                                          SpcRuleKind.KOfMConsecutiveBeyondSigma,
                                          3, 2, 2.0)
        Dim detected As SpcRuleEvaluationResult =
            EvaluateCustom({2.2, 0.1, 2.3}, rule)
        Assert.AreEqual(1, detected.SignalCount)
        CollectionAssert.AreEqual(New Integer() {0, 2},
                                  detected.Signals(0).ContributingPointIndices)

        Dim mixedSides As SpcRuleEvaluationResult =
            EvaluateCustom({2.2, 0.0, -2.3}, rule)
        Assert.AreEqual(0, mixedSides.SignalCount)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Rules")>
    Public Sub RunTrendAlternatingCentralZoneAndMixturePatterns_AreAllEvaluated()
        Dim patterns As New List(Of Tuple(Of SpcRuleDefinition, Double())) From {
            Tuple.Create(New SpcRuleDefinition("RUN", 1, SpcRuleKind.RunOnOneSide,
                                               8, 8, 0.0),
                         Enumerable.Repeat(0.2, 8).ToArray()),
            Tuple.Create(New SpcRuleDefinition("TREND", 2, SpcRuleKind.MonotonicTrend,
                                               6, 6, 0.0),
                         New Double() {0.0, 0.2, 0.4, 0.6, 0.8, 1.0}),
            Tuple.Create(New SpcRuleDefinition("ALT", 3, SpcRuleKind.Alternating,
                                               6, 6, 0.0),
                         New Double() {0.0, 1.0, -1.0, 1.0, -1.0, 1.0}),
            Tuple.Create(New SpcRuleDefinition("CENTER", 4, SpcRuleKind.AllWithinSigma,
                                               5, 5, 1.0),
                         New Double() {0.2, -0.4, 0.8, -0.7, 0.0}),
            Tuple.Create(New SpcRuleDefinition("MIX", 5,
                                               SpcRuleKind.AllBeyondSigmaOnBothSides,
                                               6, 6, 1.0),
                         New Double() {1.2, -1.3, 1.4, -1.5, 1.6, -1.7})
        }

        For Each item As Tuple(Of SpcRuleDefinition, Double()) In patterns
            Dim evaluated As SpcRuleEvaluationResult = EvaluateCustom(item.Item2, item.Item1)
            Assert.AreEqual(1, evaluated.SignalCount, item.Item1.RuleCode)
            Assert.AreEqual(item.Item2.Length - 1,
                            evaluated.Signals(0).TerminalPointIndex,
                            item.Item1.RuleCode)
        Next
    End Sub

    <TestMethod>
    <TestCategory("SPC-Rules")>
    Public Sub SignalMarkingMode_ControlsTerminalVersusEntirePatternFlags()
        Dim rule As New SpcRuleDefinition("RUN", 7, SpcRuleKind.RunOnOneSide,
                                          4, 4, 0.0)
        Dim terminal As SpcRuleEvaluationResult = EvaluateCustom(
            {0.1, 0.2, 0.3, 0.4}, rule, SpcSignalMarkingMode.TerminalPointOnly)
        CollectionAssert.AreEqual(New Integer() {3}, terminal.Signals(0).MarkedPointIndices)
        Assert.IsFalse(terminal.Panels(0).Points(0).IsSignalled)
        Assert.IsTrue(terminal.Panels(0).Points(3).IsSignalled)

        Dim whole As SpcRuleEvaluationResult = EvaluateCustom(
            {0.1, 0.2, 0.3, 0.4}, rule, SpcSignalMarkingMode.EntirePattern)
        CollectionAssert.AreEqual(New Integer() {0, 1, 2, 3},
                                  whole.Signals(0).MarkedPointIndices)
        Assert.IsTrue(whole.Panels(0).Points.All(Function(p) p.IsSignalled))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Rules")>
    Public Sub GapsEitherBreakOrSkipSequencesAccordingToOption()
        Dim rule As New SpcRuleDefinition("RUN", 1, SpcRuleKind.RunOnOneSide,
                                          4, 4, 0.0)
        Dim panel As SpcPanelResult = StandardizedPanel(
            {0.2, 0.3, 99.0, 0.4, 0.5}, excludedIndex:=2)

        Dim broken As SpcRuleEvaluationResult = EvaluateCustom(
            {0.2, 0.3, 99.0, 0.4, 0.5}, rule,
            gap:=SpcSequenceGapBehavior.BreakSequence,
            panel:=panel)
        Assert.AreEqual(0, broken.SignalCount)

        Dim skipped As SpcRuleEvaluationResult = EvaluateCustom(
            {0.2, 0.3, 99.0, 0.4, 0.5}, rule,
            gap:=SpcSequenceGapBehavior.SkipPointAndContinue,
            panel:=panel)
        Assert.AreEqual(1, skipped.SignalCount)
        CollectionAssert.AreEqual(New Integer() {0, 1, 3, 4},
                                  skipped.Signals(0).ContributingPointIndices)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Rules")>
    Public Sub PhaseScopeAndStageBoundaries_PreventCrossBoundaryPatterns()
        Dim run As New SpcRuleDefinition("RUN", 1, SpcRuleKind.RunOnOneSide,
                                         4, 4, 0.0)
        Dim phases As SpcPhase() = {
            SpcPhase.PhaseI, SpcPhase.PhaseI, SpcPhase.PhaseII, SpcPhase.PhaseII,
            SpcPhase.PhaseII, SpcPhase.PhaseII
        }
        Dim stages As String() = {"A", "A", "B", "B", "B", "B"}
        Dim panel As SpcPanelResult = StandardizedPanel(
            {0.1, 0.2, 0.3, 0.4, 0.5, 0.6}, phases:=phases, stageIds:=stages)
        Dim options As New SpcRuleOptions With {
            .Preset = SpcRulePreset.Custom,
            .CustomRules = {run},
            .PhaseScope = SpcRulePhaseScope.PhaseII,
            .GapBehavior = SpcSequenceGapBehavior.SkipPointAndContinue,
            .MarkingMode = SpcSignalMarkingMode.TerminalPointOnly
        }
        Dim evaluated As SpcRuleEvaluationResult = SpcRuleEvaluator.Evaluate({panel}, options)
        Assert.AreEqual(1, evaluated.SignalCount)
        Assert.AreEqual(5, evaluated.Signals(0).TerminalPointIndex)
        Assert.AreEqual("B", evaluated.Signals(0).StageId)
        CollectionAssert.AreEqual(New Integer() {2, 3, 4, 5},
                                  evaluated.Signals(0).ContributingPointIndices)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Rules")>
    Public Sub RuleScopes_MapEveryPanelFamilyAndIgnoreInapplicableRules()
        Assert.AreEqual(SpcRuleScope.LocationPanels,
                        SpcRuleEvaluator.GetPanelRuleScope(SpcPanelType.SubgroupMean))
        Assert.AreEqual(SpcRuleScope.DispersionPanels,
                        SpcRuleEvaluator.GetPanelRuleScope(SpcPanelType.SubgroupRange))
        Assert.AreEqual(SpcRuleScope.AttributePanels,
                        SpcRuleEvaluator.GetPanelRuleScope(SpcPanelType.Proportion))
        Assert.AreEqual(SpcRuleScope.TimeWeightedPanels,
                        SpcRuleEvaluator.GetPanelRuleScope(SpcPanelType.Ewma))
        Assert.AreEqual(SpcRuleScope.RareEventPanels,
                        SpcRuleEvaluator.GetPanelRuleScope(SpcPanelType.EventsBetweenOccurrences))
        Assert.AreEqual(SpcRuleScope.MultivariatePanels,
                        SpcRuleEvaluator.GetPanelRuleScope(SpcPanelType.PcaQ))

        Dim rule As New SpcRuleDefinition(
            "LOCATION", 1, SpcRuleKind.BeyondSigma, 1, 1, 3.0,
            scope:=SpcRuleScope.LocationPanels)
        Dim dispersion As SpcPanelResult =
            StandardizedPanel({4.0}, SpcPanelType.SubgroupRange)
        Dim evaluated As SpcRuleEvaluationResult = EvaluateCustom(
            {4.0}, rule, panel:=dispersion)
        Assert.AreEqual(0, evaluated.SignalCount)
        Assert.IsTrue(evaluated.Warnings.Any(
            Function(w) w.IndexOf("No selected", StringComparison.OrdinalIgnoreCase) >= 0))
    End Sub

End Class
