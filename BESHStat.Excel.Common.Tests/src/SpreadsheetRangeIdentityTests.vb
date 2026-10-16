Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text.Json
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SpreadsheetRangeIdentityTests
    <TestMethod>
    Public Sub A1Address_ParsesAbsoluteAndRelativeCoordinates()
        Dim parsed As SpreadsheetRangeAddress = SpreadsheetRangeAddress.Parse("$C$6:$F$17")
        Assert.AreEqual("C6:F17", parsed.Address)
        Assert.AreEqual(6, parsed.StartRow)
        Assert.AreEqual(3, parsed.StartColumn)
        Assert.AreEqual(12, parsed.RowCount)
        Assert.AreEqual(4, parsed.ColumnCount)
        Assert.AreEqual("'Study Data'!C6:F17", SpreadsheetRangeAddress.Parse("$C$6:$F$17", "Study Data").QualifiedAddress)
    End Sub

    <TestMethod>
    Public Sub A1Address_HandlesQuotedSheetAndEmbeddedApostrophe()
        Dim parsed As SpreadsheetRangeAddress = SpreadsheetRangeAddress.Parse("'O''Brien! Trial'!$B$2:$D$9")
        Assert.AreEqual("O'Brien! Trial", parsed.WorksheetName)
        Assert.AreEqual("'O''Brien! Trial'!B2:D9", parsed.QualifiedAddress)
        Assert.AreEqual(2, parsed.StartRow)
        Assert.AreEqual(2, parsed.StartColumn)
    End Sub

    <TestMethod>
    Public Sub A1Address_PreservesWorkbookQualifierAsMetadata()
        Dim parsed As SpreadsheetRangeAddress = SpreadsheetRangeAddress.Parse("'[Study.xlsx]Visit 2'!A1:C5")
        Assert.AreEqual("Study.xlsx", parsed.WorkbookName)
        Assert.AreEqual("Visit 2", parsed.WorksheetName)
        Assert.AreEqual("'[Study.xlsx]Visit 2'!A1:C5", parsed.QualifiedAddress)
    End Sub

    <TestMethod>
    Public Sub A1Address_AllowsSingleCellAndExcelLastCell()
        Dim sngle As SpreadsheetRangeAddress = SpreadsheetRangeAddress.Parse("xFd1048576")
        Assert.AreEqual("XFD1048576:XFD1048576", sngle.Address)
        Assert.AreEqual(16384, sngle.StartColumn)
        Assert.AreEqual(1048576, sngle.StartRow)
        Assert.AreEqual(1, sngle.RowCount)
    End Sub

    <TestMethod>
    Public Sub A1Address_RejectsMalformedReversedAndOutOfBounds()
        Dim malformed As String() = {"", "A0", "A1:B0", "A1:C", "A1,B2", "A1:B2:C3", "A9:B2", "E1:B4", "'Unclosed!A1", "Sheet1!Sheet2!A1"}
        For Each address As String In malformed
            Dim attempt As String = address
            Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetRangeAddress.Parse(attempt), "Expected malformed address rejection: " & attempt)
        Next

        Dim outOfBounds As String() = {"XFE1", "A1048577"}
        For Each address As String In outOfBounds
            Dim attempt As String = address
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() SpreadsheetRangeAddress.Parse(attempt), "Expected Excel bounds rejection: " & attempt)
        Next
    End Sub

    <TestMethod>
    Public Sub A1Address_RejectsConflictingSheetAndWorkbook()
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetRangeAddress.Parse("'Other'!A1", "Source"))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetRangeReference.FromAddress("'[Old.xlsx]Data'!A1", Nothing, "New.xlsx"))
    End Sub

    <TestMethod>
    Public Sub SourceFields_NormalizeQualifiedReferenceInEitherInput()
        Dim separate As SpreadsheetRangeReference = SpreadsheetRangeReference.FromUserInput("Hárok1", "A1:C4")
        Dim qualifiedAddress As SpreadsheetRangeReference = SpreadsheetRangeReference.FromUserInput("", "'Hárok1'!A1:C4")
        Dim duplicated As SpreadsheetRangeReference = SpreadsheetRangeReference.FromUserInput("'Hárok1'!A1:C4", "A1:C4")
        Dim qualifiedOnly As SpreadsheetRangeReference = SpreadsheetRangeReference.FromUserInput("'Hárok1'!A1:C4", "")
        Dim quotedName As SpreadsheetRangeReference = SpreadsheetRangeReference.FromUserInput("'Hárok1'", "A1:C4")
        Dim literalBang As SpreadsheetRangeReference = SpreadsheetRangeReference.FromUserInput("Study!Notes", "A1:C4")
        Assert.AreEqual("Study!Notes", literalBang.WorksheetName)
        Assert.AreEqual("A1:C4", literalBang.Address)

        Dim accepted As SpreadsheetRangeReference() = {separate, qualifiedAddress, duplicated, qualifiedOnly, quotedName}
        For Each item As SpreadsheetRangeReference In accepted
            Assert.AreEqual("Hárok1", item.WorksheetName)
            Assert.AreEqual("A1:C4", item.Address)
            Assert.AreEqual(1, item.StartRow)
            Assert.AreEqual(1, item.StartColumn)
            Assert.AreEqual(4, item.RowCount)
            Assert.AreEqual(3, item.ColumnCount)
        Next
    End Sub

    <TestMethod>
    Public Sub SourceFields_RejectConflictsBeforeHostAcquisition()
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetRangeReference.FromUserInput("'Hárok1'!A1:C4", "A2:C4"))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetRangeReference.FromUserInput("Hárok1", "'Hárok2'!A1:C4"))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetRangeReference.FromUserInput("Hárok1", ""))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetRangeReference.FromUserInput("", "A1:C4"))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetRangeReference.FromUserInput("'Hárok1", "A1:C4"))
        Assert.ThrowsException(Of ArgumentException)(Function() SpreadsheetRangeReference.FromUserInput("'Hárok1'!A1:C4", "'Hárok2'!A1:C4"))
    End Sub

    <TestMethod>
    Public Sub RangeSnapshot_LegacyPayloadStillValidAndTypedProvenanceOptional()
        Dim legacy As New SpreadsheetRangeData With {
            .WorksheetName = "Data", .Address = "A1:B3", .RowCount = 3, .ColumnCount = 2,
            .Rows = New List(Of List(Of SpreadsheetCellValue)) From {
                New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell("A"), SpreadsheetCellValue.TextCell("B")},
                New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(1), SpreadsheetCellValue.NumberCell(2)},
                New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(3), SpreadsheetCellValue.NumberCell(4)}
            }
        }
        Dim grouped As GroupedNumericData = GroupedNumericRangeParser.Parse(legacy)
        Assert.AreEqual(2, grouped.Groups.Length)
        Assert.AreEqual(0, legacy.StartRow)
        Assert.AreEqual("'Data'!A1:B3", legacy.QualifiedAddress)
        Dim source As SpreadsheetRangeReference = legacy.ToReference()
        Assert.AreEqual(1, source.StartRow)
        Assert.AreEqual(String.Empty, source.WorkbookId)
    End Sub

    <TestMethod>
    Public Sub RangeSnapshot_PreservesHostProvenanceWithoutChangingCells()
        Dim snapshot As New SpreadsheetRangeData With {
            .WorkbookName = "Workbook.xlsx", .WorksheetName = "Study Data", .WorksheetId = "ws_4",
            .Address = "C6:D8", .RowCount = 3, .ColumnCount = 2, .StartRow = 6, .StartColumn = 3,
            .SelectionId = "capture-1", .SelectionRevision = "1"
        }
        Dim reference As SpreadsheetRangeReference = snapshot.ToReference()
        Assert.AreEqual("'[Workbook.xlsx]Study Data'!C6:D8", reference.QualifiedAddress)
        Assert.AreEqual("ws_4", reference.WorksheetId)
        Assert.AreEqual("capture-1", reference.SelectionId)
        Assert.AreEqual(6, reference.StartRow)
        Assert.AreEqual(3, reference.StartColumn)
    End Sub

    <TestMethod>
    Public Sub RangeSelection_CollectsMultipleDifferentWorksheetsInOrder()
        Dim selection As New SpreadsheetRangeSelection With {.SelectionId = "selection-123"}
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("'Baseline'!A1:B4"))
        selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("'Follow up'!D2:E6"))
        selection.Validate()
        Assert.AreEqual(2, selection.Ranges.Count)
        Assert.AreEqual("Baseline", selection.Ranges(0).WorksheetName)
        Assert.AreEqual("Follow up", selection.Ranges(1).WorksheetName)
        Dim serialized As String = JsonSerializer.Serialize(selection)
        Dim restored As SpreadsheetRangeSelection = JsonSerializer.Deserialize(Of SpreadsheetRangeSelection)(serialized)
        Assert.IsNotNull(restored)
        Assert.AreEqual("Follow up", restored.Ranges(1).WorksheetName)
        Dim result As New SpreadsheetRangeSetData With {.SelectionId = selection.SelectionId}
        result.Ranges.Add(New SpreadsheetRangeData With {.WorksheetName = "Baseline"})
        result.Ranges.Add(New SpreadsheetRangeData With {.WorksheetName = "Follow up"})
        Assert.AreEqual("Follow up", result.Ranges(1).WorksheetName)
    End Sub

    <TestMethod>
    Public Sub RangeSelection_RejectsMissingOrUnqualifiedSources()
        Dim empty As New SpreadsheetRangeSelection()
        Assert.ThrowsException(Of ArgumentException)(Sub() empty.Validate())
        empty.Ranges.Add(SpreadsheetRangeReference.FromAddress("A1:C5"))
        Assert.ThrowsException(Of ArgumentException)(Sub() empty.Validate())
    End Sub
End Class
