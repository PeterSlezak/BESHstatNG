# Official 04A.02 — internal Continuation 09 owner handoff

**04A.02 remains NOT ACCEPTED. 04A.03 is NOT AUTHORIZED/STARTED.** This continuation implemented and tested executable successors, found and fixed two real admission/locking defects, and prepared a specific owner-context command and source-specific product patch. It launched **no R process**, performed **no independent statistical comparison**, applied **no product patch**, and performed **no Excel GUI/NativeOM/UIA provider operation**. The native helper's serializer self-tests are not a native pilot. P remains NOT AUTHORIZED.

The unchanged owner receipt SHA-256 is `8a4c8e0973e7b24f66795559ac1841d1144ba1edc27dc1fa685ff7fa274b4baf`; the unchanged C07 scientific-policy JSON is `7afdbec07a4331cda915d47e21b075b55506cf53d560b95f3f17b4dc67a27574`. Both were verified before work. Exact S/R and conditional GUI authority persist. No generic R or GUI permission is requested again. Root AGENTS, Master Plan 2.3 / official 04A.02 row / Gate C2, later 04A.01 owner acceptance and retention, C08 dossier, manifest/source pins and the relevant live Windows handlers were reviewed.

Branch `codex/04A-02`, HEAD/main `12e25924aae93f0b36ab33937176c86c19641b50` remain unchanged. All **101 original worktree paths** and **2,273 starting source pins** are byte-identical at closeout. No reset, branch change, rebase, cleanup, staging, commit, merge, push, network/package install, installer, signing or deployment occurred. Core/Common algorithms, APIs, framework targets and Windows public entry points/packaging sources were not changed.

The current [G5 dossier](04A02/04A02-C09-G5-acceptance-dossier.json), [matrix v6](04A02/04A02-acceptance-coverage-matrix-v6.json), [R context/threat evidence](04A02/04A02-C09-R-context-and-threat-review.json) and [final native engineering review v2](04A02/04A02-C09-native-risk-review-v2.json) retain all 11 cases, six G0–G5 criteria, 218 blocked controls, 239 obligations and eleven risk groups. Older C08-prefixed fields remain historical; the final C09 native review v2 supplies the final helper hashes. No original obligation is discharged, independent risk review supplied, blanket waiver selected or roadmap amendment approved.

| Case / gate | Actually executed | First remaining blocker |
|---|---|---|
| C30-SYN-ANOVA / G2 | Source/runtime file pins and host-free admission/comparator tests | Current SID is sandbox, not owner; protected profile ancestor cannot be qualified/held; chosen owner root is absent |
| C30-SYN-KW / G2 | Same qualification class; separate raw/corrected comparator tests | Same owner execution-context/root proof gap |
| 04A02-2X2-NATIVE-01 / N1 / G4 | Twelve proposed coordinator tests in each configuration; native helper compiled and serializer self-tested | Exact product patch not authorized/applied; independent shared/exact-ID risk review and actual owner-desktop adapter/context calibration absent |
| N2 and ANOVA/normality/PCA native cases | Frozen plans retained, including 1/9/11/3 Fisher/OR/RR proposal | Outside the current exact N1 execution authority; N2 needs reviewed actual N1 plus exact workbook/destination/action GO |
| G5 | Full/focused validation and preservation, review forms and publication manifest | Real reference/native evidence, two distinct human comparison reviews, later P and explicit official acceptance |

## One owner-normal-context action for R

The measured execution identity is `BESH_NTB\CodexSandboxOffline`, SID `S-1-5-21-142429299-24905182-1790040237-1012`. Repository ownership and local account translation identify Peter as `S-1-5-21-142429299-24905182-1790040237-1002`; this does not turn the current token into Peter's interactive token. No authenticated owner-context execution facility is available in this session. No elevation, impersonation, credential use, ownership or permission change was attempted.

The v4 collector actually ran with the current token and measured Windows AccessCheck results for accessible ancestors. `C:\Users\peter` remains unreadable to Get-Acl; the chosen literal reference root does not exist. Complete ancestor handle acquisition also fails closed under this identity. These observations do **not** establish that Peter cannot qualify his own fresh LocalAppData directory. The owner wrapper was tested here and refused SID 1012 **before allocating anything**.

From normal, **non-elevated owner VS 2026 Developer PowerShell**, with the repository as working directory, run this once:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\04a02\owner_reference_c09.ps1 -Root 'C:\Users\peter\AppData\Local\BESHStatNG-04A02-C09-reference-20261101-7d4a' -AllocateEmptyRoot -SourceDeclaration 'Owner normal VS 2026 Developer PowerShell; C09 ACL collection only'
```

It verifies the actual owner SID/non-elevated token, refuses a pre-existing root or reparse allocation ancestor, allocates only that empty direct LocalAppData child with inherited permissions unchanged, then runs the **read-only** collector under a 30-second bound. It runs no R or Excel. Allocation is explicit; collection does not edit ACLs. Expected retained outputs in that exact root are `owner-acl-raw.json`, `owner-acl-stderr.txt`, and `owner-collection-receipt.json`. Return the printed identity, raw SHA-256 and output location/evidence. The receipt identifies the actual collecting token and supplied source declaration; it is not an invented independent ACL/statistical acceptance.

This is the current first proof gap. After the actual owner evidence passes review, the already-authorized startup/reference route uses [operator v4](../../scripts/04a02/oracle_operator_v4.py) and **[manifest-v4-qualified-v3.json](04A02/oracle-proposals/manifest-v4-qualified-v3.json)**. Earlier v4 proposal manifests and their failed qualification evidence are retained and superseded; the filename does not mean R_RUNTIME_PROBED or R_EXECUTED. The current technical receipts for [ANOVA](04A02/04A02-C09-R-technical-C30-SYN-ANOVA.json) and [KW](04A02/04A02-C09-R-technical-C30-SYN-KW.json) are both NO GO.

## Concrete R engineering changes

The [collector](../../scripts/04a02/inspect_output_acl_v4.ps1) and [Windows AccessCheck helper](../../scripts/04a02/windows_acl_v4.cs) record actual token/group identity, complete ancestor owner/SDDL, inheritance, canonical DACL/reparse state, and current-token effective file/directory creation, DELETE_CHILD, DELETE, READ_CONTROL, WRITE_DAC and WRITE_OWNER. Other identities are conservative raw-ACE candidates, not fabricated effective-access results.

The revised threat model trusts the human owner, SYSTEM, Administrators and TrustedInstaller as authorized OS maintenance principals. It does not demand that `C:\` and `C:\Users` be owned by Peter. It distinguishes ordinary sibling creation from replacing an existing ancestor: unknown DELETE_CHILD/DELETE/WRITE_DAC/WRITE_OWNER is refused on ancestors; unknown creators/writers are also refused on the selected reference root. Inherit-only ACEs are not treated as current-object grants. An unknown Allow ACE is not silently cancelled by a guessed effective Deny result. The model permits ordinary legitimate owner directories while retaining fail-closed unknown-writer handling.

Two substantive defects were found during executed negative tests:

- Windows `abspath` normalized a trailing dot before component validation. V4 now validates literal components **before** normalization; aliases/ADS/device names stay refused.
- The inherited v2/v3 directory handle requested only FILE_READ_ATTRIBUTES. A real rename test showed that this did **not** block renaming the directory. V4 requests FILE_LIST_DIRECTORY plus FILE_READ_ATTRIBUTES, excludes delete sharing, uses OPEN_REPARSE_POINT, and checks identity around acquisition. The same rename expectation now passes with the new primitive. The complete chain is still refused under the current sandbox; an accessible temporary subtree test is not misrepresented as an owner-context full-chain pass.

V4 validates immutable owner/policy/source/runtime/fixture pins, preserves exclusive attempt creation/no-replace hard-link publication, holds full ancestor/data-access directory locks and input read locks, and validates pinned ACL-review/startup/test receipt **contents** instead of accepting nonempty qualification markers. It rejects stale hashes/ACL context, missing qualification and changed Rscript/R.dll. Frozen v2/v3 code remains unchanged. Host-free fixtures test positive legitimate ACLs and negative unknown-writer/reparse/incomplete-chain cases. Existing collision/concurrent publication/partial-output/stderr/timeout tests remain in the full suites.

The new nonstatistical startup script and `--startup-only` route check R/stats 4.5.1 x64, private HOME/R_USER/profile/environment/library/temp/locale and loaded package/search paths before statistical case GO. Actual R startup was **not run** because owner-root qualification failed first. Current executable/component/package hashes are checked in admission tests; prior C07 startup is not reused as a fresh startup pass. Loader assumptions are bounded to the frozen bundled base/stats dependencies and normal trusted Windows API-set/SxS/OS maintenance; arbitrary third-party DLL closure is not demanded or claimed.

The [independent comparator](../../scripts/04a02/compare_reference_v1.py) consumes retained operator publication/raw R output, applies the **unchanged** C07 per-field policies, checks exact candidate schema/table ordering/CLR/null/text/p metadata, maps ANOVA SS/MS/df cells and both service/table F/p values, keeps KW raw/corrected H/p separate, and reports missing/extra/reference-only fields and explicit delta/limit calculations. KW tie/rank/df auxiliaries are not invented as Common members; RowIds remain fixture provenance. Native Excel NumberFormat/display/geometry is explicitly uncaptured. Comparator unit tests use `MOCK-NOT-R` payloads; no actual independent comparison receipt exists.

Two [reviewer forms](04A02/04A02-C09-independent-reviewer-1.pending.json) ([second slot](04A02/04A02-C09-independent-reviewer-2.pending.json)) contain exact code/policy/provenance links and null human identities/verdicts. Populate them only after real raw references and comparisons exist. Codex has not supplied two human reviews, a P decision or approved numerical goldens.

## Exact RefEdit product decision and desktop action

The [unapplied five-path patch](04A02/04A02-C09-RefEdit-unapplied.patch), SHA-256 `e067eb1fc5ca70599150c349f37bf85b97ef57befc4797071a5ff67b84831b12`, passes `git apply --check`. It names:

- `BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb`
- new `BESHstatNG/src/UI/Refedit/RefEditEventLease.vb`
- new `BESHStatNG.Test/src/MigrationRefEditBoundary09Tests.vb`
- `BESHstatNG/BESHStatNG.vbproj` and `BESHStatNG.Test/BESHStatNG.Tests.vbproj` compile lists

**Approve applying this exact five-path C09 RefEdit patch?** This specific product authorization was requested asynchronously and no answer was received during this continuation. The source is the user's explicit restriction: **“The owner has NOT authorized applying the suggested production RefEdit seam patch”**. Conditional GUI permission does not authorize the product edit. No patch was applied, and no response/default choice was inferred.

The patch routes active SelectionChange add/remove sites through the actual proposed generation lease, invalidates before detach/rebind, blocks replacement after failed detach, makes disposal idempotent, ignores stale/unarmed/disposed callbacks and preserves borrowed RCWs. It guards the private selection callback, avoids reading an old sheet Name during disposal and proposes removing the event-area `_NAR(rng)` release. It also addresses the actual workbook WithEvents source and clears borrowed workbook/application references only after invalidation. The current xlApp field has no active Handles clause; the active workbook SheetActivate subscription is explicitly accounted for. The existing known `0x800401A8` hazard was not induced.

The actual proposed coordinator and twelve tests compile in a new isolated .NET Framework test assembly in Debug/Release. Tests cover synchronous callbacks during subscription, callback after disposal/rebind, failed attach/detach, repeated disposal, throwing callbacks and reentrant binds. Shipping private WinForms handler execution, WithEvents COM detachment and native STA ordering are **not** established by those tests or by the existing nine C07 lifecycle tests. Those require product integration authorization and further executed regression. No independently accepted N1-only noninvasive substitute was identified; isolation and avoiding deletion reduce exposure without proving those handlers safe.

The [real N1 helper source](../../scripts/04a02/native_n1_c09.cs) and [external supervisor](../../scripts/04a02/native_n1_supervisor_v1.py) now exist and compile, rather than supplying only dummy callbacks. They implement STA exact-process Excel7 NativeOM binding, PID/start/session/owner/HWND/window/Excel/XLL checks, held immutable executable/XLL inputs, exhaustive reviewed UIA context keys, ribbon invocation, window/option state reads, and workbook before/after identity/CodeName/UsedRange typed values/formulas/NumberFormat/names/dirty flag/saved-byte/addin observations. Individual provider reads are bracketed by context checks. Partial evidence is exclusive; the external 15-second deadline stops only its own helper. There is no ROT, coordinate/SendKeys fallback, global Quit, borrowed-RCW ReleaseComObject, save/restore or worksheet-write operation.

**The live adapter is NOT NATIVE-QUALIFIED.** Its actual owner-desktop PID/NativeOM binding, AutomationId/provider/window map, measurement behavior and deadline must be calibrated and independently reviewed. Current risk records have no accepted reviewer/source. There is no owned Excel PID/workbook/stage GO or native capture. The precise manual desktop action, after independent risk acceptance, is to prepare one saved disposable workbook in an isolated owned `/x` instance, bind its Excel/XLL/source/workbook/start/session/SID identities, and obtain the reviewed actual UIA map/context/measurement evidence for the N1-only helper. No Compute, counts/options changes, deletion or N2 is included. Readonly file inspection identified installed Excel `16.0.20326.20232` and four existing 32/64-bit XLL candidates; their hashes are retained as **candidates**, not an approved live configuration.

N1 success would still require review before N2. The [stage plans](04A02/04A02-C09-native-stage-plans.json) retain exact 1/9/11/3 Fisher/OR/RR with Chi2/association/Liddell off, alpha .05, and source-backed ANOVA/normality/PCA plans without executing or releasing them. Two future manual checks remain NOT RUN: actual initialized counts/global alpha and unchanged observable workbook state under N1; exact rendered 2x2 output under later explicitly bounded N2. No browser/WASM manual observation is claimed; automated WASM/Node coverage is below.

## Actual validation and preservation

| Suite | Debug | Release | Final failures / skips |
|---|---:|---:|---|
| Core / Common | 1,024 / 806 | 1,024 / 806 | 0 / 0 |
| Office / independent WasmSmoke managed | 505 / 45 | 505 / 45 | 0 / 0 |
| Legacy Windows tests | 790 | 790 | 0 / 0 |
| Existing C07 Windows seams / proposed C09 coordinator | 9 / 12 | 9 / 12 | 0 / 0 |
| Managed total | **3,191** | **3,191** | **0 / 0** |
| Python full / focused attempts | 203 / 255 | 205 / 255 | 0 / 0 |
| Python distinct full + focused | **373** | **373** | **0 / 0** |
| Node distinct | **916** | **916** | **0 / 0** |

Debug full discovery preceded two additional C09 negative tests; its focused pass includes all 34. Release full and both focused passes include all 34. Repetitions are not added to distinct totals. Node is Office 468 + Help 71 + independent WASM 377; “native” in a test name is not actual Excel execution. The separately compiled helper's serializer self-tests have their own receipts and are not counted as managed/native capture cases. R/startup/native cases NOT_RUN are live-work statuses, not falsely passing or skipped unit tests.

Qualification attempts are retained: attempt1 ran 31 tests with one failure and five errors (pre-normalization alias bug, denied full-chain assumption and four duplicate-import exception-class assertion errors). Attempt2 ran 32 tests with the genuine directory-rename failure. Attempt3 passed 32; full/focused final passes include the subsequent 34-test set. No expectation for the rename protection was weakened: the production-independent v4 handle primitive was corrected. Prior operator source snapshots and three proposal manifests remain retained. Native helper compile/self-test iterations and their earlier source snapshots are also retained. No failed statistical/native attempt was hidden: neither live class was attempted.

Evidence root `$E = TestResults/04A-02-initial-fixtures-c09-20261101T143728Z`; `$P = C:\Users\peter\Desktop\WPy64-31470\python\python.exe`. Principal commands:

```powershell
& $P -B scripts/04a02/make_refedit_patch_c09.py --evidence $E
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG apply --check "$E/refedit-proposal/RefEdit-C09-unapplied.patch"
& $P -B BESHStatNG.Test/C09.HostFreeSeamRunner.py --evidence $E --configuration Debug --name s9d
& $P -B scripts/04a02/compile_native_n1_c09.py --evidence $E --name native3
& $P -B scripts/04a02/checkpoint09_validation.py --evidence $E --configuration Debug
& $P -B scripts/04a02/checkpoint09_validation.py --evidence $E --configuration Release
& $P -B scripts/04a02/checkpoint09_validation.py --evidence $E --configuration Debug --focused
& $P -B scripts/04a02/checkpoint09_validation.py --evidence $E --configuration Release --focused
& $P -B scripts/verify-04a02-fixtures.py summarize --evidence $E --name checkpoint09-final
& $P -B scripts/04a02/checkpoint09_collect_results.py --evidence $E
& $P -B scripts/verify-04a02-fixtures.py preservation --evidence $E --name final
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --exit-code
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
```

Full runs use hash-bound isolated `verify-beshstatng.ps1` execution copies with `-Legacy -Wasm -Js -Python -VisualStudio 2026`, no restore and unique copied writer sandboxes. The C09 copy additionally passes `/p:BuildProjectReferences=false` to rebuild the Legacy **test** project against unchanged cached C08 Windows product references. This prevents shipping GenerateVersionAttributes/Help targets from rewriting protected source. All six Debug/Release product/Core/Common reference DLLs match their retained C08 bytes. Portable projects build/test normally. Shipping Windows/XLL generation was not rerun; no new product patch exists to build. The original literal verifier was not run into historical writer locations. Copy provenance/diffs and exact argument arrays/exits are retained.

`final-test-identities-c09.json` retains all TRX/Python/Node identities and multiline names. TRX locations are `$E/{Debug/c9D,Release/c9R}/results`, `$E/executions/{c9D,c9R}-{Common,Legacy}/results`, `$E/{o9d,o9r}/results` and `$E/{s9d,s9r}/results`. `*-command.json`, native compile/self-test and seam execution receipts retain commands, hashes and outcomes.

Preservation passed for **137,922 historical files**, with full path membership, size, creation/write nanoseconds and file identity plus frozen source hashes; it does not claim to reread every historical byte. The **830 C08 sealed artifacts** were rehashed with no discrepancies. C08's earlier **7,157 C01–C07 sealed-file** result remains protected and was not repeated as another inventory loop. All original **101 paths** and **2,273 source pins** match; generated VersionAttributes was not rewritten. Initial free bytes: **531,585,515,520**; final census: **531,414,409,216**. No protected evidence was deleted, moved, archived, compressed or rewritten.

The C07 exception remains exactly: **“summarize-c07.py earlier working source bytes/metadata not retained before its authoring revisions; first output JSON retained”**. No missing old bytes were invented. Original strict/profile remain **1/1 with 24 historical failures**; they were not rerun into sealed C30 outputs or falsely turned green. Gate C2 remains UNMET, E1–E7 OPEN and GC3–GC8 DEFERRED, with accepted 04A.01's 1,514 deferrals / 802 restricted captures / 892 incomplete controls / 622 wiring gaps unchanged.

The [publication manifest](04A02/04A02-C09-publication-manifest.json) distinguishes tracked modifications, visible untracked sources and ignored local-only documents, with exact SHA-256 and applicable ignore rules. It includes all original 101 paths and C09 additions, with self-hash excluded and covered by the final evidence seal. Thirteen C08 new documents remain ignored; no protected allowlist was edited or `git add -f` used. Untracked and ignored files are **not** represented as present in a GitHub clone. A future publication action requires explicit owner authorization. Preflight `check-ignore -v` reported 67 matching rules, including negations; that was a rule inventory, not an ignored-file count. The final manifest uses actual quiet ignore membership and records this clarification without rewriting the starting evidence.

Only the irreducible human actions remain: normal-owner context evidence; the exact product patch decision and independent Windows risk acceptance/desktop calibration; after real R outputs, two genuinely distinct comparison reviewers and later P; after actual N1, exact N2 authority; eventually complete G5 evidence and explicit official acceptance. **No generic S/R/GUI reapproval is needed. 04A.02 remains unaccepted; 04A.03 has not started.**
