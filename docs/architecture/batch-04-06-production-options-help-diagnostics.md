# Batch 04.06 — production options, Help and diagnostics

Implemented; pending owner acceptance. Baseline: clean local `main`,
`d9255ea903739fd094c43f1bf18923f3515ca87c`. Owner accepted 04.05 on 25 October 2026;
historical pending wording is superseded. Work branch: `codex/04-06`.

**Final acceptance amendment:** the owner accepted the base 04.06 functionality
and requested Force English (default ON) before final acceptance. The amendment
at the end of this report supersedes the original locale-selection and
persist-before-retire descriptions below. Final batch acceptance remains pending
this addition; the earlier test counts describe the original implementation.

## Actual-source audit and planned ownership (before implementation)

| Existing contract / source | Current consumers | Required change / owner |
|---|---|---|
| Common `PreferenceSnapshot`, `PreferenceData`, `IPreferenceStore`, schema 1 | Windows legacy adapter; independent WasmSmoke browser store | Reuse unchanged; Office owns browser adapter and one scoped effective session. No workbook settings. |
| Core `AnalysisDefaultsOptions`, `PValuePresentationSettings` | Windows `Ui12GlobalSettings`, Common normalization/output planner | Preserve numerics and normalization; Office validates draft text before Common normalization. |
| `AnalysisEditorRegistry`, `AnalysisOptionDraft` | Office navigation and ANOVA/KW editors | Office supplies global alpha; track inherited versus explicit alpha. |
| `AnalysisPilotState`, `IPilotHost` retirement/generation, native safety latch | Office guarded Run/Write | Pin preferences per run; reject application while busy; retire idle results/consent without clearing uncertain/completed outcomes. |
| `DiagnosticEmitter`, `DiagnosticRunContext`, `DiagnosticRecord`, `IDiagnosticSink` | Windows/Common lifecycle; Office development-only stage collector | Office bounded session log; additive exact code allowlist only where necessary. |
| Common execution/fault contracts; Office bounded operation state | Windows coordinator; Office pilot acquisition/inspection/write | Faithful stage projection of current Office state, indeterminate progress; no second coordinator. |
| `HelpDestination`, `HelpLaunchTarget`, generated method catalog | Windows and WasmSmoke | Office host launcher, same allowlist and published resolver; build assembly version, no invented version URL. |
| `App.razor`, `AnalysisShell`, builder previews | Two sticky tabs; opt-in development probes | Three accessible retained panels; Options, useful Log, contextual Help. Office resources own new English/Slovak strings. |

Inspected settings, diagnostics, execution/fault/help/lifecycle Phase 03 reports,
04.04/04.05 reports, Office README and verification scripts. Reports are historical
evidence, not a replacement for actual source. `Directory.Build.props` currently
sets `BeshHelpResolverPublished=true`; Common compiles `BESH_HELP_RESOLVER_PUBLISHED`.
The SDK supplies the Office assembly version from the existing root build props.

Protected baseline SHA-256 inventory (15 README files, four version/assembly
files, build props and authored methods metadata):
`TestResults/04-06/protected-before.json`. No protected content is an edit target.

Planned paths: Office `App.razor`, foundation registration, option draft/registry,
pilot component/state, analysis shell, CSS; new Office preference/browser, Help,
locale/resources and diagnostic services/components/modules/tests; only an
additive diagnostic-code allowlist and focused tests in Common if needed; this
report and only the master plan active checkpoint. No Core/Windows/WasmSmoke edits.

## Windows parity

| Control | Accepted policy |
|---|---|
| Alpha | finite, strictly between 0 and 1; default .05 |
| Seed | blank / Int32.MinValue = no explicit seed; zero normalizes to no explicit seed; other signed Int32 retained |
| Trace | default true; suppress debug only, retain warnings/errors |
| P-value digits | Core bounds 2–16, default 8 |
| Small p-values | LessThanThreshold / ScientificNotation / FixedDecimal, Core threshold policy |
| Near-one upper bound | default true |

No Core-wide defaults are mutated. ANOVA/KW remain deterministic.

## Implementation and persistence

`OfficePreferenceStore` implements the existing Common `IPreferenceStore`.
`OfficePreferenceSession` is registered once with scoped ownership in
`AddOfficeFoundation`; registry, pilot and Options resolve that same owner.
Initialization is awaited before the Analysis shell is exposed. A late load
cannot supersede Save: initialization and saves serialize, and a concurrent Save
is rejected while application is in progress. An unsuccessful load or save never
replaces the effective snapshot. Applying increments `Revision` exactly once;
`Changed` also announces transient application/status changes, so consumers use
the revision to identify effective changes.

Browser transport is the Common DTO itself, with camel-case property names:

```json
{
  "schemaVersion": 1,
  "defaultAlpha": 0.05,
  "defaultRandomSeed": -2147483648,
  "traceEnabled": true,
  "pValues": {
    "decimalPlaces": 8,
    "smallValueDisplay": 0,
    "useUpperBound": true
  }
}
```

`preferences.mjs` alone accesses `localStorage`, using only
`beshstatng.office.preferences.v1`, bounded to 4,096 UTF-8 bytes. It uses JSON
parse/stringify, exact field/type checks, explicit schema version and readback
verification. It rechecks the existing record immediately before Save. Unknown
versions/fields, malformed JSON and incompatible scalar types are preserved;
there is no migration or delete/reset recovery. Missing optional fields and
representable out-of-policy numbers are passed to `PreferenceSnapshot.Normalize`
and Core policy. Loading never writes. No workbook identities or data are stored.

| Outcome | Effective state / UI |
|---|---|
| Missing record | Common defaults, SessionOnly; no persistence claim |
| Valid complete v1 | Loaded / Persisted |
| Missing or normalized fields | Normalized / SessionOnly until intentional Save |
| Verified persistent Save | Saved / Persisted; atomically apply candidate after retirement |
| Storage denied, quota, missing API, discard, timeout, cancellation | Existing effective values retained; StorageUnavailable warning; explicit session-only action |
| Future version / invalid saved record | UnsupportedVersion / InvalidData; original data untouched; explicit session-only use available |
| Explicit session-only Save | Existing `InMemoryPreferenceStore`, Saved / SessionOnly; durable key untouched |

Interop import/read/write has a five-second bound and fixed safe failure results;
the existing pilot retains its thirty-second operation deadline. Browser storage
calls are synchronous within their individual JS invocation; no background
storage retry exists. A task pane's browser policy/partition can still deny or
scope storage differently after a reload/context change. Persistent means the
origin-local write/readback succeeded, not a guarantee about browser retention.

Options edits are detached strings. Save validates fields; Discard reloads the
effective snapshot; Restore defaults changes only the draft. Alpha input in both
languages uses invariant decimal point, permits finite exponent notation, and
rejects commas/grouping. Seeds accept a signed Int32 or blank; zero normalizes
through Core. All three display modes use Core's threshold and number formats;
there is no separately persisted threshold.

## Settings precedence, execution and consent

New ANOVA/KW drafts inherit the effective global alpha. Editing analysis alpha
marks an explicit override, including an invalid draft or an edit to the same
number. A subsequent global Save updates inherited drafts and preserves explicit
overrides. Reset analysis options returns alpha to the current inherited value.
The UI labels this distinction and keeps the selected method/input references.

The pilot pins the immutable preference snapshot at Run and captures the validated
analysis alpha with its option snapshot. Output presentation uses these pinned
values, including an explicit analysis alpha override. It never reads a newer
global preference mid-run/write or mutates `CoreServices` defaults. Seed remains
available through `CreateAnalysisDefaults` for future seed-aware methods.

The existing generation/retirement model remains authoritative. Participants
reject application while acquisition, validation/calculation, output inspection,
selection capture, builder discovery/preview or native write is active. No Save
is silently queued for later execution. The Options view explains why Save is
disabled. During persistence/retirement, new analysis actions are disabled too.
At idle, successful Save retires old previews, output plan and single-use consent
and awaits host retirement. Source-reference text remains; Run reacquires and
validates it without a new selection. There is no automatic rerun or workbook
mutation. A later write always inspects sources/target and requires fresh consent.

Verified `WriteOutcome` completion and the pane-wide uncertain-mutation latch
survive application. Preferences cannot turn uncertainty into completion, clear
the latch or authorize retry. A submitted native write remains noninterruptible;
late cancellation retains the actual observed writer result. These paths use the
existing host safety latch and `UserFaultMapper` precedence rather than a second
writer state machine.

Progress projects the existing pilot using Common `ExecutionStage`: acquisition,
validation, calculation, output inspection/write and confirmation. Busy progress
is indeterminate because this pilot reports no measured percentage. Terminal
codes/outcomes and cancellation requests remain distinct. Localized short
messages use status/alert semantics; existing technical code/guidance is available
in a collapsible view. Sensitive source-selection detail remains in Analysis.

## Help integration

`OfficeHelp` uses the running Office assembly version supplied by the existing
build props through `ProductVersionDescriptor.FromVersion`. It uses
`HelpLaunchTarget.CreateHome`, `HelpDestination.ForPageKey("GlobalSettings")` and
`ForAnalysisId` for the selected method. The generated catalog supplies routes,
titles and the JS allowlist; no hand-maintained product URL array was added.
`BeshHelpResolverPublished=true` is verified in actual build props. With this
published flag, targets go through the accepted documentation-origin resolver,
which verifies the exact versioned page before choosing identical latest/home
fallback. The app never treats a constructed version URL as publication evidence.
With the flag disabled, the existing latest policy applies. Unknown topics use
Help home with a nonintrusive fallback notification.

C# validates every target through `HelpLaunchTarget.SafeOpenUrl`. The JS bridge
accepts only the generated allowlist or canonical published resolver queries with
known keys and a canonical four-part version. User-activated Help prefers
`Office.context.ui.openBrowserWindow`; a normal return is reported as **requested**,
because the API returns void and cannot confirm page loading. Throws/unavailable
Office bridge expose a deliberate allowlisted Help-home link. In standalone
browser mode only, popup fallback distinguishes a returned window from a blocked
popup. Help never reads/writes Excel or changes preview/generation state.

Microsoft source references checked during implementation:
[runtime/storage distinctions](https://learn.microsoft.com/en-us/office/dev/add-ins/testing/runtimes),
[SharedRuntime storage requirements](https://learn.microsoft.com/en-us/javascript/api/office-runtime/officeruntime.storage),
[external browser launching](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/dialog-api-in-office-add-ins),
[Office UI API](https://learn.microsoft.com/en-us/javascript/api/office/office.ui).
No SharedRuntime or manifest requirement was added.

## Diagnostic privacy and localization

`OfficeDiagnosticLog` is scoped, session-local memory: at most 200 sanitized events
and 64 KiB of serialized log text. The view supports severity filtering, selection
and Copy; there is no filesystem logger, persistent log retention or upload.
Trace preferences suppress debug stages only. Warnings/errors and safety outcomes
remain available. The ring buffer accepts only immutable Common `DiagnosticRecord`
objects; observer/sink failures cannot mask analysis/write results. Export contains
safe UTC timestamp, fixed `office` host, run ID, severity, event code, operation
and allowed stage. It excludes raw metadata, source values/labels/names/addresses,
formulas, tokens, URLs and exceptions.

Real events cover initialization/retry, Run acquisition/validation/calculation,
failures, user cancellation, output inspection/consent and observed completion or
incompletion. Worksheet name allocation emits `output.sheet-name-adjusted` without
either name. Common's additive allowlist adds that warning, host ready/unavailable/
retry, user cancellation, output consent, analysis completion and operation `office`.
Old allowlist values and redaction remain unchanged. Run IDs persist across a
result's inspection/consent/write, change on a new Run, and suppress reports after
the prior diagnostic scope ends. Development read/probe actions remain behind
Debug plus `?beshOfficeDiagnostics=1`.

English and Slovak `.resx` resources cover new Options controls/status/validation,
Help actions/failures, Log controls/privacy, tabs and short progress/error messages.
Both resource sets are embedded in the main assembly so an Office `sk-SK` locale
works even if browser startup selected English; this is explicitly browser-tested.
One adapter prefers Office display language, then browser language, then `en`;
unsupported languages deterministically use English. Components contain no
culture-specific branches. Stored JSON/numbers, method IDs, Help keys, canonical
codes, statistical doubles and Excel number formats remain invariant.

**Partial localization boundary:** existing analysis navigation/descriptions,
ANOVA/KW editor labels, detailed source/builder/output controls, legacy diagnostic
probes and technical guidance remain English. Authored method titles are retained.
This batch does not claim that the entire add-in is translated. Numeric p-values
remain numeric under every display mode, including all accepted post-hoc tables.

## Automated verification and evidence

Final full gates and final Office foundation gates exit **0** in Debug and Release.
Core/Common remain `netstandard2.0`; Windows/legacy remain Framework 4.8; both WASM
hosts build. No package install or new dependency was needed. The installed cache
was used with process-local `RestoreSources` pointing to repository `packages` and
`NuGetAudit=false`; no network/package-install recovery was attempted. The scripts
and their assertions are unchanged.

Commands from the repository root (each final invocation exit **0**):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Release -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
node --test BESHStat.OfficeAddin/tests/*.test.mjs
node --test BESHStat.WasmSmoke/tests/*.test.cjs
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
```

The full `-Legacy` invocations use VS 2026 through the actual script parameter,
not the script's default VS 2022. Python discovery selected the existing
`C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe`.
The full gates include Python catalog/generator/inventory/resolver/MIME checks,
Help Node, independent WasmSmoke managed/Node, production Office managed/Node,
Windows tests and packed XLL builds. Final foundation reruns cover the last
Office-only diagnostics/markup changes; other layers were unchanged afterward.

| Distinct suite | Debug passed | Release passed | Failed / skipped |
|---|---:|---:|---:|
| Core MSTest | 1,024 | 1,024 | 0 / 0 |
| Common MSTest | 739 | 739 | 0 / 0 |
| Windows .NET Framework MSTest | 787 | 787 | 0 / 0 |
| Office managed/component MSTest | 467 | 467 | 0 / 0 |
| WasmSmoke managed | 45 | 45 | 0 / 0 |
| Office Node | 446 | 446 | 0 / 0 |
| Help Node | 71 | 71 | 0 / 0 |
| WasmSmoke Node | 377 | 377 | 0 / 0 |
| Python | 27 + 16 + 5 | 27 + 16 + 5 | 0 / 0 |
| **Distinct total per configuration** | **4,004** | **4,004** | **0 / 0** |

Compared with the owner's 3,888 baseline: **8 Common + 63 Office managed + 45
Office Node = 116 additional checks**. Repeated focused, full, foundation and
standalone Node invocations count only once per configuration. Browser scenarios
and asset/architecture checks are separate from this total.

Evidence under `TestResults/04-06/`: `portable.log/.exit`,
`full-Debug-final.log/.exit`, `full-Release.log/.exit`, configuration-specific
`Debug/` and `Release/` TRX copies, `foundation-{Debug,Release}-final.log/.exit`,
`office-node.log/.exit`, `wasm-node.log/.exit`, `new-assets.json` and
`xll-architectures.json`. Final foundation also checks manifest/static assets,
fingerprinted assemblies and absence of test assets; the two new JS modules have
one unambiguous static-asset entry in each configuration.

Builds report **zero compiler warnings/errors**. Existing nonfatal advisories:
`UTA001` on non-public legacy `MatrixTestHelpers`; one ambiguous generated
Form+Title mapping intentionally falls back to Help home; repeated TRX overwrite
notices. No pre-existing failing test was waived. Early failures were investigated:
test DI/disposal setup and old two-tab count, completion-status preservation,
stage projection at source pinning, and resource loading when Office/browser
languages differ. Early commands returned 1; focused corrections and final gates
returned 0. Initial managed results are retained in `initial.trx`, `components.trx`,
`options-focused.trx`, `full-Debug.log`; final focused evidence includes
`common-focused.trx` (63/63), `options-focused-final.trx` (62/62), `locale.trx` (6/6).
An initial WASM `CA1416` warning for synchronous semaphore use was fixed with
`WaitAsync(0)`, not suppressed.

The pre-existing legacy build target regenerates `VersionAttributes.vb`. Its
original bytes were backed up as `VersionAttributes.before.bin` and restored in
`finally` after every full legacy build. Final SHA-256 matches the baseline.
The existing non-PreRelease post-build guard exits before signing. No installer,
signing, deployment, publishing, commit, push or merge ran.

| Packed XLL | Bytes | PE machine |
|---|---:|---|
| Debug x86 | 1,948,672 | `0x014C` |
| Debug x64 | 1,874,432 | `0x8664` |
| Release x86 | 1,882,112 | `0x014C` |
| Release x64 | 1,807,872 | `0x8664` |

Read-only checks verify MZ/PE signatures and architecture for all four; none was
loaded into Excel. Their SHA-256 values are in `xll-architectures.json`.

## Simulated Edge and remaining host limits

**19/19 SIMULATED scenarios pass**, with individual final exit codes 0 in
`browser/outcomes-final.json`. This includes the three new production Options
cases plus all sixteen existing 04.04/04.05 scenarios:

```powershell
dotnet run --project BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj --no-build --launch-profile https
node BESHStat.OfficeAddin/tests/browser/production-options-browser.mjs
node BESHStat.OfficeAddin/tests/browser/production-options-browser.mjs --sk
node BESHStat.OfficeAddin/tests/browser/production-options-browser.mjs --storage-blocked
node BESHStat.OfficeAddin/tests/browser/specification-builder-browser.mjs
node BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs
node BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs --uncertain
node BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs
node BESHStat.OfficeAddin/tests/browser/format-input-browser.mjs
node BESHStat.OfficeAddin/tests/browser/corrective-workflow-browser.mjs
node BESHStat.OfficeAddin/tests/browser/options-browser.mjs
node BESHStat.OfficeAddin/tests/browser/options-browser.mjs --standalone
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs --pending-sync
foreach ($fixture in @('excel','pending','unsupported','missing','dependency','bridge-regression')) {
    node BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs "--fixture=$fixture"
}
```

A fresh hidden headless Edge profile under `TestResults/04-06/edge-profile`, debug
port 9227, intercepted Office.js with synthetic fixtures. No authenticated profile
or workbook was used. The sandbox account lacked access to the existing HTTPS
certificate; the approved local dev host used that existing certificate outside
the sandbox. No certificate was generated/installed/trusted. Fresh hidden Edge
launch also needed sandbox approval. Test-only origin storage cleanup touched
only the namespaced preference/test-marker keys, never `localStorage.clear()`.

New scenarios verify Save/reload persistence, intentional session fallback,
invariant bad-alpha rejection, analysis alpha inheritance/override, fresh Run
after settings, no workbook mutation, real Help callback routing/rejection,
redacted log, keyboard tab focus and 213/320/400 CSS-pixel layouts. They test
Office `sk-SK` with English browser and English Office with Slovak browser.
Screenshots and redacted summaries are in `browser/`. Final reruns after the last
diagnostic change are `production-final-{en,sk,storage}.log`.

Existing simulation selectors were updated for three tabs, explicit development
diagnostics opt-in, and the distinct development-stage `<pre>` now that Log has
its own `<pre>`. The initial batch had four harness failures; the four corrected
individual Node retries each exited 0 (`*-final.log`). A PowerShell JSON-array
bookkeeping error returned 1 while collecting those successful retries; its
summary was repaired from the actual individual JSON evidence. No product test
assertion was disabled. Stale fingerprinted dev-host assets during early rebuilds
were resolved by restarting the owned local host.

These simulations do **not** prove actual Excel Online storage partitioning,
popup/Office browser launching, workbook API support, production writer effects
or Mac behavior. No live Excel test, real screen-reader session, authenticated
tenant, genuine Office task-pane reload or owner visual acceptance is claimed.

## Owner live acceptance

NOT TESTED. No authenticated Excel or workbook session is authorized by this prompt.
Use the supplied disposable-workbook checklist for persistence/reload, alpha
inheritance/overrides, all post-hoc formatting, fresh Run/consent, safe cancellation,
uncertain writes, redacted collision warning, contextual Help in Slovak Excel,
04.05 sparse-column discovery/previews, and narrow keyboard tab navigation.

| Owner action on an authorized disposable workbook | Expected result | Result |
|---|---|---|
| Options: alpha `0.01`, seed `12345`, trace Off/On, digits `4`, Scientific, upper bound Off; Save and reload | Exact values restored with truthful persistent/session-only status; invalid alpha/seed cannot save; Restore remains draft-only | NOT TESTED |
| New ANOVA/KW, all accepted post-hoc options; explicit analysis alpha override | Global .01 inherited; override retained; numeric p-values/highlighting/number formats match effective run options | NOT TESTED |
| Change Options after results/occupied consent, return and Run, then Write | References/method retained; old results/consent retired; fresh acquisition and one-click guarded inspection/consent; no preview-stale lock | NOT TESTED |
| Safe validation error and `ANOVAresults`/`ANOVAresults1` collision | Actionable short messages; allocation to `ANOVAresults2`; only sanitized warning code in Log; no names/addresses/false Completed | NOT TESTED |
| Home, Global Settings and active ANOVA/KW Help in Slovak Excel | Actual Office browser launch, published exact-version/latest/home fallback; blocked launch has usable fallback; worksheet untouched | NOT TESTED |
| Progress, read cancellation, browser-tab return and unsafe write | Honest stages, no invented percentage; no interruption promise for synchronous/native work; uncertainty stays latched with no retry | NOT TESTED |
| Builder worksheet switch, sparse physical columns, repeated subjects; narrow keyboard tabs | `Header | VarA`, correct physical identities/model widths/subjects; read-only; sticky three-way tabs and focus retained | NOT TESTED |

No live workflow was authorized or invoked. 04.06 remains pending owner acceptance;
planning stays **38/223 accepted, 185 outstanding**. Acceptance would advance to
39/223 and 184 outstanding. 04.07 is unstarted, Gate C incomplete, Phase 04A deferred.

## Final path inventory and preservation

All changes remain unstaged and uncommitted. The 43 paths are:

```text
 M BESHStat.Excel.Common/src/Infrastructure/Diagnostics.vb
 M BESHStat.OfficeAddin/App.razor
 M BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj
 M BESHStat.OfficeAddin/Components/AnalysisPilot.razor
 M BESHStat.OfficeAddin/Components/AnalysisShell.razor
 M BESHStat.OfficeAddin/Components/ModelBuilder/ModelSpecificationEditor.razor
 M BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs
 M BESHStat.OfficeAddin/Services/AnalysisEditorRegistry.cs
 M BESHStat.OfficeAddin/Services/AnalysisOptionDraft.cs
 M BESHStat.OfficeAddin/Services/AnalysisPilotState.cs
 M BESHStat.OfficeAddin/Services/SpecificationBuilderState.cs
 M BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs
 M BESHStat.OfficeAddin/tests/AnalysisPilotTests.cs
 M BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj
 M BESHStat.OfficeAddin/tests/browser/format-input-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/specification-builder-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
 M BESHStat.OfficeAddin/wwwroot/css/app.css
 M planning/BESHStatNG_CrossPlatform_MasterPlan.md
?? BESHStat.Excel.Common.Tests/src/OfficeDiagnosticCompatibilityTests.vb
?? BESHStat.OfficeAddin/Components/DiagnosticLog.razor
?? BESHStat.OfficeAddin/Components/GlobalOptions.razor
?? BESHStat.OfficeAddin/Components/HelpAction.razor
?? BESHStat.OfficeAddin/Resources/Ui.resx
?? BESHStat.OfficeAddin/Resources/Ui.sk.resx
?? BESHStat.OfficeAddin/Services/GlobalOptionsDraft.cs
?? BESHStat.OfficeAddin/Services/OfficeBrowser.cs
?? BESHStat.OfficeAddin/Services/OfficeDiagnosticLog.cs
?? BESHStat.OfficeAddin/Services/OfficeHelp.cs
?? BESHStat.OfficeAddin/Services/OfficePreferenceSession.cs
?? BESHStat.OfficeAddin/Services/OfficePreferenceStore.cs
?? BESHStat.OfficeAddin/Services/OfficeStrings.cs
?? BESHStat.OfficeAddin/tests/GlobalOptionsComponentTests.cs
?? BESHStat.OfficeAddin/tests/OfficeDiagnosticHelpTests.cs
?? BESHStat.OfficeAddin/tests/OfficePreferenceTests.cs
?? BESHStat.OfficeAddin/tests/PreferenceLifecycleTests.cs
?? BESHStat.OfficeAddin/tests/browser/production-options-browser.mjs
?? BESHStat.OfficeAddin/tests/preferences.test.mjs
?? BESHStat.OfficeAddin/tests/workspaceHost.test.mjs
?? BESHStat.OfficeAddin/wwwroot/js/preferences.mjs
?? BESHStat.OfficeAddin/wwwroot/js/workspaceHost.mjs
?? docs/architecture/batch-04-06-production-options-help-diagnostics.md
```

Office owns the browser/UI/session/resource changes; Common changes are limited to the additive diagnostic allowlist and its tests. The Office project/test-project edits embed resources without changing target frameworks or adding packages. Existing browser tests changed only their relevant tab/diagnostics selectors. No production transport/writer, Core, Windows, installer, WasmSmoke, manifest, metadata generator or output fixture was edited.

The following baseline SHA-256 values all match the final working files (21/21); baseline/final JSON evidence is in `TestResults/04-06/protected-{before,after}.json`:

| Protected path | Baseline and final SHA-256 |
|---|---|
| `README.md` | `8B860D233D949492AC4014D2E0B5845AFA81D03446DAF1E5E15AF954E6CAECAC` |
| `planning/README.md` | `9FFB5E4090F384273A375732A249C91A7CB6021C898D81664FBD805A201DF2F2` |
| `scripts/README.md` | `DB3CB939603E8BE5189ED2A2CB8F414BFE61F080C687092A515A3CC149FFABD0` |
| `BESHStatNG_Installer/README.md` | `3FF2EA45782807F500D1A94C0269F476B3432849B0209031619A010CC2051231` |
| `BESHStat.Core/README.md` | `8C1E5DF8227621BAC3BDAD02D6FEA14950733D8866A2CF49258CAF1A769911B3` |
| `docs/architecture/README.md` | `AA693F2A048AB44323C992E0FD67F07C22806F6D5DFAA86C0050834B77CB5D52` |
| `BESHStatNG_Help_MkDocs/README.md` | `B31867AA34F729758077F859F052F0B5B5A4816766829C34F43077653CA70BC0` |
| `BESHStat.OfficeAddin/README.md` | `FFD6879AA08FD33E8EF01296AED05BCA2C5B79751AE714243558C2CDC557A5FE` |
| `BESHStat.Core.Tests/README.md` | `E1CF30D63D2C0C6061FE0250CA1FF90411F2E2FA0CEFDD4C139F7D9BAC1470EF` |
| `BESHStat.Excel.Common.Tests/README.md` | `41213DA779D5412A77FBDA011346B522413DC1B7E5642E4AC14C4A0955E00BC6` |
| `BESHStat.OfficeAddin/tests/README.md` | `446C6FFC21EA43A5579670F3DD192B4AC96DE65068D0DDD14560F2B5462550CF` |
| `BESHstatNG/README.md` | `5F776764E8B2D70A9A3798AD9025CAD2B8A70141DBFF6059CAF69C1D836B98FB` |
| `BESHStat.Excel.Common/README.md` | `38A1ED97C11E8138C2E053B01BC628F93EA0424E54FD01B02DA00B9D4D2EEB92` |
| `BESHStat.WasmSmoke/README.md` | `B5AE3C7282FEA2397E0730E64F172009B2C8017AFE44131684FB25F2A4DD8056` |
| `BESHStatNG.Test/README.md` | `945E6988B14D1F3B8DF1ED34FABDDDEDD9EF99E56EB983C6EF03EA949F418A8F` |
| `BESHstatNG/Generated/VersionAttributes.vb` | `51AE25C329B47DB3C3203FB662D456D95B8B3DBD574256998D23D49B407E2E5D` |
| `BESHstatNG/My Project/AssemblyInfo.vb` | `7938FD1DA79CCE5D4DAC7ACF5CD04B9AFD22864849709D4B4276D198A48E25B0` |
| `BESHStatNG.Test/My Project/AssemblyInfo.vb` | `04F500B6E171787B0D067D6CF4473FDB6E0E8E22BA4F1104F6954BC831B1CEA7` |
| `BESHStat.Core/Properties/AssemblyInfo.vb` | `C472E5864C9AE4CBA2082C68AF5282E14C15D06B86BB5501EE519AB35A2F3FDD` |
| `Directory.Build.props` | `B69E29BEF7EFBD0EAABE4BACBA05CAD0F58A207688C5DBC956B231B175AEF3C4` |
| `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` | `6ED831B8789426CF4A88D82776A27D1317DF64D6212E8D3EB815B8CFF2DBF1D2` |

Final branch: `codex/04-06`. HEAD remains `d9255ea903739fd094c43f1bf18923f3515ca87c`. No files are staged. Final `git diff --check` exits 0; all 21 protected hashes match. The owned local dev host and isolated Edge process were shut down after testing. The plan retains its original numbered batches/Phase 04A scope and records 04.06 as pending acceptance. Stop at this batch; no 04.07 implementation.

## Final 04.06 acceptance amendment — Force English, default ON

Implemented and fully verified by the automated gates below; final owner
acceptance pending. The incoming tree still contained
the 43 uncommitted 04.06 paths on `codex/04-06`, HEAD unchanged at
`d9255ea903739fd094c43f1bf18923f3515ca87c`. The amendment continues that branch;
no history was rewritten, and existing changes were retained. Incoming source
hashes/status and 25 protected hashes (the previous 21 plus four packed XLLs)
were recorded before edits under `TestResults/04-06/force-english/`.

### Host-only storage and coherent application

`forceEnglish` is an optional, strictly boolean Office-only field beside the
existing Common fields in the same origin-local v1 JSON record/key. Absence
means **true**, including existing saved 04.06 records. Explicit null, strings
and numbers are invalid. Unknown fields/versions and corrupt bytes remain
protected from overwrite. The 4,096-byte size bound and write/readback verification
remain. `inspectPreferenceJson` removes the host field before returning Common
data, so `PreferenceSnapshot.Normalize` receives its unchanged schema v1 DTO.
No Core/Common preference schema, numerical algorithm or parsing policy changed.

`OfficePreferenceStore` remains an `IPreferenceStore` and adds the Office-only
`IOfficePreferenceStore`/`OfficePreferenceResult` boundary for the flag. Common-only
saves preserve an existing Office flag. The one scoped `OfficePreferenceSession`
owns both the immutable Common effective snapshot and effective override; they
commit together without an await between assignments and one revision increment.
The existing Save signature remains compatible; the UI passes the draft override
explicitly. Session-only application never writes persistent storage.

The previous failure-after-persist edge is fixed: all participant retirement now
finishes **before** persistence is submitted. A failed retirement leaves the
durable record and both effective values unchanged. A persistence failure leaves
effective values unchanged too, although a preview/consent may already have been
safely retired and require a fresh Run. New operations remain disabled throughout.
Submitted native-write/uncertainty rules and consent generation are unchanged.
If verification throws after a storage write, JS attempts to restore the exact
previous bytes (or remove only the newly created namespaced key). A returned
mismatch is not overwritten because it can represent a newer external writer.
The adapter still reports failure when durable readback is unconfirmed. Browser
storage cannot guarantee rollback if the browser also rejects recovery writes;
no success or durability is claimed in that case.

Closing before persistence submission prevents Save. Once submitted, persistence
finishes coherently; an observed successful save is not reported as a failure
merely because the pane closed. Disposed owners suppress notifications, and
disposed resource consumers unsubscribe. No language-change payload, raw storage,
exception or workbook data is logged.

### Language selection, draft semantics and rendering

`OfficeStrings` retains detected host/browser culture separately from effective
interface culture. Its initialization awaits the preference owner. Force English
ON always selects English; OFF follows the accepted adapter (Office display
language, then navigator), choosing Slovak for `sk` and English fallback otherwise.
Excel/Office/browser/OS language is never changed. Both resource sets remain
embedded and all invariant numerical conventions stay unchanged.

The accessible `global-force-english` checkbox includes a label and described
help text. Toggle is draft-only. Discard restores the effective choice; Restore
defaults selects ON in the draft; Save or explicit session-only application
applies it. Effective override and interface language appear in the settings
summary with an accessible status. Tabs retain their sticky layout and keyboard
behavior. Each resource-consuming component subscribes to `OfficeStrings.Changed`
and unsubscribes on disposal, including standalone Help/Log children. Existing
Help and clipboard notifications retain resource keys so their displayed text
also changes after Save without F5. Help identity/version/URLs remain unchanged.

The localization boundary remains partial: older hard-coded Analysis navigation,
builder/input/output controls, authored method titles, development probes and
technical guidance remain English. This amendment does not translate those
surfaces, add methods, change Ribbon/manifest behavior or start 04.07.

### Amendment verification — final outcomes

All commands below were run from the repository root. Final exit codes are **0**
for every solution build, portable/full/foundation gate, focused test command,
standalone Node suite and individual simulated browser scenario. No failing or
skipped test was waived. No package/network install was needed; the installed
cache was used with process-local `RestoreSources=<repo>/packages` and
`NuGetAudit=false`. Python used the existing WinPython interpreter listed above.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1
& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe' BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:Platform=Any CPU' '/p:PythonExe=C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe' /p:BuildDate=2026-10-22 /p:PostBuildEvent= /m /v:minimal
& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe' BESHStatNG.sln /t:Build /p:Configuration=Release '/p:Platform=Any CPU' '/p:PythonExe=C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe' /p:BuildDate=2026-10-22 /p:PostBuildEvent= /m /v:minimal
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Release -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
node --test BESHStat.OfficeAddin/tests/*.test.mjs
node --test BESHStat.WasmSmoke/tests/*.test.cjs
```

The exact combined invocation and artifact-protection `finally` blocks are
retained in `TestResults/04-06/force-english/run-full-amendment.ps1`. Each
configuration performed solution Build, full gate (including legacy), then
foundation gate. Both production Office and independent WasmSmoke hosts built
in Debug/Release. Existing targets/frameworks, manifest identity and requirements
were unchanged. Builds reported zero compiler warnings/errors. The existing
nonfatal `UTA001 MatrixTestHelpers`, ambiguous Form+Title fallback and repeated
TRX overwrite advisories remain; no new warning was suppressed.

| Distinct suite | Debug passed | Release passed | Failed / skipped |
|---|---:|---:|---:|
| Core | 1,024 | 1,024 | 0 / 0 |
| Common | 739 | 739 | 0 / 0 |
| Windows legacy | 787 | 787 | 0 / 0 |
| Office managed/component | 492 | 492 | 0 / 0 |
| WasmSmoke managed | 45 | 45 | 0 / 0 |
| Office Node | 454 | 454 | 0 / 0 |
| Help Node | 71 | 71 | 0 / 0 |
| WasmSmoke Node | 377 | 377 | 0 / 0 |
| Python catalog/help/tooling | 27 + 16 + 5 | 27 + 16 + 5 | 0 / 0 |
| **Distinct total per configuration** | **4,037** | **4,037** | **0 / 0** |

The amendment adds **25 managed + 8 Node = 33 checks** to the original 4,004;
repeated solution/full/foundation/focused runs count once. Focused runs also
returned 0: initial preference/component/lifecycle filter **63/63**, expanded
Force English/component/locale/lifecycle filter **44/44**, and the complete Office
suite **492/492**. The Node preference subset is **33/33**. Focused TRX evidence
is `force-english-initial.trx`, `force-english-focused.trx`, and
`Office-amendment.trx`. Full TRXs are copied under `Debug/` and `Release/`.

Evidence root: `TestResults/04-06/force-english/`. Per-command logs and `.exit`
files include `portable`, `solution-{Debug,Release}`, `full-{Debug,Release}`,
`foundation-{Debug,Release}`, `office-node` and `wasm-node`. New/retained JS
static assets have exactly one entry in each build (`static-assets.json`).

**20/20 SIMULATED Edge scenarios passed**, individual exit codes 0 in
`browser/outcomes.json`: the existing 19 scenarios listed earlier, plus
`node BESHStat.OfficeAddin/tests/browser/production-options-browser.mjs --force-english`.
The existing `--sk` scenario now asserts forced English initially, then explicitly
saves OFF before asserting Slovak. Its former Slovak regression coverage remains.
The new scenario starts from old v1 data without the flag, tests keyboard/draft/
Discard/Defaults, live OFF/ON switching, reopen persistence, coherent session-only
fallback, unchanged Help URLs and zero unintended workbook mutation. The tests
use synthetic Office fixtures in a fresh isolated hidden Edge profile under
`force-english/edge-profile`; no authenticated browser/workbook was used. The
approved existing-certificate local dev host and isolated Edge were shut down
after testing. No certificate install or trust change occurred.

Fresh packed XLLs passed read-only MZ/PE architecture checks before the owner's
original artifacts were restored. `built-xll-architectures.json` records their
hashes and lengths: Debug x86 **1,948,672** / x64 **1,874,432** bytes; Release x86
**1,882,112** / x64 **1,807,872** bytes, machines `0x014C` / `0x8664`. The four
incoming packed XLLs and generated version file were backed up before builds and
restored in `finally`; final **25/25 protected SHA-256 matches** include all 15
README files, four version/assembly files, build props, authored metadata and
four incoming packed XLLs. Before/after manifests are `protected-before.json`
and `protected-after.json`; the earlier report's README/version hash values
remain valid. No XLL was loaded, installer run, signing routine entered or
deployment performed.

### Genuine Excel owner acceptance — NOT TESTED

Use Slovak-language Excel Online and an authorized disposable workbook:

1. With no override, or a valid old 04.06 record lacking `forceEnglish`, open the
   pane. Confirm English tabs/options, Force English checked, and prior alpha,
   seed, trace and p-value values preserved. Save other settings and reload.
2. Toggle Force English OFF without Save: language must stay English. Discard
   restores the check. Toggle OFF and Save: confirm localized tabs, options,
   Help/Log/status strings become Slovak immediately. Close/reopen: it stays OFF.
3. Restore defaults in draft: the checkbox becomes ON without changing effective
   language until Save. Save ON and reopen: English persists. With browser storage
   denied, verify failed Save keeps language and explicit session-only application
   changes it without claiming persistence.
4. Run ANOVA/KW with post-hoc tables. Verify unchanged values, p-value formatting/
   colors, numbered new-sheet naming and guarded output. Change only interface
   language: no workbook mutation; old preview/consent retires; fresh Run and
   occupied-output consent are required. Exercise keyboard tabs at narrow width.

No authenticated Excel session or genuine owner smoke has been run for this
amendment. Base functionality is owner-accepted; the final addition remains
pending. Planning stays 38/223 fully accepted and 185 outstanding until the owner
accepts this amendment. 04.07 is unstarted and Gate C remains incomplete.

### Exact amendment path inventory and final Git state

Compared with the incoming uncommitted 04.06 implementation, this amendment changes 21 paths (one new test file); the combined worktree contains 44 changed paths. All remain unstaged/uncommitted.

```text
BESHStat.OfficeAddin/App.razor
BESHStat.OfficeAddin/Components/AnalysisPilot.razor
BESHStat.OfficeAddin/Components/AnalysisShell.razor
planning/BESHStatNG_CrossPlatform_MasterPlan.md
BESHStat.OfficeAddin/Components/DiagnosticLog.razor
BESHStat.OfficeAddin/Components/GlobalOptions.razor
BESHStat.OfficeAddin/Components/HelpAction.razor
BESHStat.OfficeAddin/Resources/Ui.resx
BESHStat.OfficeAddin/Resources/Ui.sk.resx
BESHStat.OfficeAddin/Services/GlobalOptionsDraft.cs
BESHStat.OfficeAddin/Services/OfficeBrowser.cs
BESHStat.OfficeAddin/Services/OfficePreferenceSession.cs
BESHStat.OfficeAddin/Services/OfficePreferenceStore.cs
BESHStat.OfficeAddin/Services/OfficeStrings.cs
BESHStat.OfficeAddin/tests/GlobalOptionsComponentTests.cs
BESHStat.OfficeAddin/tests/OfficePreferenceTests.cs
BESHStat.OfficeAddin/tests/browser/production-options-browser.mjs
BESHStat.OfficeAddin/tests/preferences.test.mjs
BESHStat.OfficeAddin/wwwroot/js/preferences.mjs
docs/architecture/batch-04-06-production-options-help-diagnostics.md
BESHStat.OfficeAddin/tests/ForceEnglishTests.cs
```

The incoming changes outside this list were retained. In particular, the amendment changes no Core/Common source or schema, Windows source, manifests, metadata generators, numerical fixtures, framework targets, version/assembly attributes or README content. The active planning checkpoint records base functionality accepted and this final addition pending; no batch count was advanced prematurely.

Final branch: `codex/04-06`; HEAD: `d9255ea903739fd094c43f1bf18923f3515ca87c`. No commit, merge, push, sign or deploy. Stop after this amendment; 04.07 remains unstarted.
