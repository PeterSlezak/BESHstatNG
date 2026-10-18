# Modern consumers: .NET 10 migration (Stage A)

Validated before batch 01.09 adapter edits on `codex/01-09`.

## Changes

- Core.Tests and Excel.Common.Tests: `net8.0` to `net10.0`.
- Core.Tests: consolidated duplicate property groups, retaining language,
  implicit imports, nullable and runsettings settings.
- WasmSmoke: `net9.0` to `net10.0`; WebAssembly and DevServer packages
  `9.0.20` to matching stable `10.0.13`.
- MSTest `3.6.4` and Microsoft.NET.Test.Sdk `17.12.0` retained: both suites
  execute successfully on .NET 10 without a package upgrade.
- Core/Common remain `netstandard2.0`; Windows host/tests remain .NET Framework
  4.8. Installer targets and historical batch documents remain unchanged.

Package references: [WebAssembly 10.0.13](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.WebAssembly/10.0.13)
and [DevServer 10.0.13](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.WebAssembly.DevServer/10.0.13).

## Validation gate

Installed SDK `10.0.402`, runtime `10.0.13`, existing `wasm-tools` manifest
`10.0.113/10.0.100`. No workload, machine dependency or test package installation
was needed. Initial cache-only restores/builds succeeded using
`-p:RestoreSources=C:/Users/peter/.nuget/packages -p:NuGetAudit=false`.
An initial `dotnet test --source ...` invocation was rejected (MSB1001);
the corrected commands use the MSBuild `RestoreSources` property.

Gate command (exit 0):

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Js -Wasm -Legacy -VisualStudio 2026 -PythonExe "C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe"
```

Core **1,024**, Common **231**, legacy **542**, Office.js **29** passed;
zero failures/skips. WasmSmoke and VS 2026 legacy Debug build succeeded with
zero warnings/errors, including both 32/64-bit Excel-DNA packages. Python
help generation executed; the Python regression suites are reserved for the
full final command. Log: `TestResults/Codex/stage-a-validation.log`.

Generated help timestamps changed during the build. Their generated versions
are preserved under ignored `TestResults/Codex`, and original working copies
are restored from byte copies after comparison. VersionAttributes was unchanged.
The existing help edits and both stashes are retained.

Manual follow-up: open the existing WASM smoke workflow and run ANOVA/Kruskal;
on Windows run the existing Ribbon analyses and inspect output tables. No live
Excel/browser, signing, installer or publishing workflow was launched.

Review this migration separately from batch 01.09; no commits were made.
