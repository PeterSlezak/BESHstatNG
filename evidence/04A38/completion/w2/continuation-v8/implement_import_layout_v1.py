"""Move only ordered prepared array layout; retain all native acquisition/conversion/import."""
from common import *
import re,textwrap
path='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb'
source=(ROOT/path).read_text(encoding='utf-8-sig');after=source
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());assert pin(ROOT/path)==plan['source_before'][1]['live']
def method(name):
    start=source.index('    Friend Function '+name+'(');end=source.index('    End Function',start)+len('    End Function')
    return source[start:end]
rows=[];pieces=[]
def add(name,parameters,body):
    transformed=body.replace('Dim raw(rowCount - 1, totalCols - 1) As Object','ReDim raw(rowCount - 1, totalCols - 1)').replace('Dim names(totalCols - 1) As String','ReDim names(totalCols - 1)')
    pieces.append('        Public Shared Sub '+name+'('+', _\n            '.join(parameters)+')\n'+textwrap.indent(textwrap.dedent(transformed),'            ')+'        End Sub\n')
    return transformed
prefix='Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.'
glm=method('TryBuildGlmDataFromUdfArgs')
first=glm[glm.index('        Dim raw(rowCount - 1, totalCols - 1) As Object'):glm.index('        Dim nextCol As Integer')]
add('InitializeResponsePredictorsPrepared',['rowCount As Integer','totalCols As Integer','xCols As Integer','yName As String','predictorNames As String()','yCol As Object(,)','xMat As Object(,)','ByRef raw As Object(,)','ByRef names As String()'],first)
column='''        names(nextCol) = If(String.IsNullOrWhiteSpace(columnName), fallbackName, columnName)
        For i As Integer = 0 To rowCount - 1
            raw(i, nextCol) = column(i, 0)
        Next
'''
add('CopyNamedColumnPrepared',['raw As Object(,)','names As String()','rowCount As Integer','nextCol As Integer','columnName As String','fallbackName As String','column As Object(,)'],column)
for native in ['TryBuildGeeDataFromUdfArgs','TryBuildGlmDataFromUdfArgs']:
    original=method(native);changed=original;assert first in changed
    replacement='        Dim raw(,) As Object = Nothing\n        Dim names() As String = Nothing\n        '+prefix+'InitializeResponsePredictorsPrepared(rowCount, totalCols, xCols, yName, predictorNames, yCol, xMat, raw, names)\n\n'
    changed=changed.replace(first,replacement)
    splices=[dict(original=first,replacement=replacement,portable='InitializeResponsePredictorsPrepared')]
    for stem,label in [('time','Time'),('offset','Offset'),('weight','Weight')]:
        name=stem+'Name';col=stem+'Col'
        old='            names(nextCol) = If(String.IsNullOrWhiteSpace('+name+'), "'+label+'", '+name+')\n            For i As Integer = 0 To rowCount - 1\n                raw(i, nextCol) = '+col+'(i, 0)\n            Next\n'
        if old not in changed:continue
        new='            '+prefix+'CopyNamedColumnPrepared(raw, names, rowCount, nextCol, '+name+', "'+label+'", '+col+')\n'
        changed=changed.replace(old,new);splices.append(dict(original=old,replacement=new,portable='CopyNamedColumnPrepared',bindings=dict(columnName=name,fallbackName='"'+label+'"',column=col)))
    after=after.replace(original,changed);rows.append(dict(native=native,original=original,current=changed,splices=splices))
native='TryBuildPredictorDataFromUdfArgs';original=method(native)
first=original[original.index('        Dim raw(rowCount - 1, totalCols - 1) As Object'):original.index('        If hasOffsetArg Then\n            names(xCols)')]
add('InitializePredictorsPrepared',['rowCount As Integer','totalCols As Integer','xCols As Integer','expectedPredictorNames As String()','xMat As Object(,)','ByRef raw As Object(,)','ByRef names As String()'],first)
replacement='        Dim raw(,) As Object = Nothing\n        Dim names() As String = Nothing\n        '+prefix+'InitializePredictorsPrepared(rowCount, totalCols, xCols, expectedPredictorNames, xMat, raw, names)\n\n'
changed=original.replace(first,replacement)
old='            names(xCols) = If(String.IsNullOrWhiteSpace(offsetName), "Offset", offsetName)\n            For i As Integer = 0 To rowCount - 1\n                raw(i, xCols) = offsetCol(i, 0)\n            Next\n'
new='            '+prefix+'CopyNamedColumnPrepared(raw, names, rowCount, xCols, offsetName, "Offset", offsetCol)\n'
assert old in changed;changed=changed.replace(old,new);after=after.replace(original,changed)
rows.append(dict(native=native,original=original,current=changed,splices=[dict(original=first,replacement=replacement,portable='InitializePredictorsPrepared'),dict(original=old,replacement=new,portable='CopyNamedColumnPrepared',bindings=dict(nextCol='xCols',columnName='offsetName',fallbackName='"Offset"',column='offsetCol'))]))
native='TryBuildCoxDataFromUdfArgs';original=method(native)
layout=original[original.index('        Dim raw(rowCount - 1, totalCols - 1) As Object'):original.index('        Dim out As New CoxPHData')]
add('InitializeCoxPrepared',['rowCount As Integer','totalCols As Integer','xCols As Integer','timeName As String','statusName As String','strataName As String','predictorNames As String()','timeCol As Object(,)','statusCol As Object(,)','strataCol As Object(,)','xMat As Object(,)','hasStrata As Boolean','ByRef raw As Object(,)','ByRef names As String()'],layout)
replacement='        Dim raw(,) As Object = Nothing\n        Dim names() As String = Nothing\n        '+prefix+'InitializeCoxPrepared(rowCount, totalCols, xCols, timeName, statusName, strataName, predictorNames, timeCol, statusCol, strataCol, xMat, hasStrata, raw, names)\n\n'
changed=original.replace(layout,replacement);after=after.replace(original,changed)
rows.append(dict(native=native,original=original,current=changed,splices=[dict(original=layout,replacement=replacement,portable='InitializeCoxPrepared')]))
native='TryGetAlignedClusterIdColumnObject';original=method(native)
old='        ReDim col(expectedRows - 1, 0)\n        For i As Integer = 0 To expectedRows - 1\n            col(i, 0) = arr(startRow + i, 0)\n        Next\n'
new='        Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.CopyTrimmedColumnPrepared(arr, startRow, expectedRows, col)\n'
assert old in original;changed=original.replace(old,new);after=after.replace(original,changed)
rows.append(dict(native=native,original=original,current=changed,splices=[dict(original=old,replacement=new,portable='UdfNumericImportPreparation38.CopyTrimmedColumnPrepared')]))
newpath='BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb'
with (ROOT/newpath).open('x',encoding='utf-8',newline='\r\n') as f:f.write('Option Explicit On\nOption Strict On\n\nImports System\n\nNamespace ExcelCommon\n    Public NotInheritable Class UdfModelImportPreparation38\n        Private Sub New()\n        End Sub\n\n'+ '\n'.join(pieces)+'    End Class\nEnd Namespace\n')
write_source(path,after)
save('import-layout-extraction-v1.json',dict(rows=rows,current=pin(ROOT/path),portable=pin(ROOT/newpath),scope='Pure prepared allocation/copy only. Row acquisition/header/name/optional interpretation and GEE cluster conversion/dictionary code remain Windows. Original row-major paired Cox copy, native DataImportRawMatrix calls and ByRef data final assignment remain in original order.',original_queries=0))
print('Extracted prepared layouts from5 native parents; no native conversion or import moved')
