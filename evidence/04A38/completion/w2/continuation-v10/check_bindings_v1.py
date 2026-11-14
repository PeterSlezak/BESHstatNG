"""Fail-closed complete declared member/arity/ByRef/array-rank binding diagnostic."""
from common import *
from proof_kernel_v1 import bind
u=json.loads((HERE/'current-universe-packet-v2.json').read_bytes());pu=json.loads((HERE/'portable-universe-v1.json').read_bytes());meta=json.loads((HERE/'metadata-all/packet-v2-context-v5/current-Debug.json').read_bytes());shapes={r['symbol_id']:r for r in json.loads((HERE/'declaration-shapes-v1.json').read_bytes())['rows']};ids=set(json.loads((HERE/'cohort-diagnostic-v1.json').read_bytes())['reviewed_candidate_ids']);result=[]
for n in [r for r in u['members'] if r['symbol_id'] in ids]+pu['members']:
    try:method=bind(n,meta,shapes);result.append(dict(id=n['symbol_id'],method=method['method'],status='BOUND'))
    except ValueError as error:result.append(dict(id=n['symbol_id'],status='REJECTED',reason=str(error),source=shapes.get(n['symbol_id']),candidates=[r['parameter_types'] for r in meta['methods'] if (r['declaring_type'],r['member_name'])==(n['owner'],n['name'])]))
save('binding-diagnostic-v1.json',dict(rows=result,failed=sum(r['status']!='BOUND' for r in result),original_queries=0));print(json.dumps([r for r in result if r['status']!='BOUND']))
