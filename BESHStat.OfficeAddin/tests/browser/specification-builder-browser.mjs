// Real Blazor/Common/Core, simulated Office boundary only. No authenticated Excel.
import assert from 'node:assert/strict';
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
const endpoint='http://127.0.0.1:9227';
const target=await(await fetch(endpoint+'/json/new?about:blank',{method:'PUT'})).json();
const socket=new WebSocket(target.webSocketDebuggerUrl);await new Promise(r=>socket.addEventListener('open',r,{once:true}));
let serial=0;const pending=new Map(),summary={simulated:true,errors:0,stages:[],screenshots:[]};
const helper=readFileSync(new URL('../helpers/pilotWorkbook.mjs',import.meta.url),'utf8').replace('export function pilotWorkbook','function pilotWorkbook');
const fixture=helper+`
const fake=pilotWorkbook();fake.cells.clear();fake.sheets.get('sheet-1').name='Hárok 1';
fake.sheets.set('model',{id:'model',name:'Hárok2'});fake.sheets.set('repeated',{id:'repeated',name:"Visit's data"});fake.sheets.set('anova',{id:'anova',name:'ANOVA'});
function populate(id,rows){fake.state.sheetId=id;rows.forEach((row,r)=>row.forEach((v,c)=>{if(v!==null)fake.state.setCell(r+1,c+1,v);}));}
populate('sheet-1',[['y','trtn','age','base','subject','period'],[1,'T',20,0,'S1',1],[2,'C',30,1,'S2',2]]);
populate('model',[['Response',null,'Dose',null,'Treatment'],[2,null,1,null,'Control'],[5,null,2,null,'Active'],[6,null,3,null,'Control'],[9,null,4,null,'Active'],[11,null,5,null,'Control'],[14,null,6,null,'Active'],[15,null,7,null,'Control'],[18,null,8,null,'Active']]);
populate('repeated',[['Subject',null,'Visit',null,'Response',null,'Treatment',null,'Dose'],['S1',null,1,null,10,null,'Control',null,1],['S1',null,2,null,11,null,'Control',null,2],['S2',null,1,null,12,null,'Active',null,1],['S2',null,2,null,13,null,'Active',null,2],['S3',null,1,null,14,null,'Control',null,1],['S3',null,2,null,15,null,'Control',null,2]]);
populate('anova',[['A','B','C'],[1,2,5],[2,3,6],[3,4,7]]);
fake.state.sheetId='sheet-1';fake.state.selected={row:50,col:12,rows:1,cols:1};
globalThis.Office=fake.scope.Office;globalThis.Excel=fake.scope.Excel;globalThis.__pilotFake=fake;`;
function send(method,params={}){return new Promise((resolve,reject)=>{const id=++serial;pending.set(id,{resolve,reject});socket.send(JSON.stringify({id,method,params}));});}
async function evaluate(expression){const r=await send('Runtime.evaluate',{expression,awaitPromise:true,returnByValue:true});assert.equal(r.exceptionDetails,undefined);return r.result.value;}
async function wait(expression){const until=Date.now()+45000;while(Date.now()<until){if(await evaluate(expression))return;await new Promise(r=>setTimeout(r,100));}throw Error('browser.timeout');}
async function click(text){await evaluate('[...document.querySelectorAll("button")].find(b=>b.textContent.trim()==='+JSON.stringify(text)+').click()');}
async function change(id,value,event='change'){await evaluate(`{const e=document.getElementById(${JSON.stringify(id)});let value=${JSON.stringify(value)};if(e.dataset.columnRole==='true')value=[...e.options].find(o=>o.dataset.parserIndex===String(value))?.value??'-1';if(e.type==='checkbox')e.checked=value;else e.value=value;e.dispatchEvent(new Event(${JSON.stringify(event)},{bubbles:true}));}`);}
async function variables(){await wait("document.querySelector('#spec-refresh') && !document.querySelector('#spec-refresh').disabled && document.querySelector('#spec-status').textContent.includes('spec.variables')");}
async function preview(){await click('Validate specification / Preview design');await wait("document.querySelector('#spec-status')?.textContent.includes('spec.ready') || document.querySelector('#spec-status')?.textContent.includes('spec.invalid')");}
async function modelRoles(){await change('model-response','0');await change('model-predictors-1',true);await change('model-predictors-2',true);await change('model-factors-2',true);}
async function repeatedRoles(){await change('repeat-subject','0');await change('repeat-visit','1');await change('repeat-response','2');await change('repeat-group','3');await change('repeat-order',true);}
async function screenshot(name){const folder=new URL('../../../TestResults/04-05/worksheet-discovery/screenshots/',import.meta.url);mkdirSync(folder,{recursive:true});const shot=await send('Page.captureScreenshot');writeFileSync(new URL(name+'.png',folder),Buffer.from(shot.data,'base64'));summary.screenshots.push(name);}
socket.addEventListener('message',async event=>{const m=JSON.parse(event.data);if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error('cdp.failed')):p.resolve(m.result);}else if(m.method==='Runtime.exceptionThrown')summary.errors++;else if(m.method==='Fetch.requestPaused')await send('Fetch.fulfillRequest',{requestId:m.params.requestId,responseCode:200,responseHeaders:[{name:'Content-Type',value:'text/javascript'}],body:Buffer.from(fixture).toString('base64')});});
try{
    await send('Runtime.enable');await send('Page.enable');await send('Network.enable');await send('Network.setCacheDisabled',{cacheDisabled:true});
    await send('Emulation.setDeviceMetricsOverride',{width:360,height:800,deviceScaleFactor:1,mobile:false});
    await send('Fetch.enable',{patterns:[{urlPattern:'https://officeapis.public.onecdn.static.microsoft/1/office.js'}]});
    await send('Page.navigate',{url:'https://localhost:7141/?beshOfficeDiagnostics=1'});await wait("document.querySelector('#host-code')?.textContent==='office.excel-ready'");
    await click('Open specification builder preview (no fit / write)');await variables();
    assert.equal(await evaluate("document.querySelector('#spec-worksheet').value"),'sheet-1');assert.equal(await evaluate("!!document.querySelector('#spec-source')"),false);
    assert.deepEqual(await evaluate("[...document.querySelector('#model-response').options].slice(1).map(o=>o.textContent)"),['y | VarA','trtn | VarB','age | VarC','base | VarD','subject | VarE','period | VarF']);
    await evaluate("document.querySelector('#spec-worksheet').scrollIntoView({block:'start'});new Promise(r=>requestAnimationFrame(()=>requestAnimationFrame(r)))");await screenshot('automatic-owner-variables');
    await change('spec-worksheet','model');await variables();assert.equal(await evaluate("document.querySelector('#spec-worksheet').value"),'model');
    assert.deepEqual(await evaluate("[...document.querySelector('#model-response').options].slice(1).map(o=>o.textContent)"),['Response | VarA','Dose | VarC','Treatment | VarE']);
    assert.equal(await evaluate("__pilotFake.state.sheetId"),'sheet-1');await modelRoles();
    const loads=await evaluate("__pilotFake.state.loads.filter(s=>s.split(',').includes('values')).length");
    await change('term-first','1');await click('Add term');await change('term-first','2');await click('Add term');
    assert.equal(await evaluate("__pilotFake.state.loads.filter(s=>s.split(',').includes('values')).length"),loads);
    await preview();assert.equal(await evaluate("document.querySelector('.specification-builder').textContent.includes('8 complete observations; expanded design width 3')"),true);
    await change('factor-reference-2','1');await preview();
    await change('term-kind','Interaction');await change('term-first','1');await change('term-second','2');await click('Add term');await preview();
    assert.equal(await evaluate("document.querySelector('.specification-builder').textContent.includes('expanded design width 4')"),true);
    await change('term-kind','Polynomial');await change('term-first','1');await change('term-degree','2');await click('Add term');await preview();
    assert.equal(await evaluate("document.querySelector('.specification-builder').textContent.includes('expanded design width 5')"),true);
    await evaluate("document.querySelector('#normalized-rhs').scrollIntoView({block:'center'});new Promise(r=>requestAnimationFrame(()=>requestAnimationFrame(r)))");await screenshot('sparse-model-design');
    for(const width of [320,360,400]){await send('Emulation.setDeviceMetricsOverride',{width,height:800,deviceScaleFactor:1,mobile:false});assert.equal(await evaluate('document.documentElement.scrollWidth<=innerWidth+1'),true);}
    await click('Diagnostics / Log');await click('Analysis');assert.equal(await evaluate("document.querySelector('#normalized-rhs').textContent.includes('^2')"),true);
    await change('formula-manual',true);await change('formula-rhs',"'Dose' + (",'input');await preview();assert.equal(await evaluate("document.querySelector('.specification-builder').textContent.includes('model.formula')"),true);
    await change('formula-manual',false);await preview();
    await evaluate("dispatchEvent(new Event('focus'))");await variables();assert.equal(await evaluate("document.querySelector('#spec-worksheet').value"),'model');await preview();
    await evaluate("__pilotFake.state.sheetId='model';__pilotFake.state.setCell(2,1,3);__pilotFake.state.emit('onChanged');__pilotFake.state.sheetId='sheet-1'");await variables();
    assert.equal(await evaluate("!!document.querySelector('#normalized-rhs')"),false);await modelRoles();await preview();
    assert.equal(await evaluate('__pilotFake.state.mutations'),0);summary.stages.push('automatic-active-owner-six-captions-sparse-physical-model-widths-3-4-5-local-edits-source-reset');
    await change('spec-worksheet','repeated');await variables();await change('spec-family','Repeated');await repeatedRoles();await preview();
    assert.equal(await evaluate("document.querySelector('.specification-builder').textContent.includes('3 subjects; 6 observations')"),true);
    await evaluate("document.querySelector('#spec-status').scrollIntoView({block:'center'});new Promise(r=>requestAnimationFrame(()=>requestAnimationFrame(r)))");await screenshot('sparse-repeated-spec');
    await evaluate("__pilotFake.state.sheetId='repeated';__pilotFake.state.setCell(3,3,1);__pilotFake.state.emit('onChanged');__pilotFake.state.sheetId='sheet-1'");await variables();await repeatedRoles();await preview();
    assert.equal(await evaluate("document.querySelector('.specification-builder').textContent.includes('repeat.duplicate-visit') && document.querySelector('.specification-builder').textContent.includes('worksheet row/column 3/3')"),true);
    await evaluate("__pilotFake.state.sheetId='repeated';__pilotFake.state.setCell(3,3,2);__pilotFake.state.emit('onChanged');__pilotFake.state.sheetId='sheet-1'");await variables();await repeatedRoles();await preview();
    await change('spec-family','Mixed');await change('mixed-fixed-4',true);await change('mixed-random-0',true);await preview();assert.equal(await evaluate("document.querySelector('#spec-status').textContent.includes('spec.ready')"),true);
    await change('spec-family','Categorical');await change('category-column','3');await preview();assert.equal(await evaluate("document.querySelector('#spec-status').textContent.includes('spec.ready')"),true);
    assert.equal(await evaluate('__pilotFake.state.mutations'),0);summary.stages.push('unicode-apostrophe-sheet-switch-repeated-duplicate-physical-diagnostic-mixed-categorical-zero-writes');
    await click('Return to analysis');await change('analysis-group','Regression');assert.equal(await evaluate("[...document.querySelector('#analysis-method').options].filter(o=>o.value).every(o=>o.disabled)"),true);
    await evaluate("__pilotFake.state.sheetId='anova';__pilotFake.state.selected={row:1,col:1,rows:4,cols:3}");
    await change('analysis-group','Parametric');await change('analysis-method','anova.oneway');await change('input-reference',"'ANOVA'!A1:C4",'input');
    await click('Run analysis');await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    await change('output-start',"'ANOVA'!J1",'input');await click('Write results');await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    assert.ok(await evaluate('__pilotFake.state.mutations')>0);assert.equal(await evaluate("document.querySelectorAll('[role=tab]').length"),3);assert.equal(summary.errors,0);
    summary.stages.push('unsupported-menu-disabled-return-to-anova-refedit-run-and-verified-write');summary.passed=true;
    writeFileSync(new URL('../../../TestResults/04-05/worksheet-discovery/browser-summary.json',import.meta.url),JSON.stringify(summary,null,2));console.log(JSON.stringify(summary,null,2));
}catch(error){summary.passed=false;summary.failure=error.code==='ERR_ASSERTION'?{actual:error.actual,expected:error.expected}:error.message;summary.status=await evaluate("document.querySelector('#spec-status')?.textContent || document.querySelector('#pilot-status code')?.textContent");await screenshot('failure');console.log(JSON.stringify(summary,null,2));process.exitCode=1;}
finally{await send('Page.close');socket.close();}
