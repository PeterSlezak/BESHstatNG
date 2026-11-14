"""Record one unrelated static-cache enumeration-order measurement, never an alias."""
from common import *
owner='BESHStatNG.psmData+_Closure$__';rows=[]
for config in ['Debug','Release']:
    before=json.loads((PREVIOUS/('metadata-all/packet-v1-context-v3/current-'+config+'.json')).read_bytes());after=json.loads((HERE/('metadata-all/packet-v1-context-v3/current-'+config+'.json')).read_bytes())
    b=next(t for t in before['types'] if t['type']==owner);a=next(t for t in after['types'] if t['type']==owner)
    normalized=lambda t:dict(t,fields=sorted(t['fields'],key=lambda f:f['name']))
    assert normalized(b)==normalized(a) and {f['name'] for f in b['fields']}=={'$I','$I84-0'} and all(f['is_static'] for f in b['fields']) and 'AutoLayout' in b['attributes']
    roots=json.loads((HERE/'transitive-compiled-context-v1.json').read_bytes())['configurations'][config]['methods']
    assert not any(key.startswith(owner+' @ ') for key in roots)
    rows.append(dict(configuration=config,before=b,after=a,changed_order=b!=a,source=pin(ROOT/'BESHstatNG/src/DM/PSMData.vb') if (ROOT/'BESHstatNG/src/DM/PSMData.vb').is_file() else None,root_reachable=False))
save('unrelated-static-field-order-measurement-v1.json',dict(rows=rows,qualification='Only reflection enumeration order of two existing named static delegate-cache fields in an unrelated AutoLayout compiler type differs. Names/types/flags/constant state, complete ctor/cctor and every existing method CIL remain exact. This type is outside all claimed packet paths; no generic alias or mutable container closure. All old raw measurements remain immutable.',original_queries=0))
source=(HERE/'verify_c8_checkpoint_template_v2.py').read_text(encoding='utf-8')
old="if t['type'] is not None:need(types.get(t['type'])==t,'Changed retained C7 field/closure/type context')"
new="""if t['type'] is not None:
                measured=types.get(t['type'])
                if t['type']=='BESHStatNG.psmData+_Closure$__':
                    need(measured is not None and {f['name'] for f in t['fields']}=={'$I','$I84-0'} and all(f['is_static'] for f in t['fields']) and 'AutoLayout' in t['attributes'],'Unexpected unrelated cache context')
                    need(dict(t,fields=sorted(t['fields'],key=lambda f:f['name']))==dict(measured,fields=sorted(measured['fields'],key=lambda f:f['name'])),'Changed named unrelated static-cache field semantics')
                else:need(measured==t,'Changed retained C7 field/closure/type context')"""
assert old in source;source=source.replace(old,new)
needle="reached=closure(now,root_keys)['methods']"
source=source.replace(needle,needle+"\n        need(not any(key.startswith('BESHStatNG.psmData+_Closure$__ @ ') for key in reached),'Unrelated field-order qualification entered claimed path')")
with (HERE/'verify_c8_checkpoint_template_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('field-order-reader-crosswalk-v1.json',dict(before=pin(HERE/'verify_c8_checkpoint_template_v2.py'),after=pin(HERE/'verify_c8_checkpoint_template_v3.py'),measurement=pin(HERE/'unrelated-static-field-order-measurement-v1.json'),original_queries=0))
