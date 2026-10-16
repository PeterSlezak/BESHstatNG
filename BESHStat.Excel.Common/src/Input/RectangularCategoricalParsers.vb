Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon

    ''' <summary>Nonthrowing, diagnostics-first typed table and category parsers.</summary>
    Public NotInheritable Class RectangularCategoricalParsers
        Private Sub New()
        End Sub

        Public Shared Function ParseDataFrame(input As SpreadsheetRangeData,
                Optional options As SpreadsheetDataFrameParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetRectangularDataFrameInput)
            Dim config As SpreadsheetDataFrameParseOptions = If(options, New SpreadsheetDataFrameParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If Not ValidRange(input, issues) Then Return Finish(Of SpreadsheetRectangularDataFrameInput)(Nothing, issues)
            If Not [Enum].IsDefined(GetType(SpreadsheetHeaderMode), config.Header) Then
                Add(issues, input, "options.invalid", SpreadsheetValidationSeverity.[Error], "Invalid header mode.", "options.header")
                Return Finish(Of SpreadsheetRectangularDataFrameInput)(Nothing, issues)
            End If
            Dim hasHeader As Boolean = config.Header = SpreadsheetHeaderMode.Required
            If config.Header = SpreadsheetHeaderMode.Auto Then
                Dim firstAllText As Boolean = True
                Dim firstAllNumeric As Boolean = True
                For c As Integer = 0 To input.ColumnCount - 1
                    Dim cell As SpreadsheetCellValue = input.Rows(0)(c)
                    firstAllText = firstAllText AndAlso cell IsNot Nothing AndAlso cell.Kind = SpreadsheetCellKind.Text AndAlso Not String.IsNullOrWhiteSpace(cell.TextValue)
                    firstAllNumeric = firstAllNumeric AndAlso cell IsNot Nothing AndAlso cell.Kind = SpreadsheetCellKind.Number
                Next
                Dim secondAllNontext As Boolean = input.RowCount > 1
                If secondAllNontext Then
                    For c As Integer = 0 To input.ColumnCount - 1
                        Dim cell As SpreadsheetCellValue = input.Rows(1)(c)
                        If cell Is Nothing OrElse cell.Kind <> SpreadsheetCellKind.Number AndAlso cell.Kind <> SpreadsheetCellKind.Bool Then
                            secondAllNontext = False
                        End If
                    Next
                End If
                If firstAllText AndAlso secondAllNontext Then
                    hasHeader = True
                ElseIf firstAllNumeric Then
                    hasHeader = False
                Else
                    Add(issues, input, "header.ambiguous", SpreadsheetValidationSeverity.[Error],
                        "Cannot safely infer column headings; select Required or None.", "header")
                    Return Finish(Of SpreadsheetRectangularDataFrameInput)(Nothing, issues)
                End If
                Add(issues, input, "header.inferred", SpreadsheetValidationSeverity.Information,
                    If(hasHeader, "Column headings detected.", "No column headings detected."), "header")
            End If
            If hasHeader AndAlso input.RowCount < 2 Then
                Add(issues, input, "frame.no-data", SpreadsheetValidationSeverity.[Error], "A header requires at least one data row.", "rows")
                Return Finish(Of SpreadsheetRectangularDataFrameInput)(Nothing, issues)
            End If
            Dim columns As New List(Of SpreadsheetFrameColumn)()
            Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For c As Integer = 0 To input.ColumnCount - 1
                Dim name As String = "Column " & (c + 1).ToString(CultureInfo.InvariantCulture)
                If hasHeader Then
                    Dim headerCell As SpreadsheetCellValue = input.Rows(0)(c)
                    If headerCell Is Nothing OrElse headerCell.Kind <> SpreadsheetCellKind.Text OrElse String.IsNullOrWhiteSpace(headerCell.TextValue) Then
                        Add(issues, input, "header.invalid", SpreadsheetValidationSeverity.[Error],
                            "A column heading must be nonempty text.", "columns", 1, c + 1)
                        Continue For
                    End If
                    name = headerCell.TextValue.Trim()
                End If
                If config.RejectDuplicateNames AndAlso Not names.Add(name) Then
                    Add(issues, input, "header.duplicate", SpreadsheetValidationSeverity.[Error],
                        "Duplicate column heading: " & name, "columns", If(hasHeader, 1, 0), c + 1)
                End If
                columns.Add(New SpreadsheetFrameColumn(name, c + 1, AbsoluteColumn(input, c + 1)))
            Next
            If HasErrors(issues) Then Return Finish(Of SpreadsheetRectangularDataFrameInput)(Nothing, issues)
            Dim rows As New List(Of IReadOnlyList(Of SpreadsheetFrameCell))()
            For r As Integer = If(hasHeader, 1, 0) To input.RowCount - 1
                Dim cells As New List(Of SpreadsheetFrameCell)()
                For c As Integer = 0 To input.ColumnCount - 1
                    Try
                        cells.Add(New SpreadsheetFrameCell(input.Rows(r)(c), r + 1, c + 1,
                            AbsoluteRow(input, r + 1), AbsoluteColumn(input, c + 1)))
                    Catch ex As ArgumentException
                        Add(issues, input, "cell.invalid-value", SpreadsheetValidationSeverity.[Error], ex.Message, "rows", r + 1, c + 1)
                    End Try
                Next
                If cells.Count = input.ColumnCount Then rows.Add(cells.AsReadOnly())
            Next
            If HasErrors(issues) Then Return Finish(Of SpreadsheetRectangularDataFrameInput)(Nothing, issues)
            Return Finish(New SpreadsheetRectangularDataFrameInput(columns, rows, input.WorkbookName, input.WorksheetName, input.WorksheetId,
                input.WorkbookId, input.Address, input.SelectionId, input.SelectionRevision, input.DateSystem), issues)
        End Function

        Public Shared Function ParseCategoricalVector(input As SpreadsheetRangeData,
                 Optional options As SpreadsheetCategoricalVectorParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetCategoricalVectorInput)
            Dim config As SpreadsheetCategoricalVectorParseOptions = If(options, New SpreadsheetCategoricalVectorParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If Not ValidRange(input, issues) Then Return Finish(Of SpreadsheetCategoricalVectorInput)(Nothing, issues)
            If config.MinimumObservations < 1 OrElse config.CellPolicy Is Nothing OrElse
               Not [Enum].IsDefined(GetType(SpreadsheetVectorOrientation), config.Orientation) OrElse
               Not [Enum].IsDefined(GetType(SpreadsheetHeaderMode), config.Header) OrElse
               Not [Enum].IsDefined(GetType(SpreadsheetIncompleteObservationPolicy), config.Missing) Then
                Add(issues, input, "options.invalid", SpreadsheetValidationSeverity.[Error], "Invalid category vector options.", "options")
                Return Finish(Of SpreadsheetCategoricalVectorInput)(Nothing, issues)
            End If
            Dim orientation As SpreadsheetVectorOrientation = config.Orientation
            If orientation = SpreadsheetVectorOrientation.Auto Then
                If input.ColumnCount = 1 Then
                    orientation = SpreadsheetVectorOrientation.Column
                ElseIf input.RowCount = 1 Then
                    orientation = SpreadsheetVectorOrientation.Row
                Else
                    Add(issues, input, "vector.orientation-ambiguous", SpreadsheetValidationSeverity.[Error],
                        "Select category vector orientation explicitly.", "options.orientation")
                    Return Finish(Of SpreadsheetCategoricalVectorInput)(Nothing, issues)
                End If
            End If
            Dim column As Integer = config.ColumnIndex
            Dim row As Integer = config.RowIndex
            If (orientation = SpreadsheetVectorOrientation.Column AndAlso (column < 0 OrElse column >= input.ColumnCount)) OrElse
               (orientation = SpreadsheetVectorOrientation.Row AndAlso (row < 0 OrElse row >= input.RowCount)) Then
                Add(issues, input, "vector.index", SpreadsheetValidationSeverity.[Error], "Category vector index is outside the range.", "options.index")
                Return Finish(Of SpreadsheetCategoricalVectorInput)(Nothing, issues)
            End If
            If config.Header = SpreadsheetHeaderMode.Auto Then
                ' Both headings and ordinary category observations can be text: no safe inference.
                Add(issues, input, "header.ambiguous", SpreadsheetValidationSeverity.[Error],
                    "Category headings cannot be inferred from text categories; select Required or None.", "header")
                Return Finish(Of SpreadsheetCategoricalVectorInput)(Nothing, issues)
            End If
            Dim heading As Boolean = config.Header = SpreadsheetHeaderMode.Required
            Dim name As String = "Category"
            Dim length As Integer = If(orientation = SpreadsheetVectorOrientation.Column, input.RowCount, input.ColumnCount)
            If heading Then
                Dim headerCell As SpreadsheetCellValue = If(orientation = SpreadsheetVectorOrientation.Column, input.Rows(0)(column), input.Rows(row)(0))
                If headerCell Is Nothing OrElse headerCell.Kind <> SpreadsheetCellKind.Text OrElse String.IsNullOrWhiteSpace(headerCell.TextValue) Then
                    Add(issues, input, "header.invalid", SpreadsheetValidationSeverity.[Error], "Category heading must be text.", "header",
                        If(orientation = SpreadsheetVectorOrientation.Column, 1, row + 1), If(orientation = SpreadsheetVectorOrientation.Column, column + 1, 1))
                    Return Finish(Of SpreadsheetCategoricalVectorInput)(Nothing, issues)
                End If
                name = headerCell.TextValue.Trim()
            End If
            Dim observations As New List(Of SpreadsheetCategoricalObservation)()
            Dim levels As New List(Of SpreadsheetCategoryValue)()
            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
            For i As Integer = If(heading, 1, 0) To length - 1
                Dim r As Integer = If(orientation = SpreadsheetVectorOrientation.Column, i, row)
                Dim c As Integer = If(orientation = SpreadsheetVectorOrientation.Column, column, i)
                Dim level As SpreadsheetCategoryValue = Nothing
                Dim missing As Boolean = False
                If Not ReadCategory(input, r, c, config.CellPolicy, config.IgnoreTextCase, issues, level, missing) Then Continue For
                If missing Then
                    If config.Missing = SpreadsheetIncompleteObservationPolicy.Reject Then
                        Add(issues, input, "category.missing", SpreadsheetValidationSeverity.[Error], "A category observation is missing.", "observations", r + 1, c + 1)
                    Else
                        Add(issues, input, "category.missing-skipped", SpreadsheetValidationSeverity.Warning, "Missing category observation skipped.", "observations", r + 1, c + 1)
                    End If
                    Continue For
                End If
                observations.Add(New SpreadsheetCategoricalObservation(level, r + 1, c + 1,
                    AbsoluteRow(input, r + 1), AbsoluteColumn(input, c + 1)))
                If seen.Add(level.Key) Then levels.Add(level)
            Next
            If observations.Count < config.MinimumObservations Then
                Add(issues, input, "category.too-few", SpreadsheetValidationSeverity.[Error], "Too few category observations.", "observations")
            End If
            If HasErrors(issues) Then Return Finish(Of SpreadsheetCategoricalVectorInput)(Nothing, issues)
            Return Finish(New SpreadsheetCategoricalVectorInput(name, observations, levels), issues)
        End Function

        ''' <summary>Parse an already aggregated count matrix with optional row and column labels.</summary>
        Public Shared Function ParseContingencyCounts(input As SpreadsheetRangeData,
              Optional options As SpreadsheetContingencyCountParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetContingencyTableInput)
            Dim config As SpreadsheetContingencyCountParseOptions = If(options, New SpreadsheetContingencyCountParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If Not ValidRange(input, issues) Then Return Finish(Of SpreadsheetContingencyTableInput)(Nothing, issues)
            If config.CellPolicy Is Nothing OrElse config.MinimumRows < 1 OrElse config.MinimumColumns < 1 Then
                Add(issues, input, "options.invalid", SpreadsheetValidationSeverity.[Error], "Invalid count-table options.", "options")
                Return Finish(Of SpreadsheetContingencyTableInput)(Nothing, issues)
            End If
            Dim firstRow As Integer = If(config.HasColumnHeaders, 1, 0)
            Dim firstColumn As Integer = If(config.HasRowLabels, 1, 0)
            Dim numberRows As Integer = input.RowCount - firstRow
            Dim numberColumns As Integer = input.ColumnCount - firstColumn
            If numberRows < config.MinimumRows OrElse numberColumns < config.MinimumColumns Then
                Add(issues, input, "contingency.shape", SpreadsheetValidationSeverity.[Error], "Count table has too few category rows or columns.", "range")
                Return Finish(Of SpreadsheetContingencyTableInput)(Nothing, issues)
            End If
            Dim rowLevels As New List(Of SpreadsheetCategoryValue)()
            Dim columnLevels As New List(Of SpreadsheetCategoryValue)()
            Dim seenRow As New HashSet(Of String)(StringComparer.Ordinal)
            Dim seenCol As New HashSet(Of String)(StringComparer.Ordinal)
            For c As Integer = firstColumn To input.ColumnCount - 1
                Dim level As SpreadsheetCategoryValue = Nothing
                If config.HasColumnHeaders Then
                    Dim ignoredMissing As Boolean = False
                    ReadCategory(input, 0, c, config.CellPolicy, config.IgnoreTextCase, issues, level, ignoredMissing)
                    If ignoredMissing Then Add(issues, input, "category.missing", SpreadsheetValidationSeverity.[Error], "Column category label is missing.", "columns", 1, c + 1)
                Else
                    level = New SpreadsheetCategoryValue(SpreadsheetCellValue.TextCell("Column " & (c - firstColumn + 1).ToString(CultureInfo.InvariantCulture)))
                End If
                If level IsNot Nothing Then
                    If Not seenCol.Add(level.Key) Then Add(issues, input, "category.duplicate", SpreadsheetValidationSeverity.[Error], "Duplicate column category.", "columns", 1, c + 1)
                    columnLevels.Add(level)
                End If
            Next
            For r As Integer = firstRow To input.RowCount - 1
                Dim level As SpreadsheetCategoryValue = Nothing
                If config.HasRowLabels Then
                    Dim ignoredMissing As Boolean = False
                    ReadCategory(input, r, 0, config.CellPolicy, config.IgnoreTextCase, issues, level, ignoredMissing)
                    If ignoredMissing Then Add(issues, input, "category.missing", SpreadsheetValidationSeverity.[Error], "Row category label is missing.", "rows", r + 1, 1)
                Else
                    level = New SpreadsheetCategoryValue(SpreadsheetCellValue.TextCell("Row " & (r - firstRow + 1).ToString(CultureInfo.InvariantCulture)))
                End If
                If level IsNot Nothing Then
                    If Not seenRow.Add(level.Key) Then Add(issues, input, "category.duplicate", SpreadsheetValidationSeverity.[Error], "Duplicate row category.", "rows", r + 1, 1)
                    rowLevels.Add(level)
                End If
            Next
            Dim matrix(numberRows - 1)() As Long
            Dim grandTotal As Long = 0
            For r As Integer = firstRow To input.RowCount - 1
                Dim countRow(numberColumns - 1) As Long
                matrix(r - firstRow) = countRow
                For c As Integer = firstColumn To input.ColumnCount - 1
                    Dim numeric As SpreadsheetCellEvaluation
                    Try
                        numeric = SpreadsheetCellValuePolicy.Evaluate(input.Rows(r)(c), config.CellPolicy)
                    Catch ex As ArgumentException
                        Add(issues, input, "cell.invalid-value", SpreadsheetValidationSeverity.[Error], ex.Message, "counts", r + 1, c + 1)
                        Continue For
                    End Try
                    If numeric.IsMissing Then
                        If config.TreatMissingAsZero Then
                            Add(issues, input, "contingency.missing-zero", SpreadsheetValidationSeverity.Warning, "Missing count interpreted as zero.", "counts", r + 1, c + 1)
                        Else
                            Add(issues, input, "contingency.missing", SpreadsheetValidationSeverity.[Error], "Missing count cannot be assumed zero.", "counts", r + 1, c + 1)
                        End If
                        Continue For
                    End If
                    If numeric.IsError Then
                        Add(issues, input, "cell.excel-error", SpreadsheetValidationSeverity.[Error], "Excel error in count matrix.", "counts", r + 1, c + 1)
                        Continue For
                    End If
                    Dim n As Double = numeric.NumericValue
                    ' Excel numeric precision is limited: do not accept integers above 2^53-1.
                    If Not numeric.HasNumericValue OrElse n < 0.0R OrElse n <> Math.Truncate(n) OrElse n > 9007199254740991.0R Then
                        Add(issues, input, "contingency.invalid-count", SpreadsheetValidationSeverity.[Error], "Counts must be nonnegative exact integers.", "counts", r + 1, c + 1)
                        Continue For
                    End If
                    Dim count As Long = CLng(n)
                    If grandTotal > Long.MaxValue - count Then
                        Add(issues, input, "contingency.overflow", SpreadsheetValidationSeverity.[Error], "Count total exceeds Int64 capacity.", "counts", r + 1, c + 1)
                        Continue For
                    End If
                    matrix(r - firstRow)(c - firstColumn) = count
                    grandTotal += count
                Next
            Next
            If grandTotal = 0 Then Add(issues, input, "contingency.empty", SpreadsheetValidationSeverity.[Error], "Count table has zero observations.", "counts")
            If HasErrors(issues) Then Return Finish(Of SpreadsheetContingencyTableInput)(Nothing, issues)
            Return Finish(New SpreadsheetContingencyTableInput(rowLevels, columnLevels, matrix), issues)
        End Function

        ''' <summary>Aggregate aligned individual categorical observations into a first-appearance-ordered cross-tab.</summary>
        Public Shared Function ParseContingencyPairs(input As SpreadsheetRangeData,
              Optional options As SpreadsheetContingencyPairsParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetContingencyTableInput)
            Dim config As SpreadsheetContingencyPairsParseOptions = If(options, New SpreadsheetContingencyPairsParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If Not ValidRange(input, issues) Then Return Finish(Of SpreadsheetContingencyTableInput)(Nothing, issues)
            If config.CellPolicy Is Nothing OrElse config.MinimumRows < 1 OrElse config.MinimumColumns < 1 OrElse
               Not [Enum].IsDefined(GetType(SpreadsheetHeaderMode), config.Header) OrElse
               Not [Enum].IsDefined(GetType(SpreadsheetIncompleteObservationPolicy), config.Missing) OrElse
               config.FirstColumnIndex < 0 OrElse config.SecondColumnIndex < 0 OrElse
               config.FirstColumnIndex >= input.ColumnCount OrElse config.SecondColumnIndex >= input.ColumnCount OrElse
               config.FirstColumnIndex = config.SecondColumnIndex Then
                Add(issues, input, "options.invalid", SpreadsheetValidationSeverity.[Error], "Invalid categorical pair columns or options.", "options")
                Return Finish(Of SpreadsheetContingencyTableInput)(Nothing, issues)
            End If
            If config.Header = SpreadsheetHeaderMode.Auto Then
                Add(issues, input, "header.ambiguous", SpreadsheetValidationSeverity.[Error], "Categorical headers cannot safely be inferred.", "header")
                Return Finish(Of SpreadsheetContingencyTableInput)(Nothing, issues)
            End If
            If config.Header = SpreadsheetHeaderMode.Required Then
                For Each c As Integer In New Integer() {config.FirstColumnIndex, config.SecondColumnIndex}
                    Dim cell As SpreadsheetCellValue = input.Rows(0)(c)
                    If cell Is Nothing OrElse cell.Kind <> SpreadsheetCellKind.Text OrElse String.IsNullOrWhiteSpace(cell.TextValue) Then
                        Add(issues, input, "header.invalid", SpreadsheetValidationSeverity.[Error], "Categorical pair header must be text.", "header", 1, c + 1)
                    End If
                Next
                If HasErrors(issues) Then Return Finish(Of SpreadsheetContingencyTableInput)(Nothing, issues)
            End If
            Dim firstLevels As New List(Of SpreadsheetCategoryValue)()
            Dim secondLevels As New List(Of SpreadsheetCategoryValue)()
            Dim firstIndices As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            Dim secondIndices As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            Dim pairs As New List(Of Tuple(Of Integer, Integer))()
            For r As Integer = If(config.Header = SpreadsheetHeaderMode.Required, 1, 0) To input.RowCount - 1
                Dim first As SpreadsheetCategoryValue = Nothing
                Dim second As SpreadsheetCategoryValue = Nothing
                Dim firstMissing As Boolean = False
                Dim secondMissing As Boolean = False
                Dim firstOk As Boolean = ReadCategory(input, r, config.FirstColumnIndex, config.CellPolicy,
                                                     config.IgnoreTextCase, issues, first, firstMissing)
                Dim secondOk As Boolean = ReadCategory(input, r, config.SecondColumnIndex, config.CellPolicy,
                                                      config.IgnoreTextCase, issues, second, secondMissing)
                If Not firstOk OrElse Not secondOk Then Continue For
                If firstMissing OrElse secondMissing Then
                    If config.Missing = SpreadsheetIncompleteObservationPolicy.Reject Then
                        Add(issues, input, "contingency.incomplete", SpreadsheetValidationSeverity.[Error], "Incomplete categorical pair.", "pairs", r + 1)
                    Else
                        Add(issues, input, "contingency.incomplete-skipped", SpreadsheetValidationSeverity.Warning, "Incomplete categorical pair skipped.", "pairs", r + 1)
                    End If
                    Continue For
                End If
                Dim a As Integer = 0
                Dim b As Integer = 0
                If Not firstIndices.TryGetValue(first.Key, a) Then
                    a = firstLevels.Count
                    firstIndices.Add(first.Key, a)
                    firstLevels.Add(first)
                End If
                If Not secondIndices.TryGetValue(second.Key, b) Then
                    b = secondLevels.Count
                    secondIndices.Add(second.Key, b)
                    secondLevels.Add(second)
                End If
                pairs.Add(Tuple.Create(a, b))
            Next
            If firstLevels.Count < config.MinimumRows OrElse secondLevels.Count < config.MinimumColumns Then
                Add(issues, input, "contingency.shape", SpreadsheetValidationSeverity.[Error], "Not enough distinct categories for a contingency table.", "pairs")
            End If
            If HasErrors(issues) Then Return Finish(Of SpreadsheetContingencyTableInput)(Nothing, issues)
            Dim matrix(firstLevels.Count - 1)() As Long
            For r As Integer = 0 To matrix.Length - 1
                Dim countRow(secondLevels.Count - 1) As Long
                matrix(r) = countRow
            Next
            For Each pair As Tuple(Of Integer, Integer) In pairs
                matrix(pair.Item1)(pair.Item2) += 1
            Next
            Return Finish(New SpreadsheetContingencyTableInput(firstLevels, secondLevels, matrix), issues)
        End Function

        Private Shared Function ReadCategory(input As SpreadsheetRangeData, r As Integer, c As Integer,
                    policy As SpreadsheetCellPolicyOptions, ignoreTextCase As Boolean,
                    issues As List(Of SpreadsheetValidationIssue), ByRef category As SpreadsheetCategoryValue,
                    ByRef missing As Boolean) As Boolean
            category = Nothing
            missing = False
            Dim cell As SpreadsheetCellValue = input.Rows(r)(c)
            Try
                Dim evaluated As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(cell, policy)
                If evaluated.IsError Then
                    ' Even Preserve never transforms error cells into category levels.
                    If policy.Errors = SpreadsheetErrorPolicy.TreatAsMissing Then
                        missing = True
                        Return True
                    End If
                    Add(issues, input, "cell.excel-error", SpreadsheetValidationSeverity.[Error], "Excel error cannot identify a category.", "category", r + 1, c + 1)
                    Return False
                End If
                If evaluated.IsMissing Then
                    missing = True
                    Return True
                End If
                category = New SpreadsheetCategoryValue(cell, ignoreTextCase)
                Return True
            Catch ex As ArgumentException
                Add(issues, input, "cell.invalid-value", SpreadsheetValidationSeverity.[Error], ex.Message, "category", r + 1, c + 1)
                Return False
            End Try
        End Function

        Private Shared Function ValidRange(input As SpreadsheetRangeData, issues As List(Of SpreadsheetValidationIssue)) As Boolean
            If input Is Nothing Then
                Add(issues, Nothing, "range.null", SpreadsheetValidationSeverity.[Error], "No spreadsheet range was supplied.", "input")
                Return False
            End If
            If input.RowCount < 1 OrElse input.ColumnCount < 1 OrElse input.Rows Is Nothing OrElse
               input.Rows.Count <> input.RowCount OrElse input.StartRow < 0 OrElse input.StartColumn < 0 Then
                Add(issues, input, "range.shape", SpreadsheetValidationSeverity.[Error], "Range dimensions or source offsets are invalid.", "range")
                Return False
            End If
            For r As Integer = 0 To input.RowCount - 1
                If input.Rows(r) Is Nothing OrElse input.Rows(r).Count <> input.ColumnCount Then
                    Add(issues, input, "range.not-rectangular", SpreadsheetValidationSeverity.[Error], "Nonrectangular cell rows.", "range", r + 1)
                    Return False
                End If
            Next
            Return True
        End Function

        Private Shared Function Finish(Of T As SpreadsheetAnalysisInput)(data As T,
                    issues As List(Of SpreadsheetValidationIssue)) As SpreadsheetTypedParseResult(Of T)
            Return New SpreadsheetTypedParseResult(Of T)(data, New SpreadsheetValidationReport(issues))
        End Function
        Private Shared Function HasErrors(issues As List(Of SpreadsheetValidationIssue)) As Boolean
            For Each item As SpreadsheetValidationIssue In issues
                If item.Severity = SpreadsheetValidationSeverity.[Error] Then Return True
            Next
            Return False
        End Function
        Private Shared Function AbsoluteRow(input As SpreadsheetRangeData, row As Integer) As Integer
            Return If(input.StartRow > 0, input.StartRow + row - 1, 0)
        End Function
        Private Shared Function AbsoluteColumn(input As SpreadsheetRangeData, column As Integer) As Integer
            Return If(input.StartColumn > 0, input.StartColumn + column - 1, 0)
        End Function
        Private Shared Sub Add(issues As List(Of SpreadsheetValidationIssue), input As SpreadsheetRangeData,
                code As String, severity As SpreadsheetValidationSeverity, message As String, path As String,
                Optional row As Integer = 0, Optional column As Integer = 0)
            issues.Add(New SpreadsheetValidationIssue(code, severity, message, path, row, column,
                If(input IsNot Nothing AndAlso row > 0, AbsoluteRow(input, row), 0),
                If(input IsNot Nothing AndAlso column > 0, AbsoluteColumn(input, column), 0)))
        End Sub
    End Class
End Namespace
