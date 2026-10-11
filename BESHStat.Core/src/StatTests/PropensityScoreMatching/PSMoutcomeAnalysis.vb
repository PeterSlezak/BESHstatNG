Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace CausalInference

    Public NotInheritable Class PsmOutcomeAnalysis
        Private Sub New()
        End Sub

        Public Shared Function EstimateMatchedEffect(input As PsmInputData, matches As List(Of PsmMatchLink), options As PsmOptions) As PsmEffectResult
            If input.Outcome Is Nothing OrElse matches Is Nothing OrElse matches.Count = 0 Then Return Nothing
            Dim setDiffs As New List(Of Double)()
            For Each g In matches.GroupBy(Function(m) m.SetId)
                Dim treatedRows = g.Select(Function(m) m.TreatedRowIndex).Distinct().ToList()
                Dim controlRows = g.Select(Function(m) m.ControlRowIndex).Distinct().ToList()
                If treatedRows.Count = 0 OrElse controlRows.Count = 0 Then Continue For
                Dim mt As Double = treatedRows.Select(Function(i) input.Outcome(i)).Average()
                Dim mc As Double = controlRows.Select(Function(i) input.Outcome(i)).Average()
                If AppInfrastructure.IsFinite(mt) AndAlso AppInfrastructure.IsFinite(mc) Then setDiffs.Add(mt - mc)
            Next
            If setDiffs.Count = 0 Then Return Nothing

            Dim est As Double = setDiffs.Average()
            Dim se As Double = Double.NaN
            If setDiffs.Count > 1 Then se = Math.Sqrt(PsmMath.Variance(setDiffs, True) / Convert.ToDouble(setDiffs.Count))
            Dim lower As Double = If(AppInfrastructure.IsFinite(se), est - 1.95996398454005 * se, Double.NaN)
            Dim upper As Double = If(AppInfrastructure.IsFinite(se), est + 1.95996398454005 * se, Double.NaN)

            Return New PsmEffectResult With {
                .Estimand = options.Estimand,
                .Method = "Matched mean difference",
                .OutcomeType = InferOutcomeType(input.Outcome),
                .Estimate = est,
                .StandardError = se,
                .LowerConfidenceLimit = lower,
                .UpperConfidenceLimit = upper,
                .TreatedMean = matches.Select(Function(m) input.Outcome(m.TreatedRowIndex)).Average(),
                .ControlMean = matches.Select(Function(m) input.Outcome(m.ControlRowIndex)).Average(),
                .EffectiveTreatedN = matches.Select(Function(m) m.TreatedRowIndex).Distinct().Count(),
                .EffectiveControlN = matches.Select(Function(m) m.ControlRowIndex).Distinct().Count(),
                .MatchedSets = setDiffs.Count
            }
        End Function

        Public Shared Function EstimateWeightedEffect(input As PsmInputData, weights As Double(), options As PsmOptions) As PsmEffectResult
            If input.Outcome Is Nothing OrElse weights Is Nothing OrElse weights.Length <> input.RowCount Then Return Nothing
            Dim yt As New List(Of Double)()
            Dim wt As New List(Of Double)()
            Dim yc As New List(Of Double)()
            Dim wc As New List(Of Double)()
            For i As Integer = 0 To input.RowCount - 1
                If weights(i) <= 0 OrElse Not AppInfrastructure.IsFinite(weights(i)) Then Continue For
                If input.Treatment(i) >= 0.5 Then
                    yt.Add(input.Outcome(i))
                    wt.Add(weights(i))
                Else
                    yc.Add(input.Outcome(i))
                    wc.Add(weights(i))
                End If
            Next
            If yt.Count = 0 OrElse yc.Count = 0 Then Return Nothing
            Dim yta As Double() = yt.ToArray()
            Dim wta As Double() = wt.ToArray()
            Dim yca As Double() = yc.ToArray()
            Dim wca As Double() = wc.ToArray()
            Dim mt As Double = PsmMath.WeightedMean(yta, wta)
            Dim mc As Double = PsmMath.WeightedMean(yca, wca)
            Dim vt As Double = PsmMath.WeightedVariance(yta, wta, True)
            Dim vc As Double = PsmMath.WeightedVariance(yca, wca, True)
            Dim nt As Double = PsmMath.EffectiveSampleSize(wta)
            Dim nc As Double = PsmMath.EffectiveSampleSize(wca)
            Dim se As Double = Double.NaN
            If nt > 1 AndAlso nc > 1 AndAlso AppInfrastructure.IsFinite(vt) AndAlso AppInfrastructure.IsFinite(vc) Then se = Math.Sqrt(vt / nt + vc / nc)
            Dim est As Double = mt - mc
            Return New PsmEffectResult With {
                .Estimand = options.Estimand,
                .Method = "Propensity-score weighting",
                .OutcomeType = InferOutcomeType(input.Outcome),
                .Estimate = est,
                .StandardError = se,
                .LowerConfidenceLimit = If(AppInfrastructure.IsFinite(se), est - 1.95996398454005 * se, Double.NaN),
                .UpperConfidenceLimit = If(AppInfrastructure.IsFinite(se), est + 1.95996398454005 * se, Double.NaN),
                .TreatedMean = mt,
                .ControlMean = mc,
                .EffectiveTreatedN = nt,
                .EffectiveControlN = nc,
                .MatchedSets = 0
            }
        End Function

        Public Shared Function InferOutcomeType(outcome As Double()) As PsmOutcomeType
            If outcome Is Nothing OrElse outcome.Length = 0 Then Return PsmOutcomeType.Auto
            For Each y In outcome
                If Not ((Math.Abs(y) < 0.000000000001) OrElse (Math.Abs(y - 1.0) < 0.000000000001)) Then Return PsmOutcomeType.Continuous
            Next
            Return PsmOutcomeType.Binary
        End Function
    End Class

End Namespace
