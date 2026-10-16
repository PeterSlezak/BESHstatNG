"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const path = require("node:path");
const source = fs.readFileSync(path.join(__dirname, "../wwwroot/js/officeBridge.js"), "utf8");

function makeBridge(value, kind, format = "General") {
    const calls = { runs: 0, sync: 0 };
    const sheet = { id: "sheet-1", name: "Data", load() {} };
    const range = {
        worksheet: sheet, address: "'Data'!A1", rowIndex: 0, columnIndex: 0,
        rowCount: 1, columnCount: 1, load(props) { calls.loaded = props; },
        values: [[value]], valueTypes: [[kind]], numberFormat: [[format]]
    };
    const sandbox = {
        window: {},
        Office: { HostType: { Excel: "Excel" }, context: { host: "Excel", platform: "OfficeOnline", document: { url: "https://example.test/Book.xlsx" }, requirements: { isSetSupported: () => true } } },
        Excel: { run: async callback => { calls.runs++; return callback({ workbook: { getSelectedRange: () => range }, sync: async () => { calls.sync++; } }); } },
        setTimeout, decodeURIComponent
    };
    vm.runInNewContext(source, sandbox, { filename: "officeBridge.js" });
    return { bridge: sandbox.window.beshOffice, calls };
}

test("Office typed errors survive even when their value resembles text", async () => {
    const { bridge } = makeBridge("#N/A", "Error");
    const data = await bridge.getSelectedRangeData();
    assert.equal(data.rows[0][0].kind, 4);
    assert.equal(data.rows[0][0].textValue, "#N/A");
    assert.equal(data.dateSystem, 0);
});

test("Literal #N/A stays text, not an Excel error", async () => {
    const data = await makeBridge("#N/A", "String").bridge.getSelectedRangeData();
    assert.equal(data.rows[0][0].kind, 2);
});

test("Blank and typed empty text remain distinct", async () => {
    const blank = await makeBridge("", "Empty").bridge.getSelectedRangeData();
    const typed = await makeBridge("", "String").bridge.getSelectedRangeData();
    assert.equal(blank.rows[0][0].kind, 0);
    assert.equal(typed.rows[0][0].kind, 2);
    assert.equal(typed.rows[0][0].textValue, "");
});

test("Number format travels with date-like numeric cell but stays numeric", async () => {
    const { bridge, calls } = makeBridge(45352, "Double", "yyyy-mm-dd");
    const data = await bridge.getSelectedRangeData();
    assert.equal(data.rows[0][0].kind, 1);
    assert.equal(data.rows[0][0].numberValue, 45352);
    assert.equal(data.rows[0][0].numberFormat, "yyyy-mm-dd");
    assert.equal(data.dateSystem, 0);
    assert.ok(calls.loaded.includes("numberFormat"));
});

test("Boolean, zero and nonfinite values retain distinct typed semantics", async () => {
    const boolean = await makeBridge(false, "Boolean").bridge.getSelectedRangeData();
    const zero = await makeBridge(0, "Double").bridge.getSelectedRangeData();
    const nonfinite = await makeBridge(Infinity, "Double").bridge.getSelectedRangeData();
    assert.equal(boolean.rows[0][0].kind, 3);
    assert.equal(boolean.rows[0][0].booleanValue, false);
    assert.equal(zero.rows[0][0].kind, 1);
    assert.equal(zero.rows[0][0].numberValue, 0);
    assert.equal(nonfinite.rows[0][0].kind, 4);
});
