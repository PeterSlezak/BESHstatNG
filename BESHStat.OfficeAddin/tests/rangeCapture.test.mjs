import test from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { createProductionWriter } from "../wwwroot/js/productionWriter.mjs";
import { createRangeCapture } from "../wwwroot/js/rangeCapture.mjs";
import { pilotWorkbook } from "./helpers/pilotWorkbook.mjs";
const fixture = JSON.parse(readFileSync(new URL("../../BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json", import.meta.url), "utf8"));
test("timed-out capture keeps its native lease until the pending sync settles", async () => {
    const x = pilotWorkbook(), capture = createRangeCapture(x.scope, { begin: async () => {}, end: async () => {}, abort: async () => {} });
    let release, started;
    const blocked = new Promise(resolve => { release = resolve; }), entered = new Promise(resolve => { started = resolve; });
    x.state.hook = async n => { if (n === 1) { started(); await blocked; } };
    const pending = capture.selection(false); await entered;
    await assert.rejects(pending, e => e.safeCode === "input.time-budget");
    await assert.rejects(capture.selection(false), e => e.safeCode === "host.busy");
    release(); await new Promise(resolve => setImmediate(resolve));
    assert.equal((await capture.selection(false)).code, "workbook.ok");
    assert.equal(x.state.mutations, 0); await capture.dispose();
});
const callback = items => ({ async invokeMethodAsync(method, reply) { assert.equal(method, "RangeCapturedCallback"); items.push(reply); } });
async function settled(condition) { const end = Date.now() + 2000; while (!condition()) { if (Date.now() > end) throw Error("event did not settle"); await new Promise(r => setTimeout(r, 1)); } }
function ref(name = "Input", id = "") { return { worksheetId: id, worksheetName: name, workbookId: "", workbookName: "", addressDialect: 1,
    address: "A1:C4", startRow: 1, startColumn: 1, rowCount: 4, columnCount: 3 }; }
async function setup() {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope), snapshot = x.state.snapshot();
    const pin = await writer.pinSource(snapshot); assert.equal(pin.code, "workbook.ok");
    const plan = structuredClone(fixture); plan.target.worksheetName = "Input"; for (const t of plan.tables) t.autoFit = false;
    const intent = { planIdentity: "plan", targetIdentity: "target", workbookIdentity: pin.sessionIdentity, worksheetIdentity: "sheet-1",
        worksheetName: "Input", destination: 0, startRow: plan.target.startRow, startColumn: plan.target.startColumn,
        regions: plan.tables.map(t => ({ artifactId: t.artifactId, bounds: t.bounds, mayMutate: true })), reservations: [], guards: [] };
    return { ...x, writer, snapshot, pin, plan, intent };
}
for (const name of ["Study Data", "O'Brien Δ", "日本語 α ".repeat(3)]) test(`metadata capture preserves full worksheet name ${name}`, async () => {
    const x = pilotWorkbook(); x.sheets.get("sheet-1").name = name;
    const writer = createProductionWriter(x.scope), reply = await writer.captureSelection(false);
    assert.equal(reply.code, "workbook.ok"); assert.equal(reply.reference.worksheetName, name); assert.equal(reply.reference.address, "A1:C4");
    assert.equal(x.state.loads.some(p => /values|formulas|numberFormat/.test(p)), false);
    assert.equal(x.state.mutations, 0); assert.equal(x.state.listenerCount(), 0); await writer.dispose();
});
test("manual reference resolution uses Common coordinates and native identity, no data load", async () => {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope);
    const reply = await writer.resolveReference(ref()); assert.equal(reply.code, "workbook.ok");
    assert.equal(reply.reference.worksheetId, "sheet-1"); assert.equal(x.state.mutations, 0);
    x.state.selected = { row: 99, col: 9, rows: 1, cols: 1 };
    const pin = await writer.pinSource(x.state.snapshot(), null, reply.handle);
    assert.equal(pin.code, "user.document-changed"); // An unrelated acquisition cannot reuse this reference.
    await writer.dispose();
});
for (const defect of ["external-book", "multi-area", "off-grid", "oversize", "unknown-sheet", "wrong-coordinate", "native-dialect"])
test(`typed reference ${defect} is rejected without mutation`, async () => {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope), r = ref();
    if (defect === "external-book") r.workbookName = "Other.xlsx";
    if (defect === "multi-area") r.address = "A1,B2";
    if (defect === "off-grid") r.startRow = 1048577;
    if (defect === "oversize") r.rowCount = 10000;
    if (defect === "unknown-sheet") r.worksheetName = "Missing";
    if (defect === "wrong-coordinate") r.startRow = 2;
    if (defect === "native-dialect") r.addressDialect = 2;
    assert.notEqual((await writer.resolveReference(r)).code, "workbook.ok"); assert.equal(x.state.mutations, 0); await writer.dispose();
});
for (const change of ["rename", "delete-recreate", "document"])
test(`a previously resolved reference rejects ${change} without same-name fallback`, async () => {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope), r = ref();
    const first = await writer.resolveReference(r); assert.equal(first.code, "workbook.ok");
    if (change === "rename") x.sheets.get("sheet-1").name = "Changed";
    if (change === "delete-recreate") { x.sheets.delete("sheet-1"); x.sheets.set("replacement", { id: "replacement", name: "Input" }); }
    if (change === "document") x.state.document = { mode: "readWrite" };
    assert.notEqual((await writer.resolveReference(r, first.handle)).code, "workbook.ok"); assert.equal(x.state.mutations, 0); await writer.dispose();
});
test("manual addressed input can differ from selected cells and remains content-bound", async () => {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope), snapshot = x.state.snapshot();
    const resolved = await writer.resolveReference(ref());
    x.state.selected = { row: 20, col: 20, rows: 1, cols: 1 };
    const pin = await writer.pinSource(snapshot, null, resolved.handle); assert.equal(pin.code, "workbook.ok");
    assert.equal((await writer.checkSource(pin.sourceToken)).code, "workbook.ok");
    x.state.setCell(2, 1, 999); assert.equal((await writer.checkSource(pin.sourceToken)).code, "user.document-changed"); await writer.dispose();
});
test("armed events coalesce latest native selection and stop on Finish or disposal", async () => {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope), replies = [];
    assert.equal((await writer.beginCapture("owner", false, callback(replies))).code, "workbook.ok");
    assert.equal(x.state.listenerCount(), 1);
    x.state.selected = { row: 5, col: 2, rows: 3, cols: 2 };
    for (let i = 0; i < 10; i++) x.state.emit("onSelectionChanged", { worksheetId: "sheet-1", address: "STALE" });
    await settled(() => replies.length > 0); assert.equal(replies.at(-1).reference.address, "B5:C7");
    assert.equal(replies.at(-1).owner, "owner");
    const end = await writer.endCapture("owner", false); assert.equal(end.code, "workbook.ok"); assert.equal(x.state.listenerCount(), 0);
    const count = replies.length; x.state.emit("onSelectionChanged"); assert.equal(replies.length, count);
    await writer.beginCapture("next", false, callback(replies)); await writer.dispose(); assert.equal(x.state.listenerCount(), 0); assert.equal(x.state.mutations, 0);
});
test("wrong owner cannot stop capture; cancellation removes its subscription", async () => {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope);
    await writer.beginCapture("owner", false, callback([])); await writer.endCapture("wrong", true); assert.equal(x.state.listenerCount(), 1);
    await writer.endCapture("owner", true); assert.equal(x.state.listenerCount(), 0); await writer.dispose();
});
test("missing event requirement degrades to explicit metadata capture", async () => {
    const x = pilotWorkbook(); x.scope.Office.context.requirements.isSetSupported = (_n, v) => v === "1.1";
    const writer = createProductionWriter(x.scope);
    assert.equal((await writer.beginCapture("owner", false, callback([]))).code, "range.capture-unavailable");
    assert.equal((await writer.captureSelection(false)).code, "workbook.ok"); assert.equal(x.state.listenerCount(), 0); await writer.dispose();
});
test("output capture rejects rectangles and disjoint selection without cell changes", async () => {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope);
    assert.equal((await writer.captureSelection(true)).code, "input.invalid-reference");
    x.state.multiple = true; assert.notEqual((await writer.captureSelection(false)).code, "workbook.ok");
    assert.equal(x.state.mutations, 0); await writer.dispose();
});
for (const approved of [false, true]) test(`explicit different-sheet output capture preserves source and occupied ${approved ? "Yes" : "No"}`, async () => {
    const x = await setup();
    x.sheets.set("output-id", { id: "output-id", name: "O'Brien Results" });
    assert.equal((await x.writer.beginCapture("output", true, callback([]))).code, "workbook.ok");
    x.state.sheetId = "output-id"; x.state.selected = { row: 6, col: 3, rows: 1, cols: 1 };
    x.state.emit("onActivated"); x.state.emit("onSelectionChanged");
    const picked = await x.writer.endCapture("output", false); assert.equal(picked.code, "workbook.ok");
    assert.equal((await x.writer.checkSource(x.pin.sourceToken)).code, "workbook.ok");
    x.intent.worksheetIdentity = "output-id"; x.intent.worksheetName = x.plan.target.worksheetName = "O'Brien Results";
    x.state.setCell(6, 3, "occupied");
    const inspection = await x.writer.prepareOutput(x.plan, x.intent, x.pin.sourceToken); assert.equal(inspection.state, 1);
    const result = await x.writer.execute(x.plan, x.intent, { inspection, approved });
    assert.equal(result.state, approved ? 0 : 1); assert.equal(x.state.mutations > 0, approved);
    assert.equal(x.cells.get("sheet-1:1:1").value, "Group 1"); assert.equal(x.state.added, 0); await x.writer.dispose();
});
for (const change of ["content", "rename", "document"])
test(`output capture cannot conceal source ${change}`, async () => {
    const x = await setup(); await x.writer.beginCapture("output", true, callback([]));
    if (change === "content") x.state.setCell(2, 1, 987);
    if (change === "rename") x.sheets.get("sheet-1").name = "Changed";
    if (change === "document") x.state.document = { mode: "readWrite" };
    x.state.selected = { row: 9, col: 9, rows: 1, cols: 1 }; x.state.emit("onSelectionChanged");
    assert.notEqual((await x.writer.endCapture("output", false)).code, "workbook.ok");
    assert.notEqual((await x.writer.checkSource(x.pin.sourceToken)).code, "workbook.ok"); assert.equal(x.state.mutations, 0); await x.writer.dispose();
});
test("arming output invalidates earlier inspection and ordinary selection after Finish is stale", async () => {
    const x = await setup(); await x.writer.prepareOutput(x.plan, x.intent, x.pin.sourceToken);
    await x.writer.beginCapture("output", true, callback([]));
    x.state.selected = { row: 10, col: 10, rows: 1, cols: 1 }; x.state.emit("onSelectionChanged"); await x.writer.endCapture("output", false);
    assert.equal((await x.writer.execute(x.plan, x.intent, null)).state, 2);
    x.state.emit("onSelectionChanged"); assert.notEqual((await x.writer.checkSource(x.pin.sourceToken)).code, "workbook.ok");
    assert.equal(x.state.mutations, 0); await x.writer.dispose();
});
test("selection moving between Finish capture and lease rebind rejects instead of accepting stale output", async () => {
    const x = await setup(); await x.writer.beginCapture("output", true, callback([]));
    x.state.selected = { row: 10, col: 10, rows: 1, cols: 1 }; x.state.emit("onSelectionChanged");
    const before = x.state.syncs;
    x.state.hook = async n => { if (n === before + 3) x.state.selected.row = 11; };
    assert.notEqual((await x.writer.endCapture("output", false)).code, "workbook.ok");
    assert.equal(x.state.mutations, 0); await x.writer.dispose();
});
test("invalidation while armed detaches capture handler and late events cannot update it", async () => {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope), replies = [];
    await writer.beginCapture("input", false, callback(replies)); assert.equal(x.state.listenerCount(), 1);
    writer.invalidate(); await settled(() => x.state.listenerCount() === 0);
    x.state.emit("onSelectionChanged"); assert.equal(replies.length, 0); assert.equal(x.state.mutations, 0); await writer.dispose();
    const y = pilotWorkbook(), delayed = createProductionWriter(y.scope);
    let release; const gate = new Promise(r => { release = r; });
    y.state.hook = async n => { if (n === 1) await gate; };
    const pending = delayed.beginCapture("late", false, callback([]));
    await settled(() => y.state.listenerCount() === 1);
    delayed.invalidate(); release();
    assert.notEqual((await pending).code, "workbook.ok");
    await settled(() => y.state.listenerCount() === 0); await delayed.dispose();
});

test("selection-acquired source token keeps refreshed textbox bound to the original native sheet", async () => {
    const x = await setup();
    x.sheets.delete("sheet-1"); x.sheets.set("replacement", {id:"replacement",name:"Input"});
    assert.notEqual((await x.writer.resolveReference(ref(), x.pin.sourceToken)).code,"workbook.ok");
    assert.equal(x.state.mutations,0); await x.writer.dispose();
});
