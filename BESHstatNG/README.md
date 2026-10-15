# BESHStatNG — Windows Excel-DNA add-in

This is the **supported Windows desktop Excel implementation** of [BESHStatNG](../README.md). It provides Ribbon commands, WinForms analysis dialogs, worksheet functions (UDFs), Excel data acquisition, result formatting, chart creation and export, and Windows-specific integration.

For **end users**, install a published build from [beshstat.eu/download](https://beshstat.eu/download/) rather than this source project. The complete method and UDF help is available in the [online documentation](https://beshstat.eu/beshstatng/help/latest/).

## Technology and responsibilities

- Visual Basic .NET targeting **.NET Framework 4.8**, using **Excel-DNA** and Microsoft Excel COM/Interop.
- `Resources/RibbonXML.xml` declares the current Windows Ribbon buttons and callbacks; `src/UI/` contains the Windows interaction logic.
- `src/ExcelUDFs/` exposes the worksheet functions; `src/DM/` holds Windows data/writer adapters; `src/Graphics/` contains Excel chart renderers; `src/Help/` contains help integration.
- Statistical computation increasingly comes from [`../BESHStat.Core/`](../BESHStat.Core/README.md). Shared spreadsheet-analysis workflows and contracts are in [`../BESHStat.Excel.Common/`](../BESHStat.Excel.Common/README.md).
- Windows-specific behavior stays here; Core and Common should not depend on WinForms, Excel COM objects or Excel-DNA APIs.

## Build and test

1. Use Windows, desktop Excel, Visual Studio with .NET Framework 4.8 support, and NuGet restore.
2. Open `../BESHStatNG.sln`, restore packages and build the add-in for the intended Excel bitness/configuration.
3. Run the legacy and portable tests, then smoke-test representative Ribbon analyses, UDF formulas, result placement and charting in **real Excel**.
4. Create signed/packed release artifacts through the established release workflow and [`../BESHStatNG_Installer/`](../BESHStatNG_Installer/README.md). A successful source build alone is not a released MSI.

**Development status:** This Windows host is the stable reference implementation. Portable and Office.js work elsewhere in the repository is ongoing and does not change its published support scope.

For change history and usage instructions see [What's New](https://beshstat.eu/beshstatng/help/latest/whats-new/) and the [main README](../README.md).
