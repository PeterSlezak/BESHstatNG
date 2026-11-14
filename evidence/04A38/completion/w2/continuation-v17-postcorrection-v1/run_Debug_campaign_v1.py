from common import *
assert json.loads((HERE/'entry-final-reader-authentication-v1.json').read_bytes())['all_exit_zero']
prior=json.loads((PREVIOUS/'production-test-freeze-v8.json').read_bytes())
for r in prior['sources']:assert pin(ROOT/r['path'])==r
sources={r['path']:r for r in prior['sources']}
for name,digest in m.source_pins().items():
 r=pin(ROOT/name);assert r['sha256']==digest;sources[name]=r
for n in ['current-universe.json','dependency-full-core.json','dependency-full-common.json','dependency-full-adjacent.json']:
 for r in json.loads((PREVIOUS/'final-v8'/n).read_bytes())['sources']:assert pin(ROOT/r['path'])==r;sources[r['path']]=r
save('production-test-freeze-v1.json',dict(status='FINAL_CORRECTED_V17_SOURCE_AUTHENTICATED_BEFORE_REPLACEMENT_CAMPAIGNS',previous=pin(PREVIOUS/'production-test-freeze-v8.json'),sources=[sources[k] for k in sorted(sources)],compiler_inputs=m.source_pins(),entry=pin(HERE/'entry-live-authentication-v1.json'),production_changes=0,Release=DEFERRED,original_queries=0))
for label,args in [('replacement-build',bootstrap(HERE/'run_replacement_Debug_v1.py','build')),('replacement-syntax',bootstrap(HERE/'inspect_final_Debug_v8.py','syntax')),('replacement-compiled',bootstrap(HERE/'inspect_final_Debug_v8.py','compiled','replacement-v1')),('replacement-frozen-compatibility',bootstrap(HERE/'compatibility_Debug_v3.py','replacement-v1')),('replacement-managed-suites',bootstrap(HERE/'run_replacement_Debug_v1.py','tests'))]:
 r=command(label,args)
 if r['exit_code']!=0:raise SystemExit('Stopped after genuine failed step '+label+'; retain raw evidence')
print('Authorized replacement Debug campaign complete; reconciliation pending',flush=True)
