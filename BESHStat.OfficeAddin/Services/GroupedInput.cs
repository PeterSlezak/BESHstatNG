using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public sealed record InputNotice(string Code, string Role, int Count, int Populated) {
    public string Message => Code == "input.columns-narrowed"
        ? $"{Role}: whole columns narrowed to {Count} worksheet rows containing used values; interior blanks remain."
        : $"{Role}: {Count} selected cells outside the matching worksheet rows are ignored ({Populated} nonblank).";
}
public sealed record GroupedInputConstraints(int MinimumGroups = 2, int? MaximumGroups = null,
    int MinimumObservations = 2, bool RequireMatchedBlocks = false);
public sealed record GroupedInputPolicy(SpreadsheetGroupedOrientation Orientation, SpreadsheetHeaderMode Header,
    GroupedInputConstraints Constraints);
public sealed class GroupedInputException(string code) : Exception(code) { public string Code { get; } = code; }
internal sealed record GroupedInputResult(PhysicalInputView View, SpreadsheetRangeData ParserInput, GroupedNumericData? Data,
    IReadOnlyList<InputIssue> Issues, IReadOnlyList<InputNotice> Notices, bool Valid);

/// <summary>Layout/role policy around Common's sole grouped parser, independent of method IDs.</summary>
internal static class GroupedInput
{
    public static GroupedInputResult Parse(IReadOnlyList<SpreadsheetRangeData> sources, GroupedInputPolicy policy)
    {
        if (!Enum.IsDefined(policy.Orientation) || !Enum.IsDefined(policy.Header) || policy.Constraints.MinimumGroups < 1 ||
            policy.Constraints.MinimumObservations < 1 || policy.Constraints.MaximumGroups < policy.Constraints.MinimumGroups)
            throw new GroupedInputException("input.policy-invalid");
        var longFormat = policy.Orientation == SpreadsheetGroupedOrientation.LongFormat;
        if (sources.Count != (longFormat ? 2 : 1) || (longFormat && !PhysicalInputView.Aligned(sources[0], sources[1])))
            throw new GroupedInputException("input.long-alignment");
        if (longFormat && policy.Header != SpreadsheetHeaderMode.None && sources[0].StartRow != sources[1].StartRow)
            throw new GroupedInputException("input.long-header-alignment");
        if (policy.Constraints.RequireMatchedBlocks && longFormat) throw new GroupedInputException("input.block-roles-required");
        var view = new PhysicalInputView(sources); var input = view.ParserInput;
        var parsed = GroupedNumericRangeParser.ParseWithDiagnostics(input, new() {
            Orientation = policy.Orientation, Header = policy.Header, CategoryColumnIndex = 0, ValueColumnIndex = 1,
            MinimumObservationsPerGroup = policy.Constraints.MinimumObservations });
        var issues = parsed.Validation.Issues.Select(i => view.Issue(i.Code, i.FieldPath, i.SelectedRow, i.SelectedColumn)).ToList();
        var extra = new List<InputIssue>();
        if (policy.Header == SpreadsheetHeaderMode.Required && input.Rows.Count > 0 && input.Rows.All(r => r is not null && r.Count == input.ColumnCount))
        {
            var headers = policy.Orientation == SpreadsheetGroupedOrientation.GroupsInRows
                ? Enumerable.Range(0, input.RowCount).Select(r => (Row: r, Col: 0))
                : Enumerable.Range(0, input.ColumnCount).Select(c => (Row: 0, Col: c));
            foreach (var (r, c) in headers)
                if (input.Rows[r][c] is null or { Kind: SpreadsheetCellKind.Blank or SpreadsheetCellKind.CellError } ||
                    input.Rows[r][c] is { Kind: SpreadsheetCellKind.Text } text && string.IsNullOrWhiteSpace(text.TextValue))
                    extra.Add(view.Issue("input.header-required", "header", r + 1, c + 1));
        }
        if (parsed.Success)
        {
            var counts = parsed.Data.GroupCounts;
            if (counts.Length < policy.Constraints.MinimumGroups || counts.Length > policy.Constraints.MaximumGroups.GetValueOrDefault(int.MaxValue))
                extra.Add(view.Issue("input.group-count", "groups", 0, 0));
            if (policy.Constraints.RequireMatchedBlocks)
            {
                // Common has interpreted headers. Completeness additionally checks every physical block position.
                var columns = policy.Orientation == SpreadsheetGroupedOrientation.GroupsInColumns;
                int blocks = columns ? input.RowCount : input.ColumnCount;
                int offset = policy.Header == SpreadsheetHeaderMode.Required ? 1 : policy.Header == SpreadsheetHeaderMode.None ? 0 : blocks - counts[0];
                var complete = offset is 0 or 1 && counts.All(n => n == blocks - offset) &&
                    Enumerable.Range(offset, Math.Max(0, blocks - offset)).All(block =>
                        Enumerable.Range(0, counts.Length).All(group => {
                            var cell = columns ? input.Rows[block][group] : input.Rows[group][block];
                            return cell is { Kind: SpreadsheetCellKind.Number } && double.IsFinite(cell.NumberValue);
                        }));
                if (!complete) extra.Add(view.Issue("input.incomplete-blocks", "blocks", 0, 0));
            }
        }
        issues.AddRange(extra);
        return new(view, input, parsed.Data, issues.AsReadOnly(), view.Notices, parsed.Success && extra.Count == 0);
    }
}
