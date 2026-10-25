using System.Globalization;
using BESHStatNG.AppInfrastructure;
using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>Host UI text parsing only. Common/Core remain the sole normalization policy.</summary>
public sealed class GlobalOptionsDraft
{
    public string Alpha { get; set; } = "";
    public string Seed { get; set; } = "";
    public string Digits { get; set; } = "";
    public bool Trace { get; set; }
    public string Mode { get; set; } = "0";
    public bool UpperBound { get; set; }
    public bool ForceEnglish { get; set; } = true;
    public Dictionary<string, string> Errors { get; } = [];
    public void Revert(PreferenceSnapshot snapshot, bool forceEnglish = true)
    {
        Alpha = snapshot.DefaultAlpha.ToString("G", CultureInfo.InvariantCulture);
        Seed = snapshot.DefaultRandomSeed == int.MinValue ? "" : snapshot.DefaultRandomSeed.ToString(CultureInfo.InvariantCulture);
        Digits = snapshot.DecimalPlaces.ToString(CultureInfo.InvariantCulture);
        Trace = snapshot.TraceEnabled; Mode = ((int)snapshot.SmallValueDisplay).ToString(CultureInfo.InvariantCulture);
        UpperBound = snapshot.UseUpperBound; ForceEnglish = forceEnglish; Errors.Clear();
    }
    public PreferenceSnapshot? Validate()
    {
        Errors.Clear();
        // All locales accept only invariant decimal point. No grouping, comma or mixed separators.
        if (!double.TryParse(Alpha, NumberStyles.Float, CultureInfo.InvariantCulture, out var alpha) || !double.IsFinite(alpha) || alpha <= 0 || alpha >= 1)
            Errors["alpha"] = "AlphaInvalid";
        var seed = int.MinValue;
        if (!string.IsNullOrWhiteSpace(Seed) && !int.TryParse(Seed, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out seed)) Errors["seed"] = "SeedInvalid";
        if (!int.TryParse(Digits, NumberStyles.None, CultureInfo.InvariantCulture, out var digits) || digits < PValuePresentationSettings.MinimumDecimalPlaces || digits > PValuePresentationSettings.MaximumDecimalPlaces)
            Errors["digits"] = "DigitsInvalid";
        if (!int.TryParse(Mode, NumberStyles.None, CultureInfo.InvariantCulture, out var mode) || !Enum.IsDefined(typeof(PValueSmallValueDisplay), mode)) Errors["mode"] = "ModeInvalid";
        return Errors.Count > 0 ? null : PreferenceSnapshot.Normalize(new PreferenceData {
            DefaultAlpha = alpha, DefaultRandomSeed = seed, TraceEnabled = Trace,
            PValues = new() { DecimalPlaces = digits, SmallValueDisplay = (PValueSmallValueDisplay)mode, UseUpperBound = UpperBound }
        }).Snapshot;
    }
}
