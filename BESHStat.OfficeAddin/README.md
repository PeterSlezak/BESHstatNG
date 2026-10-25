# BESHStat.OfficeAddin — Office.js / Blazor WebAssembly development host

`BESHStat.OfficeAddin` is the **production-oriented, not-yet-released** cross-platform Excel host. It targets **.NET 10** and runs a Blazor WebAssembly task pane with Office.js adapters. Statistical computations execute in [`BESHStat.Core`](../BESHStat.Core/README.md); typed input, analysis orchestration, output semantics, diagnostics and capability policies come from [`BESHStat.Excel.Common`](../BESHStat.Excel.Common/README.md). JavaScript should remain a thin adapter rather than a second statistical engine.

**Release status:** this is an active development project, **not** a supported Mac, Excel Online or Windows Office.js distribution. The host foundation, selected-range adapter and method/options UI have passed owner-reported real Excel Online tests. The newer ANOVA/Kruskal–Wallis acquisition/run/write pilot is implemented and covered by simulations, but its genuine workbook writer acceptance is still pending. Mac Excel validation and production deployment are not claimed.

## Implemented capabilities

- **Host lifecycle:** Office initialization/readiness, standalone-browser fallback, bounded probing, Retry, safe failure states and no workbook reads merely from loading the pane.
- **Workbook adapter:** explicitly requested selected-range reading, typed cell snapshots, host identity, read limits, error/missing-value handling and sanitized diagnostics.
- **Method navigation:** two-level group/method dropdowns using generated shared metadata. All **12 groups / 76 documented methods** can be represented in the catalog; this is **not** a claim that all are runnable.
- **Typed analysis editors:** one-way ANOVA options (alpha, Welch and selected post-hoc tests) and Kruskal–Wallis options (alpha and Dunn). Other method entries remain unavailable until there are verified Common handlers and editors.
- **Narrow analysis pilot:** explicit range selection, grouped-data validation preview, ANOVA/Kruskal–Wallis execution via Common/Core, ordered result tables, current/new worksheet target inspection, occupied-output confirmation and guarded writes through the production Office.js writer.
- **Lifecycle protections:** source/selection/destination revalidation, document-operation identity, stale consent invalidation, structured rejection/fault codes and qualified partial-write outcomes. A native Excel mutation cannot be rolled back atomically or assumed cancellable after submission.

The Office host intentionally **does not** implement the complete Windows Ribbon method set, Office Custom Functions, full chart rendering/export, general-purpose model GUIs or a production deployment manifest. New workbook output is not supported by the current pilot. Wider reusable screens and final desktop/Mac acceptance remain development work.

## Important source files

| Area | Location |
| --- | --- |
| Task-pane navigation and analysis UI | `Components/AnalysisShell.razor`, `Components/AnalysisPilot.razor`, `Components/RangePicker.razor`, `Components/ValidationPreview.razor`, `Components/OutputPicker.razor` |
| Strongly typed editors | `Components/OptionEditors/` |
| State, catalog, analysis and workbook services | `Services/` |
| Office readiness and safe probe/fault infrastructure | `Infrastructure/` |
| Office JavaScript transport and writer | `wwwroot/js/officeHost.mjs`, `workbookAdapter.mjs`, `productionWriter.mjs` |
| Development manifest | `manifest-dev.xml` |
| C# component and JS adapter tests | `tests/` |

Historical rationale, detailed browser traces and acceptance matrices are under [`../docs/architecture/`](../docs/architecture/README.md); the README describes the current behavior rather than repeating investigative logs.

## Local development

Use **Visual Studio 2026** with the .NET 10 SDK/WebAssembly workload, **Node.js 20+**, a trusted local HTTPS certificate and a supported Excel test tenant/browser. From the repository root:

```powershell
dotnet build BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj -c Debug
dotnet run --project BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj --launch-profile https
```

The development host uses **https://localhost:7141/**. The checked-in `manifest-dev.xml` is sideloaded for a **disposable** workbook; it is **development-only** and may require tenant/admin configuration. Its minimum manifest requirement is not a guarantee that all pilot operations are available: the guarded writer requires additional Excel API/host capabilities and rejects otherwise. The application should display host readiness and Core/Common checks when opened in a standalone browser even if Office is unavailable.

Do **not** use the separate WasmSmoke manifest or its **7131** endpoint for this host. Build or reload the appropriate manifest after permission changes. No source-controlled file needs to point to a production hostname during ordinary local testing.

## User-operated verification of the current pilot

Use a disposable workbook with a control sheet. Start with a wide numeric grouped range with column headers. Choose **Parametric → One-Way ANOVA** or **Nonparametric → Kruskal–Wallis Test**, set supported options, and explicitly click **Use current selection**. Preview validation and then click **Run analysis**. Verify numerical statistics, optional post-hoc tables, issue/warning codes and preserved order against accepted Windows fixtures.

Choose a start cell (for example `J1`) on the current worksheet or choose a new worksheet name, then **Inspect output destination**. Test an empty destination, occupied destination with **No** and **Yes** confirmation, a name collision, sheet protection and a changed source/target before writing. Only an explicit accepted write may mutate workbook cells. Check result formats, table spacing and unchanged surrounding cells. Cancel/Retry/reopen and independent read diagnostics must not cause silent writes. A failed or uncertain write may require manual workbook inspection; do not assume transactional rollback.

Run the [WASM smoke harness](../BESHStat.WasmSmoke/README.md) separately to ensure its previous bridge still works. Genuine Mac/Windows desktop host checks are independent of Excel Online and should be reported explicitly, not inferred from simulated tests.

## Automated testing

The `tests/OfficeAddin.Tests.csproj` project tests actual linked C# services and Razor components; Node's built-in test runner covers Office JavaScript modules and writer behavior. Use the project verification script for host/build/asset consistency:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration Release
```

Or run the JS suite alone:

```powershell
cd BESHStat.OfficeAddin
npm test
```

For **opt-in simulated browser** validation, the repository also contains `tests/browser/readiness-browser.mjs`, `workbook-browser.mjs`, `options-browser.mjs` and `pilot-browser.mjs`. These use a locally running Debug host and an isolated Edge debugging session; see the architecture notes for their setup. Simulations validate wiring and cross-layer execution but **cannot certify a genuine Excel workbook, Office tenant, platform capability or Mac installation**.

The repository-wide verifier is [`../scripts/verify-beshstatng.ps1`](../scripts/verify-beshstatng.ps1). Re-run relevant suites in both Debug and Release and keep owner-operated workbook checks separate from automated pass counts.

## Next development priorities

Finish the owner acceptance of the guarded analysis/write pilot, then strengthen reusable form/selection/output components, broaden supported shared analysis workflows, implement portable chart specifications and host renderers, evaluate Office Custom Functions/runtime reuse, and prepare secure hosting/manifest/release procedures. Continue preserving the existing Windows Excel-DNA application as the fully supported host throughout this work.

## 04.04 amendment: capture after output

The reported `output.mutation-uncertain` below the range field comes from the
writer's fail-closed outcome latch. It is not evidence that the input sheet changed.
The initiating failure in the owner's genuine Excel Online write is still under
investigation; a populated new sheet alone cannot establish verified completion.
No unevidenced color/number-format normalization or forced latch reset was added.

After a verified Completed write, the pane retires the source/consent and old event
listeners so another range can be captured, acquired and analysed in the same pane.
Fresh source tokens and token-bound callbacks prevent stale events/consent from
affecting the next run. Timeouts retain the busy lease until native work settles.
Late Cancel cannot downgrade verified completion. The previous write outcome stays
visible while the current form is edited or initialization is retried.

After PartiallyWritten/`output.mutation-uncertain`, inspect the workbook manually
and wait for the previous native operation to finish. Do not retry that write.
Close and reopen the task pane before another operation; no rollback is available.
Capture remains blocked in that writer session, including after Retry/navigation.

For opt-in sanitized stage traces, use the task-pane DevTools frame:

```javascript
const writerTrace = await import('/js/productionWriter.mjs');
writerTrace.enableWriterTrace();
// Perform one explicit write, record its displayed outcome, then:
copy(JSON.stringify(writerTrace.getWriterTrace(), null, 2));
writerTrace.enableWriterTrace(false);
```

The memory-only trace is bounded to 256 entries and relative timings of 0–60,000 ms.
It records native mutation/readback stages, verification categories, lifecycle
events, result state/count and capture guards, without workbook data or native
exception details. See the [amendment report](../docs/architecture/batch-04-04-range-preview-destination-ui.md#22-october-amendment-capture-after-new-sheet-output)
for exact recovery, commands/results, limitations and genuine two-operation owner
acceptance steps. Automated Edge uses simulated Office objects; genuine Excel
writer acceptance remains pending. **36 / 223 batches accepted; 04.04 pending.**

## 23 October 04.04 production UX amendment

This section supersedes the acquisition/Run instructions above while preserving
the existing README content. **04.04 remains pending owner acceptance**, including
the independent genuine-Excel uncertain-write investigation; 04.05 is unstarted.

The default **Analysis** tab contains group/method navigation, typed options and
the workflow. Choose **Grouped data arrangement first**, then Header mode.
Columns/rows use one worksheet-qualified RefEdit range. Long format uses two
independent **Group/category (ID) range** and **Numeric values range** pickers;
nonadjacent columns are supported, with no integer column-index controls. The two
vectors must be distinct single columns in the same physical workbook/worksheet,
with exactly matching first/last rows. Category ID identifies a group, not a
repeated-measures subject. Manual qualified addresses, arm/Finish/Cancel and explicit
capture fallback remain available. Only one picker can own capture.

Click **Run analysis** directly: one bounded operation reads fresh sources,
validates with Common and runs the registered ANOVA/KW handler. **Validate input
(optional)** validates without statistics or output. The redacted preview appears
on request or failed Run; ordinary successful Runs show result readiness. Edits
clear stale issues. Locations use the actual category/value column and worksheet row.

**Reset form** keeps group/method and restores input, arrangement, headers, options
and output defaults, retaining any uncertain-write latch. **Diagnostics / Log**
contains Office/platform and Core/Common checks; development read controls and safe
stage summaries require Debug plus `?beshOfficeDiagnostics=1`. Opening it performs
no workbook read/write and preserves the live analysis. Tabs support keyboard and
screen-reader navigation. Retry stays above them and invalidates derived state
while preserving drafts; pane reopen starts fresh.

Save explicitly: choose an existing worksheet with a typed/picked qualified start
cell (including another sheet), or a new sheet name/start cell; **Inspect output
destination**, then **Write** or occupied-target **Yes/No**. Every write rechecks both
physical sources and fresh occupancy, consumes consent once and never retries
automatically. New Workbook remains disabled. Verified Completed permits a second
independent analysis in the same pane. Uncertain/partial output blocks work across
Reset, methods, tabs and Retry: inspect manually, wait for native settlement, never
retry that write, then close/reopen the pane.

Exact state/lifecycle contracts, automated evidence, complete file inventory and
still-required genuine Excel/control-sheet steps are in the
[production UX amendment report](../docs/architecture/batch-04-04-range-preview-destination-ui.md#23-october-production-ux-amendment-0404-remains-pending).

## Current 04.04 formatting, bounded input and Write workflow

This amendment supersedes the earlier equal-length long-input and mandatory Inspect
instructions above, preserving their history and owner-authored README content.
**04.04 remains pending genuine Excel Online acceptance (36/223; 187 outstanding).**

- Header/readiness and **Analysis | Diagnostics / Log** stay visible; only the
  selected panel scrolls. Tabs retain the draft, result, lease and scroll position.
- Qualified input may select entire columns, such as `'Hárok 1'!$F:$F` or
  `'Data'!$A:$C`. Capture/Run/optional Validate resolves the selected columns' used
  value/formula footprint before bounded reads. Original scope stays in the RefEdit
  field; a short note describes narrowing. Every selected column and interior blank
  stays in place. Zeros are data, errors remain errors, empty-string formulas remain
  text, and formatting-only blanks do not extend the footprint. Empty/oversized or
  unverified inputs reject; row-only/whole-sheet/multi-area/external inputs are unavailable.
- Long inputs use only matching **physical worksheet rows**. F1:F10/E1:E12 succeeds
  with blank extra rows. Ignored overhang is reported by role/count; populated extras
  get a prominent warning. None can intersect different starts; Required/Auto must
  start on the same header row. Original ranges, including ignored cells and whole-
  column scopes, remain protected by source revalidation and overlap safeguards.
- After Run, click **Write results** once. It inspects afresh, writes an empty safe
  destination, or pauses for occupied-target **No / Yes**. Advanced Inspect is optional.
  No makes no changes; verified source/results can remain for another explicit Write
  with new inspection/consent. No potentially submitted write is automatically retried.
- Table presentation now follows the existing Windows/WasmSmoke source conventions:
  Common roles/emphasis and effective p-value formats, Calibri Light, gray headers,
  left alignment, footer size and table borders; numeric significant p-values use
  the same green convention. Values stay numeric and Common significance is unchanged.
  Formatting/readback is confined to inspected table regions; autofit/column widths,
  charts and New Workbook remain unavailable. Native mismatches still latch uncertainty.

Settings/Options implementation remains **04.06**, using existing PreferenceSnapshot,
IPreferenceStore and PValuePresentationSettings (global alpha, seed, logging, digits,
small-value mode/upper bound and honest session-only fallback). Ribbon utility/method
inventory/parity remains **04.07**. No parallel settings schema or future statistical
handler is added: reusable grouped constraints demonstrate exactly-two-group and
matched-block input checks without enabling Mann–Whitney/Friedman.

The genuine native writer blocker is **not resolved by simulations**. Before/after
worksheet screenshots and a safe first-failing writerTrace stage remain owner checks.
After verified Completed, test a second independent analysis in the same pane; after
uncertainty, inspect manually, wait, never retry that write, then close/reopen.
See the [current source audit, policies, test evidence and owner checklist](../docs/architecture/batch-04-04-range-preview-destination-ui.md#23-october-owner-amendment-formatting-bounded-columns-and-reusable-workflows).

## Corrective 04.04 workflow: rerun options and numbered new sheets

This supersedes duplicate-name rejection/manual-renaming guidance above. After a
verified Completed write, change Welch/post-hoc options and click **Run analysis**
directly with the same references. Old results/inspection/consent retire; Run waits
for outstanding retirement, reads fresh input and applies the new options. Write
to the same existing cell obtains a new occupied **No / Yes** inspection/consent.
Options changes never write automatically. Browser/tab return may invalidate a
lease; another explicit Run resolves the retained physical references without reload.
Renamed/deleted/replaced sheets still require correcting or explicitly repicking the
reference. Whole-column reacquisition can refresh its used footprint on the same
physical sheet; an old result/consent cannot use that changed source.

For **New worksheet**, keep a requested base such as `ANOVAresults`. If free, that
exact name is used without a warning. If occupied (case-insensitively), Write picks
the first free positive suffix: base, base1 and base3 occupied gives **base2**.
The prefix shortens as needed to keep 31 characters; invalid/reserved names reject
instead of being repaired. The editable base never changes. The actual target name
appears separately in an accessible `output.sheet-name-adjusted` notice: “Will use”
before mutation and “Created” only after verified Completed. New-sheet output never
reuses another sheet and never requests occupied-cell overwrite consent. Inspection
creates nothing; Write submits one new sheet at the chosen start cell.

If a concurrent addition takes the allocated name before submission, the attempt
rejects safely; click Write again for a fresh allocation and inspection. There is
no automatic mutation retry. A conflict/fault after submission stays uncertain and
requires the existing manual recovery; a populated sheet alone does not prove Completed.

Names stay in the intended local output UI. The existing opt-in bounded memory
diagnostics and sanitized writer trace receive only `output.sheet-name-adjusted`,
without names, addresses or contents. **04.06 must forward this event to its scheduled
persistent logging feature**, with the same opt-in/redaction policy and fail-isolated
delivery. No persistent log store or Settings implementation is added in 04.04.

Use a disposable genuine Edge workbook to verify option rerun at J1 with fresh No/Yes,
base/base1/base3 → base2, another independent run with the unchanged base, browser
tab return, 31-character/case/invalid names and strict Completed readback. Stop after
uncertainty and collect a sanitized first-failing writerTrace; never bypass the latch.
See the [corrective evidence and owner checklist](../docs/architecture/batch-04-04-range-preview-destination-ui.md#23-october-corrective-amendment-rerun-numbered-worksheets-and-tab-return).
**04.04 remains pending: 36/223 accepted, 187 outstanding; 04.05 unstarted.**
