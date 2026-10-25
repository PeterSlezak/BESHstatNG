// Real Blazor/Common/Core with simulated Office. No authenticated Excel acceptance.
import assert from 'node:assert/strict';
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
const target=await(await fetch('http://127.0.0.1:9227/json/new?about:blank',{method:'PUT'})).json();
const socket=new WebSocket(target.webSocketDebuggerUrl);await new Promise(r=>socket.addEventListener('open',r,{once:true}));
let serial=0;const pending=new Map(),summary={simulated:true,errors:0,stages:[],screenshots:[]};
const helper=readFileSync(new URL('../helpers/pilotWorkbook.mjs',import.meta.url),'utf8').replace('export function pilotWorkbook','function pilotWorkbook');
const golden=JSON.parse(readFileSync(new URL('../../../BESHStat.Excel.Common.Tests/TestData/Stage304/anova-base.json',import.meta.url),'utf8'));
const fixture=`${helper}\nconst fake=pilotWorkbook(),g=${JSON.stringify(golden.groups)};
for(let c=0;c<3;c++){fake.state.setCell(1,c+1,g.names[c]);g.values[c].forEach((v,r)=>fake.state.setCell(r+2,c+1,v));}
fake.state.setCell(1,6,'Group');fake.state.setCell(1,5,'Value');let row=2;
for(let c=0;c<3;c++)for(const value of g.values[c]){fake.state.setCell(row,6,g.names[c]);fake.state.setCell(row++,5,value);}
for(const name of ['ANOVAresults','ANOVAresults1','ANOVAresults3'])fake.sheets.set(name,{id:name,name});
globalThis.Office=fake.scope.Office;globalThis.Excel=fake.scope.Excel;globalThis.__pilotFake=fake;`;
function send(method,params={}){return new Promise((resolve,reject)=>{const id=++serial;pending.set(id,{resolve,reject});socket.send(JSON.stringify({id,method,params}));});}
async function evaluate(expression){const r=await send('Runtime.evaluate',{expression,awaitPromise:true,returnByValue:true});assert.equal(r.exceptionDetails,undefined);return r.result.value;}
async function wait(expression){const until=Date.now()+45000;while(Date.now()<until){if(await evaluate(expression))return;await new Promise(r=>setTimeout(r,100));}throw Error('browser.timeout');}
async function click(text){await evaluate(`[...document.querySelectorAll('button')].find(b=>b.textContent===${JSON.stringify(text)}).click()`);}
async function change(id,value,event='change'){await evaluate(`{const e=document.getElementById(${JSON.stringify(id)});e.value=${JSON.stringify(value)};e.dispatchEvent(new Event(${JSON.stringify(event)},{bubbles:true}));}`);}
async function run(){await click('Run analysis');await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");}
async function screenshot(name){const folder=new URL('../../../TestResults/04-04/corrective/screenshots/',import.meta.url);mkdirSync(folder,{recursive:true});const shot=await send('Page.captureScreenshot');writeFileSync(new URL(`${name}.png`,folder),Buffer.from(shot.data,'base64'));summary.screenshots.push(name);}
socket.addEventListener('message',async event=>{const m=JSON.parse(event.data);if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error('cdp.failed')):p.resolve(m.result);}
    else if(m.method==='Runtime.exceptionThrown')summary.errors++;
    else if(m.method==='Fetch.requestPaused')await send('Fetch.fulfillRequest',{requestId:m.params.requestId,responseCode:200,responseHeaders:[{name:'Content-Type',value:'text/javascript'}],body:Buffer.from(fixture).toString('base64')});});
try{
    await send('Runtime.enable');await send('Page.enable');await send('Network.enable');await send('Network.setCacheDisabled',{cacheDisabled:true});
    await send('Emulation.setDeviceMetricsOverride',{width:360,height:800,deviceScaleFactor:1,mobile:false});
    await send('Fetch.enable',{patterns:[{urlPattern:'https://officeapis.public.onecdn.static.microsoft/1/office.js'}]});
    await send('Page.navigate',{url:'https://localhost:7141/?beshOfficeDiagnostics=1'});await wait("document.querySelector('#host-code')?.textContent==='office.excel-ready'");
    await change('analysis-group','Parametric');await change('analysis-method','anova.oneway');
    await wait("!!document.querySelector('#input-reference')");await change('input-reference',"'Input'!A1:C4",'input');
    await run();await change('output-start',"'Input'!J1",'input');
    await click('Write results');await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    const oldTables=await evaluate("document.querySelectorAll('.analysis-pilot li').length");
    const refs=await evaluate("[document.querySelector('#input-reference').value,document.querySelector('#output-start').value]");
    await evaluate("{for(const id of ['anova-welch','anova-fisher']){const e=document.getElementById(id);e.checked=true;e.dispatchEvent(new Event('change',{bubbles:true}));}}");
    await wait("document.querySelector('#pilot-status code')?.textContent==='input.preview-stale'");
    await run();assert.deepEqual(await evaluate("[document.querySelector('#input-reference').value,document.querySelector('#output-start').value]"),refs);
    assert.ok(await evaluate("document.querySelectorAll('.analysis-pilot li').length")>oldTables);
    await evaluate("globalThis.__oldCells=JSON.stringify([...__pilotFake.cells]);globalThis.__oldMutations=__pilotFake.state.mutations;");
    await click('Write results');await wait("document.querySelector('#pilot-status code')?.textContent==='output.occupancy-occupied'");
    await click('No, keep existing output');await wait("document.querySelector('#pilot-status code')?.textContent==='output.cancelled'");
    assert.equal(await evaluate("JSON.stringify([...__pilotFake.cells])===__oldCells && __pilotFake.state.mutations===__oldMutations"),true);
    await click('Write results');await wait("document.querySelector('#pilot-status code')?.textContent==='output.occupancy-occupied'");
    await click('Yes, overwrite inspected output');await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate("__pilotFake.state.mutations>__oldMutations"),true);summary.stages.push('completed-option-rerun-fresh-No-Yes');
    // A new logical run reuses exactly the existing qualified draft.
    await run();await click('Diagnostics / Log');await click('Analysis');
    const other=await(await fetch('http://127.0.0.1:9227/json/new?about:blank',{method:'PUT'})).json();
    await send('Page.bringToFront');await fetch(`http://127.0.0.1:9227/json/close/${other.id}`);
    await evaluate("__pilotFake.state.emit('workbook-activated')");await wait("document.querySelector('#pilot-status code')?.textContent==='input.preview-stale'");
    await run();await evaluate("__pilotFake.state.setCell(2,1,12);__pilotFake.state.emit('onChanged')");
    await wait("document.querySelector('#pilot-status code')?.textContent==='input.preview-stale'");await run();
    summary.stages.push('tab-focus-and-real-source-edit-reacquisition');
    await change('group-arrangement','2');
    await wait("!!document.querySelector('#category-reference')");
    await change('category-reference',"'Input'!F:F",'input');await change('values-reference',"'Input'!E1:E11",'input');await run();
    await change('output-destination','1');await change('output-name','ANOVAresults','input');
    await click('Write results');await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate("__pilotFake.sheets.get('new-1')?.name"),'ANOVAresults2');
    assert.equal(await evaluate("document.querySelector('#output-name').value"),'ANOVAresults');
    assert.equal(await evaluate("document.querySelector('#analysis-panel').textContent.includes('output.sheet-name-adjusted') && document.querySelector('#analysis-panel').textContent.includes(\"Created 'ANOVAresults2'\")"),true);
    assert.equal(await evaluate("[...document.querySelectorAll('#analysis-panel p[role=status]')].some(p=>p.textContent.includes('output.sheet-name-adjusted') && p.textContent.includes('ANOVAresults2'))"),true);
    assert.equal(await evaluate("!![...document.querySelectorAll('h4')].find(e=>e.textContent==='Overwrite occupied output?')"),false);
    await evaluate("[...document.querySelectorAll('#analysis-panel p[role=status]')].find(p=>p.textContent.includes('output.sheet-name-adjusted')).scrollIntoView({block:'center'})");
    await evaluate("new Promise(resolve=>requestAnimationFrame(()=>requestAnimationFrame(resolve)))");
    await screenshot('numbered-sheet-completed');
    const before=await evaluate("__pilotFake.state.added");await run();await click('Write results');
    await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate("__pilotFake.sheets.get('new-2')?.name"),'ANOVAresults4');
    assert.equal(await evaluate("__pilotFake.state.added"),before+1);
    assert.equal(await evaluate("document.querySelector('#output-name').value"),'ANOVAresults');
    summary.stages.push('long-whole-column-first-gap-repeated-base');
    assert.equal(await evaluate("document.documentElement.scrollWidth<=innerWidth"),true);assert.equal(summary.errors,0);summary.passed=true;
    console.log(JSON.stringify(summary,null,2));
}catch(error){summary.passed=false;summary.failure=error.code==='ERR_ASSERTION'?{actual:error.actual,expected:error.expected}:'browser.wait-or-evaluation-failed';
    summary.status=await evaluate("document.querySelector('#pilot-status')?.textContent");await screenshot('failure');console.log(JSON.stringify(summary,null,2));process.exitCode=1;
}finally{await send('Page.close');socket.close();}
