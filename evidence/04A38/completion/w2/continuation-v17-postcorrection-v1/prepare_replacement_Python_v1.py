from common import *
assert json.loads((HERE/'managed-full-execution-v1.json').read_bytes())['campaigns']==1
# Once each: strict/profile/MkDocs, using the newly compiled Debug audit.
save('serial-execution-v1.json',clean(m.Runner(RUNROOT).serial('w2c17pc1-replacement-v1')))
for label,args in [('python-discovery-inventory-v1',bootstrap(HERE/'python_workers_v2.py','inventory')),('scanner-retained-metadata-preflight-v1',bootstrap(HERE/'scanner_retained_preflight_v2.py')),('scanner-cache-adverse-controls-v1',bootstrap(HERE/'scanner_cache_controls_v2.py'))]:
 r=command(label,args);assert r['exit_code']==0,label
print('Python inventory and adverse controls complete; no parents executed',flush=True)
