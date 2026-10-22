# BESHStat.OfficeAddin — production host foundation

Batch **04.01 implemented / pending owner source review and real Excel acceptance**.
This is the beginning of the production Office.js host, with a minimal task pane.
It runs a real Core standard normal CDF probe and validates the actual Common
catalog/handler registry. It does not read or write a workbook or run user analyses.

`BESHStat.WasmSmoke` remains an independent regression harness with its own
manifest, identity and `https://localhost:7131/` endpoint. Neither project depends
on the other. Future workbook adapters and reusable analysis screens belong to
04.02–04.07; Gate C has not been reached.

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
for `Workbook`, requiring ExcelApi **1.1**, with **Restricted** permission.
ReadWriteDocument is unnecessary for readiness and local probes; any later
workbook adapter must separately review its permissions. There are no Ribbon
commands or VersionOverrides in this batch.

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

## Automated checks

From the repository root, with dependencies already available:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
```

Each command builds the host, runs **29 host-free C# cases**, runs **56 Node
cases**, validates manifest/launch identity and checks built static assets and
Core/Common WASM assemblies. The C# project links the exact production bootstrap
sources; it does not host the browser runtime. No live Office, public network or
workbook is required by tests. `npm.cmd test` can also be run in this directory
without `npm install`.

The opt-in seven-scenario browser run is separate from these distinct-case counts.
The [readiness debugging amendment](../docs/architecture/batch-04-01-office-host-foundation.md#post-implementation-readiness-debugging-amendment--22-october-2026)
records the proven interop defect, remaining live Excel gate and actual results.

Run the existing `scripts/verify-beshstatng.ps1` separately with
`-Configuration Debug` / `Release -VisualStudio 2026 -Legacy -Wasm -Js -Python`.
That accepted gate still tests WasmSmoke; it does not include this new host.
See the [04.01 architecture/evidence report](../docs/architecture/batch-04-01-office-host-foundation.md)
for exact commands, suite counts, limitations and the owner evidence checklist.

Microsoft references: [initialization](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/initialize-add-in),
[XML manifest](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/xml-manifest-overview),
[host/requirements and CDN](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/specify-office-hosts-and-api-requirements),
[permissions](https://learn.microsoft.com/en-us/javascript/api/manifest/permissions),
[Excel Online sideloading](https://learn.microsoft.com/en-us/office/dev/add-ins/testing/sideload-office-add-ins-for-testing).
