# Batch 01.05A — host-neutral spreadsheet contracts

**Scope:** `BESHStat.Excel.Common` contracts only. Excel-DNA and Office.js/WASM remain the supported application hosts. This note is not evidence of LibreOffice compatibility and does not define an implementation or release requirement for LibreOffice.

## Host identification vs. capability admission

`HostCapabilitySnapshot.IsExcel` is a compatibility and host identity indicator. Existing snapshots with `Host="ExcelDna"`/`"OfficeJs"` and `IsExcel=True` remain valid even if they lack the new `IsSpreadsheetHost` property. `HostCapabilityProfiles.WindowsExcelDna()` and `OfficeJsExcel(platform)` now set both indicators. The Office.js bridge's existing JSON payload need not change: its `isExcel` field continues to be accepted.

A potential different spreadsheet host must report a nonempty `Host` identity, set `IsSpreadsheetHost=True`, and advertise **only** operations it has actually implemented. `IsSpreadsheetHost=True` neither grants `ReadSelectedRange` nor implies `NewWorkbook`; the same capability IDs and preflight operations determine availability. Unknown/unidentified hosts are rejected; an identified host lacking an operation receives `host.capability-missing` or `output.destination-unsupported`. Pure statistical analysis may not require host capabilities; reading and writing still do.

Host reports are trusted adapter declarations, not authorization/security credentials. They do not demonstrate that a hypothetical host adapter exists.

## Address dialect and bounds

`SpreadsheetRangeAddress.Parse` and `SpreadsheetRangeReference.FromAddress` remain **Excel A1** parsing APIs, bounded at 1,048,576 rows and 16,384 columns. Their existing syntax and qualified-address normalization remain unchanged. Legacy `SpreadsheetRangeReference` and `SpreadsheetRangeData` instances default to `SpreadsheetAddressDialect.ExcelA1`; old serialized reports lacking `AddressDialect` retain this behavior.

An adapter that has **already resolved its local range** can supply `SpreadsheetRangeReference.FromCoordinates(worksheetName, startRow, startColumn, rowCount, columnCount, displayAddress)` or a range snapshot with `AddressDialect=HostNative` and positive, one-based coordinates. In that case `Address` is *opaque display-only metadata*: Common never runs it through Excel A1 parsing or treats it as a formula. `SpreadsheetRangeSelection.Validate` validates coordinate positivity and integer representability for native ranges, but the adapter must validate its own address syntax, permissions, source identity, and grid-size limits before acquisition. Unknown address dialects fail validation. `FromUserInput` stays an **Excel A1-specific** UI helper; do not pass host-native expressions to it.

## Date serials

`SpreadsheetDateSystem` and `ExcelDateSerialConverter` remain **Excel-specific** (`Unknown`, `Excel1900`, `Excel1904`), including Excel's rejected phantom 1900-02-29 day. A source that cannot verify its Excel date basis must set `Unknown` and cannot convert a date numeric using the Excel converter.

Other adapters can attach `SpreadsheetRangeData.DateSerialOrigin` (`Nullable(Of DateTime)`) and explicitly call `SpreadsheetCellValuePolicy.Evaluate(cell, options, SpreadsheetDateSystem.Unknown, True, source.DateSerialOrigin)`. This opt-in requires a midnight origin and defines one serial unit as one day; integer and fractional, positive and negative day offsets are supported if the resulting date is representable. It **does not** guess an origin, detect dates from number formats, or reinterpret an unknown date serial as Excel1900. Providing *both* a known Excel basis and an explicit origin is rejected for date conversion. Adapters must first verify their own null-date settings and serial-unit conventions; if unavailable, leave the origin `Nothing` and treat the original cell as a number until a date-typed operation requests verified conversion. Date time-zone normalization remains an adapter responsibility.

## Error semantics

The existing `SpreadsheetCellKind.CellError` remains distinct from `Text` (e.g. a literal `#N/A`). `SpreadsheetCellValue.ErrorCell(rawText, nativeErrorCode)` optionally preserves a host-specific error identity. In `SpreadsheetCellEvaluation.Error`, `SpreadsheetExcelErrorValue.RawText` preserves the original display, `NativeCode` retains that opaque identity, and `Code` is populated **only** for recognized Excel error spellings; unknown spellings return `String.Empty`, never an invented Excel code. Existing one-argument `ErrorCell(rawText)` and `SpreadsheetExcelErrorValue(rawText)` signatures remain available. The existing Reject/Preserve/TreatAsMissing policy still applies, and Core input handling does not infer error types from arbitrary text.

## Compatibility and next batches

No Core algorithms, analysis IDs, `methods.yml`, Excel-DNA application wiring, Office.js bridge, or Excel 1900/1904 arithmetic changed. The original `SpreadsheetCellValuePolicy.Evaluate` signature remains available (with an additive explicit-origin overload). Stage 3.04 goldens are deliberately not regenerated.

**01.06–01.08:** use typed source values, range coordinates, row identities and verified date semantics; never invoke A1 parsing for a `HostNative` reference. **01.09–01.10:** adapters perform host-native selection/acquisition, range validation, and host-dependent quotas. **02.xx:** host-native output error/style mapping belongs in writers, not statistical routines.

This work introduces extension points but **not** a UNO, Python, Python.NET, or LibreOffice bridge.
