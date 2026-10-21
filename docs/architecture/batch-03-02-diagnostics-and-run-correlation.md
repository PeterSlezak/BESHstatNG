# Batch 03.02 — safe host diagnostics and run correlation

Status: **IN PROGRESS / PENDING OWNER REVIEW**, 21 October 2026. Source review
and live owner acceptance remain separate. Branch `codex/03-02` started at
`c754fd9fd05cec7fa579a83fdf73cf9680ac852c`, identical to local accepted `main`.
The only initial modification was the owner's Revision 1.5 master-plan update.
No ZIP was overlaid, branch switched, commit made or remote changed.

## Scope and existing logging

Common adds an independent diagnostic event contract. Core's `IAppLogger`,
`NullLogger`, `DefaultErrorReporter`, `CoreServices`, `SimpleFileLogger`, trace
flag, `LogMsgType`, algorithms and settings are unchanged. Core/Common still
target netstandard2.0; the Windows host/tests remain .NET Framework 4.8.

Before this batch, AutoOpen installed `SimpleFileLogger` through
`AppGlobals.gLogger` -> `CoreServices.Logger`. DefaultErrorReporter could print
and log the original exception/stack. ANOVA/KW used that reporter in the form's
outer catch; KW also logged numerical results after writing. The shared output
seam logged original exceptions, and logger errors could escape its error path.
WASM had no diagnostic sink; its outer catch displayed exception messages.

The new path accepts no exceptions, cells, addresses, formulas, workbook/sheet
names, paths, user/account names, statistical values, request DTOs or consent
tokens. The ANOVA/KW completion logs containing numeric results are removed.
The shared output seam retains its existing `Failure` exception for local host
behavior/tests, but never forwards it to the diagnostic sink. Existing input
objects, detail results, explicit source previews and output outcomes retain
their accepted behavior; they are not diagnostic payloads.

**This is not a privacy retrofit of all legacy logging.** Other Core/host
callers, startup path/version messages, lower-level legacy error reporters and
the unhandled-exception window still have their established behavior. The new
safe record and the two new sinks do not sanitize or reclassify those logs.
Review diagnostic event lines for the run ID, rather than assuming the entire
legacy log is a redacted export.

## Common contract and safe fields

`Diagnostics.vb` is additive and has no host, filesystem, browser, COM,
WinForms, Microsoft.VisualBasic-helper or new package dependency.

| Contract | Meaning |
|---|---|
| `DiagnosticSeverity` | Debug=0, Information=1, Warning=2, Error=3. New diagnostic enum only; no published enum changed. Severity is derived from the event allowlist, not caller-supplied text. |
| `DiagnosticRecord` | Sealed immutable record; constructor private; `Create` projects into allowlists. Read-only fields: EventCode, Severity, Operation, RunId, Stage, optional Timestamp, Metadata. No retained source dictionaries/arrays/objects. |
| `IDiagnosticSink.EmitAsync` | Host-owned best-effort asynchronous delivery. No persistence, retention, retry or authorization policy in Common. |
| `DiagnosticEmitter` | Instance-owned sink/clock/GUID generator; StartAsync allocates a new explicit run and emits started. Defaults: UTC clock and Guid.NewGuid. Injected generators are responsible for unique nonempty IDs; empty/throwing generators fall back to a fresh GUID. |
| `DiagnosticRunContext` | Explicit per-invocation reference, carried through calls/awaits. ReportAsync emits a safe record; EndAsync emits a neutral end once. No AsyncLocal, global current run or durable identity. |
| `DiagnosticDelivery` | Delivered=0 means the sink call returned successfully, **not durable storage/flush confirmation**; Suppressed=1 means trace filtering or an already-ended scope; Unavailable=2 means absent/throwing sink or record-projection failure. |

Run IDs are 32 lowercase hexadecimal characters (`Guid.ToString("N")`), opaque
and unrelated to workbook identity or overwrite consent. They are regenerated
for each invocation, including ones rejected before acquisition. GUID uniqueness
is probabilistic in production; deterministic fixtures inject explicit GUIDs.
No identifier is persisted in preferences.

Operation is exactly `anova.oneway` or `nonparametric.kruskal-wallis`; otherwise
`redacted`. Stage is exactly `invocation`, `validation`, `acquisition`, `analysis`
or `output`; otherwise `redacted`. Timestamp is a nullable DateTimeOffset; an
unavailable clock omits it. Hosts need not forward that optional field: Windows
uses the existing logger timestamp; the browser console receives no timestamp.

| Event code | Severity / meaning |
|---|---|
| `diagnostic.run.started` | Information; invocation has a correlation scope. |
| `diagnostic.stage.entered` | Debug; optional verbose stage observation. |
| `diagnostic.validation.rejected` | Warning; host validation rejected input/options. |
| `diagnostic.output.attempted` | Information; existing output boundary is being invoked. Does not mean mutation occurred or consent was granted. |
| `diagnostic.output.completed` | Information; existing writer returned Completed. |
| `diagnostic.output.incomplete` | Warning; writer returned any other terminal outcome. Cancellation/partial output is never called completion. |
| `diagnostic.run.failed` | Error; exception boundary observed a failure, without exception text/type/stack. |
| `diagnostic.run.ended` | Information; correlation ended, **not an assertion of analysis success**. |
| `diagnostic.event.redacted` | Warning; unknown/null/empty event code replaced with this fixed code. |

Metadata has at most three canonical keys. Only three dictionary lookups occur;
the implementation does not enumerate/copy an unbounded incoming dictionary.

| Key | Exact permitted values |
|---|---|
| category | input, host, analysis, output, unexpected |
| outcome | completed, rejected, failed, cancelled, incomplete, ended |
| host | windows, office |

Unknown keys are dropped. Unknown/null/empty/long values become the fixed
`redacted` marker. All string allowlist values use ordinal equality, with a
64-character precheck; nothing is trimmed, escaped, partially preserved or
truncated into an accepted value. Canonical keys are copied into a new ordinal
dictionary and exposed through ReadOnlyDictionary. A caller's dictionary
comparer can match a key, but cannot introduce a noncanonical output key/value.
Formulas, A1 references, paths, CRLF, NUL, Unicode and account-like text therefore
cannot pass unless they literally equal a predefined safe code. No generic
object/string serialization, regex-based secret detection or Exception.Message
sanitizer is used.

TraceEnabled is captured per run. Debug events are suppressed when false;
informational lifecycle/output events and essential warnings/errors remain.
Windows also retains SimpleFileLogger's existing runtime Debug filter. Creating
a scope never applies preferences or mutates Core/global defaults.

Hosts emit sequentially within each run and explicitly end in Finally. The
small end guard prevents repeated end/post-end calls; it does not serialize
arbitrary competing in-flight emissions on one context. Independent concurrent
runs are supported/tested. This is a correlation lifetime, not the 03.03
execution/progress/cancel state machine. There is no rollback or cancellation
promise, sampling, Common event store or export protocol. Later 03.04 may add
an explicit bridge from user fault codes to these diagnostics; these event
codes make no promise of being that future fault taxonomy.

## Host call graphs and exception boundaries

```text
Windows AutoOpen -> existing file logger -> CoreLoggerDiagnosticSink
UibyID.Compute -> new WindowsDiagnosticRun (started, before checkInputs)
  -> checkInputs / acquisition failure -> rejected + safe run/code/stage message
  -> acquisition -> shared ANOVA wrapper or KW -> unchanged dispatcher/Core
  -> RunSharedOutput (output.attempted)
     -> existing destination, exact-cell occupancy and separate chart consent
     -> existing SafeWrite/results (completed or incomplete)
     -> shared output exception catches -> Fail once, retain original locally
  -> outer catch -> rejected for ArgumentException, otherwise failed
  -> Finally -> Dispose/ended; detach diagnostics from retained output session
```

Windows formats only safe strings and uses Info/Warn/Debug/WriteError(string)
on the existing logger. It never uses the exception overload, creates a second
file logger or owns/disposes that logger. The bridge completes synchronously;
the Windows wrapper waits only on this synchronous bridge on the current COM
thread. No COM access moves to a worker thread. Sink throws are caught by Common
without another log attempt, preserving the original operation failure and
consent path. A repeated inner/outer output catch emits failure only once.

Logs remain XLL-adjacent `Logs/all.log`, `errwarn.log`, `err.log`, with original
retention/reset/disposal behavior and `Ui10ShowLog` unchanged. Existing warning
and error entries may include the logger's method-only captured call stack;
no original exception or source path is supplied by this bridge.

```text
Home.RunSharedAnalysisAndWrite -> busy flag + captured analysis kind
  -> new emitter/BrowserDiagnosticSink -> started, before host preflight/read
  -> target/capability validation -> acquisition -> grouped parser -> analysis
  -> existing semantic/layout planners -> output.attempted
  -> existing officeBridge writer -> optional AwaitingConfirmation/No/Yes
  -> completed or incomplete + small safe run/code/stage status
  -> catch -> rejected for non-output ArgumentException, otherwise failed
     -> fixed actionable message, no original JS/.NET exception text/stack
  -> Finally -> await ended; existing busy/confirmation cleanup
```

`BrowserDiagnosticSink` projects only the safe event fields to
`beshDiagnostics.emit`. It does not send the selection, requests, preferences,
interop response/error or consent token. The JS bridge independently allowlists
fields, recomputes severity, checks exact 32-character run IDs and freezes both
the console object and metadata. It emits one console info/warn/error/debug
call, returning false if the console/getter/method throws or is unavailable.
The .NET adapter turns false into a fixed local failure; Common returns
Unavailable. A missing JS bridge, disposed interop runtime or rejected interop
task is caught at the same dispatcher boundary. No raw second-chance logging.

Browser diagnostics have no storage access, queue, telemetry/network delivery,
reload persistence or application retention. DevTools/browser console retention
is host-controlled. Status says console-only/best-effort, never claims storage
or successful delivery. Common tests cover async throwing sinks; Node tests
cover console failure. There is no automated Razor component/real JSRuntime
integration runner in this repository: that wiring is build-checked and remains
part of owner smoke acceptance. Unrelated preview/composite demo actions keep
their existing behavior and do not gain diagnostic scopes in this batch.

The 03.01 preference store/save/reload code is unchanged. Office uses the active
snapshot's TraceEnabled without applying global defaults. Existing shared
layout, p-value semantics, three Windows destinations, two supported Office
destinations, exact occupancy, overwrite No/Yes, Worksheet.CodeName fix and
new-sheet-only composite restriction are preserved. Gate B v1 DTOs/wires,
E1–E7, statistics, golden/graphics fixtures and public entry points are unchanged.

## Automated evidence

Focused Common diagnostics: **17/17**; Gate B compatibility: **6/6**;
new Windows diagnostics/safe-output cases: **13/13**; new Node cases: **16/16**.
The initial broader Windows `~Diagnostic` filter also passed 25/25, including
12 pre-existing unrelated diagnostic tests. The precise final filter below
selects only the 13 new Windows cases.

Tests cover deterministic ID/time fixtures; 64 concurrent Common runs with
four events each; async correlation; absence of ambient IDs; rejected/failed
runs with trace disabled; defensive copying/read-only metadata; bounded unknown
fields; null/empty/long/CRLF/NUL/Unicode/formula/path/account-like strings;
preservation of original validation/host exceptions with throwing/disposed
sinks; no recursive logging; existing actual ANOVA/KW writer seams, all three
destinations and overwrite No/Yes; absent/throwing consoles/getters; and
unchanged browser preference reload. No existing assertions were weakened and
no golden fixture was regenerated.

Initial test-only nullable-Boolean/VB Await-in-Finally compile errors were
corrected. The first Windows build exposed an omitted forwarding parameter in
the ANOVA wrapper; it was corrected before tests/final builds. Final source
review also moved the existing WASM busy flag ahead of the new diagnostic await
and captured analysis kind to prevent correlation/execution drift.

```powershell
dotnet test .\BESHStat.Excel.Common.Tests\BESHStat.Excel.Common.Tests.vbproj --no-restore -c Debug --filter FullyQualifiedName~DiagnosticTests --logger "trx;LogFileName=Diagnostics.trx" --results-directory TestResults/Codex/Batch0302
dotnet test .\BESHStat.Excel.Common.Tests\BESHStat.Excel.Common.Tests.vbproj --no-restore -c Debug --filter FullyQualifiedName~GateB --logger "trx;LogFileName=GateB.trx" --results-directory TestResults/Codex/Batch0302
node --check BESHStat.WasmSmoke/wwwroot/js/diagnostics.js
node --test BESHStat.WasmSmoke/tests/diagnostics.test.cjs
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\BESHStatNG.Test\BESHStatNG.Tests.vbproj /t:Build /p:Configuration=Debug '/p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' /m /v:minimal
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' .\BESHStatNG.Test\bin\Debug\BESHStatNG.Test.dll '/TestCaseFilter:FullyQualifiedName~DiagnosticAdapterTests|FullyQualifiedName~SharedOutputPlanTests.Diagnostics_' '/Logger:trx;LogFileName=WindowsDiagnosticsFocused.trx' '/ResultsDirectory:TestResults\Codex\Batch0302'
```

The requested full gate runs from repository root under VS2026 Developer
PowerShell, initialized by the installed `Launch-VsDevShell.ps1
-SkipAutomaticLocation`. An ignored launcher invokes exactly:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Python -PythonExe 'C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' -Js -Wasm -Legacy -VisualStudio 2026
```

`cmd.exe /d /c` redirects the launcher output to
`TestResults/Codex/Batch0302/full-gate.log`; this avoids Windows PowerShell's
native-stderr redirection behavior. Full gate exit **0**; no failed/skipped cases:

| Suite | Accepted 03.01 | 03.02 | Added |
|---|---:|---:|---:|
| Core | 1,024 | 1,024 | 0 |
| Common | 543 | 560 | 17 |
| Windows | 732 | 745 | 13 |
| JavaScript | 332 | 348 | 16 |
| Python | 32 | 32 (22+5+5) | 0 |
| **Total** | **2,663** | **2,709** | **46** |

WASM Debug and Windows Debug builds pass with zero compiler warnings/errors.
The unchanged UTA001 MatrixTestHelpers discovery advisory is informational.
Inventory/catalog/generated-metadata checks and retired-helper audit pass.
No network/package installation was needed.

The additional VS2026 Developer PowerShell Debug solution command also exits 0:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' /m /v:minimal
```

Both `BESHstatNG/bin/Debug/BESHStatNG-AddIn-packed.xll` and
`BESHStatNG-AddIn64-packed.xll` are built/packed. These are Debug build
artifacts, not deployed or loaded in Excel. The Debug post-build routine exits
before its existing PreRelease signing guard; installer/signing settings are
untouched. Ignored evidence resides under `TestResults/Codex/Batch0302`, with
full suite TRXs under `TestResults/Codex`.

## Owner smoke acceptance — not run by Codex

1. **Windows Excel-DNA:** in a disposable workbook open ANOVA and KW and submit
   empty/invalid grouped input. Confirm a safe message with one 32-character run
   ID and event/stage, matching the `diagnostic.*` lines in Show Log. Check those
   event lines contain no sheet/workbook names, A1 references, formulas, raw cell
   text, data or exception stack. Repeat with trace off: rejection/warning/error
   and started/ended still appear; stage debug events do not. Re-run valid ANOVA
   and KW and compare output. Exercise current/new sheet/new workbook, occupied
   output No then Yes, and separate chart consent as applicable. Restore trace
   preference after the check. Broader legacy log content is outside this safe
   projection's privacy claim.
2. **Excel Online/WasmSmoke:** run with a controlled bad source address and with
   invalid grouped input; compare the task-pane run/code/stage with DevTools
   console safe records. Source-address interop failures may use run.failed at
   acquisition; Common grouped validation uses validation.rejected. Check no
   raw selection/formula/error response reaches these records. A throwing or
   absent console/bridge must not change analysis/write behavior or crash the
   task pane. Run valid ANOVA/KW and test occupied output Cancel/Overwrite at
   C10; confirm composite current-sheet refusal remains. Save preferences/reload
   where browser storage allows it and verify unchanged persisted values;
   storage denial must still report session-only preferences truthfully.

No live Excel, browser navigation, Mac smoke, installer, signing or deployment
was executed. Automated source tests/builds do not certify those host checks.

## Delivery inventory and planning

New files:

- `BESHStat.Excel.Common/src/Infrastructure/Diagnostics.vb`
- `BESHStat.Excel.Common.Tests/src/DiagnosticTests.vb`
- `BESHstatNG/src/AppInfrastructure/WindowsDiagnostics.vb`
- `BESHStatNG.Test/src/DiagnosticAdapterTests.vb`
- `BESHStat.WasmSmoke/Infrastructure/BrowserDiagnosticSink.cs`
- `BESHStat.WasmSmoke/wwwroot/js/diagnostics.js`
- `BESHStat.WasmSmoke/tests/diagnostics.test.cjs`
- `docs/architecture/batch-03-02-diagnostics-and-run-correlation.md`

Modified files:

- `BESHstatNG/BESHStatAddIn.vb` — safe bridge composition/teardown.
- `BESHstatNG/src/AppInfrastructure/ExcelDnaHostInfrastructure.vb` — host sink reference only.
- `BESHstatNG/src/UI/UibyID.vb` — ANOVA/KW scope, safe failure display and explicit correlation forwarding only; histogram UI/handlers unchanged.
- `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb` — safe diagnostic hooks in the existing shared run; no consent/writer logic change.
- `BESHstatNG/BESHStatNG.vbproj` — one explicit compile entry.
- `BESHStatNG.Test/BESHStatNG.Tests.vbproj` — one explicit compile entry.
- `BESHStatNG.Test/src/DataManagement/SharedOutputPlanTests.vb` — eight new seam cases, existing assertions unchanged.
- `BESHStat.WasmSmoke/Pages/Home.razor` — explicit shared run and safe diagnostic status.
- `BESHStat.WasmSmoke/wwwroot/index.html` — separate console bridge script.
- `planning/BESHStatNG_CrossPlatform_MasterPlan.md` — preserved owner's existing Revision 1.5 update, aligned pending-owner-review wording and explicit future histogram-consumer/PSM exclusion note.

Revision 1.5 remains **223 total = 148 original + 75 Phase 04A; 28 accepted,
195 outstanding**. 03.01 is complete; 03.02 is pending review, not accepted.
Gate B E1–E7 and the dated 18 October Phase 04A audit remain unchanged.
**04A.75 is scheduled only, not implemented.** It depends on 04A.30/34/48 and
is a prerequisite of 04A.71/74. No histogram engine, UDF, graphics, Designer or
histogram UI logic was changed; no 03.03/03.04 implementation was started.

Generated HelpLinks/HelpContext/version source bytes are restored from exact
pre-build copies after validation. SHA-256 comparison of **1,911 initially
tracked files** finds only the **10 authorized modified paths** listed above;
all other tracked bytes, including Core, accepted statistical/golden/graphics
fixtures, generated originals and the Phase 04A audit, match the initial
manifest. There are also **8 authorized new files**, with no deletions or staged
changes. Both existing stashes remain untouched. `git diff --check` passes.
No staging, commit, push, merge, branch switch, installer, signing, deployment
or framework-target change is authorized/performed. Stop for independent
source review and the separate live owner checks above.
