"""Independently enumerate every raw/public diagnostic difference locally."""
import hashlib,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
def inspect():
 d=json.loads((HERE/'diagnostic-public-derivative.json').read_bytes());raw=(ROOT/d['raw_local']['path']).read_bytes();public=(ROOT/d['public']['path']).read_bytes()
 for b,r in [(raw,d['raw_local']),(public,d['public'])]:assert (len(b),hashlib.sha256(b).hexdigest())==(r['bytes'],r['sha256'])
 changes=[]
 def walk(a,b,path):
  assert type(a) is type(b),'Diagnostic JSON type changed'
  if isinstance(a,dict):
   assert a.keys()==b.keys()
   for k in a:walk(a[k],b[k],path+[k])
  elif isinstance(a,list):
   assert len(a)==len(b)
   for i,(x,y) in enumerate(zip(a,b)):walk(x,y,path+[i])
  elif a!=b:
   assert path[-1]=='test_name' and path[:1]==['results'] and len(path)==5,'Non-display diagnostic value changed'
   changes.append(dict(pointer=path,original_label_sha256=hashlib.sha256(a.encode()).hexdigest(),public_label_sha256=hashlib.sha256(b.encode()).hexdigest(),public_label=b))
 walk(json.loads(raw),json.loads(public),[]);assert changes==d['changes']
 return dict(status='EXHAUSTIVE_RAW_PUBLIC_JSON_DIFF_PASS',changed_fields=changes,all_other_fields_equal=True,raw_identity=d['raw_local'],public_identity=d['public'],scope='LOCAL raw diagnostic comparison; derivative alone cannot rehash omitted raw bytes',scientific_source_input_output_trace_redactions=0)
if __name__=='__main__':print(json.dumps(inspect()))
