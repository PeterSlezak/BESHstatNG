"use strict";
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");
const copy = value => JSON.parse(JSON.stringify(value));
const source = fs.readFileSync(path.join(__dirname, "../../wwwroot/js/officeBridge.js"), "utf8");

function columnName(index) {
    let name = "";
    for (let n = index + 1; n; n = Math.floor((n - 1) / 26)) name = String.fromCharCode(65 + (n - 1) % 26) + name;
    return name;
}
function address(r, c, rows, columns) {
    return `${columnName(c)}${r + 1}:${columnName(c + columns - 1)}${r + rows}`;
}

function recordingWorkbook(options = {}) {
    const trace = { runs: 0, syncs: 0, adds: [], writes: [], clears: [], styles: [], autoFits: [], selections: [],
        activations: [], ranges: [], activeResolutions: 0 };
    let cells = new Map();
    const lookup = (storage, r, c) => {
        const key = `${r}:${c}`;
        if (!storage.has(key)) storage.set(key, { kind: "blank", value: null, style: {} });
        return storage.get(key);
    };
    const cellAt = (r, c) => lookup(cells, r, c);
    function worksheet(name, storage = new Map(), added = false) {
        let nameLoaded = !added, nameRequested = false;
        return { get name() { if (!nameLoaded) throw new Error("PropertyNotLoaded: worksheet.name"); return name; },
            set name(value) { name = value; }, cells: storage, id: `sheet-id:${name}`,
            protection: { protected: !!options.protectedSheet, load() {} },
            getRange(text) {
                const match = /^([A-Z]+)([1-9][0-9]*):([A-Z]+)([1-9][0-9]*)$/.exec(text);
                if (!match) throw new Error("Bad test address");
                const column = s => [...s].reduce((n, ch) => n * 26 + ch.charCodeAt(0) - 64, 0) - 1;
                const r = +match[2] - 1, c = column(match[1]), rows = +match[4] - r, columns = column(match[3]) - c + 1;
                return { ...this.getRangeByIndexes(r, c, rows, columns), worksheet: this,
                    rowIndex: r, columnIndex: c, rowCount: rows, columnCount: columns };
            },
            load(properties) { if (properties.includes("name")) nameRequested = true; },
            syncLoads() { if (nameRequested) nameLoaded = true; }, activate() { trace.activations.push(name); },
            getRangeByIndexes(r, c, rows, columns) {
                const indexes = [r, c, rows, columns];
                trace.ranges.push(indexes);
                if (![r, c, rows, columns].every(Number.isInteger) || r < 0 || c < 0 || rows < 1 || columns < 1 || r + rows > 1048576 || c + columns > 16384) {
                    throw new Error("Mock range outside worksheet");
                }
                const a = address(r, c, rows, columns);
                function visit(action) {
                    for (let row = 0; row < rows; row++) for (let column = 0; column < columns; column++) action(lookup(storage, r + row, c + column), row, column);
                }
                function style(property, value) {
                    trace.styles.push({ address: a, property, value });
                    visit(cell => { cell.style[property] = value; });
                }
                const font = new Proxy({}, { set(_, property, value) { style(`font.${property}`, value); return true; } });
                const fill = new Proxy({ clear() { style("fill", null); } }, {
                    set(_, property, value) { style(`fill.${property}`, value); return true; }
                });
                return {
                    // Host-returned prefix deliberately differs from fabricated A1-only addresses.
                    address: `HOST:${name}!${a}`, load() {},
                    get values() { return read("value"); },
                    get formulas() { return read("formula"); },
                    get valueTypes() { return read("type"); },
                    getMergedAreasOrNullObject() {
                        if (options.failMergeProbe) throw new Error("Injected merge probe failure");
                        return { isNullObject: !options.mergedConflict };
                    },
                    select() { trace.selections.push(a); },
                    clear(applyTo) {
                        if (applyTo !== "Contents") throw new Error("Unexpected clear mode");
                        trace.clears.push(a);
                        visit(cell => { cell.kind = "blank"; cell.value = null; delete cell.formula; });
                    },
                    set values(matrix) {
                        trace.writes.push({ address: a, indexes, values: copy(matrix) });
                        if (trace.writes.length === options.failWrite) throw new Error("Injected Office write failure");
                        visit((cell, row, column) => {
                            const value = matrix[row][column];
                            if (value === null) return; // actual Range.values null leaves existing content alone
                            if (typeof value === "number") { cell.kind = "number"; cell.value = value; }
                            else if (typeof value === "boolean") { cell.kind = "boolean"; cell.value = value; }
                            else if (value.startsWith("'")) { cell.kind = "text"; cell.value = value.slice(1); }
                            else { cell.kind = /^[=+\-]/.test(value) ? "formula" : "text"; cell.value = value; }
                        });
                    },
                    set numberFormat(matrix) { visit((cell, row, column) => { cell.style.numberFormat = matrix[row][column]; }); },
                    format: { font, fill,
                        autofitColumns() { trace.autoFits.push({ address: a, axis: "columns" }); },
                        autofitRows() { trace.autoFits.push({ address: a, axis: "rows" }); } }
                };
                function read(property) {
                    if (options.failOccupancyProbe) throw new Error("Injected occupancy probe failure");
                    if (options.missingOccupancy) return undefined;
                    return Array.from({ length: rows }, (_, i) => Array.from({ length: columns }, (_, j) => {
                        const cell = storage.get(`${r + i}:${c + j}`);
                        if (property === "type") return !cell || cell.kind === "blank" ? "Empty" : cell.kind === "error" ? "Error" :
                            typeof cell.value === "number" ? "Double" : typeof cell.value === "boolean" ? "Boolean" : "String";
                        if (property === "formula" && cell?.formula) return cell.formula;
                        return cell?.value ?? "";
                    }));
                }
            } };
    }
    const sheets = (options.names || ["Actual", "BESHStat_Result"]).map((name, i) => worksheet(name, i === 0 ? cells : new Map()));
    const worksheets = { items: sheets, load() {},
        getItem(identity) {
            const found = sheets.find(sheet => sheet.id === identity || sheet.name === identity);
            if (!found) throw new Error("ItemNotFound");
            return found;
        },
        getActiveWorksheet() { trace.activeResolutions++; return options.missingSheet ? null : sheets[options.activeIndex || 0]; },
        add(name) { trace.adds.push(name); if (options.failAdd) throw new Error("Injected worksheet add failure");
            const sheet = worksheet(name, new Map(), true); sheets.push(sheet); cells = sheet.cells; return sheet; } };
    const handlers = new Map();
    if (options.lifecycleEvents) { trace.eventAdds = []; trace.eventRemoves = []; }
    if (options.lifecycleEvents) for (const name of ["onDeleted", "onActivated", "onSelectionChanged", "onNameChanged"]) {
        worksheets[name] = { add(callback) {
            if (options.failEventRegistration === name) throw new Error("Registration failed");
            const registration = { callback, removed: false, context: null, remove() {
                if (options.failEventRemoval) throw new Error("Event removal failed");
                if (!this.removed) trace.eventRemoves.push(name);
                this.removed = true;
            } };
            if (!handlers.has(name)) handlers.set(name, []);
            handlers.get(name).push(registration);
            trace.eventAdds.push(name);
            return registration;
        } };
    }
    function selectedRange() {
        const sheet = sheets[options.selectedIndex || 0], b = options.selection || [0, 0, 3, 2];
        return { ...sheet.getRangeByIndexes(...b), worksheet: sheet, rowIndex: b[0], columnIndex: b[1], rowCount: b[2], columnCount: b[3] };
    }
    const context = { workbook: options.missingWorkbook ? null : { worksheets, getSelectedRange: selectedRange,
        getSelectedRanges: () => ({ areas: { items: [selectedRange()], load() {} } }),
        protection: { protected: !!options.protectedWorkbook, load() {} } }, async sync() {
        trace.syncs++;
        if (options.onSync) options.onSync(trace.syncs, { sheets, cells, options });
        for (const sheet of sheets) sheet.syncLoads();
        if (trace.syncs === options.failSync) throw new Error("Injected Office sync failure");
    } };
    const windowHandlers = new Map();
    const sandbox = { window: { addEventListener(name, callback) { windowHandlers.set(name, callback); } }, setTimeout,
        Office: { context: { host: options.nonExcel ? "Word" : "Excel", platform: "OfficeOnline",
            document: {}, requirements: { isSetSupported: (_, version) => options.apiAvailable !== false && (version === "1.1" || options.probes !== false) } }, HostType: { Excel: "Excel" } },
        Excel: { async run(contextOrAction, action) { trace.runs++; return (action || contextOrAction)(context); } } };
    vm.runInNewContext(source, sandbox, { filename: "officeBridge.js" });
    return { bridge: sandbox.window.beshOffice, trace, get cells() { return cells; }, cellAt, sandbox, sheets, options, context,
        handlers, fire(name, args) { for (const item of handlers.get(name) || []) if (!item.removed) item.callback(args); },
        unload() { windowHandlers.get("pagehide")?.(); } };
}
module.exports = { recordingWorkbook, copy, address };
