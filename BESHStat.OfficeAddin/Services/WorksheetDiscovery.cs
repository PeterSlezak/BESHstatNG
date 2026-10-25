using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public sealed record SourceWorksheet(string Id, string Name, string Visibility);
public sealed record WorksheetCatalog(string DocumentIdentity, string ActiveWorksheetId, IReadOnlyList<SourceWorksheet> Worksheets);
public sealed class WorksheetCatalogReply
{
    public string Code { get; set; } = "host.invalid-response";
    public string? DocumentIdentity { get; set; }
    public string? ActiveWorksheetId { get; set; }
    public List<SourceWorksheet>? Worksheets { get; set; }
}
public sealed class WorksheetBoundsReply
{
    public string Code { get; set; } = "host.invalid-response";
    public string? DocumentIdentity { get; set; }
    public bool? Empty { get; set; }
    public SpreadsheetRangeReference? Reference { get; set; }
}

/// <summary>Private Office read-only metadata API. Worksheet IDs are scoped to DocumentIdentity.</summary>
public interface IWorksheetDiscoveryHost
{
    event Action<string, string>? WorksheetsInvalidated;
    Task<WorksheetCatalogReply?> GetWorksheetsAsync(string owner, CancellationToken token);
    Task<WorksheetBoundsReply?> GetWorksheetBoundsAsync(string owner, string documentIdentity, string worksheetId, CancellationToken token);
    Task StopWorksheetDiscoveryAsync(string owner);
}

public static class WorksheetDiscoveryValidation
{
    public static string Safe(string? code) => code is "workbook.ok" or "user.cancelled" or "user.document-changed" or
        "office.unavailable" or "office.unsupported-api" or "host.busy" or "input.time-budget" or "host.invalid-response" or
        "input.range-cells" or "input.range-count" or "input.estimated-bytes" or "spec.column-limit" or "spec.sheet-limit" ? code : "host.operation-failed";
    private static bool Text(string? value, int maximum) => !string.IsNullOrWhiteSpace(value) && value.Length <= maximum && !value.Any(char.IsControl);
    public static WorksheetCatalog Catalog(WorksheetCatalogReply reply)
    {
        if (reply.Code != "workbook.ok" || !Text(reply.DocumentIdentity, 128) || reply.Worksheets is not { Count: > 0 and <= 256 } sheets ||
            sheets.Any(s => s is null || !Text(s.Id, 1024) || !Text(s.Name, 256) || s.Visibility is not ("Visible" or "Hidden" or "VeryHidden")) ||
            sheets.Select(s => s.Id).Distinct(StringComparer.Ordinal).Count() != sheets.Count ||
            !sheets.Any(s => s.Id == reply.ActiveWorksheetId)) throw new InvalidDataException();
        return new(reply.DocumentIdentity!, reply.ActiveWorksheetId!, Array.AsReadOnly(sheets.ToArray()));
    }
    public static SpreadsheetRangeReference? Bounds(WorksheetBoundsReply reply, WorksheetCatalog catalog, SourceWorksheet sheet)
    {
        if(reply.DocumentIdentity!=catalog.DocumentIdentity || reply.Reference is { } physical &&
            (physical.WorksheetId!=sheet.Id || physical.WorksheetName!=sheet.Name)) throw new WorkbookTransportException("user.document-changed");
        if (reply.Code != "workbook.ok" || !reply.Empty.HasValue) throw new InvalidDataException();
        if (reply.Empty.Value) { if (reply.Reference is not null) throw new InvalidDataException(); return null; }
        var r = reply.Reference ?? throw new InvalidDataException();
        var parsed = SpreadsheetRangeAddress.Parse(r.Address);
        if (r.WorksheetId != sheet.Id || r.WorksheetName != sheet.Name || r.StartRow != 1 ||
            r.WorkbookId.Length != 0 || r.WorkbookName.Length != 0 || r.AddressDialect != SpreadsheetAddressDialect.ExcelA1 ||
            parsed.WorkbookName.Length!=0 || parsed.WorksheetName.Length!=0 || parsed.StartRow != r.StartRow || parsed.StartColumn != r.StartColumn || parsed.RowCount != r.RowCount || parsed.ColumnCount != r.ColumnCount ||
            (long)r.RowCount * r.ColumnCount > 10000 || r.ColumnCount > 128) throw new InvalidDataException();
        return r;
    }
}
