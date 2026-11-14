# BESHStatNG — Cross-platform development master plan

**Revision 2.14 · 12 November 2026 · Phase 04A.38 W2-v12 status refresh**
**Current owner-reported state:** Official batches **00.01–04.07 and 04A.01–04A.37 accepted and merged**. **04A.38 is NOT_READY, W2_IN_PROGRESS**, at the completed **continuation-v12 partial checkpoint**; W3 has **NOT_STARTED** and **04A.39 remains BLOCKED**. The source-backed continuation-v13 Codex prompt is prepared, but no v13 completion is reported in this revision. Phase 04A.38 has **not** been committed or merged.

| Roadmap measure | Count |
|---|---:|
| All numbered batches | **226** |
| Accepted (Phases 00–04: 40; Phase 04A: 37) | **77** |
| Not yet accepted (including in-progress 04A.38) | **149** |
| Phase 04A accepted / total | **37 / 75** |
| Phase 04A not yet accepted | **38** |

**Verification boundary.** Acceptance and merges through 04A.37 are **owner-reported**, not a fresh independent live-Git checkout. The latest independently inspected **uploaded derivative packages** are `BESHStatNG_04A38_ACTIVE_20261112-133023.zip` (recovered from binary volumes) and `BESHStatNG_04A38_REFERENCE_20261112-133023.zip`, with the v12 sealed owner review and final post-seal addendum v2. Their contents and archived frozen compatibility comparisons were independently reviewed; Windows builds/tests, live Git and excluded private files are supported by Codex receipts rather than independently rerun or rehashed here. The v12 Codex receipts identify `codex/04A-38` and pinned HEAD/`main`/merge-base `ba1636f5356ac761509c2cfa16fc66b92430935d`, with an empty staged index, no commit and `git diff --check` exit 0. Every subsequent Codex continuation must authenticate the actual live branch, source, preservation and sealed reference before editing.

**Binding restrictions (unchanged):** `P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED`. Gate B was accepted with E1–E7 exceptions; Gate C with recorded exceptions. No new native-control releases, original-obligation discharges, scientific-golden promotions, Office/Mac parity or Phase 04A.39 work follow from software-extraction progress. Phase 04A.38 requires completed W2, separately undertaken W3, independent readiness review and explicit owner acceptance/merge before downstream work.

## 1. Direction, authority and scope

**Architecture:** *two Excel hosts, one statistical engine*. `BESHStat.Core` (`netstandard2.0`) owns statistical methods and portable numerical/plot geometry. `BESHStat.Excel.Common` (`netstandard2.0`) owns typed input, orchestration/request/result/presentation contracts, capabilities, and host-neutral policies. `BESHStatNG` (Excel-DNA, .NET Framework 4.8) owns Windows Ribbon/WinForms, RefEdit, Excel COM, native charting and MSI. `BESHStat.OfficeAddin` (Office.js/Blazor WASM) is the future Excel Online/Mac/Windows Office add-in; `BESHStat.WasmSmoke` remains an independent prototype/testbed.

**Authoritative boundaries:**
- `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` — authored analysis names, categories, Help routes and navigation. Generated metadata must reconcile with it.
- `BESHstatNG/Resources/RibbonXML.xml` — deployed Windows Ribbon declarations, **not** evidence of available Office execution.
- Common typed handlers plus host capability checks — actual executable Office availability. In the accepted Office host, ANOVA/KW are the implemented runnable workflow; disabled catalog entries are not supported features.
- Excel-DNA `<ExcelFunction>` declarations — canonical UDF export inventory (268 at the accepted inventory), not a promise that all can be exposed as Office Custom Functions.
- Immutable `docs/architecture/` dossiers, `evidence/04A*/` manifests and original-control ledgers — detailed historical provenance. This living plan is not their substitute.

**Never:** reimplement Core statistics in JavaScript/GUI/UDF wrappers, fabricate worksheet row identities, silently change accepted numerical defaults, release native/COM operations without evidence, or treat host-free compatibility tests as independent R/scientific validation. Source and statistical semantics should converge across GUI/UDF/Office only via separately authorized, tested corrections.

**Help and documentation change control:** Update affected `methods.yml` entries, Help/MkDocs pages, versioned routes, examples, generated metadata and capability text **in the same batch when user-visible behavior changes**. 04A.72 closes earlier backlog; it does not excuse documentation omissions in later batches. Version-first Help resolution with latest fallback remains mandatory.

**Future LibreOffice/Calc** is an out-of-scope feasibility track. Keep portable Core/Common boundaries reusable; no current Python-UNO/.NET bridge, new batch or support claim.

## 2. Accepted implementation and current handoff

| Area | Accepted delivery | Continuing boundary |
|---|---|---|
| 00–01 | Catalog/registry and typed Excel range/cell, provenance, grouped/model/multivariate parsers; bounded acquisition | Stable IDs; locale, dates, errors, blanks, physical source coordinates and quotas |
| 02 | Semantic `OutputDocument`, p-value/formatting policy, ordered placement, safe write outcomes and capability/fallback | No silent overwrite, invented numerical display, or false write success |
| 03 | Shared settings, logging/diagnostics, Help/version/availability, progress/cancellation, lifecycle leases | Gate B E1–E7, privacy, source freshness and truthful cancellation |
| 04.01–04.07 | Production Office task pane, workbook bridge, catalog/navigation, live ANOVA/KW, reusable editors, output controls, settings, Help and Ribbon | Unsupported methods remain disabled; real export and Mac parity not yet delivered |
| 04A.01–04A.28 | Windows migration inventory, analyses, model/UDF input groundwork, SPC/PSM/multivariate/report/output integration | Original numerical/native issues and control restrictions remain in force |
| 04A.29–04A.33 | Graphics audit; portable ROC/Q–Q, Box/Histogram/Violin, XYZ/ScatterMatrix/3D geometry and Windows consumers | Old numerical engines, legacy quirks and COM rendering ownership preserved |
| **04A.34** | 17 specialty graphics families; 768 historical IDs and 764 pre-existing current identities reconciled; **zero OPEN member blockers** | Host-only and reserved members remain explicitly classified; native chart evidence not run |
| **04A.35** | Multiple-comparison CI, agreement references and Love series; 34 dispositions and three earlier reservations reconciled, **zero OPEN** | Original Love/native/scientific candidates stay restricted; no R/native run |
| **04A.36** | Accepted/merged: portable KM/Cox survival and univariate/multivariate SPC plotting payloads, with Windows consumption and documentation | Original statistical algorithms and native Excel/COM rendering/gates unchanged |
| **04A.37** | Accepted/merged after bounded late archived-entry coverage correction: portable PCA, factor, clustering and correspondence-analysis plot geometry/metadata | Original evidence/provenance protected; no native chart or scientific-golden promotion |
| Separate maintenance | Test rationalization branch accepted: five canonical SPC test source pairs still run 47 cases per runtime; isolated test runner | Not a numbered batch and not a waiver for failed historical tests |

**Earlier accepted validation reference (04A.35, historical):** Debug **20,374 PASS / 14 FAIL**, Release **20,374 PASS / 14 FAIL**, each with 48 new raw passing cases; Node **916 PASS**; Python **813 PASS / 132 FAIL / 11 ERROR**; new 04A.35 packet audits **32 PASS**. Strict/profile both exited **1** (gates still unmet); strict MkDocs exited **0**. Corrected, pre-edit **4,480-row V2 compatibility fixtures matched in both managed configurations**. The earlier invalid fixture and concurrent Framework deadline failures remain recorded rather than silently replaced; the complete final serial Framework rerun passed except for the nine inherited Framework failures. These are Codex Windows execution receipts, except the new audits and published source/manifest checks independently reviewed by ChatGPT. Live Excel/COM/UIA/RefEdit, independent R oracle, manual Office/Mac, signing and deployment were **NOT_RUN**. All comparisons remain `NOT_GOLDEN`.

### 04A.38 — latest partial W2 checkpoint (v12; not accepted)

| Reconciliation universe | v11 supported | v12 supported | v12 OPEN |
|---|---:|---:|---:|
| Historical behavioral bodies | 402 / 704 | **435 / 704** | **269** |
| Current Windows behavioral bodies | 432 / 726 | **465 / 726** | **261** |
| Distinct support declarations | 250 / 303 | **252 / 303** | **51** |

**What changed in v12:** 33 new historical/current closures (**26 native parent methods and seven separately indexed callbacks**) and two support closures (`MultipleCorrespondenceHandle`, `DiscriminantHandle`). The focused work covered multivariate/CA/MCA/DA report preparation and typed categorical/vector import, moving five reusable method bodies to `BESHStat.Excel.Common` while retaining compiled Windows consumers and native host/session/callback boundaries. Whole-body, callback, branch/inline and original/current identity ledgers retain the complete 704/726/303 universes and their exact OPEN complements; one portable helper call alone does not discharge a parent. Recorded fragment coverage is **10,457/20,683 original** and **6,086/14,732 current**. Additional fragment/container obligations remain OPEN.

**v12 validation receipts (Codex Windows execution; uploaded results independently reconciled, not rerun):** one full Debug managed run **20,834 PASS / 14 inherited FAIL** (**31 new PASS**); Python **830 PASS / 113 FAIL / 13 ERROR** across **956 parent tests**, with separate nonpass subtest/event totals **181 FAIL / 13 ERROR**; Node **916 PASS**; strict/profile/MkDocs exit codes **1 / 1 / 0** (not an all-green gate). Debug and Release compiled, **268 Excel-DNA exports** retained, and archived current-only compatibility matched **29,424 results per configuration**: **28,966 PRE_EDIT_FROZEN** plus **458 separately authorized `LATE_ARCHIVED_ENTRY_SUPPLEMENT`** cases (A–G). The full and selected Release **unit-test** suites were **`NOT_RUN_OWNER_PREFERENCE`**; mandatory Release compilation/ABI/CIL/compatibility checks still applied. These are behavioral compatibility checks, **not independent statistical validation** or native Excel/Office/Mac verification.

**Unresolved evidence and preservation:** 14 inherited managed failures, two historical privacy JSON **ERROR** parents (their earlier FAIL histories remain), source-pin failures, the unordered-SPC error and one newly observed failing Python subtest event are neither repaired nor waived. V12 reports **8,801 private pins** (5,661 inherited + 3,140 added), **45 retained physical archives**, 278 lineage pins, 100 protected mixed-model captures, 16 owner documents and 41 original UDF snapshots. The 4,213,129,746-byte interrupted v9 JSON and historical ignored `TestResults` (~138 GB reported) remain protected; the latter must not be bulk-traversed or deleted. The private files and current live Git were verified by Windows preservation receipts, not rehashed in the uploaded derivative review. Preserve all prior failures, post-seal corrections, authentic original source/reference captures and the distinction between pre-edit and late captures.

**Immediate work: 04A.38 continuation-v13 (W2 only).** Authenticate the final v12 `owner-review.md`, `continuation-v12-postseal-handover-addendum-v2.md`, final post-publication, source/closure/fragment, private-reader and Git receipts first. The grounded next 24 canonical OPEN historical identities comprise **14 Discriminant Analysis** method/helper identities followed by **10 PlotData UDF** method/helper/callback identities (including two separately identified lambdas); see `continuation-v12/remaining-work-queue-v1.json` and the prepared `BESHStatNG_04A38_W2_Continuation_v13_Codex_Prompt.md` for full source IDs. Finish genuine executable dependencies and preserve native Excel/COM and original/scientific semantics. Follow the owner-authorized efficient cadence: focused Debug tests after coherent packets, one full Debug campaign at source freeze, no routine Release unit suites, but Debug/Release compilation, CIL/ABI and frozen compatibility in both configurations, deterministic six-process Python validation, and one expensive privacy scan per checkpoint. No new original-reference capture, native/manual/R/scientific run, staging/commit/merge/push, W3, or 04A.39 without required authority. **Do not start W3 until all W2 identities and fragments actually close**; W3 then needs its comprehensive verification/readiness dossier and independent owner acceptance before 04A.39.

## 3. Contract and host design (implementation rules)

**Typed flow:** `Excel host adapter → source-identified range snapshot → Common parser/validation → typed analysis request/handler → Core fit/calculation → Common result + chart specs + output plan → host-specific writer/renderer`.

1. **Input.** Preserve `SpreadsheetRangeData`/range leases, physical row/column identities and `CoreDataTable` (do not invent a parallel table engine). Support scalar, vector, paired, grouped, categorical/contingency, formula/model, survival, longitudinal/mixed, matrix/classification, SPC, PSM and chart-source inputs through the accepted parsers. Document blanks/errors, date systems, locale, nonfinite values, orientation, missing and selection quotas. Pair values by the *defined* source/row contract, not accidental array offset.
2. **Analyses.** Use stable string `AnalysisId`s, validated typed handlers and immutable option/execution context (alpha, seed, culture, source provenance). Return structured results with tables, diagnostics, warnings/faults, optional predictions/residuals, chart payloads and versioned metadata; don't signal errors only through exception text.
3. **Output.** Reuse `OutputDocument`/`OutputPlan`, typed cells and ordered blocks (`Table`, `Text`, `Diagnostic`, `Chart`, etc.). Plan/inspect destinations before writes, require overwrite consent, reject stale sources, preserve numeric values separately from p-value display, and record partial/failed writes honestly. Unsupported new-workbook/chart/export capabilities remain disabled or have tested fallbacks.
4. **Charts.** Core computes statistics/geometry; Common describes series, axes, labels, CI/reference lines, units, annotations and accessibility; Windows Excel chart/shape operations and resource lifetimes stay native. Phase 10 later renders the **same payloads** in Office (native charts or SVG/Canvas fallback). Plot extraction does not equal Mac chart availability.
5. **UI, settings, Help.** Add future analyses via catalog-backed Blazor editor components and Common policies; maintain English/Slovak fallback, `Force English` default ON, stable versioned Help, scoped diagnostics and run correlation. Do not show demo readiness controls or synthetic functionality as production capability.
6. **UDFs.** Preserve the 268 source-declared exports. Phase 11 validates an Office Custom Functions JS↔WASM bridge before migrating stateless, spill and model-handle categories. Respect result shape/error semantics, workbook-scoped handles, cache/recalc invalidation, memory limits and no unrelated workbook write from a Custom Function.
7. **Lifecycle/security.** Preserve bounded tiled reads, source/output lease checks, cancellation truthfulness, permission checks, privacy-safe logging, HTTPS/CSP and workbook/worksheet disposal safety. Windows MSI/XLL packaging/signing remains a separate release track.

## 4. Complete numbered roadmap (226 batches)

**How to read:** Every ID and dependency from Revision 2.12 is retained. The rows below are a **concise execution index**, not new authority to broaden batch scope. Original detailed tests/manual smoke/source hints remain in Revision 2.12 and immutable batch dossiers. `Accepted` means the owner accepted that batch's explicitly reviewed scope, **not** that scientific/native release gates passed. Risk labels are copied from the prior plan where present.
### Phase 00 — Architecture locks and compatibility seam

*All 8 numbered batches in this phase were owner-accepted.*

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **00.01** **✓** | Stage 3.04 golden contract snapshots | 3.04 | Existing snapshot tests (developer reports passing) → Preserve numerical/output fixtures unchanged during 00.02A–B | Low |
| **00.02** **✓** | Introduced stable `AnalysisIds`, `AnalysisDescriptor`, `AnalysisCatalog` and `AnalysisCatalogTests.vb`; two descriptors | 00.01 | Existing catalog uniqueness/type/legacy mapping tests (developer reports passing) → Do not redesign or reimplement; reconcile only public metadata | Medium |
| **00.02A** **✓** | Validate methods.yml as the single authored catalog and reconcile method/Ribbon metadata. | 00.02 | YAML schema, 76 keys/slugs, 79 method Ribbon mappings, 84 XML button checks (5 commands excluded), action matching, nav refs, Bullet/Agreement regression tests → YAML is trusted canonical method metadata with **zero unexplained mismatches**; 00.02 APIs unchanged | Medium |
| **00.02B** **✓** | Generate deterministic Common analysis metadata and documentation for all declared UDFs, with parser/coverage tests. | 00.02A | Existing 2 `analysisId` links, generated help URLs/labels, deterministic regeneration, 268 UDF exports→268 docs, four PSM examples, old/new flag compatibility → No hard-coded competing user labels/help URLs, no change to numerics/dispatch, all UDF declarations represented | Medium |
| **00.03** **✓** | Build strongly typed handler registry and adapter | 00.02B | Wrong types, unknown IDs, registry dispatch, parity → Same result tables/metrics; no late binding | Medium |
| **00.04** **✓** | Define typed result artifact envelope, faults and warnings | 00.03 | Result schema/serialization; null/error cases → All report artifacts preserved; typed access | Medium |
| **00.05** **✓** | Audit/disposition transitional OfficeJs VB helpers | 00.04 | Golden old-new imports/writes; usage guard → Single authoritative Common input/output path; no regressions | Medium |
| **00.06** **✓** | Create capabilities contract and preflight metadata | 00.03 | Capability fallbacks; unsupported destination → Unsupported actions rejected before execution | Low |

### Phase 01 — Universal spreadsheet input and parsing

*All 11 numbered batches in this phase were owner-accepted.*

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **01.01** **✓** | Add workbook/range identity, structured range selection | 00.04 | Address parsing; source metadata; multi-worksheet → Stable source identity and source provenance | Medium |
| **01.02** **✓** | Implement canonical cell, missing, Excel error and date policies | 01.01 | Blank/error/Boolean/date/locales/1900-1904 tests → Deterministic no-coercion interpretation | Medium |
| **01.03** **✓** | Common validation/reporting and configurable header/orientation | 01.02 | No-header; long/wide; bad coordinates; errors → Legacy grouped preset preserved exactly | Medium |
| **01.04** **✓** | Scalar, vector, paired and grouped typed input parsers | 01.03 | Pairwise complete cases; unequal groups; bounds → Typed and row-aligned payloads usable | Medium |
| **01.05** **✓** | Rectangular data frame, category and contingency contracts | 01.03 | Categorical levels; row IDs; RxC shape → No dropped row IDs; exact category coding | High |
| **01.05A** **✓** | Harden host-neutral spreadsheet capabilities, source references and range semantics without implementing LibreOffice. | 00.06,01.05 | Simulated non-Excel capability snapshot can pass supported operations while unavailable operations fail; existing Excel profiles unchanged; date/coordinate/error seam contract tests → No Excel requirement hidden in generic `Supports`/preflight | Medium |
| **01.06** **✓** | Survival/time/subject/repeated/mixed data parsers | 01.05A | Censor/strata/ID/visit/cluster checks → All family-specific input constraints enforced | High |
| **01.07** **✓** | Formula/design matrix and model-input compilation | 01.05A | Contrasts; missingness; interactions; rank → Canonical model input matches Windows model matrix | High |
| **01.08** **✓** | Matrix, classification, PSM, SPC and chart-source parsers | 01.05A | Dimensions; treatment; class labels; time order → Validated inputs and source rows supported | High |
| **01.09** **✓** | Improve Office and Windows acquisition adapters | 01.01,01.05 | Adapter equivalence; multiple ranges; empty cells → Identical canonical source snapshots | Medium |
| **01.10** **✓** | Implement safe large-range acquisition and quotas | 01.09 | Chunking, memory and cancellation preflight → Bounded batching with informative limit messages | High |

### Phase 02 — Result document and output adapters

*All 8 numbered batches in this phase were owner-accepted.*

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **02.01** **✓** | Create output-document/artifact model from ResultTable | 00.04 | Table/footer/title/diag/plot artifact fixtures → No table metadata loss or host API coupling | Medium |
| **02.02** **✓** | Define canonical cell and semantic style/p-value instructions | 02.01 | Error mapping; alpha; styles and locale formatting → Same semantic formatting document | Medium |
| **02.03** **✓** | Centralize ordered placement/overlap calculations | 02.01 | Rows/cols, separators, target, collision bounds → Identical planned blocks and coordinates | Medium |
| **02.04** **✓** | Refactor Windows WriteSharedRequest to consume output plan | 02.02,02.03 | Interop plan adapter tests; Windows parity fixtures → Legacy styling/placement unchanged | High |
| **02.05** **✓** | Refactor Office.js writer to consume same output plan | 02.02,02.03 | JS mocked workbook adapter; normalized style tests → No policy/layout duplication in JS | High |
| **02.06** **✓** | Output capability, start-cell/new-sheet/new-workbook fallbacks | 02.04,02.05,00.06 | Sheet collision, protected and unsupported cases → No unsupported option falsely advertised | Medium |
| **02.07** **✓** | Combine diagnostics, charts, footnotes, links in one plan | 02.05 | Block placement and cross-type bounding → Complete multi-artifact result with no overlap | High |
| **02.08** **✓** | Implement safe overwrite and structured write outcomes | 02.06 | Cancel/partial failure/undo capability; diagnostics → No unexpected destructive write | High |

### Phase 03 — Common infrastructure, lifecycle and execution

*All 6 numbered batches in this phase were owner-accepted.*

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **03.01** **✓** | Create versioned app settings DTOs and preference store contracts | 00.04 | Defaults, migration, bounds, alpha parity → Persisted and effective settings visible | Medium |
| **03.02** **✓** | Host-specific logging, diagnostics and run correlation | 03.01 | Redaction; run ID; noncrashing logger → No raw worksheet data logged automatically | Medium |
| **03.03** **✓** | Execution state, progress and cooperative cancel; owner-confirmed Windows WinForms/UI-thread hotfix and Excel Online acceptance | 00.04 | State transitions; cancel-before-run; progress → Truthful cancellation support; historical evidence in 03.03 report | High |
| **03.04** **✓** | Stable safe presentation mapping | 03.02,03.03 | Stable codes, privacy, validation/warnings, cancellation/write precedence → Owner accepted and merged; actual writer state retained; historical automated evidence in 03.04 report | Medium |
| **03.05** **✓** | Common help/version/availability linked to validated `methods.yml` | 00.02B,03.01 | Exact version per-page verification; same-page latest fallback; metadata parity; both MkDocs build paths and custom output MIME artifact → Owner accepted and merged; preserve published resolver, activation and MIME build hook | Low |
| **03.06** **✓** | Operation-owned document/selection leases; source/destination identity, Windows/Office lifecycle, cancellation and RefEdit delete-sheet safety. | 01.01,03.03 | Close/switch/rename/delete/replacement, unsupported events, consent/disposal races, immutable source addresses, no Windows fixture; full Debug/Release gates → Phase 03 complete; owner acceptance reconciled with local main; no fresh Codex live-host claim | High |

### Phase 04 — Production Office.js host and reusable task-pane UX

*All 7 numbered batches in this phase were owner-accepted.*

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **04.01** **✓** | Production BESHStat.OfficeAddin foundation with scoped readiness, DI, JS initializer, test host and HTTPS sideload manifest. | 00.03,00.06 | WASM build/assets; DI and host-free startup tests; Node readiness tests → Core+Common load in real Excel owner-accepted; Gate C later owner-approved with exceptions | Medium |
| **04.02** **✓** | Typed production `Services/IWorkbookAdapter`, bounded tiled JS transport, fail-closed write seam and explicit read-only diagnostic | 04.01,01.09 | Typed C# transport/lifecycle tests and mocked range/sheet/write JS tests → No direct Office.js use in Blazor; owner-accepted on main `6b1e978` | Medium |
| **04.03** **✓** | Two-level group/method dropdowns from generated `methods.yml` metadata, verified descriptor/editor availability and reusable options UI | 04.01,00.03,00.02B | Real select render/change events, 12 groups / 76 documented methods, typed editor availability, state/lifecycle; existing dependencies → Accepted/merged on main `30d77ac`; historical pending wording superseded | Medium |
| **04.04** **✓** | Reusable grouped input/constraints and output presentation, bounded whole columns, physical-row long intersection, sticky tabs, direct Run and one-click guarded Write | 04.02,01.03,02.06 | Golden ANOVA/Welch/post-hoc and KW/Dunn values/styles/preferences → Accepted on main `659d4fb`; Common v1/E1–E7 unchanged; New Workbook remains unsupported by the Office writer; carry its feasibility assessment into the Gate C exception/limitation log | Medium |
| **04.05** **✓** | Reusable formula, subject/visit and categorical builders | 04.04,01.06,01.07 | Common/Core design equivalence, worksheet/active-sheet discovery, Excel-DNA captions, sparse physical provenance, typed roles/categories and accessible lifecycle controls; numeric post-hoc fixtures remain green → Read-only preview, no unsupported model fitting/write; passed full gates and owner acceptance | High |
| **04.06** **✓** | Production Options/Global Settings, versioned Help, diagnostics, progress, English/Slovak resources and **Force English ON by default** | 04.03,03.01–03.05 | Preference persistence and failure/retirement ordering, migration/defaults, localization override, consent retirement and security → Shared settings effective without duplicate policy; failed retirement never persists rejected changes; Help/diagnostics and consent remain safe | Medium |
| **04.07** **✓** | Production Office Ribbon and capabilities, shared pane routes, persistent Help/About, Options and disabled Export Chart placeholder. | 04.01–04.06 | Generator/manifest and runtime-command inventory; dispatch and accessibility; options, Help, lifecycle, E2E regression → Real ANOVA/KW editors and utilities reachable; unavailable methods and actual Chart Export disabled; no duplicate command panel or bypass of write safeguards | High |

### Phase 04A — Windows code migration and selected Excel-DNA enhancements (in progress)

**04A.01–04A.37 accepted/merged by owner report; 04A.38 is W2_IN_PROGRESS / NOT_READY (v12 checkpoint); W3 NOT_STARTED; 04A.39 BLOCKED; 04A.39–04A.75 remain planned.** The `04A.39–04A.48` defect/native authorization charter and 04A.75 histogram option remain binding. Later phases do not duplicate Core/Common already extracted here.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **04A.01** **✓** | Complete member-level Windows migration inventory, including UI/Designer, Graphics, UDFs, Ribbon and infrastructure; 3,866/3,866 members reviewed. | 04.07,00.02B | Method/callback and file-to-owner audit; no unclassified method → Every reusable function assigned Core/Common or explicitly host-only; candidate count frozen | Medium |
| **04A.02** **✓** | Freeze typed ANOVA/KW compatibility fixtures and qualified R/native reference evidence under the owner-amended scope (not golden/native released). | 04A.01,00.01 | Typed fixtures and R/native deferral documented; NOT_GOLDEN; no P/native release. | High |
| **04A.03** **✓** | Shared univariate selection, grouped roles and validation; retain COM/RefEdit acquisition on Windows. | 04A.02,01.03–01.05 | Grouping with blank/error/header/orientation; error diagnostics parity → UI calls typed Common role validators; Core never receives Range or Form | High |
| **04A.04** **✓** | Paired/blocked alignment and option policies; preserve source row identity and qualification of UDF positional behavior. | 04A.02,01.04,01.06 | Paired row alignment, blocks, missing and margins → Common inputs preserve source-row mapping and existing results | High |
| **04A.05** **✓** | 2×2/R×C and proportions request/table/report rules; paired McNemar and overflow corrections under native restrictions. | 04A.02,01.05 | OR/RR orientation, integer counts, categorical order, alpha and CI parity → No table statistics or numeric CI parsing remain in form code | High |
| **04A.06** **✓** | Assumptions, normality, outlier, homogeneity and descriptive supplemental presentation. | 04A.03,04A.02 | Full existing Shapiro/D’Agostino/AD/Grubbs table and warnings parity → Form handles view state only; stat/report assembly reusable | High |
| **04A.07** **✓** | ANOVA, nested/repeated, post-hoc/Welch/KW request and report alignment. | 04A.03,04A.04,04A.02 | Control comparison, unequal n, multiple-testing, output ordering goldens → Windows results flow via Common without calculating supplemental statistics in UI | High |
| **04A.08** **✓** | Model effect authoring, formula/reference and typed regression specifications. | 01.07,04A.01 | Terms, factor contrasts, hierarchies, duplicate detection and canonical formula serialization → Reusable model builder has zero ListBox, COM or WinForms dependencies | High |
| **04A.09** **✓** | Six-family regression request/report layer with current Windows wiring. | 04A.08,04A.02 | Design/offset/weights, fitted/residual outputs, convergence fixtures → Numerical fitting in Core, validated request/diagnostics in Common | High |
| **04A.10** **✓** | LM, Gaussian and binomial regression reports and projections; preserve restricted Windows strict-guard decision. | 04A.09 | NIST/GLM coefs, offsets and diagnostics regression cases → LM/binomial projections use Common result-document contracts | High |
| **04A.11** **✓** | NB2/ZIP/ordinal/multinomial integration; active Core constant-binomial/covariance corrections, gated Windows patches. | 04A.10 | Four-family reports, Core safeties, source/permission gating and nonpassing historical tests accounted for → Accepted software scope, six Help updates; original obligations OPEN and two Windows patches unapplied | High |
| **04A.12** **✓** | GEE options, fit/result/contrast projection and Windows integration; retain GEE numerical and native candidates. | 04A.08,01.06 | Cluster/order/family and completed report snapshots, supplied-start QIC failure characterization → Accepted Common/Windows integration; 35 original GEE obligations OPEN and dormant guard unapplied | High |
| **04A.13** **✓** | KM, weighted log-rank and Cox requests, reports, curves and residual payloads; known log-rank risk-set defect remains. | 04A.04,01.06 | Censor/tie/strata/curve/report parity, reproduced two R nonmatches and proven earliest-time censor risk-set cause → Actual Windows forms integrated, three Help pages updated | High |
| **04A.14** **✓** | MMRM/LMM acquisition, effective options and report selection in shipping WinForms. | 04A.08,01.06–01.07 | KR/Satt covariance and reference-grid fixtures; invalid option validation → Common model workflows own options/report semantics, not Windows controls | High |
| **04A.15** **✓** | MMRM/LMM covariance, optimizer, gradient and fitting-option projections. | 04A.14 | REML/ML and UN/CS/AR1 optimizer option validation → Windows forms only project validated covariance requests | High |
| **04A.16** **✓** | MMRM/LMM postfit LS-means, contrasts and inference report projections. | 04A.15 | Model-based contrasts and warning/effect order goldens → Host-independent mixed-model diagnostics and outputs | High |
| **04A.17** **✓** | PCA, MCA, clustering, factor and discriminant multivariate workflow (standalone CA remains separate). | 04A.02,01.08 | Scaling, missing policy, factors, priors, deterministic seeds/labels → Portable input/report/plot builders cover all six multivariate branches | High |
| **04A.18** **✓** | PCA/MCA/labelled-CA coordinates, reduced axes and projection; unlabelled CA fix remains gated. | 04A.17 | PCA scaling/row ID and CA/MCA table consistency → Reusable multivariate table/plot projection | High |
| **04A.19** **✓** | K-means/hierarchical initialization, membership and dendrogram intent. | 04A.18 | Seeded clustering, missing, empty cluster and dendrogram fixtures → Stable reproducible Common clustering requests/specs | High |
| **04A.20** **✓** | Factor/discriminant request/results; retain physical vs Core-local identity distinctions. | 04A.19 | Rotation/retention/priors confusion matrices and row labels → Statistical choices and result shape outside UI | High |
| **04A.21** **✓** | Univariate SPC Ui21 point, phase, exclusion, rule and control-limit policy. | 04A.02,01.08 | Phase windows, missing, constants, run rules, exclusions → WinForms only collects settings and invokes shared run | High |
| **04A.22** **✓** | Univariate SPC chart channels, axes, markers, labels and stage-boundary Windows consumption. | 04A.21 | X-bar, EWMA, CUSUM and p/np chart reference masks → Common documents/markers reproduce Windows outputs | High |
| **04A.23** **✓** | Multivariate SPC Ui22 request/phase/option and post-fit context; one Core Fit. | 04A.21,01.08 | T², PCA-Q, MEWMA, MCUSUM limit/signal fixtures → Shared model/plot payload preserves masks, alerts and labels | High |
| **04A.24** **✓** | Multivariate SPC ten-channel chart/report/warning/table projections. | 04A.23 | Hotelling T2, PCA-Q, MEWMA/MCUSUM seeded reference charts → Portable payloads without Excel chart collection access | High |
| **04A.25** **✓** | PSM matching options, source-row mapping, report and Love-plot projections; one Core Fit. | 04A.09,01.08 | Match ID/reproducibility, trimming, standardized mean difference goldens → Statistical policy in Core/Common; no duplicated matching in forms | High |
| **04A.26** **✓** | Eight sample-size planning families and agreement requests/reports. | 04A.04,04A.05 | Rounding, CI effect size, ICC and Deming variance variants → Range-free workflows and paired reports call reusable Common services | Medium |
| **04A.27** **✓** | Physical-row importer geometry correction and source-provenance sidecar for GUI/UDF transport. | 04A.03–04A.05,01.09–01.10 | Physical selected-row start/length across shifted ranges, identity/RowIds, coordinate/date/error/missing parity, large-range and RefEdit lease/teardown safety → One canonical typed transport with exact selection provenance | High |
| **04A.28** **✓** | Portable output-cell roles, p-value semantics, section order and three-destination placement in Windows writer. | 02.01–02.08,04A.02 | P-value, alpha, titles/footnotes, placement, rollback tests → Common owns semantic document; Excel writer owns API and workbook actions | High |
| **04A.29** **✓** | Read-only graphics audit: 38 Windows Graphics modules, 1,442 member records, canonical historical lineage and ownership map. | 04A.01 | Member crosswalk and source identity; no production changes. | Medium |
| **04A.30** **✓** | Existing Core ROC/Q–Q numeric foundations and bounded graphics helpers; Windows delegates without duplicated algorithms. | 04A.29,04A.02 | Existing numerical engine reused; source/compatibility fixtures preserved. | High |
| **04A.31** **✓** | Copy-safe ROC plot points/cutoffs/diagonal and Q–Q points/reference/permutations; Windows adapters consume Core projections. | 04A.30 | ROC cutoff XY cardinality and Q–Q 9-method cases; source-copy/permutation tests; native NOT_RUN. | High |
| **04A.32** **✓** | Portable Box/Histogram/Violin aggregates, classification edges, whiskers, axis/domain geometry and source ordinals. | 04A.31 | Box/Histogram/Violin parity, bin/axis/ordinal fixtures; 81 new managed passes/config; native NOT_RUN. | High |
| **04A.33** **✓** | Portable XYZ projection/cage/ticks, ScatterMatrix panel series and 3D wire-object geometry consumed by Windows. | 04A.32 | XYZ/Scatter/3D 9,472 pre-edit rows/config; 65 new cases/config; native NOT_RUN. | High |
| **04A.34** **✓** | 17 specialty graphics families reconciled (768 historical IDs, 764 pre-existing current identities); geometry/label contracts used by 14 Windows adapters; zero OPEN family blockers. | 04A.29–04A.30 | 17-family member map zero OPEN; 19,236 compatibility rows/config; 181 new cases/config; native NOT_RUN. | High |
| **04A.35** **✓** | Multiple-comparison CI/label/axes, agreement plot references and PSM Love series/source ordinals; close three 04A.34 reservations and 34 member/report dispositions. | 04A.34 | 34 dispositions zero OPEN; 4,480 V2 rows/config; 48 new cases/config; native NOT_RUN. | High |
| **04A.36** **✓** | Accepted/merged: portable KM censor/step/CI, Cox baseline/deferred residual payloads and univariate/multivariate SPC chart/limit/annotation data with Windows consumers; native rendering and disabled Cox residual UI preserved. | 04A.35 | Frozen KM/Cox/SPC compatibility and source/row/phase geometry obligations retained; no scientific/native activation or Mac parity. | High |
| **04A.37** **✓** | Accepted/merged: portable PCA/factor/cluster/CA plot geometry, axes, score/loadings/biplot/dendrogram labels; final late archived-entry supplement/correction evidence retained. | 04A.36 | Original/late coverage authority distinguished; source and native-only rendering boundaries preserved; no scientific-golden promotion. | High |
| **04A.38** **W2 IN PROGRESS** | Controlled extraction/reconciliation across 268 Excel-DNA UDF exports, 41 original UDF snapshots and Windows UDF-support bodies; Windows registration, cache and native session state remain host-owned. | 04A.01,01.09 | **v12:** 435/704 historical, 465/726 current, 252/303 support; **269/261/51 OPEN** plus fragments/containers; 29,424 compatibility cases/config (28,966 pre-edit + 458 approved late); W3 not started, NOT_GOLDEN, NOT_READY; no 04A.39 until W2+W3/acceptance/merge. | High |
| **04A.39** | Reconcile UDF argument/data-import policies, row provenance, nested numeric-text precision and paired positional/source-row differences; establish single defect register. | 04A.38 | Exact source-row/positional equivalence; issue-register entry and triage; no blanket scientific fix. | High |
| **04A.40** | Regression, mixed, GEE, Cox, log-rank and formula UDF request/result parity; investigate LM alpha, QIC, cluster/prediction identities and controlled scientific corrections. | 04A.39 | Independent numerical/GUI/UDF cases; candidate decisions and unchanged gate unless specifically authorized. | High |
| **04A.41** | Basic statistics, contingency, agreement, assumptions and sample-size UDF reconciliation; Shapiro, D’Agostino, nested labels, Liddell and scientific candidates. | 04A.39 | Independent p-value/CI/assumption reference; explicit policy and native decision for any behavior change. | High |
| **04A.42** | Multivariate/classification/PSM and plot-source UDF payloads, row mapping and arrays. | 04A.39 | PCA, clustering, PSM and plotted data output snapshots → Typed portable result contracts and row order preserved | High |
| **04A.43** | Shared UDF model-handle semantics, cache ownership, invalidation and recalculation; Windows session cache stays host-owned. | 04A.40–04A.42 | Handle invalidation, workbook lifecycle, parallel access and no stale cache leakage → Shared model result semantics without Windows `ConcurrentDictionary` dependency | High |
| **04A.44** | Portable settings, defaults, Help, errors and progress from Windows infrastructure; keep OS/UI/updater actions native. | 03.01–03.06,04A.01 | Alpha/help/locale/runtime fault behavior; no-host dependencies → Neutral policy shared; files, registry, UI controls, updater remain Windows | Medium |
| **04A.45** | Windows UI/Ribbon/RefEdit wiring and safe harness; verify actual Excel session/HWND/PID/XLL attribution and obtain separate COM risk approval before native observation. | 04A.03–04A.44 | Host/session and loaded-build attribution, negative tests, independent COM risk approval before live execution. | High |
| **04A.46** | GUI wiring and validation of grouped, paired, categorical, normality, nested ANOVA, sample-size and plot options. | 04A.45 | Executed or safely isolated UI wiring; original restricted native controls remain OPEN without approval. | High |
| **04A.47** | Regression/mixed/multivariate/SPC/PSM/Cox GUI wiring, cancellation and faults; gated LM/GEE/other native activations only with exact authorization. | 04A.46 | Event/state/error/Cancel evidence; gated correction activation only after case-specific approval. | High |
| **04A.48** | Windows migration and deferred-issue exit register: Core/Common ownership, UDF/GUI parity, original obligations and restricted exceptions. | 04A.01–04A.47 | Every original issue exactly one FIXED_AND_VERIFIED / RETAINED_RESTRICTED / DEFERRED_OWNER_APPROVED; Gate C2 not auto-promoted. | High |
| **04A.49** | Violin connect-means/min/max lines with independently configurable options and portable data/spec. | 04A.30,04A.48 | Unequal/empty groups; mean/min/max, missing/ordering and chart data snapshots → Independent toggles and Excel chart matching neutral spec | Medium |
| **04A.50** | Friedman tie-aware sum-rank column alongside existing mean ranks; no change to test statistics. | 04A.04,04A.48 | Rank totals = blocks * means; ties, identical groups and missing blocks → Adds Sum Rank column without changing legacy statistics | Low |
| **04A.51** | Friedman–Nemenyi critical-difference inference and typed report, separately from Conover/Dunn. | 04A.50 | Compare independent reference implementations; small/large k; alpha, ties, critical quantiles → Validated CD, mean rank contrasts and correctly described method | High |
| **04A.52** | Friedman–Nemenyi CD diagram and mean-rank plot; labels/groups/intervals match the table. | 04A.51,04A.34 | Connected non-significant ranks, overlap, sorted labels and SVG geometry fixtures → Graph agrees with CD table; legends and confidence-level wording correct | High |
| **04A.53** | Dunnett multiple-vs-control inference with unequal sample sizes and simultaneous critical values/p-values. | 04A.07,04A.48 | Numerical references/MC calibration as needed; equal/unequal n and alpha → Validated familywise inference and control direction; no independent t-test substitution | High |
| **04A.54** | Typed Dunnett control semantics, options and ANOVA report. | 04A.53,04A.07 | Control matching, blank labels, no-control, multiple-controls and invalid selection → Typed control semantics and result tables shared by future Office host | High |
| **04A.55** | Dunnett simultaneous CI/difference charts and Windows options. | 04A.54,04A.34 | Simultaneous intervals, reference-zero, direction, alpha label → CI plot and results correspond exactly | Medium |
| **04A.56** | Hsu multiple comparisons with the best (MCB), both best directions and simultaneous bounds. | 04A.07,04A.48 | Independent references and least-favorable configuration, ties, unequal n → MCB bounds validated; not relabeled Tukey intervals | High |
| **04A.57** | Typed Hsu MCB options and best-highest/lowest report. | 04A.56 | Direction, near ties, duplicate labels and alpha validation → Tables show correct best reference and simultaneous comparisons | High |
| **04A.58** | Hsu MCB bound plot and Windows UI using neutral multiple-comparison spec. | 04A.57,04A.34 | Bounds/endpoint/no-change markers, significance and label tests → Chart supports Excel today and Office renderer later | Medium |
| **04A.59** | Mann–Whitney categorical/rank histogram and optional mirrored plot. | 04A.03,04A.30 | Ties, category binning, unequal group sizes, zero counts → Chart consistent with selected data and U statistic; portable data/spec | Medium |
| **04A.60** | Cochran C group-variance outlier inference; distinct from Cochran Q and Cochran–Armitage. | 04A.06,04A.48 | Group replication, equal n assumption, max variance selection and reference values → Stat method distinct from existing Cochran Q / Cochran–Armitage | High |
| **04A.61** | Cochran C variance-by-group chart, critical line and highlighted group. | 04A.60,04A.34 | Threshold, zero/all-zero variances, group labeling and chart geometry → Chart and table agree, no false universal assumptions | Medium |
| **04A.62** | One-sample empirical-vs-fully-specified continuous CDF Kolmogorov–Smirnov test (D+, D−, D). | 04A.06,04A.48 | Compare R/SciPy fully specified CDF; ties, D+/D−; no fitted-normal p-value substitution. | High |
| **04A.63** | Fitted-normal KS calibration (Lilliefors or validated bootstrap) separate from fully specified KS. | 04A.62 | Separate fitted-parameter calibration and seeded reproducibility; report N not invented df. | High |
| **04A.64** | KS ECDF/CDF and maximum-separation plot; numeric D agrees with geometry. | 04A.62–04A.63,04A.34 | D+ and D− maximum locations, boundary steps and axis snapshots → Numerical D equals visual maximum separation; chart spec portable | Medium |
| **04A.65** | Grubbs outlier plot reusing existing Core statistic and exact flagged source identity. | 04A.06,04A.34 | Outlier index, ties, alpha, N limits, source row mapping → Existing Grubbs numerical result unchanged; plot highlights the same row | Medium |
| **04A.66** | 2×2 OR/RR forest CI plots with log axis, null=1, method-specific intervals. | 04A.05,04A.34 | Woolf/Cornfield/zero-cell policies, finite intervals, log transformation → Plot matches Core intervals and displays method-specific CI clearly | Medium |
| **04A.67** | R×C Kendall tau-b/c, Goodman–Kruskal Gamma and Somers D CI plots using existing estimates. | 04A.05,04A.34 | Bounded -1..1 association, columns-dependent orientation, CIs and asymmetric sign → No recomputation of existing 4 statistics; CI axis shows 0 correctly | Medium |
| **04A.68** | Detrended Q–Q residuals/line delta for existing score/reference methods; immutable source. | 04A.30 | Zero residual under reference fit, distribution tails, sorted-copy input and line choices → No mutation of user data; residual definition matches selected line method | Medium |
| **04A.69** | Detrended Q–Q Windows chart option and zero reference line; ordinary chart unchanged. | 04A.68,04A.34 | Detrended sign, zero-line, layout and chart regression → User can select either/both without changing existing Normal Plot result | Medium |
| **04A.70** | Paired t-test difference box plot preserving paired row alignment and subtraction direction. | 04A.04,04A.30 | Before-minus-after vs after-minus-before, quartiles and missing pair/ID mapping → Difference box plot exactly matches tested paired differences | Medium |
| **04A.71** | Cross-feature chart semantics/accessibility: CI/reference/alpha metadata, labels, warnings and palettes. | 04A.49–04A.70,04A.75,02.07 | Charts never encode numeric inference only in color/style; accessible reference/CI labels. | Medium |
| **04A.72** | Reconcile all changed Windows workflows with methods.yml/Ribbon/Help/MkDocs/screenshots, including legacy documentation backlog. | 04A.49–04A.71,04A.75,00.02B | Complete source→Help and behavior-delta crosswalk; no false Office availability. | Medium |
| **04A.73** | New-feature typed result/chart-spec serialization and .NET 10 WASM-compatible contract checks; no Office UI activation. | 04A.71–04A.72,04.07 | No COM deps; portable serialization and WASM smoke. | High |
| **04A.74** | Phase 04A exit: migration parity, Windows/software test matrix, enhancements, graphics snapshots, docs and no-duplicate handoff to 05/10/11. | 04A.48,04A.73,04A.75 | Owner exit/ledger, Windows and cross-host tests, 13 selected enhancements plus PrettyBreaks; unresolved restrictions explicit. | High |
| **04A.75** | Histogram PrettyBreaks toggle (On by default) vs raw unsnapped rule-derived bins for ordinary and categorical plots, including compatible UDF defaults. | 04A.30,04A.34,04A.48 | Pretty On preserves current outputs; Off matches theoretical raw counts (all four rules); correct edges, frequencies, small-N/degenerate data, categorical pooled bins, UI/UDF parity. | High |

### Phase 05 — Low-to-medium complexity statistical workflows

*Planned.* Office statistical workflow exposure; prerequisite 04A.74 also applies to all 05–11 batches.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **05.01** | Bring ANOVA+KW supplementary outputs to shared handler | 02.07,04.04 | Optional variance/descriptive/box and Dunn/posthoc parity → No Windows-only statistical supplemental calculations | High |
| **05.02** | Descriptive statistics table workflow and shared summary options | 05.01,01.04 | Reference means/SD/quantiles and p-value metadata → Complete descriptive table parity | Medium |
| **05.03** | Independent two-sample t-tests with variance choices | 01.04,04.04,02.05 | Equal/unequal variance and CI reference → Independent t table parity | Medium |
| **05.04** | Mann–Whitney core workflow and rank-table contract | 01.04,04.04 | Mann–Whitney ties/exact cases → Rank grouping/result contract proven | Medium |
| **05.05** | Common blocked/subject input-to-report adapter | 01.06,04.04 | Blocked data validator and row mapping → Shared blocked-report boundary usable | High |
| **05.06** | Contingency and proportions input-to-report foundation | 01.05,04.04 | Counts/levels/rates and p-value metadata → Common categorical reporting path established | Medium |
| **05.07** | Agreement input/output protocol and neutral plot payload | 01.04,04.04 | Pairs, reliability options, result metadata → Agreement result schema ready | High |
| **05.08** | Sample-size option/input/output framework | 03.01,04.04 | No-range scalar options and rounding contracts → Can dispatch range-free analytical requests | Medium |
| **05.09** | Normality tests and distribution checks | 05.02 | Normality algorithms and missing values → All normality choices use shared results | Medium |
| **05.10** | Outlier tests and flagged-row annotations | 05.02 | Outlier flags and source row mapping → Flags and row IDs agree | Medium |
| **05.11** | Variance, homogeneity and symmetry workflows | 05.02 | Levene/Bartlett/Fligner/symmetry fixtures → All tables/p-values match Windows | Medium |
| **05.12** | Paired tests as a distinct workflow | 05.03 | Pairwise missing and paired t fixtures → No groupwise misalignment | Medium |
| **05.13** | Spearman correlation workflow | 05.04 | Spearman ties/reference → Spearman outputs and labels parity | Medium |
| **05.14** | Wilcoxon signed-rank workflow | 05.04 | Signed ranks/ties/exact fixture → Signed rank parity | Medium |
| **05.15** | One-way repeated-measures ANOVA | 05.05 | Sphericity and repeated subjects → Correct subject/visit mapping | High |
| **05.16** | Friedman blocked rank workflow | 05.05 | Blocked rank inference → Friedman parity | High |
| **05.17** | Exact Fisher and 2x2/proportions outputs | 05.06 | 2x2 odds, CI and exact tests → Result formatting/parity | Medium |
| **05.18** | RxC and Mantel–Haenszel strata | 05.06 | Stratified estimators and invalid counts → Strata options and table parity | Medium |
| **05.19** | Passing–Bablok agreement regression | 05.07 | Slope/CI and ties → Numerical and plot-spec parity | High |
| **05.20** | ICC agreement methods | 05.07 | ICC type and CI fixtures → ICC options/result parity | High |
| **05.21** | T-test and proportion sample-size utilities | 05.08 | Power/alpha/integer rounding → One-input family panels work | Medium |
| **05.22** | Advanced sample-size input schema and execution adapters | 05.08 | Power option/default contract → Shared sizing schema ready | High |
| **05.23** | Deming regression agreement workflow | 05.19 | Slope/intercept, weighting and CI fixtures → All Deming options parity | Medium |
| **05.24** | Bland–Altman agreement and limits plots | 05.19 | Bias, limits and plot-source positions → Limits and diagnostic payload parity | Medium |
| **05.25** | Weighted Cohen kappa agreement | 05.20 | Weights/coding/tables → All weight choices supported | Medium |
| **05.26** | Lin concordance correlation agreement | 05.20 | CCC/CI/paired-data reference → Numeric parity | Medium |
| **05.27** | Logrank and Cox sample size calculators | 05.22 | Event power and allocation cases → Survival calculators parity | Medium |
| **05.28** | ICC and Bland–Altman sample size | 05.22 | Reliability/sample-size power → Reliability calculators parity | Medium |
| **05.29** | Kendall correlation workflow | 05.13 | Kendall tie variants/reference → Tie-corrected parity | Medium |
| **05.30** | Theil–Sen regression workflow | 05.14 | Slope/confidence and rank/reference → Model and chart payload parity | Medium |
| **05.31** | Cochran Q and Skillings–Mack blocked tests | 05.16 | Block data and missing cases → Blocked test options parity | High |

### Phase 06 — Regression and advanced categorical models

*Planned.* Office formula/regression handlers and model diagnostics, reusing extracted Core/Common.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **06.01** | Linear model formula/design and result workflow | 01.07,04.05,02.07 | NIST parity; Type I/II/III ANOVA; diagnostics → Coef/ANOVA/residual/fit tables retained | High |
| **06.02** | Poisson/log GLM shared workflow and model-result contract | 06.01 | Poisson/link model table parity → Current WASM-tested model exposed in production shell | High |
| **06.03** | GLM extended-family registration and option contract | 06.02 | Family option mapping and fault codes → Typed family contract ready for distinct handlers | High |
| **06.04** | GEE cluster/covariance workflows | 01.06,06.02 | Indep/Exch/AR1/Toeplitz/UN reference → Stable covariance options and diagnostics | High |
| **06.05** | Multinomial and ordinal logistic models | 06.01,01.05 | Coding, links, baseline levels, fit tables → All category contrasts and warnings preserved | High |
| **06.06** | Regression diagnostic, predictions and chart artifacts | 06.01-06.05 | Residual, influence, fitted and diagnostic vectors → Row alignment and fault/convergence displayed | High |
| **06.07** | Gaussian/identity GLM reporting | 06.02 | Gaussian GLM reference → Family parity | Medium |
| **06.08** | NB2 regression fit, dispersion and warnings | 06.03 | NB2 dispersion/convergence → NB2 result parity | High |
| **06.09** | ZIP model distinct EM/results workflow | 06.03 | EM stopping and fit criteria → Same convergence/fault reporting | High |
| **06.10** | Binomial GLM and contrast specifics | 06.07 | Binomial/link/odds outputs → Results and residual parity | Medium |
| **06.11** | Gamma GLM and dispersion specifics | 06.08 | Gamma deviance/link and fault codes → Statistics and model warnings parity | High |

### Phase 07 — Survival, clustered and mixed models

*Planned.* Survival/mixed models and new separately validated competing risks; table workflows before Phase 10 curves.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **07.01** | Kaplan–Meier and log-rank workflow | 01.06,04.04 | Censor/ties/strata and curve payloads → Survival tables and curves available | Medium |
| **07.02** | Cox PH model and censor/strata options | 07.01,06.01 | Breslow/Efron/Exact; residual/failure parity → Same estimates and diagnostics | High |
| **07.03** | MMRM covariance and repeated-measure model workflow | 01.06,04.05,03.03 | UN/CS/AR1/HCS etc; KR/Satt; LS means → Covariance/inference/diagnostics preserved | High |
| **07.04** | LMM random-effects/fit and inference workflow | 07.03 | Random terms, REML/ML, KR/Satt/BW → Formula, estimates, diagnostics parity | High |
| **07.05** | Mixed/survival post-estimation and model diagnostics | 07.02-07.04,06.06 | Survival curve, contrasts, LS means, fitted IDs → No host-specific inference calculation | High |
| **07.06** | MMRM post-estimation and covariance structures | 07.03 | Covariance structures and contrasts → Full inferential choices represented | High |
| **07.07** | LMM random-effect variants and fit summaries | 07.04 | Random slope and fit comparison → Variant parity and diagnostics | High |
| **07.08** | Aalen–Johansen cause-specific cumulative-incidence estimator for competing risks; portable event codes, risk sets, uncertainty only when validated. | 07.01,01.06,02.07 | Independent competing-risk oracle, ties/censor/event-code cases; no golden declaration without review. | High |
| **07.09** | Fine–Gray subdistribution-hazard regression: cause-specific failure request, covariates, baseline/CIF predictions and independent validation. | 07.08,07.02,06.01,03.03 | Independent R cmprsk/reference checks, assumptions and convergence faults; no false Cox PH equivalence. | High |
| **07.10** | Wire validated Aalen–Johansen and Fine–Gray tabular workflows into Windows and Office feature surfaces with truthful capabilities. | 07.08–07.09,04.05,02.07,03.03 | Validated tables/host exposure; no 10.x plots before tabular validation. | High |

### Phase 08 — Multivariate analysis

*Planned.* Office multivariate analysis and plot payloads.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **08.01** | PCA/Hotelling and basic scores/loadings plots | 01.08,10.01 | Eigen/score/loadings, source rows → Numerical table+plot specs retained | Medium |
| **08.02** | CA and MCA workflows | 08.01 | Contingency levels, factor maps → Maps and axis semantics available | Medium |
| **08.03** | Hierarchical and k-means clustering | 08.01,03.03 | Distance/linkage/seed; dendrogram geometry → Stable labels, assignments and dendrogram | High |
| **08.04** | Factor/discriminant common input-to-result contract | 08.01 | Rotation/class label validated schemas → Advanced model schemas support both methods | High |
| **08.05** | Multivariate diagnostics and complex plot labels | 08.02-08.04,10.02 | Label collision/scaling/reference fixtures → Readable chart and data export | High |
| **08.06** | Factor analysis/rotation details | 08.04 | Factor/rotation/loading fixtures → Rotation options and tables match | High |
| **08.07** | Discriminant analysis classification details | 08.04 | Prior/class scores/misclassification → Prediction labels/source row parity | High |

### Phase 09 — SPC and propensity score matching

*Planned.* Office SPC and PSM workflows.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **09.01** | Univariate Shewhart SPC workflow | 01.08,10.01 | Xbar and R control-rule fixtures → Ordering and limits parity | High |
| **09.02** | Multivariate SPC and time-weighted variants | 09.01 | T2, CUSUM and covariance fixtures → Statistics/rule diagnostics preserved | High |
| **09.03** | PSM nearest-neighbor matching base workflow | 01.08,06.02,03.03 | Nearest-neighbor, seed and row mapping → Base model/selection artifacts shared | High |
| **09.04** | PSM inference, sensitivity, balance and Love plot payloads | 09.03,10.02 | Weights/strata/bootstrap/balance fixtures → Full inference and plots accessible | High |
| **09.05** | Capability analysis and univariate SPC variants | 09.01 | Process limits, phases and rules → All rule flags available | High |
| **09.06** | PSM matching options and overlap diagnostics | 09.03 | Caliper/trim/common support/strata → Deterministic match/reference parity | High |

### Phase 10 — Chart specification, renderers and export

*Planned.* Office chart specifications, renderer/fallbacks and actual export. Does not recalculate Core statistics.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **10.01** | Define portable chart document and renderer capability choice | 02.07,00.06 | Serialization, axes/legend/types → No Office or COM chart types in Common | Medium |
| **10.02** | Implement basic Office native charts and SVG fallback | 10.01,04.02 | JS renderer mocks; SVG snapshots → Capability-based render choice works | High |
| **10.03** | Histogram/box/violin/categorical histogram family | 10.02 | Bins, geometry, resize snapshots → Plot scales/box alignment stable | High |
| **10.04** | CDF/ECDF, ROC, Q–Q, KM/Cox survival charts plus cause-specific Aalen–Johansen CIF and Fine–Gray prediction curves after Phase 07 validation. | 10.02,07.01,07.08–07.10 | Renderer fidelity for CIF/time step curves and Fine–Gray prediction; no new inference in renderer. | High |
| **10.05** | Specialized vector-chart spec and placement infrastructure | 10.02 | Polygons/path/label placements → Reusable specialized shape primitives | High |
| **10.06** | PCA/CA/multivariate, SPC, agreement, Love, comparisons | 08.05,09.04,10.02 | Intervals, labels, multi-panels, chart parity → No missing statistical annotations | High |
| **10.07** | Advanced chart preview/fallback policy | 10.02 | Capability/fallback selection → Renderer fallback explicitly documented | High |
| **10.08** | Chart insertion, resize, export SVG/PNG and PDF provider | 10.03-10.07 | Image fidelity, dimensions, download limits → Charts export per declared capabilities | High |
| **10.09** | Sankey ribbon and flow geometry | 10.05 | Sankey paths/flow sums → Sankey geometry fidelity | High |
| **10.10** | Polar chart specification and circular axes | 10.05 | Angles and radial axes → Polar geometry fidelity | Medium |
| **10.11** | Scatter matrix and XYZ/3D fallback | 10.07 | Fallback performance/screenshots → Explicit static fallback where needed | High |
| **10.12** | Dumbbell/ladder annotation-spec details | 10.09 | Pairs and connector vector geometry → Labels and connectors placed correctly | Medium |
| **10.13** | Bullet chart thresholds/targets and captions | 10.09 | Band/target/axis rules → Band geometry/format parity | Medium |
| **10.14** | Convex hull overlays and group labels | 10.10 | Hull orientation/group outlines → Polygon and scale parity | Medium |
| **10.15** | Kite chart geometry/axis specifics | 10.10 | Kite widths and category ordering → Correct geometry and labels | Medium |

### Phase 11 — Cross-platform Excel Custom Functions

*Planned.* Office Custom Functions runtime bridge and UDF schema, from stateless to model handles.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **11.01** | Approve complete source-derived 268-UDF classification, UDF→`methods.yml` association and metadata catalog | 00.02B | Names, aliases, docs, duplicate detection → Every function has explicit migration status | Medium |
| **11.02** | Shared UDF typed input/optional/missing/error converters | 11.01,01.02 | Scalars/ranges/errors/optional/spill shape → Identical coercion/error semantics | High |
| **11.03** | Prototype shared JS custom-functions + WASM runtime | 04.01,11.02 | Metadata load, promise/fault, warm runtime → Core invoked from Excel recalc, not rewritten | High |
| **11.04** | Migrate scalar/stateless test and distribution UDF slice | 11.03 | Scalar parity, formula errors/locale → Acceptable cold/warm invocation latency | Medium |
| **11.05** | Migrate vector/matrix and table-spill UDF slice | 11.04 | Dynamic spill, blocked cells, 2D shape → Return dimensions/values match Windows | High |
| **11.06** | Design session model handle/cache and invalidation | 11.03,06.01 | Unique handles, reopen, stale, workbook switching → No invalid cross-session model reference | High |
| **11.07** | LM model-handle Custom Functions vertical slice | 11.06,07.05 | LM fit/coef/cache invalidation → First handle-based formula truly works | High |
| **11.08** | Advanced model-handle schema and array-result adapter | 11.06,08.05,09.04 | Advanced handle types and dynamic result mapping → No per-family handle semantics duplicated | High |
| **11.09** | Performance, worker feasibility, cache/memory safety and UDF release audit | 11.04-11.08 | 100/1000 calls, cancellations, cache eviction → Meets budgets without task-pane lock | High |
| **11.10** | Office functions for GLM and GEE fit/query | 11.07 | Recalc/warm handles and covariance → Parity and cache lifecycle | High |
| **11.11** | Shared regression model UDF handle pattern for survival/mixed | 11.07 | Typed handle schema and invalidation → Survival/mixed UDF reuse boundary ready | High |
| **11.12** | PCA fit/query Custom Functions | 11.08 | PCA cache and score spill → PCA function parity | High |
| **11.13** | PSM fit/query Custom Functions | 11.08 | PSM cache, weights and effect outputs → PSM function parity | High |
| **11.14** | Office Cox fit and post-estimation UDFs | 11.11 | Cox handle and table shapes → Survival-UDF parity | High |
| **11.15** | Office LMM and MMRM fit and contrast UDFs | 11.11 | LMM/MMRM inference and session handles → Correct mixed model UDF lifecycles | High |
| **11.16** | Office CA/MCA model-handle functions | 11.12 | CA/MCA matrices and handle cache → Output shape/numerical parity | High |
| **11.17** | Office clustering model-handle functions | 11.12 | Seed/labels/tree output → Deterministic results and cache | High |
| **11.18** | Office factor/discriminant model-handle UDFs | 11.12 | Rotation/classification handles → Model result parity | High |
| **11.19** | Office plot-source table Custom Functions | 11.13 | Plot-source output shape → All advertised plot functions mapped | Medium |

### Phase 12 — Office product deployment and version lifecycle

*Planned.* Production Office deployment with Windows MSI lifecycle kept separate.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **12.01** | Harden local HTTPS, dev sideload and host diagnostics | 04.07 | Sideload recipes and origin checks → Repeatable onboarding and host detection | Medium |
| **12.02** | Production XML manifest, commands and functions registrations | 11.03,12.01 | Schema checks; function/command metadata → No localhost assets/preview-only promises | High |
| **12.03** | Static deployment, cache busting and rollback | 12.02 | Asset hash, stale browser cache, offline error → Version-consistent managed/JS assets | High |
| **12.04** | Office centralized deployment / Marketplace / permissions; `Deployment docs; release candidate` | 12.03 | Permission/requirements check by host → Supported distribution path documented | High |
| **12.05** | Keep Windows installer/version/signature parity separate | 12.03 | Signed MSI / 32+64 packing / Core/Common load → Windows production unaffected | Medium |
| **12.06** | Mac and Windows Office.js production integration | 12.04 | Host capability feature matrix → Supported UI/functions consistently available | High |

### Phase 13 — Cross-platform verification and release hardening

*Planned.* Cross-host verification and release acceptance.

| Batch | Objective / delivered scope | Depends on | Evidence / completion criterion | Risk |
|---|---|---|---|---|
| **13.01** | Comprehensive Core/Common golden contract tests | 05.08-11.09 | All families, serialization, warnings, idempotency → No untested analysis handlers | Medium |
| **13.02** | Office.js adapter mocks/browser UI/e2e wiring suite | 04.07,12.02 | Selection/actions/options, rejected inputs → No unwired screen controls or silent errors | High |
| **13.03** | Cross-host numerical/output parity dashboard | 13.01,13.02 | Tolerances, tables, plots, metadata → Documented parity gate for every feature | High |
| **13.04** | Localization/accessibility and browser security audit | 13.02 | Slovak/English, keyboard and error injection → Usable and safe with no culture corruption | Medium |
| **13.05** | Performance, memory, cancellation and large data | 13.03 | Resource budget, repeated UDFs, long models → Documented constraints; no runaway memory | High |
| **13.06** | Mac/Windows/Web release acceptance, full documentation and Help parity, signed Windows packaging and Office manifest checks. | 12.05,12.06,13.03-13.05 | Full automated suite, release artifacts, Docs/Help links, method/version availability matrix and Office-specific steps/screenshots → All promised supported cells in matrix PASS; no stale GUI instructions or unsupported Office/Mac claims in released Help | High |

## 5. Deferred-defect charter and original restriction gates

**Owner decision recorded 5 November 2026; mandatory for 04A.39–04A.48.** A candidate is **not** a fix; a successful portable test is **not** a native or scientific release. The 04A.34/35 zero-OPEN member counts concern their *in-scope extraction ledgers* and **do not** discharge the original cross-project obligations.

**Workflow and mandatory exit:**
1. **04A.39:** one authoritative versioned defect register; exact ID, reproducer, source member, scientific/native risk, GUI/UDF/Core/Common owner, original-control/obligation crosswalk, evidence hash and proposed action. Deduplicate overlapping obligations without losing historical entries.
2. **04A.39–04A.41:** evidence-based investigation and decisions: input/provenance (39), regression/mixed/survival (40), basic stats/contingency/agreement/assumptions/sizing (41). Frozen before/after tests and independently validated references where appropriate; no blanket behavior-change authority.
3. **04A.42–04A.44:** propagate **authorized** semantics to UDF/cache/settings. Keep unapproved or native-dependent corrections dormant.
4. **04A.45–04A.47:** establish actual Excel HWND/PID/session, loaded XLL/assembly build attribution and independent COM-risk approval **before** native observations. Activate a gated correction only after its original evidence gate **or exact issue-specific, time-specific owner exception**. 04A.05/04A.27 exceptions do not transfer.
5. **04A.48:** every issue receives one disposition: `FIXED_AND_VERIFIED`, `RETAINED_RESTRICTED`, or `DEFERRED_OWNER_APPROVED`, with test evidence, remaining gates, responsible owner and re-review trigger. Software-fixed/native-unverified issues retain open obligations; owner acceptance does not automatically promote Gate C2/P/N1/N2.

**Restriction populations (overlapping; not additive):** 1,514 original obligations / 802 restricted live controls; 239 transferred obligations / 218 blocked controls; 160 ANOVA/normality obligations / 141 blocked controls; 44 importer obligations; 3 2×2 obligations; 7 UiGLM obligations / 34 source-associated controls; 35 GEE obligations / 34 restricted controls; 12 survival obligations / 10 originally restricted controls. **Zero releases/discharges** under current binding state. No unauthorized rewrite of source pins, old tests or `NOT_GOLDEN` comparisons.

### Active issue assignments (condensed index)

| Domain / immutable issue IDs | Not-yet-cleared condition | Planned investigation / native gate |
|---|---|---|
| Input/paired: `C14-INPUT-FINITE-01`, 04A.07 numeric-text nested coercion, 04A.04 paired offset, 04A.27 provenance | Finite-domain, row mapping, text precision and COM import behavior may diverge | 04A.39–41; native 45–48 |
| Normality/assumptions: Shapiro bound/constant, D’Agostino overflow, empty homogeneity | GUI/Core/UDF domain and empty-output discrepancies | 04A.41; UI 46 |
| Contingency: 04A.05 Liddell label and paired reference/interval | “Risk ratio” label vs matched-pair odds; inferential candidates | 04A.41; native 46–48 |
| Formula/model: `C08-CORE-LABEL-01`, `C08-FORMULA-REF-01` | Interactions/polynomial labels, categorical-reference precision | 04A.40; native if visible |
| Windows regression: `C09-NATIVE-STRICT-01`, `C10-LM-ALPHA-01`, `C10-COV-NAMES-01`, `C11-NATIVE-CORRECTIONS-01` | Strict-guard and LM alpha/covariance-header Windows patches unapplied; Core binomial (`C10-CORE-CONSTANT-01`) and cloned names fixed but native unverified | 04A.40, 45–47; exact owner/native approval |
| GEE: `C12-NATIVE-CLASSIFICATION-01`, `C12-GEE-START-QIC-01`, `C12-GEE-CLUSTER-TEXT-01`, `C12-GEE-PREDICTION-AXIS-01` | Fitted-family guard, supplied-start QIC, text-cluster acquisition, prediction row identities; legacy weights/AR(1) methodological limits | 04A.39–41; native 45–48 |
| Survival: `C13-LOGRANK-R-01`, 04A.13 KM/Cox, residual UI | Confirmed tied earliest-time censor/event risk-set defect; KM censored/all-event axis, Cox empty-stratum/failure handling; Cox residual GUI disabled | 04A.36 portable chart-data extraction accepted; scientific 40–41 and native 45–48 still restricted |
| Mixed models: 04A.14–16 candidates | Effective options/finite checks, covariance/row semantics, KR/LS-means native inference unverified | 04A.39–41; native 45–48 |
| CA/PCA: `C17-CA-NOLABEL-INDEX-01`, `C18-CA-NOLABEL-ACTIVATION-01`, `C18-PCA-THRESHOLD-CAPTION-01` | Unlabelled CA indexing correction dormant; PCA ≥ vs > descriptive threshold | 04A.40–41; native 45–48 |
| Clustering/FA/DA: `C19-CLUSTER-ROWID-DISPLAY-01`, `C20-DA-ROWID-DISPLAY-01`, `C20-FA-ROWID-AMBIGUOUS-01` | Physical vs local row identity; duplicate IDs; DA priors/folds and cluster numeric candidates | 04A.39–41; native 47 |
| SPC: `C21-STACKED-ROWCOUNT-VALIDATION-01`, `C24-HISTORICAL-ORACLE-SCOPE-01` | Logical-point vs raw row validation and native single/multivariate chart parity remain candidate/restricted | 04A.36 portable payload extraction accepted; scientific 40–41; native 45–48 still restricted |
| PSM/Love: `C25-REENTRANT-REQUEST-SNAPSHOT-01`, `C25-LOVE-THRESHOLD-MISSING-01`, `C25-LOVE-AXIS-NOFINITE-01`, `C25-RAW-TEXT-COVARIATE-IMPORT-01`, `C25-GUI-UDF-DEFAULT-INTENT-01` | Request races, threshold/axis, raw text and defaults; 04A.35 extracted chart contracts but did not correct them | 04A.39–41; native 45–48 |
| Agreement/sizing: `C26-ICC-NONFINITE-COUNT-01`, `C26-INDEPENDENT-PROP-DEFAULTS-01`, `C26-GUI-UDF-CONTRACT-01`, `C26-RESAMPLING-MINIMUM-01` | ICC infinity handling, defaults and resampling boundaries | 04A.39–41; UI/native 46–48 |
| Graphics/audit: 04A.29 lineage 171/172, 04A.30–37 renderer candidates | Source crosswalk uncertainties; ROC cutoff/re-entrancy, Q–Q degenerate, Box repeated state, histogram one-bin/error, violin indexing, XYZ margin/sentinel, specialty labels/axis; native chart fidelity NOT_RUN | Portable geometry 04A.36–37 accepted; defects 39–41 and native 45–48 remain pending |
| Output and source inspection | 04A.28 labels/header/row-axis candidates; historical Python source-pin and policy/census failures persist after authorized extractions | 04A.39–41/47; preserve original test outcomes and successor receipts |
| Windows forms/RefEdit: `Ui0OneRefeditMulticol.progressBarExactCalc` and related controls | Native option-state, events, workbook lifetime and original controls have no approved execution evidence | 04A.45–47, verify loaded build |

**Evidence retention:** Through all Phase 04A batches preserve the complete historical `TestResults` tree, source/evidence hashes, failed tests, original-control/native/scientific metadata and published manifests. No deletion, pruning, compression, rebaseline or archive migration without explicit owner approval. Free-space thresholds of **100 GiB / 50 GiB are advisory**, not deletion authorization.

## 6. Roadmap sequencing and cross-platform acceptance

**Critical chain:** Accepted 00–04 and **04A.01–37** → **04A.38 W2 full-body/fragment extraction → W3 comprehensive verification → independent review/owner acceptance and merge** → **04A.39–44 UDF/shared infrastructure + defect investigation** → **04A.45–48 native harness/defect exit** → **04A.49–70 + 04A.75 enhancements** → **04A.71–74 semantics/docs/WASM/exit** → Phase **05–09 Office analysis exposure** → **10 renderers/export**, **11 Custom Functions**, **12 deployment**, **13 cross-host hardening**. Phases 10/11 can conduct isolated early feasibility without treating unfulfilled upstream requirements as accepted capability. The future batching order is a dependency framework, not a hard prohibition on separately approved preparations.

**Do not duplicate migrations:** Phase 05–09 consumes the Core/Common statistical services already accepted in 04A and supplies Office handlers/editors/parity. Phase 10 uses existing portable plot payloads with Office-native/SVG/Canvas rendering. Phase 11 wraps those engines as Office Custom Functions. Core remains the authoritative statistical implementation.

**Feature complete requires:** documented method/options in `methods.yml`, validated Core/Common numerical contracts, typed input/output plans, truthful host capability and Help, tested real host wiring, CI/axis/chart fidelity where relevant, accessible localized UI, performance/size/resource limits, source/build attribution, end-to-end release matrix and separately approved scientific/native goldens as required. Host-specific unsupported features are explicitly disabled; Windows Excel-DNA availability does not imply Mac or Excel Online availability.

**Verification matrix:**
- **Core/Common/Windows tests:** Debug and Release compilation, compatibility/ABI/CIL as required, exact previous identity multiset/nonpasses, independent reference fixtures where appropriate, strict exit codes and source/preservation checks. **For current 04A.38 W2 checkpoints**, the explicit owner preference is one comprehensive Debug unit campaign near source freeze and **no routine full/selected Release unit suites** (`NOT_RUN_OWNER_PREFERENCE`); Release compilation/CIL/ABI/compatibility still apply. Other batch/release validation is governed by its own acceptance requirements. Do not misclassify inherited or newly qualified historical failures as green.
- **Office/JS/WASM:** managed/Node tests, actual browser read/write/Help/capability, guarded output and cancellation/lifecycle tests, accessibility, localization and cross-platform runtime. WasmSmoke is not production deployment.
- **Native Windows Excel:** only after individual authorization; verified HWND/PID/session and loaded XLL/assembly; COM/RefEdit/UIA smoke and chart/output comparisons. Source-level equivalence is not native validation.
- **Scientific:** independent R/other reference values for affected algorithms and edge cases, with provenance, tolerances, and explicit promotion authority. Same-Core or before/after compatibility is `NOT_GOLDEN`.
- **Release:** Windows MSI/signing and Office manifest/deployment independently; Excel Online, Windows desktop and **Mac desktop** target matrix. Mobile/iPad support is not presumed.

**Host deployment policy:** Office requires trusted HTTPS, tested manifest/requirement-set feature checks, static WASM assets, versioned cache/rollback, least permissions and client-specific acceptance. Windows Excel-DNA MSI/.xll 32/64-bit packaging and signature remain separate. No automatic production release follows from passing managed tests.

## 7. Separate post-Phase-04A evidence-retention review (not yet authorized)

Only **after all official 04A batches and formal exit/merge**, seek a fresh owner decision to perform an **unnumbered, read-only file-retention review** (not `04A.76`). Inventory `TestResults`, source pins, published manifests, `docs/architecture`, `evidence`, generated caches/build outputs and back-references by path, hash, size, provenance and dependency. Classify files as `RETAIN_ACTIVE`, `RETAIN_FROZEN`, `ARCHIVE_VERIFIED`, `REGENERABLE_CACHE_CANDIDATE`, `DELETE_CANDIDATE` or `UNKNOWN_BLOCKED`, with risk and possible savings. Produce dry-run dependency/restore/backups and a per-family owner approval matrix. **Physical archiving/deletion needs a second, separate authorization**; no cleanup is approved by this plan.

## 8. References, change history and precedence

**Project internal:** `planning/BESHStatNG_CrossPlatform_MasterPlan.md` (replace with this revision); `evidence/04A38/completion/w2/continuation-v12/owner-review.md`; final `evidence/04A38/completion/w2/continuation-v12-postseal-handover-addendum-v2.md`; v12 `remaining-work-queue-v1.json`, source/closure/fragment/test/compatibility ledgers, final Git/preservation receipts and `continuation-v12-review-parts-index-v1.json`; the archived 04A.36/04A.37 acceptance and correction evidence; `docs/architecture/batch-04A-35-owner-review.md`, `docs/architecture/batch-04A-34-owner-review.md`, `docs/architecture/WindowsMigrationLedger/`, `evidence/04A35/`, `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` and `BESHstatNG/Resources/RibbonXML.xml`. Prior plan revisions and original batch descriptions remain historical authority and must not be rewritten or discarded.

**External references (future feasibility, not implementation/golden authority):**
- Office add-in manifests: https://learn.microsoft.com/en-us/office/dev/add-ins/develop/add-in-manifests
- Excel Custom Functions: https://learn.microsoft.com/en-us/office/dev/add-ins/excel/custom-functions-overview
- Office shared runtimes: https://learn.microsoft.com/en-us/office/dev/add-ins/testing/runtimes
- Excel requirement sets: https://learn.microsoft.com/en-us/office/dev/add-ins/reference/requirement-sets/excel-api-requirement-sets
- R `survival` competing-risks reference: https://stat.ethz.ch/CRAN/web/packages/survivalVignettes/vignettes/tutorial.html
- R `cmprsk` source/reference: https://github.com/cran/cmprsk
- LibreOffice UNO range capabilities (out of scope): https://api.libreoffice.org/docs/idl/ref/interfacecom_1_1sun_1_1star_1_1sheet_1_1XCellRangeData.html

**Revision history (summary):** 2.8 (03 November: accepted through 04A.13 plus 04A.27); 2.9 (05 November: through 04A.22 and 04A.27); 2.10 (05 November: through 04A.25 plus 04A.27); 2.11 (05 November: through 04A.28, test-rationalization, and 04A.39–48 defect charter); 2.12 (06 November: through 04A.32); 2.13 (07 November: through owner-accepted 04A.35, 04A.36 prepared, full-plan condensation); **2.14 (12 November: 04A.36/37 owner-accepted, 04A.38 W2-v12 partial checkpoint, gates and v13 handover updated).** All original 226 batch IDs, dependencies and planned later scopes remain fixed; no unfinished batch is deemed accepted solely by this rewrite.

**Status precedence:** most recent explicit owner acceptance + verified live Git `main` > this living plan > immutable per-batch reports > older plan editions. A plan edit does not authorize a production change, a native test, a scientific correction, evidence deletion or a release.
