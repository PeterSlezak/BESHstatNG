"""Seal complete original evidence before any v4 current compilation/execution."""
import csv,base64
from datetime import datetime,timezone
from common import *
from verify_inputs import inspect
def main():
 inspect();assert not (HERE/'current-results').exists() and not list(PRIVATE.glob('current-*'))
 configurations={}
 for config in ['Debug','Release']:
  folder=HERE/'original-results'/config;raw=(folder/'plain/results.tsv').read_bytes()
  assert raw==(folder/'repeat/results.tsv').read_bytes()==(folder/'trace/results.tsv').read_bytes()
  with (folder/'plain/results.tsv').open(encoding='utf-8',newline='') as stream:rows=list(csv.DictReader(stream,delimiter='\t'))
  traces=json.loads((folder/'trace/results.tsv.branches.json').read_bytes());inputs=[json.loads(line) for line in (HERE/('inputs-'+config+'.jsonl')).read_bytes().splitlines()]
  assert [r['case_id'] for r in rows]==[r['case_id'] for r in traces]==[r['case_id'] for r in inputs]
  cases=[]
  for row,trace in zip(rows,traces):
   events=trace['operations'][1]['events'];kind=row['case_id'].rsplit('/',1)[1]
   assert 'branch.headered_name_value=True' in events and 'headerless.attempted=False' in events
   assert not any(e in events for e in ['branch.headerless_success=True','branch.wide=True','profile.formed=True','profile.applied=True','report.returned=True'])
   if kind=='duplicate-visit-time':
    assert events.index('selection.visit_value=D3FF0000000000000')<events.index('visit.previously_specified=True')<events.index('guard.duplicate_visit_time=True')
   else:
    assert events.count('design.resolved_index=1')==2 and events.index('selection.design=treatment_active')<events.index('design.existing_count=1')<events.index('guard.duplicate_design=True')
   ops=json.loads(row['operations']);assert ops[0]['kind']=='HANDLE_CREATED' and ops[0]['error']=='None'
   actual=ops[1];assert actual['kind']=='RETURN_TEXT' and actual['error']=='None'
   cases.append(dict(case_id=row['case_id'],kind=kind,events=events,outcome=actual['kind'],result=actual['result'],decoded_return_text=base64.b64decode(actual['result'][1:]).decode()))
  configurations[config]=dict(unique_cases=4,plain_executions=4,repeat_executions=4,trace_executions=4,guard_cases=4,visit_time_guard_cases=2,design_guard_cases=2,returned_reports=0,repeat_exact=True,trace_output_exact=True,cases=cases)
 save('original-coverage.json',dict(status='ORIGINAL_TRUE_DUPLICATE_GUARDS_OBSERVED',reference_class=CLASS,configurations=configurations))
 files=[pin(p) for p in sorted(HERE.rglob('*')) if p.is_file()]
 save('original-output-seal.json',dict(status='ORIGINAL_OUTPUT_SEALED',sequence=2,sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,reference_class=CLASS,
      input_seal=pin(HERE/'input-seal.json'),files=files,cases_per_configuration=4,original_executions_per_configuration=12,current_executions=0,current_compilation='NOT_STARTED',is_pre_edit_frozen=False))
 print('ORIGINAL_OUTPUT_SEALED:4 unique cases/configuration;12 original executions/configuration;all8 case/configuration results are actual intended guard text returns; current not compiled')
if __name__=='__main__':main()
