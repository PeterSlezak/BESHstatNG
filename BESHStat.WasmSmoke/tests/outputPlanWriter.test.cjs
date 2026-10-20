"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const { recordingWorkbook, copy, address } = require("./helpers/recordingWorkbook.cjs");
const fixture = name => JSON.parse(fs.readFileSync(path.join(__dirname, "fixtures/output-plans", name + ".json"), "utf8"));

test("four Common-planned C6 tables record exact writes, gap isolation, envelope and host addresses", async () => {
    const plan = fixture("four-tables-c6");
    const mock = recordingWorkbook();
    const result = await mock.bridge.writeOutputPlan(plan);
    const actual = {
        writes: mock.trace.writes.map(write => ({ address: write.address, indexes: write.indexes,
            kinds: write.values.map(row => row.map(value => value === null ? "blank" : typeof value).join(",")) })),
        clears: mock.trace.clears,
        selections: mock.trace.selections,
        autoFits: mock.trace.autoFits,
        defaultStyle: mock.cellAt(5, 2).style
    };
    const expected = JSON.parse(fs.readFileSync(path.join(__dirname, "fixtures/four-tables-c6-write-trace.json"), "utf8"));
    assert.deepEqual(actual, expected);
    assert.deepEqual(Array.from(result.tableAddresses), ["HOST:Actual!C6:E7", "HOST:Actual!C9:D11", "HOST:Actual!C13:F13", "HOST:Actual!C15:C16"]);
    assert.equal(result.address, "HOST:Actual!C6:F16");
    assert.equal(result.worksheetName, "Actual");
    assert.equal(mock.trace.activeResolutions, 1);
    assert.deepEqual(mock.trace.adds, []);
    for (const r of [7, 11, 13]) for (let c = 2; c <= 5; c++) assert.equal(mock.cells.has(`${r}:${c}`), false, "separator row was untouched");
    assert.ok(mock.trace.syncs <= 2, "bounded batching, no sync per cell");
});

for (const name of ["anova-base", "anova-welch-tukey", "anova-all-posthoc", "kw-base", "kw-dunn", "typed-roles", "mixed-zero"]) {
    test(`${name}: actual System.Text.Json wire records every value/kind/style once at Common positions`, async () => {
        const plan = fixture(name);
        const mock = recordingWorkbook();
        const result = await mock.bridge.writeOutputPlan(plan);
        assert.equal(mock.trace.writes.length, plan.tableCount);
        assert.equal(mock.trace.clears.length, plan.tableCount);
        for (let index = 0; index < plan.tables.length; index++) {
            const table = plan.tables[index];
            const b = table.bounds;
            const a = address(b.startRow - 1, b.startColumn - 1, table.rowCount, table.columnCount);
            assert.deepEqual(mock.trace.writes[index].indexes, [b.startRow - 1, b.startColumn - 1, table.rowCount, table.columnCount]);
            assert.equal(result.tableAddresses[index], `HOST:Actual!${a}`);
            for (const cell of table.cells) {
                const recorded = mock.cellAt(b.startRow - 1 + cell.row, b.startColumn - 1 + cell.column);
                assert.equal(recorded.kind, cell.value.kind);
                assert.equal(recorded.value, ({ blank: null, number: cell.value.numberValue, text: cell.value.textValue, boolean: cell.value.booleanValue })[cell.value.kind]);
                assert.equal(recorded.style.numberFormat, cell.numberFormat);
                assert.equal(recorded.style["font.bold"], cell.strongEmphasis);
                assert.equal(recorded.style["font.color"], cell.isSignificant ? "#32FF32" : "#000000");
                assert.equal(recorded.style["font.size"], cell.role === "Footer" ? 8 : 10);
                assert.equal(recorded.style["fill.color"], cell.role === "TopColumnHeader" ? "#DDDDDD" : undefined);
                assert.equal(recorded.style["font.name"], "Calibri Light");
            }
        }
        assert.ok(mock.trace.syncs <= 2);
        assert.ok(mock.trace.styles.length < plan.tables.reduce((n, table) => n + table.cells.length, 0) * 5 + plan.tableCount,
            "styles coalesce horizontal runs rather than one host request per cell");
    });
}

test("numeric threshold, text p-values and duplicate body annotations use only resolved Common flags", async () => {
    const mock = recordingWorkbook();
    await mock.bridge.writeOutputPlan(fixture("typed-roles"));
    assert.equal(mock.cellAt(7, 3).value, 0.05);
    assert.equal(mock.cellAt(7, 3).style["font.color"], "#32FF32");
    assert.equal(mock.cellAt(8, 3).value, 0.049);
    assert.equal(mock.cellAt(8, 3).style["font.color"], "#32FF32");
    assert.equal(mock.cellAt(9, 3).value, 0.051);
    assert.equal(mock.cellAt(9, 3).style["font.color"], "#000000");
    assert.equal(mock.cellAt(10, 3).value, "0.002");
    assert.equal(mock.cellAt(10, 3).kind, "text");
    assert.equal(mock.cellAt(10, 3).style["font.color"], "#000000");
    assert.equal(mock.trace.styles.filter(s => s.address === "D8:D8" && s.property === "font.color").length, 1);
    assert.equal(mock.cellAt(7, 5).value, "=1+1");
    assert.equal(mock.cellAt(8, 4).kind, "text");
    assert.equal(mock.cellAt(8, 4).value, "");
    assert.equal(mock.cellAt(8, 5).value, "#N/A");
    assert.equal(mock.cellAt(7, 4).value, true);
});

test("renderer does not recompute alpha, guess roles, or construct a p-value display", async () => {
    const plan = fixture("typed-roles");
    const numeric = plan.tables[0].cells.find(c => c.value.numberValue === 0.051);
    numeric.isSignificant = true; numeric.strongEmphasis = false; numeric.role = "Title";
    const mock = recordingWorkbook();
    await mock.bridge.writeOutputPlan(plan);
    const cell = mock.cellAt(9, 3);
    assert.equal(cell.value, 0.051);
    assert.equal(cell.style["font.color"], "#32FF32");
    assert.equal(cell.style["font.bold"], false);
    assert.equal(cell.style["font.size"], 10);
});

test("new sheet preserves unique sanitization and AutoFit flags, with bounded syncs", async () => {
    const plan = fixture("four-tables-c6");
    plan.target.destination = 1;
    plan.target.worksheetName = "Results:bad";
    plan.tables.forEach((table, index) => { table.autoFit = index === 1; });
    const mock = recordingWorkbook({ names: ["Results_bad", "results_BAD_2"] });
    const result = await mock.bridge.writeOutputPlan(plan);
    assert.deepEqual(mock.trace.adds, ["Results_bad_3"]);
    assert.equal(result.worksheetName, "Results_bad_3");
    assert.deepEqual(mock.trace.autoFits, [{ address: "C9:D11", axis: "columns" }, { address: "C9:D11", axis: "rows" }]);
    assert.equal(mock.trace.activeResolutions, 0);
    assert.ok(mock.trace.syncs <= 3);
});

test("empty/nonspatial output has no fabricated range, selection, sheet or Excel.run", async () => {
    const plan = fixture("four-tables-c6");
    Object.assign(plan, { tables: [], tableCount: 0, sourceTableCount: 0, usedBounds: null });
    const mock = recordingWorkbook();
    const result = await mock.bridge.writeOutputPlan(plan);
    assert.equal(result.address, "");
    assert.deepEqual(Array.from(result.tableAddresses), []);
    assert.equal(mock.trace.runs, 0);
    assert.equal(mock.trace.syncs, 0);
    assert.deepEqual(mock.trace.adds, []);
    assert.deepEqual(mock.trace.ranges, []);
});

test("valid final worksheet cell stays in grid and preserves blank", async () => {
    const plan = fixture("four-tables-c6");
    const table = copy(plan.tables[3]);
    Object.assign(table, { documentIndex: 0, tableOrdinal: 0, rowCount: 1, columnCount: 1,
        cells: [table.cells[0]], bounds: { startRow: 1048576, endRow: 1048576, startColumn: 16384, endColumn: 16384 } });
    Object.assign(plan, { tables: [table], tableCount: 1, sourceTableCount: 1, artifactCount: 1, usedBounds: copy(table.bounds) });
    Object.assign(plan.target, { startRow: 1048576, startColumn: 16384 });
    const mock = recordingWorkbook();
    const result = await mock.bridge.writeOutputPlan(plan);
    assert.equal(result.address, "HOST:Actual!XFD1048576:XFD1048576");
    assert.deepEqual(mock.trace.writes[0].indexes, [1048575, 16383, 1, 1]);
    assert.equal(mock.cellAt(1048575, 16383).kind, "blank");
});

test("validated snapshot survives caller mutation during async host preflight", async () => {
    const plan = fixture("typed-roles");
    const mock = recordingWorkbook();
    const pending = mock.bridge.writeOutputPlan(plan);
    plan.target.destination = 1;
    plan.tables[0].bounds.startRow = 999;
    plan.tables[0].cells[0].value.textValue = "mutated";
    await pending;
    assert.deepEqual(mock.trace.adds, []);
    assert.equal(mock.cellAt(5, 2).value, "Title");
});

test("finite binary64 extremes and signed zero reach range.values without coercion", async () => {
    const plan = fixture("typed-roles");
    const values = [-0, Number.MIN_VALUE, Number.MAX_VALUE, -Number.MAX_VALUE];
    const body = plan.tables[0].cells.filter(c => c.role === "Body" && !c.isPValue);
    values.forEach((value, i) => { body[i].value = { kind: "number", numberValue: value, textValue: null, booleanValue: null }; });
    const mock = recordingWorkbook();
    await mock.bridge.writeOutputPlan(plan);
    values.forEach((value, i) => {
        assert.ok(Object.is(mock.cellAt(5 + body[i].row, 2 + body[i].column).value, value));
    });
});

test("sync failure after new sheet creation is propagated without claiming rollback", async () => {
    const plan = fixture("four-tables-c6"); plan.target.destination = 1;
    const mock = recordingWorkbook({ failSync: 2 });
    await assert.rejects(mock.bridge.writeOutputPlan(plan), /Injected Office sync failure/);
    assert.equal(mock.trace.adds.length, 1);
    assert.equal(mock.trace.writes.length, 4);
    assert.equal(mock.trace.selections.length, 1);
});

for (const options of [{ nonExcel: true }, { apiAvailable: false }]) {
    test(`unavailable host ${JSON.stringify(options)} has zero workbook effects`, async () => {
        const mock = recordingWorkbook(options);
        await assert.rejects(mock.bridge.writeOutputPlan(fixture("four-tables-c6")), /unavailable/);
        assert.equal(mock.trace.runs, 0);
        assert.equal(mock.trace.syncs, 0);
        assert.deepEqual(mock.trace.adds, []);
    });
}

for (const destination of [0, 1]) {
    test(`later host failure at destination ${destination} reports error with recorded partial effects`, async () => {
        const plan = fixture("four-tables-c6"); plan.target.destination = destination;
        const mock = recordingWorkbook({ failWrite: 2 });
        await assert.rejects(mock.bridge.writeOutputPlan(plan), /Injected Office write failure/);
        assert.equal(mock.trace.runs, 1);
        assert.equal(mock.trace.writes.length, 2);
        assert.equal(mock.trace.clears.length, 2);
        assert.equal(mock.trace.adds.length, destination);
        assert.equal(mock.trace.selections.length, 0);
        assert.equal(mock.cells.has("5:2"), true);
        assert.equal(mock.cells.has("12:2"), false);
    });
}

const mutations = {
    "null plan": () => null,
    "array plan": () => [],
    "unknown schema": p => { p.schemaVersion = 2; },
    "string schema": p => { p.schemaVersion = "1"; },
    "wrong casing": p => { p.SchemaVersion = p.schemaVersion; delete p.schemaVersion; },
    "missing target": p => { delete p.target; },
    "null target": p => { p.target = null; },
    "array target": p => { p.target = []; },
    "NewWorkbook": p => { p.target.destination = 2; },
    "unknown destination": p => { p.target.destination = 99; },
    "coerced destination": p => { p.target.destination = "0"; },
    "null sheet name": p => { p.target.worksheetName = null; },
    "zero row": p => { p.target.startRow = 0; },
    "negative row": p => { p.tables[0].bounds.startRow = -1; },
    "fractional row": p => { p.tables[0].bounds.startRow = 6.5; },
    "fractional local column": p => { p.tables[0].cells[0].column = 0.5; },
    "grid row overflow": p => { p.tables[0].bounds.endRow = 1048577; },
    "grid column overflow": p => { p.tables[0].bounds.endColumn = 16385; },
    "fractional shape": p => { p.tables[0].rowCount = 2.5; },
    "table count": p => { p.tableCount--; },
    "missing table": p => { p.tables.pop(); },
    "null tables": p => { p.tables = null; },
    "sparse tables": p => { delete p.tables[1]; },
    "too many artifacts": p => { p.artifactCount = 65; },
    "source table count": p => { p.sourceTableCount = 0; },
    "table ordering": p => { p.tables.reverse(); },
    "duplicate artifact ID": p => { p.tables[1].artifactId = p.tables[0].artifactId; },
    "blank artifact ID": p => { p.tables[0].artifactId = " "; },
    "duplicate document index": p => { p.tables[1].documentIndex = 0; },
    "duplicate table ordinal": p => { p.tables[1].tableOrdinal = 0; },
    "invalid ordinal identity": p => { p.tables[0].tableOrdinal = 1; },
    "rectangle width": p => { p.tables[0].bounds.endColumn++; },
    "rectangle height": p => { p.tables[0].bounds.endRow++; },
    "overlap": p => { p.tables[1].bounds.startRow = 6; p.tables[1].bounds.endRow = 8; },
    "target mismatch": p => { p.target.startColumn++; },
    "envelope mismatch": p => { p.usedBounds.endRow++; },
    "missing envelope": p => { p.usedBounds = null; },
    "null table": p => { p.tables[0] = null; },
    "zero area wire table": p => { p.tables[0].rowCount = 0; },
    "empty cells": p => { p.tables[0].cells = []; },
    "matrix instead of cells": p => { p.tables[0].cells = [[1, 2, 3], [4, 5, 6]]; },
    "missing cell style": p => { delete p.tables[0].cells[0].role; },
    "sparse cells": p => { delete p.tables[0].cells[0]; },
    "null cell": p => { p.tables[0].cells[0] = null; },
    "cell order": p => { p.tables[0].cells.reverse(); },
    "negative cell coordinate": p => { p.tables[0].cells[0].row = -1; },
    "string local coordinate": p => { p.tables[0].cells[0].row = "0"; },
    "unknown role": p => { p.tables[0].cells[0].role = "GuessHeader"; },
    "string style Boolean": p => { p.tables[0].cells[0].strongEmphasis = "true"; },
    "invalid significance flags": p => { p.tables[0].cells[0].isSignificant = true; },
    "missing format": p => { p.tables[0].cells[0].numberFormat = ""; },
    "constructed p format": p => { p.tables[0].cells[0].numberFormat = "0.00"; },
    "null scalar": p => { p.tables[0].cells[0].value = null; },
    "unknown discriminant": p => { p.tables[0].cells[0].value.kind = "unknown"; },
    "error scalar": p => { p.tables[0].cells[0].value.kind = "error"; },
    "formula scalar": p => { p.tables[0].cells[0].value.kind = "formula"; },
    "number coercion": p => { Object.assign(p.tables[0].cells[0].value, { kind: "number", numberValue: "0" }); },
    "NaN": p => { Object.assign(p.tables[0].cells[0].value, { kind: "number", numberValue: NaN }); },
    "Infinity": p => { Object.assign(p.tables[0].cells[0].value, { kind: "number", numberValue: Infinity }); },
    "negative Infinity": p => { Object.assign(p.tables[0].cells[0].value, { kind: "number", numberValue: -Infinity }); },
    "unknown union field": p => { p.tables[0].cells[0].value.textValue = "wrong union"; },
    "coerced Boolean": p => { Object.assign(p.tables[0].cells[0].value, { kind: "boolean", booleanValue: 1 }); },
    "overlong cell": p => { Object.assign(p.tables[0].cells[0].value, { kind: "text", textValue: "x".repeat(32768) }); },
    "AutoFit null": p => { p.tables[0].autoFit = null; },
    "resource excessive before cells allocation": p => { p.tables[0].rowCount = 10001; p.tables[0].columnCount = 10001; }
};
for (const [name, mutate] of Object.entries(mutations)) {
    test(`preflight ${name}: zero Excel.run, creation, ranges, assignments and syncs`, async () => {
        const original = fixture("four-tables-c6");
        const mutated = mutate(original);
        const mock = recordingWorkbook();
        await assert.rejects(mock.bridge.writeOutputPlan(mutated === undefined ? original : mutated), /Invalid output plan|New-workbook/);
        assert.deepEqual(mock.trace, { runs: 0, syncs: 0, adds: [], writes: [], clears: [], styles: [], autoFits: [],
            selections: [], activations: [], ranges: [], activeResolutions: 0 });
        assert.equal(mock.cells.size, 0);
    });
}

test("Home active statistical writer uses Common document/semantic/layout and planned interop only", () => {
    const source = fs.readFileSync(path.join(__dirname, "../Pages/Home.razor"), "utf8");
    const method = source.slice(source.indexOf("private async Task RunSharedAnalysisAndWrite()"), source.indexOf("private string BuildSuccessStatus()"));
    for (const call of ["OutputDocument.FromResult(sharedOutput)", "OutputSemanticPlanner.Create(document", "OutputLayoutPlanner.Create(document", "OfficeOutputPlanAdapter.Prepare(document", '"beshOffice.writeOutputPlan"']) assert.ok(method.includes(call), call);
    assert.ok(!method.includes("ResultTableWriteRequestFactory"));
    assert.ok(!method.includes("beshOffice.writeResultTables"));
    assert.equal((method.match(/SharedAnalysisApplicationService.Run\(/g) || []).length, 1);
});
