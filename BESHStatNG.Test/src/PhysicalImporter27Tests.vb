Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports ExcelDna.Integration
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PhysicalImporter27Tests
    ' Fake host values at the shipping operation-local acquisition boundary.
    ' SnapshotSelection/SnapshotValues are the actual existing Windows adapters.
    Private Shared Function ImportFixture(addresses As String(), values As IReadOnlyList(Of Object(,)),
        Optional reads As List(Of String) = Nothing, Optional merge As Object = Nothing,
        Optional mutate As Action(Of SpreadsheetRangeData, Integer) = Nothing) As CoreDataTable
        Return ExcelDnaDataImporter.ImportPhysicalSelection("'Data'!" & String.Join(",", addresses), addresses,
            Function(requested)
                Dim selection As New SpreadsheetRangeSelection With {.SelectionId = "synthetic-physical-import"}
                For Each address In requested
                    ExcelDnaDataImporter.ValidatePhysicalMergeState(If(merge, False))
                    selection.Ranges.Add(SpreadsheetRangeReference.FromAddress(address, "Data", "Book.xlsx"))
                Next
                Dim index As Integer
                Return ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
                    Function(reference)
                        If reads IsNot Nothing Then reads.Add(reference.Address)
                        Dim snapshot = ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, values(index),
                            Function(r, c) "yyyy-mm-dd", Nothing, SpreadsheetDateSystem.Excel1900)
                        If mutate IsNot Nothing Then mutate(snapshot, index)
                        index += 1
                        Return snapshot
                    End Function)
            End Function)
    End Function

    <TestMethod>
    Public Sub ShippingAssemblerRejectsDisjointOwnAreasBeforeStatisticsOrWriter()
        Dim reads As New List(Of String), statistics As Integer, writes As Integer
        Dim errorValue = Assert.ThrowsException(Of SpreadsheetInputValidationException)(
            Sub()
                Dim table = ImportFixture({"B3:B5", "E8:E10"}, {New Object(,) {{1}, {2}, {3}}, New Object(,) {{8}, {9}, {10}}}, reads)
                statistics += table.RowCount
                writes += 1
            End Sub)
        CollectionAssert.AreEqual(New String() {"B3:B5", "E8:E10"}, reads)
        Assert.AreEqual("transport.no-common-rows", errorValue.Report.Issues(0).Code)
        Assert.AreEqual(0, statistics) : Assert.AreEqual(0, writes)
        Assert.IsNull(ExcelDnaDataImporter.GetPhysicalImportEvidence(Nothing))
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub RealAssemblerToDataObjToPairedPolicyKeepsRowsFourAndSix(reverse As Boolean)
        Dim addresses = If(reverse, New String() {"E4:E7", "B3:B6"}, New String() {"B3:B6", "E4:E7"})
        Dim b As Object(,) = {{1}, {2}, {Nothing}, {4}}, e As Object(,) = {{10}, {20}, {40}, {80}}
        Dim values As IReadOnlyList(Of Object(,)) = If(reverse, New List(Of Object(,)) From {e, b}, New List(Of Object(,)) From {b, e})
        Dim table = ImportFixture(addresses, values)
        CollectionAssert.AreEqual(New Integer() {4, 5, 6}, table.RowIds)
        Dim evidence = ExcelDnaDataImporter.GetPhysicalImportEvidence(table)
        Assert.IsTrue(evidence.Success) : Assert.IsTrue(evidence.Validation.HasWarnings)
        Assert.AreEqual(2, evidence.Validation.Issues.Where(Function(i) i.Code = "transport.rows-excluded").Count())
        Assert.AreEqual(If(reverse, 5, 2), evidence.Data.SourceAt(0, 0).WorksheetColumn)
        Assert.AreEqual(If(reverse, "E4:E7", "B3:B6"), evidence.Data.SourceAt(0, 0).Address)
        Assert.AreEqual("Book.xlsx", evidence.Data.SourceAt(0, 1).WorkbookName)
        Assert.AreEqual(SpreadsheetDateSystem.Excel1900, evidence.Data.Areas(0).DateSystem)
        Dim imported As New DataObj
        imported.LoadCoreDataTable(table, cloneTable:=False) ' Same transfer used by ImportInto.
        Assert.AreSame(table, imported.CoreTable)
        Assert.AreSame(evidence, ExcelDnaDataImporter.GetPhysicalImportEvidence(imported.CoreTable))
        CollectionAssert.AreEqual(New Integer() {4, 6}, imported.RowIds)
        Dim message As String = ""
        Dim pairs = WindowsPairedInputAdapter.SelectPairs(imported, message)
        Assert.AreEqual("", message)
        CollectionAssert.AreEqual(If(reverse, New Double() {10, 2, 40, 4}, New Double() {2, 10, 4, 40}), pairs.X.Cast(Of Double)().ToArray())
        Assert.AreEqual(4, evidence.Data.SourceAt(0, 0).WorksheetRow)
        Assert.AreEqual(6, evidence.Data.SourceAt(2, 1).WorksheetRow)
        Assert.IsNull(ExcelDnaDataImporter.GetPhysicalImportEvidence(table.Clone())) ' No invented sidecar on an arbitrary Core clone.
    End Sub

    <DataTestMethod>
    <DataRow(3, 3, 3, 2, 3, 4)>
    <DataRow(3, 3, 4, 3, 4, 5)>
    <DataRow(3, 1, 3, 3, 3, 3)>
    <DataRow(8, 2, 7, 4, 8, 9)>
    Public Sub DifferingExtentsUseOnlyPhysicalIntersection(a As Integer, n As Integer, b As Integer, m As Integer, first As Integer, last As Integer)
        Dim av(n - 1, 0), bv(m - 1, 0) As Object
        For i = 0 To n - 1 : av(i, 0) = a + i : Next
        For i = 0 To m - 1 : bv(i, 0) = 100 + b + i : Next
        Dim table = ImportFixture({"B" & a & ":B" & (a + n - 1), "E" & b & ":E" & (b + m - 1)}, {av, bv})
        CollectionAssert.AreEqual(Enumerable.Range(first, last - first + 1).ToArray(), table.RowIds)
        Assert.AreEqual(CDbl(first), table.NumericMatrix(0, 0))
        Assert.AreEqual(100.0R + first, table.NumericMatrix(0, 1))
    End Sub

    <TestMethod>
    Public Sub RectangleSingletonAndBlockedConsumerPreserveSelectedColumnOrder()
        Dim table = ImportFixture({"B3:C4", "G4"}, {New Object(,) {{1, 2}, {3, 4}}, New Object(,) {{9}}})
        CollectionAssert.AreEqual(New Integer() {4}, table.RowIds)
        CollectionAssert.AreEqual(New Double() {3, 4, 9}, table.NumericMatrix.Cast(Of Double)().ToArray())
        Dim source = ExcelDnaDataImporter.GetPhysicalImportEvidence(table).Data
        CollectionAssert.AreEqual(New Integer() {2, 3, 7}, Enumerable.Range(0, 3).Select(Function(c) source.SourceAt(0, c).WorksheetColumn).ToArray())
        Dim data As New DataObj
        data.LoadCoreDataTable(table, cloneTable:=False)
        Dim message As String = ""
        Dim blocks = WindowsPairedInputAdapter.SelectBlocks(data, message)
        Assert.AreEqual("", message)
        CollectionAssert.AreEqual(New Double() {3, 4, 9}, blocks.X.Cast(Of Double)().ToArray())
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub HeaderCultureErrorMissingAndDateSerialPoliciesMatchReviewedProjection(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim text = If(culture = "sk-SK", "1,25", "1.25")
            Dim table = ImportFixture({"B3:B5", "E4:E6"}, {
                New Object(,) {{"discarded header"}, {text}, {61.0R}}, New Object(,) {{2.5R}, {ExcelError.ExcelErrorNA}, {8.0R}}})
            Assert.AreEqual(text, table.ColumnNames(0)) ' First common row4, not original row3.
            Assert.AreEqual("2" & CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator & "5", table.ColumnNames(1))
            Assert.AreEqual(1.25R, table.NumericMatrix(0, 0))
            Assert.AreEqual(61.0R, table.NumericMatrix(1, 0)) ' Number format is only a hint.
            Assert.IsTrue(table.MissingMask(1, 1))
            Dim sidecar = ExcelDnaDataImporter.GetPhysicalImportEvidence(table)
            Assert.AreEqual(SpreadsheetCellKind.CellError, sidecar.Data.Data.Rows(1)(1).Kind)
            Assert.AreEqual("yyyy-mm-dd", sidecar.Data.Data.Rows(1)(0).NumberFormat)
            Dim data As New DataObj
            data.LoadCoreDataTable(table, cloneTable:=False)
            CollectionAssert.AreEqual(New Integer() {4}, data.RowIds)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub CharacterColumnsSkipRowAndByIdConsumerRetainOriginalPhysicalRowsAndOrder()
        Dim table = ImportFixture({"B3:B6", "E4:E7"}, {
            New Object(,) {{"original header"}, {"B"}, {"A"}, {"B"}}, New Object(,) {{1}, {2}, {3}, {4}}})
        Dim data As New DataObj
        data.LoadCoreDataTable(table, CharCols:=0, SkipRow:=0, cloneTable:=False)
        CollectionAssert.AreEqual(New Integer() {4, 5, 6}, data.RowIds)
        Dim message As String = ""
        Dim grouped = WindowsUnivariateInputAdapter.SelectById(data, message)
        Assert.AreEqual("", message)
        CollectionAssert.AreEqual(New String() {"B", "A"}, grouped.varNames)
        CollectionAssert.AreEqual(New Double() {1, 3}, grouped.X(0))
        Dim skipped As New DataObj
        skipped.LoadCoreDataTable(table, CharCols:=0, SkipRow:=1, cloneTable:=False)
        CollectionAssert.AreEqual(New Integer() {5, 6}, skipped.RowIds)
        Assert.AreSame(table, skipped.CoreTable)
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub RepeatedAndOverlappingCellsRejectBeforeTable(rectangles As Boolean)
        Dim addresses = If(rectangles, New String() {"B3:C4", "C3:D4"}, New String() {"B3:B4", "B3:B4"})
        Dim values As IReadOnlyList(Of Object(,)) = If(rectangles,
            New List(Of Object(,)) From {New Object(,) {{1, 2}, {3, 4}}, New Object(,) {{5, 6}, {7, 8}}},
            New List(Of Object(,)) From {New Object(,) {{1}, {2}}, New Object(,) {{3}, {4}}})
        Dim errorValue = Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() ImportFixture(addresses,
            values))
        Assert.AreEqual("transport.duplicate-cell", errorValue.Report.Issues(0).Code)
    End Sub

    <DataTestMethod> <DataRow("book")> <DataRow("sheet")> <DataRow("code")> <DataRow("revision")>
    Public Sub DirectCapturedContextMismatchFailsClosed(kind As String)
        Dim errorValue = Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() ExcelDnaDataImporter.ImportPhysicalSelection("original", {"B3", "E3"},
            Function(addresses)
                Dim a = ExcelDnaSharedApplicationAdapter.SnapshotValues(SpreadsheetRangeReference.FromAddress("B3", "Data", "Book.xlsx"), 1, Nothing, Nothing, SpreadsheetDateSystem.Unknown)
                Dim b = ExcelDnaSharedApplicationAdapter.SnapshotValues(SpreadsheetRangeReference.FromAddress("E3", "Data", "Book.xlsx"), 2, Nothing, Nothing, SpreadsheetDateSystem.Unknown)
                Select Case kind
                    Case "book" : b.WorkbookName = "Other.xlsx"
                    Case "sheet" : b.WorksheetName = "Other"
                    Case "code" : b.WorksheetId = "OtherCode"
                    Case "revision" : b.SelectionRevision = "unverified"
                End Select
                Return New SpreadsheetRangeSetData With {.Ranges = New List(Of SpreadsheetRangeData) From {a, b}}
            End Function))
        Assert.AreEqual("transport.identity", errorValue.Report.Issues(0).Code)
    End Sub

    <DataTestMethod> <DataRow("nan")> <DataRow("infinity")> <DataRow("object")>
    Public Sub NonfiniteOrUnsupportedValuesNeverProduceAnAcceptedTable(kind As String)
        Dim value As Object = If(kind = "nan", CType(Double.NaN, Object), If(kind = "infinity", CType(Double.PositiveInfinity, Object), New Object()))
        If kind = "object" Then
            Assert.ThrowsException(Of ArgumentException)(Function() ImportFixture({"B3"}, {New Object(,) {{value}}}))
        Else
            Dim rejected = Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() ImportFixture({"B3"}, {New Object(,) {{value}}}))
            Assert.AreEqual("transport.cell", rejected.Report.Issues(0).Code)
            Assert.AreEqual(3, rejected.Report.Issues(0).WorksheetRow)
        End If
    End Sub

    <TestMethod>
    Public Sub MergePredicateAndMetadataBudgetRejectBeforeValueCallbacks()
        ExcelDnaDataImporter.ValidatePhysicalMergeState(False)
        For Each value As Object In New Object() {True, Nothing, DBNull.Value, "False", 0}
            Dim captured = value
            Assert.ThrowsException(Of ArgumentException)(Sub() ExcelDnaDataImporter.ValidatePhysicalMergeState(captured))
        Next
        Dim reads As New List(Of String)
        Dim size = Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() ImportFixture({"B1:B1048576"}, {New Object(,) {{1}}}, reads))
        Assert.AreEqual("input.range-cells", size.Report.Issues(0).Code)
        Assert.AreEqual(0, reads.Count)
        Assert.ThrowsException(Of ArgumentException)(Function() ImportFixture({"B3"}, {New Object(,) {{1}}}, merge:=True))
        Assert.AreEqual(0, reads.Count)
    End Sub

    <TestMethod>
    Public Sub EmptyInputDisconnectedReaderAndStaleSelectionFailAtomically()
        Dim calls As Integer
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaDataImporter.ImportPhysicalSelection("original", New String() {},
            Function(addresses)
                calls += 1
                Return Nothing
            End Function))
        Assert.AreEqual(0, calls)
        Assert.ThrowsException(Of InvalidOperationException)(Function() ExcelDnaDataImporter.ImportPhysicalSelection("original", {"B3"},
            Function(addresses) As SpreadsheetRangeSetData
                Throw New InvalidOperationException("synthetic disconnected acquisition")
            End Function))
        Assert.ThrowsException(Of ArgumentException)(Function() ImportFixture({"B3", "E3"}, {New Object(,) {{1}}, New Object(,) {{2}}},
            mutate:=Sub(snapshot, index) snapshot.StartRow = 99))
        Dim missing = Assert.ThrowsException(Of SpreadsheetInputValidationException)(Function() ExcelDnaDataImporter.ImportPhysicalSelection("original", {"B3"}, Function(a) Nothing))
        Assert.AreEqual("transport.ranges", missing.Report.Issues(0).Code)
    End Sub

    <TestMethod>
    Public Sub OriginalAddressesDetachedBeforeResolverAndSidecarIsPerTable()
        Dim input = New String() {"B3"}
        Dim table = ExcelDnaDataImporter.ImportPhysicalSelection("original", input,
            Function(addresses)
                input(0) = "Z99"
                Assert.AreEqual("B3", addresses(0))
                Dim snapshot = ExcelDnaSharedApplicationAdapter.SnapshotValues(SpreadsheetRangeReference.FromAddress(addresses(0), "Data", "Book.xlsx"), 3, Nothing, Nothing, SpreadsheetDateSystem.Unknown)
                Return New SpreadsheetRangeSetData With {.Ranges = New List(Of SpreadsheetRangeData) From {snapshot}}
            End Function)
        Dim other = ImportFixture({"G8"}, {New Object(,) {{8}}})
        Assert.AreEqual(3, ExcelDnaDataImporter.GetPhysicalImportEvidence(table).Data.SourceAt(0, 0).WorksheetRow)
        Assert.AreEqual(8, ExcelDnaDataImporter.GetPhysicalImportEvidence(other).Data.SourceAt(0, 0).WorksheetRow)
        ExcelDnaDataImporter.GetPhysicalImportEvidence(table).Data.Data.Rows(0)(0).NumberValue = 999
        Assert.AreEqual(3.0R, table.NumericMatrix(0, 0))
        Assert.AreEqual(3.0R, ExcelDnaDataImporter.GetPhysicalImportEvidence(table).Data.Data.Rows(0)(0).NumberValue)
    End Sub

    <TestMethod>
    Public Sub AllBlankCellsRemainMissingAndExistingPairedSelectorPreventsStatistics()
        Dim table = ImportFixture({"B3:B4", "E3:E4"}, {New Object(,) {{Nothing}, {DBNull.Value}}, New Object(,) {{ExcelEmpty.Value}, {ExcelMissing.Value}}})
        CollectionAssert.AreEqual(New String() {"VarB", "VarE"}, table.ColumnNames)
        Assert.IsTrue(table.MissingMask.Cast(Of Boolean)().All(Function(m) m))
        Dim data As New DataObj
        data.LoadCoreDataTable(table, cloneTable:=False)
        Assert.IsTrue(data.bZeroValid)
        Dim errorText As String = ""
        Assert.IsNull(WindowsPairedInputAdapter.SelectPairs(data, errorText))
        Assert.IsFalse(String.IsNullOrEmpty(errorText))
        Assert.AreEqual(SpreadsheetCellKind.Blank, ExcelDnaDataImporter.GetPhysicalImportEvidence(table).Data.Data.Rows(0)(1).Kind)
    End Sub

    <TestMethod>
    Public Sub GuiAndUdfCellsAndCoreShapeAgreeWithoutFabricatingArrayCoordinates()
        Dim values As Object(,) = {{1.5R, 2.5R}, {3.5R, 4.5R}}
        Dim table = ImportFixture({"B3:C4"}, {values})
        Dim udf = WindowsSpreadsheetTransport.NumericFrame(values)
        CollectionAssert.AreEqual(New Integer() {0, 0}, udf.RowIds.ToArray())
        CollectionAssert.AreEqual(New Integer() {3, 4}, table.RowIds)
        CollectionAssert.AreEqual(table.NumericMatrix.Cast(Of Double)().ToArray(), WindowsSpreadsheetTransport.NumericMatrix(values).Cast(Of Double)().ToArray())
        Dim gui = ExcelDnaDataImporter.GetPhysicalImportEvidence(table).Data
        Assert.AreEqual(gui.Data.Rows(0)(0).Kind, udf.Data.Rows(0)(0).Kind)
        Assert.AreEqual(3, gui.SourceAt(0, 0).WorksheetRow)
        Assert.AreEqual(0, udf.SourceAt(0, 0).WorksheetRow)
    End Sub
End Class
