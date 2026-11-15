from common41 import *
import shutil
lock();run=runner();build=run.build('Debug','a41-wave2-v1')
save(PRIVATE/'wave2-build-v1.json',build);assert not any(build['exit_codes']) and build['source_unchanged']
result=run.test('Debug','a41-wave2-v1','Common','a41-wave2-Common-v1','FullyQualifiedName~UdfInference41Tests|FullyQualifiedName~UdfAgreementSizing41Tests')
save(PRIVATE/'wave2-tests-v1.json',result)
assert result['outcomes'].get('Failed',0)==0 and result['outcomes'].get('Passed',0)>=18
folder=PRIVATE/'fixtures-wave2-v1';folder.mkdir()
for p in (PRIVATE/'builds/a41-wave2-v1/Debug/legacy').glob('*.dll'):shutil.copy2(p,folder/p.name)
shutil.copy2(PRIVATE/'fixtures-before-v1/Observe41.exe',folder/'Observe41.exe')
r=command('fixture-wave2-execute-v1',[str(folder/'Observe41.exe'),str(folder/'observations.json')]);assert r['exit_code']==0
before=load(PRIVATE/'fixtures-before-v1/observations.json');after=load(folder/'observations.json')
assert before==after,'CURRENT_MAIN_CASE_CHANGED'
save('waves/wave2-verification-v1.json',dict(build_exit_codes=build['exit_codes'],tests=result['outcomes'],tests_raw=pin(PRIVATE/'wave2-tests-v1.json'),current_main_materialized_cases=390,exact=True,before=pin(PRIVATE/'fixtures-before-v1/observations.json'),after=pin(folder/'observations.json'),scientific='NOT_GOLDEN'))
print('Wave2 focused actual-engine tests and390 current-main cases PASS',flush=True)
