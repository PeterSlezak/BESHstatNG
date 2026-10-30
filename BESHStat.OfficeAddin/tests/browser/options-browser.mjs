// Real Blazor/browser controls, simulated Office boundary. No authenticated profile or live workbook.
import assert from "node:assert/strict";
import { readFileSync, mkdirSync, writeFileSync } from "node:fs";
const mode = process.argv.includes("--standalone") ? "standalone" : "excel";
const endpoint = "http://127.0.0.1:9227";
const target = await (await fetch(`${endpoint}/json/new?about:blank`, { method: "PUT" })).json();
const socket = new WebSocket(target.webSocketDebuggerUrl);
await new Promise(resolve => socket.addEventListener("open", resolve, { once: true }));
let serial = 0;
const pending = new Map();
const summary = { simulated: true, mode, applicationExceptions: 0, screenshots: [] };
const canonical = readFileSync(new URL("../../../BESHStat.Excel.Common.Tests/TestData/Acquisition/canonical-two-ranges.json", import.meta.url), "utf8");
const helper = readFileSync(new URL("../helpers/workbook.mjs", import.meta.url), "utf8")
    .replace(/^import .*;\r?\n/m, "")
    .replace(/^export const canonical = .*;\r?\n/m, `const canonical = ${canonical};\n`)
    .replace("export function fakeWorkbook", "function fakeWorkbook");
const fixture = mode === "excel" ? `${helper}
    const fake = fakeWorkbook(); globalThis.Office = fake.scope.Office; globalThis.Excel = fake.scope.Excel;
    globalThis.__simulatedWorkbook = fake.state;` : "globalThis.Office = undefined;";
function send(method, params = {}) {
    return new Promise((resolve, reject) => {
        const id = ++serial; pending.set(id, { resolve, reject }); socket.send(JSON.stringify({ id, method, params }));
    });
}
socket.addEventListener("message", event => {
    const message = JSON.parse(event.data), p = message.params;
    if (message.id) {
        const request = pending.get(message.id); pending.delete(message.id);
        if (message.error) request.reject(new Error("browser.command-failed")); else request.resolve(message.result);
    } else if (message.method === "Fetch.requestPaused") {
        void send("Fetch.fulfillRequest", { requestId: p.requestId, responseCode: 200,
            responseHeaders: [{ name: "Content-Type", value: "text/javascript" }], body: Buffer.from(fixture).toString("base64") });
    } else if (message.method === "Runtime.exceptionThrown" && p.exceptionDetails.url?.startsWith("https://localhost:7141/")) {
        summary.applicationExceptions++;
    }
});
async function evaluate(expression) {
    const result = await send("Runtime.evaluate", { expression, awaitPromise: true, returnByValue: true });
    assert.equal(result.exceptionDetails, undefined, "browser.evaluation-failed");
    return result.result.value;
}
async function wait(expression) {
    const deadline = Date.now() + 40000;
    while (Date.now() < deadline) {
        if (await evaluate(expression)) return;
        await new Promise(resolve => setTimeout(resolve, 100));
    }
    throw new Error("browser.wait-timeout");
}
async function key(key, code = key) {
    const windowsVirtualKeyCode = { Tab: 9, Enter: 13, Escape: 27, Space: 32, Home: 36, ArrowDown: 40 }[code];
    await send("Input.dispatchKeyEvent", { type: "rawKeyDown", key, code, windowsVirtualKeyCode });
    if (code === "Enter" || code === "Space") await send("Input.dispatchKeyEvent", { type: "char", text: code === "Enter" ? "\r" : " ", key, code, windowsVirtualKeyCode });
    await send("Input.dispatchKeyEvent", { type: "keyUp", key, code, windowsVirtualKeyCode });
}
async function alpha(text) {
    await evaluate(`{const input=document.querySelector('#analysis-alpha');input.value=${JSON.stringify(text)};input.dispatchEvent(new Event('input',{bubbles:true}));}`);
    await wait(`document.querySelector('#analysis-alpha')?.value===${JSON.stringify(text)}`);
}
async function change(id, value) {
    await evaluate(`{const select=document.getElementById(${JSON.stringify(id)});select.value=${JSON.stringify(value)};select.dispatchEvent(new Event('change',{bubbles:true}));}`);
}
async function group(category) {
    await change("analysis-group", category);
    await wait(`document.querySelector('#analysis-group').value===${JSON.stringify(category)} && document.querySelector('#analysis-method').value===''`);
}
async function screenshot(name) {
    const metrics = await send("Page.getLayoutMetrics");
    const screenshot = await send("Page.captureScreenshot", { captureBeyondViewport: true,
        clip: { x: 0, y: 0, width: 360, height: metrics.cssContentSize.height, scale: 1 } });
    const folder = new URL("../../../TestResults/04-04/format-input/screenshots-options/", import.meta.url);
    mkdirSync(folder, { recursive: true });
    writeFileSync(new URL(`${mode}-${name}.png`, folder), Buffer.from(screenshot.data, "base64"));
    summary.screenshots.push(`${mode}-${name}.png`);
}
try {
    summary.stage = "startup";
    await send("Runtime.enable"); await send("Page.enable"); await send("Network.enable");
    await send("Network.setCacheDisabled", { cacheDisabled: true });
    await send("Emulation.setDeviceMetricsOverride", { width: 360, height: 1000, deviceScaleFactor: 1, mobile: false });
    await send("Fetch.enable", { patterns: [{ urlPattern: "https://officeapis.public.onecdn.static.microsoft/1/office.js" }] });
    await send("Page.navigate", { url: "https://localhost:7141/?beshOfficeDiagnostics=1" });
    const expected = mode === "excel" ? "office.excel-ready" : "office.unavailable";
    await wait(`document.querySelector('#host-code')?.textContent==='${expected}' && !document.querySelector('#retry-initialization')?.disabled`);
    assert.equal(await evaluate("document.querySelector('#core-probe').textContent.includes('CDF(0) = 0.5')"), true);
    assert.equal(await evaluate("document.querySelector('#common-probe').textContent.includes('2 descriptors, 4 registered handlers')"), true);
    assert.equal(await evaluate("document.querySelectorAll('[data-analysis-id]').length"), 0);
    assert.equal(await evaluate("document.querySelector('#analysis-group').options.length"), 13);
    assert.equal(await evaluate("document.querySelector('#analysis-method').disabled"), true);
    assert.equal(await evaluate("!!document.querySelector('label[for=analysis-group]') && !!document.querySelector('label[for=analysis-method]')"), true);
    // Native Tab and arrow keys operate the dependent selects; method identity remains its stable ID.
    summary.stage = "keyboard-anova";
    await evaluate("document.querySelector('#header-help').focus()");
    await key("Tab");
    assert.equal(await evaluate("document.activeElement.id"), "analysis-tab");
    await key("Tab");
    assert.equal(await evaluate("document.activeElement.id"), "analysis-panel");
    await key("Tab");
    assert.equal(await evaluate("document.activeElement.id"), "analysis-group");
    assert.equal(await evaluate("getComputedStyle(document.activeElement).outlineStyle"), "solid");
    await key("Home");
    const parametricIndex = await evaluate("[...document.querySelector('#analysis-group').options].findIndex(o=>o.value==='Parametric')");
    for (let i = 0; i < parametricIndex; i++) await key("ArrowDown");
    await key("Enter");
    await key("Escape"); // Close the native popup before moving focus with Tab.
    await wait("document.querySelector('#analysis-group').value==='Parametric' && !document.querySelector('#analysis-method').disabled");
    await key("Tab");
    assert.equal(await evaluate("document.activeElement.id"), "analysis-method");
    await key("ArrowDown"); // Disabled documentation-only methods must be skipped.
    await key("Enter");
    await key("Escape");
    await wait("document.querySelector('#option-schema')?.textContent==='anova.oneway.options.v1'");
    assert.equal(await evaluate("document.querySelectorAll('.boolean-option').length"), 5);
    assert.equal(await evaluate("[...document.querySelectorAll('.boolean-option input')].every(i=>!i.checked && document.querySelector(`label[for='${i.id}']`))"), true);
    await alpha("0");
    summary.stage = "alpha-validation";
    await wait("document.querySelector('#analysis-alpha').getAttribute('aria-invalid')==='true' && !!document.querySelector('#alpha-error')");
    assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"), "0");
    await alpha("0.125");
    await wait("document.querySelector('#analysis-alpha').getAttribute('aria-invalid')==='false'");
    await evaluate("document.querySelector('#anova-welch').click();document.querySelector('#anova-tukey').click()");
    await screenshot("anova");
    summary.stage = "dependent-kw";
    await group("Nonparametric");
    assert.equal(await evaluate("document.querySelector('#analysis-alpha')===null && document.querySelector('#alpha-error')===null"), true);
    await change("analysis-method", "nonparametric.kruskal-wallis");
    await wait("document.querySelector('#option-schema')?.textContent==='nonparametric.kruskal-wallis.options.v1'");
    assert.equal(await evaluate("document.querySelectorAll('.boolean-option').length"), 1);
    assert.equal(await evaluate("document.querySelector('#kw-dunn').checked"), true);
    assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"), "0.05");
    await alpha("0.025");
    await evaluate("document.querySelector('#kw-dunn').click()");
    // Native Space operates a focused checkbox.
    await evaluate("document.querySelector('#kw-dunn').focus()");
    await key(" ", "Space");
    await wait("document.querySelector('#kw-dunn').checked");
    await screenshot("kw");
    summary.stage = "retry-and-reset";
    await evaluate("document.querySelector('#retry-initialization').click()");
    await wait(`document.querySelector('#host-code')?.textContent==='${expected}' && !document.querySelector('#retry-initialization')?.disabled`);
    assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"), "0.025");
    assert.equal(await evaluate("document.querySelector('#kw-dunn').checked"), true);
    // Same-group/method events retain the draft rather than recreate it.
    await change("analysis-group", "Nonparametric");
    await change("analysis-method", "nonparametric.kruskal-wallis");
    assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"), "0.025");
    summary.stage = "documentation-only-groups";
    for (const category of ["Multivariate Analysis", "Graphics"]) {
        await group(category);
        assert.equal(await evaluate("document.querySelector('#analysis-alpha')===null"), true);
        assert.equal(await evaluate("document.querySelector('#analysis-navigation-status').textContent.includes('Not yet available in this Office add-in')"), true);
        assert.equal(await evaluate("[...document.querySelector('#analysis-method').options].slice(1).every(o=>o.disabled && o.textContent.includes('Not yet available'))"), true);
    }
    await screenshot("unavailable");
    await group("Parametric");
    await change("analysis-method", "anova.oneway");
    await wait("!!document.querySelector('#anova-welch')");
    assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"), "0.05");
    assert.equal(await evaluate("document.querySelector('#anova-welch').checked"), false);
    assert.equal(await evaluate("[...document.querySelectorAll('button')].find(b=>b.textContent==='Run analysis').disabled"), mode !== "excel");
    assert.equal(await evaluate("document.documentElement.scrollWidth <= innerWidth"), true, "narrow-pane overflow");
    if (mode === "excel") {
        summary.stage = "explicit-read";
        assert.equal(await evaluate("__simulatedWorkbook.runs"), 0, "option editing must not acquire workbook content");
        await evaluate("document.querySelector('#diagnostics-tab').click()");
        await evaluate("[...document.querySelectorAll('button')].find(b=>b.textContent==='Read selected range (read-only)').click()");
        await wait("document.querySelector('#range-status')?.textContent==='Status: workbook.ok'");
        assert.equal(await evaluate("__simulatedWorkbook.mutations"), 0);
    } else assert.equal(await evaluate("[...document.querySelectorAll('button')].some(b=>b.textContent==='Read selected range (read-only)')"), false);
    await send("Page.reload", { ignoreCache: true });
    summary.stage = "reopen";
    await wait(`document.querySelector('#host-code')?.textContent==='${expected}' && !document.querySelector('#retry-initialization')?.disabled`);
    assert.equal(await evaluate("document.querySelector('#analysis-alpha')===null"), true, "reload resets selection");
    assert.equal(await evaluate("document.querySelector('#analysis-group').value==='' && document.querySelector('#analysis-method').disabled"), true);
    assert.equal(summary.applicationExceptions, 0);
    summary.passed = true;
    console.log(JSON.stringify(summary, null, 2));
} catch (error) {
    summary.passed = false;
    summary.failure = error.code === "ERR_ASSERTION" ? { actual: error.actual, expected: error.expected, message: error.message } : "browser.startup-or-evaluation-failed";
    console.log(JSON.stringify(summary, null, 2));
    process.exitCode = 1;
} finally { await send("Page.close"); socket.close(); }
