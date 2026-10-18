Option Explicit On
Option Strict On

Imports System
Imports System.IO
Imports System.Runtime.Serialization.Json
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace Global.BESHStatNG.AcquisitionTestSupport
    ' Linked into modern Common and legacy Windows tests; the same JSON also drives JS tests.
    Public NotInheritable Class AcquisitionFixture
        Private Sub New()
        End Sub

        Public Shared Function Load() As SpreadsheetRangeSetData
            Using stream As Stream = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Acquisition", "canonical-two-ranges.json"))
                Return DirectCast(New DataContractJsonSerializer(GetType(SpreadsheetRangeSetData)).ReadObject(stream), SpreadsheetRangeSetData)
            End Using
        End Function

        Public Shared Sub AssertCommonSnapshot(expected As SpreadsheetRangeData, actual As SpreadsheetRangeData)
            Assert.AreEqual(expected.WorkbookId, actual.WorkbookId)
            Assert.AreEqual(expected.WorkbookName, actual.WorkbookName)
            Assert.AreEqual(expected.WorksheetName, actual.WorksheetName)
            Assert.AreEqual(expected.Address, actual.Address)
            Assert.AreEqual(expected.AddressDialect, actual.AddressDialect)
            Assert.AreEqual(expected.StartRow, actual.StartRow)
            Assert.AreEqual(expected.StartColumn, actual.StartColumn)
            Assert.AreEqual(expected.RowCount, actual.RowCount)
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount)
            Assert.AreEqual(expected.SelectionId, actual.SelectionId)
            Assert.AreEqual(expected.SelectionRevision, actual.SelectionRevision)
            Assert.AreEqual(expected.Rows.Count, actual.Rows.Count)
            For row As Integer = 0 To expected.RowCount - 1
                Assert.AreEqual(expected.Rows(row).Count, actual.Rows(row).Count)
                For column As Integer = 0 To expected.ColumnCount - 1
                    Dim left As SpreadsheetCellValue = expected.Rows(row)(column)
                    Dim right As SpreadsheetCellValue = actual.Rows(row)(column)
                    Assert.AreEqual(left.Kind, right.Kind)
                    Assert.AreEqual(left.NumberValue, right.NumberValue)
                    Assert.AreEqual(left.BooleanValue, right.BooleanValue)
                    Assert.AreEqual(left.TextValue, right.TextValue)
                    Assert.AreEqual(left.NumberFormat, right.NumberFormat)
                Next
            Next
            ' Worksheet IDs, date basis and native error codes are host-specific and tested separately.
        End Sub
    End Class
End Namespace
