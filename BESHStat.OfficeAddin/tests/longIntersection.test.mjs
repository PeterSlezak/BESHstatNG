import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {createProductionWriter} from '../wwwroot/js/productionWriter.mjs';
import {pilotWorkbook} from './helpers/pilotWorkbook.mjs';
const fixture=JSON.parse(readFileSync(new URL('../../BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json',import.meta.url),'utf8'));
async function setup(start=1){
    const x=pilotWorkbook(),writer=createProductionWriter(x.scope),sources=[];
    x.state.setCell(1,6,'Group');x.state.setCell(1,5,'Value');
    for(let r=2;r<=10;r++){x.state.setCell(r,6,`Group ${Math.floor((r-2)/3)}`);x.state.setCell(r,5,r);}
    for(const [col,row,rows] of [[6,start,11-start],[5,1,12]]){
        x.state.selected={row,col,rows,cols:1};const pick=await writer.captureSelection(false);assert.equal(pick.code,'workbook.ok');
        sources.push({snapshot:x.state.snapshot(),handle:pick.handle});
    }
    const pin=await writer.pinSources(sources);assert.equal(pin.code,'workbook.ok');
    const plan=structuredClone(fixture);plan.target.destination=1;plan.target.worksheetName='Results';for(const t of plan.tables)t.autoFit=false;
    const intent={planIdentity:'intersection',targetIdentity:'target',workbookIdentity:pin.sessionIdentity,worksheetIdentity:null,worksheetName:'Results',destination:1,
        startRow:plan.target.startRow,startColumn:plan.target.startColumn,regions:plan.tables.map(t=>({artifactId:t.artifactId,bounds:t.bounds,mayMutate:true})),guards:[],reservations:[]};
    return {...x,writer,sources,pin,plan,intent};
}
for(const start of [1,2,3])test(`native composite pins original unequal intervals with category start ${start}`,async()=>{
    const x=await setup(start);assert.deepEqual(x.sources.map(s=>s.snapshot.rowCount),[11-start,12]);
    assert.equal((await x.writer.checkSource(x.pin.sourceToken)).code,'workbook.ok');
    assert.equal((await x.writer.prepareOutput(x.plan,x.intent,x.pin.sourceToken)).code,'workbook.ok');
    assert.equal((await x.writer.execute(x.plan,x.intent,null)).code,'output.completed');await x.writer.dispose();
});
for(const phase of ['before-inspection','before-write'])test(`ignored overhang edit ${phase} invalidates the whole run without mutation`,async()=>{
    const x=await setup();if(phase==='before-write')await x.writer.prepareOutput(x.plan,x.intent,x.pin.sourceToken);
    x.state.setCell(12,5,0); // An originally blank cell excluded from logical matching remains a pinned physical source cell.
    const reply=phase==='before-write'?await x.writer.execute(x.plan,x.intent,null):await x.writer.prepareOutput(x.plan,x.intent,x.pin.sourceToken);
    assert.notEqual(reply.code,'workbook.ok');assert.notEqual(reply.code,'output.completed');assert.equal(x.state.mutations,0);await x.writer.dispose();
});
test('whole-column original footprint blocks output outside its effective used span',async()=>{
    const x=pilotWorkbook(),writer=createProductionWriter(x.scope);x.state.setCell(1,5,'H');x.state.setCell(10,5,1);
    x.state.selected={row:1,col:5,rows:1048576,cols:1};const pick=await writer.captureSelection(false);
    x.state.selected={row:1,col:5,rows:10,cols:1};const snapshot=x.state.snapshot();
    const pin=await writer.pinSource(snapshot,null,pick.handle);assert.equal(pin.code,'workbook.ok');
    const plan=structuredClone(fixture);plan.target.worksheetName='Input';plan.target.startRow+=100;plan.target.startColumn+=2;
    for(const table of plan.tables){table.autoFit=false;table.bounds.startRow+=100;table.bounds.endRow+=100;table.bounds.startColumn+=2;table.bounds.endColumn+=2;}
    const intent={planIdentity:'overlap',targetIdentity:'target',workbookIdentity:pin.sessionIdentity,worksheetIdentity:'sheet-1',worksheetName:'Input',destination:0,
        startRow:plan.target.startRow,startColumn:plan.target.startColumn,regions:plan.tables.map(t=>({artifactId:t.artifactId,bounds:t.bounds,mayMutate:true})),guards:[],reservations:[]};
    assert.equal((await writer.prepareOutput(plan,intent,pin.sourceToken)).code,'output.target-invalid');assert.equal(x.state.mutations,0);await writer.dispose();
});
