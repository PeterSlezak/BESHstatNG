from common import *
for name in ['Inspect38','MethodBodies38','Closure38']:
 d=json.loads((HERE/('inspection/families-v2/'+name+'-Debug.json')).read_bytes())
 print(name,list(d))
 for k,v in d.items():
  if isinstance(v,list) and v:print(k,len(v),json.dumps(v[0])[:1600])
u=json.loads((HERE/'current-universe-families-v2.json').read_bytes())
print('POP',len(u['members']),len(u['supports']),len(u['fragments']))
print('RECORD_KEYS',list(u['members'][0]))
