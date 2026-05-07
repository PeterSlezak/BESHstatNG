Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.WorksheetFunctions
Imports ExcelDna.Integration

Module UDFhelpers

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

    Friend Function TryGetEquivalenceMargins(lowerArg As Object, upperArg As Object, ByRef lowerValue As Double, ByRef upperValue As Double) As Boolean
        lowerValue = Double.NaN
        upperValue = Double.NaN

        If Not TryGetFiniteDouble(lowerArg, lowerValue) Then Return False

        If IsMissingArg(upperArg) Then
            Dim m As Double = Math.Abs(lowerValue)
            If m <= 0.0 Then Return False
            lowerValue = -m
            upperValue = m
            Return True
        End If

        If Not TryGetFiniteDouble(upperArg, upperValue) Then Return False
        Return True
    End Function

    Friend Function LoggedUdfError(functionName As String,
                                   ex As Exception,
                                   fallback As Object,
                                   Optional uiPrefix As String = Nothing) As Object
        Dim logMessage As String = functionName & " failed"
        If Not String.IsNullOrWhiteSpace(uiPrefix) Then logMessage &= ". " & uiPrefix.Trim()

        AppGlobals.BSlogg.Error(ex, logMessage)

        If String.IsNullOrWhiteSpace(uiPrefix) Then Return fallback

        Return uiPrefix & ex.Message
    End Function

    Friend Function LoggedUdfExceptionText(functionName As String, ex As Exception) As String
        AppGlobals.BSlogg.Error(ex, functionName & " failed")
        Return ex.GetType().Name & ": " & ex.Message
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
        values = New List(Of Double)()
        Dim col(,) As Object = Nothing
        Dim inferredName As String = Nothing
        If Not TryGetTrimmedColumnObject(v, col, inferredName, "numeric") Then Return False

        Dim rows As Integer = col.GetLength(0)
        If rows < 1 Then Return False

        For i As Integer = 0 To rows - 1
            Dim d As Double? = TryGetDouble(col(i, 0))
            If d.HasValue Then
                values.Add(d.Value)
            Else
                values.Add(Double.NaN)
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
        mat = Nothing
        rows = 0
        cols = 0
        Dim raw(,) As Object = Nothing
        Dim inferredNames() As String = Nothing
        If Not TryGetTrimmedNumericMatrixObject(v, raw, inferredNames) Then Return False

        rows = raw.GetLength(0)
        cols = raw.GetLength(1)
        If rows < 1 OrElse cols < 1 Then Return False

        ReDim mat(rows - 1, cols - 1)
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                Dim d As Double? = TryGetDouble(raw(i, j))
                If d.HasValue Then
                    mat(i, j) = d.Value
                Else
                    mat(i, j) = Double.NaN
                End If
            Next
        Next
        Return True
    End Function


    Friend Function HasOnlyFinite(values() As Double, Optional requirePositive As Boolean = False) As Boolean
        If values Is Nothing Then Return True

        For Each v As Double In values
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then Return False
            If requirePositive AndAlso v <= 0.0R Then Return False
        Next

        Return True
    End Function

    Friend Function TryExtractIntegerOutcomeColumn(data As DataObj, ByRef values As List(Of Integer)) As Boolean
        values = New List(Of Integer)()

        If data Is Nothing OrElse data.nRows < 1 OrElse data.nCols < 1 Then Return False

        For i As Integer = 0 To data.nRows - 1
            Dim d As Double = CDbl(data.FinalData(i, 0))
            If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return False

            Dim rounded As Double = Math.Round(d)
            If Math.Abs(d - rounded) > 0.0000001R Then Return False

            values.Add(CInt(rounded))
        Next

        Return values.Count > 0
    End Function

    ''' <summary>
    ''' Extracts numeric values column-wise from an object matrix, optionally using the first row as column names.
    ''' Non-numeric cells are ignored.
    ''' </summary>
    ''' <param name="mat">The source object matrix.</param>
    ''' <param name="startRow">The first row to scan for numeric data.</param>
    ''' <param name="hasHeader">When <c>True</c>, the first row of <paramref name="mat"/> supplies output names.</param>
    ''' <param name="defaultPrefix">Prefix used for generated names when no header row is present.</param>
    ''' <param name="groups">On success, receives one numeric array per non-empty input column.</param>
    ''' <param name="names">On success, receives one name per returned group.</param>
    ''' <returns><c>True</c> when extraction succeeds; otherwise, <c>False</c>.</returns>
    Friend Function TryExtractNumericGroupsFromMatrix(mat As Object(,),
                                                  startRow As Integer,
                                                  hasHeader As Boolean,
                                                  defaultPrefix As String,
                                                  ByRef groups()() As Double, ByRef names() As String) As Boolean
        groups = Nothing
        names = Nothing

        If mat Is Nothing Then Return False

        Dim rows As Integer = mat.GetLength(0)
        Dim cols As Integer = mat.GetLength(1)
        If rows < 1 OrElse cols < 1 Then Return False

        Dim groupList As New List(Of Double())()
        Dim nameList As New List(Of String)()

        For c As Integer = 0 To cols - 1
            Dim values As New List(Of Double)(Math.Max(0, rows - startRow))
            For r As Integer = startRow To rows - 1
                Dim d As Double? = TryGetDouble(mat(r, c))
                If d.HasValue Then values.Add(d.Value)
            Next

            If values.Count > 0 Then
                groupList.Add(values.ToArray())
                If hasHeader Then
                    nameList.Add(CellToTrimmedText(mat(0, c)))
                Else
                    nameList.Add(defaultPrefix & (groupList.Count).ToString(CultureInfo.InvariantCulture))
                End If
            End If
        Next

        groups = groupList.ToArray()
        names = nameList.ToArray()
        Return True
    End Function

    ''' <summary>
    ''' Attempts to coerce an Excel argument into a two-dimensional object array, optionally allowing scalar inputs.
    ''' </summary>
    ''' <param name="v">The worksheet argument to coerce.</param>
    ''' <param name="allowScalar">When <c>True</c>, non-range scalar values are wrapped into a 1×1 matrix.</param>
    ''' <param name="mat">On success, receives the coerced object matrix.</param>
    ''' <param name="err">On failure due to an Excel error value, receives the corresponding Excel error.</param>
    ''' <returns><c>True</c> when coercion succeeds; otherwise, <c>False</c>.</returns>
    Friend Function TryCoerceToObjectMatrix(v As Object, allowScalar As Boolean, ByRef mat As Object(,),
                                        ByRef err As ExcelError?) As Boolean
        mat = Nothing
        err = Nothing
        If v Is Nothing OrElse TypeOf v Is ExcelMissing OrElse TypeOf v Is ExcelEmpty Then
            mat = New Object(0, 0) {{ExcelEmpty.Value}}
            Return True
        End If

        If TypeOf v Is ExcelError Then
            err = DirectCast(v, ExcelError)
            Return False
        End If

        Dim arr As Object(,) = Get2D(v)
        If arr IsNot Nothing Then
            mat = arr
            Return True
        End If

        If Not allowScalar Then Return False

        Dim singleValue(0, 0) As Object
        singleValue(0, 0) = v
        mat = singleValue
        Return True
    End Function

    Friend Function ResolveImportedPredictorNames(varNames As Object, inferredNames As String()) As String()
        If inferredNames Is Nothing OrElse inferredNames.Length = 0 Then Return New String() {}
        If varNames Is Nothing OrElse TypeOf varNames Is ExcelEmpty OrElse TypeOf varNames Is ExcelMissing Then Return inferredNames
        Return GetVarNames(varNames, inferredNames.Length)
    End Function

    Friend Function TryGetTrimmedColumnObject(v As Object, ByRef col(,) As Object, ByRef inferredName As String,
                                              Optional headerMode As String = "numeric") As Boolean

        col = Nothing
        inferredName = Nothing

        Dim arr As Object(,) = Get2D(v)
        If arr Is Nothing Then Return False
        If arr.GetLength(1) <> 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim startRow As Integer = 0
        Select Case headerMode.ToLowerInvariant()
            Case "binary"
                If HasBinaryColumnHeader(arr, lastRow) Then startRow = 1
            Case "text"
                If HasTextColumnHeaderForWholeColumnReference(v, arr, lastRow) Then startRow = 1
            Case Else
                If HasNumericColumnHeader(arr, lastRow) Then startRow = 1
        End Select

        If startRow = 1 AndAlso Not IsBlankCell(arr(0, 0)) Then
            inferredName = Convert.ToString(arr(0, 0)).Trim()
        End If

        Dim rows As Integer = lastRow - startRow + 1
        If rows < 1 Then Return False

        ReDim col(rows - 1, 0)
        For i As Integer = 0 To rows - 1
            col(i, 0) = arr(startRow + i, 0)
        Next

        Return True
    End Function

    Friend Function TryGetTrimmedNumericMatrixObject(v As Object, ByRef mat(,) As Object,
                                                     ByRef inferredNames As String()) As Boolean

        mat = Nothing
        inferredNames = Nothing

        Dim arr As Object(,) = Get2D(v)
        If arr Is Nothing Then Return False

        Dim cols As Integer = arr.GetLength(1)
        If cols < 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim startRow As Integer = If(HasNumericMatrixHeader(arr, lastRow), 1, 0)
        Dim rows As Integer = lastRow - startRow + 1
        If rows < 1 Then Return False

        ReDim inferredNames(cols - 1)
        For j As Integer = 0 To cols - 1
            inferredNames(j) = "X" & (j + 1).ToString()
            If startRow = 1 AndAlso Not IsBlankCell(arr(0, j)) Then
                inferredNames(j) = Convert.ToString(arr(0, j)).Trim()
            End If
        Next

        ReDim mat(rows - 1, cols - 1)
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                mat(i, j) = arr(startRow + i, j)
            Next
        Next

        Return True
    End Function

    Friend Function TryGetAlignedClusterIdColumnObject(v As Object,
                                                       expectedRows As Integer,
                                                       ByRef col(,) As Object,
                                                       ByRef inferredName As String) As Boolean
        col = Nothing
        inferredName = Nothing

        Dim arr As Object(,) = Get2D(v)
        If arr Is Nothing Then Return False
        If arr.GetLength(1) <> 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim usedRows As Integer = lastRow + 1
        Dim startRow As Integer = 0

        If usedRows = expectedRows + 1 Then
            If Not IsBlankCell(arr(0, 0)) Then
                inferredName = Convert.ToString(arr(0, 0)).Trim()
            End If
            startRow = 1
        ElseIf usedRows = expectedRows Then
            startRow = 0
        ElseIf HasTextColumnHeaderForWholeColumnReference(v, arr, lastRow) AndAlso usedRows - 1 = expectedRows Then
            If Not IsBlankCell(arr(0, 0)) Then
                inferredName = Convert.ToString(arr(0, 0)).Trim()
            End If
            startRow = 1
        Else
            Return False
        End If

        ReDim col(expectedRows - 1, 0)
        For i As Integer = 0 To expectedRows - 1
            col(i, 0) = arr(startRow + i, 0)
        Next

        Return True
    End Function

    Friend Function TryBuildGeeDataFromUdfArgs(y As Object,
                                               x As Object,
                                               clusterId As Object,
                                               time As Object,
                                               varNames As Object,
                                               offset As Object,
                                               weights As Object,
                                               ByRef data As geeData) As Boolean
        data = Nothing

        Dim yCol(,) As Object = Nothing
        Dim xMat(,) As Object = Nothing
        Dim clusterCol(,) As Object = Nothing
        Dim timeCol(,) As Object = Nothing
        Dim offsetCol(,) As Object = Nothing
        Dim weightCol(,) As Object = Nothing

        Dim yName As String = Nothing
        Dim clusterName As String = Nothing
        Dim timeName As String = Nothing
        Dim offsetName As String = Nothing
        Dim weightName As String = Nothing
        Dim inferredPredictorNames() As String = Nothing

        If Not TryGetTrimmedColumnObject(y, yCol, yName, "numeric") Then Return False
        If Not TryGetTrimmedNumericMatrixObject(x, xMat, inferredPredictorNames) Then Return False

        Dim rowCount As Integer = yCol.GetLength(0)
        If rowCount < 1 Then Return False
        If xMat.GetLength(0) <> rowCount Then Return False
        If Not TryGetAlignedClusterIdColumnObject(clusterId, rowCount, clusterCol, clusterName) Then Return False

        Dim hasTime As Boolean = Not (time Is Nothing OrElse TypeOf time Is ExcelEmpty OrElse TypeOf time Is ExcelMissing)
        Dim hasOffset As Boolean = Not (offset Is Nothing OrElse TypeOf offset Is ExcelEmpty OrElse TypeOf offset Is ExcelMissing)
        Dim hasWeights As Boolean = Not (weights Is Nothing OrElse TypeOf weights Is ExcelEmpty OrElse TypeOf weights Is ExcelMissing)

        If hasTime Then
            If Not TryGetTrimmedColumnObject(time, timeCol, timeName, "numeric") Then Return False
            If timeCol.GetLength(0) <> rowCount Then Return False
        End If

        If hasOffset Then
            If Not TryGetTrimmedColumnObject(offset, offsetCol, offsetName, "numeric") Then Return False
            If offsetCol.GetLength(0) <> rowCount Then Return False
        End If

        If hasWeights Then
            If Not TryGetTrimmedColumnObject(weights, weightCol, weightName, "numeric") Then Return False
            If weightCol.GetLength(0) <> rowCount Then Return False
        End If

        Dim predictorNames As String() = ResolveImportedPredictorNames(varNames, inferredPredictorNames)
        Dim xCols As Integer = xMat.GetLength(1)
        Dim totalCols As Integer = 2 + xCols + If(hasTime, 1, 0) + If(hasOffset, 1, 0) + If(hasWeights, 1, 0)

        Dim raw(rowCount - 1, totalCols - 1) As Object
        Dim names(totalCols - 1) As String

        names(0) = If(String.IsNullOrWhiteSpace(yName), "Y", yName)
        For i As Integer = 0 To rowCount - 1
            raw(i, 0) = yCol(i, 0)
        Next

        For j As Integer = 0 To xCols - 1
            names(j + 1) = predictorNames(j)
            For i As Integer = 0 To rowCount - 1
                raw(i, j + 1) = xMat(i, j)
            Next
        Next

        Dim nextCol As Integer = 1 + xCols
        names(nextCol) = If(String.IsNullOrWhiteSpace(clusterName), "ClusterID", clusterName)

        Dim clusterCodes As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        For i As Integer = 0 To rowCount - 1
            Dim key As String = Convert.ToString(clusterCol(i, 0)).Trim()
            If String.IsNullOrWhiteSpace(key) Then Return False

            Dim code As Integer = 0
            If Not clusterCodes.TryGetValue(key, code) Then
                code = clusterCodes.Count + 1
                clusterCodes.Add(key, code)
            End If

            raw(i, nextCol) = code
        Next
        nextCol += 1

        If hasTime Then
            names(nextCol) = If(String.IsNullOrWhiteSpace(timeName), "Time", timeName)
            For i As Integer = 0 To rowCount - 1
                raw(i, nextCol) = timeCol(i, 0)
            Next
            nextCol += 1
        End If

        If hasOffset Then
            names(nextCol) = If(String.IsNullOrWhiteSpace(offsetName), "Offset", offsetName)
            For i As Integer = 0 To rowCount - 1
                raw(i, nextCol) = offsetCol(i, 0)
            Next
            nextCol += 1
        End If

        If hasWeights Then
            names(nextCol) = If(String.IsNullOrWhiteSpace(weightName), "Weight", weightName)
            For i As Integer = 0 To rowCount - 1
                raw(i, nextCol) = weightCol(i, 0)
            Next
        End If

        Dim out As New geeData With {
                .bTime = hasTime,
                .bOffset = hasOffset,
                .bWeights = hasWeights
            }

        out.DataImportRawMatrix(raw, names)
        If out.bZeroValid OrElse out.nRows < 1 Then Return False

        data = out
        Return True
    End Function

    Friend Function TryBuildGlmDataFromUdfArgs(y As Object, x As Object, varNames As Object, offset As Object,
                                               weights As Object, ByRef data As glmData) As Boolean

        data = Nothing

        Dim yCol(,) As Object = Nothing
        Dim xMat(,) As Object = Nothing
        Dim offsetCol(,) As Object = Nothing
        Dim weightCol(,) As Object = Nothing

        Dim yName As String = Nothing
        Dim offsetName As String = Nothing
        Dim weightName As String = Nothing
        Dim inferredPredictorNames() As String = Nothing

        If Not TryGetTrimmedColumnObject(y, yCol, yName, "numeric") Then Return False
        If Not TryGetTrimmedNumericMatrixObject(x, xMat, inferredPredictorNames) Then Return False

        Dim rowCount As Integer = yCol.GetLength(0)
        If xMat.GetLength(0) <> rowCount Then Return False

        Dim hasOffset As Boolean = Not (offset Is Nothing OrElse TypeOf offset Is ExcelEmpty OrElse TypeOf offset Is ExcelMissing)
        Dim hasWeights As Boolean = Not (weights Is Nothing OrElse TypeOf weights Is ExcelEmpty OrElse TypeOf weights Is ExcelMissing)

        If hasOffset Then
            If Not TryGetTrimmedColumnObject(offset, offsetCol, offsetName, "numeric") Then Return False
            If offsetCol.GetLength(0) <> rowCount Then Return False
        End If

        If hasWeights Then
            If Not TryGetTrimmedColumnObject(weights, weightCol, weightName, "numeric") Then Return False
            If weightCol.GetLength(0) <> rowCount Then Return False
        End If

        Dim predictorNames As String() = ResolveImportedPredictorNames(varNames, inferredPredictorNames)
        Dim xCols As Integer = xMat.GetLength(1)
        Dim totalCols As Integer = 1 + xCols + If(hasOffset, 1, 0) + If(hasWeights, 1, 0)

        Dim raw(rowCount - 1, totalCols - 1) As Object
        Dim names(totalCols - 1) As String

        names(0) = If(String.IsNullOrWhiteSpace(yName), "Y", yName)
        For i As Integer = 0 To rowCount - 1
            raw(i, 0) = yCol(i, 0)
        Next

        For j As Integer = 0 To xCols - 1
            names(j + 1) = predictorNames(j)
            For i As Integer = 0 To rowCount - 1
                raw(i, j + 1) = xMat(i, j)
            Next
        Next

        Dim nextCol As Integer = 1 + xCols
        If hasOffset Then
            names(nextCol) = If(String.IsNullOrWhiteSpace(offsetName), "Offset", offsetName)
            For i As Integer = 0 To rowCount - 1
                raw(i, nextCol) = offsetCol(i, 0)
            Next
            nextCol += 1
        End If

        If hasWeights Then
            names(nextCol) = If(String.IsNullOrWhiteSpace(weightName), "Weight", weightName)
            For i As Integer = 0 To rowCount - 1
                raw(i, nextCol) = weightCol(i, 0)
            Next
        End If

        Dim out As New glmData With {
                .bOffset = hasOffset,
                .bWeights = hasWeights
            }

        out.DataImportRawMatrix(raw, names)
        If out.bZeroValid OrElse out.nRows < 1 Then Return False

        data = out
        Return True
    End Function

    Friend Function TryBuildPredictorDataFromUdfArgs(x As Object,
                                                 expectedPredictorNames As String(),
                                                 offset As Object,
                                                 requireOffset As Boolean,
                                                 ByRef data As glmData) As Boolean
        data = Nothing

        If expectedPredictorNames Is Nothing OrElse expectedPredictorNames.Length < 1 Then Return False

        Dim xMat(,) As Object = Nothing
        Dim offsetCol(,) As Object = Nothing
        Dim inferredPredictorNames() As String = Nothing
        Dim offsetName As String = Nothing

        If Not TryGetTrimmedNumericMatrixObject(x, xMat, inferredPredictorNames) Then Return False

        Dim rowCount As Integer = xMat.GetLength(0)
        Dim xCols As Integer = xMat.GetLength(1)
        If xCols <> expectedPredictorNames.Length Then Return False

        Dim hasOffsetArg As Boolean = Not (offset Is Nothing OrElse TypeOf offset Is ExcelEmpty OrElse TypeOf offset Is ExcelMissing)
        If requireOffset AndAlso Not hasOffsetArg Then Return False

        If hasOffsetArg Then
            If Not TryGetTrimmedColumnObject(offset, offsetCol, offsetName, "numeric") Then Return False
            If offsetCol.GetLength(0) <> rowCount Then Return False
        End If

        Dim totalCols As Integer = xCols + If(hasOffsetArg, 1, 0)
        Dim raw(rowCount - 1, totalCols - 1) As Object
        Dim names(totalCols - 1) As String

        For j As Integer = 0 To xCols - 1
            names(j) = expectedPredictorNames(j)
            For i As Integer = 0 To rowCount - 1
                raw(i, j) = xMat(i, j)
            Next
        Next

        If hasOffsetArg Then
            names(xCols) = If(String.IsNullOrWhiteSpace(offsetName), "Offset", offsetName)
            For i As Integer = 0 To rowCount - 1
                raw(i, xCols) = offsetCol(i, 0)
            Next
        End If

        Dim out As New glmData With {
                .bOffset = hasOffsetArg,
                .bWeights = False
            }

        out.DataImportRawMatrix(raw, names)
        If out.bZeroValid OrElse out.nRows < 1 Then Return False

        data = out
        Return True
    End Function

    Friend Function TryBuildCoxDataFromUdfArgs(time As Object, status As Object, x As Object, varNames As Object,
                                               strata As Object, ByRef data As CoxPHData) As Boolean
        data = Nothing

        Dim timeCol(,) As Object = Nothing
        Dim statusCol(,) As Object = Nothing
        Dim strataCol(,) As Object = Nothing
        Dim xMat(,) As Object = Nothing

        Dim timeName As String = Nothing
        Dim statusName As String = Nothing
        Dim strataName As String = Nothing
        Dim inferredPredictorNames() As String = Nothing

        If Not TryGetTrimmedColumnObject(time, timeCol, timeName, "numeric") Then Return False
        If Not TryGetTrimmedColumnObject(status, statusCol, statusName, "binary") Then Return False
        If Not TryGetTrimmedNumericMatrixObject(x, xMat, inferredPredictorNames) Then Return False

        Dim rowCount As Integer = timeCol.GetLength(0)
        If statusCol.GetLength(0) <> rowCount Then Return False
        If xMat.GetLength(0) <> rowCount Then Return False

        Dim hasStrata As Boolean = Not (strata Is Nothing OrElse TypeOf strata Is ExcelEmpty OrElse TypeOf strata Is ExcelMissing)
        If hasStrata Then
            If Not TryGetTrimmedColumnObject(strata, strataCol, strataName, "text") Then Return False
            If strataCol.GetLength(0) <> rowCount Then Return False
        End If

        Dim predictorNames As String() = ResolveImportedPredictorNames(varNames, inferredPredictorNames)
        Dim xCols As Integer = xMat.GetLength(1)
        Dim totalCols As Integer = 2 + If(hasStrata, 1, 0) + xCols

        Dim raw(rowCount - 1, totalCols - 1) As Object
        Dim names(totalCols - 1) As String

        names(0) = If(String.IsNullOrWhiteSpace(timeName), "Time", timeName)
        names(1) = If(String.IsNullOrWhiteSpace(statusName), "Status", statusName)

        For i As Integer = 0 To rowCount - 1
            raw(i, 0) = timeCol(i, 0)
            raw(i, 1) = statusCol(i, 0)
        Next

        Dim firstPredictorCol As Integer = 2
        If hasStrata Then
            names(2) = If(String.IsNullOrWhiteSpace(strataName), "Strata", strataName)
            For i As Integer = 0 To rowCount - 1
                raw(i, 2) = strataCol(i, 0)
            Next
            firstPredictorCol = 3
        End If

        For j As Integer = 0 To xCols - 1
            names(firstPredictorCol + j) = predictorNames(j)
            For i As Integer = 0 To rowCount - 1
                raw(i, firstPredictorCol + j) = xMat(i, j)
            Next
        Next

        Dim out As New CoxPHData With {
                .bStrata = hasStrata
            }

        out.DataImportRawMatrix(raw, names, CharCols:=If(hasStrata, 2, -1))
        If out.bZeroValid OrElse out.nRows < 1 Then Return False

        data = out
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
    ''' Finds the last row in a two-dimensional array that contains any nonblank cell.
    ''' </summary>
    ''' <param name="arr">The two-dimensional worksheet array to inspect.</param>
    ''' <returns>The zero-based last nonblank row index, or -1 when all rows are blank.</returns>
    Friend Function FindLastNonBlankRow(arr As Object(,)) As Integer
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
    Friend Function HasNumericColumnHeader(arr As Object(,), lastRow As Integer) As Boolean
        If arr Is Nothing OrElse arr.GetLength(1) <> 1 Then Return False
        If lastRow < 1 Then Return False
        If IsBlankCell(arr(0, 0)) Then Return False
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
    Friend Function HasBinaryColumnHeader(arr As Object(,), lastRow As Integer) As Boolean
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
    Friend Function HasTextColumnHeaderForWholeColumnReference(originalArg As Object, arr As Object(,), lastRow As Integer) As Boolean
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
    Friend Function HasNumericMatrixHeader(arr As Object(,), lastRow As Integer) As Boolean
        If arr Is Nothing Then Return False
        If lastRow < 1 Then Return False

        Dim cols As Integer = arr.GetLength(1)
        Dim anyNonBlankNonNumericFirstRow As Boolean = False
        Dim anyNumericFirstRow As Boolean = False
        For j As Integer = 0 To cols - 1
            If IsBlankCell(arr(0, j)) Then Continue For

            If TryGetDouble(arr(0, j)).HasValue Then
                anyNumericFirstRow = True
            Else
                anyNonBlankNonNumericFirstRow = True
            End If
        Next

        ' A real numeric-matrix header should look like labels, not like a data row
        ' with one or more missing numeric values. This prevents explicit data ranges
        ' whose first observation has missing values from being shortened before row-wise
        ' complete-case filtering aligns y, x, subject, and visit.
        If Not anyNonBlankNonNumericFirstRow Then Return False
        If anyNumericFirstRow Then Return False

        For j As Integer = 0 To cols - 1
            If Not TryGetDouble(arr(1, j)).HasValue Then
                Return False
            End If
        Next

        Return True
    End Function

    Friend Function ParseFamilyCode(v As Object) As String
        Dim s As String = AsString(v)
        If String.IsNullOrWhiteSpace(s) Then Return "Gaussian"

        Dim key As String = NormalizeKey(s)
        Select Case key
            Case "binomial", "binary", "logistic"
                Return "Binomial"
            Case "poisson", "count"
                Return "Poisson"
            Case "negativebinomial", "negativebinom", "negativebin", "negbin", "nb", "nb2"
                Return "NegativeBinomial"
            Case "gaussian", "normal"
                Return "Gaussian"
            Case "gamma"
                Return "Gamma"
            Case Else
                Return Nothing
        End Select
    End Function

    Friend Function ParseLinkName(v As Object, familyDisplayName As String) As String
        Dim s As String = AsString(v)
        If String.IsNullOrWhiteSpace(s) Then
            Return regression.GetCanonicalLinkFromDisplayName(familyDisplayName)
        End If

        Dim key As String = NormalizeKey(s)
        Select Case key
            Case "logit"
                Return "Logit"
            Case "probit"
                Return "Probit"
            Case "log"
                Return "Log"
            Case "identity", "id"
                Return "Identity"
            Case "sqrt", "squareroot"
                Return "Sqrt"
            Case "inverse", "reciprocal"
                Return "Inverse"
            Case "power"
                Return "Power"
            Case Else
                Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Wraps a residual or leverage vector in a spilled-object array.
    ''' </summary>
    ''' <param name="vec">Vector of per-observation values.</param>
    ''' <param name="header">Column label to use when <paramref name="includeHeader"/> is True.</param>
    ''' <param name="includeHeader">Whether to include a header row.</param>
    ''' <returns>A spilled-object array containing the requested vector.</returns>
    Friend Function BuildResidualVectorOutput(vec() As Double, header As String, includeHeader As Boolean) As Object
        If vec Is Nothing Then Return ExcelError.ExcelErrorNA

        Dim n As Integer = vec.Length
        Dim outRows As Integer = If(includeHeader, n + 1, n)
        Dim out(outRows - 1, 0) As Object
        Dim r0 As Integer = 0

        If includeHeader Then
            out(0, 0) = header
            r0 = 1
        End If

        For i As Integer = 0 To n - 1
            out(r0 + i, 0) = vec(i)
        Next

        Return out
    End Function

    Friend Function TryPrepareInterceptOnlyPredictionInputs(newOffset As Object,
                                                        requireOffset As Boolean,
                                                        ByRef nRows As Integer,
                                                        ByRef offsetVals() As Double) As Boolean
        nRows = 0
        offsetVals = Nothing

        Dim hasOffsetArg As Boolean = Not IsMissingArg(newOffset)
        If requireOffset AndAlso Not hasOffsetArg Then Return False

        If hasOffsetArg Then
            Dim values As List(Of Double) = Nothing
            If Not UDFhelpers.TryReadNumericColumn(newOffset, values) Then Return False
            If values Is Nothing OrElse values.Count < 1 Then Return False
            offsetVals = values.ToArray()
            If Not UDFhelpers.HasOnlyFinite(offsetVals) Then Return False
            nRows = offsetVals.Length
            Return True
        End If

        nRows = 1
        Return True
    End Function


    Friend Function ComputeLinearPredictor(expandedX(,) As Double,
                                            rowIndex As Integer,
                                            beta() As Double,
                                            includeIntercept As Boolean,
                                            offsetVals() As Double) As Double
        Dim eta As Double = 0.0R
        Dim startBeta As Integer = 0

        If includeIntercept AndAlso beta IsNot Nothing AndAlso beta.Length > 0 Then
            eta = beta(0)
            startBeta = 1
        End If

        If expandedX IsNot Nothing Then
            Dim p As Integer = expandedX.GetLength(1)
            For j As Integer = 0 To p - 1
                eta += expandedX(rowIndex, j) * beta(startBeta + j)
            Next
        End If

        If offsetVals IsNot Nothing AndAlso rowIndex >= 0 AndAlso rowIndex < offsetVals.Length Then
            eta += offsetVals(rowIndex)
        End If

        Return eta
    End Function

    Friend Function SafeExcelNumber(value As Double) As Object
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return ExcelError.ExcelErrorNum
        Return value
    End Function

    ''' <summary>
    ''' Normalizes a string for token or key matching by trimming whitespace, optionally changing case,
    ''' and removing selected separator characters.
    ''' </summary>
    ''' <param name="value">The text to normalize.</param>
    ''' <param name="toUpper">When <c>True</c>, returns upper-case text; otherwise lower-case text.</param>
    ''' <param name="removeUnderscore">When <c>True</c>, underscore characters are removed in addition to spaces and hyphens.</param>
    ''' <returns>
    ''' A normalized comparison token, or an empty string when <paramref name="value"/> is <c>Nothing</c>.
    ''' </returns>
    Friend Function NormalizeMatchToken(value As String, Optional toUpper As Boolean = True, Optional removeUnderscore As Boolean = False) As String
        If value Is Nothing Then Return ""

        Dim normalized As String = value.Trim()
        normalized = If(toUpper, normalized.ToUpperInvariant(), normalized.ToLowerInvariant())
        normalized = normalized.Replace(" ", String.Empty).Replace("-", String.Empty)
        If removeUnderscore Then normalized = normalized.Replace("_", String.Empty)
        Return normalized
    End Function

    ''' <summary>
    ''' Normalizes an optional text argument for case-insensitive method matching.
    ''' </summary>
    ''' <param name="v">The input value to normalize.</param>
    ''' <returns>
    ''' An upper-case, trimmed string representation of <paramref name="v"/>.
    ''' Returns an empty string for missing, empty, or null-like Excel arguments.
    ''' </returns>
    Friend Function NormalizeText(v As Object) As String
        Return NormalizeMatchToken(AsString(v), toUpper:=True, removeUnderscore:=False)
    End Function

    ''' <summary>
    ''' Normalizes an optional token-style worksheet argument for case-insensitive option parsing.
    ''' </summary>
    ''' <param name="arg">The value to normalize.</param>
    ''' <returns>
    ''' An upper-case compact token with spaces and hyphens removed, or an empty string for missing input.
    ''' </returns>
    Public Function NormalizeToken(arg As Object) As String
        Return NormalizeMatchToken(AsString(arg), toUpper:=True, removeUnderscore:=False)
    End Function

    ''' <summary>
    ''' Normalizes a key for case-insensitive dictionary or option matching.
    ''' </summary>
    ''' <param name="value">The key text to normalize.</param>
    ''' <returns>
    ''' A lower-case compact key with spaces, underscores, and hyphens removed.
    ''' </returns>
    Friend Function NormalizeKey(value As String) As String
        Return NormalizeMatchToken(value, toUpper:=False, removeUnderscore:=True)
    End Function

    Friend Function LooksLikeHeaderRow(arr As Object(,), numericCols As Integer()) As Boolean
        Dim rows As Integer = arr.GetLength(0)
        If rows < 2 Then Return False

        Dim anyNonNumeric As Boolean = False
        For Each c In numericCols
            If Not TryGetDouble(arr(0, c)).HasValue Then
                anyNonNumeric = True
                Exit For
            End If
        Next
        If Not anyNonNumeric Then Return False

        For Each c In numericCols
            Dim foundNumericBelow As Boolean = False
            For r As Integer = 1 To rows - 1
                If TryGetDouble(arr(r, c)).HasValue Then
                    foundNumericBelow = True
                    Exit For
                End If
            Next
            If Not foundNumericBelow Then Return False
        Next

        Return True
    End Function

    Friend Function LooksLikeSingleColumnHeader(arr As Object(,)) As Boolean
        If arr Is Nothing Then Return False
        If arr.GetLength(1) <> 1 Then Return False

        Dim rows As Integer = arr.GetLength(0)
        If rows < 2 Then Return False
        If TryGetDouble(arr(0, 0)).HasValue Then Return False

        For r As Integer = 1 To rows - 1
            If TryGetDouble(arr(r, 0)).HasValue Then Return True
        Next

        Return False
    End Function

    Friend Function TryReadGroupedNumericColumns(input As Object, ByRef groups()() As Double, ByRef names() As String) As Boolean
        groups = Nothing
        names = Nothing
        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If cols < 1 OrElse rows < 1 Then Return False

        Dim hasHeader As Boolean = LooksLikeHeaderRow(arr, Enumerable.Range(0, cols).ToArray())
        Dim startRow As Integer = If(hasHeader, 1, 0)

        Return TryExtractNumericGroupsFromMatrix(arr, startRow:=startRow, hasHeader:=hasHeader,
                                             defaultPrefix:="Group ", groups:=groups, names:=names)
    End Function

    Friend Function TryReadIndependentNumericColumns(x As Object, y As Object, ByRef groups()() As Double, ByRef names() As String) As Boolean
        groups = Nothing
        names = Nothing

        Dim ax As Object(,) = Get2D(x)
        Dim ay As Object(,) = Get2D(y)
        If ax Is Nothing OrElse ay Is Nothing Then Return False
        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return False

        Dim hasHeaderX As Boolean = LooksLikeSingleColumnHeader(ax)
        Dim hasHeaderY As Boolean = LooksLikeSingleColumnHeader(ay)

        Dim startRowX As Integer = If(hasHeaderX, 1, 0)
        Dim startRowY As Integer = If(hasHeaderY, 1, 0)

        Dim gx As New List(Of Double)
        For r As Integer = startRowX To ax.GetLength(0) - 1
            Dim d = TryGetDouble(ax(r, 0))
            If d.HasValue Then gx.Add(d.Value)
        Next

        Dim gy As New List(Of Double)
        For r As Integer = startRowY To ay.GetLength(0) - 1
            Dim d = TryGetDouble(ay(r, 0))
            If d.HasValue Then gy.Add(d.Value)
        Next

        groups = New Double()() {gx.ToArray(), gy.ToArray()}
        names = New String() {
            If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Group 1"),
            If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Group 2")
        }

        Return True
    End Function

    Friend Function TryReadPairedNumericColumns(x As Object, y As Object, ByRef mat As Double(,), ByRef names() As String) As Boolean
        mat = Nothing
        names = Nothing
        Dim ax As Object(,) = Get2D(x)
        Dim ay As Object(,) = Get2D(y)
        If ax Is Nothing OrElse ay Is Nothing Then Return False
        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return False
        If ax.GetLength(0) <> ay.GetLength(0) Then Return False

        Dim hasHeaderX As Boolean = LooksLikeSingleColumnHeader(ax)
        Dim hasHeaderY As Boolean = LooksLikeSingleColumnHeader(ay)
        If hasHeaderX <> hasHeaderY Then Return False

        names = New String() {
                If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Sample 1"),
                If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Sample 2")
            }

        Dim pairs As List(Of Double()) = ExtractCompletePairedNumericRows(ax, ay)
        If pairs.Count = 0 Then Return True

        mat = New Double(pairs.Count - 1, 1) {}
        For r As Integer = 0 To pairs.Count - 1
            mat(r, 0) = pairs(r)(0)
            mat(r, 1) = pairs(r)(1)
        Next
        Return True
    End Function

    ''' <summary>
    ''' Extracts complete numeric pairs from two aligned one-column object matrices.
    ''' </summary>
    ''' <param name="ax">First aligned one-column matrix.</param>
    ''' <param name="ay">Second aligned one-column matrix.</param>
    ''' <returns>A list containing one two-element array per row where both cells are numeric.</returns>
    Friend Function ExtractCompletePairedNumericRows(ax As Object(,), ay As Object(,)) As List(Of Double())
        Dim pairs As New List(Of Double())()

        If ax Is Nothing OrElse ay Is Nothing Then Return pairs
        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return pairs
        If ax.GetLength(0) <> ay.GetLength(0) Then Return pairs

        For r As Integer = 0 To ax.GetLength(0) - 1
            Dim dx As Double? = TryGetDouble(ax(r, 0))
            Dim dy As Double? = TryGetDouble(ay(r, 0))
            If dx.HasValue AndAlso dy.HasValue Then
                pairs.Add(New Double() {dx.Value, dy.Value})
            End If
        Next

        Return pairs
    End Function

    ''' <summary>
    ''' Coerces a worksheet argument into a two-dimensional object array, treating scalar values as 1×1 matrices.
    ''' </summary>
    ''' <param name="v">The worksheet argument to coerce.</param>
    ''' <returns>
    ''' A two-dimensional object array when coercion succeeds; otherwise, <c>Nothing</c>.
    ''' </returns>
    Friend Function Get2DOrScalar(v As Object) As Object(,)
        Dim mat As Object(,) = Nothing
        Dim err As ExcelError? = Nothing
        If TryCoerceToObjectMatrix(v, allowScalar:=True, mat:=mat, err:=err) Then Return mat
        Return Nothing
    End Function

    ''' <summary>
    ''' Coerces a worksheet argument into a two-dimensional object array suitable for permissive UDF parsing.
    ''' </summary>
    ''' <param name="v">The worksheet argument to coerce.</param>
    ''' <param name="err">On failure due to an Excel error value, receives the corresponding Excel error.</param>
    ''' <returns>
    ''' A two-dimensional object array when coercion succeeds; otherwise, <c>Nothing</c>.
    ''' Missing arguments are returned as a 1×1 matrix containing <see cref="ExcelEmpty.Value"/>.
    ''' </returns>
    Friend Function CoerceToObjectMatrix(v As Object, ByRef err As ExcelError?) As Object(,)
        Dim mat As Object(,) = Nothing
        If TryCoerceToObjectMatrix(v, allowScalar:=True, mat:=mat, err:=err) Then Return mat
        Return Nothing
    End Function

    ''' <summary>
    ''' Counts the number of distinct outcome categories present in the filtered regression data matrix.
    ''' </summary>
    ''' <param name="fitData">Regression matrix whose first column contains the outcome.</param>
    ''' <returns>The number of distinct integer-valued response categories observed.</returns>
    Friend Function CountDistinctOutcomeCategories(fitData(,) As Double) As Integer
        If fitData Is Nothing Then Return 0
        Dim n As Integer = fitData.GetLength(0)
        Dim cats As New HashSet(Of Integer)()
        For i As Integer = 0 To n - 1
            cats.Add(CInt(Math.Round(fitData(i, 0))))
        Next
        Return cats.Count
    End Function


    ''' <summary>
    ''' Parses the reference-category option supplied to the multinomial-logit fit function.
    ''' </summary>
    ''' <param name="v">Worksheet argument containing the requested reference direction.</param>
    ''' <returns>The parsed reference-category choice, defaulting to <see cref="regression.ReferenceCategory.Last"/>.</returns>
    Friend Function ParseReferenceCategory(v As Object) As regression.ReferenceCategory
        Dim s As String = AsString(v)
        If String.IsNullOrWhiteSpace(s) Then Return regression.ReferenceCategory.Last

        Select Case s.Trim().ToLowerInvariant()
            Case "first", "smallest", "min"
                Return regression.ReferenceCategory.First
            Case Else
                Return regression.ReferenceCategory.Last
        End Select
    End Function

    ''' <summary>
    ''' Builds category-specific column headers for residual or probability outputs.
    ''' </summary>
    ''' <param name="prefix">Prefix describing the quantity shown in each category column.</param>
    ''' <param name="categories">Outcome categories in model order.</param>
    ''' <returns>An array of column labels aligned with the category-specific matrix.</returns>
    Friend Function CategoryHeaders(prefix As String, categories() As Integer) As String()
        If categories Is Nothing Then Return New String() {}
        Dim out(categories.Length - 1) As String
        For i As Integer = 0 To categories.Length - 1
            out(i) = prefix & "(" & categories(i).ToString(CultureInfo.InvariantCulture) & ")"
        Next
        Return out
    End Function

    ''' <summary>
    ''' Wraps a category-specific residual matrix in a spilled-object array.
    ''' </summary>
    ''' <param name="mat">Residual matrix with one row per observation and one column per category.</param>
    ''' <param name="headers">Column headers aligned with <paramref name="mat"/>.</param>
    ''' <param name="includeHeader">Whether to include a header row.</param>
    ''' <returns>A spilled-object array containing the requested residual matrix.</returns>
    Friend Function BuildResidualMatrixOutput(mat(,) As Double, headers() As String, includeHeader As Boolean) As Object
        If mat Is Nothing Then Return ExcelError.ExcelErrorNA

        Dim n As Integer = mat.GetLength(0)
        Dim p As Integer = mat.GetLength(1)
        Dim outRows As Integer = If(includeHeader, n + 1, n)
        Dim out(outRows - 1, p - 1) As Object
        Dim r0 As Integer = 0

        If includeHeader Then
            For j As Integer = 0 To p - 1
                out(0, j) = headers(j)
            Next
            r0 = 1
        End If

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To p - 1
                out(r0 + i, j) = mat(i, j)
            Next
        Next

        Return out
    End Function

    Friend Function CloneStringArray(values() As String) As String()
        If values Is Nothing Then Return Nothing
        Return DirectCast(values.Clone(), String())
    End Function

    Friend Function ExtractNumericColumnIgnoringNonNumeric(arg As Object, ByRef err As ExcelError?) As Double()
        err = Nothing

        Dim arr As Object(,) = CoerceToObjectMatrix(arg, err)
        If err.HasValue OrElse arr Is Nothing Then Return Array.Empty(Of Double)()

        If arr.GetLength(1) <> 1 Then
            err = ExcelError.ExcelErrorValue
            Return Array.Empty(Of Double)()
        End If

        Dim values As New List(Of Double)(arr.GetLength(0))
        For r As Integer = 0 To arr.GetLength(0) - 1
            Dim d As Double? = TryGetDouble(arr(r, 0))
            If d.HasValue Then values.Add(d.Value)
        Next

        Return values.ToArray()
    End Function

    Friend Function ExtractPairedNumericColumnsIgnoringNonNumeric(x As Object, y As Object,
                                                              ByRef err As ExcelError?) As Double(,)
        err = Nothing

        Dim ax As Object(,) = CoerceToObjectMatrix(x, err)
        If err.HasValue OrElse ax Is Nothing Then Return Nothing

        Dim ay As Object(,) = CoerceToObjectMatrix(y, err)
        If err.HasValue OrElse ay Is Nothing Then Return Nothing

        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 OrElse ax.GetLength(0) <> ay.GetLength(0) Then
            err = ExcelError.ExcelErrorValue
            Return Nothing
        End If

        Dim pairs As List(Of Double()) = ExtractCompletePairedNumericRows(ax, ay)
        If pairs.Count = 0 Then Return Nothing

        Dim out(pairs.Count - 1, 1) As Double
        For i As Integer = 0 To pairs.Count - 1
            out(i, 0) = pairs(i)(0)
            out(i, 1) = pairs(i)(1)
        Next

        Return out
    End Function

    Friend Function ExtractNumericGroupsFromColumnsIgnoringNonNumeric(arg As Object, ByRef err As ExcelError?) As Double()()
        err = Nothing

        Dim mat As Object(,) = CoerceToObjectMatrix(arg, err)
        If err.HasValue OrElse mat Is Nothing Then Return Nothing

        Dim groups()() As Double = Nothing
        Dim names() As String = Nothing
        If Not TryExtractNumericGroupsFromMatrix(mat, startRow:=0, hasHeader:=False,
                                             defaultPrefix:="Group ", groups:=groups, names:=names) Then
            err = ExcelError.ExcelErrorValue
            Return Nothing
        End If

        Return groups
    End Function

    Friend Function ExtractCompleteNumericMatrixCompleteCases(input As Object, ByRef mat As Double(,),
                                                              ByRef noRows As Integer, ByRef noCols As Integer) As Boolean
        mat = Nothing
        noRows = 0
        noCols = 0

        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False

        Dim r As Integer = arr.GetLength(0)
        Dim c As Integer = arr.GetLength(1)
        If c < 2 Then Return False

        Dim rows As New List(Of Double())()

        For i As Integer = 0 To r - 1
            Dim rowVals(c - 1) As Double
            Dim ok As Boolean = True

            For j As Integer = 0 To c - 1
                Dim d As Double? = TryGetDouble(arr(i, j))
                If Not d.HasValue Then
                    ok = False
                    Exit For
                End If
                rowVals(j) = d.Value
            Next

            If ok Then rows.Add(rowVals)
        Next

        noRows = rows.Count
        noCols = c
        If noRows < 2 Then Return True

        mat = New Double(noRows - 1, c - 1) {}
        For i As Integer = 0 To noRows - 1
            For j As Integer = 0 To c - 1
                mat(i, j) = rows(i)(j)
            Next
        Next

        Return True
    End Function

    Friend Function TryReadCompleteNumericMatrixWithHeaders(input As Object, ByRef mat As Double(,), ByRef names() As String) As Boolean
        mat = Nothing
        names = Nothing

        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows < 1 OrElse cols < 2 Then Return False

        Dim numericCols As Integer() = Enumerable.Range(0, cols).ToArray()
        Dim hasHeader As Boolean = LooksLikeHeaderRow(arr, numericCols)
        Dim startRow As Integer = If(hasHeader, 1, 0)

        names = New String(cols - 1) {}
        For c As Integer = 0 To cols - 1
            names(c) = If(hasHeader, Convert.ToString(arr(0, c)).Trim(), "Condition " & (c + 1).ToString())
        Next

        Dim keepRows As New List(Of Double())()
        For r As Integer = startRow To rows - 1
            Dim row(cols - 1) As Double
            Dim ok As Boolean = True

            For c As Integer = 0 To cols - 1
                Dim d As Double? = TryGetDouble(arr(r, c))
                If Not d.HasValue Then
                    ok = False
                    Exit For
                End If
                row(c) = d.Value
            Next

            If ok Then keepRows.Add(row)
        Next

        If keepRows.Count < 1 Then Return False

        mat = New Double(keepRows.Count - 1, cols - 1) {}
        For r As Integer = 0 To keepRows.Count - 1
            For c As Integer = 0 To cols - 1
                mat(r, c) = keepRows(r)(c)
            Next
        Next

        Return True
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
    ''' Attempts to read a one-dimensional text vector from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">
    ''' Input supplied either as a delimited string or as a one-row / one-column worksheet range.
    ''' </param>
    ''' <param name="values">Receives the parsed text values when conversion succeeds.</param>
    ''' <returns>
    ''' <c>True</c> when a non-empty vector of trimmed text values was read successfully; otherwise <c>False</c>.
    ''' </returns>
    ''' <remarks>
    ''' Accepted string separators are commas, semicolons, carriage returns, and line feeds.
    ''' Two-dimensional ranges with more than one row and more than one column are rejected.
    ''' </remarks>
    Friend Function TryReadStringVectorArgument(arg As Object, ByRef values() As String) As Boolean
        values = Nothing
        If IsMissingArg(arg) Then Return False
        Dim s As String = TryCast(arg, String)
        If s IsNot Nothing Then
            Dim parts = s.Split({","c, ";"c, ControlChars.Lf, ControlChars.Cr}, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length > 0 Then
                ReDim values(parts.Length - 1)
                For i As Integer = 0 To parts.Length - 1
                    values(i) = parts(i).Trim()
                Next
                Return True
            End If
        End If
        Dim arr As Object(,) = Get2D(arg)
        If arr Is Nothing Then Return False
        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim list As New List(Of String)
        If rows = 1 Then
            For j As Integer = 0 To cols - 1
                list.Add(CellToTrimmedText(arr(0, j)))
            Next
        ElseIf cols = 1 Then
            For i As Integer = 0 To rows - 1
                list.Add(CellToTrimmedText(arr(i, 0)))
            Next
        Else
            Return False
        End If
        values = list.ToArray()
        Return values.Length > 0
    End Function

    ''' <summary>
    ''' Attempts to read a one-dimensional numeric vector from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">
    ''' Input supplied either as a delimited string or as a one-row / one-column worksheet range.
    ''' </param>
    ''' <param name="values">Receives the parsed numeric values when conversion succeeds.</param>
    ''' <returns>
    ''' <c>True</c> when a non-empty vector of finite numeric values was read successfully; otherwise <c>False</c>.
    ''' </returns>
    ''' <remarks>
    ''' For text input, semicolons or line breaks take precedence as separators. When none are present, commas are used.
    ''' Both invariant-culture and current-culture numeric parsing are attempted so that decimal formatting is more tolerant.
    ''' Two-dimensional ranges with more than one row and more than one column are rejected.
    ''' </remarks>
    Friend Function TryReadDoubleVectorArgument(arg As Object, ByRef values() As Double) As Boolean
        values = Nothing
        If IsMissingArg(arg) Then Return False
        Dim s As String = TryCast(arg, String)
        If s IsNot Nothing Then
            Dim separators() As Char
            If s.IndexOf(";"c) >= 0 OrElse s.IndexOf(ControlChars.Lf) >= 0 OrElse s.IndexOf(ControlChars.Cr) >= 0 Then
                separators = New Char() {";"c, ControlChars.Lf, ControlChars.Cr}
            Else
                separators = New Char() {","c}
            End If
            Dim parts = s.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            Dim list As New List(Of Double)
            For Each token As String In parts
                Dim parsed As Double
                If Double.TryParse(token.Trim(), NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, parsed) OrElse
                   Double.TryParse(token.Trim(), NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, parsed) Then
                    list.Add(parsed)
                Else
                    Return False
                End If
            Next
            If list.Count > 0 Then
                values = list.ToArray()
                Return True
            End If
        End If
        Dim arr As Object(,) = Get2D(arg)
        If arr Is Nothing Then Return False
        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim vals As New List(Of Double)
        If rows = 1 Then
            For j As Integer = 0 To cols - 1
                Dim d As Double? = TryGetDouble(arr(0, j))
                If Not d.HasValue Then Return False
                vals.Add(d.Value)
            Next
        ElseIf cols = 1 Then
            For i As Integer = 0 To rows - 1
                Dim d As Double? = TryGetDouble(arr(i, 0))
                If Not d.HasValue Then Return False
                vals.Add(d.Value)
            Next
        Else
            Return False
        End If
        values = vals.ToArray()
        Return values.Length > 0
    End Function

    ''' <summary>
    ''' Attempts to convert a worksheet argument into a <c>DataObj</c> containing numeric predictors.
    ''' </summary>
    ''' <param name="input">
    ''' Numeric worksheet range to import. The helper accepts either a plain numeric block or a block whose first row
    ''' contains column labels.
    ''' </param>
    ''' <param name="varNames">
    ''' Optional replacement variable names. When omitted, inferred names from the worksheet input are used.
    ''' </param>
    ''' <param name="allowMissing">
    ''' When <c>True</c>, missing numeric cells are allowed to pass through to the <c>DataObj</c> import layer.
    ''' When <c>False</c>, the downstream import will keep only complete numeric rows.
    ''' </param>
    ''' <param name="data">Receives the populated <c>DataObj</c> when conversion succeeds.</param>
    ''' <returns>
    ''' <c>True</c> when the input could be trimmed, named, and imported into a non-empty numeric data object;
    ''' otherwise <c>False</c>.
    ''' </returns>
    ''' <remarks>
    ''' This helper centralizes the standard numeric-matrix import path used by the multivariate UDFs.
    ''' It deliberately reuses <c>TryGetTrimmedNumericMatrixObject</c> and <c>ResolveImportedPredictorNames</c>
    ''' so that header detection, trailing blank-row trimming, and predictor-name resolution stay consistent across UDF modules.
    ''' </remarks>
    Friend Function TryBuildNumericDataObject(input As Object, varNames As Object,
                                           allowMissing As Boolean, ByRef data As DataObj) As Boolean
        data = Nothing

        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim raw(,) As Object = Nothing
        Dim inferredNames() As String = Nothing
        If Not TryGetTrimmedNumericMatrixObject(arr, raw, inferredNames) Then Return False

        Dim firstSourceRow As Integer = If(HasNumericMatrixHeader(arr, lastRow), 2, 1)

        Dim imported As New DataObj()
        If allowMissing Then imported.bAllowMissing = True
        imported.DataImportRawMatrix(raw,
                                 ResolveImportedPredictorNames(varNames, inferredNames),
                                 firstSourceRow:=firstSourceRow)

        If imported.bZeroValid OrElse imported.nRows < 1 OrElse imported.nCols < 1 Then Return False
        data = imported
        Return True
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
    ''' Attempts to parse and validate an alpha value from an optional Excel argument.
    ''' </summary>
    ''' <param name="arg">
    ''' The Excel argument to parse. May be missing, numeric, or a string representation of a number.
    ''' </param>
    ''' <param name="alpha">
    ''' When this method returns <c>True</c>, contains the parsed alpha value.
    ''' Defaults to <c>0.05</c> when the argument is missing.
    ''' </param>
    ''' <returns>
    ''' <c>True</c> if a valid alpha in the open interval <c>(0, 1)</c> could be obtained;
    ''' otherwise <c>False</c>.
    ''' </returns>
    Friend Function TryParseAlpha(arg As Object, ByRef alpha As Double) As Boolean
        alpha = 0.05
        If IsMissingArg(arg) Then Return True

        Try
            If TypeOf arg Is String Then
                Dim s As String = Convert.ToString(arg).Trim()
                If Not Double.TryParse(s, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, alpha) AndAlso
                   Not Double.TryParse(s, alpha) Then
                    Return False
                End If
            Else
                alpha = Convert.ToDouble(arg)
            End If
        Catch
            Return False
        End Try

        If Double.IsNaN(alpha) OrElse Double.IsInfinity(alpha) Then Return False
        If alpha <= 0.0 OrElse alpha >= 1.0 Then Return False

        Return True
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
