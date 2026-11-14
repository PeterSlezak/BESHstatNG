"""One exact historical Python campaign; no assertion/kernel/candidate changes."""
from common import *
rows=[]
for name in ['parallel_scan_v1.py','python_workers_v1.py','freeze_python_inputs_v1.py','run_python_v1.py','prove_scanner_deferral_v1.py','prove_parallel_scan_base_v1.py']:
    source=PREVIOUS/name;value=source.read_text(encoding='utf-8')
    if name=='python_workers_v1.py':value=value.replace('w2c12','w2c13').replace('C12 worker','C13 worker')
    if name=='prove_parallel_scan_base_v1.py':value=value.replace('New C12 stale-cache','New C13 stale-cache')
    target=HERE/name
    with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
    rows.append(dict(before=pin(source),after=pin(target),scope='Current continuation command/private labels and synthetic-control provenance prose only. Same original scans, assertions, source audit, exact956 discovery and isolated partitions.'))
save('python-executor-crosswalk-v1.json',dict(files=rows,original_queries=0))
for label,args in [('scanner-base-proof-v1',[str(HERE/'prove_parallel_scan_base_v1.py')]),('scanner-deferral-proof-v1',[str(HERE/'prove_scanner_deferral_v1.py')]),('python-inventory-v1',[str(HERE/'python_workers_v1.py'),'inventory'])]:
    result=command(label,[sys.executable,'-B',*args],env={'PYTHONHASHSEED':'38'})
    save(label+'-execution.json',clean(result));assert result['exit_code']==0
(PRIVATE/'bookkeeping-code-v1').mkdir(exist_ok=False)
print('C13 exact discovery, scanner equality/adverse controls, captured synthetic preimage and isolated workers ready')
