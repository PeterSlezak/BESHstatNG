Option Explicit On
Option Strict On

Imports System
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.WorksheetFunctions
Imports ExcelDna.Integration
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfInputBoundary39Tests
    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(-3)>
    Public Sub NonzeroBoundsCopyIsDetachedAndZeroBased(lower As Integer)
        Dim source = Array.CreateInstance(GetType(Object), {2, 2}, {lower, lower + 2})
        source.SetValue(1.25, lower, lower + 2)
        source.SetValue(2.5, lower, lower + 3)
        source.SetValue(3.75, lower + 1, lower + 2)
        source.SetValue(5.0, lower + 1, lower + 3)
        Dim callbacks As Integer
        Dim actual = WindowsSpreadsheetTransport.ReadUdfMatrix(source, Function(reference)
                                                                                         callbacks += 1
                                                                                         Throw New InvalidOperationException("No acquisition for an array")
                                                                                     End Function)
        Assert.AreEqual(0, callbacks)
        Assert.AreEqual(0, actual.GetLowerBound(0))
        Assert.AreEqual(0, actual.GetLowerBound(1))
        CollectionAssert.AreEqual(New Double() {1.25, 2.5, 3.75, 5.0}, actual.Cast(Of Object)().Select(Function(v) CDbl(v)).ToArray())
        actual(0, 0) = 99
        Assert.AreEqual(1.25, CDbl(source.GetValue(lower, lower + 2)))
    End Sub

    <TestMethod>
    Public Sub MissingAndErrorsHaveDistinctCoercionPrecedenceAndByRefState()
        Dim matrix As Object(,) = {{99}}, failure As ExcelError? = ExcelError.ExcelErrorNA
        Assert.IsTrue(UdfDataImport.TryCoerceToObjectMatrix(ExcelMissing.Value, False, matrix, failure))
        Assert.IsInstanceOfType(matrix(0, 0), GetType(ExcelEmpty))
        Assert.IsFalse(failure.HasValue)
        Assert.IsFalse(UdfDataImport.TryCoerceToObjectMatrix(ExcelError.ExcelErrorDiv0, True, matrix, failure))
        Assert.IsNull(matrix)
        Assert.AreEqual(ExcelError.ExcelErrorDiv0, failure.Value)
        Assert.IsFalse(UdfDataImport.TryCoerceToObjectMatrix(42.0, False, matrix, failure))
        Assert.IsNull(matrix)
        Assert.IsFalse(failure.HasValue)
        Assert.IsTrue(UdfDataImport.TryCoerceToObjectMatrix(42.0, True, matrix, failure))
        Assert.AreEqual(42.0, CDbl(matrix(0, 0)))
    End Sub

    <TestMethod>
    Public Sub RawArraysPairPositionallyWithoutIndependentCompactionOrSourceCoordinates()
        Dim rows = WindowsSpreadsheetTransport.ExtractNumericPairs(New Object(,) {{1}, {Nothing}, {3}, {4}}, New Object(,) {{10}, {20}, {Nothing}, {40}})
        Assert.AreEqual(2, rows.Count)
        CollectionAssert.AreEqual(New Double() {1, 10}, rows(0))
        CollectionAssert.AreEqual(New Double() {4, 40}, rows(1))
        Assert.IsTrue(WindowsSpreadsheetTransport.PairedArgumentsAligned(New Object(,) {{1}}, New Object(,) {{2}}))
        Dim frame = WindowsSpreadsheetTransport.Capture(New Object(,) {{1}, {2}})
        Assert.AreEqual(0, frame.SourceAt(0, 0).WorksheetRow)
        Assert.AreEqual(0, frame.SourceAt(0, 0).WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub ReferencesRequireSameSheetAndRowBoundsButNotSameColumn()
        Dim first As New ExcelReference(10, 12, 1, 1, New IntPtr(1))
        Assert.IsTrue(WindowsSpreadsheetTransport.PairedArgumentsAligned(first, New ExcelReference(10, 12, 5, 5, New IntPtr(1))))
        Assert.IsFalse(WindowsSpreadsheetTransport.PairedArgumentsAligned(first, New ExcelReference(11, 13, 5, 5, New IntPtr(1))))
        Assert.IsFalse(WindowsSpreadsheetTransport.PairedArgumentsAligned(first, New ExcelReference(10, 12, 5, 5, New IntPtr(2))))
        Assert.IsTrue(WindowsSpreadsheetTransport.PairedArgumentsAligned(first, New Object(,) {{2}}))
    End Sub

    <TestMethod>
    Public Sub AlphaDefaultCandidateAndScalarEligibilityRemainUncorrected()
        Assert.AreEqual(0.05, UdfDataImport.ParseAlphaOrDefault(ExcelMissing.Value, 0.2))
        Assert.ThrowsException(Of ArgumentException)(Sub() UdfDataImport.ParseAlphaOrDefault("bad", 0.2))
        Assert.IsFalse(ExcelArgNumeric.TryGetDouble("2.5").HasValue)
        Assert.IsFalse(ExcelArgNumeric.TryGetDouble(True).HasValue)
        Assert.IsFalse(ExcelArgNumeric.TryGetDouble(Double.NaN).HasValue)
        Dim number As Double = 77
        Assert.IsFalse(ExcelArgNumeric.TryGetFiniteDouble(ExcelError.ExcelErrorNA, number))
        Assert.IsTrue(Double.IsNaN(number))
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub NestedCaptureAndCoreBitsRemainSeparatelyObservable(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim number = 1.2345678901234567R
            Dim data As New DataObj
            data.DataImportRawMatrix(New Object(,) {{"G", "S", number}, {"G", "S", If(culture = "", "3.5", "3,5")},
                {"H", "T", 2.0}, {"H", Nothing, 999.0}}, {"Group", "Nested", "Y"}, firstSourceRow:=20, CharCols:=1)
            Dim errorText As String = "prior"
            Dim adapted = WindowsPairedInputAdapter.SelectNested(data, errorText)
            Assert.IsNotNull(adapted)
            Assert.AreEqual("prior", errorText)
            Dim captured = WindowsPairedInputAdapter.GetNestedEvidence(adapted)
            Dim core = WindowsPairedInputAdapter.GetNestedCoreInput(adapted)
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(number), BitConverter.DoubleToInt64Bits(captured.Responses.Blocks(0)(0).Value))
            Assert.AreEqual(Convert.ToInt64("3ff3c0ca428c5a0a", 16), BitConverter.DoubleToInt64Bits(core.Responses.Blocks(0)(0).Value))
            Assert.AreNotEqual(BitConverter.DoubleToInt64Bits(number), BitConverter.DoubleToInt64Bits(core.Responses.Blocks(0)(0).Value))
            Assert.AreEqual(20, captured.Responses.Blocks(0)(0).WorksheetRow)
            Assert.AreEqual(0, captured.Responses.Blocks(0)(0).WorksheetColumn)
            Assert.AreEqual(3, core.Responses.Blocks.Count)
            Assert.AreEqual(22, core.Responses.Blocks(2)(0).WorksheetRow)
            Assert.AreEqual(GetType(String), adapted.X(0, 2).GetType())
            Assert.IsNull(WindowsPairedInputAdapter.GetNestedEvidence(New MultiGroupsPairedDataObj With {.X = CType(adapted.X.Clone(), Object(,))}))
            CollectionAssert.AreEqual(data.FinalData.Cast(Of Object)().ToArray(), adapted.X.Cast(Of Object)().ToArray())
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub NestedExtremeFiniteLegacyConversionExceptionIsRetained(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Assert.ThrowsException(Of OverflowException)(Sub()
                                                            Dim data As New DataObj
                                                            data.DataImportRawMatrix(New Object(,) {{"G", "S", Double.MaxValue}, {"G", "S", "3"}}, {"Group", "Nested", "Y"}, CharCols:=1)
                                                            Dim errorText As String = ""
                                                            WindowsPairedInputAdapter.SelectNested(data, errorText)
                                                        End Sub)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub
End Class
