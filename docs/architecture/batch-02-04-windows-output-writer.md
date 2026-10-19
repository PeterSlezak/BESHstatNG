# Batch 02.04 — Windows shared output layout writer

Implemented on `codex/02-04`; pending independent review and user acceptance.
Accepted source checkpoint: verified HEAD `1df75a8` (02.03), following `590ccea`
(02.02). The initial working tree was clean. Both existing stashes were recorded
and retained. The master plan remains **222 total / 22 accepted / 200 outstanding**;
02.04 is current, implementation underway / pending review. All 74 future 04A
batches and the sequence **04.07 → 04A.01–04A.74 → dependent Phase 05+** remain intact.

## Source audit and old/new responsibilities

The checked-out repository, `AGENTS.md`, Revision 1.4 master plan (including its
Phase 02 exit and 02.04 row), companion Windows portability audit, and accepted
02.01/.02/.03 notes govern this batch. No source ZIP is overlaid on the checkout.

Before: `RunOneWayANOVAThroughSharedLayer` and `RunKruskallWalis` dispatch to
`SharedAnalysisApplicationService.Run`; `analysisResult.Tables` feeds
`ResultTableWriteRequestFactory.CreateMany`. Each UI route calculates
`sharedRows/sharedCols`, adds supplementary heights and separators, calls
`AreaCheck` and the existing overwrite dialog, then loops through requests,
calling `shiftRowPointer()` between `WriteSharedRequest` calls. Each request
write separately advances the cursor by matrix height. `WriteOutputBlockCore`
obtains a COM range, assigns the matrix through `rng.Value`, applies `format`
and p-value rules, and optionally auto-fits.

After: both routes retain acquisition, typed options/dispatch, host capability
check, request factory, and `AutoFit=False`. Their shared `PrepareSharedOutput`
helper creates `OutputDocument.FromResult(analysisResult)` and exactly one
`OutputLayoutPlanner.Create(..., blankRowsBetweenTables:=1)` for the resolved
Windows target. `ExcelDnaSharedOutputPlanAdapter.Prepare` validates and snapshots
the entire command list. `PreflightBounds` expands the plan envelope to cover
supplementary legacy outputs. `AreaCheck` and the unchanged
`MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?")`
still precede data writes. A No returns without shared or supplementary writes
or chart creation. Yes proceeds through `WritePlannedSharedRequests` → prepared
`Execute` → the existing `WriteOutputBlockCore`/`format` implementation. The COM
range uses all four coordinates from each command's planned bounds.

`GetResultWriter` remains the destination authority: it creates/activates a
workbook or worksheet for those radio options, or resolves the requested current
worksheet and one-based range start from `RefEditOutput`. This happens before
planning/confirmation and can create a sheet/workbook even if output is later
refused. The code does not introduce new fallback, activation or naming policy.

Supplementary homogeneity, descriptive and box-plot summary tables retain
`ProcessListofResultTables.writeToSheet(writer, True)` and exactly one explicit
separator after the shared block. ANOVA retains homogeneity → descriptive → box
summary order; KW retains descriptive → homogeneity → box summary order. Optional
box plots and Fisher LSD/Bonferroni/Tukey–Kramer/Games–Howell charts remain host
responsibilities. Existing chart code and its stacking/anchor offsets, based on
the final writer row and `ColID + totcols + 2`, are unchanged. Opaque plot
artifacts do not become chart-placement rectangles.

## Adapter contracts and identity

All new adapter code is Windows-only, `Option Strict On`, without COM calls.
The existing `InternalsVisibleTo("BESHStatNG.Test")` permits focused MSTests;
there is no new public request schema, package, mocking library or host service
hierarchy. The writer adds internal planned execution and a narrow overridable
range-writing seam for tests, leaving its existing public entry points intact.

| Object | Responsibility |
|---|---|
| `OutputDocument` | Immutable complete Core matrix/metadata snapshots; ordered nonspatial diagnostics/plot references retained |
| `OutputLayoutPlan` | Single authority for absolute table geometry, original document indices, gaps and occupied envelope |
| `ResultTableWriteRequest` | Existing normalized primitive payload, metadata, target, number-format/alpha overrides and auto-fit bridge |
| `ExcelDnaSharedWriteCommand` | Immutable copied payload/metadata, exact planned bounds, artifact ID, source ordinal, document index and request overrides |
| `ExcelDnaPreparedSharedOutput` | Read-only complete command set and validated layout; bounded execution/cursor boundary |
| `ExcelDnaResultWriter` | COM range assignment and unchanged historical visual rendering |

Production request ordinal N binds to `table-N`, using invariant decimal IDs.
Custom documents can supply explicit ordered table IDs. Table ordinals include
zero-area tables; document indices also include diagnostics/plots. The adapter
filters table artifacts rather than equating request count to placement count.
For `[table, warning, plot, table, fault]`, only the two tables write, with no
extra rows for nonspatial artifacts. Zero-row or zero-column matrices remain
matched entries but create no commands or gaps. All-empty/nonspatial-only
prepared execution leaves the pointer untouched; the two UI workflows require
a nonempty shared footprint for `AreaCheck`.

Before any executor runs, the adapter checks null inputs/requests/targets,
count/order/IDs, full rectangular dimensions, every normalized value and CLR
type, title/header/footer counts, result-table flag, complete p-value annotations,
target destination/start/sheet and actual supplied worksheet name, Windows grid
limits and the one-row gap policy. A stale same-shaped but different payload
fails too. Request-specific format/alpha/auto-fit overrides are copied without
semantic reinterpretation. Closed Common artifact construction already rejects
nulls, duplicate IDs and unsupported scalar payloads; the adapter defensively
rejects unknown kinds or placed nonspatial entries.

Execution rechecks worksheet name and current cursor before its first write.
The caller's held `writer.ws` determines the physical sheet, including an inactive
requested sheet. Logical names do not prove workbook identity or live occupancy;
this is not a revision token, protected/merged-cell check, or consent proof.
No claim is made that a logical geometry plan validates a live workbook.

Worksheet bounds are **one-based inclusive**. Semantic local cells are
**zero-based**, translated as `bounds.StartRow + local.Row` and
`bounds.StartColumn + local.Column`. Core/request p-value addresses stay
**one-based body-relative**; absolute row is start + title rows + top-header
rows + body row − 1, and column is start + left-header columns + body column − 1.
The adapter preserves raw annotations, including historical duplicates/invalid
indices, for the existing formatter's historical rules.

## Exact geometry and cursor compatibility

| Table ordinal | Matrix | Planned range |
|---:|---:|---|
| 0 | 2 × 3 | C6:E7 |
| 1 | 3 × 2 | C9:D11 |
| 2 | 1 × 4 | C13:F13 |
| 3 | 2 × 1 | C15:C16 |

`UsedBounds=C6:F16`; exactly four writes; separator rows 8, 12 and 14 receive
no writes. Next row is **17**, column remains **3**. No trailing gap belongs
to the plan. A gap-only reservation does not collide with the occupied tables.
The fixture is an automated adapter preview, not an exposed product workflow.

The new planned route never inserts its own primary-table cursor gaps. Before
each command it marks the attempted start row; after successful values/formatting
it sets the pointer to inclusive end + 1. Thus complete success matches the
old UI gap + single-request-height behavior without two geometry authorities.
`WriteSharedRequest` remains callable independently: it ignores the request's
requested placement, uses current `RowID/ColID`, retains request auto-fit and
overrides, and advances by matrix height. `ResultTableWriterBase.write` and
`WriteOutputBlock` callers remain compatible; no Core writer change was needed.

The adapter deliberately refuses a positive-area plan ending on physical Excel
row 1,048,576 before any writes, since a valid next legacy cursor/chart anchor
would not exist. Combined supplementary output follows the same boundary rule.
This is an explicit safe boundary refusal, not cursor clamping or wrapping.
All supplementary endpoint/width calculations use Int64 before narrowing, with
grid checks after every addition. One separator precedes each supplementary
matrix, covering both the shared-to-legacy separator and inter-legacy gaps.
For the C6 fixture plus 2×7 and 3×2 supplements, the conservative overwrite
envelope is **C6:I23**. It includes gap rows and padding, preserving the previous
bounding-box protection even though gaps are not primary write commands.

On a later COM failure the original exception propagates; prior writes remain.
The pointer stays at the attempted table start, not at the end of unexecuted
output. Even the failing table may already have values if formatting failed.
The failure test records the first successful table and row 9 for a failure on
the second C6 fixture command. This batch provides no rollback/transaction or
structured partial-write outcome; 02.08 owns that work.

## Cells, semantics and visual parity

Production uses the canonical **document and geometry**, plus the legacy
request/formatter bridge. It does **not** create an `OutputSemanticPlan` in the
UI or use semantic previews as renderer directives. A semantic plan is optional
under 02.03; focused tests create one as a coordinate/role comparison snapshot.
Generating unused per-cell semantic graphs would add work without replacing
the historical Windows policy in this transitional batch.

Request-derived numeric payloads stay numeric with full supplied precision;
strings that look numeric, booleans, Nothing/empty text, and historical normalized
`#N/A`/`#Pinf`/`#Ninf` strings retain their values and CLR types. Formula/error
`OutputCell` factories are not a new write protocol. The scalar whitelist,
analysis-result v1, existing assembly and matrix dimensions remain unchanged.

The formatter still clears the same borders, sets Calibri Light 10, header fill
14540253, footer size 8, and preserves title/header/left-header loops (including
the historical `hLeft - foots` rule). P-value number formats and the existing
`RGB(50,255,50)` significance color are unchanged. Invalid explicit alpha
overrides fall back to `AppGlobals.DefaultAlpha`; the factory's separate invalid
input-alpha fallback remains 0.05. Blank format overrides use the configured
Core number format. Pure extracted functions are called by the real renderer
and tested without fake COM ranges. GUI requests still disable auto-fit;
standalone request auto-fit remains available.

The historical Windows predicate uses `Convert.ToDouble(raw, CurrentCulture)`,
excluding Boolean/Nothing and rejecting nonfinite/negative/out-of-alpha values.
Thus string `"0.01"` under en-US (or `"0,01"` under sk-SK) can highlight. Shared
semantic planning intentionally recognizes only numeric p-values. The regression
table retains one numeric 0.01 and one string `"0.01"`: both can highlight in
historical Windows, while only the numeric semantic cell is significant. No
coercion of the request payload is introduced. Common role/style/annotation
sanitization and Windows historical rendering therefore still contain deliberate
policy differences. **Windows/Office.js semantic rendering parity is not claimed.**

## Files and tests

Modified/new delivery paths:

* `BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb` — planned execution and reused range writer; pure legacy p-value seams.
* `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb` — preparation, immutable commands, cursor execution and conservative UI preflight.
* `BESHstatNG/src/UI/UibyID.vb` — exactly the two shared routes and their private placement/preflight helper.
* `BESHstatNG/BESHStatNG.vbproj` — explicit adapter include only.
* `BESHStatNG.Test/src/DataManagement/SharedOutputPlanTests.vb` — focused Framework 4.8 MSTests.
* `BESHStatNG.Test/BESHStatNG.Tests.vbproj` — explicit test include and linked copies of the five existing Stage304 fixtures.
* `planning/BESHStatNG_CrossPlatform_MasterPlan.md` — four active checkpoint statements only.
* This architecture note.

The 51 new Windows cases cover exact C6 bounds/cursor/reservations, mixed and
zero-area entries, null/count/order/identity/shape/type/metadata refusal before
writes, stale targets, missing worksheets, all destination intents, overflow
and final-row refusal, immutable snapshots, overwrite Yes/No, combined
supplementary bounds, later-command failure, standalone/legacy writer pointers,
role/p-value mapping, numeric text and configured/fallback p-value policies.
No Excel process is required. Five Stage304 fixture data rows run the existing
analysis producers and compare every command cell/type and metadata field to
the legacy requests, checking exact starts/one-row gaps under en-US and sk-SK.
Existing golden assertions independently protect their frozen numeric results.
No new golden files are needed or authored.

## Validation evidence

Commands run from the repository root (logs/TRX are ignored local evidence in
`TestResults/Codex/Batch0204`; full-suite TRX remain in `TestResults/Codex`):

```powershell
& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe' BESHStatNG.Test/BESHStatNG.Tests.vbproj /t:Build /p:Configuration=Debug /p:PythonExe=C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe /m /v:minimal

& 'C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe' BESHStatNG.Test/bin/Debug/BESHStatNG.Test.dll /TestCaseFilter:FullyQualifiedName~SharedOutputPlanTests '/Logger:trx;LogFileName=WindowsOutput.trx' /ResultsDirectory:TestResults/Codex/Batch0204

dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --no-restore --filter 'FullyQualifiedName~OutputLayoutPlannerTests|FullyQualifiedName~OutputDocumentTests|FullyQualifiedName~OutputCellTests|FullyQualifiedName~OutputStylesTests|FullyQualifiedName~PValuePresentationPlanTests|FullyQualifiedName~Stage304ContractSnapshotTests|FullyQualifiedName~SharedExcelLayerTests' --logger 'trx;LogFileName=OutputCompatibility.trx' --results-directory TestResults/Codex/Batch0204

powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./scripts/verify-beshstatng.ps1 -Python -PythonExe 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe' -Js -Wasm -Legacy -VisualStudio 2026
```

Focused final Windows build/test: exit **0**, **51/51** passed. Common focused
output/Stage304/shared-layer selection: exit **0**, **141/141** passed. An earlier
44-case iteration and initial full gate passed 2,108 tests. While adding the
last boundary cases, one build exited **1** for BC30198/BC30035 on three
constructor-expression `Sub` lambdas; changed them to `Function` lambdas. No
production or existing test expectations were changed to fix this test syntax.

Final full gate: exit **0**, **2,115 tests passed**, zero failures/skips:

| Suite/build | Accepted baseline | Final observed | Result |
|---|---:|---:|---|
| Core MSTest | 1,024 | 1,024 | Passed, exit 0 |
| Common MSTest | 374 | 374 | Passed, exit 0 |
| Windows Framework 4.8 MSTest | 561 | 612 | Passed, +51, exit 0 |
| Office.js Node | 74 | 74 | Passed; syntax check also passed, exit 0 |
| Python tooling/help/discovery | 21 + 5 + 5 | 31 | All OK, exit 0 each |
| WASM .NET 10 Debug | Passing | Passing | 0 warnings / 0 errors, exit 0 |
| VS2026 Windows Debug | Passing | Passing | 0 warnings / 0 errors, exit 0 |
| Excel-DNA x86/x64 packing | Passing | Both produced | 1,905,664 / 1,831,424 bytes |

Stage304 inventory, methods catalog, generated metadata `--check` and retired
helper audit all passed. Restore reported existing projects up-to-date; no
package installation was needed. Existing `UTA001 MatrixTestHelpers` discovery
advisory and TRX overwrite notices persist; neither is a compiler warning or
test failure. Redirected Python unittest output can appear as PowerShell
`NativeCommandError` text, but every Python summary was OK and all child exit
codes/full script passed. No unrelated regression failures were observed.

Final scope checks (exit **0**):

```powershell
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --stat
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff -- planning/BESHStatNG_CrossPlatform_MasterPlan.md
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG stash list
```

Git's owner SID check required the command-local safe-directory option above;
no local/global Git config was changed. `diff --check` is clean. Status contains
only the eight paths listed above (five modified, three new/untracked). Ordinary
`diff --stat` covers the five tracked files; the three new files are not staged.
The plan diff changes exactly four active checkpoint lines, with no historic
paragraphs or roadmap rows altered. CRLF-to-LF advisories are Git normalization
notices, not errors. Both original stashes remain: generated help after 01.08
on main, and VS2026 generated help before infrastructure merge.

Generated HelpContext/HelpLinks/VersionAttributes bytes were saved before any
build. Builds may rewrite their timestamp/date lines; only build-generated
changes were restored from those exact snapshots after validation and all three
SHA256 hashes match the initial bytes. Accepted
Stage304 (five), OutputSemantics (six) and OutputLayout (seven) references are
preserved. Framework targets, Core numerical/report code, Office.js/WASM host
sources, Ribbon/UDF exports, packaging/signing scripts and the Phase 04A audit
are unchanged. Debug packaging retains both x86 and x64 packed XLLs; the existing
Debug post-build guard exits before PreRelease signing. No signing keys,
installers, deployment, remote branches or environment/package changes are used.

## User-run Excel-DNA acceptance recipe (not run by Codex)

Use a disposable workbook and the accepted Windows baseline for comparison.
Record table start addresses, values and Excel numeric types/formats, separators,
styles and chart placement; automated parity does not certify COM rendering.

1. Run basic one-way ANOVA to an empty current worksheet at **C6**. Compare the
   complete table, titles/top and left headers/footers, numeric p-values, number
   formats and highlighting against the accepted baseline.
2. Enable Welch and at least two post-hoc choices. Confirm multiple shared tables
   have exactly one blank separator, no duplicated gap, and unchanged Fisher
   LSD/Bonferroni/Tukey–Kramer/Games–Howell chart positions and stacking.
3. Add descriptive, homogeneity and box-plot options. Check supplementary table
   order and a single separator after the final shared table; confirm the box
   plot and comparison charts still render in their baseline positions.
4. Run KW/Dunn with and without optional supplementary output and compare
   metadata/formatting. The checked-out `RunKruskallWalis` sets
   `IncludeDunnComparisons=True` unconditionally; it has no no-Dunn UI toggle to
   exercise. The automated `kw-base` fixture verifies no-Dunn shared output.
   Do not claim a no-Dunn live UI smoke unless a separately supported entry
   point is available; this batch does not add an option.
5. Repeat current/new worksheet/new workbook choices, including a non-A1 output
   reference. New destinations begin at their existing A1 default. Note that a
   new sheet/workbook may exist even when later placement/overwrite is refused.
6. Place a sentinel in the combined would-be output area (also test a supplementary
   table location). Run, choose **No**, and verify sentinel/data and charts are
   unchanged. Then choose **Yes** and confirm expected ranges update. Verify
   `AreaCheck` still protects the conservative complete envelope, including gaps.
7. Optional C6 four-table diagnostic preview: run the focused C6 MSTest or inspect
   its prepared command ranges. No product UI promises exactly those four
   synthetic tables, and no four-table live-host smoke is claimed.

No Excel, browser/WASM, Mac or other live host smoke was run. The full gate builds
WASM and runs existing JavaScript tests; it does not drive a live Office host.
02.05 owns Office.js integration, 02.06 target/capability policy, 02.07 composite
diagnostic/chart placement, and 02.08 structured outcomes/overwrite/undo. No work
on those batches or any Phase 04A batch is authorized by this implementation.

Suggested commit after independent review:
`feat(windows): integrate batch 02.04 shared output layout writer`.
