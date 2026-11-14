"""Independent acceptance reader for the bounded GEE evidence, never batch READY.

Only stdlib and a separate independent input reader are used. No producer,
capture harness, compilation driver, extraction code or coverage report decides
acceptance. Actual sealed runtime traces/results are inspected directly.
"""
import base64,csv,hashlib,json,re
from datetime import datetime
from pathlib import Path,PurePosixPath
from verify_inputs_public import ROOT,PREFIX,BASE,InvalidEvidence,need,load,digest,inspect as inspect_inputs
from environment_control_reader import inspect as inspect_environment,PRIVATE as PRIVATE_ENVIRONMENT
SEAL='a87736973eb6cee5b76b1fe95dd508de679c75ceffde3dd93647a047a9f63b98'
PREVIOUS='evidence/04A38/late-archived-entry-supplement/v1/publication-manifest.json'
PREVIOUS_HASH='0265ab6b17597faaeec79f846c484b7097824a519b73303e9ab8e37b042663eb'
SOURCE='35a3a35ddf4dfdb074a082dfe92495969dd1a8532f86af6650d6bde33bcf7649'
SELF=PREFIX+'publication-manifest.json'
START=['spec.accepted=Intercept|X1|X2','parser.entry=True']
REQUIRED={
 'wide-time':START+['parser.wide=True','helper.entry=time','selection.time=time','selection.time_value=D3FF0000000000000','profile.formed=True','profile.applied=True','later.no_matching_rows=True'],
 'name-value-time':START+['parser.name_value=True','helper.entry=time','selection.time=time','selection.time_value=D3FF0000000000000','profile.formed=True','profile.applied=True','later.no_matching_rows=True'],
 'name-value-visit':START+['parser.name_value=True','helper.entry=visit','selection.time=visit','selection.time_value=D3FF0000000000000','profile.formed=True','profile.applied=True','later.no_matching_rows=True'],
 'duplicate-time-visit':START+['parser.name_value=True','helper.entry=time','selection.time=time','selection.time_value=D3FF0000000000000','parser.name_value=True','helper.entry=visit','guard.duplicate_time=visit'],
 'duplicate-design':START+['parser.name_value=True','helper.entry=X1','selection.design=X1','parser.name_value=True','helper.entry=x1','guard.duplicate_design=X1'],
 'wide-multiple-rows':START+['parser.wide=True','guard.wide_multiple_rows=True']}
MESSAGES={
 'wide-time':"no observed design rows matched profile for label 'L'.",
 'name-value-time':"no observed design rows matched profile for label 'L'.",
 'name-value-visit':"no observed design rows matched profile for label 'L'.",
 'duplicate-time-visit':'at specifies time more than once.',
 'duplicate-design':'at specifies design column "X1" more than once.',
 'wide-multiple-rows':'wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row.'}

def check_pin(root,row):
 if row['path']==PRIVATE_ENVIRONMENT:
  inspect_environment(root)
  if not (root/PRIVATE_ENVIRONMENT).exists():return
 data=(root/row['path']).read_bytes()
 need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Changed payload: '+row['path'])

def timestamp(value):return datetime.fromisoformat(value.replace('Z','+00:00'))

def inspect(root=ROOT,publication=False):
 root=Path(root);here=root/PREFIX;inspect_inputs(root)
 need(digest((here/'original-output-seal.json').read_bytes())==SEAL,'Changed original output seal/source/chronology')
 seal=load(here/'original-output-seal.json')
 need(seal['status']=='ORIGINAL_OUTPUT_SEALED' and seal['sequence']==2 and seal['source_commit']==BASE,'Unsealed/unsourced original')
 need(seal['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT' and seal['is_pre_edit_frozen'] is False and seal['current_implementation_executed'] is False,'Misrepresented chronology')
 for row in seal['files']:check_pin(root,row)
 need(digest((root/PREVIOUS).read_bytes())==PREVIOUS_HASH,'Older manifest changed')
 previous=load(root/PREVIOUS)
 for row in previous['entries']:
  if row['sha256'] is not None:check_pin(root,row)
 freeze=load(here/'input-seal.json');tooling=load(here/'recorder-tooling-seal.json')
 need(timestamp(freeze['sealed_utc'])<timestamp(tooling['sealed_utc'])<timestamp(seal['sealed_utc']),'Broken input/tooling/output seal order')
 results={}
 for config in ['Debug','Release']:
  source=here/'original-results'/config
  original=source/'plain-v2/results.tsv';current=here/'current-results'/config/'results.tsv'
  need(original.read_bytes()==current.read_bytes(),'Full original/current bytes differ: '+config)
  for mode in ['repeat','trace']:need(original.read_bytes()==(source/(mode+'-v2')/'results.tsv').read_bytes(),'Repeat/instrumentation perturbed outputs')
  original_trace=source/'trace-v2/results.tsv.branches.json';current_trace=here/'current-results'/config/'results.tsv.branches.json'
  need(original_trace.read_bytes()==current_trace.read_bytes(),'Actual branch trace differs/omitted')
  lines=(here/('inputs-'+config+'.jsonl')).read_bytes().splitlines(keepends=True)
  inputs=[json.loads(line) for line in lines]
  with original.open(encoding='utf-8',newline='') as stream:rows=list(csv.DictReader(stream,delimiter='\t'))
  traces=load(original_trace)
  need([r['case_id'] for r in inputs]==[r['case_id'] for r in rows]==[r['case_id'] for r in traces],'Missing/reordered cases')
  current_receipt=load(here/('current-execution-'+config+'.json'))
  need(current_receipt['original_output_seal']['sha256']==SEAL and current_receipt['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT','Current lacks original seal')
  for row in current_receipt['files']:check_pin(root,row)
  compiled={}
  for mode in ['plain','trace']:
   info=load(here/('compile-original-'+config+'-'+mode+'.json'))
   need(info['source_commit']==BASE and info['actual_source_hash']==SOURCE and info['current_production_assemblies_consumed'] is False,'Wrong original source or new dependency')
   compiled[mode]={r['name']:r for r in info['dependencies']}
  for mode in ['plain','repeat','trace']:
   runtime=load(source/(mode+'-v2')/'execution.json')
   need(runtime['role']=='original' and runtime['source_commit']==BASE and runtime['configuration']==config and runtime['cases']==12 and runtime['native_attempts']==0,'Wrong original runtime')
   need(timestamp(tooling['sealed_utc'])<timestamp(runtime['started_utc'])<=timestamp(runtime['completed_utc'])<timestamp(seal['sealed_utc']),'Original executed outside sealed chronology')
   expected=compiled['trace' if mode=='trace' else 'plain']
   for a in runtime['assemblies']:
    need(a['name']+'.dll' in expected,'Foreign original assembly')
    target=expected[a['name']+'.dll'];need(a['bytes']==target['bytes'] and a['sha256']==target['sha256'] and re.fullmatch('[0-9a-f-]{36}',a['mvid']),'Wrong loaded original module')
  runtime=load(here/'current-results'/config/'execution.json')
  need(runtime['role']=='current' and runtime['configuration']==config and runtime['cases']==12 and runtime['native_attempts']==0,'Wrong current runtime')
  need(timestamp(runtime['started_utc'])>timestamp(seal['sealed_utc']),'Current before sealed original')
  summary=[]
  original_sessions=load(source/'plain-v2/results.tsv.sessions.json');current_sessions=load(here/'current-results'/config/'results.tsv.sessions.json')
  need(len(original_sessions)==len(current_sessions)==12,'Lost fitted model/session provenance')
  for i,(row,trace,inp,old_session,new_session) in enumerate(zip(rows,traces,inputs,original_sessions,current_sessions)):
   need(row['input_sha256']==digest(lines[i]),'Case not executed from frozen bytes')
   suffix=inp['case_id'].split('/')[-1];events=trace['operations'][1]['events']
   need(events==REQUIRED[suffix],'Early error/fabricated branch witness: '+inp['case_id'])
   need(trace['culture']==inp['culture'] and trace['operations'][0]['target']=='BESH.REGR.GEE_FIT' and trace['operations'][1]['target']=='BESH.REGR.GEE_LSMESTIMATE','Wrong branch invocation')
   operations=json.loads(row['operations']);need(len(operations)==2,'Extra operation')
   fit,query=operations
   need(fit['kind']=='HANDLE_CREATED' and fit['result']=='HANDLE:GEE:SESSION1' and fit['error']=='None','Invalid model handle')
   expected='BESH.REGR.GEE_LSMESTIMATE error: '+MESSAGES[suffix]
   need(query['kind']=='RETURN_TEXT' and query['result']=='S'+base64.b64encode(expected.encode()).decode(),'Wrong original outcome/error representation')
   for op in operations:
    need(op['phase']=='RETURNED_FROM_SOURCE' and op['error']=='None' and op['input']==op['byref_arguments'] and op['caught_errors']==[],'Changed ByRef/exception/side effects')
   for session in [old_session,new_session]:
    need(session['case_id']==inp['case_id'] and session['parameter_names']==['Intercept','X1','X2'] and session['creation_order']==1,'Lost design/session provenance')
    need(re.fullmatch('GEE:[0-9a-f]{32}',session['raw_handle']) and session['symbol']=='HANDLE:GEE:SESSION1','Fake handle normalization')
   need({k:v for k,v in old_session.items() if k!='raw_handle'}=={k:v for k,v in new_session.items() if k!='raw_handle'},'Changed fitted design/time/context/IEEE bits')
   summary.append(dict(case_id=inp['case_id'],exact=True,branch_trace_exact=True,outcome='RETURN_TEXT',
      witness='POSITIVE_PROFILE_SELECTION_THEN_LATER_STATISTICAL_ERROR' if suffix in ['wide-time','name-value-time','name-value-visit'] else 'INTENDED_GUARD',
      input_sha256=row['input_sha256'],original_result_sha256=digest(json.dumps(operations,ensure_ascii=False,separators=(',',':')).encode()),
      current_result_sha256=digest(json.dumps(operations,ensure_ascii=False,separators=(',',':')).encode())))
  results[config]=dict(unique_cases=12,positive_profile_selections=6,intended_guards=6,returned_reports=0,
       original_executions=36,current_executions=12,results_sha256=digest(original.read_bytes()),cases=summary)
 if publication:verify_publication(root)
 return dict(status='GEE_EVIDENCE_EXACT_AND_BRANCH_PASS_NOT_BATCH_READY',configurations=results,
      genuine_preedit_cases_per_configuration_separate=28966,late432_cases_per_configuration_separate=432,
      reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',scientific='NOT_GOLDEN',native='NOT_RUN')

def verify_publication(root,manifest_name=SELF,inventory_name=PREFIX+'file-inventory.json'):
 manifest=load(root/manifest_name);rows=manifest['entries'];paths=[r['path'] for r in rows]
 need(manifest['status']=='NOT_READY_04A38','This reader cannot certify whole-batch READY')
 need(paths==sorted(paths) and len({p.casefold() for p in paths})==len(paths),'Manifest duplicate/casefold/order')
 need(paths.count(manifest_name)==1,'Manifest missing/duplicate self')
 for row in rows:
  p=row['path'];pure=PurePosixPath(p)
  need(not pure.is_absolute() and pure.as_posix()==p and '\\' not in p and ':' not in p,'Unsafe publication path')
  need(all(v not in {'','.','..'} and not v.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',v) and not re.match(r'(?i)^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',v) for v in p.split('/')),'Escaping/reserved publication path')
  need((root/p).resolve().is_relative_to(root.resolve()),'Publication symlink escape')
  need(not p.startswith(('.git/','.test-rationalization-','packages/','TestResults/')),'Private publication material')
  if p==manifest_name:need(row['bytes'] is None and row['sha256'] is None,'Recursive manifest self entry')
  else:check_pin(root,row)
 inventory=load(root/inventory_name);need(inventory['paths']==paths,'Inventory omitted payload')
 previous=load(root/PREVIOUS)
 for row in previous['entries']:
  need(row['path'] in paths,'Lost older payload')
  if row['sha256'] is not None:need(next(r for r in rows if r['path']==row['path'])==row,'Altered old manifest assertion')
 ready=load(root/(PREFIX+'readiness.json'))
 need(ready['status']=='NOT_READY_04A38' and ready['gates']=={'P':'NO','N1':'NO GO','N2':'NO','C2':'UNMET','C3':'UNAPPROVED'},'Unapproved readiness/gate promotion')
 git=load(root/(PREFIX+'git-final.json'))
 need(git['index']=='EMPTY' and git['head']==git['main']==git['merge_base']==BASE and git['branch']=='codex/04A-38' and git['diff_check_exit']==0,'Git staging/commit/branch/whitespace')
 return dict(external_files=len(rows)-1,status='VALID_NOT_READY_GEE_PUBLICATION')

if __name__=='__main__':
 import sys
 try:print(json.dumps(inspect(publication='--publication' in sys.argv)))
 except (InvalidEvidence,OSError,KeyError,TypeError,ValueError) as e:
  print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
