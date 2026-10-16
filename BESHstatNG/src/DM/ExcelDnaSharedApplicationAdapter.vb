Option Explicit On
Option Strict On

''' <summary>
''' Windows Excel-DNA adapter into the host-neutral BESHStat.Excel.Common application layer.
''' The existing Windows importer already produces <see cref="MultiGroupsUnpairedData"/>,
''' so this adapter converts that host object to the shared grouped numeric contract without
''' introducing any Excel Interop dependency into the common project.
''' </summary>
Public NotInheritable Class ExcelDnaSharedApplicationAdapter

    Private Sub New()
    End Sub

    ''' <summary>Capture identity and coordinates of an existing Excel COM range.
    ''' This does not read cell values or replace the existing Windows importer.</summary>
    Public Shared Function ToRangeReference(source As Microsoft.Office.Interop.Excel.Range) As Global.BESHStatNG.ExcelCommon.SpreadsheetRangeReference
        If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
        If source.Areas.Count <> 1 Then Throw New ArgumentException("Use one rectangular area per range reference.", NameOf(source))
        Dim sheet As Microsoft.Office.Interop.Excel.Worksheet = DirectCast(source.Worksheet, Microsoft.Office.Interop.Excel.Worksheet)
        Dim book As Microsoft.Office.Interop.Excel.Workbook = DirectCast(sheet.Parent, Microsoft.Office.Interop.Excel.Workbook)
        Dim localAddress As String = CStr(source.Address(False, False))
        Dim result As Global.BESHStatNG.ExcelCommon.SpreadsheetRangeReference =
            Global.BESHStatNG.ExcelCommon.SpreadsheetRangeReference.FromAddress(localAddress, CStr(sheet.Name), CStr(book.Name))
        result.WorksheetId = CStr(sheet.CodeName)
        ' WorkbookId is deliberately unset: Excel COM does not provide a durable universal workbook ID.
        Return result
    End Function

    Public Shared Function ToGroupedNumericData(data As MultiGroupsUnpairedData) As Global.BESHStatNG.ExcelCommon.GroupedNumericData
        If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
        If data.X Is Nothing Then Throw New ArgumentException("Grouped data are missing.", NameOf(data))
        If data.varNames Is Nothing Then Throw New ArgumentException("Group names are missing.", NameOf(data))
        If data.X.Length <> data.varNames.Length Then
            Throw New ArgumentException("The number of groups and group names must match.", NameOf(data))
        End If
        If data.X.Length < 2 Then
            Throw New ArgumentException("At least two groups are required.", NameOf(data))
        End If

        Dim names As String() = DirectCast(data.varNames.Clone(), String())
        Dim groups(data.X.Length - 1)() As Double

        For i As Integer = 0 To data.X.Length - 1
            If data.X(i) Is Nothing Then
                Throw New ArgumentException("Group '" & names(i) & "' has no data array.", NameOf(data))
            End If

            groups(i) = DirectCast(data.X(i).Clone(), Double())
        Next

        Return New Global.BESHStatNG.ExcelCommon.GroupedNumericData(names, groups)
    End Function

End Class
