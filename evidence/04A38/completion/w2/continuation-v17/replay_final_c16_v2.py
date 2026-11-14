"""Run unchanged final v16 readers against exact authenticated prior source bytes.

Only the thirteen existing source/test successors are projected. No archived
source is compiled or executed. Every live successor and preserved preimage is
checked before/after. All historical assertions and reference files stay intact.
"""
import sys,os,json,hashlib,io,subprocess,runpy
from pathlib import Path
ROOT=Path.cwd().resolve();HERE=ROOT/'evidence/04A38/completion/w2/continuation-v17';OLD=HERE.parent/'continuation-v16'
sys.dont_write_bytecode=True
rb=Path.read_bytes;rt=Path.read_text;op=Path.open;st=Path.stat;original_run=subprocess.run
def digest(data):return hashlib.sha256(data).hexdigest()
def key(path):return os.path.normcase(os.path.abspath(path))
def checked(row):
 data=rb(ROOT/row['path']);assert (len(data),digest(data))==(row['bytes'],row['sha256']),row['path'];return data
transitions=json.loads(rb(HERE/'source-transitions-final-v2.json'))['files'];projected={};bindings=[]
for row in transitions:
 after=checked(row['after']);assert checked(row['snapshot'])==after
 if row['changed'] and row['before']:
  before=checked(row['preimage']);assert (len(before),digest(before))==(row['before']['bytes'],row['before']['sha256'])
  projected[key(ROOT/row['path'])]=before;bindings.append(row)
assert len(projected)==13,'Unexpected bounded prior-source projection domain'
def readbytes(path):return projected[key(path)] if key(path) in projected else rb(path)
def readtext(path,encoding=None,errors=None,newline=None):
 if key(path) not in projected:return rt(path,encoding=encoding,errors=errors,newline=newline)
 text=projected[key(path)].decode(encoding or 'utf-8',errors or 'strict')
 return text.replace('\r\n','\n').replace('\r','\n') if newline is None else text
def opened(path,mode='r',buffering=-1,encoding=None,errors=None,newline=None):
 if key(path) not in projected:return op(path,mode,buffering,encoding,errors,newline)
 assert not any(c in mode for c in 'wax+'),'Historical reader attempted a source mutation'
 stream=io.BytesIO(projected[key(path)])
 return stream if 'b' in mode else io.TextIOWrapper(stream,encoding=encoding or 'utf-8',errors=errors or 'strict',newline=newline)
def stated(path,*args,**kwargs):
 result=st(path,*args,**kwargs)
 if key(path) not in projected:return result
 parts=list(result);parts[6]=len(projected[key(path)]);return os.stat_result(parts)
def ran(args,*rest,**kwargs):
 expected=[sys.executable,'-I','-B',str(OLD/'replay_prior_v15_v1.py')]
 if list(args)==expected:
  args=[sys.executable,'-I','-B',str(Path(__file__).resolve()),'--v15']
 return original_run(args,*rest,**kwargs)
Path.read_bytes=readbytes;Path.read_text=readtext;Path.open=opened;Path.stat=stated;subprocess.run=ran
sys.path[:0]=[str(OLD),str(OLD.parent/'continuation-v15'),str(OLD.parent/'continuation-v14')]
try:
 if '--v15' in sys.argv:runpy.run_path(str(OLD/'replay_prior_v15_v1.py'),run_name='__main__')
 else:
  from verify_c16_checkpoint_v3 import compute
  result=compute(ROOT,predecessor=True)
finally:
 Path.read_bytes=rb;Path.read_text=rt;Path.open=op;Path.stat=st;subprocess.run=original_run
for row in transitions:checked(row['after']);checked(row['snapshot'])
if '--v15' not in sys.argv:print(json.dumps(dict(status='UNCHANGED_FINAL_V16_AND_PREDECESSORS_PASS_WITH_EXACT_AUTHENTICATED_SOURCE_VERSIONS',result=result,projected_existing_sources=len(bindings),historical_assertions_changed=False,source_versions=[dict(path=r['path'],before=r['before'],preimage=r['preimage'],after=r['after']) for r in bindings],original_queries=0)),flush=True)
