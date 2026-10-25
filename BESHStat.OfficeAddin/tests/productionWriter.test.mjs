import test from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { createProductionWriter, validateProductionPlan, enableWriterTrace, getWriterTrace } from "../wwwroot/js/productionWriter.mjs";
import { createWorkbookTransport, create } from "../wwwroot/js/workbookAdapter.mjs";
import { pilotWorkbook } from "./helpers/pilotWorkbook.mjs";
const fixture = JSON.parse(readFileSync(new URL("../../BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json", import.meta.url), "utf8"));
async function setup(destination = 0, options = {}) {
    const fake = pilotWorkbook(), writer = createProductionWriter(fake.scope, options);
    const pin = await writer.pinSource(fake.state.snapshot()); assert.equal(pin.code, "workbook.ok");
    const plan = structuredClone(fixture); plan.target.worksheetName = destination ? "Results" : "Input"; plan.target.destination = destination;
    for (const t of plan.tables) t.autoFit = false;
    const intent = { planIdentity: "plan", targetIdentity: "target", workbookIdentity: pin.sessionIdentity,
        worksheetIdentity: destination ? null : "sheet-1", worksheetName: plan.target.worksheetName, destination,
        startRow: plan.target.startRow, startColumn: plan.target.startColumn,
        regions: plan.tables.map(t => ({ artifactId: t.artifactId, bounds: t.bounds, mayMutate: true })), reservations: [], guards: [] };
    const inspect = () => writer.prepareOutput(plan, intent, pin.sourceToken);
    return { ...fake, writer, plan, intent, pin, inspect };
}
test("production empty current output commits and verifies exact values/formats/fonts without touching gaps", async () => {
    const x = await setup(); assert.equal((await x.inspect()).state, 0);
    const result = await x.writer.execute(x.plan, x.intent, null);
    assert.equal(result.state, 0); assert.deepEqual(result.confirmedWrittenIds, x.intent.regions.map(r => r.artifactId));
    assert.equal(x.cells.has("sheet-1:8:3"), false); assert.equal(x.state.added, 0);
    await x.writer.dispose(); assert.equal(x.state.listenerCount(), 0);
});

for (const { destination, occupied } of [{ destination: 0, occupied: false }, { destination: 0, occupied: true }, { destination: 1, occupied: false }])
test(`verified ${destination ? "new" : "existing"} sheet -> capture/resolve/pin independent source (occupied=${occupied})`, async () => {
    const x = await setup(destination);
    if (occupied && !destination) x.state.setCell(6, 3, "occupied");
    const inspection = await x.inspect(), staleEvent = x.state.queuedEvent("onAdded");
    const result = await x.writer.execute(x.plan, x.intent, inspection.state === 1 ? { inspection, approved: true } : null);
    assert.deepEqual([result.state, result.code, result.mutationMayHaveOccurred, result.confirmedWrittenIds.length], [0, "output.completed", true, x.plan.tables.length]);
    assert.equal(x.state.listenerCount(), 0);
    const firstCells = structuredClone([...x.cells.entries()]);
    x.sheets.set("source-b", { id: "source-b", name: "Second source" }); x.state.sheetId = "source-b";
    for (let c = 1; c <= 3; c++) { x.state.setCell(1, c, `B ${c}`); for (let r = 2; r <= 4; r++) x.state.setCell(r, c, r * c); }
    const callbacks = [];
    assert.equal((await x.writer.beginCapture("second-owner", false, { invokeMethodAsync: async (...args) => { callbacks.push(args); } })).code, "workbook.ok");
    const captured = await x.writer.endCapture("second-owner", false);
    assert.equal(captured.code, "workbook.ok"); assert.equal(captured.reference.worksheetId, "source-b");
    const resolved = await x.writer.resolveReference(captured.reference, captured.handle);
    const pin = await x.writer.pinSource(x.state.snapshot(), null, resolved.handle);
    assert.equal(pin.code, "workbook.ok"); assert.notEqual(pin.sourceToken, x.pin.sourceToken);
    assert.equal(x.state.listenerCount(), 9); staleEvent();
    assert.equal((await x.writer.checkSource(pin.sourceToken)).code, "workbook.ok");
    assert.notEqual((await x.writer.checkSource(x.pin.sourceToken)).code, "workbook.ok");
    assert.equal((await x.writer.execute(x.plan, x.intent, { inspection, approved: true })).state, 2);
    x.intent = { ...x.intent, workbookIdentity: pin.sessionIdentity, worksheetIdentity: "source-b", worksheetName: "Second source", destination: 0,
        planIdentity: "second-plan", targetIdentity: "second-target" };
    x.plan.target = { ...x.plan.target, worksheetName: "Second source", destination: 0 };
    assert.equal((await x.writer.prepareOutput(x.plan, x.intent, pin.sourceToken)).state, 0);
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).code, "output.completed");
    for (const [key, cell] of firstCells) assert.deepEqual(x.cells.get(key), cell);
    assert.equal(x.state.listenerCount(), 0); await x.writer.dispose();
});

for (const event of ["onAdded", "onChanged", "onActivated", "onSelectionChanged"]) for (const stage of ["mutation", "readback", "delayed"])
test(`${event} during ${stage} cannot downgrade verified completion or poison next source`, async () => {
    const x = await setup(1); await x.inspect();
    const queued = x.state.queuedEvent(event), at = x.state.syncs + (stage === "mutation" ? 3 : 4);
    x.state.hook = async n => { if (n === at && stage !== "delayed") x.state.emit(event); };
    // New-sheet execute has two inspection syncs plus one source sync before mutation.
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).code, "output.completed");
    x.state.hook = async () => {};
    const pin = await x.writer.pinSource(x.state.snapshot());
    await new Promise(resolve => setTimeout(() => { queued(); resolve(); }, 0));
    assert.equal((await x.writer.checkSource(pin.sourceToken)).code, "workbook.ok");
    x.state.emit(event);
    assert.notEqual((await x.writer.checkSource(pin.sourceToken)).code, "workbook.ok");
    await x.writer.dispose();
});

for (const destination of [0, 1]) test(`uncertain destination ${destination} blocks capture and all later writes, fresh pane requires settled operation`, async () => {
    const x = await setup(destination); await x.inspect(); x.state.rejectCommit = true;
    const result = await x.writer.execute(x.plan, x.intent, null);
    assert.deepEqual([result.state, result.code, result.mutationMayHaveOccurred, result.confirmedWrittenIds.length], [4, "output.mutation-uncertain", true, 0]);
    const mutations = x.state.mutations;
    x.writer.invalidate();
    assert.equal((await x.writer.captureSelection(false)).code, "output.mutation-uncertain");
    assert.equal((await x.writer.beginCapture("later", false, { invokeMethodAsync: async () => {} })).code, "output.mutation-uncertain");
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, 2);
    assert.equal(x.state.mutations, mutations);
    await x.writer.dispose(); x.state.rejectCommit = false;
    const fresh = createProductionWriter(x.scope); assert.equal((await fresh.captureSelection(false)).code, "workbook.ok"); await fresh.dispose();
});

test("timed-out native mutation retains busy lease through settlement and never clears uncertainty", async () => {
    const x = await setup(1, { timeoutMs: 100 }); await x.inspect();
    let release; const gate = new Promise(resolve => { release = resolve; }), at = x.state.syncs + 3;
    x.state.hook = async n => { if (n === at) await gate; };
    const result = await x.writer.execute(x.plan, x.intent, null);
    assert.equal(result.state, 4); assert.equal(result.code, "output.mutation-uncertain");
    assert.equal((await x.writer.captureSelection(false)).code, "host.busy");
    assert.equal((await x.writer.pinSource(x.state.snapshot())).code, "host.busy");
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, 2);
    release(); await new Promise(resolve => setTimeout(resolve, 10));
    assert.equal((await x.writer.captureSelection(false)).code, "output.mutation-uncertain"); await x.writer.dispose();
});

test("timeout during listener cleanup preserves already verified Completed and blocks overlap until cleanup settles", async () => {
    const x = await setup(1, { timeoutMs: 100 }); await x.inspect();
    let release; const gate = new Promise(resolve => { release = resolve; }), at = x.state.syncs + 5;
    x.state.hook = async n => { if (n === at) await gate; };
    const result = await x.writer.execute(x.plan, x.intent, null);
    assert.equal(result.code, "output.completed"); assert.equal(result.confirmedWrittenIds.length, x.plan.tables.length);
    assert.equal((await x.writer.captureSelection(false)).code, "host.busy");
    release(); await new Promise(resolve => setTimeout(resolve, 10));
    assert.equal((await x.writer.captureSelection(false)).code, "workbook.ok"); await x.writer.dispose();
});

test("native batch cleanup exception after verified readback cannot relabel Completed or retry cells", async () => {
    const x = await setup(1); await x.inspect(); const nativeRun = x.scope.Excel.run;
    x.scope.Excel.run = async (...args) => { const result = await nativeRun(...args); if (result?.code === "output.completed") throw new Error("PRIVATE CLEANUP"); return result; };
    const result = await x.writer.execute(x.plan, x.intent, null), mutations = x.state.mutations;
    assert.equal(result.code, "output.completed"); assert.equal(result.confirmedWrittenIds.length, x.plan.tables.length);
    assert.equal((await x.writer.captureSelection(false)).code, "workbook.ok");
    assert.equal((await x.writer.pinSource(x.state.snapshot())).code, "workbook.ok");
    assert.equal(x.state.listenerCount(), 9); assert.equal(x.state.mutations, mutations); await x.writer.dispose();
});

for (const category of ["worksheet", "values", "formulas", "number-format", "font-bold", "font-color", "font-name", "font-size", "alignment", "fill", "borders"])
test(`actual ${category} mismatch stays uncertain with redacted stage evidence`, async () => {
    const x = await setup(1); await x.inspect(); const at = x.state.syncs + 4;
    enableWriterTrace();
    x.state.hook = async n => {
        if (n !== at) return;
        const cell = x.cells.get("new-1:6:3"), font = x.fonts.get("new-1:6:3");
        if (category === "worksheet") x.sheets.get("new-1").name = "PRIVATE_SHEET";
        if (category === "values") cell.value = "PRIVATE_VALUE";
        if (category === "formulas") { cell.formula = "=PRIVATE_FORMULA"; cell.hasFormula = true; }
        if (category === "number-format") cell.format = "PRIVATE_FORMAT";
        if (category === "font-bold") font.bold = !font.bold;
        if (category === "font-color") font.color = "PRIVATE_COLOR";
        if (category === "font-name") font.name = "PRIVATE_FONT";
        if (category === "font-size") font.size = 13;
        if (category === "alignment") x.formats.get("new-1:6:3").horizontalAlignment = "Right";
        if (category === "fill") x.fills.get("new-1:6:3").pattern = "PRIVATE_PATTERN";
        if (category === "borders") x.borders.get("new-1:6:3:EdgeTop").style = "PRIVATE_STYLE";
    };
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).code, "output.mutation-uncertain");
    const entries = getWriterTrace();
    assert.equal(entries.some(e => e.stage === `verify.${category}` && e.detail.startsWith("mismatch")), true);
    assert.equal(entries.at(-1).detail, "output.mutation-uncertain");
    assert.equal(JSON.stringify(entries).includes("PRIVATE"), false);
    assert.equal(entries.every(e => e.ms >= 0 && e.ms <= 60000), true);
    enableWriterTrace(false); await x.writer.dispose();
});

test("case-equivalent hex colors remain supported; unevidenced named colors and formats stay uncertain", async () => {
    for (const representation of ["hex-case", "named-color", "changed-format"]) {
        const x = await setup(1); await x.inspect(); const at = x.state.syncs + 4;
        x.state.hook = async n => { if (n === at) {
            if (representation === "hex-case") for (const font of x.fonts.values()) font.color = font.color.toLowerCase();
            if (representation === "named-color") x.fonts.get("new-1:6:3").color = "black";
            if (representation === "changed-format") x.cells.get("new-1:6:3").format = "0.000000";
        } };
        assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, representation === "hex-case" ? 0 : 4);
        await x.writer.dispose();
    }
});

test("trace opt-in is bounded, detached from returned snapshots and records native failure stage without secrets", async () => {
    enableWriterTrace(false); const x = await setup(1); await x.inspect(); assert.deepEqual(getWriterTrace(), []);
    enableWriterTrace(); const at = x.state.syncs + 3;
    x.state.hook = async n => { if (n === at) throw Object.assign(new Error("PRIVATE_NATIVE"), { code: "PRIVATE_CODE" }); };
    await x.writer.execute(x.plan, x.intent, null);
    assert.equal(getWriterTrace().some(e => e.stage === "mutation.sync.start"), true);
    assert.equal(getWriterTrace().some(e => e.stage === "mutation.sync.end"), false);
    assert.equal(getWriterTrace().some(e => e.detail === "native-exception"), true);
    for (let i = 0; i < 300; i++) await x.writer.captureSelection(false);
    const copy = getWriterTrace(); assert.equal(copy.length, 256); copy[0].stage = "PRIVATE";
    assert.equal(JSON.stringify(getWriterTrace()).includes("PRIVATE"), false); enableWriterTrace(false); await x.writer.dispose();
});
for (const destination of [0, 1]) test(`production destination ${destination} real algorithm`, async () => {
    const x = await setup(destination); assert.equal((await x.inspect()).code, "workbook.ok");
    assert.equal(x.state.added, 0); assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, 0);
    assert.equal(x.state.added, destination); await x.writer.dispose();
});
for (const approved of [false, true]) test(`occupied destination explicit ${approved ? "Yes" : "No"}`, async () => {
    const x = await setup(); x.state.setCell(6, 3, "occupied", "String", "=1");
    const inspection = await x.inspect(); assert.equal(inspection.state, 1); assert.equal(inspection.hasFormulas, true);
    const result = await x.writer.execute(x.plan, x.intent, { inspection, approved });
    assert.equal(result.state, approved ? 0 : 1); assert.equal(x.state.mutations > 0, approved);
    await x.writer.dispose();
});
for (const guard of ["protected", "bookProtected", "merged", "tables", "pivots"]) test(`target guard ${guard} rejects before mutation`, async () => {
    const x = await setup(); x.state[guard] = true;
    assert.notEqual((await x.inspect()).code, "workbook.ok"); assert.equal(x.state.mutations, 0); await x.writer.dispose();
});
for (const mode of ["readOnly", null]) test(`document mode ${mode} cannot claim writability`, async () => {
    const x = await setup(); x.state.document.mode = mode;
    assert.equal((await x.inspect()).code, "output.target-unwritable"); assert.equal(x.state.mutations, 0); await x.writer.dispose();
});
for (const event of ["onChanged", "onSelectionChanged", "onNameChanged", "onDeleted", "onAdded", "onActivated", "onProtectionChanged", "onFormatChanged", "workbook-activated"])
test(`sticky invalidation on ${event}, even if visible metadata returns`, async () => {
    const x = await setup(); await x.inspect(); x.state.emit(event);
    assert.notEqual((await x.writer.checkSource(x.pin.sourceToken)).code, "workbook.ok");
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, 2); assert.equal(x.state.mutations, 0); await x.writer.dispose();
});
for (const change of ["document", "selection", "source-value", "source-formula", "same-name-replaced", "deleted", "renamed", "target-content"])
test(`last-boundary check catches ${change} without relying on an event`, async () => {
    const x = await setup(); await x.inspect();
    if (change === "document") x.state.document = {};
    if (change === "selection") x.state.selected.row++;
    if (change === "source-value") x.state.setCell(2, 1, 900);
    if (change === "source-formula") x.state.setCell(2, 1, 3, "Double", "=3");
    if (change === "same-name-replaced") { x.sheets.delete("sheet-1"); x.state.sheetId = "replacement"; x.sheets.set("replacement", { id: "replacement", name: "Input" }); }
    if (change === "deleted") x.sheets.delete("sheet-1");
    if (change === "renamed") x.sheets.get("sheet-1").name = "Renamed";
    if (change === "target-content") x.state.setCell(6, 3, "new content");
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, 2); assert.equal(x.state.mutations, 0); await x.writer.dispose();
});
for (const defect of ["autofit", "off-grid", "overlap", "wrong-intent", "formula-kind", "nan", "bad-role", "duplicate-id", "too-many-cells"])
test(`wire defect ${defect} rejects before mutation`, async () => {
    const x = await setup(), t = x.plan.tables[0];
    if (defect === "autofit") t.autoFit = true;
    if (defect === "off-grid") t.bounds.endRow = 1048577;
    if (defect === "overlap") x.plan.tables[1].bounds = { ...t.bounds };
    if (defect === "wrong-intent") x.intent.startRow++;
    if (defect === "formula-kind") t.cells[0].value.kind = "formula";
    if (defect === "nan") t.cells[0].value = { kind: "number", numberValue: NaN };
    if (defect === "bad-role") t.cells[0].role = "arbitrary";
    if (defect === "duplicate-id") x.plan.tables[1].artifactId = t.artifactId;
    if (defect === "too-many-cells") t.cells = Array(10001).fill(t.cells[0]);
    assert.throws(() => validateProductionPlan(x.plan, x.intent)); assert.equal(x.state.mutations, 0); await x.writer.dispose();
});
test("name collision and invalid names never create a worksheet", async () => {
    for (const name of ["Input", "bad/name", "History", "'quoted", "x".repeat(32)]) {
        const x = await setup(1); x.plan.target.worksheetName = x.intent.worksheetName = name;
        assert.notEqual((await x.inspect()).code, "workbook.ok"); assert.equal(x.state.added, 0); await x.writer.dispose();
    }
});
test("missing/stale consent, changed plan and single use cannot authorize writes", async () => {
    for (const mode of ["missing", "stale", "changed-plan", "consumed"]) {
        const x = await setup(); x.state.setCell(6, 3, "occupied"); const inspection = await x.inspect();
        let consent = { inspection, approved: true };
        if (mode === "missing") consent = null;
        if (mode === "stale") consent.inspection.observationIdentity = "other";
        if (mode === "changed-plan") x.plan.tables[0].cells[0].numberFormat = "0.00";
        if (mode === "consumed") await x.writer.execute(x.plan, x.intent, { inspection, approved: false });
        assert.equal((await x.writer.execute(x.plan, x.intent, consent)).state, 2); assert.equal(x.state.mutations, 0); await x.writer.dispose();
    }
});
test("string output never becomes a formula", async () => {
    const x = await setup(); x.plan.tables[0].cells[0].value = { kind: "text", textValue: "=HYPERLINK(secret)", numberValue: null, booleanValue: null };
    await x.inspect(); assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, 0);
    const c = x.cells.get("sheet-1:6:3"); assert.equal(c.type, "String"); assert.equal(c.value, "=HYPERLINK(secret)");
    await x.writer.dispose();
});
test("mutation sync refusal is uncertain, never automatically retryable", async () => {
    const x = await setup(); await x.inspect(); x.state.rejectCommit = true;
    const result = await x.writer.execute(x.plan, x.intent, null); assert.equal(result.state, 4); assert.equal(result.mutationMayHaveOccurred, true);
    assert.equal((await x.writer.pinSource(x.state.snapshot())).code, "output.mutation-uncertain"); await x.writer.dispose();
});
test("production transport uses the installed writer and rejects a write without inspection", async () => {
    const x = await setup(); const transport = createWorkbookTransport(x.scope, { writer: x.writer });
    assert.equal((await transport.write(x.plan, x.intent, null)).state, 2); assert.equal(x.state.mutations, 0); await transport.dispose();
});
for (const offset of [1, 2, 3, 4, 5]) test(`native failure at execute sync ${offset} reports mutation truthfully`, async () => {
    const x = await setup(); await x.inspect(); const failAt = x.state.syncs + offset;
    x.state.hook = async n => { if (n === failAt) throw new Error("PRIVATE NATIVE FAILURE"); };
    const result = await x.writer.execute(x.plan, x.intent, null);
    assert.equal(result.state, offset < 4 ? 2 : 4); assert.equal(result.mutationMayHaveOccurred, offset >= 4);
    assert.equal(JSON.stringify(result).includes("PRIVATE"), false); await x.writer.dispose();
});
test("late cancellation after committed sync preserves verified completion", async () => {
    const x = await setup(); await x.inspect(); const late = x.state.syncs + 5;
    x.state.hook = async n => { if (n === late) x.writer.invalidate(); };
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, 0); await x.writer.dispose();
});
test("cancel at last pre-mutation boundary queues no values or worksheet addition", async () => {
    const x = await setup(1); await x.inspect(); const cancelAt = x.state.syncs + 2;
    x.state.hook = async n => { if (n === cancelAt) x.writer.invalidate(); };
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, 2);
    assert.equal(x.state.mutations, 0); assert.equal(x.state.added, 0); await x.writer.dispose();
});
test("busy and disposed pin operations cannot publish a new binding", async () => {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope);
    let release; const gate = new Promise(resolve => { release = resolve; });
    x.state.hook = async n => { if (n === 1) await gate; };
    const pending = writer.pinSource(x.state.snapshot());
    await new Promise(resolve => setTimeout(resolve, 0));
    assert.equal((await writer.pinSource(x.state.snapshot())).code, "host.busy");
    const disposal = writer.dispose(); release(); await disposal;
    assert.notEqual((await pending).code, "workbook.ok"); assert.equal(x.state.mutations, 0);
});
test("production create factory installs the actual writer, no injected write port", async () => {
    const x = await setup(); await x.writer.dispose();
    const saved = { Office: globalThis.Office, Excel: globalThis.Excel };
    globalThis.Office = x.scope.Office; globalThis.Excel = x.scope.Excel;
    const adapter = create(false);
    try {
        const pin = await adapter.pinSource(x.state.snapshot()); assert.equal(pin.code, "workbook.ok");
        x.intent.workbookIdentity = pin.sessionIdentity;
        const inspected = await adapter.prepareOutput(x.plan, x.intent, pin.sourceToken); assert.equal(inspected.code, "workbook.ok");
        assert.equal((await adapter.write(x.plan, x.intent, null)).state, 0);
    } finally { await adapter.dispose(); globalThis.Office = saved.Office; globalThis.Excel = saved.Excel; }
});
test("missing API, bad source size, disjoint selection and spilled targets fail closed", async () => {
    for (const defect of ["api", "size", "disjoint", "spill"]) {
        const x = pilotWorkbook(), writer = createProductionWriter(x.scope);
        if (defect === "api") x.scope.Office.context.requirements.isSetSupported = () => false;
        const snapshot = x.state.snapshot(); if (defect === "size") snapshot.rowCount = 10001;
        if (defect === "disjoint") x.state.multiple = true;
        const pin = await writer.pinSource(snapshot);
        if (defect !== "spill") assert.notEqual(pin.code, "workbook.ok");
        else {
            await writer.dispose(); const y = await setup(); y.state.spill = true;
            assert.notEqual((await y.inspect()).code, "workbook.ok"); await y.writer.dispose();
        }
        assert.equal(x.state.mutations, 0); await writer.dispose();
    }
});
