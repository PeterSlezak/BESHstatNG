"""Recompute exact canonical/current/support/fragment populations, never READY."""
from common import *
from verify_checkpoint_template_v2 import compute
proof=compute(check_ledgers=False)
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v1.json').read_bytes())['members']
old=json.loads((PREVIOUS/'current-ownership-v1.json').read_bytes())
new=set(proof['newly_supported_historical']);closed=set(proof['closed_current']);hist=[]
for h,d in zip(history,proof['historical_dispositions']):
    authority=h['proof']
    if h['canonical_original_id'] in new:authority=dict(kind='C7_FULL_AUTHENTICATED_SOURCE_BRANCH_INLINE_AND_ACTUAL_TRANSITIVE_CIL',evidence='whole-body-contracts-v1.json',original_id=h['canonical_original_id'])
    hist.append(dict(h,current_ids=d['current_ids'],proof=authority,disposition='PROOF_SUPPORTED_REVIEWED_BODY' if authority else h['disposition'],partial_implementation=None if authority else h.get('partial_implementation')))
save('historical-ownership-v1.json',dict(status='NOT_READY_04A38',**proof['historical'],members=hist))
dispositions={r['id']:r for r in proof['support_dispositions']};support=[]
prior_support={(r['path'],r['kind'],r['owner'],r['declaration']):r for r in old['support']}
for row in u['supports']:
    before=prior_support[row['path'],row['kind'],row['owner'],row['declaration']];d=dispositions[row['support_id']]
    authority=before.get('proof')
    if d['supported'] and not before['fully_reconciled']:authority=dict(kind='C7_ALL_CONSTITUENT_BODIES_AND_EMPTY_STATIC_FIELD_CONTEXT',children=d['children'],evidence='whole-body-contracts-v1.json',compiled='metadata-all/packet-v1-context-v3/')
    support.append(dict(row,canonical_v6_support_id=before['support_id'],fully_reconciled=d['supported'],proof=authority,children=d['children'],disposition='SOURCE_AND_COMPILED_CONTEXT_SUPPORTED' if d['supported'] else 'OPEN_COMPLETE_CONTAINER_OR_BEHAVIOR_REVIEW_REQUIRED'))
fragments=[dict(f,review_status='COVERED_BY_REVIEWED_BODY_OR_DECLARATION' if f['member_id'] in closed or f['fragment_id'] in proof['header_fragments'] else 'OPEN_STATEMENT_INLINE_OR_BOUNDARY_REVIEW_REQUIRED',declaration_proof=proof['header_fragments'].get(f['fragment_id'])) for f in u['fragments']]
save('current-ownership-v1.json',dict(status='NOT_READY_04A38',members=[dict(n,review_status='REVIEWED_COMPLETE_BODY_OR_EXPLICIT_CALLBACK' if n['symbol_id'] in closed else 'OPEN_WHOLE_BODY_REVIEW_REQUIRED') for n in u['members']],support=support,fragments=fragments))
save('ownership-summary-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical=proof['historical'],current=proof['current'],support=proof['support'],fragments=proof['fragments'],new_historical_supported=len(new),new_current_callbacks=0,new_current_only_helpers=1,exports=268,original_queries=0))
save('fragment-coverage-v1.json',proof['fragments'])
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());supported={r['git_original_id'] for r in hist if r['proof']};canonical={r['git_original_id']:r['canonical_original_id'] for r in hist}
save('original-fragment-parent-crosswalk-v1.json',dict(rows=[dict(fragment_id=f['fragment_id'],parent=f['member_id'],canonical_original_parent=canonical.get(f['member_id']),is_branch=f['is_branch'],is_inline=f['is_inline'],declaration_proof=proof['original_header_fragments'].get(f['fragment_id']),status='COVERED_BY_PROVEN_BODY_OR_DECLARATION' if f['member_id'] in supported or f['fragment_id'] in proof['original_header_fragments'] else 'OPEN') for f in original['fragments']]))
opens=[r for r in hist if not r['proof']]
# Continue the coherent sample-size family in the exact order of the C6 queue;
# preserve every other OPEN canonical record in its original machine order.
queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes());open_ids={r['canonical_original_id'] for r in opens}
remaining=[r for r in queue['members'] if r['original_id'] in open_ids]
priority=[r for r in remaining if r['path'].endswith('/SampleSizeUDFs.vb')]
priority.extend(r for r in remaining if r not in priority and r['path'].endswith('/UDFdataImport/UdfDataImportRangeReaders.vb'))
priority.extend(r for r in remaining if r not in priority)
save('remaining-work-queue-v1.json',dict(status='NOT_READY_04A38',historical_OPEN=len(opens),first_members=priority[:24],members=remaining,current_OPEN=[dict(current_id=n['symbol_id'],path=n['path'],name=n['name']) for n in u['members'] if n['symbol_id'] not in closed],support_OPEN=[dict(support_id=s['support_id'],path=s['path'],kind=s['kind']) for s in support if not s['fully_reconciled'] and not s['already_in_primary_population']],first_action='Continue remaining sample-size bodies in the exact C6 machine queue order with their report/input/option dependencies, then remaining range-reader parents. All24 C6 priority identities are proved. No new original reference queries.',owner_decision_needed=False))
save('independent-proof-draft-v1.json',proof)
verified=compute()
print(json.dumps({k:verified[k] for k in ['status','historical','current','support','fragments','W3']}))
