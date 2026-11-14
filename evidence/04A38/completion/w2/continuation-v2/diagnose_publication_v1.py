from common import *
import re
p=HERE/'full-managed-Debug-v1.json';obj=json.loads(p.read_bytes());rows=[]
def visit(x,path):
 if isinstance(x,dict):
  for k,v in x.items():visit(v,path+[k])
 elif isinstance(x,list):
  for i,v in enumerate(x):visit(v,path+[i])
 elif isinstance(x,str):
  matches=list(re.finditer(r'https?://[^\s/"<>:@]+:[^\s/"<>@]+@',x))
  if matches:rows.append(dict(pointer=path,matches=len(matches),value_sha256=sha(x.encode()),matched_bytes_sha256=[sha(m[0].encode()) for m in matches],length=len(x),contains_assertion='Assert' in x or 'Traceback' in x))
visit(obj,[])
save('attempts/publication-privacy-v1.json',dict(status='REJECTED_BEFORE_ZIP',manifest=pin(HERE/'publication-manifest-v1.json'),payload=pin(p),diagnostic_fields=rows,policy='No URL whitelist; retain original diagnostic receipt locally; public derivative must be explicit and independently related. No statistical/source/reference byte change.'))
print(json.dumps(rows))
