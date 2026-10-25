using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>A typed, read-only preview. No handler registration, fitting or output artifact exists.</summary>
public sealed record SpecificationPreviewData(SpreadsheetAnalysisInput? Specification,
    IReadOnlyList<SpreadsheetValidationIssue> Issues, IReadOnlyList<SpecificationColumn> Columns,
    string WorkbookId, string WorksheetId, string WorksheetName, string Address, string SelectionRevision,
    int Observations, int DesignWidth, string NormalizedFormula)
{
    public string HostSessionIdentity { get; init; } = "";
    public string DocumentIdentity { get; init; } = "";
    public bool Valid => Specification is not null && !Issues.Any(i => i.Severity == SpreadsheetValidationSeverity.Error);
}

public static class SpecificationCompilation
{
    public static SpecificationPreviewData Compile(WorksheetColumnCatalog source, SpecificationDraft draft)
    {
        var compiled=Compile(source.ParserCopy(),draft);
        var columns=source.ForHeader(draft.Header).Select(c=>c with {
            Name=compiled.Columns.FirstOrDefault(n=>n.Index==c.Index)?.Name ?? c.Name }).ToArray();
        var physical=source.PhysicalCopy();
        return compiled with { Specification=source.Translate(compiled.Specification),
            Issues=Array.AsReadOnly(compiled.Issues.Select(source.Translate).ToArray()), Columns=Array.AsReadOnly(columns),
            WorkbookId=physical.WorkbookId,WorksheetId=source.WorksheetId,WorksheetName=source.WorksheetName,
            Address=source.Address, SelectionRevision=physical.SelectionRevision, DocumentIdentity=source.DocumentIdentity };
    }
    public static SpecificationPreviewData Compile(SpreadsheetRangeData source, SpecificationDraft draft)
    {
        var frame = RectangularCategoricalParsers.ParseDataFrame(source, new() { Header = draft.Header });
        var columns = frame.Success ? frame.Data.Columns.Select((c, i) => new SpecificationColumn(i, c.Name, c.WorksheetColumn)).ToArray() : [];
        SpecificationPreviewData Finish(SpreadsheetAnalysisInput? data, IEnumerable<SpreadsheetValidationIssue> issues,
            int observations = 0, int width = 0, string formula = "") => new(data, Array.AsReadOnly(issues.ToArray()), Array.AsReadOnly(columns),
                source.WorkbookId, source.WorksheetId, source.WorksheetName, source.Address, source.SelectionRevision, observations, width, formula);
        if (!frame.Success) return Finish(null, frame.Validation.Issues);
        SpreadsheetValidationIssue Issue(string code, string message, string field, int column = -1) =>
            new(code, SpreadsheetValidationSeverity.Error, message, field, selectedColumn: column + 1,
                worksheetColumn: column >= 0 ? source.StartColumn + column : 0);
        if (!Enum.IsDefined(draft.Family)) return Finish(null, [Issue("spec.family", "Choose a supported specification family.", "family")]);
        if (draft.Family == SpecificationFamily.Model)
        {
            // Visual role conflicts stay visible; never silently discard a term on a role change.
            if (!draft.ManualFormula && draft.Terms.Any(t => !draft.Predictors.Contains(t.Column) ||
                t.Kind == SpecificationTermKind.Interaction && !draft.Predictors.Contains(t.OtherColumn)))
                return Finish(null, [Issue("spec.term-role", "Remove terms referring to unselected predictors.", "formula")]);
            var options = draft.ModelOptions(columns);
            if (options.FormulaText.Length > 2048 || draft.Terms.Count > 32)
                return Finish(null, [Issue("spec.formula-limit", "Use at most 2,048 formula characters and 32 visual terms.", "formula")]);
            // Inspect Core's AST before materializing a potentially explosive interaction. This is
            // a conservative resource guard, not a second grammar or a substitute design matrix.
            if (options.PredictorColumnIndices.Length > 0 && options.PredictorColumnIndices.Distinct().Count() == options.PredictorColumnIndices.Length &&
                options.PredictorColumnIndices.All(c => c >= 0 && c < columns.Length && c != draft.Response))
            {
                var catalog = BESHStatNG.RegressionVariableCatalog.Build(options.PredictorColumnIndices.Select(c => columns[c].Name),
                    options.PredictorColumnIndices.Select(c => "V" + (c + 1)), allowAbsoluteColumnLetters: false);
                BESHStatNG.RegressionFormulaDesignSpec spec = null!; string error = "";
                if (BESHStatNG.RegressionFormulaParser.TryParseFormulaToDesignSpec(options.FormulaText, catalog, ref spec, ref error))
                {
                    if (spec.ParsedTerms.Any(t => t.Degree > 6))
                        return Finish(null, [Issue("spec.polynomial-limit", "This preview limits polynomial powers to 6.", "formula")]);
                    long bound = draft.IncludeIntercept ? 1 : 0;
                    foreach (var term in spec.ParsedTerms)
                    {
                        long width = 1;
                        foreach (var key in term.BaseVarKeys)
                        {
                            var isCategorical = term.Scale == BESHStatNG.PredictorScale.Categorical || term.InteractionCategoricalBaseKeys?.Contains(key) == true;
                            if (!isCategorical) continue;
                            var column = options.PredictorColumnIndices[Array.FindIndex(options.PredictorColumnIndices, c => "V" + (c + 1) == key)];
                            var levels = RectangularCategoricalParsers.ParseCategoricalVector(source, new() {
                                Header = draft.Header, ColumnIndex = column, Missing = SpreadsheetIncompleteObservationPolicy.Skip, IgnoreTextCase = draft.IgnoreCase
                            });
                            if (!levels.Success) continue; // Common reports the original typed-cell error below.
                            width *= Math.Max(1, levels.Data.Levels.Count);
                            if (width > 128) break;
                        }
                        bound += width;
                        if (bound > 128) return Finish(null, [Issue("spec.design-budget", "The conservative expansion bound exceeds 128 columns. Reduce factors or interaction terms.", "design")]);
                    }
                }
            }
            var parsed = SpreadsheetModelInputCompiler.Compile(source, options);
            if (parsed.Success && !draft.ManualFormula)
                foreach (var column in draft.Factors)
                {
                    var expected = draft.ReferenceKey(column);
                    if (expected is not null && parsed.Data.CategoricalColumns.FirstOrDefault(c => c.SelectedColumn == column + 1)?
                        .Levels.FirstOrDefault(l => l.Code == draft.Reference(column))?.Value.Key != expected)
                        return Finish(null, [Issue("spec.reference-stale", "The typed level for this reference code changed. Inspect current levels and choose the reference again.", "factors.reference", column)]);
                }
            return Finish(parsed.Data, parsed.Validation.Issues, parsed.Data?.ObservationCount ?? 0,
                parsed.Data?.DesignColumnCount ?? 0, parsed.Data?.NormalizedFormula ?? "");
        }
        if (draft.Family == SpecificationFamily.Repeated || draft.Family == SpecificationFamily.Mixed)
        {
            var options = draft.RepeatedOptions();
            if (draft.Family == SpecificationFamily.Mixed)
            {
                var parsed = LongitudinalParsers.ParseMixedModel(source, options);
                return Finish(parsed.Data, parsed.Validation.Issues, parsed.Data?.Rows.Count ?? 0);
            }
            var repeated = LongitudinalParsers.ParseRepeatedSubjects(source, options);
            return Finish(repeated.Data, repeated.Validation.Issues, repeated.Data?.Count ?? 0);
        }
        var categorical = RectangularCategoricalParsers.ParseCategoricalVector(source, draft.CategoryOptions());
        return Finish(categorical.Data, categorical.Validation.Issues, categorical.Data?.Count ?? 0);
    }
}
