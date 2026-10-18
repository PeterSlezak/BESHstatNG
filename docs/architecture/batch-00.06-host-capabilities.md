# Batch 00.06 — Host capabilities and preflight (15 October 2026)

**Source baseline:** `BESHstatNG(20261015-170501).zip` — developer-validated through Batch 00.05.

## Scope and architectural boundary

The existing shared application catalog, typed registry, numerical algorithms, result documents, output tables and 00.01 golden fixtures remain unchanged. Batch 00.06 introduces an *explicit, host-neutral declaration of host operations*, and a side-effect-free preflight policy. It changes only host entry points and adds tests. It does **not** implement new-workbook output in Office.js or the Phase 02 output planner.

| Layer | Role |
|---|---|
| `BESHStat.Excel.Common/src/Host/HostCapabilities.vb` | Stable operation IDs, Office.js-serializable capability snapshot, Windows/test profile, preflight result/code/exception |
| `BESHstatNG/src/DM/ExcelDnaHostCapabilities.vb` | Windows Excel-DNA profile; validates shared output targets before writing; **does not** change the existing COM workbook/sheet-selection logic |
| `BESHstatNG/src/UI/UibyID.vb` | Uses Windows preflight for existing shared ANOVA and KW output destinations; no numerical change |
| `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js` | Detects Excel host and ExcelApi 1.1; advertises only actual writer destinations; defensively rejects NewWorkbook/invalid destinations before any `Excel.run` call |
| `BESHStat.WasmSmoke/Pages/Home.razor` | Uses the runtime snapshot to disable unsupported destinations and Run/Read buttons; preflights **before selecting data or computing** |
| `BESHStat.Excel.Common.Tests/src/HostCapabilitiesTests.vb` | Common rules, failure codes, destination choices, JSON round-trip and fail-closed behavior |
| `BESHStat.WasmSmoke/tests/hostCapabilities.test.cjs` | Node built-in test suite with mocked Office.js; no NuGet/npm dependencies |

## Supported capability IDs

- `input.read-selected-range` — selected-range acquisition
- `output.write-result-tables` — writing a result-table batch
- `output.current-worksheet` — write to current worksheet
- `output.new-worksheet` — create and write to a new worksheet
- `output.new-workbook` — create and write to a new workbook

Current host matrix:

| Feature | Excel-DNA Windows | WasmSmoke Office.js (ExcelApi >= 1.1) |
|---|---|---|
| Read selected range | Yes | Yes |
| Write table batch | Yes | Yes |
| Current worksheet | Yes | Yes |
| New worksheet | Yes | Yes |
| New workbook | Yes | **No** |

Office capabilities are queried **at runtime** from `beshOffice.getHostCapabilities()`. Its feature set is empty when the page is not in Excel, `Excel.run` is missing, or ExcelApi 1.1 is unavailable or cannot be verified. A separate `getHostStatus()` remains backward compatible. `getHostCapabilities()` returns `host`, `platform`, `isExcel` and `capabilities`, compatible with the Common `HostCapabilitySnapshot` DTO.

### Preflight semantics

`HostCapabilityPreflight.CheckSelection`, `CheckAnalysis`, `CheckOutput` and `CheckRunAndWrite` are pure checks; they do not normalize an output target, change the workbook, call Core statistics or mutate host state. Results include `IsSupported`, `Code`, `Message` and `MissingCapability`. Call `EnsureSupported()` only at a host boundary where a `HostCapabilityException` (`NotSupportedException`) is appropriate.

- `host.unavailable` — not in an available Excel host
- `analysis.unknown` / `analysis.unsupported` — unregistered/unimplemented analysis
- `host.capability-missing` — operation absent (e.g. reading selection)
- `output.destination-unsupported` — destination absent (notably NewWorkbook in Office.js)
- `output.target-invalid` — invalid destination or nonpositive start-cell coordinates

For pure Core analyses, `AnalysisCatalog.RequiredCapabilities` remains an empty list: an analysis can compute from already validated in-memory input without Excel. The host operation set is checked separately when a UI *reads the selection and writes output*. `CheckRunAndWrite(..., readsSelection:=False)` supports already-acquired data in Windows. When future analyses genuinely require particular host services, those requirements should be added to their descriptors.

**Safety limitation:** This batch is a *capability* preflight, not an overwrite, protected-range, active-workbook/revision or chart-API preflight. Those concerns are planned for Phase 01/02. Windows forms still initialize the existing writer and run the computation before validating shared output; all three of their current destinations are supported. The Office.js prototype performs capability preflight before either acquiring input or executing statistical code.

### Why New Workbook remains visible but unavailable

WasmSmoke now renders a disabled “New workbook (unavailable in Office.js)” option instead of silently hiding the shared destination. The Common output enum remains unchanged. Unsupported direct JavaScript write requests throw before `Excel.run`, even when someone bypasses the UI.

## Apply

Apply the unified patch at solution root, **or** copy every file from the replacement ZIP over the relative destination. No files are deleted in this batch. Windows `BESHStatNG.vbproj` explicitly includes the new `.vb` adapter. SDK-style Common and WASM projects discover their new files automatically. Do not apply both artifacts.

## Regression checks

```powershell
# Run from the solution root.
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
python -m unittest discover -s BESHstatNG/tools/tests -p "test_*.py" -v
python -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p "test_*.py" -v

# Requires Node.js 18+; no npm install necessary.
node --test BESHStat.WasmSmoke/tests/hostCapabilities.test.cjs

dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
```

Also build the complete Windows solution and run the legacy tests. Numerical and output metadata expectations remain the Stage 3.04 goldens.

**Manual Office.js smoke:** Open WasmSmoke in Excel Online, confirm current/new worksheet are selectable and NewWorkbook is visible but disabled. Read a valid grouped range and run ANOVA (Welch) or KW (Dunn) into a new worksheet; then retry current worksheet as normal. Open the site outside Excel: Read/Run must remain disabled. If the host lacks the required Excel API, no write operation should be offered.

**Manual Windows smoke:** Run one-way ANOVA/Welch and KW/Dunn through the existing Excel-DNA dialog, including output to a new worksheet; optionally test New Workbook in Windows. Verify the same cell placement, p-value marking, and table order as the approved baseline.

## Rollback / next batch

Rollback consists of reverting the patch or replacing the modified files from the ZIP baseline (and removing three new source/test files). The next dependent work is Phase 01 input contracts and Phase 02 output semantics; the Phase 00 plan marks 00.06 as complete only after .NET and host smoke tests pass.
