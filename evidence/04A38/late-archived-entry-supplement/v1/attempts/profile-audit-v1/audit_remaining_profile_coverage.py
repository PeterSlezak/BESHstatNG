"""Read-only original-corpus audit of the next mandatory profile seam.

No original/current model is executed and no reference is added outside432.
"""
import base64,csv,json,sys
from collections import Counter
from pathlib import Path
from prepare_original import ROOT,HERE,pin,save
sys.path.insert(0,str(ROOT/'evidence/04A38/continuation/v1'))
from audit_frozen_branches import children

def main():
 h=ROOT/'evidence/04A38';entry=json.loads((h/'entry-members.json').read_bytes())
 member=next(row for row in entry['members'] if row['name']=='AddGeeLsmEstimateAtValue');by_config={};witnesses=[];inputs=[]
 corpora={'original-v1':'frozen-v2','public-options-v1':'attempts/public-options-v1','public-primitives-v1':'attempts/public-primitives-v1','model-inputs-v2':'attempts/model-inputs-v2','math-edges-v1':'attempts/math-edges-v1'}
 for config in ['Debug','Release']:
  counts=Counter()
  for corpus,relative in corpora.items():
   base=h/'preedit'/relative;cases={row['case_id']:row for row in map(json.loads,(base/'cases.jsonl').read_text(encoding='utf-8').splitlines())}
   trace=h/'preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl') if corpus=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   reached={row['case_id']:row['reached'] for row in map(json.loads,trace.read_text(encoding='utf-8').splitlines())}
   output=base/('original-'+config+'.tsv' if corpus=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as f:results={row['case_id']:row for row in csv.DictReader(f,delimiter='\t')}
   inputs.extend([pin(base/'cases.jsonl'),pin(trace),pin(output)])
   for key,case in cases.items():
    direct=case.get('target')=='AddGeeLsmEstimateAtValue'
    require_target=case.get('target')=='BESH.REGR.GEE_LSMESTIMATE'
    hit=member['symbol_id'] in reached[key]
    if direct:counts['direct_helper_cases']+=1
    if not require_target and not hit:continue
    assert require_target and not direct,'New indirect witness needs explicit analysis'
    row=results[key];arguments=children(row['byref_arguments']);assert arguments and len(arguments['values'])==6
    at=arguments['values'][4];prepared=children(at);counts['public_cases']+=1
    if hit:counts['helper_entry_cases']+=1
    if at=='N':counts['at_default_nothing']+=1
    else:
     assert prepared and prepared['element_type']=='System.Object'
     if prepared['bounds']==[(0,1),(0,2)]:
      counts['one_row_at_rejected_before_header_parser']+=1
      assert not hit and row['outcome']=='RETURN_TEXT'
     elif prepared['bounds']==[(0,2),(0,1)]:
      counts['wide_X1_at']+=1;assert hit
      assert prepared['values'][0]=='SWDE=' # UTF8/base64 X1
     else:raise AssertionError('Unexpected AT shape requiring independent review')
     witnesses.append(dict(configuration=config,culture=case['culture'],case_id=key,cohort=case['cohort'],at_descriptor=case['arguments'].get('at'),
      recorded_at=at,outcome=row['outcome'],source_member_entered=hit,recorded_result=row['result'],
      decoded_return_text=base64.b64decode(row['result'][1:]).decode() if row['result'].startswith('S') else None))
  assert counts==Counter(public_cases=100,at_default_nothing=96,one_row_at_rejected_before_header_parser=2,wide_X1_at=2,helper_entry_cases=2)
  by_config[config]=dict(counts)
 # The late supplement is strictly calibration/grouped/nested. It cannot add a
 # GEE profile witness merely because its original dependency library has GEE.
 late=[json.loads(line) for line in (HERE/'inputs.jsonl').read_text(encoding='utf-8').splitlines()]
 assert len(late)==432 and all(row['family'] in ['calibration','grouped','nested'] for row in late)
 save('remaining-profile-coverage.json',dict(status='NOT_READY_04A38',reason='NEW_ESSENTIAL_GEE_PROFILE_GAP_OUTSIDE_APPROVED432',member_id=member['symbol_id'],source_path=member['path'],
  source_signature=member['signature'],counts=by_config,witnesses=witnesses,inputs=inputs,
  earliest_missing='BESH.REGR.GEE_LSMESTIMATE with valid fitted handle/spec and at={{"Name","Value"},{"time",1.0}} in Invariant; then sk-SK',
  absent_branches=['Valid name/value AT parser','AddGeeLsmEstimateAtValue time/visit setting','Duplicate time/visit rejection','Duplicate fitted-design-column rejection'],
  actual_frozen_name_value_attempt='at_gee_name_value is one row/two columns and returns at-header minimum-row error before AddGee; its label/intended phase is not positive coverage',
  source_reason='TryGetGeeLsmEstimateAtSpec requires at least2 rows, recognizes Name/Value headers; AddGee time/visit and duplicate branches are required remaining04A38 reusable behavior',
  late432_gap_fixed=True,late432_additional_profile_cases=0,additional_reference_execution='NOT_AUTHORIZED; NONE_PERFORMED',
  next_action='Separate owner decision for bounded GEE-profile reference coverage, or remain NOT_READY. Do not silently expand432, relabel branches HOST_ONLY/deferred, or treat new tests as genuine pre-edit reference.',
  no_production_changes=True,native='NOT_RUN',scientific='NOT_GOLDEN'))
 print('Outside432 GEE gap:100 frozen public cases/config;96 default AT,2 one-row rejected inputs,2 X1 wide inputs; zero time/name-value/duplicate coverage')

if __name__=='__main__':main()
