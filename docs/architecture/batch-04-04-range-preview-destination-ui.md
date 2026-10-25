# Batch 04.04 — range, validation, destination and ANOVA/KW pilot

**IMPLEMENTED / UNCOMMITTED / PENDING OWNER ACCEPTANCE.**
Checkpoint: **223 total / 36 accepted / 187 outstanding**. 04.04 is not accepted;
Gate C remains incomplete. Only this batch is implemented.

## 23 October corrective amendment: rerun, numbered worksheets and tab return

This section supersedes earlier duplicate-name rejection/manual-renaming guidance
and the previous stale-preview amendment. The authoritative local tree was on
`codex/04-04`, HEAD `30d77acff28135cd00da145f38eb9bb708e991c7`, with 63 pre-existing
changed/untracked paths. `TestResults/04-04/corrective-preflight/inventory.json`
and byte copies preserve that baseline, all 15 owner README files and generated
VersionAttributes.vb. AGENTS, Revision 1.8 plan, this report and Office README were
read before changes. No reset/stash/new branch/staging/commit/push/merge/signing/
installer/deployment or later-batch implementation is authorized or performed.
**223 total / 36 accepted / 187 outstanding remains unchanged.**

### Proven rerun race and bounded recovery

The deterministic `PendingOptionRetirementMustFinishBeforeFreshRunPinsAnySource`
test failed on the audited implementation: expected one pinned source while the
old option-change retirement was held, but a second pin had already happened.
`Invalidate()` fired host retirement without awaiting it. That old native invalidate
could therefore retire a newly acquired source, leading to stale-preview rejection.
`corrective-preflight/reproduce.log` records the failure; this proves the ordering
defect, not that it is the only cause of the owner's genuine Excel behavior.

State now queues source/output retirements and awaits the complete queue inside
the next operation's existing single cancellation/deadline boundary. It does not
nest operations, reuse old snapshots/consent, or perform reads on option changes.
Generation checks remain after awaits; Cancel/disposal cannot revive the old run.
The shared WorkbookTransport also serializes host retirements before subsequent
interop calls across component owners. Disposal queues retirement immediately,
before awaiting picker cleanup. A second deterministic test holds the old owner's
native invalidate and proves a new pin cannot overtake it; old token callbacks stay
inert after the new pin. Retirement is not a host reinitialization or workbook read.
Capture/resolve waits on this same barrier and its own prior capture retirement,
preventing an older event subscription cleanup from overtaking its next capture.
The same input/output text survives option invalidation and Retry. A separate
regression found initial acquisition overwrote an already chosen new-sheet start
cell with the default J1; defaulting now preserves the new-sheet draft/start cell.

Explicit reacquisition of a whole-column reference may recompute its effective
used footprint while requiring the same original scope, document and exact physical
worksheet ID/name. This permits fresh Run after real content/boundary edits. A live
lease still rejects a changed fingerprint/boundary before compute or write; it is
never repaired in place. Deleted/replaced/renamed sheets reject rather than rebinding
by name. Browser focus/activation signals remain conservative invalidations, with a
fresh explicit Run as recovery. Tab changes keep the analysis component alive.

### Name allocation and exact target binding

`worksheetNames.mjs` is the sole pure allocation policy at the Office boundary.
It preserves a free requested spelling, compares names case-insensitively, and tries
positive suffixes 1 through 10,000 in ascending order. Gaps are reused. Each suffix
reserves its decimal length from the 31 UTF-16-unit limit; truncation never splits a
surrogate pair. Valid accented names/internal apostrophes are retained; malformed
Unicode, controls, empty/whitespace-only, forbidden characters, exterior apostrophes,
overlength and reserved History names reject. An ambiguous/unverifiable inventory,
more than 10,000 sheets or exhausted suffix search fails nonmutating with
`output.sheet-name-unavailable`. This is intentionally conservative, not a promise
of creating every possible workbook with arbitrarily many sheets.

The additive private `IWorksheetNameHost`/`WorksheetNameReply` bridge resolves the
candidate from **actual workbook worksheets.items id/name**, after fresh source,
document and protection verification. It creates nothing. C# validates the reply,
keeps the requested base in NewSheetName, and constructs Common's target/semantic/
wire plan and WriteIntent using the **resolved** name. A new normal inspection then
binds that exact plan/intent and candidate. No Common schema, numerical algorithm,
table style, output footprint or `validateProductionPlan` equality check changes.

Immediately before `worksheets.add`, execute rereads the workbook collection and
reinspects the same target/source. A name conflict before mutation rejects safely;
the user may explicitly click Write again for another allocation/new plan/inspection.
There is no automatic loop or second mutation. A server race/fault after add may
have been submitted remains PartiallyWritten/`output.mutation-uncertain`, with the
original sticky latch, trace and recovery. New worksheet requests never reuse,
overwrite/delete/rename existing sheets or ask occupied-output No/Yes. Existing-sheet
output retains its separate single-use occupied consent workflow. Authorized add
events follow existing operation ownership; no lifecycle event is globally ignored.

`SheetNameNotice` separates requested/resolved names and creation state. It is an
accessible nonmodal status near output controls, encoded by Razor; it says “Will use”
before mutation and “Created” only after verified Completed. A failure preserves a
truthful allocation-without-confirmed-creation notice. Last write completion reports
the actual created name separately. The base field remains unchanged across runs.
The stable warning code is **`output.sheet-name-adjusted`**; a free name emits none.
Warning delivery cannot convert Completed into failure.

Existing opt-in bounded WorkbookDiagnostics and memory-only writerTrace record the
allowlisted warning code only, **never requested/resolved names, addresses, cells or
native exception details**. Local reply/notice names are for the intended output UI.
**04.06 integration contract:** forward this event to its scheduled persistent log
pipeline using its accepted opt-in/settings/sink/redaction policy; delivery failures
must remain isolated from the write result. No parallel persistent logging or
Settings schema/store/editor is introduced here.

### Verification and full inventory

The following required commands all exited **0** on the final implementation:

```powershell
# VS 2026 MSBuild is discovered with vswhere [18.0,19.0); repeat for Debug/Release.
& $msbuild BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:Platform=Any CPU' "/p:PythonExe=$env:PythonExe" /m
& $msbuild BESHStatNG.sln /t:Build /p:Configuration=Release '/p:Platform=Any CPU' "/p:PythonExe=$env:PythonExe" /m
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Release -VisualStudio 2026 -Legacy -Wasm -Js -Python
node --test BESHStat.OfficeAddin/tests/*.test.mjs
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
```

Existing local NuGet cache/RestoreSources and configured PythonExe conventions
were used, without package/network installation or hardcoded owner home paths.
Full gates were repeated after the final transport/disposal and output-status UI
changes; `corrective/initial-gates` retains earlier runs and is not added to totals.

| Distinct suite | Debug passed | Release passed | Failed / skipped |
|---|---:|---:|---:|
| Core | 1,024 | 1,024 | 0 / 0 |
| Common | 727 | 727 | 0 / 0 |
| Windows legacy | 787 | 787 | 0 / 0 |
| Office C#/components | 316 | 316 | 0 / 0 |
| Wasm execution | 45 | 45 | 0 / 0 |
| Office JS | 361 | 361 | 0 / 0 |
| Help JS | 71 | 71 | 0 / 0 |
| WasmSmoke JS | 377 | 377 | 0 / 0 |
| Help Python/catalog/link checks | 27 + 16 + 5 | 27 + 16 + 5 | 0 / 0 |
| **Total distinct checks** | **3,756** | **3,756** | **0 / 0** |

The 13 added managed cases cover the reproduced pending-retirement race, cancellation
and recovery, Completed → Welch/Fisher rerun/fresh No/Yes, exact Common target/intent
and requested-base preservation, truthful completed/uncertain notices, nonmutating
allocation errors, focus/source-edit recovery, actual output-component status rendering,
redacted diagnostic forwarding and retirement across component owners. Node adds
35 name-policy/real-boundary cases and one whole-column refresh/replacement case.
No existing rejection/readback/uncertainty tests were weakened; direct low-level
inspection of an already bound conflicting name still rejects as required.

**15 Office simulated Edge scenarios passed** (the corrective workflow plus all 14
existing pane/readiness/options/capture/workbook cases). Corrective execution used
actual Blazor/Common/Core and production transport/writer with simulated Office:
Completed J1 → new options → direct Run → new occupied No/Yes, unchanged references,
browser/tab activation and genuine-in-the-fixture cell-change invalidation followed
by fresh Run, long F:F/E1:E11, first gap ANOVAresults2 and next ANOVAresults4 with the
same base. Accessible role=status and narrow-pane overflow were asserted. The
simulated screenshot `corrective/screenshots/numbered-sheet-completed.png` shows
the unchanged base beside the verified simulated created-name warning; it is **not
a genuine workbook screenshot**. WasmSmoke on HTTPS 7131 independently passed its
standalone unavailable-Office/disabled-write/safe-yielding check with zero exceptions.
Owned local hosts and the isolated headless Edge process were closed after checks.

Final solution/foundation/regression logs report **0 compiler warnings/errors**.
Existing noncompiler notices remain: one ambiguous Help Form+Title mapping falls
back to Home, TRX overwrite notices and Git CRLF-to-LF notices. No new dependency
advisory was reported by these local-cache gates. The Windows post-build log prints
the guarded PreRelease signing command text; Debug/Release exit before signing.
No signing or installer/publishing workflow ran.

`TestResults/04-04/corrective/final` contains exact commands/outcomes in the wrapper,
`outcomes.txt`, per-configuration solution/foundation/regression logs and TRX,
standalone `node.log`, `audit.json`, `readme-preservation.json`, `whitespace.log` and
`final-status.txt`. `corrective/browser/final-outcomes.txt` and individual JSON files
record browser results. Audit verified all four packed XLLs (x86 014C/x64 8664 in
Debug/Release), both Wasm host indexes/assets including worksheetNames.mjs and all
12 expected fingerprinted Core/Common/host Wasm assemblies. This is build/structural
evidence, not live Excel loading or native output verification.

All **15 original README byte prefixes** are preserved: 14 byte-identical, Office
README appended only. VersionAttributes.vb SHA256 is unchanged:
`0F22C5A1AD446546E773E59B495E647C58F5FF08A70750AFE7372680798D2F2D`.
Git remains `codex/04-04`, HEAD `30d77acff28135cd00da145f38eb9bb708e991c7`, with
**68 changed/untracked file paths**: the original 63 plus four new files and one
newly modified tracked diagnostics file. **22 paths were amended in this request**;
the other 46 pre-existing changed paths are byte-identical to this preflight.
Tracked and 39 untracked files passed whitespace checks; the index is empty.
Everything remains unstaged, uncommitted and unmerged.

Exact amended-file inventory (audit.json also enumerates all 68 branch file paths
with provenance/hashes):

- `BESHStat.OfficeAddin/Components/AnalysisPilot.razor`
- `BESHStat.OfficeAddin/Components/OutputPicker.razor`
- `BESHStat.OfficeAddin/README.md`
- `BESHStat.OfficeAddin/Services/AnalysisPilotState.cs`
- `BESHStat.OfficeAddin/Services/IPilotHost.cs`
- `BESHStat.OfficeAddin/Services/RangeReferenceState.cs`
- `BESHStat.OfficeAddin/Services/WorkbookDiagnostics.cs`
- `BESHStat.OfficeAddin/Services/WorkbookTransport.cs`
- `BESHStat.OfficeAddin/tests/AnalysisPilotTests.cs`
- `BESHStat.OfficeAddin/tests/browser/corrective-workflow-browser.mjs`
- `BESHStat.OfficeAddin/tests/CorrectiveWorkflowTests.cs`
- `BESHStat.OfficeAddin/tests/helpers/pilotWorkbook.mjs`
- `BESHStat.OfficeAddin/tests/wholeColumns.test.mjs`
- `BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs`
- `BESHStat.OfficeAddin/tests/worksheetNames.test.mjs`
- `BESHStat.OfficeAddin/wwwroot/js/productionWriter.mjs`
- `BESHStat.OfficeAddin/wwwroot/js/rangeCapture.mjs`
- `BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs`
- `BESHStat.OfficeAddin/wwwroot/js/worksheetNames.mjs`
- `docs/architecture/batch-04-04-range-preview-destination-ui.md`
- `planning/BESHStatNG_CrossPlatform_MasterPlan.md`
- `scripts/verify-office-foundation.ps1`

Final `git status --short` (fixtures/ contains the single prior table-presentation.json):

```text
 M BESHStat.Core.Tests/README.md
 M BESHStat.Core/README.md
 M BESHStat.Excel.Common.Tests/README.md
 M BESHStat.Excel.Common/README.md
 M BESHStat.OfficeAddin/App.razor
 M BESHStat.OfficeAddin/Components/AnalysisShell.razor
 M BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs
 M BESHStat.OfficeAddin/README.md
 M BESHStat.OfficeAddin/Services/AnalysisOptionDraft.cs
 M BESHStat.OfficeAddin/Services/WorkbookAdapter.cs
 M BESHStat.OfficeAddin/Services/WorkbookDiagnostics.cs
 M BESHStat.OfficeAddin/Services/WorkbookTransport.cs
 M BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs
 M BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs
 M BESHStat.OfficeAddin/tests/browser/options-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
 M BESHStat.OfficeAddin/tests/workbookAdapter.test.mjs
 M BESHStat.OfficeAddin/tests/workbookWriter.test.mjs
 M BESHStat.OfficeAddin/wwwroot/css/app.css
 M BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs
 M BESHStat.WasmSmoke/README.md
 M BESHStatNG.Test/README.md
 M BESHStatNG_Help_MkDocs/README.md
 M BESHStatNG_Installer/README.md
 M BESHstatNG/README.md
 M README.md
 M planning/BESHStatNG_CrossPlatform_MasterPlan.md
 M scripts/verify-office-foundation.ps1
?? BESHStat.OfficeAddin/Components/AnalysisPilot.razor
?? BESHStat.OfficeAddin/Components/OutputPicker.razor
?? BESHStat.OfficeAddin/Components/RangePicker.razor
?? BESHStat.OfficeAddin/Components/RangeReferencePicker.razor
?? BESHStat.OfficeAddin/Components/ValidationPreview.razor
?? BESHStat.OfficeAddin/Services/AnalysisPilotState.cs
?? BESHStat.OfficeAddin/Services/GroupedInput.cs
?? BESHStat.OfficeAddin/Services/IPilotHost.cs
?? BESHStat.OfficeAddin/Services/IRangeCaptureHost.cs
?? BESHStat.OfficeAddin/Services/OutputPlanPresentation.cs
?? BESHStat.OfficeAddin/Services/PhysicalInputView.cs
?? BESHStat.OfficeAddin/Services/RangeReferenceState.cs
?? BESHStat.OfficeAddin/tests/AnalysisPilotTests.cs
?? BESHStat.OfficeAddin/tests/CorrectiveWorkflowTests.cs
?? BESHStat.OfficeAddin/tests/FormatInputAmendmentTests.cs
?? BESHStat.OfficeAddin/tests/ProductionUxTests.cs
?? BESHStat.OfficeAddin/tests/README.md
?? BESHStat.OfficeAddin/tests/RangeReferenceTests.cs
?? BESHStat.OfficeAddin/tests/browser/corrective-workflow-browser.mjs
?? BESHStat.OfficeAddin/tests/browser/format-input-browser.mjs
?? BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs
?? BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs
?? BESHStat.OfficeAddin/tests/fixtures/
?? BESHStat.OfficeAddin/tests/helpers/pilotWorkbook.mjs
?? BESHStat.OfficeAddin/tests/longIntersection.test.mjs
?? BESHStat.OfficeAddin/tests/multiSource.test.mjs
?? BESHStat.OfficeAddin/tests/productionWriter.test.mjs
?? BESHStat.OfficeAddin/tests/rangeCapture.test.mjs
?? BESHStat.OfficeAddin/tests/tablePresentation.test.mjs
?? BESHStat.OfficeAddin/tests/wholeColumns.test.mjs
?? BESHStat.OfficeAddin/tests/worksheetNames.test.mjs
?? BESHStat.OfficeAddin/wwwroot/js/productionWriter.mjs
?? BESHStat.OfficeAddin/wwwroot/js/rangeCapture.mjs
?? BESHStat.OfficeAddin/wwwroot/js/tablePresentation.mjs
?? BESHStat.OfficeAddin/wwwroot/js/worksheetNames.mjs
?? docs/architecture/README.md
?? docs/architecture/batch-04-04-range-preview-destination-ui.md
?? planning/README.md
?? scripts/README.md
```

### Genuine Edge owner checklist (still pending)

Use a disposable workbook and retain an untouched control sheet. Simulated Edge
passes below do not establish authenticated/native Excel success. The separate
native uncertain-write blocker is still open; no current genuine trace establishing
its first failing stage or safe exception classification is available here.

1. ANOVA + Tukey, columns A1:C4 on the real input sheet, direct Run and existing J1
   Write. Only after verified Completed, enable Welch/Fisher and immediately Run
   without repicking/editing references. Verify refreshed option tables and a fresh
   occupied No/Yes at J1. No preserves the old cells; new explicit Write/Yes updates
   only the inspected table regions. No automatic output on changing options.
2. Prepare ANOVAresults, ANOVAresults1, ANOVAresults3, leaving suffix 2 free. Long
   category F:F and values E1:E11, direct Run, New worksheet base ANOVAresults,
   one Write. Verify ANOVAresults2, accessible warning/code, no overwrite prompt,
   one created sheet, correct start/all styled tables and strict Completed outcome.
   Save a genuine output/status screenshot and safe trace summary without private
   workbook contents/tokenized URLs. The editable base must remain ANOVAresults.
3. Run again with the same base; verify the next lowest free suffix (4 if 0–3 are
   now occupied). Switch browser tabs away/back and Analysis/Diagnostics, then Run
   without reload. Change real source data, ensure the old plan/consent is stale,
   and recover with fresh Run on the same resolvable physical references.
4. Test a valid 31-character base including suffix 9→10, accented/case-insensitive
   collisions and invalid/reserved names. Existing-sheet occupied consent remains
   independent. Protection/merged/stale/replaced-sheet guards must still reject.
5. On PartiallyWritten/uncertainty, **stop writes**, manually inspect and wait for
   native settlement. Capture the first failing sanitized writerTrace stage and
   safe classification; never retry or unlock the latch based on appearance. A
   limited native fix requires that actual evidence; no such fix is claimed here.

**04.04 remains pending owner acceptance. 04.05 is unstarted.**

## 23 October owner amendment: formatting, bounded columns and reusable workflows

This section supersedes the earlier exact-length long-input, manual-inspection and
formatting/evidence descriptions below. **04.04 remains pending genuine owner
acceptance: 36 / 223 accepted, 187 outstanding.** No 04.05/04.06/04.07 implementation
or new batch ID is included. Their existing Settings/Ribbon plan rows are clarified only.

### Current-source audit and preservation

Preflight verified `codex/04-04`, HEAD `30d77acff28135cd00da145f38eb9bb708e991c7`,
the expected existing worktree edits, clean tracked whitespace, and the last three
commits (`30d77ac`, `6b1e978`, `d7888cd`). Git uses a command-scoped safe.directory
exception for the sandbox SID; global configuration is untouched. AGENTS.md, Revision
1.8 master plan, this report and the owner Office README were read before code edits.
`TestResults/04-04/format-input-preflight/inventory.json` and byte copies record all
54 pre-existing changed/untracked paths, including all 15 README files. The exact
generated VersionAttributes.vb bytes are separately saved and restored in finally
around Windows builds. No reset/stash/staging/commit/push/merge/sign/deploy/install ran.

The audited production components, capture/adapter/transport/state, Common grouped
parsers/cell policies/validation, semantic styles/p-value/layout/writer contracts,
PreferenceSnapshot/IPreferenceStore, Windows writer/planning adapters and
Ui12GlobalSettings, RibbonXML and WasmSmoke were compared. Common/Core numerics,
frozen v1 meanings, serialization, targets and Gate B E1–E7 are unchanged. The three
named owner PNGs were not available on disk; no replacement screenshot was invented.
Source and existing Stage304/mixed-zero fixtures form the reference comparison.

### A — Presentation parity and native verification

The proven renderer differences were production's red significance color, extra
title bolding, and missing font name/size, alignment, header fill and border profile.
Both existing Windows and WasmSmoke use green `#32FF32`, normal black, Calibri Light,
10 pt body/title/header and 8 pt footer; headers are gray `#DDDDDD`. Windows supplies
left alignment and horizontal title/header/body boundaries. WasmSmoke's planned-cell
writer already consumes Common's exact number formats and StrongEmphasis flag.
Production now applies that shared-source host presentation profile, with consistent
both-side shared borders, rather than replacing Common semantic meaning. Table-only
border clearing includes diagonals; all property setters target inspected table cells.

The old production path passed null presentation settings and therefore used Core
defaults. WasmSmoke reads its existing preference state/local editor before semantic
planning, so unlike settings can also explain display differences. Current production
uses the existing default snapshot (**8 p-value decimal places, less-than-threshold
small-value mode, upper bound enabled**) until 04.06 exposes Settings; tests inject
other existing snapshots. Match those settings and the run alpha in the reference
hosts when comparing worksheets. This is not a diagnosis of native readback failure.

`OutputPlanPresentation` prepares every table using the existing semantic/layout/wire
adapters and an immutable existing PreferenceSnapshot. Default preferences match the
existing Common defaults; injected snapshots exercise digits, threshold/scientific/
fixed and upper-bound modes. No parallel settings DTO/store/editor is introduced.
Numeric values remain numeric, including tiny p-values; display uses the supplied
Excel NumberFormat. Frozen numeric-only significance at/below alpha is preserved.
Common's General intent for ordinary numbers and one-row table spacing are retained.

The host-only `tablePresentation.mjs` carries presentation rules; Common v1 and wire
v1 have no new fields or silent reinterpretation. Native completion still requires
exact values/formula-free state/number formats/bold/color plus font name/size,
alignment, fill and borders to read back correctly. Mismatches remain PartiallyWritten
with the sticky uncertain latch and sanitized first-failure stage. No named-color or
number-format coercion was guessed. No autofit, whole-column width/height changes,
chart creation, spacing-gap formatting or mutation outside inspected cells is added.
Exact width/autofit parity remains deferred unless its wider footprint is planned
and inspected explicitly. Cell-edge borders necessarily affect that shared visible
edge; no neighboring cell/column property setter is issued.

The golden host profile fixture records source provenance, not alleged live evidence.
Tests cover ANOVA/Welch/all post-hoc and KW/Dunn golden cells, geometry, preferences,
near-alpha/tiny p-values, blanks/roles and strict native style readback. This aligns
the source conventions; it does not certify genuine Excel display or fix the known
native uncertain-write failure.

### B — Original versus effective whole-column input

Qualified entire-column input such as `'Data'!$A:$C` or `'Hárok 1'!$F:$F` is supported.
A narrow host shorthand expansion delegates all worksheet quoting, workbook/area
rejection and finite A1 geometry validation to Common. Output still requires one
cell. Entire rows, the entire sheet, external workbook and multi-area input reject.
Typing never performs a workbook read.

On explicit capture/Run/Validate, the host uses the selected columns' own
[values-only used range](https://learn.microsoft.com/en-us/javascript/api/excel/excel.range#excel-excel-range-getusedrangeornullobject-member(1))
and bounded [formula-area metadata](https://learn.microsoft.com/en-us/javascript/api/excel/excel.rangeareas)
(ExcelApi 1.9 for normalization; the guarded pilot writer still requires 1.17).
At most 64 formula-area geometries are loaded. Empty-string formula results extend
the footprint even if a host omits them from values-only used bounds; no formula
cell matrix is read during normalization. Excess fragmented metadata fails with
`input.range-count`, unavailable verification fails closed, and no formula/area is
silently discarded. The first/last used value or formula row determines the effective
span, retaining **every selected column** and every interior row/cell.

True leading/trailing blanks and formatting-only cells outside that footprint are
excluded. Interior blanks remain in place. Numeric zero is data. Error, Boolean,
numeric-looking text and empty-string formula results remain their typed cells and
obey the existing Common policy (only true Blank is missing by default). An all-empty
column rejects with `input.empty-columns`; a column containing only an empty-string
formula is not silently empty. Headers apply at the effective first row, with actual
physical coordinates retained. A short `input.columns-narrowed` notice reports rows.

Metadata for **all** required inputs is resolved and cumulative effective sizes
checked before payload reads/allocations. The 10,000-cell, chunk, estimated-byte and
single 30-second operation limits remain. No distant data is truncated to fit. The
editable original column reference, native original footprint and effective read
rectangle remain distinct. Leases fingerprint all effective cells/formulas/formats,
recheck whole-column value/formula bounds before pin/compute/inspect/write, and watch
the existing worksheet/workbook lifecycle events. Later data/formulas outside old
used bounds invalidate even without a delivered event. Output overlap protects the
**original whole columns**, including rows outside the effective footprint. A new
capture/manual edit is required when a stored identity/normalization handle is stale.

### C/F — Reusable grouped interpretation and physical-row intersection

`GroupedInput` wraps Common's sole statistical parser with immutable interpretation
and constraints. `PhysicalInputView` pairs only the intersection of real worksheet
row intervals. It never zips compacted arrays or invents adjacent physical coordinates.
Finite original selections, including ignored overhang, remain separate source lease
members and are fully fingerprinted/revalidated; whole columns additionally retain
their original scopes/boundary verification. Cumulative limits include finite overhang.

Roles must be different single columns in the same physical workbook/worksheet with
nonempty overlap. Required/Auto require the effective first rows to match: otherwise
`input.long-header-alignment` asks for aligned header starts or explicit None. None
pairs every intersecting physical row as data. Interior blanks never shift pairings.
Thus F1:F10/E1:E12 with extra blank values succeeds. Ignored cells produce
`input.long-overhang`, role/count and nonblank count, without contents or addresses.
Populated overhang (including zero or empty text) is a prominent alert; blank overhang
is informational. The owner's matching-rows policy is followed without silently
claiming all selected observations contributed. Error locations map to the actual
role column, physical row and offset within its original effective source range.

ANOVA/KW use this service with existing minimum constraints. Future exactly-two-group
constraints are tested without a second picker/parser. A complete matched-block
policy rejects differently positioned blanks even when unpaired group counts match;
wide columns/rows and Common header interpretation are reused. This is input
completeness only: Friedman still needs explicit repeated/block roles, typed matched
input and its separately scheduled handler. Neither Mann–Whitney nor Friedman is
implemented/enabled here. Output presentation, host acquisition/capture and the
consent/writer remain separate from these analysis-specific constraints.

### D/E — Sticky shell and one explicit Write action

Only the selected tab panel scrolls in the viewport-height flex shell. Header,
readiness and tab strip remain visible; panels have min-height:0/overflow-y:auto,
focus padding and no body scroll trap. Existing ARIA/arrow/Home/End behavior remains.
Draft, active tab, panel scroll, result and live lease survive tab changes. The footer
notice scrolls with Analysis, leaving space at high text zoom. Development identity
and manifest remain distinct; Windows branding is untouched.

The primary **Write results** action owns fresh plan/target/source verification,
read-only destination inspection and (if empty/safe) native write within one bounded
operation. It never writes on Run. Occupied targets pause for compact No/Yes; Yes
uses single-use bound consent and fresh native verification, No never mutates.
An optional Inspect remains inside collapsed Advanced output check only. A declined
or rejected non-mutation can retain source/results only after another fresh source
check restores token-bound lifecycle callbacks; another mutation requires another
explicit Write click and new inspection/consent. Unverified continuity retires the run.
There is no automatic second mutation or retry after a possibly submitted operation.

Existing/other worksheet typed/picked start cells and named new sheet/start cell remain.
New Workbook remains disabled for Phase 04 exit review until populated-file creation,
independent-workbook handoff and verification exist. Completed retires the consumed
source/listeners and permits another independent analysis. Partial/uncertain outcomes
stay latched across Reset/method/tab/Retry/Run, with manual inspection/wait/reopen
guidance and no rollback claim. No native readback check or latch was weakened.

### Genuine Edge acceptance and unresolved writer evidence

**Not performed by this amendment:** authenticated Excel Online output screenshots,
native before/after comparison or genuine writer acceptance. A sanitized current
writerTrace was requested, but no initiating live stage/classification was available.
The genuine first-failing stage and exception category therefore remain **unknown**.
There is no evidence-based native recovery/readback fix to claim. Simulated success
and injected mismatches are not evidence about the owner's initiating exception.

Owner workflow: disposable workbook, untouched control sheet, genuine Edge frame
on 7141 and WasmSmoke separately on 7131. Use identical alpha/p-value presentation
settings in Windows/WasmSmoke and the production host's current effective defaults.

1. Capture the accepted reference ANOVA/Welch/post-hoc and KW/Dunn tables, settings
   and safe output geometry; then capture the production worksheet after Write,
   including header/fill/borders/alignment, significant/nonsignificant and small p-values.
   Compare stored numeric values separately from display. No tokenized URLs/private data.
2. Direct Run with typed/picked whole A:C and single columns on a sheet with spaces/
   accents; test sparse/distant values, zeros, interior blanks, errors, empty-string
   formulas and over-budget rejection without truncation. Check narrowing notes and
   original-scope overlap protection, plus changes outside old effective bounds.
3. Long F1:F10/E1:E12 with blank overhang, then a populated extra cell: matching-row
   parity and honest notices/alerts. Test None with different starts; Required/Auto
   reject different header starts, and diagnostics use true physical coordinates.
4. One click Write to empty/other sheet; occupied No then fresh Write/Yes without an
   unnecessary Run when continuity remains valid; edit between inspection/consent,
   protect/rename/delete/switch workbook and collide a new name. No wrong mutation.
5. Named new sheet: record exact state/code, MutationMayHaveOccurred, confirmed
   artifacts and safe writerTrace. After **verified Completed**, capture different
   source data and finish a second independent analysis in the same pane. Preserve
   reference/output/control screenshots and verify the control sheet unchanged.
6. If uncertain: stop, never retry that write. Enable the existing memory-only trace
   before one disposable attempt, record its first failing stage and safe exception
   classification, then disable tracing. A limited fix must address that proven
   category and retain strict readback/latch checks; do not unlock based on appearance.
7. Check scrolling/focus/tabs at 320–400 px and high zoom, optional validation/Reset,
   Retry/close-reopen and opt-in 04.02 Diagnostics. Recheck independent WasmSmoke.

### Current automated evidence and inventory

Both full VS 2026 solution builds and the repository Debug/Release gates exited 0.
The local NuGet cache was supplied as RestoreSources; the already configured
PythonExe was used without embedding an owner's home path. The commands were:

```powershell
# VS 2026 MSBuild discovered with vswhere [18.0,19.0), for each configuration:
& $msbuild BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:Platform=Any CPU' "/p:PythonExe=$env:PythonExe" /m
& $msbuild BESHStatNG.sln /t:Build /p:Configuration=Release '/p:Platform=Any CPU' "/p:PythonExe=$env:PythonExe" /m
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Release -VisualStudio 2026 -Legacy -Wasm -Js -Python
node --test BESHStat.OfficeAddin/tests/*.test.mjs
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
```

| Distinct suite | Debug passed | Release passed | Failed / skipped |
|---|---:|---:|---:|
| Core | 1,024 | 1,024 | 0 / 0 |
| Common | 727 | 727 | 0 / 0 |
| Windows legacy | 787 | 787 | 0 / 0 |
| Office C#/components | 303 | 303 | 0 / 0 |
| Wasm execution | 45 | 45 | 0 / 0 |
| Office JS | 325 | 325 | 0 / 0 |
| Help JS | 71 | 71 | 0 / 0 |
| WasmSmoke JS | 377 | 377 | 0 / 0 |
| Help Python/catalog/link checks | 27 + 16 + 5 | 27 + 16 + 5 | 0 / 0 |
| **Total distinct checks** | **3,707** | **3,707** | **0 / 0** |

The full regression scripts ran 301 Office C# cases; after two additional corrupt
normalization-reply guards/tests, both foundation scripts were rerun with **303/303**.
Those final TRX files supersede the 301-case files in this evidence set; repeated
executions are not added to distinct totals. Final explanatory Razor/CSS edits were
built again in Debug/Release (0 warnings/errors) and checked in the final browser run.
The 38-case amendment fixture includes golden Stage304 result/presentation parity,
header modes, physical row/column diagnostics, nonadjacent unequal vectors,
normalization boundaries, future constraint hooks and one-click output state.

The solution builds report **0 compiler warnings/errors**. Existing external/tool
notices remain: the Help builder skips one ambiguous Form+Title mapping and falls
back to Home; TRX overwrites and Git CRLF-to-LF notices are informational. No new
compiler warning or dependency advisory was reported by these offline-cache gates.
No network/package installation or signing was performed. Debug/Release Windows
post-build logging prints the guarded PreRelease signing script, but its condition
exits before signing for both configurations.

Evidence is retained under ignored `TestResults/04-04/format-input/`: `final/outcomes.txt`,
per-configuration solution/foundation/regression logs and TRX, `foundation-final.log`,
`pane-build.log`, standalone `node.log`, `final/audit.json`,
`final/readme-preservation.json` and `final/final-status.txt`. Package inspection
confirmed four nonempty packed XLLs (x86 PE 014C and x64 PE 8664, Debug/Release),
both Wasm host indexes/required assets and 12 fingerprinted Core/Common/host Wasm
assemblies. These are build/structure checks, not installed Excel execution.

Final simulated Edge evidence: all **14 Office scenarios** passed, including normal
pilot, uncertain pilot, range capture, options, standalone options, six readiness
states and two workbook scenarios, plus the new formatting/input workflow. The new
scenario uses actual Blazor/Common/Core with simulated Office, checks 320/360/400 px
at 100/200% **text sizing** (not a claim of native browser zoom testing), sticky
header/tab positions, focus, panel-only scrolling, preserved draft/tab context,
whole columns, blank/populated overhang, two full-column roles, guarded new-sheet
write, and an independent second analysis after verified simulated Completed.
`browser/*.json` records outcomes; screenshots include
`screenshots/sticky-400-200-percent-text.png` and
`screenshots/unequal-overhang-warning.png`. Earlier development captures are not
owner reference images. WasmSmoke separately on HTTPS 7131 passed its standalone
unavailable-Office/disabled-native-write/safe-yielding fixture checks with zero
runtime exceptions. No authenticated Excel workbook, genuine output screenshot,
native before/after comparison or owner acceptance is claimed.

All **15 original README byte prefixes** are retained. Fourteen are byte-identical
to preflight; Office README is appended only. VersionAttributes.vb SHA256 remains
`0F22C5A1AD446546E773E59B495E647C58F5FF08A70750AFE7372680798D2F2D`.
The branch remains `codex/04-04`, HEAD
`30d77acff28135cd00da145f38eb9bb708e991c7`. There are **63 changed/untracked file
paths**, comprising 54 pre-existing paths and nine additions; 34 paths were amended
in this request. Everything remains unstaged/uncommitted. The complete branch
inventory and final porcelain status are recorded below/in final-status.txt.

Files amended in this request (other existing branch edits remain preserved):

- `BESHStat.OfficeAddin/App.razor`
- `BESHStat.OfficeAddin/Components/AnalysisPilot.razor`
- `BESHStat.OfficeAddin/Components/RangePicker.razor`
- `BESHStat.OfficeAddin/Components/ValidationPreview.razor`
- `BESHStat.OfficeAddin/README.md`
- `BESHStat.OfficeAddin/Services/AnalysisPilotState.cs`
- `BESHStat.OfficeAddin/Services/GroupedInput.cs`
- `BESHStat.OfficeAddin/Services/IRangeCaptureHost.cs`
- `BESHStat.OfficeAddin/Services/OutputPlanPresentation.cs`
- `BESHStat.OfficeAddin/Services/PhysicalInputView.cs`
- `BESHStat.OfficeAddin/Services/RangeReferenceState.cs`
- `BESHStat.OfficeAddin/Services/WorkbookTransport.cs`
- `BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs`
- `BESHStat.OfficeAddin/tests/AnalysisPilotTests.cs`
- `BESHStat.OfficeAddin/tests/browser/format-input-browser.mjs`
- `BESHStat.OfficeAddin/tests/browser/options-browser.mjs`
- `BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs`
- `BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs`
- `BESHStat.OfficeAddin/tests/fixtures/table-presentation.json`
- `BESHStat.OfficeAddin/tests/FormatInputAmendmentTests.cs`
- `BESHStat.OfficeAddin/tests/helpers/pilotWorkbook.mjs`
- `BESHStat.OfficeAddin/tests/longIntersection.test.mjs`
- `BESHStat.OfficeAddin/tests/ProductionUxTests.cs`
- `BESHStat.OfficeAddin/tests/productionWriter.test.mjs`
- `BESHStat.OfficeAddin/tests/RangeReferenceTests.cs`
- `BESHStat.OfficeAddin/tests/tablePresentation.test.mjs`
- `BESHStat.OfficeAddin/tests/wholeColumns.test.mjs`
- `BESHStat.OfficeAddin/wwwroot/css/app.css`
- `BESHStat.OfficeAddin/wwwroot/js/productionWriter.mjs`
- `BESHStat.OfficeAddin/wwwroot/js/rangeCapture.mjs`
- `BESHStat.OfficeAddin/wwwroot/js/tablePresentation.mjs`
- `docs/architecture/batch-04-04-range-preview-destination-ui.md`
- `planning/BESHStatNG_CrossPlatform_MasterPlan.md`
- `scripts/verify-office-foundation.ps1`

Final `git status --short` (the fixtures directory contains the single listed
table-presentation.json fixture; `audit.json` enumerates files individually):

```text
 M BESHStat.Core.Tests/README.md
 M BESHStat.Core/README.md
 M BESHStat.Excel.Common.Tests/README.md
 M BESHStat.Excel.Common/README.md
 M BESHStat.OfficeAddin/App.razor
 M BESHStat.OfficeAddin/Components/AnalysisShell.razor
 M BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs
 M BESHStat.OfficeAddin/README.md
 M BESHStat.OfficeAddin/Services/AnalysisOptionDraft.cs
 M BESHStat.OfficeAddin/Services/WorkbookAdapter.cs
 M BESHStat.OfficeAddin/Services/WorkbookTransport.cs
 M BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs
 M BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs
 M BESHStat.OfficeAddin/tests/browser/options-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
 M BESHStat.OfficeAddin/tests/workbookAdapter.test.mjs
 M BESHStat.OfficeAddin/tests/workbookWriter.test.mjs
 M BESHStat.OfficeAddin/wwwroot/css/app.css
 M BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs
 M BESHStat.WasmSmoke/README.md
 M BESHStatNG.Test/README.md
 M BESHStatNG_Help_MkDocs/README.md
 M BESHStatNG_Installer/README.md
 M BESHstatNG/README.md
 M README.md
 M planning/BESHStatNG_CrossPlatform_MasterPlan.md
 M scripts/verify-office-foundation.ps1
?? BESHStat.OfficeAddin/Components/AnalysisPilot.razor
?? BESHStat.OfficeAddin/Components/OutputPicker.razor
?? BESHStat.OfficeAddin/Components/RangePicker.razor
?? BESHStat.OfficeAddin/Components/RangeReferencePicker.razor
?? BESHStat.OfficeAddin/Components/ValidationPreview.razor
?? BESHStat.OfficeAddin/Services/AnalysisPilotState.cs
?? BESHStat.OfficeAddin/Services/GroupedInput.cs
?? BESHStat.OfficeAddin/Services/IPilotHost.cs
?? BESHStat.OfficeAddin/Services/IRangeCaptureHost.cs
?? BESHStat.OfficeAddin/Services/OutputPlanPresentation.cs
?? BESHStat.OfficeAddin/Services/PhysicalInputView.cs
?? BESHStat.OfficeAddin/Services/RangeReferenceState.cs
?? BESHStat.OfficeAddin/tests/AnalysisPilotTests.cs
?? BESHStat.OfficeAddin/tests/FormatInputAmendmentTests.cs
?? BESHStat.OfficeAddin/tests/ProductionUxTests.cs
?? BESHStat.OfficeAddin/tests/README.md
?? BESHStat.OfficeAddin/tests/RangeReferenceTests.cs
?? BESHStat.OfficeAddin/tests/browser/format-input-browser.mjs
?? BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs
?? BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs
?? BESHStat.OfficeAddin/tests/fixtures/
?? BESHStat.OfficeAddin/tests/helpers/pilotWorkbook.mjs
?? BESHStat.OfficeAddin/tests/longIntersection.test.mjs
?? BESHStat.OfficeAddin/tests/multiSource.test.mjs
?? BESHStat.OfficeAddin/tests/productionWriter.test.mjs
?? BESHStat.OfficeAddin/tests/rangeCapture.test.mjs
?? BESHStat.OfficeAddin/tests/tablePresentation.test.mjs
?? BESHStat.OfficeAddin/tests/wholeColumns.test.mjs
?? BESHStat.OfficeAddin/wwwroot/js/productionWriter.mjs
?? BESHStat.OfficeAddin/wwwroot/js/rangeCapture.mjs
?? BESHStat.OfficeAddin/wwwroot/js/tablePresentation.mjs
?? docs/architecture/README.md
?? docs/architecture/batch-04-04-range-preview-destination-ui.md
?? planning/README.md
?? scripts/README.md
```

## 23 October production UX amendment (04.04 remains pending)

This section supersedes the older acquisition workflow and automated evidence below.
The separate genuine-Excel writer investigation remains open: no live evidence
identifies its initiating exception/readback mismatch. This amendment does not
resolve that blocker. Revision 1.8, Gate B E1–E7 and **223 planned / 36 accepted /
187 outstanding** are unchanged; 04.05 remains unstarted.

### Preflight and preservation

The existing branch was `codex/04-04`, HEAD
`30d77acff28135cd00da145f38eb9bb708e991c7`, with substantial existing edits.
Git required a command-scoped safe.directory exception because the sandbox SID
differs from the owner; no global configuration was changed. No branch creation,
reset, staging, commit, push, merge, deploy, installer, signing or target change ran.
The named ZIP was not present in the repository; current files remained authoritative.

`TestResults/04-04/production-ux-preflight/inventory.json` records byte lengths and
SHA-256 hashes, with byte copies, of all pre-existing changed/untracked files and
all 15 README files. Office README changes are additive; all other README bytes
are retained. Generated VersionAttributes.vb SHA-256 was
`0F22C5A1AD446546E773E59B495E647C58F5FF08A70750AFE7372680798D2F2D`;
verification saves/restores these exact bytes.

The reported previous workflow used one input for all arrangements, integer long
roles, mandatory Acquire before Run, persistent preview and main-page read/probe
diagnostics. Actual preflight already contained a partial two-range/direct-Run/tab
scaffold. It also mounted a duplicate AnalysisShell inside Diagnostics and failed
test compilation on three superseded SetInterpretation calls. Existing writer
uncertainty/tracing safeguards were present and are preserved.

### UI and owned operation state

- Analysis is the default tab. One mounted AnalysisShell retains method/options,
  input/output drafts, live source and consent when Diagnostics is opened. Panels
  use HTML hidden, tablist/tab/tabpanel, selected/controls/labelledby, roving tab
  stops, arrows/Home/End and visible focus. Tabs initiate no probes, subscriptions,
  workbook reads or writes. Availability/Retry remains accessible above the tabs.
- Arrangement precedes Header mode and RefEdit fields. Columns/rows expose one
  rectangle; long format exposes independent category/value range pickers with no
  integer role editor. Manual qualified A1, arm/Finish/Cancel and explicit selected
  range capture fallback remain. Category ID denotes a group, not a subject ID.
- Run owns one 30-second operation/cancellation/generation boundary: validate the
  registered method/options, unarmed capture and all syntax; preflight cumulative
  budgets; resolve/read fresh snapshots; validate through Common; pin/recheck all
  sources; then call the existing Common handler and semantic output document.
  There is no nested public Operation while Busy and no copied statistics/grammar.
  Production Acquire/Use current selection/Refresh prerequisite buttons are removed.
  The old service acquisition seam remains for regression compatibility only.
- Optional Validate uses the same private read/parser path, with no statistics,
  output or overwrite consent. Explicit validation requests show redacted counts
  and issues. Failed Runs show alerts and focus errors. Successful Runs without
  an outstanding optional validation request show compact result readiness; an
  error-only preview closes on successful retry. Relevant edits clear stale issues.
- Reset keeps group/method and restores input, arrangement, headers, statistical
  options and output defaults; it does not reinitialize Office or clear uncertainty.
  Editing, interpretation changes, Cancel, Reset, Retry and disposal retire stale
  capture ownership and derived results. A timed-out native metadata sync retains
  its lease until native settlement. Pane reopen still starts a fresh scope.
- Diagnostics holds Office/platform and Core/Common checks. Read-only development
  controls/stage summaries require Debug plus `?beshOfficeDiagnostics=1`. Redaction
  and bounded opt-in storage remain. Raw values/formulas/labels/names/addresses are
  excluded from diagnostics; references stay inside intended editable fields.

### Two physical ranges, Common reuse and lifecycle

Long roles must be distinct single columns in the same physical workbook/worksheet,
with equal lengths and exactly equal start/end rows. External workbook, multi-area,
shape/alignment and physical snapshot mismatches reject before compute/write with
role-specific guidance. PhysicalInputView pairs unfiltered cells by original row
into a parser-only two-column view with no invented worksheet/address/workbook
identity. GroupedNumericRangeParser.ParseWithDiagnostics/configurable Common parser
receives canonical roles 0/1 and owns grouping, Required/None/Auto and blank/error
policy. There is no independent blank removal, row shifting or bounding rectangle.
Separate physical snapshots/handles map issues to actual worksheet rows/columns.

Additive ICompositeSourceHost/PhysicalSource passes both real snapshots and handles
to native pinSources. It verifies both handles against the same Office document
object, exact sheet IDs/names and geometry; watches lifecycle events; compares both
contents/formulas/formats against acquisition. One logical source token/session
binds both per-range fingerprints, selection metadata and shared lifecycle revision
(epoch). Either member's change invalidates the whole run/consent. Both fingerprints
and selection/identity are rechecked before computation, before destination
inspection and again before write. Overlap tests each real physical range, never
the synthetic pair. Missing identity/events/readback evidence fails closed.

Combined input cells (10,000), actual estimated bytes (64 MiB), chunks and the single
operation deadline remain bounded. Native verification includes formula bytes across
both roles. Changed/renamed/deleted/replaced sheets, different document objects,
selection changes, Cancel and disposal reject stale runs or consent. Deadline
expiry remains `input.time-budget`, including a delayed composite pin.

### Output and unresolved genuine writer blocker

Run produces an in-pane preview. Save explicitly chooses an existing worksheet with
a typed/picked qualified start cell (also another sheet), or a new sheet name/start
cell; Inspect; then Write or occupied-target Yes/No. Inspection never creates a
sheet. Every write consumes inspection/consent once and the native writer performs
fresh source/destination occupancy verification. New Workbook stays disabled. No
automatic approval/retry, overlap exemption or rollback claim is introduced.

Verified Completed retires old source/consent/listeners and permits an independent
Run in the same pane. PartiallyWritten/MutationMayHaveOccurred retains the native
sticky latch. Additive IPilotSafetyHost also retains the outcome/recovery state for
new method components in the pane. Reset, methods, tabs, Retry and Run cannot release
it. Recovery remains manual workbook inspection, wait for native settlement, never
retry that write, then close/reopen. Sanitized native stage tracing and truthful
late cancellation remain. No speculative color/format/readback normalization was added.

### Required owner acceptance in genuine Edge Excel Online

Use a disposable workbook and untouched control sheet; simulations below are not
authenticated Excel acceptance:

- Parametric → One-Way ANOVA, columns A1:C4, directly Run without Validate; check
  numeric result and absent preview. Repeat equivalent rows and compare KW/options.
- Separately capture nonadjacent E1:E10/J1:J10 and type full qualified references;
  compare adjacent numerics for Required/None/Auto. Reject mismatched rows/sheets,
  duplicate roles, multi-area/rectangular picks and erroneous cells. Validate
  optionally; verify physical coordinates, paired blanks and recovery after fixes.
- Check method/options navigation, Reset, tab keyboard/focus/narrow layout, live
  draft/output preservation, Retry and fresh close/reopen. Probes/read controls
  stay in Diagnostics; explicitly verify the opt-in 04.02 read diagnostic.
- Write to empty existing/other sheet, occupied No/Yes and named new sheet. Record
  exact outcome/code, MutationMayHaveOccurred, confirmed artifacts and table/style/
  readback. After verified Completed on a new sheet, select different source data
  and finish another independent analysis in the same pane. If uncertain, stop,
  retain safe native stage evidence and never retry that write. Control sheet intact.
- Independently verify WasmSmoke on 7131; New Workbook remains unavailable. Both
  genuine writer evidence and production UX smoke remain owner acceptance blockers.

### Automated verification and complete file inventory

Final verification exited **0** for all four required script/configuration runs and
the separate Node run. **3,619 distinct tests passed in Debug and 3,619 in Release**
(55 more than the previous reported checkpoint); none failed or skipped. Repeated
foundation/standalone Node executions are not counted a second time.

| Suite | Debug | Release |
| --- | ---: | ---: |
| Core | 1,024 | 1,024 |
| Common | 727 | 727 |
| Windows legacy, VS 2026 | 787 | 787 |
| Office C# services/components | 265 | 265 |
| WasmSmoke execution C# | 45 | 45 |
| Office Node | 275 | 275 |
| Help resolver Node | 71 | 71 |
| WasmSmoke Node | 377 | 377 |
| Python tools / Help / discovery | 27 / 16 / 5 | 27 / 16 / 5 |
| **Distinct total** | **3,619** | **3,619** |

Commands actually run from the repository root (PythonExe was already configured;
the existing local NuGet cache was selected via RestoreSources, without installs):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Release -VisualStudio 2026 -Legacy -Wasm -Js -Python
node --test BESHStat.OfficeAddin/tests/*.test.mjs
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
```

`TestResults/04-04/production-ux/final/{Debug,Release}` contains full script logs and
per-suite TRX reports; `final/outcomes.txt` and `final/node.log` retain command exits
and the standalone 275/275 Node result. `run-gates.ps1` saves/restores version bytes
in finally. Debug foundation was rerun after the final timeout-alert/outcome wording
corrections; the full Release regression also passed those sources. Neither check
changes Core/Common or Windows framework targets. Final logs contain no package/
compiler advisory. The initial no-restore preflight carried cached NU1900 warnings
about an unavailable NuGet vulnerability feed; current online package audit data
was not retrieved. Git's CRLF-to-LF notices are not whitespace errors or source resets.

`final/audit.json` verifies both hosts in Debug/Release: index/static assets and one
fingerprinted WASM assembly each for Core, Common and the host (12 assembly checks).
All four packed XLLs are nonempty PE files with the expected machine types:

| Configuration | Packed product | Bytes | PE machine |
| --- | --- | ---: | --- |
| Debug | BESHStatNG-AddIn-packed.xll | 1,948,672 | 0x014C |
| Debug | BESHStatNG-AddIn64-packed.xll | 1,874,432 | 0x8664 |
| Release | BESHStatNG-AddIn-packed.xll | 1,879,552 | 0x014C |
| Release | BESHStatNG-AddIn64-packed.xll | 1,805,312 | 0x8664 |

Artifact SHA-256 values are retained in the audit; no signing or PreRelease build ran.
Both Wasm hosts remain independent. No installer, live workbook, deployment or
production add-in manifest workflow was executed.

All **13 isolated Edge Office scenarios** passed: pilot, pilot-uncertain, range-capture,
options Excel/standalone, six readiness cases, workbook and pending-workbook. The
three affected pilot/range scenarios were rerun successfully after the final UI
corrections. `TestResults/04-04/production-ux/browser` contains JSON results and exits.
An additional standalone Edge WasmSmoke check on **7131** passed with native write
disabled, no application exceptions and a Completed safe yielding fixture. Screenshots
at 360 px were reviewed: no horizontal clipping, normal vertical scrolling, correct
two-role inputs and hidden successful unrequested preview. The isolated profile was
not authenticated. The owned isolated browser and both local hosts were shut down
after verification. Early browser startup against a server launched before rebuilding
WASM failed; restarting against the rebuilt assets passed. This is development-host
evidence, not a conclusion about the unresolved native Excel writer.

New focused coverage includes direct-Run wide/row/long golden parity (both handlers,
all headers), separate physical role mapping, paired blanks/errors, invalid shapes/
references/alignment/workbooks, cumulative cells/bytes/formula bytes, a real 30-second
composite-pin deadline, capture ownership and a real five-second native capture
timeout lease, tab/draft/consent preservation, Reset and pane-scoped uncertain state.
Existing No/Yes, other/new-sheet, source-rebound, late cancellation, verified Completed
versus uncertain, one-mutation and subsequent-independent-run tests remain passing.

Tracked `git diff --check` exits 0. All 26 untracked delivered paths were also checked
against an empty file: no whitespace diagnostics (no-index exit 1 denotes a difference).
All 14 other README files are byte-identical to preflight; the entire original Office
README is an unchanged prefix of its additive update. Generated version bytes match
the preflight SHA-256 above. The final inventory has **54 branch paths**, **27 touched
by this amendment**, with hashes/pre-existing flags in `final/audit.json`. Remaining
pre-existing edits are deliberately retained. Full final `git status --short`:

```text
 M BESHStat.Core.Tests/README.md
 M BESHStat.Core/README.md
 M BESHStat.Excel.Common.Tests/README.md
 M BESHStat.Excel.Common/README.md
 M BESHStat.OfficeAddin/App.razor
 M BESHStat.OfficeAddin/Components/AnalysisShell.razor
 M BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs
 M BESHStat.OfficeAddin/README.md
 M BESHStat.OfficeAddin/Services/AnalysisOptionDraft.cs
 M BESHStat.OfficeAddin/Services/WorkbookAdapter.cs
 M BESHStat.OfficeAddin/Services/WorkbookTransport.cs
 M BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs
 M BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs
 M BESHStat.OfficeAddin/tests/browser/options-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
 M BESHStat.OfficeAddin/tests/workbookAdapter.test.mjs
 M BESHStat.OfficeAddin/tests/workbookWriter.test.mjs
 M BESHStat.OfficeAddin/wwwroot/css/app.css
 M BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs
 M BESHStat.WasmSmoke/README.md
 M BESHStatNG.Test/README.md
 M BESHStatNG_Help_MkDocs/README.md
 M BESHStatNG_Installer/README.md
 M BESHstatNG/README.md
 M README.md
 M planning/BESHStatNG_CrossPlatform_MasterPlan.md
 M scripts/verify-office-foundation.ps1
?? BESHStat.OfficeAddin/Components/AnalysisPilot.razor
?? BESHStat.OfficeAddin/Components/OutputPicker.razor
?? BESHStat.OfficeAddin/Components/RangePicker.razor
?? BESHStat.OfficeAddin/Components/RangeReferencePicker.razor
?? BESHStat.OfficeAddin/Components/ValidationPreview.razor
?? BESHStat.OfficeAddin/Services/AnalysisPilotState.cs
?? BESHStat.OfficeAddin/Services/IPilotHost.cs
?? BESHStat.OfficeAddin/Services/IRangeCaptureHost.cs
?? BESHStat.OfficeAddin/Services/PhysicalInputView.cs
?? BESHStat.OfficeAddin/Services/RangeReferenceState.cs
?? BESHStat.OfficeAddin/tests/AnalysisPilotTests.cs
?? BESHStat.OfficeAddin/tests/ProductionUxTests.cs
?? BESHStat.OfficeAddin/tests/README.md
?? BESHStat.OfficeAddin/tests/RangeReferenceTests.cs
?? BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs
?? BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs
?? BESHStat.OfficeAddin/tests/helpers/pilotWorkbook.mjs
?? BESHStat.OfficeAddin/tests/multiSource.test.mjs
?? BESHStat.OfficeAddin/tests/productionWriter.test.mjs
?? BESHStat.OfficeAddin/tests/rangeCapture.test.mjs
?? BESHStat.OfficeAddin/wwwroot/js/productionWriter.mjs
?? BESHStat.OfficeAddin/wwwroot/js/rangeCapture.mjs
?? docs/architecture/README.md
?? docs/architecture/batch-04-04-range-preview-destination-ui.md
?? planning/README.md
?? scripts/README.md
```

Not run: authenticated Excel Online/control-sheet acceptance, Windows/Mac live Excel
UI/write acceptance, signing, deployment or installers. Simulations do not identify
the real writer's initiating native exception or certify Excel readback/style behavior.
**04.04 remains pending owner review: 36 / 223 accepted, 187 outstanding.** Older dated
evidence below is historical and is not a new pass claim for this amendment.

## 22 October amendment: capture after new-sheet output

This section supersedes earlier lifecycle/test evidence below for the amendment.
The branch remains `codex/04-04`, uncommitted and pending owner acceptance;
**36 / 223 accepted** is unchanged. No 04.05 work is included.

### Confirmed cause and remaining live investigation

The confirmed cause of the blocked reference textbox is the production writer's
sticky `uncertain` latch: a write exception after `op.mutation` rejects subsequent
`beginCapture`, `captureSelection`, `resolveReference` and `pinSource` calls with
`output.mutation-uncertain`. This is an output/session outcome, not an A1 parser
failure or proof of a source-sheet edit. A populated sheet does not prove that all
tables and semantic formatting were verified.

**The initiating exception/mismatch in the owner's first genuine Excel write is
still unknown.** No authenticated Excel Online debugging session was available.
The owner-reported observation has no recorded first `WriteOutcome.State`, code,
mutation flag or confirmed-artifact count. Automated Edge tests below use fake
Office/Excel objects and cannot establish genuine Excel success or an exact native
readback representation. No format/color/value normalization was added on this
evidence. Hex-color case folding already existed and remains the only color
equivalence admitted; named colors and changed formats remain uncertain.

The fake workbook already emitted `onAdded`/`onChanged` during passing writes.
New tests additionally emit addition/change/activation/selection events during
mutation sync and readback, and dispatch saved callbacks asynchronously after a
second pin. `watch()` does increment the epoch during its own queued mutation,
but execute's post-mutation verification deliberately does not check that epoch.
It verifies the actual output instead. Thus those events alone do **not** explain
the owner's first uncertainty in the tested implementation. External events still
invalidate pre-mutation source/target/consent. Events were not globally disabled.

Initial commands checked branch, status and whitespace; command-local
`-c safe.directory=C:/Users/peter/source/repos/BESHstatNG` was needed for the
sandbox account's Git ownership check. No global Git setting changed. Existing
04.04 work and every owner's README edit were retained. The specifically named
`BESHstatNG(20261022-183200).zip` was not found in the workspace/parent/Downloads
locations checked. A separately named `../BESHstatNG.zip` was available: before
editing, writer, pilot state, workbook transport and Office README were each
byte-identical to its entries. This is a limited independent comparison, not a
claim to have inspected the unavailable named ZIP. The working tree stayed
authoritative. Evidence: `TestResults/04-04/uncertain-amendment/archive-comparison.json`.

### Changes and recovery

- `wwwroot/js/productionWriter.mjs`: opt-in bounded stage tracing; retain the busy
  lease until the actual native Promise settles after a timeout. A timed-out
  mutation stays uncertain even if it later finishes. Verified completion is
  recorded before subscription cleanup, so late Cancel/cleanup timeout cannot
  downgrade it. Successful writes retire the source and remove old listeners,
  grouped by their owning native context. Saved old closures cannot affect a new
  source. Uncertain outcomes retain the fail-closed latch, retire the consumed
  source, and never retry/roll back. Pre-mutation rejection remains
  `RejectedBeforeWrite`; queued but unverified mutation remains `PartiallyWritten`.
- `wwwroot/js/rangeCapture.mjs`: retire active source-reference pinning after a
  consumed write. Bounded read-only identity hints remain for explicit Refresh,
  including deleted/recreated-sheet rejection; they are not source/consent leases.
  Each new source pin generates a new token; old plan/inspection/consent cannot
  authorize it.
- `Services/WorkbookTransport.cs`: source invalidation callbacks carry the pin's
  token. C# accepts only the active token and generation, retiring it on write or
  invalidation. A late old callback or late pin reply cannot invalidate/rebind
  the next operation. This private callback change does not change Common wire v1.
- `Services/AnalysisPilotState.cs`: retain the previous write outcome while drafts,
  destination or Retry change; release private source/session/consent after a write.
  Record a returned outcome even after late Cancel/invalidation. After an uncertain
  mutation, the current form refuses acquisition and retains explicit recovery
  guidance. Navigation does not clear the underlying writer latch.
- `Components/RangeReferencePicker.razor`: when capture returns uncertainty, show
  the same recovery instruction next to the reference status. This also covers
  a newly selected analysis whose previous component has been disposed.
- Focused C#, rendered-component, writer/fake-host and simulated-browser regressions
  exercise independent second runs, consumed consent, delayed events, timeouts,
  disposal, Retry/Cancel, uncertainty, verification mismatches and redaction.

Recovery after a genuinely uncertain write is intentionally unchanged in scope:
**inspect the workbook manually, wait for the previous native operation to finish,
then close and reopen the task pane before another operation**. There is no reset
button, automatic retry, rollback or read-only bypass of the write latch. The pane
does not prove settlement across a reload; the owner must ensure it before reopening.
Completed writes allow a fresh source in the same pane. A timeout during listener
cleanup can return already verified Completed while briefly reporting `host.busy`
until cleanup settles. Cleanup failure leaves retired callbacks inert; native
event unregistration is best effort when the host is closed/unresponsive.

### Sanitized stage tracing and genuine owner acceptance

In Edge DevTools, select the **localhost:7141 task-pane frame**, not Excel's parent
page. Before the first explicit output write, run:

```javascript
const writerTrace = await import('/js/productionWriter.mjs');
writerTrace.enableWriterTrace();
```

This is a memory-only opt-in; it does not read or modify workbook cells. It keeps
the latest 256 records, with relative operation timings clamped to 0–60,000 ms.
Stages include `worksheet.add.before`, `mutation.sync.start/end`,
`readback.sync.start/end`, start/result for each of `worksheet`, `values`,
`formulas`, `number-format`, `font-bold`, `font-color`, `writer.result`,
`capture.guard`, lifecycle events and timeout/failure classification. Result
records include numeric state, mutation flag and confirmed-artifact count.
Native sync exceptions are distinguished from adapter exceptions, safety checks
and event-driven pre-mutation invalidation. Verification mismatch records identify
the category, never the cell. Color mismatch adds only a representation class
(`hex6`, `letters`, other string/type), never the actual encoding. No native error
messages/codes, sheet names/IDs, addresses, values, formulas, URLs or credentials
are retained; standard UI messages stay redacted. The trace does not classify an
unknown partial mutation as a verified successful write.

Use a disposable workbook with untouched control cells:

1. **New-sheet ANOVA:** acquire source A, preview/Run, choose an unused new-sheet
   name, Inspect, then explicitly Write. Before editing/navigating, record the exact
   **Write outcome** state/code, **Mutation may have occurred** and **Confirmed
   artifacts** count. Inspect all result tables, formats, bold/red significance,
   literal `=` text and blank cells locally. If uncertain, stop before retrying.
2. Export only the sanitized trace with
   `copy(JSON.stringify(writerTrace.getWriterTrace(), null, 2))`. A sync start
   without its end plus a native failure identifies that boundary; readback end
   followed by a category mismatch identifies failed verification. Lifecycle
   records show whether invalidation happened before or after mutation was queued.
   An exception after queuing is uncertainty, not proof that a particular subset
   was written. Keep any workbook inspection data private.
3. **Only after Completed:** return to the input sheet, select a different source B
   (wide or long), Select range/Finish, acquire and run ANOVA/KW without closing
   the pane. Inspect/write to a different destination. Verify the second result,
   unchanged first result and unchanged control cells. Repeat with an empty
   existing-sheet destination and occupied output with explicit Yes.
4. Recheck occupied No, duplicate new-sheet names, rename/delete, stale source and
   consent, Cancel, Retry, disposal/reopen, Office probes, 04.02 diagnostics and
   04.03 navigation. These actions alone must not write. After an actual uncertain
   outcome, inspect and establish native settlement before reopening; fresh capture
   should then work. Do not induce a real partial write by retrying an uncertain one.
5. For a genuine readback mismatch, compare native and expected structures locally:
   matrix dimensions/scalar types, blank/Boolean/text literals versus formula areas,
   number-format representation, font Boolean/color representation and exact native
   sheet name/ID. Report only category/type/equivalence evidence. A targeted semantic
   normalization requires this evidence plus a regression; none is assumed here.
   Run the separate WasmSmoke accepted smoke independently.

Disable and erase the trace with `writerTrace.enableWriterTrace(false)`.
The requested genuine A–D acceptance remains **not run / owner interaction needed**.

### Amendment validation evidence

Evidence and exact commands/results are retained under
`TestResults/04-04/uncertain-amendment`; final validation counts are recorded below.
The only documentation edited by this amendment is this report and an appended
Office README section. No Core/Common statistics/contracts, target frameworks,
master-plan counts, WasmSmoke UI, Windows public entry points or generated metadata
were changed. All existing owner README bytes are preserved as prefixes.

Final commands, from the repository root (all exit **0**):

```powershell
$env:RestoreSources = 'C:/Users/peter/.nuget/packages'
$env:PythonExe = 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe'
foreach ($configuration in @('Debug', 'Release')) {
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration $configuration
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration $configuration -VisualStudio 2026 -Legacy -Wasm -Js -Python -PythonExe $env:PythonExe
}
# Final Debug foundation recheck after the last callback/writer changes:
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
node --test BESHStat.OfficeAddin/tests/productionWriter.test.mjs
powershell -NoProfile -ExecutionPolicy Bypass -File TestResults/04-04/uncertain-amendment/run-browsers.ps1
& $env:PythonExe -B TestResults/04-04/uncertain-amendment/audit.py
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
```

The ignored `run-gates.ps1` captures per-configuration logs/TRX under `final` and
saves/restores generated version bytes in `finally`. `run-browsers.ps1` records
each scenario's exit code: pilot, pilot `--uncertain`, range capture, options,
options `--standalone`, six readiness fixtures (`excel`, `pending`, `unsupported`,
`missing`, `dependency`, `bridge-regression`), workbook and workbook `--pending-sync`.
The three writer/range browser scenarios were also rerun after the final writer
changes; their final JSON is in `final/browser`. These are **13 simulated browser
scenarios**, excluded from the distinct unit-case totals. The focused writer suite
contains 81 cases, included in the Office Node count below, not added again.

| Distinct cases (count each suite once) | Debug | Release |
|---|---:|---:|
| Core | 1,024 | 1,024 |
| Common | 727 | 727 |
| Legacy .NET Framework | 787 | 787 |
| Office C# services/components | 240 | 240 |
| WasmSmoke execution C# | 45 | 45 |
| Office / help / WasmSmoke Node | 245 / 71 / 377 | 245 / 71 / 377 |
| Python tools / help / scripts | 27 / 16 / 5 | 27 / 16 / 5 |
| **Distinct total, all passed; no skipped cases** | **3,564** | **3,564** |

No unrelated test failures remain. One interim new C# regression fixture used an
unqualified existing-sheet reference and was corrected to the existing qualified
reference contract; no parser or existing test was weakened. One later browser
startup failed after a Debug rebuild while the old dev server was still running
(`browser-stale-build-startup.json`, eight startup exceptions, no analysis/write).
Restarting that host against current assets, with no production code change,
restored passing writer/range browser checks. This is consistent with a stale
fingerprinted-asset snapshot, not evidence about genuine Excel behavior. The
sandbox account lacked the owner's HTTPS developer certificate; approved host
execution used the existing certificate. Isolated headless Edge used a new local
profile without authentication. No install, certificate generation/trust change,
signing, deployment or live workbook operation was performed. Git's existing
CRLF notices/TRX overwrite notices are not test failures.

Read-only MZ/PE checks passed for all four newly built packed XLLs; none was loaded:

| Configuration | x86 packed bytes / machine | x64 packed bytes / machine |
|---|---:|---:|
| Debug | 1,948,160 / `0x014C` | 1,873,920 / `0x8664` |
| Release | 1,880,064 / `0x014C` | 1,805,824 / `0x8664` |

`final/audit.json` records those headers/sizes, distinct TRX/log counts, all browser
exit codes, README preservation and version hash. Every other baseline README is
byte-identical. The Office README retains its original **8,084-byte** prefix.
Generated `BESHstatNG/Generated/VersionAttributes.vb` remains byte-identical with
SHA-256 `0F22C5A1AD446546E773E59B495E647C58F5FF08A70750AFE7372680798D2F2D`.
Final `git diff --check` passes; only the following amendment paths were edited
on top of the owner's existing uncommitted 04.04 tree:

```text
BESHStat.OfficeAddin/Components/RangeReferencePicker.razor
BESHStat.OfficeAddin/Services/AnalysisPilotState.cs
BESHStat.OfficeAddin/Services/WorkbookTransport.cs
BESHStat.OfficeAddin/wwwroot/js/productionWriter.mjs
BESHStat.OfficeAddin/wwwroot/js/rangeCapture.mjs
BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs
BESHStat.OfficeAddin/tests/AnalysisPilotTests.cs
BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs
BESHStat.OfficeAddin/tests/productionWriter.test.mjs
BESHStat.OfficeAddin/tests/helpers/pilotWorkbook.mjs
BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs
BESHStat.OfficeAddin/README.md
docs/architecture/batch-04-04-range-preview-destination-ui.md
```

Not run: authenticated Excel Online A–D acceptance, real Office readback comparison,
desktop/Mac Excel, live packed-XLL loading, or the separate interactive WasmSmoke
workbook smoke. Automated WasmSmoke build/execution/JS gates did pass. The genuine
two-operation and recovery instructions above are the required next owner checks.
No numerical, Common v1/E1–E7 or overall batch-count change is claimed.

## Source-to-owner pipeline and audit

| Boundary | Actual implementation / authority |
|---|---|
| Method and draft | Accepted `AnalysisNavigationMenu` + verified `AnalysisNavigationCatalog` + `AnalysisEditorRegistry`; exact stable descriptor ID/schema/type; `AnalysisOptionDraft.Snapshot()` creates typed copies |
| Explicit range acquisition | `RangePicker` → component-owned `AnalysisPilotState.AcquireAsync` → existing `IWorkbookAdapter.ReadSelectionAsync` or identity-resolved `ReadRangeAsync`; the accepted bounded/tiled reader returns `SpreadsheetRangeData` |
| Validated typed input | Private defensive range copy → `GroupedNumericRangeParser.ParseWithDiagnostics(..., SpreadsheetGroupedParseOptions)` with explicit orientation/header/roles → `GroupedNumericData` and `SpreadsheetValidationReport` |
| Source/lifetime binding | New `IPilotHost.PinSourceAsync` → `WorkbookTransport` → installed `productionWriter.mjs`; actual Office document-object continuity, worksheet IDs/names, selection geometry, source values/formulas/types/formats, sticky lifecycle events and a pane-local token |
| Explicit analysis | `RunAsync` verifies source/draft/descriptor again → `SharedAnalysisApplicationService.RunDocument` → verified Common handler registry → unchanged Core numerics → actual `AnalysisResultDocument` |
| Read-only result preview | `OutputDocument.FromAnalysis` retains the complete ordered tables and diagnostics; bounded metrics/table dimensions/codes are displayed, without cell/title/group-label disclosure |
| Target/semantic/layout plan | `SpreadsheetOutputTarget`, `HostCapabilityPreflight` through `TargetPlanner.Create`, `OutputSemanticPlanner.Create`, `OutputLayoutPlanner.Create`, `OfficeOutputPlanAdapter.Prepare(autoFit:false)`; current/new sheet only |
| Live target/occupancy | New `PrepareOutputAsync` inspects actual target values, formulas, formats, worksheet/workbook protection, merged/spilled cells, table/pivot conflicts and identities; returned typed facts pass `TargetPlanner` again; Common `WriteIntent` / `WriteInspection` bind exact regions |
| Explicit consent and mutation | Common `WriteConsent` for occupied output, or explicit empty-target Write button → unchanged `IWorkbookAdapter.WriteAsync` signature → installed production writer → fresh source/target/observation verification → bounded native values/formats/fonts → sync and actual readback → truthful Common `WriteResult` |

New host-specific pieces are the pane operation coordinator, private pilot interop
DTOs, event subscriptions/callback, target/occupancy inspector, production writer
and reusable capture/range/interpretation/preview/output components. No direct Office.js calls appear in Razor.
There is no copied statistical implementation or second A1 grammar. Output start
cells use the existing Common `SpreadsheetRangeAddress.Parse`; the writer generates
A1 only from checked integer bounds. Manual input/output references share Common parsing and metadata-only native resolution.

Read AGENTS.md, the current master plan/04.04 row, accepted 04.02/04.03 reports and
OfficeAddin README/source/tests; audited the following existing contract families:

- `SpreadsheetContracts.vb`, `SpreadsheetRangeReferences.vb`, input limits/cell
  policies, grouped/configured parsers and structured validation. Common retains
  legacy header auto-labels and independently skipped missing observations.
- `SharedAnalysisApplicationService`, descriptor/handler registry, typed options,
  ANOVA/KW application services, `AnalysisResultDocument` and safe fault mappings.
- `OutputDocument`, `Styles`, `PValuePresentation`, `OutputLayoutPlanner`,
  `TargetPlanner`, `OfficeOutputPlanAdapter`, `WriteResult`, and the separate
  `OfficeCompositeOutputPlanAdapter`; host capability and lifecycle contracts.
- Production reader, transport, diagnostics, App, navigation state/drafts/editors;
  authored `methods.yml` and its unchanged generated metadata.
- WasmSmoke's `Home.razor` orchestration, real-host writer/inspection/consent
  implementation, existing output fixtures and lifecycle safeguards, **read-only**.
  No WasmSmoke host logic or production UI was copied or modified.

`Get-Help ... -Detailed` could not find script help metadata in this environment.
Both maintained parameter blocks were read directly before invocation:
foundation accepts Configuration; complete verification accepts Configuration,
VisualStudio, Legacy, Wasm, Js, Python and PythonExe. No help update/install ran.

## Git baseline and authorized scope

Clean initial `main`, HEAD **`30d77acff28135cd00da145f38eb9bb708e991c7`**, “Implement
method navigation UI”. Verified both dropdowns, typed options, tests and accepted
04.02/04.01 parents in source/history. Owner confirms genuine 04.03 acceptance;
its historical pending report is retained. No owner changes needed carrying.

`git switch main`, status, log and branch were checked, then new **`codex/04-04`**
created. Git needed command-local `safe.directory` and approved execution for lock
files; no global config/ACL change. Base and feature HEAD remain identical: no
commit, push, merge, signing, MSI, publication, deployment, browser login or real
Excel operation. VS 2026 was explicitly selected by the complete verification script.

## Input, options and operation ownership

The owner requested this amendment inside the existing uncommitted 04.04 branch;
its 26 original paths are preserved. No branch switch, reset or new batch occurred.
The pre-amendment verified baseline was **3,458 cases per configuration**.

`RangeReferencePicker` and its component-owned `RangeReferenceState` are reused
for input rectangles and existing-sheet output cells. The labelled editable text
field displays Common's canonical worksheet-qualified A1 (always quoted, embedded
apostrophes doubled; absolute/relative input normalizes to canonical coordinates).
For example `'O''Brien Δ'!$B$2:$D$25` becomes `'O''Brien Δ'!B2:D25`. Normal text
editing is allowed; only an armed owner accepts selection callbacks. `Select range`
/ `Pick output cell` arms capture; `Finish selection` explicitly recaptures current
metadata and returns focus to the textbox. Cancel restores the previous committed
reference and removes the handler. `Capture selected range` is metadata only;
`Use current selection` remains the event-independent acquire/validate fallback.
Data acquisition always requires an explicit acquire action. Typing, parser-option
changes, selection callbacks and navigation never load values/formulas.

`SpreadsheetRangeReference.FromUserInput` owns the grammar. The qualified draft
is bounded at 1,024 characters, native worksheet name at the existing 256 guard,
and only the **unqualified A1 segment** uses the existing 40-character transport
guard. No substring truncation or new JS reference grammar was introduced.
External workbooks, multiple areas, invalid/off-grid/oversized rectangles and
non-single-cell output reject before data reads or output preparation. Resolved
references bind native sheet ID/name and the actual document session object in a
bounded private handle ledger. Selection-acquired input also retains a handle,
so refresh cannot silently resolve a deleted/recreated same-name sheet. Explicit
new selection/manual edit is needed to change identity; no workbook-name fallback.
Names/addresses are intentionally visible in editable fields, never diagnostics;
raw cell values/formulas remain absent from input previews and UI logs.

`rangeCapture.mjs` loads only native selection coordinates and worksheet ID/name.
It uses the ExcelApi **1.9** worksheet-collection selection event when available;
missing event support degrades to explicit capture/manual entry. Event address/ID
arguments are hints: current `getSelectedRange()` metadata and native full sheet
name are authoritative, so queued stale event addresses are not parsed or trusted.
One serialized/coalesced metadata read owns the latest selection, with a five-second
native budget. Handlers are scoped to a capture owner and removed on finish,
cancel, manual editing, invalidation, Retry and disposal. A late arm reply after
disposal is cleaned up again; old owner callbacks cannot overwrite another field.
No DOM polling or permanent global handler is used. Native hung cleanup is best
effort, with cancelled/disposed callbacks inert. Capture is shown as armed only
after successful native subscription, not while registration is pending.

Cell-data acquisition ceilings remain **10,000 cells / 2,000 per tile / 64 MiB
estimated / 30s**. Source binding rechecks acquired cells/formulas in <=2,000-cell
tiles. Source data is bound to the resolved sheet/range independently of whichever
cells happen to be selected; a separate selection anchor detects subsequent changes.

Both analyses use the same explicit `GroupsInColumns`, `GroupsInRows`, or
`LongFormat` options and Required/None/Auto header modes. Required defaults retain
the previous column/header policy and the pilot's explicit nonblank-label check;
rows use the first column, long format the first row at selected role columns.
None includes all observations; Auto uses unchanged Common inference and rejects
ambiguous headers. True blanks remain independently skipped, and existing cell
policies reject invalid text/Boolean/error observations. No long-format inference,
local grouping algorithm or new statistical rule was added. Long category/value
selectors are 1-based, distinct and inside the captured rectangle; only the typed
parser configuration converts them to zero-based Common indices. Common reports
invalid roles using `options.long-columns`. The ID designates a treatment/group,
**not a subject or repeated-measure identifier**.

Parser/role/header changes invalidate input/results/plan/consent and require new
acquisition; same settings do not reset unnecessarily. Statistical drafts are
unaffected. Common issue codes and row/field coordinates remain redacted and bounded
(maximum 20 displayed issues). Information messages are retained as codes rather
than mistaken for errors. The preview displays the exact selected interpretation.

The acquired cells are defensively copied. Parsed legacy data and result detail
are private to this coordinator; no deep-immutability/provenance guarantee is
invented for Common's legacy arrays. Each acquisition owns method ID, draft
revision, parser policy, source token and component/lifetime generation. Run uses
an independent typed option snapshot and checks exact registered descriptor and
option/input types. Invalid alpha or incompatible schemas cannot enable Run.

- Option changes invalidate input/results/consent; **reacquire after editing**.
- Group/method changes dispose the old keyed pilot/draft; same group/method does
  not reset the active draft. Retry preserves options but invalidates acquisition.
- Destination edits retain the result/options but clear output inspection/consent.
- Pane reopening starts unselected; no storage or global current operation.
- One busy owner rejects overlapping commands. Busy controls disable navigation,
  option editing, Retry and the independent read actions; Cancel stays available.
- Generation/cancellation checks discard late completions after invalidation or
  disposal. Synchronous Core calculation is not claimed to be interruptible.
- Native submitted mutation cannot be aborted. Cancellation during writing retains
  the observed Completed/uncertain `WriteResult`, rather than replacing it with a
  misleading cancelled status. No automatic write retry exists.

## Production writer contract

`workbookAdapter.mjs:create()` **explicitly installs** `createProductionWriter`.
The guard for a missing writer remains intact; unconfigured test/read transports
still reject. The original injected inspection/commit test port remains separate;
an additional test calls the actual production factory with no injected writer.

The pilot requires ExcelApi **1.17** and usable native event APIs. The manifest
stays at **1.1** for accepted readiness/read-only behavior. A missing requirement
or failed subscription rejects the pilot while preserving safe input summaries.
General adapter capabilities remain conservatively read-only; only successful
pilot binding enables its operation-specific current/new-sheet profile, and live
facts must verify writable target state before an inspection is exposed.

Native API references reviewed: [ExcelApi 1.17 requirements](https://learn.microsoft.com/en-us/javascript/api/requirement-sets/excel/excel-api-1-17-requirement-set),
[worksheet collection events](https://learn.microsoft.com/en-us/javascript/api/excel/excel.worksheetcollection),
[Range merged/formula inspection](https://learn.microsoft.com/en-us/javascript/api/excel/excel.range),
and [Office document mode](https://learn.microsoft.com/en-us/javascript/api/office/office.document).

Identity is the **actual Office document session object**, native worksheet ID/name,
source bounds, separate selection anchor, content fingerprint and sticky event generation. A cryptographic
opaque session token binds that object and is carried in the existing intent's
adapter identity slot; it is **not a physical/durable workbook ID**. No URL/file
name is read to manufacture identity. No sheet-name fallback resolves a different
native ID. Collection events cover content, selection, activation, name, deletion,
addition, protection and formatting; workbook activation also invalidates. Events
notify the C# owner without reading cells. Event handler registrations are the
only retained Office subscription handles; removal uses their owning context.
Disposal removes handlers with bounded caller cleanup; closed/hung native contexts
can prevent immediate removal, but disposed callbacks cannot mutate UI state.

Existing-sheet output now accepts a **resolved worksheet-qualified single cell**,
including a different existing worksheet in the same bound document. Default is
still input-sheet J1. Native ID/name, source continuity and the full planned target
are checked both during inspection and before mutation. Input overlap rejects
only when source and target are the same physical worksheet; matching coordinates
on another verified sheet are allowed. Unknown/missing/replaced destination IDs
never fall back by name.

Ordinary selection/activation changes still invalidate source/results/consent.
**Explicit output picking** is a narrow exception: arming clears every prior output
inspection, verifies source content, and blocks Run/inspect/write while active.
Only selection and worksheet-activation notifications may be tolerated during that
mode; source edits, protection/name/deletion and workbook activation still
invalidate. Finish recaptures a single cell, checks the exact same selection at
lease rebind, and rechecks the original source content/identity. A selection race
at Finish rejects. Cancel restores the prior field but never restores consent.
This controlled handoff allows worksheet clicks to choose output without weakening
normal stale-selection rejection or equating the picked cell with the input.

New-sheet output exposes a custom name and an independent unqualified A1 start
cell (default A1). Inspection checks bounds, name validity/collision and workbook
protection without creating anything. Exactly one worksheet is queued only at the
permitted explicit write stage; all tables use the entered anchor. Existing-sheet
reference draft is retained when switching destination types.

### New Workbook decision and Phase 04 exit gate

**Disabled; no XLSX export workflow is offered.** Official
[workbook creation documentation](https://learn.microsoft.com/en-us/office/dev/add-ins/excel/excel-add-ins-workbooks)
states that `Excel.createWorkbook([base64])` opens another instance while the
add-in remains in the original. ExcelApi 1.8 availability alone does not supply a
new bound context or populated-output verification. No empty workbook is created,
no old-context write is attempted, and Common `Completed` is never claimed for a
cross-workbook handoff. Native creation was **not exercised**: genuine Excel/login
and handoff verification are not available to this automated session. This is an
explicit remaining acceptance investigation, not a simulated native success.

Concrete follow-up **by the Phase 04 exit review, with no new batch ID**: design a
bounded OOXML package writer from complete ordered semantic/layout tables, including
literal text, number formats and styles; validate/reopen the generated package in
automation; define an honest separate handoff outcome and cancellation/abandonment
semantics; prove populated `createWorkbook(base64)` in genuine Edge with an untouched
original/control sheet; obtain a verified context/acknowledgement from the new
workbook before any completed-write claim. If that cannot be proven, retain the
disabled option and decide explicitly at the exit review whether to authorize a
separately tested XLSX download/export workflow. Do not change E1–E7 or introduce a
new package/dependency merely to enable this control.

Before mutation, the writer verifies:

- Supported scalar/table wire v1, exact complete intent/plan JSON, unique ordered
  artifact IDs, exact rectangles, no overlaps, guards/reservations, invalid grid
  coordinates or auto-fit; output is at most **10,000 cells / 64 MiB estimate**.
- Same source session/content/selection, exact target worksheet ID/name, explicit
  `Office.DocumentMode.ReadWrite`, workbook/worksheet unprotected and native target
  observations. Unknown mode/protection/occupancy fails closed.
- No input overlap, merged/spilled cells, Excel table or pivot-table intersection.
  Unsupported New Workbook and non-cell mutations are never enabled.
- Values, formulas, value types and formats across **each exact table rectangle**.
  Gaps/envelopes are not overwrite regions. Formula detection uses documented
  `getSpecialCellsOrNullObject("Formulas")`, distinguishing literal `=` text.
- Occupied inspection → explicit Yes/No bound to that Common intent/observation.
  No returns CancelledByUser without cell changes or sheet creation. Yes checks
  the entire source, target, observations and exact plan again. Tokens are consumed
  on every write attempt; no cross-run or repeated approval exists.

Writing preserves complete tables, title/header/body/footer cells, canonical scalar
values, Common p-value number formats and significance/strong-emphasis metadata.
Text is written as escaped literal text; formulas are never generated from labels.
Supported font styling is bold headers/emphasis and significant p-values in red;
other cell text is black. Auto-fit is disabled to avoid changes outside table
rectangles. Plots/typed-detail comparison data are explicitly not written as charts.
Non-cell diagnostics remain diagnostic artifacts, rather than unintended cells.

After mutation sync, native values/formula presence/number formats and per-cell
font style are reloaded and checked, along with actual destination identity.
Only confirmed completion returns all ordered artifact IDs as Completed. A failure
after a mutation may have been queued returns **PartiallyWritten / mutation-uncertain**,
with no fabricated confirmed regions. The JS writer latches native uncertainty
until pane reopening. Observed completion wins over late cancellation. No rollback,
automatic retry, transaction or undo is claimed.

**Remaining safety limits:** Excel does not offer an atomic compare-and-write
transaction. External edits after the final safe inspection can race a submitted
batch; events/readback reduce detection gaps but cannot prove isolation. Native
requests may outlive timeout/disposal. Large bounded payloads can still fail host
limits, yielding truthful uncertainty if mutation may have started. Genuine Excel
behavior, tenant policy and API availability remain the owner's acceptance gate.
No guarantee of durable cross-workbook identity or desktop/Mac acceptance is made.

## Gate B exceptions E1–E7

E1: Windows rendering is unchanged; no broad Windows/Office visual parity claim.
E2: Common/Office table wire v1 is unchanged and remains distinct from the frozen
Common result envelope; the new pilot binding protocol is private, local and not
persisted. Strict union/plan checks and migration/regression fixtures remain.
E3: Legacy grouped/detail alias/provenance limits remain; raw snapshots are copied
and legacy objects private, with issue coordinates preserved rather than invented
per-observation provenance. E4: Common same-instance consent is combined with a
single-use JS observation/session/plan binding, with no durable token protocol.
E5: guards/reservations/non-cell mutation refuse support; gaps are not cleared.
E6: only active checkpoints/04.03–04.04 rows are reconciled; historical reports
remain unchanged. E7: general chart/detail serialization, durable workbook IDs,
settings, other analysis families and production commands remain deferred.

## Automated verification and inventory

Final inventory per configuration (repeated passes are not additional tests):

| Distinct cases | Debug | Release |
|---|---:|---:|
| Core | 1,024 | 1,024 |
| Common | 727 | 727 |
| Windows legacy | 787 | 787 |
| WasmSmoke C# / Node | 45 / 377 | 45 / 377 |
| OfficeAddin C# / Node | **232 / 217** | **232 / 217** |
| Help resolver Node | 71 | 71 |
| Python tools / Help / discovery | 27 / 16 / 5 | 27 / 16 / 5 |
| **Distinct total** | **3,528** | **3,528** |

Delta from the pre-amendment **3,458**: **42 C#** (39 range/reference/parser/state
cases, 1 additional invalid-output reference case and 2 additional real rendered
capture/interpretation/output interactions)
and **28 Node** capture/identity/writer cases, **70 added distinct cases**. The
original 190 C#/189 Node cases remain, with assertions updated for qualified
references and supported destination behavior rather than removed. Overall delta
from accepted 04.03 is 160 distinct cases. No bUnit/package was added: the existing
Razor compiler, HtmlRenderer and Blazor event renderer exercise real components.

Golden cases cover all five accepted ANOVA/KW fixtures, exact F/H/p metrics,
all optional table dimensions/order, typed option flags, p-value styles and complete
write plans. Other new cases cover malformed/missing/error/mixed/blank/ragged input,
true blank versus empty text, alpha/schema failures, repeated refresh and different
sheet IDs, destination/start-cell/grid/unknown occupancy, No/Yes, source/draft/
destination/event invalidation, busy/disposal/late completion and zero implicit reads.

Node tests exercise the **real production algorithm**, including its installed
factory, current/new sheets, protection/merged/table/pivot/spill/read-only guards,
all nine event categories, document/selection/value/formula/sheet-replacement/
rename/delete/content changes, bad/oversized/mismatched plans, collisions, missing/
stale/single-use consent, literal formula-like text, each execution sync failure,
pre-mutation cancellation, late completion precedence and disposed/busy requests.
Old reader/interop/cancellation suites and existing Common output/fault/non-cell
contracts remain green. New production-pilot tests mock only the Office API
boundary; the original injected-port transport tests remain separate.

Exact maintained commands (cached dependencies; each final gate exit **0**):

```powershell
$env:RestoreSources = 'C:/Users/peter/.nuget/packages'
$env:PythonExe = 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe'
dotnet test BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj -c Debug --logger 'trx;LogFileName=OfficeAddin-Debug.trx' --results-directory TestResults/04-04/amendment-final/Debug
dotnet test BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj -c Release --logger 'trx;LogFileName=OfficeAddin-Release.trx' --results-directory TestResults/04-04/amendment-final/Release
node --test BESHStat.OfficeAddin/tests/productionWriter.test.mjs
Push-Location BESHStat.OfficeAddin
npm.cmd test
Pop-Location
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python -PythonExe $env:PythonExe
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Release -VisualStudio 2026 -Legacy -Wasm -Js -Python -PythonExe $env:PythonExe
dotnet build BESHStatNG.sln -c Debug
dotnet build BESHStatNG.sln -c Release
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --stat
```

No network/package install, framework change or skip. Existing **UTA001** for
non-public `MatrixTestHelpers`, the Help generator's one ambiguous Form+Title Home
fallback, TRX overwrite notices and Git CRLF normalization notices remain. New
build warnings: **none**. Both solution builds and both WASM host builds succeed.
VersionAttributes.vb bytes are saved before full builds and restored in finally;
it is neither delivered nor staged. SHA-256 remains
`0F22C5A1AD446546E773E59B495E647C58F5FF08A70750AFE7372680798D2F2D`.

Amendment logs/TRX are ignored under `TestResults/04-04/amendment-final/{Debug,Release}`; pre-amendment `final` evidence is preserved. The full
script overwrites unsuffixed `TestResults/Codex` names; `Core.trx`, `Common.trx`,
`Legacy.trx`, `OfficeAddin.trx`, `OfficeExecution.trx` were copied after each
configuration, never aggregated with historical focused TRX. Direct configuration-
named TRX and foundation/solution/regression logs distinguish both builds.
`amendment-final/outcomes.txt` and per-configuration logs/TRX record final amended-source runs; earlier exploratory logs are not substituted for them.

Development issues corrected before handoff: Razor compiler escaping/directive
errors; test-renderer static-markup handling; required-header admission versus
legacy auto-labels; literal `=` text versus actual formulas; documented API review
of OrNullObject/formula detection; string parameter literals in OutputPicker,
caught by the actual browser new-sheet path and guarded by rendered-value tests.
The pre-amendment strengthened Run guard also needed C# `ref` for the existing VB ByRef
catalog API; both final configurations were rerun after correcting that compile
error. Failed development logs are retained with `compile-failure` filenames.
No numerical/reference tests were weakened to fix these issues.

Normal builds regenerate ignored packed XLLs. Presence/PE checks cover both x86
and x64 for Debug/Release; no XLL is installed, manually edited or signed. The
read-only amendment inventory is saved in `TestResults/04-04/amendment-packed-xlls.json`.

| Packed path | Bytes | Verified PE architecture |
|---|---:|---|
| `BESHstatNG/bin/Debug/BESHStatNG-AddIn-packed.xll` | 1,948,672 | x86 / 0x014C |
| `BESHstatNG/bin/Debug/BESHStatNG-AddIn64-packed.xll` | 1,874,432 | x64 / 0x8664 |
| `BESHstatNG/bin/Release/BESHStatNG-AddIn-packed.xll` | 1,879,552 | x86 / 0x014C |
| `BESHstatNG/bin/Release/BESHStatNG-AddIn64-packed.xll` | 1,805,312 | x64 / 0x8664 |

## Actual browser runtime versus genuine Office

`pilot-browser.mjs` uses actual Blazor WASM, Common/Core execution, ES modules,
typed interop and the installed production writer against an **async simulated
Excel.run/context.sync boundary** in isolated headless Edge. It verifies ANOVA
F=13 / p=0.006591796875, occupied No/Yes, KW/Dunn, new-sheet creation, event
invalidation, Retry, narrow-pane overflow and independent 04.02 reading.
This is **not genuine Excel Online acceptance** or proof of native sync health.

The amendment has **12 browser scenarios**: one new range-capture scenario plus the original eleven (pilot, two options modes, six readiness
fixtures and two read diagnostics). These are additional scenarios, excluded from
the distinct unit-case totals. Screenshot review found no visible clipping at the
tested narrow width; the full form requires vertical scrolling. No visible UI
defect was found in the three saved pilot screens. Native Excel rendering/events and keyboard behavior still require the owner checks below. The new automated scenario checks real browser Tab/Enter and focus return, not real Excel event delivery.

```powershell
dotnet run --project BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj --no-build --launch-profile https
Start-Process -FilePath 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' -WindowStyle Hidden -ArgumentList @('--headless=new','--disable-gpu','--no-first-run','--remote-debugging-port=9227','--user-data-dir=C:/Users/peter/source/repos/BESHstatNG/TestResults/04-04/edge-amendment','about:blank') -PassThru
node BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs
node BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs
node BESHStat.OfficeAddin/tests/browser/options-browser.mjs
node BESHStat.OfficeAddin/tests/browser/options-browser.mjs --standalone
foreach ($fixtureName in @('excel','pending','unsupported','missing','dependency','bridge-regression')) {
    node BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs "--fixture=$fixtureName"
}
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs --pending-sync
```

The local server used the owner's existing certificate with approved execution;
the browser used a fresh isolated profile, no authentication or browser install.
No live Excel, desktop/Mac runtime, real Office CDN startup or live workbook write
was run. Owner-reported accepted 04.01–04.03 evidence remains distinct.
Screenshots/sanitized JSON are under `TestResults/04-04`; input/table cell values are hidden in the UI; synthetic worksheet names and full qualified references are visible in the new picker screens.

![ANOVA preview, simulated Office](../../TestResults/04-04/screenshots/anova-preview.png)
![Overwrite consent, simulated Office](../../TestResults/04-04/screenshots/overwrite-consent.png)
![KW preview, simulated Office](../../TestResults/04-04/screenshots/kw-preview.png)
![Qualified input, simulated Office](../../TestResults/04-04/screenshots-refedit/qualified-anova.png)
![Different-sheet output cell, simulated Office](../../TestResults/04-04/screenshots-refedit/qualified-output.png)
![Long-format KW, simulated Office](../../TestResults/04-04/screenshots-refedit/long-kw.png)

## Exact delivered paths

| Path | Purpose |
|---|---|
| `BESHStat.OfficeAddin/App.razor` | Readiness generation, busy ownership, pilot integration/footer |
| `BESHStat.OfficeAddin/Components/AnalysisShell.razor` | Keyed pilot, busy navigation/specification gating |
| `BESHStat.OfficeAddin/Components/AnalysisPilot.razor` | Explicit run/result/inspection/consent/outcome UI |
| `BESHStat.OfficeAddin/Components/RangePicker.razor` | Shared reference, orientation/header/role and explicit acquisition controls |
| `BESHStat.OfficeAddin/Components/ValidationPreview.razor` | Bounded redacted summaries and structured issue coordinates |
| `BESHStat.OfficeAddin/Components/OutputPicker.razor` | Qualified existing-sheet capture, new name/anchor, disabled New Workbook explanation |
| `BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs` | Borrow shared transport as typed pilot host |
| `BESHStat.OfficeAddin/Services/AnalysisOptionDraft.cs` | Revision/change notifications; disposed draft events cleared |
| `BESHStat.OfficeAddin/Services/AnalysisPilotState.cs` | Private snapshot/parse/run/plan/consent owner |
| `BESHStat.OfficeAddin/Services/IPilotHost.cs` | Private typed host replies/facts and invalidation callback |
| `BESHStat.OfficeAddin/Services/WorkbookAdapter.cs` | Preserve safe specific pre-mutation writer result codes |
| `BESHStat.OfficeAddin/Services/WorkbookTransport.cs` | Typed binding/inspection calls and owned DotNet callback cleanup |
| `BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs` | Explicit production factory installation and shared operation guard |
| `BESHStat.OfficeAddin/wwwroot/js/productionWriter.mjs` | Native source/lifecycle/target/consent/write/readback algorithm |
| `BESHStat.OfficeAddin/wwwroot/css/app.css` | Narrow picker/result/busy layout |
| `BESHStat.OfficeAddin/tests/AnalysisPilotTests.cs` | 36 coordinator/golden/validation/lifecycle cases (35 initial plus 1 amendment) |
| `BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs` | Preserve navigation tests; 4 added rendered run/consent/capture/interpretation cases |
| `BESHStat.OfficeAddin/tests/productionWriter.test.mjs` | 53 real writer algorithm/factory/failure cases |
| `BESHStat.OfficeAddin/tests/helpers/pilotWorkbook.mjs` | Async Office boundary model, no production mock port |
| `BESHStat.OfficeAddin/tests/workbookWriter.test.mjs` | Clarify the retained unconfigured-writer rejection test name |
| `BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs` | Actual browser production-pipeline simulation/screenshots |
| `BESHStat.OfficeAddin/tests/browser/options-browser.mjs` | Preserve old browser coverage; separate current screenshots |
| `BESHStat.OfficeAddin/README.md` | Current feature/limits, launch and reproducible owner instructions |
| `scripts/verify-office-foundation.ps1` | Require the new production writer static asset |
| `planning/BESHStatNG_CrossPlatform_MasterPlan.md` | Active accepted 04.03 / pending 04.04 and 36/187 checkpoint |
| `BESHStat.OfficeAddin/Components/RangeReferencePicker.razor` | Reused editable qualified range/cell textbox, capture controls and focus |
| `BESHStat.OfficeAddin/Services/IRangeCaptureHost.cs` | Private typed metadata/capture protocol and callbacks |
| `BESHStat.OfficeAddin/Services/RangeReferenceState.cs` | Scoped committed/manual/armed drafts, cancel/reset and owner filtering |
| `BESHStat.OfficeAddin/wwwroot/js/rangeCapture.mjs` | Serialized bounded native metadata capture/resolution and handle identity |
| `BESHStat.OfficeAddin/tests/RangeReferenceTests.cs` | 39 Common-grammar/state/arrangement/header/parity/role cases |
| `BESHStat.OfficeAddin/tests/rangeCapture.test.mjs` | 28 native-boundary capture/identity/output cases |
| `BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs` | Real browser RefEdit-like interactions, keyboard/focus, equivalent arrangements, custom output |
| `docs/architecture/batch-04-04-range-preview-destination-ui.md` | This audit/evidence/review gate |

Only these **33 paths** are delivered (26 pre-existing batch paths plus 7 amendment additions). Common/Core, Windows/Excel-DNA, framework
targets, WasmSmoke, generated metadata, manifest, Help resolver/MIME hook and
installer/signing source are unchanged. Initial worktree was clean; final status
is the intentional uncommitted set above. Full status is retained in
`TestResults/04-04/amendment-final-status.txt`; `git diff --check` exits 0.

## Owner-only genuine Edge acceptance — not performed by Codex

- [ ] Disposable workbook, untouched control sheet and occupied destination
  fixtures; existing 7141 dev manifest/identity/permissions. Re-sideload only if
  stale assets require it. Verify `office.excel-ready` / OfficeOnline, Core 0.5,
  Common 2/4 and 12 groups/76 methods with only ANOVA/KW enabled.
- [ ] On a worksheet with spaces/apostrophes/unicode in its name, arm Select range,
  drag, verify full qualified textbox, Finish/focus, cancel and explicit-capture
  fallback. Manually type a different absolute/relative reference and acquire.
  Invalid multi-area/external references reject without reads/writes.
- [ ] Compare equivalent columns/rows/long datasets in both analyses, using
  Required/None/Auto where applicable; verify exact F/H/p values, Dunn/post-hoc
  tables, ambiguous-header and invalid 1-based role rejection. Changes invalidate
  preview/consent and retain statistical options.
- [ ] Set options, choose valid wide nonblank headers/numeric columns, acquire,
  preview and Run ANOVA. Compare F/p against the accepted fixture; edit options
  then reacquire, verify Welch/post-hoc table completeness. No worksheet changes
  until explicit output action. Repeat KW/H/p with Dunn on/off and separate input.
- [ ] Existing sheet: type a qualified start cell or arm Pick output cell, select a single cell (also on another sheet), Finish then inspect/Write. Cancel restores text and requires fresh inspection.
  Confirm table order/title/header/body/p-value formats, placement and untouched
  gaps/control sheet/outside cells. Ensure no auto-fit or plot mutations.
- [ ] Occupied output: inspect then No, with absolutely no changes or new sheet.
  Reacquire/Run/inspect fresh and choose Yes; only authorized rectangles change.
- [ ] New worksheet: inspect without creation, then explicit Write; check valid
  custom name/start cell and complete output. Collision/protection/merged/spilled/table targets and
  New Workbook reject with an accurate disabled explanation. No XLSX export is offered. Read-only/unknown document mode cannot claim writability.
- [ ] Pending consent: edit target/source, change selection, rename/delete/recreate
  sheet or switch workbook context. Verify disabled/stale state, re-acquisition and
  fresh inspection/consent, with no same-name fallback or unintended mutation.
- [ ] Cancel before writing; test Retry/reopen and a second independent operation.
  Check truthful Completed/Cancelled/Rejected/PartiallyWritten codes and workbook
  integrity. Never assume rollback or retry an uncertain write automatically.
- [ ] Keyboard Tab/arrows/Enter/Space, visible focus, narrow scrolling, invalid
  alpha and safe issue/status announcements. Retry retains drafts but requires new
  acquisition; group/method changes and reopening discard old snapshots/consent.
- [ ] Explicit 04.02 diagnostic and independent WasmSmoke 7131 still work. Inspect
  console in the **localhost:7141 frame**, not Excel's parent shell. Capture safe
  ANOVA/KW/consent screenshots and fixed diagnostic codes if any boundary fails.

If a genuine write fails validation or changes unintended cells, **04.04 stays
unaccepted**. Record fixed code, reproduction and host boundary without workbook
data. Only owner acceptance/merge can change counts to 37/186 and authorize 04.05.
No later-batch work, commitment, merge, deployment or installer action was done.

Amendment development findings: capture was initially shown armed before native
registration completed (fixed; now Busy until acknowledged); a synthetic browser
fixture passed through an ASCII shell pipe and changed Unicode to `?`, correctly
rejected as an invalid new-sheet name (fixture fixed to Unicode escapes). An
inherited exact exception assertion was corrected to admit Common's derived
ArgumentOutOfRangeException. Existing tests now expect qualified canonical text,
use explicit selection reacquisition for a deliberately different sheet ID, and
model the new metadata host seam; no numerical or safety expectation was weakened.

Final stale-draft review also closed a UI rejection gap: invalid/empty/unqualified
existing-sheet output or a failed capture cannot silently inspect the previously
committed target. A fresh valid manual edit/capture is required. Cancel waits for
its native cleanup before releasing ownership. Selection-acquired references keep
a document/worksheet-bound handle across refresh; a same-name replacement rejects.
The full gates were repeated after these final guards; prior exploratory evidence
is kept separately and does not establish final-source verification.

Browser keyboard evidence uses the existing harness's rawKeyDown/char/keyUp
sequence; an exploratory Enter fixture omitted the char event and timed out on
button activation. It was corrected without replacing native keyboard activation
with a click. Finish/Cancel focus is scheduled after the textbox has rendered
enabled. Four new picker/long-format screenshots were visually reviewed: no
horizontal clipping at 360px, with normal vertical scrolling for the complete form.
Exact native worksheet-name spelling is required by the current manual resolver;
case-only aliases are conservatively rejected rather than guessed.

Final amended-source gates all exited **0**; all 12 browser scenario exits are 0
in `TestResults/04-04/amendment-browser/outcomes.txt`. The final whitespace audit
also checked all 19 untracked delivered files against an empty file; one extra
EOF blank line was removed, then no whitespace diagnostics remained. Node's 217
cases were rerun after that whitespace-only correction and passed. The isolated
headless Edge profile and localhost dev server were shut down after verification.
No genuine Excel event/capture/write or new-workbook creation was performed.
