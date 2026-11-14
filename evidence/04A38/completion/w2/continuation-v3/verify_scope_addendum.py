"""Fresh host-cache full generic/static context and original fragment accounting."""
import json,hashlib
from pathlib import Path
from verify_names import ROOT,HERE,need
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()+'/'
 def load(p):return json.loads(overrides[p] if p in overrides else (root/p).read_bytes())
 h=load(base+'historical-ownership.json');canonical={r['git_original_id']:r['canonical_original_id'] for r in h['members']};closed={r['git_original_id'] for r in h['members'] if r['proof']};orig=load('evidence/04A38/completion/w1/original-universe.json');rows=load(base+'original-fragment-parent-crosswalk.json')['rows']
 need(len(rows)==len(orig['fragments']) and len({r['fragment_id'] for r in rows})==len(rows),'Lost/duplicated original fragments')
 for actual,original in zip(rows,orig['fragments']):
  need(actual==dict(fragment_id=original['fragment_id'],parent=original['member_id'],canonical_original_parent=canonical.get(original['member_id']),is_branch=original['is_branch'],is_inline=original['is_inline'],status='COVERED_BY_PROVEN_WHOLE_PARENT' if original['member_id'] in closed else 'OPEN'),'False original branch/inline parent closure')
 cov=load(base+'fragment-coverage-addendum.json');cur=load(base+'current-ownership.json')['fragments']
 def counts(rs,covered):return dict(total=len(rs),branches=sum(r['is_branch'] for r in rs),inline=sum(r['is_inline'] for r in rs),covered=sum(covered(r) for r in rs),branches_covered=sum(r['is_branch'] and covered(r) for r in rs),inline_covered=sum(r['is_inline'] and covered(r) for r in rs))
 need(cov['original']==counts(rows,lambda r:r['status'].startswith('COVERED')) and cov['current']==counts(cur,lambda r:r['review_status']=='COVERED_BY_REVIEWED_BODY'),'Fragment counts unsupported')
 proof=load('evidence/04A38/completion/w2/continuation-v2/host-cache-v4-proof.json');rechecked=[]
 for c in ['Debug','Release']:
  old=load('evidence/04A38/completion/w2/continuation-v2/metadata-v4/b1-v1/current-'+c+'.json');now=load(base+'metadata-v5/names-v3/current-'+c+'.json')
  oldm={m['method']:m for m in old['methods']};newm={m['method']:m for m in now['methods']};oldtypes={t['type']:t for t in old['types']};newtypes={t['type']:t for t in now['types']}
  # Only accepted original host/cache owners; all actual methods, not a call-name projection.
  owners={r['declaring_type'] for r in old['methods'] if any(s in r['declaring_type'] for s in ['ExcelArgPredicates','ExcelArgReaders','UdfModelHandles','UdfModelCache'])}
  # Scope is also derived from exact previous source proof member names.
  names={r['name'] for r in proof['methods']}
  selected=[m for m in old['methods'] if m['member_name'] in names and m['declaring_type'].startswith('BESHStatNG.') and not '.ExcelCommon.' in m['declaring_type']]
  need(len(selected)>=33,'Host proof member scope absent')
  for m in selected:
   need(m['method'] in newm and m==newm[m['method']],'Fresh host/cache full CIL/generic/EH drift')
   owner=m['declaring_type'];need(oldtypes[owner]==newtypes[owner],'Fresh host/cache fields/base/interface drift')
   for ctor in [v for v in old['methods'] if v['declaring_type']==owner and v['member_name'] in ['.ctor','.cctor']]:need(ctor['method'] in newm and ctor==newm[ctor['method']],'Fresh host/cache initializer drift')
  rechecked.append(dict(configuration=c,unchanged_full_method_records=len(selected),types=len({m['declaring_type'] for m in selected})))
 return dict(status='FRESH_HOST_CONTEXT_AND_ORIGINAL_FRAGMENT_ACCOUNTING_PASS_NOT_READY',coverage=cov,host_context=rechecked,original_queries=0)
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
