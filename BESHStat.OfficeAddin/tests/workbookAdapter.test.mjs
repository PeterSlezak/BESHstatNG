import test from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { createWorkbookTransport, create } from "../wwwroot/js/workbookAdapter.mjs";
import { fakeWorkbook } from "./helpers/workbook.mjs";

async function snapshot(transport, limits, reference = null) {
    const begin = await transport.beginRead("read", limits, reference);
    if (begin.code !== "workbook.ok") return begin;
    const rows = Array.from({length:begin.metadata.rowCount}, () => []);
    let chunk;
    do {
        chunk = await transport.readNext("read");
        if (chunk.code !== "workbook.ok") return chunk;
        chunk.rows.forEach((r,i) => r.forEach((c,j) => rows[chunk.rowOffset+i][chunk.columnOffset+j]=c));
    } while (!chunk.done);
    const end = await transport.finishRead("read");
    return {...end, rows};
}
for (const index of [0,1]) test(`canonical acquisition fixture range ${index + 1} preserves every cell`, async () => {
    const {scope, state, fixture} = fakeWorkbook(index);
    const adapter = createWorkbookTransport(scope);
    const result = await snapshot(adapter);
    assert.equal(result.code,"workbook.ok");
    assert.deepEqual(result.rows, fixture.Rows.map(row => row.map(c => Object.fromEntries(Object.entries(c).map(([k,v])=>[k[0].toLowerCase()+k.slice(1),v])))));
    assert.equal(result.metadata.address, fixture.Address);
    assert.equal(result.metadata.startRow, fixture.StartRow);
    assert.equal(result.metadata.startColumn, fixture.StartColumn);
    assert.equal(result.metadata.worksheetId,"native-sheet");
    assert.equal("workbookId" in result.metadata,false);
    assert.equal(state.mutations,0);
    adapter.dispose();
});

test("production factory returns a plain adapter with single wrapping", () => {
    const adapter = create();
    assert.equal(typeof adapter.beginRead,"function");
    assert.equal(adapter.__jsObjectId,undefined);
    adapter.dispose();
});

test("single cell has explicit uppercase endpoints and retained position", async () => {
    const {scope,state} = fakeWorkbook(); state.selected={row:9,col:27,rows:1,cols:1};
    const result = await snapshot(createWorkbookTransport(scope));
    assert.equal(result.metadata.address,"AA9:AA9");
    assert.equal(result.rows.length,1);
});

for (const [rows,cols,cap,tiles] of [[7,4,6,7],[1,5001,2000,3],[5,5,4,10],[2,4,8,1]])
test(`bounded tiling ${rows}x${cols} at ${cap} cells`, async () => {
    const {scope,state} = fakeWorkbook(); state.selected={row:1,col:1,rows,cols};
    const result = await snapshot(createWorkbookTransport(scope),{maxChunkCells:cap});
    assert.equal(result.code,"workbook.ok");
    assert.equal(state.payloads.length,tiles);
    assert.ok(state.payloads.every(t=>t.rows*t.cols<=cap));
    assert.equal(result.rows.flat().length,rows*cols);
    assert.equal(state.runs,tiles+2);
});

for (const [option,code] of [[{maxRangeCells:7},"input.range-cells"],[{maxTotalCells:7},"input.total-cells"],
    [{maxEstimatedBytes:8191},"input.estimated-bytes"]]) test(`metadata preflight ${code} before values`,async()=>{
    const {scope,state}=fakeWorkbook(); const result=await snapshot(createWorkbookTransport(scope),option);
    assert.equal(result.code,code); assert.equal(state.payloads.length,0);
});
test("oversized source never requests values",async()=>{
    const {scope,state}=fakeWorkbook(); state.selected={row:1,col:1,rows:1048576,cols:1};
    assert.equal((await snapshot(createWorkbookTransport(scope))).code,"input.range-cells");
    assert.equal(state.payloads.length,0);
});
for (const [mutation,code] of [[s=>s.multiple=true,"input.invalid-selection"],[s=>s.selected.rows=0,"host.invalid-response"],
    [s=>s.addressOverride="Sheet!A1,C3","user.document-changed"]]) test(`invalid or multi-area selection ${code}`,async()=>{
    const {scope,state}=fakeWorkbook(); mutation(state);
    const result=await snapshot(createWorkbookTransport(scope));
    assert.equal(result.code,code);
    assert.equal(state.payloads.length,0);
});

for (const [name,mutate] of [["renamed",s=>s.sheet.name="Renamed"],["deleted",s=>s.sheet.deleted=true],
    ["replaced",s=>s.sheet.id="replacement"],["selection",s=>s.selected.row++],["document",s=>s.document={}]])
test(`operation rejects changed ${name} before another payload`,async()=>{
    const {scope,state}=fakeWorkbook(), adapter=createWorkbookTransport(scope);
    assert.equal((await adapter.beginRead("read")).code,"workbook.ok");
    mutate(state);
    const result=await adapter.readNext("read");
    assert.equal(result.code,"user.document-changed"); assert.equal(result.rows,undefined);
    assert.equal(state.payloads.length,0);
});

test("selection changes during payload sync reject the tile",async()=>{
    const {scope,state}=fakeWorkbook(), adapter=createWorkbookTransport(scope);
    await adapter.beginRead("read");
    state.hook=async n=>{if(n===3)state.selected.col++;};
    const result=await adapter.readNext("read"); assert.equal(result.code,"user.document-changed"); assert.equal(result.rows,undefined);
});
test("pinned reference uses checked sheet ID and coordinates",async()=>{
    const {scope,fixture}=fakeWorkbook();
    const ref={worksheetId:"native-sheet",worksheetName:fixture.WorksheetName,address:fixture.Address,addressDialect:1,
        startRow:fixture.StartRow,startColumn:fixture.StartColumn,rowCount:fixture.RowCount,columnCount:fixture.ColumnCount};
    assert.equal((await snapshot(createWorkbookTransport(scope),undefined,ref)).code,"workbook.ok");
    ref.worksheetId="missing";
    assert.equal((await snapshot(createWorkbookTransport(scope),undefined,ref)).code,"user.document-changed");
});
for (const field of ["workbookId","workbookName","selectionId","selectionRevision"]) test(`unverifiable reference ${field} rejected`,async()=>{
    const {scope,fixture}=fakeWorkbook();
    const ref={worksheetId:"native-sheet",worksheetName:fixture.WorksheetName,address:fixture.Address,addressDialect:1,[field]:"unknown"};
    assert.equal((await snapshot(createWorkbookTransport(scope),undefined,ref)).code,"user.input-invalid");
});

for (const [label,configure,code] of [["missing",s=>delete s.Office,"office.unavailable"],
    ["Word",s=>s.Office.onReady=async()=>({host:"Word",platform:"OfficeOnline"}),"office.unsupported-host"],
    ["API",s=>s.Office.context.requirements.isSetSupported=()=>false,"office.unsupported-api"]])
test(`host ${label} cannot enter Excel.run`,async()=>{
    const {scope,state}=fakeWorkbook(); configure(scope);
    assert.equal((await snapshot(createWorkbookTransport(scope))).code,code); assert.equal(state.runs,0);
});

for (const [label,change] of [["nonfinite",r=>r.values[0][0]=Infinity],["types",r=>r.valueTypes=null],
    ["shape",r=>r.values=[]],["object",r=>{r.values[0][0]={secret:1};r.valueTypes[0][0]="String";}],
    ["format",r=>r.numberFormat[0][0]=null]]) test(`malformed payload ${label} rejected`,async()=>{
    const {scope,state}=fakeWorkbook(); state.payloadHook=change;
    assert.equal((await snapshot(createWorkbookTransport(scope))).code,"host.invalid-response");
});
test("long text is bounded by estimated bytes in addition to cell quotas",async()=>{
    const {scope,state}=fakeWorkbook(); state.cell=()=>({Kind:2,TextValue:"x".repeat(32767),NumberFormat:"General"});
    assert.equal((await snapshot(createWorkbookTransport(scope),{maxEstimatedBytes:9000})).code,"input.estimated-bytes");
});
test("cooperative time budget uses monotonic elapsed and returns no tile",async()=>{
    const {scope,state}=fakeWorkbook(); let time=0;
    const adapter=createWorkbookTransport(scope,{now:()=>time});
    await adapter.beginRead("read",{timeBudgetMilliseconds:100});
    state.hook=async()=>{time=101;};
    assert.equal((await adapter.readNext("read")).code,"input.time-budget");
});
test("hung native sync is bounded",async()=>{
    const {scope,state}=fakeWorkbook(); state.hook=()=>new Promise(()=>{});
    assert.equal((await createWorkbookTransport(scope).beginRead("read",{timeBudgetMilliseconds:10})).code,"input.time-budget");
});
test("cancellation between chunks discards operation and next read can retry",async()=>{
    const {scope}=fakeWorkbook(),adapter=createWorkbookTransport(scope);
    await adapter.beginRead("read",{maxChunkCells:4});
    assert.equal((await adapter.readNext("read")).done,false);
    adapter.cancel("read");
    assert.equal((await adapter.readNext("read")).rows,undefined);
    assert.equal((await snapshot(adapter)).code,"workbook.ok");
});
test("concurrent operations reject without redirecting the owner",async()=>{
    const {scope}=fakeWorkbook(),adapter=createWorkbookTransport(scope);
    const first=adapter.beginRead("first");
    assert.equal((await adapter.beginRead("second")).code,"host.busy");
    await first; adapter.cancel("first"); adapter.dispose();
    assert.equal((await adapter.beginRead("after")).code,"user.cancelled");
});
test("dispose during sync and separate instances cannot share proxies or callbacks",async()=>{
    const a=fakeWorkbook(),b=fakeWorkbook(1),first=createWorkbookTransport(a.scope),second=createWorkbookTransport(b.scope);
    await first.beginRead("one"); a.state.hook=async()=>first.dispose();
    assert.equal((await first.readNext("one")).code,"user.cancelled");
    assert.equal((await snapshot(second)).code,"workbook.ok");
});
test("metadata inspection and finish load no cell matrices",async()=>{
    const {scope,state}=fakeWorkbook(),adapter=createWorkbookTransport(scope);
    assert.equal((await adapter.beginRead("metadata",undefined,null,true)).code,"workbook.ok");
    assert.equal((await adapter.finishRead("metadata")).code,"workbook.ok");
    assert.equal(state.payloads.length,0);
});

test("pending genuine readiness performs no workbook access until resolution",async()=>{
    const {scope,state}=fakeWorkbook(); let resolve;
    scope.Office.onReady=()=>new Promise(yes=>{resolve=yes;});
    const adapter=createWorkbookTransport(scope), pending=adapter.beginRead("read");
    assert.equal(state.runs,0);
    resolve({host:"Excel",platform:"OfficeOnline"});
    assert.equal((await pending).code,"workbook.ok");
    adapter.cancel("read");adapter.dispose();
});
test("concurrent tile calls cannot overlap or advance another cursor",async()=>{
    const {scope,state}=fakeWorkbook(),adapter=createWorkbookTransport(scope);
    await adapter.beginRead("read",{maxChunkCells:4});
    let release;state.hook=()=>new Promise(resolve=>{release=resolve;});
    const first=adapter.readNext("read");
    assert.equal((await adapter.readNext("read")).code,"user.input-invalid");
    state.hook=async()=>{};release();
    assert.equal((await first).done,false);
    assert.equal((await adapter.readNext("read")).done,true);
    assert.equal((await adapter.finishRead("read")).code,"workbook.ok");
});
test("unknown or raised policy fields fail before native access",async()=>{
    for(const options of [{signal:{}},{toString:"unknown"},{maxRangeCells:10001},{maxChunkCells:2001},{estimatedBytesPerCell:1},{maxRanges:0}]){
        const {scope,state}=fakeWorkbook();
        assert.equal((await createWorkbookTransport(scope).beginRead("read",options)).code,"user.input-invalid");
        assert.equal(state.runs,0);
    }
});

test("Razor uses only typed injected services and no permanent route",()=>{
    const razor=readFileSync(new URL("../App.razor",import.meta.url),"utf8");
    assert.doesNotMatch(razor,/IJSRuntime|Excel\.run|Office\.context|window\.|@page/);
    assert.equal((razor.match(/@inject.*IWorkbookAdapter/g)||[]).length,1);
    assert.match(razor,/if \(!loading && status.IsExcel && ReadDiagnostics.Enabled\)/);
    assert.equal((razor.match(/<AnalysisShell /g)||[]).length,1);
    assert.match(razor,/Read selected range \(read-only\)/);
    assert.match(razor,/if \(ReadDiagnostics.Enabled\)/);
    assert.match(razor,/Workbook\.InspectSelectionAsync\(lifetime.Token\)/);
    assert.doesNotMatch(readFileSync(new URL("../wwwroot/js/workbookAdapter.mjs",import.meta.url),"utf8"),/console\.|DotNet\.createJSObjectReference/);
});
