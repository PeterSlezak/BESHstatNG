"""Move original preparation operations; retain numerical engines and host error adapter."""
import re
from common import *

extra='BESHstatNG/src/ExcelUDFs/UdfOutputTables.vb'
p=ROOT/extra;before=pin(p);snapshot=HERE/'source-before'/ (extra+'.txt');snapshot.parent.mkdir(parents=True,exist_ok=True)
with snapshot.open('xb') as f:f.write(p.read_bytes())
save('source-before-b1-addendum.json',dict(source=before,snapshot=pin(snapshot)))

def replace_body(path,name,body,signature=None):
 p=ROOT/path;s=p.read_text(encoding='utf-8-sig')
 pattern=signature or r'(?m)^        (?:Public|Friend) Function '+name+r'\([\s\S]*?\) As Object(?:\(,\))?'
 m0=re.search(pattern,s);assert m0,name
 end=s.index('        End Function',m0.end())
 write_source(path,s[:m0.end()]+'\n'+body+'\n'+s[end:])

path='BESHstatNG/src/ExcelUDFs/DistributionUDFs.vb'
for name,target,args in [('PRTRNG','PrepareStudentizedRangeCdf','q, v, r'),('PRTRNG_TAIL','PrepareStudentizedRangeTail','q, v, r'),('F_PDF','PrepareFDistributionDensity','x, df1, df2')]:
 replace_body(path,name,'\n            Dim result As Double? = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.'+target+'('+args+')\n            If Not result.HasValue Then Return ExcelError.ExcelErrorNum\n            Return result.Value')

path='BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb';s=(ROOT/path).read_text(encoding='utf-8-sig')
insert='''
        ' Original DistributionUDFs preparation; Nothing means the host must return #NUM!.
        Public Shared Function PrepareStudentizedRangeCdf(q As Double, v As Double, r As Double) As Double?
            If Double.IsNaN(q) OrElse Double.IsNaN(v) OrElse Double.IsNaN(r) Then Return Nothing
            If q < 0 OrElse v <= 0 OrElse r < 2 Then Return Nothing
            Dim fault As Integer = 0
            Dim probability As Double = distributions.Distributions.PRTRNG(q, v, r, fault)
            If fault <> 0 OrElse Double.IsNaN(probability) OrElse Double.IsInfinity(probability) Then Return Nothing
            Return probability
        End Function

        Public Shared Function PrepareStudentizedRangeTail(q As Double, v As Double, r As Double) As Double?
            Dim cdf As Double? = PrepareStudentizedRangeCdf(q, v, r)
            If Not cdf.HasValue Then Return Nothing
            Dim tail As Double = 1.0 - cdf.Value
            If tail < 0 Then tail = 0.0
            If tail > 1 Then tail = 1.0
            Return tail
        End Function

        Public Shared Function PrepareFDistributionDensity(x As Double, df1 As Double, df2 As Double) As Double?
            If Double.IsNaN(x) OrElse Double.IsNaN(df1) OrElse Double.IsNaN(df2) Then Return Nothing
            If x < 0 OrElse df1 <= 0 OrElse df2 <= 0 Then Return Nothing
            If x = 0 Then
                If df1 > 2 Then Return 0.0
                If df1 = 2 Then Return 1.0
                Return Nothing
            End If
            Dim density As Double = distributions.Distributions.F_PDF(x, df1, df2)
            If Double.IsNaN(density) OrElse Double.IsInfinity(density) Then Return Nothing
            Return density
        End Function
'''
assert 'PrepareStudentizedRangeCdf' not in s
write_source(path,s.replace('    End Class',insert+'    End Class'))

path='BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb';s=(ROOT/path).read_text(encoding='utf-8-sig')
m0=re.search(r'(?m)^        Friend Function StackWithBlankRow[^\n]+',s);end=s.index('        End Function',m0.end());old=s[m0.end():end]
write_source(path,s[:m0.end()]+'\n            Return Global.BESHStatNG.ExcelCommon.UdfSpillPreparation38.StackWithBlankRow(a, b)\n'+s[end:])
path='BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb';s=(ROOT/path).read_text(encoding='utf-8-sig')
stack='\n        Public Shared Function StackWithBlankRow(a As Object(,), b As Object(,)) As Object(,)'+old+'        End Function\n'
path2=extra;s2=(ROOT/path2).read_text(encoding='utf-8-sig')
m0=re.search(r'(?m)^    Friend Function PrepareResultTableForUdf\(table As Object\) As Object\(,\)',s2);end=s2.index('    End Function',m0.end());body=s2[m0.end():end]
normal='\n        Public Shared Function NormalizeBlanks(table As Object) As Object(,)'+''.join('    '+line+'\n' for line in body.splitlines())+'        End Function\n'
write_source(path2,s2[:m0.end()]+'\n        Return Global.BESHStatNG.ExcelCommon.UdfSpillPreparation38.NormalizeBlanks(table)\n'+s2[end:])
assert 'Function StackWithBlankRow' not in s and 'Function NormalizeBlanks' not in s
write_source(path,s.replace('    End Class',stack+normal+'    End Class'))
save('implementation-b1.json',dict(status='SOURCE_EDITED_NOT_YET_VALIDATED',sources=[pin(ROOT/p) for p in ['BESHstatNG/src/ExcelUDFs/DistributionUDFs.vb','BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb','BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb',extra,'BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb']],numerical_engine=pin(ROOT/'BESHStat.Core/src/BaseStat/Distributions.vb'),original_queries=0))
