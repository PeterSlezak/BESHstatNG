from common41 import *
from concurrent.futures import ThreadPoolExecutor
from collections import Counter
lock();run=runner();pins=run.__class__.build.__globals__['source_pins']();config=sys.argv[1]
if config=='Debug':
    build=load(PRIVATE/'wave2-build-v1.json');assert pins==build['source_pins'] and not any(build['exit_codes'])
    entry=load(HERE/'entry/source-preimages-v1.json')
    delta=[dict(before=r,after=pin(r['path'])) for r in entry['files'] if pin(r['path'])!=r]
    assert len(delta)==10 and not any(r['after']['path'].startswith(('BESHStat.Core/','BESHstatNG/src/UI/')) for r in delta)
    save('matrix/source-freeze-v1.json',dict(utc=utc(),source_pins=pins,files=[pin(p) for p in sorted(pins)],source_delta=delta,new_tests=[pin('BESHStat.Excel.Common.Tests/src/UdfInference41Tests.vb'),pin('BESHStat.Excel.Common.Tests/src/UdfAgreementSizing41Tests.vb')],plan=pin('planning/BESHStatNG_CrossPlatform_MasterPlan.md'),plan_snapshot=pin('planning/BESHStatNG_CrossPlatform_MasterPlan2.17.md'),Debug_build=pin(PRIVATE/'wave2-build-v1.json'),no_runtime_or_test_source_changes_after_freeze=True))
    suites=[('Core','all',None),('Common','main','FullyQualifiedName!~MigrationSemantic04A02Tests'),('Legacy','all',None),('ControlMitigation','all',None),('OfficeAddin','all',None),('OfficeExecution','all',None),('Common','semantic','FullyQualifiedName~MigrationSemantic04A02Tests')]
    label='a41-wave2-v1'
else:
    assert config=='Release'
    assert pins==load(HERE/'matrix/source-freeze-v1.json')['source_pins']
    build=run.build('Release','a41-final-v1');save(PRIVATE/'final-Release-build-v1.json',build)
    assert not any(build['exit_codes']) and build['source_unchanged']
    label='a41-final-v1'
    selected='|'.join('FullyQualifiedName~'+x for x in ['UdfInference41','UdfAgreementSizing41','Agreement26','Planning26','AnovaWorkflow07','Assumptions','Contingency','Udf','Parametric','SampleSize'])
    suites=[('Core','affected',selected),('Common','affected',selected),('Legacy','affected',selected)]
def execute(spec):
    suite,part,selection=spec;name='a41-final-'+config+'-'+suite+'-'+part
    result=run.test(config,label,suite,name,selection);save(PRIVATE/(name+'.json'),result)
    return dict(suite=suite,part=part,selection=selection,name=name,outcomes=result['outcomes'],cases=pin(PRIVATE/'executions'/name/'cases.json'),TRX=pin(PRIVATE/'executions'/name/'results'/(suite+'.trx')),raw=pin(PRIVATE/(name+'.json')),command_exit=result['command']['exit_code'])
if config=='Debug':
    def lane(specs):return [execute(s) for s in specs]
    with ThreadPoolExecutor(max_workers=3) as pool:batches=list(pool.map(lane,[[suites[0],suites[1]],[suites[2],suites[3]],[suites[4],suites[5]]]))
    parts=[p for batch in batches for p in batch]+[execute(suites[6])]
else:parts=[execute(s) for s in suites]
totals=Counter()
for p in parts:totals.update(p['outcomes'])
assert pins==run.__class__.build.__globals__['source_pins']()
save('build-and-tests/'+config+'-executed-v1.json',dict(configuration=config,mode='ONE_FULL_FRESH_CAMPAIGN' if config=='Debug' else 'TARGETED_AFFECTED_CONTRACTS;FULL_RELEASE_NOT_RUN',parts=parts,outcomes=dict(totals),identities=sum(totals.values()),source_unchanged=True,Debug_label='a41-wave2-v1',Release_label='a41-final-v1',full_Release='NOT_RUN',max_parallel_test_hosts=3 if config=='Debug' else 1,protected_raw_diagnostics=True))
print(config,'campaign complete',sum(totals.values()),dict(totals),flush=True)
