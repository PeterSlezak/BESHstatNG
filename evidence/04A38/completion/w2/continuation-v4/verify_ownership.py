"""Independent population conservation, source bindings and fail-closed partial ownership."""
from functools import cache
import json,hashlib
from pathlib import Path
from verify_slice_v4 import ROOT,HERE,need
import verify_slice_v4,verify_retained,verify_text_domains_v2
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()+'/'
 def raw(p):return overrides[p] if p in overrides else (root/p).read_bytes()
 @cache
 def load(p):return json.loads(raw(p))
 slice=verify_slice_v4.inspect(root,overrides);retained=verify_retained.inspect(root,overrides);text=verify_text_domains_v2.inspect(root,overrides)
 h=load(base+'historical-ownership.json');current=load(base+'current-ownership.json');u=load(base+'current-universe-parametric-v4.json');prior=load('evidence/04A38/completion/w2/continuation-v3/historical-ownership.json');old=load('evidence/04A38/completion/w2/continuation-v3/current-ownership.json');summary=load(base+'ownership-summary.json')
 need(hashlib.sha256(raw(base+'current-universe-parametric-v4.json')).hexdigest()=='8379d4732e790ca8bcf2904c699cd05509a2360422c9bc8358cebd0a32eb6843','Authenticated fresh syntax universe changed')
 expected={r['canonical_original_id'] for r in prior['members'] if r['proof']}|{r['original_id'] for r in slice['proofs']}
 need(len(h['members'])==704 and {r['canonical_original_id'] for r in h['members']}=={r['canonical_original_id'] for r in prior['members']},'Historical population lost/duplicated')
 need({r['canonical_original_id'] for r in h['members'] if r['proof']}==expected,'Unsupported historical/host-only closure')
 bykey={}
 for m in u['members']:bykey.setdefault(m['signature_key'],[]).append(m)
 valid=set()
 for r in old['members']:
  if r['review_status'].startswith('OPEN'):continue
  matches=[m for m in bykey[r['signature_key']] if m['body_token_sha256']==r['body_token_sha256'] and (m['parent_member'] or '').split('@')[0]==(r['parent_member'] or '').split('@')[0]];need(len(matches)==1,'Lost/ambiguous retained reviewed body or callback');valid.add(matches[0]['symbol_id'])
 for r in h['members']:
  if r['proof']:valid.update(r['current_ids'])
 for r in u['sources']:need(hashlib.sha256(raw(r['path'])).hexdigest()==r['sha256'],'Stale source census')
 need(len(current['members'])==len(u['members']) and {r['symbol_id'] for r in current['members']}=={r['symbol_id'] for r in u['members']},'Current identity lost/duplicated')
 need({r['symbol_id'] for r in current['members'] if not r['review_status'].startswith('OPEN')}==valid,'Unsupported current closure')
 need(len(current['fragments'])==len(u['fragments']) and {r['fragment_id'] for r in current['fragments']}=={r['fragment_id'] for r in u['fragments']},'Fragment/branch/inline omitted')
 for r in current['fragments']:need((r['review_status']=='COVERED_BY_REVIEWED_BODY')==(r['member_id'] in valid),'Forged parent fragment coverage')
 need(len(current['support'])==325 and {r['support_id'] for r in current['support']}=={r['support_id'] for r in u['supports']},'Support dropped or duplicate deduplication')
 olds={(r['path'],r['kind'],r['owner'],r['declaration']):r for r in old['support']}
 for r in current['support']:
  before=olds[(r['path'],r['kind'],r['owner'],r['declaration'])];need(r['fully_reconciled']==before['fully_reconciled'],'Unsupported support/container closure')
  if r['kind'] in ['ModuleBlock','ClassBlock']:need(not r['fully_reconciled'],'Container auto-closed children')
 need(summary['historical']==dict(total=704,closed=len(expected),OPEN=704-len(expected)) and summary['current']==dict(total=len(u['members']),closed=len(valid),OPEN=len(u['members'])-len(valid)),'Ownership count forged')
 need(summary['support']==dict(raw=325,distinct=303,distinct_closed=234,distinct_OPEN=69),'Support count forged')
 need(summary['fragments']==dict(total=len(u['fragments']),branches=sum(r['is_branch'] for r in u['fragments']),inline=sum(r['is_inline'] for r in u['fragments']),covered=sum(r['member_id'] in valid for r in u['fragments'])),'Fragment counts forged')
 need(summary['status']=='NOT_READY_04A38' and summary['W2']=='IN_PROGRESS' and summary['W3']=='NOT_STARTED' and len(expected)<704,'False READY/W2 completion')
 need(summary['new_original_queries']==0,'Original-reference authority exceeded')
 return dict(status='INDEPENDENT_C4_PARTIAL_OWNERSHIP_PASS_NOT_READY',historical=summary['historical'],current=summary['current'],support=summary['support'],fragments=summary['fragments'],retained_context=retained,new_proven_bodies=len(slice['proofs']),public_ANOVA_bodies='OPEN:remaining import/output dependencies',text=text,original_queries=0)
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
