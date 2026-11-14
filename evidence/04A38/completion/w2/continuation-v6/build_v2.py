"""Current-only isolated production/test build; no restore or XLL packaging."""
import shutil
from common import *
def main():
 stage,config=sys.argv[1:3];assert config in ['Debug','Release'];run=PRIVATE/stage/config;out=run/'lib';out.mkdir(parents=True,exist_ok=False)
 runner=m.Runner(RUNROOT);commands=[]
 for role,project in [('core','BESHStat.Core/BESHStat.Core.vbproj'),('common','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj')]:
  r=command(stage+'-'+config+'-'+role,['dotnet','msbuild',str(ROOT/project),'/t:Build','/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(out)+'\\','/p:IntermediateOutputPath='+str(run/'obj'/role)+'\\','/m:4','/v:minimal','/nr:false']);commands.append(clean(r))
  if r['exit_code']!=0:break
 else:
  r=command(stage+'-'+config+'-host',[runner.msbuild,str(ROOT/'BESHstatNG/BESHstatNG.vbproj'),'/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory','/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(out)+'\\','/p:IntermediateOutputPath='+str(run/'obj/host')+'\\','/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']);commands.append(clean(r))
  shutil.copy2(ROOT/'packages/ExcelDna.Integration.1.9.0/lib/net462/ExcelDna.Integration.dll',out/'ExcelDna.Integration.dll')
 result=dict(stage=stage,configuration=config,commands=commands,all_exit_zero=len(commands)==3 and all(r['exit_code']==0 for r in commands),assemblies=[library_pin(p) for p in sorted(out.glob('*.dll'))],sources=[pin(ROOT/p['live']['path']) for p in json.loads((HERE/'implementation-plan.json').read_bytes())['source_before']],original_queries=0,restore='NOT_RUN',packaging='NOT_RUN',native='NOT_RUN')
 save('build-'+stage+'-'+config+'.json',result);assert result['all_exit_zero'],'Preserve this build attempt; correct with a new stage label'
 print('Built current',stage,config)
if __name__=='__main__':main()
