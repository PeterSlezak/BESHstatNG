# Batch 02.02 — Canonical output cells and semantic styles

Implementation pending independent review; this note does not accept the batch.
Source: clean `codex/02-02`, HEAD/main `b396d49`, accepted 02.01 `57aa52f`.
The two existing stashes are preserved. Revision 1.4 has 222 total batches,
20 accepted / 202 outstanding; 02.02 is current. All 74 Phase 04A batches and
their dependencies remain future scope after 04.07.

## Pre-change compatibility inventory

- `OutputDocument.vb`: ordered immutable table/diagnostic/plot snapshots, copied
  matrices/lists, scalar whitelist (including raw nonfinite floats), section
  validation. P-value annotations are unmodified one-based body coordinates,
  including malformed DTO entries. No serialization or rendering policy.
- `ResultTableWriteRequest.vb`: mutable legacy primitive transport. Factory uses
  Core number formats and 0.05 for nonfinite/out-of-(0,1) alpha. NaN becomes
  literal `#N/A`, infinities `#Pinf`/`#Ninf`. Leave this contract untouched.
- `AnalysisResultDocument.vb`: frozen v1 structural snapshot, not full cell
  serialization. Five Stage304 goldens and 02.01 tests remain compatibility gates.
- Core `ResultTableOutput.vb`: titles/top headers precede body, footers follow;
  left headers offset body columns. DTO lists allow invalid/duplicate entries.
- Core `PValuePresentation`/`PValuePresentationSettings`: decimals 2–16, default
  8; invalid settings normalize via `EnsureDefaults`. Lower threshold 10^-d,
  upper 1-lower; strict < / > display boundaries; threshold/scientific/fixed
  modes. `FormatForDisplay` accepts an explicit provider; omitted provider uses
  current culture. New previews always supply an explicit culture.
- Windows writer: gated by `IsResultTable`; title/footer/font/border settings,
  left-header loop uses `hLeft - foots` across columns. P-value offsets are
  body-relative but invalid zero/negative columns can reach left headers.
  Highlight uses `Convert.ToDouble(CurrentCulture)`, so numeric-looking strings
  can highlight; Boolean is excluded. Significance currently uses green alone.
- Office bridge: body-only left-header bolding; strict finite JS numbers for
  significance, green alone, deduplicates cell/column overlaps. Invalid body
  columns can also reach left headers. Alpha is checked for finiteness only.
  These source observations are deferred integration issues for 02.04/02.05,
  not claims of measured live parity.

## Planned files and policy decisions

Add only Common `Output/OutputCell.vb`, `Styles.vb`, `PValuePresentation.vb`,
three focused test files, normalized test fixtures and this note. Add the new
fixture copy rule to the Common test project. Correct only active checkpoint
paragraphs in the master plan; preserve its historical inventory and 04A ledger.

Cells use explicit blank/number/text/Boolean/error/trusted-formula kinds.
Binary floating values and exact decimal/integer values have separate numeric
representations so large integers and decimals never silently lose precision.
Nonfinite floats become portable nonfinite errors without native spreadsheet
codes; marker strings stay text. Unsupported payloads fail explicitly.

The planner emits one immutable row-major entry per table cell, retaining its
section role while overlaying p-value intent and non-color significance state.
Malformed annotations are skipped with ordered diagnostics; duplicates and
overlaps are deduplicated. Settings are copied and Core-normalized; invalid
alpha falls back to 0.05. No writer, numerical engine or global store changes.

## Implemented API and safe conversion

All production types are in `BESHStatNG.ExcelCommon` (the project root namespace
supplies `BESHStatNG`), with `Option Explicit On` and `Option Strict On`.

| Contract | Purpose |
|---|---|
| `OutputCell` | Closed immutable scalar union; factory methods select an explicit discriminator |
| `OutputCell.FromRaw(Object)` | Bridge for exactly the 02.01 scalar whitelist; unsupported types throw `ArgumentException` without stringification |
| `OutputCellTable(OutputTableArtifact)` | Independent canonical matrix snapshot; `CellAt(row,column)` validates zero-based array coordinates |
| `OutputNumberFormatIntent` | General, Numeric, Percentage, Scientific or PValue token, with validated optional precision |
| `OutputCellStyle` | Artifact ID, local cell ID, row/column, canonical value, resolved section role, numeric intent, significance/emphasis and optional preview |
| `OutputArtifactStylePlan` | Read-only row-major cells or warning/fault semantic role; plot references have no cell style |
| `OutputPlanDiagnostic` | Artifact ID (empty for global options), stable code and optional original annotation-list index |
| `OutputPValuePolicy` | Immutable copied/normalized Core settings and Core-derived thresholds; `Preview` delegates to Core with explicit culture |
| `OutputSemanticPlanner.Create` | Overloads for document/table; explicit settings and alpha, optional named preview culture |
| `OutputSemanticPlan` | In-memory v1 semantic graph, ordered artifacts and planner diagnostics; no destinations or host commands |

Cell kinds and selected payloads:

| Kind | Payload and policy |
|---|---|
| Blank | No payload; raw `Nothing`; distinct from empty string |
| Number / Binary64 | Finite Double; Single widens exactly to Double. Signed zero/subnormal/maximum values retain their bits |
| Number / ExactDecimal | Decimal, including every signed/unsigned CLR integer exactly; ULong.MaxValue and 96-bit Decimal limits require no rounding |
| Text | Literal String, including empty, error markers and formula-looking content |
| BooleanValue | Boolean, distinct from numeric 0/1 |
| Error | Portable error kind and stable code; original token, supplied native code and caller-approved safe detail are optional |
| Formula | Caller-attested `TrustedFormula(expression, language)`; distinct fields from text; explicit logical dialect, no grammar/evaluation/reference translation |

`NumberCell(Double)` rejects nonfinite input; `FromRaw` instead maps NaN and
positive/negative infinity (both Single/Double) to `NonFinite` with codes
`number.nan`, `number.positive-infinity`, `number.negative-infinity`. These are
portable conditions, **not** invented Excel errors: original/native error codes
are absent. No finite placeholder is substituted. All legacy marker strings
remain Text, and raw artifact values remain nonfinite floats.

`SpreadsheetError(token, optionalNativeCode, optionalSafeDetail)` is explicit,
ordinal and case-sensitive. It supports `#NULL!`, `#DIV/0!`, `#VALUE!`, `#REF!`,
`#NAME?`, `#NUM!`, `#N/A`, `#GETTING_DATA`. Their portable codes are respectively
`spreadsheet.null`, `.division-by-zero`, `.value`, `.reference`, `.name`,
`.number`, `.not-available`, `.getting-data`. Every other token (including empty,
`#SPILL!` and differently cased tokens) becomes Unknown / `spreadsheet.unknown`,
retaining the original token and only the native code actually supplied. There
is no native-code lookup, synthesis or guessed modern-error mapping. Safe detail
is caller-approved display content; the bridge does not capture host objects,
workbook data or arbitrary exception messages.

Unsupported Object/array/Date/COM/Office/DBNull/enum/structure/Char inputs fail
as a whole conversion, rather than producing partial output or `ToString()`
fallback. The supported numeric union covers the entire finite whitelist, so
there is no overflow conversion to Double for large integers/decimals. Any
future binary-only host adapter must detect and explicitly reject/report loss
before writing exact decimals. This batch supplies no such adapter.

Formula factories reject blank intent/dialect and control characters. They do
not assert that an expression is syntactically valid or safe to execute in an
arbitrary host. Future adapters must recognize the declared language and opt in
to executing trusted intent; raw `=SUM(A1:A2)` strings never enter that path.
Formula serialization tests verify inert separated intent/language fields.

## Style targeting, precedence and ordering

Target identity is `(ArtifactId, Row, Column)`, with zero-based table array
coordinates. `LocalId` is invariant `row:column`, unique only inside that
artifact. Artifact order is source document order, cells are row-major. There
are no random IDs or platform conditions. Diagnostic artifacts retain warning
or fault roles and non-color accessibility tokens `warning` / `fault`; plots
remain references with no invented rendering style or geometry.

Each cell has exactly one section role: Title, then TopColumnHeader, then
Footer, then LeftRowHeader inside body rows, otherwise Body. Validated section
counts make the row sections disjoint. Left headers never supersede title,
top-header or footer roles. Top and left headers carry strong emphasis; title
and footer remain separate semantic roles for future adapter font choices.
PValue overrides only number-format intent and adds significance state; the
Body role remains. It cannot select any other section. Significance adds
`StrongEmphasis=True` and `AccessibilityIndicator="significant"`, independent
of color and without modifying the cell value. Hosts choose its accessible
visual/text representation; the planner does not insert an asterisk into data.

General has no decimal precision. Numeric/Percentage/Scientific support 0–16;
PValue supports 2–16, generated using the normalized Core settings. Invalid enum
tokens, missing/inapplicable precision and out-of-range precision throw. These
tokens are reusable contracts, not a new custom-style override UI. Roles describe
all table matrices independently; the original `IsResultTable` flag and raw
metadata remain in the source artifact. The semantic plan enriches that source,
and does not replace it or the legacy writer's formatting gate.

Annotation translation occurs once, in the planner:

```text
firstBodyRow = TitleRows + HeaderTopRows
bodyRows = RowCount - firstBodyRow - FooterRows
bodyColumns = ColumnCount - HeaderLeftColumns
arrayRow = firstBodyRow + BodyRow - 1
arrayColumn = HeaderLeftColumns + BodyColumn - 1
```

Both BodyRow and BodyColumn must be >=1 and within the computed body. Whole
column annotations expand only across body rows. With two titles, two top
headers and one left header, `(1,2)` maps to array `(4,2)`. Zero-body/zero-column
tables have no targets, including header-only and zero-dimension matrices.
Row-major target emission uses a set, so repeated valid columns/cells and
column+cell overlap have one identical resolved instruction. Invalid entries
are skipped individually with `pvalue.column.invalid` / `pvalue.cell.invalid`
and their zero-based source-list index, columns before cells. A column in an
empty body is invalid. Valid duplicate annotations are silently deduplicated.
Raw lists, including invalid and repeated entries, remain unchanged.

## Shared p-value policy and precision

Invalid alpha (NaN/infinity or outside the open interval (0,1)) uses the legacy
factory's 0.05 fallback and diagnostic `pvalue.alpha.defaulted`. Invalid decimal
count or small-value enum is copied then normalized through Core
`EnsureDefaults`, with `pvalue.settings.defaulted`. The caller's settings and
global settings store are never mutated. Diagnostics appear settings, alpha,
then artifact/source-annotation order. Absence of settings uses Core defaults.

There is one significance predicate, `OutputCell.IsSignificantAt(alpha)`:
typed finite probability in [0,1] and `p <= alpha`. Text, Boolean, blank,
formula, errors, negative and >1 values never qualify. Binary64 compares
directly. ExactDecimal compares its 96-bit integer / 10^scale against the
actual binary64 alpha using integer cross multiplication (`System.Numerics`,
part of netstandard2.0). This avoids overflow and avoids rounding a decimal
just above alpha into significance. For example, exact
`0.050000000000000003` rounds to Double 0.05 for preview, but remains greater
than binary64 alpha 0.05 and is not significant. No statistical algorithm or
Core numerical result is changed by this presentation-only comparison.

Only optional display preview projects a valid exact-decimal probability to
Core's Double display API; the primary decimal remains exact. The policy calls
Core `LowerThreshold`, `UpperThreshold` and `FormatForDisplay`, retaining all
strict threshold/scientific/fixed/upper-bound behavior. Canonical intent is the
mode, precision, bounds and significance, not `BuildExcelNumberFormat` output.
That existing translator remains unchanged for current writers.

For decimals=3, alpha=0.05, threshold mode and upper bound enabled:

| Numeric p | Invariant / en-US preview | sk-SK preview | Significant |
|---:|---|---|---|
| 0 | `<0.001` | `<0,001` | yes |
| 0.001 | `0.001` | `0,001` | yes |
| 0.05 | `0.050` | `0,050` | yes |
| 0.05000000000000001 | `0.050` | `0,050` | no |
| 0.999 | `0.999` | `0,999` | no |
| 1 | `>0.999` | `>0,999` | no |

Scientific mode displays 0.0001 as `1.000E-004` (or `1,000E-004`); fixed mode
displays it as `0.000`. Upper-bound choice is independent of small-value mode.
`previewCultureName=Nothing` omits previews, `""` explicitly selects invariant,
`"en-US"` / `"sk-SK"` record named cultures; no machine-default provider is used.
Invalid probabilities and nonnumeric marked cells have no preview. A number
format token can still describe an annotated text/blank cell without coercing it.

## Normalized fixtures and review previews

Six read-only test fixtures live in
`BESHStat.Excel.Common.Tests/TestData/OutputSemantics/`: all five accepted
Stage304 analysis cases and `generic-mixed`. They cover complete canonical
values, every section role, number intent, significance and non-color emphasis,
policy, culture, artifact order and diagnostics. The generic document interleaves
two tables, warning/fault and a logical plot reference, including exact large
integers, NaN, blank/empty, literal error/formula strings and Boolean values.

The normalized JSON **test projection v1** is additive and test-only. It uses
symbolic enum names, invariant decimal strings (so JS JSON numbers cannot lose
large-integer precision), round-trip binary64 strings plus hexadecimal bits,
explicit `null` for absent native error codes and absent previews, stable
property/artifact/cell/diagnostic order. Example excerpts:

```json
{"kind":"Number","representation":"ExactDecimal","exactDecimal":"9007199254740993"}
{"kind":"Error","errorKind":"NonFinite","code":"number.nan","nativeCode":null,"original":null,"safeDetail":null}
```

This does not add a general raw OutputDocument deserializer or change
`AnalysisResultDocumentSnapshot` v1. `OutputSemanticPlan.SchemaVersion=1`
identifies the new in-memory plan; transport adapters must deliberately preserve
its exact decimals rather than blindly treating all JSON numbers as binary64.
No JSON package is added to Common. Fixture comparison never overwrites expected
files; actual projections are written only to ignored test-run diagnostics.

Deterministic selected cells at decimals=3, alpha=0.05 (zero-based coordinates):

| Accepted fixture / target | Raw value | Preview | Semantic significance |
|---|---|---|---|
| anova-all-posthoc / table-0 [1:5] | Double 0.006591796875 | `0.007` | strong + significant |
| anova-all-posthoc / table-0 [1:8] | Double 0.023156899810964138 | `0.023` | strong + significant |
| anova-all-posthoc / table-1 [2:4] | Text `0.0027136821` | absent | none; text preserved |
| kw-dunn / table-0 [4:1] | Double 0.01949689610859806 | `0.019` | strong + significant |
| kw-dunn / table-0 [6:1] | Double 0.01690470922541154 | `0.017` | strong + significant |
| kw-dunn / table-1 [1:3] | Double 0.692956491950643 | `0.693` | none |
| kw-dunn / table-1 [2:3] | Double 0.013330444330535762 | `0.013` | strong + significant |
| generic-mixed / generic-a [2:2] | Double 0.05 | `0.050` | strong + significant |
| generic-mixed / generic-a [3:2] | Text `0.01` | absent | none; text preserved |

The actual ANOVA matrices have top headers rather than title rows. Several
post-hoc p-values are already strings, and whole-column annotations include
blank body cells; the planner preserves both. There is no parsing of stored
numeric-looking text and no inferred statistical table schema. Generic tests
separately cover multiple actual title/top-header/footer rows.

## Compatibility, Phase 04A and deferred work

Existing 02.01 APIs, scalar whitelist, raw matrices/metadata and defensive copies
are unchanged. Analysis v1 and legacy request/factory remain unchanged, including
their nonfinite string markers and current host number-format translation.
No original tests or goldens are edited or weakened. Common/Core remain
netstandard2.0, modern tests/WasmSmoke net10.0, Windows host/tests Framework 4.8.

Future Windows-extracted ANOVA/mixed/PSM/SPC/multivariate/chart result tables may
use these generic section/number/emphasis/error tokens and structured diagnostic
styles. This implements no Phase 04A feature or ledger entry. Native chart
semantics, geometry and rendering are separate; plot references get no new
payload. The companion portability audit is protected byte-for-byte.

02.03 owns placement, spacing, worksheet bounds/collision, destinations and
overwrite planning. 02.04 and 02.05 must adapt the same semantic plan to Windows
and Office.js respectively, including typed error/formula handling, decimal
representability, native number-format translation, accessible emphasis and
existing `IsResultTable` behavior. 02.06–02.08 own the remaining overwrite,
capability/cancellation, chart and composite-output responsibilities.

Deferred source-observed issues: numeric-looking ANOVA post-hoc strings can
highlight in Windows but not Office; Windows' left-header loop subtracts footer
count; both writers can accept invalid body columns into left-header areas;
alpha validation differs; green alone has no independent accessibility signal.
Neither implementation is fixed here, and live parity is **not** claimed.

Strict non-goals: no live writer changes, layout/destination/overwrite/chart
rendering, Ribbon/UDF/statistical changes, new production UI, LibreOffice,
package/framework migration, installers, signing, deployment or network installs.

## Manual acceptance guide (not executed)

1. Review `anova-all-posthoc.json` and `kw-dunn.json` semantic fixtures and the
   preview table above before any host write. In existing Excel-DNA and
   Office/WASM workflows run the matching accepted Stage304 synthetic inputs
   (ANOVA: A={1,2,3}, B={2,3,4}, C={5,6,7}; KW/Dunn: A={1,2,2,3},
   B={2,3,4,4}, C={4,5,5,6}) with all ANOVA post-hoc options, then KW/Dunn.
   Compare existing tables/footnotes and actual stored numeric vs text types.
2. With display settings decimals=3, threshold mode, upper bound enabled and
   alpha=0.05, compare native display with the semantic expectation. Inspect
   overall ANOVA/Welch and KW/Dunn numeric p-values and the string-valued ANOVA
   post-hoc cells separately. Record Windows/Office differences above for
   02.04/02.05. Repeat a preview with `sk-SK`; numeric storage must not change.

These are future reviewer checks using existing workflows. No live Excel,
browser/Office session or replacement GUI was launched by Codex.

## Validation and protected files

Commands executed from the repository root (Debug; no dependency installs):

```powershell
# Existing 02.01 / analysis v1 / five-golden compatibility run: 37 passed, exit 0.
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Debug --no-restore --filter 'FullyQualifiedName~OutputDocumentTests|FullyQualifiedName~AnalysisResultDocumentTests|FullyQualifiedName~Stage304ContractSnapshotTests' --logger 'trx;LogFileName=batch-02-02-compatibility.trx' --results-directory TestResults/Codex

# Final focused new + compatibility run: 89 passed, exit 0.
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Debug --no-restore --filter 'FullyQualifiedName~OutputCellTests|FullyQualifiedName~OutputStylesTests|FullyQualifiedName~PValuePresentationPlanTests|FullyQualifiedName~OutputDocumentTests|FullyQualifiedName~AnalysisResultDocumentTests|FullyQualifiedName~Stage304ContractSnapshotTests' --logger 'trx;LogFileName=batch-02-02-focused.trx' --results-directory TestResults/Codex

# Final full gate: exit 0.
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\verify-beshstatng.ps1 `
  -Python `
  -PythonExe "C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe" `
  -Js -Wasm -Legacy -VisualStudio 2026

# Git commands use a command-local ownership exception, not global configuration.
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
```

| Suite/build | Accepted post-02.01 baseline | Final 02.02 | Exit |
|---|---:|---:|---:|
| Core | 1,024 | 1,024 passed | 0 |
| Common | 273 | 325 passed (+52) | 0 |
| Windows legacy | 561 | 561 passed | 0 |
| Office.js | 74 | 74 passed; syntax check passed | 0 |
| Python tooling / docs / discovery | 21 + 5 + 5 | 21 + 5 + 5 passed | 0 each |
| **Total tests** | **1,963** | **2,015 passed**, zero failures/skips | **0** |
| .NET 10 WASM Debug | Passed | Passed, zero compiler warnings/errors | 0 |
| VS2026 Framework 4.8 Debug | Passed | Passed, zero compiler warnings/errors | 0 |
| Excel-DNA Debug x86 / x64 | Packed | Both packed successfully | 0 |

The full script invokes each child through `Invoke-Checked`; reaching completion
with exit 0 confirms their exit codes. TRX counters independently confirm
1,024 / 325 / 561 executed/passed with zero failed/not-executed. The focused
TRX confirms 89 executed/passed. Catalog, generated metadata `--check`, retired
helper and frozen Stage304 inventory checks passed. No requested automated gate
was omitted or prevented by environment restrictions.

The existing legacy discovery advisory persists:
`UTA001: TestClass attribute defined on non-public class BESHStatNG.Test.MatrixTestHelpers`.
It causes no failures/skips. Existing TRX overwrite notices and the Git
CRLF-to-LF advisory for protected `VersionAttributes.vb` are informational.
PowerShell redirection labels successful Python unittest stderr as
`NativeCommandError`; all three Python summaries are OK and the script exits 0.
No compiler warnings/errors appeared in the final WASM/Windows builds.

Intermediate new-test iterations exited 1 for a test-only multidimensional-array
declaration (fixed) and six not-yet-created expected semantic fixtures. Those
actual projections were inspected, including p-value coordinates/raw types,
then frozen as the six new expected files. Subsequent comparisons never update
expected fixtures. Final precision additions cover exact-decimal comparison
against binary64 alpha, subnormal alpha and Core setting endpoints/defaults;
the full gate was rerun after those changes. Existing tests were not changed.

Ignored local evidence:

- `TestResults/Batch0202/focused-validation.log`
- `TestResults/Batch0202/final-full-validation.log`
- `TestResults/Batch0202/full-validation.log` (earlier successful intermediate run)
- `TestResults/Codex/{Core,Common,Legacy,batch-02-02-focused,batch-02-02-compatibility}.trx`
- `TestResults/Codex/Deploy_*/OutputSemanticActual/*.json` (read-only fixture diagnostics)
- `TestResults/Batch0202/protected/` and `generated-after-build/` (byte captures)

Windows builds regenerated both help files and `VersionAttributes.vb`. Each
changed file was archived and only that file restored from its pre-edit byte
capture; no Git reset/clean/restore or stash operation was used. SHA-256 equality
was confirmed after restoration:

| Protected path | SHA-256 |
|---|---|
| `BESHstatNG/src/Help/HelpContext.vb` | `FCDA18194593897DF66FBE7825886469E5C8A9DEF9F752AF5CD75DD9FD2BE14E` |
| `BESHstatNG/src/Help/HelpLinks.vb` | `291AC834EC2AF86DEBBB79E74A3A02EA940E2178A629E7ECDDEF16F58824B60E` |
| `BESHstatNG/Generated/VersionAttributes.vb` | `96CAA733CA6E8301A12DB3EFE1353D3EC6D881F9098975AD3A8DB52B477CD766` |

The Phase 04A companion audit is also byte-identical to its captured copy
(`6097F5FFCD025F964A0A4CF8AA595912B24848705B707CF92FC55654836E9948`).
The master-plan diff changes only four active checkpoint paragraphs, retaining
222 / 20 / 202 and all future scope/dependencies/historical inventory. Both
original stashes remain intact. No pre-existing working-tree edits existed.

All changed/new paths (15 files):

- `BESHStat.Excel.Common/src/Output/OutputCell.vb`
- `BESHStat.Excel.Common/src/Output/Styles.vb`
- `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`
- `BESHStat.Excel.Common.Tests/src/OutputCellTests.vb`
- `BESHStat.Excel.Common.Tests/src/OutputStylesTests.vb`
- `BESHStat.Excel.Common.Tests/src/PValuePresentationPlanTests.vb`
- `BESHStat.Excel.Common.Tests/TestData/OutputSemantics/anova-base.json`
- `BESHStat.Excel.Common.Tests/TestData/OutputSemantics/anova-welch-tukey.json`
- `BESHStat.Excel.Common.Tests/TestData/OutputSemantics/anova-all-posthoc.json`
- `BESHStat.Excel.Common.Tests/TestData/OutputSemantics/kw-base.json`
- `BESHStat.Excel.Common.Tests/TestData/OutputSemantics/kw-dunn.json`
- `BESHStat.Excel.Common.Tests/TestData/OutputSemantics/generic-mixed.json`
- `BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj` (fixture copy rule only)
- `docs/architecture/batch-02-02-output-cells-and-styles.md`
- `planning/BESHStatNG_CrossPlatform_MasterPlan.md` (active checkpoint only)

Final whitespace/diff and status checks pass with only these batch paths.
No commit, push, merge, branch switch/create, 02.03 or Phase 04A implementation
was performed. Stop here for independent review; 02.02 is not marked accepted.
Suggested commit after review:
`feat(common): add batch 02.02 canonical output cells and semantic styling`.
