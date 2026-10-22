using System.Globalization;
using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>Owns UI text separately from validated values; snapshots are independent copies.</summary>
public abstract class AnalysisOptionDraft : IDisposable
{
    private double alpha;
    private bool disposed;
    public string AlphaText { get; private set; } = "";
    public string? AlphaError { get; private set; }
    public bool IsValid => !disposed && AlphaError is null;

    protected AnalysisOptionDraft(double initialAlpha) => SetAlpha(initialAlpha.ToString("G", CultureInfo.InvariantCulture));

    public void SetAlpha(string? text)
    {
        if (disposed) return;
        AlphaText = text ?? "";
        if (double.TryParse(AlphaText, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            && double.IsFinite(value) && value > 0 && value < 1)
        {
            alpha = value;
            AlphaError = null;
        }
        else AlphaError = "Enter a finite number greater than 0 and less than 1, using a decimal point.";
    }

    protected double ValidAlpha => IsValid ? alpha : throw new InvalidOperationException("Options draft is invalid or closed.");
    protected void SetFlag(ref bool field, bool value) { if (!disposed) field = value; }
    public abstract SpreadsheetAnalysisOptions Snapshot();
    public void Dispose() => disposed = true;
}

public sealed class AnovaOptionDraft : AnalysisOptionDraft
{
    private bool welch, fisherLsd, bonferroni, tukeyKramer, gamesHowell;
    public AnovaOptionDraft(OneWayAnovaAnalysisOptions options) : base(options.Alpha)
    {
        welch = options.IncludeWelch; fisherLsd = options.IncludeFisherLsd;
        bonferroni = options.IncludeBonferroni; tukeyKramer = options.IncludeTukeyKramer;
        gamesHowell = options.IncludeGamesHowell;
    }
    public bool IncludeWelch { get => welch; set => SetFlag(ref welch, value); }
    public bool IncludeFisherLsd { get => fisherLsd; set => SetFlag(ref fisherLsd, value); }
    public bool IncludeBonferroni { get => bonferroni; set => SetFlag(ref bonferroni, value); }
    public bool IncludeTukeyKramer { get => tukeyKramer; set => SetFlag(ref tukeyKramer, value); }
    public bool IncludeGamesHowell { get => gamesHowell; set => SetFlag(ref gamesHowell, value); }
    public override SpreadsheetAnalysisOptions Snapshot() => new OneWayAnovaAnalysisOptions
    {
        Alpha = ValidAlpha, IncludeWelch = welch, IncludeFisherLsd = fisherLsd,
        IncludeBonferroni = bonferroni, IncludeTukeyKramer = tukeyKramer, IncludeGamesHowell = gamesHowell
    };
}

public sealed class KruskalWallisOptionDraft : AnalysisOptionDraft
{
    private bool dunn;
    public KruskalWallisOptionDraft(KruskalWallisAnalysisOptions options) : base(options.Alpha) => dunn = options.IncludeDunnComparisons;
    public bool IncludeDunnComparisons { get => dunn; set => SetFlag(ref dunn, value); }
    public override SpreadsheetAnalysisOptions Snapshot() => new KruskalWallisAnalysisOptions
    { Alpha = ValidAlpha, IncludeDunnComparisons = dunn };
}
