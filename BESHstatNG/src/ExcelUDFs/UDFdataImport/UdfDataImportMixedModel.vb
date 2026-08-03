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

        Dim p As Integer = xColumnIndices.Length
        Dim yList As New List(Of Double)()
        Dim xRows As New List(Of Double())()
        Dim subjectList As New List(Of Object)()
        Dim visitList As New List(Of Double)()

        For i As Integer = 0 To n - 1
            Dim yi As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(yCol(i, 0))
            Dim subjectText As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(subjectCol(i, 0))
            Dim valid As Boolean = yi.HasValue AndAlso Not String.IsNullOrWhiteSpace(subjectText)
            Dim row(p - 1) As Double

            If valid Then
                For j As Integer = 0 To p - 1
                    Dim xij As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(xRaw(i, xColumnIndices(j)))
                    If Not xij.HasValue Then
                        valid = False
                        Exit For
                    End If
                    row(j) = xij.Value
                Next
            End If

            Dim vi As Double = Double.NaN
            If valid AndAlso visitSupplied Then
                Dim parsedVisit As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(visitCol(i, 0))
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
        If expectedCount <= 0 Then Return New String() {}
        Dim out(expectedCount - 1) As String
        For i As Integer = 0 To expectedCount - 1
            Dim nm As String = Nothing
            If inputNames IsNot Nothing AndAlso i < inputNames.Length Then nm = inputNames(i)
            nm = If(nm, String.Empty).Trim()
            If nm = String.Empty Then nm = fallbackPrefix & (i + 1).ToString(CultureInfo.InvariantCulture)
            out(i) = nm
        Next
        Return out
    End Function

    Private Function DefaultMmrmNames(count As Integer, prefix As String) As String()
        Dim names(Math.Max(0, count) - 1) As String
        For i As Integer = 0 To names.Length - 1
            names(i) = prefix & (i + 1).ToString(CultureInfo.InvariantCulture)
        Next
        Return names
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

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows < 2 OrElse cols < 2 Then
            errorMessage = "spec must contain a header row and at least one data row."
            Return False
        End If

        Dim header(cols - 1) As String
        For c As Integer = 0 To cols - 1
            header(c) = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(0, c))
        Next

        Dim labelCol As Integer = MmrmFindHeaderIndex(header, "label", "contrast", "estimate", "name")
        Dim weightCol As Integer = MmrmFindHeaderIndex(header, "weight", "coef", "coefficient", "contrastweight")
        Dim visitCol As Integer = MmrmFindHeaderIndex(header, "visit", "time")

        If weightCol < 0 Then
            errorMessage = "spec header must include a weight column."
            Return False
        End If

        Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()
        For c As Integer = 0 To cols - 1
            If c = labelCol OrElse c = weightCol OrElse c = visitCol Then Continue For
            If String.IsNullOrWhiteSpace(header(c)) Then Continue For

            Dim idx As Integer = MmrmFindDesignColumnIndex(h.FixedEffectNames, header(c))
            If idx < 0 Then
                errorMessage = "profile column header """ & header(c) & """ was not found among fitted design columns: " &
                               String.Join(", ", h.FixedEffectNames) & ". Use header 'visit' for the visit/time column."
                Return False
            End If

            profileColumns.Add(New KeyValuePair(Of Integer, Integer)(c, idx))
        Next

        If profileColumns.Count = 0 AndAlso visitCol < 0 Then
            errorMessage = "spec must include at least one profile column: visit and/or a fitted design column name."
            Return False
        End If

        Dim defaultIndex As Integer = 1

        For r As Integer = 1 To rows - 1
            Dim hasAnyText As Boolean = False
            For c As Integer = 0 To cols - 1
                If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, c)) Then
                    hasAnyText = True
                    Exit For
                End If
            Next
            If Not hasAnyText Then Continue For

            Dim w As Double
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(arr(r, weightCol), w) Then
                errorMessage = "spec row " & (r + 1).ToString(CultureInfo.InvariantCulture) & " has a missing or nonnumeric weight."
                Return False
            End If

            Dim label As String = Nothing
            If labelCol >= 0 Then label = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(r, labelCol))
            If String.IsNullOrWhiteSpace(label) Then label = "Estimate " & defaultIndex.ToString(CultureInfo.InvariantCulture)

            Dim comp As New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateComponent With {
                .Label = label,
                .Weight = w,
                .VisitSpecified = False,
                .VisitValue = Double.NaN
            }

            If visitCol >= 0 AndAlso Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, visitCol)) Then
                Dim v As Double
                If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(arr(r, visitCol), v) Then
                    errorMessage = "spec row " & (r + 1).ToString(CultureInfo.InvariantCulture) & " has a nonnumeric visit value."
                    Return False
                End If
                comp.VisitSpecified = True
                comp.VisitValue = v
            End If

            For Each pair As KeyValuePair(Of Integer, Integer) In profileColumns
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, pair.Key)) Then Continue For

                Dim profileValue As Double
                If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(arr(r, pair.Key), profileValue) Then
                    errorMessage = "spec row " & (r + 1).ToString(CultureInfo.InvariantCulture) &
                                   " has a nonnumeric value for profile column """ & header(pair.Key) & """."
                    Return False
                End If

                comp.ProfileValues.Add(New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue With {
                    .Name = h.FixedEffectNames(pair.Value),
                    .ColumnIndex = pair.Value,
                    .Value = profileValue
                })
            Next

            components.Add(comp)
            defaultIndex += 1
        Next

        If components.Count = 0 Then
            errorMessage = "spec does not contain any nonblank data rows."
            Return False
        End If

        Return True
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

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows < 1 OrElse cols < 1 Then
            errorMessage = "at must contain at least one nonblank name/value setting, or be omitted."
            Return False
        End If

        Dim headerlessAt As New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile()
        Dim attemptedHeaderlessAt As Boolean = False
        If TryGetMmrmHeaderlessNameValueAtSpec(arr, h, headerlessAt, errorMessage, attemptedHeaderlessAt) Then
            atProfile = headerlessAt
            Return True
        End If
        If attemptedHeaderlessAt Then Return False

        If rows < 2 Then
            errorMessage = "at must contain a header row and at least one data row, or use two-column headerless name/value form."
            Return False
        End If

        Dim header(cols - 1) As String
        For c As Integer = 0 To cols - 1
            header(c) = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(0, c))
        Next

        Dim nameCol As Integer = MmrmFindHeaderIndex(header, "name", "variable", "effect", "column", "profile", "at")
        Dim valueCol As Integer = MmrmFindHeaderIndex(header, "value", "val", "setting", "atvalue")

        Dim parsed As New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile()

        If nameCol >= 0 AndAlso valueCol >= 0 AndAlso nameCol <> valueCol Then
            For r As Integer = 1 To rows - 1
                Dim rowHasAny As Boolean = False
                For c As Integer = 0 To cols - 1
                    If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, c)) Then
                        rowHasAny = True
                        Exit For
                    End If
                Next
                If Not rowHasAny Then Continue For

                Dim requestedName As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(r, nameCol))
                If String.IsNullOrWhiteSpace(requestedName) Then
                    errorMessage = "at row " & (r + 1).ToString(CultureInfo.InvariantCulture) & " has a missing name/variable value."
                    Return False
                End If

                Dim value As Double
                If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(arr(r, valueCol), value) Then
                    errorMessage = "at row " & (r + 1).ToString(CultureInfo.InvariantCulture) &
                                   " has a missing or nonnumeric value for """ & requestedName & """."
                    Return False
                End If

                If Not AddMmrmLsmEstimateAtValue(parsed, h, requestedName, value,
                                                 "at row " & (r + 1).ToString(CultureInfo.InvariantCulture),
                                                 errorMessage) Then
                    Return False
                End If
            Next
        Else
            Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()
            Dim visitCol As Integer = MmrmFindHeaderIndex(header, "visit", "time")

            For c As Integer = 0 To cols - 1
                If c = visitCol Then Continue For
                If String.IsNullOrWhiteSpace(header(c)) Then Continue For

                Dim idx As Integer = MmrmFindDesignColumnIndex(h.FixedEffectNames, header(c))
                If idx < 0 Then
                    errorMessage = "at column header """ & header(c) & """ was not found among fitted design columns: " &
                                   String.Join(", ", h.FixedEffectNames) & ". Use header 'visit' for the visit/time column."
                    Return False
                End If

                profileColumns.Add(New KeyValuePair(Of Integer, Integer)(c, idx))
            Next

            If visitCol < 0 AndAlso profileColumns.Count = 0 Then
                errorMessage = "at must contain either name/value headers or at least one visit/design-column header."
                Return False
            End If

            Dim dataRow As Integer = -1
            For r As Integer = 1 To rows - 1
                Dim rowHasAny As Boolean = False
                For c As Integer = 0 To cols - 1
                    If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, c)) Then
                        rowHasAny = True
                        Exit For
                    End If
                Next
                If Not rowHasAny Then Continue For

                If dataRow >= 0 Then
                    errorMessage = "wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row."
                    Return False
                End If

                dataRow = r
            Next

            If dataRow < 0 Then
                errorMessage = "at does not contain any nonblank data row."
                Return False
            End If

            If visitCol >= 0 AndAlso Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(dataRow, visitCol)) Then
                Dim visitValue As Double
                If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(arr(dataRow, visitCol), visitValue) Then
                    errorMessage = "at visit value must be numeric and finite."
                    Return False
                End If

                If Not AddMmrmLsmEstimateAtValue(parsed, h, "visit", visitValue, "at visit column", errorMessage) Then
                    Return False
                End If
            End If

            For Each pair As KeyValuePair(Of Integer, Integer) In profileColumns
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(dataRow, pair.Key)) Then Continue For

                Dim profileValue As Double
                If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(arr(dataRow, pair.Key), profileValue) Then
                    errorMessage = "at value for column """ & header(pair.Key) & """ must be numeric and finite."
                    Return False
                End If

                If Not AddMmrmLsmEstimateAtValue(parsed, h, h.FixedEffectNames(pair.Value), profileValue,
                                                 "at column """ & header(pair.Key) & """",
                                                 errorMessage) Then
                    Return False
                End If
            Next
        End If

        If Not parsed.VisitSpecified AndAlso parsed.ProfileValues.Count = 0 Then
            errorMessage = "at does not specify any nonblank visit or fitted design-column values."
            Return False
        End If

        atProfile = parsed
        Return True
    End Function

    Private Function TryGetMmrmHeaderlessNameValueAtSpec(arr(,) As Object,
                                                            h As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmHandle,
                                                            atProfile As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile,
                                                            ByRef errorMessage As String,
                                                            ByRef attempted As Boolean) As Boolean
        attempted = False
        errorMessage = Nothing

        If arr Is Nothing OrElse h Is Nothing OrElse atProfile Is Nothing Then Return False
        If arr.GetLength(1) <> 2 Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim firstNonblankRow As Integer = -1
        For r As Integer = 0 To rows - 1
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, 0)) OrElse
               Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, 1)) Then
                firstNonblankRow = r
                Exit For
            End If
        Next

        If firstNonblankRow < 0 Then Return False

        Dim firstName As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(firstNonblankRow, 0))
        If String.IsNullOrWhiteSpace(firstName) Then Return False

        Dim firstValue As Double
        If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(arr(firstNonblankRow, 1), firstValue) Then
            Return False
        End If

        Dim probe As New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile()
        Dim probeError As String = Nothing
        If Not AddMmrmLsmEstimateAtValue(probe, h, firstName, firstValue,
                                         "at row " & (firstNonblankRow + 1).ToString(CultureInfo.InvariantCulture),
                                         probeError) Then
            Return False
        End If

        attempted = True

        For r As Integer = 0 To rows - 1
            Dim nameBlank As Boolean = Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, 0))
            Dim valueBlank As Boolean = Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, 1))
            If nameBlank AndAlso valueBlank Then Continue For

            Dim requestedName As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(r, 0))
            If String.IsNullOrWhiteSpace(requestedName) Then
                errorMessage = "at row " & (r + 1).ToString(CultureInfo.InvariantCulture) & " has a missing name/variable value."
                Return False
            End If

            Dim value As Double
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(arr(r, 1), value) Then
                errorMessage = "at row " & (r + 1).ToString(CultureInfo.InvariantCulture) &
                               " has a missing or nonnumeric value for """ & requestedName & """."
                Return False
            End If

            If Not AddMmrmLsmEstimateAtValue(atProfile, h, requestedName, value,
                                             "at row " & (r + 1).ToString(CultureInfo.InvariantCulture),
                                             errorMessage) Then
                Return False
            End If
        Next

        Return atProfile.VisitSpecified OrElse atProfile.ProfileValues.Count > 0
    End Function

    Private Function AddMmrmLsmEstimateAtValue(atProfile As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile,
                                               h As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmHandle,
                                               requestedName As String,
                                               value As Double,
                                               sourceDescription As String,
                                               ByRef errorMessage As String) As Boolean
        If atProfile Is Nothing Then
            errorMessage = "internal error: AT profile was not initialized."
            Return False
        End If

        If String.Equals(MmrmNormalizeDesignColumnName(requestedName), MmrmNormalizeDesignColumnName("visit"), StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(MmrmNormalizeDesignColumnName(requestedName), MmrmNormalizeDesignColumnName("time"), StringComparison.OrdinalIgnoreCase) Then

            If atProfile.VisitSpecified Then
                errorMessage = "at specifies visit/time more than once."
                Return False
            End If

            atProfile.VisitSpecified = True
            atProfile.VisitValue = value
            Return True
        End If

        Dim idx As Integer = MmrmFindDesignColumnIndex(h.FixedEffectNames, requestedName)
        If idx < 0 Then
            errorMessage = sourceDescription & " names """ & requestedName & """, which was not found among fitted design columns: " &
                           String.Join(", ", h.FixedEffectNames) & "."
            Return False
        End If

        For Each existing As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue In atProfile.ProfileValues
            If existing.ColumnIndex = idx Then
                errorMessage = "at specifies design column """ & h.FixedEffectNames(idx) & """ more than once."
                Return False
            End If
        Next

        atProfile.ProfileValues.Add(New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue With {
            .Name = h.FixedEffectNames(idx),
            .ColumnIndex = idx,
            .Value = value
        })
        Return True
    End Function

    Private Function MmrmFindHeaderIndex(headers() As String, ParamArray acceptedNames() As String) As Integer
        If headers Is Nothing OrElse acceptedNames Is Nothing Then Return -1

        For i As Integer = 0 To headers.Length - 1
            Dim h As String = MmrmNormalizeDesignColumnName(headers(i))
            For Each accepted As String In acceptedNames
                If String.Equals(h, MmrmNormalizeDesignColumnName(accepted), StringComparison.OrdinalIgnoreCase) Then Return i
            Next
        Next

        Return -1
    End Function

    Private Function MmrmFindDesignColumnIndex(names() As String, requestedName As String) As Integer
        If names Is Nothing OrElse String.IsNullOrWhiteSpace(requestedName) Then Return -1

        For i As Integer = 0 To names.Length - 1
            If String.Equals(names(i), requestedName, StringComparison.OrdinalIgnoreCase) Then Return i
        Next

        Dim wanted As String = MmrmNormalizeDesignColumnName(requestedName)
        For i As Integer = 0 To names.Length - 1
            If String.Equals(MmrmNormalizeDesignColumnName(names(i)), wanted, StringComparison.OrdinalIgnoreCase) Then Return i
        Next

        Return -1
    End Function

    Private Function MmrmNormalizeDesignColumnName(s As String) As String
        If s Is Nothing Then Return String.Empty
        Return New String(s.Trim().ToLowerInvariant().Where(Function(ch) Char.IsLetterOrDigit(ch)).ToArray())
    End Function

End Module