# Official 04A.09 owner review — regression request, diagnostic and result policies

Date: 3 November 2026. **PARTIAL — one bounded owner disposition,
`C09-NATIVE-STRICT-01`, remains.** The six shipping handlers delegate typed
request/options, expanded parameter counts and aligned diagnostic/result plans.
New strict Common validation is implemented and tested; activating its additional
Windows rejection paths is withheld pending the exact-ID timing disposition below.
This is not a claim that the complete definition of done has been accepted.

**P NO; N1 NO GO; N2 NO; Gate C2 UNMET.** No native obligation is discharged.

## Verified source and ownership

Entry was clean on local `main`, HEAD/main
`c25c6954d4e2948e728eebf395970d7ae635d6b6`. `git log -3` showed `c25c695`,
`b488774`, `b984e20`. The accepted 04A.08 authoring/serialization/controller files
and all nine files under `evidence/04A08/` are committed at that source. Their
accepted bytes remain unchanged. No ZIP SHA was assumed to be current `main`.
The branch did not exist; the authorized `codex/04A-09` was created from that
verified commit. HEAD remains the same commit; no index entry was staged.
Git used a command-local ownership exception, without changing global settings.

The committed plan is Revision 2.5. Its older administrative next-batch wording
is superseded by the owner's explicit acceptance/merge/start authorization in
this task. The plan, 04A.02 amended acceptance and historical restrictions were
read and retained. No framework, packaging or public UDF export changed.

| Existing path changed | Exact change / retained owner |
|---|---|
| `BESHstatNG/src/UI/UiGLM.vb` | Six fit methods plus new `CaptureRegressionRequest`; original import, effect expansion, dispatch, validation dialogs, lifecycle, progress/cancel and chart methods retained |
| `BESHstatNG/src/RegModels/BinaryClassificationReporting.vb` | Adds `ComposeGlmReport`; every existing facade/UDF/ROC member remains unchanged |
| `BESHstatNG/BESHStatNG.vbproj` | One explicit adapter include |
| `BESHStatNG.Test/BESHStatNG.Tests.vbproj` | Includes adapter tests and links two new portable test sources |

New production paths are
`BESHStat.Excel.Common/src/Analysis/RegressionRequestPolicy.vb`,
`RegressionClassificationPolicy.vb`,
`BESHStat.Excel.Common/src/Results/RegressionReportPolicy.vb`, and
`BESHstatNG/src/DM/WindowsRegressionRequestAdapter.vb`.
New test paths are `BESHStat.Excel.Common.Tests/src/Regression09Baselines.vb`,
`Regression09PolicyTests.vb`, `BESHStatNG.Test/src/Regression09AdapterTests.vb`,
and `scripts/tests/test_04a09_regression_successor.py`.
The three new scripts capture, verify and summarize this batch only.

[Final source delta](../../evidence/04A09/source-delta-final.json) records exact
before/after file and member hashes, every unchanged form/facade member, and
protected Core, designer, UDF, plan and prior-public-evidence pins. It verifies
writer-tail equality after the explicitly listed substitutions. The earlier
current-batch source-delta record is retained; the final record restores original
UTF-8 BOMs. No designer, independent regression form, importer, writer, chart
engine, existing test, reference fixture or Core algorithm changed.

## Six-family scope and parity

| Family / real handler | Common delegation | Characterized output and retained boundary |
|---|---|---|
| LM / `RunOLS` | Effects/intercept, expanded count policy, covariance/residual choices, source identity and report | Ordinary intercept / weighted no-intercept baselines; LM sum-of-squares selection and Core `Fit` remain Windows/Core-owned |
| GLM / `RunGLM` | Separate typed family/link/power/dispersion, starts, iterations, tolerance, optional sections and report | Poisson log with offsets/weights; binomial classification; original weighted-event-rate helper and ROC/calibration charts remain Windows |
| NB2 / `RunGLMNB2` | Link, starts, count, convergence settings, options and report | Dispersion is estimated by unchanged Core; residual-enabled/disabled cases; one-iteration Core exception characterized |
| ZIP / `RunZIP` | Separate component snapshots, intercepts/counts/starts, aligned rows and report | Independent Poisson/logistic matrices and labels, different intercept flags, predictions and optional residuals; no observation weights added |
| Ordinal / `RunOrdLogit` | Expanded slopes + observed-category threshold count, reference and report | First/last reference, numeric Core category axis and residual/probability matrices retained |
| Multinomial / `RunMultiLogit` | Expanded slopes/intercept × nonreference-class count, reference and report | Intercept on/off and reference variants; class-specific probabilities/residuals retain actual Core axis identities |

`RegressionRequest` owns immutable/cloned expanded arrays, ordered raw/effect keys,
04A.08 term metadata and independent optional vectors. It never retains `DataObj`,
controls, COM objects or callbacks. Null starts/vectors remain distinct from
supplied-empty inputs. Numeric option setters distinguish defaulted from explicitly
supplied invalid values with field-specific diagnostics. Alpha, finite tolerance,
iteration limits, family/link compatibility, vector identity/length, duplicate
source identities, effect/source metadata and existing Core response domains are
checked. Core's existing categorical rounding and nonnegative Poisson/NB domain
are retained; no new integer-only count restriction or silent row dropping exists.
LM retains its existing strictly positive weight requirement; zero weights remain
permitted by the GLM contract. Shared offset/weight role keys are diagnosed without
silently changing legacy roles.

The form still calls `RegressionDesignCore.BuildExpandedLmDataMatrix` /
`BuildExpandedRegressionDataMatrix`. Common counts the resulting columns; it does
not expand a formula or implement a solver. Ordinal omits a separate intercept
parameter and adds K−1 thresholds; multinomial multiplies by K−1. ZIP counts and
validates both starting vectors separately. Existing initial-count dialogs consume
those counts. LM's hidden initial-values control remains ignored as before.
LM also retains its existing Core `Alpha = 0.05` default: the GUI alpha snapshot
is request/presentation metadata and is not newly propagated into that fit.
Reconciling that existing LM-specific option behavior belongs to 04A.10.

The actual importer is unchanged: offset is appended before weights and removed
after the last weight column. Shipping adapter tests exercise row exclusion and
physical transport provenance from accepted 04A.27. Only immutable response
coordinates for retained rows are copied from its existing sidecar; full raw
workbook snapshots are not copied. Noncontiguous RowIds are never changed to 1..n.

`RegressionReportPolicy` snapshots existing ordered Core tables through
`AnalysisResultDocument`, preserving cells, labels, p-value annotations and optional
sections. Fitted/residual outputs carry exact source RowIds, explicit residual
header rows and actual numeric category axes. Mismatched lengths/order or ambiguous
identity fail before the first workbook write. Null and NaN remain distinct;
host error objects are refused. Document success means composition succeeded,
not convergence. Convergence is explicit, false or unknown; missing metadata is
never treated as optimizer success. Ordinal/multinomial convergence is read from
the existing Core diagnostic table, without a new numerical criterion.

The form uses the plan's residual/prediction payloads. Managed writer-base tests
compare the previous vector/tall and new matrix payloads cell for cell. Sheet
names, placement, blank separators, `ProcessListofResultTables`, formatting and
charts remain Windows-owned. `wrapResults` is called once, before the first
workbook write, so alignment errors do not leave a partially created report.
The Windows table writer still receives the original Core `ResultTable` list;
this batch does not claim to have migrated workbook placement.

Classification section selection and Core orchestration are Common-owned.
Threshold 0/1 boundaries, invalid/nonfinite thresholds, toggles, calibration,
Brier and zero-weight/no-data behavior are tested. Windows supplies its unchanged
weighted event rate. Existing missing-output sentinel and Excel `#N/A` spill
conversion remain host-only and are explicitly tested. Classification options
are read again at their original post-fit point: progress can pump UI events,
and retaining this timing avoids changing the enabled-state/control-read behavior.

The metadata snapshot reuses the existing document snapshot and lossless 04A.08
effect serialization. It intentionally does not redefine full output-cell
serialization. Complete numeric payloads remain in the typed document/row artifacts
and frozen baseline files. Raw Core warning strings without stable typed metadata
remain in their unchanged table/log context; no new Core warning API is claimed.

04A.10 remains responsible for deeper LM/Gaussian/binomial projections; 04A.11
for deeper NB2/ZIP/ordinal/multinomial numerical option/report projections. No
whole engine, workbook layout or UDF suite was moved into this batch.

## Numerical and behavioral evidence

[Baseline manifest](../../evidence/04A09/baseline-manifest.json) pins twelve actual
Core-backed host-free captures on unedited production source, using committed
Gaussian/Poisson/binomial/NB2/ZIP/ordinal/multinomial CSVs. Offset/weight vectors and
noncontiguous identities are deterministic in the pinned capture source. The
captures retain every table cell/label, sequence, p-value annotation and fitted /
residual vector. Only elapsed-time text is masked. Comparisons use
`max(1e-9, abs(expected)*1e-9)` for Double cells; string/type/order comparisons are
exact within a runtime. The first development run omitted the capture flag and
used an unsuitable one-iteration NB2 baseline; its failures were retained. The
successful pre-edit capture is 12/12. The NB2 exception now has its own test.

[Runtime baselines](../../evidence/04A09/runtime-baselines.json) distinguish the
later .NET Framework capture against unchanged Core. Framework G-format strings
such as embedded binomial diagnostics/NB variance expressions differ from .NET 10;
separate baselines preserve those strings rather than weakening comparisons.
These are Core-backed fits, not live Excel acceptance or promoted goldens.
[Readable fixture excerpts](../../evidence/04A09/fixture-readable-summary.json)
accompany the complete hashed payloads. For example, the synthetic Poisson
intercept is `0.24440700469112617` and its x1 coefficient is
`0.656515528704878`; complete coefficient/diagnostic/prediction values are retained
in the baseline. All six families' frozen numeric/table comparisons pass.

Additional tests cover real categorical/polynomial expansion, intercept-only
and no-intercept designs, malformed starts, independent ZIP failure/clear/re-add,
observed versus internal category order, source exclusions, immutable snapshots,
bounded 4,096-row input, invariant/`sk-SK`, convergence false/unknown, option presence
and deterministic serialization. Existing NIST and independent CSV/reference
comparisons run in the full Core/Windows suites with their original tolerances.
No fresh R invocation, scientific promotion, solver correction or statistical
parity claim beyond these documented host-free tests is made.

Known 04A.05/06/07 candidates and `C08-CORE-LABEL-01` /
`C08-FORMULA-REF-01` remain open. Typed keys/metadata are retained when optional
RHS projection is lossy. No frozen expectation was repaired to improve pass counts.

## Restrictions and one bounded disposition

[Final crosswalk](../../evidence/04A09/restriction-crosswalk-v2.json) preserves
**34 source-associated controls and seven original obligations**. The initial
direct-six-handler snapshot (29/4) is retained and explicitly narrower. The final
set includes unchanged transitive acquisition/validation dependencies. These are
association counts, not assertions that native controls were exercised.
[Delta against 04A.08](../../evidence/04A09/restriction-delta.json) lists exact
intersections/set differences; two effect-list controls overlap. Do not add the
populations. Source-qualified controls without an original deferred row acquire
no invented obligation. Original row SHA, detector, owner, mitigation batch and
gate are preserved; public rows are sanitized derivatives with original-byte hashes.

The seven original IDs are:

* `control:BESHStatNG.UiGLM.lbY`
* `control:BESHStatNG.UiGLM.lbOffset`
* `control:BESHStatNG.UiGLM.lbWeights`
* `control:BESHStatNG.UiGLM.lbSelectedEffectsList`
* `control:BESHStatNG.UiGLM.lbSelectedEffectsListLogistic`
* `control:BESHStatNG.UiGLM.lblProgress`
* `control:BESHStatNG.UiGLM.ProgressBar1`

All remain OPEN. Wider populations **1,514/802**, transferred **239/218**,
ANOVA/normality **160/141**, and importer timing **44** overlap and are retained.
No timing exception from 04A.05 or 04A.27 is generalized to this form. Original
ANOVA/normality/PCA/importer obligations, PID/HWND identity, loaded XLL/assembly
provenance, independent Windows/COM risk approval and 04A.45–48 duties remain open.

The [accepted 04A.02 gate](04A02/04A02-owner-acceptance-2026-11-02.md) requires
pre-change native evidence/release or a fresh exact case/criterion/ID-bound owner
deferral before changing affected native behavior. That retained rule is the
reason for the single disposition below; host-free tests do not waive it.

**Single owner disposition requested: `C09-NATIVE-STRICT-01`** (an administrative
review ID, not a new original obligation): approve the exact-ID deferral of
activating the additional strict Common rejection paths in Windows until the
regression pre-change native capture/risk/timing authority is satisfied. Keep the
shipping observation/legacy-validation mode meanwhile. The crosswalk enumerates
all 34 associated control IDs and the seven original obligation IDs for this
bounded request. The concrete
[unapplied activation patch](../../evidence/04A09/strict-validation-activation.UNAPPLIED.patch)
passes `git apply --check`; it has not been applied. Its target source hash is in
the final source delta and its own hash in the publication manifest.

This preserves actual shipping delegation and all safe backend work while avoiding
an unauthorized new native error/refusal path. It also means strict rejection of
every newly diagnosed condition is **not activated in shipping Windows**, so a
full unqualified completion recommendation would be inaccurate. No approval of
this dossier would itself release N1/N2, promote P or complete C2.

## Validation, preservation and publication

Final counts and all exact failure identities are in
[validation](../../evidence/04A09/validation-v2.json). Both configurations run the
same six managed suites and full Node/Python checks. The five inherited managed
nonpasses are the three `PairedDisjointCountContractAndPureSemanticProjection`
data rows `(20,30,50)`, `(20,15,40)`, `(0,0,0)`,
`ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`, and
`PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`.
No new managed functional nonpass is known.

| Final suite | Debug pass/total | Release pass/total |
|---|---:|---:|
| Core | 1,055/1,055 | 1,055/1,055 |
| Common | 1,340/1,344 | 1,340/1,344 |
| Windows .NET Framework 4.8 | 1,212/1,213 | 1,212/1,213 |
| Office add-in managed | 505/505 | 505/505 |
| WASM execution managed | 45/45 | 45/45 |
| Retained control projection | 61/61 | 61/61 |
| **Managed total** | **4,218/4,223** | **4,218/4,223** |
| Node (468 + 71 + 377) | **916/916** | **916/916** |
| Python distinct methods | **642/693** | **642/693** |

There are no managed skips, no new managed nonpasses, and no Node failures.
The 182 new managed executions comprise 83 Common and 99 Windows cases.
Python has 51 nonpassing method identities and 55 failure/error events including
subtests. `validation-v2.json` fixes the initial current-batch aggregation parser,
which counted four extra subtest events as methods; the superseded v1 record is
retained and no test result was changed. Both full verifier commands exit 1 for
the disclosed inherited/pin nonpasses. Common/Windows builds, Core/Office/WASM
tests and builds, Node, the focused new matrix, semantic repeats and control
projection succeed. All exact commands/outcomes are retained locally and their
sanitized derivatives are hash-linked in public evidence.

The two additional frozen Python nonpasses are
`Authoring08SuccessorTests.test_exact_id_crosswalk_retains_original_row_hashes_and_open_counts`
and `Authoring08SuccessorTests.test_original_numerics_and_shipping_forms_are_unchanged`.
They require the whole form to remain unchanged from 04A.07.
[Frozen-pin successor](../../evidence/04A09/frozen-pin-successor.json) records their
exact IDs, old/new source hashes, six changed members, new helper, new evidence
and retained restriction. Both remain FAIL. No original test was skipped,
xfail-marked, edited or rebaselined. The new eight source checks complement actual
managed adapter tests; source assertions are not the sole correctness evidence.
Administrative 04A.08 publication tests added since the older reported 675-test
population are included in current discovery; older totals were not assumed.

The original verifier was not run literally against its historical `TestResults/Codex`
paths: it restores packages and invokes legacy packaging. Recorded execution
copies retain its suites while using fresh result paths, `--no-restore`, isolated
assembly/test-data copies and Windows
`PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory` targets with
project-reference rebuilds disabled. Exact copy hashes/diffs, raw commands, exit
codes and TRXs remain private in this batch's new evidence root. Both copies use
`-Legacy -Wasm -Js -Python -VisualStudio 2026` and preserve independent execution
after inherited failures. This is a stated deviation from literal invocation to
honor the stronger no-clobber/no-network/no-packaging requirements.

Principal commands, from repository root (privacy placeholders are deliberate):

Qualified tools observed: Python **3.14.7**, Node **24.20.0**, .NET SDK
**10.0.403** with `wasm-tools`, and VS 2026 MSBuild **18.11.1** / Framework
VSTest **18.11.0**. Libraries remain `netstandard2.0`; Windows remains .NET
Framework 4.8. [Command outcomes](../../evidence/04A09/command-outcomes.json)
include execution-copy hashes/diffs and separate hashes for original raw logs.

```powershell
$py = '<PYTHON_EXE>' # Qualified local portable Python 3.14.7
& $py -B scripts/verify-04a09.py init --evidence TestResults/04A-09-20261103-run01
$env:BESHSTAT_04A09_CAPTURE = 'PRECHANGE-c25c695' # First .NET 10 capture only
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Debug --no-restore --filter FullyQualifiedName~Regression09Baselines --logger 'trx;LogFileName=prechange-capture.trx' --results-directory TestResults/04A-09-20261103-run01/prechange-capture
& $py -B scripts/capture-04a09-contract.py
# Framework capture used baseline48 after the recorded runtime48-capture-build.
# The capture variable was absent in every later focused/full execution.
& $py -B scripts/verify-04a09.py focused --evidence TestResults/04A-09-20261103-run01 --name focused-final09 --windows
& $py -B scripts/verify-04a09.py final --evidence TestResults/04A-09-20261103-run01 --configuration Debug --name final-Debug
& $py -B scripts/verify-04a09.py final --evidence TestResults/04A-09-20261103-run01 --configuration Release --name final-Release
& $py -B scripts/verify-04a09.py historical --evidence TestResults/04A-09-20261103-run01 --name historical
& $py -B -m unittest discover -s scripts/tests -p test_04a09_regression_successor.py -v
& $py -B scripts/verify-04a09.py preserve --evidence TestResults/04A-09-20261103-run01 --name final
git -c safe.directory=<REPO> apply --check evidence/04A09/strict-validation-activation.UNAPPLIED.patch
git -c safe.directory=<REPO> diff --check
git -c safe.directory=<REPO> diff --cached --exit-code
git -c safe.directory=<REPO> status --short --untracked-files=all
```

Development compile/test attempts and repaired fixture/namespace/cross-runtime
issues are retained in the new raw directory. In particular, a pre-test-discovery
Windows run matched zero new tests before test inclusion; it is not counted as
passing bridge evidence. Final discovery includes 83 new Common cases and 99
Windows cases (linked portable tests plus 16 adapter cases). The old nonpublic
`MatrixTestHelpers` discovery warning and old RefEdit variable warnings remain.
The unchanged original strict and approved-profile checkers were actually run;
both exit **1**, as detailed in
[historical checkers](../../evidence/04A09/historical-checkers.json). The strict
checker stops on the inherited `UibyID`/`NormalPlot` stale review dependency;
the profile checker reports historical source/head/policy pin drift. Neither is
reported green or represented as the older recorded 24-failure execution.

The initial preservation inventory covers **173,832 historical TestResults files**.
Membership, size, creation/write nanosecond timestamps, file/device identity and
frozen source hashes are checked; this is not a claim of a fresh full historical
byte scan. The complete tree, raw/local references, original source pins and prior
published evidence remain in place. The final preservation receipt and free disk
observation are published separately. No deletion, cleanup, retimestamping,
relocation, archive copy or historical sanitization was performed.
The [final receipt](../../evidence/04A09/preservation-final.json) is **PASS**, with
zero errors and **534,417,285,120 bytes** free. Original raw receipt bytes have a
separate SHA-256; the public derivative contains no historical raw paths.

[Publication manifest](../../evidence/04A09/publication-manifest.json) lists every
modified/untracked proposal path and SHA-256. `evidence/04A09/` and this dossier
are normally trackable, as checked with live `git check-ignore -v` and status.
The dossier matches the explicit Markdown unignore exception; the new evidence
directory has no matching exclusion.
`.gitignore` is unchanged; no force-add is needed. Entire `TestResults`, raw
TRXs/logs/preimages/inventories, caches and build outputs remain ignored/private.
Public derivatives use `<REPO>`, `<OWNER_HOME>`, `<PYTHON_EXE>`, `<VS2026>` and
`<OWNER_SID>`; original local bytes and hashes are retained untouched. Nothing
is staged. The final manifest excludes itself from recursive hashing.

## Manual checks not run

1. After separate native authority, fit Poisson with log link, nonzero offset,
   unequal weights, excluded/noncontiguous worksheet rows, diagnostics,
   covariance and residual toggles. Compare Data/GLM sheets, RowIds, table order,
   cancel/interrupt and error dialogs to approved pre-change capture.
2. After separate native authority, fit multinomial with first/last references,
   intercept on/off and residual diagnostics; compare class-specific probability
   labels/columns and worksheet output. ZIP dual-model selection/starts and
   binomial charts remain part of the later exact native matrix.

WASM/Office projects are build-smoked; browser/authenticated Office/live Excel
acceptance is **NOT RUN**. No live Excel, COM/ROT/UIA, installer, deployment,
signing, staging, commit, merge, push, remote operation, package installation,
04A.10 or 04A.11 work occurred. **P NO; N1 NO GO; N2 NO; C2 UNMET.**


## Complete Python nonpass identities

Both configurations have the same outcomes. Failure/error remain nonpasses.

| Outcome | Exact method ID | Attribution |
|---|---|---|
| FAIL | `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a02_checkpoint11.Namespace11Tests.test_exact_retained_bytes_reproduce_only_compiler_rejection` | Inherited 04A.08 identity/outcome |
| ERROR | `test_04a02_checkpoint11.Provenance11Tests.test_both_authorized_case_pins_pass_and_frozen_false_is_not_owner_decision` | Inherited 04A.08 identity/outcome |
| ERROR | `test_04a02_checkpoint12.Diagnosis12Tests.test_both_authorized_case_pins_keep_original_compiler_and_scientific_policy` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a02_checkpoint12.Diagnosis12Tests.test_exact_owner_bytes_negative_for_frozen_v5` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a02_checkpoint13.Startup13Tests.test_corrupt_receipt_hash_fails_even_with_exit_zero` | Inherited 04A.08 identity/outcome |
| ERROR | `test_04a02_checkpoint13.Startup13Tests.test_real_successful_owner_startup_all_pins_and_native_identities` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a02_checkpoint13.Startup13Tests.test_tampered_stdout_hash_stops_before_parser` | Inherited 04A.08 identity/outcome |
| ERROR | `test_04a02_checkpoint14.Actual14Tests.test_actual_two_owner_publications_and_both_cultures_reproduce` | Inherited 04A.08 identity/outcome |
| ERROR | `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a03_control_mitigation.ControlMitigationSourceTests.test_per_mode_consumers_and_branch_order` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a03_control_mitigation.ControlMitigationSourceTests.test_sealed_sources_are_reconciled_never_rewritten` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a03_control_mitigation.NativeGateNegativeTests.test_review_packet_source_pins_match_current_sources` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a03_selection.UnivariateSelectionSourceTests.test_all_native_lifetime_cancel_write_and_reporting_methods_unchanged` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a03_selection.UnivariateSelectionSourceTests.test_only_two_form_methods_change` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a04_alignment.PairedBlockSourceGates.test_exact_crosswalk_and_authorities` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a04_alignment.PairedBlockSourceGates.test_legacy_geometry_ambiguity_is_not_hidden` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_independent_candidate_provenance_not_core_or_golden` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_nonpaired_code_and_unrelated_forms_are_unchanged_from_partial` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_owner_bound_successor_and_source_hashes` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_historical_ledgers_and_importer_unchanged` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_other_multicol_members_including_04a04_selection_unchanged` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_prior_04a04_whole_file_source_check_remains_frozen_and_disclosed` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_scientific_ambiguity_is_not_a_permanent_portable_wrong_table` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_ui82x2_source_wiring_explicitly_withheld` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_versioned_successor_sources_and_case_boundaries` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_actual_form_routes_and_unchanged_native_tails` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_importer_udf_designers_core_and_old_tests_unchanged` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_native_population_is_union_not_sum` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_new_scientific_candidates_are_explicitly_unmodified` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_no_duplicate_statistics_in_form_or_windows_adapter` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_production_sources_and_exact_open_ledger_lineage` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_core_old_tests_importer_udfs_designers_and_help_unchanged` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_exact_source_and_original_ledger_rows` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_no_fabricated_obligation_for_previously_source_qualified_controls` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_restriction_unions_and_timing_exceptions_not_generalized` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a08_authoring_successor.Authoring08SuccessorTests.test_exact_id_crosswalk_retains_original_row_hashes_and_open_counts` | Newly triggered frozen whole-form pin; versioned successor evidence only |
| FAIL | `test_04a08_authoring_successor.Authoring08SuccessorTests.test_original_numerics_and_shipping_forms_are_unchanged` | Newly triggered frozen whole-form pin; versioned successor evidence only |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_compile_targets_and_native_helpers_unchanged` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_real_consumer_routes_and_udf_exports_preserved` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_source_owner_pins_and_all_44_original_rows` | Inherited 04A.08 identity/outcome |
| FAIL | `test_04a27_transport_successor.Transport27SuccessorTests.test_exact_successor_and_unapplied_patch` | Inherited 04A.08 identity/outcome |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_all_239_ledger_rows_identified_with_original_digest_convention` | Inherited 04A.08 identity/outcome |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_complete_source_crosswalk_equality` | Inherited 04A.08 identity/outcome |
| FAIL | `test_capture_contract04a02.CaptureContracts.test_packet_covers_all_original_cases_and_six_gates` | Inherited 04A.08 identity/outcome |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_packet_retains_additional_21_deferred_obligations` | Inherited 04A.08 identity/outcome |
| ERROR | `test_investigation06.Investigation06Tests.test_all_239_exact_ids_have_reversible_root_cause_source_maps` | Inherited 04A.08 identity/outcome |
| FAIL | `test_investigation06.Investigation06Tests.test_anova_defaults_mapping_remain_explicit_source_facts` | Inherited 04A.08 identity/outcome |
| FAIL | `test_investigation06.Investigation06Tests.test_normality_5000_disagreement_preserved_as_review_defect` | Inherited 04A.08 identity/outcome |
| FAIL | `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof` | Inherited 04A.08 identity/outcome |
| FAIL | `test_investigation06.Investigation06Tests.test_two_by_two_or_condition_and_no_chart_handler` | Inherited 04A.08 identity/outcome |
