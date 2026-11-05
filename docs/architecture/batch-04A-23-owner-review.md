# Official 04A.23 owner review

READY_FOR_OWNER_04A23_ACCEPTANCE_REVIEW — software extraction only.

The shipping multivariate SPC form consumes new Common request and bounded post-fit policies. New tests pass; the complete managed/Python suites and historical strict/profile validators remain **nonzero**, with every identity retained below and in the linked receipts. This is a software review recommendation, not an owner acceptance or scientific/native release. **P=NO; N1=NO GO; N2=NO; Gate C2=UNMET. Zero native releases and zero deferred-obligation discharges.**

## 1. Verified entry, branch and owner planning edit

Actual initial `HEAD = main = origin/main` was **`519e875a329111270685a0ae4be8666abb55ae04`**, with parent `9330f8df18c0c73d2b233ec4b06dc7199cbb244a` and tree `6ef7b909649108d5e637e3e0f97bfc0de3bd01b8`. Initial status/index were clean, with one worktree. The owner explicitly accepted/merged 04A.22, and actual main contains its Common univariate projection, Ui21/renderer consumers, tests and evidence. The reviewed pre-merge archive and its older entry hash were not used as today's base. Local ancestry was checked without a fetch. `codex/04A-23` was created from this main; HEAD/main/origin refs remain unchanged.

While branch-creation approval was pending, the owner installed Master Plan Revision 2.9. This **preexisting owner edit** is separately pinned and remains byte-identical to its observed working bytes. It is excluded from the 04A.23 candidate manifest and from assertions that batch-owned modified files are the whole worktree. No plan/progress prose was authored by this batch. [owner-plan-observation.json](../../evidence/04A23/owner-plan-observation.json) records both live working and main-blob SHA-256 values and the timing. The literal 04A.23 row and 21–24,28,34/36,39–42,45–48 boundaries govern this extraction. AGENTS.md, previous dossiers and original controls/restrictions remain authoritative.

All 04A.23 files are unstaged/uncommitted; the index is empty. No commit, merge, rebase, reset, clean, stash, push, remote update, installer, signing or deployment occurred. Original historical TestResults and owner work were retained. [entry-state.json](../../evidence/04A23/entry-state.json); [final-git-state.json](../../evidence/04A23/final-git-state.json).

Measured Python entry is 753/850, not the historical reviewed 04A.22 755/850. Two protected 04A.22 methods already fail at entry: `Spc22Contracts.test_core_ui22_udfs_designers_old_tests_targets_and_frozen_evidence` (frozen owner-plan/source pins) and `Spc22Contracts.test_publication_membership_hashes_empty_index_and_privacy` (the old candidate manifest does not describe the current owner-plan/branch/head/candidate state). [historical-entry-deltas.json](../../evidence/04A23/historical-entry-deltas.json) names the exact identities; [python-test-identities.json](../../evidence/04A23/python-test-identities.json) retains all 850 entry and 868 final unittest identities per configuration. In final traces the same old source-guard method encounters the authorized Ui22 hash change before its already-failing plan pin. This additional assertion-site difference is disclosed without inventing a new method count or rewriting the old expectation.

## 2. Actual shipping call graph and ownership

Before: Ui22 `BuildRequest` acquired Excel values, grouped metadata, grid assignments/exclusions and historical values, then directly constructed one Core request; after Fit, native methods independently selected diagnostics and built the first-panel renderer input. After:

```text
Ui22.btCompute_Click
  -> BuildRequest
     -> unchanged Windows ValidateSelections and worksheet/culture conversion
     -> GetInputRowContext -> Common SpcMultivariatePointMapPolicy
     -> NormalizeGroupedMetadata -> detached point map
     -> BuildPointAssignments/BuildPointExclusions -> staged Common builders
     -> pointMap.Expand -> raw-N phase/stage/exclusion/source arrays
     -> unchanged Windows BuildHistoricalModel
     -> immutable ModelIntent + Options -> Common RequestPolicy.Create
     -> one original Core SpcMultivariateRequest constructor
  -> unchanged BeginComputation / one SpcMultivariate.Fit(request, cancellation)
  -> unchanged WriteResultsToNewWorkbook
     -> original ResultTable builders / ExcelDnaResultWriter
     -> BuildChartRenderingResult -> Common ResultAxisPolicy.Project/RenderingInput
        -> original SpcFitRequest/SpcFitResult compatibility wrapper
        -> accepted 04A.22 shared renderer and Common univariate projection
     -> BuildDiagnosticTables -> Common context.SelectDiagnostics(signalOnly)
        -> original diagnostic table construction tail
```

`PopulateObservationStructures` and `UpdateChartDependentControls` use `RequestPolicy.Compatibility`. Quick setup and imported stages/exclusions consume the new phase/import policies. Control setters, pseudo-inverse disable/restore, lower-limit clearing, warnings, chart descriptions, acquisition chronology, busy/cancel/logging and error catches remain Windows-owned. No form or COM object is constructed by the new adapter tests; compiled shipping IL calls are inspected and pure methods exercised.

Core alone validates/computes covariance, rank, inverses, PCA, control limits, sequential states, signals and missing/exclusion effects. Common only assembles requested choices and maps immutable Core metadata. There is no Common Fit, second rule detector, recurrence, threshold/ARL formula, new worksheet schema or numerical table reconstruction. Core statistical source and SpcResultTables are unchanged.

## 3. Implementation and compilation inventory

| File | Production role |
|---|---|
| `BESHStat.Excel.Common/src/Analysis/SpcMultivariatePointMapPolicy.vb` | Physical/acquired/logical/display identities; Trim/Ordinal first-seen groups; metadata collapse; raw-row expansion. |
| `BESHStat.Excel.Common/src/Analysis/SpcMultivariatePhaseExclusionPolicy.vb` | Manual stage/exclusion assembly, quick phase choices, imported phase runs and exclusion reasons. |
| `BESHStat.Excel.Common/src/Analysis/SpcMultivariateRequestPolicy.vb` | Immutable requested options, detached historical shape/variable axis, six-chart compatibility and original Core constructor composition. |
| `BESHStat.Excel.Common/src/Analysis/SpcMultivariateResultAxisPolicy.vb` | Shallow immutable Core model/panel/diagnostic context, signal-only selection and legacy first-panel input mapping. |
| `BESHstatNG/src/UI/Ui22MultivariateControlCharts.vb` | Real delegates listed above; native rendering, output selection and table builders retained. |
| `BESHStatNG.Test/BESHStatNG.Tests.vbproj` | Five explicit Compile entries: adapter plus four linked Common test/helper files. |

New compiled tests/helpers are `Spc23Tests.vb`, `Spc23Fixture.vb`, `Spc23Data.vb`, `Spc23LegacyPreparation.vb` under Common.Tests/src and `Spc23AdapterTests.vb` under Windows tests/src. Common SDK compilation includes its four new production files implicitly; targets remain netstandard2.0, Windows Framework 4.8. Framework tests link the same Common tests. [project-compilation-inventory.json](../../evidence/04A23/project-compilation-inventory.json) gives entry/final project hashes, explicit and implicit membership. [prechange-method-provenance.json](../../evidence/04A23/prechange-method-provenance.json) contains every original Ui22 method body, source SHA and original line boundaries; [source-preservation.json](../../evidence/04A23/source-preservation.json) gives the exact 16-method delta/removal inventory and final hashes.

Durable map/model/options/result captures are detached and copy-safe. Stage/exclusion builders are explicitly short-lived mutable assemblers so validation can interleave with unchanged native cell parsing. Their returned collections are read-only snapshots. Model intent copies only P names/means and P×P historical covariance; N×P measurements are borrowed synchronously until the original Core constructor makes its durable copy. Member lists make grouped collapse O(N+G), retaining logical-point then member-row failure priority. The result context retains immutable Core references and shallow list snapshots, not a copied measurement graph. Original `panel.GetPoint` diagnostic lookup behavior is preserved.

Verification tooling: `capture-04a23-contract.py`, `generate-04a23-legacy.py`, `generate-04a23-fixture.py`, `verify-04a23.py`, `summarize-04a23.py`, `verify-04a23-publication.py`; new Python contracts: `scripts/tests/test_04a23_multivariate_spc.py`. The sole Help edit is described below. Every evidence/dossier/source/test/tool path, byte size, mode and SHA-256 is enumerated by [publication-manifest.json](../../evidence/04A23/publication-manifest.json); the owner plan is the explicit excluded owner edit. No old test/helper expectation was rewritten.

## 4. Six choices, rows, phases, options and post-fit parity

| Existing choice | Observation structure | Requested settings retained |
|---|---|---|
| Hotelling T² | Individual or subgroup | Historical/Phase I model, alpha, ridge, pseudoinverse; lower-limit checkbox only for individuals. |
| Generalized variance | Subgroup required | Covariance-only history, alpha or supplied sigma multiplier; temporary pseudoinverse disable/restore. |
| PCA T² | Individual | Covariance/correlation, fixed nullable k or original cumulative percent/100. |
| PCA Q | Individual | Same requested PCA choices; residual rank/fallback remains Core-owned. |
| MEWMA | Individual | Lambda, optional supplied UCL, alpha enablement, stage/phase/signal resets and sequence-gap choice. |
| MCUSUM (Crosier) | Individual | Original k/h, disabled alpha decision control, resets/gaps and intrinsic Core alerts. |

The exact original control-to-request mapping and numeric spinner bounds/defaults are in [request-option-coverage-v1.json](../../evidence/04A23/request-option-coverage-v1.json). The two supported missing policies remain Reject/OmitPoint; UseAvailableMeasurements is not exposed. All six pass through one policy and their actual Core panel shape/order is observed in [multivariate-family-observations-v1.json](../../evidence/04A23/multivariate-family-observations-v1.json). These descriptors do not enable any extra enum choice.

Required roles alone determine header offset (2 with a required header, otherwise 1) and required-column maximum last row. Group IDs use Trim and StringComparer.Ordinal, first encounter order; nonadjacent repeated IDs coalesce, A/a remain distinct. Sparse source fixtures 101/105/118/131 demonstrate physical row, acquired index, logical point and display number separation. Labels use exact blank/conflict rules; sequence collapse skips NaN, preserves equal signed-zero handling and expands only after all groups validate. The host still rereads subgroup IDs in the original BuildRequest chronology. No worksheet provenance is guessed from point+header.

Manual stages retain OrdinalIgnoreCase uniqueness, inclusive one-based bounds, overlap-before-marking, complete coverage and default Stage1/PhaseI. Native cell parsing stays between BeginStage/ValidateRange/Assign calls. Raw request arrays remain N observations long even when stage/exclusion bounds are G grouped points. Quick all-II still requires history; quick setup clears the grid before its guards. Imported phase collapse validates before grid clear; lazy ImportedRows preserves partial successful row additions before a repeated-block error. Phase runs preserve original IDs/casing/run counters. Exclusion flags OR, reason uniqueness uses Ordinal HashSet semantics with the existing enumeration/join order; imports include reasons only from flagged rows.

Historical grid import and culture-aware conversion are unchanged, including no mean-vector read for GV. Core retains all dimensionality, finite, covariance, alpha/limit and numerical rejection authority. Common does not symmetrize, regularize or estimate. Requested options are separated from Core effective dimension, pseudoinverse use, retained PCA components, baseline counts/df and warnings in the corpus. Synthetic API-invalid option/shape cases are clearly identified; they do not claim additional UI availability or new GUI refusals.

The post-fit context preserves actual Core model/panels, point indices, all source rows, stage/phase, labels/sequence, values/limits, signal occurrences/marked contributors and eligibility/exclusion flags. Diagnostics scope uses original first-panel.GetPoint(diagnostic.PointIndex).IsSignalled; no marker is inferred from diagnostic magnitude or inverted exclusion participation. The legacy renderer input retains first physical source row or Core PointIndex fallback, sequence-or-NaN and literal Statistic name. Panel clones/title/value-axis overrides and SpcFitRequest wrapper remain native and source-equal. Table headers, CLR cell types, warnings and all diagnostic table tails remain source-equal.

Output order stays Multivariate Summary, Model Details, Multivariate Chart, Chart Data, Signals, Diagnostics, Settings and Audit, conditioned on the original seven toggles. No chart/result work is forced when its output is off; original selection validation still rejects no outputs. Workbook reuse, first output activation, AutoFit best-effort, chart warning catch and renderer cleanup are unchanged. Source equality is not live workbook verification.

## 5. Worked comparisons and negative outcomes

The new corpus has **3,196 records / 1,462 content-addressed payloads**: 1,280 EXISTING_PREPARATION_EXCEPTION, 192 EXISTING_CORE_REJECTION, 1,724 POSITIVE_PARITY. These are **NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY**. Counts include configuration/framework repeats; they are not distinct scientific validations. Preparation exceptions include original Core-constructor rejection during request assembly; Fit rejections are counted separately. Negative records are not positive parity successes.

The original public 150×3 CSV is unchanged: SHA-256 `b3c5c52b1a1c570342b89cbee53045085a4c64eb32084596147afd16c2d75a7c`; columns Temperature_C, Pressure_kPa, Flow_L_min. The embedded decimal source is independently pinned to it. Profiles 0–359 use documented deterministic input/options/negative transformations in the new fixture; published-0–6 separately reproduce 100/50 individual or 20/10 subgroup stages, exclusions 36–40/subgroup8, PCA k2, lambda .2 and ridge0. These differ deliberately from ordinary matrix profiles, whose first 2/3 stages and exclusion point3 are not presented as the published example settings.

| Published profile | Chart/panel | Points | Finite values | Signal markers (display points) | Explicit exclusions | Payload ID |
|---|---|---:|---:|---|---|---|
| published-0 | HotellingT2 / HotellingT2 | 150 | 150 | 132,135,136,137,138,139,140 | 36,37,38,39,40 | `e55fe16560d5a50d0da4c073215920b455953426ae1804fa0a880f5cf93c4846` |
| published-1 | GeneralizedVariance / GeneralizedVariance | 30 | 30 | 27 | 8 | `a373e09975ef8283b306557498f969b6f68a4d55cb6c74c5c4cf96a8e09c8193` |
| published-2 | PcaT2 / PcaT2 | 150 | 150 | 132,135 | 36,37,38,39,40 | `7b5afc4764e5f31eae7ba115b9ccf8fc538b65378cb20c66634c6880f7fc05a8` |
| published-3 | PcaQ / PcaQ | 150 | 150 | 18,136,137,138,139,140 | 36,37,38,39,40 | `d22c0c13a935c616f03ef00676257db52a662e2876677896739f5ebcea50cb7d` |
| published-4 | Mewma / Mewma | 150 | 150 | 35,132,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150 | 36,37,38,39,40 | `647ecf1bbe6577f1f933af3e471ddbb582aaea21e7c36a560065f54dd995ebcc` |
| published-5 | Mcusum / Mcusum | 150 | 150 | 30,31,32,33,34,35,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,134,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150 | 36,37,38,39,40 | `55dadbb4df6c18ecd10ba8aba61ea082867167b54f765e51f7488ee04e9dcc5f` |
| published-6 | HotellingT2 / HotellingT2 | 30 | 30 | 6,27,28,30 | 8 | `f3ce4f3fdb2eacec01197dacb2e82241329815fbd1833eb7819065bd07dfc7d6` |

Full original Value/CL/LCL/UCL bit arrays, flags, phases, labels, source rows and occurrences are in the columnar corpus; optional diagnostic arrays have typed content hashes and lengths. [worked-compatibility-examples-v1.json](../../evidence/04A23/worked-compatibility-examples-v1.json) provides readable first values and model metadata. Each request is compared with the captured prechange constructor/loops, followed by one authoritative Core Fit. The two post-fit paths receive that same result. Double bits/CLR types/array shapes and strings/order are exact; no statistical tolerance or alternative formula is introduced. Existing table-hash normalization removes only established execution-time cells. Culture/runtime/configuration and raw receipt hashes are recorded. No cross-runtime statistical identity is assumed.

Representative retained negatives (each exact message/type and request-stage boundary is also in the corpus):

- Profile 109, GeneralizedVariance, `point-map`, `System.ArgumentException`: A subgroup ID is blank at worksheet row 3. (`b1f565407b3b21589eaec38177ed819f6432d8fb619137dc828524adc5f38340`).
- Profile 126, HotellingT2, `request`, `System.ArgumentException`: Stage ID 'baseline' is duplicated. (`a24d82adca5820d03feb732bacfe7fbe7ab7560debda8629941f93fa027e7f3a`).
- Profile 180, HotellingT2, `request`, `System.ArgumentException`: The historical covariance matrix must be square and match the number of variables. (Parameter 'historicalCovariance') (`cdd880b2c6b8653fe294d63f52153f9226029e535fc624e3eac12ba7181e351c`).
- Profile 204, HotellingT2, `request`, `System.ArgumentException`: Measurements must not contain infinity. (Parameter 'measurements') (`9cc63db2bb41f2c880f582ac97af4b8671c041d60438b1fe0ff53f04af7e4737`).
- Profile 312, HotellingT2, `core-fit`, `System.ArgumentException`: At least three eligible Phase-I observations are required to estimate a multivariate model. (`065252ed4f44054db90b539c4af0f3c1a7f881c59c7019dff78290daf51697fb`).
- Profile 325, GeneralizedVariance, `core-fit`, `System.ArgumentException`: Each generalized-variance subgroup must contain more observations than variables. (`be911e8bf2fd02ecba6b5afc6b10033e11c8fc062f97e8f1afe8c92ba3785c12`).
- Profile 236, PcaT2, `core-fit`, `System.ArgumentOutOfRangeException`: The PCA component count must not exceed the covariance rank. (Parameter 'PcaComponentCount') (`d85bdac6e2dcee90bb8daf5798b820c293cabd9a1fe834b8caa990261ac8a432`).

The frozen host-free legacy helper preserves original predicates/loops/errors and the exact Core constructor expression. Its adaptations are limited to injected acquired arrays/options, inert managed grid/cell and audit-control snapshots, widened visibility, diagnostic scope Boolean and extracted renderer-input/selection loops. It imports no new Common policy. Original method hashes and both overload table variants are pinned in [legacy-preparation-provenance.json](../../evidence/04A23/legacy-preparation-provenance.json). The generator creates a fresh draft and refuses overwrite; entry suites finished before new TestClasses were compiled.

Rank profiles 0–35, each in invariant/sk-SK, add six covariance settings for all six charts: estimated singular covariance with pseudoinverse off/on, ridge .001, singular historical covariance, tied identity historical covariance, and two-variable input. Actual supported Hotelling pseudoinverse cases assert effective dimension two and UsedPseudoInverse=True. GV retains its original forced-off pseudoinverse setting and rejected singular subpaths. Actual Core variable-name axes are emitted, including normalized requested headings. [multivariate-family-observations-v1.json](../../evidence/04A23/multivariate-family-observations-v1.json) reports effective inverse/rank/variable-count observations; no model or limit is recomputed in this evidence.

## 6. Test truth: entry versus final

| Configuration/state | Logical managed | Raw framework executions | Node | Python |
|---|---|---|---|---|
| Debug entry | 8,609/8,616 (7 nonpass) | 12,860/12,868 (8 failed) | 916/916 | 753/850 (87 FAIL, 10 ERROR) |
| Debug final | 9,444/9,451 (7 nonpass) | 14,524/14,532 (8 failed) | 916/916 | 771/868 (87 FAIL, 10 ERROR) |
| Release entry | 8,609/8,616 (7 nonpass) | 12,860/12,868 (8 failed) | 916/916 | 753/850 (87 FAIL, 10 ERROR) |
| Release final | 9,444/9,451 (7 nonpass) | 14,524/14,532 (8 failed) | 916/916 | 771/868 (87 FAIL, 10 ERROR) |

New 04A.23 tests: **835/835 logical managed** per configuration, 829 Common plus six Windows adapter identities; linked Common cases execute again under Framework. **18/18 Python** pass in focused, both full and final publication qualification. Distinct logical identities and all raw executions are recorded separately; repeated focused runs are excluded from full totals. The supplemental explicit WASM Node glob passed 377/377 and is not added to 916. The six full managed suites include Core, Common, Windows, OfficeAddin, Office/WASM Execution and ControlMitigation. Builds retain netstandard2.0/net48/net10.0 and succeed Debug/Release without restore.

Seven inherited logical managed failures remain (eight raw executions; Regression10 occurs in both Common and Framework):

- `ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`: Assert.ThrowsException failed. Expected exception type:<System.OverflowException> but no exception was thrown. 
- `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,30,50)`: Assert.IsFalse failed. No silent choice between conflicting scientific mappings
- `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (0,0,0)`: Assert.IsFalse failed. No silent choice between conflicting scientific mappings
- `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,15,40)`: Assert.IsFalse failed. No silent choice between conflicting scientific mappings
- `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings (True)`: Assert.ThrowsException failed. Expected exception type:<System.NullReferenceException> but no exception was thrown. 
- `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`: Assert.AreEqual failed. Expected:<6.49310735531625E-08>. Actual:<0.499559833202511>. 
- `Spc21AdapterTests.NativeHostCoercionStateFitOutputAndChartTailStayEntryBytes (WriteResultsToNewWorkbook)`: Assert.AreEqual failed. Expected:<0e623c8c0c8c0907e100525e689126647c0f66484f05586561b9cd4b9e0b876c>. Actual:<2b648024ad85103748a6d657ba47025c08ed401dba1e8bc719e8989d9979519f>. 

No inherited assertion was weakened. The Spc21 source pin already failed at merged-main entry because accepted 04A.22 changed Ui21's output method. Earlier proportion/contingency/binomial failures remain under their original owners. All exact managed messages and every Python FAIL/ERROR identity/trace are in [nonpasses.json](../../evidence/04A23/nonpasses.json) and [python-nonpass-traces.json](../../evidence/04A23/python-nonpass-traces.json).

New Python nonpass identities relative to actual entry:

None. Already-failing whole-source pins can contain additional Ui22/project differences within the same inherited method; final traces retain them.

The complete final Python register follows. The classifications describe test method status, not discharged native cases.

| Python identity | Outcome | Entry relationship |
|---|---|---|
| `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook` | FAIL | Inherited at actual entry |
| `test_04a02_checkpoint11.Namespace11Tests.test_exact_retained_bytes_reproduce_only_compiler_rejection` | FAIL | Inherited at actual entry |
| `test_04a02_checkpoint11.Provenance11Tests.test_both_authorized_case_pins_pass_and_frozen_false_is_not_owner_decision` | ERROR | Inherited at actual entry |
| `test_04a02_checkpoint12.Diagnosis12Tests.test_both_authorized_case_pins_keep_original_compiler_and_scientific_policy` | ERROR | Inherited at actual entry |
| `test_04a02_checkpoint12.Diagnosis12Tests.test_exact_owner_bytes_negative_for_frozen_v5` | FAIL | Inherited at actual entry |
| `test_04a02_checkpoint13.Startup13Tests.test_corrupt_receipt_hash_fails_even_with_exit_zero` | FAIL | Inherited at actual entry |
| `test_04a02_checkpoint13.Startup13Tests.test_real_successful_owner_startup_all_pins_and_native_identities` | ERROR | Inherited at actual entry |
| `test_04a02_checkpoint13.Startup13Tests.test_tampered_stdout_hash_stops_before_parser` | FAIL | Inherited at actual entry |
| `test_04a02_checkpoint14.Actual14Tests.test_actual_two_owner_publications_and_both_cultures_reproduce` | ERROR | Inherited at actual entry |
| `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings` | ERROR | Inherited at actual entry |
| `test_04a03_control_mitigation.ControlMitigationSourceTests.test_per_mode_consumers_and_branch_order` | FAIL | Inherited at actual entry |
| `test_04a03_control_mitigation.ControlMitigationSourceTests.test_sealed_sources_are_reconciled_never_rewritten` | FAIL | Inherited at actual entry |
| `test_04a03_control_mitigation.NativeGateNegativeTests.test_review_packet_source_pins_match_current_sources` | FAIL | Inherited at actual entry |
| `test_04a03_selection.UnivariateSelectionSourceTests.test_all_native_lifetime_cancel_write_and_reporting_methods_unchanged` | FAIL | Inherited at actual entry |
| `test_04a03_selection.UnivariateSelectionSourceTests.test_only_two_form_methods_change` | FAIL | Inherited at actual entry |
| `test_04a04_alignment.PairedBlockSourceGates.test_exact_crosswalk_and_authorities` | FAIL | Inherited at actual entry |
| `test_04a04_alignment.PairedBlockSourceGates.test_legacy_geometry_ambiguity_is_not_hidden` | FAIL | Inherited at actual entry |
| `test_04a05_completion_successor.Completion05SuccessorTests.test_independent_candidate_provenance_not_core_or_golden` | FAIL | Inherited at actual entry |
| `test_04a05_completion_successor.Completion05SuccessorTests.test_nonpaired_code_and_unrelated_forms_are_unchanged_from_partial` | FAIL | Inherited at actual entry |
| `test_04a05_completion_successor.Completion05SuccessorTests.test_owner_bound_successor_and_source_hashes` | FAIL | Inherited at actual entry |
| `test_04a05_contingency.ContingencySourceGates.test_historical_ledgers_and_importer_unchanged` | FAIL | Inherited at actual entry |
| `test_04a05_contingency.ContingencySourceGates.test_other_multicol_members_including_04a04_selection_unchanged` | FAIL | Inherited at actual entry |
| `test_04a05_contingency.ContingencySourceGates.test_prior_04a04_whole_file_source_check_remains_frozen_and_disclosed` | FAIL | Inherited at actual entry |
| `test_04a05_contingency.ContingencySourceGates.test_scientific_ambiguity_is_not_a_permanent_portable_wrong_table` | FAIL | Inherited at actual entry |
| `test_04a05_contingency.ContingencySourceGates.test_ui82x2_source_wiring_explicitly_withheld` | FAIL | Inherited at actual entry |
| `test_04a05_contingency.ContingencySourceGates.test_versioned_successor_sources_and_case_boundaries` | FAIL | Inherited at actual entry |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_actual_form_routes_and_unchanged_native_tails` | FAIL | Inherited at actual entry |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_importer_udf_designers_core_and_old_tests_unchanged` | FAIL | Inherited at actual entry |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_native_population_is_union_not_sum` | FAIL | Inherited at actual entry |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_new_scientific_candidates_are_explicitly_unmodified` | FAIL | Inherited at actual entry |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_no_duplicate_statistics_in_form_or_windows_adapter` | FAIL | Inherited at actual entry |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_production_sources_and_exact_open_ledger_lineage` | FAIL | Inherited at actual entry |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_actual_four_shipping_routes_and_native_bodies` | FAIL | Inherited at actual entry |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_core_old_tests_importer_udfs_designers_and_help_unchanged` | FAIL | Inherited at actual entry |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_exact_source_and_original_ledger_rows` | FAIL | Inherited at actual entry |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_no_fabricated_obligation_for_previously_source_qualified_controls` | FAIL | Inherited at actual entry |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_restriction_unions_and_timing_exceptions_not_generalized` | FAIL | Inherited at actual entry |
| `test_04a08_authoring_successor.Authoring08SuccessorTests.test_exact_id_crosswalk_retains_original_row_hashes_and_open_counts` | FAIL | Inherited at actual entry |
| `test_04a08_authoring_successor.Authoring08SuccessorTests.test_original_numerics_and_shipping_forms_are_unchanged` | FAIL | Inherited at actual entry |
| `test_04a09_regression_successor.Regression09SuccessorTests.test_exact_six_changed_members_and_unchanged_native_surroundings` | FAIL | Inherited at actual entry |
| `test_04a09_regression_successor.Regression09SuccessorTests.test_frozen_tests_numerics_and_public_04a08_evidence_preserved` | FAIL | Inherited at actual entry |
| `test_04a09_regression_successor.Regression09SuccessorTests.test_real_six_routes_use_common_options_counts_and_reports` | FAIL | Inherited at actual entry |
| `test_04a09_regression_successor.Regression09SuccessorTests.test_runtime_baselines_are_incremental_and_complete` | FAIL | Inherited at actual entry |
| `test_04a10_regression_projection.Regression10ProjectionTests.test_baselines_are_actual_gaussian_and_binomial_and_not_goldens` | FAIL | Inherited at actual entry |
| `test_04a10_regression_projection.Regression10ProjectionTests.test_core_udfs_frameworks_and_accepted_evidence_are_byte_protected` | FAIL | Inherited at actual entry |
| `test_04a10_regression_projection.Regression10ProjectionTests.test_only_two_form_routes_change_and_native_tails_match` | FAIL | Inherited at actual entry |
| `test_04a11_publication.Publication11Tests.test_accepted_dossier_and_dormant_patches_unchanged` | FAIL | Inherited at actual entry |
| `test_04a11_publication.Publication11Tests.test_all_manifests_and_implementation_freeze` | FAIL | Inherited at actual entry |
| `test_04a11_publication.Publication11Tests.test_raw_local_source_pins_and_native_writer_tails` | FAIL | Inherited at actual entry |
| `test_04a11_regression_families.Regression11SuccessorTests.test_exact_six_form_deltas_and_native_writer_tails` | FAIL | Inherited at actual entry |
| `test_04a11_regression_families.Regression11SuccessorTests.test_unaffected_sources_and_all_old_evidence_are_byte_protected` | FAIL | Inherited at actual entry |
| `test_04a12_gee_workflow.Gee12WorkflowSuccessorTests.test_exact_source_scope_and_all_old_byte_pins` | FAIL | Inherited at actual entry |
| `test_04a13_survival_workflow.Survival13SourceContracts.test_all_prechange_protected_bytes_except_exact_approved_scope` | FAIL | Inherited at actual entry |
| `test_04a13_survival_workflow.Survival13SourceContracts.test_local_main_ancestry_empty_index_and_dedicated_branch` | FAIL | Inherited at actual entry |
| `test_04a13_survival_workflow.Survival13SourceContracts.test_successor_pins_and_members_reconcile_to_actual_forms` | FAIL | Inherited at actual entry |
| `test_04a14_mixed_workflow.Mixed14SourceContracts.test_branch_base_ancestry_and_empty_index` | FAIL | Inherited at actual entry |
| `test_04a14_mixed_workflow.Mixed14SourceContracts.test_exact_scope_and_external_owner_plan_are_separate` | FAIL | Inherited at actual entry |
| `test_04a14_mixed_workflow.Mixed14SourceContracts.test_original_members_and_successor_pins_are_reconcilable` | FAIL | Inherited at actual entry |
| `test_04a14_mixed_workflow.Mixed14SourceContracts.test_reports_are_consumed_by_actual_writers_and_append_seams` | FAIL | Inherited at actual entry |
| `test_04a15_fitting.Mixed15SourceContracts.test_all_protected_sources_and_new_source_hashes` | FAIL | Inherited at actual entry |
| `test_04a15_fitting.Mixed15SourceContracts.test_branch_ancestry_empty_index_and_exact_existing_scope` | FAIL | Inherited at actual entry |
| `test_04a15_fitting.Mixed15SourceContracts.test_only_actual_shipping_factory_routes_change` | FAIL | Inherited at actual entry |
| `test_04a16_post_estimation.Mixed16SourceContracts.test_branch_base_index_and_exact_scope` | FAIL | Inherited at actual entry |
| `test_04a16_post_estimation.Mixed16SourceContracts.test_unchanged_methods_designers_udfs_and_core` | FAIL | Inherited at actual entry |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_acquisition_error_order_and_legacy_grammars_preserved` | ERROR | Inherited at actual entry |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_actual_six_handlers_delegate_and_keep_one_calculation` | FAIL | Inherited at actual entry |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_live_base_branch_index_and_protected_sources` | FAIL | Inherited at actual entry |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_native_writer_statements_and_spm_dispatch_stay_identical` | FAIL | Inherited at actual entry |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_public_targets_and_successor_hashes` | FAIL | Inherited at actual entry |
| `test_04a18_projection.Projection18SourceContracts.test_live_branch_index_and_protected_entry_bytes` | FAIL | Inherited at actual entry |
| `test_04a18_projection.Projection18SourceContracts.test_only_three_form_methods_change_and_spm_remains_exact` | FAIL | Inherited at actual entry |
| `test_04a19_clustering.Clustering19Contracts.test_all_other_form_methods_and_import_catches_preserved` | FAIL | Inherited at actual entry |
| `test_04a19_clustering.Clustering19Contracts.test_core_native_writer_and_prior_test_bytes_unchanged` | FAIL | Inherited at actual entry |
| `test_04a19_clustering.Clustering19Contracts.test_live_base_branch_and_empty_index` | FAIL | Inherited at actual entry |
| `test_04a20_factor_discriminant.FactorDiscriminant20Contracts.test_core_native_prior_tests_targets_and_metadata_are_entry_bytes` | FAIL | Inherited at actual entry |
| `test_04a20_factor_discriminant.FactorDiscriminant20Contracts.test_live_main_branch_index_and_accepted_source` | FAIL | Inherited at actual entry |
| `test_04a21_spc.Spc21Contracts.test_core_ui22_native_designers_prior_tests_and_targets_unchanged` | FAIL | Inherited at actual entry |
| `test_04a21_spc.Spc21Contracts.test_live_merged_main_ancestry_branch_and_empty_index` | FAIL | Inherited at actual entry |
| `test_04a21_spc.Spc21Contracts.test_single_fit_cancellation_and_original_native_output_tail` | FAIL | Inherited at actual entry |
| `test_04a22_spc.Spc22Contracts.test_core_ui22_udfs_designers_old_tests_targets_and_frozen_evidence` | FAIL | Inherited at actual entry |
| `test_04a22_spc.Spc22Contracts.test_publication_membership_hashes_empty_index_and_privacy` | FAIL | Inherited at actual entry |
| `test_04a27_geometry_completion.Geometry27CompletionTests.test_compile_targets_and_native_helpers_unchanged` | FAIL | Inherited at actual entry |
| `test_04a27_geometry_completion.Geometry27CompletionTests.test_initial_implementation_and_historical_tests_not_rewritten` | FAIL | Inherited at actual entry |
| `test_04a27_geometry_completion.Geometry27CompletionTests.test_real_consumer_routes_and_udf_exports_preserved` | FAIL | Inherited at actual entry |
| `test_04a27_geometry_completion.Geometry27CompletionTests.test_source_owner_pins_and_all_44_original_rows` | FAIL | Inherited at actual entry |
| `test_04a27_transport_successor.Transport27SuccessorTests.test_core_designer_and_refedit_sources_are_unmodified` | FAIL | Inherited at actual entry |
| `test_04a27_transport_successor.Transport27SuccessorTests.test_exact_successor_and_unapplied_patch` | FAIL | Inherited at actual entry |
| `test_capture_contract04a02.CaptureContracts.test_all_239_ledger_rows_identified_with_original_digest_convention` | ERROR | Inherited at actual entry |
| `test_capture_contract04a02.CaptureContracts.test_complete_source_crosswalk_equality` | ERROR | Inherited at actual entry |
| `test_capture_contract04a02.CaptureContracts.test_packet_covers_all_original_cases_and_six_gates` | FAIL | Inherited at actual entry |
| `test_capture_contract04a02.CaptureContracts.test_packet_retains_additional_21_deferred_obligations` | ERROR | Inherited at actual entry |
| `test_investigation06.Investigation06Tests.test_all_239_exact_ids_have_reversible_root_cause_source_maps` | ERROR | Inherited at actual entry |
| `test_investigation06.Investigation06Tests.test_anova_defaults_mapping_remain_explicit_source_facts` | FAIL | Inherited at actual entry |
| `test_investigation06.Investigation06Tests.test_normality_5000_disagreement_preserved_as_review_defect` | FAIL | Inherited at actual entry |
| `test_investigation06.Investigation06Tests.test_pca_has_three_biplot_scales_not_the_old_two_scale_proposal` | FAIL | Inherited at actual entry |
| `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof` | FAIL | Inherited at actual entry |
| `test_investigation06.Investigation06Tests.test_two_by_two_or_condition_and_no_chart_handler` | FAIL | Inherited at actual entry |

Both full verifier commands exit **1**. Historical entry/final strict and profile exits are **1 / 1**, retaining stale NormalPlot/UibyID dependency and profile INTEGRITY_FAILURE identities. Strict MkDocs exits **0** at entry and on final authored bytes. [gate-validation.json](../../evidence/04A23/gate-validation.json) preserves actual diagnostics/command hashes. None of these nonzero suites is described as green.

## 7. Reproduction, transient receipts and final bytes

Run from `<REPO_ROOT>` with configured `<PYTHON_EXE>` (portable Python 3.14.7) and the installed offline VS2026/SDK10.0.403 toolchain. These are actual run names; the harness refuses to overwrite them. A reproduction needs a fresh batch-qualified run, not reuse of these result directories.

```text
<PYTHON_EXE> -B scripts/verify-04a23.py init --evidence TestResults/04A-23-20261105-run01
<PYTHON_EXE> -B scripts/capture-04a23-contract.py
<PYTHON_EXE> -B scripts/verify-04a23.py final --evidence TestResults/04A-23-20261105-run01 --configuration Debug --name entry-Debug
<PYTHON_EXE> -B scripts/verify-04a23.py final --evidence TestResults/04A-23-20261105-run01 --configuration Release --name entry-Release
<PYTHON_EXE> -B scripts/verify-04a23.py focused --evidence TestResults/04A-23-20261105-run01 --configuration Debug --name focused-09 --windows
<PYTHON_EXE> -B scripts/verify-04a23.py focused --evidence TestResults/04A-23-20261105-run01 --configuration Release --name focused-10 --windows
<PYTHON_EXE> -B scripts/verify-04a23.py focused --evidence TestResults/04A-23-20261105-run01 --configuration Debug --name focused-15 --windows
<PYTHON_EXE> -B scripts/verify-04a23.py focused --evidence TestResults/04A-23-20261105-run01 --configuration Release --name focused-16 --windows
<PYTHON_EXE> -B scripts/verify-04a23.py final --evidence TestResults/04A-23-20261105-run01 --configuration Debug --name final-Debug
<PYTHON_EXE> -B scripts/verify-04a23.py final --evidence TestResults/04A-23-20261105-run01 --configuration Release --name final-Release
<PYTHON_EXE> -B scripts/verify-04a23.py final --evidence TestResults/04A-23-20261105-run01 --configuration Debug --name final-qualified-Debug
<PYTHON_EXE> -B scripts/verify-04a23.py final --evidence TestResults/04A-23-20261105-run01 --configuration Release --name final-qualified-Release
<PYTHON_EXE> -B scripts/verify-04a23.py final --evidence TestResults/04A-23-20261105-run01 --configuration Debug --name final-complete-Debug
<PYTHON_EXE> -B scripts/verify-04a23.py final --evidence TestResults/04A-23-20261105-run01 --configuration Release --name final-complete-Release
<PYTHON_EXE> -B scripts/verify-04a23.py historical --evidence TestResults/04A-23-20261105-run01 --name final
<PYTHON_EXE> -B scripts/verify-04a23.py historical --evidence TestResults/04A-23-20261105-run01 --name final-qualified
<PYTHON_EXE> -B scripts/verify-04a23.py docs --evidence TestResults/04A-23-20261105-run01 --name final-mkdocs
node --test BESHStat.WasmSmoke/tests/*.test.cjs
<PYTHON_EXE> -B scripts/verify-04a23.py preserve --evidence TestResults/04A-23-20261105-run01 --name final-post
<PYTHON_EXE> -B scripts/verify-04a23-publication.py verify
```

The actual executed PowerShell verifier derivative uses `-NoProfile -ExecutionPolicy Bypass`, both configurations and `-Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe <PYTHON_EXE>`. [verification-workflow-provenance.json](../../evidence/04A23/verification-workflow-provenance.json) records original/executed SHA-256 and exact minimal diffs. It preserves suite enumeration, continues independent suites after nonzero exits, redirects result/assembly writers, uses --no-restore and compile-only Windows targets to avoid frozen log overwrites, network restore, generators, signing and packaging. The original repository verifier is unchanged. [command-outcomes.json](../../evidence/04A23/command-outcomes.json) records exact expanded commands/exit codes; raw logs/TRX remain local, publicly represented by sanitized receipts/hashes.

[transient-validation.json](../../evidence/04A23/transient-validation.json) retains unsuccessful fixture/accounting attempts: initial missing inert audit controls and loop-name syntax, Windows Count overload, signed-zero test expectation before complete-group validation, and a published subgroup fixture accidentally retaining history after changing model source. All were new fixture/tool issues, corrected without changing Core/shipping behavior; old tests remain unchanged. Accounting generation also retained its failed/aborted initial corpus/inventory attempts before correcting duplicate signal-count keys and redundant hashing/redaction traversal. No transient receipt substitutes for final results.

The initial final-Debug/final-Release matrices passed every new case with the same inherited nonpasses. Final review then restored the original host-side short circuit: IsGroupedStructure is read only for Hotelling, and supplied-limit checkbox getters are read only for their applicable chart. No statistical/accepted correction was activated. Initial compiled input pins remain in [qualification-before-final-v1.json](../../evidence/04A23/qualification-before-final-v1.json); corrected-adapter pins in [qualification-before-final-v2.json](../../evidence/04A23/qualification-before-final-v2.json). Both complete final-qualified-Debug/Release matrices rebuilt/retested that adapter and strengthened Python successor assertions. Successful rank-deficient, tied-covariance and two-variable fixtures plus actual Core variable-axis emission were then added; **final-complete-Debug/Release** rebuilt and ran every suite on the delivered test/production bytes. Only that last pair qualifies final counts. Raw earlier full/focused receipts remain retained; none is double-counted.

[final-source-qualification.json](../../evidence/04A23/final-source-qualification.json) pins final compiled/test/Help inputs before both complete matrices and verifies they remain identical afterward. Subsequent changes are evidence/dossier/accounting only, followed by final new-contract/publication tests on the final candidate set. No compiled source qualification gap remains.

## 8. Authority, preservation, privacy and publication

| Original population | Controls | Linked OPEN obligations | Restricted original controls |
|---|---:|---:|---:|
| Ui22 | 182 | 320 | 154 |
| Ui21 | 208 | 226 | 115 |
| Broader original migration union | 2,056 | 1,514 individual | 802 |

[original-control-crosswalk.json](../../evidence/04A23/original-control-crosswalk.json) rederives exact control/obligation ID sets, original row hashes, native-case intersections and source-linked renderer overlap. The two selected form sets have 390 control IDs and 546 linked obligations with no mutual obligation intersection in this ledger; broader native/PCA/case sets overlap and are never added as disjoint populations. Original 239 transferred obligations/218 blocked controls and PCA restrictions remain frozen authorities. Extraction releases none.

All 907 frozen historical authority files, 100 original mixed-model captures, accepted 04A.14–22 dossiers/evidence/corpora, Core numerics, Ui21/Common21–22/shared renderer, Designers/resources, Ribbon/UDFs, Office UI, framework/package/build settings and unrelated source retain entry bytes. [source-preservation.json](../../evidence/04A23/source-preservation.json) and [frozen-capture-hashes.json](../../evidence/04A23/frozen-capture-hashes.json) provide actual SHA checks. [historical-manifest-observations.json](../../evidence/04A23/historical-manifest-observations.json) distinguishes preexisting export CRLF/LF or accepted-successor manifest differences from this batch. No old capture, screenshot, xlsx or CSV was regenerated/promoted.

[retention.json](../../evidence/04A23/retention.json) verifies all 241,880 initial root TestResults files and 139 nested files by membership, size, creation/write metadata and file identity; zero mismatches. This is not falsely described as hashing every historical result byte. Selected frozen scientific/authority/source payloads are separately SHA-256 checked. New logs, preimages, drafts, assembly copies and builds use only ignored batch-qualified TestResults paths; no historical cleanup/relocation/compression/overwrite occurred.

The manifest contains exactly the actual new/modified batch candidates, role/size/SHA/mode and Git ignore/trackability outcomes. Only the manifest excludes its own content hash; membership, privacy and trackability are checked separately. No circular hash claim. [privacy.json](../../evidence/04A23/privacy.json) records bounded scanning of actual candidate bytes/decoded JSON/base64, known owner/host tokens, drive/profile/UNC/SID/credential patterns, with cache reuse only for identical source/scanner/artifact hashes. Public logs use <PYTHON_EXE>/<REPO_ROOT>/tool aliases; raw private receipts remain local. Pattern scanning is not proof of universal absence of secrets.

`git diff --check` passes; staged index is empty. [final-git-state.json](../../evidence/04A23/final-git-state.json) records exact changed/untracked candidate paths and the separately protected owner plan; ignored paths have a private local receipt/hash. New evidence/publication checks pass on the final candidate set. No Git publication has occurred.

## 9. Outstanding boundaries, Help and future native plan

C21-STACKED-ROWCOUNT-VALIDATION-01 remains **OPEN / CANDIDATE_ONLY / UNAPPLIED**. C09/C11/C12 native corrections, C13 earliest-time log-rank, C14 finite diagnostics, C17/C18 unlabelled CA and dormant patch, C19 row-ID/Minkowski/inversion-cut, C20 FA/DA provenance/prior/fold and all accepted 21/22 candidates remain under their original future owners. No new C23 statistical/presentation discrepancy required activation; [candidate-issues.json](../../evidence/04A23/candidate-issues.json) records zero behavior fixes. Help's lower-checkbox wording was corrected to existing UI applicability, not by changing the control or Core lower-limit behavior.

All family-specific report/warning/chart-data specialization and deeper plot payloads remain for **04A.24**. Native chart axes, zones, scale bounds, pixel positions, colors, legend, shape/series APIs, COM lifetime/rollback and visual equality remain under existing Windows/04A.22 ownership and later 04A.34/36 and native 45–48 gates. 04A.28 reconciliation and 04A.39–42 scientific/GUI/UDF corrections are untouched. No hidden correction, new GUI method or Office/Mac dialog was introduced.

Only `multivariate-control-charts.md` changed: subgroup Trim/Ordinal first-seen identity and repeated nonadjacent membership; labels/sequence/source-row separation; signal/exclusion/stage-boundary semantics; existing lower-checkbox applicability; unavailable Office.js/WASM/Mac multivariate dialog. Authored methods.yml, resolver metadata, links/version logic, numerical examples, screenshots, xlsx and CSV remain unchanged. Strict MkDocs on final page bytes passes; [documentation-audit.json](../../evidence/04A23/documentation-audit.json).

**NOT_RUN / UNAUTHORIZED:** native Excel/COM chart execution, real RefEdit/UIA, Office/Mac/browser manual runs, independent R, scientific golden promotion, screenshot/workbook capture, installer/signing/deployment. Source tests, compiled-call inspection and mock-free pure helpers are not native chart execution. Future smoke proposals only, requiring separate original prechange/build attribution and owner/native authority:

1. On the public 150-row individual MEWMA fixture: columns Temperature_C/Pressure_kPa/Flow_L_min, header row1, physical rows2–151, labels Obs-001..150, sequence1..150; Baseline1–100/I and Monitoring101–150/II, both-scope exclusions36–40, lambda .2, automatic pointwise UCL, stage/phase reset on and signal reset off. Compare exact selected sheets, original warning, markers/labels, stage line at 100/150 and chart-off/warning/cleanup behavior with attributed prechange native evidence.
2. Grouped Hotelling or GV: G01..G30, source rows preserved in groups of5, stages1–20/I and21–30/II, subgroup8 excluded with calibration reason; inspect first-seen grouped axis, diagnostic all/signalled scope, stage line at20/30, seven toggles and native selection/cancel/rollback. Confirm GV covariance-only history and temporary pseudo checkbox restoration separately. No such smoke was executed here.

## 10. Proposed formal owner decision

Accept **official 04A.23 portable multivariate SPC request and bounded result-context software extraction only**, with its real Ui22 consumers, exact compatibility evidence and disclosed inherited nonpasses. Retain P NO / N1 NO GO / N2 NO / C2 UNMET, every original OPEN obligation/restriction, all dormant candidates and future scientific/native/04A.24 responsibilities. This proposal does not grant native execution/release, independent scientific validation, golden promotion, Git publication or commencement of 04A.24. The owner decides acceptance and publication separately.
