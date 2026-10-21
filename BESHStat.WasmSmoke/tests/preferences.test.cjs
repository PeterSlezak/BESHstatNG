"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const path = require("node:path");
const source = fs.readFileSync(path.join(__dirname, "../wwwroot/js/preferences.js"), "utf8");
const key = "eu.beshstat.wasmsmoke.preferences.v1";
const data = () => ({ schemaVersion: 1, defaultAlpha: 0.01, defaultRandomSeed: 12345, traceEnabled: false,
    pValues: { decimalPlaces: 6, smallValueDisplay: 1, useUpperBound: false } });
function storage() {
    const values = new Map();
    return { values, getItem: name => values.get(name) ?? null, setItem: (name, value) => values.set(name, value) };
}
function context(store) {
    const window = { localStorage: store };
    vm.runInNewContext(source, { window }, { filename: "preferences.js" });
    return window.beshPreferences;
}
function put(store, preferences, storageVersion = 1) {
    store.values.set(key, JSON.stringify({ storageVersion, preferences }));
}

test("missing preferences report defaults without creating storage or claiming persistence", () => {
    const store = storage();
    assert.equal(context(store).load().status, "missing");
    assert.equal(store.values.size, 0);
});

test("intentional save and a fresh JS context reload all independent preferences", () => {
    const store = storage();
    const original = data();
    assert.equal(context(store).save(original).status, "saved");
    original.defaultAlpha = 0.2;
    original.pValues.decimalPlaces = 2;
    const loaded = context(store).load();
    assert.equal(loaded.status, "valid");
    assert.deepEqual(JSON.parse(JSON.stringify(loaded.data)), data());
    loaded.data.defaultAlpha = 0.3;
    assert.equal(context(store).load().data.defaultAlpha, 0.01);
    assert.equal(store.values.size, 1);
    assert.equal(JSON.parse(store.values.get(key)).storageVersion, 1);
});

test("local analysis overrides do not persist without an intentional save", () => {
    const store = storage();
    const bridge = context(store);
    bridge.save(data());
    const before = store.values.get(key);
    const local = bridge.load().data;
    local.defaultAlpha = 0.025;
    local.pValues.useUpperBound = true;
    assert.equal(store.values.get(key), before);
    assert.equal(context(store).load().data.defaultAlpha, 0.01);
});

test("absent nested or optional fields are passed to Common for per-field defaulting", () => {
    const store = storage();
    put(store, { schemaVersion: 1, defaultAlpha: 0.01, pValues: null });
    const loaded = context(store).load();
    assert.equal(loaded.status, "valid");
    assert.equal(loaded.data.defaultAlpha, 0.01);
    assert.equal(loaded.data.pValues, null);
    assert.equal(loaded.data.defaultRandomSeed, undefined);
});

test("invalid finite bounds and enums reach the single Common normalization boundary", () => {
    const store = storage();
    const invalid = data();
    invalid.defaultAlpha = 1;
    invalid.defaultRandomSeed = 0;
    invalid.pValues.decimalPlaces = 17;
    invalid.pValues.smallValueDisplay = 99;
    put(store, invalid);
    const before = store.values.get(key);
    const loaded = context(store).load();
    assert.equal(loaded.status, "valid");
    assert.equal(loaded.data.pValues.decimalPlaces, 17);
    assert.equal(loaded.data.defaultAlpha, 1);
    assert.equal(store.values.get(key), before);
});

for (const version of [0, 2]) {
    test(`unknown storage version ${version} is preserved on read and explicit save`, () => {
        const store = storage();
        put(store, data(), version);
        const before = store.values.get(key);
        const bridge = context(store);
        assert.equal(bridge.load().status, "unsupported");
        assert.equal(bridge.save(data()).status, "unsupported");
        assert.equal(store.values.get(key), before);
    });
    test(`unknown snapshot version ${version} is not an invented migration`, () => {
        const store = storage();
        put(store, { ...data(), schemaVersion: version });
        const before = store.values.get(key);
        const bridge = context(store);
        assert.equal(bridge.load().status, "unsupported");
        assert.equal(bridge.save(data()).status, "unsupported");
        assert.equal(store.values.get(key), before);
    });
}

test("corrupt JSON and wrongly typed fields are preserved instead of overwritten", () => {
    for (const raw of ["{broken", "null", "[]", JSON.stringify({ storageVersion: 1, preferences: { ...data(), defaultAlpha: "0,01" } }),
        JSON.stringify({ storageVersion: 1, preferences: { ...data(), traceEnabled: "false" } }),
        JSON.stringify({ storageVersion: 1, preferences: { ...data(), pValues: { decimalPlaces: "6" } } })]) {
        const store = storage();
        store.values.set(key, raw);
        const bridge = context(store);
        assert.equal(bridge.load().status, "invalid");
        assert.equal(bridge.save(data()).status, "invalid");
        assert.equal(store.values.get(key), raw);
    }
});

test("unknown fields are not silently discarded or persisted as workbook data", () => {
    for (const field of ["workbookId", "worksheetId", "values", "consentToken", "futureRequired"]) {
        const store = storage();
        const bridge = context(store);
        assert.equal(bridge.save({ ...data(), [field]: "sensitive" }).status, "invalid");
        assert.equal(store.values.size, 0);
        put(store, { ...data(), [field]: "preserve" });
        const before = store.values.get(key);
        assert.equal(bridge.load().status, "invalid");
        assert.equal(bridge.save(data()).status, "invalid");
        assert.equal(store.values.get(key), before);
    }
});

test("inaccessible storage getter, disabled storage and quota failures report unavailable", () => {
    const getterContext = { window: {} };
    Object.defineProperty(getterContext.window, "localStorage", { get() { throw new Error("denied"); } });
    vm.runInNewContext(source, getterContext);
    assert.equal(getterContext.window.beshPreferences.load().status, "unavailable");
    assert.equal(getterContext.window.beshPreferences.save(data()).status, "unavailable");
    const disabled = context(undefined);
    assert.equal(disabled.load().status, "unavailable");
    assert.equal(disabled.save(data()).status, "unavailable");
    const quota = context({ getItem: () => null, setItem() { throw new Error("quota"); } });
    assert.equal(quota.load().status, "missing");
    assert.equal(quota.save(data()).status, "unavailable");
    const discarded = context({ getItem: () => null, setItem() {} });
    assert.equal(discarded.save(data()).status, "unavailable");
});

test("nonfinite numbers, locale strings and invalid seed types cannot enter browser DTO", () => {
    for (const invalid of [{ ...data(), defaultAlpha: NaN }, { ...data(), defaultAlpha: Infinity },
        { ...data(), defaultAlpha: "0,01" }, { ...data(), defaultRandomSeed: 1.5 },
        { ...data(), defaultRandomSeed: 2147483648 }]) {
        const store = storage();
        assert.equal(context(store).save(invalid).status, "invalid");
        assert.equal(store.values.size, 0);
    }
});

test("an incompatible blob appearing after load blocks a later stale save", () => {
    const store = storage();
    const bridge = context(store);
    bridge.save(data());
    bridge.load();
    put(store, data(), 99);
    const before = store.values.get(key);
    assert.equal(bridge.save(data()).status, "unsupported");
    assert.equal(store.values.get(key), before);
});
