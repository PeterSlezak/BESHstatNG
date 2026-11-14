Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfInputPolicy39Tests
    Private Shared Function Frame(values As Double(,), Optional physical As Boolean = True) As SpreadsheetRangeData
        Dim result As New SpreadsheetRangeData With {.RowCount = values.GetLength(0), .ColumnCount = values.GetLength(1),
            .StartRow = If(physical, 20, 0), .StartColumn = If(physical, 4, 0),
            .WorkbookId = If(physical, "book", ""), .WorksheetId = If(physical, "sheet", "")}
        For r = 0 To result.RowCount - 1
            Dim cells As New List(Of SpreadsheetCellValue)
            For c = 0 To result.ColumnCount - 1
                cells.Add(SpreadsheetCellValue.NumberCell(values(r, c)))
            Next
            result.Rows.Add(cells)
        Next
        Return result
    End Function

    <TestMethod>
    Public Sub PhysicalIntersectionAndOrdinalPairsAreSeparateContracts()
        Dim a = Frame(New Double(,) {{1}, {2}, {3}}), b = Frame(New Double(,) {{10}, {20}, {30}})
        Dim aligned = PairedBlockInputPolicy.AlignRows(a, {40, 20, 90}, b, {20, 40, 80})
        Assert.IsTrue(aligned.Success)
        CollectionAssert.AreEqual(New Integer() {1, 2}, aligned.Data.FirstRows.ToArray())
        CollectionAssert.AreEqual(New Integer() {2, 1}, aligned.Data.SecondRows.ToArray())
        Dim ordinal = PairedBlockInputPolicy.ParsePairs(Frame(New Double(,) {{1, 10}, {2, 20}, {3, 30}}),
            New SpreadsheetPairedParseOptions, {40, 20, 90})
        Assert.IsTrue(ordinal.Success)
        Assert.AreEqual(10.0, ordinal.Data.Pairs(0).Second.Value)
        Assert.AreEqual(40, ordinal.Data.Pairs(0).First.WorksheetRow)
        Assert.AreEqual(1, ordinal.Data.Pairs(0).First.SelectedRow)
    End Sub

    <DataTestMethod> <DataRow("book")> <DataRow("sheet")> <DataRow("unknown")> <DataRow("duplicates")>
    Public Sub AmbiguousPhysicalIdentityIsRejectedWithoutInventedCoordinates(kind As String)
        Dim a = Frame(New Double(,) {{1}, {2}}), b = Frame(New Double(,) {{10}, {20}})
        Dim ids As Integer() = {20, 40}
        Select Case kind
            Case "book" : b.WorkbookId = "other"
            Case "sheet" : b.WorksheetId = "other"
            Case "unknown" : b.WorkbookId = "" : b.WorksheetId = ""
            Case "duplicates" : ids = {20, 20}
        End Select
        Assert.IsFalse(PairedBlockInputPolicy.AlignRows(a, {20, 40}, b, ids).Success)
    End Sub

    <TestMethod>
    Public Sub ExplicitSubjectKeysJoinAcrossBooksInFirstSelectionOrder()
        Dim a = Frame(New Double(,) {{1}, {2}, {3}}), b = Frame(New Double(,) {{10}, {20}, {30}})
        b.WorkbookId = "other"
        Dim joined = PairedBlockInputPolicy.AlignRows(a, {10, 20, 30}, b, {90, 80, 70}, {"B", "A", "C"}, {"A", "B", "Z"})
        Assert.IsTrue(joined.Success)
        CollectionAssert.AreEqual(New Integer() {1, 2}, joined.Data.FirstRows.ToArray())
        CollectionAssert.AreEqual(New Integer() {2, 1}, joined.Data.SecondRows.ToArray())
        Assert.IsFalse(PairedBlockInputPolicy.AlignRows(a, {10, 20, 30}, b, {90, 80, 70}, {"A", "A", "C"}, {"A", "B", "Z"}).Success)
    End Sub

    <TestMethod>
    Public Sub OpaqueArrayPositionsDoNotBecomeWorksheetRows()
        Dim parsed = PairedBlockInputPolicy.ParsePairs(Frame(New Double(,) {{1, 2}, {3, 4}}, False), New SpreadsheetPairedParseOptions)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2, parsed.Data.Pairs(1).First.SelectedRow)
        Assert.AreEqual(0, parsed.Data.Pairs(1).First.WorksheetRow)
        Assert.AreEqual(0, parsed.Data.Pairs(1).First.WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub WholePairExclusionPreservesUncompactedSourceAndSelectedPositions()
        Dim f = Frame(New Double(,) {{1, 2}, {3, 4}, {5, 6}, {7, 8}})
        f.Rows(1)(0) = SpreadsheetCellValue.BlankCell()
        f.Rows(2)(1) = SpreadsheetCellValue.BlankCell()
        Dim parsed = PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip}, {30, 40, 80, 90})
        Assert.IsTrue(parsed.Success)
        CollectionAssert.AreEqual(New Integer() {30, 90}, parsed.Data.Pairs.Select(Function(p) p.First.WorksheetRow).ToArray())
        CollectionAssert.AreEqual(New Integer() {1, 4}, parsed.Data.Pairs.Select(Function(p) p.First.SelectedRow).ToArray())
        CollectionAssert.AreEqual(New Double() {2, 8}, parsed.Data.SecondValues)
    End Sub

    <TestMethod>
    Public Sub StrictVectorCallbackOrderAndFailureOutputRemainObservable()
        Dim calls As New List(Of String), values As Double() = {99}
        Dim blank As Func(Of Integer, Boolean) = Function(i)
                                                    calls.Add("blank:" & i.ToString(CultureInfo.InvariantCulture))
                                                    Return i = 2
                                                End Function
        Dim number As Func(Of Integer, Double?) = Function(i)
                                                     calls.Add("number:" & i.ToString(CultureInfo.InvariantCulture))
                                                     Return If(i = 0, CType(Nothing, Double?), CType(2.0, Double?))
                                                 End Function
        Assert.IsTrue(UdfNumericImportPreparation38.TryStrictVector(3, blank, number, values))
        CollectionAssert.AreEqual(New Double() {2}, values)
        CollectionAssert.AreEqual(New String() {"blank:2", "blank:1", "number:0", "blank:1", "number:1"}, calls.ToArray())
        Assert.IsFalse(UdfNumericImportPreparation38.TryStrictVector(0, blank, number, values))
        Assert.IsNull(values)
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub InvariantFirstFlexibleTextAndIntegerTruncationAreRetained(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim value As Double = 19
            Assert.IsTrue(UdfScalarChoice38.TryFiniteText("1,5", value))
            Assert.AreEqual(15.0, value) ' Invariant thousands interpretation precedes current culture.
            Dim binary As Integer = 99
            Assert.IsTrue(UdfScalarChoice38.TryBinary(0.75, Nothing, binary))
            Assert.AreEqual(0, binary)
            Assert.ThrowsException(Of OverflowException)(Sub() UdfScalarChoice38.TryBinary(1.0E+20, Nothing, binary))
            Dim whole As Integer = 77
            Assert.IsFalse(UdfScalarChoice38.TryWholeNumber(0.75, whole))
            Assert.AreEqual(0, whole)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub
End Class
