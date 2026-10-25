using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public enum OfficeCommandCapability { Utility, MethodEditor, FutureMethod, UnsupportedExport }
public sealed record OfficeCommand(string Id, string CatalogKey, string? AnalysisId, string Category, string Destination,
    OfficeCommandCapability Capability, IReadOnlyList<RibbonSource> Sources);
public sealed record OfficeCommandRuntime(bool Excel = false, bool Excel117 = false, bool AddinCommands = false, bool SharedRuntime = false);

/// <summary>One canonical identity per method; all Windows occurrences retained as provenance.</summary>
public sealed class OfficeCommandRegistry
{
    public IReadOnlyList<OfficeCommand> All { get; }
    private readonly Dictionary<string, OfficeCommand> byId;
    public OfficeCommandRegistry(AnalysisNavigationMenu menu)
    {
        var choices = menu.Groups.SelectMany(g => g.Methods).ToDictionary(m => m.Key, StringComparer.Ordinal);
        var commands = GeneratedMethodMetadata.All.Select(m => new OfficeCommand("method."+m.Key, m.Key, m.AnalysisId, m.Category, "analysis",
            choices.TryGetValue(m.Key, out var choice) && choice.CanEdit ? OfficeCommandCapability.MethodEditor : OfficeCommandCapability.FutureMethod,
            GeneratedRibbonCommands.All.Where(r => r.CatalogKey == m.Key).ToArray())).ToList();
        foreach (var (id, key, destination) in new[] { ("analysis","","analysis"), ("options","Settings","options"),
            ("diagnostics","ShowLog","diagnostics"), ("about","About","about"), ("help","Help","help"), ("export","ExportChart","export") })
            commands.Add(new(id,key,null,"Utilities",destination,id == "export" ? OfficeCommandCapability.UnsupportedExport : OfficeCommandCapability.Utility,
                GeneratedRibbonCommands.All.Where(r => r.CatalogKey == key).ToArray()));
        All = commands.AsReadOnly(); byId = commands.ToDictionary(c => c.Id, StringComparer.Ordinal);
    }
    public bool TryGet(string? id, out OfficeCommand? command) { command = null; return id is not null && id.Length <= 100 && byId.TryGetValue(id, out command); }
    public string Availability(OfficeCommand command, OfficeCommandRuntime runtime, bool ribbon)
    {
        if (command.Capability is OfficeCommandCapability.FutureMethod or OfficeCommandCapability.UnsupportedExport) return "command.unavailable";
        if (ribbon && (!runtime.Excel || !runtime.AddinCommands || !runtime.SharedRuntime)) return "command.host-unsupported";
        if (command.Capability == OfficeCommandCapability.MethodEditor && (!runtime.Excel || !runtime.Excel117)) return "command.host-unsupported";
        return "command.available";
    }
}

/// <summary>App-owned navigation, never a calculation/writer. Busy intents are rejected, not replayed.</summary>
public sealed class OfficeCommandCoordinator(OfficeCommandRegistry registry, OfficePreferenceSession preferences,
    DiagnosticEmitter diagnostics) : IDisposable
{
    private readonly List<(Func<bool> Ready, Func<OfficeCommand,Task<bool>> Navigate)> owners = [];
    private bool dispatching, disposed;
    public string Code { get; private set; } = "";
    public OfficeCommandRuntime Runtime { get; set; } = new();
    public event Action? Changed;
    public IDisposable Participate(Func<bool> ready, Func<OfficeCommand, Task<bool>> navigate)
    {
        var owner = (ready,navigate); owners.Add(owner); return new Registration(() => owners.Remove(owner));
    }
    private sealed class Registration(Action remove) : IDisposable { public void Dispose() => remove(); }
    public async Task<string> DispatchAsync(string? id, bool ribbon = false)
    {
        if (disposed) return "command.disposed";
        if (dispatching) return "command.busy";
        var known = registry.TryGet(id,out var command);
        dispatching = true;
        var run = await diagnostics.StartAsync("office", preferences.Effective.TraceEnabled);
        try {
            await run.ReportAsync("diagnostic.command.invoked", "invocation");
            Code = known ? registry.Availability(command!, Runtime, ribbon) : "command.invalid";
            if (Code == "command.available") {
                if (preferences.Applying || owners.Count == 0 || owners.Any(o => !o.Ready())) Code = "command.busy";
                else {
                    var accepted = true;
                    foreach (var owner in owners.ToArray()) { if (disposed || !await owner.Navigate(command!)) { accepted = false; break; } }
                    Code = accepted ? "command.navigated" : "command.rejected";
                }
            }
            await run.ReportAsync(Code == "command.navigated" ? "diagnostic.command.completed" : "diagnostic.command.rejected", "invocation");
            return Code;
        } catch { Code = "command.rejected"; await run.ReportAsync("diagnostic.command.rejected", "invocation"); return Code; }
        finally { dispatching = false; await run.EndAsync(); Notify(); }
    }
    private void Notify() { if (!disposed && Changed is { } observers) foreach (Action observer in observers.GetInvocationList()) try { observer(); } catch { } }
    public void Dispose() { disposed = true; owners.Clear(); Changed = null; }
}
