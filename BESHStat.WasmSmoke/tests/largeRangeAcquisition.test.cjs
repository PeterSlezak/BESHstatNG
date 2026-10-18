"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");
const script = fs.readFileSync(path.join(__dirname, "../wwwroot/js/officeBridge.js"), "utf8");

function columnName(index) {
    let result = "";
    for (let n = index + 1; n > 0; n = Math.floor((n - 1) / 26)) result = String.fromCharCode(65 + (n - 1) % 26) + result;
    return result;
}
function address(row, column, rows, columns) {
    return `${columnName(column)}${row + 1}:${columnName(column + columns - 1)}${row + rows}`;
}
const large = { row: 2, column: 1, rows: 1203, columns: 10 };
const optIn = { maxRangeCells: 15000, maxTotalCells: 15000 };
const plain = value => JSON.parse(JSON.stringify(value));

// Payload properties are unavailable until sync; a large source must never be loaded.
function mock(rectangles = [large], settings = {}) {
    const calls = { runs: 0, syncs: 0, batches: [], metadata: [], areaLoads: [], narrowed: 0 };
    let pending = [];
    let time = 0;
    const sheets = new Map();
    function sheet(name) {
        if (!sheets.has(name)) {
            const value = { name, id: "id-" + name, load() {}, getRangeByIndexes(row, column, rows, columns) {
                return range(value, { row, column, rows, columns });
            }, getRange(a1) {
                return sources.find(source => source.worksheet === value && source.address.endsWith("!" + a1));
            } };
            sheets.set(name, value);
        }
        return sheets.get(name);
    }
    function range(worksheet, rectangle) {
        const { row, column, rows, columns } = rectangle;
        const value = { worksheet, address: `${worksheet.name}!${address(row, column, rows, columns)}`,
            rowIndex: row, columnIndex: column, rowCount: rows, columnCount: columns,
            load(properties) {
                if (properties.includes("values")) {
                    assert.ok(rows * columns <= (settings.cap || 2000), "oversized payload queued");
                    pending.push(value);
                } else calls.metadata.push([...properties]);
            }
        };
        return value;
    }
    const sources = rectangles.map(rectangle => range(sheet(rectangle.sheet || "Data"), rectangle));
    const reference = source => ({ worksheetName: source.worksheet.name, worksheetId: source.worksheet.id,
        address: source.address.split("!")[1] });
    const workbook = { worksheets: { getItem: name => sheet(name) },
        getSelectedRange: () => sources[0],
        getSelectedRanges: () => ({ areas: { items: sources, load(properties) {
            calls.areaLoads.push([...properties]);
            assert.deepEqual([...properties], ["items/rowCount", "items/columnCount"]);
        } } }) };
    const globals = { window: {}, performance: { now: () => time },
        Office: { HostType: { Excel: "Excel" }, context: { host: "Excel", document: { url: "https://test/Book.xlsx" },
            requirements: { isSetSupported: (_, version) => {
                if (version === "1.9") time += settings.capabilityTime || 0;
                return version !== "1.9" || settings.oldHost !== true;
            } } } },
        Excel: { run: async callback => { calls.runs++; return callback({ workbook, sync: async () => {
            calls.syncs++;
            if (!pending.length) { if (settings.metadataTime) time += settings.metadataTime; return; }
            assert.equal(pending.length, 1, "one bounded payload per sync");
            const chunk = pending[0];
            pending = [];
            calls.batches.push([chunk.rowIndex, chunk.columnIndex, chunk.rowCount, chunk.columnCount]);
            if (settings.failBatch === calls.batches.length) throw new Error("Host failure in middle chunk");
            chunk.values = [];
            chunk.valueTypes = [];
            chunk.numberFormat = [];
            for (let r = 0; r < chunk.rowCount; r++) {
                const cells = Array.from({ length: chunk.columnCount }, (_, c) =>
                    settings.cell ? settings.cell(chunk.rowIndex + r, chunk.columnIndex + c) :
                        [(chunk.rowIndex + r) * 20000 + chunk.columnIndex + c, "Double", "0.00"]);
                chunk.values.push(cells.map(cell => cell[0]));
                chunk.valueTypes.push(cells.map(cell => cell[1]));
                chunk.numberFormat.push(cells.map(cell => cell[2]));
            }
            if (settings.afterBatch) settings.afterBatch(chunk, calls.batches.length);
            time += settings.batchTime || 0;
        } }); } }, setTimeout, decodeURIComponent };
    vm.runInNewContext(script, globals);
    return { bridge: globals.window.beshOffice, calls, sources, references: sources.map(reference) };
}

test("explicit opt-in reads 12,030 cells in seven sequential bounded responses including the last partial tile", async () => {
    const { bridge, calls, references } = mock();
    const result = await bridge.getRangeDataByReference(references[0], optIn);
    assert.equal(calls.syncs, 8); // one metadata + seven payloads
    assert.deepEqual(calls.batches, Array.from({ length: 7 }, (_, i) => [2 + i * 200, 1, i === 6 ? 3 : 200, 10]));
    assert.equal(result.address, "B3:K1205");
    assert.equal(result.rows.length, 1203);
    for (let r = 0; r < 1203; r++) for (let c = 0; c < 10; c++) {
        assert.equal(result.rows[r][c].numberValue, (r + 2) * 20000 + c + 1);
        assert.equal(result.rows[r][c].numberFormat, "0.00");
    }
    assert.ok(calls.metadata[0].includes("rowIndex"));
    assert.ok(calls.metadata[0].includes("address"));
    assert.equal(result.dateSystem, 0);
    assert.equal(result.dateSerialOrigin, null);
});

test("default rejects 12,030 cells before any payload; exact 10,000 cells remains accepted", async () => {
    const { bridge, calls } = mock();
    await assert.rejects(bridge.getSelectedRangeData(), error => error.code === "input.range-cells" && error.allowed === 10000 && error.requested === 12030);
    assert.deepEqual(calls.batches, []);
    const exact = mock([{ row: 0, column: 0, rows: 1000, columns: 10 }]);
    assert.equal((await exact.bridge.getSelectedRangeData()).rows.length, 1000);
    assert.equal(exact.calls.batches.length, 5);
});

test("wide rectangles split columns and rows without losing order or exceeding cap", async () => {
    const { bridge, calls } = mock([{ row: 4, column: 3, rows: 2, columns: 5000 }]);
    const result = await bridge.getSelectedRangeData();
    assert.deepEqual(calls.batches, [[4, 3, 1, 2000], [4, 2003, 1, 2000], [4, 4003, 1, 1000],
        [5, 3, 1, 2000], [5, 2003, 1, 2000], [5, 4003, 1, 1000]]);
    assert.equal(result.rows[1][4999].numberValue, 105002);
    assert.equal(result.rows[0][2000].numberValue, 82003);
});

test("custom tile size produces exact row boundaries and final partial tile", async () => {
    const { bridge, calls } = mock([{ row: 0, column: 0, rows: 7, columns: 3 }], { cap: 8 });
    await bridge.getSelectedRangeData({ maxChunkCells: 8 });
    assert.deepEqual(calls.batches, [[0, 0, 2, 3], [2, 0, 2, 3], [4, 0, 2, 3], [6, 0, 1, 3]]);
});

test("large sparse rectangle preserves blank edges, typed empty text, Boolean, error and date-like formats", async () => {
    const cell = (row, column) => row === 202 ? [column === 1 ? "" : "#N/A", column === 1 ? "String" : "Error", "@"] :
        row === 402 ? [false, "Boolean", "General"] : row === 602 ? [61, "Double", "yyyy-mm-dd"] : ["", "Empty", "General"];
    const { bridge, calls } = mock([large], { cell });
    const result = await bridge.getSelectedRangeData(optIn);
    assert.equal(result.rows[0][0].kind, 0);
    assert.equal(result.rows[1202][9].kind, 0);
    assert.equal(result.rows[200][0].kind, 2);
    assert.equal(result.rows[200][0].textValue, "");
    assert.equal(result.rows[200][1].kind, 4);
    assert.equal(result.rows[200][1].nativeErrorCode, "");
    assert.equal(result.rows[400][0].booleanValue, false);
    assert.equal(result.rows[400][0].kind, 3);
    assert.equal(result.rows[600][0].numberFormat, "yyyy-mm-dd");
    assert.equal(result.rows[600][0].kind, 1);
    assert.equal(calls.batches.length, 7);
});

test("ordered multi-sheet ranges are preflighted together and read sequentially", async () => {
    const { bridge, references, calls } = mock([large, { row: 10, column: 5, rows: 2, columns: 3, sheet: "Other" }]);
    const result = await bridge.getRangeSetData({ selectionId: "request", ranges: references }, optIn);
    assert.deepEqual(plain(result.ranges.map(range => range.worksheetName)), ["Data", "Other"]);
    assert.equal(result.selectionId, "request");
    assert.equal(result.ranges[1].rows[1][2].numberValue, 220007);
    assert.equal(calls.batches.length, 8);
    assert.equal(calls.syncs, 9);
});

test("multi-area selected acquisition shares the bounded planner and dimension-only area load", async () => {
    const { bridge, calls } = mock([large, { row: 2000, column: 5, rows: 2, columns: 3 }]);
    const result = await bridge.getSelectedRangeSetData(optIn);
    assert.equal(result.ranges.length, 2);
    assert.equal(result.ranges[1].startRow, 2001);
    assert.equal(calls.areaLoads.length, 1);
    assert.equal(calls.batches.length, 8);
    assert.equal(calls.syncs, 10);
});

test("old host selected-area fallback uses chunking for its single rectangle", async () => {
    const { bridge, calls } = mock([large], { oldHost: true });
    const result = await bridge.getSelectedRangeSetData(optIn);
    assert.equal(result.ranges[0].rowCount, 1203);
    assert.equal(calls.batches.length, 7);
});

test("old host fallback shares the original deadline across capability and metadata reads", async () => {
    const { bridge, calls } = mock([large], { oldHost: true, capabilityTime: 6, metadataTime: 6 });
    await assert.rejects(bridge.getSelectedRangeSetData({ ...optIn, timeBudgetMilliseconds: 10 }), error => error.code === "input.time-budget");
    assert.deepEqual(calls.batches, []);
});

test("whole-column narrowing precedes opted-in metadata preflight and chunking", async () => {
    const { bridge, sources, calls } = mock();
    const used = { ...sources[0] };
    sources[0].rowCount = 1048576;
    sources[0].getUsedRangeOrNullObject = () => { calls.narrowed++; return used; };
    const result = await bridge.getSelectedRangeData(optIn);
    assert.equal(result.rowCount, 1203);
    assert.equal(calls.narrowed, 1);
    assert.equal(calls.batches.length, 7);
});

test("aggregate quota is rejected before first payload and before stale identity", async () => {
    const { bridge, calls, references } = mock([large, large]);
    references[1].worksheetId = "stale";
    await assert.rejects(bridge.getRangeSetData({ ranges: references }, optIn), error =>
        error.code === "input.total-cells" && error.requested === 24060 && error.allowed === 15000);
    assert.deepEqual(calls.batches, []);
});

test("selected aggregate limit also rejects before all payloads", async () => {
    const { bridge, calls } = mock([large, large]);
    await assert.rejects(bridge.getSelectedRangeSetData(optIn), /range set contains 24060 cells/);
    assert.deepEqual(calls.batches, []);
});

test("memory estimate rejects before payload; exact estimate is accepted", async () => {
    const { bridge, calls } = mock([{ row: 0, column: 0, rows: 2, columns: 2 }]);
    await assert.rejects(bridge.getSelectedRangeData({ maxEstimatedBytes: 4095 }), error =>
        error.code === "input.estimated-bytes" && error.requested === 4096 && error.allowed === 4095);
    assert.deepEqual(calls.batches, []);
    assert.equal((await bridge.getSelectedRangeData({ maxEstimatedBytes: 4096 })).rows.length, 2);
});

test("range count rejects explicit requests before Excel.run and selected areas before payload", async () => {
    const { bridge, references, calls } = mock([large, large]);
    await assert.rejects(bridge.getRangeSetData({ ranges: references }, { ...optIn, maxRanges: 1 }), /maxRanges/);
    assert.equal(calls.runs, 0);
    await assert.rejects(bridge.getSelectedRangeSetData({ ...optIn, maxRanges: 1 }), /maxRanges/);
    assert.deepEqual(calls.batches, []);
});

test("invalid and unbounded options reject before host acquisition", async () => {
    const { bridge, calls } = mock();
    for (const key of ["maxRangeCells", "maxTotalCells", "maxRanges", "maxChunkCells", "maxEstimatedBytes", "estimatedBytesPerCell", "timeBudgetMilliseconds"]) {
        for (const value of [0, -1, NaN, Infinity, Number.MAX_VALUE, "10000", null]) {
            await assert.rejects(bridge.getSelectedRangeData({ [key]: value }), /budget|ceilings/);
        }
    }
    for (const options of [{ maxChunkCells: 10001 }, { maxRangeCells: 1000001 }, { maxTotalCells: 1000001 },
        { maxRanges: 1025 }, { maxEstimatedBytes: 268435457 }, { timeBudgetMilliseconds: 300001 },
        { estimatedBytesPerCell: 1 }, { maxTotalCells: 2.5 }, { signal: {} }]) {
        await assert.rejects(bridge.getSelectedRangeData(options), /budget|ceilings|signal/);
    }
    assert.equal(calls.runs, 0);
});

test("cancelled signal refuses before Excel.run", async () => {
    const { bridge, calls } = mock();
    await assert.rejects(bridge.getSelectedRangeData({ ...optIn, signal: { aborted: true } }), /cancelled/);
    assert.equal(calls.runs, 0);
});

test("cancellation after second response returns no snapshot and queues no third chunk", async () => {
    const signal = { aborted: false };
    const { bridge, calls } = mock([large], { afterBatch: (_, n) => { if (n === 2) signal.aborted = true; } });
    await assert.rejects(bridge.getSelectedRangeData({ ...optIn, signal }), /cancelled/);
    assert.equal(calls.batches.length, 2);
});

test("deadline during metadata refuses first payload", async () => {
    const { bridge, calls } = mock([large], { metadataTime: 31 });
    await assert.rejects(bridge.getSelectedRangeData({ ...optIn, timeBudgetMilliseconds: 30 }), error => error.code === "input.time-budget");
    assert.deepEqual(calls.batches, []);
});

test("deadline after a response rejects without partial result or further reads", async () => {
    const { bridge, calls } = mock([large], { batchTime: 6 });
    await assert.rejects(bridge.getSelectedRangeData({ ...optIn, timeBudgetMilliseconds: 10 }), error =>
        error.code === "input.time-budget" && error.requested === 12);
    assert.equal(calls.batches.length, 2);
});

test("host failure on middle chunk rejects the whole operation", async () => {
    const { bridge, calls } = mock([large], { failBatch: 3 });
    await assert.rejects(bridge.getSelectedRangeData(optIn), /Host failure in middle chunk/);
    assert.equal(calls.batches.length, 3);
});

test("incorrect chunk positions, shapes and addresses fail closed", async () => {
    for (const corrupt of [chunk => { chunk.rowIndex++; }, chunk => { chunk.columnCount--; },
        chunk => { chunk.values.pop(); }, chunk => { chunk.valueTypes[0].pop(); },
        chunk => { chunk.numberFormat[0].pop(); }, chunk => { chunk.address = "Data!A1:B2"; }]) {
        const { bridge, calls } = mock([large], { afterBatch: (chunk, n) => { if (n === 2) corrupt(chunk); } });
        await assert.rejects(bridge.getSelectedRangeData(optIn), /dimensions|positions|coordinates conflict/);
        assert.equal(calls.batches.length, 2);
    }
});

test("single-tile selection compares refreshed proxy positions to the original preflight", async () => {
    const { bridge, calls } = mock([{ row: 2, column: 1, rows: 2, columns: 3 }], {
        afterBatch(chunk) { chunk.rowIndex = 3; chunk.address = "Data!B4:D5"; }
    });
    await assert.rejects(bridge.getSelectedRangeData(), /positions or dimensions changed/);
    assert.equal(calls.batches.length, 1);
});

test("hostile source dimensions fail before payload without numeric coercion", async () => {
    for (const dimensions of [[0, 1], [-1, 2], [Number.MAX_SAFE_INTEGER, 2], ["2", 2], [NaN, 2]]) {
        const { bridge, calls, sources } = mock();
        [sources[0].rowCount, sources[0].columnCount] = dimensions;
        await assert.rejects(bridge.getSelectedRangeData(optIn), /dimensions|at most/);
        assert.deepEqual(calls.batches, []);
    }
});

test("unsupported objects cannot become coerced text in a snapshot", async () => {
    for (const type of ["RichValue", "Double", "String", "Boolean", "Error", { unknown: true }]) {
        const { bridge } = mock([{ row: 0, column: 0, rows: 1, columns: 1 }], { cell: () => [{ object: true }, type, "General"] });
        await assert.rejects(bridge.getSelectedRangeData(), /Unsupported host/);
    }
});
