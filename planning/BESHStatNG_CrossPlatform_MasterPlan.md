# BESHStatNG — Cross-platform Excel application master development plan

**Revision:** 1.4 · 18 October 2026 (planning-only Phase 04A insertion; no code changes).

**Source of truth:** the current checked-out Git repository. Uploaded packages, including `BESHstatNG(20261018-140903).zip` and the accepted 02.06 package, are historical reference context; subsequent checked-in source and planning updates are authoritative.

**Implementation checkpoint:** **00.01 through 02.06 accepted/merged**, including 00.02A/B and 01.05A (**25 completed batches**). Last accepted/merged batch: **02.06**, confirmed by the user after Windows and Excel Online acceptance. Accepted 02.04 Windows Excel-DNA and 02.05 Excel Online new worksheet C10 evidence remain recorded; Mac acceptance is not claimed.

**Current planning status:** **02.07** is the current authorized implementation batch, pending independent review and live-host acceptance; it is not complete or accepted. See [02.07 composite planning and host limitations](../docs/architecture/batch-02-07-composite-output.md). **04A.01** remains dependent on completing 04.07.

**Batch count:** **222 total** (original 148 + 74 new Phase 04A), **25 completed**, **197 planned/outstanding**, including active 02.07. None of the 04A batches is implemented.

**Scope:** Excel-DNA and Office.js/WASM implementation remain the active workstream. LibreOffice is an *architectural portability consideration only*, explicitly not part of implementation batches or Excel release criteria.  
**Supersedes:** revision 1.2 (16 October 2026); the full original multi-phase roadmap and dependencies are retained below, updated for repository progress and portable-host readiness.

## Executive recommendation

Continue the existing *two-host, one-engine* design, but establish reusable contracts and registry/discovery before migrating more UI handlers. Preserve Windows Excel-DNA as a working .NET Framework 4.8 production application. Create a new **BESHStat.OfficeAddin** production-oriented Blazor WebAssembly/Office.js host, leaving **BESHStat.WasmSmoke** intact as the small independent Core/WASM regression harness. The statistical algorithms remain in **BESHStat.Core**; analysis coordination, validation, output semantics, chart specifications, and function argument conversion belong in **BESHStat.Excel.Common**; COM/Excel-DNA and Office.js remain thin adapters. Avoid translating WinForms screen-by-screen.

**New catalog decision (14 October):** `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` is the **canonical authored catalog of user-facing BESHStatNG analysis methods and their help/navigation identity**. It is already used by `BESHstatNG/tools/generate_help_links.py` to generate Windows `HelpLinks.vb` and `HelpContext.vb`. It supersedes the **manually maintained Ribbon CSV as an analysis-method inventory**. But the actual `Resources/RibbonXML.xml` remains the authority for what the current Windows Ribbon *really declares*, and `<ExcelFunction>` attributes in the VB source remain the authority for the UDF export surface. A build-time reconciler/generator should compare these sources; the historical CSVs become optional derived audit reports, not a separate maintained catalog. Stage 00.02's `AnalysisCatalog` is already present and currently has two descriptors; its **display/help metadata**, but **not its handler implementations**, should eventually derive from `methods.yml`.

The new Phase **04A**, inserted after 04.07, is a mandatory Windows portable-code completion and selected feature-enhancement gate. It must avoid duplicate implementation in Phases 05–11. The plan is deliberately **incremental**: finish the foundational architecture and parity checks first, then build a real reusable UI, then expand analysis families, then graphics and custom functions, and finally release engineering. A late feature may be piloted earlier to validate architectural assumptions, but it must not bypass stable shared contracts.

## 0. Current repository checkpoint and historical 16 October review

**18 October package re-audit (Revision 1.4):** The uploaded ZIP contains `InputSizeLimits.vb`, bounded Windows/Office range readers, `batch-01-10-large-range-acquisition.md`, and 15 new Common/8 legacy/24 JavaScript tests described in that note. That note records Core **1,024**, Common **251**, legacy **561**, Office.js **74**, and Python **21+5+5**, plus successful Debug WASM and VS 2026 x86/x64 builds. I also independently ran `npm test` on the extracted Office.js tests in this review environment: **74/74 passed**. This container has no `dotnet` CLI or live Excel, so VB test/build and GUI successes are **documented repository results**, not fresh independent re-runs. Revision 1.3's “01.10 in progress” language below is retained as history and superseded by this checkpoint. Existing sources confirm Core/Common `netstandard2.0`; modern tests and WasmSmoke `net10.0`; Windows Excel-DNA and legacy tests Framework 4.8.


**18 October repository update:** 01.05A is implemented in `HostCapabilities.vb`, range/cell contracts and compatibility tests. Identified non-Excel spreadsheet hosts can pass capability preflight through `IsSpreadsheetHost`; Excel compatibility flags, A1 APIs and date policies remain. 01.06 adds `LongitudinalInputs.vb`, `LongitudinalParsers.vb` and `LongitudinalParserTests.vb` for survival/repeated/mixed input validation. 01.07 adds `ModelInput.vb`, `ModelInputCompiler.vb` and `ModelInputCompilerTests.vb`, reusing Core formula/design services with aligned source rows and typed factors. These are Common input contracts, not new host UI or fitted-analysis handlers. Core/Common remain `netstandard2.0`; Windows remains .NET Framework 4.8. No LibreOffice implementation is authorized.

The repository review ran the portable verification script: **Core 1,024 passed; Common 208 passed; zero failures/skips**. Office bridge Node tests: **29 passed**; method catalog validation, generated metadata `--check` and retired-helper audit passed. This checkpoint does not certify live Windows/Online/Mac Excel behavior. Infrastructure verification commands and limitations are documented in `docs/architecture/development-validation.md`.

**Batch 01.08 completion:** commit `5d5986d` implements **Matrix, Classification, PSM, SPC and Chart-Source Input Parsers** in `AdvancedInputs.vb`, `AdvancedInputParsers.vb` and `AdvancedInputParserTests.vb`. These are additive Common input contracts, not fitted analyses or host UI. See [batch 01.08 architecture and acceptance notes](../docs/architecture/batch-01-08-advanced-inputs.md) for dimension, class/treatment, order and source-row validation, native error/missing-value preservation, and deterministic matrix/SPC/PSM preview fixtures. The note records final Core **1,024 passed**, Common **231 passed** (including 23 new tests), JS **29 passed**, and a successful WASM Debug build with zero warnings/errors. Python validation and legacy MSTests remain incomplete because Python discovery / `ResolveHelpPython` failed; live host smoke checks were not run. These are recorded implementation results, not tests rerun for this planning-only update. Core/Common remain `netstandard2.0`; no LibreOffice implementation is authorized.

**Batch 01.09 accepted checkpoint:** commit `2d9f418` implements lossless Windows/Office acquisition adapters, canonical parity fixtures and the selected-areas preview, following .NET 10 migration commit `caba668`. Recorded validation: Core **1,024**, Common **236**, legacy **553**, Office.js **50**, Python **21 + 5 + 5** passed; WASM and Windows x86/x64 packaging built. See [01.09 evidence and host limitations](../docs/architecture/batch-01-09-acquisition-adapters.md). Core/Common remain `netstandard2.0`, modern tests/WasmSmoke `net10.0`, and legacy host/tests .NET Framework 4.8. Live host acceptance remains separate. **Historical 01.09 note, superseded by Revision 1.4:** batch 01.10 was then in progress; its implementation and full validation are recorded in the uploaded 18 October package.

**Historical 16 October review follows.** Its counts and runtime limitations describe that review environment, not the current repository. Later completed batches supersede its proposed next steps.

**What changed since revision 1.1.** The source package contains the implemented method catalog validator and deterministic generated metadata; the typed handler registry; typed read-only analysis-result artifact envelope; removal/guard of the old Windows Office.js placeholders; host capability preflight; range/document/selection references; explicit cell/error/date policies; configurable grouped parsing and structured diagnostics; typed scalar/vector/pair/group parsers; and rectangular frame, typed-category and contingency-count/pair parsers. The original ANOVA and Kruskal–Wallis services remain the two executable shared-analysis examples. Catalog entries for the other documented methods do not execute automatically.

| Delivered batch | Source evidence | Review outcome |
|---|---|---|
| 00.01–00.02 | `Stage304ContractSnapshotTests.vb`, five golden JSON fixtures; `AnalysisIds.vb`, `AnalysisDescriptor.vb`, `AnalysisCatalog.vb` and catalog tests | **Implemented**; frozen compatibility fixtures should remain regression gates |
| 00.02A–00.02B | `docs/_meta/methods.yml`, `validate_methods_catalog.py`, `generate_analysis_metadata.py`, `GeneratedMethodMetadata.vb`, generator and UDF documentation tests | **Implemented**; YAML validator and generated-metadata `--check` pass; 268-source-UDF parser coverage is asserted by tests |
| 00.03–00.04 | `IAnalysisHandler.vb`, `AnalysisHandlerRegistry.vb`, registry tests, `AnalysisResultDocument.vb` and result document tests | **Implemented**; typed registry provides ANOVA/KW handlers, legacy façade remains; typed detail is intentionally not a serializable general-purpose DTO yet |
| 00.05–00.06 | `audit_transitional_officejs.py`, `HostCapabilities.vb`, capability tests, JS host bridge | **Implemented**; retired Windows placeholder classes absent; **host preflight still requires `IsExcel=True`**, a future portability constraint |
| 01.01–01.03 | `SpreadsheetRangeReferences.vb`, `CellValuePolicy.vb`, `SpreadsheetValidation.vb`, `ConfigurableGroupedNumericParser.vb` and associated tests | **Implemented**; A1 bounds/date systems/error names currently express Excel conventions |
| 01.04–01.05 | `TypedNumericInputs.vb`, `TypedNumericRangeParsers.vb`, `RectangularCategoricalInputs.vb`, `RectangularCategoricalParsers.vb`, `SpreadsheetCoreTableBridge.vb`; detailed batch architecture notes and tests | **Implemented**; input types preserve source coordinates and typed categories; these are contracts, **not** new GUI/model/Office executions |

**Independent checks performed in this review:** methods manifest validator **PASS** (76 method entries, 84 Ribbon buttons, 79 mappings, five utility commands); generated metadata `--check` **PASS**; retired-helper audit **PASS**; Python tooling unit tests **21 passed**, MkDocs Python tests **5 passed**; `officeBridge.js` syntax **PASS**; Office bridge Node tests **29 passed**. These check only their stated scopes. A .NET SDK and Excel/LibreOffice desktop runtimes were not available here: **no new Core/Common/legacy MSTest, Visual Studio build, Excel GUI smoke or LibreOffice runtime validation was performed**. Accept the user-reported completed batches as the development checkpoint, not as independently certified full-platform behavior.

**Architecture review conclusion:** no need to redo any completed batch. Preserve the `netstandard2.0` Core/Common boundary, generated `methods.yml` metadata and source-native typed registry. The cross-host contract adjustment proposed by the historical review (01.05A) is now implemented, followed by 01.06 and 01.07. See §2B and §13 for the retained architectural rationale.

## 1. Basis and limitations of the audit

The **previous 14 October baseline** was inspected, including `.vb`, `.vbproj`, `.razor`, `.js`, `.xml`, YAML help metadata, source declarations, solution/project manifests, tests and Ribbon XML. This is **static source analysis**, not an independent successful Visual Studio/Excel/Mac run; developer-reported builds, tests and smoke tests (including successful 00.01/00.02) are accepted as the working baseline but were **not rerun** for this proposal. The 14 October ZIP did not contain a separate WiX installer project, so MSI design can be discussed based on the Windows host and known process but installer details are **not re-audited** in this checkpoint. Likewise, a Mac desktop Excel smoke test is not established by this ZIP. Inventory counts describe **source declarations**, not a guarantee that every exposed function/control is semantically independent.

**Historical 14 October inventory (superseded by §0 for implemented batches; do not use the old "currently" wording below to determine present status):**

| Component | Confirmed current contents | Status / observation |
|---|---|---|
| `BESHStat.Core` (`netstandard2.0`) | Numerical/data/statistical modules, 12 portable graphics calculation files, `ResultTable` and `ResultTableOutput`, common settings/logger/progress contracts | Broad portable calculation foundation; some legacy files still carry `Option Strict Off`, so modernization must be incremental |
| `BESHStat.Excel.Common` (`netstandard2.0`) | Stage 3.04 services plus 00.02 `AnalysisIds.vb`, `AnalysisDescriptor.vb`, `AnalysisCatalog.vb`; still uses the original two-case dispatcher | ID/descriptor catalog implemented for ANOVA and Kruskal–Wallis; handler registry is next, not yet implemented |
| `BESHStat.Excel.Common.Tests` (`net8.0`) | Existing `SharedExcelLayerTests.vb`, new `Stage304ContractSnapshotTests.vb`, `AnalysisCatalogTests.vb`, and five `TestData/Stage304/*.json` fixtures | 00.01/00.02 artifacts found in ZIP; build/test pass reported by developer, not executed in this audit |
| `BESHStat.WasmSmoke` (`net9.0`) | `Home.razor`, `wwwroot/js/officeBridge.js`, `office-manifest.xml` | Working Stage 3.04 spike, not production UX/deployment architecture |
| `BESHStatNG` | Windows Excel-DNA / .NET Framework 4.8, interop writer and importer, major WinForms/graphics code, Ribbon and Excel UDF modules | Retain as production host and parity oracle |
| `BESHStatNG.Test` and `BESHStat.Core.Tests` | Legacy regression suite and portable numerical tests | Keep both; expand app/Office/browser coverage |
| Windows Ribbon | **84** `<button>` declarations in current `RibbonXML.xml` | Real registration authority for Ribbon; reconcile automatically with `methods.yml` (76 authored methods / 79 mapping entries, 78 unique IDs) |
| Excel-DNA UDFs | **268** `<ExcelFunction>` declarations in current source; `generate_udf_docs_v2.py` currently extracts only **264** | Code annotations define exports; fix the four missing PSM documentation entries and generate any audit inventory automatically |

**Previous CSV audit inventories are retained only as historical snapshots.** The revised plan does **not** require editing/maintaining either CSV. Future inventories should be reproducibly generated from `methods.yml`, `RibbonXML.xml` and `<ExcelFunction>` declarations, together with a discrepancy report. The old UDF complexity labels are preliminary and should not be considered validated classifications.

### Source landmarks from the 14 October baseline (superseded changes marked in §0)

- `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml`: **76** method entries, **2** utility/help pages, **12** curated categories; presently drives generated Windows help mapping but not Ribbon construction or UDF docs generation.
- `BESHstatNG/tools/generate_help_links.py`: consumes `methods.yml`, generates `src/Help/HelpLinks.vb` and `src/Help/HelpContext.vb`; requires a clean/validated method schema.
- `BESHStat.Excel.Common/src/Analysis/AnalysisIds.vb`, `AnalysisDescriptor.vb`, `AnalysisCatalog.vb`: **00.02 implemented**, with two stable IDs and registered descriptors; metadata currently duplicated from documentation YAML.
- `BESHStat.Excel.Common.Tests/src/Stage304ContractSnapshotTests.vb`, `AnalysisCatalogTests.vb` and `TestData/Stage304/*.json`: **00.01 and 00.02** compatibility artifacts in the new source.
- `BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb`: `SpreadsheetAnalysisInput`, `SpreadsheetCellValue`, `SpreadsheetRangeData`, `SpreadsheetOutputTarget`.
- `.../Input/GroupedNumericRangeParser.vb`: two-or-more numeric columns, a mandatory first-row header assumption, blank-skip semantics and finite numeric validation.
- `.../Analysis/SpreadsheetAnalysisContracts.vb` and `SharedAnalysisApplicationService.vb`: enum `OneWayAnova`, `KruskalWallis`, switch dispatcher, mutable request; generic `Detail As Object` and metrics dictionary.
- `.../Analysis/OneWayAnovaApplicationService.vb`, `KruskalWallisApplicationService.vb`: typed application services, Core computation and multi-table results.
- `.../Output/ResultTableWriteRequest.vb`: `Object` rows, result-table row/column metadata, p-value targets/Excel number-format string, alpha, output destination, `AutoFit`.
- `BESHStat.Core/src/DM/ResultWriter/ResultTable.vb`, `ResultTableOutput.vb`, `PValuePresentation.vb`: portable table semantics and established result-table writer inheritance.
- `BESHstatNG/src/DM/ExcelDnaSharedApplicationAdapter.vb`: converts legacy `MultiGroupsUnpairedData` into shared `GroupedNumericData`.
- `BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb`: actual COM writes, formatting, and `WriteSharedRequest`.
- `BESHstatNG/src/UI/UibyID.vb`: current ANOVA/Kruskal paths through Common; variance tests, descriptive statistics, box-plot and comparison graphics still orchestrated in Windows GUI.
- `BESHstatNG/src/DM/OfficeJsDataImporter.vb` and `.../ResultWriter/OfficeJsResultWriter.vb`: older VB Office.js-oriented transitional helpers **inside Windows project**, not the current Blazor/Office.js implementation. `OfficeJsDataImporter.vb` explicitly sets `Option Strict Off`.
- `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js`: selected-range read/typing, sheet naming, p-value/formatting, multiple result-table writes; rejects `NewWorkbook` explicitly.
- `BESHStat.WasmSmoke/Pages/Home.razor`: UI uses an analysis `switch`, typecasts `Detail`, prepares settings/output, runs Core synchronously and writes via JS interop; still a prototype.
- `BESHStat.WasmSmoke/office-manifest.xml`: development task-pane-only XML manifest with HTTPS localhost, minimum ExcelApi 1.1, no custom-functions declaration or production command surface.
- `BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb`: multiple `ConcurrentDictionary` model-handle caches; UDF state, lifecycle and recalc need an explicit design, not a simple signature translation.

## 2. Historical pre-implementation gap audit (14 October; see §0 and §2B for current disposition)

| Priority | Finding and evidence | Consequence | Decision |
|---|---|---|---|
| **P0** | Dispatcher remains a two-value `Select Case`; **00.02 has already introduced** two stable IDs and `AnalysisCatalog`, but without handler registration | Two hosts still require wiring changes for every feature | Continue with 00.03 typed handler registry; do not reimplement 00.02 |
| **P0** | `SpreadsheetAnalysisResult.Detail As Object` and free-form double metrics | Lost compile-time type guarantees and poor typed serialization for model/plot payloads | Move to typed result/artifact union, explicit metric metadata, optional per-analysis typed detail accessible through typed handlers |
| **P0** | Windows ANOVA supplements are still UI-calculated in `UibyID.vb`; equivalent UI options absent in WASM demo | Same analysis name can produce different output by host | Separate complete analysis *workflow orchestration* from GUI; include optional reports and chart specs in service |
| **P0** | `ResultTableWriteRequestFactory.CreateMany` does not own table spacing: Office.js adds a blank row, Windows caller adds blank rows | Output layout duplicated and likely to drift | Introduce immutable ordered output-document/placement plan with precomputed coordinates and overlap bounds |
| **P0** | `officeBridge.js` applies p-value color/formatting and result-table style directly; Windows writer does the same in COM | Presentation policy can diverge | Shared semantic formatting/style tokens; host translates tokens into its API calls |
| **P0** | Older Windows `OfficeJsDataImporter` / `OfficeJsResultWriter` coexist with new Common DTOs and the actual JS bridge | Competing representations and misleading host ownership | Mark legacy helpers transitional, add compatibility tests, then relocate/replace or delete only after consumers are gone |
| **P1** | Range DTO holds sheet name/address but no stable workbook/sheet identity, multiple ranges, orientation/header/missing policies or selection revision | Nontrivial analyses and workbooks fragile | Define source provenance, multiple-range sources, data schemas and validation diagnostics |
| **P1** | `GroupedNumericRangeParser` always expects a header row and >=2 numeric columns | Won't cover no-header, long-format grouping, unequal types or other family-specific ingestion | Keep legacy behavior as named preset; add configurable parsers with parity tests |
| **P1** | Core's `CoreDataTable` already stores typed/untyped matrices, row IDs and source metadata; Common now has a second rectangular `SpreadsheetRangeData` | Duplication if both become analysis domain models | Treat `SpreadsheetRangeData` as raw transport, `CoreDataTable` as computational tabular representation, and typed Common inputs as schema-validated interpretations |
| **P1** | Raw output `Object` cells and string replacements `#Pinf`/`#Ninf`/`#N/A` are not a fully defined typed Excel-value/error contract | Formula-looking strings, Excel error and nonfinite semantics are ambiguous | Explicit output-cell discriminated kind (number/text/boolean/blank/error/formula), proper Excel error mapping and test fixtures |
| **P1** | `NewWorkbook` exists in shared destination enum but Office.js writer explicitly throws | Host capability difference | Expose capability and fallback; research/test product-specific new-workbook behavior rather than promising unsupported UI |
| **P1** | `OfficeJsDataImporter.vb` has `Option Strict Off`; Windows `.vbproj` also defaults Strict Off | Risk of portable late-binding propagation | Set Strict On for all **new** Common/Core/UI-independent modules; modernize transitional files in bounded patches |
| **P1** | No production Office add-in project, reusable UX, shared runtime/custom-functions manifest or deployment pipeline | Prototype shouldn't accumulate production-specific complexity | New `BESHStat.OfficeAddin` plus adapter/functional test assets; preserve WasmSmoke |
| **P1** | Model UDF handles are cached in-process in the Windows add-in | Workbook reopen/host change/multi-instance behavior not portable | Explicit session-scoped handle manager, invalidation and recalculation strategy |
| **P2** | Additional Windows chart modules `ROC.vb`, `NormalPlot.vb`, `XYZscatter.vb`, `GifAnimator.vb`, `ChartExport.vb` mix host graphics and calculations | Risk of treating graphics migration as just chart rendering | Isolate last portable plot calculations then define chart-spec/renderer boundary |

**Non-negotiable early rule:** No major new analysis screen may directly instantiate a Core model, assemble Excel output tables or apply statistical formatting policy; it must call a Common handler and host adapter. Existing Windows code remains until parity is proven.

## 2A. Authoritative method metadata audit and compatibility plan (14 October baseline; 00.02A/B now delivered)

> **Historical audit only:** references below to missing scripts, inaccurate ribbon IDs or a 264-UDF parser describe the **14 October** starting state, **not** the inspected 16 October state. §0 contains the latest verified status.

### 2A.1 What is authoritative, and for what purpose?

**Decision: use `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` instead of the Ribbon CSV for authored analysis-method identity, title, category, short description, help slug, and GUI mapping.** It is an existing maintained semantic catalog and already has proven consumers. Its method `key` (e.g. `OneWayANOVA`) identifies a documentation method; a new optional `analysisId` should bridge to the stable execution identifiers implemented in batch 00.02 (`anova.oneway`, `nonparametric.kruskal-wallis`). Do not equate a documentation `key` with the newer runtime `AnalysisId`, do not rename the existing keys, and do not reassign the already published runtime IDs.

| Surface | Authoritative source | Derived/validated outputs | Why |
|---|---|---|---|
| User-facing analysis identity, help pages, descriptions and method category | `docs/_meta/methods.yml` (authored) | Windows generated help topics; Office catalog display metadata, navigation and help URLs | Existing metadata contains 76 method records and help linkage |
| Windows Ribbon registration and exact callbacks | `BESHstatNG/Resources/RibbonXML.xml` (deployed source) | Validation of `methods.yml.ribbonIds` and `onActions`; optional generated audit CSV | Ribbon XML, **not** YAML, is what Excel-DNA actually loads |
| Cross-platform executable analysis handlers, options, type contracts and supported capabilities | `BESHStat.Excel.Common` typed handler registrations + descriptors | Runtime registry; validation against YAML `analysisId` | YAML cannot supply compiled handlers or reliable host capabilities |
| Actual exported worksheet functions, signatures and XML-doc summaries | VB `<ExcelFunction>` attributes and XML doc comments under `src/ExcelUDFs/` | Generated UDF manifest and docs; optional catalog references to methods | 268 functions cannot be inferred from 76 dialog/method records |
| Curated help navigation and independent method pages | `mkdocs.base.yml` and content markdown (currently manually maintained) | Optional navigation/slug checks; future generated nav when explicitly adopted | The runner does not currently generate navigation from `methods.yml` |

`methods.yml` is **not currently the authoritative executable runtime catalog or Ribbon XML generator**. Transition in stages rather than declaring the data in sync before checking it.

### 2A.2 Historical findings from the 14 October ZIP (resolved items are now tested)

- Parsed `methods.yml`: **76 unique method keys, 76 unique slugs, 2 non-method pages, 12 categories**, and **78 curated category-navigation entries** (three intentionally repeated method references: Kaplan–Meier, correspondence analysis, Cox regression; the Bullet Chart method is absent from that curated list). The YAML has **79 `ribbonIds` entries but only 78 distinct IDs**. All 76 method slugs currently resolve to `docs/methods/<slug>.md`; this does not imply complete source coverage because `resampling.md` and `family-link-covmat.md` are additional top-level method pages not represented as YAML methods.
- Current Windows Ribbon XML defines **84 buttons**. YAML currently omits **five utility buttons** that are not analysis methods: About, Help, Settings, Show Log and Export Chart. These need not become analysis methods; keep them as `pages`/`commands` with explicit semantics. `pages` already includes `GlobalSettings` and `ExportChart`, but has no mapping to the corresponding Ribbon buttons.
- Four **agreement-method** Ribbon entries are inconsistent between YAML and actual Ribbon XML: Bland–Altman has `Ui9ANOVA2nested` instead of `btn10_3Bland`; Lin CCC has `UiTwoInputRefedits` instead of `btn10_4LCCC`; Cohen Kappa has `UiTwoInputRefedits` instead of `btn10_5Cohen`; ICC has `btn10_3icc` instead of `btn10_6icc`. The two `UiTwoInputRefedits` values are form names, not Ribbon IDs. The curated `categories[].items` records contain some of the correct IDs but are not consistently synchronized with `methods[].ribbonIds`.
- The Bullet Chart method has `onActions: [Ui01BulletChart]` and a matching `forms[].handler`, while Ribbon XML declares **`OnbtmBulletPressed`**. Preserve `forms[].form: Ui01BulletChart`, but correct the action/handler mapping using the actual production callback.
- Exactly **79 YAML method-to-Ribbon mapping entries** would be expected to correspond to 79 analysis buttons in XML (84 total minus five utilities). They do not currently match, so **YAML cannot yet be used as an unvalidated generated registry**. Correct these fields, then fail CI on broken method references, duplicate Ribbon IDs across unrelated method mappings, unknown Ribbon IDs and mismatched callback targets.
- `generate_help_links.py` reads `methods.yml` to produce `HelpLinks.vb` and `HelpContext.vb`, using `key`, `slug`, `title` and `forms[].form/analysisTitle`. It does **not** validate `ribbonIds` or `onActions`. Adding optional `analysisId`/cross-platform display fields will not break this generator if existing fields and shapes are retained. Regenerate Windows help files whenever form/help mappings change, and test form-title conflicts and fallback behavior.
- The current `AnalysisCatalog.vb` created in **00.02** has hard-coded English labels, categories and full `latest` URLs for the two analyses; these overlap with `methods.yml`. **Never replace the stable runtime IDs or typed options with YAML-driven dynamic dispatch.** Generate/cross-check immutable descriptive metadata at build time while leaving strong type registration in VB.
- The source contains **268 `<ExcelFunction>` exports**. The current `generate_udf_docs_v2.py` and generated `docs/udf/*.md` account for **264**. Four functions are absent as headings: `BESH.PS.SCORES`, `BESH.PS.WEIGHTS`, `BESH.PS.EFFECT`, `BESH.PS.TABLE`. The parser's `parse_udfs_from_vb()` always advances to *later* lines when finding the attribute close, even if `<ExcelFunction(...)>` ends **on the same starting line**; it can inadvertently swallow the following single-line attribute. Fix its tokenization before relying on generated UDF counts. UDF `HelpTopic:=HelpLinks.FallbackBaseUrl & ...` expressions are also not picked up by the current quoted-string-only help-topic regex; use semantic method associations instead of attempting to evaluate arbitrary VB expressions.

### 2A.3 Original `methods.yml` changes proposed on 14 October (**now implemented; retained for provenance**)

1. **Repair verified stale mappings** listed above and the missing Bullet Chart category reference; keep the method `key`/`slug`/forms and existing published help URLs stable.
2. Extend **only already-implemented** methods with optional `analysisId`, aligned with `AnalysisIds.vb`. Initial examples: `OneWayANOVA → anova.oneway`, `KruskalWallisTest → nonparametric.kruskal-wallis`. A missing `analysisId` means **documented method not yet registered in Common**, not a broken record; assign future IDs when each handler is implemented and explicitly approved.
3. Preserve `categories` as curated navigation (including deliberate cross-listings) and `methods[].categories` as classification; enforce that category entries reference known method keys and that declared Ribbon mappings reconcile with deployed XML. Add an explicit `commands` subsection or extend `pages` for About/Help/Settings/Show Log/Export Chart, **without** treating them as analysis handlers.
4. Optional future fields should be additive and well-defined: e.g. `featureKind` (`analysis`/`chart`/`tool`) and `udfNames`/`udfPatterns` for **curated** method-to-UDF cross-links. Avoid a 268-function hand-maintained duplicate list in YAML. Keep runtime availability, Core type names, options factories and function signatures in code or generated source manifests.
5. Validate uniqueness of `key`, `slug`, `analysisId` (when supplied), canonicalized help URL, `ribbonIds` where applicable, handler existence, referenced file existence and normalized form-title disambiguation. Do not silently resolve conflicting metadata by taking the first entry. Detect and report unknown/unimplemented analyses without failing simply because a documented feature has not yet been ported.

**Example of an additive field for an existing entry (illustrative only):**

```yaml
- key: OneWayANOVA
  analysisId: anova.oneway
  title: One-Way ANOVA
  slug: one-way-anova
  # existing short, ribbonIds, onActions, forms and categories remain
```

**Important compatibility rule:** the YAML is the **authored descriptor metadata authority**, and the Ribbon and UDF declarations are **verified implementation sources**. Do not make the VB runtime parse YAML dynamically on every calculation. For .NET Standard 2.0/.NET Framework 4.8/WASM use generated checked-in strongly typed constants/DTO metadata (or a build-generated static JSON asset with schema validation) and a separately compiled typed handler registry.

### 2A.4 Effect on documentation scripts — actual call-graph audit

| File | Current behavior | Changes required **if adding `analysisId` only** | Recommended future improvements |
|---|---|---|---|
| `BESHstatNG/tools/generate_help_links.py` | **Directly reads** `methods.yml`; emits `HelpLinks.vb` and `HelpContext.vb` | **No parser change**; preserve `key`, `slug`, `forms`, and re-run generator after fixing existing metadata | Preflight YAML schema + Ribbon callback checks; deterministic output independent of timestamp where practical; regression checks for `Form`+title collisions |
| `BESHStatNG_Help_MkDocs/mkdocs_beshstatng.py` | Version-aware MkDocs serve/build runner; reads `Directory.Build.props`, writes inherited config, runs MkDocs | **No functional change needed**: currently does **not read** `methods.yml` | Add optional `--validate-methods`/pre-build validator invocation, with fail-fast errors for broken slugs/navigation/IDs; keep versioned URL behavior unchanged |
| `BESHStatNG_Help_MkDocs/generate_udf_docs_v2.py` | Reads VB `<ExcelFunction>` declarations/XML doc comments; optional `--method-links-yaml` accepts a **different** schema: `patterns`/`groups`; currently does **not read** `methods.yml` | **No change** unless you want to use the new canonical method catalog for UDF↔method cross-linking | **First fix** the one-line attribute parser (268/268 fixture), then add explicit `--methods-yml` support for `key`/`slug`/optional UDF associations and validated relative links; preserve optional legacy `--method-links-yaml` overrides; do not evaluate `HelpTopic` VB expressions; retain code XML doc comments as canonical UDF argument/return help |
| `BESHStatNG_Help_MkDocs/mkdocs.base.yml` | Static MkDocs navigation including method/UDF pages; not generated from `methods.yml` | **No change needed** for extra YAML fields | Add a validation comparison or **optional** navigation generation later; preserve manual subpages and special groups (MMRM/LMM), and independently documented `resampling.md`/`family-link-covmat.md` |
| `tools/validate_methods_catalog.py` and `tools/generate_analysis_metadata.py` | **Implemented in 00.02A/B** | Maintain as dedicated read-only validator / deterministic generator, not runtime YAML loading | Parse YAML once; validate against XML, Common IDs, docs and UDF source; emit deterministic checked-in metadata plus optional CSV discrepancy reports |

`generate_udf_docs_v2.py --method-links-yaml docs/_meta/methods.yml` is **not** a correct migration shortcut: it expects root-level `patterns`/`groups` fields and will not automatically interpret `methods`. The new `--methods-yml` would be explicitly distinct and additive. Regression-test old generation without `--methods-yml`, generation with it, and preservation of links (including source slugs and category titles).

### 2A.5 Original build and acceptance sequence (**completed, retained for provenance**)

- **00.02A:** first fix/validate `methods.yml` and add read-only manifest validation with tests; do not change the 00.02 runtime catalog API. Run generation of `HelpLinks.vb`/`HelpContext.vb` only if the YAML help/form mapping corrections require it, and confirm Windows About/Help/Settings still resolve correctly. Compare YAML method button mappings to **all 84** XML buttons with five explicit utility exclusions.
- **00.02B:** add a deterministic metadata projection from YAML and parity tests against 00.02 `AnalysisCatalog` and its existing two IDs; fix and test UDF parser completeness (268/268) and optionally enable UDF docs cross-links via `--methods-yml`. Avoid merging a generated all-feature executable registry: unported method rows are metadata-only. Keep generator run as an explicit development/CI step, not a runtime YAML dependency.
- **00.03 (unchanged conceptual purpose):** build the typed handler registry on top of the now reconciled catalog metadata; use the already frozen 00.01 JSON fixtures to prove ANOVA/Kruskal–Wallis equivalence.

## 2B. Long-term LibreOffice/Calc feasibility and shared-library design constraints (16 October 2026)

**Decision: feasible in principle, not yet demonstrated.** The attached `LibreOffice_Extension_Strategy.txt` proposes LibreOffice Calc (Windows/macOS) with Python/UNO (PyUNO) as a host, calling existing .NET `BESHStat.Core` code, ideally without reproducing statistics in Python. This is compatible with the architectural *intent* of Core and Common: platform-neutral numerical algorithms and spreadsheet application contracts under host-specific acquisition/writer/UI layers. LibreOffice publishes a UNO spreadsheet range API with data-array read/write operations. It does **not** prove the proposed Python/.NET bridge, binary compatibility or chart/UDF features work end-to-end. **No LibreOffice/PyUNO/bridge batches are included in this plan**, and no LibreOffice project is to be added before the Office.js Windows/Mac product is complete.

**Future integration boundary only (informational):**

```text
                         BESHStat.Core  (portable algorithms)
                                 ↑
                    BESHStat.Excel.Common (.NET Standard 2.0)
                    (shared spreadsheet/application semantics)
                       ↑                             ↑
              Excel host adapters            Possible future Calc host
             Excel-DNA / Office.js       (external .NET invocation + UNO)
                              [not in current roadmap]
```

**The 16 October review identified five portability seams (01.05A now implements the targeted contract hardening; retain these constraints in later batches):**

1. **Host identity is not a capability.** Implemented in 01.05A: `HostCapabilitySnapshot.Supports()` and `HostCapabilityPreflight.CheckHost()` admit identified hosts through `IsExcel` or explicit `IsSpreadsheetHost`, then check actual capability IDs. The existing `IsExcel` field remains a compatibility indicator. Do not advertise a capability merely because the host is a spreadsheet. This contract is not evidence of an implemented non-Excel adapter.
2. **Separate addresses/limits from mathematical inputs.** `SpreadsheetRangeAddress` uses Excel A1 syntax and `16384` columns / `1048576` rows; its existing `FromAddress` is useful for Excel. Keep those as an **Excel address dialect/host bounds profile**, not a universal `SpreadsheetRangeReference` limit. Neutral parsed ranges should store row/column coordinates and source identifiers; future adapters normalize their local qualified addresses before Core/Common analysis. Avoid introducing Calc syntax into Common in this Excel delivery.
3. **Represent dates with explicit origin or normalized value.** `SpreadsheetDateSystem.Excel1900/Excel1904`, phantom leap-day treatment, and `ExcelDateSerialConverter` are correctly Excel-specific. LibreOffice’s UNO number-format settings expose a configurable `NullDate`. Future hosts must supply a defined date basis (or normalized `DateTime` value); never silently reinterpret a Calc serial using Excel1900. Keep existing Excel default behavior unchanged.
4. **Keep cell/error/category provenance semantically neutral.** `SpreadsheetCellKind.CellError` and type-tagged categorical values are reusable. `SpreadsheetExcelErrorValue` and Excel error spellings should be regarded as one host’s error-code mapping, not the universal taxonomy. A future adapter must preserve blank vs empty text, boolean, number, formula result, error, missing policy, locale and source row identity before invoking the current parsers. UNO’s `getDataArray` alone may not retain every one of those distinctions; host-specific cell inspection may be necessary.
5. **Keep analysis/report/chart contracts separated from renderers.** Core numerical classes, typed Common handlers, warnings, result documents, future semantic output/style plans and portable chart specs must contain **no** COM, Excel-DNA, Office.js, UNO, Python or filesystem requirements. The host handles range read, document lifetime, output placement, native charts, image insertion, UDF registration, GUI and settings persistence. Do not put `Host = Excel` conditions inside statistical algorithms or validation rules.

**Calling .NET from a future LibreOffice host:** Python.NET can load the desktop CLR (`coreclr`) and .NET assemblies on Windows/macOS, *subject to a compatible CPython process, runtime loading, architecture and deployment*. It is not proof that it can be safely embedded in LibreOffice’s actual Python interpreter, nor that a local .NET runtime or dependency packaging will be trivial. A separate local .NET worker with a versioned JSON/IPC contract is a plausible lower-risk fallback if embedding conflicts with LibreOffice. The present Blazor browser WASM bundle relies on the browser/JavaScript bootstrap and **cannot simply be loaded by a generic Python Wasmtime/Wasmer package as though it were a portable standalone .wasm library**. A distinct runtime/ABI experiment would be needed *after* the Excel add-in release; do not promise zero-install dependency behavior for the LibreOffice design.

**Decision guardrail:** retain project/namespace names for compatibility (`BESHStat.Excel.Common`, `ExcelCommon`); focus on **host-neutral semantics**, not a costly rename or an unrequested LibreOffice port. Modern .NET consumers can reference `netstandard2.0` libraries, but true compatibility is conditional on referenced dependencies and actual runtime tests. A future UNO-facing adapter should be able to reuse all numerical code and most Common parsing/orchestration/output semantics, while its UI, range acquisition, UDF registration, date coercion and chart renderer necessarily remain independent.

**Use existing future batch acceptance gates to prevent accidental Excel lock-in:**

| Planned work | Portable-design acceptance addition (no Calc implementation) |
|---|---|
| **01.05A (complete)** | Removed the *logical requirement* that a host must be Excel; documented value/date/address policies and preserved existing Excel behavior. Tests use simulated non-Excel capability reports, not a live UNO adapter. |
| 01.06–01.08 | Survival, repeated, formula, time-series and multivariate parsers consume typed values and source row/column IDs, not Excel formulas/COM ranges. |
| 01.09–01.10 | Excel readers translate from host data into neutral snapshots; Common size/time quotas use requested host bounds, not fixed Excel limits as universal truths. |
| 02.01–02.08 | Shared result layout/format/style intents are workbook-neutral; native writer APIs and host-specific formats stay in adapters. |
| 03.01–03.06 | Persistent settings, file logs, progress, workbook lifecycle, run IDs and fault messages use interfaces/DTOs rather than Windows/browser globals. |
| 04–09 | Shared method catalogs, options and analysis handlers do not import Office.js, DOM, Excel Range or WinForms types. |
| 10.01–10.15 | Portable chart specs preserve data/annotations; native Excel rendering and future Calc rendering are **both adapter choices**, not constraints on specs. |
| 11.01–11.19 | UDF math/argument/error/spill semantics are portable where possible; Excel-DNA and Office Custom Functions registrations remain host implementations. |
| 12–13 | Excel Office add-in release tests remain Excel-only. Do not silently claim LibreOffice support without a separate later product validation track. |

## 3. Target project architecture and ownership

```text
BESHStat.Core (.NET Standard 2.0)
  statistics, numeric methods, matrices, model fit/result, chart geometry
  ├── BESHStat.Core.Tests
  └── BESHStat.Excel.Common (.NET Standard 2.0)
       schemas, parsing, validation, analysis catalog and orchestration,
       result document/chart specs, shared settings, UDF argument/result semantics
       ├── BESHStat.Excel.Common.Tests
       ├── BESHStatNG (Windows .NET Framework 4.8 Excel-DNA host)
       │    COM Range/workbook adapter, WinForms + ribbon, native Excel charts,
       │    Excel-DNA registration, Windows file/settings/deployment
       │    └── BESHStatNG.Tests (existing legacy tests)
       ├── BESHStat.OfficeAddin (production Blazor WASM host)
       │    reusable task pane, JS/TypeScript Office adapter,
       │    rendering, Office commands, Office Custom Functions runtime bridge
       │    ├── BESHStat.OfficeAddin.Tests (unit/component/JS adapters)
       │    └── E2E host test harness (Excel Online/Windows/Mac)
       └── BESHStat.WasmSmoke (small independent compatibility harness)
```

**Architecture decisions:**

1. **Do not rename `BESHStat.Core` namespaces en masse** merely because portable classes retain the `BESHStatNG` root namespace. Migration can preserve binary and source compatibility; namespace cleanup, if any, is a separate, low-priority task.
2. **Do not expand `BESHStat.WasmSmoke` into the whole product.** It combines demonstration UI, Office.js transport and experiment status logic in one `Home.razor`. Keep a tiny independent runtime smoke project and create production `BESHStat.OfficeAddin` with testable layers. Move reusable code through tests, not wholesale copying.
3. **Use a practical monolith for Common initially:** feature folders within one Common assembly, not many tiny packages. Split only if measured technical need appears. Production add-in may have a TypeScript adapter subproject if it materially improves type safety and mocks.
4. **Version Common contracts explicitly** via schema/contract version. The Windows and WASM hosts are built together but production web assets may be cached independently.
5. **Portable Core must not know Excel:** it can define generic statistical result/table/plot payloads. Excel-specific destination semantics belong exclusively to Common. Hosting/capability declarations belong to each adapter.

### Ownership matrix

| Concern | Core | Common | Windows host | Office host |
|---|---|---|---|---|
| Numerical/statistical methods | **Own** | Call | — | — |
| Model fit, covariance, residual numerical values, plot geometry | **Own** | Compose into artifacts | — | — |
| Input interpretation, missing/header/category rules | Core generic data containers | **Own spreadsheet semantics, parsers, validation** | Acquire COM cells only | Acquire Office.js cells only |
| Formula/design model construction | Core parser/algorithms | **Own user-facing model request conversion** | WinForms controls | Blazor controls |
| Analysis registration, options, application workflow | — | **Own** | Invoke | Invoke |
| Statistical result tables, p-value semantics | **Own raw result / numerical metadata** | **Own output document/format instructions** | Apply COM writes | Apply JS writes |
| Workbook/worksheet/range operations | — | Neutral contracts + capability model | **Own COM** | **Own Office.js** |
| Chart specs/series/labels/visual semantics | Plot primitives as appropriate | **Own portable chart document specs** | **Own COM renderer/export** | **Own Office native/SVG/Canvas renderer/export** |
| UDF math | **Own** | **Own argument conversion, errors, spill schema** | Excel-DNA registration | JS custom functions + WASM bridge |
| GUI state/options/help/catalog/validation | — | **Own rules and view schemas** | **Own WinForms UI** | **Own Blazor UI** |
| Runtime settings/logging/progress/cancel | Neutral interfaces where already in Core | App-level policies and service orchestration | Windows persistence/UI adapters | Browser persistence/UI adapters |
| Ribbon, event hooks, clipboard, file access, installer | — | Capabilities and intentions | **Own** | **Own where supported** |

Do not push Office.js API objects, WinForms types, COM `Range`, JavaScript promises or dynamic `Object` invocation into Common. DTOs are intentionally transport-friendly and strongly typed. A primitive `Object` field may remain only at established Core/table boundaries with explicit validation and serialization adapters, never as an uncontrolled dispatcher input.

## 4. Shared analysis design

**Preferred pattern:** **the already implemented 00.02 descriptor catalog**, complemented by a handler registry, typed per-family input/options and an immutable common result envelope. Author public-facing labels/help identity in `methods.yml`; compile typed bindings in Common.

- `AnalysisId` is a stable **string identifier** (e.g. `anova.oneway`, `nonparametric.kruskal-wallis`, `regression.glm`). Avoid relying on the numeric enum as serialized identity. Maintain the current enum as a compatibility adapter until consumers are migrated.
- `IAnalysisDescriptor` / `AnalysisDescriptor` (**implemented in 00.02 for two analyses**): stable identifier, display/category, input schema, option schema/defaults, output kinds, help URL, required host capabilities, estimated cost, version and availability/state. The public-facing metadata must be validated against (and preferably generated from) `methods.yml`; typed options/handlers remain in code.
- `IAnalysisHandler` with a non-generic invocation boundary for registry storage, alongside strongly typed `IAnalysisHandler(Of TInput,TOptions,TDetail)` implementations; validate declared types **before** invocation, avoiding late binding or reflection-based untrusted dispatch.
- `AnalysisRequest`: analysis ID, validated input refs or typed input, typed options, execution context (alpha/culture/random seed), output preferences held separately, cancellation/progress, schema version and request ID. Keep request data immutable or defensively copied once validated.
- `AnalysisResultDocument`: ordered tables, metrics (key + label + unit + number/presentation role), diagnostics, warnings, error/fault codes, elapsed time, optional prediction/residual blocks, provenance, convergence details, chart specifications and optional typed detail. Errors should not be encoded solely as thrown exception text.
- The planned typed handler registry registers handlers **explicitly** at compile time; don't require DI containers or runtime reflection inaccessible to older .NET/WASM. A simple hand-written registration list is acceptable, optionally generated once registry stabilizes.
- Phased registry migration: wrap ANOVA/Kruskal–Wallis first, preserve their existing signatures and numerical outputs; have Windows + WasmSmoke use the new façade; **only then** add new analyses through the registry.
- Handle long runs through task-based API and cancellation callbacks where the Core routine supports them. A synchronous Core algorithm cannot be truthfully 'cancelled' just by closing a spinner; distinguish *requested* from *cooperative* cancellation and expose support metadata.

**Data flow:**

```text
Excel range(s) ──host adapter──> SpreadsheetRangeData / source refs
                                 │
                                 ▼
                     Common parser + validation
                                 │
                                 ▼
                       Typed analysis input
                                 │
                                 ▼
                      Catalog/handler dispatch
                                 │
                                 ▼
                            BESHStat.Core
                                 │
                                 ▼
                  AnalysisResultDocument + charts
                                 │
                                 ▼
                    Common output-layout planner
                                 │
                      capability negotiation
                        /             \
                   Excel-DNA         Office.js
                 COM renderer       workbook/UI renderer
```

## 5. Shared input design

**Transport:** `SpreadsheetRangeData` represents a rectangular cell snapshot; extend with workbook/worksheet identifiers, qualified address, source orientation, top-left coordinates, source selection ID/revision and header/formula/error metadata where available. Add `SpreadsheetRangeSet` for multiple selections, including source identity. Don't assume a workbook will remain active during a long calculation. An Excel 1900/1904 date-system flag is needed for date/time semantics when these are relevant; numerical date serials must not be confused with unitless numeric data.

**Generic computational bridge:** preserve Core's `CoreDataTable`, which already includes `ObjectMatrix`, `NumericMatrix`, `MissingMask`, `RowIds`, `DataSourceInfo`. Common converts validated spreadsheet snapshots into such Core containers or family-specific domain inputs; don't add a competing general-purpose table engine.

| Shared shape | Main consumers | Critical validation/semantics |
|---|---|---|
| `NumericScalarInput`, `NumericVectorInput` | Sample-size, assumptions, distributions | finite/missing rules, minimum N, scalar bounds |
| `PairedVectorInput` | paired tests, agreement | pairwise row alignment; **not** independent dropping of missing values |
| `GroupedNumericInput` long/wide | ANOVA, rank tests, charts | labels, header or no header, ragged groups, group order |
| `RectangularDataFrameInput` | descriptive, LM/GLM, PCA, PSM | mixed numeric/categorical column schema, row IDs, missing policy |
| `CategoricalInput`, `ContingencyInput` | proportions, Fisher/RxC, kappa | case sensitivity, counts/nonnegative integers, levels, optional weights |
| `FormulaModelInput` | LM/GLM/GEE/mixed | response/predictors, contrasts, interactions, intercept, reference levels |
| `SurvivalInput` | KM/log-rank/Cox | time, status/event coding, strata, entry/start/stop where supported |
| `RepeatedSubjectInput`, `MixedModelInput` | RM tests, GEE, MMRM, LMM | subject uniqueness/visit ordering/cluster IDs, covariance choices |
| `MatrixInput`, `ClassificationInput` | PCA/multivariate, ROC | dimensions, column order, class labels, row mapping |
| `SpcTimeSeriesInput` | SPC | time/order, subgrouping, phase/reference limits, rational subgroups |
| `PsmCohortInput` | PSM | treatment coding, propensity covariates, outcome, row identity |
| `ChartSourceInput` | all chart families | series IDs, categorical/numeric axes, missing and sorting rules |

**Validation conventions:** shared `ValidationIssue` code + severity + field/path + row/column + localization key; warnings distinguish dropped/excluded records from fatal errors. Explicit policies for empty string, Excel blank, error value, Boolean, date serial, locale decimal, nonfinite doubles, missing category and header auto-detection. Address provenance should be display-only; do not use raw user text as formula or executable expression. Support empty/merged/hidden cells and large-range size warnings when applicable. Benchmark reading in bounded batches where Office.js proxies impose overhead.

**Legacy adapter migration:** preserve `TwoGroupsData`, `MultiGroupsUnpairedData`, `MmrmData`, UDF `UdfDataImport*`, `ExcelDnaDataImporter` while thin converters are introduced and verified. Migrate **input semantics**, not merely the class names. Use parity tests to document differences between GUI and UDF cleaning policies rather than silently 'standardizing' them.

## 6. Full result/output-writing contract

Use one shared `OutputDocument` / `OutputPlan` with ordered `OutputBlock`s rather than sending unrelated tables with repeated targets. An output block can be `Table`, `Text`, `Note`, `Diagnostic`, `Chart`, `Image`, `Link`, and if supported `Formula`. Each block has a relative start and calculated width/height. An `OutputLayoutPlanner` converts blocks into absolute row/column bounds **before any host calls**; output scopes specify current sheet/new sheet/new workbook, exact start cell, spacing, collision policy and supported capabilities. Avoid host code independently computing spacing.

**Cell types:** `Blank`, `Number`, `Text`, `Boolean`, `Error`, `Formula` (explicit opt-in and escaping); distinguish a literal `"#N/A"` from an Excel error and represent NaN/infinity through a defined policy. Retain the Core `ResultTableOutputModel` as the input to the converter; do **not** duplicate the underlying statistical table calculation.

**Presentation:** shared table-style tokens (title, column/row headers, body, footer, note, diagnostic), role-based number-format/percent/decimal/scientific/p-value instructions, highlight threshold and optional accessible visual cues; optional border/merge/width/height/auto-fit specifications, hyperlinks, footnotes and warning blocks. P-value significance uses a single shared alpha/predicate, and both hosts convert format instructions to their own API values. Protect empty source cells; require user-approved overwrite, a preview and protected/merged-range checks where feasible. Do not rely on color alone to communicate significance.

**Capability policy:** New workbook, chart insertion, sheet activation, formula/conditional formatting and export may differ by Office client/version. Expose host capabilities and fallbacks at runtime; `NewWorkbook` currently throws in the JS bridge and should remain disabled in the UI unless a tested implementation exists. The Office.js adapter should batch writes into a small number of `Excel.run/context.sync` operations and provide a structured `WriteResult` with actual sheet/address/partial failure details.

**Cross-host parity fixture:** store canonical normalized output-plan JSON for identical analysis inputs and settings. Compare semantic instructions **before** Excel rendering; then smoke-test visual/placement fidelity in both hosts. Host-specific font/width pixel differences may be allowed if documented, but no silent loss of table, p-value or warning content.

## 7. Graphics: calculations, specification, rendering

**Layer 1:** Core remains responsible for bins, densities, ECDFs, data transformations, regression/survival/ROC/SPC plot metrics, axis bounds, error bars, polygons and series geometry. The 12 current Core graphics files include `HistogramBinningCore`, `ViolinPlotCore`, `CumulativeDistributionPlotCore`, `PolarPlotCore`, `SankeyPlotCore`, `ConvexHullPlotCore`, `DumbbellPlotCore`, `LadderPlotCore`, `BulletChartCore`, `KiteChartCore`, `CategoricalHistogramCore`, `ChartAxisScalingCore`.

**Layer 2:** Common `ChartDocument` / `ChartSeries` / `AxisSpec` / `ScaleSpec` / `LegendSpec` / `AnnotationSpec` / `ShapeSpec`, plus dataset/semantic roles, accessible description and preferred output mechanism. This is **not** a serialized Excel COM chart. Units/coordinates should be explicit, and both SVG and Excel renderers consume the same precomputed geometry/payload where relevant.

**Layer 3:** Host renderer. Use native Excel chart types where supported and helpful for workbook editing; use SVG/Canvas/HTML for advanced geometry and insert a static image into Excel when appropriate. Preserve a task-pane preview and data table even if image insertion is unavailable. Do not promise 3D native chart equivalence between hosts.

| Plot family | Preferred Office path | Comment |
|---|---|---|
| Simple scatter/line/column; paired/trend plots; many SPC trend charts | Native Excel chart when API available | Spreadsheet-editable series, scale and legend; capability-test each formatting feature |
| Histogram, cumulative plots, ROC, normal plot, survival step charts | Native line/scatter or SVG where necessary | Data/step/smoothing and confidence-bound fidelity determines choice |
| Box/violin; histogram + horizontal Tukey box | SVG/Canvas, optional native/chart hybrid | Box-polygon alignment and consistent resize fidelity |
| Polar, kite, convex hull, Sankey, ladder, dumbbell, bullet | SVG/Canvas by default; native when simple | Complex paths, ribbons, annotation layers |
| PCA scores/loadings, biplots, CA/MCA, agreement, PSM Love, multiple comparisons | Native scatter/line if faithful; SVG for dense labels/custom intervals | Keep consistent axes, confidence regions, label rules |
| 3D XYZ, scatterplot matrix, animations/GIF | Dedicated SVG/Canvas/WebGL preview or static fallback | Avoid claiming native Excel parity; performance/renderer selection later |
| Chart export | Browser-side SVG/PNG when rendered in task pane; Office-host chart/image export where supported | PDF requires a distinct print/export provider; do not presume Office.js exposes general-purpose PDF saving |

**Deferred numerical audit:** `BESHstatNG/src/Graphics/ROC.vb`, `NormalPlot.vb`, `XYZscatter.vb`, `CalibrationPlot.vb`, `ScatterPlotMatrix.vb`, `SymetryPlot.vb`, `ChartExport.vb`, `GifAnimator.vb` may still have combined calculation/rendering responsibilities. Review individually before the corresponding chart port. Tests: deterministic plot-data fixtures, bounds/labels, SVG serialization validity, snapshot comparison, image placement after resize, chart export and host capability fallbacks.

## 8. UI design

Build **one Blazor task-pane application** with navigation/catalog/search, not dozens of independently ported WinForms dialogs. A feature screen is composed of shared components: `SelectionPicker`, `InputRolePicker`, `VariableList`, `Paired/GroupSelector`, `FormulaBuilder`, `AnalysisOptionsPanel`, `OutputTargetPicker`, `ResultPreview`, `WarningPanel`, `ProgressPanel`, `ChartPreview`, and contextual help. Large model builders may open larger dialogs or supported separate views; ordinary analyses remain within task pane.

Common owns option defaults, validation rules, input roles, feature metadata, help paths, execution rules and durable analysis state **DTOs**. Blazor owns component rendering, focus/keyboard accessibility, navigation, transient form state and visual feedback. Windows forms remain intact at first; optionally consume shared validation later. Range selection is a host service (current selection, explicit address, worksheet picker) with a 'reacquire/refresh data' action. Never assume the selection stays unchanged between preview and run.

Localization: start with English UI plus locale-sensitive numeric display and a Slovak Excel smoke fixture. Store **numeric values** independently of their displayed localized strings; translate label/help/error keys later. Treat multi-workbook, selection changes, inactive sheets and stale selections explicitly. Provide progress and cancellation **only** where the called algorithm supports it. Hide options requiring unsupported Office APIs instead of allowing a late exception.

## 9. Excel UDF / Office Custom Functions strategy

Historical 14 October observation: **268 Excel-DNA functions** were declared but only **264** entered the UDF documentation parser. **Batch 00.02B has since corrected this:** the 16 October test suite asserts **268/268** and includes the four PSM examples. Keep this as a regression gate, not an open work item. A source-derived report can list every name/method; **do not assume** every Excel-DNA function has a direct JavaScript signature or that all should be public in v1. Preserve exact statistical logic by calling Core; never reimplement math in JavaScript.

1. First consolidate **UDF input and output behavior** in Common: numeric/categorical/matrix conversion, optional/omitted arguments, stable Excel error codes (`#VALUE!`, `#NUM!`, `#N/A` etc.), 1D/2D spill payloads, formula-reference vs copied values, missing handling, error propagation and output dimensions. Compare existing GUI and UDF conventions explicitly.
2. Add a `UdfCatalog` with name, version, signature, parameter schemas, return shape, help URL, volatility/caching hints and computational cost; **generate the baseline inventory from VB `<ExcelFunction>` source, not the obsolete hand-curated UDF CSV**. Link UDFs to known `methods.yml` keys through explicit, validated association fields, retaining VB XML comments as authoritative per-function docs. Generate Office custom-function metadata from the catalog where feasible, but do not force VB reflection into browser runtime.
3. **Office Custom Functions call a JS bridge that invokes a warm, reusable .NET WASM runtime.** First prove the bridge from real cells, then test startup cost, concurrent invocations, recalc behavior and memory. A separate pure-JS implementation of the same statistics is prohibited. A JS-only runtime cannot be assumed to host Blazor/WASM like a task pane; plan around a **shared browser runtime** or a validated worker/runtime bridge.
4. Pilot a small, stateless table/scalar set. Some array outputs are naturally dynamic-array spill functions; preserve their layout and test blocked spill cells. Favor reuse of the cached WASM instance over one runtime initialization per cell.
5. Explicit high-risk category: all **model-handle** functions (LM/GLM/GEE/Cox/LMM/MMRM/multivariate fit-and-query families, PSM), as shown by `ConcurrentDictionary` in the Windows UDFs. Session-scoped handles must have owner/workbook, expiration/invalidity, recalculation invalidation and memory limits; decide if each output should recompute deterministically or reference an in-memory model cache.
6. Classify candidates **provisionally**: low/medium = stateless descriptive/distribution/simple tests and fixed-size tabular outputs; medium = contingency, matrices, agreement, plot-data tables and rich arrays; high = formula-based regressions, model-handle families, large multivariate/PSM computations, dynamic result schemas or heavy recalc. Confirm risk on each function's actual code before its batch.
7. Long-running custom functions can be asynchronous/cancelable. In shared runtimes, intensive synchronous work may block the task-pane UI. Benchmark a worker or chunked orchestration if needed. A Custom Function must not mutate unrelated workbook cells as a side effect.

**Microsoft Office constraint to validate at implementation:** custom functions and task panes may be separate runtimes unless a shared runtime is configured; a shared runtime can coordinate state but incurs startup/memory/UI-blocking tradeoffs. For production, Microsoft's September 2026 samples still caution that custom functions under the **unified manifest** are preview, so prefer the supported add-in-only XML manifest for the initial production custom-function release, revisiting unified manifest later. See links in §17.

## 10. Infrastructure, lifecycle, reliability

- **Settings:** Core already owns `BeshStatNgSettings`, `PValuePresentationSettings` and infrastructure interfaces. Common should own host-neutral settings DTOs, defaults validation and versioned migrations. Windows persists through `BeshStatNgSettingsStore` (XML); browser adapter through approved Office/browser storage as appropriate. Isolate per-user vs per-workbook settings. Never auto-copy Windows local paths to Mac/Web.
- **Diagnostics:** reuse `IAppLogger`, `IErrorReporter`, `IProgressReporter`; register host-specific log sinks, sanitized export and correlation/run ID. Do not log raw worksheet data by default; telemetry is opt-in (if ever adopted) and separate from local diagnostics.
- **Cancellation/asynchrony:** Common execution coordinator with state (`pending/running/cancel-requested/completed/failed`), progress updates and cooperative cancellation tokens/adapters. Evaluate Core algorithms individually for cancellation points; don't fabricate interruptibility. Avoid COM calls from background .NET worker threads; marshal Windows workbook actions back to Excel thread.
- **Workbook lifecycle:** handle workbook close/switch, sheet rename/delete, selection changes, protected sheets, canceled output confirmation, stale cache, runtime unload/recalc, and chart placement. Office.js event availability must be capability-tested, and event handlers removed on disposal.
- **Culture:** invariant serialized numeric payloads, Excel-localized display formats, date serial-system policy, configurable decimal display, test Slovak formatting and non-English worksheet naming.
- **Help/version/about/update:** Common publishes feature help IDs and application version metadata; Windows retains current About/installer update mechanism. Office add-in uses deployment/manifest version plus runtime asset version and release notes. Do not download/execute installers from WASM.
- **Security:** strict HTTPS, least permissions, safe error messages, no formula injection or unsafe HTML, content-security policy as permitted by Office webviews, explicit handling of potentially sensitive workbook contents, dependency audit and safe asset caching/versioning.

## 11. Deployment and environment

Keep the existing Windows Excel-DNA MSI, 32/64-bit packaging, DLL packing/signing and its regression pipeline **separate**. New production Office host will have versioned HTTPS static assets (Blazor WASM including .NET runtime and Core/Common assemblies), a production XML manifest, command/Ribbon entry, Office.js CDN dependency, icons, cache-busting and a controlled deployment/rollback strategy. Use dev/test/prod manifest IDs and origins so installations do not conflict.

Development: trusted local HTTPS + sideloading on Excel Online first, Windows desktop second, Mac desktop third. Production: check Office add-in-only XML manifest requirements, Office Marketplace or Microsoft 365 centralized deployment suitability, permissions, supported clients/requirement sets, and static hosting with immutable fingerprinted assets. Add a capability-negotiation screen/diagnostic page so unsupported features are disabled with a clear reason, not discovered mid-run. Test content updates against an already-open task pane and a stale cached manifest. Add browser/server-only release smoke tests and accessibility/keyboard checks.

**Host matrix:**

| Host | Target | What must be validated |
|---|---|---|
| Windows Excel-DNA | Existing production Windows Excel 32/64-bit | Full legacy parity, MSI, signature, numerical results, COM charting |
| Excel Online Office.js | First fully supported Office.js target | UI, reads/writes, charts/fallbacks, custom functions, deployment, browser limitations |
| Windows desktop Office.js | Second Office.js target | WebView/runtime/permission/performance differences and side-by-side Excel-DNA |
| Mac desktop Office.js | Required release target | Native Mac Excel Office APIs, permission/capability parity, fonts/chart/focus, deployment |
| Other clients (mobile/iPad) | **Not automatically in scope** | Explicit support decision based on runtime and API capability matrix |

## 12. Migration waves: why this order

1. **Foundation and current parity:** registry, input/output contracts, complete ANOVA/Kruskal workflows, Common tests.
2. **Production shell:** stable Office.js adapter and reusable UI with catalog/validation instead of incremental code added to WasmSmoke.
3. **Small and medium table-only workflows:** descriptive/assumptions, paired/grouped, rank tests, contingency/proportions, sample-size, agreement; fastest way to exercise each shared input shape.
4. **Formula/model and longitudinal workflows:** LM, GLM variants, GEE, survival/Cox, mixed models; introduces model builder, repeated observations and structured diagnostics.
5. **Large/multivariate and operational families:** PCA, CA/MCA, clustering, factor/discriminant, SPC and PSM; adds dimensional, chart and computation-cost concerns.
6. **Complete graphics:** start chart spec/native basics early, but mass-render graphics after stable analysis output and chart payload contracts.
7. **Custom Functions:** start manifest/WASM bridge **feasibility early**, mass-convert only after shared argument/result semantics; model-handle UDFs last.
8. **Hardening and release:** feature-by-host parity, Mac, localization, performance, deployment and release gates.

## 13. Detailed numbered implementation roadmap

**Notation:** W = Windows source changes; O = Office.js/WASM source changes; `Small` means compatibility/adapter edits; `Adapter` means focused host adapter changes; `Optional` only if parity exposes a need. Every batch is a **separate patch/apply/build/test/smoke/checkpoint**. For **COMPLETED** batches, test names refer to code/test assets now present; for remaining batches, they denote new required assertions. Dependencies refer to the batch IDs, with the validated Stage 3.04 baseline as starting point. File/class names are likely targets, not a commitment to create an inappropriate fixed folder before reviewing that batch’s latest ZIP. The acceptance column is the **release gate for that individual batch**.

### Phase 00 — Architecture locks and compatibility seam

**Phase exit:** Approved registry contracts, a runnable two-analysis parity baseline, a **reconciled methods.yml source of truth**, and a nonduplicated future migration pattern. **Current status: Phase 00 fully implemented through 00.06; do not repeat it.**

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **00.01 — COMPLETE** | Stage 3.04 golden contract snapshots; `Common.Tests/src/Stage304ContractSnapshotTests.vb`, five JSON fixtures | 3.04 | W=No O=No | Existing snapshot tests (developer reports passing) | Reported previously | Preserve numerical/output fixtures unchanged during 00.02A–B | **Low** |
| **00.02 — COMPLETE** | Introduced stable `AnalysisIds`, `AnalysisDescriptor`, `AnalysisCatalog` and `AnalysisCatalogTests.vb`; two descriptors | 00.01 | W=Small O=Small | Existing catalog uniqueness/type/legacy mapping tests (developer reports passing) | Reported previously | Do not redesign or reimplement; reconcile only public metadata | **Medium** |
| **00.02A — COMPLETE** | Correct and validate authored method manifest; `docs/_meta/methods.yml`; proposed `tools/validate_methods_catalog.py`; reconcile `Resources/RibbonXML.xml`, `mkdocs.base.yml`, `generate_help_links.py` | 00.02 | W=Help generator regeneration only if affected; O=No | YAML schema, 76 keys/slugs, 79 method Ribbon mappings, 84 XML button checks (5 commands excluded), action matching, nav refs, Bullet/Agreement regression tests | Open help for ANOVA, Bland–Altman and Bullet Chart in Windows; inspect categorization | YAML is trusted canonical method metadata with **zero unexplained mismatches**; 00.02 APIs unchanged | **Medium** |
| **00.02B — COMPLETE** | Project metadata generator and UDF doc completeness; proposed `tools/generate_analysis_metadata.py`; generated Common metadata file; `generate_udf_docs_v2.py` and its parser tests | 00.02A | W=Generated metadata/help only; O=Generated display catalog and links | Existing 2 `analysisId` links, generated help URLs/labels, deterministic regeneration, 268 UDF exports→268 docs, four PSM examples, old/new flag compatibility | Browse MkDocs help/related UDF pages and Office analysis catalog preview if available | No hard-coded competing user labels/help URLs, no change to numerics/dispatch, all UDF declarations represented | **Medium** |
| **00.03 — COMPLETE** | Build strongly typed handler registry and adapter; `Common/Analysis/IAnalysisHandler.vb; SharedAnalysisApplicationService.vb` | 00.02B | W=Small O=Small | Wrong types, unknown IDs, registry dispatch, parity | Run both existing analyses | Same result tables/metrics; no late binding | **Medium** |
| **00.04 — COMPLETE** | Define typed result artifact envelope, faults and warnings; `Common/Results/AnalysisResultDocument.vb; Contracts` | 00.03 | W=Small O=Small | Result schema/serialization; null/error cases | ANOVA with all posthoc and KW/Dunn | All report artifacts preserved; typed access | **Medium** |
| **00.05 — COMPLETE** | Audit/disposition transitional OfficeJs VB helpers; `Windows/DM/OfficeJsDataImporter.vb and OfficeJsResultWriter.vb; Common/Adapters` | 00.04 | W=Yes O=No | Golden old-new imports/writes; usage guard | Windows smoke on existing ANOVA/KW | Single authoritative Common input/output path; no regressions | **Medium** |
| **00.06 — COMPLETE** | Create capabilities contract and preflight metadata; `Common/Host/HostCapabilities.vb; Office bridge; Win adapter` | 00.03 | W=Small O=Yes | Capability fallbacks; unsupported destination | New-workbook option disabled in Office | Unsupported actions rejected before execution | **Low** |

### Phase 01 — Universal spreadsheet input and parsing

**Phase exit:** Every analysis family can be expressed in validated source-neutral input without rewriting import rules.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **01.01 — COMPLETE** | Add workbook/range identity, structured range selection; `Common/Contracts/SpreadsheetContracts.vb; host adapters` | 00.04 | W=Small O=Yes | Address parsing; source metadata; multi-worksheet | Read non-active named sheet | Stable source identity and source provenance | **Medium** |
| **01.02 — COMPLETE** | Implement canonical cell, missing, Excel error and date policies; `Common/Input/CellValuePolicy.vb; Core table bridge` | 01.01 | W=Small O=Yes | Blank/error/Boolean/date/locales/1900-1904 tests | Select mixed-type/error-containing range | Deterministic no-coercion interpretation | **Medium** |
| **01.03 — COMPLETE** | Common validation/reporting and configurable header/orientation; `Common/Input/Validation.vb; GroupedNumericRangeParser.vb` | 01.02 | W=Small O=Small | No-header; long/wide; bad coordinates; errors | ANOVA headers vs no-header sample | Legacy grouped preset preserved exactly | **Medium** |
| **01.04 — COMPLETE** | Scalar, vector, paired and grouped typed input parsers; `Common/Input/TypedNumericInputs.vb; TypedNumericRangeParsers.vb` | 01.03 | W=No O=No (Common-only delivery) | Pairwise complete cases; unequal groups; bounds | Paired and independent ranges preview | Typed and row-aligned payloads usable | **Medium** |
| **01.05 — COMPLETE** | Rectangular data frame, category and contingency contracts; `Common/Input/RectangularCategoricalInputs.vb; RectangularCategoricalParsers.vb; CoreDataTable bridge` | 01.03 | W=No O=No (Common-only delivery) | Categorical levels; row IDs; RxC shape | Select mixed predictor matrix | No dropped row IDs; exact category coding | **High** |
| **01.05A — COMPLETE** | Harden the *shared spreadsheet host boundary*, without implementing LibreOffice: `Common/Host/HostCapabilities.vb`, `Common/Contracts/SpreadsheetRangeReferences.vb`, `Common/Input/CellValuePolicy.vb`, related tests and brief compatibility note. Keep `IsExcel` and existing Excel APIs working; introduce neutral spreadsheet host/capability admission; specify host-specific A1 bounds, date origins, normalized cell error policy as extensibility seams (avoid implementing new dialect parsers now). | 00.06,01.05 | W=Compatibility smoke only O=Compatibility smoke only | Simulated non-Excel capability snapshot can pass supported operations while unavailable operations fail; existing Excel profiles unchanged; date/coordinate/error seam contract tests | Windows ANOVA/KW output and Office prototype selected range/new worksheet | No Excel requirement hidden in generic `Supports`/preflight; no calculation/parser output change; no UNO/Python/.NET bridge code | **Medium** |
| **01.06 — COMPLETE** | Survival/time/subject/repeated/mixed data parsers; `Common/Input/LongitudinalInputs.vb` | 01.05A | W=Adapter O=Yes | Censor/strata/ID/visit/cluster checks | Preview subject/visit data | All family-specific input constraints enforced | **High** |
| **01.07 — COMPLETE** | Formula/design matrix and model-input compilation; `Common/Input/ModelInput.vb; Core regression formula parser` | 01.05A | W=Small O=Yes | Contrasts; missingness; interactions; rank | Preview LM predictor/response roles | Canonical model input matches Windows model matrix | **High** |
| **01.08 — COMPLETE** | Matrix, classification, PSM, SPC and chart-source parsers; `Common/Input/AdvancedInputs.vb` | 01.05A | W=Adapter O=Yes | Dimensions; treatment; class labels; time order | Matrix/SPC/PSM preview fixtures | Validated inputs and source rows supported | **High** |
| **01.09 — COMPLETE** | Improve Office and Windows acquisition adapters; `Windows/DM/ExcelDnaDataImporter.vb; Office/js/RangeAdapter` | 01.01,01.05 | W=Yes O=Yes | Adapter equivalence; multiple ranges; empty cells | Choose 2 ranges on either host | Identical canonical source snapshots | **Medium** |
| **01.10 — COMPLETE** | Implement safe large-range acquisition and quotas; `Office range batching; Common/InputSizeLimits.vb` | 01.09 | W=Optional O=Yes | Chunking, memory and cancellation preflight | Read a large rectangular dataset | Bounded batching with informative limit messages | **High** |

### Phase 02 — Result document and output adapters

**Phase exit:** One layout/formatting plan drives both hosts and preserves all output data and metadata.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **02.01 — COMPLETE** | Create output-document/artifact model from ResultTable; `Common/Output/OutputDocument.vb; Core ResultTable bridge` | 00.04 | W=Small O=Small | Table/footer/title/diag/plot artifact fixtures | ANOVA multi-table preview | No table metadata loss or host API coupling | **Medium** |
| **02.02 — COMPLETE** | Define canonical cell and semantic style/p-value instructions; `Common/Output/OutputCell.vb; Styles.vb; PValuePresentation.vb` | 02.01 | W=Small O=Small | Error mapping; alpha; styles and locale formatting | Inspect p-value display in both hosts | Same semantic formatting document | **Medium** |
| **02.03 — COMPLETE** | Centralize ordered placement/overlap calculations; `Common/Output/OutputLayoutPlanner.vb` | 02.01 | W=Small O=Small | Rows/cols, separators, target, collision bounds | 4-table output starting at C6 | Identical planned blocks and coordinates | **Medium** |
| **02.04 — COMPLETE** | Refactor Windows WriteSharedRequest to consume output plan; `Windows/DM/ResultWriter/ExcelDnaResultWriter.vb; UI/UibyID.vb` | 02.02,02.03 | W=Yes O=No | Interop plan adapter tests; Windows parity fixtures | Multi-table ANOVA output and overwrites | Legacy styling/placement unchanged | **High** |
| **02.05 — COMPLETE** | Refactor Office.js writer to consume same output plan; `Office/js/officeBridge.js replacement; output bridge` | 02.02,02.03 | W=No O=Yes | JS mocked workbook adapter; normalized style tests | Multiple tables with p-value annotations | No policy/layout duplication in JS | **High** |
| **02.06 — COMPLETE** | Output capability, start-cell/new-sheet/new-workbook fallbacks; `Common/Output/TargetPlanner.vb; host capability adapters` | 02.04,02.05,00.06 | W=Yes O=Yes | Sheet collision, protected and unsupported cases | Current/new worksheet; unsupported new workbook | No unsupported option falsely advertised | **Medium** |
| **02.07** | Combine diagnostics, charts, footnotes, links in one plan; `Common/Output/CompositeOutput.vb; host insertion adapters` | 02.05 | W=Small O=Yes | Block placement and cross-type bounding | Table plus chart placeholder | Complete multi-artifact result with no overlap | **High** |
| **02.08** | Implement safe overwrite and structured write outcomes; `Common/Output/WriteResult.vb; two host adapters` | 02.06 | W=Yes O=Yes | Cancel/partial failure/undo capability; diagnostics | Cancel overwrite of occupied range | No unexpected destructive write | **High** |

### Phase 03 — Common infrastructure, lifecycle and execution

**Phase exit:** Defaults, fault handling, progress, cancellation and workbook/run identity consistent across hosts.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **03.01** | Create versioned app settings DTOs and preference store contracts; `Common/Infrastructure/Settings.vb; Core/AppSettings.vb` | 00.04 | W=Small O=Small | Defaults, migration, bounds, alpha parity | Change alpha and reopen add-in | Persisted and effective settings visible | **Medium** |
| **03.02** | Host-specific logging, diagnostics and run correlation; `Common/Infrastructure/Diagnostics.vb; host log sinks` | 03.01 | W=Small O=Yes | Redaction; run ID; noncrashing logger | Trigger expected validation error | No raw worksheet data logged automatically | **Medium** |
| **03.03** | Define execution state, task progress and cooperative cancel; `Common/Infrastructure/ExecutionCoordinator.vb; IProgressReporter` | 00.04 | W=Small O=Yes | State transitions; cancel-before-run; progress | Cancel a safe long-running fixture | Truthful cancellation support and no UI lock | **High** |
| **03.04** | Create stable fault-code mapping and user error model; `Common/Results/Faults.vb; Core fault adapters` | 03.02,03.03 | W=Small O=Yes | Exceptions vs validation/convergence faults | Trigger invalid model and retry | Clear actionable error without stack dump | **Medium** |
| **03.05** | Common help/version/feature availability metadata linked to validated `methods.yml`; `Common/Infrastructure/HelpVersion.vb; generated method metadata; Win HelpLinks.vb` | 00.02B,03.01 | W=Small O=Yes | Help URLs; unknown feature fallback | Open help from both hosts | No duplicated hard-coded help mapping | **Low** |
| **03.06** | Workbook/sheet/selection changes and disposal hooks; `Common/Host/DocumentLifecycle.vb; Win/Office adapters` | 01.01,03.03 | W=Yes O=Yes | Stale selection; close/rename/switch simulation | Change active sheet mid-preview | No writes to stale or wrong workbook | **High** |

### Phase 04 — Production Office.js host and reusable task-pane UX

**Phase exit:** Real OfficeAddin can run any registered basic analysis using generated/reusable screens and host services.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **04.01** | Create new production BESHStat.OfficeAddin project; `BESHStat.OfficeAddin.csproj; JS adapter; manifest-dev` | 00.03,00.06 | W=No O=Yes | WASM start/load; dependency injection smoke | Sideload and open task pane | Core+Common load in real Excel | **Medium** |
| **04.02** | Build typed Office adapter and mockable JS transport; `Office/Services/IWorkbookAdapter; wwwroot/js module` | 04.01,01.09 | W=No O=Yes | Mocked range/sheet/write JS tests | Read selected Excel range | No direct Office.js use in Blazor components | **Medium** |
| **04.03** | Build method-catalog navigation from `methods.yml` projections and reusable options UI; `Office/Components/AnalysisShell; OptionEditors` | 04.01,00.03,00.02B | W=No O=Yes | bUnit/component state, descriptor mapping | Select ANOVA then KW | Descriptor-based screens without duplicate switch | **Medium** |
| **04.04** | Range picker, preview, validation and output destination UI; `Office/Components/RangePicker; OutputPicker` | 04.02,01.03,02.06 | W=No O=Yes | Select and validation component tests | Pick, preview, run and write | Reusable controls across both current analyses | **Medium** |
| **04.05** | Reusable formula, subject/visit and categorical builders; `Office/Components/ModelBuilder and RoleEditors` | 04.04,01.06,01.07 | W=No O=Yes | Formula AST/options and UI validation | Build simple LM and repeated-data specification | No Core calls during visual option editing | **High** |
| **04.06** | UI settings/help/progress/errors/localization; `Office/Components/AppShell; Infrastructure adapters` | 04.03,03.01-03.05 | W=No O=Yes | State, resource and culture tests | Slovak Excel; change alpha; help | Consistent, accessible UI with failure recovery | **Medium** |
| **04.07** | Office ribbon commands, lifecycle and component E2E; `Office manifest commands; JS boot; Playwright harness` | 04.01-04.06 | W=No O=Yes | Command activation, UI binding, E2E | Click command in Excel Online then run ANOVA | Production shell and Office integration smoke green | **High** |

### Phase 04A — Finish Windows code migration and add selected Excel-DNA enhancements (new, 18 October 2026)

**Companion source ledger:** [Windows portability audit, 18 October 2026](BESHStatNG_Phase04A_Windows_Portability_Audit_20261018.md).

**Placement:** immediately after **04.07**, before **05.01**. This deliberately changes the execution critical path while preserving all existing Phase 05–13 batch IDs. The phase has **74 small batches**: **04A.01–04A.48** for comprehensive Windows migration inventory, incremental extraction and parity gate; **04A.49–04A.70** for the explicitly requested PDF enhancements; **04A.71–04A.74** for chart semantics, documentation, portable compatibility and exit validation. All 74 are **planned, not implemented**.

**Scope and ownership:** The existing Excel-DNA product receives the new features **now**, but the numerical tests/geometry belong in `BESHStat.Core`, analysis inputs/options/result documents/chart specifications in `BESHStat.Excel.Common`, and WinForms controls/COM chart placement only in `BESHstatNG`. A future Office.js host should invoke the same compiled services. This does **not** prematurely implement Office.js screens or LibreOffice integration, and it does **not** port WinForms visuals verbatim. Phase 04 must first prove the production Office shell, but Phase 04A features can be smoke-tested in Windows without adding UI in that shell.

**No false code-migration target:** A *complete portable-code extraction* is not deletion of every Windows file. Excel-DNA Ribbon registration, COM `Range` access, WinForms `.Designer.vb`, clipboard/file dialogs, Excel chart objects and MSI signing are legitimate Windows adapters and remain there. Conversely, any algorithm, model specification, statistical report rule, input validation, chart geometry or reusable settings policy found inside a form must be considered for Core/Common. `Option Strict On` and host-dependency guards apply to new portable code. Existing Core APIs must be reused when present, not copied.

**Source-derived audit priorities:** The 18 October ZIP has **32 non-designer UI source files (28,296 lines), 29 `.Designer.vb` files, 38 Windows Graphics `.vb` files and 41 `.vb` UDF sources**; the UI file scan counts **1,232 method declarations** (automated textual estimate, not independently verified functionality). The nested `UI/Refedit/Excel2007RefEdit.vb` also contains selection and worksheet lifecycle logic that belongs in the audit, although the control itself remains Windows-only. The largest behavioral forms include `Ui22MultivariateControlCharts.vb`, `Ui21ControlCharts.vb`, `Ui18MMRM.vb`, `UibyID.vb`, `UiGLM.vb`, `Ui11PCA.vb`, `Ui19LMM.vb`. Portable calculations are interspersed with COM input, local `ResultTable` composition and chart creation. `RegressionEffectsController.vb` uses WinForms `ListBox` despite reusable term-specification logic. `Graphics/NormalPlot.vb` calculates plotting positions and normal-score reference lines within an Excel-dependent class, and its constructor sorts the caller's input array in-place. `Ui82x2.vb` and `Ui0OneRefeditMulticol.vb` already expose OR, RR and all four requested ordinal association measures, making most of those additions **plot/report enhancement work**, not new statistical algorithms. `Assumptions.Grubbs` exists in Core; Friedman already computes internal `rankSums` and public `MeanRanks`, while `MCP` performs Conover/Dunn rather than Nemenyi. Searches found no named Dunnett/Hsu/Nemenyi implementation in Core/Common/Windows source; `Kolmogorov` appears only in descriptive text. **Cochran's Q and Cochran–Armitage are different tests from Cochran's variance-outlier C test**.

**Statistical validity gates (do not skip):** Dunnett requires properly calibrated *simultaneous* comparisons to a specified control, not multiple standalone t-tests. Hsu's MCB requires validated best-group selection, direction and simultaneous bounds rather than ordinary Tukey comparisons. Nemenyi critical difference must be correct for the chosen rank/blocked-design assumptions and multiplicity family; existing Dunn/Conover p-values cannot simply be plotted as a Nemenyi CD. A one-sample KS test against a **fully specified** CDF and a normality test against a **sample-fitted** normal CDF require different p-value calibration; parameter estimation must not silently use the fully specified-null KS p-value. Standard one-sample KS does **not** have a general degrees-of-freedom parameter; display sample size and explicitly documented reference parameters, and show df only for another well-defined statistic. For Grubbs, enforce assumptions (approximately normal, single extreme observation per test) and data-to-plot row identity. In 2x2 and RxC plots preserve existing CI method, orientation, sign and null reference (OR/RR=1, ordinal association=0). Numerical reference fixtures must be reviewed before enabling UI checkboxes.

**Selected PDF requests, and no more:** items 1, 2, 4, 5, 6, 7, 9, 13, 14, 15, 25, 38, 39 from `Content.pdf`. Other items in that 40-item PDF (pie charts, Sidak, REGWQ, BH, extra 2x2 tests, etc.) are *not authorized in this phase*. Page 34 also suggests CI/effect-size enhancements to paired t-test, but the user's selected request is specifically the **difference box plot**; those other changes remain outside this phase unless separately approved.

**Gate before 04A.49:** 04A.48 must establish no missing portable-code migration candidates and stable Windows numeric/output fixtures. New statistical methods should then be built from Core outward; Excel-only graph implementations must consume the Common chart payload rather than redoing the statistic.

| Batch | Objective and likely projects/files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **04A.01** | Inventory every Windows code path, including all WinForms code-behind/Designer event wiring, DM, 38 Graphics modules, 41 UDF sources, RegModels, AppInfrastructure, Help, update and Ribbon; `docs/architecture/WindowsMigrationLedger` | 04.07,00.02B | W=Audit O=No | Method/callback and file-to-owner audit; no unclassified method | Review categorized ledger and launch each Windows Ribbon family | Every reusable function assigned Core/Common or explicitly host-only; candidate count frozen | **Medium** |
| **04A.02** | Freeze representative Windows statistical/table/chart outputs and form option states as migration goldens; `BESHStatNG.Test`, Core/Common tests and reference datasets | 04A.01,00.01 | W=Tests O=No | Golden numerical/table/chart/option fixture serialization; no uncontrolled baselines | Run ANOVA, normality, 2x2 and PCA with saved workbook | Baseline includes data and exact settings, seed, alpha, culture, result and chart geometry | **High** |
| **04A.03** | Extract host-neutral univariate selection, grouped-input role and validation policy from `UI/UibyID.vb`; preserve RefEdit COM acquisition in Windows | 04A.02,01.03–01.05 | W=Yes O=No (contract tests) | Grouping with blank/error/header/orientation; error diagnostics parity | Run existing normality/descriptive/ANOVA in Windows | UI calls typed Common role validators; Core never receives Range or Form | **High** |
| **04A.04** | Extract paired/blocked data alignment and alternative/margin option policies from `UiTwoInputRefedits.vb`, `Ui0OneRefeditMulticol.vb`, `Ui9ANOVA2nested.vb` | 04A.02,01.04,01.06 | W=Yes O=No (contract tests) | Paired row alignment, blocks, missing and margins | Run paired t-test, Friedman and repeated-measures ANOVA | Common inputs preserve source-row mapping and existing results | **High** |
| **04A.05** | Consolidate 2x2/RxC/proportions table construction and ordinal/report mapping in Common; `Ui82x2.vb`, `Ui0OneRefeditMulticol.vb`, `Ui8Proportions.vb` | 04A.02,01.05 | W=Yes O=No (contract tests) | OR/RR orientation, integer counts, categorical order, alpha and CI parity | Run 2x2 and RxC from worksheet and in-dialog counts | No table statistics or numeric CI parsing remain in form code | **High** |
| **04A.06** | Extract assumptions, normality, outliers, homogeneity and descriptive supplementary report builders from `UibyID.vb` into Common | 04A.03,04A.02 | W=Yes O=No (contract tests) | Full existing Shapiro/D’Agostino/AD/Grubbs table and warnings parity | Run normality with optional descriptive table and box plot | Form handles view state only; stat/report assembly reusable | **High** |
| **04A.07** | Reconcile one-way/nested/repeated ANOVA, post-hoc, Welch and KW supplementary output pathways; `UibyID.vb`, `Ui9ANOVA2nested.vb`, Common handlers | 04A.03,04A.04,04A.02 | W=Yes O=No (contract tests) | Control comparison, unequal n, multiple-testing, output ordering goldens | Run all existing ANOVA/KW options | Windows results flow via Common without calculating supplemental statistics in UI | **High** |
| **04A.08** | Split model-effect authoring rules from WinForms `RegressionEffectsController.vb`, `UiGLM.vb`, `Ui13GEE.vb`, `Ui4Cox.vb` into typed Common model-spec service | 01.07,04A.01 | W=Yes O=No (contract tests) | Terms, factor contrasts, hierarchies, duplicate detection and canonical formula serialization | Select main effects and interactions in LM/GLM dialogs | Reusable model builder has zero ListBox, COM or WinForms dependencies | **High** |
| **04A.09** | Extract regression/model request/result policies for LM, GLM, NB2, ZIP, ordinal and multinomial GUI paths; `UiGLM.vb`, `RegModels/BinaryClassificationReporting.vb` | 04A.08,04A.02 | W=Yes O=No (contract tests) | Design/offset/weights, fitted/residual outputs, convergence fixtures | Fit Poisson and multinomial model with diagnostics | Numerical fitting in Core, validated request/diagnostics in Common | **High** |
| **04A.10** | Split LM/Gaussian and binomial GLM request/response/report projection from Windows regression entry points; `UiGLM.vb`, Core LM/GLM and Common model specs | 04A.09 | W=Yes O=No (contract tests) | NIST/GLM coefs, offsets and diagnostics regression cases | Fit LM and binomial GLM | LM/binomial projections use Common result-document contracts | **High** |
| **04A.11** | Split NB2/ZIP/multinomial/ordinal numerical option/report projection; Core statistical families, Windows regression UDFs/GUI | 04A.10 | W=Yes O=No (contract tests) | Link/dispersion/convergence, ordered class probabilities and residual goldens | Fit NB2 and ordinal model | No regression algorithm reimplementation in Windows UI | **High** |
| **04A.12** | Extract GEE cluster/time/correlation selection and report/contrast orchestration from `Ui13GEE.vb` | 04A.08,01.06 | W=Yes O=No (contract tests) | AR1/order/cluster/covariance/offset/robust-SE parity | Run existing GEE sample including residual output | No GEE-specific numerical preparation stranded in Windows form | **High** |
| **04A.13** | Extract KM/log-rank/Cox request, censoring, stratification and residual/plot payloads from `Ui4KMandLogRank.vb`, `Ui4Cox.vb` | 04A.04,01.06 | W=Yes O=No (contract tests) | Strata/ties/event coding and survival estimate equivalence | Run KM + stratified Cox with plot | Portable survival report/chart data; host only renders | **High** |
| **04A.14** | Extract MMRM/LMM effect, covariance, LS-means, reference-grid, contrast, optimizer and diagnostics policy from `Ui18MMRM.vb`, `Ui19LMM.vb`, `RegModels/MixedModels` | 04A.08,01.06–01.07 | W=Yes O=No (contract tests) | KR/Satt covariance and reference-grid fixtures; invalid option validation | Run MMRM and random-slope LMM in Windows | Common model workflows own options/report semantics, not Windows controls | **High** |
| **04A.15** | Extract covariance and fitting algorithm choices for MMRM and LMM into separate Common model options; `Ui18MMRM.vb`, `Ui19LMM.vb` | 04A.14 | W=Yes O=No (contract tests) | REML/ML and UN/CS/AR1 optimizer option validation | Fit MMRM under AR1 | Windows forms only project validated covariance requests | **High** |
| **04A.16** | Extract LS-means, pairwise/reference grids, KR/Satt and structured diagnostics to typed Common result mapping | 04A.15 | W=Yes O=No (contract tests) | Model-based contrasts and warning/effect order goldens | Fit random-slope LMM with LS-means | Host-independent mixed-model diagnostics and outputs | **High** |
| **04A.17** | Extract PCA/CA/MCA, cluster, discriminant and factor selection, priors, row labels and plot payload preparation from `Ui11PCA.vb` | 04A.02,01.08 | W=Yes O=No (contract tests) | Scaling, missing policy, factors, priors, deterministic seeds/labels | Run PCA biplot and k-means with row labels | Portable input/report/plot builders cover all six multivariate branches | **High** |
| **04A.18** | PCA, CA and MCA input transform and score/label mapping for shared results; `Ui11PCA.vb` | 04A.17 | W=Yes O=No (contract tests) | PCA scaling/row ID and CA/MCA table consistency | Run PCA and CA on reference input | Reusable multivariate table/plot projection | **High** |
| **04A.19** | K-means and hierarchical clustering initialization/distance/standardization and membership output mapping; `Ui11PCA.vb` | 04A.18 | W=Yes O=No (contract tests) | Seeded clustering, missing, empty cluster and dendrogram fixtures | Run both cluster methods | Stable reproducible Common clustering requests/specs | **High** |
| **04A.20** | Factor analysis and discriminant analysis factor/rotation/prior/validation option mapping; `Ui11PCA.vb` | 04A.19 | W=Yes O=No (contract tests) | Rotation/retention/priors confusion matrices and row labels | Run factor analysis and discriminant | Statistical choices and result shape outside UI | **High** |
| **04A.21** | Move SPC data phase/exclusion/signal/limits rules from `Ui21ControlCharts.vb` to Common/Core as appropriate | 04A.02,01.08 | W=Yes O=No (contract tests) | Phase windows, missing, constants, run rules, exclusions | Run X-bar/control chart with phases and flags | WinForms only collects settings and invokes shared run | **High** |
| **04A.22** | Univariate SPC chart-type families and signal-rule/phase geometry payloads; `Ui21ControlCharts.vb`, Core SPC | 04A.21 | W=Yes O=No (contract tests) | X-bar, EWMA, CUSUM and p/np chart reference masks | Run EWMA and attribute chart | Common documents/markers reproduce Windows outputs | **High** |
| **04A.23** | Move multivariate SPC covariance/limits/detection choices from `Ui22MultivariateControlCharts.vb` | 04A.21,01.08 | W=Yes O=No (contract tests) | T², PCA-Q, MEWMA, MCUSUM limit/signal fixtures | Run multivariate chart including excluded points | Shared model/plot payload preserves masks, alerts and labels | **High** |
| **04A.24** | Multivariate SPC chart-type variants, covariance estimation, warning and chart payloads; `Ui22MultivariateControlCharts.vb` | 04A.23 | W=Yes O=No (contract tests) | Hotelling T2, PCA-Q, MEWMA/MCUSUM seeded reference charts | Run two multivariate SPC modes | Portable payloads without Excel chart collection access | **High** |
| **04A.25** | Extract PSM matching, calipers, weighting/strata, balance and Love-plot report coordination from `Ui20PropensityScoreMatching.vb`, `DM/PSMdataImport.vb` | 04A.09,01.08 | W=Yes O=No (contract tests) | Match ID/reproducibility, trimming, standardized mean difference goldens | Run NN match and Love plot | Statistical policy in Core/Common; no duplicated matching in forms | **High** |
| **04A.26** | Extract sample-size/assurance and agreement reporting/input rules from `Ui12SampleSizeTtestSingleProp.vb`, `UiTwoInputRefedits.vb` and related forms | 04A.04,04A.05 | W=Yes O=No (contract tests) | Rounding, CI effect size, ICC and Deming variance variants | Run paired sample-size and Bland–Altman | Range-free workflows and paired reports call reusable Common services | **Medium** |
| **04A.27** | Reconcile Windows `DM/ExcelDnaDataImporter.vb`, `DataObjContracts.vb`, `Helpers.vb`, `UI/Refedit/Excel2007RefEdit.vb` selection semantics, UDF range readers against Common parser contracts | 04A.03–04A.05,01.09–01.10 | W=Yes O=No (contract tests) | Source coordinate/date/error/missing parity, large-range safety | Import multi-range table and a UDF reference | One canonical typed transport; COM conversion stays Windows | **High** |
| **04A.28** | Reconcile output cells, formatting, p-values, layout/overwrite and three destinations; `DM/ResultWriter/ExcelDnaResultWriter.vb`, `SheetManagement.vb` | 02.01–02.08,04A.02 | W=Yes O=No (contract tests) | P-value, alpha, titles/footnotes, placement, rollback tests | Write several tables and chart to new/current sheet | Common owns semantic document; Excel writer owns API and workbook actions | **High** |
| **04A.29** | Audit all 38 Windows Graphics modules for numerical geometry mixed with Excel/Office objects; record Core already migrated vs remaining | 04A.01 | W=Audit O=No | Graphics-calculation inventory, dependency guards, chart equivalence baselines | Produce representative histogram, Sankey, PCA, survival plot | Every plot family given neutral geometry/spec and host renderer ownership | **Medium** |
| **04A.30** | Migrate remaining portable ROC, normal Q–Q, box, histogram, scatter matrix, XYZ and chart-scaling calculations; `Graphics/ROC.vb`, `NormalPlot.vb`, `BoxPlot.vb`, `ChartingFunc.vb` | 04A.29,04A.02 | W=Yes O=No (contract tests) | ROC/AUC, quantiles, sorted input immutability, axis/geometry fixtures | Run ROC and Normal Plot, check data unchanged | Core data calculations independent of Excel; renderer remains host-only | **High** |
| **04A.31** | ROC and normal plot data/quantiles/ref lines, preserve source immutability; `Graphics/ROC.vb`, `NormalPlot.vb` | 04A.30 | W=Yes O=No (contract tests) | AUC, tie handling, Q–Q fit line, copy-safe vectors | Plot ROC and Q–Q | Core-based plotted coordinates | **High** |
| **04A.32** | Box plot/histogram/violin aggregate and chart-axis semantics; `Graphics/BoxPlot.vb`, `Histogram.vb`, `ChartingFunc.vb` | 04A.31 | W=Yes O=No (contract tests) | Quartile method/bin edges/axis limits consistency | Plot box and distribution chart | One portable numeric basis for all hosts | **High** |
| **04A.33** | XYZ/ScatterMatrix/3D coordinate transformations, interpolation and series payloads | 04A.32 | W=Yes O=No (contract tests) | Scaling, labels, missing and deterministic 3D projection | Plot XYZ and scatter matrix | Portable geometry or explicitly documented host-only projection | **High** |
| **04A.34** | Migrate remaining reusable calibration, agreement, multiple-comparison, SPC, PSM, 3D and specialty graphic geometry/labels; corresponding `Graphics/*Excel.vb` | 04A.29–04A.30 | W=Yes O=No (contract tests) | Domain geometry, CI/zero reference, missing/empty values, deterministic bounds | Run PSM Love, survival and SPC plots | Host-independent chart payload for each family; explicit Excel-only exception logged | **High** |
| **04A.35** | Extract multiple-comparison/CI and agreement/PSM report and plot geometry; `Graphics/MultipleComparisonPlotExcel.vb`, `AgreementPlotExcel.vb`, `PsmLovePlotExcel.vb` | 04A.34 | W=Yes O=No (contract tests) | Zero/null reference, CI endpoints and label placement | Plot Bland–Altman, Love and comparisons | Portable chart specs with Excel drawing only in adapter | **High** |
| **04A.36** | Extract KM/Cox survival and SPC chart series/limits and annotations; `Graphics/*SurvivalPlotExcel.vb`, `SpcControlChartExcel.vb` | 04A.35 | W=Yes O=No (contract tests) | Censor marks, step curves and time origins; phase boundaries | Render survival and SPC charts | Renderer only maps spec into Excel APIs | **High** |
| **04A.37** | Extract PCA/factor/cluster/CA display geometry and label projection; `Graphics/PcaPlotExcel.vb`, `FactorAnalysisPlotExcel.vb`, `ClusterAnalysisPlotExcel.vb` | 04A.36 | W=Yes O=No (contract tests) | Loadings/scores/biplot and dendrogram reference geometry | Run PCA biplot and dendrogram | Shared chart data independently testable | **High** |
| **04A.38** | Audit 268 Excel-function exports across 41 UDF .vb sources, argument marshaling, model caches, data import and algorithm duplication; extract portable calc/validation | 04A.01,01.09 | W=Yes O=No (contract tests) | UDF signature inventory; scalar/array/error and handle-lifecycle fixtures | Sample scalar, array and cached-model UDFs | All reusable UDF logic in Core/Common; Windows-only registration remains | **High** |
| **04A.39** | UDF data import and argument normalization helper audit/extraction; all `ExcelUDFs/UDFdataImport/*.vb`, `ExcelArgModules.vb` | 04A.38 | W=Yes O=No (contract tests) | Typed scalar/matrix/range/error/locale inputs and spill bounds | Recalculate standard UDF with mixed cell types | Generic argument policies in Common; Range reads Excel-only | **High** |
| **04A.40** | Audit/extract portable regression UDF request builders for LM/GLM/GEE/Cox/ZIP/mixed/ordinal/multinomial; `ExcelUDFs/Regression/*.vb` | 04A.39 | W=Yes O=No (contract tests) | Model formula/weights/offset/result shapes; wrappers parity | Run one scalar and one model-array regression UDF | UDF core calculations shared; registration remains Excel-DNA | **High** |
| **04A.41** | Audit/extract portable basic stats, contingency, nonparametric, agreement, assumptions and sample-size UDF validation/results | 04A.39 | W=Yes O=No (contract tests) | p-value/error/scalar/matrix and CI parity across families | Run t, agreement, contingency and sample-size UDFs | No duplicate basic statistical algorithms in wrappers | **High** |
| **04A.42** | Audit/extract portable multivariate, classification, PSM, plot-data UDF preparation and array output; `MultivariateAnalysisUDFs.vb` and peers | 04A.39 | W=Yes O=No (contract tests) | PCA, clustering, PSM and plotted data output snapshots | Run PCA array and matching UDF | Typed portable result contracts and row order preserved | **High** |
| **04A.43** | Define UDF model-handle/cache ownership, thread-safety and deterministic recalc strategy; keep Excel-DNA session caches Windows-specific | 04A.40–04A.42 | W=Yes O=No (contract tests) | Handle invalidation, workbook lifecycle, parallel access and no stale cache leakage | Fit a cached-model UDF and recalculate | Shared model result semantics without Windows `ConcurrentDictionary` dependency | **High** |
| **04A.44** | Separate portable settings/default/help/fault/progress policy from `AppGlobals.vb`, `ExcelDnaHostInfrastructure.vb`, `UIprocedures.vb`, `Help/*`, `AutoUpdate.vb` | 03.01–03.06,04A.01 | W=Yes O=No (contract tests) | Alpha/help/locale/runtime fault behavior; no-host dependencies | Change settings, open help and show error | Neutral policy shared; files, registry, UI controls, updater remain Windows | **Medium** |
| **04A.45** | Add Windows UI wiring and execution integration harness for all Ribbon callbacks, nested RefEdit control and non-designer forms; safe fake hosts around option/state logic | 04A.03–04A.44 | W=Yes O=No (contract tests) | Callback resolution; enable/disable; option combinations; error paths | Exercise each form family with two option sets | Every supported control maps to validated request or explicitly host-only action | **High** |
| **04A.46** | GUI wiring tests for grouped, paired, categorical, sample-size and plot-dialog option families | 04A.45 | W=Yes O=No (contract tests) | All validation/option toggles and output choice callbacks | Toggle controls on UibyID and Ui82x2 | Non-designer Windows UI handlers all exercised | **High** |
| **04A.47** | GUI wiring tests for regression/mixed/multivariate/SPC/PSM long-running and fault paths | 04A.46 | W=Yes O=No (contract tests) | Event handlers, disabled options, help links, progress/cancel/failure state | Open mixed and SPC dialogs, trigger validation error | No silent disabled or unwired actions | **High** |
| **04A.48** | Full Windows code-extraction exit gate (including all decomposed source-family batches): compare per-method ledger, Core/Common APIs, reference outputs and retained COM/WinForms-only files | 04A.01–04A.47 | W=Yes O=Contract smoke | Static forbidden dependencies, parity and no-dead-code scans | Windows Excel-DNA regression tour across categories | No unreviewed portable routines in Windows; any exceptions documented with owner | **High** |
| **04A.49** | Violin connect means/min/max: compute ordered per-group finite summary anchor polyline in Core; options/spec in Common; `Graphics/ViolinPlot.vb`, `UI/UibyID.vb` | 04A.30,04A.48 | W=Yes O=No (contract tests) | Unequal/empty groups; mean/min/max, missing/ordering and chart data snapshots | Toggle each of three paths on 3+ violins | Independent toggles and Excel chart matching neutral spec | **Medium** |
| **04A.50** | Friedman sum ranks: expose sums alongside existing Core `Friedman.MeanRanks` with tie-aware blocks; Common report update | 04A.04,04A.48 | W=Yes O=No (contract tests) | Rank totals = blocks * means; ties, identical groups and missing blocks | Run Friedman with 3+ matched groups | Adds Sum Rank column without changing legacy statistics | **Low** |
| **04A.51** | Friedman–Nemenyi procedure: portable CD computation, assumptions, distribution quantiles and typed result separate from existing Conover/Dunn | 04A.50 | W=Yes O=No (contract tests) | Compare independent reference implementations; small/large k; alpha, ties, critical quantiles | Friedman Nemenyi sample with 4 groups | Validated CD, mean rank contrasts and correctly described method | **High** |
| **04A.52** | Friedman Nemenyi CD diagram and optional mean-rank chart; neutral rank-axis/interval/label spec and Excel renderer | 04A.51,04A.34 | W=Yes O=No (contract tests) | Connected non-significant ranks, overlap, sorted labels and SVG geometry fixtures | Plot CD with alpha 0.05 and 0.01 | Graph agrees with CD table; legends and confidence-level wording correct | **High** |
| **04A.53** | Dunnett multiple-vs-control engine: pooled variance, unequal sample sizes, simultaneous critical values and p-values | 04A.07,04A.48 | W=No O=No (Core/Common) | Numerical references/MC calibration as needed; equal/unequal n and alpha | Reference three-treatment-plus-control ANOVA | Validated familywise inference and control direction; no independent t-test substitution | **High** |
| **04A.54** | Dunnett control-role options and ANOVA Common report; optional explicit control column vs within-group control label | 04A.53,04A.07 | W=Yes O=No (contract tests) | Control matching, blank labels, no-control, multiple-controls and invalid selection | Select control from ANOVA dialog and compare tables | Typed control semantics and result tables shared by future Office host | **High** |
| **04A.55** | Dunnett CI/difference comparison chart and Excel-DNA UI wiring; `UI/UibyID.vb`, `Graphics/MultipleComparisonPlotExcel.vb` | 04A.54,04A.34 | W=Yes O=No (contract tests) | Simultaneous intervals, reference-zero, direction, alpha label | Run Dunnett on uneven groups and inspect chart | CI plot and results correspond exactly | **Medium** |
| **04A.56** | Hsu multiple comparisons with best (MCB) numerical engine; best/worst direction, simultaneous bounds and tie/variance assumptions | 04A.07,04A.48 | W=No O=No (Core/Common) | Independent references and least-favorable configuration, ties, unequal n | Fit 4-group ANOVA with one near-best group | MCB bounds validated; not relabeled Tukey intervals | **High** |
| **04A.57** | Hsu MCB typed Common options/report with best-is-highest/lowest policy and reference inference | 04A.56 | W=Yes O=No (contract tests) | Direction, near ties, duplicate labels and alpha validation | Run MCB with best-high vs best-low | Tables show correct best reference and simultaneous comparisons | **High** |
| **04A.58** | Hsu MCB Windows UI and bound plot using neutral multiple-comparison plot spec | 04A.57,04A.34 | W=Yes O=No (contract tests) | Bounds/endpoint/no-change markers, significance and label tests | Run both best directions and compare graph | Chart supports Excel today and Office renderer later | **Medium** |
| **04A.59** | Mann–Whitney categorical comparison histogram: shared rank/frequency distributions and optional mirrored chart spec, Excel renderer | 04A.03,04A.30 | W=Yes O=No (contract tests) | Ties, category binning, unequal group sizes, zero counts | Run two-group MW and inspect horizontal chart | Chart consistent with selected data and U statistic; portable data/spec | **Medium** |
| **04A.60** | Cochran variance outlier C test: portable pooled group-variance test, selected sample assumptions and reference p/critical values | 04A.06,04A.48 | W=No O=No (Core/Common) | Group replication, equal n assumption, max variance selection and reference values | Run group-variance outlier dataset | Stat method distinct from existing Cochran Q / Cochran–Armitage | **High** |
| **04A.61** | Cochran C variance outlier chart: variance-by-group with threshold and highlighted group; Common spec, Excel renderer and UI | 04A.60,04A.34 | W=Yes O=No (contract tests) | Threshold, zero/all-zero variances, group labeling and chart geometry | Plot variance outlier data | Chart and table agree, no false universal assumptions | **Medium** |
| **04A.62** | Kolmogorov–Smirnov empirical-vs-reference one-sample engine: D+/D−/D, fully specified continuous CDF, ties/validity | 04A.06,04A.48 | W=No O=No (Core/Common) | SciPy/R reference tests, exact/asymptotic p where valid, reproducible edge cases | One-sample KS against fixed normal parameters | Inference states null and restrictions; separate from fitted normality calibration | **High** |
| **04A.63** | Normality KS fit-parameters policy: separately validated Lilliefors/parametric-bootstrap p-value if mean/SD estimated; integrate result with `RunNormality` | 04A.62 | W=Yes O=No (contract tests) | Fully-specified vs estimated-parameter p-values; deterministic bootstrap seed | Run normality on normal/non-normal samples | No invalid ordinary KS p-values for fitted normal; report N, not invented df | **High** |
| **04A.64** | KS ECDF-vs-CDF comparison and max-separation plot in Common; Excel-DNA chart and optional standalone test entry | 04A.62–04A.63,04A.34 | W=Yes O=No (contract tests) | D+ and D− maximum locations, boundary steps and axis snapshots | Render one-sample KS plot | Numerical D equals visual maximum separation; chart spec portable | **Medium** |
| **04A.65** | Grubbs outlier chart reusing existing Core `Assumptions.Grubbs` plus flagged observation/critical region data; Excel UI | 04A.06,04A.34 | W=Yes O=No (contract tests) | Outlier index, ties, alpha, N limits, source row mapping | Run Grubbs with a known high outlier | Existing Grubbs numerical result unchanged; plot highlights the same row | **Medium** |
| **04A.66** | 2x2 odds/risk ratio confidence-interval forest-style plots with logarithmic axis and null=1; reuse Core OR/RR result estimates | 04A.05,04A.34 | W=Yes O=No (contract tests) | Woolf/Cornfield/zero-cell policies, finite intervals, log transformation | Run two 2x2 tables and compare CI endpoints | Plot matches Core intervals and displays method-specific CI clearly | **Medium** |
| **04A.67** | RxC Kendall tau-b/c, Goodman–Kruskal Gamma and Somers D CI plots; reuse existing `cTableORDINALassoc` and Windows `Ui0OneRefeditMulticol` output | 04A.05,04A.34 | W=Yes O=No (contract tests) | Bounded -1..1 association, columns-dependent orientation, CIs and asymmetric sign | Run ordered 3x4 table and inspect four plots | No recomputation of existing 4 statistics; CI axis shows 0 correctly | **Medium** |
| **04A.68** | Detrended normal Q–Q residuals and reference-line delta: move `Graphics/NormalPlot.vb` normal scores/fit data to Core and spec to Common | 04A.30 | W=No O=No (Core/Common) | Zero residual under reference fit, distribution tails, sorted-copy input and line choices | Compute normal + detrended Q–Q from same data | No mutation of user data; residual definition matches selected line method | **Medium** |
| **04A.69** | Detrended normal Q–Q chart and UI option in existing Normal Plot workflow; Excel renderer with zero reference | 04A.68,04A.34 | W=Yes O=No (contract tests) | Detrended sign, zero-line, layout and chart regression | Toggle ordinary and detrended plots | User can select either/both without changing existing Normal Plot result | **Medium** |
| **04A.70** | Paired t-test difference box plot: Common paired-difference vector with sign convention and missing-row alignment; Excel-DNA option/renderer | 04A.04,04A.30 | W=Yes O=No (contract tests) | Before-minus-after vs after-minus-before, quartiles and missing pair/ID mapping | Run paired t-test and compare difference quartiles | Difference box plot exactly matches tested paired differences | **Medium** |
| **04A.71** | Cross-feature plot semantics and accessibility: shared CI/critical-line/alpha metadata, chart/table warnings and palettes | 04A.49–04A.70,02.07 | W=Yes O=No (contract tests) | Chart-spec schema/snapshot and neutral legend/axis markers across new charts | Run sample comparison/KS/rank charts and export | No statistical data or precision encoded only by styling | **Medium** |
| **04A.72** | Update `methods.yml`, generated method metadata, Ribbon callback/help wiring, Windows screenshots and MkDocs for only selected PDF additions | 04A.49–04A.71,00.02B | W=Yes O=No (contract tests) | Manifest validators, help topic links and versioned docs checks | Open help from modified forms; inspect new options | Public methods discoverable without duplicate source of truth | **Medium** |
| **04A.73** | Cross-host-ready contract suite for new methods: serialize typed results/specs and execute under .NET 10 WASM-compatible Core/Common (no new Office feature screens) | 04A.71–04A.72,04.07 | W=No O=Contract test only | WASM smoke numerical contracts, golden fixtures, no COM references | Prototype request/response of one new chart-result DTO | New feature logic reusable by Office.js host without writing it in JavaScript | **High** |
| **04A.74** | Phase 04A acceptance audit: Windows regression, Core/Common + legacy + JS tests, graphics snapshots, source ledger closure, Phase 05/10/11 no-duplicate map | 04A.48,04A.73 | W=Yes O=Contract smoke | All code migrations parity and every feature result+chart validation | Exercise all 13 selected feature requests in Excel-DNA | Portable code/GUI adaptation complete; existing Excel-DNA behavior preserved; Phase 05 ready | **High** |

**Wave transition and avoiding duplicate work:** Existing Phase 05–09 migration batches retain their identifiers but become *Office feature-exposure, handler-registration, product-UI and host-parity waves* for any Core/Common service already extracted by 04A; they must **not re-extract algorithms or reimplement the 04A statistical enhancements**. Phase 10 consumes the portable chart payloads from 04A for Office-native/SVG/browser renderers rather than remaking chart statistics. Phase 11 exposes the existing Core/Common UDF logic through Office Custom Functions rather than reimplementing numerical algorithms. A future batch must use the 04A ledger to mark each overlap `reuse / host wiring / unresolved` before changing sources.

**Rollback and package protection:** Keep Windows Excel-DNA .NET Framework 4.8, .NET Standard 2.0 Core/Common, net10.0 tests/WASM, existing MSI signing, and generated `methods.yml` consumers stable. Batch changes should be small enough for patch/apply/build/full tests/1–2 Windows smoke runs. A failed statistical-reference check blocks the new feature but does not disable existing Windows methods.

### Phase 05 — Low-to-medium complexity statistical workflows

**Changed prerequisite (Revision 1.4):** Complete **04A.74** first. For methods already extracted and regression-tested during 04A, these batches are **Office task-pane exposure/feature wiring/parity**, not a second migration of calculations. Existing IDs, batch intent, and other listed prerequisites remain; treat the 04A gate as an additional implicit dependency of every 05.xx–11.xx implementation batch.

**Phase exit:** First large usable Office feature set; input/output shapes and shared validation proven.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **05.01** | Bring ANOVA+KW supplementary outputs to shared handler; `Common/Analysis/ANOVA/KW; Windows UI/UibyID.vb` | 02.07,04.04 | W=Yes O=Yes | Optional variance/descriptive/box and Dunn/posthoc parity | ANOVA with Welch/posthoc/box; KW+Dunn | No Windows-only statistical supplemental calculations | **High** |
| **05.02** | Descriptive statistics table workflow and shared summary options; `Common/Analysis/Descriptive.vb` | 05.01,01.04 | W=Yes O=Yes | Reference means/SD/quantiles and p-value metadata | Descriptive table | Complete descriptive table parity | **Medium** |
| **05.03** | Independent two-sample t-tests with variance choices; `Common/Analysis/IndependentT.vb` | 01.04,04.04,02.05 | W=Yes O=Yes | Equal/unequal variance and CI reference | Unpaired t sample | Independent t table parity | **Medium** |
| **05.04** | Mann–Whitney core workflow and rank-table contract; `Common/Analysis/MannWhitney.vb` | 01.04,04.04 | W=Yes O=Yes | Mann–Whitney ties/exact cases | Mann–Whitney example | Rank grouping/result contract proven | **Medium** |
| **05.05** | Common blocked/subject input-to-report adapter; `Common/Analysis/BlockedDesign.vb` | 01.06,04.04 | W=Yes O=Yes | Blocked data validator and row mapping | Subject/visit preview | Shared blocked-report boundary usable | **High** |
| **05.06** | Contingency and proportions input-to-report foundation; `Common/Analysis/ContingencyBase.vb` | 01.05,04.04 | W=Yes O=Yes | Counts/levels/rates and p-value metadata | Basic 2x2 table | Common categorical reporting path established | **Medium** |
| **05.07** | Agreement input/output protocol and neutral plot payload; `Common/Analysis/AgreementBase.vb` | 01.04,04.04 | W=Yes O=Yes | Pairs, reliability options, result metadata | Bland–Altman table preview | Agreement result schema ready | **High** |
| **05.08** | Sample-size option/input/output framework; `Common/Analysis/SampleSizeBase.vb` | 03.01,04.04 | W=Yes O=Yes | No-range scalar options and rounding contracts | Sample-size calculator screen | Can dispatch range-free analytical requests | **Medium** |
| **05.09** | Normality tests and distribution checks; `Common/Analysis/Normality.vb` | 05.02 | W=Yes O=Yes | Normality algorithms and missing values | Run normality check | All normality choices use shared results | **Medium** |
| **05.10** | Outlier tests and flagged-row annotations; `Common/Analysis/Outliers.vb` | 05.02 | W=Yes O=Yes | Outlier flags and source row mapping | Run univariate outlier example | Flags and row IDs agree | **Medium** |
| **05.11** | Variance, homogeneity and symmetry workflows; `Common/Analysis/Assumptions.vb` | 05.02 | W=Yes O=Yes | Levene/Bartlett/Fligner/symmetry fixtures | Variance homogeneity example | All tables/p-values match Windows | **Medium** |
| **05.12** | Paired tests as a distinct workflow; `Common/Analysis/PairedTests.vb` | 05.03 | W=Yes O=Yes | Pairwise missing and paired t fixtures | Run paired t with missing pairs | No groupwise misalignment | **Medium** |
| **05.13** | Spearman correlation workflow; `Common/Analysis/Spearman.vb` | 05.04 | W=Yes O=Yes | Spearman ties/reference | Spearman matrix | Spearman outputs and labels parity | **Medium** |
| **05.14** | Wilcoxon signed-rank workflow; `Common/Analysis/Wilcoxon.vb` | 05.04 | W=Yes O=Yes | Signed ranks/ties/exact fixture | Wilcoxon | Signed rank parity | **Medium** |
| **05.15** | One-way repeated-measures ANOVA; `Common/Analysis/RepeatedAnova.vb` | 05.05 | W=Yes O=Yes | Sphericity and repeated subjects | Repeated-measures example | Correct subject/visit mapping | **High** |
| **05.16** | Friedman blocked rank workflow; `Common/Analysis/Friedman.vb` | 05.05 | W=Yes O=Yes | Blocked rank inference | Friedman | Friedman parity | **High** |
| **05.17** | Exact Fisher and 2x2/proportions outputs; `Common/Analysis/Proportions.vb` | 05.06 | W=Yes O=Yes | 2x2 odds, CI and exact tests | Fisher table | Result formatting/parity | **Medium** |
| **05.18** | RxC and Mantel–Haenszel strata; `Common/Analysis/StratifiedContingency.vb` | 05.06 | W=Yes O=Yes | Stratified estimators and invalid counts | Stratified RxC workbook | Strata options and table parity | **Medium** |
| **05.19** | Passing–Bablok agreement regression; `Common/Analysis/PassingBablok.vb` | 05.07 | W=Yes O=Yes | Slope/CI and ties | Passing–Bablok | Numerical and plot-spec parity | **High** |
| **05.20** | ICC agreement methods; `Common/Analysis/ICC.vb` | 05.07 | W=Yes O=Yes | ICC type and CI fixtures | ICC | ICC options/result parity | **High** |
| **05.21** | T-test and proportion sample-size utilities; `Common/Analysis/SampleSizeBasic.vb` | 05.08 | W=Yes O=Yes | Power/alpha/integer rounding | Independent proportion sizing | One-input family panels work | **Medium** |
| **05.22** | Advanced sample-size input schema and execution adapters; `Common/Analysis/AdvancedSizingInputs.vb` | 05.08 | W=Yes O=Yes | Power option/default contract | Advanced sizing preview | Shared sizing schema ready | **High** |
| **05.23** | Deming regression agreement workflow; `Common/Analysis/DemingAgreement.vb` | 05.19 | W=Yes O=Yes | Slope/intercept, weighting and CI fixtures | Deming agreement | All Deming options parity | **Medium** |
| **05.24** | Bland–Altman agreement and limits plots; `Common/Analysis/BlandAltmanAgreement.vb` | 05.19 | W=Yes O=Yes | Bias, limits and plot-source positions | Bland–Altman | Limits and diagnostic payload parity | **Medium** |
| **05.25** | Weighted Cohen kappa agreement; `Common/Analysis/WeightedKappa.vb` | 05.20 | W=Yes O=Yes | Weights/coding/tables | Weighted kappa | All weight choices supported | **Medium** |
| **05.26** | Lin concordance correlation agreement; `Common/Analysis/LinConcordance.vb` | 05.20 | W=Yes O=Yes | CCC/CI/paired-data reference | Lin CCC | Numeric parity | **Medium** |
| **05.27** | Logrank and Cox sample size calculators; `Common/Analysis/SurvivalPower.vb` | 05.22 | W=Yes O=Yes | Event power and allocation cases | Logrank sizing | Survival calculators parity | **Medium** |
| **05.28** | ICC and Bland–Altman sample size; `Common/Analysis/AgreementPower.vb` | 05.22 | W=Yes O=Yes | Reliability/sample-size power | ICC sizing | Reliability calculators parity | **Medium** |
| **05.29** | Kendall correlation workflow; `Common/Analysis/KendallCorrelation.vb` | 05.13 | W=Yes O=Yes | Kendall tie variants/reference | Kendall analysis | Tie-corrected parity | **Medium** |
| **05.30** | Theil–Sen regression workflow; `Common/Analysis/TheilSen.vb` | 05.14 | W=Yes O=Yes | Slope/confidence and rank/reference | Theil–Sen analysis | Model and chart payload parity | **Medium** |
| **05.31** | Cochran Q and Skillings–Mack blocked tests; `Common/Analysis/IncompleteBlocks.vb` | 05.16 | W=Yes O=Yes | Block data and missing cases | Skillings–Mack example | Blocked test options parity | **High** |

### Phase 06 — Regression and advanced categorical models

**Phase exit:** Design/formula builders and core model families reach browser without duplicated engine code.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **06.01** | Linear model formula/design and result workflow; `Common/Analysis/LinearModel.vb; Core/LinearModel` | 01.07,04.05,02.07 | W=Yes O=Yes | NIST parity; Type I/II/III ANOVA; diagnostics | Fit multi-predictor LM | Coef/ANOVA/residual/fit tables retained | **High** |
| **06.02** | Poisson/log GLM shared workflow and model-result contract; `Common/Analysis/GLMPoisson.vb` | 06.01 | W=Yes O=Yes | Poisson/link model table parity | Poisson/log GLM | Current WASM-tested model exposed in production shell | **High** |
| **06.03** | GLM extended-family registration and option contract; `Common/Analysis/GLMFamilySpec.vb` | 06.02 | W=Yes O=Yes | Family option mapping and fault codes | Select NB2 and Gamma family | Typed family contract ready for distinct handlers | **High** |
| **06.04** | GEE cluster/covariance workflows; `Common/Analysis/GEE.vb` | 01.06,06.02 | W=Yes O=Yes | Indep/Exch/AR1/Toeplitz/UN reference | Run GEE by subject ID | Stable covariance options and diagnostics | **High** |
| **06.05** | Multinomial and ordinal logistic models; `Common/Analysis/CategoricalModels.vb` | 06.01,01.05 | W=Yes O=Yes | Coding, links, baseline levels, fit tables | Fit multinomial or ordinal model | All category contrasts and warnings preserved | **High** |
| **06.06** | Regression diagnostic, predictions and chart artifacts; `Common/Results/ModelDiagnostics.vb; Core regression payloads` | 06.01-06.05 | W=Yes O=Yes | Residual, influence, fitted and diagnostic vectors | LM fitted/residual plot preview | Row alignment and fault/convergence displayed | **High** |
| **06.07** | Gaussian/identity GLM reporting; `Common/Analysis/GLMGaussian.vb` | 06.02 | W=Yes O=Yes | Gaussian GLM reference | Gaussian GLM | Family parity | **Medium** |
| **06.08** | NB2 regression fit, dispersion and warnings; `Common/Analysis/GLMNB2.vb` | 06.03 | W=Yes O=Yes | NB2 dispersion/convergence | NB2 model | NB2 result parity | **High** |
| **06.09** | ZIP model distinct EM/results workflow; `Common/Analysis/ZeroInflated.vb` | 06.03 | W=Yes O=Yes | EM stopping and fit criteria | ZIP model | Same convergence/fault reporting | **High** |
| **06.10** | Binomial GLM and contrast specifics; `Common/Analysis/GLMBinomial.vb` | 06.07 | W=Yes O=Yes | Binomial/link/odds outputs | Binomial GLM | Results and residual parity | **Medium** |
| **06.11** | Gamma GLM and dispersion specifics; `Common/Analysis/GLMGamma.vb` | 06.08 | W=Yes O=Yes | Gamma deviance/link and fault codes | Gamma GLM | Statistics and model warnings parity | **High** |

### Phase 07 — Survival, clustered and mixed models

**Phase exit:** Censoring, clustering, covariance and sophisticated model diagnostics exposed and tested.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **07.01** | Kaplan–Meier and log-rank workflow; `Common/Analysis/Survival.vb` | 01.06,04.04 | W=Yes O=Yes | Censor/ties/strata and curve payloads | KM/log-rank in browser | Survival tables and curves available | **Medium** |
| **07.02** | Cox PH model and censor/strata options; `Common/Analysis/Cox.vb` | 07.01,06.01 | W=Yes O=Yes | Breslow/Efron/Exact; residual/failure parity | Fit Cox with ties | Same estimates and diagnostics | **High** |
| **07.03** | MMRM covariance and repeated-measure model workflow; `Common/Analysis/MMRM.vb; Core/MixedModels` | 01.06,04.05,03.03 | W=Yes O=Yes | UN/CS/AR1/HCS etc; KR/Satt; LS means | Run MMRM on sample longitudinal data | Covariance/inference/diagnostics preserved | **High** |
| **07.04** | LMM random-effects/fit and inference workflow; `Common/Analysis/LMM.vb; Core/MixedModels` | 07.03 | W=Yes O=Yes | Random terms, REML/ML, KR/Satt/BW | Fit simple random-intercept LMM | Formula, estimates, diagnostics parity | **High** |
| **07.05** | Mixed/survival post-estimation and model diagnostics; `Common/Results/AdvancedModelArtifacts.vb` | 07.02-07.04,06.06 | W=Yes O=Yes | Survival curve, contrasts, LS means, fitted IDs | Posthoc contrasts from MMRM | No host-specific inference calculation | **High** |
| **07.06** | MMRM post-estimation and covariance structures; `Common/Analysis/MMRMPost.vb` | 07.03 | W=Yes O=Yes | Covariance structures and contrasts | MMRM posthoc | Full inferential choices represented | **High** |
| **07.07** | LMM random-effect variants and fit summaries; `Common/Analysis/LMMVariants.vb` | 07.04 | W=Yes O=Yes | Random slope and fit comparison | Random-slope LMM | Variant parity and diagnostics | **High** |

### Phase 08 — Multivariate analysis

**Phase exit:** Matrix-driven workflows and metadata-rich results ported, with usable plots.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **08.01** | PCA/Hotelling and basic scores/loadings plots; `Common/Analysis/PCA.vb; Core/Multivariate/PCA` | 01.08,10.01 | W=Yes O=Yes | Eigen/score/loadings, source rows | PCA by Excel selection | Numerical table+plot specs retained | **Medium** |
| **08.02** | CA and MCA workflows; `Common/Analysis/Correspondence.vb` | 08.01 | W=Yes O=Yes | Contingency levels, factor maps | Run CA and MCA | Maps and axis semantics available | **Medium** |
| **08.03** | Hierarchical and k-means clustering; `Common/Analysis/Cluster.vb` | 08.01,03.03 | W=Yes O=Yes | Distance/linkage/seed; dendrogram geometry | K-means with seeded fixture | Stable labels, assignments and dendrogram | **High** |
| **08.04** | Factor/discriminant common input-to-result contract; `Common/Analysis/MultivariateModelBase.vb` | 08.01 | W=Yes O=Yes | Rotation/class label validated schemas | Preview factor options | Advanced model schemas support both methods | **High** |
| **08.05** | Multivariate diagnostics and complex plot labels; `Common/Charts/MultivariateChartSpecs.vb` | 08.02-08.04,10.02 | W=Small O=Yes | Label collision/scaling/reference fixtures | Biplot and dendrogram preview | Readable chart and data export | **High** |
| **08.06** | Factor analysis/rotation details; `Common/Analysis/Factor.vb` | 08.04 | W=Yes O=Yes | Factor/rotation/loading fixtures | Run factor rotation | Rotation options and tables match | **High** |
| **08.07** | Discriminant analysis classification details; `Common/Analysis/Discriminant.vb` | 08.04 | W=Yes O=Yes | Prior/class scores/misclassification | Run discriminant analysis | Prediction labels/source row parity | **High** |

### Phase 09 — SPC and propensity score matching

**Phase exit:** Ordered time/cluster series and large stateful causal workflows ported.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **09.01** | Univariate Shewhart SPC workflow; `Common/Analysis/ShewhartSPC.vb` | 01.08,10.01 | W=Yes O=Yes | Xbar and R control-rule fixtures | Xbar/R chart | Ordering and limits parity | **High** |
| **09.02** | Multivariate SPC and time-weighted variants; `Common/Analysis/MultiSPC.vb` | 09.01 | W=Yes O=Yes | T2, CUSUM and covariance fixtures | Multivariate chart example | Statistics/rule diagnostics preserved | **High** |
| **09.03** | PSM nearest-neighbor matching base workflow; `Common/Analysis/PSMBase.vb` | 01.08,06.02,03.03 | W=Yes O=Yes | Nearest-neighbor, seed and row mapping | Simple NN-PSM match | Base model/selection artifacts shared | **High** |
| **09.04** | PSM inference, sensitivity, balance and Love plot payloads; `Common/Analysis/PSMDiagnostics.vb` | 09.03,10.02 | W=Yes O=Yes | Weights/strata/bootstrap/balance fixtures | Love plot with balance table | Full inference and plots accessible | **High** |
| **09.05** | Capability analysis and univariate SPC variants; `Common/Analysis/SpcCapability.vb` | 09.01 | W=Yes O=Yes | Process limits, phases and rules | Capability and CUSUM examples | All rule flags available | **High** |
| **09.06** | PSM matching options and overlap diagnostics; `Common/Analysis/PSMAdvanced.vb` | 09.03 | W=Yes O=Yes | Caliper/trim/common support/strata | PSM with caliper | Deterministic match/reference parity | **High** |

### Phase 10 — Chart specification, renderers and export

**Phase exit:** Charts work through native Office APIs when faithful, otherwise SVG/Canvas/image fallback.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **10.01** | Define portable chart document and renderer capability choice; `Common/Charts/ChartDocument.vb; Core/Graphics` | 02.07,00.06 | W=Small O=Small | Serialization, axes/legend/types | Chart-spec preview with 2 series | No Office or COM chart types in Common | **Medium** |
| **10.02** | Implement basic Office native charts and SVG fallback; `Office/Charts/NativeChartRenderer.ts; SvgChartRenderer` | 10.01,04.02 | W=No O=Yes | JS renderer mocks; SVG snapshots | Insert scatter and a custom SVG | Capability-based render choice works | **High** |
| **10.03** | Histogram/box/violin/categorical histogram family; `Common/Charts/DistributionCharts.vb; Core/Graphics` | 10.02 | W=Small O=Yes | Bins, geometry, resize snapshots | Histogram+box, violin | Plot scales/box alignment stable | **High** |
| **10.04** | CDF/ECDF, ROC, normal plot, survival plots; `Common/Charts/StatDiagnosticCharts.vb; Core/Graphics and Windows ROC/NormalPlot` | 10.02,07.01 | W=Small O=Yes | Numerical curve, step, reference line snapshots | ROC and Kaplan–Meier plot | No duplicated statistical geometry | **High** |
| **10.05** | Specialized vector-chart spec and placement infrastructure; `Common/Charts/VectorChartSpec.vb` | 10.02 | W=Small O=Yes | Polygons/path/label placements | One SVG vector chart | Reusable specialized shape primitives | **High** |
| **10.06** | PCA/CA/multivariate, SPC, agreement, Love, comparisons; `Common/Charts/ScientificPlots.vb` | 08.05,09.04,10.02 | W=Small O=Yes | Intervals, labels, multi-panels, chart parity | PCA biplot and PSM Love plot | No missing statistical annotations | **High** |
| **10.07** | Advanced chart preview/fallback policy; `Office/Charts/AdvancedFallback.vb` | 10.02 | W=Small O=Yes | Capability/fallback selection | Static advanced-plot preview | Renderer fallback explicitly documented | **High** |
| **10.08** | Chart insertion, resize, export SVG/PNG and PDF provider; `Common/Output/ChartExportPlan.vb; host exporters` | 10.03-10.07 | W=Yes O=Yes | Image fidelity, dimensions, download limits | Resize and export violin/PNG | Charts export per declared capabilities | **High** |
| **10.09** | Sankey ribbon and flow geometry; `Common/Charts/SankeySpec.vb` | 10.05 | W=Small O=Yes | Sankey paths/flow sums | Sankey plot | Sankey geometry fidelity | **High** |
| **10.10** | Polar chart specification and circular axes; `Common/Charts/PolarSpec.vb` | 10.05 | W=Small O=Yes | Angles and radial axes | Polar plot | Polar geometry fidelity | **Medium** |
| **10.11** | Scatter matrix and XYZ/3D fallback; `Office/Charts/MatrixAnd3DRenderer` | 10.07 | W=No O=Yes | Fallback performance/screenshots | XYZ plot preview | Explicit static fallback where needed | **High** |
| **10.12** | Dumbbell/ladder annotation-spec details; `Common/Charts/DumbbellLadderSpec.vb` | 10.09 | W=Small O=Yes | Pairs and connector vector geometry | Ladder chart | Labels and connectors placed correctly | **Medium** |
| **10.13** | Bullet chart thresholds/targets and captions; `Common/Charts/BulletChartSpec.vb` | 10.09 | W=Small O=Yes | Band/target/axis rules | Bullet chart | Band geometry/format parity | **Medium** |
| **10.14** | Convex hull overlays and group labels; `Common/Charts/ConvexHullSpec.vb` | 10.10 | W=Small O=Yes | Hull orientation/group outlines | Hull plot | Polygon and scale parity | **Medium** |
| **10.15** | Kite chart geometry/axis specifics; `Common/Charts/KiteChartSpec.vb` | 10.10 | W=Small O=Yes | Kite widths and category ordering | Kite chart | Correct geometry and labels | **Medium** |

### Phase 11 — Cross-platform Excel Custom Functions

**Phase exit:** UDF argument/result semantics, WASM runtime reuse and feature-family migration complete.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **11.01** | Approve complete source-derived 268-UDF classification, UDF→`methods.yml` association and metadata catalog; `Common/UDF/UdfCatalog.vb; ExcelUDFs source scanner` | 00.02B | W=Small O=No | Names, aliases, docs, duplicate detection | Check sample function list/help | Every function has explicit migration status | **Medium** |
| **11.02** | Shared UDF typed input/optional/missing/error converters; `Common/UDF/UdfArgumentParser.vb; UdfResult.vb` | 11.01,01.02 | W=Small O=Yes | Scalars/ranges/errors/optional/spill shape | Stateless ANOVA1 input by UDF rules | Identical coercion/error semantics | **High** |
| **11.03** | Prototype shared JS custom-functions + WASM runtime; `Office/custom-functions manifest/JS bridge; startup manager` | 04.01,11.02 | W=No O=Yes | Metadata load, promise/fault, warm runtime | Use =BESH.* in Excel Online | Core invoked from Excel recalc, not rewritten | **High** |
| **11.04** | Migrate scalar/stateless test and distribution UDF slice; `Common/UDF/StatelessFunctions.vb; Office function metadata` | 11.03 | W=Small O=Yes | Scalar parity, formula errors/locale | 3 scalar functions on worksheet | Acceptable cold/warm invocation latency | **Medium** |
| **11.05** | Migrate vector/matrix and table-spill UDF slice; `Common/UDF/ArrayFunctions.vb` | 11.04 | W=Small O=Yes | Dynamic spill, blocked cells, 2D shape | ANOVA/RxC table spills | Return dimensions/values match Windows | **High** |
| **11.06** | Design session model handle/cache and invalidation; `Common/UDF/ModelSession.vb; Windows caches; Office runtime` | 11.03,06.01 | W=Yes O=Yes | Unique handles, reopen, stale, workbook switching | Fit then change input then query | No invalid cross-session model reference | **High** |
| **11.07** | LM model-handle Custom Functions vertical slice; `Common/UDF/LMModelFunctions.vb` | 11.06,07.05 | W=Small O=Yes | LM fit/coef/cache invalidation | LM FIT+COEF formula | First handle-based formula truly works | **High** |
| **11.08** | Advanced model-handle schema and array-result adapter; `Common/UDF/AdvancedModelBridge.vb` | 11.06,08.05,09.04 | W=Small O=Yes | Advanced handle types and dynamic result mapping | PCA handle syntax preview | No per-family handle semantics duplicated | **High** |
| **11.09** | Performance, worker feasibility, cache/memory safety and UDF release audit; `Office UDF runtime; Common/UDF/PerfPolicy.vb` | 11.04-11.08 | W=Optional O=Yes | 100/1000 calls, cancellations, cache eviction | Recalculate dense UDF workbook | Meets budgets without task-pane lock | **High** |
| **11.10** | Office functions for GLM and GEE fit/query; `Common/UDF/GLMGEEFunctions.vb` | 11.07 | W=Small O=Yes | Recalc/warm handles and covariance | GLM/GEE formula fit+coef | Parity and cache lifecycle | **High** |
| **11.11** | Shared regression model UDF handle pattern for survival/mixed; `Common/UDF/AdvancedRegressionHandle.vb` | 11.07 | W=Small O=Yes | Typed handle schema and invalidation | Cox/LMM handle descriptor | Survival/mixed UDF reuse boundary ready | **High** |
| **11.12** | PCA fit/query Custom Functions; `Common/UDF/PcaFunctions.vb` | 11.08 | W=Small O=Yes | PCA cache and score spill | PCA FIT/LOADINGS | PCA function parity | **High** |
| **11.13** | PSM fit/query Custom Functions; `Common/UDF/PsmFunctions.vb` | 11.08 | W=Small O=Yes | PSM cache, weights and effect outputs | PSM FIT/RESULTS | PSM function parity | **High** |
| **11.14** | Office Cox fit and post-estimation UDFs; `Common/UDF/CoxFunctions.vb` | 11.11 | W=Small O=Yes | Cox handle and table shapes | Cox FIT and coef | Survival-UDF parity | **High** |
| **11.15** | Office LMM and MMRM fit and contrast UDFs; `Common/UDF/MixedModelFunctions.vb` | 11.11 | W=Small O=Yes | LMM/MMRM inference and session handles | LMM fit/contrast | Correct mixed model UDF lifecycles | **High** |
| **11.16** | Office CA/MCA model-handle functions; `Common/UDF/CorrespondenceFunctions.vb` | 11.12 | W=Small O=Yes | CA/MCA matrices and handle cache | CA fit/query | Output shape/numerical parity | **High** |
| **11.17** | Office clustering model-handle functions; `Common/UDF/ClusterFunctions.vb` | 11.12 | W=Small O=Yes | Seed/labels/tree output | Kmeans fit/query | Deterministic results and cache | **High** |
| **11.18** | Office factor/discriminant model-handle UDFs; `Common/UDF/FactorDiscriminantFunctions.vb` | 11.12 | W=Small O=Yes | Rotation/classification handles | Factor fit/query | Model result parity | **High** |
| **11.19** | Office plot-source table Custom Functions; `Common/UDF/PlotSourceFunctions.vb` | 11.13 | W=Small O=Yes | Plot-source output shape | Plot data formula spill | All advertised plot functions mapped | **Medium** |

### Phase 12 — Office product deployment and version lifecycle

**Phase exit:** Reproducible production build, manifest, sideload/central deployment and rollback.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **12.01** | Harden local HTTPS, dev sideload and host diagnostics; `OfficeAddin project assets; dev manifest` | 04.07 | W=No O=Yes | Sideload recipes and origin checks | Dev Office Online + Windows desktop | Repeatable onboarding and host detection | **Medium** |
| **12.02** | Production XML manifest, commands and functions registrations; `Office/manifest-prod.xml; icons, help assets` | 11.03,12.01 | W=No O=Yes | Schema checks; function/command metadata | Sideload production-shaped manifest | No localhost assets/preview-only promises | **High** |
| **12.03** | Static deployment, cache busting and rollback; `OfficeAddin publish/build/hosting config` | 12.02 | W=No O=Yes | Asset hash, stale browser cache, offline error | Update while workbook open then reopen | Version-consistent managed/JS assets | **High** |
| **12.04** | Office centralized deployment / Marketplace / permissions; `Deployment docs; release candidate` | 12.03 | W=No O=Yes | Permission/requirements check by host | Admin deployment or pilot sideload | Supported distribution path documented | **High** |
| **12.05** | Keep Windows installer/version/signature parity separate; `Windows packaging and XLL pack audit` | 12.03 | W=Yes O=No | Signed MSI / 32+64 packing / Core/Common load | Install on clean Windows Excel | Windows production unaffected | **Medium** |
| **12.06** | Mac and Windows Office.js production integration; `Office add-in deployment, host compatibility list` | 12.04 | W=No O=Yes | Host capability feature matrix | Open same workbook on Mac/Win/Web | Supported UI/functions consistently available | **High** |

### Phase 13 — Cross-platform verification and release hardening

**Phase exit:** Signoff against declared host/feature completeness matrix and performance budgets.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **13.01** | Comprehensive Core/Common golden contract tests; `Core.Tests; Common.Tests; benchmark fixtures` | 05.08-11.09 | W=Small O=Small | All families, serialization, warnings, idempotency | Two sample analyses from every family | No untested analysis handlers | **Medium** |
| **13.02** | Office.js adapter mocks/browser UI/e2e wiring suite; `OfficeAddin.Tests; JS tests; Playwright` | 04.07,12.02 | W=No O=Yes | Selection/actions/options, rejected inputs | Button-click regression and host read/write | No unwired screen controls or silent errors | **High** |
| **13.03** | Cross-host numerical/output parity dashboard; `Test harness comparing Excel-DNA and WASM` | 13.01,13.02 | W=Yes O=Yes | Tolerances, tables, plots, metadata | Run same GLM/GEE/KM/PSM fixture | Documented parity gate for every feature | **High** |
| **13.04** | Localization/accessibility and browser security audit; `Office UI/JS; Common culture rules` | 13.02 | W=Small O=Yes | Slovak/English, keyboard and error injection | Localized Excel + keyboard-only run | Usable and safe with no culture corruption | **Medium** |
| **13.05** | Performance, memory, cancellation and large data; `Core benchmark; Office/WASM perf harness` | 13.03 | W=Small O=Yes | Resource budget, repeated UDFs, long models | Large PCA/PSM; cancel supported model | Documented constraints; no runaway memory | **High** |
| **13.06** | Mac/Windows/Web regression and release feature signoff; `Release matrix; installer and manifest CI` | 12.05,12.06,13.03-13.05 | W=Yes O=Yes | Full automated suite and release artifacts | Clean-install smoke each host | All promised supported cells in matrix PASS | **High** |

## 14. Critical path, parallelization and architecture decision log

### Main dependency chain

**Revision 1.4 insertion:** `01.10 (complete, per user + source validation) → 02.xx → 03.xx → 04.01–04.07 → 04A.01–04A.48 (full Windows audit/extraction gate) → 04A.49–04A.74 (selected Windows features and portability gate) → Phase 05–11 Office exposure/rendering/UDF workflows → Phase 12–13 release gates`. Where independent infrastructure can proceed in parallel, retain its original smaller dependency graph; this is the primary acceptance chain for mass feature migration. All Phase 04A numerical methods belong in Core, all typed application/plot semantics in Common, and Windows retains only host UI/rendering.


```text
Stage 3.04 baseline
    └─ 00.01 golden fixtures [COMPLETE]
        └─ 00.02 IDs/descriptors [COMPLETE]
            └─ 00.02A–00.02B metadata + UDF docs [COMPLETE]
                └─ 00.03–00.06 registry, result contracts, host preflight [COMPLETE]
                    └─ 01.01–01.05 typed source/input parsers [COMPLETE]
                        └─ 01.05A host-neutral contract hardening [COMPLETE]
                            └─ 01.06–01.07 longitudinal/model inputs [COMPLETE]
                                └─ 01.08 advanced inputs [COMPLETE]
                                    └─ 01.09 host acquisition [complete] → 01.10 large-range acquisition and quotas [implemented; host smoke separate]
            ├─ 01.xx input shapes & host acquisition
            ├─ 02.xx output document/semantic formatting/layout
            └─ 03.xx execution, settings, progress/fault contracts
                └─ 04.xx production OfficeAddin shell + reusable GUI
                    ├─ 05.xx basic analysis parity
                    ├─ 06.xx regression → 07.xx survival/mixed
                    ├─ 08.xx multivariate → 09.xx SPC/PSM
                    └─ 10.xx chart specification/rendering (starts early)
                         └─ 11.xx UDF full migration (bridge spike starts earlier)
                             └─ 12.xx production distribution
                                 └─ 13.xx cross-host release gates
```

**The remaining critical path** is large-range acquisition/quotas → output semantics/layout → host adapters → production shell. Once those contracts are settled, several analysis families can proceed independently and graphics specifications can be developed in parallel. Do not spend months completing every Windows graphics renderer before validating a single production OfficeAddin UI. Conversely, **do not bulk-migrate UDFs** before proving a custom-function-managed warm WASM runtime across real Office clients.

**Suggested overlap without violating gates:** From the current checkpoint, prototype chart-spec serialization (`10.01`) and Custom Functions runtime feasibility (part of `11.03`) in disposable test branches while implementing 01/02. Do not merge a feasibility shortcut into production without its declared dependencies. Prepare GUI test harness while writing reusable components, not after the entire catalog is exposed. Run Windows parity smoke after **every** batch that edits the Windows host.

### Decisions to approve before mass migration

| Decision | Recommended choice | Trade-off / trigger for reconsideration |
|---|---|---|
| Authoritative method catalog | `methods.yml` for user-visible methods/help/nav; validate actual Ribbon XML separately; UDF declarations remain in code | Prevents duplicate hand-maintained CSV feature lists and metadata drift without making YAML an executable registry |
| Product host vs spike | Create **new production OfficeAddin**, retain small WasmSmoke | Slight temporary duplication of bootstrap code, but isolates experiments and releases |
| Central `Select Case` | **Implemented 00.03:** typed handler registry and compatibility enum façade; extend this registry instead of reintroducing switches | Small upfront abstraction; avoids gigantic dispatcher and duplicated C# switches |
| Typed result details | Artifact envelope + per-handler typed detail and validated DTO serialization | More contracts; prevents `Object` becoming uncontrolled public API |
| CoreDataTable vs SpreadsheetRangeData | Transport **→** typed Common parse **→** Core containers | Two intentional layers, no competing data-frame engines |
| ResultTable vs output plan | Core retains statistical tables; Common owns document semantics, placement, destination | Preserves existing Core numerics and guarantees output parity |
| Office chart output | Mixed native editable charts + SVG/Canvas/image where native is unsuitable | Different editability, so document plot-by-plot and preserve source data |
| Custom functions | **Shared browser runtime, warm WASM bridge feasibility first**; no JS math rewrite | Startup/UI-blocking/memory risk, benchmark before committing |
| Custom function manifest | Add-in-only **XML manifest first** for production candidate | Unified manifest Custom Functions still preview in Sept. 2026 Microsoft sample; reassess when GA |
| Workbook output | Capability gate and fallback, not blanket promise | `NewWorkbook` currently unsupported by prototype JS writer |
| Settings | Shared schema/defaults; Windows XML and Office storage adapters | Some setting persistence scopes are host-specific |
| Cancellation | Cooperative, report unsupported when model can't cancel | Cannot interrupt arbitrary heavy managed calculation safely by UI flag alone |
| Release scope | Full declared capability on **Excel Online + Windows/Mac Office.js**; Windows Excel-DNA remains independent | Office API feature differences are explicit, no pretending 100% visual identity |
| Versioning | API/schema version + host asset/manifest version; MSI version separate | Avoid mixing web cache version with assembly or installer versions |

### Decision gates and reversal costs

- **Gate A / after 00.04 [SOURCE IMPLEMENTED; FULL HOST TESTS USER-REPORTED]:** 00.01 snapshots and 00.02 IDs/descriptors retained; 00.02A–B metadata reconciliation and full UDF docs parser parity passed; typed registry/dispatch and existing ANOVA/Kruskal output parity established.
- **Gate B / after 02.08:** agreed source/row semantics, shared complete output-plan format, p-value highlights and destination policy; **freeze Common v1 DTO envelope**, permitting additive versioning only.
- **Gate C / after 04.07:** new production Office host and reusable GUI with reliable tests; stop adding features to WasmSmoke.
- **Gate C2 / after 04A.48:** Windows-wide portable-code audit and extraction closure with source-to-owner ledger, including WinForms, nested `UI/Refedit/Excel2007RefEdit.vb`, graphics and UDF wrappers.
- **Gate C3 / after 04A.74:** 13 selected PDF enhancement topics numerically validated in Core/Common, visible in Excel-DNA, renderer-independent chart payloads verified and Phase 05 reuse map signed off.
- **Gate D / after 06.01:** real formula-model vertical slice proves multi-column/mixed-type inputs, model builder and diagnostic results. Reassess regression family scale-up.
- **Gate E / after 10.02 and 11.03:** renderer and Custom Functions/WASM feasibility decisions validated with actual Excel hosts. Change course based on measured results, not architecture optimism.
- **Gate F / before 12.02:** feature/capability matrix and deployment identity approved.
- **Gate G / after 13.06:** quantitative and GUI acceptance complete on each declared supported platform.

## 15. Definition of cross-platform feature complete

**A feature is cross-platform complete** only when all of the following are true:

1. One Core algorithm/implementation is used for statistical computation in Windows and Office/WASM; no second mathematical implementation in JS or WinForms.
2. An explicit Common descriptor, validated typed options and typed input exist, including missing/categorical/header/culture behavior.
3. Windows and Office hosts produce equivalent primary and optional results, including estimates, tests, p-values, confidence intervals, fit/convergence information, warnings and ancillary tables; numeric tolerances are documented per algorithm.
4. Any required chart/document artifacts can be rendered in each supported host through native Excel chart or documented fallback; no data/labels/statistical annotations are silently lost.
5. Output destination, start position, blank-row spacing, p-value formats, significance policy, footnotes and protected/occupied-range behavior are tested.
6. A usable Office task-pane workflow exists (selection, input-role assignment, options, preview, Run, results, errors/help). All controls are wired and keyboard-accessible to an agreed standard.
7. Core/Common unit tests, host adapter tests, UI component tests and numeric/metadata parity fixtures pass; at least one real Excel Online integration run and a Windows regression smoke pass.
8. For features advertised on Mac, a **real Mac desktop Excel** test passes. Merely building WASM or working in Excel Online does not imply Mac parity.
9. Runtime/cancellation/resource behavior is declared and safe; significant limitations visible before users trigger unsupported actions.
10. Documentation identifies supported hosts and any differences in native editability, output, storage, rendering or custom-function behavior.

**The Office add-in as a whole is cross-platform feature complete** when every intended Ribbon feature and UDF has an explicit inventory disposition: `Implemented/Verified on host`, `Supported with documented fallback`, or `Explicitly out of scope for this product release`. A banner saying 'feature complete' is **not permitted** while a promised Ribbon/UDF item is silently absent, even if Core already implements the math. Product release scope must be approved separately from numerical calculation completeness. The 84 Ribbon button entries are not 84 distinct capabilities; reconcile repeated commands against their underlying workflow. Likewise 268 UDF declarations must be individually reconciled against Office Custom Function names, even if the release consciously narrows the initially supported catalog.

## 16. Testing strategy, datasets and release matrix

### Automated and manual test matrix

| Test level | Main owner / project | Scope | Per-batch requirement | Release requirement |
|---|---|---|---|---|
| Core statistics/numerics | `BESHStat.Core.Tests` | Numerical reference, convergence, matrix/graphics calculations | For new Core extraction/bug fix | All existing and new tests green |
| Common unit | `BESHStat.Excel.Common.Tests` | Typed parsers, validation, registry/options, output layout, style, UDF argument conversion | **Every** Common batch | All handlers + inputs have schema tests |
| Contract snapshots | Common tests + JSON reference fixtures | Request/result/formatting serialization, backward compatibility | Changes to any DTO | No unexplained schema drift |
| Windows legacy regression | `BESHStatNG.Tests` + Windows Excel GUI | Existing feature semantics and UI wiring | Any Windows touched batch | Production behavior unchanged except approved improvements |
| Independent WASM smoke | `BESHStat.WasmSmoke` | Load Core/Common under browser WASM, execute simple + advanced methods | Common/Core dependency changes | ANOVA, GLM and representative heavy model start/finish |
| Office adapter JS | Office JS/TS unit tests, mocked Excel.run/context.sync | Cells, read/write, start cell, table layout, formatting, capability | Any Office adapter batch | Error/collision/recalc scenarios covered |
| Blazor component | `BESHStat.OfficeAddin.Tests` (e.g. bUnit) | Button/input wiring, defaults, validation, help, options, lifecycle | Any UI batch | All feature screens have click/option tests |
| Excel Online E2E | Browser E2E + manual workbook fixture | Sideload, selected range, task pane, output and Custom Functions | Every family release | Every promised feature exercised |
| Windows desktop Office.js | Host integration/manual | WebView, workbook/JS runtime differences | Infrastructure + feature release | Full selected parity suite |
| Mac desktop Excel | Real Mac Office add-in test | Requirement set, selection, output, charts, UDF, performance | When family flagged Mac-ready | Every Mac-promised feature verified |
| Cross-host parity | Golden dataset comparison harness | Tables, p-value targets, warnings, diagnostic arrays, chart specs | Every migrated analysis | No unexplained cross-host delta |
| Performance/memory | Benchmarks and browser profiling | Large data, chunked IO, WASM cold/warm, UDF 100–1000 recalc | High-risk batch | Thresholds approved; no memory leak |
| Cancellation/lifecycle | Common + Office tests | Cancel during model, close workbook, switch sheet, refresh | Models and host lifecycle batches | No stale writes, no false cancellation claims |
| Chart and export | SVG snapshots, geometry references, workbook image smoke | Axes/labels/resize/graphics fidelity | Every new plot family | Visual reference and exported files readable |
| Deployment/security | Manifest validation, HTTPS, asset fingerprint, dependency checks | Install, permissions, caching, rollback, version | Deployment batch | Supported installs/updates repeatable |

### Test data policy

Maintain a versioned, license-clean reference workbook/data library using the existing Core/Windows test assets as seeds. Separate **golden numerical vectors/tables**, **golden Common output documents**, **Office host E2E workbooks**, and **chart reference images**. Cover: (a) normal data, (b) ties/ragged groups, (c) missing/categorical, (d) near-singular/ill-conditioned, (e) error cells/nonfinite, (f) locale decimal/date, (g) empty/protected/merged output, (h) large datasets, (i) multiple worksheets and unstable selection, (j) seeded PSM/clustering and stochastic methods. Lock seeds and platform tolerance. Avoid comparisons of screenshots alone to judge numerical correctness.

**Acceptance template for every batch** (copy into future work requests):

```text
Batch ID / ZIP baseline / exact changes / backward-compatible API implications
1. Source inspection findings specific to this batch
2. Patch + updated standalone replacement files (in future implementation chats)
3. Automated tests added or updated and expected commands
4. One/two manual Excel GUI/WASM smoke steps
5. Expected numerical/output reference and acceptance results
6. Known platform limits, rollback, and next dependent batch
```

At the end of every batch, user supplies a new complete ZIP as the source of truth, confirms Windows build and Core/Common/legacy tests, and reports host smoke results. The next patch must **re-audit current files** rather than apply changes against the previous ZIP.

## 17. Implementation checkpoint, next batch and historical 01.10 acceptance notes

**Current Revision 1.4 planning checkpoint:** **02.06 is accepted/merged**, confirmed by the user after Windows and Excel Online acceptance. **02.07 is the current implementation**, pending independent review and live-host acceptance, not complete or accepted. The accepted checkpoint is **222 total / 25 completed / 197 outstanding**, including 02.07. Phase 04A remains after full Phase 04: preserve **04.07 → 04A.01–04A.74 → dependent Phase 05+ work**. No Phase 04A batch is implemented or authorized.

**02.08 safety handoff:** In Excel Online, 02.06 currently overwrites existing cells in the planned table rectangles without prompting. The user tested this and confirmed it; it was intentionally deferred to 02.08. That later batch MUST check **all** planned composite/table rectangles (not just the bounding envelope or first table), obtain consent when occupied, perform no destructive write when the user selects No, and truthfully report partial writes. The 02.07 opt-in composite path must not make this more dangerous; it writes only to a new empty worksheet and rejects current-sheet requests before mutation.


**Historical Revision 1.3 checkpoint (superseded):** batches through **01.09**, including **01.05A**, were implemented (18 completed; 130 outstanding). See §0, commit `2d9f418` and [the batch 01.09 architecture note](../docs/architecture/batch-01-09-acquisition-adapters.md). Live Excel and Mac acceptance remains separate. The user authorized only 01.10 on the existing `codex/01-10` branch; implementation and independent review do not authorize the next batch.

### 01.10 — Safe large-range acquisition and quotas (source-implemented and validated by reported full run; no fresh live Excel acceptance)

**Approved scope / likely files:** Office range batching and host-neutral `Common/InputSizeLimits.vb`, with narrow optional Windows preflight. **Dependency:** 01.09. **Host changes:** W=Optional O=Yes. **Risk:** High.

**Required automated tests:** Chunking, memory and cancellation preflight. **Manual smoke:** Read a large rectangular dataset. **Acceptance criterion:** Bounded batching with informative limit messages. Retain 01.09 canonical parity, ordering, identity verification and single-range analysis semantics. See [01.10 implementation/review notes](../docs/architecture/batch-01-10-large-range-acquisition.md).

These are the unchanged §13 requirements. This status correction records accepted 01.09; it does not declare 01.10 complete or authorize 02.01.

### 01.08 — Matrix, Classification, PSM, SPC and Chart-Source Input Parsers (**complete**)

Commit `5d5986d` implements the five Common input families using existing typed frame/category/numeric inputs, cell policies, source identities and structured diagnostics. Dimension checks, treatment/class-label validation, time/order checks and source-row alignment have ordinary, edge and compatibility tests, including deterministic matrix/SPC/PSM preview fixtures. The §13 dependency remains **01.05A**. Core math and `netstandard2.0` targets are unchanged; native error/missing-value metadata is preserved. Later host-adapter integration remains separate; no production GUI or LibreOffice adapter is added. See §0 and the architecture note for recorded validation and outstanding host acceptance.

### Completed 01.05A/01.06 acceptance rationale (retained)

### 01.05A — protect host-neutral spreadsheet semantics before complex inputs (**complete**)

**Objective:** allow `BESHStat.Excel.Common` to remain usable by any future spreadsheet host **without adding LibreOffice code now**. Fix the assumption in `HostCapabilityPreflight` that a usable spreadsheet host necessarily has `IsExcel=True`; separate host-reported capabilities from brand. Document precise interpretation of source addresses, Excel-only date serials and spreadsheet errors as host-supplied metadata. **Preserve existing Office.js and Windows behavior by default**.

**Scope / likely edits:** `BESHStat.Excel.Common/src/Host/HostCapabilities.vb`, small contract additions only when needed to `Contracts/SpreadsheetRangeReferences.vb` and `Input/CellValuePolicy.vb`; tests in `HostCapabilitiesTests.vb` and corresponding contract tests; `docs/architecture/` compatibility note. Avoid introducing a full platform-provider interface hierarchy or any new .NET project prematurely. `BESHStat.Core` algorithms should not change. Existing shared analysis IDs and generated YAML metadata should not change. **No UNO, Python, pythonnet, Wasmtime, Calc GUI or extension packaging work.**

**Dependencies:** 00.06, 01.05. **Risk:** Medium, limited to shared contract compatibility.

**Required tests:** unchanged success/failure for `WindowsExcelDna()` and `OfficeJsExcel()` capability profiles, including Office `NewWorkbook` rejection; simulated non-Excel host with explicitly provided read/write capabilities accepted; simulated non-Excel host missing a requested operation rejected; unknown/incomplete host rejected; `IsExcel` preserved for existing serialized consumers; current Excel A1 validation and Excel 1900/1904 date-policy tests unchanged. Add unit tests or documentation for a non-Excel source declaring an unknown/explicit date origin without silently assuming 1900 dates. Run Core/Common/legacy suites and node/browser tests on the actual development machine.

**Manual smoke:** Windows Ribbon ANOVA/Kruskal output (current/new worksheet) and Excel Online task-pane selected range/new worksheet. No LibreOffice smoke test required.

**Acceptance:** every existing Excel flow remains stable, `HostCapabilityPreflight` no longer makes Excel branding a universal spreadsheet requirement, and future date/address/error variability is documented without speculative Calc adapters.

### 01.06 — survival and longitudinal inputs (**complete**)

Implemented **01.06 — survival, time/status, subject/repeated and mixed-model grouping input parsers**, using the existing typed frame/category/validation contracts. Tests cover censoring/strata, cluster/subject identity, visit ordering, zero/negative times when disallowed, duplicate subject/visit pairs, missing values and diagnostic source rows. These remain Common-only input contracts; **no Office GUI or Python bridge** was added. 01.07 likewise compiles model inputs through existing Core formula/design services; it does not fit models or add host UI.

### Already completed, not to be repeated

00.01 → 00.02 → 00.02A → 00.02B → 00.03 → 00.04 → 00.05 → 00.06 → 01.01 → 01.02 → 01.03 → 01.04 → 01.05 → 01.05A → 01.06 → 01.07 → 01.08 → 01.09.

## 18. Reference documents for Office product assumptions

These are external Microsoft documentation links used to qualify **future** design decisions; they are not source-derived assertions from the BESHStatNG ZIP.

- Microsoft Office Add-ins manifests (add-in-only XML vs unified JSON): https://learn.microsoft.com/en-us/office/dev/add-ins/develop/add-in-manifests
- Create Excel Custom Functions (manifest permissions, namespaces and runtime): https://learn.microsoft.com/en-us/office/dev/add-ins/excel/custom-functions-overview
- Office add-in runtimes and shared runtime behavior: https://learn.microsoft.com/en-us/office/dev/add-ins/testing/runtimes
- Shared runtime sample, September 2026; unified-manifest Custom Functions noted as preview: https://learn.microsoft.com/en-us/samples/officedev/office-add-in-samples/office-add-in-shared-runtime-global-data/
- Excel JavaScript API requirement sets and version/platform availability: https://learn.microsoft.com/en-us/office/dev/add-ins/reference/requirement-sets/excel-api-requirement-sets
- Runtime requirement-set feature detection: https://learn.microsoft.com/en-us/office/dev/add-ins/develop/specify-api-requirements-runtime
- Custom Function cancellation and streaming semantics: https://learn.microsoft.com/en-us/office/dev/add-ins/excel/custom-functions-web-reqs
- Avoid modifying workbook from Custom Functions: https://learn.microsoft.com/en-us/office/dev/add-ins/excel/call-excel-apis-from-custom-function

---

**Historical Revision 1.3 checkpoint (superseded on 18 October):** 18 source-implemented batches through accepted 01.09, including 01.05A, before 01.10 was completed. The full roadmap, dependency columns, historical checkpoint sections and LibreOffice architecture-only constraints are retained. `methods.yml` remains the authoritative authored method catalog; Ribbon XML and VB `<ExcelFunction>` declarations retain their authority. No later batch is authorized by this correction.

## 19. External technical references for future-host feasibility (not implementation authorization)

- LibreOffice SDK, UNO `XCellRangeData.getDataArray` / `setDataArray`: https://api.libreoffice.org/docs/idl/ref/interfacecom_1_1sun_1_1star_1_1sheet_1_1XCellRangeData.html — spreadsheet range read/write exists, but per-cell typing/format semantics require closer adapter analysis.
- LibreOffice `NumberFormatSettings.NullDate`: https://api.libreoffice.org/docs/idl/ref/servicecom_1_1sun_1_1star_1_1util_1_1NumberFormatSettings.html — date serial origin is configurable, not synonymous with Excel1900.
- LibreOffice Python scripting: https://help.libreoffice.org/latest/en-US/text/sbasic/python/main0000.html — Python-UNO is an available scripting boundary.
- Python.NET embedding .NET into Python: https://pythonnet.github.io/pythonnet/python.html — `coreclr` can be chosen explicitly; feasibility within an embedded LibreOffice Python process is unverified.
- Microsoft Blazor WebAssembly architecture: https://learn.microsoft.com/aspnet/core/blazor — assemblies and the .NET browser runtime load together and interact through JavaScript; the existing app is not a standalone WASI library.

**Boundary:** this appendix only influences Core/Common abstractions. It does **not** add a LibreOffice Python bridge phase, implementation batch, or release requirement.

## 20. Revision 1.4 change record (18 October 2026)

- Adopted the checked-in Revision 1.3 planning baseline from the **18 October** ZIP rather than the older 16 October standalone master plan.
- Reconciled user-reported completion through **01.10**, documented source/test evidence and independently reran **74/74** Office.js Node tests. No `.NET`/Excel live rerun was possible here.
- Inserted **74 new batches (04A.01–04A.74)** immediately after Phase 04; preserved **all original IDs and descriptions** in Phases 00–13, except for additive dependency/reuse notes and current status.
- Added a complete **WinForms-inclusive** Windows portability audit and incremental Core/Common completion gate before enhancements, spanning backend, 32 behavioral UI files, 29 Designer files, 38 Graphics modules, 41 UDF files and shared infrastructure. The detailed static coverage ledger is available as a companion file: `BESHStatNG_Phase04A_Windows_Portability_Audit_20261018.md`.
- Scoped selected `Content.pdf` requests to **1, 2, 4, 5, 6, 7, 9, 13, 14, 15, 25, 38 and 39 only**, with existing-statistic reuse and statistical calibration gates.
- Kept LibreOffice an architectural constraint only; no PyUNO or external host implementation batch added. No source code or documentation generator files were modified.
