from common import *
tests=r'''
    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub IndependentImport38KeepsCallbackOrderAndPostGroupNameFailure(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim trace As New System.Collections.Generic.List(Of String)()
            Dim groups()() As Double = Nothing
            Dim names() As String = {"entry"}
            Dim numericX As Func(Of Integer, Double?) = Function(row)
                trace.Add("x" & row.ToString())
                names = {"callback"}
                Return If(row = 1, CType(2.0R, Double?), Nothing)
            End Function
            Dim numericY As Func(Of Integer, Double?) = Function(row)
                trace.Add("y" & row.ToString())
                Return row + 10.0R
            End Function
            Dim textX As Func(Of String) = Function()
                Assert.AreEqual(2, groups.Length)
                trace.Add("nameX")
                Throw New FormatException("name failed after values")
            End Function
            Assert.ThrowsException(Of FormatException)(Sub() UdfNumericImportPreparation38.TryReadIndependentNumericColumnsPrepared(
                3, 2, 1, 0, True, False, numericX, numericY, textX, Function() "unused", groups, names))
            CollectionAssert.AreEqual(New String() {"x1", "x2", "y0", "y1", "nameX"}, trace)
            CollectionAssert.AreEqual(New Double() {2.0R}, groups(0))
            CollectionAssert.AreEqual(New Double() {10.0R, 11.0R}, groups(1))
            CollectionAssert.AreEqual(New String() {"callback"}, names)
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

    <TestMethod>
    Public Sub Probability38DeduplicationComparesAdjacentSortedInputs()
        Dim input() As Double = {0.5R + 0.0000000000015R, 0.5R, 0.5R + 0.00000000000075R}
        Dim output() As Double = {99.0R}
        Assert.IsTrue(UdfNumericImportPreparation38.TryReadProbabilityThresholdsPrepared(1, input.Length,
            Function(row, column, values)
                values.Add(input(column))
                Return True
            End Function, output))
        CollectionAssert.AreEqual(New Double() {0.5R}, output)
        CollectionAssert.AreEqual(New Double() {0.5R + 0.0000000000015R, 0.5R, 0.5R + 0.00000000000075R}, input)
        Dim sentinel = New Double() {17.0R}
        output = sentinel
        Assert.IsFalse(UdfNumericImportPreparation38.TryReadProbabilityThresholdsPrepared(2, 2,
            Function(row, column, values) ThrowUnexpectedProbabilityCallback38(), output))
        Assert.AreSame(sentinel, output)
    End Sub

    Private Shared Function ThrowUnexpectedProbabilityCallback38() As Boolean
        Throw New InvalidOperationException("shape must reject before callback")
    End Function

    <TestMethod>
    Public Sub CompleteMatrix38PreparedPhaseRetainsAcquisitionReentryWhilePublicApiResets()
        Dim original As Double(,) = {{17.0R, 18.0R}}
        Dim matrix As Double(,) = original
        Dim rows As Integer = 17, columns As Integer = 18
        Assert.IsTrue(UdfNumericImportPreparation38.ExtractCompleteNumericMatrixCompleteCasesPrepared(
            0, 2, Function(row, column) CType(Nothing, Double?), matrix, rows, columns))
        Assert.AreSame(original, matrix)
        Assert.AreEqual(0, rows)
        Assert.AreEqual(2, columns)
        Assert.IsTrue(UdfNumericImportPreparation38.ExtractCompleteNumericMatrixCompleteCases(
            0, 2, Function(row, column) CType(Nothing, Double?), matrix, rows, columns))
        Assert.IsNull(matrix)
        matrix = original
        Dim names() As String = Nothing
        Assert.IsFalse(UdfNumericImportPreparation38.TryReadCompleteNumericMatrixWithHeadersPrepared(
            0, 2, Function(row, column) CType(Nothing, Double?), Function(row, column) "unused", matrix, names))
        Assert.AreSame(original, matrix)
        CollectionAssert.AreEqual(New String() {"Condition 1", "Condition 2"}, names)
        Assert.IsFalse(UdfNumericImportPreparation38.TryReadCompleteNumericMatrixWithHeaders(
            0, 2, Function(row, column) CType(Nothing, Double?), Function(row, column) "unused", matrix, names))
        Assert.IsNull(matrix)
    End Sub
'''
adapter=r'''
    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub IndependentAndPaired38KeepLowerBoundsSourceRowsAndOwnership(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim x = Array.CreateInstance(GetType(Object), {4, 1}, {4, 7})
            Dim y = Array.CreateInstance(GetType(Object), {4, 1}, {9, 2})
            For row As Integer = 0 To 3
                x.SetValue(New Object() {"x", 1.0R, Nothing, 3.0R}(row), 4 + row, 7)
                y.SetValue(New Object() {"y", 10.0R, 20.0R, 30.0R}(row), 9 + row, 2)
            Next
            Dim groups()() As Double = Nothing, names() As String = Nothing
            Assert.IsTrue(UdfDataImport.TryGetIndependentNumericColumns(x, y, groups, names))
            CollectionAssert.AreEqual(New String() {"x", "y"}, names)
            CollectionAssert.AreEqual(New Double() {1.0R, 3.0R}, groups(0))
            CollectionAssert.AreEqual(New Double() {10.0R, 20.0R, 30.0R}, groups(1))
            Dim pairs As Double(,) = Nothing
            Assert.IsTrue(UdfDataImport.TryGetPairedNumericColumns(x, y, pairs, names))
            Assert.AreEqual(2, pairs.GetLength(0))
            Assert.AreEqual(1.0R, pairs(0, 0))
            Assert.AreEqual(10.0R, pairs(0, 1))
            Assert.AreEqual(3.0R, pairs(1, 0))
            Assert.AreEqual(30.0R, pairs(1, 1))
            groups(0)(0) = 99.0R
            Assert.AreEqual(1.0R, x.GetValue(5, 7))
            Assert.IsNull(x.GetValue(6, 7))
            Dim thresholds() As Double = {99.0R}
            Assert.IsFalse(ExcelArgNumeric.TryGetOptionalThresholdVector(New Object(,) {{0.1R, ExcelError.ExcelErrorNA}}, thresholds))
            Assert.IsNull(thresholds)
            Assert.IsTrue(ExcelArgNumeric.TryGetOptionalThresholdVector(New Object(,) {{Nothing, 0.2R, 0.2R, 0.1R}}, thresholds))
            CollectionAssert.AreEqual(New Double() {0.1R, 0.2R}, thresholds)
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub
'''
for path,addition in [('BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb',tests),('BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb',adapter)]:
 text=(ROOT/path).read_text(encoding='utf-8-sig');assert text.count('\nEnd Class')==1
 write_source(path,text.replace('\nEnd Class','\n'+addition+'\nEnd Class'))
for name in ['build.py','focused.py','compatibility.py','selected_release.py']:
 src=(PREVIOUS/name).read_bytes()
 with (HERE/name).open('xb') as f:f.write(src)
print('Focused import boundary/culture/ByRef/row-identity regression tests added')
