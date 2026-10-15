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
