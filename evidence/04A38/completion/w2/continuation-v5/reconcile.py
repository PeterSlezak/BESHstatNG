from common import *
from collections import defaultdict
from verify_slice import inspect

proof = inspect()
save('independent-slice-proof.json', proof)
u = json.loads((HERE / 'current-universe-final-v2.json').read_bytes())
old = json.loads((PREVIOUS / 'current-ownership.json').read_bytes())
history = json.loads((PREVIOUS / 'historical-ownership.json').read_bytes())['members']
review = {r['original_id']: r for r in json.loads((HERE / 'body-contracts.json').read_bytes())['contracts']}
bykey = defaultdict(list)
for row in u['members']:
    bykey[row['signature_key']].append(row)
closed = set()
members = []
for h in history:
    keys = {i.split('@')[0] for i in h['current_ids']} | {h['git_original_id'].split('@')[0]}
    ids = sorted({r['symbol_id'] for key in keys for r in bykey[key]})
    supporting = h['proof']
    if h['canonical_original_id'] in review:
        supporting = dict(kind='C5_COMPLETE_BODY_SOURCE_CIL_CONTEXT_DEPENDENCY_PROOF',
                          evidence='body-contracts.json', original_id=h['canonical_original_id'])
    if supporting:
        assert ids, h['canonical_original_id']
        closed.update(ids)
    members.append(dict(h, current_ids=ids, proof=supporting,
                        disposition='PROOF_SUPPORTED_REVIEWED_BODY' if supporting else 'OPEN_W2_WHOLE_BODY_OR_BOUNDARY_REVIEW_REQUIRED',
                        partial_implementation=None if supporting else h.get('partial_implementation')))
for row in old['members']:
    if row['review_status'].startswith('OPEN'):
        continue
    matches = [m for m in bykey[row['signature_key']] if m['body_token_sha256'] == row['body_token_sha256'] and
               (m['parent_member'] or '').split('@')[0] == (row['parent_member'] or '').split('@')[0]]
    assert len(matches) == 1, row['symbol_id']
    closed.add(matches[0]['symbol_id'])
support_old = {(r['path'],r['kind'],r['owner'],r['declaration']): r for r in old['support']}
support = []
for r in u['supports']:
    before = support_old[(r['path'],r['kind'],r['owner'],r['declaration'])]
    row = dict(r, disposition=before['disposition'], fully_reconciled=before['fully_reconciled'], reason=before['reason'])
    if 'proof' in before:
        row['proof'] = before['proof']
    if r['kind'] in ['ModuleBlock','ClassBlock']:
        row.update(fully_reconciled=False, children=[m['symbol_id'] for m in u['members'] if m['path']==r['path'] and
            m['span_start'] >= r['span_start'] and m['span_start']+m['span_length'] <= r['span_start']+r['span_length']])
    if r['kind'] == 'FieldDeclaration':
        row['aliases'] = [m['symbol_id'] for m in u['members'] if m['path']==r['path'] and m['span_start'] >= r['span_start'] and
                         m['span_start']+m['span_length'] <= r['span_start']+r['span_length']]
    support.append(row)
fragments = [dict(r,review_status='COVERED_BY_REVIEWED_BODY' if r['member_id'] in closed else 'OPEN_STATEMENT_INLINE_OR_BOUNDARY_REVIEW_REQUIRED') for r in u['fragments']]
save('historical-ownership.json', dict(status='NOT_READY_04A38',total=len(members),closed=sum(bool(r['proof']) for r in members),
                                     OPEN=sum(not r['proof'] for r in members),members=members))
current = dict(status='NOT_READY_04A38', members=[dict(r,review_status='REVIEWED_BODY_OR_EXPLICIT_CALLBACK' if r['symbol_id'] in closed else 'OPEN_WHOLE_BODY_REVIEW_REQUIRED') for r in u['members']],
               support=support,fragments=fragments,callback_proofs=old['callback_proofs'],
               new_portable_contracts=['UdfCellConversion38: complete legacy text conversion and uncaught finite conversion; typed numeric-code whitelist; actual Windows consumers'])
save('current-ownership.json',current)
summary = dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',
    historical=dict(total=len(members),closed=sum(bool(r['proof']) for r in members),OPEN=sum(not r['proof'] for r in members)),
    current=dict(total=len(u['members']),closed=len(closed),OPEN=len(u['members'])-len(closed)),
    support=dict(raw=len(support),distinct=sum(not r['already_in_primary_population'] for r in support),
                 distinct_closed=sum(r['fully_reconciled'] and not r['already_in_primary_population'] for r in support),
                 distinct_OPEN=sum(not r['fully_reconciled'] and not r['already_in_primary_population'] for r in support)),
    fragments=dict(total=len(fragments),branches=sum(r['is_branch'] for r in fragments),inline=sum(r['is_inline'] for r in fragments),
                   covered=sum(r['member_id'] in closed for r in fragments)),
    new_historical_supported=proof['newly_supported'],source_files=len(u['sources']),exports=268,new_original_queries=0,owner_decision_needed=False)
save('ownership-summary.json',summary)
original = json.loads((W1 / 'original-universe.json').read_bytes())
closed_original = {r['git_original_id'] for r in members if r['proof']}
canon = {r['git_original_id']:r['canonical_original_id'] for r in members}
rows = [dict(fragment_id=r['fragment_id'],parent=r['member_id'],canonical_original_parent=canon.get(r['member_id']),
             is_branch=r['is_branch'],is_inline=r['is_inline'],status='COVERED_BY_PROVEN_WHOLE_PARENT' if r['member_id'] in closed_original else 'OPEN') for r in original['fragments']]
save('original-fragment-parent-crosswalk.json',dict(rows=rows))
def counts(rs, fn):
    return dict(total=len(rs),branches=sum(r['is_branch'] for r in rs),inline=sum(r['is_inline'] for r in rs),
                covered=sum(fn(r) for r in rs),branches_covered=sum(r['is_branch'] and fn(r) for r in rs),
                inline_covered=sum(r['is_inline'] and fn(r) for r in rs))
save('fragment-coverage.json',dict(original=counts(rows,lambda r:r['status'].startswith('COVERED')),
    current=counts(fragments,lambda r:r['review_status']=='COVERED_BY_REVIEWED_BODY'),whole_universe_closed=False))
opens = [r for r in members if not r['proof']]
priority = ['TryReadIndependentNumericColumns','TryGetIndependentNumericColumns','TryReadPairedNumericColumns','TryGetPairedNumericColumns',
            'ExtractCompletePairedNumericRows','TryReadCompleteNumericMatrixWithHeaders','TryGetCompleteNumericMatrixWithHeaders',
            'TryGetSingleNumericColumn','TryGetOptionalProbabilityThresholds','TryAppendProbabilityThreshold',
            'TryGetOptionalThresholdVector','TryGetSingleThresholdFromArg','RMANOVA1','ANOVA2_NESTED','RMANOVA1_MCP',
            'TTEST_UNPAIRED','TTEST_PAIRED','TTEST_UNPAIRED_NI','TTEST_UNPAIRED_EQUIV',
            'MaterializePortableReport38','BuildNamedMatrixOutput','BuildCaseMatrixOutput','PrepareWrappedResultTableForUdf']
first = [dict(original_id=r['canonical_original_id'],path=r['path'],name=r['name']) for name in priority for r in opens if r['name']==name]
save('remaining-work-queue.json',dict(status='NOT_READY_04A38',historical_OPEN=len(opens),first_members=first,
    members=[dict(original_id=r['canonical_original_id'],path=r['path'],name=r['name'],next='Complete source/caller/portable/host/exception/ByRef/fragment/dependency proof; no new original-reference authority') for r in opens],
    current_OPEN=[dict(current_id=r['symbol_id'],path=r['path'],name=r['name']) for r in current['members'] if r['review_status'].startswith('OPEN')],
    support_OPEN=[dict(support_id=r['support_id'],path=r['path'],kind=r['kind']) for r in support if not r['fully_reconciled'] and not r['already_in_primary_population']],
    first_action='Complete independent/paired/repeated-matrix import and probability-vector dependencies, then their RM/nested/t-test parents. ANOVA1/ANOVA1_WELCH/ANOVA1_MCP and first primitive conversions are supported.',owner_decision_needed=False))
save('per-family-ownership.json',dict(historical=[dict(path=p,total=sum(r['path']==p for r in members),closed=sum(r['path']==p and bool(r['proof']) for r in members),OPEN=sum(r['path']==p for r in opens)) for p in sorted({r['path'] for r in members})],
    current=[dict(path=p,total=sum(r['path']==p for r in current['members']),closed=sum(r['path']==p and not r['review_status'].startswith('OPEN') for r in current['members']),OPEN=sum(r['path']==p and r['review_status'].startswith('OPEN') for r in current['members'])) for p in sorted({r['path'] for r in current['members']})]))
print(json.dumps(summary))
