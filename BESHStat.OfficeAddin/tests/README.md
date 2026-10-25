# Office add-in test suites

This folder validates the development [`BESHStat.OfficeAddin`](../README.md) through several complementary layers:

- `OfficeAddin.Tests.csproj` and the C# test sources exercise linked production services, lifecycle contracts, navigation, Razor-component bindings, specification previews, global options/locale changes, Ribbon dispatch and guarded output behavior on **.NET 10**.
- `*.test.mjs` use Node's built-in test runner to exercise Office JavaScript adapters, diagnostics, workbook/whole-column acquisition, physical-row matching, persisted preferences, command routing, presentation readback and production writer/consent paths.
- `browser/*.mjs` are **opt-in** isolated-Edge scenarios running real WASM/Blazor with simulated Office interfaces, not an authenticated Excel workbook. The suite includes simulations for the four-tab workspace, specification builders, settings, Help and Ribbon navigation.

From the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
```

Or in this project's parent directory run `npm test`; C# tests can be executed with `dotnet test BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj -c Debug`. Browser automation requires a running HTTPS development host on 7141 and an isolated Edge debugging session; follow the parent README and architecture notes rather than using a personal browser profile.

For an **authorized, genuine Edge/Excel Online** session and disposable workbook, `browser/live-edge-workflow.mjs` can automate the occupied-output → option-change → fresh-overwrite-consent workflow. This is an opt-in integration test requiring manual authentication/sideloading and the local Office host; it is not part of `npm test` or the synthetic browser suite.

```powershell
# Use only after preparing an explicitly authorized disposable Excel Online session
node BESHStat.OfficeAddin/tests/browser/live-edge-workflow.mjs my-run-label
```

**Acceptance labels matter:** unit/component tests and synthetic browser fixtures can show safe wiring but do not prove Excel Online/desktop compatibility, writable workbook behavior or Mac support. Record manual host evidence separately.
