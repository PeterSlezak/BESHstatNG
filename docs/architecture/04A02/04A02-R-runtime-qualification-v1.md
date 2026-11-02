# R 4.5.1 runtime qualification ? read-only probe scope

**QUALIFIED_FOR_READ_ONLY_PROBING. NOT QUALIFIED FOR OFFICIAL REFERENCE EXECUTION.** Owner C06 explicitly authorized local discovery/version/startup/hash/dependency/ACL inspection. S remains PENDING; R/P/N remain NO. No ANOVA, KW or other statistical reference script was executed; historical R 4.4.3 was not invoked. No installation, package download, PATH/registry or system setting change occurred.

Observed installation: `C:\Program Files\R\R-4.5.1`. Explicit operator-compatible executable: `bin/x64/Rscript.exe`; required runtime `bin/x64/R.dll`. Root bin/R.exe and bin/Rscript.exe are separate wrappers and were hashed, not substituted for x64 executable. R_HOME was unset in the parent inspection; each child received an explicit scoped environment.

| Observed file | SHA-256 |
|---|---|
| `C:\Program Files\R\R-4.5.1\bin\R.exe` | `49bea1f72a9b4b37c420f9b2386b9f9c4f3fc46bf1f2ff1dfd783b69f03902eb` |
| `C:\Program Files\R\R-4.5.1\bin\Rscript.exe` | `49bea1f72a9b4b37c420f9b2386b9f9c4f3fc46bf1f2ff1dfd783b69f03902eb` |
| `C:\Program Files\R\R-4.5.1\bin\x64\R.exe` | `5bbc95a505dfd3320d0ecc3f6624409f4b48c52c78c759905c6c3a3e4d3e0556` |
| `C:\Program Files\R\R-4.5.1\bin\x64\Rscript.exe` | `ba14ed16b879d617ab9ec69570c6f9edf4d7f2c5ea17235021812a9d9af0f16d` |
| `C:\Program Files\R\R-4.5.1\bin\x64\R.dll` | `11d32b3dcb3e919ea3bb95be4965d3c6c6dc3a63abb66d74e45973a24d654ed1` |
| `C:\Program Files\R\R-4.5.1\library\stats\libs\x64\stats.dll` | `ee1171add01148d6d025612d7fa34deada3d79347be745213270f2a4625b185a` |

Rscript `--version` reports 4.5.1 (2025-06-13). Nonstatistical startup checks `getRversion`, bundled `utils::packageVersion("stats")`, arch and pointer width: **R 4.5.1 / stats 4.5.1 / x86_64 / 8-byte pointers**, valid JSON and empty stderr under explicit `LC_ALL=C`, `LANG=C`, private HOME/R_USER/TEMP/library dirs, `--vanilla`. A separate operator-shaped minimal environment (`R_HOME`, R bin PATH, TEMP/TMP, LC_ALL=C) also started successfully with empty stderr. During that nonstatistical startup wait, 38 actual loaded modules were enumerated and hashed, then R exited normally.

The first retained attempt inherited C.UTF-8 and generated four Windows locale startup warnings. Its probe JSON was also overescaped by the probe author. Neither was admitted as a clean JSON smoke. A new uniquely named C-locale sandbox corrected both; original stdout/stderr/command files remain untouched. The frozen v2 operator already sets LC_ALL=C, so this does not justify changing old source or weakening its stderr refusal.

Static PE review inspected 7 R-owned components / 98 normal import edges, no unresolved normal imports; API-set imports remain OS loader contracts, not invented standalone DLL hashes. The dossier pins 35 R-owned loaded/runtime/package files, including bundled package DESCRIPTION/NAMESPACE/R databases/indexes where present. **Normal import inspection and startup module observation do not qualify delay/dynamic statistical loading, complete Windows SxS resolution, installer provenance or a future method run.** All runtime/closure proposals remain for independent owner review.

Authenticode for x64 Rscript is NotSigned; runtime hash/version pinning is not a publisher or installer-origin attestation. Program Files ancestors were inspected with owner/SDDL/rights; Users have read/execute while administrative/system identities may modify. The sandbox evidence root is owned by CodexSandboxOffline and inherits multiple modifying principals. **No private official-output ACL attestation is granted**; a fresh trusted run root and owner ACL/ancestor review remain R prerequisites. No ACL was changed.

Machine paths/ACLs/version outputs live in qualification/provenance records only, never numerical comparison keys. Raw evidence is under `TestResults/04A-02-initial-fixtures-checkpoint06-20261030T181007Z`: `runtime-acl-and-gui-environment.json`, `r-probe-commands.json`, `r-probe-c-locale-commands.json`, `r-static-dependency-review.json`, `r-loaded-modules.json` and per-attempt raw files.

Supplemental full ancestor inspection recorded 11 paths in `runtime-ancestor-acls-qualified-status.json`. `C:\Users\peter` ACL was inaccessible under current access and is explicitly **UNVERIFIABLE_ACCESS_DENIED**, not passed or inferred from adjacent inheritance. The first partial diagnostic is retained separately. This reinforces the outstanding trusted-output/ancestor review for any official R release; it does not invalidate the actually executed read-only version smoke or authorize privilege/ACL changes.

The new `04A02-C06-SR-decisions.pending.json` supplies realistic actual paths/hashes and 35 component pins for both original cases, while owner/date/signature/review fields and S/R/P/N approvals remain absent/false. Historical pending forms/manifests/scripts are unchanged. Review/adopt the exact method/tolerance/edge/p-domain proposals, then supply a separately attributable positive R execution decision with binary/script/operator/manifest hashes, runtime closure and output ACL proof. Only then may v2 run through the no-clobber operator. Real reference comparison and subsequent two independent reviewers/P promotion remain later steps.
