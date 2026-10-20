# Batch 02.06: output targets and host fallbacks

Implementation on `codex/02-06`; **pending independent source review and live host
acceptance**, not accepted or complete. Initial branch and `main` HEAD were both
`460ea93eab75efd7ff4ce66ceedaebd5645597b3` (`feat(office): integrate batch 02.05
shared output plan writer`). Initial `git status --short` was empty. The checkout,
`AGENTS.md`, Revision 1.4 master plan and 04A companion audit were inspected first;
no ZIP was extracted over source. Referenced APIs/files were present. The meaningful
baseline discrepancy was the plan's four stale active checkpoints: they still said
02.05 pending and 23/199. They now record the user's accepted 02.05 merge and Excel
Online C10 smoke, **222 total / 24 accepted / 198 outstanding**, with 02.06 current
and pending review. Historical descriptions and every Phase 04A entry/dependency
remain unchanged, including **04.07 -> 04A.01-04A.74 -> dependent Phase 05+**.

## Destination flow before and after

Before Office:

```text
Home bound destination/start -> runtime capability/analysis preflight
  -> source acquisition -> Common analysis -> OutputDocument + semantic plan
  -> OutputLayoutPlanner -> OfficeOutputPlanAdapter (wire v1)
  -> JS validateOutputPlan -> runtime capabilities -> Excel.run
  -> active worksheet OR enumerate/add uniquely named worksheet -> clears/writes
```

After Office: insert `TargetPlanner.Create(...).ToTarget()` immediately after the
existing run/write preflight, before acquisition. The rest of the Common pipeline
retains its responsibilities. JS revalidates the complete wire plan and actual
runtime capabilities before `Excel.run`. Inside that run, `resolveOutputWorksheet`
holds the intended active worksheet object, or enumerates names and creates a
distinct worksheet. Read-only protection/merge probes precede cell mutation.
No target-decision DTO or additional field crosses the v1 wire boundary.

Before Windows ANOVA/KW:

```text
analysis + supplementary models -> GetResultWriter (may Add workbook/sheet)
  -> EnsureCanWrite -> requests -> document/layout -> prepared commands/bounds
  -> AreaCheck / existing Yes-No MsgBox -> shared writes -> supplementary/charts
```

After Windows ANOVA/KW:

```text
analysis + supplementary models -> GetSharedResultWriter
  -> read requested radio/reference/start + host facts
  -> existing analysis/capability check -> TargetPlanner
  -> existing layout + supplementary envelope calculation (no creation)
  -> current-target merge/protection check
  -> unchanged GetResultWriter (actual Workbooks.Add / Worksheets.Add / reference)
  -> actual resolved name/start -> requests -> document/layout -> prepared commands
  -> recheck actual host facts -> unchanged AreaCheck / Yes-No MsgBox
  -> unchanged shared writes -> supplementary/charts
```

The early layout is a validation preview, not a second geometry algorithm. The
existing `PreflightBounds` now has a layout overload so both pre-creation and
prepared-command paths use the same supplementary envelope calculation. It also
honors an observed smaller host grid. The later preparation retains the accepted
payload/identity/style validation. Other `GetResultWriter` callers are untouched.

## Compact Common policy and contracts

`TargetPlanner.vb` is pure, `Option Strict On`, `netstandard2.0`, with no COM,
Office.js, filesystem, WinForms or Microsoft.VisualBasic helper dependencies.

* `OutputTargetFacts` is an immutable set of optional host observations. Nullable
  facts mean **unverified**, including writability, existence and physical identity.
  Facts must describe this exact requested target, not the input/source sheet.
* `OutputTargetDecision` snapshots destination, requested/resolved name, both
  one-based coordinates, host/platform, selected capability and immutable facts.
  Its `Result` reuses `CapabilityPreflightResult`/`HostCapabilityException`.
  `ToTarget` first ensures support and returns a fresh mutable legacy DTO.
  Mutating the original request, host snapshot or returned DTO cannot alter it.
  As with existing planners, callers must not mutate inputs concurrently during
  snapshot construction.
* Requested and effective destinations are always equal. There is no fallback
  option, automatic retry at another destination, cross-workbook copy, or consent
  implied by alternatives in an error message. A new explicit user choice is
  required before another write.
* Existing codes `host.unavailable`, `host.capability-missing`,
  `output.destination-unsupported`, `output.target-invalid` remain. Additive codes
  are `output.sheet-name-collision`, `output.target-unwritable`,
  `output.target-merged`. Messages tell the user how to choose another target.
* The existing capability preflight remains the capability authority. Its OfficeJs
  output check now rejects NewWorkbook even if an incorrect snapshot claims it.
  The 00.06/01.05A identified spreadsheet admission and legacy IsExcel compatibility
  are unchanged. Optional unknown facts do not block valid legacy paths.
* Invalid/unknown/null requests, nonpositive/out-of-grid starts, inaccessible
  workbook/sheet, ambiguous identity and known naming/protection/merge conflicts
  fail without repairing coordinates or calling `EnsureDefaults`.

The enum values, mutable target API, analysis-result schemas, Office wire schema v1,
numeric algorithms, table values/order/sections, p-value semantics and C6 fixtures
are unchanged. These contracts are additive; the Windows adapter and bounds
overload are internal. Reservations still mean caller-supplied logical reservations,
never live occupancy. A name or successful preflight does not establish permission
to overwrite a physical workbook/sheet.

| Destination | Windows Excel-DNA | Office.js/WasmSmoke |
|---|---|---|
| CurrentWorksheet = 0 | Existing reference and row/column pointers | Intended active worksheet held within the write run |
| NewWorksheet = 1 | Existing Excel automatic naming and actual Worksheets.Add | Enumerate names, choose unique name, Worksheets.add |
| NewWorkbook = 2 | Existing Workbooks.Add | Unadvertised/disabled; explicitly rejected in Common and active/legacy JS |

Windows new sheet/workbook UI still starts at A1 as before; it does not acquire new
start-cell controls. The portable planner/adapters preserve any explicit C10 target.
Windows automatic sheet names remain unknown during early validation and are
reported from the actual writer worksheet after Excel assigns them. Common never
pretends to assign the physical name.

Office keeps trimming, invalid-character substitution, 31 UTF-16-unit maximum,
case-insensitive collisions and `_2` through `_999` suffixes. It additionally handles
boundary apostrophes, control characters, reserved `History`, and avoids splitting
a surrogate pair during truncation. Unicode spelling is retained, not normalized
to ASCII. Inaccessible enumeration or exhausted suffixes fail before add/activation;
an existing sheet is never selected/reused to resolve a new-sheet collision. A
concurrent name collision at add may still fail; it is not retried on an old sheet.

Home preserves its successful default Blazor change binding. Commit numeric inputs
with Tab as in the accepted smoke; row and column remain distinct. An unsupported
selected destination now has an explicit explanation. Capability snapshots refresh
on connect/run and the JS writer checks again at execution; a disabled UI option
is not the enforcement boundary. Existing capability loss requires refresh/run to
be observed; there is no new host-change subscription or production UI.

## Implemented host probes and practical limits

Office's mandatory execution baseline remains ExcelApi 1.1. Optional probes use
documented APIs only: [WorksheetProtection.protected, ExcelApi 1.2](https://learn.microsoft.com/en-us/javascript/api/excel/excel.worksheetprotection?view=excel-js-preview),
[WorkbookProtection.protected, ExcelApi 1.7](https://learn.microsoft.com/en-us/javascript/api/excel/excel.workbookprotection?view=excel-js-preview),
and [Range.getMergedAreasOrNullObject, ExcelApi 1.13](https://learn.microsoft.com/en-us/javascript/api/excel/excel.range?view=office-js#getmergedareasornullobject()).

Current worksheet: load name/protection and inspect only planned table rectangles
for merges. One read-only sync discovers these conditions before any clear, values,
formatting, activate or select. The active sheet object is reused after that sync.
New worksheet: load collection names and, if supported, workbook structure protection
in one read-only sync before add. A new empty sheet has no pre-existing merged-cell
regions; no extra post-creation safety assertion is made. Workbook/sheet/API access
errors propagate before cell mutation. A claimed supported probe missing at runtime
fails; an unsupported optional API is unverified and does not remove ExcelApi 1.1
capabilities. No per-cell sync or entire-workbook cell scan is introduced.

The active wire plan is already bounded to 64 artifacts/10,000 cells by accepted
prototype guards. Merge probes use those rectangles. The documented merge API
fails above 512 merged areas; such probe failures abort before writes rather than
being swallowed or treated as permission. Worksheet protection is conservatively
rejected even if some individual cells might be unlocked; this renderer also writes
formatting. Office read-only/coauthoring permissions without an available probe
remain unverified, not a false safe result.

Windows uses actual read-only COM properties: Workbook.ReadOnly, ProtectStructure,
Worksheet.ProtectContents, actual Rows/Columns.Count and Range.MergeCells over the
conservative shared-plus-supplementary envelope. Mixed merged/unmerged (Null) is
also rejected. This conservative check includes envelope gaps/padding. It does
not enumerate workbook contents or call Activate/Select. Known invalid geometry,
including supplementary overflow/next-cursor restrictions, is checked before
GetResultWriter. Actual target facts are checked again before AreaCheck/dialog.
COM exceptions during inspection propagate without writing; optional absent facts
in recording tests are not proof of real Excel permissions.

No probe is a lock. Protection, identity or permission may change between inspection
and writing. Excel default templates/events can also change a just-created target.
An Office new sheet or Windows new sheet/workbook can persist after a later failure,
invalid post-creation state, or Windows overwrite No. Late COM/Office errors may
leave prior table values/styles, or a partly rendered table. This is not a
transaction. The Windows Yes/No overwrite prompt, AutoFit=False, charts and ANOVA/KW
supplement order are retained. Office still clears **only planned table rectangles**,
then assigns values, leaving separator-only cells untouched; it has **no occupied-cell
consent**. General overwrite protection, partial-write outcomes, rollback, undo and
recovery remain 02.08 work. No Mac runtime acceptance is claimed.

Isolated legacy JS writeResultTable(s) retain their renderer/cursor behavior. They
still reject NewWorkbook, and now reject malformed explicit destinations/illegal
starts instead of silently defaulting/clamping. Omitted legacy fields retain their
compatibility defaults. The active writer's new live probes are not retrofitted
into this compatibility renderer in this batch.

## Complete delivery inventory

| Path | Change |
|---|---|
| `BESHStat.Excel.Common/src/Output/TargetPlanner.vb` | New pure facts/decision/planner |
| `BESHStat.Excel.Common/src/Host/HostCapabilities.vb` | Additive codes; reject false Office NewWorkbook claim |
| `BESHStat.Excel.Common.Tests/src/OutputTargetPlannerTests.vb` | New 36 planner cases |
| `BESHStat.Excel.Common.Tests/src/OfficeOutputPlanAdapterTests.vb` | Existing eight wire golden cases also pass through target planner |
| `BESHStat.WasmSmoke/Pages/Home.razor` | Target planner before acquisition; unsupported explanation |
| `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js` | Read-only target resolution/probes, compatible naming, legacy explicit-input refusal |
| `BESHStat.WasmSmoke/tests/helpers/recordingWorkbook.cjs` | Runtime capability/protection/merge/failure recording seams |
| `BESHStat.WasmSmoke/tests/hostCapabilities.test.cjs` | Invalid legacy starts/destinations before Excel.run |
| `BESHStat.WasmSmoke/tests/outputPlanWriter.test.cjs` | C10/D20/A10, host probes/loss, naming/collision/exhaustion |
| `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb` | New read-only COM facts and pure pre-creation seam |
| `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb` | Reuse bounds before creation and honor actual grid limits |
| `BESHstatNG/src/UI/UibyID.vb` | Narrow early ANOVA/KW preflight and actual target recheck |
| `BESHstatNG/BESHStatNG.vbproj` | Explicit new adapter include only |
| `BESHStatNG.Test/src/DataManagement/SharedOutputPlanTests.vb` | 14 added recording cases; accepted 51 retained |
| `planning/BESHStatNG_CrossPlatform_MasterPlan.md` | Exactly four active checkpoint lines |
| `docs/architecture/batch-02-06-output-targets-and-host-fallbacks.md` | This design/evidence/acceptance guide |

No deleted paths. Core/Common remain netstandard2.0; modern tests/WasmSmoke net10.0;
Windows host/tests Framework 4.8. No new packages, framework changes, golden fixture
edits, Ribbon/UDF rename, signing/installer changes or Phase 04A work. Generated
HelpContext/HelpLinks/VersionAttributes are restored from exact pre-build byte
snapshots after validation; both existing stashes are retained. Tracked-file diffs
are distinguished from four new untracked delivery files, which ordinary
`git diff --stat` does not include.

## Validation evidence

Commands run in the repository (ignored logs/TRX in `TestResults/Codex/Batch0206`):

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --no-restore --filter 'FullyQualifiedName~OutputTargetPlannerTests|FullyQualifiedName~HostCapabilitiesTests|FullyQualifiedName~OfficeOutputPlanAdapterTests|FullyQualifiedName~OutputLayoutPlannerTests|FullyQualifiedName~Stage304ContractSnapshotTests' --logger 'trx;LogFileName=FocusedCommon.trx' --results-directory TestResults/Codex/Batch0206

node --check BESHStat.WasmSmoke/wwwroot/js/officeBridge.js
node --test BESHStat.WasmSmoke/tests/hostCapabilities.test.cjs BESHStat.WasmSmoke/tests/outputPlanWriter.test.cjs

& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe' BESHStatNG.Test/BESHStatNG.Tests.vbproj /t:Build /p:Configuration=Debug /p:PythonExe=C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe /m /v:minimal
& 'C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe' BESHStatNG.Test/bin/Debug/BESHStatNG.Test.dll /TestCaseFilter:FullyQualifiedName~SharedOutputPlanTests '/Logger:trx;LogFileName=FocusedWindows.trx' /ResultsDirectory:TestResults/Codex/Batch0206

powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./scripts/verify-beshstatng.ps1 -Python -PythonExe 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe' -Js -Wasm -Legacy -VisualStudio 2026
```

Focused Common **132/132**, final focused JS **122/122**, focused Windows **65/65**;
all exit 0, JS syntax exit 0. New Common names cover Matrix_NoImplicitFallback,
InvalidCapabilities_HaveStableActionableFailures, IllegalStart_IsNeverRepaired,
KnownHostFacts_RejectWithoutConfusingNamesWithOccupancy,
UnknownFactsAndResolvedNames_AreNotClaimsOfSafetyOrIdentity,
Snapshot_IsIsolatedFromRequestHostAndReturnedDtoMutation, and
Coordinates_SurvivePlannerLayoutAndOfficeAdapter. Windows additions are
TargetPreflight_AllWindowsDestinationsPreserveC10BeforeHostCreation and
TargetPreflight_KnownInvalidRequestDoesNotReachMutationOrConfirmation. These are
unit/recorded-host tests, not certification of actual COM/Office permissions.

During development, two focused Common compile attempts exited 1: the new test
file lacked an explicit MSTest import, then its semantic-plan helper omitted
required arguments. Both were fixed in the new tests. One JS iteration passed
116/117 because PowerShell stdin converted Unicode test literals to question marks;
explicit Unicode escapes fixed the test transport. No accepted assertions were
weakened. The first full gate exited 0 with JS 184; five additional malformed legacy
destination regressions were then added with their fix and the exact full gate was
rerun against the final source. Final full-gate results are recorded below.

Final exact full gate: **exit 0, 2,307 passed, zero failures/skips**, +79 against
accepted 02.05. Individual child commands completed successfully:

| Suite/build | 02.05 baseline | Final | Outcome |
|---|---:|---:|---|
| Core MSTest | 1,024 | 1,024 | Exit 0 |
| Common MSTest | 400 | 436 | Exit 0, +36 |
| Windows Framework 4.8 MSTest | 612 | 626 | Exit 0, +14 |
| Office.js Node | 160 | 189 | Exit 0, +29 |
| Python Windows tooling | 22 | 22 | OK, exit 0 |
| Python help/MkDocs | 5 | 5 | OK, exit 0 |
| Python discovery scripts | 5 | 5 | OK, exit 0 |
| WASM Debug | Passing | Passing | Exit 0, 0 warnings/errors |
| VS2026 Windows Debug | Passing | Passing | Exit 0, 0 warnings/errors |
| Excel-DNA x86 packed XLL | Passing | 1,913,344 bytes | Produced |
| Excel-DNA x64 packed XLL | Passing | 1,839,104 bytes | Produced |

Stage304 inventory, method catalog validation, generated metadata `--check` and
retired-helper audit all passed. Restore reported projects up-to-date; no packages
were installed. Existing `UTA001 MatrixTestHelpers` test-discovery advisory and
TRX overwrite notices remain. Redirected Python unittest stderr appears as
PowerShell NativeCommandError text, while every subgroup reports OK and exits 0;
this is not a compiler warning or a failed gate. No unrelated failing tests were
observed. Full logs are `FullGate.log` (first successful gate) and
`FullGateFinal.log` (final source), with focused TRX/logs alongside them.

Final scope commands, all exit 0:

```powershell
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG branch --show-current
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --stat
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --name-only
```

The sandbox owner SID differs from the checkout owner; plain Git initially refused
read access. All subsequent Git operations used the command-local safe-directory
exception shown above, with no local/global Git configuration change. `diff --check`
is clean. Status contains only the 16 inventory paths (12 modified tracked files,
4 new untracked files). Git may report CRLF-to-LF normalization advisories; these
are not compiler warnings or changes to archived baseline fixtures. Byte comparisons
against Git HEAD confirm all accepted fixture files and the entire Phase 04A audit
are unchanged. All master-plan lines except the four active checkpoint lines match
HEAD exactly. Generated help/version files match their original pre-build snapshots
byte-for-byte and have no tracked diff. Both original stashes remain unchanged.
No source was copied from an older archive; no archive-only differences are included.


## Manual acceptance guide (user-run; not executed by Codex)

Use disposable workbooks, especially for Office current-sheet tests: the current
Office writer clears the planned table rectangles without overwrite consent.

1. **Excel Online NewWorksheet C10**: select a valid three-group ANOVA input range.
   Choose New worksheet, set row **10**, column **3**, and commit both inputs with
   Tab. Run output. Verify the source/group header is at **C10**, first table is
   **C10:H13**, remaining tables/footer follow the existing one-blank-row layout,
   values remain Excel numbers and p-value formatting matches the accepted result.
   Inspect the `beshOffice.writeOutputPlan` argument in browser debugger/console:
   target row/column 10/3 and first bounds 10/3; the range call is
   `getRangeByIndexes(9,2,4,6)`. Compare actual Excel-returned `tableAddresses` and
   overall `address` to the plan, allowing Excel's sheet-name/address quoting.
2. **Excel Online CurrentWorksheet D20**: put source elsewhere, leave a deliberately
   empty output region at D20. Commit row 20/column 4, activate the intended output
   sheet, and run. Check the actual active sheet, D20 start, all table addresses,
   footer, p-values and untouched separator-only cells. Repeat with KW/Dunn.
3. **Excel Online unsupported NewWorkbook**: confirm the option is disabled and
   unadvertised. In a disposable debugging session, invoke `writeOutputPlan` with
   a copied valid plan whose destination is 2 (and legacy writeResultTables with
   destination 2). Verify explicit unsupported rejection, no Excel.run/add/write,
   no new worksheet/workbook and no automatic fallback. Common preflight must also
   reject destination 2. Select another destination explicitly for any later run.
4. **Excel Online duplicate name**: request the same new sheet twice, including a
   case-only variant and a 31-character name. Verify the unique `_2`/`_3` suffix,
   unchanged original result sheet, matching returned names/addresses, formatting
   and C10 geometry. Also try invalid characters; the normalized base must create
   a new distinct sheet. Do not expect silent reuse if a concurrent collision occurs.
5. **Excel Online negative probes**: only if safe and available, protect a disposable
   target sheet or merge cells within one planned rectangle. Current-sheet output
   should reject after read-only inspection with no clear/values/activation. If
   workbook structure protection is available, verify NewWorksheet refuses before
   add. Optional APIs absent in the actual runtime remain unverified; record that
   explicitly rather than claiming the negative smoke passed.
6. **Windows Excel-DNA**: run ANOVA, Welch/posthoc and Kruskal-Wallis/Dunn to current
   worksheet, new worksheet and **actual new workbook**. Use a non-A1 reference
   such as C6/C10 on a current sheet (including a referenced inactive sheet).
   Confirm all existing pointers/selection, controls, supplementary homogeneity/
   descriptive/box summary order, charts, footers, p-value formats/highlights and
   AutoFit=False. New sheet/book retains Excel automatic naming/A1 start. Confirm
   Workbooks.Add creates an independent workbook only for NewWorkbook. Put disposable
   content in the output envelope; test overwrite **No**, then **Yes**, preserving
   the original dialog. No must leave shared/supplementary/chart content unwritten.
   Test a protected/merged current target and protected workbook structure without
   valuable data; expect early rejection without destination creation/content.
7. **Failure recording**: record whether failure was before host access, during
   read-only sync/COM inspection, after creation, or during writes/styles. A created
   target or earlier table may remain after late failure; do not describe this as
   rollback or safe-overwrite acceptance. Mac Office.js runtime testing is separate.

Suggested commit message after later user approval:
`feat(output): add batch 02.06 cross-host target planning and destination preflight`

Stop at review. No commit, push, merge, branch switch, live Excel launch, deployment,
installer, signing action, acceptance claim or 02.07/02.08/04A implementation is
authorized by this patch.
