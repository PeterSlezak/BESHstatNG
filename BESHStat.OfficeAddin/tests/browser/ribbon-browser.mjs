// SIMULATED Office commands; real production Blazor/Core/Common. No authenticated Excel.
import assert from 'node:assert/strict';
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
const folder=new URL('../../../TestResults/04-07/ui-amendment/simulated/',import.meta.url);mkdirSync(folder,{recursive:true});
const target=await(await fetch('http://127.0.0.1:9227/json/new?about:blank',{method:'PUT'})).json();
const socket=new WebSocket(target.webSocketDebuggerUrl);await new Promise(r=>socket.addEventListener('open',r,{once:true}));
let serial=0;const pending=new Map(),summary={simulated:true,stage:'startup',errors:0};
const helper=readFileSync(new URL('../helpers/pilotWorkbook.mjs',import.meta.url),'utf8').replace('export function pilotWorkbook','function pilotWorkbook');
const fixture=helper+`
const fake=pilotWorkbook();globalThis.Office=fake.scope.Office;globalThis.Excel=fake.scope.Excel;globalThis.__pilotFake=fake;
Office.context.displayLanguage='sk-SK';Office.context.requirements={isSetSupported:()=>true};
globalThis.__commands={};globalThis.__opens=0;globalThis.__completed=0;globalThis.__help=[];
Office.context.ui={openBrowserWindow(url){__help.push(url);}};
Office.addin={async showAsTaskpane(){__opens++;}};
Office.actions={associate(name,fn){__commands[name]=fn;if(name==='BeshSettings')queueMicrotask(async()=>{globalThis.__cold=await fn({completed(){__completed++;}});});}};
Office.onReady=callback=>{const info={host:'Excel',platform:'OfficeOnline'};if(callback)callback(info);return Promise.resolve(info);};
localStorage.removeItem('beshstatng.office.preferences.v1');
`;
function send(method,params={}){return new Promise((resolve,reject)=>{const id=++serial;pending.set(id,{resolve,reject});socket.send(JSON.stringify({id,method,params}));});}
socket.addEventListener('message',event=>{const m=JSON.parse(event.data);if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error('cdp.failed')):p.resolve(m.result);}else if(m.method==='Runtime.exceptionThrown')summary.errors++;else if(m.method==='Fetch.requestPaused')void send('Fetch.fulfillRequest',{requestId:m.params.requestId,responseCode:200,responseHeaders:[{name:'Content-Type',value:'text/javascript'}],body:Buffer.from(fixture).toString('base64')});});
async function evaluate(expression){const r=await send('Runtime.evaluate',{expression,awaitPromise:true,returnByValue:true});assert.equal(r.exceptionDetails,undefined);return r.result.value;}
async function wait(expression){const until=Date.now()+45000;while(Date.now()<until){if(await evaluate(expression))return;await new Promise(r=>setTimeout(r,100));}throw Error('browser.timeout');}
async function action(name){return evaluate(`__commands[${JSON.stringify(name)}]({completed(){__completed++;}})`);}
async function change(id,value,event='input'){await evaluate(`{const e=document.getElementById(${JSON.stringify(id)});e.value=${JSON.stringify(value)};e.dispatchEvent(new Event(${JSON.stringify(event)},{bubbles:true}));}`);}
async function click(text){await evaluate(`[...document.querySelectorAll('button')].find(b=>b.textContent.trim()===${JSON.stringify(text)}).click()`);}
async function screenshot(name){const shot=await send('Page.captureScreenshot');writeFileSync(new URL(name+'.png',folder),Buffer.from(shot.data,'base64'));}
try{
    await send('Runtime.enable');await send('Page.enable');await send('Network.enable');await send('Network.setCacheDisabled',{cacheDisabled:true});
    await send('Emulation.setDeviceMetricsOverride',{width:360,height:900,deviceScaleFactor:1,mobile:false});
    await send('Fetch.enable',{patterns:[{urlPattern:'https://officeapis.public.onecdn.static.microsoft/1/office.js'}]});
    await send('Page.navigate',{url:'https://localhost:7141/'});
    await wait("document.querySelector('#options-tab')?.getAttribute('aria-selected')==='true' && !document.querySelector('#retry-initialization').disabled");
    assert.equal(await evaluate('__cold'),'command.pending');assert.equal(await evaluate('Object.keys(__commands).length'),7);
    assert.equal(await evaluate("document.querySelector('#options-tab').textContent"),'Options');
    assert.equal(await evaluate("document.querySelector('#global-force-english').checked"),true);
    summary.stage='workspace-header';
    assert.equal(await evaluate("document.querySelector('.workspace-header #header-about').textContent"),'About BESHStatNG');
    assert.equal(await evaluate("document.querySelector('.workspace-header #header-help').textContent"),'Help home');
    assert.equal(await evaluate("!!document.querySelector('#workspace-commands, #command-analysis, #command-options, #command-log, #command-export')"),false);
    await evaluate("document.querySelector('#export-tab').click()");
    assert.equal(await evaluate("document.querySelector('#export-tab').getAttribute('aria-selected')"),'true');
    assert.equal(await evaluate("document.querySelector('#export-panel').hidden"),false);
    assert.equal(await evaluate("document.querySelector('#export-placeholder').textContent"),'Chart export is not yet implemented in this Office add-in.');
    await screenshot('export-header-360');
    assert.equal(await evaluate("document.querySelector('#export-help').href.includes('route=ExportChart')"),true);
    assert.equal(await evaluate("document.querySelector('#export-help').target"),'_blank');
    assert.equal(await evaluate("document.querySelector('#header-help').href.startsWith('https://www.beshstat.eu/')"),true);
    assert.equal(await evaluate("document.querySelector('#header-help').target"),'_blank');
    assert.equal(await evaluate('__help.length'),0);
    await evaluate("document.querySelector('#header-about').click()");await wait("document.querySelector('#about-panel')?.open && document.activeElement.id==='about-close'");
    assert.equal(await evaluate("document.querySelector('#about-license').textContent"),'License: Apache License 2.0');
    assert.ok((await evaluate("document.querySelector('#about-panel').textContent")).includes('BESHStatNG · Office workspace'));
    await screenshot('about-360');
    await evaluate("document.querySelector('#about-close').click()");await wait("!document.querySelector('#about-panel') && document.activeElement.id==='header-about'");
    await evaluate("document.querySelector('#header-about').click()");await wait("document.querySelector('#about-panel')?.open");
    await send('Input.dispatchKeyEvent',{type:'keyDown',key:'Escape',code:'Escape',windowsVirtualKeyCode:27});await send('Input.dispatchKeyEvent',{type:'keyUp',key:'Escape',code:'Escape',windowsVirtualKeyCode:27});await wait("!document.querySelector('#about-panel') && document.activeElement.id==='header-about'");
    await evaluate('__help.length=0');assert.equal(await evaluate('__pilotFake.state.mutations'),0);
    assert.equal(await evaluate("document.querySelector('.workspace-header #retry-initialization')===null"),true);
    assert.equal(await evaluate("!!document.querySelector('#diagnostics-panel #retry-initialization')"),true);
    assert.equal(await evaluate("getComputedStyle(document.querySelector('.workspace-header h1')).position==='absolute'"),true);
    summary.stage='manifest';
    const manifest=readFileSync(new URL('../../manifest-dev.xml',import.meta.url),'utf8');
    const counts=await evaluate(`(()=>{const doc=new DOMParser().parseFromString(${JSON.stringify(manifest)},'text/xml');const items=[...doc.getElementsByTagNameNS('*','Item')];return {items:items.length,disabled:items.filter(i=>i.getElementsByTagNameNS('*','Enabled')[0]?.textContent==='false').length,menus:[...doc.getElementsByTagNameNS('*','Control')].filter(c=>c.getAttributeNS('http://www.w3.org/2001/XMLSchema-instance','type')==='Menu').length};})()`);
    assert.deepEqual(counts,{items:79,disabled:77,menus:12});summary.manifest=counts;
    summary.stage='anova';assert.equal(await action('BeshAnova'),'command.navigated');await wait("document.querySelector('#analysis-method').value==='anova.oneway'");
    await change('analysis-alpha','0.125');assert.equal(await action('BeshAnova'),'command.navigated');assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"),'0.125');
    assert.equal(await evaluate('__pilotFake.state.runs'),0);assert.equal(await evaluate('__pilotFake.state.mutations'),0);
    summary.stage='anova-run';await change('input-reference',"'Input'!A1:C4");await click('Run analysis');await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    await change('output-start',"'Input'!J1");await evaluate('__pilotFake.state.setCell(1,10,99)');await wait("[...document.querySelectorAll('button')].some(b=>b.textContent==='Write results' && !b.disabled)");await click('Write results');
    summary.stage='anova-consent';await wait("!!document.querySelector('#overwrite-heading')");await click('Yes, overwrite inspected output');await wait("document.querySelector('#pilot-status code')?.textContent==='output.completed'");
    const mutations=await evaluate('__pilotFake.state.mutations');assert.ok(mutations>0);
    summary.stage='kw';assert.equal(await action('BeshKruskal'),'command.navigated');await wait("document.querySelector('#analysis-method').value==='nonparametric.kruskal-wallis'");
    assert.equal(await evaluate("!!document.querySelector('#overwrite-heading')"),false);assert.equal(await evaluate('__pilotFake.state.mutations'),mutations);
    await change('input-reference',"'Input'!A1:C4");await click('Run analysis');await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    await click('Run analysis');await wait("document.querySelector('#pilot-status code')?.textContent==='analysis.completed'");
    summary.stage='utilities';assert.equal(await action('BeshSettings'),'command.navigated');assert.equal(await evaluate("document.querySelector('#options-tab').getAttribute('aria-selected')"),'true');
    assert.equal(await action('BeshLog'),'command.navigated');assert.equal(await evaluate("document.querySelector('#diagnostics-tab').getAttribute('aria-selected')"),'true');
    const log=await evaluate("document.querySelector('#sanitized-log').textContent");assert.ok(log.includes('diagnostic.command.completed'));assert.equal(/Input|A1:C4|J1|https:/.test(log),false);
    summary.stage='about-open';assert.equal(await action('BeshAbout'),'command.navigated');await wait("document.querySelector('#about-panel')?.open && document.activeElement.id==='about-close'");
    summary.stage='about-close';
    await send('Input.dispatchKeyEvent',{type:'keyDown',key:'Escape',code:'Escape',windowsVirtualKeyCode:27});await send('Input.dispatchKeyEvent',{type:'keyUp',key:'Escape',code:'Escape',windowsVirtualKeyCode:27});await wait("!document.querySelector('#about-panel') && document.activeElement.id==='diagnostics-tab'");
    assert.equal(await action('BeshHelp'),'command.navigated');assert.equal(await evaluate('__help.length'),1);assert.ok((await evaluate('__help[0]')).startsWith('https://www.beshstat.eu/'));
    assert.equal(await action('BeshUnavailable'),'command.unavailable');assert.equal(await evaluate('__pilotFake.state.mutations'),mutations);
    assert.equal(await evaluate("localStorage.getItem('beshstatng.office.preferences.v1')"),null);
    await evaluate("document.querySelector('#retry-initialization').click()");await wait("!document.querySelector('#retry-initialization').disabled");assert.equal(await evaluate('Object.keys(__commands).length'),7);
    assert.equal(await action('BeshAnova'),'command.navigated');assert.equal(await evaluate("document.querySelector('#analysis-method').value"),'anova.oneway');
    await evaluate("document.querySelector('#export-tab').click()");
    for(const width of [360,213]){await send('Emulation.setDeviceMetricsOverride',{width,height:900,deviceScaleFactor:1,mobile:false});assert.equal(await evaluate('document.documentElement.scrollWidth<=innerWidth'),true);assert.equal(await evaluate("document.querySelector('.workspace-tabs').getBoundingClientRect().bottom<innerHeight && document.querySelector('#header-help').getBoundingClientRect().top>=0"),true);}
    const shot=await send('Page.captureScreenshot');writeFileSync(new URL('ribbon.png',folder),Buffer.from(shot.data,'base64'));
    assert.equal(summary.errors,0);summary.stage='completed';summary.passed=true;
}catch(error){summary.passed=false;summary.failure=error.code==='ERR_ASSERTION'?{actual:error.actual,expected:error.expected,message:error.message}:error.message;summary.status=await evaluate("({status:document.querySelector('#pilot-status')?.textContent,output:document.querySelector('#output-start-status')?.textContent,buttons:[...document.querySelectorAll('.analysis-pilot button')].map(b=>({text:b.textContent,disabled:b.disabled})),outputValue:document.querySelector('#output-start')?.value})");const shot=await send('Page.captureScreenshot');writeFileSync(new URL('ribbon-failure.png',folder),Buffer.from(shot.data,'base64'));process.exitCode=1;}
finally{writeFileSync(new URL('ribbon.json',folder),JSON.stringify(summary,null,2));console.log(JSON.stringify(summary));await send('Page.close');socket.close();}
