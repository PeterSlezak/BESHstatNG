"""Packet A: exact source-bound report/name extraction; native fit/science unchanged."""
from common import *
import re,textwrap
assert json.loads((HERE/'entry-authentication-v1.json').read_bytes())['status']=='ENTRY_GATES_PASS'
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes())
for row in plan['source_before']:assert pin(ROOT/row['live']['path'])==row['live']
paths=['BESHstatNG/src/ExcelUDFs/Regression/'+n+'.vb' for n in ['GeeUDFs','GlmNbUDFs','GlmUDFs','LinearModelUDFs']]
gee,nb,glm,lm=paths
sources={p:(ROOT/p).read_text(encoding='utf-8-sig') for p in paths}
owner='UdfRegressionFamilyReportPreparation38';prefix='Global.BESHStatNG.ExcelCommon.'+owner+'.'
moves=[];methods={}

def extent(path,name):
 text=sources[path];match=re.search(r'^        (?:Public|Private|Friend) (?:Function|Sub) '+re.escape(name)+r'\(',text,re.M)
 assert match is not None,name
 end=re.search(r'^        End (?:Function|Sub)\s*$',text[match.start():],re.M)
 assert end is not None
 return match.start(),match.start()+end.end()

def move(path,host,start,end,name,signature,call,aliases=(),suffix='Return out',destination=owner,emit=True):
 first,last=extent(path,host);text=sources[path];parent=text[first:last]
 assert parent.count(start)==1,(host,start)
 a=first+parent.index(start);b=text.index(end,a);assert b<=last
 segment=text[a:b].rstrip();portable=segment
 for before,after in aliases:portable=portable.replace(before,after)
 portable=textwrap.dedent(portable)
 phase=portable+('\n'+suffix if suffix else '')
 if emit:
  body='        Public Shared '+signature+'\n'+textwrap.indent(phase,'            ')+'\n        End '+('Sub' if signature.startswith('Sub ') else 'Function')+'\n'
  if name in methods:assert methods[name]==body,'Shared complete body differs '+name
  else:methods[name]=body
 sources[path]=text[:a]+call+'\n\n'+text[b:]
 moves.append(dict(path=path,host=host,name=name,destination=destination,original_segment=segment,portable_segment=portable,portable_suffix=suffix,portable_signature=signature,native_call=call,aliases=[dict(before=a,after=b) for a,b in aliases],emitted=emit))

move(gee,'GEE_LSMESTIMATE','                Dim hdr() As String = {"Label", "Estimate", "Std. Error", "Z", "P-value",','                Dim zCrit As Double',
 'CreateGeeLsmEstimateTable','Function CreateGeeLsmEstimateTable(labelCount As Integer, alphaValue As Double) As Object(,)',
 '                Dim out As Object(,) = '+prefix+'CreateGeeLsmEstimateTable(labels.Count, alphaValue)',aliases=[('labels.Count','labelCount')])
move(gee,'GEE_LSMESTIMATE','                    out(r + 1, 0) = labels(r)','                Next\n\n                Return PreparePValueTableForUdf(out, 5)',
 'AppendGeeLsmEstimateRow','Sub AppendGeeLsmEstimateRow(out As Object(,), r As Integer, label As String, estOut As Double, seOut As Double, z As Double, pValue As Double, loOut As Double, hiOut As Double, scaleLabel As String, covName As String, safeNumber As Func(Of Double, Object))',
 '                    '+prefix+'AppendGeeLsmEstimateRow(out, r, labels(r), estOut, seOut, z, pValue, loOut, hiOut, scaleLabel, covName, AddressOf SafeExcelNumber)',
 aliases=[('labels(r)','label'),('SafeExcelNumber(','safeNumber(')],suffix='')
for path,host,alpha in [(nb,'GLMNB_SUMMARY','ciAlpha'),(glm,'GLM_SUMMARY','alphaValue')]:
 move(path,host,'                Dim outRows As Integer = If(hdr, beta.Length + 1, beta.Length)','                Return PreparePValueTableForUdf(out, 6)',
  'BuildGlmCoefficientTable','Function BuildGlmCoefficientTable(beta As Double(), se As Double(), names As String(), hdr As Boolean, alphaValue As Double, interceptIncluded As Boolean) As Object(,)',
  '                Dim out As Object(,) = '+prefix+'BuildGlmCoefficientTable(beta, se, names, hdr, '+alpha+', h.InterceptIncluded)',aliases=[(alpha,'alphaValue'),('h.InterceptIncluded','interceptIncluded')])
for path,host,end in [(nb,'GLMNB_TESTS','                Dim nbAlpha As Double = h.Model.NBalpha'),(glm,'GLM_TESTS','                If TypeOf h.Model.pFamily Is regression.Binomial Then\n                    out(r, 0)')]:
 move(path,host,'                Dim outRows As Integer = If(hdr, labels.Length + extraRows + 1, labels.Length + extraRows)',end,
  'BuildRegressionTestsPrefix','Function BuildRegressionTestsPrefix(labels As String(), vals As Object(,), hdr As Boolean, extraRows As Integer, ByRef r As Integer) As Object(,)',
  '                Dim r As Integer = 0\n                Dim out As Object(,) = '+prefix+'BuildRegressionTestsPrefix(labels, vals, hdr, extraRows, r)',aliases=[('Dim r As Integer = 0','r = 0')])
for path in [nb,glm]:
 move(path,'BuildParameterNames','            Dim predictorNames() As String = If(h.VarNames, New String() {})','        End Function',
  'CompleteRegressionParameterNames','Function CompleteRegressionParameterNames(out As String(), nextIndex As Integer, varNames As String()) As String()',
  '            Return '+prefix+'CompleteRegressionParameterNames(out, nextIndex, h.VarNames)',
  aliases=[('h.VarNames','varNames'),('UBound(out)','(out.Length - 1)')],suffix='')
for path,host in [(nb,'GLMNB_PRED'),(glm,'GLM_PRED')]:
 move(path,host,'                Dim outRows As Integer = If(hdr, nRows + 1, nRows)','                For i As Integer = 0 To nRows - 1\n                    Dim eta As Double',
  'CreateGeePredictionTable','Function CreateGeePredictionTable(nRows As Integer, hdr As Boolean, ByRef r0 As Integer) As Object(,)',
  '                Dim r0 As Integer = 0\n                Dim out As Object(,) = Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.CreateGeePredictionTable(nRows, hdr, r0)',
  aliases=[('Dim r0 As Integer = 0','r0 = 0')],destination='UdfCoxGeeReportPreparation38',emit=False)
move(glm,'BuildAllResidualOutput','            Dim n As Integer = model.pRaw_res.Length','        End Function',
 'BuildAllGlmResidualTable','Function BuildAllGlmResidualTable(raw As Double(), dev As Double(), pear As Double(), leverage As Double(), sdev As Double(), spear As Double(), cook As Double(), includeHeader As Boolean) As Object(,)',
 '            Return '+prefix+'BuildAllGlmResidualTable(model.pRaw_res, model.pDeviance_res, model.pPearsChisq_res, model.pLeverage, model.pStDeviance_res, model.pStPearsChisq_res, model.pCookDistance, includeHeader)',
 aliases=[('model.pRaw_res','raw'),('model.pDeviance_res','dev'),('model.pPearsChisq_res','pear'),('model.pLeverage','leverage'),('model.pStDeviance_res','sdev'),('model.pStPearsChisq_res','spear'),('model.pCookDistance','cook')],suffix='')
move(lm,'LM_SUMMARY','                Dim outRows As Integer = q + If(hdr, 1, 0) + noteCount','                AppendFootnotesInPlace(out, q + If(hdr, 1, 0), h.PredictorCodingFootnotes)',
 'BuildLmCoefficientTable','Function BuildLmCoefficientTable(model As regression.LinearModel, coef As Double(), se As Double(), dfResid As Double, q As Integer, hdr As Boolean, noteCount As Integer, alphaValue As Double) As Object(,)',
 '                Dim out As Object(,) = '+prefix+'BuildLmCoefficientTable(h.Model, coef, se, dfResid, q, hdr, noteCount, alphaValue)',aliases=[('h.Model','model')])
move(lm,'LM_TESTS','                Dim outRows As Integer = n + If(hdr, 1, 0) + noteCount','                AppendFootnotesInPlace(out, n + If(hdr, 1, 0), h.PredictorCodingFootnotes)',
 'BuildLmTestsTable','Function BuildLmTestsTable(labels As String(), vals As Object(,), n As Integer, hdr As Boolean, noteCount As Integer) As Object(,)',
 '                Dim out As Object(,) = '+prefix+'BuildLmTestsTable(labels, vals, n, hdr, noteCount)')
for name,signature,call,start in [
 ('DropTopRows','Function DropTopRows(arr As Object(,), rowsToDrop As Integer) As Object(,)','Return '+prefix+'DropTopRows(arr, rowsToDrop)','            If arr Is Nothing Then Return Nothing'),
 ('AppendFootnotes','Function AppendFootnotes(arr As Object(,), notes As String()) As Object(,)','Return '+prefix+'AppendFootnotes(arr, notes)','            If arr Is Nothing Then Return Nothing'),
 ('AppendFootnotesInPlace','Sub AppendFootnotesInPlace(arr As Object(,), startRow As Integer, notes As String())',prefix+'AppendFootnotesInPlace(arr, startRow, notes)','            If arr Is Nothing OrElse notes Is Nothing Then Exit Sub'),
]:
 move(lm,name,start,'        End '+('Sub' if signature.startswith('Sub ') else 'Function'),name,signature,'            '+call,suffix='')
move(lm,'LM_RESID','                Dim startRow As Integer = If(hdr, 0, 1)','                Return PrepareResultTableForUdf(out)',
 'BuildLmResidualColumn','Function BuildLmResidualColumn(fullTable As Object(,), hdr As Boolean, colIndex As Integer, colName As String) As Object(,)',
 '                Dim out As Object(,) = '+prefix+'BuildLmResidualColumn(fullTable, hdr, colIndex, colName)')
move(lm,'LM_PRED','                Dim outRows As Integer = If(hdr, nRows + 1, nRows)','                For i As Integer = 0 To nRows - 1\n                    Dim pred As Double',
 'CreateLmPredictionTable','Function CreateLmPredictionTable(nRows As Integer, hdr As Boolean, ByRef r0 As Integer) As Object(,)',
 '                Dim r0 As Integer = 0\n                Dim out As Object(,) = '+prefix+'CreateLmPredictionTable(nRows, hdr, r0)',aliases=[('Dim r0 As Integer = 0','r0 = 0')])
target=ROOT/'BESHStat.Excel.Common/src/Results/UdfRegressionFamilyReportPreparation38.vb'
header='Option Explicit On\nOption Strict On\nOption Infer On\n\nImports System\nImports System.Globalization\n\nNamespace ExcelCommon\n'
content=header+'    Public NotInheritable Class '+owner+'\n        Private Sub New()\n        End Sub\n\n'+'\n'.join(methods.values())+'    End Class\nEnd Namespace\n'
assert not re.search(r'\b(?:UBound|LBound|Microsoft\.VisualBasic|ExcelDna|WinForms)\b',content)
with target.open('x',encoding='utf-8',newline='\n') as f:f.write(content)
for path,text in sources.items():
 file=ROOT/path;before=file.read_bytes();newline='\r\n' if b'\r\n' in before else '\n'
 file.write_bytes((b'\xef\xbb\xbf' if before.startswith(b'\xef\xbb\xbf') else b'')+text.replace('\r\n','\n').replace('\n',newline).encode())
save('packet-A-source-moves-v1.json',dict(status='SUBSTANTIVE_PACKET_A_IMPLEMENTED_NOT_YET_CLOSED_OR_TESTED',moves=moves,sources=[pin(ROOT/p) for p in paths]+[pin(target)],new_portable_methods=len(methods),scientific_Core_changes=0,unit_test_runs=0,Release=DEFERRED,original_queries=0))
print('Packet A source-bound segments',len(moves),'new shared methods',len(methods),'no test campaign',flush=True)
