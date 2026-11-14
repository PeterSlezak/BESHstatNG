from common import *
from collections import Counter
plan=json.loads((HERE/'managed-execution-path-map-v1.json').read_bytes());freeze=json.loads((HERE/'production-test-freeze-v1.json').read_bytes());runner=m.Runner(RUNROOT);before=m.source_pins();results=[];totals=Counter();start=time.monotonic()
for r in freeze['sources']:assert pin(ROOT/r['path'])==r
for row in plan['parts']:
 name=row['name'];suite=row['suite'];part=row['part']
 if row['state']=='COMPLETED':result=json.loads((RUNROOT/'executions'/name/'cases.json').read_bytes())
 else:
  assert not (RUNROOT/'executions'/name).exists() and not (RUNROOT/'commands'/name).exists()
  result=runner.test('Debug',plan['build_label'],suite,name,row['selection'])
  save('managed-executed-'+suite+'-'+part+'-v1.json',dict(suite=suite,part=part,selection=row['selection'],outcomes=result['outcomes'],trx_sha256=result['sha256'],command=clean(result['command']),raw_cases=pin(RUNROOT/'executions'/name/'cases.json')))
 results.append(result);totals.update(result['outcomes'])
assert before==m.source_pins()
for r in freeze['sources']:assert pin(ROOT/r['path'])==r
raw=PRIVATE/'full-managed-Debug-v1.json'
with raw.open('x',encoding='utf-8',newline='\n') as f:json.dump(dict(results=clean(results),source_unchanged=True),f,indent=2,ensure_ascii=False);f.write('\n')
save('managed-full-execution-v1.json',dict(status='ONE_REPLACEMENT_COMPREHENSIVE_DEBUG_CAMPAIGN_COMPLETED',outcomes=dict(totals),raw=pin(raw),seconds_continuation=time.monotonic()-start,campaigns=1,source_unchanged=True,source_freeze=pin(HERE/'production-test-freeze-v1.json'),execution_path_map=pin(HERE/'managed-execution-path-map-v1.json'),prelaunch_copy_failure_retained=True,completed_test_repeats=0,Release=DEFERRED,original_queries=0))
print('Single managed campaign completed',dict(totals),flush=True)
