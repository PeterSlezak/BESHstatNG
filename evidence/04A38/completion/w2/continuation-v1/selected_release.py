"""Selected Release complements the new family focus; never full Release."""
import shutil
from common import *
stage='p1-v5';run=PRIVATE/stage/'Release';prod=run/'lib';runner=m.Runner(RUNROOT)
folder=run/'tests-Core';folder.mkdir()
for n in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll']:shutil.copy2(prod/n,folder/n)
b=command('selected-Release-Core-build',['dotnet','msbuild',str(ROOT/'BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj'),'/t:Build','/p:Configuration=Release','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(folder)+'\\','/p:IntermediateOutputPath='+str(run/'obj/selected-Core')+'\\','/m:4','/v:minimal','/nr:false']);assert b['exit_code']==0
records=[]
for suite,dll,filter in [('Core',folder/'BESHStat.Core.Tests.dll','FullyQualifiedName~UdfRowMaskPreparation38Tests|FullyQualifiedName~UdfNumericPreparation38Tests'),('Common',run/'tests-Common/BESHStat.Excel.Common.Tests.dll','FullyQualifiedName~UdfPreparation38Tests')]:
 dest=run/('results-selected-'+suite);dest.mkdir()
 r=command('selected-Release-'+suite,['dotnet','vstest',str(dll),'/Settings:'+str(ROOT/'test.runsettings'),'/TestCaseFilter:'+filter,'/Logger:trx;LogFileName=selected.trx','/ResultsDirectory:'+str(dest)])
 tests=m.trx(dest/'selected.trx');records.append(dict(suite=suite,command=clean(r),tests=clean(tests)));assert r['exit_code']==0
save('selected-Release.json',dict(build=clean(b),results=records,full_Release='NOT_RUN_OWNER_PREFERENCE',new_family_focus='focused-p1-v5-Release.json; count identities once'))
print('Additional selected Release Core/Common PASS')
