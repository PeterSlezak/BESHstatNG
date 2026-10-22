using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BESHStat.OfficeAddin.Infrastructure;

public static class FoundationServices
{
    public static IServiceCollection AddOfficeFoundation(this IServiceCollection services)
    {
        services.TryAddScoped<IOfficeHostReadiness, OfficeHostReadiness>();
        services.TryAddScoped<IFoundationProbe, FoundationProbe>();
        return services;
    }
}
