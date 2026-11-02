Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports ExcelDna.Integration
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Transport27AdapterTests
    <TestMethod>
    Public Sub OriginalFirstAreaDefectIsReproducedAndCorrectedFrameDoesNotInventRows()
        Dim rewrite = GetType(ExcelDnaDataImporter).GetMethod("BuildConcreteRangeAddress", BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.AreEqual("E3:E5", CStr(rewrite.Invoke(Nothing, {"E8:E10", 3, 3}))) ' Frozen shipping defect; corrected switch not authorized.
        Dim selection As New SpreadsheetRangeSelection
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("B3:B5", "Data", "Book.xlsx"))
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("E8:E10", "Data", "Book.xlsx"))
        Dim reads As New List(Of String)
        Dim snapshot = ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
            Function(reference)
                reads.Add(reference.Address)
                Dim values(reference.RowCount - 1, reference.ColumnCount - 1) As Object
                For r = 0 To reference.RowCount - 1
                    values(r, 0) = reference.StartRow + r
                Next
                Return ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, values, Nothing, Nothing, SpreadsheetDateSystem.Excel1900)
            End Function)
        CollectionAssert.AreEqual(New String() {"B3:B5", "E8:E10"}, reads)
        Assert.AreEqual(8.0R, snapshot.Ranges(1).Rows(0)(0).NumberValue)
        Assert.AreEqual("transport.row-alignment", SpreadsheetSelectionTransport.JoinColumns(snapshot, SpreadsheetAreaAlignment.RequireIdenticalRows).Validation.Issues(0).Code)
        Assert.AreEqual("transport.no-common-rows", SpreadsheetSelectionTransport.JoinColumns(snapshot, SpreadsheetAreaAlignment.PhysicalIntersection).Validation.Issues(0).Code)
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub CorrectedProducerThroughCoreTableAndActualPairedAdapterUsesPhysicalIntersection(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim a = ExcelDnaSharedApplicationAdapter.SnapshotValues(SpreadsheetRangeReference.FromAddress("B3:B6", "Data", "Book.xlsx"), New Object(,) {{1}, {2}, {Nothing}, {4}}, Nothing, Nothing, SpreadsheetDateSystem.Excel1900)
            Dim b = ExcelDnaSharedApplicationAdapter.SnapshotValues(SpreadsheetRangeReference.FromAddress("E4:E7", "Data", "Book.xlsx"), New Object(,) {{10}, {20}, {40}, {80}}, Nothing, Nothing, SpreadsheetDateSystem.Excel1900)
            Dim setData As New SpreadsheetRangeSetData With {.Ranges = New List(Of SpreadsheetRangeData) From {a, b}}
            Dim table = WindowsSpreadsheetTransport.ImportAreas(setData, {"pre", "post"}, SpreadsheetAreaAlignment.PhysicalIntersection)
            CollectionAssert.AreEqual(New Integer() {4, 5, 6}, table.RowIds)
            Dim imported As New DataObj
            imported.LoadCoreDataTable(table)
            CollectionAssert.AreEqual(New Integer() {4, 6}, imported.RowIds)
            Dim message As String = ""
            Dim pairs = WindowsPairedInputAdapter.SelectPairs(imported, message)
            Assert.AreEqual("", message)
            CollectionAssert.AreEqual(New Double() {2, 10, 4, 40}, pairs.X.Cast(Of Double)().ToArray())
            Dim parsed = PairedBlockInputPolicy.ParsePairs(WindowsPairedInputAdapter.Capture(imported), New SpreadsheetPairedParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip}, table.RowIds)
            CollectionAssert.AreEqual(New Integer() {4, 6}, parsed.Data.Pairs.Select(Function(p) p.First.WorksheetRow).ToArray())
            Assert.AreEqual("Book.xlsx", SpreadsheetSelectionTransport.JoinColumns(setData, SpreadsheetAreaAlignment.PhysicalIntersection).Data.SourceAt(0, 1).WorkbookName)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub GuiValue2AndUdfOpaqueValuesHaveIdenticalTypesButDifferentHonestProvenance()
        Dim raw As Object(,) = {{Nothing, DBNull.Value, ExcelEmpty.Value, ExcelMissing.Value, "0", True, 61.0R, ExcelError.ExcelErrorNA, New ErrorWrapper(-2146826281)}}
        Dim gui = ExcelDnaSharedApplicationAdapter.SnapshotValues(SpreadsheetRangeReference.FromAddress("B8:J8", "Data", "Book.xlsx"), raw, Nothing, Nothing, SpreadsheetDateSystem.Excel1904)
        Dim udf = WindowsSpreadsheetTransport.Capture(raw).Data
        For c = 0 To raw.GetLength(1) - 1
            Assert.AreEqual(gui.Rows(0)(c).Kind, udf.Rows(0)(c).Kind)
            Assert.AreEqual(gui.Rows(0)(c).NumberValue, udf.Rows(0)(c).NumberValue)
            Assert.AreEqual(gui.Rows(0)(c).NativeErrorCode, udf.Rows(0)(c).NativeErrorCode)
        Next
        Assert.AreEqual(8, gui.StartRow) : Assert.AreEqual(0, udf.StartRow)
        Assert.AreEqual("", udf.WorkbookName) : Assert.AreEqual(SpreadsheetDateSystem.Unknown, udf.DateSystem)
        Assert.AreEqual(SpreadsheetCellKind.Text, udf.Rows(0)(4).Kind)
        Assert.AreEqual(SpreadsheetCellKind.Bool, udf.Rows(0)(5).Kind)
        Assert.AreEqual(SpreadsheetCellKind.Number, udf.Rows(0)(6).Kind)
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(5)>
    Public Sub UdfGet2DAndActualNumericAndPairedReadersSupportArbitraryArrayLowerBounds(lower As Integer)
        Dim source = Array.CreateInstance(GetType(Object), {4, 2}, {lower, lower})
        Dim data As Object(,) = {{"x", "y"}, {1.5R, 5.0R}, {Nothing, 7.0R}, {2.5R, 9.0R}}
        For r = 0 To 3
            For c = 0 To 1
                source.SetValue(data(r, c), r + lower, c + lower)
            Next
        Next
        Dim copied = UdfDataImport.Get2D(source)
        Assert.AreEqual(0, copied.GetLowerBound(0))
        Assert.AreEqual("x", copied(0, 0))
        Dim matrix As Double(,) = Nothing, rows As Integer, cols As Integer
        Assert.IsTrue(UdfDataImport.TryReadNumericMatrix(source, matrix, rows, cols))
        Assert.AreEqual(3, rows) : Assert.AreEqual(2, cols)
        Assert.AreEqual(1.5R, matrix(0, 0)) : Assert.IsTrue(Double.IsNaN(matrix(1, 0)))
        Dim a As Object(,) = {{"x"}, {1.5R}, {Nothing}, {2.5R}}, b As Object(,) = {{"y"}, {5.0R}, {7.0R}, {9.0R}}
        Dim names As String() = Nothing
        Assert.IsTrue(UdfDataImport.TryReadPairedNumericColumns(a, b, matrix, names))
        CollectionAssert.AreEqual(New Double() {1.5, 5, 2.5, 9}, matrix.Cast(Of Double)().ToArray())
        CollectionAssert.AreEqual(New String() {"x", "y"}, names)
        source.SetValue(999, lower + 1, lower)
        Assert.AreEqual(1.5R, copied(1, 0))
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub UdfNumericEligibilityRetainsExistingWholePairSemantics(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim a As Object(,) = {{1.25R}, {"1,25"}, {True}, {Double.NaN}, {ExcelError.ExcelErrorNA}, {Nothing}, {8.0R}}, b As Object(,) = {{2.5R}, {3}, {4}, {5}, {6}, {7}, {9.0R}}
            Dim pairs = UdfDataImport.ExtractCompletePairedNumericRows(a, b)
            Assert.AreEqual(2, pairs.Count)
            CollectionAssert.AreEqual(New Double() {1.25, 2.5}, pairs(0))
            CollectionAssert.AreEqual(New Double() {8, 9}, pairs(1))
            Assert.AreEqual(0, UdfDataImport.ExtractCompletePairedNumericRows(New Object(,) {{"x"}}, New Object(,) {{"y"}}).Count)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub UdfReferenceMetadataIsBoundedAndArraysNeverInventPhysicalRows()
        ' Explicit sheet-token constructor has no Excel callback or COM acquisition.
        Dim reference As New ExcelReference(7, 10, 2, 3, New IntPtr(77))
        Dim values As Object(,) = {{1, 2}, {3, 4}}
        Dim frame = WindowsSpreadsheetTransport.CaptureUdf(values, reference, 1)
        CollectionAssert.AreEqual(New Integer() {9, 10}, frame.RowIds.ToArray())
        Assert.AreEqual(3, frame.SourceAt(0, 0).WorksheetColumn)
        Assert.AreEqual("", frame.Data.WorkbookName)
        Assert.AreEqual("ExcelReference:77", frame.Data.WorksheetId)
        Assert.AreEqual(SpreadsheetDateSystem.Unknown, frame.Data.DateSystem)
        CollectionAssert.AreEqual(New Integer() {0, 0}, WindowsSpreadsheetTransport.CaptureUdf(values, values).RowIds.ToArray())
        Assert.ThrowsException(Of ArgumentException)(Function() WindowsSpreadsheetTransport.CaptureUdf(values, reference, 3))
    End Sub

    <DataTestMethod> <DataRow(7, 77, True)> <DataRow(8, 77, False)> <DataRow(7, 78, False)>
    Public Sub UdfPhysicalReferenceAlignmentFailsBeforeCoercingUnrelatedOrShiftedCells(start As Integer, sheet As Integer, valid As Boolean)
        Dim first As New ExcelReference(7, 9, 1, 1, New IntPtr(77))
        Dim second As New ExcelReference(start, start + 2, 4, 4, New IntPtr(sheet))
        Assert.AreEqual(valid, WindowsSpreadsheetTransport.PairedArgumentsAligned(first, second))
        If Not valid Then
            Dim matrix As Double(,) = Nothing, names As String() = Nothing, errorValue As ExcelError? = Nothing
            Assert.IsFalse(UdfDataImport.TryReadPairedNumericColumns(first, second, matrix, names))
            Assert.IsNull(matrix)
            Assert.IsNull(UdfDataImport.ExtractPairedNumericColumnsIgnoringNonNumeric(first, second, errorValue))
            Assert.AreEqual(ExcelError.ExcelErrorValue, errorValue.Value)
        End If
    End Sub

    <TestMethod>
    Public Sub ReaderFaultUnsupportedTypeAndShapeCannotReturnPartialAcceptedData()
        Dim reference = SpreadsheetRangeReference.FromAddress("A1:B2", "Data", "Book.xlsx")
        Assert.ThrowsException(Of InvalidOperationException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotValueReader(reference,
            Function() As Object
                Throw New InvalidOperationException("reader failed")
            End Function, Nothing, Nothing, SpreadsheetDateSystem.Unknown))
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, 1, Nothing, Nothing, SpreadsheetDateSystem.Unknown))
        Assert.ThrowsException(Of ArgumentException)(Function() WindowsSpreadsheetTransport.Capture(New Object(,) {{New Object()}}))
        Assert.ThrowsException(Of ArgumentException)(Function() WindowsSpreadsheetTransport.Capture(New Object(,) {{New DateTime(2026, 1, 1)}}))
    End Sub

    <TestMethod>
    Public Sub AllRangesArePreflightedBeforeCallbacksAndIdentityMutationFailsClosed()
        Dim selection As New SpreadsheetRangeSelection
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("A1", "Data", "Book.xlsx"))
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("B1:B1048576", "Data", "Book.xlsx"))
        Dim calls As Integer
        Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
            Function(r)
                calls += 1
                Return Nothing
            End Function))
        Assert.AreEqual(0, calls)
        selection.Ranges.RemoveAt(1)
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
            Function(r)
                Dim snapshot = ExcelDnaSharedApplicationAdapter.SnapshotValues(r, 1, Nothing, Nothing, SpreadsheetDateSystem.Unknown)
                snapshot.WorkbookName = "Unrelated.xlsx"
                Return snapshot
            End Function))
        selection.Ranges(0).WorkbookId = "unverifiable"
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection, Function(r) Nothing))
    End Sub

    <TestMethod>
    Public Sub UdfBudgetAndObservedShapeAreCheckedBeforeAcceptingAnyValues()
        Dim huge As New ExcelReference(0, 1048575, 0, 16383, New IntPtr(77))
        Dim calls As Integer
        Dim reader As Func(Of ExcelReference, Object) = Function(r)
                                                         calls += 1
                                                         Return New Object(,) {{1}}
                                                     End Function
        Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() WindowsSpreadsheetTransport.ReadUdfMatrix(huge, reader))
        Assert.AreEqual(0, calls)
        Dim column As New ExcelReference(0, 1048575, 0, 0, New IntPtr(77))
        Assert.ThrowsException(Of ArgumentException)(Function() WindowsSpreadsheetTransport.ReadUdfMatrix(column, reader))
        Assert.AreEqual(1, calls) ' Full column is allowed by metadata, truncated coercion is not.
        Dim cellReference As New ExcelReference(7, 7, 2, 2, New IntPtr(77))
        Assert.AreEqual(5, WindowsSpreadsheetTransport.ReadUdfMatrix(cellReference, Function(r) 5)(0, 0))
        Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() WindowsSpreadsheetTransport.ReadUdfMatrix(New Object(,) {{1, 2}}, reader, New InputSizeLimits(maxRangeCells:=1)))
        Assert.AreEqual(1, calls)
        Dim responseLimit = Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() WindowsSpreadsheetTransport.ReadUdfMatrix(
            New ExcelReference(0, 1, 0, 0, New IntPtr(77)), reader, New InputSizeLimits(maxChunkCells:=1)))
        Assert.AreEqual("input.udf-response", responseLimit.Report.Issues(0).Code)
        Assert.AreEqual(1, calls) ' Per-response limit also applies before a callback.
        Assert.ThrowsException(Of ArgumentException)(Function() WindowsSpreadsheetTransport.ReadUdfMatrix(New ExcelReference(0, 1, 0, 0, New IntPtr(77)), Function(r) 1))
    End Sub

    <TestMethod>
    Public Sub AcquisitionCallbackCannotRewriteItsOwnExpectedIdentity()
        Dim selection As New SpreadsheetRangeSelection
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("B3", "Data", "Book.xlsx"))
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
            Function(reference)
                reference.StartRow = 8 : reference.Address = "B8"
                Return ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, 88, Nothing, Nothing, SpreadsheetDateSystem.Unknown)
            End Function))
        Assert.AreEqual("B3:B3", selection.Ranges(0).Address) ' Existing A1 parser's singleton canonical spelling.
    End Sub

    <TestMethod>
    Public Sub LaterCallbackCannotMutatePreviouslyAcceptedSnapshot()
        Dim selection As New SpreadsheetRangeSelection, retained As SpreadsheetRangeData = Nothing
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("B3", "Data", "Book.xlsx"))
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("E3", "Data", "Book.xlsx"))
        Dim result = ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
            Function(reference)
                If retained IsNot Nothing Then retained.Rows(0)(0).NumberValue = 999
                Dim raw = ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, 3, Nothing, Nothing, SpreadsheetDateSystem.Unknown)
                If retained Is Nothing Then retained = raw
                Return raw
            End Function)
        Assert.AreEqual(3.0R, result.Ranges(0).Rows(0)(0).NumberValue)
        Assert.AreEqual(999.0R, retained.Rows(0)(0).NumberValue)
    End Sub

    <DataTestMethod> <DataRow("coordinates")> <DataRow("shape")> <DataRow("revision")>
    Public Sub AcquisitionCannotAcceptUnknownOrContradictoryObservedMetadata(kind As String)
        Dim selection As New SpreadsheetRangeSelection
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("B3", "Data", "Book.xlsx"))
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
            Function(reference)
                Dim snapshot = ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, 3, Nothing, Nothing, SpreadsheetDateSystem.Unknown)
                Select Case kind
                    Case "coordinates" : snapshot.StartRow = 0
                    Case "shape" : snapshot.RowCount = 2
                    Case "revision" : snapshot.SelectionRevision = "fabricated"
                End Select
                Return snapshot
            End Function))
    End Sub
End Class
