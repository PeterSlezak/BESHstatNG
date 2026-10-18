# Batch 00.05 — Transitional Office.js VB helpers: disposition and compatibility audit

**Baseline:** `BESHstatNG(20261015-163835).zip` (Batch 00.04 developer-validated).
**Scope:** Retire two unused Windows-host VB placeholders and guard against their return.
**Runtime changes:** None in the active Windows, Core, Common, or Office.js execution paths.

## Evidence and decisions

Static references were searched throughout the supplied solution (VB/C#/Razor/JS, project files and solution assets):

| Legacy class / file | Defined | Actual consumers outside its file | Disposition |
| --- | --- | --- | --- |
| `OfficeJsDataImporter`, `OfficeJsDataImportOptions` / `BESHstatNG/src/DM/OfficeJsDataImporter.vb` | Windows host; two import overloads plus `ImportInto` | **None**; only the `.vbproj` Compile entry | Delete source and project entry |
| `OfficeJsResultWriter`, `GoogleSheetsResultWriter` / `BESHstatNG/src/DM/ResultWriter/OfficeJsResultWriter.vb` | Windows host; block accumulator and Google Sheets placeholder subclass | **None**; only the `.vbproj` Compile entry | Delete source and project entry |

**Why no port of these specific classes?** Neither was used by the current Office.js/WASM prototype. Moving them into `BESHStat.Excel.Common` would introduce a second, unused input conversion and result representation. Deletion follows the user's explicit authorization to remove both placeholders. `GoogleSheetsResultWriter` disappears with its base class because it is also unused and out of scope.

**Current, retained paths:**

- **Windows input:** `ExcelDnaDataImporter` → `ExcelDnaSharedApplicationAdapter.ToGroupedNumericData` → Common `SharedAnalysisApplicationService`.
- **Windows output:** Common `ResultTableWriteRequestFactory` → `ExcelDnaResultWriter.WriteSharedRequest` (Excel Interop formatting remains in the host).
- **Office/WASM input:** `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js` → typed `SpreadsheetRangeData` and `GroupedNumericRangeParser` in Common.
- **Office/WASM output:** Common `ResultTableWriteRequestFactory` → `officeBridge.js` Excel API calls.

The existing Excel-DNA code and JS bridge have **not** been changed. No Core numerical code, `AnalysisIds`, typed registry, metadata catalog, or result document was touched. We deliberately **do not** remove `ResultTableWriterBase` or `ResultTableOutputBlock` in Core: the Windows `ExcelDnaResultWriter` still inherits/uses them.

## Automated checks

New Python read-only guard:

```powershell
python BESHstatNG/tools/audit_transitional_officejs.py
python -m unittest discover -s BESHstatNG/tools/tests -p 'test_*.py' -v
```

The audit fails if a deleted file persists, if a live source/project file mentions a retired class, or if the active host contracts disappear. Six tests cover the current solution, accidentally restored source, stale project reference, stale public-type use, missing canonical path, and historical-document exclusion.

**Existing behavioral regression gates (unchanged):**

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
```

Build the full Windows solution and run `BESHStatNG.Test` as configured locally. The existing `Stage304ContractSnapshotTests` already compare five ANOVA/Welch/post-hoc/Kruskal–Wallis golden fixtures, range import policy and serialized write-request metadata. Because the retired classes have **no call sites**, an 'old versus new host behavioral equivalence' execution test would be misleading: they were not active execution paths. There is no new output or coercion behavior to test.

**Manual smoke:** (1) Windows Excel-DNA ANOVA with Welch and a post-hoc method to a new worksheet; (2) WasmSmoke/Excel Online Kruskal–Wallis with Dunn, p-value formatting and output to a new worksheet. Optionally verify About/Help/Settings and UDF registration remain unaffected.

## Apply/rollback

Use the `.patch` for a clean removal of both sources and their two Compile declarations. Because the uploaded archive has CRLF bytes in some source files whereas `.gitattributes` normalizes text to LF, a raw ZIP extraction may require `git apply --ignore-whitespace BESHStatNG_Batch_00.05.patch`; a normalized Git checkout can use plain `git apply`. If using the replacement-files ZIP, copy the included files to matching relative paths **and explicitly delete**:

```
BESHstatNG/src/DM/OfficeJsDataImporter.vb
BESHstatNG/src/DM/ResultWriter/OfficeJsResultWriter.vb
```

The ZIP includes `REMOVE_00.05_FILES.txt` as a reminder; simply copying the ZIP does not delete files. The Windows `.vbproj` no longer compiles them even if residual copies remain. Roll back by restoring the two source files and original `.vbproj` from `BESHstatNG(20261015-163835).zip`.

**External API caution:** Both removed classes were `Public`. We verified no in-solution use, but cannot exclude external binary consumers not present in the ZIP; such integrations would require replacement.

**Next:** 00.06 — host capabilities and preflight metadata. This batch does not introduce an Office production add-in or change Excel Online client capabilities.
