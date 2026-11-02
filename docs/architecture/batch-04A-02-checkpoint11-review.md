# Official 04A.02 — C11 owner R startup qualification

**04A.02 NOT ACCEPTED. Gate C2 UNMET. P NOT AUTHORIZED. N2 NOT AUTHORIZED. 04A.03 NOT STARTED.** C11 adds a pinned forward operator/policy/startup instrument and one-shot owner handoff. No existing source, proposal, C10 manifest, owner attempt or historical report was modified. All 50 new admission/provenance tests pass. Both configurations retain the same three C10 compatibility failures. **The new owner startup has not executed; no new qualified receipt, statistical reference or independent comparison exists. No native Excel ran.**

## Reproduced cause and provenance

The retained owner attempt is `C:\Users\peter\AppData\Local\BESHStatNG-04A02-C09-reference-20261101-7d4a\c10-startup-20261101-v1`. Its original wrapper matches `d3edefcbae844689a3b27a6e4479bd6e78918f4152ba859aa9fced9481827e55`. Actual exit.json is 0, stderr is empty, and the failure is `Unapproved loaded package`. Startup-receipt.json is absent. These facts establish successful **R process** startup followed by operator namespace rejection, not a failed R launch or a sandbox SID error in the owner's run.

Exact retained stdout is 807 bytes, SHA-256 `1296920a22a1f3778aaa54478a605989004f82865d18454e9fc1f8b5b2f46b5f`. Its namespaces are compiler, graphics, utils, grDevices, stats and base. The sole difference from v4's allowlist is **compiler**. A host-free test feeds the exact copied bytes to unmodified v4 and reproduces that rejection. Removing only compiler from an **in-memory projection** makes every other v4 runtime, x64, locale, private HOME/profile and library-path check pass, including the profile check after namespace admission. Old bytes remain unchanged.

The installed package at `C:\Program Files\R\R-4.5.1\library\compiler` identifies Package compiler, Version 4.5.1, Priority base, Built R 4.5.1. Its namespace loader derives the lazy-load database from `loadingNamespaceInfo()$libname`, not a hardcoded package name alone. The retained owner command/environment use the pinned 4.5.1 runtime and private empty user/site libraries; the only remaining installation library in observed .libPaths is the pinned R home. Read-only raw-SID ACL inspection passes for 35 package objects: trusted maintenance owners, canonical non-reparse objects, no unknown modifying principals under the unchanged threat model. No ACL or library was changed.

**Old stdout has namespace names, not measured namespace locations.** Available package attribution follows from those controlled paths, the empty private libraries and the installed bundle; direct old namespace-location measurement is not invented. The forward startup explicitly loads compiler for metadata inspection from the bundled R-home library and emits its actual namespace path/version and discovered package path/version. All four must match the pinned installation and 4.5.1. A preloaded substitute still fails the measured namespace-path check. Actual forward path observations remain pending owner execution.

The old runtime component pins cover **none of the compiler package's 29 files**. The [new provenance record](04A02/04A02-C11-compiler-provenance-v1.json) and [namespace policy](04A02/04A02-C11-namespace-policy-v1.json) pin the entire exact package membership, including DESCRIPTION/NAMESPACE, namespace/package metadata, loader and lazy-load databases. V5 rejects missing/extra package files, altered hashes, path aliases/escapes and spoofed provenance. These new files become part of the execution-local runtime closure and are hash-checked and held deny-write/delete locked before R starts. The old v2/v3/v4 manifests and scientific-policy bytes are untouched. Trusted Windows API-set/SxS/system-loader assumptions are unchanged; exhaustive dynamic DLL closure is not newly claimed.

## Additive correction and exact source hashes

| Added source/fixture | SHA-256 |
|---|---|
| `scripts/04a02/oracle_operator_v5.py` | `9fe7b00eeab346e5c3964497cf91d98101f4cbc220cacafa5bdc64163ceebe1b` |
| `scripts/04a02/r_startup_v2.R` | `6af48e2d873bd413a6b5d8cdb4d2534040fb756c5a0a7b4a230dbd9d545b9106` |
| `scripts/04a02/owner_startup_c11_v1.ps1` | `0e2fc7f942baff74f649ff0d27039585fe99a35dd5b773689980041f65bb5495` |
| `scripts/tests/test_04a02_checkpoint11.py` | `facbd4194129352cc7fc10b1f53338df18ea1a9cd54b4110db9c3a1d94517584` |
| `scripts/tests/fixtures/04a02-c11-retained-startup-stdout.json` | `1296920a22a1f3778aaa54478a605989004f82865d18454e9fc1f8b5b2f46b5f` |

Other additions are the versioned namespace policy, v5 execution manifest, operator-test receipt, additive exact-observation ACL review, compiler provenance record, review diff, G2/G4/G5 dossier, this report, publication manifest and fresh exclusive C11 evidence root. **Existing files modified by C11: none.** All five C10 RefEdit integration files are byte-identical to the starting checkout. No algorithm, numerical tolerance, target framework, public API, output table, p-value semantics, UI or packaging source changed.

The [v4-to-v5 review diff](04A02/04A02-C11-operator-v4-to-v5-review.patch) compares immutable v4 with the new file; it is not applied as a modification to v4. V5 imports hash-pinned v4/v3/v2 primitives and reuses exact owner-SID/ACL checks, complete ancestry locking, private process environment, --vanilla, clean stderr/exit requirements, exclusive creation and no-replace hard-link publication. The only added namespace is compiler under qualified R/stats **4.5.1 x64**, with full pinned-file and fresh namespace/package path/version proof. Unknown namespaces still fail; portable R 4.6.2 is not adopted. Every original v4 probe condition remains active through a read-only projection that removes only compiler after binding the forward schema; additional environment/provenance checks fail closed.

The new manifest is SHA-256 `a25243a3baa84d6950a473641c7a2fca53d729608a70361b6b14a2e97d4ced29`; namespace policy is `27a933a782ef225f058fab993068e15e3b31927120ffe3a237631e95df1e3785`. The unchanged frozen v4 operator is `2fbc57aa49b0fbe6a5229be831a4fb3fc42a0b5bba4f174509c65bff5e20884b`; its qualified-v3 manifest remains `6c7995b70590f84e55235f980209544e59a6d963db4b197e64a374bc39c0393f`. The unsealed C11 authoring manifest draft and focused attempt are retained separately; final pins bind the final script and current tests. No rejected or historical attempt was overwritten.

## Authority reconciliation

Frozen `runtime_proposal.scientific_method_approved=False` and pending proposal fields describe their historical authoring state. They are not the authoritative later owner decision. The pinned C08 receipt `C08-OWNER-20261030-S-R-N-v1` records exact C07 ANOVA/KW S approval and independent R execution authority; C11 permits this bounded technical namespace correction. V5 still invokes unchanged `v3.owner_admission` through v4, verifies those owner statements/case/policy hashes, and retains P=NO. A focused test admits both authorized cases while asserting that the frozen false field is still false. No proposal field or tolerance was rewritten to manufacture authority.

## Validation and limits

| Actual suite | Debug | Release |
|---|---:|---:|
| Core / Common | 1,024 / 806 pass | 1,024 / 806 pass |
| Office / independent WASM managed | 505 / 45 pass | 505 / 45 pass |
| Legacy, including existing 31 coordinator/control tests | 821 pass | 821 pass |
| Existing C07 seams against freshly built shipping DLL | 9 pass | 9 pass |
| Managed total | **3,210 pass, 0 fail/skip** | **3,210 pass, 0 fail/skip** |
| Python distinct | **420 pass / 423; 3 nonpassing** | **420 pass / 423; 3 nonpassing** |
| Node distinct | **916 pass** | **916 pass** |

The 50 new C11 cases all pass in both full Python configurations and in focused qualification. They cover exact retained-byte reproduction, stock observed namespace sets with explicitly simulated forward path proof, unknown namespaces, library/profile/environment escape, wrong R/stats/architecture/locale, spoofed compiler paths/versions/hashes, membership omissions, old schema/name replay, duplicate attempts, unchanged owner-ACL policy, unknown writers, real Windows input locks and retained no-replace publication rejection. Host-free forward objects are **simulations**, not live R outputs or receipts. No R subprocess runs in these tests.

Each configuration has 510 Python full/focused attempts but **423 distinct** identities; focused repetitions are not added again. Compared with C10, 50 distinct Python cases were added and zero managed/Node identities changed. Node is Office 468 + Help 71 + independent WASM 377. Exact command arrays, timings, exits, TRX and identities are in `$E/final-test-identities-c11.json` and per-run receipts.

The same three current-source C10 compatibility identities remain nonpassing (two failures, one error):

1. `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook` requires the old C09 setter anchor.
2. `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings` enforces the frozen pre-integration RefEdit hash.
3. `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof` requires the old implicit Handles/unsubscribe syntax.

No expectations were weakened. Full Python regression therefore remains **not green**. Historical strict/profile 1/1 and 24 known failures are a separate retained audit result, unchanged and not rerun into old writer paths.

Read-only collection diagnostics are retained: the first package-ACL command omitted a parameter dash; the second account-name translation could not resolve an identity. The corrected collector reads raw SID access rules directly, keeps unknown SIDs visible and rejects unknown modification grants; all 35 objects pass. The first script-syntax harness used an uninitialized PowerShell [ref] variable; the corrected parse-only check passes. These were collection/harness failures, not accepted ACL/provenance-policy violations, and no security expectation was loosened.

Principal commands (`$P = C:\Users\peter\Desktop\WPy64-31470\python\python.exe`, `$E = TestResults/04A-02-initial-fixtures-c11-20261101T171233Z`):

```powershell
& $P -B "$E/preflight.py"
& $P -B "$E/make_successor.py"
& $P -B "$E/prepare_manifest.py"
& $P -B scripts/tests/test_04a02_checkpoint11.py
& $P -B "$E/execute.py" windows --configuration Debug --name winD1
& $P -B "$E/validation.py" full --configuration Debug
& $P -B "$E/validation.py" python --configuration Debug
& $P -B "$E/validation.py" seams --configuration Debug
& $P -B "$E/execute.py" windows --configuration Release --name winR1
& $P -B "$E/validation.py" seams --configuration Release
& $P -B "$E/validation.py" full --configuration Release
& $P -B "$E/validation.py" python --configuration Release
& $P -B "$E/collect.py"
& $P -B "$E/preserve.py"
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --exit-code
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
```

Shared builds/test writers ran sequentially using the source-preserving C10 workflow in new C11 directories. Real Windows projects use controlled compilation/resource/output targets with isolated intermediates, cached Core/Common references and no VersionAttributes/Help writers, packing, signing or restore. Portable/Office/WASM/Node use hash-bound isolated execution copies of `verify-beshstatng.ps1`, with --no-restore and safe Common/internal writer sandboxes. Its Python section ran separately to retain all groups after expected failures. Original literal verifier/default -Legacy Build/packaging was not run into protected writer locations; exact substitutions and command arrays are recorded. No network/package installation occurred. No native Excel, GUI pilot, NativeOM/UIA or browser manual smoke ran; such manual checks remain outside this R correction and their prior gates remain.

## One-shot owner handoff — not yet executed

From normal **non-elevated owner PowerShell**, with this repository as the current directory, first independently verify:

```powershell
Get-FileHash -Algorithm SHA256 .\scripts\04a02\owner_startup_c11_v1.ps1
```

Expected SHA-256: **`0e2fc7f942baff74f649ff0d27039585fe99a35dd5b773689980041f65bb5495`**.

Inspect the literal pinned manifest/review/test-receipt arguments, then execute **once**:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\04a02\owner_startup_c11_v1.ps1
```

The new name is `c11-startup-20261101-v1` under the existing owner reference root. It is proven absent at C11 closeout. The wrapper refuses sandbox SID 1012, elevated tokens, source/hash drift, an existing attempt, changed old stdout or an unexpected receipt on the rejected old run. It invokes only pinned v5 nonstatistical startup. The operator recollects exact owner ACLs under complete held ancestry, verifies/locks every source/runtime/compiler input and emits a receipt only after all fresh fields pass. Current sandbox identity is not changed or impersonated.

The additive [C11 ACL review](04A02/04A02-C11-owner-acl-review-v1.json), SHA-256 `8fe99ff0c6ae04f0b19b953f3066003a0ef1ad121362b6372f98e56d59405eb7`, retains the exact canonical owner observation accepted in the C10 owner invocation. That invocation's passage through pinned v4 pre-R checks is control-flow corroboration, not a fabricated new raw ACL measurement. New owner startup still requires a freshly collected byte-equivalent canonical observation. On drift it stops **before R**; refresh collection/review in a new evidence path and a versioned handoff, never edit sealed reviews or reuse an attempt. Do not rerun C09 root allocation or the rejected C10 startup.

Return actual raw stdout/stderr, exit.json and startup-receipt.json or retained failure files. A qualified receipt must include the fresh measured compiler location/version proof and bind this v5 manifest/policy. **No new C11 startup actually executed here; no new qualified receipt exists.** The current sandbox remains unable to act as the normal owner. Both ANOVA and KW references and independent comparisons remain **NOT RUN**. Once a genuine qualified startup exists, proceed with the already-approved references only from a genuinely qualified owner context and after matching per-case technical GO/test/ACL/input-lock gates. Candidate comparisons remain unpromoted; two distinct reviews and later explicit P are still required.

## Preservation, publication and remaining gates

All **2,306 starting source pins**, all **101 original paths**, and all five C10 product/test integration files remain byte-identical. Historical metadata protection passed for **140,156 top-level TestResults files**. C08/C09/C10 seals rehashed 830/983/1,249 artifacts and their listed source documents without errors. Earlier C01–C07 7,157-file seal evidence remains protected; this is not a claim to reread every historical byte. The rejected owner attempt's **7 files and 5 directories**, plus **3 owner collection files**, preserve exact bytes, membership and creation/write/file-identity metadata. No old attempt was retried, renamed, deleted, reused or written. The C07 missing-unsealed-helper-byte exception remains unchanged.

Final preservation measured **530,237,235,200 free bytes**. Branch remains `codex/04A-02`; HEAD/main remain `12e25924aae93f0b36ab33937176c86c19641b50`; no existing tracked file changed in C11, and the index is clean. A command-local exact-case safe.directory exception was used; the initial case-mismatched trust argument failed read-only. No global Git config, ACL or ownership changes occurred. No checkout/reset/rebase/staging/commit/merge/push/install/deploy/signing operation ran.

The [publication manifest](04A02/04A02-C11-publication-manifest.json) records all additive sources/evidence with exact hashes and actual tracked/untracked/ignored status. Protected .gitignore allowlists were not edited; local-only and untracked files are not claimed to be on GitHub. The new evidence seal covers this report and manifests with self-hash exclusion. Full status and remaining criteria are in the [G2/G4/G5 dossier](04A02/04A02-C11-G2-G4-G5-dossier.json).

- **G2:** qualified fresh owner startup; two current per-case GO receipts; actual independent ANOVA/KW raw references and comparator outputs. Existing S/R approval is retained; current technical release is pending.
- **G4:** independent named Windows risk acceptance of shared UNKNOWN_HIGH RefEdit and exact Ui82x2 controls; real owner-desktop PID/XLL/source/NativeOM/UIA calibration; reviewed disposable workbook/state proof and exact N1 stage GO. N1 remains NO GO; N2 remains unauthorized.
- **G5:** all genuine case evidence, two distinct independent comparison reviews, later explicit P and separate official owner acceptance. Three C10 compatibility failures and the 24 historical audit failures remain disclosed; no scope waiver or native/release approval is invented.

**04A.02 NOT ACCEPTED. Gate C2 UNMET. P NOT AUTHORIZED. 04A.03 NOT STARTED.**
