"""Exact source binder plus measured VB nested-delegate dotted spelling."""
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
