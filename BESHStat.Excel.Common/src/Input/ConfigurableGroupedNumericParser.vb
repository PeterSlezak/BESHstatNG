Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon

    Public Enum SpreadsheetGroupedOrientation
        GroupsInColumns = 0
        GroupsInRows = 1
        LongFormat = 2
    End Enum

    Public Enum SpreadsheetHeaderMode
        Required = 0
        None = 1
        Auto = 2
    End Enum

    ''' <summary>
    ''' Opt-in grouped-range interpretation. In the wide-column mode, the first row supplies
    ''' group names when headers are required. In the wide-row mode, the first column supplies
    ''' names. Long format uses category and observation columns, with first-appearance order.
    ''' </summary>
    Public NotInheritable Class SpreadsheetGroupedParseOptions
        Public Property Orientation As SpreadsheetGroupedOrientation = SpreadsheetGroupedOrientation.GroupsInColumns
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
        Public Property CategoryColumnIndex As Integer = 0
        Public Property ValueColumnIndex As Integer = 1
        Public Property MinimumObservationsPerGroup As Integer = 2
        Public Property WarnOnSkippedObservations As Boolean = False
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()

        ''' <summary>The Stage 3.04 wide-columns/header/blank-only numeric interpretation.</summary>
        Public Shared Function LegacyWideHeaders() As SpreadsheetGroupedParseOptions
            Return New SpreadsheetGroupedParseOptions()
        End Function
    End Class

    Public NotInheritable Class SpreadsheetGroupedParseResult
        Public Sub New(data As GroupedNumericData, validation As SpreadsheetValidationReport)
            If validation Is Nothing Then Throw New ArgumentNullException(NameOf(validation))
            If validation.HasErrors AndAlso data IsNot Nothing Then
                Throw New ArgumentException("An invalid parse cannot contain grouped data.", NameOf(data))
            End If
            Me.Data = data
            Me.Validation = validation
        End Sub
        Public ReadOnly Property Data As GroupedNumericData
        Public ReadOnly Property Validation As SpreadsheetValidationReport
        Public ReadOnly Property Success As Boolean
            Get
                Return Validation.IsValid AndAlso Data IsNot Nothing
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Side-effect-free configurable parser. Original GroupedNumericRangeParser.Parse(input)
    ''' remains the unchanged legacy entry point. This new parser never guesses culture,
    ''' errors, or date types; all conversions are explicit via CellPolicy.
    ''' </summary>
    Public NotInheritable Class ConfigurableGroupedNumericParser
        Private Sub New()
        End Sub

        Public Shared Function Parse(input As SpreadsheetRangeData,
                                     Optional options As SpreadsheetGroupedParseOptions = Nothing) As SpreadsheetGroupedParseResult
            Dim config As SpreadsheetGroupedParseOptions = If(options, SpreadsheetGroupedParseOptions.LegacyWideHeaders())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If Not ValidateShape(input, config, issues) Then
                Return Result(Nothing, issues)
            End If

            Dim names As New List(Of String)()
            Dim groups As New List(Of List(Of Double))()
            Select Case config.Orientation
                Case SpreadsheetGroupedOrientation.GroupsInColumns
                    ParseWideColumns(input, config, names, groups, issues)
                Case SpreadsheetGroupedOrientation.GroupsInRows
                    ParseWideRows(input, config, names, groups, issues)
                Case SpreadsheetGroupedOrientation.LongFormat
                    ParseLong(input, config, names, groups, issues)
            End Select

            If names.Count < 2 Then
                Add(issues, input, "groups.too-few", SpreadsheetValidationSeverity.[Error],
                    "At least two groups are required.", "groups")
            End If
            For groupIndex As Integer = 0 To names.Count - 1
                If groups(groupIndex).Count < config.MinimumObservationsPerGroup Then
                    Add(issues, input, "groups.too-few-observations", SpreadsheetValidationSeverity.[Error],
                        "Group '" & names(groupIndex) & "' has fewer than " &
                        config.MinimumObservationsPerGroup.ToString(CultureInfo.InvariantCulture) & " numeric observations.",
                        "groups[" & groupIndex.ToString(CultureInfo.InvariantCulture) & "]")
                End If
            Next
            If HasErrors(issues) Then Return Result(Nothing, issues)

            Dim dataGroups(groups.Count - 1)() As Double
            For i As Integer = 0 To groups.Count - 1
                dataGroups(i) = groups(i).ToArray()
            Next
            Return Result(New GroupedNumericData(names.ToArray(), dataGroups), issues)
        End Function

        Private Shared Function Result(data As GroupedNumericData, issues As List(Of SpreadsheetValidationIssue)) As SpreadsheetGroupedParseResult
            Return New SpreadsheetGroupedParseResult(data, New SpreadsheetValidationReport(issues))
        End Function

        Private Shared Function HasErrors(issues As List(Of SpreadsheetValidationIssue)) As Boolean
            For Each issue As SpreadsheetValidationIssue In issues
                If issue.Severity = SpreadsheetValidationSeverity.[Error] Then Return True
            Next
            Return False
        End Function

        Private Shared Function ValidateShape(input As SpreadsheetRangeData,
                                              config As SpreadsheetGroupedParseOptions,
                                              issues As List(Of SpreadsheetValidationIssue)) As Boolean
            If input Is Nothing Then
                Add(issues, Nothing, "range.null", SpreadsheetValidationSeverity.[Error], "No spreadsheet range was supplied.", "input")
                Return False
            End If
            If input.RowCount < 1 OrElse input.ColumnCount < 1 OrElse input.Rows Is Nothing OrElse
                input.Rows.Count <> input.RowCount Then
                Add(issues, input, "range.shape", SpreadsheetValidationSeverity.[Error],
                    "The range dimensions do not match the supplied cells.", "rows")
                Return False
            End If
            For r As Integer = 0 To input.RowCount - 1
                Dim row As List(Of SpreadsheetCellValue) = input.Rows(r)
                If row Is Nothing OrElse row.Count <> input.ColumnCount Then
                    Add(issues, input, "range.not-rectangular", SpreadsheetValidationSeverity.[Error],
                        "The range must contain the same number of cells in every row.", "rows", r + 1)
                    Return False
                End If
            Next
            If Not System.Enum.IsDefined(GetType(SpreadsheetGroupedOrientation), config.Orientation) OrElse
                Not System.Enum.IsDefined(GetType(SpreadsheetHeaderMode), config.Header) OrElse
                config.MinimumObservationsPerGroup < 1 OrElse config.CellPolicy Is Nothing Then
                Add(issues, input, "options.invalid", SpreadsheetValidationSeverity.[Error],
                    "The grouped parsing options are invalid.", "options")
                Return False
            End If
            If config.Orientation = SpreadsheetGroupedOrientation.LongFormat Then
                If config.CategoryColumnIndex < 0 OrElse config.ValueColumnIndex < 0 OrElse
                    config.CategoryColumnIndex >= input.ColumnCount OrElse config.ValueColumnIndex >= input.ColumnCount OrElse
                    config.CategoryColumnIndex = config.ValueColumnIndex Then
                    Add(issues, input, "options.long-columns", SpreadsheetValidationSeverity.[Error],
                        "Long-format category and value columns must be distinct valid column indices.", "options.columns")
                    Return False
                End If
            ElseIf (config.Orientation = SpreadsheetGroupedOrientation.GroupsInColumns AndAlso input.ColumnCount < 2) OrElse
                   (config.Orientation = SpreadsheetGroupedOrientation.GroupsInRows AndAlso input.RowCount < 2) Then
                Add(issues, input, "range.too-few-groups", SpreadsheetValidationSeverity.[Error],
                    "At least two group columns or group rows are required.", "rows")
                Return False
            End If
            Return True
        End Function

        Private Shared Sub ParseWideColumns(input As SpreadsheetRangeData, config As SpreadsheetGroupedParseOptions,
                                            names As List(Of String), groups As List(Of List(Of Double)),
                                            issues As List(Of SpreadsheetValidationIssue))
            Dim headers As Boolean = HasWideHeader(input, config, True, issues)
            If HasErrors(issues) Then Return
            For c As Integer = 0 To input.ColumnCount - 1
                names.Add(If(headers, HeaderText(input.Rows(0)(c), c), AutoLabel(c)))
                groups.Add(New List(Of Double)())
                For r As Integer = If(headers, 1, 0) To input.RowCount - 1
                    AddNumeric(input, config, input.Rows(r)(c), r, c, names(c), groups(c), issues)
                Next
            Next
        End Sub

        Private Shared Sub ParseWideRows(input As SpreadsheetRangeData, config As SpreadsheetGroupedParseOptions,
                                         names As List(Of String), groups As List(Of List(Of Double)),
                                         issues As List(Of SpreadsheetValidationIssue))
            Dim headers As Boolean = HasWideHeader(input, config, False, issues)
            If HasErrors(issues) Then Return
            For r As Integer = 0 To input.RowCount - 1
                Dim label As String = If(headers, HeaderText(input.Rows(r)(0), r), AutoLabel(r))
                names.Add(label)
                Dim values As New List(Of Double)()
                For c As Integer = If(headers, 1, 0) To input.ColumnCount - 1
                    AddNumeric(input, config, input.Rows(r)(c), r, c, label, values, issues)
                Next
                groups.Add(values)
            Next
        End Sub

        Private Shared Function HasWideHeader(input As SpreadsheetRangeData, config As SpreadsheetGroupedParseOptions,
                                              columnGroups As Boolean,
                                              issues As List(Of SpreadsheetValidationIssue)) As Boolean
            If config.Header = SpreadsheetHeaderMode.Required Then Return True
            If config.Header = SpreadsheetHeaderMode.None Then Return False
            Dim textCount As Integer = 0
            Dim numericCount As Integer = 0
            Dim total As Integer = If(columnGroups, input.ColumnCount, input.RowCount)
            For n As Integer = 0 To total - 1
                Dim cell As SpreadsheetCellValue = If(columnGroups, input.Rows(0)(n), input.Rows(n)(0))
                If cell Is Nothing OrElse cell.Kind = SpreadsheetCellKind.Blank Then Continue For
                Dim ev As SpreadsheetCellEvaluation = Nothing
                Try
                    ev = SpreadsheetCellValuePolicy.Evaluate(cell, config.CellPolicy)
                Catch ex As ArgumentException
                    Add(issues, input, "header.invalid-cell", SpreadsheetValidationSeverity.[Error],
                        ex.Message, "header", If(columnGroups, 1, n + 1), If(columnGroups, n + 1, 1))
                    Return False
                End Try
                If ev.HasNumericValue AndAlso Not ev.IsMissing Then
                    numericCount += 1
                ElseIf cell.Kind = SpreadsheetCellKind.Text OrElse cell.Kind = SpreadsheetCellKind.Bool Then
                    textCount += 1
                ElseIf cell.Kind = SpreadsheetCellKind.CellError Then
                    Add(issues, input, "header.excel-error", SpreadsheetValidationSeverity.[Error],
                        "An Excel error cannot be used to infer a header.", "header",
                        If(columnGroups, 1, n + 1), If(columnGroups, n + 1, 1))
                    Return False
                End If
            Next
            If textCount > 0 AndAlso numericCount > 0 Then
                Add(issues, input, "header.ambiguous", SpreadsheetValidationSeverity.[Error],
                    "The possible header mixes names and numeric observations; choose Required or None explicitly.", "header")
                Return False
            End If
            If textCount = 0 AndAlso numericCount = 0 Then
                Add(issues, input, "header.ambiguous", SpreadsheetValidationSeverity.[Error],
                    "The possible header is empty; choose Required or None explicitly.", "header")
                Return False
            End If
            Add(issues, input, "header.inferred", SpreadsheetValidationSeverity.Information,
                If(textCount > 0, "A header was detected.", "No header was detected."), "header")
            Return textCount > 0
        End Function

        Private Shared Sub ParseLong(input As SpreadsheetRangeData, config As SpreadsheetGroupedParseOptions,
                                     names As List(Of String), groups As List(Of List(Of Double)),
                                     issues As List(Of SpreadsheetValidationIssue))
            Dim headers As Boolean = config.Header = SpreadsheetHeaderMode.Required
            If config.Header = SpreadsheetHeaderMode.Auto Then
                Dim valueCell As SpreadsheetCellValue = input.Rows(0)(config.ValueColumnIndex)
                If valueCell Is Nothing OrElse valueCell.Kind = SpreadsheetCellKind.Blank OrElse valueCell.Kind = SpreadsheetCellKind.CellError Then
                    Add(issues, input, "header.ambiguous", SpreadsheetValidationSeverity.[Error],
                        "Long-format header cannot be inferred from a blank or Excel-error value cell.", "header", 1, config.ValueColumnIndex + 1)
                    Return
                End If
                Try
                    Dim ev As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(valueCell, config.CellPolicy)
                    headers = Not ev.HasNumericValue
                Catch ex As ArgumentException
                    Add(issues, input, "header.invalid-cell", SpreadsheetValidationSeverity.[Error], ex.Message,
                        "header", 1, config.ValueColumnIndex + 1)
                    Return
                End Try
                Add(issues, input, "header.inferred", SpreadsheetValidationSeverity.Information,
                    If(headers, "A long-format header was detected.", "No long-format header was detected."), "header")
            End If

            Dim lookup As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            Dim labelKinds As New Dictionary(Of String, SpreadsheetCellKind)(StringComparer.Ordinal)
            For r As Integer = If(headers, 1, 0) To input.RowCount - 1
                Dim categoryCell As SpreadsheetCellValue = input.Rows(r)(config.CategoryColumnIndex)
                Dim valueCell As SpreadsheetCellValue = input.Rows(r)(config.ValueColumnIndex)
                If (categoryCell Is Nothing OrElse categoryCell.Kind = SpreadsheetCellKind.Blank) AndAlso
                   (valueCell Is Nothing OrElse valueCell.Kind = SpreadsheetCellKind.Blank) Then
                    If config.WarnOnSkippedObservations Then
                        Add(issues, input, "rows.blank-skipped", SpreadsheetValidationSeverity.Warning,
                            "An empty long-format row was skipped.", "rows", r + 1)
                    End If
                    Continue For
                End If
                Dim label As String = String.Empty
                Dim kind As SpreadsheetCellKind = SpreadsheetCellKind.Blank
                If categoryCell IsNot Nothing Then kind = categoryCell.Kind
                Select Case kind
                    Case SpreadsheetCellKind.Text, SpreadsheetCellKind.CellError
                        If kind = SpreadsheetCellKind.CellError Then
                            Add(issues, input, "groups.category-error", SpreadsheetValidationSeverity.[Error],
                                "An Excel error is not a valid category.", "category", r + 1, config.CategoryColumnIndex + 1)
                            Continue For
                        End If
                        label = If(categoryCell.TextValue, String.Empty).Trim()
                    Case SpreadsheetCellKind.Number
                        If Double.IsNaN(categoryCell.NumberValue) OrElse Double.IsInfinity(categoryCell.NumberValue) Then
                            Add(issues, input, "groups.category-invalid", SpreadsheetValidationSeverity.[Error],
                                "Category labels must be finite.", "category", r + 1, config.CategoryColumnIndex + 1)
                            Continue For
                        End If
                        label = categoryCell.NumberValue.ToString("G15", CultureInfo.InvariantCulture)
                    Case SpreadsheetCellKind.Bool
                        label = categoryCell.BooleanValue.ToString(CultureInfo.InvariantCulture)
                End Select
                If label.Length = 0 Then
                    Add(issues, input, "groups.category-missing", SpreadsheetValidationSeverity.[Error],
                        "A long-format observation requires a category.", "category", r + 1, config.CategoryColumnIndex + 1)
                    Continue For
                End If
                If labelKinds.ContainsKey(label) AndAlso labelKinds(label) <> kind Then
                    Add(issues, input, "groups.category-collision", SpreadsheetValidationSeverity.[Error],
                        "Distinct category cell types produce the same label: '" & label & "'.", "category", r + 1, config.CategoryColumnIndex + 1)
                    Continue For
                End If
                Dim idx As Integer = 0
                If Not lookup.TryGetValue(label, idx) Then
                    idx = names.Count
                    lookup.Add(label, idx)
                    labelKinds.Add(label, kind)
                    names.Add(label)
                    groups.Add(New List(Of Double)())
                End If
                AddNumeric(input, config, valueCell, r, config.ValueColumnIndex, label, groups(idx), issues)
            Next
        End Sub

        Private Shared Sub AddNumeric(input As SpreadsheetRangeData, config As SpreadsheetGroupedParseOptions,
                                      cell As SpreadsheetCellValue, r As Integer, c As Integer,
                                      groupName As String, values As List(Of Double),
                                      issues As List(Of SpreadsheetValidationIssue))
            Dim ev As SpreadsheetCellEvaluation
            Try
                ev = SpreadsheetCellValuePolicy.Evaluate(cell, config.CellPolicy)
            Catch ex As ArgumentException
                Add(issues, input, "cell.invalid-value", SpreadsheetValidationSeverity.[Error],
                    ex.Message, "observations", r + 1, c + 1)
                Return
            End Try
            If ev.IsError AndAlso Not ev.IsMissing Then
                Add(issues, input, "cell.excel-error", SpreadsheetValidationSeverity.[Error],
                    "Excel error in group '" & groupName & "' is not a numeric observation.", "observations", r + 1, c + 1)
            ElseIf ev.IsMissing Then
                If config.WarnOnSkippedObservations Then
                    Add(issues, input, "cell.missing-skipped", SpreadsheetValidationSeverity.Warning,
                        "A missing observation was skipped in group '" & groupName & "'.", "observations", r + 1, c + 1)
                End If
            ElseIf Not ev.HasNumericValue Then
                Add(issues, input, "cell.not-numeric", SpreadsheetValidationSeverity.[Error],
                    "Non-numeric value in group '" & groupName & "'.", "observations", r + 1, c + 1)
            Else
                values.Add(ev.NumericValue)
            End If
        End Sub

        Private Shared Function HeaderText(cell As SpreadsheetCellValue, index As Integer) As String
            If cell IsNot Nothing Then
                Select Case cell.Kind
                    Case SpreadsheetCellKind.Text, SpreadsheetCellKind.CellError
                        Dim label As String = If(cell.TextValue, String.Empty).Trim()
                        If label.Length > 0 Then Return label
                    Case SpreadsheetCellKind.Number
                        Return cell.NumberValue.ToString("G15", CultureInfo.InvariantCulture)
                    Case SpreadsheetCellKind.Bool
                        Return cell.BooleanValue.ToString(CultureInfo.InvariantCulture)
                End Select
            End If
            Return AutoLabel(index)
        End Function

        Private Shared Function AutoLabel(index As Integer) As String
            Return "Group " & (index + 1).ToString(CultureInfo.InvariantCulture)
        End Function

        Private Shared Sub Add(issues As List(Of SpreadsheetValidationIssue), input As SpreadsheetRangeData,
                               code As String, severity As SpreadsheetValidationSeverity,
                               message As String, fieldPath As String,
                               Optional selectedRow As Integer = 0, Optional selectedColumn As Integer = 0)
            Dim absoluteRow As Integer = 0
            Dim absoluteColumn As Integer = 0
            If input IsNot Nothing Then
                If selectedRow > 0 AndAlso input.StartRow > 0 Then absoluteRow = input.StartRow + selectedRow - 1
                If selectedColumn > 0 AndAlso input.StartColumn > 0 Then absoluteColumn = input.StartColumn + selectedColumn - 1
            End If
            issues.Add(New SpreadsheetValidationIssue(code, severity, message, fieldPath,
                                                       selectedRow, selectedColumn, absoluteRow, absoluteColumn))
        End Sub
    End Class
End Namespace
