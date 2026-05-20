Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.CausalInference
Imports BESHStatNG.WorksheetFunctions
Imports ExcelDna.Integration

' PSM-specific UDF import facade.  The worksheet UDF layer should call this routine and then pass
' the resulting psmData.Input directly to the PSM backend.  The implementation deliberately reuses the
' existing UdfDataImport range readers and the DataObj-based psmData importer so that GUI and UDF paths
' share row cleaning, row alignment, formula expansion, and source-row tracking.
Partial Friend Module UdfDataImport

    Friend Function TryGetPsmData(id As Object,
                                  treatment As Object,
                                  outcome As Object,
                                  covariates As Object,
                                  varNames As Object,
                                  scoreMethod As PsmScoreMethod,
                                  suppliedScore As Object,
                                  exactGroups As Object,
                                  formula As Object,
                                  formulaAddressing As Object,
                                  ByRef data As psmData) As Boolean
        data = Nothing

        Dim treatmentCol(,) As Object = Nothing
        Dim treatmentName As String = Nothing
        If Not TryGetTrimmedColumnObject(treatment, treatmentCol, treatmentName, "binary") Then Return False

        Dim outcomeCol(,) As Object = Nothing
        Dim outcomeName As String = Nothing
        If Not TryGetTrimmedColumnObject(outcome, outcomeCol, outcomeName, "numeric") Then Return False

        Dim covariateMatrix(,) As Object = Nothing
        Dim inferredCovariateNames() As String = Nothing
        If Not TryGetTrimmedNumericMatrixObject(covariates, covariateMatrix, inferredCovariateNames) Then Return False

        Dim n As Integer = covariateMatrix.GetLength(0)
        If treatmentCol.GetLength(0) <> n OrElse outcomeCol.GetLength(0) <> n Then Return False

        Dim p As Integer = covariateMatrix.GetLength(1)
        Dim covariateNames() As String = ResolveImportedPredictorNames(varNames, inferredCovariateNames)
        If covariateNames Is Nothing OrElse covariateNames.Length <> p Then Return False

        Dim treatmentKey As String = If(String.IsNullOrWhiteSpace(treatmentName), "Treatment", treatmentName.Trim())
        Dim outcomeKey As String = If(String.IsNullOrWhiteSpace(outcomeName), "Outcome", outcomeName.Trim())
        Dim modelNames(p) As String
        modelNames(0) = treatmentKey
        For j As Integer = 0 To p - 1
            modelNames(j + 1) = covariateNames(j)
        Next

        Dim modelInput(n - 1, p) As Object
        For i As Integer = 0 To n - 1
            modelInput(i, 0) = treatmentCol(i, 0)
            For j As Integer = 0 To p - 1
                modelInput(i, j + 1) = covariateMatrix(i, j)
            Next
        Next

        Dim scoreInput(,) As Object = Nothing
        Dim scoreNames() As String = Nothing
        If scoreMethod = PsmScoreMethod.Supplied OrElse Not ExcelArgPredicates.IsMissingArg(suppliedScore) Then
            Dim scoreCol(,) As Object = Nothing
            Dim scoreName As String = Nothing
            If Not TryGetTrimmedColumnObject(suppliedScore, scoreCol, scoreName, "numeric") Then Return False
            If scoreCol.GetLength(0) <> n Then Return False
            scoreInput = scoreCol
            scoreNames = New String() {If(String.IsNullOrWhiteSpace(scoreName), "PropensityScore", scoreName.Trim())}
        End If

        Dim idInput(,) As Object = Nothing
        Dim idNames() As String = Nothing
        If Not ExcelArgPredicates.IsMissingArg(id) Then
            Dim idCol(,) As Object = Nothing
            Dim idName As String = Nothing
            If Not TryGetTrimmedColumnObject(id, idCol, idName, "text") Then Return False
            If idCol.GetLength(0) <> n Then Return False
            idInput = idCol
            idNames = New String() {If(String.IsNullOrWhiteSpace(idName), "ID", idName.Trim())}
        End If

        Dim exactInput(,) As Object = Nothing
        Dim exactNames() As String = Nothing
        If Not ExcelArgPredicates.IsMissingArg(exactGroups) Then
            If Not TryGetTrimmedObjectMatrixAligned(exactGroups, n, "Exact", exactInput, exactNames) Then Return False
        End If

        Dim selectedCovariates As New List(Of String)(covariateNames)
        Dim formulaText As String = ExcelArgReaders.AsString(formula)
        Dim addressing As String = GetFormulaAddressingMode(formulaAddressing, "relative")
        Dim allowRelative As Boolean = Not String.Equals(addressing, "absolute", StringComparison.OrdinalIgnoreCase)
        Dim allowAbsolute As Boolean = String.Equals(addressing, "absolute", StringComparison.OrdinalIgnoreCase)
        Dim allowNames As Boolean = True
        Dim absoluteLetters() As String = Nothing
        If allowAbsolute AndAlso Not String.IsNullOrWhiteSpace(formulaText) Then
            If Not TryGetAbsoluteColumnLetters(covariates, p, absoluteLetters) Then Return False
        End If

        Dim spec As New PsmDataRawMatrixSpec With {
            .ModelRawInput = modelInput,
            .ModelVariableNames = modelNames,
            .TreatmentKey = treatmentKey,
            .OutcomeRawInput = outcomeCol,
            .OutcomeVariableNames = New String() {outcomeKey},
            .SelectedCovariateKeys = selectedCovariates,
            .ScoreMethod = scoreMethod,
            .SuppliedScoreRawInput = scoreInput,
            .SuppliedScoreVariableNames = scoreNames,
            .IdRawInput = idInput,
            .IdVariableNames = idNames,
            .ExactGroupRawInput = exactInput,
            .ExactGroupVariableNames = exactNames,
            .FormulaText = formulaText,
            .FormulaAddressing = addressing,
            .AbsoluteColumnLetters = absoluteLetters,
            .AllowRelativeColumnLetters = allowRelative,
            .AllowAbsoluteColumnLetters = allowAbsolute,
            .AllowQuotedVariableNames = allowNames,
            .FirstSourceRow = GetFirstSourceRow(covariates)
        }

        Dim imported As New psmData()
        imported.DataImportFromRawMatrices(spec)
        If imported.bZeroValid OrElse imported.Input Is Nothing OrElse imported.Input.RowCount < 1 Then Return False

        data = imported
        Return True
    End Function

    Friend Function TryGetTrimmedObjectMatrixAligned(input As Object,
                                                     expectedRows As Integer,
                                                     defaultNamePrefix As String,
                                                     ByRef matrix(,) As Object,
                                                     ByRef names() As String) As Boolean
        matrix = Nothing
        names = Nothing
        If expectedRows < 1 Then Return False

        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim usedRows As Integer = lastRow + 1
        Dim startRow As Integer
        If usedRows = expectedRows Then
            startRow = 0
        ElseIf usedRows - 1 = expectedRows Then
            startRow = 1
        Else
            Return False
        End If

        Dim cols As Integer = arr.GetLength(1)
        If cols < 1 Then Return False

        ReDim names(cols - 1)
        For j As Integer = 0 To cols - 1
            If startRow = 1 AndAlso Not IsBlankCell(arr(0, j)) Then
                names(j) = Convert.ToString(arr(0, j)).Trim()
            End If
            If String.IsNullOrWhiteSpace(names(j)) Then names(j) = defaultNamePrefix & (j + 1).ToString(Global.System.Globalization.CultureInfo.InvariantCulture)
        Next

        ReDim matrix(expectedRows - 1, cols - 1)
        For i As Integer = 0 To expectedRows - 1
            For j As Integer = 0 To cols - 1
                matrix(i, j) = arr(startRow + i, j)
            Next
        Next

        Return True
    End Function

    Friend Function GetFirstSourceRow(input As Object) As Integer
        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return 1
        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return 1
        Return If(HasNumericMatrixHeader(arr, lastRow), 2, 1)
    End Function

End Module
