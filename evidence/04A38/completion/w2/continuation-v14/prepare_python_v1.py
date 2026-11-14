from common import *
rows=[]
for name in ['parallel_scan_v1.py','python_workers_v1.py','freeze_python_inputs_v1.py','run_python_v1.py','prove_scanner_deferral_v1.py','prove_parallel_scan_base_v1.py']:
 s=PREVIOUS/name;v=s.read_text(encoding='utf-8')
 if name=='python_workers_v1.py':v=v.replace('w2c13','w2c14').replace('C13 worker','C14 worker')
 if name=='prove_parallel_scan_base_v1.py':v=v.replace('New C13 stale-cache','New C14 stale-cache')
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(v)
 rows.append(dict(before=pin(s),after=pin(HERE/name)))
save('python-executor-crosswalk-v1.json',dict(files=rows,original_queries=0,scope='Exact inherited original scanner/AST/assertion/parent partition logic, current labels and bounded synthetic-control roots only.'))
for label,args in [('scanner-base-proof-v1',[str(HERE/'prove_parallel_scan_base_v1.py')]),('scanner-deferral-proof-v1',[str(HERE/'prove_scanner_deferral_v1.py')]),('python-inventory-v1',[str(HERE/'python_workers_v1.py'),'inventory'])]:
 r=command(label,[sys.executable,'-B',*args],env={'PYTHONHASHSEED':'38'});save(label+'-execution.json',clean(r));assert r['exit_code']==0
(PRIVATE/'bookkeeping-code-v1').mkdir(exist_ok=False)
print('Exact956 parent inventory, scanner equality/adverse controls, deferral errors and isolated workers ready')
