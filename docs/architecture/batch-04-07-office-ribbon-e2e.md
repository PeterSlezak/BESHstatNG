# Batch 04.07 — Office Ribbon and pane integration

Implemented / pending owner acceptance. Initial clean `main` and
HEAD: `52458098f2bd0dde0709a14cb49d3c35700d08af`, containing accepted 04.04–04.06
including Force English ON. Branch `codex/04-07`; no commits or remote operations.
Owner checkpoint: 39/223 accepted, 184 outstanding; historical pending wording is
superseded. Gate C remains incomplete; Phase 04A remains planned.

The final workspace UI amendment below supersedes the original three-tab/body
command descriptions and original test totals. Earlier verification evidence is
retained as history; the amendment preserves the working state present at its start.

Read root AGENTS, editor configuration, master plan, Office/test READMEs, 04.03–
04.06 reports and current components/services. No subordinate AGENTS were found.
Pre-edit protected hashes/backups: `TestResults/04-07/protected-before.json` and
`protected-artifacts/`. Owner README contents are not edit targets.

| Authority | Existing responsibility | 04.07 addition |
|---|---|---|
| Windows Ribbon XML | Actual IDs/captions/callbacks/category occurrences | Reproducible parity ledger; callbacks remain Windows-only |
| methods.yml / GeneratedMethodMetadata | Authored method/help/utility identities | Reconcile IDs; no metadata edit or automatic method activation |
| Common handlers + Office navigation/editor registry | Actual two supported methods | Typed command capability registry rechecks intersection |
| App/AnalysisShell + pilot/builder | One pane, existing owner/generation/safety rules | Scoped router with busy/retirement guards; no Run/Write command |
| Preferences/OfficeStrings/Help/Log | Accepted settings/language/version/help/privacy | Reuse without changing storage or numerical meaning |
| Dev manifest/index | Excel 1.1 fallback and independent sideload identity | VersionOverrides, shared runtime, one-level category menus, static disabled items |

Changed paths: a reproducible Office tooling generator and parity JSON; generated
Office mapping/manifest; minimal icon assets; command bridge, registry/coordinator,
About/pane integration, focused C#/Node/tool/browser tests; this report and active
master-plan checkpoint. No Core/Windows/WasmSmoke numerical or UI implementation.

The shared runtime is necessary to route commands into the already-open Blazor
instance without reloading drafts or abandoning native writes. A ShowTaskpane
launcher remains; function commands use one bounded in-memory intent, not browser
storage. Published Marketplace `showAsTaskpane` is ignored since 23 February 2026;
this is a sideload development design, not a Marketplace deployment claim.

References consulted: [commands](https://learn.microsoft.com/en-us/office/dev/add-ins/design/add-in-commands),
[availability](https://learn.microsoft.com/en-us/office/dev/add-ins/design/disable-add-in-commands),
[navigation](https://learn.microsoft.com/en-us/office/dev/add-ins/design/navigation-patterns),
[show/hide](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/show-hide-add-in),
[menu grammar](https://learn.microsoft.com/en-us/javascript/api/manifest/control-menu?view=word-js-preview),
[XML manifest](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/xml-manifest-overview),
[shared runtime](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/configure-your-add-in-to-use-a-shared-runtime).
The supplied `/excel/show-hide-add-in` reference is superseded by the `/develop/`
page above. Menu items support Enabled; menus cannot contain submenus.

## Inventory and manifest

The checked-out Ribbon has one tab, the Analyse parent menu, 12 category menus,
79 method button appearances and five utilities: **84 buttons**. The authored
catalog contains **76 methods**, all reconciled. There are no unknown IDs,
unmapped utilities, omitted catalog methods or ambiguous callback matches.
CorrespondenceAnalysis, CoxRegression and KaplanMeierPlot each appear twice.
The ledger retains both occurrences under one canonical method identity.

`BESHStat.OfficeAddin/tools/generate_ribbon.py` reads the actual XML and YAML,
rejects mismatches and produces the [machine-readable parity ledger](batch-04-07-ribbon-parity.json),
`GeneratedRibbonCommands.cs` and `manifest-dev.xml`. `--check` is read-only.
Every ledger row includes the Windows ID, actual parent category/caption/callback,
authored key/analysis ID, Office ID/group/function, capability and disposition.
The two release-enabled method IDs are an explicit ceiling, tested against the
real handler/editor/navigation intersection; catalog additions cannot activate a
method automatically. Windows callbacks are provenance, never Office handlers.

The custom BESHStatNG tab uses four groups and 18 top-level controls: a workspace
group with Open plus five utilities, and three Analysis groups containing four
category menus each. Each category has one flat list of method items. All 79
Windows appearances are represented: ANOVA and KW are enabled; the other 77
items are disabled and labelled “not yet available”. The six working occurrence
controls are two methods plus About, Help, Settings and Show Log. Export Chart
is statically disabled. Open is an additional ShowTaskpane control.

Stable manifest IDs use `Besh.<WindowsID>`, category IDs `Besh.Category0`–`11`,
`Besh.Workspace`, `Besh.Analysis1`–`3`, `Besh.Tab` and `Besh.Open`. Resource IDs
are unique, at most 32 characters; labels/titles at most 125 and descriptions
at most 250. Local PNG icons are valid 16/32/80 pixel assets. No external icon or
font dependency was introduced. Manifest development version is now 1.0.1.0;
the product version, source identity and HTTPS port are unchanged.

VersionOverridesV1_0 declares Workbook/DesktopFormFactor/PrimaryCommandSurface,
AddinCommands 1.1, SharedRuntime 1.1 and RibbonApi 1.1. The root ExcelApi 1.1
fallback remains. The long Runtime, FunctionFile and ShowTaskpane use the same
`Taskpane.Url`; there is no TaskpaneID, as required for shared runtime. No
`Office.ribbon.requestUpdate` or RibbonApi 1.2/1.3 feature is used. Static
Enabled=false and independent dispatch rejection protect unavailable features.

Microsoft `office-addin-manifest` validation returned **exit 0: “The manifest is
valid”**, including Valid Manifest Schema. Evidence:
`TestResults/04-07/manifest-validator.log` and `.exit`. The first validation
attempt exposed an XML QName namespace serialization defect; the generator was
corrected and the final official validation passed. Validation verifies the
schema, not genuine Excel rendering, enablement or Marketplace compatibility.

## Capability, lifecycle and navigation

| Entry | Display/Help | Effective execution | Routing |
|---|---|---|---|
| Open | Available when overrides supported | No calculation/write | Show existing pane |
| One-Way ANOVA / KW | Canonical authored titles and contextual Help | Existing registered editor/handler; ExcelApi 1.17 required | Select exact category and analysis ID |
| Other 74 canonical methods | Disabled Ribbon occurrences and full disabled pane catalog; authored Help remains separate | Unavailable | Reject even a forged dispatch |
| Specification builder | Existing pane preview controls | Read-only LM/repeated/categorical preview, no fitted method | Remains separate from runnable method registry |
| Settings / Show Log | Available utilities | Existing preference owner / bounded log | Existing Options / Diagnostics tab |
| Help | Published version-aware resolver | External request only | Accepted OfficeHelp service |
| About | Accessible native modal | Version metadata, verified host platform, development/support boundary | Same pane; Escape/Close returns focus |
| Export Chart | Disabled; separate documentation action in pane | Unsupported; no renderer/export capability | Reject; no workbook operation |

`OfficeCommandRegistry` derives method identities from GeneratedMethodMetadata
and verifies availability through AnalysisNavigationMenu's actual editor/handler
intersection. Generated Ribbon provenance records preserve every appearance.
An App-owned scoped coordinator dispatches exact, case-sensitive allowlisted
IDs. Neither manifest captions nor Windows callbacks are routing inputs. It
does not construct another pilot or call Run/Write.

The JS bridge registers seven action functions once. It holds at most one
in-memory intent, expiring after 30 seconds, collapsing cold-start clicks to the
latest serial. No URL/query intents or persistent command queue are used.
After an explicit show request succeeds, a ready .NET owner dispatches it once.
Runtime AddinCommands/SharedRuntime/Excel support is checked; method dispatch
also checks ExcelApi 1.17. Missing APIs, rejection, disposal and superseded
asynchronous opens fail closed. Every function-command event completes in
finally. A hung native show promise remains subject to Office's own command
timeout; this bridge does not promise native cancellation. The bootstrap adds
no Office readiness waiter on hosts lacking the actions API and contains
rejected readiness promises. The accepted officeHost readiness state machine
and retry waiter remain unchanged.

App installs the bridge capability snapshot before draining cold intents. Busy
reading/acquisition/calculation, builder compilation, pending output inspection,
native write or preference application rejects navigation; it is not silently
replayed later. Utilities preserve the analysis draft. Selecting the same method
preserves explicit options and valid consent. Selecting a different method
awaits existing host retirement and creates the existing editor, retiring old
preview/consent without running it. Builder preview retirement is awaited.
The host's uncertain-write latch survives editor navigation; a command cannot
clear it or trigger retry. Submitted native writes remain non-cancellable.

About uses native dialog modality/focus containment with labelled heading and
Escape/Close handling. Browser verification found and fixed focus restoration
that originally occurred before modal removal; it now happens after rendering.
The sticky three-tab keyboard navigation remains. Command outcome announcements
are polite and localized; “selected/requested” does not assert an external Help
page loaded or a native Ribbon repaint occurred.

## Help, privacy, settings and platform boundaries

Home, GlobalSettings, canonical ANOVA/KW and ExportChart page destinations use
the accepted HelpDestination/HelpLaunchTarget and actual assembly version.
`BeshHelpResolverPublished` remains true. SafeOpenUrl and the existing Office
browser bridge enforce the HTTPS allowlist and published version-first/latest
fallback. Export Chart documentation is separate from export execution. No
resolver, generated Help metadata or published MIME routing changed.

Only three Common diagnostic codes were added: `diagnostic.command.invoked`,
`diagnostic.command.completed`, `diagnostic.command.rejected` (Warning). Common
compatibility tests cover all three. Events use existing correlation/run IDs,
fixed operation/stage and bounded redacted sink. They contain no command input,
raw URL, exception message, sheet identity, range or cell data. Command completion
means accepted navigation, never statistical/output completion. Logs remain
session-local. Bootstrap failure before binding can only show a fixed safe
notice; it cannot claim a successful navigation.

No preference schema/key/save behavior changed. Force English remains ON by
default; command navigation never writes preferences. New pane utility/About
and command status strings have English and Slovak resources and refresh through
the accepted OfficeStrings event. Existing hard-coded Analysis specification,
range/output controls, development diagnostics, workspace title/build caption,
bootstrap failure notice and authored method titles remain English. Static
manifest strings are English and do not follow the pane preference dynamically.
Excel/Office language is never changed.

| Host | Intended fallback / requirement | Evidence |
|---|---|---|
| Excel Online, current supported API sets | Sideload commands + shared pane; Excel 1.17 for ANOVA/KW | Deterministic and SIMULATED Edge only; genuine NOT TESTED |
| Excel Windows desktop | Same declared requirements, root task-pane fallback | Builds/XLL checks passed; native Office commands NOT TESTED |
| Excel Mac | Host must support declared command/shared runtime sets and method APIs | NOT TESTED; schema platform listing is not a Mac test |
| Missing/older Office APIs | Existing root task-pane UI; unavailable method operations remain guarded | Missing/unsupported/readiness simulations and dispatch tests |
| Standalone browser | Utility UI and truthful Excel unavailable status | SIMULATED browser startup tests; no workbook execution |
| Marketplace deployment | Function-command showAsTaskpane cannot be promised; Open ShowTaskpane fallback | Not deployed or certified; deployment design must be revisited |

## Verification commands and final totals

All final listed gate exits are **0**, with zero failing/skipped managed/Node
tests. Commands ran on installed Visual Studio 2026/.NET 10, using the existing
local package cache (`RestoreSources=<repo>/packages`, `NuGetAudit=false`) and
installed WinPython `C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe`.
No project/global dependency or framework update occurred. The official
validator was fetched with explicit approval into isolated
`TestResults/04-07/npm-cache`; its first install emitted two transitive tooling
deprecation notices, not product build warnings.

For **each** configuration Debug and Release:

```powershell
& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe' BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:Platform=Any CPU' /p:PythonExe=C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe /p:BuildDate=2026-10-22 /p:PostBuildEvent= /m /v:minimal
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
# Repeat those commands with Release.
```

Exact orchestration with exit capture, per-configuration TRX copies, four XLL PE
checks and finally restoration is `TestResults/04-07/run-full.ps1` (exit 0).
`solution-{Debug,Release}.log`, `full-{Debug,Release}.log`,
`foundation-{Debug,Release}.log` and corresponding `.exit` files are the full
evidence. Both production WASM hosts were built in both configurations; no
publish/sign/installer/live Excel operation ran. XLL checks passed PE machine
0x014C and 0x8664 for Debug and Release; sizes and built hashes are in
`built-xll-architectures.json`. Original packed files were restored after checks.

| Distinct suite | Debug | Release |
|---|---:|---:|
| Core managed | 1,024 | 1,024 |
| Common managed | 742 | 742 |
| Windows legacy managed | 787 | 787 |
| Office managed/components | 501 | 501 |
| WasmSmoke managed | 45 | 45 |
| Office Node | 468 | 468 |
| Help Node | 71 | 71 |
| WasmSmoke Node | 377 | 377 |
| Existing Python tooling/documentation/resolver/scripts | 27 + 16 + 5 | 27 + 16 + 5 |
| New Ribbon Python | 8 | 8 |
| **Distinct checks across these suites** | **4,071** | **4,071** |

These totals are counted from TRX/runner outputs, not asserted targets. The first
full runs had 467 Office Node tests; the final readiness regression adds one.
Final foundation reruns in both configurations validate 501 Office managed and
468 Node tests after the focus/readiness fixes. Core/Common/legacy/Wasm/Help
results remain from the full gates; only the affected Office suites were repeated.
Repeated foundation/standalone tests are not added to the distinct total.
Compared with the owner baseline 4,037, additions are Common 3, Office managed 9,
Office Node 14 and Ribbon Python 8 = 34 checks.

Additional exact commands (exit 0):

```powershell
node --test BESHStat.OfficeAddin/tests/*.test.mjs
node --test BESHStat.WasmSmoke/tests/*.test.cjs
& 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe' -B BESHStat.OfficeAddin/tools/generate_ribbon.py --check
& 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe' -B -m unittest discover -s BESHStat.OfficeAddin/tools/tests -v
npm.cmd exec --offline --cache TestResults/04-07/npm-cache --package office-addin-manifest -- office-addin-manifest validate BESHStat.OfficeAddin/manifest-dev.xml
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
```

New Python tests run in both configuration evidence sets. Final Office rerun
evidence is `foundation-final-{Debug,Release}.log/.exit`. Existing advisories:
TRX overwrite notices and one ambiguous Windows Form+Title Help key falling
back to Home. Product builds reported zero compiler warnings/errors. Initial
focused-test fixture/disposal/UTF-8-read failures were corrected; no failing
golden/fixture was changed to hide a regression. A redundant dev-host launch
returned address-in-use; only the verified test process was restarted. A browser
run during rebuilding hit stale WASM fingerprints; final browser evidence uses
a restarted host and clean resource loads.

## Browser evidence and genuine Excel acceptance

The production WASM cold-load scenario `tests/browser/ribbon-browser.mjs` uses
the real Blazor/Core/Common and a simulated Office actions/workbook boundary.
It verifies the actual manifest's 12 flat menus/79 items/77 disabled methods,
cold latest-intent delivery, ANOVA/KW preselection without automatic reads/writes,
same-method draft retention, real guarded occupied-output write, fresh method
consent, repeated Run, utilities, native About modal/focus, Help, unavailable
export, no preference writes, Retry and 213px layout. Evidence is
`TestResults/04-07/simulated/ribbon.json` and `ribbon.png`, **SIMULATED**, exit 0,
zero browser exceptions. This is not an Excel Ribbon rendering test.

The prior 20 production-host Edge scenarios also run, covering Force English
Save/reload/session fallback, both locale directions/storage blocked, builder
discovery, pilot/uncertainty, range capture, formatting/long input, corrective
workflow, standalone/options, pending native sync and six readiness fixtures.
`TestResults/04-07/run-browser.ps1`, `simulated/regression-exits.json` and each
captured log provide exact commands/exits. A readiness fixture initially failed
because of the extra waiter; final reruns preserve its original assertions.

**Genuine authenticated Excel Online, Windows desktop commands and Mac: NOT
TESTED.** No authorized authenticated disposable workbook/session was supplied;
no account, sign-in/MFA or personal workbook was accessed. Status is recorded in
`TestResults/04-07/live-edge/status.json`. Schema validation and simulated Edge
must not be substituted for native command activation. Gate C is incomplete.

Owner acceptance procedure:

1. Trust the existing local development TLS certificate and run
   `dotnet run --project BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj --launch-profile https`
   after building. Check `https://localhost:7141/`. Keep WasmSmoke on 7131 with
   its independent manifest. Use an authorized disposable workbook in Edge.
2. Remove/re-sideload **OfficeAddin/manifest-dev.xml** using the same development
   identity, then reopen the pane/workbook. Command/runtime manifest changes
   may need a host restart; `HostRestartNeeded`, stale manifests or old tenants'
   sideload caches require reopening Excel/Edge and confirming the right
   profile/tenant/catalog, not changing application state or bypassing login.
3. Click the real BESHStatNG Ribbon tab. Confirm four coherent groups and all
   12 categories; unsupported method items and Export Chart must be greyed out.
   Open must show the same pane. Record real controls/state screenshots.
4. From the real Ribbon launch Parametric → One-Way ANOVA. Verify the existing
   editor, run known disposable data, write to occupied output with explicit
   Yes consent, and inspect numeric post-hoc highlighting/readback. Launch
   Nonparametric → KW; verify fresh method/consent, repeated Run and guarded
   write. Also test new-sheet numbered suffixes and long/whole-column input.
5. Settings opens existing Options: Force English ON initially even in Slovak
   Excel; save/reload alpha/seed/digits/modes/trace and language OFF/ON. A settings
   change must retire results/consent, preserve source draft, and permit fresh
   Run. Navigation alone must not change settings or workbook cells.
6. Show Log is bounded/redacted. Help Home/context/GlobalSettings/ExportChart
   uses allowlisted version-first/latest fallback; blocked opening is truthful.
   About shows actual version/platform and closes with focus return. Export
   remains unavailable and cannot write.
7. Test cold/already-open/repeated clicks, Retry/reopen, narrow keyboard tabs,
   missing API fallback and builder previews. Reject switching during an active
   operation; never interrupt/retry an uncertain submitted write. Confirm the
   uncertainty latch is retained after a later navigation attempt.
8. Save sanitized screenshots, console exceptions, native command states and
   outcomes in `TestResults/04-07/live-edge/`. Existing authorized
   `tests/browser/live-edge-workflow.mjs <label>` may supplement the Run/Write
   checks; it does not itself certify a real Ribbon click and its historical
   evidence directory is 04-04. Record 04.07 native activation separately.

Only after green gates, genuine activation/Excel acceptance (or explicit owner
disposition) and owner confirmation may 04.07 be accepted. It remains pending;
39/223 accepted, 184 outstanding. No 04A work is authorized by this batch.

## Changed paths and preservation evidence

All paths below remain unstaged/uncommitted on `codex/04-07`. HEAD remains
`52458098f2bd0dde0709a14cb49d3c35700d08af`. No remote operation occurred.
`.gitignore` adds only narrow exceptions for the new generator/tests and ledger.
The master plan changes only the active checkpoint; its 223 rows/75 Phase 04A
batches and historical material remain intact. New supplemental instructions are
in this report; no owner README was edited.

```text
 M .gitignore
 M BESHStat.Excel.Common.Tests/src/OfficeDiagnosticCompatibilityTests.vb
 M BESHStat.Excel.Common/src/Infrastructure/Diagnostics.vb
 M BESHStat.OfficeAddin/App.razor
 M BESHStat.OfficeAddin/Components/AnalysisShell.razor
 M BESHStat.OfficeAddin/Components/HelpAction.razor
 M BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs
 M BESHStat.OfficeAddin/Resources/Ui.resx
 M BESHStat.OfficeAddin/Resources/Ui.sk.resx
 M BESHStat.OfficeAddin/Services/OfficeHelp.cs
 M BESHStat.OfficeAddin/manifest-dev.xml
 M BESHStat.OfficeAddin/tests/FoundationTests.cs
 M BESHStat.OfficeAddin/wwwroot/css/app.css
 M BESHStat.OfficeAddin/wwwroot/index.html
 M planning/BESHStatNG_CrossPlatform_MasterPlan.md
?? BESHStat.OfficeAddin/Services/GeneratedRibbonCommands.cs
?? BESHStat.OfficeAddin/Services/OfficeCommandBridge.cs
?? BESHStat.OfficeAddin/Services/OfficeCommands.cs
?? BESHStat.OfficeAddin/tests/OfficeCommandTests.cs
?? BESHStat.OfficeAddin/tests/browser/ribbon-browser.mjs
?? BESHStat.OfficeAddin/tests/ribbonCommands.test.mjs
?? BESHStat.OfficeAddin/tools/generate_ribbon.py
?? BESHStat.OfficeAddin/tools/tests/test_ribbon.py
?? BESHStat.OfficeAddin/wwwroot/icons/ribbon-16.png
?? BESHStat.OfficeAddin/wwwroot/icons/ribbon-32.png
?? BESHStat.OfficeAddin/wwwroot/icons/ribbon-80.png
?? BESHStat.OfficeAddin/wwwroot/js/ribbonCommands.mjs
?? docs/architecture/batch-04-07-office-ribbon-e2e.md
?? docs/architecture/batch-04-07-ribbon-parity.json
```

Final status evidence: `TestResults/04-07/git-status-final.txt` and
`git-head-final.txt`. The machine-readable manifest summary is
`TestResults/04-07/manifest-summary.json`; the tracked parity ledger supplies
per-control provenance. Verification evidence is local/ignored, while this report,
ledger, generator and tests are source deliverables.

All **27/27** protected files compare equal, including **15/15 README files**,
four version/assembly files, root props, authored metadata, Windows Ribbon,
WasmSmoke manifest and four packed XLLs. Pre-capture, backups and final comparison
are `protected-before.json`, `protected-artifacts/` and
`protected-comparison.json`. Equal SHA-256 values below mean before = after.

| Protected path | SHA-256 (before = after) |
|---|---|
| `scripts/README.md` | `DB3CB939603E8BE5189ED2A2CB8F414BFE61F080C687092A515A3CC149FFABD0` |
| `README.md` | `8B860D233D949492AC4014D2E0B5845AFA81D03446DAF1E5E15AF954E6CAECAC` |
| `planning/README.md` | `9FFB5E4090F384273A375732A249C91A7CB6021C898D81664FBD805A201DF2F2` |
| `docs/architecture/README.md` | `AA693F2A048AB44323C992E0FD67F07C22806F6D5DFAA86C0050834B77CB5D52` |
| `BESHStat.Core/README.md` | `8C1E5DF8227621BAC3BDAD02D6FEA14950733D8866A2CF49258CAF1A769911B3` |
| `BESHStatNG_Help_MkDocs/README.md` | `B31867AA34F729758077F859F052F0B5B5A4816766829C34F43077653CA70BC0` |
| `BESHStatNG_Installer/README.md` | `3FF2EA45782807F500D1A94C0269F476B3432849B0209031619A010CC2051231` |
| `BESHStat.Excel.Common/README.md` | `38A1ED97C11E8138C2E053B01BC628F93EA0424E54FD01B02DA00B9D4D2EEB92` |
| `BESHstatNG/README.md` | `5F776764E8B2D70A9A3798AD9025CAD2B8A70141DBFF6059CAF69C1D836B98FB` |
| `BESHStat.Core.Tests/README.md` | `E1CF30D63D2C0C6061FE0250CA1FF90411F2E2FA0CEFDD4C139F7D9BAC1470EF` |
| `BESHStat.OfficeAddin/README.md` | `FFD6879AA08FD33E8EF01296AED05BCA2C5B79751AE714243558C2CDC557A5FE` |
| `BESHStat.Excel.Common.Tests/README.md` | `41213DA779D5412A77FBDA011346B522413DC1B7E5642E4AC14C4A0955E00BC6` |
| `BESHStatNG.Test/README.md` | `945E6988B14D1F3B8DF1ED34FABDDDEDD9EF99E56EB983C6EF03EA949F418A8F` |
| `BESHStat.WasmSmoke/README.md` | `B5AE3C7282FEA2397E0730E64F172009B2C8017AFE44131684FB25F2A4DD8056` |
| `BESHStat.OfficeAddin/tests/README.md` | `446C6FFC21EA43A5579670F3DD192B4AC96DE65068D0DDD14560F2B5462550CF` |
| `BESHstatNG/Generated/VersionAttributes.vb` | `51AE25C329B47DB3C3203FB662D456D95B8B3DBD574256998D23D49B407E2E5D` |
| `BESHstatNG/My Project/AssemblyInfo.vb` | `7938FD1DA79CCE5D4DAC7ACF5CD04B9AFD22864849709D4B4276D198A48E25B0` |
| `BESHStatNG.Test/My Project/AssemblyInfo.vb` | `04F500B6E171787B0D067D6CF4473FDB6E0E8E22BA4F1104F6954BC831B1CEA7` |
| `BESHStat.Core/Properties/AssemblyInfo.vb` | `C472E5864C9AE4CBA2082C68AF5282E14C15D06B86BB5501EE519AB35A2F3FDD` |
| `Directory.Build.props` | `B69E29BEF7EFBD0EAABE4BACBA05CAD0F58A207688C5DBC956B231B175AEF3C4` |
| `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` | `6ED831B8789426CF4A88D82776A27D1317DF64D6212E8D3EB815B8CFF2DBF1D2` |
| `BESHstatNG/Resources/RibbonXML.xml` | `162370CB5D317F58DAC392E8B62BEDD4A6817E138C3763FCF5A39E80D32021E3` |
| `BESHStat.WasmSmoke/office-manifest.xml` | `5B97404E848E260CF7C9F9123DA41552DDE7AC3F4CFC5D636C0515926A553DF8` |
| `BESHstatNG/bin/Debug/BESHStatNG-AddIn-packed.xll` | `4ED97749E01D6033496881EDB610C9196F1DF2E911BAC4D8D1880C659CF02DAE` |
| `BESHstatNG/bin/Debug/BESHStatNG-AddIn64-packed.xll` | `E22B4180E51F7C97D1E8A61E9A418F02D9B92ECB28AC0E9D28AB222B422E3426` |
| `BESHstatNG/bin/Release/BESHStatNG-AddIn-packed.xll` | `CE9AB314CFBA8D1D84F71560B68A797294FA2326BD8F5A2F00FDB0722A383C5D` |
| `BESHstatNG/bin/Release/BESHStatNG-AddIn64-packed.xll` | `CE9EC51488207135E56304B11DA36145CA9D3A2AA2163378D2907B3D2DFE3A1B` |

Full solution builds temporarily regenerated VersionAttributes and packed XLLs;
finally blocks restored the exact owner bytes after architecture verification.
No installed/signed artifacts, fixtures, numerical implementations, target
frameworks, permissions, packages, resolver/MIME files or WasmSmoke source changed.
Remaining acceptance risk is native Office command/shared-runtime lifecycle and
rendering on real Excel hosts; no simulation or schema result closes that gate.

## Final workspace UI amendment — 25 October 2026

Continued on `codex/04-07`, HEAD
`52458098f2bd0dde0709a14cb49d3c35700d08af`, preserving the existing uncommitted
04.07 implementation. Initial/final worktree snapshots and all amendment evidence
are in `TestResults/04-07/ui-amendment/`. The pre-existing Windows generated
BuildDate was already **2026-10-25**, with CRLF bytes; this amendment preserves
that exact file instead of restoring the earlier report's 2026-10-22 baseline.
The current packed XLLs were likewise backed up before any build and restored.
No README was an edit target; all 15 current owner README contents are preserved.

### Final user-facing behavior

- The body Workspace commands section is removed, including redundant Analysis,
  Options, Log, About, Help home and disabled Export buttons. Its internal typed
  command services remain available to the Ribbon and existing navigation.
- The sticky header has a compact wrapping menu beside/after development-build
  text: **About BESHStatNG** (a keyboard-accessible button styled as a text menu
  action) and a permanent **Help home** anchor. About uses the existing guarded
  coordinator and modal. A header invocation returns focus to the invoking menu
  action on Close/Escape; a Ribbon invocation retains active-tab focus return.
- About identifies BESHStatNG / Office workspace, current development batch and
  actual assembly version, verified host platform, **Apache License 2.0**, and a
  Documentation link through OfficeHelp. No version/date attribute was invented
  or changed. The root LICENSE was checked for Apache License 2.0.
- Tabs are **Analysis | Options | Export Chart | Diagnostics / Log**. Export is
  a selectable normal panel, explaining that export is not implemented and
  containing a permanent Chart export Help link. It performs no fit, read, chart
  rendering or workbook mutation. The manifest's Export Chart command remains
  disabled and its dispatcher still rejects execution.
- Four-way Left/Right wrapping, Home/End, roving tabindex and linked tab/panel
  semantics are maintained. Existing panels remain mounted, retaining scroll,
  drafts and safety state. At widths up to 480px tabs use two columns so enlarged
  text does not fragment into narrow vertical strings. The existing 320/360/400px
  checks at 100%/200% text size pass, including focused-field visibility below
  the sticky header.
- `HelpAction` now supports an anchor presentation with a SafeOpenUrl-validated,
  version-aware href; ordinary activation prevents native navigation and uses
  the accepted Office browser-launch bridge. Existing button presentation stays
  supported. New anchors avoid a redundant Home fallback after successful
  requests, retaining the existing actionable fallback for blocked/fallback
  outcomes. ExportChart uses the existing authored page identity; no catalog or
  route changes were needed. Unknown page keys follow the existing Home policy.
- New header/About/export text uses English and Slovak resources. Force English
  remains ON by default and effective Save-driven switching also changes the
  new text without a reload. Numerical grammar/results and Help identities/URLs
  are invariant. Remaining older English-only analysis/development controls stay
  within the documented partial-localization boundary.

### Exact amendment source paths

The following 17 paths changed relative to the amendment's starting worktree;
other original 04.07 changes remain present. Generated manifest/ledger/mapping,
command registries, numerical services and preference schema did not change.

```text
BESHStat.OfficeAddin/App.razor
BESHStat.OfficeAddin/Components/HelpAction.razor
BESHStat.OfficeAddin/Resources/Ui.resx
BESHStat.OfficeAddin/Resources/Ui.sk.resx
BESHStat.OfficeAddin/wwwroot/css/app.css
BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs
BESHStat.OfficeAddin/tests/GlobalOptionsComponentTests.cs
BESHStat.OfficeAddin/tests/OfficeCommandTests.cs
BESHStat.OfficeAddin/tests/browser/options-browser.mjs
BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs
BESHStat.OfficeAddin/tests/browser/production-options-browser.mjs
BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs
BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs
BESHStat.OfficeAddin/tests/browser/ribbon-browser.mjs
BESHStat.OfficeAddin/tests/browser/specification-builder-browser.mjs
BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
docs/architecture/batch-04-07-office-ribbon-e2e.md
```

Tests extend real component callbacks for all new actions, removed control IDs,
placeholder selection, blocked Help and immutable no-write behavior, four-tab
keyboard navigation and persistence of existing overwrite consent. Four new
managed cases cover forced English on Slovak/English hosts, automatic Slovak and
unsupported-locale English fallback. Existing Force English Save tests now also
check the new live tab/header labels. Browser fixtures locate Retry by its stable
ID rather than its former first-button position; native keyboard expectations
are retained. The extended Ribbon browser scenario captures the real modal and
new header/export page while using only simulated Office APIs.

### Amendment validation and evidence

The full installed Windows/VS 2026 gates were rerun, without installs, signing,
publishing or live Excel. Exact orchestration is
`TestResults/04-07/ui-amendment/run-full.ps1`; it runs the same solution/full/
foundation Debug and Release commands shown above, using the local package cache,
installed WinPython and `/p:BuildDate=2026-10-25` to match the current pre-existing
build metadata. Packed XLL PE checks run before restoration, and finally blocks
restore protected bytes. Per-command exits/logs and per-configuration TRX copies
are in this amendment directory, separate from original evidence.

```powershell
# Each Debug command is also run with Release.
& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe' BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:Platform=Any CPU' /p:PythonExe=C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe /p:BuildDate=2026-10-25 /p:PostBuildEvent= /m /v:minimal
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration Debug -VisualStudio 2026 -Legacy -Wasm -Js -Python
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
& 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe' -B -m unittest discover -s BESHStat.OfficeAddin/tools/tests -v
& 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe' -B BESHStat.OfficeAddin/tools/generate_ribbon.py --check
node BESHStat.OfficeAddin/tests/browser/ribbon-browser.mjs
powershell -NoProfile -ExecutionPolicy Bypass -File TestResults/04-07/ui-amendment/run-browser.ps1
```

Final solution, full regression and Office foundation commands return **exit 0
in both Debug and Release**. Both production WASM hosts build, all four packed
x86/x64 PE checks pass, and `git diff --check` returns 0. Standalone Office/Wasm
Node commands also return 0. The following are final observed runner counts:

| Distinct suite | Debug passed | Release passed |
|---|---:|---:|
| Core managed | 1,024 | 1,024 |
| Common managed | 742 | 742 |
| Windows legacy managed | 787 | 787 |
| Office managed/components | 505 | 505 |
| WasmSmoke managed | 45 | 45 |
| Office Node | 468 | 468 |
| Help Node | 71 | 71 |
| WasmSmoke Node | 377 | 377 |
| Existing Python tooling/documentation/scripts | 27 + 16 + 5 | 27 + 16 + 5 |
| Ribbon Python | 8 | 8 |
| **Distinct checks** | **4,075** | **4,075** |

All final suites have zero failures/skips. Four managed cases are new relative
to the earlier 4,071 total; repeated foundation/standalone checks are not added.
`test-totals.json` and `{Debug,Release}/*.trx` provide managed evidence. Exact
exit files are `solution-*.exit`, `full-*.exit`, `foundation-*.exit`,
`ribbon-python-*.exit`, `office-node-explicit.exit` and `wasm-node-explicit.exit`.
`run-full.exit` is 0. New focused callbacks/keyboard coverage is 14/14 passing.

**21/21 SIMULATED Edge scenarios pass**, each final exit 0: the prior 20-case
regression suite plus the extended Ribbon/header/export scenario. Command/exits
are `run-browser.ps1`, `simulated/regression-exits.json`, `browser-regressions.exit`
and `ribbon-browser.exit`. The new scenario reports zero browser exceptions in
`simulated/ribbon.json`; screenshots are `export-header-360.png`, `about-360.png`
and `ribbon.png` (213px), all in `simulated/`. The retained formatting simulation
also passes all six 320/360/400px × 100%/200% text cases. Existing scripts' older
screenshot directories are unchanged; their current outputs are identified by
these captured final logs. No simulated result is described as native Office.

Additional explicit commands (exit 0):

```powershell
node --test BESHStat.OfficeAddin/tests/*.test.mjs
node --test BESHStat.WasmSmoke/tests/*.test.cjs
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
```


The manifest was not affected: its SHA-256 matches the captured amendment-start
bytes, and generated `--check`, inventory tests and manifest managed tests pass.
The previous official validator's exit-0 schema result therefore applies to the
same bytes; a new network-based official validation was not needed or claimed.
Help resolver/JS and Python documentation/MIME checks ran as part of both full
verification configurations; no resolver/published MIME file changed.

Initial failures and their resolutions are retained honestly: four new focused
cases used a render-tree assertion that omitted static Razor markup; the test
now checks the actual static and dynamic frames, with full DOM content also
verified in Edge. An existing builder assertion expected three tabs and was
updated to the requested four. The first full gate exit 1 / 504 passed + 1 failed
is archived under `initial-attempt/`. Initial browser runs exposed the large-text
layout failure (fixed in CSS without weakening checks) and positional Retry
selectors (replaced with its ID). Final gate and simulation results are recorded
separately. Existing advisories are TRX overwrite notices and the one ambiguous
Windows Form+Title Help key fallback; Git also reports the pre-existing generated
file's CRLF/LF advisory. Those bytes were preserved rather than cleaned up.

Genuine Excel Online, Windows desktop Excel and Mac were **NOT TESTED** in this
amendment. Only an isolated, unauthenticated Edge profile with simulated Office
was used. Existing owner/native acceptance remains separate; no authenticated
workbook was accessed. No deployment, commit, merge or Phase 04A work occurred.

### Remaining owner checks and intentional deferrals

1. Reload the production pane at `https://localhost:7141/` using the existing
   04.07 sideload manifest. No re-sideload is required solely for this UI change;
   if original 04.07 commands were not yet sideloaded, follow the prior native
   manifest acceptance procedure. Confirm persistent header About/Help, no body
   command duplicates, and the four tabs at narrow width/enlarged text.
2. Open About from both header and real Ribbon: verify actual version/platform,
   Apache license, documentation opening, Escape/Close focus return. Open Help
   home and Export Chart Help, including blocked-opening behavior and version
   resolver fallback, in genuine Slovak-language Excel.
3. Toggle Force English OFF/ON through Save and confirm the new header/tab/page
   text changes and persists. Verify Options save/restore, redacted Log, ANOVA/KW
   run/write, fresh overwrite consent after effective settings changes and native
   Ribbon destinations. Merely opening the export placeholder must not read or
   change workbook/chart content or retire valid consent.
4. Retain uncertain-write protections; do not interrupt/retry a submitted write.
   Complete the earlier full native 04.07 checklist on an authorized disposable
   workbook before acceptance. Native Excel Online/Desktop/Mac evidence is still
   absent. Chart rendering/export, unsupported analyses and broader translation
   remain intentionally deferred. 04.07/Gate C remain pending; no next batch began.

### Amendment preservation and final Git handoff

All **27/27** protected hashes match the amendment-start capture; this includes
**15/15 README files**, four assembly/version files, root props, authored methods,
Windows Ribbon, WasmSmoke manifest and four packed XLLs. Original packed files
were restored after architecture checks. The current development manifest is
also byte-identical, SHA-256 `486033CD65FDD5AD479BB9FE1CEE5839227BDFF2788861C06690A482B6E853F5`.
`protected-before.json`, `protected-artifacts/` and `protected-comparison.json`
contain the independent amendment capture/backups/comparison. Equal values mean
before = after:

| Protected path | Amendment SHA-256 (before = after) |
|---|---|
| `scripts/README.md` | `DB3CB939603E8BE5189ED2A2CB8F414BFE61F080C687092A515A3CC149FFABD0` |
| `README.md` | `8B860D233D949492AC4014D2E0B5845AFA81D03446DAF1E5E15AF954E6CAECAC` |
| `planning/README.md` | `9FFB5E4090F384273A375732A249C91A7CB6021C898D81664FBD805A201DF2F2` |
| `docs/architecture/README.md` | `AA693F2A048AB44323C992E0FD67F07C22806F6D5DFAA86C0050834B77CB5D52` |
| `BESHStat.Core/README.md` | `8C1E5DF8227621BAC3BDAD02D6FEA14950733D8866A2CF49258CAF1A769911B3` |
| `BESHStatNG_Help_MkDocs/README.md` | `B31867AA34F729758077F859F052F0B5B5A4816766829C34F43077653CA70BC0` |
| `BESHStatNG_Installer/README.md` | `3FF2EA45782807F500D1A94C0269F476B3432849B0209031619A010CC2051231` |
| `BESHStat.Excel.Common/README.md` | `38A1ED97C11E8138C2E053B01BC628F93EA0424E54FD01B02DA00B9D4D2EEB92` |
| `BESHstatNG/README.md` | `5F776764E8B2D70A9A3798AD9025CAD2B8A70141DBFF6059CAF69C1D836B98FB` |
| `BESHStat.Core.Tests/README.md` | `E1CF30D63D2C0C6061FE0250CA1FF90411F2E2FA0CEFDD4C139F7D9BAC1470EF` |
| `BESHStat.OfficeAddin/README.md` | `FFD6879AA08FD33E8EF01296AED05BCA2C5B79751AE714243558C2CDC557A5FE` |
| `BESHStat.Excel.Common.Tests/README.md` | `41213DA779D5412A77FBDA011346B522413DC1B7E5642E4AC14C4A0955E00BC6` |
| `BESHStatNG.Test/README.md` | `945E6988B14D1F3B8DF1ED34FABDDDEDD9EF99E56EB983C6EF03EA949F418A8F` |
| `BESHStat.WasmSmoke/README.md` | `B5AE3C7282FEA2397E0730E64F172009B2C8017AFE44131684FB25F2A4DD8056` |
| `BESHStat.OfficeAddin/tests/README.md` | `446C6FFC21EA43A5579670F3DD192B4AC96DE65068D0DDD14560F2B5462550CF` |
| `BESHstatNG/Generated/VersionAttributes.vb` | `B59D2BE62609CEB0A66A96B45C8F6AFD3F69661D69EF4EFC4486D33BE85DBBCD` |
| `BESHstatNG/My Project/AssemblyInfo.vb` | `7938FD1DA79CCE5D4DAC7ACF5CD04B9AFD22864849709D4B4276D198A48E25B0` |
| `BESHStatNG.Test/My Project/AssemblyInfo.vb` | `04F500B6E171787B0D067D6CF4473FDB6E0E8E22BA4F1104F6954BC831B1CEA7` |
| `BESHStat.Core/Properties/AssemblyInfo.vb` | `C472E5864C9AE4CBA2082C68AF5282E14C15D06B86BB5501EE519AB35A2F3FDD` |
| `Directory.Build.props` | `B69E29BEF7EFBD0EAABE4BACBA05CAD0F58A207688C5DBC956B231B175AEF3C4` |
| `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` | `6ED831B8789426CF4A88D82776A27D1317DF64D6212E8D3EB815B8CFF2DBF1D2` |
| `BESHstatNG/Resources/RibbonXML.xml` | `162370CB5D317F58DAC392E8B62BEDD4A6817E138C3763FCF5A39E80D32021E3` |
| `BESHStat.WasmSmoke/office-manifest.xml` | `5B97404E848E260CF7C9F9123DA41552DDE7AC3F4CFC5D636C0515926A553DF8` |
| `BESHstatNG/bin/Debug/BESHStatNG-AddIn-packed.xll` | `95897EEBEB3EAA7346097E04851AAC773DB2AA10A7F9445BD7C2B61BBFBA488B` |
| `BESHstatNG/bin/Debug/BESHStatNG-AddIn64-packed.xll` | `6076D66AC1101EF954D34B019BFD948581E220905C4D65E1B9D37427D3F4643E` |
| `BESHstatNG/bin/Release/BESHStatNG-AddIn-packed.xll` | `CE9AB314CFBA8D1D84F71560B68A797294FA2326BD8F5A2F00FDB0722A383C5D` |
| `BESHstatNG/bin/Release/BESHStatNG-AddIn64-packed.xll` | `CE9EC51488207135E56304B11DA36145CA9D3A2AA2163378D2907B3D2DFE3A1B` |

Final branch `codex/04-07`, unchanged HEAD
`52458098f2bd0dde0709a14cb49d3c35700d08af`. The index is empty of changes;
all original 04.07 and amendment changes remain unstaged/uncommitted. The
pre-existing VersionAttributes.vb difference remains deliberately present.
No README update or other existing unstaged work was discarded. Full final
`git status --short --untracked-files=all` (also `git-status-final.txt`):

```text
 M .gitignore
 M BESHStat.Excel.Common.Tests/src/OfficeDiagnosticCompatibilityTests.vb
 M BESHStat.Excel.Common/src/Infrastructure/Diagnostics.vb
 M BESHStat.OfficeAddin/App.razor
 M BESHStat.OfficeAddin/Components/AnalysisShell.razor
 M BESHStat.OfficeAddin/Components/HelpAction.razor
 M BESHStat.OfficeAddin/Infrastructure/FoundationServices.cs
 M BESHStat.OfficeAddin/Resources/Ui.resx
 M BESHStat.OfficeAddin/Resources/Ui.sk.resx
 M BESHStat.OfficeAddin/Services/OfficeHelp.cs
 M BESHStat.OfficeAddin/manifest-dev.xml
 M BESHStat.OfficeAddin/tests/AnalysisComponentTests.cs
 M BESHStat.OfficeAddin/tests/FoundationTests.cs
 M BESHStat.OfficeAddin/tests/GlobalOptionsComponentTests.cs
 M BESHStat.OfficeAddin/tests/browser/options-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/pilot-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/production-options-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/range-capture-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/specification-builder-browser.mjs
 M BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
 M BESHStat.OfficeAddin/wwwroot/css/app.css
 M BESHStat.OfficeAddin/wwwroot/index.html
 M BESHstatNG/Generated/VersionAttributes.vb
 M planning/BESHStatNG_CrossPlatform_MasterPlan.md
?? BESHStat.OfficeAddin/Services/GeneratedRibbonCommands.cs
?? BESHStat.OfficeAddin/Services/OfficeCommandBridge.cs
?? BESHStat.OfficeAddin/Services/OfficeCommands.cs
?? BESHStat.OfficeAddin/tests/OfficeCommandTests.cs
?? BESHStat.OfficeAddin/tests/browser/ribbon-browser.mjs
?? BESHStat.OfficeAddin/tests/ribbonCommands.test.mjs
?? BESHStat.OfficeAddin/tools/generate_ribbon.py
?? BESHStat.OfficeAddin/tools/tests/test_ribbon.py
?? BESHStat.OfficeAddin/wwwroot/icons/ribbon-16.png
?? BESHStat.OfficeAddin/wwwroot/icons/ribbon-32.png
?? BESHStat.OfficeAddin/wwwroot/icons/ribbon-80.png
?? BESHStat.OfficeAddin/wwwroot/js/ribbonCommands.mjs
?? docs/architecture/batch-04-07-office-ribbon-e2e.md
?? docs/architecture/batch-04-07-ribbon-parity.json
```
