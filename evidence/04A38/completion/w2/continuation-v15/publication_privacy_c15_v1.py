"""Exact authenticated C14 decisions and unchanged base policy for new bytes."""
from common import *
sys.path.append(str(W2/'continuation-v6'))
import publication_policy as policy
safe_path=policy.safe_path
base=pin(W2/'continuation-v6/publication_policy.py')
previous=json.loads((PREVIOUS/'publication-manifest-v2.json').read_bytes())
assert previous['privacy_policy']==base and len(previous['entries'])==9067
cache={}
for row in previous['entries']:
 if row['sha256'] is None:continue
 decision=row['privacy_decision']
 assert decision['status']=='POLICY_EVALUATED' and decision['policy_sha256']==base['sha256'] and decision['content_sha256']==row['sha256']
 cache[row['path'],row['bytes'],row['sha256']]=decision['findings']
prior_keys=set(cache)
stats=dict(prior_exact=0,current_exact=0,fresh=0)
checkpoint=HERE/'publication-privacy-decisions-v1.json'
if checkpoint.exists():
 for row in json.loads(checkpoint.read_bytes())['decisions']:
  cache[row['path'],row['bytes'],row['sha256']]=row['findings']

def privacy(path,data):
 key=path,len(data),sha(data)
 if key in cache:
  stats['prior_exact' if key in prior_keys else 'current_exact']+=1
  return cache[key]
 stats['fresh']+=1
 return policy.privacy(path,data)
