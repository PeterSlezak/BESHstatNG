# Batch 04A.22 owner-review dossier

## 1. Decision — software extraction only

**READY_FOR_OWNER_04A22_ACCEPTANCE_REVIEW** — software extraction only. Both complete matrices, final source qualification, preservation, publication/privacy/trackability and all 14 closing policy contracts are qualified. This is a proposal for owner acceptance of the software batch, not native visual equivalence or independent scientific accuracy. Overall historical suites remain nonzero as recorded below.

`P = NO; N1 = NO GO; N2 = NO; Gate C2 = UNMET`. **Zero native releases and zero deferred-obligation discharges.** No Excel/COM/RefEdit/UIA/chart execution, authenticated Office/Mac/browser run, R oracle, numerical-golden promotion, installer/signing/deployment, Git publication or 04A.23 work is authorized or performed.

## 2. Verified entry and Git

The actual clean local `main`, existing `origin/main` and accepted `codex/04A-21` pointed to **9330f8df18c0c73d2b233ec4b06dc7199cbb244a**. Its parent is **0f1980e5e223cf4770ce2bebe2d5828daa7ac81e**, tree **545d92922125c6ffe1712846c785b374959eff9d**. [Entry state](../../evidence/04A22/entry-state.json) records the live observation. The shipping four Common policies, real Ui21 consumers, tests, dossier and evidence are present in that commit. History is linear; the owner's explicit acceptance and live source establish provenance without inventing a separate merge commit. The earlier ZIP/pre-merge SHA is historical context. No fetch occurred.

The initial tracked worktree/index was clean and there was one worktree. `codex/04A-22` was created from verified main. Git required a per-command ownership exception, without global configuration changes; branch creation alone needed sandbox approval because `.git/index.lock` was protected. No owner edit was overwritten, and no reset/clean/stash/rebase/stage/commit/merge/push occurred.

Authorities read: repository AGENTS; Master Plan Revision 2.8 literal 04A.21–24, 04A.34/36, 04A.39–42 and 04A.45–48 boundaries; original 04A.01/02 restriction/control/gate records; accepted 04A.17–21 dossiers/manifests/decisions and frozen comparisons. The stale introductory progress summary and all official IDs remain unchanged.

The entry freeze pins **1,024** source/project/tool files, **865** authority/evidence files and **235,231** historical result files by membership/metadata, plus **139** nested historical test-result files. Whole-file SHA-256 values preserve exact live bytes; method bodies use normalized LF only for method comparison. Private raw baselines and preimages remain under the fresh `TestResults/04A-22-20261105-run01/` directory. Public historical-manifest observations distinguish accepted successors and preexisting CRLF/LF differences from this batch's changes.

## 3. Implementation inventory and call graph

| Path | Shipping purpose |
| --- | --- |
| `BESHStat.Excel.Common/src/Analysis/SpcUnivariateChartProjectionPolicy.vb` | New strict portable policy and immutable axis, panel/channel, label and stage-boundary contracts. Reuses Core's chart/family and actual panel order, immutable points and occurrence records. |
| `BESHstatNG/src/UI/Ui21ControlCharts.vb` | Only `WriteResultsToNewWorkbook` changes: captures appearance, projects the actual result and passes that projection inside the existing chart-warning catch. |
| `BESHstatNG/src/Graphics/SpcControlChartExcel.vb` | Existing public three-argument `AddCharts` remains callable. A Friend overload accepts Ui21's projection. Real series consume Common categories/channels/labels; stage lines consume Common fractions/text. |
| `BESHStatNG.Test/BESHStatNG.Tests.vbproj` | Three compile inclusions: shared Spc22 tests/legacy characterization and Windows adapter tests. No framework/package change. |
| `BESHStat.Excel.Common.Tests/src/Spc22Tests.vb`, `Spc22LegacyRenderer.vb` | New same-result legacy-versus-Common matrix, identities, immutable masks, edges and frozen host-free source derivative. |
| `BESHStatNG.Test/src/Spc22AdapterTests.vb` | Executes actual pure shipping bridge/bounds/zone methods without constructing Excel objects. |
| `scripts/tests/test_04a22_spc.py` | New shipping-consumer, preservation, authority, corpus, schema and publication contracts. |
| `BESHStatNG_Help_MkDocs/docs/methods/control-charts.md` | Clarifies existing panel gaps, typed sequence fallback, markers and stage boundaries. |

New batch capture/verification/accounting/publication tools and `evidence/04A22/` complete the review package. The publication manifest lists every actual candidate path, role, size, mode and hash. Project compilation inventory records entry and added compilation membership. Common remains `netstandard2.0`, strict, host-free and free of statistical recurrences/rule evaluators. Windows host/tests remain .NET Framework 4.8. Existing SDK inclusion compiles the new Common file.

Public Common contracts are `SpcChartChannel`, `SpcChartAxisPoint`, `SpcStageBoundaryIntent`, `SpcChartLabelIntent`, `SpcPanelSeriesProjection`, `SpcUnivariateChartProjection` and `SpcUnivariateChartProjectionPolicy.Project`. Core immutable models remain the provenance/signal authority. Exact source and method hashes are in the entry inventories, source-preservation report, final compilation qualification and publication manifest.

Before:

```text
Ui21 accepted 04A.21 preparation -> one SpcEngine.Fit(request, cancellation)
  -> WriteResultsToNewWorkbook -> existing SpcResultTables/writer
  -> SpcControlChartExcel.AddCharts
     -> Windows BuildSharedAxisContext / BuildPanelChartData
     -> Windows label eligibility/content and adjacent-stage predicate/fraction
     -> native Excel drawing, appearance, bounds, zones, cleanup
```

After:

```text
Ui21 accepted 04A.21 preparation -> one unchanged SpcEngine.Fit(request, cancellation)
  -> WriteResultsToNewWorkbook -> existing SpcResultTables/writer
  -> existing chart Try: appearance + Common.Project(actual result, sequence intent)
  -> SpcControlChartExcel.AddCharts(sheet, result, appearance, same projection)
     -> MarshalChartCategories / MarshalChartValues -> actual XValues / Values
     -> projected point/signal/exclusion labels -> existing SetPointLabel(index + 1)
     -> projected stage boundaries -> existing Refresh/PlotArea/Shapes.AddLine
     -> retained native appearance, bounds, zones, legend, cleanup and rollback
```

The public three-argument renderer delegates to the same path, validating appearance before creating a projection when none was supplied. The internal overload rejects only mismatched payload/result/axis API misuse. Ui21 creates no projection when charts are unselected. Workbook creation, first-sheet reuse, five checkbox chronology, sheet names, first-output activation and table materialization stay in their original positions. Chart projection/creation failures still write the original warning table with `ex.Message`; other selected outputs continue.

Ui22 already calls that public renderer after wrapping its panels in `SpcFitResult`, retaining its original multivariate chart type. This existing shared consumer is preserved by generic point/panel pass-through rather than a new family rejection. A constructed-model compatibility test compares those panel snapshots with the legacy renderer; it runs no multivariate Fit and claims no multivariate/native verification. Ui22, its conversion helper, covariance choices, scientific calculations and availability remain untouched; their extraction remains 04A.23–24.

No separate output-plan registry was added: it would add no necessary portable decision and could disturb table/exception chronology. Core `SpcResultTables` remains the sole table authority. All request preparation, validation/cancellation and the single Fit handler remain exact entry method bytes.

## 4. Chart-family and presentation parity

| Existing Windows choice | Authoritative returned panels, in order |
| --- | --- |
| Individuals | IndividualValue |
| Moving Range | MovingRange |
| I–MR | IndividualValue, MovingRange |
| X-bar | SubgroupMean |
| R | SubgroupRange |
| S | SubgroupStandardDeviation |
| X-bar–R | SubgroupMean, SubgroupRange |
| X-bar–S | SubgroupMean, SubgroupStandardDeviation |
| p | Proportion |
| np | NumberNonconforming |
| c | DefectCount |
| u | DefectRate |
| CUSUM | UpperCusum, LowerCusum |
| EWMA | Ewma |
| Moving Average | MovingAverage |

This table is an observation/test expectation, not a second production calculator registry. Production consumes `result.ChartType`, `result.ChartFamily` and `result.Panels` directly. No additional Core-only enum is enabled in Ui21; Ui22 is untouched.

Common transfers these exact pure decisions:

- Sparse sorted union of Core indices; first encountered panel representative wins. No dense zero-fill, label/date sorting or physical-row inference.
- Numeric `Double` category only when sequence use is selected and the representative has a sequence; otherwise the original `String` label. Duplicates and nonmonotone sequences remain categories.
- Ten aligned nullable finite channels: statistic, CL, LCL, UCL, lower/upper 1σ, lower/upper 2σ, signal, excluded. Markers contain the finite statistic only when the corresponding Core flag is set. Native boxing preserves exact Double bits; absent/nonfinite samples become `Nothing`.
- Original Core points retain index, displayed index+1, all source rows, stage, phase, finite status, estimation/rule inclusion, explicit exclusion scope/reason and rule enumeration. Absent panel slots remain distinguishable from present NaN points.
- Eligible finite point labels, signal `R...` text and exclusion reason/`Excluded` content, using zero-based aligned series slots. Core normalizes constructed rule-number arrays; the projection formats the enumeration it receives without another sort/deduplication.
- Adjacent representative stage changes under `OrdinalIgnoreCase`: ordinal i, denominator n, exact Double i/n and `Stage boundary: previous to next`. Phase changes alone do not add boundaries.

Immutable collections reuse immutable Core points/panels/occurrences and do not copy the original measurement matrix. Building the union sorts indices; panel alignment uses dictionaries and linear passes. A 5,000-point/three-panel sparse stress fixture exercises the mapping without quadratic lookup scans. Occurrence windows/contributors/marked arrays stay separate from plotted point flags.

Native work deliberately retained for 04A.34/36 includes stacked-area zone layers/offset/widths, all-seven finite monotone eligibility, secondary axes/legend removal, reference-last-finite labels, specification/target eligibility, natural zero/proportion clipping, 7% padding, 10% flat expansion and [0,1] fallback. Bounds/zone eligibility are compared through the actual pure renderer methods against frozen entry methods with identical bridged values. Those methods and all appearance/title/axis/styling/COM/error helpers are exact entry source. Pixel coordinates, Single conversion, delayed PlotArea handling and Excel lifetime remain native. This is not live visual verification.

## 5. Worked compatibility examples and non-golden evidence

[The corpus](../../evidence/04A22/spc-postfit-same-core-comparisons-v1.json) stores content-addressed XML result/presentation payloads inside standard JSON, raw TRX aliases/hashes, configurations, runtimes and test identities. Each XML retains typed categories, original point/source/stage/phase identities, channel samples as exact IEEE-754 bit strings or `BLANK`, marker labels and separate signal occurrence metadata. Request/settings/table hashes link the result to its fixture. No source statistical value is rounded, normalized or recalculated by Common.

All records are **NOT_GOLDEN / SAME_CORE_PRESENTATION_COMPATIBILITY_ONLY**. Both paths receive the same authoritative Core result. Core/request rejections are recorded separately and are **not** presentation successes. Failed setup/initial test attempts are retained separately from final qualified comparisons.

Table hashes reuse the unchanged accepted helper's typed volatile marker for only `Execution time (ms)`, `Execution started (UTC)` and `Execution completed (UTC)`. No statistical cell, table ordering/type/format or plotted value is normalized for comparison.

The public 60-sample CSV is unchanged, SHA-256 **7ea168ca8e7812a2bf9b925d5261413f20a057f79a087414b4032c08b2492a2b**. Accepted `Spc21Fixture` decimal literals supply five-wide subgroup measurements, individual measurements, counts, sample sizes and exposures. np ordinary fixtures use constant 100; varying sizes are a separate rejected fixture. Original data labels, Unicode, source rows and settings vary explicitly in the matrix.

Worked examples cover X-bar–R's mean/range vectors and first/last aligned categories; EWMA dynamic/steady limits, short sequences and lambda edges; CUSUM signed upper/lower channels and intrinsic markers; p variable sample-size limits; and np constant-size count channels versus unchanged varying-size rejection. Supplemental fixtures preserve A/B/A/B groups with physical rows 101/105/118/131 as logical points 0/1 with source arrays [101,118]/[105,131]. Sparse synthetic panels test later-only indices and first-panel representative priority. Matched marker/contributor and same-point exclusion/signal fixtures prevent occurrence-to-marker conflation.

[Worked payload references](../../evidence/04A22/spc-worked-compatibility-examples-v1.json) give exact profile hashes, request options, first-four vectors, complete marker membership and links to the original full bit vectors. For invariant net10 profiles 6/13/12/8/9: X-bar–R begins at mean 49.881/range 0.5450000000000017; EWMA begins 49.964/49.9872; CUSUM begins upper 0 and lower −0.5335662943176147 (later negative zero is retained); p begins 0.02/0.010869565217391304; np begins 2/1. Every example retains exclusion series slot 17 (displayed point 18). The selected EWMA profile has 25 signal markers; CUSUM has 40 upper/zero lower. Selected Shewhart profiles use preset None and therefore show no rule markers. These are fixture-specific Core returns, not general recommendations or new statistical references.

Final corpus: **4,724 records / 2,346 content-addressed payloads** across Debug/Release and net10.0/net48. There are **2,432 ordinary matched presentation records**, **68 matched synthetic presentation records**, and **2,224 explicit Core/request rejection records**. Rejections are never added to the 2,500 presentation successes. Only actual Fit cases establish the 15-family panel matrix; constructed generic model shapes are excluded from that matrix.

Detailed family/axis/marker/stage/reference evidence files link to the corpus and exact fixture/source hashes. Moving-average span 1 and infinite input reject during Core construction before Fit; longer spans, missing windows and all-missing reference masks preserve actual Core behavior. These observations do not independently validate numerical SPC accuracy.

## 6. Outstanding issues and retained restrictions

`C21-STACKED-ROWCOUNT-VALIDATION-01` remains **OPEN / CANDIDATE_ONLY / UNAPPLIED**. Projection never reinterprets stage/exclusion definitions using raw stacked row counts. Existing Windows point checks remain.

All previous C09/C11/C12 native patches, C13 survival risk-set defect, C14 diagnostic candidate, C17 unlabelled-CA indexing, C19 row-ID/Minkowski/height-cut and C20 duplicate provenance/DA prior/fold candidates remain unchanged and restricted under their accepted records. No new scientific correction or chart redesign is activated. No C22 behavior correction is proposed by the matched presentation comparisons; test-author assumptions about Core constructor timing/rule normalization were corrected in new tests only.

Full native Excel rendering, legend/label/PlotArea visual regression, COM/UIA/RefEdit, authenticated Office/Mac/browser execution and independent R validation are **NOT_RUN / UNAUTHORIZED**. All native release and deferred scientific/native obligations remain open. Office/Mac SPC GUI remains unavailable.

## 7. Test truth and reproducible commands

Entry Debug and Release each measured **7,971/7,977 logical managed**, **11,599/11,606 raw managed**, **916/916 Node**, and **745/836 Python**. These are actual merged-main observations, not forced historical pre-merge totals. Six logical/seven raw managed failures are inherited; entry Python has one additional frozen branch/base assertion relative to the prior pre-merge reference.

| Qualification | Debug | Release |
| --- | --- | --- |
| Final logical managed | **8,609/8,616**, 7 nonpasses | **8,609/8,616**, 7 nonpasses |
| Final raw managed framework executions | **12,860/12,868**, 8 failures | **12,860/12,868**, 8 failures |
| New 04A.22 logical managed | **639/639** | **639/639** |
| New Common / Windows raw cases | **623/623; 639/639** | **623/623; 639/639** |
| Node, three existing suites | **916/916** | **916/916** |
| Python distinct methods | **755/850**, 84 FAIL + 11 ERROR | **755/850**, 84 FAIL + 11 ERROR |
| New Python contracts | **14/14** | **14/14** |
| Full verifier / comparable Common Python exits | **1 / 1** | **1 / 1** |

Managed suite raw totals are Core 1,058/1,058; Common 5,508/5,513; Windows 5,683/5,686; OfficeAddin 505/505; OfficeExecution 45/45; ControlMitigation 61/61 in each configuration. Logical deduplication uses shared source SHA/class/method/data-row identity and retains established typed null/empty cohort weights. Repeated focused/semantic runs are excluded from the totals. Python subtests attach to their actual method: raw failure events are 88 FAIL + 11 ERROR, distinct nonpassing methods 84 + 11. Raw events are not substituted for the 850-method denominator.

The six inherited logical managed failures remain: three `PairedDisjointCountContractAndPureSemanticProjection` rows, `ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`, `ConstantAndSeparatedBinomialPreserveActualFitWarnings(True)` and `PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`. The constant-binomial case repeats in both frameworks. The only new managed nonpass is the frozen `Spc21AdapterTests.NativeHostCoercionStateFitOutputAndChartTailStayEntryBytes(WriteResultsToNewWorkbook)` hash: expected `0e623c8c…`, actual `2b648024…` for the authorized three-line replacement.

Four additional frozen Python method identities fail on the authorized source extraction:

- `test_04a19_clustering.Clustering19Contracts.test_core_native_writer_and_prior_test_bytes_unchanged`
- `test_04a20_factor_discriminant.FactorDiscriminant20Contracts.test_core_native_prior_tests_targets_and_metadata_are_entry_bytes`
- `test_04a21_spc.Spc21Contracts.test_core_ui22_native_designers_prior_tests_and_targets_unchanged`
- `test_04a21_spc.Spc21Contracts.test_single_fit_cancellation_and_original_native_output_tail`

All four pin the historical renderer/output source; no Core/test expectation is changed to satisfy them. New consumer, exact workbook-tail equivalence, retained-method and same-result bridge tests are their functional successors. [Nonpasses](../../evidence/04A22/nonpasses.json), sanitized Python traces and full logical/raw identity inventories retain every failing method, assertion/exception and entry/final difference. No new 04A.22 test fails. Overall suites are **not green**.

Qualified focused runs 09/10 pass 622 Common/638 Windows cases before the final generic shared-consumer model check was added; both complete final matrices qualify that additional case. Initial build/helper generation/filter failures, the new test's incorrect rule-order expectation, earlier-than-Fit Core rejections and Python discovery syntax error remain in [transient validation](../../evidence/04A22/transient-validation.json). Zero-test filters do not count as success. Protected tests/expectations are untouched. Exact local identities remain in TRX; synthetic URL/drive-path display literals are publicly replaced by stable SHA-256 aliases.

The accepted verification workflow is reused through a new 04A.22 wrapper. The repository verifier is not edited. Its execution copy redirects all result writers into fresh batch paths, adds `--no-restore`, isolates assembly/test-data copies, continues independent suites after nonzero outcomes and avoids Windows generators/packaging/signing targets. Exact original/execution script hashes and transformations remain in local provenance receipts; sanitized commands and all exits are published in `command-outcomes.json`.

```powershell
& <PYTHON_EXE> -B scripts/verify-04a22.py init --evidence TestResults/04A-22-20261105-run01
& <PYTHON_EXE> -B scripts/capture-04a22-contract.py
& <PYTHON_EXE> -B scripts/verify-04a22.py final --evidence TestResults/04A-22-20261105-run01 --configuration Debug --name entry-Debug
& <PYTHON_EXE> -B scripts/verify-04a22.py affected --evidence TestResults/04A-22-20261105-run01 --configuration Debug --name entry-qualified-Debug
& <PYTHON_EXE> -B scripts/verify-04a22.py final --evidence TestResults/04A-22-20261105-run01 --configuration Release --name entry-full-Release
& <PYTHON_EXE> -B scripts/verify-04a22.py focused --evidence TestResults/04A-22-20261105-run01 --windows --configuration Debug --name focused-09
& <PYTHON_EXE> -B scripts/verify-04a22.py focused --evidence TestResults/04A-22-20261105-run01 --windows --configuration Release --name focused-10
& <PYTHON_EXE> -B scripts/verify-04a22.py final --evidence TestResults/04A-22-20261105-run01 --configuration Debug --name final-Debug
& <PYTHON_EXE> -B scripts/verify-04a22.py final --evidence TestResults/04A-22-20261105-run01 --configuration Release --name final-Release
& <PYTHON_EXE> -B scripts/verify-04a22.py historical --evidence TestResults/04A-22-20261105-run01 --name final
& <PYTHON_EXE> -B scripts/verify-04a22.py docs --evidence TestResults/04A-22-20261105-run01 --name final-mkdocs
```

Each name is non-overwriting. For a new reproduction choose a fresh batch-qualified run/name. The full transformed workflow invokes the actual `powershell -NoProfile -ExecutionPolicy Bypass -File ...` verifier with `-Legacy -Wasm -Js -Python -VisualStudio 2026`, both configurations, and portable Python `-B`. It runs Core, Common, Windows, OfficeAddin, OfficeExecution and ControlMitigation managed suites; Office Add-in (468), help-resolver (71) and WASM (377) Node suites; all Python roots plus unchanged semantic observations. WASM npm uses Node automatic discovery. The separate explicit `node --test BESHStat.WasmSmoke/tests/*.test.cjs` rerun passes **377/377** and is not added twice to 916. It builds the real netstandard2.0 and net48 sources without installing/restoring packages. Tool versions are recorded separately.

Historical strict/profile entry **and final** exits are **1/1**, with stale UibyID/NormalPlot review dependency and profile `INTEGRITY_FAILURE`; Gate C2 stays UNMET. Entry and final strict MkDocs both pass. The final exact documentation command is `<PYTHON_EXE> -B -m mkdocs build --config-file BESHStatNG_Help_MkDocs/mkdocs.latest.yml --site-dir <REPO_ROOT>/TestResults/04A-22-20261105-run01/final-mkdocs --strict`, exit **0**, on Help SHA-256 `45436839ba3dece3a93174d7c748243f8dc49918ea8b50cd3f411f5059f3c003`. No hidden locale/default path or existing suite is removed.

## 8. Authority, preservation, publication and privacy

Original ID-set derivation confirms Ui21 **208 controls / 226 linked OPEN obligations / 115 restricted original controls**. Ui22 has 182/320/154; the Ui21/Ui22 union is 390 controls/546 obligations. Renderer and original native-case links are crosswalks into those sets, never added as disjoint populations. The broader original populations and all restriction fields remain frozen. **Zero releases/discharges.**

All original Core statistical source/SpcResultTables, Ui22, UDFs, Designers/Ribbon, Office UI, public captures/screenshots/data/workbooks, old tests/evidence/ledgers/dossiers and 100 mixed-model captures remain exact live entry hashes. Historical TestResults retention is checked by full initial membership/metadata; selected frozen captures are also byte-hashed. No claim is made that every historical TestResults byte was reread.

Post-matrix retention passes all **235,231** historical files and **139** nested historical files; selected authority/capture bytes and every unchanged tracked entry file also pass. [Compiled source qualification](../../evidence/04A22/final-source-qualification.json) verifies final application/test/Help pins match the pre-matrix snapshot. After full matrices, edits are confined to evidence, dossier and accounting tooling; compiled/test sources are unchanged. Index is empty and `git diff --check` exits 0. Closing policy contracts pass **14/14**. Publication verification passes **57 actual candidates / 56 external manifest hashes**; only the manifest's own hash is excluded. Final branch/base remain `codex/04A-22` / `9330f8df18c0c73d2b233ec4b06dc7199cbb244a`; local main/origin ref and worktree count remain unchanged.

Public JSON/Markdown use `<PYTHON_EXE>`/`<REPO_ROOT>` and exclude private raw paths/logs. The manifest omits only its own hash to avoid a self-reference cycle while checking its own membership/privacy/trackability. Privacy checks hash every candidate; identical large artifacts reuse a prior actual scan only with matching artifact and scanner/verifier SHA-256 values. Both receipt files are always scanned. Git-ignored historical/build/raw paths remain excluded from publication. Terminal verification receipts remain local after sealing to avoid recursively changing the publication with its own receipt.

## 9. Documentation and future native smoke plan — unexecuted

The control-charts page clarifies existing behavior. Authored `methods.yml`, Windows-only availability, the 60-row CSV, screenshots and published xlsx captures remain unchanged. Strict MkDocs validates the final Help bytes in a fresh site directory. No screenshot/native recapture occurred.

For later separately authorized native testing:

1. **EWMA:** public 60-point Individual column, lambda 0.2, dynamic startup limits; Baseline points 1–30 Phase I and Monitoring 31–60 Phase II referencing Baseline; explicit point 18 exclusion with reason; signal/exclusion labels and stage boundaries enabled. Compare actual chart categories, all returned limit masks, intrinsic markers and the boundary at 30/60 against the reviewed payload. Repeat steady-state selection with a new run. Check warning behavior separately under approved native fault injection.
2. **Attribute/composite:** p using public count/sample-size columns with varying sizes and the same phases/exclusion; compare pointwise CL/UCL/LCL and marker membership without percentage conversion. np uses constant size 100; varying-size np must retain its controlled rejection. Optional X-bar–R uses the five measurement columns and must preserve mean/range panel order, shared gaps/categories and stage-line alignment. Review five output toggles, first-sheet reuse, labels/legend/axis appearance and cleanup in actual Excel.

These are plans, not executed outcomes. Portable payload tests do not establish native chart appearance, Office/Mac availability or scientific-golden accuracy.

## 10. Proposed formal owner decision

Proposed owner decision: accept **only 04A.22 portable univariate SPC presentation extraction** on this software evidence. Keep all scientific/native restrictions, original controls and open candidates; retain `P=NO`, `N1=NO GO`, `N2=NO`, `Gate C2=UNMET`. There are no unfinished in-scope software subpaths or proposed activated behavior fixes. Retained native geometry/visual work and scientific controls remain with their later authorized batches. Git staging/commit/merge/push and commencement of 04A.23 each require separate explicit owner authorization.
