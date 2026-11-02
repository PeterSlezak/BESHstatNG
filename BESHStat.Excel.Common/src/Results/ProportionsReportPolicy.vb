Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    Public NotInheritable Class ProportionsReportPolicy
        Private Sub New()
        End Sub
        Public Shared Function Analyze(request As ProportionsRequest) As ContingencyReportResult
            Dim tables As New List(Of ContingencySemanticTable), issues As New List(Of SpreadsheetValidationIssue), metrics As New List(Of AnalysisMetricArtifact)
            If request Is Nothing Then
                issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.input", "Validated proportions request is required.", "input"))
                Return New ContingencyReportResult(tables, issues)
            End If
            Dim n1 = request.Total1, r1 = request.Responders1, n2 = request.Total2, r2 = request.Responders2, alpha = request.Alpha
            Dim ciLabel = $"{100.0 * (1.0 - alpha):0.##}% CI"
            Try
                If request.Mode = ProportionsMode.SingleSample Then
                    Dim value = contingencytable.SingleProportion(r1, n1, alpha)
                    Dim p = Math.Min(1.0, distributions.BinomDist(Math.Min(r1, n1 - r1), n1, 0.5, True) * 2.0)
                    tables.Add(T("Single proportion", {{"Total Number of Subjects", n1}, {"Number of Responders", r1}, {"Proportion", value.Estimate},
                        {ciLabel, value.strConfidenceInterval(CIformat.LL_to_UL)}, {"two-sided P-value", p}}, 5))
                    CI(metrics, "single", value)
                ElseIf request.Mode = ProportionsMode.Independent Then
                    Select Case request.Hypothesis
                        Case IndependentProportionsHypothesis.Noninferiority
                            Dim ni = equivalencetests.TestIndependentProportionsNonInferiority(r1, n1, r2, n2, request.Margin, alpha)
                            tables.Add(T("Two independent proportions - Noninferiority", {
                                {"Control / Reference sample size", ni.NumberOfControls}, {"Control / Reference responders", ni.ControlResponders}, {"Control / Reference proportion", ni.ControlProportion},
                                {"Experimental / Test sample size", ni.NumberOfExperimental}, {"Experimental / Test responders", ni.ExperimentalResponders}, {"Experimental / Test proportion", ni.ExperimentalProportion},
                                {"Difference (Experimental - Control)", ni.DifferenceExperimentalMinusControl}, {"Noninferiority margin", ni.NonInferiorityMargin}, {"Noninferiority limit", ni.NonInferiorityLimit},
                                {$"{100.0 * (1.0 - 2.0 * alpha):0.##}% CI for proportion difference", ni.TwoSidedEquivalentConfidenceInterval.strConfidenceInterval(CIformat.LL_to_UL)},
                                {"Lower one-sided confidence limit", ni.LowerOneSidedConfidenceLimit}, {"Z statistic", ni.ZStatistic}, {"One-sided P-value", ni.PValue}, {"Conclusion", ni.Conclusion}}, 13))
                            CI(metrics, "noninferiority", ni.TwoSidedEquivalentConfidenceInterval)
                            metrics.Add(New AnalysisMetricArtifact("noninferiority.lower-one-sided", ni.LowerOneSidedConfidenceLimit, AnalysisMetricRole.Statistic))
                        Case IndependentProportionsHypothesis.Equivalence
                            Dim eq = equivalencetests.TestIndependentProportionsEquivalence(r1, n1, r2, n2, -request.Margin, request.Margin, alpha)
                            tables.Add(T("Two independent proportions - Equivalence", {
                                {"Control / Reference sample size", eq.NumberOfControls}, {"Control / Reference responders", eq.ControlResponders}, {"Control / Reference proportion", eq.ControlProportion},
                                {"Experimental / Test sample size", eq.NumberOfExperimental}, {"Experimental / Test responders", eq.ExperimentalResponders}, {"Experimental / Test proportion", eq.ExperimentalProportion},
                                {"Difference (Experimental - Control)", eq.DifferenceExperimentalMinusControl}, {"Lower equivalence margin", eq.LowerMargin}, {"Upper equivalence margin", eq.UpperMargin},
                                {$"{100.0 * (1.0 - 2.0 * alpha):0.##}% CI for proportion difference", eq.EquivalentConfidenceInterval.strConfidenceInterval(CIformat.LL_to_UL)},
                                {"Lower TOST Z statistic", eq.LowerComponentStatistic}, {"Lower TOST one-sided P-value", eq.LowerComponentPValue},
                                {"Upper TOST Z statistic", eq.UpperComponentStatistic}, {"Upper TOST one-sided P-value", eq.UpperComponentPValue}, {"TOST P-value", eq.TostPValue}, {"Conclusion", eq.Conclusion}}, 12, 14, 15))
                            CI(metrics, "equivalence", eq.EquivalentConfidenceInterval)
                        Case Else
                            Dim value = contingencytable.TwoIndependentProportions(r1, n1, r2, n2, alpha)
                            Dim tab = request.IndependentFisherTable()
                            Dim fisher = contingencytable.FisherExact2x2(tab(0, 0), tab(0, 1), tab(1, 0), tab(1, 1))
                            tables.Add(T("Two independent proportions", {{"Total Number of Subjects in Sample 1", n1}, {"Number of Responders in Sample 1", r1}, {"Proportion in Sample 1", r1 / n1},
                                {"Total Number of Subjects in Sample 2", n2}, {"Number of Responders in Sample 2", r2}, {"Proportion in Sample 2", r2 / n2},
                                {"Proportions Difference", value.Estimate}, {ciLabel, value.strConfidenceInterval(CIformat.LL_to_UL)}, {"Exact two-sided P-value", fisher.Pvalue}, {"Exact Mid two-sided P-value", fisher.Pvalue2}}, 9, 10))
                            CI(metrics, "independent", value)
                    End Select
                ElseIf request.Mode = ProportionsMode.Paired Then
                    Dim interval = contingencytable.PairedProportions(n1, r1, r2, request.Both, alpha)
                    Dim exact = contingencytable.Liddell_McNemar(request.CoreLiddellTable(), alpha)
                    Return ProjectPaired(request, interval, exact.Item1)
                End If
            Catch ex As ArgumentException
                issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.core", ex.Message, "analysis"))
            Catch ex As ArithmeticException
                issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.core", ex.Message, "analysis"))
            Catch ex As InvalidOperationException
                issues.Add(ContingencyInputPolicy.ErrorIssue("proportions.core", ex.Message, "analysis"))
            End Try
            Return New ContingencyReportResult(tables, issues, metrics)
        End Function

        ''' <summary>Owner-approved total responders/proportions; nine rows and p-value position retained.</summary>
        Public Shared Function ProjectPaired(request As ProportionsRequest, interval As ConfidenceIntervalResult, test As TestResult) As ContingencyReportResult
            If request Is Nothing OrElse request.Mode <> ProportionsMode.Paired OrElse interval Is Nothing OrElse test Is Nothing Then
                Return New ContingencyReportResult(Array.Empty(Of ContingencySemanticTable)(), {ContingencyInputPolicy.ErrorIssue("proportions.paired-evidence", "Paired request, Core interval and Core test evidence are required.", "analysis.paired")})
            End If
            If Not Finite(interval.Estimate) OrElse Not Finite(interval.LowerLimit) OrElse Not Finite(interval.UpperLimit) OrElse
                interval.LowerLimit > interval.UpperLimit OrElse Not Finite(test.Pvalue) OrElse test.Pvalue < 0 OrElse test.Pvalue > 1 Then
                Return New ContingencyReportResult(Array.Empty(Of ContingencySemanticTable)(), {ContingencyInputPolicy.ErrorIssue("proportions.paired-nonfinite", "Paired inference did not produce a finite ordered proportion interval and a valid exact p-value.", "analysis.paired")})
            End If
            Dim n = request.Total1, a = request.Responders1, b = request.Responders2, both = request.Both
            Dim table = T("Two paired proportions", {{"Total Number of Subjects", n}, {"Total Responders in the 1st Category", a + both},
                {"Proportion in the 1st Category", (a + both) / n}, {"Total Responders in the 2nd Category", b + both},
                {"Proportion in the 2nd Category", (b + both) / n}, {"Number of Responders in Both Categories", both},
                {"Proportions Difference", interval.Estimate}, {$"{100.0 * (1.0 - request.Alpha):0.##}% CI", interval.strConfidenceInterval(CIformat.LL_to_UL)},
                {"Two-sided P-value", test.Pvalue}}, 9)
            Dim metrics As New List(Of AnalysisMetricArtifact)
            CI(metrics, "paired", interval)
            Return New ContingencyReportResult({table}, Array.Empty(Of SpreadsheetValidationIssue)(), metrics)
        End Function

        Private Shared Function Finite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function

        Private Shared Function T(title As String, body As Object(,), ParamArray rows As Integer()) As ContingencySemanticTable
            Return New ContingencySemanticTable({title, ""}, body, rows)
        End Function
        Private Shared Sub CI(metrics As List(Of AnalysisMetricArtifact), name As String, value As ConfidenceIntervalResult)
            metrics.Add(New AnalysisMetricArtifact(name & ".estimate", value.Estimate, AnalysisMetricRole.Statistic))
            metrics.Add(New AnalysisMetricArtifact(name & ".lower", value.LowerLimit, AnalysisMetricRole.Statistic))
            metrics.Add(New AnalysisMetricArtifact(name & ".upper", value.UpperLimit, AnalysisMetricRole.Statistic))
        End Sub
    End Class
End Namespace
