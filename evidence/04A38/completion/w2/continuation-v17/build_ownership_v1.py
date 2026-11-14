"""Canonical source-bound ownership proposal; strict reproducible read-back."""
from proof_support_v17_v3 import *
from collections import Counter
verify='--verify' in sys.argv
def emit(name,value):
 if verify:assert load(HERE/name)==value,'Final ledger read-back drift '+name
 else:save(name,value)
prior_h=load(PREVIOUS/'historical-ownership-v2.json');prior_c=load(PREVIOUS/'current-ownership-v1.json')
entry=load(HERE/'entry-queue-receipt-v1.json');source=load(HERE/'source-composition-proof-v2.json');assert not source['unresolved']
regular=load(HERE/'whole-body-contracts-v1.json')['contracts'];callbacks=load(HERE/'callback-proofs-v1.json');properties=load(HERE/'original-only-property-proofs-v1.json')['rows']
proofs={r['original_id']:dict(kind='COMPLETE_ORIGINAL_CURRENT_NATIVE_PORTABLE_SOURCE_AND_DEBUG_CIL',evidence='whole-body-contracts-v1.json',original_id=r['original_id']) for r in regular}
for name,rows in [('callback-proofs-v1.json',callbacks['rows']),('original-only-property-proofs-v1.json',properties)]:
 for r in rows:assert r['original_id'] not in proofs;proofs[r['original_id']]=dict(kind='EXACT_DISTINCT_SOURCE_AND_ACTUAL_CONSTRUCTED_DEBUG_BINDING',evidence=name,original_id=r['original_id'])
assert len(proofs)==97 and set(proofs)=={r['original_id'] for r in entry['rows']}
h=copy.deepcopy(prior_h);current_map=load(HERE/'current-source-position-crosswalk-v1.json')['rows'];mp={r['canonical_current_id']:r['current_record'] for r in current_map}
# The three ambiguous v16 lambda candidate lists are resolved by exact complete
# source and parent/delegate ordinal, without changing their source identities.
cb={r['original_id']:r for r in callbacks['rows']}
for r in h['members']:
 if r['proof']:continue
 assert r['canonical_original_id'] in proofs
 r['prior_current_candidates']=r['current_ids']
 if r['canonical_original_id'] in cb:r['current_ids']=cb[r['canonical_original_id']]['current_ids']
 r.update(proof=proofs[r['canonical_original_id']],disposition='FULLY_SUPPORTED_C17_ENTIRE_SOURCE_CONTRACT_AND_NATIVE_BOUNDARY')
assert len(h['members'])==704 and sum(bool(r['proof']) for r in h['members'])==704
logical=copy.deepcopy(prior_c['members']);new_current={k for r in regular+callbacks['rows'] for k in r['current_ids']}
new_current.update(r['canonical_current_id'] for r in callbacks['current_only']);assert len(new_current)==91
for r in logical:
 oldid=r['symbol_id'];target=mp[oldid];r['actual_source_record']=target;r['canonical_current_id']=oldid
 if not r['review_status'].startswith('OPEN'):
  assert r['body_sha256']==target['body_sha256'],'Retained supported complete source body changed '+oldid
 else:
  assert oldid in new_current
  r['review_status']='SUPPORTED_C17_COMPLETE_SOURCE_CONTRACT_AND_DEBUG_BINDING';r['proof']=dict(kind='WHOLE_CURRENT_BODY_AND_ALL_NESTED_FRAGMENTS',source_record=target,evidence='whole-body-contracts-v1.json' if oldid not in {q['canonical_current_id'] for q in callbacks['current_only']} else 'callback-proofs-v1.json')
assert len(logical)==726 and all(not r['review_status'].startswith('OPEN') for r in logical)
supports=copy.deepcopy(prior_c['support']);support_proofs={r['canonical_support_id']:r for r in load(HERE/'support-fragment-crosswalk-v1.json')['new_support']};assert len(support_proofs)==32
for r in supports:
 if r['fully_reconciled']:continue
 if not r['distinct']:
  assert r['already_in_primary_population'];r['fully_reconciled']=True;r['proof']='EXACT_PRIMARY_POPULATION_BODY_PROOF';continue
 assert r['support_id'] in support_proofs and all(k in mp for k in r['children'])
 r['fully_reconciled']=True;r['proof']=dict(kind='INDIVIDUAL_ENTIRE_CHILDREN_PLUS_EXACT_FIELDS_INITIALIZERS_AND_HOST_CONTEXT',evidence='support-fragment-crosswalk-v1.json',support_id=r['support_id'])
distinct=[r for r in supports if r['distinct']];assert len(supports)==325 and len(distinct)==303 and all(r['fully_reconciled'] for r in distinct)
o=load(W2.parent/'w1/original-universe.json');u=load(FINAL/'current-universe.json');om={r['git_original_id']:r for r in h['members']}
original_fragments=copy.deepcopy(load(PREVIOUS/'original-fragment-parent-crosswalk-v2.json')['rows'])
structural={r['fragment']['fragment_id'] for r in load(HERE/'support-fragment-crosswalk-v1.json')['remaining_original_structural_fragments']}
for f in original_fragments:
 if f['covered']:continue
 if f['member_id'] is not None:
  assert om[f['member_id']]['proof'];certificate=om[f['member_id']]['proof']
 else:assert f['fragment_id'] in structural;certificate='STRUCTURAL_DECLARATION_INDIVIDUAL_CHILD_AND_FIELD_INITIALIZER_COMPOSITE'
 f.update(covered=True,proof_member=f['member_id'],completion_certificate=certificate)
assert len(original_fragments)==20683 and all(f['covered'] for f in original_fragments)
actual_members={r['symbol_id'] for r in u['members']};current_fragments=[]
for f in u['fragments']:
 assert f['member_id'] is None or f['member_id'] in actual_members
 current_fragments.append(dict(f,review_status='SUPPORTED_C17_COMPLETE_MEMBER_OR_STRUCTURAL_CONTEXT',proof_member=f['member_id']))
assert len(current_fragments)==12659
canonical=[]
for before,after,keys in [(prior_h['members'],h['members'],('historical','canonical_original_id')),(prior_c['members'],logical,('current','symbol_id')),( [r for r in prior_c['support'] if r['distinct']],distinct,('support','support_id'))]:
 population,key=keys
 for a,b in zip(before,after):
  assert a[key]==b[key]
  retained=bool(a['proof']) if population=='historical' else not a['review_status'].startswith('OPEN') if population=='current' else a['fully_reconciled']
  canonical.append(dict(population=population,identity=b[key],prior_supported=retained,supported=True,transition='RETAINED' if retained else 'NEW',proof=b.get('proof')))
assert len(canonical)==1733 and len({(r['population'],r['identity']) for r in canonical})==1733
counts={p:dict(Counter(r['transition'] for r in canonical if r['population']==p)) for p in ['historical','current','support']}
assert counts==dict(historical=dict(RETAINED=607,NEW=97),current=dict(RETAINED=635,NEW=91),support=dict(RETAINED=271,NEW=32))
queue=dict(authority='FINAL_C17_SOURCE_BOUND_PROPOSAL_SUBJECT_TO_INDEPENDENT_OWNER_REVIEW',members=[],current_OPEN=[],support_OPEN=[],first_members=[],historical_total=704,current_logical_total=726,current_physical_native=723,current_moved_to_Common=3,distinct_support_total=303,original_queries=0)
summary=dict(historical=dict(total=704,supported=704,OPEN=0,NEW=97,RETAINED=607),current=dict(total=726,supported=726,OPEN=0,NEW=91,RETAINED=635,physical_native=723,moved_to_Common=3),support=dict(distinct_total=303,supported=303,OPEN=0,NEW=32,RETAINED=271,raw_declarations=325),fragments=dict(original=dict(total=20683,covered=20683,OPEN=0),current_physical_native=dict(total=12659,covered=12659,OPEN=0),prior_current_physical_native=13695),W2='OWNERSHIP_UNIVERSE_SOURCE_PROOF_PROPOSED_COMPLETE;INDEPENDENT_REVIEW_PENDING',phase='NOT_READY_04A38',W3='NOT_STARTED',Release=DEFERRED,original_queries=0)
emit('historical-ownership-v1.json',h)
emit('current-ownership-v1.json',dict(members=logical,support=supports,fragments=current_fragments,source_identity_policy='Canonical726 from authenticated v16; actual723 Native plus three exact Common lambda counterparts. All stable old IDs retained with precise final source records.'))
emit('original-fragment-parent-crosswalk-v1.json',dict(rows=original_fragments,original_queries=0))
emit('canonical-identity-status-v1.json',dict(entries=canonical,counts=counts,original_queries=0))
emit('remaining-work-queue-v1.json',queue)
emit('ownership-summary-v1.json',summary)
emit('new-closures-final-v1.json',dict(historical_new=97,current_new=91,distinct_support_new=32,rows=[r for r in canonical if r['transition']=='NEW'],original_queries=0))
emit('next-family-handover-v1.json',dict(next_historical_OPEN=[],next_current_OPEN=[],next_support_OPEN=[],next='INDEPENDENT_W2_SOURCE_BOUND_REVIEW;NO_AUTOMATIC_W3_OR_RELEASE_FINALIZATION',terminal='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED',Release=DEFERRED))
print('Source-bound proposal/read-back:704/704;726/726 logical(723native+3Common);303/303 support; no remaining OPEN; owner review still required')
