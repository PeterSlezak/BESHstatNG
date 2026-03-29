Imports System
Imports System.Collections.Generic
Imports ExcelDna.Integration

Module UDFhelpers
    ''' <summary>
    ''' Attempts to convert a value to a finite <see cref="Double"/>.
    ''' </summary>
    ''' <param name="v">The value to inspect.</param>
    ''' <returns>
    ''' A finite numeric value when conversion succeeds; otherwise, <c>Nothing</c>.
    ''' </returns>
    Function TryGetDouble(v As Object) As Double?
        If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then
            Return Nothing
        End If
        If TypeOf v Is ExcelError OrElse TypeOf v Is Boolean OrElse TypeOf v Is String Then
            Return Nothing
        End If
        Try
            Dim d As Double = Convert.ToDouble(v)
            If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return Nothing
            Return d
        Catch
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Converts a worksheet argument into trimmed text.
    ''' </summary>
    ''' <param name="v">The value to convert.</param>
    ''' <returns>
    ''' The trimmed text representation of the value, or <c>Nothing</c> when the input is missing.
    ''' </returns>
    Public Function AsString(v As Object) As String
        If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return Nothing
        Return Convert.ToString(v).Trim()
    End Function

    ''' <summary>
    ''' Parses an optional Boolean worksheet argument using common textual aliases.
    ''' </summary>
    ''' <param name="v">The value to parse.</param>
    ''' <param name="defaultValue">The value returned when the input is blank or unrecognized.</param>
    ''' <returns>The parsed Boolean value or <paramref name="defaultValue"/>.</returns>
    Public Function GetOptionalBool(v As Object, defaultValue As Boolean) As Boolean
        If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return defaultValue
        If TypeOf v Is Boolean Then Return CBool(v)
        Dim s As String = Convert.ToString(v).Trim()
        If String.IsNullOrEmpty(s) Then Return defaultValue
        Select Case s.ToLowerInvariant()
            Case "true", "t", "yes", "y", "1"
                Return True
            Case "false", "f", "no", "n", "0"
                Return False
            Case Else
                Return defaultValue
        End Select
    End Function

    ''' <summary>
    ''' Parses an optional integer worksheet argument.
    ''' </summary>
    ''' <param name="v">The value to parse.</param>
    ''' <param name="defaultValue">The value returned when the input is blank or unrecognized.</param>
    ''' <returns>The parsed integer value or <paramref name="defaultValue"/>.</returns>
    Public Function GetOptionalInt(v As Object, defaultValue As Integer) As Integer
        If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return defaultValue
        Dim d As Double?
        d = TryGetDouble(v)
        If d.HasValue Then
            Return CInt(Math.Truncate(d.Value))
        End If
        Return defaultValue
    End Function

    ''' <summary>
    ''' Parses an optional floating-point worksheet argument.
    ''' </summary>
    ''' <param name="v">The value to parse.</param>
    ''' <param name="defaultValue">The value returned when the input is blank or unrecognized.</param>
    ''' <returns>The parsed numeric value or <paramref name="defaultValue"/>.</returns>
    Public Function GetOptionalDouble(v As Object, defaultValue As Double) As Double
        If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return defaultValue
        Dim d As Double?
        d = TryGetDouble(v)
        If d.HasValue Then Return d.Value
        Return defaultValue
    End Function

    ''' <summary>
    ''' Returns <c>exp(exponent)</c> formatted for Excel output, with overflow rendered as "Inf"
    ''' and severe underflow rendered as 0.
    ''' </summary>
    Public Function ExpForDisplay(exponent As Double) As Object
        If Double.IsNaN(exponent) Then Return ExcelError.ExcelErrorNum
        If Double.IsPositiveInfinity(exponent) Then Return "Inf"
        If Double.IsNegativeInfinity(exponent) Then Return 0.0R

        Dim maxLog As Double = Math.Log(Double.MaxValue)
        Dim minLog As Double = Math.Log(Double.Epsilon)

        If exponent > maxLog Then Return "Inf"
        If exponent < minLog Then Return 0.0R

        Dim value As Double = Math.Exp(exponent)
        If Double.IsPositiveInfinity(value) Then Return "Inf"
        If Double.IsNaN(value) Then Return ExcelError.ExcelErrorNum
        Return value
    End Function

    ''' <summary>
    ''' Parses an optional Cox ties-method argument.
    ''' </summary>
    ''' <param name="v">The value to parse.</param>
    ''' <param name="defaultValue">The method returned when the input is blank or unrecognized.</param>
    ''' <returns>The parsed <see cref="TieMethod"/> value or <paramref name="defaultValue"/>.</returns>
    Public Function ParseTieMethod(v As Object, defaultValue As TieMethod) As TieMethod
        Dim s As String = AsString(v)
        If String.IsNullOrWhiteSpace(s) Then Return defaultValue
        Select Case s.ToLowerInvariant()
            Case "breslow"
                Return TieMethod.Breslow
            Case "efron"
                Return TieMethod.Efron
            Case "exact"
                Return TieMethod.Exact
            Case Else
                Return defaultValue
        End Select
    End Function

    ''' <summary>
    ''' Resolves predictor names from an optional name list or range.
    ''' </summary>
    ''' <param name="varNames">Either a comma-separated string, a one-row range, a one-column range, or a missing value.</param>
    ''' <param name="p">The expected predictor count.</param>
    ''' <returns>
    ''' A predictor-name array of length <paramref name="p"/>. When names are missing or invalid, fallback names X1, X2, … are returned.
    ''' </returns>
    Public Function GetVarNames(varNames As Object, p As Integer) As String()
        Dim fallback(p - 1) As String
        For i As Integer = 0 To p - 1
            fallback(i) = "X" & (i + 1).ToString()
        Next

        If varNames Is Nothing OrElse TypeOf varNames Is ExcelEmpty OrElse TypeOf varNames Is ExcelMissing Then
            Return fallback
        End If

        Dim s As String = TryCast(varNames, String)
        If s IsNot Nothing Then
            Dim parts = s.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length = p Then
                For i As Integer = 0 To p - 1
                    parts(i) = parts(i).Trim()
                Next
                Return parts
            End If
        End If

        Dim arr As Object(,) = Get2D(varNames)
        If arr Is Nothing Then Return fallback

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim list As New List(Of String)

        If rows = 1 AndAlso cols >= 1 Then
            For j As Integer = 0 To cols - 1
                Dim v = arr(0, j)
                If IsBlankCell(v) Then
                    list.Add("")
                Else
                    list.Add(Convert.ToString(v).Trim())
                End If
            Next
        ElseIf cols = 1 AndAlso rows >= 1 Then
            For i As Integer = 0 To rows - 1
                Dim v = arr(i, 0)
                If IsBlankCell(v) Then
                    list.Add("")
                Else
                    list.Add(Convert.ToString(v).Trim())
                End If
            Next
        End If

        If list.Count = p Then
            For i As Integer = 0 To p - 1
                If String.IsNullOrWhiteSpace(list(i)) Then list(i) = fallback(i)
            Next
            Return list.ToArray()
        End If

        Return fallback
    End Function

    ''' <summary>
    ''' Attempts to coerce an Excel argument into a two-dimensional object array.
    ''' </summary>
    ''' <param name="v">The worksheet argument to coerce.</param>
    ''' <returns>
    ''' A two-dimensional object array when coercion succeeds; otherwise, <c>Nothing</c>.
    ''' </returns>
    Public Function Get2D(v As Object) As Object(,)
        If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return Nothing

        If TypeOf v Is Object(,) Then
            Return CType(v, Object(,))
        End If

        If TypeOf v Is ExcelReference Then
            Try
                Dim coerced As Object = XlCall.Excel(XlCall.xlCoerce, CType(v, ExcelReference))
                If TypeOf coerced Is Object(,) Then
                    Return CType(coerced, Object(,))
                End If

                Dim sngle(0, 0) As Object
                sngle(0, 0) = coerced
                Return sngle
            Catch
                Return Nothing
            End Try
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Reads a one-column numeric range, trimming unused bottom rows and optionally skipping a header row.
    ''' </summary>
    ''' <param name="v">The worksheet argument to read.</param>
    ''' <param name="values">On success, receives one numeric value per retained row. Invalid nonblank cells are returned as <see cref="Double.NaN"/>.</param>
    ''' <returns>True when a one-column input can be read; otherwise, False.</returns>
    Public Function TryReadNumericColumn(v As Object, ByRef values As List(Of Double)) As Boolean
        values = New List(Of Double)
        Dim arr As Object(,) = Get2D(v)
        If arr Is Nothing Then Return False
        If arr.GetLength(1) <> 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim startRow As Integer = If(HasNumericColumnHeader(arr, lastRow), 1, 0)
        If startRow > lastRow Then Return False

        For i As Integer = startRow To lastRow
            Dim d As Double? = TryGetDouble(arr(i, 0))
            If d.HasValue Then
                values.Add(d.Value)
            Else
                values.Add(Double.NaN)
            End If
        Next

        Return values.Count > 0
    End Function

    ''' <summary>
    ''' Reads a one-column binary range, trimming unused bottom rows and optionally skipping a header row.
    ''' </summary>
    ''' <param name="v">The worksheet argument to read.</param>
    ''' <param name="values">On success, receives one integer value per retained row. Invalid nonblank cells are returned as -1.</param>
    ''' <returns>True when a one-column input can be read; otherwise, False.</returns>
    Public Function TryReadBinary01Column(v As Object, ByRef values As List(Of Integer)) As Boolean
        values = New List(Of Integer)
        Dim arr As Object(,) = Get2D(v)
        If arr Is Nothing Then Return False
        If arr.GetLength(1) <> 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim startRow As Integer = If(HasBinaryColumnHeader(arr, lastRow), 1, 0)
        If startRow > lastRow Then Return False

        For i As Integer = startRow To lastRow
            Dim iv As Integer
            If TryGetBinary01(arr(i, 0), iv) Then
                values.Add(iv)
            Else
                values.Add(-1)
            End If
        Next

        Return values.Count > 0
    End Function

    ''' <summary>
    ''' Reads a one-column text range, trimming unused bottom rows and optionally skipping a header row for full-column references.
    ''' </summary>
    ''' <param name="v">The worksheet argument to read.</param>
    ''' <param name="values">On success, receives one text value per retained row.</param>
    ''' <param name="skipHeaderForWholeColumn">
    ''' When True, a nonblank first row is skipped if the input appears to be a full worksheet-column reference.
    ''' </param>
    ''' <returns>True when a one-column input can be read; otherwise, False.</returns>
    Public Function TryReadTextColumn(v As Object,
                                      ByRef values As List(Of String),
                                      Optional skipHeaderForWholeColumn As Boolean = False) As Boolean
        values = New List(Of String)
        Dim arr As Object(,) = Get2D(v)
        If arr Is Nothing Then Return False
        If arr.GetLength(1) <> 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim startRow As Integer = 0
        If skipHeaderForWholeColumn AndAlso HasTextColumnHeaderForWholeColumnReference(v, arr, lastRow) Then
            startRow = 1
        End If

        If startRow > lastRow Then Return False

        For i As Integer = startRow To lastRow
            Dim cell = arr(i, 0)
            If IsBlankCell(cell) Then
                values.Add("")
            Else
                values.Add(Convert.ToString(cell).Trim())
            End If
        Next

        Return values.Count > 0
    End Function

    ''' <summary>
    ''' Reads a numeric matrix, trimming unused bottom rows and optionally skipping a single header row.
    ''' </summary>
    ''' <param name="v">The worksheet argument to read.</param>
    ''' <param name="mat">On success, receives the numeric matrix. Invalid nonblank cells are returned as <see cref="Double.NaN"/>.</param>
    ''' <param name="rows">On success, receives the retained row count after trimming and optional header removal.</param>
    ''' <param name="cols">On success, receives the column count.</param>
    ''' <returns>True when a numeric matrix can be read; otherwise, False.</returns>
    Public Function TryReadNumericMatrix(v As Object, ByRef mat As Double(,), ByRef rows As Integer, ByRef cols As Integer) As Boolean
        mat = Nothing : rows = 0 : cols = 0
        Dim arr As Object(,) = Get2D(v)
        If arr Is Nothing Then Return False

        cols = arr.GetLength(1)
        If cols < 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim usedRows As Integer = lastRow + 1
        Dim startRow As Integer = If(HasNumericMatrixHeader(arr, lastRow), 1, 0)
        rows = usedRows - startRow
        If rows < 1 Then Return False

        mat = New Double(rows - 1, cols - 1) {}
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                Dim d As Double? = TryGetDouble(arr(startRow + i, j))
                If d.HasValue Then
                    mat(i, j) = d.Value
                Else
                    mat(i, j) = Double.NaN
                End If
            Next
        Next

        Return True
    End Function

    ''' <summary>
    ''' Inverts a square matrix using the shared regression-model inversion routine.
    ''' </summary>
    ''' <param name="a">The square matrix to invert.</param>
    ''' <param name="inv">On success, receives the inverse matrix.</param>
    ''' <returns>True when inversion succeeds; otherwise, False.</returns>
    Public Function TryInvertMatrix(a As Double(,), ByRef inv As Double(,)) As Boolean
        inv = Nothing
        If a Is Nothing Then Return False
        If a.Rank <> 2 Then Return False

        Dim nRows As Integer = a.GetLength(0)
        Dim nCols As Integer = a.GetLength(1)
        If nRows <> nCols OrElse nRows = 0 Then Return False

        Try
            Dim iErr As Integer = 0
            inv = Global.BESHStatNG.Matrix.Matrix.MatInv(a, "LU", iErr, False)

            If iErr <> 0 OrElse inv Is Nothing Then
                inv = Nothing
                Return False
            End If

            If inv.GetLength(0) <> nRows OrElse inv.GetLength(1) <> nCols Then
                inv = Nothing
                Return False
            End If

            Return True
        Catch
            inv = Nothing
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Parses a formula-addressing mode option into a canonical value.
    ''' </summary>
    ''' <param name="v">The user-supplied addressing-mode option.</param>
    ''' <param name="defaultValue">The canonical mode to use when the input is blank or unrecognized.</param>
    ''' <returns>One of: <c>relative</c>, <c>absolute</c>, or <c>names</c>.</returns>
    Public Function ParseFormulaAddressingMode(v As Object, defaultValue As String) As String
        Dim s As String = AsString(v)
        If String.IsNullOrWhiteSpace(s) Then Return defaultValue

        Select Case s.Trim().ToLowerInvariant()
            Case "relative", "rel", "x"
                Return "relative"
            Case "absolute", "abs", "worksheet"
                Return "absolute"
            Case "names", "name", "quoted", "variables", "varnames"
                Return "names"
            Case Else
                Return defaultValue
        End Select
    End Function

    ''' <summary>
    ''' Attempts to derive absolute worksheet column letters from an Excel reference.
    ''' </summary>
    ''' <param name="referenceArg">The original worksheet argument supplied by the user.</param>
    ''' <param name="expectedCount">The expected number of columns in the reference.</param>
    ''' <param name="absoluteColumnLetters">On success, receives one worksheet column letter per predictor column.</param>
    ''' <returns>True when the input is a direct Excel reference and the column count matches; otherwise, False.</returns>
    Public Function TryGetAbsoluteColumnLettersFromRange(referenceArg As Object,
                                                         expectedCount As Integer,
                                                         ByRef absoluteColumnLetters As String()) As Boolean
        absoluteColumnLetters = Nothing

        If expectedCount < 1 Then Return False
        If referenceArg Is Nothing Then Return False
        If Not TypeOf referenceArg Is ExcelReference Then Return False

        Try
            Dim xref As ExcelReference = CType(referenceArg, ExcelReference)
            Dim firstCol As Integer = xref.ColumnFirst
            Dim lastCol As Integer = xref.ColumnLast
            Dim width As Integer = lastCol - firstCol + 1

            If width <> expectedCount Then
                Return False
            End If

            ReDim absoluteColumnLetters(width - 1)
            For j As Integer = 0 To width - 1
                absoluteColumnLetters(j) = RegressionVariableCatalog.NumberToLetters(firstCol + j + 1)
            Next

            Return True
        Catch
            absoluteColumnLetters = Nothing
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Determines whether a worksheet cell should be treated as blank for range-trimming purposes.
    ''' </summary>
    ''' <param name="cell">The worksheet cell to inspect.</param>
    ''' <returns>True when the cell is missing or contains only whitespace text; otherwise, False.</returns>
    Private Function IsBlankCell(cell As Object) As Boolean
        If cell Is Nothing OrElse TypeOf cell Is ExcelEmpty OrElse TypeOf cell Is ExcelMissing Then Return True
        If TypeOf cell Is String Then Return String.IsNullOrWhiteSpace(CStr(cell))
        Return False
    End Function

    ''' <summary>
    ''' Finds the last row in a two-dimensional array that contains any nonblank cell.
    ''' </summary>
    ''' <param name="arr">The two-dimensional worksheet array to inspect.</param>
    ''' <returns>The zero-based last nonblank row index, or -1 when all rows are blank.</returns>
    Private Function FindLastNonBlankRow(arr As Object(,)) As Integer
        If arr Is Nothing Then Return -1

        For i As Integer = arr.GetLength(0) - 1 To 0 Step -1
            For j As Integer = 0 To arr.GetLength(1) - 1
                If Not IsBlankCell(arr(i, j)) Then
                    Return i
                End If
            Next
        Next

        Return -1
    End Function

    ''' <summary>
    ''' Determines whether a numeric column has a header row that should be skipped.
    ''' </summary>
    ''' <param name="arr">The one-column worksheet array to inspect.</param>
    ''' <param name="lastRow">The last retained nonblank row index.</param>
    ''' <returns>True when the first row appears to be a header; otherwise, False.</returns>
    Private Function HasNumericColumnHeader(arr As Object(,), lastRow As Integer) As Boolean
        If arr Is Nothing OrElse arr.GetLength(1) <> 1 Then Return False
        If lastRow < 1 Then Return False

        Dim firstIsNumeric As Boolean = TryGetDouble(arr(0, 0)).HasValue
        Dim secondIsNumeric As Boolean = TryGetDouble(arr(1, 0)).HasValue
        Return (Not firstIsNumeric) AndAlso secondIsNumeric
    End Function

    ''' <summary>
    ''' Determines whether a binary column has a header row that should be skipped.
    ''' </summary>
    ''' <param name="arr">The one-column worksheet array to inspect.</param>
    ''' <param name="lastRow">The last retained nonblank row index.</param>
    ''' <returns>True when the first row appears to be a header; otherwise, False.</returns>
    Private Function HasBinaryColumnHeader(arr As Object(,), lastRow As Integer) As Boolean
        If arr Is Nothing OrElse arr.GetLength(1) <> 1 Then Return False
        If lastRow < 1 Then Return False

        Dim dummy As Integer
        Dim firstIsBinary As Boolean = TryGetBinary01(arr(0, 0), dummy)
        Dim secondIsBinary As Boolean = TryGetBinary01(arr(1, 0), dummy)
        Return (Not firstIsBinary) AndAlso secondIsBinary
    End Function

    ''' <summary>
    ''' Determines whether a text column should skip a header row when the original input was a full worksheet-column reference.
    ''' </summary>
    ''' <param name="originalArg">The original worksheet argument before coercion.</param>
    ''' <param name="arr">The one-column worksheet array to inspect.</param>
    ''' <param name="lastRow">The last retained nonblank row index.</param>
    ''' <returns>True when the first row should be treated as a header; otherwise, False.</returns>
    Private Function HasTextColumnHeaderForWholeColumnReference(originalArg As Object,
                                                                arr As Object(,),
                                                                lastRow As Integer) As Boolean
        If arr Is Nothing OrElse arr.GetLength(1) <> 1 Then Return False
        If lastRow < 1 Then Return False
        If originalArg Is Nothing OrElse Not TypeOf originalArg Is ExcelReference Then Return False

        Try
            Dim xr As ExcelReference = CType(originalArg, ExcelReference)
            Dim selectedRows As Integer = xr.RowLast - xr.RowFirst + 1

            ' Only auto-skip for likely full-column selections such as A:A.
            If xr.RowFirst <> 0 Then Return False
            If selectedRows < 1048576 AndAlso xr.RowLast < 1048575 Then Return False

            If IsBlankCell(arr(0, 0)) Then Return False
            If IsBlankCell(arr(1, 0)) Then Return False

            Return True

        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Determines whether a numeric matrix has a single header row that should be skipped.
    ''' </summary>
    ''' <param name="arr">The worksheet array to inspect.</param>
    ''' <param name="lastRow">The last retained nonblank row index.</param>
    ''' <returns>True when the first row appears to be a header; otherwise, False.</returns>
    Private Function HasNumericMatrixHeader(arr As Object(,), lastRow As Integer) As Boolean
        If arr Is Nothing Then Return False
        If lastRow < 1 Then Return False

        Dim cols As Integer = arr.GetLength(1)
        Dim anyNonNumericFirstRow As Boolean = False
        For j As Integer = 0 To cols - 1
            If Not TryGetDouble(arr(0, j)).HasValue Then
                anyNonNumericFirstRow = True
                Exit For
            End If
        Next

        If Not anyNonNumericFirstRow Then Return False

        For j As Integer = 0 To cols - 1
            If Not TryGetDouble(arr(1, j)).HasValue Then
                Return False
            End If
        Next

        Return True
    End Function

    ''' <summary>
    ''' Attempts to interpret a worksheet cell as a binary 0/1 value.
    ''' </summary>
    ''' <param name="cell">The cell value to inspect.</param>
    ''' <param name="value">On success, receives 0 or 1.</param>
    ''' <returns>True when the cell represents a binary value; otherwise, False.</returns>
    Private Function TryGetBinary01(cell As Object, ByRef value As Integer) As Boolean
        value = -1

        If cell Is Nothing OrElse TypeOf cell Is ExcelEmpty OrElse TypeOf cell Is ExcelMissing Then
            Return False
        End If

        Dim d As Double? = TryGetDouble(cell)
        If d.HasValue Then
            Dim iv As Integer = CInt(Math.Truncate(d.Value))
            If iv = 0 OrElse iv = 1 Then
                value = iv
                Return True
            End If
            Return False
        End If

        If TypeOf cell Is Boolean Then
            value = If(CBool(cell), 1, 0)
            Return True
        End If

        Return False
    End Function
End Module
