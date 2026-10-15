# BESHStat.Excel.Common.Tests — shared contract and application tests

This **.NET 8 / MSTest** project exercises [`BESHStat.Excel.Common`](../BESHStat.Excel.Common/README.md), including spreadsheet input parsing, ANOVA/Kruskal–Wallis workflows, the analysis catalog/registry, typed result contracts and host-capability preflight.

It also maintains frozen regression fixtures under `TestData/Stage304/` so schema and numerical/output changes can be checked against the approved shared-layer baseline.

## Run

From the repository root:

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
```

Test changes to data parsing, result presentation, analysis identifiers and capabilities with positive and rejection cases. Preserve approved golden fixtures unless a separately reviewed behavior change justifies updating them. Integration into actual Excel and Office.js remains the host projects' responsibility.

**Status:** active test project; refer to [main README](../README.md) for release status.
