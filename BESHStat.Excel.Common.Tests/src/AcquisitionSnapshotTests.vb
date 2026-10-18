Option Explicit On
Option Strict On

Imports System
Imports System.Text.Json
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AcquisitionTestSupport
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AcquisitionSnapshotTests
    <TestMethod>
    Public Sub CanonicalTwoRanges_RoundTripPreservesOrderAndAllSourceCells()
        Dim expected = AcquisitionFixture.Load()
        Dim actual = JsonSerializer.Deserialize(Of SpreadsheetRangeSetData)(JsonSerializer.Serialize(expected))
        Assert.AreEqual("fixture-request", actual.SelectionId)
        Assert.AreEqual(2, actual.Ranges.Count)
        For index As Integer = 0 To 1
            AcquisitionFixture.AssertCommonSnapshot(expected.Ranges(index), actual.Ranges(index))
            Assert.AreEqual(SpreadsheetDateSystem.Unknown, actual.Ranges(index).DateSystem)
            Assert.IsFalse(actual.Ranges(index).DateSerialOrigin.HasValue)
        Next
        Assert.AreEqual("Study Data", actual.Ranges(0).WorksheetName)
        Assert.AreEqual("Follow up", actual.Ranges(1).WorksheetName)
    End Sub

    <TestMethod>
    Public Sub CanonicalMixedCells_BridgeKeepsMissingnessTypesAndOriginalRows()
        Dim source = AcquisitionFixture.Load().Ranges(0)
        Dim table = SpreadsheetCoreTableBridge.ToCoreDataTable(source, {"A", "B", "C", "D"},
            New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.Preserve})
        CollectionAssert.AreEqual(New Integer() {4, 5}, table.RowIds)
        Assert.AreEqual(3, table.SourceInfo.FirstSourceColumn)
        Assert.AreEqual("'Study Data'!C4:F5", table.SourceInfo.Address)
        Assert.IsTrue(table.MissingMask(0, 0))
        Assert.IsFalse(table.MissingMask(0, 1))
        Assert.AreEqual("", DirectCast(table.ObjectMatrix(0, 1), String))
        Assert.AreEqual(0.0R, DirectCast(table.ObjectMatrix(0, 2), Double))
        Assert.AreEqual(False, DirectCast(table.ObjectMatrix(0, 3), Boolean))
        Assert.AreEqual("#N/A", DirectCast(table.ObjectMatrix(1, 0), String))
        Assert.IsInstanceOfType(Of SpreadsheetExcelErrorValue)(table.ObjectMatrix(1, 2))
        Assert.AreEqual(SpreadsheetCellKind.CellError, source.Rows(1)(2).Kind)
        Assert.AreEqual("yyyy-mm-dd", source.Rows(1)(1).NumberFormat)
    End Sub

    <TestMethod>
    Public Sub CanonicalErrors_RequireExplicitMissingPolicyWithoutRewritingSource()
        Dim source = AcquisitionFixture.Load().Ranges(0)
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCoreTableBridge.ToCoreDataTable(source, {"A", "B", "C", "D"}))
        source.Rows(1)(2).NativeErrorCode = "host-supplied-error"
        Dim table = SpreadsheetCoreTableBridge.ToCoreDataTable(source, {"A", "B", "C", "D"},
            New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.TreatAsMissing})
        Assert.IsTrue(table.MissingMask(1, 2))
        Assert.AreEqual("#N/A", source.Rows(1)(2).TextValue)
        Assert.AreEqual("host-supplied-error", source.Rows(1)(2).NativeErrorCode)
        Assert.IsFalse(table.MissingMask(1, 0))
    End Sub

    <TestMethod>
    Public Sub CanonicalDateFormattedNumber_RequiresVerifiedBasisOrExplicitOrigin()
        Dim source = AcquisitionFixture.Load().Ranges(0)
        Dim cell = source.Rows(1)(1)
        Assert.IsFalse(SpreadsheetCellValuePolicy.Evaluate(cell).HasDateValue)
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetCellValuePolicy.Evaluate(cell, Nothing, source.DateSystem, True))
        Dim explicitOrigin As New DateTime(2000, 1, 1)
        source.DateSerialOrigin = explicitOrigin
        Dim interpreted = SpreadsheetCellValuePolicy.Evaluate(cell, Nothing, source.DateSystem, True, source.DateSerialOrigin)
        Assert.AreEqual(explicitOrigin.AddDays(61), interpreted.DateValue)
        Assert.AreEqual(SpreadsheetCellKind.Number, cell.Kind)
        Assert.AreEqual(61.0R, cell.NumberValue)
    End Sub

    <TestMethod>
    Public Sub CanonicalTwoRangeReferences_KeepProvenanceAndRejectFalseRectangle()
        Dim source = AcquisitionFixture.Load()
        Dim selection As New SpreadsheetRangeSelection()
        For Each snapshot In source.Ranges
            selection.Ranges.Add(snapshot.ToReference())
        Next
        selection.Validate()
        Assert.AreEqual(4, selection.Ranges(0).StartRow)
        Assert.AreEqual(9, selection.Ranges(1).StartRow)
        Assert.AreEqual("second-input", selection.Ranges(1).SelectionId)
        selection.Ranges(0).Address = "C4:F5,H9:I10"
        Assert.ThrowsException(Of ArgumentException)(Sub() selection.Validate())
    End Sub
End Class
