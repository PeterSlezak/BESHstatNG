using System.Diagnostics;

namespace BESHStat.OfficeAddin.Services;

public sealed record WorkbookStageEvent(string Code, long ElapsedMs);

// Development-only, local memory. No request IDs, workbook data or exceptions.
public sealed class WorkbookDiagnostics
{
    private readonly object gate = new();
    private readonly List<WorkbookStageEvent> events = [];
    private readonly long started = Stopwatch.GetTimestamp();
    private static readonly HashSet<string> Codes = [
        "cs.read-started", "cs.metadata-started", "cs.capabilities-started", "cs.capabilities-completed",
        "cs.import-started", "cs.import-completed", "cs.create-started", "cs.create-completed",
        "cs.import-failed", "cs.create-failed", "cs.invoke-failed",
        "cs.begin-started", "cs.begin-completed", "cs.tile-started", "cs.tile-completed",
        "cs.finish-started", "cs.finish-completed", "cs.read-completed", "cs.cancelled", "cs.timeout", "output.sheet-name-adjusted",
        "cs.js-timeout-received", "cs.read-rejected", "cs.cleanup-started", "cs.cleanup-completed"
    ];
    public bool Enabled { get; }
    public WorkbookDiagnostics(bool enabled = false)
    {
#if DEBUG
        Enabled = enabled;
#endif
    }
    public void Record(string code)
    {
        if (!Enabled || !Codes.Contains(code)) return;
        lock (gate)
        {
            if (events.Count == 80) events.RemoveAt(0);
            events.Add(new(code, Math.Clamp((long)Stopwatch.GetElapsedTime(started).TotalMilliseconds, 0, 600000)));
        }
    }
    public WorkbookStageEvent[] Snapshot() { lock (gate) return events.ToArray(); }
}
