"""Keep original Cox text-header policy in the new regression; no production change."""
from common import *
import shutil
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';target=HERE/'source-before-focused-fix-v2'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
with target.open('xb') as f:f.write((ROOT/path).read_bytes())
text=(ROOT/path).read_text(encoding='utf-8-sig')
old='        Assert.IsTrue(UdfDataImport.TryBuildCoxDataFromUdfArgs(time, New Object(,) {{"Event"}, {0.0R}, {1.0R}, {0.0R}, {1.0R}}, x, Nothing, groups, cox))'
new='''        Assert.IsFalse(UdfDataImport.TryBuildCoxDataFromUdfArgs(time, New Object(,) {{"Event"}, {0.0R}, {1.0R}, {0.0R}, {1.0R}}, x, Nothing, groups, cox))
        Assert.IsNull(cox)
        Assert.IsTrue(UdfDataImport.TryBuildCoxDataFromUdfArgs(time, New Object(,) {{"Event"}, {0.0R}, {1.0R}, {0.0R}, {1.0R}}, x, Nothing, New Object(,) {{"a"}, {"b"}, {"a"}, {"b"}}, cox))'''
assert text.count(old)==1;write_source(path,text.replace(old,new))
save('focused-new-test-correction-v2.json',dict(preimage=pin(target),after=pin(ROOT/path),authorities=[pin(ROOT/'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb'),pin(PREVIOUS/'source-before/BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb.txt')],reason='Original text-strata trimming skips a header only for a full-column ExcelReference, not an ordinary object matrix. Assert headered object strata rejection/reset, then supply unheadered strata and retain all type/order assertions. Only newly appended test changed.',original_queries=0))
run=PRIVATE/'focused-corrective-v2';run.mkdir(exist_ok=False);prod=PRIVATE/'packet-v1/Debug/lib';folder=run/'lib';folder.mkdir();runner=m.Runner(RUNROOT)
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(prod/name,folder/name)
built=command('focused-corrective-v2-build',[runner.msbuild,str(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj'),'/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(folder)+'\\','/p:IntermediateOutputPath='+str(run/'obj')+'\\','/m:4','/v:minimal','/nr:false']);assert built['exit_code']==0
result=command('focused-corrective-v2-test',[runner.vstest,str(folder/'BESHStatNG.Test.dll'),'/Settings:'+str(ROOT/'test.runsettings'),'/TestCaseFilter:FullyQualifiedName~Continuation8ImportParentsKeepAlignmentFlagsAndFailureResets','/Logger:trx;LogFileName=corrective.trx','/ResultsDirectory:'+str(run/'results'),'/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common')])
save('focused-corrective-v2.json',dict(build=clean(built),test=clean(result),outcomes=clean(m.trx(run/'results/corrective.trx')),original_queries=0));assert result['exit_code']==0
