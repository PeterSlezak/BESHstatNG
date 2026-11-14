"""Post-gate body/ownership and original-corpus review; no new reference calls."""
import base64,csv,re
from collections import Counter
from common import *
def main():
 assert json.loads((HERE/'MMRM_DUPLICATE_GUARD_GATE_PASS.json').read_bytes())['status']=='MMRM_DUPLICATE_GUARD_GATE_PASS'
 inventory=json.loads((ROOT/'evidence/04A38/entry-members.json').read_bytes())['members']
 add=next(m for m in inventory if m['name']=='AddMmrmLsmEstimateAtValue')
 member=next(m for m in inventory if m['name']=='TryBuildMmrmAtProfileRowMask')
 with (ROOT/'evidence/04A38/ownership-progress-validation-v1-v2.csv').open(encoding='utf-8-sig',newline='') as stream:
  ledger={r['original_member_id']:r for r in csv.DictReader(stream)}
 save('post-gate-member-review.json',dict(status='SOURCE_REVIEWED_NOT_IMPLEMENTATION_CLOSED',new_production_changes=0,new_compiled_consumer_edges=[],
      add_mmrm_member_id=add['symbol_id'],inherited_row=ledger[add['symbol_id']],source_snapshot=pin(ROOT/'evidence/04A38/preedit/source/UDFdataImport/UdfDataImportMixedModel.vb.txt'),
      statement_dispositions=[
       dict(behavior='Initialized profile guard',disposition='Internal profile contract; callers construct typed state; preserve adapter exception/guard boundary'),
       dict(behavior='Visit/time and fitted-name normalization/lookup',disposition='Already consumed Common metadata helpers through compiled Windows wrappers; do not duplicate their lookup grammar'),
       dict(behavior='Visit acceptance/state and duplicate visit guard',disposition='Reusable typed selection state; positive/duplicate coverage now authenticated; Common consumer extraction still required'),
       dict(behavior='Unknown-name error and fitted column resolution',disposition='Preserve existing shared lookup and caller-facing error/source-description phase; not closed by generic helper entry'),
       dict(behavior='Design duplicate rejection/list append',disposition='Reusable typed selection state; identical-index/casefold guard proven by v4; extraction and compiled delegation remain pending'),
       dict(behavior='Finite checks, error retention, handle ownership',disposition='Caller validates finite values; do not introduce finite checks inside this member or change success error retention. Native model/cache ownership remains Windows.')],
      next_review_member=member['symbol_id'],reason_not_closed='Adjacent mandatory profile-mask seam needs independent empty-selection reference before migration can be completed; no HOST_ONLY or zero-OPEN claim'))
 corpora=[('original-v1','frozen-v2'),('public-options-v1','attempts/public-options-v1'),('public-primitives-v1','attempts/public-primitives-v1'),('model-inputs-v2','attempts/model-inputs-v2'),('math-edges-v1','attempts/math-edges-v1')]
 counts_by_config={};witnesses=[];pins=[]
 message='no observed design rows matched the supplied LS-means profile.'
 for config in ['Debug','Release']:
  counts=Counter()
  for corpus,relative in corpora:
   base=ROOT/'evidence/04A38/preedit'/relative
   cases={r['case_id']:r for r in map(json.loads,(base/'cases.jsonl').read_bytes().splitlines())}
   trace=(ROOT/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')) if corpus=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   reached={r['case_id']:r['reached'] for r in map(json.loads,trace.read_bytes().splitlines())}
   output=base/('original-'+config+'.tsv' if corpus=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as stream:rows={r['case_id']:r for r in csv.DictReader(stream,delimiter='\t')}
   pins += [pin(base/'cases.jsonl'),pin(trace),pin(output)]
   for key,case in cases.items():
    hit=member['symbol_id'] in reached[key];target=case.get('target')
    if target=='BESH.REGR.MMRM_LSMEANS':counts['original_public_lsmeans_cases']+=1
    if hit:
     assert target=='BESH.REGR.MMRM_LSMEANS','Unreviewed indirect/direct mask witness'
     row=rows[key];counts['mask_entry_cases']+=1
     if row['outcome']=='RETURN_ARRAY':counts['returned_report_after_mask']+=1
     decoded=base64.b64decode(row['result'][1:]).decode() if row['outcome']=='RETURN_TEXT' else None
     if decoded and message in decoded:counts['no_matching_rows_guard_outputs']+=1
     witnesses.append(dict(configuration=config,case_id=key,culture=case['culture'],target=target,original_input=row['byref_arguments'],outcome=row['outcome'],decoded_return_text=decoded))
  counts['no_matching_rows_guard_outputs']+=0
  # All approved later MMRM queries are LSMESTIMATE; that public path does not
  # call this separate LSMEANS row-mask member. Do not borrow its report/guard.
  for packet in [V3,HERE]:
   cases=[json.loads(line) for line in (packet/('inputs-'+config+'.jsonl')).read_bytes().splitlines()]
   assert all(r['operations'][1]['target']=='BESH.REGR.MMRM_LSMESTIMATE' for r in cases)
  counts.update(late_headered_lsmeans_mask_cases=0,late_duplicate_lsmeans_mask_cases=0)
  assert counts['mask_entry_cases']==counts['returned_report_after_mask']==4 and counts['no_matching_rows_guard_outputs']==0
  counts_by_config[config]=dict(counts)
 source=ROOT/'evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt'
 lines=source.read_text(encoding='utf-8-sig').splitlines();assert lines[1425].strip()=='If matchedCount <= 0 Then' and message in lines[1426]
 save('remaining-profile-mask-coverage.json',dict(status='NOT_READY_04A38',earliest_failed_phase='POST_DUPLICATE_GATE_REMAINING_PROFILE_MASK_REFERENCE_COVERAGE',
      member_id=member['symbol_id'],source_path=member['path'],signature=member['signature'],inherited_ownership_row=ledger[member['symbol_id']],
      source_snapshot=pin(source),missing_source_branch=dict(line=1426,condition='If matchedCount <= 0 Then',source_error_literal=message),
      configurations=counts_by_config,witnesses=witnesses,inputs=pins,
      observed='All four original public row-mask helper entries per configuration continue to reports; no empty-selection guard witness. Later approved MMRM queries are LSMESTIMATE, not this LSMEANS mask path.',
      required='A real fitted original MMRM_LSMEANS profile with no matching saved rows, proving the line1426 true arm and real public outcome/mask side-effect semantics before corresponding current comparison',
      reason='The remaining reusable mask loop, all-false mask/source-row alignment, matched-count rejection and public error boundary cannot be declared compatibility-complete from successful masks or LSMESTIMATE duplicate failures.',
      expected_output='UNKNOWN_NOT_CAPTURED',new_reference_executions=0,new_case_packets=0,new_production_changes=0,
      next_action='STOP; separate owner decision for bounded unmatched MMRM_LSMEANS profile reference. Do not run a fifth v4 case, classify reusable mask HOST_ONLY or begin04A39.',
      qualification='First remaining independently established mask gap; not a full704/current/support/inline closure certificate',native='NOT_RUN',scientific='NOT_GOLDEN'))
 print(json.dumps(dict(status='NOT_READY_04A38',member_id=member['symbol_id'],configurations=counts_by_config,new_references=0,new_production_changes=0)))
if __name__=='__main__':main()
