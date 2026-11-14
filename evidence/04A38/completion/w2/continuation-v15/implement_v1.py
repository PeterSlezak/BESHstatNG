"""Source-bound report/input extraction; all moved statements retained verbatim or with recorded aliases."""
from common import *
import textwrap

plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes())
for row in plan['source_before']:
    assert pin(ROOT/row['live']['path'])==row['live']
assert json.loads((HERE/'entry-authentication-v2.json').read_bytes())['status']=='ENTRY_GATES_PASS'
cox='BESHstatNG/src/ExcelUDFs/Regression/CoxUDFs.vb'
gee='BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb'
sources={p:(ROOT/p).read_text(encoding='utf-8-sig') for p in [cox,gee]}
moves=[]
input_methods=[]
report_methods=[]


def extract(path,start,end,name,signature,call,aliases=(),suffix='Return out',destination='report'):
    text=sources[path]
    a=text.index(start); b=text.index(end,a)
    segment=text[a:b].rstrip()
    portable=segment
    for old,new in aliases:
        portable=portable.replace(old,new)
    portable=textwrap.dedent(portable)
    method='        Public Shared '+signature+'\n'+textwrap.indent(portable,'            ')+'\n'
    if suffix:method+='            '+suffix+'\n'
    method+='        End '+('Sub' if signature.startswith('Sub ') else 'Function')+'\n'
    (input_methods if destination=='input' else report_methods).append(method)
    indent=text[a:text.index('\n',a)]
    indentation=len(indent)-len(indent.lstrip())
    # The start marker includes the complete original indentation.
    replacement=call
    sources[path]=text[:a]+replacement+'\n\n'+text[b:]
    moves.append(dict(path=path,name=name,original_segment=segment,portable_segment=portable,
                      aliases=[dict(before=a,after=b) for a,b in aliases],native_call=replacement,
                      portable_signature=signature,portable_suffix=suffix,destination=destination))


extract(cox,'                Dim records As New List(Of survival.SurvivalRecord)(rowCount)',
        '                If records.Count < 3 Then', 'PrepareRecords',
        'Function PrepareRecords(imported As CoxPHData, rowCount As Integer, fitColCount As Integer, fitX As Double(,)) As List(Of survival.SurvivalRecord)',
        '                Dim records As List(Of survival.SurvivalRecord) = Global.BESHStatNG.ExcelCommon.UdfCoxInputPreparation38.PrepareRecords(imported, rowCount, fitColCount, fitX)',
        suffix='Return records',destination='input')

extract(cox,'            Dim rows As Integer = If(hdr, p + 1, p)',
        '            Return PreparePValueTableForUdf(out, 5)', 'BuildCoxSummaryTable',
        'Function BuildCoxSummaryTable(hdr As Boolean, p As Integer, varNames As String(), result As CoxResult, robust As Boolean, zCrit As Double, ciPct As String, expForDisplay As Func(Of Double, Object)) As Object(,)',
        '            Dim out As Object(,) = Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.BuildCoxSummaryTable(hdr, p, h.VarNames, h.Result, h.Robust, zCrit, ciPct, AddressOf ExpForDisplay)',
        aliases=[('h.VarNames','varNames'),('h.Result','result'),('h.Robust','robust'),('ExpForDisplay(','expForDisplay(')])

extract(cox,'                Dim nRows As Integer = If(hdr, 7, 5)',
        '                Return PreparePValueTableForUdf(out, 4)', 'BuildCoxTestsTable',
        'Function BuildCoxTestsTable(hdr As Boolean, p As Integer, result As CoxResult, lr As Object, lrP As Object, wald As Object, waldP As Object, unavailable As Object) As Object(,)',
        '                Dim out As Object(,) = Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.BuildCoxTestsTable(hdr, p, h.Result, lr, lrP, wald, waldP, ExcelError.ExcelErrorNA)',
        aliases=[('h.Result','result'),('ExcelError.ExcelErrorNA','unavailable')])

extract(cox,'                Dim out(keys.Count, width) As Object',
        '                Return out\n            Catch', 'BuildCoxResidualTable',
        'Function BuildCoxResidualTable(d As Dictionary(Of Integer, Double()), keys As List(Of Integer), width As Integer, varNames As String(), rt As ResidualType) As Object(,)',
        '                Dim out As Object(,) = Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.BuildCoxResidualTable(d, keys, width, h.VarNames, rt)',
        aliases=[('h.VarNames','varNames'),('ResidualLabel(rt)','CoxUDFsMetadata38.ResidualLabel(rt)')])

extract(cox,'                Dim cols As Integer = If(tp = "survival" OrElse tp = "cumhaz", 3, 4)',
        '                Dim r As Integer = 1', 'CreateCoxBaselineTable',
        'Function CreateCoxBaselineTable(total As Integer, tp As String) As Object(,)',
        '                Dim out As Object(,) = Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.CreateCoxBaselineTable(total, tp)')

extract(cox,'                    For i As Integer = 0 To arr.GetLength(0) - 1\n                        out(r, 0) = Convert.ToString(kv.Key)',
        '                Next\n                Return out', 'AppendCoxBaselineRows',
        'Sub AppendCoxBaselineRows(out As Object(,), arr As Double(,), key As Object, tp As String, ByRef r As Integer)',
        '                    Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.AppendCoxBaselineRows(out, arr, kv.Key, tp, r)',
        aliases=[('kv.Key','key')],suffix='')

extract(gee,'                Dim outRows As Integer = If(hdr, beta.Length + 1, beta.Length)',
        '                Return PreparePValueTableForUdf(out, 6)', 'BuildGeeSummaryTable',
        'Function BuildGeeSummaryTable(beta As Double(), se As Double(), names As String(), hdr As Boolean, alphaValue As Double) As Object(,)',
        '                Dim out As Object(,) = Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.BuildGeeSummaryTable(beta, se, names, hdr, alphaValue)')

extract(gee,'                Dim outRows As Integer = If(hdr, labels.Length + 2, labels.Length + 1)',
        '                out(r, 0) = "Computational time (s)"', 'BuildGeeTestsPrefix',
        'Function BuildGeeTestsPrefix(labels As String(), vals As Object(,), hdr As Boolean, ByRef r As Integer) As Object(,)',
        '                Dim r As Integer = 0\n                Dim out As Object(,) = Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.BuildGeeTestsPrefix(labels, vals, hdr, r)',
        aliases=[('Dim r As Integer = 0','r = 0')])

extract(gee,'                        For i As Integer = 0 To raw.Length - 1\n                            rows.Add(New Object() {raw(i), dev(i), pear(i), sdev(i), spear(i), work(i)})',
        '                        Return MaterializeRows(rows,', 'AppendGeeResidualRows',
        'Sub AppendGeeResidualRows(rows As List(Of Object()), raw As Double(), dev As Double(), pear As Double(), sdev As Double(), spear As Double(), work As Double())',
        '                        Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.AppendGeeResidualRows(rows, raw, dev, pear, sdev, spear, work)',suffix='')

extract(gee,'                Dim outRows As Integer = If(hdr, nRows + 1, nRows)',
        '                For i As Integer = 0 To nRows - 1\n                    Dim eta As Double', 'CreateGeePredictionTable',
        'Function CreateGeePredictionTable(nRows As Integer, hdr As Boolean, ByRef r0 As Integer) As Object(,)',
        '                Dim r0 As Integer = 0\n                Dim out As Object(,) = Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.CreateGeePredictionTable(nRows, hdr, r0)',
        aliases=[('Dim r0 As Integer = 0','r0 = 0')])

extract(gee,'            If rows Is Nothing Then Return ExcelError.ExcelErrorNA',
        '        End Function\n\n    End Module', 'MaterializeGeeRows',
        'Function MaterializeGeeRows(rows As List(Of Object()), includeHeader As Boolean, headers As String(), unavailable As Object) As Object',
        '            Return Global.BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38.MaterializeGeeRows(rows, includeHeader, headers, ExcelError.ExcelErrorNA)',
        aliases=[('ExcelError.ExcelErrorNA','unavailable')],suffix='')

header='Option Explicit On\nOption Strict On\nOption Infer On\n\nImports System\nImports System.Collections.Generic\nImports System.Globalization\n\nNamespace ExcelCommon\n'
new_sources=[('BESHStat.Excel.Common/src/Analysis/UdfCoxInputPreparation38.vb','UdfCoxInputPreparation38',input_methods),
             ('BESHStat.Excel.Common/src/Results/UdfCoxGeeReportPreparation38.vb','UdfCoxGeeReportPreparation38',report_methods)]
for path,owner,methods in new_sources:
    content=header+'    Public NotInheritable Class '+owner+'\n        Private Sub New()\n        End Sub\n\n'+'\n'.join(methods)+'    End Class\nEnd Namespace\n'
    with (ROOT/path).open('x',encoding='utf-8',newline='\n') as stream:stream.write(content)
for path,text in sources.items():write_source(path,text)
save('implementation-source-moves-v1.json',dict(moves=moves,
    sources=[pin(ROOT/path) for path in sources]+[pin(ROOT/p) for p,_,_ in new_sources],
    original_queries=0,notes='Every segment is derived from authenticated current preimages. Engine formulas are not cloned; original fit/session/import/exception/materialization paths are retained.'))
print(json.dumps(dict(moved_methods=len(moves),paths=list(sources)+[p for p,_,_ in new_sources])))
