Option Explicit On
Option Strict On
Imports ExcelDna.Integration
Imports System.Collections.Generic

' Formula-related UDF import helpers.
' Keeps formula addressing, worksheet column-letter discovery, and explicit variable-name parsing behind the shared UdfDataImport facade.
Partial Friend Module UdfDataImport

    ''' <summary>
    ''' Parses a formula-addressing argument such as relative or absolute, preserving the existing defaulting behavior.
    ''' </summary>
    Friend Function GetFormulaAddressingMode(arg As Object, defaultMode As String) As String
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportOptions38.GetFormulaAddressingMode(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), defaultMode)
    End Function

    ''' <summary>
    ''' Attempts to discover absolute worksheet column letters for a direct range argument.
    ''' </summary>
    Friend Function TryGetAbsoluteColumnLetters(rangeArg As Object, expectedColumns As Integer, ByRef columnLetters() As String) As Boolean
        columnLetters = Nothing

        If expectedColumns < 1 Then Return False
        If rangeArg Is Nothing Then Return False
        If Not TypeOf rangeArg Is ExcelReference Then Return False

        Try
            Dim xref As ExcelReference = CType(rangeArg, ExcelReference)
            Dim firstCol As Integer = xref.ColumnFirst
            Dim lastCol As Integer = xref.ColumnLast
            Dim width As Integer = lastCol - firstCol + 1

            If width <> expectedColumns Then Return False

            ReDim columnLetters(width - 1)
            For j As Integer = 0 To width - 1
                columnLetters(j) = RegressionVariableCatalog.NumberToLetters(firstCol + j + 1)
            Next

            Return True
        Catch
            columnLetters = Nothing
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Resolves explicit variable names from a worksheet argument, falling back to generated names.
    ''' Accepts comma-separated text, a one-row range, or a one-column range. Blank supplied names fall back positionally.
    ''' </summary>
    Friend Function GetVariableNames(arg As Object, expectedCount As Integer) As String()
        If expectedCount <= 0 Then Return New String() {}
        Return Global.BESHStatNG.ExcelCommon.UdfNamePreparation38.GetVariableNames(expectedCount,
            Function() Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg),
            Function() TryCast(arg, String),
            Function() As Global.BESHStatNG.ExcelCommon.UdfVariableNameRange38
                Dim arr As Object(,) = Get2D(arg)
                If arr Is Nothing Then Return Nothing
                Dim rows As Integer = arr.GetLength(0)
                Dim cols As Integer = arr.GetLength(1)
                Return New Global.BESHStatNG.ExcelCommon.UdfVariableNameRange38(rows, cols,
                    Function(row As Integer, column As Integer) As String
                        Dim cell As Object = arr(row, column)
                        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(cell) Then Return String.Empty
                        Return Convert.ToString(cell).Trim()
                    End Function)
            End Function)
    End Function

    ''' <summary>
    ''' Imports new-predictor data for formula-based prediction using either the full fit-time raw key set
    ''' or the formula-required raw key set, then expands it with the supplied design specification.
    ''' </summary>
    Friend Function TryGetPredictionDesignFromCandidateKeys(newX As Object,
                                                            newOffset As Object,
                                                            hasOffset As Boolean,
                                                            fullRawPredictorKeys As String(),
                                                            requiredRawPredictorKeys As String(),
                                                            designSpec As RegressionFormulaDesignSpec,
                                                            omitCategoricalReference As Boolean,
                                                            expectedExpandedPredictorNames As String(),
                                                            ByRef nRows As Integer,
                                                            ByRef offsetVals() As Double,
                                                            ByRef expandedX(,) As Double) As Boolean
        nRows = 0
        offsetVals = Nothing
        expandedX = Nothing

        Dim candidateKeySets As New List(Of String())()
        If fullRawPredictorKeys IsNot Nothing AndAlso fullRawPredictorKeys.Length > 0 Then
            candidateKeySets.Add(fullRawPredictorKeys)
        End If

        If requiredRawPredictorKeys IsNot Nothing AndAlso requiredRawPredictorKeys.Length > 0 Then
            Dim addRequired As Boolean = True
            If candidateKeySets.Count > 0 AndAlso SequenceEqualStrings(candidateKeySets(0), requiredRawPredictorKeys) Then addRequired = False
            If addRequired Then candidateKeySets.Add(requiredRawPredictorKeys)
        End If

        For Each candidateKeys As String() In candidateKeySets
            Dim imported As glmData = Nothing
            If Not TryGetPredictorData(newX, candidateKeys, newOffset, hasOffset, imported) Then Continue For
            If imported Is Nothing OrElse imported.nCols <> candidateKeys.Length Then Continue For

            Dim expandedNames() As String = Nothing
            Dim designErr As String = Nothing
            Dim candidateExpandedX(,) As Double = Nothing
            If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromDesignSpec(rawX:=imported.DataDbl,
                                                                                                fullRawPredictorKeys:=candidateKeys,
                                                                                                designSpec:=designSpec,
                                                                                                expandedX:=candidateExpandedX,
                                                                                                expandedPredictorNames:=expandedNames,
                                                                                                errorMessage:=designErr,
                                                                                                omitCategoricalReference:=omitCategoricalReference) Then
                Continue For
            End If

            If expandedNames Is Nothing Then expandedNames = New String() {}
            If expectedExpandedPredictorNames IsNot Nothing AndAlso expandedNames.Length <> expectedExpandedPredictorNames.Length Then Continue For

            Dim candidateOffset() As Double = If(imported.bOffset, imported.OffsetData, Nothing)
            If Not UdfDataImport.HasOnlyFinite(candidateOffset) Then Continue For

            nRows = imported.nRows
            offsetVals = candidateOffset
            expandedX = candidateExpandedX
            Return True
        Next

        Return False
    End Function

    Private Function SequenceEqualStrings(left As String(), right As String()) As Boolean
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportMetadata38.SequenceEqualStrings(left, right)
    End Function

End Module
