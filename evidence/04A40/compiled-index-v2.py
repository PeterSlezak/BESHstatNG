"""Reviewable instruction/type multisets, exact outside authorized method deltas."""
from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'evidence/04A40'))
from common40_v3 import *
import re,collections
lock()
def canonical(v):
    if isinstance(v,dict):
        out={k:canonical(x) for k,x in v.items() if k!='raw_IL_sha256'}
        if 'type' in v and 'fields' in v and '+_Closure$__' in v['type'] and all(f['is_static'] for f in v['fields']):
            out['fields']=sorted(out['fields'],key=lambda x:json.dumps(x,sort_keys=True))
        return out
    if isinstance(v,list):return [canonical(x) for x in v]
    if isinstance(v,str):
        for pattern,replacement in [(r'_Closure\$__(\d+)-(\d+)',r'_Closure$__ORD-\2'),(r'_Lambda\$__(\d+)-(\d+)',r'_Lambda$__ORD-\2'),(r'\$I(\d+)-(\d+)',r'$IORD-\2')]:v=re.sub(pattern,replacement,v)
    return v
def hashed(v):return sha(json.dumps(canonical(v),sort_keys=True,ensure_ascii=False).encode())
reports=[]
for config in ['Debug','Release']:
    old=load(ROOT/('evidence/04A39/abi-cil/'+config+'/current-v1.json'));now=load(PRIVATE/('abi-cil-final2/'+config+'/current.json'))
    t_before=collections.Counter(hashed(r) for r in old['types']);t_after=collections.Counter(hashed(r) for r in now['types']);assert t_before==t_after,'TYPE_FIELD_OR_INTERFACE_CHANGED'
    allrows=[]
    for label,doc in [('accepted39',old),('current40',now)]:
        index=[dict(method=canonical(r['method']),signature=r['signature'],record_sha256=hashed(r)) for r in doc['methods']]
        save('abi-cil/'+config+'-'+label+'-method-index-v2.json',dict(methods=index,normalization='Only raw IL hash and generated leading closure/lambda/cache ordinals; full instruction/call/exception/locals data bound by canonical record hash.'))
        allrows.append(dict(label=label,methods=len(index),types=len(doc['types']),full_metadata=pin(ROOT/('evidence/04A39/abi-cil/'+config+'/current-v1.json')) if label=='accepted39' else pin(PRIVATE/('abi-cil-final2/'+config+'/current.json'))))
    changes=load(PRIVATE/('abi-final2-'+config+'-canonical-changes-v3.json'));save('abi-cil/'+config+'-changed-method-records-v2.json',changes)
    common=[r for r in now['methods'] if ' @ BESHStat.Excel.Common,' in r['method']]
    forbidden=('ExcelDna.Integration','Microsoft.Office.Interop','System.Windows.Forms')
    offenders=[r['method'] for r in common if any(v in json.dumps(r) for v in forbidden)]
    assert not offenders,'HOST_DEPENDENCY_IN_COMMON'
    reports.append(dict(configuration=config,indices=allrows,type_field_base_interface_multiset_exact=True,private_static_generic_cache_field_enumeration_order='Three unchanged Core compiler closure types enumerated static fields in a different reflection order. Canonical per-type field multiset preserves every name/type/visibility/value; no public/instance layout order normalized.',Common_methods=len(common),forbidden_host_dependency_calls=offenders,shipping_proof=pin(HERE/('abi-cil/'+config+'-proof-v2.json'))))
save('abi-cil/complete-index-readback-v2.json',dict(configurations=reports,Core_source_unchanged=True,cross_config='Distinct optimization-specific instruction streams inspected against same-config39; both48/77 fixtures and29424 corpora compared separately. Raw Debug/Release IL is not required to hash equally.'))
print('Both configuration complete type/field/interface/metadata closure indices verified',flush=True)
