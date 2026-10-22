using Microsoft.JSInterop;

namespace BESHStat.OfficeAddin.Infrastructure;

public interface IOfficeHostReadiness : IAsyncDisposable
{
    Task<HostStatus> InitializeAsync(CancellationToken cancellationToken = default, bool retry = false);
}

// Scoped to this pane. This is bootstrap readiness, not a workbook/execution lifecycle.
public sealed class OfficeHostReadiness(IJSRuntime js) : IOfficeHostReadiness
{
    private readonly CancellationTokenSource lifetime = new();
    private readonly object gate = new();
    private Task<HostStatus>? startup;
    private IJSObjectReference? module;
    private IJSObjectReference? adapter;
    private bool disposed;

    public async Task<HostStatus> InitializeAsync(CancellationToken cancellationToken = default, bool retry = false)
    {
        if (cancellationToken.IsCancellationRequested) return HostStatus.Disposed;
        Task<HostStatus> pending;
        lock (gate)
        {
            if (disposed) return HostStatus.Disposed;
            if (retry && startup?.IsCompleted == true) startup = null;
            pending = startup ??= StartCoreAsync(retry);
        }
        try { return await pending.WaitAsync(cancellationToken); }
        catch (OperationCanceledException) { return HostStatus.Disposed; }
    }

    private async Task<HostStatus> StartCoreAsync(bool retry)
    {
        using var bound = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        bound.CancelAfter(TimeSpan.FromSeconds(12));
        var failureStage = "bootstrap.interop-import-failed";
        try
        {
            module ??= await js.InvokeAsync<IJSObjectReference>("import", bound.Token, "./js/officeHost.mjs");
            bound.Token.ThrowIfCancellationRequested();
            failureStage = "bootstrap.interop-create-failed";
            adapter ??= await module.InvokeAsync<IJSObjectReference>("create", bound.Token);
            bound.Token.ThrowIfCancellationRequested();
            failureStage = "bootstrap.interop-invoke-failed";
            var reply = await adapter.InvokeAsync<HostReply>("initialize", bound.Token, retry);
            return lifetime.IsCancellationRequested ? HostStatus.Disposed : HostStatus.Project(reply);
        }
        catch (OperationCanceledException)
        {
            if (!lifetime.IsCancellationRequested) await RecordFailureAsync(failureStage);
            return lifetime.IsCancellationRequested ? HostStatus.Disposed : new(HostState.Failed, "office.timeout");
        }
        catch (Exception)
        {
            // Interop errors may contain document URLs or response bodies. Never render/log them.
            await RecordFailureAsync(failureStage);
            return HostStatus.Failed;
        }
    }

    private async Task RecordFailureAsync(string code)
    {
        if (lifetime.IsCancellationRequested) return;
        using var bound = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        bound.CancelAfter(TimeSpan.FromSeconds(1));
        try { await js.InvokeVoidAsync("__beshOfficeDebug.record", bound.Token, code); }
        catch (Exception) { /* Optional diagnostics never change the public status. */ }
    }

    public async ValueTask DisposeAsync()
    {
        Task<HostStatus>? pending;
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            lifetime.Cancel();
            pending = startup;
        }
        // Drain cancellation before releasing references, including an import/create race.
        if (pending is not null) await pending;
        using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        if (adapter is not null)
        {
            try { await adapter.InvokeVoidAsync("dispose", cleanup.Token); } catch (Exception) { }
            try { await adapter.DisposeAsync().AsTask().WaitAsync(cleanup.Token); } catch (Exception) { }
        }
        if (module is not null)
        {
            try { await module.DisposeAsync().AsTask().WaitAsync(cleanup.Token); } catch (Exception) { }
        }
        lifetime.Dispose();
    }
}
