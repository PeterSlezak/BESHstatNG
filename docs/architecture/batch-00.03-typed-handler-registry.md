# BESHStatNG Cross-Platform — Batch 00.03: Typed Analysis Handler Registry

**Patch baseline:** `BESHstatNG(20261015-154612).zip` (00.02B and the subsequent test corrections incorporated).
**Status:** implementation ready for local Visual Studio and Excel validation.
**Depends on:** approved 00.01, 00.02, 00.02A and 00.02B.
**Next:** 00.04 — typed result artifacts, diagnostics and faults.

## What changed

- Added `BESHStat.Excel.Common/src/Analysis/IAnalysisHandler.vb`: a non-generic execution boundary and generic `IAnalysisHandler(Of TInput,TOptions,TDetail)` plus a reusable strongly typed adapter. All new VB files use `Option Strict On`.
- Added `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`: explicit and immutable registrations for **two analysis IDs × two input representations** (a spreadsheet range and already-parsed grouped numeric data). Descriptor input/options contracts and registration completeness are checked at startup, without reflection, `Object`-based invocation, or late binding.
- Changed `SharedAnalysisApplicationService.Run(request)` to resolve the legacy enum through `AnalysisCatalog` and invoke the registry. Also added `Run(analysisId As String, input As SpreadsheetAnalysisInput, Optional options As SpreadsheetAnalysisOptions = Nothing)` for stable-ID callers.
- Changed `BESHStat.WasmSmoke/Pages/Home.razor` to dispatch by stable ID (looked up from the already catalog-driven selection), while Windows Excel-DNA retains its original enum/request API. Both paths now share the same handler registry.
- Updated the obsolete 00.02 remarks in `AnalysisCatalog.vb`.
- Added `BESHStat.Excel.Common.Tests/src/AnalysisHandlerRegistryTests.vb`: registration coverage, option/input rejection, unknown IDs, readonly inventory, five 00.01 fixture parity cases, both data shapes, null-option defaults and direct-vs-legacy output equivalence.

## Current contracts

| Analysis ID | Legacy enum | Input shapes | Options type | Detail type |
|---|---|---|---|---|
| `anova.oneway` | `OneWayAnova=1` | `SpreadsheetRangeData`, `GroupedNumericData` | `OneWayAnovaAnalysisOptions` | `OneWayAnovaApplicationResult` |
| `nonparametric.kruskal-wallis` | `KruskalWallis=2` | `SpreadsheetRangeData`, `GroupedNumericData` | `KruskalWallisAnalysisOptions` | `KruskalWallisApplicationResult` |

**Compatibility:** `SpreadsheetAnalysisRequest` constructors/properties, `SpreadsheetAnalysisKind` values, `SpreadsheetAnalysisResult`, metric keys and output tables are unchanged. The existing application services still compute all the statistical results, and a missing options object still uses their previous per-analysis defaults. Wrong types and unsupported IDs are rejected **before** executing Core. Error messages/parameter names for wrong options may be more specific, while the legacy unsupported-enum exception is retained. `GeneratedMethodMetadata.vb` and `methods.yml` are **not modified**. Metadata-only methods do not register handlers.

**Important design boundary:** `SpreadsheetAnalysisResult.Detail As Object` is deliberately unchanged in this batch, as required by the 00.01 frozen ABI. The typed handler exposes a strongly typed detail at the calculation boundary; Batch 00.04 will introduce a type-safe result-artifact envelope. For now, `SpreadsheetAnalysisResult` still carries the historical enum, so future non-legacy analyses require 00.04 result schema design before activation.

## How to use

The existing Windows code can remain unchanged:

```vb
Dim result = SharedAnalysisApplicationService.Run(
    New SpreadsheetAnalysisRequest(SpreadsheetAnalysisKind.OneWayAnova, grouped, options))
```

A new consumer can call a stable ID:

```vb
Dim result = SharedAnalysisApplicationService.Run(AnalysisIds.OneWayAnova, grouped, options)
```

New analysis handlers must be registered explicitly with a concrete input, options and detail type and must satisfy the matching `AnalysisCatalog` descriptor. Do not add statistical calculations, reflection-based invocation, host API objects or YAML runtime loading to the registry.

## Test checklist

From the solution root in PowerShell:

```powershell
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
python -m unittest discover -s BESHstatNG/tools/tests -p "test_*.py" -v
python -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p "test_*.py" -v

dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
# Run the existing Windows legacy test project using the full Visual Studio solution.
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
```

Manual smoke:

1. **Windows Excel-DNA:** One-way ANOVA with Welch + Tukey–Kramer on the Stage 00.01 dataset, output to a new worksheet. Also run Kruskal–Wallis/Dunn. Verify tables, p-values, multi-table order and p-value formatting remain unchanged.
2. **Excel Online / WasmSmoke:** Choose ANOVA and then Kruskal–Wallis through the catalog dropdown; run both on the Stage 00.01 sample range. Confirm the stable-ID route writes the same tables in the selected destination, with no additional UI options or behavior changes.

**Acceptance gate:** all existing 00.01 golden snapshots, 00.02 catalog tests and 00.02B generated metadata tests pass; the new registry tests pass; Core + legacy unit tests and the two hosts' smoke checks pass. No host API or numerical change is expected.

## Environment verification

The Python YAML/Ribbon validator, metadata `--check`, and all existing Python tooling tests passed against the patched tree (15 solution-tool tests plus 5 documentation tests). The environment has no `dotnet` toolchain or Excel; **VB.NET builds, MSTest and Excel smoke tests were not executable here**, and the patch must be developer-validated before proceeding to 00.04.

## Rollback

Revert this patch or restore the 15 October baseline ZIP. No workbook schema, installer, generated metadata, UDF documentation, statistical engine or settings migration is involved.
