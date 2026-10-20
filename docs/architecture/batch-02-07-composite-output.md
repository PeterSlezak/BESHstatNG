# Batch 02.07 — Composite output planning

Implementation on `codex/02-07`, based on clean commit `288fcf2` (`Update HistogramBinningCore.vb`). Batch 02.06 is user-accepted and merged following Windows and Excel Online acceptance. Batch 02.07 remains pending independent review and live-host acceptance. The roadmap checkpoint is **222 total / 25 completed / 197 outstanding**. No Phase 04A work is implemented here.

## Open safety gap and 02.08 handoff

**In Excel Online, 02.06 currently overwrites existing cells in the planned table rectangles without prompting. The user tested this and confirmed it; it was intentionally deferred to 02.08. That later batch MUST check all planned composite/table rectangles (not just the bounding envelope or first table), obtain consent when occupied, perform no destructive write when the user selects No, and truthfully report partial writes. The 02.07 opt-in composite path must not make this more dangerous.**

The new Office composite entry point rejects current-worksheet and new-workbook requests before `Excel.run`. It only acquires a new, uniquely named empty worksheet through the existing 02.06 acquisition helper. It never clears cells. Workbook structure/protection inspection is reused when the host supports it. New empty sheets have no pre-existing merges or protected regions; all composite rectangles, rather than their envelope, are passed into the acquisition boundary. Existing table-only behavior is unchanged, including its known current-sheet consent gap.

There is no transaction, rollback, undo, generic structured write outcome, overwrite dialog or late-failure recovery. A failure can leave a new worksheet and earlier/partial writes. Errors propagate, and the demo explicitly tells the user that partial output may remain. Do not interpret a successful mock as live Excel Online acceptance.

## Before and after

The existing route remains `OutputDocument → OutputSemanticPlanner → OutputLayoutPlanner.Create → OfficeOutputPlanAdapter / Windows shared table adapter`. Only positive-area tables occupy cells in that default layout; diagnostics and logical plot references remain nonspatial.

The opt-in route is `OutputDocument + ordered CompositeOutputItem intents + TargetPlanner decision → CompositeOutputPlanner → CompositeOutputPlan → host projection/insertion seam`. Each original artifact must occur exactly once in original document order. Notes and links may be interleaved between them. IDs are ordinal and unique across the complete composite sequence. Each placement retains its composite index, optional original document index, source artifact ID or annotation ID/anchor, dimensions, occupied bounds and explicit state. Missing, duplicated, reordered or stale references fail before projection.

`OutputArtifactKind` remains its existing three-member closed family. `AnalysisResultDocument` v1, table snapshots, semantic contracts, Office table wire v1 and the default Windows ANOVA/KW/form/chart workflows are unchanged. The synthetic example is never appended to real analysis output. There are no framework, dependency, installer, ribbon, graphics-engine, UDF or statistical changes.

`OutputSpatialGeometry.NextBounds` shares the overflow-safe rectangle arithmetic between default table and composite planning. Default table snapshots and artifact-specific reservation errors remain regression-tested. Composite layout is calculated in a single ordered pass, so visible diagnostics and placeholders advance the cursor before subsequent tables are placed. It never overlays placeholders onto a previously computed table layout.

## Portable intent and geometry policies

* Tables retain whole matrix snapshots, title/header/footer counts, scalar runtime types and body-relative p-value metadata. Table styling/scalars come from the existing semantic plan and Office adapter; hosts do not recalculate alpha or significance. The example includes integer, double, Boolean, formula-looking literal text, numeric p-value and an embedded footer.
* Diagnostics need explicit positive dimensions to be visible; otherwise explicit `0 × 0` intent keeps their ordered nonspatial entry. Text includes `WARNING [code]` or `FAULT [code]`, message and field path. Severity is never communicated by color alone. Projection retains severity, code, message and field path separately.
* Plot references remain opaque logical keys. A visible `PlotPlaceholder` reserves actual cells and displays `CHART PLACEHOLDER (not rendered)` with purpose, caption, description and logical key. It contains no statistical chart data or native chart definition. Unrequested plots remain ordered nonspatial entries and are returned as such, not silently omitted or claimed rendered.
* Additional notes have their own ID, complete text, explicit positive dimensions and optional related artifact ID, validated against the source document. Embedded table footer rows remain inside their matrices and are never duplicated as notes.
* Link blocks have a separate ID, literal display text, optional anchor, explicit dimensions and validated HTTPS target. Both hosts deliberately degrade to `display text [link not active: target]`, state `linked-text-only`. No native hyperlink or chart capability is claimed. Plot reference keys are never parsed as links.

All Common rectangles use one-based inclusive coordinates. Hosts convert to zero-based indexes once at their Office API boundary. Intermediate additions/products use `Long`; grid checks precede narrowing or worksheet-sized allocations. Composite plans respect configured limits, target grid facts and Excel's maximum 1,048,576 rows × 16,384 columns. Zero/negative visible dimensions, mismatched nonspatial dimensions, out-of-grid blocks and intersections with caller reservations fail before writes. Zero-area tables retain their source identity and table ordinal without consuming rows or a separator. The default blank separator is one row between positive-area blocks; configurable nonnegative gaps apply to every spatial kind.

`OccupiedRectangles` lists only cells owned by visible blocks, including placeholders and annotations. `UsedBounds` is the truthful enclosing rectangle and includes unowned gaps. Caller reservations in separator gaps are valid. These contracts describe geometry, not physical existing workbook occupancy or overwrite permission.

Items and placements are sealed immutable snapshots with read-only copied lists, immutable strings/rectangles and defensive table-model copies. Target decisions capture scalar identity; obtaining and mutating a new target DTO cannot change the plan. Semantic source provenance is validated by the existing adapter.

## Text, wrapping and link security

Block text is stored in full in the top-left cell of its explicitly sized rectangle. The Office adapter merges only that owned block on the new empty sheet and enables wrapping. It does not AutoFit, resize rows/columns, estimate fonts or infer a height from text length. Fixed dimensions can visually clip long text; the demo states this explicitly. The complete cell value remains accessible in Excel's formula bar. Increase explicit dimensions when needed. No text is truncated. The Windows literal-block sink must obey the same containment and literal-text contract; no live Windows text renderer is supplied in this batch.

Common and JS reject strings exceeding 32,767 UTF-16 units and unsupported control characters; tab/CR/LF are allowed. Display plus inactive-link suffix must also fit that limit. Literal text is passed with a leading Excel apostrophe, including `=`, `+`, `-`, numeric-looking strings and native-error-looking text. Annotation anchors use text number format. No formula transport, `HYPERLINK` formula, URI navigation, network fetch, file access or native error coercion is introduced.

The narrow URL policy accepts lowercase `https://`, an ASCII DNS hostname with at least two valid labels (labels ≤63, hostname ≤253), no credentials, no explicit port, no whitespace/backslash/control characters and total URL length ≤2,048. Paths, queries and fragments may follow the hostname. `javascript:`, `data:`, `file:`, HTTP, formula-looking targets and protocol-relative targets are rejected. Targets are validated again in JS before any host operation. The policy is intentionally conservative and does not establish that a remote resource exists or is trusted.

The Office composite transport is separate wire v1: `schemaVersion`, `documentId`, target snapshot, all ordered items, caller reservations, used bounds and an unchanged table-wire-v1 `tablePlan`. A table-only envelope inside that nested plan covers only tables; if an annotation leads, its table target begins at the first table. The outer target/envelope still covers the complete composite. Exact object keys, enum/union fields, source/table order, IDs/anchors, dimensions, metadata/text alignment, rectangle shapes, bounds, overlap, destination and table semantics are checked before `Excel.run`. Copied primitives survive asynchronous capability checks. Prototype limits reuse the existing 64-item/range, 10,000-total-cell and 64-MiB estimated transport budgets; reservations are also limited to 64. Full reserved placeholder area counts toward cells, not merely its text anchor.

## Exact C10 evidence

The fixture comes from `CompositeOutputExample.Create`, actual .NET JSON serialization and independent JS workbook mocks. It includes all five kinds and a second table that detects failure to advance past intervening content.

| Item | Dimensions | Excel rectangle | Common rectangle |
|---|---|---|---|
| Main table | 4 × 6 | C10:H13 | R10C3:R13C8 |
| Warning | 1 × 6 | C15:H15 | R15C3:R15C8 |
| Chart placeholder | 8 × 6 | C17:H24 | R17C3:R24C8 |
| Additional note | 1 × 6 | C26:H26 | R26C3:R26C8 |
| Inactive safe link | 1 × 6 | C28:H28 | R28C3:R28C8 |
| Second table | 2 × 3 | C30:E31 | R30C3:R31C5 |

The first five blocks have envelope **C10:H28**; all six have **C10:H31**. Pairwise inclusive rectangle intersections are false. Separator rows 14, 16, 25, 27 and 29 receive no writes/merges/styles. Office table range calls are `(9, 2, 4, 6)` and `(29, 2, 2, 3)`; the four text blocks have their complete planned ranges merged and literal values written at `(14,2)`, `(16,2)`, `(25,2)` and `(27,2)`. Returned addresses come from the host range objects. The Windows recording seam emits the same six kinds/rectangles in the same order and calls the existing `WriteOutputBlockCore` for both tables with the original model and Common p-value format/alpha.

The secondary fixtures include a 2 × 3 table C10:E11, a zero-area table, an unplaced plot, a two-line fault at C13:G14 and reservation C12:J12 inside the separator. An optional link is C16:E16. Nonspatial entries retain order and identity without insertion calls. JS also covers entirely nonspatial output and the final Excel cell.

## File and test inventory

Portable implementation:

* `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`: immutable intents, placements, plan, URL/text policies and shared rectangle arithmetic.
* `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`: explicitly synthetic opt-in reference document.
* `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`: separate transport and validation; reuses the existing table projection.
* `BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb`: factors rectangle arithmetic while retaining default behavior.

Host boundaries:

* `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js`: `writeCompositeOutputPlan`, strict whole-payload validation, accepted worksheet acquisition and bounded writes; old entry point unchanged.
* `BESHStat.WasmSmoke/Pages/Home.razor`: collapsible developer demo, address/degradation JSON and partial-write message; no changes to the ANOVA/KW execution route.
* `BESHstatNG/src/DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb`: preparation plus ordered recording/insertion seam. Tables reuse the legacy formatter boundary; diagnostics/placeholders/notes/inactive links are explicit commands for a caller-supplied literal-block sink. It performs no COM invocation or destination creation and is not wired to production forms. Both sinks are checked before the first call. A real Windows caller must acquire/verify the destination and implement the literal sink before live use.

Tests and fixtures:

* `BESHStat.Excel.Common.Tests/src/CompositeOutputTests.vb`: geometry, order, complete references, dimensions/gaps/reservations, inclusive boundaries, overflow/grid bounds, text/URL validation, snapshots, metadata/types, table semantic equivalence, provenance, new-sheet-only projection and actual wire fixtures.
* `BESHStat.WasmSmoke/tests/compositeOutputWriter.test.cjs`: actual .NET projection consumed by independent JS mocks, exact host coordinates and later-table shift, literal values/formats/severity, safe degradation, skipped entries, invalid schemas/unions/targets/URLs, no write side effects, new-sheet isolation and propagated partial failures.
* `BESHStat.WasmSmoke/tests/helpers/compositeWorkbook.cjs`: independent per-worksheet recorder with existing-sheet sentinels and forbidden formula/clear operations; old table mock is unchanged.
* `BESHStat.WasmSmoke/tests/fixtures/composite-output/{composite-c10,composite-secondary,composite-secondary-link}.json`: deterministic actual .NET transport fixtures. Tests write comparison output under ignored `TestResults/Codex/Batch0207`, never update the checked-in fixtures automatically.
* `BESHStatNG.Test/src/DataManagement/CompositeOutputPlanTests.vb`: seven tests for recording order/rectangles, legacy table formatting/metadata, accessible warning text, placeholder/link fallback, immutable commands, rejected provenance/destination, nonspatial command retention and propagated sink failure.
* Common test project adds fixture-copy metadata; Windows and legacy test projects explicitly include the new adapter/test files. The master plan only updates the active checkpoint, 02.06 completion label and precise 02.08 handoff; the 74 Phase 04A rows and separate audit ledger remain intact.

## Validation and limits

The required complete gate is:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Python -PythonExe "C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe" -Js -Wasm -Legacy -VisualStudio 2026
```

Focused commands include:

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --no-restore --filter 'FullyQualifiedName~CompositeOutputTests|FullyQualifiedName~OutputDocumentTests|FullyQualifiedName~OutputLayoutPlannerTests|FullyQualifiedName~OfficeOutputPlanAdapterTests|FullyQualifiedName~OutputTargetPlannerTests'
node --test BESHStat.WasmSmoke/tests/compositeOutputWriter.test.cjs
# VS 2026 MSBuild /t:Build /p:Configuration=Debug, then:
vstest.console.exe .\BESHStatNG.Test\bin\Debug\BESHStatNG.Test.dll '/TestCaseFilter:FullyQualifiedName~CompositeOutputPlanTests|FullyQualifiedName~SharedOutputPlanTests'
```

The complete gate passed on the final implementation, with no failures or skips:

| Suite | Accepted 02.06 baseline | 02.07 result | Added |
|---|---:|---:|---:|
| Core | 1,024 | 1,024 | 0 |
| Common | 436 | 481 | 45 |
| Windows legacy | 626 | 633 | 7 |
| JavaScript | 189 | 260 | 71 |
| Python (22 + 5 + 5) | 32 | 32 | 0 |
| Total | 2,307 | **2,430** | **123** |

Debug WASM built with zero warnings/errors. The VS 2026 legacy build passed and generated both x86/x64 packed Excel-DNA XLLs. A separate solution build also passed:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:Platform=Any CPU' /p:PythonExe="C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe" /m /v:minimal
```

Focused final checks passed 178 Common output/compatibility tests (including 45 new composite tests), 71 independent composite JS tests and 72 Windows shared/composite recording tests (including seven new composite tests). The complete gate includes every older contract/fixture suite. Logs are under ignored `TestResults/Codex/Batch0207/{full-gate,solution-build,focused-common,focused-js,focused-legacy-tests}.log`; full TRX reports are `TestResults/Codex/{Core,Common,Legacy}.trx`.

The historical non-failing `UTA001 MatrixTestHelpers` discovery advisory remains. Existing TRX filenames produce overwrite notices when the gate is repeated; those notices refer to test reports, not workbook output. Early implementation failures were fixed without changing existing assertions or statistical fixtures: reservation error details, Windows VB lambda/name resolution, an explicit current-sheet test destination and the distinction between a maximum-length scalar and its extra host apostrophe escape. New aggregate-byte tests exercise both individual and combined table/annotation transport limits.

No live Excel, Excel Online, Mac or COM smoke was run. No packages/installers, deployment, commit, push, merge, branch switch, stash change or signing operation was performed. Both historical stashes remain present. No framework or dependency changes were made. Byte/line-ending inspection confirmed existing source encodings were preserved, including the Windows project BOM and missing final newline. All 74 Phase 04A roadmap rows and the separate audit ledger compare unchanged to HEAD.

**Preserved generated files:** Windows builds refreshed only the generated timestamp comments in `BESHstatNG/src/Help/HelpContext.vb` and `BESHstatNG/src/Help/HelpLinks.vb`. `BESHstatNG/Generated/VersionAttributes.vb` has generated CRLF line endings (Git reports normalization to LF; no normalized content difference). These generated artifacts were not discarded or restored, per the instruction to preserve them. They are incidental build output, separate from the 19 intended batch paths listed above and this note/checkpoint. There are no deleted files.

## Independent acceptance recipe — user execution only

1. Use a disposable Excel Online workbook and the WasmSmoke developer demo **Write synthetic composite to new worksheet**. It always requests a new worksheet at C10, independent of normal analysis destination controls. Confirm all six actual worksheet rectangles in the C10 table above and returned addresses; especially the second table at C30:E31 and envelope C10:H31.
2. Confirm the warning has a readable severity/code/path cue, chart block explicitly says it is a placeholder, and HTTPS link is explicitly inactive linked text. Inspect formula-looking table text and note/link strings in the formula bar: they must remain literal. Long-block content is retained in full even if fixed dimensions clip its display. Unsupported hyperlink/chart capability is disclosed by the returned degradation states, not claimed implemented.
3. Recheck normal ANOVA/KW current/new-sheet outputs, including C10, p-values, number formats and existing supplements. The old current-sheet path still lacks overwrite consent; use disposable content. Do not treat this as fixed by 02.07.
4. For unsafe links, malformed/out-of-grid or current-sheet composite payloads, verify an error with no sheet creation or write. A developer can invoke `beshOffice.writeCompositeOutputPlan(payload)` in the task-pane Console with a camelCase fixture. No script fetches/follows its HTTPS target. Valid links remain explicitly inactive even if native hyperlink APIs are available; those APIs are intentionally unused.
5. Compare Windows recording tests with the same geometry and metadata. There is no new production/live Windows composite renderer; any later opt-in live smoke requires a literal-block sink and separately approved disposable destination. Preserve existing Windows post-hoc/native chart workflows.

Acceptance requires independent review plus live Office geometry/text/format verification. 02.07 is not marked completed here. Stop after review; 02.08 and Phase 04A require separate authorization.
