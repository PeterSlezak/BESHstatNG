using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public interface IAnalysisNavigationCatalog
{
    IReadOnlyList<IAnalysisDescriptor> All { get; }
    bool TryGet(string? id, out IAnalysisDescriptor? descriptor);
}

/// <summary>Metadata projection only. Handler presence does not imply Office run/write support.</summary>
public sealed class AnalysisNavigationCatalog : IAnalysisNavigationCatalog
{
    private readonly Dictionary<string, IAnalysisDescriptor> byId;
    public IReadOnlyList<IAnalysisDescriptor> All { get; }

    public AnalysisNavigationCatalog() : this(AnalysisCatalog.All, AnalysisHandlerRegistry.All) { }

    internal AnalysisNavigationCatalog(IEnumerable<IAnalysisDescriptor> descriptors, IEnumerable<IAnalysisHandler> handlers)
    {
        var entries = descriptors.ToArray();
        var registrations = handlers.ToArray();
        AnalysisCatalog.ValidateDescriptors(entries);
        var verified = new List<IAnalysisDescriptor>();
        foreach (var descriptor in entries.Where(d => d.Availability == AnalysisAvailability.ImplementedInCommon))
        {
            try
            {
                // Require every declared input registration, including exact option types.
                AnalysisHandlerRegistry.ValidateRegistrations(new[] { descriptor },
                    registrations.Where(h => string.Equals(h.AnalysisId, descriptor.Id, StringComparison.Ordinal)));
                verified.Add(descriptor);
            }
            catch (ArgumentException) { /* Invalid registrations never enter navigation. */ }
        }
        All = Array.AsReadOnly(verified.OrderBy(d => d.Category, StringComparer.Ordinal)
            .ThenBy(d => d.Id, StringComparer.Ordinal).ToArray());
        byId = All.ToDictionary(d => d.Id, StringComparer.Ordinal);
    }

    public bool TryGet(string? id, out IAnalysisDescriptor? descriptor)
    {
        descriptor = null;
        return id is not null && byId.TryGetValue(id, out descriptor);
    }
}
