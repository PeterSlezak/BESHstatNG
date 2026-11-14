"""Independently recompute all C5 partial populations; do not trust status fields."""
import json
from collections import Counter
from pathlib import Path
from verify_slice import ROOT, HERE, PREFIX, PRIOR, inspect as slice_proof, need, sha


def inspect(root=ROOT, overrides=None):
    root = Path(root)
    overrides = overrides or {}
    def raw(path):
        return overrides[path] if path in overrides else (root/path).read_bytes()
    def load(path):
        return json.loads(raw(path))
    proof = slice_proof(root, overrides)
    previous = load(PRIOR+'historical-ownership.json')['members']
    h = load(PREFIX+'historical-ownership.json')
    current = load(PREFIX+'current-ownership.json')
    universe = load(PREFIX+'current-universe-final-v2.json')
    old = load(PRIOR+'current-ownership.json')
    summary = load(PREFIX+'ownership-summary.json')
    expected = {r['canonical_original_id'] for r in previous if r['proof']} | set(proof['original_ids'])
    need(len(h['members'])==704 and Counter(r['canonical_original_id'] for r in h['members'])==Counter(r['canonical_original_id'] for r in previous), 'Historical identity population changed')
    need({r['canonical_original_id'] for r in h['members'] if r['proof']}==expected, 'Missing/unsupported historical whole-body proof')
    bykey = {}
    for r in universe['members']:
        bykey.setdefault(r['signature_key'],[]).append(r)
    valid = set()
    for r in old['members']:
        if r['review_status'].startswith('OPEN'):
            continue
        matches=[m for m in bykey[r['signature_key']] if m['body_token_sha256']==r['body_token_sha256'] and
                 (m['parent_member'] or '').split('@')[0]==(r['parent_member'] or '').split('@')[0]]
        need(len(matches)==1,'Lost inherited current body/callback')
        valid.add(matches[0]['symbol_id'])
    for row, before in zip(h['members'],previous):
        need(row['canonical_original_id']==before['canonical_original_id'],'Historical ordering changed')
        keys={i.split('@')[0] for i in before['current_ids']} | {before['git_original_id'].split('@')[0]}
        ids=sorted({m['symbol_id'] for key in keys for m in bykey.get(key,[])})
        need(row['current_ids']==ids,'Forged split/relocation/current mapping')
        if row['proof']:
            valid.update(ids)
    need(len(current['members'])==len(universe['members']) and Counter(r['symbol_id'] for r in current['members'])==Counter(r['symbol_id'] for r in universe['members']),'Current member population changed')
    need({r['symbol_id'] for r in current['members'] if not r['review_status'].startswith('OPEN')}==valid,'Unsupported/missing current body proof')
    for row in universe['sources']:
        need(sha(raw(row['path']))==row['sha256'],'Current syntax source dependency changed')
    need(Counter(r['fragment_id'] for r in current['fragments'])==Counter(r['fragment_id'] for r in universe['fragments']),'Missing branch/statement/inline obligation')
    actual_fragments={r['fragment_id']:r for r in universe['fragments']}
    for row in current['fragments']:
        original=actual_fragments[row['fragment_id']]
        need(all(row[k]==v for k,v in original.items()),'Altered current branch/source fragment')
        need((row['review_status']=='COVERED_BY_REVIEWED_BODY')==(row['member_id'] in valid),'Unsupported branch/inline parent coverage')
    prior_support={(r['path'],r['kind'],r['owner'],r['declaration']):r for r in old['support']}
    need(Counter(r['support_id'] for r in current['support'])==Counter(r['support_id'] for r in universe['supports']),'Support obligation dropped')
    for row in current['support']:
        prior=prior_support[(row['path'],row['kind'],row['owner'],row['declaration'])]
        need(row['fully_reconciled']==prior['fully_reconciled'] and row['disposition']==prior['disposition'],'Unsupported declaration/host-only closure')
        if row['kind'] in ['ModuleBlock','ClassBlock']:
            need(not row['fully_reconciled'],'Header substituted for children')
    original=load('evidence/04A38/completion/w1/original-universe.json')
    original_closed={r['git_original_id'] for r in h['members'] if r['proof']}
    canon={r['git_original_id']:r['canonical_original_id'] for r in h['members']}
    crosswalk=load(PREFIX+'original-fragment-parent-crosswalk.json')['rows']
    need(len(crosswalk)==len(original['fragments']),'Original fragment lost')
    for row, frag in zip(crosswalk,original['fragments']):
        need(row==dict(fragment_id=frag['fragment_id'],parent=frag['member_id'],canonical_original_parent=canon.get(frag['member_id']),
                       is_branch=frag['is_branch'],is_inline=frag['is_inline'],status='COVERED_BY_PROVEN_WHOLE_PARENT' if frag['member_id'] in original_closed else 'OPEN'),
             'Missing/unsupported original branch proof')
    def counts(rows, fn):
        return dict(total=len(rows),branches=sum(r['is_branch'] for r in rows),inline=sum(r['is_inline'] for r in rows),covered=sum(fn(r) for r in rows),
                    branches_covered=sum(r['is_branch'] and fn(r) for r in rows),inline_covered=sum(r['is_inline'] and fn(r) for r in rows))
    need(load(PREFIX+'fragment-coverage.json')==dict(original=counts(crosswalk,lambda r:r['status'].startswith('COVERED')),
         current=counts(current['fragments'],lambda r:r['member_id'] in valid),whole_universe_closed=False),'Fragment count drift')
    hs=dict(total=704,closed=len(expected),OPEN=704-len(expected))
    cs=dict(total=len(universe['members']),closed=len(valid),OPEN=len(universe['members'])-len(valid))
    ss=dict(raw=len(current['support']),distinct=sum(not r['already_in_primary_population'] for r in current['support']),
            distinct_closed=sum(r['fully_reconciled'] and not r['already_in_primary_population'] for r in current['support']),
            distinct_OPEN=sum(not r['fully_reconciled'] and not r['already_in_primary_population'] for r in current['support']))
    need(summary['historical']==hs and summary['current']==cs and summary['support']==ss,'Forged ownership totals')
    queue=load(PREFIX+'remaining-work-queue.json')
    need(Counter(r['original_id'] for r in queue['members'])==Counter(r['canonical_original_id'] for r in h['members'] if not r['proof']),'Incomplete remaining queue')
    need(Counter(r['current_id'] for r in queue['current_OPEN'])==Counter(r['symbol_id'] for r in current['members'] if r['review_status'].startswith('OPEN')),'Current queue incomplete')
    need(summary['status']=='NOT_READY_04A38' and summary['W2']=='IN_PROGRESS' and summary['W3']=='NOT_STARTED' and hs['OPEN']>0,'Unsupported READY or W3 promotion')
    return dict(status='INDEPENDENT_C5_PARTIAL_OWNERSHIP_PASS_NOT_READY',historical=hs,current=cs,support=ss,
                fragments=load(PREFIX+'fragment-coverage.json'),new_supported=proof['newly_supported'],original_queries=0,W3='NOT_STARTED')


if __name__=='__main__':
    try:
        print(json.dumps(inspect()))
    except Exception as error:
        print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))))
        raise SystemExit(1)
