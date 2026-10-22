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
        services.TryAddScoped<IWorkbookAdapter, WorkbookAdapter>();
        return services;
    }
}
