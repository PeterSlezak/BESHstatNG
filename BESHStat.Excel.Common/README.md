# BESHStat.Excel.Common — shared Excel application layer

`BESHStat.Excel.Common` provides host-neutral spreadsheet-facing contracts above [`BESHStat.Core`](../BESHStat.Core/README.md). It targets **.NET Standard 2.0** so the Windows add-in and future Office.js/WASM hosts can reuse spreadsheet analysis rules without duplicating the statistics.

## Responsibilities

The present shared layer includes spreadsheet cell/range and output-target DTOs, grouped-numeric parsing, typed scalar/vector/paired numeric input adapters, immutable rectangular frames, categorical vectors and contingency tables, a small typed analysis catalog/handler registry, analysis request/result contracts, ordered result tables, metadata and fault/warning artifacts, result-write requests, and host-capability/preflight contracts. This is a growing application layer, **not** a complete cross-platform copy of every Windows analysis.

The supported shared analysis vertical slices include one-way ANOVA and Kruskal–Wallis. Descriptive/help metadata for additional methods are not automatically executable handlers. Core owns statistical algorithms; this project owns interpretation/orchestration and host-neutral Excel semantics; host adapters perform actual Excel reads/writes.

Numeric input parsing is diagnostics-first: Phase 01.04 offers `TypedNumericRangeParsers.ParseScalar`, `ParseVector`, `ParsePaired` and `ParseGrouped` with explicit cell/missing/header policies. See [Batch 01.04 architecture notes](../docs/architecture/batch-01.04-typed-numeric-inputs.md). The older grouped-parser entry point remains unchanged. Batch 01.05 adds `RectangularCategoricalParsers.ParseDataFrame`, `ParseCategoricalVector`, `ParseContingencyCounts` and `ParseContingencyPairs`, with type-tagged categories, strict count validation and structured diagnostics. See [Batch 01.05 architecture notes](../docs/architecture/batch-01.05-rectangular-categories-contingency.md).

## Build and tests

```powershell
dotnet build BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj -c Release
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
```

Keep this assembly free of Office.js browser objects, Excel COM/Interop, Windows Forms and Excel-DNA. Stable runtime IDs and public metadata are reconciled with the documentation method catalog by repository generator/validation scripts.

**Status:** active cross-platform development infrastructure, not an independently installable Excel product. See the [main README](../README.md).
