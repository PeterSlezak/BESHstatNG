import test from "node:test";
import assert from "node:assert/strict";
import vm from "node:vm";
import { readFileSync } from "node:fs";

const source = readFileSync(new URL("../wwwroot/js/officeDiagnostics.js", import.meta.url), "utf8");
function fixture(enabled = true) {
    const listeners = new Map();
    const scope = { URL, URLSearchParams, Set,
        location: { search: enabled ? "?beshOfficeDiagnostics=1" : "", origin: "https://localhost:7141" },
        performance: { now: () => 12, getEntriesByType: () => [
            {name:"https://appsforoffice.microsoft.com/lib/1.1/hosted/en-us/office_strings.js?SECRET",startTime:1,duration:2,responseStatus:200},
            {name:"https://private.example/SECRET",startTime:1,duration:2,responseStatus:500}
        ] },
        addEventListener(name, fn) { listeners.set(name, fn); },
        removeEventListener(name) { listeners.delete(name); }
    };
    scope.top = scope;
    vm.runInNewContext(source, scope);
    return {scope, listeners};
}

test("diagnostics are opt-in with no listeners or resources when disabled", () => {
    const {scope, listeners} = fixture(false);
    scope.__beshOfficeDebug.record("bootstrap.onready-started");
    const snapshot = scope.__beshOfficeDebug.snapshot();
    assert.equal(listeners.size, 0);
    assert.equal(snapshot.events.length, 0);
    assert.equal(snapshot.resources.length, 0);
});

test("primary failure and observed dependency error have distinct evidence codes", () => {
    const {scope, listeners} = fixture();
    listeners.get("error")({target:{id:"office-js"}});
    listeners.get("error")({target:{tagName:"SCRIPT",src:"https://appsforoffice.microsoft.com/lib/1.1/hosted/office_strings.js"}});
    const events = scope.__beshOfficeDebug.snapshot().events;
    assert.equal(events[0].code, "bootstrap.office-script-missing-or-failed");
    assert.equal(events[1].code, "bootstrap.office-dependency-failed-or-blocked");
});

test("timeout and unrelated script errors never imply blocked Office dependencies", () => {
    const {scope, listeners} = fixture();
    listeners.get("error")({target:{tagName:"SCRIPT",src:"https://other.example/SECRET"}});
    scope.__beshOfficeDebug.record("bootstrap.onready-timeout");
    assert.equal(scope.__beshOfficeDebug.snapshot().events.length, 1);
});

test("CSP evidence, script load and unhandled rejection are scrubbed", () => {
    const {scope, listeners} = fixture();
    listeners.get("load")({target:{id:"office-js"}});
    listeners.get("securitypolicyviolation")({blockedURI:"https://appsforoffice.microsoft.com/lib/1.1/hosted/office_strings.js?SECRET"});
    listeners.get("securitypolicyviolation")({blockedURI:"https://appsforoffice.microsoft.com"});
    listeners.get("unhandledrejection")({reason:"SECRET"});
    scope.__beshOfficeDebug.record("SECRET");
    const text = JSON.stringify(scope.__beshOfficeDebug.snapshot());
    assert.equal(text.includes("SECRET"), false);
    assert.equal(text.includes("http"), false);
    assert.equal(scope.__beshOfficeDebug.snapshot().events.length, 3);
});

test("diagnostic events are bounded, copied and removed on pane teardown", () => {
    const {scope, listeners} = fixture();
    for (let i = 0; i < 100; i++) scope.__beshOfficeDebug.record("bootstrap.onready-started");
    const snapshot = scope.__beshOfficeDebug.snapshot();
    assert.equal(snapshot.events.length, 80);
    snapshot.events[0].code = "SECRET";
    assert.equal(scope.__beshOfficeDebug.snapshot().events[0].code, "bootstrap.onready-started");
    listeners.get("pagehide")();
    assert.equal(listeners.size, 0);
});

test("frame categories use only the selected frame origin without reading parent data", () => {
    const {scope} = fixture();
    Object.defineProperty(scope, "parent", { get() { assert.fail("parent data accessed"); } });
    scope.top = {};
    assert.equal(scope.__beshOfficeDebug.snapshot().frame, "localhost-iframe");
    scope.location.origin = "https://excel.cloud.microsoft";
    assert.equal(scope.__beshOfficeDebug.snapshot().frame, "other");
});
