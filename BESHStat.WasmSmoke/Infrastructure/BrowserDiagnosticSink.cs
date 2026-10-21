using BESHStatNG.ExcelCommon;
using Microsoft.JSInterop;

namespace BESHStat.WasmSmoke.Infrastructure;

// No queue, storage or raw JS response logging. Common catches all delivery failures.
public sealed class BrowserDiagnosticSink(IJSRuntime js) : IDiagnosticSink
{
    public async Task EmitAsync(DiagnosticRecord record)
    {
        var delivered = await js.InvokeAsync<bool>("beshDiagnostics.emit", new
        {
            eventCode = record.EventCode, runId = record.RunId, operation = record.Operation,
            stage = record.Stage, metadata = record.Metadata
        });
        if (!delivered) throw new InvalidOperationException("Diagnostic sink unavailable.");
    }
}
