# Batch 02.05 — Office.js shared output plan writer

Implemented on user-created `codex/02-05`; pending independent review and
acceptance. Starting HEAD `f2a6aa7` contains accepted 02.04 `c923927` in its
ancestry. The user reports 02.04 accepted, merged and live Windows Excel-DNA
smoke-tested. Initial working tree was clean; both existing stashes remain.
The active master-plan checkpoint is **222 total / 23 completed / 199
outstanding**, with **02.05 in progress / pending independent review**.
No 02.05 acceptance, 02.06 work or Phase 04A implementation is recorded.
The sequence **04.07 → 04A.01–04A.74 → dependent Phase 05+** is unchanged.

## Source audit and call graphs

The checked-out source, AGENTS.md, Revision 1.4 Phase 02 row/exit and Phase 04A
dependencies, companion Windows portability audit, and accepted 02.01–02.04
architecture notes were inspected. No ZIP was applied. Core/Common remain
netstandard2.0, WasmSmoke/modern tests net10.0, Windows/legacy tests Framework
4.8. Numerical engines, analysis-result v1, Windows 02.04 writer, UibyID,
Ribbon/UDF declarations, acquisition adapters and accepted fixtures are preserved.

Before:

```text
Home.razor.RunSharedAnalysisAndWrite
  → host capability preflight → AcquireRangeAsync → shared analysis Run (once)
  → sharedOutput.Tables → ResultTableWriteRequestFactory.CreateMany
  → JS.InvokeAsync<OfficeWriteResult>("beshOffice.writeResultTables", requests)
  → JS: cursor/one-row gaps + section-count roles + numeric p <= alpha
  → Excel.run → worksheet resolution/creation → values/formatting/select/sync
```

After:

```text
Home.razor.RunSharedAnalysisAndWrite
  → unchanged host capability preflight/acquisition/options/Run (once)
  → OutputDocument.FromResult(sharedOutput) [captured result.Document]
  → OutputSemanticPlanner.Create(document, settings, selected alpha)
  → OutputLayoutPlanner.Create(document, target, gap=1, semanticPlan=semantics)
  → OfficeOutputPlanAdapter.Prepare(document, semantics, layout)
  → JS.InvokeAsync<OfficeWriteResult>("beshOffice.writeOutputPlan", preparedPlan)
  → JS: full wire preflight + host/destination preflight
  → Excel.run → existing sheet resolution/naming → absolute rectangles
  → render supplied values/roles/formats/flags → select supplied envelope → sync
```

Core computes statistical values. Common owns the captured document, canonical
cells, annotation resolution, significance and absolute geometry. The small
Common **prototype adapter** makes a strongly typed, immutable, defensively
copied scalar projection. This avoids an untested C# object graph and lets the
existing Common MSTest/full gate verify the actual System.Text.Json payload
without adding a project or packages. It is an internal adapter protocol, **not
the frozen Common v1 envelope**; the Phase 02 freeze remains after 02.08.
Public CLR DTO types enable C#/VB consumption without host dependencies.

The semantic plan now retains a **Friend** source-document reference, supplied
by its existing planner. This internal, nonserialized provenance is needed to
reject stale roles/annotations even if a different document has identical IDs,
values and shape. Public semantic properties, preview behavior and fixtures are
unchanged. Layout provenance uses each placement's original Artifact reference.
No second role/p-value planner is introduced in the adapter.

The exact active Office caller is the method above, used for both ANOVA and KW.
It no longer constructs write requests or sends unplanned tables. Repository
source searches found no other active statistical JS interop caller.
`writeResultTable` and `writeResultTables` remain exported, deprecated pre-02.05
compatibility implementations with their original return/validation behavior.
Their old geometry/style helpers are isolated below the planned writer and
are never called by it. Existing downstream callers can still use those old
exports; they should migrate to a .NET-prepared plan. There is no claim that
deprecated calls consume shared plans. All active statistical UI writes do.
The existing 74 JS tests are unchanged.

## Internal adapter wire v1

Blazor's System.Text.Json interop uses camelCase properties, numeric destination
enums and explicit null union fields. Common tests serialize the real DTO using
that same naming policy and compare byte-normalized JSON fixtures; Node loads
those exact payloads. No arbitrary object graph, Office/COM type, preview or
alpha value crosses this boundary.

| Object | Exact JSON fields |
|---|---|
| Plan | `schemaVersion:1`, `target`, `artifactCount`, `sourceTableCount`, `tableCount`, `tables`, `usedBounds`, `pvalueNumberFormat` |
| Target | `destination:0 or 1`, `worksheetName:string`, `startRow:int`, `startColumn:int` |
| Rectangle | `startRow`, `startColumn`, `endRow`, `endColumn` (positive integers; inclusive) |
| Table | `artifactId`, `documentIndex`, `tableOrdinal`, `bounds`, `rowCount`, `columnCount`, `cells`, `autoFit:boolean` |
| Cell | `row`, `column`, `value`, `role`, `numberFormat`, `isPValue`, `isSignificant`, `strongEmphasis` |
| Value | `kind`, `numberValue:double or null`, `textValue:string or null`, `booleanValue:boolean or null` |

`kind` is exactly `blank`, `number`, `text` or `boolean`. Only its selected union
field is nonnull; all fields are null for blank. `role` is exactly `Title`,
`TopColumnHeader`, `LeftRowHeader`, `Body` or `Footer`. `numberFormat` is
`General` or the Core-produced Excel p-value format selected by the semantic
intent. The adapter reconstructs normalized settings from `semantics.Policy`
and calls the existing Core `BuildExcelNumberFormat`; JS does not build it.
Other format intents are explicitly unsupported by this prototype adapter.

`documentIndex` counts **all** document artifacts. `tableOrdinal` counts all
source tables, including zero-area tables. `tableCount` counts only positive-area
tables sent to JS; `sourceTableCount` retains the total source-table count.
Source plans retain warnings, faults, plot references and zero-area tables,
which receive no imaginary rectangles and consume no gaps. The wire contains
only placed tables, in source order. Mixed fixtures verify ordinal/index gaps.
`cells` is a flat, complete row-major sequence combining value and style, so
separate mutable value/style arrays cannot drift apart.

All rectangles are **one-based inclusive**. Cell `row`/`column` are **zero-based
within that table**. JS uses exactly:

```text
getRangeByIndexes(bounds.startRow - 1, bounds.startColumn - 1,
                  rowCount, columnCount)
```

It neither calculates separators nor advances a cursor. Four-table C6 is
exactly C6:E7, C9:D11, C13:F13, C15:C16; envelope C6:F16. Rows 8, 12 and 14
receive no values, clears or cell-format operations. Selection includes the
interior gaps. AutoFit changes worksheet row heights/column widths and can
therefore affect the dimensions of other cells sharing those rows/columns.

### Scalar and text safety

Finite binary64 values preserve their numeric representation, including zero,
signed zero, subnormals and maximum values at the transport boundary. Decimal
and integer cells are converted only if the exact rational value equals the
binary64 result; a BigInteger comparison checks this without Decimal-to-Double
round-trip heuristics. Thus 0.5 and exactly representable large powers of two
are accepted, while 0.1D, 9007199254740993, UInt64.MaxValue and unsupported large
decimals are explicitly rejected. There is **no exact-text fallback** and no
claim of numeric preservation through text. Ordinary binary64 probabilities
such as 0.05 are preserved as supplied doubles.

The exact decimal 0.050000000000000003D is above binary64 alpha 0.05 under
Common's rational significance comparison, even though converting it to Double
would compare equal. The adapter refuses its lossy transport before interop.
DisplayPreview never becomes a worksheet value. Preview culture (en-US/sk-SK)
cannot change the wire, resolved significance or worksheet numeric format.

All error cells, including known Excel errors, unknown native tokens and
canonical nonfinite errors, are rejected. All trusted formula intents are
rejected, including `excel-a1`: this adapter supports **no executable formula
dialect**. Unknown discriminants/union fields and nonfinite JSON numbers fail
closed. No native error or formula is guessed from an ordinary string.

Text is sent to Range.values with an explicit leading apostrophe so Excel treats
formula-looking (`=`, `+`, `-`), numeric-looking and error-looking strings as
literal text. Existing leading apostrophes are preserved by adding one escape
prefix. Empty text uses an apostrophe alone. Booleans stay booleans, never 0/1.
True blanks use null after an explicit `range.clear("Contents")` for each table:
Office's null Range.values entries otherwise leave prior contents untouched.
Each table then receives exactly one values assignment. The mock models this
null behavior and text prefix consumption; **real Excel round-trip confirmation
of empty text, literal prefixes and numeric formats remains a manual blocker**.

### Preflight and resource bounds

.NET rejects null/mismatched counts, identity/kind/order/shape mismatches, stale
source provenance, row-major cell drift, overlaps, unsupported destinations,
out-of-grid rectangles, unsupported scalars and excessive transport size before
calling JavaScript. JS checks the actual received objects, arrays (including
sparse arrays), strict runtime types/field sets, schema, target/envelope,
counts/order/unique IDs, positive inclusive rectangles and matching dimensions,
all local coordinates, value/style cardinality/union fields, resolved format
consistency, nonoverlap and Excel's 1,048,576 × 16,384 grid before Excel.run.
It checks wire integrity, not the statistical truth of a supplied flag.
The validated commands are copied before asynchronous capability probing so a
caller cannot change them while Office initialization is awaited.

The prototype adapter uses the existing 01.10 default bounds as explicit memory
guards: **64 artifacts, 10,000 total placed cells, 64 MiB estimated bytes**;
text is also limited to Excel's 32,767 characters/cell. The estimate includes
1,024 bytes/cell plus UTF-16 text/format lengths, not a heap-size guarantee.
JS bounds dimension products before allocating matrices/mapping cells; tests
reject excessive dimensions without building worksheet-sized matrices. These
are adapter guards, not a new reusable product output quota/cancellation API.

## Office rendering, lifecycle and failures

Only CurrentWorksheet (0) and NewWorksheet (1) are supported. NewWorkbook (2)
is explicitly refused and unadvertised. Capability IDs/probing and host checks
are preserved. Current output resolves the active worksheet just as before;
the target's name is a requested new-sheet base, not a current-sheet lookup.
New output reuses existing sanitization and case-insensitive unique suffixes.
No target fallback, workbook creation or selected-area acquisition change occurs.

Each table clears its contents/fill, writes its typed matrix and sets Calibri
Light. Horizontal equal-style runs apply the supplied number format, emphasis,
font size (footer 8, other roles 10), significant green (#32FF32) or black,
and top-header gray (#DDDDDD). Header/left-header/title/footer identities come
only from Common roles. Bold comes from StrongEmphasis, including significant
body cells; this adds the semantic plan's emphasis to the old green-only Office
highlight. Resetting body text/fills prevents stale formatting surviving an
overwrite. AutoFit is honored per table; the active legacy producer expectation
remains True and tests cover False/mixed flags. IsResultTable section-count
inference is absent from the planned path.

These semantic decisions follow Common. Accepted Windows 02.04 remains
unchanged, including its historical numeric-text significance behavior and
left-header loops. Shared fixture role/annotation coordinates are comparable;
**Windows/Office semantic identity for every legacy edge case and visual pixel
parity are not claimed**. In particular, Common treats numeric-looking p-value
text as text, while the historical Windows renderer may highlight it.

Office operations are queued without a sync per cell or style run. Current
worksheet output uses one final sync; new worksheet output additionally syncs
the names collection before creation. Tests assert bounded batching, not a
fragile universal round-trip count. Table ranges and the positive-area envelope
load their actual `address`; return values are taken from Excel after sync:
`worksheetName`, `address`, `tableAddresses` in document table order,
`pvalueNumberFormat`, and compatibility `pvalueColumns:[]`. Home only displays
the format/status/addresses; it no longer needs body-relative p-value columns.
Empty output returns empty addresses without Excel.run, worksheet creation or
selection. Home's statistical caller still refuses a no-table analysis.

Malformed input has **zero Excel.run/add/range/write/sync effects**. Office API
failures after preflight propagate and can leave partial values, clears,
formatting, selection or a newly created worksheet. A rejected final sync does
not imply earlier operations were rolled back. Recording tests cover later
assignment and sync failures. Output is **not atomic, transactional or undoable**.
There is no live occupied/merged/protected-cell check, overwrite confirmation,
permission policy, structured write outcome or rollback (02.08). Nonspatial
diagnostic/chart/footer/link spatial composition remains 02.07; no artifact is
silently converted into a synthetic row.

## Changed-file inventory

Eight modified files:

- `BESHStat.WasmSmoke/Pages/Home.razor` — active planned interop route.
- `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js` — preflight and planned renderer; acquisition/legacy behavior retained.
- `BESHStat.Excel.Common/src/Output/Styles.vb` — internal semantic source provenance.
- `BESHStat.Excel.Common/src/Output/PValuePresentation.vb` — supply provenance from existing planner.
- `BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj` — link new wire fixtures into existing tests.
- `BESHstatNG/tools/audit_transitional_officejs.py` — migrate the current-caller guard to the planned API while retaining the Common legacy-factory guard.
- `BESHstatNG/tools/tests/test_audit_transitional_officejs.py` — require each new caller boundary marker independently.
- `planning/BESHStatNG_CrossPlatform_MasterPlan.md` — exactly four active checkpoint lines.

Fourteen new files:

- `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`.
- `BESHStat.Excel.Common.Tests/src/OfficeOutputPlanAdapterTests.vb`.
- `BESHStat.WasmSmoke/tests/outputPlanWriter.test.cjs`.
- `BESHStat.WasmSmoke/tests/helpers/recordingWorkbook.cjs`.
- `BESHStat.WasmSmoke/tests/fixtures/four-tables-c6-write-trace.json`.
- `BESHStat.WasmSmoke/tests/fixtures/output-plans/anova-base.json`.
- `BESHStat.WasmSmoke/tests/fixtures/output-plans/anova-welch-tukey.json`.
- `BESHStat.WasmSmoke/tests/fixtures/output-plans/anova-all-posthoc.json`.
- `BESHStat.WasmSmoke/tests/fixtures/output-plans/kw-base.json`.
- `BESHStat.WasmSmoke/tests/fixtures/output-plans/kw-dunn.json`.
- `BESHStat.WasmSmoke/tests/fixtures/output-plans/four-tables-c6.json`.
- `BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json`.
- `BESHStat.WasmSmoke/tests/fixtures/output-plans/typed-roles.json`.
- This architecture note.

No deleted files. The eight new wire references are the actual serializer output
from the five unchanged Stage304 producers plus three deterministic structural
fixtures. Common tests compare them with fresh production serialization and
accepted semantic roles/flags; Node records every written value/kind and rendered
style. The small trace fixture is a separate test-only normalization of range
indexes, addresses, cell kinds, clears, selection, AutoFit and default styles.
It does not introduce another product output schema. The six accepted semantic
JSON and seven accepted layout references are untouched and remain regression
gates. The sixth `generic-mixed` semantic fixture includes intentionally lossy
integers/nonfinite values and is intentionally unsupported by the Office adapter;
focused tests confirm those scalar refusals instead of rewriting that fixture.

## Validation evidence

Commands, run from repository root without package installation or live Excel:

```powershell
node --test BESHStat.WasmSmoke/tests/outputPlanWriter.test.cjs BESHStat.WasmSmoke/tests/hostCapabilities.test.cjs

dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --no-restore --filter FullyQualifiedName~OfficeOutputPlanAdapterTests

dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --no-restore --filter 'FullyQualifiedName~OfficeOutputPlanAdapterTests|FullyQualifiedName~OutputLayoutPlannerTests|FullyQualifiedName~OutputDocumentTests|FullyQualifiedName~OutputCellTests|FullyQualifiedName~OutputStylesTests|FullyQualifiedName~PValuePresentationPlanTests|FullyQualifiedName~Stage304ContractSnapshotTests|FullyQualifiedName~SharedExcelLayerTests' --logger 'trx;LogFileName=OfficeOutputCompatibility.trx' --results-directory TestResults/Codex/Batch0205

& 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe' -B -m unittest discover -s BESHstatNG/tools/tests -p test_audit_transitional_officejs.py -v

powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Python -PythonExe 'C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' -Js -Wasm -Legacy -VisualStudio 2026
```

Focused JS + host capabilities: **93/93**, exit 0. Focused Common output/Stage304:
**167/167**, exit 0, including **26 new adapter cases**. The initial isolated
adapter run passed 25 cases before the final binary64-bit test was added; that
test passed in the 167-case focused run. Python audit focus: **7/7**, exit 0.
No accepted tests/fixtures were weakened or removed.

The complete required VS2026 gate exited **0**, with **2,228 passing tests**,
zero failed/skipped, an increase of **113** over the accepted 2,115 baseline:

| Suite/build | Accepted baseline | Observed 02.05 | Outcome |
|---|---:|---:|---|
| Core MSTest | 1,024 | 1,024 | Passed, exit 0 |
| Common MSTest | 374 | 400 | Passed, +26, exit 0 |
| Windows Framework 4.8 MSTest | 612 | 612 | Passed, including all 51 Windows SharedOutputPlanTests, exit 0 |
| Office.js Node | 74 | 160 | Passed, +86, including syntax check, exit 0 |
| Python tooling/help/discovery | 21 + 5 + 5 | 22 + 5 + 5 | All OK, +1, exit 0 |
| WASM .NET 10 Debug build | Passing | Passing | 0 compiler warnings / 0 errors, exit 0 |
| VS2026 Windows Debug build | Passing | Passing | 0 compiler warnings / 0 errors, exit 0 |
| Excel-DNA x86/x64 packing | Passing | Both produced | 1,910,272 / 1,836,032 bytes |

Stage304 inventory, method catalog, generated metadata `--check` and retired
helper audit passed. Modern restore reported existing projects up-to-date; no
package download/install was needed. The retained legacy discovery advisory is
`UTA001: TestClass attribute defined on non-public class BESHStatNG.Test.MatrixTestHelpers`.
It caused no failure or skip. Existing TRX names generate overwrite notices.
Redirected Python unittest stderr appears as PowerShell `NativeCommandError`
text in the full log, but all three summaries are OK and the script returned 0.
The Debug post-build guard exits before the printed PreRelease signing commands;
no signing routine or installer ran. No unrelated regression failure remains.

Final review commands (all exit 0):

```powershell
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --stat
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff -- planning/BESHStatNG_CrossPlatform_MasterPlan.md
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG stash list
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG branch --show-current
```

The owner SID check required command-local safe.directory; no global/local Git
configuration was changed. The final diff contains only the 22 inventoried files
(eight modified, fourteen new, zero deleted). Ordinary diff --stat lists tracked
modifications only; new files remain unstaged. Git's CRLF normalization notice
for the plan is not a compiler warning or diff-check error. Only four active
master-plan lines changed; historical reviews, all roadmap rows/dependencies
and the Phase 04A companion audit are unchanged.

Build-generated copies were archived under ignored `GeneratedByBuild`; exact
pre-build bytes were restored for HelpLinks.vb, HelpContext.vb and
VersionAttributes.vb. SHA-256 matches were verified for all three. Accepted
Stage304/OutputSemantics/OutputLayout fixtures, framework targets, production
Windows sources, UDF/Ribbon, installers and signing configuration have no diff.
Both original stashes remain: `On main: Generated help files after batch 01.08`
and `On codex/infrastructure-before-01-08: VS2026 generated help files before infrastructure merge`.
Branch remains codex/02-05. No commit, push, merge, branch switch, stash
operation, Release build, deployment or live host test was performed.

Initial development failures were corrected: Option Strict nullable comparisons,
a VB test helper/loop-variable name collision, and an off-by-one test evidence
directory (sandbox refused the unintended outside-repository path; it was fixed
to the intended ignored in-repository location). New wire fixture bootstrapping
initially refused missing expected files; generated actuals were inspected and
installed as new references only. The first full gate exited 1 at the Python
audit's obsolete Home factory marker, after Core/Common passed; the audit now
requires the new boundary and has an additional regression test. These are batch
integration/test defects, not pre-existing unrelated failures.

Ignored evidence: `TestResults/batch-02-05-{common-initial,common-focused,
js-focused,full-initial,full-validation}.log`, focused TRX and generated actual
wire JSON under `TestResults/Codex/Batch0205`, full Core/Common/Legacy TRX under
`TestResults/Codex`. Generated help/version originals are captured before builds;
only this run's generated bytes are restored from those exact copies.

## User-run Excel Online acceptance recipe — not run by Codex

Run **after independent source review**, in a disposable workbook. This is the
experimental WasmSmoke host, not a production Office add-in. No Excel Online,
Windows Office.js, browser-WASM, Mac or production-host certification is claimed.
No live host, installer, deployment, signing or publishing was run by Codex.

1. Start the existing HTTPS profile:
   `dotnet run --project BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj --launch-profile https`.
   Sideload `BESHStat.WasmSmoke/office-manifest.xml` (https://localhost:7131/)
   into a disposable Excel Online workbook. In the task-pane console, inspect
   `await beshOffice.getHostCapabilities()`: current/new worksheet capabilities
   must be present; `output.new-workbook` must be absent.
2. With synthetic A={1,2,3}, B={2,3,4}, C={5,6,7} and headers, run ANOVA to a
   new worksheet at row 6/column 3. Toggle Welch and inspect its added columns,
   numeric values, header styles and p-value formats/highlights. **The current
   UI exposes Welch only, no ANOVA post-hoc controls.** Do not report a live
   post-hoc/multiple-table ANOVA UI run: the five-table ANOVA post-hoc path is
   automated via the accepted fixtures. Welch is columns in the primary table,
   not another table. The synthetic four-table C6 fixture is not a UI analysis.
3. Run KW with the existing Dunn checkbox to an empty current worksheet at a
   known non-A1 cell; verify multiple table starts, one blank separator, table
   titles/headers/footers, numeric p-values and untouched gaps. Repeat in a new
   worksheet; reuse a result sheet base name and inspect the unique suffix.
4. Compare shapes, values and types with the accepted pre-02.05 output. Confirm
   no duplicate rows or changed statistics. The UI must show returned worksheet
   and table addresses matching Excel's visible ranges, in source order, and
   the shared p-value format. For a diagnostic prepared typed payload, check
   `=1+1`, `#N/A`, numeric-looking strings and empty text stay literal text,
   true blanks clear existing contents, and no preview string replaces a number.
   Formula-looking text must not execute. Semantic significance adds bold plus
   green; host fonts/column widths need not match Windows pixels.
5. NewWorkbook is visible but disabled in this UI. If a diagnostic call requests
   it, require an explicit unsupported error before any write; no sheet fallback
   is allowed. Invalid-plan console errors start with `Invalid output plan:`;
   valid payloads on unsupported hosts report result writing unavailable.
6. In the disposable workbook only, request a start near the final worksheet
   row/column with a table that would overflow. Require failure before values or
   worksheet creation. Do not use a meaningful workbook: occupied/merged/protected
   cell safety and overwrite/undo support have not been implemented.

Keep screenshots/console evidence in user acceptance records, excluding private
workbook data from source control. Live literal-text/empty-text handling,
host-specific number-format rendering and Office error side effects remain
acceptance unknowns until the user supplies host evidence.

Suggested commit after independent review:
`feat(office): integrate batch 02.05 shared output plan writer`.
Stop here; do not commit, push, merge, change branches or start the next batch.
