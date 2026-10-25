using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BESHStat.OfficeAddin.Services;

namespace BESHStat.OfficeAddin.Infrastructure;

public static class FoundationServices
{
    public static IServiceCollection AddOfficeFoundation(this IServiceCollection services)
    {
        services.TryAddScoped<IOfficeHostReadiness, OfficeHostReadiness>();
        services.TryAddScoped<IFoundationProbe, FoundationProbe>();
        services.TryAddScoped<WorkbookDiagnostics>(provider => new(
            provider.GetService<Microsoft.AspNetCore.Components.NavigationManager>() is { } navigation &&
            new Uri(navigation.Uri).Query.TrimStart('?').Split('&').Contains("beshOfficeDiagnostics=1")));
        services.TryAddScoped<IWorkbookTransport, WorkbookTransport>();
        services.TryAddScoped<IPilotHost>(provider => (IPilotHost)provider.GetRequiredService<IWorkbookTransport>());
        services.TryAddScoped<IWorkbookAdapter, WorkbookAdapter>();
        services.TryAddScoped<IAnalysisNavigationCatalog, AnalysisNavigationCatalog>();
        services.TryAddScoped<AnalysisEditorRegistry>();
        services.TryAddScoped<AnalysisNavigationMenu>();
        services.TryAddScoped<OfficeBrowser>();
        services.TryAddScoped<IOfficePreferenceStorage>(p => p.GetRequiredService<OfficeBrowser>());
        services.TryAddScoped<IOfficeWorkspaceBrowser>(p => p.GetRequiredService<OfficeBrowser>());
        services.TryAddScoped<BESHStatNG.ExcelCommon.IPreferenceStore, OfficePreferenceStore>();
        services.TryAddScoped<OfficePreferenceSession>();
        services.TryAddScoped<OfficeStrings>();
        services.TryAddScoped<OfficeHelp>();
        services.TryAddScoped<OfficeDiagnosticLog>();
        services.TryAddScoped<BESHStatNG.ExcelCommon.IDiagnosticSink>(p => p.GetRequiredService<OfficeDiagnosticLog>());
        services.TryAddScoped<BESHStatNG.ExcelCommon.DiagnosticEmitter>();
        return services;
    }
}
