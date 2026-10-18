# BESHStat.WasmSmoke — experimental WebAssembly/Office.js harness

`BESHStat.WasmSmoke` is a **Blazor WebAssembly experiment**, not a released Excel add-in. It targets **.NET 10**, references [`BESHStat.Core`](../BESHStat.Core/README.md) and [`BESHStat.Excel.Common`](../BESHStat.Excel.Common/README.md), and exercises statistical routines and selected Excel range/output operations through an Office.js JavaScript bridge.

This project is deliberately a small regression/proof-of-concept host. Future production-oriented Excel Online and Mac support is a development goal, **not a currently supported distribution**.

## Main files

- `Pages/Home.razor` — analysis/probe UI.
- `wwwroot/js/officeBridge.js` — Excel host detection, range reads, output writing and capability checks.
- `office-manifest.xml` — **development** task-pane manifest with an HTTPS localhost URL; not a production manifest.
- `tests/*.test.cjs` — Node-based JavaScript tests of host capabilities, typed cell reads, workbook identity and safe range acquisition.
- `package.json` — dependency-free `npm test` and JavaScript syntax-check commands.

## Build and test

From the repository root:

```powershell
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
cd BESHStat.WasmSmoke
node --version
npm test
```

The `.cjs` files are **Node.js CommonJS test files**, not C# or VB MSTest projects, so Visual Studio Test Explorer does not discover them by default. In Visual Studio, open **View → Terminal**, switch to the `BESHStat.WasmSmoke` directory and run `npm test`. This command checks `officeBridge.js` syntax and runs all `.test.cjs` files using Node's built-in runner; no `npm install` is required. Alternatively, from the solution root, run:

```powershell
node --check BESHStat.WasmSmoke/wwwroot/js/officeBridge.js
cd BESHStat.WasmSmoke
node --test
```

The Excel source reader checks selection dimensions **before** acquiring cell values. Entire Excel rows/columns are narrowed to the populated cells **within the selection**. Explicitly bounded ranges remain unchanged, including their blank edges. A temporary **10,000-cell limit** avoids oversized Office.js payloads in this smoke harness. Datasets beyond that limit are rejected with a clear message; real chunked acquisition belongs to the later input-size/batching work, not this prototype correction.

The bridge also exposes `getSelectedRangeSetData()` for noncontiguous selections
on ExcelApi 1.9 hosts and `getRangeSetData({ selectionId, ranges })` for explicit
ordered rectangles, including different worksheets. Each area remains separate;
the current task pane continues using its existing single-range flow. Older hosts
use the single selected-range fallback, which rejects multiple selected areas.
Snapshots use canonical A1 endpoints (`C4:C4` for one cell); request coordinates
and worksheet identities are checked, and unverified selection revisions fail.
Office date basis and native error codes remain unknown when the reader cannot
obtain them. See [batch 01.09](../docs/architecture/batch-01-09-acquisition-adapters.md)
for Windows/Office fixture parity and manual two-range acceptance.

For an interactive test, start the project using its configured **HTTPS** development profile and confirm that its URL matches `office-manifest.xml` (the checked-in manifest uses `https://localhost:7131/`). After trusting the local development certificate, sideload the manifest in a supported Excel test environment. Verify host detection, reading selected data and output to current/new worksheets. **New Workbook is not supported by the present Office.js bridge** and is capability-gated.

Do not treat a successful browser calculation as evidence of Mac Excel, production deployment, or full Windows feature parity. See the [main README](../README.md).
