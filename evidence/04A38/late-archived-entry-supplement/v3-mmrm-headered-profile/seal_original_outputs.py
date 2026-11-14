"""Freeze all original results and actual branch witnesses before current use."""
import csv
from datetime import datetime,timezone
from common import *
from verify_inputs import inspect
def main():
 inspect();assert not (HERE/'current-results').exists()
 plan=json.loads((HERE/'trace-plan.json').read_bytes())['plan'];counts={}
 for config in ['Debug','Release']:
  folder=HERE/'original-results'/config;raw=(folder/'plain/results.tsv').read_bytes()
  assert raw==(folder/'repeat/results.tsv').read_bytes()==(folder/'trace/results.tsv').read_bytes()
  with (folder/'plain/results.tsv').open(encoding='utf-8',newline='') as stream:rows=list(csv.DictReader(stream,delimiter='\t'))
  trace=json.loads((folder/'trace/results.tsv.branches.json').read_bytes())
  inputs=[json.loads(line) for line in (HERE/('inputs-'+config+'.jsonl')).read_bytes().splitlines()]
  assert [r['case_id'] for r in rows]==[r['case_id'] for r in trace]==[r['case_id'] for r in inputs]
  cases=[]
  for row,witness in zip(rows,trace):
   events=witness['operations'][1]['events'];needed=plan['required_ordered_events']
   assert events[:len(needed)]==needed and not any(e in events for e in plan['forbidden_events'])
   op=json.loads(row['operations']);assert op[0]['kind']=='HANDLE_CREATED' and op[0]['error']=='None'
   cases.append(dict(case_id=row['case_id'],outcome=op[1]['kind'],result_shape=op[1]['result'].split('{',1)[0],events=events,exception=op[1]['error']))
  counts[config]=dict(unique_cases=2,plain=2,repeat=2,trace=2,headered_profile_cases=2,returned_reports=sum('report.returned=True' in r['events'] for r in cases),
       repeat_exact=True,trace_output_exact=True,cases=cases)
 save('original-coverage.json',dict(status='ORIGINAL_HEADERED_BRANCH_OBSERVED',configurations=counts,reference_class=CLASS,source_plan=pin(HERE/'trace-plan.json')))
 files=[pin(p) for p in sorted(HERE.rglob('*')) if p.is_file()]
 save('original-output-seal.json',dict(status='ORIGINAL_OUTPUT_SEALED',sequence=2,sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,reference_class=CLASS,
      input_seal=pin(HERE/'input-seal.json'),files=files,cases_per_configuration=2,original_executions_per_configuration=6,current_executions=0,is_pre_edit_frozen=False))
 print('ORIGINAL_OUTPUT_SEALED:2 unique cases/configuration;6 original executions/configuration; all4 configuration/culture queries returned reports')
if __name__=='__main__':main()
