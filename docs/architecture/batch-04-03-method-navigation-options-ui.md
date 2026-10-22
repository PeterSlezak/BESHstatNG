# Batch 04.03 — method navigation and reusable analysis options

**IMPLEMENTED / UNCOMMITTED / PENDING OWNER REVIEW AND LIVE EXCEL ACCEPTANCE.**
Only 04.03 is delivered. Checkpoint: **223 total / 35 accepted / 188 outstanding**.
04.03 is still outstanding; Gate C is incomplete. No 04.04, 04A or 08.03 work.

**Owner UI amendment:** the owner reports that the original 04.03 functionality
and genuine Excel Online smoke passed, then requested two-level dropdown navigation
before formal acceptance. This report describes the amended implementation.
Acceptance of the redesigned dropdowns remains pending; no new numbered batch.

## Git checkpoint and authorization

- Base and feature HEAD: **`6b1e97837278e9e2125adacd05bc940cc1f267a6`**, the accepted
  04.02 adapter commit on local `main`; its parent `d7888cd` contains the accepted
  04.01 foundation and single-wrap fix. Source still returns the adapter directly
  from `officeHost.mjs:create()`. No changes to that module in this batch.
- Initial status was **not clean**: only
  ` M planning/BESHStatNG_CrossPlatform_MasterPlan.md`, changing the 04.02 row
  label to COMPLETE. Codex stopped and reported the dirty-start gate. The owner
  then explicitly instructed “please create codex/04-03 branch and continue”.
  That authorized carrying this edit forward; it was preserved and reconciled.
- Branch: **`codex/04-03`**, created from the verified current `main`. Command-local
  `-c safe.directory=C:/Users/peter/source/repos/BESHstatNG` was necessary because
  the sandbox account differs from the repository owner. Branch creation required
  approved escalation for `.git` lock-file access. No global Git/ACL changes.
- Owner acceptance of 04.02 and genuine selected-range success are owner-reported,
  not fresh Codex live Excel evidence. Historical pending wording in the 04.02
  report is superseded by the active plan and README checkpoint.
- No commit, push, merge, installer, signing, deployment, extension installation,
  live Excel automation or framework-target change was performed.
- Amendment start: branch/status/log reconfirmed `codex/04-03` at that same HEAD,
  with all intended uncommitted 04.03 files and the owner plan modification intact.
  No branch switch, reset or cleaning was performed. The amendment changes 12
  paths listed below; the complete batch now comprises 21 paths.

## Descriptor-to-editor mapping

Before: category headings and a button for each of the two implemented methods.
After: **Analysis group** and dependent **Statistical method** native dropdowns,
followed by the existing selected descriptor description/schema and typed editor.

`methods.yml` → `GeneratedMethodMetadata.All` → `AnalysisNavigationMenu` supplies
all **12 primary categories / 76 documented methods**. Labels are generated, with
no runtime YAML parsing, alternate category membership or handwritten catalog.
The separate `AnalysisCatalog.All` → `AnalysisNavigationCatalog` projection
verifies Common handler registrations; `AnalysisEditorRegistry` verifies exact
schema/type/default-factory compatibility. The menu enables only the intersection
of generated metadata, verified descriptors and supported editors. Common handler
presence, editor support and Office execution/write capability remain distinct.
Help launch is deferred to 04.06; no URL probes/table or Help resolver change.

The catalog includes only `ImplementedInCommon` descriptors with a complete,
exact set of typed handlers, verified by `AnalysisHandlerRegistry.ValidateRegistrations`.
It orders by category then stable ID using ordinal comparison. Thus Nonparametric
precedes Parametric in this checkout. Lookups are exact and case-sensitive.
Missing, duplicate or incompatible registrations exclude the method. Documentation
entries alone never become executable. Catalog verification never invokes handlers.

The presentation menu sorts categories ordinally, then methods by display name
and metadata key. Generated metadata has no Ribbon-order field, so exact Ribbon
order is not claimed. No group or documented method is excluded. Unknown/null
analysis IDs remain documentation-only, with disabled choices labelled **Not yet
available**. Their option values are `unavailable:<generated Key>`, distinct from
executable stable IDs. Groups with no editor announce **Not yet available in this
Office add-in** with `role=status`. The method dropdown remains browsable so its
disabled entries can be inspected; before choosing a group it is disabled.

Duplicate metadata keys, nonempty analysis IDs, or labels within one group, and
missing required key/category/name fields cause an explicit `ArgumentException`
during projection construction. Ambiguity is not resolved with first-match logic.
Missing metadata cannot expose a descriptor. Unknown IDs, mismatched primary
category/title, incomplete handlers, unsupported schemas/types and invalid default
factories never enable an editor. Availability probing creates/disposes only a
local options draft, never invoking a handler, workbook service or Core analysis.

| Stable method ID | Validated options schema / exact Common type | Draft / component |
|---|---|---|
| `anova.oneway` | `anova.oneway.options.v1` / `OneWayAnovaAnalysisOptions` | `AnovaOptionDraft` / `AnovaOptions` |
| `nonparametric.kruskal-wallis` | `nonparametric.kruskal-wallis.options.v1` / `KruskalWallisAnalysisOptions` | `KruskalWallisOptionDraft` / `KruskalWallisOptions` |

The compile-time generic registration pairs the concrete Common options type,
draft and component. The schema/type pair must match exactly; the default factory
must also return the expected concrete type. Only descriptors retrieved through
the verified navigation catalog reach this factory. Unsupported schemas/types or
invalid default factories leave no editor and display the fixed reason:

> Options are unavailable: the method, handler registration or option schema is unsupported.

Future implemented Common descriptors appear automatically after complete handler
registration. An additional typed editor requires one schema/type registration
and its editor/draft, without modifying App or navigation dispatch switches.
There is no analysis-execution dispatch in this UI and no serialization of Type.

## UI state and lifecycle policy

The shell owns one `AnalysisNavigationState`; DI catalog/editor services are scoped
and contain no current selection or mutable draft. No static mutable UI state,
browser storage, workbook data, background operations or event subscriptions.

- Initially no group or method is selected. Changing group closes the old draft,
  clears the method and its validation errors. Same-group reselection preserves
  the current draft. Unknown/malformed group input safely clears navigation.
- Selecting a different method closes the old
  draft and calls the selected descriptor's `CreateDefaultOptions()` again.
- ANOVA → KW → ANOVA **resets to Common defaults** without field contamination.
  Selecting the already selected method preserves its draft.
- Retry initializes the existing host component; it preserves selection and all
  draft text/flags, including invalid alpha. Existing read summaries still clear.
- Method selection requires an exact supported ID in the currently selected
  group; wrong-group/stale events, unavailable and unknown choices reject safely.
  Placeholder selection clears the draft without manufacturing an error.
- Close/disposal releases the draft and rejects late mutations. Reload/reopen
  creates a fresh unselected shell. No persistence across pane sessions is claimed.
- Snapshot methods explicitly copy every concrete option field. Mutating a Common
  defaults object or returned snapshot cannot mutate another editor's state.

Defaults come from Common: alpha 0.05, five ANOVA flags false, KW Dunn true.
The alpha primitive keeps entered text independently from its last valid number.
Parsing/formatting uses invariant culture, rejects thousands/decimal commas, blank,
NaN/infinity and values outside the strict `(0,1)` interval. Invalid text remains
visible with `aria-invalid`, `aria-describedby` and an inline live error. An invalid
draft cannot produce a snapshot; it is never silently replaced with 0.05. Common
warning/numerical behavior is unchanged. `NumberStyles.Float` permits scientific
notation and surrounding whitespace.

Navigation uses labelled native selects with stable `analysis-group` and
`analysis-method` IDs, generated keys, dependent options and visible keyboard
focus. Tab/arrow/Enter operate the selects; checkbox Space still works. Unavailable
entries use disabled semantics plus text, without relying on color. Typed editors
retain fieldsets/legends and labelled Boolean controls. Styles constrain both
dropdowns to pane width and wrap schema IDs; screens scroll vertically.

## Preserved host boundaries and deferred dependencies

`App.razor` adds the shell outside readiness gating, so it works in standalone,
failed, pending and Excel-ready hosts. Its existing readiness, Retry, probes,
read-only actions, cancellation/disposal and diagnostic summaries remain intact.
The only other App change is the displayed batch number. Core CDF(0)=0.5 and
Common 2 descriptors / 4 handlers remain real probes.

The option shell has **no workbook adapter or execution service dependency**.
Rendered App event tests use an adapter that fails any workbook call and show
zero calls during edits/switches/Retry. Actual-browser simulation checks zero
`Excel.run` calls until the explicit existing read action, then `workbook.ok` and
zero mutations. Existing typed reader/cancellation and writer tests remain green.
Workbook adapter/transport/JavaScript sources, manifest identity/permissions, Help
resolver and Windows forms are unchanged. Production JavaScript still installs
no writer. Run is visibly disabled with an explanation that it awaits 04.04.

04.04 still owns input/range picking, validation preview, output destination,
execution integration, production result writing and explicit consent. This batch
adds no ranges/results/previews, overwrite prompts or statistical execution.
04.05 model/formula builders, 04.06 settings/help/progress/errors/localization,
04.07 commands/E2E, Phase 04A and all Phase 08 work remain deferred.

## Automated evidence

All final commands below exited **0**. Dependencies were already cached; validation
set `RestoreSources=C:/Users/peter/.nuget/packages` and used the existing Python
`C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe`. No package was
added or installed. VS **2026** MSBuild/vstest was selected by the existing script.

```powershell
# Focused amendment check, then separate complete runs for each configuration:
dotnet test BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj -c Debug --no-restore --filter 'FullyQualifiedName~Analysis' --logger 'trx;LogFileName=Navigation-Debug.trx' --results-directory TestResults/04-03-dropdown/Debug
dotnet test .\BESHStat.OfficeAddin\tests\OfficeAddin.Tests.csproj -c Debug --logger 'trx;LogFileName=OfficeAddin-Debug.trx' --results-directory TestResults/04-03-dropdown/Debug
dotnet test .\BESHStat.OfficeAddin\tests\OfficeAddin.Tests.csproj -c Release --logger 'trx;LogFileName=OfficeAddin-Release.trx' --results-directory TestResults/04-03-dropdown/Release
Push-Location BESHStat.OfficeAddin
npm.cmd test
Pop-Location
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-office-foundation.ps1 -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python -PythonExe $env:PythonExe
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Configuration Release -VisualStudio 2026 -Legacy -Wasm -Js -Python -PythonExe $env:PythonExe
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
```

| Distinct suite cases | Debug | Release |
|---|---:|---:|
| Core | 1,024 | 1,024 |
| Common | 727 | 727 |
| Windows legacy | 787 | 787 |
| WasmSmoke C# | 45 | 45 |
| OfficeAddin C# | **153** | **153** |
| WasmSmoke Node | 377 | 377 |
| OfficeAddin Node | 136 | 136 |
| Help resolver Node | 71 | 71 |
| Python tools / Help / discovery | 27 / 16 / 5 | 27 / 16 / 5 |
| **Total distinct cases per configuration** | **3,368** | **3,368** |

All passed, none skipped. Repeated suite runs are not additional distinct tests.
The focused amendment run passes **75 cases**: **68 catalog/draft + 7 render/event**.
This is **24 additional cases** over the original 04.03's 129 OfficeAddin cases,
or 75 cases over the pre-batch 78. Node cases are unchanged. Separate browser scenarios
and build/catalog checks are excluded from the distinct unit-case totals.

The test project now uses the SDK Razor compiler and existing ASP.NET framework
reference to compile the exact production App/components alongside linked services.
No bUnit or new test dependency. HtmlRenderer verifies actual HTML semantics;
a small test-only Blazor renderer dispatches real component event handlers and
exercises typed control bindings, selected editor visibility, invalid text, Retry,
standalone/failure/Excel host states, reopening and disposal during pending startup.
Browser keyboard/focus and CSS are tested separately below.

Amendment tests retain all prior option and host cases, replacing button events
with actual select/change events. Additional cases cover all 12 categories and
76 methods, stable ordering, seven malformed/duplicate metadata cases, nullable/
unknown IDs, missing metadata, mismatched category, wrong-group/unavailable/stale
selection, invalid groups, same-group/method retention, group disposal/reset and
real rendered unavailable-group messages. Retry also retains invalid alpha text.

`AnalysisNavigationTests` covers exact descriptor metadata/inclusion/order, seven
unknown/nonexact IDs, missing input handlers, six bad registration/schema/factory
cases, both editor types, actual defaults and custom descriptor factory defaults,
copy isolation, five valid/thirteen invalid alpha cases and decimal-comma culture,
all five flags independently, Dunn, switching and closed-draft mutation rejection.

Amendment evidence is ignored under `TestResults/04-03-dropdown/{Debug,Release}`;
original 04.03 evidence remains separately under `TestResults/04-03`. Current files:
`office-test.log`, `foundation.log`, `regression.log`, the direct configuration-named
OfficeAddin TRX, plus `Core.trx`, `Common.trx`, `Legacy.trx`, `OfficeAddin.trx` and
`OfficeExecution.trx` copied **immediately after each configuration**. The existing
regression script overwrites its unsuffixed `TestResults/Codex` filenames; only
these five current suite files are copied and counted. `outcomes.txt` records
all seven final command exits. Scripts themselves were not modified.

Existing advisories remain: **UTA001** for non-public `MatrixTestHelpers`, the
Help generator's single ambiguous Form+Title fallback, and TRX overwrite notices.
Final builds report **0 warnings / 0 errors**; no new code warnings were introduced.
Git also emits its existing CRLF-normalization notice for the generated version
file; the file's bytes and clean source status are preserved.
Version generation was allowed during builds, then its originally clean bytes
were restored in finally. Final SHA-256 of `Generated/VersionAttributes.vb`:
`0F22C5A1AD446546E773E59B495E647C58F5FF08A70750AFE7372680798D2F2D`.

Development failures, corrected before final verification:

- Five initial component cases exposed DI choosing an empty enumerable test
  constructor, producing no navigation. Making that constructor internal ensures
  production DI selects the Common-backed public constructor. All render cases pass.
- An initial log wrapper stopped on successful Python unittest stderr. The ignored
  wrapper was corrected and both complete regression gates rerun successfully.
- Initial browser driver assumptions about menu order and CDP Enter/Space were
  corrected: ordinal category order is retained, and native character events are
  now sent. No app behavior or existing assertions were weakened to pass.
- The sandbox account could not locate the owner's existing developer HTTPS
  certificate and reported EventLog access denial while logging startup failure.
  Approved execution of the same launch command succeeded. No certificate was
  generated, trusted or installed.

Amendment development checks initially exposed two test-renderer assumptions:
Razor represents an empty static option value as Boolean true, and static Run
markup lives in markup frames. Helpers now inspect those actual rendered forms;
disabled Run remains asserted in real HTML and browser tests. The initial browser
driver left a native select popup open before Tab; it now closes that popup using
Escape. No production keyboard handlers or existing assertions were removed.

Normal legacy builds produced the existing x86/x64 Excel-DNA packages; none was
installed, loaded, manually edited or signed. Read-only presence/PE checks:

```powershell
Get-ChildItem BESHstatNG/bin/Debug,BESHstatNG/bin/Release -Filter '*packed.xll' | ForEach-Object {
    $packageBytes = [IO.File]::ReadAllBytes($_.FullName)
    $peOffset = [BitConverter]::ToInt32($packageBytes, 60)
    [pscustomobject]@{ Configuration=$_.Directory.Name; Name=$_.Name; Bytes=$_.Length; Machine=('0x{0:X4}' -f [BitConverter]::ToUInt16($packageBytes, $peOffset+4)) }
}
```

| Configuration | File under `BESHstatNG/bin/<Configuration>` | Bytes | PE machine |
|---|---|---:|---|
| Debug | `BESHStatNG-AddIn-packed.xll` | 1,947,648 | x86 / 0x014C |
| Debug | `BESHStatNG-AddIn64-packed.xll` | 1,873,408 | x64 / 0x8664 |
| Release | `BESHStatNG-AddIn-packed.xll` | 1,880,064 | x86 / 0x014C |
| Release | `BESHStatNG-AddIn64-packed.xll` | 1,805,824 | x64 / 0x8664 |

## Browser evidence and screenshots

Used the existing HTTPS profile on 7141 and an approved isolated headless installed
Edge profile/CDP on 9227, without authentication or live Excel. No browser/package
installation. The two new scenarios intercept Office.js with simulated Excel or
missing Office, and exercise actual Blazor WASM controls at **360px** width:

```powershell
dotnet run --project BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj --no-build --launch-profile https
Start-Process -FilePath 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' -WindowStyle Hidden -ArgumentList @('--headless=new','--disable-gpu','--no-first-run','--remote-debugging-port=9227','--user-data-dir=C:/Users/peter/source/repos/BESHstatNG/TestResults/04-03-dropdown/edge-options','about:blank') -PassThru
node BESHStat.OfficeAddin/tests/browser/options-browser.mjs
node BESHStat.OfficeAddin/tests/browser/options-browser.mjs --standalone
# Existing 04.01/04.02 browser simulations, each exit 0:
foreach ($fixtureName in @('excel','pending','unsupported','missing','dependency','bridge-regression')) {
    node BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs "--fixture=$fixtureName"
}
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs --pending-sync
```

The revised scenarios verify native Tab/arrow/Enter selects and checkbox Space,
visible focus, all category choices, disabled unavailable methods, dependent
group resets, documentation-only Multivariate Analysis/Graphics screens, schema
screens, alpha error/recovery, flags, resetting, Retry, reload, unchanged probes,
disabled Run, no horizontal overflow and zero automatic workbook calls. The Excel
scenario then verifies the explicit read diagnostic. Existing pending-sync tests
retain the time-budget/cancellation/retry checks. Sanitized browser JSON and six
screenshots are in `TestResults/04-03-dropdown` (screenshots subdirectory). Visually
inspected ANOVA, KW and unavailable-group screens: no clipping/overlap found; vertical scrolling is
expected. No claim of a complete accessibility audit or genuine Excel acceptance.
The isolated browser and test server were stopped after testing. Genuine dropdown
owner smoke, live Office CDN startup and desktop/Mac host checks were not run by
Codex in this amendment. No automated simulation is substituted for those checks.

![ANOVA — simulated Excel at 360px](../../TestResults/04-03-dropdown/screenshots/excel-anova.png)
![Kruskal–Wallis — simulated Excel at 360px](../../TestResults/04-03-dropdown/screenshots/excel-kw.png)
![Unavailable group — simulated Excel at 360px](../../TestResults/04-03-dropdown/screenshots/excel-unavailable.png)

## Touched paths and purpose

| Path | Change |
|---|---|
| `BESHStat.OfficeAddin/App.razor` | Embed shell independently of readiness; batch label |
| `BESHStat.OfficeAddin/_Imports.razor` | Component/service namespaces |
| `BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs` | Scoped catalog/editor registration |
| `BESHStat.OfficeAddin/Services/AnalysisNavigationCatalog.cs` | Verified descriptor projection, deterministic exact lookup |
| `BESHStat.OfficeAddin/Services/AnalysisNavigationMenu.cs` | Generated groups/methods and verified editor availability |
| `BESHStat.OfficeAddin/Services/AnalysisEditorRegistry.cs` | Typed schema/type/editor registration |
| `BESHStat.OfficeAddin/Services/AnalysisNavigationState.cs` | Dependent group/method selection, rejection/reset/disposal policy |
| `BESHStat.OfficeAddin/Services/AnalysisOptionDraft.cs` | Typed drafts, alpha validation, explicit copy snapshots |
| `BESHStat.OfficeAddin/Components/AnalysisShell.razor` | Two labelled dropdowns, unavailable status and selected specification |
| `BESHStat.OfficeAddin/Components/OptionEditors/AlphaEditor.razor` | Shared accessible alpha primitive |
| `BESHStat.OfficeAddin/Components/OptionEditors/BooleanOption.razor` | Shared labelled checkbox |
| `BESHStat.OfficeAddin/Components/OptionEditors/AnovaOptions.razor` | Typed ANOVA controls |
| `BESHStat.OfficeAddin/Components/OptionEditors/KruskalWallisOptions.razor` | Typed KW controls |
| `BESHStat.OfficeAddin/wwwroot/css/app.css` | Narrow dropdown layout, focus/error styling |
| `BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj` | Compile production Razor with existing dependencies |
| `BESHStat.OfficeAddin/tests/AnalysisNavigationTests.cs` | Catalog/editor/draft tests |
| `BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs` | Real HTML, event bindings and lifecycle tests |
| `BESHStat.OfficeAddin/tests/browser/options-browser.mjs` | Isolated browser scenarios/screenshots |
| `BESHStat.OfficeAddin/README.md` | Current accepted checkpoint, navigation and owner smoke |
| `planning/BESHStatNG_CrossPlatform_MasterPlan.md` | Active 35/188 checkpoint; accepted 04.02 and pending 04.03 |
| `docs/architecture/batch-04-03-method-navigation-options-ui.md` | This architecture, evidence and review gate |

All implementation remains uncommitted. Final status contains these paths only;
no Core/Common/Windows/framework/manifest/generated metadata or installer source
changes. The initial plan modification is included in this inventory.

**Exact amendment-only inventory (12 paths):**

- `BESHStat.OfficeAddin/Components/AnalysisShell.razor`
- `BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs`
- `BESHStat.OfficeAddin/Services/AnalysisNavigationMenu.cs` (new)
- `BESHStat.OfficeAddin/Services/AnalysisNavigationState.cs`
- `BESHStat.OfficeAddin/Services/AnalysisEditorRegistry.cs`
- `BESHStat.OfficeAddin/wwwroot/css/app.css`
- `BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs`
- `BESHStat.OfficeAddin/tests/AnalysisNavigationTests.cs`
- `BESHStat.OfficeAddin/tests/browser/options-browser.mjs`
- `BESHStat.OfficeAddin/README.md`
- `planning/BESHStatNG_CrossPlatform_MasterPlan.md`
- `docs/architecture/batch-04-03-method-navigation-options-ui.md`

The plan's second active “Current Revision 1.8 planning checkpoint” also retained
34/189 and pending 04.02 wording. It is now aligned with the existing active top
checkpoint: 35/188 and pending amended 04.03. Historical counts, all Phase 04A/08
rows and total 223 remain unchanged.

Final `git status --short` (exit 0; `git diff --check` also exit 0):

```text
 M BESHStat.OfficeAddin/App.razor
 M BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs
 M BESHStat.OfficeAddin/README.md
 M BESHStat.OfficeAddin/_Imports.razor
 M BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj
 M BESHStat.OfficeAddin/wwwroot/css/app.css
 M planning/BESHStatNG_CrossPlatform_MasterPlan.md
?? BESHStat.OfficeAddin/Components/
?? BESHStat.OfficeAddin/Services/AnalysisEditorRegistry.cs
?? BESHStat.OfficeAddin/Services/AnalysisNavigationCatalog.cs
?? BESHStat.OfficeAddin/Services/AnalysisNavigationMenu.cs
?? BESHStat.OfficeAddin/Services/AnalysisNavigationState.cs
?? BESHStat.OfficeAddin/Services/AnalysisOptionDraft.cs
?? BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs
?? BESHStat.OfficeAddin/tests/AnalysisNavigationTests.cs
?? BESHStat.OfficeAddin/tests/browser/options-browser.mjs
?? docs/architecture/batch-04-03-method-navigation-options-ui.md
```

## One-page owner acceptance checklist — NOT executed by Codex

Use the existing accepted manifest and `https://localhost:7141/` server. Capture
genuine Excel ANOVA/KW screenshots separately from the simulations above.

- [ ] Standalone: `office.unavailable`, Core CDF(0) **0.5**, Common **2 / 4**;
  two labelled dropdowns, 12 documented groups, methods filtered by group and no
  long method button list. No worksheet operations available.
- [ ] Choose Parametric → One-Way ANOVA with mouse and keyboard. Verify descriptor title/category/
  description, `anova.oneway.options.v1`, alpha **0.05**, five unchecked flags.
  Edit alpha and two flags. Enter `0`, `1`, `NaN`, blank and `0,05`: entered text
  stays visible with an inline error. Correct it using a decimal point.
- [ ] Choose Nonparametric → Kruskal-Wallis Test: descriptor metadata and
  `nonparametric.kruskal-wallis.options.v1`, alpha **0.05**, Dunn checked. Edit both.
  Choose Multivariate Analysis, then Graphics: unavailable explanation, documented
  methods labelled unavailable/disabled, no invented editor. Return to Parametric
  → ANOVA: Common defaults again. Same group/method reselection preserves drafts.
- [ ] Genuine Edge Excel Online: `office.excel-ready` / `OfficeOnline` and unchanged
  probes. Repeat both screens in a narrow pane. Tab focus is visible; arrow/Enter
  operate dropdowns; labelled checkboxes work with Space; no clipping/overlap.
- [ ] Retry preserves selected group/method and draft, including invalid text.
  Changing groups removes the old method/editor/errors. Close/reopen starts with
  no group or method selected. No background reads or stale summary/events.
- [ ] Editing leaves worksheet cells, formats, formulas, sheets and selection
  unchanged. Run stays disabled with the 04.04 explanation; writer unavailable.
- [ ] Explicit 04.02 read-only diagnostic returns `workbook.ok` and correct
  dimensions on a small selection. Retry still clears that summary without reading.
- [ ] Check independent WasmSmoke **7131** separately. Record actual defects and
  screenshots. No fresh Mac/desktop Excel runtime acceptance is claimed here.

Stop at the **04.03 owner review/live acceptance gate**. No next batch begins
without explicit authorization.
