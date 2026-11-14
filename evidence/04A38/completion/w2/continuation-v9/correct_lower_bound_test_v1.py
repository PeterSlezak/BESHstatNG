"""Correct a new assertion using the unchanged accepted Windows transport source."""
from common_v2 import *
import shutil
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';before=pin(ROOT/path);target=HERE/'attempts/new-lower-bound-test-before-v1.vb.txt';target.parent.mkdir(exist_ok=True)
with target.open('xb') as f:f.write((ROOT/path).read_bytes())
source=(ROOT/path).read_text(encoding='utf-8-sig');old='''        Assert.IsInstanceOfType(result, GetType(String))
        Assert.IsTrue(CStr(result).StartsWith("IndexOutOfRangeException:", StringComparison.Ordinal))''';new='''        Assert.IsInstanceOfType(result, GetType(Object(,)))
        Dim expected = DirectCast(ClassificationReportUDFs.CLASS_BRIER(New Object(,) {{0.0R}, {1.0R}}, New Object(,) {{0.2R}, {0.8R}}), Object(,))
        CollectionAssert.AreEqual(expected.Cast(Of Object)().ToArray(), DirectCast(result, Object(,)).Cast(Of Object)().ToArray())'''
assert source.count(old)==1;write_source(path,source.replace(old,new,1))
save('new-lower-bound-test-correction-v1.json',dict(before=before,preimage=pin(target),after=pin(ROOT/path),reason='WindowsSpreadsheetTransport.ReadUdfMatrix calls existing CopyValues, which normalizes source Array lower bounds before strict-vector callbacks. Only the new mistaken assertion changes; existing import or statistical behavior is untouched.',source_authority=pin(ROOT/'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb'),historical_assertions_changed=0,production_changes=0))
run=PRIVATE/'focused-corrective-v1';run.mkdir(exist_ok=False);lib=run/'lib';lib.mkdir();runner=m.Runner(RUNROOT)
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(PRIVATE/'packet-v1/Debug/lib'/name,lib/name)
built=command('focused-corrective-v1-build',[runner.msbuild,str(ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj'),'/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(lib)+'\\','/p:IntermediateOutputPath='+str(run/'obj')+'\\','/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']);assert built['exit_code']==0
results=run/'results';results.mkdir();tested=command('focused-corrective-v1-test',[runner.vstest,str(lib/'BESHStatNG.Test.dll'),'/Settings:'+str(ROOT/'test.runsettings'),'/TestCaseFilter:FullyQualifiedName~Continuation9BrierValidationPrecedesHeaderAndLowerBoundsStayNative','/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common'),'/Logger:trx;LogFileName=focused.trx','/ResultsDirectory:'+str(results)])
cases=m.trx(results/'focused.trx');save('focused-corrective-v1.json',dict(build=clean(built),command=clean(tested),cases=clean(cases),first_attempt='focused-v3-packet-v1-Debug.json',source_unchanged=True));assert tested['exit_code']==0 and cases['outcomes']=={'Passed':1}
print('One corrected new adapter assertion PASS; production sources unchanged')
