// Observed Excel Online decimal round-trip, with actual writer/consent/latch behavior.
// This is an Office-boundary simulation; genuine evidence is kept under live-edge.
import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import * as writerModule from '../wwwroot/js/productionWriter.mjs';
import { pilotWorkbook } from './helpers/pilotWorkbook.mjs';
const planFixture = JSON.parse(readFileSync(new URL('../../BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json', import.meta.url), 'utf8'));
const golden = JSON.parse(readFileSync(new URL('../../BESHStat.Excel.Common.Tests/TestData/Stage304/anova-welch-tukey.json', import.meta.url), 'utf8'));
const number = n => ({kind:'number',numberValue:n,textValue:null,booleanValue:null});

for (const [scenario, expected, actual, platform, completed] of [
    ['Welch F web round-trip',golden.metrics.WelchFStatistic,Number(golden.metrics.WelchFStatistic.toPrecision(15)),'OfficeOnline',true],
    ['Welch p web round-trip',golden.metrics.WelchPValue,Number(golden.metrics.WelchPValue.toPrecision(15)),'OfficeOnline',true],
    ['other nearby value',golden.metrics.WelchPValue,golden.metrics.WelchPValue + Number.EPSILON * golden.metrics.WelchPValue,'OfficeOnline',false],
    ['changed rounded value',golden.metrics.WelchFStatistic,11.1428571428572,'OfficeOnline',false],
    ['numeric text',golden.metrics.WelchFStatistic,String(golden.metrics.WelchFStatistic),'OfficeOnline',false],
    ['integer precision loss',1234567890123456,Number((1234567890123456).toPrecision(15)),'OfficeOnline',false],
    ['underflow to zero',Number.MIN_VALUE,0,'OfficeOnline',false],
    ['nonfinite readback',golden.metrics.WelchPValue,Infinity,'OfficeOnline',false],
    ['desktop unverified rounding',golden.metrics.WelchFStatistic,Number(golden.metrics.WelchFStatistic.toPrecision(15)),'PC',false]
]) test(`production numeric readback: ${scenario}`, async () => {
    const x=pilotWorkbook();x.scope.Office.context.platform=platform;
    const writer=writerModule.createProductionWriter(x.scope),pin=await writer.pinSource(x.state.snapshot());
    const plan=structuredClone(planFixture);plan.target.destination=0;plan.target.worksheetName='Input';
    for(const t of plan.tables)t.autoFit=false;
    plan.tables[0].cells[0].value=number(expected);
    const intent={planIdentity:'numeric-readback',targetIdentity:'target',workbookIdentity:pin.sessionIdentity,worksheetIdentity:'sheet-1',worksheetName:'Input',destination:0,
        startRow:plan.target.startRow,startColumn:plan.target.startColumn,regions:plan.tables.map(t=>({artifactId:t.artifactId,bounds:t.bounds,mayMutate:true})),guards:[],reservations:[]};
    const inspection=await writer.prepareOutput(plan,intent,pin.sourceToken);assert.equal(inspection.state,0);
    const at=x.state.syncs+5;let observed=false;
    x.state.hook=async n=>{if(n===at){const t=plan.tables[0],cell=x.cells.get(`sheet-1:${t.bounds.startRow}:${t.bounds.startColumn}`);observed=!!cell;cell.value=actual;}};
    const result=await writer.execute(plan,intent,null);
    assert.equal(observed,true);assert.equal(result.code,completed?'output.completed':'output.mutation-uncertain');
    assert.equal(plan.tables[0].cells[0].value.numberValue,expected,'The wire value must remain unchanged.');
    if(!completed){assert.equal(result.mutationMayHaveOccurred,true);assert.equal((await writer.captureSelection(false)).code,'output.mutation-uncertain');}
    await writer.dispose();
});

test('native number comparison accepts one canonical web representation, not an epsilon tolerance',()=>{
    const match=writerModule.matchesNativeOutputValue;
    for(const n of [0,-0,1,-42,1e-20,1e20,Number.MIN_VALUE,Number.MAX_VALUE])assert.equal(match(n,number(n),false),true);
    for(const n of [golden.metrics.WelchFStatistic,golden.metrics.WelchPValue,-golden.metrics.WelchPValue,1.234567890123456e-200]){
        assert.equal(match(Number(n.toPrecision(15)),number(n),true),true);
        assert.equal(match(Number(n.toPrecision(15)),number(n),false),false);
    }
    for(const actual of [null,undefined,'0.1',true,NaN,Infinity,-Infinity])assert.equal(match(actual,number(0.1),true),false);
    assert.equal(match(0,number(1e-200),true),false);
    assert.equal(match(-0.1,number(0.1),true),false);
    for(const v of [{kind:'text',textValue:'literal'}, {kind:'boolean',booleanValue:true}, {kind:'blank'}]){
        const expected=v.kind==='text'?'literal':v.kind==='boolean'?true:'';
        assert.equal(match(expected,v,true),true);assert.equal(match(null,v,true),false);
    }
});

test('decimal-equivalent source edits still fail strict physical-source continuity',async()=>{
    const x=pilotWorkbook();x.scope.Office.context.platform='OfficeOnline';
    const writer=writerModule.createProductionWriter(x.scope),pin=await writer.pinSource(x.state.snapshot());
    const original=x.cells.get('sheet-1:2:1').value,changed=original+Number.EPSILON*original;
    assert.notEqual(original,changed);assert.equal(original.toPrecision(15),changed.toPrecision(15));
    x.state.setCell(2,1,changed);
    assert.equal((await writer.checkSource(pin.sourceToken)).code,'user.document-changed');
    assert.equal(x.state.mutations,0);await writer.dispose();
});

test('decimal-equivalent target edits cannot reuse occupied overwrite consent',async()=>{
    const x=pilotWorkbook();x.scope.Office.context.platform='OfficeOnline';
    const writer=writerModule.createProductionWriter(x.scope),pin=await writer.pinSource(x.state.snapshot());
    const plan=structuredClone(planFixture);plan.target.destination=0;plan.target.worksheetName='Input';for(const t of plan.tables)t.autoFit=false;
    const intent={planIdentity:'strict-consent',targetIdentity:'target',workbookIdentity:pin.sessionIdentity,worksheetIdentity:'sheet-1',worksheetName:'Input',destination:0,
        startRow:plan.target.startRow,startColumn:plan.target.startColumn,regions:plan.tables.map(t=>({artifactId:t.artifactId,bounds:t.bounds,mayMutate:true})),guards:[],reservations:[]};
    const bounds=plan.tables[0].bounds,original=golden.metrics.WelchPValue,changed=original+Number.EPSILON*original;
    assert.notEqual(original,changed);assert.equal(original.toPrecision(15),changed.toPrecision(15));
    x.state.setCell(bounds.startRow,bounds.startColumn,original);
    const inspection=await writer.prepareOutput(plan,intent,pin.sourceToken);assert.equal(inspection.state,1);
    x.state.setCell(bounds.startRow,bounds.startColumn,changed);
    const result=await writer.execute(plan,intent,{inspection,approved:true});
    assert.equal(result.code,'output.consent-stale');assert.equal(result.mutationMayHaveOccurred,false);assert.equal(x.state.mutations,0);
    await writer.dispose();
});
