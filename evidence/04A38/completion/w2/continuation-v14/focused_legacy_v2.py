"""Current portable/actual shipping preparation contract tests."""
import shutil
from common import *
def main():
 stage,config=sys.argv[1:3];assert config=='Debug','Release unit tests NOT_RUN_OWNER_PREFERENCE';run=PRIVATE/(stage+'-focus-legacy-v2')/config;run.mkdir(parents=True,exist_ok=False);prod=PRIVATE/stage/config/'lib';runner=m.Runner(RUNROOT);results=[]
 for suite,project,framework,filter in [('Legacy','BESHStatNG.Test/BESHStatNG.Tests.vbproj',True,'FullyQualifiedName~Continuation14')]:
  folder=run/('tests-'+suite);folder.mkdir()
  for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(prod/n,folder/n)
  args=([runner.msbuild] if framework else ['dotnet','msbuild'])+[str(ROOT/project),'/t:Build','/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(folder)+'\\','/p:IntermediateOutputPath='+str(run/'obj'/('tests-'+suite))+'\\','/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']
  built=command(stage+'-focus-legacy-v2-'+config+'-build-'+suite+'-tests',args);results.append(dict(kind='BUILD',suite=suite,command=clean(built)))
  if built['exit_code']!=0:break
  dest=run/('results-'+suite);dest.mkdir();dll=folder/('BESHStatNG.Test.dll' if framework else 'BESHStat.Core.Tests.dll' if suite=='Core' else 'BESHStat.Excel.Common.Tests.dll')
  args=([runner.vstest] if framework else ['dotnet','vstest'])+[str(dll),'/Settings:'+str(ROOT/'test.runsettings'),'/TestCaseFilter:'+filter,'/Logger:trx;LogFileName=focused.trx','/ResultsDirectory:'+str(dest)]
  if framework:args+=['/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common')]
  tested=command(stage+'-focus-legacy-v2-'+config+'-test-'+suite,args);result=dict(kind='TEST',suite=suite,command=clean(tested),tests=clean(m.trx(dest/'focused.trx')));results.append(result)
  assert sum(result['tests']['outcomes'].values())>0,'Zero-test focused selector'
  if tested['exit_code']!=0:break
 save('focused-legacy-v2-'+stage+'-'+config+'.json',dict(results=results,original_queries=0));assert len(results)==2 and all(r['command']['exit_code']==0 for r in results),'Focused lane failed; preserve failed receipt'
 print('Focused current Common/Windows tests PASS',stage,config)
if __name__=='__main__':main()
