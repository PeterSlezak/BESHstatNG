# BESHStat.Core — portable statistical and numerical engine

`BESHStat.Core` is the reusable numerical foundation for BESHStatNG. It is written primarily in VB.NET, targets **.NET Standard 2.0**, and builds for both the Windows .NET Framework 4.8 add-in and modern .NET/WebAssembly runtimes. It has **no dependency on Excel, Excel-DNA, COM, WinForms or Office.js**.

## Implemented responsibilities

Core contains statistical distributions and special functions; matrix, decomposition and linear-algebra utilities; typed statistical data/result models; descriptive, assumption, parametric, nonparametric and agreement procedures; linear, generalized, categorical and mixed regression; survival analysis; resampling; multivariate analysis; statistical process control; propensity-score matching; and reusable plot calculations such as histogram binning, ECDF, violin and other chart geometry. Results and algorithms are covered by numerical and reference-case tests, including NIST data.

Core owns *statistical computation*. It should not know where spreadsheet cells are located, how a host opens a workbook, what a Ribbon button is, or how native Excel charts are rendered. User-facing spreadsheet acquisition, validation, output semantics and orchestration belong to [`BESHStat.Excel.Common`](../BESHStat.Excel.Common/README.md); Excel COM and browser operations belong to their respective hosts.

## Build and verification

Run from the repository root with the .NET 10 SDK:

```powershell
dotnet build BESHStat.Core/BESHStat.Core.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
```

The library's target remains `netstandard2.0`, even though its **test** project targets `net10.0`. For changes to statistical routines, add precision/edge-case/regression tests, including a WASM compatibility check when runtime behavior might differ.

## Ongoing direction

Keep numerical and chart-data functions reusable rather than duplicating them in Windows dialogs or JavaScript. Extract any remaining portable calculation helpers from WinForms, model-specific UI code, and Windows chart renderers only with behavior-preserving tests. Preserve `Option Strict On`, typed APIs, deterministic results and portable progress abstractions. Host-independent implementation makes future spreadsheet integrations technically possible, without adding a third host to the current release scope. The current Office add-in calls Core for its supported ANOVA/Kruskal–Wallis computations and read-only model-design preview; exposing other existing Core algorithms requires a verified Common handler, host editor and output lifecycle rather than a second calculation implementation.

**Product status:** this is an actively maintained library, not a standalone application. The supported user product is the [Windows add-in](../BESHstatNG/README.md).
