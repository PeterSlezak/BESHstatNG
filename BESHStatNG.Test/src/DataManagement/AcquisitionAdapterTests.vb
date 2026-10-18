Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports ExcelDna.Integration
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AcquisitionTestSupport
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AcquisitionAdapterTests
    Private Shared Function Capture(expected As SpreadsheetRangeData, values As Object) As SpreadsheetRangeData
        Dim reference = expected.ToReference()
        reference.WorksheetId = "windows-" & expected.WorksheetName
        Return ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, values,
            Function(row, column) expected.Rows(row - 1)(column - 1).NumberFormat,
            Function(row, column) expected.Rows(row - 1)(column - 1).TextValue,
            SpreadsheetDateSystem.Excel1900)
    End Function

    <TestMethod>
    Public Sub WindowsTwoRangeAcquisition_MatchesSharedOfficeCanonicalFixture()
        Dim expected = AcquisitionFixture.Load()
        Dim selection As New SpreadsheetRangeSelection With {.SelectionId = expected.SelectionId}
        For Each source In expected.Ranges
            selection.Ranges.Add(source.ToReference())
        Next
        Dim calls As New List(Of String)()
        Dim actual = ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
            Function(reference)
                calls.Add(reference.WorksheetName)
                If reference.WorksheetName = "Study Data" Then
                    Dim values(,) As Object = {{Nothing, "", 0.0R, False}, {"#N/A", 61.0R, New ErrorWrapper(-2146826246), 7.5R}}
                    Return Capture(expected.Ranges(0), values)
                End If
                Dim followup(,) As Object = {{True, " label "}, {Nothing, ""}}
                Return Capture(expected.Ranges(1), followup)
            End Function)
        CollectionAssert.AreEqual(New String() {"Study Data", "Follow up"}, calls)
        Assert.AreEqual(expected.SelectionId, actual.SelectionId)
        Assert.AreEqual(2, actual.Ranges.Count)
        For index As Integer = 0 To 1
            AcquisitionFixture.AssertCommonSnapshot(expected.Ranges(index), actual.Ranges(index))
            Assert.AreEqual("windows-" & expected.Ranges(index).WorksheetName, actual.Ranges(index).WorksheetId)
            Assert.AreEqual(SpreadsheetDateSystem.Excel1900, actual.Ranges(index).DateSystem)
        Next
        Assert.AreEqual("-2146826246", actual.Ranges(0).Rows(1)(2).NativeErrorCode)
    End Sub

    <TestMethod>
    Public Sub WindowsDisjointSameSheetAcquisition_KeepsRequestOrderAndBlankEdges()
        Dim selection As New SpreadsheetRangeSelection()
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("H9:I10", "Data"))
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("B3:C4", "Data"))
        Dim actual = ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
            Function(reference) ExcelDnaSharedApplicationAdapter.SnapshotValues(reference,
                New Object(,) {{Nothing, ""}, {Nothing, Nothing}}, Nothing, Nothing, SpreadsheetDateSystem.Excel1904))
        Assert.AreEqual("H9:I10", actual.Ranges(0).Address)
        Assert.AreEqual(9, actual.Ranges(0).StartRow)
        Assert.AreEqual("B3:C4", actual.Ranges(1).Address)
        Assert.AreEqual(2, actual.Ranges(1).RowCount)
        Assert.AreEqual(SpreadsheetCellKind.Blank, actual.Ranges(1).Rows(0)(0).Kind)
        Assert.AreEqual(SpreadsheetCellKind.Text, actual.Ranges(1).Rows(0)(1).Kind)
    End Sub

    <TestMethod>
    Public Sub WindowsValue2Array_RespectsNonzeroLowerBoundsAndCopiesValues()
        Dim reference = SpreadsheetRangeReference.FromAddress("D7:E8", "Data", "Book.xlsx")
        Dim values = Array.CreateInstance(GetType(Object), {2, 2}, {1, 1})
        values.SetValue(5.0R, 1, 1)
        values.SetValue("", 1, 2)
        values.SetValue(False, 2, 1)
        Dim actual = ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, values, Nothing, Nothing, SpreadsheetDateSystem.Unknown)
        values.SetValue(99.0R, 1, 1)
        Assert.AreEqual(5.0R, actual.Rows(0)(0).NumberValue)
        Assert.AreEqual(SpreadsheetCellKind.Blank, actual.Rows(1)(1).Kind)
        Assert.AreEqual(7, actual.StartRow)
        Assert.AreEqual(4, actual.StartColumn)
    End Sub

    <TestMethod>
    Public Sub WindowsSingleCell_PreservesErrorWrapperExcelDnaErrorAndComHresult()
        Dim reference = SpreadsheetRangeReference.FromAddress("A1", "Data")
        Dim values As Object() = {New ErrorWrapper(-2146826281), ExcelError.ExcelErrorDiv0, -2146826281}
        Dim codes As String() = {"-2146826281", "7", "-2146826281"}
        For index As Integer = 0 To values.Length - 1
            Dim actual = ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, values(index), Nothing,
                Function(row, column) "#DIV/0!", SpreadsheetDateSystem.Unknown)
            Assert.AreEqual(SpreadsheetCellKind.CellError, actual.Rows(0)(0).Kind)
            Assert.AreEqual("#DIV/0!", actual.Rows(0)(0).TextValue)
            Assert.AreEqual(codes(index), actual.Rows(0)(0).NativeErrorCode)
        Next
    End Sub

    <TestMethod>
    Public Sub WindowsNumbers_AreNotMistakenForErrorCodesOrFormattedDates()
        Dim reference = SpreadsheetRangeReference.FromAddress("A1:E1", "Data")
        Dim actual = ExcelDnaSharedApplicationAdapter.SnapshotValues(reference,
            New Object(,) {{2042, -2146826281.0R, 61.0R, 0.0R, False}}, Function(row, column) "yyyy-mm-dd", Nothing, SpreadsheetDateSystem.Excel1904)
        Assert.AreEqual(SpreadsheetCellKind.Number, actual.Rows(0)(0).Kind)
        Assert.AreEqual(SpreadsheetCellKind.Number, actual.Rows(0)(1).Kind)
        Assert.AreEqual(61.0R, actual.Rows(0)(2).NumberValue)
        Assert.AreEqual(SpreadsheetCellKind.Bool, actual.Rows(0)(4).Kind)
        Assert.IsFalse(actual.DateSerialOrigin.HasValue)
        Assert.AreEqual(New DateTime(1904, 3, 2), SpreadsheetCellValuePolicy.Evaluate(actual.Rows(0)(2), Nothing, actual.DateSystem, True).DateValue)
    End Sub

    <TestMethod>
    Public Sub WindowsDateSnapshots_Preserve1900PhantomSerialWithoutInterpretingTheFormat()
        Dim reference = SpreadsheetRangeReference.FromAddress("$c$4", "Data")
        Dim actual = ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, 60.0R,
            Function(row, column) "yyyy-mm-dd", Nothing, SpreadsheetDateSystem.Excel1900)
        Assert.AreEqual("C4:C4", actual.Address)
        Assert.AreEqual(SpreadsheetCellKind.Number, actual.Rows(0)(0).Kind)
        Assert.AreEqual(60.0R, actual.Rows(0)(0).NumberValue)
        Assert.IsFalse(SpreadsheetCellValuePolicy.Evaluate(actual.Rows(0)(0)).HasDateValue)
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.Evaluate(actual.Rows(0)(0), Nothing, actual.DateSystem, True))
    End Sub

    <TestMethod>
    Public Sub WindowsAcquisition_RejectsInvalidReferenceBeforeReadingAnyCells()
        Dim selection As New SpreadsheetRangeSelection()
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("A1", "Data"))
        selection.Ranges.Add(New SpreadsheetRangeReference With {.WorksheetName = "Data", .Address = "A1,C3"})
        Dim calls As Integer = 0
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
            Function(reference)
                calls += 1
                Return New SpreadsheetRangeData()
            End Function))
        Assert.AreEqual(0, calls)
    End Sub

    <TestMethod>
    Public Sub WindowsReference_RejectsUnverifiedIdentityRevisionAndConflictingCoordinates()
        Dim reference = SpreadsheetRangeReference.FromAddress("A1:B2", "Data")
        reference.WorkbookId = "invented"
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.ValidateAcquisitionReference(reference))
        reference.WorkbookId = ""
        reference.SelectionRevision = "unverified"
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.ValidateAcquisitionReference(reference))
        reference.SelectionRevision = ""
        reference.StartRow = 3
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.ValidateAcquisitionReference(reference))
        reference.StartRow = 1
        reference.AddressDialect = SpreadsheetAddressDialect.HostNative
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.ValidateAcquisitionReference(reference))
    End Sub

    <TestMethod>
    Public Sub WindowsReference_RejectsStaleWorksheetAndMismatchedHostRectangle()
        Dim requested = SpreadsheetRangeReference.FromAddress("A1:B2", "Data", "Book.xlsx")
        requested.WorksheetId = "original"
        Dim actual = SpreadsheetRangeReference.FromAddress("A1:B2", "Data", "Book.xlsx")
        actual.WorksheetId = "replaced"
        Assert.ThrowsException(Of ArgumentException)(Sub() ExcelDnaSharedApplicationAdapter.VerifyReference(requested, actual))
        actual.WorksheetId = "original"
        actual.StartRow = 5
        Assert.ThrowsException(Of ArgumentException)(Sub() ExcelDnaSharedApplicationAdapter.VerifyReference(requested, actual))
        actual.StartRow = 1
        actual.WorkbookName = "Other.xlsx"
        Assert.ThrowsException(Of ArgumentException)(Sub() ExcelDnaSharedApplicationAdapter.VerifyReference(requested, actual))
    End Sub

    <TestMethod>
    Public Sub WindowsValue2_RejectsMalformedMatricesAndUnsupportedHostTypes()
        Dim reference = SpreadsheetRangeReference.FromAddress("A1:B2", "Data")
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, 1.0R, Nothing, Nothing, SpreadsheetDateSystem.Unknown))
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, New Object() {1.0R, 2.0R}, Nothing, Nothing, SpreadsheetDateSystem.Unknown))
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, New Object(1, 0) {}, Nothing, Nothing, SpreadsheetDateSystem.Unknown))
        reference = SpreadsheetRangeReference.FromAddress("A1", "Data")
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, New DateTime(2020, 1, 1), Nothing, Nothing, SpreadsheetDateSystem.Unknown))
    End Sub

    <TestMethod>
    Public Sub LegacyNormalizeExcelValue_RetainsExistingErrorToMissingBehavior()
        Assert.IsNull(ExcelDnaDataImporter.NormalizeExcelValue(ExcelError.ExcelErrorNA))
        Assert.IsNull(ExcelDnaDataImporter.NormalizeExcelValue(ExcelEmpty.Value))
        Assert.IsNull(ExcelDnaDataImporter.NormalizeExcelValue(ExcelMissing.Value))
        Assert.AreEqual("", ExcelDnaDataImporter.NormalizeExcelValue(""))
        Assert.AreEqual(False, ExcelDnaDataImporter.NormalizeExcelValue(False))
        Assert.AreEqual(0.0R, ExcelDnaDataImporter.NormalizeExcelValue(0.0R))
    End Sub
End Class
