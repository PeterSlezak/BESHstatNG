from common import *
u=json.loads((HERE/'current-universe-b1-v1.json').read_bytes());old=json.loads((PREVIOUS/'current-ownership.json').read_bytes());by={}
for r in u['members']:by.setdefault(r['signature_key'],[]).append(r)
for r in old['members']:
 if r['review_status'].startswith('OPEN'):continue
 matches=by.get(r['signature_key'],[])
 if len(matches)!=1 or matches[0]['body_token_sha256']!=r['body_token_sha256']:print(r['symbol_id'],r['parent_member'],r['body_token_sha256'],[(m['symbol_id'],m['parent_member'],m['body_token_sha256']) for m in matches])
