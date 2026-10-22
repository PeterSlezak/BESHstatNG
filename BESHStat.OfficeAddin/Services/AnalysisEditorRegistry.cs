using BESHStatNG.ExcelCommon;
using BESHStat.OfficeAddin.Components.OptionEditors;
using Microsoft.AspNetCore.Components;

namespace BESHStat.OfficeAddin.Services;

public interface IAnalysisOptionEditor<TDraft> : IComponent where TDraft : AnalysisOptionDraft { }

public sealed record AnalysisEditor(AnalysisOptionDraft Draft, Type ComponentType);

/// <summary>Compile-time editor registrations, keyed by schema AND exact Common option type.</summary>
public sealed class AnalysisEditorRegistry
{
    private interface IRegistration { AnalysisEditor? Create(SpreadsheetAnalysisOptions options); }
    private sealed class Registration<TOptions, TDraft, TComponent>(Func<TOptions, TDraft> create) : IRegistration
        where TOptions : SpreadsheetAnalysisOptions
        where TDraft : AnalysisOptionDraft
        where TComponent : IAnalysisOptionEditor<TDraft>
    {
        public AnalysisEditor? Create(SpreadsheetAnalysisOptions options) =>
            options.GetType() == typeof(TOptions) ? new(create((TOptions)options), typeof(TComponent)) : null;
    }

    private readonly Dictionary<(string Schema, Type Options), IRegistration> registrations = new();
    public AnalysisEditorRegistry()
    {
        Register<OneWayAnovaAnalysisOptions, AnovaOptionDraft, AnovaOptions>("anova.oneway.options.v1", o => new(o));
        Register<KruskalWallisAnalysisOptions, KruskalWallisOptionDraft, KruskalWallisOptions>("nonparametric.kruskal-wallis.options.v1", o => new(o));
    }
    private void Register<TOptions, TDraft, TComponent>(string schema, Func<TOptions, TDraft> create)
        where TOptions : SpreadsheetAnalysisOptions where TDraft : AnalysisOptionDraft
        where TComponent : IAnalysisOptionEditor<TDraft> =>
        registrations.Add((schema, typeof(TOptions)), new Registration<TOptions, TDraft, TComponent>(create));

    // Caller must obtain this descriptor through the verified navigation catalog.
    internal bool Supports(IAnalysisDescriptor descriptor)
    {
        var editor = Create(descriptor);
        editor?.Draft.Dispose();
        return editor is not null;
    }

    internal AnalysisEditor? Create(IAnalysisDescriptor descriptor)
    {
        if (!registrations.TryGetValue((descriptor.OptionsSchemaId, descriptor.OptionsType), out var registration)) return null;
        try { return registration.Create(descriptor.CreateDefaultOptions()); }
        catch (InvalidOperationException) { return null; }
        catch (ArgumentException) { return null; }
    }
}
