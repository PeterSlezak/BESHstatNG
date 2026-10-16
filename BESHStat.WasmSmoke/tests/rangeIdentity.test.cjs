"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const path = require("node:path");

const source = fs.readFileSync(path.join(__dirname, "../wwwroot/js/officeBridge.js"), "utf8");

function workbookMock() {
    const calls = { runs: 0, syncs: 0, active: 0, getItems: [], getRanges: [] };
    function sheet(name, id) {
        const obj = { name, id, load() {}, getRange(address) { calls.getRanges.push({ name, address }); return range(obj, address); } };
        return obj;
    }
    function range(worksheet, address) {
        return {
            worksheet, address: `'${worksheet.name}'!${address}`, rowIndex: 3, columnIndex: 2,
            rowCount: 3, columnCount: 2, load() {},
            values: [["A", "B"], [1, 2], [3, 4]],
            valueTypes: [["String", "String"], ["Double", "Double"], ["Double", "Double"]]
        };
    }
    const sheetA = sheet("Active", "active-id");
    const sheetB = sheet("Study Data", "stable-id");
    const sheetC = sheet("Follow up", "followup-id");
    const workbook = {
        worksheets: {
            getActiveWorksheet() { calls.active++; return sheetA; },
            getItem(name) { calls.getItems.push(name); const found = [sheetA, sheetB, sheetC].find(s => s.name === name); if (!found) throw new Error("Worksheet missing"); return found; }
        },
        getSelectedRange() { return range(sheetB, "C4:D6"); }
    };
    const mock = {
        window: {},
        Office: {
            context: { host: "Excel", platform: "OfficeOnline", document: { url: "https://contoso.example/Book1.xlsx" },
                requirements: { isSetSupported: () => true } }, HostType: { Excel: "Excel" }
        },
        Excel: { run: async fn => { calls.runs++; return fn({ workbook, sync: async () => { calls.syncs++; } }); } },
        setTimeout, decodeURIComponent
    };
    vm.runInNewContext(source, mock, { filename: "officeBridge.js" });
    return { bridge: mock.window.beshOffice, calls };
}

const selection = (worksheetName, address, extra = {}) => ({ worksheetName, address, ...extra });

test("selected range uses range.worksheet rather than active worksheet", async () => {
    const { bridge, calls } = workbookMock();
    const data = await bridge.getSelectedRangeData();
    assert.equal(data.worksheetName, "Study Data");
    assert.equal(data.worksheetId, "stable-id");
    assert.equal(data.address, "C4:D6");
    assert.equal(data.startRow, 4);
    assert.equal(data.startColumn, 3);
    assert.equal(data.workbookName, "Book1.xlsx");
    assert.equal(data.workbookId, "");
    assert.equal(data.rows[1][0].numberValue, 1);
    assert.equal(calls.active, 0);
    assert.equal(calls.syncs, 1);
});

test("named worksheet read does not activate the worksheet", async () => {
    const { bridge, calls } = workbookMock();
    const result = await bridge.getRangeDataByReference(selection("Follow up", "C4:D6"));
    assert.equal(result.worksheetId, "followup-id");
    assert.equal(result.worksheetName, "Follow up");
    assert.deepEqual(calls.getItems, ["Follow up"]);
    assert.equal(calls.active, 0);
    assert.equal(calls.runs, 1);
});

test("multi-worksheet read batches all range snapshots in one Excel.run", async () => {
    const { bridge, calls } = workbookMock();
    const data = await bridge.getRangeSetData({ selectionId: "s1", ranges: [selection("Study Data", "C4:D6"), selection("Follow up", "C4:D6")] });
    assert.equal(data.selectionId, "s1");
    assert.equal(data.ranges.length, 2);
    assert.equal(data.ranges[0].worksheetId, "stable-id");
    assert.equal(data.ranges[1].worksheetId, "followup-id");
    assert.equal(calls.runs, 1);
    assert.equal(calls.syncs, 1);
});

test("stale worksheet identity is rejected after acquisition", async () => {
    const { bridge } = workbookMock();
    await assert.rejects(bridge.getRangeDataByReference(selection("Study Data", "C4:D6", { worksheetId: "old-id" })), /identity changed/);
});

test("unverified workbook reference fails closed before Excel.run", async () => {
    const { bridge, calls } = workbookMock();
    await assert.rejects(bridge.getRangeDataByReference(selection("Study Data", "C4:D6", { workbookId: "foreign" })), /cannot be verified/);
    assert.equal(calls.runs, 0);
});

test("workbook name mismatch fails closed before Excel.run", async () => {
    const { bridge, calls } = workbookMock();
    await assert.rejects(bridge.getRangeDataByReference(selection("Study Data", "C4:D6", { workbookName: "Other.xlsx" })), /not the current/);
    assert.equal(calls.runs, 0);
});

test("qualified address in worksheetName is rejected before Excel.run", async () => {
        const { bridge, calls } = workbookMock();
        await assert.rejects(
                bridge.getRangeDataByReference(selection("'Study Data'!C4:D6", "C4:D6")),
                /worksheetName field must contain only the sheet name/
            );
        assert.equal(calls.runs, 0);
    });

test("invalid A1 and empty range sets fail without Excel.run", async () => {
    const { bridge, calls } = workbookMock();
    await assert.rejects(bridge.getRangeDataByReference(selection("Study Data", "A1,C3")), /rectangular/);
    await assert.rejects(bridge.getRangeDataByReference(selection("Study Data", "XFE1")), /Excel bounds/);
    await assert.rejects(bridge.getRangeDataByReference(selection("Study Data", "D5:A1")), /reversed endpoints/);
    await assert.rejects(bridge.getRangeSetData({ ranges: [] }), /nonempty/);
    assert.equal(calls.runs, 0);
});
