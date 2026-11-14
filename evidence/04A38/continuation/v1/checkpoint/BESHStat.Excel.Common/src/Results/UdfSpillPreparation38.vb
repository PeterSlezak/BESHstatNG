Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    ''' <summary>A zero-based spill-array cell, unrelated to a worksheet address.</summary>
    Public NotInheritable Class UdfSpillCell38
        Public Sub New(rowOrdinal As Integer, columnOrdinal As Integer)
            Me.RowOrdinal = rowOrdinal
            Me.ColumnOrdinal = columnOrdinal
        End Sub
        Public ReadOnly Property RowOrdinal As Integer
        Public ReadOnly Property ColumnOrdinal As Integer
    End Class

    Public NotInheritable Class UdfSpillPreparation38
        Private Sub New()
        End Sub

        Public Shared Function TwoValueTable(title As String, firstLabel As String, secondLabel As String) As UdfReportProjection38
            Dim table As New Global.BESHStatNG.ResultTable()
            table.SetBody(New Object(,) {{firstLabel, New UdfSourceCellReference38(0)}, {secondLabel, New UdfSourceCellReference38(1)}})
            table.AddPvalueCellToFormat(2, 2)
            table.AddHeaderTopRow({title, ""})
            Return New UdfReportProjection38(table.ToOutputModel(), UdfReportPreparation38.NormalizeBlanks)
        End Function

        ''' <summary>Retains column-first then explicit-cell order, including repeated cells.</summary>
        Public Shared Function PvalueCells(model As Global.BESHStatNG.ResultTableOutputModel,
                                          rows As Integer, columns As Integer) As UdfSpillCell38()
            Dim result As New List(Of UdfSpillCell38)()
            Dim firstBodyRow As Integer = model.TitleRows + model.HeaderTopRows
            Dim lastBodyRow As Integer = rows - model.FooterRows - 1
            For Each bodyColumn As Integer In model.PvalueColumns
                Dim columnIndex As Integer = model.HeaderLeftColumns + bodyColumn - 1
                If columnIndex < 0 OrElse columnIndex >= columns Then Continue For
                For rowIndex As Integer = Math.Max(0, firstBodyRow) To Math.Min(lastBodyRow, rows - 1)
                    result.Add(New UdfSpillCell38(rowIndex, columnIndex))
                Next
            Next
            For Each address As Global.BESHStatNG.ResultTableCellAddress In model.PvalueCells
                Dim rowIndex As Integer = firstBodyRow + address.BodyRow - 1
                Dim columnIndex As Integer = model.HeaderLeftColumns + address.BodyColumn - 1
                If rowIndex < firstBodyRow OrElse rowIndex > lastBodyRow Then Continue For
                If columnIndex < 0 OrElse columnIndex >= columns Then Continue For
                result.Add(New UdfSpillCell38(rowIndex, columnIndex))
            Next
            Return result.ToArray()
        End Function

        Public Shared Function IsPValueLabel(label As String) As Boolean
            If String.IsNullOrWhiteSpace(label) Then Return False
            Dim normalized As New System.Text.StringBuilder(label.Length)
            For Each ch As Char In label
                If Char.IsLetterOrDigit(ch) Then normalized.Append(Char.ToLowerInvariant(ch))
            Next
            Return normalized.ToString().Contains("pvalue")
        End Function
    End Class
End Namespace
