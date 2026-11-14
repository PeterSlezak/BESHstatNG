"""Offline isolated production build; no restore, XLL packaging or shared outputs."""
import sys,json,shutil
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
G=ROOT/'evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide'
sys.path.insert(0,str(G))
from common import RUNROOT,m,clean,pin,library_pin

def main():
 config,attempt=sys.argv[1:3];assert config in ['Debug','Release']
 run=RUNROOT/'completion-w2'/attempt/config;output=run/'lib';output.mkdir(parents=True,exist_ok=False)
 runner=m.Runner(RUNROOT);commands=[]
 for label,project in [('core','BESHStat.Core/BESHStat.Core.vbproj'),('common','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj')]:
  args=['dotnet','msbuild',str(ROOT/project),'/t:Build','/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false',
   '/p:OutputPath='+str(output)+'\\','/p:IntermediateOutputPath='+str(run/'obj'/label)+'\\','/m:4','/v:minimal','/nr:false']
  r=runner.command('w2-'+attempt+'-'+config+'-'+label,args);commands.append(clean(r))
  if r['exit_code']!=0:break
 else:
  args=[runner.msbuild,str(ROOT/'BESHstatNG/BESHstatNG.vbproj'),'/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory','/p:Configuration='+config,'/p:BuildProjectReferences=false',
   '/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(output)+'\\','/p:IntermediateOutputPath='+str(run/'obj/host')+'\\','/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']
  commands.append(clean(runner.command('w2-'+attempt+'-'+config+'-host',args)))
  shutil.copy2(ROOT/'packages/ExcelDna.Integration.1.9.0/lib/net462/ExcelDna.Integration.dll',output/'ExcelDna.Integration.dll')
 result=dict(configuration=config,attempt=attempt,commands=commands,all_exit_zero=len(commands)==3 and all(r['exit_code']==0 for r in commands),assemblies=[library_pin(p) for p in output.glob('*.dll')],
  source_before=pin(HERE/'source-before-inventory.json'),changed_source=[pin(ROOT/p) for p in json.loads((HERE/'profile-extraction-v1.json').read_bytes())['changed']],restore='NOT_RUN',packaging='NOT_RUN',native='NOT_RUN')
 with (HERE/('build-'+attempt+'-'+config+'.json')).open('x',encoding='utf-8') as f:json.dump(result,f,indent=2)
 assert result['all_exit_zero'],'Build failed; retain raw command receipt and repair source under next attempt'
 print('W2 current production built',config,attempt)
if __name__=='__main__':main()
