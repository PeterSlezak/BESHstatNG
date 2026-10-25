// In-memory Office boundary only. Production writer/reader algorithms are imported unchanged by tests.
export function pilotWorkbook() {
    const events = new Map(), sheets = new Map(), cells = new Map(), fonts = new Map();
    const fills = new Map(), borders = new Map(), formats = new Map();
    const state = { document: { mode: "readWrite" }, selected: { row: 1, col: 1, rows: 4, cols: 3 }, sheetId: "sheet-1", sheetName: "Input",
        runs: 0, syncs: 0, mutations: 0, loads: [], merged: false, tables: false, pivots: false, protected: false, bookProtected: false,
        hook: async () => {}, rejectCommit: false, added: 0 };
    sheets.set(state.sheetId, { id: state.sheetId, name: state.sheetName });
    const key = (id, r, c) => `${id}:${r}:${c}`;
    function cell(id, r, c) { return cells.get(key(id, r, c)) || { value: "", type: "Empty", formula: "", format: "General" }; }
    state.setCell = (r, c, value, type = typeof value === "number" ? "Double" : "String", formula = value) =>
        cells.set(key(state.sheetId, r, c), { value, type, formula, format: "General", hasFormula: typeof formula === "string" && formula.startsWith("=") && formula !== value });
    for (let c = 1; c <= 3; c++) { state.setCell(1, c, `Group ${c}`); for (let r = 2; r <= 4; r++) state.setCell(r, c, r + c); }
    state.emit = (name, args = {}) => { for (const handler of events.get(name) || []) handler(args); };
    // Snapshot dispatched callbacks: removal cannot recall an already queued host event.
    state.queuedEvent = name => { const handlers = [...events.get(name) || []]; return () => { for (const handler of handlers) handler({}); }; };
    state.listenerCount = () => [...events.values()].reduce((n, set) => n + set.size, 0);
    state.snapshot = () => ({ worksheetId: state.sheetId, worksheetName: sheets.get(state.sheetId).name, startRow: state.selected.row,
        startColumn: state.selected.col, rowCount: state.selected.rows, columnCount: state.selected.cols,
        rows: Array.from({ length: state.selected.rows }, (_, r) => Array.from({ length: state.selected.cols }, (_, c) => {
            const x = cell(state.sheetId, state.selected.row + r, state.selected.col + c);
            const kind = { Empty: 0, Double: 1, String: 2, Boolean: 3, Error: 4 }[x.type];
            return { kind, numberValue: kind === 1 ? x.value : 0, textValue: [2, 4].includes(kind) ? x.value : "", booleanValue: kind === 3 ? x.value : false, numberFormat: x.format, nativeErrorCode: "" };
        })) });
    let nextToken = 0;
    const scope = { crypto: { randomUUID: () => `session-token-${++nextToken}` }, Office: {
        DocumentMode: { ReadWrite: "readWrite", ReadOnly: "readOnly" },
        onReady: async () => ({ host: "Excel", platform: "OfficeOnline" }),
        context: { get document() { return state.document; }, requirements: { isSetSupported: (_name, version) => ["1.1", "1.9", "1.17"].includes(version) } }
    }, Excel: { async run(first, second) {
        const callback = second || first; state.runs++;
        let alive = true;
        const queue = [], check = () => { if (!alive) throw new Error("expired proxy"); };
        function event(name) {
            return { add(handler) {
                if (!events.has(name)) events.set(name, new Set()); events.get(name).add(handler);
                return { context, remove() { events.get(name).delete(handler); } };
            } };
        }
        function sheet(data) {
            if (!data) throw Object.assign(new Error(), { code: "ItemNotFound" });
            const obj = { load() { check(); queue.push(() => { if (!sheets.has(data.id)) throw Object.assign(new Error(), { code: "ItemNotFound" }); obj.id = data.id; obj.name = data.name; }); return obj; },
                protection: { protected: false, load() { queue.push(() => { this.protected = state.protected; }); } },
                getRange(text) {
                    const m = /^([A-Z]+)(\d+):([A-Z]+)(\d+)$/.exec(text);
                    const col = s => [...s].reduce((n, c) => n * 26 + c.charCodeAt(0) - 64, 0);
                    if (!m) throw new Error("Invalid test generated bounds");
                    return range(data, Number(m[2]), col(m[1]), Number(m[4]) - Number(m[2]) + 1, col(m[3]) - col(m[1]) + 1);
                } };
            return obj;
        }
        function range(data, row, col, rows, cols) {
            const letters = n => { let s = ""; for (; n; n = Math.floor((n - 1) / 26)) s = String.fromCharCode(65 + (n - 1) % 26) + s; return s; };
            const loaded = {};
            const matrix = fn => Array.from({ length: rows }, (_, r) => Array.from({ length: cols }, (_, c) => fn(cell(data.id, row + r, col + c))));
            const obj = { worksheet: sheet(data), rowIndex: row - 1, columnIndex: col - 1, rowCount: rows, columnCount: cols,
                address: `'${data.name.replace(/'/g, "''")}'!${letters(col)}${row}:${letters(col + cols - 1)}${row + rows - 1}`,
                get values() { return loaded.values; }, set values(values) { check(); queue.push(() => {
                    if (state.rejectCommit) throw new Error("simulated write refusal");
                    state.mutations++;
                    values.forEach((line, r) => line.forEach((value, c) => {
                        if (typeof value === "string" && value.startsWith("'")) value = value.slice(1);
                        cells.set(key(data.id, row + r, col + c), { value, formula: value, hasFormula: false,
                            type: value === "" ? "Empty" : typeof value === "number" ? "Double" : typeof value === "boolean" ? "Boolean" : "String", format: "General" });
                    })); state.emit("onChanged");
                }); },
                get numberFormat() { return loaded.numberFormat; }, set numberFormat(formats) { queue.push(() => {
                    formats.forEach((line, r) => line.forEach((format, c) => { cell(data.id, row + r, col + c).format = format; }));
                }); },
                load(properties) { check(); state.loads.push(properties); queue.push(() => {
                    const requested = properties.split(",");
                    if (requested.includes("values")) loaded.values = matrix(x => x.value);
                    if (requested.includes("numberFormat")) loaded.numberFormat = matrix(x => x.format);
                    if (requested.includes("valueTypes")) obj.valueTypes = matrix(x => x.type);
                    if (requested.includes("formulas")) obj.formulas = matrix(x => x.formula);
                    if (requested.includes("hasSpill")) obj.hasSpill = state.spill || false;
                }); return obj; },
                getMergedAreasOrNullObject() { const o = {}; queue.push(() => { o.isNullObject = !state.merged; }); return o; },
                getTables() { return { items: state.tables ? [{ id: "table" }] : [], load() {} }; },
                getPivotTables() { return { items: state.pivots ? [{ id: "pivot" }] : [], load() {} }; },
                getSpecialCellsOrNullObject() {
                    const o = {load(){return o;},areas:{items:[],load(){return this;}}};
                    queue.push(() => {
                        const selected = [...cells].map(([k,v])=>({coordinates:k.split(':'),v})).filter(x=>x.coordinates[0]===data.id&&x.v.hasFormula&&
                            Number(x.coordinates[1])>=row&&Number(x.coordinates[1])<row+rows&&Number(x.coordinates[2])>=col&&Number(x.coordinates[2])<col+cols)
                            .map(x=>({rowIndex:Number(x.coordinates[1])-1,columnIndex:Number(x.coordinates[2])-1,rowCount:1})).sort((a,b)=>a.columnIndex-b.columnIndex||a.rowIndex-b.rowIndex);
                        const areas=[];for(const item of selected){const last=areas.at(-1);if(last&&last.columnIndex===item.columnIndex&&last.rowIndex+last.rowCount===item.rowIndex)last.rowCount++;else areas.push({...item});}
                        o.isNullObject=areas.length===0;o.areaCount=areas.length;o.areas.items=areas;
                    });return o;
                },
                getUsedRangeOrNullObject(valuesOnly) {
                    if (valuesOnly !== true) throw Error("Expected values-only normalization");
                    const used = { load() { queue.push(() => {
                        const matching = [...cells].map(([key, value]) => ({ parts: key.split(':'), value }))
                            .filter(x => x.parts[0] === data.id && Number(x.parts[1]) >= row && Number(x.parts[1]) < row + rows &&
                                Number(x.parts[2]) >= col && Number(x.parts[2]) < col + cols && x.value.type !== 'Empty' &&
                                !(state.ignoreEmptyFormulaValues && x.value.hasFormula && x.value.value === ''));
                        used.isNullObject = matching.length === 0;
                        if (!used.isNullObject) {
                            const r = matching.map(x => Number(x.parts[1])), c = matching.map(x => Number(x.parts[2]));
                            Object.assign(used, {rowIndex: Math.min(...r) - 1, columnIndex: Math.min(...c) - 1,
                                rowCount: Math.max(...r) - Math.min(...r) + 1, columnCount: Math.max(...c) - Math.min(...c) + 1});
                        }
                    }); return used; } };
                    return used;
                },
                getCell(r, c) {
                    const k = key(data.id, row + r, col + c);
                    function properties(store, id, names) {
                        const loaded = {}, proxy = { load() { queue.push(() => { Object.assign(loaded, store.get(id)); }); } };
                        for (const name of names) Object.defineProperty(proxy, name, {enumerable: true, get() { return loaded[name]; }, set(value) { queue.push(() => { const current=store.get(id)||{};current[name]=value;store.set(id,current); }); }});
                        return proxy;
                    }
                    const format = properties(formats, k, ['horizontalAlignment']);
                    format.font = properties(fonts, k, ['bold','color','name','size']);
                    format.fill = properties(fills, k, ['pattern','color']);
                    format.fill.clear = () => queue.push(() => { fills.set(k, {pattern:'None',color:null}); });
                    format.borders = { getItem: edge => {
                        const id=k+':'+edge, aliases={EdgeTop:[row+r-1,col+c,'EdgeBottom'],EdgeBottom:[row+r+1,col+c,'EdgeTop'],EdgeLeft:[row+r,col+c-1,'EdgeRight'],EdgeRight:[row+r,col+c+1,'EdgeLeft']};
                        const shared=borders.get(id)||{};borders.set(id,shared);
                        if(aliases[edge]){const [rr,cc,other]=aliases[edge];borders.set(key(data.id,rr,cc)+':'+other,shared);}
                        return properties(borders,id,['style','weight','color']);
                    } };
                    const result = { format, load() { queue.push(() => { result.hasFormula = cell(data.id, row + r, col + c).hasFormula || false; }); } };
                    return result;
                } };
            return obj;
        }
        const collection = { items: [], load() { queue.push(() => { collection.items = [...sheets.values()].map(d => ({ ...d })); }); },
            getItem(id) { return sheet(sheets.get(id) || [...sheets.values()].find(s => s.name === id)); }, add(name) {
                const data = { id: `new-${++state.added}`, name };
                queue.push(() => { if ([...sheets.values()].some(s => s.name.toUpperCase() === name.toUpperCase())) throw new Error("name collision"); sheets.set(data.id, data); state.mutations++; state.emit("onAdded"); });
                // Pending worksheet is usable before creation sync, as in Office.js.
                return sheet(data);
            } };
        for (const name of ["onChanged", "onSelectionChanged", "onNameChanged", "onDeleted", "onAdded", "onActivated", "onProtectionChanged", "onFormatChanged"]) collection[name] = event(name);
        const context = { workbook: { worksheets: collection, getSelectedRange() {
            if (state.multiple) throw Object.assign(new Error(), { code: "InvalidSelection" });
            return range(sheets.get(state.sheetId), state.selected.row, state.selected.col, state.selected.rows, state.selected.cols);
        }, protection: { protected: false, load() { queue.push(() => { this.protected = state.bookProtected; }); } }, onActivated: event("workbook-activated") },
            async sync() { check(); state.syncs++; await state.hook(state.syncs); for (const fn of queue.splice(0)) fn(); } };
        try { return await callback(context); } finally { alive = false; }
    } } };
    return { scope, state, sheets, cells, fonts, fills, borders, formats };
}
