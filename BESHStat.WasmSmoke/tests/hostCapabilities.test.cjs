"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const path = require("node:path");

const source = fs.readFileSync(path.join(__dirname, "../wwwroot/js/officeBridge.js"), "utf8");

function loadBridge(isExcel = true, supportsExcelApi = true) {
    let excelRuns = 0;
    const mock = {
        window: {},
        Office: {
            context: {
                host: isExcel ? "Excel" : "Word",
                platform: "OfficeOnline",
                requirements: { isSetSupported: (name, version) =>
                    name === "ExcelApi" && version === "1.1" && supportsExcelApi }
            },
            HostType: { Excel: "Excel" }
        },
        Excel: { run: async () => { excelRuns++; throw new Error("Excel.run should not execute in this test"); } },
        setTimeout
    };
    vm.runInNewContext(source, mock, { filename: "officeBridge.js" });
    return { bridge: mock.window.beshOffice, getRunCount: () => excelRuns };
}

const makePayload = destination => ({
    values: [["heading", "p"], ["A", 0.05]],
    outputTarget: { destination, startRow: 1, startColumn: 1, worksheetName: "Results" }
});

test("Office capability discovery advertises only the two supported destinations", async () => {
    const { bridge } = loadBridge();
    const snapshot = await bridge.getHostCapabilities();
    assert.equal(snapshot.host, "OfficeJs");
    assert.equal(snapshot.isExcel, true);
    assert.equal(snapshot.platform, "OfficeOnline");
    assert.deepEqual(Array.from(snapshot.capabilities), [
        "input.read-selected-range", "output.write-result-tables",
        "output.current-worksheet", "output.new-worksheet"
    ]);
    assert.ok(!snapshot.capabilities.includes("output.new-workbook"));
});

test("non-Excel host does not advertise operations", async () => {
    const { bridge } = loadBridge(false);
    const snapshot = await bridge.getHostCapabilities();
    assert.equal(snapshot.isExcel, false);
    assert.equal(snapshot.capabilities.length, 0);
});

test("unavailable Excel API fails closed", async () => {
    const { bridge } = loadBridge(true, false);
    const snapshot = await bridge.getHostCapabilities();
    assert.equal(snapshot.isExcel, true);
    assert.equal(snapshot.capabilities.length, 0);
});

test("writer rejects NewWorkbook before Excel.run", async () => {
    const { bridge, getRunCount } = loadBridge();
    await assert.rejects(bridge.writeResultTables([makePayload(2)]), /New-workbook output is not supported/);
    assert.equal(getRunCount(), 0);
});

test("writer rejects unknown destination before Excel.run", async () => {
    const { bridge, getRunCount } = loadBridge();
    await assert.rejects(bridge.writeResultTables([makePayload(999)]), /Unknown result output destination/);
    assert.equal(getRunCount(), 0);
});

test("writer rejects a host with unavailable Excel APIs before Excel.run", async () => {
    const { bridge, getRunCount } = loadBridge(true, false);
    await assert.rejects(bridge.writeResultTables([makePayload(1)]), /result writing is unavailable/);
    assert.equal(getRunCount(), 0);
});

test("writer rejects mixed destinations before Excel.run", async () => {
    const { bridge, getRunCount } = loadBridge();
    await assert.rejects(bridge.writeResultTables([makePayload(0), makePayload(1)]), /same output destination/);
    assert.equal(getRunCount(), 0);
});

test("deprecated single and batch exports cannot bypass current-sheet scoped consent", async () => {
    const { bridge, getRunCount } = loadBridge();
    for (const call of [() => bridge.writeResultTables([makePayload(0)]), () => bridge.writeResultTable(makePayload(0))]) {
        await assert.rejects(call(), e => e.code === "output.legacy-current-unsupported");
    }
    assert.equal(getRunCount(), 0);
});

for (const [row, column] of [[0, 1], [-1, 1], [1, 0], [1048577, 1], [1, 16385], [1.5, 1]]) {
    test(`legacy illegal start ${row},${column} rejects before Excel.run`, async () => {
        const { bridge, getRunCount } = loadBridge();
        const payload = makePayload(1); payload.outputTarget.startRow = row; payload.outputTarget.startColumn = column;
        await assert.rejects(bridge.writeResultTables([payload]), /outside the worksheet grid/);
        assert.equal(getRunCount(), 0);
    });
}

for (const destination of ["invalid", 1.5, null, false, ""]) {
    test(`legacy malformed destination ${JSON.stringify(destination)} has no silent fallback`, async () => {
        const { bridge, getRunCount } = loadBridge();
        await assert.rejects(bridge.writeResultTables([makePayload(destination)]), /Unknown result output destination/);
        assert.equal(getRunCount(), 0);
    });
}
