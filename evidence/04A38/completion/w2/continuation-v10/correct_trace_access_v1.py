"""Keep existing Core visibility; pass the real original Friend Trace target from Windows."""
from common import *
paths=['BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb','BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb','BESHStat.Excel.Common.Tests/src/UdfContinuation10Tests.vb'];before=[]
for p in paths:
    target=HERE/'source-attempts/bc-trace-access-v1'/(p+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as f:f.write((ROOT/p).read_bytes())
    before.append(dict(live=pin(ROOT/p),snapshot=pin(target)))
p=paths[0];text=(ROOT/p).read_text(encoding='utf-8-sig');old='ByRef includeHeader As Object, readHeader As Func(Of Object, Boolean, Boolean)';assert old in text
text=text.replace(old,'ByRef includeHeader As Object, matrixTrace As Func(Of Double(,), Double), readHeader As Func(Of Object, Boolean, Boolean)').replace('Global.BESHStatNG.Matrix.MatrixArithmeticCore.Trace(model.WorkingMatrix)','matrixTrace(model.WorkingMatrix)');write_source(p,text)
p=paths[1];text=(ROOT/p).read_text(encoding='utf-8-sig');old='BuildFactorEigen(h.Model, includeHeader, AddressOf GetOptionalBool)';assert old in text;text=text.replace(old,'BuildFactorEigen(h.Model, includeHeader, AddressOf Global.BESHStatNG.Matrix.MatrixArithmeticCore.Trace, AddressOf GetOptionalBool)');write_source(p,text)
p=paths[2];text=(ROOT/p).read_text(encoding='utf-8-sig');assert text.count('BuildFactorEigen(model, slot,')==3 and text.count('BuildFactorEigen(Nothing, slot,')==1
text=text.replace('BuildFactorEigen(model, slot,','BuildFactorEigen(model, slot, CurrentCoreTrace(),').replace('BuildFactorEigen(Nothing, slot,','BuildFactorEigen(Nothing, slot, CurrentCoreTrace(),')
helper='''    Private Shared Function CurrentCoreTrace() As Func(Of Double(,), Double)
        Dim owner = GetType(Multivariate.FactorAnalysis).Assembly.GetType("BESHStatNG.Matrix.MatrixArithmeticCore", True)
        Dim method = owner.GetMethod("Trace", System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Static)
        Return DirectCast(method.CreateDelegate(GetType(Func(Of Double(,), Double))), Func(Of Double(,), Double))
    End Function

'''
index=text.rfind('End Class');write_source(p,text[:index]+helper+text[index:])
save('BC-trace-access-correction-v1.json',dict(before=before,after=[pin(ROOT/p) for p in paths],first_build=pin(HERE/'build-packet-v1-Debug.json'),reason='Core MatrixArithmeticCore is Friend to Windows, not Common. Preserve its visibility and exact algorithm. Windows passes its actual original Trace method as a typed named delegate, evaluated at the original point inside Common. Common tests bind that same current Core method by metadata; no duplicate trace implementation or original execution.',Core_changes=0,original_queries=0))
print('Original Core Trace target passed from authorized Windows consumer; no Core API/formula change')
