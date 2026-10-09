Option Explicit On
Option Strict On

Imports System.Collections.Generic

Namespace ExcelCommon

    ''' <summary>
    ''' Portable representation of the small set of primitive spreadsheet values used by
    ''' BESHStatNG analysis-input adapters. Host adapters translate Excel/Office.js/COM cells
    ''' into this type before any analysis-specific validation occurs.
    ''' </summary>
    Public Enum SpreadsheetCellKind
        Blank = 0
        Number = 1
        Text = 2
        Bool = 3
        CellError = 4
    End Enum

    Public Class SpreadsheetCellValue

        Public Sub New()
            Kind = SpreadsheetCellKind.Blank
            TextValue = String.Empty
        End Sub

        Public Property Kind As SpreadsheetCellKind
        Public Property NumberValue As Double
        Public Property TextValue As String
        Public Property BooleanValue As Boolean

        Public Shared Function BlankCell() As SpreadsheetCellValue
            Return New SpreadsheetCellValue()
        End Function

        Public Shared Function NumberCell(value As Double) As SpreadsheetCellValue
            Return New SpreadsheetCellValue With {
                .Kind = SpreadsheetCellKind.Number,
                .NumberValue = value
            }
        End Function

        Public Shared Function TextCell(value As String) As SpreadsheetCellValue
            Return New SpreadsheetCellValue With {
                .Kind = SpreadsheetCellKind.Text,
                .TextValue = If(value, String.Empty)
            }
        End Function

        Public Shared Function BooleanCell(value As Boolean) As SpreadsheetCellValue
            Return New SpreadsheetCellValue With {
                .Kind = SpreadsheetCellKind.Bool,
                .BooleanValue = value
            }
        End Function

        Public Shared Function ErrorCell(value As String) As SpreadsheetCellValue
            Return New SpreadsheetCellValue With {
                .Kind = SpreadsheetCellKind.CellError,
                .TextValue = If(value, String.Empty)
            }
        End Function
    End Class

    ''' <summary>
    ''' Host-neutral rectangular worksheet selection. Rows are zero-based in memory, while
    ''' RowCount/ColumnCount describe the original host range.
    ''' </summary>
    Public Class SpreadsheetRangeData

        Public Sub New()
            WorksheetName = String.Empty
            Address = String.Empty
            Rows = New List(Of List(Of SpreadsheetCellValue))()
        End Sub

        Public Property WorksheetName As String
        Public Property Address As String
        Public Property RowCount As Integer
        Public Property ColumnCount As Integer
        Public Property Rows As List(Of List(Of SpreadsheetCellValue))
    End Class

    ''' <summary>
    ''' Where a host-specific result writer should place an output table.
    ''' NewWorkbook is reserved for a later host capability batch.
    ''' </summary>
    Public Enum SpreadsheetOutputDestination
        CurrentWorksheet = 0
        NewWorksheet = 1
        NewWorkbook = 2
    End Enum

    Public Class SpreadsheetOutputTarget

        Public Sub New()
            Destination = SpreadsheetOutputDestination.NewWorksheet
            WorksheetName = "BESHStat_Result"
            StartRow = 1
            StartColumn = 1
        End Sub

        Public Property Destination As SpreadsheetOutputDestination
        Public Property WorksheetName As String
        Public Property StartRow As Integer
        Public Property StartColumn As Integer

        Public Sub EnsureDefaults()
            If StartRow < 1 Then StartRow = 1
            If StartColumn < 1 Then StartColumn = 1
            If String.IsNullOrWhiteSpace(WorksheetName) Then WorksheetName = "BESHStat_Result"
        End Sub
    End Class

End Namespace
