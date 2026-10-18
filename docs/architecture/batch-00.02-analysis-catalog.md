# BESHStatNG — Batch 00.02: stable analysis IDs and discovery descriptors

**Baseline:** `BESHstatNG(20261013-190356).zip` (developer-validated Batch 00.01).
**Scope:** descriptor/catalog metadata, compatibility tests, a small `BESHStat.WasmSmoke` selector update.
**Next:** 00.03 — strongly typed handler registry and compatibility adapter.

## Architecture decision

An analysis now has a stable, lowercase **string ID**, independent of Excel-DNA Ribbon callback names, C# UI display labels, and integer enum values:

| Stable ID | Existing enum | Current shared detail result | Windows help method |
|---|---|---|---|
| `anova.oneway` | `SpreadsheetAnalysisKind.OneWayAnova = 1` | `OneWayAnovaApplicationResult` | `/methods/one-way-anova/` |
| `nonparametric.kruskal-wallis` | `SpreadsheetAnalysisKind.KruskalWallis = 2` | `KruskalWallisApplicationResult` | `/methods/kruskal-wallis-test/` |

The stable IDs agree with the two `analysisIdProposed` values frozen in the five Batch 00.01 golden JSON fixtures and with `docs/architecture/manifests/ribbon-workflow-map-stage304.csv`. **Do not renumber the enum or rename released IDs.** Future serialized analysis requests should store these string IDs. In this batch **requests and results still use the legacy enum**: introducing a string-based execution request, handler registry, or serialized request contract is specifically reserved for 00.03–00.04.

New public members in `BESHStatNG.ExcelCommon`:

- `AnalysisIds`: two string constants.
- `IAnalysisDescriptor` / `AnalysisDescriptor`: immutable identifying/display/help/category metadata; contract/schema IDs; input and options CLR types; output-kind flags; current availability; estimated (advisory) cost; required host capabilities; nullable legacy kind; factory returning a **fresh** default-options DTO.
- `AnalysisCatalog.All`, `GetById` / `TryGetById`, `GetByLegacyKind` / `TryGetByLegacyKind`, `TryGetLegacyKind`, and `ValidateDescriptors`.

Catalog lookups use **ordinal, case-sensitive** IDs. Invalid identifiers, missing schemas, invalid option factories, duplicate IDs, and duplicate legacy mappings fail explicitly. Descriptor enumeration and per-entry input/capability lists are read-only copies. Registration is explicitly enumerated in Common, compatible with .NET Standard 2.0, .NET Framework 4.8, and browser WASM, and does not use dynamic invocation/reflective discovery of handlers.

### Descriptor meanings and limitations

- `InputSchemaId = grouped-numeric.wide.v1` refers to the existing **header-required, two-or-more numeric columns** parser; it is not a new schema implementation.
- `OptionsSchemaId` names the current .NET options type and default behavior, not a machine-generated JSON Schema.
- `OutputKinds` advertises current tables, metrics and typed detail. It does **not** promise an output document or charts.
- `RequiredCapabilities` is empty for the two calculations; real host capabilities/new-workbook preflight follow in 00.06. An empty list **does not** imply that every output destination is available in Office.js.
- `EstimatedCost = Low` is an initial advisory classification and is not a benchmark or cancellation promise.
- `HelpUrl` matches the current latest-path links in Windows `src/Help/HelpLinks.vb`; URLs are metadata and may point at existing documentation, not a separate support/uptime guarantee.
- `CreateDefaultOptions()` reproduces existing DTO defaults: ANOVA alpha 0.05 and all extra report options off; Kruskal–Wallis alpha 0.05 and Dunn on. Hosts can override defaults with their current UI state.
- The descriptor availability is **ImplementedInCommon** (not "feature verified on Mac"). The two workflows still need host smoke checks.

### Host behavior

**Windows Excel-DNA is untouched.** It still submits enum-based `SpreadsheetAnalysisRequest` objects and runs through `SharedAnalysisApplicationService.Run`; no numerical or output semantics are modified. In `BESHStat.WasmSmoke/Pages/Home.razor`, only the **analysis selector's options/labels** and display-name lookup now come from `AnalysisCatalog`. Its existing selection state, per-analysis options controls, dispatch switch, result parsing and Office.js writer are unchanged. This demonstrates catalog-driven selection without prematurely implementing the handler registry or translating WinForms screens.

To discover features in VB.NET:

```vb
For Each descriptor As IAnalysisDescriptor In AnalysisCatalog.All
    Dim stableId As String = descriptor.Id
    Dim oldKind As Nullable(Of SpreadsheetAnalysisKind) = descriptor.LegacyKind
    Dim defaults As SpreadsheetAnalysisOptions = descriptor.CreateDefaultOptions()
Next
```

## Files delivered

| File | Change |
|---|---|
| `BESHStat.Excel.Common/src/Analysis/AnalysisIds.vb` | New stable constants |
| `BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb` | New descriptor contracts and validation |
| `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb` | New compile-time catalog and enum-compatibility lookup |
| `BESHStat.Excel.Common.Tests/src/AnalysisCatalogTests.vb` | New metadata, defaults, identity, mapping, validation and 00.01 fixture tests |
| `BESHStat.WasmSmoke/Pages/Home.razor` | Selector populates from the catalog |
| `docs/architecture/batch-00.02-analysis-catalog.md` | This file |

The SDK-style VB projects auto-include the added `.vb` files; no project manifest update is needed. All Batch 00.01 fixtures and inventories remain unchanged.

## Automated acceptance

From the solution root on the development machine:

```shell
python tools/verify_stage304_inventory.py
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --configuration Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj --configuration Release
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj --configuration Release
```

Then build the full Windows solution in Visual Studio (including Windows host + `BESHStatNG.Test`) and run the legacy MSTest suite. Expected assertions: exactly two distinct IDs; correct historical 1/2 enum mapping; exact schema/help/type metadata; independent ANOVA/Kruskal default DTO instances; read-only catalog collections; duplicate detection and missing-ID behavior; all five 00.01 golden fixtures still match their catalog IDs/enums. No compiled tests were run in the patch authoring container (no .NET SDK present).

## Manual host checks

1. **Windows Excel-DNA:** use `docs/architecture/golden-inputs/anova-stage304.csv`, select A1:C4, run One-Way ANOVA with Welch and Tukey–Kramer enabled, alpha 0.05, New Worksheet. Expect F=13, p=0.006591796875, Welch F≈11.142857142857142, Welch p≈0.023156899810964086; verify table order and significance styling are unchanged.
2. **Excel Online / WasmSmoke:** select `docs/architecture/golden-inputs/kruskal-wallis-stage304.csv` A1:C5, open the task pane, verify the analysis dropdown shows **One-way ANOVA** and **Kruskal-Wallis** (catalog-derived), select Kruskal–Wallis with Dunn on, alpha 0.05, New Worksheet. Expect H=7.875, corrected H≈8.1603260869565215, corrected p≈0.016904709225411534; check Dunn table follows the primary table and p-value formatting is retained.
3. Optional exploratory check: change the dropdown between analyses and verify Welch/Dunn inputs appear as before. This step checks selector wiring; it does not establish Mac Excel support.

**Known limits:** this is descriptor discovery only, not 00.03 registry dispatch, 00.04 typed result artifacts, 01.xx input schemas, 02.xx output planning, 00.06 capabilities, or production `BESHStat.OfficeAddin`. The early `BESHStat.WasmSmoke` still uses the enum internally. Rollback is to remove the three new Common files, the new test file, the documentation page, and restore the prior `Home.razor` from the baseline ZIP (or reverse the supplied patch).
