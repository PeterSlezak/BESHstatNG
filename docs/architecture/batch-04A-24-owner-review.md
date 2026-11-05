# Official 04A.24 owner review

## 1. Decision line

`READY_FOR_OWNER_04A24_ACCEPTANCE_REVIEW`

Software extraction only; this is a proposal for owner review, not owner acceptance or native/scientific release. All new tests pass and every full-suite nonpass is retained and attributed below. Native/scientific restrictions remain `P=NO; N1=NO GO; N2=NO; Gate C2=UNMET`; zero releases and discharges. C3 and E1–E7 are unchanged. No 04A.25 authorization is implied.

## 2. Real branch/main ancestry and HEAD

Branch `codex/04A-24` was created from clean, verified `main`/`origin/main`/HEAD `44b32955bb4f1b3aa43a9b65e42564c4fc9d436c`, parent `519e875a329111270685a0ae4be8666abb55ae04`, tree `ac52d230d316f7b04419bd409011883550c13123`. The actual main commit is “Move multivariate SPC covariance/limits/detection choices”; production contracts, real consumers, tests and the accepted 04A.23 dossier were present. The parent is the preceding reported base, not today's main. No fetch or ZIP-based inference was used. There were no owner edits at entry. Revision 2.9 and its stale progress narrative remain untouched; the owner's later acceptance governs progress. No staging, commit, merge, reset, stash, push or package installation was performed. Entry and final indexes are empty; main and HEAD remain the verified base. See `entry-state.json` and `final-git-state.json`.

## 3. Full production symbol/call graph

Before: `Ui22.btCompute_Click -> BuildRequest -> accepted 04A.23 policies -> one SpcMultivariate.Fit(request, AddressOf CancellationRequested) -> WriteResultsToNewWorkbook -> native private table/wrapper builders -> ExcelDnaResultWriter / shared SPC renderer`.

After: the same request/Fit/cancellation path calls `WriteResultsToNewWorkbook`, which creates one lazy `SpcMultivariateReportProjection` after the original workbook-creation boundary. Selected native sheet branches delegate to its Summary, Model, Chart Data, Signals, Diagnostics and Audit builders. `SpcMultivariateFamilyPresentation` supplies actual chart caption/grouping and Core request/model applicability. `SpcMultivariateWarningProjection` supplies the original ordered warning union. `BuildChartRenderingResult` reads native title strings and calls `SpcMultivariateChartProjectionPolicy.Prepare`; its returned Core wrapper delegates series preparation to accepted `SpcUnivariateChartProjectionPolicy.Project`. Windows passes those channels through the existing four-argument `SpcControlChartExcel.AddCharts` overload. Public renderer API, native marshalling and drawing remain unchanged.

Three new Common production files are `SpcMultivariateReportProjection.vb`, `SpcMultivariatePresentation.vb` and `SpcMultivariateChartProjectionPolicy.vb`. They use strict VB.NET/netstandard2.0, immutable Core results, read-only metadata and per-selected-section fresh ResultTables. Optional diagnostic arrays and selected model matrices are captured once rather than repeatedly copied per cell. Excel inputs, controls, coercion, output selection, cancellation, COM lifetime, writers and native geometry remain Windows-owned. Exact compiled membership and hashes are in `project-compilation-inventory.json` and final qualification evidence.

| Compiled file | Change and actual consumer |
| --- | --- |
| `BESHstatNG/src/UI/Ui22MultivariateControlCharts.vb` | Native writer constructs one lazy report projection; six private report builders delegate; chart builder captures strings and consumes the Common wrapper; existing four-argument renderer consumes its series. Final SHA-256 `6dc7279dc0ac30a095eccb2c3c74776372c421664f0639ac6af345bd5de063a3`. |
| `BESHStat.Excel.Common/src/Analysis/SpcMultivariateReportProjection.vb` | Fresh existing-schema Summary/Model/Chart Data/Signals/Diagnostics/Audit/Warnings tables, consumed by Ui22. |
| `BESHStat.Excel.Common/src/Analysis/SpcMultivariatePresentation.vb` | Family, warning union, seven section names, detached audit choices and internal once-captured diagnostic arrays, consumed by those builders. |
| `BESHStat.Excel.Common/src/Analysis/SpcMultivariateChartProjectionPolicy.vb` | Detached title intent, original renderer wrapper and delegation to accepted 04A.22 channels, consumed by Ui22 and the existing renderer overload. |
| `BESHStat.Excel.Common.Tests/src/Spc24Tests.vb`, `Spc24Fixture.vb`, `Spc24LegacyProjection.vb`, `Spc24ExpectedRejections.vb` | New runtime/culture/negative/typed-table/marker/source-provenance compatibility coverage and frozen-reference exception identities. Implicit Common test compilation and explicit Windows links. |
| `BESHStatNG.Test/src/Spc24AdapterTests.vb` | Compiled shipping call-site and actual host marshalling checks; no Form, Worksheet or Chart created. |
| `BESHStatNG.Test/BESHStatNG.Tests.vbproj` | Five Compile additions only. Final SHA-256 `440d097057f935c54c4534b01b70a95219ade1a3cf0d407b1e1ffb8e62244f97`. |

Common/Core remain netstandard2.0 and Windows host/tests remain Framework 4.8. Existing Common/host project source inclusion and all package/reference/target settings are preserved. Every new/modified candidate's exact bytes/SHA/mode/role is in the publication manifest. `source-preservation.json` records method-level before/after hashes; `prechange-overload-provenance-v1.json` additionally distinguishes both original NullableValue overloads. The Windows renderer, Ui21 and prior Common policies are unchanged.

## 4. Six-family before/after parity

All six existing choices retain Core-owned covariance/limits/signals: individual/grouped Hotelling T², grouped generalized variance, PCA T², PCA Q, MEWMA and MCUSUM (Crosier). Actual Core panel order is used; currently each branch returns one panel. Synthetic multi-panel tests characterize the adapter without enabling another GUI mode. Model reporting preserves variable order, returned eigenpair dimensions, effective rank, retained components, regularization, inverse/pseudoinverse flags, baseline counts and parameter sections. No covariance estimate, inverse, limit or recurrence is recomputed.

The shared 04A.22 projection retains all ten channels, typed sequence/label categories, source-row members, sparse point identity, finite/missing/marker masks, point/signal/exclusion label intents and stage boundaries. Native last-finite reference-label scanning/placement, zones, bounds, pixel geometry, legend, styles and number formats remain unchanged under 04A.34/36. Physical source rows, Core ordinal, zero-based series slot and one-based displayed point remain distinct. Diagnostics preserve optional vector/matrix shapes and the actual all/signalled scope predicate. Warnings are top-level first, then panels, first-seen Ordinal case-sensitive without trim.

Checkbox-conditioned sheets remain: Multivariate Summary; Model Details; Multivariate Chart; Chart Data; Signals; Diagnostics; Settings and Audit. Creation, first-sheet reuse/activation, AutoFit, writer pointers, chart-warning catch and partial outputs remain native in their original chronology. No output is forced.

## 5. Same-Core comparisons

The actual main Ui22 source and normalized method bodies were frozen before extraction. A host-free new legacy derivative retains original table and wrapper expressions, replacing only controls/acquisition with injected inert values. The new fixture compares complete typed ResultTable output models and wrappers against that derivative on the same returned Core result. Accepted 04A.22 pre-Common renderer observations independently compare categories, channels, markers, labels and stage fractions. Same-Core observations are `NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY`, with rejection and synthetic categories separated.

Profiles include 360 option/missing/phase/exclusion cases and 36 singular/historical/tied/two-variable cases in invariant and sk-SK cultures, plus seven published monitoring examples, sparse synthetic panels and a 3,000-point repeated projection. Expected negative exception identities come from the frozen 04A.23 corpus. Framework CRLF is reconstructed from XML-normalized LF solely for raw-message comparison. No scientific oracle or native appearance claim follows. The original 04A.23 3,196/1,462 and 04A.22 4,724/2,346 corpora remain unchanged.

Final corpus: **3,200 execution records / 1,600 content-addressed payloads**, comprising **1,724 positive comparisons, 1,472 preserved rejections and four synthetic API observations**. The negative records retain their frozen boundary classification: request/preparation versus Core Fit; neither is counted as positive statistical parity. Index `spc-multivariate-postfit-same-core-comparisons-v1.json` pins four bounded JSON buckets. SHA-256 hashes canonical UTF-8 JSON with sorted keys and comma/colon separators; exact CLR cell types, hexadecimal Double bits, all ordered row hashes and typed axes/channels are retained. Identical per-presentation panels share a hash; no orphan/colliding payloads or missing runtime/profile observations are accepted.

All assertions compare full typed table output models, including the same returned Fit timing, without tolerance. Only exported execution-time cells are marked volatile for reproducible payload hashes. Original raw XML/TRX remains local and hash-pinned. `worked-compatibility-examples-v1.json` references exact profile/dataset/model/table hashes and records these readable Debug/net10 public-data observations:

| Profile | Variant / points | First statistic | First UCL | Signal / excluded markers |
| --- | --- | ---: | ---: | ---: |
| published-0 | Hotelling individuals / 150 | 3.1496052282952647 | 14.606831276162012 | 7 / 5 |
| published-6 | Hotelling subgroups / 30 | 1.9967932775761479 | 12.41864274253283 | 4 / 1 |
| published-1 | Generalized variance / 30 | 0.49437599247567837 | 1.4277841741293917 | 1 / 1 |
| published-2 | PCA T² / 150 | 2.11832165423988 | 12.445221717322415 | 2 / 5 |
| published-3 | PCA Q / 150 | 0.372836021988628 | 3.297157801175971 | 6 / 5 |
| published-4 | MEWMA / 150 | 3.1496052282952656 | 14.156252500540894 | 17 / 5 |
| published-5 | MCUSUM / 150 | 1.274712717116566 | 5.5 | 37 / 5 |

These are decoded existing Core values, not new calculations or scientific reference values. Published individual examples use 100/50 Phase I/II points, excluding points 36–40 from estimation/rules; grouped examples use 20/10 subgroups, excluding subgroup 8. Both retain 95 baseline observations. Negative examples preserve missing/Infinity rejection, positive-component-count and MCUSUM-decision validation, no complete observations and MEWMA zero covariance rank (profile 220, Core Fit). Runtime-specific exception type/message and raw message hashes are retained. Signed-zero, sparse source rows, duplicate labels, sequence fallback, optional arrays, repeatability and caller mutation are covered by executed tests.

## 6. Edge-case/new test inventory and full-suite truth

Accepted 04A.23 full receipts are reused as entry context only after exact/line-ending-only compiled-source reconciliation; fresh live Debug Core/Spc22/23 checks passed before shipping edits. The live 18-case 04A.23 Python entry guard measured 17 passes and one already-triggered publication/branch-domain pin. A redundant complete live entry matrix was deliberately avoided. Historical complete entry figures below are qualified predecessor measurements, not newly executed 04A.24 entry totals.

| Per configuration | Accepted 04A.23 qualified reference | Final Debug | Final Release |
| --- | ---: | ---: | ---: |
| Logical managed pass / total | 9,444 / 9,451 | **10,267 / 10,280** | **10,267 / 10,280** |
| Raw managed pass / total | 14,524 / 14,532 | **16,163 / 16,177** | **16,163 / 16,177** |
| Node pass / total | 916 / 916 | **916 / 916** | **916 / 916** |
| Python method pass / total | 771 / 868 | **789 / 890** | **789 / 890** |
| Python failing / error methods | 87 / 10 | **91 / 10** | **91 / 10** |
| New 04A.24 logical managed | — | **829 / 829** | **829 / 829** |
| New 04A.24 Python | — | **22 / 22** | **22 / 22** |

Final raw managed containers are Core 1,058/1,058; Office 505/505; WASM execution 45/45; Common 7,153/7,158; Windows 7,341/7,350; ControlMitigation 61/61. Common contributes 816 new cases and Windows contributes those linked 816 plus 13 adapter cases: 829 distinct logical / 1,645 raw new executions per configuration. Full suites are **nonzero**, not green. Existing logical case identities and typed null/empty cohort weights match the accepted inventory; the canonical cohort keys avoid order-dependent display representatives. New full-case identities equal both focused discovery/execution sets.

The seven inherited logical managed nonpasses remain: three `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection` rows (20,15,40; 0,0,0; 20,30,50); `ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`; `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings(True)` (two raw frameworks); Windows `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`; and `Spc21AdapterTests.NativeHostCoercionStateFitOutputAndChartTailStayEntryBytes(WriteResultsToNewWorkbook)`. Six additional Windows `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer(0..5)` assertions require direct old Ui22 IL calls. Those calls now occur inside Common; new compiled-call and exact functional successors pass. No old expectation is rewritten.

Python has 101 distinct nonpassing methods; raw unittest output has 95 FAIL events and 10 ERROR events because subtests can repeat a method. All prior 97 method identities remain nonpassing with unchanged FAIL/ERROR classification. Four additional protected 04A.23 methods fail: `test_actual_publication_membership_hash_privacy_trackability_and_empty_index` (already measured at live entry); `test_existing_tables_and_warnings_remain_byte_exact_methods`; `test_native_workbook_order_warning_and_appearance_are_exact`; and `test_result_context_is_consumed_and_diagnostic_table_tail_is_exact`. The latter three pin extracted native bodies. Full identities, assertions, traces, source hashes and before/after classification are in `nonpasses.json`, per-configuration logical/raw identity files, Python traces and `entry-final-test-identities.json`.

Failed focused attempts are preserved: initial zero-match filter, new helper compile/setup errors, new oracle field shadowing, Framework XML LF/CRLF expectation encoding and synthetic Core model guards. Corrections affected only authorized new code/helpers. The final focused configurations pass. Accounting-only representative-key and repeated-TRX-hash investigations are disclosed in `accounting-transients.json`; they do not replace test results. Final compiled sources match both final build snapshots. Later edits are evidence/dossier/accounting only, followed by terminal source/publication checks.

## 7. Test-speed performance

`scripts/verify-04a24.py` is a batch-local offline derivative of the unchanged original verifier's applicable coverage. It builds sequentially once per final configuration, then runs four prebuilt lanes: portable managed; Windows/ControlMitigation; Office/WASM managed plus Node; and serialized Python/source checks. Every invocation gets unique copied binaries, sandbox-relative writers, observation/export paths, TRX and stdout/stderr. MaxCpuCount and MSTest Workers are one, Node file concurrency is one. No fifth worker, background service or concurrent compiler is used. Strict/profile/MkDocs and publication checks run serially. Root-command CPU/peak working set are measured where available; aggregate child-process RAM is not claimed.

| Lane | Debug pilot seconds | Release seconds |
| --- | ---: | ---: |
| A portable | 125.97 | 111.87 |
| B Windows/ControlMitigation | 194.89 | 153.85 |
| C Office/WASM/Node | 60.55 | 61.33 |
| D serialized Python/source | 451.38 | 436.04 |
| Whole four-lane run | **451.39** | **436.04** |

The two measured matrix wall clocks total **887.43 seconds**. Debug pilot Core lacked project-relative TestData (71 setup failures); an incorrectly enabled optional full-suite C02 writer caused four prefix-guard failures in Common. All launched lanes were collected. Read-only fixture copying/hash checks and the accepted earlier verifier's unset optional writer repaired the runner; full Core/Common reruns qualified Debug using the same binaries. Their test command time adds **94.69 seconds**, excluding container setup. Other Debug lanes are retained; Release uses the repaired runner. No product/compiled-test change was needed. Initial and corrected runner/preflight hashes and all failed receipts remain available.

Maximum concurrent command roots: **four**; no fifth worker or timeout. Associated testhost infrastructure is not a claim of only four total OS PIDs. Maximum observed root working set was **1,912,090,624 bytes**; child-process/system peaks were not measured. Lane-duration sums (832.79 / 763.09 seconds) are scheduling estimates only, not independent serial benchmarks or a claimed measured speedup. Historical whole-tree Python audits dominate the serialized lane. Exact build/command times, root CPU/RAM observations, source hashes and isolation records are in `parallel-validation-audit.json` and `command-outcomes.json`.

Actual commands used (placeholders replace local tool paths; invocation names/run roots must be new for any later rerun):

```powershell
& <PYTHON_EXE> -B scripts/verify-04a24.py build --configuration Debug --name final-build-Debug
& <PYTHON_EXE> -B scripts/verify-04a24.py build --configuration Release --name final-build-Release
& <PYTHON_EXE> -B scripts/verify-04a24.py matrix --configuration Debug --name final-Debug
& <PYTHON_EXE> -B scripts/verify-04a24.py portable --configuration Debug --name requalified-Debug
& <PYTHON_EXE> -B scripts/verify-04a24.py matrix --configuration Release --name final-Release
& <PYTHON_EXE> -B scripts/verify-04a24.py serial --name final-serial
```

The wrapper preserves the original portable/Legacy/Wasm/Js/Python commands' coverage using prebuilt `dotnet vstest` / VS2026 VSTest, offline builds and isolated results, rather than writing to historical `TestResults/Codex`. Host compile-only targets avoid signing/packaging. WASM `node --test --test-concurrency=1` includes its `.cjs` suites; Office Node and resolver run separately. Three Python discovery roots plus the 168-case Common discovery and fresh 15-case semantic probe execute. Test source is unchanged after final builds. Strict/profile actual exits are **1 / 1**: inherited stale UibyID/NormalPlot review dependency and `INTEGRITY_FAILURE` with empty error, matching the accepted 04A.23 observations. They remain nonpasses. Strict MkDocs exits **0** on final authored Help bytes.

## 8. Original controls and gates

Re-derived original Ui22 IDs: **182 controls / 320 linked OPEN obligations / 154 restricted controls**; Ui21: **208 / 226 / 115**. Broader original union: **2,056 controls / 1,514 obligations / 802 restricted controls**. These are overlapping authority populations, not permission to execute Excel or add unrelated counts. Original 04A.02 scientific/native 239-obligation/218-control restriction populations remain historical authorities, not additional disjoint totals. Exact IDs, row hashes and native-case overlaps are in `original-control-crosswalk.json`. Zero native releases and zero obligation discharges. `P=NO; N1=NO GO; N2=NO; C2=UNMET`; C3 and E1–E7 unchanged.

## 9. Source/history, manifest, documentation and candidates

Entry captured **251,415 historical result files** by membership, size, write/creation nanoseconds and filesystem identity, plus **1,666 frozen authority/evidence files** by SHA-256, including ignored historical corpora. Final retention passes with zero metadata/membership errors; this is not a claim of rehashing every historical result byte. The **100 mixed-model captures** additionally match their original registry hashes. Core, Ui21, renderer, earlier Common, old tests, Designers/UDFs/Ribbon, target/package settings, metadata and published data/screenshots/workbooks remain unchanged. Existing main/working-tree CRLF/LF observations are recorded separately; no ZIP was used to repair history.

Only three tracked files change: Ui22, the five-link Windows test project and three Help paragraphs (six added lines). New compiled/test/verification/evidence/dossier files are enumerated mechanically in `evidence/04A24/publication-manifest.json`. Every candidate has actual path/role/bytes/SHA/mode and Git trackability; ignored older 04A.22 data stays preserved and excluded, with no ignore-rule change. The manifest omits only its own hash; privacy omits itself/manifest from its external hash cache, while both receipts are always scanned. Terminal `git diff --check`, empty index, frozen preservation, manifest, privacy and trackability checks qualify the final set. Raw logs/TRX/metadata/build copies stay local under `TestResults/04A-24/run01/`. Public evidence uses stable placeholders and contains no private process trace/workbook capture.

`C24-HISTORICAL-ORACLE-SCOPE-01` records the accepted older test helper's shadowed diagnostic-scope field. The historical helper/corpus remains frozen; the new derivative explicitly uses `Me.SignalOnly` to reproduce the actual old UI predicate, and tests both scopes. No shipping behavior correction is activated. `C21-STACKED-ROWCOUNT-VALIDATION-01`, C09/C11/C12 native and the C13–C20 candidates retain their original owners/statuses.

The Help page clarifies variable/component axes, provenance/categories, masks/stage boundaries, warning order and Windows-only availability. Metadata and captured examples remain unchanged; strict MkDocs passes. Native Excel/COM/RefEdit/UIA, Office/Mac/browser manual runs, R oracle, screenshots, installers and golden promotion are **NOT_RUN / UNAUTHORIZED**. A later separately authorized smoke may use (1) published MEWMA, 100/50 phases, exclusions 36–40, 17 signal/five exclusion markers; and (2) grouped Hotelling, 20/10 subgroups, exclusion 8, four signal/one exclusion markers. Both should compare the two diagnostic scopes, source members, sequence/label categories, stage line, original limit-label/appearance choices and warning fallback. These are proposals from software fixtures, not observed Excel outcomes or permission to execute them.

## 10. Proposed owner decision

Accept **only 04A.24 portable multivariate SPC post-fit software extraction**, if the independent owner review agrees with this code and evidence. Retain all historical exceptions/nonpasses, native/scientific gates, original controls, C21 stacked-row validation, the C24 historical-oracle candidate and future 04A.34/36 and 04A.39–48 responsibilities. This proposal authorizes no native release, obligation discharge, Git publication or 04A.25. Those require separate explicit owner decisions.
