Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

Namespace ExcelCommon

    ''' <summary>
    ''' Host-neutral spreadsheet-to-Core regression design bridge. No model is fitted and
    ''' no alternate formula grammar or statistical contrast calculation is implemented.
    ''' </summary>
    Public NotInheritable Class SpreadsheetModelInputCompiler
        Private Sub New()
        End Sub

        Public Shared Function Compile(input As SpreadsheetRangeData,
                        Optional options As SpreadsheetModelParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetModelInput)
            Dim config As SpreadsheetModelParseOptions = If(options, New SpreadsheetModelParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If config.CellPolicy Is Nothing OrElse config.MinimumCompleteRows < 1 OrElse
               config.MaximumExpandedColumns < 1 OrElse
               Not [Enum].IsDefined(GetType(SpreadsheetIncompleteObservationPolicy), config.Missing) OrElse
               Not [Enum].IsDefined(GetType(SpreadsheetHeaderMode), config.Header) Then
                Add(issues, Nothing, "model.options", SpreadsheetValidationSeverity.[Error],
                    "Invalid model input options.", "options")
                Return Finish(Nothing, issues)
            End If
            Dim frameResult = RectangularCategoricalParsers.ParseDataFrame(input,
                New SpreadsheetDataFrameParseOptions With {.Header = config.Header})
            issues.AddRange(frameResult.Validation.Issues)
            If Not frameResult.Success Then Return Finish(Nothing, issues)
            Dim frame As SpreadsheetRectangularDataFrameInput = frameResult.Data
            Dim responseColumn As Integer = config.ResponseColumnIndex
            If responseColumn < 0 OrElse responseColumn >= frame.ColumnCount Then
                Add(issues, Nothing, "model.response-role", SpreadsheetValidationSeverity.[Error],
                    "Response column is outside the selected range.", "options.response")
                Return Finish(Nothing, issues)
            End If

            Dim predictors As New List(Of Integer)()
            If config.PredictorColumnIndices Is Nothing Then
                For col As Integer = 0 To frame.ColumnCount - 1
                    If col <> responseColumn Then predictors.Add(col)
                Next
            Else
                predictors.AddRange(config.PredictorColumnIndices)
            End If
            If predictors.Count = 0 OrElse predictors.Distinct().Count() <> predictors.Count OrElse
               predictors.Any(Function(col) col < 0 OrElse col >= frame.ColumnCount OrElse col = responseColumn) Then
                Add(issues, Nothing, "model.predictor-role", SpreadsheetValidationSeverity.[Error],
                    "Select unique predictor columns that do not overlap the response.", "options.predictors")
                Return Finish(Nothing, issues)
            End If
            Dim categorical As Integer() = If(config.CategoricalColumnIndices, Array.Empty(Of Integer)())
            Dim categories As New HashSet(Of Integer)(categorical)
            If categories.Count <> categorical.Length OrElse
               categories.Any(Function(col) Not predictors.Contains(col)) Then
                Add(issues, Nothing, "model.category-role", SpreadsheetValidationSeverity.[Error],
                    "Categorical column roles must be unique and selected as predictors.", "options.categorical")
                Return Finish(Nothing, issues)
            End If
            If If(config.FormulaText, String.Empty).Contains("~") Then
                Add(issues, Nothing, "model.rhs-only", SpreadsheetValidationSeverity.[Error],
                    "The response is selected separately; supply only the formula right-hand side.", "options.formula")
                Return Finish(Nothing, issues)
            End If

            Dim names As New List(Of String)()
            Dim keys As New List(Of String)()
            Dim absoluteLetters As New List(Of String)()
            For Each c As Integer In predictors
                names.Add(frame.Columns(c).Name)
                keys.Add("V" & (c + 1).ToString(CultureInfo.InvariantCulture))
                absoluteLetters.Add(If(frame.Columns(c).WorksheetColumn > 0,
                    Global.BESHStatNG.RegressionVariableCatalog.NumberToLetters(frame.Columns(c).WorksheetColumn), String.Empty))
            Next
            Dim spec As Global.BESHStatNG.RegressionFormulaDesignSpec = Nothing
            Dim formulaError As String = Nothing
            Try
                Dim catalog = Global.BESHStatNG.RegressionVariableCatalog.Build(names, keys, absoluteLetters,
                    config.AllowRelativeColumnLetters, config.AllowAbsoluteColumnLetters, config.AllowQuotedVariableNames)
                If Not Global.BESHStatNG.RegressionFormulaParser.TryParseFormulaToDesignSpec(config.FormulaText, catalog, spec, formulaError) Then
                    Add(issues, Nothing, "model.formula", SpreadsheetValidationSeverity.[Error],
                        If(formulaError, "Invalid regression formula."), "options.formula")
                    Return Finish(Nothing, issues)
                End If
            Catch ex As ArgumentException
                Add(issues, Nothing, "model.formula", SpreadsheetValidationSeverity.[Error], ex.Message, "options.formula")
                Return Finish(Nothing, issues)
            End Try

            Dim required As New HashSet(Of String)(spec.RequiredRawVarKeys, StringComparer.Ordinal)
            Dim factorKeys As New HashSet(Of String)(StringComparer.Ordinal)
            For Each term As Global.BESHStatNG.TermSpec In spec.TermSpecs.Values
                If term.Scale = Global.BESHStatNG.PredictorScale.Categorical AndAlso term.BaseVarKeys IsNot Nothing Then
                    For Each key As String In term.BaseVarKeys
                        ' Interaction term specs may be mixed-type; Core adds categorical
                        ' helper terms to TermSpecs for each actual factor base variable.
                        If spec.TermSpecs.ContainsKey(Global.BESHStatNG.RegressionDesignCore.MakeCategoricalEffectKey(key)) Then
                            factorKeys.Add(key)
                        End If
                    Next
                End If
            Next
            For p As Integer = 0 To predictors.Count - 1
                If required.Contains(keys(p)) AndAlso categories.Contains(predictors(p)) AndAlso Not factorKeys.Contains(keys(p)) Then
                    Add(issues, Nothing, "model.factor-required", SpreadsheetValidationSeverity.[Error],
                        "Categorical predictor '" & names(p) & "' must use factor(...) in the formula.", "options.formula")
                End If
            Next
            If HasErrors(issues) Then Return Finish(Nothing, issues)

            ' Validate the response and *required* predictors first. Excluded/unreferenced
            ' columns never discard a case or change factor levels.
            Dim complete As New List(Of Integer)()
            Dim numericRows As New List(Of Double())()
            Dim typedRows As New List(Of SpreadsheetCategoryValue())()
            Dim responses As New List(Of Double)()
            For r As Integer = 0 To frame.RowCount - 1
                Dim responseCell As SpreadsheetFrameCell = frame.Cell(r, responseColumn)
                Dim responseValue As Double = 0.0R
                Dim rowMissing As Boolean = False
                Dim rowInvalid As Boolean = False
                ReadNumeric(responseCell, config.CellPolicy, issues, "response", responseValue, rowMissing, rowInvalid)
                Dim numeric(predictors.Count - 1) As Double
                Dim typed(predictors.Count - 1) As SpreadsheetCategoryValue
                For p As Integer = 0 To predictors.Count - 1
                    If Not required.Contains(keys(p)) Then Continue For
                    Dim cell As SpreadsheetFrameCell = frame.Cell(r, predictors(p))
                    If categories.Contains(predictors(p)) Then
                        Dim missing As Boolean = False
                        Dim invalid As Boolean = False
                        typed(p) = ReadCategory(cell, config.CellPolicy, config.IgnoreCategoryCase, issues, missing, invalid)
                        rowMissing = rowMissing OrElse missing
                        rowInvalid = rowInvalid OrElse invalid
                    Else
                        Dim missing As Boolean = False
                        Dim invalid As Boolean = False
                        ReadNumeric(cell, config.CellPolicy, issues, "predictors", numeric(p), missing, invalid)
                        rowMissing = rowMissing OrElse missing
                        rowInvalid = rowInvalid OrElse invalid
                    End If
                Next
                If rowInvalid Then Continue For
                If rowMissing Then
                    Dim severity As SpreadsheetValidationSeverity = If(config.Missing = SpreadsheetIncompleteObservationPolicy.Skip,
                        SpreadsheetValidationSeverity.Warning, SpreadsheetValidationSeverity.[Error])
                    Add(issues, responseCell, If(config.Missing = SpreadsheetIncompleteObservationPolicy.Skip,
                        "model.incomplete-skipped", "model.incomplete"), severity,
                        "Incomplete model observation " & If(severity = SpreadsheetValidationSeverity.Warning, "skipped.", "is not permitted."), "rows")
                    Continue For
                End If
                complete.Add(r)
                numericRows.Add(numeric)
                typedRows.Add(typed)
                responses.Add(responseValue)
            Next
            If complete.Count < config.MinimumCompleteRows Then
                Add(issues, Nothing, "model.too-few", SpreadsheetValidationSeverity.[Error],
                    "Not enough complete model observations.", "rows")
            End If
            If HasErrors(issues) Then Return Finish(Nothing, issues)

            Dim raw(complete.Count - 1, predictors.Count - 1) As Double
            Dim levels As New List(Of SpreadsheetModelCategoricalColumn)()
            For p As Integer = 0 To predictors.Count - 1
                If categories.Contains(predictors(p)) AndAlso required.Contains(keys(p)) Then
                    Dim observed As New List(Of SpreadsheetCategoryValue)()
                    Dim seenKeys As New HashSet(Of String)(StringComparer.Ordinal)
                    For i As Integer = 0 To complete.Count - 1
                        Dim category As SpreadsheetCategoryValue = typedRows(i)(p)
                        If seenKeys.Add(category.Key) Then observed.Add(category)
                    Next
                    ' Preserve original numeric/Boolean levels for Core factor(reference)
                    ' compatibility. Text or mixed-type categories need collision-free codes.
                    Dim allNumeric As Boolean = observed.All(Function(v) v.Kind = SpreadsheetCellKind.Number)
                    Dim allBoolean As Boolean = observed.All(Function(v) v.Kind = SpreadsheetCellKind.Bool)
                    Dim codes As New Dictionary(Of String, Double)(StringComparer.Ordinal)
                    Dim encountered As New List(Of SpreadsheetModelCategoryLevel)()
                    For ix As Integer = 0 To observed.Count - 1
                        Dim category As SpreadsheetCategoryValue = observed(ix)
                        Dim code As Double = If(allNumeric, category.NumberValue,
                            If(allBoolean, If(category.BooleanValue, 1.0R, 0.0R), CDbl(ix + 1)))
                        codes.Add(category.Key, code)
                        encountered.Add(New SpreadsheetModelCategoryLevel(category, code))
                    Next
                    For i As Integer = 0 To complete.Count - 1
                        raw(i, p) = codes(typedRows(i)(p).Key)
                    Next
                    levels.Add(New SpreadsheetModelCategoricalColumn(names(p), predictors(p) + 1, encountered))
                Else
                    For i As Integer = 0 To complete.Count - 1
                        raw(i, p) = numericRows(i)(p)
                    Next
                End If
            Next

            Dim built As Global.BESHStatNG.RegressionFormulaMatrixBuildResult = Nothing
            Try
                If Not Global.BESHStatNG.RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromFormula(
                    rawX:=raw, result:=built, errorMessage:=formulaError,
                    predictorNames:=names, formulaText:=config.FormulaText,
                    baseKeys:=keys, absoluteColumnLetters:=absoluteLetters,
                    allowRelativeColumnLetters:=config.AllowRelativeColumnLetters,
                    allowAbsoluteColumnLetters:=config.AllowAbsoluteColumnLetters,
                    allowQuotedVariableNames:=config.AllowQuotedVariableNames,
                    omitCategoricalReference:=config.IncludeIntercept) Then
                    Add(issues, Nothing, "model.design", SpreadsheetValidationSeverity.[Error],
                        If(formulaError, "Unable to expand formula."), "options.formula")
                    Return Finish(Nothing, issues)
                End If
            Catch ex As ArgumentException
                Add(issues, Nothing, "model.design", SpreadsheetValidationSeverity.[Error], ex.Message, "options.formula")
                Return Finish(Nothing, issues)
            End Try
            If built Is Nothing OrElse built.ExpandedPredictorMatrix Is Nothing OrElse
               built.ExpandedPredictorNames Is Nothing OrElse
               built.ExpandedPredictorMatrix.GetLength(0) <> complete.Count OrElse
               built.ExpandedPredictorNames.Length <> built.ExpandedPredictorMatrix.GetLength(1) Then
                Add(issues, Nothing, "model.design", SpreadsheetValidationSeverity.[Error],
                    "Core returned an incomplete design matrix or column schema.", "design")
                Return Finish(Nothing, issues)
            End If
            Dim expanded As Double(,) = built.ExpandedPredictorMatrix
            Dim n As Integer = complete.Count
            Dim width As Integer = expanded.GetLength(1) + If(config.IncludeIntercept, 1, 0)
            If width < 1 OrElse width > config.MaximumExpandedColumns Then
                Add(issues, Nothing, "model.design-width", SpreadsheetValidationSeverity.[Error],
                    "Expanded design contains zero or too many columns.", "options.max-columns")
                Return Finish(Nothing, issues)
            End If
            Dim design(n - 1, width - 1) As Double
            Dim outputNames As New List(Of String)()
            If config.IncludeIntercept Then outputNames.Add("(Intercept)")
            outputNames.AddRange(built.ExpandedPredictorNames)
            For i As Integer = 0 To n - 1
                If config.IncludeIntercept Then design(i, 0) = 1.0R
                For j As Integer = 0 To expanded.GetLength(1) - 1
                    Dim value As Double = expanded(i, j)
                    If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                        Add(issues, Nothing, "model.nonfinite-design", SpreadsheetValidationSeverity.[Error],
                            "Expanded design contains non-finite values.", "design")
                        Return Finish(Nothing, issues)
                    End If
                    design(i, j + If(config.IncludeIntercept, 1, 0)) = value
                Next
            Next
            Dim rank As Integer = EstimateRank(design)
            If rank < width Then
                Add(issues, Nothing, "model.rank-deficient", If(config.RejectRankDeficiency,
                    SpreadsheetValidationSeverity.[Error], SpreadsheetValidationSeverity.Warning),
                    "Design matrix has rank " & rank.ToString(CultureInfo.InvariantCulture) &
                    " of " & width.ToString(CultureInfo.InvariantCulture) & ".", "design")
                If HasErrors(issues) Then Return Finish(Nothing, issues)
            End If
            Dim rows As New List(Of SpreadsheetModelSourceRow)()
            For Each r As Integer In complete
                Dim cell As SpreadsheetFrameCell = frame.Cell(r, responseColumn)
                rows.Add(New SpreadsheetModelSourceRow(cell.SelectedRow, cell.WorksheetRow))
            Next
            Dim model As New SpreadsheetModelInput(frame.Columns(responseColumn).Name, responses.ToArray(), raw,
                design, outputNames, names, built.RequiredRawPredictorNames, rows, levels,
                config.FormulaText, spec.NormalizedFormulaText, config.IncludeIntercept, rank,
                frame.WorkbookId, frame.WorksheetId, frame.SelectionRevision)
            Return Finish(model, issues)
        End Function

        Private Shared Sub ReadNumeric(cell As SpreadsheetFrameCell, policy As SpreadsheetCellPolicyOptions,
                                      issues As List(Of SpreadsheetValidationIssue), path As String,
                                      ByRef value As Double, ByRef missing As Boolean, ByRef invalid As Boolean)
            Dim eval As SpreadsheetCellEvaluation
            Try
                eval = SpreadsheetCellValuePolicy.Evaluate(cell.ToCellValue(), policy)
            Catch ex As ArgumentException
                Add(issues, cell, "model.invalid-value", SpreadsheetValidationSeverity.[Error], ex.Message, path)
                invalid = True
                Return
            End Try
            If eval.IsError AndAlso policy.Errors = SpreadsheetErrorPolicy.Reject Then
                Add(issues, cell, "model.cell-error", SpreadsheetValidationSeverity.[Error], "Error cell cannot be a model value.", path)
                invalid = True
            ElseIf eval.IsMissing Then
                missing = True
            ElseIf Not eval.HasNumericValue Then
                Add(issues, cell, "model.not-numeric", SpreadsheetValidationSeverity.[Error], "A numeric cell is required.", path)
                invalid = True
            Else
                value = eval.NumericValue
            End If
        End Sub

        Private Shared Function ReadCategory(cell As SpreadsheetFrameCell, policy As SpreadsheetCellPolicyOptions,
                                             ignoreCase As Boolean, issues As List(Of SpreadsheetValidationIssue),
                                             ByRef missing As Boolean, ByRef invalid As Boolean) As SpreadsheetCategoryValue
            Dim eval As SpreadsheetCellEvaluation
            Try
                eval = SpreadsheetCellValuePolicy.Evaluate(cell.ToCellValue(), policy)
            Catch ex As ArgumentException
                Add(issues, cell, "model.invalid-value", SpreadsheetValidationSeverity.[Error], ex.Message, "predictors")
                invalid = True
                Return Nothing
            End Try
            If eval.IsError AndAlso policy.Errors = SpreadsheetErrorPolicy.Reject Then
                Add(issues, cell, "model.cell-error", SpreadsheetValidationSeverity.[Error], "Error cell cannot be a category.", "predictors")
                invalid = True
                Return Nothing
            End If
            If eval.IsMissing Then
                missing = True
                Return Nothing
            End If
            Try
                Return New SpreadsheetCategoryValue(cell.ToCellValue(), ignoreCase)
            Catch ex As ArgumentException
                Add(issues, cell, "model.category-invalid", SpreadsheetValidationSeverity.[Error], ex.Message, "predictors")
                invalid = True
                Return Nothing
            End Try
        End Function

        ''' <summary>Reorthogonalized MGS for preflight rank diagnostics, not a model fit.</summary>
        Private Shared Function EstimateRank(design As Double(,)) As Integer
            Dim n As Integer = design.GetLength(0)
            Dim p As Integer = design.GetLength(1)
            Dim maxNorm As Double = 0.0R
            For c As Integer = 0 To p - 1
                Dim ss As Double = 0.0R
                For r As Integer = 0 To n - 1
                    ss += design(r, c) * design(r, c)
                Next
                maxNorm = Math.Max(maxNorm, Math.Sqrt(ss))
            Next
            Dim tolerance As Double = maxNorm * 0.0000000001R
            Dim vectors As New List(Of Double())()
            For c As Integer = 0 To p - 1
                Dim v(n - 1) As Double
                For r As Integer = 0 To n - 1
                    v(r) = design(r, c)
                Next
                For pass As Integer = 1 To 2
                    For Each q As Double() In vectors
                        Dim dot As Double = 0.0R
                        For r As Integer = 0 To n - 1
                            dot += v(r) * q(r)
                        Next
                        For r As Integer = 0 To n - 1
                            v(r) -= dot * q(r)
                        Next
                    Next
                Next
                Dim sum As Double = 0.0R
                For Each number As Double In v
                    sum += number * number
                Next
                Dim norm As Double = Math.Sqrt(sum)
                If norm > tolerance Then
                    For r As Integer = 0 To n - 1
                        v(r) /= norm
                    Next
                    vectors.Add(v)
                End If
            Next
            Return vectors.Count
        End Function

        Private Shared Sub Add(issues As List(Of SpreadsheetValidationIssue), cell As SpreadsheetFrameCell,
                               code As String, severity As SpreadsheetValidationSeverity,
                               message As String, path As String)
            issues.Add(New SpreadsheetValidationIssue(code, severity, message, path,
                If(cell Is Nothing, 0, cell.SelectedRow), If(cell Is Nothing, 0, cell.SelectedColumn),
                If(cell Is Nothing, 0, cell.WorksheetRow), If(cell Is Nothing, 0, cell.WorksheetColumn)))
        End Sub
        Private Shared Function HasErrors(issues As List(Of SpreadsheetValidationIssue)) As Boolean
            Return issues.Any(Function(issue) issue.Severity = SpreadsheetValidationSeverity.[Error])
        End Function
        Private Shared Function Finish(data As SpreadsheetModelInput, issues As List(Of SpreadsheetValidationIssue)) As SpreadsheetTypedParseResult(Of SpreadsheetModelInput)
            Return New SpreadsheetTypedParseResult(Of SpreadsheetModelInput)(data, New SpreadsheetValidationReport(issues))
        End Function
    End Class
End Namespace
