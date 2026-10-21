"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const path = require("node:path");
const code = fs.readFileSync(path.join(__dirname, "../wwwroot/js/diagnostics.js"), "utf8");
const id = "4f58068b27544ba7b232c6d936bc8f28";
const record = (eventCode = "diagnostic.validation.rejected") => ({ eventCode, runId: id,
    operation: "anova.oneway", stage: "validation", metadata: { category: "input" } });
function setup(consoleValue) {
    const rows = [];
    const console = consoleValue === undefined ? Object.fromEntries(["info", "warn", "error", "debug"].map(
        name => [name, value => rows.push({ name, value })])) : consoleValue;
    const window = { console };
    Object.defineProperty(window, "localStorage", { get() { throw Error("diagnostics must never access storage"); } });
    Object.defineProperty(window, "fetch", { get() { throw Error("diagnostics must never access network"); } });
    vm.runInNewContext(code, { window }, { filename: "diagnostics.js" });
    return { sink: window.beshDiagnostics, rows, window };
}
const plain = value => JSON.parse(JSON.stringify(value));

test("one run retains its opaque ID from start through validation to end", async () => {
    const { sink, rows } = setup();
    for (const event of ["diagnostic.run.started", "diagnostic.validation.rejected", "diagnostic.run.ended"]) {
        await Promise.resolve();
        assert.equal(sink.emit(record(event)), true);
    }
    assert.deepEqual(rows.map(r => r.value.runId), [id, id, id]);
    assert.deepEqual(rows.map(r => r.name), ["info", "warn", "info"]);
});

test("parallel runs do not use a shared current ID", async () => {
    const { sink, rows } = setup();
    const other = "921938bcd6824d08852c27430c34ec48";
    await Promise.all([id, other].map(async runId => {
        sink.emit({ ...record("diagnostic.run.started"), runId });
        await Promise.resolve();
        sink.emit({ ...record("diagnostic.output.completed"), runId, stage: "output" });
        sink.emit({ ...record("diagnostic.run.ended"), runId });
    }));
    for (const runId of [id, other]) assert.equal(rows.filter(r => r.value.runId === runId).length, 3);
});

for (const payload of ["=SUM(A1:B99)", "'Payroll'!$A$1", "C:\\Users\\private\\salary.xlsx",
    "patient Alice account 12345", "input\r\nforged=secret", "input\0", "秘密🔒\u2028", "x".repeat(100000)]) {
    test(`untrusted payload is redacted (${payload.slice(0, 24)})`, () => {
        const { sink, rows } = setup();
        const raw = { ...record(), eventCode: payload, operation: payload, stage: payload, severity: payload,
            exception: new Error(payload), cells: [[payload]], metadata: { category: payload, outcome: payload, host: payload, address: payload } };
        assert.equal(sink.emit(raw), true);
        assert.deepEqual(plain(rows[0].value), { eventCode: "diagnostic.event.redacted", severity: "warning", runId: id,
            operation: "redacted", stage: "redacted", metadata: { category: "redacted", outcome: "redacted", host: "redacted" } });
    });
}

test("invalid source objects and workbook-derived IDs are refused without logging", () => {
    const { sink, rows } = setup();
    for (const value of [null, undefined, {}, [], "secret", { ...record(), runId: "Payroll!A1" }, { ...record(), runId: id + "\r\n" }])
        assert.equal(sink.emit(value), false);
    assert.equal(rows.length, 0);
});

test("allowlisted fields survive and logged object cannot retain source mutation", () => {
    const { sink, rows } = setup();
    const raw = record();
    raw.metadata.host = "office";
    raw.metadata.outcome = "rejected";
    assert.equal(sink.emit(raw), true);
    raw.metadata.category = "=SECRET()";
    raw.operation = "Payroll";
    assert.deepEqual(plain(rows[0].value.metadata), { category: "input", outcome: "rejected", host: "office" });
    assert.ok(Object.isFrozen(rows[0].value));
    assert.ok(Object.isFrozen(rows[0].value.metadata));
});

test("disabled trace suppresses debug while validation and errors remain", () => {
    const { sink, rows } = setup();
    assert.equal(sink.emit(record("diagnostic.stage.entered"), false), true);
    assert.equal(sink.emit(record(), false), true);
    assert.equal(sink.emit(record("diagnostic.run.failed"), false), true);
    assert.deepEqual(rows.map(r => r.name), ["warn", "error"]);
});

test("throwing unavailable or partial consoles cannot escape or report delivery", () => {
    for (const console of [null, {}, { warn() { throw Error("Payroll!A1"); } }]) {
        assert.equal(setup(console).sink.emit(record()), false);
    }
    const { sink, window } = setup();
    Object.defineProperty(window, "console", { get() { throw Error("private path"); } });
    assert.equal(sink.emit(record()), false);
});

test("hostile property getters and prototypes never escape or reach console", () => {
    const { sink, rows } = setup();
    const raw = record();
    Object.defineProperty(raw, "metadata", { get() { throw new Error("=SECRET()"); } });
    assert.equal(sink.emit(raw), false);
    assert.equal(sink.emit({ ...record(), eventCode: "__proto__" }), true);
    assert.equal(rows[0].value.eventCode, "diagnostic.event.redacted");
});

test("diagnostics never rewrite or interfere with 03.01 preference save/reload", () => {
    const { sink } = setup();
    const values = new Map();
    // Use a fresh preference host because diagnostics' storage getter is intentionally sealed.
    const preferenceWindow = { localStorage: { getItem: k => values.get(k) ?? null, setItem: (k, v) => values.set(k, v) } };
    const preferences = fs.readFileSync(path.join(__dirname, "../wwwroot/js/preferences.js"), "utf8");
    vm.runInNewContext(preferences, { window: preferenceWindow });
    const data = { schemaVersion: 1, traceEnabled: false, defaultAlpha: 0.01, defaultRandomSeed: 123,
        pValues: { decimalPlaces: 6, smallValueDisplay: 1, useUpperBound: false } };
    assert.equal(preferenceWindow.beshPreferences.save(data).status, "saved");
    const before = JSON.stringify([...values]);
    sink.emit(record("diagnostic.run.failed"));
    vm.runInNewContext(preferences, { window: preferenceWindow });
    assert.equal(preferenceWindow.beshPreferences.load().data.traceEnabled, false);
    assert.equal(preferenceWindow.beshPreferences.load().data.defaultAlpha, 0.01);
    assert.equal(JSON.stringify([...values]), before);
});
