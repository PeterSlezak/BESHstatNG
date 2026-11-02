# Batch 04A.03 owner review

**04A.03 PARTIAL — EXACT BLOCKERS:** production extraction and managed parity are
implemented; four frozen native-source provenance checks need explicit owner
disposition before full batch acceptance. This is the single consolidated review
package, not owner acceptance or native release.

Owner review package on `codex/04A-03`. Starting HEAD and local `main`:
`9e2326d4b562cc94660745f1453b231df938bff0`; cached `origin/main` agrees, without fetch.
The owner created the branch after sandbox denial of `.git/index.lock` and instructed continuation.
Initial working tree/index were clean. The 04A.02 implementation is in this commit;
the acceptance receipt, accepted amendment and later publication manifests remain ignored,
local-only authorities, not blobs in `main`. This is a publication limitation, not a claim
that the receipt was committed. The local accepted amendment SHA-256 is
`8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307`.
Initial available disk space: 543,361,290,240 bytes (about 506 GiB).

## Ownership established before implementation

| Existing behavior/call sites | Decision and concrete implementation paths | Defaults, ordering, diagnostics |
|---|---|---|
| `UibyID.checkInputs`: first/second input roles | Move pure role choice to Common `UnivariateSelectionPolicy`; Windows still checks/resets RefEdit and output destination | First input required and single-column for two-sample or by-ID; second always required; wide multi-column otherwise. Same Boolean truth table |
| `getDataMultipleGroups`, wide; normality/descriptive/ANOVA/KW | Common selection of already imported columns; pure Windows `WindowsUnivariateInputAdapter`; actual form delegates | Keep per-column `ImportInto(..., True)` and checkpoints; independently omit `FinalData Is Nothing`; preserve selected column order, `varNames`, unequal n, `No valid data` |
| `getDataMultipleGroups`, by-ID; same analyses | Common composition over existing `DataObj` grouping; Windows retains address concatenation/import and worksheet comparison | Keep `ImportInto(..., True, 0)`, `Object.Equals` category identity and first observation order; exact existing labels, including whitespace/type distinctions; no new native rejection |
| Typed raw snapshot roles/validation | Common service composes `ConfigurableGroupedNumericParser`; focused Common tests | Explicit orientation/header/cell policy; separate group-count constraints; existing codes/coordinates retained; no implicit Windows default migration |
| `DataObj.RowIds` | Separate immutable observation provenance; never add RowIds to `GroupedNumericData` | Physical rows come from `LoadCoreDataTable`/`RemoveMissing`, not grouped output; preserve gaps and group membership |
| `getData2Groups` | Keep acquisition and two-group behavior in Windows for this batch | Existing MW/t/ROC count diagnostic/late indexing behavior needs separate compatibility decision; pure input-role truth table shared |
| `getGroupedContinuousData...`, `ResolveGroupedColumnName`, unique plot labels | Remain Windows, unchanged | Chart-specific numeric-text header correction and label deduplication are outside statistical selector extraction |
| `BindPilotLifecycle`, `EnsurePilotAcquisition`, option event handlers, compute dispatcher, report builders/writers | Remain Windows, unchanged | Preserve frozen addresses, cancel, document switch guards, RefEdit leases, UI state, output/error presentation; no 04A.04–07 work |

Implemented source/test paths: Common `src/Input/UnivariateSelectionPolicy.vb` and
`ConfigurableGroupedNumericParser.vb`; Windows `src/DM/WindowsUnivariateInputAdapter.vb`,
`src/UI/UibyID.vb`, project compile entry; Common and Windows focused test files and
Windows test compile entry; an isolated local validation driver under `scripts/`.
No numerical algorithms, sealed fixtures, framework targets, Ribbon/UDF exports or designer changes.

## Native pre-change disposition

The accepted amendment's `case_transfers` and exact `obligation_transfers` were inspected
for both `04A02-ANOVA-NATIVE-01` (primary 04A.07) and
`04A02-NORMALITY-NATIVE-01` (primary 04A.06), earliest consumer 04A.03.
Both share the same 141 controls / 160 obligations: do not sum these populations.
The affected selection IDs are `control:BESHStatNG.UibyID.btCompute`, `.RefEdit1`,
`.RefEdit2`, `.optByID`, `.optByColumn`. Their `capture_authorized` remains false.
Their gate requires evidence, exact mitigation/regression, independent risk review and
N authority, or a fresh exact-case/criterion/ID-bound owner deferral **before a native
behavior change or reliance on the missing native golden**.

Only equivalent policy moves were implemented: existing acquisition and cleaning,
grouping algorithm, group/name order, accepted/error results and option state stay intact.
New strict typed-snapshot validation is not silently substituted for the permissive legacy
importer. No native golden is relied upon. Host-free characterization/parity is evidence
of managed equivalence only, not N1 safety, original-obligation discharge or native release.
Any newly detected incompatibility will remain unchanged and be recorded here.

Gate C2 remains UNMET; P and N1/N2 are not released; 04A.04 is not started.

The [exact source/obligation crosswalk](04A03/pre-change-crosswalk.json) binds the five
touched IDs to their original CSV row hashes, declaration IDs, P0 risk bases,
mitigation requirements, ledger digest and accepted pre-change gate. All five CSV
and ledger rows and both complete historical file hashes were verified without edits.
Both case populations remain shared, OPEN and capture-unauthorized. The new source
checks compare all form methods to Git commit `9e2326d`: only `checkInputs` and
`getDataMultipleGroups` changed. The dispatcher, lifecycle, cancel, option handlers,
reports, plot-specific preparation and two-sample importer remain identical.

This honors the prompt's explicit permission for demonstrated behavior-preserving
extraction; it does not interpret C10, the earlier conditional GUI approval, C14 or
the owner-created branch as N1 release. N14-HWND/SOURCE/OBSERVATION and independent
Windows/COM risk review remain missing. No affected native behavior was newly
qualified or discharged; future behavioral corrections still need exact-case,
criterion and ID-bound authority or the full baseline/mitigation/regression/risk/N gate.

## Production APIs and actual wiring

```vb
UnivariateSelectionPolicy.InputRoles(twoSample As Boolean, byId As Boolean) As UnivariateInputRoles
UnivariateSelectionPolicy.Parse(data As SpreadsheetRangeData,
    options As SpreadsheetGroupedParseOptions,
    Optional groupIdentifiers As SpreadsheetRangeData = Nothing,
    Optional physicalRowIds As IReadOnlyList(Of Integer) = Nothing,
    Optional groupPhysicalRowIds As IReadOnlyList(Of Integer) = Nothing) As UnivariateSelectionResult
UnivariateSelectionPolicy.CaptureImportedColumn(column As DataObj) As UnivariateImportedColumn
UnivariateSelectionPolicy.SelectCapturedColumns(columns As IReadOnlyList(Of UnivariateImportedColumn)) As UnivariateSelectionResult
UnivariateSelectionPolicy.SelectImportedColumns(columns As IReadOnlyList(Of DataObj)) As UnivariateSelectionResult
UnivariateSelectionPolicy.SelectImportedById(imported As DataObj) As UnivariateSelectionResult
```

`UnivariateSelectionResult` carries `Data`, `Validation`, `Sources`, `SourceErrors`
and `Success`. Immutable provenance is separate from the unchanged
`GroupedNumericData` constructor/properties. Error metadata uses the existing
immutable `SpreadsheetFrameCell`; no additional error serialization is invented.
`SpreadsheetGroupedParseOptions.MinimumGroups` (default 2) and `MaximumGroups`
(default Int32.MaxValue) are additive options; the existing parser now captures
`ObservationSources` while accepting numeric cells, with unchanged default
validation text/codes and numerical results. These are library APIs, not a changed
Office wire schema. Existing serializers, statistical APIs and framework targets
are unchanged; there are no new packages.

`UibyID.checkInputs` calls the Common role selector and passes its single-column
requirement to existing `CheckRefEdit`. The multigroup wide loop still imports each
column with `ImportInto(..., True)` and immediately calls
`WindowsUnivariateInputAdapter.CaptureColumn`. This preserves conversion and
exception timing and avoids retaining all raw importer matrices. The form calls
`SelectCapturedColumns` after acquisition; the by-ID branch retains its exact
worksheet check/address concatenation/`ImportInto(..., True, 0)` and calls
`SelectById`. The bridge delegates selection to Common and converts only the
existing `X`/`varNames` result. It preserves an already populated worksheet error.

All four acceptance targets use this real selector through the unchanged compute
dispatcher: normality, descriptive, one-way ANOVA and KW, each in by-ID and
by-column mode. There is no source-only unused facade, runtime feature flag,
alternative UI, write capability or host acquisition callback in Common.
Common composes `ConfigurableGroupedNumericParser` for raw typed snapshots and
reuses `DataObj.DataByID2ByColumn` for established imported grouping; no new numeric
grammar or statistical/grouping algorithm is substituted.

## Role and validation contract

| Choice/condition | Explicit typed snapshot API | Existing Windows imported route |
|---|---|---|
| Wide / by column | Column order preserved; independently missing observations | Same selected-column order and independent cleaning; `FinalData Is Nothing` columns omitted |
| Long / by ID | Explicit category/value indices, or two aligned single-column ranges; ordinal first-observed group order | Same joined import, first-observed cleaned ID order; `CharCols=0` normalizes identifiers to current-culture strings before grouping |
| Rows | Existing parser's explicit `GroupsInRows`; no new form option | Unsupported in current form; no UI expansion |
| Header | Required/None/Auto explicitly chosen; ambiguous inference diagnosed; strict text `"0"` is a label | Existing first-cell `varNames` acquisition and cleaning retained; numeric-looking text remains an observation in the statistical wide path |
| Numeric text | Rejected by default; explicit invariant/culture policy uses existing Float parsing | Existing current-culture DataObj conversion unchanged |
| Boolean/date | Boolean coercion opt-in; format hint alone never creates a date type | Existing `.Value` acquisition and cleaning retained; boolean/DateTime data are not silently reinterpreted |
| Blank/error/text | Existing explicit missing/error policy; Excel errors retained losslessly in `SourceErrors`; text errors give structured diagnostics | Existing Windows normalization and DataObj filtering; lost native error identities cannot be recovered after import |
| Group count/n | Explicit `MinimumGroups`, `MaximumGroups`, `MinimumObservationsPerGroup`; one group allowed for univariate use, exactly two expressible | Existing downstream normality/descriptive/ANOVA/KW validation retained; no new early rejection or statistical change |
| Unequal n/labels | Unequal lengths allowed; wide duplicate labels retained; typed long-label collisions rejected by existing parser | Unequal n, exact legacy labels, whitespace/case and duplicate column labels preserved |
| Identity/alignment | Captured workbook/sheet identities, dimensions/start rows and optional matching physical row lists checked; no active-host lookup | Existing Windows workbook/lifecycle and worksheet checks unchanged; no new COM reads |
| Invalid selection/options | Deterministic report; `Data = Nothing`, no accepted-observation sources | Existing `No valid data` and worksheet diagnostics preserved; empty by-ID import retains its existing exception route |

Raw sources map each accepted observation by group index and observation index,
selected row/column and physical worksheet row/column, with captured workbook and
worksheet identity/address. Noncontiguous physical rows must be positive, strictly
increasing and match snapshot length; separately supplied group rows must match.
No rows are fabricated by zipping independently filtered data and identifiers.
The flat source list follows parser visitation order; join using its explicit
group/observation indices, rather than assuming every long-format list is grouped.

`DataObj.LoadCoreDataTable` calls `RemoveMissing`, which assigns the physical source
row to `RowIds(count - 1)` for retained rows. That is the API preserving source-row
identity. The new imported captures map those IDs before the original object can
be discarded. The legacy importer does not capture workbook IDs or physical
source columns; these remain empty/zero in its provenance, rather than guessed.
Its source address and sheet name remain available. The form's old Windows result
container still transports only values/names; provenance is returned by Common
and proven through the adapter seam, not falsely claimed present in that container.

## Fixtures, before/after observations and limitations

The before source is frozen from Git `9e2326d` under the new local evidence root.
`UnivariateSelectionAdapterTests.BeforeColumns`/`BeforeById` characterize its exact
post-import selection; these are test-only oracles, not parallel production parsers
or independent scientific references. Tests cover all eight target/mode routes,
missing columns/rows, one/single-observation groups, exact diagnostics, captured
immutability, culture and ordering. `UnivariateSelectionSourceTests` binds the
tested adapter to the real form, verifies the acquisition/conversion checkpoints,
and confirms invalid-input exits precede all four analysis dispatches. Existing
RefEdit lifetime tests are reused; no fake test claims native COM safety.

Representative observations, unchanged before/after:

| Input | Observed outcome |
|---|---|
| Wide B=[8, blank, 9], wholly missing middle column, A=[1,2,3] | B then A; values [8,9] / [1,2,3]; original rows [21,23] for B |
| Long B=8, A=1, B=blank, B=9, A=2 | B then A, independent n=2/2; B original rows [21,24] |
| No usable wide column | `Nothing` result, exact `No valid data`; structured code `groups.no-valid-data`; no analysis dispatch |
| Existing different-sheet diagnostic | Exact original misspelled message retained through conversion; dispatcher exits before writing |
| Wide first text `"0"`, then 1 and 2 | Legacy values [0,1,2]; explicit strict-header API instead treats `"0"` as a label by its requested policy |
| Long IDs `" A "`, `"A"`, `"a"`, numeric 1 and text `"1"` | Importer produces four groups; final numeric/text identifiers both normalize to `"1"` before selection |
| Typed `#N/A` at selected row 2, physical column 4 | `cell.excel-error`, original native code retained immutably; category errors map to their own group-range column |
| Typed missing value with row IDs 101,103,107,109 | Matching values/IDs remain aligned; skipped row 107 never appears as a retained observation |

The two existing C30 fixtures are reused without edits. New tests preserve exact
labels/values and each physical RowId, compare primary services before/after, and
compare every serialized metric/table/CLR/null/p-value annotation with hash-bound
C14 **candidate** observations in invariant and sk-SK cultures. No R process runs,
no new tolerances or expected statistical values are generated, and no P promotion
is implied. ANOVA's retained F=2.3333333333333335 and p=0.177978515625 remain
candidate evidence, not newly approved scientific goldens.

Disclosed alignment/correction candidates have executable fixtures and are **not
silently repaired**: UDF group headers trim whitespace while GUI labels preserve
it (`GuiUdfAlignmentCandidate_HeaderTrimming_IsExecutableAndUnchanged`); the
legacy importer admits NaN/infinity while the raw policy rejects them
(`LegacyNonFiniteCompatibility...`); an empty by-ID import takes its pre-existing
NullReference/error-presentation route (`EmptyByIdCompatibility...`). The requested
behavior-preserving extraction leaves these native semantics intact. Replacing
them with the raw policy's stricter errors, changing header inference or label
normalization would require an explicit contract/correctness decision and exact
native gate disposition for the five selection IDs above. This is not a blocker
to the equivalent selection extraction and is not represented as a fixed defect.

## Validation and evidence

All new evidence is under `TestResults/04A-03-20261102-run01/`, excluded from Git.
The validation driver reuses the accepted assembly-copy isolation helper; copied
Common and Windows assemblies/data live deeply enough that test-internal relative
writers stay within the new sandbox. TRX redirection alone is not relied upon.
Frozen 04A.02 tests' optional candidate writer is disabled because it only admits
04A.02 root names; new 04A.03 fixture tests write equivalent candidates exclusively
to the new sandbox using their own checked environment variable. No historical
test/assertion is edited or weakened.

The original `verify-beshstatng.ps1` is unchanged. Its hash-bound, diff-recorded
execution copy redirects results, uses `--no-restore`, isolates assemblies, and
continues independent suites after nonpasses to report their actual outcomes.
Windows production `PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory` plus the legacy test Build
compile the actual changed host without BeforeBuild metadata generators or
AfterBuild packaging/signing. This is a managed legacy compilation/test, not an
XLL packaging/installation test. Core/Common retain netstandard2.0; Windows retains
.NET Framework 4.8 and both Excel-DNA packaging definitions unchanged.

The separate semantic capture executes the unchanged
`MigrationSemantic04A02Tests` (15 repeated tests) in its own copied assembly with
the original optional writer enabled. That writer admits the new scratch path
without changing its assertions and creates the four `C05-G3-*` observations used
by existing Python contract tests. The full Common suite's C02-specific writer
remains disabled; the new typed fixture tests write their four candidates using
`BESHSTAT_04A03_OBSERVATIONS`. Both sets are new execution evidence, not goldens.

### Commands and troubleshooting

All commands ran from the repository; no network or package install was used.
`PY` below abbreviates only the displayed executable path
`C:\Users\peter\Desktop\WPy64-31470\python\python.exe`.

```powershell
# Initial protection and exact-row verification
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py init --evidence TestResults/04A-03-20261102-run01
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py crosswalk --evidence TestResults/04A-03-20261102-run01

# Focused development: same command with names focused01 through focused05
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py focused --evidence TestResults/04A-03-20261102-run01 --configuration Debug --name focused05 --windows
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/tests/test_04a03_selection.py

# Final sequential verification (source script preserved; execution copies recorded)
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py final --evidence TestResults/04A-03-20261102-run01 --configuration Debug --name final-debug02
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py final --evidence TestResults/04A-03-20261102-run01 --configuration Release --name final-release

# Debug repairs: only affected legacy/resource and observation-dependent suites
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py execute --evidence TestResults/04A-03-20261102-run01 --configuration Debug --name final-debug-resource --windows
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py execute --evidence TestResults/04A-03-20261102-run01 --configuration Debug --name final-debug-resource-Legacy --legacy --vstest 'C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe'
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py semantic --evidence TestResults/04A-03-20261102-run01 --configuration Debug --name final-debug02
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py common-python --evidence TestResults/04A-03-20261102-run01 --configuration Debug --name final-debug-corrected --candidate-name final-debug02
```

Each final execution copy invokes PowerShell with `-Legacy -Wasm -Js -Python
-VisualStudio 2026 -PythonExe <the exact path above> -Configuration Debug|Release`.
The original script is **not executed unmodified**, because its shared TRX and
test-internal writers would violate retention. Its preserved source hash is
`e54e1fb0e7f3699f5d9aee361d255801531a546d8169edbab704ac0d8dd017fd`;
the execution-copy `.provenance.json` files contain the exact transformation and
hashes. Individual subprocess command arrays/exits are recorded in `*-command.json`
and `executions/*/execution.json`; logs and TRX remain local-only.

Focused failures were ordinary characterization/driver defects: a VB ParamArray
blank-row fixture was ambiguous; the new expected group count initially overlooked
existing `CharCols=0` string normalization; an isolation proxy initially omitted
`STDOUT`. Final driver repairs moved initialization after PowerShell's parameter
block, removed the obsolete original legacy invocation after replacing it,
enabled UTF-8 output, added managed resource preparation, and separated C05 semantic
observation capture from the C02 root-name-restricted optional writer. No old test
or sealed fixture changed. Earlier failures/results are retained as attempts,
not counted as additional distinct acceptance tests. The first malformed final
copy (`final-debug`) ran no managed suites; `final-debug02` completed portable,
Office/WASM and Node checks before its legacy handoff failed. Only affected legacy
and observation-dependent checks were repeated; no statistical source repair was needed.

### Nonpasses and exact acceptance blocker

The known historical three Python nonpasses remain:

- `test_04a02_checkpoint09.Native09Tests.test_unapplied_patch_routes_every_active_selection_binding_and_guards_workbook`
- `test_04a02_source_specs.SourceSpecifications.test_all_native_blockers_and_anchored_mappings`
- `test_investigation06.Investigation06Tests.test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof`

Eight additional Python nonpasses already exist in the verified starting Git
baseline. The C11 retained-startup fixture in both `main` and the untouched working
file has LF SHA-256 `f2d35a348c993d8f4a892de42343959246a19bacbbef34587aa71401a562c290`;
the accepted C11/C14 records pin its CRLF bytes,
`1296920a22a1f3778aaa54478a605989004f82865d18454e9fc1f8b5b2f46b5f`.
Normalizing the Git blob to CRLF reproduces that expected hash. This blocks two
C11 tests, two C12 tests, three C13 tests and one C14 retained-reference-reader test.
It is a baseline publication/byte-preservation discrepancy, not a new R/runtime
or statistical failure. The fixture and accepted pins were not rewritten.

Four **new** Python source-provenance nonpasses arise from the intended form
refactor, and are not labeled pre-existing:

- `test_capture_contract04a02.CaptureContracts.test_all_239_ledger_rows_identified_with_original_digest_convention`
- `test_capture_contract04a02.CaptureContracts.test_complete_source_crosswalk_equality`
- `test_capture_contract04a02.CaptureContracts.test_packet_retains_additional_21_deferred_obligations`
- `test_investigation06.Investigation06Tests.test_all_239_exact_ids_have_reversible_root_cause_source_maps`

They enforce historical whole-form hashes and still require the original
`UibyID.vb` SHA-256 `11a87f3e5399b16bdc8229599ecb7f9b282a7a623ae3c568f38751a57862b6ca`,
which matches starting Git `main`. The changed form hash is
`123d181ba1e91b10509d994ac916d3ef0bb298a66f568e4188d4a201dc0671c0`.
The new eight source checks and five exact-row crosswalk checks pass, but do not
turn the four old whole-file checks green or release native restrictions.

**Smallest remaining acceptance action:** one owner review must explicitly
dispose of this historical source-qualification conflict for
`04A02-ANOVA-NATIVE-01` / `04A02-NORMALITY-NATIVE-01`, the five exact selection IDs
in the crosswalk, and the four named source checks. The new source/body-parity
crosswalk can be accepted as superseding managed extraction evidence while the
native criteria remain OPEN, or the owner can require additional qualification.
No general 04A.03 exemption, rewritten historical pin or silent N release is supplied.
The unrelated C11 LF/CRLF baseline discrepancy also remains for owner disposition.

## Owner manual acceptance smoke — NOT RUN

**OWNER MANUAL / NOT RUN.** No live Excel, COM/RefEdit UI Automation, saved workbook
capture, native chart rendering or N1/N2 pilot was launched. These instructions
must wait for the applicable native authorization and desktop execution.

1. Existing normality and descriptive dialogs: select two named numeric columns,
   each with at least eight observations and different missing rows. Choose
   by-column and the usual output destination. Expect the original group order,
   independent missing-value counts and existing tables/options. Repeat by-ID
   with aligned identifier/value columns. An all-invalid wide selection must show
   `No valid data` and create no results workbook/sheet.
2. Existing one-way ANOVA: use the unchanged C30 primary fixture values A=[1,2,3],
   B=[2,4,6], C=[3,5,7], with Welch/post-hoc/supplementary options off. Run by-column,
   then an aligned by-ID equivalent. Expect A/B/C order and the same primary
   table/numerics as the retained **candidate** evidence, without promoting it to
   a native golden. After separately authorized lifecycle testing, cancel or
   switch the active document before acquisition and confirm existing invalidation
   and no-write behavior. Do not infer this native outcome from the host-free tests.
3. Optional existing WASM smoke: load the current typed ANOVA/KW sample and confirm
   the existing primary result/group order. Browser smoke is **OWNER MANUAL / NOT
   RUN**; managed/Node tests do not constitute browser or Office native UI testing.

## Modified paths and proposed commit scope — nothing staged

| Path | Purpose |
|---|---|
| `BESHStat.Excel.Common/src/Input/UnivariateSelectionPolicy.vb` | Typed roles, parser composition, imported compatibility selection and immutable provenance/error snapshots |
| `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb` | Additive explicit group limits and accepted-observation provenance; existing defaults preserved |
| `BESHStat.Excel.Common.Tests/src/UnivariateSelectionPolicyTests.vb` | Raw selection/diagnostics/culture/row identity and accepted-candidate parity |
| `BESHstatNG/src/DM/WindowsUnivariateInputAdapter.vb` | Pure actual Windows conversion seam |
| `BESHstatNG/src/UI/UibyID.vb` | Only two methods delegate equivalent role/selection policy |
| `BESHstatNG/BESHStatNG.vbproj` | Compile the new Windows adapter |
| `BESHStatNG.Test/src/UnivariateSelectionAdapterTests.vb` | Frozen before/after characterization, actual adapter/capture tests and GUI/UDF alignment candidate |
| `BESHStatNG.Test/BESHStatNG.Tests.vbproj` | Compile the new Windows tests |
| `scripts/tests/test_04a03_selection.py` | Source-bound real wiring, no-write guard and unchanged restricted-host methods |
| `scripts/verify-04a03.py` | Isolated verification, protected evidence and exact native-obligation crosswalk |
| `docs/architecture/04A03/pre-change-crosswalk.json` | Compact source/obligation review matrix; ignored, requires explicit later staging decision |
| `docs/architecture/batch-04A-03-owner-review.md` | This single consolidated owner dossier |

Proposed commit scope is exactly these 12 paths after owner review and separate
Git authorization; no actual staging or commit is performed. No new production
source/test path is ignored. The crosswalk is ignored by the existing architecture
allowlist and must be admitted explicitly later; ignore rules are unchanged.

The entire new TestResults root, logs/TRX, execution copies, generated candidates,
assembly/data sandboxes, failed attempts and protection inventories are local-only
and excluded from GitHub. Existing acceptance receipt/amendment/publication files
and original raw reference/ledger evidence also remain local-only as disclosed
above. A GitHub clone is not self-contained for these historical reader checks.
The starting and ending commit remain the same; all work is unstaged/uncommitted.

## Final verification results and retention

| Final suite | Debug | Release |
|---|---:|---:|
| Core managed | 1,024 PASS | 1,024 PASS |
| Common managed | 852 PASS | 852 PASS |
| Office add-in managed | 505 PASS | 505 PASS |
| WASM execution managed | 45 PASS | 45 PASS |
| Windows legacy managed | 838 PASS | 838 PASS |
| **Distinct managed cases in these five assemblies** | **3,264 PASS, 0 fail/skip** | **3,264 PASS, 0 fail/skip** |
| Node (Office 468 + help 71 + WASM 377) | 916 PASS, 0 fail/skip | 916 PASS, 0 fail/skip |
| Python (tools 27 + help 16 + scripts 351 + Common 168) | 547/562 PASS; 15 nonpasses | 547/562 PASS; 15 nonpasses |

The same 63 new managed cases (46 Common, 17 Windows) and eight new Python source
checks pass in each configuration and are included in these totals. The last
focused pass was 62/62 Common (including existing grouped tests), 29/29 Windows
(including 12 existing RefEdit tests), and 8/8 standalone source checks. The 15/15
semantic-capture repetitions per configuration are additional executions of
already counted cases, not 15 more distinct tests. Debug and Release are separate
executions of the same case identities. Historical C14 3,210/916/551-of-554 counts
are context, not a forced denominator for this final suite selection.

Final Python nonpasses are exactly the three previously disclosed failures,
eight pre-existing main/receipt-byte failures, and four new whole-form source-pin
conflicts listed above. There are no final managed, numerical-candidate, Node or
new wiring/parity test failures. The overall verification exits remain **1** due
to those Python checks; this is not reported as a green full validation. All old
tests and records are retained. Original strict/profile status remains the
reported **exit 1 / exit 1 with 24 historical failures**, reused and not rerun;
no expensive historical audit writer, qualified R execution or native pilot ran.

Authoritative local consolidated results:
`TestResults/04A-03-20261102-run01/final-validation-summary-v2.json` contains exact
final TRX paths/counters, Python identities/outcomes, Node counts, baseline byte
analysis and all retained managed attempts. The earlier summary is retained;
v2 makes the new-class counting predicate explicit. Final TRX locations are:

- `Debug/results/{Core,OfficeAddin,OfficeExecution}.trx`
- `executions/final-debug02-Common/results/Common.trx`
- `executions/final-debug-resource-Legacy/results/Legacy.trx`
- `Release/results/{Core,OfficeAddin,OfficeExecution}.trx`
- `executions/final-release-Common/results/Common.trx`
- `executions/final-release-Legacy/results/Legacy.trx`

Paths above are relative to the new evidence root. Full logs are
`final-debug02-full.log`, `final-release-full.log`,
`final-debug-corrected-common-python.log`, `final-release-common-python.log`,
and the per-execution logs. Four accepted-candidate comparisons per configuration
and four fresh C05 semantic captures are isolated there. They remain candidates.

Final evidence commands:

```powershell
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B TestResults/04A-03-20261102-run01/final-analysis.py
& 'C:/Users/peter/Desktop/WPy64-31470/python/python.exe' -B scripts/verify-04a03.py preserve --evidence TestResults/04A-03-20261102-run01 --name final
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --name-status
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG rev-parse HEAD main origin/main
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG merge-base HEAD main
```

`preservation-final.json`: **PASS, all 144,224 historical TestResults files retained**,
zero missing/added historical members or metadata/file-identity changes. This
checks original paths, sizes, creation/write timestamps, file identity and frozen
authority/verifier/VersionAttributes hashes; it does not claim a new full-byte
hash audit of every historical object. Historical files were never overwritten,
removed, relocated, compressed or rebaselined. Final free disk space is
**542,888,263,680 bytes (about 506 GiB)**, above both retention warning thresholds.

`git diff --check` passes; the index is empty. Only the 12 listed paths are changed
or newly created, including the explicitly disclosed ignored crosswalk. Ending
HEAD, `main`, cached `origin/main`, and the branch merge base all remain
`9e2326d4b562cc94660745f1453b231df938bff0`. There was no fetch, staging, commit,
merge, push, installer, signing, deployment, framework change or 04A.04 work.

Native risk remains unqualified; retaining legacy ingestion also retains the
explicitly tested header/nonfinite/empty-by-ID limitations. Provenance adds O(n)
metadata allocation. No native large-range performance or desktop smoke is claimed.
Gate C2 remains UNMET; P=NO, N1=NO GO, N2=NO; 04A.04 was not started.
