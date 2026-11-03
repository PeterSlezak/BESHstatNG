# Official 04A.10 consolidated owner review

**READY_FOR_OWNER_04A10_ACCEPTANCE_REVIEW — owner acceptance PENDING.**
This is the single bounded 04A.10 software handoff. Native/scientific qualification
and the disclosed unchanged Core candidates remain OPEN.

**P NO; N1 NO GO; N2 NO; Gate C2 UNMET. Native Excel execution NOT RUN.**
No numerical reference promotion, independent scientific certification, native
control release, Git publication or next-batch authorization is implied.

## Starting state and protected authority

Entry was local `main`, with empty `git status --short` and an empty index.
`main`/HEAD was `49fcc30a6b39f6a2bebcc9353ce2dea0d3e13055`, whose subject is
the six-family 04A.09 extraction. Its parent is `c25c695`; the accepted 04A.09
source, dossier, fixtures and restriction records are present. This verifies
the local accepted implementation is on main; it asserts no remote state or
separate merge-commit topology. The authorized branch `codex/04A-10` was newly
created from that main. HEAD remains the base; no changes were staged or committed.

Git required the same command-local `safe.directory` exception as prior batches.
Branch creation additionally required sandbox approval to write `.git/index.lock`;
the authorized branch was then created. No global Git setting changed. The
checked-out Revision 2.5 Master Plan and AGENTS.md were read, along with the
04A.09 dossier/crosswalk and retained 04A.02 amended gate. Older next-batch wording
in the plan is superseded by the owner's current explicit 04A.09 acceptance/merge
confirmation and authorization of 04A.10. The plan itself is unchanged.

[Pre-edit inventory](../../evidence/04A10/prechange-manifest.json) hashes tracked
sources and existing docs before production changes. It was taken after creating
the new capture harness, so those two authorized untracked files appear in its
status. The earlier entry status was clean. No unrelated tracked/untracked owner
content was overwritten. All accepted 04A.02–09 tracked evidence, original tests,
Core sources, public UDFs, designers, framework targets, packaging, versions,
signing, importer, writers, charts and shared six-family policies are protected by
the [source receipt](../../evidence/04A10/source-delta.json).

## Implemented request, response and report contract

| Shipping path | Common-owned projection | Preserved calculation/native ownership |
|---|---|---|
| `UiGLM.RunOLS` | `LinearBinomialRegressionPolicy.Project` snapshots existing expanded request, intercept, exact term groups, Type I/III selection and ordered coding notes; effective/request alpha distinguished | Actual `LinearModel.Data`, `Fit`, `wrapResults`; original weight order and Core calculations; Data/LM writers |
| Gaussian branch of `UiGLM.RunGLM` | Existing Gaussian family and selected Core link; expanded data, typed options/provenance; real response-scale predictions, seven-column residual axis, convergence and ordered document | Existing GLM engine, offsets/weights, alpha/iterations/epsilon/starts, progress/cancel and Data/GLM write sequence |
| Binomial branch of `UiGLM.RunGLM` | Same family-specific intent, probability-scale predictions, existing diagnostics; immutable post-fit classification document/options/source-row plan | Existing `RegressionClassificationPolicy` through the Windows facade, weighted event-rate helper, classification control reads and ROC/calibration chart tail |

New production files are
`BESHStat.Excel.Common/src/Analysis/LinearBinomialRegressionPolicy.vb` and
`BESHStat.Excel.Common/src/Results/LinearBinomialReport.vb`.
Existing production edits are limited to `BESHstatNG/src/UI/UiGLM.vb`
(`RunOLS`, `RunGLM` only) and the additive typed overload in
`BESHstatNG/src/DM/WindowsRegressionRequestAdapter.vb`.
`BESHStatNG.Test/BESHStatNG.Tests.vbproj` adds explicit test/link inclusions.

New tests are `Regression10Baselines.vb`, `Regression10PolicyTests.vb` under
`BESHStat.Excel.Common.Tests/src`, `BESHStatNG.Test/src/Regression10AdapterTests.vb`
and `scripts/tests/test_04a10_regression_projection.py`. New scripts and evidence
are listed in the publication manifest. No existing accepted fixture/test was
edited. No project target or production project inclusion needed changing.

The intent composes the accepted `RegressionRequest`, `RegressionOptions`,
`RegressionComponent`, `RegressionVector` and `RegressionValidation`; it creates
no parallel request grammar or estimator. It clones term-group arrays and coding
notes. The existing request clones expanded arrays, names, starts, vectors,
physical response coordinates and source RowIds. No host objects or Core model
references survive in the portable response. Supplied-empty and absent inputs
remain distinct. Intercept and term labels come from existing Core expansion and
04A.08 typed effects; hidden LM starts remain ignored in Windows.

The response consumes actual `wrapResults` tables once, before the first workbook
write, using the existing `RegressionReportPolicy`, `RegressionRowOutput` and
`AnalysisResultDocument`. It retains table count/order, raw scalar types, missing
and nonfinite cells, titles/headers/footnotes, p-value columns/cells and all Core
numerical/presentation decisions. Fresh `ResultTable` instances are rehydrated
from that immutable document for `ProcessListofResultTables`; the native writer
therefore consumes the projected document, not an unused auxiliary plan. The
bridge preserves header intersections and blank cells as well as body values.
The shipping writer base executes the same ordered blocks and separators in
host-free tests. This is not a new output writer or a layout redesign.

LM preserves coefficient, model diagnostic, overall ANOVA, selected term ANOVA,
VIF/partial-correlation and optional covariance order. Its six residual columns
remain fitted, raw residual, leverage, standardized residual, Cook's D and
jackknife. Gaussian/binomial preserve the actual coefficient/fit, binomial
case/odds/Hosmer–Lemeshow, optional iteration and covariance sections as Core
generates them. Their seven residual columns remain raw, deviance, Pearson,
leverage, standardized deviance/Pearson and Cook's distance. No absent section
is invented. Gaussian dispersion/SE scaling stays in the existing GLM; no NB
dispersion option is applied to it and Gaussian GLM is not replaced by LM/WLS.

Core residual matrices contain **no embedded RowId column**. Their axis follows
the unchanged fit input order; the portable row output carries exact source
identities separately. Length, zero bounds, residual width and explicit string
headers are checked before any writer. A producer permuting residual values
without supplying corresponding identities cannot be detected from that legacy
array alone; no such permutation is introduced here. Fitted vectors are not
inverse-linked again. NaN, infinity, zero and Nothing are not conflated.

Validation diagnostics remain separate from optimizer convergence and composition
success. LM is explicitly `NotIterative`; GLM's actual boolean convergence is
retained, including false. Empty optional sections and unavailable residuals stay
explicit, with the existing report warnings. Strict typed diagnostics do not add
a new Windows refusal branch. `CaptureRegressionRequest` is byte/member unchanged.

The binomial controls are re-read **after fitting**, at their original enabled-state
checks and threshold getter. `LinearBinomialClassificationPlan` snapshots those
actual later options, document and exact request RowIds, rather than substituting
the earlier request options. Threshold/threshold-table, calibration/bin count,
Brier and p-value policy remain represented. Existing Common/Core classification
does the calculations; the Windows event-rate/ROC/calibration helpers are unchanged.
The native table, ROC, calibration order and optional `GLM Classification` sheet
are unchanged. Charts are not executed by host-free tests.

Other GLM families continue on their accepted 04A.09 overload/family construction.
`RunGLMNB2`, `RunZIP`, `RunOrdLogit`, `RunMultiLogit` and every other form member
are unchanged. Their family-specific options/numerics/report projection remain
04A.11 ownership. All public UDF source bytes/exports remain unchanged.

## Numerical evidence and bounded candidates

[Baseline registry](../../evidence/04A10/baseline-registry.json) records **six
pre-production-edit real-Core captures per runtime**, .NET 10 and .NET Framework
4.8. The original 24 04A.09 captures remain byte-identical. The new captures cover
OLS/Type III/intercept, WLS/Type I/no intercept, Gaussian identity/log and binomial
logit/probit, with appropriate offsets, unequal weights, residuals, covariance,
iteration sections, coding notes and reversed noncontiguous identities.
04A.09 GLM ordinary remains Poisson; it is never claimed as Gaussian.

The [byte provenance receipt](../../evidence/04A10/baseline-byte-provenance.json)
identifies one additional **pre-existing** frozen Python nonpass compared with
the reported 04A.09 totals: `test_runtime_baselines_are_incremental_and_complete`.
All 24 accepted runtime-receipt hashes expect CRLF, while the checked-out main
files and their Git blobs use LF (`text:auto`, `eol:lf`). The initial inventory
and current raw hashes match for every file; converting a diagnostic in-memory
copy to CRLF reproduces every accepted receipt hash. No baseline or receipt was
changed to hide this. Line-based numerical comparisons still pass. This is live
entry provenance drift, separate from the two source expectations made stale by
04A.10, and remains a reported FAIL.

Every table/vector is compared using the existing typed capture encoding and
`max(1e-9, abs(expected)*1e-9)` Double tolerance, with exact strings/types/order and
presentation metadata within each runtime; only elapsed-time text is masked.
Both cultures compare numeric cells to frozen captures and exact strings to
same-culture projected output. Core's culture-dependent embedded strings are
not falsely claimed invariant. Runtime-specific captures preserve runtime strings.

The [readable numeric receipt](../../evidence/04A10/numerical-comparisons.json)
contains section metadata and coefficient/SE/CI/p-value excerpts. Representative
intercepts are LM OLS `2.021084044061574`, Gaussian identity `2.022589385687878`,
Gaussian log `0.5706046158683356`, binomial logit `-0.44734995259382415` and probit
`-0.27066631037738786`. The frozen weighted binomial paths report **Converged=False**;
that is preserved, not relabelled success. Full unchanged Core tests also exercise
the eleven NIST linear cases and committed GLM all-link reference cases. New
Common tests explicitly compare projected NIST Norris coefficients/SEs in both
cultures. Existing reference tolerances and provenance remain unchanged. No
external reference run, reference promotion or independent scientific review
was performed. All new observations are **NOT_GOLDEN**.

| Candidate | Exact current finding and disposition |
|---|---|
| `C10-LM-ALPHA-01` | Original `LinearModel.Alpha` defaults to 0.05; original RunOLS never assigns it. New intent records requested alpha separately from effective 0.05, and response receives actual `lm.Alpha`. Request alpha 0.10 still yields the existing 95% coefficient intervals. No numerical correction made. |
| `C10-CORE-CONSTANT-01` | Constant binomial response can trigger Core early exit; `GLM.wrapResults` then requests an unavailable Hosmer–Lemeshow payload and throws `NullReferenceException` at its `SetBody` call. Focused direct-Core and projected-path regression fixture asserts the same failure. Proposed future correction: guard/report the unavailable section after early exit, with separately reviewed output/behavior disposition. Core unchanged. |
| `C10-COV-NAMES-01` | Existing `GLM.wrapResults` covariance block aliases `pVarNames` and writes element zero to `Intercept`. The shipping Data header can inherit this mutation. It is preserved. The capture harness subsequently clones names at the Core boundary for reusable request metadata; original capture-source bytes/hash remain private and registry records that test-only delta. All frozen table/vector captures are unchanged. Proposed future correction requires explicit output compatibility review. |
| `C09-NATIVE-STRICT-01` | Previously approved deferral continues. New validation remains diagnostic/observational in Windows. The activation patch stays unapplied. |

Read-only UDF reconnaissance confirms `LM_FIT` explicitly applies its alpha and
fits Type III plus a separate Type I model, while GUI RunOLS keeps its Core default
and selected SS mode. GLM UDF and GUI reuse Core family/link/fit operations, but
UDF strict-input, optional output/cache and GUI native-chart behavior are distinct.
No UDF surface was rewritten to force equivalence. Earlier C08 label/formula
limitations and 04A.05/06/07 candidates remain open.

The new tests cover intercept/weight/SS/covariance/residual matrices, genuine typed
polynomial/categorical term groups/coding order, intercept-only, singular/near-
collinear and constant outcomes, valid Gaussian/binomial starts, wrong/empty
starts, post-fit classification changes, invalid thresholds, source/vector/key
diagnostics through the accepted policies, GLM zero weights versus positive LM
weights, malformed residual/fitted axes, nonfinite cells, zero rows, absent sections,
separation/nonconvergence, cloning and host independence. Existing 04A.09 negative
contracts are rerun rather than copied or weakened. Degenerate designs are compared
to unchanged Core; unavailable Core outputs are not fabricated for testing.

## Validation and retention

Current full outcomes, exact command/exit-code receipts, all managed and Python
failure identities and source-pin successor attribution are linked in
[validation](../../evidence/04A10/validation.json),
[commands](../../evidence/04A10/command-outcomes.json) and
[historical checkers](../../evidence/04A10/historical-checkers.json).
| Final suite | Debug passed/total | Release passed/total |
|---|---:|---:|
| Core | 1,055/1,055 | 1,055/1,055 |
| Common | 1,384/1,388 | 1,384/1,388 |
| Windows .NET Framework 4.8 | 1,270/1,271 | 1,270/1,271 |
| Office add-in managed | 505/505 | 505/505 |
| WASM execution managed | 45/45 | 45/45 |
| Retained control projection | 61/61 | 61/61 |
| **Managed** | **4,320/4,325** | **4,320/4,325** |
| **Node** (468 + 71 + 377) | **916/916** | **916/916** |
| **Python distinct methods** | **647/701** | **647/701** |

Both complete verifier execution copies exit **1**, as do Common/Legacy suite
invocations and Python discovery with inherited failures. Core/Office/WASM tests,
both Office/WASM builds, both safe Windows builds, all Node suites, focused
04A.09+10 contracts, semantic repeats and retained control projections pass.
The historical strict/profile checkers were actually run and each exits **1**:
strict stops at the inherited `UibyID`/`NormalPlot` stale review dependency;
profile reports historical source/policy/head/branch pins. They are never green.

There are **102 new passing managed executions per configuration**: 44 Common
and 58 Windows (44 linked portable plus 14 adapter cases). The final focused
matrix is **127/127 Common**, **157/157 Windows**, including accepted 04A.09
cases. Eight new Python source compatibility checks pass. There are no managed
skips or unexplained new functional failures. The five managed nonpasses exactly
match the historical identities/outcomes: three
`PairedDisjointCountContractAndPureSemanticProjection` rows `(20,30,50)`,
`(20,15,40)`, `(0,0,0)`,
`ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`, and
`PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`.

Python retains all **51** previously reported nonpassing methods with the same
outcomes, plus the entry-existing baseline byte-pin failure described above and
two source expectations made stale by this batch. All **54** remain nonpasses;
their complete identities are appended below. The two newly stale identities are:

* `test_04a09_regression_successor.Regression09SuccessorTests.test_exact_six_changed_members_and_unchanged_native_surroundings`
* `test_04a09_regression_successor.Regression09SuccessorTests.test_real_six_routes_use_common_options_counts_and_reports`

The separately discovered entry-existing identity is
`test_04a09_regression_successor.Regression09SuccessorTests.test_runtime_baselines_are_incremental_and_complete`.
The [successor receipt](../../evidence/04A10/frozen-pin-successor.json) preserves
all frozen assertions and supplies exact source/pre-change/current hashes; it
does not turn any of these failures into passing tests.

Development compile/fixture mistakes and their failing runs are retained in the
fresh raw directory. Corrections were confined to new code/tests: nullable enum
comparison under Strict On, explicit VB loop variable typing, avoiding GLM options
on LM test requests, preserving false convergence, masking elapsed time in paired
Core captures, characterizing the constant-response Core throw, and cloning capture
names before the existing covariance mutation. No accepted assertion was weakened.
The existing nonpublic `MatrixTestHelpers` discovery warning and RefEdit compiler
warnings remain pre-existing warnings. No production change followed the final
focused pass; later work only completed provenance/summary artifacts and checks.

Full suites run once per configuration after focused corrections. The literal
`scripts/verify-beshstatng.ps1` is unchanged and **not invoked directly** because
its fixed TestResults/Codex writers would clobber retained history, and its default
restore/Windows Build hooks cross the no-network/no-packaging boundary. The same
portable, Legacy, WASM, JS and Python suite sequence runs via audited source-hashed
execution copies with fresh destinations, `--no-restore`, VS 2026, isolated managed
Common/Legacy assembly copies, safe host compile targets and independent continuation
after inherited failures. Every transformation/diff is retained in the command
receipt. This is an explicit limitation/deviation from the literal two commands.

Principal commands from repository root (`<PYTHON_EXE>` denotes the owner's
specified portable Python executable; no alternate Python was substituted):

```powershell
& <PYTHON_EXE> -B scripts/verify-04a10.py init --evidence TestResults/04A-10-20261103-run01
# Explicit pre-edit create-new capture flag was used only for the following two runs.
& <PYTHON_EXE> -B scripts/verify-04a10.py capture --evidence TestResults/04A-10-20261103-run01 --name prechange-net10
& <PYTHON_EXE> -B scripts/verify-04a10.py capture48 --evidence TestResults/04A-10-20261103-run01 --name prechange-net48
& <PYTHON_EXE> -B scripts/verify-04a10.py focused --evidence TestResults/04A-10-20261103-run01 --name focused-10 --windows
& <PYTHON_EXE> -B scripts/capture-04a10-contract.py
& <PYTHON_EXE> -B -m unittest discover -s scripts/tests -p test_04a10_regression_projection.py -v
& <PYTHON_EXE> -B scripts/verify-04a10.py final --evidence TestResults/04A-10-20261103-run01 --configuration Debug --name final-Debug
& <PYTHON_EXE> -B scripts/verify-04a10.py final --evidence TestResults/04A-10-20261103-run01 --configuration Release --name final-Release
& <PYTHON_EXE> -B scripts/verify-04a10.py historical --evidence TestResults/04A-10-20261103-run01 --name historical
& <PYTHON_EXE> -B scripts/verify-04a10.py preserve --evidence TestResults/04A-10-20261103-run01 --name final
git -c safe.directory=<REPO> diff --check
git -c safe.directory=<REPO> diff --cached --exit-code
git -c safe.directory=<REPO> status --short --untracked-files=all
```

The execution copies invoke PowerShell with `-NoProfile -ExecutionPolicy Bypass`,
`-Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe <PYTHON_EXE>` and the
requested configuration. Node runs OfficeAddin `npm.cmd test`, the Help resolver
test and WasmSmoke `npm.cmd test` (whose `node --test` discovers the CJS suites,
including `tests/*.test.cjs`). All configured Python discovery/checkers run,
including the additional Common Python suite and historical strict/profile checkers.
Detailed effective commands and exit codes, not just these wrapper commands,
are published in the command receipt.

Qualified versions observed: .NET SDK **10.0.403** with installed `wasm-tools`,
VS 2026 MSBuild **18.11.1**, Framework VSTest **18.11.0**, Node **24.20.0** and
the requested portable Python **3.14.7**. Portable targets remain `netstandard2.0`;
Windows remains .NET Framework 4.8 with original x86/x64 Excel-DNA packaging.

The initial historical inventory contains **177,059 TestResults files**.
Membership, size, creation/write nanosecond timestamps and file/device identity
are checked at closeout, together with accepted tracked source/evidence SHA-256.
This is **not** a full byte-level rehash of historical TestResults. No history was
deleted, rewritten, moved, cleaned, retimestamped or pruned. Original raw traces,
local-only evidence and external plan bytes remain in place. The final preservation
receipt records its scope and free disk independently.

## Restrictions, publication and manual smoke NOT RUN

The [exact-ID crosswalk](../../evidence/04A10/restriction-crosswalk.json) carries
forward the original 34-control/seven-obligation set and original row hashes.
No additional control joins the union. The seven original obligations remain OPEN:

* `control:BESHStatNG.UiGLM.lbY`
* `control:BESHStatNG.UiGLM.lbOffset`
* `control:BESHStatNG.UiGLM.lbWeights`
* `control:BESHStatNG.UiGLM.lbSelectedEffectsList`
* `control:BESHStatNG.UiGLM.lbSelectedEffectsListLogistic`
* `control:BESHStatNG.UiGLM.lblProgress`
* `control:BESHStatNG.UiGLM.ProgressBar1`

All 1,514 original obligations and 802 restricted controls, overlapping transferred
239/218, ANOVA/normality 160/141 and importer 44 obligations remain OPEN; do not
sum these populations. Source tests do not establish native exercise or discharge.
The strict activation patch SHA-256 remains
`aab350401c873d1a4f56450295688e9a046122aca2269fc9a618765f8fe79fe1` and the patch
is unapplied. No rejection based on `Validate(...).Blocking`/`IsValid` was added
to capture or the shipping routes.

Before any native pilot: corrected HWND identity, trusted live XLL/assembly
attribution, independent Windows/COM risk review, pre-change captures and explicit
N/timing authority are still required. Later 04A.45–48 duties remain intact.

Proposed Excel checks, both **NOT RUN**:

1. Fit LM with intercept on/off, noncontiguous retained worksheet rows, unequal
   positive weights, Type I/III, covariance/residual toggles and categorical coding.
   Compare Data/LM sheet count/names, original data/weight columns, fitted/residual
   headers and values, section order, p-value display and coding footnotes with
   authorized pre-change capture. At GUI alpha 0.10 expect the retained 95% Core
   intervals, not newly computed 90% intervals. Include error/cancel behavior.
2. Fit binomial logit/probit with offset/weights, covariance/residuals/iteration
   options, then classification/threshold/table/calibration/Brier choices. Compare
   Data, GLM and optional GLM Classification writes, probability scale/source rows,
   table → ROC → calibration order and progress/cancel behavior. A separately
   authorized event-pumping test must verify post-fit control reads. Gaussian
   identity/log with offsets/weights is an additional comparison, not LM substitution.

WASM/Office managed builds/tests are host-free validation; browser/authenticated
Office workbook smoke is **NOT RUN**. No live Excel, COM/ROT/UIA, installer,
publishing, signing, network/package install, staging/commit/merge/push or remote
operation occurred. No 04A.11 work was started.

Public candidate files are directly under verified non-ignored paths. The
[publication manifest](../../evidence/04A10/publication-manifest.json) lists paths,
hashes, Git-ignore results and privacy scan scope. Raw local commands/TRXs/logs and
inventory stay separately ignored. Public derivatives use relative paths and
`<REPO>`, `<OWNER_HOME>`, `<PYTHON_EXE>`, `<VS2026>`, `<OWNER_SID>` placeholders.
Raw-original hashes are labelled separately from sanitized derivatives; no derivative
is asserted to have its raw original's hash. Nothing is staged or published.

## Owner decision requested

Review and accept the bounded 04A.10 **software** extraction, with the disclosed
unchanged Core candidates, historical nonpasses, approved C09 strict deferral,
and native/scientific qualifications remaining OPEN. This request neither
self-accepts the batch nor asks to release controls, promote P, publish Git changes
or start 04A.11.

| Acceptance criterion | Current disposition |
|---|---|
| 1. Correct branch/base and protected baselines | MET for live entry bytes; the pre-existing CRLF/LF receipt mismatch is disclosed, not repaired |
| 2. Substantive LM/Gaussian/binomial request/Core-response/document projection | MET; no duplicate solver or request hierarchy |
| 3. Actual shipping delegation and retained lifecycle/timing | MET by executable adapter/writer tests plus exact route/tail source delta; native qualification still OPEN |
| 4. Rows/options/defaults/SS/p-value/section alignment | MET for bounded software contracts, including request versus actual fit alpha |
| 5. NIST and actual Gaussian/binomial comparisons | MET as NOT_GOLDEN regression evidence; no scientific promotion |
| 6. Focused/full compatibility and failure reconciliation | MET; all 102 new managed executions pass per configuration; historical/source-pin nonpasses retained explicitly |
| 7. Protected 04A.11/Core/UDF/acquisition/native modules | MET; 58 other form members and 3,171 other existing tracked/docs files protected by hashes |
| 8. Strict patch, gates and original obligations | MET as restrictions retained; patch unapplied; P/N/C2 and every original obligation unchanged |
| 9. Historical retention, publication/privacy and Git limits | MET; preservation PASS, zero privacy findings, non-ignored public candidate paths; no Git publication action |
| 10. One candid dossier and owner boundary | MET; owner decision pending, no 04A.11 work |


## Final preservation and complete Python nonpass ledger

Preservation **PASS**: 177,059 historical files, zero membership/metadata/source-pin errors; 533,906,780,160 bytes free at the receipt. Scope is membership, size, creation/write nanosecond timestamps and file/device identity plus protected source hashes, not a full historical byte audit. All 3,171 protected existing tracked/docs files match entry hashes, including 51 prior public evidence files and 841 architecture/planning files.

`git diff --check` and the empty-index check pass. Current changes are the three approved tracked files plus the new Common/tests/scripts/dossier/evidence candidates listed in the publication manifest. No other tracked file changed. All current proposal paths are checked with `git check-ignore`; raw TestResults remain ignored. Privacy receipt covers all proposal text and all decoded baseline string cells.

Both configurations retain 54 distinct nonpassing methods and 58 failure/error events including subtests. No inherited outcome was changed.

| Outcome | Exact identity | Attribution |
|---|---|---|
| FAIL | `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a02_checkpoint11.Namespace11Tests.test_exact_retained_bytes_reproduce_only_compiler_rejection` | Inherited reported 04A.09 nonpass |
| ERROR | `test_04a02_checkpoint11.Provenance11Tests.test_both_authorized_case_pins_pass_and_frozen_false_is_not_owner_decision` | Inherited reported 04A.09 nonpass |
| ERROR | `test_04a02_checkpoint12.Diagnosis12Tests.test_both_authorized_case_pins_keep_original_compiler_and_scientific_policy` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a02_checkpoint12.Diagnosis12Tests.test_exact_owner_bytes_negative_for_frozen_v5` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a02_checkpoint13.Startup13Tests.test_corrupt_receipt_hash_fails_even_with_exit_zero` | Inherited reported 04A.09 nonpass |
| ERROR | `test_04a02_checkpoint13.Startup13Tests.test_real_successful_owner_startup_all_pins_and_native_identities` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a02_checkpoint13.Startup13Tests.test_tampered_stdout_hash_stops_before_parser` | Inherited reported 04A.09 nonpass |
| ERROR | `test_04a02_checkpoint14.Actual14Tests.test_actual_two_owner_publications_and_both_cultures_reproduce` | Inherited reported 04A.09 nonpass |
| ERROR | `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a03_control_mitigation.ControlMitigationSourceTests.test_per_mode_consumers_and_branch_order` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a03_control_mitigation.ControlMitigationSourceTests.test_sealed_sources_are_reconciled_never_rewritten` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a03_control_mitigation.NativeGateNegativeTests.test_review_packet_source_pins_match_current_sources` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a03_selection.UnivariateSelectionSourceTests.test_all_native_lifetime_cancel_write_and_reporting_methods_unchanged` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a03_selection.UnivariateSelectionSourceTests.test_only_two_form_methods_change` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a04_alignment.PairedBlockSourceGates.test_exact_crosswalk_and_authorities` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a04_alignment.PairedBlockSourceGates.test_legacy_geometry_ambiguity_is_not_hidden` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_independent_candidate_provenance_not_core_or_golden` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_nonpaired_code_and_unrelated_forms_are_unchanged_from_partial` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a05_completion_successor.Completion05SuccessorTests.test_owner_bound_successor_and_source_hashes` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_historical_ledgers_and_importer_unchanged` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_other_multicol_members_including_04a04_selection_unchanged` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_prior_04a04_whole_file_source_check_remains_frozen_and_disclosed` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_scientific_ambiguity_is_not_a_permanent_portable_wrong_table` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_ui82x2_source_wiring_explicitly_withheld` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a05_contingency.ContingencySourceGates.test_versioned_successor_sources_and_case_boundaries` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_actual_form_routes_and_unchanged_native_tails` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_importer_udf_designers_core_and_old_tests_unchanged` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_native_population_is_union_not_sum` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_new_scientific_candidates_are_explicitly_unmodified` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_no_duplicate_statistics_in_form_or_windows_adapter` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_production_sources_and_exact_open_ledger_lineage` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_core_old_tests_importer_udfs_designers_and_help_unchanged` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_exact_source_and_original_ledger_rows` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_no_fabricated_obligation_for_previously_source_qualified_controls` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a07_workflow_successor.Anova07SuccessorTests.test_restriction_unions_and_timing_exceptions_not_generalized` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a08_authoring_successor.Authoring08SuccessorTests.test_exact_id_crosswalk_retains_original_row_hashes_and_open_counts` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a08_authoring_successor.Authoring08SuccessorTests.test_original_numerics_and_shipping_forms_are_unchanged` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a09_regression_successor.Regression09SuccessorTests.test_exact_six_changed_members_and_unchanged_native_surroundings` | Newly stale frozen source expectation; exact successor evidence, remains FAIL |
| FAIL | `test_04a09_regression_successor.Regression09SuccessorTests.test_real_six_routes_use_common_options_counts_and_reports` | Newly stale frozen source expectation; exact successor evidence, remains FAIL |
| FAIL | `test_04a09_regression_successor.Regression09SuccessorTests.test_runtime_baselines_are_incremental_and_complete` | Pre-existing at live main entry: CRLF receipt/LF checkout pin |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_compile_targets_and_native_helpers_unchanged` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_real_consumer_routes_and_udf_exports_preserved` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a27_geometry_completion.Geometry27CompletionTests.test_source_owner_pins_and_all_44_original_rows` | Inherited reported 04A.09 nonpass |
| FAIL | `test_04a27_transport_successor.Transport27SuccessorTests.test_exact_successor_and_unapplied_patch` | Inherited reported 04A.09 nonpass |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_all_239_ledger_rows_identified_with_original_digest_convention` | Inherited reported 04A.09 nonpass |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_complete_source_crosswalk_equality` | Inherited reported 04A.09 nonpass |
| FAIL | `test_capture_contract04a02.CaptureContracts.test_packet_covers_all_original_cases_and_six_gates` | Inherited reported 04A.09 nonpass |
| ERROR | `test_capture_contract04a02.CaptureContracts.test_packet_retains_additional_21_deferred_obligations` | Inherited reported 04A.09 nonpass |
| ERROR | `test_investigation06.Investigation06Tests.test_all_239_exact_ids_have_reversible_root_cause_source_maps` | Inherited reported 04A.09 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_anova_defaults_mapping_remain_explicit_source_facts` | Inherited reported 04A.09 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_normality_5000_disagreement_preserved_as_review_defect` | Inherited reported 04A.09 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof` | Inherited reported 04A.09 nonpass |
| FAIL | `test_investigation06.Investigation06Tests.test_two_by_two_or_condition_and_no_chart_handler` | Inherited reported 04A.09 nonpass |
