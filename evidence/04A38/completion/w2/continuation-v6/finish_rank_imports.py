"""Complete rank-test import consumption using the canonical collection helpers."""
from common import *
import re
assert json.loads((HERE/'compatibility-families-v1.json').read_bytes())['all_exact']
paths=['BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb']
rows=[]
for path in paths:
 dst=HERE/'intermediate-preimages/rank-imports'/(path+'.txt');dst.parent.mkdir(parents=True,exist_ok=True)
 with dst.open('xb') as f:f.write((ROOT/path).read_bytes())
 rows.append(dict(live=pin(ROOT/path),retained=pin(dst)))
numeric=(ROOT/paths[0]).read_text(encoding='utf-8-sig')
old='Public Shared Function ReadFiniteColumn(rows As Integer, startRow As Integer, numeric As Func(Of Integer, Double?)) As Double()\n        Dim list As New List(Of Double)()'
new='''Public Shared Function ReadFiniteColumn(rows As Integer, startRow As Integer, numeric As Func(Of Integer, Double?),
            Optional initialCapacity As Integer? = Nothing) As Double()
        Dim list As List(Of Double) = If(initialCapacity.HasValue, New List(Of Double)(initialCapacity.Value), New List(Of Double)())'''
assert numeric.count(old)==1;write_source(paths[0],numeric.replace(old,new,1))
native=(ROOT/paths[1]).read_text(encoding='utf-8-sig')
old='''        Dim values As New List(Of Double)(arr.GetLength(0))
        For r As Integer = 0 To arr.GetLength(0) - 1
            Dim d As Double? = TryGetDouble(arr(r, 0))
            If d.HasValue Then values.Add(d.Value)
        Next

        Return values.ToArray()'''
new='''        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.ReadFiniteColumn(
            arr.GetLength(0), 0, Function(row) TryGetDouble(arr(row, 0)), initialCapacity:=arr.GetLength(0))'''
assert native.count(old)==1;native=native.replace(old,new,1)
old='''        Dim out(pairs.Count - 1, 1) As Double
        For i As Integer = 0 To pairs.Count - 1
            out(i, 0) = pairs(i)(0)
            out(i, 1) = pairs(i)(1)
        Next

        Return out'''
new='''        Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.RowsToMatrix(pairs, 2)'''
assert native.count(old)==1;native=native.replace(old,new,1);write_source(paths[1],native)
save('rank-import-source-composition.json',dict(preimages=rows,
    changes=['ReadNumericColumnIgnoringNonNumeric consumes the C6 ReadFiniteColumn helper with the original list capacity',
             'ExtractPairedNumericColumnsIgnoringNonNumeric consumes existing Core RowsToMatrix(pairs,2)'],
    domain_proof='Native TryGetDouble returns only finite Double?; the shared helper finite checks add no callback or exception and cannot reject a native present value. Preserve native sentinel/coercion/error branches, order and original list capacity. Single-column reader retains default-capacity path. No caller arrays are copied or mutated by callbacks.',
    public_api_scope='ReadFiniteColumn was first introduced in this C6 execution; no v5/public UDF signature is changed. Earlier C6 signature/build receipts remain.',
    original_queries=0))
print('Single-column and paired rank-test collectors reuse Common/Core without duplicate loops')
