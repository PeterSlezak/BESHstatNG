# Official 04A.25 owner review

## 1. Decision

`READY_FOR_OWNER_04A25_ACCEPTANCE_REVIEW`

Accept **04A.25 software extraction only**. This is a proposal for owner review, not owner acceptance, a statistical golden, native release or general project readiness. `P=NO; N1=NO GO; N2=NO; Gate C2=UNMET`. Zero controls released and zero obligations discharged. Git publication and 04A.26 require separate owner actions.

## 2. Verified baseline and authority

Actual clean merged `main`, `origin/main` and entry HEAD were `bd46904dca7f3860d799e2d6d9c069de9f2fb13a`. Local main reflog records `bd46904 merge codex/04A-24: Fast-forward`; the feature pointer is the same commit and ancestry succeeds. Parent `44b32955bb4f1b3aa43a9b65e42564c4fc9d436c` is the prior entry base, not the post-merge main. The owner reports formal 04A.24 acceptance; local Git establishes commit/merge ancestry and presence of the accepted dossier, not an independent human approval signature. No fetch or ZIP SHA inference was used.

Branch is `codex/04A-25`; final HEAD/main remain the verified base. Entry was clean, with empty index and no owner planning edits. Creating the authorized branch required the approved sandbox ref write; no global Git trust/configuration was changed. Per-command `safe.directory` handled the sandbox ownership mismatch. Nothing staged, committed, merged, pushed, reset, rebased or stashed. The Master Plan SHA and all owner text remain exact. Rev 2.9's official 04A.25 row governs this batch; its stale progress header is only a separate recommendation in `plan-progress-recommendation.json`.

Entry captured live compiled-source and authority hashes, seven original PSM screenshot/workbook asset hashes in the private all-tracked baseline, 259,051 historical result file identities/metadata and 1,714 current authority/corpus file hashes. Complete entry Debug and Release matrices ran before shipping consumers changed. Two new Common declarations existed without tests/consumers at that measurement; exact entry build pins record this qualification. Historical 04A.24 measurements are context, never substituted for the live entry counts.

## 3. Shipping call graph and changed symbols

`GetData -> psmData.DataImportFromWorksheet -> original imports, RowIds intersection/subset, design expansion and BuildPsmInput -> Core PsmInputData + Common PsmAlignedInputAxis`. Both native worksheet and raw-matrix paths retain their conversion, exception and early-return boundaries. `GetData` carries the detached axis, and the real writer uses its copied physical source rows.

`RunPSM -> BuildFitOptions -> Windows ParseUiDouble/ParseUiInteger/ParseEnum and original clamps -> PsmRequestSelection.CreateCemSpec / selected snapshot / CreateEffectiveOptions -> exactly one PsmComprehensiveBackend.Fit(myData.Input, fitOptions)`. Core capability APIs remain the authority. RunPSM, progress/DoEvents and outer exception/finally bodies remain entry bytes after LF method normalization. Core algorithms/options/table generators are unchanged.

`WritePsmResults -> original second BuildFitOptions -> native workbook -> PsmResultPresentation + phased PsmReportSelectionPolicy -> PsmReportSheet.BuildTables -> existing PsmFormattedResultTables -> native ExcelDnaResultWriter / ProcessListofResultTables`. Separate diagnostic, Love, match and CEM phases preserve original checkbox-read chronology, including one shared diagnostics decision for Diagnostics plus Row Audit.

`WriteLovePlotSheetIfAvailable -> one PsmLovePlotProjection -> PsmLovePlotExcel.BuildProjectedDataTable` **and** `AddProjectedChart`. Existing public `BuildPlotDataTable(rows)` and `AddChart(ws, rows, left, top, width, height)` remain compatibility facades. Both real table/chart paths consume Common's frozen ordered values. The entire native chart construction/style/labels/threshold/axis tail is source-equal after replacing the two count references. Existing helper geometry, colors, markers, labels, COM ownership and chart-warning catch remain Windows-owned.

| Existing compiled source | Modified methods |
| --- | --- |
| `BESHstatNG/src/UI/Ui20PropensityScoreMatching.vb` | GetData, BuildFitOptions, InitializeDefaultUiValues, SetOptionControlsForCurrentSelections, RefreshEstimandChoicesForRunMethod, NormalizeFitOptionsForRunMethod, WritePsmResults, WriteLovePlotSheetIfAvailable; added: none |
| `BESHstatNG/src/DM/PSMdataImport.vb` | ResetPsmImportState, BuildPsmInput; added: none |
| `BESHstatNG/src/Graphics/PsmLovePlotExcel.vb` | BuildPlotDataTable, AddChart, AbsForChart, AbsOrMissing, MaxFiniteAbs, MaxFinite; added: AddProjectedChart, BuildProjectedDataTable |

Five new strict/netstandard2.0 Common files: `PsmRequestSelection.vb` (selected/effective copies, GUI defaults, capability display intent), `PsmAlignedInputAxis.vb` (source/ordinal/ID/role axes), `PsmResultPresentation.vb` (stored Core link/weight/support/trim/stratum metadata), `PsmReportSelectionPolicy.vb` (ordered existing table routing), and `PsmLovePlotProjection.vb` (one table/chart projection). Small identity/options arrays are copied, including nested CEM cut points; covariate matrices are never copied or expanded in Common. No UI/COM/filesystem/static mutable state enters these contracts.

Other changed tracked files are four test Compile additions in `BESHStatNG.Tests.vbproj` and the same-batch PSM Help page. New managed tests/fixture/source-pinned legacy derivative, Python contracts and batch-qualified verification/evidence helpers are additive. Exact candidate paths, bytes, modes and hashes are in `publication-manifest.json`. Source and Designer/UDF/Core boundaries are independently checked in `source-preservation.json` and `final-source-qualification.json`.

Untransferred Windows work is explicit: worksheet/RefEdit/COM acquisition, role-list validation/messages, culture parsing, DataObj coercion/alignment/design construction, control events/retained disabled text, live import summary strings, workbook/sheet naming/activation, formatting, chart geometry and catches. BuildFitOptions still parses the same disabled numeric text as before; Common display intent does not clear or reinterpret it. The second options capture remains; no re-entrant UI defect fix is activated.

## 4. Before/after behavior

| Route / concern | Before and after retained behavior |
| --- | --- |
| NN | ATT/ATC; ratio/replacement/order/distance/caliper/seed choices; stored set/focal/matched/treated/control indices and multiplicity |
| Optimal pair | ATT/ATC; effective 1:1, without replacement, AsInput; one authoritative Core optimal engine |
| Subclassification | ATT/ATC/ATE/ATO; Core strata/weights/effects; inactive matching options reset exactly |
| Weighting only | ATT/ATC/ATE/ATO; Core weights/diagnostics; no manufactured pair count or pair links |
| CEM | ATT/ATC/ATE/ATO; GUI default covariate/score bins 5; Core retained/dropped strata, weights, keys and CEM tables |
| Defaults | GUI tolerance/ridge 1e-7, max iterations 100, ratio 1, strata/CEM bins 5, overlap bins 20, trim [0,1], threshold 0.1, extreme cutoff 10, empty caliper NaN; hidden seed 12345. Core/UDF raw ridge 1e-6 and Core CEM covariate bins 4 remain distinct |
| Controls | Capability-derived allowed estimands/enabled state; native mutation timing unchanged. Mahalanobis-within-caliper selects standardized-logit/0.2 only under the original scale/empty-text/dropdown conditions |
| Axes | Physical RowIds retain order/nonconsecutive values; Core ordinal is zero-based; duplicate/blank display IDs are not uniquified. Exact columns join in source order using literal ` | `, Nothing to empty string |
| Reports | Input Data, Results, optional Diagnostics/Row Audit, optional Love Plot, optional Matched Pairs/Matched Data, actual CEM; original Core table order/titles/footnotes and skip-empty-list rules. A Core empty-message table can still create a sheet |
| Love data | Filter missing/blank names; descending maximum finite absolute SMD, then OrdinalIgnoreCase name with stable ties; same seven columns, missing SMD cells, finite masks, series order/labels/threshold/fallback geometry |
| Native compatibility | Same public renderer/UDF signatures, first-sheet reuse, unique sanitized names, final Results activation and warning catch. No native appearance claim |

## 5. Same-Core compatibility evidence

Corpus: **3,840 execution records / 1,920 content-addressed payloads**: **2,264 POSITIVE_PARITY**, **1,448 PRESERVED_CORE_REJECTION**, **128 NONCONVERGED**. Every comparison is `NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY`. The 960 parameterized main cases span both cultures, five methods, all estimands, score/distance/caliper/order/support/trim choices, ties/replacement/large ratio/no-match cases and invalid inputs. Each case compares original normalized request intent, then deterministic same-Core results/rejections; report comparisons use the same returned Core result. Two Fits occur only in the test comparison, never in the shipping RunPSM.

Payloads pin synthetic fixture/source hashes, culture/runtime/configuration, exact option snapshots/seed, physical row IDs and display/category fingerprints, all match indices/order/multiplicity, selected score/weight/result hashes, balance dimensions, warnings, exceptions and exact typed table model hashes. IEEE Double bits are preserved; no statistical tolerance or independent estimator is introduced. Corpus buckets are bounded at 128 payloads. Separate 320 executed Love records retain labels/channels/threshold/max-axis bits and compare the frozen prechange Windows pure planner. Thirty-two report-toggle combinations and raw host import/formula tests supplement the corpus.

Original Core PSM tests remain scientific/compatibility authority; no R run, internet fixture, independent numerical oracle or golden promotion occurred. Match ID/reproducibility, trimming and SMD independent-golden risks remain open. Prior 04A.09/04A.02 and 04A.24 corpora remain immutable. Actual prior hash reconciliation: {'EXACT': 1666}; all 100 mixed-model captures match their original registry hashes.

Five OPEN/CANDIDATE_ONLY/UNAPPLIED items in `candidate-issues.json`: re-entrant second request snapshot; nonfinite Love threshold boxing zero; all-missing Love x-axis fallback 0.25 despite a larger threshold; raw text model covariates retaining the existing numeric-import no-valid-rows result; source-observed GUI/UDF default/order differences. Numeric-coded factor/interactions are tested. No correction, fallback, extra rejection, source geometry fix or independent scientific conclusion is activated.

## 6. Test inventories and nonpasses

**1,102 new logical managed cases** (1,082 linked Common plus 20 Windows adapters) and **20 new Python methods** pass in both configurations. Raw new managed executions are 2,184 per configuration. Full prior managed membership/weights reconcile exactly using canonical source/case fingerprints and cohort multiplicities, independent of TRX receipt paths/discovery order; focused and full new discoveries are checked. Raw execution fingerprints remain separately preserved. Original tests/expectations are unchanged.

| Configuration | Live entry logical managed | Final logical managed | Final raw managed | Node | Qualified Python methods |
| --- | ---: | ---: | ---: | ---: | ---: |
| Debug | 10,267/10,280 | 11,369/11,382 | 18,347/18,361 | 916/916 | 807/910 (93 FAIL / 10 ERROR) |
| Release | 10,267/10,280 | 11,369/11,382 | 18,347/18,361 | 916/916 | 807/910 (93 FAIL / 10 ERROR) |

Entry raw managed was 16,163/16,177 and Node 916/916 per configuration. Actual entry Python was 786/890 (94 FAIL / 10 ERROR), versus the historical 04A.24 789/890. Entry added two new-evidence privacy failures and the prior batch publication-membership/branch-domain pin. The privacy defects are corrected and requalified, not relabelled inherited.

Initial final Python was 805/910 (95 FAIL / 10 ERROR). Evidence-only sanitization corrected two privacy methods; final qualified results use the complete repeated 699-method scripts suite per configuration, plus that configuration's already executed other roots. **910 unique methods executed; zero NOT_RUN_REUSED_EVIDENCE method passes; 1,609 final-phase raw method invocations per configuration including repeats.** Initial failures/commands/traces remain preserved. Synthetic identity strings are sanitized; raw identity SHA-256 and private raw TRX hashes retain forensic membership. Complete identities and FAIL/ERROR traces are in the entry/final logical/raw/Python files, `nonpasses.json`, `initial-full-matrix-validation.json` and `python-requalification.json`.

The 13 logical / 14 raw managed nonpasses are unchanged from entry (raw identities below include the linked Framework repetition):

- `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (0,0,0)`
- `ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`
- `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings (True)`
- `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,30,50)`
- `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection (20,15,40)`
- `Spc21AdapterTests.NativeHostCoercionStateFitOutputAndChartTailStayEntryBytes (WriteResultsToNewWorkbook)`
- `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (0)`
- `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (5)`
- `Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings (True)`
- `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (3)`
- `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (1)`
- `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (4)`
- `Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer (2)`
- `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`

The one newly triggered protected Python method is `MultivariatePayload24.test_original_numerical_ui21_renderer_and_prior_common_hashes`: its original Graphics pin includes `PsmLovePlotExcel.vb`, now legitimately extracted. Its failed assertion is retained. New compiled shipping-route/table/series parity successors pass. All remaining failed/error identities are retained in machine-readable evidence; no suite is called all-green.

## 7. Validation commands, isolation and timing

All actual exact arguments, root PIDs, exits, source/binary/log hashes and durations are in `command-outcomes.json`; private receipts stay under `TestResults/04A-25/run01`. Public executable/root aliases redact private paths. Representative exact commands:

```text
<PYTHON_EXE> -B scripts/verify-04a25.py build --name delivery-build-Debug --configuration Debug
<PYTHON_EXE> -B scripts/verify-04a25.py build --name delivery-build-Release --configuration Release
<PYTHON_EXE> -B scripts/verify-04a25.py matrix --name final-Debug --configuration Debug
<PYTHON_EXE> -B scripts/verify-04a25.py matrix --name final-Release --configuration Release
<PYTHON_EXE> -B scripts/verify-04a25.py source --name requalified-Debug-python-scripts --configuration Debug
<PYTHON_EXE> -B scripts/verify-04a25.py source --name requalified-Release-python-scripts --configuration Release
<PYTHON_EXE> -B scripts/verify-04a25.py serial --name final-serial
git -c safe.directory=<REPO_ROOT> diff --check
git -c safe.directory=<REPO_ROOT> diff --cached --name-only
git -c safe.directory=<REPO_ROOT> status --short
```

Build commands are offline `dotnet build --no-restore -m:4`, sequential by project/configuration; host MSBuild uses PrepareForBuild/PrepareResources/Compile/CopyFilesToOutputDirectory, then Framework tests and isolated ControlMitigation build. Both standard targets and 32/64-bit packaging definitions stay unchanged. Test commands use copied prebuilt binaries, MaxCpuCount=1/MSTest Workers=1, unique TRX/sandbox/exports and copied project-relative TestData. Optional full-suite C02 observation writers remain unset; the isolated semantic-only probe feeds Common Python. No nested build or runner fanout.

Four command roots maximum: A Core/Common; B Windows/ControlMitigation; C Office/WASM managed plus JS/resolver/WASM Node; D sequential Python/source. D starts first; no fifth worker. Node concurrency is one. Strict/profile/MkDocs and terminal integrity/publication checks run serially. The original `verify-beshstatng.ps1` and 04A.24 runner remain unchanged. The original script was **not executed in place** because its fixed TestResults/Codex destination and restore-capable dotnet test conflict with preservation/offline requirements; equivalent portable, Legacy, Office/WASM, Node and Python commands run through the new isolated runner.

| Final matrix seconds | A | B | C | D | Concurrent wall |
| --- | ---: | ---: | ---: | ---: | ---: |
| Debug | 111.11 | 152.14 | 58.72 | 333.34 | 333.34 |
| Release | 123.60 | 158.56 | 63.78 | 392.26 | 392.26 |

Final matrix wall total is **725.60 s**; additional complete Python-source requalification is **283.75 s**. Final builds took **41.55 s** and final serial strict/profile/MkDocs commands **23.56 s**. Build command sum is 218.81 s across all retained entry/development/final attempts. End-to-end measured prefix is **5409.95 s** from entry-freeze creation through terminal audit, including builds, both entry/final matrices, debugging, requalification, accounting attempts and serial gates. Earlier interactive Git/authority preflight and later publication sealing are outside that explicitly bounded timing; no fabricated whole-session stopwatch or independently established speedup is claimed.

Failed development build/fixture attempts are retained. The first accounting attempt was stopped via its owned tool session after repeated immutable TRX hashing was identified; no test process was stopped. A metadata-keyed hash cache plus terminal SHA reread now avoids redundant accounting I/O. `receipt-hash-cache.json` is explicit hashing provenance, not an unexecuted test pass. The 52-module Python dependency/side-effect audit preserves historical scanners and complete membership; potential writers stay serialized. No alternating-full-matrix policy is activated.

## 8. Gates, preservation and publication

Strict/profile exits are **1/1** at entry and exit. Strict reports the inherited stale UibyID/NormalPlot review dependency; profile reports `INTEGRITY_FAILURE` with an empty error string. These are real nonzero validator failures, with receipts, not successful gate qualification. Strict MkDocs passes at entry and final. `Gate C2=UNMET` and all other prior native/scientific gates remain in force.

Live Ui20 authority contains **103 protected controls / 54 linked OPEN obligations**. Original overall populations remain **3,866 source members / 2,056 controls / 1,514 obligations**; native crosswalk and 04A.27's 44 importer obligations overlap and are not added as disjoint populations. Source-backed crosswalk rows/hashes preserve every restriction, zero releases and zero discharges. C09/C11/C12 unapplied patches, C13 and C17-C24 candidates are unchanged, including C21-STACKED-ROWCOUNT-VALIDATION-01 and C24-HISTORICAL-ORACLE-SCOPE-01.

All 259,051 historical result members retain size, creation/write timestamps and file identity; all 1,714 actual entry authority/corpus byte hashes are rechecked. Prior 04A.24 frozen hash comparison and the original 100-capture registry are separately reconciled. No history normalization, cleanup, relocation or overwrite occurs. Core/Designer/UDF/Ribbon/framework/package/signing/installer boundaries and the owner plan are unchanged. Final source pins match both compiled matrices; only evidence/accounting/dossier files changed afterward.

`publication-manifest.json` lists every intended candidate with exact SHA/bytes/mode; only its own self-hash is omitted. The final independent scanner covers paths, decoded JSON/base64 and private identifiers/credentials. Trackability, empty index and git diff checks are repeated after dossier generation. No `.gitignore` change or force-publication is used. This dossier is the single owner-acceptance document; all raw logs/TRX/build outputs remain ignored and private.

## 9. Documentation and native scope

Same-batch Help clarifies retained input versus dropped alignment rows, physical row/display/match-set identity, zero effective weights, optional output/source table/chart warning behavior and Windows-only availability. Authored methods.yml and all five screenshots/two workbook assets remain entry bytes. Strict MkDocs succeeds. No source screenshot or workbook was opened, recaptured or changed.

Native Excel/COM/RefEdit/UIA, browser/Office/Mac/WASM manual runs, R, installers, deployment and signing are **NOT_RUN / UNAUTHORIZED**. Existing Office/WASM managed/Node tests passed; they do not establish a functional PSM Office/Mac GUI or native Love chart acceptance.

Suggested separately authorized Windows smoke checks: (1) NN ATT with the existing public PSM workbook, inspect physical source rows/match-set identities and native Love series/labels/threshold, then final Results activation; (2) weighting or CEM with diagnostic/match outputs toggled, inspect method-specific empty-message/pair/stratum tables and repeated-run sheet order. Both remain behind the original native gates; no live run occurred here.

## 10. Scoped owner decision

Accept **04A.25 software extraction only**, retaining all statistical/native restrictions, independent match/trimming/SMD validation risks and every unapplied candidate. Git stage/commit/merge/push and official 04A.26 are separate owner actions. Nothing in this delivery discharges a native obligation or changes scientific policy.
