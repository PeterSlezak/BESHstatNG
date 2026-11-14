import json
from pathlib import Path
from verify_slice_v5 import ROOT,HERE,need
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()+'/'
 def load(p):return json.loads(overrides[p] if p in overrides else (root/p).read_bytes())
 h=load(base+'historical-ownership.json');closed={r['git_original_id'] for r in h['members'] if r['proof']};canonical={r['git_original_id']:r['canonical_original_id'] for r in h['members']};o=load('evidence/04A38/completion/w1/original-universe.json');rows=load(base+'original-fragment-parent-crosswalk.json')['rows'];need(len(rows)==len(o['fragments']),'Original branch universe lost')
 for r,a in zip(rows,o['fragments']):need(r==dict(fragment_id=a['fragment_id'],parent=a['member_id'],canonical_original_parent=canonical.get(a['member_id']),is_branch=a['is_branch'],is_inline=a['is_inline'],status='COVERED_BY_PROVEN_WHOLE_PARENT' if a['member_id'] in closed else 'OPEN'),'Original branch/inline parent forged')
 def counts(rs,fn):return dict(total=len(rs),branches=sum(r['is_branch'] for r in rs),inline=sum(r['is_inline'] for r in rs),covered=sum(fn(r) for r in rs),branches_covered=sum(r['is_branch'] and fn(r) for r in rs),inline_covered=sum(r['is_inline'] and fn(r) for r in rs))
 c=load(base+'current-ownership.json')['fragments'];expected=dict(original=counts(rows,lambda r:r['status'].startswith('COVERED')),current=counts(c,lambda r:r['review_status']=='COVERED_BY_REVIEWED_BODY'),whole_universe_closed=False);need(load(base+'fragment-coverage.json')==expected,'Fragment aggregate forged');return expected
