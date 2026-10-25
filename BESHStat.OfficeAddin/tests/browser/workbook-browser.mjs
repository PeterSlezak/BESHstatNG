// Opt-in real browser + Blazor interop, SIMULATED Excel only. Fresh isolated tab.
// Requires owner-approved Edge CDP on 127.0.0.1:9227 and local Debug host on 7141.
// No packages/authentication/existing tabs/workbooks. No workbook data is logged.
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
const endpoint="http://127.0.0.1:9227";
const pendingSync=process.argv.includes("--pending-sync");
const target=await(await fetch(`${endpoint}/json/new?about:blank`,{method:"PUT"})).json();
const socket=new WebSocket(target.webSocketDebuggerUrl);
await new Promise(resolve=>socket.addEventListener("open",resolve,{once:true}));
let serial=0;
const pending=new Map();
const summary={simulated:true,fixture:pendingSync?"pending-payload-sync":"canonical-workbook",moduleResponses:[],applicationExceptions:0};
function send(method,params={}) {
    return new Promise((resolve,reject)=>{
        const id=++serial;pending.set(id,{resolve,reject});socket.send(JSON.stringify({id,method,params}));
    });
}
const canonical=readFileSync(new URL("../../../BESHStat.Excel.Common.Tests/TestData/Acquisition/canonical-two-ranges.json",import.meta.url),"utf8");
const helper=readFileSync(new URL("../helpers/workbook.mjs",import.meta.url),"utf8")
    .replace(/^import .*;\r?\n/m,"")
    .replace(/^export const canonical = .*;\r?\n/m,`const canonical = ${canonical};\n`)
    .replace("export function fakeWorkbook", "function fakeWorkbook");
const fixture=`${helper}\nconst fake=fakeWorkbook(); globalThis.Office=fake.scope.Office; globalThis.Excel=fake.scope.Excel; globalThis.__simulatedWorkbook=fake.state;`;
socket.addEventListener("message",event=>{
    const message=JSON.parse(event.data),p=message.params;
    if(message.id) {
        const request=pending.get(message.id);pending.delete(message.id);
        if(message.error)request.reject(new Error("browser.command-failed"));else request.resolve(message.result);
    } else if(message.method==="Fetch.requestPaused") {
        void send("Fetch.fulfillRequest",{requestId:p.requestId,responseCode:200,
            responseHeaders:[{name:"Content-Type",value:"text/javascript"}],body:Buffer.from(fixture).toString("base64")});
    } else if(message.method==="Network.responseReceived") {
        const url=new URL(p.response.url);
        if(url.origin==="https://localhost:7141" && /\/(officeHost|workbookAdapter)\.mjs$/.test(url.pathname))
            summary.moduleResponses.push({module:url.pathname.endsWith("workbookAdapter.mjs")?"workbook":"readiness",status:p.response.status,mime:p.response.mimeType});
    } else if(message.method==="Runtime.exceptionThrown") {
        // Categorize using URL locally; never retain raw exception/stack/URL.
        if(p.exceptionDetails.url?.startsWith("https://localhost:7141/"))summary.applicationExceptions++;
    }
});
async function evaluate(expression) {
    const result=await send("Runtime.evaluate",{expression,awaitPromise:true,returnByValue:true});
    assert.equal(result.exceptionDetails,undefined,"browser.evaluation-failed");
    return result.result.value;
}
async function wait(expression) {
    const deadline=Date.now()+40000;
    while(Date.now()<deadline) {
        if(await evaluate(expression))return;
        await new Promise(resolve=>setTimeout(resolve,100));
    }
    throw new Error("browser.wait-timeout");
}
async function click(label) {
    await evaluate(`Array.from(document.querySelectorAll('button')).find(b=>b.textContent===${JSON.stringify(label)}).click()`);
}
try {
    await send("Runtime.enable");await send("Page.enable");await send("Network.enable");
    await send("Network.setCacheDisabled",{cacheDisabled:true});
    await send("Fetch.enable",{patterns:[{urlPattern:"https://officeapis.public.onecdn.static.microsoft/1/office.js"}]});
    await send("Page.navigate",{url:"https://localhost:7141/?beshOfficeDiagnostics=1"});
    await wait("document.querySelector('#host-code')?.textContent==='office.excel-ready' && !document.querySelector('button')?.disabled");
    assert.equal(await evaluate("document.querySelector('#core-probe')?.textContent.includes('CDF(0) = 0.5')"),true);
    assert.equal(await evaluate("document.querySelector('#common-probe')?.textContent.includes('2 descriptors, 4 registered handlers')"),true);
    assert.equal(await evaluate("getComputedStyle(document.querySelector('#blazor-error-ui')).display==='none'"),true);
    assert.equal(await evaluate("__simulatedWorkbook.runs"),0,"no automatic read");
    await click("Diagnostics / Log");
    await click("Inspect selection metadata only (read-only)");
    await wait("document.querySelector('#range-status')?.textContent==='Status: workbook.ok'");
    assert.equal(await evaluate("__simulatedWorkbook.payloads.length"),0);
    if(pendingSync) await evaluate("{ const blocked=__simulatedWorkbook.syncs+3; __simulatedWorkbook.hook=n=>n===blocked?new Promise(()=>{}):Promise.resolve(); }");
    await click("Read selected range (read-only)");
    await wait(pendingSync?"document.querySelector('#range-status')?.textContent==='Status: input.time-budget'":
        "document.querySelector('#range-status')?.textContent==='Status: workbook.ok' && __simulatedWorkbook.payloads.length===1");
    assert.equal(await evaluate("__simulatedWorkbook.mutations"),0);
    await click("Refresh stage summary");
    const stages=await evaluate("document.querySelector('#development-stage-summary').textContent");
    assert.ok(pendingSync ? /cs\.(timeout|js-timeout-received)/.test(stages) : stages.includes("cs.read-completed"));
    if(pendingSync) summary.timeoutOrigin=stages.includes("cs.js-timeout-received")?"javascript":"csharp";
    summary.fullRead=pendingSync?"input.time-budget":"workbook.ok";
    summary.metadataOnly="workbook.ok";
    summary.stageCodes=stages.split("\n").filter(Boolean).map(s=>s.split(" ")[0]);
    summary.javascriptStages=await evaluate("__beshOfficeDebug.snapshot().events.filter(e=>e.code.startsWith('read.'))");
    assert.ok(summary.javascriptStages.some(e=>e.code===(pendingSync?"read.payload-sync-started":"read.payload-sync-completed")));
    if(pendingSync) {
        assert.ok(!summary.javascriptStages.some(e=>e.code==="read.payload-sync-completed"));
        await evaluate("__simulatedWorkbook.hook=async()=>{}");
        await click("Read selected range (read-only)");
        await wait("document.querySelector('#range-status')?.textContent==='Status: workbook.ok'");
        summary.retryAfterTimeout="workbook.ok";
    }
    assert.ok(summary.moduleResponses.some(r=>r.module==="workbook"&&r.status===200&&r.mime.includes("javascript")));
    assert.equal(summary.applicationExceptions,0);
    await click("Retry initialization");
    await wait("!document.querySelector('button')?.disabled && !document.querySelector('#range-status')");
    assert.equal(await evaluate("__simulatedWorkbook.payloads.length"),1,"Retry must not read");
    console.log(JSON.stringify(summary,null,2));
} catch {
    summary.status="browser.check-failed";
    summary.javascriptStages=await evaluate("__beshOfficeDebug?.snapshot().events.filter(e=>e.code.startsWith('read.'))");
    summary.stageCodes=await evaluate("document.querySelector('#development-stage-summary')?.textContent.split('\\n').map(s=>s.split(' ')[0]).filter(s=>/^cs\\.[a-z-]+$/.test(s))");
    console.log(JSON.stringify(summary,null,2));process.exitCode=1;
} finally { await send("Page.close");socket.close(); }
