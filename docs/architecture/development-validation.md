# Development validation before batch 01.08

Core/Common remain `netstandard2.0`; portable MSTest projects run on `net8.0`.
The Windows host and legacy tests remain .NET Framework 4.8. No 01.08 parser,
production Office UI, framework migration or LibreOffice adapter is included.

## Reproducing validation from a checkout

Prerequisites: a suitable .NET SDK/runtime (including .NET 8 for portable tests),
Node 20+ for JS tests, and Python 3.10+ with PyYAML for generators and Python tests.
WASM requires the existing .NET 9 dependencies and a compatible WASM workload.
Windows legacy builds require VS 2022 or 2026, the .NET Framework 4.8 targeting
pack and the existing `packages.config` packages restored. The script does not
install Python packages, workloads, Visual Studio or legacy packages.

```powershell
# Existing default and TRX paths are preserved.
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1

# Optional checks; specify an executable path, not a command plus arguments.
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Python -PythonExe C:\Python311\python.exe -Js -Wasm
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Legacy -VisualStudio 2026 -PythonExe C:\Python311\python.exe
```

`-Python` runs the frozen Stage 3.04 inventory check, authored method validator,
generated metadata `--check`, retired-helper audit, Windows tooling unittests,
MkDocs/UDF documentation unittests and Python discovery regression tests.
The generated metadata check does not rewrite its VB output. Python tests use
temporary directories for negative cases and help/UDF generation.

`Core.trx`, `Common.trx` and optional `Legacy.trx` retain their existing names in
`TestResults/Codex`. Repeated runs overwrite these reports. `-Wasm` builds only;
it does not demonstrate browser or Excel integration. `-Js` runs the existing
dependency-free mocked Office bridge tests.

## Python discovery and help generation

The shared `scripts/resolve-beshstatng-python.ps1` accepts an explicit `PythonExe`
(also inherited from the environment). Otherwise it probes `python`, `python3`,
then `py -3`, returning the actual interpreter path. It requires Python 3.10+
and an importable PyYAML, skips interactive Windows Store aliases, and fails with
an actionable message. An invalid explicit override fails without fallback.
No machine-specific interpreter location is stored in the project.

For direct MSBuild/Visual Studio use, set `/p:PythonExe=...` or the `PythonExe`
environment variable before starting Visual Studio. The existing Windows
`GenerateHelpLinks` target still generates `HelpLinks.vb` and `HelpContext.vb`
before build using the authored `methods.yml`. Both VS toolchains use Windows
PowerShell for discovery. Paths containing spaces must be quoted at the caller.

## Source files exposed by narrow Git exceptions

Only `docs/architecture/*.md`, the three named Stage 3.04 inventory CSVs, the
two named manual-smoke CSVs, `tools/verify_stage304_inventory.py`, and the seven
named files in `BESHstatNG/tools` and its tests are exposed. These preserve
existing batch contracts, frozen compatibility inputs, generator dependencies
and regression tooling. Other ignored docs/tools content stays ignored.
`__pycache__`, bytecode and local Python environments remain ignored. Existing
previously tracked artifacts are not removed by this infrastructure change.

## Legacy generation and pre-push review

Before legacy validation, preserve byte-for-byte copies of
`BESHstatNG/Generated/VersionAttributes.vb`, `BESHstatNG/src/Help/HelpLinks.vb`
and `BESHstatNG/src/Help/HelpContext.vb`, including any user edits. The Windows
build regenerates these files. Record its diff and preserve generated copies
under ignored `TestResults` before restoring only files that this run changed
from their captured state. Never use a broad reset or discard unrelated edits.
Debug validation does not invoke the PreRelease signing routine.

`.githooks/pre-push` is an optional Git for Windows hook which runs only the
default portable checks and blocks pushes on failure. Merely tracking it does
not activate it: `core.hooksPath` must be configured separately. This change
does not enable the hook, change Git ownership exceptions or push anything.
Python/JS/WASM/legacy coverage must be requested through the verification script.

Manual checks remain separate: Windows Ribbon ANOVA/Kruskal output to current
and new worksheets; Office/WASM selected-range analysis to a new worksheet.
No live Excel, installer or publishing workflow is launched by these checks.
