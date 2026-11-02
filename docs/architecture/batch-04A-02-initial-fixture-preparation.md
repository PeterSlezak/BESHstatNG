# Official 04A.02 — initial fixture preparation / Checkpoint 01

**04A.02 PARTIAL / OWNER REVIEW REQUIRED.** Initial fixture implementation and
permitted validation are complete with disclosed historical audit failures. Official
04A.02 is NOT ACCEPTED. No authority to begin 04A.03 follows from this checkpoint.

The live clean starting main and reused `codex/04A-02` HEAD are
`12e25924aae93f0b36ab33937176c86c19641b50`. Its direct parent is inventory commit
`ec5e47a0cfc352ea5fa9fcd598e1f10b5b8fbd9b`. `merge-base --is-ancestor ec5e47a main`
returned 0. The later commit independently changes Office code/tests/Help links.
This proves both changes are included in main; the linear history does not prove
two explicit merge commits. The owner's later acceptance statement is current
governance authority. See [status reconciliation](04A02/current-owner-status.md).
The old unsigned records and Master Plan 2.3 were preserved byte-for-byte.

The new evidence root is
`TestResults/04A-02-initial-fixtures-20261030T131010Z/`. `start.json` records Git
identifiers, clean initial status, empty index, source SHA-256 pins and free disk.
`historical-metadata.json` freezes 129,722 pre-existing historical files before
test execution. Initial free disk was 504,598,622,208 bytes. No ZIP was extracted.

The only datasets added are `C30-SYN-ANOVA` and `C30-SYN-KW`, schema
`04A02-typed-preparation-v1`, in `BESHStat.Excel.Common.Tests/TestData/04A02/`.
Both preserve exact C30 values, labels A/B/C and RowIds
`[101,103,107]/[109,113,127]/[131,137,139]`. Alpha is 0.05, seed N/A; ANOVA is
classical with all optional flags OFF; KW explicitly disables the otherwise-ON
Dunn default. RowIds are independently validated provenance only, not transported
by `GroupedNumericData`. Scientific admission is PENDING_ORACLE_APPROVAL,
reference status NOT_APPROVED and expected numerical reference null.

Canonical dataset SHA-256:

| Case | SHA-256 |
|---|---|
| C30-SYN-ANOVA | `98dce529f444e6ede2f5f8e34ef072cd7407efd2a3a25597d2d1504c6b4f6cba` |
| C30-SYN-KW | `7e156caca9ded21a8e0967d5755ec40143a11542b2094cc03ebe7474400e120e` |

The test-only support uses existing System.Text.Json, ordinal object-key ordering,
array order, invariant UTF-8 and finite-number admission. It checks source file
hashes and exact values against the frozen C30 proposal. It contains no approved
snapshot update mode. Negative tests cover duplicate cases, RowId lengths, input
infinity, missing labels, invalid/unapproved alpha, absent reference metadata,
false approval/native status, source/data hash drift, unapproved options/methods
and rehashed dataset drift. Round trips and invariant/sk-SK capture are checked.
See [schema and execution details](04A02/README.md).

Candidate observations use schema `04A02-candidate-observation-v1` and status
`UNVERIFIED_AGAINST_INDEPENDENT_ORACLE`. They preserve ordered tables, CLR cell
types/values, nulls, dimensions/header/footer/title and p-value cell/column
metadata. They are semantic observations, not native workbook goldens or a
complete approved Common-document fixture. Services leave the typed input and
fixture unchanged. These observed numbers are **not independent expected values**:

| Case | Candidate observation only |
|---|---|
| ANOVA | F `2.3333333333333335`, p `0.177978515625`; table 4 × 6 |
| KW | H `4.62222222222222`, p `0.09915102252982544`; corrected H `4.781609195402297`, corrected p `0.09155598849604307`; table 7 × 3 |

The [scientific decision sheet](04A02/scientific-decisions.md) proposes separately
pinned independent R/statistical conventions, field-specific tolerance candidates
and future degeneracy policies. No proposed choice is approved and no new R or
external numerical computation was run. Existing NIST/nonparametric references
are context only. Owner/domain-owner decisions are required before G2 promotion.

The [coverage register](04A02/coverage-register-v1.json) contains two current typed
cases and seven separate future case IDs for native ANOVA, normality, 2x2, PCA,
semantic tables, charts and form options. Each lists actual source/API, proposed
data/settings/reference, owner, prerequisites and downstream batch consumption.
Future cases remain NOT RUN. No reduced full-batch scope is inferred.

The [restriction crosswalk](04A02/native-restriction-crosswalk-v1.json) preserves
141 flagged UibyID, two Ui82x2 and 75 Ui11PCA control IDs and their linked deferred
obligations, plus the shared RefEdit xlApp UNKNOWN_HIGH edge. These are form-wide
source-backed lists, not assertions that each option exercises every control.
Every applicable exact ID requires mitigation, executed regression and explicit
owner release; disposable-workbook execution requires separate authority.
Source-predicted chart series/configuration is labeled as prediction; chart
bounds/geometry remain uncaptured. All 802 affected live-capture restrictions,
Gate C2 UNMET, E1–E7 OPEN and GC3–GC8 DEFERRED remain unchanged.

Validation uses `scripts/verify-04a02-fixtures.py` and hash-bound execution copies
of the unchanged live `scripts/verify-beshstatng.ps1`, following the accepted
04A.01 assembly-copy method. TRX redirection alone is not used as isolation.
Common binaries/dependencies/TestData are copied and hash-verified for every run;
four-parent internal JSON writers resolve below the new sandbox. Legacy binaries
and source TestData are copied/hash-verified and CSV exports separately routed.
All builds are sequential; no restore, package install or network build is run.

The final managed suites have these distinct counts in each configuration:

| Suite | Debug | Release | Final failures/skips |
|---|---:|---:|---|
| Core | 1,024 | 1,024 | 0 / 0 |
| Common | 771 | 771 | 0 / 0 |
| OfficeAddin managed | 505 | 505 | 0 / 0 |
| Independent WasmSmoke execution | 45 | 45 | 0 / 0 |
| Windows .NET Framework legacy | 790 | 790 | 0 / 0 |
| Managed distinct total | **3,135** | **3,135** | 0 / 0 |
| Python (27 Windows tooling + 16 Help + 9 scripts) | **52** | **52** | 0 / 0 |
| Node (468 Office + 71 Help + 377 WasmSmoke) | **916** | **916** | 0 / 0 |

There are **29 new managed identities**: two fixture/provenance rows, four
service/culture rows, one duplicate-set test, 18 invalid-admission rows, one KW
default rejection, two rehashed-drift rows and one serialization test. Four new
Python tests protect the isolation root, required snapshot, exclusive output
creation and internal writer routing. Debug's four new Python tests ran separately
after its original 48-test script run; Release discovered all 52 together.

The initial focused suite passed 25 before four additional admission tests were
added. Final focused Debug passed 29; final complete Common Debug and Release
each passed 771. Repetitions are not added to distinct totals. The first Debug
validation was interrupted before native/legacy activity to extend legacy export
isolation. The second Debug script completed Core/Common/Python/Office/WASM/Node,
then failed 70 legacy tests solely because source TestData was not yet copied.
Its Common assembly contained the earlier 25 new cases (767 total). After fixing
the helper, the complete legacy Debug rerun passed 790; rebuilding and rerunning
the complete final Common suite passed 771. The second Debug wrapper therefore
retains its actual exit 1; final green Debug evidence combines the unaffected
suite results with those two replacement runs. Release's complete wrapper returned
0. No failing test or tolerance was weakened. The legacy non-public helper-class
discovery advisory remains; no tests were reported skipped.

Exact command families (run at repository root; `$E` below denotes
`TestResults/04A-02-initial-fixtures-20261030T131010Z`, `$P` denotes
`C:\Users\peter\Desktop\WPy64-31470\python\python.exe`):

```powershell
dotnet build BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Debug --no-restore -v minimal
& $P -B scripts/verify-04a02-fixtures.py common --evidence $E --name focused-Debug --filter FullyQualifiedName~MigrationFixture04A02Tests
& $P -B scripts/verify-04a02-fixtures.py prepare --evidence $E --configuration Debug --name full-Debug-02
powershell -NoProfile -ExecutionPolicy Bypass -File "$E/Debug/full-Debug-02/verify-isolated.ps1" -Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe $P -Configuration Debug
& $P -B scripts/verify-04a02-fixtures.py legacy --evidence $E --configuration Debug --name legacy-Debug-data-fixed --vstest 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe'
dotnet build BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Debug --no-restore -v minimal
& $P -B scripts/verify-04a02-fixtures.py common --evidence $E --name final-Debug-Common
& $P -B scripts/verify-04a02-fixtures.py common --evidence $E --name focused-final-Debug --filter FullyQualifiedName~MigrationFixture04A02Tests
& $P -B scripts/tests/test_04a02_isolation.py
& $P -B scripts/verify-04a02-fixtures.py prepare --evidence $E --configuration Release --name full-Release
powershell -NoProfile -ExecutionPolicy Bypass -File "$E/Release/full-Release/verify-isolated.ps1" -Legacy -Wasm -Js -Python -VisualStudio 2026 -PythonExe $P -Configuration Release
& $P -B scripts/verify-04a02-fixtures.py summarize --evidence $E --name complete-managed
& $P -B scripts/verify-04a02-fixtures.py preservation --evidence $E --name post-managed
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --self-test
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check --strict
```

Each generated validation script has its original source hash, copy hash and
complete routing/offline/isolation diff next to it. The original script was not
directly executed because its fixed TRX paths and assembly-relative writers would
overwrite protected evidence. No historical closeout writer was rerun. Legacy
build-generated VersionAttributes bytes were retained as new evidence and the
original product-source bytes restored after each configuration; no historical
timestamp restoration or protection rebaseline occurred.

Exact managed identities and final TRX paths are in
`$E/final-managed-identities.json`; all attempts including failures/repetitions are
in `$E/test-identities-complete-managed.json`. Final Debug uses
`Debug/full-Debug-02/results/{Core,OfficeAddin,OfficeExecution}.trx`,
`executions/final-Debug-Common/results/Common.trx`, and
`executions/legacy-Debug-data-fixed/results/Legacy.trx`, relative to `$E`.
Release uses `Release/full-Release/results/{Core,OfficeAddin,OfficeExecution}.trx`
and `executions/full-Release-{Common,Legacy}/results/{Common,Legacy}.trx`.
`python-node-identities-complete.json` records 52 Python and 916 distinct Node
identities per configuration. The earlier interim text parser missed multiline
names and misread console timing strings; that interim record is retained and
explicitly superseded by the complete identity record.

All 129,722 initial historical paths retained size, creation/write ns and file
identity at the post-managed check. The complete prior 128,309-entry SHA-256 seal
was reused only where initial metadata/file identity matched exactly; all entries
matched. Eleven protected JSON writers were freshly hashed and matched. All 611
frozen ledger/reference source files passed fresh SHA-256 verification. These
checks do not claim rereading approximately 100 GB of historical bytes. The
cross-source import guard passed and Core/Common remain netstandard2.0. The
post-managed free-space measurement was 504,340,180,992 bytes, above both suggested
warning/review levels. The final preservation check also passed all 129,722 files,
with **504,101,056,512 bytes free**. No historical deletion, relocation,
compression, overwrite, archive or protection rebaseline occurred.

Historical audit reruns are **not all green** and are separate from the fixture
and product suites:

| Historical suite/check | Actual outcome |
|---|---|
| `test_reconciliation.py` | 463 executed: 440 passed, 23 failed, 0 skipped |
| `test_continuation30.py` | 29 executed: 28 passed, 1 failed, 0 skipped |
| `test_owner_closeout.py` | 33 passed, 0 failed/skipped |
| Distinct historical audit tests | **525 executed: 501 passed, 24 failed** |
| Original parser self-test | Exit 0, 14 assertions passed |
| Original strict full-contract checker | **Exit 1**, expected historical incomplete-contract exit 2 was NOT obtained |
| Inventory profile with `--index` | **Exit 1**, NOT a current profile pass |

Commands for those audit runs, with process-local Git ownership configuration
(`GIT_CONFIG_COUNT=1`, `GIT_CONFIG_KEY_0=safe.directory`, value = repository root):

```powershell
& $P -B scripts/windows-migration-audit/test_reconciliation.py
& $P -B scripts/windows-migration-audit/test_continuation30.py
& $P -B scripts/windows-migration-audit/test_owner_closeout.py
& $P -B scripts/windows-migration-audit/owner_closeout.py --profile inventory-complete-with-approved-deferrals --index
& $P -B scripts/windows-migration-audit/test_reconciliation.py Continuation13Tests.test_13_prior_raw_crlf_bytes_preserved_and_newline_drift_rejected
& $P -B scripts/verify-04a02-fixtures.py preservation --evidence $E --name final
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --exit-code
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
```

The 23 reconciliation failures comprise 11 historical strict-exit expectations
and 12 raw-prefix byte checks. C30 adds one whole-literal-store byte check failure.
The focused C13 diagnostic repeated one failed identity and is not added to 525.
Exact identities and failures are in `$E/audit-identities.json`, command outcomes
in `$E/audit-checks.json`, and full assertion diagnostics in the retained logs.

The strict checker reports only `coverage.json` and `source-provenance.json` as
different. Those existing records still bind HEAD/main `11fa5d99...` and branch
`codex/04A-01`; the actual batch uses the separately accepted post-merge main.
Its reported Windows census remains 171 files, 3,866 members, 268 exports and 84
Ribbon entries. This exit 1 is disclosed, not relabeled as the desired exit 2.
The owner-profile check also binds the old branch/HEAD/worktree and byte snapshot:
it detects the accepted Office update, the authorized `.gitignore`/test-project
edits, and existing audit tooling/raw-store byte differences. It is not a reusable
current-branch pass without separately reviewed provenance handling.

Read-only diagnosis across **39 historical store/prefix comparisons** found zero
semantic changes and zero remaining differences after diagnostic-only CRLF/LF
normalization. For the C13 member prefix, all 1,312 objects agree semantically;
1,199 byte mismatches are entirely newline-related. The current tracked store
has LF, while old retained snapshots contain CRLF, consistent with the existing
Git `* text=auto eol=lf` policy. These are checkout/history limitations, not
new fixture writers: initial Git was clean, and final historical metadata and
ledger hashes remain unchanged. No actual file was normalized or restored from
old audit bytes; no checker was patched or failure suppressed. A separate owner
review must reconcile this post-merge audit provenance/newline issue. This does
not revoke the owner's later 04A.01 inventory acceptance or qualify its deferrals.

Modified paths are limited to `.gitignore` (explicit dossier allowlist), the Common
test project fixture-copy rule, exactly two fixture JSON files, two VB test-only
files, the new isolation helper and its Python tests, five `docs/architecture/04A02/`
dossier files, and this report. No product API, algorithm, UI, framework, Office
source, old golden, plan, sealed audit or checker is changed. Nothing is staged,
committed, pushed, merged, installed, signed or deployed.

Exact changed/untracked paths (two tracked edits and twelve new files; index empty):

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

Manual smoke proposals, **NOT RUN**: after exact-ID release and native authority,
run classical ANOVA in a disposable workbook and compare typed/native tables plus
option state; run the proposed PCA dataset and record axes, data/labels, series,
chart geometry and option state. Existing automated WASM/Office tests establish
managed/JS contracts only, not Excel UI behavior or browser/native acceptance.

**STOP: 04A.02 initial implementation/fixture preparation COMPLETE; official
04A.02 NOT ACCEPTED; further reference capture awaiting decisions.** Required
next decisions are independent review of this checkpoint and its disclosed audit
limitations; domain-owner approval of each oracle/method/version/tolerance/edge
policy; and, separately, exact-ID mitigation/regression/release plus authorization
before any native disposable-workbook activity. No native Excel/COM/XLL loading,
WinForms/RefEdit run, browser/authenticated Office/Mac acceptance, new external/R
golden capture, installer, signing, deployment, or 04A.03 was run.
