from common import *
common='''    <TestMethod>
    Public Sub PreparedPhases38KeepCallerStateAndExistingApisKeepEntryReset()
        Dim value As Integer = 17
        Assert.IsFalse(UdfScalarChoice38.TryBinaryPrepared(2.25R, Nothing, value))
        Assert.AreEqual(17, value)
        Assert.IsFalse(UdfScalarChoice38.TryBinary(2.25R, Nothing, value))
        Assert.AreEqual(-1, value)
        value = 17
        Assert.IsFalse(UdfScalarChoice38.TryWholeNumberPrepared(2.25R, value))
        Assert.AreEqual(17, value)
        Assert.IsFalse(UdfScalarChoice38.TryWholeNumber(2.25R, value))
        Assert.AreEqual(0, value)
        value = 17
        Assert.IsFalse(UdfScalarChoice38.TryPositiveIntegerPrepared(2.25R, value, 1))
        Assert.AreEqual(17, value)
        Assert.IsFalse(UdfScalarChoice38.TryPositiveInteger(2.25R, False, value, 10, 1))
        Assert.AreEqual(10, value)
        Dim groups As New List(Of Double(,)) From {New Double(,) {{17.0R}}}
        Dim names As New List(Of String) From {"seed"}
        Dim seedGroups = groups
        Dim seedNames = names
        Assert.IsFalse(UdfNumericImportPreparation38.TryGetGroupedCompleteMatrixPrepared(Nothing, Nothing, Nothing, Nothing, Nothing, groups, names))
        Assert.AreSame(seedGroups, groups)
        Assert.AreSame(seedNames, names)
        Assert.IsFalse(UdfNumericImportPreparation38.TryGetGroupedCompleteMatrix(Nothing, Nothing, Nothing, Nothing, Nothing, groups, names))
        Assert.IsNull(groups)
        Assert.IsNull(names)
        Dim data As Object(,) = {{17}}
        Dim nestedNames As String() = {"seed"}
        Dim seedData = data
        Dim seedNested = nestedNames
        Assert.IsFalse(UdfNumericImportPreparation38.TryGetNestedThreeColumnDataPrepared(Nothing, Nothing, Nothing, data, nestedNames))
        Assert.AreSame(seedData, data)
        Assert.AreSame(seedNested, nestedNames)
        Assert.IsFalse(UdfNumericImportPreparation38.TryGetNestedThreeColumnData(Nothing, Nothing, Nothing, data, nestedNames))
        Assert.IsNull(data)
        Assert.IsNull(nestedNames)
    End Sub

'''
adapter='''    Private NotInheritable Class AliasBox38
        Public Value As Integer = 42
    End Class

    Private NotInheritable Class AliasNumber38
        Implements IConvertible
        Private ReadOnly output As AliasBox38
        Private ReadOnly number As Double
        Public Sub New(target As AliasBox38, input As Double)
            output = target
            number = input
        End Sub
        Public Function ToDouble(provider As IFormatProvider) As Double Implements IConvertible.ToDouble
            output.Value = 17
            Return number
        End Function
        Public Function GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode
            Return TypeCode.Double
        End Function
'''
for name,typ in [('Boolean','Boolean'),('Byte','Byte'),('Char','Char'),('DateTime','DateTime'),('Decimal','Decimal'),('Int16','Short'),('Int32','Integer'),('Int64','Long'),('SByte','SByte'),('Single','Single'),('String','String'),('UInt16','UShort'),('UInt32','UInteger'),('UInt64','ULong')]:
    adapter+='        Public Function To'+name+'(provider As IFormatProvider) As '+typ+' Implements IConvertible.To'+name+'\n            Throw New NotSupportedException()\n        End Function\n'
adapter+='''        Public Function ToType(conversionType As Type, provider As IFormatProvider) As Object Implements IConvertible.ToType
            Throw New NotSupportedException()
        End Function
    End Class

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub NativeScalar38PreservesAliasedConverterOutputOnFailedValidation(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim box As New AliasBox38()
            Assert.IsFalse(ExcelArgNumeric.TryGetBinary01(New AliasNumber38(box, 2.25R), box.Value))
            Assert.AreEqual(17, box.Value)
            box.Value = 42
            Assert.IsFalse(ExcelArgNumeric.TryGetWholeNumber(New AliasNumber38(box, 2.25R), box.Value))
            Assert.AreEqual(17, box.Value)
            box.Value = 42
            Assert.IsFalse(ExcelArgNumeric.TryGetWholeNumber(New AliasNumber38(box, Double.NaN), box.Value))
            Assert.AreEqual(17, box.Value)
            box.Value = 42
            Assert.IsFalse(ExcelArgNumeric.TryGetOptionalPositiveInteger(New AliasNumber38(box, 2.25R), box.Value))
            Assert.AreEqual(17, box.Value)
            box.Value = 42
            Assert.ThrowsException(Of OverflowException)(Sub() ExcelArgNumeric.TryGetBinary01(New AliasNumber38(box, Double.MaxValue), box.Value))
            Assert.AreEqual(17, box.Value)
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

'''
for path,snippet in [('BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb',common),('BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb',adapter)]:
    text=(ROOT/path).read_text(encoding='utf-8-sig');i=text.rindex('End Class');write_source(path,text[:i]+snippet+text[i:])
save('byref-tests-added.json',dict(scope='Source-backed reentrant converter alias side effects; prepared phases versus unchanged public reset contracts; both cultures; no archived-original execution',new_managed_cases=3))
