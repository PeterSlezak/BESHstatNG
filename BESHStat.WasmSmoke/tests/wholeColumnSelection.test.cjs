"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const script = fs.readFileSync(path.join(__dirname, "../wwwroot/js/officeBridge.js"), "utf8");

function mockBridge(options = {}) {
    const calls = { syncs: 0, runs: 0, selectionUsedCalls: 0, valueLoads: [], currentSheetRequests: 0 };
    const sheet = { name: "Hárok1", id: "sheet-1", load() {} };

    function range(address, rowIndex, columnIndex, rows, columns, values) {
        const obj = {
            worksheet: sheet,
            address,
            rowIndex, columnIndex,
            rowCount: rows, columnCount: columns,
            isNullObject: false,
            load(names) {
                const properties = Array.isArray(names) ? names : [names];
                if (properties.includes("values")) {
                    calls.valueLoads.push(address);
                    if (rows * columns > 10000) {
                        throw new Error("MOCK RESOURCE LIMIT: oversized cell matrices were requested");
                    }
                }
            }
        };
        if (values) {
            obj.values = values;
            obj.valueTypes = values.map(row => row.map(value =>
                value === null ? "Empty" : typeof value === "string" ? "String" : "Double"));
            obj.numberFormat = values.map(row => row.map(() => "General"));
        }
        return obj;
    }

    const populated = options.populated || range("'Hárok1'!E1:F4", 0, 4, 4, 2,
        [["Value", "GRP"], [1, "A"], [2, "A"], [3, "B"]]);
    const selected = options.selected || range("'Hárok1'!E:F", 0, 4, 1048576, 2);
    selected.getUsedRangeOrNullObject = () => { calls.selectionUsedCalls++; return populated; };
    selected.getUsedRange = () => { calls.selectionUsedCalls++; return populated; };
    const workbook = {
        getSelectedRange() { return selected; },
        worksheets: {
            getItem(name) {
                calls.currentSheetRequests++;
                if (name !== sheet.name) throw new Error("Unexpected worksheet");
                return sheet;
            }
        }
    };
    sheet.getRange = address => options.explicit || range(`'Hárok1'!${address}`, 0, 0, 3, 2,
        [["A", "B"], [1, 2], [3, 4]]);
    const globals = {
        window: {},
        Office: {
            context: {
                host: "Excel", platform: "OfficeOnline", document: { url: "https://contoso/Book.xlsx" },
                requirements: { isSetSupported: (name, version) => name === "ExcelApi" &&
                    (version === "1.1" || (version === "1.4" && options.supports14 !== false)) }
            },
            HostType: { Excel: "Excel" }
        },
        Excel: { run: async execute => {
            calls.runs++;
            return execute({ workbook, sync: async () => { calls.syncs++; } });
        } },
        setTimeout, decodeURIComponent
    };
    vm.runInNewContext(script, globals, { filename: "officeBridge.js" });
    return { bridge: globals.window.beshOffice, calls, range };
}

test("whole-column selection is trimmed to cells in the selected columns before values load", async () => {
    const { bridge, calls } = mockBridge();
    const actual = await bridge.getSelectedRangeData();
    assert.equal(actual.address, "E1:F4");
    assert.equal(actual.startRow, 1);
    assert.equal(actual.startColumn, 5);
    assert.equal(actual.rowCount, 4);
    assert.equal(actual.columnCount, 2);
    assert.equal(actual.rows[3][1].textValue, "B");
    assert.equal(calls.selectionUsedCalls, 1);
    assert.deepEqual(calls.valueLoads, ["'Hárok1'!E1:F4"]);
    assert.equal(calls.syncs, 3); // selection dimensions, used dimensions, bounded values
});

test("ordinary rectangular selections retain their blank edges", async () => {
    const sample = mockBridge();
    const small = sample.range("'Hárok1'!C3:D6", 2, 2, 4, 2,
        [[null, null], ["Group A", "Group B"], [1, 2], [null, null]]);
    const { bridge, calls } = mockBridge({ selected: small });
    const actual = await bridge.getSelectedRangeData();
    assert.equal(actual.startRow, 3);
    assert.equal(actual.rowCount, 4);
    assert.equal(actual.rows[0][0].kind, 0);
    assert.equal(actual.rows[3][0].kind, 0);
    assert.equal(calls.selectionUsedCalls, 0);
    assert.equal(calls.syncs, 2);
});

test("whole-column selection with no populated cells shows a clear error", async () => {
    const { range } = mockBridge();
    const empty = range("'Hárok1'!E:F", 0, 4, 1048576, 2);
    const missing = range("'Hárok1'!E:F", 0, 4, 0, 0);
    missing.isNullObject = true;
    const { bridge, calls } = mockBridge({ selected: empty, populated: missing });
    await assert.rejects(bridge.getSelectedRangeData(), /contain no populated cells/);
    assert.deepEqual(calls.valueLoads, []);
});

test("whole-column selection uses the older scoped used range when ExcelApi 1.4 unavailable", async () => {
    const { bridge, calls } = mockBridge({ supports14: false });
    const actual = await bridge.getSelectedRangeData();
    assert.equal(actual.rowCount, 4);
    assert.equal(calls.selectionUsedCalls, 1);
});

test("large populated selections fail before loading payload matrices", async () => {
    const { range } = mockBridge();
    const huge = range("'Hárok1'!E1:F7000", 0, 4, 7000, 2);
    const { bridge, calls } = mockBridge({ populated: huge });
    await assert.rejects(bridge.getSelectedRangeData(), /at most 10000 cells/);
    assert.deepEqual(calls.valueLoads, []);
});

test("oversized bounded selections are rejected rather than silently trimmed", async () => {
    const { range } = mockBridge();
    const large = range("'Hárok1'!E1:F7000", 0, 4, 7000, 2);
    const { bridge, calls } = mockBridge({ selected: large });
    await assert.rejects(bridge.getSelectedRangeData(), /at most 10000 cells/);
    assert.equal(calls.selectionUsedCalls, 0);
    assert.deepEqual(calls.valueLoads, []);
});

test("oversized explicit range fails before loading cell matrices", async () => {
    const { range } = mockBridge();
    const large = range("'Hárok1'!A1:B7000", 0, 0, 7000, 2);
    const { bridge, calls } = mockBridge({ explicit: large });
    await assert.rejects(bridge.getRangeDataByReference({ worksheetName: "Hárok1", address: "A1:B7000" }), /at most 10000 cells/);
    assert.deepEqual(calls.valueLoads, []);
});

test("a multi-range read rejects oversized aggregate before reading any values", async () => {
    const { range } = mockBridge();
    const first = range("'Hárok1'!A1:B4000", 0, 0, 4000, 2);
    const { bridge, calls } = mockBridge({ explicit: first });
    await assert.rejects(bridge.getRangeSetData({ ranges: [
        { worksheetName: "Hárok1", address: "A1:B4000" },
        { worksheetName: "Hárok1", address: "C1:D4000" }
    ] }), /range set contains 16000 cells/);
    assert.deepEqual(calls.valueLoads, []);
});
