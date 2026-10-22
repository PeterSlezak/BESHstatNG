# BESHStat.OfficeAddin — production host foundation

Batch **04.01 owner-accepted / committed on main at d7888cd**. The owner confirmed
real Excel Online/standalone, probes, Retry and independent WasmSmoke.
Batch **04.02 owner-accepted / merged on main at 6b1e978**, including genuine
selected-range reading and diagnostics. This supersedes historical pending
acceptance wording in the 04.02 architecture report and investigation below.
Batch **04.03 implemented / uncommitted / pending owner review and live Excel acceptance**.
The owner reports the original 04.03 functionality and genuine Excel Online smoke
passed; the requested two-level dropdown amendment still awaits owner acceptance.
The minimal production host runs real Core/Common probes and offers an explicit
read-only selected-range check when Excel-ready. Page load and Retry do not read
workbook cells. No analysis or production output write is enabled.

`BESHStat.WasmSmoke` remains an independent regression harness with its own
manifest, identity and `https://localhost:7131/` endpoint. Neither project depends
on the other. Future workbook adapters and reusable analysis screens belong to
04.04–04.07; Gate C has not been reached.

## Batch 04.03 method navigation and options

Launch on **7141** using the existing `https` profile and unchanged manifest.
The **Analysis options** section works even when Office is unavailable or still
initializing. Choose **Analysis group**, then **Statistical method**, using the
two labelled native dropdowns. Tab/arrow keys/Enter and visible focus are supported.
All **12 groups / 76 documented methods** come from `GeneratedMethodMetadata.All`,
using the generated primary category. Groups sort ordinally by category; methods
sort by display name then metadata key. Generated metadata has no Ribbon-order
field, so this UI does not claim exact Ribbon ordering.

Only **Parametric → One-Way ANOVA** and **Nonparametric → Kruskal-Wallis Test**
currently have verified Common handlers and supported typed Office editors.
Other methods are labelled **Not yet available** and disabled. Multivariate
Analysis, Graphics and other unsupported groups announce **Not yet available in
this Office add-in**; their documented methods remain listed. Listing a method
does not imply calculation or output support. Below the dropdowns, the selected
method's Common descriptor supplies its title/description and active option schema.

ANOVA offers alpha and Welch, Fisher LSD, Bonferroni, Tukey–Kramer and
Games–Howell flags. Kruskal–Wallis offers alpha and Dunn comparisons. Defaults
come from Common: alpha **0.05**, ANOVA flags **false**, Dunn **true**. Alpha uses
an invariant decimal point and must be finite and strictly between zero and one.
Invalid text stays visible with an inline error; it cannot produce a snapshot.

Changing groups clears the selected method and closes its draft. Changing methods
**resets options to Common defaults**. Reselecting the same group/method and Retry
initialization preserve its draft, including invalid alpha text. Closing/reopening
or reloading starts with no selected group or method.
There is no draft storage. Editing never reads cells, writes output or executes
an analysis. **Run analysis** is disabled with a 04.04 explanation. Help launch,
range picker, validation preview and output/consent UI are deferred.

Owner acceptance (record genuine host results separately from automated simulations):

- Standalone: `office.unavailable`, Core **0.5**, Common **2 / 4**. Choose
  Parametric → ANOVA, edit alpha/two flags; choose Nonparametric → KW and edit
  alpha/Dunn; switch back and confirm defaults. Enter `0`, `1`, `NaN` or `0,05`:
  inline error. Multivariate Analysis and Graphics show unavailable explanations.
- Genuine Edge Excel Online: `office.excel-ready` / `OfficeOnline`, same probes.
  Check all 12 groups and filtered methods; no per-method button list remains.
  Select Parametric → ANOVA then Nonparametric → KW; verify description/schema,
  unavailable methods cannot be selected, labelled controls,
  visible keyboard focus and narrow-pane layout. Retry retains the current
  group/method/draft. Reopen starts unselected. Editing leaves cells and selection unchanged.
- Explicit 04.02 read-only check still returns `workbook.ok` and dimensions for
  a small selection. Writer remains unavailable; Run stays disabled. Confirm
  independent WasmSmoke **7131** separately.
- Capture both ANOVA and KW screens and record any UI defects. Codex screenshots
  use simulated Office and do not establish genuine Excel acceptance.

See [04.03 architecture and test evidence](../docs/architecture/batch-04-03-method-navigation-options-ui.md).

## Prerequisites and launch

Use Visual Studio 2026 with .NET 10 Blazor/WebAssembly development support, the
.NET 10 SDK (verified here: **10.0.402**), and Node.js for dependency-free tests
(verified here: **24.20.0**). Core/Common remain `netstandard2.0`; Windows Excel-DNA
remains .NET Framework 4.8. Blazor packages match WasmSmoke's **10.0.13**.
No Node packages, extensions or SDKs are installed by the test scripts.

The owner must trust a developer HTTPS certificate, if not already trusted:

```powershell
dotnet dev-certs https --check --trust
dotnet run --project BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj --launch-profile https
```

Certificate trust is an owner setup action; it was not run by Codex. Alternatively,
select this project as the Visual Studio startup project and choose `https`.
The URL is **https://localhost:7141/**. Keep the server running while sideloaded.
The profile does not automatically open a browser. No production/published URL is
configured. This manifest is development-only and is not a release package.

## Distinct manifest and owner smoke

Upload **`BESHStat.OfficeAddin/manifest-dev.xml`**. Its display name is
**BESHStatNG Office Foundation Dev**, GUID
**`5f70b95c-dc4a-4f38-b78d-0a7348657e19`**. It is an add-in-only XML `TaskPaneApp`
for `Workbook`, requiring ExcelApi **1.1**, with **ReadWriteDocument** permission.
Microsoft requires this permission for application-specific Excel APIs even when
only reading. Higher permission permits writes in principle; this batch's UI
does not authorize or perform them. The production writer remains fail closed.
There are no Ribbon commands or VersionOverrides. **Remove/re-sideload this
development manifest after rebuilding so the changed permission takes effect.**

1. First open the HTTPS URL in a normal browser to confirm certificate trust and
   successful WASM loading. Expect **Office host not available**, Core CDF(0)
   **0.5**, and Common **2 descriptors / 4 registered handlers**, both passed.
   This is browser evidence only; it must not report Excel connectivity.
2. In a disposable workbook in Excel Online, open **Home → Add-ins → More Settings →
   Upload My Add-in** (older clients may use More Add-ins / My Add-ins), browse to the new
   XML, then open **BESHStatNG Office Foundation Dev**. Sideloading may require
   administrator permission for the tenant. Keep local HTTPS reachable from the
   browser. Record the display name, host diagnostic `office.excel-ready`, actual
   Core/Common values, client/platform and absence of uncaught console errors.
3. For Windows desktop Excel, use the same development manifest through an
   owner-configured trusted shared-folder catalog: **File → Options → Trust
   Center → Trust Center Settings → Trusted Add-in Catalogs**, add the shared
   folder URL containing the XML, enable its menu entry, restart Excel, then
   select the add-in from **My Add-ins → Shared Folder**. Some Microsoft 365
   clients also expose a custom upload route. No catalog, registry, share or
   Office installation is configured by this repository's tests.
4. Retry initialization and repeatedly reload, close and reopen the pane. The
   probes should repeat deterministically; the selection, sheets and values must
   remain unchanged. Check WasmSmoke separately on **7131** with its original
   `office-manifest.xml` and original display name.

### Batch 04.02 selected-range acceptance

Use a disposable workbook and the re-sideloaded manifest. In standalone **7141**,
expect `office.unavailable`, **0.5 / 2 / 4**, working Retry and no read button.
In genuine Excel Online expect `office.excel-ready`, `OfficeOnline` and those same
probes. Select a small rectangle containing headers, numeric/zero values, a true
blank, Boolean, text, `=""` and an error formula such as `=NA()`. Click **Read
selected range (read-only)** once. Expect `workbook.ok`, correct rows/columns/cell
count, and no echoed values, worksheet/workbook names or addresses.

Repeat with an alternate small rectangle and a single cell. Select an oversized
rectangle (over 10,000 cells): expect `input.range-cells` before value transfer.
A disjoint selection must reject; its bounding rectangle is never substituted.
Retry clears the previous summary and performs no background read. Close/reopen
the pane and inspect again. Verify unchanged sheet count, values, formulas,
formatting and selection. No result/write controls or worksheet creation exist.
Confirm independent WasmSmoke **7131**; desktop Excel/Mac are optional owner checks,
unperformed by Codex for 04.02. The canonical mixed-cell parity is automated, not
an instruction to export workbook contents for comparison.

### Historical investigation of the owner-observed read timeout (04.02 now accepted)

During 04.02 the owner observed `input.time-budget` for a small selection in genuine Excel
Online. The failing stage and root cause are **not yet established**. Parent-shell
CSP/Office.js messages are not task-pane evidence. The reader/deadlines remain
unchanged; the instrumentation was added for obtaining that evidence. Subsequent
owner-confirmed genuine selected-range success supersedes this pending gate.

1. Run the host in **Debug** with the current dev manifest re-sideloaded. In Edge
   DevTools select **https://localhost:7141/** from the execution-context/frame
   dropdown, not `baseFrame`, `about:blank` or the Excel parent shell. Evaluate
   `location.origin === "https://localhost:7141"`; continue only if **true**.
2. Enable local diagnostics in that frame, retaining Office's existing parameters:

   ```js
   { const u = new URL(location.href); u.searchParams.set("beshOfficeDiagnostics", "1"); location.replace(u); }
   ```

   After reload, confirm genuine readiness/probes again. Select the same small
   rectangle. Click **Inspect selection metadata only (read-only)**, record only
   status/dimensions, then **Read selected range (read-only)** and record its
   status/dimensions. The second action still acquires the full typed snapshot;
   metadata-only success is insufficient for acceptance. Neither reads on load
   or Retry. While an operation is pending or after it ends, **Refresh stage
   summary** shows C# fixed codes/timings. In the same frame capture:

   ```js
   JSON.stringify(__beshOfficeDebug.snapshot())
   ```

   Share only that sanitized snapshot and the C# stage summary. Both are bounded
   to 80 events and contain no workbook/sheet IDs, names, addresses, values,
   formulas, exception messages/stacks or URLs. No telemetry or storage is used.
   C# milliseconds start at scoped recorder creation; JS milliseconds start at
   page diagnostic installation. Compare adjacent stage differences within each
   trace, not absolute timestamps across traces. `cs.timeout` identifies the C#
   deadline/checkpoint; `read.js-timeout` and `cs.js-timeout-received` identify a JS
   deadline/reply. Closely racing deadlines can leave both traces; the returned
   public code remains `input.time-budget`.
3. If needed run this independent **read-only** control in that same frame. It
   reports only a fixed stage/status, dimensions on success, and elapsed time:

   ```js
   (async () => {
     if (location.origin !== "https://localhost:7141") {
       console.log({status:"control.wrong-frame"}); return;
     }
     const start = performance.now();
     let timer, stage = "control.run-started";
     try {
       const native = Excel.run(async context => {
         stage = "control.run-entered";
         const r = context.workbook.getSelectedRange();
         r.load("rowCount,columnCount");
         stage = "control.sync-started";
         await context.sync();
         stage = "control.sync-completed";
         return {status:"excel-read-ok", rows:r.rowCount, columns:r.columnCount};
       }).catch(() => ({status:"excel-read-failed"}));
       const result = await Promise.race([native, new Promise(resolve => {
         timer = setTimeout(() => resolve({status:"excel-read-timeout"}), 10000);
       })]);
       console.log({...result, stage, elapsedMs:Math.round(performance.now()-start)});
     } catch {
       console.log({status:"excel-read-failed", stage, elapsedMs:Math.round(performance.now()-start)});
     } finally { clearTimeout(timer); }
   })();
   ```

   A timeout cannot abort an already submitted native request. Do not run repeated
   controls while one is pending; close/reopen the pane before the next attempt.
   If the control also hangs, report its fixed stage and sanitized dependency
   statuses from this frame. Do not share HARs, request headers/bodies, credentials
   or workbook data. Compare independent WasmSmoke/Edge versus Opera only within
   owner-approved tenant/browser policy; do not weaken CSP/sandbox settings.
4. Disable by removing only `beshOfficeDiagnostics` from the frame URL and
   reloading. The added workbook traces and metadata button are disabled by
   default. The C# recorder and button are compiled **off in Release**, even with
   the query parameter; the production transport passes `false` to the JS
   diagnostic option in that configuration.
   The accepted 04.01 bootstrap diagnostic remains unchanged in availability.

See the [timeout amendment](../docs/architecture/batch-04-02-office-workbook-adapter.md#owner-smoke-timeout-investigation-amendment--22-october-2026)
for historical stage interpretation and simulations; its live gate is now accepted.

### Adapter limits and ownership

`IWorkbookAdapter` borrows the accepted scoped readiness service and returns typed
Common ranges. It advertises only `input.read-selected-range`; writing and new
workbooks are unavailable. Its metadata-only query returns dimensions and a typed
Common reference for later selectors, without cell values. Explicit references additionally require the actual worksheet ID,
name and canonical coordinates; unverifiable book/selection identities reject.
References address only the workbook currently hosting the pane; they do not
establish persistent identity across operations or workbooks.

Default ceilings: **10,000 cells**, **2,000 cells per interop tile**, **64 MiB
estimated bytes**, **30 seconds**. Callers can lower budgets, not raise them in
this batch. All source metadata is checked before allocating rows/loading cells.
Wide single rows tile across columns. Byte estimates include a conservative base
and actual transported string lengths, but cannot guarantee Office/browser heap
use. Retained snapshots still contain the entire accepted input.

Each tile uses a fresh `Excel.run`, actual type metadata and fresh identity/
selection observations before and after payload sync. No Excel proxy/context
crosses a run. A changed document session, worksheet identity/name or selection
bounds rejects the operation; no partial Common snapshot escapes. Verification
uses current selection at checkpoints, not a persistent selection ID or an
atomic historical cell-value snapshot. Transient changes that return to identical
metadata between checkpoints, and concurrent cell edits, cannot all be detected;
reads are not transactionally isolated. No whole-row/column narrowing is applied.
Physical workbook ID/name and selection revision stay unknown; DateSystem stays
Unknown and numeric serials remain numeric. Native error text is preserved only
for actual Error cells; ExcelApi 1.1 supplies no separate native error code here.

Cancel/dispose stops further payload scheduling and discards the pending snapshot.
An already submitted native sync cannot be forcibly aborted. Concurrent requests
on one adapter return `host.busy`; separate panes own separate adapters. The
readiness service lifetime is independent. Cleanup releases module/object references
and operation metadata; there are no workbook event listeners or background reads.

`WriteAsync` accepts existing Common `OfficeOutputPlan` / `WriteIntent` /
`WriteConsent` and returns existing `WriteResult`. Production JavaScript has **no
writer installed**, so requests reject before mutation. Injected, explicitly
simulated test ports exercise occupancy/consent revalidation, actual mock identity,
successful sync, cancellation and mutation uncertainty. They are not a production
writer or a promise of 04.04 destination/consent UI. No rollback or undo is claimed.

The two add-ins can coexist. To replace an older sideload of this foundation,
remove only **BESHStatNG Office Foundation Dev** in My Add-ins (or remove its
catalog entry), close its task pane, then upload/select the new XML. Do not reuse
WasmSmoke's GUID or remove its separate entry. For stale developer assets, close
the pane, remove this development sideload, reload/clear this localhost origin's
browser cache as appropriate, and add it again. Avoid broad Office cache cleanup.

**Mac Excel is a later acceptance target and has not been verified.** Automated
Mac platform metadata cases do not establish Mac host support.

## Diagnostics and troubleshooting

Only fixed diagnostic codes and a whitelisted platform are displayed. Do not
paste workbook contents, URLs, addresses, HTTP response bodies or credentials
into logs or bug reports. There is no telemetry or diagnostic export service.

- `office.unavailable`: outside Office, missing/blocked CDN, or no Office.js after
  the five-second bound. The browser page remains usable; load the sideloaded pane
  or resolve CDN access and click **Retry initialization**.
- `office.timeout`: readiness did not finish in an embedded/unknown context, or
  the outer interop deadline expired. A top-level standalone window with a pending
  Office promise falls back to `office.unavailable` after five seconds. Retry
  after checking connectivity/tenant restrictions. Office context is never used
  as a shortcut around `Office.onReady`.
- `office.unsupported-host` / `office.unsupported-api`: a non-Excel/unknown Office
  host or missing ExcelApi 1.1 support. Workbook features are not advertised.
- `office.initialization-failed`: safe projection of JS/interop failure. Check
  the local server, TLS certificate and browser network panel for asset-loading
  failures. The UI does not display raw exception text.
- WASM never starts: verify the HTTPS server and `_framework` requests; reload
  once the server/certificate is working. An unrecoverable framework error offers
  a reload link. No offline service worker is installed.
- `probe.failed`: record the displayed check results and build configuration.
  The probes run without Office and never acquire workbook input.

Office.js is loaded by a blocking script tag in the HTML head before Blazor startup,
matching WasmSmoke's script ordering. Blazor renders the local probes/progress UI
independently; the adapter calls `Office.onReady` after the first render and gates
all Office requirement metadata on its completion. A blocked
CDN may delay the HTML page load; the five-second JS readiness bound starts only
once the application is running. .NET interop has a twelve-second outer bound.
Retry is explicit.
Promise-based Office readiness cannot be unsubscribed from Office.js. Retry shares
one pending wait for the same Office/onReady identity, with removable attempt
subscribers. Late completion can be used by a later explicit Retry. Timed-out or
disposed attempts cannot inspect Office context; their listeners/timers are removed.

### Read-only owner diagnostic (no workbook access)

In Excel Online DevTools, use the Console **execution-context/frame dropdown** to
select **`https://localhost:7141/`**, not a `baseFrame...`, Excel shell or
`about:blank` context. First run this Boolean check:

```javascript
location.origin === "https://localhost:7141"
```

If false, select the task-pane context before continuing. Enable the local-only
diagnostic and reload **that frame**, preserving Office's existing query/hash:

```javascript
(() => {
  const url = new URL(location.href);
  url.searchParams.set("beshOfficeDiagnostics", "1");
  location.replace(url.href);
})();
```

Reselect the localhost context after reloading. After readiness settles, run:

```javascript
JSON.stringify(globalThis.__beshOfficeDebug.snapshot())
```

The snapshot contains only a frame category, fixed stage codes, relative timings
and allowlisted resource categories/status numbers. It has no Office APIs, network
telemetry, workbook data, raw errors or URLs. `status: 0`/`null` in resource timing
is **inconclusive**, particularly across origins; it does not prove blocking.
The collector is inactive by default, bounds event history and detaches on pagehide.
`__beshOfficeDebug.stop()` removes its listeners immediately. Remove the diagnostic
query parameter and reload to disable it completely.

In Network, disable cache and group/filter by the **localhost frame/initiator**.
Inspect primary `office.js` and its actual strings/locale/host dependencies. Record
only category, HTTP status, MIME type, redirect yes/no and blocked/abort reason.
Do not export a HAR, headers, response bodies or full request URLs. A pending
`onReady` alone never produces `bootstrap.office-dependency-failed-or-blocked`;
that stage requires a captured Office script error or CSP violation. Network
blocking can still require manual inspection when the browser hides the event.

For `people.aspx`, sandboxed `about:blank`, icon and preload errors, identify the
source frame first. Errors in Microsoft's shell are not automatically task-pane
failures. Do not alter Office iframe sandbox permissions. If dependencies fail in
both add-ins, compare an owner-run Edge session without VPN/content blockers with
Opera under the same tenant policy; do not bypass organizational restrictions.

### Opt-in actual-browser regression runner

An installed Edge and Node are sufficient; no npm/browser installation is needed.
With the HTTPS dev server running, launch a **fresh isolated** headless browser:

```powershell
Start-Process -FilePath 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' -WindowStyle Hidden -ArgumentList @('--headless=new','--disable-gpu','--no-first-run','--remote-debugging-port=9227','--user-data-dir=C:/Users/peter/source/repos/BESHstatNG/TestResults/04-01/edge-readiness','about:blank')
node BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs
foreach ($fixtureName in @('excel','pending','unsupported','missing','dependency','bridge-regression')) {
    node BESHStat.OfficeAddin/tests/browser/readiness-browser.mjs "--fixture=$fixtureName"
    if ($LASTEXITCODE -ne 0) { throw "Browser fixture failed: $fixtureName" }
}
```

Use this isolated debug endpoint only, never an authenticated browser profile.
The driver creates/closes its own tabs, disables stale browser caching and emits
sanitized JSON. The default checks real CDN standalone startup, actual module
import, Blazor interop, probes and three retries. All `--fixture` cases are
**simulated**, including Excel readiness and a deliberately restored double-wrap
regression. They do not certify genuine Excel, platform metadata or pane reopening.
Close the isolated browser after testing. If assets were rebuilt while the server
was running, restart the server so its static-asset manifest is current.

With a **Debug** host and the same isolated browser, the 04.02 browser driver
uses the shared canonical fixture and mocked Office/Excel boundaries, while
exercising real ES-module import, Blazor WASM, typed interop and explicit buttons:

```powershell
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs
node BESHStat.OfficeAddin/tests/browser/workbook-browser.mjs --pending-sync
```

The second run deliberately stalls a simulated payload sync for the unchanged
30-second budget, reports which timer wins, then checks a successful simulated
retry. Both runs are **simulations**, excluded from distinct unit-test counts;
neither authenticates to Office or proves native `context.sync()` health.

## Automated checks

With the same isolated Edge/CDP setup above and a Debug server on 7141, exercise
the two dropdowns and typed options in actual Blazor, with simulated Office only:

```powershell
node BESHStat.OfficeAddin/tests/browser/options-browser.mjs
node BESHStat.OfficeAddin/tests/browser/options-browser.mjs --standalone
```

These check native Tab/arrow/Enter, unavailable methods/groups, alpha/flags,
reset/Retry/reload behavior, narrow layout and no automatic workbook operations.
The Excel simulation also clicks the explicit read-only diagnostic. Sanitized
JSON and six screenshots for this amendment are under `TestResults/04-03-dropdown`.
They do not certify the owner's genuine Excel Online acceptance checklist.

From the repository root, with dependencies already available:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
```

Each command builds the host, runs **153 C# cases**, runs **136 Node
cases**, validates manifest/launch identity and checks built static assets and
Core/Common WASM assemblies. The C# project links the exact production bootstrap
sources and compiles the exact production Razor components. Real HtmlRenderer
output and a dependency-free Blazor event renderer exercise UI bindings and
lifecycle; bUnit is not installed. These tests do not host the browser runtime.
No live Office, public network or
workbook is required by tests. `npm.cmd test` can also be run in this directory
without `npm install`.

The opt-in seven-scenario browser run is separate from these distinct-case counts.
The [readiness debugging amendment](../docs/architecture/batch-04-01-office-host-foundation.md#post-implementation-readiness-debugging-amendment--22-october-2026)
records the proven interop defect, remaining live Excel gate and actual results.
That report's historical 04.01 pending gate is superseded by the owner acceptance
recorded at the top of this README. 04.02 selected-range smoke is owner-accepted;
04.03 navigation/options live acceptance remains pending.

Run the existing `scripts/verify-beshstatng.ps1` separately with
`-Configuration Debug` / `Release -VisualStudio 2026 -Legacy -Wasm -Js -Python`.
The `-Wasm` gate now includes the production host assembly/build as well as
WasmSmoke; `-Js` includes both hosts' Node tests and the resolver.
See the [04.01 architecture/evidence report](../docs/architecture/batch-04-01-office-host-foundation.md)
for exact commands, suite counts, limitations and the owner evidence checklist.
See the [04.02 adapter report](../docs/architecture/batch-04-02-office-workbook-adapter.md)
for this batch's source mapping, test inventory and pending owner checklist.

Microsoft references: [initialization](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/initialize-add-in),
[XML manifest](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/xml-manifest-overview),
[host/requirements and CDN](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/specify-office-hosts-and-api-requirements),
[permissions](https://learn.microsoft.com/en-us/javascript/api/manifest/permissions),
[application-specific API permission requirement](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/requesting-permissions-for-api-use-in-content-and-task-pane-add-ins),
[Excel Online sideloading](https://learn.microsoft.com/en-us/office/dev/add-ins/testing/sideload-office-add-ins-for-testing).
