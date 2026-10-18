# Batch 01.10 — Safe large-range acquisition and quotas

Implementation for independent review; acceptance is pending. Work starts from
accepted 01.09 commit `2d9f418`, after the .NET 10 migration `caba668`, on the
existing `codex/01-10` branch. The master plan now records 18 accepted batches
and 130 outstanding of 148, with 01.10 in progress. Historical checkpoint text
and future requirements are retained. No 02.01 work is included.

## Portable contract

`BESHStat.Excel.Common/src/Input/InputSizeLimits.vb` contains an immutable policy
and a small per-operation `InputAcquisitionBudget`. There are no host objects,
address parsers, Excel dimensions, filesystem dependencies or WASM assumptions.
Hosts add every rectangle's metadata before reading any cells, then call
`Checkpoint` before/after payloads. `AddRange` rejects without changing counters.

| Policy | Common / Office default | Meaning |
|---|---:|---|
| `MaxRangeCells` / `maxRangeCells` | 10,000 | Individual rectangle, including blank edges |
| `MaxTotalCells` / `maxTotalCells` | 10,000 | Sum of all rectangles; duplicates count again |
| `MaxRanges` / `maxRanges` | 64 | Number of rectangles |
| `MaxChunkCells` / `maxChunkCells` | 2,000 | Maximum cells in one payload slice |
| `MaxEstimatedBytes` / `maxEstimatedBytes` | 67,108,864 | Estimated retained/transient application memory budget (64 MiB) |
| `EstimatedBytesPerCell` / `estimatedBytesPerCell` | 1,024 | Planning estimate per source cell |
| `TimeBudgetMilliseconds` / `timeBudgetMilliseconds` | 30,000 | Cooperative deadline measured from operation start |

Chunk cells and total cells are separate limits. The canonical DTO still holds
the entire accepted snapshot. Chunking does not make its retained size constant.
The estimate is `totalCells * estimatedBytesPerCell`: it allows for typed cell
objects, row references, source matrices, strings and serialization copies for
ordinary data. It is a conservative planning assumption for ordinary cells,
**not a measured allocation or a hard heap/response-byte bound**. Long text,
format strings, Office proxy caches and Blazor interop copies can exceed it.
Neither host can determine actual string sizes from dimension metadata alone.
Failure releases application references for garbage collection; immediate
reclamation of Office caches is not guaranteed.

Common accepts explicit positive Int64 cell counts and positive Int32
range/chunk counts. Byte/estimate options must be finite, positive and at most
9,007,199,254,740,991; the time budget is finite, positive and at most Int32.MaxValue
milliseconds. A host choosing a custom per-cell estimate must justify that
estimate for its representation. Multiplication checks division before multiplying;
aggregation checks available capacity before adding. Zero/negative dimensions,
Int64 multiplication overflow and aggregate overflow cannot trigger a cell
allocation. Future hosts may supply dimensions beyond Excel's maxima; Common
tests explicitly do so. No LibreOffice adapter is introduced.

Common reuses `SpreadsheetInputValidationException` and
`SpreadsheetValidationReport`. Stable issue codes identify dimensions, overflow,
range cells, total cells, range count, estimated bytes and time budget; messages
contain requested and allowed quantities and a corrective action. Cancellation
uses the standard `OperationCanceledException` and `CancellationToken`.
`Stopwatch` provides elapsed time. The optional elapsed callback is a deterministic
host/test seam and must return finite, nonnegative, monotonic milliseconds.

## Office bounded opt-in and acquisition

All four existing bridge functions retain no-options support:

```javascript
getSelectedRangeData(options)
getSelectedRangeSetData(options)
getRangeDataByReference(reference, options)
getRangeSetData(selection, options)
```

Omitting options preserves the 10,000-cell individual/aggregate ceilings and
existing over-limit messages. Options may explicitly raise those limits, but
Office enforces additional safety ceilings: 1,000,000 individual/total cells,
1,024 ranges, 10,000 cells/chunk, 256 MiB estimated memory, 300,000 ms and at least
1,024 estimated bytes/cell. All limits apply together: the memory ceiling means
at most 262,144 cells at the minimum estimate, even when the cell ceiling is
higher. Infinity, zero, negative, unsafe/fractional counts and nonnumeric options
are rejected. There is no unlimited mode. These are Office adapter safeguards;
they are not universal spreadsheet limits in Common.

The bridge mirrors the Common policy because JavaScript must reject metadata
before Office sends cell payloads to the WASM application. Common and JS tests
cover the same defaults, exact limits, aggregate/estimate limits and deadlines.
The existing WASM UI and selected-areas preview remain unchanged and bounded by
their defaults. Explicit opt-in smoke reads use task-pane developer tools only;
this adds no production UI or multi-range statistical analysis.

Acquisition proceeds as follows:

1. Validate options/cancellation, explicit references and range count. Load
   dimensions, positions, address and worksheet name/id as metadata only.
   `RangeAreas.areas` still loads only `items/rowCount` and `items/columnCount`
   initially; never default-load `items`. Its count is checked before preparing
   the per-area metadata. That host collection metadata itself is not chunked.
2. Narrow selected whole rows/columns to their selection-scoped used range,
   retaining the ExcelApi 1.4 null-safe path and older scoped fallback. Explicit
   addressed rectangles retain all blank edges and are never silently narrowed.
3. Check each area, aggregate cells, range count and estimate before the first
   payload. Preserve aggregate-size rejection before per-entry identity checks.
   Verify worksheet identity, Excel bounds, canonical address and coordinates.
   Workbook IDs, unverifiable workbook names, stale worksheet IDs, unsupported
   address dialects and selection revisions still fail closed under 01.09 rules.
4. Lazily generate tiles. Width is `min(columns, maxChunkCells)`, height is
   `max(1, floor(maxChunkCells / width))`. Walk row blocks then column blocks,
   clipping the last block. Thus wide rectangles split columns as well as rows.
   A source that already fits one tile can itself be the bounded slice;
   larger sources use `Worksheet.getRangeByIndexes` for every slice.
5. Load one slice's values/valueTypes/numberFormat and metadata, then await
   `context.sync()`. Never queue multiple cell payloads concurrently. Check
   cancellation/deadline again, verify positions, address and matrix dimensions,
   and copy typed cells into their original rectangle offsets. Only the finished
   selection is returned. Any middle failure rejects the entire acquisition.

The returned DTO schema is unchanged. All blank edges, empty text, error text,
Booleans, numbers, formats, one-based source coordinates, source worksheet and
area order remain. Nonfinite numbers retain the existing bridge error-cell
behavior. Unsupported host objects fail instead of becoming coerced strings.
Office supplies no native error code or verified workbook date origin here:
those fields remain unknown. It never infers a date from a format. Explicit
multi-sheet requests preserve caller order and duplicates; selected areas
preserve host order, which does not promise chronological mouse-click order.
ExcelApi 1.9 selected areas and older-host single-range fallback share the reader.

## Windows scope and compatibility

The original public adapter signatures remain and delegate to bounded overloads
accepting `InputSizeLimits` and `CancellationToken`. Existing importer lossless
entry points inherit defaults. `Import`, `ImportInto`, `NormalizeExcelValue`,
Ribbon callbacks and UDF exports are untouched.

Windows now gathers all requested areas/references and preflights their
dimensions/aggregate before any `Value2`. Explicit requests also verify all COM
range identities before the first value read. The live path calls the same
`SnapshotValueReader` delegate seam exercised by the oversized-before-reader
regression. Snapshot conversion checks the operation between rows, as well as
before/after `Value2` and between areas.

COM chunking is outside this narrow optional change. Windows defaults override
the Common chunk size to 10,000 for its bounded single `Value2` response, keeping
the ordinary 01.09 acquisition ceiling aligned with Office. With explicit
options a rectangle must fit both the shared policy and
`min(MaxChunkCells, 10000)`. Even a larger policy cannot read a larger single
`Value2`. An explicit larger aggregate may permit several small rectangles;
memory/time/range-count quotas still apply. Whole-column requests remain
rejected before `Value2`; Windows adds no used-range narrowing policy.
Optional request workbook/sheet IDs constrain verification without replacing the
actual host workbook name/worksheet CodeName in the returned snapshot. The
caller SelectionId still travels with it. Cell formats, native errors, array lower bounds and workbook Date1904 semantics
retain the 01.09 canonical fixture tests.

Core/Common remain `netstandard2.0`, modern tests/WasmSmoke `net10.0`, legacy
host/tests .NET Framework 4.8. No target/dependency, packaging, signing,
installer, statistical calculation, p-value or result-table changes are made.

## Cancellation, deadline and host limitations

JavaScript accepts `options.signal` (an AbortSignal or a host-neutral object
exposing an `aborted` Boolean). Common/Windows use `CancellationToken`.
Already-cancelled requests refuse before host acquisition. Checkpoints cover
initialization/metadata time, before each chunk, after each response and before
return. Cancellation or timeout returns no usable partial snapshot. Errors carry
JS `code`, `limit`, `requested`, `allowed` fields for quota failures; existing
Blazor exception presentation uses the actionable message.

These are cooperative guarantees. They **cannot abort an in-flight Excel.run,
context.sync, COM property read, or blocked host**, nor guarantee completion or
UI response within the stated milliseconds. A response that arrives after the
deadline is discarded and no next payload is queued. Native host errors retain
their diagnostic. A cancellation event needs an opportunity to run on the
browser event loop. There is no new lifecycle/progress framework.

Metadata/payload reads are historical observations, not atomic workbook
transactions. Coordinates and identities are checked, but edits between chunks
can still produce a snapshot of different moments. No live revision service,
edit isolation or durable cross-host workbook ID is claimed. Mock tests cannot
certify Excel Online/Mac/desktop marshaling, payload-byte limits, actual memory,
host availability or latency. Those observations remain open for manual review.

## Automated validation

Focused commands:

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Debug --no-restore
npm.cmd --prefix BESHStat.WasmSmoke test
```

Required full command (run from the repository root):

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\verify-beshstatng.ps1 `
  -Python `
  -PythonExe "C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe" `
  -Js -Wasm -Legacy -VisualStudio 2026
```

Final full validation: exit **0**; **zero failures/skips**.

| Suite | Passed | Added over checked-out 01.09 |
|---|---:|---:|
| Core | 1,024 | 0 |
| Common | 251 | 15 |
| Legacy | 561 | 8 |
| Office.js | 74 | 24 (checkout already contained 50, including the 01.09 UI follow-up) |
| Python tooling / documentation / discovery | 21 + 5 + 5 | 0 |

WASM Debug and VS 2026 legacy Debug builds succeeded with **zero compiler
warnings/errors**, including Excel-DNA x86/x64 packing. Frozen inventory,
method catalog, generated metadata `--check` and retired-helper audit passed.
Log: `TestResults/Codex/batch-01-10-confirmed-validation.log`. The earlier
`batch-01-10-full-validation.log` passed before the final proxy-position and
optional-request-identity regressions; `batch-01-10-final-validation.log` passed
after those fixes. The confirmed full run also includes the shared older-host
fallback deadline regression.

Windows PowerShell formats successful Python unittest stderr as a
`NativeCommandError` in the outer redirected log. All three Python summaries
are OK and the captured verification process exit code is 0. TRX overwrite
notices are expected when reusing the script's report names. Git may print a
CRLF-to-LF notice for preserved VersionAttributes; its captured bytes remain
unchanged. Debug exits the existing post-build routine before PreRelease
signing. No signing workflow or key is used.

Generated HelpLinks,
HelpContext and VersionAttributes are captured before legacy validation;
regenerated copies are archived under ignored TestResults and only those files
are restored from their captured bytes, with SHA-256 equality confirmed for all
three. Final `git diff --check` passes; status contains only the eleven batch
paths listed below. Both original stashes are retained. No reset, stash modification, branch
switch, commit, hook activation, installer, deployment or live Excel is used.

Existing tests remain. Two sync-count assertions now require sequential area
payloads; the single-cell absolute-address fixture now supplies a true 1x1
matrix, so strict chunk-shape checks can reject malformed host matrices.

Changed/new paths:

| Path | Change |
|---|---|
| `BESHStat.Excel.Common/src/Input/InputSizeLimits.vb` | New portable quota/checkpoint contract |
| `BESHStat.Excel.Common.Tests/src/InputSizeLimitsTests.vb` | New quota/overflow/cancellation tests |
| `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js` | Shared preflight and sequential tiled reader |
| `BESHStat.WasmSmoke/tests/largeRangeAcquisition.test.cjs` | New bounded-payload regressions |
| `BESHStat.WasmSmoke/tests/acquisitionAdapters.test.cjs` | Sequential sync count and correct 1x1 mock payload |
| `BESHStat.WasmSmoke/tests/rangeIdentity.test.cjs` | Sequential area sync assertion |
| `BESHstatNG/src/DM/ExcelDnaSharedApplicationAdapter.vb` | Pre-Value2/aggregate checks; bounded overloads |
| `BESHStatNG.Test/src/DataManagement/AcquisitionAdapterTests.vb` | Safety seams and provenance/parity regressions |
| `docs/architecture/batch-01-10-large-range-acquisition.md` | This design, validation and manual procedures |
| `docs/architecture/development-validation.md` | Link to quota/manual validation guidance |
| `planning/BESHStatNG_CrossPlatform_MasterPlan.md` | Accepted 01.09/current 01.10 status correction |

## Manual smoke 1 — explicit large bounded read (not executed)

Use synthetic cells only, in Excel Online with the existing development harness.
Repeat in desktop Office.js if available; record platform/API version locally.
On `Data`, fill **B3:K1205** (1,203 x 10 = **12,030 cells**) with
`=(ROW()-1)*20000+COLUMN()-1`. In task-pane developer tools:

```javascript
const options = { maxRangeCells: 15000, maxTotalCells: 15000, maxChunkCells: 2000 };
const reference = { worksheetName: "Data", address: "B3:K1205" };
const snapshot = await beshOffice.getRangeDataByReference(reference, options);
```

Expected: **seven payloads** of 2,000/2,000/2,000/2,000/2,000/2,000/**30**
cells, plus one metadata sync. Tile rows are 3–202, 203–402, 403–602, 603–802,
803–1002, 1003–1202 and 1203–1205. Check both sides of boundaries:
B202 = 4,020,001, B203 = 4,040,001, and K1205 = 24,080,010. Snapshot coordinates
remain startRow 3, startColumn 2 and address B3:K1205. Inspect payload requests
with temporary debugger breakpoints, without changing tracked code.

Then omit options: expect refusal at 12,030 requested / 10,000 allowed before
any values load. Repeat with maxEstimatedBytes 1,024: expect estimate refusal
before payload. Use a very small positive timeBudgetMilliseconds to exercise
the time message; record observed behavior without assuming a measured duration.
For cancellation use an AbortController and schedule abort (or trigger it while
paused between responses); await rejection, confirm no next payload/snapshot:

```javascript
const controller = new AbortController();
const reading = beshOffice.getRangeDataByReference(reference, { ...options, signal: controller.signal });
setTimeout(() => controller.abort(), 0);
await reading; // expected cancellation; if already finished, repeat with a debugger pause
```

Timing/cancellation placement depends on host latency and browser scheduling.
Do not report automated fixture execution times as Excel benchmarks.

## Manual smoke 2 — multi-area and ordinary 01.09 parity (not executed)

On `Data` add **F2001:H2002** (2 x 3 = **6 cells**) with TRUE, empty text,
true blank, a numeric zero, literal text #N/A and `=NA()`, using different
number formats. Ctrl-select B3:K1205 and F2001:H2002. Call
`getSelectedRangeSetData(options)` from developer tools. Expect **two separate
snapshots**, **12,036 total cells**, **eight payloads** (seven + one) and two
metadata syncs = **10 syncs**, in host area order. Confirm the original
coordinates, blank/empty-text/error/Boolean distinctions and formats. Default
UI acquisition should refuse this large selection. Two copies of B3:K1205 in
an explicit selection with options request **24,060 cells** and exceed the
15,000 aggregate quota before the first payload. Nothing is truncated.

Use the unchanged 01.09 two-range fixture (C4:F5 and B9:C10: **8 + 4 = 12
cells**) with **Read selected areas (01.09)**: expect two previews and two
payloads, no statistical execution. Rerun the existing valid single-range
ANOVA and Kruskal–Wallis Read/Run workflows, confirm the same result tables,
formats and blank-edge behavior. On an older host one rectangle remains
readable; disjoint selection still reports the ExcelApi 1.9 requirement.

Optional Windows debugger/test caller: use existing lossless acquisition for
the 12-cell fixture and verify native errors/date basis. Attempt a 10,002-cell
bounded rectangle and confirm refusal before the Value2 delegate/COM read.
An exact 100x100 rectangle is accepted with default Windows limits. Run normal
Ribbon ANOVA/Kruskal manually and compare existing output. Do not launch Excel
automatically as part of validation.
