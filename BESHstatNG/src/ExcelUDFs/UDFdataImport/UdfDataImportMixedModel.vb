Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

' Mixed-model data import helpers for worksheet UDFs.
' Keeps MMRM-specific option/name parsing behind the shared UdfDataImport facade.
Partial Friend Module UdfDataImport

    ''' <summary>
    ''' Imports a one-dimensional MMRM predictor-name list from comma-separated text or a one-row / one-column worksheet range.
    ''' Range blanks are preserved so callers can apply positional fallback names.
    ''' </summary>
    Friend Function TryGetMmrmNameList(arg As Object,
                                       ByRef names() As String) As Boolean
        names = Nothing
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return False

        Dim s As String = TryCast(arg, String)
        If s IsNot Nothing Then
            Dim parts As String() = s.Split({","c}, StringSplitOptions.RemoveEmptyEntries).
                Select(Function(part) If(part, String.Empty).Trim()).
                ToArray()
            If parts.Length > 0 Then
                names = parts
                Return True
            End If
        End If

        Dim arr As Object(,) = Get2D(arg)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim list As New List(Of String)()

        If rows = 1 AndAlso cols >= 1 Then
            For j As Integer = 0 To cols - 1
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(0, j)) Then
                    list.Add(String.Empty)
                Else
                    list.Add(Convert.ToString(arr(0, j), CultureInfo.InvariantCulture).Trim())
                End If
            Next
        ElseIf cols = 1 AndAlso rows >= 1 Then
            For i As Integer = 0 To rows - 1
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(i, 0)) Then
                    list.Add(String.Empty)
                Else
                    list.Add(Convert.ToString(arr(i, 0), CultureInfo.InvariantCulture).Trim())
                End If
            Next
        Else
            Return False
        End If

        names = list.ToArray()
        Return names.Length > 0
    End Function

    ''' <summary>
    ''' Imports, formula-prunes, and row-aligns MMRM response, predictor, subject, and optional visit inputs.
    ''' This is the UDF-side equivalent of the GUI/DataObj import boundary for MMRM fitting.
    ''' </summary>
    Friend Function TryGetMmrmAlignedInputs(y As Object,
                                            x As Object,
                                            subject As Object,
                                            visit As Object,
                                            ByRef yValues() As Double,
                                            ByRef xValues(,) As Double,
                                            ByRef subjectValues() As Object,
                                            ByRef visitValues() As Double,
                                            ByRef inferredXNames() As String,
                                            ByRef inferredXAbsoluteLetters() As String,
                                            ByRef droppedRows As Integer,
                                            ByRef errorMessage As String,
                                            formulaText As String,
                                            formulaAddressingMode As String,
                                            varNames As Object) As Boolean
        yValues = Nothing
        xValues = Nothing
        subjectValues = Nothing
        visitValues = Nothing
        inferredXNames = Nothing
        inferredXAbsoluteLetters = Nothing
        droppedRows = 0
        errorMessage = Nothing

        Dim yCol(,) As Object = Nothing
        Dim yName As String = Nothing
        If Not TryGetTrimmedColumnObject(y, yCol, yName, "numeric") Then
            errorMessage = "y must be a non-empty single-column numeric range."
            Return False
        End If

        Dim xRaw(,) As Object = Nothing
        If Not TryGetTrimmedNumericMatrixObject(x, xRaw, inferredXNames) Then
            errorMessage = "x must be a non-empty matrix."
            Return False
        End If

        Dim xColumnIndices() As Integer = Nothing
        If Not TryResolveMmrmFormulaRequiredXColumns(x,
                                                     xRaw,
                                                     inferredXNames,
                                                     formulaText,
                                                     formulaAddressingMode,
                                                     xColumnIndices,
                                                     inferredXNames,
                                                     inferredXAbsoluteLetters,
                                                     errorMessage,
                                                     varNames:=varNames) Then
            Return False
        End If

        Dim subjectCol(,) As Object = Nothing
        Dim subjectName As String = Nothing
        If Not TryGetTrimmedColumnObject(subject, subjectCol, subjectName, "text") Then
            errorMessage = "subject must be a non-empty single-column range."
            Return False
        End If

        Dim visitCol(,) As Object = Nothing
        Dim visitSupplied As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(visit)
        If visitSupplied Then
            Dim visitName As String = Nothing
            If Not TryGetTrimmedColumnObject(visit, visitCol, visitName, "numeric") Then
                errorMessage = "visit must be a numeric single-column range when supplied."
                Return False
            End If
        End If

        Dim n As Integer = yCol.GetLength(0)
        If xRaw.GetLength(0) <> n OrElse subjectCol.GetLength(0) <> n OrElse (visitSupplied AndAlso visitCol.GetLength(0) <> n) Then
            errorMessage = "y, x, subject, and visit inputs must have the same number of data rows after optional header trimming."
            Return False
        End If

        Return Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.AlignMmrmRows(yCol, xRaw, subjectCol, visitCol, xColumnIndices, visitSupplied, n, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText, yValues, xValues, subjectValues, visitValues, droppedRows, errorMessage)
    End Function

    Private Function TryResolveMmrmFormulaRequiredXColumns(xArg As Object,
                                                           xRaw(,) As Object,
                                                           allInferredNames() As String,
                                                           formulaText As String,
                                                           formulaAddressingMode As String,
                                                           ByRef columnIndices() As Integer,
                                                           ByRef selectedNames() As String,
                                                           ByRef selectedAbsoluteLetters() As String,
                                                           ByRef errorMessage As String,
                                                           varNames As Object) As Boolean
        columnIndices = Nothing
        selectedNames = Nothing
        selectedAbsoluteLetters = Nothing
        errorMessage = Nothing

        If xRaw Is Nothing Then
            errorMessage = "x must be a non-empty matrix."
            Return False
        End If

        Dim pAll As Integer = xRaw.GetLength(1)
        If pAll < 1 Then
            errorMessage = "x must contain at least one predictor column."
            Return False
        End If

        Dim allNames() As String = If(allInferredNames, DefaultMmrmNames(pAll, "X"))
        If allNames.Length <> pAll Then allNames = DefaultMmrmNames(pAll, "X")

        Dim allAbsoluteLetters() As String = Nothing
        Dim hasAbsoluteLetters As Boolean = TryGetAbsoluteColumnLetters(xArg, pAll, allAbsoluteLetters)
        Dim allDisplayNames() As String = ResolveMmrmRawPredictorNames(varNames, allNames, pAll)

        Return Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.ResolveMmrmFormulaColumns(formulaText, formulaAddressingMode, pAll, allDisplayNames, allAbsoluteLetters, hasAbsoluteLetters, columnIndices, selectedNames, selectedAbsoluteLetters, errorMessage)
    End Function

    Private Function ResolveMmrmRawPredictorNames(varNames As Object,
                                                  inferredNames() As String,
                                                  expectedCount As Integer) As String()
        Dim inferred() As String = NormalizeMmrmNameList(inferredNames, expectedCount, "X")
        If expectedCount <= 0 Then Return New String() {}
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(varNames) Then Return inferred

        Dim suppliedNames() As String = Nothing
        If TryGetMmrmNameList(varNames, suppliedNames) AndAlso suppliedNames IsNot Nothing AndAlso suppliedNames.Length = expectedCount Then
            Return NormalizeMmrmNameList(suppliedNames, expectedCount, "X")
        End If

        Return inferred
    End Function

    Private Function NormalizeMmrmNameList(inputNames() As String, expectedCount As Integer, fallbackPrefix As String) As String()
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportMetadata38.NormalizeMmrmNameList(inputNames, expectedCount, fallbackPrefix)
    End Function

    Private Function DefaultMmrmNames(count As Integer, prefix As String) As String()
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportMetadata38.DefaultMmrmNames(count, prefix)
    End Function

    ''' <summary>
    ''' Imports an MMRM LS-mean/contrast estimate specification range.
    ''' </summary>
    Friend Function TryGetMmrmLsmEstimateSpec(spec As Object,
                                              h As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmHandle,
                                              ByRef components As List(Of Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateComponent),
                                              ByRef errorMessage As String) As Boolean
        components = New List(Of Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateComponent)()
        errorMessage = Nothing

        Dim arr As Object(,) = Get2D(spec)
        If arr Is Nothing Then
            errorMessage = "spec must be a worksheet range with a header row."
            Return False
        End If

        Return Global.BESHStatNG.ExcelCommon.UdfModelProfilePreparation38.TryGetMmrmSpec(Of Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateComponent, Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue)(
            arr, Function() h.FixedEffectNames, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell,
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble,
            components, errorMessage)
    End Function

    ''' <summary>
    ''' Imports an optional MMRM LS-mean/contrast AT-profile range.
    ''' </summary>
    Friend Function TryGetMmrmLsmEstimateAtSpec(at As Object,
                                                h As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmHandle,
                                                ByRef atProfile As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile,
                                                ByRef errorMessage As String) As Boolean
        atProfile = Nothing
        errorMessage = Nothing

        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(at) Then Return True

        Dim arr As Object(,) = Get2D(at)
        If arr Is Nothing Then
            errorMessage = "at must be blank or a worksheet range with a header row."
            Return False
        End If

        Return Global.BESHStatNG.ExcelCommon.UdfModelProfilePreparation38.TryGetMmrmAtSpec(Of Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile, Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue)(
            arr, h IsNot Nothing, Function() h.FixedEffectNames,
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText,
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble, atProfile, errorMessage)
    End Function





    Private Function MmrmFindHeaderIndex(headers() As String, ParamArray acceptedNames() As String) As Integer
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportMetadata38.MmrmFindHeaderIndex(headers, acceptedNames)
    End Function

    Private Function MmrmFindDesignColumnIndex(names() As String, requestedName As String) As Integer
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportMetadata38.MmrmFindDesignColumnIndex(names, requestedName)
    End Function

    Private Function MmrmNormalizeDesignColumnName(s As String) As String
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportMetadata38.MmrmNormalizeDesignColumnName(s)
    End Function

End Module