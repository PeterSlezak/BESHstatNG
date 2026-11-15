"""Schedule one replacement finalization for two demonstrated source-contract defects."""
from common40_v3 import *
lock()
changes=[]
for name in ['RegressionRequestPolicy.vb','GeeWorkflowPolicy.vb']:
    path='BESHStat.Excel.Common/src/Analysis/'+name
    previous=PRIVATE/'pre-correction-v1'/name
    assert pin(previous)['sha256']==load(HERE/'matrix/source-freeze-v1.json')['source_pins'][path]
    changes.append(dict(before=pin(previous),after=pin(path)))
plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';old=plan.read_bytes();assert pin(plan)['sha256']==next(x['after']['sha256'] for x in load(HERE/'matrix/source-freeze-v1.json')['source_delta'] if x['after']['path'].startswith('planning/'))
save('matrix/replacement-authorization-v2.json',dict(utc=utc(),reason='Final Python exposed two new source-contract failures. Remove misleading host-keyword comment; restore unnecessary GUI GEE intent setter rewrite to authenticated main. Existing test source unchanged; no algorithm/default/native change.',
    source_changes=changes,original_full_Python=pin(HERE/'python/parent-event-reconciliation-v1.json'),old_full_campaigns_retained=True,
    focused_corrected_parents=dict(executed=2,PASS=2,exit_code=0,provenance='Actual unified-exec tool result; original stdout two tests/OK retained in conversation; no rerun to manufacture raw receipt.'),
    new_campaigns='One fullDebug and one full956Python per corrected source state; Release build/targeted and fresh original-reference current-only compatibility. Node remains exact unchanged JS input carry-forward of fresh916.',gates=GATES))
dest=HERE/'planning/masterplan-2.16-frozen-v1.md';dest.parent.mkdir(exist_ok=True);dest.open('xb').write(old)
text=old.decode('utf-8-sig').replace('Revision 2.16','Revision 2.17',1)
before='**04A.01–04A.38 owner-accepted;04A.38 merge independently verified;04A.39 IN_PROGRESS and not accepted;04A.40–04A.75 remain planned.**'
after='**04A.01–04A.39 owner-accepted;04A.39 merge independently verified at `f2eb060c9c422c1fec4e82989d07907dbd4b9337`;04A.40 IN_PROGRESS and not accepted;04A.41–04A.75 remain planned.**'
assert text.count(before)==1;text=text.replace(before,after)
plan.write_text(text,encoding='utf-8',newline='\n')
def recipe(source,target,edits):
    s=(HERE/source).read_text(encoding='utf-8')
    for a,b in edits:
        assert a in s,(source,a);s=s.replace(a,b)
    (HERE/target).open('x',encoding='utf-8',newline='\n').write(s)
recipe('freeze-and-debug-v1.py','freeze-and-debug-v2.py',[
 ("build=load(PRIVATE/'wave2-Debug-build-v2.json');assert pins==build['source_pins'] and not any(build['exit_codes'])","build=run.build('Debug','a40-final-v2');save(PRIVATE/'Debug-build-v2.json',build);assert pins==build['source_pins'] and not any(build['exit_codes'])"),
 ('source-freeze-v1.json','source-freeze-v2.json'),('Debug_build=pin(PRIVATE/\'wave2-Debug-build-v2.json\')','Debug_build=pin(PRIVATE/\'Debug-build-v2.json\')'),
 ('campaigns-v1.json','campaigns-v2.json'),('a40-final-Debug-v1','a40-final-Debug-v2'),('a40-final-Debug-','a40-final2-Debug-'),("run.test('Debug','a40-wave2-v2'","run.test('Debug','a40-final-v2'"),('Debug-full-executed-v1.json','Debug-full-executed-v2.json')])
recipe('release-v1.py','release-v2.py',[('a40-final-v1','a40-final-v2'),('Release-build-v1.json','Release-build-v2.json'),('a40-final-Release-','a40-final2-Release-'),('Release-targeted-executed-v1.json','Release-targeted-executed-v2.json')])
recipe('reconcile-managed-v1.py','reconcile-managed-v2.py',[("+'-executed-v1.json'","+'-executed-v2.json'"),("config+'-identities-v1.json'","config+'-identities-v2.json'")])
recipe('compiled-final-v1.py','compiled-final-v2.py',[("'a40-wave2-v2' if config=='Debug' else 'a40-final-v1'","'a40-final-v2'"),("PRIVATE/config/'lib'","PRIVATE/'final2'/config/'lib'"),("PRIVATE/'abi-cil'/config","PRIVATE/'abi-cil-final2'/config"),("'final-'+config+'-'","'final2-'+config+'-'")])
recipe('compiled-readback-v3.py','compiled-readback-v4.py',[("PRIVATE/'abi-cil'/config","PRIVATE/'abi-cil-final2'/config"),('compiled-final-v1.py','compiled-final-v2.py'),("'abi-'+config","'abi-final2-'+config"),("PRIVATE/config/'lib'","PRIVATE/'final2'/config/'lib'"),("proof-v1.json","proof-v2.json"),("'observe39-'+config","'observe39-final2-'+config"),("'final-'+config","'final2-'+config"),('retained-inputs-fresh-results-v1.json','retained-inputs-fresh-results-v2.json')])
recipe('compatibility-final-v1.py','compatibility-final-v2.py',[("config+'-proof-v1.json'","config+'-proof-v2.json'"),("PRIVATE/'compatibility'","PRIVATE/'compatibility-final2'"),('matrix/source-freeze-v1.json','matrix/source-freeze-v2.json'),
 ("module40.HERE=target;","module40.HERE=target;module40.PRIVATE=PRIVATE/'final2';"),("config+'-v1.py'","config+'-v2.py'"),("config+'-crosswalk-v1.json'","config+'-crosswalk-v2.json'"),("'a40-'+config+'-v1'","'a40-'+config+'-v2'")])
recipe('compatibility-readback-v3.py','compatibility-readback-v4.py',[("'compatibility/compatibility-a40-'+config+'-v1.json'","'compatibility-final2/compatibility-a40-'+config+'-v2.json'"),('current-only-v3.json','current-only-v4.json')])
print('Corrected source state scheduled; earlier failed campaigns remain immutable. Existing test source untouched.',flush=True)
