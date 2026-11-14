"""Seal observed original public outcomes and private mask witnesses before current."""
import csv,base64
from datetime import datetime,timezone
from common import *
from verify_inputs import inspect
from verify_original_source import inspect as source
def main():
 inspect();proof=source();assert not (HERE/'current-results').exists() and not list(PRIVATE.glob('current-*'))
 save('original-source-verification.json',proof)
 configurations={}
 for config in ['Debug','Release']:
  folder=HERE/'original-results'/config;raw=(folder/'plain/results.tsv').read_bytes()
  assert raw==(folder/'repeat/results.tsv').read_bytes()==(folder/'trace/results.tsv').read_bytes()
  with (folder/'plain/results.tsv').open(encoding='utf-8',newline='') as stream:rows=list(csv.DictReader(stream,delimiter='\t'))
  traces=json.loads((folder/'trace/results.tsv.branches.json').read_bytes());inputs=[json.loads(line) for line in (HERE/('inputs-'+config+'.jsonl')).read_bytes().splitlines()]
  assert [r['case_id'] for r in rows]==[r['case_id'] for r in traces]==[r['case_id'] for r in inputs] and len(rows)==2
  cases=[]
  for row,trace in zip(rows,traces):
   events=trace['operations'][1]['events'];mask='System.Boolean[0:54]{'+'|'.join(['B0']*54)+'}'
   assert all(e in events for e in ['public.range=True','public.alpha_missing=True','branch.wide=True','headerless.attempted=False','profile.formed=True','mask.rows=54','mask.target_visit=D4058C00000000000','guard.unmatched=True','guard.matched_count=0','guard.mask='+mask,'public.mask='+mask,'guard.description=N','public.description=N','public.mask_rejected=True'])
   assert 'branch.headered_name_value=True' not in events and 'report.continued=True' not in events
   for i in range(54):
    a=events.index('row.index='+str(i));assert events[a+1:a+3]==['row.visit=D'+['3FF0000000000000','4000000000000000','4008000000000000'][i%3],'row.keep=False']
   assert events.index('row.index=53')<events.index('guard.unmatched=True')<events.index('public.mask_rejected=True')
   ops=json.loads(row['operations']);assert ops[0]['kind']=='HANDLE_CREATED' and ops[0]['error']=='None'
   actual=ops[1];assert actual['kind']=='RETURN_TEXT' and actual['error']=='None'
   observed=base64.b64decode(actual['result'][1:]).decode()
   error=next(e[len('guard.error='):] for e in events if e.startswith('guard.error='))
   assert observed=='BESH.REGR.MMRM_LSMEANS error: '+error
   cases.append(dict(case_id=row['case_id'],events=events,outcome=actual['kind'],result=actual['result'],decoded_return_text=observed,
    original_true_branch_line=1426,mask_observation='TRACE_ONLY_PRIVATE_BYREF',rowMask=dict(runtime_type='System.Boolean[]',rank=1,lower_bounds=[0],lengths=[54],upper_bounds=[53],values=[False]*54),matchedCount=0,profileDescription=None,errorMessage=error))
  configurations[config]=dict(unique_cases=2,plain_executions=2,repeat_executions=2,trace_executions=2,unmatched_guard_cases=2,returned_reports=0,repeat_exact=True,trace_output_exact=True,cases=cases)
 save('original-coverage.json',dict(status='ORIGINAL_LINE1426_TRUE_ARM_AND_54_FALSE_MASK_OBSERVED',reference_class=CLASS,configurations=configurations))
 files=[pin(p) for p in sorted(HERE.rglob('*')) if p.is_file()]
 save('original-output-seal.json',dict(status='ORIGINAL_OUTPUT_SEALED',sequence=2,sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,reference_class=CLASS,input_seal=pin(HERE/'input-seal.json'),files=files,cases_per_configuration=2,original_executions_per_configuration=6,current_executions=0,current_compilation='NOT_STARTED',is_pre_edit_frozen=False))
 print('ORIGINAL_OUTPUT_SEALED:2 unique cases/configuration;6 original executions/configuration;all4 original true-arm mask witnesses; current compilation not started')
if __name__=='__main__':main()
