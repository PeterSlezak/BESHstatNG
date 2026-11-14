"""Independent exact public/private-mask, source, chronology and runtime verifier.

Observed original text below was fixed only AFTER original-output sealing.
No builder, capture producer or statistical implementation is imported.
"""
import base64,csv,json,re
from datetime import datetime
from pathlib import Path
from verify_inputs import ROOT,PREFIX,BASE,KINDS,InvalidEvidence,need,load,digest,check_pin,inspect as inputs
SEAL='f1913f11c89b9f812df5c8c6584af5cb65753012c2ded8b4e1a63ac7c88ba79d'
PREVIOUS='evidence/04A38/completion/w1/publication-manifest-v2.json'
ERRORS={'GEE_SPEC_INTERCEPT_ROLE': 'the intercept column should not be used as a profile column; it is supplied automatically by the fitted design.', 'MMRM_WIDE_MULTIROW_REJECTION': 'wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row.'}
EVENTS={'GEE_SPEC_INTERCEPT_ROLE': ['G1.spec.entry=True', 'G1.header.weight_col=1', 'G1.header.label_col=0', 'G1.lookup.index=0', 'G1.lookup.name=Intercept', 'G1.guard.intercept=True', 'G1.guard.components_count=0', 'G1.guard.error=the intercept column should not be used as a profile column; it is supplied automatically by the fitted design.', 'G1.public.error=the intercept column should not be used as a profile column; it is supplied automatically by the fitted design.', 'G1.public.components_count=0', 'G1.public.spec_rejected=True'], 'MMRM_WIDE_MULTIROW_REJECTION': ['G2.spec.accepted=True', 'G2.parser.entry=True', 'G2.headerless.bad_precondition=False', 'G2.headerless.columns=1', 'G2.headerless.column_guard_true=True', 'G2.headerless.attempted=False', 'G2.branch.wide=True', 'G2.header.visit_col=0', 'G2.wide.row=1', 'G2.wide.prior_data_row=-1', 'G2.first_row.accepted=1', 'G2.wide.row=2', 'G2.wide.prior_data_row=1', 'G2.guard.multirow=True', 'G2.guard.atProfile_Nothing=True', 'G2.guard.error=wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row.', 'G2.public.error=wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row.', 'G2.public.atProfile_Nothing=True', 'G2.public.at_rejected=True']}

def time(s):return datetime.fromisoformat(s.replace('Z','+00:00'))
def inspect(root=ROOT):
 root=Path(root);here=root/PREFIX;inputs(root)
 need(len(SEAL)==64 and digest((here/'original-output-seal.json').read_bytes())==SEAL,'Original output/source/chronology seal changed')
 seal=load(here/'original-output-seal.json')
 need(seal['status']=='ORIGINAL_OUTPUT_SEALED' and seal['sequence']==2 and seal['source_commit']==BASE and seal['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT','Wrong original seal/provenance')
 need(seal['current_executions']==0 and seal['current_compilation']=='NOT_STARTED' and seal['is_pre_edit_frozen'] is False,'Late/postcomparison seal or pre-edit relabel')
 for row in seal['files']:check_pin(root,row)
 entry=load(here/'entry-inspection.json');check_pin(root,entry['predecessor_manifest'])
 for row in load(root/PREVIOUS)['entries']:
  if row['sha256'] is not None:check_pin(root,row)
 input_time=time(load(here/'input-seal.json')['sealed_utc']);output_time=time(seal['sealed_utc']);need(input_time<output_time,'Broken input/output seal order')
 results={}
 for config in ['Debug','Release']:
  original=here/'original-results-v2'/config;current=here/'current-results'/config
  raw=(original/'plain/results.tsv').read_bytes()
  need(raw==(original/'repeat/results.tsv').read_bytes()==(original/'trace/results.tsv').read_bytes()==(current/'results.tsv').read_bytes(),'Full output bytes differ/omitted/reordered/normalized')
  need((original/'trace/results.tsv.branches.json').read_bytes()==(current/'results.tsv.branches.json').read_bytes(),'Actual original/current branch traces differ')
  lines=(here/('inputs-'+config+'.jsonl')).read_bytes().splitlines(keepends=True);cases=[json.loads(line) for line in lines]
  with (original/'plain/results.tsv').open(encoding='utf-8',newline='') as stream:rows=list(csv.DictReader(stream,delimiter='\t'))
  traces=load(original/'trace/results.tsv.branches.json')
  need([r['case_id'] for r in rows]==[r['case_id'] for r in traces]==[c['case_id'] for c in cases],'Missing/reordered/extra case IDs')
  current_receipt=load(here/('current-execution-'+config+'.json'))
  need(current_receipt['role']=='current' and current_receipt['original_output_seal']['sha256']==SEAL,'Current did not bind sealed original')
  for row in current_receipt['files']:check_pin(root,row)
  allowed={mode:{r['name']:r for r in load(here/('compile-original-'+config+'-'+mode+'.json'))['assemblies']} for mode in ['plain','trace']}
  all_sessions=[]
  for mode in ['plain','repeat','trace']:
   runtime=load(original/mode/'execution.json')
   need(runtime['role']=='original' and runtime['configuration']==config and runtime['cases']==4 and runtime['source_commit']==BASE and runtime['native_attempts']==0 and runtime['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT','Wrong original runtime/source/count/native boundary')
   need(input_time<time(runtime['started_utc'])<=time(runtime['completed_utc'])<output_time,'Original outside sealed chronology')
   expected=allowed['trace' if mode=='trace' else 'plain']
   for assembly in runtime['assemblies']:
    need(assembly['name']+'.dll' in expected,'Foreign original assembly')
    target=expected[assembly['name']+'.dll'];need((assembly['bytes'],assembly['sha256'])==(target['bytes'],target['sha256']) and re.fullmatch('[0-9a-f-]{36}',assembly['mvid']),'Wrong original loaded module')
   all_sessions.append(load(original/mode/'results.tsv.sessions.json'))
  runtime=load(current/'execution.json')
  need(runtime['role']=='current' and runtime['configuration']==config and runtime['cases']==4 and runtime['native_attempts']==0 and time(runtime['started_utc'])>output_time,'Current before seal or wrong runtime/count')
  current_allowed={r['name']:r for r in load(here/('compile-current-'+config+'-trace.json'))['assemblies']}
  for a in runtime['assemblies']:
   target=current_allowed[a['name']+'.dll'];need((a['bytes'],a['sha256'])==(target['bytes'],target['sha256']),'Wrong current loaded module')
  all_sessions.append(load(current/'results.tsv.sessions.json'));need(all(len(s)==4 for s in all_sessions),'Lost model/session context')
  for location in [original/'plain',original/'repeat',original/'trace',current]:
   restored=load(location/'results.tsv.cultures.json');need(len(restored)==4,'Missing culture restoration')
   for i,c in enumerate(restored):need(c['case_id']==cases[i]['case_id'] and c['applied_culture']==c['applied_ui_culture']==cases[i]['culture'] and c['original_culture']==c['restored_culture'] and c['original_ui_culture']==c['restored_ui_culture'],'Culture/UI scope changed')
  detail=[]
  for i,(case,row,trace) in enumerate(zip(cases,rows,traces)):
   kind=case['case_kind'];family='GEE' if kind==KINDS[0] else 'MMRM';error=ERRORS[kind];message='BESH.REGR.'+family+'_LSMESTIMATE error: '+error
   need(row['input_sha256']==digest(lines[i]),'Runtime input differs from sealed packet')
   need(trace['culture']==case['culture'] and len(trace['operations'])==2 and trace['operations'][0]['events']==[] and trace['operations'][1]['events']==EVENTS[kind],'Earlier guard/fabricated mask/wrong true branch/index/order')
   ops=json.loads(row['operations']);need(len(ops)==2,'Unapproved extra operation')
   fit,query=ops
   need(fit['target']=='BESH.REGR.'+family+'_FIT' and fit['kind']=='HANDLE_CREATED' and fit['result']=='HANDLE:'+family+':SESSION1','Not real successful original/current fit')
   need(query['target']=='BESH.REGR.'+family+'_LSMESTIMATE' and query['kind']=='RETURN_TEXT' and query['result']=='S'+base64.b64encode(message.encode()).decode(),'Wrong original/public guard outcome/type/text')
   for op in ops:need(op['phase']=='RETURNED_FROM_SOURCE' and op['error']=='None' and op['caught_errors']==[] and op['input']==op['byref_arguments'],'Changed argument/ByRef/exception side effects')
   sessions=[s[i] for s in all_sessions]
   for session in sessions:
    need(session['case_id']==case['case_id'] and session['parameter_names']==case['session']['expected_parameter_names'] and session['creation_order']==1 and session['symbol']=='HANDLE:'+family+':SESSION1','Lost real fixed-design/session provenance')
    need(re.fullmatch(family+':[0-9a-f]{32}',session['raw_handle']),'Synthetic handle')
    need(session['family']==family and session['cached_result_nonnull'] is True,'Failed/foreign original fitted model')
    need(session['fitted_design'].startswith('System.Double[0:36,0:3]{' if family=='GEE' else 'System.Double[0:54,0:6]{'),'Wrong fitted design shape')
    if family=='MMRM':need(session['fitted_time']=='System.Double[0:54]{'+'|'.join(['D'+['3FF0000000000000','4000000000000000','4008000000000000'][j%3] for j in range(54)])+'}','Actual saved visits differ')
    else:need(session['fitted_time']=='N','GEE factory time changed')
   comparable=[{k:v for k,v in s.items() if k!='raw_handle'} for s in sessions];need(all(s==comparable[0] for s in comparable),'Fitted matrix/visits/source rows/IEEE bits differ across original repeats/current')
   qb=json.dumps(query,ensure_ascii=False,separators=(',',':')).encode()
   detail.append(dict(case_id=case['case_id'],culture=case['culture'],kind=kind,exact=True,true_guard_original_lines=([1506,1507,1508,1509] if family=='GEE' else [592,593,594]),outcome='RETURN_TEXT',returned_report=False,input_sha256=row['input_sha256'],original_query_sha256=digest(qb),current_query_sha256=digest(qb),decoded_result=message,events=EVENTS[kind],private_ByRef=(dict(components_nonnull=True,components_count=0,errorMessage=error) if family=='GEE' else dict(atProfile=None,errorMessage=error))))
  results[config]=dict(unique_cases=4,exact=4,G1_true_guard_cases=2,G2_true_guard_cases=2,returned_reports=0,total_original_executions=24,accepted_original_executions=12,current_executions=4,result_bytes=len(raw),original_sha256=digest(raw),current_sha256=digest((current/'results.tsv').read_bytes()),cases=detail)
 report=dict(status='APPROVAL_G_EVIDENCE_PASS_NOT_BATCH_READY',configurations=results,reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',distinct_components_per_configuration=dict(genuine_preedit=28966,A=432,B=12,C=2,D=4,E=2,F=2,G=4),native='NOT_RUN',scientific='NOT_GOLDEN')
 if (here/'compatibility-comparison.json').exists():need(load(here/'compatibility-comparison.json')==report,'Producer comparison differs from independent byte/branch analysis')
 return report
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except (ValueError,OSError,KeyError,TypeError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
