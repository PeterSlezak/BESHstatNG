# Automated-test rationalization owner review

`READY_FOR_OWNER_TEST_RATIONALIZATION_REVIEW` — unnumbered maintenance, not a Phase 04A feature batch. The complete regression matrix still **FAILS** because inherited failures remain real. This decision concerns review of the bounded test-only change, not an all-green product, scientific/native acceptance or publication.

## Authorization and actual source

The owner explicitly confirmed, contemporaneously in this conversation, exclusive repository/shared-output writer access, no other Codex/agent/IDE/build/test/Git writers, formal 04A26 acceptance/commit/merge, and permission for safe test-only improvements. That owner decision controls over stale plan/dossier pending wording. Entry worktree/index were clean; HEAD and local main were both `c2ea685e893d5d1379a1bd8c13c390fc6a259f58`; accepted 04A26 ancestry was verified. No fetch or guessed newer SHA was used. Branch: `codex/test-suite-rationalization`, created from that verified main after explicit owner confirmation. The sandbox initially denied `.git/index.lock`; a narrowly scoped branch-creation escalation was approved. No global Git configuration was changed; Git commands use process-local safe.directory.

No staging, commit, merge, push, remote update, publication or 04A28 work occurred. The earlier off-repository audit is present, reused and unchanged: all 15 manifest hashes match at entry/final. The prior static inventory (2,877 managed source groups, 1,308 Python methods, 400 Node callsites) remains a static audit, not a runtime denominator. Its original dossier was not rewritten to change its historical authorization finding.

## Exact changes and preserved coverage

Only one existing source file changes: `BESHStatNG.Test/BESHStatNG.Tests.vbproj`. Its five local SPC Compile entries now link to the existing unchanged Core test bodies, with the original IDE Link paths. Shewhart 12, time-weighted 10, multivariate 9, rules 9, and result/dispatch 7 methods: **47 tests still execute in each runtime**. Both original and canonical VB files are retained byte-exact. Expected numbers, tolerances, seeds, cases, helpers and categories were not edited. PDB source paths now identify the canonical Core files; compiled class/method/data-row identities and project root namespaces are preserved.

The Windows SPC files are preserved historical source copies; active maintenance occurs in the five canonical Core files. This removes 47 duplicated maintained method bodies, **zero test executions**. The other 51 linked Common test/support files and their 235 declared groups / 523 DataRows remain unchanged. Ordinal, multinomial, SPC-model and Parametric near-copies remain separate because helper/fixture/numeric equivalence was not proved. No historical source-pin assertion, golden, reference or fixture was changed, skipped or xfailed.

```diff
diff --git a/BESHStatNG.Test/BESHStatNG.Tests.vbproj b/BESHStatNG.Test/BESHStatNG.Tests.vbproj
index 4e660c5..9778d48 100644
--- a/BESHStatNG.Test/BESHStatNG.Tests.vbproj
+++ b/BESHStatNG.Test/BESHStatNG.Tests.vbproj
@@ -258,12 +258,12 @@
     <Compile Include="src\StatFuncTests.vb" />
     <Compile Include="src\StatisticalProcessControl\SpcCapabilityTests.vb" />
     <Compile Include="src\StatisticalProcessControl\SpcModelsStatisticsEngineTests.vb" />
-    <Compile Include="src\StatisticalProcessControl\SpcMultivariateTests.vb" />
-    <Compile Include="src\StatisticalProcessControl\SpcResultTablesAndDispatchTests.vb" />
-    <Compile Include="src\StatisticalProcessControl\SpcRuleEvaluatorTests.vb" />
-    <Compile Include="src\StatisticalProcessControl\SpcShewhartChartTests.vb" />
+    <Compile Include="..\BESHStat.Core.Tests\src\StatisticalProcessControl\SpcMultivariateTests.vb"><Link>src\StatisticalProcessControl\SpcMultivariateTests.vb</Link></Compile>
+    <Compile Include="..\BESHStat.Core.Tests\src\StatisticalProcessControl\SpcResultTablesAndDispatchTests.vb"><Link>src\StatisticalProcessControl\SpcResultTablesAndDispatchTests.vb</Link></Compile>
+    <Compile Include="..\BESHStat.Core.Tests\src\StatisticalProcessControl\SpcRuleEvaluatorTests.vb"><Link>src\StatisticalProcessControl\SpcRuleEvaluatorTests.vb</Link></Compile>
+    <Compile Include="..\BESHStat.Core.Tests\src\StatisticalProcessControl\SpcShewhartChartTests.vb"><Link>src\StatisticalProcessControl\SpcShewhartChartTests.vb</Link></Compile>
     <Compile Include="src\StatisticalProcessControl\SpcTestFixtures.vb" />
-    <Compile Include="src\StatisticalProcessControl\SpcTimeWeightedChartTests.vb" />
+    <Compile Include="..\BESHStat.Core.Tests\src\StatisticalProcessControl\SpcTimeWeightedChartTests.vb"><Link>src\StatisticalProcessControl\SpcTimeWeightedChartTests.vb</Link></Compile>
     <Compile Include="src\UDFs\UdfBatch2Tests.vb" />
     <Compile Include="src\UDFs\UdfBatch3Tests.vb" />
     <Compile Include="src\UDFs\UdfImportFacadeTests.vb" />
```

New `scripts/test_suite_rationalization.py` provides an opt-in offline build-once runner: fresh create-new ignored `.test-rationalization-*.tmp` roots, no historical result writes; separate modern/Framework dependency containers; fixed four outer lanes and serial inner hosts; actual case records; zero-test rejection; preserved nonzero matrix/focused/discovery/serial status and refusal to qualify changed source inputs. Windows host compile-only targets avoid version/Help regeneration, XLL packaging and signing. The exact small protected managed ControlMitigation projection is hash-qualified and copied into a new build sandbox, never regenerated; it does not execute the shipping constructor or Excel/COM/UIA.

New `scripts/tests/test_test_suite_rationalization.py` adds **14 passing fault-focused Python tests**: receipt/directory collision refusal, path escape, runtime dependency contamination, missing/duplicated cases, changed runtime identity, outcome visibility, empty TRX, failed/skipped result preservation and full-matrix nonpass aggregation. These strengthen test infrastructure assertions. The audit did not prove an unprotected existing scientific test weak enough to safely rewrite; no speculative assertion/tolerance edit was made. Same-Core alignment is still NOT_GOLDEN.

## Fresh entry/final evidence

The entry matrices include the new runner and its same 14 tests, before changing any existing Compile entry. Every pre-existing test source remains main-exact. All original 930 Python methods were freshly executed alongside the 14 additions. No historical pass was substituted for an execution. Debug and Release each have fresh discovery and full executions. Results below exclude focused repetitions and the separate Common semantic-observation prerequisite.

| Matrix | Managed logical continuity pass/total | Raw managed pass/total | Node pass/total | Python method pass/total | Python nonpass method counts |
|---|---:|---:|---:|---:|---|
| before-Debug | 11955/11968 | 19505/19519 | 916/916 | 838/944 | {'ERROR': 10, 'FAIL': 96} |
| before-Release | 11955/11968 | 19505/19519 | 916/916 | 838/944 | {'ERROR': 10, 'FAIL': 96} |
| after-Debug | 11955/11968 | 19505/19519 | 916/916 | 838/944 | {'ERROR': 10, 'FAIL': 96} |
| after-Release | 11955/11968 | 19505/19519 | 916/916 | 838/944 | {'ERROR': 10, 'FAIL': 96} |

Managed raw: **19,519 each**, 19,505 PASS, 14 FAIL, zero SKIP/NOT_RUN. Node: **916 PASS**, zero FAIL/SKIP/NOT_RUN. Python: **944 methods each**, 838 pass, 96 FAIL identities and 10 ERROR identities, zero SKIP/NOT_RUN. The original subset remains 824/930; the additional 14 are runner tests. Unittest emits 100 failure events plus 10 error events because some already-failing methods have multiple subtest failures; events are not additional method identities. Exact parent identities were reconciled from verbose statuses and failure headers, with multiplicity preserved.

No managed/Node test ID was added, removed or altered by linking. Entry/final Python ID delta is zero; main-to-maintenance adds exactly 14. All original failure identities/outcomes remain, with **no new or removed nonpass identities** in either final configuration. Entry/final complete identity counters are in test-inventory-v1.json; original-vs-final failure rows and excerpts are in validation-v1.json.

Logical accounting needs an explicit distinction. The original accepted shared-source/null-cohort partition has **11,968 logical cases / 11,955 pass**; this partition is retained for before/after continuity and failure reporting. The current source-SHA canonical algorithm reports **11,921 / 11,908 pass** after linking, because 47 newly shared all-passing SPC bodies are now one source group across two runtimes. The baseline raw LF/CRLF source hashes differed. This is a disclosed source-group canonicalization, not loss of 47 executions or evidence of a faster suite. All 19,519 runtime executions and every null/empty weighted cohort remain. No failure is removed by changing a denominator.

## Measured timings and execution policy

Builds were sequential and separate from test workers; tests used four lanes: A Core/Common, B Windows/ControlMitigation, C Office/Execution/Node, D Python/source plus original semantic prerequisite. Inner MaxCpuCount/MSTest workers/Node concurrency is 1. `test.runsettings`, legacy settings and the original routine verifier remain unchanged. No accepted full cadence or default selection was relaxed; optional fast/standard/historical profiles were not activated. Python ran independently in both configurations; no REUSED_SOURCE_ONLY verdicts or historical cache was used.

| Matrix | A s | B s | C s | D s | Concurrent wall s |
|---|---:|---:|---:|---:|---:|
| before-Debug | 139.37 | 208.99 | 56.01 | 377.27 | 377.28 |
| before-Release | 112.21 | 158.26 | 55.12 | 338.42 | 338.42 |
| after-Debug | 132.79 | 202.51 | 55.55 | 339.63 | 339.63 |
| after-Release | 117.39 | 159.32 | 56.13 | 340.90 | 340.90 |

Matched focused test-body selections:

| Phase | Configuration | Runtime suite | Command wall s | Pass/total |
|---|---|---|---:|---:|
| before | Debug | Core | 1.683 | 47/47 |
| before | Debug | Legacy | 3.478 | 47/47 |
| before | Release | Core | 1.663 | 47/47 |
| before | Release | Legacy | 3.462 | 47/47 |
| after | Debug | Core | 1.896 | 47/47 |
| after | Debug | Legacy | 3.943 | 47/47 |
| after | Release | Core | 1.764 | 47/47 |
| after | Release | Legacy | 3.795 | 47/47 |

Source/test-body hashes, assembly hashes and command timings are retained. Runtime differences include cache/IO/host setup variation; these single full-matrix observations do not prove a causal speedup. **No runtime speedup or execution-count reduction is claimed.** CPU/peak process memory and assembly-load-only timings were not instrumented; framework per-case durations remain in the private TRX. The reusable build-once/isolation workflow avoids shared output contention without claiming a controlled serial-versus-parallel speedup.

Initial setup receipts remain separate: a first build pilot mixed modern adapter files into Framework output, and its zero-case discovery was correctly rejected. Production dependencies are now frozen before modern test adapters enter output; framework containers use MSTest 2 explicitly. The corrective contamination test passes. No pilot zero-test result is counted as a passing baseline. The fresh before/after matrices use the corrected same runner bytes.

After the four matched matrices, final review strengthened only CLI exit/status aggregation and its existing runner assertion. Test/container/command/build selection bodies and all compiled test/helper code remained unchanged. The complete affected `scripts/tests` discovery (733 methods) was then freshly rerun independently for Debug and Release on final runner bytes. These replace the corresponding earlier script-root verdicts in final Python accounting; they are not added as extra passes. Other executed roots and all compiled suites retain unchanged source identities. The new 14 runner tests pass in both requalified roots. Original strict/profile and strict documentation also execute on final runner bytes; serial CLI exits 1 correctly. Requalification timings are separate from the matched full-matrix measurements.

## Validation commands and truthful status

Private receipt root: `.test-rationalization-20261105-01.tmp` (ignored; never staged or copied as historical corpora). Commands executed from the project root, using installed offline tools:

```text
<PYTHON_EXE> -B -m unittest discover -s scripts/tests -p test_test_suite_rationalization.py -v
<PYTHON_EXE> -B scripts/test_suite_rationalization.py init --run .test-rationalization-20261105-01.tmp
<PYTHON_EXE> -B scripts/test_suite_rationalization.py build --run .test-rationalization-20261105-01.tmp --configuration <Debug|Release> --label <before|after>
<PYTHON_EXE> -B scripts/test_suite_rationalization.py discover --run .test-rationalization-20261105-01.tmp --configuration <Debug|Release> --label <before|after>
<PYTHON_EXE> -B scripts/test_suite_rationalization.py focused --run .test-rationalization-20261105-01.tmp --configuration <Debug|Release> --label <before|after>
<PYTHON_EXE> -B scripts/test_suite_rationalization.py matrix --run .test-rationalization-20261105-01.tmp --configuration <Debug|Release> --label <before|after>
<PYTHON_EXE> -B scripts/test_suite_rationalization.py serial --run .test-rationalization-20261105-01.tmp --label final
<PYTHON_EXE> -B -m unittest discover -s scripts/tests -p test_*.py -v  [fresh qualified Debug and Release command receipts]
git -c safe.directory=<REPOSITORY> diff --check
git -c safe.directory=<REPOSITORY> status --short
git -c safe.directory=<REPOSITORY> diff --cached --name-only
```

All qualified builds, discoveries and SPC focused selections exit 0; every full matrix exits **1**, retaining Common/Windows/Python failures. Source inputs remain unchanged during each matrix. Original strict exits **1**, original profile **1**, strict MkDocs **0**. Strict/profile remain genuine nonpasses, not skipped obligations. The unmodified `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1` and `-Legacy` wrapper are NOT_RUN because they overwrite fixed historical TestResults/Codex receipts; the isolated runner executes their full managed/Node/Python selections and additional required historical selections. It does not invoke packaging/deployment.

OfficeAddin and WasmSmoke application builds also passed in both configurations using fresh isolated outputs and no restore; all production source bytes remained unchanged. No application was deployed or opened.

No native Excel/COM/UIA, independent R, Office/Mac/browser manual validation, installer/signing, scientific correction, golden promotion or obligation discharge occurred. P=NO; N1=NO GO; N2=NO; Gate C2=UNMET. Future owner-authorized manual checks: confirm a representative Excel SPC phase/exclusion/report result and a WASM row/error/culture projection; both are NOT_RUN here and do not release any original gate.

## Exact inherited managed nonpasses

The same identities/outcomes appear in all four matrices. Proportions/Contingency scientific/domain discrepancies and Regression10 exception characterizations remain unresolved product/compatibility assertions; SPC adapter source/reflection/hash failures remain protected structural authorities. None was weakened or renamed. There is no new test-rationalization regression identity.

| Suite | Exact managed identity | Outcome |
|---|---|---|
| Common | `BESHStat.Excel.Common.Tests.ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (0,0,0)` | Failed |
| Common | `BESHStat.Excel.Common.Tests.Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings (True)` | Failed |
| Common | `BESHStat.Excel.Common.Tests.ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,30,50)` | Failed |
| Common | `BESHStat.Excel.Common.Tests.ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted` | Failed |
| Common | `BESHStat.Excel.Common.Tests.ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,15,40)` | Failed |
| Legacy | `BESHStatNG.Test.Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (3)` | Failed |
| Legacy | `BESHStatNG.Test.Spc21AdapterTests.NativeHostCoercionStateFitOutputAndChartTailStayEntryBytes (WriteResultsToNewWorkbook)` | Failed |
| Legacy | `BESHStatNG.Test.Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (2)` | Failed |
| Legacy | `BESHStatNG.Test.Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (0)` | Failed |
| Legacy | `BESHStatNG.Test.ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden` | Failed |
| Legacy | `BESHStatNG.Test.Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (4)` | Failed |
| Legacy | `BESHStatNG.Test.Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (5)` | Failed |
| Legacy | `BESHStatNG.Test.Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings (True)` | Failed |
| Legacy | `BESHStatNG.Test.Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (1)` | Failed |

## Exact inherited Python nonpasses

All four matrices retain the same 96 FAIL and 10 ERROR parent-method identities. Historical branch/source/provenance assertions remain real failures; error/fixture/environment causes are not silently converted into obsolete tests. Exact traces remain in unique private command receipts; sanitized outcome maps are in validation-v1.json. Gate/source controls remain part of full.

| Method identity | Outcome |
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

## Proposals requiring a separate owner decision

No retirement, exclusion, DataRow removal or cross-runtime reduction was implemented. The proposal JSON lists the exact 47 Windows SPC case candidates for any future smaller runtime subset, with its coverage risk; **no subset is selected or judged removable**. Python once-per-source reuse and historical-profile movement remain proposals only, with dependency/identity/cadence qualifications and OWNER_APPROVAL_REQUIRED. Timing saving is not estimated without a measured approved selection. Near-copy numerical/tolerance differences are not normalized.

Rollback is a reversal of the five project Compile/Link entries and removal of only this task's newly authored runner/tests/review packet after owner instruction; retained historical files require no repair. No rollback or deletion was executed.

## Preservation, privacy and publication boundary

Every pre-existing source/test byte except the approved test project is unchanged. All ten SPC source files retain entry hashes. The original 100 mixed-model capture hashes match their unchanged registry at entry/final; no native controls were released and no obligations discharged. The prior audit's 15 manifest entries remain exact. Historical TestResults and historical JSON/CSV/corpora/manifests/ledgers were not rewritten, moved or deleted; no generated-file inventory or cleanup proposal was made. Only the exact small protected projection fixture was read/copied for its existing managed test requirement.

The publication manifest covers only the changed test project and new runner/tests/dossier/review metadata, with SHA-256, privacy and trackability. Private raw receipts remain in the fresh ignored run root. Synthetic adversarial fixture paths in test identities are preserved as test input; no owner workbook data, credentials or private keys were collected. Final diff/status/index/manifest/privacy checks are recorded in the seal receipt. No staging/commit/merge/push/publish is authorized by this packet.

Original gate diagnostics are byte-identical to the accepted 04A26 receipts (both stdout and stderr hashes, both checks). Strict reports the stale `UibyID.vb` review dependency for `NormalPlot.vb` / `ConstructorBlock|0fdb131aa98f9dfe`; the profile reports `INTEGRITY_FAILURE` with its original empty error string. These are preserved integrity failures, not newly waived gates.

Installed toolchain: Python 3.14.7, .NET SDK 10.0.403, Node v24.20.0, MSBuild 18.9.15, modern VSTest 18.7.0 and Framework VSTest 18.11.0. New private TRX/command receipts and build manifests are hash-qualified in validation-v1.json. The compact inventory uses explicit class/method/display-name/outcome dictionaries; resolving its named indices reconstructs every raw identity and repeated-row outcome count.
