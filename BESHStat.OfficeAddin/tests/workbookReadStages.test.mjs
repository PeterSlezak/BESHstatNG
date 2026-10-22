// All Excel calls here are simulated. These tests cannot establish live Office transport health.
import test from "node:test";
import assert from "node:assert/strict";
import {createWorkbookTransport} from "../wwwroot/js/workbookAdapter.mjs";
import {fakeWorkbook} from "./helpers/workbook.mjs";

function traced() {
    const fake = fakeWorkbook();
    const events = [];
    fake.scope.__beshOfficeDebug = {record(code) { events.push(code); }};
    return {...fake, events, adapter:createWorkbookTransport(fake.scope, {diagnostics:true})};
}
const smallBudget = {timeBudgetMilliseconds:25};
const includesInOrder = (events, expected) => {
    let position = -1;
    for (const code of expected) {
        position = events.indexOf(code, position + 1);
        assert.notEqual(position, -1, code);
    }
};

test("full mixed-cell read reports each native stage without data", async () => {
    const {adapter,events,state} = traced();
    assert.equal((await adapter.beginRead("private-operation")).code,"workbook.ok");
    const chunk = await adapter.readNext("private-operation");
    assert.equal(chunk.code,"workbook.ok");
    assert.ok(chunk.rows.flat().some(c=>c.kind===1));
    assert.equal((await adapter.finishRead("private-operation")).code,"workbook.ok");
    includesInOrder(events,["read.full-started","read.readiness-started","read.readiness-completed",
        "read.metadata-run-started","read.metadata-run-entered","read.metadata-sync-started","read.metadata-sync-completed",
        "read.metadata-validated","read.tile-run-started","read.preflight-validated","read.payload-sync-started",
        "read.payload-sync-completed","read.reconcile-validated","read.final-run-started","read.final-validated","read.completed"]);
    assert.ok(events.every(code=>/^(read|bootstrap)\.[a-z-]+$/.test(code)));
    assert.equal(JSON.stringify(events).includes("private"),false);
    assert.equal(state.mutations,0);
    adapter.dispose();
});

test("metadata-only comparison never runs payload stages", async () => {
    const {adapter,events,state} = traced();
    assert.equal((await adapter.beginRead("metadata",undefined,null,true)).code,"workbook.ok");
    assert.equal((await adapter.finishRead("metadata")).code,"workbook.ok");
    assert.ok(events.includes("read.metadata-started"));
    assert.ok(!events.includes("read.tile-run-started"));
    assert.equal(state.payloads.length,0);
    adapter.dispose();
});

test("unresolved second readiness is distinguished from Excel.run", async () => {
    const {scope,state} = fakeWorkbook(); const events=[];
    scope.__beshOfficeDebug={record:code=>events.push(code)};
    scope.Office.onReady=()=>new Promise(()=>{});
    const adapter=createWorkbookTransport(scope,{diagnostics:true});
    assert.equal((await adapter.beginRead("read",smallBudget)).code,"input.time-budget");
    includesInOrder(events,["read.readiness-started","read.js-timeout","read.rejected"]);
    assert.ok(!events.includes("read.readiness-completed"));
    assert.equal(state.runs,0);
    adapter.dispose();
});

test("delayed readiness resolves before native metadata and caches for retry", async () => {
    const {scope,state}=fakeWorkbook(); let resolve, calls=0;
    scope.Office.onReady=()=>{calls++;return new Promise(yes=>{resolve=yes;});};
    const adapter=createWorkbookTransport(scope);
    const pending=adapter.beginRead("first");
    assert.equal(state.runs,0);
    resolve({host:"Excel",platform:"OfficeOnline"});
    assert.equal((await pending).code,"workbook.ok"); adapter.cancel("first");
    assert.equal((await adapter.beginRead("retry")).code,"workbook.ok");
    assert.equal(calls,1); adapter.dispose();
});

test("Excel.run pending before callback has a distinct stage and late entry cannot access workbook", async () => {
    const {scope,state}=fakeWorkbook(); const events=[]; let callback;
    scope.__beshOfficeDebug={record:code=>events.push(code)};
    const original=scope.Excel.run;
    scope.Excel.run=fn=>{callback=fn;return new Promise(()=>{});};
    const adapter=createWorkbookTransport(scope,{diagnostics:true});
    assert.equal((await adapter.beginRead("old",smallBudget)).code,"input.time-budget");
    assert.ok(events.includes("read.metadata-run-started"));
    assert.ok(!events.includes("read.metadata-run-entered"));
    assert.throws(()=>callback({get workbook(){assert.fail("late workbook access");}}),/user.cancelled/);
    const count=events.length;
    scope.Excel.run=original;
    assert.equal((await adapter.beginRead("retry")).code,"workbook.ok");
    assert.equal(state.runs,1);
    assert.ok(events.length>count);adapter.dispose();
});

for (const [label, sync, expected] of [["metadata",1,"read.metadata-sync-started"],
    ["preflight",2,"read.preflight-sync-started"],["payload",3,"read.payload-sync-started"],
    ["reconcile",4,"read.reconcile-sync-started"],["final",5,"read.final-sync-started"]]) {
    test(`pending ${label} sync times out once; late completion cannot publish rows or contaminate retry`,async()=>{
        const {adapter,events,state}=traced(); let release;
        state.hook=n=>n===sync?new Promise(yes=>{release=yes;}):Promise.resolve();
        let result=await adapter.beginRead("old",smallBudget);
        if(sync>1) result=await adapter.readNext("old");
        if(sync===5) result=await adapter.finishRead("old");
        assert.deepEqual(result,{code:"input.time-budget"});
        assert.ok(events.includes(expected));
        assert.equal(events.filter(c=>c==="read.js-timeout").length,1);
        const count=events.length;
        state.hook=async()=>{};release();
        await new Promise(yes=>setImmediate(yes));
        assert.equal(events.length,count,"late callbacks must not record completion");
        assert.equal((await adapter.beginRead("retry")).code,"workbook.ok");
        adapter.dispose();
    });
}

for(const action of ["cancel","dispose"]) test(`${action} during native sync ignores late rows`,async()=>{
    const {adapter,events,state}=traced();
    await adapter.beginRead("read");
    let release;state.hook=()=>new Promise(yes=>{release=yes;});
    const pending=adapter.readNext("read");
    adapter[action]("read");state.hook=async()=>{};release();
    assert.deepEqual(await pending,{code:"user.cancelled"});
    assert.ok(!events.includes("read.completed"));
    adapter.dispose();
});

test("diagnostic sink is disabled by default and cannot break reads when explicitly enabled",async()=>{
    for(const diagnostics of [false,true]) {
        const {scope}=fakeWorkbook();
        scope.__beshOfficeDebug={enableWorkbookDevelopment(){throw new Error("SECRET");},record(){throw new Error("SECRET");}};
        const adapter=createWorkbookTransport(scope,{diagnostics});
        assert.equal((await adapter.beginRead("read")).code,"workbook.ok");
        adapter.dispose();
    }
});
