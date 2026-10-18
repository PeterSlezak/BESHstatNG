Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Runtime.InteropServices
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop.Excel
Imports BESHStatNG.ExcelCommon

''' <summary>
''' Windows Excel-DNA adapter into the host-neutral BESHStat.Excel.Common application layer.
''' Acquires lossless rectangular snapshots and converts legacy grouped data without
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

    ''' <summary>Lossless raw acquisition, separate from legacy error-to-missing import.</summary>
    Public Shared Function ToRangeData(source As Range) As SpreadsheetRangeData
        Dim reference As SpreadsheetRangeReference = ToRangeReference(source)
        Dim sheet As Worksheet = DirectCast(source.Worksheet, Worksheet)
        Dim book As Workbook = DirectCast(sheet.Parent, Workbook)
        ' Value2 avoids Currency/Date coercion. Number formats remain hints, never types.
        Dim values As Object = source.Value2
        Return SnapshotValues(reference, values,
            Function(row, column) CStr(DirectCast(source.Cells.Item(row, column), Range).NumberFormat),
            Function(row, column) CStr(DirectCast(source.Cells.Item(row, column), Range).Text),
            If(book.Date1904, SpreadsheetDateSystem.Excel1904, SpreadsheetDateSystem.Excel1900))
    End Function

    ''' <summary>Each area remains a separate rectangle in host enumeration order.</summary>
    Public Shared Function ToRangeSetData(sources As IEnumerable(Of Range)) As SpreadsheetRangeSetData
        If sources Is Nothing Then Throw New ArgumentNullException(NameOf(sources))
        Dim result As New SpreadsheetRangeSetData()
        For Each source As Range In sources
            If source Is Nothing Then Throw New ArgumentException("A source range is missing.", NameOf(sources))
            For index As Integer = 1 To source.Areas.Count
                result.Ranges.Add(ToRangeData(source.Areas.Item(index)))
            Next
        Next
        If result.Ranges.Count = 0 Then Throw New ArgumentException("At least one range is required.", NameOf(sources))
        Return result
    End Function

    ''' <summary>Read explicit ordered rectangles, including different sheets in the supplied workbook.</summary>
    Public Shared Function ToRangeSetData(selection As SpreadsheetRangeSelection, book As Workbook) As SpreadsheetRangeSetData
        If book Is Nothing Then Throw New ArgumentNullException(NameOf(book))
        Return SnapshotSelection(selection,
            Function(reference)
                If reference.WorkbookName.Length > 0 AndAlso Not String.Equals(reference.WorkbookName, book.Name, StringComparison.OrdinalIgnoreCase) Then
                    Throw New ArgumentException("The requested workbook is not the supplied workbook.")
                End If
                Dim sheet As Worksheet = DirectCast(book.Worksheets.Item(reference.WorksheetName), Worksheet)
                Dim source As Range = sheet.Range(reference.Address)
                VerifyReference(reference, ToRangeReference(source))
                Return ToRangeData(source)
            End Function)
    End Function

    ' The callback isolates COM from controlled host-value tests without introducing a provider hierarchy.
    Friend Shared Function SnapshotSelection(selection As SpreadsheetRangeSelection,
                                            acquire As Func(Of SpreadsheetRangeReference, SpreadsheetRangeData)) As SpreadsheetRangeSetData
        If selection Is Nothing Then Throw New ArgumentNullException(NameOf(selection))
        selection.Validate()
        ' Validate every request before accessing COM or reading any cell values.
        Dim references As New List(Of SpreadsheetRangeReference)()
        For Each reference As SpreadsheetRangeReference In selection.Ranges
            references.Add(ValidateAcquisitionReference(reference))
        Next
        If acquire Is Nothing Then Throw New ArgumentNullException(NameOf(acquire))
        Dim result As New SpreadsheetRangeSetData With {.SelectionId = If(selection.SelectionId, String.Empty)}
        For Each reference As SpreadsheetRangeReference In references
            Dim snapshot As SpreadsheetRangeData = acquire(reference)
            VerifyReference(reference, snapshot.ToReference())
            ' SelectionId is caller correlation metadata, not a host-issued revision token.
            snapshot.SelectionId = If(reference.SelectionId, String.Empty)
            result.Ranges.Add(snapshot)
        Next
        Return result
    End Function

    Friend Shared Function ValidateAcquisitionReference(reference As SpreadsheetRangeReference) As SpreadsheetRangeReference
        If reference Is Nothing Then Throw New ArgumentNullException(NameOf(reference))
        If reference.AddressDialect <> SpreadsheetAddressDialect.ExcelA1 Then Throw New ArgumentException("Windows acquisition requires Excel A1 references.")
        If Not String.IsNullOrEmpty(reference.WorkbookId) Then Throw New ArgumentException("A workbook ID cannot be verified by this Windows adapter.")
        If Not String.IsNullOrEmpty(reference.SelectionRevision) Then Throw New ArgumentException("A selection revision cannot be verified by this Windows adapter.")
        Dim normalized As SpreadsheetRangeReference = SpreadsheetRangeReference.FromAddress(reference.Address, reference.WorksheetName, reference.WorkbookName)
        If String.IsNullOrWhiteSpace(normalized.WorksheetName) Then Throw New ArgumentException("A worksheet name is required.")
        CheckCoordinate(reference.StartRow, normalized.StartRow)
        CheckCoordinate(reference.StartColumn, normalized.StartColumn)
        CheckCoordinate(reference.RowCount, normalized.RowCount)
        CheckCoordinate(reference.ColumnCount, normalized.ColumnCount)
        normalized.WorksheetId = If(reference.WorksheetId, String.Empty)
        normalized.SelectionId = If(reference.SelectionId, String.Empty)
        Return normalized
    End Function

    Private Shared Sub CheckCoordinate(requested As Integer, actual As Integer)
        If requested <> 0 AndAlso requested <> actual Then Throw New ArgumentException("Source coordinates conflict with the requested address.")
    End Sub

    Friend Shared Sub VerifyReference(requested As SpreadsheetRangeReference, actual As SpreadsheetRangeReference)
        If Not String.Equals(requested.WorksheetName, actual.WorksheetName, StringComparison.OrdinalIgnoreCase) OrElse
           (requested.WorksheetId.Length > 0 AndAlso Not String.Equals(requested.WorksheetId, actual.WorksheetId, StringComparison.Ordinal)) OrElse
           (requested.WorkbookName.Length > 0 AndAlso Not String.Equals(requested.WorkbookName, actual.WorkbookName, StringComparison.OrdinalIgnoreCase)) OrElse
           requested.StartRow <> actual.StartRow OrElse requested.StartColumn <> actual.StartColumn OrElse
           requested.RowCount <> actual.RowCount OrElse requested.ColumnCount <> actual.ColumnCount Then
            Throw New ArgumentException("The source identity or coordinates changed; refresh the selection.")
        End If
    End Sub

    ' Controlled Value2 fixtures exercise exactly the conversion used by live COM acquisition.
    ' Array lower bounds are host-defined; scalar Value2 is valid only for a single cell.
    Friend Shared Function SnapshotValues(reference As SpreadsheetRangeReference, values As Object,
                                         numberFormatAt As Func(Of Integer, Integer, String),
                                         errorTextAt As Func(Of Integer, Integer, String),
                                         dateSystem As SpreadsheetDateSystem) As SpreadsheetRangeData
        Dim normalized As SpreadsheetRangeReference = ValidateAcquisitionReference(reference)
        Dim matrix As Array = TryCast(values, Array)
        If matrix IsNot Nothing Then
            If matrix.Rank <> 2 OrElse matrix.GetLength(0) <> normalized.RowCount OrElse matrix.GetLength(1) <> normalized.ColumnCount Then
                Throw New ArgumentException("Host values do not match the range dimensions.", NameOf(values))
            End If
        ElseIf normalized.RowCount <> 1 OrElse normalized.ColumnCount <> 1 Then
            Throw New ArgumentException("A multi-cell range requires a two-dimensional host value array.", NameOf(values))
        End If
        Dim result As New SpreadsheetRangeData With {
            .WorkbookName = normalized.WorkbookName, .WorksheetName = normalized.WorksheetName,
            .WorksheetId = normalized.WorksheetId, .Address = normalized.Address,
            .StartRow = normalized.StartRow, .StartColumn = normalized.StartColumn,
            .RowCount = normalized.RowCount, .ColumnCount = normalized.ColumnCount,
            .DateSystem = dateSystem, .SelectionId = normalized.SelectionId,
            .SelectionRevision = String.Empty
        }
        For row As Integer = 1 To result.RowCount
            Dim cells As New List(Of SpreadsheetCellValue)()
            For column As Integer = 1 To result.ColumnCount
                Dim value As Object = If(matrix Is Nothing, values,
                    matrix.GetValue(matrix.GetLowerBound(0) + row - 1, matrix.GetLowerBound(1) + column - 1))
                Dim cell As SpreadsheetCellValue = ToSnapshotCell(value, errorTextAt, row, column)
                cell.NumberFormat = If(numberFormatAt Is Nothing, String.Empty, If(numberFormatAt(row, column), String.Empty))
                cells.Add(cell)
            Next
            result.Rows.Add(cells)
        Next
        Return result
    End Function

    Private Shared Function ToSnapshotCell(value As Object, errorTextAt As Func(Of Integer, Integer, String),
                                          row As Integer, column As Integer) As SpreadsheetCellValue
        If value Is Nothing OrElse TypeOf value Is DBNull OrElse TypeOf value Is ExcelEmpty Then Return SpreadsheetCellValue.BlankCell()
        Dim nativeCode As String = Nothing
        If TypeOf value Is ExcelError Then
            nativeCode = CInt(DirectCast(value, ExcelError)).ToString(CultureInfo.InvariantCulture)
        ElseIf TypeOf value Is ErrorWrapper Then
            nativeCode = DirectCast(value, ErrorWrapper).ErrorCode.ToString(CultureInfo.InvariantCulture)
        ElseIf TypeOf value Is Integer AndAlso (CInt(value) And &HFFFF0000) = &H800A0000 Then
            ' VT_ERROR values returned by Excel COM use the FACILITY_CONTROL HRESULT.
            ' Preserve the supplied HRESULT, including newer Excel errors, without an error-name lookup.
            nativeCode = CInt(value).ToString(CultureInfo.InvariantCulture)
        End If
        If nativeCode IsNot Nothing Then
            Return SpreadsheetCellValue.ErrorCell(If(errorTextAt Is Nothing, String.Empty, errorTextAt(row, column)), nativeCode)
        End If
        If TypeOf value Is String Then Return SpreadsheetCellValue.TextCell(DirectCast(value, String))
        If TypeOf value Is Boolean Then Return SpreadsheetCellValue.BooleanCell(CBool(value))
        Select Case Type.GetTypeCode(value.GetType())
            Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32,
                 TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                Return SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, CultureInfo.InvariantCulture))
            Case Else
                Throw New ArgumentException("Unsupported host cell value type: " & value.GetType().FullName)
        End Select
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
