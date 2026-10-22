import { readFileSync } from "node:fs";

export const canonical = JSON.parse(readFileSync(new URL("../../../BESHStat.Excel.Common.Tests/TestData/Acquisition/canonical-two-ranges.json", import.meta.url), "utf8"));
function column(letters) { return [...letters].reduce((n, c) => n * 26 + c.charCodeAt(0) - 64, 0); }
function letters(value) { let result = ""; for (; value; value = Math.floor((value - 1) / 26)) result = String.fromCharCode(65 + (value - 1) % 26) + result; return result; }
export function fakeWorkbook(index = 0) {
    const fixture = canonical.Ranges[index];
    const state = { runs: 0, syncs: 0, payloads: [], mutations: 0, hook: async () => {}, multiple: false,
        sheet: { id: "native-sheet", name: fixture.WorksheetName, deleted: false },
        selected: { row: fixture.StartRow, col: fixture.StartColumn, rows: fixture.RowCount, cols: fixture.ColumnCount },
        document: {}, cell: (row, col) => fixture.Rows[row - fixture.StartRow]?.[col - fixture.StartColumn] ??
            {Kind:1,NumberValue:0,TextValue:"",BooleanValue:false,NumberFormat:"General",NativeErrorCode:""} };
    const scope = { Office: { onReady: async () => ({host:"Excel",platform:"OfficeOnline"}),
        context: { get document() { return state.document; }, requirements: { isSetSupported: (name, version) => name === "ExcelApi" && version === "1.1" } } },
        Excel: { async run(callback) {
            state.runs++;
            let alive = true;
            const pending = [];
            const check = () => { if (!alive) throw new Error("proxy reused across runs"); };
            function range(geometry) {
                const result = { worksheet: sheet(), load(properties) { check(); pending.push(() => {
                    const g = geometry();
                    Object.assign(result, {rowIndex:g.row - 1,columnIndex:g.col - 1,rowCount:g.rows,columnCount:g.cols,
                        address: state.addressOverride || `'${state.sheet.name.replace(/'/g,"''")}'!$${letters(g.col)}$${g.row}:$${letters(g.col+g.cols-1)}$${g.row+g.rows-1}`});
                    if (properties.includes("values")) {
                        state.payloads.push({ rows:g.rows, cols:g.cols });
                        const matrix = field => Array.from({length:g.rows}, (_, r) => Array.from({length:g.cols}, (_, c) => field(state.cell(g.row+r,g.col+c))));
                        result.values = matrix(cell => [null,cell.NumberValue,cell.TextValue,cell.BooleanValue,cell.TextValue][cell.Kind]);
                        result.valueTypes = matrix(cell => ["Empty","Double","String","Boolean","Error"][cell.Kind]);
                        result.numberFormat = matrix(cell => cell.NumberFormat);
                        state.payloadHook?.(result);
                    }
                }); return result; } };
                return result;
            }
            function sheet() {
                const result = { load() { check(); pending.push(() => {
                    if (state.sheet.deleted) throw Object.assign(new Error("SECRET"), {code:"ItemNotFound"});
                    result.id=state.sheet.id; result.name=state.sheet.name;
                }); return result; }, getRange(value) {
                    check();
                    const m = /^([A-Z]+)(\d+):([A-Z]+)(\d+)$/.exec(value);
                    if (!m) throw new Error("invalid generated A1");
                    return range(() => ({row:Number(m[2]),col:column(m[1]),rows:Number(m[4])-Number(m[2])+1,cols:column(m[3])-column(m[1])+1}));
                } };
                return result;
            }
            const context = { workbook: { getSelectedRange() {
                check();
                if (state.multiple) throw Object.assign(new Error("SECRET"),{code:"InvalidSelection"});
                return range(() => state.selected);
            }, worksheets: { getItem(id) { check(); if (id !== state.sheet.id || state.sheet.deleted) throw Object.assign(new Error("SECRET"),{code:"ItemNotFound"}); return sheet(); } } },
                async sync() { check(); state.syncs++; await state.hook(state.syncs); for (const apply of pending.splice(0)) apply(); } };
            try { return await callback(context); } finally { alive = false; }
        } } };
    return {scope, state, fixture};
}
