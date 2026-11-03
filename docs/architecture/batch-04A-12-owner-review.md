# Official 04A.12 consolidated owner review

**READY FOR OWNER SOFTWARE ACCEPTANCE REVIEW ? owner decision PENDING.**
One bounded timing disposition: **C12-NATIVE-CLASSIFICATION-01**. All permitted
software extraction and host-free corrections are complete. The shipping fitted-family
guard is tested but **UNAPPLIED**, not represented as a live Windows fix.

**P NO; N1 NO GO; N2 NO; Gate C2 UNMET. Zero native-control releases and zero
original obligation discharges. Excel/COM/UIA NOT RUN. No 04A.13 work.**

## Verified branch, authority and scope

The actual checked-out local `main`, HEAD and local `origin/main` tracking ref were
`90d990c51a691f30a6f90dfdebe5019a4cb24c63`; the worktree/index were clean. Local
tracking divergence was **0/0**, with no server query, fetch or pull. The authorized
`codex/04A-12` branch was created from that main; Git lock creation required the
sandbox's narrowly approved branch-write operation. HEAD remains that base; no
stage, commit, merge, reset, stash, push, deployment or install occurred.

AGENTS.md, installed Master Plan Revision 2.7, accepted 04A.11 source/publication
handoffs, amended 04A.02 decision and original ledgers were inspected. The owner's
current confirmed 04A.11 acceptance/merge supersedes stale plan/dossier pending
language; historical documents were not rewritten. The supplied older ZIP is not
used as live Git authority.

Exactly four existing files change: `Ui13GEE.vb`, the Windows host/test project
compile lists, and the existing GEE MkDocs method page. New Common workflow/report/
contrast policies, the thin Windows adapter, focused tests and evidence/scripts are
listed with hashes in the [publication inventory](../../evidence/04A12/publication-manifest.json).
All **3276** other entry source/document files are protected, including 70 actual
historical filename pins kept privately. Public source maps exclude those sensitive
historical keys and retain 3206 safe protected relative paths.

Core GEE, `geeData`, `GEEcovStruct`, all numerical engines, public UDF sources/exports,
Designers, Ribbon, Office/WASM UX, installer/signing/version files and accepted older
records remain byte-identical. Libraries remain `netstandard2.0`; the Windows host/
legacy tests remain .NET Framework 4.8. No public UDF grammar or estimator is replaced.

## Shipping call chain and decisions

| Stage / actual methods | Common/Core ownership and consumed output |
|---|---|
| Constructor, `GetFamilyCodeFromDisplayName`, `RefreshLinkOptionsForSelectedFamily` | `GeeWorkflowPolicy` lists/delegates the existing Core families, compatible links, working-correlation list and SE ordering. Display/default selections are unchanged. |
| `valiateInputs`, `GetData` | Unchanged dialogs, required roles, intercept-only confirmation, threshold parsing, RefEdit acquisition and `ExcelDnaDataImporter` calls stay in Windows. |
| `geeData.OnDataImported` / `FinalizeGeeImport` / `SubsetByRowIdValues` | Unchanged Core import/filter/sort. Cluster then supplied time; otherwise cluster then original Row ID. All response/effect/offset/weight/time/cluster arrays move together. Subset dictionary insertion order and supplied output IDs are retained. |
| `BuildExpandedRegressionInputs` / `ValidateExpandedInitialValuesCount` | Existing `RegressionDesignCore` expands typed C08 effects. Common supplies implicit-intercept/reference omission and exact expanded parameter count; existing highlight/dialog/refusal logic is unchanged. |
| `RunGEE` -> `WindowsGeeWorkflowAdapter.Capture` -> `GeeWorkflowPolicy.Project` | Reuses `RegressionRequest`, component/vector/options/diagnostic contracts for the marginal mean. GEE-specific immutable intent adds cluster/time axes, correlation, SE, UseP, requested/effective intercept and actual alpha. Physical coordinate sidecars follow retained rows when available; ambiguity remains diagnostic, never a new shipping strict exception. |
| `GeeWorkflowIntent.CreateCoreModel` -> existing `GEE.data/setVarNames/settingInputs` | Configures the existing Core engine with cloned exact matrices/rows/roles/names/starts/options. No localized numeric round trip, row normalization, new parser or solver. Windows retains the actual `Fit` call and progress reporter. |
| Prediction/residual write seams -> `GeeReportPolicy.Predictions/Residuals` | Header/shape/row axes are explicit. Original Core cluster-concatenated predictions are retained and mapped to input rows; this map is **identity for the actual sorted Windows import**. Observed/residual/weight arrays remain on their original input axis. No inverse link is applied again. |
| Original post-Data `GEE.wrapResults` -> adapter `Report` -> `GeeReport.LegacyTables()` | Immutable `AnalysisResultDocument` identity `regression.gee`, exact Core table cells/types/labels/footnotes/p-value annotations/order, working correlation and convergence. Document success is composition, never inferred optimizer success. Existing `ProcessListofResultTables` consumes the document-derived tables. |
| Original post-fit classification controls -> `GeeReportPolicy.Classification` | Accepted `RegressionClassificationPolicy` computes the selected sections; Windows supplies weighted event rate and keeps ROC/calibration charts. Threshold/calibration/Brier choices are read after Fit/GEE output, not borrowed from an earlier control snapshot. The rare family-change compatibility path is quarantined separately in Windows pending C12 activation. |

`Data`, `GEE`, optional `GEE Classification` sheet names, writer-call order, original
Data column order, post-fit control-read boundary, chart/error/finally tail and
callback cancellation/close lifecycle are source-pinned and exercised where host-free.
No GEE form constructor or live workbook/chart workflow was executed. Actual enabled/
visibility transitions and rendered outputs remain native **NOT TESTED** boundaries.

The live UI initially selects Binomial / Logit / Independence / Robust; maximum
iterations 50, epsilon 1e-6, UseP/residual/iteration-detail/trace unchecked, power text
1 and blank NB dispersion. Alpha comes from the existing global-default helper,
otherwise .05. Common records requested alpha as actual GEE fit alpha; C11's separate
LM .05 native limitation is untouched. Core always inserts an intercept even if the
GEE checkbox is cleared. The trace checkbox has no per-model assignment in the old
run, so its request metadata does not falsely claim to change Core logging.

Core's positive supplied dispersion property normalization is preserved. NB dispersion
is used by the NB family; a positive property supplied for another family is retained
with an unused-family diagnostic. Empty vs missing arrays, vector/row/key collisions,
unsupported links/options and overflow-prone controls have scoped diagnostics. They
are not converted to new Windows `Blocking`/`IsValid` throws. Old visible refusals and
Core exceptions remain in their existing host paths.

## Numerical/semantic evidence and limitations

[The baseline registry](../../evidence/04A12/baseline-registry.json) pins **30 actual
pre-edit Core cases per runtime**, two cultures and **120 captures**. The numerical
engines/importer were unchanged during capture. The cases cover all five working
correlations with Robust/Naive/Bias Reduced SE, irregular visit labels, absent time,
intercept-only, iteration limit, supplied starts, power link, Gaussian, binomial
logit/probit, Poisson, NB and Gamma. Inputs are deterministic synthetic data; original
source IDs are noncontiguous and import ordering is explicit.

After projection, **116 completed-report captures match**; four captures preserve the
same valid-start Core exception. Of the completed captures, Core reports convergence
in **112** and nonconvergence in **four**; no result presence is used as convergence.
Numeric typed cells use `max(1e-9, abs(expected)*1e-9)`; labels/types/metadata/order are
exact within culture/runtime, with only elapsed computational time masked. Additional
round-trip invariant vectors compare exactly. The three expanded raw/filter/sort/Core
input trace files were emitted later from the unchanged source-pinned fixture/importer;
they are not represented as earlier-created receipts.

The [comparison receipt](../../evidence/04A12/numerical-comparisons.json) records
actual QIC/QICu, correlation/table dimensions and statuses. Existing committed Core
GEE numerical-reference tests also pass; no new external R reference was needed or
executed. All captures remain **NOT_GOLDEN**, not an independent scientific review.
Weights are accepted/echoed and used for classification when selected, but do not
enter current GEE estimating equations. Offset semantics and all three SE/covariance
choices remain the original Core implementation. AR(1) uses within-cluster positions,
not continuous numeric gaps. Duplicate-time tie order is not promised stable.

[Issue records](../../evidence/04A12/issues.json) disclose:

- **C12-GEE-START-QIC-01:** valid supplied starts bypass the automatic-start GLM's
  independence covariance initialization; `EstimateQIC` can throw `NullReferenceException`.
  Constant-binomial fixtures also encounter unavailable starting-fit inference. This
  pre-existing Core failure remains, with absent-result fault documents and no invented
  QIC, SE, p-value or successful fit. Automatic-start subsequent fits recover. Core/
  numerical reviewer follow-up needs separate authority; 04A.40 owns consumer parity.
- **C12-GEE-CLUSTER-TEXT-01:** default GUI numeric import drops nonnumeric cluster labels;
  Core can accept text labels via compatible input. Numeric-string ordering and scalar
  text-label collisions are not silently recoded. No acquisition fix is activated.
- **C12-GEE-PREDICTION-AXIS-01:** Core/UDF predictions and observed/residual axes differ
  for interleaved direct input. Common exposes both and aligns via an explicit map;
  shipping sorted data has identity mapping. UDF export reconciliation remains 04A.40.
- **C12-NATIVE-CLASSIFICATION-01:** portable actual-fitted-family selection is correct;
  the Windows legacy selected-family mismatch activation is pending as detailed below.

Stress fixtures compare old direct Core against the intent path for constant responses,
singular designs, one cluster, singleton AR clusters and duplicated-time unstructured
correlation. Existing failures and nonfinite numerical payloads remain actual outcomes;
a completed small-sample fit is not certified as valid sandwich inference. Typed plans
also cover missing/empty starts, wrong dimensions, duplicate/misaligned IDs, nonfinite
vectors, invalid links/correlation, missing output/header axes, source coordinates,
cloning, culture, cancellation/recovery and post-fit classification control changes.

## Existing contrasts and worksheet parity

**NO_SHIPPING_GUI_CONTRAST_PATH**: no GEE contrast panel exists or was added.
Existing `GEE_LSMESTIMATE` observed-profile/time/AT handling and public exports remain
unchanged. `GeeContrastPolicy` projects already resolved observed-design profile
indices/weights into immutable L rows using the existing matching/order conventions;
it does not implement new Wald inference, response links or a formula parser.

Host-free tests seed an isolated private cache with a real Core model, then execute
existing SUMMARY/WCORR/VCOV/RESID/PRED/LSMESTIMATE post-fit APIs and cleanup. They never
call the fit export's Excel-function-wizard path or construct a GEE form. Formula
prediction metadata comes from the existing Core formula-design service, not a fake
grammar. Normal-reference Wald inference, covariance selection, inverse-link of the
combined L-beta value and exclusion of offsets from the observed design contrast are
characterized read-only. GUI/UDF defaults differ (GUI 50/1e-6 versus UDF 20/1e-8);
there is no incidental public UDF rewrite. Remaining parity duties are 04A.40.

## One exact native timing disposition

The original GEE inventory has **87 controls**, **35 OPEN original obligations**, **34
restricted controls** and 52 source controls with no original deferred-obligation row.
There are 39 controls associated with changed methods. The [exact crosswalk](../../evidence/04A12/restriction-crosswalk.json)
contains every ID, original control/obligation row hash, source-record hash and retained
status. The separate UiGLM seven obligations / 34 controls are not borrowed as GEE IDs.

Source plus a host-free state-change fixture reproduce an old mismatch: the Family
selector can differ from the captured fit by the original post-fit classification
boundary. Common uses the fitted family. Windows preserves its old selected-family
behavior in `DeferredPostFitFamilyReport` solely as a bounded compatibility quarantine.
A one-line fitted-family guard is prepared; normal binomial output, control-read timing
and chart/writer order remain the same after that guard. The real WinForms transition
is **NOT TESTED** here.

The [unapplied patch](../../evidence/04A12/C12-NATIVE-CLASSIFICATION-01.UNAPPLIED.patch)
passes `git apply --check`; its exact full patched form compiles via an isolated source
override. No constructor/native writer runs and the shipping source still has its
original outer UI predicate. Current source SHA: `80a276c182f1bcc8a582ae9de90a7c05b01b912fe085504b5f3170eb7199206b`.
Patch SHA: `d1d525bbf6ebff2e371de4b8f6cb43689d4790ec2c4297da9fc29c08e4a58540`. Exact original affected controls, obligation IDs/row hashes,
named fixtures and the retained pre-change gate are in
[native-disposition-lineage.json](../../evidence/04A12/native-disposition-lineage.json).
No original GEE representative golden case/criterion ID is invented where the accepted
04A.02 representative packet supplies none.

**Proposed single owner decision:** accept completed 04A.12 software scope with
**C12-NATIVE-CLASSIFICATION-01 activation deferred** until original pre-change evidence,
case-specific independent risk/N authority or a fresh exact-ID timing exception is
available. Retain the portable correction and narrow Windows compatibility quarantine.
Do not appropriate the earlier 2x2/importer exceptions.

C09 strict validation and C11 LM-alpha/covariance-header patches remain UNAPPLIED at
their accepted hashes. Active C10 constant-binomial safeguards and Core name cloning
remain untouched. All 1514/802 original obligations/controls, 239/218 transferred
and other overlapping subsets remain OPEN/restricted; never add them. P NO / N1 NO GO /
N2 NO / C2 UNMET. HWND/PID identity, loaded XLL/assembly attribution and independent
Windows/COM review remain later 04A.45-48 prerequisites. Zero releases/discharges.

## Final validation and actual failure accounting

| Distinct cases | Debug passed / total | Release passed / total |
|---|---:|---:|
| Managed logical cases (shared source/data-row identity deduplicated) | 4399 / 4405 | 4399 / 4405 |
| Core assembly | 1058 / 1058 | 1058 / 1058 |
| Common assembly | 1541 / 1546 | 1541 / 1546 |
| Windows assembly | 1473 / 1475 | 1473 / 1475 |
| Office C# managed | 505 / 505 | 505 / 505 |
| Office execution C# managed | 45 / 45 | 45 / 45 |
| Retained C03 control projection | 61 / 61 | 61 / 61 |
| Node (Office 468 + Help 71 + WasmSmoke 377) | 916 / 916 | 916 / 916 |
| Python distinct methods | 665 / 730 | 665 / 730 |

No skips. Assembly rows contain shared linked cases; do not sum them as unique tests.
Runtime executions are 4683/4690; logical cases merge only exact shared test-source
hashes and unambiguous class/method/data rows, normalizing MSTest String quoting only.
All **140 new logical managed cases pass**, including 103 Common / 137 Windows
executions and three Core cases. Ten new Python successor checks pass in each full run.
Focused repeats and the explicit CJS confirmation (377/377) are excluded from totals.

The six logical managed nonpasses match the executed entry baseline: three
`PairedDisjointCountContractAndPureSemanticProjection` data rows, the existing paired
integer-overflow expectation, the Windows paired-compatibility expectation, and the
constant-binomial True case still expecting the old NullReferenceException. The last
is one logical case/two runtime failures. No historical assertion was changed.

Python has **55 FAIL / 10 ERROR = 65 distinct nonpassing methods** (subtest events are
not inflated into methods). Full identities/outcomes and collision ledger are in
[validation.json](../../evidence/04A12/validation.json). The live pre-edit standard
roots executed **498/552**, 54 nonpasses; the additional 168-method Common semantic
root is separately identified, not falsely claimed as an entry execution. Compared
with the prior 61-nonpass receipt: 60 remain nonpassing; one untouched git-diff scope
guard already passes at merged entry because the earlier Core edits are committed.
That does not erase its historical failure or fix statistical obligations. Four
publication/raw-path/branch snapshot nonpasses were already present at live entry.
One new expected source-pin failure is:
`test_04a11_publication.Publication11Tests.test_accepted_dossier_and_dormant_patches_unchanged`.
It pins the Windows test project before this batch's approved compile inclusions;
the C12 scope/private-pin successor proves unaffected source, old assertions, dossier
and patches remain unchanged. It is still reported FAIL, never rebaselined. No
unexplained new functional failure remains.

Executed full commands (both configurations):

```text
<PYTHON_EXE> -B scripts/verify-04a12.py final --evidence TestResults/04A-12-20261103-run01 --configuration Debug --name final-Debug
<PYTHON_EXE> -B scripts/verify-04a12.py final --evidence TestResults/04A-12-20261103-run01 --configuration Release --name final-Release
```

Both exit **1** for the disclosed managed/Python nonpasses. These run source-hashed,
diff-recorded execution copies of `verify-beshstatng.ps1` with portable/Legacy/WASM/JS/
Python flags, VS 2026, no restore, fresh results and safe compile targets. The literal
script is not directly run: fixed historical writers, restore and host packaging hooks
conflict with retention/offline/no-release restrictions. Original script bytes and all
assertions remain intact. Six managed suite TRX paths/hashes per configuration and
exact sanitized commands/exits are in the validation and command receipts. Raw originals
remain in ignored local evidence; public command derivatives do not claim raw hashes.

Explicit `node --test BESHStat.WasmSmoke/tests/*.test.cjs` exits 0 (377/377). Historical
strict and profile checkers exit **1/1**: strict stops on an inherited stale UibyID /
NormalPlot review dependency; profile reports INTEGRITY_FAILURE. These are retained
checker integrity nonpasses, not a new GEE numerical failure and not an assertion that
all historical checks passed. Development compilation/fixture metadata issues were
corrected and focused reruns passed; no full matrix rerun was needed after completion.
No R, install, Excel/COM/UIA or native harness was executed.

## Documentation, retention, privacy and later manual smoke

[Documentation impact](../../evidence/04A12/documentation-impact.json): one existing
GEE method page clarifies actual sorting/text-ID/trace/starts/residual limitations and
existing contrasts. Method slug/catalog, Help resolver/links and screenshots are
unchanged. Installed strict MkDocs build exits **0**; both full configurations execute
Help/link/build tests. Office has no executable GEE task pane; no invented Online steps.
Screenshots and real enabled-state/render validation remain **NOT RUN / NOT AUTHORIZED**.

Retention **PASS**: **185334 original root TestResults files**, zero membership/metadata/
source-pin errors, 530549166080 bytes free at the receipt. Scope covers complete initial
membership, sizes, creation/write nanoseconds and file identities, plus frozen source
hashes; it is not a full byte audit of historical results. All older accepted evidence
and local raw originals stay in place, without deletion/overwrite/compression/relocation.
A public Git clone or ZIP cannot independently verify excluded historical trees/private
ledgers; local originals and protection receipts remain necessary.

Complete changed/new candidate paths, byte hashes, trackability and privacy verdicts
are in the publication manifest/privacy receipt. Historical filename keys are excluded
from new public maps, not replaced with filenames falsely claimed to exist. Public
fixtures are synthetic; raw TRX/logs, baseline.json, private originals/mappings, local
ledgers, build outputs and caches are excluded. Root .gitignore is unchanged. Scan
assurance is patterns plus targeted review, not proof against every possible secret.
Final verification passes: **162/162 public candidate privacy/trackability checks**,
**161/161 external SHA-256/byte counts**, with the manifest excluded from its own
hash map and included in privacy/trackability scope. Four reviewed false-positive
groups are public product/fixture naming and unchanged shared Windows SDK tool paths;
zero unresolved findings. Existing signing targets/fingerprint are unchanged and were
not executed. Current status: **four tracked modifications / 158 untracked candidates**;
index empty, all unstaged/uncommitted. `git diff --check` passes.

Later manual smoke **NOT RUN / NOT AUTHORIZED**:

1. After native prerequisites/exact authority, use a disposable copy of the existing
   GEE sample; compare numeric Cluster/Visit sort, retained source Row IDs, offset/weight
   columns and predictions/residuals. Expect Data then GEE output with original names.
2. Check AR(1) with irregular visit labels and robust/naive/bias-reduced SE; distinguish
   position-based lag from continuous-time inference. Leave starting values blank for
   the known Core QIC limitation; do not call host-free parity a native golden.
3. Fit binomial GEE with custom threshold, threshold table, calibration bins, Brier,
   ROC and calibration chart; verify GEE Classification writer/chart order and toggle
   behavior. Family switching and the dormant guard need the exact native disposition.
4. Exercise interrupt/cancel/close and residual export under a later owned Excel process;
   confirm callback cleanup and actual form state without releasing controls here.

**Owner decision requested only once:** software acceptance with the bounded C12
activation timing deferral above. No staging/publication, native/P release or next
numbered batch is authorized by this handoff.
