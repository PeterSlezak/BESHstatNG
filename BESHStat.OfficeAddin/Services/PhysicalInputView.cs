using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>A parser-only view. Canonical columns have no invented Excel address or identity.</summary>
internal sealed class PhysicalInputView(IReadOnlyList<SpreadsheetRangeData> physical)
{
    public int FirstRow => physical.Max(s => s.StartRow);
    private int LastRow => physical.Min(s => s.StartRow + s.RowCount - 1);
    public SpreadsheetRangeData ParserInput => physical.Count == 1 ? physical[0] : new()
    {
        RowCount = LastRow - FirstRow + 1, ColumnCount = 2,
        Rows = Enumerable.Range(FirstRow, LastRow - FirstRow + 1)
            .Select(row => physical.Select(s => s.Rows[row - s.StartRow][0]).ToList()).ToList()
    };

    public InputIssue Issue(string code, string field, int row, int column)
    {
        if (physical.Count == 1)
            return new(code, field, row, column, row > 0 ? physical[0].StartRow + row - 1 : 0,
                column > 0 ? physical[0].StartColumn + column - 1 : 0);
        if (column is not (1 or 2)) return new(code, field, row, 0, row > 0 ? FirstRow + row - 1 : 0, 0);
        var range = physical[column - 1];
        return new(code, $"{(column == 1 ? "category" : "values")}.{field}", row > 0 ? FirstRow + row - range.StartRow : 0, 1,
            row > 0 ? FirstRow + row - 1 : 0, range.StartColumn);
    }

    public static bool Aligned(SpreadsheetRangeData category, SpreadsheetRangeData values) =>
        category.ColumnCount == 1 && values.ColumnCount == 1 && category.StartColumn != values.StartColumn &&
        category.WorkbookId == values.WorkbookId && category.WorkbookName == values.WorkbookName &&
        category.WorksheetId.Length > 0 && category.WorksheetId == values.WorksheetId && category.WorksheetName == values.WorksheetName &&
        Math.Max(category.StartRow, values.StartRow) <= Math.Min(category.StartRow + category.RowCount - 1, values.StartRow + values.RowCount - 1) &&
        new[] { category, values }.All(s => s.Rows.Count == s.RowCount && s.Rows.All(r => r is { Count: 1 }));

    public IReadOnlyList<InputNotice> Notices => physical.Count == 1 ? [] : physical.Select((s, role) => {
        var ignored = s.Rows.Where((_, index) => s.StartRow + index < FirstRow || s.StartRow + index > LastRow).ToArray();
        return new InputNotice("input.long-overhang", role == 0 ? "Group/category" : "Numeric values", ignored.Length,
            ignored.Count(r => r[0] is { Kind: not SpreadsheetCellKind.Blank }));
    }).Where(n => n.Count > 0).ToArray();
}
