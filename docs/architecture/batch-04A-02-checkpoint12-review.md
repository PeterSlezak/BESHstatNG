# Official 04A.02 — C12 actual compiler-path diagnosis and bounded successor

**04A.02 NOT ACCEPTED; Gate C2 UNMET; P/N2 NOT AUTHORIZED; 04A.03 NOT STARTED.** Classification: **BENIGN_NORMALIZATION**, authenticated against actual owner C11 stdout and real Windows filesystem identity. V6 corrects two observed 8.3 aliases without changing the compiler package, version, membership, R startup source, scientific method/tolerance or any historical file. Source qualification and all 65 new tests pass. **No C12 R startup, qualified receipt, ANOVA/KW reference, independent comparison or native Excel execution occurred. Stop for owner review.**

## Actual failed attempt and precise fields

Read-only source: `C:\Users\peter\AppData\Local\BESHStatNG-04A02-C09-reference-20261101-7d4a\c11-startup-20261101-v1`. Wrapper SHA matches the independently verified `0e2fc7f942baff74f649ff0d27039585fe99a35dd5b773689980041f65bb5495`. Production strict JSON parsed the retained bytes. R exit is **0**, stderr is **0 bytes**, and the retained failure is **Compiler namespace/package provenance mismatch**. There is **no startup-receipt.json**. This is v5 admission failure after successful R process startup, distinct from C10's Unapproved loaded package. It was not caused by a sandbox SID mismatch in the owner run.

| Field | Actual owner value | Pinned expected value | Finding |
|---|---|---|---|
| `namespace_path` | `C:/Program Files/R/R-4.5.1/library/compiler` | `C:/Program Files/R/R-4.5.1/library/compiler` | Exact match |
| `namespace_version` | `4.5.1` | `4.5.1` | Exact match |
| `package_path` | `C:/PROGRA~1/R/R-45~1.1/library/compiler` | `C:/Program Files/R/R-4.5.1/library/compiler` | Different spelling; authenticated same directory |
| `package_version` | `4.5.1` | `4.5.1` | Exact match |

The first difference is therefore **package_path alone** among the four compiler fields. `getNamespaceInfo` reports the long compiler path while `find.package` reports its genuine Windows 8.3 spelling. Both versions are exactly 4.5.1. The JSON helper only converts slash direction; it does not expand 8.3 names. V5 compares the entire compiler dictionary literally, so rejects line 98 even though these paths identify the same directory.

A **second real mismatch** was masked by that first rejection: `environment.R_HOME` is `C:/PROGRA~1/R/R-45~1.1`, while v5 expects `C:/Program Files/R/R-4.5.1`. Projecting only package_path in memory reproduces v5's subsequent **Private environment escape** rejection. Projecting only these two authenticated representations makes every other frozen v5 check pass. No old JSON was rewritten or retroactively qualified.

Exact C11 attempt file inventory:

| File | Bytes | SHA-256 |
|---|---:|---|
| `empty-startup` | 0 | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |
| `exit.json` | 16 | `d24219f6de94b955cf36d25bd2aca88e2d35eef45b9317b7083f55ec8fe9653c` |
| `failure-670761df39a54d248a1b29e3505e9105.json` | 95 | `55c2ff4b637c93660643a1f48b0ed03eda7277460eac9482afb473b054d2770e` |
| `preflight.json` | 11955 | `fd6739350975b891c32cacb02cebbe71f08292d2cdabf4558bac053938987720` |
| `raw-stderr.bin` | 0 | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |
| `raw-stdout.bin` | 1719 | `f9ca4049ba9be04d2ac405aa35f1fa2d636ee44a5de5eab09ed2e47ca88d855b` |
| `startup.R` | 1964 | `6af48e2d873bd413a6b5d8cdb4d2534040fb756c5a0a7b4a230dbd9d545b9106` |

Full names, exact UTC creation/write times, nanoseconds, file identities, all directory metadata, UTF-8 field byte hex and both consumed-attempt inventories are retained in the [field diagnosis](04A02/04A02-C12-compiler-field-diagnosis-v1.json) and `$E/owner-evidence-before.json`. Both consumed names remain permanently unavailable.

## Authentication and narrow correction

Real `GetFileInformationByHandle` calls establish volume serial **1760959597**, NTFS compiler file ID **6755399442338630** for both compiler spellings, and R-home file ID **34058472183269276** for both R-home spellings. Normalized `GetFinalPathNameByHandleW` results are the expected long paths. Python samefile/stat/resolve independently corroborate those comparisons. Both full chains are held with directory data-read/attribute handles excluding delete sharing; OPEN_REPARSE_POINT and per-component attributes reject any reparse object before traversing it. Current runtime/installation/ancestor raw-SID ACL inspection passes for **40 objects**, and all **29 compiler file hashes/membership** still match the immutable C11 policy.

Microsoft documents volume/file-index comparison for identifying the same directory, and normalized final-path queries for handle-bound names. The implementation is restricted to **local fixed C: NTFS**, since the legacy 64-bit identifier is not a sufficient general ReFS identity. See [GetFileInformationByHandle](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getfileinformationbyhandle), [BY_HANDLE_FILE_INFORMATION](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/ns-fileapi-by_handle_file_information), and [GetFinalPathNameByHandleW](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getfinalpathnamebyhandlew).

V6 permits absolute C: slash direction, **actual** Windows case equivalence and **actual** 8.3 aliases only when held native identities equal both the expected directory and its new manifest-pinned volume/file ID. It does not trust lowercase or a path prefix. Relative paths, raw dot/parent traversal, other drives, UNC, device/extended prefixes, ADS, wildcards, reparse/junction/symlink objects, absent/unqueryable paths, changed trusted IDs and unqualified filesystems fail closed. Extended-prefix input is deliberately not qualified by this narrow policy; the OS's internal canonical return prefix is not an input exception.

Only after authenticating `compiler.namespace_path`, `compiler.package_path` and `environment.R_HOME` does v6 project their expected names into an **in-memory deep copy**, then call the full unchanged v5 validator. All versions, compiler metadata, unknown namespace, private HOME/profile/environment/library, owner-SID, ACL, process, timeout, clean-output, source/runtime hash, complete-ancestor locking and no-overwrite/publication safeguards remain. Per-field path/version diagnostics identify which assertion failed. Raw stdout stays exact, and a future C12 receipt retains the original strings plus native identity evidence. New C12 receipt/GO schemas prevent old failed attempts or old receipts from being relabeled successful.

**No r_startup_v3.R is needed.** Frozen r_startup_v2.R, SHA `6af48e2d873bd413a6b5d8cdb4d2534040fb756c5a0a7b4a230dbd9d545b9106`, already reports every needed actual field. The C11 namespace policy SHA remains **27a933a782ef225f058fab993068e15e3b31927120ffe3a237631e95df1e3785**; its 4.5.1 versions and complete compiler pins are unchanged. Portable R 4.6.2 is not substituted. The source-specific [v5-to-v6 review diff](04A02/04A02-C12-operator-v5-to-v6-review.patch) compares two files; v5 itself is untouched.

C11's synthetic forward fixture used the expected long spelling for **both** package_path and R_HOME. Its passing result therefore never exercised the real owner 8.3 strings. C12 tests use the exact **1,719-byte** owner output, SHA **f9ca4049ba9be04d2ac405aa35f1fa2d636ee44a5de5eab09ed2e47ca88d855b**, reproduce frozen-v5 failure, and pass the full successor against authenticated real identities. The old C11 attempt remains failed and has no receipt.

## Added files and hashes

| Added source/fixture | SHA-256 |
|---|---|
| `scripts/04a02/oracle_operator_v6.py` | `faf529c20e7d8ed7e3f9159a5618aaf442152be4214ea8bf57ce776db57a47fc` |
| `scripts/04a02/windows_r_path_identity_v1.py` | `623227969878b298606cd519ed8e0318feb8d96d6dbbffe8e57b3b0180a26b4b` |
| `scripts/04a02/owner_startup_c12_v1.ps1` | `5bdb6ac8006fdb2c278d1955c6fac321e84c1c1acd87a35b802e8ae5f49c614b` |
| `scripts/tests/test_04a02_checkpoint12.py` | `0ee49feb21502346cdcdcf30bba200e4a606b8e912b3f52f486f65a185544d19` |
| `scripts/tests/fixtures/04a02-c12-retained-c11-stdout.json` | `f9ca4049ba9be04d2ac405aa35f1fa2d636ee44a5de5eab09ed2e47ca88d855b` |

Additional files are the versioned C12 path-identity policy, v6 manifest, qualification-test receipt, field diagnosis, review diff, G2/G4/G5 dossier, report, publication manifest and fresh exclusive evidence. **Existing files modified by C12: none.** All five C10 integration files, v5/v4/v3/v2, C11 wrapper/startup/policy/manifest/receipts, old fixtures and approved S/R scientific-policy bytes are unchanged. Full exact inventory/hashes and actual ignored/untracked membership are in the [publication manifest](04A02/04A02-C12-publication-manifest.json).

Final v6 manifest SHA: **d8f3338bfe588e47ba6f78b589b43f587448ffbd52948558ad9e0af6f2bc8bd1**. Path identity policy SHA: **73f5ff0e2420b9937d158781a7ca95fd9cbfe2066232f66a61feb73a414d3667**. The original C08 exact S/R receipt remains authoritative; historical proposal `scientific_method_approved=False` is not a reversal of those later owner approvals. V6 preserves v5/v4/v3 owner admission and exact scientific hashes, P=NO, and independent technical GO gates. No numerical golden or native control was promoted.

## Actual regression and preservation

| Suite | Debug | Release |
|---|---:|---:|
| Core / Common | 1,024 / 806 pass | 1,024 / 806 pass |
| Office / independent WASM managed | 505 / 45 pass | 505 / 45 pass |
| Legacy / existing C07 seams | 821 / 9 pass | 821 / 9 pass |
| Managed distinct execution total | **3,210 pass, 0 fail/skip** | **3,210 pass, 0 fail/skip** |
| Python distinct | **485 pass / 488; 3 nonpassing** | **485 pass / 488; 3 nonpassing** |
| Node distinct | **916 pass, 0 fail** | **916 pass, 0 fail** |

All **65 new C12 tests pass**. Each configuration has 575 Python full/focused attempts but 488 distinct identities; repetitions and supplemental tests are not added to totals. Compared with C11, there are 65 new distinct Python cases, zero new managed/Node cases. Exact identities/commands/exits/TRX are in `$E/final-test-identities-c12.json` and per-run receipts. There was no new R process in these tests; the new code performs real read-only Windows metadata operations, and test-only PowerShell creates/removes one temporary junction without showing a UI or touching an installation.

Coverage includes actual v5 negative/v6 positive bytes and real loader-directory identities; valid short/slash/case forms; personal/site/other-version roots, spoofed short/case/Unicode forms, other drive, UNC, dot/traversal/ADS/device-prefix and reparse rejection; changed pinned IDs/hashes/membership; missing/wrong metadata or versions; unknown namespaces, injected environment/profile/library; output collision, consumed names and exit-0-with-failed-admission semantics. No P is allowed. Actual temporary junction leaf rejection and injected reparse attributes are distinct from full-path proof: the first test attempt hit an earlier unreadable sandbox profile ancestor. Its failing log/source/manifest draft remain retained. The corrected test keeps a strict Reparse assertion on the actual production OPEN_REPARSE_POINT leaf guard, corroborates alias/target samefile, removes only the verified temporary link itself and confirms its target remains. It does not mislabel profile Access Denied as junction validation. A real different-directory negative now uses accessible pinned base-library ancestry and asserts an identity mismatch.

The three disclosed C10 current-checkout failures remain unchanged: C09 unapplied-transformer setter anchor; frozen native source hash admission; C06 implicit Handles/unsubscribe syntax. They are two failures plus one error, not new C12 failures. No historical expectation was rewritten. Version-aware supplemental characterization also passes: **17 unchanged assertions** against C10's explicitly versioned current source-context input and **2 unchanged historical tests** on the sealed original preimage. These never replace the original three current-checkout failures or establish native safety. Full Python regression remains **not green**.

All shared build/test writers ran sequentially. Real Windows projects used the existing controlled compile/resource/output pipeline with new output/intermediates, cached Core/Common references, no source generators, signing, XLL packing or restore. Portable/Office/WASM/Node used hash-bound isolated copies of `verify-beshstatng.ps1`, with --no-restore and private Common/internal writer sandboxes; Python groups ran separately to retain results after expected failures. The literal default verifier/-Legacy Build/packaging path was not rerun into protected writer locations; exact substitutions and arrays are retained. No package install or network command ran. Read-only primary API documentation was consulted; no native Excel/GUI/NativeOM/UIA or manual browser smoke was executed. The prior conditional N1 approval remains gated.

Principal repeatable commands (`$P = C:\Users\peter\Desktop\WPy64-31470\python\python.exe`, `$E = TestResults/04A-02-initial-fixtures-c12-20261101T180814Z`):

```powershell
& $P -B "$E/preflight.py"
& $P -B "$E/qualify_identity.py"
& $P -B "$E/make_successor.py"
& $P -B "$E/prepare_manifest.py"
& $P -B scripts/tests/test_04a02_checkpoint12.py
& $P -B "$E/assess_acl.py"
& $P -B "$E/execute.py" windows --configuration Debug --name winD1
& $P -B "$E/validation.py" seams --configuration Debug
& $P -B "$E/validation.py" full --configuration Debug
& $P -B "$E/validation.py" python --configuration Debug
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

All **2,311 starting source pins**, all **101 original paths**, and every existing product/test file remain byte-identical. Complete membership/size/creation-write-nanosecond/file-identity protection passes for **141,142 historical top-level TestResults files**. C08–C11 sealed artifacts rehashed **830 / 983 / 1,249 / 985**, plus listed source documents, without errors. This is not a claim to reread every historical byte; the earlier C01–C07 7,157-file seal result remains protected. Both failed owner attempts' **14 files and 10 directories**, plus three owner collection files, retain bytes, path membership and metadata across **27 external objects**. Neither attempt was retried, reused, rewritten, renamed, removed or compressed. Original strict/profile **1/1 and 24 historical audit failures** remain unchanged retained results, not newly rerun on sealed destinations. C07's missing-unsealed-helper-byte exception remains exactly disclosed in the preservation receipt.

Preservation free bytes: **530,088,181,760**. Branch remains `codex/04A-02`, HEAD/main `12e25924aae93f0b36ab33937176c86c19641b50`; staged index is empty and diff/check passes. Protected ignore allowlists remain unchanged; untracked/ignored local evidence is not claimed to exist on GitHub. No existing source changes, checkout/reset/clean/rebase, staging, commits, merge, push, ownership/ACL edit, install, deployment or signing occurred. The new artifact seal covers the complete report/manifest/evidence with self-exclusion.

## Single fresh owner command — only after owner source/hash review

Source readiness is demonstrated by actual field authentication, immutable 29-file compiler membership/version pins, current installation ACLs, the full source review and 65 focused passes plus the reported supported regressions. This is Codex technical source review, not an independent Windows risk or numerical-promotion review.

From normal **non-elevated owner PowerShell**, with this repository as cwd, independently check:

```powershell
Get-FileHash -Algorithm SHA256 .\scripts\04a02\owner_startup_c12_v1.ps1
```

Expected SHA-256: **5bdb6ac8006fdb2c278d1955c6fac321e84c1c1acd87a35b802e8ae5f49c614b**. Inspect the fixed manifest/operator/helper/policy/test-receipt arguments, then run **once**:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\04a02\owner_startup_c12_v1.ps1
```

Fresh name: **c12-startup-20261101-v1**, proven absent at closeout. Both old names are consumed forever. The wrapper requires exact owner SID ...-1002, non-elevated token, expected repository, pinned sources/manifest/test record, fresh no-clobber name and unchanged old stdout/failed-receipt semantics. It reuses the unchanged C11 reviewed ACL record, SHA **8fe99ff0c6ae04f0b19b953f3066003a0ef1ad121362b6372f98e56d59405eb7**; v6 still recollects under complete held ancestry and demands the same canonical observation. C11's actual owner R invocation reached that strict gate before its path rejection, corroborating it at that invocation. No new owner ACL measurement is fabricated. On current drift, stop before R and obtain a new read-only collection/review in a new evidence path; do not alter old reviews or reuse a name. The current sandbox SID ...-1012 is never impersonated or elevated.

Return unchanged **raw-stdout.bin, raw-stderr.bin, exit.json, preflight.json, startup.R and startup-receipt.json**, or every retained failure file. A genuine C12 receipt must bind the new manifest/policy, exact raw hash, versions/isolation and authenticated native identities. **The wrapper has only been parsed, not executed; no C12 attempt/receipt exists.** No additional R owner run was consumed by this continuation.

**G2 remains blocked** until that genuine fresh startup and complete current technical qualification exist. Only then prepare/run separate per-case ANOVA and KW references under existing S/R authority, exact GO/startup/test/ACL/input-lock bindings, from an actually qualified owner context. Neither reference, candidate comparison nor golden promotion ran here. P still needs two distinct independent comparison reviews and explicit later owner approval.

**G4 remains blocked** on independent named Windows risk/source acceptance of shared UNKNOWN_HIGH RefEdit and exact Ui82x2 controls, real owner-desktop PID/XLL/source/NativeOM/UIA calibration and reviewed disposable-workbook/immutable-state N1 GO. No native Excel was run; N1 remains NO GO and N2 unauthorized. **G5 remains blocked** on all genuine original case evidence, two real independent reviews, later P and separate official acceptance. Full details are in the [G2/G4/G5 dossier](04A02/04A02-C12-G2-G4-G5-dossier.json).

**STOP FOR OWNER REVIEW. 04A.02 NOT ACCEPTED; Gate C2 UNMET; P/N2 unauthorized; 04A.03 NOT STARTED.**
