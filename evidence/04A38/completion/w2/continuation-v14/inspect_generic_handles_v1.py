from common import *
q=json.loads((HERE/'entry-queue-receipt-v1.json').read_bytes());owners={(n['owner'],n['name']) for n in q['required_native_parents']};meta=json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes())
for m in meta['methods']:
 if (m['declaring_type'],m['member_name']) in owners and None in m['parameter_types']:print(json.dumps(dict(method=m['method'],parameter_types=m['parameter_types'],attributes=m['attributes'])))
for r in q['required_native_parents']:
 if r['name']=='TryGetCachedHandle':print(r['signature'])
