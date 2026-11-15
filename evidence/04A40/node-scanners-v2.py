"""Fresh impacted host-free Node suites; shared scanners run serially after Python."""
from common40_v3 import *
import re
lock();runs=[]
for suffix,cwd,args in [('office-check','BESHStat.OfficeAddin',['node','--check','wwwroot/js/officeHost.mjs']),('office-node','BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),('resolver','.', ['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),('wasm-check','BESHStat.WasmSmoke',['node','--check','wwwroot/js/officeBridge.js']),('wasm-node','BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]:
    result=command('final-'+suffix,args,cwd=ROOT/cwd);runs.append(result)
save(PRIVATE/'Node-raw-v1.json',runs)
counts=[]
for suffix,r in zip(['office-check','office-node','resolver','wasm-check','wasm-node'],runs):
    p=PRIVATE/'commands'/('a40-final-'+suffix)/'stdout.log';text=p.read_text(encoding='utf-8-sig')
    counts.append(dict(command=r['command'],exit_code=r['exit_code'],tests=re.findall(r'# tests (\d+)',text),passed=re.findall(r'# pass (\d+)',text),failed=re.findall(r'# fail (\d+)',text),stdout=pin(p)))
save('python/Node-fresh-v1.json',dict(runs=counts,raw=pin(PRIVATE/'Node-raw-v1.json'),Office_Mac_browser_native='NOT_RUN_UNAUTHORIZED'))
serial=runner().serial('a40-final-serial-v1');save(PRIVATE/'serial-raw-v1.json',serial)
save('python/scanners-fresh-v1.json',dict(exits=[r['exit_code'] for r in serial],raw=pin(PRIVATE/'serial-raw-v1.json'),meaning=['strict nonzero remains nonpass','profile nonzero remains nonpass','MkDocs actual exit'],disposition=['INHERITED_RESTRICTED','INHERITED_RESTRICTED','ACTUAL_EXECUTION'],no_native=True))
