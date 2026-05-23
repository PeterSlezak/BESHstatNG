Option Explicit On
Option Strict Off
Option Infer On

''' <summary>
''' Describes how an imported data matrix originated. GUI imports normally populate this from a worksheet range;
''' UDF imports can populate it from the supplied Excel argument or raw object matrix.
''' </summary>
Public Class DataSourceInfo
    Public Property SourceKind As String = "Unknown"
    Public Property Address As String = Nothing
    Public Property SheetName As String = Nothing
    Public Property FirstSourceRow As Integer = 1
    Public Property FirstSourceColumn As Integer = 1
    Public Property ColumnNames As String() = Nothing
End Class

''' <summary>
''' Host-neutral representation of tabular analysis data.  The table stores values exactly as a front-end importer
''' provides them, together with source row ids, variable names, and missing-value metadata.  It deliberately has
''' no dependency on Excel-DNA, Excel Interop, WinForms, Office.js, or Google Sheets APIs.
''' </summary>
Public Class CoreDataTable
    Public Property ColumnNames As String()
    Public Property RowIds As Integer()
    Public Property ObjectMatrix As Object(,)
    Public Property NumericMatrix As Double(,)
    Public Property MissingMask As Boolean(,)
    Public Property SourceInfo As DataSourceInfo
    Public Property FirstSourceRow As Integer = 1
    Public Property FirstSourceColumn As Integer = 1

    Public ReadOnly Property RowCount As Integer
        Get
            If ObjectMatrix Is Nothing Then Return 0
            Return ObjectMatrix.GetLength(0)
        End Get
    End Property

    Public ReadOnly Property ColumnCount As Integer
        Get
            If ObjectMatrix Is Nothing Then Return 0
            Return ObjectMatrix.GetLength(1)
        End Get
    End Property

    Public Shared Function FromObjectMatrix(values(,) As Object,
                                            columnNames() As String,
                                            Optional firstSourceRow As Integer = 1,
                                            Optional sourceInfo As DataSourceInfo = Nothing,
                                            Optional copyValues As Boolean = True) As CoreDataTable
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If columnNames Is Nothing Then Throw New ArgumentNullException(NameOf(columnNames))
        Dim rows As Integer = values.GetLength(0)
        Dim cols As Integer = values.GetLength(1)
        If rows < 1 OrElse cols < 1 Then Throw New ArgumentException("values must contain at least one row and one column.")
        If columnNames.Length <> cols Then Throw New ArgumentException($"columnNames length ({columnNames.Length}) must match the number of columns ({cols}).")

        Dim table As New CoreDataTable()
        ' By default CoreDataTable owns a defensive copy of the input matrix.
        ' Importers that created a fresh temporary matrix may pass copyValues:=False
        ' to transfer ownership and avoid one full-size matrix copy for large datasets.
        table.ObjectMatrix = If(copyValues, CopyObjectMatrix(values), values)
        table.ColumnNames = CopyColumnNames(columnNames)
        table.FirstSourceRow = Math.Max(1, firstSourceRow)
        table.RowIds = BuildSequentialRowIds(rows, table.FirstSourceRow)
        table.MissingMask = BuildMissingMask(table.ObjectMatrix)
        table.NumericMatrix = BuildNumericMatrix(table.ObjectMatrix, table.MissingMask)
        table.SourceInfo = If(sourceInfo, New DataSourceInfo())
        table.FirstSourceColumn = Math.Max(1, table.SourceInfo.FirstSourceColumn)
        table.SourceInfo.FirstSourceRow = table.FirstSourceRow
        table.SourceInfo.FirstSourceColumn = table.FirstSourceColumn
        table.SourceInfo.ColumnNames = CopyColumnNames(table.ColumnNames)
        Return table
    End Function

    Public Function Clone() As CoreDataTable
        Dim copy As New CoreDataTable()
        copy.ColumnNames = CopyColumnNames(Me.ColumnNames)
        copy.RowIds = If(Me.RowIds Is Nothing, Nothing, CType(Me.RowIds.Clone(), Integer()))
        copy.ObjectMatrix = CopyObjectMatrix(Me.ObjectMatrix)
        copy.NumericMatrix = If(Me.NumericMatrix Is Nothing, Nothing, DirectCast(Me.NumericMatrix.Clone(), Double(,)))
        copy.MissingMask = If(Me.MissingMask Is Nothing, Nothing, DirectCast(Me.MissingMask.Clone(), Boolean(,)))
        copy.FirstSourceRow = Me.FirstSourceRow
        copy.FirstSourceColumn = Me.FirstSourceColumn
        If Me.SourceInfo IsNot Nothing Then
            copy.SourceInfo = New DataSourceInfo With {
                .SourceKind = Me.SourceInfo.SourceKind,
                .Address = Me.SourceInfo.Address,
                .SheetName = Me.SourceInfo.SheetName,
                .FirstSourceRow = Me.SourceInfo.FirstSourceRow,
                .FirstSourceColumn = Me.SourceInfo.FirstSourceColumn,
                .ColumnNames = CopyColumnNames(Me.SourceInfo.ColumnNames)
            }
        End If
        Return copy
    End Function

    Public Shared Function IsMissingValue(value As Object) As Boolean
        If value Is Nothing Then Return True
        If value Is DBNull.Value Then Return True

        Dim s As String = TryCast(value, String)
        If s IsNot Nothing AndAlso s.Trim() = String.Empty Then Return True

        Return False
    End Function

    Public Shared Function CopyObjectMatrix(values(,) As Object) As Object(,)
        If values Is Nothing Then Return Nothing
        Dim rows As Integer = values.GetLength(0)
        Dim cols As Integer = values.GetLength(1)
        Dim copy(rows - 1, cols - 1) As Object
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                copy(i, j) = values(i, j)
            Next
        Next
        Return copy
    End Function

    Public Shared Function CopyColumnNames(names() As String) As String()
        If names Is Nothing Then Return Nothing
        Dim copy(names.Length - 1) As String
        For i As Integer = 0 To names.Length - 1
            copy(i) = If(names(i), String.Empty)
        Next
        Return copy
    End Function

    Public Shared Function BuildSequentialRowIds(rowCount As Integer, firstSourceRow As Integer) As Integer()
        If rowCount <= 0 Then Return New Integer() {}
        Dim ids(rowCount - 1) As Integer
        Dim startRow As Integer = Math.Max(1, firstSourceRow)
        For i As Integer = 0 To rowCount - 1
            ids(i) = startRow + i
        Next
        Return ids
    End Function

    Public Shared Function BuildMissingMask(values(,) As Object) As Boolean(,)
        If values Is Nothing Then Return Nothing
        Dim rows As Integer = values.GetLength(0)
        Dim cols As Integer = values.GetLength(1)
        Dim mask(rows - 1, cols - 1) As Boolean
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                mask(i, j) = IsMissingValue(values(i, j))
            Next
        Next
        Return mask
    End Function

    Public Shared Function BuildNumericMatrix(values(,) As Object, Optional missingMask(,) As Boolean = Nothing) As Double(,)
        If values Is Nothing Then Return Nothing
        Dim rows As Integer = values.GetLength(0)
        Dim cols As Integer = values.GetLength(1)
        Dim numeric(rows - 1, cols - 1) As Double
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                If (missingMask IsNot Nothing AndAlso missingMask(i, j)) OrElse IsMissingValue(values(i, j)) Then
                    numeric(i, j) = Double.NaN
                ElseIf IsNumeric(values(i, j)) Then
                    numeric(i, j) = CDbl(values(i, j))
                Else
                    numeric(i, j) = Double.NaN
                End If
            Next
        Next
        Return numeric
    End Function
End Class

''' <summary>
''' Minimal host-neutral container for one or more imported tables plus metadata.  This is intentionally small and
''' is meant to grow into the future Core/Data project without changing Excel-facing import code again.
''' </summary>
Public Class AnalysisDataSet
    Public Property Tables As New List(Of CoreDataTable)()
    Public Property Metadata As New Dictionary(Of String, Object)()

    Public Sub Add(table As CoreDataTable)
        If table Is Nothing Then Throw New ArgumentNullException(NameOf(table))
        Tables.Add(table)
    End Sub
End Class
