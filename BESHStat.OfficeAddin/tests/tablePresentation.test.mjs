import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {tablePresentation} from '../wwwroot/js/tablePresentation.mjs';
import {createProductionWriter} from '../wwwroot/js/productionWriter.mjs';
import {pilotWorkbook} from './helpers/pilotWorkbook.mjs';
const golden=JSON.parse(readFileSync(new URL('./fixtures/table-presentation.json',import.meta.url),'utf8'));
const fixture=JSON.parse(readFileSync(new URL('../../BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json',import.meta.url),'utf8'));

for(const role of ['Title','TopColumnHeader','LeftRowHeader','Body','Footer'])for(const significant of [false,true])
test(`reference profile preserves Common ${role} / significance=${significant} directives`,()=>{
    const roles=['Title','TopColumnHeader','Body','Footer'];
    const cells=roles.map((r,i)=>({row:i,column:0,role:r,strongEmphasis:r==='TopColumnHeader',isSignificant:false}));
    const cell={row:role==='Footer'?3:role==='Title'?0:role==='TopColumnHeader'?1:2,column:0,role,
        strongEmphasis:significant||['TopColumnHeader','LeftRowHeader'].includes(role),isSignificant:significant};
    const style=tablePresentation({rowCount:4,cells},cell);
    assert.equal(style.name,golden.fontName);assert.equal(style.alignment,golden.alignment);
    assert.equal(style.size,role==='Footer'?golden.footerSize:golden.bodySize);
    assert.equal(style.color,significant?golden.significantColor:golden.normalColor);
    assert.equal(style.bold,cell.strongEmphasis);assert.equal(style.fill,role==='TopColumnHeader'?golden.headerFill:null);
    if(role==='Title'&&!significant)assert.equal(style.bold,golden.titleBold);
    assert.equal(style.bottom,role!=='Footer');assert.equal(style.top,cell.row>0);
});
test('production native write/readback reproduces the reference inside exact table regions and leaves spacing gaps untouched',async()=>{
    const x=pilotWorkbook(),writer=createProductionWriter(x.scope),pin=await writer.pinSource(x.state.snapshot());
    const plan=structuredClone(fixture);plan.target.worksheetName='Results';plan.target.destination=1;for(const t of plan.tables)t.autoFit=false;
    const intent={planIdentity:'presentation',targetIdentity:'target',workbookIdentity:pin.sessionIdentity,worksheetIdentity:null,worksheetName:'Results',destination:1,
        startRow:plan.target.startRow,startColumn:plan.target.startColumn,regions:plan.tables.map(t=>({artifactId:t.artifactId,bounds:t.bounds,mayMutate:true})),guards:[],reservations:[]};
    assert.equal((await writer.prepareOutput(plan,intent,pin.sourceToken)).code,'workbook.ok');
    assert.equal((await writer.execute(plan,intent,null)).code,'output.completed');
    const authorized=new Set();
    for(const table of plan.tables)for(const c of table.cells){
        const key=`new-1:${table.bounds.startRow+c.row}:${table.bounds.startColumn+c.column}`;authorized.add(key);
        const style=tablePresentation(table,c),font=x.fonts.get(key),fill=x.fills.get(key);
        assert.deepEqual(font,{bold:style.bold,color:style.color,name:golden.fontName,size:style.size});
        assert.equal(fill.pattern,style.fill?'Solid':'None');assert.equal(x.formats.get(key).horizontalAlignment,golden.alignment);
        assert.equal(x.cells.get(key).format,c.numberFormat);
        if(c.value.kind==='number')assert.equal(x.cells.get(key).value,c.value.numberValue);
    }
    for(const key of x.fonts.keys())assert.equal(authorized.has(key),true);
    for(const key of x.fills.keys())assert.equal(authorized.has(key),true);
    assert.equal([...x.cells.keys()].some(key=>key==='new-1:8:3'),false); // Existing output fixture's spacing gap.
    await writer.dispose();
});
