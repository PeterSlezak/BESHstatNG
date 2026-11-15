from common41 import *
import shutil
lock();run=runner();build=run.build('Debug','a41-wave1-v2')
save(PRIVATE/'wave1-build-v2.json',build);assert not any(build['exit_codes']) and build['source_unchanged']
result=run.test('Debug','a41-wave1-v2','Common','a41-wave1-Common-v2','FullyQualifiedName~UdfInference41Tests')
save(PRIVATE/'wave1-tests-v2.json',result)
assert result['outcomes'].get('Failed',0)==0 and result['outcomes'].get('Passed',0)>=8
folder=PRIVATE/'fixtures-wave1-v2';folder.mkdir()
for p in (PRIVATE/'builds/a41-wave1-v2/Debug/legacy').glob('*.dll'):shutil.copy2(p,folder/p.name)
shutil.copy2(PRIVATE/'fixtures-before-v1/Observe41.exe',folder/'Observe41.exe')
r=command('fixture-wave1-execute-v2',[str(folder/'Observe41.exe'),str(folder/'observations.json')]);assert r['exit_code']==0
before=load(PRIVATE/'fixtures-before-v1/observations.json');after=load(folder/'observations.json')
assert before==after,'CURRENT_MAIN_CASE_CHANGED'
save('waves/wave1-verification-v2.json',dict(build_exit_codes=build['exit_codes'],tests=result['outcomes'],tests_raw=pin(PRIVATE/'wave1-tests-v2.json'),current_main_materialized_cases=390,exact=True,before=pin(PRIVATE/'fixtures-before-v1/observations.json'),after=pin(folder/'observations.json'),scientific='NOT_GOLDEN'))
print('Wave1 focused tests and390 materialized current-main cases PASS',flush=True)
