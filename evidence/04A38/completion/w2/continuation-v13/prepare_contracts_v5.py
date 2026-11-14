"""Correct explicit VB nested delegate spelling; preserve the rejected v4 attempt."""
from common import *
source=HERE/'build_contracts_v4.py';value=source.read_text()
kernel='''"""Exact source binder plus measured VB nested-delegate dotted spelling."""
from proof_kernel_v5 import *
import proof_kernel_v4 as previous

def bind(node,meta,shapes):
    params=shapes[node['symbol_id']]['parameters'] or []
    types={t['type'] for t in meta['types'] if t['type']}
    def matches(p,a):
        if parameter_matches(p,a) or nested_parameter_matches(node,p,a,meta):return True
        plain=a.removesuffix('&')
        return '+' in plain and plain in types and parameter_matches(p,a.replace('+','.'))
    candidates=[r for r in meta['methods'] if (r['declaring_type'],r['member_name'])==(node['owner'],node['name']) and len(r['parameter_types'])==len(params) and all(matches(p,a) for p,a in zip(params,r['parameter_types']))]
    need(len(candidates)==1,'Missing/ambiguous complete source/actual nested delegate binding '+node['symbol_id'])
    return candidates[0]
'''
with (HERE/'proof_kernel_v6.py').open('x',encoding='utf-8',newline='\n') as f:f.write(kernel)
value=value.replace('from proof_kernel_v5 import','from proof_kernel_v6 import')
with (HERE/'build_contracts_v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(value)
save('contract-binder-correction-v1.json',dict(rejected_source=pin(source),successor=pin(HERE/'build_contracts_v5.py'),binder=pin(HERE/'proof_kernel_v6.py'),
    reason='VB UdfModelProfilePreparation38.FiniteDoubleReader is the actual compiled nested type UdfModelProfilePreparation38+FiniteDoubleReader. Require measured type membership, full owner/name/parameter/ByRef/rank and unique candidate. No ordinal guessing.',
    premature_consumers=['postscan-nonpass-deltas-v1','source-composition-v1'],qualification='Those two attempts stopped at missing contract/transition inputs. Both failures and raw streams remain.',original_queries=0))
