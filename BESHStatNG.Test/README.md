# BESHStatNG.Test — legacy Windows regression tests

This is the existing **.NET Framework 4.8** VB.NET test project for the Windows BESHStatNG codebase and historical validation scenarios. It preserves compatibility checks while statistical implementation is shared with portable Core.

## Run

Use **Visual Studio on Windows** with .NET Framework 4.8 targeting support and the project's restored test adapter/dependencies. Open `BESHStatNG.Test/BESHStatNG.Test.sln` (or your established Windows test solution) and run the tests through Test Explorer. The project also contains reference datasets and validation outputs under its test folders.

These are not the same as [`BESHStat.Core.Tests`](../BESHStat.Core.Tests/README.md) or [`BESHStat.Excel.Common.Tests`](../BESHStat.Excel.Common.Tests/README.md), which target .NET 8 and exercise portable/shared layers. Run the relevant suites together when changing code shared with Windows Excel.

For releases, complete **real-Excel** manual tests of Ribbon callbacks, worksheet UDFs, output positions and charting; no headless test suite by itself proves GUI behavior.

See the [main README](../README.md) and [validation page](https://beshstat.eu/validation/).
