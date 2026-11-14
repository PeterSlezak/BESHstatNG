from common import *
old=json.loads((PREVIOUS/'historical-ownership.json').read_bytes())['members']
for r in old:
 if any(x in r['canonical_original_id'] for x in ['838c70b','581132','824c4e','0b82712','0b97495']) or '|VariableDeclarator|' in r['canonical_original_id']:print(json.dumps({k:r.get(k) for k in ['canonical_original_id','path','current_ids','proof']},ensure_ascii=False))
u=json.loads((HERE/'current-universe-b1-v1.json').read_bytes());print('member',u['members'][0]);print('support',u['supports'][0]);print('fragment',u['fragments'][0]);print('totals',len(u['members']),len(u['supports']),len(u['fragments']))
for c in ['Debug','Release']:
 x=json.loads((HERE/'metadata/b1-v1'/('original-'+c+'.json')).read_bytes());y=json.loads((HERE/'metadata/b1-v1'/('current-'+c+'.json')).read_bytes());print(c,'metadata',len(x['types']),len(y['types']),len(x['methods']),len(y['methods']))
