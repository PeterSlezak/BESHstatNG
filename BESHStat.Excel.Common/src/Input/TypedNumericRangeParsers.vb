Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon

    ''' <summary>
    ''' Host-independent scalar, vector, complete-pair, and grouped numeric input adapters.
    ''' All errors use the 01.03 validation report and preserve 01.02 cell policies.
    ''' Existing grouped parser entry points and the Stage 3.04 dispatcher are untouched.
    ''' </summary>
    Public NotInheritable Class TypedNumericRangeParsers
        Private Sub New()
        End Sub

        Public Shared Function ParseScalar(input As SpreadsheetRangeData,
                          Optional cellPolicy As SpreadsheetCellPolicyOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetScalarNumericInput)
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If Not ValidRange(input, issues) Then Return Finish(Of SpreadsheetScalarNumericInput)(Nothing, issues)
            If input.RowCount <> 1 OrElse input.ColumnCount <> 1 Then
                AddIssue(issues, input, "scalar.shape", SpreadsheetValidationSeverity.[Error],
                         "A scalar must be selected as exactly one cell.", "range")
                Return Finish(Of SpreadsheetScalarNumericInput)(Nothing, issues)
            End If
            Dim value As SpreadsheetNumericObservation = Nothing
            ReadObservation(input, 0, 0, If(cellPolicy, New SpreadsheetCellPolicyOptions()), issues, value)
            If value Is Nothing AndAlso Not AnyError(issues) Then
                AddIssue(issues, input, "scalar.missing", SpreadsheetValidationSeverity.[Error],
                         "A scalar numeric value is required.", "value", 1, 1)
            End If
            If AnyError(issues) Then Return Finish(Of SpreadsheetScalarNumericInput)(Nothing, issues)
            Return Finish(New SpreadsheetScalarNumericInput(value), issues)
        End Function

        Public Shared Function ParseVector(input As SpreadsheetRangeData,
                          Optional options As SpreadsheetVectorParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetVectorNumericInput)
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If Not ValidRange(input, issues) Then Return Finish(Of SpreadsheetVectorNumericInput)(Nothing, issues)
            Dim config As SpreadsheetVectorParseOptions = If(options, New SpreadsheetVectorParseOptions())
            If Not System.Enum.IsDefined(GetType(SpreadsheetVectorOrientation), config.Orientation) OrElse
               Not ValidOptions(config.Header, config.Missing, config.CellPolicy, config.MinimumObservations) Then
                AddIssue(issues, input, "options.invalid", SpreadsheetValidationSeverity.[Error],
                         "Vector parsing options are invalid.", "options")
                Return Finish(Of SpreadsheetVectorNumericInput)(Nothing, issues)
            End If
            Dim inColumn As Boolean
            Select Case config.Orientation
                Case SpreadsheetVectorOrientation.Auto
                    If input.ColumnCount = 1 Then
                        inColumn = True
                    ElseIf input.RowCount = 1 Then
                        inColumn = False
                    Else
                        AddIssue(issues, input, "vector.orientation-ambiguous", SpreadsheetValidationSeverity.[Error],
                                 "Choose a vector row or column for a two-dimensional range.", "options.orientation")
                        Return Finish(Of SpreadsheetVectorNumericInput)(Nothing, issues)
                    End If
                Case SpreadsheetVectorOrientation.Column
                    inColumn = True
                Case SpreadsheetVectorOrientation.Row
                    inColumn = False
            End Select
            Dim fixedIndex As Integer = If(inColumn, config.ColumnIndex, config.RowIndex)
            If fixedIndex < 0 OrElse fixedIndex >= If(inColumn, input.ColumnCount, input.RowCount) Then
                AddIssue(issues, input, "vector.index", SpreadsheetValidationSeverity.[Error],
                         "Selected vector row or column is outside the range.", "options.index")
                Return Finish(Of SpreadsheetVectorNumericInput)(Nothing, issues)
            End If

            Dim initial As SpreadsheetCellValue = If(inColumn, input.Rows(0)(fixedIndex), input.Rows(fixedIndex)(0))
            Dim firstRow As Integer = If(inColumn, 0, fixedIndex)
            Dim firstColumn As Integer = If(inColumn, fixedIndex, 0)
            Dim header As Boolean = ResolveHeader(input, initial, Nothing, config.Header, config.CellPolicy,
                                                   firstRow, firstColumn, -1, -1, issues)
            If AnyError(issues) Then Return Finish(Of SpreadsheetVectorNumericInput)(Nothing, issues)
            Dim name As String = If(header, LabelText(initial), "Vector 1")
            Dim observations As New List(Of SpreadsheetNumericObservation)()
            For i As Integer = If(header, 1, 0) To If(inColumn, input.RowCount, input.ColumnCount) - 1
                Dim r As Integer = If(inColumn, i, fixedIndex)
                Dim c As Integer = If(inColumn, fixedIndex, i)
                Dim found As SpreadsheetNumericObservation = Nothing
                Dim valid As Boolean = ReadObservation(input, r, c, config.CellPolicy, issues, found)
                If Not valid Then Continue For
                If found IsNot Nothing Then
                    observations.Add(found)
                ElseIf config.Missing = SpreadsheetIncompleteObservationPolicy.Reject Then
                    AddIssue(issues, input, "vector.missing", SpreadsheetValidationSeverity.[Error],
                             "A missing vector observation is not permitted.", "observations", r + 1, c + 1)
                ElseIf config.WarnOnSkippedObservations Then
                    AddIssue(issues, input, "vector.missing-skipped", SpreadsheetValidationSeverity.Warning,
                             "A missing vector observation was skipped.", "observations", r + 1, c + 1)
                End If
            Next
            If observations.Count < config.MinimumObservations Then
                AddIssue(issues, input, "vector.too-few", SpreadsheetValidationSeverity.[Error],
                         "The numeric vector has too few observations.", "observations")
            End If
            If AnyError(issues) Then Return Finish(Of SpreadsheetVectorNumericInput)(Nothing, issues)
            Return Finish(New SpreadsheetVectorNumericInput(name, observations), issues)
        End Function

        Public Shared Function ParsePaired(input As SpreadsheetRangeData,
                          Optional options As SpreadsheetPairedParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetPairedNumericInput)
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If Not ValidRange(input, issues) Then Return Finish(Of SpreadsheetPairedNumericInput)(Nothing, issues)
            Dim config As SpreadsheetPairedParseOptions = If(options, New SpreadsheetPairedParseOptions())
            If Not System.Enum.IsDefined(GetType(SpreadsheetPairOrientation), config.Orientation) OrElse
               Not ValidOptions(config.Header, config.Missing, config.CellPolicy, config.MinimumPairs) Then
                AddIssue(issues, input, "options.invalid", SpreadsheetValidationSeverity.[Error],
                         "Paired parsing options are invalid.", "options")
                Return Finish(Of SpreadsheetPairedNumericInput)(Nothing, issues)
            End If
            Dim columns As Boolean = config.Orientation = SpreadsheetPairOrientation.TwoColumns
            Dim available As Integer = If(columns, input.ColumnCount, input.RowCount)
            If config.FirstIndex < 0 OrElse config.SecondIndex < 0 OrElse config.FirstIndex >= available OrElse
               config.SecondIndex >= available OrElse config.FirstIndex = config.SecondIndex Then
                AddIssue(issues, input, "paired.indices", SpreadsheetValidationSeverity.[Error],
                         "The two paired series must be distinct valid rows or columns.", "options.indices")
                Return Finish(Of SpreadsheetPairedNumericInput)(Nothing, issues)
            End If

            Dim r1 As Integer = If(columns, 0, config.FirstIndex)
            Dim c1 As Integer = If(columns, config.FirstIndex, 0)
            Dim r2 As Integer = If(columns, 0, config.SecondIndex)
            Dim c2 As Integer = If(columns, config.SecondIndex, 0)
            Dim firstHeaderCell As SpreadsheetCellValue = input.Rows(r1)(c1)
            Dim secondHeaderCell As SpreadsheetCellValue = input.Rows(r2)(c2)
            Dim header As Boolean = ResolveHeader(input, firstHeaderCell, secondHeaderCell,
                                                   config.Header, config.CellPolicy, r1, c1, r2, c2, issues)
            If AnyError(issues) Then Return Finish(Of SpreadsheetPairedNumericInput)(Nothing, issues)
            Dim firstName As String = If(header, LabelText(firstHeaderCell), "Series 1")
            Dim secondName As String = If(header, LabelText(secondHeaderCell), "Series 2")
            Dim pairs As New List(Of SpreadsheetNumericPair)()
            For i As Integer = If(header, 1, 0) To If(columns, input.RowCount, input.ColumnCount) - 1
                Dim firstRow As Integer = If(columns, i, config.FirstIndex)
                Dim firstColumn As Integer = If(columns, config.FirstIndex, i)
                Dim secondRow As Integer = If(columns, i, config.SecondIndex)
                Dim secondColumn As Integer = If(columns, config.SecondIndex, i)
                Dim first As SpreadsheetNumericObservation = Nothing
                Dim second As SpreadsheetNumericObservation = Nothing
                Dim firstValid As Boolean = ReadObservation(input, firstRow, firstColumn, config.CellPolicy, issues, first)
                Dim secondValid As Boolean = ReadObservation(input, secondRow, secondColumn, config.CellPolicy, issues, second)
                If Not firstValid OrElse Not secondValid Then Continue For
                If first IsNot Nothing AndAlso second IsNot Nothing Then
                    pairs.Add(New SpreadsheetNumericPair(first, second))
                ElseIf config.Missing = SpreadsheetIncompleteObservationPolicy.Reject Then
                    AddIssue(issues, input, "paired.incomplete", SpreadsheetValidationSeverity.[Error],
                             "Both observations are required for an aligned pair.", "pairs",
                             If(first Is Nothing, firstRow + 1, secondRow + 1),
                             If(first Is Nothing, firstColumn + 1, secondColumn + 1))
                ElseIf config.WarnOnSkippedPairs Then
                    AddIssue(issues, input, "paired.incomplete-skipped", SpreadsheetValidationSeverity.Warning,
                             "A whole incomplete pair was skipped to preserve alignment.", "pairs",
                             If(first Is Nothing, firstRow + 1, secondRow + 1),
                             If(first Is Nothing, firstColumn + 1, secondColumn + 1))
                End If
            Next
            If pairs.Count < config.MinimumPairs Then
                AddIssue(issues, input, "paired.too-few", SpreadsheetValidationSeverity.[Error],
                         "Too few complete numeric pairs were supplied.", "pairs")
            End If
            If AnyError(issues) Then Return Finish(Of SpreadsheetPairedNumericInput)(Nothing, issues)
            Return Finish(New SpreadsheetPairedNumericInput(firstName, secondName, pairs), issues)
        End Function

        ''' <summary>Delegates to the 01.03 configurable grouped parser; no competing grouped algorithm.</summary>
        Public Shared Function ParseGrouped(input As SpreadsheetRangeData,
                          Optional options As SpreadsheetGroupedParseOptions = Nothing) As SpreadsheetTypedParseResult(Of GroupedNumericData)
            Dim original As SpreadsheetGroupedParseResult = ConfigurableGroupedNumericParser.Parse(input, options)
            Return New SpreadsheetTypedParseResult(Of GroupedNumericData)(original.Data, original.Validation)
        End Function

        Private Shared Function Finish(Of T As SpreadsheetAnalysisInput)(data As T,
                        issues As List(Of SpreadsheetValidationIssue)) As SpreadsheetTypedParseResult(Of T)
            Return New SpreadsheetTypedParseResult(Of T)(data, New SpreadsheetValidationReport(issues))
        End Function

        Private Shared Function ValidOptions(header As SpreadsheetHeaderMode, missing As SpreadsheetIncompleteObservationPolicy,
                           policy As SpreadsheetCellPolicyOptions, minimum As Integer) As Boolean
            Return System.Enum.IsDefined(GetType(SpreadsheetHeaderMode), header) AndAlso
                   System.Enum.IsDefined(GetType(SpreadsheetIncompleteObservationPolicy), missing) AndAlso
                   policy IsNot Nothing AndAlso minimum > 0
        End Function

        Private Shared Function ValidRange(input As SpreadsheetRangeData, issues As List(Of SpreadsheetValidationIssue)) As Boolean
            If input Is Nothing Then
                AddIssue(issues, Nothing, "range.null", SpreadsheetValidationSeverity.[Error],
                         "No spreadsheet range was supplied.", "input")
                Return False
            End If
            If input.RowCount < 1 OrElse input.ColumnCount < 1 OrElse input.Rows Is Nothing OrElse
               input.Rows.Count <> input.RowCount OrElse input.StartRow < 0 OrElse input.StartColumn < 0 Then
                AddIssue(issues, input, "range.shape", SpreadsheetValidationSeverity.[Error],
                         "The range shape or source coordinates are invalid.", "range")
                Return False
            End If
            For r As Integer = 0 To input.RowCount - 1
                If input.Rows(r) Is Nothing OrElse input.Rows(r).Count <> input.ColumnCount Then
                    AddIssue(issues, input, "range.not-rectangular", SpreadsheetValidationSeverity.[Error],
                             "All rows must match the declared range width.", "range", r + 1)
                    Return False
                End If
            Next
            Return True
        End Function

        Private Shared Function ReadObservation(input As SpreadsheetRangeData, row As Integer, column As Integer,
                             policy As SpreadsheetCellPolicyOptions,
                             issues As List(Of SpreadsheetValidationIssue),
                             ByRef observation As SpreadsheetNumericObservation) As Boolean
            observation = Nothing
            Dim cell As SpreadsheetCellValue = input.Rows(row)(column)
            Dim evaluated As SpreadsheetCellEvaluation
            Try
                evaluated = SpreadsheetCellValuePolicy.Evaluate(cell, policy)
            Catch ex As ArgumentException
                AddIssue(issues, input, "cell.invalid-value", SpreadsheetValidationSeverity.[Error],
                         ex.Message, "observations", row + 1, column + 1)
                Return False
            End Try
            If evaluated.IsMissing Then Return True
            If evaluated.IsError Then
                AddIssue(issues, input, "cell.excel-error", SpreadsheetValidationSeverity.[Error],
                         "An Excel error cannot be used as a numeric observation.", "observations", row + 1, column + 1)
                Return False
            End If
            If Not evaluated.HasNumericValue Then
                AddIssue(issues, input, "cell.not-numeric", SpreadsheetValidationSeverity.[Error],
                         "This cell is not numeric under the selected conversion policy.", "observations", row + 1, column + 1)
                Return False
            End If
            observation = New SpreadsheetNumericObservation(evaluated.NumericValue, row + 1, column + 1,
                If(input.StartRow > 0, input.StartRow + row, 0),
                If(input.StartColumn > 0, input.StartColumn + column, 0))
            Return True
        End Function

        Private Shared Function ResolveHeader(input As SpreadsheetRangeData, first As SpreadsheetCellValue,
                  second As SpreadsheetCellValue, mode As SpreadsheetHeaderMode,
                  policy As SpreadsheetCellPolicyOptions, firstRow As Integer, firstColumn As Integer,
                  secondRow As Integer, secondColumn As Integer,
                  issues As List(Of SpreadsheetValidationIssue)) As Boolean
            If mode = SpreadsheetHeaderMode.None Then Return False
            If mode = SpreadsheetHeaderMode.Required Then Return True
            Dim firstNumber As Boolean = False
            Dim firstText As Boolean = HeaderIsLabel(first, policy, firstNumber)
            If secondRow < 0 Then
                If firstText Then
                    AddIssue(issues, input, "header.inferred", SpreadsheetValidationSeverity.Information,
                             "A vector header was detected.", "header", firstRow + 1, firstColumn + 1)
                    Return True
                End If
                If firstNumber Then
                    AddIssue(issues, input, "header.inferred", SpreadsheetValidationSeverity.Information,
                             "No vector header was detected.", "header", firstRow + 1, firstColumn + 1)
                    Return False
                End If
            Else
                Dim secondNumber As Boolean = False
                Dim secondText As Boolean = HeaderIsLabel(second, policy, secondNumber)
                If firstText AndAlso secondText Then
                    AddIssue(issues, input, "header.inferred", SpreadsheetValidationSeverity.Information,
                             "Paired-series headers were detected.", "header")
                    Return True
                End If
                If firstNumber AndAlso secondNumber Then
                    AddIssue(issues, input, "header.inferred", SpreadsheetValidationSeverity.Information,
                             "No paired-series headers were detected.", "header")
                    Return False
                End If
            End If
            AddIssue(issues, input, "header.ambiguous", SpreadsheetValidationSeverity.[Error],
                     "Header inference is ambiguous. Choose Required or None explicitly.", "header",
                     firstRow + 1, firstColumn + 1)
            Return False
        End Function

        Private Shared Function HeaderIsLabel(cell As SpreadsheetCellValue, policy As SpreadsheetCellPolicyOptions,
                                             ByRef numeric As Boolean) As Boolean
            numeric = False
            If cell Is Nothing OrElse cell.Kind = SpreadsheetCellKind.Blank OrElse
               cell.Kind = SpreadsheetCellKind.CellError Then Return False
            Try
                Dim ev As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(cell, policy)
                If ev.IsMissing Then Return False
                numeric = ev.HasNumericValue
                If numeric Then Return False
            Catch ex As ArgumentException
                Return False
            End Try
            Return cell.Kind = SpreadsheetCellKind.Text AndAlso Not String.IsNullOrWhiteSpace(cell.TextValue)
        End Function

        Private Shared Function LabelText(cell As SpreadsheetCellValue) As String
            If cell Is Nothing Then Return String.Empty
            Select Case cell.Kind
                Case SpreadsheetCellKind.Text
                    Return If(cell.TextValue, String.Empty).Trim()
                Case SpreadsheetCellKind.Number
                    Return cell.NumberValue.ToString("G15", CultureInfo.InvariantCulture)
                Case SpreadsheetCellKind.Bool
                    Return cell.BooleanValue.ToString(CultureInfo.InvariantCulture)
            End Select
            Return String.Empty
        End Function

        Private Shared Function AnyError(issues As List(Of SpreadsheetValidationIssue)) As Boolean
            For Each issue As SpreadsheetValidationIssue In issues
                If issue.Severity = SpreadsheetValidationSeverity.[Error] Then Return True
            Next
            Return False
        End Function

        Private Shared Sub AddIssue(issues As List(Of SpreadsheetValidationIssue), input As SpreadsheetRangeData,
                  code As String, severity As SpreadsheetValidationSeverity, message As String, fieldPath As String,
                  Optional row As Integer = 0, Optional column As Integer = 0)
            Dim absoluteRow As Integer = 0
            Dim absoluteColumn As Integer = 0
            If input IsNot Nothing Then
                If row > 0 AndAlso input.StartRow > 0 Then absoluteRow = input.StartRow + row - 1
                If column > 0 AndAlso input.StartColumn > 0 Then absoluteColumn = input.StartColumn + column - 1
            End If
            issues.Add(New SpreadsheetValidationIssue(code, severity, message, fieldPath,
                                                       row, column, absoluteRow, absoluteColumn))
        End Sub
    End Class
End Namespace
