# Official 04A.02 — Checkpoint 02 review

**04A.02 CHECKPOINT 02 COMPLETE / OWNER REVIEW REQUIRED; official 04A.02 NOT ACCEPTED.** Original historical audit failures are not rewritten. Independent reference execution, golden promotion and native capture require separate explicit approval. No next checkpoint or 04A.03 is started.

## Verified starting state

Branch `codex/04A-02`; HEAD/main `12e25924aae93f0b36ab33937176c86c19641b50`. Inventory commit `ec5e47a0cfc352ea5fa9fcd598e1f10b5b8fbd9b` is an ancestor of main; the subsequent separate Office update is HEAD. The history is linear, not two observed merge commits. Owner statements supply acceptance authority.

Preflight verified exactly **14 Checkpoint 01 files** (two modified, twelve untracked), every recorded byte hash, and an empty index. No ZIP extraction, reset or unrelated edit occurred. The initial file list and exact hashes are in `start.json` and `start-source/` under the new exclusive evidence root:

`TestResults/04A-02-initial-fixtures-checkpoint02-20261030T135915Z/`

Initial free disk: **503,968,665,600 bytes**. Fresh historical census: **131,250 files**, including all Checkpoint 01 evidence. Original 14-path inventory:

```text
.gitignore
BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj
BESHStat.Excel.Common.Tests/TestData/04A02/C30-SYN-ANOVA.json
BESHStat.Excel.Common.Tests/TestData/04A02/C30-SYN-KW.json
BESHStat.Excel.Common.Tests/src/MigrationFixture04A02Support.vb
BESHStat.Excel.Common.Tests/src/MigrationFixture04A02Tests.vb
scripts/verify-04a02-fixtures.py
scripts/tests/test_04a02_isolation.py
docs/architecture/04A02/README.md
docs/architecture/04A02/current-owner-status.md
docs/architecture/04A02/scientific-decisions.md
docs/architecture/04A02/coverage-register-v1.json
docs/architecture/04A02/native-restriction-crosswalk-v1.json
docs/architecture/batch-04A-02-initial-fixture-preparation.md
```

## Audit reconciliation

The [reconciliation memo](04A02/checkpoint02-audit-reconciliation.md) lists all **24 exact failed identities**, assertions, source locations, old/current branch/commit, local-only dependencies and snapshot/Git identities. All retained logs were available. Twenty original `before.json` records independently confirm their old branch/HEAD. Original per-test TemporaryDirectory review overrides were not retained and remain **UNAVAILABLE**; no historical scratch input was recreated.

The stable diagnostic verified **554 product files**, all **1,527 Checkpoint 01 artifact-manifest entries**, and **39** store/prefix comparisons. Exact-number JSON parsing and bidirectional CRLF/LF reconstruction confirm the member-prefix differences are newline-only. Older whole files also contain later appended audit decisions; the report does not mislabel those whole-file differences as newline-only. The C30 member store differs by 45,914 newline bytes; control and wiring stores are byte-identical.

The 24 Checkpoint 01 failures remain FAILED: 11 strict-exit expectations, 12 prefix checks, one C30 whole-store check. The old 525-test suite (501 pass / 24 fail) was **not rerun** here. Fresh runs of the unchanged original checkers returned:

| Check | Current outcome |
|---|---|
| Original parser self-test | Exit **0**, 14 assertions |
| Original `--check --strict` | Exit **1**, not historical expected exit 2 |
| Original inventory profile with `--index` | Exit **1**, not a profile pass |
| New supplemental diagnostic | Exit **2**, `ORIGINAL_SNAPSHOT_NOT_REPRODUCIBLE_IN_CURRENT_CHECKOUT`; explanatory incompatibility, **not** an original gate pass |

The profile has 34 distinct conditions: 15 accepted Office source/test changes, two authorized 04A.02 infrastructure edits, 14 newline-only audit-tool/raw-store changes, three frozen Git/worktree/raw-prefix conditions. The accepted Office update changes genuine Help/navigation behavior and tests, not merely metadata. The original strict analyzer inventories Windows/Core/Common; its census remains 171 compiled Windows files / 3,866 members / 268 exports / 84 Ribbon entries.

No old checker, expectation, signoff, input record or byte store changed. A separately scoped future branch-aware verifier is recommended; the new diagnostic is only an explanation. The owner-accepted 04A.01 inventory and its 1,514 restricted obligations remain in force. Gate C2 UNMET, E1–E7 OPEN, GC3–GC8 DEFERRED and all 802 live-capture restrictions remain.

## Fixture and isolation changes

Canonical integer tokens now retain exact **Int64** values; larger integers are rejected instead of rounded through Double. Decimal/exponent tokens retain binary64 normalization (`1.0` can become `1`): this is not an arbitrary-precision decimal or raw-token serializer. Provenance RowIds require exact **Int32 JSON integer tokens**, rejecting fraction/exponent aliases, null and out-of-range values. Future wider IDs/exact-decimal semantics need a separately reviewed schema.

Both fixture files, both canonical dataset hashes, and all four invariant/sk-SK candidate JSON observations remain **byte-identical** to Checkpoint 01. Source pins match actual files. Expected numerical references remain null and NOT_APPROVED. RowIds remain outside production GroupedNumericData transport.

Two new projection tests check every actual service cell/type and table dimensions, header/footer/title flags, p-value addresses and columns. Both KW H/p pairs stay distinct. This tests serialization, not independent numerical correctness. ANOVA flags and Dunn are explicitly OFF; production options/services are unchanged.

The isolation helper checks the original verification-script SHA against preflight and requires **11 unique exact full-line anchors**. Missing/duplicate/altered anchors and added dotnet commands fail before output creation. All six managed build/test commands require `--no-restore`. Debug/Release, execution names and output paths are constrained. Every cloned dependency/TestData file is hash-verified before execution; scratch corruption prevents launching tests. The actual `scripts/verify-beshstatng.ps1` is unchanged.

New managed identities: **12** (seven RowId admission rows, one Int64 case, two unchanged-hash rows, two complete projection rows). Focused fixture suite: **41**, including the previous 29. New Python identities: **16** (nine added isolation tests and seven new diagnostic tests). Isolation suite totals 13; diagnostic suite totals seven.

## Scientific source proposals — NOT EXECUTED

The [scientific release proposal](04A02/checkpoint02-scientific-release-proposal.md) and [source manifest](04A02/oracle-proposals/manifest-v1.json) pin the two R scripts, inputs, settings, SHA-256 values and candidate R/stats version **4.5.1**. This is not an assertion that R is installed. No R process, parse/version lookup, external calculation, reference output, download or installation ran.

All separate gates remain **NO**: run independent reference, promote numerical golden, native workbook capture. Executable/runtime SHA fields remain null. Script hash fields are future operator attestations after external verification, not automatic approval. R syntax/runtime, JSON writer and exclusive-output behavior remain source-only proposals requiring a separately authorized controlled first run. Existing NIST/nonparametric tests provide context, not validation of these exact new datasets. Table/native captures remain deferred.

## Actual tests and interim failures

| Suite | Debug | Release | Final failures/skips |
|---|---:|---:|---|
| Core | 1,024 | 1,024 | 0 / 0 |
| Common | 783 | 783 | 0 / 0 |
| OfficeAddin managed | 505 | 505 | 0 / 0 |
| Independent WasmSmoke execution | 45 | 45 | 0 / 0 |
| Legacy .NET Framework 4.8 | 790 | 790 | 0 / 0 |
| Distinct managed total | **3,147** | **3,147** | 0 / 0 |
| Python: 27 tooling + 16 Help + 25 scripts | **68** | **68** | 0 / 0 |
| Node: 468 Office + 71 Help + 377 WasmSmoke | **916** | **916** | 0 / 0 |

Both broader wrapper invocations returned **0**. Builds were sequential and used VS2026 for legacy, cached dependencies and no implicit restore. Common/legacy executed copied assemblies with all dependencies/TestData; JSON/CSV/TRX output stayed under the new run. TRX redirection alone was not used as isolation. Office/WASM automation does not establish native Excel behavior. Legacy's existing non-public helper discovery advisory remains; no test was skipped.

All distinct managed identities/counters/TRX locations are in `managed-final-identities.json`; all managed attempts are in `test-identities-checkpoint02-complete.json`. Python/Node identities are in `python-node-final-identities.json`. Debug uses `Debug/c02-full-Debug/results/{Core,OfficeAddin,OfficeExecution}.trx` and `executions/c02-full-Debug-{Common,Legacy}/results/{Common,Legacy}.trx`; Release substitutes `Release`.

Retained interim failures: the initial isolation copytree mock did not accept recursive positional arguments (two test errors); correcting its signature produced 13 passes. An initial diagnostic scratch input placed CRLF outside raw objects, so its expected raw-object mismatch correctly failed; corrected multiline scratch objects produced seven passes. No production test, tolerance, fixture or original audit expectation was weakened. A later missing-local-source guard was covered by a fresh seven-test diagnostic rerun after Debug; repetitions do not add distinct identities.

Two source-check attempts ran before post-build restoration and stopped on generated VersionAttributes. Investigation disproved the encoding-only assumption: the unchanged MSBuild target also changed BuildDate from 2026-10-25 to 2026-10-30. The four other version/import lines were identical. Generated bytes and exact diff were retained, then the saved source was restored byte-for-byte. The stable diagnostic subsequently verified all 554 product files. Interim `NEW_PRODUCT_REGRESSION_DETECTED` records remain retained; the evidence identifies an expected build-metadata transient, not a new statistical/API regression. No historical checker input was restored or changed.

## Exact command families

Run from repository root. `$E` denotes the evidence root above; `$P` is exactly `C:\Users\peter\Desktop\WPy64-31470\python\python.exe`. Existing execution names cannot be replayed; future runs need new names/root and protection snapshot.

```powershell
dotnet build BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Debug --no-restore -v minimal
& $P -B scripts/tests/test_04a02_isolation.py
& $P -B scripts/tests/test_04a02_audit_compatibility.py
& $P -B scripts/verify-04a02-fixtures.py common --evidence $E --configuration Debug --name c02-focused-Debug --filter FullyQualifiedName~MigrationFixture04A02Tests
& $P -B scripts/verify-04a02-fixtures.py prepare --evidence $E --configuration Debug --name c02-full-Debug
powershell -NoProfile -ExecutionPolicy Bypass -File "$E/Debug/c02-full-Debug/verify-isolated.ps1" -Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe $P -Configuration Debug
& $P -B scripts/verify-04a02-fixtures.py prepare --evidence $E --configuration Release --name c02-full-Release
powershell -NoProfile -ExecutionPolicy Bypass -File "$E/Release/c02-full-Release/verify-isolated.ps1" -Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe $P -Configuration Release
& $P -B scripts/04a02/audit_compatibility.py --evidence $E --name audit-compatibility-diagnostic-restored
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --self-test
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check --strict
& $P -B scripts/windows-migration-audit/owner_closeout.py --profile inventory-complete-with-approved-deferrals --index
& $P -B scripts/verify-04a02-fixtures.py summarize --evidence $E --name checkpoint02-complete
& $P -B scripts/verify-04a02-fixtures.py preservation --evidence $E --name final
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --exit-code
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
```

Original checker subprocesses used process-local Git safe.directory configuration; no global Git config changed. Exact current checker commands, elapsed time, exit codes and logs are in `current-historical-checker-runs.json`. Copied wrapper source/copy hashes and complete transformation diffs accompany each script. Focused results and every interim failure have distinct retained log names.

## Protection and exact changed paths

Final start-to-end protection passed **131,250 historical paths**: membership, size, creation/write ns and file identity. All **2,172** immutable tracked-source SHA-256 values and **611** original ledger/reference hashes match. The prior complete **128,309**-entry hash seal was reused only for exact initial metadata/file-identity matches; all matched. Eleven protected writer outputs were freshly hashed. Current member/control/wiring stores also match their diagnostic hashes. This is not a new full 100-GB byte scan or protection rebaseline.

Free disk at final preservation: **503,761,616,896 bytes**, above warning/review levels; publication measures it again in `validation-summary.json`. Retention R continues through Phase 04A, including all new evidence. No deletion, relocation, archive, compression, historical overwrite or manifest rebaseline occurred.

Checkpoint 02 modifies five pre-existing Checkpoint 01 paths:

```text
.gitignore
BESHStat.Excel.Common.Tests/src/MigrationFixture04A02Support.vb
BESHStat.Excel.Common.Tests/src/MigrationFixture04A02Tests.vb
scripts/verify-04a02-fixtures.py
scripts/tests/test_04a02_isolation.py
```

It adds eight paths:

```text
scripts/04a02/audit_compatibility.py
scripts/tests/test_04a02_audit_compatibility.py
docs/architecture/04A02/checkpoint02-audit-reconciliation.md
docs/architecture/04A02/checkpoint02-scientific-release-proposal.md
docs/architecture/04A02/oracle-proposals/anova-proposed.R
docs/architecture/04A02/oracle-proposals/kw-proposed.R
docs/architecture/04A02/oracle-proposals/manifest-v1.json
docs/architecture/batch-04A-02-checkpoint02-review.md
```

Combined worktree: **22 authorized paths**, two tracked modifications and twenty untracked files; **index empty**. The other nine original Checkpoint 01 files, including both fixture JSON records and its report/decision/coverage/crosswalk documents, remain byte-identical. Exact final per-file SHA-256 values and Git status are in `final-git-and-source-checks.json`; original fourteen hashes are in `start.json`. The review report's own hash is recorded in the sidecar, avoiding a circular self-hash.

No shipping Core/Common/Windows/Office source, algorithm, API, framework target, original audit script/store, C30 proposal, old golden or signed decision changed. Manual smoke proposals only, **NOT RUN**: after exact-ID release and separate native authority, compare classical ANOVA table/options in a disposable workbook; capture the planned PCA chart series/axes/geometry and option state. The existing crosswalk retains 141 UibyID, two Ui82x2 and 75 Ui11PCA flagged controls plus shared UNKNOWN_HIGH RefEdit; none is released.

**Owner decisions requested separately:** review the audit reproducibility limitation and checkpoint; approve/decline each exact R method/version/executable/runtime/script SHA/tail/tolerance/edge policy before reference execution; decide golden promotion only after reviewing independent candidates; authorize native capture only after applicable exact-ID mitigation/regression/release. All three release booleans remain NO. No R/reference execution, authenticated Office/Mac/native Excel/COM/XLL loading/WinForms/RefEdit/chart capture, package installation, signing, installer, deployment, staging, commit, push or merge ran.

**STOP: 04A.02 CHECKPOINT 02 COMPLETE / OWNER REVIEW REQUIRED; official 04A.02 NOT ACCEPTED; original historical audit failures not rewritten; independent reference execution/golden promotion/native capture still require separate explicit approval.**
