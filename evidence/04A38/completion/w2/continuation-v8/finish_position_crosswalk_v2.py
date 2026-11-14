"""Finish explicit mapping of duplicate lambda signatures, preserving failed draft."""
from common import *
current=json.loads((HERE/'current-ownership-v2.json').read_bytes())
source=(HERE/'finish_ownership_v2.py').read_text(encoding='utf-8');tail=source[source.index("old=json.loads((PREVIOUS/'current-ownership-v1.json')"):]
needle="    assert len(choices)==1,n['symbol_id'];r=choices[0]"
replacement="""    if len(choices)>1:
        exact=[r for r in choices if r['symbol_id']==n['symbol_id']]
        if exact:choices=exact
        else:choices=[r for r in choices if r['body_token_sha256']==n['body_token_sha256']]
    assert len(choices)==1,n['symbol_id'];r=choices[0]"""
assert needle in tail;tail=tail.replace(needle,replacement)
exec(compile(tail,str(HERE/'finish_position_crosswalk_v2.py')+':versioned-tail','exec'))
