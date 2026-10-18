# Batch 01.08 — matrix, classification, PSM, SPC and chart-source inputs

Implemented on `codex/01-08`, building on the existing 01.01–01.07 input
contracts. This batch adds parsing and validation in Common (`netstandard2.0`).
Statistical algorithms, fitted analyses, Windows/Office.js UI, framework
targets, signing and installers are unchanged. No subsequent batch is started.

## Files and entry points

- `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`: options, immutable aligned
  rows, matrix and family-specific input contracts.
- `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`: the five public
  entry points below, sharing one role/row validation implementation.
- `BESHStat.Excel.Common.Tests/src/AdvancedInputParserTests.vb`: 23 focused
  VB.NET MSTests, including ordinary, edge and compatibility cases.
- This architecture note. SDK project inclusion needs no project-file edit.

| Entry point on `AdvancedInputParsers` | Typed input | Validation |
|---|---|---|
| `ParseMatrix` | `SpreadsheetMatrixInput` | Finite numeric rectangle; configurable row/column minima; optional square shape after complete-case omission. |
| `ParseClassification` | `SpreadsheetClassificationInput` | Numeric features and typed class labels; at least two classes by default. |
| `ParsePsm` | `SpreadsheetPsmInput` | Numeric covariates and exactly the two declared treatment levels; both arms present after omission. |
| `ParseSpc` | `SpreadsheetSpcInput` | Individual measurements or one row per subgroup with multiple measurements; optional subgroup ID and strictly increasing numeric/date order. |
| `ParseChartSource` | `SpreadsheetChartSourceInput` | One or more numeric Y series, optional typed X labels/numeric/date coordinates and group IDs; optional increasing X within each group. |

Each entry point accepts the existing `SpreadsheetRangeData` and its
`Spreadsheet...ParseOptions`, returning `SpreadsheetTypedParseResult(Of T)`.
Failure returns `Data = Nothing` and structured diagnostics. All APIs are
additive; existing input parsers, serialization DTOs and golden fixtures remain
unchanged.

## Shared semantics

Indices are zero-based within the selected rectangle. Optional role indices
use `-1` for absence. Roles cannot overlap, and explicit numeric selections
must be nonempty, unique and valid. `NumericColumnIndices = Nothing` selects
all columns not assigned another role; an explicit array preserves its order
and leaves unselected columns out of complete-case eligibility. Source frames
retain those unselected cells, including native error payloads.

Headers default to `Required`; `None` supports headerless data. `Auto` is
rejected because a class, group or chart label could be mistaken for a heading.
The existing frame parser owns shape, heading and raw finite-number validation.

`SpreadsheetCellValuePolicy` owns numeric conversion and missingness. Text and
Boolean measurements require explicit coercion settings. Class, treatment,
subgroup and chart-label identities remain typed: number `1`, text `"1"` and
Boolean `TRUE` do not collide. Classification optionally folds text case;
`Rows(i).LabelLevel.Key` then maps directly to `Classes`, while `Label` keeps
the original spelling and source coordinates. Class order follows first
appearance among retained rows.

PSM defaults to numeric `1` treated and `0` control. Custom typed levels can
declare text or Boolean treatment codes. Treatment comparisons use the exact
declared category keys; numeric-text coercion never changes treatment identity.
`Rows(i).IsTreated` records arm membership alongside the original label cell.
This prepares matching inputs without computing propensity scores or matches.

Missing data are rejected by default. `Missing = Skip` omits the entire row
across every selected role/series with a coordinate-bearing warning. Invalid
cells still cause errors even if another field in the row is missing.
`Errors = Preserve` retains source metadata but cannot make an error a valid
analysis value. `TreatAsMissing` together with `Skip` explicitly allows omission.
The immutable `Source` frame retains omitted rows and original error text,
native error codes, number formats, workbook/sheet identity, source address,
selection revision and cell coordinates. `DateSerialOrigin` also survives in
the advanced input. Unknown worksheet coordinates remain zero, and host-native
address text is never parsed as A1.

`Rows` and `Columns` are read-only snapshots. Every numeric observation keeps
its original selected/worksheet coordinates. `ToArray()` returns a fresh
two-dimensional numeric array; mutations of input cells, option arrays or an
exported array cannot change the parsed input.

## Order, dates and chart alignment

With no explicit order/X role, `CoordinateValue` is the original one-based
selected row number, including the header offset and any gaps after omission;
`Coordinate` is absent. No sorting or row renumbering occurs.

An SPC row is one ordered sample/subgroup, whose numeric columns are its
measurements. Explicit order must strictly increase across retained source
rows, including across different subgroup identifiers. Without explicit order,
source order is retained and measurement values need not increase. SPC limits,
control rules, long-form subgroup aggregation and method-specific distribution
assumptions belong to later analysis handlers.

Charts accept categorical X by default, including repeated labels. Numeric X,
increasing X and date interpretation are explicit options. Increasing X is
checked within each typed group (globally without a group). All Y series share
one retained-row mask; missing values cannot independently shift series or X.
This batch does not create chart specifications, renderers or preview UI.

Date interpretation requires numeric source serials and a verified Excel
1900/1904 basis or explicit host `DateSerialOrigin`. Unknown/conflicting origins
and Excel's phantom day 60 fail through the existing policy. Number formats do
not infer date interpretation. No new date arithmetic is introduced.

## Preview fixtures and automated acceptance

The MSTest source contains deterministic preview fixtures without workbook
data or runtime-host dependencies:

| Fixture | Expected parsed preview |
|---|---|
| Matrix columns A/B/C, rows `1,2,3` and `4,5,6`, select C/A | Square matrix `[[3,1],[6,4]]`, numeric columns retain original source positions. |
| PSM T/X/Z rows `0,10,11`; `1,blank,21`; `1,30,31`, skip missing | Control/treated flags `False,True`, X/Z rows `10,11` and `30,31`, selected rows 2 and 4. |
| SPC Time/Subgroup/A/B rows `1,a,10,11`; `2,b,12,13` | Two subgroups, ordered coordinates 1 and 2, two measurements each; repeated time fails even across subgroup IDs. |

Focused compile/test command:

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --no-restore -c Debug
```

Final result: **231 Common tests passed, zero failed/skipped**, including all
23 new tests and the existing compatibility/snapshot suites.

Requested full command (attempted):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Python -Js -Wasm -Legacy -VisualStudio 2026
```

Core **1,024 passed**, Common **228 passed** at that point. The script then
stopped at Python discovery: only Windows Store aliases were available and
`py` was absent. Python validators and unittest suites did not execute. Three
additional Common regression tests were added afterward and passed as above.
Log: `TestResults/Codex/batch-01-08-validation.log`.

The remaining stages were attempted with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Js -Wasm -Legacy -VisualStudio 2026
```

Core **1,024 passed**, Common **228 passed**, WASM Debug build succeeded with
**zero warnings/errors**, JS **29 passed**, zero failed/skipped. VS 2026 MSBuild
was found and invoked, but its `ResolveHelpPython` target failed with MSB3073
for the same missing Python prerequisite. Legacy MSTests did not execute.
Log: `TestResults/Codex/batch-01-08-remaining-validation.log`.

Final source was validated with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Js -Wasm
```

Core **1,024 passed**, Common **231 passed**, WASM Debug build succeeded,
JS **29 passed**; zero test failures/skips and zero build warnings/errors.
Log: `TestResults/Codex/batch-01-08-final-validation.log`.

The Python and legacy failures are environment prerequisites, separate from
parser regressions. No package installation or validation bypass was performed.
To complete those gates, rerun the full command with `-PythonExe` pointing to an
existing Python 3.10+ executable with PyYAML.

Before legacy validation, byte copies of `VersionAttributes.vb`, `HelpLinks.vb`
and `HelpContext.vb` were saved beneath ignored
`TestResults/Codex/batch-01-08-preserved`. SHA-256 comparisons afterward
confirmed all three unchanged. Generated files were neither reset nor deleted.
Git checks use command-local `-c safe.directory=C:/Users/peter/source/repos/BESHstatNG`
because this sandbox account differs from the repository owner; no global Git
configuration was modified.

## Manual host acceptance (not run)

No new parser is wired into a host UI in this batch. Parser preview acceptance
is covered by the fixtures above; actual adapter integration remains separate.

1. Windows Excel-DNA: select `docs/architecture/golden-inputs/anova-stage304.csv`
   including headers, run ANOVA with Welch/Tukey to a new worksheet. Confirm
   F = 13, p approximately 0.006591796875 and unchanged tables/formatting.
2. Office/WASM: select the existing Kruskal–Wallis golden fixture and run with
   Dunn to a new worksheet. Confirm corrected H approximately 8.1603260869565215,
   corrected p approximately 0.016904709225411534 and unchanged table order.

No Excel process, installer, signing or publishing workflow was launched.
No commit or push was made. Review 01.08 before authorizing another batch.
