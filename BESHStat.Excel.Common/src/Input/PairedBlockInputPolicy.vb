Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Immutable complete blocks. Each cell carries its own original position.</summary>
    Public NotInheritable Class SpreadsheetNumericBlocks
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly _names As IReadOnlyList(Of String)
        Private ReadOnly _blocks As IReadOnlyList(Of IReadOnlyList(Of SpreadsheetNumericObservation))
        Public Sub New(names As IEnumerable(Of String), blocks As IEnumerable(Of IEnumerable(Of SpreadsheetNumericObservation)))
            _names = Array.AsReadOnly(names.ToArray())
            Dim copied As New List(Of IReadOnlyList(Of SpreadsheetNumericObservation))
            For Each block In blocks
                Dim cells = block.ToArray()
                If cells.Length <> _names.Count OrElse cells.Any(Function(c) c Is Nothing) Then Throw New ArgumentException("Invalid block.")
                copied.Add(Array.AsReadOnly(cells))
            Next
            _blocks = copied.AsReadOnly()
        End Sub
        Public ReadOnly Property Names As IReadOnlyList(Of String)
            Get
                Return _names
            End Get
        End Property
        Public ReadOnly Property Blocks As IReadOnlyList(Of IReadOnlyList(Of SpreadsheetNumericObservation))
            Get
                Return _blocks
            End Get
        End Property
        Public ReadOnly Property Matrix As Double(,)
            Get
                Dim values(_blocks.Count - 1, _names.Count - 1) As Double
                For r = 0 To _blocks.Count - 1
                    For c = 0 To _names.Count - 1
                        values(r, c) = _blocks(r)(c).Value
                    Next
                Next
                Return values
            End Get
        End Property
    End Class

    Public NotInheritable Class SpreadsheetBlockParseOptions
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.None
        Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
        Public Property MinimumBlocks As Integer = 1
        Public Property MinimumTreatments As Integer = 3
        Public Property RequireUniqueNames As Boolean
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
    End Class

    ''' <summary>Original selected-row indices, ordered by the first selection; never an ordinal zip.</summary>
    Public NotInheritable Class SpreadsheetRowAlignment
        Inherits SpreadsheetAnalysisInput
        Public Sub New(first As IEnumerable(Of Integer), second As IEnumerable(Of Integer))
            FirstRows = Array.AsReadOnly(first.ToArray())
            SecondRows = Array.AsReadOnly(second.ToArray())
        End Sub
        Public ReadOnly Property FirstRows As IReadOnlyList(Of Integer)
        Public ReadOnly Property SecondRows As IReadOnlyList(Of Integer)
    End Class

    Public NotInheritable Class SpreadsheetNestedInput
        Inherits SpreadsheetAnalysisInput
        Public Sub New(groups As IEnumerable(Of String), nested As IEnumerable(Of String), responses As SpreadsheetNumericBlocks)
            GroupLabels = Array.AsReadOnly(groups.ToArray())
            NestedLabels = Array.AsReadOnly(nested.ToArray())
            Me.Responses = responses
        End Sub
        Public ReadOnly Property GroupLabels As IReadOnlyList(Of String)
        Public ReadOnly Property NestedLabels As IReadOnlyList(Of String)
        Public ReadOnly Property Responses As SpreadsheetNumericBlocks
    End Class

    ''' <summary>Alignment/complete-observation policy only. Acquisition and statistics remain with their owners.</summary>
    Public NotInheritable Class PairedBlockInputPolicy
        Private Sub New()
        End Sub

        Private Shared Sub Issue(issues As List(Of SpreadsheetValidationIssue), code As String, message As String,
                                 Optional row As Integer = 0, Optional col As Integer = 0, Optional physicalRow As Integer = 0,
                                 Optional physicalCol As Integer = 0, Optional warning As Boolean = False)
            issues.Add(New SpreadsheetValidationIssue(code, If(warning, SpreadsheetValidationSeverity.Warning, SpreadsheetValidationSeverity.Error),
                       message, If(row > 0, $"input.rows[{row}].cells[{col}]", "input"), row, col, physicalRow, physicalCol))
        End Sub

        Private Shared Function Shape(input As SpreadsheetRangeData, rowIds As IReadOnlyList(Of Integer), issues As List(Of SpreadsheetValidationIssue)) As Boolean
            If input Is Nothing OrElse input.Rows Is Nothing OrElse input.RowCount < 1 OrElse input.ColumnCount < 1 OrElse
                input.Rows.Count <> input.RowCount OrElse input.Rows.Any(Function(r) r Is Nothing OrElse r.Count <> input.ColumnCount) Then
                Issue(issues, "alignment.shape", "A nonempty rectangular input is required.")
                Return False
            End If
            If rowIds IsNot Nothing AndAlso (rowIds.Count <> input.RowCount OrElse rowIds.Any(Function(r) r < 1) OrElse rowIds.Distinct().Count() <> rowIds.Count) Then
                Issue(issues, "alignment.row-ids", "Source row IDs must be positive, unique and match the captured rows.")
                Return False
            End If
            Return True
        End Function

        Private Shared Function Physical(input As SpreadsheetRangeData, rows As IReadOnlyList(Of Integer), index As Integer) As Integer
            Return If(rows IsNot Nothing, rows(index), If(input.StartRow > 0, input.StartRow + index, 0))
        End Function

        Public Shared Function ParsePairs(input As SpreadsheetRangeData, options As SpreadsheetPairedParseOptions,
                                          Optional rowIds As IReadOnlyList(Of Integer) = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetPairedNumericInput)
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If Not Shape(input, rowIds, issues) Then Return New SpreadsheetTypedParseResult(Of SpreadsheetPairedNumericInput)(Nothing, New SpreadsheetValidationReport(issues))
            Dim parsed = TypedNumericRangeParsers.ParsePaired(input, options)
            For Each i In parsed.Validation.Issues
                issues.Add(New SpreadsheetValidationIssue(i.Code, i.Severity, i.Message, i.FieldPath, i.SelectedRow, i.SelectedColumn,
                    If(i.SelectedRow > 0, Physical(input, rowIds, i.SelectedRow - 1), i.WorksheetRow), i.WorksheetColumn))
            Next
            If Not parsed.Success Then Return New SpreadsheetTypedParseResult(Of SpreadsheetPairedNumericInput)(Nothing, New SpreadsheetValidationReport(issues))
            Dim pairs As New List(Of SpreadsheetNumericPair)
            For Each p In parsed.Data.Pairs
                pairs.Add(New SpreadsheetNumericPair(Remap(p.First, input, rowIds), Remap(p.Second, input, rowIds)))
            Next
            Return New SpreadsheetTypedParseResult(Of SpreadsheetPairedNumericInput)(New SpreadsheetPairedNumericInput(parsed.Data.FirstName, parsed.Data.SecondName, pairs), New SpreadsheetValidationReport(issues))
        End Function

        Private Shared Function Remap(o As SpreadsheetNumericObservation, input As SpreadsheetRangeData, rows As IReadOnlyList(Of Integer)) As SpreadsheetNumericObservation
            Return New SpreadsheetNumericObservation(o.Value, o.SelectedRow, o.SelectedColumn, Physical(input, rows, o.SelectedRow - 1), o.WorksheetColumn)
        End Function

        Public Shared Function ParseBlocks(input As SpreadsheetRangeData, options As SpreadsheetBlockParseOptions,
                                           Optional rowIds As IReadOnlyList(Of Integer) = Nothing,
                                           Optional names As IReadOnlyList(Of String) = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetNumericBlocks)
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If Not Shape(input, rowIds, issues) Then Return New SpreadsheetTypedParseResult(Of SpreadsheetNumericBlocks)(Nothing, New SpreadsheetValidationReport(issues))
            If options Is Nothing OrElse options.MinimumBlocks < 1 OrElse options.MinimumTreatments < 1 OrElse
                Not [Enum].IsDefined(GetType(SpreadsheetIncompleteObservationPolicy), options.Missing) OrElse
                (options.Header <> SpreadsheetHeaderMode.None AndAlso options.Header <> SpreadsheetHeaderMode.Required) Then
                Issue(issues, "blocks.options", "Explicit None/Required headers and valid complete-block options are required.")
                Return New SpreadsheetTypedParseResult(Of SpreadsheetNumericBlocks)(Nothing, New SpreadsheetValidationReport(issues))
            End If
            If input.ColumnCount < options.MinimumTreatments Then Issue(issues, "blocks.treatments", "Too few treatment columns.")
            Dim labels As New List(Of String)
            For c = 0 To input.ColumnCount - 1
                labels.Add(If(names IsNot Nothing AndAlso names.Count = input.ColumnCount, names(c), "Series" & (c + 1).ToString(Globalization.CultureInfo.InvariantCulture)))
                If options.Header = SpreadsheetHeaderMode.Required Then
                    Dim cell = input.Rows(0)(c)
                    If cell Is Nothing OrElse cell.Kind <> SpreadsheetCellKind.Text OrElse String.IsNullOrWhiteSpace(cell.TextValue) Then
                        Issue(issues, "blocks.header", "An explicit text treatment header is required.", 1, c + 1, Physical(input, rowIds, 0))
                    Else
                        labels(c) = cell.TextValue
                    End If
                End If
            Next
            If names IsNot Nothing AndAlso names.Count <> input.ColumnCount Then Issue(issues, "blocks.names", "Treatment labels do not match columns.")
            If options.RequireUniqueNames AndAlso labels.Distinct(StringComparer.Ordinal).Count() <> labels.Count Then Issue(issues, "blocks.duplicate-name", "Treatment labels must be unique.")
            Dim blocks As New List(Of IEnumerable(Of SpreadsheetNumericObservation))
            For r = If(options.Header = SpreadsheetHeaderMode.Required, 1, 0) To input.RowCount - 1
                Dim cells As New List(Of SpreadsheetNumericObservation)
                Dim incomplete As Boolean = False
                For c = 0 To input.ColumnCount - 1
                    Dim physicalCol = If(input.StartColumn > 0, input.StartColumn + c, 0)
                    Try
                        Dim value = SpreadsheetCellValuePolicy.Evaluate(input.Rows(r)(c), options.CellPolicy)
                        If value.IsMissing Then
                            incomplete = True
                        ElseIf value.IsError OrElse Not value.HasNumericValue Then
                            Issue(issues, "blocks.numeric", "A permitted finite numeric value is required.", r + 1, c + 1, Physical(input, rowIds, r), physicalCol)
                            incomplete = True
                        Else
                            cells.Add(New SpreadsheetNumericObservation(value.NumericValue, r + 1, c + 1, Physical(input, rowIds, r), physicalCol))
                        End If
                    Catch ex As ArgumentException
                        Issue(issues, "blocks.numeric", ex.Message, r + 1, c + 1, Physical(input, rowIds, r), physicalCol)
                        incomplete = True
                    End Try
                Next
                If incomplete Then
                    Issue(issues, If(options.Missing = SpreadsheetIncompleteObservationPolicy.Skip, "blocks.incomplete-skipped", "blocks.incomplete"),
                          "The entire incomplete block is excluded.", r + 1, 0, Physical(input, rowIds, r), warning:=options.Missing = SpreadsheetIncompleteObservationPolicy.Skip)
                Else
                    blocks.Add(cells)
                End If
            Next
            If blocks.Count < options.MinimumBlocks Then Issue(issues, "blocks.too-few", "Too few complete blocks.")
            Dim report As New SpreadsheetValidationReport(issues)
            Return New SpreadsheetTypedParseResult(Of SpreadsheetNumericBlocks)(If(report.HasErrors, Nothing, New SpreadsheetNumericBlocks(labels, blocks)), report)
        End Function

        ''' <summary>Physical row identity requires explicit, equal workbook and worksheet identity.</summary>
        Public Shared Function AlignRows(first As SpreadsheetRangeData, firstRows As IReadOnlyList(Of Integer),
                                         second As SpreadsheetRangeData, secondRows As IReadOnlyList(Of Integer),
                                         Optional firstKeys As IReadOnlyList(Of String) = Nothing,
                                         Optional secondKeys As IReadOnlyList(Of String) = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetRowAlignment)
            Dim issues As New List(Of SpreadsheetValidationIssue)
            Dim valid = Shape(first, firstRows, issues)
            valid = Shape(second, secondRows, issues) AndAlso valid
            If valid Then
                If first.ColumnCount <> second.ColumnCount Then Issue(issues, "alignment.columns", "Paired matrices require the same number of columns.")
                Dim keyed = firstKeys IsNot Nothing OrElse secondKeys IsNot Nothing
                If Not keyed AndAlso (firstRows Is Nothing OrElse secondRows Is Nothing OrElse String.IsNullOrEmpty(first.WorkbookId) OrElse
                    String.IsNullOrEmpty(first.WorksheetId) OrElse first.WorkbookId <> second.WorkbookId OrElse first.WorksheetId <> second.WorksheetId) Then
                    Issue(issues, "alignment.identity", "Physical-row pairing requires verified equal workbook/worksheet identities and row IDs.")
                End If
                If keyed AndAlso (Not ValidKeys(firstKeys, first.RowCount) OrElse Not ValidKeys(secondKeys, second.RowCount)) Then
                    Issue(issues, "alignment.keys", "Both explicit subject-key lists must be complete, unique and match their selections.")
                End If
                If Not New SpreadsheetValidationReport(issues).HasErrors Then
                    Dim lookup As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
                    For r = 0 To second.RowCount - 1
                        lookup.Add(If(keyed, secondKeys(r), secondRows(r).ToString(Globalization.CultureInfo.InvariantCulture)), r + 1)
                    Next
                    Dim a As New List(Of Integer), b As New List(Of Integer)
                    For r = 0 To first.RowCount - 1
                        Dim k = If(keyed, firstKeys(r), firstRows(r).ToString(Globalization.CultureInfo.InvariantCulture))
                        If lookup.ContainsKey(k) Then
                            a.Add(r + 1) : b.Add(lookup(k))
                        Else
                            Issue(issues, "alignment.unmatched-first", "Unmatched first-selection row excluded.", r + 1, 0, Physical(first, firstRows, r), warning:=True)
                        End If
                    Next
                    For r = 0 To second.RowCount - 1
                        If Not b.Contains(r + 1) Then Issue(issues, "alignment.unmatched-second", "Unmatched second-selection row excluded.", r + 1, 0, Physical(second, secondRows, r), warning:=True)
                    Next
                    If a.Count > 0 Then Return New SpreadsheetTypedParseResult(Of SpreadsheetRowAlignment)(New SpreadsheetRowAlignment(a, b), New SpreadsheetValidationReport(issues))
                    Issue(issues, "alignment.empty", "No aligned observations remain.")
                End If
            End If
            Return New SpreadsheetTypedParseResult(Of SpreadsheetRowAlignment)(Nothing, New SpreadsheetValidationReport(issues))
        End Function

        Private Shared Function ValidKeys(keys As IReadOnlyList(Of String), count As Integer) As Boolean
            Return keys IsNot Nothing AndAlso keys.Count = count AndAlso Not keys.Any(Function(k) String.IsNullOrWhiteSpace(k)) AndAlso keys.Distinct(StringComparer.Ordinal).Count() = count
        End Function

        Public Shared Function ParseNested(input As SpreadsheetRangeData, rowIds As IReadOnlyList(Of Integer),
                                           Optional missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Skip) As SpreadsheetTypedParseResult(Of SpreadsheetNestedInput)
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If Not Shape(input, rowIds, issues) Then Return New SpreadsheetTypedParseResult(Of SpreadsheetNestedInput)(Nothing, New SpreadsheetValidationReport(issues))
            If input.ColumnCount < 3 Then
                Issue(issues, "nested.roles", "Group, nested group and response columns are required.")
                Return New SpreadsheetTypedParseResult(Of SpreadsheetNestedInput)(Nothing, New SpreadsheetValidationReport(issues))
            End If
            Dim numeric As New SpreadsheetRangeData With {.RowCount = input.RowCount, .ColumnCount = input.ColumnCount - 2, .StartRow = input.StartRow}
            Dim groups As New List(Of String), nested As New List(Of String)
            For r = 0 To input.RowCount - 1
                Dim g = Label(input.Rows(r)(0)), n = Label(input.Rows(r)(1))
                groups.Add(g) : nested.Add(n)
                Dim cells = input.Rows(r).Skip(2).ToList()
                If String.IsNullOrWhiteSpace(g) OrElse String.IsNullOrWhiteSpace(n) Then
                    Issue(issues, "nested.missing-label", "Missing group/nested label; the entire observation is excluded.", r + 1, If(String.IsNullOrWhiteSpace(g), 1, 2), Physical(input, rowIds, r), warning:=missing = SpreadsheetIncompleteObservationPolicy.Skip)
                    cells(0) = SpreadsheetCellValue.BlankCell()
                End If
                numeric.Rows.Add(cells)
            Next
            Dim parsed = ParseBlocks(numeric, New SpreadsheetBlockParseOptions With {.MinimumTreatments = 1, .Missing = missing}, rowIds)
            issues.AddRange(parsed.Validation.Issues)
            Dim report As New SpreadsheetValidationReport(issues)
            If Not parsed.Success OrElse report.HasErrors Then Return New SpreadsheetTypedParseResult(Of SpreadsheetNestedInput)(Nothing, report)
            ' Restore original response column positions; labels are a hierarchical pair, never a global subgroup key.
            Dim mapped As New List(Of IEnumerable(Of SpreadsheetNumericObservation))
            Dim keptGroups As New List(Of String), keptNested As New List(Of String)
            For Each block In parsed.Data.Blocks
                Dim r = block(0).SelectedRow - 1
                keptGroups.Add(groups(r)) : keptNested.Add(nested(r))
                mapped.Add(block.Select(Function(o) New SpreadsheetNumericObservation(o.Value, o.SelectedRow, o.SelectedColumn + 2, o.WorksheetRow, If(input.StartColumn > 0, input.StartColumn + o.SelectedColumn + 1, 0))))
            Next
            Return New SpreadsheetTypedParseResult(Of SpreadsheetNestedInput)(New SpreadsheetNestedInput(keptGroups, keptNested, New SpreadsheetNumericBlocks(parsed.Data.Names, mapped)), report)
        End Function

        Private Shared Function Label(cell As SpreadsheetCellValue) As String
            If cell Is Nothing Then Return Nothing
            If cell.Kind = SpreadsheetCellKind.Text Then Return cell.TextValue
            If cell.Kind = SpreadsheetCellKind.Number AndAlso Not Double.IsNaN(cell.NumberValue) AndAlso Not Double.IsInfinity(cell.NumberValue) Then Return cell.NumberValue.ToString("R", Globalization.CultureInfo.InvariantCulture)
            Return Nothing
        End Function
    End Class
End Namespace
