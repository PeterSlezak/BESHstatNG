# BESHStatNG — supported Windows Excel-DNA add-in

This project is the existing, released **Windows desktop Excel** implementation. It targets **.NET Framework 4.8** and uses VB.NET, Excel-DNA, Microsoft Excel COM/Interop, WinForms and native Excel charting. Users should install an official signed package from [beshstat.eu/download](https://beshstat.eu/download/), not this source directory.

## Responsibilities and source layout

- `Resources/RibbonXML.xml`: actual Windows Ribbon commands/callbacks.
- `src/UI/`: Windows dialogs, selection controls and host UI handling, including RefEdit lifecycle integration.
- `src/DM/`: Excel data/import, workbook and result-writer adapters.
- `src/ExcelUDFs/`: registered worksheet functions and Windows-specific registration/Excel value conversion.
- `src/Graphics/`: native Excel chart construction, renderers and export.
- `src/Help/`: Windows help launch and generated route integration.
- `tools/`: metadata generation, validation and support scripts.

Statistical algorithms and chart calculations are increasingly owned by [`BESHStat.Core`](../BESHStat.Core/README.md). Shared parsing, analysis options/orchestration, result/output semantics and lifecycle contracts belong to [`BESHStat.Excel.Common`](../BESHStat.Excel.Common/README.md). Windows-specific API calls and the WinForms presentation layer remain here.

## Current migration state and development direction

The comprehensive source inventory has reviewed Windows forms, UDFs, Graphics and infrastructure. Accepted migrations have moved substantial request/report, output and graphics preparation into Core/Common, with Windows adapters consuming shared contracts. Recent chart work covers ROC/Q–Q, box/histogram/violin, XYZ/scatter-matrix/3D, 17 specialty-graphics families, and multiple-comparison confidence intervals, agreement references and PSM Love series. **Native chart/shape rendering, Excel error bars, labels applied through COM, events, RefEdit, resource lifetime and file export remain Windows responsibilities.**

The next planned extraction covers Kaplan–Meier/Cox survival and univariate/multivariate SPC plot payloads. Existing statistical engines and known compatibility quirks are not silently rewritten; original scientific/native authorization and host-validation gates remain restricted. No accepted extraction alone enables a chart in the Office add-in. Future methods and charts should reuse Core computation and Common presentation contracts and require separate statistical and Excel verification. See the [current roadmap](../planning/BESHStatNG_CrossPlatform_MasterPlan.md) and [architecture acceptance dossiers](../docs/architecture/README.md).

## Build and verify

Use **Visual Studio 2026 on Windows**, the .NET Framework 4.8 targeting pack, restored Excel-DNA/Interop packages, and an appropriate Excel installation. Open `../BESHStatNG.sln`, restore packages and build the matching x86/x64 configuration. Run Core/Common and the legacy regression suites and perform real-Excel smoke tests for representative Ribbon actions, UDF spill/results, output targets, Help and charts.

The Windows XLL release is built/packed/signed through the existing Windows process and delivered using [`BESHStatNG_Installer`](../BESHStatNG_Installer/README.md). A passing `dotnet build` for a portable project does not validate Excel COM behavior or an MSI.

See the [online method and UDF help](https://beshstat.eu/beshstatng/help/latest/) and the [root README](../README.md) for public capabilities and support scope.
