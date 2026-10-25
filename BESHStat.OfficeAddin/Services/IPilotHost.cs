using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>Private host protocol, separate from Common wire/physical workbook identities.</summary>
public sealed class PilotReply
{
    public string Code { get; set; } = "host.invalid-response";
    public string? SourceToken { get; set; }
    public string? SessionIdentity { get; set; }
    public OutputOccupancy State { get; set; } = OutputOccupancy.Unknown;
    public string? ObservationIdentity { get; set; }
    public List<string> OccupiedRegionIds { get; set; } = [];
    public bool HasValues { get; set; }
    public bool HasFormulas { get; set; }
    public PilotTargetFacts? Facts { get; set; }
}

public sealed class PilotTargetFacts
{
    public bool? WorkbookAvailable { get; set; }
    public bool? SheetExists { get; set; }
    public bool? IdentityUnambiguous { get; set; }
    public bool? Writable { get; set; }
    public bool? ProtectedSheet { get; set; }
    public bool? MergedConflict { get; set; }
    public bool? CanCreateWorksheet { get; set; }
    public bool? WorksheetNameValid { get; set; }
    public bool? NewWorksheetNameCollision { get; set; }
    public OutputTargetFacts ToCommon() => new(WorkbookAvailable, SheetExists, IdentityUnambiguous, Writable, ProtectedSheet,
        MergedConflict, CanCreateWorksheet, worksheetNameValid: WorksheetNameValid, newWorksheetNameCollision: NewWorksheetNameCollision);
    public bool Verified(SpreadsheetOutputDestination destination) => WorkbookAvailable == true && IdentityUnambiguous == true && Writable == true &&
        ProtectedSheet == false && MergedConflict == false && WorksheetNameValid == true &&
        (destination == SpreadsheetOutputDestination.CurrentWorksheet ? SheetExists == true : CanCreateWorksheet == true && NewWorksheetNameCollision == false);
}

public interface IPilotHost
{
    event Action? Invalidated;
    Task<PilotReply?> PinSourceAsync(SpreadsheetRangeData snapshot, CancellationToken token);
    Task<PilotReply?> CheckSourceAsync(string sourceToken, CancellationToken token);
    Task<PilotReply?> PrepareOutputAsync(OfficeOutputPlan plan, WriteIntent intent, string sourceToken, CancellationToken token);
    Task InvalidateAsync();
    Task InvalidateOutputAsync();
}

/// <summary>Host-scoped composite lease; each member is a real physical range and resolved handle.</summary>
public sealed record PhysicalSource(SpreadsheetRangeData Snapshot, string Handle);
public interface ICompositeSourceHost
{
    Task<PilotReply?> PinSourcesAsync(IReadOnlyList<PhysicalSource> sources, CancellationToken token);
}

/// <summary>Host-scoped allocation, before Common plans an exact physical target. Names stay local to UI.</summary>
public sealed class WorksheetNameReply
{
    public string Code { get; set; } = "host.invalid-response";
    public string? RequestedName { get; set; }
    public string? ResolvedName { get; set; }
    public string? WarningCode { get; set; }
}
public interface IWorksheetNameHost
{
    Task<WorksheetNameReply?> ResolveNewWorksheetNameAsync(string requestedName, string sourceToken, CancellationToken token);
}
public sealed record WorksheetNameNotice(string RequestedName, string ResolvedName, string Code, bool Created = false);

/// <summary>Pane-scoped display of the native writer latch. Navigation and form reset cannot release it.</summary>
public interface IPilotSafetyHost
{
    bool RequiresRecovery { get; }
    WriteResult? LastWriteOutcome { get; }
    void RecordWriteOutcome(WriteResult outcome);
}
