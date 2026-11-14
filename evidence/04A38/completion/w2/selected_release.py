"""Selected existing04A38 Release regression tests, not a full Release suite."""
from capture_current import *
def main():
 run=RUNROOT/'completion-w2/profiles-v3/Release';lib=run/'lib';legacy=run/'legacy-selected';legacy.mkdir();runner=m.Runner(RUNROOT)
 for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(lib/n,legacy/n)
 b=runner.command('w2-selected-Release-legacy-build',[runner.msbuild,str(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj'),'/t:Build','/p:Configuration=Release','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(legacy)+'\\','/p:IntermediateOutputPath='+str(run/'obj/legacy-selected')+'\\','/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']);assert b['exit_code']==0
 records=[]
 for suite,dll,typename in [('Core',lib/'BESHStat.Core.Tests.dll','UdfNumericPreparation38Tests'),('Common',lib/'BESHStat.Excel.Common.Tests.dll','UdfPreparation38Tests'),('Legacy',legacy/'BESHStatNG.Test.dll','UdfPreparation38AdapterTests')]:
  dest=run/('selected-existing-'+suite);dest.mkdir()
  args=([runner.vstest] if suite=='Legacy' else ['dotnet','vstest'])+[str(dll),'/Settings:'+str(ROOT/'test.runsettings'),'/Logger:trx;LogFileName=selected.trx','/ResultsDirectory:'+str(dest),'/TestCaseFilter:FullyQualifiedName~'+typename]
  if suite=='Legacy':args+=['/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common')]
  r=runner.command('w2-selected-Release-'+suite,args);tests=m.trx(dest/'selected.trx');records.append(dict(suite=suite,command=clean(r),tests=clean(tests)));assert r['exit_code']==0 and set(tests['outcomes'])=={'Passed'}
 with (HERE/'selected-Release.json').open('x',encoding='utf-8') as f:json.dump(dict(legacy_build=clean(b),results=records,full_Release='NOT_RUN_OWNER_PREFERENCE',new27_Release_cases='Already executed once; focused-validation-corrected.json, not counted twice'),f,indent=2)
 print('Selected existing49 Release cases PASS; new27 separatelyPASS; no fullRelease')
if __name__=='__main__':main()
