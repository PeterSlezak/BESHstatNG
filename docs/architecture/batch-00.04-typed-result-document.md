# Batch 00.04 — Typed analysis result envelope, faults and warnings

**Source checkpoint:** `BESHstatNG(20261015-161047).zip` (00.03 passed by developer).
**Scope:** BESHStat.Excel.Common result contracts, handler result projection and Common tests.
**Status:** Implementation patch; developer acceptance/build still required.

## Source audit

- `AnalysisHandlerRegistry` has four registered input-shape handlers for two executable analyses, corresponding to `AnalysisCatalog` and the YAML-derived stable analysis IDs.
- `SpreadsheetAnalysisResult` exposes mutable `List(Of ResultTableOutputModel)`, `Dictionary(Of String, Double)` and `Detail As Object`; Windows and WasmSmoke consume these directly.
- Existing ANOVA and Kruskal application services produce ordered Core `ResultTableOutputModel` tables and typed detail classes. Host-specific output placement and p-value presentation remain outside the Core computation.
- All five Stage 3.04 golden fixtures and 00.01–00.03 tests are left unchanged.

## Files

- **New:** `BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb`: v1 `AnalysisResultDocument`, ordered `AnalysisTableArtifact`/`AnalysisMetricArtifact`/`AnalysisDetailArtifact`, typed detail getter, structured `AnalysisWarning`/`AnalysisFault`, status, and JSON-friendly **structural** `ToContractSnapshot` DTO.
- **Changed:** `BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb`: extends the existing `SpreadsheetAnalysisResult` (retaining its original four-argument CLR constructor overload) with the `Document` property, and adds option-warning reporting to existing option classes.
- **Changed:** `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`: adds warnings for out-of-range/nonfinite alpha that existing algorithms already silently replace with 0.05. Execution stays with existing typed application services.
- **Changed:** `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`: opt-in `RunDocument(id, input, options)` returns structured faults for known request/unsupported/execution failures. Existing `Run(...)` overloads **continue throwing** as before.
- **New:** `BESHStat.Excel.Common.Tests/src/AnalysisResultDocumentTests.vb`: nine tests; success, all five fixtures, warnings, failure, immutability, typed detail, snapshot round-trip and invalid requests.

## Public contract / compatibility rules

1. The original four-argument `SpreadsheetAnalysisResult` constructor remains a distinct CLR overload for binary compatibility. The legacy `Tables`, `Metrics`, `Detail` and `PrimaryTable` properties, enum values, analysis IDs, handler definitions, option defaults and statistical calculations are unchanged. Existing Windows and Office consumers need no edits.
2. `result.Document.AnalysisId` uses the stable ID, `.Status` is `Succeeded` or `Failed`, `.Tables` is in exactly the original statistical table order, `.Metrics` is sorted by invariant ordinal key, and `.Artifacts` contains tables, metrics and (if present) one typed-detail artifact, in that order.
3. A table artifact defensively copies its source `Object(,)` matrix and p-value lists. The legacy `ResultTableOutputModel` is still mutable for existing callers. Typed details are **typed references** (they are not deep-cloned and do not participate in the structural snapshot).
4. `TryGetDetail(Of T)(ByRef detail)` returns a detail only when its runtime type is compatible; incompatible types return `False` and `Nothing`. The former `Detail As Object` remains for binary/source compatibility during migration.
5. Only established p-value keys are assigned the `PValue` role; all others are `Statistic`. Future typed metric metadata is added by analysis handlers, not inferred by arbitrary name suffixes.
6. Invalid alpha in either existing options class uses the **same** fallback 0.05 as before, with one new `options.alpha.defaulted` warning. Valid/default alpha adds no warning.
7. `RunDocument(...)` maps expected `ArgumentException` to `analysis.invalid-request`, `NotSupportedException` to `analysis.unsupported`, and `InvalidOperationException` to `analysis.execution-failed`. Messages are deliberately generic to avoid leaking worksheet values. Unexpected programming exceptions are **not** caught. Existing `Run(...)` exceptions are unchanged.
8. `ToContractSnapshot()` is a **v1 JSON-friendly structural snapshot**, not full result-table output serialization. It includes table dimensions/headers/footer, all explicit p-value positions, scalar metrics (with explicit NaN/infinity classification), typed-detail identity, warnings and faults. It does **not** serialize raw worksheet/result-cell contents or the analysis-specific detail object. This boundary is deliberately postponed to Phase 02's `OutputDocument` / `OutputCell` contracts.
9. `AnalysisResultDocument` has read-only artifact/issue collections. Result-table contents are copied when entering and leaving artifacts. Snapshot DTOs are mutable transport types and should not be mistaken for full result documents.
10. No source dependency on WinForms, COM, Excel-DNA, Office.js, dynamic invocation, reflection-based dispatch or a JSON library is added to Common.

## Example: legacy and new usage

```vb
' Existing code (unchanged):
Dim result As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(
    AnalysisIds.OneWayAnova, grouped, options)
Dim legacyTable As Global.BESHStatNG.ResultTableOutputModel = result.PrimaryTable

' New typed document (optional):
Dim report As AnalysisResultDocument = result.Document
Dim detail As OneWayAnovaApplicationResult = Nothing
If report.TryGetDetail(Of OneWayAnovaApplicationResult)(detail) Then
    ' detail is strongly typed
End If
For Each warning As AnalysisWarning In report.Warnings
    ' warning.Code / warning.FieldPath / warning.Message
Next

' Opt-in structured failure result:
Dim safeReport As AnalysisResultDocument = SharedAnalysisApplicationService.RunDocument(
    AnalysisIds.OneWayAnova, grouped, options)
If safeReport.Status = AnalysisDocumentStatus.Failed Then
    ' Examine safeReport.Faults; no exception from known request faults.
End If
```

## Local acceptance

Build the full solution and execute Core/Common/legacy suites, including the nine new Common tests:

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
```

Re-run Windows Excel-DNA one-way ANOVA with Welch/Tukey–Kramer and a second run with all available post-hoc choices. In WasmSmoke/Excel Online run Kruskal–Wallis with Dunn. Verify table order, headline metrics, p-value formatting, destinations and help remain as in the Stage 3.04 fixtures. The new document API is not required by either host yet.

**Environment limitation:** Only static checks and Python tests are runnable in the patch-authoring environment; developer must run MSBuild/MSTest/Excel.

**Rollback:** revert the changed Common files and remove the new `Results/AnalysisResultDocument.vb` and Common test. No migrations or changed Excel files are involved.
**Next batch:** 00.05 — transitional Windows `OfficeJsDataImporter.vb` / `OfficeJsResultWriter.vb` compatibility audit.
