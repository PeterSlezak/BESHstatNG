using System.Globalization;
using System.Resources;

namespace BESHStat.OfficeAddin.Services;

/// <summary>One host locale adapter. Storage/method identities/numerics remain invariant.</summary>
public sealed class OfficeStrings : IDisposable
{
    private readonly IOfficeWorkspaceBrowser browser;
    private readonly OfficePreferenceSession preferences;
    private bool disposed;
    private static readonly ResourceManager Resources = new("BESHStat.OfficeAddin.Resources.Ui", typeof(OfficeStrings).Assembly);
    private static readonly ResourceManager SlovakResources = new("BESHStat.OfficeAddin.Resources.Ui.sk", typeof(OfficeStrings).Assembly);
    private Task? initialization;
    public CultureInfo Culture { get; private set; } = CultureInfo.GetCultureInfo("en");
    public CultureInfo DetectedCulture { get; private set; } = CultureInfo.GetCultureInfo("en");
    public string LanguageName => this[Culture.Name == "sk" ? "SlovakLanguage" : "EnglishLanguage"];
    public event Action? Changed;
    public OfficeStrings(IOfficeWorkspaceBrowser browser, OfficePreferenceSession preferences)
    {
        this.browser = browser; this.preferences = preferences;
        preferences.Changed += ApplyLanguage;
    }
    public string this[string key] => (Culture.TwoLetterISOLanguageName == "sk" ? SlovakResources : Resources).GetString(key, CultureInfo.InvariantCulture)
        ?? Resources.GetString(key, CultureInfo.InvariantCulture) ?? key;
    public Task InitializeAsync() => initialization ??= DetectAsync();
    private async Task DetectAsync()
    {
        string? locale;
        try { locale = await browser.DisplayLocaleAsync(); } catch { locale = null; }
        if (disposed) return;
        try { DetectedCulture = string.IsNullOrWhiteSpace(locale) ? CultureInfo.GetCultureInfo("en") : CultureInfo.GetCultureInfo(locale); }
        catch (CultureNotFoundException) { DetectedCulture = CultureInfo.GetCultureInfo("en"); }
        await preferences.InitializeAsync();
        ApplyLanguage();
    }
    private void ApplyLanguage()
    {
        if (disposed) return;
        var next = CultureInfo.GetCultureInfo(!preferences.ForceEnglish && DetectedCulture.TwoLetterISOLanguageName == "sk" ? "sk" : "en");
        if (next.Name == Culture.Name) return;
        Culture = next;
        if (Changed is { } observers) foreach (Action observer in observers.GetInvocationList()) try { observer(); } catch { }
    }
    public void Dispose() { disposed = true; preferences.Changed -= ApplyLanguage; Changed = null; }
    public string ActionMessage(string code) => this[code switch {
        "analysis.completed" => "ResultReady",
        "user.cancelled" or "output.cancelled" => "Cancelled",
        "input.preview-stale" or "user.document-changed" => "StaleInput",
        "output.consent-stale" => "StaleConsent",
        "output.mutation-uncertain" => "Uncertain",
        "input.validation-failed" => "ValidationFailed",
        "input.time-budget" => "Timeout",
        "output.writing" => "Writing",
        "output.cancellation-requested" => "CancelRequested",
        "output.inspecting" => "Inspecting",
        "output.confirmation-required" => "Confirming",
        "output.completed" => "WriteCompleted",
        "host.busy" => "Wait",
        _ when code.StartsWith("input.", StringComparison.Ordinal) => "CheckInput",
        _ when code.StartsWith("output.", StringComparison.Ordinal) => "CheckOutput",
        _ => "OperationFailed"
    }];
}
