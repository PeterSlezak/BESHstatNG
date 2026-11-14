from common import *
for label,filename,args in [('scanner-base-proof-v2','prove_parallel_scan_base_v2.py',[]),('scanner-deferral-proof-v1','prove_scanner_deferral_v1.py',[]),('python-inventory-v1','python_workers_v1.py',['inventory'])]:
 r=command(label,[sys.executable,'-B',str(HERE/filename),*args],env={'PYTHONHASHSEED':'38'});save(label+'-execution.json',clean(r));assert r['exit_code']==0
(PRIVATE/'bookkeeping-code-v1').mkdir(exist_ok=False)
