"""Selected Release, with focused Core preparation in Debug as well."""
import shutil
from common import *
stage=sys.argv[1];runner=m.Runner(RUNROOT);records=[]
for config in ['Debug','Release']:
 run=PRIVATE/stage/config;prod=run/'lib';folder=run/'tests-Core';folder.mkdir()
 for n in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll']:shutil.copy2(prod/n,folder/n)
 b=command(stage+'-'+config+'-selected-Core-build',['dotnet','msbuild',str(ROOT/'BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj'),'/t:Build','/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(folder)+'\\','/p:IntermediateOutputPath='+str(run/'obj/selected-Core')+'\\','/m:4','/v:minimal','/nr:false']);records.append(dict(kind='BUILD',configuration=config,command=clean(b)));assert b['exit_code']==0
 dest=run/'results-selected-Core';dest.mkdir()
 r=command(stage+'-'+config+'-selected-Core',['dotnet','vstest',str(folder/'BESHStat.Core.Tests.dll'),'/Settings:'+str(ROOT/'test.runsettings'),'/TestCaseFilter:FullyQualifiedName~UdfRowMaskPreparation38Tests|FullyQualifiedName~UdfNumericPreparation38Tests','/Logger:trx;LogFileName=selected.trx','/ResultsDirectory:'+str(dest)])
 records.append(dict(kind='TEST',configuration=config,suite='Core',command=clean(r),tests=clean(m.trx(dest/'selected.trx'))));assert r['exit_code']==0
save('selected-Core-'+stage+'.json',dict(results=records,full_Release='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
