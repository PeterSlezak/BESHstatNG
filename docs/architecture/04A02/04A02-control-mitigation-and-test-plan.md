# Deferred-control mitigation and test plan ? proposed, not released

**No original obligation is discharged or re-rated here.** All 239 exact obligations map reversibly to 11 overlapping dependency/root-cause groups in `04A02-control-root-cause-matrix-v1.json`. These are evidence-gap classifications, not 239 proven bugs, 218 independent root causes or 218 required GUI runs. Whole-form blocker union remains 218; UibyID is shared between ANOVA/normality.

The accepted ledger contains 23 P0, 195 P1 and 21 P2 records in this subset; shared RefEdit UNKNOWN_HIGH and Ui0 progress P0 are additional retained dependencies. The C05 G4 manifest, C04 source plan, original ledger and source CSV row digests remain unchanged. The original source-record hash is a canonical CSV row-dict digest, not a whole-file hash. All 239 row identities/hashes were already verified in C05 and are rechecked by retained focused tests; missing local-only input means UNVERIFIABLE, never reconstructed.

| Group | Behavior / evidence gap and concrete tests | Actual owner batch / code need |
|---|---|---|
| SRC ? Per-ID definition/default/parent/provenance gaps | Source inventory/hash/row proof; review passive controls without inventing event handlers | 04A.45-47; No change established; source contract review can settle specific fields |
| MODE ? Constructor/tag/event enable-visible and mode transitions | Tag-specific defaults/parent ownership and event-order tests; native initialized-state evidence | 04A.45-47; Potential transition policy change only after owner review |
| OPTIONS ? UI options/statistical dispatch/default divergence | Explicit typed options and error branches; scientific policy and GUI propagation evidence | 04A.05-07/17; Normality boundary/text decision at 04A.06; otherwise policy/code need not established |
| REFEDIT ? COM connector/event lifetime and disconnected RCWs | Injectable fake-host subscribe/rebind/detach; executed live lifetime regression only after owner release | 04A.27,04A.45-47; Potential host ownership/lifetime fix after root cause proof; UNKNOWN_HIGH stays |
| IDENTITY ? Workbook/sheet/range identity and stale selections | Real Common lease tests + host adapter fake callbacks + later native identity capture | 04A.27/28; No portable fix assumed; host observation/lifecycle completeness requires review |
| OUTPUT ? Destination/occupied cells/partial-write/error paths | Sentinel, consent/cancel, layout/bounds and no unexpected-write evidence | 04A.28; No behavior change authorized; side-effect pipeline qualification pending |
| EXECUTION ? Reentry/cancellation/progress/STA/disposal boundaries | Common execution cancellation + Windows dispatcher/lifecycle adapter seams; live UI thread and late callback proof | 04A.46; Potential host callback/progress fix only in assigned batch; synchronous work may not pump cancellation |
| CHART ? Chart series/axes/geometry/host defaults | Source/model chart manifest and native points/series/configuration capture | 04A.29/30; Host adapter normalization or algorithm change requires separate decision |
| CULTURE ? Runtime alpha/p-format/locale/typed parsing | Invariant/sk-SK real semantic tests; initialized global prefs and rendered Excel text | 04A.28; No formatting change authorized; native rendering separate |
| OTHER_MODE ? Controls on other model tabs still constructed/traversed | Constructor reachability and parent graph; owner-reviewed applicability before any narrowing | 04A.17/45-47; May be legitimately inactive for case; no automatic waiver |
| HELP ? Passive help/label ownership and dynamic help wiring | WireHelp handler source + portable metadata tests; host disposal callback review | 04A.45-47; Potential source-only contract completion; not removal or safety release |

## Per-ID evidence and limits

Each exact row records definition/type, initialization, parent Add/AddRange source, explicit designer defaults, executable (not commented) Handles/AddHandler/RemoveHandler/WireHelp references, handler/member ranges, option/enable/visible/value transitions and candidate local call edges, shipping source SHA, owners/severity/earliest/dependent batches, affected cases, clusters and shared edges. Dynamic helper/reflection/COM delivery and inherited runtime defaults remain explicitly unresolved. Passive labels do not require button handlers; absence of a handler is not automatically a defect.

C05 G4 kept broader source reachability conservative. C06 distinguishes the actual code: PCA always accepts Worksheet and reads ws.Parent; its KMeans RefEdit binding is in the KMeans tag branch, not an unconditional PCA-path connector call. Other-tab controls can be constructed but detached. ANOVA traverses top-level Controls, not every field individually; disabling a parent can affect children. None of this grants applicability narrowing: review actual parent/event/tag reachability and exact mitigation/regression proof with the owner first.

Quick wins: complete per-ID source identity/default/parent/event fields and owned fixture links; verify passive display/helper wiring, explicit option maps and existing host-free policy boundaries. These can support owner review of specific contract fields without Excel. **They do not make an original P0/UNKNOWN_HIGH/live-capture obligation safe or owner released.** Native initialized state/event ordering/range identity/write effects and chart output still need real evidence.

## Focused executed tests

Eight new managed Common tests combine copied-input/invalid-output leases (worksheet removed/workbook closed), cancellation at acquisition/analysis/output and reversed completion-before-start. They exercise real Common policies with a mock write witness remaining zero; no form, RefEdit or COM is instantiated. Existing WindowsDocumentLifecycle/WindowsExecutionDispatcher injection seams are re-run separately in an isolated Legacy assembly; no Observe(app,book,sheet) overload or form constructor is called. Their evidence is not actual RefEdit teardown qualification.

39 Python source/runtime/pilot guard tests verify all IDs and source snapshots, event/default mutations, missing/unwired compute inventory, commented Handles exclusion, passive-label distinction, bulk traversal, explicit option propagation, n=5000 source disagreement, PCA scales 0/.5/1, 2x2 OR/error routing, R version/bitness/locale/stderr/probe-only gates, actual PE/component hashes and GUI proposal gating. PowerShell AST parsing checks proposal syntax only. Prior 17 source, 79 capture/packet, 50 oracle-mock and isolation/audit-compatibility tests remain unchanged and rerun. Full tests/TRX/actual counts are in the checkpoint report, not invented here.

Commands: `<WinPython> -B BESHStat.Excel.Common.Tests/src/test_investigation06.py`; isolated Common filter `FullyQualifiedName~MigrationControlBoundary06Tests|FullyQualifiedName~MigrationSemantic04A02Tests|FullyQualifiedName~MigrationFixture04A02Tests`; isolated Legacy filter `FullyQualifiedName~ExecutionAdapterTests|FullyQualifiedName~DocumentLifecycleAdapterTests`. All output needs a new preflighted sandbox; never invoke old writers at sealed locations.

## High-risk analysis and next proof

**RefEdit** (`Excel2007RefEdit.vb:xlSheetChangeHandle/ExcelConnector/txtAddress_Enter/txtAddress_Leave/Disposed`): current source already guards disposed/not-capturing SheetActivate and rebinds from target without dereferencing old worksheet properties. Enter still reads cached workbook/sheet properties inside guarded recovery; RemoveHandler can fail on disconnected RCW and repeated event/teardown sequences need evidence. xlApp WithEvents is assigned but WorkbookActivate handler is commented. Disposed removes SelectionChange; it does not explicitly clear all cached application/book/connector references; commented _NAR calls do not prove ownership release. Do not add blanket ReleaseComObject: borrowed RCW ownership must be reviewed. Fake-host subscribe/rebind/detach/dispose/late-event proof first; separate native focus/replace/close/delete simulation only after approved risk plan. 0x800401A8 was not reproduced. Owners stay 04A.27 and 04A.45?47.

**Shared progress**: Ui0OneRefeditMulticol.progressBarExactCalc has designer declaration/init/parent placement but no direct codebehind reference found. This is not proof dead or safe: framework traversal, owner intent and other mode lifetime still need review. It is not one of the three G4 form controls, and P0 / 04A.46 remain independent. No removal is authorized. Cox residual-plot removal remains 04A.47 only.

**ANOVA**: execution dispatcher, pinned addresses, lifecycle/STA and dynamic cancel/label/panel interact with bulk disable/restore. Cancellation may only be observed at supported synchronous boundaries; unit success does not prove UI responsiveness or late callback safety. Existing source settings/default divergence is a reviewed-path issue, not license to alter defaults.

**Normality**: n<5000 guard versus n>5000 NA text is a confirmed source disagreement requiring domain choice (guard or text) in 04A.06. No n=5000 oracle/native scenario ran.

**PCA**: actual dispatch calls 0/.5/1 biplots plus guarded 2D/3D/scree charts. Geometry/axis/display defaults are host-dependent; sign and tied-eigenspace equivalence need scientific policy. Keep 04A.17/29 owners and do not change numerical/chart behavior here.

**2x2**: constructor binds both RefEdits even for screen-count path; screen input therefore does not bypass the shared edge. ckChi2 OR ckAssociation emits both Pearson and association tables; exact-sample limit/error and no-selection paths can create modal prompts. Screen/worksheet orientation, OR/RR/CI text and cancellation need separately released tests. No chart exists in compute handler.

## Coverage and cost (categories overlap)

| Case | Blocked / deferred | Source-field candidates | Actually discharged | Code-change review candidates | Needs native evidence / scientific interpretation |
|---|---:|---:|---:|---:|---:|
| 04A02-ANOVA-NATIVE-01 | 141 / 160 | 160 | 0 | 138 | 160 / 99 |
| 04A02-NORMALITY-NATIVE-01 | 141 / 160 | 160 | 0 | 138 | 160 / 99 |
| 04A02-2X2-NATIVE-01 | 2 / 3 | 3 | 0 | 2 | 3 / 0 |
| 04A02-PCA-NATIVE-01 | 75 / 76 | 76 | 0 | 3 | 76 / 72 |

Source-field candidates mean definition/init/provenance/default/event fields can be reviewed, not full obligations resolved. No per-ID product-change necessity has been established merely by count; code-review candidates require a policy/defect decision. The separate normality source disagreement is method-level, not 160 distinct control bugs. Every original record remains restricted without owner release.

Minimal normal capture set is four representative case runs, with state-read and separate output stage gates; shared lifecycle proof may be reused only after review. Prioritize 2x2 (fewest local deferred records/no chart), then shared UibyID tag paths, then PCA. This is cost prioritization, not a declaration of safety. Add distinct cancellation/reentry/stale-range/modal/close/delete negative scenarios only as separately scoped mitigations; do not induce destructive worksheet deletion in a first pilot.

Path A is preferred: source contract review + focused host-free tests, exact shared-edge/host mitigation proof, then narrowly approved pilots and complete Windows captures. Path B must be a chosen, narrow case/criterion/root-dependent amendment with exact IDs/owner/deadline-before-earliest-consumer/resumption tests. Do not defer source fields already evidenced or automatically defer all four families. No Path B approval is present.
