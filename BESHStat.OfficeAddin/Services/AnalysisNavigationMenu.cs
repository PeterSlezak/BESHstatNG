using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public sealed record AnalysisMethodChoice(string Key, string? AnalysisId, string DisplayName, bool CanEdit)
{
    // Documentation keys never masquerade as executable analysis IDs.
    public string Value => CanEdit ? AnalysisId! : $"unavailable:{Key}";
}

public sealed record AnalysisMethodGroup(string Category, IReadOnlyList<AnalysisMethodChoice> Methods)
{
    public bool HasSupportedEditor => Methods.Any(m => m.CanEdit);
}

/// <summary>Documentation discovery is separate from verified handler/editor availability.</summary>
public sealed class AnalysisNavigationMenu
{
    private readonly Dictionary<string, AnalysisMethodGroup> byCategory;
    public IReadOnlyList<AnalysisMethodGroup> Groups { get; }

    public AnalysisNavigationMenu(IAnalysisNavigationCatalog catalog, AnalysisEditorRegistry editors)
        : this(catalog, editors, GeneratedMethodMetadata.All) { }

    internal AnalysisNavigationMenu(IAnalysisNavigationCatalog catalog, AnalysisEditorRegistry editors,
        IEnumerable<AnalysisMethodMetadata> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var entries = metadata.ToArray();
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var labels = new HashSet<(string Category, string Name)>();
        foreach (var entry in entries)
        {
            // Ambiguity is a catalog defect, never resolved by picking the first match.
            if (entry is null || string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Category)
                || string.IsNullOrWhiteSpace(entry.DisplayName) || !keys.Add(entry.Key)
                || !labels.Add((entry.Category, entry.DisplayName))
                || (!string.IsNullOrEmpty(entry.AnalysisId) && !ids.Add(entry.AnalysisId)))
                throw new ArgumentException("Method metadata is missing required fields or contains duplicate identities/labels.", nameof(metadata));
        }
        bool Supported(AnalysisMethodMetadata entry) => catalog.TryGet(entry.AnalysisId, out var descriptor)
            && string.Equals(entry.Category, descriptor!.Category, StringComparison.Ordinal)
            && string.Equals(entry.DisplayName, descriptor.DisplayName, StringComparison.Ordinal)
            && editors.Supports(descriptor);

        Groups = Array.AsReadOnly(entries.GroupBy(m => m.Category, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new AnalysisMethodGroup(g.Key, Array.AsReadOnly(g
                .OrderBy(m => m.DisplayName, StringComparer.Ordinal).ThenBy(m => m.Key, StringComparer.Ordinal)
                .Select(m => new AnalysisMethodChoice(m.Key, m.AnalysisId, m.DisplayName, Supported(m))).ToArray())))
            .ToArray());
        byCategory = Groups.ToDictionary(g => g.Category, StringComparer.Ordinal);
    }

    public bool TryGetGroup(string? category, out AnalysisMethodGroup? group)
    {
        group = null;
        return category is not null && byCategory.TryGetValue(category, out group);
    }
}
