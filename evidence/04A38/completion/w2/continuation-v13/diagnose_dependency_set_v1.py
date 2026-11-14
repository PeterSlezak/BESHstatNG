from common import *
from proof_kernel_v6 import definitions
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());cs=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];d=json.loads((HERE/'cohort-diagnostic-v1.json').read_bytes())
meta=json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes());defs=definitions(meta);names={(defs[k]['declaring_type'],defs[k]['member_name']) for k in d['graphs']['Debug']['methods']}
actual={n['symbol_id'] for n in u['members'] if n['parent_member'] is None and (n['owner'],n['name']) in names};expected={c['current_id'] for c in cs}
print('extra',sorted(actual-expected));print('missing',sorted(expected-actual))
