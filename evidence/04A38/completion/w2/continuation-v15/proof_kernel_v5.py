"""Exact prior binder plus explicitly proved repeated-KM callback identities only."""
from proof_kernel_v4 import *

def map_current(before,after,callback_context=None):
    exact={n['symbol_id']:n for n in after};bykey=defaultdict(list);result={};used=set()
    for n in after:bykey[n['signature_key']].append(n)
    callback_context=callback_context or {}
    for n in before:
        if n['symbol_id'] in exact:
            candidate=exact[n['symbol_id']];need(candidate['signature_key']==n['signature_key'],'Exact source ID signature changed')
            if n['parent_member'] is not None:need(candidate['body_sha256']==n['body_sha256'],'Existing callback body changed')
        else:
            choices=[v for v in bykey[n['signature_key']] if (v['parent_member'] or '').split('@')[0]==(n['parent_member'] or '').split('@')[0] and (n['parent_member'] is None or v['body_sha256']==n['body_sha256'])]
            if len(choices)!=1:
                need(n['symbol_id'] in callback_context,'Ambiguous source identity without complete source/CIL callback context')
                need('PlotDataUDFs|SingleLineFunctionLambdaExpression|8cbe8cb0546eb818@' in n['symbol_id'] and '|FunctionBlock|0fcb4b292f4645d0@' in (n['parent_member'] or ''),'Unsupported special callback mapping')
                choices=[v for v in choices if v['symbol_id']==callback_context[n['symbol_id']]];used.add(n['symbol_id'])
            need(len(choices)==1,'Missing/overlapping explicitly bound callback');candidate=choices[0]
        result[n['symbol_id']]=candidate
    need(len({n['symbol_id'] for n in result.values()})==len(result),'Overlapping old/current source identity')
    need(used==set(callback_context),'Unused/unjustified special callback mapping')
    return result
