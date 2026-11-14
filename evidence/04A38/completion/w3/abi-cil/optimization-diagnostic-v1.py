from common import *
from collections import Counter
d=load(HERE/'abi-cil/cross-configuration-v1.json')
for r in d['ordered_call_delegate_dependency_differences']:
    a=Counter(map(tuple,r['Debug']));b=Counter(map(tuple,r['Release']))
    print(json.dumps(dict(method=r['method'],removed=list((a-b).elements()),added=list((b-a).elements()))),flush=True)
diffs=[]
for r in d['signature_attribute_differences']:
    a=r['Debug_signature'];b=r['Release_signature']
    diffs.append(dict(method=r['method'],changed_keys=[k for k in a if a[k]!=b[k]],
        removed_attributes=list((Counter(a.get('attributes',[]))-Counter(b.get('attributes',[]))).elements()),
        added_attributes=list((Counter(b.get('attributes',[]))-Counter(a.get('attributes',[]))).elements()),
        member_flags_changed=r['Debug_attributes']!=r['Release_attributes']))
save('abi-cil/signature-attribute-differences-v1.json',dict(rows=diffs))
print('SIGNATURE_KEY_PATTERNS',dict(Counter(tuple(r['changed_keys']) for r in diffs)),flush=True)
print('ATTRIBUTE_REMOVALS',dict(Counter(x for r in diffs for x in r['removed_attributes'])),flush=True)
