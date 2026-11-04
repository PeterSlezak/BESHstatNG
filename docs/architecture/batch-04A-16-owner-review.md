# Official 04A.16 owner review

**READY_FOR_OWNER_04A16_ACCEPTANCE_REVIEW**. Software extraction complete with the explicitly retained nonpasses below; native/scientific gates remain unmet.

## 1. Live baseline and exact scope

Branch `codex/04A-16`; base, merge base, unchanged HEAD and verified main: `60bb832b8e10c94166b86cf3f661cc87f61eb7f3`. Main contains the accepted 04A.14 and 04A.15 implementations, tests and evidence. Entry worktree/index were clean; no owner edits were absorbed. Creating the authorized branch required sandbox permission for `.git/index.lock`; the Git ownership exception remained command/process scoped. No global configuration was changed.

[Prechange manifest](../../evidence/04A16/prechange-manifest.json) and [method inventory](../../evidence/04A16/prechange-method-inventory.json) freeze relevant source hashes, original method hashes/locations, table order and optional-warning boundaries before production changes. Full initial tracked byte pins and private historical filename/metadata maps remain local. [Source contract](../../evidence/04A16/source-contract.json) records the eight approved before/after files, ten new sources, exact 20 MMRM/six LMM changed methods and unchanged members. All **3,062** other initially tracked files remain byte-identical. No files were deleted.

Changed existing paths:

- `BESHStat.Excel.Common/src/Results/MixedModelReportPolicy.vb`
- `BESHstatNG/src/DM/WindowsMixedModelWorkflowAdapter.vb`
- `BESHstatNG/src/UI/Ui18MMRM.vb`
- `BESHstatNG/src/UI/Ui19LMM.vb`
- `BESHStatNG.Test/BESHStatNG.Tests.vbproj` (three test Compile entries)
- `BESHStatNG_Help_MkDocs/docs/methods/mmrm/lsmeans-and-contrasts.md`
- `BESHStatNG_Help_MkDocs/docs/methods/mmrm/options-and-output.md`
- `BESHStatNG_Help_MkDocs/docs/methods/lmm/options-and-output.md`

New sources:

- `BESHStat.Excel.Common/src/Results/MixedModelPostEstimationPolicy.vb`
- `BESHStat.Excel.Common/src/Results/MixedModelDiagnosticPolicy.vb`
- `BESHStat.Excel.Common.Tests/src/LegacyMixed16Selection.vb`
- `BESHStat.Excel.Common.Tests/src/Mixed16PostEstimationTests.vb`
- `BESHStatNG.Test/src/Mixed16AdapterTests.vb`
- `scripts/verify-04a16.py`
- `scripts/capture-04a16-contract.py`
- `scripts/summarize-04a16.py`
- `scripts/verify-04a16-publication.py`
- `scripts/tests/test_04a16_post_estimation.py`

This dossier and the explicitly enumerated `evidence/04A16/*.json` complete the proposed publication set. [Publication manifest](../../evidence/04A16/publication-manifest.json) enumerates every final candidate's SHA-256, bytes and live trackability; only its own self-hash is excluded to avoid a cycle, with separate checking. The index remains empty; nothing was staged, committed, merged, pushed, deployed, installed, signed or promoted. No 04A.17 work occurred.

## 2. Real shipping integration and method migration

| Shipping method/boundary | 04A.16 ownership and actual consumption |
|---|---|
| `CaptureMMRMReportOptions` | Adds lossless requested contrast/direction text to existing copied options; original UI labels/fallback distinctions retained |
| `AppendMMRMEstimatedMeanTables` | Keeps original acquired-input guards, logging and data-dependent UI refresh. Captures intent after refresh, preserves the original alpha capture, invokes Common `ProjectMMRM`, then actual adapter `ApplySupplemental` |
| `AppendMMRMReferenceGridTables` | Builds the current GUI specification through its adapter/helper path and calls Common `ProjectReference`; no independent Windows row/contrast orchestration remains |
| Baseline/control/comparison resolvers and controlled contrast helpers | Thin delegates to Common's original selection semantics and existing Core operations; same missing-comparison exception text and optional failure boundaries |
| Authored categorical/continuous key selection and grouping | Common `MainEffectKeys`, `GroupingChoice`, `HasGroupingLevels` and `BetterGroupingCandidate`; Windows still acquires numeric columns and logs host extraction failures |
| `BuildMMRMReferenceGridSpec` and additional-factor/covariate helpers | Reuse accepted `MixedModelReferenceIntent`; Common chooses role exclusion and Core factor/covariate requests through `AddReferenceColumn`. Windows retains original per-column acquisition/catch/log timing |
| MMRM class builder / LMM `AppendLMMClassInfoTable` | Windows acquires original class display levels; Common composes the exact original two table layouts. LMM attaches the detached class report through the actual adapter |
| `WriteMMRMModelSheet` / `WriteLMMModelSheet` | Consume `MixedModelReportPolicy.Project(..., decision).LegacyTables()` through the original `ProcessListofResultTables` and `ExcelDnaResultWriter` path |
| `RunMMRMAsync` / `RunLMMAsync` | Only additional post-fit argument plumbing carries the accepted immutable fit decision; one `Fit`, callbacks, progress, interruption/cancellation, UI invoke, cleanup and disposal remain otherwise identical |

The original three-argument public `MixedModelReportPolicy.Project` binary entry point is preserved. A separate four-argument overload adds decision metadata. It retains the original KR-to-REML decision rather than recalculating it. Source/native-operation tests verify unchanged workbook names/order, native writer statements, G/R/random-effect suppression, trace logic and all unrelated form members. UI refresh, messages, enable/disable logic, event bindings, acquisition, RefEdit and Help remain Windows-owned.

Common projections materialize immutable supplemental tables without appending to the fit. The thin Windows adapter attaches rehydrated tables to the **same** existing `AdditionalResultTables` list, preserving original Core additional tables and insertion order. A weak fit-keyed identity tracker replaces only its own prior tables on repeated attachment. It stores no X/Z or inference workspace. The final report still calls original Core `wrapResults`, so KR term tests and supplemental tables precede covariance/fit/diagnostic/convergence/trace sections as before. Detached reports remain usable after original workspace release.

The legacy LMM writer's existing suppression mutations (G/R matrices and random-effect predictions) remain in place; this batch did not change their timing or silently calculate disabled sections. Original unused private reference-profile compatibility helpers remain byte-identical and have no active shipping route. They are not claimed as newly portable services or a second active pipeline.

All Core covariance/fitting/KR/Satterthwaite/DF/reference-grid/multiplicity algorithms, worksheet UDF signatures/exports/handles, Designers, front-end acquisition helpers, 04A.14/15 fitting policies, metadata authority and version-aware Help links are unchanged at their source hashes. Core/Common stay `netstandard2.0`; Windows stays .NET Framework 4.8; Office targets and x86/x64 packaging remain unchanged. No dependencies were added.

## 3. Typed Common contracts and exact behavior matrix

`MixedModelPostEstimationIntent` wraps the existing detached reference intent, copied report options, explicit source RowIds and optional accepted fit decision. No X/Z, Core result or workspace is stored. `MixedModelReferenceColumn` snapshots one host-acquired numeric role column for immediate Core specification construction. `MixedModelClassLevel` carries immutable display metadata. Supplemental reports contain detached `AnalysisResultDocument` tables, ordered section enums and bounded optional warnings. `MixedModelDiagnosticSnapshot` retains scalar status/method/availability data only; `MixedModelReportSection` supplies stable index/code/original-label descriptors without changing table or contrast-row ordering.

| Choice | Retained default/effective behavior and limits |
|---|---|
| Observed design | Default; visit means, optional baseline change, grouped means/differences/change/difference-in-change, in original order |
| Reference grid | Visit/group by-profiles, other authored categorical main effects marginalized, continuous covariates at means or zero. Means and optional group contrasts only; no observed baseline-change tables |
| Group selection | `(None)` disables grouping. `(Auto)` uses the first smallest eligible authored categorical candidate with at least two finite levels; there is no six-level cap in the original implementation. Explicit grouping still requires two levels |
| Baseline / control | `(Smallest)` / `(First)` defaults; original formatted-level match, invariant-first numeric parse, current-culture fallback and first-level fallback preserved |
| Selected comparison | `(Auto)` selects first level different from control; manual comparison parse remains current-culture first, then invariant. Missing/same-control selection keeps the original optional-table exception. No new dialog/refusal |
| Contrast / direction | None, pairwise, each-versus-control, selected comparison; higher-minus-lower, treatment-minus-control, control-minus-treatment. Requested raw strings retained where unknown Core string fallbacks differ from enum selection |
| Weighting | Equal cells or observed class-cell frequency. Core owns profile ordering, joint cell counts, normalization, L vectors and footnotes |
| Continuous covariates | Shipping GUI supports observed means or zero. A nonzero portable requested `CovariateValue` is candidate-diagnosed and does not invent a GUI control; existing direct Core explicit-value services remain unchanged |
| Multiplicity / alpha | None, Bonferroni, Holm, Sidak delegated to Core. Reference-grid family is the full contrast table across profiles/visits; raw and adjusted p-values retained, CIs unadjusted. Observed-design contrasts remain unadjusted despite a selected multiplicity setting. Existing Core alpha fallback is projected as metadata, not reimplemented probability math |
| Fit/inference status | Converged, nonconverged, cancelled, interrupted, failed/unavailable; requested accepted decision and actual Core fit/inference remain distinct. Presence of a workspace is availability metadata, not proof that KR inference is valid |
| Diagnostics | Core warnings in order, alpha fallback, coefficient/covariance availability/nonfinite/negative-diagonal candidates, actual optimizer/provider and trace/support/restart/KR finite-difference availability. At most 128 structured warnings; existing Core diagnostic tables retain their original order/data |
| Optional failure | Missing/failed optional table is omitted and candidate-diagnosed; other tables continue. Reference specification/row failure retains the original outer-catch boundary and existing class output. No replacement zero effect, invented DF or new strict Windows rejection |
| LMM | Existing class/G/R/BLUP/inference/diagnostic/report behavior only. Portable Core reference estimates can be requested explicitly from characterized Core services; no LMM GUI editor/report switch was added |

Portable request validation remains separate from the acquired Windows path. `C14-INPUT-FINITE-01` and new post-fit candidate checks do not activate stricter host refusal. Source RowIds are copied only when supplied; the X/visit/group axis follows acquisition order, while Core's subject/visit permutations remain separate. Neither positional arrays nor reference rows fabricate worksheet coordinates. No long-lived full-size design or KR workspace snapshot is retained in Common.

Example: create a `MixedModelPostEstimationIntent` after acquiring/refreshing data-dependent choices, call `ProjectMMRM` or `ProjectReference` with the fitted result and existing Core services, inspect its ordered sections/warnings, and materialize through `LegacyTables`. Windows `ApplySupplemental` and final `MixedModelReportPolicy.Project` are the shipping consumers; standalone consumers can use the detached documents directly. Existing public report enums and `MixedModelReferenceIntent` are reused, not duplicated.

## 4. Numerical and inference compatibility

[Retained evidence](../../evidence/04A16/retained-evidence.json) verifies all original **100** 04A.14 captures and the original 04A.15 ledger at unchanged SHA-256/bytes. The latter remains **1,408** same-Core comparisons, **712 converged / 696 nonconverged**. Nothing was regenerated, cherry-picked, replaced or promoted. All are **NOT_GOLDEN** compatibility references.

[Frozen selection provenance](../../evidence/04A16/legacy-selection-provenance.json) pins six actual prechange form methods and their test-only derivative. Only visibility and original ComboBox text acquisition were adapted to copied strings; Core calls, parser ordering, comparison branches and errors remain intact. The independent assembly of original Core calls is a regression oracle, not independent statistical validation.

[Fresh comparisons](../../evidence/04A16/post-estimation-comparisons-v1.json) records 192 specified-coefficient/covariance option profiles across invariant/sk-SK, net10/net48 and Debug/Release: **1,536** execution records. Those synthetic profiles are **not fits**. A separate **200** records reuse the 25 actual accepted fixture families across both cultures/frameworks/configurations: **192 converged / eight nonconverged executions**, retaining every original classification. These include Orthodont, sleepstudy, incomplete/unbalanced samples, KR/Satterthwaite, one-iteration nonconvergence and original KR multi-df report metadata. Existing Core/backend one-/multi-df reference suites also run unchanged.

Table comparisons cover values, profiles/L vectors/weights, labels, CLR cell kinds, DF/CI/p-values, footnotes, p-value coordinates, optional-section order and report placement plans. Fixed tolerance: `max(1e-9, abs(expected)*1e-9)` for doubles; exact nonnumeric/shape/order/metadata. Only computational timing cells are masked by the unchanged 04A.14 canonicalizer. Tolerance was never tuned. Canonical per-execution output digests are explicitly distinguished from raw TRX hashes and independent references.

Negative/ownership regressions cover empty/invalid grids, duplicate role names, absent or same control/comparison, raw unknown choices, no/single grouping, invalid alpha, missing/nonfinite Beta/VarBeta, negative variance, nonconverged/cancelled/interrupted results, missing/partial KR state, no optional tables, immutability, row/permutation identity, bounded warnings, repeated projections/attachments and report usability after workspace release. GUI/UDF parity is restricted to intentionally equivalent Core requests; UDF profile/range grammar, positional identity and handle behavior remain unchanged.

Independent R: **NOT RUN**. Existing R-derived references remain unpromoted; no independent verification claim is made. Native Excel/COM/UIA, Office execution and Mac parity were **NOT RUN**. Numerical defects found outside this extraction remain separately owned; no estimator or inferential fallback was changed.

## 5. Original controls, obligations and gates

The byte-identical [original crosswalk](../../evidence/04A14/restriction-crosswalk.json), original authority files and per-control/obligation/source row hashes remain the exact-ID authority, referenced with new successor hashes rather than rewritten.

| Original population | Controls | OPEN obligations | Restricted controls |
|---|---:|---:|---:|
| Ui18MMRM | 93 | 40 | 39 |
| Ui19LMM | 84 | 29 | 28 |
| Mixed-model total | 177 | 69 | 67 |

Zero releases/discharges or invented obligations. Wider original populations remain 1,514 obligations/802 restricted controls; transferred 239/218; ANOVA/normality 160/141; importer 44; 2x2 three; UiGLM seven/34; GEE 35/34; survival 12/10. These overlap and must not be added together or narrowed by this batch.

`C09-NATIVE-STRICT-01`, `C11-NATIVE-CORRECTIONS-01` and `C12-NATIVE-CLASSIFICATION-01` stay UNAPPLIED at their original bytes; current `git apply --check` tests pass. `C13-LOGRANK-R-01` remains an unresolved scientific defect. `C14-INPUT-FINITE-01` stays candidate-only/nonblocking at the Windows boundary. 04A.15's option/fallback/nonconvergence decisions remain unchanged. **P=NO; N1=NO GO; N2=NO; Gate C2=UNMET.**

## 6. Validation, commands and named nonpasses

[Final validation](../../evidence/04A16/validation.json), per configuration:

| Suite/accounting | Debug pass/total | Release pass/total | Exit / nonpasses |
|---|---:|---:|---|
| Core managed | 1,058/1,058 | 1,058/1,058 | 0 |
| Common managed net10 | 2,592/2,597 | 2,592/2,597 | 1 / five inherited |
| Windows managed net48 | 2,596/2,598 | 2,596/2,598 | 1 / two inherited, including one shared with Common |
| Office add-in managed | 505/505 | 505/505 | 0 |
| Office execution managed | 45/45 | 45/45 | 0 |
| Control mitigation managed | 61/61 | 61/61 | 0 |
| All raw managed framework executions | 6,857/6,864 | 6,857/6,864 | Seven runtime failures |
| Distinct logical managed cases | **5,522/5,528** | **5,522/5,528** | **Six inherited logical failures** |
| New 04A.16 logical managed cases | 285/285 | 285/285 | All pass; included above |
| Node | 916/916 | 916/916 | 0 |
| Python distinct unittest methods | **699/775** | **699/775** | 1 / **74 inherited at entry + two new frozen-source assertions** |
| New 04A.16 Python methods | 8/8 | 8/8 | 0; included above |
| Complete canonical workflow derivative | Nonzero | Nonzero | 1, retaining failures |
| Historical strict / profile, configuration independent | NOT PASSING | NOT PASSING | 1 / 1, reasons below |
| Strict MkDocs, configuration independent | PASS | PASS | 0 |

Logical cases deduplicate identical linked source/method/data rows across frameworks using the established normalization and retained null-cohort weights. Raw framework counts, failed identities and individual receipts remain separate. Focused runs and C02 environment probes are excluded from these totals.

Entry Debug: Core **1,058/1,058**, Common **2,333/2,338**, Windows **2,311/2,313**. Comparable entry Python: **693/767**, **74** nonpassing methods. The accepted 72 remain, plus already-failing 04A.15 successor assertions `Mixed15SourceContracts.test_all_protected_sources_and_new_source_hashes` and `test_branch_ancestry_empty_index_and_exact_existing_scope`. Entry measured these before shipping edits; fresh unchanged C02 representative observations qualify the comparable Common-Python probe. Original missing-environment probe remains local. Release/Office/Node entry were not independently measured.

The same six inherited logical managed failures remain: three `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection` rows `(20,15,40)`, `(0,0,0)`, `(20,30,50)`; `ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`; `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`; and shared `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings(True)`. Their paired mapping/refusal/overflow and superseded constant-binomial exception expectations are retained. No old assertion was weakened.

Two newly triggered predecessor source assertions are **not** classified as inherited or passing:

- `test_04a14_mixed_workflow.Mixed14SourceContracts.test_reports_are_consumed_by_actual_writers_and_append_seams` requires a direct `MixedModelReportPolicy.Plan` call inside the old MMRM append method. That unchanged planning policy now runs inside Common `ProjectMMRM`; the new successor proves actual adapter/writer consumption and numeric/order parity.
- `test_04a15_fitting.Mixed15SourceContracts.test_only_actual_shipping_factory_routes_change` permits only 04A.15's three whole-form factory substitutions. Authorized 04A.16 post-fit methods differ; their exact successors and unchanged native operations are pinned/tested here.

[Nonpass register](../../evidence/04A16/nonpasses.json) enumerates every inherited/new method, causal explanation, current sanitized terminal and raw traceback provenance. Existing predecessor source-pin assertions also encounter this authorized Common-report/form successor; the current source map records those differences explicitly. The suites are **not green**.

`PYTHON_EXE` denotes the configured existing WinPython executable, aliased in public receipts. Exact final workflows:

```text
PYTHON_EXE -B scripts/verify-04a16.py init --evidence TestResults/04A-16-20261104-run01
PYTHON_EXE -B scripts/capture-04a16-contract.py audit
PYTHON_EXE -B scripts/verify-04a16.py entry --evidence TestResults/04A-16-20261104-run01 --name entry-Debug
PYTHON_EXE -B scripts/verify-04a16.py entry-python --evidence TestResults/04A-16-20261104-run01 --name entry-python
PYTHON_EXE -B scripts/verify-04a16.py semantic --evidence TestResults/04A-16-20261104-run01 --name entry-comparable
PYTHON_EXE -B scripts/verify-04a16.py common-python --evidence TestResults/04A-16-20261104-run01 --name entry-comparable
PYTHON_EXE -B scripts/verify-04a16.py focused --evidence TestResults/04A-16-20261104-run01 --name api-compatibility --windows
PYTHON_EXE -B scripts/verify-04a16.py final --evidence TestResults/04A-16-20261104-run01 --name final-Debug-complete --configuration Debug
PYTHON_EXE -B scripts/verify-04a16.py final --evidence TestResults/04A-16-20261104-run01 --name final-Release --configuration Release
PYTHON_EXE -B scripts/verify-04a16.py docs --evidence TestResults/04A-16-20261104-run01 --name mkdocs-final
PYTHON_EXE -B scripts/verify-04a16.py historical --evidence TestResults/04A-16-20261104-run01
PYTHON_EXE -B scripts/verify-04a16.py preserve --evidence TestResults/04A-16-20261104-run01 --name final
PYTHON_EXE -B scripts/capture-04a16-contract.py source
PYTHON_EXE -B -m unittest discover -s scripts/tests -p test_04a16_post_estimation.py -v
PYTHON_EXE -B scripts/summarize-04a16.py entry
PYTHON_EXE -B scripts/summarize-04a16.py validation
PYTHON_EXE -B scripts/summarize-04a16.py numerical
PYTHON_EXE -B scripts/summarize-04a16.py nonpasses
PYTHON_EXE -B scripts/summarize-04a16.py retention
PYTHON_EXE -B scripts/summarize-04a16.py commands
PYTHON_EXE -B scripts/verify-04a16-publication.py seal
PYTHON_EXE -B scripts/verify-04a16-publication.py verify
git diff --check
git status --short
git diff --cached --name-only
```

The required canonical `scripts/verify-beshstatng.ps1` portable/Legacy/WASM/JS/Python workflows run through inherited, exact-anchor isolated execution derivatives in both configurations, using `powershell -NoProfile -ExecutionPolicy Bypass -File <local derivative> -Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe PYTHON_EXE`. The literal default was not run because it overwrites protected historical results and invokes packaging/signing hooks outside this authorization. [Execution adaptations](../../evidence/04A16/execution-adaptations.json) preserves original/executed SHA-256 and exact sanitized diffs. Actual Windows host/test assemblies compile through safe managed targets, without restore/install/packaging/signing. [Command outcomes](../../evidence/04A16/command-outcomes.json) records resolved commands, exits, isolated execution receipts and distinct raw hashes.

Historical strict retains exit 1 for stale UibyID/NormalPlot review dependency; historical profile retains exit 1/`INTEGRITY_FAILURE`. Their evidence/exit semantics remain unchanged. Strict MkDocs passes; inherited unlisted `family-link-covmat.md` is INFO only. Development-only compile diagnosis corrected access to Core's internal alpha normalizer by projecting its established fallback as metadata and explicitly naming the warning-loop variable. Review preserved the original public three-argument binary entry point; its focused regression and final full rerun qualify the correction. Earlier runs remain local and are excluded from final totals.

Final-source qualification: both full matrices cover the final executable production/test logic, including that API correction. Afterwards a single trailing LF in `Ui18MMRM.vb` was removed to restore the original no-final-newline state, and evidence-helper classification/accounting text was completed. [Exact formatting receipt](../../evidence/04A16/final-formatting.json) pins both byte states; no VB token or method body changed. The full matrices were **not rerun after this bounded formatting edit**. The affected `eof-final` Common/Windows suites and current `source-final-complete` contracts were rerun; commands and exits are retained. This qualification is explicit in validation JSON rather than claiming byte-identical final-source full execution.

## 7. Preservation, publication and Help

[Retention](../../evidence/04A16/retention.json) covers all **204,317** initial root and **139** nested historical files: membership, size, creation/write nanoseconds and file identity. Protected source/evidence, all original captures and the 04A.15 comparison/obligation ledgers additionally retain SHA-256/bytes. This is not an all-historical-byte audit. Nothing historical was deleted, overwritten, relocated, compressed, recreated or rebaselined. Raw original receipts, ignored logs/TRX, full source maps and historical filename maps remain private/local; public derivatives have separate hashes and explicit aliases.

[Documentation impact](../../evidence/04A16/documentation-impact.json) audits all nine requested pages. Three editorial updates clarify unchanged table families, factor/covariate/multiplicity limits and absent LMM/Office UI. The other six need no change because controls/screenshots, equations, example values and behavior remain unchanged. No screenshot or task-pane instruction was fabricated. `_meta/methods.yml`, Help routes and UDF metadata are untouched.

The owner-maintained Master Plan remains unchanged at Revision 2.8. Proposed progress amendment only: mark 04A.14 and 04A.15 accepted/merged at the verified main entry; record 04A.16 as implemented pending owner review; retain the official next-batch scopes and every native/scientific restriction. No stale audit/decision record or externally edited plan was overwritten. Fixture CSV bytes/newlines are unchanged; the existing documentation CRLF/LF warning does not introduce a separate line-ending-only publication change. Original form end-of-file state is preserved.

[Privacy receipt](../../evidence/04A16/privacy.json) scans the complete exact candidate set, UTF-8 text, decoded JSON/base64, Unicode/case variants and current owner/host/path/SID/credential/secret patterns. All candidates pass ordinary live ignore checks; new files also pass `--no-index`. Already-tracked Help pages retain their inherited broad Help `docs/*` ignore rule under `--no-index`, with tracked status and both exits explicitly recorded. JSON parsing, allowed enumeration, hashes/bytes, manifest membership and empty index are checked. The manifest's own JSON/privacy/trackability/hash is checked separately, without circular self-hashing. This is bounded scanning plus review, not proof of absence of arbitrary secrets. No `.gitignore`, private workbook data, path-bearing raw receipt, cache or large historical result is proposed for publication.

## 8. Native disposition

No new behavior-changing Windows activation or blocked numerical correction is proposed. This is a preserving extraction through actual post-fit/report seams; candidate validation remains outside native rejection and dialog timing. No new dormant patch, reused timing exception or exact-ID owner activation disposition is needed. All original native prechange/attribution requirements remain unmet and in force. Source and managed equality establish neither native Excel parity nor independent statistical validity.

## 9. Acceptance recommendation and later work

Accept 04A.16 software scope after reviewing the final table, named nonpasses and manifest. Acceptance does not grant P promotion, native execution/control release, Gate C2 completion, publication, or 04A.17 authorization. Later 07.05/07.06 and 04A.39-41 retain scientific/consumer reconciliation; 04A.28 retains wider writer work; 04A.45-48 retain controlled native attribution/parity/exit gates. Existing scientific issues and unsupported factor/profile/front-door limitations are not silently corrected by this batch.

Suggested later native smoke checks, **NOT RUN**, requiring separate owner authority and original prechange/loaded-build attribution:

1. MMRM observed versus reference-grid means on incomplete/unbalanced data; baseline/control/selected-comparison refresh, direction signs, whole-table multiplicity family, KR/Satt DF/CI/p-values and omission of an unavailable supplemental table. Verify the original Data/model/trace order and physical row provenance.
2. Random-slope LMM: existing class/KR/G/R/BLUP/diagnostic writer behavior, interrupt/cancel/close and cleanup. The eventual LMM-with-LS-means native smoke remains deferred: no such GUI editor/report switch exists now; it needs a separately supported entry point and native authorization.

All changes remain unstaged/uncommitted and the index empty. **P NO / N1 NO GO / N2 NO / Gate C2 UNMET.**
