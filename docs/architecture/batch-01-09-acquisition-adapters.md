# Batch 01.09: Office and Windows acquisition adapters

Implemented for review on `codex/01-09` after the separately validated
[Stage A .NET 10 migration](dotnet-10-migration.md). The master plan is unchanged;
this note does not mark host acceptance complete or authorize 01.10.

## Scope and files

Production adapters:

- `BESHstatNG/src/DM/ExcelDnaDataImporter.vb`: additive `AcquireRangeData` and
  `AcquireRangeSetData` entry points alongside unchanged `Import`/`ImportInto`.
- `BESHstatNG/src/DM/ExcelDnaSharedApplicationAdapter.vb`: COM range/area
  acquisition, reference verification and raw host-value conversion.
- `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js`: selected-area acquisition,
  reference/coordinate verification and canonical A1 snapshot spelling.

Regression artifacts:

- `BESHStat.Excel.Common.Tests/src/AcquisitionSnapshotTests.vb` (five tests).
- `BESHStat.Excel.Common.Tests/TestSupport/AcquisitionFixture.vb`: shared fixture
  loader/assertions, linked into legacy tests.
- `BESHStat.Excel.Common.Tests/TestData/Acquisition/canonical-two-ranges.json`:
  one canonical fixture consumed by Common, Windows and JavaScript tests.
- `BESHStatNG.Test/src/DataManagement/AcquisitionAdapterTests.vb` (11 tests).
- `BESHStat.WasmSmoke/tests/acquisitionAdapters.test.cjs` (14 tests).
- Common test project: fixture output inclusion; legacy test project: test/helper
  inclusion, fixture link, Common project reference and framework-provided
  `System.Runtime.Serialization` reference. No additional test packages.
- Current WasmSmoke README and development-validation notes; this document.

Core/Common production sources, schemas, algorithms and framework targets are
unchanged. No Ribbon/UDF/output writer, production UI, installer or signing
changes. Historical batch documents remain intact.

## Implemented contracts

Both adapters return the existing `SpreadsheetRangeData` and ordered
`SpreadsheetRangeSetData`. No new provider hierarchy or host objects enter
Core/Common. A range-set is transport, not a joined analysis table.

| Host entry point | Acquisition behavior |
|---|---|
| Windows `AcquireRangeData(range)` | Requires one rectangular COM area; captures its actual sheet/book, coordinates, values and formats. |
| Windows `AcquireRangeSetData(range)` | Captures every `Range.Areas.Item(i)` separately, in host enumeration order. |
| Windows `AcquireRangeSetData(IEnumerable(Of Range))` | Preserves caller range order, then each range's area order; supports distinct worksheets. |
| Windows `AcquireRangeSetData(selection, workbook)` | Validates structured A1 requests, resolves sheets in the supplied workbook, checks identity/coordinates and preserves request order, including duplicates. |
| Office `getSelectedRangeData()` | Existing single selected-range behavior; a disjoint selection still fails. |
| Office `getSelectedRangeSetData()` | ExcelApi 1.9 `getSelectedRanges().areas`; one snapshot per area in host order. Earlier hosts use the existing single-range reader, which cannot flatten disjoint areas. |
| Office `getRangeDataByReference(reference)` / `getRangeSetData(selection)` | Existing explicit single/multiple worksheet APIs, with metadata verification before loading values. |

Windows `SnapshotSelection` and `SnapshotValues` are internal test seams used by
the actual COM path. Tests supply controlled `Value2` fixtures and delegates;
they do not create Excel processes. JavaScript tests mock Excel.run, ranges,
worksheets, areas and metadata/value loading.

Coordinates are original one-based worksheet positions. Rows/columns within
`Rows` are zero-based; each cell's source coordinate is `StartRow + rowIndex`,
`StartColumn + columnIndex`. Blank edges remain part of bounded rectangles.
No sorting, gap-filling, concatenation or bounding-box union occurs.

A1 addresses use the existing Common convention: uppercase, no `$`, explicit
endpoints, including `C4:C4` for a single cell. Office snapshot address spelling
is now normalized to match Windows. Input references still accept single cells
and absolute A1 addresses. Worksheet/workbook names remain host-reported.

## Types, provenance and compatibility

Windows reads `Value2`, avoiding Date/Currency conversion through `Value`.
`Nothing`/DBNull/ExcelEmpty are blank; `""` is Text, zero is Number, Boolean
remains Bool, and text resembling an error remains Text. Native ExcelError,
ErrorWrapper and Excel COM integer HRESULT errors become CellError with the
original supplied code and per-cell host error text. Integer 2042 is a number;
boxed Double values are never reclassified by matching an error HRESULT.
Per-cell NumberFormat is retained, including for blanks/errors. Unsupported
host value objects fail explicitly; no blanket string conversion is used.

Windows reads workbook `Date1904` and records Excel1900/Excel1904 without
converting serials. Serial 60 remains numeric; explicit 1900 date interpretation
continues to reject Excel's phantom leap day through the existing Common policy.
Office's current primitive `values`/`valueTypes` API supplies error text but no
native error code, and this bridge does not verify a workbook date basis:
`NativeErrorCode = ""`, `DateSystem = Unknown`, `DateSerialOrigin = null`.
Neither adapter infers dates from number formats. Common's explicit host-origin
policy remains unchanged and is tested against the canonical fixture.

Windows WorksheetId is Worksheet.CodeName; Office WorksheetId is Worksheet.id.
They are host-scoped and must not be treated as interchangeable cross-host IDs.
Windows WorkbookName comes from Workbook.Name; Office retains its existing
Office.context.document.url name hint, which may be unavailable. Neither host
manufactures a WorkbookId. Structured requests with an unverifiable WorkbookId,
stale WorksheetId, conflicting workbook/coordinates or non-Excel address dialect
fail. Unverified SelectionRevision requests now fail instead of being silently
dropped. Both adapters return an empty revision; no live revision service exists.
SelectionId is preserved as caller correlation metadata, not asserted to be a
host-issued identifier. Direct selected-area acquisition creates no IDs.

The public legacy Windows `Import` and `ImportInto` implementations and
`NormalizeExcelValue` are unchanged, including their existing error-to-missing
conversion. Lossless acquisition is separate and is not wired into existing
forms. Statistical results, p-values and user-visible result tables are unchanged.

## Representative canonical snapshots

The shared JSON fixture contains full existing DTO fields and cell values.
Its common projection is:

```json
{
  "selectionId": "fixture-request",
  "ranges": [
    {
      "workbookName": "Book1.xlsx", "worksheetName": "Study Data",
      "address": "C4:F5", "startRow": 4, "startColumn": 3,
      "rowCount": 2, "columnCount": 4, "selectionId": "first-input",
      "kinds": [[0, 2, 1, 3], [2, 1, 4, 1]],
      "values": [[null, "", 0, false], ["#N/A", 61, "#N/A", 7.5]]
    },
    {
      "workbookName": "Book1.xlsx", "worksheetName": "Follow up",
      "address": "B9:C10", "startRow": 9, "startColumn": 2,
      "rowCount": 2, "columnCount": 2, "selectionId": "second-input",
      "kinds": [[3, 2], [0, 2]],
      "values": [[true, " label "], [null, ""]]
    }
  ]
}
```

`kinds` and `values` above abbreviate `Rows` for readability; they are not new
DTO fields. Kind numbers are the unchanged Blank=0, Number=1, Text=2, Bool=3,
CellError=4. Formats in the first rectangle are
`[General, @, 0.00, General]` / `[@, yyyy-mm-dd, General, 0.0]`.

Parity tests compare every common field and every cell kind/value/format against
the same fixture. They check native host fields separately: Windows error
`-2146826246`, host-scoped sheet IDs, and verified Windows date basis; Office
keeps unavailable native error/date metadata unknown. Equal common snapshots
do not imply identical host-specific IDs or fabricated Office error codes.

## Validation

Stage A gate: Core 1,024, Common 231, legacy 542, JS 29 passed before adapter
edits. Final batch command:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\verify-beshstatng.ps1 `
  -Python `
  -PythonExe "C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe" `
  -Js -Wasm -Legacy -VisualStudio 2026
```

Full results: Core **1,024**, Common **236**, legacy **553**, Office.js **43**
passed; Python suites **21 + 5 + 5** passed. Zero test failures/skips.
WasmSmoke and VS 2026 legacy Debug builds succeeded with zero compiler
warnings/errors, including 32/64-bit Excel-DNA packaging. Inventory comparison,
method catalog, generated metadata `--check` and retired-helper audit passed.
The specified WinPython executable was accessible inside the sandbox.

Logs: `TestResults/Codex/batch-01-09-full-validation.log` and
`TestResults/Codex/batch-01-09-full-validation-confirmed.log`. Windows PowerShell
formatted Python unittest stderr as a NativeCommandError in the outer logging
shell despite successful suites. The confirmation run explicitly captures and
returns the verification process's own exit code (0). Final source, including
the dimension-only area-collection load, is validated again in
`TestResults/Codex/batch-01-09-final-validation.log` (exit 0). TRX overwrite notices are
expected because the verification script reuses report names.

Focused validation used
`dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Debug --no-restore`
and `npm.cmd test` from `BESHStat.WasmSmoke`. The intermediate Windows build/
legacy verification passed 552 tests before adding the final phantom-date test.
No existing test was deleted or weakened. The original aggregate-size rejection
test caught a changed failure precedence; the adapter was corrected to retain
the size check before identity checks, with no test modification.

## Manual two-range acceptance (not executed)

Use synthetic fixture cells only. Save a workbook as `Book1.xlsx`. On `Study
Data`, fill `C4:F5` with the values/formats above: leave C4 truly blank; use
`=""` in D4; enter 0 and FALSE in E4/F4; use literal text `#N/A` in C5, 61 in
D5, `=NA()` in E5 and 7.5 in F5. On `Follow up`, fill `B9:C10` with TRUE,
` label `, a true blank and `=""`. Do not use a visually empty text formula
for the true blank cells.

1. **Windows:** with the add-in loaded in a development Excel session, use its
   debugger or a temporary local test caller to pass a `SpreadsheetRangeSelection`
   containing `Study Data!C4:F5` then `Follow up!B9:C10` to
   `ExcelDnaDataImporter.AcquireRangeSetData(selection, workbook)`. Inspect two
   snapshots in that order, dimensions 2x4 and 2x2, original coordinates, cell
   kinds/formats, worksheet CodeNames, actual workbook date basis and native E5
   error metadata. Separately Ctrl-select two disjoint bounded rectangles on
   one sheet and pass that COM Range to `AcquireRangeSetData(range)`; verify one
   snapshot per Areas item. Rerun existing Ribbon ANOVA/Welch or KW/Dunn and
   confirm unchanged output tables and formatting.
2. **Office/WASM:** sideload the existing development harness in the intended
   Excel test host. From task-pane developer tools call
   `beshOffice.getRangeSetData({selectionId: "fixture-request", ranges:
   [{worksheetName: "Study Data", address: "C4:F5", selectionId: "first-input"},
   {worksheetName: "Follow up", address: "B9:C10", selectionId: "second-input"}]})`.
   Compare the common projection to the fixture without activating the other
   sheet. Ctrl-select two disjoint rectangles on one sheet and click
   **Read selected areas (01.09)** on an ExcelApi 1.9 host; verify two separate
   previews in the returned host order, each with worksheet, address, dimensions
   and cell kinds/values. Check the existing single-range Read/Run
   ANOVA or KW workflow and selected whole-column narrowing. Capture the host
   version/platform and any divergence locally for acceptance.

These are recommendations, not automated/live host results. The selected-areas
button is an acquisition preview only; the harness has no multi-range analysis
computation. No Excel process or browser was launched here.

### Manual-acceptance UI follow-up

`BESHStat.WasmSmoke/Pages/Home.razor` now exposes the existing
`getSelectedRangeSetData()` bridge via **Read selected areas (01.09)** and
deserializes directly into `SpreadsheetRangeSetData`. It refreshes capabilities
and runs `CheckSelection` before acquisition. It always reads the current Excel
selection, independently of the optional single-range source fields. The
preview shows all acquired cells (bounded by the existing aggregate 10,000-cell
preflight), reuses the existing cell kind/value formatter and keeps each area
separate in host order. It does not invoke grouped parsing or a statistical
handler. Single-range Read/Run still use the original `AcquireRangeAsync` path.

`InvalidSelection` errors now explain that single-range operations require one
rectangle and point to the selected-areas button. On older hosts, one selected
range still works; multiple selected areas explain the ExcelApi 1.9 requirement.
Invalid/non-cell selections show a short retry instruction. The UI displays the
first line of JSException messages, removing appended JavaScript stack traces.
Unexpected non-selection bridge errors retain their original messages.

Seven additional Node regressions cover a single selected area on ExcelApi 1.9,
two disjoint areas, friendly synchronous/asynchronous InvalidSelection errors,
empty selections and older-host success/rejection. Existing order, mixed-type,
whole-column and size-preflight tests remain unchanged. No new dependencies or
framework targets are introduced. Component compilation is verified by the WASM
build; live button/rendering acceptance remains manual.

Manual follow-up: read one area, then two disjoint areas with the new button;
confirm the count, separate metadata and typed empty/blank/error values. With
the disjoint selection still active and source fields empty, click each original
single-range operation and confirm the concise explanatory status and no result
write. Select ordinary valid ANOVA/KW data and verify the original successful
Read/Run flow. Repeat one-range and multiple-area attempts on an older host if
available; only one range should be accepted there.

Follow-up automated validation: `npm.cmd --prefix BESHStat.WasmSmoke test`
passed **50** tests (seven new), and
`dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Debug --no-restore`
succeeded with zero warnings/errors. The full PowerShell command in the
Validation section was rerun with the explicit WinPython path, `-Python -Js
-Wasm -Legacy -VisualStudio 2026`: exit **0**, Core **1,024**, Common **236**,
legacy **553**, JS **50**, Python **21 + 5 + 5**, all passed without failures or
skips. WASM and legacy builds reported zero warnings/errors. Log:
`TestResults/Codex/batch-01-09-ui-validation.log`.
The UI follow-up changes only Home.razor, officeBridge.js, its existing
acquisitionAdapters.test.cjs and this architecture note. Starting help-file bytes
and VersionAttributes are captured separately under ignored
`TestResults/Codex/batch-01-09-ui-preserved` and compared/restored after generation;
the earlier adapter work, other local changes and stashes are retained.

## Limits and review boundary

- Host area enumeration order is preserved; neither API guarantees chronological
  mouse-click order. Explicit references provide deterministic caller order.
- Office explicit references address the currently hosting workbook only.
  Unknown workbook names/IDs cannot verify foreign workbook requests.
- Selected Office whole rows/columns retain selection-scoped used-range
  narrowing and the existing 10,000-cell individual/aggregate ceilings before
  cell payload loading. Area collections explicitly load only item dimensions,
  avoiding default Range-property loading before the size preflight. No 01.10
  chunking, quotas or cancellation implementation.
- Windows raw snapshots read the supplied rectangles, including blank edges;
  use bounded rectangles for manual tests. They add no whole-column optimization
  or large-range policy. Per-cell format/error-text reads favor fidelity here.
- Acquisitions are historical snapshots. Identity checks do not promise atomic
  multi-range transactions, workbook edit isolation or live selection revisions.
- Unit mocks/fixtures do not validate COM marshaling or real Online/Mac/Windows
  Office API behavior. Those require the manual host acceptance above.
- Generated help timestamps are compared and archived beneath ignored
  `TestResults/Codex`, then original working-copy bytes restored. Existing edits,
  stashes and VersionAttributes are preserved. Git may report existing CRLF/LF
  normalization notices on generated CSVs; these are inspected separately from
  compiler/test warnings.

Recommend separate migration and adapter commits after review, splitting the
Common test project's target and fixture-inclusion hunks. No commit/push/merge,
branch switch, deployment, 01.10 work or master-plan completion update is made.

Final repository review uses command-local
`git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check`
and `git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short`.
The ownership exception is process-local; global Git configuration is unchanged.
Generated HelpLinks/HelpContext differed only in timestamp lines; originals were
restored and SHA-256 comparisons confirmed both original help files and
VersionAttributes byte-for-byte. Both original stashes remain present.
