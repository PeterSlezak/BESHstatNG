// Actual Blazor/Core/Common and production writer. Office/Excel are SIMULATED, never authenticated.
import assert from "node:assert/strict";
import { readFileSync, writeFileSync, mkdirSync } from "node:fs";
const target = await (await fetch("http://127.0.0.1:9227/json/new?about:blank", { method: "PUT" })).json();
const socket = new WebSocket(target.webSocketDebuggerUrl);
await new Promise(resolve => socket.addEventListener("open", resolve, { once: true }));
let serial = 0;
const pending = new Map(), summary = { simulated: true, productionWriter: true, errors: 0, screenshots: [] };
const helper = readFileSync(new URL("../helpers/pilotWorkbook.mjs", import.meta.url), "utf8").replace("export function pilotWorkbook", "function pilotWorkbook");
const golden = JSON.parse(readFileSync(new URL("../../../BESHStat.Excel.Common.Tests/TestData/Stage304/anova-base.json", import.meta.url), "utf8"));
const fixture = `${helper}\n const fake=pilotWorkbook();const golden=${JSON.stringify(golden.groups)};
for(let c=0;c<3;c++){fake.state.setCell(1,c+1,golden.names[c]);golden.values[c].forEach((v,r)=>fake.state.setCell(r+2,c+1,v));}
fake.sheets.set('source-b',{id:'source-b',name:'Second source'});fake.state.sheetId='source-b';
for(let c=0;c<3;c++){fake.state.setCell(1,c+1,golden.names[c]);golden.values[c].forEach((v,r)=>fake.state.setCell(r+2,c+1,v*2));}
fake.state.sheetId='sheet-1';
globalThis.Office=fake.scope.Office;globalThis.Excel=fake.scope.Excel;globalThis.__pilotFake=fake;`;
function send(method, params = {}) { return new Promise((resolve, reject) => { const id = ++serial; pending.set(id, { resolve, reject }); socket.send(JSON.stringify({ id, method, params })); }); }
socket.addEventListener("message", event => {
    const m = JSON.parse(event.data), p = m.params;
    if (m.id) { const r = pending.get(m.id); pending.delete(m.id); if (m.error) r.reject(new Error("browser.command-failed")); else r.resolve(m.result); }
    else if (m.method === "Fetch.requestPaused") void send("Fetch.fulfillRequest", { requestId: p.requestId, responseCode: 200,
        responseHeaders: [{ name: "Content-Type", value: "text/javascript" }], body: Buffer.from(fixture).toString("base64") });
    else if (m.method === "Runtime.exceptionThrown" && p.exceptionDetails.url?.startsWith("https://localhost:7141")) summary.errors++;
});
async function evaluate(expression) { const r = await send("Runtime.evaluate", { expression, awaitPromise: true, returnByValue: true }); assert.equal(r.exceptionDetails, undefined); return r.result.value; }
async function wait(expression) { const until = Date.now() + 45000; while (Date.now() < until) { if (await evaluate(expression)) return; await new Promise(r => setTimeout(r, 100)); } throw new Error("browser.wait-timeout"); }
async function click(label) { await evaluate(`[...document.querySelectorAll('button')].find(b=>b.textContent===${JSON.stringify(label)}).click()`); }
async function change(id, value, event = "change") { await evaluate(`{const e=document.getElementById(${JSON.stringify(id)});e.value=${JSON.stringify(value)};e.dispatchEvent(new Event(${JSON.stringify(event)},{bubbles:true}));}`); }
async function choose(group, id) { await change("analysis-group", group); await change("analysis-method", id); await wait("!!document.querySelector('.analysis-pilot')"); }
async function screenshot(name) {
    const folder = new URL("../../../TestResults/04-04/format-input/screenshots/", import.meta.url); mkdirSync(folder, { recursive: true });
    const metrics = await send("Page.getLayoutMetrics"), shot = await send("Page.captureScreenshot", { captureBeyondViewport: true,
        clip: { x: 0, y: 0, width: 360, height: metrics.cssContentSize.height, scale: 1 } });
    writeFileSync(new URL(`${name}.png`, folder), Buffer.from(shot.data, "base64")); summary.screenshots.push(name);
}
try {
    await send("Runtime.enable"); await send("Page.enable"); await send("Network.enable");
    await send("Network.setCacheDisabled", { cacheDisabled: true });
    await send("Emulation.setDeviceMetricsOverride", { width: 360, height: 1000, deviceScaleFactor: 1, mobile: false });
    await send("Fetch.enable", { patterns: [{ urlPattern: "https://officeapis.public.onecdn.static.microsoft/1/office.js" }] });
    await send("Page.navigate", { url: "https://localhost:7141/?beshOfficeDiagnostics=1" });
    summary.stage = "startup";
    await wait("document.querySelector('#host-code')?.textContent==='office.excel-ready' && !document.querySelector('button').disabled");
    assert.equal(await evaluate("document.querySelector('#core-probe').textContent.includes('0.5') && document.querySelector('#common-probe').textContent.includes('2 descriptors, 4 registered handlers')"), true);
    await choose("Parametric", "anova.oneway");
    assert.equal(await evaluate("__pilotFake.state.runs"), 0);
    await change("analysis-alpha", "0", "input");
    assert.equal(await evaluate("document.querySelector('#analysis-alpha').getAttribute('aria-invalid')"), "true");
    await change("analysis-alpha", "0.05", "input");
    summary.stage = "acquire-run";
    await change("input-reference", "'Input'!A1:C4", "input");
    assert.equal(await evaluate("__pilotFake.state.mutations"), 0);
    await click("Run analysis"); await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='FStatistic').nextElementSibling.textContent"), "13");
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='PValue').nextElementSibling.textContent"), "0.006591796875");
    assert.equal(await evaluate("!!document.querySelector('[aria-label=\"Redacted input preview\"]')"), false);
    await screenshot("anova-preview");
    summary.stage = "new-sheet-anova-then-independent-source";
    await change("output-destination", "1"); await change("output-name", "First ANOVA", "input");
    await click("Write results"); await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    summary.firstOutcome = await evaluate("[...document.querySelectorAll('p')].find(p=>p.textContent.startsWith('Last write outcome:')).textContent");
    assert.equal(summary.firstOutcome.includes("Completed"), true);
    assert.equal(summary.firstOutcome.includes("Mutation may have occurred: True"), true);
    assert.equal(await evaluate("__pilotFake.state.listenerCount()"), 0);
    await evaluate("globalThis.__firstResult=JSON.stringify([...__pilotFake.cells].filter(([k])=>k.startsWith('new-1:')));__pilotFake.state.sheetId='source-b';__pilotFake.state.emit('onActivated')");
    await click("Select range"); await wait("!![...document.querySelectorAll('button')].find(b=>b.textContent==='Finish selection')");
    await click("Finish selection"); await wait("document.querySelector('#input-reference').value.includes('Second source')");
    await click("Run analysis"); await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    await change("output-destination", "0"); await change("output-start", "'Second source'!J1", "input");
    await click("Write results"); await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate("JSON.stringify([...__pilotFake.cells].filter(([k])=>k.startsWith('new-1:')))===__firstResult"), true);
    assert.equal(await evaluate("__pilotFake.cells.has('source-b:1:10') && __pilotFake.state.listenerCount()===0"), true);
    // The same pane now captures source A again after existing-sheet completion.
    await evaluate("__pilotFake.state.sheetId='sheet-1';__pilotFake.state.emit('onSelectionChanged')");
    await click("Select range"); await wait("!![...document.querySelectorAll('button')].find(b=>b.textContent==='Finish selection')");
    await click("Finish selection"); await wait("document.querySelector('#input-reference').value.includes('Input')");
    await click("Run analysis"); await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    await change("output-start", "'Input'!J1", "input");
    summary.stage = "occupied-no";
    // Simulated foreign edit without event permits an occupied fixture for explicit inspection.
    await evaluate("__pilotFake.state.setCell(1,10,'occupied')");
    await click("Write results"); await wait("document.querySelector('#pilot-status code')?.textContent==='output.occupancy-occupied'");
    await screenshot("overwrite-consent"); await click("No, keep existing output");
    await wait("document.querySelector('#pilot-status code')?.textContent==='output.cancelled'");
    assert.equal(await evaluate("__pilotFake.cells.get('sheet-1:1:10').value==='occupied'"), true);
    summary.stage = "occupied-yes";
    // No keeps verified source/results; a fresh explicit Write click obtains a new inspection/consent without another Run.
    await click("Write results"); await wait("document.querySelector('#pilot-status code')?.textContent==='output.occupancy-occupied'");
    await click("Yes, overwrite inspected output"); await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate("__pilotFake.state.mutations>0"), true);
    summary.stage = "kw-new-sheet";
    await choose("Nonparametric", "nonparametric.kruskal-wallis");
    assert.equal(await evaluate("document.querySelector('#kw-dunn').checked"), true);
    await change("input-reference", "'Input'!A1:C4", "input");
    await click("Run analysis"); await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    await screenshot("kw-preview"); await change("output-destination", "1");
    await change("output-name", "Pilot results", "input");
    summary.stage = "kw-inspection";
    assert.equal(await evaluate("__pilotFake.state.added"), 1);
    summary.stage = "kw-writing";
    await click("Write results"); await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate("document.querySelector('#pilot-status code').textContent"), "output.completed");
    assert.equal(await evaluate("__pilotFake.state.added"), 2);
    summary.stage = "events-and-retry";
    await click("Run analysis"); await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    await evaluate("__pilotFake.state.emit('onSelectionChanged')");
    await wait("document.querySelector('#pilot-status code')?.textContent==='input.preview-stale'");
    assert.equal(await evaluate("[...document.querySelectorAll('button')].find(b=>b.textContent==='Run analysis').disabled"), false);
    await change("analysis-alpha", "0.1", "input"); await click("Retry initialization");
    await wait("!document.querySelector('button').disabled");
    assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"), "0.1");
    assert.equal(await evaluate("document.documentElement.scrollWidth<=innerWidth"), true);
    await click("Diagnostics / Log");
    await click("Read selected range (read-only)"); await wait("document.querySelector('#range-status')?.textContent==='Status: workbook.ok'");
    await click("Analysis");
    if (process.argv.includes("--uncertain")) {
        summary.stage = "uncertain-outcome-and-capture-recovery";
        await evaluate("import('/js/productionWriter.mjs').then(m=>m.enableWriterTrace())");
        await change("input-reference", "'Input'!A1:C4", "input");
        await click("Run analysis"); await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
        await change("output-destination", "1"); await change("output-name", "Uncertain result", "input");
        await evaluate("__pilotFake.state.rejectCommit=true");
        await click("Write results"); await wait("document.querySelector('#pilot-status code')?.textContent==='output.mutation-uncertain'");
        summary.uncertainOutcome = await evaluate("[...document.querySelectorAll('p')].find(p=>p.textContent.startsWith('Last write outcome:')).textContent");
        assert.equal(summary.uncertainOutcome.includes("PartiallyWritten"), true);
        assert.equal(summary.uncertainOutcome.includes("Confirmed artifacts: 0"), true);
        const mutations = await evaluate("__pilotFake.state.mutations");
        await change("input-reference", "'Input'!A1:C4", "input"); await click("Select range");
        await wait("document.querySelector('#input-reference-status code')?.textContent==='output.mutation-uncertain'");
        assert.equal(await evaluate("document.querySelector('#input-reference-status').textContent.includes('Close and reopen')"), true);
        await click("Retry initialization"); await wait("!document.querySelector('button').disabled");
        await change("input-reference", "'Input'!A1:C4", "input");
        assert.equal(await evaluate("document.body.textContent.includes('PartiallyWritten') && document.body.textContent.includes('wait for the previous native operation to finish')"), true);
        assert.equal(await evaluate("__pilotFake.state.mutations"), mutations);
        await click("Reset form");
        await choose("Parametric", "anova.oneway");
        assert.equal(await evaluate("[...document.querySelectorAll('button')].find(b=>b.textContent==='Run analysis').disabled"), true);
        assert.equal(await evaluate("document.body.textContent.includes('PartiallyWritten') && document.body.textContent.includes('Close and reopen')"), true);
        summary.trace = await evaluate("import('/js/productionWriter.mjs').then(m=>m.getWriterTrace())");
    }
    assert.equal(summary.errors, 0); summary.passed = true;
    console.log(JSON.stringify(summary, null, 2));
} catch (error) {
    summary.passed = false; summary.failure = error.code === "ERR_ASSERTION" ? { actual: error.actual, expected: error.expected } : "browser.wait-or-evaluation-failed";
    summary.status = await evaluate("document.querySelector('#pilot-status')?.textContent");
    console.log(JSON.stringify(summary, null, 2)); process.exitCode = 1;
} finally { await send("Page.close"); socket.close(); }
