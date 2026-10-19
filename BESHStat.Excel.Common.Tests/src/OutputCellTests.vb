Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text.Json
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class OutputCellTests
    <TestMethod>
    Public Sub RawScalars_KeepBlankTextBooleanAndNumberDistinct()
        Assert.AreEqual(OutputCellKind.Blank, OutputCell.FromRaw(Nothing).Kind)
        For Each text As String In New String() {"", "#N/A", "#Pinf", "#Ninf", "=1+1", "=SUM(A1:A2)", "0.04"}
            Dim cell As OutputCell = OutputCell.FromRaw(text)
            Assert.AreEqual(OutputCellKind.Text, cell.Kind)
            Assert.AreEqual(text, cell.TextValue)
            Assert.IsNull(cell.ErrorKind)
            Assert.IsNull(cell.FormulaExpression)
        Next
        Assert.AreEqual(OutputCellKind.Number, OutputCell.FromRaw(0).Kind)
        Assert.AreEqual(OutputCellKind.BooleanValue, OutputCell.FromRaw(False).Kind)
        Assert.AreEqual(False, OutputCell.FromRaw(False).BooleanValue.Value)
    End Sub

    <TestMethod>
    Public Sub ExactNumbers_PreserveIntegerBoundariesAndDecimalPrecision()
        For Each raw As Object In New Object() {SByte.MinValue, Byte.MaxValue, Short.MinValue, UShort.MaxValue,
            Integer.MinValue, UInteger.MaxValue, Long.MinValue, Long.MaxValue, ULong.MaxValue,
            9007199254740993L, Decimal.MinValue, Decimal.MaxValue, 0.1234567890123456789012345678D}
            Dim cell As OutputCell = OutputCell.FromRaw(raw)
            Assert.AreEqual(OutputNumberKind.ExactDecimal, cell.NumberKind.Value)
            Assert.AreEqual(Convert.ToDecimal(raw, CultureInfo.InvariantCulture), cell.DecimalValue.Value)
            Assert.IsNull(cell.BinaryValue)
        Next
        Dim probability As Double
        Assert.IsFalse(OutputCell.DecimalCell(1.0000000000000000000000000001D).TryGetProbability(probability))
        Assert.IsFalse(OutputCell.DecimalCell(-0.0000000000000000000000000001D).TryGetProbability(probability))
        Assert.IsFalse(OutputCell.DecimalCell(Decimal.MaxValue).TryGetProbability(probability))
    End Sub

    <TestMethod>
    Public Sub BinaryNumbers_PreserveFiniteBoundariesAndSignedZero()
        For Each value As Double In New Double() {0, BitConverter.Int64BitsToDouble(Long.MinValue),
            Double.Epsilon, -Double.Epsilon, Double.MinValue, Double.MaxValue}
            Dim cell As OutputCell = OutputCell.FromRaw(value)
            Assert.AreEqual(OutputNumberKind.Binary64, cell.NumberKind.Value)
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(value), BitConverter.DoubleToInt64Bits(cell.BinaryValue.Value))
        Next
        For Each value As Single In New Single() {Single.MinValue, Single.MaxValue, Single.Epsilon, -0.0F}
            Assert.AreEqual(CDbl(value), OutputCell.FromRaw(value).BinaryValue.Value)
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(CDbl(value)), BitConverter.DoubleToInt64Bits(OutputCell.FromRaw(value).BinaryValue.Value))
        Next
    End Sub

    <TestMethod>
    Public Sub DecimalSignificance_DoesNotRoundAboveAlphaValuesIntoSignificance()
        Dim above As OutputCell = OutputCell.DecimalCell(0.050000000000000003D)
        Dim projection As Double
        Assert.IsTrue(above.TryGetProbability(projection))
        Assert.AreEqual(0.05, projection)
        Assert.IsFalse(above.IsSignificantAt(0.05))
        Assert.IsTrue(OutputCell.DecimalCell(0.05D).IsSignificantAt(0.05))
        Assert.IsTrue(OutputCell.DecimalCell(0D).IsSignificantAt(Double.Epsilon))
        Assert.IsFalse(OutputCell.DecimalCell(0.0000000000000000000000000001D).IsSignificantAt(Double.Epsilon))
        Assert.IsFalse(OutputCell.DecimalCell(1D).IsSignificantAt(0.9999999999999999))
        Assert.IsFalse(OutputCell.DecimalCell(Decimal.MaxValue).IsSignificantAt(0.05))
        Assert.IsFalse(OutputCell.BooleanCell(False).IsSignificantAt(0.05))
        Assert.IsFalse(OutputCell.SpreadsheetError("#N/A").IsSignificantAt(0.05))
        Assert.IsFalse(OutputCell.TrustedFormula("=0", "excel-a1").IsSignificantAt(0.05))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() above.IsSignificantAt(Double.NaN))
    End Sub

    <DataTestMethod>
    <DataRow("#NULL!", OutputErrorKind.NullIntersection, "spreadsheet.null")>
    <DataRow("#DIV/0!", OutputErrorKind.DivisionByZero, "spreadsheet.division-by-zero")>
    <DataRow("#VALUE!", OutputErrorKind.InvalidValue, "spreadsheet.value")>
    <DataRow("#REF!", OutputErrorKind.InvalidReference, "spreadsheet.reference")>
    <DataRow("#NAME?", OutputErrorKind.UnknownName, "spreadsheet.name")>
    <DataRow("#NUM!", OutputErrorKind.InvalidNumber, "spreadsheet.number")>
    <DataRow("#N/A", OutputErrorKind.NotAvailable, "spreadsheet.not-available")>
    <DataRow("#GETTING_DATA", OutputErrorKind.GettingData, "spreadsheet.getting-data")>
    <DataRow("#SPILL!", OutputErrorKind.Unknown, "spreadsheet.unknown")>
    <DataRow("", OutputErrorKind.Unknown, "spreadsheet.unknown")>
    Public Sub ExplicitErrors_MapOnlySupportedStandardTokens(token As String, kind As OutputErrorKind, code As String)
        Dim cell As OutputCell = OutputCell.SpreadsheetError(token)
        Assert.AreEqual(OutputCellKind.Error, cell.Kind)
        Assert.AreEqual(kind, cell.ErrorKind.Value)
        Assert.AreEqual(code, cell.ErrorCode)
        Assert.AreEqual(token, cell.OriginalError)
        Assert.IsNull(cell.NativeErrorCode)
        Dim supplied As OutputCell = OutputCell.SpreadsheetError(token, 2042, "Safe host detail")
        Assert.AreEqual(2042, supplied.NativeErrorCode.Value)
        Assert.AreEqual("Safe host detail", supplied.SafeErrorDetail)
        Assert.AreEqual(OutputCellKind.Text, OutputCell.FromRaw(token).Kind)
    End Sub

    <TestMethod>
    Public Sub NonfiniteValues_ArePortableErrorsWithoutInventedNativeCodes()
        Dim raw As Object() = {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity,
            Single.NaN, Single.PositiveInfinity, Single.NegativeInfinity}
        Dim codes As String() = {"number.nan", "number.positive-infinity", "number.negative-infinity"}
        For index As Integer = 0 To raw.Length - 1
            Dim cell As OutputCell = OutputCell.FromRaw(raw(index))
            Assert.AreEqual(OutputCellKind.Error, cell.Kind)
            Assert.AreEqual(OutputErrorKind.NonFinite, cell.ErrorKind.Value)
            Assert.AreEqual(codes(index Mod 3), cell.ErrorCode)
            Assert.IsNull(cell.NativeErrorCode)
            Assert.IsNull(cell.OriginalError)
            Assert.IsNull(cell.BinaryValue)
        Next
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputCell.NumberCell(Double.PositiveInfinity))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputCell.NumberCell(Double.NaN))
    End Sub

    <TestMethod>
    Public Sub UnsupportedInputs_RejectWithoutStringificationOrCoercion()
        For Each value As Object In New Object() {New NoStringConversion(), New Object(), New Integer() {1},
            DateTime.MinValue, DBNull.Value, DayOfWeek.Monday, "x"c, Guid.Empty, New List(Of Double)(), TimeSpan.Zero}
            Assert.ThrowsException(Of ArgumentException)(Function() OutputCell.FromRaw(value))
        Next
    End Sub

    <TestMethod>
    Public Sub FormulaIntent_IsExplicitAndSerializesAsDataOnly()
        Dim formula As OutputCell = OutputCell.TrustedFormula("=SUM(A1:A2)", "excel-a1")
        Assert.AreEqual(OutputCellKind.Formula, formula.Kind)
        Assert.IsNull(formula.TextValue)
        Using json As JsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(formula))
            Assert.AreEqual("=SUM(A1:A2)", json.RootElement.GetProperty("FormulaExpression").GetString())
            Assert.AreEqual("excel-a1", json.RootElement.GetProperty("FormulaLanguage").GetString())
            Assert.AreEqual(JsonValueKind.Null, json.RootElement.GetProperty("BinaryValue").ValueKind)
        End Using
        Assert.AreEqual(OutputCellKind.Text, OutputCell.FromRaw(formula.FormulaExpression).Kind)
        ' No grammar is guessed; an explicit dialect/intent can be non-Excel.
        Assert.AreEqual("sum(x)", OutputCell.TrustedFormula("sum(x)", "future-expression").FormulaExpression)
        Assert.ThrowsException(Of ArgumentException)(Function() OutputCell.TrustedFormula("", "excel-a1"))
        Assert.ThrowsException(Of ArgumentException)(Function() OutputCell.TrustedFormula("=1", ""))
        Assert.ThrowsException(Of ArgumentException)(Function() OutputCell.TrustedFormula("=1" & ChrW(0), "excel-a1"))
        Assert.ThrowsException(Of ArgumentNullException)(Function() OutputCell.TextCell(Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(Function() OutputCell.SpreadsheetError(Nothing))
    End Sub

    <TestMethod>
    Public Sub CellTableBridge_IsIndependentAndRetainsRawMetadataAndPayloads()
        Dim values As Object(,) = {{Nothing, "", "#N/A", "=1+1", False, Double.NaN, ULong.MaxValue}}
        Dim model As New ResultTableOutputModel(values, pvalueColumns:=New Integer() {1, 1, -1, 99})
        Dim artifact As New OutputTableArtifact("raw", model)
        Dim canonical As New OutputCellTable(artifact)
        values(0, 0) = "mutated"
        model.PvalueColumns.Clear()
        artifact.Model.Values(0, 1) = "mutated"
        Assert.AreEqual(OutputCellKind.Blank, canonical.CellAt(0, 0).Kind)
        Assert.AreEqual("", canonical.CellAt(0, 1).TextValue)
        Assert.AreEqual(OutputCellKind.Error, canonical.CellAt(0, 5).Kind)
        Assert.IsTrue(Double.IsNaN(DirectCast(artifact.Model.Values(0, 5), Double)))
        Assert.AreEqual(GetType(ULong), artifact.Model.Values(0, 6).GetType())
        CollectionAssert.AreEqual(New Integer() {1, 1, -1, 99}, artifact.Model.PvalueColumns)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() canonical.CellAt(-1, 0))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() canonical.CellAt(1, 0))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() canonical.CellAt(0, 7))
        Assert.ThrowsException(Of ArgumentNullException)(Function() New OutputCellTable(Nothing))
    End Sub

    Private Class NoStringConversion
        Public Overrides Function ToString() As String
            Throw New InvalidOperationException("Unsupported values must not be stringified.")
        End Function
    End Class
End Class
