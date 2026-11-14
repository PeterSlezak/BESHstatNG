"""After frozen Python completes, expose the current-only error-box alias gap."""
from common import *
import shutil
assert (HERE/'python-frozen-input-postcheck-v1.json').is_file(),'Do not mutate frozen Python inputs during the campaign'
assert json.loads((HERE/'python-execution-summary-v1.json').read_bytes())['executed']==956
changes=[]
for path in ['BESHStat.Excel.Common.Tests/src/UdfContinuation17Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']:
 raw=(ROOT/path).read_bytes();p=HERE/'error-cell-test-before'/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(raw);s=raw.decode('utf-8-sig');end=s.rfind('End Class');assert end>0
 if path.startswith('BESHStat.Excel.Common.Tests'):
  extra='''    Private Enum MedianError17
        NotAvailable = 42
    End Enum

    Private Shared Function MedianFinite17(value As Object, ByRef parsed As Double) As Boolean
        parsed = DirectCast(value, Double)
        Return Not Double.IsNaN(parsed) AndAlso Not Double.IsInfinity(parsed)
    End Function

    Private Shared Function MedianStatus17(value As Object, ByRef parsed As Integer) As Boolean
        parsed = CInt(DirectCast(value, Double))
        Return parsed = 0 OrElse parsed = 1
    End Function

    <TestMethod>
    Public Sub Median17ErrorCellsRetainDistinctOriginalBoxingInPortablePreparation()
        Dim times As Object(,) = {{1.0}, {2.0}, {3.0}}
        Dim status As Object(,) = {{0.0}, {0.0}, {0.0}}
        Dim result(,) As Object = Nothing
        Assert.IsTrue(UdfW2CompletionPreparation38.ComputeSurvivalMedianCi(3, times, status, Nothing, False, 0.05R, result, MedianError17.NotAvailable, AddressOf MedianFinite17, AddressOf MedianStatus17, Function(v) Convert.ToString(v)))
        Assert.AreEqual(MedianError17.NotAvailable, DirectCast(result(0, 1), MedianError17))
        Assert.AreEqual(MedianError17.NotAvailable, DirectCast(result(0, 2), MedianError17))
        Assert.AreNotSame(result(0, 1), result(0, 2))
        Assert.AreNotSame(result(0, 2), result(0, 3))
    End Sub

'''
 else:
  extra='''    <TestMethod>
    Public Sub Median17ErrorCellsRetainDistinctOriginalBoxesThroughWindowsAdapter()
        Dim times As Object(,) = {{1.0}, {2.0}, {3.0}}
        Dim status As Object(,) = {{0.0}, {0.0}, {0.0}}
        Dim result(,) As Object = Nothing
        Assert.IsTrue(Global.BESHStatNG.UdfDataImport.TryComputeSurvivalMedianCi(times, status, Nothing, 0.05R, result))
        Assert.AreEqual(ExcelError.ExcelErrorNA, DirectCast(result(0, 1), ExcelError))
        Assert.AreEqual(ExcelError.ExcelErrorNA, DirectCast(result(0, 2), ExcelError))
        Assert.AreNotSame(result(0, 1), result(0, 2))
        Assert.AreNotSame(result(0, 2), result(0, 3))
    End Sub

'''
 s=s[:end]+extra+s[end:];(ROOT/path).write_bytes((b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+s.encode('utf-8'));changes.append(dict(path=path,before=pin(p),after=pin(ROOT/path)))
freeze=json.loads((HERE/'production-test-freeze-v5.json').read_bytes());freeze['prior_candidate']=pin(HERE/'production-test-freeze-v5.json')
for r in freeze['files']:
 r['after']=pin(ROOT/r['path']);p=HERE/'source-after-v6'/r['path'];p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((ROOT/r['path']).read_bytes());r['snapshot']=pin(p)
freeze['sources']=[r['after'] for r in freeze['files']];save('production-test-freeze-v6.json',freeze)
label='w2c17-error-cell-counterexample-v1';run=RUNROOT/'builds'/label/'Debug';run.mkdir(parents=True,exist_ok=False)
for role in ['portable','legacy']:shutil.copytree(RUNROOT/'builds/w2c17-new-fixture-correction-v1/Debug'/role,run/role)
runner=m.Runner(RUNROOT);commands=[];results=[]
for suite,project,role in [('Common','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','portable'),('Legacy','BESHStatNG.Test/BESHStatNG.Tests.vbproj','legacy')]:
 args=(['dotnet','msbuild'] if suite=='Common' else [runner.msbuild])+[project,'/t:Build','/p:Configuration=Debug','/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(run/role)+'\\','/p:IntermediateOutputPath='+str(run/'obj'/suite)+'\\','/m:4','/v:minimal','/nr:false']
 r=command('error-cell-counterexample-build-'+suite,args);commands.append(clean(r));assert r['exit_code']==0
 r=runner.test('Debug',label,suite,label+'-'+suite,'FullyQualifiedName~Median17ErrorCells');results.append(clean(r))
save('error-cell-current-only-counterexample-v1.json',dict(changes=changes,source_freeze=pin(HERE/'production-test-freeze-v6.json'),commands=commands,results=results,original_queries=0,authority='Existing original native source/CIL boxes each error cell independently. New current-only tests expose reuse of one Object sentinel introduced by extraction. No original implementation is executed.',full_campaign_scope='Earlier full managed/Python campaigns predate this correction; retain genuine results and do not call them final-source comprehensive campaigns.'))
assert all(r['outcomes']=={'Failed':1} and all('AreNotSame' in x['message'] for x in r['records']) for r in results)
print('Both current-only box-alias counterexamples FAIL as predicted; no original run')
