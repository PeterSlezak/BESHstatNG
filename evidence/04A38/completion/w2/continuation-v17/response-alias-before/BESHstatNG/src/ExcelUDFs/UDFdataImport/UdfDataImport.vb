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
        Return TryBuildNumericDataObject(input, varNames, allowMissing, data)
    End Function

    ''' <summary>
    ''' Returns True when every supplied value is finite; optionally requires strictly positive values.
    ''' A Nothing array is treated as valid to preserve optional offset/weight semantics.
    ''' </summary>
    Friend Function HasOnlyFinite(values() As Double, Optional requirePositive As Boolean = False) As Boolean
        Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.HasOnlyFinite(values, requirePositive)
    End Function

    ''' <summary>
    ''' Parses an optional Cox ties-method argument.
    ''' </summary>
    Friend Function GetCoxTieMethod(arg As Object, defaultValue As TieMethod) As TieMethod
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportOptions38.GetCoxTieMethod(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), defaultValue)
    End Function

    ''' <summary>
    ''' Extracts an integer response column from the first column of an imported regression DataObj.
    ''' Values must be finite and exactly integer-valued after normal DataObj row screening.
    ''' </summary>
    Friend Function TryGetIntegerOutcomeColumn(data As DataObj, ByRef values As List(Of Integer)) As Boolean
        values = New List(Of Integer)()

        If data Is Nothing OrElse data.nRows < 1 OrElse data.nCols < 1 Then Return False

        Return Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.ReadIntegerOutcomes(data.FinalData, data.nRows, values, AddressOf Microsoft.VisualBasic.CompilerServices.Conversions.ToDouble)
    End Function

    ''' <summary>
    ''' Extracts a non-negative integer response vector from the first column of a numeric data matrix.
    ''' Used by count-model UDFs where the response must be a valid count.
    ''' </summary>
    Friend Function TryGetNonnegativeIntegerResponse(data(,) As Double, ByRef response() As Integer) As Boolean
        Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TryGetNonnegativeIntegerResponse(data, response)
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

        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False
        If arr.GetLength(1) <> 1 Then Return False

        Dim hasHeader As Boolean = LooksLikeSingleColumnHeader(arr)
        If hasHeader Then detectedName = Convert.ToString(arr(0, 0)).Trim()

        Dim startRow As Integer = If(hasHeader, 1, 0)
        values = Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.ReadFiniteColumn(
            arr.GetLength(0), startRow, Function(row) Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(row, 0)))
        Return True
    End Function

    ''' <summary>
    ''' Imports independent grouped data from a wide range where each column is a group.
    ''' This preserves the existing UDF semantics: numeric values are collected column-wise and non-numeric cells are ignored.
    ''' </summary>
    Friend Function TryGetGroupedNumericColumns(input As Object,
                                                ByRef groups()() As Double,
                                                ByRef names() As String) As Boolean
        Return TryReadGroupedNumericColumns(input, groups, names)
    End Function

    ''' <summary>
    ''' Imports two independent one-column samples. Each column is filtered independently, so unequal group sizes are allowed.
    ''' </summary>
    Friend Function TryGetIndependentNumericColumns(x As Object,
                                                    y As Object,
                                                    ByRef groups()() As Double,
                                                    ByRef names() As String) As Boolean
        Return TryReadIndependentNumericColumns(x, y, groups, names)
    End Function

    ''' <summary>
    ''' Imports two aligned one-column samples for paired tests, retaining only complete numeric pairs.
    ''' </summary>
    Friend Function TryGetPairedNumericColumns(x As Object,
                                               y As Object,
                                               ByRef mat As Double(,),
                                               ByRef names() As String) As Boolean
        Return TryReadPairedNumericColumns(x, y, mat, names)
    End Function

    ''' <summary>
    ''' Imports a complete numeric matrix and optional header row for repeated-measures and multivariate UDFs.
    ''' </summary>
    Friend Function TryGetCompleteNumericMatrixWithHeaders(input As Object,
                                                           ByRef mat As Double(,),
                                                           ByRef names() As String) As Boolean
        Return TryReadCompleteNumericMatrixWithHeaders(input, mat, names)
    End Function

    ''' <summary>
    ''' Imports a single numeric column, preserving row alignment by returning Double.NaN for invalid/non-numeric cells.
    ''' </summary>
    Friend Function TryGetNumericColumn(input As Object, ByRef values As List(Of Double)) As Boolean
        Return TryReadNumericColumn(input, values)
    End Function

    ''' <summary>
    ''' Imports a required offset-only prediction column for count-model prediction paths.
    ''' </summary>
    Friend Function TryGetRequiredOffsetPredictionInputs(newOffset As Object,
                                                         ByRef offsetVals() As Double,
                                                         ByRef nRows As Integer) As Boolean
        offsetVals = Nothing
        nRows = 0

        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(newOffset) Then Return False

        Dim values As List(Of Double) = Nothing
        If Not TryGetNumericColumn(newOffset, values) Then Return False
        If values Is Nothing OrElse values.Count < 1 Then Return False

        offsetVals = values.ToArray()
        If Not UdfDataImport.HasOnlyFinite(offsetVals) Then Return False

        nRows = offsetVals.Length
        Return True
    End Function

    ''' <summary>
    ''' Imports a one-dimensional numeric vector from comma/semicolon/space/tab-delimited text or a one-row / one-column range.
    ''' This preserves legacy GEE start-parameter parsing semantics.
    ''' </summary>
    Friend Function TryGetLooseNumericVector(input As Object, ByRef values() As Double) As Boolean
        values = Nothing

        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(input) Then Return False

        Dim s As String = TryCast(input, String)
        If s IsNot Nothing Then
            Return Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.TryGetLooseNumericTextPrepared(s, values)
        End If

        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows < 1 OrElse cols < 1 Then Return False
        If rows <> 1 AndAlso cols <> 1 Then Return False

        Dim count As Integer = rows * cols
        ReDim values(count - 1)
        Dim k As Integer = 0
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                Dim d As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(i, j))
                If Not d.HasValue Then
                    values = Nothing
                    Return False
                End If
                values(k) = d.Value
                k += 1
            Next
        Next

        Return True
    End Function

    ''' <summary>
    ''' Imports offset values for intercept-only prediction UDF paths.
    ''' When an offset is required, the argument must be supplied as a finite numeric column.
    ''' When no offset is supplied, one prediction row is implied.
    ''' </summary>
    Friend Function TryGetInterceptOnlyPredictionInputs(newOffset As Object,
                                                        requireOffset As Boolean,
                                                        ByRef nRows As Integer,
                                                        ByRef offsetVals() As Double) As Boolean
        nRows = 0
        offsetVals = Nothing

        Dim hasOffsetArg As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(newOffset)
        If requireOffset AndAlso Not hasOffsetArg Then Return False

        If hasOffsetArg Then
            Dim values As List(Of Double) = Nothing
            If Not TryGetNumericColumn(newOffset, values) Then Return False
            If values Is Nothing OrElse values.Count < 1 Then Return False
            offsetVals = values.ToArray()
            If Not UdfDataImport.HasOnlyFinite(offsetVals) Then Return False
            nRows = offsetVals.Length
            Return True
        End If

        nRows = 1
        Return True
    End Function

    ''' <summary>
    ''' Imports a numeric matrix with no header handling.
    ''' </summary>
    Friend Function TryGetNumericMatrix(input As Object,
                                        ByRef mat As Double(,),
                                        ByRef rows As Integer,
                                        ByRef cols As Integer) As Boolean
        Return TryReadNumericMatrix(input, mat, rows, cols)
    End Function

    ''' <summary>
    ''' Imports a trimmed single-column text range and returns any inferred header/name.
    ''' This is used for aligned categorical/group inputs while keeping raw range parsing behind the import facade.
    ''' </summary>
    Friend Function TryGetTextColumn(input As Object,
                                     ByRef column(,) As Object,
                                     ByRef inferredName As String) As Boolean
        Return TryGetTrimmedColumnObject(input, column, inferredName, "text")
    End Function

    ''' <summary>
    ''' Imports a single-column sample for legacy nonparametric UDFs, ignoring non-numeric cells.
    ''' </summary>
    Friend Function GetNumericColumnIgnoringNonNumeric(input As Object, ByRef err As ExcelError?) As Double()
        Return ExtractNumericColumnIgnoringNonNumeric(input, err)
    End Function

    ''' <summary>
    ''' Imports two paired single-column samples for legacy nonparametric UDFs, retaining complete numeric pairs only.
    ''' </summary>
    Friend Function GetPairedNumericColumnsIgnoringNonNumeric(x As Object,
                                                              y As Object,
                                                              ByRef err As ExcelError?) As Double(,)
        Return ExtractPairedNumericColumnsIgnoringNonNumeric(x, y, err)
    End Function

    ''' <summary>
    ''' Imports grouped wide-column samples for legacy nonparametric UDFs, ignoring non-numeric cells column-wise.
    ''' </summary>
    Friend Function GetNumericGroupsFromColumnsIgnoringNonNumeric(input As Object,
                                                                  ByRef err As ExcelError?) As Double()()
        Return ExtractNumericGroupsFromColumnsIgnoringNonNumeric(input, err)
    End Function

    ''' <summary>
    ''' Imports a repeated-measures matrix for legacy nonparametric UDFs, keeping complete numeric rows only.
    ''' </summary>
    Friend Function TryGetCompleteNumericMatrixCompleteCases(input As Object,
                                                             ByRef mat As Double(,),
                                                             ByRef noRows As Integer,
                                                             ByRef noCols As Integer) As Boolean
        Return ExtractCompleteNumericMatrixCompleteCases(input, mat, noRows, noCols)
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

        Dim arr As Object(,) = Get2D(input)
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
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryStrictVector(
            arr.GetLength(0), Function(index) Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(index, 0)),
            Function(index) Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(index, 0)), values)
    End Function

    Private Function TryReadStrictNumericVectorFromSingleRow(arr As Object(,), ByRef values() As Double) As Boolean
        values = Nothing
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryStrictVector(
            arr.GetLength(1), Function(index) Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(0, index)),
            Function(index) Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(0, index)), values)
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

        Dim dataArr As Object(,) = Get2D(data)
        Dim groupArr As Object(,) = Get2D(groups)
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryGetGroupedCompleteMatrixPrepared(
            dataArr, groupArr, AddressOf LooksLikeHeaderRow, Function(cell) Convert.ToString(cell).Trim(),
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble, groupMatrices, groupNames)
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

        Dim arr As Object(,) = Get2D(input)
        Return Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.TryGetNestedThreeColumnDataPrepared(
            arr, Function(cell) Convert.ToString(cell).Trim(),
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble, data, names)
    End Function

    Private Function RowsToMatrix(rows As List(Of Double()), cols As Integer) As Double(,)
        Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.RowsToMatrix(rows, cols)
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
        Return TryBuildGlmDataFromUdfArgs(y, x, varNames, offset, weights, data)
    End Function

    ''' <summary>
    ''' Imports prediction-time predictors and optional offset into a GLM-compatible data object.
    ''' </summary>
    Friend Function TryGetPredictorData(x As Object,
                                        expectedPredictorNames As String(),
                                        offset As Object,
                                        requireOffset As Boolean,
                                        ByRef data As glmData) As Boolean
        Return TryBuildPredictorDataFromUdfArgs(x, expectedPredictorNames, offset, requireOffset, data)
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
        Return TryBuildGeeDataFromUdfArgs(y, x, clusterId, time, varNames, offset, weights, data)
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
        Return TryBuildCoxDataFromUdfArgs(time, status, x, varNames, strata, data)
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

        If Not TryGetTrimmedColumnObject(y, yCol, yName, "numeric") Then Return False
        If Not TryGetTrimmedNumericMatrixObject(xCount, xCountMat, inferredCountNames) Then Return False
        If Not TryGetTrimmedNumericMatrixObject(xZero, xZeroMat, inferredZeroNames) Then Return False

        Dim rowCount As Integer = yCol.GetLength(0)
        If xCountMat.GetLength(0) <> rowCount Then Return False
        If xZeroMat.GetLength(0) <> rowCount Then Return False

        Dim hasOffset As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(offset)
        If hasOffset Then
            If Not TryGetTrimmedColumnObject(offset, offsetCol, offsetName, "numeric") Then Return False
            If offsetCol.GetLength(0) <> rowCount Then Return False
        End If

        Dim countPredictorNames As String() = ResolveImportedPredictorNames(countVarNames, inferredCountNames)
        Dim zeroPredictorNames As String() = ResolveImportedPredictorNames(zeroVarNames, inferredZeroNames)

        Dim rawCount(,) As Object = Nothing
        Dim countNames() As String = Nothing
        Dim rawZero(,) As Object = Nothing
        Dim zeroNames() As String = Nothing
        Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.BuildZipRawMatrices(yCol, xCountMat, xZeroMat, offsetCol, rowCount, hasOffset, yName, offsetName, countPredictorNames, zeroPredictorNames, rawCount, countNames, rawZero, zeroNames)

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
        If hasOffset AndAlso Not UdfDataImport.HasOnlyFinite(countOut.OffsetData) Then Return False

        countData = countOut
        zeroData = zeroOut
        Return True
    End Function

    Private Function ResponseColumnsMatchObjects(countData(,) As Object, zeroData(,) As Object) As Boolean
        Return Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.ResponseColumnsMatchObjects(countData, zeroData, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble)
    End Function

    Private Function TryExtractNonnegativeIntegerResponseObjects(data(,) As Object, ByRef response() As Integer) As Boolean
        Return Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.TryExtractNonnegativeIntegerResponseObjects(data, response, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble)
    End Function

End Module