from common import *
from verify_checkpoint_v1 import compute
proof=compute(check_ledgers=False)
u=json.loads((HERE/'current-universe-families-v2.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v3.json').read_bytes())['members']
old=json.loads((PREVIOUS/'current-ownership-v3.json').read_bytes())
closed=set(proof['closed_current']);new=set(proof['newly_supported_historical'])
hist=[]
for before,expected in zip(history,proof['historical_dispositions']):
 supporting=before['proof']
 if before['canonical_original_id'] in new:supporting=dict(kind='C6_COMPLETE_ORIGINAL_CURRENT_SOURCE_CIL_DEPENDENCY_PROOF',evidence='whole-body-contracts-v1.json',original_id=before['canonical_original_id'])
 hist.append(dict(before,current_ids=expected['current_ids'],proof=supporting,disposition='PROOF_SUPPORTED_REVIEWED_BODY' if supporting else 'OPEN_W2_WHOLE_BODY_OR_BOUNDARY_REVIEW_REQUIRED',partial_implementation=None if supporting else before.get('partial_implementation')))
save('historical-ownership-v1.json',dict(status='NOT_READY_04A38',**proof['historical'],members=hist))
support=[];dispositions={r['id']:r for r in proof['support_dispositions']}
bydecl={(r['path'],r['owner'],r['kind'],r['declaration']):r for r in old['support']}
for row in u['supports']:
 prior=bydecl[row['path'],row['owner'],row['kind'],row['declaration']];d=dispositions[row['support_id']]
 supporting=prior.get('proof')
 if d['supported'] and not prior['fully_reconciled']:supporting=dict(kind='C6_COMPLETE_STATELESS_MODULE_CONTEXT_AND_ALL_CHILDREN',children=d['children'],compiled='compiled-context-and-consumers.json')
 support.append(dict(row,canonical_v5_support_id=prior['support_id'],fully_reconciled=d['supported'],children=d['children'],
     disposition='SOURCE_AND_COMPILED_CONTEXT_SUPPORTED' if d['supported'] else 'OPEN_COMPLETE_CONTAINER_OR_BEHAVIOR_REVIEW_REQUIRED',
     reason='Stateless declaration source/CLR type/field context and every contained body supported' if supporting and not prior['fully_reconciled'] else prior.get('reason'),proof=supporting))
fragments=[dict(r,review_status='COVERED_BY_REVIEWED_BODY_OR_DECLARATION' if r['member_id'] in closed or r['fragment_id'] in proof['header_fragments'] else 'OPEN_STATEMENT_INLINE_OR_BOUNDARY_REVIEW_REQUIRED',
    declaration_proof=proof['header_fragments'].get(r['fragment_id'])) for r in u['fragments']]
save('current-ownership-v1.json',dict(status='NOT_READY_04A38',members=[dict(r,review_status='REVIEWED_COMPLETE_BODY_OR_EXPLICIT_CALLBACK' if r['symbol_id'] in closed else 'OPEN_WHOLE_BODY_REVIEW_REQUIRED') for r in u['members']],support=support,fragments=fragments,callback_proofs=json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows']))
save('ownership-summary-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical=proof['historical'],current=proof['current'],support=proof['support'],fragments=proof['fragments'],new_historical_supported=len(new),source_files=len(u['sources']),exports=268,original_queries=0))
save('fragment-coverage-v1.json',proof['fragments'])
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());orig_closed={r['git_original_id'] for r in hist if r['proof']}
canon={r['git_original_id']:r['canonical_original_id'] for r in hist}
save('original-fragment-parent-crosswalk-v1.json',dict(rows=[dict(fragment_id=r['fragment_id'],parent=r['member_id'],canonical_original_parent=canon.get(r['member_id']),
    is_branch=r['is_branch'],is_inline=r['is_inline'],declaration_proof=proof['original_header_fragments'].get(r['fragment_id']),status='COVERED_BY_PROVEN_BODY_OR_DECLARATION' if r['member_id'] in orig_closed or r['fragment_id'] in proof['original_header_fragments'] else 'OPEN') for r in original['fragments']]))
opens=[r for r in hist if not r['proof']]
priority=['TryGetTrimmedColumnObject','TryGetTrimmedNumericMatrixObject','TryReadNumericColumn','TryReadNumericMatrix','TryGetNumericColumn','TryGetRequiredOffsetPredictionInputs',
 'PrepareExistingObjectTableForUdf','BuildSimpleNoteTable','AD_NORMAL','SHAPIRO_WILK','GRUBBS','ROSNER']
first=[r for name in priority for r in opens if r['name']==name]
first.extend(r for r in opens if r not in first and r['path'].endswith(('/AssumptionsUDFs.vb','/DistributionUDFs.vb','/SampleSizeUDFs.vb')))
save('remaining-work-queue-v1.json',dict(status='NOT_READY_04A38',historical_OPEN=len(opens),first_members=[dict(original_id=r['canonical_original_id'],path=r['path'],name=r['name']) for r in first[:24]],
    members=[dict(original_id=r['canonical_original_id'],path=r['path'],name=r['name'],next='Complete original/current body, dependency, native boundary, CIL, branch and inline proof; no new original-reference query authority') for r in opens],
    current_OPEN=[dict(current_id=r['symbol_id'],path=r['path'],name=r['name']) for r in u['members'] if r['symbol_id'] not in closed],
    support_OPEN=[dict(support_id=r['support_id'],path=r['path'],kind=r['kind']) for r in support if not r['fully_reconciled'] and not r['already_in_primary_population']],
    first_action='Complete trimmed-column/matrix/offset import and remaining output adapters, then normality/outlier, distribution and sample-size parent families. C5 first22 canonical queue entries are now supported.',owner_decision_needed=False))
save('per-family-ownership-v1.json',dict(historical=[dict(path=p,total=sum(r['path']==p for r in hist),closed=sum(r['path']==p and bool(r['proof']) for r in hist),OPEN=sum(r['path']==p for r in opens)) for p in sorted({r['path'] for r in hist})]))
save('independent-proof-v1.json',proof)
verified=compute()
print(json.dumps({k:verified[k] for k in ['historical','current','support','fragments','W3']}))
