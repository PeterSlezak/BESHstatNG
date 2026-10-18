# BESHStatNG — Batch 01.01: workbook/range identity and structured selection

**Baseline:** `BESHstatNG(20261016-095007).zip` (developer reports Phase 00.06 passed).

## Scope and API compatibility

This is an additive source-provenance and range-acquisition batch. It changes **neither** statistical calculations **nor** the existing `SpreadsheetAnalysisRequest`/`SpreadsheetRangeData` constructor, grouped parser, result tables, output destinations or dispatcher. The two current analyses still consume a single `SpreadsheetRangeData`. Multiple ranges are transport contracts for later input-family batches, not a new ANOVA parsing rule.

New Common types (all `Option Strict On`, .NET Standard 2.0):

- `SpreadsheetRangeAddress.Parse(text, [defaultWorksheetName])`: parses a **single bounded rectangular A1 area**, single cells, `$` absolute coordinates, optional quoted worksheet and `[workbook]` qualifier. Preserves workbook/sheet naming and normalizes coordinates to one-based Excel rows and columns. Handles apostrophes escaped as `''`; rejects reversed/out-of-grid/multi-area addresses.
- `SpreadsheetRangeReference`: an explicit request for an A1 range on a worksheet, with optional workbook name/host-supplied IDs, selection ID/revision and coordinates. `FromAddress` is the checked construction entry point. `QualifiedAddress` is for **display**; host acquisition uses validated worksheet and local A1 fields.
- `SpreadsheetRangeSelection`: an ordered set of range references. `Validate` requires explicit worksheet identities (multiple worksheets are allowed).
- `SpreadsheetRangeSetData`: ordered snapshots returned by an adapter.
- `SpreadsheetRangeData` gains optional `WorkbookId`, `WorkbookName`, `WorksheetId`, `SelectionId`, `SelectionRevision`, `StartRow`, `StartColumn`, `QualifiedAddress` and `ToReference()`. Older serialized inputs without these fields still work; zero coordinates and empty IDs signal unknown provenance.

### Source identity and protection against stale references

`WorksheetId` is populated from `Excel.Worksheet.id` in Office.js and from `Worksheet.CodeName` in the Windows provenance helper. These identifiers are **host scoped**, not promised as universally stable across different hosts or workbook copies. `WorkbookName` is a display hint, **not** a durable ID. Office.js can sometimes infer it from `Office.context.document.url`; if unavailable it is empty. `WorkbookId` is deliberately empty unless a future host provides a verified identity. The Office adapter fails closed when an unverified nonempty workbook ID is supplied, when a supplied workbook name conflicts with the current workbook, or when the requested worksheet ID differs from the acquired worksheet. It does not attempt cross-workbook acquisition in Batch 01.01. Selection revisions are optional transport fields, **not** an implemented change-event/revision tracking system yet (see Phase 03.06).

### Office.js / WasmSmoke

`getSelectedRangeData()` now obtains the worksheet from the selected **range** instead of assuming `getActiveWorksheet()` matches it. It includes worksheet ID and absolute coordinates in the snapshot. `getRangeDataByReference(reference)` reads an explicitly named worksheet without activating it. `getRangeSetData({ selectionId, ranges })` reads several worksheet ranges with **one** `Excel.run`/`context.sync` acquisition. Both reject malformed requests before interacting with Excel; worksheet-ID mismatches reject the result after the read.

The prototype task pane offers optional **Worksheet name** and **A1 address** fields. Leave both empty for original selected-range behavior; otherwise provide a worksheet + address, or a qualified A1 address. Both the preview and analysis Run use the same acquisition code. The grouped parser, calculations and workbook writer remain unchanged. The range-set bridge exists for future analyses; the current GUI deliberately does not combine multiple ranges into a grouped input.

Windows `ExcelDnaSharedApplicationAdapter.ToRangeReference(range)` captures the worksheet name, workbook name, worksheet code name and coordinates from an existing COM range. This **does not replace** the Windows importer and is not wired into existing forms in this batch.

## Verification

From repository root:

```powershell
# New and previous Office.js tests
node --test BESHStat.WasmSmoke/tests/*.test.cjs

# Existing catalog/source checks
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
python -m unittest discover -s BESHstatNG/tools/tests -p "test_*.py" -v

# All Common/Core and legacy tests, full Windows build, WASM build
# Run with the Visual Studio configuration used for your 00.06 checkpoint.
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
```

New `SpreadsheetRangeIdentityTests.vb` tests A1 quoting, `$`, single cell, Excel limits, invalid coordinates, optional legacy identity, provenance and serializable multi-worksheet selections. New `rangeIdentity.test.cjs` mocks an active worksheet different from the selected range, tests non-active named-sheet read, batches two ranges in a single `Excel.run`, stale ID and workbook mismatches, and pre-run validation. Existing 00.01 golden result fixtures must remain unchanged.

### Manual host smoke

1. In **Excel Online**, open the existing WasmSmoke task pane, select a regular ANOVA/KW rectangular range and leave the two new fields blank; Read then Run, confirm previous numerical results and output formatting.
2. Put the same grouped data on a **non-active worksheet** named `Study Data`. Keep another sheet active, enter `Study Data` and `A1:C5` (or `'Study Data'!A1:C5` in the address box), and run. Confirm the source preview reports `Study Data`, its coordinates and the expected test statistic. The sheet should not have to become active to read.
3. In **Windows Excel-DNA**, run ANOVA/Welch and KW/Dunn with old GUI inputs and check unchanged result placement.

### Limitations / next steps

- Names, ID fields and source addresses are transport metadata, **not** a live binding; historical snapshots are not automatically refreshed on workbook changes.
- Only one contiguous A1 area per reference; multi-area selections are represented as separate range references. Named Excel tables, formula references, orientation, merged-cell policies, dates, errors and large-range quotas belong to later batches.
- Excel Online/desktop/Mac must be tested on actual hosts. Node tests are adapter mocks, **not** a substitute for Excel integration tests.
- This patch is designed for the exact October 16 ZIP. Roll back the edited files and remove the added contract/test/documentation files to restore 00.06. Do not apply to a different checkpoint without re-auditing.

**Next dependency:** Phase 01.02 cell/error/date policies, after local 01.01 acceptance.
