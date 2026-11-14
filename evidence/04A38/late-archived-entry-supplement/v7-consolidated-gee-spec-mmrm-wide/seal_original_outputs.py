"""Seal all actually observed G original outcomes before any current build."""
import base64,csv
from datetime import datetime,timezone
from common import *
from verify_inputs import inspect
from verify_original_source import inspect as source
def main():
 inspect();save('original-source-verification.json',source());assert not (HERE/'current-results').exists() and not list(PRIVATE.glob('current-*'))
 reports={}
 for config in ['Debug','Release']:
  folder=HERE/'original-results'/config;raw=(folder/'plain/results.tsv').read_bytes();assert raw==(folder/'repeat/results.tsv').read_bytes()==(folder/'trace/results.tsv').read_bytes()
  with (folder/'plain/results.tsv').open(encoding='utf-8',newline='') as f:rows=list(csv.DictReader(f,delimiter='\t'))
  traces=json.loads((folder/'trace/results.tsv.branches.json').read_bytes());assert len(rows)==len(traces)==4;cases=[]
  for row,t in zip(rows,traces):
   assert row['case_id']==t['case_id'];events=t['operations'][1]['events'];ops=json.loads(row['operations']);assert ops[0]['kind']=='HANDLE_CREATED' and ops[1]['kind']=='RETURN_TEXT' and all(o['error']=='None' and o['caught_errors']==[] and o['input']==o['byref_arguments'] for o in ops)
   kind=row['case_id'].split('/')[-1]
   if kind=='GEE_SPEC_INTERCEPT_ROLE':
    assert all(e in events for e in ['G1.lookup.index=0','G1.lookup.name=Intercept','G1.guard.intercept=True','G1.guard.components_count=0','G1.public.components_count=0','G1.public.spec_rejected=True'])
    assert events.index('G1.lookup.name=Intercept')<events.index('G1.guard.intercept=True')<events.index('G1.public.spec_rejected=True');private=dict(components_type='List(Of GeeLsmEstimateComponent)',components_nonnull=True,components_count=0)
   else:
    assert all(e in events for e in ['G2.headerless.bad_precondition=False','G2.headerless.columns=1','G2.headerless.column_guard_true=True','G2.headerless.attempted=False','G2.branch.wide=True','G2.header.visit_col=0','G2.first_row.accepted=1','G2.wide.row=2','G2.guard.multirow=True','G2.public.atProfile_Nothing=True','G2.public.at_rejected=True'])
    assert events.index('G2.first_row.accepted=1')<events.index('G2.guard.multirow=True');private=dict(atProfile=None,attemptedHeaderless=False,accepted_dataRow=1,rejected_row=2)
   actual=base64.b64decode(ops[1]['result'][1:]).decode();error=next(e.split('=',1)[1] for e in events if '.guard.error=' in e)
   assert actual=='BESH.REGR.'+('GEE' if kind.startswith('GEE') else 'MMRM')+'_LSMESTIMATE error: '+error
   private['errorMessage']=error
   cases.append(dict(case_id=row['case_id'],kind=kind,events=events,public_kind='RETURN_TEXT',public_result=ops[1]['result'],decoded_result=actual,private_ByRef=private,returned_report=False))
  reports[config]=dict(unique_cases=4,original_executions=12,G1_true_guards=2,G2_true_guards=2,repeated_and_traced_public_bytes_exact=True,cases=cases)
 save('original-coverage.json',dict(status='ALL8_G_ORIGINAL_TRUE_GUARDS_AND_PRIVATE_STATE_OBSERVED',reference_class=CLASS,configurations=reports))
 save('original-output-seal.json',dict(status='ORIGINAL_OUTPUT_SEALED',sequence=2,sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,reference_class=CLASS,input_seal=pin(HERE/'input-seal.json'),files=[pin(p) for p in sorted(HERE.rglob('*')) if p.is_file()],cases_per_configuration=4,original_executions_per_configuration=12,current_executions=0,current_compilation='NOT_STARTED',is_pre_edit_frozen=False))
 print('ORIGINAL_OUTPUT_SEALED:8 original true guards;12 original executions/config; current compilation not started')
if __name__=='__main__':main()
