"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const { recordingWorkbook } = require("./helpers/recordingWorkbook.cjs");
const fixture = () => JSON.parse(fs.readFileSync(path.join(__dirname, "fixtures/output-plans/four-tables-c6.json"), "utf8"));
const metadata = mock => ({ worksheetId: mock.sheets[0].id, worksheetName: mock.sheets[0].name,
    startRow: 1, startColumn: 1, rowCount: 3, columnCount: 2 });
async function begin(mock, plan = fixture(), copied = true, areas = false) {
    const binding = await mock.bridge.beginDocumentLifecycle({ target: plan.target, selectedAreas: areas });
    await mock.bridge.acceptLifecycleInput(binding.handle, metadata(mock), copied);
    return binding;
}
function noWrites(mock) {
    for (const name of ["adds", "clears", "writes", "styles"]) assert.deepEqual(mock.trace[name], [], name);
}
async function flush() { await new Promise(resolve => setImmediate(resolve)); }

test("real sheet IDs and context continuity, with no fabricated workbook ID or revision", async () => {
    const mock = recordingWorkbook({ lifecycleEvents: true });
    const binding = await begin(mock);
    assert.equal(binding.workbookIdentity, "");
    assert.equal(binding.selectionRevision, "");
    assert.equal(binding.worksheetIdentity, mock.sheets[0].id);
    assert.equal(binding.eventsRegistered, 4);
    assert.equal(await mock.bridge.verifyDocumentLifecycle(binding.handle), true);
    await mock.bridge.disposeDocumentLifecycle(binding.handle);
    assert.equal(mock.trace.eventRemoves.length, 4);
});

test("pinned output remains on the original sheet after input copy and unrelated selection/activation", async () => {
    const mock = recordingWorkbook({ lifecycleEvents: true }), plan = fixture();
    const binding = await begin(mock, plan);
    mock.options.activeIndex = 1; mock.options.selection = [8, 8, 1, 1];
    mock.fire("onSelectionChanged", { worksheetId: mock.sheets[0].id, address: "I9" });
    mock.fire("onActivated", { worksheetId: mock.sheets[1].id });
    const result = await mock.bridge.writeOutputPlan(plan, null, binding.handle);
    assert.equal(result.state, "Completed");
    assert.equal(result.worksheetName, "Actual");
    assert.equal(mock.sheets[0].cells.get("5:2").style["font.name"], "Calibri Light");
    assert.equal(mock.sheets[1].cells.size, 0);
});

for (const events of [true, false]) for (const change of ["rename", "delete", "replace", "context", "missing-id"]) {
    test(`${events ? "events" : "no-event fallback"}: ${change} rejects before any mutation`, async () => {
        const mock = recordingWorkbook({ lifecycleEvents: events }), plan = fixture();
        const binding = await begin(mock, plan);
        if (change === "rename") { mock.sheets[0].name = "Renamed"; if (events) mock.fire("onNameChanged", { worksheetId: mock.sheets[0].id }); }
        if (change === "delete") { const sheet = mock.sheets.shift(); if (events) mock.fire("onDeleted", { worksheetId: sheet.id }); }
        if (change === "replace") { mock.sheets[0].id = "same-name-new-sheet"; }
        if (change === "context") mock.sandbox.Office.context.document = {};
        if (change === "missing-id") mock.sheets[0].id = "";
        await flush(); // Scoped event removal uses its original read-only host context.
        const before = mock.trace.runs;
        const result = await mock.bridge.writeOutputPlan(plan, null, binding.handle);
        assert.equal(result.state, "RejectedBeforeWrite");
        assert.equal(result.code, "output.lifecycle-stale");
        if (change === "context" || (events && ["rename", "delete"].includes(change))) assert.equal(mock.trace.runs, before);
        noWrites(mock);
    });
}

for (const events of [true, false]) test(`volatile preview/acquisition notices selection changes (${events})`, async () => {
    const mock = recordingWorkbook({ lifecycleEvents: events }), binding = await begin(mock, fixture(), false, true);
    mock.options.selection = [4, 0, 3, 2];
    if (events) mock.fire("onSelectionChanged", { worksheetId: mock.sheets[0].id, address: "A5:B7" });
    await assert.rejects(mock.bridge.verifyDocumentLifecycle(binding.handle), /original document/);
    noWrites(mock);
});

test("same-address notification is benign, double disposal detaches once and late callbacks do nothing", async () => {
    let callbacks = 0;
    const mock = recordingWorkbook({ lifecycleEvents: true });
    const binding = await mock.bridge.beginDocumentLifecycle({}, { invokeMethodAsync() { callbacks++; return Promise.resolve(); } });
    mock.fire("onSelectionChanged", { worksheetId: mock.sheets[0].id, address: "A1:B3" });
    assert.equal(await mock.bridge.verifyDocumentLifecycle(binding.handle), true);
    const late = [...mock.handlers.values()].flat().map(item => item.callback);
    assert.equal(await mock.bridge.disposeDocumentLifecycle(binding.handle), true);
    assert.equal(await mock.bridge.disposeDocumentLifecycle(binding.handle), false);
    const runs = mock.trace.runs;
    for (const callback of late) callback({ worksheetId: mock.sheets[0].id, address: "A9:B10" });
    await flush();
    assert.equal(callbacks, 0); assert.equal(mock.trace.runs, runs);
    assert.equal(mock.trace.eventRemoves.length, 4);
});

test("registration failure removes partial subscriptions and retains boundary revalidation", async () => {
    const mock = recordingWorkbook({ lifecycleEvents: true, failEventRegistration: "onNameChanged" });
    const binding = await begin(mock);
    assert.equal(binding.eventsRegistered, 0);
    assert.deepEqual(mock.trace.eventRemoves, mock.trace.eventAdds);
    mock.sheets[0].name = "changed";
    await assert.rejects(mock.bridge.verifyDocumentLifecycle(binding.handle));
    noWrites(mock);
});

test("API set 1.7 does not advertise or subscribe the 1.17 name event", async () => {
    const mock = recordingWorkbook({ lifecycleEvents: true });
    mock.sandbox.Office.context.requirements.isSetSupported = (_, version) => ["1.1", "1.2", "1.7", "1.13"].includes(version);
    const binding = await begin(mock);
    assert.equal(binding.eventsRegistered, 3);
    assert.ok(!mock.trace.eventAdds.includes("onNameChanged"));
    mock.sheets[0].name = "renamed";
    await assert.rejects(mock.bridge.verifyDocumentLifecycle(binding.handle));
});

test("lifecycle invalidation clears only its own overwrite token; disposal cannot discard another run's consent", async () => {
    const mock = recordingWorkbook(), plan = fixture();
    mock.cells.set("5:2", { kind: "text", value: "existing", style: {} });
    const first = await begin(mock, plan), second = await begin(mock, plan);
    const one = await mock.bridge.writeOutputPlan(plan, null, first.handle);
    const two = await mock.bridge.writeOutputPlan(plan, null, second.handle);
    assert.equal(mock.bridge.discardOutputConfirmation(two.confirmation.token, first.handle), false);
    await mock.bridge.disposeDocumentLifecycle(first.handle);
    const runs = mock.trace.runs;
    const stale = await mock.bridge.writeOutputPlan(plan, { token: one.confirmation.token, approved: true }, first.handle);
    assert.equal(stale.code, "output.lifecycle-stale"); assert.equal(mock.trace.runs, runs);
    const cancelled = await mock.bridge.writeOutputPlan(plan, { token: two.confirmation.token, approved: false }, second.handle);
    assert.equal(cancelled.state, "CancelledByUser");
    noWrites(mock);
});

test("changed fingerprint or protection invalidates approval on the bound sheet", async () => {
    for (const change of ["fingerprint", "protection"]) {
        const mock = recordingWorkbook(), plan = fixture(), binding = await begin(mock, plan);
        mock.cells.set("5:2", { kind: "text", value: "existing", style: {} });
        const pending = await mock.bridge.writeOutputPlan(plan, null, binding.handle);
        if (change === "fingerprint") mock.cells.get("5:2").value = "changed";
        else mock.sheets[0].protection.protected = true;
        const result = await mock.bridge.writeOutputPlan(plan, { token: pending.confirmation.token, approved: true }, binding.handle);
        assert.equal(result.state, "RejectedBeforeWrite"); noWrites(mock);
    }
});

test("new worksheet verifies source before creation and reports mutation uncertainty after creation", async () => {
    for (const failure of ["before", "after"]) {
        const mock = recordingWorkbook({ failWrite: failure === "after" ? 1 : undefined }), plan = fixture();
        plan.target.destination = 1;
        const binding = await begin(mock, plan);
        if (failure === "before") mock.sheets[0].id = "replacement";
        const result = await mock.bridge.writeOutputPlan(plan, null, binding.handle);
        assert.equal(result.state, failure === "before" ? "RejectedBeforeWrite" : "PartiallyWritten");
        if (failure === "before") noWrites(mock);
        else { assert.equal(mock.trace.adds.length, 1); assert.equal(result.mutationMayHaveOccurred, true); }
    }
});

test("unknown sheet identity cannot acquire a lease; disposed/unloaded context schedules no Excel batch", async () => {
    for (const id of ["", " "]) {
        const unknown = recordingWorkbook(); unknown.sheets[0].id = id;
        await assert.rejects(unknown.bridge.beginDocumentLifecycle({}));
    }
    const mock = recordingWorkbook({ lifecycleEvents: true }), binding = await begin(mock);
    const runs = mock.trace.runs;
    mock.unload();
    await flush();
    assert.equal(mock.trace.runs, runs);
    assert.equal(await mock.bridge.disposeDocumentLifecycle(binding.handle), false);
    await assert.rejects(mock.bridge.verifyDocumentLifecycle(binding.handle));
});

test("subsequent run gets a fresh lease and successful output is immutable after a late event", async () => {
    const mock = recordingWorkbook({ lifecycleEvents: true }), plan = fixture();
    const first = await begin(mock, plan);
    await mock.bridge.disposeDocumentLifecycle(first.handle);
    const second = await begin(mock, plan);
    assert.notEqual(first.handle, second.handle);
    const result = await mock.bridge.writeOutputPlan(plan, null, second.handle);
    mock.fire("onDeleted", { worksheetId: second.worksheetIdentity });
    await flush();
    assert.equal(result.state, "Completed");
});

test("an explicitly pinned source ignores unrelated selection before copying; matching IDs still get revalidated", async () => {
    const mock = recordingWorkbook({ lifecycleEvents: true });
    const binding = await mock.bridge.beginDocumentLifecycle({
        sourceReference: { worksheetName: "Actual", address: "A1:B3" }
    });
    mock.options.selection = [9, 9, 1, 1]; mock.options.selectedIndex = 1;
    mock.fire("onSelectionChanged", { worksheetId: mock.sheets[1].id, address: "J10" });
    assert.equal(await mock.bridge.verifyDocumentLifecycle(binding.handle), true);
    mock.sheets[0].id = "replacement";
    await assert.rejects(mock.bridge.verifyDocumentLifecycle(binding.handle));
    noWrites(mock);
});

for (const destination of [0, 1]) test(`last pre-mutation sync rejects document change, destination ${destination}`, async () => {
    const mock = recordingWorkbook(), plan = fixture(); plan.target.destination = destination;
    const binding = await begin(mock, plan);
    mock.options.onSync = () => { mock.sandbox.Office.context.document = {}; };
    const result = await mock.bridge.writeOutputPlan(plan, null, binding.handle);
    assert.equal(result.state, "RejectedBeforeWrite");
    assert.equal(result.code, "output.lifecycle-stale");
    noWrites(mock);
});

test("plan destination cannot be changed under a captured lease and metadata is bounded", async () => {
    const mock = recordingWorkbook(), plan = fixture(), binding = await begin(mock, plan);
    plan.target.startRow = 7;
    const runs = mock.trace.runs;
    const result = await mock.bridge.writeOutputPlan(plan, null, binding.handle);
    assert.equal(result.state, "RejectedBeforeWrite"); assert.equal(mock.trace.runs, runs);
    await assert.rejects(mock.bridge.beginDocumentLifecycle({ sourceReference: { worksheetName: "x".repeat(257), address: "A1:B3" } }));
    noWrites(mock);
});

test("failed native removal cannot accumulate more sinks; closed callbacks remain inert and next lease uses polling", async () => {
    const mock = recordingWorkbook({ lifecycleEvents: true, failEventRemoval: true });
    const first = await begin(mock);
    await mock.bridge.disposeDocumentLifecycle(first.handle);
    const adds = mock.trace.eventAdds.length;
    const next = await begin(mock);
    assert.equal(next.eventsRegistered, 0);
    assert.equal(mock.trace.eventAdds.length, adds);
    mock.fire("onDeleted", { worksheetId: first.worksheetIdentity });
    assert.equal(await mock.bridge.verifyDocumentLifecycle(next.handle), true);
    mock.sheets[0].id = "replaced";
    await assert.rejects(mock.bridge.verifyDocumentLifecycle(next.handle));
    noWrites(mock);
});
