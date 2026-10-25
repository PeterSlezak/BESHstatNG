using Microsoft.JSInterop;
using BESHStatNG.ExcelCommon;
using Microsoft.Extensions.DependencyInjection;

namespace BESHStat.OfficeAddin.Services;

public sealed class PreferenceStorageReply
{
    public string Status { get; set; } = "unavailable";
    public PreferenceData? Data { get; set; }
    public bool? ForceEnglish { get; set; } = true;
}
public interface IOfficePreferenceStorage
{
    Task<PreferenceStorageReply> LoadAsync();
    Task<PreferenceStorageReply> SaveAsync(PreferenceData data, bool? forceEnglish = null);
}
public interface IOfficeWorkspaceBrowser
{
    Task<string?> DisplayLocaleAsync();
    Task<string> OpenHelpAsync(string safeUrl);
    Task<bool> CopyAsync(string sanitizedLog);
}

/// <summary>Bounded local browser interop; no exception text crosses the boundary.</summary>
public sealed class OfficeBrowser(IServiceProvider services) : IOfficePreferenceStorage, IOfficeWorkspaceBrowser, IAsyncDisposable
{
    private Task<IJSObjectReference?>? preferences, workspace;
    private bool disposed;
    private async Task<IJSObjectReference?> ImportAsync(string path)
    {
        try {
            var js = services.GetService<IJSRuntime>();
            return js is null || disposed ? null : await js.InvokeAsync<IJSObjectReference>("import", path).AsTask().WaitAsync(TimeSpan.FromSeconds(5));
        } catch { return null; }
    }
    private Task<IJSObjectReference?> Preferences() => preferences ??= ImportAsync("./js/preferences.mjs");
    private Task<IJSObjectReference?> Workspace() => workspace ??= ImportAsync("./js/workspaceHost.mjs");
    public async Task<PreferenceStorageReply> LoadAsync()
    {
        try { return await (await Preferences())!.InvokeAsync<PreferenceStorageReply>("loadPreferences").AsTask().WaitAsync(TimeSpan.FromSeconds(5)); }
        catch { return new(); }
    }
    public async Task<PreferenceStorageReply> SaveAsync(PreferenceData data, bool? forceEnglish = null)
    {
        try { return await (await Preferences())!.InvokeAsync<PreferenceStorageReply>("savePreferences", data, forceEnglish).AsTask().WaitAsync(TimeSpan.FromSeconds(5)); }
        catch { return new(); }
    }
    public async Task<string?> DisplayLocaleAsync()
    {
        try {
            var module = (await Workspace())!;
            await module.InvokeVoidAsync("configureHelp", GeneratedMethodMetadata.All.Select(m => m.HelpUrl)
                .Concat(GeneratedMethodMetadata.Pages.Select(m => m.HelpUrl)).Append(GeneratedMethodMetadata.HomeUrl).ToArray(),
                HelpLaunchTarget.ResolverUrl, HelpLaunchTarget.ResolverPublished,
                GeneratedMethodMetadata.All.Select(m => m.Key).Concat(GeneratedMethodMetadata.Pages.Select(m => m.Key)).Append("Home").ToArray());
            return await module.InvokeAsync<string>("displayLocale");
        } catch { return null; }
    }
    public async Task<string> OpenHelpAsync(string safeUrl)
    {
        // Defence at every call, including fallback links supplied by a component.
        safeUrl = HelpLaunchTarget.SafeOpenUrl(safeUrl);
        try { return await (await Workspace())!.InvokeAsync<string>("openHelp", safeUrl); }
        catch { return "unavailable"; }
    }
    public async Task<bool> CopyAsync(string sanitizedLog)
    {
        try { return await (await Workspace())!.InvokeAsync<bool>("copyLog", sanitizedLog); }
        catch { return false; }
    }
    public async ValueTask DisposeAsync()
    {
        disposed = true;
        foreach (var task in new[] { preferences, workspace })
            try { if (task is not null && await task is { } module) await module.DisposeAsync(); } catch { }
    }
}
