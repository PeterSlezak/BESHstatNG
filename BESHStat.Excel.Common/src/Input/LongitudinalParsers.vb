Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon

    ''' <summary>Host-neutral survival, subject/visit and mixed-model input parsing. No model fitting.</summary>
    Public NotInheritable Class LongitudinalParsers
        Private Sub New()
        End Sub

        Public Shared Function ParseSurvival(input As SpreadsheetRangeData,
                  Optional options As SpreadsheetSurvivalParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetSurvivalInput)
            Dim config As SpreadsheetSurvivalParseOptions = If(options, New SpreadsheetSurvivalParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            Dim frame As SpreadsheetRectangularDataFrameInput = Prepare(input, config.Header, issues)
            If frame Is Nothing Then Return Finish(Of SpreadsheetSurvivalInput)(Nothing, issues)
            If config.CellPolicy Is Nothing OrElse config.MinimumObservations < 1 OrElse
               Not ValidMissing(config.Missing) OrElse
               Not ColumnsValid(frame.ColumnCount, New Integer() {config.TimeColumnIndex, config.StatusColumnIndex},
                                New Integer() {config.EntryColumnIndex, config.StratumColumnIndex}) Then
                Add(issues, Nothing, "survival.options", SpreadsheetValidationSeverity.[Error], "Invalid survival column roles or parsing options.", "options")
                Return Finish(Of SpreadsheetSurvivalInput)(Nothing, issues)
            End If
            If (config.EventText Is Nothing) Xor (config.CensorText Is Nothing) Then
                Add(issues, Nothing, "survival.status-coding", SpreadsheetValidationSeverity.[Error], "Both text status labels must be supplied.", "options.status")
                Return Finish(Of SpreadsheetSurvivalInput)(Nothing, issues)
            End If
            If config.EventText IsNot Nothing AndAlso (String.IsNullOrWhiteSpace(config.EventText) OrElse
                String.IsNullOrWhiteSpace(config.CensorText) OrElse
                String.Equals(config.EventText, config.CensorText, If(config.IgnoreStatusCase, StringComparison.OrdinalIgnoreCase, StringComparison.Ordinal))) Then
                Add(issues, Nothing, "survival.status-coding", SpreadsheetValidationSeverity.[Error], "Event and censor labels must be distinct nonblank text.", "options.status")
                Return Finish(Of SpreadsheetSurvivalInput)(Nothing, issues)
            End If
            Dim rows As New List(Of SpreadsheetSurvivalObservation)()
            For r As Integer = 0 To frame.RowCount - 1
                Dim timeValue As Double = 0.0R
                Dim entryValue As Double = 0.0R
                Dim status As Boolean = False
                Dim stratum As SpreadsheetCategoryValue = Nothing
                Dim missing As Boolean = False
                Dim rowBad As Boolean = False
                Dim isMissing As Boolean = False
                If Not TryNumeric(frame.Cell(r, config.TimeColumnIndex), config.CellPolicy, issues, "survival.time", timeValue, isMissing) Then rowBad = True
                missing = missing OrElse isMissing
                If config.EntryColumnIndex >= 0 Then
                    isMissing = False
                    If Not TryNumeric(frame.Cell(r, config.EntryColumnIndex), config.CellPolicy, issues, "survival.entry", entryValue, isMissing) Then rowBad = True
                    missing = missing OrElse isMissing
                End If
                isMissing = False
                If Not TryStatus(frame.Cell(r, config.StatusColumnIndex), config, issues, status, isMissing) Then rowBad = True
                missing = missing OrElse isMissing
                If config.StratumColumnIndex >= 0 Then
                    isMissing = False
                    If Not TryCategory(frame.Cell(r, config.StratumColumnIndex), config.CellPolicy, config.IgnoreCategoryCase,
                                      issues, "survival.stratum", stratum, isMissing) Then rowBad = True
                    missing = missing OrElse isMissing
                End If
                If rowBad Then Continue For
                Dim source As SpreadsheetFrameCell = frame.Cell(r, config.TimeColumnIndex)
                If missing Then
                    MissingRow(issues, source, config.Missing, "survival.missing")
                    Continue For
                End If
                If (config.RequirePositiveTime AndAlso timeValue <= 0.0R) OrElse
                   (Not config.RequirePositiveTime AndAlso timeValue < 0.0R) OrElse entryValue < 0.0R OrElse entryValue >= timeValue Then
                    ' Entry defaults to zero. With RequirePositiveTime = False, zero is only valid
                    ' without delayed entry, since interval length must be positive when entry exists.
                    If Not (config.EntryColumnIndex < 0 AndAlso Not config.RequirePositiveTime AndAlso timeValue = 0.0R) Then
                        Add(issues, source, "survival.interval", SpreadsheetValidationSeverity.[Error],
                            "Survival time/entry interval is invalid.", "time")
                        Continue For
                    End If
                End If
                Dim entry As SpreadsheetNumericObservation = Nothing
                If config.EntryColumnIndex >= 0 Then
                    entry = NumericObservation(frame.Cell(r, config.EntryColumnIndex), entryValue)
                End If
                rows.Add(New SpreadsheetSurvivalObservation(NumericObservation(source, timeValue), entry, status, stratum))
            Next
            If rows.Count < config.MinimumObservations Then
                Add(issues, Nothing, "survival.too-few", SpreadsheetValidationSeverity.[Error], "Too few valid survival rows.", "observations")
            End If
            If HasErrors(issues) Then Return Finish(Of SpreadsheetSurvivalInput)(Nothing, issues)
            Return Finish(New SpreadsheetSurvivalInput(rows), issues)
        End Function

        Public Shared Function ParseRepeatedSubjects(input As SpreadsheetRangeData,
                    Optional options As SpreadsheetRepeatedParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetRepeatedSubjectInput)
            Dim config As SpreadsheetRepeatedParseOptions = If(options, New SpreadsheetRepeatedParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            Dim frame As SpreadsheetRectangularDataFrameInput = Prepare(input, config.Header, issues)
            If frame Is Nothing Then Return Finish(Of SpreadsheetRepeatedSubjectInput)(Nothing, issues)
            Dim rows As List(Of SpreadsheetRepeatedObservation) = ReadRepeated(input, frame, config, False, issues)
            If HasErrors(issues) Then Return Finish(Of SpreadsheetRepeatedSubjectInput)(Nothing, issues)
            Return Finish(New SpreadsheetRepeatedSubjectInput(rows), issues)
        End Function

        ''' <summary>
        ''' Mixed models can omit a visit column (cross-sectional LMM). Covariates remain
        ''' source-typed until a later formula/design compiler decides their roles.
        ''' </summary>
        Public Shared Function ParseMixedModel(input As SpreadsheetRangeData,
                    Optional options As SpreadsheetMixedModelParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetMixedModelInput)
            Dim config As SpreadsheetMixedModelParseOptions = If(options, New SpreadsheetMixedModelParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            Dim frame As SpreadsheetRectangularDataFrameInput = Prepare(input, config.Header, issues)
            If frame Is Nothing Then Return Finish(Of SpreadsheetMixedModelInput)(Nothing, issues)
            Dim fixedIndices As Integer() = If(config.FixedEffectColumnIndices, Array.Empty(Of Integer)())
            Dim randomIndices As Integer() = If(config.RandomGroupColumnIndices, Array.Empty(Of Integer)())
            If Not ValidIndices(frame.ColumnCount, fixedIndices) OrElse Not ValidIndices(frame.ColumnCount, randomIndices) OrElse
               Array.IndexOf(fixedIndices, config.ResponseColumnIndex) >= 0 OrElse Array.IndexOf(randomIndices, config.ResponseColumnIndex) >= 0 Then
                Add(issues, Nothing, "mixed.columns", SpreadsheetValidationSeverity.[Error],
                    "Invalid, duplicate or response-overlapping mixed-model column roles.", "options.columns")
                Return Finish(Of SpreadsheetMixedModelInput)(Nothing, issues)
            End If
            If Not RepeatedOptionsValid(frame, config, True) Then
                Add(issues, Nothing, "repeat.options", SpreadsheetValidationSeverity.[Error],
                    "Invalid subject, visit, response or grouping roles.", "options")
                Return Finish(Of SpreadsheetMixedModelInput)(Nothing, issues)
            End If
            ' First apply fixed-effect and random-group missingness. Duplicate-visit/order
            ' checks then run only on complete rows actually retained for this model.
            Dim excluded As New HashSet(Of Integer)()
            Dim extras As New Dictionary(Of Integer, Tuple(Of List(Of SpreadsheetFrameCell), List(Of SpreadsheetCategoryValue)))()
            For r As Integer = 0 To frame.RowCount - 1
                Dim covariates As New List(Of SpreadsheetFrameCell)()
                Dim groups As New List(Of SpreadsheetCategoryValue)()
                Dim missing As Boolean = False
                Dim bad As Boolean = False
                For Each c As Integer In fixedIndices
                    Dim cell As SpreadsheetFrameCell = frame.Cell(r, c)
                    Dim isMissing As Boolean = False
                    If Not CheckCovariate(cell, config.CellPolicy, issues, isMissing) Then bad = True
                    missing = missing OrElse isMissing
                    covariates.Add(cell)
                Next
                For Each c As Integer In randomIndices
                    Dim category As SpreadsheetCategoryValue = Nothing
                    Dim isMissing As Boolean = False
                    If Not TryCategory(frame.Cell(r, c), config.CellPolicy, config.IgnoreCategoryCase,
                                      issues, "mixed.group", category, isMissing) Then bad = True
                    missing = missing OrElse isMissing
                    If category IsNot Nothing Then groups.Add(category)
                Next
                If bad Then
                    excluded.Add(r)
                    Continue For
                End If
                If missing Then
                    MissingRow(issues, frame.Cell(r, config.ResponseColumnIndex), config.Missing, "mixed.missing")
                    excluded.Add(r)
                    Continue For
                End If
                extras.Add(r, Tuple.Create(covariates, groups))
            Next
            Dim repeated As List(Of SpreadsheetRepeatedObservation) = ReadRepeated(input, frame, config, True, issues, excluded)
            If HasErrors(issues) Then Return Finish(Of SpreadsheetMixedModelInput)(Nothing, issues)
            Dim rows As New List(Of SpreadsheetMixedModelObservation)()
            For Each observation As SpreadsheetRepeatedObservation In repeated
                Dim rowIndex As Integer = observation.SelectedRow - 1 - If(config.Header = SpreadsheetHeaderMode.Required, 1, 0)
                Dim parts As Tuple(Of List(Of SpreadsheetFrameCell), List(Of SpreadsheetCategoryValue)) = extras(rowIndex)
                rows.Add(New SpreadsheetMixedModelObservation(observation, parts.Item1, parts.Item2))
            Next
            If rows.Count < config.MinimumObservations Then
                Add(issues, Nothing, "mixed.too-few", SpreadsheetValidationSeverity.[Error], "Too few complete mixed-model rows.", "rows")
            End If
            If HasErrors(issues) Then Return Finish(Of SpreadsheetMixedModelInput)(Nothing, issues)
            Dim covNames As New List(Of String)()
            For Each c As Integer In fixedIndices
                covNames.Add(frame.Columns(c).Name)
            Next
            Dim groupNames As New List(Of String)()
            For Each c As Integer In randomIndices
                groupNames.Add(frame.Columns(c).Name)
            Next
            Return Finish(New SpreadsheetMixedModelInput(rows, covNames, groupNames), issues)
        End Function

        Private Shared Function ReadRepeated(input As SpreadsheetRangeData, frame As SpreadsheetRectangularDataFrameInput,
                    config As SpreadsheetRepeatedParseOptions, allowNoVisit As Boolean,
                    issues As List(Of SpreadsheetValidationIssue),
                    Optional excludedRows As HashSet(Of Integer) = Nothing) As List(Of SpreadsheetRepeatedObservation)
            Dim rows As New List(Of SpreadsheetRepeatedObservation)()
            If Not RepeatedOptionsValid(frame, config, allowNoVisit) Then
                Add(issues, Nothing, "repeat.options", SpreadsheetValidationSeverity.[Error], "Invalid subject, visit, response or grouping roles.", "options")
                Return rows
            End If
            Dim pairKeys As New HashSet(Of String)(StringComparer.Ordinal)
            Dim subjects As New HashSet(Of String)(StringComparer.Ordinal)
            Dim lastVisit As New Dictionary(Of String, Double)(StringComparer.Ordinal)
            For r As Integer = 0 To frame.RowCount - 1
                If excludedRows IsNot Nothing AndAlso excludedRows.Contains(r) Then Continue For
                Dim subject As SpreadsheetCategoryValue = Nothing
                Dim visit As SpreadsheetCategoryValue = Nothing
                Dim treatmentGroup As SpreadsheetCategoryValue = Nothing
                Dim response As Double = 0.0R
                Dim visitNumber As Nullable(Of Double) = Nothing
                Dim visitDate As Nullable(Of DateTime) = Nothing
                Dim missing As Boolean = False
                Dim bad As Boolean = False
                Dim isMissing As Boolean = False
                If Not TryCategory(frame.Cell(r, config.SubjectColumnIndex), config.CellPolicy, config.IgnoreCategoryCase,
                                   issues, "repeat.subject", subject, isMissing) Then bad = True
                missing = missing OrElse isMissing
                If config.VisitColumnIndex >= 0 Then
                    isMissing = False
                    Dim visitCell As SpreadsheetFrameCell = frame.Cell(r, config.VisitColumnIndex)
                    If Not TryCategory(visitCell, config.CellPolicy, config.IgnoreCategoryCase,
                                       issues, "repeat.visit", visit, isMissing) Then bad = True
                    missing = missing OrElse isMissing
                    If visit IsNot Nothing Then
                        If visit.Kind = SpreadsheetCellKind.Number Then
                            visitNumber = visit.NumberValue
                        ElseIf config.RequireAscendingVisits OrElse config.InterpretVisitAsDate Then
                            Add(issues, visitCell, "repeat.visit-type", SpreadsheetValidationSeverity.[Error],
                                "Time-ordered or calendar visits must be numeric serial values.", "visit")
                            bad = True
                        End If
                        If config.InterpretVisitAsDate AndAlso visitNumber.HasValue Then
                            Try
                                Dim evaluation As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(
                                    visitCell.ToCellValue(), config.CellPolicy, input.DateSystem, True, input.DateSerialOrigin)
                                If Not evaluation.HasDateValue Then Throw New ArgumentException("Date origin was not verified.")
                                visitDate = evaluation.DateValue
                            Catch ex As ArgumentException
                                Add(issues, visitCell, "repeat.date-origin", SpreadsheetValidationSeverity.[Error], ex.Message, "visit")
                                bad = True
                            End Try
                        End If
                    End If
                End If
                isMissing = False
                Dim valueCell As SpreadsheetFrameCell = frame.Cell(r, config.ResponseColumnIndex)
                If Not TryNumeric(valueCell, config.CellPolicy, issues, "repeat.response", response, isMissing) Then bad = True
                missing = missing OrElse isMissing
                If config.GroupColumnIndex >= 0 Then
                    isMissing = False
                    If Not TryCategory(frame.Cell(r, config.GroupColumnIndex), config.CellPolicy, config.IgnoreCategoryCase,
                                       issues, "repeat.group", treatmentGroup, isMissing) Then bad = True
                    missing = missing OrElse isMissing
                End If
                If bad Then Continue For
                If missing Then
                    MissingRow(issues, valueCell, config.Missing, "repeat.missing")
                    Continue For
                End If
                If visit IsNot Nothing AndAlso config.RejectDuplicateSubjectVisits Then
                    ' Length-prefixed subject component prevents delimiter collisions.
                    Dim key As String = subject.Key.Length.ToString(CultureInfo.InvariantCulture) & ":" & subject.Key & visit.Key
                    If Not pairKeys.Add(key) Then
                        Add(issues, frame.Cell(r, config.VisitColumnIndex), "repeat.duplicate-visit", SpreadsheetValidationSeverity.[Error],
                            "Duplicate visit for the same subject.", "visit")
                        Continue For
                    End If
                End If
                If config.RequireAscendingVisits Then
                    Dim previous As Double = 0.0R
                    If lastVisit.TryGetValue(subject.Key, previous) AndAlso visitNumber.Value <= previous Then
                        Add(issues, frame.Cell(r, config.VisitColumnIndex), "repeat.visit-order", SpreadsheetValidationSeverity.[Error],
                            "Visits must increase in source-row order within each subject.", "visit")
                        Continue For
                    End If
                    lastVisit(subject.Key) = visitNumber.Value
                End If
                subjects.Add(subject.Key)
                rows.Add(New SpreadsheetRepeatedObservation(subject, visit, visitNumber, visitDate,
                            NumericObservation(valueCell, response), treatmentGroup))
            Next
            If rows.Count < config.MinimumObservations Then
                Add(issues, Nothing, "repeat.too-few", SpreadsheetValidationSeverity.[Error], "Too few complete repeated observations.", "observations")
            End If
            If subjects.Count < config.MinimumSubjects Then
                Add(issues, Nothing, "repeat.too-few-subjects", SpreadsheetValidationSeverity.[Error], "Too few distinct subject IDs.", "subjects")
            End If
            Return rows
        End Function

        Private Shared Function Prepare(input As SpreadsheetRangeData, header As SpreadsheetHeaderMode,
                                       issues As List(Of SpreadsheetValidationIssue)) As SpreadsheetRectangularDataFrameInput
            If header <> SpreadsheetHeaderMode.None AndAlso header <> SpreadsheetHeaderMode.Required Then
                Add(issues, Nothing, "header.ambiguous", SpreadsheetValidationSeverity.[Error],
                    "Longitudinal fields require an explicit Required/None header choice.", "header")
                Return Nothing
            End If
            Dim parsed As SpreadsheetTypedParseResult(Of SpreadsheetRectangularDataFrameInput) =
                RectangularCategoricalParsers.ParseDataFrame(input, New SpreadsheetDataFrameParseOptions With {.Header = header})
            For Each issue As SpreadsheetValidationIssue In parsed.Validation.Issues
                issues.Add(issue)
            Next
            If Not parsed.Success Then Return Nothing
            Return parsed.Data
        End Function

        Private Shared Function TryNumeric(cell As SpreadsheetFrameCell, policy As SpreadsheetCellPolicyOptions,
                    issues As List(Of SpreadsheetValidationIssue), path As String,
                    ByRef value As Double, ByRef missing As Boolean) As Boolean
            value = 0.0R
            missing = False
            Try
                Dim evaluation As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(cell.ToCellValue(), policy)
                If evaluation.IsError AndAlso policy.Errors <> SpreadsheetErrorPolicy.TreatAsMissing Then
                    Add(issues, cell, "cell.excel-error", SpreadsheetValidationSeverity.[Error], "Spreadsheet error is not numeric.", path)
                    Return False
                End If
                If evaluation.IsMissing Then
                    missing = True
                    Return True
                End If
                If Not evaluation.HasNumericValue Then
                    Add(issues, cell, "cell.not-numeric", SpreadsheetValidationSeverity.[Error], "Numeric value required.", path)
                    Return False
                End If
                value = evaluation.NumericValue
                Return True
            Catch ex As ArgumentException
                Add(issues, cell, "cell.invalid-value", SpreadsheetValidationSeverity.[Error], ex.Message, path)
                Return False
            End Try
        End Function

        Private Shared Function TryCategory(cell As SpreadsheetFrameCell, policy As SpreadsheetCellPolicyOptions,
                    ignoreCase As Boolean, issues As List(Of SpreadsheetValidationIssue), path As String,
                    ByRef category As SpreadsheetCategoryValue, ByRef missing As Boolean) As Boolean
            category = Nothing
            missing = False
            Try
                Dim evaluation As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(cell.ToCellValue(), policy)
                If evaluation.IsError AndAlso policy.Errors <> SpreadsheetErrorPolicy.TreatAsMissing Then
                    Add(issues, cell, "cell.excel-error", SpreadsheetValidationSeverity.[Error], "Spreadsheet error cannot identify a category.", path)
                    Return False
                End If
                If evaluation.IsMissing Then
                    missing = True
                    Return True
                End If
                category = New SpreadsheetCategoryValue(cell.ToCellValue(), ignoreCase)
                Return True
            Catch ex As ArgumentException
                Add(issues, cell, "cell.invalid-value", SpreadsheetValidationSeverity.[Error], ex.Message, path)
                Return False
            End Try
        End Function

        Private Shared Function TryStatus(cell As SpreadsheetFrameCell, config As SpreadsheetSurvivalParseOptions,
                    issues As List(Of SpreadsheetValidationIssue), ByRef isEvent As Boolean, ByRef missing As Boolean) As Boolean
            isEvent = False
            missing = False
            Try
                Dim evaluation As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(cell.ToCellValue(), config.CellPolicy)
                If evaluation.IsError AndAlso config.CellPolicy.Errors <> SpreadsheetErrorPolicy.TreatAsMissing Then
                    Add(issues, cell, "cell.excel-error", SpreadsheetValidationSeverity.[Error], "Error in survival status.", "status")
                    Return False
                End If
                If evaluation.IsMissing Then
                    missing = True
                    Return True
                End If
                If cell.Kind = SpreadsheetCellKind.Number AndAlso (cell.NumberValue = 0.0R OrElse cell.NumberValue = 1.0R) Then
                    isEvent = cell.NumberValue = 1.0R
                    Return True
                End If
                If cell.Kind = SpreadsheetCellKind.Bool AndAlso config.AllowBooleanStatus Then
                    isEvent = cell.BooleanValue
                    Return True
                End If
                If cell.Kind = SpreadsheetCellKind.Text AndAlso config.EventText IsNot Nothing Then
                    Dim compare As StringComparison = If(config.IgnoreStatusCase, StringComparison.OrdinalIgnoreCase, StringComparison.Ordinal)
                    If String.Equals(cell.TextValue, config.EventText, compare) Then
                        isEvent = True
                        Return True
                    End If
                    If String.Equals(cell.TextValue, config.CensorText, compare) Then Return True
                End If
                Add(issues, cell, "survival.status", SpreadsheetValidationSeverity.[Error],
                    "Status must be exactly 0/1, an enabled Boolean, or an explicitly configured text label.", "status")
                Return False
            Catch ex As ArgumentException
                Add(issues, cell, "cell.invalid-value", SpreadsheetValidationSeverity.[Error], ex.Message, "status")
                Return False
            End Try
        End Function

        Private Shared Function CheckCovariate(cell As SpreadsheetFrameCell, policy As SpreadsheetCellPolicyOptions,
                    issues As List(Of SpreadsheetValidationIssue), ByRef missing As Boolean) As Boolean
            missing = False
            Try
                Dim evaluation As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(cell.ToCellValue(), policy)
                If evaluation.IsError AndAlso policy.Errors <> SpreadsheetErrorPolicy.TreatAsMissing Then
                    Add(issues, cell, "cell.excel-error", SpreadsheetValidationSeverity.[Error], "Error in fixed-effect covariate.", "covariates")
                    Return False
                End If
                missing = evaluation.IsMissing
                Return True
            Catch ex As ArgumentException
                Add(issues, cell, "cell.invalid-value", SpreadsheetValidationSeverity.[Error], ex.Message, "covariates")
                Return False
            End Try
        End Function

        Private Shared Sub MissingRow(issues As List(Of SpreadsheetValidationIssue), cell As SpreadsheetFrameCell,
                    policy As SpreadsheetIncompleteObservationPolicy, code As String)
            Dim skip As Boolean = policy = SpreadsheetIncompleteObservationPolicy.Skip
            Add(issues, cell, If(skip, code & "-skipped", code),
                If(skip, SpreadsheetValidationSeverity.Warning, SpreadsheetValidationSeverity.[Error]),
                If(skip, "Incomplete source row omitted without shifting other rows.", "Incomplete source row."), "rows")
        End Sub

        Private Shared Function NumericObservation(cell As SpreadsheetFrameCell, value As Double) As SpreadsheetNumericObservation
            Return New SpreadsheetNumericObservation(value, cell.SelectedRow, cell.SelectedColumn, cell.WorksheetRow, cell.WorksheetColumn)
        End Function

        Private Shared Function ColumnsValid(width As Integer, required As Integer(), optionalIndices As Integer()) As Boolean
            Dim used As New HashSet(Of Integer)()
            For Each c As Integer In required
                If c < 0 OrElse c >= width OrElse Not used.Add(c) Then Return False
            Next
            For Each c As Integer In optionalIndices
                If c < -1 OrElse c >= width Then Return False
                If c >= 0 AndAlso Not used.Add(c) Then Return False
            Next
            Return True
        End Function
        Private Shared Function RepeatedOptionsValid(frame As SpreadsheetRectangularDataFrameInput,
                        config As SpreadsheetRepeatedParseOptions, allowNoVisit As Boolean) As Boolean
            Return config.CellPolicy IsNot Nothing AndAlso config.MinimumObservations >= 1 AndAlso
                   config.MinimumSubjects >= 1 AndAlso ValidMissing(config.Missing) AndAlso
                   ColumnsValid(frame.ColumnCount, New Integer() {config.SubjectColumnIndex, config.ResponseColumnIndex},
                                New Integer() {config.VisitColumnIndex, config.GroupColumnIndex}) AndAlso
                   (config.VisitColumnIndex >= 0 OrElse allowNoVisit) AndAlso
                   (config.VisitColumnIndex >= 0 OrElse Not (config.InterpretVisitAsDate OrElse config.RequireAscendingVisits))
        End Function
        Private Shared Function ValidIndices(width As Integer, indices As Integer()) As Boolean
            Dim seen As New HashSet(Of Integer)()
            For Each c As Integer In indices
                If c < 0 OrElse c >= width OrElse Not seen.Add(c) Then Return False
            Next
            Return True
        End Function
        Private Shared Function ValidMissing(value As SpreadsheetIncompleteObservationPolicy) As Boolean
            Return value = SpreadsheetIncompleteObservationPolicy.Reject OrElse value = SpreadsheetIncompleteObservationPolicy.Skip
        End Function
        Private Shared Function Finish(Of T As SpreadsheetAnalysisInput)(data As T,
                    issues As List(Of SpreadsheetValidationIssue)) As SpreadsheetTypedParseResult(Of T)
            Return New SpreadsheetTypedParseResult(Of T)(data, New SpreadsheetValidationReport(issues))
        End Function
        Private Shared Function HasErrors(issues As List(Of SpreadsheetValidationIssue)) As Boolean
            For Each issue As SpreadsheetValidationIssue In issues
                If issue.Severity = SpreadsheetValidationSeverity.[Error] Then Return True
            Next
            Return False
        End Function
        Private Shared Sub Add(issues As List(Of SpreadsheetValidationIssue), cell As SpreadsheetFrameCell,
                    code As String, severity As SpreadsheetValidationSeverity, message As String, path As String)
            issues.Add(New SpreadsheetValidationIssue(code, severity, message, path,
                If(cell Is Nothing, 0, cell.SelectedRow), If(cell Is Nothing, 0, cell.SelectedColumn),
                If(cell Is Nothing, 0, cell.WorksheetRow), If(cell Is Nothing, 0, cell.WorksheetColumn)))
        End Sub
    End Class
End Namespace
