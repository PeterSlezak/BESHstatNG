import test from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { createOfficeHost, create } from "../wwwroot/js/officeHost.mjs";

test("create returns the adapter for Blazor's IJSObjectReference projection", () => {
    const adapter = create();
    assert.equal(typeof adapter.initialize, "function");
    assert.equal(typeof adapter.dispose, "function");
    assert.equal(adapter.__jsObjectId, undefined, "Blazor must perform the only reference wrapping");
    adapter.dispose();
});

test("Office.js CDN is a blocking head script, loaded before Blazor", () => {
    const html = readFileSync(new URL("../wwwroot/index.html", import.meta.url), "utf8");
    const head = html.match(/<head>([\s\S]*?)<\/head>/i)?.[1];
    assert.ok(head, "document must have a head");
    const script = head.match(/<script\b[^>]*\bid="office-js"[^>]*><\/script>/i)?.[0];
    assert.ok(script, "Office.js CDN script must be in the head");
    assert.match(script, /src="https:\/\/officeapis\.public\.onecdn\.static\.microsoft\/1\/office\.js"/);
    assert.doesNotMatch(script, /\s(?:async|defer)\b/i,
        "Office.js must not be loaded asynchronously or deferred");
    assert.ok(html.indexOf(script) < html.indexOf("_framework/blazor.webassembly.js"),
        "Office.js must precede the framework bootstrap");
});

function office(host = "Excel", platform = "OfficeOnline", supported = true) {
    return { Office: { onReady: async () => ({ host, platform }), context: { requirements: {
        isSetSupported: (name, version) => { assert.equal(name, "ExcelApi"); assert.equal(version, "1.1"); return supported; }
    } } } };
}
function deferred() {
    let resolve, reject;
    const promise = new Promise((yes, no) => { resolve = yes; reject = no; });
    return { promise, resolve, reject };
}

test("Office absent is a safe standalone browser", async () => {
    const result = await createOfficeHost({}).initialize();
    assert.equal(result.state, "standalone");
    assert.equal(result.officeReady, false);
    assert.equal(result.excelApiSupported, false);
});

test("Excel is reported only after async onReady, without workbook access", async () => {
    const scope = office();
    const ready = deferred();
    scope.Office.onReady = () => ready.promise;
    let inspected = false;
    const context = scope.Office.context;
    Object.defineProperty(scope.Office, "context", { get() { assert.equal(inspected, true); return context; } });
    Object.defineProperty(context, "document", { get() { assert.fail("workbook access"); } });
    Object.defineProperty(scope, "Excel", { get() { assert.fail("Excel.run access"); } });
    const result = createOfficeHost(scope).initialize();
    inspected = true;
    ready.resolve({ host: "Excel", platform: "OfficeOnline" });
    assert.deepEqual(await result, { state: "excel", code: "office.excel-ready", platform: "OfficeOnline", officeReady: true, excelApiSupported: true });
});

for (const platform of ["PC", "Mac", "OfficeOnline"]) test(`supported Excel platform ${platform}`, async () => {
    assert.equal((await createOfficeHost(office("Excel", platform)).initialize()).platform, platform);
});

for (const host of ["Word", "Outlook", "Unknown", "excel", {}, 1]) test(`unsupported host ${JSON.stringify(host)}`, async () => {
    const scope = office(host);
    scope.Office.context.requirements.isSetSupported = () => assert.fail("other host requirements accessed");
    const result = await createOfficeHost(scope).initialize();
    assert.equal(result.state, "unsupported");
    assert.equal(result.code, "office.unsupported-host");
    assert.equal(result.excelApiSupported, false);
});

test("Office onReady outside Office returns null host/platform", async () => {
    assert.equal((await createOfficeHost(office(null, null)).initialize()).state, "standalone");
});

for (const supported of [false, "true", 1, null]) test(`requirement support must be Boolean true: ${supported}`, async () => {
    const result = await createOfficeHost(office("Excel", "PC", supported)).initialize();
    assert.equal(result.state, "unsupported");
    assert.equal(result.code, "office.unsupported-api");
});

test("missing requirement API is unsupported", async () => {
    const scope = office();
    scope.Office.context = {};
    assert.equal((await createOfficeHost(scope).initialize()).code, "office.unsupported-api");
});

for (const stage of ["onReady", "requirements", "context"]) test(`JS exception at ${stage} is projected safely`, async () => {
    const scope = office();
    const fail = () => { throw new Error("SECRET address and HTTP response"); };
    if (stage === "onReady") scope.Office.onReady = fail;
    if (stage === "requirements") scope.Office.context.requirements.isSetSupported = fail;
    if (stage === "context") Object.defineProperty(scope.Office, "context", { get: fail });
    const result = await createOfficeHost(scope).initialize();
    assert.equal(result.code, "office.initialization-failed");
    assert.equal(JSON.stringify(result).includes("SECRET"), false);
});

test("onReady rejection is handled without raw error output", async () => {
    const scope = office();
    scope.Office.onReady = async () => { throw new Error("SECRET"); };
    assert.equal((await createOfficeHost(scope).initialize()).code, "office.initialization-failed");
});

for (const value of [undefined, null, {}, { host: "Excel" }]) test(`invalid onReady return ${JSON.stringify(value)}`, async () => {
    const scope = office();
    scope.Office.onReady = () => value;
    assert.equal((await createOfficeHost(scope).initialize()).state, "failed");
});

test("missing onReady cannot use cached context as proof", async () => {
    const scope = office();
    delete scope.Office.onReady;
    scope.Office.context.host = "Excel";
    assert.equal((await createOfficeHost(scope).initialize()).state, "failed");
});

test("duplicate calls share startup and terminal result; explicit retry can recover", async () => {
    const scope = office();
    const ready = deferred();
    let calls = 0;
    scope.Office.onReady = () => { calls++; return ready.promise; };
    const adapter = createOfficeHost(scope);
    const first = adapter.initialize();
    assert.equal(first, adapter.initialize(true));
    ready.reject(new Error("transient"));
    assert.equal((await first).state, "failed");
    assert.equal(first, adapter.initialize());
    scope.Office.onReady = async () => { calls++; return { host: "Excel", platform: "PC" }; };
    assert.equal((await adapter.initialize(true)).state, "excel");
    assert.equal(calls, 2);
});

test("timeout is bounded; late completion cannot change result or inspect context", async () => {
    const scope = office();
    const ready = deferred();
    scope.Office.onReady = () => ready.promise;
    Object.defineProperty(scope.Office, "context", { get() { assert.fail("late context read"); } });
    const adapter = createOfficeHost(scope, 10);
    const result = await adapter.initialize();
    assert.equal(result.code, "office.timeout");
    ready.resolve({ host: "Excel" });
    await Promise.resolve();
    assert.equal(await adapter.initialize(), result);
});

test("dispose cancels pending readiness and ignores late callbacks idempotently", async () => {
    const scope = office();
    const ready = deferred();
    scope.Office.onReady = () => ready.promise;
    Object.defineProperty(scope.Office, "context", { get() { assert.fail("disposed context read"); } });
    const adapter = createOfficeHost(scope);
    const pending = adapter.initialize();
    adapter.dispose(); adapter.dispose();
    assert.equal((await pending).state, "disposed");
    ready.resolve({ host: "Excel" });
    await Promise.resolve();
    assert.equal((await adapter.initialize(true)).state, "disposed");
});

function scriptScope() {
    const script = new EventTarget();
    let listeners = 0;
    const add = script.addEventListener.bind(script), remove = script.removeEventListener.bind(script);
    script.addEventListener = (...args) => { listeners++; add(...args); };
    script.removeEventListener = (...args) => { listeners--; remove(...args); };
    return { script, scope: { document: { getElementById: () => script } }, listeners: () => listeners };
}

test("async CDN load reaches Office readiness and removes listeners", async () => {
    const mock = scriptScope();
    const adapter = createOfficeHost(mock.scope);
    const result = adapter.initialize();
    mock.scope.Office = office().Office;
    mock.script.dispatchEvent(new Event("load"));
    assert.equal((await result).state, "excel");
    assert.equal(mock.listeners(), 0);
});

for (const ending of ["error", "timeout", "dispose"]) test(`CDN ${ending} is bounded and removes listeners`, async () => {
    const mock = scriptScope();
    const adapter = createOfficeHost(mock.scope, 10);
    const result = adapter.initialize();
    if (ending === "error") mock.script.dispatchEvent(new Event("error"));
    if (ending === "dispose") adapter.dispose();
    assert.equal((await result).state, ending === "dispose" ? "disposed" : "standalone");
    assert.equal(mock.listeners(), 0);
});

test("untrusted platform is discarded and no extra metadata is returned", async () => {
    const result = await createOfficeHost(office("Excel", "SECRET URL")).initialize();
    assert.equal(result.platform, "");
    assert.deepEqual(Object.keys(result).sort(), ["code", "excelApiSupported", "officeReady", "platform", "state"]);
});

test("invalid initialize argument fails safely without poisoning later startup", async () => {
    const adapter = createOfficeHost(office());
    assert.equal((await adapter.initialize("secret")).state, "failed");
    assert.equal((await adapter.initialize()).state, "excel");
});

test("browser absence can recover when Office subsequently loads", async () => {
    const scope = {};
    const adapter = createOfficeHost(scope);
    assert.equal((await adapter.initialize()).state, "standalone");
    scope.Office = office().Office;
    assert.equal((await adapter.initialize(true)).state, "excel");
});

test("old timed-out completion cannot overwrite a successful retry", async () => {
    const scope = office();
    const old = deferred();
    scope.Office.onReady = () => old.promise;
    const adapter = createOfficeHost(scope, 10);
    assert.equal((await adapter.initialize()).code, "office.timeout");
    scope.Office.onReady = async () => ({ host: "Excel", platform: "PC" });
    const retried = await adapter.initialize(true);
    old.resolve({ host: "Word", platform: "PC" });
    await Promise.resolve();
    assert.equal(await adapter.initialize(), retried);
    assert.equal(retried.state, "excel");
});

test("dispose before initialization never calls Office", async () => {
    const scope = office();
    scope.Office.onReady = () => assert.fail("disposed start");
    const adapter = createOfficeHost(scope);
    adapter.dispose();
    assert.equal((await adapter.initialize()).state, "disposed");
});

test("invalid global scope is safe", async () => {
    assert.equal((await createOfficeHost(null).initialize()).state, "standalone");
});

test("throwing Office global getter is safely projected", async () => {
    const scope = {};
    Object.defineProperty(scope, "Office", { get() { throw new Error("SECRET"); } });
    assert.equal((await createOfficeHost(scope).initialize()).code, "office.initialization-failed");
});

test("a real top-level window with pending Office fails safely as unavailable", async () => {
    const scope = office();
    scope.window = scope.top = scope;
    scope.Office.onReady = () => new Promise(() => {});
    Object.defineProperty(scope.Office, "context", { get() { assert.fail("pre-ready context"); } });
    assert.deepEqual(await createOfficeHost(scope, 5).initialize(), {
        state: "standalone", code: "office.unavailable", platform: "", officeReady: false, excelApiSupported: false
    });
});

test("an iframe cannot infer Excel from its surrounding window", async () => {
    const scope = office();
    scope.window = scope;
    scope.top = {};
    scope.Office.onReady = () => new Promise(() => {});
    assert.equal((await createOfficeHost(scope, 5).initialize()).code, "office.timeout");
});

test("retries reuse a pending Office promise and detach each timed-out attempt", async () => {
    const scope = office();
    const ready = deferred();
    let calls = 0;
    scope.Office.onReady = () => { calls++; return ready.promise; };
    const adapter = createOfficeHost(scope, 5);
    for (let i = 0; i < 6; i++) assert.equal((await adapter.initialize(true)).code, "office.timeout");
    assert.equal(calls, 1);
    // Late completion is retained for explicit retry, but cannot read context yet.
    const context = scope.Office.context;
    let permitted = false;
    Object.defineProperty(scope.Office, "context", { get() { assert.ok(permitted); return context; } });
    ready.resolve({ host: "Excel", platform: "OfficeOnline" });
    await Promise.resolve();
    permitted = true;
    assert.equal((await adapter.initialize(true)).code, "office.excel-ready");
    assert.equal(calls, 1);
    adapter.dispose();
});

test("delayed readiness before the deadline still verifies the API", async () => {
    const scope = office();
    scope.Office.onReady = () => new Promise(resolve => setTimeout(() => resolve({host: "Excel", platform: "OfficeOnline"}), 5));
    assert.equal((await createOfficeHost(scope, 100).initialize()).code, "office.excel-ready");
});

test("diagnostic stages are distinct from public status and cannot break readiness", async () => {
    const scope = office();
    const stages = [];
    scope.__beshOfficeDebug = { record(code) { stages.push(code); } };
    const result = await createOfficeHost(scope).initialize();
    assert.equal(result.code, "office.excel-ready");
    assert.deepEqual(stages, ["bootstrap.interop-invoked", "bootstrap.office-script-loaded", "bootstrap.onready-started",
        "bootstrap.onready-resolved", "bootstrap.excel-ready"]);
    scope.__beshOfficeDebug.record = () => { throw new Error("SECRET"); };
    assert.equal((await createOfficeHost(scope).initialize()).code, "office.excel-ready");
});

test("script already failed before initialization remains bounded and removable", async () => {
    const mock = scriptScope();
    mock.script.dispatchEvent(new Event("error"));
    assert.equal((await createOfficeHost(mock.scope, 5).initialize()).code, "office.unavailable");
    assert.equal(mock.listeners(), 0);
});

test("production deadline stays at exactly five seconds even with a larger argument", async t => {
    t.mock.timers.enable({ apis: ["setTimeout"] });
    const scope = office();
    scope.Office.onReady = () => new Promise(() => {});
    const pending = createOfficeHost(scope, 9000).initialize();
    let completed = false;
    void pending.then(() => { completed = true; });
    t.mock.timers.tick(4999);
    await Promise.resolve();
    assert.equal(completed, false);
    t.mock.timers.tick(1);
    assert.equal((await pending).code, "office.timeout");
});

test("genuine resolution immediately before the deadline wins", async t => {
    t.mock.timers.enable({ apis: ["setTimeout"] });
    const scope = office(), ready = deferred();
    scope.Office.onReady = () => ready.promise;
    const adapter = createOfficeHost(scope);
    const pending = adapter.initialize();
    t.mock.timers.tick(4999);
    ready.resolve({ host: "Excel", platform: "OfficeOnline" });
    assert.equal((await pending).code, "office.excel-ready");
    t.mock.timers.tick(1);
    assert.equal((await adapter.initialize()).code, "office.excel-ready");
});
