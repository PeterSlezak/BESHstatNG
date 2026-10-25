using BESHStat.OfficeAddin.Services;
using BESHStatNG.AppInfrastructure;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BESHStat.OfficeAddin.Tests.OfficePreferenceTests;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class PreferenceLifecycleTests
{
    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task InheritedAlphaFollowsSaveWhileExplicitOverrideStays(bool explicitAlpha)
    {
        var owner = new OfficePreferenceSession(new OfficePreferenceStore(new Storage())); await owner.InitializeAsync();
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement)); var host = new AnalysisPilotTests.Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host, preferenceSession: owner);
        if (explicitAlpha) draft.SetAlpha("0.125");
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync(); Assert.AreEqual("analysis.completed", state.Code);
        Assert.IsTrue(await owner.SaveAsync(Snapshot())); Assert.AreEqual(explicitAlpha ? "0.125" : "0.01", draft.AlphaText);
        Assert.AreEqual(!explicitAlpha, draft.AlphaInherited); Assert.AreEqual(0, state.Tables.Count); Assert.IsTrue(state.CanRun);
        await state.RunAsync(); await state.WriteResultsAsync(); Assert.AreEqual(2, workbook.Reads);
        var pValues = workbook.Plan!.Tables.SelectMany(t => t.Cells).Where(c => c.IsPValue && c.Value.Kind == "number").ToArray();
        Assert.IsTrue(pValues.Length > 0);
        foreach (var p in pValues) Assert.AreEqual(p.Value.NumberValue < (explicitAlpha ? .125 : .01), p.IsSignificant);
    }
    [DataTestMethod]
    [DataRow("anova-all-posthoc")] [DataRow("anova-welch-tukey")] [DataRow("kw-dunn")]
    public async Task EveryDisplayModeAndUpperBoundKeepsNumericPosthocValuesAndAlpha(string fixture)
    {
        using var f = AnalysisPilotTests.Fixture(fixture); var root = f.RootElement;
        double[]? original = null;
        foreach (var mode in Enum.GetValues<PValueSmallValueDisplay>()) foreach (var upper in new[] { true, false }) {
            var data = Snapshot(.01, 4, mode).ToData(); data.PValues.UseUpperBound = upper;
            var preferences = PreferenceSnapshot.Normalize(data).Snapshot;
            using var draft = AnalysisPilotTests.Draft(root); draft.SetAlpha("0.05");
            var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(root));
            await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(root.GetProperty("analysisIdProposed").GetString()!), draft, workbook, new AnalysisPilotTests.Host(), preferences: preferences);
            state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync(); await state.WriteResultsAsync();
            var pValues = workbook.Plan!.Tables.SelectMany(t => t.Cells).Where(c => c.IsPValue && c.Value.Kind == "number").ToArray();
            Assert.IsTrue(pValues.Length > 0); var numbers = pValues.Select(p => p.Value.NumberValue!.Value).ToArray();
            if (original is null) original = numbers; else CollectionAssert.AreEqual(original, numbers);
            foreach (var p in pValues) Assert.AreEqual(p.Value.NumberValue < .05, p.IsSignificant);
            Assert.AreEqual(BESHStatNG.PValuePresentation.BuildExcelNumberFormat(preferences.CreateDisplaySettings()), workbook.Plan.PvalueNumberFormat);
        }
    }
    [TestMethod]
    public async Task SettingsAreRejectedDuringAcquisitionThenFreshRunRemainsAvailable()
    {
        var storage = new Storage(); var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await owner.InitializeAsync();
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var host = new AnalysisPilotTests.Host { Pending = new() }; var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host, preferenceSession: owner);
        state.InputReference.Edit("'Input'!A1:C4"); var run = state.RunAsync();
        Assert.IsTrue(state.Busy); Assert.AreEqual(ExecutionStage.Acquisition, state.Stage);
        Assert.IsFalse(await owner.SaveAsync(Snapshot())); Assert.AreEqual(0, storage.Saves);
        state.Cancel(); await run; host.Pending = null;
        Assert.IsTrue(await owner.SaveAsync(Snapshot())); await state.RunAsync(); Assert.AreEqual("analysis.completed", state.Code);
    }
    [TestMethod]
    public async Task SettingsAreRejectedDuringOutputInspectionAndRetireOccupiedConsentAfterward()
    {
        var owner = new OfficePreferenceSession(new OfficePreferenceStore(new Storage())); await owner.InitializeAsync();
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var host = new AnalysisPilotTests.Host(); var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host, preferenceSession: owner);
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync();
        host.PendingInspection = new(); var inspecting = state.InspectAsync(); Assert.IsFalse(await owner.SaveAsync(Snapshot()));
        state.Cancel(); await inspecting; host.PendingInspection = null; host.Occupancy = OutputOccupancy.Occupied;
        await state.RunAsync(); await state.WriteResultsAsync(); Assert.IsTrue(state.Inspection!.ConfirmationRequired);
        Assert.IsTrue(await owner.SaveAsync(Snapshot())); Assert.IsNull(state.Inspection);
        await state.WriteAsync(true); Assert.AreEqual(0, workbook.Writes);
        await state.RunAsync(); await state.WriteResultsAsync(); Assert.IsTrue(state.Inspection!.ConfirmationRequired);
    }
    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task SubmittedWriteRejectsSettingsAndObservedCompletedOrUncertainOutcomeSurvivesLaterSave(bool uncertain)
    {
        var storage = new Storage(); var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await owner.InitializeAsync();
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement)) { PendingWrite = new() }; var host = new AnalysisPilotTests.Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host, preferenceSession: owner);
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync(); var writing = state.WriteResultsAsync();
        Assert.IsTrue(state.NativeWriteSubmitted); Assert.IsFalse(await owner.SaveAsync(Snapshot())); Assert.AreEqual(0, storage.Saves);
        state.Cancel(); Assert.IsFalse(await owner.SaveAsync(Snapshot()));
        var outcome = uncertain ? new WriteResult(OutputWriteState.PartiallyWritten, WriteCodes.MutationUncertain, "Unverified", workbook.Intent!, true)
            : new WriteResult(OutputWriteState.Completed, WriteCodes.Completed, "Written", workbook.Intent!, true, workbook.Intent!.Regions.Select(r => r.ArtifactId));
        workbook.PendingWrite.SetResult(outcome); await writing;
        Assert.AreSame(outcome, state.WriteOutcome); Assert.IsTrue(await owner.SaveAsync(Snapshot())); Assert.AreSame(outcome, state.WriteOutcome);
        Assert.AreEqual(uncertain, state.RequiresRecovery); Assert.AreEqual(uncertain ? WriteCodes.MutationUncertain : WriteCodes.Completed, state.Code);
        if (uncertain) { Assert.IsFalse(state.CanRun); Assert.IsFalse(state.CanInspect); }
        Assert.AreEqual(1, workbook.Writes);
    }
    [TestMethod]
    public async Task RetirementIsAwaitedAndEffectiveValuesChangeOnlyAfterItFinishes()
    {
        var owner = new OfficePreferenceSession(new OfficePreferenceStore(new Storage())); await owner.InitializeAsync();
        var retired = new TaskCompletionSource(); using var participant = owner.Participate(() => true, () => retired.Task);
        var save = owner.SaveAsync(Snapshot()); Assert.IsTrue(owner.Applying); Assert.AreEqual(.05, owner.Effective.DefaultAlpha);
        retired.SetResult(); Assert.IsTrue(await save); Assert.AreEqual(.01, owner.Effective.DefaultAlpha);
    }
    [TestMethod]
    public async Task BuilderBusyRejectsSettingsAndIdleSaveRetiresPreviewWithoutWorksheetMutation()
    {
        var owner = new OfficePreferenceSession(new OfficePreferenceStore(new Storage())); await owner.InitializeAsync();
        var workbook = new AnalysisPilotTests.Workbook(WorksheetDiscoveryTests.AtRowOne(SpecificationBuilderTests.Repeated()));
        var host = new AnalysisPilotTests.Host { PendingCatalog = new() }; host.ConfigureWorksheet(workbook.Range);
        await using var builder = new SpecificationBuilderState(workbook, host, owner);
        var discover = builder.InitializeAsync(); Assert.IsTrue(builder.Busy); Assert.IsFalse(await owner.SaveAsync(Snapshot()));
        builder.Cancel(); await discover; host.PendingCatalog = null;
        await builder.InitializeAsync(); Assert.IsTrue(await owner.SaveAsync(Snapshot())); Assert.IsNull(builder.Preview); Assert.AreEqual(0, workbook.Writes);
    }
}
