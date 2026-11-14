"""Extract lazy normalized numeric import preparation, preserving rejected cells."""
import re,textwrap
from preflight import ROOT,save,pin

path='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb'
p=ROOT/path;data=p.read_bytes();t=data.decode('utf-8-sig');edits=[];methods=[];maps=[]
names=['TryExtractNumericGroupsFromMatrix','ExtractCompleteNumericMatrixCompleteCases','TryReadCompleteNumericMatrixWithHeaders']
for name in names:
 m=re.search(r'^    Friend Function '+name+r'\b.*?^    End Function',t,re.M|re.S);assert m
 original=m[0];header_match=re.search(r'\) As Boolean(?=\r?\n)',original);assert header_match;header_end=header_match.end();body=original[header_end:original.rfind('    End Function')]
 if name=='TryExtractNumericGroupsFromMatrix':
  a=body.index('        If mat Is Nothing Then Return False');b=body.index('        Dim groupList',a)
  body=body[:a]+'''        If rows < 1 OrElse cols < 1 Then Return False

'''+body[b:]
  signature='rows As Integer, cols As Integer, startRow As Integer, hasHeader As Boolean, defaultPrefix As String, numeric As Func(Of Integer, Integer, Double?), text As Func(Of Integer, Integer, String), ByRef groups()() As Double, ByRef names() As String'
  body=body.replace('TryGetDouble(mat(r, c))','numeric(r, c)').replace('CellToTrimmedText(mat(0, c))','text(0, c)')
  native='''        groups = Nothing
        names = Nothing
        If mat Is Nothing Then Return False
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryExtractNumericGroupsFromMatrix(
            mat.GetLength(0), mat.GetLength(1), startRow, hasHeader, defaultPrefix,
            Function(row, column) TryGetDouble(mat(row, column)), Function(row, column) CellToTrimmedText(mat(row, column)), groups, names)
'''
 else:
  a=body.index('        Dim arr As Object(,) = Get2D(input)');b=body.index('        Dim rows As New List' if name=='ExtractCompleteNumericMatrixCompleteCases' else '        Dim numericCols',a)
  # Original missing/shape guards remain at the native acquisition boundary.
  body=body[:a]+body[b:]
  if name=='ExtractCompleteNumericMatrixCompleteCases':
   signature='r As Integer, c As Integer, numeric As Func(Of Integer, Integer, Double?), ByRef mat As Double(,), ByRef noRows As Integer, ByRef noCols As Integer'
   body=body.replace('TryGetDouble(arr(i, j))','numeric(i, j)')
   native='''        mat = Nothing
        noRows = 0
        noCols = 0
        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False
        Dim r As Integer = arr.GetLength(0)
        Dim c As Integer = arr.GetLength(1)
        If c < 2 Then Return False
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.ExtractCompleteNumericMatrixCompleteCases(
            r, c, Function(row, column) TryGetDouble(arr(row, column)), mat, noRows, noCols)
'''
  else:
   signature='rows As Integer, cols As Integer, numeric As Func(Of Integer, Integer, Double?), text As Func(Of Integer, Integer, String), ByRef mat As Double(,), ByRef names() As String'
   body=body.replace('LooksLikeHeaderRow(arr, numericCols)','UdfRangePreparation38.LooksLikeHeaderRow(rows, numericCols, Function(row, column) numeric(row, column).HasValue)')
   body=body.replace('Convert.ToString(arr(0, c)).Trim()','text(0, c)').replace('TryGetDouble(arr(r, c))','numeric(r, c)')
   native='''        mat = Nothing
        names = Nothing
        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False
        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows < 1 OrElse cols < 2 Then Return False
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryReadCompleteNumericMatrixWithHeaders(
            rows, cols, Function(row, column) TryGetDouble(arr(row, column)),
            Function(row, column) Convert.ToString(arr(row, column)).Trim(), mat, names)
'''
 assert not any(x in body for x in ['As Object','arr(', 'mat(r, c) = arr', 'Excel']),name
 methods.append('        Public Shared Function '+name+'('+signature+') As Boolean'+body+'        End Function')
 eol='\r\n' if '\r\n' in t else '\n';replacement=original[:header_end]+eol+native.replace('\n',eol)+'    End Function'
 edits.append((m.start(),m.end(),replacement));maps.append(dict(path=path,name=name,portable='UdfNumericImportPreparation38.'+name,semantics='Lazy typed number/text access; no native objects; original filtering, header, order and shape rules'))
target=ROOT/'BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb';assert not target.exists()
source='Option Explicit On\nOption Strict On\n\nImports System\nImports System.Collections.Generic\nImports System.Globalization\nImports System.Linq\n\nNamespace ExcelCommon\n    Public NotInheritable Class UdfNumericImportPreparation38\n        Private Sub New()\n        End Sub\n\n'+'\n\n'.join(methods).replace('\r\n','\n')+'\n    End Class\nEnd Namespace\n'
target.write_text(source,encoding='utf-8',newline='\n')
for start,end,new in reversed(edits):t=t[:start]+new+t[end:]
p.write_bytes((b'\xef\xbb\xbf' if data.startswith(b'\xef\xbb\xbf') else b'')+t.encode('utf-8'))
save('numeric-import-preparation-transition-v1.json',dict(status='IMPLEMENTED_NOT_YET_VALIDATED',members=maps,common=pin(target.relative_to(ROOT).as_posix()),windows=pin(path)))
print('Extracted three normalized group/complete-case matrix preparations')
