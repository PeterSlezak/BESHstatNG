"""Exact inherited/new/open ownership, including fail-closed dependency propagation."""
from proof_support_v16 import *
from collections import Counter
_save_new=save
def save(name,value):
 path=HERE/name
 if path.exists():assert load(path)==value, 'Existing append-only ledger differs '+name
 else:_save_new(name,value)

import csv
old=load(PREVIOUS/'current-ownership-v3.json');oh=load(PREVIOUS/'historical-ownership-v2.json')['members']
u=load(HERE/'final-v4/current-universe.json');o=load(W2.parent/'w1/original-universe.json')
cs=load(HERE/'whole-body-contracts-v1.json')['contracts'];cb=load(HERE/'callback-proofs-v1.json')['rows']
special=load(HERE/'historical-only-callback-proof-v1.json')['rows']
source=load(HERE/'source-composition-proof-v1.json')
assert {r['name'] for r in source['unresolved']}=={'NormalizeMixedModelResidualCovarianceName'}
assert load(HERE/'managed-validation-v1.json')['Debug']['outcomes']==dict(Passed=21039,Failed=14)
mapping=map_current(old['members'],u['members']);reverse={n['symbol_id']:k for k,n in mapping.items()}
oldm={n['symbol_id']:n for n in old['members']};members_byid={n['symbol_id']:n for n in u['members']}
inherited={mapping[n['symbol_id']]['symbol_id'] for n in old['members'] if not n['review_status'].startswith('OPEN')}
assert len(inherited)==566
assert all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in old['members'] if not n['review_status'].startswith('OPEN'))
blocked={r['current_id']:dict(reason=r['reason'],direct=True,unresolved_current_id=r['current_id']) for r in source['unresolved']}
bad_methods={r['compiled_Debug']['complete_method']['method']:r['current_id'] for r in cs if r['current_id'] in blocked}
for r in cs:
 reached=set(r['compiled_Debug']['transitive']['methods'])
 failures=[(m,n) for m,n in bad_methods.items() if m in reached]
 if failures:blocked.setdefault(r['current_id'],dict(reason='Unresolved genuine outbound dependency; parent remains OPEN',direct=False,unresolved_current_id=failures[0][1],compiled_method=failures[0][0]))
assert not (set(blocked)&inherited),'New defect invalidates inherited closure: stop for owner decision'
eligible={r['current_id'] for r in cs+cb if r['current_id'] not in blocked}
closed=inherited|eligible
for n in u['members']:
 if n['symbol_id'] in closed-inherited:
  assert all(c['symbol_id'] in closed for c in u['members'] if c['parent_member']==n['symbol_id']),'Hidden OPEN nested member '+n['symbol_id']
new={r['original_id']:r for r in cs+cb if r['new_support'] and r['current_id'] not in blocked and r['original_id']}
for r in special:
 assert r['original_id'] not in new;new[r['original_id']]=r
assert None not in new
history=[];newrows=[]
for h in oh:
 row=dict(h,current_ids=[mapping[k]['symbol_id'] for k in h['current_ids']])
 if h['canonical_original_id'] in new:
  r=new[h['canonical_original_id']];assert not h['proof']
  expected=[] if r in special else [r['current_id']]
  assert row['current_ids']==expected
  evidence='historical-only-callback-proof-v1.json' if r in special else 'callback-proofs-v1.json' if r in cb else 'whole-body-contracts-v1.json'
  row.update(proof=dict(kind='C16_ENTIRE_ORIGINAL_CURRENT_DEBUG_SOURCE_CIL_TRANSITIVE_HOST_PORTABLE_OWNERSHIP',evidence=evidence,original_id=h['canonical_original_id'],current_ids=expected,behavior='source-composition-proof-v1.json',compiled_context='native-state-contexts-v1.json',complete_dependencies=['complete-compiled-dependency-catalog-v1.json','complete-original-compiled-dependency-catalog-v1.json'],Release=DEFERRED),disposition='FULLY_SUPPORTED_C16_COMPLETE_DEBUG_RECONCILIATION')
  newrows.append(dict(original_id=h['canonical_original_id'],current_ids=expected,name=h['name'],proof=row['proof']))
 history.append(row)
members=[dict(n,review_status='FULLY_SUPPORTED_C16_ENTIRE_BODY_DEBUG_OWNERSHIP' if n['symbol_id'] in closed else oldm[reverse[n['symbol_id']]]['review_status'],open_obligation=blocked.get(n['symbol_id'])) for n in u['members']]
key=lambda s:(s['path'],s['kind'],s['owner'],s['declaration'])
oldss={key(s):s for s in old['support']}
contexts=load(HERE/'native-state-contexts-v1.json')['classes'];context_ids={r['source']['support_id'] for r in contexts}
module_names={'Public Module GEEUDFs','Public Module GLMNbUDFs','Public Module GLMUDFs','Public Module LinearModelUDFs','Public Module LmmUDFs'}
def children(s):
 owner=s['owner'].rstrip('.')+'.'+s['declaration'].split()[-1]
 return [n for n in u['members'] if n['path']==s['path'] and (n['owner']==owner or n['owner'].startswith(owner+'.'))]
def complete(s):
 was=oldss[key(s)]
 if was['fully_reconciled']:return True
 if s['support_id'] in context_ids:return all(n['symbol_id'] in closed for n in children(s))
 if s['kind']=='ModuleBlock' and s['declaration'] in module_names:
  nested=[v for v in u['supports'] if v['path']==s['path'] and v['support_id']!=s['support_id'] and s['span_start']<=v['span_start'] and v['span_start']+v['span_length']<=s['span_start']+s['span_length']]
  return all(n['symbol_id'] in closed for n in children(s)) and all(complete(v) for v in nested)
 return False
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
supports=[];support_proofs=[]
for s in u['supports']:
 was=oldss[key(s)];done=complete(s);p=was.get('proof')
 if done and not was['fully_reconciled']:
  p=dict(kind='C16_COMPLETE_CONTAINER_STATE_AND_ALL_ACTUAL_CHILDREN',evidence='support-container-proofs-v1.json',support_id=s['support_id'],Release=DEFERRED)
  before=next(v for v in o['supports'] if key(v)==key(s))
  now,nv=source_part(u,s,(ROOT/s['path']).read_bytes());prior,pv=source_part(o,before,(ROOT/snapshots[s['path']]['path']).read_bytes())
  support_proofs.append(dict(current_source=s,original_source=before,current_whole_declaration=now,original_whole_declaration=prior,current_view=nv,original_view=pv,original_snapshot=snapshots[s['path']],all_primary_children=children(s),blocking_primary_ids=[n['symbol_id'] for n in children(s) if n['symbol_id'] not in closed],state_and_compiled_evidence='native-state-contexts-v1.json' if s['kind']=='ClassBlock' else 'constructed-owner-proofs-v1.json'))
 supports.append(dict(s,fully_reconciled=done,proof=p,children=[n['symbol_id'] for n in children(s)],distinct=not s['already_in_primary_population']))
new_support=[s['support_id'] for s in supports if s['fully_reconciled'] and not oldss[key(s)]['fully_reconciled']]
new_keys={key(s) for s in supports if s['support_id'] in new_support}
old_headers=load(PREVIOUS/'support-fragment-crosswalk-v1.json')
old_original_f={f['fragment_id']:f for f in o['fragments']}
old_current_f={f['fragment_id']:f for f in old['fragments']}
def headers(universe,previous_fragments,previous_map):
 result={};lookup={s['support_id']:s for s in old['support']}
 for oldid,oldowner in previous_map.items():
  prior=previous_fragments[oldid]
  # Map by the exact declaration owner and occurrence, never a nearby method.
  oldsupport=lookup.get(oldowner)
  if oldsupport is None:
   oldsupport=next(s for s in o['supports'] if s['support_id']==oldowner)
  current_support=next(s for s in universe['supports'] if key(s)==key(oldsupport))
  candidates=[f for f in universe['fragments'] if f['member_id'] is None and f['path']==prior['path'] and f['kind']==prior['kind'] and f['token_sha256']==prior['token_sha256'] and current_support['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=current_support['span_start']+current_support['span_length']]
  assert len(candidates)==1,'Ambiguous retained support fragment '+oldid
  result[candidates[0]['fragment_id']]=current_support['support_id']
 for s in universe['supports']:
  if key(s) not in new_keys:continue
  for f in universe['fragments']:
   if f['member_id'] is None and f['path']==s['path'] and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']:result[f['fragment_id']]=s['support_id']
 return result
hc=headers(u,old_current_f,old_headers['current']);ho=headers(o,old_original_f,old_headers['original'])
historical_closed={h['git_original_id'] for h in history if h['proof']}
fragments=[dict(f,review_status='FULLY_SUPPORTED_COMPLETE_DEBUG_CONTEXT' if f['member_id'] in closed or f['fragment_id'] in hc else 'OPEN',proof_member=f['member_id'] if f['member_id'] in closed else hc.get(f['fragment_id'])) for f in u['fragments']]
ofragments=[dict(f,covered=f['member_id'] in historical_closed or f['fragment_id'] in ho,proof_member=f['member_id'] if f['member_id'] in historical_closed else ho.get(f['fragment_id'])) for f in o['fragments']]
count=lambda rows,p:dict(total=len(rows),covered=sum(p(r) for r in rows),branches=sum(r['is_branch'] for r in rows),branches_covered=sum(r['is_branch'] and p(r) for r in rows),inline=sum(r['is_inline'] for r in rows),inline_covered=sum(r['is_inline'] and p(r) for r in rows))
summary=dict(historical=dict(total=704,closed=sum(bool(h['proof']) for h in history),OPEN=sum(not h['proof'] for h in history)),current=dict(total=726,closed=len(closed),OPEN=726-len(closed)),support=dict(raw=len(supports),distinct=303,distinct_closed=sum(s['fully_reconciled'] for s in supports if s['distinct']),distinct_OPEN=sum(not s['fully_reconciled'] for s in supports if s['distinct'])),fragments=dict(current=count(fragments,lambda f:f['review_status']!='OPEN'),original=count(ofragments,lambda f:f['covered'])))
assert (len(history),len(members),sum(s['distinct'] for s in supports))==(704,726,303)
assert sum(bool(h['proof']) for h in oh)==537
save('historical-ownership-v1.json',dict(members=history,total=704));save('current-ownership-v1.json',dict(members=members,support=supports,fragments=fragments))
save('ownership-summary-v1.json',summary);save('support-container-proofs-v1.json',dict(rows=support_proofs,original_queries=0))
save('support-fragment-crosswalk-v1.json',dict(current=hc,original=ho,qualification='All previously covered memberless fragments retained by unique actual owner/kind/token match. Only newly fully reconciled complete containers add header/state fragments. No OPEN-parent fragment is closed by a narrow Common call.'))
save('original-fragment-parent-crosswalk-v1.json',dict(rows=ofragments,original_queries=0))
save('new-closures-final-v1.json',dict(rows=newrows,new_support=new_support,historical_new=len(new),current_new=len(closed-inherited),distinct_support_new=len(new_support),current_only=[r['current_id'] for r in cb if r['new_support'] and r['original_id'] is None],historical_only=[r['original_id'] for r in special],all_v15_supported='RETAINED',prior_four_ROC_bodies='ALL_RETAINED_NO_REOPEN',blocked=blocked,original_queries=0,Release=DEFERRED))
priorq=load(PREVIOUS/'remaining-work-queue-v2.json');remaining=[r for r in priorq['members'] if r['original_id'] not in new]
assert len(remaining)==summary['historical']['OPEN']
save('remaining-work-queue-v1.json',dict(members=remaining,first_members=remaining[:24],current_OPEN=[dict(current_id=n['symbol_id'],name=n['name'],path=n['path'],obligation=n['open_obligation']) for n in members if n['review_status'].startswith('OPEN')],support_OPEN=[s for s in supports if s['distinct'] and not s['fully_reconciled']],original_queries=0))
ledger=[]
for h,p in zip(history,oh):ledger.append(dict(population='historical',identity=h['canonical_original_id'],transition='NEW' if h['canonical_original_id'] in new else 'RETAINED' if h['proof'] else 'OPEN',prior_supported=bool(p['proof']),supported=bool(h['proof']),current_ids=h['current_ids'],proof=h['proof']))
for n in members:
 prior=oldm[reverse[n['symbol_id']]];was=not prior['review_status'].startswith('OPEN');done=n['symbol_id'] in closed
 ledger.append(dict(population='current',identity=n['symbol_id'],before_identity=prior['symbol_id'],transition='RETAINED' if was else 'NEW' if done else 'OPEN',prior_supported=was,supported=done))
for s in supports:
 if not s['distinct']:continue
 prior=oldss[key(s)];was=prior['fully_reconciled'];done=s['fully_reconciled']
 ledger.append(dict(population='support',identity=s['support_id'],before_identity=prior['support_id'],declaration=s['declaration'],owner=s['owner'],transition='RETAINED' if was else 'NEW' if done else 'OPEN',prior_supported=was,supported=done,proof=s['proof']))
assert len(ledger)==1733
save('canonical-identity-status-v1.json',dict(entries=ledger,scope='Exact704/726/303, genuine IDs only; Debug-only intermediate checkpoint; unresolved dependency and all transitive selected parents OPEN.'))
save('open-obligations-v1.json',dict(blocked=blocked,source_obligations=source['unresolved'],selected58=[dict(ordinal=r['ordinal'],original_id=r['historical_id'],supported=r['historical_id'] in new) for r in load(HERE/'entry-queue-receipt-v1.json')['rows']],source_changes_after_final_freeze=False,original_queries=0))
save('readiness-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',mandatory_blockers=summary,owner_acceptance='NOT_ACCEPTED_NOT_MERGED',Release=DEFERRED,original_queries=0))
print(json.dumps(summary),flush=True)
