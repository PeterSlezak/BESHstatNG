using System.Globalization;
using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public enum SpecificationFamily { Model, Repeated, Mixed, Categorical }
public enum SpecificationTermKind { Main, Interaction, Polynomial }
public sealed record SpecificationTerm(SpecificationTermKind Kind, int Column, int OtherColumn = -1, int Degree = 2);
public sealed record SpecificationColumn(int Index, string Name, int WorksheetColumn)
{
    public string WorksheetId { get; init; } = "";
    public string DocumentIdentity { get; init; } = "";
    public string HeaderText { get; init; } = Name;
    public bool HasHeader { get; init; } = true;
    public string DisplayText { get; init; } = WorksheetColumn>0 ? Name + " | Var" + BESHStatNG.RegressionVariableCatalog.NumberToLetters(WorksheetColumn) : Name;
    public int SelectedColumn { get; init; } = Index + 1;
    public SpreadsheetFrameCell? Header { get; init; }
    public string Identity => DocumentIdentity + "/" + WorksheetId + "/" + WorksheetColumn;
}

/// <summary>Local component draft, never a persisted setting or an executable analysis request.</summary>
public sealed class SpecificationDraft
{
    private SpreadsheetHeaderMode header = SpreadsheetHeaderMode.Required;
    private SpreadsheetIncompleteObservationPolicy missing = SpreadsheetIncompleteObservationPolicy.Reject;
    private SpecificationFamily family;
    private int response, subject, visit = 1, group = -1, category;
    private bool intercept = true, ignoreCase, ascending, dateVisits, manual;
    private string formula = "";
    private int[] predictors = [], factors = [], fixedEffects = [], randomGroups = [];
    private readonly List<SpecificationTerm> terms = [];
    private readonly Dictionary<int, double> references = [];
    private readonly Dictionary<int, string> referenceKeys = [];
    public event Action? Changed;
    public long Revision { get; private set; }
    private void Set<T>(ref T field, T value) { if (EqualityComparer<T>.Default.Equals(field, value)) return; field = value; Touch(); }
    private void Touch() { Revision++; Changed?.Invoke(); }
    public SpecificationFamily Family { get => family; set => Set(ref family, value); }
    public SpreadsheetHeaderMode Header { get => header; set => Set(ref header, value); }
    public SpreadsheetIncompleteObservationPolicy Missing { get => missing; set => Set(ref missing, value); }
    public int Response { get => response; set => Set(ref response, value); }
    public int Subject { get => subject; set => Set(ref subject, value); }
    public int Visit { get => visit; set => Set(ref visit, value); }
    public int Group { get => group; set => Set(ref group, value); }
    public int Category { get => category; set => Set(ref category, value); }
    public bool IncludeIntercept { get => intercept; set => Set(ref intercept, value); }
    public bool IgnoreCase { get => ignoreCase; set => Set(ref ignoreCase, value); }
    public bool AscendingVisits { get => ascending; set => Set(ref ascending, value); }
    public bool DateVisits { get => dateVisits; set => Set(ref dateVisits, value); }
    public bool ManualFormula { get => manual; set => Set(ref manual, value); }
    public string FormulaText { get => formula; set => Set(ref formula, value ?? ""); }
    public IReadOnlyList<int> Predictors => Array.AsReadOnly(predictors);
    public IReadOnlyList<int> Factors => Array.AsReadOnly(factors);
    public IReadOnlyList<int> FixedEffects => Array.AsReadOnly(fixedEffects);
    public IReadOnlyList<int> RandomGroups => Array.AsReadOnly(randomGroups);
    public IReadOnlyList<SpecificationTerm> Terms => terms.AsReadOnly();
    public double? Reference(int column) => references.TryGetValue(column, out var code) ? code : null;
    public string? ReferenceKey(int column) => referenceKeys.GetValueOrDefault(column);
    public void SetReference(int column, double? code, string? typedKey = null)
    {
        if (code.HasValue) references[column] = code.Value; else references.Remove(column);
        if (typedKey is not null) referenceKeys[column] = typedKey; else referenceKeys.Remove(column);
        Touch();
    }
    public void SetPredictors(IEnumerable<int> value) { predictors = value.ToArray(); Touch(); }
    public void SetFactors(IEnumerable<int> value) { factors = value.ToArray(); Touch(); }
    public void SetFixedEffects(IEnumerable<int> value) { fixedEffects = value.ToArray(); Touch(); }
    public void SetRandomGroups(IEnumerable<int> value) { randomGroups = value.ToArray(); Touch(); }
    public void AddTerm(SpecificationTerm value) { terms.Add(value); Touch(); }
    public void RemoveTerm(int index) { terms.RemoveAt(index); Touch(); }
    public void ResetRoles()
    {
        response=subject=visit=group=category=-1; predictors=factors=fixedEffects=randomGroups=[];
        terms.Clear(); references.Clear(); referenceKeys.Clear(); manual=false; formula=""; Touch();
    }
    public string TermCaption(SpecificationTerm term, IReadOnlyList<SpecificationColumn> columns)
    {
        string Caption(int column) => columns.FirstOrDefault(c=>c.Index==column)?.DisplayText ?? "Unselected column";
        return term.Kind switch { SpecificationTermKind.Interaction => Caption(term.Column)+" : "+Caption(term.OtherColumn),
            SpecificationTermKind.Polynomial => Caption(term.Column)+" ^ "+term.Degree, _=>Caption(term.Column) };
    }

    // Serialization of typed visual terms only. Core remains the only parser of the resulting RHS.
    // Quoting follows RegressionVariableCatalog's doubled-apostrophe convention. Before discovery,
    // relative letters address the predictor subset, never the worksheet's absolute columns.
    public string TermText(SpecificationTerm term, IReadOnlyList<SpecificationColumn> columns)
    {
        string Variable(int column)
        {
            var named = columns.FirstOrDefault(c => c.Index == column);
            var token = named is null ? (Array.IndexOf(predictors, column) < 0 ? "[unselected column]" : BESHStatNG.RegressionVariableCatalog.NumberToLetters(Array.IndexOf(predictors, column) + 1))
                : "'" + named.Name.Replace("'", "''") + "'";
            if (!factors.Contains(column)) return token;
            var reference = Reference(column);
            return "factor(" + token + (reference.HasValue ? ", ref=" + reference.Value.ToString("R", CultureInfo.InvariantCulture) : "") + ")";
        }
        var first = Variable(term.Column);
        return term.Kind switch {
            SpecificationTermKind.Main => first,
            SpecificationTermKind.Interaction => first + ":" + Variable(term.OtherColumn),
            SpecificationTermKind.Polynomial => first + "^" + term.Degree.ToString(CultureInfo.InvariantCulture),
            _ => ""
        };
    }
    public string Rhs(IReadOnlyList<SpecificationColumn> columns) => ManualFormula ? FormulaText
        : string.Join(" + ", (terms.Count == 0 ? predictors.Select(c => new SpecificationTerm(SpecificationTermKind.Main, c)) : terms)
            .Select(t => TermText(t, columns)));
    public SpreadsheetModelParseOptions ModelOptions(IReadOnlyList<SpecificationColumn> columns) => new() {
        Header = Header, ResponseColumnIndex = Response, PredictorColumnIndices = predictors.ToArray(),
        CategoricalColumnIndices = factors.ToArray(), FormulaText = Rhs(columns), IncludeIntercept = IncludeIntercept,
        Missing = Missing, IgnoreCategoryCase = IgnoreCase, MaximumExpandedColumns = 128, MinimumCompleteRows = 2
    };
    public SpreadsheetMixedModelParseOptions RepeatedOptions() => new() {
        Header = Header, SubjectColumnIndex = Subject, VisitColumnIndex = Visit, ResponseColumnIndex = Response,
        GroupColumnIndex = Group, Missing = Missing, IgnoreCategoryCase = IgnoreCase,
        RequireAscendingVisits = AscendingVisits, InterpretVisitAsDate = DateVisits, RejectDuplicateSubjectVisits = true,
        FixedEffectColumnIndices = fixedEffects.ToArray(), RandomGroupColumnIndices = randomGroups.ToArray()
    };
    public SpreadsheetCategoricalVectorParseOptions CategoryOptions() => new() {
        Header = Header, ColumnIndex = Category, Missing = Missing, IgnoreTextCase = IgnoreCase
    };
}
