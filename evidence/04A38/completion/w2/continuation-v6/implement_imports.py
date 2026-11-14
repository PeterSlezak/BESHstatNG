"""Exact ordered native/portable decomposition; preserve entry resets in host."""
from common import *
import re
NUM='BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb'
READ='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb'
DATA='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb'
OPTIONS='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportOptions.vb'
def member(source,name):
 m=re.search(r'^([ \t]*)(?:Friend|Public|Private)(?: Shared)? Function '+name+r'\b',source,re.M);assert m,name
 end=source.index('\n'+m[1]+'End Function',m.end())+len('\n'+m[1]+'End Function')
 return source[m.start():end]
trans=[]
def change(path,name,new):
 text=(ROOT/path).read_text(encoding='utf-8-sig');old=member(text,name);assert old!=new
 write_source(path,text.replace(old,new,1));trans.append(dict(path=path,name=name,before_body=old,after_body=new))
source=(ROOT/NUM).read_text(encoding='utf-8-sig');appended=[]
for name,reset,signature_args in [
 ('TryReadCompleteNumericMatrixWithHeaders','        mat = Nothing\n        names = Nothing','rows, cols, numeric, text, mat, names'),
 ('ExtractCompleteNumericMatrixCompleteCases','        mat = Nothing\n        noRows = 0\n        noCols = 0','r, c, numeric, mat, noRows, noCols')]:
 old=member(source,name);prepared=old.replace('Function '+name+'(', 'Function '+name+'Prepared(',1)
 assert prepared.count(reset)==1
 prepared=prepared.replace(reset,'',1);appended.append(prepared)
 header=old[:old.index(reset)]
 new=header+reset+'\n            Return '+name+'Prepared('+signature_args+')\n        End Function'
 source=source.replace(old,new,1)
 trans.append(dict(path=NUM,name=name,before_body=old,after_body=new,prepared=prepared,
    contract='Existing public API resets preserved. Native consumer resets before acquisition and calls prepared phase without resetting after reentry.'))

read=(ROOT/READ).read_text(encoding='utf-8-sig')
ind=member(read,'TryReadIndependentNumericColumns')
loop=ind[ind.index('        Dim gx As New List'):ind.rindex('\n    End Function')]
loop=loop.replace('startRowX To ax.GetLength(0) - 1','startRowX To rowsX - 1').replace('startRowY To ay.GetLength(0) - 1','startRowY To rowsY - 1')
loop=loop.replace('TryGetDouble(ax(r, 0))','numericX(r)').replace('TryGetDouble(ay(r, 0))','numericY(r)')
loop=loop.replace('Convert.ToString(ax(0, 0)).Trim()','textX()').replace('Convert.ToString(ay(0, 0)).Trim()','textY()')
appended.append('''        Public Shared Function TryReadIndependentNumericColumnsPrepared(rowsX As Integer, rowsY As Integer,
            startRowX As Integer, startRowY As Integer, hasHeaderX As Boolean, hasHeaderY As Boolean,
            numericX As Func(Of Integer, Double?), numericY As Func(Of Integer, Double?),
            textX As Func(Of String), textY As Func(Of String), ByRef groups()() As Double, ByRef names() As String) As Boolean
'''+loop+'''
        End Function''')
change(READ,'TryReadIndependentNumericColumns',ind[:ind.index('        Dim gx As New List')]+'''        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryReadIndependentNumericColumnsPrepared(
            ax.GetLength(0), ay.GetLength(0), startRowX, startRowY, hasHeaderX, hasHeaderY,
            Function(row) TryGetDouble(ax(row, 0)), Function(row) TryGetDouble(ay(row, 0)),
            Function() Convert.ToString(ax(0, 0)).Trim(), Function() Convert.ToString(ay(0, 0)).Trim(), groups, names)
    End Function''')
paired=member(read,'TryReadPairedNumericColumns')
start=paired.index('        mat = New Double(pairs.Count - 1, 1) {}');end=paired.index('        Return True',start)
change(READ,'TryReadPairedNumericColumns',paired[:start]+'''        mat = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.RowsToMatrix(pairs, 2)
'''+paired[end:])
for name in ['TryReadCompleteNumericMatrixWithHeaders','ExtractCompleteNumericMatrixCompleteCases']:
 text=(ROOT/READ).read_text(encoding='utf-8-sig')
 # Native complete-cases function has a distinct reader name.
 oldcall='UdfNumericImportPreparation38.'+name+'('
 assert text.count(oldcall)==1
 write_source(READ,text.replace(oldcall,'UdfNumericImportPreparation38.'+name+'Prepared(',1))

single=member((ROOT/DATA).read_text(encoding='utf-8-sig'),'TryGetSingleNumericColumn')
start=single.index('        Dim list As New List');end=single.index('        Return True',start)
loop=single[start:end].replace('arr.GetLength(0)','rows').replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(r, 0))','numeric(r)').replace('        values = list.ToArray()','        Return list.ToArray()')
appended.append('''        Public Shared Function ReadFiniteColumn(rows As Integer, startRow As Integer, numeric As Func(Of Integer, Double?)) As Double()
'''+loop+'''        End Function''')
change(DATA,'TryGetSingleNumericColumn',single[:start]+'''        values = Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.ReadFiniteColumn(
            arr.GetLength(0), startRow, Function(row) Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(row, 0)))
'''+single[end:])

prob=member((ROOT/OPTIONS).read_text(encoding='utf-8-sig'),'TryGetOptionalProbabilityThresholds')
start=prob.index('        If rows <> 1 AndAlso cols <> 1 Then Return False')
tail=prob[start:prob.rindex('\n    End Function')]
tail=tail.replace('TryAppendProbabilityThreshold(arr(0, j), values)','appendCell(0, j, values)').replace('TryAppendProbabilityThreshold(arr(i, 0), values)','appendCell(i, 0, values)')
appended.append('''        Public Shared Function TryReadProbabilityThresholdsPrepared(rows As Integer, cols As Integer,
            appendCell As Func(Of Integer, Integer, List(Of Double), Boolean), ByRef thresholds() As Double) As Boolean
'''+tail+'''
        End Function''')
change(OPTIONS,'TryGetOptionalProbabilityThresholds',prob[:start]+'''        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryReadProbabilityThresholdsPrepared(
            rows, cols, Function(row, column, values) TryAppendProbabilityThreshold(arr(row, column), values), thresholds)
    End Function''')
assert source.count('    End Class')==1
source=source.replace('    End Class','\n'+'\n\n'.join(appended)+'\n    End Class')
write_source(NUM,source)
save('import-source-composition-v1.json',dict(transitions=trans,new_portable_bodies=appended,
    source_preimages=pin(HERE/'implementation-plan.json'),original_queries=0,
    boundary='Native acquisition/sentinels/finite conversion/text conversion/physical pair alignment/Err remain host-owned. Indexed callbacks avoid another Object forwarding copy. All native output resets remain before acquisition.'))
print('Shared independent/paired/single/complete-matrix/probability import slice implemented')
