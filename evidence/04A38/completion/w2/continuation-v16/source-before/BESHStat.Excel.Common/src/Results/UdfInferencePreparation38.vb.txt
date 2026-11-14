Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.equivalencetests
Imports BESHStatNG.nonparametric

Namespace ExcelCommon
    ''' <summary>Shared legacy inference preparation; statistical engines and numeric semantics remain in Core.</summary>
    Public NotInheritable Class UdfInferencePreparation38
        Private Sub New()
        End Sub

        Public Shared Function MeanNonInferiorityBody(result As MeanNonInferiorityResult, names() As String) As Object(,)
                Dim body As Object(,) = {
                    {"Control group", names(0)},
                    {"Experimental group", names(1)},
                    {"Variance model", If(result.AssumeEqualVariances, "Equal variances (pooled Student t)", "Unequal variances (Welch)")},
                    {"Observations in control", result.NumberOfControls},
                    {"Observations in experimental", result.NumberOfExperimental},
                    {"Mean control", result.MeanControl},
                    {"Mean experimental", result.MeanExperimental},
                    {"Difference (experimental - control)", result.DifferenceExperimentalMinusControl},
                    {"Standard error of the difference", result.StandardError},
                    {"Degrees of freedom", result.DegreesOfFreedom},
                    {"Non-inferiority margin magnitude", result.NonInferiorityMargin},
                    {"Null boundary on difference scale", result.NonInferiorityLimit},
                    {"One-sided alpha", result.AlphaOneSided},
                    {"Test statistic", result.TestStatistic},
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

        Public Shared Function MeanEquivalenceBody(result As MeanEquivalenceResult, names() As String) As Object(,)
                Dim body As Object(,) = {
                    {"Control group", names(0)},
                    {"Experimental group", names(1)},
                    {"Variance model", If(result.AssumeEqualVariances, "Equal variances (pooled Student t)", "Unequal variances (Welch)")},
                    {"Observations in control", result.NumberOfControls},
                    {"Observations in experimental", result.NumberOfExperimental},
                    {"Mean control", result.MeanControl},
                    {"Mean experimental", result.MeanExperimental},
                    {"Difference (experimental - control)", result.DifferenceExperimentalMinusControl},
                    {"Standard error of the difference", result.StandardError},
                    {"Degrees of freedom", result.DegreesOfFreedom},
                    {"Lower equivalence margin", result.LowerMargin},
                    {"Upper equivalence margin", result.UpperMargin},
                    {"One-sided alpha", result.AlphaOneSided},
                    {"Lower TOST statistic", result.LowerComponentStatistic},
                    {"Lower TOST p-value", result.LowerComponentPValue},
                    {"Upper TOST statistic", result.UpperComponentStatistic},
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

        Public Shared Function ApplyRepeatedCorrection(mdl As Global.BESHStatNG.parametric.OneWayRmANOVA, which As String) As Boolean
                Select Case LegacyUdfText38.CaseToken(which, {"", "NONE", "GG", "GREENHOUSE", "GREENHOUSE-GEISSER", "GREENHOUSEGEISSER", "HF", "HUYNH", "HUYNH-FELDT", "HUYNHFELDT", "BOTH", "ALL"})
                    Case "", "NONE"
                        ' no correction
                    Case "GG", "GREENHOUSE", "GREENHOUSE-GEISSER", "GREENHOUSEGEISSER"
                        mdl.GreenhouseGeisser()
                    Case "HF", "HUYNH", "HUYNH-FELDT", "HUYNHFELDT"
                        mdl.HuyhnFeldt()
                    Case "BOTH", "ALL"
                        mdl.GreenhouseGeisser()
                        mdl.HuyhnFeldt()
                    Case Else
                        Return False
                End Select
            Return True
        End Function

        Public Shared Function BuildRepeatedMcpTables(mat(,) As Double, names() As String, which As String, alphaValue As Double, ByRef stackAll As Boolean) As List(Of Global.BESHStatNG.ResultTable)
                Dim mdl As New parametric.OneWayRmANOVA(mat, names)
                mdl.compute()

            stackAll = False
                Select Case LegacyUdfText38.CaseToken(which, {"", "RM2", "TUKEYRM2", "TUKEY-RM2", "RECOMMENDED", "NOSPHERICITY", "NO-SPHERICITY", "TUKEY", "SPHERICITY", "ASSUME-SPHERICITY", "ASSUMESPHERICITY", "CLASSICAL", "ALL", "BOTH"})
                    Case "", "RM2", "TUKEYRM2", "TUKEY-RM2", "RECOMMENDED", "NOSPHERICITY", "NO-SPHERICITY"
                        mdl.TukeyKramerRM2(alphaValue)


                    Case "TUKEY", "SPHERICITY", "ASSUME-SPHERICITY", "ASSUMESPHERICITY", "CLASSICAL"
                        mdl.Tukey(alphaValue)


                    Case "ALL", "BOTH"
                        mdl.TukeyKramerRM2(alphaValue)
                        mdl.Tukey(alphaValue)

                        stackAll = True

                    Case Else
                        Return Nothing
                End Select
            Return mdl.wrapResults()
        End Function

        Public Shared Function NestedTableSelection(which As String) As Integer
            If LegacyUdfText38.EqualText(which, "") OrElse LegacyUdfText38.EqualText(which, "BOTH") OrElse LegacyUdfText38.EqualText(which, "ALL") Then
                Return 0
            ElseIf LegacyUdfText38.EqualText(which, "MAIN") OrElse LegacyUdfText38.EqualText(which, "CLASSICAL") Then
                Return 1
            ElseIf LegacyUdfText38.EqualText(which, "SATTERTHWAITE") OrElse LegacyUdfText38.EqualText(which, "SW") OrElse LegacyUdfText38.EqualText(which, "ADJUSTED") Then
                Return 2
            Else
                Return -1
            End If
        End Function

        Public Shared Function IndependentTTableSelection(which As String) As Integer
            Select Case LegacyUdfText38.CaseToken(which, {"", "BOTH", "ALL", "EQUAL", "POOLED", "STUDENT", "ASSUME-EQUAL", "ASSUMEEQUAL", "UNEQUAL", "WELCH", "ASSUME-UNEQUAL", "ASSUMEUNEQUAL"})
                Case "", "BOTH", "ALL" : Return 0
                Case "EQUAL", "POOLED", "STUDENT", "ASSUME-EQUAL", "ASSUMEEQUAL" : Return 1
                Case "UNEQUAL", "WELCH", "ASSUME-UNEQUAL", "ASSUMEUNEQUAL" : Return 2
                Case Else : Return -1
            End Select
        End Function

        Public Shared Sub SplitCorrelationColumns(pairs(,) As Double, ByRef x() As Double, ByRef y() As Double)
            Dim n As Integer = pairs.GetLength(0)
            x = New Double(n - 1) {}
            y = New Double(n - 1) {}
            For i As Integer = 0 To n - 1
                x(i) = pairs(i, 0)
                y(i) = pairs(i, 1)
            Next
        End Sub

        Public Shared Function KruskalGroupNames(count As Integer) As String()
            Dim names(count - 1) As String
            For i As Integer = 0 To count - 1
                names(i) = "G" & (i + 1).ToString()
            Next
            Return names
        End Function

        Public Shared Function FriedmanGroupNames(count As Integer) As String()
            Dim names(count - 1) As String
                For j = 0 To count - 1
                    names(j) = $"G{j + 1}"
                Next
            Return names
        End Function

        Public Shared Function BuildKruskalMcpTables(data()() As Double, names() As String, alphaValue As Double) As List(Of Global.BESHStatNG.ResultTable)
                Dim test = New KruskallWalis(data, names)
                test.compute()
                test.MCP(alphaValue)

            Return test.wrapResults()
        End Function

        Public Shared Function BuildFriedmanMcpTables(mat(,) As Double, names() As String, alphaValue As Double) As List(Of Global.BESHStatNG.ResultTable)
                Dim test = New Friedman(mat, names)
                test.compute()
                test.MCP(alphaValue)

            Return test.wrapResults()
        End Function
    End Class
End Namespace
