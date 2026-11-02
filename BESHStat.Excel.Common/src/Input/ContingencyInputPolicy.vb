Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Checked Core boundary with immutable original count table and count-cell provenance.</summary>
    Public NotInheritable Class ContingencyCountSelection
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly _core As Integer(,)
        Friend Sub New(table As SpreadsheetContingencyTableInput, core As Integer(,), sources As IEnumerable(Of SpreadsheetFrameCell), source As SpreadsheetRangeData)
            Me.Table = table
            _core = DirectCast(core.Clone(), Integer(,))
            Me.Sources = Array.AsReadOnly(sources.ToArray())
            WorkbookId = If(source.WorkbookId, String.Empty)
            WorksheetId = If(source.WorksheetId, String.Empty)
            WorksheetName = If(source.WorksheetName, String.Empty)
            SourceAddress = If(source.Address, String.Empty)
        End Sub
        Public ReadOnly Property Table As SpreadsheetContingencyTableInput
        Public ReadOnly Property Sources As IReadOnlyList(Of SpreadsheetFrameCell)
        Public ReadOnly Property WorkbookId As String
        Public ReadOnly Property WorksheetId As String
        Public ReadOnly Property WorksheetName As String
        Public ReadOnly Property SourceAddress As String
        Public ReadOnly Property CoreCounts As Integer(,)
            Get
                Return DirectCast(_core.Clone(), Integer(,))
            End Get
        End Property
    End Class

    Public NotInheritable Class ContingencyInputPolicy
        Private Sub New()
        End Sub
        ''' <summary>One parser: the established categorical count parser, followed by checked Int32 projection.</summary>
        Public Shared Function Parse(input As SpreadsheetRangeData, options As SpreadsheetContingencyCountParseOptions,
                                     Optional requireTwoByTwo As Boolean = False,
                                     Optional sourceRows As IReadOnlyList(Of Integer) = Nothing) As SpreadsheetTypedParseResult(Of ContingencyCountSelection)
            Dim parsed = RectangularCategoricalParsers.ParseContingencyCounts(input, options)
            Dim issues As New List(Of SpreadsheetValidationIssue)(parsed.Validation.Issues)
            If sourceRows IsNot Nothing AndAlso (input Is Nothing OrElse sourceRows.Count <> input.RowCount OrElse sourceRows.Any(Function(r) r < 1) OrElse sourceRows.Distinct().Count() <> sourceRows.Count) Then
                issues.Add(ErrorIssue("contingency.source-rows", "Source rows must match the selection and be positive and unique.", "source.rows"))
            End If
            If sourceRows IsNot Nothing AndAlso input IsNot Nothing AndAlso sourceRows.Count = input.RowCount AndAlso sourceRows.All(Function(r) r > 0) Then
                issues = issues.Select(Function(i) New SpreadsheetValidationIssue(i.Code, i.Severity, i.Message, i.FieldPath, i.SelectedRow, i.SelectedColumn,
                    If(i.SelectedRow > 0 AndAlso i.SelectedRow <= sourceRows.Count, sourceRows(i.SelectedRow - 1), i.WorksheetRow), i.WorksheetColumn)).ToList()
            End If
            If Not parsed.Success OrElse New SpreadsheetValidationReport(issues).HasErrors Then Return Finish(Nothing, issues)
            Dim table = parsed.Data
            If requireTwoByTwo AndAlso (table.RowCount <> 2 OrElse table.ColumnCount <> 2) Then issues.Add(ErrorIssue("contingency.two-by-two", "Wrong dimensions of input table, 2x2 is expected.", "counts"))
            Dim values = table.Counts
            Dim core(table.RowCount - 1, table.ColumnCount - 1) As Integer
            Dim sources As New List(Of SpreadsheetFrameCell)
            Dim firstRow = If(options IsNot Nothing AndAlso options.HasColumnHeaders, 1, 0)
            Dim firstColumn = If(options IsNot Nothing AndAlso options.HasRowLabels, 1, 0)
            ' Nothing options means the established parser's explicit default: both label axes.
            If options Is Nothing Then firstRow = 1 : firstColumn = 1
            For r = 0 To table.RowCount - 1
                For c = 0 To table.ColumnCount - 1
                    Dim sr = r + firstRow + 1, sc = c + firstColumn + 1
                    Dim wr = If(sourceRows IsNot Nothing, sourceRows(sr - 1), If(input.StartRow > 0, input.StartRow + sr - 1, 0))
                    Dim wc = If(input.StartColumn > 0, input.StartColumn + sc - 1, 0)
                    If values(r)(c) > Integer.MaxValue Then
                        issues.Add(New SpreadsheetValidationIssue("contingency.int32-count", SpreadsheetValidationSeverity.Error, "Count exceeds the Core Int32 boundary.", "counts", sr, sc, wr, wc))
                    Else
                        core(r, c) = CInt(values(r)(c))
                    End If
                    sources.Add(New SpreadsheetFrameCell(input.Rows(sr - 1)(sc - 1), sr, sc, wr, wc))
                Next
            Next
            If table.GrandTotal > Integer.MaxValue Then issues.Add(ErrorIssue("contingency.int32-total", "Table total exceeds the Core Int32 boundary.", "counts.total"))
            Return Finish(If(New SpreadsheetValidationReport(issues).HasErrors, Nothing, New ContingencyCountSelection(table, core, sources, input)), issues)
        End Function

        Public Shared Function FromDialog(a As Double, b As Double, c As Double, d As Double) As SpreadsheetTypedParseResult(Of ContingencyCountSelection)
            Return Parse(NumericSnapshot(New Double(,) {{a, b}, {c, d}}), New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders = False, .HasRowLabels = False}, True)
        End Function

        Public Shared Function NumericSnapshot(values As Double(,)) As SpreadsheetRangeData
            If values Is Nothing Then Return Nothing
            Dim snapshot As New SpreadsheetRangeData With {.RowCount = values.GetLength(0), .ColumnCount = values.GetLength(1)}
            For r = 0 To snapshot.RowCount - 1
                Dim row As New List(Of SpreadsheetCellValue)
                For c = 0 To snapshot.ColumnCount - 1
                    row.Add(SpreadsheetCellValue.NumberCell(values(r, c)))
                Next
                snapshot.Rows.Add(row)
            Next
            Return snapshot
        End Function

        Friend Shared Function ErrorIssue(code As String, message As String, field As String) As SpreadsheetValidationIssue
            Return New SpreadsheetValidationIssue(code, SpreadsheetValidationSeverity.Error, message, field)
        End Function
        Private Shared Function Finish(data As ContingencyCountSelection, issues As List(Of SpreadsheetValidationIssue)) As SpreadsheetTypedParseResult(Of ContingencyCountSelection)
            Return New SpreadsheetTypedParseResult(Of ContingencyCountSelection)(data, New SpreadsheetValidationReport(issues))
        End Function
    End Class

    Public Enum ProportionsMode
        SingleSample = 0
        Independent = 1
        Paired = 2
    End Enum
    Public Enum IndependentProportionsHypothesis
        Superiority = 0
        Noninferiority = 1
        Equivalence = 2
    End Enum

    Public NotInheritable Class ProportionsRequest
        Inherits SpreadsheetAnalysisInput
        Friend Sub New(mode As ProportionsMode, hypothesis As IndependentProportionsHypothesis, n1 As Integer, r1 As Integer, n2 As Integer, r2 As Integer, both As Integer, alpha As Double, margin As Double)
            Me.Mode = mode : Me.Hypothesis = hypothesis
            Total1 = n1 : Responders1 = r1 : Total2 = n2 : Responders2 = r2 : Me.Both = both
            Me.Alpha = alpha : Me.Margin = margin
        End Sub
        Public ReadOnly Property Mode As ProportionsMode
        Public ReadOnly Property Hypothesis As IndependentProportionsHypothesis
        Public ReadOnly Property Total1 As Integer
        Public ReadOnly Property Responders1 As Integer
        Public ReadOnly Property Total2 As Integer
        Public ReadOnly Property Responders2 As Integer
        Public ReadOnly Property Both As Integer
        Public ReadOnly Property Alpha As Double
        Public ReadOnly Property Margin As Double
        ''' <summary>Rows=response status, columns=control and experimental; no sign reversal.</summary>
        Public Function IndependentFisherTable() As Integer(,)
            Return New Integer(,) {{Responders1, Responders2}, {Total1 - Responders1, Total2 - Responders2}}
        End Function
        ''' <summary>Disjoint categories: row first condition yes/no, column second condition yes/no.
        ''' Owner-approved mutually exclusive counts; the difference direction is first minus second.</summary>
        Public Function PairedOutcomeTable() As Integer(,)
            Return New Integer(,) {{Both, Responders1}, {Responders2, Total1 - Responders1 - Responders2 - Both}}
        End Function
        ''' <summary>Core layout: concordant negative, first-only / second-only, concordant positive.
        ''' The same off-diagonal discordants are retained in both named conventions.</summary>
        Public Function CoreLiddellTable() As Integer(,)
            Return New Integer(,) {{Total1 - Both - Responders1 - Responders2, Responders1}, {Responders2, Both}}
        End Function
    End Class

    Public NotInheritable Class ProportionsInputPolicy
        Private Sub New()
        End Sub
        Public Shared Function ModeFromChoices(singleSample As Boolean, independent As Boolean, paired As Boolean) As ProportionsMode
            If If(singleSample, 1, 0) + If(independent, 1, 0) + If(paired, 1, 0) <> 1 Then Return CType(-1, ProportionsMode)
            Return If(singleSample, ProportionsMode.SingleSample, If(independent, ProportionsMode.Independent, ProportionsMode.Paired))
        End Function
        Public Shared Function ValidateMargin(hypothesis As IndependentProportionsHypothesis, margin As Double) As SpreadsheetValidationReport
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If Not [Enum].IsDefined(GetType(IndependentProportionsHypothesis), hypothesis) Then
                issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.mode", "Invalid proportions hypothesis.", "options.hypothesis"))
            ElseIf hypothesis <> IndependentProportionsHypothesis.Superiority AndAlso (Double.IsNaN(margin) OrElse Double.IsInfinity(margin) OrElse margin <= 0) Then
                issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.margin", "The margin must be greater than 0.", "options.margin"))
            End If
            Return New SpreadsheetValidationReport(issues)
        End Function
        Public Shared Function Validate(mode As ProportionsMode, hypothesis As IndependentProportionsHypothesis,
                                        n1 As Double, r1 As Double, n2 As Double, r2 As Double, both As Double,
                                        alpha As Double, margin As Double) As SpreadsheetTypedParseResult(Of ProportionsRequest)
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If Not [Enum].IsDefined(GetType(ProportionsMode), mode) OrElse Not [Enum].IsDefined(GetType(IndependentProportionsHypothesis), hypothesis) Then issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.mode", "Invalid proportions mode/hypothesis.", "options.mode"))
            If Double.IsNaN(alpha) OrElse Double.IsInfinity(alpha) OrElse alpha <= 0 OrElse alpha >= 1 OrElse
                (mode = ProportionsMode.Independent AndAlso hypothesis <> IndependentProportionsHypothesis.Superiority AndAlso alpha >= 0.5) Then
                issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.alpha", "Alpha is outside the supported interval for the selected hypothesis.", "options.alpha"))
            End If
            If mode = ProportionsMode.Independent Then issues.AddRange(ValidateMargin(hypothesis, margin).Issues)
            Dim values As Double() = If(mode = ProportionsMode.SingleSample, New Double() {n1, r1}, If(mode = ProportionsMode.Independent, New Double() {n1, r1, n2, r2}, New Double() {n1, r1, r2, both}))
            Dim fields As String() = If(mode = ProportionsMode.SingleSample, New String() {"total1", "responders1"}, If(mode = ProportionsMode.Independent, New String() {"total1", "responders1", "total2", "responders2"}, New String() {"total1", "firstOnly", "secondOnly", "both"}))
            Dim raw(0, values.Length - 1) As Double
            For c = 0 To values.Length - 1
                raw(0, c) = values(c)
            Next
            Dim parsed = RectangularCategoricalParsers.ParseContingencyCounts(ContingencyInputPolicy.NumericSnapshot(raw), New SpreadsheetContingencyCountParseOptions With {
                .HasColumnHeaders = False, .HasRowLabels = False, .MinimumRows = 1, .MinimumColumns = 1})
            For Each i In parsed.Validation.Issues
                issues.Add(New SpreadsheetValidationIssue(i.Code, i.Severity, i.Message, If(i.SelectedColumn > 0, "counts." & fields(i.SelectedColumn - 1), "counts"), i.SelectedRow, i.SelectedColumn))
            Next
            If values.Any(Function(v) v > Integer.MaxValue) Then issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.int32", "Count exceeds the Core Int32 boundary.", "counts"))
            If Not New SpreadsheetValidationReport(issues).HasErrors Then
                If n1 <= 0 Then issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.total", "Total sample size must be positive.", "counts.total1"))
                If r1 > n1 Then issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.responders", "Total Number of observations is less then the Number of Responders", "counts.responders1"))
                If mode = ProportionsMode.Independent Then
                    If n2 <= 0 Then issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.total", "Total sample size must be positive.", "counts.total2"))
                    If r2 > n2 Then issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.responders", "Total Number of observations is less then the Number of Responders", "counts.responders2"))
                    If n1 + n2 > Integer.MaxValue Then issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.int32-total", "Combined sample size exceeds the Core Int32 boundary.", "counts"))
                ElseIf mode = ProportionsMode.Paired Then
                    If r1 + r2 + both > n1 Then issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.paired-accounting", "The disjoint first-only, second-only and both counts must not exceed N.", "counts.neither"))
                End If
            End If
            Dim report As New SpreadsheetValidationReport(issues)
            Dim request As ProportionsRequest = Nothing
            If report.IsValid Then request = New ProportionsRequest(mode, hypothesis, CInt(n1), CInt(r1), If(mode = ProportionsMode.Independent, CInt(n2), 0), If(mode = ProportionsMode.SingleSample, 0, CInt(r2)), If(mode = ProportionsMode.Paired, CInt(both), 0), alpha, margin)
            Return New SpreadsheetTypedParseResult(Of ProportionsRequest)(request, report)
        End Function
    End Class
End Namespace
