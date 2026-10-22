using BESHStatNG.ExcelCommon;
using System.Text.Json.Serialization;

namespace BESHStat.OfficeAddin.Services;

public sealed record WorkbookResult<T>(string Code, T? Value = default) where T : class
{
    public bool Succeeded => Code == "workbook.ok" && Value is not null;
}

public sealed record SelectedRangeMetadata(int RowCount, int ColumnCount, SpreadsheetRangeReference Reference)
{
    public long CellCount => (long)RowCount * ColumnCount;
}

public interface IWorkbookAdapter : IAsyncDisposable
{
    Task<WorkbookResult<HostCapabilitySnapshot>> GetCapabilitiesAsync(CancellationToken cancellationToken = default);
    Task<WorkbookResult<SelectedRangeMetadata>> InspectSelectionAsync(CancellationToken cancellationToken = default);
    Task<WorkbookResult<SpreadsheetRangeData>> ReadSelectionAsync(InputSizeLimits? limits = null, CancellationToken cancellationToken = default);
    Task<WorkbookResult<SpreadsheetRangeData>> ReadRangeAsync(SpreadsheetRangeReference reference, InputSizeLimits? limits = null, CancellationToken cancellationToken = default);
    Task<WriteResult> WriteAsync(OfficeOutputPlan plan, WriteIntent intent, WriteConsent? consent = null, CancellationToken cancellationToken = default);
}

// Host-layer transport DTOs. Never expose Office proxies or arbitrary objects.
public sealed class RangeMetadata
{
    [JsonRequired]
    public string? WorksheetId { get; set; }
    [JsonRequired]
    public string? WorksheetName { get; set; }
    [JsonRequired]
    public string? Address { get; set; }
    [JsonRequired]
    public int StartRow { get; set; }
    [JsonRequired]
    public int StartColumn { get; set; }
    [JsonRequired]
    public int RowCount { get; set; }
    [JsonRequired]
    public int ColumnCount { get; set; }
}

public sealed class CellReply
{
    [JsonRequired] public SpreadsheetCellKind Kind { get; set; }
    [JsonRequired] public double NumberValue { get; set; }
    [JsonRequired] public string? TextValue { get; set; }
    [JsonRequired] public bool BooleanValue { get; set; }
    [JsonRequired] public string? NumberFormat { get; set; }
    [JsonRequired] public string? NativeErrorCode { get; set; }
}

public sealed class ReadReply
{
    [JsonRequired] public string? Code { get; set; }
    public RangeMetadata? Metadata { get; set; }
    public int? RowOffset { get; set; }
    public int? ColumnOffset { get; set; }
    public List<List<CellReply>>? Rows { get; set; }
    public bool? Done { get; set; }
}

public sealed class WriteReply
{
    [JsonRequired]
    public OutputWriteState State { get; set; }
    [JsonRequired]
    public string? Code { get; set; }
    [JsonRequired]
    public bool MutationMayHaveOccurred { get; set; }
    [JsonRequired]
    public List<string>? ConfirmedWrittenIds { get; set; }
    [JsonRequired]
    public bool? ConfirmationRequired { get; set; }
    [JsonRequired]
    public bool? OverwriteApproved { get; set; }
}

public interface IWorkbookTransport : IAsyncDisposable
{
    Task<ReadReply?> BeginReadAsync(string operationId, InputSizeLimits limits, SpreadsheetRangeReference? reference, bool metadataOnly, CancellationToken cancellationToken);
    Task<ReadReply?> ReadNextAsync(string operationId, CancellationToken cancellationToken);
    Task<ReadReply?> FinishReadAsync(string operationId, CancellationToken cancellationToken);
    Task CancelAsync(string operationId);
    Task<WriteReply?> WriteAsync(OfficeOutputPlan plan, WriteIntent intent, WriteConsent? consent, CancellationToken cancellationToken);
}
