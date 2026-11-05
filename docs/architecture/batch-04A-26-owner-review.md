# Official 04A.26 owner review

## 1. Decision

`READY_FOR_OWNER_04A26_ACCEPTANCE_REVIEW`

Software extraction is ready for scoped owner review. This decision does not approve statistical/native gates, Git publication or another batch.

All eight sizing families and five target agreement handlers consume shipping Common policies. Core formulas, rounding, CI algorithms and registered UDF contracts are unchanged. Native Excel/COM/RefEdit/UIA, Office/Mac/browser and independent R were **NOT_RUN / UNAUTHORIZED**. No independent numerical golden or chart appearance is claimed.

## 2. Actual Git authority and owner edits

Verified merged `main`, feature HEAD and entry/base: `97d54617703287f20886d82bc2896bd95e475770`. Branch: `codex/04A-26`. Local main reflog records `97d5461 merge codex/04A-25: Fast-forward`; the accepted 04A.25 branch is an ancestor at that commit. The owner reports formal acceptance; Git corroborates merge/ancestry and the accepted dossier, rather than independently proving a human decision. The pre-merge archive and former `bd46904...` entry are not substituted for current main.

Entry worktree/index were empty. During this run the owner revised the Master Plan from 2.9 to 2.10. That existing concurrent edit is preserved, hashed separately and **excluded from publication candidates**; no agent plan write occurred. Revision 2.10 retains the exact 04A.26 row and documents 04A.26 as in progress. Initial/final status, ancestry, all source pins and the excluded edit hashes are in `evidence/04A26/entry-state.json`, `entry-source-qualification.json` and `final-source-qualification.json`.

The staged index remains empty. Nothing was staged, committed, merged, pushed, reset, rebased, stashed or cleaned. The only Git mutation was creation of the authorized feature branch, after the sandbox denied writing its ref and explicit tool approval allowed that operation. No historical file was repaired to a former archive hash.

## 3. Shipping ownership and changed symbols

| Windows source | Changed existing members (normalized member-body audit) |
|---|---|
| `BESHstatNG/src/UI/Ui12SampleSizeTtestSingleProp.vb` | RunLogRank, RunCox, RunICC, RunBlandAltman, RunIndependentProp, RunSingleProp, RunPTtest, RunUPTtest |
| `BESHstatNG/src/UI/UiTwoInputRefedits.vb` | getData, getPairedCategoricalData, RunCohensKappa, RunLinsCCC, RunDeming |
| `BESHstatNG/src/UI/Ui9ANOVA2nested.vb` | getDataPB, RunBlandAltman |
| `BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb` | getDataICC11, RunICC |

The Ui12 constructor also consumes portable paired-t/single-proportion numeric defaults; native layout helpers and their toggling chronology remain unchanged. The test project adds six linked Common files and one adapter file; original ProjectReference XML is unchanged. Entry BOM/LF/terminal-newline conventions were restored on modified files, with qualified Windows rebuild/retest and strict MkDocs on final Help bytes; `encoding-audit.json` records exact counts. No historical tree normalization occurred. Eight new portable files under `BESHStat.Excel.Common/src/Analysis/` are `SampleSizePlanningRequest`, `SampleSizePlanningPolicy`, `SampleSizeTextProjection`, `SampleSizePlanningDescriptions`, `SampleSizePlanningDefaults`, `AgreementInputAxis`, `AgreementOptionSelections` and `AgreementReportPlan`. All use `Option Strict On` and contain no Excel/WinForms/COM/filesystem/Microsoft.VisualBasic dependency.

Actual call graph:

```text
Ui12.btCompute_Click (original title dispatch and CheckData)
  -> RunPTtest / RunUPTtest / RunSingleProp / RunIndependentProp
     / RunLogRank / RunCox / RunICC / RunBlandAltman
  -> Windows parsing and selected-branch SampleSizePlanningRequest
  -> Common SampleSizePlanningPolicy.Calculate (one selected Core call)
  -> original Core SampleSizeCalculator.Calculate... API
  -> Common SampleSizeTextProjection -> immutable report.Text
  -> original tbOutput.AppendText
  -> original btnSaveToSheet_Click: current Lines, new sheet, title first

UiTwo.getData/getPairedCategoricalData (original import/conversion)
  -> detached source-row/category axis
UiTwo.RunDeming / RunLinsCCC / RunCohensKappa
  -> Common option snapshots and original CI/variance execution intent
  -> existing Core constructor, original selected Fit/analytical/jackknife call
  -> original wrapResults -> Common AgreementReportPlan -> fresh table list
  -> original GetResultWriter / ProcessListofResultTables / writeToSheet
  -> original native plot only for Deming/Lin

Ui9.RunBlandAltman
  -> original ProjectBland / Common BlandRequestSelection / original ValidateBland
  -> converted observation/subject/source axis -> original Core Fit once
  -> original optional bias/LoA assessments and native table builders
  -> Common decision-table order / report span / plot flag
  -> original Excel writer and Bland plot

Ui0.RunICC
  -> original import branch / accepted paired block evidence
  -> Common IccRequestIntent + AgreementInputAxis
  -> original six ICC... and conditional repeatability methods, same order
  -> original wrapResults label -> Common report span -> original writer
```

Source-row metadata is stored in form-owned weak tables against imported DTOs; no Core result or importer schema changes. Unknown physical rows remain nullable, rather than manufactured ordinals. Subject keys, categorical values, physical rows and observation positions remain separate. `SpreadsheetRowAlignment` and accepted paired blocks are reused; numeric/categorical conversion, missing filtering and first-row interpretation stay native. ICC one-way jagged measurement arrays and two-way matrices retain original shapes and column ordering.

Deliberately host-owned/untransferred portions are control visibility/title/help dispatch, culture/Excel conversion, messages/focus and rejection timing, RefEdit/SD acquisition, decision-limit table formatting, overwrite/area/destination checks, native progress/errors, actual text append/save and plot geometry. These are unchanged boundary responsibilities, not a claim of portable GUI/Excel execution. Agreement engines stay in Core; Common does not reproduce estimators or tables.

## 4. Before/after behavior matrix

| Route/intent | Original behavior retained by Common/native composition |
|---|---|
| Paired t | Entered difference/SD/alpha/beta; original number of pairs and text order; no new rounding. |
| Unpaired t | Superiority, NI and EQ stay separate Core APIs; absolute margin converted to original signed bounds; control/experimental allocations, bound outputs and driving bound unchanged. |
| Single proportion | Entered alternative/null proportions, corrected/uncorrected sizing and original report labels remain. |
| Independent proportions | Three hypotheses, group order, kappa, signed margins, correction/Fisher/chi-square text preserved. Constructor versus toggled default orientation retained and recorded as a dormant candidate. |
| Log-rank | HR, group event proportions, allocation, alpha/beta and two-sided=True unchanged; C13 scientific issue remains unapplied. |
| Cox | Binary versus continuous Core calls; optional overall event proportion NaN versus explicit zero, R-squared absent=0; original “not provided”/“not estimated”, log HR/events/effective variance and subject estimates retained. |
| ICC planning | Null/alternative, one-sided alpha, original integer guard then CInt, subject count/observations/achieved power preserved. Nonfinite count issue characterized, not fixed. |
| Bland–Altman planning | SD, desired CI half-width, LoA multiplier and alpha; hidden beta remains NaN; original achieved precision/count preserved. |
| All sizing reports | Same current-culture interpolation, punctuation, field order and explicit CRLF; byte-exact original text derivative checked in invariant/sk-SK. Append and one-column new-sheet save methods remain unchanged. |
| Deming | GUI index1=constant CV, index2=pointwise SD, default=constant lambda (Core enum ordering differs). Original SD imports/messages/length validation and fit-intercept retained. Percentile > BCa > analytical > jackknife; MCR wins within analytical when Linnet checked. Existing analytical/MCR/jackknife/Fit invocation retained once. |
| Lin CCC | Analytical > percentile > BCa > analytical fallback; null concordance/alpha/replicates and conditional native reporter preserved; plot requested. |
| Kappa | Five GUI weights and original analytical > jackknife > percentile > BCa > analytical fallback; text/Unicode/blank categories and category order retained; resampling reporter retained; no plot. |
| ICC estimation | All ICC(1,1), ICC(1,k), ICC(2,1), ICC(2,k), ICC(3,1), ICC(3,k) branches; first-checked precedence, six original methods, repeatability order and exact k label interpolation from measurement columns retained. No plot. |
| Bland–Altman agreement | Original two/three-column conversion, subject identity, Auto/repeated/simple, raw/percent/log scale, X-axis/plot choice, proportional bias, original validation/fallback and <=0 replicates→2000 preserved. Bias table precedes LoA table only when enabled. |
| Output layout | Deming/Lin/Kappa/ICC `tableRows + sections - 1`; Bland `tableRows + sections + sections - 1`. Fresh lists preserve original Core table/footnote order. All writer destinations, occupied-area prompts and Excel lifetime remain native. |

`SampleSizePlanningDefaults` describes fresh numeric defaults without changing disabled/retained control text. Dynamic layouts remain native to preserve event side effects. Alpha/beta/hypothesis roles are detached; beta is not reinterpreted as power. No new validation is activated: Common diagnostics are candidate-only. Core exceptions and original GUI guards remain authoritative at their former boundaries.

Live assurance search found **no separate assurance solver or GUI**. ICC achieved power and BA achieved LoA half-width are result metadata under entered assumptions; no assurance algorithm, guarantee or public solver was invented.

## 5. Same-Core compatibility and reference limits

Corpus: **1152 executed records / 576 distinct content-addressed payloads**. Classifications: `{'POSITIVE_PARITY': 696, 'PRESERVED_CORE_REJECTION': 456}`; family execution counts: `{'planning': 832, 'agreement': 320}`. Every record is `NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY`. Profile/test hashes:

- `Planning26Tests`: `3297a5ad5adb1246bc0e4b9748831923a4fd014de29adbf033eded208f7c96eb`
- `Planning26LegacyCore`: `5cc4b146044136e297c2e3c14897447842bf28fc38876870a6b838cda96450be`
- `Planning26LegacyText`: `afa5ac6b99e0539ad93ed6f417e6c088c78ff45754fa28c4d3d71bae2f3dd26c`
- `Agreement26Tests`: `760c14d8952442df169fcce228dc9076a38453daf70699e2835bfb49ff3a60c1`
- `Agreement26Legacy`: `58cc74bf1d1521c1e8815fbb30721d162552f347764f9855eeda78dcdad08a5c`
- `Agreement26Reference`: `1a7ad5fbf8759a31360ef3b8a95d2bb59444fab66071f031686c7c5b790a76a9`

The 208 sizing comparisons per runtime cover thirteen branches × eight parameter/rejection profiles × two cultures. Eighty agreement comparisons per runtime cover five methods × eight profiles × two cultures. Original option/dispatch/text derivatives are compared with the new projection using the same unchanged Core APIs, exact typed result hashes, exact paragraphs and result-table content/order. Agreement payloads include full typed input/subject/source axes, effective options, computed result values and existing ToOutputModel cells, including nonfinite IEEE representations. ICC uses its actual five-subject, three-measurement fixture axis. Invalid Core cases compare exception class/message and remain rejections. There is no fabricated effect or zero-result success.

Existing seed parameters use 12345 in tests only. Native Core default `Integer.MinValue` and bootstrap behavior are unchanged; no unsupported cross-run CI equivalence is inferred. Fixture comparisons are synthetic and bounded, not independent R/native goldens. Full historical Core and plot-payload tests supplement new cases. Output appearance, placement, real worksheet coercion and clinical/scientific validity still require the original owner-native/reference gates.

Payloads and all per-case configuration/runtime/culture/options/source-row/subject/result metadata are indexed in `sample-agreement-same-core-v1.json` and its SHA buckets. Source-only GUI guard/append/save/SD-range characterizations are not mislabeled as native executions. Historic PSM **3840 records / 1920 payloads** and **100 original mixed-model captures** remain hash-verified and unchanged.

## 6. Entry versus final validation and nonpasses

| Configuration | Entry logical reference | Final logical managed | Final raw managed | Node | Executed Python |
|---|---:|---:|---:|---:|---:|
| Debug | 11,369/11,382 (reused) | 11,955/11,968 | 19,505/19,519 | 916/916 | 824/930 (96 FAIL / 10 ERROR) |
| Release | 11,369/11,382 (reused) | 11,955/11,968 | 19,505/19,519 | 916/916 | 824/930 (96 FAIL / 10 ERROR) |

Entry managed/Node identities are **NOT_RUN_REUSED_EVIDENCE**, qualified against all 801 actual-main compiled/test input members, 799 exact SHA and two demonstrated LF/CRLF-only PSM source differences, with accepted raw TRX and Node receipt hashes verified. Fresh main focused builds/tests also ran. Full Debug entry Python was freshly measured **807/910 (93 FAIL / 10 ERROR)**; a separate Release entry Python execution is **NOT_RUN**, not an asserted second pass. This explicit entry-reference qualification minimizes unnecessary baseline whole-suite reruns and does not omit either final configuration.

New unique logical managed: **586**, included in final totals (linked Common source counted once logically). New Python: **20**, all pass in both final configurations. Every final Python method was executed for each configuration; **zero final verdicts reused**. Raw framework executions, logical shared identities and separate semantic-only probe repetitions are not added together. Complete identities, traces and private receipt SHA references are in `entry-final-test-identities.json`, all final logical/raw identity files, Python trace files and `nonpasses.json`.

New managed nonpasses against entry:

None.

New Python nonpasses against freshly measured entry:

- `Debug: test_04a23_multivariate_spc.Spc23Contracts.test_actual_merged_base_and_protected_owner_plan` — FAIL
- `Debug: test_04a24_multivariate_payload.MultivariatePayload24.test_frozen_evidence_authority_hashes_and_unchanged_metadata` — FAIL
- `Debug: test_04a25_psm.Psm25SourceContracts.test_core_designer_udf_and_owner_plan_remain_exact_entry_bytes` — FAIL
- `Release: test_04a23_multivariate_spc.Spc23Contracts.test_actual_merged_base_and_protected_owner_plan` — FAIL
- `Release: test_04a24_multivariate_payload.MultivariatePayload24.test_frozen_evidence_authority_hashes_and_unchanged_metadata` — FAIL
- `Release: test_04a25_psm.Psm25SourceContracts.test_core_designer_udf_and_owner_plan_remain_exact_entry_bytes` — FAIL

Frozen prior tests are unchanged. Literal-source tests may now fail because an authorized wrapper moved, or because the owner plan/hash/publication authority advanced; these remain **real failed tests**. New IL adapter tests prove actual shipping Common request/option/axis/report calls; new Python successors prove preserved validation/save/import/native boundary member hashes. No failed source pin is rewritten green. Earlier nonpasses remain visible; this is not an all-green suite.

The measured Debug additions are three protected owner-plan hash assertions in 04A.23, 04A.24 and 04A.25 tests, triggered by the owner's concurrent Revision 2.10 update. They are retained as failures, with the owner edit excluded from publication. No new managed failure was introduced by the extraction; no extraction source-pin failure is invented to explain a historical nonpass.

Debug managed raw nonpass identities and assertion excerpts (Release exact identities are independently retained in JSON):

| Identity | Actual assertion excerpt |
|---|---|
| `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,30,50)` | Assert.IsFalse failed. No silent choice between conflicting scientific mappings |
| `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (0,0,0)` | Assert.IsFalse failed. No silent choice between conflicting scientific mappings |
| `ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted` | Assert.ThrowsException failed. Expected exception type:<System.OverflowException> but no exception was thrown.  |
| `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,15,40)` | Assert.IsFalse failed. No silent choice between conflicting scientific mappings |
| `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings (True)` | Assert.ThrowsException failed. Expected exception type:<System.NullReferenceException> but no exception was thrown.  |
| `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (1)` | Assert.IsTrue failed.  |
| `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (5)` | Assert.IsTrue failed.  |
| `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (2)` | Assert.IsTrue failed.  |
| `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings (True)` | Assert.ThrowsException failed. No exception thrown. NullReferenceException exception was expected.  |
| `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (4)` | Assert.IsTrue failed.  |
| `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (0)` | Assert.IsTrue failed.  |
| `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden` | Assert.AreEqual failed. Expected:<6.49310735531625E-08>. Actual:<0.499559833202511>.  |
| `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (3)` | Assert.IsTrue failed.  |
| `Spc21AdapterTests.NativeHostCoercionStateFitOutputAndChartTailStayEntryBytes (WriteResultsToNewWorkbook)` | Assert.AreEqual failed. Expected:<0e623c8c0c8c0907e100525e689126647c0f66484f05586561b9cd4b9e0b876c>. Actual:<2b648024ad85103748a6d657ba47025c08ed401dba1e8bc719e8989d9979519f>.  |

Debug Python method identities (individual FAIL/ERROR traces and corresponding Release identities in the machine-readable artifacts):

| Identity | Outcome |
|---|---|
| `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook` | FAIL |
| `test_04a02_checkpoint11.Namespace11Tests.test_exact_retained_bytes_reproduce_only_compiler_rejection` | FAIL |
| `test_04a02_checkpoint11.Provenance11Tests.test_both_authorized_case_pins_pass_and_frozen_false_is_not_owner_decision` | ERROR |
| `test_04a02_checkpoint12.Diagnosis12Tests.test_both_authorized_case_pins_keep_original_compiler_and_scientific_policy` | ERROR |
| `test_04a02_checkpoint12.Diagnosis12Tests.test_exact_owner_bytes_negative_for_frozen_v5` | FAIL |
| `test_04a02_checkpoint13.Startup13Tests.test_corrupt_receipt_hash_fails_even_with_exit_zero` | FAIL |
| `test_04a02_checkpoint13.Startup13Tests.test_real_successful_owner_startup_all_pins_and_native_identities` | ERROR |
| `test_04a02_checkpoint13.Startup13Tests.test_tampered_stdout_hash_stops_before_parser` | FAIL |
| `test_04a02_checkpoint14.Actual14Tests.test_actual_two_owner_publications_and_both_cultures_reproduce` | ERROR |
| `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings` | ERROR |
| `test_04a03_control_mitigation.ControlMitigationSourceTests.test_per_mode_consumers_and_branch_order` | FAIL |
| `test_04a03_control_mitigation.ControlMitigationSourceTests.test_sealed_sources_are_reconciled_never_rewritten` | FAIL |
| `test_04a03_control_mitigation.NativeGateNegativeTests.test_review_packet_source_pins_match_current_sources` | FAIL |
| `test_04a03_selection.UnivariateSelectionSourceTests.test_all_native_lifetime_cancel_write_and_reporting_methods_unchanged` | FAIL |
| `test_04a03_selection.UnivariateSelectionSourceTests.test_only_two_form_methods_change` | FAIL |
| `test_04a04_alignment.PairedBlockSourceGates.test_exact_crosswalk_and_authorities` | FAIL |
| `test_04a04_alignment.PairedBlockSourceGates.test_legacy_geometry_ambiguity_is_not_hidden` | FAIL |
| `test_04a05_completion_successor.Completion05SuccessorTests.test_independent_candidate_provenance_not_core_or_golden` | FAIL |
| `test_04a05_completion_successor.Completion05SuccessorTests.test_nonpaired_code_and_unrelated_forms_are_unchanged_from_partial` | FAIL |
| `test_04a05_completion_successor.Completion05SuccessorTests.test_owner_bound_successor_and_source_hashes` | FAIL |
| `test_04a05_contingency.ContingencySourceGates.test_historical_ledgers_and_importer_unchanged` | FAIL |
| `test_04a05_contingency.ContingencySourceGates.test_other_multicol_members_including_04a04_selection_unchanged` | FAIL |
| `test_04a05_contingency.ContingencySourceGates.test_prior_04a04_whole_file_source_check_remains_frozen_and_disclosed` | FAIL |
| `test_04a05_contingency.ContingencySourceGates.test_scientific_ambiguity_is_not_a_permanent_portable_wrong_table` | FAIL |
| `test_04a05_contingency.ContingencySourceGates.test_ui82x2_source_wiring_explicitly_withheld` | FAIL |
| `test_04a05_contingency.ContingencySourceGates.test_versioned_successor_sources_and_case_boundaries` | FAIL |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_actual_form_routes_and_unchanged_native_tails` | FAIL |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_importer_udf_designers_core_and_old_tests_unchanged` | FAIL |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_native_population_is_union_not_sum` | FAIL |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_new_scientific_candidates_are_explicitly_unmodified` | FAIL |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_no_duplicate_statistics_in_form_or_windows_adapter` | FAIL |
| `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_production_sources_and_exact_open_ledger_lineage` | FAIL |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_actual_four_shipping_routes_and_native_bodies` | FAIL |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_core_old_tests_importer_udfs_designers_and_help_unchanged` | FAIL |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_exact_source_and_original_ledger_rows` | FAIL |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_no_fabricated_obligation_for_previously_source_qualified_controls` | FAIL |
| `test_04a07_workflow_successor.Anova07SuccessorTests.test_restriction_unions_and_timing_exceptions_not_generalized` | FAIL |
| `test_04a08_authoring_successor.Authoring08SuccessorTests.test_exact_id_crosswalk_retains_original_row_hashes_and_open_counts` | FAIL |
| `test_04a08_authoring_successor.Authoring08SuccessorTests.test_original_numerics_and_shipping_forms_are_unchanged` | FAIL |
| `test_04a09_regression_successor.Regression09SuccessorTests.test_exact_six_changed_members_and_unchanged_native_surroundings` | FAIL |
| `test_04a09_regression_successor.Regression09SuccessorTests.test_frozen_tests_numerics_and_public_04a08_evidence_preserved` | FAIL |
| `test_04a09_regression_successor.Regression09SuccessorTests.test_real_six_routes_use_common_options_counts_and_reports` | FAIL |
| `test_04a09_regression_successor.Regression09SuccessorTests.test_runtime_baselines_are_incremental_and_complete` | FAIL |
| `test_04a10_regression_projection.Regression10ProjectionTests.test_baselines_are_actual_gaussian_and_binomial_and_not_goldens` | FAIL |
| `test_04a10_regression_projection.Regression10ProjectionTests.test_core_udfs_frameworks_and_accepted_evidence_are_byte_protected` | FAIL |
| `test_04a10_regression_projection.Regression10ProjectionTests.test_only_two_form_routes_change_and_native_tails_match` | FAIL |
| `test_04a11_publication.Publication11Tests.test_accepted_dossier_and_dormant_patches_unchanged` | FAIL |
| `test_04a11_publication.Publication11Tests.test_all_manifests_and_implementation_freeze` | FAIL |
| `test_04a11_publication.Publication11Tests.test_raw_local_source_pins_and_native_writer_tails` | FAIL |
| `test_04a11_regression_families.Regression11SuccessorTests.test_exact_six_form_deltas_and_native_writer_tails` | FAIL |
| `test_04a11_regression_families.Regression11SuccessorTests.test_unaffected_sources_and_all_old_evidence_are_byte_protected` | FAIL |
| `test_04a12_gee_workflow.Gee12WorkflowSuccessorTests.test_exact_source_scope_and_all_old_byte_pins` | FAIL |
| `test_04a13_survival_workflow.Survival13SourceContracts.test_all_prechange_protected_bytes_except_exact_approved_scope` | FAIL |
| `test_04a13_survival_workflow.Survival13SourceContracts.test_local_main_ancestry_empty_index_and_dedicated_branch` | FAIL |
| `test_04a13_survival_workflow.Survival13SourceContracts.test_successor_pins_and_members_reconcile_to_actual_forms` | FAIL |
| `test_04a14_mixed_workflow.Mixed14SourceContracts.test_branch_base_ancestry_and_empty_index` | FAIL |
| `test_04a14_mixed_workflow.Mixed14SourceContracts.test_exact_scope_and_external_owner_plan_are_separate` | FAIL |
| `test_04a14_mixed_workflow.Mixed14SourceContracts.test_original_members_and_successor_pins_are_reconcilable` | FAIL |
| `test_04a14_mixed_workflow.Mixed14SourceContracts.test_reports_are_consumed_by_actual_writers_and_append_seams` | FAIL |
| `test_04a15_fitting.Mixed15SourceContracts.test_all_protected_sources_and_new_source_hashes` | FAIL |
| `test_04a15_fitting.Mixed15SourceContracts.test_branch_ancestry_empty_index_and_exact_existing_scope` | FAIL |
| `test_04a15_fitting.Mixed15SourceContracts.test_only_actual_shipping_factory_routes_change` | FAIL |
| `test_04a16_post_estimation.Mixed16SourceContracts.test_branch_base_index_and_exact_scope` | FAIL |
| `test_04a16_post_estimation.Mixed16SourceContracts.test_unchanged_methods_designers_udfs_and_core` | FAIL |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_acquisition_error_order_and_legacy_grammars_preserved` | ERROR |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_actual_six_handlers_delegate_and_keep_one_calculation` | FAIL |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_live_base_branch_index_and_protected_sources` | FAIL |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_native_writer_statements_and_spm_dispatch_stay_identical` | FAIL |
| `test_04a17_multivariate.Multivariate17SourceContracts.test_public_targets_and_successor_hashes` | FAIL |
| `test_04a18_projection.Projection18SourceContracts.test_live_branch_index_and_protected_entry_bytes` | FAIL |
| `test_04a18_projection.Projection18SourceContracts.test_only_three_form_methods_change_and_spm_remains_exact` | FAIL |
| `test_04a19_clustering.Clustering19Contracts.test_all_other_form_methods_and_import_catches_preserved` | FAIL |
| `test_04a19_clustering.Clustering19Contracts.test_core_native_writer_and_prior_test_bytes_unchanged` | FAIL |
| `test_04a19_clustering.Clustering19Contracts.test_live_base_branch_and_empty_index` | FAIL |
| `test_04a20_factor_discriminant.FactorDiscriminant20Contracts.test_core_native_prior_tests_targets_and_metadata_are_entry_bytes` | FAIL |
| `test_04a20_factor_discriminant.FactorDiscriminant20Contracts.test_live_main_branch_index_and_accepted_source` | FAIL |
| `test_04a21_spc.Spc21Contracts.test_core_ui22_native_designers_prior_tests_and_targets_unchanged` | FAIL |
| `test_04a21_spc.Spc21Contracts.test_live_merged_main_ancestry_branch_and_empty_index` | FAIL |
| `test_04a21_spc.Spc21Contracts.test_single_fit_cancellation_and_original_native_output_tail` | FAIL |
| `test_04a22_spc.Spc22Contracts.test_core_ui22_udfs_designers_old_tests_targets_and_frozen_evidence` | FAIL |
| `test_04a22_spc.Spc22Contracts.test_publication_membership_hashes_empty_index_and_privacy` | FAIL |
| `test_04a23_multivariate_spc.Spc23Contracts.test_actual_merged_base_and_protected_owner_plan` | FAIL |
| `test_04a23_multivariate_spc.Spc23Contracts.test_actual_publication_membership_hash_privacy_trackability_and_empty_index` | FAIL |
| `test_04a23_multivariate_spc.Spc23Contracts.test_existing_tables_and_warnings_remain_byte_exact_methods` | FAIL |
| `test_04a23_multivariate_spc.Spc23Contracts.test_native_workbook_order_warning_and_appearance_are_exact` | FAIL |
| `test_04a23_multivariate_spc.Spc23Contracts.test_result_context_is_consumed_and_diagnostic_table_tail_is_exact` | FAIL |
| `test_04a24_multivariate_payload.MultivariatePayload24.test_frozen_evidence_authority_hashes_and_unchanged_metadata` | FAIL |
| `test_04a24_multivariate_payload.MultivariatePayload24.test_original_numerical_ui21_renderer_and_prior_common_hashes` | FAIL |
| `test_04a24_multivariate_payload.MultivariatePayload24.test_publication_exact_membership_hashes_and_empty_index` | FAIL |
| `test_04a25_psm.Psm25SourceContracts.test_core_designer_udf_and_owner_plan_remain_exact_entry_bytes` | FAIL |
| `test_04a27_geometry_completion.Geometry27CompletionTests.test_compile_targets_and_native_helpers_unchanged` | FAIL |
| `test_04a27_geometry_completion.Geometry27CompletionTests.test_initial_implementation_and_historical_tests_not_rewritten` | FAIL |
| `test_04a27_geometry_completion.Geometry27CompletionTests.test_real_consumer_routes_and_udf_exports_preserved` | FAIL |
| `test_04a27_geometry_completion.Geometry27CompletionTests.test_source_owner_pins_and_all_44_original_rows` | FAIL |
| `test_04a27_transport_successor.Transport27SuccessorTests.test_core_designer_and_refedit_sources_are_unmodified` | FAIL |
| `test_04a27_transport_successor.Transport27SuccessorTests.test_exact_successor_and_unapplied_patch` | FAIL |
| `test_capture_contract04a02.CaptureContracts.test_all_239_ledger_rows_identified_with_original_digest_convention` | ERROR |
| `test_capture_contract04a02.CaptureContracts.test_complete_source_crosswalk_equality` | ERROR |
| `test_capture_contract04a02.CaptureContracts.test_packet_covers_all_original_cases_and_six_gates` | FAIL |
| `test_capture_contract04a02.CaptureContracts.test_packet_retains_additional_21_deferred_obligations` | ERROR |
| `test_investigation06.Investigation06Tests.test_all_239_exact_ids_have_reversible_root_cause_source_maps` | ERROR |
| `test_investigation06.Investigation06Tests.test_anova_defaults_mapping_remain_explicit_source_facts` | FAIL |
| `test_investigation06.Investigation06Tests.test_normality_5000_disagreement_preserved_as_review_defect` | FAIL |
| `test_investigation06.Investigation06Tests.test_pca_has_three_biplot_scales_not_the_old_two_scale_proposal` | FAIL |
| `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof` | FAIL |
| `test_investigation06.Investigation06Tests.test_two_by_two_or_condition_and_no_chart_handler` | FAIL |

## 7. Four-lane execution and timing

| Configuration | A Core/Common seconds | B Windows seconds | C Office/Node seconds | D Python/source seconds | Matrix wall seconds |
|---|---:|---:|---:|---:|---:|
| Debug | 145.89 | 215.92 | 62.54 | 401.69 | 401.69 |
| Release | 109.73 | 146.63 | 60.07 | 370.64 | 370.65 |

Final matrices total **772.34 s**; final builds **54.57 s**; serial strict/profile/MkDocs **47.38 s**; requalification **1089.13 s**. Entry-freeze through terminal audit measured **6213.96 s**, including development and setup/debug attempts; initial interactive authority/Git preflight and later dossier/manifest sealing are explicitly outside that interval. Final sealing timing is recorded separately. No measured sequential baseline exists and no speedup is claimed.

Lane D starts first. Each root uses copied prebuilt binaries, isolated TRX/logs/exports and project-relative TestData. No concurrent builds across configurations; no testhost Excel activation. MaxCpuCount=1, MSTest Workers=1, Node concurrency=1, at most four runner command roots; OS root PID/CPU/working-set samples are retained, not misrepresented as complete child-process memory measurements. All lanes are awaited despite inherited failures. No collision or historical write was found.

Python dependency/side-effect matrix records source hashes and conservative module classifications. Source/inventory validators remain unmodified and fully executed twice. Immutable completed TRX parsing/hash lookup is cached only in accounting with a terminal SHA reread. No Python method verdict, historical source scan or test inventory is skipped by caching. No fifth worker, unsafe discovery split or alternating-batch policy was activated.

Bounded, serial post-matrix profiling measures three representative historical source/corpus modules and two fresh immutable-registry hash scans; these profiling repetitions do not inflate final totals. Other modules have root-command timings only. Method identities, outcomes and scan byte counts/durations are in `python-module-profile.json`:

- `test_04a14_mixed_workflow`: 16 repeated methods, 39.92 s; 4 FAIL / 0 ERROR.
- `test_04a24_multivariate_payload`: 22 repeated methods, 3.16 s; 3 FAIL / 0 ERROR.
- `test_04a25_psm`: 20 repeated methods, 0.08 s; 1 FAIL / 0 ERROR.

The original Release matrix additionally recorded two privacy-test failures from a slash-separated prose phrase in the new dossier helper, matching the frozen home-path pattern. The prose was corrected without relaxing the scanner or frozen tests. Corpus review then strengthened two new fixture/reference files: valid 200-replicate positive profiles supplement genuine Core rejections, ICC provenance matches the five-subject matrix, and full typed numerical snapshots are emitted. Affected full Common/Windows assemblies and scripts discovery were requalified in both configurations. Entry encodings were restored and Windows Debug was separately rebuilt/retested. Initial totals, source hashes and receipts remain in `initial-full-matrix-validation.json`, `initial-profile-coverage.json` and `python-requalification.json`; every intermediate attempt remains private. Repetitions never inflate distinct final totals. Product semantics, Core and frozen tests did not change during this requalification.

Setup receipts are preserved: zero-test filter, import-before-Option, VB request shadowing, ProjectReference XML insertion, Unicode reference decoding and comparison-type hash mistakes were corrected before final build. Final source hashes equal both compiled build manifests. Evidence-only post-matrix updates require fresh audits/sealing, rather than rerunning unchanged managed binaries.

## 8. Original restrictions, retention and dormant candidates

| Original host | Control records | Linked OPEN obligations (overlapping) |
|---|---:|---:|
| `Ui12SampleSizeTtestSingleProp` | 20 | 1 |
| `UiTwoInputRefedits` | 77 | 13 |
| `Ui9ANOVA2nested` | 51 | 9 |
| `Ui0OneRefeditMulticol` | 39 | 7 |

Actual combined host population is **187 controls / 30 linked OPEN obligations**. These host-wide records include unrelated modes and overlap with native/source obligations. They are not added to overall original **3,866 source members / 2,056 controls / 1,514 obligations**, nor to 04A.27's **44 OPEN importer obligations**. Exact IDs/original hashes and 04A.02 blocked-control overlap are in `original-control-crosswalk.json` and `restriction-retention.json`.

Historical results: **270444 files**, exact membership and file identity/size/creation-write metadata retained. This is explicitly a metadata preservation check, not a claim of rereading every large TestResults byte. **1779 authority/corpus hashes** were separately verified, with **1778 entry-exact files** and only the excluded owner Master Plan revision differing. Previous 04A.25 registry reconciliation: `{'EXACT': 1713, 'ENTRY_OWNER_AUTHORITY_CHANGE': 1}`; differences are recorded without altering history. The original 100 mixed-model hashes and prior PSM bucket hashes are independently checked.

Dormant candidates, all **OPEN / CANDIDATE_ONLY / UNAPPLIED**:

- `C26-ICC-NONFINITE-COUNT-01`: Original GUI guard and CInt timing retained; no new blocking Common validation. Proposed later owner: Windows validation owner / later 04A.47. Activation: Separate native/behavior authorization.
- `C26-INDEPENDENT-PROP-DEFAULTS-01`: Existing control initialization/toggling untouched. Proposed later owner: Windows/UDF planning owners / 04A.39–04A.42 and 04A.47. Activation: Separate default/output approval.
- `C26-GUI-UDF-CONTRACT-01`: All UDF source/registrations unchanged; GUI branches retain original arguments. Proposed later owner: UDF/scientific owner / 04A.39–04A.42. Activation: Separate scientific/public-contract review.
- `C26-RESAMPLING-MINIMUM-01`: Core policies and original GUI spinner/exception boundaries retained; Common echoes entered replicates without new guard. Proposed later owner: Scientific/agreement and Windows owners / later 04A.39–42 and 04A.47. Activation: Separate scientific/default and native validation approval.

All prior C09/C11/C12 patches, C13 log-rank, C17–C25 issues, C21 stacked row validation and C24 historical oracle scope remain unchanged/unapplied. **Zero native controls released; zero obligations discharged. P=NO; N1=NO GO; N2=NO; Gate C2=UNMET.** Other E/GC/C3 restrictions remain under their original authority.

Privacy screening covers every publication candidate including decoded JSON/base64 and private owner, host, home, drive, SID and credential patterns. Full unredacted logs/TRX stay private under `TestResults/04A-26/run01`. Public paths use `<REPO_ROOT>`, `<PYTHON_EXE>` and tool aliases. `publication-manifest.json` seals every candidate SHA-256/size/mode except its explicitly omitted self-hash; the owner plan is separately accounted for. Trackability, empty index, diff-check and preservation are independently verified by the new publication helper. Exact terminal candidate/hash count is in that manifest and console receipt; no ignored history is forced into publication.

## 9. Help, gates and excluded execution

All eight sizing pages, Bland–Altman, Deming, ICC, Kappa and Lin-concordance pages plus authored `_meta/methods.yml` were audited. Only ICC/BA sizing pages gain one clarification each: achieved power/half-width is existing result metadata and there is no separate assurance solver. Authored metadata, links, screenshots and workbook assets remain unchanged; original assets are hash-listed in `documentation-audit.json`.

Actual serial exits: strict **1**, profile **1**, strict MkDocs **0**. Strict/profile nonzero results remain UNMET; raw stdout/stderr hashes and command identities are retained. MkDocs uses final authored Help bytes and isolated `--site-dir`. Exact installed versions and target frameworks are in `toolchain.json`.

No working Office.js/WASM sample-size/agreement panel is implemented or claimed. No native Excel/COM/RefEdit/UIA smoke, authenticated Office/Mac/browser run, workbook/chart capture, R oracle, installer/signing or release occurred. Representative future owner-native smoke remains paired sample-size (repeat append/save) and Bland–Altman (subject layout, optional decision tables and spacing/plot). These are deferred checks, not newly authorized runs. Core mathematics, chart geometry (later 04A.35), UDF contracts (later 04A.39–42), unrelated UI, accepted 04A.27 transport and frozen references are excluded.

## 10. Proposed scoped owner decision

Accept **04A.26 software extraction only**, subject to independent review of this dossier and manifest. Retain every inherited/protected nonpass, dormant correction and native/scientific gate. This proposed decision does **not** authorize Git staging/commit/merge/push, numerical-golden promotion, native-control release, obligation discharge or another numbered batch. Owner acceptance/publication and any later batch remain separate actions.
