"""Once-only Node and strict/profile/docs checks; no restore/install/native run."""
from common import *
runner=m.Runner(RUNROOT)
if sys.argv[1]=='node':
    results=[]
    for suffix,cwd,args in [('office-node','BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),('resolver','.', ['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),('wasm-node','BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]:
        results.append(dict(suite=suffix,command=clean(command('checkpoint-'+suffix,args,cwd=ROOT/cwd))))
    save('node-execution-v1.json',dict(results=results))
elif sys.argv[1]=='serial':save('serial-execution-v1.json',clean(runner.serial('w2c13-checkpoint-v1')))
else:raise ValueError(sys.argv[1])
