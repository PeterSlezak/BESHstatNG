"""Extract original lazy header/trim decisions; native cell coercion stays Windows.

Callbacks preserve original evaluation order and early exits. They expose only
normalized Boolean/numeric information, never an Excel or untyped cell object.
This is a post-edit transformation tool, not an oracle generator.
"""
from pathlib import Path
import re
from preflight import ROOT,save,pin

path='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb'
source=ROOT/path
data=source.read_bytes(); text=data.decode('utf-8-sig')
names=['FindLastNonBlankRow','HasNumericColumnHeader','HasBinaryColumnHeader','HasNumericMatrixHeader','LooksLikeHeaderRow','LooksLikeSingleColumnHeader']
methods=[]; edits=[]; mapping=[]
for name in names:
    match=re.search(r'^    Friend Function '+name+r'\b.*?^    End Function',text,re.M|re.S)
    assert match,name
    lines=match.group().splitlines(); body='\n'.join(lines[1:-1])
    # Preserve null/shape guard in Windows before normalized dimensions are read.
    guards=[]
    while body.lstrip().startswith('If arr Is Nothing') or body.lstrip().startswith('If arr.GetLength(1) <> 1'):
        lines2=body.splitlines(); guards.append(lines2.pop(0).strip()); body='\n'.join(lines2)
    if name=='LooksLikeHeaderRow':
        signature='rows As Integer, numericCols As Integer(), numeric As Func(Of Integer, Integer, Boolean)'
        arguments='arr.GetLength(0), numericCols, Function(row, column) TryGetDouble(arr(row, column)).HasValue'
        body=body.replace('        Dim rows As Integer = arr.GetLength(0)\n','')
    elif name=='LooksLikeSingleColumnHeader':
        signature='rows As Integer, numeric As Func(Of Integer, Integer, Boolean)'
        arguments='arr.GetLength(0), Function(row, column) TryGetDouble(arr(row, column)).HasValue'
        body=body.replace('        Dim rows As Integer = arr.GetLength(0)\n','')
    elif name=='FindLastNonBlankRow':
        signature='rows As Integer, columns As Integer, blank As Func(Of Integer, Integer, Boolean)'
        arguments='arr.GetLength(0), arr.GetLength(1), Function(row, column) IsBlankCell(arr(row, column))'
    elif name=='HasBinaryColumnHeader':
        signature='lastRow As Integer, binary As Func(Of Integer, Integer, Boolean)'
        arguments='lastRow, Function(row, column) IsBinaryCell38(arr(row, column))'
        body=body.replace('        Dim dummy As Integer\n','')
        body=re.sub(r'TryGetBinary01\(arr\((\d), (\d)\), dummy\)',r'binary(\1, \2)',body)
    else:
        signature='lastRow As Integer, columns As Integer, blank As Func(Of Integer, Integer, Boolean), numeric As Func(Of Integer, Integer, Boolean)'
        arguments='lastRow, arr.GetLength(1), Function(row, column) IsBlankCell(arr(row, column)), Function(row, column) TryGetDouble(arr(row, column)).HasValue'
        body=body.replace('        Dim cols As Integer = arr.GetLength(1)','        Dim cols As Integer = columns')
    body=body.replace('arr.GetLength(0)','rows').replace('arr.GetLength(1)','columns')
    body=re.sub(r'IsBlankCell\(arr\(([^,]+), ([^)]+)\)\)',r'blank(\1, \2)',body)
    body=re.sub(r'TryGetDouble\(arr\(([^,]+), ([^)]+)\)\)\.HasValue',r'numeric(\1, \2)',body)
    assert not any(s in body for s in ['arr(', 'As Object','Excel']),name
    methods.append('        Public Shared Function '+name+'('+signature+') As '+('Integer' if name=='FindLastNonBlankRow' else 'Boolean')+'\n'+body+'\n        End Function')
    call='Return Global.BESHStatNG.ExcelCommon.UdfRangePreparation38.'+name+'('+arguments+')'
    replacement=lines[0]+'\n'+''.join('        '+g+'\n' for g in guards)+'        '+call+'\n'+lines[-1]
    edits.append((match.start(),match.end(),replacement))
    mapping.append(dict(path=path,original_name=name,common='UdfRangePreparation38.'+name,host='Null/shape guards and lazy native cell coercion; exact evaluation order retained'))
target=ROOT/'BESHStat.Excel.Common/src/Analysis/UdfRangePreparation38.vb';assert not target.exists()
target.write_text('Option Explicit On\nOption Strict On\n\nImports System\n\nNamespace ExcelCommon\n    Public NotInheritable Class UdfRangePreparation38\n        Private Sub New()\n        End Sub\n\n'+ '\n\n'.join(methods)+'\n    End Class\nEnd Namespace\n',encoding='utf-8',newline='\n')
for start,end,replacement in reversed(edits):text=text[:start]+replacement+text[end:]
marker='Partial Friend Module UdfDataImport'
text=text.replace(marker,marker+'\n    Private Function IsBinaryCell38(value As Object) As Boolean\n        Dim result As Integer\n        Return TryGetBinary01(value, result)\n    End Function\n',1)
eol='\r\n' if b'\r\n' in data else '\n';text=text.replace('\r\n','\n').replace('\n',eol)
source.write_bytes((b'\xef\xbb\xbf' if data.startswith(b'\xef\xbb\xbf') else b'')+text.encode('utf-8'))
save('range-preparation-transition-v1.json',dict(status='IMPLEMENTED_NOT_YET_VALIDATED',members=mapping,common=pin(target.relative_to(ROOT).as_posix()),windows=pin(path)))
print('Extracted six lazy normalized range/header decisions')
