"""One authorized replacement Debug campaign; offline compilation and disjoint suite executions."""
from common import *
from collections import Counter
import shutil
LABEL='w2c17pc1-replacement-v1'
mode=sys.argv[1]
freeze=json.loads((HERE/'production-test-freeze-v1.json').read_bytes())
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
assert json.loads((HERE/'entry-final-reader-authentication-v1.json').read_bytes())['all_exit_zero']
runner=m.Runner(RUNROOT)
if mode=='build':
 build=runner.build('Debug',LABEL)
 save('managed-Debug-build-v1.json',dict(build=clean(build),raw=pin(RUNROOT/'builds'/LABEL/'Debug/build.json'),source_freeze=pin(HERE/'production-test-freeze-v1.json'),tests_started=0,Release=DEFERRED,original_queries=0))
 assert not any(build['exit_codes']) and build['source_unchanged'],'Retain failed build; no tests started'
 lib=PRIVATE/'replacement-v1/Debug/lib';lib.mkdir(parents=True,exist_ok=False)
 for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(RUNROOT/'builds'/LABEL/'Debug/legacy'/n,lib/n)
 save('shipping-Debug-assemblies-v1.json',dict(assemblies=[pin(p) for p in sorted(lib.glob('*.dll'))],build=pin(HERE/'managed-Debug-build-v1.json'),source_freeze=pin(HERE/'production-test-freeze-v1.json')))
elif mode=='tests':
 assert json.loads((HERE/'compatibility-replacement-v1.json').read_bytes())['totals_per_configuration']=={'Debug':29424}
 before=m.source_pins();start=time.monotonic();results=[];totals=Counter()
 for suite,part,selection in [('Core','all',None),('Common','main','FullyQualifiedName!~MigrationSemantic04A02Tests'),('Common','semantic','FullyQualifiedName~MigrationSemantic04A02Tests'),('Legacy','all',None),('ControlMitigation','all',None),('OfficeAddin','all',None),('OfficeExecution','all',None)]:
  name=LABEL+'-Debug-serial-'+suite+('-'+part if suite=='Common' else '')
  result=runner.test('Debug',LABEL,suite,name,selection);results.append(result);totals.update(result['outcomes'])
  save('managed-executed-'+suite+'-'+part+'-v1.json',dict(suite=suite,part=part,selection=selection,outcomes=result['outcomes'],trx_sha256=result['sha256'],command=clean(result['command']),raw_cases=pin(RUNROOT/'executions'/name/'cases.json')))
 raw=PRIVATE/'full-managed-Debug-v1.json'
 with raw.open('x',encoding='utf-8',newline='\n') as f:json.dump(dict(results=clean(results),source_unchanged=before==m.source_pins()),f,indent=2,ensure_ascii=False);f.write('\n')
 assert before==m.source_pins()
 save('managed-full-execution-v1.json',dict(status='ONE_REPLACEMENT_COMPREHENSIVE_DEBUG_CAMPAIGN_EXECUTED',outcomes=dict(totals),raw=pin(raw),seconds=time.monotonic()-start,campaigns=1,source_unchanged=True,source_freeze=pin(HERE/'production-test-freeze-v1.json'),Release=DEFERRED,original_queries=0))
else:raise ValueError(mode)
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
print('Replacement Debug '+mode+' complete',flush=True)
