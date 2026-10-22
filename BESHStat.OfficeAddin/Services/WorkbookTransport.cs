using BESHStatNG.ExcelCommon;
using Microsoft.JSInterop;

namespace BESHStat.OfficeAddin.Services;

public sealed class WorkbookTransportException(string code) : Exception(code)
{
    public string Code { get; } = code;
}

public sealed class WorkbookTransport(IJSRuntime js, WorkbookDiagnostics? diagnostics = null) : IWorkbookTransport
{
    private IJSObjectReference? module, adapter;
    private readonly object gate = new();
    private Task<IJSObjectReference>? initialization;
    private bool disposed;

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
        var target = await GetAsync(token);
        try { return await target.InvokeAsync<T?>(method, token, arguments); }
        catch (OperationCanceledException) { throw; }
        catch (Exception) { diagnostics?.Record("cs.invoke-failed"); throw new WorkbookTransportException("host.interop-invoke-failed"); }
    }

    public Task<ReadReply?> BeginReadAsync(string id, InputSizeLimits limits, SpreadsheetRangeReference? reference, bool metadataOnly, CancellationToken token)
        => InvokeAsync<ReadReply>("beginRead", token, id, limits, reference, metadataOnly);
    public Task<ReadReply?> ReadNextAsync(string id, CancellationToken token) => InvokeAsync<ReadReply>("readNext", token, id);
    public Task<ReadReply?> FinishReadAsync(string id, CancellationToken token) => InvokeAsync<ReadReply>("finishRead", token, id);
    public Task<WriteReply?> WriteAsync(OfficeOutputPlan plan, WriteIntent intent, WriteConsent? consent, CancellationToken token)
        => InvokeAsync<WriteReply>("write", token, plan, intent, consent);

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
