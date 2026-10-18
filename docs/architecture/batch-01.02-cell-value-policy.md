# Batch 01.02 — Canonical cell, missing-value, Excel error and date policies

**Baseline:** `BESHstatNG(20261016-115816).zip` (Batch 01.01 accepted).
**Change type:** additive Common contracts + Office.js acquisition fidelity + regression tests.
**Windows Excel-DNA production algorithms, GUI, registry, UDFs, writer and installer:** unchanged.

## Purpose and ownership

`SpreadsheetRangeData` remains the raw snapshot transport; `CoreDataTable` remains the generic portable calculation table. Neither is replaced by a new dataframe type. Batch 01.02 adds a deliberate, opt-in interpretation layer between them:

- `BESHStat.Excel.Common/src/Input/CellValuePolicy.vb`: cell classification, missing/error policies, numeric/coercion rules, date-system conversions.
- `BESHStat.Excel.Common/src/Input/SpreadsheetCoreTableBridge.vb`: validated range-to-`CoreDataTable` mapping with source row/column provenance, explicit missing mask and canonical numeric matrix.
- `SpreadsheetContracts.vb`: optional `SpreadsheetCellValue.NumberFormat` and `SpreadsheetRangeData.DateSystem`; both are additive. `DateSystem=Unknown` (0) is intentional when the host does not provide a verified workbook setting.
- `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js`: reads Excel's `valueTypes` and `numberFormat`; carries the cell kind and format without guessing date semantics. Returns `dateSystem: 0` until a verified Office host date-system capability is implemented.

## Canonical interpretation (default policy)

| Source cell | Kind | Missing | Numeric | Other semantics |
|---|---|---|---|---|
| Truly blank | `Blank` | Yes | No | Never confused with number zero |
| `0`, `2.5` | `Number` | No | Yes | Reject NaN and ±infinity |
| Literal text `""` or whitespace | `Text` | No | No | Configurable missing policy below |
| Text `"2,5"` or `"2.5"` | `Text` | No | No | Only parsed when explicitly opted in with a specific culture |
| `TRUE`/`FALSE` | `Bool` | No | No | Optional explicit Boolean numeric conversion |
| Excel error `#N/A` | `CellError` | No | No | Error policy is Reject (default), Preserve, or TreatAsMissing |
| Literal *text* `#N/A` | `Text` | No | No | **Never** auto-reclassified as an Excel error |
| Formatted date number | `Number` | No | Yes | Number format is a hint, not a date type; explicit date intent and verified basis required |

`SpreadsheetCellMissingTextPolicy` (BlankOnly / BlankOrEmptyText / BlankOrWhitespaceText), `SpreadsheetErrorPolicy` (Reject / Preserve / TreatAsMissing), and `SpreadsheetNumericTextPolicy` (Reject / Invariant / ExplicitCulture) express conversion choices **per workflow**. The grouped ANOVA/Kruskal–Wallis parser retains its original blank-skip, numeric-only data behavior; this patch does not silently change the meaning of any analysis.

`SpreadsheetExcelErrorValue` stores raw Excel error text and a normalized known code (or an empty code if unrecognized). An error cannot be manufactured just because a text cell contains `#REF!` or `#N/A`.

### Date semantics

Excel stores most dates and times as ordinary numbers. Therefore dates **must not be inferred** from a numeric value alone, from its format string, or from the machine's locale. `ExcelDateSerialConverter` has two explicit bases: `Excel1900` and `Excel1904`. The same date has serials differing by 1,462 days. It rejects Excel 1900 serials in the range `[60, 61)` because Excel's historical fictitious 1900-02-29 is not representable as a .NET date, along with nonfinite/negative/out-of-range serials. An unknown/unverified basis does not convert. Calendar date and time fractions round-trip when supported by `DateTime.FromOADate`.

Office.js's current prototype does not establish the workbook's 1900/1904 flag. Its returned `dateSystem: Unknown` is **not** a declaration that the workbook uses 1900. The actual feature/host-specific date-intent classification and settings acquisition belong to subsequent batches; the new converter is ready for that integration. This avoids silently shifting dates when a workbook is opened on Mac or online.

### CoreDataTable bridge

Call `SpreadsheetCoreTableBridge.ToCoreDataTable(input, columnNames, options)` after you have determined the expected column schema (no automatic header detection). The `ObjectMatrix` contains the source primitive kind (numbers, strings, booleans), or a typed `SpreadsheetExcelErrorValue` in Preserve mode; TreatAsMissing converts errors to `Nothing`. `MissingMask` and `NumericMatrix` use the **explicit Common policy** even if Core's generic object-to-double routine allows culture-dependent numeric strings. `RowIds` reflect source worksheet rows, and `SourceInfo` captures the address, sheet and first cell. Consumers of this bridge should use its canonical `MissingMask`, rather than re-running Core's generic `IsMissingValue` on raw strings; Core's generic string-empty semantics predate this separate cell policy.

New options do not affect the Windows `MultiGroupsUnpairedData` importer or existing `GroupedNumericRangeParser.Parse`; converting those code paths is deferred to the controlled parser/host migration batches.

## Regeneration and acceptance

No code generation or UDF docs changes are required. From the solution root:

```powershell
node --check BESHStat.WasmSmoke/wwwroot/js/officeBridge.js
node --test BESHStat.WasmSmoke/tests/*.test.cjs
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
python -m unittest discover -s BESHstatNG/tools/tests -p 'test_*.py' -v
python -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p 'test_*.py' -v

dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
```

Build the full Windows solution and run `BESHStatNG.Test` locally. New tests cover blanks/empty text, Excel error vs literal error-like text, numeric/culture/Boolean policies, nonfinite numbers, Excel date epochs, 1900 phantom leap day, Core source row identity, and legacy grouped parser invariance. The host tests verify format hints and typed blank/error distinction.

## Manual smoke cases

1. In Excel Online, select the original ANOVA sample and run the existing unchanged one-way ANOVA, then Kruskal–Wallis. Both must write the same ordered result tables and p-value semantics as 01.01.
2. Read a rectangular range with one number formatted as a date, a blank, the text `#N/A`, and a true Excel `#N/A` formula error. Verify through the selected-range inspection/UI or browser debugging that the transport preserves `kind` (1/0/2/4 respectively), and that the date-like number has `numberFormat` but remains numeric. Running grouped ANOVA on these mixed cells should continue to reject nonnumeric values; successful read does **not** imply successful numeric analysis.
3. On Windows Excel-DNA, rerun ANOVA with Welch/post-hoc to confirm no host behavior changed.

## Limitations and rollback

- No workbook-wide date-system probing, date-format inference, new GUI options, or automatic type coercion is introduced.
- Office.js `Range.numberFormat` is loaded alongside `valueTypes`; hosts lacking this API must be treated as unsupported rather than assigning a speculative date type.
- Full output-cell type/error policy and configurable long/wide/paired numeric parsing belong to later batches.
- Rollback: restore the previous `SpreadsheetContracts.vb` and `officeBridge.js`; remove the two new Common input modules and their tests and this document. SDK-style projects include new `.vb` files automatically.

**Next batch:** 01.03 — shared validation diagnostics and configurable header/orientation policies.
