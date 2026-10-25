# BESHStat.OfficeAddin — Office.js / Blazor WebAssembly Excel host

`BESHStat.OfficeAddin` is the **production-oriented Office.js/Blazor WebAssembly implementation under development**. It targets **.NET 10** and runs in an Excel task pane. Its numerical calculations come from [`BESHStat.Core`](../BESHStat.Core/README.md), while typed spreadsheet inputs, analysis requests, result documents, application preferences and lifecycle contracts come from [`BESHStat.Excel.Common`](../BESHStat.Excel.Common/README.md). JavaScript adapts these services to Office.js; it must not become an independent statistical engine.

**Availability:** This is a validated development implementation, **not a released or supported Mac/Excel Online edition**. Existing owner-accepted Excel Online flows include ANOVA/Kruskal–Wallis runs and guarded writes, options, specification previews and workspace/Ribbon navigation. Those checks are not a substitute for a complete Windows desktop, Mac desktop, tenant, accessibility, deployment and security release matrix. The supported product remains the [Windows Excel-DNA add-in](../BESHstatNG/README.md).

## Current capabilities

- **Office host and lifecycle:** readiness, retry and failure handling, standalone-browser fallback, scoped services, documented host requirements and no automatic workbook mutation on page load.
- **Method catalog and Ribbon:** group/method navigation using the authoritative generated catalog; Office commands open the existing task pane. Only supported runnable methods are enabled. Other documented methods and the Ribbon Chart Export command are unavailable, not silently emulated.
- **Runnable analyses:** **One-Way ANOVA** (including Welch and supported Fisher LSD, Bonferroni, Tukey–Kramer and Games–Howell options) and **Kruskal–Wallis** (including supported comparisons) through registered Common/Core handlers. This is not a claim that all 76 documented methods run in Office.
- **Input experience:** RefEdit-like worksheet-qualified range capture and manual entry; groups in columns, groups in rows and long format with two independently selected, possibly nonadjacent columns. Bounded whole-column acquisition, actual worksheet-row alignment, missing-value semantics, overhang warnings and source revalidation are enforced.
- **Read-only specification builders:** formula/design, categorical and reference-level, subject/visit and mixed-model role previews. Variables are discovered from the active or chosen worksheet, named like Excel-DNA (`Header | VarA` or `VarA`), with blank physical columns omitted but original Excel letters retained. These previews **do not fit or write unsupported models**.
- **Results and guarded output:** Common result artifacts and numeric/p-value presentation; current or another existing worksheet, or a new worksheet. `Write results` performs fresh inspection, requests explicit consent for occupied cells, supports safe numeric suffixes for new worksheet names, and verifies submitted values and styles. **New Workbook output is unavailable**. Native writes cannot be rolled back as an atomic transaction.
- **Global Options:** alpha, random seed, diagnostic trace, p-value precision and small-value/upper-bound display policy. Preferences use browser persistence when available, with a visible session-only fallback. **Force English interface** is on by default; users can disable it to follow supported Office/browser languages.
- **Help and diagnostics:** version-aware documentation links; persistent About/Help header links (About includes Apache License 2.0); a sanitized, bounded in-memory Diagnostics/Log pane; truthful progress, faults and partial-write warnings.

## Workspace

The sticky workspace header includes **About BESHStatNG** and **Help home**. Four tabs are available:

| Tab | Purpose |
|---|---|
| **Analysis** | Select a supported method, edit options, capture inputs, run, inspect results and write with explicit safeguards; open the *specification builder preview* separately from runnable analyses. |
| **Options** | Edit, save, discard or restore global preferences and language policy; session-only Save is identified when browser persistence fails. |
| **Export Chart** | **Informational placeholder** and chart-export Help link. Export is not implemented in this host; its Ribbon action is disabled. |
| **Diagnostics / Log** | Review and copy sanitized session diagnostics; opt-in development probes remain separate from normal user workflows. |

The Office Ribbon links to the existing workspace, with enabled One-Way ANOVA/Kruskal–Wallis and working About, Settings, Help and Show Log commands. Other methods remain disabled based on capabilities. Ribbon rendering, command activation and task-pane behavior must be checked in genuine Excel; a validated manifest and synthetic browser tests alone are insufficient.

## Running an analysis in a disposable workbook

1. Sideload the **development manifest**, start the Office host and open the task pane in Excel. Select **Analysis**, then an available method and options.
2. Choose the grouped-data arrangement, enter or capture the source range(s), and click **Run analysis**. Input is acquired and validated afresh; separate Validate is optional. For long-format input, the group and value ranges use matching physical worksheet rows, with explicit warnings for excluded unmatched overhang.
3. Review the read-only results. Set the existing-sheet start cell or a new worksheet name and anchor, then click **Write results**. Inspection is automatic; if occupied cells would change, the add-in asks for fresh **No / Yes** consent. Declining writes nothing.
4. Verify the returned **Completed** status and resulting tables. A verified write can be followed by another independent analysis or a rerun after changing options. Changes retire old results and consent, not the editable source references.

When a write outcome is uncertain or partially written, **inspect the workbook manually and do not automatically retry that write**. The writer deliberately retains a protective uncertainty state. Sheet names, source references, destinations and preferences are revalidated as required; never interpret an occupied or populated sheet by itself as proof of successful readback.

New worksheet names use the lowest unused positive suffix if the requested base is occupied (`ANOVAresults`, `ANOVAresults1`, `ANOVAresults2`), preserving the user's original base field. This is not permission to overwrite an existing worksheet. Output formatting follows the shared numeric p-value/significance presentation; actual Excel number and style readback remains an acceptance requirement.

## Source and ownership

| Concern | Primary locations |
|---|---|
| Workspace shell, tabs, header and About | `App.razor`, `wwwroot/` |
| Analysis navigation, options and controls | `Components/AnalysisShell.razor`, `Components/AnalysisPilot.razor`, `Components/OptionEditors/`, `Components/RangePicker.razor`, `Components/OutputPicker.razor` |
| Read-only model builders | `Components/ModelBuilder/`, `Components/RoleEditors/` |
| Global options, Help and diagnostic UI | `Components/GlobalOptions.razor`, `Components/DiagnosticLog.razor`, `Components/HelpAction.razor` |
| Typed host/operation services | `Services/`, `Infrastructure/` |
| Office.js transport, range, writer and commands | `wwwroot/js/officeHost.mjs`, `workbookAdapter.mjs`, `productionWriter.mjs`, `ribbonCommands.mjs` |
| Office development manifest and tests | `manifest-dev.xml`, [`tests/`](tests/README.md) |

Maintain host-neutral contracts in Common, numerical methods in Core, and browser-specific Excel requests here. Keep the authoritatively documented method catalog separate from the actually runnable handler/command set. Historical implementation evidence is in [`../docs/architecture/`](../docs/architecture/README.md); this README describes the **current** product behavior, not individual corrective amendments.

## Build and local development

Use Visual Studio 2026 with the **.NET 10 SDK** and WebAssembly workload, Node.js 20+, a trusted local HTTPS certificate, and a supported Excel test environment. From the repository root:

```powershell
dotnet build BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj -c Debug
dotnet run --project BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj --launch-profile https
```

The Office host normally serves **https://localhost:7141/**. Sideload `manifest-dev.xml` in a **disposable** Excel workbook, following tenant policy; re-sideload after manifest command changes. The independent [`BESHStat.WasmSmoke`](../BESHStat.WasmSmoke/README.md) harness has its **own manifest and port 7131** and must not be substituted for this host. No production deployment is implied by local HTTPS, sideloading or a successful standalone browser run.

## Verification and live-host evidence

```powershell
# Run from repository root
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release

# JavaScript adapter and writer tests
cd BESHStat.OfficeAddin
npm test
```

C# service/Razor-component tests are in `tests/OfficeAddin.Tests.csproj`. The `tests/browser/` scripts provide opt-in Edge simulations for readiness, workbook acquisition, inputs, builders, settings, Ribbon commands and guarded output. The repository also includes a **genuine authenticated Excel Online** regression runner, `tests/browser/live-edge-workflow.mjs`, which requires an explicitly prepared and authorized Edge workbook session. Follow [`tests/README.md`](tests/README.md) before using browser automation; do not confuse simulated Office objects with live Office.js operations.

For any host acceptance, check both empty and occupied outputs, option-change reruns, stale references, new-sheet naming, workbook controls, numeric p-value formatting, the About/Help links, all four tabs, Force English and Ribbon activation. Record Excel Online, Windows desktop and Mac evidence **separately**, and redact workbook content and credentials from diagnostic artifacts.

## Development direction and limits

The remaining work includes the formal shared-host **Gate C review**, audit/extraction of portable code still in the Windows UI and renderers, additional verified Common analysis handlers and Office editors, portable chart specifications/renderers/export, possible Office Custom Functions, deployment/hosting and platform qualification. A documented method is not an implemented Office method. New Workbook output and native Office chart export remain unavailable. No Mac general-availability or LibreOffice integration is asserted.

See the [current cross-platform roadmap](../planning/README.md) and [architecture/acceptance evidence](../docs/architecture/README.md).
