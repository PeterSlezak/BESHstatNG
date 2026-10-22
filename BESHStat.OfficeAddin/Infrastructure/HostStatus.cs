namespace BESHStat.OfficeAddin.Infrastructure;

public enum HostState { Standalone, Excel, Unsupported, Failed, Disposed }

// No raw Office error, host name, address, document identity or stack crosses this boundary.
public sealed record HostStatus(HostState State, string Code, string Platform = "")
{
    public static HostStatus Unavailable { get; } = new(HostState.Standalone, "office.unavailable");
    public static HostStatus Failed { get; } = new(HostState.Failed, "office.initialization-failed");
    public static HostStatus Disposed { get; } = new(HostState.Disposed, "office.cancelled");
    public bool IsExcel => State == HostState.Excel;
    public string Message => State switch
    {
        HostState.Excel => "Excel host initialized; ExcelApi 1.1 is available. Selected-range inspection is available on request.",
        HostState.Standalone => "Office host not available. Office readiness could not be established. Open the development add-in in Excel or check Office.js access and retry.",
        HostState.Unsupported => "Office host or ExcelApi requirement is unsupported. Open this add-in in a supported Excel host.",
        HostState.Disposed => "Initialization cancelled.",
        _ => "Office initialization failed or timed out. Check the development server and Office.js access, then retry."
    };

    internal static HostStatus Project(HostReply? reply)
    {
        if (reply is null) return Failed;
        var platform = reply.Platform is "PC" or "Mac" or "OfficeOnline" ? reply.Platform : "";
        return (reply.State, reply.Code, reply.OfficeReady, reply.ExcelApiSupported) switch
        {
            ("excel", "office.excel-ready", true, true) => new(HostState.Excel, "office.excel-ready", platform),
            ("standalone", "office.unavailable", false, false) => Unavailable,
            ("unsupported", "office.unsupported-host", true, false) => new(HostState.Unsupported, "office.unsupported-host", platform),
            ("unsupported", "office.unsupported-api", true, false) => new(HostState.Unsupported, "office.unsupported-api", platform),
            ("failed", "office.timeout", false, false) => new(HostState.Failed, "office.timeout"),
            _ => Failed
        };
    }
}

// Interop DTO is deliberately separate from the safe UI model.
public sealed class HostReply
{
    public string? State { get; set; }
    public string? Code { get; set; }
    public string? Platform { get; set; }
    public bool OfficeReady { get; set; }
    public bool ExcelApiSupported { get; set; }
}
