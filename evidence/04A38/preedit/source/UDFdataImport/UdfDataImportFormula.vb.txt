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
        Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)
        If String.IsNullOrWhiteSpace(s) Then Return defaultMode

        Select Case s.Trim().ToLowerInvariant()
            Case "relative", "rel", "x"
                Return "relative"
            Case "absolute", "abs", "worksheet"
                Return "absolute"
            Case "names", "name", "quoted", "variables", "varnames"
                Return "names"
            Case Else
                Return defaultMode
        End Select
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

        Dim fallback(expectedCount - 1) As String
        For i As Integer = 0 To expectedCount - 1
            fallback(i) = "X" & (i + 1).ToString()
        Next

        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return fallback

        Dim s As String = TryCast(arg, String)
        If s IsNot Nothing Then
            Dim parts As String() = s.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length = expectedCount Then
                For i As Integer = 0 To expectedCount - 1
                    parts(i) = parts(i).Trim()
                    If String.IsNullOrWhiteSpace(parts(i)) Then parts(i) = fallback(i)
                Next
                Return parts
            End If
        End If

        Dim arr As Object(,) = Get2D(arg)
        If arr Is Nothing Then Return fallback

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim list As New List(Of String)()

        If rows = 1 AndAlso cols >= 1 Then
            For j As Integer = 0 To cols - 1
                Dim cell As Object = arr(0, j)
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(cell) Then
                    list.Add(String.Empty)
                Else
                    list.Add(Convert.ToString(cell).Trim())
                End If
            Next
        ElseIf cols = 1 AndAlso rows >= 1 Then
            For i As Integer = 0 To rows - 1
                Dim cell As Object = arr(i, 0)
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(cell) Then
                    list.Add(String.Empty)
                Else
                    list.Add(Convert.ToString(cell).Trim())
                End If
            Next
        End If

        If list.Count = expectedCount Then
            For i As Integer = 0 To expectedCount - 1
                If String.IsNullOrWhiteSpace(list(i)) Then list(i) = fallback(i)
            Next
            Return list.ToArray()
        End If

        Return fallback
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
        If left Is Nothing OrElse right Is Nothing Then Return left Is right
        If left.Length <> right.Length Then Return False
        For i As Integer = 0 To left.Length - 1
            If Not String.Equals(left(i), right(i), StringComparison.Ordinal) Then Return False
        Next
        Return True
    End Function

End Module
