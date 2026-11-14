"""Seal all original plain/repeat/trace captures before any current execution."""
import csv,json
from datetime import datetime,timezone
from inspect_entry import HERE,BASE,CLASS,pin,save
from verify_inputs import inspect

def main():
 inspect();assert not (HERE/'current-results').exists()
 plan=json.loads((HERE/'trace-plan.json').read_bytes())['case_kinds']
 configurations={}
 for config in ['Debug','Release']:
  base=HERE/'original-results'/config
  raw=(base/'plain-v2/results.tsv').read_bytes()
  assert raw==(base/'repeat-v2/results.tsv').read_bytes()==(base/'trace-v2/results.tsv').read_bytes()
  with (base/'plain-v2/results.tsv').open(encoding='utf-8',newline='') as stream:results=list(csv.DictReader(stream,delimiter='\t'))
  traces=json.loads((base/'trace-v2/results.tsv.branches.json').read_bytes())
  inputs=[json.loads(line) for line in (HERE/('inputs-'+config+'.jsonl')).read_text(encoding='utf-8').splitlines()]
  assert [r['case_id'] for r in results]==[r['case_id'] for r in traces]==[r['case_id'] for r in inputs]
  cases=[]
  for row,trace in zip(results,traces):
   suffix=row['case_id'].split('/')[-1];required=plan[suffix]['required'];events=trace['operations'][1]['events']
   assert all(event in events for event in required),(row['case_id'],events,required)
   assert events[0]=='spec.accepted=Intercept|X1|X2' and events[1]=='parser.entry=True'
   if plan[suffix]['classification']=='POSITIVE_PROFILE_SELECTION':assert not any(e.startswith('guard.') for e in events)
   else:assert 'profile.formed=True' not in events and 'profile.applied=True' not in events
   operations=json.loads(row['operations']);assert operations[0]['kind']=='HANDLE_CREATED' and operations[0]['error']=='None'
   cases.append(dict(case_id=row['case_id'],witness_class=plan[suffix]['classification'],events=events,
                     outcome_class=operations[1]['kind'],result=operations[1]['result'],exception=operations[1]['error'],
                     later_statistical_error='later.no_matching_rows=True' in events,returned_report='report.returned=True' in events))
  configurations[config]=dict(unique_cases=12,positive_profile_selection=6,intended_guards=6,
       later_statistical_errors=sum(r['later_statistical_error'] for r in cases),returned_reports=sum(r['returned_report'] for r in cases),
       reference_executions=dict(plain=12,repeat=12,trace=12),repeat_exact=True,trace_output_exact=True,cases=cases)
 save('original-coverage.json',dict(status='ORIGINAL_REQUIRED_BRANCHES_OBSERVED',reference_class=CLASS,configurations=configurations,
      source_expectations_sealed_before_reference=pin(HERE/'trace-plan.json')))
 files=[]
 for path in sorted(HERE.rglob('*')):
  if path.is_file():files.append(pin(path))
 save('original-output-seal.json',dict(status='ORIGINAL_OUTPUT_SEALED',sequence=2,sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,
      reference_class=CLASS,files=files,input_seal=pin(HERE/'input-seal.json'),current_implementation_executed=False,
      is_pre_edit_frozen=False,cases_per_configuration=12,original_executions_per_configuration=36,
      cases_are_not_execution_multiplicity=True,scientific='NOT_GOLDEN',native='NOT_RUN'))
 print('ORIGINAL_OUTPUT_SEALED: each configuration12 unique cases,36 original executions;6 positive profiles,6 intended guards; no current executions')
if __name__=='__main__':main()
