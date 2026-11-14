"""Only the current Debug production binaries, offline, without restore/packaging."""
from common import *
import shutil
stage=sys.argv[1];config='Debug'
freeze=json.loads((HERE/'production-test-freeze-v7.json').read_bytes())
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
run=PRIVATE/stage/config;out=run/'lib';out.mkdir(parents=True,exist_ok=False)
runner=m.Runner(RUNROOT);commands=[]
for role,project in [('core','BESHStat.Core/BESHStat.Core.vbproj'),('common','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj')]:
 r=command(stage+'-Debug-'+role,['dotnet','msbuild',str(ROOT/project),'/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(out)+'\\','/p:IntermediateOutputPath='+str(run/'obj'/role)+'\\','/m:4','/v:minimal','/nr:false']);commands.append(clean(r))
 if r['exit_code']!=0:break
else:
 r=command(stage+'-Debug-host',[runner.msbuild,str(ROOT/'BESHstatNG/BESHstatNG.vbproj'),'/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(out)+'\\','/p:IntermediateOutputPath='+str(run/'obj/host')+'\\','/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']);commands.append(clean(r))
 shutil.copy2(ROOT/'packages/ExcelDna.Integration.1.9.0/lib/net462/ExcelDna.Integration.dll',out/'ExcelDna.Integration.dll')
result=dict(stage=stage,configuration='Debug',commands=commands,all_exit_zero=len(commands)==3 and all(r['exit_code']==0 for r in commands),assemblies=[library_pin(p) for p in sorted(out.glob('*.dll'))],source_freeze=pin(HERE/'production-test-freeze-v7.json'),Release_compilation=DEFERRED,Release_unit_suites=DEFERRED,Release_ABI_CIL_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,restore='NOT_RUN',packaging='NOT_RUN',original_queries=0)
save('build-'+stage+'-Debug.json',result)
assert result['all_exit_zero'],'Preserve failed build; correct and version the source freeze before retry'
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
print('Combined Debug production build PASS',stage,flush=True)
