Option Explicit On
Option Strict Off
Option Infer On

''' <summary>
''' Host-neutral adapter for tabular values received from an Office.js task pane/API call.
''' The VB.NET code does not reference Office.js directly; Office.js should serialize range values and metadata,
''' and this class converts that payload into a portable <see cref="CoreDataTable"/>.
''' </summary>
Public Class OfficeJsDataImportOptions
    Public Property HasHeaderRow As Boolean = True
    Public Property FirstSourceRow As Integer = 1
    Public Property FirstSourceColumn As Integer = 1
    Public Property SheetName As String = Nothing
    Public Property SourceAddress As String = Nothing
    Public Property SourceKind As String = "OfficeJsRangeValues"
End Class

Public Class OfficeJsDataImporter
    ''' <summary>
    ''' Imports a jagged/list row payload, such as the array-of-arrays JSON shape normally produced by Office.js,
    ''' and converts it to the rectangular matrix path used by the existing importer.
    ''' </summary>
    ''' <remarks>
    ''' Each row must be non-null and have the same number of columns. Use this overload for payloads such as
    ''' <c>Object()()</c>, <c>List(Of List(Of Object))</c>, or other enumerable row collections.
    ''' </remarks>
    Public Shared Function Import(rows As System.Collections.Generic.IEnumerable(Of System.Collections.Generic.IEnumerable(Of Object)), Optional options As OfficeJsDataImportOptions = Nothing) As CoreDataTable
        Return Import(RowsToRectangularMatrix(rows), options)
    End Function

    Public Shared Function Import(values(,) As Object, Optional options As OfficeJsDataImportOptions = Nothing) As CoreDataTable
        If options Is Nothing Then options = New OfficeJsDataImportOptions()
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))

        Dim totalRows As Integer = values.GetLength(0)
        Dim cols As Integer = values.GetLength(1)
        If totalRows < 1 OrElse cols < 1 Then Throw New ArgumentException("Office.js values must contain at least one row and one column.")

        Dim firstDataRow As Integer = If(options.HasHeaderRow, 1, 0)
        Dim dataRows As Integer = totalRows - firstDataRow
        If dataRows < 1 Then Throw New ArgumentException("Office.js values do not contain any data rows.")

        Dim names(cols - 1) As String
        For j As Integer = 0 To cols - 1
            If options.HasHeaderRow AndAlso Not CoreDataTable.IsMissingValue(values(0, j)) Then
                names(j) = CStr(values(0, j))
            Else
                names(j) = "Var" & CStr(j + 1)
            End If
        Next

        Dim body(dataRows - 1, cols - 1) As Object
        For i As Integer = 0 To dataRows - 1
            For j As Integer = 0 To cols - 1
                body(i, j) = values(i + firstDataRow, j)
            Next
        Next

        Dim firstSourceRow As Integer = Math.Max(1, options.FirstSourceRow + firstDataRow)
        Dim source As New DataSourceInfo With {
            .SourceKind = If(String.IsNullOrWhiteSpace(options.SourceKind), "OfficeJsRangeValues", options.SourceKind),
            .Address = options.SourceAddress,
            .SheetName = options.SheetName,
            .FirstSourceRow = firstSourceRow,
            .FirstSourceColumn = Math.Max(1, options.FirstSourceColumn),
            .ColumnNames = names
        }

        Return CoreDataTable.FromObjectMatrix(body, names, firstSourceRow:=firstSourceRow, sourceInfo:=source, copyValues:=False)
    End Function

    Public Shared Sub ImportInto(target As DataObj,
                                 values(,) As Object,
                                 Optional options As OfficeJsDataImportOptions = Nothing,
                                 Optional CharCols As Integer = -1,
                                 Optional SkipRow As Integer = 0)
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        Dim table As CoreDataTable = Import(values, options)
        target.LoadCoreDataTable(table, CharCols:=CharCols, SkipRow:=SkipRow, cloneTable:=False)
    End Sub

    Public Shared Sub ImportInto(target As DataObj,
                                 rows As System.Collections.Generic.IEnumerable(Of System.Collections.Generic.IEnumerable(Of Object)),
                                 Optional options As OfficeJsDataImportOptions = Nothing,
                                 Optional CharCols As Integer = -1,
                                 Optional SkipRow As Integer = 0)
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        Dim table As CoreDataTable = Import(rows, options)
        target.LoadCoreDataTable(table, CharCols:=CharCols, SkipRow:=SkipRow, cloneTable:=False)
    End Sub

    Private Shared Function RowsToRectangularMatrix(rows As System.Collections.Generic.IEnumerable(Of System.Collections.Generic.IEnumerable(Of Object))) As Object(,)
        If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))

        Dim materializedRows As New System.Collections.Generic.List(Of Object())()
        Dim expectedColumnCount As Integer = -1
        Dim rowIndex As Integer = 0

        For Each rowValues As System.Collections.Generic.IEnumerable(Of Object) In rows
            If rowValues Is Nothing Then Throw New ArgumentException($"Office.js row payload contains a null row at index {rowIndex}.")
            Dim currentRow As Object() = RowToArray(rowValues)
            If expectedColumnCount < 0 Then
                expectedColumnCount = currentRow.Length
                If expectedColumnCount < 1 Then Throw New ArgumentException("Office.js row payload must contain at least one column.")
            ElseIf currentRow.Length <> expectedColumnCount Then
                Throw New ArgumentException($"Office.js row payload must be rectangular. Row {rowIndex} has {currentRow.Length} columns; expected {expectedColumnCount}.")
            End If

            materializedRows.Add(currentRow)
            rowIndex += 1
        Next

        If materializedRows.Count < 1 Then Throw New ArgumentException("Office.js row payload must contain at least one row.")

        Dim values(materializedRows.Count - 1, expectedColumnCount - 1) As Object
        For i As Integer = 0 To materializedRows.Count - 1
            Dim currentRow As Object() = materializedRows(i)
            For j As Integer = 0 To expectedColumnCount - 1
                values(i, j) = currentRow(j)
            Next
        Next
        Return values
    End Function

    Private Shared Function RowToArray(rowValues As System.Collections.Generic.IEnumerable(Of Object)) As Object()
        Dim cells As New System.Collections.Generic.List(Of Object)()
        For Each cell As Object In rowValues
            cells.Add(cell)
        Next
        Return cells.ToArray()
    End Function
End Class