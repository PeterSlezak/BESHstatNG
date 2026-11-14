from common import *
for c in ['Debug','Release']:
    a=json.loads((PREVIOUS/'metadata-v6/parametric-v4'/('current-'+c+'.json')).read_bytes())
    b=json.loads((HERE/'inspection/final-v4'/('MethodMetadata38-'+c+'.json')).read_bytes())
    old={t['type']:t for t in a['types']};new={t['type']:t for t in b['types']}
    print(c,'removed types',sorted(old.keys()-new.keys()),'added types',sorted(new.keys()-old.keys()))
    print(c,'changed types',[k for k in old.keys()&new.keys() if old[k]!=new[k]])
    from verify_slice_v2 import semantic
    om={m['method']:m for m in a['methods']};nm={m['method']:m for m in b['methods']}
    print(c,'removed methods',[k for k in om if k not in nm])
    print(c,'changed methods',[(om[k]['declaring_type'],om[k]['member_name']) for k in om.keys()&nm.keys() if semantic(om[k])!=semantic(nm[k])])
