# Repository verification and development scripts

This folder contains cross-project verification and Python-toolchain discovery helpers. Run scripts from the **repository root** with PowerShell on a supported development machine.

## Main commands

```powershell
# Portable validation and standard regression checks
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1

# Extended Windows/Office/JS/Python/WASM checks with installed prerequisites
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Release -VisualStudio 2026 -Legacy -Wasm -Js -Python

# Production-oriented Office WebAssembly host and test assets
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
```

`verify-beshstatng.ps1` checks Core/Common and optionally Windows legacy tests, WASM builds, Office JavaScript and Python generator/test suites. Use a valid Python 3.10+ executable with PyYAML; `resolve-beshstatng-python.ps1` discovers a suitable interpreter or respects an explicit `-PythonExe` override. These scripts do not install Python packages, Visual Studio workloads or Git hooks.

`verify-office-foundation.ps1` checks the development Office add-in build and supported C#/JS/manifest/static asset tests. It does not sign/deploy a manifest or drive an authenticated Excel workbook. Browser simulations require separate opt-in setup. A dedicated, opt-in authenticated Edge integration runner exists under `BESHStat.OfficeAddin/tests/browser/live-edge-workflow.mjs`; it needs a disposable authorized Excel Online workbook and is **not** included in simulated pass counts. Real Excel Online, Windows desktop and Mac acceptance must be reported separately.

Run current repository-wide Debug and Release gates before accepting shared-contract changes, Ribbon/manifest changes or portable Windows migrations. During ongoing graphics extraction, preserve original test identities, source-hash pins and nonpassing receipts; a successful isolated/serial retry does not erase a failed concurrent attempt. The latest accepted migration dossier reports **14 inherited managed failures per configuration** and qualified Python nonpasses; do not represent those suites as globally green. Publication manifests and historical source-preservation checks are maintained with each accepted batch in `evidence/`, without rehashing the large historical `TestResults` payload. Generated `TestResults` and TRX outputs are evidence, not authoritative project metadata. Read [`../docs/architecture/development-validation.md`](../docs/architecture/development-validation.md) for details and limitations.
