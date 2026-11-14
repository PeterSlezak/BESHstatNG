from common import *
for name in sys.argv[1:]:
 d=json.loads((HERE/name).read_bytes());print(name,list(d) if isinstance(d,dict) else ('list',len(d)))
 for k,v in d.items() if isinstance(d,dict) else []:
  if isinstance(v,list):print(k,len(v),list(v[0]) if v and isinstance(v[0],dict) else '')
