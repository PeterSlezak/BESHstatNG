Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.WorksheetFunctions
Imports ExcelDna.Integration

Partial Friend Module UdfDataImport
    Private Function IsBinaryCell38(value As Object) As Boolean
        Dim result As Integer
        Return TryGetBinary01(value, result)
    End Function

    ''' <summary>
    ''' Attempts to coerce an Excel argument into a two-dimensional object array.
    ''' </summary>
    ''' <param name="v">The worksheet argument to coerce.</param>
    ''' <returns>
    ''' A two-dimensional object array when coercion succeeds; otherwise, <c>Nothing</c>.
    ''' </returns>
    Friend Function Get2D(v As Object) As Object(,)
        If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return Nothing

        Try
            Return WindowsSpreadsheetTransport.ReadUdfMatrix(v,
                Function(reference) XlCall.Excel(XlCall.xlCoerce, reference))
        Catch
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Reads a one-column numeric range, trimming unused bottom rows and optionally skipping a header row.
    ''' </summary>
    ''' <param name="v">The worksheet argument to read.</param>
    ''' <param name="values">On success, receives one numeric value per retained row. Invalid nonblank cells are returned as <see cref="Double.NaN"/>.</param>
    ''' <returns>True when a one-column input can be read; otherwise, False.</returns>
    Friend Function TryReadNumericColumn(v As Object, ByRef values As List(Of Double)) As Boolean
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
    Friend Function TryReadNumericMatrix(v As Object, ByRef mat As Double(,), ByRef rows As Integer, ByRef cols As Integer) As Boolean
        mat = Nothing
        rows = 0
        cols = 0
        Dim raw(,) As Object = Nothing
        Dim inferredNames() As String = Nothing
        Dim original = Get2D(v)
        If original Is Nothing Then Return False
        If Not TryGetTrimmedNumericMatrixObject(original, raw, inferredNames) Then Return False

        rows = raw.GetLength(0)
        cols = raw.GetLength(1)
        If rows < 1 OrElse cols < 1 Then Return False

        Dim offset As Integer = If(HasNumericMatrixHeader(original, FindLastNonBlankRow(original)), 1, 0)
        mat = WindowsSpreadsheetTransport.NumericMatrix(raw, v, offset)
        Return True
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
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryExtractNumericGroupsFromMatrix(
            mat.GetLength(0), mat.GetLength(1), startRow, hasHeader, defaultPrefix,
            Function(row, column) TryGetDouble(mat(row, column)), Function(row, column) CellToTrimmedText(mat(row, column)), groups, names)
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
        Return Global.BESHStatNG.UdfDataImport.GetVariableNames(varNames, inferredNames.Length)
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

        Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.CopyTrimmedColumnPrepared(arr, startRow, rows, col)

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

        Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.CopyTrimmedMatrixPrepared(arr, startRow, rows, cols, mat)

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

        Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.CopyTrimmedColumnPrepared(arr, startRow, expectedRows, col)

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

        Dim raw(,) As Object = Nothing
        Dim names() As String = Nothing
        Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.InitializeResponsePredictorsPrepared(rowCount, totalCols, xCols, yName, predictorNames, yCol, xMat, raw, names)

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
            Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.CopyNamedColumnPrepared(raw, names, rowCount, nextCol, timeName, "Time", timeCol)
            nextCol += 1
        End If

        If hasOffset Then
            Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.CopyNamedColumnPrepared(raw, names, rowCount, nextCol, offsetName, "Offset", offsetCol)
            nextCol += 1
        End If

        If hasWeights Then
            Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.CopyNamedColumnPrepared(raw, names, rowCount, nextCol, weightName, "Weight", weightCol)
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

        Dim raw(,) As Object = Nothing
        Dim names() As String = Nothing
        Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.InitializeResponsePredictorsPrepared(rowCount, totalCols, xCols, yName, predictorNames, yCol, xMat, raw, names)

        Dim nextCol As Integer = 1 + xCols
        If hasOffset Then
            Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.CopyNamedColumnPrepared(raw, names, rowCount, nextCol, offsetName, "Offset", offsetCol)
            nextCol += 1
        End If

        If hasWeights Then
            Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.CopyNamedColumnPrepared(raw, names, rowCount, nextCol, weightName, "Weight", weightCol)
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
        Dim raw(,) As Object = Nothing
        Dim names() As String = Nothing
        Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.InitializePredictorsPrepared(rowCount, totalCols, xCols, expectedPredictorNames, xMat, raw, names)

        If hasOffsetArg Then
            Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.CopyNamedColumnPrepared(raw, names, rowCount, xCols, offsetName, "Offset", offsetCol)
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

        Dim raw(,) As Object = Nothing
        Dim names() As String = Nothing
        Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.InitializeCoxPrepared(rowCount, totalCols, xCols, timeName, statusName, strataName, predictorNames, timeCol, statusCol, strataCol, xMat, hasStrata, raw, names)

        Dim out As New CoxPHData With {
                .bStrata = hasStrata
            }

        out.DataImportRawMatrix(raw, names, CharCols:=If(hasStrata, 2, -1))
        If out.bZeroValid OrElse out.nRows < 1 Then Return False

        data = out
        Return True
    End Function


    ''' <summary>
    ''' Finds the last row in a two-dimensional array that contains any nonblank cell.
    ''' </summary>
    ''' <param name="arr">The two-dimensional worksheet array to inspect.</param>
    ''' <returns>The zero-based last nonblank row index, or -1 when all rows are blank.</returns>
    Friend Function FindLastNonBlankRow(arr As Object(,)) As Integer
        If arr Is Nothing Then Return -1
        Return Global.BESHStatNG.ExcelCommon.UdfRangePreparation38.FindLastNonBlankRow(arr.GetLength(0), arr.GetLength(1), Function(row, column) IsBlankCell(arr(row, column)))
    End Function

    ''' <summary>
    ''' Determines whether a numeric column has a header row that should be skipped.
    ''' </summary>
    ''' <param name="arr">The one-column worksheet array to inspect.</param>
    ''' <param name="lastRow">The last retained nonblank row index.</param>
    ''' <returns>True when the first row appears to be a header; otherwise, False.</returns>
    Friend Function HasNumericColumnHeader(arr As Object(,), lastRow As Integer) As Boolean
        If arr Is Nothing OrElse arr.GetLength(1) <> 1 Then Return False
        Return Global.BESHStatNG.ExcelCommon.UdfRangePreparation38.HasNumericColumnHeader(lastRow, arr.GetLength(1), Function(row, column) IsBlankCell(arr(row, column)), Function(row, column) TryGetDouble(arr(row, column)).HasValue)
    End Function

    ''' <summary>
    ''' Determines whether a binary column has a header row that should be skipped.
    ''' </summary>
    ''' <param name="arr">The one-column worksheet array to inspect.</param>
    ''' <param name="lastRow">The last retained nonblank row index.</param>
    ''' <returns>True when the first row appears to be a header; otherwise, False.</returns>
    Friend Function HasBinaryColumnHeader(arr As Object(,), lastRow As Integer) As Boolean
        If arr Is Nothing OrElse arr.GetLength(1) <> 1 Then Return False
        Return Global.BESHStatNG.ExcelCommon.UdfRangePreparation38.HasBinaryColumnHeader(lastRow, Function(row, column) IsBinaryCell38(arr(row, column)))
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
        Return Global.BESHStatNG.ExcelCommon.UdfRangePreparation38.HasNumericMatrixHeader(lastRow, arr.GetLength(1), Function(row, column) IsBlankCell(arr(row, column)), Function(row, column) TryGetDouble(arr(row, column)).HasValue)
    End Function

    Friend Function LooksLikeHeaderRow(arr As Object(,), numericCols As Integer()) As Boolean
        Return Global.BESHStatNG.ExcelCommon.UdfRangePreparation38.LooksLikeHeaderRow(arr.GetLength(0), numericCols, Function(row, column) TryGetDouble(arr(row, column)).HasValue)
    End Function

    Friend Function LooksLikeSingleColumnHeader(arr As Object(,)) As Boolean
        If arr Is Nothing Then Return False
        If arr.GetLength(1) <> 1 Then Return False
        Return Global.BESHStatNG.ExcelCommon.UdfRangePreparation38.LooksLikeSingleColumnHeader(arr.GetLength(0), Function(row, column) TryGetDouble(arr(row, column)).HasValue)
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

        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryReadIndependentNumericColumnsPrepared(
            ax.GetLength(0), ay.GetLength(0), startRowX, startRowY, hasHeaderX, hasHeaderY,
            Function(row) TryGetDouble(ax(row, 0)), Function(row) TryGetDouble(ay(row, 0)),
            Function() Convert.ToString(ax(0, 0)).Trim(), Function() Convert.ToString(ay(0, 0)).Trim(), groups, names)
    End Function

    Friend Function TryReadPairedNumericColumns(x As Object, y As Object, ByRef mat As Double(,), ByRef names() As String) As Boolean
        mat = Nothing
        names = Nothing
        If Not WindowsSpreadsheetTransport.PairedArgumentsAligned(x, y) Then Return False
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

        mat = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.RowsToMatrix(pairs, 2)
        Return True
    End Function

    ''' <summary>
    ''' Extracts complete numeric pairs from two aligned one-column object matrices.
    ''' </summary>
    ''' <param name="ax">First aligned one-column matrix.</param>
    ''' <param name="ay">Second aligned one-column matrix.</param>
    ''' <returns>A list containing one two-element array per row where both cells are numeric.</returns>
    Friend Function ExtractCompletePairedNumericRows(ax As Object(,), ay As Object(,)) As List(Of Double())
        Return WindowsSpreadsheetTransport.ExtractNumericPairs(ax, ay)
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

    Friend Function ExtractNumericColumnIgnoringNonNumeric(arg As Object, ByRef err As ExcelError?) As Double()
        err = Nothing

        Dim arr As Object(,) = CoerceToObjectMatrix(arg, err)
        If err.HasValue OrElse arr Is Nothing Then Return Array.Empty(Of Double)()

        If arr.GetLength(1) <> 1 Then
            err = ExcelError.ExcelErrorValue
            Return Array.Empty(Of Double)()
        End If

        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.ReadFiniteColumn(
            arr.GetLength(0), 0, Function(row) TryGetDouble(arr(row, 0)), initialCapacity:=arr.GetLength(0))
    End Function

    Friend Function ExtractPairedNumericColumnsIgnoringNonNumeric(x As Object, y As Object, ByRef err As ExcelError?) As Double(,)
        err = Nothing

        If Not WindowsSpreadsheetTransport.PairedArgumentsAligned(x, y) Then
            err = ExcelError.ExcelErrorValue
            Return Nothing
        End If

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

        Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.RowsToMatrix(pairs, 2)
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
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.ExtractCompleteNumericMatrixCompleteCasesPrepared(
            r, c, Function(row, column) TryGetDouble(arr(row, column)), mat, noRows, noCols)
    End Function

    Friend Function TryReadCompleteNumericMatrixWithHeaders(input As Object, ByRef mat As Double(,), ByRef names() As String) As Boolean
        mat = Nothing
        names = Nothing
        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False
        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows < 1 OrElse cols < 2 Then Return False
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryReadCompleteNumericMatrixWithHeadersPrepared(
            rows, cols, Function(row, column) TryGetDouble(arr(row, column)),
            Function(row, column) Convert.ToString(arr(row, column)).Trim(), mat, names)
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

End Module
