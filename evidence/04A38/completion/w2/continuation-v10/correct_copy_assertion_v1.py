"""Correct only the new assertion from observed C9/C10 real CIL; preserve the failed TRX."""
from common import *
import shutil
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';target=HERE/'source-attempts/new-copy-assertion-v1'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
with target.open('xb') as f:f.write((ROOT/path).read_bytes())
before=pin(ROOT/path);source=(ROOT/path).read_text(encoding='utf-8-sig');old='        Assert.IsFalse(Calls(fa).Any(Function(target) target.Name = "GetObjectValue"))';assert source.count(old)==1
new='''        Assert.AreEqual(1, Calls(fa).Count(Function(target) target.Name = "GetObjectValue"), "The original handle-argument copy stays native.")
        Dim portableCalls = Calls(api).ToList()
        Assert.AreEqual(1, portableCalls.Count(Function(target) target.Name = "GetObjectValue"), "The original header copy occurs only at late delegate invocation.")
        Assert.IsTrue(portableCalls.FindIndex(Function(target) target.Name = "GetObjectValue") > portableCalls.FindLastIndex(Function(target) target.Name = "Cumulative"))'''
write_source(path,source.replace(old,new))
save('new-copy-assertion-correction-v1.json',dict(before=before,preimage=pin(target),after=pin(ROOT/path),basis=pin(HERE/'boxing-copy-investigation-v1.json'),first_tests=pin(HERE/'focused-bc-v1-packet-v2-Debug.json'),reason='New assertion incorrectly prohibited every GetObjectValue in FA_EIGEN. Preserved C9 CIL has one handle copy and one late header copy; C10 keeps the handle copy native and the single header copy after derived arrays in Common. Only the new test changes; no production or historical assertion change.',original_queries=0))
run=PRIVATE/'focused-copy-corrective-v1';out=run/'lib';out.mkdir(parents=True,exist_ok=False)
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(PRIVATE/'packet-v2/Debug/lib'/name,out/name)
runner=m.Runner(RUNROOT);built=command('copy-corrective-build-v1',[runner.msbuild,str(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj'),'/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(out)+'\\','/p:IntermediateOutputPath='+str(run/'obj')+'\\','/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']);assert built['exit_code']==0
results=run/'results';results.mkdir();tested=command('copy-corrective-test-v1',[runner.vstest,str(out/'BESHStatNG.Test.dll'),'/Settings:'+str(ROOT/'test.runsettings'),'/TestCaseFilter:FullyQualifiedName~Continuation10PcaFactorCompiledConsumersKeepPortableAndHeaderTargets','/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common'),'/Logger:trx;LogFileName=corrective.trx','/ResultsDirectory:'+str(results)]);parsed=m.trx(results/'corrective.trx');save('focused-copy-corrective-v1.json',dict(build=clean(built),test=clean(tested),result=clean(parsed),raw_TRX=pin(results/'corrective.trx'),scope='Only the one affected new assertion rerun. First BC focused Windows4PASS/1FAIL retained, no comprehensive campaign yet.',original_queries=0));assert tested['exit_code']==0 and parsed['outcomes']=={'Passed':1}
print('One affected new copy-boundary case PASS; first attempt retained')
