"""Execute the unchanged final v15 checkpoint using only exact authenticated v16 preimages.

This read-only projection is bounded to recorded existing-source transitions.
The actual v16 successor and both snapshots are checked before projection. No
assertion in any historical reader is edited and no archived engine is invoked.
"""
import sys,json,hashlib,contextlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
W2=HERE.parent;PREVIOUS=W2/'continuation-v15'
def digest(data):return hashlib.sha256(data).hexdigest()
read_bytes=Path.read_bytes;read_text=Path.read_text
transitions=json.loads(read_bytes(HERE/'source-transitions-freeze-v3.json'))['files']
projected={};bindings=[]
def check(record):
 path=ROOT/record['path'];data=read_bytes(path)
 assert (len(data),digest(data))==(record['bytes'],record['sha256']),'Projection source pin failed '+record['path']
 return data
for row in transitions:
 after=check(row['after']);assert check(row['snapshot'])==after
 if row['before']:
  before=check(row['preimage'])
  assert (len(before),digest(before))==(row['before']['bytes'],row['before']['sha256'])
  projected[(ROOT/row['path']).resolve()]=before
  bindings.append(dict(path=row['path'],before=row['before'],preimage=row['preimage'],after=row['after']))
assert len(projected)==7,'Unexpected historical projection domain'
def projected_bytes(path):
 key=path.resolve()
 return projected[key] if key in projected else read_bytes(path)
def projected_text(path,encoding=None,errors=None,newline=None):
 key=path.resolve()
 if key not in projected:return read_text(path,encoding=encoding,errors=errors,newline=newline)
 value=projected[key].decode(encoding or 'utf-8',errors or 'strict')
 if newline is None:value=value.replace('\r\n','\n').replace('\r','\n')
 return value
sys.path[:0]=[str(PREVIOUS),str(W2/'continuation-v14')]
Path.read_bytes=projected_bytes;Path.read_text=projected_text
try:
 from verify_c15_checkpoint_v1 import compute
 result=compute(ROOT)
finally:Path.read_bytes=read_bytes;Path.read_text=read_text
for row in transitions:check(row['after']);check(row['snapshot'])
print(json.dumps(dict(status='UNCHANGED_FINAL_C15_READER_AND_PREDECESSOR_CHAIN_PASS_WITH_EXACT_AUTHORIZED_PREIMAGES',prior=result,projection=bindings,only_v16_source_successors_projected=True,historical_assertions_changed=False,original_queries=0)),flush=True)
