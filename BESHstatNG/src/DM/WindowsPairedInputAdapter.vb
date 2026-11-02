Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon

''' <summary>Pure adapter after Windows acquisition. No active application, controls or writes.</summary>
Public NotInheritable Class WindowsPairedInputAdapter
    ' Operation-keyed immutable parser evidence. No synthetic IDs or native object ownership.
    Private Shared ReadOnly blockEvidence As New System.Runtime.CompilerServices.ConditionalWeakTable(Of MultiGroupsPairedData, SpreadsheetNumericBlocks)
    Private Shared ReadOnly nestedEvidence As New System.Runtime.CompilerServices.ConditionalWeakTable(Of MultiGroupsPairedDataObj, NestedSelectionEvidence)
    Private NotInheritable Class NestedSelectionEvidence
        Public Sub New(original As SpreadsheetNestedInput, coreInput As SpreadsheetNestedInput)
            Me.Original = original : Me.CoreInput = coreInput
        End Sub
        Public ReadOnly Property Original As SpreadsheetNestedInput
        Public ReadOnly Property CoreInput As SpreadsheetNestedInput
    End Class
    Private Sub New()
    End Sub

    ''' <summary>Preserve legacy importer conversion, including text numeric culture and whole-row dropping.
    ''' Physical columns are unknown: joined acquisition does not supply trustworthy per-column coordinates.</summary>
    Public Shared Function Capture(imported As DataObj, Optional labelColumns As Integer = 0,
                                   Optional workbookId As String = "", Optional worksheetId As String = "") As SpreadsheetRangeData
        If imported Is Nothing OrElse imported.CoreTable Is Nothing Then Return Nothing
        Dim table = imported.CoreTable
        Dim snapshot As New SpreadsheetRangeData With {.RowCount = table.RowCount, .ColumnCount = table.ColumnCount,
            .StartRow = table.FirstSourceRow, .StartColumn = 0, .WorkbookId = workbookId, .WorksheetId = worksheetId,
            .WorksheetName = If(table.SourceInfo Is Nothing, "", table.SourceInfo.SheetName)}
        For r = 0 To table.RowCount - 1
            Dim row As New List(Of SpreadsheetCellValue)
            For c = 0 To table.ColumnCount - 1
                Dim raw = table.ObjectMatrix(r, c)
                If CoreDataTable.IsMissingValue(raw) Then
                    row.Add(SpreadsheetCellValue.BlankCell())
                ElseIf c < labelColumns Then
                    row.Add(SpreadsheetCellValue.TextCell(Convert.ToString(raw, CultureInfo.CurrentCulture)))
                Else
                    Dim value = table.NumericMatrix(r, c)
                    ' NumericMatrix uses NaN for nonnumeric cells. Preserve genuine numeric NaN for diagnostics.
                    Dim parsed As Double
                    Dim numericNaN = Double.IsNaN(value) AndAlso
                        ((TypeOf raw Is Double AndAlso Double.IsNaN(CDbl(raw))) OrElse
                         (TypeOf raw Is Single AndAlso Single.IsNaN(CSng(raw))) OrElse
                         (TypeOf raw Is String AndAlso Double.TryParse(CStr(raw), parsed) AndAlso Double.IsNaN(parsed)))
                    row.Add(If(Not Double.IsNaN(value) OrElse numericNaN, SpreadsheetCellValue.NumberCell(value), SpreadsheetCellValue.BlankCell()))
                End If
            Next
            snapshot.Rows.Add(row)
        Next
        Return WindowsSpreadsheetTransport.Require(SpreadsheetSelectionTransport.Capture(snapshot, table.RowIds)).Data
    End Function

    Public Shared Function SelectPairs(imported As DataObj, ByRef errorText As String) As TwoGroupsPairedData
        Dim result = PairedBlockInputPolicy.ParsePairs(Capture(imported), New SpreadsheetPairedParseOptions With {
            .Missing = SpreadsheetIncompleteObservationPolicy.Skip}, If(imported Is Nothing OrElse imported.CoreTable Is Nothing, Nothing, imported.CoreTable.RowIds))
        If Not Accept(result.Validation, errorText) Then Return Nothing
        Dim values(result.Data.Count - 1, 1) As Double
        For i = 0 To result.Data.Count - 1
            values(i, 0) = result.Data.Pairs(i).First.Value
            values(i, 1) = result.Data.Pairs(i).Second.Value
        Next
        Return New TwoGroupsPairedData With {.X = values, .name1 = imported.varNames(0), .name2 = imported.varNames(1)}
    End Function

    Public Shared Function SelectBlocks(imported As DataObj, ByRef errorText As String) As MultiGroupsPairedData
        Dim result = PairedBlockInputPolicy.ParseBlocks(Capture(imported), New SpreadsheetBlockParseOptions With {
            .Missing = SpreadsheetIncompleteObservationPolicy.Skip, .MinimumTreatments = 2},
            If(imported Is Nothing OrElse imported.CoreTable Is Nothing, Nothing, imported.CoreTable.RowIds),
            If(imported Is Nothing, Nothing, imported.varNames))
        If Not Accept(result.Validation, errorText) Then Return Nothing
        Dim legacy = New MultiGroupsPairedData With {.X = result.Data.Matrix, .varNames = result.Data.Names.ToArray()}
        blockEvidence.Add(legacy, result.Data)
        Return legacy
    End Function

    Public Shared Function SelectNested(imported As DataObj, ByRef errorText As String) As MultiGroupsPairedDataObj
        Dim result = PairedBlockInputPolicy.ParseNested(Capture(imported, 2),
            If(imported Is Nothing OrElse imported.CoreTable Is Nothing, Nothing, imported.CoreTable.RowIds))
        If Not Accept(result.Validation, errorText) Then Return Nothing
        ' Keep the legacy categorical runtime types, extra response columns and Core's existing sorting semantics.
        Dim rowLookup = imported.RowIds.Select(Function(id, index) New With {.Id = id, .Index = index}).ToDictionary(Function(p) p.Id, Function(p) p.Index)
        Dim matrix(result.Data.Responses.Blocks.Count - 1, imported.FinalData.GetLength(1) - 1) As Object
        For r = 0 To result.Data.Responses.Blocks.Count - 1
            Dim index = rowLookup(result.Data.Responses.Blocks(r)(0).WorksheetRow)
            For c = 0 To matrix.GetLength(1) - 1
                matrix(r, c) = imported.FinalData(index, c)
            Next
        Next
        Dim legacy = New MultiGroupsPairedDataObj With {.X = matrix, .varNames = CType(imported.varNames.Clone(), String())}
        'DataObj may stringify a whole response column when any numeric text is
        'present. On .NET Framework this can round Double.ToString to 15 digits.
        'Keep original captured values separately; Core receives matrix verbatim.
        Dim coreResponses As New List(Of IEnumerable(Of SpreadsheetNumericObservation))
        For r = 0 To matrix.GetLength(0) - 1
            Dim cells As New List(Of SpreadsheetNumericObservation)
            For c = 0 To result.Data.Responses.Names.Count - 1
                Dim source = result.Data.Responses.Blocks(r)(c)
                cells.Add(New SpreadsheetNumericObservation(Convert.ToDouble(matrix(r, c + 2), CultureInfo.CurrentCulture),
                    source.SelectedRow, source.SelectedColumn, source.WorksheetRow, source.WorksheetColumn))
            Next
            coreResponses.Add(cells)
        Next
        Dim coreInput As New SpreadsheetNestedInput(result.Data.GroupLabels, result.Data.NestedLabels,
            New SpreadsheetNumericBlocks(result.Data.Responses.Names, coreResponses))
        nestedEvidence.Add(legacy, New NestedSelectionEvidence(result.Data, coreInput))
        Return legacy
    End Function

    Public Shared Function GetBlockEvidence(data As MultiGroupsPairedData) As SpreadsheetNumericBlocks
        Dim result As SpreadsheetNumericBlocks = Nothing
        If data IsNot Nothing Then blockEvidence.TryGetValue(data, result)
        Return result
    End Function

    Public Shared Function GetNestedEvidence(data As MultiGroupsPairedDataObj) As SpreadsheetNestedInput
        Dim result As NestedSelectionEvidence = Nothing
        If data IsNot Nothing Then nestedEvidence.TryGetValue(data, result)
        Return If(result Is Nothing, Nothing, result.Original)
    End Function

    ''' <summary>Actual surviving Core-input numbers with original coordinates.
    ''' Raw captured numbers remain available separately through GetNestedEvidence.</summary>
    Public Shared Function GetNestedCoreInput(data As MultiGroupsPairedDataObj) As SpreadsheetNestedInput
        Dim result As NestedSelectionEvidence = Nothing
        If data IsNot Nothing Then nestedEvidence.TryGetValue(data, result)
        Return If(result Is Nothing, Nothing, result.CoreInput)
    End Function

    Public Shared Function AlignHotelling(first As DataObj, second As DataObj, firstWorkbook As String, firstSheet As String,
                                          secondWorkbook As String, secondSheet As String, ByRef errorText As String) As Integer()
        ' This path deliberately uses the producer's independently complete rows, then joins their real physical IDs.
        Dim a = CaptureClean(first, firstWorkbook, firstSheet), b = CaptureClean(second, secondWorkbook, secondSheet)
        Dim result = PairedBlockInputPolicy.AlignRows(a, If(first Is Nothing, Nothing, first.RowIds), b, If(second Is Nothing, Nothing, second.RowIds))
        If Not Accept(result.Validation, errorText) Then Return Nothing
        Return result.Data.FirstRows.Select(Function(r) first.RowIds(r - 1)).ToArray()
    End Function

    Private Shared Function CaptureClean(imported As DataObj, workbook As String, sheet As String) As SpreadsheetRangeData
        If imported Is Nothing OrElse imported.FinalData Is Nothing Then Return Nothing
        Dim values = imported.DataDbl
        Dim snapshot As New SpreadsheetRangeData With {.RowCount = values.GetLength(0), .ColumnCount = values.GetLength(1), .WorkbookId = workbook, .WorksheetId = sheet}
        For r = 0 To values.GetLength(0) - 1
            Dim row As New List(Of SpreadsheetCellValue)
            For c = 0 To values.GetLength(1) - 1
                row.Add(SpreadsheetCellValue.NumberCell(values(r, c)))
            Next
            snapshot.Rows.Add(row)
        Next
        Return snapshot
    End Function

    Public Shared Function Accept(report As SpreadsheetValidationReport, ByRef errorText As String) As Boolean
        If report.HasErrors Then
            errorText = report.Issues.First(Function(i) i.Severity = SpreadsheetValidationSeverity.Error).Message
            Return False
        End If
        Return True
    End Function
End Class
