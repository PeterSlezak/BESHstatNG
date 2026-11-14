"""Correct a new test's expectation to the unchanged Core GEE sort contract."""
from common import *
import shutil
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb'
target=HERE/'source-before-focused-fix-v1'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
with target.open('xb') as f:f.write((ROOT/path).read_bytes())
text=(ROOT/path).read_text(encoding='utf-8-sig')
old='        Assert.AreEqual(2.0R, Convert.ToDouble(gee.ClusterIdData(1)))'
new='''        CollectionAssert.AreEqual(New Double() {1.0R, 1.0R, 2.0R, 2.0R}, gee.ClusterIdData.Select(Function(value) Convert.ToDouble(value)).ToArray())
        CollectionAssert.AreEqual(New Integer() {1, 3, 2, 4}, gee.RowIds)
        CollectionAssert.AreEqual(New Double() {-1.0R, 1.0R, 0.0R, 2.0R}, gee.OffsetData)
        CollectionAssert.AreEqual(New Double() {1.0R, 3.0R, 2.0R, 4.0R}, gee.TimeData)'''
assert text.count(old)==1;write_source(path,text.replace(old,new))
save('focused-new-test-correction-v1.json',dict(preimage=pin(target),after=pin(ROOT/path),unchanged_core=pin(ROOT/'BESHStat.Core/src/DM/GeeData.vb'),first_attempt='focused-v3-packet-v1-Debug.json',reason='New test incorrectly assumed source order after geeData.OnDataImported. Authentic unchanged Core sorts by cluster/time and carries RowIds, offsets, weights and time together. Replace that new assertion with the complete sorted cluster/row/offset/time contract. No production or historical test assertion changed.',original_queries=0))
run=PRIVATE/'focused-corrective-v1';run.mkdir(exist_ok=False);prod=PRIVATE/'packet-v1/Debug/lib';folder=run/'lib';folder.mkdir();runner=m.Runner(RUNROOT)
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(prod/name,folder/name)
built=command('focused-corrective-v1-build',[runner.msbuild,str(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj'),'/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(folder)+'\\','/p:IntermediateOutputPath='+str(run/'obj')+'\\','/m:4','/v:minimal','/nr:false']);assert built['exit_code']==0
result=command('focused-corrective-v1-test',[runner.vstest,str(folder/'BESHStatNG.Test.dll'),'/Settings:'+str(ROOT/'test.runsettings'),'/TestCaseFilter:FullyQualifiedName~Continuation8ImportParentsKeepAlignmentFlagsAndFailureResets','/Logger:trx;LogFileName=corrective.trx','/ResultsDirectory:'+str(run/'results'),'/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common')])
save('focused-corrective-v1.json',dict(build=clean(built),test=clean(result),outcomes=clean(m.trx(run/'results/corrective.trx')),original_queries=0));assert result['exit_code']==0
