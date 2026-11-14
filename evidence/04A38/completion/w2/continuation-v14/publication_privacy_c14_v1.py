"""Unchanged policy, authenticated exact C13 decisions; changed/new inputs fail closed."""
import json,sys,hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
sys.path.append(str(HERE.parent/'continuation-v6'))
import publication_policy as policy
need=policy.need;sha=policy.sha;safe_path=policy.safe_path
def checked(row):
    data=(ROOT/row['path']).read_bytes();need((len(data),sha(data))==(row['bytes'],row['sha256']),'Privacy authority changed '+row['path']);return data
authority=json.loads((HERE/'publication-privacy-authority-v1.json').read_bytes());checked(authority['base_policy'])
need(Path(policy.__file__).resolve()==(ROOT/authority['base_policy']['path']).resolve(),'Wrong loaded base privacy policy')
previous=json.loads(checked(authority['C10_publication_receipt']));records=previous['physical_zip']['records']
need(previous['physical_zip']['status']=='ALL_PHYSICAL_MEMBERS_SHA_STORED_CRC_PRIVACY_PASS' and len(records)==7075,'Wrong historical physical privacy authority')
cache={(r['path'],r['bytes'],r['sha256']):[] for r in records}
for finding in previous['physical_zip']['privacy_findings']:
    row=next(r for r in records if r['path']==finding['path']);cache[row['path'],row['bytes'],row['sha256']].append(finding)
manifest=json.loads(checked(authority['C13_complete_manifest']));need(len(manifest['entries'])==8516 and manifest['privacy_policy']==authority['base_policy'],'Wrong C13 policy/inventory')
for row in manifest['entries']:
    if row['sha256'] is None:continue
    decision=row['privacy_decision'];need(decision['status']=='POLICY_EVALUATED' and decision['policy_sha256']==authority['base_policy']['sha256'] and decision['content_sha256']==row['sha256'],'Missing C13 exact policy/content authority')
    cache[row['path'],row['bytes'],row['sha256']]=decision['findings']
stats=dict(prior_exact=0,fresh=0)
def privacy(path,data):
    key=path,len(data),sha(data)
    if key in cache:stats['prior_exact']+=1;return cache[key]
    stats['fresh']+=1;return policy.privacy(path,data)
