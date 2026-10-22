using BESHStatNG.ExcelCommon;
using BESHStatNG.distributions;

namespace BESHStat.OfficeAddin.Infrastructure;

public interface IFoundationProbe { ProbeResult Run(); }

public sealed record ProbeResult(bool CorePassed, double NormalCdf, bool CommonPassed,
    int DescriptorCount, int HandlerCount, string Code);

public sealed class FoundationProbe : IFoundationProbe
{
    public ProbeResult Run()
    {
        double value = double.NaN;
        bool core = false;
        int descriptors = 0, handlers = 0;
        bool common = false;
        try
        {
            value = Distributions.PNorm(0);
            core = double.IsFinite(value) && Math.Abs(value - 0.5) < 1e-14;
        }
        catch (Exception) { }
        try
        {
            var catalog = AnalysisCatalog.All;
            var registrations = AnalysisHandlerRegistry.All;
            AnalysisHandlerRegistry.ValidateRegistrations(catalog, registrations);
            descriptors = catalog.Count;
            handlers = registrations.Count;
            common = descriptors > 0 && handlers > 0 &&
                catalog.Any(d => d.Id == AnalysisIds.OneWayAnova) &&
                registrations.Any(h => h.AnalysisId == AnalysisIds.OneWayAnova);
        }
        catch (Exception) { }
        return new(core, value, common, descriptors, handlers,
            core && common ? "probe.passed" : "probe.failed");
    }
}
