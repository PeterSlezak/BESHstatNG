# 04A.03 control-mitigation addendum — owner review

**Official 04A.03 acceptance stands.** The owner's 2 November 2026 instruction
accepts the behavior-preserving extraction and disposes of its four historical
source-provenance conflicts without erasing them. This is a separate optional
post-acceptance mitigation addendum. It neither reopens that implementation nor
starts 04A.04. **No controls are released and no complete obligations are discharged.**

## Baseline, authority and separation

Live preflight: branch `codex/04A-03`; HEAD and local `main` both
`9e2326d4b562cc94660745f1453b231df938bff0`. The accepted extraction is still
uncommitted. Its four tracked modifications, seven visible new files and ignored
pre-change crosswalk are preserved. There was no branch switch/reset/stash/clean,
fetch, staging, commit, merge or push. Master Plan Revision 2.3 and 04A.03–07 rows
were read; no plan or sealed record was amended.

The current available 04A.02 owner receipt is
`docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json`, SHA-256
`218cd34c910e20d7e0b99f28b42afae38d69706bc0f90c7d0b4d3cd4fa3eaa19`.
It records owner-approved amended acceptance and explicitly disclaims a
cryptographic/independent signature. No newer signed local receipt or separate
04A.03 decision file was found. The current owner instruction supplies the later
04A.03 acceptance authority; no signature is invented. The old dossier's PARTIAL
text remains historical, not the current owner decision.

The immutable technical amendment remains SHA-256
`8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307`;
the original obligation ledger remains
`363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10`.
The untouched 04A.03 pre-change crosswalk hash is
`86ee4c2fc2ee17ef6a2f445f496db79b707ed24759e8ba8bcea0f8789fe2c824`.
The accepted/current `UibyID.vb` hash remains
`123d181ba1e91b10509d994ac916d3ef0bb298a66f568e4188d4a201dc0671c0`.

New evidence root: `TestResults/04A-03-controls-20261102-run01/`.
`accepted-baseline.json` pins 20 inherited product/test/authority files; exact
copies and the pre-existing tracked patch are retained there. The addendum adds
only seven new files listed below. No accepted product, test project, existing
test, RefEdit, designer, parser, report builder, numerical method or help route
was changed. Local review patches distinguish this addendum from the inherited
uncommitted extraction.

## Exact populations and target status

Counts were recomputed using set unions/intersections from the accepted case
transfers and original ledger, not added across shared cases:

| Population | Before | After | Released/discharged here |
|---|---:|---:|---:|
| ANOVA/normality shared blocked controls | 141 | 141 | 0 |
| ANOVA/normality shared deferred obligations | 160 | 160 | 0 |
| Four-case distinct blocked controls | 218 | 218 | 0 |
| Four-case distinct transferred obligations | 239 | 239 | 0 |
| Broader restricted live controls | 802 | 802 | 0 |
| Wider original obligations | 1,514 | 1,514 | 0 |
| This exact cohort: blocked controls / obligations | 20 / 21 | 20 / 21 | 0 / 0 |

Every row below belongs to **both** `04A02-ANOVA-NATIVE-01` and
`04A02-NORMALITY-NATIVE-01`. Primary integration owners remain 04A.07 and 04A.06;
earliest consumer remains 04A.03. The table uses suffixes of the exact canonical
prefix `control:BESHStatNG.UibyID.`. Full spelling, original CSV/decision row
hashes, ledger/source hashes, owner roles, clusters, prior source fields, original
transfer/OPEN status, preconditions, missing evidence and test identities are in
[the exact-ID successor matrix](04A03/control-mitigation/successor-candidates.json).

| Exact ID suffix | Risk | Host-free evidence added | Full original obligation |
|---|---|---|---|
| `TabControl1` | P1 | Mode tabs, parent/order, unsupported tags | OPEN |
| `TabPage1` | P1 | Input tab retained in all three modes | OPEN |
| `TabPage_Options` | P1 | Attached in three modes; detached for unsupported tags | OPEN |
| `grpInput` | P1 | Initialized parent graph | OPEN |
| `grpANOVA1MCP` | P1 | ANOVA local availability; hidden retained state elsewhere | OPEN |
| `grpHomogeneityVariances` | P1 | ANOVA/KW local availability; normality hidden | OPEN |
| `lblAlpha` | P1 | Local hidden state in all three target modes | OPEN |
| `lblRefedit1` | P1 | Parent/text, orientation enabled state and reentry | OPEN |
| `lblRefedit2` | P1 | Parent/text and orientation handler stability | OPEN |
| `ckDescriptiveStatistics` | P1 | Checked default, mode availability, all four shared-extra toggle states | OPEN |
| `ckBoxPlot` | P1 | Checked default, mode availability, all four shared-extra toggle states | OPEN |
| `ckWelch` | P1 | Default, applicability, real typed ANOVA request/result | OPEN |
| `ckLevene` | P1 | Default and actual variance-helper branch/metadata | OPEN |
| `ckBartlett` | P1 | Default and actual variance-helper branch/metadata | OPEN |
| `ckFlignerKilleen` | P1 | Default and actual variance-helper branch/metadata | OPEN |
| `ckSquaredRanks` | P1 | Default and actual variance-helper branch/metadata | OPEN |
| `ckBonferroni` | P1 | Default, applicability, real typed ANOVA request/result | OPEN |
| `ckLSD` | P1 | Default, applicability, real typed ANOVA request/result | OPEN |
| `ckTukey` | P1 | Default, applicability, real typed ANOVA request/result | OPEN |
| `ckGamesHowell` | P1 | Default, applicability, real typed ANOVA request/result | OPEN |
| `btnHelp` | P2 | Resolver and subscription inventory; no browser invocation | OPEN; **not a blocked control** |

All 21 original root-cause rows require native evidence. None meets the frozen
complete closure requirements, so **zero full IDs are READY_FOR_EXPLICIT_OWNER_DISPOSITION
for release**. The partial source/managed fields for these exact IDs are ready for
owner disposition as *partial qualification only*. No `OWNER_RELEASED` record or
successor owner signature exists. A future case-specific observation cannot
automatically release the other case, other modes or downstream controls.

There are **42 newly checked source-field assertions**: accepted-source-chain
reconciliation plus a mode/consumer-or-help map for each target. They rebind and
extend prior C07 source fields; they are not 42 new original obligations or native
qualifications. Original declaration/default/parent text and all recorded runtime
source uses were checked against the accepted source. All 21 original CSV row
digests match their frozen ledger records; decision-row digests are separately
recorded. No source row was fabricated or historical hash rewritten.

## What actually executed

`UibyID.New` calls `AppGlobals.app` three times and binds RefEdit connectors. A
shipping constructor invocation would cross the forbidden Excel/native boundary.
It was **not run**. No uninitialized WinForms object, hidden global mutation, COM
substitute or live Excel instance was used.

The new test-only project compiles a hash-bound source projection into an isolated
assembly. It uses the **actual designer source**, original constructor mode/tab
statements, exact `optByColumn_Click` / `optByID_Click` bodies, exact ANOVA option
initializer, and exact `ComputeVarianceHomogeneity` body. Normal initialized managed
controls are constructed on an STA thread and never shown. Designer resources
are read from the real product assembly. The projection has exactly disclosed
differences:

- Class/resource identity is adapted for the separate test assembly.
- Three host connector assignments and the initial RefEdit focus selection are omitted.
- Three alpha preference reads are replaced with an explicit test input, 0.05;
  preferences are not qualified.
- Five unrelated mode helpers are fail-closed `NotSupportedException` stubs.

This qualifies **source-projected real managed controls**, not the shipping full
constructor, effective desktop visibility, native UIA, COM subscriptions, workbook
identity or chart/output effects. Local visibility is read from .NET Framework's
managed control state because the test Form is never shown. Original option
handlers run through managed `OnClick`/`Handles` routing, not native mouse/UIA
clicks. Their RefEdit connectors remain null. Help is never clicked.

The 61 projected MSTest identities cover:

- Three actual enum modes: `OneWayANOVA`, `KruskalWallisTest`, `NormalityTests`.
  Defaults/parent graph, three orientation/reentry routes, unsupported -1/Int32.MaxValue
  tags, null title/tag, Dispose twice and fresh-instance state are characterized.
- 32 ANOVA toggle combinations, each in invariant and sk-SK cultures: **64 typed
  request pathways**. The original initializer feeds real
  `SharedAnalysisApplicationService.Run`; Welch and four post-hoc result/plot
  metadata, primary p-value metadata, group order/counts and no-optional-analysis
  behavior are checked. No second statistical algorithm or numerical golden is added.
- All 16 variance toggle combinations execute the original pure helper and Core
  assumptions methods. Result ordering is Fligner–Killeen, Levene, Squared Ranks,
  Bartlett, vertically stacked; dimensions and every p-value cell coordinate are
  checked. The no-option helper's existing 1x1 empty-table normalization is
  characterized; source tests show callers guard it before adding output.
- Twelve descriptive/box checkbox-state combinations across three modes. Their
  actual report/chart branches are **source-mapped, not executed**. Normality
  does not exercise hidden ANOVA/variance controls; KW does not consume ANOVA
  post-hoc checkboxes.
- Actual `HelpContext.ResolveTopic` and actual `HelpWiring` subscription inventory
  on initialized test controls, including null/unsupported state. The source map
  verifies shipping integer Tag/title fallback separately from enum/string tests.

The 22 Python tests reject shared double counting, help inflation, disappearing
IDs, changed case coverage, invented row hashes, unauthorized OPEN→release or
population reduction, gate promotion and host-free/native mislabeling. They bind
the option/source branch ordering, retained code hash and source manifest. Offline
identity adversaries reject reviewed/live HWND or foreground drift, Boolean PID
aliases and missing loaded-build attribution. Even a complete synthetic context
always returns NO GO; there is **no native executor** in this addendum.

## Findings and remaining risks

No product defect requiring a behavior-changing patch was established. Managed
help subscription inventory proves that calling `WireHelp` twice on the *same*
control adds two delegates. The shipping constructor contains one call, so this
is a reentry/idempotence risk candidate, not evidence of a duplicated shipping
browser launch. No handler is removed or production subscription behavior changed.

Native descendants remain dependent on `Excel2007RefEdit.xlApp UNKNOWN_HIGH` even
when their eventual state getters do not themselves acquire ranges: the full
constructor binds that edge first. The nine container/label OTHER_MODE obligations
are not resolved by three-mode projection. `lblAlpha` is hidden in all three
target modes; a missing visible provider must not be fabricated into a pass.
Help browser delivery remains untested, and original help owner 04A.44 is retained.

Remaining work for every complete target: independent Windows/COM review, exact
owned-process/native initialization and provider evidence, applicable other-mode
and output/scientific interpretation review, and a real exact-ID/case owner
successor decision. Owners remain 04A.06/07 plus 04A.27 and 04A.45–47 for shared
RefEdit/GUI. Estimated shared effort is 45–90 minutes independent review,
30–60 minutes separately authorized calibration and 5–10 minutes per mode
observation; these are planning estimates, not permission or evidence.

## Minimal native packet — reviewable, deliberately not executable

[Read-only native packet and unsigned independent risk form](04A03/control-mitigation/native-readonly-plan.json)
pins the actual sources and exact two-case cohort. C14's retained packet was for
2x2; its shared-edge findings are reused, **not its case authority**. The new
packet is initialized-state observation only: no option/orientation toggles,
RefEdit focus, compute, output, charts or help/browser click. All measured context,
independent reviewer/signature and exact owner N1 fields remain null.

- **N14-HWND remains blocking in the frozen native helper.** The offline predicate
  tests reviewed/live equality, but does not repair or run that helper. The packet
  specifies the exact unapplied check immediately after binding MainWindowHandle,
  before NativeOM/UIA access, plus PID/foreground checks at every boundary.
- **N14-SOURCE remains blocking.** Supplied source hashes and on-disk XLL existence
  do not prove loaded product identity. Require held file identities, loaded
  module path/hash, assembly version/MVID and independently attributable source/build
  manifest measured in the exact owned process.
- **N14-OBSERVATION remains blocking.** Provider/property getters can execute code;
  saved workbook equality does not prove absence of transient effects. Exact
  provider/AutomationId/getter sets, timing/deadline and read-only constraints need
  independent risk acceptance and actual calibration.

After separately authorized review/calibration, the plan would bind exclusive
owner-created PID/start/session/SID/HWND, require live HWND equality with the
reviewed HWND, verify foreground and loaded Excel/XLL/source linkage, and observe
one mode at a time against saved disposable workbook/state. Unknown providers,
dialog/deadline, identity drift or unexpected workbook activity abort with retained
evidence. No ROT/global application lookup, unowned process kill/Quit, coordinate
fallback, borrowed RCW release, save/delete or N2 operation is permitted. The owner
closes only the recorded disposable instance under a reviewed procedure.

**Native attempts = 0. N1 = NO GO; N2 = NO; P = NO; Gate C2 = UNMET.**

## Validation, preservation and review scope

Full final Debug/Release managed/Python/Node verification uses the unchanged
`scripts/verify-04a03.py` isolation wrapper around the documented verifier. It
preserves the original script, records its exact execution-copy diff/hash, uses
VS 2026 and the exact WinPython executable below, disables restore and isolates
assembly/data and test-internal writers. The additional projection project builds
only into fresh per-run folders. TRX location alone is not relied upon.

Focused troubleshooting is retained: the first build had test-code type/array
syntax errors; two zero-discovery attempts are **not passes**. After adapter
binding, reflection was corrected for VB WithEvents properties. New fixture
expectations were corrected to the existing ResultTable 1x1 empty normalization
and the legacy HorizontalStackArrays name's actual row-stacking semantics. No
old test, product behavior or statistical expectation was weakened. A new runner
guard now requires all 61 identities to execute. The last focused execution is
61/61; standalone source/gate checks are 22/22.

The ledger generator also refuses qualification without an actual 61/61 TRX
receipt, independently of process exit code. Its final receipt-guard repair was
verified by repeating only the 22 affected source/gate cases in each configuration;
those repetitions are not extra distinct tests.

Final verification commands, all from the repository (no network/install/restore):

```powershell
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py init --evidence TestResults/04A-03-controls-20261102-run01
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/04a03_control_mitigation.py freeze --evidence TestResults/04A-03-controls-20261102-run01
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/04a03_control_mitigation.py prepare --evidence TestResults/04A-03-controls-20261102-run01
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/04a03_control_mitigation.py matrix --evidence TestResults/04A-03-controls-20261102-run01
# Focused managed commands used distinct names focused01 through focused06.
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/tests/test_04a03_control_mitigation.py

& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py final --evidence TestResults/04A-03-controls-20261102-run01 --configuration Debug --name final-debug
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/04a03_control_mitigation.py managed --evidence TestResults/04A-03-controls-20261102-run01 --configuration Debug --name final-debug
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py final --evidence TestResults/04A-03-controls-20261102-run01 --configuration Release --name final-release
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/04a03_control_mitigation.py managed --evidence TestResults/04A-03-controls-20261102-run01 --configuration Release --name final-release

& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B TestResults/04A-03-controls-20261102-run01/finalize.py finish
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py preserve --evidence TestResults/04A-03-controls-20261102-run01 --name final
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --name-status
```

The matrix was initially assembled during development and finalized only after
the discovered-test failures were resolved; its recorded 61/61 receipt is
`focused06`. The later final receipts below reproduce it. Existing new addendum
JSON drafts were corrected in place before review; **no inherited or historical
artifact was changed**. Running generation commands against these already-existing
destinations now fails exclusive creation; use a fresh root for a later run.

Each full final wrapper calls a diff/hash-recorded copy of
`verify-beshstatng.ps1` with `-Legacy -Wasm -Js -Python -VisualStudio 2026
-PythonExe C:\Users\peter\Desktop\WPy64-31470\python\python.exe` and the relevant
configuration. The original verifier was not run unmodified because shared
writers would violate retention. Its SHA remains
`e54e1fb0e7f3699f5d9aee361d255801531a546d8169edbab704ac0d8dd017fd`.
Windows managed build uses `PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory`
and legacy test Build with `BuildProjectReferences=false`, avoiding source
generators and XLL packaging/signing. The new project uses the existing MSTest
2.2.10 adapter explicitly and asserts discovery/execution count 61.

| Final suite, distinct cases per configuration | Debug | Release |
|---|---:|---:|
| Core | 1,024 PASS | 1,024 PASS |
| Common | 852 PASS | 852 PASS |
| Windows legacy | 838 PASS | 838 PASS |
| Office add-in managed | 505 PASS | 505 PASS |
| WASM execution managed | 45 PASS | 45 PASS |
| New projected control/option cases | 61 PASS | 61 PASS |
| **Managed total** | **3,325 PASS; 0 fail/skip** | **3,325 PASS; 0 fail/skip** |
| Node (Office 468 + help 71 + WASM 377) | 916 PASS | 916 PASS |
| Python (tools 27 + help 16 + scripts 373 + Common 168) | 569/584 PASS | 569/584 PASS |

All 61 new managed and 22 new Python cases pass. There are no new final failures.
The full verification exits **1**, not green, because the same 15 historical
Python nonpasses remain: three original RefEdit/source compatibility checks,
eight accepted-baseline LF/CRLF receipt failures, and four owner-dispositioned
04A.03 provenance conflicts. `final-summary.json` compares the exact exception
identity/outcome dictionary to the retained 04A.03 summary; they are identical in
both configurations. The four provenance errors are not erased or relabeled
new regressions. Original strict/profile **1/1 with 24 historical failures** is
retained context, not rerun or represented as green. No expensive historical
audit writer, R reference execution, numerical rebaseline or golden promotion ran.

The 15 semantic-capture cases per configuration and final 22-case guard rechecks
are repetitions of counted cases. Earlier failed/zero-discovery attempts remain
visible and are excluded from the final passing totals. Debug and Release execute
the same distinct identities separately; totals must not be called twice as many
unique cases.

Exact TRX locations relative to the new evidence root:

- `Debug/results/{Core,OfficeAddin,OfficeExecution}.trx`
- `executions/final-debug-{Common,Legacy}/results/{Common,Legacy}.trx`
- `projection-executions/final-debug/results/ControlMitigation.trx`
- `Release/results/{Core,OfficeAddin,OfficeExecution}.trx`
- `executions/final-release-{Common,Legacy}/results/{Common,Legacy}.trx`
- `projection-executions/final-release/results/ControlMitigation.trx`

`final-summary.json` contains per-suite counters/hashes and the exact retained
Python exception identities. Full command arrays, exits, elapsed times and logs
are under `*-command.json`, `final-{debug,release}-full.log`,
`final-{debug,release}-common-python.log`, projection build/test logs and
`executions/*/execution.json`. Projection source/omission/hash attribution is in
`projection/source-projection.json`. These are local-only, not native captures.

Preservation: **PASS, all 146,682 historical TestResults files retained**, zero
membership/size/timestamp/file-identity drift. Frozen source/authority/verifier and
VersionAttributes pins pass; all 20 accepted baseline pins remain unchanged.
The final C28 CSV hash is
`050b78c5b35757c0e7c35e7f0ecac9222e7102a41271d558d1a7efae6e4a1193` and the root-cause
matrix hash remains
`e7ffa327ecb0d6d6143bfbbc8c3b11e00f4f7876a91e33e2bbdfe87b530ebbf5`.
This is a membership/metadata plus selected frozen-hash check, not a claimed full
re-hash of every historical object. No historical file was overwritten, removed,
renamed, compressed or rebaselined. Free space was 542,783,492,096 bytes at initial
preflight, 542,732,963,840 at protection snapshot, and **542,445,076,480 bytes
(about 505 GiB)** at final preservation, above the 100/50 GB thresholds.

`git diff --check` passes and the index is empty. HEAD, main and cached origin/main
remain `9e2326d4b562cc94660745f1453b231df938bff0`. Git status shows the inherited
04A.03 modifications unchanged plus the five visible new addendum files below;
the other two new JSON files are explicitly ignored. The new local
`addendum-only.patch` / `addendum-paths-and-hashes.json` inventories all seven
additions separately from `accepted-tracked-diff.patch` and the baseline copies.

Manual checks remain **OWNER MANUAL / NOT RUN**: after exact N1 release, observe
the three initialized Windows mode/container/option states under the bound packet
without RefEdit/compute/output actions; separately, the unchanged existing WASM
typed ANOVA/KW browser smoke can check group order and primary metadata. Neither
managed/Node results nor this checklist claims an actual Windows/Office/WASM
desktop or browser pass.

New addendum-only paths (the inherited accepted files are unchanged):

1. `BESHStatNG.Test/ControlMitigation/ControlMitigation.Tests.vbproj`
2. `BESHStatNG.Test/ControlMitigation/ControlMitigationProjectionTests.vb`
3. `scripts/04a03_control_mitigation.py`
4. `scripts/tests/test_04a03_control_mitigation.py`
5. `docs/architecture/04A03/control-mitigation/successor-candidates.json`
6. `docs/architecture/04A03/control-mitigation/native-readonly-plan.json`
7. `docs/architecture/batch-04A-03-control-mitigation-owner-review.md`

The two new JSON documents remain ignored by the existing architecture allowlist;
later admission requires an explicit staging decision. All new binaries, generated
projection sources/manifests, old failing attempts, baseline copies, patch, logs,
TRX and protection inventories are local-only under the fresh TestResults root.
Historical local-only 04A.01/02/03 evidence is still required; a clone alone is not
self-contained. No ignore rules, package/framework targets or production controls
were changed.

**Owner decision requested:** accept the 42 source-field and scoped host-free
qualifications for these exact 21 obligations as partial evidence, with all complete
obligations OPEN and zero releases. The next meaningful release action is to assign
an independent Windows/COM reviewer to this bound minimal packet and its three
blocking findings. A narrowly scoped owned-process observation becomes worthwhile
only after that review, helper/source-attribution correction, actual calibration
and exact owner N1 authority. No fresh open-ended investigation cycle or N release
is inferred from accepting this addendum. Official 04A.03 acceptance stands;
04A.04 was not started.
