"""Append the two separately proved original-only callbacks; preserve every old ledger."""
from proof_support_v16 import *
add=load(HERE/'original-only-callback-addendum-v1.json')['rows'];ids={r['original_id'] for r in add}
history=load(HERE/'historical-ownership-v1.json');new=load(HERE/'new-closures-final-v1.json');canonical=load(HERE/'canonical-identity-status-v1.json')
fragments=load(HERE/'original-fragment-parent-crosswalk-v1.json');summary=load(HERE/'ownership-summary-v1.json');cross=load(HERE/'new-parent-fragment-crosswalk-v1.json')
o=load(W2.parent/'w1/original-universe.json');common=load(HERE/'final-v4/dependency-full-common.json')
byid={r['original_id']:r for r in add}
for row in history['members']:
 if row['canonical_original_id'] not in ids:continue
 assert not row['proof'] and row['current_ids']==[]
 row.update(proof=dict(kind='C16_ADDITIVE_COMPLETE_ORIGINAL_ONLY_CALLBACK_SOURCE_TYPED_DELEGATE_AND_DEBUG_CIL',evidence='original-only-callback-addendum-v1.json',original_id=row['canonical_original_id'],current_ids=[],Release=DEFERRED),disposition='FULLY_SUPPORTED_C16_INDEPENDENT_ORIGINAL_ONLY_CALLBACK')
 new['rows'].append(dict(original_id=row['canonical_original_id'],current_ids=[],name=row['name'],proof=row['proof']))
new.update(historical_new=70,historical_only=new['historical_only']+sorted(ids))
for row in canonical['entries']:
 if row['population']=='historical' and row['identity'] in ids:
  assert row['transition']=='OPEN' and not row['prior_supported'];row.update(transition='NEW',supported=True,proof=next(r['proof'] for r in history['members'] if r['canonical_original_id']==row['identity']))
gitids={r['original_record']['symbol_id'] for r in add}
for f in fragments['rows']:
 if f['member_id'] in gitids:
  assert not f['covered'];f.update(covered=True,proof_member=f['member_id'])
for c in add:
 pool=[f for f in common['fragments'] if f['member_id']==c['current_portable_record']['symbol_id']]
 for f in o['fragments']:
  if f['member_id']!=c['original_record']['symbol_id']:continue
  matches=[v for v in pool if v['kind']==f['kind'] and tokens(v['code'])==tokens(f['code'])];assert matches
  cross['rows'].append(dict(original_fragment=f,original_parent=c['original_id'],current_parent=None,mode='EXACT_ORIGINAL_ONLY_CALLBACK_SOURCE_AND_INDEPENDENT_TYPED_DEBUG_DELEGATE',current_matches=[dict(fragment_id=v['fragment_id'],member_id=v['member_id'],path=v['path'],kind=v['kind'],token_sha256=v['token_sha256']) for v in matches],source_certificate='original-only-callback-addendum-v1.json',original_parent_record=c['original_record'],original_source=c['original_source'],current_source=c['current_portable_source'],is_branch=f['is_branch'],is_inline=f['is_inline'],Release=DEFERRED))
cross['original_fragments']=len(cross['rows']);cross['modes']['EXACT_ORIGINAL_ONLY_CALLBACK_SOURCE_AND_INDEPENDENT_TYPED_DEBUG_DELEGATE']=sum(r['mode']=='EXACT_ORIGINAL_ONLY_CALLBACK_SOURCE_AND_INDEPENDENT_TYPED_DEBUG_DELEGATE' for r in cross['rows'])
summary['historical']=dict(total=704,closed=607,OPEN=97)
of=fragments['rows'];summary['fragments']['original'].update(covered=sum(f['covered'] for f in of),branches_covered=sum(f['is_branch'] and f['covered'] for f in of),inline_covered=sum(f['is_inline'] and f['covered'] for f in of))
assert sum(bool(r['proof']) for r in history['members'])==607
bygit={r['git_original_id']:r for r in history['members']};closed_new={r['original_id'] for r in new['rows']}
for n in o['members']:
 if n['parent_member'] in bygit and bygit[n['parent_member']]['canonical_original_id'] in closed_new:assert bygit[n['symbol_id']]['proof'],'New original parent has OPEN nested identity '+n['symbol_id']
queue=load(HERE/'remaining-work-queue-v1.json');queue['members']=[r for r in queue['members'] if r['original_id'] not in ids];queue['first_members']=queue['members'][:24];assert len(queue['members'])==97
readiness=load(HERE/'readiness-v1.json');readiness['mandatory_blockers']=summary
handover=load(HERE/'next-family-handover-v1.json');handover.update(ordered_next=queue['first_members'],historical_OPEN=97)
outputs={'historical-ownership-v2.json':history,'new-closures-final-v2.json':new,'canonical-identity-status-v2.json':canonical,'original-fragment-parent-crosswalk-v2.json':fragments,'new-parent-fragment-crosswalk-v2.json':cross,'ownership-summary-v2.json':summary,'remaining-work-queue-v2.json':queue,'readiness-v2.json':readiness,'next-family-handover-v2.json':handover}
for name,doc in outputs.items():write_json(HERE,name,doc)
print('Additive exact two original-only identities; historical607/704 OPEN97; current635/726 OPEN91; support271/303 OPEN32',flush=True)
