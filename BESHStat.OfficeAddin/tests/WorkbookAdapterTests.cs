using BESHStat.OfficeAddin.Infrastructure;
using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class WorkbookAdapterTests
{
    private static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web);
    private static string Root()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BESHStatNG.sln"))) dir = dir.Parent;
        return dir!.FullName;
    }
    private static (RangeMetadata Metadata, List<List<CellReply>> Rows) Fixture(int index = 0)
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root(), "BESHStat.Excel.Common.Tests/TestData/Acquisition/canonical-two-ranges.json")));
        var range = json.RootElement.GetProperty("Ranges")[index];
        var meta = JsonSerializer.Deserialize<RangeMetadata>(range.GetRawText(), Wire)!;
        meta.WorksheetId = "native-sheet";
        return (meta, JsonSerializer.Deserialize<List<List<CellReply>>>(range.GetProperty("Rows").GetRawText(), Wire)!);
    }

    [TestMethod]
    public async Task RegistrationsAreScopedIdempotentAndBorrowTheAcceptedReadiness()
    {
        var services = new ServiceCollection().AddScoped<IOfficeHostReadiness>(_ => new Ready()).AddScoped<IWorkbookTransport>(_ => new Transport());
        services.AddOfficeFoundation().AddOfficeFoundation();
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IWorkbookAdapter)));
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IWorkbookTransport)));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using var first = provider.CreateAsyncScope();
        await using var second = provider.CreateAsyncScope();
        var adapter = first.ServiceProvider.GetRequiredService<IWorkbookAdapter>();
        Assert.AreSame(adapter, first.ServiceProvider.GetRequiredService<IWorkbookAdapter>());
        Assert.AreNotSame(adapter, second.ServiceProvider.GetRequiredService<IWorkbookAdapter>());
        Assert.IsTrue((await adapter.GetCapabilitiesAsync()).Value!.Supports(HostCapabilityIds.ReadSelectedRange));
        Assert.IsFalse((await adapter.GetCapabilitiesAsync()).Value!.Supports(HostCapabilityIds.WriteResultTables));
        Assert.AreEqual(2, ((Ready)first.ServiceProvider.GetRequiredService<IOfficeHostReadiness>()).Calls);
    }

    [DataTestMethod]
    [DataRow(0)] [DataRow(1)]
    public async Task CanonicalCellsMapThroughTypedChunksWithoutInventedProvenance(int index)
    {
        var fake = new Transport(index);
        await using var adapter = new WorkbookAdapter(new Ready(), fake);
        var result = await adapter.ReadSelectionAsync(new InputSizeLimits(maxChunkCells: 2));
        Assert.AreEqual("workbook.ok", result.Code);
        var actual = result.Value!;
        var fixture = Fixture(index);
        Assert.AreEqual(fixture.Metadata.Address, actual.Address);
        Assert.AreEqual(fixture.Metadata.StartRow, actual.StartRow);
        Assert.AreEqual(fixture.Metadata.StartColumn, actual.StartColumn);
        Assert.AreEqual("native-sheet", actual.WorksheetId);
        Assert.AreEqual("", actual.WorkbookId);
        Assert.AreEqual("", actual.WorkbookName);
        Assert.AreEqual("", actual.SelectionId);
        Assert.AreEqual("", actual.SelectionRevision);
        Assert.AreEqual(SpreadsheetDateSystem.Unknown, actual.DateSystem);
        Assert.IsNull(actual.DateSerialOrigin);
        Assert.AreEqual(JsonSerializer.Serialize(fixture.Rows, Wire), JsonSerializer.Serialize(actual.Rows, Wire));
        Assert.AreEqual(1, fake.Cancelled);
    }

    [DataTestMethod]
    [DataRow("office.unavailable", HostState.Standalone)]
    [DataRow("office.unsupported-api", HostState.Unsupported)]
    [DataRow("office.timeout", HostState.Failed)]
    public async Task NoWorkbookCallBeforeExcelReady(string code, HostState state)
    {
        var transport = new Transport();
        await using var adapter = new WorkbookAdapter(new Ready { Status = new(state, code) }, transport);
        Assert.AreEqual(code, (await adapter.ReadSelectionAsync()).Code);
        Assert.AreEqual(0, transport.Begins);
    }

    [TestMethod]
    public async Task PublicCapabilityErrorsAreSafeAndDisposedReadinessIsNotOwned()
    {
        var host = new Ready { Throw = true };
        var adapter = new WorkbookAdapter(host, new Transport());
        Assert.AreEqual("host.operation-failed", (await adapter.GetCapabilitiesAsync()).Code);
        await adapter.DisposeAsync();
        Assert.AreEqual(0, host.Disposals);
        Assert.AreEqual("user.cancelled", (await adapter.ReadSelectionAsync()).Code);
        Assert.AreEqual("user.cancelled", (await adapter.GetCapabilitiesAsync()).Code);
        await adapter.DisposeAsync();
    }

    [TestMethod]
    public async Task InconsistentReadyStateCannotAuthorizeAWorkbookCall()
    {
        var transport = new Transport();
        await using var adapter = new WorkbookAdapter(new Ready { Status = new(HostState.Excel, "office.unavailable") }, transport);
        Assert.AreEqual("office.unavailable", (await adapter.ReadSelectionAsync()).Code);
        Assert.AreEqual(0, transport.Begins);
    }

    [TestMethod]
    public async Task MetadataQueryLoadsNoCellsAndReturnsDimensionsAndTypedReference()
    {
        var transport = new Transport();
        await using var adapter = new WorkbookAdapter(new Ready(), transport);
        var result = await adapter.InspectSelectionAsync();
        Assert.AreEqual(2, result.Value!.RowCount);
        Assert.AreEqual(4, result.Value.ColumnCount);
        Assert.AreEqual("native-sheet", result.Value.Reference.WorksheetId);
        Assert.AreEqual("C4:F5", result.Value.Reference.Address);
        Assert.AreEqual("", result.Value.Reference.WorkbookId);
        Assert.AreEqual(8L, result.Value!.CellCount);
        Assert.AreEqual(0, transport.Tiles);
    }

    [TestMethod]
    public async Task CancellationBeforeStartupDoesNotTouchEitherBoundary()
    {
        var host = new Ready(); var transport = new Transport();
        await using var adapter = new WorkbookAdapter(host, transport);
        Assert.AreEqual("user.cancelled", (await adapter.ReadSelectionAsync(cancellationToken: new(true))).Code);
        Assert.AreEqual(0, host.Calls); Assert.AreEqual(0, transport.Begins);
    }

    [TestMethod]
    public async Task CancellationBetweenTilesDiscardsPartialCopyAndAllowsRetry()
    {
        using var cancel = new CancellationTokenSource();
        var transport = new Transport { AfterTile = () => cancel.Cancel() };
        await using var adapter = new WorkbookAdapter(new Ready(), transport);
        var result = await adapter.ReadSelectionAsync(new InputSizeLimits(maxChunkCells: 2), cancel.Token);
        Assert.AreEqual("user.cancelled", result.Code); Assert.IsNull(result.Value);
        transport.AfterTile = null;
        Assert.IsTrue((await adapter.ReadSelectionAsync()).Succeeded);
    }

    [TestMethod]
    public async Task TimeoutIsBoundedEvenWhenInjectedTransportIgnoresCancellation()
    {
        var transport = new Transport { Hang = true };
        await using var adapter = new WorkbookAdapter(new Ready(), transport);
        var result = await adapter.ReadSelectionAsync(new InputSizeLimits(timeBudgetMilliseconds: 20));
        Assert.AreEqual("input.time-budget", result.Code); Assert.IsNull(result.Value);
        Assert.AreEqual(1, transport.Cancelled);
    }

    [TestMethod]
    public async Task AcquisitionTimeoutDuringReadinessIsNotMislabelledAsUserCancellation()
    {
        var transport = new Transport();
        await using var adapter = new WorkbookAdapter(new Ready { Hang = true }, transport);
        Assert.AreEqual("input.time-budget", (await adapter.ReadSelectionAsync(new InputSizeLimits(timeBudgetMilliseconds: 20))).Code);
        Assert.AreEqual(0, transport.Begins);
    }

    [TestMethod]
    public async Task ConcurrentReadIsRejectedAndDisposeCancelsOwnerWithoutDisposingReadiness()
    {
        var host = new Ready(); var transport = new Transport { Hang = true };
        var adapter = new WorkbookAdapter(host, transport);
        var first = adapter.ReadSelectionAsync();
        Assert.AreEqual("host.busy", (await adapter.ReadSelectionAsync()).Code);
        await adapter.DisposeAsync();
        Assert.AreEqual("user.cancelled", (await first).Code);
        Assert.AreEqual(1, transport.Disposals); Assert.AreEqual(0, host.Disposals);
    }

    [DataTestMethod]
    [DataRow("shape")] [DataRow("position")] [DataRow("done")] [DataRow("number")]
    [DataRow("kind")] [DataRow("code")] [DataRow("metadata")] [DataRow("sheet")]
    public async Task MalformedOrStaleReplyNeverReturnsPartialSnapshot(string failure)
    {
        var transport = new Transport { ChangeReply = reply => {
            if (failure == "shape") reply.Rows = [];
            if (failure == "position") reply.RowOffset = 9;
            if (failure == "done") reply.Done = false;
            if (failure == "number") reply.Rows![0][0].NumberValue = double.NaN;
            if (failure == "kind") reply.Rows![0][0].Kind = (SpreadsheetCellKind)99;
            if (failure == "code") reply.Code = "SECRET workbook URL";
            if (failure == "metadata") reply.Metadata!.Address = "not A1";
            if (failure == "sheet") reply.Metadata!.WorksheetId = "replacement";
        } };
        await using var adapter = new WorkbookAdapter(new Ready(), transport);
        var result = await adapter.ReadSelectionAsync();
        Assert.AreEqual(failure == "sheet" ? "user.document-changed" : "host.invalid-response", result.Code);
        Assert.IsNull(result.Value); Assert.IsFalse(result.ToString().Contains("SECRET"));
    }

    [DataTestMethod]
    [DataRow("cell")] [DataRow("metadata")] [DataRow("write")]
    public void MissingJsonFieldsCannotBecomeDefaultValidCellsOrOutcomes(string kind)
    {
        if (kind == "cell") Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<CellReply>("{}", Wire));
        if (kind == "metadata") Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<RangeMetadata>("{}", Wire));
        if (kind == "write") Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<WriteReply>("{}", Wire));
    }

    [TestMethod]
    public async Task QuotasAreRecheckedInCommonBeforeAnyTile()
    {
        var transport = new Transport();
        await using var adapter = new WorkbookAdapter(new Ready(), transport);
        Assert.AreEqual("input.range-cells", (await adapter.ReadSelectionAsync(new InputSizeLimits(maxRangeCells: 7))).Code);
        Assert.AreEqual(0, transport.Tiles);
        Assert.AreEqual("user.input-invalid", (await adapter.ReadSelectionAsync(new InputSizeLimits(maxTotalCells: 10001))).Code);
    }

    [TestMethod]
    public async Task ExplicitReferenceMustCarryVerifiableSheetAndMatchingCoordinates()
    {
        await using var adapter = new WorkbookAdapter(new Ready(), new Transport());
        var reference = SpreadsheetRangeReference.FromAddress("C4:F5", "Study Data");
        Assert.AreEqual("user.input-invalid", (await adapter.ReadRangeAsync(reference)).Code);
        reference.WorksheetId = "native-sheet";
        Assert.IsTrue((await adapter.ReadRangeAsync(reference)).Succeeded);
        reference.WorkbookId = "unknown";
        Assert.AreEqual("user.input-invalid", (await adapter.ReadRangeAsync(reference)).Code);
    }

    [DataTestMethod]
    [DataRow("import", "host.interop-import-failed")]
    [DataRow("create", "host.interop-create-failed")]
    [DataRow("beginRead", "host.interop-invoke-failed")]
    public async Task RealTransportSeparatesImportCreateInvokeFailures(string stage, string code)
    {
        var js = new Runtime { FailAt = stage };
        await using var adapter = new WorkbookAdapter(new Ready(), new WorkbookTransport(js));
        var result = await adapter.ReadSelectionAsync();
        Assert.AreEqual(code, result.Code); Assert.IsFalse(result.ToString().Contains("SECRET"));
    }

    [TestMethod]
    public async Task TransportUsesPlainFactoryOnceAndReleasesBothReferences()
    {
        var js = new Runtime();
        var transport = new WorkbookTransport(js);
        await transport.BeginReadAsync("first", new(), null, true, default);
        await transport.BeginReadAsync("second", new(), null, true, default);
        Assert.AreEqual(1, js.Calls.Count(c => c == "import"));
        Assert.AreEqual(1, js.Calls.Count(c => c == "create"));
        await transport.DisposeAsync(); await transport.DisposeAsync();
        Assert.AreEqual(2, js.Disposals); Assert.AreEqual(1, js.Calls.Count(c => c == "dispose"));
    }

    [DataTestMethod]
    [DataRow("import", 1)] [DataRow("create", 2)]
    public async Task LateImportOrFactoryCompletionAfterDisposeReleasesEveryAcquiredReference(string stage, int expected)
    {
        var js = new Runtime { BlockAt = stage, Pause = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        var transport = new WorkbookTransport(js);
        using var cancel = new CancellationTokenSource();
        var pending = transport.BeginReadAsync("late", new(), null, true, cancel.Token);
        cancel.Cancel();
        var closing = transport.DisposeAsync().AsTask();
        js.Pause.SetResult(true);
        await closing;
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await pending);
        Assert.AreEqual(expected, js.Disposals);
        Assert.IsFalse(js.Calls.Contains("beginRead"));
    }

    [TestMethod]
    public async Task FailedImportCanBeRetriedWithoutReusingTheFailedTask()
    {
        var js = new Runtime { FailAt = "import" };
        await using var transport = new WorkbookTransport(js);
        await Assert.ThrowsExceptionAsync<WorkbookTransportException>(async () => await transport.BeginReadAsync("first", new(), null, true, default));
        js.FailAt = null;
        await transport.BeginReadAsync("retry", new(), null, true, default);
        Assert.AreEqual(2, js.Calls.Count(c => c == "import"));
        Assert.AreEqual(1, js.Calls.Count(c => c == "create"));
    }

    private static (OfficeOutputPlan Plan, WriteIntent Intent) Output()
    {
        var document = new OutputDocument([new OutputTableArtifact("table", new BESHStatNG.ResultTableOutputModel(new object[,] { { 1d } }))]);
        var target = new SpreadsheetOutputTarget { Destination = SpreadsheetOutputDestination.CurrentWorksheet };
        var semantics = OutputSemanticPlanner.Create(document, null, 0.05);
        var layout = OutputLayoutPlanner.Create(document, target, semanticPlan: semantics);
        var plan = OfficeOutputPlanAdapter.Prepare(document, semantics, layout);
        return (plan, new WriteIntent("fixture-plan", "fixture-target", target, plan.Tables.Select(t => new WriteRegion(t.ArtifactId, t.Bounds)),
            workbookIdentity: "fixture-workbook", worksheetIdentity: "native-sheet"));
    }

    [TestMethod]
    public async Task SourceCallbacksAreBoundToTheCurrentPinAndRetiredByWriteAndInvalidation()
    {
        var js = new Runtime { PinReply = new() { Code = "workbook.ok", SourceToken = "first", SessionIdentity = "first" } };
        await using var transport = new WorkbookTransport(js); var invalidations = 0;
        transport.Invalidated += () => invalidations++;
        await transport.PinSourceAsync(new(), default);
        transport.SourceInvalidated("first"); Assert.AreEqual(1, invalidations);
        js.PinReply = new() { Code = "workbook.ok", SourceToken = "second", SessionIdentity = "second" };
        await transport.PinSourceAsync(new(), default);
        transport.SourceInvalidated("first"); Assert.AreEqual(1, invalidations);
        transport.SourceInvalidated("second"); Assert.AreEqual(2, invalidations);
        var output = Output(); await transport.WriteAsync(output.Plan, output.Intent, null, default);
        transport.SourceInvalidated("second"); Assert.AreEqual(2, invalidations);
        await transport.PinSourceAsync(new(), default); await transport.InvalidateAsync();
        transport.SourceInvalidated("second"); Assert.AreEqual(2, invalidations);
        transport.SourceInvalidated(null!); Assert.AreEqual(2, invalidations);
    }

    [TestMethod]
    public async Task LatePinReplyAfterInvalidationCannotRebindCallbacks()
    {
        var js = new Runtime { PinReply = new() { Code = "workbook.ok", SourceToken = "late", SessionIdentity = "late" }, BlockAt = "pinSource", Pause = new() };
        await using var transport = new WorkbookTransport(js); var invalidations = 0;
        transport.Invalidated += () => invalidations++;
        var pending = transport.PinSourceAsync(new(), default); await transport.InvalidateAsync();
        js.Pause.SetResult(true); await pending; transport.SourceInvalidated("late");
        Assert.AreEqual(0, invalidations);
    }

    [TestMethod]
    public async Task SharedTransportWaitsForPreviousOwnersRetirementBeforeFreshPinAndIgnoresOldCallbacks()
    {
        var js = new Runtime { PinReply = new() { Code = "workbook.ok", SourceToken = "first", SessionIdentity = "first" } };
        await using var transport = new WorkbookTransport(js); int invalidations = 0;
        transport.Invalidated += () => invalidations++;
        await transport.PinSourceAsync(new(), default);
        js.BlockAt = "invalidate"; js.Pause = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var retirement = transport.InvalidateAsync();
        js.PinReply = new() { Code = "workbook.ok", SourceToken = "second", SessionIdentity = "second" };
        var next = transport.PinSourceAsync(new(), default);
        Assert.AreEqual(1, js.Calls.Count(c => c == "pinSource"));
        js.Pause.SetResult(true); await retirement; await next;
        transport.SourceInvalidated("first"); Assert.AreEqual(0, invalidations);
        transport.SourceInvalidated("second"); Assert.AreEqual(1, invalidations);
    }

    [DataTestMethod]
    [DataRow(OutputWriteState.Completed, "output.completed", true)]
    [DataRow(OutputWriteState.PartiallyWritten, "output.mutation-uncertain", true)]
    [DataRow(OutputWriteState.RejectedBeforeWrite, "output.rejected", false)]
    public async Task TypedWriterPreservesCommonTerminalStates(OutputWriteState state, string code, bool mutation)
    {
        var output = Output();
        var transport = new Transport { WriteReply = new() { State = state, Code = code, MutationMayHaveOccurred = mutation,
            ConfirmedWrittenIds = state == OutputWriteState.Completed ? ["table"] : [] } };
        await using var adapter = new WorkbookAdapter(new Ready(), transport);
        var result = await adapter.WriteAsync(output.Plan, output.Intent);
        Assert.AreEqual(state, result.State); Assert.AreEqual(mutation, result.MutationMayHaveOccurred);
        Assert.IsFalse(result.RollbackAttempted); Assert.IsFalse(result.UndoSupported);
    }

    [TestMethod]
    public async Task CompletedWriteCannotBeRelabelledByLateCancellation()
    {
        using var cancel = new CancellationTokenSource();
        var output = Output();
        var transport = new Transport { BeforeWriteReturn = () => cancel.Cancel(), WriteReply = new() {
            State = OutputWriteState.Completed, Code = WriteCodes.Completed, MutationMayHaveOccurred = true, ConfirmedWrittenIds = ["table"] } };
        await using var adapter = new WorkbookAdapter(new Ready(), transport);
        Assert.AreEqual(OutputWriteState.Completed, (await adapter.WriteAsync(output.Plan, output.Intent, cancellationToken: cancel.Token)).State);
    }

    [TestMethod]
    public async Task UnverifiedWriterReplyRemainsMutationUncertain()
    {
        var output = Output();
        await using var adapter = new WorkbookAdapter(new Ready(), new Transport());
        Assert.AreEqual(OutputWriteState.PartiallyWritten, (await adapter.WriteAsync(output.Plan, output.Intent)).State);
    }

    [TestMethod]
    public async Task ContradictoryWriteStateAndCodeCannotClaimSuccess()
    {
        var output = Output();
        var transport = new Transport { WriteReply = new() { State = OutputWriteState.Completed, Code = WriteCodes.Rejected,
            MutationMayHaveOccurred = true, ConfirmedWrittenIds = ["table"] } };
        await using var adapter = new WorkbookAdapter(new Ready(), transport);
        Assert.AreEqual(OutputWriteState.PartiallyWritten, (await adapter.WriteAsync(output.Plan, output.Intent)).State);
    }

    [TestMethod]
    public async Task TargetOrRegionMismatchNeverInvokesTheMockWriter()
    {
        var output = Output();
        var target = new SpreadsheetOutputTarget { Destination = SpreadsheetOutputDestination.CurrentWorksheet };
        var wrong = new WriteIntent("different-plan", "different-target", target, [new WriteRegion("wrong", new OutputRectangle(2, 2, 2, 2))]);
        var transport = new Transport();
        await using var adapter = new WorkbookAdapter(new Ready(), transport);
        Assert.AreEqual(OutputWriteState.RejectedBeforeWrite, (await adapter.WriteAsync(output.Plan, wrong)).State);
        Assert.AreEqual(0, transport.Writes);
    }

    [TestMethod]
    public void DevelopmentStagesAreBoundedAllowlistedAndDisabledInRelease()
    {
        Assert.AreEqual(0, new WorkbookDiagnostics().Snapshot().Length);
        var trace = new WorkbookDiagnostics(true);
        trace.Record("SECRET");
        for (int i = 0; i < 100; i++) trace.Record("cs.read-started");
#if DEBUG
        Assert.IsTrue(trace.Enabled);
        Assert.AreEqual(80, trace.Snapshot().Length);
        Assert.IsTrue(trace.Snapshot().All(e => e.Code == "cs.read-started" && e.ElapsedMs >= 0));
        trace.Snapshot()[0] = new("SECRET", 0);
        Assert.AreEqual("cs.read-started", trace.Snapshot()[0].Code);
#else
        Assert.IsFalse(trace.Enabled);
        Assert.AreEqual(0, trace.Snapshot().Length);
#endif
    }

    [DataTestMethod]
    [DataRow("begin", "cs.begin-started")]
    [DataRow("tile", "cs.tile-started")]
    [DataRow("finish", "cs.finish-started")]
    public async Task CSharpDeadlineIdentifiesStageAndRejectsLateCompletionBeforeSuccessfulRetry(string stage, string expected)
    {
        var trace = new WorkbookDiagnostics(true);
        var fake = new Transport { PendingStage = stage, Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        await using var adapter = new WorkbookAdapter(new Ready(), fake, trace);
        var result = await adapter.ReadSelectionAsync(new InputSizeLimits(timeBudgetMilliseconds: 100));
        Assert.AreEqual("input.time-budget", result.Code);
        Assert.IsNull(result.Value);
#if DEBUG
        Assert.IsTrue(trace.Snapshot().Any(e => e.Code == expected));
        Assert.AreEqual(1, trace.Snapshot().Count(e => e.Code == "cs.timeout"));
        Assert.IsFalse(trace.Snapshot().Any(e => e.Code == "cs.js-timeout-received" || e.Code == "cs.read-completed"));
#endif
        var count = trace.Snapshot().Length;
        fake.Pending.SetResult(new() { Code = "workbook.ok", Metadata = Fixture().Metadata });
        await Task.Yield();
        Assert.AreEqual(count, trace.Snapshot().Length);
        fake.PendingStage = null;
        Assert.AreEqual("workbook.ok", (await adapter.ReadSelectionAsync()).Code);
    }

    [TestMethod]
    public async Task JavaScriptBudgetReplyHasDifferentOriginFromCSharpCancellation()
    {
        var trace = new WorkbookDiagnostics(true);
        var fake = new Transport { BeginCode = "input.time-budget" };
        await using var adapter = new WorkbookAdapter(new Ready(), fake, trace);
        Assert.AreEqual("input.time-budget", (await adapter.ReadSelectionAsync()).Code);
#if DEBUG
        Assert.IsTrue(trace.Snapshot().Any(e => e.Code == "cs.js-timeout-received"));
        Assert.IsFalse(trace.Snapshot().Any(e => e.Code == "cs.timeout"));
#endif
        fake.BeginCode = "workbook.ok";
        Assert.AreEqual("workbook.ok", (await adapter.ReadSelectionAsync()).Code);
    }

    [TestMethod]
    public async Task DelayedPaneReadinessGatesTransportAndMetadataComparisonDoesNotReadTiles()
    {
        var ready = new Ready { Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        var fake = new Transport();
        await using var adapter = new WorkbookAdapter(ready, fake);
        var pending = adapter.InspectSelectionAsync();
        Assert.AreEqual(0, fake.Begins);
        ready.Pending.SetResult(ready.Status);
        Assert.AreEqual("workbook.ok", (await pending).Code);
        Assert.AreEqual(0, fake.Tiles);
        Assert.AreEqual("workbook.ok", (await adapter.ReadSelectionAsync()).Code);
        Assert.IsTrue(fake.Tiles > 0);
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task CancelOrDisposeDuringPayloadDoesNotAcceptLateResult(bool dispose)
    {
        var fake = new Transport { PendingStage = "tile", Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        using var cancellation = new CancellationTokenSource();
        var trace = new WorkbookDiagnostics(true);
        await using var adapter = new WorkbookAdapter(new Ready(), fake, trace);
        var pending = adapter.ReadSelectionAsync(cancellationToken: cancellation.Token);
        Assert.AreEqual(1, fake.Tiles);
        if (dispose) await adapter.DisposeAsync(); else cancellation.Cancel();
        Assert.AreEqual("user.cancelled", (await pending).Code);
        fake.Pending.SetResult(new() { Code = "workbook.ok", Metadata = Fixture().Metadata });
        await Task.Yield();
        Assert.IsFalse(trace.Snapshot().Any(e => e.Code == "cs.read-completed"));
        if (!dispose) { fake.PendingStage = null; Assert.AreEqual("workbook.ok", (await adapter.ReadSelectionAsync()).Code); }
    }

    private sealed class Ready : IOfficeHostReadiness
    {
        public HostStatus Status { get; set; } = new(HostState.Excel, "office.excel-ready", "OfficeOnline");
        public int Calls, Disposals;
        public bool Throw, Hang;
        public TaskCompletionSource<HostStatus>? Pending;
        public Task<HostStatus> InitializeAsync(CancellationToken cancellationToken = default, bool retry = false)
        { Calls++; if (Throw) throw new Exception("SECRET"); return Pending?.Task ?? (Hang ? new TaskCompletionSource<HostStatus>().Task : Task.FromResult(Status)); }
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
    }

    private sealed class Transport(int fixture = 0) : IWorkbookTransport
    {
        public int Begins, Tiles, Cancelled, Disposals, Writes;
        public bool Hang;
        public string? PendingStage;
        public TaskCompletionSource<ReadReply?>? Pending;
        public string BeginCode = "workbook.ok";
        public Action? AfterTile, BeforeWriteReturn;
        public Action<ReadReply>? ChangeReply;
        public WriteReply? WriteReply;
        private InputSizeLimits limits = new();
        private int row, col;
        public Task<ReadReply?> BeginReadAsync(string id, InputSizeLimits policy, SpreadsheetRangeReference? reference, bool metadataOnly, CancellationToken token)
        {
            Begins++; limits = policy; row = col = 0;
            if (PendingStage == "begin") return Pending!.Task;
            return Hang ? new TaskCompletionSource<ReadReply?>().Task : Task.FromResult<ReadReply?>(new() { Code = BeginCode, Metadata = Fixture(fixture).Metadata });
        }
        public Task<ReadReply?> ReadNextAsync(string id, CancellationToken token)
        {
            Tiles++;
            if (PendingStage == "tile") return Pending!.Task;
            var source = Fixture(fixture);
            int width = Math.Min(source.Metadata.ColumnCount, limits.MaxChunkCells), height = Math.Max(1, limits.MaxChunkCells / width);
            var rows = source.Rows.Skip(row).Take(height).Select(r => r.Skip(col).Take(width).ToList()).ToList();
            var reply = new ReadReply { Code = "workbook.ok", Metadata = source.Metadata, RowOffset = row, ColumnOffset = col, Rows = rows };
            col += width;
            if (col >= source.Metadata.ColumnCount) { col = 0; row += height; }
            reply.Done = row >= source.Metadata.RowCount;
            ChangeReply?.Invoke(reply); AfterTile?.Invoke();
            return Task.FromResult<ReadReply?>(reply);
        }
        public Task<ReadReply?> FinishReadAsync(string id, CancellationToken token) => PendingStage == "finish" ? Pending!.Task : Task.FromResult<ReadReply?>(new() { Code = "workbook.ok", Metadata = Fixture(fixture).Metadata });
        public Task CancelAsync(string id) { Cancelled++; return Task.CompletedTask; }
        public Task<WriteReply?> WriteAsync(OfficeOutputPlan plan, WriteIntent intent, WriteConsent? consent, CancellationToken token)
        { Writes++; BeforeWriteReturn?.Invoke(); return Task.FromResult(WriteReply); }
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
    }

    private sealed class Runtime : IJSRuntime, IJSObjectReference
    {
        public PilotReply? PinReply;
        public string? FailAt;
        public string? BlockAt;
        public TaskCompletionSource<bool>? Pause;
        public List<string> Calls { get; } = [];
        public int Disposals;
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, default, args);
        public async ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken token, object?[]? args)
        {
            Calls.Add(identifier);
            if (identifier == FailAt) throw new JSException("SECRET worksheet URL/values");
            if (identifier == BlockAt) await Pause!.Task;
            if (identifier is "import" or "create") return (T)(object)this;
            if (identifier == "pinSource") return (T)(object?)PinReply!;
            return default!;
        }
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
    }
}
