import {test} from 'node:test';
import assert from 'node:assert/strict';
import {createCommandBridge} from '../wwwroot/js/ribbonCommands.mjs';
test('bootstrap does not add a readiness waiter without command APIs and swallows rejection',async()=>{
    const original=globalThis.Office;let calls=0;
    try {
        globalThis.Office={onReady(){calls++;throw Error('must not wait');}};
        await import('../wwwroot/js/ribbonCommands.mjs?without-command-api');assert.equal(calls,0);
        globalThis.Office={actions:{associate(){}},onReady(){calls++;return Promise.reject(Error('PRIVATE'));}};
        await import('../wwwroot/js/ribbonCommands.mjs?rejected-ready');await new Promise(r=>setImmediate(r));assert.equal(calls,1);
    } finally {globalThis.Office=original;}
});
function fixture(){
    const calls=[],handlers=new Map();let done=0;
    const scope={Office:{context:{requirements:{isSetSupported:()=>true}},actions:{associate:(n,f)=>handlers.set(n,f)},
        addin:{showAsTaskpane:async()=>calls.push('show')}}};
    const bridge=createCommandBridge(scope);
    return {scope,bridge,calls,handlers,event:{completed(){done++;}},get done(){return done;},reference:{invokeMethodAsync:async(_,id)=>{calls.push(id);return 'command.navigated';}}};
}
test('associations install once and every event completes without Excel or storage access',async()=>{
    const f=fixture();assert.equal(f.bridge.install(),true);f.bridge.install();assert.equal(f.handlers.size,7);
    f.bridge.bind('owner',f.reference);await f.bridge.ready('owner');
    for(const name of ['BeshAnova','BeshKruskal','BeshAbout','BeshSettings','BeshLog','BeshHelp']) assert.equal(await f.handlers.get(name)(f.event),'command.navigated');
    assert.equal(f.done,6);assert.ok(f.calls.includes('method.OneWayANOVA'));assert.ok(f.calls.includes('method.KruskalWallisTest'));
});
test('cold commands collapse to latest bounded intent and are consumed once',async()=>{
    const f=fixture();assert.equal(await f.bridge.invoke('options',f.event),'command.pending');await f.bridge.invoke('about',f.event);
    f.bridge.bind('owner',f.reference);await f.bridge.ready('owner');await f.bridge.ready('owner');
    assert.deepEqual(f.calls,['show','show','about']);assert.equal(f.done,2);
});
test('expired/disposed/old-owner intents never replay',async()=>{
    const f=fixture();let time=0;const b=createCommandBridge(f.scope,()=>time);await b.invoke('options');time=30001;
    b.bind('old',f.reference);await b.ready('old');assert.deepEqual(f.calls,['show']);
    b.bind('new',f.reference);b.unbind('old');await b.ready('new');await b.invoke('about');assert.ok(f.calls.includes('about'));
    b.unbind('new');b.bind('third',f.reference);await b.ready('third');assert.equal(f.calls.filter(c=>c==='about').length,1);
});
for(const id of ['export','method.LinearRegression','javascript:alert(1)','https://evil.example/','OPTIONS','',null])test('invalid/unavailable dispatch: '+id,async()=>{
    const f=fixture();const code=await f.bridge.invoke(id,f.event);assert.ok(['command.invalid','command.unavailable'].includes(code));assert.deepEqual(f.calls,[]);assert.equal(f.done,1);
});
test('unsupported requirement sets and missing APIs fail closed',async()=>{
    const f=fixture();f.scope.Office.context.requirements.isSetSupported=()=>false;
    assert.equal(await f.bridge.invoke('options',f.event),'command.host-unsupported');assert.deepEqual(f.calls,[]);
    const missing=createCommandBridge({});assert.equal(missing.install(),false);assert.equal(await missing.invoke('about'),'command.host-unsupported');
});
test('rejected show and rejected managed dispatch are truthful and complete',async()=>{
    const f=fixture();f.scope.Office.addin.showAsTaskpane=async()=>{throw Error('PRIVATE');};
    assert.equal(await f.bridge.invoke('about',f.event),'command.open-failed');assert.equal(f.done,1);
    f.scope.Office.addin.showAsTaskpane=async()=>{};f.bridge.bind('owner',{invokeMethodAsync(){throw Error('PRIVATE');}});await f.bridge.ready('owner');
    assert.equal(await f.bridge.invoke('about',f.event),'command.rejected');assert.equal(f.done,2);
});
test('rapid show completion cannot publish an older command',async()=>{
    const f=fixture();let release;f.scope.Office.addin.showAsTaskpane=()=>new Promise(r=>{release=r;});
    f.bridge.bind('owner',f.reference);await f.bridge.ready('owner');const first=f.bridge.invoke('options');
    const releaseFirst=release;const second=f.bridge.invoke('about');release();assert.equal(await second,'command.navigated');releaseFirst();
    assert.equal(await first,'command.superseded');assert.deepEqual(f.calls,['about']);
});
