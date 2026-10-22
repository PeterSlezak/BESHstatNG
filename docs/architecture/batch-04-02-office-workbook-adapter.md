# Batch 04.02 — typed Office workbook adapter

**Owner-smoke amendment below supersedes the implementation-only test inventory
and records a genuine Excel read timeout. Acceptance remains blocked.**

**IMPLEMENTED / PENDING OWNER REVIEW AND REAL SELECTED-RANGE SMOKE.**
Only 04.02 is implemented. No 04.03, 04.04, Phase 04A, installer or publication work.
Checkpoint: **223 total / 34 accepted / 189 outstanding**; 04.02 is outstanding.

## Start gate and focused source audit

Initial checkout was clean on `main`, HEAD **`d7888cd`**, “New production
BESHStat.OfficeAddin foundation.” This commit contains the accepted single-wrap
fix: `officeHost.mjs:create()` returns the adapter directly; Blazor performs the
only IJSObjectReference conversion. The owner explicitly confirmed real Excel
Online `office.excel-ready` / `OfficeOnline`, Core **0.5**, Common **2 / 4**,
standalone fallback, Retry and independent WasmSmoke. That acceptance supersedes
historical pending wording in the 04.01 report; it is not a new Codex live test.

Ran status/branch/log, required `switch main`, status/log again, then created
**`codex/04-02`**. Git needed a command-local safe-directory exception because the
sandbox account differs from the repository owner. Branch switching/creation
required approved execution for `.git/index.lock`; no global Git setting or ACL
changed. No dirty changes were switched, discarded, transplanted or committed.
An owner/local edit marking the 04.01 plan row COMPLETE appeared during the run;
that label was retained when reconciling active acceptance checkpoints.

Read AGENTS.md; current master plan 04.01/04.02 and Gate B E1–E7; 01.09, 01.10,
03.06 and 04.01 reports; production Program/App/DI/readiness/status/module,
manifest, tests and README; WasmSmoke selected/reference/range-set reads, typed
conversion, budgets/tiles, document lifecycle, inspection/consent and writer;
Common spreadsheet/range contracts, budgets, host capabilities/lifecycle and
output plans/intent/consent/outcomes. The source audit and concrete path inventory
were announced before implementation. No Common or Core change was necessary.

| Existing authority | Reuse in this batch |
|---|---|
| `SpreadsheetRangeData`, `SpreadsheetCellValue`, `SpreadsheetRangeReference`, A1 parser | Final typed snapshots, strict primitive-cell projection, explicit reference validation |
| `InputSizeLimits`, `InputAcquisitionBudget` | Same default ceilings, preflight and cooperative elapsed/cancellation semantics; host may lower budgets |
| `DocumentLifecycleSnapshot`, `DocumentLifecycleLease`, `OutputRectangle` | Operation-owned document/sheet/bounds checks, sticky invalidation, mark-copied only after final verification |
| `HostCapabilitySnapshot`, `HostCapabilityIds.ReadSelectedRange` | Only implemented read capability advertised; no write/new-sheet/new-workbook capability |
| `OfficeOutputPlan`, `WriteIntent`, `WriteConsent`, `WriteResult`, existing state/code enums | Typed output seam, target/region checks and terminal outcome projection; production fails closed |
| Canonical `canonical-two-ranges.json` | Both ranges tested independently, preserving every cell kind/value/format and coordinates |
| Existing `mixed-zero.json` Office output fixture | Mock writer uses the actual established output-plan shape, not a new writer envelope |

WasmSmoke remains independent, unchanged, and is not imported at runtime. This
module reuses its reviewed semantics and fixtures rather than bulk-copying its
large prototype bridge. One important API distinction: the prototype uses
`getRangeByIndexes`, which requires **ExcelApi 1.7**. The production adapter uses
generated, unqualified A1 with worksheet `getRange` (**1.1**), so the conservative
manifest requirement stays truthful. `getSelectedRange` (**1.1**) throws for
multiple selected ranges; no bounding rectangle or optional multi-area assumption
is introduced. See Microsoft's [Workbook reference](https://learn.microsoft.com/en-us/javascript/api/excel/excel.workbook)
and [Worksheet reference](https://learn.microsoft.com/en-us/javascript/api/excel/excel.worksheet).

## Public service and transport ownership

`AddOfficeFoundation()` registers scoped, idempotent `IWorkbookAdapter` and
`IWorkbookTransport`. Program still calls the same registration extension. The
workbook service borrows the **same accepted `IOfficeHostReadiness`**; disposing a
read operation or workbook adapter never disposes readiness. Workbook access
requires both `HostState.Excel` and exact `office.excel-ready`. JS independently
awaits real onReady and exact ExcelApi 1.1 support before reading context/document.

`IWorkbookAdapter` exposes:

- `GetCapabilitiesAsync`: typed host snapshot containing only selected-range read.
- `InspectSelectionAsync`: bounded dimensions plus typed Common reference for
  later selectors; no value loads. Names/addresses stay out of the UI/diagnostics.
- `ReadSelectionAsync`: current single rectangle, optional lower `InputSizeLimits`.
- `ReadRangeAsync`: Common reference with actual worksheet ID/name and matching
  canonical coordinates; unverifiable workbook/selection identities reject.
  This addresses the current hosting workbook, not a persisted physical workbook.
- `WriteAsync(OfficeOutputPlan, WriteIntent, WriteConsent, CancellationToken)`:
  existing Common `WriteResult`; production rejects before mutation.

All operations accept cancellation. Public read results contain a fixed safe code
and a strongly typed value only on success. Transport DTOs require essential JSON
fields, validate enum/shape/string/finite-number bounds, and project into existing
Common DTOs. No arbitrary object, native exception, COM or Excel proxy crosses
the public service boundary. `ReadReply` is a host transport envelope, not a new
Common serialization schema. No 03.04 fault mapper or frozen output schema changed.

The service rejects overlapping operations with `host.busy`; it does not queue a
read that might later act on another selection. Separate panes own independent
instances. Cancellation/timeout discards the accumulated rows and requests JS
operation cleanup. Dispose cancels the owner, drains the operation and releases
JS adapter/module references with bounded cleanup. Import/create failures can be
retried. Late import/factory completion releases locally acquired references
instead of retaining them after disposal. There are no workbook event listeners,
background reads, global pane state or retained Excel RequestContexts.

## Acquisition protocol and mapping

`create()` returns a **plain** transport adapter. C# imports it lazily only after
readiness succeeds, wrapping once through `InvokeAsync<IJSObjectReference>`.

1. `beginRead(operationId, limits, reference, metadataOnly)` captures the actual
   Office document session object, gets metadata inside a fresh Excel.run/sync,
   validates actual worksheet identity/address/geometry, and enforces quotas.
2. `readNext(operationId)` plans one bounded tile internally. In a fresh run it
   reobserves the selection or pinned reference, reads only that tile's values,
   valueTypes and numberFormat, checks tile geometry and observes the source again.
   Only a primitive tile crosses interop; JS does not retain its cell matrix.
3. C# validates expected offsets, dimensions, completion flag, cell discriminants
   and finite primitives, copies row-major cells and checks the Common lease.
4. `finishRead(operationId)` revalidates document/sheet/selection after all tiles.
   C# checks its budget/lease, marks copied and returns the complete Common range.
   Cancellation or any failure returns **no partial snapshot**.

Metadata-only inspection uses begin/finish without any readNext/value request.
The opaque GUID is only an operation handle. The Common lease's ContextIdentity
represents that handle's verified capture of the real Office document object;
it is never placed in WorkbookId or SelectionId, never persisted, and cannot
authorize a write to an unidentified workbook.

| Host observation | Common projection |
|---|---|
| Actual `rowIndex` / `columnIndex` | One-based StartRow / StartColumn |
| Actual address plus geometry and worksheet qualifier | Uppercase unqualified A1, no `$`, explicit endpoints even for one cell |
| `Empty` vs `String` with `""` | Blank vs explicit empty Text |
| Number, including zero | Finite Number; formatted date serial remains numeric |
| Boolean | Bool without coercion |
| String `#N/A` vs actual `Error` value | Text vs CellError; preserve error text only when reported as Error |
| Actual numberFormat | Retained hint, never a date inference |
| Worksheet ID/name | Actual native identity and spelling, checked at boundaries |
| Unverified book ID/name, selection ID/revision | Empty; no invented or URL-derived identities |
| Unverified date system/origin, separate native error code | Unknown / null / empty |

Canonical fixture parity covers both rectangles separately; multi-range acquisition
is deferred. The fixture's workbook name/request selection IDs are not claimed
as verified host identities; simulated native sheet IDs are supplied by the fake
host. Its blanks, empty strings, zero, Boolean, error/text distinction, numeric
serial, whitespace text, formats and original coordinates match exactly.

Default limits: **10,000 individual/total cells**, **2,000 cells per tile**,
**64 MiB estimated bytes**, **1,024 estimated bytes per cell**, **30 seconds**.
Limits can be lowered, not raised in 04.02; one rectangle uses the existing range
count policy. Dimension/cell/byte preflight precedes cell loads and row allocation.
Wide single rows tile across columns. Both sides check budgets between tiles and
charge actual transported string lengths in addition to the base estimate.
This is not a hard bound on Office/browser heap or transient native buffers.
Snapshots retain the entire accepted input; chunking bounds individual responses.

Selection semantics are current-at-checkpoints, with stable document/sheet/name/
bounds required throughout an operation. Pinned references ignore unrelated active
selection but require their own bound sheet/geometry. Renamed/deleted/replaced
sheets, observed selection or document switches reject safely. There is no
fabricated selection revision, transactional value snapshot or guarantee against
transient ABA changes between checkpoints. Concurrent cell edits may produce a
current live view. A submitted native sync cannot be forcibly aborted; cancellation
prevents further payload scheduling and suppresses the incomplete snapshot.

## Output seam and mutation limits

There is **no production writer implementation or write button**. `create()`
installs no writer port. Production `write` returns RejectedBeforeWrite, including
unsupported NewWorkbook, without entering Excel.run. Higher manifest permission
does not turn this on. No cells, styles, sheets, charts or selections are mutated.

The typed C# seam checks plan target/ordered regions against Common WriteIntent
and projects the existing outcome states. Explicit injected test writers use
existing inspection/consent vocabulary: rejected/unknown inspection, missing or
declined overwrite consent, stale observation/identity/selection and target
mismatch cannot call commit. The test port supplies mock sheet lookup/inspection
and mutation; it is not production occupancy or rendering logic. Real destination
collision/protection/merge inspection, complete payload-bound consent and UI remain
04.04 responsibilities before any production write can be enabled.

Mock successful sync reports Completed and ordered confirmed regions, even when
cancellation arrives after observed completion. A mutation followed by an exception,
failed sync, missing/malformed reply or uncertain completion is PartiallyWritten /
`output.mutation-uncertain`, never CancelledByUser or a rollback claim. Import/create
failure before invocation is FailedBeforeWrite; explicit refusal is
CancelledByUser only with the required consent metadata. Contradictory wire
states/codes cannot claim success. Neither Undo nor rollback is advertised.

## Manifest and minimal read-only UI

Development permission changes **Restricted → ReadWriteDocument**, retaining GUID,
HTTPS **7141**, Workbook host, ExcelApi **1.1**, manifest version and packaging.
Microsoft explicitly [requires read/write permission for application-specific APIs,
even read-only calls](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/requesting-permissions-for-api-use-in-content-and-task-pane-add-ins).
The description and manifest regression test reflect the change. The owner must
remove/re-sideload this dev manifest; an old sideload can retain the old permission.
WasmSmoke's manifest/7131 identity is unchanged.

App injects only typed services. Once genuinely Excel-ready, it shows **Inspect
selected range (read-only)**. This explicit action reads through IWorkbookAdapter
and displays only status and row/column/cell counts. No values, worksheet/workbook
names, addresses or arbitrary errors enter markup/diagnostics. No new route or
navigation was added. Startup and Retry do not read cells; Retry clears the old
summary and is disabled while a read is running. Reopen creates fresh scoped state.

## Exact path inventory

| Path | Change |
|---|---|
| `BESHStat.OfficeAddin/Services/IWorkbookAdapter.cs` (new) | Typed workbook/transport interfaces, result and strict wire DTOs |
| `BESHStat.OfficeAddin/Services/WorkbookAdapter.cs` (new) | Readiness, budgets/lease, tiled projection, safe results and write-state seam |
| `BESHStat.OfficeAddin/Services/WorkbookTransport.cs` (new) | Lazy/coalesced JS import/create, typed calls, safe stage failures and cleanup |
| `BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs` (new) | Bounded, mockable 1.1 reader and fail-closed production write port |
| `BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs` | Scoped idempotent registrations |
| `BESHStat.OfficeAddin/Infrastructure/HostStatus.cs` | Ready message reflects explicit inspection |
| `BESHStat.OfficeAddin/App.razor` | Explicit typed read-only diagnostic and safe summary |
| `BESHStat.OfficeAddin/manifest-dev.xml` | Required permission and read-only purpose description |
| `BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs` (new) | Fake readiness/transport, JSON, canonical mapping, cancellation/races, outcomes |
| `BESHStat.OfficeAddin/tests/helpers/workbook.mjs` (new) | Shared fixture-backed fake Excel proxies; detects reuse outside run |
| `BESHStat.OfficeAddin/tests/workbookAdapter.test.mjs` (new) | Actual JS reader, metadata, quota, identity, lifecycle and Razor boundary cases |
| `BESHStat.OfficeAddin/tests/workbookWriter.test.mjs` (new) | Fail-closed production and explicitly simulated guarded writer cases |
| `BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj` | Link production service sources into existing host test assembly |
| `BESHStat.OfficeAddin/tests/FoundationTests.cs` | Manifest expectation updates to required permission; all cases retained |
| `scripts/verify-beshstatng.ps1` | Include OfficeAddin C#/build with -Wasm and Node with -Js |
| `scripts/verify-office-foundation.ps1` | Include new JS module in built static-asset checks |
| `BESHStat.OfficeAddin/README.md` | Permission/re-sideload, service limits, owner smoke and counts |
| `planning/BESHStatNG_CrossPlatform_MasterPlan.md` | Accepted 04.01, 34/189 checkpoint; 04.02 pending; retain owner COMPLETE label |
| `docs/architecture/batch-04-02-office-workbook-adapter.md` (new) | This audit, evidence and owner gate |

Core/Common, WasmSmoke, Windows, solution, package/framework versions, Help and
03.04 fault contracts were not edited. No installs, signing, system/browser setting
changes, sideload, live Excel, deployment, stage, commit, push or merge performed.

## Verification and evidence

Focused checks execute the actual production JS module against mocked Office/
Excel.run/sync/range/sheet boundaries and exact production C# sources against
fake readiness/transport/IJSRuntime. No test opens a workbook or authenticates.
New reader tests cover both canonical ranges, exact types, single/wide/tiled
rectangles, preflight-before-values, malformed/oversized inputs, unsupported host/
API, pending readiness, document/sheet/selection changes, time budget, hung sync,
between-tile cancellation, overlap/retry/dispose and per-instance ownership.
Writer tests cover the default rejection and simulated inspection/consent/outcomes.
C# adds JSON-required-field checks, Common projection, DI, late import/create
cleanup, interop failures, readiness-timeout distinction and completion precedence.
All pre-existing tests remain, including 04.01's 29 C# and 56 Node cases.

Final distinct inventory (Debug and Release are reruns, not additional cases):

| Suite | Debug | Release |
|---|---:|---:|
| Core | 1,024 | 1,024 |
| Common | 727 | 727 |
| Windows legacy | 787 | 787 |
| WasmSmoke C# / JS | 45 / 377 | 45 / 377 |
| Help resolver JS | 71 | 71 |
| Python tools / help / discovery | 27 / 16 / 5 | 27 / 16 / 5 |
| Production OfficeAddin C# / Node | **70 / 122** | **70 / 122** |
| **Total distinct cases** | **3,271** | **3,271** |

This is **+41 C# +66 JS =107** over the accepted **3,164** floor. The new C# cases
are in the existing OfficeAddin assembly; no extra project or package is needed.
Intermediate development counts are superseded by final checks, not counted again.

Exact commands, repository root, using only already cached packages:

```powershell
$env:RestoreSources = 'C:/Users/peter/.nuget/packages'
$env:PythonExe = 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe'

# Focused development runs:
dotnet test BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj -c Debug --no-restore
node --test BESHStat.OfficeAddin/tests/workbookAdapter.test.mjs BESHStat.OfficeAddin/tests/workbookWriter.test.mjs
# npm.cmd test also runs all 04.01 and 04.02 Node cases in BESHStat.OfficeAddin.

# Final host gate and complete repository gate, each in Debug and Release:
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-office-foundation.ps1 -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python -PythonExe $env:PythonExe
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Configuration Release -VisualStudio 2026 -Legacy -Wasm -Js -Python -PythonExe $env:PythonExe
dotnet build BESHStatNG.sln -c Debug
dotnet build BESHStatNG.sln -c Release

Get-ChildItem BESHstatNG/bin/Debug,BESHstatNG/bin/Release -Filter '*packed.xll' | Select-Object DirectoryName,Name,Length
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --stat
```

Logs remain ignored under `TestResults/04-02`: `foundation-{Debug,Release}.log`,
`regression-{Debug,Release}.log`, `solution-{Debug,Release}.log`. Script TRX files
remain in their existing `TestResults/04-01` and `TestResults/Codex` directories.
The full gates run catalog inventory/metadata parity, version-first Help links,
resolver generation, retired-helper audit, actual MkDocs MIME/build tests and
Windows legacy MSBuild/vstest, as well as both WASM host builds. Full solution
builds cover both configurations and Excel-DNA packaging. TRX overwrite notices
and the existing non-failing `UTA001 MatrixTestHelpers` discovery advisory are
recorded separately from build warnings. Help generation also reports the existing
single ambiguous Form+Title key skipped with Home fallback. No tests are weakened
or skipped. Git's CRLF-normalization notice for the preserved generated version
file is informational; its actual bytes remain unchanged.

The generated version file was clean initially, and its exact bytes were saved
before each full build/gate and restored in finally if generation changed them.
Initial/final SHA-256:
`0F22C5A1AD446546E773E59B495E647C58F5FF08A70750AFE7372680798D2F2D`.
No generated source is a delivered change. Packed XLLs are inspected read-only
after normal build packaging; none is manually edited, signed, installed or loaded.

Read-only PE checks confirmed all four packaged outputs (normal rebuilds regenerate
these ignored artifacts; no claim that their binary hashes equal an earlier build):

| Configuration | Packed XLL | Bytes | PE machine |
|---|---|---:|---|
| Debug | `BESHStatNG-AddIn-packed.xll` | 1,948,160 | 0x014C / x86 |
| Debug | `BESHStatNG-AddIn64-packed.xll` | 1,873,920 | 0x8664 / x64 |
| Release | `BESHStatNG-AddIn-packed.xll` | 1,880,064 | 0x014C / x86 |
| Release | `BESHStatNG-AddIn64-packed.xll` | 1,805,824 | 0x8664 / x64 |

Sizes, signatures and SHA-256 are retained in ignored
`TestResults/04-02/packed-xlls.json`; signing and loading were not performed.

**Live evidence boundary:** no browser/Excel session was opened by Codex for
04.02. Existing 04.01 browser fixtures do not prove the new read transport.
All new Excel interactions in automated tests are explicitly simulated. Owner
04.01 acceptance is retained; selected-range acceptance, re-sideloaded permission,
actual host timing, desktop Excel and Mac remain owner checks. No screenshots or
workbook-data logs are offered as live proof.

## Owner acceptance and stop gate

Use the [host README](../../BESHStat.OfficeAddin/README.md#batch-0402-selected-range-acceptance)
and a disposable workbook. Rebuild/run **7141**; remove/re-sideload the distinct
updated dev manifest to apply ReadWriteDocument. Keep WasmSmoke on **7131** with
its original manifest. If diagnosing, select **https://localhost:7141/** in the
DevTools frame dropdown, not the Excel shell/baseFrame/about:blank context.
Share only fixed codes and dimensions, never workbook data, names, addresses,
headers, credentials, request URLs or full HAR/exception dumps.

- [ ] Standalone: `office.unavailable`, Core **0.5**, Common **2 / 4**; no read
  affordance, no workbook access, Retry still works.
- [ ] Genuine Excel Online: `office.excel-ready`, **OfficeOnline**, probes pass.
  Select a small mixed rectangle (headers, number/zero, blank, Boolean, text,
  formula-returned empty string, error), click the explicit read-only action once;
  verify correct dimensions, `workbook.ok`, no data echoed or worksheet changes.
- [ ] Repeat alternate range and single cell; oversized selection rejects before
  large transfer; disjoint selection safely rejects. Retry clears the summary;
  close/reopen has no stale snapshot or background reads.
- [ ] No changed values/formulas/formats/selection, no output writes or new sheet;
  independent WasmSmoke **7131** and accepted Windows smoke remain working.
- [ ] Optional desktop Excel/Mac tests, if executed, are recorded as actual owner
  evidence; simulated readiness/read tests are not substituted for them.

Stop at **04.02**, uncommitted on **codex/04-02**, pending owner acceptance. Full
input/output selectors, consent UI, production writing, analyses/navigation,
multi-area/range-set acquisition, events/revision tracking and later host UI remain
deferred to their approved batches. Gate B and all framework targets stay intact.

Final outcomes: both complete regression commands, both foundation commands and
both solution builds exited **0**. Final foundation runs after metadata-query
review passed **70 C# / 122 Node** in each configuration; solution/host builds
reported zero warnings/errors. Final `git diff --check` exited **0**; additional
whitespace checks covered all nine added files. Final branch is `codex/04-02`;
the version-file hash above remains unchanged. All nineteen changed paths are
listed in the inventory; no unrelated generated/Windows/Core/Common/WasmSmoke
source change is present. Final `git status --short`:

```text
 M BESHStat.OfficeAddin/App.razor
 M BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs
 M BESHStat.OfficeAddin/Infrastructure/HostStatus.cs
 M BESHStat.OfficeAddin/README.md
 M BESHStat.OfficeAddin/manifest-dev.xml
 M BESHStat.OfficeAddin/tests/FoundationTests.cs
 M BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj
 M planning/BESHStatNG_CrossPlatform_MasterPlan.md
 M scripts/verify-beshstatng.ps1
 M scripts/verify-office-foundation.ps1
?? BESHStat.OfficeAddin/Services/
?? BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs
?? BESHStat.OfficeAddin/tests/helpers/
?? BESHStat.OfficeAddin/tests/workbookAdapter.test.mjs
?? BESHStat.OfficeAddin/tests/workbookWriter.test.mjs
?? BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs
?? docs/architecture/batch-04-02-office-workbook-adapter.md
```

## Owner-smoke timeout investigation amendment — 22 October 2026

**Acceptance blocked; actual Excel read root cause remains unproved.** Owner
reported genuine Excel Online `office.excel-ready` / `OfficeOnline`, Core **0.5**,
Common **2 / 4**, then `input.time-budget` on an apparently small 4 × 3 rectangle.
No successful genuine full selected-range read has been supplied. The changes
below provide stage attribution and a metadata comparison; they do **not** claim
to cure native Office transport or establish acceptance through mocks.

### Entry checkpoint and source evidence

Continued existing **codex/04-02**, HEAD
`d7888cd272e5bb304139e2fa3b4632c561f20725`, with all nineteen pending implementation
paths preserved. Initial branch/HEAD/status are recorded locally in ignored
`TestResults/04-02-hotfix/initial-{head,status}.txt`. The tree was intentionally
dirty with uncommitted 04.02 work; no clean checkpoint or new branch is claimed.
`VersionAttributes.vb` initially matched the SHA-256 in the implementation report.
No branch, commit, staging, reset, merge, install, certificate or policy change.

Audited the actual App, DI, readiness, service, transport, module, existing tests,
manifest and launch profile against AGENTS/master plan. The previous label
"Inspect selected range" invoked **ReadSelectionAsync**, including actual cell
matrices. This labeling mismatch is proven by source. It now says **Read selected
range (read-only)**; the full typed reader remains intact. A separate metadata
button is available only with explicit Debug diagnostics.

| Stage / clock | Actual sequence and bound |
|---|---|
| Pane startup | Blocking Office CDN in head, bootstrap diagnostic before it; Blazor loads; accepted readiness module imported/created once; real onReady observed; ExcelApi 1.1 checked only after readiness. Owner reports this succeeds. |
| C# acquire | Explicit button → ReadSelectionAsync → operation ownership → **30,000 ms** linked deadline, then capability check using the existing pane readiness (its own wait ceiling is 12 seconds). Time spent here/import/create consumes the C# budget. |
| Workbook transport | Import workbookAdapter.mjs → factory returning plain adapter, wrapped once by Blazor → beginRead invocation. New stage traces distinguish these steps. |
| JS begin | Its separate **30,000 ms** operation clock starts here, later than C#; creates/uses its own createOfficeHost observer, with **5,000 ms** readiness bound. No context access before resolution. This second observer has not been shown to hang in the owner's frame. |
| First native request | Excel.run initiation → callback entry → selected-range metadata loads → first context.sync → validated sheet/address/dimensions and quota checks. |
| Each payload tile | Fresh Excel.run → selection/sheet preflight sync → bounded values/valueTypes/numberFormat tile sync → selection/sheet reconciliation sync. No RequestContext/range proxy crosses runs. |
| Finish | Fresh Excel.run and final metadata/identity sync → C# lease/budget validation → full Common snapshot returned, or no snapshot on failure. |
| Cleanup | Cancel operation handle; up to two additional seconds for best-effort interop cleanup. A submitted native request is not forcibly abortable; late results are ignored. |

A one-tile full read normally uses **3 Excel.run calls / 5 syncs**. Metadata-only
uses **2 runs / 2 syncs**, including final revalidation. The budget is not increased
or restarted between tiles. C# has less remaining time than JS after initialization
cost; actual callback/event-loop ordering can still let either deadline win.

### Hypotheses and attribution

| Hypothesis | Evidence / conclusion |
|---|---|
| Accepted single-wrap interop regressed | Factories remain plain objects. Actual browser import + C# create/invoke + typed full-read projection passed for the canonical **simulated** workbook; no double wrapping. This rules out that basic failure in the exercised path, not every possible real payload. |
| Extra readiness observer does not settle | Still a hypothesis for the genuine pane. Source ordinarily bounds this at five seconds with `office.timeout`; it does not by itself explain a 30-second `input.time-budget`. Delayed/unresolved observers are tested separately with zero premature workbook calls. No speculative observer removal. |
| Excel.run does not invoke callback | Possible; distinct `*-run-started` without `*-run-entered` identifies it. Simulated regression proves this trace and rejects late callback entry. No genuine observation yet. |
| Native sync hangs | Possible. Separate metadata/preflight/payload/reconciliation/final start/completion events identify which sync. The deliberately stalled browser fixture proves instrumentation, not the owner's root cause. |
| C# capability/import/invoke delay | Possible. C# events and independent timer-origin codes distinguish it; no elapsed timings were provided from the owner's failure. |
| Parent shell CSP, Office.js warnings, VPN/extension or CDN dependency failure | Unattributed; neither ruled in nor ruled out. No task-pane request failure evidence supplied. Do not assign shell/baseFrame/about:blank errors to localhost or weaken sandbox/CSP. |
| Large selection/quota rejection | Source preflights before values. Owner describes a small selection, but its actual metadata has not been observed. Existing oversized/invalid/lifecycle tests remain passing; no limit bypass. |

Only the owner observation is genuine Excel evidence in this amendment. Codex has
no authenticated Office session and has not opened Excel, signed in, sideloaded,
accessed private documents or captured workbook data. A pending question requests
only the owner's same-frame minimal read-control result. Browser/policy comparisons
and the about:blank error's actual frame remain owner investigations.

### Opt-in diagnostics and changes

Use the exact frame-selection, query switch, independent read-only control and
disable instructions in the [README](../../BESHStat.OfficeAddin/README.md#investigating-the-owner-observed-read-timeout-0402-pending).
First select **https://localhost:7141/** in Edge DevTools' execution-context
dropdown and check `location.origin === "https://localhost:7141"`. Do not select
the parent Excel shell, baseFrame or about:blank. The same-frame control loads only
row/column counts in one Excel.run/sync with a ten-second reporting deadline.

The existing `beshOfficeDiagnostics=1` query enables the additional read tracing
only through a **Debug** production-host service. Release compiles the C# recorder
and comparison affordance off and passes `false` to the JS factory diagnostic
option. Bootstrap diagnostics retain accepted 04.01 behavior. Diagnostics have no
network sink, console logging, persistent storage, workbook identifiers or raw
exceptions. Both collectors hold at most **80 allowlisted code/elapsed-ms pairs**.
The pre-existing resource summary has at most 40 category/status/timing entries;
status zero/missing is inconclusive, never proof of blocked dependencies.

The C# UI's **Refresh stage summary** works during a pending read. JS events come
from `__beshOfficeDebug.snapshot()` in that same frame. C# times start at recorder
creation, JS at page diagnostic installation; compare adjacent durations within
each trace. `cs.timeout` covers C# cancellation deadline **and** Common budget
checkpoint expiry. `cs.js-timeout-received` denotes a JS `input.time-budget` reply;
`read.js-timeout` denotes the JS deadline. Near-simultaneous timers may race.
Stages include import/create/invoke failures and native callback/sync boundaries.
No request IDs, sheet names/IDs, addresses, cell values/formulas, exception messages,
stacks, URLs, headers or bodies are recorded. Late callbacks do not publish rows,
completion events or stale diagnostic stages into a newer operation.

The fourteen paths touched in this corrective investigation (other pending batch
paths are preserved):

- `BESHStat.OfficeAddin/App.razor`
- `BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs`
- `BESHStat.OfficeAddin/Services/WorkbookAdapter.cs`
- `BESHStat.OfficeAddin/Services/WorkbookTransport.cs`
- `BESHStat.OfficeAddin/Services/WorkbookDiagnostics.cs` (new)
- `BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs`
- `BESHStat.OfficeAddin/wwwroot/js/officeDiagnostics.js`
- `BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs`
- `BESHStat.OfficeAddin/tests/workbookAdapter.test.mjs`
- `BESHStat.OfficeAddin/tests/workbookReadStages.test.mjs` (new)
- `BESHStat.OfficeAddin/tests/officeDiagnostics.test.mjs`
- `BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs` (new)
- `BESHStat.OfficeAddin/README.md`
- `docs/architecture/batch-04-02-office-workbook-adapter.md`

No readiness algorithm/budget/DTO/limit/selection validation or writer policy is
relaxed. The shared Excel.run helper repeats the existing operation guard before
late callback entry, allowing a separate trace for callback initiation versus
execution. Core/Common, WasmSmoke, accepted readiness implementation and generated
version bytes are preserved. This is an evidence-gathering amendment, not a claim
that changing the label/adding metadata inspection fixes the full read timeout.

### Reproduction and automated evidence

Codex used installed Edge with an isolated, unsigned-in test profile and the local
Debug HTTPS host. Sandbox startup was denied; approved execution used the existing
certificate without changing it. No browser/package installation or browser/OS
settings changes. The local dev server and isolated browser were stopped afterward.

| Actual browser run | Observed result |
|---|---|
| readiness-browser.mjs (no fixture) | Real CDN standalone, module HTTP 200 / text/javascript, office.unavailable, Core 0.5, Common 2/4, no application exception UI; three retries passed. CDN reports hostInfo validation failure in standalone; categorized Office script error, not genuine Excel evidence. |
| workbook-browser.mjs | Real Blazor WASM/import/create/invoke and two HTTP 200 text/javascript modules, **simulated Office/Excel**, shared canonical fixture: metadata and full read workbook.ok, no automatic read, no mutation, Retry no background read. |
| workbook-browser.mjs --pending-sync | **Simulated payload sync stalled** after metadata-only workbook.ok. Payload start **1,262 ms**, JS timeout **31,271 ms** relative to page diagnostic installation (about 30,009 ms later). C# received JS timeout; subsequent simulated read workbook.ok. No application exceptions. This demonstrates attribution, not native Office transport success. |

The browser fixture initially assumed C#'s earlier-started deadline must win; this
assertion was corrected to accept and report either **observed** timer origin.
That does not weaken a production-code test: the clocks are independent and native
event-loop scheduling is part of the behavior under investigation.

New host-free coverage: **8 C# cases** (bounded/disabled recorder; blocked begin,
tile and finish with late completion/retry; JS-vs-C# timeout origin; delayed pane
readiness and metadata/full comparison; cancellation and disposal during payload).
New Node coverage: **14 cases** (13 read-stage tests plus one collector gate/privacy
case), including unresolved/delayed readiness, pending run callback and all five
sync positions, canonical mixed data, metadata-only, cancellation/disposal, retry,
no late completion events and diagnostic sink isolation. Existing full read,
invalid/multi-area/over-limit/lifecycle, interop and fail-closed writer tests remain.
Browser scenarios are separate and not added to distinct unit-test counts.

Final verification results and the owner gate follow below. Previous 70/122/3,271
figures above are the preserved implementation baseline, not the amended inventory.

### Amended verification results

All final commands below exited **0**, using the installed toolchain and cached
packages only. Both `.sln` builds and both OfficeAddin/WasmSmoke builds passed;
both foundation gates checked manifest identity and static assets. Catalog,
metadata, version-first Help, resolver and actual MkDocs MIME/build checks passed.

| Distinct tests | Debug | Release |
|---|---:|---:|
| Core | 1,024 | 1,024 |
| Common | 727 | 727 |
| Windows legacy | 787 | 787 |
| Independent WasmSmoke C# / JS | 45 / 377 | 45 / 377 |
| Help resolver JS | 71 | 71 |
| Python tools / Help / discovery | 27 / 16 / 5 | 27 / 16 / 5 |
| Production OfficeAddin C# / JS | **78 / 136** | **78 / 136** |
| **Distinct total per configuration** | **3,293** | **3,293** |

Delta from implementation floor **3,271: +8 C# +14 JS = +22**. No tests removed,
weakened or skipped. Debug/Release repetitions, foundation/full-gate repetitions
and opt-in browser scenarios are not additional distinct cases. The Razor label
assertion was updated to the accurate full-read label; all prior safety assertions
remain. Builds reported **0 warnings / 0 errors**. Existing non-failing advisory
`UTA001 MatrixTestHelpers`, result-file overwrite notices, Help's single ambiguous
Form+Title/Home fallback and Git's CRLF-normalization notice remain advisory.

Exact commands from repository root (the wrapper saved the version bytes before
builds and restored them in finally; no generator was disabled):

```powershell
$env:RestoreSources = 'C:/Users/peter/.nuget/packages'
$env:PythonExe = 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe'
dotnet test BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj -c Debug --no-restore
node --test BESHStat.OfficeAddin/tests/workbookReadStages.test.mjs BESHStat.OfficeAddin/tests/workbookAdapter.test.mjs BESHStat.OfficeAddin/tests/workbookWriter.test.mjs BESHStat.OfficeAddin/tests/officeDiagnostics.test.mjs
dotnet build BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj -c Debug --no-restore

# Existing isolated-browser endpoint and local Debug HTTPS host; no authentication:
dotnet run --project BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj --no-build --launch-profile https
node BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs --pending-sync
node --check BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs

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

Focused run: **78 C# / 86 Node** passed. The final full foundation/full repository
runs passed **78 C# / 136 Node** per configuration. Logs and sanitized browser
results are ignored under `TestResults/04-02-hotfix`; the test scripts retain their
existing TRX paths. A local logging wrapper initially stopped on successful Python
unittest stderr progress under PowerShell 5.1 Stop semantics. Its exit-code handling
was corrected, then both complete gates rerun successfully; the interrupted run
is not counted as a pass or additional test coverage.

Read-only PE checks again confirmed normal packaged artifacts:

| Configuration | Packed XLL | Bytes | PE machine |
|---|---|---:|---|
| Debug | BESHStatNG-AddIn-packed.xll | 1,948,160 | 0x014C / x86 |
| Debug | BESHStatNG-AddIn64-packed.xll | 1,873,920 | 0x8664 / x64 |
| Release | BESHStatNG-AddIn-packed.xll | 1,880,064 | 0x014C / x86 |
| Release | BESHStatNG-AddIn64-packed.xll | 1,805,824 | 0x8664 / x64 |

Artifact signatures/sizes/hashes are saved in `TestResults/04-02-hotfix/packed-xlls.json`.
Normal builds regenerate ignored packaging; no XLL was manually edited, signed,
installed or loaded. Final generated-version SHA-256 still exactly
`0F22C5A1AD446546E773E59B495E647C58F5FF08A70750AFE7372680798D2F2D`.

### Required owner evidence and stop gate

- [ ] Rebuild/run Debug **7141**, re-sideload the current development manifest in
  a **disposable genuine Excel Online workbook**. Confirm office.excel-ready,
  OfficeOnline, Core 0.5 and Common 2/4.
- [ ] From the verified localhost task-pane frame, provide metadata-only versus
  **full read** status/dimensions and sanitized stage summaries. Run the independent
  minimal control once if needed. If it hangs too, classify the native transport /
  environment as a candidate blocker, without attributing parent-shell CSP errors.
- [ ] Full A1:C4 read returns **workbook.ok / 4 × 3**. Mixed cells, alternate
  selection and a single cell succeed. Metadata-only or simulated success cannot
  satisfy this check. Over 10,000 cells and disjoint selections safely reject.
- [ ] Retry and pane close/reopen do not inherit a stale snapshot/context or hang;
  no background reads and no changes to values, formulas, formatting, selection,
  worksheets or workbook. Production writes remain disabled.
- [ ] Standalone 7141 still office.unavailable, probes pass, read actions absent;
  independent WasmSmoke **7131** still works. Its browser/Excel read smoke was
  **not** run by Codex here; its existing tests/builds passed unchanged.
- [ ] Desktop Excel/Mac checks remain unperformed unless separately observed.

**Stop here on codex/04-02, uncommitted and unmerged.** Genuine failing stage,
root cause and successful full-read acceptance remain owner-gated. No deadline
increase, fabricated success, disabled checks or claim of environmental causality.
Plan remains **223 total / 34 accepted / 189 outstanding**, 04.02 pending;
04.03/04.04/04A remain unstarted. Final pending inventory is twenty-four paths
(fourteen touched in this investigation plus ten retained from implementation).

Final `git diff --check` and whitespace checks for all **12 untracked files**
passed. The 24 paths match the approved batch inventory; no staged changes.
HEAD remains `d7888cd272e5bb304139e2fa3b4632c561f20725`. Final status (including
preserved implementation changes, not just this investigation):

```text
 M BESHStat.OfficeAddin/App.razor
 M BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs
 M BESHStat.OfficeAddin/Infrastructure/HostStatus.cs
 M BESHStat.OfficeAddin/README.md
 M BESHStat.OfficeAddin/manifest-dev.xml
 M BESHStat.OfficeAddin/tests/FoundationTests.cs
 M BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj
 M BESHStat.OfficeAddin/tests/officeDiagnostics.test.mjs
 M BESHStat.OfficeAddin/wwwroot/js/officeDiagnostics.js
 M planning/BESHStatNG_CrossPlatform_MasterPlan.md
 M scripts/verify-beshstatng.ps1
 M scripts/verify-office-foundation.ps1
?? BESHStat.OfficeAddin/Services/
?? BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs
?? BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
?? BESHStat.OfficeAddin/tests/helpers/
?? BESHStat.OfficeAddin/tests/workbookAdapter.test.mjs
?? BESHStat.OfficeAddin/tests/workbookReadStages.test.mjs
?? BESHStat.OfficeAddin/tests/workbookWriter.test.mjs
?? BESHStat.OfficeAddin/wwwroot/js/workbookAdapter.mjs
?? docs/architecture/batch-04-02-office-workbook-adapter.md
```
