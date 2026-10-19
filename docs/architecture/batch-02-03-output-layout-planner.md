# Batch 02.03 — Centralized output layout planner

Implemented on the user-created `codex/02-03` branch from accepted 02.02 commit
`590ccea`. Pending independent source review; this note does not accept the batch.
The initial working tree was clean. Git required a command-local
`-c safe.directory=C:/Users/peter/source/repos/BESHstatNG` for the sandbox account;
no persistent Git configuration was changed. No branch, commit or stash changes.
The active roadmap checkpoint is 222 total / 21 accepted / 201 outstanding,
including this pending batch. All 74 Phase 04A batches remain after 04.07, with
04A.74 required before Phases 05–11. The Phase 04A audit is unchanged.

## Before-change source audit

* Core `ResultTable.returnSelf()` assembles/pads the complete matrix, including
  titles, top/left headers, body and footnotes. An ordinary empty Core table
  becomes a 1×1 blank matrix; a DTO can instead carry zero-length dimensions.
  `ResultTableOutputModel.RowCount/ColumnCount` describe that assembled matrix.
* Core `ResultTableWriterBase` uses one-based row/column pointers and advances by
  matrix height. `ProcessListofResultTables.writeToSheet` separately inserts
  optional separators or calls `shiftRowPointer()` for a blank row. Its existing
  below/beside dimension helpers are independent of Common output contracts.
* Windows `ExcelDnaResultWriter.WriteSharedRequest` uses its configured pointers,
  writes the full matrix, then calls `shiftRowPointer(model.RowCount)`. Its
  write core skips zero-area tables; shared-request conversion rejects missing
  rows/columns. Calling UI code owns inter-table pointer gaps. No writer changes.
* `ResultTableWriteRequestFactory.CreateMany` preserves table order and repeats
  the requested target for each request; it explicitly leaves spacing to hosts.
  `Create` calls the mutable target's `EnsureDefaults()`. That API and behavior
  remain unchanged; the new planner never invokes it.
* Office.js `writeResultTables` independently normalizes the first target,
  converts its one-based start to zero-based indexes, advances its own cursor
  by table height, and inserts one blank row between tables. It checks host
  capabilities, rejects NewWorkbook and creates/resolves sheets. Its current
  tests include destination rejection and mixed-destination protection. Neither
  the bridge nor its tests are changed here.
* 02.01 `OutputDocument.Artifacts` supplies authoritative order/IDs and immutable
  table/diagnostic/opaque plot snapshots. 02.02 `OutputSemanticPlan.Artifacts`
  follows that same order and shape. Styles are zero-based matrix-local cells;
  Core p-value annotations are one-based body-local and have already been
  translated by 02.02. Diagnostics and plots have zero semantic dimensions.

Source review included the accepted 02.01/02.02 notes, OutputDocument/Cell/Styles/
PValuePresentation tests, Stage304 and all six OutputSemantics fixtures. Existing
host differences in number formatting, text-valued p-values, header loops, alpha,
typed errors/formulas and destination support remain deferred to host stages.
This pure Common planner closes the geometry contract gap without removing any
existing placement/spacing duplication yet.

## Public contracts and policy choices

All new production types reside in `OutputLayoutPlanner.vb`, under
`BESHStatNG.ExcelCommon` (the project's root namespace plus `Namespace ExcelCommon`),
with `Option Explicit On` / `Option Strict On`. Common remains netstandard2.0.

| Contract | Meaning |
|---|---|
| `OutputLayoutOptions` | Immutable constructor values: `BlankRowsBetweenTables=1`, `MaxRows=1048576`, `MaxColumns=16384` |
| `OutputRectangle` | Immutable positive-area, one-based absolute start/end row/column; inclusive ends; intersection and containment |
| `OutputBlockPlacement` | Original immutable `Artifact`, ID/kind, zero-based original `Index`, full matrix dimensions, optional `Bounds` and nullable offsets, `BlankRowsBefore`, `IsPlaced` |
| `OutputLayoutPlan` | Read-only ordered `Placements`, immutable target scalars, options, reservation snapshot and optional `UsedBounds` envelope |
| `OutputLayoutPlanner.Create` | Pure all-or-nothing geometry preflight; optional semantic identity/shape validation |

```vb
Dim target As New SpreadsheetOutputTarget With {
    .Destination = SpreadsheetOutputDestination.CurrentWorksheet,
    .StartRow = 6, .StartColumn = 3}
Dim layout As OutputLayoutPlan = OutputLayoutPlanner.Create(
    document, target, New OutputLayoutOptions(blankRowsBetweenTables:=1),
    occupiedRectangles:=reservations, semanticPlan:=semantics)
' reservations and semantics are optional; no host calls are made.
For Each block As OutputBlockPlacement In layout.Placements
    If block.IsPlaced Then
        ' block.Bounds.StartRow/StartColumn/EndRow/EndColumn are absolute.
        ' Match styles by artifact ID and original index.
    End If
Next
```

Vertical stacking is the sole policy. There is no horizontal direction or column
gap option. Nonempty tables keep the requested start column; each subsequent
nonempty start is previous inclusive end row + 1 + configured blank rows.
The entire assembled matrix is occupied, including blank padding, titles, headers
and footers. Captions add no row. No hidden title/footer rows are synthesized.
The first block has `BlankRowsBefore=0`; each later positive-area block records
the configured gap. No trailing gap is computed or required to fit the grid.

`Bounds=Nothing`, `IsPlaced=False`, nullable offsets with no value, and
`BlankRowsBefore=0` explicitly represent an unplaced entry. Zero-row **or**
zero-column tables retain their original dimensions and metadata, occupy no
rectangle and do not advance the cursor or add a separator. Diagnostics/plots
retain the entire original immutable artifact (severity/code/message/field or
reference/purpose/caption/description), with zero dimensions. They consume no
rows and are not claimed as rendered. Composite diagnostic, footnote and chart
dimensioning/insertion remains 02.07; no invented one-row strings or chart size.
All-empty and nonspatial-only documents have `UsedBounds=Nothing`.

Absolute worksheet coordinates are one-based; offsets from the requested start
and semantic cell coordinates are zero-based. For each placed styled cell:

```text
absoluteRow    = block.Bounds.StartRow    + style.Row
absoluteColumn = block.Bounds.StartColumn + style.Column
```

Do not translate Core body annotations again. Layout never examines p-value
values, alpha, precision or significance. An optional semantic plan must match
artifact count, ID (ordinal), order, kind and table dimensions exactly, including
the zero dimensions for nonspatial entries. Mismatch throws `ArgumentException`;
no semantic plan is needed for placement. This validates alignment, not semantic
equivalence between different documents that reuse the same identities/shapes.

The target is copied into plan scalars without mutating caller input. `Nothing`
uses the existing target constructor defaults: NewWorksheet, BESHStat_Result,
A1. Explicit nonpositive or out-of-grid starts and undefined destination enums
fail fast, even for empty documents. Null/empty/whitespace worksheet names resolve
to `BESHStat_Result` in the snapshot only; other strings are retained verbatim,
without trimming, name sanitation, availability or physical sheet checks.
CurrentWorksheet/NewWorksheet/NewWorkbook preserve requested logical intent.
NewWorkbook in a plan asserts no host capability. Names never establish physical
workbook identity or existence. A future host may reject/resolve the request.

Options require positive Int32 grid limits and a nonnegative gap strictly less
than MaxRows, even for an empty or single-table document. Custom smaller or
larger grids are supported; the defaults are explicit Excel grid limits, without
any Excel API dependency. Immutable options/rectangles may safely be shared;
input collections are copied into read-only lists. Changing the original target,
replacing an options variable or modifying a reservation list after planning
cannot change the plan. Concurrent mutation during snapshot construction is not
supported. The planner allocates per-artifact/per-reservation data, never a full
worksheet or per-cell output matrix.

## Exact C6 acceptance fixture

Target: CurrentWorksheet, StartRow=6, StartColumn=3, one blank row between blocks.

| ID | Assembled rows × columns | Inclusive occupied rectangle | Row/column offset | Blank rows before |
|---|---:|---|---|---:|
| table-a | 2 × 3 | C6:E7 | 0,0 | 0 |
| table-b | 3 × 2 | C9:D11 | 3,0 | 1 |
| table-c | 1 × 4 | C13:F13 | 7,0 | 1 |
| table-d | 2 × 1 | C15:C16 | 9,0 | 1 |

```text
             C        D        E        F
 6–7       [ table-a: C6:E7      ]
 8         blank separator
 9–11      [ table-b: C9:D11 ]
12         blank separator
13         [ table-c: C13:F13             ]
14         blank separator
15–16      [table-d]
```

The envelope (`UsedBounds`) is C6:F16, an 11×4 bounding rectangle, **not** a
claim that all 44 cells are occupied. The exact union has 18 matrix cells;
rows 8, 12 and 14 are separators and the narrower table blocks leave other
envelope holes. No row 17 separator belongs to the plan. Tests repeat the same
document at E2 with two blank rows: E2:G3, E6:F8, E11:H11, E14:E15; envelope
E2:H15. Repeated plans have identical scalar projections independent of culture.

## Geometry, bounds and reservations

`Intersects` uses inclusive row and column interval intersections. Sharing an
end/start **cell coordinate** is overlap; adjacent ranges whose next start is
the prior end + 1 do not overlap. Containment includes boundary cells and full
enclosure. Diagonally separated row/column intervals are disjoint. Geometry
construction rejects nonpositive starts and reversed/negative extents.

All caller-supplied occupied/reserved rectangles refer to this plan's **same
resolved logical scope**. The caller must select that scope before supplying
them; there is no cross-sheet reservation collection or case-sensitive sheet
identity inference. Reservations are copied, validated against grid limits,
then checked in supplied order against each placed block in document order.
Null or out-of-grid reservations fail, including with an empty document.
Reservations may overlap each other; they are constraints, not output blocks.
An intersection throws `InvalidOperationException` naming the artifact, its
inclusive coordinates, zero-based reservation index and reservation coordinates.
No partial usable plan returns, no auto-shift occurs, and no overwrite is implied.
Gap-only reservations and holes outside exact table rectangles do not collide.
Generated blocks are also cross-checked pairwise for internal overlap.

Coordinate arithmetic uses Int64 before narrowing. Each input operand is bounded
by Int32 and each accepted prior end is grid-bounded, so intermediate sums cannot
overflow Int64; over-Int32/grid results are rejected before `CInt`. Out-of-bounds
errors identify the artifact, required rectangle and configured grid. Invalid
inputs/options use `ArgumentException` subclasses. Tests exercise Integer.MaxValue
grid corners, additions exceeding Int32, oversize gaps and exact last-cell fits
without allocating large matrices. Negative matrix dimensions cannot enter the
frozen OutputTableArtifact contract; the planner also defensively rejects them.

These checks cannot certify live occupancy, merged/protected cells, current-sheet
availability, name availability, overwrite consent or reservation freshness.
No supplied reservations means only that none were supplied. Actual preflight,
stale-state checks and safe write outcomes remain host responsibilities in
02.06/02.08.

## Compatibility and downstream handoff

02.01 documents/models and 02.02 canonical typed cells/styles/p-value policies
are unchanged. `IsResultTable`, exact decimals, literal text, nonfinite errors
and original one-based annotations retain their source semantics. Analysis-result
v1, Core ResultTable/ResultTableOutputModel and ResultTableWriteRequest/CreateMany
are unchanged. Tests compare legacy factory requests before/after planning and
confirm every request still repeats the original target without owning spacing.
All five Stage304 goldens and the six OutputSemantics fixtures remain unchanged.

02.04 Excel-DNA must resolve/validate its target and consume each exact planned
rectangle and artifact identity; it must stop independently advancing layout
pointers for the planned route. 02.05 Office.js must consume those same absolute
coordinates, converting to zero-based API indexes once at the boundary, and
match style cells by artifact ID/local offsets. Neither adapter should recompute
table starts, spacing or semantic p-values. Later capability/target validation
and safe outcomes remain necessary. No support for NewWorkbook is advertised
by this geometry API and no host implementation is changed in 02.03.

Future Phase 04A Windows report extraction returning generic tables can reuse
these same coordinates and style identities without analysis-specific layout.
No report extraction, chart/PDF enhancement or ledger edits are included.
Core/Common remain netstandard2.0; modern tests/WasmSmoke net10.0; Windows add-in
and tests Framework 4.8. No packages, interfaces, serializers, statistical
algorithms, Ribbon/UDFs, task pane, bridge or chart renderer changed.

## Files, fixtures and acceptance

Changed files:

* `BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb` — all additive contracts/planner.
* `BESHStat.Excel.Common.Tests/src/OutputLayoutPlannerTests.vb` — focused MSTests.
* `BESHStat.Excel.Common.Tests/TestData/OutputLayout/*.txt` — seven layout-only references.
* `BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj` — copy those references.
* This architecture note.
* `planning/BESHStatNG_CrossPlatform_MasterPlan.md` — four active checkpoint lines only.

The test-only reference projection includes order, ID/kind, logical scope/start,
full dimensions, gap/options, offsets, nonspatial flags, exact rectangles and
envelope. Six references were derived from the accepted semantic fixture shapes;
the C6 fixture states the independently specified acceptance coordinates.
They are read-only expected files, never updated by tests; there is no production
wire/JSON schema. Tests use invariant scalar text and fixed ordering, and check
repeatability under sk-SK as well. Acceptance requires exact C6 blocks, correct
mixed/empty behavior, immutable snapshots, fail-closed collisions/bounds, semantic
alignment and passing compatibility/full regression gates.

## Portable preview and optional manual host comparison

Run the focused C6 test below and inspect `Layout.trx` standard output (or the
`four-tables-c6.txt` reference) for the four exact rectangles above. This is a
portable plan preview, not a live worksheet write. It has been executed in the
automated tests.

Optional, **not executed**: in an already approved Excel test workbook, visually
compare four synthetic matrices at C6:E7, C9:D11, C13:F13 and C15:C16, leaving
8/12/14 blank. Record any current Windows writer/pointer differences for 02.04.
In an already approved Office/WASM smoke session, compare existing multi-table
spacing with the corresponding analysis reference; record host behavior for
02.05. No live Excel, browser/WASM, Mac or Office GUI smoke is claimed by 02.03.

## Validation evidence

Observed on 19 October 2026, all final commands below exited **0**:

```powershell
dotnet test .\BESHStat.Excel.Common.Tests\BESHStat.Excel.Common.Tests.vbproj --no-restore --filter FullyQualifiedName~OutputLayoutPlannerTests --logger 'trx;LogFileName=Layout.trx' --results-directory .\TestResults\Codex\Batch0203

dotnet test .\BESHStat.Excel.Common.Tests\BESHStat.Excel.Common.Tests.vbproj --no-build --no-restore --filter 'FullyQualifiedName~OutputLayoutPlannerTests|FullyQualifiedName~OutputDocumentTests|FullyQualifiedName~OutputCellTests|FullyQualifiedName~OutputStylesTests|FullyQualifiedName~PValuePresentationPlanTests|FullyQualifiedName~Stage304ContractSnapshotTests' --logger 'trx;LogFileName=OutputCompatibility.trx' --results-directory .\TestResults\Codex\Batch0203

powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\verify-beshstatng.ps1 `
  -Python `
  -PythonExe "C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe" `
  -Js -Wasm -Legacy -VisualStudio 2026
```

The first focused compile attempts exited 1 for two new test-source errors:
`single` was a VB keyword used as an identifier (BC30183 with cascading parser
errors), then `IReadOnlyList.Count(predicate)` bound to the property (BC32016).
Renamed the variable and used `Where(predicate).Count()`. No production or
existing test failure remained. The final focused run passed 49/49; combined
output compatibility selection passed 129/129 with no failures or skips.

| Full gate component | Actual result | Exit |
|---|---|---:|
| Core MSTest | 1,024 passed, 0 failed/skipped | 0 |
| Common MSTest | 374 passed, 0 failed/skipped (325 baseline + 49 new) | 0 |
| Windows Framework 4.8 MSTest | 561 passed, 0 failed/skipped | 0 |
| Office.js Node tests | 74 passed, 0 failed/skipped | 0 |
| Python tooling / help / scripts | 21 + 5 + 5 = 31 passed | 0 each |
| **Total full-gate tests** | **2,064 passed (2,015 baseline + 49)** | **0** |
| Stage304 inventory, methods catalog, generated metadata check, retired-helper audit | All passed | 0 each |
| .NET 10 WASM Debug build | Succeeded, 0 warnings / 0 errors | 0 |
| VS2026 legacy Debug build / Excel-DNA packing | Succeeded, 0 warnings / 0 errors; both x86 and x64 packed XLLs produced | 0 |

Existing legacy discovery advisory: `UTA001`, TestClass attribute on non-public
`BESHStatNG.Test.MatrixTestHelpers`; it is not a compiler/test failure. TRX logger
warnings about overwriting prior ignored results are also non-failures. There
were no unrelated test failures. Git also emits its CRLF-to-LF advisory for the
restored VersionAttributes.vb; its bytes match the pre-run copy and Git reports
no content change. Restore reported all projects up-to-date; no
packages were installed. The Debug post-build script is echoed by MSBuild but
exits at its non-PreRelease guard before signing; signing was not executed.

Full gate output was captured to
`TestResults/Codex/Batch0203/full-gate.log`; focused TRX files are alongside it.
Full-suite `Core.trx`, `Common.trx` and `Legacy.trx` are under
`TestResults/Codex/`. These are ignored local evidence, not delivery sources.
Packed artifacts were confirmed at `BESHstatNG/bin/Debug/`:
`BESHStatNG-AddIn-packed.xll` (1,902,080 bytes) and
`BESHStatNG-AddIn64-packed.xll` (1,827,840 bytes).

Before validation, exact copies of HelpContext.vb, HelpLinks.vb and
Generated/VersionAttributes.vb were saved under the ignored Batch0203 evidence
directory. The legacy build changed only their generated timestamp/date lines;
each was restored from its saved copy and SHA256 checked byte-for-byte. The six
OutputSemantics fixture SHA256 values also match the pre-run values. Git confirms
no changes to Stage304, accepted 02.01/02.02 production sources, legacy factory
or the Phase 04A audit. Both original stashes remain present.

Final checks (command-local safe-directory flag as described above):

```powershell
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
```

Both exit 0; only the inventory above is changed. No live Excel write or
browser/WASM/Mac GUI smoke was run; no host integration is claimed. No commit,
push, merge, branch change, installer, publishing or 02.04 work was performed.
Suggested commit message for after independent review:
`feat(common): add batch 02.03 portable output layout planner`.
