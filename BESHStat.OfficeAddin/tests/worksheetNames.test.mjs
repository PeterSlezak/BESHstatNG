import test from "node:test";
import assert from "node:assert/strict";
import { allocateWorksheetName, validWorksheetName } from "../wwwroot/js/worksheetNames.mjs";
import { createProductionWriter, validateProductionPlan, enableWriterTrace, getWriterTrace } from "../wwwroot/js/productionWriter.mjs";
import { createWorkbookTransport } from "../wwwroot/js/workbookAdapter.mjs";
import { pilotWorkbook } from "./helpers/pilotWorkbook.mjs";
import { readFileSync } from "node:fs";
const fixture = JSON.parse(readFileSync(new URL("../../BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json", import.meta.url), "utf8"));

for (const [names, resolved] of [ [[], "ANOVAresults"], [["ANOVAresults"], "ANOVAresults1"],
    [["ANOVAresults", "ANOVAresults1"], "ANOVAresults2"],
    [["ANOVAresults", "ANOVAresults1", "ANOVAresults3"], "ANOVAresults2"], [["anovaRESULTS"], "ANOVAresults1"] ])
test(`lowest free suffix: ${names.join(",") || "free"}`, () => {
    assert.deepEqual(allocateWorksheetName("ANOVAresults", names), {requestedName:"ANOVAresults", resolvedName:resolved,
        warningCode: resolved === "ANOVAresults" ? null : "output.sheet-name-adjusted"});
});
for (const name of ["Hárok", "O'Brien", "Measurements 2026", "📊 Study"])
test(`valid Unicode/internal apostrophe spelling retained: ${name}`, () => {
    assert.equal(validWorksheetName(name), true); assert.equal(allocateWorksheetName(name, []).resolvedName, name);
    assert.equal(allocateWorksheetName(name, [name]).resolvedName, name + "1");
});
for (const name of ["", " ", "History", "history", "bad:name", "bad\\name", "bad/name", "bad?name", "bad*name", "[name]", "'name", "name'", "x".repeat(32), "bad\nname", "bad\ud800"])
test(`invalid name cannot be repaired by suffixing: ${JSON.stringify(name)}`, () => {
    assert.equal(validWorksheetName(name), false);
    assert.throws(() => allocateWorksheetName(name, []), e => e.safeCode === "output.target-invalid");
});
test("31-character base, suffix 9 to 10 and truncated-prefix collisions reuse the first gap", () => {
    const base = "x".repeat(31), names = [base, ...Array.from({length:9},(_,i)=>base.slice(0,30)+(i+1))];
    assert.equal(allocateWorksheetName(base,names).resolvedName,base.slice(0,29)+"10");
    assert.equal(allocateWorksheetName(base.slice(0,30)+"y",[base.slice(0,30)+"y",base.slice(0,30)+"1"]).resolvedName,base.slice(0,30)+"2");
    const unicode="x".repeat(28)+"📊z";
    assert.equal(validWorksheetName(allocateWorksheetName(unicode,[unicode,...Array.from({length:9},(_,i)=>unicode.slice(0,30)+(i+1))]).resolvedName),true);
});
test("search and workbook inventory are bounded and ambiguity fails closed", () => {
    for(const names of [["a","a1","a2"],["a","A"],["bad/name"]])
        assert.throws(()=>allocateWorksheetName("a",names,2),e=>e.safeCode==="output.sheet-name-unavailable");
    assert.throws(()=>allocateWorksheetName("a",Array(10001).fill("b")),e=>e.safeCode==="output.sheet-name-unavailable");
});

async function setup() {
    const fake=pilotWorkbook(), writer=createProductionWriter(fake.scope);
    const transport=createWorkbookTransport(fake.scope,{writer});
    async function pin(){return transport.pinSource(fake.state.snapshot());}
    function planFor(name,pinned){
        const plan=structuredClone(fixture); plan.target.destination=1;plan.target.worksheetName=name;
        plan.tables.forEach(t=>t.autoFit=false);
        const intent={planIdentity:fake.scope.crypto.randomUUID(),targetIdentity:fake.scope.crypto.randomUUID(),workbookIdentity:pinned.sessionIdentity,
            worksheetIdentity:null,worksheetName:name,destination:1,startRow:plan.target.startRow,startColumn:plan.target.startColumn,
            regions:plan.tables.map(t=>({artifactId:t.artifactId,bounds:t.bounds,mayMutate:true})),guards:[],reservations:[]};
        validateProductionPlan(plan,intent); return {plan,intent};
    }
    return {...fake,writer,transport,pin,planFor};
}
for(const existing of [[],["ANOVAresults"],["ANOVAresults","ANOVAresults1","ANOVAresults3"],["anovaRESULTS"]])
test(`real boundary allocation, exact plan and formatted one-sheet write: ${existing.join(",")||"free"}`,async()=>{
    const x=await setup();existing.forEach((name,i)=>x.sheets.set(`existing-${i}`,{id:`existing-${i}`,name}));
    const pin=await x.pin(); const expected=allocateWorksheetName("ANOVAresults",["Input",...existing]);
    enableWriterTrace();
    const resolution=await x.transport.resolveNewWorksheetName("ANOVAresults",pin.sourceToken);
    assert.equal(resolution.code,"workbook.ok");assert.equal(resolution.resolvedName,expected.resolvedName);
    assert.equal(resolution.warningCode,expected.warningCode);assert.equal(x.state.added,0);assert.equal(x.state.mutations,0);
    const {plan,intent}=x.planFor(resolution.resolvedName,pin);
    const inspected=await x.transport.prepareOutput(plan,intent,pin.sourceToken);
    assert.equal(inspected.state,0);assert.deepEqual(inspected.occupiedRegionIds,[]);assert.equal(x.state.added,0);
    const result=await x.transport.write(plan,intent,null);
    assert.equal(result.code,"output.completed");assert.equal(result.confirmationRequired,false);assert.equal(x.state.added,1);
    assert.equal(x.sheets.get("new-1").name,resolution.resolvedName);
    assert.deepEqual(result.confirmedWrittenIds,plan.tables.map(t=>t.artifactId));
    for(const table of plan.tables)for(const c of table.cells){
        const k=`new-1:${table.bounds.startRow+c.row}:${table.bounds.startColumn+c.column}`;
        assert.equal(x.cells.get(k).value,c.value.kind==="number"?c.value.numberValue:c.value.kind==="text"?c.value.textValue:c.value.kind==="boolean"?c.value.booleanValue:"");
        assert.equal(x.cells.get(k).format,c.numberFormat);assert.equal(x.fonts.get(k).name,"Calibri Light");
    }
    const trace=JSON.stringify(getWriterTrace());assert.equal(trace.includes("ANOVAresults"),false);
    if(expected.warningCode)assert.equal(trace.includes("output.sheet-name-adjusted"),true);
    enableWriterTrace(false);await x.transport.dispose();
});
test("successive independent runs allocate unchanged base, base1, base2",async()=>{
    const x=await setup(), names=[];
    for(let i=0;i<3;i++){
        const pin=await x.pin();const allocation=await x.transport.resolveNewWorksheetName("ANOVAresults",pin.sourceToken);
        names.push(allocation.resolvedName);const {plan,intent}=x.planFor(allocation.resolvedName,pin);
        assert.equal((await x.transport.prepareOutput(plan,intent,pin.sourceToken)).code,"workbook.ok");
        assert.equal((await x.transport.write(plan,intent,null)).code,"output.completed");
    }
    assert.deepEqual(names,["ANOVAresults","ANOVAresults1","ANOVAresults2"]);assert.equal(x.state.added,3);await x.transport.dispose();
});
for(const when of ["before-inspection","before-execute"])
test(`concurrent collision ${when} is safe without a native mutation`,async()=>{
    const x=await setup(),pin=await x.pin();const allocation=await x.transport.resolveNewWorksheetName("ANOVAresults",pin.sourceToken);
    const {plan,intent}=x.planFor(allocation.resolvedName,pin);
    if(when==="before-execute")assert.equal((await x.transport.prepareOutput(plan,intent,pin.sourceToken)).code,"workbook.ok");
    x.sheets.set("foreign",{id:"foreign",name:allocation.resolvedName});
    const rejected=when==="before-inspection"?await x.transport.prepareOutput(plan,intent,pin.sourceToken):await x.transport.write(plan,intent,null);
    assert.equal(rejected.code,"output.sheet-name-collision");assert.equal(x.state.mutations,0);assert.equal(x.state.added,0);
    const fresh=await x.transport.resolveNewWorksheetName("ANOVAresults",pin.sourceToken);assert.equal(fresh.resolvedName,"ANOVAresults1");
    await x.transport.dispose();
});
test("post-submission conflict remains uncertain and never allocates or mutates again",async()=>{
    const x=await setup(),pin=await x.pin();const allocated=await x.transport.resolveNewWorksheetName("ANOVAresults",pin.sourceToken);
    const {plan,intent}=x.planFor(allocated.resolvedName,pin);await x.transport.prepareOutput(plan,intent,pin.sourceToken);
    // Simulates an external add reaching the server after final collection read, at add submission.
    x.state.hook=async()=>{if(x.state.added===1)x.sheets.set("foreign",{id:"foreign",name:allocated.resolvedName});};
    const result=await x.transport.write(plan,intent,null);
    assert.equal(result.code,"output.mutation-uncertain");assert.equal(result.mutationMayHaveOccurred,true);
    assert.equal(x.state.added,1);assert.equal((await x.transport.resolveNewWorksheetName("ANOVAresults",pin.sourceToken)).code,"output.mutation-uncertain");
    await x.transport.write(plan,intent,null);assert.equal(x.state.added,1);await x.transport.dispose();
});
test("resolved plan versus original intent cannot bypass equality validation",async()=>{
    const x=await setup(),pin=await x.pin(),{plan,intent}=x.planFor("ANOVAresults1",pin);intent.worksheetName="ANOVAresults";
    assert.throws(()=>validateProductionPlan(plan,intent));assert.equal((await x.transport.prepareOutput(plan,intent,pin.sourceToken)).code,"output.rejected");
    assert.equal(x.state.added,0);await x.transport.dispose();
});
