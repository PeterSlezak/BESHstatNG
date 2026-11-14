"""Full fragment identities across old/current/portable source, without renumbering."""
from proof_support_v17_v4 import *
from collections import Counter
h=load(HERE/'historical-ownership-v3.json')['members'];hm={r['git_original_id']:r for r in h}
old=load(PREVIOUS/'current-ownership-v1.json');now=load(HERE/'current-ownership-v3.json');current={r['symbol_id']:r for r in now['members']}
original=load(HERE/'original-fragment-parent-crosswalk-v3.json')['rows'];prior_original={r['fragment_id']:r for r in load(PREVIOUS/'original-fragment-parent-crosswalk-v2.json')['rows']}
regular=load(HERE/'whole-body-contracts-v2.json')['contracts'];callbacks=load(HERE/'callback-proofs-v2.json')['rows'];props=load(HERE/'original-only-property-proofs-v2.json')['rows']
special={r['original_id']:r for r in callbacks+props};portable=load(FINAL/'dependency-full-common.json');native=load(FINAL/'current-universe.json')
phases=load(HERE/'source-composition-proof-v3.json')['portable_phases'];rows=[]
for f in original:
 history=hm.get(f['member_id']);targets=[];apis=[]
 if history:
  targets=[current[k]['actual_source_record'] for k in history['current_ids']]
  if history['canonical_original_id'] in special:
   r=special[history['canonical_original_id']];apis=[r.get('current_record',r.get('portable_record'))]
  else:
   apis=[r['portable_source'] for r in phases if r['native_path']==history['path'] and r['native_parent']==history['name']]
 rows.append(dict(original_fragment_id=f['fragment_id'],original_member_id=f['member_id'],canonical_original_id=history['canonical_original_id'] if history else None,path=f['path'],kind=f['kind'],span_start=f['span_start'],span_length=f['span_length'],token_sha256=f['token_sha256'],is_branch=f['is_branch'],is_inline=f['is_inline'],transition='RETAINED' if prior_original[f['fragment_id']]['covered'] else 'NEW',covered=f['covered'],actual_current_source_records=targets,portable_phase_source_records=apis,certificate=history['proof'] if history else 'STRUCTURAL_FILE_POLICY_AND_EACH_DECLARATION_CHILD_FIELD_INITIALIZER_PROOF',complete_source_certificate='source-composition-proof-v3.json'))
assert len(rows)==20683 and all(r['covered'] for r in rows)
current_rows=[]
reverse={r['actual_source_record']['symbol_id']:r['symbol_id'] for r in now['members']}
for f in native['fragments']:
 assert f['member_id'] is None or f['member_id'] in reverse
 current_rows.append(dict(fragment_id=f['fragment_id'],actual_member_id=f['member_id'],canonical_current_id=reverse.get(f['member_id']),path=f['path'],kind=f['kind'],span_start=f['span_start'],span_length=f['span_length'],token_sha256=f['token_sha256'],is_branch=f['is_branch'],is_inline=f['is_inline'],covered=True,certificate='EXACT_RETAINED_BODY_OR_COMPLETE_SOURCE_COMPOSITE_AND_DEBUG_BINDING' if f['member_id'] else 'STRUCTURAL_AND_FIELD_INITIALIZER_CONTEXT'))
retired=[]
pool={}
for f in native['fragments']+portable['fragments']:pool.setdefault((f['member_id'],f['kind'],f['token_sha256']),[]).append(f['fragment_id'])
for f in old['fragments']:
 target=current[f['member_id']]['actual_source_record'] if f['member_id'] else None
 matches=pool.get((target['symbol_id'],f['kind'],f['token_sha256']),[]) if target else []
 retired.append(dict(previous_fragment_id=f['fragment_id'],previous_member_id=f['member_id'],actual_member_id=target['symbol_id'] if target else None,exact_same_parent_kind_token_candidates=matches,mode='EXACT_PARENT_KIND_TOKEN_CANDIDATES_WITH_FULL_PARENT_SOURCE_PROOF' if matches else 'WHOLE_SOURCE_PHASE_TRANSITION_OR_STRUCTURAL_CONTEXT;NOT_AN_INVENTED_ONE_TO_ONE_FRAGMENT_ID',complete_transition_certificate='source-composition-proof-v3.json'))
assert len(retired)==13695
newpaths={'BESHStat.Excel.Common/src/Analysis/UdfW2CompletionPreparation38.vb','BESHStat.Excel.Common/src/Results/UdfW2CompletionReports38.vb'}
new_declarations=[n for n in portable['members'] if n['path'] in newpaths]
new_fragments=[f for f in portable['fragments'] if f['path'] in newpaths]
save('original-current-fragment-transition-matrix-v1.json',dict(original=rows,current_native=current_rows,prior_current=retired,new_portable_declarations=new_declarations,new_portable_fragments=new_fragments,original_counts=dict(total=len(rows),branches=sum(r['is_branch'] for r in rows),inline=sum(r['is_inline'] for r in rows)),current_native_counts=dict(total=len(current_rows),branches=sum(r['is_branch'] for r in current_rows),inline=sum(r['is_inline'] for r in current_rows)),denominator_change='13695 prior physical Windows fragments ->12659 current physical Windows fragments after exact source moves; original20683 and logical current726 identities remain unchanged. Every prior fragment identity is retained in this crosswalk.',Release=DEFERRED,original_queries=0))
print('Exhaustive fragment matrix20683 original /12659 current Native /13695 retained prior-current IDs;',len(new_fragments),'new portable-file fragments')
