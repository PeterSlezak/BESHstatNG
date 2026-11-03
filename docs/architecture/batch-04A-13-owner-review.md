# Batch 04A.13 — consolidated owner review

**Software implementation complete; owner acceptance pending.** Changes are on
`codex/04A-13`, unstaged and uncommitted. Native Excel/COM/UIA, rendered charts,
screenshots and N1/N2 captures are **NOT RUN / NOT AUTHORIZED**. P remains NO,
N1 NO GO, N2 NO and Gate C2 UNMET. No control release, obligation discharge,
deployment, signing, installer, Git publication or 04A.14 work occurred.

## Verified entry and scope

Local `main` and entry HEAD were both
`f7d1c693eadda871c82807947922b2fa8596b87b`, the merged 04A.12 GEE extraction.
`git status --porcelain=v1` was empty; `git branch --show-current` returned `main`.
The 04A.12 branch is an ancestor of main. Locally cached `origin/main` divergence
was **0 ahead / 0 behind**; no fetch or remote contact was made. The branch was
created from that verified local main. Git required its existing command-local
`safe.directory` ownership exception and sandbox approval to write the branch/index
lock; global configuration was not changed. HEAD and main remain at the entry commit.
The prompt's older ZIP was not used as a baseline.

Authorities reviewed: root `AGENTS.md`; Master Plan Revision 2.7, including §2C,
04A.13 and retained issue owners; 04A.12 dossier and live merged GEE sources;
04A.01 ledger/owner approvals/closeout; dated 04A.02 amended owner acceptance and
its immutable exact-hash proposal. The owner's current acceptance/implementation
authorization supersedes older pending/next-batch language in those records. The
original pre-change native gate remains in force; no general timing exception exists.

[Pre-change manifest](../../evidence/04A13/prechange-manifest.json) records selected
source pins, disk headroom and protection scope. The full private baseline pins all
initial tracked/existing docs files; the compact public manifest does not expose
historical private filename keys. [Source inventory](../../evidence/04A13/source-inventory.json)
lists inspected survival engines/forms/Designers/renderers, UDF exports, transport,
longitudinal input and established workflow/report implementations. Original tests
and Core source remain unchanged.

Eight existing paths changed:

- `BESHstatNG/src/UI/Ui4KMandLogRank.vb`
- `BESHstatNG/src/UI/Ui4Cox.vb`
- `BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb`
- `BESHstatNG/BESHStatNG.vbproj`
- `BESHStatNG.Test/BESHStatNG.Tests.vbproj`
- `BESHStatNG_Help_MkDocs/docs/methods/kaplan-meier-plot.md`
- `BESHStatNG_Help_MkDocs/docs/methods/logrank-test.md`
- `BESHStatNG_Help_MkDocs/docs/methods/cox-regression.md`

Added implementation: Common `Analysis/SurvivalWorkflowPolicy.vb`,
`Analysis/CoxWorkflowPolicy.vb`, `Results/SurvivalReportPolicy.vb`; Windows
`src/DM/WindowsSurvivalWorkflowAdapter.vb` (KM and Cox bridge classes).
Added tests: Common `Survival13Baselines.vb`, `Survival13WorkflowTests.vb`,
`Survival13ReferenceTests.vb`, and Windows `Survival13AdapterTests.vb`.
Added tooling: `scripts/verify-04a13.py`, `capture-04a13-contract.py`,
`summarize-04a13.py`, `verify-04a13-publication.py`, `reference-04a13.R`,
`scripts/tests/test_04a13_survival_workflow.py`, this dossier and `evidence/04A13/`.
No paths were deleted. The [publication manifest](../../evidence/04A13/publication-manifest.json)
is the exact expanded path/byte/SHA-256 inventory, including every individual capture.

## Shipping delegation and retained host ownership

| Shipping entry/seam | Common delegation and original numerical authority | Windows ownership retained |
|---|---|---|
| `btCompute_Click` → `RunKM`; `RunLogrank` → `RunKM` | `WindowsSurvivalWorkflowAdapter.Capture` constructs `SurvivalWorkflowIntent`; `ExecuteAcquiredRecords` calls original `Survival_KM_LR` in the original semantic order: weighted test, Brookmeyer–Crowley median CI, optional detailed table, median equality, two-group fixed-time comparison | RefEdit validation/focus, same-sheet checks, optional `"0"` synthesis, existing integer conversion and Core error dialog; original `getData`/`checkInputs`/`GetResultWriter` remain unchanged |
| KM results | `SurvivalReportPolicy.Compose(intent, LRKM.wrapResults())` → immutable `AnalysisResultDocument` → `LegacyTables()` using the existing table bridge | `ProcessListofResultTables`, totals/blank separators, output-range/new-sheet/new-workbook selection, overwrite prompt and native writer |
| KM plot, after writing | `SurvivalReportPolicy.Plot` snapshots original `GetKaplanMeierPlotData(alpha)`; `ToCoreData()` supplies identical arrays and requested presentation options | Payload overload calls the unchanged renderer body. Series indices, clipping, censor marks, CI styling, legend/title/units and COM remain Windows-owned. Existing analysis overload/`AddKMplot` extension remain available |
| Cox `GetData`/`BuildExpandedCoxInputs` | Existing `RegressionEffectsController`/04A.08 authoring and `RegressionDesignCore.GetRequiredRawVarKeys`/`BuildExpandedPredictorMatrix` remain authoritative; Common supplies categorical reference omission | Workbook/worksheet selection, column keys and raw acquisition stay in the original form methods |
| `BuildExpandedSurvivalRecords` and starts | `CoxWorkflowPolicy.BuildExpandedRecords` copies retained times/status/strata/RowIds and exact expanded X rows; `InitialValuesCountMatches` preserves existing post-expansion count semantics | Original numeric parsing, highlight/dialog text, refusal and validation timing remain; no raw-X count is substituted |
| Cox `btCalculate_Click` fit | `WindowsCoxWorkflowAdapter.Capture` → `CoxWorkflowIntent.CreateAcquiredModel`; original Core constructor/options/starts; exactly one shipping `cox.Fit(m, WinFormsProgressReporter)` | `BeginRegressionComputation`, cancel/interrupt callbacks, form close/finally cleanup and exception presentation remain unchanged |
| Cox report and adjusted curves | `CoxReportPolicy.Compose` snapshots original `cox.wrapResults(strataName, alpha)` and actual `CoxResult` convergence. `Curve` snapshots original `ComputeBaseline(False)`/`BaseSurvivalForPloting` values per original dictionary iteration | New workbook; **Cox Regression → Adjusted Curves → Residuals**. Formula strings, editable locations, comments, first-stratum selection, `PlotCox`, `SeriesCollection(count)` and all sheet/writer operations retain their original order |
| Cox residual export | `CoxReportPolicy.Residuals(intent, cox.wrapResiduals())` supplies cloned original blocks and aligned RowId/time/status/expanded-predictor sidecars | Same columns, two-row headers, block order and native writes; no new residual graph |

These are the actual shipping calls, not unused portable implementations. Source
contracts test the handler calls, one-Fit boundary and original lifecycle; host-free
Windows tests execute the real adapters, Core models and memory-writer seams. The
KM renderer body from `noGroups` onward is source-equal after line-ending
normalization. Cox formula/comment/series-index lines and writer-call sequence are
source-equal. [Successor provenance](../../evidence/04A13/source-delta.json) records
changed and unchanged member pins and verifies every other baseline source/evidence
pin. No native behavior observation is inferred from these checks.

All four existing survival exports (`LOGRANK_P`, `LOGRANK_STAT`, `MEDIAN_CI`,
`KM_TABLE`) and seven Cox exports (`COX_FIT`, `COX_SUMMARY`, `COX_TESTS`, `COX_RESID`,
`COX_BASELINE`, `COX_PRED`, `COX_DROP`) retain their signatures, source bytes,
result shapes and session-handle behavior. Full request migration remains 04A.40/43.
Framework targets and 32/64-bit Excel-DNA packaging are unchanged.

## Typed semantics and compatibility

Requests defensively copy numeric roles, ordinal labels, rows, expanded design,
effect metadata, starts and option builders. Reports copy Core tables through the
existing `AnalysisResultDocument`/table bridge; plots and residuals return detached
array copies. Common adds no parser grammar, estimator, executable Excel formula,
COM handle, DataObj, file access, WinForms or Office.js dependency.

`SurvivalInput` reuses `RegressionVector`, `RegressionComponent`, immutable effect
authoring and accepted `SpreadsheetTransportFrame`. The existing longitudinal
survival input has Boolean event state and cannot express invalid status diagnostics;
the new numeric-status snapshot retains those invalid values. Original transport
areas and explicit logical role-column mapping are retained when available. Source
lookup never reconstructs a first-area rectangle or fabricates coordinates for raw
arrays. Cox expanded `SurvivalRecord.Index` equals the retained RowId; residual
sidecars/X/events retain exactly that row order. Original KM Core records are supplied
unchanged on the compatibility seam; their historically unused Index is not silently
redefined as physical provenance. The typed request carries KM source identity.

| Subject | Previous versus current behavior |
|---|---|
| Coding/time | Required convention remains **0 censored / 1 event**, finite nonnegative time; zero is valid. Strict portable diagnostics reject fractional/-1/2/missing-vector/nonfinite input, negative time, shape/row collisions and missing selected labels. Diagnostics are bounded at 128. No new Windows `IsValid` refusal is activated |
| GUI/UDF conversion | KM GUI keeps its rounding conversion; UDF binary helper truncates before exact-code fallback. The UDF's `.5` status can therefore become censored; portable validation rejects it. GUI invalid/missing acquisition and UDF row filtering remain distinct |
| Labels/strata | GUI/Core literal labels preserve case/culture/first-seen order. `CreatSurvivalData` maps group/stratum labels to first-seen numeric IDs while retaining text fields; Cox uses literal `Stratum`. KM_TABLE UDF trims and groups ignoring case; other survival UDF paths have their own literal-after-trimming conventions. No normalization was introduced |
| Five request states | KM no group, KM grouped, grouped log-rank, stratified log-rank and stratified Cox are explicit. Log-rank requires selected group; absent KM group/strata remain `"0"` synthesis. Selected Help mode remains separate from the shared calculation path |
| KM options | Exact Core weight names: `Logrank`, `Tarone-Ware`, `Gehan-Breslow`, `Peto`, `modified Peto`. Unknown options diagnose without standard-test fallback. Median/CI is always requested by Windows; detailed output, median equality and fixed-time requests remain separate. One group skips weighted/median-equality tests; fixed-time computation requires two groups. The legacy two-group wrapper still emits a heading-only fixed-time table when unchecked |
| Presentation | Alpha is captured from the form, normally preference/default .05; .05/.10 tested. Plot CI/legend default on, title display off, units days; selected output target is neutral metadata. Presentation controls are read at the original post-writer plot boundary |
| Cox effects/starts | Required raw keys retain aliases such as `cat | VarD`; Core expands categorical/interactions with reference omission. Cox has no intercept. Supplied-start validation uses expanded coefficient count; empty versus absent starts remain distinct |
| Cox options | Breslow default, Efron/Exact typed Core enum; robust, returned covariance, all residuals, PH score, iteration details, trace, alpha, max iterations (GUI 50) and epsilon (GUI 1e-6) are carried without numerical reimplementation. Existing Core fallback/error behavior remains on shipping compatibility calls |
| Import discrepancy | Cox raw-matrix/UDF import allows text strata. Shipping `LoadCoreDataTable` with default CharCols can exclude them. `DataObj` derives its row axis from physical RawData rather than arbitrary supplied RowIds metadata. Accepted KM True/multi-area physical intersection remains intact; no new geometry shortcut |
| Faults/undefined outputs | Core all-censored/median-not-reached, singular/no-event/no-convergence and plotting faults remain actual outcomes. Document composition is not optimizer success. Cox convergence comes from `CoxResult`, and an absent-result fault has no invented coefficients or successful convergence |

Original Core negative-time error wording (including its typo and reported status
value) remains unchanged. Missing/error/date interpretation remains host-specific.
No left truncation, start–stop, time-varying predictors, competing risks, Aalen–Johansen
or Fine–Gray implementation was added.

## Numerical and structured-output evidence

[Frozen registry](../../evidence/04A13/baseline-registry.json): **37 original Core cases
per runtime**, invariant/Slovak cultures, portable .NET and .NET Framework 4.8,
**148 pre-change captures**. The first fixture-only compile errors were corrected
before capture. Production Core/forms were unchanged during capture; only the test
project's new fixture inclusion was needed for the Windows capture. No capture was
overwritten or promoted. Subsequent tests compare original and projected typed cells,
labels/order/headers/footnotes/p-value metadata, residual blocks and curve arrays.
Tolerance is `max(1e-9, abs(expected)*1e-9)` for floating cells; text/types/axes and
fault types are exact. Only elapsed computational-time text is masked.

There are **132 complete captures and 16 captured faults**: four runtime/culture
copies each of the all-event KM plot-data fault, Cox no-event-stratum fault, all-no-event
Cox fault and singular-design fault. Cox reports 52 converged and 12 nonconverged
captures; 12 fail before usable inference. A separated-design fixture's reported
convergence is retained, not certified as scientifically valid inference.
[Numerical comparison report](../../evidence/04A13/numerical-comparisons.json) records
each table shape, selected fields and failure status. Memory-writer comparisons
preserve placement plans and separator strategy without executing Excel.

Executed independent candidate: R **4.5.1**, `survival` **3.8.3**, explicit 0/1 coding,
all three Cox tie modes and stratification, KM log-log 95% reference selection, and
`survdiff(rho=0)`. **18/20 Cox/log-rank scalar comparisons agree**; all **19 observed-time
KM survival estimates agree**. Cox tolerances are 5e-7 for coefficients and 1e-6 for
log likelihood; KM survival tolerance 1e-12. These allow optimizer/CSV precision and
do not authorize changed inference. Four inherited R LC_* startup warnings occurred.
An initial R fixture had incorrect modulo precedence and no-convergence warnings;
its unqualified output is retained privately and excluded. The corrected fresh
[candidate CSV](../../evidence/04A13/R-candidate-reference-v2.csv) is used by the
reference tests. Its script/output hashes and exact qualification are recorded.

The **two log-rank nonmatches are OPEN scientific candidates**:

| Fixture | Original Core chi-square | R rho=0 chi-square | Absolute difference |
|---|---:|---:|---:|
| Grouped | 0.9623537488337257 | 1.01806251989924 | 0.0557087710655142 |
| Stratified | 1.0965259390388649 | 1.15607908970711 | 0.0595531506682452 |

Both exceed 1e-6. The synthetic 24-row recipe and precise sources are in
`C13-LOGRANK-R-01` in the [issue register](../../evidence/04A13/issues.json).
Investigate risk-set/variance conventions through scientific review; Core inference
is unchanged. All five weighted modes still match their own original Core paths.
No name-based equivalence is claimed for weighted variance, median CI, exact-tie
baseline conventions, PH/residual scaling or dynamic adjusted Excel formulas.
All new/old candidate references remain **NOT_GOLDEN**; no P/C2 advancement.

## Exact original native-control lineage

[Restriction crosswalk](../../evidence/04A13/restriction-crosswalk.json) enumerates
all **103 original controls**, original control/obligation row hashes, source record
hashes, source/Designer/member pins, original owner/fixture/dependent batches and
retained verification/authorization statuses. Source association includes before and
after changed members, including newly observed output-target metadata reads.

| Original form | Controls | OPEN original obligations | Restricted controls | Source controls associated with changed members | Controls with no original obligation |
|---|---:|---:|---:|---:|---:|
| Ui4KMandLogRank | 39 | 6 | 5 | 18 | 33 |
| Ui4Cox | 64 | 6 | 5 | 17 | 58 |

Exact original obligation IDs retain the `control:BESHStatNG.` prefix:

- `Ui4KMandLogRank.btnHelp`, `.RefEditOutput`, `.RefEdit1_SurvivalTime`,
  `.RefEdit2_Censor`, `.RefEdit3_GroupID`, `.RefEdit4_StrataID`.
- `Ui4Cox.lblProgress`, `.btnHelp`, `.btCalculate`, `.ProgressBar1`,
  `.ckResidualPlots`, `.btInterrupt`.

The JSON includes the full IDs and per-row hashes; the separate source-only lists
do not invent obligations for other controls. Historical codebehind pins are
`63422aae7d7410b12cf5628dbe9704a768d5864f0c416e0d0715aa6930a37916`
(KM) and `69889949268e29a3a663d915e4f54f65b70ba77d7b51f0f3d14d94e4c3fc5d48`
(Cox). Original Designer pins are
`27c442208e2f3516af077b4908cfee972e9117f36b037b2392645a59f207908c`
and `bef4039467e46293f83dd4e4c195976fc4a0167713d527bd169c7ff8394a8b90`.
These historical records were not rewritten; current merged-before/successor hashes
are separately recorded in source-delta. A changed whole-file hash is never asserted
equal to the historical pin.

The wider **1,514 obligations / 802 restricted controls**, accepted 04A.02 **239/218
transfers** and other overlapping populations remain in force, with **zero releases
and zero discharges**. They are not additive populations. GEE's 35 obligations and
UiGLM's seven obligations are not survival obligations. Native risk review,
PID/HWND/source/loaded-XLL identity and exact pre-change evidence remain later gates.

`C09-NATIVE-STRICT-01`, `C11-NATIVE-CORRECTIONS-01` and
`C12-NATIVE-CLASSIFICATION-01` remain **UNAPPLIED** at their accepted byte hashes;
all three `git apply --check` checks pass. GEE policies/adapter/Ui13GEE and its outer
selected-family predicate are unchanged. No shared helper activates any candidate.
This behavior-preserving batch requires no new dormant patch or timing waiver.

## Retained issues and downstream owners

The [issue register](../../evidence/04A13/issues.json) distinguishes scientific
candidates, existing input discrepancies, native wiring and retained boundaries:

- Disabled/unwired `ckResidualPlots`: **04A.36** payload reuse; **04A.45–47** exact
  native wiring/disposition, **04A.48** reconciliation. The original removal approval
  was not silently implemented, and no graph was added.
- Status/label/missingness and physical versus positional provenance: **04A.39–41**,
  with native **04A.45–48**. No source group is normalized away.
- Cox text-stratum acquisition and raw/import discrepancies: **04A.39–41**,
  native **04A.45–48**. Expanded aliases/reference omission/starts/RowId alignment
  are integrated and tested here; established numerical fault candidates remain open.
- KM no-censor plot-data failure and incomplete censor-count axis: **04A.36**,
  scientific/consumer **04A.40–41**, native **04A.45–47**. Arrays and renderer indexing
  are preserved rather than correcting a restricted shipping chart.
- Cox no-event-stratum baseline, singular/no-event inference and the two independent
  log-rank nonmatches: scientific/consumer **04A.40–41**, separate P review, native
  **04A.45–48**. No numerical correction is hidden in this extraction.
- Adjusted-curve formulas, first-stratum selection, editable cells/comments and
  `SeriesCollection` convention: **04A.28**, **04A.36**, **04A.45–47**. Common's neutral
  descriptor explains intent but cannot execute workbook formulas.

## Validation and command accounting

The live merged entry remeasurement was Core **1058/1058**, Common **1541/1546**,
Windows **1473/1475**. Python with comparable semantic observations was **664/730**,
**56 FAIL / 10 ERROR**. This is one more failure than the previous 04A.12 final
receipt: `test_04a12_gee_workflow.Gee12WorkflowSuccessorTests.test_exact_source_scope_and_all_old_byte_pins`
already fails at merged entry because its frozen C12 branch/diff scope is no longer
the live batch. It was not repaired or rebaselined.

| Full distinct cases | Debug passed / total | Release passed / total |
|---|---:|---:|
| Managed logical cases, exact shared sources/data rows deduplicated | 4531 / 4537 | 4531 / 4537 |
| Core assembly | 1058 / 1058 | 1058 / 1058 |
| Common assembly | 1662 / 1667 | 1662 / 1667 |
| Windows assembly | 1605 / 1607 | 1605 / 1607 |
| Office managed | 505 / 505 | 505 / 505 |
| Office execution managed | 45 / 45 | 45 / 45 |
| Retained C03 projection | 61 / 61 | 61 / 61 |
| Raw managed runtime executions | 4936 / 4943 | 4936 / 4943 |
| Node: Office 468 + Help 71 + WasmSmoke 377 | 916 / 916 | 916 / 916 |
| Python distinct methods | 678 / 744 | 678 / 744 |

All **132 new logical managed cases** and **14 new Python methods** pass in both Debug and Release.
Shared portable/Windows executions are not added twice. No skips. Focused repeats
and the explicit CJS confirmation are excluded from full denominators. The completed
[validation receipt](../../evidence/04A13/validation.json) supplies each configuration,
six TRX hashes/paths, raw counters, exact nonpass identities and collision ledger.

Six inherited logical managed failures remain: three
`ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection` rows
`(20,15,40)`, `(0,0,0)`, `(20,30,50)`; the paired overflow expectation
`ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`; Windows
`PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`; and
`Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings(True)`
(one logical case failing under two runtimes). All historical assertions are intact.

The retained final Python nonpasses remain **56 FAIL / 10 ERROR**, with no new
identity. All 66 inherited Python nonpasses are individually named in
[entry-validation](../../evidence/04A13/entry-validation.json) and the final receipt.
They include historical source-pin/branch/scope expectations, disclosed paired/
RefEdit/source-contract cases and missing local historical proof dependencies.
Multiple subtest events remain attached to one real method. The new successor verifies
the authorized survival deltas without erasing old failing assertions. Both configurations have
**no new nonpass identity**. The [whole-file pin crosswalk](../../evidence/04A13/frozen-pin-conflicts.json)
records every approved merged-before/successor hash and earlier authorities containing
those exact pins. Already-failing frozen tests can stop before later pin checks; no
claim that such checks became green is made.

| Command/workflow (portable aliases, not literal machine paths) | Outcome/artifact |
|---|---|
| `git status --porcelain=v1`; `git branch --show-current`; `git rev-parse HEAD`; `git rev-parse main`; ancestry/local divergence; `git switch -c codex/04A-13 main` | Clean verified entry/branch; local origin divergence 0/0; no remote operations |
| `PYTHON_EXE -B scripts/verify-04a13.py init --evidence TestResults/04A-13-20261103-run01` | Exit 0; initial protection/metadata/private pins |
| Same runner `entry --name entry-Debug`; standard Python entry plus `entry-merged-python`; semantic and `common-python --name entry-comparable` | Actual entry failures above; raw probes retained, including nonexistent first Common discovery path and missing observation environment |
| Same runner `capture --name prechange-Core-03`; `capture --name prechange-net48 --windows` | 37/37 each runtime; 148 frozen culture/runtime captures. Two earlier fixture compile attempts retained |
| Same runner `focused --name contracts-final --windows` | 121/121 Common and 132/132 Windows executions; earlier new-test compile/expectation fixes retained in command log |
| `PYTHON_EXE -B scripts/verify-04a13.py final --evidence TestResults/04A-13-20261103-run01 --configuration Debug --name final-Debug` | Exit 1 for disclosed inherited managed/Python failures; full portable/Legacy/WASM/JS/Python workflows executed |
| Same command with `--configuration Release --name final-Release` | Exit 1 for the same inherited managed/Python failures; full matrix completed |
| `node --test BESHStat.WasmSmoke/tests/*.test.cjs` | Exit 0, 377/377 explicit confirmation |
| `PYTHON_EXE -B -m mkdocs build --config-file BESHStatNG_Help_MkDocs/mkdocs.latest.yml --site-dir RUN_ALIAS/mkdocs-strict-site --strict` | Exit 0; Help/resolver/link/version-routing suites also executed in full matrix |
| `Rscript --vanilla scripts/reference-04a13.R evidence/04A13/R-candidate-reference-v2.csv` | Exit 0; 18/20 scalar matches, 19 KM estimates match; two OPEN scientific nonmatches, NOT_GOLDEN |
| Same runner `historical` | Strict exit 1: stale UibyID/NormalPlot review dependency; profile exit 1: INTEGRITY_FAILURE. These remain non-green |
| `PYTHON_EXE -B scripts/capture-04a13-contract.py`; new `unittest` source suite; publication seal/verify; `git diff --check`; `git status --short` | Source/control/pin checks pass; final publication/preservation receipts supply settled counts |

The immutable `scripts/verify-beshstatng.ps1` is executed through source-hashed,
diff-recorded isolated copies with `-Legacy -Wasm -Js -Python -VisualStudio 2026`.
The literal script would reuse historical result paths, restore and invoke native
packaging hooks. Its accepted isolation runner redirects every writer, uses installed
packages with `--no-restore`, and compiles the real Windows host through safe targets.
Independent failures are collected while preserving a nonzero final exit; strict/
profile exits are never weakened. [Command outcomes](../../evidence/04A13/command-outcomes.json)
contain every recorded command/exit and raw-receipt hash. Machine paths are sanitized
to explicit aliases; public derivatives do not claim raw byte hashes.

## Documentation, retention and publication

[Documentation-impact receipt](../../evidence/04A13/documentation-impact.json): no
Windows behavior change activated; three existing Help pages clarify actual inputs,
defaults, options, legacy differences and limitations in this batch. Each states the
Office survival editor is unavailable. No fictitious Online procedures, enabled
Office survival methods or Mac verification were added. Existing screenshots remain
historical Windows images, unchanged; no new screenshot evidence is claimed.

Initial protection covers **189,815 root TestResults files**, their membership,
size, creation/write nanoseconds and file identity, plus frozen source/evidence
hashes. Initial free disk: **530233479168 bytes**. The [final preservation receipt](../../evidence/04A13/retention.json) is **PASS**,
with zero membership/metadata/source-pin errors and **529334722560 bytes** free; it is a membership/metadata audit,
not a claim to rehash every historical result byte. No historical results were
cleaned, overwritten, relocated, compressed or rebaselined. The [nested inventory qualification](../../evidence/04A13/nested-retention-scope.json)
records two empty nested roots and 139 Windows historical files, all written before
this batch. This additional observation was taken at closeout; it lacks a new
pre-edit nested per-file membership/identity snapshot and is not retrospective proof.
No command wrote into those nested historical roots.
A shared ZIP/Git clone cannot independently prove excluded private originals or
local retention metadata.

The [private evidence inventory](../../evidence/04A13/private-evidence-inventory.json)
keeps raw proof roles and hashes distinct from public derivatives.
Final publication checks cover **189 candidate paths** and **188 external file hashes/byte counts**.
The public manifest gives exact candidate SHA-256, byte sizes and live ignore/
trackability checks. It excludes its own hash explicitly and includes itself in
privacy/trackability scope. The entire candidate set is scanned: docs, tests, source,
JSON keys/values, decoded capture strings, known current owner/host tokens and path/
credential patterns. Private original maps, raw TRX/logs, private baseline metadata,
build outputs and caches remain excluded. Alias values are explicitly aliases, never
invented filesystem paths. No original evidence was redacted in place or claimed
hash-equal after sanitization. Final index is empty; all changes remain uncommitted.

Later owner-authorized manual smoke — **NOT RUN / NOT AUTHORIZED**:

1. Disposable KM single/two-group input with tied event/censor times; compare optional
   table/median/fixed-time choices, alpha .05/.10, plot CI/legend/title/units and output
   target. Verify multi-area physical rows and native censor markers after exact gate.
2. Weighted stratified log-rank: all five weights, original group/stratum captions and
   table order. Resolve the scientific rho=0 discrepancy separately before promotion.
3. Cox tied/stratified fit with reference-coded effects, expanded starts, robust/PH/
   residual options; verify three-sheet sequence, residual row identity, editable
   adjusted curve/stratum/time-axis formulas, native series and cancel/interrupt/close.
   Residual Plots remains disabled; no residual graph is expected.

## Proposed owner acceptance wording

> Accept Batch 04A.13 software scope: typed Common KM/log-rank/Cox requests,
> diagnostics, original Core-backed orchestration and report/curve/residual payloads,
> consumed by the shipping Windows forms, with focused/full validation and Help
> clarifications. Retain all scientific candidates, historical nonpasses, original
> OPEN native obligations and TestResults preservation requirements; C09/C11/C12
> remain unapplied. This acceptance grants no native execution/release, P promotion,
> C2 completion, Git publication or 04A.14 authorization.

No additional timing/behavior-change decision is requested for this extraction.
