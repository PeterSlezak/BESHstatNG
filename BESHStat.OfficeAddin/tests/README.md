# Office add-in test suites

This folder validates the development [`BESHStat.OfficeAddin`](../README.md) through several complementary layers:

- `OfficeAddin.Tests.csproj` and the C# test sources exercise linked production services, lifecycle contracts, navigation, Razor-component bindings, analysis requests and guarded output behavior on **.NET 10**.
- `*.test.mjs` use Node's built-in test runner to exercise Office JavaScript adapters, diagnostics, workbook acquisition and production writer/consent paths.
- `browser/*.mjs` are **opt-in** isolated-Edge scenarios running real WASM/Blazor with simulated Office interfaces, not an authenticated Excel workbook.

From the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
```

Or in this project's parent directory run `npm test`; C# tests can be executed with `dotnet test BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj -c Debug`. Browser automation requires a running HTTPS development host on 7141 and an isolated Edge debugging session; follow the parent README and architecture notes rather than using a personal browser profile.

Acceptance labels matter: unit/component tests and synthetic browser fixtures can show safe wiring but do not prove Excel Online/desktop compatibility, writable workbook behavior or Mac support. Record manual host evidence separately.
