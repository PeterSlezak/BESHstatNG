# BESHStat.Core — portable statistical engine

`BESHStat.Core` is the portable numerical and statistical library used by BESHStatNG. It targets **.NET Standard 2.0** and is designed to run independently of Excel, WinForms, Excel-DNA and COM.

## Scope

The library contains statistical distributions and special functions; numerical linear algebra; core data and statistical-result models; regression, generalized and mixed models; survival analysis; nonparametric and agreement methods; multivariate analysis, SPC and PSM calculations; resampling; and host-neutral chart data/geometry routines. The implementation is shared by the Windows add-in and exploratory WebAssembly integration.

The core should return computations and portable result data, **not** select worksheet ranges, create Excel charts or dialogs, or write to a workbook. Spreadsheet semantics belong in [`../BESHStat.Excel.Common/`](../BESHStat.Excel.Common/README.md), and concrete host operations belong in the host projects.

## Build and verify

```powershell
dotnet build BESHStat.Core/BESHStat.Core.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
```

Run these commands from the repository root using a compatible .NET SDK. The test suite includes numerical references and regression checks; a passing Core build alone does not establish Excel host parity.

**Status:** portable library under ongoing development. The [Windows Excel-DNA add-in](../BESHstatNG/README.md) is the stable user-facing product.
