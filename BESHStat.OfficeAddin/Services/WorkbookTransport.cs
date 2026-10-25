using BESHStatNG.ExcelCommon;
using Microsoft.JSInterop;

namespace BESHStat.OfficeAddin.Services;

public sealed class WorkbookTransportException(string code) : Exception(code)
{
    public string Code { get; } = code;
}

public sealed class WorkbookTransport(IJSRuntime js, WorkbookDiagnostics? diagnostics = null) : IWorkbookTransport, IPilotHost, IRangeCaptureHost, ICompositeSourceHost, IPilotSafetyHost, IWorksheetNameHost, IWorksheetDiscoveryHost
{
    private IJSObjectReference? module, adapter;
    private readonly object gate = new();
    private Task<IJSObjectReference>? initialization;
    private Task retirement = Task.CompletedTask;
    private bool disposed;
    private string? activeSourceToken;
    private long sourceGeneration;
    private DotNetObjectReference<WorkbookTransport>? callback;
    public bool RequiresRecovery { get; private set; }
    public WriteResult? LastWriteOutcome { get; private set; }
    public void RecordWriteOutcome(WriteResult outcome)
    {
        LastWriteOutcome = outcome;
        RequiresRecovery |= outcome.State == OutputWriteState.PartiallyWritten && outcome.MutationMayHaveOccurred;
    }
    public event Action? Invalidated;
    public event Action<RangeCaptureReply>? RangeCaptured;
    public event Action<string, string>? WorksheetsInvalidated;
    [JSInvokable] public void WorksheetCatalogInvalidated(string owner, string code) { if (!disposed) WorksheetsInvalidated?.Invoke(owner, code == "user.document-changed" ? code : "spec.source-stale"); }
    [JSInvokable] public void SourceInvalidated(string sourceToken) { if (!disposed && !string.IsNullOrEmpty(activeSourceToken) && activeSourceToken == sourceToken) Invalidated?.Invoke(); }
    [JSInvokable] public void RangeCapturedCallback(RangeCaptureReply reply) { if (!disposed) RangeCaptured?.Invoke(reply); }

    private Task<IJSObjectReference> GetAsync(CancellationToken token)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (initialization is { IsCompleted: true, IsCompletedSuccessfully: false }) initialization = null;
            return initialization ??= InitializeAsync(token);
        }
    }

    private async Task<IJSObjectReference> InitializeAsync(CancellationToken token)
    {
        var stage = "host.interop-import-failed";
        IJSObjectReference? acquiredModule = null, acquiredAdapter = null;
        var retained = false;
        try
        {
            diagnostics?.Record("cs.import-started");
            acquiredModule = await js.InvokeAsync<IJSObjectReference>("import", token, "./js/workbookAdapter.mjs");
            token.ThrowIfCancellationRequested();
            diagnostics?.Record("cs.import-completed");
            stage = "host.interop-create-failed";
            diagnostics?.Record("cs.create-started");
            acquiredAdapter = await acquiredModule.InvokeAsync<IJSObjectReference>("create", token, diagnostics?.Enabled == true);
            token.ThrowIfCancellationRequested();
            diagnostics?.Record("cs.create-completed");
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                module = acquiredModule;
                adapter = acquiredAdapter;
                retained = true;
            }
            return acquiredAdapter;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception)
        {
            diagnostics?.Record(stage == "host.interop-import-failed" ? "cs.import-failed" : "cs.create-failed");
            throw new WorkbookTransportException(stage);
        }
        finally { if (!retained) await ReleaseAsync(acquiredModule, acquiredAdapter); }
    }

    private async Task<T?> InvokeAsync<T>(string method, CancellationToken token, params object?[] arguments) where T : class
    {
        Task observed;
        do { lock (gate) observed = retirement; await observed.WaitAsync(token); }
        while (!ReferenceEquals(observed, retirement));
        var target = await GetAsync(token);
        try { return await target.InvokeAsync<T?>(method, token, arguments); }
        catch (OperationCanceledException) { throw; }
        catch (Exception) { diagnostics?.Record("cs.invoke-failed"); throw new WorkbookTransportException("host.interop-invoke-failed"); }
    }

    public Task<ReadReply?> BeginReadAsync(string id, InputSizeLimits limits, SpreadsheetRangeReference? reference, bool metadataOnly, CancellationToken token)
        => InvokeAsync<ReadReply>("beginRead", token, id, limits, reference, metadataOnly);
    public Task<WorksheetCatalogReply?> GetWorksheetsAsync(string owner, CancellationToken token) =>
        InvokeAsync<WorksheetCatalogReply>("getWorksheets", token, owner, callback ??= DotNetObjectReference.Create(this));
    public Task<WorksheetBoundsReply?> GetWorksheetBoundsAsync(string owner, string documentIdentity, string worksheetId, CancellationToken token) =>
        InvokeAsync<WorksheetBoundsReply>("getWorksheetBounds", token, owner, documentIdentity, worksheetId);
    public async Task StopWorksheetDiscoveryAsync(string owner)
    {
        if (adapter is null || disposed) return;
        using var bound = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        try { await adapter.InvokeVoidAsync("stopWorksheetDiscovery", bound.Token, owner); }
        catch (Exception) { /* The component has already unsubscribed; late owner callbacks stay inert. */ }
    }
    public Task<ReadReply?> ReadNextAsync(string id, CancellationToken token) => InvokeAsync<ReadReply>("readNext", token, id);
    public Task<ReadReply?> FinishReadAsync(string id, CancellationToken token) => InvokeAsync<ReadReply>("finishRead", token, id);
    public Task<WriteReply?> WriteAsync(OfficeOutputPlan plan, WriteIntent intent, WriteConsent? consent, CancellationToken token)
    {
        activeSourceToken = null; // The write consumes this source; queued old callbacks cannot affect a later owner.
        sourceGeneration++;
        return InvokeAsync<WriteReply>("write", token, plan, intent, consent);
    }
    public Task<PilotReply?> PinSourceAsync(SpreadsheetRangeData snapshot, CancellationToken token)
        => PinAsync(snapshot, null, token);
    public Task<PilotReply?> PinReferenceSourceAsync(SpreadsheetRangeData snapshot, string handle, CancellationToken token)
        => PinAsync(snapshot, handle, token);
    public async Task<PilotReply?> PinSourcesAsync(IReadOnlyList<PhysicalSource> sources, CancellationToken token)
    {
        activeSourceToken = null;
        var version = ++sourceGeneration;
        var reply = await InvokeAsync<PilotReply>("pinSources", token, sources, callback ??= DotNetObjectReference.Create(this));
        if (!disposed && version == sourceGeneration && !token.IsCancellationRequested && reply?.Code == "workbook.ok") activeSourceToken = reply.SourceToken;
        return reply;
    }
    private async Task<PilotReply?> PinAsync(SpreadsheetRangeData snapshot, string? handle, CancellationToken token)
    {
        activeSourceToken = null;
        var version = ++sourceGeneration;
        var reply = await InvokeAsync<PilotReply>("pinSource", token, snapshot, callback ??= DotNetObjectReference.Create(this), handle);
        if (!disposed && version == sourceGeneration && !token.IsCancellationRequested && reply?.Code == "workbook.ok") activeSourceToken = reply.SourceToken;
        return reply;
    }
    public Task<RangeCaptureReply?> ResolveReferenceAsync(SpreadsheetRangeReference reference, string? handle, CancellationToken token)
        => InvokeAsync<RangeCaptureReply>("resolveReference", token, reference, handle);
    public Task<RangeCaptureReply?> CaptureSelectionAsync(bool singleCell, CancellationToken token)
        => InvokeAsync<RangeCaptureReply>("captureSelection", token, singleCell);
    public Task<RangeCaptureReply?> BeginCaptureAsync(string owner, bool output, CancellationToken token)
        => InvokeAsync<RangeCaptureReply>("beginCapture", token, owner, output, callback ??= DotNetObjectReference.Create(this));
    public Task<RangeCaptureReply?> EndCaptureAsync(string owner, bool cancel, CancellationToken token)
        => InvokeAsync<RangeCaptureReply>("endCapture", token, owner, cancel);
    public async Task<PilotReply?> CheckSourceAsync(string sourceToken, CancellationToken token)
    {
        var version = sourceGeneration;
        var reply = await InvokeAsync<PilotReply>("checkSource", token, sourceToken);
        if (!disposed && version == sourceGeneration && !token.IsCancellationRequested && reply?.Code == "workbook.ok") activeSourceToken = sourceToken;
        return reply;
    }
    public Task<PilotReply?> PrepareOutputAsync(OfficeOutputPlan plan, WriteIntent intent, string sourceToken, CancellationToken token)
        => InvokeAsync<PilotReply>("prepareOutput", token, plan, intent, sourceToken);
    public async Task<WorksheetNameReply?> ResolveNewWorksheetNameAsync(string requestedName, string sourceToken, CancellationToken token)
    {
        var reply = await InvokeAsync<WorksheetNameReply>("resolveNewWorksheetName", token, requestedName, sourceToken);
        // Existing opt-in bounded memory sink receives only the allowlisted code, never sheet names.
        try { if (reply?.WarningCode == "output.sheet-name-adjusted") diagnostics?.Record(reply.WarningCode); }
        catch (Exception) { /* Diagnostics cannot change allocation or write outcomes. */ }
        return reply;
    }
    public Task InvalidateAsync()
    {
        activeSourceToken = null;
        sourceGeneration++;
        return QueueRetirement("invalidate");
    }
    public Task InvalidateOutputAsync() => QueueRetirement("clearOutput");
    private Task QueueRetirement(string method)
    {
        lock (gate)
        {
            if (adapter is null || disposed) return Task.CompletedTask; // No initialization/read on navigation.
            return retirement = RetireAfterAsync(retirement, adapter, method);
        }
    }
    private static async Task RetireAfterAsync(Task previous, IJSObjectReference target, string method)
    {
        await previous;
        using var bound = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try { await target.InvokeVoidAsync(method, bound.Token); } catch (Exception) { }
    }

    public async Task CancelAsync(string id)
    {
        if (adapter is null) return;
        using var bound = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try { await adapter.InvokeVoidAsync("cancel", bound.Token, id); } catch (Exception) { }
    }

    public async ValueTask DisposeAsync()
    {
        Task<IJSObjectReference>? pending;
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            pending = initialization;
        }
        using var bound = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        if (pending is not null) try { await pending.WaitAsync(bound.Token); } catch (Exception) { }
        await ReleaseAsync(module, adapter);
        callback?.Dispose(); callback = null; Invalidated = null; RangeCaptured = null; WorksheetsInvalidated = null;
    }

    private static async Task ReleaseAsync(IJSObjectReference? acquiredModule, IJSObjectReference? acquiredAdapter)
    {
        using var bound = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        if (acquiredAdapter is not null)
        {
            try { await acquiredAdapter.InvokeVoidAsync("dispose", bound.Token); } catch (Exception) { }
            try { await acquiredAdapter.DisposeAsync().AsTask().WaitAsync(bound.Token); } catch (Exception) { }
        }
        if (acquiredModule is not null)
            try { await acquiredModule.DisposeAsync().AsTask().WaitAsync(bound.Token); } catch (Exception) { }
    }
}
