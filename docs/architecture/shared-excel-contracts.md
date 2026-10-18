# BESHStatNG — Stage 3.04 shared Excel contract baseline (Batch 00.01)

**Status:** source-verified baseline and automated tests supplied; **not yet developer-approved by host smoke runs**.

**Master plan:** `BESHStatNG_CrossPlatform_MasterPlan_Stage304`, §13 Phase 00 / §17.

**Source ZIP:** `BESHstatNG(20261013-183240).zip`, SHA-256 `944ecef22ac371c8740dedc2a051ff0ddc12bffec5873e40a3a71ab5c345b9a9`.

**Date:** 2026-10-13.

## 1. Scope and boundary

Batch **00.01** freezes *observable Stage 3.04 contracts*, not the future contract design. Statistical algorithms, Common production APIs, the Windows Ribbon, Excel-DNA, and the prototype Office.js bridge remain unchanged. New work consists of regression tests, versioned reference inputs/expected results, inventory manifests, a source verifier, and this document.

Stage 3.04 currently has precisely **two** shared `SpreadsheetAnalysisKind` values:

| Legacy enum (numeric) | Proposed stable string ID **for 00.02, not implemented here** | Application service | Existing result detail |
|---|---|---|---|
| `OneWayAnova` (`1`) | `anova.oneway` | `OneWayAnovaApplicationService` | `OneWayAnovaApplicationResult` |
| `KruskalWallis` (`2`) | `nonparametric.kruskal-wallis` | `KruskalWallisApplicationService` | `KruskalWallisApplicationResult` |

Both are dispatched by `SharedAnalysisApplicationService.Run(SpreadsheetAnalysisRequest)`. Requests accept a rectangular `SpreadsheetRangeData` or pre-parsed `GroupedNumericData`. The dispatcher currently returns `SpreadsheetAnalysisResult` holding ordered `ResultTableOutputModel` tables, a `Dictionary(Of String, Double)` of named metrics, and analysis-specific `Detail As Object`. These limitations are intentional subjects of **00.02–00.04**, not modified in 00.01.

### Current defaults frozen by the tests

- `OneWayAnovaAnalysisOptions.Alpha = 0.05`; Welch, Fisher LSD, Bonferroni, Tukey–Kramer, and Games–Howell switches default to **False**.
- `KruskalWallisAnalysisOptions.Alpha = 0.05`; Dunn comparisons default to **True**.
- `SpreadsheetOutputTarget`: `NewWorksheet`, worksheet name `BESHStat_Result`, start row/column `(1,1)`.
- The grouped-range parser requires a first-row header, two or more columns, at least two numeric observations per group, skips blanks independently for each group, generates `Group n` for empty headers, and rejects body text/errors/Boolean/non-finite numbers. This is **legacy behavior**, not a promise about the future configurable input pipeline.
- `SpreadsheetCellKind` serialized numeric values: `Blank=0`, `Number=1`, `Text=2`, `Bool=3`, `CellError=4`. Current cell JSON includes primitive fields; no typed output error contract yet.

## 2. Golden fixtures

`BESHStat.Excel.Common.Tests/TestData/Stage304/` contains five hand-reviewed **semantic** JSON fixtures; matching input CSVs under `docs/architecture/golden-inputs/` can be pasted into Excel. Golden metrics come from the Stage 3.04 regression tests; table headers/order and metadata were checked against the existing Core `wrapResults` implementations. These are *target reference values*, not a claim that new tests were executed in this environment.

| Fixture | Primary statistic | Relevant p-value | Expected table order |
|---|---|---|---|
| `anova-base` | F = **13** | **0.006591796875** | Classical ANOVA |
| `anova-welch-tukey` | F = **13**; Welch F ≈ **11.142857142857142**, denominator df = **4** | Welch p ≈ **0.023156899810964086** | ANOVA with Welch columns → Tukey–Kramer |
| `anova-all-posthoc` | Same F and Welch values | Same reference p-values | ANOVA/Welch → Fisher LSD → Bonferroni → Tukey–Kramer → Games–Howell |
| `kw-base` | H = **7.875**, tie-corrected H ≈ **8.1603260869565215** | Uncorrected **0.019496896108597995**; corrected **0.016904709225411534** | Kruskal–Wallis |
| `kw-dunn` | Same H and corrected H | Same reference p-values | Kruskal–Wallis → Dunn |

The ANOVA CSV has 3 groups × 3 observations; the Kruskal–Wallis CSV has 3 groups × 4 observations with ties. Values have no timestamps, localization-dependent rendered strings, Excel theme/font selections, or sheet-creation side effects. Metric comparisons use tolerance **5×10⁻⁸**, with stricter tolerances for selected numeric table cells. The fixtures explicitly check table dimensions, top-left header label, top-header/footer counts and **one-based body-relative** p-value columns/cells. Test fixtures additionally verify selected key numeric cells, not a byte-for-byte snapshot of every post-hoc confidence-interval string.

The test `Stage304GoldenCases_WriteRequestsPreserveFormattingAndOrder` also checks `ResultTableWriteRequestFactory.CreateMany` preserves **table ordering**, alpha, number-format policy, header/footer counts, p-value column/cell markers, target sheet, and start row/column. **Spacing is still host-owned** in Stage 3.04; the future shared `OutputLayoutPlanner` is Batch 02.03. Do not describe the current independent requests as one fully positioned output document.

The serialized-cell tests deliberately capture the existing `#N/A`, `#Pinf`, `#Ninf` string substitution for non-finite output numbers and distinguish it from input `SpreadsheetCellKind.CellError`. This is an observed legacy transport behavior to replace with a canonical error policy in Batch 01.02 / 02.02; it must **not** be interpreted as Excel error/formula-safety approval.

## 3. Feature exposure manifests

`docs/architecture/manifests/` contains:

- `ribbon-inventory-stage304.csv`: 84 `<button>` declarations and their exact IDs, labels, callbacks.
- `ribbon-workflow-map-stage304.csv`: one row **per button**, a callback-based `WorkflowKey`, and an ID for only the two currently shared workflows; all Office parity flags deliberately remain **unverified**.
- `udf-inventory-stage304.csv`: all 268 `<ExcelFunction>` declarations across 23 files, with name, VB method, category, and preliminary *family-based* migration classification from the attached audit.

There are **84 unique Ribbon button IDs** and **81 distinct actions**. Three callbacks are intentionally used twice: `OnbtmKMPressed`, `OnbtmCAPressed`, `OnbtmCoxPressed`. Their duplicate Ribbon placements do **not** imply independent implementations. All 81 callback names have corresponding `Sub` methods in `BESHstatNG/BESHStatAddIn.vb`. The 268 declared UDF names and 268 method names are unique in the audited source. A UDF listed here is **not** automatically an implemented/verified Office Custom Function; dispositions are deferred to Batch 11.01.

The deterministic, offline verifier uses the current Ribbon XML and actual annotated UDF method declarations (not filename counts alone). Run:

```shell
python tools/verify_stage304_inventory.py
```

Expected result: `PASS Stage 3.04 inventory/source comparison` and the counts above. A future intentional public-exposure change requires a separately versioned manifest update and review; this frozen baseline should remain auditable.

## 4. Build/test commands and acceptance

From the repository/solution root with the .NET 8 and project-required SDKs installed:

```shell
python tools/verify_stage304_inventory.py
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --configuration Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj --configuration Release
```

Also build the full Windows solution and run all `BESHStatNG.Test` legacy tests in your existing Visual Studio environment. Run the independent `BESHStat.WasmSmoke` ANOVA and Kruskal–Wallis workflows to ensure Core + Common continue to load in browser WASM. The new contract tests are **not** host integration tests; a PASS does not by itself confirm Excel Online/Mac parity.

### Manual host smoke A — Windows Excel-DNA

1. Open `docs/architecture/golden-inputs/anova-stage304.csv` in Excel; select the three-column data including headers (`A1:C4`).
2. Run **One-Way ANOVA** from the BESHStatNG Ribbon; enable **Welch** and **Tukey–Kramer**, use alpha `0.05`, choose **New Worksheet**.
3. Verify primary F ≈ **13**, p ≈ **0.006591796875**; Welch F ≈ **11.142857142857142**, Welch p ≈ **0.023156899810964086**; inspect two shared Core tables in the declared order and their p-value formatting. The Windows UI may also emit optional UI-owned supplemental output; note this separately rather than treating it as shared parity.
4. Save the output workbook or screenshots and record sheet name, output start cell, formatting and any additional tables in a local acceptance log. The golden fixtures intentionally exclude unpredictable generated sheet names.

### Manual host smoke B — Excel Online / Office.js

1. Paste `docs/architecture/golden-inputs/kruskal-wallis-stage304.csv` into an Excel Online workbook; select `A1:C5`, including headers.
2. Open the existing **BESHStat.WasmSmoke** task pane, select **Kruskal–Wallis** with **Dunn** enabled, alpha `0.05`, and **New Worksheet** output. Enable the existing p-value highlighting preference where available.
3. Verify H = **7.875**, corrected H ≈ **8.1603260869565215**, corrected p ≈ **0.016904709225411534**. Check that the **Kruskal–Wallis** table appears before **Dunn's** table and that the primary table's two p-value cells and Dunn's p-value column carry formatting.
4. Save output or screenshots and record client, browser, result-sheet name, start positions and formatting, including any divergences from the Windows policy.

### Batch gate

- [ ] New Common snapshot and serializer tests pass (not independently run in the patch-creation environment).
- [ ] Inventory verifier passes (performed by the patch author against the supplied source).
- [ ] Existing Core, Common, and legacy tests plus full Windows build pass.
- [ ] Windows ANOVA + Welch/Tukey smoke passes.
- [ ] Excel Online Kruskal–Wallis + Dunn smoke passes.
- [ ] Golden metrics, ordered tables, and p-value metadata reconciled; any intentional delta recorded before approval.

**Not assessed in this batch:** live Mac desktop Excel, production Office add-in deployment, Office custom-function execution, complete optional Windows ANOVA extras, Workbook creation in Office.js, semantic styling parity, user-approved overwrites, and model-specific UI wiring. The source review alone cannot close those gates.

## 5. API compatibility, rollback and next step

**Changes to production API/statistics/host behavior: none.** The single existing project edit is to `BESHStat.Excel.Common.Tests.vbproj` to copy the golden JSON fixtures to test output; all other additions are test/documentation/support files. No modification is made to numerical algorithms, `BESHStat.Excel.Common`, WinForms, Ribbon XML, Office.js scripts, or legacy transitional OfficeJs classes. Reverting Batch 00.01 requires undoing that test `.vbproj` change and deleting the added test/fixture/docs/verifier files.

Once the developer has validated the build/tests, run both manual smokes and approved the inventory and semantic snapshots, proceed to **Batch 00.02 — stable analysis IDs and descriptors**, preserving numeric enum compatibility. The proposed IDs above are for review only and are **not** serialized/API additions in this batch.
