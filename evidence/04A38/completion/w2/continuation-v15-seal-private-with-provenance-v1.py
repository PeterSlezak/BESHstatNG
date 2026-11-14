"""Create the bounded private seal; its own raw streams use an explicit new root."""
import sys
from pathlib import Path
W2=Path(__file__).resolve().parent;HERE=W2/'continuation-v15'
sys.path.insert(0,str(HERE))
from common import *
folder=RUNROOT/'private-seal-w2c15-v1';folder.mkdir(exist_ok=False)
bootstrap='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
args=[sys.executable,'-I','-B','-c',bootstrap,str(HERE),str(W2/'continuation-v15-retained-private-reader-v3.py')]
start=time.monotonic()
with (folder/'stdout.log').open('xb') as out,(folder/'stderr.log').open('xb') as err:
 result=subprocess.run(args,cwd=ROOT,stdout=out,stderr=err)
raw=dict(command=args,cwd=str(ROOT),exit_code=result.returncode,seconds=time.monotonic()-start)
with (folder/'command.json').open('x',encoding='utf-8') as f:json.dump(raw,f,indent=2)
pins=[pin(folder/n) for n in ['command.json','stdout.log','stderr.log']]
receipt=W2/'continuation-v15-private-seal-generation-v1.json'
doc=dict(status='ACTUAL_PRIVATE_SEAL_GENERATION_PASS' if result.returncode==0 else 'STOP_PRIVATE_SEAL_GENERATION_FAILED',reader=pin(W2/'continuation-v15-retained-private-reader-v3.py'),command=clean(raw),raw_private_pins=pins,original_queries=0)
if result.returncode==0:doc['private_seal']=pin(W2/'continuation-v15-retained-private-preservation-v2.json')
with receipt.open('x',encoding='utf-8',newline='\n') as f:json.dump(doc,f,indent=2);f.write('\n')
print(json.dumps(dict(receipt=pin(receipt),status=doc['status'],exit_code=result.returncode,seconds=raw['seconds'])),flush=True)
assert result.returncode==0,'Private preservation gate failed; preserve and STOP'
