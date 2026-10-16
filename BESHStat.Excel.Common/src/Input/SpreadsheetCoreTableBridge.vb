Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon

    ''' <summary>
    ''' Converts a policy-validated raw range snapshot into the existing CoreDataTable.
    ''' This does NOT implement a new general-purpose dataframe or change Core's import rules.
    ''' No header guessing or row dropping happens here; callers supply column names explicitly.
    ''' </summary>
    Public NotInheritable Class SpreadsheetCoreTableBridge
        Private Sub New()
        End Sub

        Public Shared Function ToCoreDataTable(input As SpreadsheetRangeData,
                                               columnNames As String(),
                                               Optional options As SpreadsheetCellPolicyOptions = Nothing) As Global.BESHStatNG.CoreDataTable
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If columnNames Is Nothing Then Throw New ArgumentNullException(NameOf(columnNames))
            If input.RowCount < 1 OrElse input.ColumnCount < 1 OrElse input.Rows Is Nothing OrElse
               input.Rows.Count <> input.RowCount OrElse columnNames.Length <> input.ColumnCount Then
                Throw New ArgumentException("The source range is empty or its dimensions/columns disagree.", NameOf(input))
            End If

            Dim policy As SpreadsheetCellPolicyOptions = If(options, New SpreadsheetCellPolicyOptions())
            Dim values(input.RowCount - 1, input.ColumnCount - 1) As Object
            Dim missing(input.RowCount - 1, input.ColumnCount - 1) As Boolean
            Dim numbers(input.RowCount - 1, input.ColumnCount - 1) As Double

            For r As Integer = 0 To input.RowCount - 1
                Dim row As List(Of SpreadsheetCellValue) = input.Rows(r)
                If row Is Nothing OrElse row.Count <> input.ColumnCount Then
                    Throw New ArgumentException("The source range is not rectangular.", NameOf(input))
                End If
                For c As Integer = 0 To input.ColumnCount - 1
                    Dim cell As SpreadsheetCellValue = row(c)
                    Dim evaluated As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(cell, policy)
                    If evaluated.IsError AndAlso policy.Errors = SpreadsheetErrorPolicy.Reject Then
                        Throw New ArgumentException("Excel error cell at source row " & (If(input.StartRow > 0, input.StartRow, 1) + r).ToString() &
                                                    ", column " & (If(input.StartColumn > 0, input.StartColumn, 1) + c).ToString() & ".", NameOf(input))
                    End If
                    missing(r, c) = evaluated.IsMissing
                    numbers(r, c) = If(evaluated.HasNumericValue AndAlso Not evaluated.IsMissing, evaluated.NumericValue, Double.NaN)
                    If evaluated.IsMissing Then
                        values(r, c) = Nothing
                    ElseIf evaluated.IsError Then
                        values(r, c) = evaluated.Error
                    ElseIf cell.Kind = SpreadsheetCellKind.Number Then
                        values(r, c) = cell.NumberValue
                    ElseIf cell.Kind = SpreadsheetCellKind.Bool Then
                        values(r, c) = cell.BooleanValue
                    ElseIf cell.Kind = SpreadsheetCellKind.Text Then
                        values(r, c) = cell.TextValue
                    End If
                Next
            Next

            Dim source As New Global.BESHStatNG.DataSourceInfo With {
                .SourceKind = "SpreadsheetRange",
                .Address = input.QualifiedAddress,
                .SheetName = input.WorksheetName,
                .FirstSourceRow = If(input.StartRow > 0, input.StartRow, 1),
                .FirstSourceColumn = If(input.StartColumn > 0, input.StartColumn, 1)
            }
            Dim table As Global.BESHStatNG.CoreDataTable =
                Global.BESHStatNG.CoreDataTable.FromObjectMatrix(values, columnNames,
                    source.FirstSourceRow, source, copyValues:=False)
            ' Explicit canonical policy overrides Core's deliberately permissive generic
            ' numeric string conversion, which depends on the current machine culture.
            table.MissingMask = missing
            table.NumericMatrix = numbers
            Return table
        End Function
    End Class
End Namespace
