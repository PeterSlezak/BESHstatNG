using System.Text.Json;
using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class AnalysisPilotTests
{
    internal sealed class Host : IPilotHost, IRangeCaptureHost, ICompositeSourceHost, IPilotSafetyHost, IWorksheetNameHost, IWorksheetDiscoveryHost
    {
        public event Action? Invalidated;
        public event Action<string,string>? WorksheetsInvalidated;
        public string? DiscoveryOwner;
        public WorksheetCatalogReply Catalog = new() { Code="workbook.ok", DocumentIdentity="document-1", ActiveWorksheetId="sheet-1", Worksheets=[new("sheet-1","Input","Visible")] };
        public Func<string,WorksheetBoundsReply>? BoundsReply;
        public TaskCompletionSource<WorksheetCatalogReply?>? PendingCatalog;
        public int Catalogs, Bounds;
        public void ConfigureWorksheet(SpreadsheetRangeData source)
        {
            Catalog=new(){Code="workbook.ok",DocumentIdentity="document-1",ActiveWorksheetId=source.WorksheetId,Worksheets=[new(source.WorksheetId,source.WorksheetName,"Visible")]};
            BoundsReply=id=>new(){Code="workbook.ok",DocumentIdentity=Catalog.DocumentIdentity,Empty=false,
                Reference=SpreadsheetRangeReference.FromAddress(source.Address,source.WorksheetName)};
            var previous=BoundsReply;
            BoundsReply=id=>{var reply=previous(id);reply.Reference!.WorksheetId=source.WorksheetId;return reply;};
        }
        public void EmitWorksheet(string? owner=null,string code="spec.source-stale")=>WorksheetsInvalidated?.Invoke(owner??DiscoveryOwner!,code);
        public Task<WorksheetCatalogReply?> GetWorksheetsAsync(string owner,CancellationToken token)
        {DiscoveryOwner=owner;Catalogs++;return PendingCatalog?.Task.WaitAsync(token)??Task.FromResult<WorksheetCatalogReply?>(Catalog);}
        public Task<WorksheetBoundsReply?> GetWorksheetBoundsAsync(string owner,string documentIdentity,string id,CancellationToken token)
        {Bounds++;var reference=SpreadsheetRangeReference.FromAddress(Reference.Address,Reference.WorksheetName);reference.WorksheetId=Reference.WorksheetId.Length>0?Reference.WorksheetId:"sheet-1";return Task.FromResult<WorksheetBoundsReply?>(BoundsReply?.Invoke(id)??new(){Code="workbook.ok",DocumentIdentity=Catalog.DocumentIdentity,Empty=false,Reference=reference});}
        public Task StopWorksheetDiscoveryAsync(string owner)=>Task.CompletedTask;
        public bool RequiresRecovery { get; private set; }
        public WriteResult? LastWriteOutcome { get; private set; }
        public void RecordWriteOutcome(WriteResult outcome) { LastWriteOutcome = outcome; RequiresRecovery |= outcome.State == OutputWriteState.PartiallyWritten && outcome.MutationMayHaveOccurred; }
        public event Action<RangeCaptureReply>? RangeCaptured;
        public void EmitCapture(RangeCaptureReply reply) => RangeCaptured?.Invoke(reply);
        public string? Owner;
        public string CaptureCode = "workbook.ok";
        public int Begins, Ends, Resolutions;
        public TaskCompletionSource<RangeCaptureReply?>? PendingCapture;
        public SpreadsheetRangeReference Reference = SpreadsheetRangeReference.FromAddress("A1:C4", "Input");
        public Func<SpreadsheetRangeReference, RangeCaptureReply>? ResolveReply;
        public Task<RangeCaptureReply?> ResolveReferenceAsync(SpreadsheetRangeReference reference, string? handle, CancellationToken token)
        { Resolutions++; if (ResolveReply is not null) return Task.FromResult<RangeCaptureReply?>(ResolveReply(reference)); reference.WorksheetId = Reference.WorksheetId.Length > 0 ? Reference.WorksheetId : "sheet-1"; return Task.FromResult<RangeCaptureReply?>(new() { Code = "workbook.ok", Handle = "reference", Reference = reference }); }
        public Task<RangeCaptureReply?> CaptureSelectionAsync(bool singleCell, CancellationToken token) => Task.FromResult<RangeCaptureReply?>(new() { Code = CaptureCode, Handle = "reference", Reference = Reference });
        public Task<RangeCaptureReply?> BeginCaptureAsync(string owner, bool output, CancellationToken token)
        { Owner = owner; Begins++; return PendingCapture?.Task ?? Task.FromResult<RangeCaptureReply?>(new() { Code = CaptureCode, Owner = owner }); }
        public Task<RangeCaptureReply?> EndCaptureAsync(string owner, bool cancel, CancellationToken token)
        { Ends++; return Task.FromResult<RangeCaptureReply?>(new() { Code = cancel ? "range.capture-cancelled" : "workbook.ok", Owner = owner, Handle = "reference", Reference = Reference }); }
        public Task<PilotReply?> PinReferenceSourceAsync(SpreadsheetRangeData snapshot, string handle, CancellationToken token) => PinSourceAsync(snapshot, token);
        public IReadOnlyList<PhysicalSource>? Sources;
        public Task<PilotReply?> PinSourcesAsync(IReadOnlyList<PhysicalSource> sources, CancellationToken token)
        { Sources = sources; return PinSourceAsync(sources[0].Snapshot, token); }
        public void ChangeDocument() => Invalidated?.Invoke();
        public string CheckCode = "workbook.ok";
        public OutputOccupancy Occupancy;
        public int Pins, Checks, Inspections;
        public TaskCompletionSource<PilotReply?>? PendingInspection;
        public TaskCompletionSource<PilotReply?>? Pending;
        public Task<PilotReply?> PinSourceAsync(SpreadsheetRangeData snapshot, CancellationToken token)
        { Pins++; Reference.WorksheetId = snapshot.WorksheetId; return Pending?.Task.WaitAsync(token) ?? Task.FromResult<PilotReply?>(new() { Code = "workbook.ok", SourceToken = $"source-{Pins}", SessionIdentity = $"session-{Pins}" }); }
        public Task<PilotReply?> CheckSourceAsync(string sourceToken, CancellationToken token)
        { Checks++; return Task.FromResult<PilotReply?>(new() { Code = CheckCode }); }
        public Task<PilotReply?> PrepareOutputAsync(OfficeOutputPlan plan, WriteIntent intent, string id, CancellationToken token)
        { Inspections++; if (PendingInspection is not null) return PendingInspection.Task.WaitAsync(token); return Task.FromResult<PilotReply?>(new() { Code = "workbook.ok", State = Occupancy, ObservationIdentity = "observation",
            Facts = new() { WorkbookAvailable = true, SheetExists = intent.Destination == SpreadsheetOutputDestination.CurrentWorksheet,
                IdentityUnambiguous = true, Writable = true, ProtectedSheet = false, MergedConflict = false, CanCreateWorksheet = true,
                WorksheetNameValid = true, NewWorksheetNameCollision = false },
            OccupiedRegionIds = Occupancy == OutputOccupancy.Occupied ? [intent.Regions[0].ArtifactId] : [], HasValues = Occupancy == OutputOccupancy.Occupied }); }
        public Func<Task>? Retire;
        public WorksheetNameReply? NameReply;
        public Task<WorksheetNameReply?> ResolveNewWorksheetNameAsync(string requestedName, string sourceToken, CancellationToken token)
            => Task.FromResult<WorksheetNameReply?>(NameReply ?? new() { Code = "workbook.ok", RequestedName = requestedName, ResolvedName = requestedName });
        public Task InvalidateAsync() => Retire?.Invoke() ?? Task.CompletedTask;
        public Task InvalidateOutputAsync() => Task.CompletedTask;
    }
    internal sealed class Workbook(SpreadsheetRangeData range) : IWorkbookAdapter
    {
        public SpreadsheetRangeData Range = range;
        public Func<SpreadsheetRangeReference, SpreadsheetRangeData>? ResolveRange;
        public string ReadCode = "workbook.ok";
        public int Reads, Writes;
        public OfficeOutputPlan? Plan;
        public WriteConsent? Consent;
        public WriteIntent? Intent;
        public bool Uncertain;
        public Action? BeforeWriteReturn;
        public TaskCompletionSource<WriteResult>? PendingWrite;
        public Task<WorkbookResult<SpreadsheetRangeData>> ReadSelectionAsync(InputSizeLimits? limits = null, CancellationToken cancellationToken = default)
        { Reads++; return Task.FromResult(new WorkbookResult<SpreadsheetRangeData>(ReadCode, ReadCode == "workbook.ok" ? Range : null)); }
        public Task<WriteResult> WriteAsync(OfficeOutputPlan plan, WriteIntent intent, WriteConsent? consent = null, CancellationToken cancellationToken = default)
        {
            Writes++; Plan = plan; Consent = consent; Intent = intent;
            BeforeWriteReturn?.Invoke();
            if (PendingWrite is not null) return PendingWrite.Task;
            if (Uncertain) return Task.FromResult(new WriteResult(OutputWriteState.PartiallyWritten, WriteCodes.MutationUncertain, "Unverified", intent, true));
            return Task.FromResult(consent?.Approved == false ? new WriteResult(OutputWriteState.CancelledByUser, WriteCodes.Cancelled, "Declined", intent, false, confirmationRequired: true, overwriteApproved: false)
                : new WriteResult(OutputWriteState.Completed, WriteCodes.Completed, "Written", intent, true, intent.Regions.Select(r => r.ArtifactId), consent is not null, consent?.Approved));
        }
        public Task<WorkbookResult<HostCapabilitySnapshot>> GetCapabilitiesAsync(CancellationToken cancellationToken = default) => throw new AssertFailedException();
        public Task<WorkbookResult<SelectedRangeMetadata>> InspectSelectionAsync(CancellationToken cancellationToken = default) => throw new AssertFailedException();
        public Task<WorkbookResult<SpreadsheetRangeData>> ReadRangeAsync(SpreadsheetRangeReference reference, InputSizeLimits? limits = null, CancellationToken cancellationToken = default)
        {
            if (ResolveRange is null) return ReadSelectionAsync(limits, cancellationToken);
            Reads++; return Task.FromResult(new WorkbookResult<SpreadsheetRangeData>(ReadCode, ReadCode == "workbook.ok" ? ResolveRange(reference) : null));
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
    internal static JsonDocument Fixture(string name)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BESHStatNG.sln"))) directory = directory.Parent;
        return JsonDocument.Parse(File.ReadAllText(Path.Combine(directory!.FullName, "BESHStat.Excel.Common.Tests/TestData/Stage304", name + ".json")));
    }
    internal static SpreadsheetRangeData Range(JsonElement fixture)
    {
        var names = fixture.GetProperty("groups").GetProperty("names").EnumerateArray().Select(v => v.GetString()!).ToArray();
        var groups = fixture.GetProperty("groups").GetProperty("values").EnumerateArray().Select(g => g.EnumerateArray().Select(v => v.GetDouble()).ToArray()).ToArray();
        var result = new SpreadsheetRangeData { WorksheetId = "sheet-1", WorksheetName = "Input", Address = $"A1:C{groups.Max(g => g.Length) + 1}",
            StartRow = 1, StartColumn = 1, RowCount = groups.Max(g => g.Length) + 1, ColumnCount = names.Length };
        result.Rows.Add(names.Select(n => new SpreadsheetCellValue { Kind = SpreadsheetCellKind.Text, TextValue = n }).ToList());
        for (int r = 0; r < result.RowCount - 1; r++) result.Rows.Add(groups.Select(g => r < g.Length ? new SpreadsheetCellValue { Kind = SpreadsheetCellKind.Number, NumberValue = g[r] } : new SpreadsheetCellValue()).ToList());
        return result;
    }
    internal static AnalysisOptionDraft Draft(JsonElement fixture)
    {
        var d = AnalysisCatalog.GetById(fixture.GetProperty("analysisIdProposed").GetString()!);
        var options = fixture.GetProperty("options");
        bool Flag(string key) => options.TryGetProperty(key, out var v) && v.GetBoolean();
        if (d.Id == AnalysisIds.OneWayAnova) return new AnovaOptionDraft(new() { IncludeWelch = Flag("welch"), IncludeFisherLsd = Flag("fisherLsd"),
            IncludeBonferroni = Flag("bonferroni"), IncludeTukeyKramer = Flag("tukeyKramer"), IncludeGamesHowell = Flag("gamesHowell") });
        return new KruskalWallisOptionDraft(new() { IncludeDunnComparisons = Flag("dunn") });
    }

    [DataTestMethod]
    [DataRow("anova-base")] [DataRow("anova-welch-tukey")] [DataRow("anova-all-posthoc")] [DataRow("kw-base")] [DataRow("kw-dunn")]
    public async Task RealPilotMatchesGoldenMetricsAndCompleteOrderedTableDimensions(string name)
    {
        using var fixture = Fixture(name); var root = fixture.RootElement;
        using var draft = Draft(root); var workbook = new Workbook(Range(root)); var host = new Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(root.GetProperty("analysisIdProposed").GetString()!), draft, workbook, host);
        Assert.AreEqual(0, workbook.Reads); Assert.IsFalse(state.Current);
        await state.AcquireAsync(); Assert.IsTrue(state.CanRun); Assert.AreEqual(0, workbook.Writes);
        await state.RunAsync(); Assert.AreEqual("analysis.completed", state.Code);
        foreach (var metric in root.GetProperty("metrics").EnumerateObject())
            Assert.AreEqual(metric.Value.GetDouble(), state.Metrics.Single(m => m.Name == metric.Name).Value, 1e-10, metric.Name);
        var expected = root.GetProperty("tables").EnumerateArray().ToArray();
        Assert.AreEqual(expected.Length, state.Tables.Count);
        for (int i = 0; i < expected.Length; i++)
        { Assert.AreEqual($"table-{i}", state.Tables[i].Id); Assert.AreEqual(expected[i].GetProperty("rows").GetInt32(), state.Tables[i].Rows); Assert.AreEqual(expected[i].GetProperty("columns").GetInt32(), state.Tables[i].Columns); }
        await state.InspectAsync(); Assert.IsTrue(state.CanWrite); Assert.AreEqual(0, workbook.Writes);
        await state.WriteAsync(); Assert.AreEqual(OutputWriteState.Completed, state.WriteOutcome!.State);
        Assert.AreEqual(expected.Length, workbook.Plan!.Tables.Count); Assert.IsTrue(workbook.Plan.Tables.All(t => !t.AutoFit));
        Assert.IsTrue(workbook.Plan.Tables.SelectMany(t => t.Cells).Where(c => c.IsPValue).All(c => c.NumberFormat == workbook.Plan.PvalueNumberFormat));
        Assert.AreEqual(1, workbook.Writes); Assert.IsFalse(state.CanWrite);
        await state.WriteAsync(); Assert.AreEqual(1, workbook.Writes);
    }

    [DataTestMethod]
    [DataRow("text")] [DataRow("error")] [DataRow("boolean")] [DataRow("header")] [DataRow("ragged")] [DataRow("one-group")] [DataRow("empty-group")]
    public async Task InvalidGroupedInputRetainsSafeIssueLocationsWithoutRunning(string defect)
    {
        using var f = Fixture("anova-base"); var r = Range(f.RootElement);
        if (defect == "text") r.Rows[1][0] = new() { Kind = SpreadsheetCellKind.Text, TextValue = "SECRET" };
        if (defect == "error") r.Rows[1][0] = SpreadsheetCellValue.ErrorCell("#N/A");
        if (defect == "boolean") r.Rows[1][0] = SpreadsheetCellValue.BooleanCell(true);
        if (defect == "header") r.Rows[0] = r.Rows[0].Select(_ => new SpreadsheetCellValue()).ToList();
        if (defect == "ragged") r.Rows[1].RemoveAt(0);
        if (defect == "one-group") { r.ColumnCount = 1; r.Rows = r.Rows.Select(row => row.Take(1).ToList()).ToList(); }
        if (defect == "empty-group") for (int i = 1; i < r.Rows.Count; i++) r.Rows[i][0] = new();
        var host = new Host(); using var draft = new AnovaOptionDraft(new());
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, new Workbook(r), host);
        await state.AcquireAsync(); Assert.IsFalse(state.Current); Assert.IsFalse(state.Preview!.Valid);
        Assert.IsTrue(state.Preview.Issues.Count > 0); Assert.AreEqual(0, host.Pins);
        Assert.IsFalse(JsonSerializer.Serialize(state.Preview).Contains("SECRET"));
        await state.RunAsync(); Assert.AreEqual(0, state.Tables.Count);
    }

    [DataTestMethod]
    [DataRow("input.range-cells")] [DataRow("input.invalid-selection")] [DataRow("input.time-budget")] [DataRow("user.document-changed")]
    public async Task AcquisitionFailuresCannotEnableRun(string code)
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var host = new Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, new Workbook(Range(f.RootElement)) { ReadCode = code }, host);
        await state.AcquireAsync(); Assert.AreEqual(code, state.Code); Assert.IsFalse(state.Current); Assert.AreEqual(0, host.Pins);
    }

    [DataTestMethod]
    [DataRow("0")] [DataRow("NaN")] [DataRow("0,05")] [DataRow(".1")]
    public async Task AnyOptionChangeInvalidatesInputResultsAndConsentWithoutReading(string alpha)
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var workbook = new Workbook(Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, new Host());
        await state.AcquireAsync(); await state.RunAsync(); await state.InspectAsync();
        draft.SetAlpha(alpha); Assert.IsFalse(state.Current); Assert.IsFalse(state.CanWrite); Assert.IsNull(state.Inspection);
        Assert.AreEqual(2, workbook.Reads); Assert.AreEqual(0, state.Tables.Count);
        await state.RunAsync(); Assert.AreEqual(0, workbook.Writes);
    }

    [DataTestMethod]
    [DataRow("A0")] [DataRow("XFE1")] [DataRow("A1:B2")] [DataRow("'[Other.xlsx]Other'!A1")] [DataRow("J1")]
    public async Task InvalidStartCellRejectsBeforeHostInspection(string cell)
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var host = new Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, new Workbook(Range(f.RootElement)), host);
        await state.AcquireAsync(); await state.RunAsync(); state.SetDestination(SpreadsheetOutputDestination.CurrentWorksheet, cell, "Results");
        await state.InspectAsync(); Assert.IsFalse(state.CanWrite); Assert.AreEqual(0, host.Inspections);
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task CommonConsentIsPerInspectionAndExplicit(bool approve)
    {
        using var f = Fixture("kw-dunn"); using var draft = Draft(f.RootElement); var workbook = new Workbook(Range(f.RootElement)); var host = new Host { Occupancy = OutputOccupancy.Occupied };
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.KruskalWallis), draft, workbook, host);
        await state.AcquireAsync(); await state.RunAsync(); await state.InspectAsync();
        await state.WriteAsync(); Assert.AreEqual(0, workbook.Writes); // No implicit approval.
        await state.WriteAsync(approve); Assert.AreEqual(approve, workbook.Consent!.Approved);
        Assert.AreSame(workbook.Intent, workbook.Consent.Inspection.Intent);
        Assert.AreEqual(approve ? OutputWriteState.Completed : OutputWriteState.CancelledByUser, state.WriteOutcome!.State);
    }

    [TestMethod]
    public async Task NewSheetAndDestinationChangePreserveDraftAndResultButClearConsent()
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var workbook = new Workbook(Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, new Host());
        await state.AcquireAsync(); await state.RunAsync(); await state.InspectAsync();
        state.SetDestination(SpreadsheetOutputDestination.NewWorksheet, "B7", "Results");
        Assert.IsNull(state.Inspection); Assert.IsTrue(state.Current); Assert.AreEqual(1, state.Tables.Count); Assert.AreEqual("0.05", draft.AlphaText);
        await state.InspectAsync(); await state.WriteAsync(); Assert.AreEqual(SpreadsheetOutputDestination.NewWorksheet, workbook.Intent!.Destination);
        Assert.IsNull(workbook.Intent.WorksheetIdentity);
    }

    [TestMethod]
    public async Task StaleCheckCannotRunAndDisposalIgnoresLatePinCompletion()
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var host = new Host { CheckCode = "user.document-changed" };
        var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, new Workbook(Range(f.RootElement)), host);
        await state.AcquireAsync(); await state.RunAsync(); Assert.IsFalse(state.Current); Assert.AreEqual(0, state.Tables.Count);
        host.Pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = state.AcquireAsync(); Assert.IsTrue(state.Busy); await state.AcquireAsync(); Assert.AreEqual("host.busy", state.Code);
        await state.DisposeAsync(); host.Pending.SetResult(new() { Code = "workbook.ok", SourceToken = "late", SessionIdentity = "late" });
        await pending; Assert.IsFalse(state.Current); Assert.IsFalse(state.CanRun);
    }

    [TestMethod]
    public async Task HostEventDisablesRunAndConsentWithoutAcquiringCellsOrResettingOptions()
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new() { IncludeWelch = true });
        var host = new Host(); var workbook = new Workbook(Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        await state.AcquireAsync(); await state.RunAsync(); await state.InspectAsync();
        host.ChangeDocument(); Assert.IsFalse(state.Current); Assert.IsFalse(state.CanWrite);
        Assert.IsTrue(draft.IncludeWelch); Assert.AreEqual(2, workbook.Reads); Assert.AreEqual(0, workbook.Writes);
    }

    [TestMethod]
    public async Task IncompatibleDraftCannotInvokeCommonExecutionOrHostRunCheck()
    {
        using var f = Fixture("anova-base"); using var draft = new KruskalWallisOptionDraft(new()); var host = new Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, new Workbook(Range(f.RootElement)), host);
        await state.AcquireAsync(); Assert.IsFalse(state.Current); await state.RunAsync(); Assert.AreEqual(0, host.Checks); Assert.AreEqual(0, state.Tables.Count);
        Assert.AreEqual("analysis.invalid-request", state.Code);
    }

    [TestMethod]
    public async Task RefreshOwnsIndependentSourceAndClearsOldResultsAndConsent()
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var host = new Host();
        var workbook = new Workbook(Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        await state.AcquireAsync(); await state.RunAsync(); await state.InspectAsync();
        workbook.Range = Range(f.RootElement); workbook.Range.WorksheetId = "same-name-new-id";
        await state.AcquireSelectionAsync(); Assert.IsNull(state.Inspection); Assert.AreEqual(0, state.Tables.Count); Assert.AreEqual(3, host.Pins);
        await state.RunAsync(); await state.InspectAsync(); await state.WriteAsync();
        Assert.AreEqual("same-name-new-id", workbook.Intent!.WorksheetIdentity);
    }

    [DataTestMethod]
    [DataRow(SpreadsheetOutputDestination.CurrentWorksheet)] [DataRow(SpreadsheetOutputDestination.NewWorksheet)]
    public async Task CompletedWriteThenDifferentSourceHasFreshIntentConsentAndSession(SpreadsheetOutputDestination destination)
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var host = new Host { Occupancy = OutputOccupancy.Occupied }; var workbook = new Workbook(Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        await state.AcquireAsync(); await state.RunAsync(); state.SetDestination(destination, destination == SpreadsheetOutputDestination.NewWorksheet ? "J1" : "'Input'!J1", "First result");
        await state.InspectAsync(); await state.WriteAsync(true);
        var firstIntent = workbook.Intent!; var firstConsent = workbook.Consent!;
        Assert.AreEqual(OutputWriteState.Completed, state.WriteOutcome!.State); Assert.IsFalse(state.Current);
        workbook.Range = Range(f.RootElement); workbook.Range.WorksheetId = "source-b"; workbook.Range.WorksheetName = "Second input";
        await state.InputReference.CaptureAsync(); // Read-only capture alone keeps the previous outcome visible.
        Assert.AreEqual(OutputWriteState.Completed, state.WriteOutcome.State);
        await state.AcquireSelectionAsync(); await state.RunAsync();
        state.SetDestination(SpreadsheetOutputDestination.CurrentWorksheet, "'Second input'!J1", "Second result");
        await state.InspectAsync(); await state.WriteAsync(true);
        Assert.AreEqual(2, workbook.Writes); Assert.AreEqual(4, host.Pins);
        Assert.AreNotEqual(firstIntent.WorkbookIdentity, workbook.Intent!.WorkbookIdentity);
        Assert.AreNotSame(firstIntent, workbook.Intent); Assert.AreNotSame(firstConsent, workbook.Consent);
        Assert.AreEqual("source-b", workbook.Intent.WorksheetIdentity); Assert.IsFalse(state.RequiresRecovery);
    }

    [TestMethod]
    public async Task UncertainOutcomeSurvivesEditingCancelRetryAndCannotAcquireOrWriteAgain()
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var workbook = new Workbook(Range(f.RootElement)) { Uncertain = true };
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, new Host());
        await state.AcquireAsync(); await state.RunAsync(); await state.InspectAsync(); await state.WriteAsync();
        var outcome = state.WriteOutcome;
        Assert.AreEqual(OutputWriteState.PartiallyWritten, outcome!.State); Assert.IsTrue(state.RequiresRecovery);
        state.InputReference.Edit("'Input'!B1:C4"); state.Cancel(); state.Invalidate();
        await state.AcquireSelectionAsync(); await state.RunAsync(); await state.InspectAsync(); await state.WriteAsync();
        Assert.AreSame(outcome, state.WriteOutcome); Assert.AreEqual(1, workbook.Writes); Assert.AreEqual(2, workbook.Reads);
        Assert.IsFalse(state.Current); Assert.IsFalse(state.CanRun); Assert.IsFalse(state.CanWrite);
        StringAssert.Contains(state.Guidance, "wait for the previous native operation to finish");
        StringAssert.Contains(state.Guidance, "Close and reopen");
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task VerifiedWriteIsRecordedEvenAfterLateCancelOrHostInvalidation(bool hostEvent)
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var workbook = new Workbook(Range(f.RootElement)); var host = new Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        await state.AcquireAsync(); await state.RunAsync(); await state.InspectAsync();
        workbook.BeforeWriteReturn = hostEvent ? host.ChangeDocument : state.Cancel;
        await state.WriteAsync();
        Assert.AreEqual(OutputWriteState.Completed, state.WriteOutcome!.State); Assert.AreEqual("output.completed", state.Code);
        Assert.IsTrue(state.WriteOutcome.ConfirmedWritten.Count > 0); Assert.IsFalse(state.Current);
    }

    [TestMethod]
    public async Task TrueBlanksAreSkippedButEmptyTextRemainsAnError()
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var range = Range(f.RootElement);
        range.Rows[1][0] = SpreadsheetCellValue.BlankCell(); var workbook = new Workbook(range);
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, new Host());
        await state.AcquireAsync(); Assert.IsTrue(state.CanRun); Assert.AreEqual(2, state.Preview!.GroupCounts[0]);
        range.Rows[1][0] = SpreadsheetCellValue.TextCell(""); await state.AcquireAsync();
        Assert.IsFalse(state.Current); Assert.IsTrue(state.Preview!.Issues.Count > 0);
    }

    [DataTestMethod]
    [DataRow("new-workbook")] [DataRow("off-grid-layout")] [DataRow("unknown-occupancy")]
    public async Task UnsupportedOrUnverifiedOutputCannotWrite(string defect)
    {
        using var f = Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); var host = new Host(); var workbook = new Workbook(Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        await state.AcquireAsync(); await state.RunAsync();
        if (defect == "new-workbook") state.SetDestination(SpreadsheetOutputDestination.NewWorkbook, "J1", "Results");
        if (defect == "off-grid-layout") state.SetDestination(SpreadsheetOutputDestination.CurrentWorksheet, "'Input'!XFD1048576", "Results");
        if (defect == "unknown-occupancy") host.Occupancy = OutputOccupancy.Unknown;
        await state.InspectAsync(); Assert.IsFalse(state.CanWrite); await state.WriteAsync(); Assert.AreEqual(0, workbook.Writes);
    }
}
