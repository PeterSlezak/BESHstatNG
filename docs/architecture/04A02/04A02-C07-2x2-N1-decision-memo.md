# Exact 2x2 N1 state-only go/no-go memo — currently NO GO

**Case:** `04A02-2X2-NATIVE-01`. **Stage:** N1 initialized state read only, not N2 output. No N permission is supplied by the C07 prompt or the later personal-library setup update. No Excel/COM/UIA provider was launched, attached, inspected or clicked in C07.

## Exact prerequisite set

- Blocked: `BESHStatNG.Ui82x2.RefEdit1_WorksheetData`, `BESHStatNG.Ui82x2.RefEditOutput`.
- Additional deferred: `control:BESHStatNG.Ui82x2.btnHelp`. Preserve both blocked IDs' `control:` obligations too; do not silently omit help or change the accepted ledger.
- Shared edge: `BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb|WithEvents|xlApp@34`, UNKNOWN_HIGH, owners 04A.27/45–47. Screen-count mode still initializes both ExcelConnector properties in Ui82x2.New. It does not bypass the shared edge.
- Executed candidate proof: nine C07 fake-host tests against actual WindowsDocumentLifecycle/WindowsExecutionRun injection seams; source/definition/event evidence and prior adapter tests. They do **not** execute private RefEdit handlers or prove real COM lifetime. External risk-owner review and direct-handler testability request CR-REFEDIT-SEAM-07 remain open.
- Separate `Ui0OneRefeditMulticol.progressBarExactCalc` P0 / 04A.46 and Cox 04A.47 restrictions remain unchanged; do not infer they were fixed or waived by this narrower case.

## What an N1 request would authorize, if prerequisites are actually satisfied

1. A human owner/operator prepares and saves only the new disposable workbook with proposed future N2 data `Data!A1:B2 = [[1,9],[11,3]]`, row/column orientation R1C1/R1C2/R2C1/R2C2, filename/hash and initial active-sheet/process snapshot. No user/production data or existing live workbook.
2. After a new exact N1 decision, use only the owner-approved Excel/XLL source/build/hash in a separate owned `/x` process. Verify PID, start time, session, no other Excel instances/workbooks/addins/security prompts and the exact saved disposable context. Opening the ribbon `Onbtm2x2Pressed` constructs the form and binds RefEdit, so this step already requires shared-risk mitigation and N1.
3. Read actual initialized controls/AutomationIds/names/Enabled/Visible/Toggle/Value/culture/window state. Do not change options/counts/alpha, click Compute, toggle state, focus an unknown control, write a sheet or load another XLL. Designer screen counts are 10 each; the initialized state must be recorded as observed, not falsely reported as the proposed N2 counts. Alpha may reflect global preferences; do not invent .05 as observed initialized state.
4. A separately reviewed helper runs under an external short deadline (proposed 15 seconds), bound to only the approved owned process. Unsupported provider/AutomationId/modal/context/PID changes fail closed; no coordinate/index guessing or automatic dialog dismissal. Preserve raw logs, metadata, before/after saved-workbook/state checks and separately allowed screenshots under a new exclusive output path.
5. On failure stop and retain partial output. Never retry a path, change persistent settings, delete sheets or taskkill arbitrary Excel. Supervisor may stop only its recorded helper PID; owner closes only the owned disposable Excel after preserving evidence. No ordinary pilot induces worksheet deletion or historical 0x800401A8.

## N0 findings that must be resolved before choosing a helper

The frozen C06 v1 source attaches using GetActiveObject before it can verify the returned Excel PID. ROT selection may not identify the intended `/x` instance. Checking PID afterward is necessary but does not undo attachment to the wrong instance. UIA property reads can invoke provider code and race with application state; `NoWorkbookWrite=True` is an issued-call claim, not measured proof of no state change. The original v1 proposal remains unchanged and **must not be run as-is merely because N1 is later granted**.

Resolve CR-N1-ROT-READ-07 with a new reviewed exact-instance or explicitly human-supervised UIA-only/context protocol. Bind script/XLL/context hashes and revalidate before/after; distinguish read calls issued from observed state changes. The host-free N0 contract never permits native calls even if all synthetic Boolean fields are set true: authenticity and runtime proof are independent.

## Minimum external approval record — not authored by Codex

Provide the exact case/stage/steps, risk-owner mitigation and executed regression receipts for all three obligations/shared edge; independently attributable owner/role/date/source; reviewed new helper/XLL/executable hashes; approved literal workbook/input/output paths and PID/session/ownership protocol; provider read-side-effect and timeout policy; explicit allowed UIA/optional exact-instance COM read scope; before/after no-write acceptance and stop/cleanup protocol. Default answer: **NO / PENDING**, not a signed form.

**N2 is a new decision after successful N1 review.** Only then set the proposed Fisher/OR/RR on, Chi2/association/Liddell off, alpha .05 and counts 1/9/11/3; separately review worksheet-input/zero/invalid/overflow/large-N/modal variants. Export complete typed cells/formulas/NumberFormat/display/errors/warnings/placement/options and saved workbook hashes. There is no chart in this handler. N1 success cannot authorize computation, output writes, independent reference publication or golden promotion.

**Current decision:** NO GO — no authentic N1, incomplete direct RefEdit mitigation/risk review, unresolved ROT/provider-read/context assurance. Source-only N0 tests pass within their scope. No original obligation is discharged or N released.
