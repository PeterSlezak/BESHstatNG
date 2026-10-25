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
    const folder = new URL("../../../TestResults/04-04/format-input/screenshots-refedit/", import.meta.url); mkdirSync(folder, { recursive: true });
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
    await wait("document.querySelector('#host-code')?.textContent==='office.excel-ready' && !document.querySelector('#retry-initialization').disabled");
    // Synthetic sheet contents only. The application still uses actual typed reader/parser/writer paths.
    await evaluate(`{
        const x=__pilotFake, g=${JSON.stringify(golden.groups)};
        x.sheets.get('sheet-1').name="Study O'Brien \u0394";
        for(let c=0;c<3;c++) { x.state.setCell(c+1,8,g.names[c]);g.values[c].forEach((v,r)=>x.state.setCell(c+1,r+9,v)); }
        x.state.setCell(8,5,'Group ID');x.state.setCell(8,10,'Value');
        let row=9;for(let c=0;c<3;c++) for(const v of g.values[c]) {x.state.setCell(row,5,g.names[c]);x.state.setCell(row++,10,v);}
        x.sheets.set('results-id',{id:'results-id',name:"Results O'Brien"});
    }`);
    await choose("Parametric", "anova.oneway");
    assert.equal(await evaluate("document.querySelectorAll('.range-reference input').length"),1);
    assert.equal(await evaluate("document.querySelector('#group-arrangement').compareDocumentPosition(document.querySelector('#input-reference')) & Node.DOCUMENT_POSITION_FOLLOWING"),4);
    assert.equal(await evaluate("!!document.querySelector('#analysis-panel #core-probe, #analysis-panel #host-status')"),false);
    summary.stage="input-capture";
    await click("Select range");
    await wait("!![...document.querySelectorAll('button')].find(b=>b.textContent==='Finish selection')");
    await evaluate("__pilotFake.state.emit('onSelectionChanged',{worksheetId:'sheet-1',address:'STALE'})");
    await wait(`document.querySelector('#input-reference').value==="'Study O''Brien \u0394'!A1:C4"`);
    await click("Finish selection");
    await wait("document.activeElement.id==='input-reference'");
    await send("Input.dispatchKeyEvent",{type:"rawKeyDown",key:"Tab",code:"Tab",windowsVirtualKeyCode:9});
    await send("Input.dispatchKeyEvent",{type:"keyUp",key:"Tab",code:"Tab",windowsVirtualKeyCode:9});
    assert.equal(await evaluate("document.activeElement.textContent"),"Select range");
    await send("Input.dispatchKeyEvent",{type:"rawKeyDown",key:"Enter",code:"Enter",windowsVirtualKeyCode:13});
    await send("Input.dispatchKeyEvent",{type:"char",text:"\r",key:"Enter",code:"Enter",windowsVirtualKeyCode:13});
    await send("Input.dispatchKeyEvent",{type:"keyUp",key:"Enter",code:"Enter",windowsVirtualKeyCode:13});
    await wait("!![...document.querySelectorAll('button')].find(b=>b.textContent==='Cancel selection' && !b.disabled)");
    await click("Cancel selection");await wait("document.activeElement.id==='input-reference'");
    assert.equal(await evaluate("__pilotFake.state.mutations"),0);
    await click("Run analysis"); await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='FStatistic').nextElementSibling.textContent"),"13");
    await screenshot("qualified-anova");
    summary.stage="different-sheet-output";
    await click("Pick output cell");
    await evaluate("__pilotFake.state.sheetId='results-id';__pilotFake.state.selected={row:12,col:4,rows:1,cols:1};__pilotFake.state.emit('onActivated');__pilotFake.state.emit('onSelectionChanged')");
    await wait(`document.querySelector('#output-start').value==="'Results O''Brien'!D12:D12"`);
    await click("Finish selection"); await wait("document.activeElement.id==='output-start'");
    assert.equal(await evaluate("__pilotFake.state.mutations"),0);
    await screenshot("qualified-output");
    await click("Write results"); await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate("__pilotFake.cells.has('results-id:12:4')"),true);
    assert.equal(await evaluate("__pilotFake.cells.get('sheet-1:1:1').value"),golden.groups.names[0]);
    summary.stage="rows-and-long";
    // Manual source references differ from the active worksheet/selection.
    await change("input-reference","'Study O''Brien \u0394'!$H$1:$K$3","input"); await change("group-arrangement","1");
    await click("Run analysis");await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='FStatistic').nextElementSibling.textContent"),"13");
    await change("group-arrangement","2");
    assert.equal(await evaluate("document.querySelectorAll('.range-reference input').length"),2);
    assert.equal(await evaluate("!!document.querySelector('#group-category, #group-value, #input-reference')"),false);
    await evaluate("__pilotFake.state.sheetId='sheet-1';__pilotFake.state.selected={row:8,col:5,rows:10,cols:1};[...document.querySelector('#category-reference').closest('.range-reference').querySelectorAll('button')].find(b=>b.textContent==='Capture selected range').click()");
    await wait("document.querySelector('#category-reference').value.includes('E8:E17')");
    assert.equal(await evaluate("document.querySelector('#values-reference').value"),"");
    await evaluate("__pilotFake.state.selected={row:8,col:10,rows:10,cols:1};[...document.querySelector('#values-reference').closest('.range-reference').querySelectorAll('button')].find(b=>b.textContent==='Capture selected range').click()");
    await wait("document.querySelector('#values-reference').value.includes('J8:J17')");
    assert.equal(await evaluate("document.querySelector('#category-reference').value.includes('E8:E17')"),true);
    await change("category-reference","'Study O''Brien \u0394'!$E$8:$E$17","input");
    await change("values-reference","'Study O''Brien \u0394'!$J$9:$J$18","input");
    const mismatchReads=await evaluate("__pilotFake.state.loads.length");
    await click("Run analysis");await wait("document.querySelector('#pilot-status code')?.textContent==='input.long-header-alignment'");
    assert.equal(await evaluate("__pilotFake.state.loads.length"),mismatchReads);
    assert.equal(await evaluate("!!document.querySelector('[aria-label=\"Input validation issues\"][role=alert]')"),true);
    await change("values-reference","'Study O''Brien \u0394'!$J$8:$J$17","input");
    assert.equal(await evaluate("!!document.querySelector('[aria-label=\"Input validation issues\"]')"),false);
    await click("Run analysis");await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='FStatistic').nextElementSibling.textContent"),"13");
    assert.equal(await evaluate("!!document.querySelector('[aria-label=\"Redacted input preview\"]')"),false);
    summary.stage="tabs-preserve-live-composite";
    const tabReads=await evaluate("__pilotFake.state.loads.length"), tabListeners=await evaluate("__pilotFake.state.listenerCount()");
    await evaluate("document.querySelector('#analysis-tab').focus()");
    await send("Input.dispatchKeyEvent",{type:"rawKeyDown",key:"ArrowLeft",code:"ArrowLeft",windowsVirtualKeyCode:37});
    await send("Input.dispatchKeyEvent",{type:"keyUp",key:"ArrowLeft",code:"ArrowLeft",windowsVirtualKeyCode:37});
    await wait("document.activeElement.id==='diagnostics-tab' && document.querySelector('#diagnostics-tab').getAttribute('aria-selected')==='true'");
    assert.equal(await evaluate("document.querySelector('#analysis-panel').hidden"),true);
    await click("Analysis");assert.equal(await evaluate("__pilotFake.state.loads.length"),tabReads);
    assert.equal(await evaluate("__pilotFake.state.listenerCount()"),tabListeners);
    assert.equal(await evaluate("document.querySelector('#values-reference').value.includes('J8')"),true);
    summary.stage="headers-and-optional-preview";
    await click("Validate input (optional)");await wait("document.querySelector('#pilot-status code')?.textContent==='workbook.ok'");
    assert.equal(await evaluate("!!document.querySelector('[aria-label=\"Redacted input preview\"]')"),true);
    await change("group-headers","1");
    await change("category-reference","'Study O''Brien \u0394'!E9:E17","input");await change("values-reference","'Study O''Brien \u0394'!J9:J17","input");
    await click("Run analysis");await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='FStatistic').nextElementSibling.textContent"),"13");
    await change("group-headers","2");
    await change("category-reference","'Study O''Brien \u0394'!E8:E17","input");await change("values-reference","'Study O''Brien \u0394'!J8:J17","input");
    await click("Run analysis");await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='FStatistic').nextElementSibling.textContent"),"13");
    await evaluate("__pilotFake.state.sheetId='sheet-1';__pilotFake.state.setCell(10,10,'#N/A','Error');__pilotFake.state.emit('onChanged')");
    await click("Run analysis");await wait("document.querySelector('#pilot-status code')?.textContent==='input.validation-failed'");
    assert.equal(await evaluate("document.querySelector('[aria-label=\"Redacted input preview\"]').textContent.includes('worksheet row/column 10/10')"),true);
    await evaluate(`__pilotFake.state.setCell(10,10,${JSON.stringify(golden.groups.values[0][1])});__pilotFake.state.emit('onChanged')`);
    await click("Run analysis");await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    assert.equal(await evaluate("!!document.querySelector('[aria-label=\"Redacted input preview\"]')"),false);
    await screenshot("long-anova");
    await choose("Nonparametric","nonparametric.kruskal-wallis");
    assert.equal(await evaluate("document.querySelector('#group-arrangement').value"),"0");
    assert.equal(await evaluate("document.querySelector('#group-headers').value"),"0");
    await change("group-arrangement","2");
    await change("category-reference","'Study O''Brien \u0394'!E8:E17","input");await change("values-reference","'Study O''Brien \u0394'!J8:J17","input");
    await click("Run analysis");await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    assert.equal(await evaluate("document.querySelector('#kw-dunn').checked"),true);
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='HStatistic').nextElementSibling.textContent"),"5.955555556");
    await screenshot("long-kw");
    summary.stage="new-sheet-custom-cell";
    await change("output-destination","1");await change("output-name","Custom \u0394","input");await change("output-start","E9","input");
    assert.equal(await evaluate("__pilotFake.state.added"),0);
    await click("Write results");await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate("__pilotFake.cells.has('new-1:9:5')"),true);
    assert.equal(await evaluate("document.querySelector('#output-destination option[value=\"2\"]').disabled"),true);
    summary.stage="manual-invalid-and-retry";
    await change("values-reference","'[Other.xlsx]Study'!A1","input");
    assert.equal(await evaluate("document.querySelector('#values-reference').getAttribute('aria-invalid')"),"true");
    await change("analysis-alpha","0.1","input");await click("Retry initialization");await wait("!document.querySelector('#retry-initialization').disabled");
    assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"),"0.1");
    assert.equal(await evaluate("document.querySelector('#group-arrangement').value"),"2");
    assert.equal(await evaluate("document.querySelector('#group-headers').value"),"0");
    assert.equal(await evaluate("document.documentElement.scrollWidth<=innerWidth"),true);
    await click("Reset form");
    assert.equal(await evaluate("document.querySelector('#analysis-method').value"),"nonparametric.kruskal-wallis");
    assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"),"0.05");
    assert.equal(await evaluate("document.querySelector('#input-reference').value"),"");
    assert.equal(summary.errors,0);summary.passed=true;console.log(JSON.stringify(summary,null,2));
} catch(error) {
    summary.passed=false; summary.failure=error.code==='ERR_ASSERTION'?{actual:error.actual,expected:error.expected}:String(error.message);
    summary.status=await evaluate("JSON.stringify({status:document.querySelector('#pilot-status')?.textContent,reference:document.querySelector('#input-reference')?.value,capture:document.querySelector('#input-reference-status')?.textContent,output:document.querySelector('#output-start')?.value,outputStatus:document.querySelector('#output-start-status')?.textContent})");
    console.log(JSON.stringify(summary,null,2));process.exitCode=1;
} finally {await send("Page.close");socket.close();}
