# BESHStat.Excel.Common.Tests — shared contract and workflow tests

This **.NET 10 MSTest** project validates [`BESHStat.Excel.Common`](../BESHStat.Excel.Common/README.md) without using a real Excel workbook. It protects the shared boundary used by both the Windows and Office.js hosts.

Tests cover typed range/cell and host-identity contracts, date/error/missing policies, numeric/categorical/longitudinal/model/SPC/PSM parsers, stable method metadata, analysis-handler dispatch, statistical workflow assembly, output formatting/layout/targets, safe-write contracts, settings, diagnostics, faults, cancellation and lifecycle safeguards. Approved regression snapshots live under `TestData/Stage304/` and related fixture folders.

## Run

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
```

Each contract change should test both accepted inputs and explicit failures: invalid ranges, inconsistent sources, missing headers, nonfinite numbers, date-system ambiguity, oversize acquisitions, unsupported host capabilities, stale operations and conflicting destinations. Do not silently rebaseline accepted output or error codes.

These tests check shared semantics only. Actual Office.js request ordering and Excel mutations are covered by host adapter tests and owner-performed Excel smoke tests.
