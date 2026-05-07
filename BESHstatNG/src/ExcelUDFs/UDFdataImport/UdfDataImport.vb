Option Explicit On
Option Strict On
Imports ExcelDna.Integration

' UDF-facing data import facade. Public worksheet UDFs should call this module first, then pass the
' resulting DataObj-derived objects to the analysis/model layer. Batch 2 starts routing existing UDF
' call sites through this boundary while preserving existing worksheet signatures and behavior.
Partial Friend Module UdfDataImport

    ''' <summary>
    ''' Imports a generic numeric worksheet block into a DataObj, using the shared DataObj cleaning path.
    ''' </summary>
    Friend Function TryGetNumericData(input As Object,
                                      varNames As Object,
                                      allowMissing As Boolean,
                                      ByRef data As DataObj) As Boolean
        Return UDFhelpers.TryBuildNumericDataObject(input, varNames, allowMissing, data)
    End Function

    ''' <summary>
    ''' Imports a single numeric sample column for scalar diagnostics such as normality and outlier tests.
    ''' Non-numeric cells are ignored, and a first non-numeric cell is treated as a header when numeric values follow.
    ''' </summary>
    Friend Function TryGetSingleNumericColumn(input As Object,
                                              ByRef values() As Double,
                                              ByRef detectedName As String) As Boolean
        values = Nothing
        detectedName = String.Empty

        Dim arr As Object(,) = UDFhelpers.Get2D(input)
        If arr Is Nothing Then Return False
        If arr.GetLength(1) <> 1 Then Return False

        Dim hasHeader As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(arr)
        If hasHeader Then detectedName = Convert.ToString(arr(0, 0)).Trim()

        Dim startRow As Integer = If(hasHeader, 1, 0)
        Dim list As New List(Of Double)()
        For r As Integer = startRow To arr.GetLength(0) - 1
            Dim d As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(r, 0))
            If d.HasValue AndAlso Not Double.IsNaN(d.Value) AndAlso Not Double.IsInfinity(d.Value) Then list.Add(d.Value)
        Next

        values = list.ToArray()
        Return True
    End Function

    ''' <summary>
    ''' Imports independent grouped data from a wide range where each column is a group.
    ''' This preserves the existing UDF semantics: numeric values are collected column-wise and non-numeric cells are ignored.
    ''' </summary>
    Friend Function TryGetGroupedNumericColumns(input As Object,
                                                ByRef groups()() As Double,
                                                ByRef names() As String) As Boolean
        Return UDFhelpers.TryReadGroupedNumericColumns(input, groups, names)
    End Function

    ''' <summary>
    ''' Imports two independent one-column samples. Each column is filtered independently, so unequal group sizes are allowed.
    ''' </summary>
    Friend Function TryGetIndependentNumericColumns(x As Object,
                                                    y As Object,
                                                    ByRef groups()() As Double,
                                                    ByRef names() As String) As Boolean
        Return UDFhelpers.TryReadIndependentNumericColumns(x, y, groups, names)
    End Function

    ''' <summary>
    ''' Imports two aligned one-column samples for paired tests, retaining only complete numeric pairs.
    ''' </summary>
    Friend Function TryGetPairedNumericColumns(x As Object,
                                               y As Object,
                                               ByRef mat As Double(,),
                                               ByRef names() As String) As Boolean
        Return UDFhelpers.TryReadPairedNumericColumns(x, y, mat, names)
    End Function

    ''' <summary>
    ''' Imports a complete numeric matrix and optional header row for repeated-measures and multivariate UDFs.
    ''' </summary>
    Friend Function TryGetCompleteNumericMatrixWithHeaders(input As Object,
                                                           ByRef mat As Double(,),
                                                           ByRef names() As String) As Boolean
        Return UDFhelpers.TryReadCompleteNumericMatrixWithHeaders(input, mat, names)
    End Function

    ''' <summary>
    ''' Imports a single numeric column, preserving row alignment by returning Double.NaN for invalid/non-numeric cells.
    ''' </summary>
    Friend Function TryGetNumericColumn(input As Object, ByRef values As List(Of Double)) As Boolean
        Return UDFhelpers.TryReadNumericColumn(input, values)
    End Function

    ''' <summary>
    ''' Imports a numeric matrix with no header handling.
    ''' </summary>
    Friend Function TryGetNumericMatrix(input As Object,
                                        ByRef mat As Double(,),
                                        ByRef rows As Integer,
                                        ByRef cols As Integer) As Boolean
        Return UDFhelpers.TryReadNumericMatrix(input, mat, rows, cols)
    End Function

    ''' <summary>
    ''' Imports a single-column sample for legacy nonparametric UDFs, ignoring non-numeric cells.
    ''' </summary>
    Friend Function GetNumericColumnIgnoringNonNumeric(input As Object,
                                                       ByRef err As ExcelError?) As Double()
        Return UDFhelpers.ExtractNumericColumnIgnoringNonNumeric(input, err)
    End Function

    ''' <summary>
    ''' Imports two paired single-column samples for legacy nonparametric UDFs, retaining complete numeric pairs only.
    ''' </summary>
    Friend Function GetPairedNumericColumnsIgnoringNonNumeric(x As Object,
                                                              y As Object,
                                                              ByRef err As ExcelError?) As Double(,)
        Return UDFhelpers.ExtractPairedNumericColumnsIgnoringNonNumeric(x, y, err)
    End Function

    ''' <summary>
    ''' Imports grouped wide-column samples for legacy nonparametric UDFs, ignoring non-numeric cells column-wise.
    ''' </summary>
    Friend Function GetNumericGroupsFromColumnsIgnoringNonNumeric(input As Object,
                                                                  ByRef err As ExcelError?) As Double()()
        Return UDFhelpers.ExtractNumericGroupsFromColumnsIgnoringNonNumeric(input, err)
    End Function

    ''' <summary>
    ''' Imports a repeated-measures matrix for legacy nonparametric UDFs, keeping complete numeric rows only.
    ''' </summary>
    Friend Function TryGetCompleteNumericMatrixCompleteCases(input As Object,
                                                             ByRef mat As Double(,),
                                                             ByRef noRows As Integer,
                                                             ByRef noCols As Integer) As Boolean
        Return UDFhelpers.ExtractCompleteNumericMatrixCompleteCases(input, mat, noRows, noCols)
    End Function

    ''' <summary>
    ''' Imports a scalar, single-row, or single-column numeric vector. A leading text header is allowed and skipped.
    ''' Unlike TryGetNumericColumn, this is strict: interior blanks and non-numeric values fail the import.
    ''' </summary>
    Friend Function TryGetNumericVector(input As Object, ByRef values() As Double) As Boolean
        values = Nothing

        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(input) Then Return False

        Dim scalar As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(input)
        If scalar.HasValue Then
            ReDim values(0)
            values(0) = scalar.Value
            Return True
        End If

        Dim arr As Object(,) = UDFhelpers.Get2D(input)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows < 1 OrElse cols < 1 Then Return False
        If rows > 1 AndAlso cols > 1 Then Return False

        If rows = 1 Then
            Return TryReadStrictNumericVectorFromSingleRow(arr, values)
        End If
        Return TryReadStrictNumericVectorFromSingleColumn(arr, values)
    End Function

    ''' <summary>
    ''' Imports an optional numeric vector. Missing or empty arguments are accepted and return Nothing.
    ''' </summary>
    Friend Function TryGetOptionalNumericVector(input As Object, ByRef values() As Double) As Boolean
        values = Nothing
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(input) Then Return True
        Return TryGetNumericVector(input, values)
    End Function

    Private Function TryReadStrictNumericVectorFromSingleColumn(arr As Object(,), ByRef values() As Double) As Boolean
        values = Nothing
        Dim rows As Integer = arr.GetLength(0)
        If rows < 1 Then Return False

        Dim last As Integer = rows - 1
        While last >= 0 AndAlso Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(last, 0))
            last -= 1
        End While
        If last < 0 Then Return False

        Dim start As Integer = 0
        If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(0, 0)).HasValue Then
            If last = 0 Then Return False
            start = 1
        End If
        If start > last Then Return False

        Dim out As New List(Of Double)()
        For i As Integer = start To last
            If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(i, 0)) Then Return False
            Dim d As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(i, 0))
            If Not d.HasValue Then Return False
            out.Add(d.Value)
        Next

        If out.Count = 0 Then Return False
        values = out.ToArray()
        Return True
    End Function

    Private Function TryReadStrictNumericVectorFromSingleRow(arr As Object(,), ByRef values() As Double) As Boolean
        values = Nothing
        Dim cols As Integer = arr.GetLength(1)
        If cols < 1 Then Return False

        Dim last As Integer = cols - 1
        While last >= 0 AndAlso Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(0, last))
            last -= 1
        End While
        If last < 0 Then Return False

        Dim start As Integer = 0
        If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(0, 0)).HasValue Then
            If last = 0 Then Return False
            start = 1
        End If
        If start > last Then Return False

        Dim out As New List(Of Double)()
        For j As Integer = start To last
            If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(0, j)) Then Return False
            Dim d As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(0, j))
            If Not d.HasValue Then Return False
            out.Add(d.Value)
        Next

        If out.Count = 0 Then Return False
        values = out.ToArray()
        Return True
    End Function

    ''' <summary>
    ''' Imports a data matrix and aligned group labels, retaining rows with a nonblank label and complete numeric data.
    ''' Used by Box's M and future grouped multivariate UDFs.
    ''' </summary>
    Friend Function TryGetGroupedCompleteMatrix(data As Object,
                                                groups As Object,
                                                ByRef groupMatrices As List(Of Double(,)),
                                                ByRef groupNames As List(Of String)) As Boolean
        groupMatrices = Nothing
        groupNames = Nothing

        Dim dataArr As Object(,) = UDFhelpers.Get2D(data)
        Dim groupArr As Object(,) = UDFhelpers.Get2D(groups)
        If dataArr Is Nothing OrElse groupArr Is Nothing Then Return False
        If groupArr.GetLength(1) <> 1 Then Return False

        Dim dataRows As Integer = dataArr.GetLength(0)
        Dim dataCols As Integer = dataArr.GetLength(1)
        If dataRows < 1 OrElse dataCols < 1 Then Return False

        Dim dataColIndexes(dataCols - 1) As Integer
        For c As Integer = 0 To dataCols - 1
            dataColIndexes(c) = c
        Next

        Dim dataHasHeader As Boolean = UDFhelpers.LooksLikeHeaderRow(dataArr, dataColIndexes)
        Dim startData As Integer = If(dataHasHeader, 1, 0)

        Dim usableRows As Integer = dataRows - startData
        Dim groupRows As Integer = groupArr.GetLength(0)

        Dim startGroup As Integer
        If groupRows = usableRows Then
            startGroup = 0
        ElseIf groupRows = usableRows + 1 Then
            startGroup = 1
        Else
            Return False
        End If

        Dim buckets As New Dictionary(Of String, List(Of Double()))(StringComparer.OrdinalIgnoreCase)
        Dim order As New List(Of String)()

        For i As Integer = 0 To usableRows - 1
            Dim label As String = Convert.ToString(groupArr(startGroup + i, 0)).Trim()
            If String.IsNullOrWhiteSpace(label) Then Continue For

            Dim row(dataCols - 1) As Double
            Dim ok As Boolean = True
            For c As Integer = 0 To dataCols - 1
                Dim d As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(dataArr(startData + i, c))
                If Not d.HasValue OrElse Double.IsNaN(d.Value) OrElse Double.IsInfinity(d.Value) Then
                    ok = False
                    Exit For
                End If
                row(c) = d.Value
            Next
            If Not ok Then Continue For

            If Not buckets.ContainsKey(label) Then
                buckets(label) = New List(Of Double())()
                order.Add(label)
            End If
            buckets(label).Add(row)
        Next

        groupMatrices = New List(Of Double(,))()
        groupNames = New List(Of String)(order)
        For Each label As String In order
            groupMatrices.Add(RowsToMatrix(buckets(label), dataCols))
        Next

        Return True
    End Function

    ''' <summary>
    ''' Imports a three-column nested-ANOVA block: group, subgroup, response. A leading header row is allowed.
    ''' Rows with blank labels or nonnumeric responses are skipped.
    ''' </summary>
    Friend Function TryGetNestedThreeColumnData(input As Object,
                                                ByRef data(,) As Object,
                                                ByRef names() As String) As Boolean
        data = Nothing
        names = Nothing

        Dim arr As Object(,) = UDFhelpers.Get2D(input)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If cols <> 3 OrElse rows < 1 Then Return False

        Dim hasHeader As Boolean = False
        If rows >= 2 Then
            Dim firstRespNumeric As Boolean = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(0, 2)).HasValue
            Dim belowRespNumeric As Boolean = False
            For r As Integer = 1 To rows - 1
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(r, 2)).HasValue Then
                    belowRespNumeric = True
                    Exit For
                End If
            Next
            hasHeader = (Not firstRespNumeric) AndAlso belowRespNumeric
        End If
        Dim startRow As Integer = If(hasHeader, 1, 0)

        names = New String() {
            If(hasHeader, Convert.ToString(arr(0, 0)).Trim(), "Group"),
            If(hasHeader, Convert.ToString(arr(0, 1)).Trim(), "Subgroup"),
            If(hasHeader, Convert.ToString(arr(0, 2)).Trim(), "Response")
        }

        Dim rowsOut As New List(Of Object())()
        For r As Integer = startRow To rows - 1
            Dim g As String = Convert.ToString(arr(r, 0)).Trim()
            Dim sg As String = Convert.ToString(arr(r, 1)).Trim()
            Dim y As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(r, 2))
            If g <> "" AndAlso sg <> "" AndAlso y.HasValue Then
                rowsOut.Add(New Object() {g, sg, y.Value})
            End If
        Next

        If rowsOut.Count < 1 Then Return False
        data = New Object(rowsOut.Count - 1, 2) {}
        For r As Integer = 0 To rowsOut.Count - 1
            data(r, 0) = rowsOut(r)(0)
            data(r, 1) = rowsOut(r)(1)
            data(r, 2) = rowsOut(r)(2)
        Next
        Return True
    End Function

    Private Function RowsToMatrix(rows As List(Of Double()), cols As Integer) As Double(,)
        Dim out(rows.Count - 1, cols - 1) As Double
        For r As Integer = 0 To rows.Count - 1
            For c As Integer = 0 To cols - 1
                out(r, c) = rows(r)(c)
            Next
        Next
        Return out
    End Function

    ''' <summary>
    ''' Imports response, predictor, optional offset, and optional weight inputs into a GLM-ready data object.
    ''' </summary>
    Friend Function TryGetGlmData(y As Object,
                                  x As Object,
                                  varNames As Object,
                                  offset As Object,
                                  weights As Object,
                                  ByRef data As glmData) As Boolean
        Return UDFhelpers.TryBuildGlmDataFromUdfArgs(y, x, varNames, offset, weights, data)
    End Function

    ''' <summary>
    ''' Imports prediction-time predictors and optional offset into a GLM-compatible data object.
    ''' </summary>
    Friend Function TryGetPredictorData(x As Object,
                                        expectedPredictorNames As String(),
                                        offset As Object,
                                        requireOffset As Boolean,
                                        ByRef data As glmData) As Boolean
        Return UDFhelpers.TryBuildPredictorDataFromUdfArgs(x, expectedPredictorNames, offset, requireOffset, data)
    End Function

    ''' <summary>
    ''' Imports response, predictors, cluster id, and optional GEE time/order, offset, and weight inputs.
    ''' </summary>
    Friend Function TryGetGeeData(y As Object,
                                  x As Object,
                                  clusterId As Object,
                                  time As Object,
                                  varNames As Object,
                                  offset As Object,
                                  weights As Object,
                                  ByRef data As geeData) As Boolean
        Return UDFhelpers.TryBuildGeeDataFromUdfArgs(y, x, clusterId, time, varNames, offset, weights, data)
    End Function

    ''' <summary>
    ''' Imports Cox PH time, status, predictors, and optional strata inputs into a CoxPHData object.
    ''' </summary>
    Friend Function TryGetCoxData(time As Object,
                                  status As Object,
                                  x As Object,
                                  varNames As Object,
                                  strata As Object,
                                  ByRef data As CoxPHData) As Boolean
        Return UDFhelpers.TryBuildCoxDataFromUdfArgs(time, status, x, varNames, strata, data)
    End Function

    ''' <summary>
    ''' Imports and jointly row-aligns Zero-Inflated Poisson count and zero-component UDF inputs.
    ''' </summary>
    Friend Function TryGetZipData(y As Object,
                                  xCount As Object,
                                  xZero As Object,
                                  countVarNames As Object,
                                  zeroVarNames As Object,
                                  offset As Object,
                                  ByRef countData As glmData,
                                  ByRef zeroData As glmData) As Boolean
        countData = Nothing
        zeroData = Nothing

        Dim yCol(,) As Object = Nothing
        Dim xCountMat(,) As Object = Nothing
        Dim xZeroMat(,) As Object = Nothing
        Dim offsetCol(,) As Object = Nothing

        Dim yName As String = Nothing
        Dim offsetName As String = Nothing
        Dim inferredCountNames() As String = Nothing
        Dim inferredZeroNames() As String = Nothing

        If Not UDFhelpers.TryGetTrimmedColumnObject(y, yCol, yName, "numeric") Then Return False
        If Not UDFhelpers.TryGetTrimmedNumericMatrixObject(xCount, xCountMat, inferredCountNames) Then Return False
        If Not UDFhelpers.TryGetTrimmedNumericMatrixObject(xZero, xZeroMat, inferredZeroNames) Then Return False

        Dim rowCount As Integer = yCol.GetLength(0)
        If xCountMat.GetLength(0) <> rowCount Then Return False
        If xZeroMat.GetLength(0) <> rowCount Then Return False

        Dim hasOffset As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(offset)
        If hasOffset Then
            If Not UDFhelpers.TryGetTrimmedColumnObject(offset, offsetCol, offsetName, "numeric") Then Return False
            If offsetCol.GetLength(0) <> rowCount Then Return False
        End If

        Dim countPredictorNames As String() = UDFhelpers.ResolveImportedPredictorNames(countVarNames, inferredCountNames)
        Dim zeroPredictorNames As String() = UDFhelpers.ResolveImportedPredictorNames(zeroVarNames, inferredZeroNames)

        Dim countCols As Integer = xCountMat.GetLength(1)
        Dim zeroCols As Integer = xZeroMat.GetLength(1)

        Dim rawCount(rowCount - 1, countCols + If(hasOffset, 1, 0)) As Object
        Dim countNames(countCols + If(hasOffset, 1, 0)) As String
        countNames(0) = If(String.IsNullOrWhiteSpace(yName), "Y", yName)

        For i As Integer = 0 To rowCount - 1
            rawCount(i, 0) = yCol(i, 0)
        Next
        For j As Integer = 0 To countCols - 1
            countNames(j + 1) = countPredictorNames(j)
            For i As Integer = 0 To rowCount - 1
                rawCount(i, j + 1) = xCountMat(i, j)
            Next
        Next
        If hasOffset Then
            countNames(countCols + 1) = If(String.IsNullOrWhiteSpace(offsetName), "Offset", offsetName)
            For i As Integer = 0 To rowCount - 1
                rawCount(i, countCols + 1) = offsetCol(i, 0)
            Next
        End If

        Dim rawZero(rowCount - 1, zeroCols) As Object
        Dim zeroNames(zeroCols) As String
        zeroNames(0) = If(String.IsNullOrWhiteSpace(yName), "Y", yName)

        For i As Integer = 0 To rowCount - 1
            rawZero(i, 0) = yCol(i, 0)
        Next
        For j As Integer = 0 To zeroCols - 1
            zeroNames(j + 1) = zeroPredictorNames(j)
            For i As Integer = 0 To rowCount - 1
                rawZero(i, j + 1) = xZeroMat(i, j)
            Next
        Next

        Dim countOut As New glmData With {.bOffset = hasOffset, .bWeights = False}
        countOut.DataImportRawMatrix(rawCount, countNames)
        If countOut.bZeroValid OrElse countOut.nRows < 1 Then Return False

        Dim zeroOut As New glmData With {.bOffset = False, .bWeights = False}
        zeroOut.DataImportRawMatrix(rawZero, zeroNames)
        If zeroOut.bZeroValid OrElse zeroOut.nRows < 1 Then Return False

        Dim keepCount As Dictionary(Of Integer, Integer) = CommonItems(countOut.RowIds, zeroOut.RowIds)
        Dim keepZero As Dictionary(Of Integer, Integer) = CommonItems(zeroOut.RowIds, countOut.RowIds)
        If keepCount Is Nothing OrElse keepZero Is Nothing Then Return False
        If keepCount.Count < 1 OrElse keepZero.Count < 1 Then Return False
        If keepCount.Count <> keepZero.Count Then Return False

        countOut.SubsetByRowIdValues(keepCount)
        zeroOut.SubsetByRowIdValues(keepZero)
        If countOut.nRows <> zeroOut.nRows Then Return False
        If countOut.nRows < 1 Then Return False

        If Not ResponseColumnsMatchObjects(countOut.FinalData, zeroOut.FinalData) Then Return False
        Dim response() As Integer = Nothing
        If Not TryExtractNonnegativeIntegerResponseObjects(countOut.FinalData, response) Then Return False
        If hasOffset AndAlso Not UDFhelpers.HasOnlyFinite(countOut.OffsetData) Then Return False

        countData = countOut
        zeroData = zeroOut
        Return True
    End Function

    Private Function ResponseColumnsMatchObjects(countData(,) As Object, zeroData(,) As Object) As Boolean
        If countData Is Nothing OrElse zeroData Is Nothing Then Return False
        If countData.GetLength(0) <> zeroData.GetLength(0) Then Return False

        For i As Integer = 0 To countData.GetLength(0) - 1
            Dim yc As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(countData(i, 0))
            Dim yz As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(zeroData(i, 0))
            If Not yc.HasValue OrElse Not yz.HasValue Then Return False
            If Math.Abs(yc.Value - yz.Value) > 0.0000001R Then Return False
        Next

        Return True
    End Function

    Private Function TryExtractNonnegativeIntegerResponseObjects(data(,) As Object, ByRef response() As Integer) As Boolean
        response = Nothing
        If data Is Nothing Then Return False
        Dim n As Integer = data.GetLength(0)
        If n < 1 Then Return False

        ReDim response(n - 1)
        For i As Integer = 0 To n - 1
            Dim yi As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(data(i, 0))
            If Not yi.HasValue Then Return False
            Dim yr As Double = Math.Round(yi.Value)
            If yi.Value < 0.0R OrElse Math.Abs(yi.Value - yr) > 0.0000001R Then Return False
            response(i) = CInt(yr)
        Next

        Return True
    End Function

End Module