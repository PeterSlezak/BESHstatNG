"""Launch one named, versioned W3 packet and preserve its raw exit/streams."""
import json
from pathlib import Path
import subprocess
import sys
import time

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[3]
name,script,*args=sys.argv[1:]
assert name.replace('-','').isalnum()
target=(HERE/script).resolve()
assert target.is_relative_to(HERE) and target.is_file()
out=HERE/'execution'/name
out.mkdir(parents=True,exist_ok=False)
cmd=[sys.executable,'-I','-B','-c',
     'import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")',
     str(HERE),str(target),*args]
start=time.time()
with (out/'stdout.log').open('xb') as stdout,(out/'stderr.log').open('xb') as stderr:
    result=subprocess.run(cmd,cwd=ROOT,stdout=stdout,stderr=stderr)
with (out/'receipt.json').open('x',encoding='utf-8') as f:
    json.dump(dict(command=cmd,cwd=str(ROOT),start_utc=time.strftime('%Y-%m-%dT%H:%M:%SZ',time.gmtime(start)),
                   end_utc=time.strftime('%Y-%m-%dT%H:%M:%SZ',time.gmtime()),seconds=time.time()-start,exit_code=result.returncode),f,indent=2)
print(name,'exit',result.returncode,flush=True)
if result.returncode:
    print((out/'stderr.log').read_text(encoding='utf-8',errors='replace')[-6000:])
sys.exit(result.returncode)
