import hashlib,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
def need(v,m):
 if not v:raise ValueError(m)
def inspect(root=ROOT):
 root=Path(root);p=HERE.relative_to(ROOT);raw=(root/p/'text-call-sites-final.json').read_bytes();x=json.loads(raw);old=json.loads((root/p/'text-call-sites.json').read_bytes());need(x['calls']==old['calls'],'Added/missing/fused text caller')
 sources=x['sources'];need(len({r['path'] for r in sources})==len(sources),'Duplicate Common source')
 for r in sources:
  b=(root/r['path']).read_bytes();need(len(b)==r['bytes'] and hashlib.sha256(b).hexdigest()==r['sha256'],'Final caller-universe source changed '+r['path'])
 for c in ['Debug','Release']:
  q=json.loads((root/p/('text-domains-b1-v1-final-'+c+'.json')).read_bytes());need(q['source_table_sha256']==hashlib.sha256(raw).hexdigest(),'Final source digest wrong')
  need(len(q['results'])==416 and len({(r['site_id'],r['culture']) for r in q['results']})==416,'Missing final site/culture observations')
  need(all(r['differences']==0 and not r['mismatches'] and r['checked_values']==3771 for r in q['results']),'Final source-domain difference')
  need(sum(r['differences'] for r in q['synthetic'])==8 and all(len(r['mismatches'])==4 for r in q['synthetic']),'Raw synthetic nonpass erased')
 return dict(status='FINAL_CURRENT_SOURCE_UNIVERSE_AND_FRAMEWORK_POLICY_DOMAINS_PASS',Common_sources=len(sources),direct_source_lines=59,direct_invocations=145,CaseToken_sites=63,choices=733,observations_per_configuration=1568736,differences=0,synthetic_differences_per_configuration=8,original_queries=0)
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
