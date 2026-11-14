"""One comprehensive Debug campaign; disjoint Common environments, no Release tests."""
from common import *
from collections import Counter
LABEL='w2c10-checkpoint-v1'
runner=m.Runner(RUNROOT);before=m.source_pins();planned=json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'];pins=[pin(ROOT/p) for p in planned]
save('source-freeze-managed-v1.json',dict(sources=pins,focused=['focused-v3-packet-A-v1-Debug.json','focused-BC-packet-v2-Debug.json','focused-copy-corrective-v2.json'],frozen_comparison=pin(HERE/'compatibility-packet-v2.json'),original_queries=0))
start=time.monotonic();build=runner.build('Debug',LABEL);assert not any(build['exit_codes'])
results=[];totals=Counter()
for suite,part,selection in [('Core','all',None),('Common','main','FullyQualifiedName!~MigrationSemantic04A02Tests'),('Common','semantic','FullyQualifiedName~MigrationSemantic04A02Tests'),('Legacy','all',None),('ControlMitigation','all',None),('OfficeAddin','all',None),('OfficeExecution','all',None)]:
    name=LABEL+'-Debug-serial-'+suite+('-'+part if suite=='Common' else '')
    result=runner.test('Debug',LABEL,suite,name,selection);results.append(result);totals.update(result['outcomes'])
    save('managed-executed-'+suite+'-'+part+'-v1.json',dict(suite=suite,part=part,selection=selection,outcomes=result['outcomes'],trx_sha256=result['sha256'],command=clean(result['command']),raw_cases=pin(RUNROOT/'executions'/name/'cases.json')))
raw=PRIVATE/'full-managed-Debug-v1.json'
with raw.open('x',encoding='utf-8',newline='\n') as f:json.dump(dict(build=clean(build),results=clean(results),source_unchanged=before==m.source_pins()),f,ensure_ascii=False,indent=2);f.write('\n')
assert before==m.source_pins() and [pin(ROOT/p) for p in planned]==pins
save('managed-full-execution-v1.json',dict(raw=pin(raw),private=True,seconds=time.monotonic()-start,source_unchanged=True,first_full_outcomes=dict(totals),Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',Common_partition='Two complementary filters; each Common identity executes once. Only MigrationSemantic04A02Tests gets its existing isolated observation directory. Main Common tests retain the standard empty observation environment; this prevents the known C7 fixture contamination without assertion changes.',original_queries=0))
print(json.dumps(dict(first_full_outcomes=dict(totals),campaigns=1,Common_parts='DISJOINT_COMPLEMENTARY',Release_unit='NOT_RUN_OWNER_PREFERENCE')),flush=True)
