import sys,runpy
from pathlib import Path
directory=Path.cwd()/'evidence/04A38/completion/w2/continuation-v14';sys.path.insert(0,str(directory))
from common import *
script=PRIVATE/'bookkeeping-code-v1'/sys.argv[2]
r=command(sys.argv[1],[sys.executable,'-B','-c','import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")',str(HERE),str(script),*sys.argv[3:]])
out=PRIVATE/'bookkeeping-code-v1'/(sys.argv[1]+'-execution.json')
with out.open('x',encoding='utf-8',newline='\n') as f:json.dump(dict(source=pin(script),result=clean(r)),f,indent=2);f.write('\n')
assert r['exit_code']==0,'Preserve private draft attempt '+sys.argv[1]
