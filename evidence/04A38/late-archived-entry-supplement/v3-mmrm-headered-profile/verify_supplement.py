"""Independent source/input/runtime/branch/chronology/exact-result acceptance.

No producer or current implementation module is imported. This reader only
certifies the two-case MMRM evidence gate, never whole-batch READY.
"""
import csv,re,json
from datetime import datetime
from verify_inputs import ROOT,PREFIX,BASE,InvalidEvidence,need,load,digest,check_pin,inspect as inputs
from pathlib import Path
SEAL='0cd8079316509a4e5a24f9164569b0ff7702c04e314970a866d36fddc6b96fdb'
PREVIOUS='evidence/04A38/late-archived-entry-supplement/v2-gee-profile/publication-manifest-closeout-v1.json'
NAMES=['Intercept','treatment_active','visit2','visit3','treatment_active:visit2','treatment_active:visit3']
EVENTS=['spec.accepted='+'|'.join(NAMES),'parser.entry=True','headerless.attempted=False','header.name_col=0','header.value_col=1','branch.headered_name_value=True',
 'headered.row=1','helper.entry=treatment_active','helper.value=D3FF0000000000000','selection.design=treatment_active','headered.row=2','helper.entry=visit',
 'helper.value=D3FF0000000000000','selection.visit=visit','selection.visit_value=D3FF0000000000000','profile.formed=True','profile.applied=True',
 'row_selection.enter=True','row_selection.matched=9','contrast.report_enter=True','report.returned=True']
def time(s):return datetime.fromisoformat(s.replace('Z','+00:00'))

def inspect(root=ROOT):
 root=Path(root);here=root/PREFIX;inputs(root)
 need(digest((here/'original-output-seal.json').read_bytes())==SEAL,'Original-output seal changed/unsourced')
 seal=load(here/'original-output-seal.json');need(seal['status']=='ORIGINAL_OUTPUT_SEALED' and seal['sequence']==2 and seal['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT' and seal['source_commit']==BASE,'Wrong original seal/class/source')
 need(seal['current_executions']==0 and seal['is_pre_edit_frozen'] is False,'Postcomparison/pre-edit relabeling')
 for row in seal['files']:check_pin(root,row)
 previous=load(root/PREVIOUS)
 entry=load(here/'entry-inspection.json');check_pin(root,entry['predecessor_manifest'])
 for row in previous['entries']:
  if row['sha256'] is not None:check_pin(root,row)
 input_time=time(load(here/'input-seal.json')['sealed_utc']);output_time=time(seal['sealed_utc']);need(input_time<output_time,'Broken seal order')
 results={}
 for config in ['Debug','Release']:
  original=here/'original-results'/config;current=here/'current-results'/config
  raw=(original/'plain/results.tsv').read_bytes()
  need(raw==(original/'repeat/results.tsv').read_bytes()==(original/'trace/results.tsv').read_bytes()==(current/'results.tsv').read_bytes(),'Result mismatch/partial match/reordering/normalization')
  need((original/'trace/results.tsv.branches.json').read_bytes()==(current/'results.tsv.branches.json').read_bytes(),'Original/current actual traces differ')
  lines=(here/('inputs-'+config+'.jsonl')).read_bytes().splitlines(keepends=True)
  with (original/'plain/results.tsv').open(encoding='utf-8',newline='') as stream:rows=list(csv.DictReader(stream,delimiter='\t'))
  expected=[load_case['case_id'] for load_case in map(json.loads,lines)]
  traces=load(original/'trace/results.tsv.branches.json');need([r['case_id'] for r in rows]==[r['case_id'] for r in traces]==expected,'Lost/extra/reordered case')
  current_receipt=load(here/('current-execution-'+config+'.json'))
  need(current_receipt['role']=='current' and current_receipt['original_output_seal']['sha256']==SEAL,'Current missing source oracle/seal')
  for row in current_receipt['files']:check_pin(root,row)
  compiled={mode:{r['name']:r for r in load(here/('compile-original-'+config+'-'+mode+'.json'))['assemblies']} for mode in ['plain','trace']}
  for mode in ['plain','repeat','trace']:
   runtime=load(original/mode/'execution.json')
   need(runtime['role']=='original' and runtime['cases']==2 and runtime['configuration']==config and runtime['source_commit']==BASE and runtime['native_attempts']==0,'Original runtime/source/count/native mismatch')
   need(runtime['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT','Original capture relabeled')
   need(input_time<time(runtime['started_utc'])<=time(runtime['completed_utc'])<output_time,'Original result created outside sealed chronology')
   allowed=compiled['trace' if mode=='trace' else 'plain']
   for assembly in runtime['assemblies']:
    need(assembly['name']+'.dll' in allowed,'Foreign/new original assembly')
    expected_assembly=allowed[assembly['name']+'.dll'];need((assembly['bytes'],assembly['sha256'])==(expected_assembly['bytes'],expected_assembly['sha256']) and re.fullmatch('[0-9a-f-]{36}',assembly['mvid']),'Wrong original module identity')
  runtime=load(current/'execution.json');need(runtime['role']=='current' and runtime['cases']==2 and runtime['configuration']==config and runtime['native_attempts']==0,'Current runtime/count/native mismatch')
  need(time(runtime['started_utc'])>output_time,'Current preceded original seal')
  old_sessions=load(original/'plain/results.tsv.sessions.json');new_sessions=load(current/'results.tsv.sessions.json')
  need(len(old_sessions)==len(new_sessions)==2,'Lost model session context')
  cases=[]
  for i,(row,trace,old_session,new_session) in enumerate(zip(rows,traces,old_sessions,new_sessions)):
   need(row['input_sha256']==digest(lines[i]),'Runtime input not sealed bytes')
   need(trace['culture']==('' if i==0 else 'sk-SK') and trace['operations'][1]['events']==EVENTS,'Missing/forged headered branch witness, headerless/wide/early guard substituted')
   operations=json.loads(row['operations']);need(len(operations)==2,'Additional operation')
   fit,query=operations
   need(fit['target']=='BESH.REGR.MMRM_FIT' and fit['kind']=='HANDLE_CREATED' and fit['result']=='HANDLE:MMRM:SESSION1','Not genuine successful fitted handle')
   need(query['target']=='BESH.REGR.MMRM_LSMESTIMATE' and query['kind']=='RETURN_ARRAY' and query['result'].startswith('System.Object[0:5,0:8]{'),'Wrong result type/bounds/shape/report')
   for op in operations:need(op['phase']=='RETURNED_FROM_SOURCE' and op['error']=='None' and op['caught_errors']==[] and op['input']==op['byref_arguments'],'Changed exception/ByRef/side effects')
   for session in [old_session,new_session]:
    need(session['case_id']==row['case_id'] and session['parameter_names']==NAMES and session['creation_order']==1,'Lost fitted identity/design provenance')
    need(re.fullmatch('MMRM:[0-9a-f]{32}',session['raw_handle']) and session['symbol']=='HANDLE:MMRM:SESSION1','Synthetic/unbound handle normalization')
   need({k:v for k,v in old_session.items() if k!='raw_handle'}=={k:v for k,v in new_session.items() if k!='raw_handle'},'Fitted context/source rows/IEEE bits differ')
   query_bytes=json.dumps(query,ensure_ascii=False,separators=(',',':')).encode()
   cases.append(dict(case_id=row['case_id'],culture='' if i==0 else 'sk-SK',exact=True,actual_headered_branch=True,returned_report=True,
       outcome='RETURN_ARRAY',shape='System.Object[0:5,0:8]',input_sha256=row['input_sha256'],original_query_sha256=digest(query_bytes),current_query_sha256=digest(query_bytes),events=EVENTS))
  results[config]=dict(unique_cases=2,exact=2,headered_profile_witnesses=2,returned_reports=2,original_executions=6,current_executions=2,
       result_bytes=len(raw),original_sha256=digest(raw),current_sha256=digest((current/'results.tsv').read_bytes()),cases=cases)
 return dict(status='MMRM_HEADERED_PROFILE_EVIDENCE_PASS_NOT_BATCH_READY',configurations=results,reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',
      distinct_components_per_configuration=dict(genuine_preedit=28966,late432=432,lateGEE=12,lateMMRM=2),scientific='NOT_GOLDEN',native='NOT_RUN')
if __name__=='__main__':
 import json
 try:print(json.dumps(inspect()))
 except (InvalidEvidence,OSError,KeyError,ValueError,TypeError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
