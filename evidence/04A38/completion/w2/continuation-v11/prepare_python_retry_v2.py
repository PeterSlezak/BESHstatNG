from common import *
source=(HERE/'prove_parallel_scan_base_v3.py').read_text(encoding='utf-8')
source=source.replace("PRIVATE/'parallel-scan-proof-v2'", "PRIVATE/'parallel-scan-proof-v3'")
source=source.replace("HERE/'prove_parallel_scan_base_v3.py'", "HERE/'prove_parallel_scan_base_v4.py'")
with (HERE/'prove_parallel_scan_base_v4.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('scanner-proof-directory-correction-v2.json',dict(before=pin(HERE/'prove_parallel_scan_base_v3.py'),
    after=pin(HERE/'prove_parallel_scan_base_v4.py'),failure=pin(HERE/'scanner-base-proof-v2-execution.json'),
    stderr=pin(RUNROOT/'commands/w2c11-scanner-base-proof-v2/stderr.log'),
    cause='The carried v10 v2 proof already used a v2 private directory, so the intended retry replacement did not change it. Exclusive creation correctly refused reuse.',
    correction='Use a fresh v3 private proof directory. No deletion, fixture overwrite, scanner or assertion change.',original_queries=0))
for label,args in [('scanner-base-proof-v3',[str(HERE/'prove_parallel_scan_base_v4.py')]),
                   ('scanner-deferral-proof-v1',[str(HERE/'prove_scanner_deferral_v1.py')]),
                   ('python-inventory-v1',[str(HERE/'python_workers_v1.py'),'inventory'])]:
    result=command(label,[sys.executable,'-B',*args],env={'PYTHONHASHSEED':'38'})
    save(label+'-execution.json',clean(result));assert result['exit_code']==0
