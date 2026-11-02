Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PhysicalImporter27ColumnNameTests
    <DataTestMethod>
    <DataRow(1, "A")> <DataRow(26, "Z")> <DataRow(27, "AA")>
    <DataRow(702, "ZZ")> <DataRow(16384, "XFD")>
    Public Sub UsesExistingA1FormatterWithoutAnActiveWorksheet(column As Integer, expected As String)
        Assert.AreEqual(expected, SpreadsheetRangeAddress.ColumnLetters(column))
        Dim address = SpreadsheetRangeAddress.Parse(expected & "7", "Data")
        Assert.AreEqual(column, address.StartColumn)
    End Sub

    <DataTestMethod> <DataRow(-1)> <DataRow(0)> <DataRow(16385)>
    Public Sub InvalidExcelColumnCannotInventLabel(column As Integer)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() SpreadsheetRangeAddress.ColumnLetters(column))
    End Sub
End Class
