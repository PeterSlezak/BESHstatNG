"""Seal authentic observed F results before any F current compilation."""
import base64,csv
from datetime import datetime,timezone
from common import *
from verify_inputs import inspect
from verify_original_source import inspect as source
def main():
 inspect();save('original-source-verification.json',source());assert not (HERE/'current-results').exists() and not list(PRIVATE.glob('current-*'))
 configurations={}
 for config in ['Debug','Release']:
  folder=HERE/'original-results'/config;raw=(folder/'plain/results.tsv').read_bytes();assert raw==(folder/'repeat/results.tsv').read_bytes()==(folder/'trace/results.tsv').read_bytes()
  with (folder/'plain/results.tsv').open(encoding='utf-8',newline='') as stream:rows=list(csv.DictReader(stream,delimiter='\t'))
  traces=json.loads((folder/'trace/results.tsv.branches.json').read_bytes());cases=[]
  assert len(rows)==len(traces)==2
  for row,t in zip(rows,traces):
   assert row['case_id']==t['case_id'];events=t['operations'][1]['events'];ops=json.loads(row['operations']);actual=ops[1]
   assert ops[0]['kind']=='HANDLE_CREATED' and actual['kind']=='RETURN_TEXT' and actual['error']=='None'
   assert all(e in events for e in ['public.scalar_group=Intercept','lookup.index=0','lookup.resolved_name=Intercept','guard.intercept=True','initialized.resolved_name=N','initialized.values=N','initialized.error=N','guard.resolved_name=N','guard.values=N','public.resolved_name=N','public.values=N','public.group_rejected=True'])
   assert events.index('lookup.resolved_name=Intercept')<events.index('guard.intercept=True')<events.index('public.group_rejected=True') and 'public.range=True' not in events and 'report.continued=True' not in events
   text=base64.b64decode(actual['result'][1:]).decode();error=next(e.split('=',1)[1] for e in events if e.startswith('guard.error_assigned='));assert text=='BESH.REGR.MMRM_LSMEANS error: '+error
   cases.append(dict(case_id=row['case_id'],events=events,outcome=actual['kind'],result=actual['result'],decoded_return_text=text,observed_index=0,resolved_fitted_column='Intercept',
    private_ByRef=dict(resolvedName=None,values=None,errorMessage=error,observation='TRACE_ONLY_INITIALIZED_AND_RETURNED_STATE'),original_guard_lines=[1477,1478,1479,1480],returned_false_proof='Immutable original next statement is ReturnFalse at1480; actual public Not TryGetDesignColumnValues arm executes'))
  configurations[config]=dict(unique_cases=2,plain_executions=2,repeat_executions=2,trace_executions=2,intercept_guard_cases=2,returned_reports=0,repeat_exact=True,trace_output_exact=True,cases=cases)
 save('original-coverage.json',dict(status='ORIGINAL_INTERCEPT_TRUE_GUARD_AND_BYREF_OBSERVED',reference_class=CLASS,configurations=configurations))
 save('materialization-wording-qualification.json',dict(note='seal_inputs.py retained inherited console wording typed arrays; actual F group is scalar System.String. CLR materialization and independent fixed-type reader, not that console wording, establish the sealed input type.',input_packet_modified=False))
 save('original-output-seal.json',dict(status='ORIGINAL_OUTPUT_SEALED',sequence=2,sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,reference_class=CLASS,input_seal=pin(HERE/'input-seal.json'),files=[pin(p) for p in sorted(HERE.rglob('*')) if p.is_file()],cases_per_configuration=2,original_executions_per_configuration=6,current_executions=0,current_compilation='NOT_STARTED',is_pre_edit_frozen=False))
 print('ORIGINAL_OUTPUT_SEALED: all4 intended intercept true guards and public outcomes; current compilation not started')
if __name__=='__main__':main()
