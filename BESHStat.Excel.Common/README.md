# BESHStat.Excel.Common — shared spreadsheet application services

`BESHStat.Excel.Common` is the **.NET Standard 2.0** application layer between [`BESHStat.Core`](../BESHStat.Core/README.md) and spreadsheet hosts. It is shared by the existing .NET Framework 4.8 Excel-DNA add-in and the .NET 10 Office.js/WebAssembly development application. It is a library, not an installable add-in.

## What is implemented

- **Source contracts:** workbook, worksheet, range and selection identity; normalized cell types; explicit error, missing-value and date policies; bounded acquisition limits and host capability/preflight metadata.
- **Typed inputs and validation:** scalar/vector/paired/grouped data, rectangular and categorical frames, contingency tables, survival/subject/repeated-measures data, formulas and model inputs, matrix/classification/PSM/SPC/chart-source inputs. Parsing emits structured diagnostics rather than host exceptions.
- **Analysis orchestration:** stable analysis identifiers, a typed descriptor/handler registry, strongly typed options, validated requests and safe result artifacts. The currently Office-runnable shared handlers include **One-Way ANOVA** and **Kruskal–Wallis**, with supported post-hoc options; current browser input supports wide/long forms and explicit reference/row provenance. Metadata for other methods are available but are **not** runnable handlers by themselves.
- **Result and output semantics:** ordered tables and other artifacts; cell/style/p-value presentation; output-document conversion, layout and target planning; composite outputs; safe-write inspection/consent/outcome contracts. Actual workbook mutations are implemented by host adapters, including Office.js readback verification and explicit overwrite consent; Common does not itself mutate worksheets.
- **Portable chart/report preparation:** reusable series ordering, axes, reference/threshold lines, labels, legend/annotation instructions and chart sizing/formatting policies. Accepted Windows integrations now consume shared preparation for multiple comparisons, agreement, PSM Love plots, Bullet/Sankey/CDF/Dumbbell/Ladder and other specialty graphics. Core computes numerical plot geometry; Common does not create native Excel charts.
- **Application infrastructure:** versioned settings, diagnostics/run correlation, safe faults/warnings, help/version/availability metadata, progress/cancellation coordination, and document/operation lifecycle protection. The Office task pane uses these contracts for persisted preferences, guarded source retirement and numeric p-value presentation.

See [`src/Input`](src/Input), [`src/Analysis`](src/Analysis), [`src/Output`](src/Output), [`src/Host`](src/Host) and [`src/Infrastructure`](src/Infrastructure) for the actual source. Detailed design decisions and historical validation reports are maintained in [`../docs/architecture/`](../docs/architecture/README.md).

## Boundary rules

Core computes statistical results. Common interprets typed spreadsheet data, coordinates analyses and describes output. The Windows host performs COM/Excel-DNA read/write and WinForms presentation; OfficeAddin performs Office.js read/write and Blazor presentation. No COM types, Office.js handles, WinForms controls or platform-specific runtime dependencies should enter Common. Shared host-neutral operation and data semantics should also allow a future non-Excel adapter without promising an implemented LibreOffice integration.

## Development direction

The Windows code audit and successive application/graphics-policy extractions are complete through comparison, agreement and PSM Love preparation. Continue extending the typed handler registry method by method, reusing existing input/output and lifecycle contracts. The next planned Windows extraction concerns survival/SPC chart-series specifications; it does **not** enable those methods in Office. Keep Windows chart rendering host-owned, preserve accepted fixtures and require separate Office editor, renderer, writer and host checks for every newly exposed workflow.

## Build and test

```powershell
dotnet build BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj -c Release
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
```

The library targets `netstandard2.0`; its tests target `net10.0`. Actual workbook behavior must additionally be verified in the relevant hosts.
