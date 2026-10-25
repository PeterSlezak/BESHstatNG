# BESHStatNG.Test — Windows legacy and host regression tests

This project (`BESHStatNG.Tests.vbproj`) preserves the existing **.NET Framework 4.8** VB.NET/MSTest compatibility and validation suite for the Windows Excel-DNA add-in. It covers legacy statistical and model workflows, host-related transformations, reference outputs and behavior that must not change while code moves to Core/Common.

Its test folders contain historical numeric/model fixtures, NIST/reference data, regression outputs and specialist comparison cases. This project is deliberately separate from the modern `net10.0` Core/Common tests.

## Run

On Windows, open `../BESHStatNG.sln` in **Visual Studio 2026** with the .NET Framework 4.8 developer/targeting tools installed. Restore packages, build the test project and run its tests from Test Explorer or through the repository's legacy validation script:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Legacy -VisualStudio 2026
```

Execute from the repository root. Some older Visual Studio test discovery advisories may remain without implying failure; review the actual results/TRX report.

## Validation scope

Run this suite alongside Core/Common tests when migrating Windows calculation or UI-adjacent logic. Automated tests do not replace owner-operated Excel GUI, workbook lifecycle, Ribbon callback, UDF, chart-output and MSI checks. Retain historical tolerances and golden data unless a reviewed numerical change requires updating them.
