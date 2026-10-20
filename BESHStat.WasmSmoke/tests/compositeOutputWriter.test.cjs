"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
async function assertWriteOutcome(promise, expected) {
    const result = await promise;
    assert.ok(["RejectedBeforeWrite", "FailedBeforeWrite", "PartiallyWritten"].includes(result.state), JSON.stringify(result));
    if (typeof expected === "string") assert.equal(result.state, expected);
    else if (expected) assert.match(result.message, expected);
    assert.equal(result.mutationMayHaveOccurred, result.state === "PartiallyWritten");
    assert.deepEqual(Array.from(result.confirmedWritten), []);
    for (const key of ["undoSupported", "rollbackAttempted", "rollbackSucceeded"]) assert.equal(result[key], false);
}
const fs = require("node:fs");
const path = require("node:path");
const { compositeWorkbook } = require("./helpers/compositeWorkbook.cjs");
const copy = value => JSON.parse(JSON.stringify(value));
const fixture = () => JSON.parse(fs.readFileSync(path.join(__dirname, "fixtures/composite-output/composite-c10.json"), "utf8"));
const addresses = ["C10:H13", "C15:H15", "C17:H24", "C26:H26", "C28:H28", "C30:E31"];

test("actual Common composite wire writes all kinds at C10 including second table at C30", async () => {
    const plan = fixture(), mock = compositeWorkbook();
    const result = await mock.bridge.writeCompositeOutputPlan(plan);
    assert.equal(result.documentId, plan.documentId);
    assert.deepEqual(Array.from(result.itemAddresses, p => p.address), addresses.map(a => "HOST:Composite!" + a));
    assert.equal(result.address, "HOST:Composite!C10:H31");
    assert.deepEqual(mock.trace.writes.map(w => w.indexes), [[9, 2, 4, 6], [14, 2, 1, 1], [16, 2, 1, 1], [25, 2, 1, 1], [27, 2, 1, 1], [29, 2, 2, 3]]);
    assert.deepEqual(mock.trace.merges, addresses.slice(1, 5));
    assert.deepEqual(mock.trace.adds, ["Composite"]);
    const sheet = mock.sheets.get("Composite");
    assert.match(sheet.cellAt(14, 2).value, /^WARNING \[demo.warning\]/);
    assert.match(sheet.cellAt(16, 2).value, /^CHART PLACEHOLDER \(not rendered\)/);
    assert.equal(sheet.cellAt(27, 2).value, "Example report [link not active: https://example.org/report]");
    assert.equal(sheet.cellAt(11, 6).kind, "text");
    assert.equal(sheet.cellAt(11, 6).value, "=1+1");
    assert.equal(sheet.cellAt(11, 7).value, 0.01);
    assert.equal(sheet.cellAt(11, 7).style["font.color"], "#32FF32");
    assert.equal(sheet.cellAt(11, 7).style.numberFormat, plan.tablePlan.tables[0].cells[17].numberFormat);
    assert.match(sheet.cellAt(12, 2).value, /^Embedded footer/);
    for (const row of [13, 15, 24, 26, 28]) for (let c = 2; c < 8; c++) assert.equal(sheet.cells.has(`${row}:${c}`), false, "gaps untouched");
    assert.equal(mock.oldCells.get("9:2").value, "Existing workbook data");
    assert.deepEqual(Array.from(result.degraded, p => ({ ...p })), [{ id: "plot", state: "placeholder-not-rendered" }, { id: "link", state: "linked-text-only" }]);
    assert.ok(mock.trace.syncs <= 2, "no sync per item or cell");
});

const invalid = {
    "schema": p => { p.schemaVersion = 2; },
    "missing kind": p => { delete p.items[2].kind; },
    "unknown kind": p => { p.items[2].kind = "NativeChart"; },
    "unknown field": p => { p.extra = true; },
    "missing document ID": p => { p.documentId = ""; },
    "duplicate ID": p => { p.items[3].id = "main"; },
    "source order": p => { p.items[2].sourceIndex = 3; },
    "source ID": p => { p.items[2].artifactId = "other"; },
    "table order": p => { p.tablePlan.tables.reverse(); },
    "table coordinates ignored": p => { p.tablePlan.tables[1].bounds.startRow--; p.tablePlan.tables[1].bounds.endRow--; },
    "missing source": p => { p.items.splice(2, 1); },
    "item order": p => { p.items[4].index = 3; },
    "missing anchor": p => { p.items[3].relatedArtifactId = "absent"; },
    "plot logical key": p => { p.items[2].referenceKey = "changed"; },
    "plot caption": p => { p.items[2].caption = "changed"; },
    "severity": p => { p.items[1].severity = "Info"; },
    "warning text": p => { p.items[1].text = "only a color warning"; },
    "diagnostic path": p => { p.items[1].fieldPath = "other"; },
    "diagnostic message": p => { p.items[1].message = "other"; },
    "rendering claim": p => { p.items[2].state = "rendered"; },
    "active link claim": p => { p.items[4].state = "hyperlink"; },
    "overlapping later table": p => { p.items[5].bounds.startRow = 24; p.items[5].bounds.endRow = 25; },
    "block overlap": p => { p.items[3].bounds.startRow = 24; p.items[3].bounds.endRow = 24; },
    "reserved block": p => { p.reservations.push(copy(p.items[2].bounds)); },
    "reservation outside grid": p => { p.reservations.push({ startRow: 1, endRow: 1048577, startColumn: 1, endColumn: 1 }); },
    "null reservation": p => { p.reservations.push(null); },
    "null item": p => { p.items[3] = null; },
    "huge text": p => { p.items[3].text = "x".repeat(32768); },
    "control text": p => { p.items[3].text = "bad\u0000text"; },
    "zero block": p => { p.items[3].rowCount = 0; },
    "negative dimensions": p => { p.items[3].rowCount = -1; },
    "huge dimensions": p => { p.items[3].rowCount = 2147483647; },
    "too many cells": p => { p.items[3].rowCount = 10001; p.items[3].columnCount = 1; },
    "end outside grid": p => { p.items[2].bounds.endRow = 1048577; },
    "fractional coordinate": p => { p.items[2].bounds.startRow = 17.5; },
    "wrong envelope": p => { p.usedBounds.endRow = 32; },
    "wrong target": p => { p.target.startColumn = 4; },
    "current sheet rejected": p => { p.target.destination = 0; },
    "new workbook rejected": p => { p.target.destination = 2; },
    "table target mismatch": p => { p.tablePlan.target.worksheetName = "Other"; },
    "autofit expands placeholder footprint": p => { p.tablePlan.tables[0].autoFit = true; },
    "table cell formula type": p => { p.tablePlan.tables[0].cells[0].value.kind = "formula"; },
    "unsafe javascript": p => { p.items[4].linkTarget = "javascript:alert(1)"; },
    "unsafe data": p => { p.items[4].linkTarget = "data:text/html,hello"; },
    "unsafe file": p => { p.items[4].linkTarget = "file:///C:/private"; },
    "insecure HTTP": p => { p.items[4].linkTarget = "http://example.org/report"; },
    "formula link": p => { p.items[4].linkTarget = '=HYPERLINK("https://example.org")'; },
    "credential URL": p => { p.items[4].linkTarget = "https://user:pass@example.org"; },
    "whitespace URL": p => { p.items[4].linkTarget = "https://example.org/a b"; },
    "trailing newline URL": p => { p.items[4].linkTarget = "https://example.org\n"; },
    "port URL": p => { p.items[4].linkTarget = "https://example.org:443/report"; },
    "invalid host": p => { p.items[4].linkTarget = "https://."; },
    "invalid DNS label": p => { p.items[4].linkTarget = "https://-bad.example/report"; },
    "huge item count": p => { p.items = Array(65).fill(p.items[0]); }
};
for (const [name, mutate] of Object.entries(invalid)) test(`invalid composite ${name} has zero host mutations`, async () => {
    const plan = fixture(), mock = compositeWorkbook(); mutate(plan);
    await assertWriteOutcome(mock.bridge.writeCompositeOutputPlan(plan));
    assert.equal(mock.trace.runs, 0); assert.equal(mock.trace.syncs, 0);
    assert.deepEqual(mock.trace.adds, []); assert.deepEqual(mock.trace.writes, []);
});

test("separator reservations do not own the envelope gaps", async () => {
    const plan = fixture(), mock = compositeWorkbook();
    plan.reservations = [{ startRow: 14, endRow: 14, startColumn: 3, endColumn: 12 }];
    await mock.bridge.writeCompositeOutputPlan(plan);
    assert.equal(mock.trace.writes.length, 6);
});

test("literal note and link display text retain formula prefixes, numeric-looking text and multiline content", async () => {
    const plan = fixture(), mock = compositeWorkbook();
    plan.items[3].text = "=SUM(1,2)\n00123\n#N/A";
    plan.items[4].text = "+1+1";
    await mock.bridge.writeCompositeOutputPlan(plan);
    const sheet = mock.sheets.get("Composite");
    assert.equal(sheet.cellAt(25, 2).kind, "text");
    assert.equal(sheet.cellAt(25, 2).value, plan.items[3].text);
    assert.equal(sheet.cellAt(27, 2).kind, "text");
    assert.match(sheet.cellAt(27, 2).value, /^\+1\+1 \[link not active/);
});

test("validated composite snapshot survives mutation during Office capability await", async () => {
    const plan = fixture(), mock = compositeWorkbook();
    const pending = mock.bridge.writeCompositeOutputPlan(plan);
    plan.items[3].text = "changed"; plan.items[3].bounds.startRow = 1;
    plan.items[5].bounds.startRow = 2; plan.target.destination = 0;
    await pending;
    assert.equal(mock.sheets.get("Composite").cellAt(25, 2).value, "Additional note: synthetic demonstration only.");
    assert.deepEqual(mock.trace.writes[5].indexes, [29, 2, 2, 3]);
});

test("unique worksheet naming uses accepted 02.06 acquisition", async () => {
    const mock = compositeWorkbook({ names: ["Composite", "COMPOSITE_2"] });
    const result = await mock.bridge.writeCompositeOutputPlan(fixture());
    assert.equal(result.worksheetName, "Composite_3");
});

for (const options of [{ nonExcel: true }, { apiAvailable: false }, { protectedWorkbook: true }, { missingWorkbook: true }]) {
    test(`unavailable composite destination ${JSON.stringify(options)} never writes`, async () => {
        const mock = compositeWorkbook(options);
        await assertWriteOutcome(mock.bridge.writeCompositeOutputPlan(fixture()));
        assert.deepEqual(mock.trace.adds, []); assert.deepEqual(mock.trace.writes, []);
    });
}

test("write failure propagates and leaves an explicit partial-output limitation", async () => {
    const mock = compositeWorkbook({ failWrite: 3 });
    await assertWriteOutcome(mock.bridge.writeCompositeOutputPlan(fixture()), "PartiallyWritten");
    assert.deepEqual(mock.trace.adds, ["Composite"]);
    assert.equal(mock.trace.writes.length, 3);
    assert.match(mock.sheets.get("Composite").cellAt(14, 2).value, /^WARNING/);
    assert.equal(mock.oldCells.get("9:2").value, "Existing workbook data");
});

for (const name of ["composite-secondary", "composite-secondary-link"]) test(`${name}: actual Common no-chart wire preserves nonspatial entries, multiline fault and gap reservation`, async () => {
    const plan = JSON.parse(fs.readFileSync(path.join(__dirname, "fixtures/composite-output", name + ".json"), "utf8"));
    const mock = compositeWorkbook();
    const result = await mock.bridge.writeCompositeOutputPlan(plan);
    assert.deepEqual(Array.from(result.itemAddresses, p => p.address), ["HOST:Composite!C10:E11", "HOST:Composite!C13:G14", ...(name.endsWith("-link") ? ["HOST:Composite!C16:E16"] : [])]);
    assert.equal(result.degraded.find(p => p.id === "plot").state, "nonspatial");
    assert.equal(result.degraded.find(p => p.id === "zero").state, "nonspatial");
    assert.equal(mock.trace.writes[1].matrix[0][0], "'FAULT [bad] line one\nline two");
    assert.equal(mock.sheets.get("Composite").cells.has("11:2"), false);
    assert.equal(mock.trace.merges.some(a => a.includes("C17")), false);
});

test("all nonspatial composite returns explicit skipped identity without Excel.run or a sheet", async () => {
    const plan = fixture(), mock = compositeWorkbook();
    const plot = plan.items[2];
    Object.assign(plot, { index: 0, sourceIndex: 0, bounds: null, rowCount: 0, columnCount: 0, state: "nonspatial" });
    plan.items = [plot]; plan.usedBounds = null;
    Object.assign(plan.tablePlan, { artifactCount: 1, sourceTableCount: 0, tableCount: 0, tables: [], usedBounds: null });
    const result = await mock.bridge.writeCompositeOutputPlan(plan);
    assert.equal(result.address, ""); assert.equal(result.degraded[0].id, "plot");
    assert.equal(result.degraded[0].state, "nonspatial"); assert.equal(mock.trace.runs, 0);
});

test("last Excel cell is a valid composite text block without zero-based overflow", async () => {
    const plan = fixture(), mock = compositeWorkbook();
    const bounds = { startRow: 1048576, endRow: 1048576, startColumn: 16384, endColumn: 16384 };
    const note = plan.items[3];
    Object.assign(note, { index: 0, bounds, rowCount: 1, columnCount: 1, relatedArtifactId: null });
    plan.items = [note]; plan.usedBounds = bounds;
    Object.assign(plan.target, { startRow: 1048576, startColumn: 16384 });
    Object.assign(plan.tablePlan, { target: copy(plan.target), artifactCount: 0, sourceTableCount: 0, tableCount: 0, tables: [], usedBounds: null });
    const result = await mock.bridge.writeCompositeOutputPlan(plan);
    assert.equal(result.address, "HOST:Composite!XFD1048576:XFD1048576");
    assert.deepEqual(mock.trace.writes[0].indexes, [1048575, 16383, 1, 1]);
});

function largeTextPlan(rows, notes) {
    const plan = fixture(), table = plan.tablePlan.tables[0], main = plan.items[0], template = plan.items[3];
    const text = "x".repeat(32767);
    const bounds = { startRow: 10, startColumn: 3, endRow: 9 + rows, endColumn: 3 };
    Object.assign(main, { rowCount: rows, columnCount: 1, bounds });
    Object.assign(table, { rowCount: rows, columnCount: 1, bounds,
        cells: Array.from({ length: rows }, (_, row) => ({ row, column: 0, value: { kind: "text", numberValue: null, textValue: text, booleanValue: null },
            role: "Body", numberFormat: "General", isPValue: false, isSignificant: false, strongEmphasis: false })) });
    plan.items = [main];
    Object.assign(plan.tablePlan, { artifactCount: 1, sourceTableCount: 1, tableCount: 1, tables: [table], usedBounds: bounds });
    for (let i = 0; i < notes; i++) {
        const row = bounds.endRow + 2 + i * 2;
        plan.items.push({ ...template, id: `note-${i}`, index: i + 1, text, relatedArtifactId: null, columnCount: 1,
            bounds: { startRow: row, endRow: row, startColumn: 3, endColumn: 3 } });
    }
    plan.usedBounds = { ...bounds, endRow: bounds.endRow + notes * 2 };
    return plan;
}

test("large table text has a bounded byte estimate before Excel.run", async () => {
    const mock = compositeWorkbook();
    await assertWriteOutcome(mock.bridge.writeCompositeOutputPlan(largeTextPlan(1100, 0)), /Invalid output plan: adapter byte limit/);
    assert.equal(mock.trace.runs, 0);
});

test("combined table and annotation byte estimate is bounded independently of individual limits", async () => {
    const mock = compositeWorkbook();
    await assertWriteOutcome(mock.bridge.writeCompositeOutputPlan(largeTextPlan(990, 20)), /Invalid composite output plan: (aggregate )?byte limit/);
    assert.equal(mock.trace.runs, 0); assert.deepEqual(mock.trace.adds, []);
});

test("maximum-length literal text survives the host apostrophe escape without truncation", async () => {
    const plan = fixture(), mock = compositeWorkbook(), text = "=" + "x".repeat(32766);
    plan.items[3].text = text;
    plan.tablePlan.tables[0].cells[0].value.textValue = text;
    await mock.bridge.writeCompositeOutputPlan(plan);
    const sheet = mock.sheets.get("Composite");
    assert.equal(sheet.cellAt(9, 2).value, text);
    assert.equal(sheet.cellAt(25, 2).value, text);
    assert.equal(sheet.cellAt(25, 2).value.length, 32767);
    assert.equal(sheet.cellAt(25, 2).kind, "text");
});
