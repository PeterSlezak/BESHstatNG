# BESHStatNG Batch 01.04 — scalar, vector, paired and grouped numeric input parsers

**Baseline:** `BESHstatNG(20261016-154002).zip`, following locally accepted Batch 01.03 and its whole-column/Office.js corrections.

## Purpose and compatibility boundary

Introduce reusable **typed numeric analysis inputs** in `BESHStat.Excel.Common` while retaining the Stage 3.04 and 01.03 production workflows. Batch 01.04 does **not** modify statistical calculations, Windows Excel-DNA, the analysis catalog or registry, the Office.js bridge, the WASM user interface, output contracts, Ribbon handlers or generated metadata.

The existing `GroupedNumericRangeParser.Parse(input)` is unchanged. The configurable `GroupedNumericRangeParser.ParseWithDiagnostics` and `ConfigurableGroupedNumericParser.Parse` from Batch 01.03 are also unchanged. The new grouped entry point is a thin adapter, **not** another implementation of group aggregation.

## Added files

- `BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb` — immutable numeric observations with worksheet coordinates; `SpreadsheetScalarNumericInput`, `SpreadsheetVectorNumericInput`, `SpreadsheetPairedNumericInput`, `SpreadsheetNumericPair`, and the generic `SpreadsheetTypedParseResult(Of T)`. Also includes vector/paired options and explicit orientation/missingness enums.
- `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb` — `ParseScalar`, `ParseVector`, `ParsePaired`, and `ParseGrouped` (grouped delegates to 01.03).
- `BESHStat.Excel.Common.Tests/src/TypedNumericRangeParserTests.vb` — focused MSTest coverage of shapes, headers, missing values, aligned pairs, coercion, source coordinates, Excel errors, and existing grouped reference results.
- This document; the Common README mentions the new contracts.

The SDK-style VB projects automatically compile these new `.vb` files; no explicit project-file `Compile` updates are required.

## Parsers and types

| Entry point | Result data | Selection and behavior |
|---|---|---|
| `ParseScalar(range, policy)` | `SpreadsheetScalarNumericInput` | Exactly **1 × 1** cell; numeric, nonmissing, finite; no implicit Boolean/text/date conversions |
| `ParseVector(range, options)` | `SpreadsheetVectorNumericInput` | Row or column series, optionally indexed from a larger rectangle; Auto orientation only when range is one-dimensional; Required/None/Auto header; reject or skip missing cells |
| `ParsePaired(range, options)` | `SpreadsheetPairedNumericInput` | Two **distinct** columns (pairs by row) or rows (pairs by column); Required/None/Auto header; reject incomplete pairs by default, or **skip the entire pair** without misaligning series |
| `ParseGrouped(range, options)` | `GroupedNumericData` | Wraps the established 01.03 wide-columns, wide-rows, or long-format parser and exposes the same generic diagnostics/result contract |

The generic result provides `.Success`, `.Data` and `.Validation`. On normal validation failure, `.Data = Nothing`, `.Success = False`, and `.Validation.Issues` contain the stable issue code, severity, field path, selected-range (one-based) coordinates and worksheet coordinates if known. Source cell objects are never changed.

`SpreadsheetNumericObservation` provides `Value`, `SelectedRow`, `SelectedColumn`, `WorksheetRow` and `WorksheetColumn`. A zero worksheet coordinate indicates unverified/missing origin information. Vectors expose read-only observation collections and defensive-copy `Values` arrays. Paired inputs expose read-only pairs, defensive-copy first/second value arrays and source positions **for both members of each pair**.

The new API supports the current 01.02 `SpreadsheetCellPolicyOptions`, including explicit invariant/culture numeric-text and Boolean conversion, missing-text policies, and Excel error handling. Text containing `#N/A` is not automatically treated as an Excel error. A formatted Excel date serial remains numeric rather than becoming a `DateTime` without an explicit date interpretation. The older grouped parser's semantics are unaffected.

### Default interpretation

- Scalar: strict numeric; missing is an error.
- Vector: one-dimensional orientation automatically detected; **no header** by default, true blanks skipped, minimum one numeric observation.
- Paired: first two columns; **no header** by default, incomplete pairs rejected, minimum one complete pair. `Missing=Skip` explicitly skips both members of each incomplete pair; optional warnings identify omissions.
- Grouped: existing 01.03 configured default (wide columns, required header, minimum two observations per group). The legacy one-parameter parser remains available independently.

Default minimum observations/pairs are **parsing constraints only**, not statistical-method assumptions. Analyses such as a paired t-test must enforce their own higher sample-size requirements in a later typed-handler batch.

Automatic header detection is conservative. A clear typed text label followed by numeric data can identify a header; numeric first values imply no header. Blank, Excel-error or mixed paired label/numeric candidates are `header.ambiguous` rather than silently dropped. Explicit `Required` overrides inference.

## Example calls

```vb
Dim scalarResult = TypedNumericRangeParsers.ParseScalar(singleCell)
If scalarResult.Success Then
    Dim value As Double = scalarResult.Data.Value
End If

Dim vectorResult = TypedNumericRangeParsers.ParseVector(seriesRange,
    New SpreadsheetVectorParseOptions With {
        .Orientation = SpreadsheetVectorOrientation.Column,
        .ColumnIndex = 0,
        .Header = SpreadsheetHeaderMode.Required,
        .Missing = SpreadsheetIncompleteObservationPolicy.Skip})

Dim pairedResult = TypedNumericRangeParsers.ParsePaired(pairedRange,
    New SpreadsheetPairedParseOptions With {
        .Orientation = SpreadsheetPairOrientation.TwoColumns,
        .Header = SpreadsheetHeaderMode.Required,
        .Missing = SpreadsheetIncompleteObservationPolicy.Skip})
' pairedResult.Data.Pairs contains only aligned complete pairs, if successful.

Dim groupedResult = TypedNumericRangeParsers.ParseGrouped(groupRange,
    SpreadsheetGroupedParseOptions.LegacyWideHeaders())
```

Use diagnostics before invoking any Core engine, and distinguish parser validation from later method-specific constraints.

## New diagnostic codes

- `range.null`, `range.shape`, `range.not-rectangular`
- `scalar.shape`, `scalar.missing`
- `vector.orientation-ambiguous`, `vector.index`, `vector.missing`, `vector.missing-skipped`, `vector.too-few`
- `paired.indices`, `paired.incomplete`, `paired.incomplete-skipped`, `paired.too-few`
- `cell.invalid-value`, `cell.excel-error`, `cell.not-numeric`
- `header.ambiguous`, `header.inferred`, `options.invalid`

For `ParseGrouped`, diagnostic codes are inherited **unchanged** from the 01.03 configurable grouped parser.

## Acceptance checks

From the repository root on the Windows development machine:

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

Also build the full Windows solution, run `BESHStatNG.Test`, and repeat a small ANOVA/Welch and Kruskal–Wallis/Dunn smoke check on the original inputs. With no host files modified, the user-visible Excel behavior should be unchanged. New typed contracts are exercised directly through MSTest in this batch; host-specific role-selection UI is deferred.

### Suggested sample values

- Scalar: cell B2 containing `12.5` ⇒ 12.5. Text `"12.5"` is rejected by default but may be opt-in parsed.
- Vector: B1:B5 with `Score,1,(blank),3,4`; Header Required and Missing Skip ⇒ `[1,3,4]` and the original cell coordinates.
- Paired: A1:B5 with `Before,After` / `1,10` / `(blank),20` / `3,(blank)` / `4,40`; Header Required and Missing Skip ⇒ pairs `(1,10)` and `(4,40)` without shifting observations.
- Grouped: `A,B,C` with the three familiar rows `1,2,5`, `2,3,6`, `3,4,7` ⇒ ANOVA `F=13`, p-value `0.006591796875` (unchanged golden input).

## Limitations and next dependency

This is an additive typed **numeric** input layer, not an input-role UI, general-purpose dataframe, categorical/date-vector implementation, or new statistical model. Host-specific selection workflows and method-specific sample-size/parameter validation are future work. The new public classes have no dependency on Excel COM, Excel-DNA, WinForms or browser JavaScript.

To roll back, delete the two new Common VB modules, the new Common test module, and this architecture document, and restore the Common README; all existing project sources are unchanged.
