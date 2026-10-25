using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;

namespace BESHStat.OfficeAddin.Services;

public interface IOfficeCommandBridge : IAsyncDisposable
{
    Task<OfficeCommandRuntime> BindAsync(Func<string, Task<string>> dispatch);
    Task ReadyAsync();
    Task ShowAboutAsync();
}
public sealed class OfficeCommandBridge(IServiceProvider services) : IOfficeCommandBridge
{
    private IJSObjectReference? module;
    private DotNetObjectReference<OfficeCommandBridge>? reference;
    private Func<string,Task<string>>? dispatch;
    private readonly string owner = Guid.NewGuid().ToString("N");
    private bool disposed;
    public async Task<OfficeCommandRuntime> BindAsync(Func<string,Task<string>> handler)
    {
        dispatch = handler;
        try {
            var js = services.GetService<IJSRuntime>(); if (js is null || disposed) return new();
            module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/ribbonCommands.mjs").AsTask().WaitAsync(TimeSpan.FromSeconds(5));
            if (disposed) { await module.DisposeAsync(); module = null; return new(); }
            reference ??= DotNetObjectReference.Create(this);
            // Dispatch only after App has installed the returned runtime capability snapshot.
            return await module.InvokeAsync<OfficeCommandRuntime>("bind", owner, reference);
        } catch { return new(); }
    }
    public async Task ReadyAsync() { if (!disposed && module is not null) try { await module.InvokeVoidAsync("ready",owner); } catch { } }
    public async Task ShowAboutAsync() { if (!disposed && module is not null) try { await module.InvokeVoidAsync("showAbout"); } catch { } }
    [JSInvokable] public Task<string> Dispatch(string id) => disposed || dispatch is null ? Task.FromResult("command.disposed") : dispatch(id);
    public async ValueTask DisposeAsync()
    {
        if (disposed) return; disposed = true; dispatch = null;
        if (module is not null) try { await module.InvokeVoidAsync("unbind",owner); await module.DisposeAsync(); } catch { }
        reference?.Dispose(); reference = null;
    }
}
