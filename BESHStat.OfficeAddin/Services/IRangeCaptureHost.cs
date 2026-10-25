using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public sealed class RangeCaptureReply
{
    public string Code { get; set; } = "host.invalid-response";
    public string? Owner { get; set; }
    public string? Handle { get; set; }
    public SpreadsheetRangeReference? Reference { get; set; }
    public SpreadsheetRangeReference? OriginalReference { get; set; }
}

/// <summary>Private metadata-only protocol. Common parses text before host resolution.</summary>
public interface IRangeCaptureHost
{
    event Action<RangeCaptureReply>? RangeCaptured;
    Task<RangeCaptureReply?> ResolveReferenceAsync(SpreadsheetRangeReference reference, string? handle, CancellationToken token);
    Task<RangeCaptureReply?> CaptureSelectionAsync(bool singleCell, CancellationToken token);
    Task<RangeCaptureReply?> BeginCaptureAsync(string owner, bool output, CancellationToken token);
    Task<RangeCaptureReply?> EndCaptureAsync(string owner, bool cancel, CancellationToken token);
    Task<PilotReply?> PinReferenceSourceAsync(SpreadsheetRangeData snapshot, string handle, CancellationToken token);
}
