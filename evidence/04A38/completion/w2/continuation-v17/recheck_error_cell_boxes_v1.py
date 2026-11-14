"""The smallest two-test Debug recheck after the boxed-value correction."""
from common import *
import shutil
assert json.loads((HERE/'build-combined-v7-Debug.json').read_bytes())['all_exit_zero']
label='w2c17-error-cell-correction-v1';run=RUNROOT/'builds'/label/'Debug';run.mkdir(parents=True,exist_ok=False)
lib=PRIVATE/'combined-v7/Debug/lib'
for role in ['portable','legacy']:
 shutil.copytree(RUNROOT/'builds/w2c17-error-cell-counterexample-v1/Debug'/role,run/role)
 for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(lib/name,run/role/name)
runner=m.Runner(RUNROOT);commands=[];results=[]
for suite,project,role in [('Common','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','portable'),('Legacy','BESHStatNG.Test/BESHStatNG.Tests.vbproj','legacy')]:
 args=(['dotnet','msbuild'] if suite=='Common' else [runner.msbuild])+[project,'/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(run/role)+'\\','/p:IntermediateOutputPath='+str(run/'obj'/suite)+'\\','/m:4','/v:minimal','/nr:false']
 r=command('error-cell-correction-build-'+suite,args);commands.append(clean(r));assert r['exit_code']==0
 for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll']:assert library_pin(run/role/name)==library_pin(lib/name)
 r=runner.test('Debug',label,suite,label+'-'+suite,'FullyQualifiedName~Median17ErrorCells');results.append(clean(r))
for row in json.loads((HERE/'production-test-freeze-v7.json').read_bytes())['sources']:assert pin(ROOT/row['path'])==row
save('error-cell-focused-final-recheck-v1.json',dict(source_freeze=pin(HERE/'production-test-freeze-v7.json'),commands=commands,results=results,scope='TWO_NEW_ERROR_BOX_ALIAS_IDENTITIES_ONLY;NO_SECOND_FULL_MANAGED_OR_PYTHON_CAMPAIGN',counterexample=pin(HERE/'error-cell-current-only-counterexample-v1.json'),original_queries=0,Release=DEFERRED))
assert all(r['outcomes']=={'Passed':1} for r in results)
print('Both conditional error-box identities PASS through current Common and actual Windows adapter')
