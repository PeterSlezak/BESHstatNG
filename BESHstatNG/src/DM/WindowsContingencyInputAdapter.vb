Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon

''' <summary>Host-free conversion after Windows acquisition; no active Excel state, controls or writes.</summary>
Public NotInheritable Class WindowsContingencyInputAdapter
    Private Sub New()
    End Sub
    ''' <summary>Capture raw producer values, retaining the original raw row IDs.
    ''' Legacy Excel error normalization has already lost native error payloads; Nothing is not claimed as an error capture.</summary>
    Public Shared Function ParseWorksheet(imported As DataObj, options As SpreadsheetContingencyCountParseOptions,
                                          Optional twoByTwo As Boolean = False) As SpreadsheetTypedParseResult(Of ContingencyCountSelection)
        If imported Is Nothing OrElse imported.CoreTable Is Nothing Then Return ContingencyInputPolicy.Parse(Nothing, options, twoByTwo)
        Dim table = imported.CoreTable
        Dim snapshot As New SpreadsheetRangeData With {.RowCount = table.RowCount, .ColumnCount = table.ColumnCount,
            .WorksheetName = If(table.SourceInfo Is Nothing, "", table.SourceInfo.SheetName),
            .Address = If(table.SourceInfo Is Nothing, "", table.SourceInfo.Address), .StartRow = table.FirstSourceRow, .StartColumn = 0}
        For r = 0 To table.RowCount - 1
            Dim row As New List(Of SpreadsheetCellValue)
            For c = 0 To table.ColumnCount - 1
                Dim raw = table.ObjectMatrix(r, c)
                Dim label = options IsNot Nothing AndAlso ((options.HasColumnHeaders AndAlso r = 0) OrElse (options.HasRowLabels AndAlso c = 0))
                If CoreDataTable.IsMissingValue(raw) Then
                    row.Add(SpreadsheetCellValue.BlankCell())
                ElseIf TypeOf raw Is String AndAlso label Then
                    row.Add(SpreadsheetCellValue.TextCell(CStr(raw)))
                ElseIf TypeOf raw Is Boolean Then
                    row.Add(SpreadsheetCellValue.BooleanCell(CBool(raw)))
                ElseIf TypeOf raw Is DateTime Then
                    row.Add(SpreadsheetCellValue.TextCell(DirectCast(raw, DateTime).ToString("O", CultureInfo.InvariantCulture)))
                Else
                    Dim value = table.NumericMatrix(r, c)
                    If Double.IsNaN(value) AndAlso TypeOf raw Is String AndAlso Not Double.TryParse(CStr(raw), value) Then
                        row.Add(SpreadsheetCellValue.TextCell(CStr(raw)))
                    Else
                        row.Add(SpreadsheetCellValue.NumberCell(value))
                    End If
                End If
            Next
            snapshot.Rows.Add(row)
        Next
        Return ContingencyInputPolicy.Parse(snapshot, options, twoByTwo, table.RowIds)
    End Function

    Public Shared Function AnalyzeRxC(imported As DataObj, options As RxCReportOptions) As ContingencyReportResult
        Dim input = ParseWorksheet(imported, New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders = False, .HasRowLabels = False})
        If Not input.Success Then Return New ContingencyReportResult(Array.Empty(Of ContingencySemanticTable)(), input.Validation.Issues)
        Return ContingencyReportPolicy.RxC(input.Data, options)
    End Function

    Public Shared Function AnalyzeDialog(a As Double, b As Double, c As Double, d As Double, options As TwoByTwoReportOptions) As ContingencyReportResult
        Dim input = ContingencyInputPolicy.FromDialog(a, b, c, d)
        If Not input.Success Then Return New ContingencyReportResult(Array.Empty(Of ContingencySemanticTable)(), input.Validation.Issues)
        Return ContingencyReportPolicy.TwoByTwo(input.Data, options)
    End Function

    Public Shared Function AnalyzeProportions(mode As ProportionsMode, hypothesis As IndependentProportionsHypothesis,
                                             n1 As Double, r1 As Double, n2 As Double, r2 As Double, both As Double,
                                             alpha As Double, margin As Double) As ContingencyReportResult
        Dim input = ProportionsInputPolicy.Validate(mode, hypothesis, n1, r1, n2, r2, both, alpha, margin)
        If Not input.Success Then Return New ContingencyReportResult(Array.Empty(Of ContingencySemanticTable)(), input.Validation.Issues)
        Return ProportionsReportPolicy.Analyze(input.Data)
    End Function

End Class
