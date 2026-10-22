using BESHStatNG.ExcelCommon;

namespace BESHStat.WasmSmoke.Infrastructure;

// Internal bridge metadata, separate from Gate B input/output JSON. ContextIdentity
// proves continuity of Office.context.document; WorkbookIdentity remains unknown.
public sealed class OfficeLifecycleBinding
{
    public string Handle { get; set; } = "";
    public string ContextIdentity { get; set; } = "";
    public string WorkbookIdentity { get; set; } = "";
    public string WorksheetIdentity { get; set; } = "";
    public string WorksheetName { get; set; } = "";
    public string SelectionRevision { get; set; } = "";
    public int EventsRegistered { get; set; }
}

// Owns a single JS lease and one guarded cleanup delegate. The JS adapter verifies
// physical sheet/context facts and volatile selection coordinates; this guard stops
// late callbacks/awaits from updating a disposed component. No synthetic revision.
public sealed class OfficeDocumentLifecycle : IAsyncDisposable
{
    public string Handle { get; }
    private readonly DocumentLifecycleLease lease;
    private readonly Func<string, Task> release;
    private bool disposed;
    public OfficeDocumentLifecycle(OfficeLifecycleBinding binding, Func<string, Task> release)
    {
        if (binding.Handle.Length is < 1 or > 128) throw new DocumentLifecycleException(DocumentInvalidationReason.Unverifiable);
        Handle = binding.Handle;
        this.release = release;
        var snapshot = new DocumentLifecycleSnapshot(binding.WorkbookIdentity, binding.WorksheetIdentity,
            binding.WorksheetName, contextIdentity: binding.ContextIdentity);
        lease = new DocumentLifecycleLease(snapshot);
        lease.EnsureCurrent(snapshot);
    }
    public bool NotifyInvalidated(string handle)
    {
        if (disposed || handle != Handle) return false;
        // JS decides whether a selection event invalidates this scope (preview or
        // pre-copy input) or is benign to an already-copied/pinned source.
        return lease.Invalidate(DocumentInvalidationReason.DocumentChanged);
    }
    public void EnsureCurrent() => lease.EnsureCurrent(lease.Binding);
    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        lease.Dispose();
        try { await release(Handle); }
        catch { /* JS guards and scope disposal already prohibit stale writes/rendering. */ }
    }
}
