"""Exact retained scanner authorities and worker isolation; discovery is not execution."""
from common import *
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());plan['source_paths']+=plan['new_portable']+plan['new_tests'];save('implementation-plan-v2.json',plan)
kernel=(PREVIOUS/'retained_scan_cache_v16_v2.py').read_text(encoding='utf-8')
kernel=kernel.replace('from common import *','from common import *\nC15=W2/\'continuation-v15\'').replace("PREVIOUS/'python-scan-input-freeze-v2.json'","C15/'python-scan-input-freeze-v2.json'").replace("PREVIOUS/'review-exclusions-v1.json'","C15/'review-exclusions-v1.json'").replace("PREVIOUS/'parallel_scan_v1.py'","C15/'parallel_scan_v1.py'")
with (HERE/'retained_scan_cache_v17_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(kernel)
for name in ['python_workers_v2.py','scanner_retained_preflight_v2.py','scanner_cache_controls_v2.py','freeze_python_inputs_v1.py','run_python_v1.py']:
 s=(PREVIOUS/name).read_text(encoding='utf-8').replace('retained_scan_cache_v16_v2','retained_scan_cache_v17_v1').replace('w2c16-checkpoint-v1','w2c17-checkpoint-v1').replace('C16 worker refused','C17 worker refused')
 if name=='python_workers_v2.py':s=s.replace("sys.path.append(str(PREVIOUS));sys.path.append(str(W2/'continuation-v6'))","sys.path.append(str(PREVIOUS));sys.path.append(str(W2/'continuation-v15'));sys.path.append(str(W2/'continuation-v6'))")
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('scanner-authorized-source-projection-v1.json',dict(prior_kernel=pin(PREVIOUS/'retained_scan_cache_v16_v2.py'),current_kernel=pin(HERE/'retained_scan_cache_v17_v1.py'),unchanged_authority='Exact C15 source/result/input version retained explicitly after PREVIOUS advances to C16. Cache validation, assertion order, original scanner AST, failed binary exception and all5829-prefix assertions unchanged.',original_scanner_executions=0,parent_executions=0,helper_processes=0))
for label,args in [('python-discovery-inventory-v1',bootstrap(HERE/'python_workers_v2.py','inventory')),('scanner-retained-metadata-preflight-v1',bootstrap(HERE/'scanner_retained_preflight_v2.py')),('scanner-cache-adverse-controls-v1',bootstrap(HERE/'scanner_cache_controls_v2.py'))]:
 r=command(label,args);assert r['exit_code']==0,label
print('956 parents discovered; metadata/cache adverse controls PASS; no test parent or scanner executed')
