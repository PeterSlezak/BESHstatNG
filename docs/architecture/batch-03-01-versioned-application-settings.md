# Batch 03.01 — versioned application preferences

Status: IN PROGRESS / PENDING OWNER REVIEW. Branch `codex/03-01`, initial HEAD
`ce2aaee0f50ffae56ea75b03fdf3bedf57248deb`; initial working tree clean. Both
historical stashes retained. Owner confirms Gate B accepted with E1–E7 and merged;
stale pending status in the freeze, inventory and active plan will be reconciled.
The checkpoint remains 222 total / 27 accepted / 195 outstanding; all 74 Phase
04A rows remain after 04.07. This is additive to the approved output envelope.

## Source inspection and planned changes (recorded before implementation)

Before:
```text
AutoOpen -> XLL directory/BESHStatNG.settings.xml -> Core legacy store.Load
         -> AppGlobals.ApplySettings -> mutable gSettings + CoreServices defaults
Global Settings OK -> mutate active gSettings -> ApplySettings -> legacy Save
UibyID ANOVA/KW -> AppGlobals.DefaultAlpha -> existing Common options/output
Home.razor fields -> explicit ANOVA/KW alpha + semantic planner (lost on reload)
```

The legacy XML constructor supplies version 3, alpha .05, seed Int32.MinValue,
trace True, digits 8, enum 0, upper bound True. EnsureDefaults upgrades versions
below 3, normalizes seed 0 to the sentinel and invalid bounds to defaults; it
does not catch NaN alpha. AnalysisDefaultsOptions does catch NaN/infinities.
The new boundary will reuse that finite-alpha policy and Core display policy.
No prior Common/browser preferences schema exists, so no invented v0 migration.
Source-backed legacy XML versions 1/2/3 will use the existing XmlSerializer.

Planned after:
```text
Windows startup -> legacy preference adapter (future-version guard)
                -> existing XML store -> shared normalized immutable snapshot
                -> AppGlobals -> CoreServices + legacy display accessors
Settings OK -> detached candidate -> shared normalization -> XML Save
            -> Apply only on successful save
Task-pane first render -> JS namespaced storage -> typed DTO -> Common normalize
                       -> host-owned snapshot + existing fields
Explicit Save/apply -> Common normalize -> JS safe write -> session snapshot
ANOVA/KW run -> explicit normalized local options + same alpha in highlighting
```

Concrete planned edit list: new Common `src/Infrastructure/Settings.vb` and
Common tests; new Windows `src/AppInfrastructure/LegacyPreferenceStore.vb`,
AppGlobals, AutoOpen, Settings OK handler and explicit project compile entry;
new Windows adapter tests and compile entry; Home.razor, index.html, a separate
preferences JS adapter and Node tests; this report; active master-plan status;
status-only corrections to the Gate B freeze/inventory. Core XML implementation
will be reused, not replaced. No designer, output wire, golden fixture, math,
framework, dependency or production add-in shell changes are planned.

## Evidence and acceptance

No live Excel, browser navigation, deployment, signing, installer or Mac runtime
execution was performed. Automated validation results are recorded below.

## Implemented API and lifetime inventory

All new Common declarations are additive, in `ExcelCommon`:

| Type | Contract |
|---|---|
| `PreferenceData`, `PValuePreferenceData` | Mutable transport with nullable optional scalar fields. Missing differs from zero/False. SchemaVersion defaults to 1 in .NET; the browser requires an explicit version. Not an arbitrary workbook-data container. |
| `PreferenceSnapshot` | Immutable scalar properties; schema constant/current version 1. `Defaults`, `Normalize`, `ToData`, `CreateDisplaySettings`, `CreateAnalysisDefaults`, `CreateAnovaOptions`, `CreateKruskalWallisOptions`, explicit `ApplyToCoreDefaults`. Every mutable export is a fresh object. |
| `PreferenceResult` | Immutable snapshot/status/persistence and copied read-only stable-code list; `CanUseStoredData` distinguishes usable load/default/normalization/migration/save outcomes from failures. A fallback snapshot on failure does not claim successful migration. |
| `PreferenceStatus` | Loaded=0, Defaulted=1, Normalized=2, Migrated=3, Saved=4, UnsupportedVersion=5, InvalidData=6, StorageUnavailable=7, Failed=8. |
| `PreferencePersistence` | Unknown=0, Persisted=1, SessionOnly=2, Unavailable=3. Persisted refers to this effective snapshot, not just the existence of storage. |
| `IPreferenceStore` | `LoadAsync()`, `SaveAsync(PreferenceSnapshot)` return `Task(Of PreferenceResult)`. No implicit application on read/save. Async accommodates JS interop; no fictitious cancellation guarantee for a single storage operation. |
| `InMemoryPreferenceStore` | Host-owned deterministic session store. Empty load defaults, save/replay retains immutable values; never claims durable persistence. |

Windows `LegacyPreferenceStore` implements that interface and exposes synchronous
`Load`/`Save` for existing Windows initialization, plus `FromLegacy`/`ToLegacy`.
It delegates writes and historical recovery to `BeshStatNgSettingsStore`.
`AppGlobals.LoadPreferences`, `SavePreferences`, `PreferenceState` form a small
facade. Existing fields/accessors/ApplySettings remain available; a new Apply
copies effective settings and explicitly refreshes CoreServices alpha/seed/trace.
`GetCurrentSettings` still returns the legacy mutable object for old callers.
This batch does not make that old API immutable or intercept arbitrary external
field mutation; normal startup and Settings OK use the new boundary.

Office `BrowserPreferenceStore` implements the same interface, translates a
small private JS response, then calls Common normalization. `preferences.js`
alone accesses browser storage. Home owns its applied snapshot for its component
lifetime. It preserves unexposed seed/trace values during Save and supplies
explicit alpha to new ANOVA/KW options and the same semantic planner call.
Unmodified constructors of frozen analysis options still default to .05;
hosts explicitly use snapshot factories rather than changing frozen APIs.

## Independent versions and migration policy

| Boundary | Version / read behavior / write behavior |
|---|---|
| Common preference DTO/snapshot | **1**, new in 03.01. Only v1 is known. Version 0, negative or future versions explicitly return UnsupportedVersion plus session defaults; no migration is invented. Missing optional fields default individually. |
| Windows legacy XML | **3**, unchanged root `BeshStatNgSettings`, attribute `version`, file `BESHStatNG.settings.xml` beside the XLL. Existing XmlSerializer constructor/default behavior and EnsureDefaults read v1/v2/v3; explicit Save emits v3. Source-backed tests build representative old XML, not a claim to possess historical released binary fixtures. Missing version uses the serializer's constructor v3 default. |
| Office prototype preferences storage | **storageVersion 1**, namespaced key `eu.beshstat.wasmsmoke.preferences.v1`; envelope `{storageVersion, preferences}` nests independent Common schemaVersion 1. No prior browser format exists. Unknown old/new storage or snapshot versions are preserved and refused, including on Save. |
| Analysis/result/output protocols | `AnalysisResultDocument.SchemaVersion=1`, existing Common output/outcome semantics and prototype table/composite output wire v1 remain independent and unchanged. No settings fields enter these wires. Equal numbers do not identify a shared protocol. |

Legacy migration is source-backed by `BeshStatNgSettings.New`, `EnsureDefaults`
and the actual XmlSerializer. V1/v2 upgrade in memory, retain valid alpha/display
fields and do not rewrite on read. A subsequent explicit save emits v3; repeat
load/save is idempotent. XML constructors supply omitted nested/optional fields;
explicit Nothing nested objects default through the shared boundary.

Normalization uses `AnalysisDefaultsOptions` and Core
`PValuePresentationSettings.EnsureDefaults`, not a parallel numeric algorithm:

- Alpha must be finite and strictly in (0,1); invalid values become .05, including
  NaN and both infinities. Valid finite extreme endpoints survive unchanged.
- No-explicit-seed is Int32.MinValue; historical zero maps to that sentinel.
  Other signed Int32 seeds remain exact. No seed is generated while loading.
- Decimal places use Core's published 2–16 bounds/default 8. Unknown Core enum
  values default to LessThanThreshold; enum meanings 0/1/2 remain unchanged.
- Missing trace and upper-bound fields default True. Explicit False survives.
  Missing/invalid individual fields do not erase valid adjacent values.
- New normalization reports stable missing/defaulted codes and is idempotent.
  Display changes affect number-format/significance intent only, never stored
  numeric p-values. Explicit local analysis overrides do not save global settings.

Core's legacy EnsureDefaults still allows NaN alpha; the adapter fixes its
effective value using the existing finite Core runtime policy. A NaN XML file is
not silently rewritten on read, and reports Normalized / SessionOnly until Save.
The Core file/store and all its public declarations remain byte-identical to
the starting checkout. There is no new XML file, target or installation migration.

## Read/write and failure policy

| Host/situation | Outcome and preservation |
|---|---|
| Windows missing file | Existing store creates historical defaults at the existing path. Defaulted/Persisted only after verifying equivalent v3 XML; inaccessible path reports StorageUnavailable/Unavailable. |
| Windows v1/v2 or invalid v3 fields | Existing store reads; adapter reports Migrated/Normalized. SessionOnly until effective values actually match unnormalized v3 on disk. Load does not apply implicitly; facade explicitly applies safe values. |
| Windows future/unknown version | Inspect root before invoking the legacy loader/recovery. UnsupportedVersion, session defaults, bytes untouched; explicit Save refused. Invalid/unrepresentable version attributes also preserve bytes as unknown version, a deliberate conservative exception to malformed-file recovery. Even future-version XML with malformed trailing body is protected. |
| Windows malformed supported XML | Established loader/recreate-defaults recovery retained; adapter verifies whether recovery really persisted. No claim of preserving a malformed supported file. |
| Windows Save failure / Cancel | Form edits a detached copy; successful legacy Save precedes Apply. Failed Save keeps session and old XML where the existing store can; form remains open with existing error reporting. Cancelling an edit does not save/apply. Existing legacy file-copy replacement is not a transactional filesystem guarantee. |
| Browser no key | Defaulted, session-only/unconfirmed. Reading does not create a key or claim that writes are possible. |
| Browser supported blob | JS validates exact keys/scalar kinds, Common normalizes bounds. Loaded/Persisted only for already valid values; Normalized/SessionOnly until intentional Save. Reading never rewrites. |
| Browser corrupt/unknown format | InvalidData/UnsupportedVersion; session defaults; original blob preserved on read and explicit Save. Owner can deliberately remove that one prototype key in browser site storage to recover; the app never auto-deletes it. |
| Browser storage getter/read/write denied, quota, absent API, silent discard | StorageUnavailable/Unavailable. Explicit Save/apply still applies session preferences and visibly warns that reload persistence is unavailable/unconfirmed. No workbook settings write is attempted. |

Writes recheck incompatible existing storage before replacement. Neither adapter
promises concurrency isolation against another writer between check and write.
Diagnostics contain stable outcome codes/status, not raw preferences/workbook
contents or credentials. Existing Windows logger, XML recovery and help wiring
remain in place. No source identifiers or E4 consent/COM tokens are stored.

## Cross-host parity and actual persistence limits

| Preference | Windows effective path | Prototype effective path |
|---|---|---|
| Alpha .05 default / .01 saved | XML -> shared snapshot -> CoreServices -> AppGlobals; UibyID initialization and ANOVA/KW explicitly use facade alpha | JS -> Common -> component fields; new run uses snapshot option factories and identical alpha in highlighting |
| Seed sentinel / 12345 | XML -> snapshot -> CoreServices -> existing accessors | Retained in stored snapshot; no new seed UI or unsupported stochastic workflow claim |
| Trace True/False | XML -> snapshot -> existing CoreServices flag | Retained in stored snapshot; no new Office logger implemented (03.02 remains pending) |
| Digits 8 / 6, enum 0 / 1, upper bound True / False | Existing Settings controls -> snapshot -> existing display accessors/legacy formatter | Existing controls -> snapshot -> unchanged semantic planner/Core display policy |
| Restart | Real legacy XML save/reload tested without Excel | Fresh simulated JS contexts with shared storage tested; actual task-pane/browser reload still requires owner smoke |

Browser persistence is implemented using localStorage and verified write/readback
under the current origin's storage policy. **Actual Excel Online task-pane iframe
storage was not exercised in this batch**: persistence/partitioning or denial in
that iframe is unverified until the owner's acceptance check. No Mac runtime
claim is made. A denial is an expected visible session-only outcome, not a claim
that durable Office persistence worked. No technical implementation blocker was
encountered; this remains pending owner host acceptance, not automatic completion.
No existing component test project/bUnit dependency is present; the small UI
wiring is build-checked and covered by the manual steps below.

## Owner manual acceptance (not executed by Codex)

1. **Windows x64 Excel-DNA:** close Excel, back up the existing XLL-adjacent
   `BESHStatNG.settings.xml` privately, and reopen with the rebuilt add-in. Open
   Global Settings: title reports persistence/load status. Set alpha .01, places
   6, Scientific notation, upper bound Off, seed 12345; keep trace as desired.
   Select OK. Reopen Settings immediately and verify all effective values. Close
   and reopen Excel and verify again. Open ANOVA or KW and inspect .01 alpha
   initialization and numeric p-value display. Restore the backup afterward while
   Excel is closed, or restore alpha .05, seed blank (Int32.MinValue), trace On,
   digits 8, Less-than-threshold, upper bound On. Check Cancel leaves values alone;
   an unwritable test copy of the settings location should retain session settings
   and show the existing save error rather than close the form.
2. **Excel Online/WasmSmoke:** in a disposable workbook, set alpha .01, digits 6,
   Scientific notation and upper bound Off; select **Save/apply preferences** and
   inspect the status. Reload the actual task-pane URL; check fields and run
   ANOVA/KW, including p-value highlighting. Unsaved local alpha .025 must not
   change stored defaults after reload. With browser storage denied, Save/apply
   must display **Session-only** / persistence unavailable or unconfirmed; do not
   expect values after reload. Confirm current-sheet occupied output Cancel and
   Overwrite still behave as accepted, and new/current output requested at **C10**
   retains column C / row 10. Restore original prototype preferences afterward.

No output fixture regeneration is required or permitted for these checks.

## Automated validation and commands

Baseline was run before production edits. It confirms **2,612** existing passes:
Core 1,024 / Common 519 / Windows 719 / JS 318 / Python 32 (22+5+5).
The initial PowerShell `*> baseline.log` launcher reported exit 1 from native
stderr handling despite every internal command and the final completion message
passing. Final full validation uses cmd redirection (as the Gate B report did)
and returned exit 0. No package/network install was needed.

Focused commands, from the repository root:

```powershell
dotnet test .\BESHStat.Excel.Common.Tests\BESHStat.Excel.Common.Tests.vbproj --no-restore -c Debug --filter FullyQualifiedName~PreferenceTests --logger "trx;LogFileName=Preferences.trx" --results-directory TestResults/Codex/Batch0301
dotnet test .\BESHStat.Core.Tests\BESHStat.Core.Tests.vbproj --no-restore -c Debug --filter FullyQualifiedName~SettingsAndPValueTests --logger "trx;LogFileName=CoreSettings.trx" --results-directory TestResults/Codex/Batch0301
node --check BESHStat.WasmSmoke/wwwroot/js/preferences.js
node --test BESHStat.WasmSmoke/tests/preferences.test.cjs
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\BESHStatNG.Test\BESHStatNG.Tests.vbproj /t:Build /p:Configuration=Debug '/p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' /m /v:minimal
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' .\BESHStatNG.Test\bin\Debug\BESHStatNG.Test.dll '/TestCaseFilter:FullyQualifiedName~PreferenceAdapterTests' '/Logger:trx;LogFileName=WindowsPreferences.trx' '/ResultsDirectory:TestResults\Codex\Batch0301'
```

Focused outcomes: **24/24 new Common**, **4/4 existing Core settings**,
**14/14 new Node**, **13/13 new Windows** passed. The existing six Gate B
compatibility tests also passed in an early compile check. Initial new-test
compile errors (`DoubleValue` vs actual `BinaryValue`, and an inferred XML loop
variable colliding with the System.Xml namespace) were corrected; no existing
tests were weakened or fixtures regenerated.

Final full gate (same supported gate used for baseline; log redirection only):

```powershell
cmd.exe /d /c 'powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Python -PythonExe "C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe" -Js -Wasm -Legacy -VisualStudio 2026 > TestResults\Codex\Batch0301\full-gate.log 2>&1'
```

| Suite | Baseline | Final | Added |
|---|---:|---:|---:|
| Core | 1,024 | 1,024 | 0 |
| Common | 519 | 543 | 24 |
| Windows .NET Framework | 719 | 732 | 13 |
| JavaScript | 318 | 332 | 14 |
| Python | 32 | 32 | 0 |
| **Total** | **2,612** | **2,663** | **51** |

All full-gate tests pass with zero failures/skips. WASM Debug and VS2026 Windows
Debug builds pass with zero compiler warnings/errors, including packed x86 and
x64 XLLs. `UTA001 MatrixTestHelpers` remains the known nonfatal discovery advisory.
TRX overwrite notices concern ignored test reports. The Debug post-build routine
exits before its PreRelease signing guard; no signing or installer was executed.
Stage304 inventory, authored methods catalog, generated metadata check, and
retired-helper audit pass. No Mac/runtime Excel/browser acceptance is implied.

The script builds the Windows test project and WASM separately. The additional
VS2026 Debug solution build also returned **exit 0**, with no compiler warning or
error messages, and packed both XLLs:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' /m /v:minimal
```

Final packed artifacts in `BESHstatNG/bin/Debug`: `BESHStatNG-AddIn-packed.xll`
(1,931,264 bytes), `BESHStatNG-AddIn64-packed.xll` (1,857,024 bytes). These are
build artifacts, not live-host validation or deployment.

Local ignored evidence is under `TestResults/Codex/Batch0301`: baseline/full-gate
logs, focused logs/TRX, original generated bytes, and initial SHA-256 manifest
for every initially tracked file. Full suite TRX files use
`TestResults/Codex/{Core,Common,Legacy}.trx`.

## Explicit delivery inventory

New files:

- `BESHStat.Excel.Common/src/Infrastructure/Settings.vb`
- `BESHStat.Excel.Common.Tests/src/PreferenceTests.vb`
- `BESHstatNG/src/AppInfrastructure/LegacyPreferenceStore.vb`
- `BESHStatNG.Test/src/PreferenceAdapterTests.vb`
- `BESHStat.WasmSmoke/Infrastructure/BrowserPreferenceStore.cs`
- `BESHStat.WasmSmoke/wwwroot/js/preferences.js`
- `BESHStat.WasmSmoke/tests/preferences.test.cjs`
- `docs/architecture/batch-03-01-versioned-application-settings.md`

Modified files:

- `BESHstatNG/src/AppInfrastructure/AppGlobals.vb`
- `BESHstatNG/src/UI/Ui12GlobalSettings.vb` (detached OK candidate, save-before-apply,
  status in existing title; no designer/control/help change)
- `BESHstatNG/BESHStatAddIn.vb`
- `BESHstatNG/BESHStatNG.vbproj` (one explicit compile entry)
- `BESHStatNG.Test/BESHStatNG.Tests.vbproj` (one explicit test compile entry)
- `BESHStat.WasmSmoke/Pages/Home.razor`
- `BESHStat.WasmSmoke/wwwroot/index.html` (load separate adapter script)
- `docs/architecture/gate-b-common-v1-contract-freeze.md` (owner-status only)
- `docs/architecture/gate-b-common-v1-contract-inventory.md` (owner-status only)
- `planning/BESHStatNG_CrossPlatform_MasterPlan.md` (active statuses only)

No Core source edits were needed: its real XML serializer/store, finite defaults,
display enum/policy and numeric algorithms were reused unchanged. All existing
Common v1 output/analysis APIs, output JS, accepted goldens and transport fixtures
are untouched. The original 03.01 roadmap row, dependencies, 74 Phase 04A rows,
companion audit and historical records are preserved. Counts remain **222 / 27 /
195**, with **03.01 IN PROGRESS / PENDING OWNER REVIEW**, not accepted. Separate
owner acceptance and merge would change that to 28 / 194. No 03.02 or later work.

Suggested commit message (not committed):
`feat(common): add batch 03.01 versioned application preferences and host stores`.

## Final hygiene and review handoff

Git originally refused the sandbox account's ownership. All Git checks therefore
use `-c safe.directory=C:/Users/peter/source/repos/BESHstatNG` for this command
only, without modifying global configuration. Initial/final branch and HEAD are
unchanged: `codex/03-01`, `ce2aaee0f50ffae56ea75b03fdf3bedf57248deb`.

Before builds, SHA-256 hashes were captured for **1,903** initially tracked files
and exact bytes saved for HelpLinks.vb, HelpContext.vb and VersionAttributes.vb.
After the last build, generated bytes were archived locally and all three source
files restored to their initial exact bytes. Hash comparison confirms that only
the **10 modified paths** listed above differ from the initial checkout; every
other tracked file retains its original bytes, including Core, accepted histogram
work, protected source, Stage304/OutputSemantics/OutputLayout/Office fixtures,
help/version originals, installer/signing files and the Phase 04A companion audit.
Separate comparisons confirm the original 03.01 row, all 74 Phase 04A rows and
the entire seven E1–E7 table rows remain unchanged.

Final commands:

```powershell
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --stat
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --name-only
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG ls-files --others --exclude-standard
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG stash list
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG rev-parse HEAD
```

`diff --check` is clean. Git's existing CRLF-to-LF advisory appears for several
Windows sources (and the byte-restored VersionAttributes.vb); no global line-ending
configuration was changed. Final working tree contains exactly **10 modified +
8 new authorized paths** from the inventory above, with no deletions. Both
historical stashes are unchanged, unapplied and undropped. Initial status was
clean. No commit, push, merge, branch switch, remote-ref change, installation,
deployment, signing or framework-target change occurred. Stop for independent
review and the separate owner host acceptance above.
