Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.WorksheetFunctions
Imports ExcelDna.Integration

''' <summary>
''' Worksheet-spill output helpers shared by Excel-DNA UDF modules.
''' </summary>
Friend Module UdfOutputTables

    Friend Function SafeCiLabel(ci As ConfidenceIntervalResult) As String
        If ci Is Nothing Then Return "Confidence interval"
        If String.IsNullOrWhiteSpace(ci.CIlabel) Then Return "Confidence interval"
        Return ci.CIlabel
    End Function

    Friend Function SafeCiText(ci As ConfidenceIntervalResult) As String
        If ci Is Nothing Then Return ""
        Return ci.strConfidenceInterval(CIformat.LL_to_UL)
    End Function

    Friend Function BuildResultTable(title As String, body As Object(,)) As Object(,)
        Dim t As New ResultTable
        t.SetBody(body)
        t.AddHeaderTopRow({title, ""})
        Return PrepareResultTableForUdf(t.returnSelf())
    End Function

    ''' <summary>
    ''' Builds a spilled worksheet table from a numeric matrix whose rows and columns have display labels.
    ''' </summary>
    ''' <param name="idHeader">
    ''' Header placed in the top-left cell when <paramref name="includeHeader"/> is <c>True</c>.
    ''' This is typically a label such as <c>Variable</c>, <c>Factor</c>, or <c>Dimension</c>.
    ''' </param>
    ''' <param name="rowNames">Labels aligned with the rows of <paramref name="mat"/>.</param>
    ''' <param name="colNames">Labels aligned with the columns of <paramref name="mat"/>.</param>
    ''' <param name="mat">Numeric matrix to convert into a worksheet spill range.</param>
    ''' <param name="includeHeader">
    ''' When <c>True</c>, returns a header row containing <paramref name="idHeader"/> and the supplied column names.
    ''' When <c>False</c>, only the row labels and numeric values are returned.
    ''' </param>
    ''' <returns>
    ''' A worksheet-ready object matrix produced by <c>PrepareResultTableForUdf</c>, or <c>#N/A</c> when
    ''' <paramref name="mat"/> is <c>Nothing</c>.
    ''' </returns>
    Friend Function BuildNamedMatrixOutput(idHeader As String, rowNames() As String, colNames() As String,
                                           mat(,) As Double, includeHeader As Boolean) As Object
        If mat Is Nothing Then Return ExcelError.ExcelErrorNA

        Dim n As Integer = mat.GetLength(0)
        Dim p As Integer = mat.GetLength(1)
        Dim out(n - 1 + If(includeHeader, 1, 0), p) As Object
        Dim r0 As Integer = 0

        If includeHeader Then
            out(0, 0) = idHeader
            For j As Integer = 0 To p - 1
                out(0, j + 1) = If(colNames IsNot Nothing AndAlso j < colNames.Length, colNames(j), (j + 1).ToString(CultureInfo.InvariantCulture))
            Next
            r0 = 1
        End If

        For i As Integer = 0 To n - 1
            out(r0 + i, 0) = If(rowNames IsNot Nothing AndAlso i < rowNames.Length, rowNames(i), (i + 1).ToString(CultureInfo.InvariantCulture))
            For j As Integer = 0 To p - 1
                out(r0 + i, j + 1) = mat(i, j)
            Next
        Next

        Return PrepareResultTableForUdf(out)
    End Function

    ''' <summary>
    ''' Builds a spilled worksheet table from a numeric matrix whose rows are identified by case or observation IDs.
    ''' </summary>
    ''' <param name="idHeader">
    ''' Header placed in the top-left cell when <paramref name="includeHeader"/> is <c>True</c>.
    ''' This is typically a label such as <c>Row</c> or <c>Observation</c>.
    ''' </param>
    ''' <param name="rowIds">Case identifiers aligned with the rows of <paramref name="mat"/>.</param>
    ''' <param name="colNames">Labels aligned with the columns of <paramref name="mat"/>.</param>
    ''' <param name="mat">Numeric matrix to convert into a worksheet spill range.</param>
    ''' <param name="includeHeader">
    ''' When <c>True</c>, returns a header row containing <paramref name="idHeader"/> and the supplied column names.
    ''' When <c>False</c>, only the case IDs and numeric values are returned.
    ''' </param>
    ''' <returns>
    ''' A worksheet-ready object matrix produced by <c>PrepareResultTableForUdf</c>, or <c>#N/A</c> when
    ''' <paramref name="mat"/> or <paramref name="rowIds"/> is <c>Nothing</c>.
    ''' </returns>
    Friend Function BuildCaseMatrixOutput(idHeader As String, rowIds() As Integer,
                                          colNames() As String, mat(,) As Double, includeHeader As Boolean) As Object
        If mat Is Nothing OrElse rowIds Is Nothing Then Return ExcelError.ExcelErrorNA

        Dim n As Integer = mat.GetLength(0)
        Dim p As Integer = mat.GetLength(1)
        Dim out(n - 1 + If(includeHeader, 1, 0), p) As Object
        Dim r0 As Integer = 0

        If includeHeader Then
            out(0, 0) = idHeader
            For j As Integer = 0 To p - 1
                out(0, j + 1) = If(colNames IsNot Nothing AndAlso j < colNames.Length, colNames(j), (j + 1).ToString(CultureInfo.InvariantCulture))
            Next
            r0 = 1
        End If

        For i As Integer = 0 To n - 1
            out(r0 + i, 0) = rowIds(i)
            For j As Integer = 0 To p - 1
                out(r0 + i, j + 1) = mat(i, j)
            Next
        Next

        Return PrepareResultTableForUdf(out)
    End Function

    ''' <summary>
    ''' Converts a wrapped result table into a worksheet spill range.
    ''' </summary>
    ''' <param name="tableWithTitle">
    ''' A result table that begins with an optional title row followed by a header row and data rows.
    ''' </param>
    ''' <param name="includeHeader">
    ''' When <c>True</c>, the returned spill keeps the header row and drops only the title row.
    ''' When <c>False</c>, both the title row and the header row are removed.
    ''' </param>
    ''' <returns>
    ''' A worksheet-ready object matrix produced by <c>PrepareResultTableForUdf</c>, or <c>#N/A</c> when
    ''' <paramref name="tableWithTitle"/> is <c>Nothing</c>.
    ''' </returns>
    ''' <remarks>
    ''' This helper is intended for outputs created by multivariate back-end wrappers that prepend a descriptive
    ''' title row above the actual column headers.
    ''' </remarks>
    Friend Function PrepareWrappedResultTableForUdf(tableWithTitle As Object(,), includeHeader As Boolean) As Object
        If tableWithTitle Is Nothing Then Return ExcelError.ExcelErrorNA
        Dim totalRows As Integer = tableWithTitle.GetLength(0)
        Dim totalCols As Integer = tableWithTitle.GetLength(1)
        If totalRows <= 1 Then Return PrepareResultTableForUdf(tableWithTitle)
        Dim startRow As Integer = If(includeHeader, 1, 2)
        If startRow >= totalRows Then startRow = totalRows - 1
        Dim out(totalRows - startRow - 1, totalCols - 1) As Object
        For i As Integer = startRow To totalRows - 1
            For j As Integer = 0 To totalCols - 1
                out(i - startRow, j) = tableWithTitle(i, j)
            Next
        Next
        Return PrepareResultTableForUdf(out)
    End Function

    ''' <summary>
    ''' Converts an existing object table into a worksheet spill range, optionally dropping the header row.
    ''' </summary>
    ''' <param name="table">Object table that already contains its own header row.</param>
    ''' <param name="includeHeader">
    ''' When <c>True</c>, the full table is returned.
    ''' When <c>False</c>, the first row is removed before spilling the result.
    ''' </param>
    ''' <returns>
    ''' A worksheet-ready object matrix produced by <c>PrepareResultTableForUdf</c>, or <c>#N/A</c> when
    ''' <paramref name="table"/> is <c>Nothing</c>.
    ''' </returns>
    Friend Function PrepareExistingObjectTableForUdf(table As Object(,), includeHeader As Boolean) As Object
        If table Is Nothing Then Return ExcelError.ExcelErrorNA
        If includeHeader Then Return PrepareResultTableForUdf(table)

        Dim totalRows As Integer = table.GetLength(0)
        Dim totalCols As Integer = table.GetLength(1)
        If totalRows <= 1 Then Return PrepareResultTableForUdf(table)

        Dim out(totalRows - 2, totalCols - 1) As Object
        For i As Integer = 1 To totalRows - 1
            For j As Integer = 0 To totalCols - 1
                out(i - 1, j) = table(i, j)
            Next
        Next
        Return PrepareResultTableForUdf(out)
    End Function

    ''' <summary>
    ''' Builds a simple two-column note table that can be returned directly from a UDF.
    ''' </summary>
    ''' <param name="label">Label shown in the first column.</param>
    ''' <param name="value">Value or explanatory message shown in the second column.</param>
    ''' <returns>
    ''' A two-row object table whose first row acts as the header and whose second row contains the supplied note.
    ''' </returns>
    Friend Function BuildSimpleNoteTable(label As String, value As String) As Object(,)
        Dim out(1, 1) As Object
        out(0, 0) = label
        out(0, 1) = "Value"
        out(1, 0) = label
        out(1, 1) = value
        Return out
    End Function

    ''' <summary>
    ''' Converts a result table object into a 2D object array suitable for returning
    ''' from an Excel-DNA UDF.
    ''' </summary>
    ''' <param name="table">
    ''' The source table object, expected to be a two-dimensional <see cref="Object"/> array.
    ''' </param>
    ''' <returns>
    ''' A two-dimensional <see cref="Object"/> array with <c>Nothing</c> and
    ''' <see cref="DBNull"/> values converted to empty strings.
    ''' Returns <c>Nothing</c> if <paramref name="table"/> cannot be cast to
    ''' a two-dimensional object array.
    ''' </returns>
    Friend Function PrepareResultTableForUdf(table As Object) As Object(,)
        Dim arr As Object(,) = TryCast(table, Object(,))
        If arr Is Nothing Then Return Nothing

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim out(rows - 1, cols - 1) As Object

        For r As Integer = 0 To rows - 1
            For c As Integer = 0 To cols - 1
                Dim v As Object = arr(r, c)

                If v Is Nothing Then
                    out(r, c) = String.Empty ' ExcelEmpty.Value
                ElseIf TypeOf v Is DBNull Then
                    out(r, c) = String.Empty ' ExcelEmpty.Value
                Else
                    out(r, c) = v
                End If
            Next
        Next

        Return out
    End Function

    ''' <summary>
    ''' Ensures probabilities lie in [0,1] and are finite; otherwise returns #NUM!.
    ''' </summary>
    Friend Function ClampProb(p As Double) As Object
        If Double.IsNaN(p) OrElse Double.IsInfinity(p) Then Return ExcelError.ExcelErrorNum
        If p < 0.0 Then p = 0.0
        If p > 1.0 Then p = 1.0
        Return p
    End Function

    Friend Function StackResultTables(tables As List(Of ResultTable)) As Object(,)
        If tables Is Nothing OrElse tables.Count = 0 Then Return Nothing
        Dim stacked As Object(,) = Nothing
        For Each t In tables
            Dim arr As Object(,) = PrepareResultTableForUdf(t.returnSelf())
            stacked = PrepareResultTableForUdf(ParametricUDFs.StackWithBlankRow(stacked, arr))
        Next
        Return stacked
    End Function
End Module