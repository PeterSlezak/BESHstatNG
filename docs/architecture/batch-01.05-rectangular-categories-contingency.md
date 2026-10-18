# BESHStatNG Batch 01.05 — Rectangular data frame, category and contingency contracts

**Baseline:** `BESHstatNG(20261016-160852).zip`, with Batch 01.04 accepted locally.

## Purpose and compatibility

Adds host-neutral **rectangular data frame**, **typed categorical vector** and **contingency table** input contracts in `BESHStat.Excel.Common`. Input is still the existing `SpreadsheetRangeData` / `SpreadsheetCellValue` model from 01.01/01.02; parse results use `SpreadsheetTypedParseResult(Of T)` and `SpreadsheetValidationReport` from 01.03/01.04.

These are **input/parsing contracts**, not new statistical algorithms, analysis IDs, Ribbon methods or Office.js task pane flows. No Windows Excel-DNA, Core, WASM, Office bridge, existing ANOVA/Kruskal–Wallis parser or existing test fixtures have been changed.

## Files

- `BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb` — immutable typed frame cells/columns/rows, type-tagged category values and observations, categorical vectors, count-table contracts, configuration options.
- `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb` — `ParseDataFrame`, `ParseCategoricalVector`, `ParseContingencyCounts`, `ParseContingencyPairs`.
- `BESHStat.Excel.Common.Tests/src/RectangularCategoricalParserTests.vb` — regression tests for source identity, data types, category semantics, count validation and pairwise omissions.
- `BESHStat.Excel.Common/README.md` — updated scope; this architecture note.

SDK-style VB projects include the new `.vb` files automatically.

## 1. Rectangular data frame

`ParseDataFrame(input, options)` returns `SpreadsheetRectangularDataFrameInput` and a structured validation report. `SpreadsheetDataFrameParseOptions.Header` supports `Required` (default), `None` or conservative `Auto`:

- `Required`: the first row must contain nonblank text column names. Duplicate headings (case-insensitive by default) are errors. A data row must exist.
- `None`: generated column names (`Column 1`, etc.); all cells are data.
- `Auto`: first row of typed text **and** second row of numeric/boolean values infers a heading; all-numeric first row infers no heading; anything else is `header.ambiguous` and requires explicit selection. Do not infer on ambiguous categorical tables.

Each `SpreadsheetFrameCell` preserves raw `Kind`, value, `NumberFormat`, selected-range coordinates and known absolute worksheet coordinates. The returned frame snapshots the original values, so editing `SpreadsheetRangeData.Rows` afterward cannot change parsed cells. The frame also stores available workbook/sheet identity, selection ID/revision, source address and date-system hint. It does not reinterpret formatted numbers as dates.

## 2. Typed category values and vectors

`ParseCategoricalVector(input, options)` returns `SpreadsheetCategoricalVectorInput`: source-positioned observations and unique levels in **first-appearance order**. Options allow a selected column or row, required/no header, skip/reject missing, minimum observations and opt-in case-insensitive text levels.

Category identity is type-tagged: numeric `1`, text `"1"` and Boolean `TRUE` are three distinct levels. Text case is preserved by default. Literal text `"#N/A"` is text, not an Excel error. Missing values are determined by the existing 01.02 cell policy. Typed Excel errors **cannot** become levels; only explicit `Errors = TreatAsMissing` allows them to be skipped (subject to the missing policy). Numeric-text coercion is **not** applied to category identities.

`Header = Auto` for category observations is intentionally rejected: an initial text value could be either a header or a legitimate category. Use Required/None explicitly.

## 3. Labeled count tables

`ParseContingencyCounts(input, options)` consumes **already aggregated** nonnegative integer counts. Defaults assume a top row of column labels and left column of row labels (top-left corner ignored), at least two levels per axis, and a positive grand total. Both headers/labels can be disabled to generate `Row N` and `Column N` levels. Duplicate typed labels are errors.

Counts must be finite, nonnegative **exact integers** not exceeding the Excel-double safe integer limit (2^53−1), and the grand total must fit `Int64`. Text counts are rejected by default; enable `SpreadsheetCellPolicyOptions.NumericText = Invariant` explicitly to parse numeric strings. Missing counts are **errors**, never silently zero; `TreatMissingAsZero = True` enables zero with a warning. Excel error cells are rejected unless both `Errors = TreatAsMissing` and `TreatMissingAsZero = True` are selected. Zero frequency cells are otherwise valid; a completely empty count matrix is not.

The parsed `SpreadsheetContingencyTableInput` exposes read-only typed row/column levels, `Count(r,c)`, `GrandTotal`, defensive-copy `Counts`, `RowTotals` and `ColumnTotals`. Indices are **zero-based** in memory. Count tables do not silently apply marginal/totals rows or columns; select only the actual contingency data.

## 4. Individual categorical pairs

`ParseContingencyPairs(input, options)` builds the same count-table contract from two selected categorical columns (zero-based `FirstColumnIndex`, `SecondColumnIndex`) of individual observations. It maintains **pair alignment by row** and level order by first appearance. Skipped incomplete pairs drop **both** members and emit warnings; by default incomplete pairs are errors. Excel error cells are rejected by default. Optional `Header = Required` skips the first header row, but `Auto` is ambiguous for categorical data.

Both contingency parsers require at least two distinct category levels on each axis by default (configurable parser-level minimum). Method-specific chi-square/Fisher applicability and expected-frequency assumptions belong to later statistical handlers, not these contracts.

## Examples

```vb
Dim frame = RectangularCategoricalParsers.ParseDataFrame(selection,
    New SpreadsheetDataFrameParseOptions With {.Header = SpreadsheetHeaderMode.Required})

Dim category = RectangularCategoricalParsers.ParseCategoricalVector(selection,
    New SpreadsheetCategoricalVectorParseOptions With {
        .ColumnIndex = 1,
        .Header = SpreadsheetHeaderMode.Required,
        .Missing = SpreadsheetIncompleteObservationPolicy.Skip})

Dim matrix = RectangularCategoricalParsers.ParseContingencyCounts(countRange)
If matrix.Success Then
    Dim total As Long = matrix.Data.GrandTotal
End If

Dim pairedCategories = RectangularCategoricalParsers.ParseContingencyPairs(longRange,
    New SpreadsheetContingencyPairsParseOptions With {
        .Header = SpreadsheetHeaderMode.Required,
        .Missing = SpreadsheetIncompleteObservationPolicy.Skip})
```

Always check `.Success` and `.Validation.Issues` before passing data to statistical code. Failed parses have `.Data = Nothing`; diagnostics provide stable codes, selected-cell coordinates, and absolute worksheet coordinates when provided.

## Representative diagnostics

`range.null`, `range.shape`, `range.not-rectangular`, `header.invalid`, `header.duplicate`, `header.ambiguous`, `header.inferred`, `frame.no-data`, `options.invalid`, `vector.index`, `vector.orientation-ambiguous`, `category.missing`, `category.missing-skipped`, `category.too-few`, `category.duplicate`, `cell.invalid-value`, `cell.excel-error`, `contingency.shape`, `contingency.missing`, `contingency.missing-zero`, `contingency.invalid-count`, `contingency.overflow`, `contingency.empty`, `contingency.incomplete`, `contingency.incomplete-skipped`.

## Acceptance

From the repository root:

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
node --check BESHStat.WasmSmoke/wwwroot/js/officeBridge.js
node --test BESHStat.WasmSmoke/tests/*.test.cjs
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
python -m unittest discover -s BESHstatNG/tools/tests -p "test_*.py" -v
python -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p "test_*.py" -v
```

Also build the full Windows solution, run the legacy tests, and repeat the approved Excel GUI smoke tests. These are new Common API contracts exercised by MSTest; no new GUI control or analysis is enabled by this batch. On systems without .NET/Excel, source and JavaScript/Python checks **do not substitute** for the remaining .NET and host acceptance gates.

## Rollback

Delete the two new Common VB input modules, the Common test module, and this note; restore `BESHStat.Excel.Common/README.md`. Existing production files and Core statistical algorithms were not changed.
