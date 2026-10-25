import test from 'node:test';
import assert from 'node:assert/strict';
import { createProductionWriter } from '../wwwroot/js/productionWriter.mjs';
import { createWorkbookTransport } from '../wwwroot/js/workbookAdapter.mjs';
import { pilotWorkbook } from './helpers/pilotWorkbook.mjs';

function setup() {
    const x = pilotWorkbook(), writer = createProductionWriter(x.scope), transport = createWorkbookTransport(x.scope, {writer});
    return {...x, writer, transport};
}
const ref = (column = 5, width = 1) => ({ worksheetId:'sheet-1', worksheetName:'Input', workbookId:'', workbookName:'', addressDialect:1,
    startRow:1, startColumn:column, rowCount:1048576, columnCount:width,
    address:`${String.fromCharCode(64 + column)}1:${String.fromCharCode(63 + column + width)}1048576` });
async function read(x, reference, id = 'read') {
    const begin = await x.transport.beginRead(id, undefined, reference, false); assert.equal(begin.code, 'workbook.ok');
    const rows = []; for (;;) { const tile = await x.transport.readNext(id); assert.equal(tile.code,'workbook.ok'); rows.push(...tile.rows); if(tile.done)break; }
    assert.equal((await x.transport.finishRead(id)).code,'workbook.ok');
    return {...begin.metadata, rows};
}
for(const mode of ['manual','capture']) test(`whole-column ${mode} normalizes metadata, preserves zeros/interior blanks and does not load a million rows`, async()=>{
    const x=setup();x.state.setCell(7,5,'Header');x.state.setCell(9,5,0);x.state.setCell(12,5,'#N/A','Error');
    x.state.selected={row:1,col:5,rows:1048576,cols:1};
    const pick=mode==='manual'?await x.transport.resolveReference(ref()):await x.transport.captureSelection(false);
    assert.equal(pick.code,'workbook.ok');assert.deepEqual([pick.reference.startRow,pick.reference.rowCount],[7,6]);
    assert.equal(pick.originalReference.rowCount,1048576);assert.equal(x.state.loads.some(s=>/values|formulas/.test(s)),false);
    const snapshot=await read(x,pick.reference);
    assert.equal(snapshot.rows.length,6);assert.equal(snapshot.rows[1][0].kind,0);assert.equal(snapshot.rows[2][0].numberValue,0);
    assert.equal(snapshot.rows[5][0].kind,4);
    const pin=await x.transport.pinSource(snapshot,null,pick.handle);assert.equal(pin.code,'workbook.ok');
    assert.equal((await x.transport.checkSource(pin.sourceToken)).code,'workbook.ok');assert.equal(x.state.mutations,0);await x.transport.dispose();
});
test('whole multicolumn scope retains all selected columns, including empty ones',async()=>{
    const x=setup();x.state.setCell(4,5,'E');x.state.setCell(8,7,7);
    const pick=await x.transport.resolveReference(ref(5,3));assert.equal(pick.code,'workbook.ok');
    assert.deepEqual([pick.reference.startRow,pick.reference.rowCount,pick.reference.startColumn,pick.reference.columnCount],[4,5,5,3]);
    const data=await read(x,pick.reference);assert.equal(data.rows[2][1].kind,0);assert.equal(data.rows.length,5);await x.transport.dispose();
});
for(const defect of ['empty','distant','multi-over-budget','whole-sheet','row-only','multi-area'])test(`whole-column ${defect} fails before payload or mutation`,async()=>{
    const x=setup();let reference=ref();
    if(defect==='distant'){x.state.setCell(1,5,'H');x.state.setCell(10001,5,1);}
    if(defect==='multi-over-budget'){reference=ref(5,3);x.state.setCell(1,5,'H');x.state.setCell(4000,7,1);}
    if(defect==='whole-sheet')x.state.selected={row:1,col:1,rows:1048576,cols:16384};
    if(defect==='row-only')x.state.selected={row:4,col:1,rows:1,cols:16384};
    if(defect==='multi-area')x.state.multiple=true;
    const reply=['whole-sheet','row-only','multi-area'].includes(defect)?await x.transport.captureSelection(false):await x.transport.resolveReference(reference);
    assert.notEqual(reply.code,'workbook.ok');assert.equal(x.state.loads.some(s=>/values|formulas/.test(s)),false);assert.equal(x.state.mutations,0);await x.transport.dispose();
});
for(const when of ['before-pin','after-pin'])for(const change of ['extend','leading','within','document','rename','delete'])test(`${when}: full-column ${change} cannot escape source verification`,async()=>{
    const x=setup();x.state.setCell(7,5,'H');x.state.setCell(9,5,3);
    const pick=await x.transport.resolveReference(ref()),snapshot=await read(x,pick.reference);
    let pin=when==='after-pin'?await x.transport.pinSource(snapshot,null,pick.handle):null;
    if(change==='extend')x.state.setCell(100,5,9);
    if(change==='leading')x.state.setCell(2,5,'new');
    if(change==='within')x.state.setCell(8,5,8);
    if(change==='document')x.state.document={mode:'readWrite'};
    if(change==='rename')x.sheets.get('sheet-1').name='Replacement';
    if(change==='delete')x.sheets.delete('sheet-1');
    const reply=pin?await x.transport.checkSource(pin.sourceToken):await x.transport.pinSource(snapshot,null,pick.handle);
    assert.notEqual(reply.code,'workbook.ok');assert.equal(x.state.mutations,0);await x.transport.dispose();
});
test('two whole columns retain independent original scopes and detect an edit outside the old used span without an event',async()=>{
    const x=setup(),sources=[];
    for(const col of [5,10]){
        x.state.setCell(1,col,'Header');for(let r=2;r<=10;r++)x.state.setCell(r,col,col===5?'Group':r);
        const pick=await x.transport.resolveReference(ref(col));sources.push({snapshot:await read(x,pick.reference,`read-${col}`),handle:pick.handle});
    }
    const pin=await x.transport.pinSources(sources);assert.equal(pin.code,'workbook.ok');
    x.state.setCell(90,10,9);assert.equal((await x.transport.checkSource(pin.sourceToken)).code,'user.document-changed');
    assert.equal(x.state.mutations,0);await x.transport.dispose();
});
test('empty-string formula results are text, formatted Empty cells are missing, and no interior rows compact',async()=>{
    const x=setup();x.state.setCell(1,5,'H');x.state.setCell(3,5,'','String','=IF(TRUE,"",1)');x.state.setCell(4,5,2);
    x.cells.set('sheet-1:8:5',{value:'',type:'Empty',formula:'',format:'0.000'});
    const pick=await x.transport.resolveReference(ref());assert.equal(pick.reference.rowCount,4);
    const data=await read(x,pick.reference);assert.deepEqual(data.rows.map(r=>r[0].kind),[2,0,2,1]);await x.transport.dispose();
});
for(const omitted of [false,true])test(`formula metadata retains leading/trailing empty-string results (values-only omits=${omitted})`,async()=>{
    const x=setup();x.state.ignoreEmptyFormulaValues=omitted;
    x.state.setCell(4,5,'','String','=IF(TRUE,"",1)');x.state.setCell(7,5,0);x.state.setCell(10,5,'','String','=IF(TRUE,"",1)');
    const pick=await x.transport.resolveReference(ref());assert.equal(pick.code,'workbook.ok');
    assert.deepEqual([pick.reference.startRow,pick.reference.rowCount],[4,7]);
    const snapshot=await read(x,pick.reference);assert.equal(snapshot.rows[0][0].kind,2);assert.equal(snapshot.rows[6][0].kind,2);assert.equal(snapshot.rows[3][0].numberValue,0);
    await x.transport.dispose();
});
test('a column containing only an empty-string formula is not silently treated as empty',async()=>{
    const x=setup();x.state.ignoreEmptyFormulaValues=true;x.state.setCell(20,5,'','String','=IF(TRUE,"",1)');
    const pick=await x.transport.resolveReference(ref());assert.equal(pick.code,'workbook.ok');assert.equal(pick.reference.startRow,20);assert.equal(pick.reference.rowCount,1);
    const snapshot=await read(x,pick.reference);assert.equal(snapshot.rows[0][0].kind,2);await x.transport.dispose();
});
test('excessive formula-area metadata fails closed before loading cell matrices',async()=>{
    const x=setup();x.state.ignoreEmptyFormulaValues=true;
    for(let i=0;i<65;i++)x.state.setCell(i*2+1,5,'','String','=IF(TRUE,"",1)');
    assert.equal((await x.transport.resolveReference(ref())).code,'input.range-count');assert.equal(x.state.loads.some(s=>/values|formulas/.test(s)),false);
    assert.equal(x.state.mutations,0);await x.transport.dispose();
});
test('a later empty-string formula outside the old footprint invalidates a pinned whole column without an event',async()=>{
    const x=setup();x.state.ignoreEmptyFormulaValues=true;x.state.setCell(1,5,'H');x.state.setCell(3,5,3);
    const pick=await x.transport.resolveReference(ref()),snapshot=await read(x,pick.reference);
    const pin=await x.transport.pinSource(snapshot,null,pick.handle);assert.equal(pin.code,'workbook.ok');
    x.state.setCell(100,5,'','String','=IF(TRUE,"",1)');
    assert.equal((await x.transport.checkSource(pin.sourceToken)).code,'user.document-changed');assert.equal(x.state.mutations,0);await x.transport.dispose();
});
test('explicit Run reacquisition refreshes changed whole-column bounds without rebinding a replaced physical sheet',async()=>{
    const x=setup();x.state.setCell(1,5,'H');x.state.setCell(3,5,3);
    const pick=await x.transport.resolveReference(ref()),snapshot=await read(x,pick.reference);
    const pin=await x.transport.pinSource(snapshot,null,pick.handle);assert.equal(pin.code,'workbook.ok');
    x.state.setCell(8,5,8);x.state.emit('onChanged');
    assert.equal((await x.transport.checkSource(pin.sourceToken)).code,'user.document-changed');
    await x.transport.invalidate();
    const fresh=await x.transport.resolveReference(ref(),pick.handle);
    assert.equal(fresh.code,'workbook.ok');assert.equal(fresh.reference.rowCount,8);
    assert.equal((await x.transport.pinSource(await read(x,fresh.reference),null,fresh.handle)).code,'workbook.ok');
    x.sheets.delete('sheet-1');x.sheets.set('replacement',{id:'replacement',name:'Input'});
    assert.equal((await x.transport.resolveReference(ref(),fresh.handle)).code,'user.document-changed');
    assert.equal(x.state.mutations,0);await x.transport.dispose();
});
