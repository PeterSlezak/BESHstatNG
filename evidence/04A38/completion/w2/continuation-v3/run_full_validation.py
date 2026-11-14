"""Final-current isolated offline verification; preserves every nonzero suite exit."""
from common import *
mode=sys.argv[1];label='w2c3-validation-v1';runner=m.Runner(RUNROOT)
if mode=='managed':
 b=runner.build('Debug',label);assert not any(b['exit_codes'])
 before=m.source_pins();results=[]
 for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
  results.append(runner.test('Debug',label,suite,label+'-Debug-serial-'+suite))
 save('full-managed-Debug-v1.json',dict(build=clean(b),results=clean(results),source_unchanged=before==m.source_pins()))
elif mode=='source':
 results=[]
 for suffix,cwd,args in [('office-node','BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),('resolver','.', ['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),('wasm-node','BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]:results.append(runner.command(label+'-'+suffix,args,cwd=ROOT/cwd))
 for suffix,path in [('tools','BESHstatNG/tools/tests'),('help','BESHStatNG_Help_MkDocs/tests'),('scripts','scripts/tests')]:results.append(runner.command(label+'-python-'+suffix,[sys.executable,'-B','-m','unittest','discover','-s',path,'-p','test_*.py','-v']))
 save('source-validation-v1.json',dict(results=clean(results)))
elif mode=='common-python':
 probe=label+'-semantic';r=runner.test('Debug',label,'Common',probe,'FullyQualifiedName~MigrationSemantic04A02Tests')
 result=runner.command(label+'-common-python',[sys.executable,'-B','-m','unittest','discover','-s','BESHStat.Excel.Common.Tests/src','-p','test_*.py','-v'],env={'BESHSTAT_04A02_G3_CANDIDATES':str(RUNROOT/'executions'/probe/'observations')})
 save('common-python-validation-v1.json',dict(semantic=clean(r),python=clean(result)))
elif mode=='serial':save('serial-validation-v1.json',clean(runner.serial(label)))
else:raise ValueError(mode)
