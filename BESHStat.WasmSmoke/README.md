# BESHStat.WasmSmoke — experimental WebAssembly/Office.js harness

`BESHStat.WasmSmoke` is a **Blazor WebAssembly experiment**, not a released Excel add-in. It targets **.NET 9**, references [`BESHStat.Core`](../BESHStat.Core/README.md) and [`BESHStat.Excel.Common`](../BESHStat.Excel.Common/README.md), and exercises statistical routines and selected Excel range/output operations through an Office.js JavaScript bridge.

This project is deliberately a small regression/proof-of-concept host. Future production-oriented Excel Online and Mac support is a development goal, **not a currently supported distribution**.

## Main files

- `Pages/Home.razor` — analysis/probe UI.
- `wwwroot/js/officeBridge.js` — Excel host detection, range reads, output writing and capability checks.
- `office-manifest.xml` — **development** task-pane manifest with an HTTPS localhost URL; not a production manifest.
- `tests/hostCapabilities.test.cjs` — Node-based JavaScript tests of host capability behavior.

## Build and test

From the repository root:

```powershell
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
node --test BESHStat.WasmSmoke/tests/hostCapabilities.test.cjs
```

For an interactive test, start the project using its configured **HTTPS** development profile and confirm that its URL matches `office-manifest.xml` (the checked-in manifest uses `https://localhost:7131/`). After trusting the local development certificate, sideload the manifest in a supported Excel test environment. Verify host detection, reading selected data and output to current/new worksheets. **New Workbook is not supported by the present Office.js bridge** and is capability-gated.

Do not treat a successful browser calculation as evidence of Mac Excel, production deployment, or full Windows feature parity. See the [main README](../README.md).
