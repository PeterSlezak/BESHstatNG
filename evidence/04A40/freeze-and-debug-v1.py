from common40_v3 import *
from collections import Counter
from concurrent.futures import ThreadPoolExecutor
lock();run=runner();pins=run.__class__.build.__globals__['source_pins']()
build=load(PRIVATE/'wave2-Debug-build-v2.json');assert pins==build['source_pins'] and not any(build['exit_codes'])
save('matrix/source-freeze-v1.json',dict(utc=utc(),files=[pin(p) for p in sorted(pins)],source_pins=pins,Debug_build=pin(PRIVATE/'wave2-Debug-build-v2.json'),
    source_delta=[dict(before=r,after=pin(r['path'])) for r in load(HERE/'entry/source-preimages-v3.json')['files'] if pin(r['path'])!=r],
    new_tests=[pin('BESHStat.Excel.Common.Tests/src/Regression40FitPolicyTests.vb'),pin('BESHStat.Excel.Common.Tests/src/Regression40MixedSurvivalTests.vb')],
    no_source_changes_after_this_campaign=True))
suites=[('Core','all',None),('Common','main','FullyQualifiedName!~MigrationSemantic04A02Tests'),('Legacy','all',None),('ControlMitigation','all',None),
 ('OfficeAddin','all',None),('OfficeExecution','all',None),('Common','semantic','FullyQualifiedName~MigrationSemantic04A02Tests')]
save('matrix/campaigns-v1.json',dict(Debug=dict(mode='ONE_FRESH_FULL',label='a40-final-Debug-v1',parts=suites,expected_inherited=21127,expected_added=12),
    Release=dict(mode='FRESH_BUILD_TARGETED_ONLY;FULL_NOT_RUN',target='Regression/formula/GEE/mixed/survival/UDF preparation and39 input policies',expected_exports=268),
    compatibility=dict(mode='FRESH_CURRENT_ONLY_DEBUG_AND_RELEASE',preedit=28966,supplement=458,total_per_configuration=29424,original_executions=0),
    Python=dict(mode='ONE_FRESH_FINAL_956_PARENT_CAMPAIGN',worker_limit=6,shared_scanners='SERIAL'),
    Node=dict(mode='FRESH_FINAL_SHARED_POLICY_DEPENDENCY_CHECK',baseline=916),strict_profile_MkDocs='SERIAL_REAL_EXITS',
    native_scientific='NOT_RUN_UNAUTHORIZED;C13_AND_C12_BEHAVIOR_FIXES_NOT_ACTIVATED',gates=GATES))
def execute(spec):
    suite,part,selection=spec;name='a40-final-Debug-'+suite+'-'+part
    result=run.test('Debug','a40-wave2-v2',suite,name,selection)
    save(PRIVATE/(name+'.json'),result)
    return dict(suite=suite,part=part,selection=selection,name=name,outcomes=result['outcomes'],
        cases=pin(PRIVATE/'executions'/name/'cases.json'),TRX=pin(PRIVATE/'executions'/name/'results'/(suite+'.trx')),raw=pin(PRIVATE/(name+'.json')),command_exit=result['command']['exit_code'])
def lane(items):return [execute(x) for x in items]
with ThreadPoolExecutor(max_workers=3) as pool:
    batches=list(pool.map(lane,[[suites[0],suites[1]],[suites[2],suites[3]],[suites[4],suites[5]]]))
parts=[p for batch in batches for p in batch];parts.append(execute(suites[6]))
totals=Counter()
for p in parts:totals.update(p['outcomes'])
assert pins==run.__class__.build.__globals__['source_pins']()
save('build-and-tests/Debug-full-executed-v1.json',dict(configuration='Debug',mode='FULL_FRESH',parts=parts,outcomes=dict(totals),identities=sum(totals.values()),
    source_unchanged=True,max_parallel_test_hosts=3,isolated_paths=True,repeat_identities=0))
lock();print('One fullDebug campaign completed',sum(totals.values()),dict(totals),flush=True)
