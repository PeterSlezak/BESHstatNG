using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>Component-owned. Reset on method change; Retry leaves state alone; disposal closes drafts.</summary>
public sealed class AnalysisNavigationState : IDisposable
{
    public const string UnavailableReason = "Options are unavailable: the method, handler registration or option schema is unsupported.";
    public const string GroupUnavailableReason = "Not yet available in this Office add-in. This group currently has no supported option editors.";
    private readonly IAnalysisNavigationCatalog catalog;
    private readonly AnalysisEditorRegistry editors;
    private readonly AnalysisNavigationMenu menu;
    private bool disposed;
    public AnalysisMethodGroup? SelectedGroup { get; private set; }
    public IAnalysisDescriptor? Selected { get; private set; }
    public AnalysisEditor? Editor { get; private set; }
    public string? Rejection { get; private set; }

    public AnalysisNavigationState(IAnalysisNavigationCatalog catalog, AnalysisEditorRegistry editors)
        : this(catalog, editors, new AnalysisNavigationMenu(catalog, editors)) { }

    public AnalysisNavigationState(IAnalysisNavigationCatalog catalog, AnalysisEditorRegistry editors, AnalysisNavigationMenu menu)
    { this.catalog = catalog; this.editors = editors; this.menu = menu; }

    public bool SelectGroup(string? category)
    {
        if (disposed) return false;
        if (SelectedGroup is not null && string.Equals(SelectedGroup.Category, category, StringComparison.Ordinal)) return true;
        ClearMethod();
        SelectedGroup = null;
        if (menu.TryGetGroup(category, out var group)) { SelectedGroup = group; return true; }
        if (!string.IsNullOrEmpty(category)) Rejection = UnavailableReason;
        return false;
    }

    public bool Select(string? id)
    {
        if (disposed) return false;
        if (Selected is not null && string.Equals(Selected.Id, id, StringComparison.Ordinal)) return Editor is not null;
        ClearMethod();
        if (string.IsNullOrEmpty(id)) return false; // Safe dropdown placeholder.
        if (SelectedGroup?.Methods.Any(m => m.CanEdit && string.Equals(m.AnalysisId, id, StringComparison.Ordinal)) == true
            && catalog.TryGet(id, out var descriptor)
            && string.Equals(descriptor!.Category, SelectedGroup.Category, StringComparison.Ordinal))
        {
            Editor = editors.Create(descriptor);
            if (Editor is not null) Selected = descriptor;
        }
        if (Editor is null) Rejection = UnavailableReason;
        return Editor is not null;
    }

    private void ClearMethod()
    {
        Editor?.Draft.Dispose();
        Editor = null;
        Selected = null;
        Rejection = null;
    }

    public void Dispose()
    {
        disposed = true;
        ClearMethod();
        SelectedGroup = null;
    }
}
