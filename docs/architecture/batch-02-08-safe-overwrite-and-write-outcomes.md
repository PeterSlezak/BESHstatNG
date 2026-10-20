# Batch 02.08 — Safe overwrite and structured write outcomes

Review candidate on `codex/02-08`, based on merged `main` / HEAD `4639df5` (accepted 02.07). **Pending independent review and owner acceptance; not complete.** The owner's accepted Excel Online C10 composite demonstration belongs to 02.07. The original implementation's automated-only evidence is retained below. The owner subsequently reported all 02.08 Excel Online live checks passed but Windows output failed for every destination. The Windows corrective investigation, native COM evidence and updated gate are recorded in the final section; they do not mark 02.08 accepted or claim Mac acceptance.

## Call graphs and scope

Before:

* Common: result → immutable document → semantic/layout plans → unchanged table wire v1 or separate composite wire v1.
* Office: `writeOutputPlan` → capability/target probes → `clear('Contents')` → values/styles → sync. Existing values and formulas were not inspected. Composite insertion accepted new sheets only and propagated late exceptions.
* Windows ANOVA/KW: validate destination → acquire writer (possibly create destination) → conservative `AreaCheck` envelope → existing Yes/No prompt → shared tables → supplementary tables → native charts. Exceptions could leave earlier output without a structured result.

After:

* Common: immutable `WriteIntent` + exact ordered `WriteRegion`s → `WriteInspection` → optional scoped `WriteConsent` → reinspection → ordered actions → `WriteResult`. `SafeWrite` is a synchronous, delegate-based gate, not a transaction framework.
* Office tables: strict entire-payload validation/snapshot → capability check → **read-only** destination/protection/merge/values/formulas/type probes → either write immediately to an empty destination or return `AwaitingConfirmation` → visible task-pane Overwrite / Cancel → same prepared plan, without another analysis run → single-use consent lookup → immediate read-only revalidation → exact clears/values/styles → final sync → structured result.
* Office composite: unchanged strict composite/table validation → read-only new-sheet name and structure checks → create sheet → six exact insertions → sync → structured result. **Current-sheet composite remains deliberately disabled**, in both Common projection and JS, before `Excel.run`. Its merging of annotation blocks remains confined to a newly created empty sheet. NewWorkbook remains unsupported; no destination substitution occurs.
* Windows ANOVA/KW: shared analysis and supplements → pure target/geometry validation → acquire destination → prepare immutable commands and exact primary/supplementary footprint → `InspectWrite` / `InspectWriteCore` → existing recognizable `Output range not empty! Overwrite?` prompt, once → reinspection → existing formatter boundary, supplementary cursor policy and native chart actions → Common outcome. Cancellation returns from the form without an error dialog. Other failures display a safe explanation. `LastSharedWriteOutcome` / writer `LastSharedWriteResult` provide internal structured results without changing public Excel-DNA entrypoints.

Nonshared analysis writers and the opt-in Windows composite recording/insertion seam retain their existing behavior. That composite seam is not connected to a production form or a live COM renderer. Its existing new-sheet projection restriction remains in force.

## Occupancy, geometry, APIs and quotas

All Common rectangles are one-based, inclusive and positive-area. A region with no bounds is nonspatial; `MayMutate` distinguishes a native chart addition from an unrendered logical artifact. The intent retains ordered identities and reservations, rejects duplicate identities, intersecting cell writes, reservation intersections and out-of-grid geometry, and copies mutable collections and target scalar fields. Envelopes are not write footprints. Separator rows are never cleared, styled, merged or assigned by the planned writers.

Office reads `values`, `formulas`, `valueTypes` and addresses for **each exact planned table rectangle**, alongside the merge probe. Numbers (including zero), text (including typed empty strings), Booleans, errors and formulas all count as occupied. In particular, `=""` counts even when its value is the empty string. Emptiness requires complete, supported observations with Empty type and empty values/formulas. Formatting alone does not require consent; output still changes formatting. Unsupported rich/unknown types, incomplete observations, missing APIs, probe errors or quota excess never become permission to overwrite. No worksheet used-range or enclosing-envelope scan is used by Office.

Office table limits remain 64 artifacts/ranges, 10,000 output cells and the accepted 64 MiB estimated plan budget. Occupancy adds a 64 MiB estimated values/formulas/type budget before creating a snapshot token. Observations stay in the bridge closure, never in returned diagnostics or logs. At most eight requests are retained, each expiring after five minutes. Tokens are single-use handles into this task-pane instance, not bearer permission independent of their stored plan/target/observation.

Current-sheet writes now require ExcelApi 1.13 for `getMergedAreasOrNullObject` and the worksheet protection API from 1.2. New-sheet writes require workbook structure protection inspection from 1.7. Missing or unverified protection properties reject safely. There is no unsafe fallback when a safety API is absent. An excessive merged-area probe can fail; it then prevents writing. The capability snapshot still describes the basic host operations; the stricter per-operation safety probes can reject an older host at execution time.

The API basis is Microsoft's [Range reference](https://learn.microsoft.com/en-us/javascript/api/excel/excel.range?view=office-js) and [ExcelApi 1.13 requirement set](https://learn.microsoft.com/en-us/javascript/api/requirement-sets/excel/excel-api-1-13-requirement-set?view=office-js). Mocks now also enforce that a newly added worksheet's name cannot be read until its `load` has crossed `context.sync()`. The adapter uses the requested resolved name for early failure diagnostics and the host-loaded name after success.

For the normal C6 fixture, the inspected/written cells are C6:E7, C9:D11, C13:F13 and C15:C16. Rows 8, 12 and 14 remain gaps. For composite C10, the exact regions are C10:H13, C15:H15, C17:H24, C26:H26, C28:H28 and C30:E31; their enclosing C10:H31 is not cleared. Composite current-sheet requests cannot reach any occupancy or write path.

Windows checks each shared table and every actual supplementary table rectangle. Supplementary placement starts two rows after the preceding table's final row, matching the historical cursor plus one blank separator. The old conservative envelope remains an additional initial destination/protection/merge guard, not a substitute for exact per-table validation. Bulk COM `Value2` plus `Formula` reads distinguish values and formulas. Native one-based matrices must have the exact expected dimensions; missing/malformed matrices and DBNull fail closed. ErrorWrapper native codes participate in the private fingerprint. The fingerprint also distinguishes scalar runtime types and uses invariant conversion.

**Historical chart policy, superseded by the false-prompt correction below:** native Windows charts initially used a conservative snapshot of pre-existing UsedRange cells as a chart guard. This incorrectly classified distant input data as occupied output whenever charts were requested. That guard has been removed. Native charts remain ordered non-cell mutations; exact table occupancy and existing-shape safety now have separate inspections and decisions. Windows cell reads are capped at 100,000 cells and 64 MiB, entirely within planned cell-write rectangles. Chart placement remains host-dependent; it is not presented as a verified cell footprint.

## Consent and revalidation

The Blazor task pane displays the actual sheet name, all planned rectangle addresses, occupied-region count, consequences, and **Overwrite** / **Cancel** buttons. The ordinary analysis task stays busy while awaiting a `TaskCompletionSource<bool>` completed by these buttons. No default approval, global preference, console flag or analysis rerun is used. Changing other UI controls cannot change the already prepared plan submitted with the decision.

The first occupied Office call is read-only. Cancel consumes its token and returns `CancelledByUser` without entering `Excel.run` again: no content, formula, format, merge, sheet-add, activation or selection change. Invalid, expired or replayed tokens reject. Approval must match the complete validated plan (including typed values/styles/destination/start), task-pane document context, worksheet ID and name, and exact observed values/formulas/types. The bridge re-resolves the active worksheet, reloads protection/merges and all exact rectangles immediately before the first write. Changed content or identity requires a fresh user-initiated inspect/confirmation attempt; it never auto-retries.

Windows consent is similarly tied to the immutable intent and a private SHA-256 observation identity. `SafeWrite` calls read-only inspection again after the modal prompt and compares state, token, occupied identities and formula/value flags before any output action. A declined prompt skips reinspection and all actions. The recording seam exercises the production inspection implementation with replacement native reads and target facts, without COM.

No host can lock out concurrent workbook edits between the last observation and execution. These checks narrow that race; they do not claim atomicity, cross-client isolation, undo or rollback. New Office worksheets are created only after read-only preflight. The Windows legacy destination acquisition still creates a requested new sheet/workbook before subsequent live occupancy inspection; failures can leave it behind. Its pure target/combined geometry checks happen before creation, and creation attempts are tracked as possible mutation. Existing-sheet cancellation never creates a destination just to inspect it.

Deprecated `writeResultTable(s)` names remain exported. Their current-sheet path now explicitly rejects with `output.legacy-current-unsupported` before `Excel.run`, so it cannot bypass consent; their old new-sheet signature remains available. Normal analysis uses `writeOutputPlan` only. Deprecated exports remain exception-based compatibility APIs; structured execution results belong to the planned writer entrypoints.

## Common outcome v1 and host projection

The additive Common contract is in `Output/WriteResult.vb`:

* `WriteIntent`: opaque full-payload plan identity and target execution identity, optional observable workbook/worksheet identities, requested destination/start/name, ordered immutable regions, reservations and additional inspection guards.
* `WriteInspection`: Empty / Occupied / Unknown / Rejected, opaque observation identity, bounded affected artifact IDs, values/formulas flags, stable code, safe explanation and confirmation requirement. Unknown/rejected observations cannot ask the user to waive missing verification.
* `WriteConsent`: explicit Boolean decision bound to the exact intent instance and observation; only known occupied observations can create it. Declined or stale consent cannot authorize execution.
* `WriteResult`: schemaVersion 1, state, stable code/message, intent, confirmed-written regions in original order, mutation-may-have-occurred, nullable confirmation-required/overwrite-approved, and three explicit false undo/rollback fields. Unknown consent information stays null; no prompt is represented by false/null. Invalid state/mutation/confirmation combinations and fabricated confirmed IDs/order fail construction.

| Execution state | Typical code | Mutation possible | Confirmed artifacts |
|---|---|---|---|
| Completed | `output.completed` | true for actual writes or destination creation | All observed successful mutation actions |
| CancelledByUser | `output.cancelled` | false | None |
| RejectedBeforeWrite | `output.rejected`, `output.occupancy-unverified`, `output.consent-stale`, existing target/capability codes | false | None |
| FailedBeforeWrite | `output.host-failed` | false | None |
| PartiallyWritten | `output.mutation-uncertain` (or a specific failure/cancellation code after tracked destination creation) | true | Only independently observed successes, if any |

Office projects the same semantics into a JSON-safe object with string states, `planned` / `confirmedWritten` IDs and rectangles, host addresses, sheet name and existing `address` / `tableAddresses` / p-value fields on successful table writes. `AwaitingConfirmation` / `output.confirmation-required` is an **intermediate interaction response**, not a completed execution outcome. Its opaque `confirmation` object contains no workbook values/formulas. Decline returns a final cancelled result. Native exception text is not copied into execution diagnostics; bounded native error codes can be retained. Locally authored safe target explanations remain actionable.

Examples (abridged host projection):

```json
{"state":"Completed","code":"output.completed","confirmationRequired":true,"overwriteApproved":true,"mutationMayHaveOccurred":true,"confirmedWritten":[{"artifactId":"table-0","address":"Sheet1!D20:I23"}],"undoSupported":false}
{"state":"CancelledByUser","code":"output.cancelled","confirmationRequired":true,"overwriteApproved":false,"mutationMayHaveOccurred":false,"confirmedWritten":[],"rollbackAttempted":false}
{"state":"RejectedBeforeWrite","code":"output.consent-stale","mutationMayHaveOccurred":false,"confirmedWritten":[],"rollbackSucceeded":false}
{"state":"PartiallyWritten","code":"output.mutation-uncertain","mutationMayHaveOccurred":true,"confirmedWritten":[],"undoSupported":false,"rollbackAttempted":false,"rollbackSucceeded":false}
```

Common/Windows can confirm individual synchronous actions after they return. A COM failure during a block may have changed values before formatting failed: the failing block is not confirmed, earlier completed actions remain confirmed. Office queues several operations and syncs once after writing. A failed setter/sync after a clear or add was queued is conservatively partial, with **no fabricated committed artifact set**. No automatic retry, delete-created-sheet cleanup, transaction or undo command occurs.

The accepted AnalysisResultDocument v1, five analysis goldens, table/style/p-value contracts, Office table plan v1 and composite plan v1 are unchanged. The single composite projection edit clarifies that current-sheet insertion stays disabled. **Gate B handoff:** review and agree on these new immutable contracts before freezing the Common v1 envelope after acceptance of 02.08. Existing envelopes permit additive/versioned extensions only; this implementation does not itself approve the freeze or advance the next batch.

## Automated evidence

Focused verification commands (run inside this checkout; logs under ignored `TestResults/Codex/Batch0208-*.log`):

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj --no-restore --filter 'FullyQualifiedName~WriteResultTests|FullyQualifiedName~CompositeOutputTests|FullyQualifiedName~OfficeOutputPlanAdapterTests|FullyQualifiedName~OutputLayoutPlannerTests|FullyQualifiedName~OutputDocumentTests|FullyQualifiedName~OutputTargetPlannerTests' --verbosity quiet
node --test BESHStat.WasmSmoke/tests/outputPlanWriter.test.cjs BESHStat.WasmSmoke/tests/compositeOutputWriter.test.cjs BESHStat.WasmSmoke/tests/safeOverwrite.test.cjs BESHStat.WasmSmoke/tests/hostCapabilities.test.cjs
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' BESHStatNG.Test/BESHStatNG.Tests.vbproj /t:Build /p:Configuration=Debug '/p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' /m /v:minimal
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' BESHStatNG.Test/bin/Debug/BESHStatNG.Test.dll '/TestCaseFilter:FullyQualifiedName~SharedOutputPlanTests|FullyQualifiedName~CompositeOutputPlanTests'
```

Focused results: **210 Common**, **251 Node**, **95 Windows**, all passed. New cases: **32 Common**, **58 Node** (57 safe-overwrite cases and one compatibility-export test), **23 Windows**. Existing invalid-input tests now assert structured rejection/partial results while retaining their no-mutation, geometry, type/style and ordering assertions; unchanged numerical/wire fixtures remain authoritative. An error after queued writes is now asserted partial instead of asserting an escaping exception.

Common tests cover defensive copies, bounded/invalid construction, exact C6 and six-region C10 snapshots, nonspatial/zero-area actions, reservations/gaps, all outcomes, unknown inspection, scoped decisions, revalidation, safe failure messages, confirmation metadata and serialized outcome fields. Existing five analysis/result fixtures, 02.02 semantics, C6 table placements and C10 composite fixtures still run in the full suite.

Node tests cover values/text/errors/Booleans, `=""`, literal empty text, format-only emptiness, second-table occupancy, gap/outside sentinels, zero-mutation Cancel, exact approved writes, worksheet/plan/style/content/context changes, expired/replayed/missing consent, capability/protection/merge/probe/byte-quota failures, failure after add/clear/write/sync, six-region composite compatibility, inactive links/placeholders and task-pane button wiring. Mocks separate worksheet data and preserve load/sync boundaries. No production chart renderer or active hyperlink was added.

Windows recording tests cover current/new-sheet/new-workbook destinations, exact primary and supplementary bounds, once-only prompt, acceptance/decline, cursor and formatting/AutoFit parity, block/supplement failures, actual native inspection scalar/formula/error handling, native matrix validation and pre-read quota rejection. The original broad chart-guard expectation is explicitly superseded by the regression tests below. Unit tests do not invoke COM or alter a workbook.

The required full command is:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Python -PythonExe "C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe" -Js -Wasm -Legacy -VisualStudio 2026
```

The separate solution gate is:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:Platform=Any CPU' '/p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' /m /v:minimal
```

The final full command **passed (exit 0)** on the final source: all suites below passed with no failures or skips, Python catalog/generated-metadata/retired-helper checks passed, and Debug WASM built with zero warnings/errors. The separate VS 2026 full solution Debug build **passed (exit 0)** and produced both `BESHStatNG-AddIn-packed.xll` (x86) and `BESHStatNG-AddIn64-packed.xll` (x64). Final logs: `TestResults/Codex/Batch0208-full-gate-final.log` and `Batch0208-solution-build.log`; focused logs use `Batch0208-focused-{common,js,windows}.log`. TRX reports are `TestResults/Codex/{Core,Common,Legacy}.trx`. No live host result is inferred from these results.

| Suite | Accepted 02.07 | 02.08 review candidate | Added |
|---|---:|---:|---:|
| Core | 1,024 | 1,024 | 0 |
| Common | 481 | 513 | 32 |
| Windows Framework | 633 | 656 | 23 |
| Office.js Node | 260 | 318 | 58 |
| Python (22 + 5 + 5) | 32 | 32 | 0 |
| Total | 2,430 | **2,543** | **113** |

The known nonfatal `UTA001 MatrixTestHelpers` discovery advisory is preserved. TRX overwrite notices refer to test reports, not workbook content. Implementation-stage VB keyword/name-resolution and test fixture issues were corrected without weakening older statistical assertions. A final API review corrected early access to a newly added worksheet's unloaded name and made both independent workbook mocks enforce that boundary.

## Manual acceptance — owner execution only, disposable workbooks

This was the original implementation's owner checklist, before the Windows corrective investigation below. The owner has since reported the Excel Online checks passed. Use the accepted ANOVA/KW input samples for the remaining owner-facing Windows acceptance.

1. Office current sheet at D20, empty planned cells: run once, confirm Completed, actual address, numeric p-values and existing formats.
2. Seed a first-table text/formula plus distinctive formatting. Record sheet count and selection. Run again; the task-pane prompt must identify the actual sheet and rectangles. Cancel: verify cells, formulas, formats, merges, sheet count and selection unchanged, and CancelledByUser without PASS. Repeat with Overwrite; only exact tables change.
3. Repeat with a second-table sentinel, then a separator-only sentinel. The second table requires consent; the separator alone does not, and remains unchanged. A destination formula `=""` must still prompt; Cancel preserves it.
4. While confirmation is open, change an inspected value/formula, active worksheet or protection. Overwrite must reject as stale/unsafe without writing. Start a fresh attempt for fresh consent; no auto-retry.
5. New sheet C10: no occupancy prompt, first ANOVA rectangle C10:H13, collision-safe naming, correct literal text and styles. NewWorkbook rejects. Merged/protected current targets reject before output.
6. Synthetic composite demo: verify all six addresses above, including C30:E31, literal `=1+1`, visible chart placeholder and inactive HTTPS link. Current-sheet composite is disabled and must reject before acquisition. Do not attempt to render a chart or activate a link through this demo.
7. Review mocked late-failure tests/logs to distinguish partial/mutation-uncertain from cancelled. Do not deliberately damage a live workbook to provoke an error.
8. Windows ANOVA and KW current-sheet blank/occupied destinations: check one prompt where occupied, No gives no output or error dialog, Yes preserves all formats/p-values, AutoFit=False, non-A1 position, cursor and order. Include supplementary tables and plots. Chart options can cause the documented conservative prompt for pre-existing data elsewhere on the sheet.
9. Windows new sheet and actual new workbook: verify primary/supplementary tables and native chart order/positions. Invalid/protected/merged targets reject before prompting/writing; a failed destination creation can honestly leave a new destination. No rollback is claimed.
10. Mac runtime acceptance remains separate. If available, record its Office.js results independently; Windows/Online mocks do not certify Mac.

## Changed paths, preservation and checkpoint

Implementation/test paths:

* `BESHStat.Excel.Common/src/Output/WriteResult.vb` (new).
* `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb` (restriction explanation only).
* `BESHStat.Excel.Common.Tests/src/WriteResultTests.vb` (new).
* `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js`.
* `BESHStat.WasmSmoke/Pages/Home.razor`.
* `BESHStat.WasmSmoke/tests/safeOverwrite.test.cjs` (new).
* `BESHStat.WasmSmoke/tests/outputPlanWriter.test.cjs`.
* `BESHStat.WasmSmoke/tests/compositeOutputWriter.test.cjs`.
* `BESHStat.WasmSmoke/tests/hostCapabilities.test.cjs`.
* `BESHStat.WasmSmoke/tests/helpers/recordingWorkbook.cjs`.
* `BESHStat.WasmSmoke/tests/helpers/compositeWorkbook.cjs`.
* `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.
* `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`.
* `BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb`.
* `BESHstatNG/src/UI/UibyID.vb`.
* `BESHStatNG.Test/src/DataManagement/SharedOutputPlanTests.vb`.

Documentation: this file and minimal active checkpoint / 02.07 / 02.08 status edits in `planning/BESHStatNG_CrossPlatform_MasterPlan.md`. The existing owner edit marking the 02.07 row complete was preserved. Historical checkpoint prose and the original safety handoff remain, with a new active handoff appended.

Build-generated `BESHstatNG/src/Help/HelpContext.vb` and `HelpLinks.vb` timestamp comments are retained, not discarded. Generated `BESHstatNG/Generated/VersionAttributes.vb` remains preserved; Git can report CRLF normalization without a normalized content difference. No authored help/method metadata was regenerated manually. Existing file encodings, including the UI BOM, are preserved. Both historical stashes remain untouched.

The plan stays **222 total / 26 accepted-completed / 196 outstanding**, 02.07 accepted/merged and 02.08 in progress / pending independent review. Phase placement remains **04.07 → 04A.01–04A.74 → dependent Phase 05+**; all 74 rows, dependencies and the separate Windows portability-audit file are preserved. Gate B is a review handoff, not automatic acceptance.

No Core numerics, histogram binning, p-value decisions, framework targets, packages, Ribbon/UDF API, signing keys, installer, production Office UI, native chart engine, transaction/rollback system, active hyperlinks, Phase 04A or 02.09+ implementation is included. No commit, push, merge, branch switch, stash mutation, deployment or installer was performed. The original implementation ran no live Excel workflow; the later explicitly authorized corrective native checks are distinguished below.

Suggested commit message after owner review: `feat(output): implement batch 02.08 safe overwrite consent and write outcomes`.

## Windows regression correction — 20 October 2026

The owner's Windows reproduction failed for Current/Output Range F11, New Worksheet and New Workbook, while the 2,543-test gate and Excel Online acceptance had passed. The former `RecordSharedPreparationFailure` preserved a safe modal but discarded the exception from developer diagnostics. Its modal text alone could not identify the failing operation. This correction remains part of **02.08, pending owner acceptance**; the master plan/audit and Gate B status were not advanced or edited.

### Exact failure and evidence

An instrumented, isolated Excel 16.0 COM session reproduced the same failure for all three destinations using the real form's destination/projection methods and the resolved Windows adapter. The logical precreation intent and shared request projection both succeeded. The first failing adapter call was the **resolved intent construction**, whose `worksheetIdentity` was supplied from `Worksheet.CodeName`.

Native Excel returned **CodeName length 0** on the current and newly created sheets, while workbook and worksheet names were nonempty. CodeName is a VBA-facing property and is not a reliable required physical identity before that project is initialized. The new Common validation correctly rejected an empty supplied identity; the Windows adapter supplied an inappropriate identity.

Sanitized observed exception/stack from the pre-correction native probe (line numbers refer to that instrumented source):

```text
System.ArgumentException: Invalid bounded output identifier/message.
  BESHStatNG.ExcelCommon.WriteIntent.CheckedText(String value, Int32 maximum)
  BESHStatNG.ExcelCommon.WriteIntent..ctor(..., String workbookIdentity, String worksheetIdentity)
  BESHStatNG.ExcelDnaSharedOutputPlanAdapter.CreateWriteIntent(OutputLayoutPlan, ...) : line 245
  BESHStatNG.ExcelDnaSharedOutputPlanAdapter.CreateWriteIntent(ExcelDnaPreparedSharedOutput, ...) : line 215
```

The former production caller was `UibyID.ExecuteSharedOutput` at the intent-construction step. `SafeWrite.Execute` had not yet been entered, and no table action had run. This explains a safe prewrite failure on the existing sheet and a potentially remaining blank destination after new-sheet/book creation. The early logical sheet-name fallback (`BESHStat_Result`) was **not** the exception source. Neither a native array-shape mismatch nor action-count mismatch was observed as the root cause.

The original trace and identity-length-only diagnostics are retained under ignored `TestResults/Codex/Batch0208-correction-native-before-fix.log`. No workbook cell contents, formula bodies or fingerprint bytes were logged as diagnostics.

### Targeted correction

* `ExcelDnaOutputTargetAdapter.TargetIdentity` now derives an opaque identity from the actual workbook/worksheet COM `IUnknown` identity, using HMAC-SHA256 with a process-local nonce. It releases the temporary COM reference in `Finally`. Native pointers, cell data and VBA access are not exposed or required. Creation and reinspection use the same adapter identity function on Excel's caller apartment. A renamed/replaced worksheet is still checked against the intended physical object and observed sheet name.
* `InspectWrite` checks these physical identities instead of requiring workbook Name / worksheet CodeName as identity. Protection, writability, merged-cell checks, exact rectangle reads, formula detection, quotas and revalidation remain enforced. Common v1 constructors/public surface and all Office.js code are unchanged by this correction.
* The two form routes now invoke the same Windows-only `ExcelDnaSharedOutputRun` orchestration in the existing shared adapter file. Its host seam replaces only native acquisition/identity/reads, chart callbacks and user interaction in tests. Production and tests share real result projection, command preparation, intent construction, action creation, inspection/consent/revalidation and execution. This closes the prior gap where tests started after the failing form orchestration.
* The redundant full `pendingSharedIntent` before destination resolution was removed even though it was not the root cause. The existing pure capability/target/combined-geometry preflight still runs before creation. An unresolved preparation failure uses a minimal safe requested-target scope; a full immutable write intent is constructed after acquiring the actual destination.
* The shared path retains the workbook and worksheet objects it validated. Current-sheet workbook identity comes from the resolved worksheet's parent. New Worksheet uses the captured workbook and the actual sheet returned by `Worksheets.Add`; New Workbook uses the actual book returned by `Workbooks.Add` and that book's worksheet. It does not re-read ActiveWorkbook after creation. The nonshared legacy `GetResultWriter` path is unchanged.
* `CreationAttempted` is recorded immediately before Add, and `DestinationCreated` only after Add returns the new object. Failures before either are prewrite; an exception during Add is conservatively mutation-uncertain; a confirmed new destination can be reported as potentially remaining. No actual result write or rollback is claimed on creation failure.
* `CoreServices.Logger.Error(originalException, stageContext)` now records the original exception/stack in the established local diagnostic log, with stage and creation-attempt/confirmation flags. Stages include target resolution, precreation plan, capability, target facts, preflight, destination creation, shared projection, chart intents, intent construction, action validation, occupancy inspection/revalidation, consent and execution. Native/delegate exceptions that Common intentionally converts to safe outcomes are logged before rethrowing into that gate. User-facing outcome/modal text remains generic and contains no original exception or workbook data.
* Common `SafeWrite.Execute` preconditions do occur before its internal Try. The Windows orchestration validates the command/action count and invokes the gate inside its own logged Try. Each actual table, supplementary table and native noncell chart action retains its corresponding ordered region. A failing action is not falsely confirmed written.

### Native shapes and full orchestration tests

The disposable native probe observed these actual Excel returns:

| Read | Native CLR shape |
|---|---|
| Blank multi-cell Value2 | `System.Object[,]`, rank 2, lower bounds 1/1, exact requested dimensions |
| Blank multi-cell Formula | `System.Object[,]`, rank 2, lower bounds 1/1, exact requested dimensions |
| Blank single-cell Value2 | `Nothing` |
| Blank single-cell Formula | `System.String` |

The existing reader already handles these shapes. No shape validation was relaxed. Scalar responses remain permitted only for a one-cell rectangle; malformed/incomplete multi-cell reads, wrong lower bounds, DBNull and unknown observations fail closed. A formula `=""` remains occupied.

**34 additional Windows tests** exercise the production orchestration from actual `SpreadsheetAnalysisResult` fixtures: all five ANOVA/Welch/posthoc/KW/Dunn goldens across Current F11, New Worksheet and New Workbook; blank output reaching the first real recording-writer action and Completed; F11 Yes/No; second supplementary occupancy with a blank-result formula; target changes during consent; original inspection exceptions; null action validation; ordered supplementary and native chart actions including late chart failure; and creation failure before/during/after confirmed Add. Native scalar/matrix variants are explicitly covered. Decline has no writer content/format/AutoFit action and preserves the row cursor. Existing tests remain intact.

Focused command (after the VS 2026 legacy build shown above):

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' BESHStatNG.Test/bin/Debug/BESHStatNG.Test.dll '/TestCaseFilter:FullyQualifiedName~SharedOutputPlanTests|FullyQualifiedName~CompositeOutputPlanTests'
```

Result: **129 passed**, zero failures, up from 95 focused tests. The full corrective gate also includes the added action-validation assertion. No unit test uses live COM.

### Corrective build gate and packed binary verification

The exact required gate and the separate full solution Debug command documented above were run again. Both exited **0**. Final corrective counts:

| Suite | Original 02.08 candidate | Corrected candidate | Added in correction |
|---|---:|---:|---:|
| Core | 1,024 | 1,024 | 0 |
| Common | 513 | 513 | 0 |
| Windows Framework | 656 | **690** | **34** |
| Office.js Node | 318 | 318 | 0 |
| Python | 32 | 32 | 0 |
| Total | 2,543 | **2,577** | **34** |

There are no failures/skips. WASM/legacy Debug builds passed; x86 and x64 packed XLLs were regenerated. `UTA001 MatrixTestHelpers` remains the known nonfatal discovery advisory. Logs are `TestResults/Codex/Batch0208-correction-{build,focused,full-gate,solution-build}.log`.

Both packed XLLs were inspected **as PE resource data without executing them**. Their `ASSEMBLY_LZMA / BESHSTATNG` resources were decompressed with the already installed Excel-DNA packer's decoder and compared with `BESHstatNG/bin/Debug/BESHStatNG.dll`. Both embedded assembly SHA-256 values matched the corrected DLL:

```text
8000561ACA4D57B915CFFD4AE740D83982D11240C548BC0E4029B63050436349
```

This verifies the corrected code was packed, rather than relying on an old XLL timestamp or build assertion. Evidence: `TestResults/Codex/Batch0208-correction-packed-verification.log`; local reproduction helper: `Batch0208-correction-verify-packed.ps1` in that ignored directory. The hash records this build and will change if generated version/timestamp inputs change on a later build.

### Authorized native smoke evidence and remaining owner acceptance

After the final full build, a **fresh isolated Excel 16.0 process** registered the rebuilt `BESHStatNG-AddIn64-packed.xll` successfully. No stale existing Excel process/add-in was reused. The real form's `RunSharedOutput` and its native adapters/legacy formatter were invoked on the accepted three synthetic groups. Each case used a fresh disposable source workbook, and all probe-owned workbooks were closed without saving. The probe was explicitly authorized; no installer or normal user workbook was used.

The following **14 native output cases passed**:

* ANOVA Current F11, New Worksheet, New Workbook: Completed, one primary table confirmed in each.
* KW+Dunn Current F11, New Worksheet, New Workbook: Completed, two primary tables confirmed in each.
* For each analysis, a literal sentinel at F11 and a `=""` formula at the **exact second supplementary table** were each tested with No and Yes. The location was asserted against the returned intent. The actual recognizable Windows Yes/No dialog was driven by clicking the matching button on the probe process's own dialog, not by bypassing consent.
* Each occupied case prompted exactly once. No returned CancelledByUser, left the checked formula/content, sentinel fill/font, selection, worksheet count and native shape count unchanged, and wrote no primary output. Yes completed the primary/supplementary output and added the requested native box plot. Failed/partial results were not silently accepted by the probe.

Native evidence and the local disposable helper are `TestResults/Codex/Batch0208-correction-native-after-fix.log` and `Batch0208-correction-probe.ps1`. An intermediate probe reused a hidden workbook activation context after creating a new book; hidden Excel kept the other book active and returned `DISP_E_BADINDEX` during reference lookup. That probe setup issue was corrected by using independent disposable source workbooks with explicitly set references; it is not presented as an observed product regression or as successful acceptance evidence.

These checks cover the affected native **output orchestration**, with the rebuilt x64 XLL loaded and the matching managed DLL's form methods under automation. They are not a claim that the complete interactive Ribbon → Group by Column → whole-column input acquisition → Compute workflow was automated inside the Excel-DNA process. Owner acceptance remains required:

1. Close/reload the newly rebuilt XLL in the owner's normal Excel profile, avoiding an old copied XLL or Excel instance. Repeat the exact ANOVA `'anova-stage304'!$A:$C` → F11 Group by Column reproduction and all three destinations through the actual Ribbon/dialog.
2. Repeat KW current/new destinations and No/Yes in the normal form. Inspect numeric p-values, highlighting, AutoFit=False, non-A1 placement, supplementary output, native box/posthoc chart positions and output order visually. Automated native tests counted/created box plots; they did not certify visual rendering or every optional posthoc chart.
3. Record any remaining local exception with its stage from the established `Logs/err.log` adjacent to the loaded XLL. Do not copy workbook cell data/formula bodies into public diagnostics.
4. No x86 Excel runtime or Mac runtime was exercised. Both x86/x64 packs contain the corrected DLL, and the x64 pack loaded in fresh Excel. The owner's previously passed Office Online acceptance is reported separately; this correction leaves the Office source and 318-test Node suite unchanged.

### Corrective paths and preservation

Only these five authored paths were changed for this corrective task:

* `BESHstatNG/src/UI/UibyID.vb` — shared form orchestration, exact destination acquisition, creation tracking.
* `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb` — shared Windows orchestration/host recording seam and stage/error diagnostics.
* `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb` — opaque COM identity in creation/revalidation; strict native occupancy reader preserved.
* `BESHStatNG.Test/src/DataManagement/SharedOutputPlanTests.vb` — 34 regression tests and additional action-validation assertion.
* This architecture note — root cause, evidence, correction and acceptance limits.

Build-generated help timestamp comments remain preserved. The pre-existing Batch 02.08 changes, both historical stashes, generated version files, authored help metadata, accepted histogram binning and all framework targets remain preserved. The master plan is unchanged by this correction: **222 total / 26 accepted / 196 outstanding**, 02.08 pending owner acceptance. The separate Phase 04A audit/74-batch ledger remains unchanged. No Gate B/02.09 work, branch creation/switch, commit, push, merge, deployment or installer was performed.

## Windows false overwrite prompt correction — 20 October 2026

**Active checkpoint: 02.08 remains pending review and acceptance.** This section supersedes the broad UsedRange chart policy and the earlier limitation that whole-column form input/Compute had not been automated. The previous blank-CodeName correction and its opaque COM identity remain intact. Office.js source and its owner-reported live acceptance are unchanged.

### Verified cause and before/after flow

This regression is a wrong occupancy decision, not a new exception. `UibyID.RunSharedOutput` supplied the entire `writer.ws.UsedRange` through `host.Guards` whenever there were chart callbacks. `ExcelDnaSharedOutputRun.Execute` attached it to the write intent. `InspectWriteCore` read that guard and assigned occupied non-cell `native-chart-*` identities when it encountered any input data. Default ANOVA box/posthoc options made this path common. Thus A1:C4 input incorrectly authorized the wording “Output range not empty” for empty output beginning at F11. The preceding recording host had supplied guard geometry but left its contents empty, missing the real condition.

Before: exact table reads + whole UsedRange chart-cell read → input data marks chart artifact occupied → ordinary cell-overwrite prompt.

After: exact primary/supplementary table reads → separate bounded existing-shape inspection → optional **Add charts?** decision → optional actual cell-overwrite decision → revalidate exact cells, physical target/protection/merges and existing shapes → original ordered tables, supplements, charts → truthful outcome. No sheet-wide cell occupancy query, creation probe or chart renderer/default change is used.

Four new fixture/orchestration cases were first run against the old code. ANOVA-all-posthoc and KW+Dunn each passed with charts off and failed with charts on: expected Completed, actual CancelledByUser, because the fake A1:C4 guard now contained numeric input. This isolates the chart guard from scalar-blank classification. Native scalar/matrix readers, including formulas returning blank, were not relaxed.

### Separate chart policy and diagnostics

`BoxPlot.AddBoxPlot` calls `Shapes.AddChart` without setting Left/Top; `MultipleComparisonPlotExcel.AddPlot` adds a new chart and sets point coordinates calculated later from the final writer cursor. Both modify the newly added chart, not existing shapes or worksheet cells. Box-plot placement cannot be predicted accurately through read-only preflight. It therefore has no invented exact collision rectangle.

* Only positive `WriteRegion.Bounds` are read for cell occupancy. Every primary and supplementary rectangle is validated and fingerprinted, including numbers/text/errors/Booleans and `=""` formulas. Gaps, distant data and formatting-expanded UsedRange are excluded. Obsolete Windows cell guards now reject before any read rather than being silently ignored; the Common guard contract itself is unchanged.
* Chart additions retain their ordered non-cell region/action identities. When there are no existing shapes, append-only chart creation proceeds without a confirmation. This says nothing about whether Excel's chart position will visually cover cells; the chart does not erase their contents.
* With existing shapes, a separate **Add charts?** Yes/No dialog names their count and explains that new charts may visually overlap because placement cannot be checked in advance. It asks whether to continue adding charts and writing output. It does not say output cells are occupied. This is a conservative decision for any existing shape, not a claim to have detected a precise collision. Actual cell overwrite, if also needed, still requires the original separate Yes/No prompt.
* Before either prompt, exact cell/protection/merge checks must pass. Shape inspection checks the same opaque workbook/worksheet identity and name, read-only/content/drawing-object protection, and at most **1,000 shapes**. An uninspectable inventory or quota failure fails closed. No existing shape is selected, moved, deleted or recreated.
* The private shape fingerprint includes ordered opaque COM identities and finite Left/Top/Width/Height observations, plus count. It excludes shape names, text and formula bodies. After user interaction, exact cell inspection and the same shape inventory/protection checks run again before the first action. Changed cells or shapes reject stale approval. Concurrent edits after the final read remain possible; there is no atomicity or rollback guarantee.
* Developer Debug logs distinguish `role=cell-write` (artifact ID, safe rectangle coordinates, occupied Boolean) from `role=append-only-shape` (inspection/revalidation stage, planned chart count, existing shape count, host-dependent/unverified placement). No cell contents, formula bodies, shape text, COM pointers or fingerprint bytes are logged. Exceptions still go to the existing stage-aware developer error logger.

No preserves content, formatting and shapes. Chart cancellation reports `output.chart-cancelled` / CancelledByUser on a read-only current-sheet path. If destination creation was attempted earlier, it conservatively reports PartiallyWritten and explains that the created destination may remain. Successful chart-overlap approval is reported as `output.completed.chart-overlap-approved`, with an explicit unverified-placement explanation. Completed results retain Common's cell-overwrite confirmation fields; they do not reinterpret shape approval as occupied-cell approval. Stale observations use the existing changed-destination rejection. Precreation read-only failures remain FailedBeforeWrite/RejectedBeforeWrite; no undo or rollback is claimed.

### Automated validation on corrected source

Focused commands:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' BESHStatNG.Test/BESHStatNG.Tests.vbproj /t:Build /p:Configuration=Debug '/p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' /m /v:minimal
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' BESHStatNG.Test/bin/Debug/BESHStatNG.Test.dll '/TestCaseFilter:FullyQualifiedName~SharedOutputPlanTests|FullyQualifiedName~CompositeOutputPlanTests' '/Logger:trx;LogFileName=Batch0208-chart-focused.trx' '/ResultsDirectory:TestResults/Codex'
```

**158/158 focused tests passed.** There are **29 additional Windows cases**, raising the Windows suite from 690 to 719. They exercise real ANOVA/KW result projection, supplementary geometry, action construction and production occupancy orchestration: distant A1:C4 input with charts on/off; empty F11; first/later/supplementary blank formulas with No/Yes; unchanged noncell action order; separate existing-shape No/Yes; unreadable shape inventory before/after creation; stale cell/shape approval; all destinations; unchanged AutoFit=False and CodeName-independent identity. Existing scalar-blank, native one-based matrix, strict malformed-read, protection/merge, partial failure and compatibility tests remain.

One previous test deliberately encoded the withdrawn policy: `NativeOccupancySeam_ChartGuardIsAdditionalAndNeverAClearEnvelope` expected distant guard contents to occupy a chart ID. It is replaced by `NativeOccupancySeam_RejectsObsoleteBroadChartCellGuards`, which requires rejection without reads. A separate new test proves an ordinary noncell chart adds no cell reads or occupied IDs. No numerical or wire fixture was changed.

The required full gate and the separate full solution Debug build both **passed, exit 0**:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-beshstatng.ps1 -Python -PythonExe "C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe" -Js -Wasm -Legacy -VisualStudio 2026
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\BESHStatNG.sln /t:Build /p:Configuration=Debug '/p:Platform=Any CPU' '/p:PythonExe=C:\Users\peter\Desktop\WPy64-31150\python-3.11.5.amd64\python.exe' /m /v:minimal
```

| Suite | Passing |
|---|---:|
| Core | 1,024 |
| Common | 513 |
| Windows Framework | 719 |
| Office.js Node | 318 |
| Python | 32 (22 + 5 + 5) |
| **Total** | **2,606** |

Logs are under ignored `TestResults/Codex`: `Batch0208-chart-focused.log` / `.trx`, `Batch0208-chart-full-gate-clean.log`, and `Batch0208-chart-solution-build.log`. An initial outer PowerShell `2>&1` capture returned status 1 after all tests completed successfully because Python unittest progress on stderr became NativeCommandError records. The identical full gate was rerun with shell-level stream capture; it exited 0 with no suite failures. This capture issue is not presented as a product failure. The established nonfatal **UTA001 MatrixTestHelpers** advisory remains.

Both packed Debug XLLs were inspected as resources after the final gate; their decompressed embedded `BESHSTATNG` assemblies match the built DLL byte-for-byte (SHA-256 `73F60AD8A1B4727C8227C6B43FAFF56BFEC146C9A2108D85F66324672EE063AF`). The existing local helper `Batch0208-correction-verify-packed.ps1` checks both x86 and x64; it does not execute the x86 binary.

### Fresh native Excel evidence and owner checklist

**24/24 native cases passed** in a fresh disposable Excel 16.0 instance after registering the rebuilt x64 packed XLL. The matching managed form was driven with its actual Group by Column controls, whole-column `'anova-stage304'!$A:$C` reference and private Compute event handler. Unlike the previous native probe, this invokes the input acquisition and ANOVA/KW form paths as well as output orchestration. It is still an external automation probe with COM, not a click on the installed Ribbon callback inside the owner's Excel process.

Each case used a newly created unsaved workbook containing the accepted A1:C4 sample, plus a distant AZ200 sentinel. For **each** analysis:

1. Empty output, charts/default ANOVA posthocs on: current F11, new worksheet, new workbook all Completed with zero cell/shape prompts. Default output contained five ANOVA charts or one KW box plot.
2. Box/posthoc options off: all three destinations Completed with zero prompts and no charts.
3. Literal F11 sentinel and `=""` at the exact last supplementary rectangle: each No/Yes choice produced exactly one cell prompt and zero shape prompts. No returned CancelledByUser and preserved checked content/formula, fill/font, selection, worksheet count and shape count; no primary output was written. Yes completed every ordered artifact and produced the expected charts.
4. A real large existing shape overlapping the subsequently added box plot: each No/Yes choice produced exactly one **Add charts?** prompt and zero cell prompts. No wrote no primary cells or shapes and preserved selection/counts. Yes Completed with the explicit overlap-approved outcome and retained the existing shape's native ID. The successful probe asserted actual overlap of the added chart and the pre-existing shape, rather than only assuming a collision.

Source A1:C4 and distant AZ200 remained unchanged. All probe-owned workbooks were closed without saving. No normal user workbook, installer or persistent add-in registration was used. Evidence/helper: ignored `TestResults/Codex/Batch0208-chart-native.log` and `Batch0208-chart-native.ps1`; safe diagnostics are in `Batch0208-chart-native-diagnostics`.

The final 24-case run reloaded the binaries produced by the final gate. An additional intermediate probe assertion incorrectly assumed that a pre-existing shape's geometry must remain exactly 1000 × 1000 points after an approved full output. Native output retained that shape but changed its width to approximately 1038.4 points for ANOVA and 1026.4 for KW, with Left/Top/Height unchanged. The unchanged legacy formatter and native Excel layout/anchoring remain in effect; the exact cause of this native geometry adjustment was not isolated. The corrected probe checks retained identity and actual overlap and records the safe before/after coordinates. It does not claim shape geometry remains unchanged on Yes. No renderer or default was changed to mask this observation. Owner visual acceptance should explicitly include existing anchored shapes.

Owner acceptance remains outstanding. Reload the rebuilt XLL in a fresh normal Excel profile and use the actual Ribbon:

1. ANOVA and KW: Group by Column, A1:C4 or whole-column A:C input, current F11 output; check charts on/off, supplementary spacing, numeric p-values/highlighting, non-A1 placement, AutoFit=False and chart placement/output order visually.
2. Put a value or `=""` formula in the first output table, then a later/supplementary table; choose No and Yes. No must preserve all output cells, formatting, selection and shapes. A separator-only or distant sentinel must neither prompt nor change.
3. Repeat new worksheet and new workbook; verify the intended workbook/sheet, CodeName independence, complete supplements and charts.
4. With an existing chart/shape, check the distinct overlap wording and No/Yes. Rejected/uninspectable safety checks must not be mistaken for actual occupied cells. Unit tests cover stale cell/shape changes and injected partial failures; these were not induced destructively in live Excel.
5. Visually certify chart rendering and the normal in-process Ribbon flow. No x86 Excel or Mac runtime acceptance is claimed. Office Online was not rerun live for this Windows-only correction; its unchanged 318-test suite passed.

### Corrective paths and preserved scope

Only five authored paths changed for this follow-up:

* `BESHstatNG/src/UI/UibyID.vb` — replace UsedRange chart guard with separate shape inspection/decision wiring; original cell prompt retained.
* `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb` — separate cell and shape decisions, cached first read plus fresh revalidation, truthful chart cancellation/completion reporting and safe role/stage logs.
* `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb` — exact cell reads only, bounded shape inventory/protection fingerprint, reject obsolete broad cell guards, safe cell diagnostics.
* `BESHStatNG.Test/src/DataManagement/SharedOutputPlanTests.vb` — 29 new regression cases and the explicitly explained obsolete guard-test replacement.
* This architecture note — verified cause, policy, commands/results, native evidence and acceptance limits.

The required build refreshed only the already-generated HelpContext/HelpLinks timestamp comments; authored help metadata and generated version content were preserved. Existing batch changes and both historical stashes remain. The master plan and separate Phase 04A audit are byte-for-byte unchanged by this follow-up: **222 / 26 / 196**, **02.08 pending review/acceptance**. Common v1 DTOs, Office.js, numerical code, histogram binning, renderer defaults, framework targets, Ribbon/UDF contracts and accepted fixtures were not edited. No Gate B, Phase 04A, later batch, commit, push, merge, branch switch, installer or deployment was performed.
