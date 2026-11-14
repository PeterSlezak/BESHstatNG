"""Read-only review of a remaining profile seam AFTER the GEE gate.

No original/current MMRM invocation, new reference, or typed case materialization.
Only the original sealed corpora, runtime member-entry traces and source are read.
"""
import base64,csv,json,sys
from collections import Counter
from inspect_entry import ROOT,HERE,pin,save
sys.path.insert(0,str(ROOT/'evidence/04A38/continuation/v1'))
from audit_frozen_branches import children

def text(token):return base64.b64decode(token[1:]).decode() if token.startswith('S') else None

def main():
 gate=json.loads((HERE/'GEE_SUPPLEMENT_GATE_PASS.json').read_bytes());assert gate['status']=='GEE_SUPPLEMENT_GATE_PASS'
 inventory=json.loads((ROOT/'evidence/04A38/entry-members.json').read_bytes())['members']
 names=['TryGetMmrmLsmEstimateAtSpec','AddMmrmLsmEstimateAtValue','TryBuildMmrmAtProfileRowMask']
 members={name:next(r for r in inventory if r['name']==name and ('UdfDataImportMixedModel' in r['path'] or name=='TryBuildMmrmAtProfileRowMask')) for name in names}
 corpora={'original-v1':'frozen-v2','public-options-v1':'attempts/public-options-v1','public-primitives-v1':'attempts/public-primitives-v1','model-inputs-v2':'attempts/model-inputs-v2','math-edges-v1':'attempts/math-edges-v1'}
 result={};witnesses=[];dependencies=[];other_entries=[]
 for config in ['Debug','Release']:
  counts=Counter()
  for corpus,relative in corpora.items():
   base=ROOT/'evidence/04A38/preedit'/relative
   cases={c['case_id']:c for c in map(json.loads,(base/'cases.jsonl').read_text(encoding='utf-8').splitlines())}
   trace=(ROOT/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')) if corpus=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   reached={r['case_id']:r['reached'] for r in map(json.loads,trace.read_text(encoding='utf-8').splitlines())}
   output=base/('original-'+config+'.tsv' if corpus=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as stream:outputs={r['case_id']:r for r in csv.DictReader(stream,delimiter='\t')}
   dependencies += [pin(base/'cases.jsonl'),pin(trace),pin(output)]
   for key,case in cases.items():
    hits=[name for name,member in members.items() if member['symbol_id'] in reached[key]]
    target=case.get('target');public=target in ['BESH.REGR.MMRM_LSMEANS','BESH.REGR.MMRM_LSMESTIMATE']
    if not public:
     if hits:other_entries.append(dict(configuration=config,case_id=key,target=target,hits=hits,input=outputs[key]['byref_arguments'],outcome=outputs[key]['outcome']))
     continue
    counts['public_cases']+=1
    args=children(outputs[key]['byref_arguments']);assert args
    assert len(args['values'])==(3 if target.endswith('_LSMEANS') else 4)
    at=args['values'][1 if target.endswith('_LSMEANS') else 3]
    if at=='N':counts['default_profile']+=1;continue
    arr=children(at)
    if arr is None:counts['scalar_profile']+=1;continue
    counts['array_profile']+=1
    for name in hits:counts['member_entry/'+name]+=1
    headered=False
    if len(arr['bounds'])==2:
     rows,columns=[n for _,n in arr['bounds']]
     if rows and columns:
      header=[(text(v) or '').strip().lower() for v in arr['values'][:columns]]
      namecols=[i for i,h in enumerate(header) if h in ['name','variable','effect','column','profile','at']]
      valuecols=[i for i,h in enumerate(header) if h in ['value','val','setting','atvalue']]
      headered=any(i!=j for i in namecols for j in valuecols)
    if headered:counts['headered_name_value_candidate']+=1
    decoded=base64.b64decode(outputs[key]['result'][1:]).decode() if outputs[key]['outcome']=='RETURN_TEXT' else None
    witnesses.append(dict(configuration=config,corpus=corpus,case_id=key,culture=case.get('culture'),target=target,
       input_descriptor=case.get('arguments'),recorded_profile=at,headered_name_value_candidate=headered,
       source_members_entered=hits,outcome=outputs[key]['outcome'],decoded_return_text=decoded))
  counts['headered_name_value_candidate']+=0
  result[config]=dict(counts)
 # Any indirect/headered candidate must be reviewed, never silently omitted.
 assert not other_entries,other_entries
 assert all(counts['headered_name_value_candidate']==0 for counts in result.values())
 parser=members['TryGetMmrmLsmEstimateAtSpec']
 save('remaining-mmrm-profile-coverage.json',dict(status='NOT_READY_04A38',earliest_failed_phase='POST_GEE_GATE_REMAINING_PROFILE_REFERENCE_COVERAGE',
      reason='ESSENTIAL_HEADERED_MMRM_NAME_VALUE_PARSER_HAS_NO_ORIGINAL_FROZEN_WITNESS',
      member_id=parser['symbol_id'],source_path=parser['path'],source_signature=parser['signature'],
      source_members={k:dict(member_id=v['symbol_id'],path=v['path'],signature=v['signature']) for k,v in members.items()},
      counts=result,witnesses=witnesses,indirect_cases=other_entries,inputs=dependencies,
      original_source=pin(ROOT/'evidence/04A38/preedit/source/UDFdataImport/UdfDataImportMixedModel.vb.txt'),
      source_branch='TryGetMmrmLsmEstimateAtSpec: If nameCol >= 0 AndAlso valueCol >= 0 AndAlso nameCol <> valueCol Then; headered Name/Value loop calling AddMmrmLsmEstimateAtValue',
      observed='Existing at_mmrm_name_value is HEADERLESS treatment_active/value and visit/value; original MMRM explicitly supports that separate branch. Wide treatment_active/visit is also separate. Neither witnesses headered Name/Value.',
      required='A genuinely independent original-source execution reaching the distinct valid headered Name/Value parser on a real fitted MMRM public path before corresponding current comparison',
      illustrative_unmaterialized_input=[['Name','Value'],['visit',1.0]],expected_output='UNKNOWN_NOT_CAPTURED',
      late432_scope='Calibration/grouped/nested only; no MMRM cases',late12_scope='GEE_LSMESTIMATE only; no MMRM cases',
      new_reference_execution='NOT_AUTHORIZED_NOT_EXECUTED',production_changes_after_gee_gate=0,
      next_action='STOP. Separate owner decision for any bounded MMRM original-source supplement; do not capture, classify HOST_ONLY or defer reusable parser logic.',
      qualification='First remaining proved gap; not a certification that all other704/current/support/inline behaviors have coverage',
      native='NOT_RUN',scientific='NOT_GOLDEN'))
 print(json.dumps(dict(status='NOT_READY_04A38',counts=result,member_id=parser['symbol_id'],new_reference_executions=0)))
if __name__=='__main__':main()
