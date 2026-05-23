Option Explicit On
Option Strict Off
Option Infer On

Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Excel-DNA/Excel Interop adapter that imports worksheet ranges into the portable <see cref="CoreDataTable"/> model.
''' Keep Excel object-model code here rather than in <see cref="DataObj"/>.
''' </summary>
Public Class ExcelDnaDataImporter
    Public Shared Function Import(ref As String, Optional bStartRow As Boolean = False) As CoreDataTable
        If String.IsNullOrWhiteSpace(ref) Then CoreServices.Errors.LogAndThrow(New ArgumentException("Excel range reference cannot be empty.", NameOf(ref)))

        Dim ws As Worksheet = WorksheetFromRefAdress(ref)
        If ws Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentException("Unable to resolve worksheet from reference: " & ref))

        Dim fullString() As String = ColumListFromRefAdress(ref)
        Dim nCols As Integer = 0
        Dim nRows As Integer = 0
        Dim startRow As Integer = 1
        Dim testVal As Range = Nothing
        Dim maxRows As Integer

        With ws
            For i As Integer = 0 To UBound(fullString, 1)
                nCols += .Range(fullString(i)).Columns.Count
            Next
            If nCols <= 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("Reference does not contain any columns: " & ref))

            testVal = .Range(fullString(0))
            maxRows = MaxRowsInSheet(ws)

            If testVal.Rows.Count = .Cells.Rows.Count Then
                nRows = testVal(maxRows, 1).End(XlDirection.xlUp).Row
            Else
                nRows = testVal.Rows.Count
            End If
            If nRows <= 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("Reference does not contain any rows: " & ref))

            If bStartRow Then
                startRow = ResolveStartRow(fullString(0))
            Else
                startRow = 1
            End If

            Dim values(nRows - 1, nCols - 1) As Object
            Dim columnNames(nCols - 1) As String
            Dim currentCol As Integer = 0

            For areaIndex As Integer = 0 To UBound(fullString, 1)
                Dim rangeAddress As String = BuildConcreteRangeAddress(CStr(fullString(areaIndex)), startRow, nRows)
                testVal = .Range(rangeAddress)

                For k As Integer = 1 To testVal.Columns.Count
                    Dim headerValue As Object = NormalizeExcelValue(testVal(1, k).Value)
                    If headerValue Is Nothing Then
                        columnNames(currentCol) = "Var" & ColNumber2Letter(testVal.Column + k - 1)
                    Else
                        columnNames(currentCol) = CStr(headerValue)
                    End If

                    For rowIndex As Integer = 1 To nRows
                        values(rowIndex - 1, currentCol) = NormalizeExcelValue(testVal(rowIndex, k).Value)
                    Next
                    currentCol += 1
                Next
            Next

            Dim source As New DataSourceInfo With {
                .SourceKind = "WorksheetRange",
                .Address = ref,
                .SheetName = ws.Name,
                .FirstSourceRow = startRow,
                .ColumnNames = columnNames
            }

            Return CoreDataTable.FromObjectMatrix(values, columnNames, firstSourceRow:=startRow, sourceInfo:=source, copyValues:=False)
        End With
    End Function

    Public Shared Sub ImportInto(target As DataObj,
                                 ref As String,
                                 Optional bStartRow As Boolean = False,
                                 Optional CharCols As Integer = -1,
                                 Optional SkipRow As Integer = 0)
        If target Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(target)))
        Dim table As CoreDataTable = Import(ref, bStartRow)
        target.ws = WorksheetFromRefAdress(ref)
        target.LoadCoreDataTable(table, CharCols:=CharCols, SkipRow:=SkipRow, cloneTable:=False)
    End Sub

    Friend Shared Function NormalizeExcelValue(value As Object) As Object
        If value Is Nothing Then Return Nothing
        If TypeOf value Is ExcelEmpty OrElse TypeOf value Is ExcelMissing OrElse TypeOf value Is ExcelError Then Return Nothing
        Return value
    End Function

    Private Shared Function ResolveStartRow(rangeAddress As String) As Integer
        If InStr(rangeAddress, ":") = 0 Then
            Dim digits As String = removeNonDigits(CStr(rangeAddress))
            If digits = String.Empty Then Return 1
            Return CLng(digits)
        End If

        Dim parts As Object = Split(rangeAddress, ":")
        If UBound(parts) >= 1 Then
            Dim digits As String = removeNonDigits(CStr(parts(0)))
            If digits = String.Empty Then Return 1
            Return CLng(digits)
        End If

        Return 1
    End Function

    Private Shared Function BuildConcreteRangeAddress(rangeAddress As String, startRow As Integer, nRows As Integer) As String
        If InStr(rangeAddress, ":") = 0 Then Return rangeAddress

        Dim partString As Object = Split(rangeAddress, ":")
        Dim str1 As Object = Split(partString(0), StrReverse(Val(StrReverse(partString(0)))))
        Dim str2 As Object = Split(partString(1), StrReverse(Val(StrReverse(partString(1)))))
        Return CStr(str1(0)) & CStr(startRow) & ":" & CStr(str2(0)) & CStr(nRows + (startRow - 1))
    End Function
End Class