# Official Batch 04A.14 — owner review

**READY_FOR_OWNER_04A14_ACCEPTANCE_REVIEW.** The MMRM and LMM shipping paths now use Common request, effective-option and report-selection policies. Core remains the numerical authority. This is a software acceptance recommendation with disclosed nonpasses, not native or scientific qualification.

## Source of truth and working tree

- Branch: `codex/04A-14`.
- Entry local `main`, feature HEAD, final HEAD and merge base: **`b02480a71c1f2de45beb9cb0726a564eae15076b`**.
- Initial `git status --porcelain=v1`: empty; `git diff --check`: exit 0. Local `main...origin/main`: **0 / 0**, using local refs only. No remote contact occurred. The feature branch was created from that verified local main; no existing divergent branch was replaced.
- That main commit contains the accepted survival forms, Common policies, adapters, tests and 04A.13 dossier. The owner's current instruction and subsequently observed Master Plan 2.8 acceptance statement supersede the older dossier's proposed-acceptance wording; no historical acceptance document was rewritten.
- Read before implementation: root `AGENTS.md`; Master Plan **2.7**, official 04A.14–16 rows; 04A.13 dossier and evidence; original 04A.01 inventory/deferred ledger and accepted 04A.02 amendment. Inspected both actual forms/Designers, mixed-model helper, effect controllers/04A.08 authoring and design compiler, Core fit/block/covariance/inference/reference/post-estimation contracts, UDF imports/exports, existing mixed-model tests/reference inputs and all twenty affected Help pages.
- During implementation the owner/external editor updated the Master Plan to **2.8**. That file was preserved untouched by this implementation. Its 04A.14–16 scope is unchanged. [Owner-plan receipt](../../evidence/04A14/owner-plan-update.json) retains both hashes and the private raw-diff hash; original entry/protection records remain intact. The owner file is explicitly excluded from the proposed batch publication set.
- Index **EMPTY**. No staging, commit, merge, push, deployment, installer, signing, package installation, numerical promotion or native execution occurred. No 04A.15 implementation began.

[Prechange manifest](../../evidence/04A14/prechange-manifest.json), [source inventory](../../evidence/04A14/source-inventory.json), [source delta](../../evidence/04A14/source-delta.json), [final Git state](../../evidence/04A14/final-state.json) and [exact path inventory](../../evidence/04A14/path-inventory.json) reconcile the baseline and successor. The initial compact manifest has an inherited survival/GEE scope sentence; the inventory records that description erratum without changing its original bytes or hashes.

Agent-modified tracked files, exactly seven:

- `BESHstatNG/src/UI/Ui18MMRM.vb`
- `BESHstatNG/src/UI/Ui19LMM.vb`
- `BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb`
- `BESHstatNG/BESHStatNG.vbproj`
- `BESHStatNG.Test/BESHStatNG.Tests.vbproj`
- `BESHStatNG_Help_MkDocs/docs/methods/mixed-models-for-repeated-measures-mmrm.md`
- `BESHStatNG_Help_MkDocs/docs/methods/linear-mixed-models-lmm.md`

Added implementation: Common `Analysis/MixedModelOptionPolicy.vb`, `Analysis/MixedModelWorkflowPolicy.vb`, `Results/MixedModelReportPolicy.vb`; Windows `DM/WindowsMixedModelWorkflowAdapter.vb`. Added tests: `Mixed14Baselines.vb`, `LegacyMixed14Options.vb`, `Mixed14WorkflowTests.vb`, `Mixed14ReportTests.vb` in Common.Tests, and `Mixed14AdapterTests.vb` in the Windows project. Added four 04A.14 verification/accounting scripts, `scripts/tests/test_04a14_mixed_workflow.py`, this dossier and public `evidence/04A14/` artifacts, including 100 individually listed captures. **No deletions.** The exact added filenames, sizes and hashes are in the publication manifest, rather than treating a directory wildcard as a hash pin.

## Production delegation and ownership

The compact [pre-edit ownership map](../../evidence/04A14/ownership-before.json) preceded production edits. The following describes the implemented boundary.

| Shipping entry/route | Common operation actually consumed | Core authority | Windows boundary retained |
|---|---|---|---|
| Both `ValidateInputs` methods | `LegacyRejectIterations`, `LegacyRejectEpsilon`, `EmptyEffectSelection` preserve the existing predicates | Existing expansion/structure validation | Selection checks, culture-aware parsing, exact warning strings, focus and dialogs |
| `Ui18MMRM.RunMMRMAsync` | `DecideFit(MMRM,...)`, `WindowsMixedModelWorkflowAdapter.CaptureMMRM`, `workflow.CreateAcquiredCoreRequest(rStruct)` | Original blocks, `CreateMMRM`, full KR/Satt configuration, original control structure | Existing acquisition/expansion, KR message/combo update, callbacks, one `Task.Run(Function() model.Fit())` |
| `Ui19LMM.RunLMMAsync` | Corresponding LMM decision/capture/request calls | Original `CreateLMM`, expanded X/Z, R/G instances and dimensional guard | Optional visit acquisition, fixed/random controllers, same one asynchronous fit and messages |
| LMM simple-structure promotion methods | `PromotedRandomStructure` with the original authored count and full term dictionary snapshot | Existing authoring and G-structure contracts | Widget assignment/enablement, exception presentation; original dimensional refusal remains |
| `Ui18MMRM.AppendMMRMEstimatedMeanTables` | Fresh `CaptureMMRMReportOptions` after original level refresh; `MixedModelReportPolicy.Plan` selects sections | Existing observed-design means/change/difference builders | UI level acquisition, `AddOptionalMMRMTable` logging and append operations |
| `AppendMMRMReferenceGridTables` / `BuildMMRMReferenceGridSpec` | Reference section plan, `MixedModelReferenceIntent.CreateCoreSpec`, typed weighting/multiplicity/covariate/direction decisions | Original grid normalization, profiles, estimates and contrast calculations | Additional factor/covariate acquisition and existing profile-matching/contrast builders remain for 04A.16 |
| `Ui19LMM.AppendLMMClassInfoTable` | LMM class-information section plan | Existing class/term information | Existing class row construction and `AdditionalResultTables` append timing |
| Both `Write*ModelSheet` methods | `MixedModelReportPolicy.Project(...).LegacyTables()` | Exactly one original `result.wrapResults` per projection | Original writers and separators; LMM's selected G/R/random-effect suppression mutations remain at the same host seam |
| Both result/data writers | `ShouldWriteTrace`, `ShouldWriteResidual` | Original trace/residual arrays | New workbook **Data → MMRM/LMM → optional MMRM Trace/LMM Trace**; same captions, raw RowIds and writer operation order |
| Existing shared helper's six option parsers | `MixedModelOptionPolicy` | Established Core enums/defaults | Friend signatures, UDF callsites, extraction/effect/trace helpers unchanged |
| Run completion/finally/close | No UI operation in Common | Original workspace release methods | Progress/cancel/interrupt/close/DoEvents/thread invocation and large-array cleanup preserved; new workflow reference is cleared before managed cleanup |

The source successor verifier checks actual handlers, exactly one `Fit`, writer/message sequences, unchanged acquisition/expansion/progress/cleanup members and exact changed-member sets. Host-free adapter tests invoke the **shipping adapter class**, build equivalent Core requests and compare the real result-table memory-writer blocks. They do not construct or display the shipping forms or invoke Excel.

All Core algorithm files, original Designers, UDF declarations/model-handle sources, importer implementation, accepted survival/GEE code and prior evidence remain byte-pinned. The six shared parser delegations are exercised against the frozen source-derived helper oracle, including aliases/fallbacks and both cultures. Validation-only R/G alias projections reproduce the exact unchanged Core factory selection branches while omitting logger/throw side effects; source equality and a logger probe verify this boundary. They create only stateless Core type/visit descriptors, add no aliases or covariance mathematics, and leave actual request creation with the original Core factories/acquired instances. Their deeper specialization is assigned to 04A.15. Existing 04A.08 categorical/reference/interaction/polynomial fixtures and original mixed-model regression suites also execute in the full matrix. No second formula grammar or estimator was introduced.

## Request, row, option and report semantics

| Area | Previous and current behavior / portable boundary |
|---|---|
| Snapshot ownership | Core's immutable subject-block snapshot is reused. No second retained n×p/n×q design copy; O(n) original row/subject sidecars and explicit block-to-input permutation. Arrays/options/names/reference intent return independent copies; immutable accepted effect models and transport frames are reused. Output tables are defensively snapshotted and rehydrated; Common retains no mutable fitted result or control |
| Roles/provenance | Response/subject/visit metadata uses the original Core captions. Separate immutable `SourceResponseKey`, `SourceSubjectKey`, `SourceVisitKey` retain the original selection IDs. Optional physical-import transport is carried without inventing source coordinates for raw positional arrays |
| Sorting/missingness | Windows acquisition/filtering is unchanged. MMRM calls the original block builder with `sortWithinSubjectByVisit:=True`, `rowNumbers:=raw.RowIds`; LMM sorts by visit only if supplied. Core subject order is ordinal; within blocks, visit/source-row ordering is preserved. Input order and block order are distinct. A physical three-area fixture retains rows **101/103/104**, sorted permutation **104/103/101**, and original source columns |
| Subjects/effects | Text/numeric subject identities and source-specific conversions remain Core/host-owned. Fixed/random snapshots retain typed term IDs, intercepts and expanded names. Portable diagnostics identify typed-label aliases, missing identities and ambiguous coefficient names; they do not silently relabel or merge input |
| Likelihood/inference | GUI defaults remain REML/KR. KR with ML still forces effective REML, updates the combo where possible and displays the exact model-specific Windows information message. Exact GUI selection fallback remains **MMRM BetweenWithin / LMM WaldNormal**. The original case-insensitive KR guard and case-sensitive inference switch remain distinct, including the lowercase edge case |
| Covariance/optimizer | Original R/G factories and aliases are authoritative. MMRM has no G design; LMM preserves simple-intercept/slope limits and explicit VC/Diag promotion. AI/BFGS/analytic/numerical mappings, model-specific unknown-option fallbacks and gradient overrides match the original helper/form code. No new unsupported-combination refusal is invented; deeper specialization belongs to 04A.15 |
| Controls | UI MaxIter **500** and epsilon **1e-6** remain. The requested epsilon populates `Epsilon`, `StepTolerance`, `FunctionTolerance`; the edited value-type control is assigned back to `req.Control`. Trace is requested by trace **or** iteration-details. Core defaults for other fields are retained |
| Optional reports | Class information, means, change, group differences, difference in change, reference means/contrasts retain their original inclusion/order. Observed design means and reference-grid LS-means are separate intents. Equal/observed weights, observed-mean/user-zero covariates, multiplicity, baseline/control/comparison and directed sign remain distinct. LMM plans do not advertise MMRM-only sections |
| Failures/undefined output | Core tables, footnotes, p-value metadata and missing/undefined values remain authoritative. `Converged`, `Cancelled`, `Interrupted` are explicit projection properties. A projected document does not establish fit convergence. Absent Core result produces a typed unavailable fault; no numerical row is fabricated. Existing optional-table omission/catch/log behavior remains Windows-owned |
| Strict diagnostic boundary | Portable entry validates shape, empty/nonfinite arrays, source rows/subjects, roles/names, authored dimensions, covariance/visit requirements and numeric/report options, with at most **128** diagnostics. Strict `CreateCoreRequest` rejects invalid portable requests. Shipping uses the explicitly named **acquired** seam and original host guards, so no new native refusal is activated |

`C14-INPUT-FINITE-01` records the existing epsilon finite-check/typed-identity validation gap as an **input candidate**, not a demonstrated scientific defect. In particular the legacy `epsilon <= 0` guard permits NaN/infinity. Candidate checks remain inactive for Windows; empty-random/intercept selection already has its original host guard. The original `tbEps`/related control IDs are crosswalked; where no original obligation exists, none is invented. No new stricter activation or dormant patch is proposed in this batch.

[Compatibility/handoff register](../../evidence/04A14/handoff-and-compatibility.json) distinguishes `same`, `intentionally Windows-only presentation`, and `open defect candidate`. GUI physical RowIds versus UDF positional arrays, distinct inference defaults and KR/ML handling were not normalized away. Public UDF signatures/handles remain unchanged.

## Numerical and semantic evidence

[Baseline registry](../../evidence/04A14/baseline-registry.json) pins **100 pre-production-edit captures**: 25 original Core cases × invariant/sk-SK × net10/net48. The capture fixture was frozen before product changes; its first compile correction is retained in raw evidence. Captures include input blocks/permutation, coefficients/covariance/DF/p-values/theta/fitted/residual arrays, fit status and every original table cell/type/order/header/footer/p-value coordinate. All 100 complete; four capture variants record the intentional one-iteration nonconverged LMM.

Cases include MMRM Identity/CS/AR1/UN/CSH/Toeplitz, missing longitudinal observations, ML/REML and all GUI inference/fallback choices; Orthodont KR; LMM intercept/slope/VC, optional-visit behavior, sleepstudy and unbalanced slopes. Further tests compare three BFGS configurations to original Core request construction; four weighting/covariate reference profiles and original Core linear-estimate tables; directed signs; cancellation/interruption; five deficient/tied designs and bounded negative inputs. Original Core/Windows KR, Satterthwaite and existing reference CSV suites run in the full matrix.

Compatibility tolerance is **max(1e-9, abs(expected)×1e-9)** for doubles, accommodating runtime roundoff in the same calculation. Labels/order/CLR kinds/nulls/metadata are exact. Only computational timing-value cells are masked. [Numerical comparisons](../../evidence/04A14/numerical-comparisons.json) lists each capture, table schema and status. Memory-writer parity proves neutral values/placement plans, not Excel placement or event safety.

**Independent R: NOT RUN in 04A.14.** Existing Orthodont/sleepstudy/KR/pbkrtest inputs and reference scripts remain unchanged **reference candidates / NOT_GOLDEN**. Neither same-engine compatibility nor existing CSV assertions independently qualify every inference/covariance combination. No estimator, tolerance or reference promotion was changed. No new confirmed mixed-model scientific defect was registered. **`C13-LOGRANK-R-01` remains OPEN**, including the earliest-time censor risk-set issue; scientific/consumer owner 04A.40–41. Its source/evidence is unchanged.

## Native restriction lineage and later ownership

[Original-control crosswalk](../../evidence/04A14/restriction-crosswalk.json) retains each original control ID, original control-row hash, original obligation ID/row hash, source record/source/Designer/member hashes, verification/owner fields and **OPEN** status.

| Original form | Original controls | OPEN obligations | Restricted controls | Source-associated changed controls | No original obligation |
|---|---:|---:|---:|---:|---:|
| Ui18MMRM | 93 | 40 | 39 | 31 | 53 |
| Ui19LMM | 84 | 29 | 28 | 24 | 55 |

Across the **55** source-associated changed/added-method controls, **34** have original deferred obligations and **21** have none. Separate exact lists are provided; no obligations were created from current control counts. Wider overlapping populations remain **1,514 obligations / 802 restricted live controls**, with accepted 04A.02 **239/218 transfers** and other case subsets carried forward. These are not additive populations. **Zero releases, zero discharges.**

Original immutable source pins:

| Source | Original source SHA-256 | Original Designer SHA-256 |
|---|---|---|
| Ui18MMRM | `f8967b80168e6bd5b830468293ce90ab9e01130fa0bee1db2af1ba3dcaf49a73` | `e3d804eca057d3913497c6b0309c38b1a4f560212f63b3de9fbdbda64d754ff1` |
| Ui19LMM | `dc354d6d85d96da12087be74b94ee485ab4bdc937bd023408112e985e91ce381` | `26a9b3760293fafc4dfa698514abe59bb4ef00f0b10720fd3787fe05d6376e93` |

Changed whole-form pins receive honest main-entry/successor provenance in `source-delta.json`; original records were not replaced. `C09-NATIVE-STRICT-01`, `C11-NATIVE-CORRECTIONS-01`, `C12-NATIVE-CLASSIFICATION-01` remain **UNAPPLIED** at their original bytes; all three `git apply --check` checks pass. No shared helper activates them. No native timing exception was requested or reused.

| Later owner | Exact retained boundary |
|---|---|
| 04A.15 | Deep R/G covariance and optimizer/fitting specialization/combination coverage, using the new lossless request options and original Core engines |
| 04A.16 | Deeper reference-grid/LS-means/KR/Satt diagnostics mapping; `BuildMMRMReferenceGridGroupContrastsTable`, additional factors/covariates in `BuildMMRMReferenceGridSpec`, grouping/selected baseline/control/comparison resolution, MMRM class builder and LMM `AppendLMMClassInfoTable`/`AppendClassInfoRows` |
| 04A.39–41, 04A.45–48 | Input/UDF distinctions and `C14-INPUT-FINITE-01`, exact invalid-input/native decisions; C13 scientific issue remains separately owned |
| 04A.28/36, 04A.45–47 | Native output/placement/chart work if separately authorized; current writers/trace/residual tables remain Windows-owned |

The remaining pure post-estimation helpers are an explicit 04A.16 handoff, not claimed as already portable. No new plot or Office UI was implemented. **P NO; N1 NO GO; N2 NO; Gate C2 UNMET.**

## Validation commands and actual outcomes

[Entry measurements](../../evidence/04A14/entry-validation.json) are from the merged live checkout: Debug Core **1058/1058**, Common **1662/1667**, Windows **1605/1607**; comparable Python **676/744**, **68 nonpasses**. The initial Common Python probe lacked the G3 observation environment (168 methods, 27 nonpasses); fresh unchanged C02 representative-source observations remove its 17 environment-only errors. Both attempts are retained. Release/Office/Node entry were not separately executed; they are not claimed as measured entry totals. The final matrices measure both configurations.

| Full suite | Debug passed / total | Release passed / total |
|---|---:|---:|
| Core managed | 1058 / 1058 | 1058 / 1058 |
| Common managed | 1792 / 1797 | 1792 / 1797 |
| Windows net48 managed | 1766 / 1768 | 1766 / 1768 |
| Office managed | 505 / 505 | 505 / 505 |
| WASM execution managed | 45 / 45 | 45 / 45 |
| Retained C03 host-free projection | 61 / 61 | 61 / 61 |
| **Distinct logical managed cases** | **4692 / 4698** | **4692 / 4698** |
| Raw managed runtime executions | 5227 / 5234 | 5227 / 5234 |
| Node: Office / resolver / WASM | 468 / 468; 71 / 71; 377 / 377 | 468 / 468; 71 / 71; 377 / 377 |
| **Node total** | **916 / 916** | **916 / 916** |
| **Python distinct methods** | **691 / 760** | **691 / 760** |

**All 161 new logical managed cases and 16 new Python source contracts pass.** No managed skips. Linked cross-target executions are source/data-row reconciled, not summed as distinct logical tests. Three Mixed14 null-display cohorts additionally reconcile MSTest 2/3: the parser's Nothing/empty pair has indistinguishable Windows names, so its cohort has logical weight two and any failed execution would conservatively fail both. Original displays, source hashes and collision/cohort ledger remain in [validation.json](../../evidence/04A14/validation.json).

After the full matrices, final review retained the three lossless source-role key fields and completed candidate-only checks for nonfinite numeric subjects and empty random authoring without an intercept, with corresponding adapter assertions; validation was also made free of duplicate Core-factory logging. [Current-build focused receipts](../../evidence/04A14/focused-final.json) document rebuilds of Common and the actual Windows host/test assemblies in **both** configurations: **130/130 Common and 161/161 Windows** each. Core captions/options/numerics are unchanged. These focused results qualify those final portable-contract corrections; they do not replace or inflate the full-suite totals. The full suites were not repeated after these bounded corrections.

Six inherited logical managed failures remain: three `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection` data rows; `ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`; `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`; `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings(True)` (two runtime failures for one logical case). The paired cases preserve existing geometry/inference assertions; the binomial assertion still expects the exception superseded by an earlier accepted defensive report. No historical assertion was changed. Development-only nonpasses were corrected in new code/tests: the first capture fixture shadowed `IO.Path` with a local name; the logger probe initially mismatched an interface optional parameter (BC30401/BC30149); a new source assertion used a plural residual-predicate name. Successful focused reruns are retained alongside those attempts; none is reclassified as an inherited failure.

Python has **68 inherited nonpasses plus one newly triggered frozen project-source pin**:
`test_04a13_survival_workflow.Survival13SourceContracts.test_successor_pins_and_members_reconcile_to_actual_forms` still pins the pre-04A.14 Windows test-project bytes. Both new Compile-entry sets are deliberate; their successors are pinned. [Nonpass register](../../evidence/04A14/nonpasses.json) names/classifies **every** managed/Python nonpass and supplies sanitized terminal diagnostics plus raw-log/canonical-traceback hashes. Inherited categories include source/hash/scope assertions, native/compiler receipt assumptions, source/gate contracts and one unavailable historical redacted-path expectation. No new functional regression was found. These suites are **not green**.

`PYTHON_EXE` denotes the verified configured portable executable; `LOCAL_RUN_ALIAS:04A14-01` identifies the ignored fresh raw run, not an invented filesystem path. Exact resolved commands/exits and distinct raw/derivative hashes are recorded in [command outcomes](../../evidence/04A14/command-outcomes.json).

| Executed command/workflow | Outcome |
|---|---|
| `PYTHON_EXE -B scripts/verify-04a14.py init --evidence TestResults/04A-14-20261103-run01` | Exit 0; initial source pins, root and nested retention snapshots |
| Same runner `entry --name entry-Debug`; `entry-python --name entry-python` | Measured entry above; managed/Python failure exits retained |
| Same runner `semantic --name entry-comparable`, then `common-python --name entry-comparable` | 15/15 representative observations; Common Python exit 1 with five FAIL/five ERROR, not the environment-only probe errors |
| Same runner `capture --name prechange-Core-02`; `capture --name prechange-net48 --windows` | 25/25 each runtime; 100 original captures. First new-fixture compile error corrected; raw failed attempt retained |
| Same runner `focused --name edges-01 --windows` | 130/130 Common, 161/161 Windows |
| Same runner `final --name final-Debug --configuration Debug` | Exit 1; complete portable/Legacy/Office/WASM/Node/Python matrix with disclosed nonpasses |
| Same command, `--name final-Release --configuration Release` | Exit 1; same full outcomes/nonpass identities |
| Same runner `focused --name settled-Debug-02 --configuration Debug --windows`; Release uses `--name settled-Release` | Both exit 0; current-build 130/130 and 161/161 per configuration; earlier role-key and contract reruns also pass |
| `node --test BESHStat.WasmSmoke/tests/*.test.cjs` | Exit 0; explicit 377/377 confirmation, excluded from full totals |
| `PYTHON_EXE -B -m mkdocs build --config-file BESHStatNG_Help_MkDocs/mkdocs.latest.yml --site-dir <fresh run site> --strict` | Exit 0; inherited unlisted family-link-covmat page INFO only. Resolver/link/navigation/version-routing suites also ran in both full matrices |
| Same runner `historical` | Strict exit 1: stale UibyID/NormalPlot review dependency. Profile exit 1: INTEGRITY_FAILURE. Exits not weakened |
| Same runner `preserve --name final` | Exit 1, **only** owner-plan source-pin drift; no historical membership/metadata error. Original FAIL retained |
| `scripts/capture-04a14-contract.py`, new Python source suite, publication seal/verify, `git diff --check`, branch/index checks | Final successor/privacy/trackability/Git receipts; no old test disabled |
| Independent R / native Windows/Office/Mac execution | **NOT RUN**; native execution not authorized |

The literal default `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1` invocation would overwrite protected historical destinations and invoke disallowed build hooks. Its unchanged portable/Legacy/WASM/JS/Python workflows were run through the inherited, exact-anchor, source-hashed execution derivatives. They redirect results, isolate Common/Windows execution, use `--no-restore`, compile the real Windows host through safe managed targets and collect independent failures while returning a nonzero aggregate exit. [Execution adaptations](../../evidence/04A14/execution-adaptations.json) preserves original/executed hashes and sanitized diffs. No claim of unchanged shell bytes or completed packaging/signing is made. Strict/profile exits are untouched. VS2026/net48, netstandard2.0 libraries and Office/WASM builds succeeded; frameworks and packaging definitions are unchanged.

## Preservation, publication and privacy

[Retention successor](../../evidence/04A14/retention.json) verifies **194,109** initial root historical files and **139** nested project TestResults files, complete membership, size, creation/write nanoseconds and file identity. No loss or metadata change. Frozen source/evidence pins reconcile except the independently hash-verified external Master Plan edit. The original raw preservation **FAIL** remains unedited and separately hashed; the successor distinguishes that source edit from historical result loss and clears no gate. Initial free disk **529,140,027,392 bytes**; final snapshot is recorded in the receipt, approximately 528 GB free.

This is **not** an all-historical-byte audit. A shared ZIP cannot prove original workstation file identity/timestamps/private path membership. No historical file was deleted, relocated, compressed, recreated or rebaselined. Protected/private original ledgers, raw source-pin maps and test receipts remain at their original locations. [Private inventory](../../evidence/04A14/private-evidence-inventory.json) exposes aliases/hashes/scope only.

[Publication manifest](../../evidence/04A14/publication-manifest.json) enumerates every proposed publishable path, SHA-256, bytes, tracked/new status and Git-ignore/trackability result. It explicitly omits its self-hash while including itself in privacy/trackability checks. The owner-plan path, all ignored raw logs/TRX/maps, tool outputs and caches are excluded. Public normalized derivatives have their own manifest hashes, distinct from raw originals. The [privacy receipt](../../evidence/04A14/privacy.json) covers the **whole proposed set**, including decoded JSON/base64 capture content, escaped paths, known owner/host identifiers, SID/credential/path patterns. No unresolved finding; this is bounded scanning plus review, not proof that arbitrary secrets cannot exist. No ignore-rule change, private path alias masquerade or source-hash equivalence after redaction is claimed.

## Documentation and manual validation

Twenty method pages/subpages were audited; [documentation-impact.json](../../evidence/04A14/documentation-impact.json) lists each path/hash/disposition. Both landing pages were updated with truthful Office availability, historical screenshot qualification, existing KR/ML behavior and row-order/selection notes. Eighteen MMRM/LMM subpages remain unchanged because this extraction preserves their described numeric/control/output behavior; deeper mathematical/reference qualification was not asserted. Existing metadata/method IDs/versioned routes are unchanged. Strict MkDocs and Help/resolver/link/version tests pass.

**Office add-in execution is not yet available for either method.** Its mixed-role preview explicitly performs no LMM/MMRM fit. Windows tests establish neither Excel Online availability nor Mac desktop support. No screenshot was recaptured.

Later owner-authorized smoke plan, **NOT RUN**: MMRM repeated/missing visits with UN/CS/AR1 and KR+ML information message; observed versus reference-grid/group/control/baseline/contrast options; LMM intercept/slope with/without visit, text subjects and VC promotion; Data/model/trace sheets, fitted/residual RowIds, class/G/R/random-effect switches; progress/interrupt/cancel/close and large-workspace cleanup. Native source/binary/Excel attribution and original gate evidence remain prerequisites.

## Proposed owner acceptance

> Accept official 04A.14's MMRM/LMM Common request/effective-option/report-intent extraction and actual Windows delegates, with the recorded current-build focused qualification, full-suite nonpasses and preservation/source-plan exception. Retain all original OPEN obligations, C09/C11/C12 unapplied corrections, C13 scientific issue and candidate-only stricter input checks. This accepts software scope only: no P promotion, N1/N2 authorization, Gate C2 completion, control release, Git publication or 04A.15 authorization.

No additional owner timing/activation decision is required for the behavior-preserving shipping integration. Changes remain unstaged and uncommitted for the owner's single review.
