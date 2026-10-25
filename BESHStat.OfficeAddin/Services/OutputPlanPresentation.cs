using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>Reusable presentation preparation using Common's existing immutable preference snapshot.</summary>
internal static class OutputPlanPresentation
{
    public static OfficeOutputPlan Prepare(OutputDocument document, SpreadsheetOutputTarget target, double alpha, PreferenceSnapshot preferences)
    {
        var semantic = OutputSemanticPlanner.Create(document, preferences.CreateDisplaySettings(), alpha);
        var layout = OutputLayoutPlanner.Create(document, target, new OutputLayoutOptions(), semanticPlan: semantic);
        return OfficeOutputPlanAdapter.Prepare(document, semantic, layout, autoFit: false);
    }
}
