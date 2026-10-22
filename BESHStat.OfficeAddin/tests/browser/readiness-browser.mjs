// Opt-in CDP check using an already installed, owner-launched isolated browser.
// No packages, browser install, authentication, workbook APIs or existing tabs.
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
const fixture = process.argv.find(arg => arg.startsWith("--fixture="))?.split("=")[1] || "live";
assert.ok(["live", "excel", "pending", "unsupported", "missing", "dependency", "bridge-regression"].includes(fixture));
const endpoint = "http://127.0.0.1:9227";
const target = await (await fetch(`${endpoint}/json/new?about:blank`, { method: "PUT" })).json();
const socket = new WebSocket(target.webSocketDebuggerUrl);
await new Promise(resolve => socket.addEventListener("open", resolve, { once: true }));
let serial = 0;
const pending = new Map(), requests = new Map();
const summary = { fixture, simulated: fixture !== "live", resources: [], errors: [] };
function send(method, params = {}) {
    return new Promise((resolve, reject) => {
        const id = ++serial;
        pending.set(id, { resolve, reject });
        socket.send(JSON.stringify({ id, method, params }));
    });
}
function category(value) {
    try {
        const url = new URL(value);
        if (url.origin === "https://localhost:7141") return url.pathname.endsWith(".mjs") ? "localhost-module" : "localhost";
        if (["officeapis.public.onecdn.static.microsoft", "appsforoffice.microsoft.com"].includes(url.hostname))
            return /office_strings/i.test(url.pathname) ? "office-strings" : /\/office\.js$/i.test(url.pathname) ? "office-primary" : "office-dependency";
    } catch { /* Not a URL. */ }
    return "other";
}
async function route(params) {
    if (category(params.request.url) === "localhost-module") {
        const source = readFileSync(new URL("../../wwwroot/js/officeHost.mjs", import.meta.url), "utf8")
            .replace("return adapter;", "return DotNet.createJSObjectReference(adapter);");
        return send("Fetch.fulfillRequest", { requestId: params.requestId, responseCode: 200,
            responseHeaders: [{ name: "Content-Type", value: "text/javascript" }], body: Buffer.from(source).toString("base64") });
    }
    if (fixture === "missing" || category(params.request.url) !== "office-primary")
        return send("Fetch.failRequest", { requestId: params.requestId, errorReason: "BlockedByClient" });
    const script = `(() => {
        let resolved = false;
        globalThis.__fixtureCalls = 0;
        globalThis.Office = {
            onReady() {
                globalThis.__fixtureCalls++;
                return ${fixture === "pending" || fixture === "dependency" ? "new Promise(() => {})" : `new Promise(resolve => setTimeout(() => { resolved = true; resolve({host:"${fixture === "unsupported" ? "Word" : "Excel"}",platform:"OfficeOnline"}); }, 25))`};
            },
            get context() {
                if (!resolved) throw new Error("fixture.pre-ready-access");
                return { requirements: { isSetSupported: (name, version) => name === "ExcelApi" && version === "1.1" },
                    get document() { throw new Error("fixture.workbook-access"); } };
            }
        };
        ${fixture === "dependency" ? 'const script=document.createElement("script"); script.src="https://appsforoffice.microsoft.com/lib/1.1/hosted/en-us/office_strings.js"; document.head.append(script);' : ""}
    })();`;
    return send("Fetch.fulfillRequest", { requestId: params.requestId, responseCode: 200,
        responseHeaders: [{ name: "Content-Type", value: "application/javascript" }], body: Buffer.from(script).toString("base64") });
}
socket.addEventListener("message", event => {
    const message = JSON.parse(event.data), params = message.params;
    if (message.id) {
        const operation = pending.get(message.id);
        pending.delete(message.id);
        if (message.error) operation.reject(new Error("browser.command-failed")); else operation.resolve(message.result);
        return;
    }
    if (message.method === "Fetch.requestPaused") void route(params);
    if (message.method === "Network.requestWillBeSent") requests.set(params.requestId, category(params.request.url));
    if (message.method === "Network.responseReceived") {
        const kind = requests.get(params.requestId);
        if (kind && kind !== "other" && (kind !== "localhost" || params.response.status >= 400))
            summary.resources.push({ category: kind, status: params.response.status, mime: params.response.mimeType,
                ...(kind === "localhost" && new URL(params.response.url).pathname.startsWith('/_framework/') ? {asset:new URL(params.response.url).pathname.split('/').at(-1)} : {}) });
    }
    if (message.method === "Network.loadingFailed") {
        const kind = requests.get(params.requestId);
        if (kind && kind !== "other") summary.resources.push({ category: kind, failed: true,
            blocked: params.blockedReason || "", cancelled: !!params.canceled });
    }
    if (message.method === "Runtime.exceptionThrown") summary.errors.push({
        category: category(params.exceptionDetails.url),
        code: params.exceptionDetails.text.includes("hostInfo validation failed") ? "office.hostinfo-validation-failed" : "script.exception"
    });
});
async function evaluate(expression) {
    const result = await send("Runtime.evaluate", { expression, awaitPromise: true, returnByValue: true });
    assert.equal(result.exceptionDetails, undefined, "browser.evaluation-failed");
    return result.result.value;
}
async function waitForStatus() {
    const deadline = Date.now() + 30000;
    while (Date.now() < deadline) {
        const status = await evaluate("document.querySelector('code')?.textContent");
        if (status?.startsWith("office.") && await evaluate("document.querySelector('button')?.disabled === false")) return status;
        await new Promise(resolve => setTimeout(resolve, 100));
    }
    throw new Error("browser.startup-timeout");
}
try {
    await send("Network.enable");
    await send("Network.setCacheDisabled", { cacheDisabled: true });
    await send("Runtime.enable");
    await send("Page.enable");
    if (fixture !== "live") await send("Fetch.enable", { patterns: [
        { urlPattern: "https://officeapis.public.onecdn.static.microsoft/*" },
        { urlPattern: "https://appsforoffice.microsoft.com/*" },
        ...(fixture === "bridge-regression" ? [{urlPattern:"https://localhost:7141/js/officeHost.mjs*"}] : [])
    ] });
    await send("Page.navigate", { url: "https://localhost:7141/?beshOfficeDiagnostics=1" });
    summary.status = await waitForStatus();
    summary.page = await evaluate(`(async () => ({
        moduleImport: await import('./js/officeHost.mjs').then(() => true, () => false),
        corePassed: document.querySelector('#core-probe')?.textContent.includes('CDF(0) = 0.5'),
        commonPassed: document.querySelector('#common-probe')?.textContent.includes('2 descriptors, 4 registered handlers'),
        blazorError: getComputedStyle(document.querySelector('#blazor-error-ui')).display !== 'none',
        referenceFactory: typeof DotNet.createJSObjectReference,
        scriptInHead: document.querySelector('#office-js')?.parentElement === document.head,
        scriptAsync: document.querySelector('#office-js')?.async,
        diagnostic: globalThis.__beshOfficeDebug.snapshot()
    }))()`);
    assert.equal(summary.page.moduleImport, true);
    assert.equal(summary.page.corePassed, true);
    assert.equal(summary.page.commonPassed, true);
    assert.equal(summary.page.blazorError, false);
    assert.equal(summary.page.scriptInHead, true);
    assert.equal(summary.page.scriptAsync, false);
    assert.equal(summary.page.diagnostic.frame, "localhost-standalone");
    assert.equal(summary.status, fixture === "excel" ? "office.excel-ready" : fixture === "unsupported" ? "office.unsupported-host"
        : fixture === "bridge-regression" ? "office.initialization-failed" : "office.unavailable");
    assert.equal(summary.errors.filter(error => error.category.startsWith("localhost")).length, 0);
    for (let i = 0; i < 3; i++) {
        await evaluate("(async () => { document.querySelector('button').click(); await new Promise(resolve => setTimeout(resolve, 50)); })()");
        assert.equal(await waitForStatus(), summary.status);
    }
    if (["excel", "pending", "unsupported", "dependency"].includes(fixture))
        assert.equal(await evaluate("globalThis.__fixtureCalls"), 1, "Retry must not add Office waiters");
    if (fixture === "dependency") assert.ok(summary.page.diagnostic.events.some(event => event.code === "bootstrap.office-dependency-failed-or-blocked"));
    if (fixture === "bridge-regression") {
        assert.ok(summary.page.diagnostic.events.some(event => event.code === "bootstrap.interop-invoke-failed"));
        assert.equal(summary.page.diagnostic.events.some(event => event.code === "bootstrap.onready-started"), false);
    }
    summary.retryPassed = true;
    assert.equal(await evaluate(`__beshOfficeDebug.snapshot().events.filter(e => e.code === '${fixture === "bridge-regression" ? "bootstrap.interop-invoke-failed" : "bootstrap.interop-invoked"}').length`), 4);
    console.log(JSON.stringify(summary, null, 2));
} catch (error) {
    summary.failure = "browser.check-failed";
    summary.assertion = error.code === "ERR_ASSERTION" ? { actual: error.actual, expected: error.expected } : { code: "browser.startup-or-evaluation-failed" };
    summary.diagnostic = await evaluate("globalThis.__beshOfficeDebug?.snapshot()");
    console.log(JSON.stringify(summary, null, 2));
    process.exitCode = 1;
} finally {
    await send("Page.close");
    socket.close();
}
