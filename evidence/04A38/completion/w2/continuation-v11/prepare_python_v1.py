"""Preserve the exact scanner/test semantics and isolate all six C11 workers."""
from common import *
rows=[]
for name in ['parallel_scan_v1.py','python_workers_v1.py','freeze_python_inputs_v1.py',
             'run_python_v1.py','prove_parallel_scan_base_v2.py','prove_scanner_deferral_v1.py']:
    source=(PREVIOUS/name).read_text(encoding='utf-8')
    if name=='python_workers_v1.py':source=source.replace('w2c10','w2c11').replace('C10 worker','C11 worker')
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
    rows.append(dict(before=pin(PREVIOUS/name),after=pin(HERE/name)))
save('python-executor-crosswalk-v1.json',dict(files=rows,
    scope='Existing deterministic956-parent discovery, six isolated lanes, exact unchanged scanner kernels/proxy/cache/adversarial controls, >256MiB deferred to unchanged sequential fallback, four disjoint relocation tests once in their proven copy. Only C11 labels and private output roots differ. No test/scanner/giant-file rewrite or omission.',original_queries=0))
for label,args in [('scanner-base-proof-v1',[str(HERE/'prove_parallel_scan_base_v2.py')]),
                   ('scanner-deferral-proof-v1',[str(HERE/'prove_scanner_deferral_v1.py')]),
                   ('python-inventory-v1',[str(HERE/'python_workers_v1.py'),'inventory'])]:
    result=command(label,[sys.executable,'-B',*args],env={'PYTHONHASHSEED':'38'})
    save(label+'-execution.json',clean(result));assert result['exit_code']==0
print('C11 exact discovery and unchanged scanner equivalence/deferral controls ready')
