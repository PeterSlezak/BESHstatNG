# Official 04A.17 owner review

## 1. Verdict

**READY_FOR_OWNER_04A17_ACCEPTANCE_REVIEW.** Authorized software extraction and evidence work are complete, with the explicit nonpasses and qualifications below. This is one consolidated software-scope owner handoff; no self-acceptance, native release or scientific promotion.

## 2. Git baseline and authority

Branch `codex/04A-17`, from verified current local main/base/merge-base/HEAD **`13c6364f63fb8d4a254e5759e871896caf29cd7b`**. The live main commit contains accepted 04A.16 and has accepted 04A.15/14 ancestors. Entry tracked worktree and index were clean. No owner edits or untracked private evidence were absorbed. Creating the requested branch required sandbox permission for Git's branch lock; the safe-directory exception was command/process scoped. No global Git configuration changed.

Read root AGENTS and Master Plan Revision 2.8, official 04A.17/18/19/20 boundaries. The owner's explicit acceptance governs over the plan's stale “04A.14 next” prose. The plan remains byte-identical. Proposed owner progress amendment only: record 04A.14–16 accepted/merged and 04A.17 software extraction awaiting this review, without authorizing 04A.18 or changing gates.

[Prechange manifest](../../evidence/04A17/prechange-manifest.json), [27-file method/encoding inventory](../../evidence/04A17/prechange-method-inventory.json) and [source-specific implementation plan](../../evidence/04A17/implementation-plan.json) preceded production edits. The full initial tracked SHA map and historical census remain local. [Source contract](../../evidence/04A17/source-contract.json) pins ten modified existing files, new sources, exact 40 changed form methods and every unchanged form member; all 3,087 other original tracked files are byte-identical. No files were deleted. Full initial raw-map hash/size is an explicit private alias, not a public-derivative hash claim.

Everything remains **unstaged / uncommitted / unpushed**, index empty. No reset, clean, stash, historical deletion/rebaseline, package/network install, native Excel/COM/UIA, independent R, deployment, signing, publication, control release or obligation discharge occurred. No 04A.18 work was started.

## 3. Six real shipping routes and preserved host ownership

| Actual Ui11PCA route | Common and actual adapter integration | Preserved calculation / output / plots | Executable proof |
|---|---|---|---|
| `RunPCA` | `MultivariatePcaOptions`; `WindowsMultivariateWorkflowAdapter.Capture`; detached input roles, RowIds, labels/groups; `MultivariateReportPolicy.Project`; `RenderPca` consumes ordered plot requests | One original `Calculate`; Data, standardized and reduced blocks, PCA results; scree, scores2D, loadings2D, biplot 0/0.5/1, scores3D, loadings3D | Frozen choice/retention tests; 24 Core-backed profiles; source six-handler/native-writer tests; actual writer memory sink |
| `RunMCA` | Capture follows original `SubsetByRowIdValues`; categorical intent and report projection; `RenderMca` | Original `DataMultiple`/`Calculate`; Data/design/Burt blocks, MCA results; correspondence then column contributions axes 0/1 | Categorical/case fixture; acquired first-row/RowId regression; report/plot/source tests |
| `RunKmeans` | Common initialization/distance/standardization/missing/empty-cluster choice mapping; typed settings and seed sentinel; input-table composition; Capture/Project | Original `dataInputs`, optional imported centers, settings then one `Fit`; Data / KMeans results; no new GUI chart | 24 fixed-seed setting profiles, missing/listwise/centers negatives, labels and writer tests |
| `RunHierarchicalClustering` | Typed settings/report options configured at the original separate boundaries; copied identity and report; Common optional dendrogram plan is consumed at original post-write guard | One `Fit`; Data / Hierarchical results / optional Dendrogram; original renderer settings, title, 680×420 size, height/orientation/labels and cut inputs | 49 linkage/distance profiles retaining Core rejection boundaries; choice/plot/source/writer tests |
| `RunFA` | Typed extraction/retention/matrix/rotation/scoring/communality/missing options; shared original clustering-style input table; Capture/Project/RenderFactor | One `Calculate`; Data / Factor analysis results; scree, loading2D, loading3D, original renderer guards | 30 extraction/rotation profiles with matrix/retention/scoring variants; convergence flags and writer tests |
| `RunDiscriminantAnalysis` | Group-first role metadata; typed model and validation settings; Common original prior grammar, row-label fallback and input-table composition; Capture/Project | Original data/settings → conditional priorInputs → validationInputs → one `Fit`; Data / Discriminant results; no invented GUI chart | 24 LDA/QDA/prior/validation profiles; culture/case/parser/negative and writer tests |

All six create the report used by their existing `ProcessListofResultTables`/`ExcelDnaResultWriter` consumer. Report detachment is not an unused parallel output. Native model tables still originate in Core `wrapResults`; projection preserves their sequence, dimensions, scalar cell kinds, titles/headers/footers and p-value formatting coordinates. Rehydration uses the established Common mechanism. Repeated projections produce independent tables; Core tables are not appended, removed, reordered or mutated by Common.

`MultivariateLegacyChoices` is the explicit historical index/text compatibility boundary. Its typed return values use existing Core enums; Windows wrappers read current controls on each invocation and delegate. Identical hierarchical/ k-means standardization and missing mappings reuse one implementation. UI state callbacks continue calling those wrappers; no cache, duplicated parsing pipeline, event subscription or Designer was introduced. `MultivariateWorkflowPolicy` owns only portable role validation already performed by Windows, fallback labels, primitive seed intent and input-table construction.

Native acquisition, Excel numeric parsing, labels' per-cell CStr/catch behavior, control refresh, worksheet selection, workbook/sheet names, writer/plot timing and exception reporting remain Windows-owned. DA retains model configuration before prior parsing and validation configuration afterwards. Hierarchical settings and report configuration retain their original error-precedence boundary. The original btCalculate try/catch and complete SPM dispatch/`graphics.ScatterPlotMatrix` method are unchanged. The remaining numeric predictor conversion and RefEdit readers are acquisition, not a second domain engine.

Standalone simple CA is **shared-contract and Core-backed coverage only**. Its distinct `Ui0OneRefeditMulticol.RunCA`, `ckLabels`, CInt count conversion, output destination, overlap prompt, writer and five plot calls remain byte-identical. Full live simple-CA mapping is deferred to 04A.18/45–48. This is not represented as a seventh Ui11PCA route or as new successful native CA integration.

## 4. Typed contracts and behavior matrix

| Contract | Exact retained behavior / qualification |
|---|---|
| Acquired input | `MultivariateInputIntent` snapshots family, matrix shape/cell semantics, ordered column display names/source selection keys, supplied physical RowIds and optional user labels/display groups. DA's first column is a grouping role; other analysis columns retain order. No guessed worksheet addresses |
| Identity | Physical RowId, Core retained/removed positional index, user RowLabel and display class/category are distinct. Supplied arrays are copied and getters return copies. Absent provenance stays absent. The intent stores no second full-size matrix or Core fit/workspace |
| PCA | Correlation default; eigenvalue default cutoff 1, fixed alternative 2, variance alternative 80 percent, 50 solver sweeps and epsilon 1e-6. Existing eigenvalue/cumulative-variance `>=` boundaries and minimum-one-component Core clamp remain unchanged, including the old descriptive `>` caption. No production sign/subspace normalization |
| PCA auxiliary labels | After original Cells(RowIds(i), column) acquisition/conversion, selected labels trim; blank row label becomes RowId text; blank group becomes `(missing)`. Duplicate text is retained. Original row-label conflict precedes group conflict and same-column checks |
| MCA | Character importer with original 20000 argument; optional first acquired row removed through original positional dictionary plus physical RowId values before DataMultiple. Categories/case and original design/Burt/report behavior retained |
| K-means | K-Means++, squared Euclidean, no standardization, error-on-missing, farthest-observation recovery, three clusters, ten starts, 100 iterations, tolerance 1e-6. Forgy/random-partition/user centers, alternative distance/standards/listwise/empty strategies map exactly. Seed text uses the application prefill; blank is Integer.MinValue, not zero. Core retains time-based seeding |
| Hierarchical | Ward, squared Euclidean, no standardization, error-on-missing, Minkowski power 2; original linkage/distance restrictions remain Core rejections. Membership defaults to three clusters; optional height/cut/render choices remain distinct. Dendrogram initially selected |
| FA | Principal Axis/SMC/error-on-missing/Varimax/regression scores; fixed two-factor UI default. Matrix, all six extractions, three retention modes, five rotations, three score methods, Kaiser/Promax and convergence limits retain exact values. Percent is not converted to a fraction |
| DA | LDA, proportional priors, no standardization, error-on-missing, regularization 1e-8, validation None; folds/holdout/stratification read existing controls. Core option defaults and actual UI snapshots are distinct: the form passes the checkbox value, including its original unchecked stratification default |
| Prior grammar | `=` preferred, then `:`; semicolon/CRLF/CR/LF separators; trim label/value, current-culture numeric parse then invariant with original Float/AllowThousands behavior. Raw case, duplicates, NaN/Infinity, invalid probabilities and sums go to unchanged Core validation; no silent normalization |
| Cluster/DA labels | Blank selected label becomes `Row {RowId}`; nonblank whitespace/case is otherwise retained. Cluster/FA input table uses OriginalRow and optional RowLabel. DA's distinct table uses optional “Row label”, grouping header, then predictors |
| Portable candidates | Nonblocking shape/name/role metadata diagnostics use existing RegressionDiagnostic/Validation. Unknown/null legacy choices retain original fallbacks or exceptions. Candidate metadata does not introduce GUI rejection, dialog text or refusal timing |
| Report / plots | Detached AnalysisResultDocument and ordered immutable plot requests reference existing Core data roles/axes/scaling. No eigensolver, biplot geometry, distance, rotation, dendrogram or DA probability engine in Common. Dimensionality descriptors describe requirements; native renderers keep existing guards/errors |

Primitive option-object defaults do not replace a raw GUI snapshot: for example, the unprovided FA retention value is the existing Core count 1, while the GUI explicitly supplies its selected count 2; DA's primitive stratification default True is explicitly replaced by the original checkbox value. All six shipping routes project current controls, including disabled values where previously read. Consumers seeking GUI parity must supply that same explicit snapshot rather than assume all parameterless Core/default choices equal GUI defaults.

API example: after host acquisition, capture a `MultivariateInputIntent`; configure the existing model with typed family options; run the original Core calculation once; call `MultivariateReportPolicy.Project(intent, model.wrapResults())`; pass `LegacyTables()` to the existing writer and consume `Plots` with the Windows renderer. Simple CA can use the same detached CA intent/report plan without changing its native entry point. Caller-supplied matrices are borrowed for immediate Core/table calls, not retained by the intent.

GUI/UDF comparison is limited to deliberately equivalent explicit Core requests. Existing source distinctions are preserved: PCA UDF defaults to 250 solver iterations versus the GUI's 50 and uses a different report selection caption; FA UDF rotation defaults to None versus GUI Varimax; DA UDF stratification defaults True versus the original unchecked GUI checkbox. UDF optional priors are separate label/probability arrays rather than the GUI text grammar. UDF matrix/header/label handling and positional row identities are not the GUI's physical Cells(RowIds(i), column) acquisition. K-means UDF user-center arrays are not the physical RefEdit importer. Both stochastic UDFs retain Integer.MinValue for absent seeds. None of these differences is silently normalized or presented as native/UDF end-to-end parity.

## 5. Compatibility and scientific qualifications

[Frozen selector provenance](../../evidence/04A17/legacy-selection-provenance.json) derives 25 original form option functions and the actual prior parser before production changes. Test adaptations replace only visibility/control acquisition with explicit parameters; parser ordering/messages and choice branches remain original. [Comparisons](../../evidence/04A17/multivariate-comparisons-v1.json) records new versioned same-Core executions, not scientific goldens. Fixed tolerance, chosen before executing comparisons: **max(1e-9, abs(expected)×1e-9)** for finite doubles; exact labels/CLR kinds/nonfinite classes/shapes/order/metadata. No tuned tolerance, timing mask or production sign canonicalization.

153 request profiles cover PCA 24, CA one, MCA one, k-means 24, hierarchical 49, FA 30 and DA 24, across invariant/sk-SK and net10/net48 in Debug/Release. These are source-backed synthetic compatibility fixtures, not independent numerical references. Existing Core and Windows numerical reference suites/fixtures run unchanged. Rejected Core combinations stay rejected. Completed calculation, k-means convergence and FA extraction/rotation flags are separately recorded; completion is never renamed convergence. Negative calls retain original return/exception classes and messages.

The ledger contains **1,224** profile executions and **48** negative-call records. Of the profile executions, **120** preserve original hierarchical ArgumentException rejections; **192** k-means runs report convergence; **168** FA runs report extraction/rotation convergence and **72** retain nonconverged extraction with converged rotation. The other **672** record completed Core calls without inventing an iterative convergence status. Thus 1,104 returned calculations is not a claim of 1,104 converged fits. Each logical request is repeated across cultures/frameworks/configurations; these execution counts are not distinct statistical datasets.

Existing PCA scores/eigenvalues/eigenvectors/loadings/standardized arrays, CA/MCA factor/contribution/label data, clustering/DA retained and removed identities, FA loading/score/row arrays and report metadata are compared. Hierarchical geometry remains its existing Core/renderer responsibility; 04A.17 tests original membership/merge reports and request intent, not a new geometry engine. Fixed seed 1729 is used for stochastic equivalence. Blank random sentinel is tested as an option only; nondeterministic runs are not presented as exact equality.

Candidate issues remain explicit:

- **C17-CA-NOLABEL-INDEX-01**: unchanged standalone no-label `j=0` / assignment `TableData(i,j-1)` throws on nonempty input. Host-free exact-loop regression preserves this visible failure. Owner: 04A.18/40–41 scientific/input characterization and 04A.45–48 native closure; no patch applied.
- PCA eigenvector signs/tied or near-tied subspaces, current biplot geometry/orientation and old `>` descriptive caption versus Core `>=` remain characterized limitations for 04A.18/29–36 and later scientific/native review.
- MCA categorical/first-row identity and DA case/prior parsing remain compatibility semantics, not scientific validation or a promise to normalize labels. Detailed cluster initialization/cuts/geometry remain 04A.19; FA/DA semantics remain 04A.20.

Independent R is **NOT RUN / NOT AUTHORIZED**. Native Excel/XLL/COM/UIA, Mac, Office/browser manual execution and PCA case capture are **NOT RUN**. Approved new goldens: **none**. Same-Core equality, chart specs, old R-derived fixtures and source evidence cannot establish independent statistical or native equivalence.

## 6. Validation and every nonpass

| Suite/accounting | Debug pass/total | Release pass/total | Exit / qualification |
|---|---:|---:|---|
| Core managed | 1,058/1,058 | 1,058/1,058 | 0 |
| Common managed net10 | 3,059/3,064 | 3,059/3,064 | 1 / five inherited |
| Windows managed net48 | 3,072/3,074 | 3,072/3,074 | 1 / two inherited, one shared with Common |
| Office add-in managed | 505/505 | 505/505 | 0 |
| Office execution/WASM managed | 45/45 | 45/45 | 0; managed harness, no browser/native execution |
| Control mitigation managed | 61/61 | 61/61 | 0 |
| Raw managed framework executions | **7,800/7,807** | **7,800/7,807** | Seven runtime nonpasses |
| Distinct logical managed cases | **5,998/6,004** | **5,998/6,004** | Six inherited logical nonpasses |
| New 04A.17 logical managed | 476/476 | 476/476 | Included above; 467 linked Common + nine Windows-only |
| Node | 916/916 | 916/916 | 0; 468 + 71 + 377 |
| Python distinct unittest methods | **704/783** | **704/783** | 1 / 78 entry nonpasses + one new source-placement assertion |
| New 04A.17 Python contracts | 8/8 | 8/8 | 0, included above |
| Complete canonical workflow derivative | NONPASS | NONPASS | 1 / 1 |
| Historical strict / profile | NONPASS | NONPASS | Configuration-independent exits 1 / 1 |
| Strict MkDocs, final Help bytes | PASS | PASS | Configuration-independent exit 0 |

[Final validation](../../evidence/04A17/validation.json) keeps counters, raw receipt hashes and failure identities. Five string-option null/empty cohorts each retain two distinct authored logical cases across four runtime executions: MSTest 2 displays null and empty identically, while MSTest 3 differs. Source SHA and both runtime memberships are checked before applying weight two; neither case nor failure is dropped. New source-contract tests and final publication verification were rerun after evidence/accounting-only edits; executable production/test code did not change after final full runs.

Measured entry Debug: Core 1,058/1,058, Common 2,592/2,597 and Windows 2,596/2,598. Node entry is 916/916 (468 Office, 71 Help, 377 smoke). Comparable entry Python is **697/775 with 78 nonpasses**. Original missing-environment Common probe is retained locally; fresh unchanged C02 representative observations qualify the comparable probe. Entry Release/Office managed totals were not separately measured.

Accepted 04A.16 reported 699/775, not a green baseline. Entry adds two already-failing identities before production edits: `test_04a16_post_estimation.Mixed16SourceContracts.test_branch_base_index_and_exact_scope` requires codex/04A-16; `test_unchanged_methods_designers_udfs_and_core` expects the old raw CRLF SHA of `mmrm/lsmeans-and-contrasts.md`. Main's committed LF bytes differ from that frozen raw-public pin. Actual entry main bytes are frozen and preserved here; old evidence/assertions are not rewritten.

The six inherited logical managed failures remain the three `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection` rows (20,15,40), (0,0,0), (20,30,50); `ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`; `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`; and shared `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings(True)`. Their original mapping/refusal/overflow/constant-binomial exception expectations stay failing. Seven raw runtime failures represent six logical cases.

[Nonpass register](../../evidence/04A17/nonpasses.json) lists every named Python/managed failure/error, entry inheritance, newly triggered predecessor assertion, sanitized terminal/root cause and private raw traceback/log hash. No nonzero suite is called passing. Full managed cases deduplicate exact linked source/class/method/data-row identities with the established display normalization/null-cohort weights; raw executions remain separate. Focused runs and C02 environment observations are excluded from aggregate totals.

The single newly triggered final Python identity is `test_investigation06.Investigation06Tests.test_pca_has_three_biplot_scales_not_the_old_two_scale_proposal`. Its frozen assertion requires three literal Biplot calls in the form; the actual consumed Common plan and Windows adapter preserve all three. It remains **FAIL**, with passing successor plan/handler/writer contracts, rather than an edited assertion. Earlier privacy failures caused by mathematical colon/backslash fragments were fixed by the stated harmless TeX spacing and do not remain in final totals. Inherited method identities can have additional source-pin differences after this authorized extraction; the register preserves both entry/current traceback hashes and marks changed terminal details instead of assuming identical root causes.

Review corrected new correspondence/contribution dimensionality descriptors and added assertions. Earlier Debug is retained as preliminary and excluded from final totals; final Release and Debug-complete follow final production/test edits. Final-source qualification and any evidence-only helper changes are recorded in validation JSON. No final behavioral edit is hidden behind an earlier full run.

Public command aliases use the configured existing WinPython interpreter and VS2026. Exact resolved commands/exits/raw receipt hashes are in [command outcomes](../../evidence/04A17/command-outcomes.json). Core workflow commands:

```text
PYTHON_EXE -B scripts/verify-04a17.py init --evidence TestResults/04A-17-20261104-run01
PYTHON_EXE -B scripts/capture-04a17-contract.py audit
PYTHON_EXE -B scripts/verify-04a17.py entry --evidence TestResults/04A-17-20261104-run01 --name entry-Debug
PYTHON_EXE -B scripts/verify-04a17.py entry-python --evidence TestResults/04A-17-20261104-run01 --name entry-python
PYTHON_EXE -B scripts/verify-04a17.py semantic --evidence TestResults/04A-17-20261104-run01 --name entry-comparable
PYTHON_EXE -B scripts/verify-04a17.py common-python --evidence TestResults/04A-17-20261104-run01 --name entry-comparable
PYTHON_EXE -B scripts/verify-04a17.py focused --evidence TestResults/04A-17-20261104-run01 --name final-focused --windows
PYTHON_EXE -B scripts/verify-04a17.py final --evidence TestResults/04A-17-20261104-run01 --name final-Release --configuration Release
PYTHON_EXE -B scripts/verify-04a17.py final --evidence TestResults/04A-17-20261104-run01 --name final-Debug-complete --configuration Debug
PYTHON_EXE -B scripts/verify-04a17.py docs --evidence TestResults/04A-17-20261104-run01 --name mkdocs-final
PYTHON_EXE -B scripts/verify-04a17.py historical --evidence TestResults/04A-17-20261104-run01
PYTHON_EXE -B scripts/verify-04a17.py preserve --evidence TestResults/04A-17-20261104-run01 --name final-complete
PYTHON_EXE -B scripts/capture-04a17-contract.py source
PYTHON_EXE -B -m unittest discover -s scripts/tests -p test_04a17_multivariate.py -v
PYTHON_EXE -B scripts/summarize-04a17.py entry|validation|numerical|nonpasses|retention|commands
PYTHON_EXE -B scripts/verify-04a17-publication.py seal
PYTHON_EXE -B scripts/verify-04a17-publication.py verify
git diff --check
git status --short
git diff --cached --name-only
```

Canonical `scripts/verify-beshstatng.ps1` portable/Legacy/WASM/JS/Python flows use inherited exact-anchor isolated execution derivatives with `powershell -NoProfile -ExecutionPolicy Bypass -File LOCAL_DERIVATIVE -Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe PYTHON_EXE -Configuration Debug|Release`. Literal default execution was not used because it overwrites protected historical TestResults/Codex and invokes packaging/signing hooks. [Adaptations](../../evidence/04A17/execution-adaptations.json) retains original/executed SHA and exact sanitized diffs. Actual host and net48 tests compile through safe managed targets with no restore/install. Independent suites continue while aggregate failure remains nonzero.

Historical strict retains exit 1 for stale UibyID/NormalPlot review dependency; profile retains exit 1/INTEGRITY_FAILURE. Both remain nonpassing and their historical authority is unchanged. Strict MkDocs exits 0, including the final `mkdocs-final-complete` rerun after TeX spacing; existing unlisted family-link-covmat INFO is not hidden or promoted to failure. [Final source qualification](../../evidence/04A17/final-source-qualification.json) verifies all ten compiled VB/project source pins against the final aggregate source snapshot and lists later nonbehavioral Help/helper deltas. Traceback-segment digests use decoded UTF-8/LF text; separate whole-log hashes identify exact private raw bytes.

## 7. Preservation and privacy

[Retention](../../evidence/04A17/retention.json) verifies all **208,946** initial root and **139** nested historical files by membership, size, creation/write nanoseconds and file identity. Frozen captures/ledgers/protected source additionally retain SHA-256/bytes. This is not a claim of rereading every historical byte. No cleanup, compression, relocation, overwrite, regeneration or rebaseline occurred. New raw logs/TRX, baseline maps and development originals remain in the exclusive ignored 04A.17 local root.

[Retained evidence](../../evidence/04A17/retained-evidence.json) includes all 100 original 04A.14 NOT_GOLDEN captures, 04A.15's unchanged 1,408 comparisons (712 converged, 696 nonconverged), and accepted 04A.16 evidence/ledgers. Prior raw-public newline pin inconsistencies remain historical facts, not repaired evidence. The 177/69 OPEN/67 restricted mixed-model lineage remains unchanged, zero releases/discharges.

## 8. Native restrictions

[Original control crosswalk](../../evidence/04A17/original-control-crosswalk.json) and [native lineage successor](../../evidence/04A17/native-lineage-successor.json) enumerate exact original IDs/row hashes, current unchanged Designer declaration IDs, original PCA case and shared RefEdit edge. Authority includes the accepted C02 amendment `8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307` and owner acceptance receipt; no retrospective signature or native execution is inferred.

| Distinct authority population | Original controls | Linked OPEN obligations | Restricted original controls |
|---|---:|---:|---:|
| Current Ui11PCA Designer/ledger | 147 | 145 | 75 |
| Current Ui0OneRefeditMulticol host/ledger | 39 | 7 | 3 |
| Historical PCA case 04A02-PCA-NATIVE-01 | 75 blocked | 76 deferred | Historical case population only |
| Original mixed-model lineage | 177 | 69 | 67 |

These overlap and must not be added. Current declarations match every original control ID, with no invented new live-control population. Original PCA native execution remains BLOCKED/NOT_RUN. Shared edge `BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb|WithEvents|xlApp@34` remains UNKNOWN_HIGH with original 04A.27/45/46/47 ownership. The application/shared RefEdit scope was not expanded or released.

Wider 04A.02 populations remain 1,514 original obligations/802 restricted controls, 239/218 transferred and all original case-specific timing restrictions, unchanged and nonadditive. C09-NATIVE-STRICT-01, C11-NATIVE-CORRECTIONS-01 and C12-NATIVE-CLASSIFICATION-01 patches remain UNAPPLIED. C13-LOGRANK-R-01 earliest-time censor/risk-set scientific defect remains for 04A.40–41. C14-INPUT-FINITE-01 stays candidate-only/nonblocking; accepted 04A.15 option and 04A.16 inference decisions are retained.

**P NO / N1 NO GO / N2 NO / Gate C2 UNMET.** Form changes are software ownership/delegation only. No newly activated stricter refusal, behavior-changing patch or exact-case timing exception is proposed; no additional native disposition is required for this preserving extraction. All original native prechange/loaded-build attribution requirements remain in force.

## 9. Documentation

Seven Help pages receive one narrowly scoped paragraph each: `principal-component-analysis.md`, `correspondence-analysis.md`, `multiple-correspondence-analysis.md`, `k-means-clustering.md`, `hierarchical-clustering.md`, `factor-analysis.md`, `discriminant-analysis.md`. [Documentation impact](../../evidence/04A17/documentation-impact.json) pins them. Paragraphs clarify unchanged Windows labels/roles/options/seeds/priors/table and plot limits, the CA no-label defect, and absence of these dialog/chart workflows in Office/Excel Online. The PCA page also adds two spaces after mathematical set-builder colons: the old TeX fragments resembled drive paths to the existing whole-publication privacy checks. Equations/rendered meaning are unchanged; strict MkDocs was rerun afterwards. Screenshots, data, numerical example values, authored metadata, Help/ribbon links and public UDF declarations are unchanged. No fabricated task-pane instructions or screenshots.

## 10. Scope, future ownership and bounded acceptance

Recommended wording: “Accept 04A.17 software scope: shared typed multivariate selection/input/report/plot preparation is used by the six actual Ui11PCA analytical routes, with the named nonpasses and qualifications retained. Standalone CA native integration/defect correction remains deferred. This acceptance grants no statistical golden promotion, native execution/control release, obligation discharge, Gate C2 completion, Git publication or 04A.18 commencement.”

04A.18 owns deeper PCA/CA/MCA transformation and score/sign/subspace consistency; 04A.19 cluster initialization/distance/assignment/dendrogram detail; 04A.20 FA/DA inference/rotation/retention/prior/validation detail. 04A.29–36 retain renderer/geometry/native chart parity. 04A.40–41 retain separately authorized scientific corrections; 04A.45–48 retain native capture/attribution/release and Gate C2 closure. Independent reference review/promotion remains separate.

Suggested later native checks, **NOT RUN**, needing separate owner authority and original prechange/build attribution:

1. PCA on a disposable workbook with noncontiguous source rows, blank/duplicate labels and grouping: Data/results ordering, score/loading series, all three biplot scalings, dimension guards and native sign/geometry capture.
2. Fixed-seed k-means with row labels and user centers imported through the physical RefEdit path: OriginalRow/RowLabel, retained/removed identities, assignment/report placement, rerun/sheet switch and error cleanup.

Stop here for owner software-scope acceptance. No stage/commit/merge/push, native/R execution or 04A.18 work follows by implication.

## 11. Final publication list

Modified existing paths (ten): Ui11PCA; Windows project (one Compile entry); Windows test project (three Compile entries); the seven named Help pages. New sources: Common MultivariateLegacyChoices/MultivariateWorkflowPolicy/MultivariateReportPolicy; WindowsMultivariateWorkflowAdapter; frozen LegacyMultivariate17Selection, Multivariate17Tests and Multivariate17AdapterTests; four 04A.17 verification/capture/accounting/publication scripts; the new Python source-contract file. This dossier and the enumerated evidence JSON files complete the candidate set. No Core, renderer, UDF/importer, Designer/resx, Framework/dependency, metadata, Master Plan, fixture CSV/newline or unrelated form change is included.

[Exact publication manifest](../../evidence/04A17/publication-manifest.json) lists **every path, modified/new status, category/role, SHA-256 and bytes**; deleted paths are empty. Its self-hash alone is excluded to avoid a cycle and checked separately. [Privacy receipt](../../evidence/04A17/privacy.json) scans the complete set, decoded JSON/base64, Unicode/case variants and owner/host/profile/SID/secret patterns. Raw/public hashes are explicitly distinct; no absolute profile path, raw workbook data, local receipt or large historical result is proposed.

All candidates pass live ordinary git check-ignore; new files also pass --no-index. Already tracked Help pages retain their inherited broad Help-docs ignore rule under --no-index, with both exits recorded. JSON parsing, allowed enumeration, hash/size membership, empty staging index and git diff --check are verified. Existing CRLF-to-LF warnings do not represent a separate line-ending-only publication change; original form encoding/newline/EOF state is preserved. No .gitignore edit or excluded owner/private path was absorbed.

The final inventory contains **44 candidates: 43 externally hashed files plus the separately checked manifest itself**. The complete list is the linked manifest, with modified/new flags, source/result role, categories, SHA-256 and bytes for each external file. No line-ending-only file or private/nonpublication path is included. Nothing is staged, committed or pushed.
