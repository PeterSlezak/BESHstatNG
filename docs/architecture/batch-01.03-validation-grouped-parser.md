# BESHStatNG Batch 01.03 — shared validation and configurable grouped inputs

Baseline: `BESHstatNG(20261016-134340).zip` (01.02 developer-validated).

## Scope

This batch adds **opt-in** spreadsheet validation diagnostics and grouped-data interpretation to `BESHStat.Excel.Common`, and a small opt-in preview in the independent `BESHStat.WasmSmoke` test host. The shipped Windows Excel-DNA add-in, Core statistics, registered analysis IDs/handlers, output formats, documentation catalog, and Office.js transport are unchanged.

**Critical compatibility boundary:** `GroupedNumericRangeParser.Parse(input)` remains the original Stage 3.04 implementation. It still requires two or more group columns, a first-row header and at least two numeric observations per group. It skips only true Blank cells, rejects text/Boolean/errors in the data body, and retains the original messages/exception behavior. Windows Excel-DNA continues to use this overload.

## New Common files and contracts

- `src/Input/SpreadsheetValidation.vb`: `SpreadsheetValidationSeverity`, `SpreadsheetValidationIssue` (stable code, message, path, 1-based selection coordinates and optional absolute worksheet coordinates), immutable `SpreadsheetValidationReport`, and throwing `SpreadsheetInputValidationException`.
- `src/Input/ConfigurableGroupedNumericParser.vb`: `SpreadsheetGroupedParseOptions`, orientation and header enums, `SpreadsheetGroupedParseResult`, and typed configurable conversion.
- `src/Input/GroupedNumericRangeParser.vb`: additive `ParseWithDiagnostics(input, options)` and `Parse(input, options)` overloads; original `Parse(input)` is retained.
- `BESHStat.Excel.Common.Tests/src/SpreadsheetGroupedValidationTests.vb`: 16 new MSTest cases exercising all three arrangements, headers, missing values, locales and diagnostics.
- `BESHStat.WasmSmoke/Pages/Home.razor`: optional preview controls and issue display; **unchecked/default uses the original Stage 3.04 path**. When enabled, running ANOVA/Kruskal–Wallis passes validated `GroupedNumericData` to the existing typed registry. No alternative statistics engine is added.

No manually maintained Ribbon or UDF catalogs are introduced.

## Supported interpretations

| Arrangement | Required header | No header | Auto header |
|---|---|---|---|
| Groups in columns | First row holds group names | Entire rectangle contains observations; `Group 1`, etc. | First row all typed labels -> header; all numbers -> no header; mixed/empty -> explicit error |
| Groups in rows | First column holds group names | Entire rectangle contains observations, one group per row | First column labels/numbers interpreted conservatively as above |
| Long format | First row names the fields, then category/value observations | All selected rows are observations | Examines the first value cell, rejects empty/error ambiguity; preserves first-seen group order |

Long-format category and value column options are zero-based in Common, one-based in the WasmSmoke preview UI. Extra columns are allowed and ignored. Group names are case-sensitive, and differently typed categories that would display with the same name (e.g., numeric `2` versus text `"2"`) are rejected rather than silently merged. Entirely blank long-format rows are ignored; partially specified categories are invalid.

## Cell conversion and diagnostics

- The configurable parser uses the existing 01.02 `SpreadsheetCellValuePolicy` and its **strict default**: `Blank` missing, numeric `Number` accepted, literal text/Boolean and typed Excel errors rejected; no guessed locale, date conversion, or error-string-to-error reinterpretation.
- Missing-text, numeric-text culture, and optional Boolean coercion are opt-in through `SpreadsheetGroupedParseOptions.CellPolicy`. Date-like serials remain plain numbers unless a later schema explicitly interprets them as dates.
- `WarnOnSkippedObservations` enables per-cell/row missingness warnings. Default is quiet for valid blanks (matching legacy behavior).
- Failures return issue codes such as `range.not-rectangular`, `header.ambiguous`, `cell.excel-error`, `cell.not-numeric`, `groups.too-few-observations`, and `groups.category-missing`, with selected and worksheet coordinates where known.
- `ParseWithDiagnostics` returns a result with `.Success=False` and `.Data=Nothing` when any error exists. It does not throw for normal malformed input. The configured two-argument `Parse` overload instead throws `SpreadsheetInputValidationException` containing the same report. Both run without WinForms/COM/Office.js dependencies.
- This batch does **not** introduce a general-purpose dataframe, a new Core computation layer or future 01.04 paired/scalar input families.

## Example (VB.NET)

```vb
Dim opts As New SpreadsheetGroupedParseOptions With {
    .Orientation = SpreadsheetGroupedOrientation.GroupsInColumns,
    .Header = SpreadsheetHeaderMode.None
}
Dim parsed As SpreadsheetGroupedParseResult =
    GroupedNumericRangeParser.ParseWithDiagnostics(sourceRange, opts)
If parsed.Success Then
    Dim grouped As GroupedNumericData = parsed.Data
    ' Pass grouped to the existing typed shared analysis dispatcher.
Else
    For Each issue As SpreadsheetValidationIssue In parsed.Validation.Issues
        ' Display issue.Code, issue.Message, issue.SelectedRow/Column.
    Next
End If
```

## Automated tests

From the repository root:

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
node --test BESHStat.WasmSmoke/tests/*.test.cjs
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
python -m unittest discover -s BESHstatNG/tools/tests -p "test_*.py" -v
python -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p "test_*.py" -v
```

Also build the full Windows solution and run the legacy Windows tests. Existing 00.01 golden fixtures, 00.02 catalog APIs, and 01.02 typed-cell tests must continue passing. New VB/C# tests need to be compiled and run on a development machine; offline source checks alone do not establish their success.

## Manual Excel smoke tests

1. **Regression:** Leave the new `Use configurable grouped-input interpretation` checkbox unchecked in WasmSmoke. Select the familiar 3-column group dataset *with* headers. Read the range, run ANOVA/Welch or Kruskal–Wallis/Dunn and write the output. Confirm identical table order, numerical results and formatting to the Stage 3.04 fixture. Repeat an existing ANOVA or KW Windows Excel-DNA run.
2. **No-header:** On a separate worksheet use the following data, select `A1:C3`, enable configurable parsing with **Groups in columns / No header**, and click `Read source range` then Run. The preview should show groups `Group 1`–`Group 3`, each with `n=3`, and ANOVA should return `F=13`, `p=0.006591796875`:

    ```text
    1  2  5
    2  3  6
    3  4  7
    ```

3. **Long format:** Create `Category,Value` headers, then `A,1`, `B,2`, `A,2`, `B,3`, `A,3`, `B,4`. Select the two columns including headers; choose **Long / Header required**, category=1, value=2. Confirm each group has three values and execution succeeds. With one actual Excel error in the value column, verify a structured `cell.excel-error` diagnostic and its selected/worksheet coordinates; no worksheet output should be written.
4. **Row-oriented:** With rows `A,1,2,3` and `B,2,3,4`, choose **Groups in rows / Header required**, verify correct group order and counts.

Do not treat the current WasmSmoke task pane as a production user interface. Full reusable input-role validation and presentation are future phases.

## Rollback and next dependency

Apply the batch patch **or** the replacement-file ZIP, not both. To roll back, restore the two changed files (`GroupedNumericRangeParser.vb`, `Home.razor`) and remove the three newly added files (two Common modules and one Common MSTest module); this leaves all Phase 01.02 behavior intact. The next planned batch is **01.04 — scalar/vector/paired/grouped typed input parsers**, after the 01.03 .NET and Excel acceptance gates pass.
