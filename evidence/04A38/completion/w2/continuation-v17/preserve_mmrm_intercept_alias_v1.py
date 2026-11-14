"""Restore an authenticated original alias boundary; leave the Core API unchanged."""
from proof_support_v17 import *
p='BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb';raw=(ROOT/p).read_bytes();target=HERE/'intercept-alias-before'/p;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(raw)
s=raw.decode('utf-8-sig');old='            Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.AddInterceptIfRequested(x, includeIntercept)'
newline='\r\r\n';assert old in s
new=('            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))\n            If Not includeIntercept Then Return x\n'+old).replace('\n',newline)
assert s.count(old)==1;s=s.replace(old,new,1);(ROOT/p).write_bytes((b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+s.encode('utf-8'))
t=ROOT/'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';testbefore=t.read_bytes();snap=HERE/'intercept-alias-test-before-v1.vb.txt';snap.write_bytes(testbefore);text=testbefore.decode('utf-8-sig');index=text.rfind('End Class');assert index>0
added='''    <TestMethod>
    Public Sub Mmrm17NoInterceptRetainsOriginalArrayAliasAndNullPrecedence()
        Dim native = GetType(Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs).GetMethod("AddInterceptIfRequested", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Static)
        Assert.IsNotNull(native)
        Dim data(,) As Double = {{1.0, 2.0}, {3.0, 4.0}}
        Assert.AreSame(data, native.Invoke(Nothing, New Object() {data, False}))
        Dim withIntercept = DirectCast(native.Invoke(Nothing, New Object() {data, True}), Double(,))
        Assert.AreNotSame(data, withIntercept)
        Assert.AreEqual(3, withIntercept.GetLength(1))
        Assert.AreEqual(1.0, withIntercept(0, 0))
        For Each includeIntercept As Boolean In New Boolean() {False, True}
            Dim ex = Assert.ThrowsException(Of System.Reflection.TargetInvocationException)(Sub() native.Invoke(Nothing, New Object() {Nothing, includeIntercept}))
            Assert.IsInstanceOfType(ex.InnerException, GetType(ArgumentNullException))
            Assert.AreEqual("x", DirectCast(ex.InnerException, ArgumentNullException).ParamName)
        Next
    End Sub

'''
text=text[:index]+added+text[index:];t.write_bytes((b'\xef\xbb\xbf' if testbefore.startswith(b'\xef\xbb\xbf') else b'')+text.encode('utf-8'))
save('intercept-alias-correction-v1.json',dict(path=p,before=pin(target),after=pin(ROOT/p),replacement=dict(before=old,after=new),test_before=pin(snap),test_after=pin(t),Core_modified=False,reason='Original null guard precedes false/no-intercept alias return. Existing Core copy API remains unchanged and is reached only with includeIntercept=True.',previous_compatibility=pin(HERE/'compatibility-combined-v3.json'),previous_result_scope='EXACT29424_AT_EARLIER_FREEZE3;NOT_FINAL_AFTER_ALIAS_CORRECTION',managed_campaigns_started=0,original_queries=0))
f=load(HERE/'production-test-freeze-v3.json');f['prior_candidate']=pin(HERE/'production-test-freeze-v3.json')
for r in f['files']:
 r['after']=pin(ROOT/r['path']);p2=HERE/'source-after-v4'/r['path'];p2.parent.mkdir(parents=True,exist_ok=True);p2.write_bytes((ROOT/r['path']).read_bytes());r['snapshot']=pin(p2);r['changed_since_v16']=r['after']!=r['prior_freeze']
f['sources']=[r['after'] for r in f['files']];save('production-test-freeze-v4.json',f)
for a,b in [('build_final_Debug_v3.py','build_final_Debug_v4.py'),('inspect_final_Debug_v3.py','inspect_final_Debug_v4.py'),('compatibility_Debug_v1.py','compatibility_Debug_v2.py')]:
 s=(HERE/a).read_text(encoding='utf-8').replace('production-test-freeze-v3.json','production-test-freeze-v4.json')
 if b.startswith('inspect'):s=s.replace('final-v3','final-v4').replace('inspection-v3','inspection-v4').replace('-v17-final3','-v17-final4')
 with (HERE/b).open('x',encoding='utf-8',newline='\n') as x:x.write(s)
print('Original MMRM alias/null precedence restored; earlier source and compatibility retained')
