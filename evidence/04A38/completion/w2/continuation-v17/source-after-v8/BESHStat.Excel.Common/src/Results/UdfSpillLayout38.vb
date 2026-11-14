Option Explicit On
Option Strict On

Imports System

Namespace ExcelCommon
    ''' <summary>Spill-array dimensions and source ordinals; no worksheet coordinates.</summary>
    Public NotInheritable Class UdfSpillSlice38
        Public Sub New(sourceRows As Integer, columns As Integer, startRow As Integer)
            Me.SourceStartRow = startRow
            Me.RowCount = sourceRows - startRow
            Me.ColumnCount = columns
        End Sub
        Public ReadOnly Property SourceStartRow As Integer
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
    End Class

    Public NotInheritable Class UdfSpillStack38
        Public Sub New(firstRows As Integer, firstColumns As Integer, secondRows As Integer, secondColumns As Integer)
            RowCount = firstRows + 1 + secondRows
            ColumnCount = Math.Max(firstColumns, secondColumns)
            SecondStartRow = firstRows + 1
        End Sub
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
        Public ReadOnly Property SecondStartRow As Integer
    End Class

    Public NotInheritable Class UdfSpillLayout38
        Private Sub New()
        End Sub

        Public Shared Function WrappedTable(rows As Integer, columns As Integer, includeHeader As Boolean) As UdfSpillSlice38
            Dim startRow As Integer = 0
            If rows > 1 Then
                startRow = If(includeHeader, 1, 2)
                If startRow >= rows Then startRow = rows - 1
            End If
            Return New UdfSpillSlice38(rows, columns, startRow)
        End Function

        Public Shared Function ExistingTable(rows As Integer, columns As Integer, includeHeader As Boolean) As UdfSpillSlice38
            Return New UdfSpillSlice38(rows, columns, If(includeHeader OrElse rows <= 1, 0, 1))
        End Function
    End Class
End Namespace
