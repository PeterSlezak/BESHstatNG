"""Launch a specified original worker action with a new namespace."""
from common import *
target=HERE/'python-node-scanners/campaign-v1'
mode=sys.argv[1]
code='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
if mode=='prepare':
    specs=[('discovery',target/'python_workers_v4.py',['inventory']),
           ('cache-preflight',target/'scanner_retained_preflight_v2.py',[]),
           ('cache-controls',target/'scanner_cache_controls_v2.py',[])]
elif mode=='execute':
    specs=[('execute',target/'python_workers_v4.py',['execute'])]
else:raise ValueError(mode)
results=[]
for label,script,args in specs:
    result=command('python-'+label,[sys.executable,'-I','-B','-c',code,str(target),str(script),*args])
    results.append(dict(label=label,script=pin(script),result=result))
    save('python-node-scanners/python-'+label+'-execution-v1.json',results[-1])
    assert result['exit_code']==0,'PYTHON_PACKET_STOP '+label
print('Python packet',mode,'complete',flush=True)
