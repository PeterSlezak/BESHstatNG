using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BESHStat.OfficeAddin.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class CorrectiveWorkflowTests
{
    [TestMethod]
    public async Task PendingOptionRetirementMustFinishBeforeFreshRunPinsAnySource()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base");
        using var draft = new AnovaOptionDraft(new());
        var host = new AnalysisPilotTests.Host(); var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = true;
        host.Retire = async () => {
            if (!first) return; first = false; started.SetResult();
            await release.Task;
            // Mirrors a delayed native invalidate clearing whichever lease is live at dispatch.
            if (host.Pins > 1) host.CheckCode = "user.document-changed";
        };
        draft.IncludeWelch = true; await started.Task;
        var run = state.RunAsync();
        try { await Task.Yield(); Assert.AreEqual(1, host.Pins, "Fresh pin overtook outstanding host retirement."); }
        finally { release.TrySetResult(); await run; }
        Assert.AreEqual("analysis.completed", state.Code);
        Assert.AreEqual(2, host.Pins); Assert.IsTrue(state.Current);
    }

    [TestMethod]
    public async Task CompletedExistingWriteThenNewOptionsReacquireAndDemandFreshOccupiedConsent()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base");
        using var draft = new AnovaOptionDraft(new() { IncludeTukeyKramer = true });
        var host = new AnalysisPilotTests.Host(); var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.InputReference.Edit("'Input'!A1:C4"); state.SetDestination(SpreadsheetOutputDestination.CurrentWorksheet, "'Input'!J1", "ANOVAresults");
        await state.RunAsync(); await state.WriteResultsAsync(); Assert.AreEqual(OutputWriteState.Completed, state.WriteOutcome!.State);
        int oldTables = state.Tables.Count, reads = workbook.Reads;
        draft.IncludeWelch = true; draft.IncludeFisherLsd = true;
        Assert.IsFalse(state.Current); Assert.IsNull(state.Inspection); Assert.AreEqual(0, state.Tables.Count);
        Assert.AreEqual("'Input'!J1", state.StartCell); Assert.IsTrue(state.InputReference.Text.Contains("Input"));
        await state.RunAsync(); Assert.AreEqual("analysis.completed", state.Code); Assert.AreEqual(reads + 1, workbook.Reads);
        Assert.IsTrue(state.Tables.Count > oldTables); Assert.AreEqual(1, workbook.Writes);
        host.Occupancy = OutputOccupancy.Occupied;
        await state.WriteResultsAsync(); var firstInspection = state.Inspection; Assert.IsNotNull(firstInspection);
        await state.WriteAsync(false); Assert.AreEqual(OutputWriteState.CancelledByUser, state.WriteOutcome.State);
        await state.WriteResultsAsync(); Assert.AreNotSame(firstInspection, state.Inspection);
        await state.WriteAsync(true); Assert.AreEqual(OutputWriteState.Completed, state.WriteOutcome.State);
        Assert.IsTrue(workbook.Consent!.Approved); Assert.AreEqual(3, workbook.Writes);
    }

    [TestMethod]
    public async Task CancelDuringRetirementCannotPinAndNextExplicitRunCanRecover()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var host = new AnalysisPilotTests.Host(); var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); host.Retire = () => release.Task;
        draft.IncludeWelch = true; var pending = state.RunAsync(); state.Cancel();
        await pending; Assert.AreEqual(1, host.Pins); Assert.IsFalse(state.Current); Assert.IsFalse(state.Busy);
        release.SetResult(); host.Retire = null;
        await state.RunAsync(); Assert.AreEqual("analysis.completed", state.Code); Assert.AreEqual(2, host.Pins);
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task ResolvedNameBindsCommonPlanIntentAndCompletionWithoutEditingRequestedBase(bool uncertain)
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var host = new AnalysisPilotTests.Host { NameReply = new() { Code = "workbook.ok", RequestedName = "ANOVAresults", ResolvedName = "ANOVAresults2", WarningCode = "output.sheet-name-adjusted" } };
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement)) { Uncertain = uncertain };
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.InputReference.Edit("'Input'!A1:C4"); state.SetDestination(SpreadsheetOutputDestination.NewWorksheet, "D7", "ANOVAresults");
        await state.RunAsync(); await state.InspectAsync();
        Assert.AreEqual(0, workbook.Writes); Assert.AreEqual("ANOVAresults", state.NewSheetName);
        Assert.AreEqual("ANOVAresults2", state.ResolvedWorksheetName, state.Code); Assert.IsFalse(state.SheetNameNotice!.Created);
        Assert.IsFalse(state.Inspection!.ConfirmationRequired);
        await state.WriteAsync(); Assert.AreEqual(1, workbook.Writes); Assert.IsNull(workbook.Consent);
        Assert.AreEqual("ANOVAresults2", workbook.Plan!.Target.WorksheetName);
        Assert.AreEqual("ANOVAresults2", workbook.Intent!.WorksheetName); Assert.AreEqual(7, workbook.Intent.StartRow); Assert.AreEqual(4, workbook.Intent.StartColumn);
        Assert.AreEqual("ANOVAresults", state.NewSheetName); Assert.AreEqual(!uncertain, state.SheetNameNotice!.Created);
        Assert.AreEqual(uncertain ? null : "ANOVAresults2", state.CreatedWorksheetName);
        Assert.AreEqual(uncertain, state.RequiresRecovery);
    }

    [DataTestMethod]
    [DataRow("output.target-invalid")] [DataRow("output.sheet-name-unavailable")] [DataRow("output.sheet-name-collision")]
    public async Task NonmutatingAllocationFailureKeepsTablesOptionsAndResolvableDraft(string code)
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new() { IncludeWelch = true });
        var host = new AnalysisPilotTests.Host { NameReply = new() { Code = code } };
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync(); int tables = state.Tables.Count;
        state.SetDestination(SpreadsheetOutputDestination.NewWorksheet, "A1", "ANOVAresults"); await state.WriteResultsAsync();
        Assert.AreEqual(code, state.Code); Assert.AreEqual(0, workbook.Writes); Assert.AreEqual(tables, state.Tables.Count);
        Assert.IsTrue(draft.IncludeWelch); Assert.IsTrue(state.Current); Assert.IsFalse(state.RequiresRecovery);
        host.NameReply = null; await state.WriteResultsAsync(); Assert.AreEqual(OutputWriteState.Completed, state.WriteOutcome!.State);
        Assert.IsNull(state.SheetNameNotice); Assert.AreEqual("ANOVAresults", state.CreatedWorksheetName);
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task BrowserFocusInvalidationOrActualCellChangeCanRunAgainWithoutRepicking(bool changeCells)
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var host = new AnalysisPilotTests.Host(); var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync(); await state.InspectAsync();
        if (changeCells) workbook.Range.Rows[1][0] = SpreadsheetCellValue.NumberCell(43);
        host.ChangeDocument(); Assert.IsNull(state.Inspection); Assert.IsFalse(state.Current);
        await state.RunAsync(); Assert.AreEqual("analysis.completed", state.Code); Assert.AreEqual(2, workbook.Reads);
        Assert.IsTrue(state.Current); Assert.AreEqual(0, workbook.Writes);
    }

    [TestMethod]
    public async Task OutputComponentKeepsRequestedNameAndExplainsAutomaticSuffixWithAccessiblePilotStatus()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var host = new AnalysisPilotTests.Host(); var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.SetDestination(SpreadsheetOutputDestination.NewWorksheet, "A1", "ANOVAresults");
        host.NameReply = new() { Code = "workbook.ok", RequestedName = "ANOVAresults", ResolvedName = "ANOVAresults2", WarningCode = "output.sheet-name-adjusted" };
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync(); await state.InspectAsync();
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<OutputPicker>(
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["State"] = state, ["HostAvailable"] = true }))).ToHtmlString());
        StringAssert.Contains(html, "value=\"ANOVAresults\""); StringAssert.Contains(html, "first available positive numeric suffix");
        StringAssert.Contains(html, "role=\"status\""); StringAssert.Contains(html, "output.sheet-name-adjusted");
        StringAssert.Contains(html, "ANOVAresults2"); StringAssert.Contains(html, "Will use"); Assert.IsFalse(html.Contains("Created"));
        Assert.IsFalse(html.Contains("existing names reject")); Assert.AreEqual(1, workbook.Reads); Assert.AreEqual(0, workbook.Writes);
    }

    [TestMethod]
    public async Task ManagedResolutionBridgeRecordsOnlyWarningCodeInExistingOptInBoundedDiagnostics()
    {
        var diagnostics = new WorkbookDiagnostics(true); var runtime = new NameRuntime();
        await using var transport = new WorkbookTransport(runtime, diagnostics);
        var allocation = await transport.ResolveNewWorksheetNameAsync("PRIVATE requested name", "token", default);
        Assert.AreEqual("PRIVATE resolved name", allocation!.ResolvedName);
        Assert.AreEqual("resolveNewWorksheetName", runtime.Calls.Last());
#if DEBUG
        Assert.IsTrue(diagnostics.Snapshot().Any(e => e.Code == "output.sheet-name-adjusted"));
#endif
        Assert.IsFalse(System.Text.Json.JsonSerializer.Serialize(diagnostics.Snapshot()).Contains("PRIVATE"));
    }
    private sealed class NameRuntime : IJSRuntime, IJSObjectReference
    {
        public List<string> Calls { get; } = [];
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, default, args);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken token, object?[]? args)
        {
            Calls.Add(identifier);
            return ValueTask.FromResult((T)(identifier is "import" or "create" ? (object)this : identifier == "resolveNewWorksheetName"
                ? new WorksheetNameReply { Code = "workbook.ok", RequestedName = "PRIVATE requested name", ResolvedName = "PRIVATE resolved name", WarningCode = "output.sheet-name-adjusted" } : default!));
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
