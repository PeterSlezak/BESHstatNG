from common import *
from proof_kernel_v6 import definitions
for config in ['Debug','Release']:
 od=definitions(json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')).read_bytes()))
 cs=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
 for c in cs:
  r=next(r for r in c['compiled'] if r['configuration']==config)
  if c['original_id'] and r['original_method']['method'] not in od:print('missing_contract',config,c['current_record']['name'],r['original_method']['method'])
 print('IsMissingArg records',config,[k for k in od if '.IsMissingArg(' in k])
