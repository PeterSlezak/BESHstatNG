# Official 04A.02 — internal Continuation 10 owner review

**04A.02 NOT ACCEPTED. Gate C2 UNMET. 04A.03 NOT STARTED.** The corrected five-file RefEdit successor is integrated **uncommitted** on `codex/04A-02`. Real shipping and Legacy project compilation and all managed regressions pass in Debug and Release. The complete current-checkout Python regression is **not green**: three historical source-characterization/admission identities fail after the authorized integration. Their expectations and historical files remain unchanged. No R process, reference comparison, Excel instance, native provider, or N1/N2 capture ran. P and N2 remain unauthorized.

## Exact integration and authority

The owner's C10 prompt authorizes this bounded corrected integration, reinforced by “please continue.” That continuation also permits a command-local `git -c safe.directory=C:/Users/peter/source/repos/BESHStatNG` trust exception after the initial ownership failure; no global Git setting, token, ownership or ACL was changed. HEAD/main remain `12e25924aae93f0b36ab33937176c86c19641b50`; the index is clean. Root AGENTS, Master Plan 2.3 and official 04A.02 row, 04A.01 retention/restriction records, C09 report/v2 risk/stage records, scientific S/R decisions and all five live file states were inspected. Later explicit owner decisions govern over historical pending wording in the plan and old approval forms.

The preserved C09 patch is still SHA-256 `e067eb1fc5ca70599150c349f37bf85b97ef57befc4797071a5ff67b84831b12`. It was not applied. The [corrected source-specific C10 v2 successor](04A02/04A02-C10-RefEdit-integrated-v2.patch) is SHA-256 **`42cc209673ee0aca8158c224f3eab0524133b1915374c11fea61d3d1d5b1150e`**. Its exact scope and applied source hashes are in the [approval linkage](04A02/04A02-C10-integration-approval-linkage.json). This is direct C10 authorization of the corrected scope; no approval of altered bytes is inferred from the C09 hash.

Exactly five product/test paths changed:

1. `BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb`
2. `BESHstatNG/src/UI/Refedit/RefEditEventLease.vb` (new)
3. `BESHStatNG.Test/src/MigrationRefEditBoundary09Tests.vb` (new; original 12 tests retained verbatim, plus C10 cases)
4. `BESHstatNG/BESHStatNG.vbproj` (compile item only)
5. `BESHStatNG.Test/BESHStatNG.Tests.vbproj` (compile item only)

New C10 review/evidence documents are additive. Historical reports, patches, policies, manifests, allowlists and TestResults were not rewritten. No algorithms, Core/Common public contracts, p-value semantics, target frameworks, public RefEdit declarations or x86/x64 packaging sources changed. No staging, commits, merges, pushes, installs, network/package restore, signing, deployment or branch manipulation occurred.

## Behavior and managed evidence

The shipping coordinator owns subscription add/remove actions, invalidates generations before calling providers and prevents a stale/reentrant callback from targeting a replacement source. Both worksheet SelectionChange and workbook SheetActivate now use explicit leases; borrowed `xlBook`, `xlSheet` and `xlApp` fields have no implicit WithEvents subscription setters. This avoids retaining a reference solely because implicit COM unsubscribe throws. The original restricted `xlApp@34` obligation remains an historical source ID with UNKNOWN_HIGH risk; changing implementation does not discharge it.

Focus enter, connector replacement and leave use the actual `RunCaptureBoundary` production method. Capture is disabled before cleanup. Clear and provider failures are caught, both callback generations are invalidated before either detach, diagnostics are logged, references are cleared and the entered address is retained. Leave cleanup does not depend on a non-null sheet or connector. Disposal closes both leases before reference cleanup, including failed detach and repeated disposal. A faulted control **must be closed and recreated**; no lease reset or automatic rebind to an inconsistent workbook is permitted.

Queued WriteData carries the originating generation and checks it again on the UI thread, along with active workbook/sheet identity. Selection callbacks ignore null and stale identities. Borrowed event/Areas RCWs are not released. The old unused private `_NAR` helper remains, but no event/Areas path calls it. These are source and managed regression findings, not observations of Excel COM provider ordering.

There are 21 coordinator tests and 10 tests invoking actual managed control entrypoints/handlers on an STA thread without showing a form or opening Excel. Real control tests inject managed unsubscribe delegates into shipping leases via reflection; they do not implement fake Excel COM providers. The [exception-case matrix](04A02/04A02-C10-exception-case-matrix.json) distinguishes executed boundaries from manual/native assertions. Real sheet deletion/disconnection, cross-workbook activation/selection ordering, live UI queue ordering, real event-provider failures and workbook immutability remain untested. Sheet deletion is outside the current N1 permission.

The real Windows build found an integration error hidden by the isolated assembly: `Imports Microsoft.Office.Interop.Excel` shadows the nongeneric `Action` name. V2 fixes the UI boundary parameter to `System.Action`. VB root namespaces remain `BESHStatNG` and `BESHStatNG.Test`; integrated tests resolve the shipping Friend coordinator. The compiler accepted the real PIA delegates `WorkbookEvents_SheetActivateEventHandler(Object)` and `DocEvents_SelectionChangeEventHandler(Range)`.

## Actual validation

| Suite | Debug | Release | Outcome |
|---|---:|---:|---|
| Core / Common | 1,024 / 806 | 1,024 / 806 | all pass |
| Office / independent WASM managed | 505 / 45 | 505 / 45 | all pass |
| Legacy, including 31 coordinator/control tests | 821 | 821 | all pass |
| Existing C07 lifecycle seams, using freshly built shipping DLL | 9 | 9 | all pass |
| Managed distinct execution total | **3,210** | **3,210** | 0 failures, 0 not executed |
| Original Python identities | **370 pass / 373** | **370 pass / 373** | 2 failures + 1 error, same identities |
| Node distinct | **916** | **916** | all pass |

C09 managed baseline was 3,191; the increase is 19 additional coordinator/control cases. Isolated coordinator repetitions are not added again. Python has 460 full/focused attempts per configuration but 373 distinct identities. Node is Office 468 + Help 71 + independent WASM 377. Exact identities and outcomes are retained in `$E/final-test-identities-c10.json`; no skipped/live-not-run activity is counted as a pass.

The three current-checkout Python incompatibilities are integration-induced, not pre-existing unrelated failures:

- `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook`: frozen C09 transformer requires the original setter anchor, which the corrected handler intentionally replaces.
- `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings`: frozen native specification correctly rejects the changed RefEdit hash.
- `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof`: requires the old implicit `Handles xlBook.SheetActivate` and old unsubscribe syntax.

No tests were weakened or changed to mask these results. The [versioned C10 source-context successor](04A02/04A02-C10-windows-source-context-v2.json) changes only the current global source hash and adds provenance; every native gate, exact blocker, risk and other source reference stays unchanged. All 17 unchanged source-admission tests pass against that explicit successor input. Two unchanged C09/C06 historical characterization tests also pass when run against the frozen pre-integration source snapshot. Those supplemental results **do not replace** the three current-checkout failures or constitute current native qualification. Migrating the frozen helper/checks themselves was outside the authorized five product/test paths.

Principal commands, with `$P = C:\Users\peter\Desktop\WPy64-31470\python\python.exe` and `$E = TestResults/04A-02-initial-fixtures-c10-20261101T161342Z`:

```powershell
& $P -B "$E/preflight.py"
& $P -B "$E/prepare.py"
& $P -B "$E/execute.py" isolated --configuration Debug --name isolatedD2
& $P -B "$E/execute.py" isolated --configuration Release --name isolatedR
& $P -B "$E/source_checks.py"
& $P -B "$E/execute.py" patch
& $P -B "$E/execute.py" integrate
& $P -B "$E/revise_v2.py"
& $P -B "$E/execute.py" windows --configuration Debug --name winD6
& $P -B "$E/execute.py" windows --configuration Release --name winR1
& $P -B "$E/validation.py" full --configuration Debug
& $P -B "$E/validation.py" python --configuration Debug
& $P -B "$E/validation.py" seams --configuration Debug
& $P -B "$E/validation.py" seams --configuration Release
& $P -B "$E/validation.py" full --configuration Release
& $P -B "$E/validation.py" python --configuration Release
& $P -B "$E/qualify_r.py"
& $P -B "$E/collect.py"
& $P -B "$E/preserve.py"
git -c safe.directory=C:/Users/peter/source/repos/BESHStatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHStatNG diff --cached --exit-code
git -c safe.directory=C:/Users/peter/source/repos/BESHStatNG status --short --untracked-files=all
```

All shared builders/test writers ran sequentially. Each attempt has fresh exclusive outputs and command/exit/log receipts. `winD1`–`winD5` retain failed build-harness/compiler attempts; `failure-lineage.json` explains them, including the genuine Action binding defect corrected in v2. The first source-check harness incorrectly expected 33 methods instead of the actual 31; its failure/source snapshot is retained. No test bodies were changed for that count correction.

Portable verification used hash-bound execution copies of `scripts/verify-beshstatng.ps1`, with recorded diffs, `-Wasm -Js -VisualStudio 2026`, `--no-restore`, and isolated Common/test-internal writer sandboxes. Python commands from its `-Python` section were run separately so all results survived a failing group. The original literal verifier was **not** run into historical TestResults. Its literal `-Legacy` mode/default shipping Build pipeline was **not** run: real shipping/Legacy projects were instead compiled separately with `PrepareForBuild;ResolveReferences;PrepareResources;Compile;CopyFilesToOutputDirectory`, cached unchanged Core/Common DLLs, `/m:1`, redirected output/intermediate directories and transitive copy disabled. Exact full arrays/exits are in `winD6/*-command.json` and `winR1/*-command.json`. This compiles real product sources/resources and test references without triggering protected VersionAttributes/Help writers, package restore, XLL packing, signing or installers. Default packaging/full production build remains not run.

## R owner context and portable candidate

The actual owner ACL collection now exists. Receipt SHA-256 is `c8978449ad5d2d197339978b6badf9dfc281fc9fe3619577209993f47a052c57`; raw observation SHA-256 is `2ca82cdc4d24377b2d79bdb386a09c5a56c6305332a527b7a67b24dd611b8d7b`. Collector hash and normal-owner SID 1002 match, allocation is recorded empty and collection exited 0. The exact observed ancestry passes unchanged v4 static ACL assessment. This is actual owner-collected evidence and a Codex technical review, not an invented independent human review or a transfer of the owner's token to the sandbox.

Fresh current collection still reports SID 1012, unreadable `C:\Users\peter` ancestry and inadequate current root access. Complete ancestor locking fails at that profile directory. Therefore startup/reference execution remains **NO GO here**, despite the valid owner receipt. No elevation, impersonation or ACL relaxation was attempted. The existing owner root still contains only the three ACL collection files. **Do not rerun the C09 `-AllocateEmptyRoot` command against it.**

Both case policies/manifests and all frozen R 4.5.1 runtime/script/package pins pass. The preferred route remains the unchanged pinned 4.5.1 operator. The portable candidate contains both `bin/Rscript.exe` and `bin/x64/Rscript.exe`; both PE images are x64. File metadata for x64 Rscript/R.dll and base/stats DESCRIPTION records agree on 4.6.2. Each runtime has 105 statically inventoried bundled/base/stats files; complete file hashes and PE normal imports are retained. All 189 normal import references for 4.5.1 and 186 for the portable candidate resolve statically to bundled files, System32 or Windows API-set contracts. Delay/dynamic imports, actual loaded DLL paths, startup R/stats identity, locale and library search paths remain unobserved. Neither runtime is newly startup-qualified. No portable operator/policy/manifest replaced v2/v3/v4; no tolerance or method changed.

The [R qualification record](04A02/04A02-C10-R-context-and-runtime-qualification.json) binds those findings, raw copies, hashes and private-environment startup policy. From normal **non-elevated owner PowerShell**, in this repository, the next one-time command is:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\TestResults\04A-02-initial-fixtures-c10-20261101T161342Z\r-qualification\owner-startup-c10.ps1
```

This prepared wrapper refuses a sandbox/elevated token or existing startup attempt, then invokes only the already-authorized nonstatistical 4.5.1 startup through frozen v4 admission, current ACL recollection, complete held ancestry/input locks and private HOME/R_USER/libraries. Its technical ACL review is attributed to Codex and bound to the actual owner observation; any changed observation fails closed. Return actual startup raw stdout/stderr, exit and startup receipt. Per-case technical GO and real references/comparisons still follow a successful qualified startup. Statistical reference runs, independent comparisons, two human reviews and P remain absent.

## Native and acceptance boundaries

The [N1 decision](04A02/04A02-C10-native-risk-decision.json) remains **NO GO**. Conditional GUI approval persists. Product integration is now evidenced, but there is no independent named Windows risk reviewer/source, accepted shared edge/exact control release, bound owner-desktop Excel PID/start/session/SID/window/XLL/source receipt, real UIA AutomationId/provider map, calibrated NativeOM binding, reviewed saved disposable `/x` workbook/immutable measurements or exact stage GO. C09 helper self-tests are not native evidence. No N1 helper or Excel process was launched.

The exact blocked controls remain `Ui82x2.RefEdit1_WorksheetData`, `Ui82x2.RefEditOutput` and `Ui82x2.btnHelp`. All 218 blocked controls, 239 obligations and 11 risk groups persist. N2 is forbidden. No ROT, global Quit, SendKeys, coordinate fallback, borrowed-RCW release, worksheet deletion, save or native output capture was used. Later N1 can cover initialized form state only after independent stage GO; lifecycle deletion scenarios require separate bounded native authority.

Manual smoke checks **NOT RUN**: (1) after independent N1 stage GO, initialize exact Ui82x2 and read counts/options/global alpha while proving disposable workbook state unchanged; (2) browser/WASM smoke-harness ANOVA/KW navigation and synthetic typed-input validation. Automated Node/managed WASM tests do not establish these manual observations. No production smoke-harness UI was added.

## Preservation and publication

Preflight verified all 101 original changed/untracked paths, all 2,273 original source pins and all 132 C09 publication entries. Final checks retain **all 101 original paths byte-identically**; 2,270 original pins remain equal, exactly three tracked pins are intentionally superseded by the authorized five-file integration, and two files are new. All 2,304 combined starting source pins are reconciled to the explicit integration allowlist. Generated VersionAttributes bytes remain unchanged.

Final preservation passed for **138,906 historical top-level TestResults files**, with complete path membership, size, creation/write nanoseconds and file identity. C08's 830 sealed artifacts and C09's 983 sealed artifacts plus their listed source documents were rehashed without errors. This does not claim to reread every historical byte. The earlier C08 7,157-file C01–C07 seal result remains protected. The original strict/profile **1/1 and 24 historical failures** remain unchanged historical receipts; no audit writer reran into old outputs, and they are not a fresh source audit of the modified product.

The C07 exception remains exactly: “summarize-c07.py earlier working source bytes/metadata not retained before its authoring revisions; first output JSON retained”. No missing bytes were reconstructed. Historical paths were not renamed, moved, archived, deleted or rewritten. Initial free bytes were **530,876,317,696**; the final preservation census measured **530,643,894,272**. Git diff/check, clean-index and reverse-patch checks passed; branch and HEAD/main are unchanged.

The additive [C10 publication manifest](04A02/04A02-C10-publication-manifest.json) records exact hashes, actual tracked/untracked/ignored membership and matching ignore rules, including the ignored evidence root. No protected allowlist was edited or force-added. Ignored/local-only and untracked files are **not** claimed to exist on GitHub. The final artifact seal covers this report, manifest and C10 evidence; its own hash is excluded. See the [G5 dossier](04A02/04A02-C10-G5-acceptance-dossier.json) and [eleven-case matrix](04A02/04A02-C10-G5-case-matrix.json) for the retained criteria and owner actions.

Stop for owner review: source integration is reviewable; current Python compatibility failures are disclosed; R awaits actual owner-context startup/execution; native N1 awaits independent risk and desktop evidence; numerical promotion awaits real independent comparisons, two distinct human reviews and explicit P. **04A.02 remains NOT ACCEPTED; Gate C2 UNMET; 04A.03 NOT STARTED.**
