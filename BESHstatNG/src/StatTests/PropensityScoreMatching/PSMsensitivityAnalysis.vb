Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace CausalInference

    ''' <summary>
    ''' Direction used for matched-pair Rosenbaum sensitivity calculations.
    ''' Greater tests whether treated outcomes tend to be larger than matched controls;
    ''' Less tests whether treated outcomes tend to be smaller; TwoSided reports twice
    ''' the smaller one-sided bound, truncated at 1.
    ''' </summary>
    Public Enum PsmSensitivityAlternative
        Greater = 0
        Less = 1
        TwoSided = 2
    End Enum

    ''' <summary>
    ''' One row of matched-pair sensitivity analysis over a specified Gamma value.
    ''' Gamma = 1 is the no-hidden-bias randomization benchmark. Higher Gamma values
    ''' represent progressively stronger unmeasured confounding within matched sets.
    ''' </summary>
    Public Class PsmRosenbaumGammaRow
        Public Property Gamma As Double
        Public Property LowerTailProbability As Double
        Public Property UpperTailProbability As Double
        Public Property LowerBoundPValue As Double
        Public Property UpperBoundPValue As Double
        Public Property HodgesLehmannShift As Double
        Public Property Flag As String = ""
    End Class

    ''' <summary>
    ''' Result container for matched-pair Rosenbaum sensitivity analysis.
    ''' </summary>
    Public Class PsmRosenbaumSensitivityResult
        Public Property Alternative As PsmSensitivityAlternative
        Public Property Alpha As Double = 0.05
        Public Property InformativePairs As Integer
        Public Property PositiveDifferences As Integer
        Public Property NegativeDifferences As Integer
        Public Property TiedDifferences As Integer
        Public Property MeanDifference As Double
        Public Property MedianDifference As Double
        Public Property TippingPointGamma As Double = Double.NaN
        Public Property Rows As New List(Of PsmRosenbaumGammaRow)()
        Public Property Warnings As New List(Of String)()
    End Class

    ''' <summary>
    ''' Standardized sensitivity summary for an estimated effect. These values are not a
    ''' substitute for a matched-pair Rosenbaum analysis, but provide a compact diagnostic
    ''' for reporting and for GUI/UDF front ends.
    ''' </summary>
    Public Class PsmEffectSensitivitySummary
        Public Property Method As String = ""
        Public Property Estimand As PsmEstimand
        Public Property Estimate As Double
        Public Property StandardError As Double
        Public Property ZStatistic As Double
        Public Property TwoSidedPValue As Double
        Public Property ConfidenceLower As Double
        Public Property ConfidenceUpper As Double
        Public Property EffectCrossesZero As Boolean
        Public Property Warning As String = ""
    End Class

    ''' <summary>
    ''' Backend-only sensitivity helpers. The main implementation is a Rosenbaum-style
    ''' sensitivity analysis for matched pairs using sign-test bounds over Gamma values.
    ''' This is intentionally independent of Excel-DNA and WinForms so it can be reused by
    ''' the GUI, UDF and unit-test projects.
    ''' </summary>
    Public NotInheritable Class PsmSensitivityAnalysis
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Computes Rosenbaum matched-pair sensitivity bounds using the matched treatment-control
        ''' outcome differences. Only matched sets with exactly one treated row and exactly one
        ''' control row are used. Ties are counted and excluded from the sign test.
        ''' </summary>
        Public Shared Function RosenbaumMatchedPairs(input As PsmInputData,
                                                     matches As List(Of PsmMatchLink),
                                                     Optional maxGamma As Double = 3.0,
                                                     Optional gammaStep As Double = 0.1,
                                                     Optional alpha As Double = 0.05,
                                                     Optional alternative As PsmSensitivityAlternative = PsmSensitivityAlternative.TwoSided) As PsmRosenbaumSensitivityResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If matches Is Nothing Then Throw New ArgumentNullException("matches")
            If input.Outcome Is Nothing OrElse input.Outcome.Length <> input.RowCount Then Throw New ArgumentException("Outcome is required and must match the input row count.")
            If maxGamma < 1.0 OrElse Double.IsNaN(maxGamma) Then maxGamma = 1.0
            If gammaStep <= 0.0 OrElse Double.IsNaN(gammaStep) Then gammaStep = 0.1
            If alpha <= 0.0 OrElse alpha >= 1.0 OrElse Double.IsNaN(alpha) Then alpha = 0.05

            Dim result As New PsmRosenbaumSensitivityResult With {
                .Alternative = alternative,
                .Alpha = alpha
            }

            Dim diffs As Double() = MatchedPairDifferences(input, matches, result.Warnings)
            If diffs.Length = 0 Then
                result.Warnings.Add("No one-to-one matched sets with finite outcome differences were available for Rosenbaum sensitivity analysis.")
                Return result
            End If

            result.MeanDifference = diffs.Average()
            result.MedianDifference = PsmMath.Quantile(diffs, 0.5)
            result.PositiveDifferences = System.Linq.Enumerable.Count(diffs, Function(d) d > 0.0)
            result.NegativeDifferences = System.Linq.Enumerable.Count(diffs, Function(d) d < 0.0)
            result.TiedDifferences = System.Linq.Enumerable.Count(diffs, Function(d) Math.Abs(d) <= 0.000000000001)
            result.InformativePairs = result.PositiveDifferences + result.NegativeDifferences

            If result.InformativePairs = 0 Then
                result.Warnings.Add("All matched-pair differences are tied; sign-test sensitivity bounds are not informative.")
                Return result
            End If

            Dim statistic As Integer = StatisticForAlternative(result.PositiveDifferences, result.NegativeDifferences, alternative)
            Dim gamma As Double = 1.0
            Do While gamma <= maxGamma + gammaStep * 0.5
                Dim cappedGamma As Double = Math.Min(gamma, maxGamma)
                result.Rows.Add(BuildGammaRow(cappedGamma, result.InformativePairs, statistic, diffs, alternative, alpha))
                gamma += gammaStep
                If cappedGamma >= maxGamma Then Exit Do
            Loop

            result.TippingPointGamma = FindTippingPointGamma(result.Rows, alpha)
            If Double.IsNaN(result.TippingPointGamma) Then
                result.Warnings.Add("The upper-bound p-value did not exceed alpha within the requested Gamma grid.")
            End If

            If matches.GroupBy(Function(m) m.SetId).Any(Function(g) g.Count() <> 1) Then
                result.Warnings.Add("Some matched sets are not one-to-one. Rosenbaum sign-test sensitivity used only sets with exactly one treated-control pair.")
            End If

            Return result
        End Function

        ''' <summary>
        ''' Computes a compact normal-approximation sensitivity summary for any PSM effect result.
        ''' This is useful for reporting weighted, subclassification and doubly robust estimates.
        ''' </summary>
        Public Shared Function SummarizeEffectSensitivity(effect As PsmEffectResult) As PsmEffectSensitivitySummary
            If effect Is Nothing Then Return Nothing

            Dim summary As New PsmEffectSensitivitySummary With {
                .Method = effect.Method,
                .Estimand = effect.Estimand,
                .Estimate = effect.Estimate,
                .StandardError = effect.StandardError,
                .ConfidenceLower = effect.LowerConfidenceLimit,
                .ConfidenceUpper = effect.UpperConfidenceLimit
            }

            If Not AppInfrastructure.IsFinite(effect.Estimate) OrElse Not AppInfrastructure.IsFinite(effect.StandardError) OrElse effect.StandardError <= 0.0 Then
                summary.ZStatistic = Double.NaN
                summary.TwoSidedPValue = Double.NaN
                summary.EffectCrossesZero = False
                summary.Warning = "Finite estimate and positive standard error are required for the normal-approximation sensitivity summary."
                Return summary
            End If

            summary.ZStatistic = effect.Estimate / effect.StandardError
            summary.TwoSidedPValue = 2.0 * Math.Min(Global.BESHStatNG.distributions.Distributions.PNorm(summary.ZStatistic, 0.0, 1.0), 1.0 - Global.BESHStatNG.distributions.Distributions.PNorm(summary.ZStatistic, 0.0, 1.0))
            summary.EffectCrossesZero = AppInfrastructure.IsFinite(effect.LowerConfidenceLimit) AndAlso AppInfrastructure.IsFinite(effect.UpperConfidenceLimit) AndAlso effect.LowerConfidenceLimit <= 0.0 AndAlso effect.UpperConfidenceLimit >= 0.0
            Return summary
        End Function

        Private Shared Function MatchedPairDifferences(input As PsmInputData, matches As List(Of PsmMatchLink), warnings As List(Of String)) As Double()
            Dim diffs As New List(Of Double)()
            For Each setGroup In matches.GroupBy(Function(m) m.SetId)
                If setGroup.Count() <> 1 Then Continue For
                Dim m As PsmMatchLink = setGroup.First()
                If m.TreatedRowIndex < 0 OrElse m.TreatedRowIndex >= input.RowCount OrElse m.ControlRowIndex < 0 OrElse m.ControlRowIndex >= input.RowCount Then Continue For
                Dim yt As Double = input.Outcome(m.TreatedRowIndex)
                Dim yc As Double = input.Outcome(m.ControlRowIndex)
                If Not AppInfrastructure.IsFinite(yt) OrElse Not AppInfrastructure.IsFinite(yc) Then Continue For
                diffs.Add(yt - yc)
            Next
            Return diffs.ToArray()
        End Function

        Private Shared Function StatisticForAlternative(positive As Integer, negative As Integer, alternative As PsmSensitivityAlternative) As Integer
            Select Case alternative
                Case PsmSensitivityAlternative.Less
                    Return negative
                Case PsmSensitivityAlternative.TwoSided
                    Return Math.Max(positive, negative)
                Case Else
                    Return positive
            End Select
        End Function

        Private Shared Function BuildGammaRow(gamma As Double, informativePairs As Integer, statistic As Integer, diffs As Double(), alternative As PsmSensitivityAlternative, alpha As Double) As PsmRosenbaumGammaRow
            Dim lowerP As Double = 1.0 / (1.0 + gamma)
            Dim upperP As Double = gamma / (1.0 + gamma)
            Dim lowerBound As Double
            Dim upperBound As Double

            Select Case alternative
                Case PsmSensitivityAlternative.Less
                    lowerBound = LowerTail(statistic, informativePairs, lowerP)
                    upperBound = LowerTail(statistic, informativePairs, upperP)
                Case PsmSensitivityAlternative.TwoSided
                    Dim lowerGreater As Double = UpperTail(statistic, informativePairs, lowerP)
                    Dim upperGreater As Double = UpperTail(statistic, informativePairs, upperP)
                    Dim lowerLess As Double = LowerTail(informativePairs - statistic, informativePairs, lowerP)
                    Dim upperLess As Double = LowerTail(informativePairs - statistic, informativePairs, upperP)
                    lowerBound = Math.Min(1.0, 2.0 * Math.Min(lowerGreater, lowerLess))
                    upperBound = Math.Min(1.0, 2.0 * Math.Min(upperGreater, upperLess))
                Case Else
                    lowerBound = UpperTail(statistic, informativePairs, lowerP)
                    upperBound = UpperTail(statistic, informativePairs, upperP)
            End Select

            Dim flag As String = If(upperBound <= alpha, "Robust at alpha", "Not robust at alpha")
            Return New PsmRosenbaumGammaRow With {
                .Gamma = gamma,
                .LowerTailProbability = lowerP,
                .UpperTailProbability = upperP,
                .LowerBoundPValue = lowerBound,
                .UpperBoundPValue = upperBound,
                .HodgesLehmannShift = PsmMath.Quantile(diffs, 0.5),
                .Flag = flag
            }
        End Function

        Private Shared Function UpperTail(x As Integer, n As Integer, p As Double) As Double
            If x <= 0 Then Return 1.0
            If x > n Then Return 0.0
            Dim lower As Double = Global.BESHStatNG.distributions.Distributions.BinomDist(x - 1, n, p, True)
            If Not AppInfrastructure.IsFinite(lower) Then Return Double.NaN
            Return PsmMath.Clamp(1.0 - lower, 0.0, 1.0)
        End Function

        Private Shared Function LowerTail(x As Integer, n As Integer, p As Double) As Double
            If x < 0 Then Return 0.0
            If x >= n Then Return 1.0
            Dim lower As Double = Global.BESHStatNG.distributions.Distributions.BinomDist(x, n, p, True)
            If Not AppInfrastructure.IsFinite(lower) Then Return Double.NaN
            Return PsmMath.Clamp(lower, 0.0, 1.0)
        End Function

        Private Shared Function FindTippingPointGamma(rows As List(Of PsmRosenbaumGammaRow), alpha As Double) As Double
            If rows Is Nothing OrElse rows.Count = 0 Then Return Double.NaN
            For Each r In rows.OrderBy(Function(x) x.Gamma)
                If AppInfrastructure.IsFinite(r.UpperBoundPValue) AndAlso r.UpperBoundPValue > alpha Then Return r.Gamma
            Next
            Return Double.NaN
        End Function
    End Class

    ''' <summary>
    ''' Object-array table builders for sensitivity outputs. These are intentionally small and
    ''' Excel-neutral so they can be consumed by both GUI writers and UDF spill functions.
    ''' </summary>
    Public NotInheritable Class PsmSensitivityTables
        Private Sub New()
        End Sub

        Public Shared Function RosenbaumTable(result As PsmRosenbaumSensitivityResult) As Object(,)
            If result Is Nothing OrElse result.Rows Is Nothing OrElse result.Rows.Count = 0 Then Return PsmResult.EmptyTable("No Rosenbaum sensitivity result available")
            Dim table(result.Rows.Count, 7) As Object
            table(0, 0) = "Gamma" : table(0, 1) = "Lower p" : table(0, 2) = "Upper p" : table(0, 3) = "Lower p-value" : table(0, 4) = "Upper p-value" : table(0, 5) = "Median difference" : table(0, 6) = "Alpha" : table(0, 7) = "Flag"
            For i As Integer = 0 To result.Rows.Count - 1
                Dim r As PsmRosenbaumGammaRow = result.Rows(i)
                table(i + 1, 0) = r.Gamma
                table(i + 1, 1) = r.LowerTailProbability
                table(i + 1, 2) = r.UpperTailProbability
                table(i + 1, 3) = r.LowerBoundPValue
                table(i + 1, 4) = r.UpperBoundPValue
                table(i + 1, 5) = r.HodgesLehmannShift
                table(i + 1, 6) = result.Alpha
                table(i + 1, 7) = r.Flag
            Next
            Return table
        End Function

        Public Shared Function RosenbaumSummaryTable(result As PsmRosenbaumSensitivityResult) As Object(,)
            If result Is Nothing Then Return PsmResult.EmptyTable("No Rosenbaum sensitivity result available")
            Dim table(8, 1) As Object
            table(0, 0) = "Metric" : table(0, 1) = "Value"
            table(1, 0) = "Alternative" : table(1, 1) = result.Alternative.ToString()
            table(2, 0) = "Informative pairs" : table(2, 1) = result.InformativePairs
            table(3, 0) = "Positive differences" : table(3, 1) = result.PositiveDifferences
            table(4, 0) = "Negative differences" : table(4, 1) = result.NegativeDifferences
            table(5, 0) = "Tied differences" : table(5, 1) = result.TiedDifferences
            table(6, 0) = "Mean difference" : table(6, 1) = result.MeanDifference
            table(7, 0) = "Median difference" : table(7, 1) = result.MedianDifference
            table(8, 0) = "Tipping point Gamma" : table(8, 1) = result.TippingPointGamma
            Return table
        End Function

        Public Shared Function EffectSensitivitySummaryTable(summary As PsmEffectSensitivitySummary) As Object(,)
            If summary Is Nothing Then Return PsmResult.EmptyTable("No effect sensitivity summary available")
            Dim table(1, 8) As Object
            table(0, 0) = "Method" : table(0, 1) = "Estimand" : table(0, 2) = "Estimate" : table(0, 3) = "Std. Error" : table(0, 4) = "z" : table(0, 5) = "p-value" : table(0, 6) = "Lower 95%" : table(0, 7) = "Upper 95%" : table(0, 8) = "Warning"
            table(1, 0) = summary.Method
            table(1, 1) = summary.Estimand.ToString()
            table(1, 2) = summary.Estimate
            table(1, 3) = summary.StandardError
            table(1, 4) = summary.ZStatistic
            table(1, 5) = summary.TwoSidedPValue
            table(1, 6) = summary.ConfidenceLower
            table(1, 7) = summary.ConfidenceUpper
            table(1, 8) = summary.Warning
            Return table
        End Function
    End Class

End Namespace
