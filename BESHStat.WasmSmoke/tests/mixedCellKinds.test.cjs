"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const path = require("node:path");

const bridgeSource = fs.readFileSync(path.join(__dirname, "../wwwroot/js/officeBridge.js"), "utf8");

// Regression: an Excel range containing an actual error, a date-formatted number,
// text resembling an error, and an empty string must not lose its cell kinds.
test("mixed-type range snapshot keeps raw kinds, formats and worksheet coordinates", async () => {
    const worksheet = { id: "sheet-01", name: "Hárok1", load() {} };
    const range = {
        worksheet, address: "'Hárok1'!A8:C10", rowIndex: 7, columnIndex: 0,
        rowCount: 3, columnCount: 3, load() {},
        values: [
            ["A", "B", "C"],
            ["#HODNOTA!", 61, 2],
            ["#N/A", 0, ""]
        ],
        valueTypes: [
            ["String", "String", "String"],
            ["Error", "Double", "Integer"],
            ["String", "Double", "String"]
        ],
        numberFormat: [
            ["General", "General", "General"],
            ["General", "d.m.yyyy", "General"],
            ["General", "General", "General"]
        ]
    };
    const sandbox = {
        window: {},
        Office: {
            HostType: { Excel: "Excel" },
            context: {
                host: "Excel", platform: "OfficeOnline",
                document: { url: "https://example.test/Book.xlsx" },
                requirements: { isSetSupported: () => true }
            }
        },
        Excel: {
            run: async callback => callback({
                workbook: { getSelectedRange: () => range },
                sync: async () => {}
            })
        },
        setTimeout, decodeURIComponent
    };
    vm.runInNewContext(bridgeSource, sandbox, { filename: "officeBridge.js" });
    const raw = await sandbox.window.beshOffice.getSelectedRangeData();
    const snapshot = JSON.parse(JSON.stringify(raw));
    assert.equal(snapshot.startRow, 8);
    assert.equal(snapshot.startColumn, 1);
    assert.equal(snapshot.worksheetName, "Hárok1");
    assert.deepEqual(snapshot.rows.map(row => row.map(cell => cell.kind)), [
        [2, 2, 2],
        [4, 1, 1],
        [2, 1, 2]
    ]);
    assert.equal(snapshot.rows[1][0].textValue, "#HODNOTA!");
    assert.equal(snapshot.rows[1][1].numberValue, 61);
    assert.equal(snapshot.rows[1][1].numberFormat, "d.m.yyyy");
    assert.equal(snapshot.rows[2][0].textValue, "#N/A");
    assert.equal(snapshot.rows[2][2].textValue, "");
});
