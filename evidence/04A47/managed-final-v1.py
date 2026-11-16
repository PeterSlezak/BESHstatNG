from common47 import *
from concurrent.futures import ThreadPoolExecutor
from collections import Counter
lock();config=sys.argv[1];assert config in ['Debug','Release'];run=runner();label='a47-final-v1'
assert run.__class__.build.__globals__['source_pins']()==load(HERE/'matrix/source-freeze-v1.json')['source_pins']
build=run.build(config,label);save(PRIVATE/('final-'+config+'-build-v1.json'),build);assert not any(build['exit_codes']) and build['source_unchanged']
if config=='Debug':
 suites=[('Core','all',None),('Common','main','FullyQualifiedName!~MigrationSemantic04A02Tests'),('Legacy','all',None),('ControlMitigation','all',None),('OfficeAddin','all',None),('OfficeExecution','all',None),('Common','semantic','FullyQualifiedName~MigrationSemantic04A02Tests')]
else:
 selected='|'.join('FullyQualifiedName~'+x for x in ['GuiWiring47','GuiWiring46','Regression09','Regression10','Regression11','Gee12','Mixed14','Mixed15','Mixed16','Multivariate17','Multivariate18','Multivariate19','Multivariate20','Spc21','Spc22','Spc23','Spc24','Psm25','Survival13','AssumptionReport06','PairedBlockAdapter','ContingencyAdapter','WindowsHarness45','MigrationRefEditBoundary','DocumentLifecycleAdapter','ExecutionAdapter','InfrastructurePolicy44','Preference','Settings','Help','Diagnostic','Fault','Execution','Culture','Progress','NumberText'])
 suites=[('Core','affected',selected),('Common','affected',selected),('Legacy','affected',selected)]
def execute(spec):
 suite,part,selection=spec;name='a47-final-'+config+'-'+suite+'-'+part
 result=run.test(config,label,suite,name,selection);save(PRIVATE/(name+'.json'),result)
 return dict(suite=suite,part=part,selection=selection,name=name,outcomes=result['outcomes'],cases=pin(PRIVATE/'executions'/name/'cases.json'),TRX=pin(PRIVATE/'executions'/name/'results'/(suite+'.trx')),raw=pin(PRIVATE/(name+'.json')),command_exit=result['command']['exit_code'])
if config=='Debug':
 def lane(specs):return [execute(s) for s in specs]
 with ThreadPoolExecutor(max_workers=3) as pool:batches=list(pool.map(lane,[[suites[0],suites[1]],[suites[2],suites[3]],[suites[4],suites[5]]]))
 parts=[p for batch in batches for p in batch]+[execute(suites[6])]
else:parts=[execute(s) for s in suites]
totals=Counter()
for p in parts:totals.update(p['outcomes'])
assert build['source_pins']==run.__class__.build.__globals__['source_pins']()
save('build-and-tests/'+config+'-executed-v1.json',dict(configuration=config,mode='ONE_FULL_FRESH_CAMPAIGN' if config=='Debug' else 'TARGETED_AFFECTED_CONTRACTS;FULL_RELEASE_NOT_RUN',parts=parts,outcomes=dict(totals),identities=sum(totals.values()),source_unchanged=True,full_Release='NOT_RUN',max_parallel_test_hosts=3 if config=='Debug' else 1))
print(config,'campaign',dict(totals),flush=True)
