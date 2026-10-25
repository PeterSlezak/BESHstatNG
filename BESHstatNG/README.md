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

## Current development direction

Preserve all existing Ribbon, UDF, output and chart behavior while finishing extraction of portable calculations and business rules still mixed into Windows source—**including the form code**, model builders, chart preparation, regression/survival/multivariate/SPC/PSM report assembly and UDF wrappers. New methods and chart enhancements should put their statistics in Core and their host-neutral result/plot contracts in Common so they can later be exposed in the Office add-in without reimplementing numerical algorithms in JavaScript. Improvements planned for distribution/normality, comparisons, categorical and ordinal association, paired tests and advanced charts will be introduced with independent statistical validation.

The Windows application remains the behavioral reference during migration, **not** the desired home for new host-neutral algorithms. Host-specific controls, native Excel rendering, file export and COM events stay here.

## Build and verify

Use **Visual Studio 2026 on Windows**, the .NET Framework 4.8 targeting pack, restored Excel-DNA/Interop packages, and an appropriate Excel installation. Open `../BESHStatNG.sln`, restore packages and build the matching x86/x64 configuration. Run Core/Common and the legacy regression suites and perform real-Excel smoke tests for representative Ribbon actions, UDF spill/results, output targets, Help and charts.

The Windows XLL release is built/packed/signed through the existing Windows process and delivered using [`BESHStatNG_Installer`](../BESHStatNG_Installer/README.md). A passing `dotnet build` for a portable project does not validate Excel COM behavior or an MSI.

See the [online method and UDF help](https://beshstat.eu/beshstatng/help/latest/) and the [root README](../README.md) for public capabilities and support scope.
