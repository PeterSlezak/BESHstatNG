import test from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { createProductionWriter } from "../wwwroot/js/productionWriter.mjs";
import { createWorkbookTransport } from "../wwwroot/js/workbookAdapter.mjs";
import { pilotWorkbook } from "./helpers/pilotWorkbook.mjs";

const fixture = JSON.parse(readFileSync(new URL("../../BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json", import.meta.url), "utf8"));
async function setup() {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope);
    const transport = createWorkbookTransport(x.scope, { writer });
    x.state.setCell(1, 5, "Category"); x.state.setCell(1, 10, "Values");
    for (let r = 2; r <= 10; r++) { x.state.setCell(r, 5, `Group ${Math.floor((r - 2) / 3)}`); x.state.setCell(r, 10, r); }
    const sources = [];
    for (const startColumn of [5, 10]) {
        const reference = { workbookId: "", workbookName: "", worksheetId: "sheet-1", worksheetName: "Input",
            addressDialect: 1, address: `${startColumn === 5 ? "E1:E10" : "J1:J10"}`, startRow: 1, startColumn, rowCount: 10, columnCount: 1 };
        const resolved = await transport.resolveReference(reference);
        assert.equal(resolved.code, "workbook.ok");
        const id = `read-${startColumn}`;
        const begin = await transport.beginRead(id, undefined, resolved.reference, false);
        assert.equal(begin.code, "workbook.ok");
        const tile = await transport.readNext(id); assert.equal(tile.code, "workbook.ok"); assert.equal(tile.done, true);
        assert.equal((await transport.finishRead(id)).code, "workbook.ok");
        sources.push({ snapshot: { ...begin.metadata, rows: tile.rows }, handle: resolved.handle });
    }
    const callbacks = [];
    const pin = await transport.pinSources(sources, { invokeMethodAsync: async (...args) => { callbacks.push(args); } });
    assert.equal(pin.code, "workbook.ok");
    const plan = structuredClone(fixture); plan.target.worksheetName = "Results"; plan.target.destination = 1;
    for (const table of plan.tables) table.autoFit = false;
    const intent = { planIdentity: "plan", targetIdentity: "target", workbookIdentity: pin.sessionIdentity,
        worksheetIdentity: null, worksheetName: "Results", destination: 1, startRow: plan.target.startRow, startColumn: plan.target.startColumn,
        regions: plan.tables.map(t => ({ artifactId: t.artifactId, bounds: t.bounds, mayMutate: true })), reservations: [], guards: [] };
    const inspect = () => transport.prepareOutput(plan, intent, pin.sourceToken);
    return { ...x, writer, transport, sources, callbacks, pin, plan, intent, inspect };
}

test("composite transport reads only E/J, pins both, writes verified new sheet then accepts another independent source", async () => {
    const x = await setup();
    assert.equal((await x.transport.checkSource(x.pin.sourceToken)).code, "workbook.ok");
    assert.equal((await x.inspect()).state, 0);
    const untouched = structuredClone([...x.cells.entries()]);
    assert.equal((await x.transport.write(x.plan, x.intent)).code, "output.completed");
    for (const [k, value] of untouched) assert.deepEqual(x.cells.get(k), value);
    assert.equal(x.state.listenerCount(), 0);
    const fresh = await x.transport.pinSource(x.state.snapshot()); assert.equal(fresh.code, "workbook.ok");
    assert.notEqual(fresh.sourceToken, x.pin.sourceToken);
    await x.transport.dispose(); assert.equal(x.state.listenerCount(), 0);
});

for (const column of [5, 10]) for (const defect of ["contents", "formula", "rename", "delete", "replace", "workbook", "selection", "cancel", "dispose"])
test(`either physical source ${column}: ${defect} retires composite lease and cannot use old consent`, async () => {
    const x = await setup(); await x.inspect(); const before = x.state.mutations;
    if (defect === "contents") x.state.setCell(3, column, column === 5 ? "Other" : 999);
    if (defect === "formula") { const cell = x.cells.get(`sheet-1:3:${column}`); cell.formula = "=1+2"; }
    if (defect === "rename") x.sheets.get("sheet-1").name = "Renamed";
    if (defect === "delete") x.sheets.delete("sheet-1");
    if (defect === "replace") { x.sheets.delete("sheet-1"); x.sheets.set("new-id", { id: "new-id", name: "Input" }); x.state.sheetId = "new-id"; }
    if (defect === "workbook") x.state.document = { mode: "readWrite" };
    if (defect === "selection") x.state.selected = { row: 4, col: column, rows: 1, cols: 1 };
    if (defect === "cancel") x.transport.invalidate();
    if (defect === "dispose") await x.transport.dispose();
    assert.notEqual((await x.transport.checkSource(x.pin.sourceToken)).code, "workbook.ok");
    assert.notEqual((await x.transport.write(x.plan, x.intent)).state, 0);
    assert.equal(x.state.mutations, before); await x.transport.dispose();
});

for (const column of [5, 10]) test(`output overlapping physical source column ${column} rejects without bounding-rectangle guessing`, async () => {
    const x = await setup();
    // Preserve the output fixture shape while placing it on one real source column.
    const shift = column - x.plan.tables[0].bounds.startColumn;
    x.plan.target.destination = x.intent.destination = 0; x.plan.target.worksheetName = x.intent.worksheetName = "Input";
    x.intent.worksheetIdentity = "sheet-1";
    for (const table of x.plan.tables) { table.bounds.startColumn += shift; table.bounds.endColumn += shift; table.bounds.startRow -= 5; table.bounds.endRow -= 5; }
    x.plan.target.startRow = x.intent.startRow = 1; x.plan.target.startColumn += shift; x.intent.startColumn = x.plan.target.startColumn;
    x.intent.regions = x.plan.tables.map(t => ({ artifactId: t.artifactId, bounds: t.bounds, mayMutate: true }));
    assert.equal((await x.inspect()).code, "output.target-invalid"); assert.equal(x.state.mutations, 0);
    await x.transport.dispose();
});

for (const defect of ["rows", "sheet", "duplicate", "rectangle", "foreign-handle", "total-cells"])
test(`composite pin rejects ${defect} before output`, async () => {
    const x = await setup(); const sources = structuredClone(x.sources), b = sources[1].snapshot;
    if (defect === "rows") b.startRow++;
    if (defect === "sheet") b.worksheetId = "other-sheet";
    if (defect === "duplicate") sources[1] = structuredClone(sources[0]);
    if (defect === "rectangle") b.columnCount = 2;
    if (defect === "foreign-handle") { x.state.document = { mode: "readWrite" }; }
    if (defect === "total-cells") {
        // Get genuine oversized role handles, rather than letting mismatched handles mask the size check.
        for (let i = 0; i < 2; i++) {
            const col = i ? "J" : "E";
            const reply = await x.transport.resolveReference({ ...sources[i].snapshot, addressDialect: 1, address: `${col}1:${col}6000`, rowCount: 6000 });
            assert.equal(reply.code, "workbook.ok"); sources[i].handle = reply.handle;
            sources[i].snapshot.rowCount = 6000; sources[i].snapshot.rows = Array.from({ length: 6000 }, () => [x.sources[i].snapshot.rows[1][0]]);
        }
    }
    const reply = await x.transport.pinSources(sources);
    assert.notEqual(reply.code, "workbook.ok"); if (defect === "total-cells") assert.equal(reply.code, "input.total-cells");
    assert.equal(x.state.mutations, 0); await x.transport.dispose();
});

test("uncertain composite output remains latched after invalidation, reset-equivalent state and new pin", async () => {
    const x = await setup(); await x.inspect(); x.state.rejectCommit = true;
    const outcome = await x.transport.write(x.plan, x.intent);
    assert.deepEqual([outcome.state, outcome.code, outcome.mutationMayHaveOccurred], [4, "output.mutation-uncertain", true]);
    const before = x.state.mutations; x.transport.invalidate(); x.transport.clearOutput();
    assert.equal((await x.transport.pinSources(x.sources)).code, "output.mutation-uncertain");
    assert.equal((await x.transport.captureSelection(false)).code, "output.mutation-uncertain");
    assert.equal(x.state.mutations, before); await x.transport.dispose();
});

test("composite verification applies a cumulative formula-byte budget to both physical ranges", async () => {
    const x = await setup(), sources = [], formula = '=' + 'x'.repeat(32766);
    for (const column of [5, 10]) {
        x.state.selected = { row: 1, col: column, rows: 510, cols: 1 };
        for (let r = 1; r <= 510; r++) x.state.setCell(r, column, column === 5 ? 'Group' : r, column === 5 ? 'String' : 'Double', formula);
        const captured = await x.transport.captureSelection(false);
        assert.equal(captured.code, "workbook.ok");
        sources.push({ snapshot: x.state.snapshot(), handle: captured.handle });
    }
    const pin = await x.transport.pinSources(sources);
    assert.equal(pin.code, "input.estimated-bytes"); assert.equal(x.state.mutations, 0);
    await x.transport.dispose();
});
