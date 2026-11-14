"""After the gate: inspect existing references for the remaining MMRM guards.

Read-only corpus/source review. No proposed invalid input is materialized or run.
"""
import base64,csv,re
from collections import Counter
from common import *
sys.path.insert(0,str(ROOT/'evidence/04A38/continuation/v1'))
from audit_frozen_branches import children

def main():
 assert json.loads((HERE/'MMRM_HEADERED_PROFILE_GATE_PASS.json').read_bytes())['status']=='MMRM_HEADERED_PROFILE_GATE_PASS'
 member=next(m for m in json.loads((ROOT/'evidence/04A38/entry-members.json').read_bytes())['members'] if m['name']=='AddMmrmLsmEstimateAtValue')
 ledger_path=ROOT/'evidence/04A38/ownership-progress-validation-v1-v2.csv'
 with ledger_path.open(encoding='utf-8-sig',newline='') as stream:ledger=next(r for r in csv.DictReader(stream) if r['original_member_id']==member['symbol_id'])
 corpora=[('original-v1','frozen-v2'),('public-options-v1','attempts/public-options-v1'),('public-primitives-v1','attempts/public-primitives-v1'),('model-inputs-v2','attempts/model-inputs-v2'),('math-edges-v1','attempts/math-edges-v1')]
 configurations={};witnesses=[];pins=[]
 for config in ['Debug','Release']:
  counts=Counter()
  for corpus,relative in corpora:
   base=ROOT/'evidence/04A38/preedit'/relative
   cases={r['case_id']:r for r in map(json.loads,(base/'cases.jsonl').read_bytes().splitlines())}
   trace=(ROOT/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')) if corpus=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   reached={r['case_id']:r['reached'] for r in map(json.loads,trace.read_bytes().splitlines())}
   output=base/('original-'+config+'.tsv' if corpus=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as stream:outputs={r['case_id']:r for r in csv.DictReader(stream,delimiter='\t')}
   pins += [pin(base/'cases.jsonl'),pin(trace),pin(output)]
   for key,case in cases.items():
    target=case.get('target');hit=member['symbol_id'] in reached[key]
    if target not in ['BESH.REGR.MMRM_LSMEANS','BESH.REGR.MMRM_LSMESTIMATE']:
     assert not hit,('Unreviewed indirect helper witness',key);continue
    counts['original_public_cases']+=1
    args=children(outputs[key]['byref_arguments']);at=args['values'][1 if target.endswith('_LSMEANS') else 3]
    arr=children(at)
    if arr is None:continue
    counts['original_array_profiles']+=1
    if hit:counts['original_helper_entry_cases']+=1
    cols=arr['bounds'][1][1];values=arr['values'];headers=[base64.b64decode(v[1:]).decode() if v.startswith('S') else None for v in values[:cols]]
    # The twelve actual array profiles are exactly these headerless/wide/early
    # numeric forms. No other shape/name is silently classified as covered.
    if arr['bounds']==[(0,2),(0,2)] and headers==['treatment_active',None]:
     names=[base64.b64decode(values[r*cols][1:]).decode() for r in range(arr['bounds'][0][1])];kind='HEADERLESS_VALID'
    elif arr['bounds']==[(0,2),(0,2)] and headers==['treatment_active','visit']:
     names=headers;kind='WIDE_VALID'
    else:
     assert all(v.startswith('D') for v in values),'Unreviewed profile name/layout';names=[];kind='EARLY_NUMERIC_HEADER_ERROR'
    normalized=['visit' if re.sub(r'[^a-z0-9]','',n.lower()) in ['time','visit'] else re.sub(r'[^a-z0-9]','',n.lower()) for n in names]
    duplicate=len(normalized)!=len(set(normalized))
    if duplicate:counts['original_duplicate_selection_candidates']+=1
    result=outputs[key];decoded=base64.b64decode(result['result'][1:]).decode() if result['outcome']=='RETURN_TEXT' else None
    if decoded and ('at specifies visit/time more than once.' in decoded or ('at specifies design column' in decoded and 'more than once.' in decoded)):counts['original_duplicate_guard_outputs']+=1
    witnesses.append(dict(configuration=config,case_id=key,target=target,recorded_profile=at,kind=kind,selected_names=names,
      duplicate_candidate=duplicate,helper_entered=hit,outcome=result['outcome'],decoded_return_text=decoded))
  # Approved late432/GEE have no MMRM operations. The two new headered cases
  # have one design selection and one visit selection and return reports.
  current_cases=[json.loads(line) for line in (HERE/('inputs-'+config+'.jsonl')).read_bytes().splitlines()]
  traces=json.loads((HERE/'original-results'/config/'trace/results.tsv.branches.json').read_bytes())
  assert len(current_cases)==len(traces)==2
  for case,trace in zip(current_cases,traces):
   events=trace['operations'][1]['events'];assert [e for e in events if e.startswith('helper.entry=')]==['helper.entry=treatment_active','helper.entry=visit']
   assert 'report.returned=True' in events
  counts.update(approved_headered_cases=2,approved_headered_helper_entry_cases=2,approved_headered_reports=2)
  for key in ['original_duplicate_selection_candidates','original_duplicate_guard_outputs','approved_headered_duplicate_guard_witnesses']:counts[key]+=0
  configurations[config]=dict(counts)
 # Strictly scope-check prior approvals; no combined total may hide MMRM cases.
 late432=[json.loads(line) for line in (ROOT/'evidence/04A38/late-archived-entry-supplement/v1/inputs.jsonl').read_bytes().splitlines()]
 assert len(late432)==432 and all(r['family'] in ['calibration','grouped','nested'] for r in late432)
 for config in ['Debug','Release']:
  gee=[json.loads(line) for line in (V2/('inputs-'+config+'.jsonl')).read_bytes().splitlines()]
  assert len(gee)==12 and all(r['public_entry']=='BESH.REGR.GEE_LSMESTIMATE' for r in gee)
  assert configurations[config]['original_duplicate_selection_candidates']==configurations[config]['original_duplicate_guard_outputs']==0
 save('remaining-duplicate-guard-coverage-v2.json',dict(status='NOT_READY_04A38',earliest_failed_phase='POST_MMRM_GATE_REMAINING_PROFILE_GUARD_REFERENCE_COVERAGE',
      member_id=member['symbol_id'],source_path=member['path'],signature=member['signature'],source_snapshot=pin(ROOT/'evidence/04A38/preedit/source/UDFdataImport/UdfDataImportMixedModel.vb.txt'),
      inherited_ownership_row=ledger,ownership_ledger=pin(ledger_path),configurations=configurations,witnesses=witnesses,inputs=pins,
      missing_source_branches=[dict(guard='Duplicate normalized visit/time selection',line=726,condition='If atProfile.VisitSpecified Then',message='at specifies visit/time more than once.'),
      dict(guard='Duplicate resolved fitted-design ColumnIndex',line=744,condition='If existing.ColumnIndex = idx Then',message='at specifies design column <canonical name> more than once.')],
      reason='Positive headered,headerless,wide parser/helper entry does not exercise either original duplicate rejection. These pure validation branches remain in the reusable Common migration scope.',
      required='Separately authorized original-source negative witnesses on real fitted MMRM public paths before corresponding current comparisons; current tests cannot be their own oracle',
      expected_results='UNKNOWN_NOT_CAPTURED',new_invalid_profile_materializations=0,new_invalid_profile_executions=0,new_production_changes=0,
      next_action='STOP. Separate owner decision for bounded duplicate visit/time and fitted-design reference coverage; do not capture extra cases, weaken guard coverage, mark HOST_ONLY or begin04A39.',
      qualification='First remaining essential gap, not a claim that all other historical/current/support/inline behavior is reviewed',native='NOT_RUN',scientific='NOT_GOLDEN'))
 print(json.dumps(dict(status='NOT_READY_04A38',member_id=member['symbol_id'],configurations=configurations,new_references=0)))
if __name__=='__main__':main()
