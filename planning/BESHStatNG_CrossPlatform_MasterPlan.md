# BESHStatNG — Cross-platform Excel application master development plan

**Revision:** 2.3 · 30 October 2026 — C30 owner approvals and approved retention of historical `TestResults` until the end of Phase 04A; official 04A.01 acceptance still pending.

**Source of truth:** the current live Git checkout and explicit owner decisions. The original merged 04.07 baseline was `BESHstatNG(20261025-161012).zip`; the active Phase 04A.01 audit has reached C30, evidenced by `BESHstatNG(20261030-101126).zip` and its sealed ledger. This planning revision records owner decisions, not implementation or final audit acceptance. The live checkout takes precedence over archived ZIPs and earlier reports.

**Implementation checkpoint:** **Phases 00–04 completed, owner-accepted and merged** (including 00.02A/B and 01.05A). **40 of 223 numbered batches accepted; 183 outstanding**. The last accepted batch is **04.07 — Office Ribbon integration and end-to-end wiring**. Acceptance incorporates the 04.04 production UX and post-hoc numeric p-values, the 04.05 worksheet-based specification builders, the 04.06 Options/Help/Diagnostics and Force English default, and the 04.07 Ribbon and task-pane navigation refinement.

**Gates:** **Gate B approved with E1–E7 exceptions** ([Common v1 freeze](../docs/architecture/gate-b-common-v1-contract-freeze.md)); **Gate C is OWNER APPROVED — GO WITH DOCUMENTED EXCEPTIONS** following the formal audit and owner-confirmed remaining native Excel checks ([decision](../docs/architecture/gate-c-owner-decision-20261025.md)). Any residual Windows-desktop/Mac compatibility limitations must be explicitly documented rather than claimed tested; Mac remains a later cross-platform release requirement. Phase **04A.01** has completed C30 inventory evidence but remains **unaccepted** pending the approved-profile closeout and separate owner sign-off; **04A.02–04A.75 have not started**. Gate C, Gate C2 and Gate C3 are milestones, not numbered batches.

**Total roadmap:** **223 unique batches = 148 original + 75 Phase 04A**. Completion counts are derived from the numbered roadmap: Phase 00 = 8; 01 = 11; 02 = 8; 03 = 6; 04 = 7 (**40 total**). Phases 04A and 05–13 account for **183 outstanding batches**.

**Last reported automated evidence:** 04.07 final amendment: **4,075 passed in each Debug and Release configuration**, 21 simulated Edge scenarios, Office/Help/WasmSmoke JavaScript regression suites and packed-XLL checks (see the [04.07 implementation report](../docs/architecture/batch-04-07-office-ribbon-e2e.md)). The owner accepted 04.07 in genuine Excel; this does not itself establish a complete Windows desktop or Mac qualification. Historic batch-by-batch numerical inventories belong in the linked implementation reports, not in the active plan.

**Scope:** Excel-DNA and Office.js/WASM are the active workstreams. LibreOffice/Calc is an architectural portability consideration only, with no implementation batches or Excel release commitment.

## Executive recommendation

Maintain the **two-host, one-engine** architecture: `BESHStat.Core` owns portable calculations; `BESHStat.Excel.Common` owns typed spreadsheet input, application orchestration, result/presentation semantics and portable contracts; `BESHStatNG` remains the .NET Framework 4.8 Excel-DNA host; `BESHStat.OfficeAddin` is the production Office.js/Blazor WASM host; `BESHStat.WasmSmoke` stays an independent regression probe.

The production Office foundation, input/output semantics, two working statistical editors, specification builders, global options, Help, diagnostics and Ribbon command entry points are now **implemented and accepted**. The formal Gate C audit is complete and the owner has approved **GO WITH DOCUMENTED EXCEPTIONS**. The next proposed implementation action is the **separately authorized 04A.01 acceptance-profile and test-output-isolation closeout**; formal 04A.01 acceptance and 04A.02 authorization remain distinct later decisions. The remaining Phase 04A migration and selected enhancements follow those decisions. Phases 05–11 then expose reusable computations and charts through Office rather than reimplementing them. Do not translate WinForms screen-by-screen or enable unavailable Office analyses prematurely.

`BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` remains the **canonical authored method/help/navigation catalog**. `BESHstatNG/Resources/RibbonXML.xml` remains authoritative for deployed Windows Ribbon declarations; compiled Common handlers/capabilities control execution; `<ExcelFunction>` declarations remain authoritative for Excel-DNA UDF exports. Generated metadata and reconciliation tests must prevent these surfaces from drifting.

## 0. Current implementation baseline and evidence

| Area | Accepted state | Continued obligations |
|---|---|---|
| Catalog, contracts and registry (00) | Stable IDs, typed handlers, generated method metadata, UDF documentation parity, host capability preflight | Keep `methods.yml`, Windows Ribbon XML and executable capabilities distinct and reconciled |
| Input/parsers and acquisition (01) | Typed cell/range/data-model, group/longitudinal/model/multivariate inputs, bounded host readers | Preserve source identity, source-row provenance, missing/error/date policies and quotas |
| Output document and safety (02) | Shared semantic output/style and placement plans; typed write outcomes; overwrite inspection and consent | Preserve no-silent-overwrite, stale-source rejection, readback and truthful partial-write state |
| Execution infrastructure (03) | Preferences, diagnostics, progress/cancellation, Help resolver, host lifecycle leases | Keep Gate B E1–E7, privacy and version-aware Help fallback behavior |
| Production Office host (04.01–04.03) | Production WASM host, typed Office bridge, method group/navigation and safe component lifecycle | Do not promote unsupported methods to executable actions |
| Real analysis/formatting (04.04) | ANOVA/KW, reusable grouped input, wide/long/whole-column selections, guarded outputs, numeric significant post-hoc p-values | Strict native readback, source leases, one-time consent and uncertainty latch |
| Specification builders (04.05) | Read-only formula, categorical, subject/visit and mixed roles; worksheet dropdown; Excel-DNA-style variable names | Do not fit/write unsupported models; preserve sparse physical-column identity |
| Production settings/UI (04.06) | Alpha, seed, p-value display, logging, Help, Options, diagnostics, localization; Force English ON by default | Persist safely; prevent rejected settings writes; apply settings to future runs correctly |
| Ribbon and navigation (04.07) | Supported commands, disabled unavailable methods/Export Chart, persistent About/Help header, Export Chart placeholder tab | Gate C owner-approved with documented exceptions; chart export still not implemented |

**Evidence rule:** an accepted batch means the owner accepted its stated scope, not that every future method, desktop host or platform is supported. The authoritative detailed acceptance history is in `docs/architecture/batch-*.md`, the [Common v1 freeze](../docs/architecture/gate-b-common-v1-contract-freeze.md), and test results retained by the repository. Historical reports need not be rewritten to remove their old pending wording.

## 1. Audit basis and planning boundaries

The initial October baseline was statically audited against Core, Common, Windows Excel-DNA, WasmSmoke, Office.js, the method catalog, WinForms, RibbonXML, graphics and Excel UDFs. Subsequent source/test evidence and owner acceptance supersede its incomplete-project findings. **Do not treat an old baseline inventory as the present implementation status.** The most recent 04.07 ZIP was reviewed separately; the owner's merged `main` is authoritative for any later changes.

The current portable boundary is **Core/Common `netstandard2.0`**, the Windows Excel-DNA host is **.NET Framework 4.8**, and current WASM/modern testing targets use **.NET 10** as applicable. Preserve separate XLL x86/x64 packing, Windows MSI and Office add-in deployment. No supported Mac desktop Excel acceptance or LibreOffice bridge should be inferred from browser tests alone.

## 2. Remaining portability and product gaps

These are **future responsibilities**, not unfinished work to repeat from accepted Phases 00–04:

| Area | Remaining work | Primary phases / gate |
|---|---|---|
| Windows-only statistical/GUI calculations | Inventory all backend, WinForms, RefEdit, graphics and UDF code; migrate remaining reusable calculation/orchestration to Core/Common without changing Excel-DNA behavior | 04A.01–04A.48 / Gate C2 |
| Selected Windows enhancements | Implement requested selected methods/plots and histogram PrettyBreaks/raw-bin option with portable numerics/specs and Windows integration | 04A.49–04A.75 / Gate C3 |
| Office method coverage | Register and expose additional validated analysis families using accepted Common input, output and UI builders | 05–09 |
| Office graphics and chart export | Implement native/SVG/browser renderers and real export; placeholder UI and disabled Ribbon export are **not** implemented export functionality | 10 |
| Office Custom Functions | Validate runtime/manifest, argument/result contracts, recalculation and performance; reuse Core math | 11 |
| Distribution and release | Production identity/deployment, host capability matrix, full Windows desktop/Mac acceptance, security/performance/accessibility | 12–13 |
| Future non-Excel host | Preserve neutral contracts only; no LibreOffice/Python bridge now | Out of scope |

**Non-negotiable rule:** new Office statistical screens call typed Common handlers and host adapters. They do not instantiate ad hoc Core models, duplicate output semantics, reimplement statistical formulas in JavaScript or silently bypass consent/lifecycle checks. Existing Windows code remains authoritative for parity until a migration batch proves equivalence.

## 2A. Authored catalog, Ribbon and UDF authority

| Surface | Source of authority | Required practice |
|---|---|---|
| Analysis name, category and documentation/help identity | `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` | Validate generated display metadata, stable slugs/IDs, docs and curated navigation |
| Windows Ribbon callbacks and buttons | `BESHstatNG/Resources/RibbonXML.xml` | Inventory deployed commands independently of YAML; reconcile rather than infer runtime availability |
| Executable Office methods/options | `BESHStat.Excel.Common` typed registrations and host capability checks | Metadata-only methods remain disabled; currently executable Office editors include ANOVA/KW |
| Excel-DNA worksheet functions | VB `<ExcelFunction>` declarations and source XML comments | Keep deterministic UDF manifest/docs generation and regression coverage |
| Help navigation/resolution | MkDocs, generated Help maps and version-aware resolver | Version-specific Help when published; validated latest fallback and MIME serving |

The historical inventory recorded **76 documented methods, 12 categories, 84 Windows Ribbon buttons and 268 UDF declarations**. Counts describe source inventory and must be regenerated/validated as the catalog changes. The XML's five utility commands are **not** additional executable analysis methods. `methods.yml` is not an executable handler registry and does not replace actual Ribbon XML.

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

**The earlier portability review identified five architectural seams; 01.05A implemented the targeted contract hardening, and their future-host constraints remain applicable:**

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

**Transport:** `SpreadsheetRangeData` represents a rectangular cell snapshot. The established range-reference and source-lease contracts capture workbook/worksheet identity, qualified addresses, physical coordinates, headers and cell provenance. Preserve those contracts when extending multi-source inputs and source-revision tracking; do not invent an incompatible second identity model. Never assume that an active workbook or worksheet stays unchanged during a calculation. An Excel 1900/1904 date-system flag is needed for date/time semantics when these are relevant; numerical date serials must not be confused with unitless numeric data.

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

**Implemented Office UX foundation (04.03–04.07):** one Blazor task pane provides catalog/group navigation, the operational ANOVA/KW editors, shared range capture and typed grouped options, guarded result preview/Write, worksheet-based read-only formula/category/subject/visit builders, and sticky navigation for **Analysis, Options, Export Chart (placeholder), Diagnostics/Log**. The persistent header exposes About (with Apache 2.0 license text) and version-aware Help. The actual Ribbon exposes only supported commands; unimplemented methods and Chart Export remain disabled.

**Reusable design for later features:** compose new workflows from host range/worksheet selection, typed role editors, method option builders, source-backed validation, output-plan previews, progress/warning panels and contextual Help. Common owns defaults, input policy, help metadata, application rules and typed result/format contracts. Blazor owns focus/keyboard navigation and transient form state. Do not duplicate the Windows WinForms screens; adapt their reusable validation and model semantics through Core/Common. Input acquisition/validation occurs at Run as needed, without mandatory preview steps. Capture source identity/revision, and retire stale previews and overwrite consent on source or option changes.

**Current localization:** English is forced **ON by default** even in Slovak Excel; switching Force English OFF follows available Office/browser language resources with English fallback. Locale-sensitive display cannot change numeric stored values or serialized probabilities. Some older controls remain English when translation is unavailable; do not claim comprehensive multilingual coverage. Keep source/workbook lifecycle, stale-selection, quota, cancel and write safeguards. Unsupported API-dependent actions must remain disabled rather than fail only after a click.

## 9. Excel UDF / Office Custom Functions strategy

Historical 14 October observation: **268 Excel-DNA functions** were declared but only **264** entered the UDF documentation parser. **Batch 00.02B has since corrected this:** the 16 October test suite asserts **268/268** and includes the four PSM examples. Keep this as a regression gate, not an open work item. A source-derived report can list every name/method; **do not assume** every Excel-DNA function has a direct JavaScript signature or that all should be public in v1. Preserve exact statistical logic by calling Core; never reimplement math in JavaScript.

1. First consolidate **UDF input and output behavior** in Common: numeric/categorical/matrix conversion, optional/omitted arguments, stable Excel error codes (`#VALUE!`, `#NUM!`, `#N/A` etc.), 1D/2D spill payloads, formula-reference vs copied values, missing handling, error propagation and output dimensions. Compare existing GUI and UDF conventions explicitly.
2. Add a `UdfCatalog` with name, version, signature, parameter schemas, return shape, help URL, volatility/caching hints and computational cost; **generate the baseline inventory from VB `<ExcelFunction>` source, not the obsolete hand-curated UDF CSV**. Link UDFs to known `methods.yml` keys through explicit, validated association fields, retaining VB XML comments as authoritative per-function docs. Generate Office custom-function metadata from the catalog where feasible, but do not force VB reflection into browser runtime.
3. **Office Custom Functions call a JS bridge that invokes a warm, reusable .NET WASM runtime.** First prove the bridge from real cells, then test startup cost, concurrent invocations, recalc behavior and memory. A separate pure-JS implementation of the same statistics is prohibited. A JS-only runtime cannot be assumed to host Blazor/WASM like a task pane; plan around a **shared browser runtime** or a validated worker/runtime bridge.
4. Pilot a small, stateless table/scalar set. Some array outputs are naturally dynamic-array spill functions; preserve their layout and test blocked spill cells. Favor reuse of the cached WASM instance over one runtime initialization per cell.
5. Explicit high-risk category: all **model-handle** functions (LM/GLM/GEE/Cox/LMM/MMRM/multivariate fit-and-query families, PSM), as shown by `ConcurrentDictionary` in the Windows UDFs. Session-scoped handles must have owner/workbook, expiration/invalidity, recalculation invalidation and memory limits; decide if each output should recompute deterministically or reference an in-memory model cache.
6. Classify candidates **provisionally**: low/medium = stateless descriptive/distribution/simple tests and fixed-size tabular outputs; medium = contingency, matrices, agreement, plot-data tables and rich arrays; high = formula-based regressions, model-handle families, large multivariate/PSM computations, dynamic result schemas or heavy recalc. Confirm risk on each function's actual code before its batch.
7. Long-running custom functions can be asynchronous/cancelable. In shared runtimes, intensive synchronous work may block the task-pane UI. Benchmark a worker or chunked orchestration if needed. A Custom Function must not mutate unrelated workbook cells as a side effect.

**Microsoft Office constraint to validate at implementation:** custom functions and task panes may be separate runtimes unless a shared runtime is configured; a shared runtime can coordinate state but incurs startup/memory/UI-blocking tradeoffs. Recheck manifest feature support when Phase 11 begins; initial planning favors a supported add-in-only XML manifest until any unified-manifest feature is fully available on the targeted Excel clients. See the authoritative Microsoft links in §18. These are future Custom Functions decisions, not unresolved 04.07 work.

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
| **02.07 — COMPLETE** | Combine diagnostics, charts, footnotes, links in one plan; `Common/Output/CompositeOutput.vb; host insertion adapters` | 02.05 | W=Small O=Yes | Block placement and cross-type bounding | Table plus chart placeholder | Complete multi-artifact result with no overlap | **High** |
| **02.08 — COMPLETE** | Implement safe overwrite and structured write outcomes; `Common/Output/WriteResult.vb; two host adapters` | 02.06 | W=Yes O=Yes | Cancel/partial failure/undo capability; diagnostics | Cancel overwrite of occupied range | No unexpected destructive write | **High** |

### Phase 03 — Common infrastructure, lifecycle and execution

**Phase exit:** Defaults, fault handling, progress, cancellation and workbook/run identity consistent across hosts.

**Phase 03 exit — accepted:** Windows ANOVA/KW UI and output remain intact; deleting a selected worksheet fails safely through the RefEdit lifecycle guard. Synthetic execution fixtures were removed and the real progress/cancel UI retained. Cancellation remains cooperative; synchronous Core/COM or active Office.js writes cannot be arbitrarily interrupted. See the 03.06 report for the historical fix and evidence.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **03.01 — COMPLETE** | Create versioned app settings DTOs and preference store contracts; `Common/Infrastructure/Settings.vb; Core/AppSettings.vb` | 00.04 | W=Small O=Small | Defaults, migration, bounds, alpha parity | Change alpha and reopen add-in | Persisted and effective settings visible | **Medium** |
| **03.02 — COMPLETE** | Host-specific logging, diagnostics and run correlation; `Common/Infrastructure/Diagnostics.vb; host log sinks` | 03.01 | W=Small O=Yes | Redaction; run ID; noncrashing logger | Trigger expected validation error | No raw worksheet data logged automatically | **Medium** |
| **03.03 — COMPLETE** | Execution state, progress and cooperative cancel; owner-confirmed Windows WinForms/UI-thread hotfix and Excel Online acceptance; `Common/Infrastructure/ExecutionCoordinator.vb; IProgressReporter` | 00.04 | W=Small O=Yes | State transitions; cancel-before-run; progress | Owner accepted Windows and Online | Truthful cancellation support; historical evidence in 03.03 report | **High** |
| **03.04 — COMPLETE** | Stable safe presentation mapping; `Common/Results/Faults.vb; Windows/Office pilot adapters`; three validation-code corrections included; visible Windows demo checkbox/row and normal delay removed, hidden fixture temporarily retained with mandatory Phase 03 exit removal above | 03.02,03.03 | W=Small O=Yes | Stable codes, privacy, validation/warnings, cancellation/write precedence; no fabricated convergence signal | Owner confirmed Windows and Office smoke, restored normal ANOVA/KW appearance with real Cancel/status | Owner accepted and merged; actual writer state retained; historical automated evidence in 03.04 report | **Medium** |
| **03.05 — COMPLETE** | Common help/version/availability linked to validated `methods.yml`; shared version-first resolver and latest-only MkDocs `.htaccess`; [report](../docs/architecture/batch-03-05-help-version-feature-metadata.md) | 00.02B,03.01 | W=Small O=Yes | Exact version per-page verification; same-page latest fallback; metadata parity; both MkDocs build paths and custom output MIME artifact | Owner confirmed published Help routing in Windows and Excel Online | Owner accepted and merged; preserve published resolver, activation and MIME build hook | **Low** |
| **03.06 — COMPLETE** | Operation-owned document/selection leases; scoped Windows/Office lifecycle checks and cleanup; pinned source/destination identity; Windows fixture removal; final deleted-sheet RefEdit fix in main `db85ff8`; [report](../docs/architecture/batch-03-06-document-lifecycle.md) | 01.01,03.03 | W=Yes O=Yes | Close/switch/rename/delete/replacement, unsupported events, consent/disposal races, immutable source addresses, no Windows fixture; full Debug/Release gates | Owner confirms Phase 03 smoke and **Restored ANOVA/KW appearance and actual output**, including `user.document-changed` on deleted-sheet RefEdit | Phase 03 complete; owner acceptance reconciled with local main; no fresh Codex live-host claim | **High** |

### Phase 04 — Production Office.js host and reusable task-pane UX

**Phase exit — ACCEPTED:** Production OfficeAddin runs the registered ANOVA/KW methods with typed host services; read-only model builders, settings, diagnostics and Office Ribbon navigation are established. This is not blanket support for all catalog methods or all host platforms.

| Batch | Objective and likely files/classes | Depends on | Host changes | Required new automated tests | Manual smoke | Acceptance criterion | Risk |
|---|---|---|---|---|---|---|---|
| **04.01 — COMPLETE** | New production BESHStat.OfficeAddin foundation; scoped readiness DI, read-only Core/Common probes, small JS initializer, separate localhost dev manifest; [report](../docs/architecture/batch-04-01-office-host-foundation.md) | 00.03,00.06 | W=No O=Yes | WASM build/assets; DI and host-free startup tests; Node readiness tests | Owner-confirmed Excel Online/standalone, probes, Retry and independent WasmSmoke | Core+Common load in real Excel owner-accepted; Gate C later owner-approved with exceptions | **Medium** |
| **04.02 — COMPLETE** | Typed production `Services/IWorkbookAdapter`, bounded tiled JS transport, fail-closed write seam and explicit read-only diagnostic; [report](../docs/architecture/batch-04-02-office-workbook-adapter.md) | 04.01,01.09 | W=No O=Yes | Typed C# transport/lifecycle tests and mocked range/sheet/write JS tests | Owner accepted genuine selected-range reading and diagnostics | No direct Office.js use in Blazor; owner-accepted on main `6b1e978` | **Medium** |
| **04.03 — COMPLETE** | Two-level group/method dropdowns from generated `methods.yml` metadata, verified descriptor/editor availability and reusable options UI; `OfficeAddin/Components/AnalysisShell; OptionEditors`; [report](../docs/architecture/batch-04-03-method-navigation-options-ui.md) | 04.01,00.03,00.02B | W=No O=Yes | Real select render/change events, 12 groups / 76 documented methods, typed editor availability, state/lifecycle; existing dependencies | Owner accepted both dropdowns and genuine Excel options/Retry/reopen smoke | Accepted/merged on main `30d77ac`; historical pending wording superseded | **Medium** |
| **04.04 — COMPLETE** | Reusable grouped input/constraints and output presentation, bounded whole columns, physical-row long intersection, sticky tabs, direct Run and one-click guarded Write; [report](../docs/architecture/batch-04-04-range-preview-destination-ui.md) | 04.02,01.03,02.06 | W=No O=Yes | Golden ANOVA/Welch/post-hoc and KW/Dunn values/styles/preferences; original/effective footprints, overhang warnings, source/consent/readback/uncertainty and independent-next-run guards | Owner accepted genuine Excel Online numeric post-hoc p-values, write/overwrite and option-change rerun | Accepted on main `659d4fb`; Common v1/E1–E7 unchanged; New Workbook remains unsupported by the Office writer; carry its feasibility assessment into the Gate C exception/limitation log | **Medium** |
| **04.05 — COMPLETE** | Reusable formula, subject/visit and categorical builders; `OfficeAddin/Components/ModelBuilder and RoleEditors`; [report](../docs/architecture/batch-04-05-model-role-builders.md) | 04.04,01.06,01.07 | W=No O=Yes | Common/Core design equivalence, worksheet/active-sheet discovery, Excel-DNA captions, sparse physical provenance, typed roles/categories and accessible lifecycle controls; numeric post-hoc fixtures remain green | Owner-accepted genuine Excel worksheet/variable and specification preview smoke | Read-only preview, no unsupported model fitting/write; passed full gates and owner acceptance | **High** |
| **04.06 — COMPLETE** | Production Options/Global Settings, versioned Help, diagnostics, progress, English/Slovak resources and **Force English ON by default**; reused shared preference/store contracts; [report](../docs/architecture/batch-04-06-production-options-help-diagnostics.md) | 04.03,03.01–03.05 | W=No O=Yes | Preference persistence and failure/retirement ordering, migration/defaults, localization override, consent retirement and security | Owner accepted Slovak Excel: English default, OFF → Slovak, ON → English, persistence/reopen and Run/Write parity | Shared settings effective without duplicate policy; failed retirement never persists rejected changes; Help/diagnostics and consent remain safe | **Medium** |
| **04.07 — COMPLETE** | Actual Office Ribbon commands and capability reconciliation; shared pane routing; compact persistent About/Help header, Apache 2.0 About dialog, four tabs (Analysis, Options, Export Chart placeholder, Diagnostics); [report](../docs/architecture/batch-04-07-office-ribbon-e2e.md) | 04.01–04.06 | W=No O=Yes | Generator/manifest and runtime-command inventory; dispatch and accessibility; options, Help, lifecycle, E2E regression | Owner accepted genuine Excel activation/UI and guard behavior; Windows desktop/Mac platform evidence remains distinct | Real ANOVA/KW editors and utilities reachable; unavailable methods and actual Chart Export disabled; no duplicate command panel or bypass of write safeguards | **High** |

**Phase 04 status — ACCEPTED / MERGED:** All seven production Office batches are complete. The detailed 04.04 UX/recovery and numbered-new-sheet amendments were incorporated into the accepted implementation; their design evidence is preserved in the linked [04.04 report](../docs/architecture/batch-04-04-range-preview-destination-ui.md), not repeated as pending acceptance requirements. Office **New Workbook** and actual **Chart Export** remain unavailable until separately demonstrated; see Gate C limitations and Phases 10–13.

### Phase 04A — Finish Windows code migration and add selected Excel-DNA enhancements (planned)

**Companion source ledger:** [Windows portability audit, 18 October 2026](BESHStatNG_Phase04A_Windows_Portability_Audit_20261018.md).

**Placement:** immediately after **04.07**, before **05.01**. This deliberately changes the execution critical path while preserving all existing Phase 05–13 batch IDs. The phase has **75 small batches**: **04A.01–04A.48** for comprehensive Windows migration inventory, incremental extraction and parity gate; **04A.49–04A.70** for the previously selected PDF enhancements; **04A.75** for optional histogram PrettyBreaks versus raw bin counts across histogram entry points; **04A.71–04A.74** for chart semantics, documentation, portable compatibility and final exit validation. IDs 04A.01–04A.74 are preserved. **04A.01 has completed C30 inventory evidence and received owner approval for an inventory-only acceptance amendment on 30 October 2026; official acceptance is pending implementation and verification of the revised profile. The other 74 Phase 04A batches remain unstarted.**

**Scope and ownership:** As each planned 04A enhancement is implemented, the existing Excel-DNA product receives its host UI/renderer additions, but the numerical tests/geometry belong in `BESHStat.Core`, analysis inputs/options/result documents/chart specifications in `BESHStat.Excel.Common`, and WinForms controls/COM chart placement only in `BESHstatNG`. A future Office.js host should invoke the same compiled services. This does **not** prematurely implement Office.js screens or LibreOffice integration, and it does **not** port WinForms visuals verbatim. Phase 04 has already established the production Office shell; Phase 04A features can be smoke-tested in Windows without prematurely adding Office-specific method screens.

**No false code-migration target:** A *complete portable-code extraction* is not deletion of every Windows file. Excel-DNA Ribbon registration, COM `Range` access, WinForms `.Designer.vb`, clipboard/file dialogs, Excel chart objects and MSI signing are legitimate Windows adapters and remain there. Conversely, any algorithm, model specification, statistical report rule, input validation, chart geometry or reusable settings policy found inside a form must be considered for Core/Common. `Option Strict On` and host-dependency guards apply to new portable code. Existing Core APIs must be reused when present, not copied.

**Source-derived audit priorities:** The 18 October ZIP has **32 non-designer UI source files (28,296 lines), 29 `.Designer.vb` files, 38 Windows Graphics `.vb` files and 41 `.vb` UDF sources**; the UI file scan counts **1,232 method declarations** (automated textual estimate, not independently verified functionality). The nested `UI/Refedit/Excel2007RefEdit.vb` also contains selection and worksheet lifecycle logic that belongs in the audit, although the control itself remains Windows-only. The largest behavioral forms include `Ui22MultivariateControlCharts.vb`, `Ui21ControlCharts.vb`, `Ui18MMRM.vb`, `UibyID.vb`, `UiGLM.vb`, `Ui11PCA.vb`, `Ui19LMM.vb`. Portable calculations are interspersed with COM input, local `ResultTable` composition and chart creation. `RegressionEffectsController.vb` uses WinForms `ListBox` despite reusable term-specification logic. `Graphics/NormalPlot.vb` calculates plotting positions and normal-score reference lines within an Excel-dependent class, and its constructor sorts the caller's input array in-place. `Ui82x2.vb` and `Ui0OneRefeditMulticol.vb` already expose OR, RR and all four requested ordinal association measures, making most of those additions **plot/report enhancement work**, not new statistical algorithms. `Assumptions.Grubbs` exists in Core; Friedman already computes internal `rankSums` and public `MeanRanks`, while `MCP` performs Conover/Dunn rather than Nemenyi. Searches found no named Dunnett/Hsu/Nemenyi implementation in Core/Common/Windows source; `Kolmogorov` appears only in descriptive text. **Cochran's Q and Cochran–Armitage are different tests from Cochran's variance-outlier C test**.

**Statistical validity gates (do not skip):** Dunnett requires properly calibrated *simultaneous* comparisons to a specified control, not multiple standalone t-tests. Hsu's MCB requires validated best-group selection, direction and simultaneous bounds rather than ordinary Tukey comparisons. Nemenyi critical difference must be correct for the chosen rank/blocked-design assumptions and multiplicity family; existing Dunn/Conover p-values cannot simply be plotted as a Nemenyi CD. A one-sample KS test against a **fully specified** CDF and a normality test against a **sample-fitted** normal CDF require different p-value calibration; parameter estimation must not silently use the fully specified-null KS p-value. Standard one-sample KS does **not** have a general degrees-of-freedom parameter; display sample size and explicitly documented reference parameters, and show df only for another well-defined statistic. For Grubbs, enforce assumptions (approximately normal, single extreme observation per test) and data-to-plot row identity. In 2x2 and RxC plots preserve existing CI method, orientation, sign and null reference (OR/RR=1, ordinal association=0). Numerical reference fixtures must be reviewed before enabling UI checkboxes.

**Selected PDF requests, and no more:** items 1, 2, 4, 5, 6, 7, 9, 13, 14, 15, 25, 38, 39 from `Content.pdf`. Other items in that 40-item PDF (pie charts, Sidak, REGWQ, BH, extra 2x2 tests, etc.) are *not authorized in this phase*. Page 34 also suggests CI/effect-size enhancements to paired t-test, but the user's selected request is specifically the **difference box plot**; those other changes remain outside this phase unless separately approved.

**Gate before 04A.49:** 04A.48 must establish no missing portable-code migration candidates and stable Windows numeric/output fixtures. New statistical methods should then be built from Core outward; Excel-only graph implementations must consume the Common chart payload rather than redoing the statistic.

**04A.01 C30 owner-policy update (30 October 2026; Master Plan 2.2):** The source/member ownership inventory is complete (3,866/3,866; UI 1,547/1,547; 268 Excel UDF exports). On 30 October 2026 the owner **approved the concept and criteria** of a distinct `inventory-complete-with-approved-deferrals` profile, without accepting 892 incomplete UI control contracts or 622 still-unqualified wiring edges as fully functional. The detailed ledger contains 1,514 individually owned deferred obligations, with risk 385 P0 / 416 P1 / 78 P2 / 12 P3 / 1 UNKNOWN_HIGH, and 802 blocked affected live-control golden cases. Original strict-full-contract verification remains unchanged (C30 exit 2). **The new acceptance profile is not implemented/effective and 04A.01 has not yet received official final acceptance**. Gate C2 remains UNMET, E1–E7 OPEN and GC3–GC8 DEFERRED. The original 04A.01 row is retained as historical/full-contract scope; its inventory-only acceptance criterion is superseded **only after** the separately validated amended profile takes effect and an explicit owner acceptance record exists.

**Owner-approved exceptional dispositions:** (1) Cox `Ui4Cox.ckResidualPlots` is an *unimplemented option*; removal from the Windows form is approved, but shall be performed and tested in a separately authorized product change (default owner 04A.47), not in audit-only closeout. Its historical P0 record remains until the change passes regression. (2) RefEdit `Excel2007RefEdit.xlApp` remains `UNKNOWN_HIGH`, assigned to 04A.27 and related 04A.45–47, with affected native/GUI operations blocked pending subscription, replacement, teardown and workbook-identity tests. (3) `Ui0OneRefeditMulticol.progressBarExactCalc` remains P0, assigned to 04A.46 pending consumer/progress/cancel/close tests. None is granted an untested functional qualification.

**C30-PRES-001 acknowledged by owner:** Eleven historical managed-test JSON files were byte-identically rewritten (SHA-256 unchanged; timestamps altered). The owner accepts **this disclosed protocol exception**, not a claim that the no-overwrite protocol passed. Before the next full validation, isolate both test-internal writers in `OfficeOutputPlanAdapterTests.vb` and `CompositeOutputTests.vb` and verify that historical bytes *and metadata* remain unchanged. The owner separately decided on 30 October 2026 to **retain all historical `TestResults` until the end of Phase 04A**; no deletion, relocation, compression, pruning or protection-manifest rebaseline is authorized by the acceptance closeout.

**Sequencing and scope:** First perform a narrowly authorized C30 acceptance-policy/test-output-isolation closeout, passing both the new independent inventory checker and unchanged original strict checker; then record an explicit official 04A.01 owner acceptance. **04A.02 is a separate owner decision**, not authorized by the amendment. A possible *first 04A.02 subscope* is `C30-SYN-ANOVA` and `C30-SYN-KW` typed synthetic Core/Common cases with frozen RowIds, independent references and tolerances. No live Excel/COM/RefEdit/WinForms/chart output, unmitigated P0/UNKNOWN_HIGH capture or new numerical goldens are authorized by this planning note. The broad existing 04A.02 Windows numerical/table/chart/form-state acceptance obligations (including ANOVA, normality, 2x2 and PCA) remain; any reduced final scope requires an explicit case-level roadmap amendment, not an inferred pass. Deferred checks belong to their earliest actual 04A.03–47 owner; 04A.48 is final extraction/parity review, not a catch-all destination. Historical `TestResults` must be retained **through Phase 04A**, and any later cleanup requires separately approved lossless archiving, restore verification and a protection-manifest rebaseline.

**Owner-approved historical-evidence retention decision — 30 October 2026:** The owner elected to leave the approximately 100 GB accumulated `TestResults` evidence **in place for the remainder of Phase 04A**, including all 04A.01 continuations and subsequent migration/enhancement batches. This supersedes the earlier suggestion to archive/delete C01–C20 or otherwise prune the folders during 04A.01. The owner reports approximately **300 GB of free disk space** at this decision point; that is a capacity observation, **not a guarantee** that future test-output growth will fit. Retain historical paths, hashes, metadata and C30-PRES-001 evidence; isolate future test writers to new distinct output directories without changing prior files. Document disk usage/free space at major batch checkpoints. If free space becomes unsafe, **stop and request an explicit new owner decision** rather than silently deleting or rebaselining protected history. A practical early warning is ~100 GB free, with a review/stop recommendation at ~50 GB; these are recommendations, not already implemented automated gates.

**Separate end-of-Phase-04A archival/cleanup work package (proposed, NOT started):** After the Phase 04A migration/enhancement acceptance evidence is sealed (including 04A.74 and 04A.75, and the associated Gate C3 owner review), propose a distinct archival and cleanup job. **`04A.76` is only a candidate future batch identifier; it is NOT added to the approved 75-batch Phase 04A roadmap or to the 223-batch denominator by this decision.** The owner can then choose to approve a new numbered batch or a separate post-phase maintenance operation. Its eventual acceptance contract should include: complete evidence/path/hash/metadata inventory; content-addressed or similarly verifiable lossless archive stored outside the working results tree; verified sample and representative full restores; confirmation historical validation and C30-PRES-001 records are preserved; tested audit/protection-manifest relocation/rebaseline policy; explicit authorization of the particular paths to prune; and a recorded post-cleanup integrity and free-space check. No archiving/cleanup code or file deletion is authorized now, nor is this package an implicit prerequisite for 04A.02–04A.75.

**Authoritative C30 decision record to apply:** `docs/architecture/WindowsMigrationLedger/04A01-owner-approvals-20261030.md` (new, not present in the C30 ZIP; retain unsigned `continuation30-owner-decisions.md` as historical evidence). The source C30 dossier, owners, exact control/edge IDs, risk flags and source hashes remain authoritative; this note changes no underlying statistical code or completed audit decision objects.

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
| **04A.75** | Histogram raw-bin mode and optional PrettyBreaks: `HistogramBinningCore.HistogramBinsComputation` gains a backward-compatible on/off argument (legacy default **On**); expose checkbox **Use PrettyBreaks** (checked by default) for ordinary and categorical histogram modes in `UI/UibyID.vb` and any other UI backed by this binning engine; carry the option through `CategoricalHistogramOptions`, Windows `Graphics/Histogram.vb`, `ChartingFunc.vb`, and histogram UDFs without breaking existing calls. Preserve selected Sturges/Doane/Scott/Freedman–Diaconis **raw target bin counts** when Off, rather than re-snap bin edges/count through `PrettyBreaks`. Put computation in Core, options/preview contracts in Common where needed, and WinForms/COM wiring only in Windows. | 04A.30,04A.34,04A.48 | W=Yes O=Contract tests only | Independent theoretical raw-bin counts and midpoints for all four rules; pretty-on golden parity; endpoint/frequency sum, ties/constant/zero-IQR/zero-variance and small-N handling; categorical pooled-bin alignment, density/count preservation, UDF default compatibility, UI checkbox wiring | Compare both checkbox states for ordinary and categorical histogram, plus chart/table/bin UDF paths across data where pretty mode changes the number of bins | Off mode uses the unsnapped method-derived count (except documented degenerate safeguards), accurate frequencies and chart labels; On mode matches existing outputs; all affected histogram UIs expose the option, no histogram duplication in Office.js | **High** |
| **04A.71** | Cross-feature plot semantics and accessibility: shared CI/critical-line/alpha metadata, chart/table warnings and palettes | 04A.49–04A.70,04A.75,02.07 | W=Yes O=No (contract tests) | Chart-spec schema/snapshot and neutral legend/axis markers across new charts | Run sample comparison/KS/rank charts and export | No statistical data or precision encoded only by styling | **Medium** |
| **04A.72** | Update `methods.yml`, generated method metadata, Ribbon callback/help wiring, Windows screenshots and MkDocs for only selected PDF additions | 04A.49–04A.71,00.02B | W=Yes O=No (contract tests) | Manifest validators, help topic links and versioned docs checks | Open help from modified forms; inspect new options | Public methods discoverable without duplicate source of truth | **Medium** |
| **04A.73** | Cross-host-ready contract suite for new methods: serialize typed results/specs and execute under .NET 10 WASM-compatible Core/Common (no new Office feature screens) | 04A.71–04A.72,04.07 | W=No O=Contract test only | WASM smoke numerical contracts, golden fixtures, no COM references | Prototype request/response of one new chart-result DTO | New feature logic reusable by Office.js host without writing it in JavaScript | **High** |
| **04A.74** | Phase 04A acceptance audit: Windows regression, Core/Common + legacy + JS tests, graphics snapshots, source ledger closure, Phase 05/10/11 no-duplicate map | 04A.48,04A.73,04A.75 | W=Yes O=Contract smoke | All code migrations parity and every feature result+chart validation | Exercise all 13 PDF-selected enhancements plus histogram PrettyBreaks On/Off in Excel-DNA | Portable code/GUI adaptation complete; existing Excel-DNA behavior preserved; Phase 05 ready | **High** |

**Wave transition and avoiding duplicate work:** Existing Phase 05–09 migration batches retain their identifiers but become *Office feature-exposure, handler-registration, product-UI and host-parity waves* for any Core/Common service already extracted by 04A; they must **not re-extract algorithms or reimplement the 04A statistical enhancements**. Phase 10 consumes the portable chart payloads from 04A for Office-native/SVG/browser renderers rather than remaking chart statistics. Phase 11 exposes the existing Core/Common UDF logic through Office Custom Functions rather than reimplementing numerical algorithms. A future batch must use the 04A ledger to mark each overlap `reuse / host wiring / unresolved` before changing sources.

**Rollback and package protection:** Keep Windows Excel-DNA .NET Framework 4.8, .NET Standard 2.0 Core/Common, net10.0 tests/WASM, existing MSI signing, and generated `methods.yml` consumers stable. Batch changes should be small enough for patch/apply/build/full tests/1–2 Windows smoke runs. A failed statistical-reference check blocks the new feature but does not disable existing Windows methods.

### Phase 05 — Low-to-medium complexity statistical workflows

**Additional prerequisite:** Complete **04A.74** first. For methods already extracted and regression-tested during 04A, these batches are **Office task-pane exposure/feature wiring/parity**, not a second migration of calculations. Existing IDs, batch intent, and other listed prerequisites remain; treat the 04A gate as an additional implicit dependency of every 05.xx–11.xx implementation batch.

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

**Active critical path (Gate C approved):** `04A.01–04A.48 Windows extraction/audit → Gate C2 → 04A.49–04A.75 enhancements and portable plot specs → Gate C3 (04A.74 acceptance, including dependency 04A.75) → Phases 05–11 Office analysis/chart/UDF coverage → Phases 12–13 product/release gates`. Every Phase 05–11 batch inherits the 04A parity/reuse gate even if its original dependency column predates Phase 04A.

```text
Phases 00–03: Common registry, inputs, outputs and infrastructure       [ACCEPTED]
Phase 04: Production Office host, usable analyses, UI, settings, Ribbon  [ACCEPTED]
    └─ Gate C: production-host exit decision                              [APPROVED WITH EXCEPTIONS]
        └─ Phase 04A.01–04A.48: full Windows portability ledger           [PLANNED]
            └─ Gate C2: extraction closure                                [PLANNED]
                └─ Phase 04A.49–04A.75: enhancements + histogram controls [PLANNED]
                    └─ Gate C3: 04A.74 final parity sign-off              [PLANNED]
                        └─ Phases 05–09: Office statistical workflows
                            ├─ Phase 10: graphics + real chart export
                            └─ Phase 11: Office custom functions
                                └─ Phases 12–13: deployment, Mac/Win/Web release matrix
```

**Parallelization:** Prepare renderer/Custom Functions feasibility in disposable branches when their interfaces are stable; do not mark the corresponding numbered batch complete or merge production code before its declared prerequisites. Keep `BESHStat.WasmSmoke` as a small independent regression probe. Windows Excel-DNA must pass smoke/parity checks after each future Windows-touching 04A patch. There is **no remaining authorization to implement 01/02/03/04 foundational batches**; they are complete.

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
- **Gate B / after 02.08 — APPROVED WITH E1–E7 EXCEPTIONS, merged:** owner accepted the [Common v1 freeze and documented exceptions](../docs/architecture/gate-b-common-v1-contract-freeze.md), permitting compatible additive versioning only. All 03.xx and 04.xx batches have since been accepted and merged. Gate B is not counted as a batch.
- **Gate C / after 04.07 — OWNER APPROVED, GO WITH DOCUMENTED EXCEPTIONS (25 October 2026):** the owner accepted the prior source-bound genuine Excel observations and confirmed the four remaining native checks (Help, keyboard/focus, repeated/duplicate validation, busy/Cancel/Retry) as passed. E1–E7 stay open/carried; GC3–GC8 are deferred with the documented limitations. The [formal audit](../docs/architecture/gate-c-office-application-layer-exit.md) retains its historical NO_GO and incomplete automated evidence classifications; the [owner decision](../docs/architecture/gate-c-owner-decision-20261025.md) is the current authority. **04A.01 is authorized but not yet implemented.** Office New Workbook, actual Chart Export, additional Office methods, Windows desktop/XLL and Mac qualification are not implied. Keep WasmSmoke independent; desktop/Mac release qualification stays with Gate G.
- **Gate C2 / after 04A.48:** Windows-wide portable-code audit and extraction closure with source-to-owner ledger, including WinForms, nested `UI/Refedit/Excel2007RefEdit.vb`, graphics and UDF wrappers.
- **Gate C3 / after 04A.74:** 13 selected PDF enhancement topics plus optional raw/PrettyBreaks histogram bins numerically validated in Core/Common, visible in Excel-DNA, renderer-independent chart payloads verified and Phase 05 reuse map signed off.
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

## 17. Current handoff — approved Gate C to Phase 04A.01

**Owner-accepted baseline:** **00.01–04.07 complete, accepted and merged to `main`**; `40/223` accepted, `183/223` outstanding. This includes the 04.04 native-readback and p-value fixes, 04.05 worksheet-based model-role builders, 04.06 Force English/local settings and 04.07 Ribbon/header/Export Chart placeholder. Historical reports may retain their then-current pending wording, but they do **not** supersede this owner-confirmed checkpoint.

**Gate C decision: APPROVED — GO WITH DOCUMENTED EXCEPTIONS (25 October 2026).** The owner explicitly approved the exit and acknowledged E1–E7 carry-forward and GC3–GC8 deferrals. The [formal Gate C audit](../docs/architecture/gate-c-office-application-layer-exit.md) retains earlier NO_GO/unsigned history and exact simulated/native evidence boundaries; the [owner decision record](../docs/architecture/gate-c-owner-decision-20261025.md) documents the later owner confirmation. The four outstanding native cases were confirmed by the owner, **not independently rerun or source-bound as four new automated checks**. Windows desktop and Mac remain release-gate work.

**Next authorized implementation:** **04A.01 — Comprehensive Windows Code Inventory and Portability Audit**, on a fresh branch from the current merged repository. Inventory every Windows code path, including WinForms designer and event wiring, statistics and graphics, UDFs, RefEdit, help, settings, update and Ribbon; produce the architecture migration ledger with Core/Common/host-only ownership. Do not start or claim 04A.01 until the actual source has been inspected. **All 75 Phase 04A batches remain unimplemented at this decision checkpoint.** The companion source inventory is [BESHStatNG Phase 04A Windows portability audit](BESHStatNG_Phase04A_Windows_Portability_Audit_20261018.md).

**No false release claims:** Office chart export and new-workbook writing remain unavailable; only ANOVA/KW currently execute as Office analyses. Owner-approved Gate C is **not** a cross-platform release certification and does not close the registered deferred exceptions. Keep acceptance at **40/223** until an individual future numbered batch is implemented and accepted.

**Historical status detail:** See the [Common v1 freeze](../docs/architecture/gate-b-common-v1-contract-freeze.md) and `docs/architecture/batch-*.md` for earlier decisions, test counts and corrective amendments. Those reports are maintained as historical evidence; superseded chronological notes have been removed from the active roadmap.

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

## 20. Revision history and status precedence

- **1.4 · 18 October 2026:** audited the then-current portable/shared code and introduced the Windows portability and selected-feature Phase 04A workstream (04A.01–04A.74). Prior batch summaries and test inventories are retained in architecture reports.
- **1.5 · 21 October 2026:** added **04A.75**, the backward-compatible histogram raw-bin/PrettyBreaks option, yielding **223 numbered batches**. The 13 selected enhancements and all pre-existing roadmap batch IDs were retained.
- **1.9 · 25 October 2026:** tracked acceptance through 04.06 and the implementation of 04.07; historical 04.04 corrective text remained in the planning document after the batch was accepted.
- **2.0 · 25 October 2026:** reconciled owner-accepted/merged **04.07**, advanced counts to **40/223**, removed obsolete pending 04.04/04.05/04.06 narratives and historical duplicated progress notes, corrected Gate B and critical-path language, and identified **Gate C formal exit review** as the next action. The 223-row implementation roadmap, 75 Phase 04A batches, accepted Core/Common boundary and E1–E7 exceptions are unchanged.

- **2.1 · 25 October 2026 (historical revision):** recorded the explicit Gate C owner decision **GO WITH DOCUMENTED EXCEPTIONS**, preserved the historical NO_GO audit, authorized 04A.01 (not started), and carried E1–E7 / GC3–GC8 with desktop/Mac release qualifications deferred. The numbered ledger remains **40 accepted / 183 remaining / 223 total**.
- **2.2 · 30 October 2026 (owner-policy rebaseline, historical):** Recorded owner approval of 04A.01 inventory-only acceptance criteria; dispositioned Cox `ckResidualPlots` as unimplemented/removal approved, deferred `xlApp` and `progressBarExactCalc` to named Phase 04A owners with risks intact, and accepted C30-PRES-001 as a disclosed protocol deviation. Implementation and official 04A.01 acceptance remain pending; 04A.02 remains separately unauthorized. Existing 75 Phase 04A IDs, Gate C2/C3 and E1–E7/GC3–GC8 obligations are unchanged.
- **2.3 · 30 October 2026 (owner-approved TestResults retention):** Retain all existing `TestResults` throughout Phase 04A, monitor disk capacity and defer any archival/pruning until after Phase 04A acceptance. A separate, owner-approved post-phase cleanup package is envisaged (provisional identifier `04A.76`, **not** added to the numbered roadmap). C30-PRES-001 still requires test-writer isolation, and preservation checks remain fully active. Official 04A.01 acceptance and 04A.02 authorization are unchanged/pending. The official roadmap remains **40 accepted / 183 remaining / 223 total**.

**Status precedence:** Current `main` + explicit owner acceptance > latest master-plan checkpoint > per-batch implementation reports (historical) > archived ZIP or earlier revisions. Historical reports can retain original acceptance wording for traceability; the *active roadmap* must not present it as a current blocker.
