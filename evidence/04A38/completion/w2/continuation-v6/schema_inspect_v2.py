from common import *
for name in ['Inspect38','MethodBodies38','Closure38']:
 d=json.loads((HERE/('inspection/families-v2/'+name+'-Debug.json')).read_bytes())
 if isinstance(d,list):print(name,'LIST',len(d),'record_keys',list(d[0]) if d else []);continue
 print(name,'KEYS',list(d))
 for k,v in d.items():
  if isinstance(v,list) and v:print('COLLECTION',k,len(v),'record_keys',list(v[0]) if isinstance(v[0],dict) else str(type(v[0])))
u=json.loads((HERE/'current-universe-families-v2.json').read_bytes())
print('POP',len(u['members']),len(u['supports']),len(u['fragments']))
print('RECORD_KEYS',list(u['members'][0]))
