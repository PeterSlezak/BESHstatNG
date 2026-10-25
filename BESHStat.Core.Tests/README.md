# BESHStat.Core.Tests — statistical correctness and portability

This **.NET 10 MSTest** project validates [`BESHStat.Core`](../BESHStat.Core/README.md) independently of Excel. Its tests cover statistical accuracy, edge cases, numerical conditioning, regression models and portable calculations. Reference fixtures include NIST ANOVA/linear-regression cases and model data for GLM, GEE, survival, mixed models, multivariate procedures and other supported algorithms.

## Run

From the repository root:

```powershell
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
```

Prefer deterministic input/expected-output fixtures and explicit numerical tolerances. Check singular matrices, missing or invalid observations, convergence warnings, tie-handling and boundary cases as appropriate to each method. Avoid dependencies on workbook state, native Excel or UI controls. Do not replace golden data without a reviewed statistical reason. Accepted corrections to numeric post-hoc p-values must retain numeric precision and independent significance/presentation tests; do not restore obsolete string-valued probability outputs to satisfy a snapshot.

## Role in development

This is the portable calculation gate. It works alongside [`BESHStat.Excel.Common.Tests`](../BESHStat.Excel.Common.Tests/README.md), the Windows legacy suite and the browser/Office adapter suites. A passing Core test run does **not** demonstrate that a Ribbon form, chart renderer, Excel writer or Mac/Online host works.
