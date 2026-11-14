Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Namespace ExcelCommon
    Public NotInheritable Class UdfW2CompletionPreparation38
        Private Sub New()
        End Sub

        Public Shared Function ResolveMmrmFormulaColumns(formulaText As String, formulaAddressingMode As String, pAll As Integer, allDisplayNames() As String, allAbsoluteLetters() As String, hasAbsoluteLetters As Boolean, ByRef columnIndices() As Integer, ByRef selectedNames() As String, ByRef selectedAbsoluteLetters() As String, ByRef errorMessage As String) As Boolean
        Dim normalizedFormula As String = If(formulaText, String.Empty).Trim()
        If normalizedFormula = String.Empty Then
            ReDim columnIndices(pAll - 1)
            For j As Integer = 0 To pAll - 1
                columnIndices(j) = j
            Next
            selectedNames = CType(allDisplayNames.Clone(), String())
            If hasAbsoluteLetters Then selectedAbsoluteLetters = CType(allAbsoluteLetters.Clone(), String())
            Return True
        End If

        Dim mode As String = If(formulaAddressingMode, String.Empty).Trim().ToLowerInvariant()
        Dim allowAbsolute As Boolean = String.Equals(mode, "absolute", StringComparison.OrdinalIgnoreCase)
        Dim allowRelative As Boolean = String.Equals(mode, "relative", StringComparison.OrdinalIgnoreCase)
        Dim allowQuoted As Boolean = True

        If allowAbsolute AndAlso Not hasAbsoluteLetters Then
            errorMessage = "formulaAddressing='absolute' requires x to be passed as a direct worksheet range so absolute worksheet column letters can be determined."
            Return False
        End If

        Dim catalog As RegressionVariableCatalog = RegressionVariableCatalog.Build(varNames:=allDisplayNames,
                                                                                   absoluteColumnLetters:=If(allowAbsolute, allAbsoluteLetters, Nothing),
                                                                                   allowRelativeColumnLetters:=allowRelative,
                                                                                   allowAbsoluteColumnLetters:=allowAbsolute,
                                                                                   allowQuotedVariableNames:=allowQuoted)

        Dim spec As RegressionFormulaDesignSpec = Nothing
        Dim parseErr As String = Nothing
        If Not RegressionFormulaParser.TryParseFormulaToDesignSpec(formulaText:=normalizedFormula,
                                                                   variableCatalog:=catalog,
                                                                   designSpec:=spec,
                                                                   errorMessage:=parseErr) Then
            errorMessage = "formula could not be parsed or expanded: " & If(parseErr, String.Empty)
            Return False
        End If

        If spec Is Nothing OrElse spec.RequiredRawVarKeys Is Nothing OrElse spec.RequiredRawVarKeys.Count = 0 Then
            errorMessage = "formula did not reference any predictor columns."
            Return False
        End If

        If allowRelative Then
            ReDim columnIndices(pAll - 1)
            For j As Integer = 0 To pAll - 1
                columnIndices(j) = j
            Next
            selectedNames = CType(allDisplayNames.Clone(), String())
            If hasAbsoluteLetters Then selectedAbsoluteLetters = CType(allAbsoluteLetters.Clone(), String())
            Return True
        End If

        Dim entries As New List(Of RegressionVariableCatalogEntry)()
        For Each requiredKey As String In spec.RequiredRawVarKeys
            Dim entry As RegressionVariableCatalogEntry = catalog.Variables.FirstOrDefault(Function(v) String.Equals(v.BaseKey, requiredKey, StringComparison.Ordinal))
            If entry Is Nothing Then
                errorMessage = "formula referenced an unknown predictor column."
                Return False
            End If
            If Not entries.Any(Function(v) String.Equals(v.BaseKey, entry.BaseKey, StringComparison.Ordinal)) Then
                entries.Add(entry)
            End If
        Next

        entries = entries.OrderBy(Function(v) v.RelativeColumnIndex).ToList()
        ReDim columnIndices(entries.Count - 1)
        ReDim selectedNames(entries.Count - 1)
        If hasAbsoluteLetters Then ReDim selectedAbsoluteLetters(entries.Count - 1)

        For j As Integer = 0 To entries.Count - 1
            Dim originalIndex As Integer = entries(j).RelativeColumnIndex - 1
            If originalIndex < 0 OrElse originalIndex >= pAll Then
                errorMessage = "formula referenced a predictor column outside the supplied x range."
                Return False
            End If
            columnIndices(j) = originalIndex
            selectedNames(j) = entries(j).DisplayName
            If hasAbsoluteLetters Then selectedAbsoluteLetters(j) = entries(j).AbsoluteColumnLetter
        Next

        Return True
        End Function

        Public Shared Function AlignMmrmRows(yCol(,) As Object, xRaw(,) As Object, subjectCol(,) As Object, visitCol(,) As Object, xColumnIndices() As Integer, visitSupplied As Boolean, n As Integer, readNumber As Func(Of Object, Double?), readText As Func(Of Object, String), ByRef yValues() As Double, ByRef xValues(,) As Double, ByRef subjectValues() As Object, ByRef visitValues() As Double, ByRef droppedRows As Integer, ByRef errorMessage As String) As Boolean
        Dim p As Integer = xColumnIndices.Length
        Dim yList As New List(Of Double)()
        Dim xRows As New List(Of Double())()
        Dim subjectList As New List(Of Object)()
        Dim visitList As New List(Of Double)()

        For i As Integer = 0 To n - 1
            Dim yi As Double? = readNumber(yCol(i, 0))
            Dim subjectText As String = readText(subjectCol(i, 0))
            Dim valid As Boolean = yi.HasValue AndAlso Not String.IsNullOrWhiteSpace(subjectText)
            Dim row(p - 1) As Double

            If valid Then
                For j As Integer = 0 To p - 1
                    Dim xij As Double? = readNumber(xRaw(i, xColumnIndices(j)))
                    If Not xij.HasValue Then
                        valid = False
                        Exit For
                    End If
                    row(j) = xij.Value
                Next
            End If

            Dim vi As Double = Double.NaN
            If valid AndAlso visitSupplied Then
                Dim parsedVisit As Double? = readNumber(visitCol(i, 0))
                If Not parsedVisit.HasValue Then
                    valid = False
                Else
                    vi = parsedVisit.Value
                End If
            End If

            If valid Then
                yList.Add(yi.Value)
                xRows.Add(row)
                subjectList.Add(subjectText)
                If visitSupplied Then visitList.Add(vi)
            Else
                droppedRows += 1
            End If
        Next

        If yList.Count = 0 Then
            errorMessage = "no valid complete rows remain after removing rows with missing/invalid y, x, subject, or visit values."
            Return False
        End If

        yValues = yList.ToArray()
        subjectValues = subjectList.ToArray()
        ReDim xValues(yList.Count - 1, p - 1)
        For i As Integer = 0 To yList.Count - 1
            For j As Integer = 0 To p - 1
                xValues(i, j) = xRows(i)(j)
            Next
        Next

        If visitSupplied Then visitValues = visitList.ToArray()
        Return True
        End Function

        Public Shared Function GetReferenceCategory(s As String) As regression.ReferenceCategory
If String.IsNullOrWhiteSpace(s) Then Return regression.ReferenceCategory.Last

        Select Case s.Trim().ToLowerInvariant()
            Case "first", "smallest", "min"
                Return regression.ReferenceCategory.First
            Case Else
                Return regression.ReferenceCategory.Last
        End Select
        End Function
    End Class
End Namespace
