from common43 import *
for p in ['scope/cache-export-matrix-v1.json','scope/portable-ownership-lock-v1.json','register/issue-delta-v1.json','fixtures/Debug-43-v1.json','fixtures/Debug-43-races-v1.json','fixtures/Debug-42-v1.json','policy/Debug-simulation-v1.json','preservation/readback-v1.json','planning/reconciliation-v1.json','matrix/source-freeze-v1.json','fixtures/relational-review-v1.json']:
 d=load(HERE/p);print(p,list(d))
 for k,v in d.items():
  if isinstance(v,list) and v:print(k,'length',len(v),'firstkeys',list(v[0]) if isinstance(v[0],dict) else type(v[0]).__name__)
  elif isinstance(v,dict):print(k,'keys',list(v))
