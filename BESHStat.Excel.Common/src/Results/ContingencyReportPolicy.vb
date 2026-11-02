Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Immutable semantic body/header/formatting snapshot. Conversion creates a fresh Core table.</summary>
    Public NotInheritable Class ContingencySemanticTable
        Private ReadOnly _header As String()
        Private ReadOnly _body As Object(,)
        Private ReadOnly _pRows As Integer()
        Private ReadOnly _pColumn As Integer
        Public Sub New(header As String(), body As Object(,), Optional pRows As Integer() = Nothing, Optional pColumn As Integer = 0)
            _header = CType(header.Clone(), String())
            _body = DirectCast(body.Clone(), Object(,))
            _pRows = If(pRows Is Nothing, Array.Empty(Of Integer)(), CType(pRows.Clone(), Integer()))
            _pColumn = pColumn
        End Sub
        Public ReadOnly Property Header As String()
            Get
                Return CType(_header.Clone(), String())
            End Get
        End Property
        Public ReadOnly Property Body As Object(,)
            Get
                Return DirectCast(_body.Clone(), Object(,))
            End Get
        End Property
        Public Function ToCoreTable() As ResultTable
            Dim result As New ResultTable
            result.AddHeaderTopRow(Header)
            result.SetBody(Body)
            If _pColumn > 0 Then result.AddPvalueToFormat(_pColumn)
            For Each row In _pRows
                result.AddPvalueCellToFormat(row, 2)
            Next
            Return result
        End Function
        Public Function ToArtifact(index As Integer) As AnalysisTableArtifact
            Return New AnalysisTableArtifact(index, ToCoreTable().ToOutputModel())
        End Function
    End Class

    Public NotInheritable Class ContingencyReportResult
        Public Sub New(tables As IEnumerable(Of ContingencySemanticTable), issues As IEnumerable(Of SpreadsheetValidationIssue),
                       Optional metrics As IEnumerable(Of AnalysisMetricArtifact) = Nothing)
            Validation = New SpreadsheetValidationReport(issues)
            Me.Tables = Array.AsReadOnly(If(Validation.HasErrors, Array.Empty(Of ContingencySemanticTable)(), tables.ToArray()))
            Me.Metrics = Array.AsReadOnly(If(Validation.HasErrors OrElse metrics Is Nothing, Array.Empty(Of AnalysisMetricArtifact)(), metrics.ToArray()))
        End Sub
        Public ReadOnly Property Tables As IReadOnlyList(Of ContingencySemanticTable)
        Public ReadOnly Property Metrics As IReadOnlyList(Of AnalysisMetricArtifact)
        Public ReadOnly Property Validation As SpreadsheetValidationReport
        Public ReadOnly Property Success As Boolean
            Get
                Return Validation.IsValid AndAlso Tables.Count > 0
            End Get
        End Property
        Public Function ToLegacyTables() As List(Of ResultTable)
            Return Tables.Select(Function(t) t.ToCoreTable()).ToList()
        End Function
    End Class

    Public NotInheritable Class TwoByTwoReportOptions
        Public Property Alpha As Double = 0.05
        Public Property Fisher As Boolean
        Public Property ChiSquare As Boolean
        Public Property Association As Boolean
        Public Property Liddell As Boolean
        Public Property OddsRatio As Boolean
        Public Property RiskRatio As Boolean
    End Class
    Public NotInheritable Class RxCReportOptions
        Public Property Alpha As Double = 0.05
        Public Property Nominal As Boolean
        Public Property Exact As Boolean
        Public Property Ordinal As Boolean
        Public Property Trend As Boolean
    End Class

    ''' <summary>Core-backed statistical dispatch and legacy semantic report mapping; no workbook or UI operations.</summary>
    Public NotInheritable Class ContingencyReportPolicy
        Private Sub New()
        End Sub
        Private Shared Function Table(title As String, body As Object(,), ParamArray pRows As Integer()) As ContingencySemanticTable
            Return New ContingencySemanticTable({title, ""}, body, pRows)
        End Function
        Private Shared Sub Warn(issues As List(Of SpreadsheetValidationIssue), code As String, message As String)
            issues.Add(New SpreadsheetValidationIssue(code, SpreadsheetValidationSeverity.Warning, message, "options"))
        End Sub
        Private Shared Function ValidateInput(input As ContingencyCountSelection, alpha As Double) As List(Of SpreadsheetValidationIssue)
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If input Is Nothing Then issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.input", "Validated count selection is required.", "input"))
            If input IsNot Nothing AndAlso (input.Table.RowCount < 2 OrElse input.Table.ColumnCount < 2) Then issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.shape", "Table statistics require at least two rows and two columns.", "counts"))
            issues.AddRange(PairedOptionPolicy.Validate(alpha, PairedAlternative.TwoSided, PairedDifferenceDirection.FirstMinusSecond).Issues)
            Return issues
        End Function
        Private Shared Sub CI(metrics As List(Of AnalysisMetricArtifact), name As String, value As ConfidenceIntervalResult)
            metrics.Add(New AnalysisMetricArtifact(name & ".estimate", value.Estimate, AnalysisMetricRole.Statistic))
            metrics.Add(New AnalysisMetricArtifact(name & ".lower", value.LowerLimit, AnalysisMetricRole.Statistic))
            metrics.Add(New AnalysisMetricArtifact(name & ".upper", value.UpperLimit, AnalysisMetricRole.Statistic))
        End Sub

        Public Shared Function TwoByTwo(input As ContingencyCountSelection, options As TwoByTwoReportOptions) As ContingencyReportResult
            If options Is Nothing Then Return New ContingencyReportResult(Array.Empty(Of ContingencySemanticTable)(), {ContingencyInputPolicy.ErrorIssue("contingency.options", "Options are required.", "options")})
            Dim issues = ValidateInput(input, options.Alpha), tables As New List(Of ContingencySemanticTable), metrics As New List(Of AnalysisMetricArtifact)
            If input IsNot Nothing AndAlso (input.Table.RowCount <> 2 OrElse input.Table.ColumnCount <> 2) Then issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.two-by-two", "Wrong dimensions of input table, 2x2 is expected.", "counts"))
            If Not (options.Fisher OrElse options.ChiSquare OrElse options.Association OrElse options.Liddell OrElse options.OddsRatio OrElse options.RiskRatio) Then issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.options.none", "Select statistic(s) to compute.", "options"))
            If New SpreadsheetValidationReport(issues).HasErrors Then Return New ContingencyReportResult(tables, issues)
            Dim tab = input.CoreCounts, alpha = options.Alpha
            Try
                tables.Add(Table("Analyzed 2x2 table", ObjectCounts(tab)))
                If options.Fisher Then
                    If input.Table.GrandTotal > 1000 Then
                        Warn(issues, "contingency.fisher.too-large", "Too large sample size for exact computation.")
                    Else
                        Dim f = contingencytable.FisherExact2x2(tab(0, 0), tab(0, 1), tab(1, 0), tab(1, 1))
                        tables.Add(New ContingencySemanticTable({"Fisher's Exact Test", ""},
                            {{"one-sided P-value", f.PvalueLowerSide}, {"two-sided P-value", f.Pvalue},
                             {"one-sided mid P-value", f.pValueExactLowerSide}, {"two-sided mid P-value", f.Pvalue2}}, pColumn:=2))
                    End If
                End If
                If options.ChiSquare OrElse options.Association Then
                    Dim chi = contingencytable.Chi2TESTindependence(tab)
                    tables.Add(Table("Pearson's Chi-squared Test", {{"Chi-Square", chi.Item1.TestStatistics1}, {"two-sided P-value", chi.Item1.Pvalue}}, 2))
                    tables.Add(Nominal(chi.Item2, chi.Item3, chi.Item4))
                End If
                If options.Liddell Then
                    Dim value = contingencytable.Liddell_McNemar(tab, alpha)
                    tables.Add(Table("Liddell's Test", {{"P-value", value.Item1.Pvalue}, {"Risk ratio", value.Item2.Estimate},
                        {value.Item2.CIlabel, value.Item2.strConfidenceInterval(CIformat.LL_to_UL)}}, 1))
                    CI(metrics, "liddell", value.Item2)
                End If
                If options.OddsRatio Then
                    Dim value = contingencytable.OddsRatio(tab, alpha)
                    tables.Add(Table("Odds Ratio", {{"OR", value.Item1.Estimate},
                        {value.Item1.CIlabel & " (Woolf)", value.Item1.strConfidenceInterval(CIformat.LL_to_UL)},
                        {value.Item2.CIlabel & " (Cornfield)", value.Item2.strConfidenceInterval(CIformat.LL_to_UL)}}))
                    CI(metrics, "or.woolf", value.Item1) : CI(metrics, "or.cornfield", value.Item2)
                End If
                If options.RiskRatio Then
                    Dim value = contingencytable.RiskRatio(tab, alpha)
                    tables.Add(Table("Risk Ratio", {{"RR", value.Estimate}, {value.CIlabel, value.strConfidenceInterval(CIformat.LL_to_UL)}}))
                    CI(metrics, "rr", value)
                End If
                If tables.Count = 1 Then issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.no-result", "Select statistic(s) to compute.", "options"))
            Catch ex As ArgumentException
                issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.core", ex.Message, "analysis"))
            Catch ex As ArithmeticException
                issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.core", ex.Message, "analysis"))
            Catch ex As InvalidOperationException
                issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.core", ex.Message, "analysis"))
            End Try
            Return New ContingencyReportResult(tables, issues, metrics)
        End Function

        Public Shared Function RxC(input As ContingencyCountSelection, options As RxCReportOptions,
                                   Optional exactEvaluator As Func(Of Integer(,), Double) = Nothing) As ContingencyReportResult
            If options Is Nothing Then Return New ContingencyReportResult(Array.Empty(Of ContingencySemanticTable)(), {ContingencyInputPolicy.ErrorIssue("contingency.options", "Options are required.", "options")})
            Dim issues = ValidateInput(input, options.Alpha), tables As New List(Of ContingencySemanticTable)
            If New SpreadsheetValidationReport(issues).HasErrors Then Return New ContingencyReportResult(tables, issues)
            Dim tab = input.CoreCounts
            Try
                Dim head(tab.GetLength(1) - 1) As String
                head(0) = "Analyzed table"
                tables.Add(New ContingencySemanticTable(head, ObjectCounts(tab)))
                Dim chi = contingencytable.Chi2TESTindependence(tab)
                tables.Add(Table("Pearson's Chi-squared Test", {{"Chi-Square", chi.Item1.TestStatistics1}, {"two-sided P-value", chi.Item1.Pvalue}}, 2))
                If options.Nominal Then tables.Add(Nominal(chi.Item2, chi.Item3, chi.Item4))
                If options.Exact Then
                    Dim p As Object
                    Try
                        If exactEvaluator IsNot Nothing Then
                            p = exactEvaluator(DirectCast(tab.Clone(), Integer(,)))
                        Else
                            Dim engine As New contingencytable.FisherExactEngine(tab)
                            engine.Run() : p = engine.PValue
                        End If
                    Catch
                        p = ".error. not possible to compute"
                    End Try
                    tables.Add(Table("Fisher-Freeman-Halton Exact Test", {{"two-sided P-value", p}}, 1))
                End If
                If options.Ordinal Then
                    Dim ordinal = contingencytable.cTableORDINALassoc(tab, options.Alpha)
                    Dim ciLabel = $"{(1.0 - options.Alpha) * 100.0:0.##}% CI"
                    tables.Add(Table("Measures of Ordinal Association", {
                        {"Kendall's tau-b", ordinal.Item1.TestStatistics1}, {"Std.Err.", ordinal.Item1.DF1}, {ciLabel, ordinal.Item1.strSpecialInformation}, {"two-sided P-value", ordinal.Item1.Pvalue},
                        {"Kendall's tau-c", ordinal.Item2.TestStatistics1}, {"Std.Err.", ordinal.Item2.DF1}, {ciLabel, ordinal.Item2.strSpecialInformation}, {"two-sided P-value", ordinal.Item2.Pvalue},
                        {"Goodman-Kruskal's Gamma", ordinal.Item3.TestStatistics1}, {"Std.Err.", ordinal.Item3.DF1}, {ciLabel, ordinal.Item3.strSpecialInformation}, {"two-sided P-value", ordinal.Item3.Pvalue},
                        {"Somers' D (columns as dependent var.)", ordinal.Item4.TestStatistics1}, {"Std.Err.", ordinal.Item4.DF1}, {ciLabel, ordinal.Item4.strSpecialInformation}, {"two-sided P-value", ordinal.Item4.Pvalue}}, 4, 8, 12, 16))
                End If
                If options.Trend Then
                    Dim rows = tab.GetLength(0), cols = tab.GetLength(1)
                    If Not (rows = 2 OrElse cols = 2) OrElse rows + cols < 5 Then
                        Warn(issues, "contingency.trend.shape", "Cannot compute Cochran-Armitage test for linear trend because of inapropriate table dimensions.")
                    Else
                        Dim value = contingencytable.CochranArmitage(If(rows = 2, Transpose(tab), tab))
                        tables.Add(Table("Cochran-Armitage Test for Linear Trend", {{"Chi2 for Linear Trend", value.TestStatistics1},
                            {"two-sided P-value for Linear Trend", value.Pvalue}, {"Chi2 for Departure from Linear Trend", value.TestStatistics2},
                            {"two-sided P-value for Linear Trend", value.Pvalue2}}, 2, 4))
                    End If
                End If
            Catch ex As ArgumentException
                issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.core", ex.Message, "analysis"))
            Catch ex As ArithmeticException
                issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.core", ex.Message, "analysis"))
            Catch ex As InvalidOperationException
                issues.Add(ContingencyInputPolicy.ErrorIssue("contingency.core", ex.Message, "analysis"))
            End Try
            Return New ContingencyReportResult(tables, issues)
        End Function

        Private Shared Function Nominal(v As Double, coefficient As Double, phi As Double) As ContingencySemanticTable
            Return Table("Measures of Nominal Association", {{"Cramer's V", v}, {"Pearson's contingency coefficient", coefficient}, {"Phi", phi}})
        End Function
        Private Shared Function ObjectCounts(tab As Integer(,)) As Object(,)
            Dim body(tab.GetLength(0) - 1, tab.GetLength(1) - 1) As Object
            For r = 0 To tab.GetLength(0) - 1
                For c = 0 To tab.GetLength(1) - 1
                    body(r, c) = tab(r, c)
                Next
            Next
            Return body
        End Function
        Private Shared Function Transpose(tab As Integer(,)) As Integer(,)
            Dim result(tab.GetLength(1) - 1, tab.GetLength(0) - 1) As Integer
            For r = 0 To tab.GetLength(0) - 1
                For c = 0 To tab.GetLength(1) - 1
                    result(c, r) = tab(r, c)
                Next
            Next
            Return result
        End Function
    End Class
End Namespace
