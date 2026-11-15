from common40_v3 import *
import shutil
lock();run=runner();build=run.build('Debug','a40-wave2-v1')
save(PRIVATE/'wave2-Debug-build-v1.json',build);assert not any(build['exit_codes']) and build['source_unchanged']
tests=run.test('Debug','a40-wave2-v1','Common','a40-wave2-Common-new','FullyQualifiedName~Regression40')
save(PRIVATE/'wave2-Common-tests-v1.json',tests)
assert tests['command']['exit_code']==0 and tests['outcomes']=={'Passed':12}
folder=PRIVATE/'fixtures-after-wave2-v1';folder.mkdir()
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:
    shutil.copy2(PRIVATE/'builds/a40-wave2-v1/Debug/legacy'/name,folder/name)
shutil.copy2(PRIVATE/'fixtures-before-v2/Observe40.exe',folder/'Observe40.exe')
record=command('after-wave2-fixtures-v1',[str(folder/'Observe40.exe'),str(PRIVATE/'after-wave2-v1.json'),'policy'])
save(PRIVATE/'after-wave2-fixture-command-v1.json',record);assert record['exit_code']==0
before=load(PRIVATE/'before-v2.json');after=load(PRIVATE/'after-wave2-v1.json')
assert before==after,'CURRENT_MAIN_BEHAVIOR_CHANGED'
save('waves/wave2-v1.json',dict(status='12_NEW_FOCUSED_PASS_48_EXACT_CURRENT_MAIN_OBSERVATIONS',build_codes=build['exit_codes'],
    tests=12,outcomes=tests['outcomes'],test_raw=pin(PRIVATE/'wave2-Common-tests-v1.json'),
    before=pin(PRIVATE/'before-v2.json'),after=pin(PRIVATE/'after-wave2-v1.json'),distinct_cases=len(before['rows']),exact=True,
    retained_GEE_start_exceptions=4,new_production_refusals=0,scientific='NOT_GOLDEN',native='NOT_RUN_UNAUTHORIZED'))
lock();print('Wave2:12 new tests PASS;48 current-main cases exact including4 retained supplied-start GEE exceptions',flush=True)
