# BESHStat.Core.Tests — portable numerical validation

This **.NET 8 / MSTest** project verifies the behavior of [`BESHStat.Core`](../BESHStat.Core/README.md). It includes numerical regression checks, reference cases, model tests, and test datasets (including NIST benchmark material), independently of desktop Excel.

## Run

From the repository root with a compatible SDK:

```powershell
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
```

The test project uses `../test.runsettings` where configured. Several cases test numerical tolerances, singular/ill-conditioned cases, statistical reference outputs, or host-neutral callback behavior. Add focused tests with changes to the corresponding Core methods; keep shared-state tests isolated when test parallelism could interfere.

This suite is a **portable computation gate**, not a replacement for Windows Excel GUI, output-writer, installer, or future Office.js integration tests. See the [main README](../README.md) for project status.
