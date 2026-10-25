import test from 'node:test';
import assert from 'node:assert/strict';
import {createWorksheetDiscovery} from '../wwwroot/js/worksheetDiscovery.mjs';
import {createWorkbookTransport} from '../wwwroot/js/workbookAdapter.mjs';
import {pilotWorkbook} from './helpers/pilotWorkbook.mjs';

function fixture(){
    const f=pilotWorkbook();f.cells.clear();f.sheets.get('sheet-1').name='Hárok 1';
    const events=[];const callback={async invokeMethodAsync(...args){events.push(args);}};
    const api=createWorksheetDiscovery(f.scope);
    return {...f,events,callback,api};
}
async function open(f){const r=await f.api.catalog('owner',f.callback);assert.equal(r.code,'workbook.ok');return r.documentIdentity;}
test('catalog defaults to active worksheet, preserves Unicode/order and includes hidden worksheets',async()=>{
    const f=fixture();f.sheets.set('two',{id:'two',name:'Hárok2',visibility:'Hidden'});f.sheets.set('three',{id:'three',name:"Visit's data",visibility:'VeryHidden'});f.state.sheetId='two';
    const r=await f.api.catalog('owner',f.callback);assert.equal(r.code,'workbook.ok');assert.equal(r.activeWorksheetId,'two');
    assert.deepEqual(r.worksheets.map(s=>s.name),['Hárok 1','Hárok2',"Visit's data"]);assert.equal(r.worksheets[2].visibility,'VeryHidden');assert.equal(f.state.mutations,0);await f.api.dispose();
});
test('catalog/bounds never consult selected range and never load value payloads',async()=>{
    const f=fixture();f.state.setCell(1,1,'y');f.state.setCell(3,6,3);
    const run=f.scope.Excel.run;f.scope.Excel.run=(first,second)=>run(async context=>{context.workbook.getSelectedRange=()=>{throw Error('selection must not determine dataset');};return (second||first)(context);});
    const id=await open(f);const r=await f.api.bounds('owner',id,'sheet-1');assert.equal(r.code,'workbook.ok');
    assert.deepEqual([r.reference.startRow,r.reference.startColumn,r.reference.rowCount,r.reference.columnCount],[1,1,3,6]);
    assert.equal(f.state.loads.some(s=>s.split(',').includes('values')),false);assert.equal(f.state.mutations,0);await f.api.dispose();
});
test('late physical column starts at row one without inventing A-column source',async()=>{
    const f=fixture();f.state.setCell(4,12,3);const id=await open(f);const r=await f.api.bounds('owner',id,'sheet-1');
    assert.equal(r.reference.address,'L1:L4');assert.equal(r.reference.startColumn,12);assert.equal(r.reference.rowCount,4);await f.api.dispose();
});
test('empty sheet has explicit empty reply and no fake range',async()=>{
    const f=fixture();const id=await open(f);const r=await f.api.bounds('owner',id,'sheet-1');assert.equal(r.empty,true);assert.equal(r.reference,undefined);await f.api.dispose();
});
test('formatting-only empty cells do not extend data bounds',async()=>{
    const f=fixture();f.state.setCell(1,1,'y');f.state.setCell(100,200,'','Empty');const id=await open(f);const r=await f.api.bounds('owner',id,'sheet-1');assert.equal(r.reference.address,'A1:A1');await f.api.dispose();
});
test('empty-string formulas outside value-only used bounds are retained via bounded area metadata',async()=>{
    const f=fixture();f.state.ignoreEmptyFormulaValues=true;f.state.setCell(1,1,'y');f.state.setCell(20,5,'','String','=IF(TRUE,"","")');
    const id=await open(f);const r=await f.api.bounds('owner',id,'sheet-1');assert.equal(r.reference.address,'A1:E20');assert.equal(f.state.loads.some(s=>s.includes('values')),false);await f.api.dispose();
});
for(const [row,col,code] of [[20000,1,'input.range-cells'],[2,129,'spec.column-limit'],[1048576,16384,'spec.column-limit']])test(`outlier ${row}/${col} rejects before payload`,async()=>{
    const f=fixture();f.state.setCell(1,1,'y');f.state.setCell(row,col,1);const id=await open(f);const r=await f.api.bounds('owner',id,'sheet-1');assert.equal(r.code,code);assert.equal(f.state.mutations,0);await f.api.dispose();
});
test('too many formula areas reject before loading unbounded geometry',async()=>{
    const f=fixture();for(let i=0;i<65;i++)f.state.setCell(1+i*2,1,'','String','=IF(TRUE,"","")');const id=await open(f);const r=await f.api.bounds('owner',id,'sheet-1');assert.equal(r.code,'input.range-count');await f.api.dispose();
});
test('new worksheet event refresh signal contains only owner and stable code',async()=>{
    const f=fixture();await open(f);f.sheets.set('two',{id:'two',name:'New private name'});f.state.emit('onAdded');
    assert.deepEqual(f.events,[['WorksheetCatalogInvalidated','owner','spec.source-stale']]);assert.equal(JSON.stringify(f.events).includes('private'),false);await f.api.dispose();
});
test('document activation retires session identity even when Office document proxy is reused',async()=>{
    const f=fixture();const old=await open(f);f.state.emit('workbook-activated');assert.equal((await f.api.bounds('owner',old,'sheet-1')).code,'user.document-changed');
    const fresh=await open(f);assert.notEqual(fresh,old);await f.api.dispose();
});
test('rename/delete are detected by ID, not a same-name fallback',async()=>{
    const f=fixture();f.state.setCell(1,1,'y');const id=await open(f);f.sheets.delete('sheet-1');f.sheets.set('replacement',{id:'replacement',name:'Hárok 1'});
    assert.equal((await f.api.bounds('owner',id,'sheet-1')).code,'user.document-changed');await f.api.dispose();
});
test('late callbacks from a stopped owner cannot invalidate the next owner',async()=>{
    const f=fixture();await open(f);const late=f.state.queuedEvent('onChanged');await f.api.stop('owner');
    await f.api.catalog('new-owner',f.callback);late();assert.equal(f.events.length,0);await f.api.dispose();assert.equal(f.state.listenerCount(),0);
});
test('catalog enforces worksheet count before enumerating response',async()=>{
    const f=fixture();for(let i=0;i<256;i++)f.sheets.set('s'+i,{id:'s'+i,name:'Sheet'+i});const r=await f.api.catalog('owner',f.callback);assert.equal(r.code,'spec.sheet-limit');await f.api.dispose();
});
test('missing API is explicit and a later supported call can recover',async()=>{
    const f=fixture();const supported=f.scope.Office.context.requirements.isSetSupported;f.scope.Office.context.requirements.isSetSupported=()=>false;
    assert.equal((await f.api.catalog('owner',f.callback)).code,'office.unsupported-api');f.scope.Office.context.requirements.isSetSupported=supported;assert.equal((await f.api.catalog('owner',f.callback)).code,'workbook.ok');await f.api.dispose();
});
test('native faults are redacted and no exception text reaches reply',async()=>{
    const f=fixture();f.state.hook=async()=>{throw Error('PRIVATE WORKBOOK CONTENT');};const r=await f.api.catalog('owner',f.callback);assert.deepEqual(r,{code:'host.operation-failed'});await f.api.dispose();
});
test('timeout retains native serialization until settlement then allows retry',async()=>{
    const f=fixture();let release;const wait=new Promise(r=>{release=r;});let once=true;f.state.hook=async()=>{if(once){once=false;await wait;}};
    assert.equal((await f.api.catalog('owner',f.callback)).code,'input.time-budget');release();assert.equal((await f.api.catalog('owner',f.callback)).code,'workbook.ok');await f.api.dispose();
});
test('typed workbook transport exposes discovery without enabling a writer',async()=>{
    const f=fixture();const adapter=createWorkbookTransport(f.scope);const r=await adapter.getWorksheets('owner',f.callback);assert.equal(r.code,'workbook.ok');
    assert.equal((await adapter.write({},{})).code,'output.rejected');await adapter.dispose();assert.equal(f.state.mutations,0);
});
test('worksheet transport waits for the existing Office readiness gate before native metadata',async()=>{
    const f=fixture();let release,started;const waiting=new Promise(r=>{release=r;}),entered=new Promise(r=>{started=r;});
    f.scope.Office.onReady=async()=>{started();await waiting;return {host:'Excel',platform:'OfficeOnline'};};
    const adapter=createWorkbookTransport(f.scope),pending=adapter.getWorksheets('owner',f.callback);await entered;
    assert.equal(f.state.syncs,0);assert.equal(f.state.loads.length,0);release();assert.equal((await pending).code,'workbook.ok');await adapter.dispose();
});
test('stop during metadata cannot install a watch or publish a late owner',async()=>{
    const f=fixture();let release,started;const pause=new Promise(r=>{release=r;}),entered=new Promise(r=>{started=r;});let once=true;
    f.state.hook=async()=>{if(once){once=false;started();await pause;}};
    const pending=f.api.catalog('owner',f.callback);await entered;await f.api.stop('owner');release();
    assert.equal((await pending).code,'user.document-changed');assert.equal(f.state.listenerCount(),0);
    assert.equal((await f.api.catalog('next-owner',f.callback)).code,'workbook.ok');await f.api.dispose();assert.equal(f.state.listenerCount(),0);
});
