"use strict";
// Independent composite mock. Existing table-only mocked workbook and fixtures stay unchanged.
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");
const { address } = require("./recordingWorkbook.cjs");
function compositeWorkbook(options = {}) {
    const trace = { runs: 0, syncs: 0, adds: [], writes: [], merges: [], styles: [], ranges: [] };
    const oldCells = new Map([["9:2", { value: "Existing workbook data", kind: "text", style: {} }]]);
    const sheets = new Map();
    function sheet(name, cells = new Map()) {
        const cellAt = (r, c) => {
            const key = `${r}:${c}`;
            if (!cells.has(key)) cells.set(key, { value: null, kind: "blank", style: {} });
            return cells.get(key);
        };
        const result = { name, cells, cellAt, load() {}, activate() {}, getRangeByIndexes(r, c, rows, columns) {
            if (![r, c, rows, columns].every(Number.isInteger) || r < 0 || c < 0 || rows < 1 || columns < 1 || r + rows > 1048576 || c + columns > 16384) throw new Error("Out-of-grid range");
            const indexes = [r, c, rows, columns], a = address(r, c, rows, columns);
            trace.ranges.push(indexes);
            const visit = action => { for (let i = 0; i < rows; i++) for (let j = 0; j < columns; j++) action(cellAt(r + i, c + j), i, j); };
            const style = (key, value) => { trace.styles.push({ address: a, key, value }); visit(cell => { cell.style[key] = value; }); };
            const proxy = prefix => new Proxy({}, { set(_, key, value) { style(prefix + key, value); return true; } });
            const format = new Proxy({ font: proxy("font."), fill: proxy("fill.") }, { set(_, key, value) { style(key, value); return true; } });
            return { address: `HOST:${name}!${a}`, load() {}, merge() { trace.merges.push(a); }, format,
                clear() { throw new Error("Composite route must never clear cells"); },
                set formulas(_) { throw new Error("Formula execution forbidden"); },
                set values(matrix) {
                    trace.writes.push({ address: a, indexes, matrix: JSON.parse(JSON.stringify(matrix)) });
                    if (trace.writes.length === options.failWrite) throw new Error("Injected write failure");
                    visit((cell, i, j) => {
                        const value = matrix[i][j];
                        if (value === null) return;
                        if (typeof value === "string") {
                            cell.kind = value.startsWith("'") ? "text" : /^[=+\-@]/.test(value) ? "formula" : "text";
                            cell.value = value.startsWith("'") ? value.slice(1) : value;
                        } else { cell.value = value; cell.kind = typeof value; }
                    });
                },
                set numberFormat(matrix) { visit((cell, i, j) => { cell.style.numberFormat = matrix[i][j]; }); }
            };
        } };
        sheets.set(name, result);
        return result;
    }
    const existing = sheet("Existing", oldCells);
    const worksheets = { items: [...(options.names || ["Existing"])].map(name => ({ name })), load() {},
        getActiveWorksheet() { throw new Error("Composite may not acquire current worksheet"); },
        add(name) { trace.adds.push(name); return sheet(name); } };
    const context = { workbook: options.missingWorkbook ? null : { worksheets, protection: { protected: !!options.protectedWorkbook, load() {} } },
        async sync() { trace.syncs++; if (trace.syncs === options.failSync) throw new Error("Injected sync failure"); } };
    const sandbox = { window: {}, setTimeout, Office: { context: { host: options.nonExcel ? "Word" : "Excel", platform: "OfficeOnline",
        requirements: { isSetSupported: () => options.apiAvailable !== false } }, HostType: { Excel: "Excel" } },
        Excel: { async run(action) { trace.runs++; return action(context); } } };
    vm.runInNewContext(fs.readFileSync(path.join(__dirname, "../../wwwroot/js/officeBridge.js"), "utf8"), sandbox);
    return { bridge: sandbox.window.beshOffice, trace, oldCells, sheets, existing };
}
module.exports = { compositeWorkbook };
