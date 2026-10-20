"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const { recordingWorkbook, copy } = require("./helpers/recordingWorkbook.cjs");
const { compositeWorkbook } = require("./helpers/compositeWorkbook.cjs");
const fixture = () => JSON.parse(fs.readFileSync(path.join(__dirname, "fixtures/output-plans/four-tables-c6.json"), "utf8"));
const composite = () => JSON.parse(fs.readFileSync(path.join(__dirname, "fixtures/composite-output/composite-c10.json"), "utf8"));
function seed(mock, r, c, value, formula) {
    mock.cells.set(`${r}:${c}`, { kind: formula ? "formula" : typeof value === "string" && value.startsWith("#") ? "error" : typeof value,
        value, ...(formula ? { formula } : {}), style: { original: "preserve" } });
}
function noMutation(mock, snapshot) {
    for (const key of ["adds", "clears", "writes", "styles", "autoFits", "activations", "selections"]) assert.deepEqual(mock.trace[key], [], key);
    if (snapshot) assert.deepEqual([...mock.cells], snapshot);
}
function noUndo(result) {
    for (const key of ["undoSupported", "rollbackAttempted", "rollbackSucceeded"]) assert.equal(result[key], false);
}

for (const [label, value, formula] of [["number", 123], ["zero", 0], ["text", "sentinel"], ["error", "#DIV/0!"],
    ["true", true], ["false", false], ["formula", 2, "=1+1"], ["blank-result formula", "", '=IF(TRUE,"",1)'], ["empty text constant", ""]]) {
    for (const approved of [false, true]) test(`${label}: explicit ${approved ? "Overwrite" : "Cancel"} after read-only inspection`, async () => {
        const plan = fixture(), mock = recordingWorkbook();
        seed(mock, 5, 2, value, formula);
        const snapshot = copy([...mock.cells]);
        const pending = await mock.bridge.writeOutputPlan(plan);
        assert.equal(pending.state, "AwaitingConfirmation");
        assert.equal(pending.code, "output.confirmation-required");
        assert.equal(pending.confirmation.worksheetName, "Actual");
        assert.equal(pending.confirmation.occupiedRegionCount, 1);
        assert.equal(pending.hasFormulas, !!formula);
        assert.equal(pending.confirmation.rectangles.length, 4);
        assert.equal(pending.mutationMayHaveOccurred, false);
        noMutation(mock, snapshot); noUndo(pending);
        const reads = mock.trace.runs;
        const result = await mock.bridge.writeOutputPlan(plan, { token: pending.confirmation.token, approved });
        assert.equal(result.state, approved ? "Completed" : "CancelledByUser");
        noUndo(result);
        if (!approved) {
            assert.equal(result.code, "output.cancelled");
            assert.equal(result.mutationMayHaveOccurred, false);
            assert.equal(mock.trace.runs, reads, "Cancel does not even enter Excel.run");
            noMutation(mock, snapshot);
        } else {
            assert.equal(mock.trace.clears.length, 4);
            assert.equal(result.confirmedWritten.length, 4);
            assert.equal(result.confirmedWritten[0].address, "HOST:Actual!C6:E7");
        }
        assert.ok(!JSON.stringify(result).includes("sentinel"), "never return workbook data");
    });
}

test("format-only empty cells write without approval and disclose visual mutation", async () => {
    const mock = recordingWorkbook();
    mock.cells.set("5:2", { kind: "blank", value: null, style: { "fill.color": "#123456" } });
    const result = await mock.bridge.writeOutputPlan(fixture());
    assert.equal(result.state, "Completed");
    assert.equal(result.mutationMayHaveOccurred, true);
    assert.equal(result.confirmation, undefined);
    assert.equal(mock.trace.styles.length > 0, true);
});

test("second table requires consent; separator and outside cells never grant consent or change", async () => {
    const plan = fixture(), mock = recordingWorkbook();
    seed(mock, 8, 2, "second table"); seed(mock, 7, 2, "gap"); seed(mock, 4, 1, "outside");
    const pending = await mock.bridge.writeOutputPlan(plan);
    assert.equal(pending.confirmation.occupiedRegionCount, 1);
    assert.deepEqual(Array.from(pending.planned, p => p.artifactId), plan.tables.map(t => t.artifactId));
    await mock.bridge.writeOutputPlan(plan, { token: pending.confirmation.token, approved: true });
    assert.equal(mock.cellAt(7, 2).value, "gap"); assert.equal(mock.cellAt(4, 1).value, "outside");
    assert.equal(mock.cellAt(7, 2).style.original, "preserve");
});

test("separator-only sentinel does not prompt and remains byte-for-byte unchanged", async () => {
    const mock = recordingWorkbook(); seed(mock, 7, 2, "gap", '=IF(TRUE,"",1)');
    const before = copy(mock.cells.get("7:2"));
    const result = await mock.bridge.writeOutputPlan(fixture());
    assert.equal(result.state, "Completed"); assert.deepEqual(mock.cells.get("7:2"), before);
});

for (const change of ["value", "formula", "new-occupied-table", "active-sheet", "sheet-id", "sheet-name", "protection", "merge", "plan-value", "plan-style", "plan-target", "new-destination"]) {
    test(`approval fails closed after changed ${change}`, async () => {
        const plan = fixture(), mock = recordingWorkbook(); seed(mock, 5, 2, "old");
        const pending = await mock.bridge.writeOutputPlan(plan);
        if (change === "value") seed(mock, 5, 2, "changed");
        if (change === "formula") seed(mock, 5, 2, "old", '="old"');
        if (change === "new-occupied-table") seed(mock, 8, 2, true);
        if (change === "active-sheet") mock.options.activeIndex = 1;
        if (change === "sheet-id") mock.sheets[0].id = "replacement";
        if (change === "sheet-name") mock.sheets[0].name = "renamed";
        if (change === "protection") mock.sheets[0].protection.protected = true;
        if (change === "merge") mock.options.mergedConflict = true;
        if (change === "plan-value") plan.tables[0].cells[0].value = { kind: "text", textValue: "new", numberValue: null, booleanValue: null };
        if (change === "plan-style") plan.tables[0].cells[0].strongEmphasis = !plan.tables[0].cells[0].strongEmphasis;
        if (change === "plan-target") plan.target.worksheetName = "different";
        if (change === "new-destination") plan.target.destination = 1;
        const snapshot = copy([...mock.cells]);
        const result = await mock.bridge.writeOutputPlan(plan, { token: pending.confirmation.token, approved: true });
        assert.equal(result.state, "RejectedBeforeWrite"); assert.equal(result.mutationMayHaveOccurred, false);
        assert.ok(["output.consent-stale", "output.target-unwritable", "output.target-merged"].includes(result.code));
        noMutation(mock, snapshot);
    });
}

for (const consent of [null, { approved: true }, { token: "fake", approved: true }, { token: "fake", approved: "true" }]) {
    test(`absent or unscoped approval ${JSON.stringify(consent)} never writes`, async () => {
        const mock = recordingWorkbook(); seed(mock, 5, 2, "occupied");
        const result = await mock.bridge.writeOutputPlan(fixture(), consent);
        assert.ok(["AwaitingConfirmation", "RejectedBeforeWrite"].includes(result.state)); noMutation(mock);
    });
}

test("approval is single-use and cannot be replayed after cancellation", async () => {
    const mock = recordingWorkbook(), plan = fixture(); seed(mock, 5, 2, "occupied");
    const pending = await mock.bridge.writeOutputPlan(plan);
    const decision = { token: pending.confirmation.token, approved: false };
    await mock.bridge.writeOutputPlan(plan, decision); decision.approved = true;
    const result = await mock.bridge.writeOutputPlan(plan, decision);
    assert.equal(result.code, "output.consent-stale"); noMutation(mock);
});

test("expired consent and changed workbook context never authorize a write", async () => {
    for (const change of ["expired", "workbook"]) {
        const mock = recordingWorkbook(), plan = fixture(); seed(mock, 5, 2, "occupied");
        const pending = await mock.bridge.writeOutputPlan(plan);
        if (change === "expired") mock.sandbox.Date = { now: () => Date.now() + 300001 };
        else mock.sandbox.Office.context.document = {};
        const result = await mock.bridge.writeOutputPlan(plan, { token: pending.confirmation.token, approved: true });
        assert.equal(result.code, "output.consent-stale"); noMutation(mock);
    }
});

test("bounded occupancy bytes fail closed instead of accepting a partial observation", async () => {
    const plan = fixture(), mock = recordingWorkbook(), table = plan.tables[0];
    const template = copy(table.cells[0]);
    Object.assign(template, { role: "Body", numberFormat: "General", isPValue: false, isSignificant: false, strongEmphasis: false,
        value: { kind: "blank", numberValue: null, textValue: null, booleanValue: null } });
    Object.assign(table, { rowCount: 10000, columnCount: 1, bounds: { startRow: 6, startColumn: 3, endRow: 10005, endColumn: 3 },
        cells: Array.from({ length: 10000 }, (_, row) => ({ ...template, row, column: 0 })) });
    Object.assign(plan, { tables: [table], tableCount: 1, sourceTableCount: 1, artifactCount: 1, usedBounds: copy(table.bounds) });
    const text = "x".repeat(32767);
    for (let row = 5; row < 10005; row++) seed(mock, row, 2, text);
    const result = await mock.bridge.writeOutputPlan(plan);
    assert.equal(result.code, "output.occupancy-unverified"); assert.equal(result.state, "RejectedBeforeWrite"); noMutation(mock);
});

test("add queued before a host failure is conservatively partial even without confirmed sheet creation", async () => {
    const plan = fixture(), mock = recordingWorkbook({ failAdd: true }); plan.target.destination = 1;
    const result = await mock.bridge.writeOutputPlan(plan);
    assert.equal(result.state, "PartiallyWritten"); assert.equal(result.mutationMayHaveOccurred, true);
    assert.equal(result.confirmedWritten.length, 0); assert.equal(mock.trace.writes.length, 0); noUndo(result);
});

for (const options of [{ probes: false }, { missingOccupancy: true }, { failOccupancyProbe: true }, { failMergeProbe: true },
    { failSync: 1 }, { protectedSheet: true }, { mergedConflict: true }]) {
    test(`unsafe/unverified target ${JSON.stringify(options)} never prompts or mutates`, async () => {
        const mock = recordingWorkbook(options);
        const result = await mock.bridge.writeOutputPlan(fixture());
        assert.ok(["RejectedBeforeWrite", "FailedBeforeWrite"].includes(result.state));
        assert.notEqual(result.confirmationRequired, true); noMutation(mock); noUndo(result);
    });
}

test("read-only occupancy inspection is bounded to exact rectangles", async () => {
    const plan = fixture(), mock = recordingWorkbook(); seed(mock, 5, 2, 1);
    await mock.bridge.writeOutputPlan(plan);
    assert.equal(mock.trace.syncs, 1);
    assert.equal(mock.trace.ranges.length, plan.tables.length * 2, "one merge probe and one data read per rectangle");
    for (const indexes of mock.trace.ranges) assert.ok(plan.tables.some(t => JSON.stringify(indexes) === JSON.stringify([
        t.bounds.startRow - 1, t.bounds.startColumn - 1, t.rowCount, t.columnCount])));
    noMutation(mock);
});

for (const options of [{ failWrite: 1 }, { failWrite: 2 }, { failSync: 2 }]) {
    test(`failure after possible clear reports partial, never an exact committed set ${JSON.stringify(options)}`, async () => {
        const mock = recordingWorkbook(options);
        const result = await mock.bridge.writeOutputPlan(fixture());
        assert.equal(result.state, "PartiallyWritten"); assert.equal(result.code, "output.mutation-uncertain");
        assert.equal(result.mutationMayHaveOccurred, true); assert.equal(result.confirmedWritten.length, 0); noUndo(result);
        assert.ok(mock.trace.clears.length > 0);
    });
}

test("new worksheet collision is resolved without asking consent or altering old data", async () => {
    const plan = fixture(); plan.target.destination = 1; plan.target.worksheetName = "Actual";
    const mock = recordingWorkbook(); seed(mock, 200, 50, "outside");
    const result = await mock.bridge.writeOutputPlan(plan);
    assert.equal(result.state, "Completed"); assert.equal(result.worksheetName, "Actual_2");
    assert.equal(result.confirmation, undefined); assert.equal(mock.sheets[0].cells.get("200:50").value, "outside");
});

test("new sheet remains honestly reported if creation or first write fails", async () => {
    const plan = fixture(); plan.target.destination = 1;
    const mock = recordingWorkbook({ failWrite: 1 });
    const result = await mock.bridge.writeOutputPlan(plan);
    assert.equal(result.state, "PartiallyWritten"); assert.equal(result.worksheetName, "BESHStat_Result_2");
    assert.equal(result.confirmedWritten.length, 0); noUndo(result);
});

test("current-sheet composite is explicitly rejected with no acquisition or mutation", async () => {
    const plan = composite(), mock = compositeWorkbook(); plan.target.destination = 0;
    const result = await mock.bridge.writeCompositeOutputPlan(plan);
    assert.equal(result.state, "RejectedBeforeWrite"); assert.equal(result.mutationMayHaveOccurred, false);
    assert.equal(mock.trace.runs, 0); assert.deepEqual(mock.trace.adds, []); assert.deepEqual(mock.trace.writes, []);
});

test("six composite regions have structured confirmed IDs and unchanged C10 geometry", async () => {
    const plan = composite(), mock = compositeWorkbook();
    const result = await mock.bridge.writeCompositeOutputPlan(plan);
    assert.equal(result.state, "Completed"); assert.equal(result.planned.length, 6);
    assert.equal(result.confirmedWritten.length, 6); assert.equal(result.address, "HOST:Composite!C10:H31");
    assert.deepEqual(copy(Array.from(result.planned, p => p.bounds)), plan.items.filter(i => i.bounds).map(i => i.bounds)); noUndo(result);
});

test("task-pane consent awaits actual buttons with no second analysis invocation", () => {
    const source = fs.readFileSync(path.join(__dirname, "../Pages/Home.razor"), "utf8");
    assert.match(source, /ChooseOverwrite\(true\).*Overwrite<\/button>/);
    assert.match(source, /ChooseOverwrite\(false\).*Cancel<\/button>/);
    assert.match(source, /await overwriteChoice.Task/);
    assert.match(source, /@if \(writeResult\?\.State == "Completed"\)/, "success banner requires actual completed output");
    assert.match(source, /writeResult.State == "CancelledByUser" \? "alert-secondary"/);
    const method = source.slice(source.indexOf("private async Task RunSharedAnalysisAndWrite()"), source.indexOf("private string BuildSuccessStatus()"));
    assert.equal((method.match(/SharedAnalysisApplicationService.Run\(/g) || []).length, 1);
    assert.match(method, /writeResult.State == "Completed" \? BuildSuccessStatus\(\)/);
});
