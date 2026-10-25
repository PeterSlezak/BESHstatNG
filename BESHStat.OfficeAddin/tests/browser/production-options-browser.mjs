// SIMULATED Office only. Real Blazor/WASM/Common/Core, fresh isolated Edge profile on port 9227.
import assert from 'node:assert/strict';
import { readFileSync, mkdirSync, writeFileSync } from 'node:fs';
const slovak = process.argv.includes('--sk'), blocked = process.argv.includes('--storage-blocked');
const amendment = process.argv.includes('--force-english');
const label = amendment ? 'force-english' : blocked ? 'storage-blocked' : slovak ? 'sk-office-en-browser' : 'en-office-sk-browser';
const folder = new URL('../../../TestResults/04-06/force-english/browser/', import.meta.url); mkdirSync(folder, { recursive: true });
const target = await (await fetch('http://127.0.0.1:9227/json/new?about:blank', { method: 'PUT' })).json();
const socket = new WebSocket(target.webSocketDebuggerUrl); await new Promise(r => socket.addEventListener('open', r, { once: true }));
let serial = 0; const pending = new Map(), summary = { simulated: true, label, errors: 0, stage: 'startup' };
const helper = readFileSync(new URL('../helpers/pilotWorkbook.mjs', import.meta.url), 'utf8').replace('export function pilotWorkbook', 'function pilotWorkbook');
const fixture = helper + `
const fake = pilotWorkbook(); globalThis.Office = fake.scope.Office; globalThis.Excel = fake.scope.Excel; globalThis.__pilotFake = fake;
Object.defineProperty(navigator, 'language', {value:${JSON.stringify(slovak ? 'en-US' : 'sk-SK')}, configurable:true});
Office.context.displayLanguage=${JSON.stringify(slovak || amendment ? 'sk-SK' : 'en-US')};
globalThis.__helpRequests=[];Office.context.ui={openBrowserWindow(url){if(globalThis.__blockHelp)throw Error('blocked');__helpRequests.push(url);}};
if (!sessionStorage.getItem('besh.options-test')) {
    localStorage.removeItem('beshstatng.office.preferences.v1'); sessionStorage.setItem('besh.options-test','1');
    ${amendment ? "localStorage.setItem('beshstatng.office.preferences.v1',JSON.stringify({schemaVersion:1,defaultAlpha:0.025,defaultRandomSeed:12345,traceEnabled:false,pValues:{decimalPlaces:4,smallValueDisplay:1,useUpperBound:false}}));" : ''}
}
${blocked ? "Object.defineProperty(globalThis,'localStorage',{get(){throw Error('SecurityError');},configurable:true});" : ''}
`;
function send(method, params = {}) { return new Promise((resolve, reject) => { const id = ++serial; pending.set(id, { resolve, reject }); socket.send(JSON.stringify({ id, method, params })); }); }
socket.addEventListener('message', async event => {
    const m = JSON.parse(event.data);
    if (m.id) { const p = pending.get(m.id); pending.delete(m.id); m.error ? p.reject(Error('cdp.failed')) : p.resolve(m.result); }
    else if (m.method === 'Runtime.exceptionThrown') summary.errors++;
    else if (m.method === 'Fetch.requestPaused') await send('Fetch.fulfillRequest', { requestId: m.params.requestId, responseCode: 200, responseHeaders: [{ name: 'Content-Type', value: 'text/javascript' }], body: Buffer.from(fixture).toString('base64') });
});
async function evaluate(expression) { const r = await send('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true }); assert.equal(r.exceptionDetails, undefined); return r.result.value; }
async function wait(expression) { const until = Date.now() + 40000; while (Date.now() < until) { if (await evaluate(expression)) return; await new Promise(r => setTimeout(r, 100)); } throw Error('browser.timeout'); }
async function change(id, value, event = 'input') { await evaluate(`{const e=document.getElementById(${JSON.stringify(id)});if(e.type==='checkbox')e.checked=${JSON.stringify(value)};else e.value=${JSON.stringify(value)};e.dispatchEvent(new Event(${JSON.stringify(event)},{bubbles:true}));}`); }
async function clickId(id) { await evaluate(`document.getElementById(${JSON.stringify(id)}).click()`); }
async function click(text) { await evaluate(`[...document.querySelectorAll('button')].find(b=>b.textContent.trim()===${JSON.stringify(text)}).click()`); }
try {
    await send('Runtime.enable'); await send('Page.enable'); await send('Network.enable'); await send('Network.setCacheDisabled', { cacheDisabled: true });
    await send('Emulation.setDeviceMetricsOverride', { width: 320, height: 800, deviceScaleFactor: 1, mobile: false });
    await send('Fetch.enable', { patterns: [{ urlPattern: 'https://officeapis.public.onecdn.static.microsoft/1/office.js' }] });
    await send('Page.navigate', { url: 'https://localhost:7141/' }); await wait("!!document.querySelector('#analysis-group') && !document.querySelector('#retry-initialization').disabled");
    assert.equal(await evaluate("document.querySelectorAll('[role=tab]').length"), 4);
    assert.equal(await evaluate("document.querySelector('#options-tab').textContent"), 'Options');
    assert.equal(await evaluate("document.querySelector('#global-force-english').checked"), true);
    if (amendment) {
        summary.stage = 'force-english-draft'; await clickId('options-tab');
        assert.equal(await evaluate("document.querySelector('#global-alpha').value"), '0.025');
        assert.equal(await evaluate("document.querySelector('#global-seed').value"), '12345');
        assert.equal(await evaluate("!!document.querySelector('label[for=global-force-english]')"), true);
        await evaluate("document.querySelector('#global-force-english').focus()");
        await send('Input.dispatchKeyEvent', { type:'keyDown', key:' ', code:'Space', windowsVirtualKeyCode:32 });
        await send('Input.dispatchKeyEvent', { type:'keyUp', key:' ', code:'Space', windowsVirtualKeyCode:32 });
        await wait("!document.querySelector('#global-force-english').checked");
        assert.equal(await evaluate("document.querySelector('#options-tab').textContent"), 'Options');
        await clickId('global-discard'); assert.equal(await evaluate("document.querySelector('#global-force-english').checked"), true);
        await change('global-force-english', false, 'change'); await clickId('global-defaults');
        assert.equal(await evaluate("document.querySelector('#global-force-english').checked"), true); await clickId('global-discard');
        await click('Global settings Help'); await wait('__helpRequests.length===1');
        const helpUrl = await evaluate('__helpRequests[0]');
        await change('global-force-english', false, 'change'); await clickId('global-save');
        await wait("document.querySelector('#options-tab').textContent==='Možnosti'");
        assert.deepEqual(await evaluate("['analysis-tab','options-tab','diagnostics-tab'].map(id=>document.getElementById(id).textContent)"), ['Analýza','Možnosti','Diagnostika / Záznam']);
        assert.equal(await evaluate("document.querySelector('#global-alpha').value"), '0.025');
        assert.equal(await evaluate("JSON.parse(localStorage.getItem('beshstatng.office.preferences.v1')).forceEnglish"), false);
        await click('Pomocník ku globálnym nastaveniam'); await wait('__helpRequests.length===2'); assert.equal(await evaluate('__helpRequests[1]'), helpUrl);
        await send('Page.reload',{ignoreCache:true}); await wait("document.querySelector('#options-tab')?.textContent==='Možnosti' && !!document.querySelector('#analysis-group')");
        assert.equal(await evaluate("document.querySelector('#global-force-english').checked"), false);
        await clickId('options-tab'); await change('global-force-english', true, 'change');
        assert.equal(await evaluate("document.querySelector('#options-tab').textContent"), 'Možnosti');
        await clickId('global-save'); await wait("document.querySelector('#options-tab').textContent==='Options'");
        await send('Page.reload',{ignoreCache:true}); await wait("document.querySelector('#global-force-english')?.checked && !!document.querySelector('#analysis-group')");
        await clickId('options-tab');
        await evaluate("Storage.prototype.setItem=function(){throw Error('quota');}");
        await change('global-force-english', false, 'change'); await clickId('global-save'); await wait("!!document.querySelector('#global-session')");
        assert.equal(await evaluate("document.querySelector('#options-tab').textContent"), 'Options');
        await clickId('global-session'); await wait("document.querySelector('#options-tab').textContent==='Možnosti'");
        assert.equal(await evaluate("JSON.parse(localStorage.getItem('beshstatng.office.preferences.v1')).forceEnglish"), true);
        assert.equal(await evaluate('__pilotFake.state.mutations'),0);
        await send('Page.reload',{ignoreCache:true}); await wait("document.querySelector('#global-force-english')?.checked && !!document.querySelector('#analysis-group')");
        assert.equal(await evaluate("document.querySelector('#options-tab').textContent"), 'Options');
    }
    summary.stage = 'save'; await clickId('options-tab');
    await change('global-alpha', '0,01'); await wait("document.querySelector('#global-alpha').getAttribute('aria-invalid')==='true'");
    await change('global-alpha', '0.01'); await change('global-seed', '12345'); await change('global-digits', '4'); await change('global-mode', '1', 'change');
    await change('global-trace', false, 'change'); await change('global-upper', false, 'change');
    if (slovak) await change('global-force-english', false, 'change');
    await clickId('global-save');
    if (blocked) {
        await wait("!!document.querySelector('#global-session')");
        assert.equal(await evaluate("document.querySelector('#effective-settings dd').textContent"), '0.05'); await clickId('global-session');
    }
    await wait("document.querySelector('#effective-settings dd').textContent==='0.01'");
    assert.equal(await evaluate("document.querySelector('#preference-state').textContent"), blocked ? 'Session only — settings may be lost when the pane closes' : slovak ? 'Trvalo uložené' : 'Saved persistently');
    if (!blocked) {
        summary.stage = 'reload'; await send('Page.reload', { ignoreCache: true }); await wait("document.querySelector('#global-alpha')?.value==='0.01' && !!document.querySelector('#analysis-group')");
        assert.deepEqual(await evaluate("['global-seed','global-digits','global-mode'].map(id=>document.getElementById(id).value)"), ['12345', '4', '1']);
    }
    summary.stage = 'analysis'; await clickId('analysis-tab'); await change('analysis-group', 'Parametric', 'change'); await change('analysis-method', 'anova.oneway', 'change');
    await wait("!!document.querySelector('#analysis-alpha')"); assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"), '0.01');
    await change('input-reference', "'Input'!A1:C4"); await click('Run analysis'); await wait("document.querySelector('#pilot-status').textContent.includes('analysis.completed')");
    await change('analysis-alpha', '0.125'); await click('Run analysis'); await wait("document.querySelector('#pilot-status').textContent.includes('analysis.completed')");
    await clickId('options-tab'); await change('global-alpha', '0.02'); await clickId('global-save');
    if (blocked) { await wait("!!document.querySelector('#global-session')"); await clickId('global-session'); }
    await wait("document.querySelector('#effective-settings dd').textContent==='0.02'");
    await clickId('analysis-tab'); assert.equal(await evaluate("document.querySelector('#analysis-alpha').value"), '0.125');
    assert.equal(await evaluate("!!document.querySelector('#output-start')"), false); await click('Run analysis'); await wait("document.querySelector('#pilot-status').textContent.includes('analysis.completed')");
    assert.equal(await evaluate('__pilotFake.state.mutations'), 0);
    summary.stage = 'help'; await click(slovak ? 'Pomocník k vybranej metóde' : 'Selected method Help');
    await wait('__helpRequests.length===1'); assert.equal(await evaluate("__helpRequests[0].includes('route=OneWayANOVA')"), true);
    await evaluate('globalThis.__blockHelp=true'); await clickId('options-tab'); await click(slovak ? 'Pomocník ku globálnym nastaveniam' : 'Global settings Help');
    await wait(`document.querySelector('.global-options .help-action [role=status]')?.textContent.includes(${JSON.stringify(slovak ? 'nepodarilo' : 'could not')})`);
    summary.stage = 'keyboard-and-narrow'; await evaluate("document.querySelector('#options-tab').focus()");
    await send('Input.dispatchKeyEvent', { type: 'keyDown', key: 'ArrowRight', code: 'ArrowRight', windowsVirtualKeyCode: 39 });
    await send('Input.dispatchKeyEvent', { type: 'keyUp', key: 'ArrowRight', code: 'ArrowRight', windowsVirtualKeyCode: 39 });
    await wait("document.activeElement.id==='export-tab' && document.querySelector('#export-tab').getAttribute('aria-selected')==='true'");
    await send('Input.dispatchKeyEvent', { type: 'keyDown', key: 'ArrowRight', code: 'ArrowRight', windowsVirtualKeyCode: 39 });
    await send('Input.dispatchKeyEvent', { type: 'keyUp', key: 'ArrowRight', code: 'ArrowRight', windowsVirtualKeyCode: 39 });
    await wait("document.activeElement.id==='diagnostics-tab' && document.querySelector('#diagnostics-tab').getAttribute('aria-selected')==='true'");
    const log = await evaluate("document.querySelector('#sanitized-log').textContent"); assert.equal(log.includes('Input'), false); assert.equal(log.includes('A1:C4'), false); assert.equal(log.includes('diagnostic.output.completed'), false);
    for (const width of [320, 400, 213]) {
        await send('Emulation.setDeviceMetricsOverride', { width, height: 800, deviceScaleFactor: 1, mobile: false });
        assert.equal(await evaluate("document.documentElement.scrollWidth <= innerWidth && document.querySelector('.workspace-tabs').getBoundingClientRect().bottom < innerHeight"), true);
    }
    const shot = await send('Page.captureScreenshot'); writeFileSync(new URL(label + '.png', folder), Buffer.from(shot.data, 'base64'));
    assert.equal(summary.errors, 0); summary.passed = true;
} catch (error) { summary.passed = false; summary.failure = error.code === 'ERR_ASSERTION' ? { actual: error.actual, expected: error.expected, message: error.message } : error.message; process.exitCode = 1; }
finally {
    try { await evaluate("sessionStorage.removeItem('besh.options-test');try{localStorage.removeItem('beshstatng.office.preferences.v1');}catch{}"); } catch {}
    writeFileSync(new URL(label + '.json', folder), JSON.stringify(summary, null, 2)); console.log(JSON.stringify(summary)); await send('Page.close'); socket.close();
}
