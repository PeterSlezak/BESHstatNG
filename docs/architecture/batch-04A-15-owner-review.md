# Official 04A.15 owner review

**READY_FOR_OWNER_04A15_ACCEPTANCE_REVIEW.** Software scope is complete with the inherited nonpasses and native/scientific limits below. Changes are unstaged and uncommitted.

## 1. Branch, baseline and exact publication scope

Branch `codex/04A-15`; base and unchanged HEAD/main: `0ed7f42a57e781443bf2c3495703a963b4332c47`. The checked-out `main` was clean and contained accepted 04A.14's Common policies, actual Windows adapter/form integration, tests and documentation. Local ancestry was verified. No owner modifications or untracked files existed before the new verification runner was authored. The sandbox required permission only to create the authorized branch; Git's ownership exception is command/process scoped, without global configuration edits.

[Prechange manifest](../../evidence/04A15/prechange-manifest.json) pins 71 relevant sources, accepted baseline identities and original lineage. Full tracked source hashes and historical filename/metadata maps remain local. [Source contract](../../evidence/04A15/source-contract.json) records exact before/after hashes, all nine new source hashes, form member hashes and unchanged public source pins. All 3,037 other initial tracked files are byte-identical. No files were deleted. [Publication manifest](../../evidence/04A15/publication-manifest.json) supplies every final candidate's bytes/SHA-256 and trackability; it excludes only itself from its hash map and is checked separately.

Changed existing paths:

- `BESHStat.Excel.Common/src/Analysis/MixedModelWorkflowPolicy.vb`
- `BESHstatNG/src/DM/WindowsMixedModelWorkflowAdapter.vb`
- `BESHstatNG/src/UI/Ui18MMRM.vb`
- `BESHstatNG/src/UI/Ui19LMM.vb`
- `BESHStatNG.Test/BESHStatNG.Tests.vbproj` (three test Compile entries only)
- `BESHStatNG_Help_MkDocs/docs/methods/mmrm/options-and-output.md`
- `BESHStatNG_Help_MkDocs/docs/methods/mmrm/user-interface.md`
- `BESHStatNG_Help_MkDocs/docs/methods/lmm/random-effects-and-covariance.md`
- `BESHStatNG_Help_MkDocs/docs/methods/lmm/options-and-output.md`
- `BESHStatNG_Help_MkDocs/docs/methods/lmm/user-interface.md`

New source/test/tool paths:

- `BESHStat.Excel.Common/src/Analysis/MixedModelFittingOptions.vb`
- `BESHStat.Excel.Common.Tests/src/Mixed15FittingTests.vb`
- `BESHStat.Excel.Common.Tests/src/Mixed15FitParityTests.vb`
- `BESHStatNG.Test/src/Mixed15AdapterTests.vb`
- `scripts/verify-04a15.py`
- `scripts/capture-04a15-contract.py`
- `scripts/summarize-04a15.py`
- `scripts/verify-04a15-publication.py`
- `scripts/tests/test_04a15_fitting.py`

This dossier and the explicitly enumerated `evidence/04A15/*.json` complete the proposed set. Everything remains unstaged/uncommitted; the index is empty. No commit, merge, push, deployment, installer, signing operation, native-control release or 04A.16 work occurred.

## 2. Completed contracts and shipping paths

`MixedModelFittingOptions` captures independent requested options, typed R/G metadata, fit decision, effective request optimizer/gradient enums and the original control projection. `MixedModelCovarianceSelection` retains the exact token, concrete Core type/name, support, visit-index and degenerate-G metadata. Parameter counts and Q restrictions delegate to Core. Choices return read-only copies; no data matrix, control, form, filesystem service or numerical estimator is retained in this portable surface.

The existing workflow request owns `Fitting`, obtains its decision, delegates R/G construction and applies its control projection. The public 04A.14 `ApplyOptimizer` API delegates to the specialist. Original alias grammar remains in `MixedModelOptionPolicy`; its validation-only factory projections are reused and compared against every original factory alias. There is no second new parsing table. `PromotedRandomStructure`, authoring guards and report policy remain in their accepted ownership.

Both real `RunMMRMAsync` and `RunLMMAsync` handlers now call the actual `WindowsMixedModelWorkflowAdapter.CreateResidualStructure`; LMM also calls `CreateRandomStructure`. The adapter delegates to Common's thin original-Core factory calls. Construction remains at the original pre-KR-message sites, and the resulting instances are passed to `CreateAcquiredCoreRequest` and reused. Metadata selection is silent; each physical construction preserves exactly the original single factory log/exception sequence. The original acquired seam retains host guard timing and does not enforce new strict diagnostics.

The form changes are exactly three factory-call substitutions; all other characters/methods, event bindings, control defaults/enabling, KR messages, callback assignments, one `Fit` per run, cancellation/interruption, writers, post-estimation, output ordering, cleanup/disposal and Help navigation match main. Existing private optimizer wrapper methods remain unchanged delegates for compatibility with the accepted source/reflection contract; they contain no independent parser. `MixedModelFrontEndHelpers` also remains unchanged, retaining legacy host guards and UDF parser delegation.

Core R/G constructors, covariance/parameter transforms, derivatives, analytic gradients, AI/BFGS optimizers, engine likelihood/linear algebra, inference and convergence modules are unchanged. Designers, worksheet UDF exports/signatures/handles, report policy, authored metadata, version-aware Help routes and fixture CSVs are byte-protected. Core/Common remain `netstandard2.0`; Windows stays .NET Framework 4.8 with unchanged x86/x64 packaging definitions.

Example portable usage:

```vb
Dim fitting = New MixedModelFittingOptions(MixedModelWorkflowKind.MMRM,
    New MixedModelWorkflowOptions With {.ResidualStructure = "AR1",
        .Optimizer = "Projected BFGS (analytic gradient)"})
' fitting.Residual.RequestedToken = "AR1"
' fitting.Residual.EffectiveCoreType = "AR1R"
' fitting.OptimizerMode = ProjectedBfgsAnalyticGradient
' fitting.GradientMode = AnalyticScore
```

For a complete request, inspect `request.Validation` and use `CreateCoreRequest` for strict portable validation. `CreateAcquiredCoreRequest` intentionally retains the shipping compatibility boundary. Effective request enums describe what is sent to Core; actual runtime fallback/provider selection and convergence remain Core results.

## 3. Covariance and fitting matrix

Every original alias, including trimmed/case variants, is executable against concrete original Core factories in both frameworks/cultures. Interior-spacing and cross-side aliases retain their original rejection boundaries. Unknown R/G tokens throw their original factory exceptions; no scientific model is invented. Null/empty R remains unsupported; null/empty G means `NoRandomEffects`. MMRM ignores requested G and has no Z; LMM's GUI excludes `None`, and strict portable LMM validation rejects a zero G selection.

| R display selection | Effective Core type | Visit-index based | Core parameter count |
|---|---|---|---|
| Identity | `IdentityR` | No | 1 |
| Diagonal Heterogeneous | `DiagonalHeterogeneousR` | Yes | T |
| Compound Symmetry | `CompoundSymmetryR` | No | 2 |
| Heterogeneous CS | `HeterogeneousCSR` | Yes | T+1 |
| AR(1) | `AR1R` | Yes | 2 |
| Heterogeneous AR(1) | `HeterogeneousAR1R` | Yes | T+1 |
| Toeplitz (TOEP) | `ToeplitzR` | Yes | T |
| Heterogeneous Toeplitz (TOEPH) | `HeterogeneousToeplitzR` | Yes | 2T-1 |
| Unstructured | `UnstructuredR` | Yes | T(T+1)/2 |

T is Core's global retained visit dimension, or its original maximum-cluster-size fallback without visits. R AR/Toeplitz lags use sorted distinct visit indices, not elapsed numeric distance. Missing visits retain their observed submatrices and subject/visit/source-row permutations. Core can warn and use sequential pseudo-visits; the Windows visit guard still requires a role for indexed R, and MMRM still requires its visit role. A missing role is not conflated with optional visits under Identity/CS. With one visit, CS/HCS/AR/HAR retain their original extra correlation parameter; its allocation does not establish estimability. Documentation was corrected editorially, without changing this behavior.

| G display selection | Effective Core type | Design constraint |
|---|---|---|
| None | `NoRandomEffects` | No active G; not an LMM GUI choice |
| Random Intercept | `RandomIntercept` | Exactly Q=1; original authored-intercept guard |
| Random Intercept + Slope | `RandomInterceptSlope` | Exactly Q=2; intercept plus one authored effect |
| Identity | `IdentityRandomEffects` | Q>=1, one shared variance |
| Variance Components (VC/Diag) | `VarianceComponentsRandomEffects` | Q>=1, one variance per expanded column |
| Compound Symmetry (CS) | `CompoundSymmetryRandomEffects` | Q>=1 |
| Heterogeneous Compound Symmetry (CSH) | `HeterogeneousCompoundSymmetryRandomEffects` | Q>=1 |
| Autoregressive (AR1) | `AutoregressiveRandomEffects` | Q>=1; ordered Z columns |
| Heterogeneous Autoregressive (ARH1) | `HeterogeneousAutoregressiveRandomEffects` | Q>=1; ordered Z columns |
| Toeplitz (TOEP) | `ToeplitzRandomEffects` | Q>=1; ordered Z columns |
| Heterogeneous Toeplitz (TOEPH) | `HeterogeneousToeplitzRandomEffects` | Q>=1; ordered Z columns |
| Unstructured Random Effects | `UnstructuredRandomEffects` | Q>=1; Core Cholesky parameterization |

General Z columns are never reordered by this policy. Intercept/slope/general, categorical/interaction/polynomial authoring and the original automatic VC/Diag promotion remain unchanged. The 108 displayed R/G pairs are independently exercised for metadata with and without visits; they are not advertised as 108 converged fits. Separate tests fit every displayed nonzero G with Identity R and both methods/four optimizers, plus combined R structures with intercept/slope designs. Invalid Q/aliases distinguish strict portable diagnostics from unchanged acquired construction and existing Core rejection.

| Requested optimizer | Effective request optimizer | Effective request gradient |
|---|---|---|
| AI/Fisher scoring (default), empty/whitespace | `AverageInformationReml` | Separate gradient parser |
| Projected BFGS (auto gradient) | `ProjectedBfgs` | Separate gradient parser; Auto by default |
| Projected BFGS (analytic gradient) | `ProjectedBfgsAnalyticGradient` | `AnalyticScore` exact-label override, ignoring case |
| Projected BFGS (finite-difference gradient) | `ProjectedBfgs` | `NumericalFiniteDifference` exact-label override, ignoring case |
| Unknown nonblank MMRM token | `ProjectedBfgs` | Separate gradient parser |
| Unknown nonblank LMM token | `AverageInformationReml` | Separate gradient parser |

All original optimizer/gradient parser literals and shipping labels, including case/outer-spacing variants, are compared against frozen `LegacyMixed14Options` plus prechange control construction. An outer-spaced label can parse differently from an exact-label override; that distinction is retained. Gradients map Auto/Analytic/analytic-with-finite-difference-validation/Numerical to their original four enum values. Validation mode is distinct from numerical optimization. Core's analytic-optimizer override and provider/AI fallback are unchanged; AI attempts REML and may fall back to projected BFGS for ML or unusable AI results.

GUI fit selection recognizes ML ignoring case and otherwise retains REML; the broader UDF helper grammar remains intentionally separate. KR's case-insensitive ML guard still coerces REML while the original case-sensitive inference selection retains model-specific fallbacks (MMRM between-within, LMM Wald). Unknown inference/optimizer/gradient values are not normalized to a new effective scientific choice.

Maximum iterations and epsilon copy verbatim to `MaxIter`, `Epsilon`, `StepTolerance` and `FunctionTolerance`; trace is trace OR iteration-details. Other Core controls remain intact. New `mixed.optimizer.fallback`, `mixed.gradient.fallback`, `mixed.fit.fallback`, `mixed.random.dimension` and `mixed.residual.dimension` diagnostics join existing `RegressionDiagnostic` contracts in deterministic order, bounded by 128 overall. Strict diagnostics never activate new Windows dialogs/refusals. `C14-INPUT-FINITE-01` remains diagnostic-only and OPEN, including legacy acceptance of NaN/infinite epsilon at the acquired seam.

## 4. Numerical and negative evidence

[Baseline and lineage](../../evidence/04A15/baseline-and-lineage.json) verifies all original 100 04A.14 captures at original SHA-256/bytes: 25 cases x invariant/sk-SK x net10/net48. They are unchanged compatibility references, **NOT_GOLDEN**. Their frozen capture-source identity is retained. Existing Orthodont, sleepstudy, unbalanced/missing longitudinal, KR/Satterthwaite and original report-order regressions run unchanged.

[New versioned fit comparisons](../../evidence/04A15/fit-comparisons-v1.json) record 176 new fit combinations x two cultures x two frameworks x two configurations. Original factory/control requests and specialized requests use the same unchanged Core. Coefficients, theta, covariance, objective, DF/inference, fitted/residual arrays and original table cell kinds/order/metadata are compared. This is same-engine compatibility, not independent statistical validation. The 12-iteration new matrix deliberately preserves nonconverged outcomes; those are not relabelled converged/PASS fits.

The 1,408 recorded comparisons retain **712 CONVERGED and 696 NONCONVERGED** outcomes. None of those 176 supported fit combinations was rejected by Core. Invalid-Q rejection is separately asserted; metadata-only pairs, including `None`, are not counted as fits.

Tolerance was fixed before fitting: `max(1e-9, abs(expected)*1e-9)` for doubles, exact nonnumeric/shape/status/order/metadata. Only computational timing cells are masked by the unchanged canonicalizer. No tolerance or old expectation was tuned. Separate gradient modes, all optimizer cancellation/interruption and one-iteration limits, zero/constant responses, unsupported/oversized Q, malformed/null data, nonfinite tolerances, iteration extremes, source RowIds/permutation, copy isolation and factory logger/failure boundaries have executable coverage. Public evidence distinguishes selected metadata, actual fits, nonconvergence and Core rejection.

GUI/UDF parity is intentional only where requests are equivalent. UDF positional arrays do not invent worksheet coordinates; source role captions/physical RowIds and Core block order remain distinct. GUI inference fallbacks, KR/ML handling and broader UDF option aliases are retained differences. UDF signatures/handles are unchanged. R was **NOT RUN**; existing public R-derived references remain unpromoted and do not independently qualify every new combination. Native Excel/Office/Mac execution was **NOT RUN**.

## 5. Original restrictions and later ownership

The byte-identical [04A.14 original-control crosswalk](../../evidence/04A14/restriction-crosswalk.json), its referenced original row/obligation/source hashes and authority pins remain the exact-ID authority:

| Form | Original controls | OPEN obligations | Restricted controls |
|---|---:|---:|---:|
| Ui18MMRM | 93 | 40 | 39 |
| Ui19LMM | 84 | 29 | 28 |
| Total | 177 | 69 | 67 |

Zero controls released; zero obligations discharged or invented. Original broader populations remain 1,514 obligations/802 restricted controls; transferred 239/218; ANOVA/normality 160/141; importer 44; 2x2 three; UiGLM seven/34; GEE 35/34; survival 12/10. These overlap and must never be added together or narrowed through this batch.

`C09-NATIVE-STRICT-01`, `C11-NATIVE-CORRECTIONS-01` and `C12-NATIVE-CLASSIFICATION-01` remain UNAPPLIED at their original bytes, with current `git apply --check` tests. `C13-LOGRANK-R-01` remains an OPEN scientific defect; its source/evidence is untouched. `C14-INPUT-FINITE-01` remains an OPEN candidate diagnostic. **P=NO; N1=NO GO; N2=NO; Gate C2=UNMET.**

04A.16 retains deep LS-means/reference grids/contrasts/KR/Satt/result diagnostics. 04A.39-41 retain cross-consumer/scientific reconciliation; 04A.45-48 retain native attribution, controlled prechange observation, parity and exit gates. No post-estimation engine or Office fitting UI was added.

## 6. Validation commands, totals and nonpasses

[Final validation](../../evidence/04A15/validation.json) uses the last complete Debug/Release matrices. Per-suite managed rows below count framework executions; the distinct-logical row reconciles shared linked tests by source SHA and data-row identity, retaining the established Mixed14 typed-null cohorts. Repeats are excluded. There are no managed or Node skips.

| Suite/check | Debug passed / total | Release passed / total | Exit / qualification |
|---|---:|---:|---|
| Core managed | 1,058 / 1,058 | 1,058 / 1,058 | 0 each |
| Common managed | 2,333 / 2,338 | 2,333 / 2,338 | 1 each; five inherited runtime failures |
| Windows net48 managed | 2,311 / 2,313 | 2,311 / 2,313 | 1 each; two inherited runtime failures |
| Office managed | 505 / 505 | 505 / 505 | 0 each |
| WASM execution managed | 45 / 45 | 45 / 45 | 0 each |
| Retained C03 host-free projection | 61 / 61 | 61 / 61 | 0 each |
| **Distinct logical managed cases** | **5,237 / 5,243** | **5,237 / 5,243** | Six inherited logical failures |
| Raw managed framework executions | 6,313 / 6,320 | 6,313 / 6,320 | Seven runtime failures |
| New 04A.15 logical managed cases | 545 / 545 | 545 / 545 | Zero new failures |
| Node Office / resolver / WASM | 468/468; 71/71; 377/377 | 468/468; 71/71; 377/377 | 0 each workflow |
| **Node total** | **916 / 916** | **916 / 916** | 0 |
| **Python distinct methods** | **695 / 767** | **695 / 767** | 1 each; same 72 entry nonpasses |
| New 04A.15 Python source contracts | 7 / 7 | 7 / 7 | Current focused rerun also 7/7 |
| Full aggregate workflow | exit 1 | exit 1 | Retains independent failure exits |
| Strict MkDocs latest | exit 0 | Single configuration-independent run | Final affected help source |
| Historical strict / profile | exits 1 / 1 | Single configuration-independent run | Stale dependency / INTEGRITY_FAILURE |
| Historical retention | exit 0 | Single preservation run | Zero membership/metadata/pin errors |

No new or changed managed/Python failure identity remains. Python's three additional entry failures relative to accepted 04A.14 are `Mixed14SourceContracts.test_branch_base_ancestry_and_empty_index`, `test_exact_scope_and_external_owner_plan_are_separate`, and `test_original_members_and_successor_pins_are_reconcilable` in `test_04a14_mixed_workflow`. They were already nonpassing before shipping edits on the new branch; all 69 accepted Python nonpasses also remain. The register retains all 72 current traceback provenances, not just aggregate counts.

`PYTHON_EXE` denotes the existing configured WinPython executable; public receipts use aliases instead of profile paths. Commands use fresh ignored `TestResults/04A-15-20261104-run01` destinations:

```text
PYTHON_EXE -B scripts/verify-04a15.py init --evidence TestResults/04A-15-20261104-run01
PYTHON_EXE -B scripts/verify-04a15.py entry --evidence TestResults/04A-15-20261104-run01 --name entry-Debug
PYTHON_EXE -B scripts/verify-04a15.py entry-python --evidence TestResults/04A-15-20261104-run01 --name entry-python
PYTHON_EXE -B scripts/verify-04a15.py semantic --evidence TestResults/04A-15-20261104-run01 --name entry-comparable
PYTHON_EXE -B scripts/verify-04a15.py common-python --evidence TestResults/04A-15-20261104-run01 --name entry-comparable
PYTHON_EXE -B scripts/verify-04a15.py focused --evidence TestResults/04A-15-20261104-run01 --name aliases-complete --windows
PYTHON_EXE -B scripts/verify-04a15.py final --evidence TestResults/04A-15-20261104-run01 --name final-Debug-complete --configuration Debug
PYTHON_EXE -B scripts/verify-04a15.py final --evidence TestResults/04A-15-20261104-run01 --name final-Release --configuration Release
PYTHON_EXE -B scripts/verify-04a15.py docs --evidence TestResults/04A-15-20261104-run01 --name mkdocs-final
PYTHON_EXE -B scripts/verify-04a15.py historical --evidence TestResults/04A-15-20261104-run01
PYTHON_EXE -B scripts/verify-04a15.py preserve --evidence TestResults/04A-15-20261104-run01 --name final
PYTHON_EXE -B scripts/capture-04a15-contract.py
PYTHON_EXE -B scripts/summarize-04a15.py entry
PYTHON_EXE -B scripts/summarize-04a15.py validation
PYTHON_EXE -B scripts/summarize-04a15.py numerical
PYTHON_EXE -B scripts/summarize-04a15.py nonpasses
PYTHON_EXE -B scripts/summarize-04a15.py retention
PYTHON_EXE -B scripts/summarize-04a15.py commands
PYTHON_EXE -B -m unittest discover -s scripts/tests -p test_04a15_fitting.py -v
PYTHON_EXE -B scripts/verify-04a15-publication.py seal
PYTHON_EXE -B scripts/verify-04a15-publication.py verify
git diff --check
git status --short
git diff --cached --name-only
```

The required `scripts/verify-beshstatng.ps1` portable, `-Legacy`, `-Wasm`, `-Js`, `-Python`, Debug/Release workflows run through inherited exact-anchor isolated derivatives using `powershell -NoProfile -ExecutionPolicy Bypass -File <local execution derivative> -VisualStudio 2026 -PythonExe PYTHON_EXE`. The literal default script was not run: it overwrites protected historical destinations and invokes build hooks outside this authorization. [Execution adaptations](../../evidence/04A15/execution-adaptations.json) records original/executed SHA-256 and the sanitized exact diff. Actual Windows host/test assemblies are compiled with safe managed targets; no packaging/signing/installer hooks run. No restore, package installation, target change or network dependency was introduced. [Command outcomes](../../evidence/04A15/command-outcomes.json) retains exact sanitized commands/exits and distinct raw receipt hashes.

Entry Debug: Core 1,058/1,058, Common 1,792/1,797, Windows 1,766/1,768. Comparable Python 688/760, 72 nonpassing methods: the accepted 69 plus three 04A.14 branch/base/scope expectations already failing on this successor branch. The original missing-G3-environment probe remains local; fresh unchanged C02 semantic observations qualify the comparable probe. Release/Office/Node entry were not independently measured.

The six inherited logical managed failures are three `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection` rows `(20,15,40)`, `(0,0,0)`, `(20,30,50)`; `ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`; `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`; and `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings(True)` (shared across two frameworks). They retain paired mapping/refusal/overflow assertions and the old constant-binomial exception expectation. No frozen test was weakened. [Nonpass register](../../evidence/04A15/nonpasses.json) enumerates every current method, inherited/new identity, root cause, sanitized terminal and raw traceback provenance. These suites are not green.

Historical strict remains exit 1 for stale UibyID/NormalPlot review dependency; historical profile remains exit 1/`INTEGRITY_FAILURE`. Their evidence and exit semantics are untouched. Strict MkDocs passes, with only inherited unlisted `family-link-covmat.md` INFO. Initial development-only test corrections fixed a reserved VB local identifier and the mistaken historical RefEdit patch path in the new test; no shipping defect or old assertion was changed. Earlier focused/full runs remain local and are excluded from final totals; the last complete matrices use the final production and test sources.

Evidence accounting subsequently corrected its Node reporter-prefix matcher and its initial mistaken assumption that the successful preservation receipt would contain MkDocs pin exceptions. The original receipts were never altered. Full suites were not repeated after these evidence-helper-only edits; final current source-contract tests and publication checks were rerun. No production, managed-test, Python-test or method-help source changed after the final complete matrix sources were frozen.

## 7. Preservation, documentation and privacy

[Retention](../../evidence/04A15/retention.json) records preservation of all 199,945 initial root and 139 nested historical files: membership, size, creation/write nanoseconds and file identity. Frozen mixed-model captures, original ledgers, protected sources and all old public evidence additionally retain their byte hashes. This is not an all-historical-byte rehash. No historical evidence was deleted, overwritten, relocated, compressed, recreated or rebaselined.

The inherited generic preservation runner returns PASS with zero historical or protected-source errors. The source contract additionally verifies all other original tracked bytes and the ten approved before/after hashes, including five MkDocs pages. Full local source-pin maps, raw logs/TRX, command/provenance receipts and retention maps remain ignored/local, with explicit raw/public derivative distinctions. No raw original is uploaded or falsely assigned a redacted derivative hash.

[Documentation impact](../../evidence/04A15/documentation-impact.json) audits all seven requested pages. Five receive editorial clarifications of unchanged optimizer overrides/fallbacks, indexed lag/single-visit limits, or Windows-only availability. The two implementation-details pages need no changes because algorithms/inference/output behavior remain unchanged. Screenshots were not fabricated or recaptured. Authored metadata and Help links remain unchanged; Office mixed-model role preview still performs no MMRM/LMM fit.

The Master Plan remains unchanged at Revision 2.8. Proposed owner amendment only: mark 04A.14 accepted/merged at the verified main entry; record 04A.15 as implemented pending this review, preserving the official 04A.15/16 scopes and every scientific/native restriction. No stale narrative or old audit/decision record was rewritten. No owner-edited plan was absorbed. Fixture CSV bytes/newlines are unchanged; Git's existing documentation CRLF-to-LF normalization warnings do not represent additional line-ending-only publication changes. Original form end-of-file newline state is preserved.

[Privacy receipt](../../evidence/04A15/privacy.json) and publication verification cover the entire exact candidate set, decoded JSON/base64 and current environment identifiers/path/secret patterns. All candidates pass ordinary live ignore checks; every new file also passes `--no-index`. The five already-tracked Help pages retain the inherited broad `docs/` ignore rule under `--no-index`, which does not prevent publishing tracked modifications; the manifest records their tracked status and both exact exits. All candidates parse as standard JSON where applicable and match manifest bytes/hashes. The manifest itself is separately parsed, privacy/trackability checked and hashed; no circular self-hash is claimed. The scan is bounded evidence, not proof of absence of arbitrary secrets. No `.gitignore` change, private workbook content, cache, raw path-bearing receipt or large historical result is proposed for publication.

## 8. Native disposition

No new behavior-changing Windows path is blocked or proposed. The shipping integration is a source- and managed-compatibility-preserving delegation; new strict portable diagnostics remain inactive at the acquired seam. No new timing exception, dormant activation patch or owner disposition is needed. All pre-existing exact native gates and OPEN obligations remain in force. Source/managed equivalence does not establish native Excel parity.

## 9. Acceptance recommendation and later smoke checks

Accept 04A.15 software scope after reviewing this dossier and its final manifest, with the disclosed inherited nonpasses and qualification limits. Acceptance grants no scientific promotion, native authorization/control release, Gate C2 completion, publication or 04A.16 implementation authority. Keep the index empty and all candidates unstaged/uncommitted for the owner's decision.

Suggested later separately authorized native tests, **NOT RUN**:

1. MMRM AR1 with incomplete/irregular visits, REML/ML and KR coercion, all optimizer labels; verify actual loaded reviewed build, message timing, fit/convergence/trace tables and physical RowIds/output order.
2. LMM intercept/slope and categorical/general random effects with VC promotion, ordered G/R choices and optional visits; compare reviewed request/output provenance, then interrupt/cancel/close and confirm one fit, original cleanup and worksheet behavior.

Both require original native prechange evidence/attribution and separate owner execution approval. P=NO; N1=NO GO; N2=NO; Gate C2=UNMET.
