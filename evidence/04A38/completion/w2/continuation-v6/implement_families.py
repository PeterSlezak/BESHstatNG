"""Extract shared report/selection/array preparation; Core engines unchanged."""
from common import *
import re
assert json.loads((HERE/'compatibility-imports-v1.json').read_bytes())['all_exact']
assert json.loads((HERE/'inspection-imports-v1.json').read_bytes())['exports']==[{'configuration':'Debug','exact':268},{'configuration':'Release','exact':268}]
PAR='BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb'
NP='BESHstatNG/src/ExcelUDFs/NonparametricUDFs.vb'
NEW='BESHStat.Excel.Common/src/Results/UdfInferencePreparation38.vb'
def member(text,name):
 m=re.search(r'^([ \t]*)(?:Friend|Public|Private)(?: Shared)? Function '+name+r'\b',text,re.M);assert m,name
 end=text.index('\n'+m[1]+'End Function',m.end())+len('\n'+m[1]+'End Function')
 return text[m.start():end]
records=[];bodies=[]
def update(path,name,operation):
 text=(ROOT/path).read_text(encoding='utf-8-sig');old=member(text,name);new=operation(old);assert old!=new,name
 write_source(path,text.replace(old,new,1));records.append(dict(path=path,name=name,before_body=old,after_body=new))
def method(name,args,ret,body):
 code='        Public Shared Function '+name+'('+args+') As '+ret+'\n'+body+'\n        End Function'
 bodies.append(code);return 'Global.BESHStatNG.ExcelCommon.UdfInferencePreparation38.'+name

# Report bodies stay byte-for-byte equivalent statements; only CI helpers qualify
# their existing canonical Common owner. Engine fit and setting/conversion timing
# remain in each original native caller.
for name,result_type,helper in [('TTEST_UNPAIRED_NI','MeanNonInferiorityResult','MeanNonInferiorityBody'),('TTEST_UNPAIRED_EQUIV','MeanEquivalenceResult','MeanEquivalenceBody')]:
 text=(ROOT/PAR).read_text(encoding='utf-8-sig');old=member(text,name)
 start=old.index('                Dim body As Object(,) = {');end=old.index('\n\n                Return BuildResultTable(',start)
 chunk=old[start:end]
 code=chunk.replace('SafeCiLabel(', 'UdfOutputTablesReports38.SafeCiLabel(').replace('SafeCiText(', 'UdfOutputTablesReports38.SafeCiText(')+'\n            Return body'
 call=method(helper,'result As '+result_type+', names() As String','Object(,)',code)
 update(PAR,name,lambda old,chunk=chunk,call=call:old.replace(chunk,'                Dim body As Object(,) = '+call+'(result, names)',1))

text=(ROOT/PAR).read_text(encoding='utf-8-sig');old=member(text,'RMANOVA1')
start=old.index('                Select Case which');end=old.index('\n\n                Dim tables',start)
select=old[start:end].replace('Select Case which','Select Case LegacyUdfText38.CaseToken(which, {"", "NONE", "GG", "GREENHOUSE", "GREENHOUSE-GEISSER", "GREENHOUSEGEISSER", "HF", "HUYNH", "HUYNH-FELDT", "HUYNHFELDT", "BOTH", "ALL"})',1).replace('Return ExcelError.ExcelErrorValue','Return False')
call=method('ApplyRepeatedCorrection','mdl As Global.BESHStatNG.parametric.OneWayRmANOVA, which As String','Boolean',select+'\n            Return True')
update(PAR,'RMANOVA1',lambda old:old[:start]+'                If Not '+call+'(mdl, which) Then Return ExcelError.ExcelErrorValue'+old[end:])

# RM MCP mirrors the established one-way MCP contract; selector conversion precedes
# engine construction, and the entire engine runs before invalid selection rejects.
text=(ROOT/PAR).read_text(encoding='utf-8-sig');old=member(text,'RMANOVA1_MCP')
start=old.index('                Dim mdl As New parametric.OneWayRmANOVA');stop=old.index('\n            Catch ex',start)
chunk=old[start:stop]
prefix=chunk[:chunk.index('                Select Case which')]
select=chunk[chunk.index('                Select Case which'):]
select=re.sub(r'                        Dim tables = mdl.wrapResults\(\)\n                        Return PrepareResultTableForUdf\(tables\(1\)\)', '',select)
allstart=select.index('                        Dim tables = mdl.wrapResults()')
allend=select.index('\n\n                    Case Else',allstart)
select=select[:allstart]+'                        stackAll = True'+select[allend:]
select=select.replace('Return ExcelError.ExcelErrorValue','Return Nothing')
tokens=['','RM2','TUKEYRM2','TUKEY-RM2','RECOMMENDED','NOSPHERICITY','NO-SPHERICITY','TUKEY','SPHERICITY','ASSUME-SPHERICITY','ASSUMESPHERICITY','CLASSICAL','ALL','BOTH']
select=select.replace('Select Case which','Select Case LegacyUdfText38.CaseToken(which, {'+', '.join('"'+t+'"' for t in tokens)+'})',1)
call=method('BuildRepeatedMcpTables','mat(,) As Double, names() As String, which As String, alphaValue As Double, ByRef stackAll As Boolean','List(Of Global.BESHStatNG.ResultTable)',prefix+'            stackAll = False\n'+select+'\n            Return mdl.wrapResults()')
replacement='''                Dim stackAll As Boolean = False
                Dim tables = '''+call+'''(mat, names, which, alphaValue, stackAll)
                If tables Is Nothing Then Return ExcelError.ExcelErrorValue
                If Not stackAll Then Return PrepareResultTableForUdf(tables(1))
                Dim stacked As Object(,) = PrepareResultTableForUdf(tables(1))
                For i As Integer = 2 To tables.Count - 1
                    stacked = StackWithBlankRow(stacked, PrepareResultTableForUdf(tables(i)))
                Next
                Return PrepareResultTableForUdf(stacked)'''
update(PAR,'RMANOVA1_MCP',lambda old:old[:start]+replacement+old[stop:])

# These output selections occur AFTER original engine computation, wrapping and
# potentially side-effectful NormalizeText in the host; preserve that position.
call=method('NestedTableSelection','which As String','Integer','''            If LegacyUdfText38.EqualText(which, "") OrElse LegacyUdfText38.EqualText(which, "BOTH") OrElse LegacyUdfText38.EqualText(which, "ALL") Then
                Return 0
            ElseIf LegacyUdfText38.EqualText(which, "MAIN") OrElse LegacyUdfText38.EqualText(which, "CLASSICAL") Then
                Return 1
            ElseIf LegacyUdfText38.EqualText(which, "SATTERTHWAITE") OrElse LegacyUdfText38.EqualText(which, "SW") OrElse LegacyUdfText38.EqualText(which, "ADJUSTED") Then
                Return 2
            Else
                Return -1
            End If''')
def nested(old):
 a=old.index('                If which = "" OrElse');b=old.index('\n            Catch ex',a)
 return old[:a]+'''                Select Case '''+call+'''(which)
                    Case 0
                        Return StackWithBlankRow(PrepareResultTableForUdf(tables(0)), PrepareResultTableForUdf(tables(1)))
                    Case 1
                        Return PrepareResultTableForUdf(tables(0))
                    Case 2
                        Return PrepareResultTableForUdf(tables(1))
                    Case Else
                        Return ExcelError.ExcelErrorValue
                End Select'''+old[b:]
update(PAR,'ANOVA2_NESTED',nested)
call=method('IndependentTTableSelection','which As String','Integer','''            Select Case LegacyUdfText38.CaseToken(which, {"", "BOTH", "ALL", "EQUAL", "POOLED", "STUDENT", "ASSUME-EQUAL", "ASSUMEEQUAL", "UNEQUAL", "WELCH", "ASSUME-UNEQUAL", "ASSUMEUNEQUAL"})
                Case "", "BOTH", "ALL" : Return 0
                Case "EQUAL", "POOLED", "STUDENT", "ASSUME-EQUAL", "ASSUMEEQUAL" : Return 1
                Case "UNEQUAL", "WELCH", "ASSUME-UNEQUAL", "ASSUMEUNEQUAL" : Return 2
                Case Else : Return -1
            End Select''')
def unpaired(old):
 old=old.replace('Select Case which','Select Case '+call+'(which)',1)
 for a,b in [('Case "", "BOTH", "ALL"','Case 0'),('Case "EQUAL", "POOLED", "STUDENT", "ASSUME-EQUAL", "ASSUMEEQUAL"','Case 1'),('Case "UNEQUAL", "WELCH", "ASSUME-UNEQUAL", "ASSUMEUNEQUAL"','Case 2')]:old=old.replace(a,b,1)
 return old
update(PAR,'TTEST_UNPAIRED',unpaired)

# Shared rank-correlation columns; engines already live in Core (KendallsTau
# intentionally remains internal to Core and its existing Windows friend).
split='''        Public Shared Sub SplitCorrelationColumns(pairs(,) As Double, ByRef x() As Double, ByRef y() As Double)
            Dim n As Integer = pairs.GetLength(0)
            x = New Double(n - 1) {}
            y = New Double(n - 1) {}
            For i As Integer = 0 To n - 1
                x(i) = pairs(i, 0)
                y(i) = pairs(i, 1)
            Next
        End Sub'''
bodies.append(split)
for name in ['SPEARMAN_RHO','SPEARMAN_P','KENDALL_TAU','KENDALL_P']:
 def correlation(old):
  a=old.index('            Dim n As Integer = pairs.GetLength(0)');b=old.index('\n\n            Dim test',a)
  return old[:a]+'''            Dim x() As Double = Nothing
            Dim y() As Double = Nothing
            Global.BESHStatNG.ExcelCommon.UdfInferencePreparation38.SplitCorrelationColumns(pairs, x, y)'''+old[b:]
 update(NP,name,correlation)
for source_name,helper,names in [('KW_STAT','KruskalGroupNames',['KW_STAT','KW_P']),('FRIEDMAN_STAT','FriedmanGroupNames',['FRIEDMAN_STAT','FRIEDMAN_P'])]:
 old=member((ROOT/NP).read_text(encoding='utf-8-sig'),source_name)
 a=old.index('            Dim names(');b=old.index('\n\n',a)
 chunk=old[a:b];count='data.Length' if source_name=='KW_STAT' else 'k'
 portable=chunk.replace(count,'count')+'\n            Return names'
 call=method(helper,'count As Integer','String()',portable)
 for name in names:
  update(NP,name,lambda old,chunk=chunk,call=call,count=count:old.replace(chunk,'            Dim names() As String = '+call+'('+count+')',1))

for parent,helper,mdl_type,input_type in [('KW_MCP','BuildKruskalMcpTables','KruskallWalis','Double()()'),('FRIEDMAN_MCP','BuildFriedmanMcpTables','Friedman','Double(,)')]:
 old=member((ROOT/NP).read_text(encoding='utf-8-sig'),parent)
 start=old.index('                Dim test = New '+mdl_type)
 end=old.index('                Dim tables = test.wrapResults()',start)+len('                Dim tables = test.wrapResults()')
 chunk=old[start:end].replace('                Dim tables = test.wrapResults()','            Return test.wrapResults()')
 arg='data()() As Double' if parent=='KW_MCP' else 'mat(,) As Double'
 call=method(helper,arg+', names() As String, alphaValue As Double','List(Of Global.BESHStatNG.ResultTable)',chunk)
 value='data' if parent=='KW_MCP' else 'mat'
 update(NP,parent,lambda old,chunk=old[start:end],call=call,value=value:old.replace(chunk,'                Dim tables = '+call+'('+value+', names, alphaValue)',1))

header='''Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.equivalencetests
Imports BESHStatNG.nonparametric

Namespace ExcelCommon
    '''+"''' <summary>Shared legacy inference preparation; statistical engines and numeric semantics remain in Core.</summary>"+'''
    Public NotInheritable Class UdfInferencePreparation38
        Private Sub New()
        End Sub

'''
with (ROOT/NEW).open('x',encoding='utf-8',newline='\n') as f:f.write(header+'\n\n'.join(bodies)+'\n    End Class\nEnd Namespace\n')
save('inference-source-composition-v1.json',dict(native_transitions=records,portable_bodies=bodies,new_source=pin(ROOT/NEW),
    unchanged_Core_engines=True,original_queries=0,
    ordering='All native import/default/alpha/selector-conversion/fit/catch/formatting positions retained; no new engine or host callback invoking archival sources. Public signatures/attributes untouched.'))
print('Repeated/nested ANOVA, t-test report bodies and shared rank-test preparation extracted')
