"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const script = fs.readFileSync(path.join(__dirname, "../wwwroot/js/officeBridge.js"), "utf8");
const fixturePath = path.join(__dirname, "../../BESHStat.Excel.Common.Tests/TestData/Acquisition/canonical-two-ranges.json");
function camel(value) {
    if (Array.isArray(value)) return value.map(camel);
    if (value && typeof value === "object") return Object.fromEntries(Object.entries(value).map(([key, child]) =>
        [key[0].toLowerCase() + key.slice(1), camel(child)]));
    return value;
}
const fixture = () => camel(JSON.parse(fs.readFileSync(fixturePath, "utf8")));
const plain = value => JSON.parse(JSON.stringify(value));

function mock(options = {}) {
    const expected = fixture();
    if (options.sameSheet) expected.ranges[1].worksheetName = expected.ranges[0].worksheetName;
    const calls = { runs: 0, syncs: 0, valueLoads: [], areaLoads: [], selectedCalls: 0, usedCalls: 0 };
    const sheets = new Map();
    for (const item of expected.ranges) {
        if (!sheets.has(item.worksheetName)) sheets.set(item.worksheetName,
            { name: item.worksheetName, id: "office-" + item.worksheetName, load() {} });
    }
    const ranges = expected.ranges.map(item => {
        const range = {
            worksheet: sheets.get(item.worksheetName), address: `'${item.worksheetName}'!${item.address}`,
            rowCount: item.rowCount, columnCount: item.columnCount,
            rowIndex: item.startRow - 1, columnIndex: item.startColumn - 1,
            values: item.rows.map(row => row.map(cell => cell.kind === 0 ? "" :
                cell.kind === 1 ? cell.numberValue : cell.kind === 3 ? cell.booleanValue : cell.textValue)),
            valueTypes: item.rows.map(row => row.map(cell => ["Empty", "Double", "String", "Boolean", "Error"][cell.kind])),
            numberFormat: item.rows.map(row => row.map(cell => cell.numberFormat)),
            load(properties) { if (properties.includes("values")) calls.valueLoads.push(item.address); }
        };
        return range;
    });
    const selectionFailure = () => Object.assign(new Error("Raw Office selection diagnostic"),
        options.debugInfoCode ? { debugInfo: { code: "InvalidSelection" } } : { code: "InvalidSelection" });
    for (const sheet of sheets.values()) sheet.getRange = address => {
        const found = ranges.find(range => range.worksheet === sheet && range.address.endsWith("!" + address));
        if (!found) throw new Error("Unexpected rectangle " + address);
        return found;
    };
    const workbook = {
        worksheets: { getItem(name) { return sheets.get(name); }, getActiveWorksheet() { throw new Error("Do not assume active worksheet"); } },
        getSelectedRange() {
            calls.selectedCalls++;
            if (options.invalidSelection && !options.failSync) throw selectionFailure();
            if (options.disjoint) throw new Error("Multiple selections require getSelectedRanges");
            return ranges[0];
        },
        getSelectedRanges() {
            if (options.invalidSelection && !options.failSync) throw selectionFailure();
            const selected = (options.reverse ? [...ranges].reverse() : ranges).slice(0, options.areaCount ?? ranges.length);
            return { areas: { items: selected,
            load(properties) {
                calls.areaLoads.push(...properties);
                if (typeof properties === "string" || properties.some(property =>
                    property === "items" || /values|valueTypes|numberFormat/.test(property))) {
                    throw new Error("Area payload requested before size preflight");
                }
            } } }; }
    };
    const globals = {
        window: {},
        Office: { HostType: { Excel: "Excel" }, context: { host: "Excel", document: { url: "https://example.test/Book1.xlsx" },
            requirements: { isSetSupported: (name, version) => version !== "1.9" || options.supports19 !== false } } },
        Excel: { run: async fn => { calls.runs++; return fn({ workbook, sync: async () => {
            calls.syncs++;
            if (options.invalidSelection && options.failSync) throw selectionFailure();
        } }); } },
        setTimeout, decodeURIComponent
    };
    vm.runInNewContext(script, globals, { filename: "officeBridge.js" });
    const references = expected.ranges.map(item => ({ worksheetName: item.worksheetName, worksheetId: "office-" + item.worksheetName,
        workbookName: item.workbookName, address: item.address, addressDialect: 1,
        startRow: item.startRow, startColumn: item.startColumn, rowCount: item.rowCount, columnCount: item.columnCount,
        selectionId: item.selectionId }));
    return { bridge: globals.window.beshOffice, calls, ranges, expected, references };
}

test("Office two-range acquisition matches the same canonical fixture as Windows and Common", async () => {
    const { bridge, calls, expected, references } = mock();
    const actual = plain(await bridge.getRangeSetData({ selectionId: expected.selectionId, ranges: references }));
    actual.ranges.forEach(range => { range.worksheetId = ""; }); // host-scoped IDs are compared separately below
    assert.deepEqual(actual, expected);
    assert.equal(calls.runs, 1);
    assert.equal(calls.syncs, 3); // metadata, then one bounded response per area
    assert.deepEqual(calls.valueLoads, ["C4:F5", "B9:C10"]);
});

test("Office selected disjoint areas keep host order and separate rectangles on one sheet", async () => {
    const { bridge, calls } = mock({ sameSheet: true, reverse: true, disjoint: true });
    const actual = plain(await bridge.getSelectedRangeSetData());
    assert.deepEqual(actual.ranges.map(range => range.address), ["B9:C10", "C4:F5"]);
    assert.deepEqual(actual.ranges.map(range => [range.startRow, range.startColumn]), [[9, 2], [4, 3]]);
    assert.equal(actual.ranges[0].worksheetId, "office-Study Data");
    assert.equal(actual.ranges[0].rows[1][0].kind, 0);
    assert.equal(actual.ranges[0].rows[1][1].kind, 2);
    assert.equal(actual.selectionId, "");
    assert.equal(actual.ranges[0].selectionId, "");
    assert.equal(actual.ranges[0].selectionRevision, "");
    assert.equal(calls.selectedCalls, 0);
    assert.equal(calls.runs, 1);
});

test("Office original single selected-range API still rejects a disjoint selection", async () => {
    const { bridge } = mock({ disjoint: true });
    await assert.rejects(bridge.getSelectedRangeData(), /Multiple selections/);
});

test("Office selected-area fallback uses the original single-range reader on older hosts", async () => {
    const { bridge, calls } = mock({ supports19: false });
    const result = await bridge.getSelectedRangeSetData();
    assert.equal(result.ranges.length, 1);
    assert.equal(result.ranges[0].address, "C4:F5");
    assert.equal(calls.selectedCalls, 1);
});

test("Office older host cannot silently flatten a disjoint selection", async () => {
    const { bridge } = mock({ supports19: false, disjoint: true });
    await assert.rejects(bridge.getSelectedRangeSetData(), /Multiple selections/);
});

test("Office range-set acquisition preserves duplicate inputs and explicit selection order", async () => {
    const { bridge, references } = mock();
    const actual = plain(await bridge.getRangeSetData({ ranges: [references[1], references[0], references[1]] }));
    assert.deepEqual(actual.ranges.map(range => range.address), ["B9:C10", "C4:F5", "B9:C10"]);
    assert.deepEqual(actual.ranges.map(range => range.selectionId), ["second-input", "first-input", "second-input"]);
});

test("Office stale worksheet identity fails during metadata preflight before cell reads", async () => {
    const { bridge, references, calls } = mock();
    references[1].worksheetId = "replaced-sheet";
    await assert.rejects(bridge.getRangeSetData({ ranges: references }), /identity changed/);
    assert.deepEqual(calls.valueLoads, []);
});

test("Office conflicting coordinates, unsupported dialects and unverified revisions fail before Excel.run", async () => {
    const { bridge, references, calls } = mock();
    for (const extra of [{ startRow: 1 }, { columnCount: -1 }, { addressDialect: 2 }, { selectionRevision: "invented" }]) {
        await assert.rejects(bridge.getRangeDataByReference({ ...references[0], ...extra }), /coordinates conflict|Excel A1|cannot be verified/);
    }
    assert.equal(calls.runs, 0);
});

test("Office changed host rectangle is rejected before reading cells", async () => {
    const { bridge, references, ranges, calls } = mock();
    ranges[1].rowIndex = 99;
    await assert.rejects(bridge.getRangeSetData({ ranges: references }), /coordinates changed/);
    assert.deepEqual(calls.valueLoads, []);
});

test("Office selected range set narrows each whole-column area independently", async () => {
    const { bridge, ranges, calls } = mock({ disjoint: true });
    const firstUsed = { ...ranges[0] };
    const secondUsed = { ...ranges[1] };
    for (const [index, used] of [firstUsed, secondUsed].entries()) {
        ranges[index].rowCount = 1048576;
        ranges[index].getUsedRangeOrNullObject = () => { calls.usedCalls++; return used; };
    }
    const actual = await bridge.getSelectedRangeSetData();
    assert.equal(calls.usedCalls, 2);
    assert.ok(calls.areaLoads.length > 0);
    assert.deepEqual(plain(actual.ranges.map(range => range.rowCount)), [2, 2]);
    assert.deepEqual(calls.valueLoads, ["C4:F5", "B9:C10"]);
});

test("Office selected areas enforce aggregate size before loading any values", async () => {
    const { bridge, ranges, calls } = mock({ disjoint: true });
    ranges.forEach(range => { range.rowCount = 2000; range.columnCount = 3; });
    await assert.rejects(bridge.getSelectedRangeSetData(), /range set contains 12000 cells/);
    assert.deepEqual(calls.valueLoads, []);
});

test("Office unknown error codes and date origin remain unknown while raw error text and format survive", async () => {
    const { bridge, references } = mock();
    const actual = await bridge.getRangeDataByReference(references[0]);
    assert.equal(actual.rows[1][2].kind, 4);
    assert.equal(actual.rows[1][2].textValue, "#N/A");
    assert.equal(actual.rows[1][2].nativeErrorCode, "");
    assert.equal(actual.rows[1][1].kind, 1);
    assert.equal(actual.rows[1][1].numberFormat, "yyyy-mm-dd");
    assert.equal(actual.dateSystem, 0);
    assert.equal(actual.dateSerialOrigin, null);
    assert.equal(actual.workbookId, "");
});

test("Office single-cell and absolute addresses use the same canonical A1 spelling as Windows", async () => {
    const { bridge, ranges } = mock();
    const range = ranges[0];
    range.address = "'Study Data'!$c$4";
    range.rowCount = 1;
    range.columnCount = 1;
    range.values = [[range.values[0][0]]];
    range.valueTypes = [[range.valueTypes[0][0]]];
    range.numberFormat = [[range.numberFormat[0][0]]];
    const actual = await bridge.getSelectedRangeData();
    assert.equal(actual.address, "C4:C4");
    assert.equal(actual.startRow, 4);
    assert.equal(actual.startColumn, 3);
    assert.equal(actual.rows[0][0].kind, 0);
});

test("Office rejects a host address inconsistent with its reported coordinates", async () => {
    const { bridge, ranges } = mock();
    ranges[0].address = "'Study Data'!A1:D2";
    await assert.rejects(bridge.getSelectedRangeData(), /coordinates conflict/);
});

test("selected-areas preview reads one area through ExcelApi 1.9 without using the single-range API", async () => {
    const { bridge, calls } = mock({ areaCount: 1 });
    const actual = plain(await bridge.getSelectedRangeSetData());
    assert.equal(actual.ranges.length, 1);
    assert.equal(actual.ranges[0].worksheetName, "Study Data");
    assert.equal(actual.ranges[0].address, "C4:F5");
    assert.deepEqual(actual.ranges[0].rows.map(row => row.map(cell => cell.kind)), [[0, 2, 1, 3], [2, 1, 4, 1]]);
    assert.equal(calls.selectedCalls, 0);
});

test("selected-areas preview reads two disjoint areas with independent metadata and values", async () => {
    const { bridge, calls } = mock({ sameSheet: true, disjoint: true });
    const actual = plain(await bridge.getSelectedRangeSetData());
    assert.deepEqual(actual.ranges.map(range => [range.worksheetName, range.address, range.rowCount, range.columnCount]),
        [["Study Data", "C4:F5", 2, 4], ["Study Data", "B9:C10", 2, 2]]);
    assert.equal(actual.ranges[0].rows[0][1].textValue, "");
    assert.equal(actual.ranges[1].rows[0][1].textValue, " label ");
    assert.equal(calls.selectedCalls, 0);
});

test("single-range InvalidSelection explains how to preview multiple areas", async () => {
    const { bridge, calls } = mock({ invalidSelection: true });
    await assert.rejects(bridge.getSelectedRangeData(), error => {
        assert.equal(error.code, "InvalidSelection");
        assert.match(error.message, /one rectangular range/);
        assert.match(error.message, /Read selected areas \(01\.09\)/);
        assert.doesNotMatch(error.message, /Raw Office/);
        return true;
    });
    assert.deepEqual(calls.valueLoads, []);
});

test("selected-areas preview reports InvalidSelection from context.sync concisely", async () => {
    const { bridge, calls } = mock({ invalidSelection: true, failSync: true, debugInfoCode: true });
    await assert.rejects(bridge.getSelectedRangeSetData(), error => {
        assert.equal(error.code, "InvalidSelection");
        assert.equal(error.message, "Select worksheet cells and try Read selected areas (01.09) again.");
        return true;
    });
    assert.deepEqual(calls.valueLoads, []);
});

test("selected-areas preview rejects an empty selection without acquiring values", async () => {
    const { bridge, calls } = mock({ areaCount: 0 });
    await assert.rejects(bridge.getSelectedRangeSetData(), /At least one range must be selected/);
    assert.deepEqual(calls.valueLoads, []);
});

test("selected-areas preview on an older host still reads one range", async () => {
    const { bridge, calls } = mock({ supports19: false, areaCount: 0 });
    const actual = await bridge.getSelectedRangeSetData();
    assert.equal(actual.ranges.length, 1);
    assert.equal(actual.ranges[0].address, "C4:F5");
    assert.equal(calls.selectedCalls, 1);
    assert.deepEqual(calls.areaLoads, []);
});

test("older host InvalidSelection explains the ExcelApi 1.9 requirement", async () => {
    const { bridge, calls } = mock({ supports19: false, invalidSelection: true });
    await assert.rejects(bridge.getSelectedRangeSetData(), error => {
        assert.match(error.message, /ExcelApi 1\.9/);
        assert.match(error.message, /Select one rectangular range/);
        assert.doesNotMatch(error.message, /Raw Office/);
        return true;
    });
    assert.deepEqual(calls.valueLoads, []);
});
