"""Packet B: native-validated LMM row/design preparation and report composition."""
from common import *
import re,textwrap
assert json.loads((HERE/'entry-authentication-v1.json').read_bytes())['status']=='ENTRY_GATES_PASS'
assert json.loads((HERE/'packet-A-source-moves-v1.json').read_bytes())['unit_test_runs']==0
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes())
path='BESHstatNG/src/ExcelUDFs/Regression/LmmUDFs.vb'
before=next(r['live'] for r in plan['source_before'] if r['live']['path']==path)
assert pin(ROOT/path)==before
text=(ROOT/path).read_text(encoding='utf-8-sig')
moves=[];input_methods=[];report_methods=[]
ip='Global.BESHStatNG.ExcelCommon.UdfLmmInputPreparation38.'
rp='Global.BESHStatNG.ExcelCommon.UdfLmmReportPreparation38.'
def bounds(host):
 start=re.search(r'^        (?:Public|Private|Friend) (?:Function|Sub) '+host+r'\(',text,re.M).start()
 end=re.search(r'^        End (?:Function|Sub)\s*$',text[start:],re.M)
 return start,start+end.end()
def move(host,start,end,name,signature,call,aliases=(),suffix='Return out',role='report'):
 global text
 a,z=bounds(host);assert text[a:z].count(start)==1
 first=text.index(start,a);last=text.index(end,first);assert last<=z
 segment=text[first:last].rstrip();portable=segment
 for old,new in aliases:portable=portable.replace(old,new)
 portable=textwrap.dedent(portable)
 code='        Public Shared '+signature+'\n'+textwrap.indent(portable+('\n'+suffix if suffix else ''),'            ')+'\n        End '+('Sub' if signature.startswith('Sub ') else 'Function')+'\n'
 (input_methods if role=='input' else report_methods).append(code)
 text=text[:first]+call+'\n\n'+text[last:]
 moves.append(dict(kind='EXTRACTION',path=path,host=host,name=name,destination='UdfLmmInputPreparation38' if role=='input' else 'UdfLmmReportPreparation38',original_segment=segment,portable_segment=portable,portable_suffix=suffix,portable_signature=signature,native_call=call,aliases=[dict(before=a,after=b) for a,b in aliases]))

a,z=bounds('TryGetLmmAlignedInputs')
first=text.index('            Dim yList As New List(Of Double)()',a)
middle=text.index('            imported = New LmmImportedInputs With {',first)
last=text.index('        End Function',middle);assert last<=z
original_segment=text[first:last].rstrip();row_prefix=text[first:middle].rstrip()
aliases=[('ExcelArgNumeric.TryGetDouble(','readNumber('),('ExcelArgReaders.CellToTrimmedText(','readText(')]
portable=row_prefix
for old,new in aliases:portable=portable.replace(old,new)
portable=textwrap.dedent(portable)
projection='''prepared = New UdfLmmPreparedRows38 With {
    .Y = yList.ToArray(),
    .X = xValues,
    .Z = zValues,
    .Subject = subjectList.ToArray(),
    .Visit = finalVisit,
    .DroppedRows = dropped
}
Return True'''
signature='Function TryPrepareRows(yCol As Object(,), xRaw As Object(,), zRaw As Object(,), subjectCol As Object(,), visitCol As Object(,), zSupplied As Boolean, visitSupplied As Boolean, n As Integer, pX As Integer, pZ As Integer, readNumber As Func(Of Object, Double?), readText As Func(Of Object, String), ByRef prepared As UdfLmmPreparedRows38, ByRef errorMessage As String) As Boolean'
input_methods.append('        Public Shared '+signature+'\n            prepared = Nothing\n'+textwrap.indent(portable+'\n\n'+projection,'            ')+'\n        End Function\n')
call='''            Dim prepared As Global.BESHStatNG.ExcelCommon.UdfLmmPreparedRows38 = Nothing
            If Not Global.BESHStatNG.ExcelCommon.UdfLmmInputPreparation38.TryPrepareRows(
                yCol, xRaw, zRaw, subjectCol, visitCol, zSupplied, visitSupplied, n, pX, pZ,
                AddressOf ExcelArgNumeric.TryGetDouble, AddressOf ExcelArgReaders.CellToTrimmedText,
                prepared, errorMessage) Then Return False

            imported = New LmmImportedInputs With {
                .Y = prepared.Y,
                .X = prepared.X,
                .Z = prepared.Z,
                .Subject = prepared.Subject,
                .Visit = prepared.Visit,
                .InferredXNames = inferredXNames,
                .InferredZNames = If(zSupplied, inferredZNames, New String() {}),
                .InferredXAbsoluteLetters = xLetters,
                .InferredZAbsoluteLetters = zLetters,
                .DroppedRows = prepared.DroppedRows
            }
            Return True'''
text=text[:first]+call+'\n'+text[last:]
moves.append(dict(kind='TYPED_PREPARED_ROWS_AND_NATIVE_NAME_METADATA_COMPOSITE',path=path,host='TryGetLmmAlignedInputs',name='TryPrepareRows',destination='UdfLmmInputPreparation38',original_segment=original_segment,original_row_prefix=row_prefix,portable_segment=portable,portable_prefix='prepared = Nothing',portable_projection=projection,native_call=call,portable_signature=signature,aliases=[dict(before=a,after=b) for a,b in aliases],native_conversion_callbacks=['ExcelArgNumeric.TryGetDouble','ExcelArgReaders.CellToTrimmedText'],state='Native imported/error resets and all import/dimension/absolute-letter checks remain before this exact row-order phase. Prepared DTO is unpublished until success; native name/letter aliases retained.'))
move('TryBuildLmmExpandedDesign','            design = Nothing','        End Function',
 'TryBuildExpandedDesign','Function TryBuildExpandedDesign(raw As Double(,), rawNames As String(), formulaText As String, absoluteColumnLetters As String(), allowRelativeColumnLetters As Boolean, allowAbsoluteColumnLetters As Boolean, allowQuotedVariableNames As Boolean, role As String, fallbackPrefix As String, ByRef design As Double(,), ByRef designNames As String(), ByRef errorMessage As String) As Boolean',
 '            Return '+ip+'TryBuildExpandedDesign(raw, rawNames, formulaText, absoluteColumnLetters, allowRelativeColumnLetters, allowAbsoluteColumnLetters, allowQuotedVariableNames, role, fallbackPrefix, design, designNames, errorMessage)',
 aliases=[('NormalizeNameList(','LmmUDFsMetadata38.NormalizeNameList(')],suffix='',role='input')
move('EnsureLmmResultUsesHandleNames','            If h.FixedEffectNames IsNot Nothing AndAlso h.FixedEffectNames.Length > 0 Then','        End Sub',
 'ApplyHandleNames','Sub ApplyHandleNames(result As regression.MixedModelResult, fixedEffectNames As String(), randomEffectNames As String())',
 '            '+rp+'ApplyHandleNames(h.Result, h.FixedEffectNames, h.RandomEffectNames)',
 aliases=[('h.FixedEffectNames','fixedEffectNames'),('h.RandomEffectNames','randomEffectNames'),('h.Result','result')],suffix='')
move('LMM_FITTED','                Dim out(fitted.Length - 1 + If(hdr, 1, 0), 1) As Object','                Return out\n',
 'BuildFittedTable','Function BuildFittedTable(fitted As Double(), hdr As Boolean) As Object(,)',
 '                Dim out As Object(,) = '+rp+'BuildFittedTable(fitted, hdr)')
move('LMM_RESID','                Dim out(n - 1 + If(hdr, 1, 0), 2) As Object','                Return out\n',
 'BuildResidualTable','Function BuildResidualTable(fitted As Double(), resid As Double(), n As Integer, hdr As Boolean) As Object(,)',
 '                Dim out As Object(,) = '+rp+'BuildResidualTable(fitted, resid, n, hdr)')
old='                    Return If(Y Is Nothing, 0, Y.Length)'
assert text.count(old)==1
call='                    Return '+rp+'ObservationCount(Y)'
text=text.replace(old,call,1)
report_methods.append('        Public Shared Function ObservationCount(values As Double()) As Integer\n            Return If(values Is Nothing, 0, values.Length)\n        End Function\n')
moves.append(dict(kind='PROPERTY_GETTER_VALUE_PROJECTION',path=path,host='N/Get',name='ObservationCount',destination='UdfLmmReportPreparation38',original_segment=old,portable_segment='Return If(values Is Nothing, 0, values.Length)',portable_suffix='',native_call=call,aliases=[dict(before='Y',after='values')],constraint='Native imported Y auto-getter has no side effects; exact constructor/setter/writer inventory required before closure. N and Get remain distinct source identities bound to the actual get_N method.'))
header='Option Explicit On\nOption Strict On\nOption Infer On\n\nImports System\nImports System.Collections.Generic\n\nNamespace ExcelCommon\n'
dto='''    Public NotInheritable Class UdfLmmPreparedRows38
        Public Property Y As Double()
        Public Property X As Double(,)
        Public Property Z As Double(,)
        Public Property Subject As Object()
        Public Property Visit As Double()
        Public Property DroppedRows As Integer
    End Class

'''
new=[]
for role,owner,methods in [('Analysis','UdfLmmInputPreparation38',input_methods),('Results','UdfLmmReportPreparation38',report_methods)]:
 target=ROOT/('BESHStat.Excel.Common/src/'+role+'/'+owner+'.vb')
 content=header+(dto if role=='Analysis' else '')+'    Public NotInheritable Class '+owner+'\n        Private Sub New()\n        End Sub\n\n'+'\n'.join(methods)+'    End Class\nEnd Namespace\n'
 assert not re.search(r'\b(?:Microsoft\.VisualBasic|ExcelDna|WinForms|UBound|LBound)\b',content)
 with target.open('x',encoding='utf-8',newline='\n') as f:f.write(content)
 new.append(pin(target))
file=ROOT/path;raw=file.read_bytes();newline='\r\n' if b'\r\n' in raw else '\n'
file.write_bytes((b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+text.replace('\r\n','\n').replace('\n',newline).encode())
save('packet-B-source-moves-v1.json',dict(status='SUBSTANTIVE_PACKET_B_IMPLEMENTED_NOT_YET_CLOSED_OR_TESTED',moves=moves,sources=[pin(file)]+new,scientific_Core_changes=0,unit_test_runs=0,Release=DEFERRED,original_queries=0))
print('Packet B typed row/design/report segments',len(moves),'no scientific changes or test campaign',flush=True)
