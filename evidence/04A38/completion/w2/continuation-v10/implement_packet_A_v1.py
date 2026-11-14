"""Move complete existing typed Contingency phases; preserve native guard/conversion order."""
from common import *
import textwrap
path='BESHstatNG/src/ExcelUDFs/ContingencyTableUDFs.vb';portable='BESHStat.Excel.Common/src/Analysis/UdfContingencyPreparation38.vb'
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());before={r['live']['path']:r for r in plan['source_before']}
for p in [path,portable]:assert pin(ROOT/p)==before[p]['live']
source=(ROOT/path).read_text(encoding='utf-8-sig');common=(ROOT/portable).read_text(encoding='utf-8-sig');entries={r['name']:r for r in json.loads((HERE/'target-entry-bodies-v1.json').read_bytes())['rows']};rows=[];methods=[]
specs=[
 ('CT_MCNEMAR_EXACT','McNemarBody','Dim res = contingencytable.Liddell_McNemar','tab(,) As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)','tab, alphaValue, AddressOf IsFinite',True),
 ('CT_ODDS_RATIO','OddsRatioBody','Dim res = contingencytable.OddsRatio','tab(,) As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)','tab, alphaValue, AddressOf IsFinite',True),
 ('CT_RISK_RATIO','RiskRatioBody','Dim res As ConfidenceIntervalResult','tab(,) As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)','tab, alphaValue, AddressOf IsFinite',True),
 ('CT_TREND','TrendBody','Dim trendTable(,) As Integer','tab(,) As Integer, rows As Integer, cols As Integer, isFinite As Func(Of Double, Boolean)','tab, rows, cols, AddressOf IsFinite',True),
 ('CT_ORDINAL_ASSOC','OrdinalAssociationBody','Dim res = contingencytable.cTableORDINALassoc','tab(,) As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)','tab, alphaValue, AddressOf IsFinite',True),
 ('CT_MANTEL_HAENSZEL','MantelHaenszelBody','Dim res = contingencytable.MantelHaenszel','mat(,) As Double, alphaValue As Double, isFinite As Func(Of Double, Boolean)','mat, alphaValue, AddressOf IsFinite',True),
 ('CT_SINGLE_PROPORTION','SingleProportionBody','Dim ci As ConfidenceIntervalResult','x As Integer, n As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)','x, n, alphaValue, AddressOf IsFinite',True),
 ('CT_TWO_INDEPENDENT_PROPORTIONS','IndependentProportionsBody','Dim ci As ConfidenceIntervalResult','x1 As Integer, n1 As Integer, x2 As Integer, n2 As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)','x1, n1, x2, n2, alphaValue, AddressOf IsFinite',True),
 ('CT_PAIRED_PROPORTIONS','PairedProportionsBody','Dim ci As ConfidenceIntervalResult','n As Integer, only1 As Integer, only2 As Integer, both As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)','n, only1, only2, both, alphaValue, AddressOf IsFinite',True),
 ('CT_TWO_INDEPENDENT_PROPORTIONS_NI','ProportionsNonInferiorityBody','Dim result As equivalencetests.ProportionNonInferiorityResult','x0 As Integer, n0 As Integer, x1 As Integer, n1 As Integer, marginValue As Double, alphaValue As Double','x0, n0, x1, n1, marginValue, alphaValue',False),
 ('CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV','ProportionsEquivalenceBody','Dim result As equivalencetests.ProportionEquivalenceResult','x0 As Integer, n0 As Integer, x1 As Integer, n1 As Integer, lowerValue As Double, upperValue As Double, alphaValue As Double','x0, n0, x1, n1, lowerValue, upperValue, alphaValue',False)]
for name,api,start,signature,arguments,fault in specs:
    body=entries[name]['current_whole_body'].replace('\r\n','\n');assert source.count(body)==1
    begin=body.index('                '+start);end=body.index('                Return BuildResultTable(',begin);phase=body[begin:end];code=textwrap.dedent(phase).rstrip()
    code=code.replace('Return ExcelError.ExcelErrorNum','Return Nothing')
    code=code.replace('TransposeIntMatrix(tab)','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TransposeIntMatrix(tab)')
    code=code.replace('SafeCiLabel(', 'UdfOutputTablesReports38.SafeCiLabel(').replace('SafeCiText(', 'UdfOutputTablesReports38.SafeCiText(')
    assert not any(token in code for token in ['ExcelError','GetOptional','TryParseAlpha','TryGetWholeNumber'])
    method='        Public Shared Function '+api+'('+signature+') As Object(,)\n'+textwrap.indent(code,'            ')+'\n            Return body\n        End Function\n';methods.append(method)
    call='                Dim body As Object(,) = Global.BESHStatNG.ExcelCommon.UdfContingencyPreparation38.'+api+'('+arguments+')\n'
    if fault:call+='                If body Is Nothing Then Return ExcelError.ExcelErrorNum\n'
    call+='\n';changed=body[:begin]+call+body[end:];source=source.replace(body,changed,1)
    rows.append(dict(name=name,original_id=entries[name]['original_id'],entry_current_id=entries[name]['current_record']['symbol_id'],api=api,source_phase=phase,portable_body=code,consumer_arguments=arguments,returns_Nothing_for_original_Num_guard=fault,unchanged_native_prefix=body[:begin],unchanged_native_suffix=body[end:],qualification='Complete parent closure still requires transitive source/CIL/branch/inline/import/catch/output proof. Named original Core finite predicate is passed directly; no anonymous callback declaration.'))
assert common.count('    End Class')==1;common=common.replace('    End Class','\n'+'\n'.join(methods)+'    End Class')
write_source(path,source);write_source(portable,common)
save('packet-A-source-composition-v1.json',dict(rows=rows,after=[pin(ROOT/path),pin(ROOT/portable)],original_queries=0,Core_changes=0))
print('Eleven typed Contingency phases moved; native imports/conversions/guards/catches/export metadata preserved')
