# Official 04A.02 — C13 verified owner startup and per-case R handoff

**04A.02 NOT ACCEPTED; Gate C2 UNMET; P NO; N1 NO GO; N2 NO; 04A.03 NOT STARTED. Stop for owner review.**

The actual successful owner C12 startup is locally verified against retained originals, including the previously unuploaded zero-byte stderr. R/stats 4.5.1 x64 and the bundled compiler 4.5.1, all 64 component pins, 29 compiler membership/hash pins and all three native NTFS path chains pass. No startup was rerun. **Neither ANOVA nor KW statistical reference executed in C13; no independent R comparison or golden promotion exists.**

The current process is sandbox SID `S-1-5-21-142429299-24905182-1790040237-1012`. Live v4 ACL assessment returns `EXECUTION_NOT_NORMAL_OWNER`, `UNREADABLE:C:\Users\peter`, and `OWNER_LACKS_REFERENCE_ROOT_ACCESS`; full ancestry locking fails at `C:\Users\peter`. The expected owner is SID `...-1002`. The prior owner startup does not transfer its token to this process. No escalation, impersonation, ACL change or R invocation bypass was attempted.

The two source-reviewed technical records use v6's exact `04A02-C12-R-technical-v1` schema and `R_TECHNICAL_GO` status **conditionally for the exact normal owner context after hash review**. Both pass qualification replay against the actual retained owner observation and actual C12 receipt. This is explicitly a replay, not a current sandbox GO. Their activation/reviewer fields disclose pending owner technical review. Wrappers require an explicit exact reviewed GO hash, capture the actual owner identity in a new exclusive review receipt, and call unchanged v6 with fresh native ACL/ancestry/all-input locks before R. No owner decision, independent review signature or live observation is fabricated.

The C11 ACL review remains SHA `8fe99ff0c6ae04f0b19b953f3066003a0ef1ad121362b6372f98e56d59405eb7`; its exact canonical observation SHA is `9aac36df8886ab8f50f89e69ad1ae37137c8c7d122e3e903ac5bedd03081c2bb`. It is not a blanket current-ACL attestation. Every execution must recollect byte-equivalent canonical evidence under held ancestry. On drift, stop and collect/review new evidence in a new path; do not alter an old review. Current installation/package/ancestor ACL inspection separately passes for 40 objects.

The owner C12 originals are at `C:\Users\peter\AppData\Local\BESHStatNG-04A02-C09-reference-20261101-7d4a\c12-startup-20261101-v1`. Receipt schema/status are `04A02-C12-startup-receipt-v1` / `R_RUNTIME_PROBED`; exit is 0. Full inventory includes the retained pending hard-link publication file. The C10/C11 failures are unchanged and unqualified. The new [startup verification record](04A02/04A02-C13-C12-startup-verification-v1.json) separates owner-provided independent ChatGPT review from Codex's read-only local verification. Raw owner receipt/stdout remain outside source-control snapshots; the new source fixture holds hash pointers only.

| Original artifact | Locally verified SHA-256 |
|---|---|
| `startup-receipt.json` | `d18fb85b20041c20a6b04c4386c1baaef285edbb203c3d841e8ce9dc2781be42` |
| `raw-stdout.bin` | `65d9770ddd915052b2fbb0b0ebdc0ad758581b6af961cbff07e0905fe04582f4` |
| `exit.json` | `d24219f6de94b955cf36d25bd2aca88e2d35eef45b9317b7083f55ec8fe9653c` |
| `preflight.json` | `5f8d8a1a070c81b57b729b0073e5b21471db6ce9f2ed94d3cbc69f9ab7b40e87` |
| `startup.R` | `6af48e2d873bd413a6b5d8cdb4d2534040fb756c5a0a7b4a230dbd9d545b9106` |
| `raw-stderr.bin` | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |

Manifest v6 remains SHA `d8f3338bfe588e47ba6f78b589b43f587448ffbd52948558ad9e0af6f2bc8bd1`; the startup wrapper remains `5bdb6ac8006fdb2c278d1955c6fac321e84c1c1acd87a35b802e8ae5f49c614b`. Native identity proves `compiler.namespace_path`, `compiler.package_path` and `environment.R_HOME` resolve through complete non-reparse held NTFS chains to the pinned canonical directories, including actual 8.3 aliases. Compiler file ID is 6755399442338630, R-home ID 34058472183269276, volume serial 1760959597. No portable R 4.6.2 substitution or literal-string allowlist relaxation occurred.

The C08 receipt (`8a4c8e0973e7b24f66795559ac1841d1144ba1edc27dc1fa685ff7fa274b4baf`) already authorizes exact C07 S and independent R for both cases. No generic S/R reapproval is requested. Frozen `runtime_proposal.scientific_method_approved=false`, R false, null tolerance slots and original PROPOSED field labels remain historical proposal bytes; current authority comes from the later exact owner receipt. The [policy-resolution record](04A02/04A02-C13-approved-policy-resolution-v1.json) resolves every field without filling those slots or changing any tolerance.

| Approved fields | atol | rtol |
|---|---:|---:|
| ANOVA SSBetween/SSWithin/SSTotal/MSBetween/MSWithin | 1e-12 | 2e-13 |
| ANOVA F; KW raw/corrected H and separate corrected formula | 5e-13 | 5e-12 |
| ANOVA upper-F p; KW raw/corrected upper-chi-square p | 2e-14 | 2e-10 |
| KW tie correction | 5e-15 | 1e-13 |
| ANOVA three df; KW df and ordered rank sums | exact value/type | exact |

Non-exact rule is `abs(subject-reference) <= max(atol, rtol*abs(reference))`. Approved ordinary p domain is inclusive `[1e-12, 0.999999999999]`. ANOVA is classical equal variance F(2,6), alpha .05, no Welch/posthoc. KW uses pooled midranks, raw H and corrected H/p separately, asymptotic chi-square(2) upper tails, Dunn off. Labels/ordered values/options/RowIds remain frozen; RowIds are provenance only, not GroupedNumericData transport. No field lacks a resolved approved tolerance.

| Case | R generation | Common observations | Independent numeric difference | Next gate |
|---|---|---|---|---|
| C30-SYN-ANOVA | NOT RUN | Strict schema pass; Debug/Release and both cultures byte-identical to C12 | unavailable | exact owner technical review/live GO and one-shot execution |
| C30-SYN-KW | NOT RUN | Strict schema pass; Debug/Release and both cultures byte-identical to C12 | unavailable | separate owner review after completed ANOVA publication/comparisons |

The [per-field candidate table](04A02/04A02-C13-candidate-comparison-status-v1.json) gives actual Common values, exact policies/mappings, and null R/reference/delta/limit values. Reference-only KW rank sums, tie factor, df and separate formula remain distinct from unavailable Common members. This table is source-only continuity, not independent statistical validation. Each wrapper runs only the original frozen v2 statistical script through v6, retains raw/protocol/command/environment/publication artifacts, and invokes the unchanged comparator separately for invariant and sk-SK observations. Any reference/parser/comparison failure stops with retained evidence; candidates remain NOT_GOLDEN.

Only five new source/test files were added; existing files modified by C13: **none**. No Core/Common algorithm, public contract, product UI, project target, frozen operator/manifest/startup, C10 integration, historical fixture or approved scientific-policy bytes changed.

| New source | SHA-256 |
|---|---|
| `scripts/04a02/reference_handoff_c13.py` | `6d2e5aadb343fbfe5873f1dace66b9b1c6db9bee027c06a217fb65c2deb2fc6c` |
| `scripts/04a02/owner_reference_c13_anova_v1.ps1` | `15fe18668a44ac2d0ad28004b7254dfda917b7d18202c1f351a5ac64507189ed` |
| `scripts/04a02/owner_reference_c13_kw_v1.ps1` | `844f543500e57638f514f9987e5d0a6d6b700b6a9abab2778c1f6b87472a7de7` |
| `scripts/tests/test_04a02_checkpoint13.py` | `353d5032bac8b0b70438ce6d6c4666abb9d60544f1f0ad9fdcd25c1959ef2cf4` |
| `scripts/tests/fixtures/04a02-c13-owner-c12-startup-pointer-v1.json` | `56555a3c3a2deff63c9b1c06e8b1fd6d2aa1eb5aac2774756b58dad0d9abba01` |

Additional new evidence comprises case GO records, test/policy/startup receipts, two unsigned scientific review packets, P decision form, all-eleven-case matrix v7, native risk decision, G0-G5 dossier, candidate status table, handoff/source/publication manifests, this report and the fresh exclusive evidence tree. The publication manifest records exact paths/hashes and actual ignored/untracked status. Protected ignore allowlists remain unchanged; no local-only artifact is claimed to be on GitHub.

| Validation per configuration | Debug | Release |
|---|---:|---:|
| Managed pass | 3210 | 3210 |
| Managed failures | 0 | 0 |
| Managed not executed | 0 | 0 |
| Distinct Python identities | 516 | 516 |
| Distinct Python all-pass | 513 | 513 |
| Distinct Python nonpass | 3 | 3 |
| Node pass/distinct | 916 | 916 |

Managed comprises Core 1,024; Common 806; Office 505; WASM managed 45; real Legacy 821 (including 31 C10 coordinator/control tests); C07 seams 9. All pass without skips in each configuration. Node comprises Office 468, Help 71, WASM 377, all pass without skips. Compared with C09's 3,191 / 373 / 916 baseline, managed adds the already integrated C10 19-test delta and Python adds C11 50, C12 65, C13 28 distinct identities. Focused 65 unchanged C12 plus 28 C13 = **93 pass**, zero failures/skips; those identities already occur in discovery and are not added again. Full/focused per-config Python attempts are 603, deduplicated to 516 identities. The 93-test executed receipt pins v6 and current test/helper/fixture sources.

The unchanged three C10-era Python compatibility nonpasses are:

- `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook`: FAIL.
- `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings`: ERROR.
- `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof`: FAIL.

The C09 test expects an old setter anchor; source-spec admission expects the old RefEdit SHA; investigation06 expects old Handles/RemoveHandler syntax. These are unchanged historical expectations against the C10-authorized integration. Supplemental version-aware characterization separately reruns 17 original assertions against the C10 current-source context and two historical-preimage characterizations; it never replaces the three failures or creates a false green full-Python claim. No unrelated test expectation was edited. Initial C13 authoring retained a 27-pass/1-error test attempt (expected internal exception versus public Refused); corrected current tests pass. A wrapper-generator raw-string SyntaxError occurred before producing source; its authoring source was retained, then corrected. The sequential driver's first legacy-characterization invocation omitted its required mode and exited 2 without running tests; its command/log remain retained, and fresh explicitly named current/historical invocations pass. None of these attempts launched R or rewrote an existing destination.

Shared build/test writers ran sequentially, with fresh isolated outputs. Principal exact repeatable commands from repository root (`$P = C:\Users\peter\Desktop\WPy64-31470\python\python.exe`, `$E = TestResults/04A-02-initial-fixtures-c13-20261101T190032Z`):

```powershell
& $P -B "$E/execute.py" windows --configuration Debug --name winD1
& $P -B "$E/validation.py" full --configuration Debug
& $P -B "$E/run_remaining.py"
& $P -B -m unittest discover -s scripts/tests -p 'test_04a02_checkpoint1[23].py' -v
& $P -B "$E/verify_handoffs.py"
& $P -B "$E/comparison_plan.py"
& $P -B "$E/collect.py"
& $P -B "$E/preserve.py"
```

**These historical run commands describe completed exclusive destinations; never rerun them into this sealed tree.** Reproduction requires a new exclusive evidence root/names and copied execution helpers. Command/exit/log/TRX receipts contain full actual argument arrays and hashes. Portable validation uses a hash-bound source-preserving execution copy of `scripts/verify-beshstatng.ps1 -Wasm -Js -VisualStudio 2026`, cached --no-restore builds, and isolated internal test writers. Its Python section runs separately to retain all groups despite known nonpasses. Literal verifier/default `-Legacy` packaging targets were not run into historical outputs: real shipping and Legacy projects use controlled `PrepareForBuild;ResolveReferences;PrepareResources;Compile;CopyFilesToOutputDirectory`, `/m:1`, isolated output/intermediate paths and cached unchanged Core/Common references. This avoids VersionAttributes/Help-output writers, restore, packaging/signing/installers. Full production packaging is not tested. VB root namespaces remain BESHStatNG / BESHStatNG.Test; real PIA delegate compilation and System.Action qualification from C10 remain accepted by the compiler. This is managed compile/regression evidence, not a native COM regression.

Both owner wrappers pass PowerShell AST parsing. Actual invocation under SID1012 refuses at the first identity check before R, review receipt or attempt creation. Native qualification tests include held NTFS path identity, a real temporary junction rejection, deny-delete directory rename and file-write denial on accessible subtrees, plus injected provider/reparse/API failures. These do not qualify the protected owner ancestor under this token or claim a real owner-desktop/native Excel pilot. Manual Excel smoke checks remain blocked: (1) exact Ui82x2 initialized controls and RefEdit focus/rebind/dispose behavior only after independent N1 GO; (2) real sheet/workbook/provider ordering and immutable workbook checks remain separate unexecuted native obligations, with worksheet deletion/N2 disallowed. WASM smoke coverage here is managed/Node harness validation; no interactive browser smoke was run.

Preservation passes for **2316 starting source pins**, all **101 original paths**, and **142138 historical TestResults files** by membership/size/creation-write nanoseconds/file identity. C08–C12 sealed artifact byte rechecks cover **830 / 983 / 1249 / 985 / 995** files plus their listed source documents. The prior C01–C07 7,157-file seal result remains protected; this is not a claim to reread every historical byte. All **40 external owner objects (25 files, 15 directories)** preserve membership, bytes and metadata across C10/C11 failures, actual C12 success and owner collection files. No attempt was reused, deleted, renamed, moved, compressed or rewritten. The C12 pending hard-link publication artifact is retained. Original strict/profile **exit 1/1 and 24 historical audit failures** remain unchanged retained results, not rerun into sealed writer directories.

The C07 exception remains: “summarize-c07.py earlier working source bytes/metadata not retained before its authoring revisions; first output JSON retained”. Final preservation free bytes: **529705304064**. Branch remains codex/04A-02 and HEAD/main `12e25924aae93f0b36ab33937176c86c19641b50`; staged index and diff/check are clean. No stage/commit/merge/push/reset/checkout/clean/rebase/install/ACL edit occurred.

Review the exact ANOVA technical record and wrapper first. Independently check the wrapper hash, then run **only this first command** in normal non-elevated owner PowerShell from `C:\Users\peter\source\repos\BESHstatNG`. The explicit hash parameter records review of that exact conditional technical GO; it does not request generic S/R authority or approve P/native execution.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\04a02\owner_reference_c13_anova_v1.ps1 -ReviewedTechnicalGoSha256 7294f4f9a724d6991bc142fad70eea2ec234f3236db18c0ddf5ecc824b7461aa
```

ANOVA wrapper SHA-256: **`15fe18668a44ac2d0ad28004b7254dfda917b7d18202c1f351a5ac64507189ed`**. ANOVA technical GO SHA-256: **`7294f4f9a724d6991bc142fad70eea2ec234f3236db18c0ddf5ecc824b7461aa`**. New attempt: `C:\Users\peter\AppData\Local\BESHStatNG-04A02-C09-reference-20261101-7d4a\c13-reference-anova-20261101-v1`; no such directory exists at closeout. KW wrapper SHA-256: **`844f543500e57638f514f9987e5d0a6d6b700b6a9abab2778c1f6b87472a7de7`**; KW GO SHA-256 **`0b5fe52776092a33b366472eca1d1e9a8c60cab13349952bb6c6f2fb6b9ab494`**. The separate KW command is in the [handoff record](04A02/04A02-C13-owner-handoffs-v1.json) and additionally requires the actual reviewed completed ANOVA publication hash; it is not supplied or invented now.

After ANOVA, return the new `TestResults/04A-02-c13-owner-review-anova-20261101-v1.json` and all files in its fresh owner-root attempt: `preflight-v6.json`, `approved-input.dcf`, copied proposal source, exact raw stdout/stderr, process-exit, reference-candidate, publication-v3 and both comparison-c13 files, plus any failure/pending/partial files. Keep originals untouched. The operator's own publication hashes and all strict parser/finite/metadata/tail/schema checks must pass; every comparison must be checked for actual numeric discrepancies. On any failure retain everything and stop; do not reuse a name. Do not rerun C10/C11/C12 startup. A successful candidate publication still has NOT_GOLDEN status and no independent scientific review or P.

The [G0-G5 dossier](04A02/04A02-C13-G0-G5-dossier-v1.json) and matrix v7 retain all eleven original rows and **218 unique blocked controls / 239 deferred obligations / 11 groups**. G2 needs actual owner-context per-case execution, publications and independent comparisons. G4 needs a named independent Windows risk/source acceptance of shared UNKNOWN_HIGH RefEdit and exact `Ui82x2.RefEdit1_WorksheetData`, `RefEditOutput`, `btnHelp`, an owned Excel PID/start/session/SID/window/Excel-XLL/source binding, read-only NativeOM/UIA provider calibration and a reviewed saved disposable /x workbook with immutable state proofs; only an exact N1 stage GO can then permit initialized-form state. None exists here. G5 needs complete original case evidence, two genuinely distinct named scientific reviewers with verifiable sources/verdicts, later explicit P and separate official acceptance. The two review packets deliberately have null identities/verdicts until real reviewers are assigned; two Codex forms are not two reviews. The owner P form remains NO. No native waiver, scope amendment, 04A.03 work or official batch acceptance is inferred.

**04A.02 NOT ACCEPTED; Gate C2 UNMET; P NO; N1 NO GO; N2 NO; 04A.03 NOT STARTED. STOP FOR OWNER REVIEW.**
