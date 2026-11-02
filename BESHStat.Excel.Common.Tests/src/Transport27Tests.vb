Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Transport27Tests
    Private Shared Function Area(start As Integer, count As Integer, column As Integer, Optional width As Integer = 1) As SpreadsheetRangeData
        Dim result As New SpreadsheetRangeData With {.WorkbookName = "Book.xlsx", .WorksheetName = "Data", .WorksheetId = "Sheet1",
            .StartRow = start, .StartColumn = column, .RowCount = count, .ColumnCount = width, .DateSystem = SpreadsheetDateSystem.Excel1900}
        For r = 0 To count - 1
            Dim cells As New List(Of SpreadsheetCellValue)
            For c = 0 To width - 1
                cells.Add(SpreadsheetCellValue.NumberCell((column + c) * 1000.0R + start + r))
            Next
            result.Rows.Add(cells)
        Next
        Return result
    End Function
    Private Shared Function SetOf(ParamArray areas As SpreadsheetRangeData()) As SpreadsheetRangeSetData
        Return New SpreadsheetRangeSetData With {.Ranges = areas.ToList(), .SelectionId = "test-correlation"}
    End Function

    <DataTestMethod>
    <DataRow(3, 3, 3, 3, True)>
    <DataRow(3, 3, 8, 3, False)>
    <DataRow(3, 3, 3, 2, False)>
    <DataRow(3, 3, 4, 3, False)>
    <DataRow(1, 1, 1, 1, True)>
    <DataRow(8, 3, 3, 3, False)>
    Public Sub IdenticalRowsNeverBorrowFirstAreaGeometry(a As Integer, n As Integer, b As Integer, m As Integer, valid As Boolean)
        Dim first = Area(a, n, 2), second = Area(b, m, 5)
        Dim actual = SpreadsheetSelectionTransport.JoinColumns(SetOf(first, second), SpreadsheetAreaAlignment.RequireIdenticalRows)
        Assert.AreEqual(valid, actual.Success)
        Assert.AreEqual(b, second.StartRow) : Assert.AreEqual(m, second.RowCount)
        If valid Then
            Assert.AreEqual(5000.0R + b, actual.Data.Data.Rows(0)(1).NumberValue)
            Assert.AreEqual(b, actual.Data.SourceAt(0, 1).WorksheetRow)
            Assert.AreEqual(5, actual.Data.SourceAt(0, 1).WorksheetColumn)
            Assert.AreEqual(1, actual.Data.SourceAt(0, 1).AreaIndex)
        Else
            Assert.IsNull(actual.Data)
            Assert.AreEqual("transport.row-alignment", actual.Validation.Issues(0).Code)
        End If
    End Sub

    <DataTestMethod>
    <DataRow(3, 3, 4, 4, 4, 5)>
    <DataRow(4, 4, 3, 3, 4, 5)>
    <DataRow(3, 3, 3, 2, 3, 4)>
    <DataRow(3, 1, 3, 3, 3, 3)>
    Public Sub ExplicitIntersectionRetainsPhysicalRowsAndOriginalSelections(a As Integer, n As Integer, b As Integer, m As Integer, start As Integer, finish As Integer)
        Dim actual = SpreadsheetSelectionTransport.JoinColumns(SetOf(Area(a, n, 2), Area(b, m, 5)), SpreadsheetAreaAlignment.PhysicalIntersection)
        Assert.IsTrue(actual.Success)
        CollectionAssert.AreEqual(Enumerable.Range(start, finish - start + 1).ToArray(), actual.Data.RowIds.ToArray())
        Assert.IsTrue(actual.Validation.HasWarnings)
        For r = 0 To actual.Data.RowIds.Count - 1
            Dim row = start + r
            Assert.AreEqual(row - a + 1, actual.Data.SourceAt(r, 0).SelectedRow)
            Assert.AreEqual(row - b + 1, actual.Data.SourceAt(r, 1).SelectedRow)
            Assert.AreEqual(2000.0R + row, actual.Data.Data.Rows(r)(0).NumberValue)
            Assert.AreEqual(5000.0R + row, actual.Data.Data.Rows(r)(1).NumberValue)
        Next
        Dim table = actual.Data.ToCoreDataTable({"first", "second"})
        CollectionAssert.AreEqual(actual.Data.RowIds.ToArray(), table.RowIds)
        Assert.AreEqual(0, table.FirstSourceColumn) ' Joined columns are not contiguous.
        Assert.AreEqual(a, actual.Data.Areas(0).StartRow)
        Assert.AreEqual(b, actual.Data.Areas(1).StartRow)
    End Sub

    <DataTestMethod>
    <DataRow("disjoint", "transport.no-common-rows")>
    <DataRow("book", "transport.identity")>
    <DataRow("sheet", "transport.identity")>
    <DataRow("code", "transport.identity")>
    <DataRow("revision", "transport.identity")>
    <DataRow("date", "transport.identity")>
    <DataRow("duplicate", "transport.duplicate-cell")>
    <DataRow("unknown", "transport.identity")>
    <DataRow("shape", "transport.shape")>
    Public Sub InvalidJoinsHaveNoPartialFrame(kind As String, code As String)
        Dim a = Area(3, 3, 2), b = Area(3, 3, 5)
        Select Case kind
            Case "disjoint" : b.StartRow = 8
            Case "book" : b.WorkbookName = "Other.xlsx"
            Case "sheet" : b.WorksheetName = "Other"
            Case "code" : b.WorksheetId = "OtherCode"
            Case "revision" : b.SelectionRevision = "unmatched"
            Case "date" : b.DateSystem = SpreadsheetDateSystem.Excel1904
            Case "duplicate" : b.StartColumn = 2
            Case "unknown" : b.WorkbookName = ""
            Case "shape" : b.Rows.RemoveAt(1)
        End Select
        Dim actual = SpreadsheetSelectionTransport.JoinColumns(SetOf(a, b), SpreadsheetAreaAlignment.PhysicalIntersection)
        Assert.IsFalse(actual.Success) : Assert.IsNull(actual.Data)
        Assert.AreEqual(code, actual.Validation.Issues(0).Code)
    End Sub

    <TestMethod>
    Public Sub RectangleThenColumnAndReverseOrderPreserveTreatmentOrderAndDefensiveCopies()
        Dim a = Area(101, 3, 2, 2), b = Area(101, 3, 7)
        Dim actual = SpreadsheetSelectionTransport.JoinColumns(SetOf(b, a), SpreadsheetAreaAlignment.RequireIdenticalRows).Data
        CollectionAssert.AreEqual(New Integer() {7, 2, 3}, Enumerable.Range(0, 3).Select(Function(c) actual.SourceAt(0, c).WorksheetColumn).ToArray())
        b.Rows(0)(0).NumberValue = -1
        actual.Data.Rows(0)(0).NumberValue = -2
        actual.Areas(0).Rows(0)(0).NumberValue = -3
        Assert.AreEqual(7101.0R, actual.Data.Rows(0)(0).NumberValue)
        Assert.AreEqual(7101.0R, actual.Areas(0).Rows(0)(0).NumberValue)
    End Sub

    <TestMethod>
    Public Sub NoncontiguousRowMapSurvivesCompleteBlockFilteringAndUnknownArraysStayUnknown()
        Dim data = Area(0, 3, 0, 3)
        data.Rows(1)(1) = SpreadsheetCellValue.BlankCell()
        Dim frame = SpreadsheetSelectionTransport.Capture(data, New Integer() {101, 103, 107}).Data
        Dim blocks = PairedBlockInputPolicy.ParseBlocks(frame.Data, New SpreadsheetBlockParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip}, frame.RowIds)
        Assert.IsTrue(blocks.Success)
        CollectionAssert.AreEqual(New Integer() {101, 107}, blocks.Data.Blocks.Select(Function(b) b(0).WorksheetRow).ToArray())
        Assert.AreEqual(103, blocks.Validation.Issues.Single(Function(i) i.Code = "blocks.incomplete-skipped").WorksheetRow)
        Dim opaque = SpreadsheetSelectionTransport.Capture(data).Data
        CollectionAssert.AreEqual(New Integer() {0, 0, 0}, opaque.RowIds.ToArray())
        Assert.ThrowsException(Of ArgumentException)(Function() opaque.ToCoreDataTable({"a", "b", "c"}))
        Assert.IsFalse(SpreadsheetSelectionTransport.Capture(data, New Integer() {101, 101, 107}).Success)
    End Sub

    <DataTestMethod>
    <DataRow("", "1.25", 1.25R)>
    <DataRow("sk-SK", "1,25", 1.25R)>
    Public Sub RawTypesDateHintsErrorCoordinatesAndExplicitCultureArePreserved(culture As String, text As String, expected As Double)
        Dim a = Area(8, 2, 2), b = Area(8, 2, 5)
        a.Rows(0)(0) = SpreadsheetCellValue.TextCell(text)
        a.Rows(1)(0) = SpreadsheetCellValue.NumberCell(60) : a.Rows(1)(0).NumberFormat = "dd/mm/yyyy"
        b.Rows(0)(0) = SpreadsheetCellValue.ErrorCell("#N/A", "2042")
        b.Rows(1)(0) = SpreadsheetCellValue.BooleanCell(True)
        Dim frame = SpreadsheetSelectionTransport.JoinColumns(SetOf(a, b), SpreadsheetAreaAlignment.RequireIdenticalRows).Data
        Dim ex = Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() frame.ToCoreDataTable({"a", "b"}))
        Assert.AreEqual("transport.cell", ex.Report.Issues(0).Code)
        Assert.AreEqual(8, ex.Report.Issues(0).WorksheetRow) : Assert.AreEqual(5, ex.Report.Issues(0).WorksheetColumn)
        Dim table = frame.ToCoreDataTable({"a", "b"}, New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.TreatAsMissing,
            .NumericText = SpreadsheetNumericTextPolicy.ExplicitCulture, .NumericCultureName = If(culture = "", "en-US", culture), .AllowBooleanNumbers = True})
        Assert.AreEqual(expected, table.NumericMatrix(0, 0))
        Assert.AreEqual(60.0R, table.NumericMatrix(1, 0)) ' A date-looking format does not change a Value2 number.
        Assert.IsTrue(table.MissingMask(0, 1))
        Assert.AreEqual("2042", frame.Data.Rows(0)(1).NativeErrorCode)
        Assert.AreEqual("dd/mm/yyyy", frame.Data.Rows(1)(0).NumberFormat)
    End Sub

    <TestMethod>
    Public Sub InvalidOptionsBudgetsAndCoordinatesRejectWithoutAllocatingJoinedData()
        Dim source = SetOf(Area(3, 3, 2), Area(3, 3, 5))
        Assert.AreEqual("transport.alignment", SpreadsheetSelectionTransport.JoinColumns(source, CType(99, SpreadsheetAreaAlignment)).Validation.Issues(0).Code)
        Assert.AreEqual("input.total-cells", SpreadsheetSelectionTransport.JoinColumns(source, SpreadsheetAreaAlignment.RequireIdenticalRows, New InputSizeLimits(maxTotalCells:=5)).Validation.Issues(0).Code)
        Assert.AreEqual("transport.ranges", SpreadsheetSelectionTransport.JoinColumns(Nothing, SpreadsheetAreaAlignment.RequireIdenticalRows).Validation.Issues(0).Code)
        Dim overflow = Area(Integer.MaxValue, 2, 2)
        Assert.AreEqual("transport.coordinates", SpreadsheetSelectionTransport.Capture(overflow).Validation.Issues(0).Code)
    End Sub
End Class
