# Batch 03.04 — stable fault mapping and safe user errors

Status: **IN PROGRESS / PENDING OWNER REVIEW**. Implementation only; automated
validation and owner host acceptance are separate. No acceptance/merge claimed.

## Checkout and source inventory

Initial working tree was clean on existing `codex/03-04`. Owner explicitly
authorized continuing on that branch after the requested existing-branch stop.
Branch, HEAD and local `main` all pointed to
`8a136407a796a0c574b72d99c986f41022a199b0`. No checkout, reset, ZIP overlay,
remote synchronization or global Git setting was needed. Command-local
`-c safe.directory=C:/Users/peter/source/repos/BESHstatNG` handles the sandbox
account's different Windows owner SID.

The owner confirms 03.03 accepted/committed/merged, including Windows hotfix
and Excel Online acceptance, with 2,755 reported passing tests. Current source
contains real WinForms demo/Cancel controls and the installed/restored
`WindowsFormsSynchronizationContext` hotfix. The old plan/report pending
wording is historical; this batch updates the plan's active checkpoint to
**223 total / 30 accepted / 193 outstanding**, without rewriting the 03.03
report's historical evidence. Gate B E1–E7 and all 75 planned 04A batches remain.

Read before coding: root `AGENTS.md`, master plan (including 03.05/03.06),
03.03 execution report, Gate B freeze, the implementations and tests below.

| Source | Existing distinction retained |
|---|---|
| Common `Results/AnalysisResultDocument.vb` | v1 `AnalysisFault`/`AnalysisWarning`, stable public codes, legacy result/typed detail; failure document is separate from a thrown exception |
| Common `Analysis/SharedAnalysisApplicationService.vb`, registry and ANOVA/KW services | Enum and stable-ID `Run` throw; opt-in `RunDocument` has existing broad Argument/NotSupported/InvalidOperation catches. No changes to those APIs or catches |
| Common `Input/SpreadsheetValidation.vb`, configurable and legacy grouped parsers | Immutable issue/report, error vs nonfatal warning/information; selected offsets, worksheet coordinates, field, original code/message remain in native contracts. Messages can contain group labels |
| Common `Output/WriteResult.vb`, target/capability preflight | Awaiting consent is informational; No is `CancelledByUser`; rejected/failed before write differ from mutation-uncertain/partial; exact consent and no-rollback semantics retained |
| Common `Infrastructure/Diagnostics.vb` | Fixed `diagnostic.*` event allowlist; category/outcome allowlists, per-operation run ID, best-effort delivery; user fault codes never become diagnostic event codes |
| Common `Infrastructure/ExecutionCoordinator.vb` | Request is nonterminal; only owning token/checkpoint confirms cancellation; unrelated OCE fails; completed write wins a late request |
| Windows `UibyID.vb`, `WindowsExecutionRun.vb`, `ExcelDnaHostInfrastructure.vb`, `WindowsDiagnostics.vb`, shared-output adapters/tests | UI-thread ownership, form-local dispatcher, closing deferral, control restoration, actual `LastSharedWriteOutcome`, separate diagnostic lifetime; local logger's existing controlled native-exception troubleshooting path retained |
| Office `Home.razor`, `OfficeExecutionRun.cs`, lifecycle tests and `officeBridge.js` | Awaited acquisition/consent, inline analysis, awaited uninterruptible write, single-use consent cleanup, actual wire result; no JS error text is trusted for presentation |
| Core `StatTests/Parametric.vb`, `Nonparametric.vb`, `StatisticalResultContracts.vb` and Common pilot services | ANOVA/KW return tables/TestResult statistics. No typed fatal convergence/singularity condition is exposed through these services. Generic InvalidOperation/Argument exceptions do not prove a numerical/model condition |

Concrete edit inventory was declared before coding: new Common fault model and
tests; Windows pilot/execution adapter and tests; Office pilot/presentation
adapter and linked lifecycle tests/project; master plan and this report. No Core
adapter was justified by the available typed pilot signals. No JS change needed.

## Additive public contracts and stable code policy

`Common/src/Results/Faults.vb` is `Option Strict On`, has no host, filesystem,
ambient-state or Microsoft.VisualBasic dependency. `UserFault` is sealed with
read-only scalar/string properties and a Friend-only constructor. It retains
no exception, report, mutable array, worksheet address, absolute coordinate or
cell value. `FromReport` copies into a read-only collection of immutable faults.

`UserFaultSeverity`: Information / Warning / Error. `UserFaultCategory`:
Input / Analysis / Host / Output / Cancellation / Unexpected. `Code`, `Title`,
`Summary`, `Action`, optional `OriginalCode`, optional validated `FieldPath`, and
bounded selected row/column offsets form the projection. Run correlation is a
separate host-supplied formatting argument, accepted only as a nonempty GUID in
32-character N format; it is never ambient state.

`UserFaultMapper` offers `FromAnalysisFault`, `FromValidation`, `FromReport`,
`FromWarning`, `FromException`, `FromWriteResult`, `FromWriteState`,
`AwaitingConfirmation`, `FromOutcome`, and `FormatDisplay`. It performs no
logging, worksheet mutation, consent approval or execution-state transition.

Canonical codes emitted (exact ordinal strings; English copy may evolve):

| Code | Category / severity | Proven signal and action |
|---|---|---|
| `user.input-invalid` | Input / Error | Typed validation error; correct range/layout/observations |
| `user.input-warning` | Input / Warning | Nonfatal validation/analysis warning; review before using results |
| `user.input-information` | Input / Information | Parser information; review inferred interpretation |
| `user.cancelled` | Cancellation / Information | Confirmed execution Cancelled; run again when ready |
| `user.host-unavailable` | Host / Error | Typed known unavailable host/missing capability; use supported host/operation |
| `user.unexpected` | Unexpected / Error | Unknown/unclassified/null failure; retry/report code and run reference |
| `analysis.invalid-request` | Input / Error | Existing `AnalysisFaultCodes.InvalidRequest`; check grouped data/options |
| `analysis.unsupported` | Analysis / Error | Existing UnsupportedAnalysis or typed capability unknown/unsupported analysis; select supported analysis |
| `analysis.execution-failed` | Analysis / Error | Existing ExecutionFailed; review inputs/options, no specific convergence claim |
| `output.cancelled` | Cancellation / Information | Writer CancelledByUser; no writes, choose another target/review consent |
| `output.host-failed` | Output / Error | Writer FailedBeforeWrite; check destination/host before retry |
| `output.rejected` | Output / Error | Writer rejected or typed known invalid/unsupported/unwritable/merged/colliding target; choose supported writable target |
| `output.consent-stale` | Output / Warning | Rejected with Changed; inspect and confirm again |
| `output.occupancy-unverified` | Output / Warning | Rejected with Unverified; check destination or new empty sheet |
| `output.confirmation-required` | Output / Information | Awaiting consent or rejected with Occupied/ConfirmationRequired; explicit Overwrite/Cancel |
| `output.mutation-uncertain` | Output / Warning | Partial/possibly mutated write or thrown in-flight interop call; inspect remaining output, no rollback claim |

Completed output produces **no fault**, including late cancellation. Existing
`AnalysisFaultCodes`, `WriteCodes`, input issue codes and diagnostic events keep
their original meanings/values. No schema/JSON fields were added to result,
output-plan, writer or Office wire DTOs. Gate B E1–E7 remain unchanged.

The mapper does not echo an arbitrary code even if it resembles a namespace.
Known analysis codes can be `OriginalCode`; known validation originals are:

```text
cell.not-numeric, cell.invalid-value, cell.excel-error
groups.too-few, range.too-few-groups, groups.too-few-observations
options.invalid, options.long-columns
header.invalid-cell, header.excel-error, header.ambiguous
groups.category-error, groups.category-invalid, groups.category-missing, groups.category-collision
cell.missing-skipped, rows.blank-skipped, header.inferred
range.null, range.shape, range.not-rectangular
```

Unknown validation codes get generic input guidance while retaining their typed
severity; unknown analysis faults get `user.unexpected`. Unknown analysis warnings
remain warnings with generic copy. `options.alpha.defaulted` gets fixed default
alpha copy and safe original-code metadata. Original contracts are untouched.

Allowed fields: `input`, `request`, `analysisId`, `options`, `options.columns`,
`options.alpha`, `observations`, `groups`, `rows`, `header`, `category`, `execution`, `output`, and canonical
`groups[0]` through `groups[16383]`. Other strings, controls, leading zeros,
negative/oversized indexes and unrecognized option paths are discarded. Selected
offsets must be positive and within 1048576 rows / 16384 columns; unknown/invalid
offsets become zero. Worksheet coordinates stay available only in original
issues and are absent from the safe error display.

Exception whitelist: `SpreadsheetInputValidationException` and
`HostCapabilityException` with these recognized capability codes:
`analysis.unknown`, `analysis.unsupported`, `host.unavailable`,
`host.capability-missing`, `output.target-invalid`, `output.sheet-name-collision`,
`output.target-unwritable`, `output.target-merged`, `output.destination-unsupported`.
Other exceptions, including base ArgumentException, InvalidOperationException,
NotSupportedException, arithmetic exceptions, unrelated/default-token OCE and
JS failures, use the fixed unexpected fallback. Never inspect native Message,
ToString, StackTrace, inner exception text or string-matched permission clues.

## Precedence and host integration

1. Actual writer envelope wins: Completed -> no fault; partial/mutation uncertainty
   -> remaining-output warning; CancelledByUser -> informational no-write message;
   failed/rejected before write -> specific safe action. Writer state wins over
   contradictory code text. Unknown rejected codes become `output.rejected`.
2. Without a writer outcome, confirmed execution Completed/Cancelled wins. A
   cancellation request alone neither ends execution nor creates a cancellation
   fault; token ownership remains the coordinator's responsibility.
3. Typed validation report errors precede an explicit mapped analysis fault;
   highest-severity issue (first on ties) supplies the summary.
4. Explicit analysis fault precedes an exception; whitelisted exceptions map
   deterministically, other failures get the fixed fallback. Failed state without
   more evidence is also unexpected.
5. Warning/information-only reports remain nonfatal and valid. Pending/running
   state without an issue produces no fault.

Windows maps only ANOVA/KW validation, catch and writer prompts. Other analyses
retain their old flows. No new Task.Run, DoEvents, background COM, framework
change or extra async event handler. The existing Async Sub retains catch/finally,
UI synchronization-context installation/restoration, TryFail/TryComplete,
ExecutionFormClosing, status/control cleanup and independent diagnostics.
`LastSharedWriteOutcome` resets per run and remains the actual writer result.
Mapped presentation cannot change it. Existing reference-validation prompts are
fixed generic strings; a subsequent pilot prompt adds code/action/correlation.
The existing logger's controlled native-exception troubleshooting call remains;
no new automatic native message logging or diagnostic payload was added.

Office uses the same mapper via `OfficeFaultPresentation`. The adapter recognizes
only exact known writer-state strings; numeric or arbitrary strings fall back
safely. An exception after dispatch of a write warns about uncertain remaining
output; planning/preflight errors do not assume mutation. Completed/partial
writer results are retained even if later handling fails. Confirmation token
cleanup stays in finally; mapping never approves consent or starts a write.
`OfficeExecutionRun` timing, token filters, synchronous analysis/write boundaries
and finalization remain; only diagnostic rejection classification is narrowed to
typed validation instead of all ArgumentException.

Legacy Office grouped parser exceptions are translated only at that exact parser
boundary into a generic typed input report; parser grammar and accepted inputs
are unchanged. Configurable parser details remain visible with safe action,
severity, recognized original issue codes and selected offsets, without interpolated group labels/native messages.
The explicit raw-cell inspection preview is still an intentional smoke-harness
feature; it is not used as an error-message payload. Other acquisition error
displays also use the fixed mapper instead of first-line JS/native exception text.

Safe example displays (run ID appended separately when valid):

```text
Check analysis input [user.input-invalid] The selected input needs correction.
Correct the numeric observation in the selected location, then retry. Selected row 4. Selected column 2.

Destination changed [output.consent-stale] The previous overwrite consent is no longer valid.
No output was written. Inspect the destination and confirm again.

Output may remain [output.mutation-uncertain] Output may be partially written.
No rollback was attempted. Inspect the destination for remaining output before retrying.

Operation could not complete [user.unexpected] An unexpected failure occurred.
Retry the operation. If it repeats, report the code and run reference.
```

## Windows demo and appearance disposition

**Visible demo cleanup implemented; narrowly gated fixture removal deferred.**
Removed `executionDemo` field/checkbox, enable/disable wiring, demo label and its
footer row/span. Removed the unconditional normal-run 100 ms dummy delay;
`Task.Yield` retains the UI message-loop boundary/checkpoint without a timer.
The footer shrinks from 68 to 36 pixels, one operational row with a mnemonic
Cancel button and ellipsized status plus safe full-message tooltip. No Designer
file or nonpilot form was changed.

Dependency audit: Windows tests use the execution adapter and safe checkpoints;
no existing test needs the visible checkbox. 03.05 help metadata needs none of
the demo. 03.06 close-during-run acceptance still needs a reproducible responsive
Windows await: normal statistics and COM writes are synchronous, and a single
yield is too brief for a reliable owner close/cancel interaction. Retain only:

- `WindowsExecutionRun.SyntheticFixtureAsync` and its 50 cancellable 100 ms timers.
- `WindowsExecutionRun.DeveloperFixtureEnabled` (only exact value `1` enables).
- The environment opt-in invocation in `UibyID.vb` using
  `BESHSTAT_DEV_EXECUTION_FIXTURE`, default unset/off. No visible demo controls.

To exercise the temporary fixture, the owner may launch Excel from a process
with that variable set to `1`, run ANOVA/KW, then Cancel/close during the initial
five-second synthetic stage. Remove/unset it for normal appearance/output checks.
Codex does not launch Excel. This is a temporary developer seam, not a production
cancellation promise. The master plan makes removal of these exact symbols,
opt-in and artificial timers mandatory before 03.06 acceptance / Phase 03 exit,
with the named **Restored ANOVA/KW appearance and actual output** owner smoke.
At that removal, retire `ExecutionAdapterTests.DeveloperFixtureRequiresExplicitOptIn`
and change the form source regression's fixture-presence assertion to require
absence, preserving its no-demo and UI ownership assertions.
No additional numbered batch; no claim that fixture cleanup is fully complete.

## Changed paths

New:

- `BESHStat.Excel.Common/src/Results/Faults.vb`
- `BESHStat.Excel.Common.Tests/src/UserFaultTests.vb`
- `BESHStat.WasmSmoke/Infrastructure/OfficeFaultPresentation.cs`
- `BESHStat.WasmSmoke/tests/Execution/OfficeFaultTests.cs`
- `docs/architecture/batch-03-04-fault-model.md`

Modified:

- `BESHstatNG/src/UI/UibyID.vb`
- `BESHstatNG/src/AppInfrastructure/WindowsExecutionRun.vb`
- `BESHStatNG.Test/src/ExecutionAdapterTests.vb`
- `BESHStat.WasmSmoke/Infrastructure/OfficeExecutionRun.cs`
- `BESHStat.WasmSmoke/Pages/Home.razor`
- `BESHStat.WasmSmoke/tests/Execution/Execution.Tests.csproj` (link presentation adapter)
- `planning/BESHStatNG_CrossPlatform_MasterPlan.md`

## Automated validation

No host smoke is implied. Initial compile exposed a VB reserved identifier
(`error`); it was renamed to `failureException` before focused validation.
The initial command below failed compilation before executing tests:

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --no-restore --filter FullyQualifiedName~AnalysisResultDocumentTests
```

Existing tests were not weakened. Focused commands then completed:

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --no-restore --filter FullyQualifiedName~UserFaultTests
dotnet test BESHStat.WasmSmoke/tests/Execution/Execution.Tests.csproj --no-restore
```

Focused results: Common **50 passed**, Office lifecycle **27 passed** (14 existing
+ 13 new), zero failures/skips. NU1900 advisory-feed warnings recur with cached
dependencies; no package installation or network restore was forced.

The first full Debug gate passed Core/Common/Python/Office tests and then failed
the WASM build on the new catalog check (`out` cannot bind to VB `ByRef`). It was
corrected to an explicitly typed descriptor with `ref`. Subsequent gates passed.
Final review added a hostile-exception getter test and four known field-path
cases, giving **55 new Common tests**. The final complete gates below ran after
those changes; final solution builds also validate the last safe Razor display
copy change. No pre-existing unrelated functional failure was found.

Exact final gate commands (each **exit 0**):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Legacy -Wasm -Js -Python -VisualStudio 2026 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Legacy -Wasm -Js -Python -VisualStudio 2026 -Configuration Release
```

Both configurations independently report:

| Suite | Accepted 03.03 | Final Debug | Final Release | Failed / skipped, each |
|---|---:|---:|---:|---:|
| Core | 1,024 | 1,024 | 1,024 | 0 / 0 |
| Common | 580 | 635 | 635 | 0 / 0 |
| Windows legacy | 755 | 767 | 767 | 0 / 0 |
| JS bridge/mock Office | 350 | 350 | 350 | 0 / 0 |
| Python tooling/help/discovery | 32 | 32 (22+5+5) | 32 (22+5+5) | 0 / 0 |
| C# Office lifecycle | 14 | 27 | 27 | 0 / 0 |
| **Distinct total** | **2,755** | **2,835** | **2,835** | **0 / 0** |

Increase: **80 tests** = Common 55 + Windows 12 + Office 13. JS/Python/Core
counts intentionally unchanged because their implementations/bridge are unchanged;
the complete existing suites protect consent cleanup, snapshot protection,
occupied-output No/Yes, rejection, partial writes and numerical compatibility.
The same tests run in two configurations are not counted as different tests.

New tests cover known/unknown codes; typed validation severity/selected offsets;
bounded field paths; exact whitelisted host exceptions; fixed fallback for
Argument/InvalidOperation/unrelated cancellation/unknown runtime failures;
hostile Message/StackTrace/ToString getters; private paths, worksheet labels and
JS-stack-shaped payloads; immutable collections; null/empty inputs; culture
independence; actual writer precedence; late cancellation; diagnostic sink failure
without exception replacement; consent cancellation with zero later writes; and
Windows source/UI ownership and default-off developer opt-in. No Excel automation
is required for these tests.

WASM builds succeed in both gates. The Windows legacy builds pack both
`BESHstatNG/bin/{Debug,Release}/BESHStatNG-AddIn-packed.xll` (32-bit) and
`BESHStatNG-AddIn64-packed.xll` (64-bit). Separate solution commands:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' BESHStatNG.sln /t:Build /p:Configuration=Debug /p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe /m /v:minimal /nologo
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' BESHStatNG.sln /t:Build /p:Configuration=Release /p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe /m /v:minimal /nologo
```

Both separate solution commands returned **exit 0**, including WASM Debug/Release
and 32/64-bit Excel-DNA packing. Final packed file sizes (bytes):

| Configuration | 32-bit packed XLL | 64-bit packed XLL |
|---|---:|---:|
| Debug | 1,943,552 | 1,869,312 |
| Release | 1,874,944 | 1,800,704 |

These packages were not loaded in Excel or installed. No browser was launched.

Warnings: **NU1900**, unavailable NuGet vulnerability advisory feed; cached
dependencies remain usable. Existing **UTA001** for non-public `MatrixTestHelpers`
is unchanged and is not a skipped test. TRX overwrite notices are expected in the
established verification script. No suite was omitted. Debug/Release post-build
scripts exit at the existing non-PreRelease guard, before any signing command.
No signing, installer or deployment executed.

Ignored local evidence:
`TestResults/Codex/Batch0304-Debug-complete.log`,
`Batch0304-Release-complete.log`, `Batch0304-Solution-Debug.log`,
`Batch0304-Solution-Release.log`, and the script's Core/Common/Legacy/OfficeExecution
TRX reports. Earlier iteration logs are retained separately. Initial byte copies
of generated help/version files are in `Batch0304-originals`; only build-generated
timestamp/line-ending changes are restored from those copies before handoff.

Final repository verification (command-local Git ownership exception only):

```powershell
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --stat
```

`diff --check` passes. Final status has **seven modified and five new paths**,
exactly the 12-path inventory above, no staged or unrelated changes. Initial
status was clean. The tracked stat is seven files, 178 insertions / 62 deletions;
the five untracked delivery files are separately listed in status and the source
inventory. Generated help/version bytes match their initial copies; a Git CRLF/LF
normalization advisory for VersionAttributes can appear without a content diff.
Branch/base remain unchanged at `codex/03-04` / `8a13640`; no commit created.

## Owner manual acceptance — not performed by Codex

Windows Excel-DNA (disposable workbook):

1. **Restored ANOVA/KW appearance and actual output**: compare normal appearance
   at the owner's DPI; capture ANOVA and KW screenshots for review. No demo
   checkbox/blank demo row; operational Cancel/status should fit and be keyboard
   accessible without overlap. Check the hidden fixture is off by default.
2. Invalid selection/options: short stable code, corrective action, run correlation;
   no native exception, stack, group values or personal path. Retry after failure.
3. Run valid ANOVA and KW with accepted 03.03 options; compare tables/p-values,
   current/new-sheet/new-workbook paths and supplementary charts.
4. Occupied output No/Yes, safely reproducible stale/rejected/failed/partial output:
   exact writer state, no write on No, explicit remaining-output warning where
   applicable. No rollback claim. Controls/status restore after each attempt.
5. With the explicit temporary environment opt-in, cancel/close during yielding;
   no later output, cross-thread exception or premature disposal. Unset it and
   rerun. A completed write remains Completed even after a late request.

Excel Online:

1. Invalid grouped data/configured options: same canonical input category/action,
   safe issue severity and selected offsets, separately correlated diagnostic run
   ID; no JS/native stack or arbitrary group payload in error text.
2. Valid ANOVA/KW and occupied-output No/Yes: compare output to accepted 03.03;
   current/new-sheet geometry and p-value behavior unchanged.
3. Cancel during yielding/acquisition or consent: no stale approval starts a write;
   completed output stays Completed. Abandoned tokens are cleaned up.
4. Unknown host/interop failure: fixed generic fallback, or explicit remaining-
   output warning if a dispatched write's mutation is uncertain; no JS stack,
   arbitrary data or fabricated permission/convergence diagnosis.

## Limits and next ownership

No typed fatal convergence/singularity signal exists in the pilot contract;
generic analysis failure cannot honestly diagnose one. Future feature batches
may adapt actual typed model conditions when those analyses are authorized.
No Core numerics, input semantics, output DTOs/wire, consent protocol, diagnostic
allowlist, framework target, ribbon/UDF export, localization/help metadata,
workbook lifecycle hook or production Office shell was changed. Core/Common
remain netstandard2.0; Windows remains Framework 4.8; modern host/tests net10.0.

No live Excel/browser acceptance, commit, push, merge, installer, signing,
deployment, 03.05/03.06 implementation or Phase 04/04A implementation. The 03.06
plan criterion is a required future exit condition, not implementation of it.
