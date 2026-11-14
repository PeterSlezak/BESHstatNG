"""Current portable/actual shipping preparation contract tests."""
import shutil
from common import *
def main():
 stage,config=sys.argv[1:3];run=PRIVATE/stage/config;prod=run/'lib';runner=m.Runner(RUNROOT);results=[]
 for suite,project,framework,filter in [('Common','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj',False,'FullyQualifiedName~UdfModelProfilePreparation38Tests|FullyQualifiedName~UdfPreparation38Tests'),('Legacy','BESHStatNG.Test/BESHStatNG.Tests.vbproj',True,'FullyQualifiedName~UdfPreparation38AdapterTests')]:
  folder=run/('tests-'+suite);folder.mkdir()
  for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(prod/n,folder/n)
  args=([runner.msbuild] if framework else ['dotnet','msbuild'])+[str(ROOT/project),'/t:Build','/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(folder)+'\\','/p:IntermediateOutputPath='+str(run/'obj'/('tests-'+suite))+'\\','/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']
  built=command(stage+'-'+config+'-build-'+suite+'-tests',args);results.append(dict(kind='BUILD',suite=suite,command=clean(built)))
  if built['exit_code']!=0:break
  dest=run/('results-'+suite);dest.mkdir();dll=folder/('BESHStatNG.Test.dll' if framework else 'BESHStat.Excel.Common.Tests.dll')
  args=([runner.vstest] if framework else ['dotnet','vstest'])+[str(dll),'/Settings:'+str(ROOT/'test.runsettings'),'/TestCaseFilter:'+filter,'/Logger:trx;LogFileName=focused.trx','/ResultsDirectory:'+str(dest)]
  if framework:args+=['/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common')]
  tested=command(stage+'-'+config+'-test-'+suite,args);result=dict(kind='TEST',suite=suite,command=clean(tested),tests=clean(m.trx(dest/'focused.trx')));results.append(result)
  if tested['exit_code']!=0:break
 save('focused-'+stage+'-'+config+'.json',dict(results=results,original_queries=0));assert len(results)==4 and all(r['command']['exit_code']==0 for r in results),'Focused lane failed; preserve failed receipt'
 print('Focused current Common/Windows tests PASS',stage,config)
if __name__=='__main__':main()
