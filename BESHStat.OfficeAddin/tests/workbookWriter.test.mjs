import test from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { createWorkbookTransport } from "../wwwroot/js/workbookAdapter.mjs";
import { fakeWorkbook } from "./helpers/workbook.mjs";

const fixture = JSON.parse(readFileSync(new URL("../../BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json", import.meta.url), "utf8"));
function setup(options = {}) {
    const {scope,state} = fakeWorkbook();
    const plan=structuredClone(fixture);
    state.sheet.name=plan.target.worksheetName;
    state.workbookIdentity="fixture-book";
    const intent={planIdentity:"fixture-plan",targetIdentity:"fixture-target",workbookIdentity:"fixture-book",worksheetIdentity:state.sheet.id,
        worksheetName:state.sheet.name,destination:0,startRow:plan.target.startRow,startColumn:plan.target.startColumn,
        regions:plan.tables.map(table=>({artifactId:table.artifactId,bounds:table.bounds,mayMutate:true})),reservations:[],guards:[]};
    let inspections=0;
    const observation=()=>({intent,state:options.occupied?1:0,observationIdentity:`fixture-${state.selected.row}-${state.selected.col}-${state.selected.rows}-${state.selected.cols}`,
        occupiedRegionIds:options.occupied?[intent.regions[0].artifactId]:[],hasValues:!!options.occupied,hasFormulas:false});
    const writer={async inspect(context){
        inspections++;
        const sheet=context.workbook.worksheets.getItem(intent.worksheetIdentity);
        sheet.load("id,name"); await context.sync();
        const result=observation();
        if (options.reject || intent.workbookIdentity!==state.workbookIdentity || sheet.id!==intent.worksheetIdentity || sheet.name!==intent.worksheetName) result.state=3;
        if (options.stale && inspections===2) result.observationIdentity="changed";
        if (options.changeDocument && inspections===2) state.document={};
        return result;
    },async commit(){state.mutations++; if(options.throwCommit)throw new Error("SECRET");}};
    const adapter=createWorkbookTransport(scope,{writer:options.production?null:writer});
    const consent=options.occupied?{inspection:observation(),approved:true}:null;
    return {adapter,scope,state,plan,intent,consent};
}

test("production writer remains fail closed despite valid plan and permissions",async()=>{
    const {adapter,state,plan,intent}=setup({production:true});
    const result=await adapter.write(plan,intent);
    assert.equal(result.state,2);assert.equal(result.code,"output.rejected");assert.equal(state.mutations,0);assert.equal(state.runs,0);
    intent.destination=2;
    assert.equal((await adapter.write(plan,intent)).code,"output.destination-unsupported");
});
for(const reason of ["protected","merged","collision-unverified","occupancy-unknown"])
test(`mock inspection ${reason} cannot mutate`,async()=>{
    const {adapter,state,plan,intent}=setup({reject:true});
    const result=await adapter.write(plan,intent);
    assert.equal(result.code,"output.occupancy-unverified"); assert.equal(result.state,2);assert.equal(state.mutations,0);
});
test("occupied output requires explicit consent",async()=>{
    const {adapter,state,plan,intent}=setup({occupied:true});
    assert.equal((await adapter.write(plan,intent)).code,"output.confirmation-required"); assert.equal(state.mutations,0);
});
test("declined occupied output returns CancelledByUser with no mutation",async()=>{
    const {adapter,state,plan,intent,consent}=setup({occupied:true});consent.approved=false;
    const result=await adapter.write(plan,intent,consent);
    assert.equal(result.state,1);assert.equal(result.code,"output.cancelled");assert.equal(state.mutations,0);
});
for(const stale of ["token","reinspection"])
test(`stale consent ${stale} cannot mutate`,async()=>{
    const {adapter,state,plan,intent,consent}=setup({occupied:true,stale:stale==="reinspection"});
    if(stale==="token")consent.inspection.observationIdentity="old";
    assert.equal((await adapter.write(plan,intent,consent)).code,"output.consent-stale");assert.equal(state.mutations,0);
});
for(const field of ["workbookIdentity","worksheetIdentity","destination","worksheetName","startRow"])
test(`mismatched or missing write binding ${field} cannot mutate`,async()=>{
    const {adapter,state,plan,intent}=setup();
    intent[field]=field==="destination"?2:field==="startRow"?10:"";
    assert.equal((await adapter.write(plan,intent)).state,2);assert.equal(state.mutations,0);
});
test("document change between inspection and mutation is rejected",async()=>{
    const {adapter,state,plan,intent}=setup({changeDocument:true});
    assert.equal((await adapter.write(plan,intent)).state,2);assert.equal(state.mutations,0);
});
test("mismatched actual mock workbook identity cannot mutate",async()=>{
    const {adapter,state,plan,intent}=setup();intent.workbookIdentity="other-workbook";
    assert.equal((await adapter.write(plan,intent)).state,2);assert.equal(state.mutations,0);
});
test("stale selection observation cannot authorize simulated mutation",async()=>{
    const {adapter,state,plan,intent}=setup();state.hook=async n=>{if(n===2)state.selected.col++;};
    assert.equal((await adapter.write(plan,intent)).code,"output.consent-stale");assert.equal(state.mutations,0);
});
test("completed simulated write reports confirmed regions",async()=>{
    const {adapter,state,plan,intent,consent}=setup({occupied:true});
    const result=await adapter.write(plan,intent,consent);
    assert.equal(result.state,0);assert.equal(result.code,"output.completed");assert.equal(result.mutationMayHaveOccurred,true);
    assert.deepEqual(result.confirmedWrittenIds,intent.regions.map(r=>r.artifactId));assert.equal(state.mutations,1);
});
for(const failure of ["commit","sync"])
test(`post-mutation ${failure} failure is uncertain with no rollback claim`,async()=>{
    const {adapter,state,plan,intent}=setup({throwCommit:failure==="commit"});
    if(failure==="sync")state.hook=async n=>{if(n===3)throw new Error("SECRET");};
    const result=await adapter.write(plan,intent);
    assert.equal(result.state,4);assert.equal(result.code,"output.mutation-uncertain");assert.equal(result.mutationMayHaveOccurred,true);
    assert.equal(state.mutations,1);assert.equal(JSON.stringify(result).includes("SECRET"),false);
});
test("cancellation before commit rejects, observed success wins over late cancellation",async()=>{
    const first=setup();first.state.hook=async n=>{if(n===2)first.adapter.cancel("test-write");};
    assert.equal((await first.adapter.write(first.plan,first.intent)).state,2);assert.equal(first.state.mutations,0);
    const late=setup();late.state.hook=async n=>{if(n===3)late.adapter.cancel("test-write");};
    assert.equal((await late.adapter.write(late.plan,late.intent)).state,0);assert.equal(late.state.mutations,1);
});
