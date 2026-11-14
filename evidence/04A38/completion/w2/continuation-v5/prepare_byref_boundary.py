from common import *
import re
entry=json.loads((HERE/'entry.json').read_bytes())
paths=['BESHStat.Excel.Common/src/Analysis/UdfScalarChoice38.vb',
       'BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb',
       'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb']
for path in paths:
    dst=HERE/'source-before/byref-boundary'/(path+'.txt');dst.parent.mkdir(parents=True,exist_ok=True)
    dst.write_bytes((ROOT/path).read_bytes())
    entry['source_before'].append(dict(live=pin(ROOT/path),preimage=pin(dst)))
save('entry-v2.json',entry)
for path in ['BESHstatNG/src/ExcelUDFs/ExcelArgModules.vb','BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']:
    dst=HERE/'attempts/before-byref-boundary'/(path+'.txt');dst.parent.mkdir(parents=True,exist_ok=True);dst.write_bytes((ROOT/path).read_bytes())
def split_method(source,name,reset,wrapper,signature=None):
    match=re.search(r'^([ \t]*)Public Shared Function '+name+r'\b',source,re.M);assert match
    end=source.index('\n'+match[1]+'End Function',match.end())+len('\n'+match[1]+'End Function')
    original=source[match.start():end]
    prepared=original.replace('Function '+name+'(','Function '+name+'Prepared(',1)
    assert reset in prepared;prepared=prepared.replace(reset,'',1)
    if signature:
        prepared=signature
    return source[:match.start()]+wrapper.rstrip()+'\n\n'+prepared+source[end:]
path=paths[0];source=(ROOT/path).read_text(encoding='utf-8-sig')
source=split_method(source,'TryBinary','            result = -1\n','''        Public Shared Function TryBinary(value As Double?, logicalValue As Boolean?, ByRef result As Integer) As Boolean
            result = -1
            Return TryBinaryPrepared(value, logicalValue, result)
        End Function''')
source=split_method(source,'TryWholeNumber','            result = 0\n','''        Public Shared Function TryWholeNumber(value As Double?, ByRef result As Integer) As Boolean
            result = 0
            Return TryWholeNumberPrepared(value, result)
        End Function''')
source=split_method(source,'TryPositiveInteger','            result = defaultValue\n','''        Public Shared Function TryPositiveInteger(value As Double?, missing As Boolean, ByRef result As Integer,
                                                   defaultValue As Integer, minimum As Integer) As Boolean
            result = defaultValue
            If missing Then Return True
            If Not value.HasValue Then Return False
            Dim number As Double = value.Value
            If Double.IsNaN(number) OrElse Double.IsInfinity(number) Then Return False
            Return TryPositiveIntegerPrepared(number, result, minimum)
        End Function''',signature='''        Public Shared Function TryPositiveIntegerPrepared(number As Double, ByRef result As Integer, minimum As Integer) As Boolean
            Dim rounded As Double = Math.Round(number)
            If Math.Abs(number - rounded) > 0.0000001R Then Return False
            If rounded < minimum OrElse rounded > Integer.MaxValue Then Return False
            result = CInt(rounded)
            Return True
        End Function''')
write_source(path,source)
path=paths[1];source=(ROOT/path).read_text(encoding='utf-8-sig')
for name,first,second,signature,args in [
    ('TryGetGroupedCompleteMatrix','groupMatrices','groupNames','''dataArr As Object(,), groupArr As Object(,),
            header As Func(Of Object(,), Integer(), Boolean), labelText As Func(Of Object, String), numeric As Func(Of Object, Double?),
            ByRef groupMatrices As List(Of Double(,)), ByRef groupNames As List(Of String)''','dataArr, groupArr, header, labelText, numeric, groupMatrices, groupNames'),
    ('TryGetNestedThreeColumnData','data','names','''arr As Object(,), labelText As Func(Of Object, String),
            numeric As Func(Of Object, Double?), ByRef data As Object(,), ByRef names As String()''','arr, labelText, numeric, data, names')]:
    wrapper='        Public Shared Function '+name+'('+signature+') As Boolean\n            '+first+' = Nothing\n            '+second+' = Nothing\n            Return '+name+'Prepared('+args+')\n        End Function'
    source=split_method(source,name,'            '+first+' = Nothing\n            '+second+' = Nothing\n',wrapper)
write_source(path,source)
path='BESHstatNG/src/ExcelUDFs/ExcelArgModules.vb';source=(ROOT/path).read_text(encoding='utf-8-sig')
source=source.replace('UdfScalarChoice38.TryBinary(d, logicalValue, value)','UdfScalarChoice38.TryBinaryPrepared(d, logicalValue, value)')
source=source.replace('UdfScalarChoice38.TryWholeNumber(d, value)','UdfScalarChoice38.TryWholeNumberPrepared(d, value)')
source=source.replace('UdfScalarChoice38.TryPositiveInteger(d, False, value, defaultValue, minValue)','UdfScalarChoice38.TryPositiveIntegerPrepared(d, value, minValue)')
write_source(path,source)
path=paths[2];source=(ROOT/path).read_text(encoding='utf-8-sig')
source=source.replace('UdfNumericImportPreparation38.TryGetGroupedCompleteMatrix(','UdfNumericImportPreparation38.TryGetGroupedCompleteMatrixPrepared(')
source=source.replace('UdfNumericImportPreparation38.TryGetNestedThreeColumnData(','UdfNumericImportPreparation38.TryGetNestedThreeColumnDataPrepared(')
write_source(path,source)
save('byref-boundary-implementation.json',dict(status='SOURCE_BACKED_BYREF_TIMING_CORRECTION',new_sources=paths,
    defect='A custom IConvertible can mutate an aliased caller output during native conversion. Repeating a reset in Common after conversion erases an original observable side effect on failed validation. Acquisition is likewise outside the portable reset boundary.',
    correction='Existing public portable APIs retain entry resets; new prepared phases do not reset. Native adapters retain the original reset before conversion/acquisition and call prepared validation only.',
    original_queries=0,statistical_engine_changes='NONE',existing_portable_public_signatures='UNCHANGED',provisional_review='Earlier C5 source contracts claiming no intervening observable operation are retained as adverse review attempts; final contracts must supersede them.'))
