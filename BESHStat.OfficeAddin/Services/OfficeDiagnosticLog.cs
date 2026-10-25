using System.Text;
using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>Pane-local sanitized records only. No free-form message or exception API.</summary>
public sealed class OfficeDiagnosticLog : IDiagnosticSink, IDisposable
{
    public const int MaximumEvents = 200, MaximumBytes = 65536;
    private readonly object gate = new();
    private readonly Queue<(DiagnosticRecord Record, int Bytes)> records = new();
    private int bytes;
    private bool disposed;
    public event Action? Changed;
    public IReadOnlyList<DiagnosticRecord> Snapshot() { lock (gate) return records.Select(e => e.Record).ToArray(); }
    public Task EmitAsync(DiagnosticRecord record)
    {
        lock (gate) {
            if (disposed) return Task.CompletedTask;
            var length = Encoding.UTF8.GetByteCount(Line(record));
            if (length > MaximumBytes) return Task.CompletedTask;
            while (records.Count > 0 && (records.Count >= MaximumEvents || bytes + length > MaximumBytes)) bytes -= records.Dequeue().Bytes;
            records.Enqueue((record, length)); bytes += length;
        }
        if (Changed is { } observers) foreach (Action observer in observers.GetInvocationList()) try { observer(); } catch { }
        return Task.CompletedTask;
    }
    private static string Line(DiagnosticRecord r) => $"{r.Timestamp?.ToString("O", System.Globalization.CultureInfo.InvariantCulture)} office {r.RunId} {r.Severity} {r.EventCode} {r.Operation} {r.Stage}";
    public string Export(bool warningsOnly = false) => string.Join("\n", Snapshot().Where(r => !warningsOnly || r.Severity >= DiagnosticSeverity.Warning).Select(Line));
    public void Dispose() { lock (gate) { disposed = true; records.Clear(); bytes = 0; Changed = null; } }
}
