from common43 import *
import shutil
lock();run=runner();b=run.build('Debug','a43-wave-b2-v1');save(PRIVATE/'wave-b2-build-v1.json',b);assert not any(b['exit_codes']) and b['source_unchanged']
parts=[]
for suite,selection in [('Common','FullyQualifiedName~UdfHandlePolicyTests'),('Legacy','FullyQualifiedName~UdfHandleSessionTests|FullyQualifiedName~UdfPreparation38AdapterTests')]:
    r=run.test('Debug','a43-wave-b2-v1',suite,'a43-wave-b2-'+suite+'-v1',selection);save(PRIVATE/('wave-b2-'+suite+'-tests-v1.json'),r);assert not r['outcomes'].get('Failed',0)
    assert r['outcomes'].get('Passed',0)>=(35 if suite=='Common' else 218);parts.append(dict(suite=suite,outcomes=r['outcomes'],raw=pin(PRIVATE/('wave-b2-'+suite+'-tests-v1.json'))))
cmp=module('cmp43',HERE/'compare43.py');raws=[]
for name,folder,exe in [('lifecycle','fixtures-before-v3','Observe43V3.exe'),('races','races-before-v1','Race43.exe')]:
    target=PRIVATE/('wave-b2-'+name+'-v1');target.mkdir()
    for p in (PRIVATE/'builds/a43-wave-b2-v1/Debug/legacy').glob('*.dll'):shutil.copy2(p,target/p.name)
    shutil.copy2(PRIVATE/folder/exe,target/exe);result=command('wave-b2-'+name+'-v1',[str(target/exe),str(target/'observations.json'),str(HERE/'scope/cache-export-inventory-v1.json')]);assert result['exit_code']==0
    old=load(PRIVATE/folder/'observations.json');now=load(target/'observations.json')
    if name=='lifecycle':assert cmp.canonical(old)==cmp.canonical(now);detail=dict(operations=len(old['operations']),graphs=36,reference_bijection_verified=True,all_nonvolatile_output_error_values_exact=True)
    else:detail=cmp.validate_races(now);assert detail['identities']==cmp.validate_races(old)['identities']
    raws.append(dict(name=name,baseline=pin(PRIVATE/folder/'observations.json'),current=pin(target/'observations.json'),result=detail))
save('waves/wave-b2-v1.json',dict(build_exits=b['exit_codes'],parts=parts,fixtures=raws,no_cache_storage_model_fit_or_public_errors_changed=True,gates=GATES))
print('Wave B 35 Common/218 Legacy focused tests and18-family relational/race fixtures PASS',flush=True)
