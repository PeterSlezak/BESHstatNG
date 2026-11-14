import json,hashlib
from pathlib import Path
from verify_slice_v2 import ROOT,HERE,need
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()+'/'
 def raw(p):return overrides.get(p,(root/p).read_bytes())
 t=raw(base+'text-call-sites.json');table=json.loads(t);calls=table['calls'];domain=json.loads(raw(base+'text-caller-domains.json'));ids={r['site_id'] for r in calls}
 need(len(calls)==len(ids)==209 and len([r for r in calls if r['kind']=='EqualText'])==145 and len({r['line_site_id'] for r in calls if r['kind']=='EqualText'})==59,'Caller universe wrong')
 for s in table['sources']:
  b=raw(s['path']);need(len(b)==s['bytes'] and hashlib.sha256(b).hexdigest()==s['sha256'],'Stale Common source universe')
 for r in calls:
  vals=[r['right_literal']] if r['kind']=='EqualText' else r['choices'];need(all(isinstance(v,str) and all(32<=ord(ch)<=126 for ch in v) for v in vals),'Reachable null/ignorable/dynamic RHS')
  b=raw(r['path']);snippet=b.decode('utf-8-sig').encode('utf-16-le')[r['span_start']*2:(r['span_start']+r['span_length'])*2].decode('utf-16-le');need(r['right_expression'] in snippet,'Forged source span')
 need({r['site_id'] for r in domain['calls']}==ids and all(not r['normalization_to_be_independently_classified'] for r in domain['calls']),'Unreviewed caller domain')
 policy=raw('BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb');need(hashlib.sha256(policy).hexdigest()=='d42882ab96ae198e6e8c2c260f38d3e59439ab1adbe7c49e1c4f9347fb1c46cf','Policy changed')
 for c in ['Debug','Release']:
  p=json.loads(raw(base+'text-domains-parametric-v4-v4-'+c+'.json'));need(p['source_table_sha256']==hashlib.sha256(t).hexdigest(),'Wrong syntax table digest')
  rr=p['results'];need(len(rr)==418 and {(r['site_id'],r['culture']) for r in rr}=={(id,c) for id in ids for c in ['Invariant','sk-SK']},'Source/culture observation missing/duplicate')
  need(all(r['checked_values']==3771 and r['differences']==0 and not r['mismatches'] for r in rr),'Source-domain runtime difference')
  need(len(p['synthetic'])==2 and all(r['differences']==4 and len(r['mismatches'])==4 and r['nonnull_differences']==0 for r in p['synthetic']),'Erased raw synthetic nonpass')
 return dict(status='CURRENT_DOMAIN_EQUIVALENCE_PASS_NOT_GLOBAL_TEXT_IDENTITY',direct_lines=59,direct_calls=145,CaseToken_sites=64,choices=750,comparisons_per_configuration=1576278,source_domain_differences=0,raw_synthetic_differences_per_configuration=8,policy_unchanged=True,original_queries=0)
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
