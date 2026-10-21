"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const { recordingWorkbook } = require("./helpers/recordingWorkbook.cjs");
const fixture = () => JSON.parse(fs.readFileSync(path.join(__dirname, "fixtures/output-plans/four-tables-c6.json"), "utf8"));

test("execution cancellation discards consent without any Excel transaction or mutation", async () => {
    const mock = recordingWorkbook();
    mock.cells.set("5:2", { kind: "string", value: "sentinel", style: {} });
    const pending = await mock.bridge.writeOutputPlan(fixture());
    assert.equal(pending.state, "AwaitingConfirmation");
    const runs = mock.trace.runs;
    const before = JSON.stringify([...mock.cells]);
    assert.equal(mock.bridge.discardOutputConfirmation(pending.confirmation.token), true);
    assert.equal(mock.bridge.discardOutputConfirmation(pending.confirmation.token), false);
    assert.equal(mock.trace.runs, runs);
    assert.equal(JSON.stringify([...mock.cells]), before);
    const stale = await mock.bridge.writeOutputPlan(fixture(), { token: pending.confirmation.token, approved: true });
    assert.equal(stale.state, "RejectedBeforeWrite");
    assert.equal(stale.code, "output.consent-stale");
    assert.equal(mock.trace.writes.length, 0);
});

test("discard is scoped to one execution's consent", async () => {
    const mock = recordingWorkbook();
    mock.cells.set("5:2", { kind: "string", value: "sentinel", style: {} });
    const first = await mock.bridge.writeOutputPlan(fixture());
    const second = await mock.bridge.writeOutputPlan(fixture());
    mock.bridge.discardOutputConfirmation(first.confirmation.token);
    const result = await mock.bridge.writeOutputPlan(fixture(), { token: second.confirmation.token, approved: false });
    assert.equal(result.state, "CancelledByUser");
    assert.equal(mock.trace.writes.length, 0);
});
