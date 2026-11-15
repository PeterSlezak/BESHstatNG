from common40_v3 import *
lock()
# Versioned report readers are corrected; successful extractions and prior adverse reports are preserved.
s=(PRIVATE/'pending/compiled-index-v1.py').read_text(encoding='utf-8')
old="if isinstance(v,dict):return {k:canonical(x) for k,x in v.items() if k!='raw_IL_sha256'}"
new="""if isinstance(v,dict):
        out={k:canonical(x) for k,x in v.items() if k!='raw_IL_sha256'}
        if 'type' in v and 'fields' in v and '+_Closure$__' in v['type'] and all(f['is_static'] for f in v['fields']):
            out['fields']=sorted(out['fields'],key=lambda x:json.dumps(x,sort_keys=True))
        return out"""
assert old in s;s=s.replace(old,new)
for a,b in [("'abi-cil/'+config+'/current.json'","'abi-cil-final2/'+config+'/current.json'"),('method-index-v1.json','method-index-v2.json'),('changed-method-records-v1.json','changed-method-records-v2.json'),("'abi-'+config+'-canonical-changes-v3.json'","'abi-final2-'+config+'-canonical-changes-v3.json'"),('config+\'-proof-v1.json\'','config+\'-proof-v2.json\''),('complete-index-readback-v1.json','complete-index-readback-v2.json')]:
    assert a in s,a;s=s.replace(a,b)
s=s.replace("type_field_base_interface_multiset_exact=True", "type_field_base_interface_multiset_exact=True,private_static_generic_cache_field_enumeration_order='Three unchanged Core compiler closure types enumerated static fields in a different reflection order. Canonical per-type field multiset preserves every name/type/visibility/value; no public/instance layout order normalized.'")
(HERE/'compiled-index-v2.py').open('x',encoding='utf-8').write(s)
for name in ['node-readback-v3.py','review_readback.py','publish-v1.py']:
    p=PRIVATE/'pending'/name;s=p.read_text(encoding='utf-8')
    if name in ['review_readback.py','publish-v1.py']:
        for a,b in [('Debug-identities-v1.json','Debug-identities-v2.json'),('Release-identities-v1.json','Release-identities-v2.json'),('proof-v1.json','proof-v2.json'),('current-only-v3.json','current-only-v4.json'),('retained-inputs-fresh-results-v1.json','retained-inputs-fresh-results-v2.json'),('parent-event-reconciliation-v1.json','parent-event-reconciliation-v2.json'),('matrix/source-freeze-v1.json','matrix/source-freeze-v2.json'),("'abi-cil/'+config+'/exports.json'","'abi-cil-final2/'+config+'/exports.json'")]:s=s.replace(a,b)
        if name=='publish-v1.py':s=s.replace("PRIVATE/'pending/review_readback.py'","HERE/'review_readback.py'").replace("PRIVATE/'after-wave2-v2.json'","PRIVATE/'fixtures-final2-Debug/observations.json'").replace("PRIVATE/'fixtures-final-Release/observations.json'","PRIVATE/'fixtures-final2-Release/observations.json'")
    (HERE/name).open('x',encoding='utf-8').write(s)
print('Final2 compiled/index/publication readers prepared; old attempts retained',flush=True)
