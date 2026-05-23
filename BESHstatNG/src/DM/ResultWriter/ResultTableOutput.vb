Option Explicit On

''' <summary>
''' Host-neutral representation of a table that is ready to be written by a target-specific writer.
''' </summary>
''' <remarks>
''' This class deliberately contains no Excel-DNA, Excel Interop, Office.js, or Google Sheets references.
''' It carries the assembled cell values plus the metadata needed by writers to reproduce the
''' existing statistical-table formatting.
''' </remarks>
Public Class ResultTableOutputModel
    Public ReadOnly Property Values As Object(,)
    Public ReadOnly Property HeaderTopRows As Integer
    Public ReadOnly Property HeaderLeftColumns As Integer
    Public ReadOnly Property FooterRows As Integer
    Public ReadOnly Property PvalueColumns As List(Of Integer)
    Public ReadOnly Property TitleRows As Integer
    Public ReadOnly Property IsResultTable As Boolean

    Public Sub New(values As Object(,),
                   Optional headerTopRows As Integer = 0,
                   Optional headerLeftColumns As Integer = 0,
                   Optional footerRows As Integer = 0,
                   Optional pvalueColumns As IEnumerable(Of Integer) = Nothing,
                   Optional titleRows As Integer = 0,
                   Optional isResultTable As Boolean = True)
        Me.Values = values
        Me.HeaderTopRows = headerTopRows
        Me.HeaderLeftColumns = headerLeftColumns
        Me.FooterRows = footerRows
        Me.PvalueColumns = If(pvalueColumns Is Nothing, New List(Of Integer), New List(Of Integer)(pvalueColumns))
        Me.TitleRows = titleRows
        Me.IsResultTable = isResultTable
    End Sub

    Public ReadOnly Property RowCount As Integer
        Get
            If Me.Values Is Nothing Then Return 0
            Return UBound(Me.Values, 1) + 1
        End Get
    End Property

    Public ReadOnly Property ColumnCount As Integer
        Get
            If Me.Values Is Nothing Then Return 0
            Return UBound(Me.Values, 2) + 1
        End Get
    End Property
End Class

''' <summary>
''' A positioned table/matrix output block produced by <see cref="ResultTableWriterBase"/>.
''' </summary>
Public Class ResultTableOutputBlock
    Public ReadOnly Property StartRow As Integer
    Public ReadOnly Property StartColumn As Integer
    Public ReadOnly Property Model As ResultTableOutputModel

    Public Sub New(startRow As Integer, startColumn As Integer, model As ResultTableOutputModel)
        Me.StartRow = startRow
        Me.StartColumn = startColumn
        Me.Model = model
    End Sub

    Public ReadOnly Property EndRow As Integer
        Get
            If Me.Model Is Nothing OrElse Me.Model.RowCount = 0 Then Return Me.StartRow
            Return Me.StartRow + Me.Model.RowCount - 1
        End Get
    End Property

    Public ReadOnly Property EndColumn As Integer
        Get
            If Me.Model Is Nothing OrElse Me.Model.ColumnCount = 0 Then Return Me.StartColumn
            Return Me.StartColumn + Me.Model.ColumnCount - 1
        End Get
    End Property
End Class

''' <summary>
''' Base class for all result writers. It owns pointer movement and conversion of raw arrays,
''' scalars, and <see cref="ResultTable"/> instances into host-neutral output blocks.
''' </summary>
Public MustInherit Class ResultTableWriterBase
    Private lastRowID As Integer
    Private lastColumID As Integer

    Protected Sub New(Optional row As Integer = 1, Optional col As Integer = 1)
        Me.lastRowID = row
        Me.lastColumID = col
    End Sub

    ''' <summary>Returns the current row pointer indicating where the next write will begin.</summary>
    Public ReadOnly Property RowID() As Integer
        Get
            Return lastRowID
        End Get
    End Property

    ''' <summary>Returns the current column pointer indicating where the next write will begin.</summary>
    Public ReadOnly Property ColID() As Integer
        Get
            Return lastColumID
        End Get
    End Property

    ''' <summary>Sets the internal row pointer to a specific one-based row index.</summary>
    Public Sub setRowPointer(Optional r As Integer = 1)
        Me.lastRowID = r
    End Sub

    ''' <summary>Shifts the internal row pointer downward by a specified number of rows.</summary>
    Public Sub shiftRowPointer(Optional by As Integer = 1)
        Me.lastRowID += by
    End Sub

    ''' <summary>Sets the internal column pointer to a specific one-based column index.</summary>
    Public Sub setColumnPointer(Optional c As Integer = 1)
        Me.lastColumID = c
    End Sub

    ''' <summary>Shifts the internal column pointer to the right by a specified number of columns.</summary>
    Public Sub shiftColumnPointer(Optional by As Integer = 1)
        Me.lastColumID += by
    End Sub

    ''' <summary>
    ''' Writes a scalar, array, or <see cref="ResultTable"/> and advances the row pointer.
    ''' </summary>
    Public Overridable Sub write(ds As Object, Optional bTall As Boolean = False)
        Dim block As ResultTableOutputBlock = BuildOutputBlock(ds, bTall)
        WriteOutputBlock(block)

        If block IsNot Nothing AndAlso block.Model IsNot Nothing Then
            Me.lastRowID += block.Model.RowCount
        End If
    End Sub

    ''' <summary>
    ''' Converts the input into a positioned, normalized output block without writing it.
    ''' </summary>
    Protected Function BuildOutputBlock(ds As Object, Optional bTall As Boolean = False) As ResultTableOutputBlock
        Dim model As ResultTableOutputModel

        If TypeOf ds Is ResultTable Then
            model = DirectCast(ds, ResultTable).ToOutputModel()
        Else
            model = New ResultTableOutputModel(ToMatrix(ds, bTall), isResultTable:=False)
        End If

        Dim safeValues As Object(,) = NormalizeMatrixForOutput(model.Values)
        Dim safeModel As New ResultTableOutputModel(
            safeValues,
            model.HeaderTopRows,
            model.HeaderLeftColumns,
            model.FooterRows,
            model.PvalueColumns,
            model.TitleRows,
            model.IsResultTable)

        Return New ResultTableOutputBlock(Me.lastRowID, Me.lastColumID, safeModel)
    End Function

    Protected MustOverride Sub WriteOutputBlock(block As ResultTableOutputBlock)

    Private Shared Function ToMatrix(value As Object, bTall As Boolean) As Object(,)
        If value Is Nothing Then
            Dim empty(0, 0) As Object
            empty(0, 0) = Nothing
            Return empty
        End If

        If Not IsArray(value) Then
            Dim scalar(0, 0) As Object
            scalar(0, 0) = value
            Return scalar
        End If

        Dim arr As Array = DirectCast(value, Array)

        If arr.Rank = 1 Then
            Dim n As Integer = arr.GetUpperBound(0) - arr.GetLowerBound(0) + 1

            If bTall Then
                Dim out(n - 1, 0) As Object
                For i As Integer = 0 To n - 1
                    out(i, 0) = arr.GetValue(arr.GetLowerBound(0) + i)
                Next
                Return out
            Else
                Dim out(0, n - 1) As Object
                For i As Integer = 0 To n - 1
                    out(0, i) = arr.GetValue(arr.GetLowerBound(0) + i)
                Next
                Return out
            End If
        End If

        If arr.Rank = 2 Then
            Dim nRows As Integer = arr.GetUpperBound(0) - arr.GetLowerBound(0) + 1
            Dim nCols As Integer = arr.GetUpperBound(1) - arr.GetLowerBound(1) + 1
            Dim out(nRows - 1, nCols - 1) As Object

            For i As Integer = 0 To nRows - 1
                For j As Integer = 0 To nCols - 1
                    out(i, j) = arr.GetValue(arr.GetLowerBound(0) + i, arr.GetLowerBound(1) + j)
                Next
            Next

            Return out
        End If

        Throw New NotSupportedException("Only scalar values, one-dimensional arrays, two-dimensional arrays, and ResultTable instances can be written.")
    End Function

    Protected Shared Function NormalizeMatrixForOutput(values As Object(,)) As Object(,)
        If values Is Nothing Then Return Nothing

        Dim out(UBound(values, 1), UBound(values, 2)) As Object
        For i As Integer = 0 To UBound(values, 1)
            For j As Integer = 0 To UBound(values, 2)
                out(i, j) = NormalizeScalarForOutput(values(i, j))
            Next
        Next

        Return out
    End Function

    Protected Shared Function NormalizeScalarForOutput(value As Object) As Object
        If value Is Nothing Then Return Nothing

        If TypeOf value Is Double Then
            Dim d As Double = CDbl(value)
            If Double.IsNaN(d) Then Return "#N/A"
            If Double.IsPositiveInfinity(d) Then Return "#Pinf"
            If Double.IsNegativeInfinity(d) Then Return "#Ninf"
            Return d
        End If

        If TypeOf value Is Single Then
            Dim d As Double = CDbl(value)
            If Double.IsNaN(d) Then Return "#N/A"
            If Double.IsPositiveInfinity(d) Then Return "#Pinf"
            If Double.IsNegativeInfinity(d) Then Return "#Ninf"
            Return value
        End If

        Return value
    End Function
End Class