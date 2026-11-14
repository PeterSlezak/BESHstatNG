"""One comprehensive post-freeze Debug campaign, preserving every identity/nonpass."""
from common import *
from collections import Counter
LABEL='w2c17-checkpoint-v1'
freeze=json.loads((HERE/'production-test-freeze-v4.json').read_bytes())
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
compat=json.loads((HERE/'compatibility-combined-v4.json').read_bytes())
assert compat['all_exact'] and compat['totals_per_configuration']=={'Debug':29424}
runner=m.Runner(RUNROOT);before=m.source_pins();start=time.monotonic()
build=runner.build('Debug',LABEL)
save('managed-Debug-build-v1.json',dict(build=clean(build),raw=pin(RUNROOT/'builds'/LABEL/'Debug/build.json'),source_freeze=pin(HERE/'production-test-freeze-v4.json'),tests_started=0,Release=DEFERRED,original_queries=0))
assert not any(build['exit_codes']),'Build failed before any managed parent starts; retain and version correction'
results=[];totals=Counter()
for suite,part,selection in [('Core','all',None),('Common','main','FullyQualifiedName!~MigrationSemantic04A02Tests'),('Common','semantic','FullyQualifiedName~MigrationSemantic04A02Tests'),('Legacy','all',None),('ControlMitigation','all',None),('OfficeAddin','all',None),('OfficeExecution','all',None)]:
 name=LABEL+'-Debug-serial-'+suite+('-'+part if suite=='Common' else '')
 result=runner.test('Debug',LABEL,suite,name,selection);results.append(result);totals.update(result['outcomes'])
 save('managed-executed-'+suite+'-'+part+'-v1.json',dict(suite=suite,part=part,selection=selection,outcomes=result['outcomes'],trx_sha256=result['sha256'],command=clean(result['command']),raw_cases=pin(RUNROOT/'executions'/name/'cases.json')))
raw=PRIVATE/'full-managed-Debug-v1.json'
with raw.open('x',encoding='utf-8',newline='\n') as f:json.dump(dict(build=clean(build),results=clean(results),source_unchanged=before==m.source_pins()),f,indent=2,ensure_ascii=False);f.write('\n')
assert before==m.source_pins()
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
save('managed-full-execution-v1.json',dict(status='ONE_COMPLETE_POST_FREEZE_DEBUG_CAMPAIGN_EXECUTED_WITH_GENUINE_OUTCOMES',raw=pin(raw),seconds=time.monotonic()-start,source_unchanged=True,outcomes=dict(totals),source_freeze=pin(HERE/'production-test-freeze-v4.json'),campaigns=1,Common_partition='Two disjoint complementary filters; existing observation directory only for the semantic class.',Release_compilation=DEFERRED,Release_full_selected_focused_unit_tests=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,original_queries=0))
print('Managed Debug campaign completed',dict(totals),'actual outcomes; inherited/new reconciliation follows',flush=True)
