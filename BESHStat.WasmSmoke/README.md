# BESHStat.WasmSmoke — independent WebAssembly/Office.js regression harness

`BESHStat.WasmSmoke` is a **.NET 10 Blazor WebAssembly proof-of-compatibility application**. It executes real `BESHStat.Core` calculations under WASM and exercises selected spreadsheet acquisition/output paths via Office.js and `BESHStat.Excel.Common`. It deliberately remains separate from the production-oriented [`BESHStat.OfficeAddin`](../BESHStat.OfficeAddin/README.md).

This project is **not** a released Excel Online/Mac add-in and does not supply the main production-oriented Ribbon or Options UI. It is useful for catching WebAssembly runtime regressions and testing the original Office.js bridge independently of the newer host.

## Key files and behavior

- `Pages/Home.razor` presents the manual calculation/Office bridge probes.
- `wwwroot/js/officeBridge.js` contains the smoke bridge for selected range acquisition, host capability checks and supported output paths.
- `office-manifest.xml` is a **local development** manifest, normally pointing to `https://localhost:7131/`.
- `tests/*.test.cjs` and `package.json` hold dependency-free Node tests and syntax checks.

The bridge supports bounded typed cell snapshots and selected-area validation. It does not guarantee every workbook destination or unlimited-size import. It must report unsupported operations explicitly rather than guessing, and it must not claim Mac Excel validation based on a standalone browser run.

## Build and test

```powershell
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
cd BESHStat.WasmSmoke
npm test
```

Node's built-in test runner executes `.test.cjs` tests; these do not appear in MSTest Test Explorer. For manual Office testing, use the project's own HTTPS launch profile, trust its local certificate, sideload `office-manifest.xml` in a disposable workbook and verify explicit read/write actions. Keep this harness on **port 7131**, distinct from OfficeAddin's **7141**.

## Future maintenance

Keep this project intentionally small: statistical/WASM compatibility, bridge regression fixtures and early interoperability experiments. The production-oriented four-tab workspace, capability-aware Ribbon, read-only model builders, persisted Options and guarded ANOVA/KW writer belong in `BESHStat.OfficeAddin`. Maintain separate manifests/ports and run both hosts during cross-platform regression work.
