Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG
Imports BESHStatNG.contingencytable
Imports BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38

Namespace ExcelCommon
    Public NotInheritable Class UdfContingencyPreparation38
        Private Sub New()
        End Sub

        Public Shared Function ChiSquaredBody(tab(,) As Integer, isFinite As Func(Of Double, Boolean)) As Object(,)
                Dim res = contingencytable.Chi2TESTindependence(tab)
                If res.Item1 Is Nothing OrElse Not IsFinite(res.Item1.TestStatistics1) OrElse Not IsFinite(res.Item1.Pvalue) Then
                    Return Nothing
                End If

                Dim df As Integer = EffectiveDf(tab)
                Dim reportBody As Object(,) = {
                    {"Chi-square", res.Item1.TestStatistics1},
                    {"Degrees of freedom", df},
                    {"Two-sided p-value", res.Item1.Pvalue}
                }
                Return reportBody
        End Function

        Public Shared Function NominalAssociationBody(tab(,) As Integer) As Object(,)
                Dim res = contingencytable.Chi2TESTindependence(tab)
                If res.Item1 Is Nothing Then Return Nothing

                Dim reportBody As Object(,) = {
                    {"Cramer's V", res.Item2},
                    {"Pearson's contingency coefficient", res.Item3},
                    {"Phi", res.Item4}
                }
                Return reportBody
        End Function

        Public Shared Function Fisher2x2Body(tab(,) As Integer, isFinite As Func(Of Double, Boolean)) As Object(,)
                Dim res As TestResult = contingencytable.FisherExact2x2(tab(0, 0), tab(0, 1), tab(1, 0), tab(1, 1))
                If res Is Nothing OrElse Not IsFinite(res.Pvalue) Then Return Nothing

                Dim reportBody As Object(,) = {
                    {"One-sided exact p-value", res.PvalueLowerSide},
                    {"Two-sided exact p-value", res.Pvalue},
                    {"One-sided mid-p value", res.pValueExactLowerSide},
                    {"Two-sided mid-p value", res.Pvalue2}
                }
                Return reportBody
        End Function

        Public Shared Function FreemanHaltonBody(tab(,) As Integer, isFinite As Func(Of Double, Boolean)) As Object(,)
                Dim exact As New FisherExactEngine(tab)
                exact.Run()

                If Not IsFinite(exact.PObserved) OrElse Not IsFinite(exact.PValue) Then Return Nothing

                Dim reportBody As Object(,) = {
                    {"Observed-table probability", exact.PObserved},
                    {"Two-sided exact p-value", exact.PValue}
                }
                Return reportBody
        End Function

        Public Shared Function McNemarBody(tab(,) As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)) As Object(,)
            Dim res = contingencytable.Liddell_McNemar(tab, alphaValue)
            If res.Item1 Is Nothing OrElse res.Item2 Is Nothing OrElse Not IsFinite(res.Item1.Pvalue) Then Return Nothing

            Dim ciText As String = If(String.IsNullOrWhiteSpace(res.Item2.strConfidenceInterval(CIformat.LL_to_UL)), "", res.Item2.strConfidenceInterval(CIformat.LL_to_UL))
            Dim body As Object(,) = {
                {"Two-sided exact p-value", res.Item1.Pvalue},
                {"Matched-pairs odds ratio", res.Item2.Estimate},
                {res.Item2.CIlabel, ciText}
            }
            Return body
        End Function

        Public Shared Function OddsRatioBody(tab(,) As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)) As Object(,)
            Dim res = contingencytable.OddsRatio(tab, alphaValue)
            If res.Item1 Is Nothing OrElse res.Item2 Is Nothing OrElse Not IsFinite(res.Item1.Estimate) Then Return Nothing

            Dim body As Object(,) = {
                {"Odds ratio", res.Item1.Estimate},
                {res.Item1.CIlabel & " (Woolf)", res.Item1.strConfidenceInterval(CIformat.LL_to_UL)},
                {res.Item2.CIlabel & " (Cornfield)", res.Item2.strConfidenceInterval(CIformat.LL_to_UL)}
            }
            Return body
        End Function

        Public Shared Function RiskRatioBody(tab(,) As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)) As Object(,)
            Dim res As ConfidenceIntervalResult = contingencytable.RiskRatio(tab, alphaValue)
            If res Is Nothing OrElse Not IsFinite(res.Estimate) Then Return Nothing

            Dim body As Object(,) = {
                {"Risk ratio", res.Estimate},
                {res.CIlabel, res.strConfidenceInterval(CIformat.LL_to_UL)}
            }
            Return body
        End Function

        Public Shared Function TrendBody(tab(,) As Integer, rows As Integer, cols As Integer, isFinite As Func(Of Double, Boolean)) As Object(,)
            Dim trendTable(,) As Integer = tab
            If rows = 2 AndAlso cols > 2 Then
                trendTable = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TransposeIntMatrix(tab)
            End If

            Dim res As TestResult = contingencytable.CochranArmitage(trendTable)
            If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then Return Nothing

            Dim body As Object(,) = {
                {"Chi-square for linear trend", res.TestStatistics1},
                {"Two-sided p-value for linear trend", res.Pvalue},
                {"Chi-square for departure from linear trend", res.TestStatistics2},
                {"Two-sided p-value for departure", res.Pvalue2}
            }
            Return body
        End Function

        Public Shared Function OrdinalAssociationBody(tab(,) As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)) As Object(,)
            Dim res = contingencytable.cTableORDINALassoc(tab, alphaValue)
            If res.Item1 Is Nothing OrElse Not IsFinite(res.Item1.TestStatistics1) OrElse Not IsFinite(res.Item1.Pvalue) Then Return Nothing

            Dim ciLabel As String = $"{(1.0R - alphaValue) * 100.0R:0.##}% CI"
            Dim body As Object(,) = {
                {"Kendall's tau-b", res.Item1.TestStatistics1},
                {"Std.Err.", res.Item1.DF1},
                {ciLabel, res.Item1.strSpecialInformation},
                {"Two-sided p-value", res.Item1.Pvalue},
                {"Kendall's tau-c", res.Item2.TestStatistics1},
                {"Std.Err.", res.Item2.DF1},
                {ciLabel, res.Item2.strSpecialInformation},
                {"Two-sided p-value", res.Item2.Pvalue},
                {"Goodman-Kruskal's gamma", res.Item3.TestStatistics1},
                {"Std.Err.", res.Item3.DF1},
                {ciLabel, res.Item3.strSpecialInformation},
                {"Two-sided p-value", res.Item3.Pvalue},
                {"Somers' D (columns as dependent variable)", res.Item4.TestStatistics1},
                {"Std.Err.", res.Item4.DF1},
                {ciLabel, res.Item4.strSpecialInformation},
                {"Two-sided p-value", res.Item4.Pvalue}
            }
            Return body
        End Function

        Public Shared Function MantelHaenszelBody(mat(,) As Double, alphaValue As Double, isFinite As Func(Of Double, Boolean)) As Object(,)
            Dim res = contingencytable.MantelHaenszel(mat, alphaValue)
            If res.Item1 Is Nothing OrElse res.Item2 Is Nothing OrElse Not IsFinite(res.Item1.TestStatistics1) Then Return Nothing

            Dim body As Object(,) = {
                {"Chi-square", res.Item1.TestStatistics1},
                {"Degrees of freedom", 1},
                {"Two-sided p-value", res.Item1.Pvalue},
                {"Common odds ratio", res.Item2.Estimate},
                {res.Item2.CIlabel, res.Item2.strConfidenceInterval(CIformat.LL_to_UL)}
            }
            Return body
        End Function

        Public Shared Function SingleProportionBody(x As Integer, n As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)) As Object(,)
            Dim ci As ConfidenceIntervalResult = contingencytable.SingleProportion(x, n, alphaValue)
            If ci Is Nothing OrElse Not IsFinite(ci.Estimate) OrElse Not IsFinite(ci.LowerLimit) OrElse Not IsFinite(ci.UpperLimit) Then
                Return Nothing
            End If

            Dim body As Object(,) = {
                {"Total number of observations", n},
                {"Number of responders", x},
                {"Estimated proportion", ci.Estimate},
                {"Lower confidence limit", ci.LowerLimit},
                {"Upper confidence limit", ci.UpperLimit},
                {ci.CIlabel, ci.strConfidenceInterval(CIformat.LL_to_UL)}
            }
            Return body
        End Function

        Public Shared Function IndependentProportionsBody(x1 As Integer, n1 As Integer, x2 As Integer, n2 As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)) As Object(,)
            Dim ci As ConfidenceIntervalResult = contingencytable.TwoIndependentProportions(x1, n1, x2, n2, alphaValue)
            If ci Is Nothing OrElse Not IsFinite(ci.Estimate) OrElse Not IsFinite(ci.LowerLimit) OrElse Not IsFinite(ci.UpperLimit) Then
                Return Nothing
            End If

            Dim p1 As Double = CDbl(x1) / CDbl(n1)
            Dim p2 As Double = CDbl(x2) / CDbl(n2)

            Dim body As Object(,) = {
                {"Total observations in sample 1", n1},
                {"Responders in sample 1", x1},
                {"Estimated proportion in sample 1", p1},
                {"Total observations in sample 2", n2},
                {"Responders in sample 2", x2},
                {"Estimated proportion in sample 2", p2},
                {"Estimated difference (p1 - p2)", ci.Estimate},
                {"Lower confidence limit", ci.LowerLimit},
                {"Upper confidence limit", ci.UpperLimit},
                {ci.CIlabel, ci.strConfidenceInterval(CIformat.LL_to_UL)}
            }
            Return body
        End Function

        Public Shared Function PairedProportionsBody(n As Integer, only1 As Integer, only2 As Integer, both As Integer, alphaValue As Double, isFinite As Func(Of Double, Boolean)) As Object(,)
            Dim ci As ConfidenceIntervalResult = contingencytable.PairedProportions(n, only1, only2, both, alphaValue)
            If ci Is Nothing OrElse Not IsFinite(ci.Estimate) OrElse Not IsFinite(ci.LowerLimit) OrElse Not IsFinite(ci.UpperLimit) Then
                Return Nothing
            End If

            Dim p1 As Double = CDbl(only1 + both) / CDbl(n)
            Dim p2 As Double = CDbl(only2 + both) / CDbl(n)

            Dim body As Object(,) = {
                {"Total paired observations", n},
                {"Responders only in the first condition", only1},
                {"Responders only in the second condition", only2},
                {"Responders in both conditions", both},
                {"Estimated proportion in the first condition", p1},
                {"Estimated proportion in the second condition", p2},
                {"Estimated difference (p1 - p2)", ci.Estimate},
                {"Lower confidence limit", ci.LowerLimit},
                {"Upper confidence limit", ci.UpperLimit},
                {ci.CIlabel, ci.strConfidenceInterval(CIformat.LL_to_UL)}
            }
            Return body
        End Function

        Public Shared Function ProportionsNonInferiorityBody(x0 As Integer, n0 As Integer, x1 As Integer, n1 As Integer, marginValue As Double, alphaValue As Double) As Object(,)
            Dim result As equivalencetests.ProportionNonInferiorityResult = equivalencetests.EquivalenceNonInferiorityMethods.TestIndependentProportionsNonInferiority(
                x0, n0, x1, n1, marginValue, alphaValue)

            Dim body As Object(,) = {
                {"Control observations", result.NumberOfControls},
                {"Control responders", result.ControlResponders},
                {"Control proportion", result.ControlProportion},
                {"Experimental observations", result.NumberOfExperimental},
                {"Experimental responders", result.ExperimentalResponders},
                {"Experimental proportion", result.ExperimentalProportion},
                {"Difference (experimental - control)", result.DifferenceExperimentalMinusControl},
                {"Standard error of the difference", result.StandardError},
                {"Non-inferiority margin magnitude", result.NonInferiorityMargin},
                {"Null boundary on difference scale", result.NonInferiorityLimit},
                {"One-sided alpha", result.AlphaOneSided},
                {"Z statistic", result.ZStatistic},
                {"One-sided p-value", result.PValue},
                {"Lower one-sided confidence limit", result.LowerOneSidedConfidenceLimit},
                {UdfOutputTablesReports38.SafeCiLabel(result.TwoSidedEquivalentConfidenceInterval), UdfOutputTablesReports38.SafeCiText(result.TwoSidedEquivalentConfidenceInterval)},
                {"Point estimate within stated limits", result.CiAssessment.IsPointEstimateWithinMargins},
                {"Confidence interval within stated limits", result.CiAssessment.IsConfidenceIntervalWithinMargins},
                {"Lower-bound non-inferiority supported by CI", result.CiAssessment.SupportsLowerNonInferiority},
                {"Upper-bound non-inferiority supported by CI", result.CiAssessment.SupportsUpperNonInferiority},
                {"Conclusion", result.Conclusion}
            }
            Return body
        End Function

        Public Shared Function ProportionsEquivalenceBody(x0 As Integer, n0 As Integer, x1 As Integer, n1 As Integer, lowerValue As Double, upperValue As Double, alphaValue As Double) As Object(,)
            Dim result As equivalencetests.ProportionEquivalenceResult = equivalencetests.EquivalenceNonInferiorityMethods.TestIndependentProportionsEquivalence(
                x0, n0, x1, n1, lowerValue, upperValue, alphaValue)

            Dim body As Object(,) = {
                {"Control observations", result.NumberOfControls},
                {"Control responders", result.ControlResponders},
                {"Control proportion", result.ControlProportion},
                {"Experimental observations", result.NumberOfExperimental},
                {"Experimental responders", result.ExperimentalResponders},
                {"Experimental proportion", result.ExperimentalProportion},
                {"Difference (experimental - control)", result.DifferenceExperimentalMinusControl},
                {"Standard error of the difference", result.StandardError},
                {"Lower equivalence margin", result.LowerMargin},
                {"Upper equivalence margin", result.UpperMargin},
                {"One-sided alpha", result.AlphaOneSided},
                {"Lower TOST z statistic", result.LowerComponentStatistic},
                {"Lower TOST p-value", result.LowerComponentPValue},
                {"Upper TOST z statistic", result.UpperComponentStatistic},
                {"Upper TOST p-value", result.UpperComponentPValue},
                {"TOST p-value = max(component p-values)", result.TostPValue},
                {UdfOutputTablesReports38.SafeCiLabel(result.EquivalentConfidenceInterval), UdfOutputTablesReports38.SafeCiText(result.EquivalentConfidenceInterval)},
                {"Point estimate within margins", result.CiAssessment.IsPointEstimateWithinMargins},
                {"Confidence interval within margins", result.CiAssessment.IsConfidenceIntervalWithinMargins},
                {"Lower margin supported by CI", result.CiAssessment.SupportsLowerNonInferiority},
                {"Upper margin supported by CI", result.CiAssessment.SupportsUpperNonInferiority},
                {"Supports equivalence", result.SupportsEquivalence},
                {"Conclusion", result.Conclusion}
            }
            Return body
        End Function
    End Class
End Namespace
