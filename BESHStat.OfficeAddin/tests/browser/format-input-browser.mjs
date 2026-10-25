// Real Blazor/Core/Common, SIMULATED Office only. No authenticated Excel or workbook acceptance claim.
import assert from 'node:assert/strict';
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
const target=await(await fetch('http://127.0.0.1:9227/json/new?about:blank',{method:'PUT'})).json();
const socket=new WebSocket(target.webSocketDebuggerUrl);await new Promise(r=>socket.addEventListener('open',r,{once:true}));
let serial=0;const pending=new Map(),summary={simulated:true,errors:0,screenshots:[],reflowCases:[]};
const helper=readFileSync(new URL('../helpers/pilotWorkbook.mjs',import.meta.url),'utf8').replace('export function pilotWorkbook','function pilotWorkbook');
const golden=JSON.parse(readFileSync(new URL('../../../BESHStat.Excel.Common.Tests/TestData/Stage304/anova-base.json',import.meta.url),'utf8'));
const fixture=`${helper}\nconst fake=pilotWorkbook(),g=${JSON.stringify(golden.groups)};
for(let c=0;c<3;c++){fake.state.setCell(1,c+1,g.names[c]);g.values[c].forEach((v,r)=>fake.state.setCell(r+2,c+1,v));}
fake.state.setCell(1,6,'Group');fake.state.setCell(1,5,'Value');let row=2;
for(let c=0;c<3;c++)for(const value of g.values[c]){fake.state.setCell(row,6,g.names[c]);fake.state.setCell(row++,5,value);}
globalThis.Office=fake.scope.Office;globalThis.Excel=fake.scope.Excel;globalThis.__pilotFake=fake;`;
function send(method,params={}){return new Promise((resolve,reject)=>{const id=++serial;pending.set(id,{resolve,reject});socket.send(JSON.stringify({id,method,params}));});}
socket.addEventListener('message',event=>{const m=JSON.parse(event.data),p=m.params;if(m.id){const r=pending.get(m.id);pending.delete(m.id);m.error?r.reject(Error('browser.command-failed')):r.resolve(m.result);}
else if(m.method==='Fetch.requestPaused')void send('Fetch.fulfillRequest',{requestId:p.requestId,responseCode:200,responseHeaders:[{name:'Content-Type',value:'text/javascript'}],body:Buffer.from(fixture).toString('base64')});
else if(m.method==='Runtime.exceptionThrown'&&p.exceptionDetails.url?.startsWith('https://localhost:7141'))summary.errors++;});
async function evaluate(expression){const r=await send('Runtime.evaluate',{expression,awaitPromise:true,returnByValue:true});assert.equal(r.exceptionDetails,undefined);return r.result.value;}
async function wait(expression){const until=Date.now()+45000;while(Date.now()<until){if(await evaluate(expression))return;await new Promise(r=>setTimeout(r,100));}throw Error('browser.timeout');}
async function click(text){await evaluate(`[...document.querySelectorAll('button')].find(b=>b.textContent===${JSON.stringify(text)}).click()`);}
async function change(id,value,event='change'){await evaluate(`{const e=document.getElementById(${JSON.stringify(id)});e.value=${JSON.stringify(value)};e.dispatchEvent(new Event(${JSON.stringify(event)},{bubbles:true}));}`);}
async function run(){await click('Run analysis');await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");}
async function screenshot(name){const folder=new URL('../../../TestResults/04-04/format-input/screenshots/',import.meta.url);mkdirSync(folder,{recursive:true});const shot=await send('Page.captureScreenshot');writeFileSync(new URL(`${name}.png`,folder),Buffer.from(shot.data,'base64'));summary.screenshots.push(name);}
try{
    await send('Runtime.enable');await send('Page.enable');await send('Network.enable');await send('Network.setCacheDisabled',{cacheDisabled:true});
    await send('Emulation.setDeviceMetricsOverride',{width:360,height:760,deviceScaleFactor:1,mobile:false});
    await send('Fetch.enable',{patterns:[{urlPattern:'https://officeapis.public.onecdn.static.microsoft/1/office.js'}]});
    await send('Page.navigate',{url:'https://localhost:7141/?beshOfficeDiagnostics=1'});await wait("document.querySelector('#host-code')?.textContent==='office.excel-ready'");
    await change('analysis-group','Parametric');await change('analysis-method','anova.oneway');await wait("!!document.querySelector('#input-reference')");
    summary.stage='whole-column-capture-and-direct-run';
    await evaluate("__pilotFake.state.selected={row:1,col:1,rows:1048576,cols:3}");await click('Capture selected range');
    await wait("document.querySelector('#input-reference').value===\"'Input'!A:C\"");await run();
    assert.equal(await evaluate("document.querySelector('#analysis-panel').textContent.includes('whole columns narrowed to 4')"),true);
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='FStatistic').nextElementSibling.textContent"),'13');
    assert.equal(await evaluate("!!document.querySelector('[aria-label=\"Redacted input preview\"]')"),false);
    summary.stage='sticky-widths-and-text-zoom';
    for(const width of [320,360,400])for(const percent of [100,200]){
        summary.currentReflow={width,textZoom:percent};
        await send('Emulation.setDeviceMetricsOverride',{width,height:760,deviceScaleFactor:1,mobile:false});
        await evaluate(`document.documentElement.style.fontSize='${percent}%'`);
        const before=await evaluate("document.querySelector('.workspace-header').getBoundingClientRect().top");
        await evaluate("document.querySelector('#analysis-panel').scrollTop=100000");
        assert.equal(await evaluate("document.querySelector('#analysis-panel').scrollTop>0"),true,'panel scrolls');
        assert.equal(await evaluate("document.querySelector('.workspace-header').getBoundingClientRect().top"),before);
        assert.equal(await evaluate("document.querySelector('.workspace-tabs').getBoundingClientRect().bottom<=innerHeight && document.querySelector('#analysis-panel').clientHeight>0"),true,'header fits and panel remains usable');
        assert.equal(await evaluate("document.scrollingElement.scrollTop===0 && document.querySelector('.workspace-tabs').scrollWidth<=document.querySelector('.workspace-tabs').clientWidth"),true,'no body scroll or clipped tabs');
        const position=await evaluate("document.querySelector('#analysis-panel').scrollTop"),reads=await evaluate('__pilotFake.state.loads.length');
        await click('Diagnostics / Log');await click('Analysis');
        assert.equal(await evaluate("document.querySelector('#analysis-panel').scrollTop"),position);assert.equal(await evaluate('__pilotFake.state.loads.length'),reads);
        await evaluate("document.querySelector('#analysis-tab').focus();document.querySelector('#input-reference').focus()");
        assert.equal(await evaluate("document.querySelector('#input-reference').getBoundingClientRect().top>=document.querySelector('.workspace-header').getBoundingClientRect().bottom-1"),true,'focused field is below sticky header');
        summary.reflowCases.push({width,textZoom:percent});
    }
    await screenshot('sticky-400-200-percent-text');await evaluate("document.documentElement.style.fontSize='100%'");
    await send('Emulation.setDeviceMetricsOverride',{width:360,height:760,deviceScaleFactor:1,mobile:false});
    summary.stage='unequal-blank-and-populated-overhang';
    await change('group-arrangement','2');await change('category-reference',"'Input'!F1:F10",'input');await change('values-reference',"'Input'!E1:E12",'input');await run();
    assert.equal(await evaluate("document.querySelector('#analysis-panel').textContent.includes('2 selected cells outside the matching worksheet rows are ignored (0 nonblank)')"),true);
    await evaluate("__pilotFake.state.setCell(12,5,0);__pilotFake.state.emit('onChanged')");await run();
    assert.equal(await evaluate("[...document.querySelectorAll('[role=alert]')].some(e=>e.textContent.includes('(1 nonblank)'))"),true);
    assert.equal(await evaluate("[...document.querySelectorAll('dt')].find(d=>d.textContent==='FStatistic').nextElementSibling.textContent"),'13');
    await screenshot('unequal-overhang-warning');
    summary.stage='two-whole-columns';
    await evaluate("__pilotFake.cells.delete('sheet-1:12:5');__pilotFake.state.emit('onChanged')");
    await change('category-reference',"'Input'!F:F",'input');await change('values-reference',"'Input'!E:E",'input');await run();
    assert.equal(await evaluate("[...document.querySelectorAll('code')].filter(e=>e.textContent==='input.columns-narrowed').length"),2);
    summary.stage='one-click-new-sheet-with-readback';
    await change('output-destination','1');await change('output-name','Parity result','input');
    assert.equal(await evaluate('__pilotFake.state.added'),0);await click('Write results');await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate('__pilotFake.state.added'),1);assert.equal(await evaluate("__pilotFake.fonts.get('new-1:2:6').color"),'#32FF32');
    assert.equal(await evaluate("__pilotFake.fills.get('new-1:1:1').color"),'#DDDDDD');
    assert.equal(await evaluate("__pilotFake.formats.get('new-1:2:6').horizontalAlignment"),'Left');
    summary.stage='independent-second-run';
    await evaluate(`{const x=__pilotFake,g=${JSON.stringify(golden.groups)};globalThis.__firstResult=JSON.stringify([...x.cells].filter(([k])=>k.startsWith('new-1:')));x.sheets.set('source-b',{id:'source-b',name:'Second'});x.state.sheetId='source-b';for(let c=0;c<3;c++){x.state.setCell(1,c+1,g.names[c]);g.values[c].forEach((v,r)=>x.state.setCell(r+2,c+1,v*2));}x.state.selected={row:1,col:1,rows:4,cols:3};}`);
    await change('group-arrangement','0');await click('Capture selected range');await wait("document.querySelector('#input-reference').value.includes('Second')");await run();
    await change('output-destination','0');await change('output-start',"'Second'!J1",'input');await click('Write results');await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.equal(await evaluate("JSON.stringify([...__pilotFake.cells].filter(([k])=>k.startsWith('new-1:')))===__firstResult"),true);
    assert.equal(await evaluate('__pilotFake.state.listenerCount()'),0);assert.equal(summary.errors,0);summary.passed=true;console.log(JSON.stringify(summary,null,2));
}catch(e){summary.passed=false;summary.failure=e.code==='ERR_ASSERTION'?{actual:e.actual,expected:e.expected,message:e.message}:e.message;summary.status=await evaluate("document.querySelector('#pilot-status code')?.textContent");await screenshot('amendment-failure');console.log(JSON.stringify(summary,null,2));process.exitCode=1;}
finally{await send('Page.close');socket.close();}
