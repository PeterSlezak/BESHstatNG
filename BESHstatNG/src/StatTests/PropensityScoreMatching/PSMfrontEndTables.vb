Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Namespace CausalInference

    ''' <summary>
    ''' Front-end oriented PSM table builders. These are intentionally free of Excel-DNA,
    ''' COM and WinForms dependencies so the GUI, UDF layer and unit tests can reuse the
    ''' exact same audit/output tables.
    ''' </summary>
    Public NotInheritable Class PsmFrontEndTables
        Private Sub New()
        End Sub

        Public Shared Function OptionsTable(fitOptions As PsmComprehensiveFitOptions) As Object(,)
            If fitOptions Is Nothing OrElse fitOptions.StandardOptions Is Nothing Then Return PsmResult.EmptyTable("No PSM options available")
            Dim o As PsmOptions = fitOptions.StandardOptions
            Dim rows As New List(Of Object())()
            rows.Add(New Object() {"Run method", fitOptions.RunMethod.ToString()})
            rows.Add(New Object() {"Score method", o.ScoreMethod.ToString()})
            rows.Add(New Object() {"Estimand", o.Estimand.ToString()})
            rows.Add(New Object() {"Distance metric", o.DistanceMetric.ToString()})
            rows.Add(New Object() {"Matching ratio", o.MatchingRatio})
            rows.Add(New Object() {"With replacement", o.WithReplacement})
            rows.Add(New Object() {"Caliper scale", o.CaliperScale.ToString()})
            rows.Add(New Object() {"Caliper", If(Double.IsNaN(o.Caliper), "", CType(o.Caliper, Object))})
            rows.Add(New Object() {"Matching order", o.MatchingOrder.ToString()})
            rows.Add(New Object() {"Common support", o.CommonSupport.ToString()})
            rows.Add(New Object() {"Include intercept", o.IncludeIntercept})
            rows.Add(New Object() {"Standardize covariates", o.StandardizeCovariates})
            rows.Add(New Object() {"Logistic max iterations", o.LogisticMaxIterations})
            rows.Add(New Object() {"Logistic tolerance", o.LogisticTolerance})
            rows.Add(New Object() {"Logistic ridge penalty", o.LogisticRidgePenalty})
            rows.Add(New Object() {"SMD threshold", o.BalanceSmdThreshold})
            rows.Add(New Object() {"Variance ratio lower", o.BalanceVarianceRatioLower})
            rows.Add(New Object() {"Variance ratio upper", o.BalanceVarianceRatioUpper})
            rows.Add(New Object() {"Subclassification strata", o.SubclassificationStrata})
            rows.Add(New Object() {"Normalize weights to sample size", o.NormalizeWeightsToSampleSize})
            rows.Add(New Object() {"Trim propensity lower", o.TrimPropensityLower})
            rows.Add(New Object() {"Trim propensity upper", o.TrimPropensityUpper})
            rows.Add(New Object() {"Include doubly robust AIPW", fitOptions.IncludeDoublyRobustEstimate})
            rows.Add(New Object() {"Include overlap diagnostics", fitOptions.IncludeOverlapDiagnostics})
            rows.Add(New Object() {"Include weight diagnostics", fitOptions.IncludeWeightDiagnostics})
            rows.Add(New Object() {"Include Love-plot rows", fitOptions.IncludeLovePlotRows})
            rows.Add(New Object() {"Overlap bin count", fitOptions.OverlapBinCount})
            rows.Add(New Object() {"Love-plot threshold", fitOptions.LovePlotThreshold})
            rows.Add(New Object() {"Extreme weight cutoff", fitOptions.ExtremeWeightCutoff})
            If fitOptions.CoarseningSpec IsNot Nothing Then
                rows.Add(New Object() {"CEM propensity-score bins", fitOptions.CoarseningSpec.PropensityScoreBins})
                rows.Add(New Object() {"CEM estimand", fitOptions.CoarseningSpec.Estimand.ToString()})
            End If
            Return TwoColumnTable("Option", "Value", rows)
        End Function

        Public Shared Function SampleSizeTable(result As PsmResult) As Object(,)
            If result Is Nothing OrElse result.SampleSize Is Nothing Then Return PsmResult.EmptyTable("No sample-size summary available")
            Dim s As PsmSampleSizeSummary = result.SampleSize
            Dim rows As New List(Of Object()) From {
                New Object() {"Total rows", s.TotalRows},
                New Object() {"Treated rows", s.TreatedRows},
                New Object() {"Control rows", s.ControlRows},
                New Object() {"Eligible treated rows", s.EligibleTreatedRows},
                New Object() {"Eligible control rows", s.EligibleControlRows},
                New Object() {"Matched treated rows", s.MatchedTreatedRows},
                New Object() {"Matched control rows", s.MatchedControlRows},
                New Object() {"Matched sets", s.MatchedSets},
                New Object() {"Unmatched treated rows", s.UnmatchedTreatedRows},
                New Object() {"Unmatched control rows", s.UnmatchedControlRows},
                New Object() {"Dropped by common support", s.DroppedByCommonSupport},
                New Object() {"Dropped by trimming", s.DroppedByTrimming}
            }
            Return TwoColumnTable("Metric", "Value", rows)
        End Function

        Public Shared Function RowLevelAuditTable(input As PsmInputData, result As PsmResult, Optional sourceRowIds As Integer() = Nothing) As Object(,)
            If input Is Nothing OrElse result Is Nothing Then Return PsmResult.EmptyTable("No row-level PSM audit data available")
            Dim n As Integer = input.RowCount
            If n = 0 Then Return PsmResult.EmptyTable("No row-level PSM audit data available")

            Dim observationByRow As Dictionary(Of Integer, PsmObservation) = BuildObservationMap(result)
            Dim matchedSetsByRow As Dictionary(Of Integer, List(Of Integer)) = BuildMatchedSetMap(result)
            Dim matchedRoleByRow As Dictionary(Of Integer, String) = BuildMatchedRoleMap(result)

            Dim table(n, 16) As Object
            table(0, 0) = "Source Row"
            table(0, 1) = "Analysis Row"
            table(0, 2) = "ID"
            table(0, 3) = "Treatment"
            table(0, 4) = "Outcome"
            table(0, 5) = "Propensity Score"
            table(0, 6) = "Logit Propensity"
            table(0, 7) = "Exact Group"
            table(0, 8) = "Included by Common Support"
            table(0, 9) = "Included by Trimming"
            table(0, 10) = "Matching Weight"
            table(0, 11) = "Balancing Weight"
            table(0, 12) = "Matched Sets"
            table(0, 13) = "Matched Role"
            table(0, 14) = "Match Count"
            table(0, 15) = "Positive Analysis Weight"
            table(0, 16) = "Status"

            For i As Integer = 0 To n - 1
                Dim obs As PsmObservation = Nothing
                observationByRow.TryGetValue(i, obs)
                Dim score As Double = GetScore(result, i)
                Dim logitScore As Double = If(obs Is Nothing, If(AppInfrastructure.IsFinite(score), PsmMath.SafeLogit(score), Double.NaN), obs.LogitPropensityScore)
                Dim matchingWeight As Double = GetWeight(result.MatchingWeights, i)
                Dim balancingWeight As Double = GetWeight(result.BalancingWeights, i)
                Dim setsText As String = ""
                Dim matchCount As Integer = 0
                If matchedSetsByRow.ContainsKey(i) Then
                    Dim sets As List(Of Integer) = matchedSetsByRow(i)
                    matchCount = sets.Count
                    setsText = String.Join(",", sets.Select(Function(v) v.ToString()).ToArray())
                End If
                Dim role As String = ""
                If matchedRoleByRow.ContainsKey(i) Then role = matchedRoleByRow(i)
                Dim positiveAnalysisWeight As Boolean = (AppInfrastructure.IsFinite(matchingWeight) AndAlso matchingWeight > 0.0) OrElse (AppInfrastructure.IsFinite(balancingWeight) AndAlso balancingWeight > 0.0)

                table(i + 1, 0) = If(sourceRowIds IsNot Nothing AndAlso sourceRowIds.Length = n, sourceRowIds(i), i + 1)
                table(i + 1, 1) = i + 1
                table(i + 1, 2) = input.GetId(i)
                table(i + 1, 3) = input.Treatment(i)
                table(i + 1, 4) = If(input.Outcome Is Nothing, "", CType(input.Outcome(i), Object))
                table(i + 1, 5) = score
                table(i + 1, 6) = logitScore
                table(i + 1, 7) = If(obs Is Nothing, If(input.ExactGroupLabels Is Nothing, "", input.ExactGroupLabels(i)), obs.ExactGroupLabel)
                table(i + 1, 8) = If(obs Is Nothing, True, obs.IncludedByCommonSupport)
                table(i + 1, 9) = If(obs Is Nothing, True, obs.IncludedByTrimming)
                table(i + 1, 10) = matchingWeight
                table(i + 1, 11) = balancingWeight
                table(i + 1, 12) = setsText
                table(i + 1, 13) = role
                table(i + 1, 14) = matchCount
                table(i + 1, 15) = positiveAnalysisWeight
                table(i + 1, 16) = BuildRowStatus(obs, matchingWeight, balancingWeight, matchCount)
            Next
            Return table
        End Function

        Public Shared Function MatchedDatasetTable(input As PsmInputData, result As PsmResult, Optional sourceRowIds As Integer() = Nothing) As Object(,)
            If input Is Nothing OrElse result Is Nothing Then Return PsmResult.EmptyTable("No matched/weighted analysis dataset available")
            Dim selectedRows As New List(Of Integer)()
            For i As Integer = 0 To input.RowCount - 1
                Dim mw As Double = GetWeight(result.MatchingWeights, i)
                If AppInfrastructure.IsFinite(mw) AndAlso mw > 0.0 Then selectedRows.Add(i)
            Next
            If selectedRows.Count = 0 Then Return PsmResult.EmptyTable("No rows with positive matching weight")

            Dim p As Integer = input.CovariateCount
            Dim table(selectedRows.Count, 8 + p) As Object
            table(0, 0) = "Source Row"
            table(0, 1) = "Analysis Row"
            table(0, 2) = "ID"
            table(0, 3) = "Treatment"
            table(0, 4) = "Outcome"
            table(0, 5) = "Propensity Score"
            table(0, 6) = "Matching Weight"
            table(0, 7) = "Balancing Weight"
            table(0, 8) = "Exact Group"
            For j As Integer = 0 To p - 1
                table(0, 9 + j) = input.GetCovariateName(j)
            Next

            For r As Integer = 0 To selectedRows.Count - 1
                Dim i As Integer = selectedRows(r)
                table(r + 1, 0) = If(sourceRowIds IsNot Nothing AndAlso sourceRowIds.Length = input.RowCount, sourceRowIds(i), i + 1)
                table(r + 1, 1) = i + 1
                table(r + 1, 2) = input.GetId(i)
                table(r + 1, 3) = input.Treatment(i)
                table(r + 1, 4) = If(input.Outcome Is Nothing, "", CType(input.Outcome(i), Object))
                table(r + 1, 5) = GetScore(result, i)
                table(r + 1, 6) = GetWeight(result.MatchingWeights, i)
                table(r + 1, 7) = GetWeight(result.BalancingWeights, i)
                table(r + 1, 8) = If(input.ExactGroupLabels Is Nothing, "", input.ExactGroupLabels(i))
                For j As Integer = 0 To p - 1
                    table(r + 1, 9 + j) = input.Covariates(i, j)
                Next
            Next
            Return table
        End Function

        Public Shared Function SubclassTable(result As PsmResult) As Object(,)
            If result Is Nothing OrElse result.Subclasses Is Nothing OrElse result.Subclasses.Count = 0 Then Return PsmResult.EmptyTable("No subclassification rows available")
            Dim table(result.Subclasses.Count, 8) As Object
            table(0, 0) = "Stratum"
            table(0, 1) = "Lower Score"
            table(0, 2) = "Upper Score"
            table(0, 3) = "Treated N"
            table(0, 4) = "Control N"
            table(0, 5) = "Treated Outcome Mean"
            table(0, 6) = "Control Outcome Mean"
            table(0, 7) = "Effect"
            table(0, 8) = "Weight"
            For i As Integer = 0 To result.Subclasses.Count - 1
                Dim r As PsmSubclassRow = result.Subclasses(i)
                table(i + 1, 0) = r.Stratum
                table(i + 1, 1) = r.LowerScore
                table(i + 1, 2) = r.UpperScore
                table(i + 1, 3) = r.TreatedN
                table(i + 1, 4) = r.ControlN
                table(i + 1, 5) = r.TreatedOutcomeMean
                table(i + 1, 6) = r.ControlOutcomeMean
                table(i + 1, 7) = r.Effect
                table(i + 1, 8) = r.Weight
            Next
            Return table
        End Function

        Public Shared Function EffectSensitivitySummaryTable(result As PsmComprehensiveResult) As Object(,)
            If result Is Nothing OrElse result.Result Is Nothing Then Return PsmResult.EmptyTable("No effect sensitivity summaries available")
            Dim rows As New List(Of PsmEffectSensitivitySummary)()
            If result.Result.MatchedEffect IsNot Nothing Then rows.Add(PsmSensitivityAnalysis.SummarizeEffectSensitivity(result.Result.MatchedEffect))
            If result.Result.WeightedEffect IsNot Nothing Then rows.Add(PsmSensitivityAnalysis.SummarizeEffectSensitivity(result.Result.WeightedEffect))
            If result.Result.SubclassificationEffect IsNot Nothing Then rows.Add(PsmSensitivityAnalysis.SummarizeEffectSensitivity(result.Result.SubclassificationEffect))
            If result.DoublyRobustResult IsNot Nothing AndAlso result.DoublyRobustResult.Effect IsNot Nothing Then rows.Add(PsmSensitivityAnalysis.SummarizeEffectSensitivity(result.DoublyRobustResult.Effect))
            rows = rows.Where(Function(r) r IsNot Nothing).ToList()
            If rows.Count = 0 Then Return PsmResult.EmptyTable("No effect sensitivity summaries available")

            Dim table(rows.Count, 9) As Object
            table(0, 0) = "Method"
            table(0, 1) = "Estimand"
            table(0, 2) = "Estimate"
            table(0, 3) = "Std. Error"
            table(0, 4) = "z"
            table(0, 5) = "p-value"
            table(0, 6) = "Lower 95%"
            table(0, 7) = "Upper 95%"
            table(0, 8) = "Crosses zero"
            table(0, 9) = "Warning"
            For i As Integer = 0 To rows.Count - 1
                Dim r As PsmEffectSensitivitySummary = rows(i)
                table(i + 1, 0) = r.Method
                table(i + 1, 1) = r.Estimand.ToString()
                table(i + 1, 2) = r.Estimate
                table(i + 1, 3) = r.StandardError
                table(i + 1, 4) = r.ZStatistic
                table(i + 1, 5) = r.TwoSidedPValue
                table(i + 1, 6) = r.ConfidenceLower
                table(i + 1, 7) = r.ConfidenceUpper
                table(i + 1, 8) = r.EffectCrossesZero
                table(i + 1, 9) = r.Warning
            Next
            Return table
        End Function

        Private Shared Function TwoColumnTable(firstHeader As String, secondHeader As String, rows As List(Of Object())) As Object(,)
            If rows Is Nothing OrElse rows.Count = 0 Then Return PsmResult.EmptyTable("No rows available")
            Dim table(rows.Count, 1) As Object
            table(0, 0) = firstHeader
            table(0, 1) = secondHeader
            For i As Integer = 0 To rows.Count - 1
                table(i + 1, 0) = rows(i)(0)
                table(i + 1, 1) = rows(i)(1)
            Next
            Return table
        End Function

        Private Shared Function BuildObservationMap(result As PsmResult) As Dictionary(Of Integer, PsmObservation)
            Dim out As New Dictionary(Of Integer, PsmObservation)()
            If result Is Nothing OrElse result.Observations Is Nothing Then Return out
            For Each obs As PsmObservation In result.Observations
                If obs IsNot Nothing AndAlso Not out.ContainsKey(obs.RowIndex) Then out.Add(obs.RowIndex, obs)
            Next
            Return out
        End Function

        Private Shared Function BuildMatchedSetMap(result As PsmResult) As Dictionary(Of Integer, List(Of Integer))
            Dim out As New Dictionary(Of Integer, List(Of Integer))()
            If result Is Nothing OrElse result.Matches Is Nothing Then Return out
            For Each m As PsmMatchLink In result.Matches
                AddSet(out, m.TreatedRowIndex, m.SetId)
                AddSet(out, m.ControlRowIndex, m.SetId)
            Next
            Return out
        End Function

        Private Shared Function BuildMatchedRoleMap(result As PsmResult) As Dictionary(Of Integer, String)
            Dim out As New Dictionary(Of Integer, String)()
            If result Is Nothing OrElse result.Matches Is Nothing Then Return out
            For Each m As PsmMatchLink In result.Matches
                MergeRole(out, m.TreatedRowIndex, "Treated")
                MergeRole(out, m.ControlRowIndex, "Control")
            Next
            Return out
        End Function

        Private Shared Sub AddSet(map As Dictionary(Of Integer, List(Of Integer)), rowIndex As Integer, setId As Integer)
            If rowIndex < 0 Then Return
            If Not map.ContainsKey(rowIndex) Then map.Add(rowIndex, New List(Of Integer)())
            If Not map(rowIndex).Contains(setId) Then map(rowIndex).Add(setId)
        End Sub

        Private Shared Sub MergeRole(map As Dictionary(Of Integer, String), rowIndex As Integer, role As String)
            If rowIndex < 0 Then Return
            If Not map.ContainsKey(rowIndex) OrElse String.IsNullOrWhiteSpace(map(rowIndex)) Then
                map(rowIndex) = role
            ElseIf Not map(rowIndex).Contains(role) Then
                map(rowIndex) &= "," & role
            End If
        End Sub

        Private Shared Function GetScore(result As PsmResult, rowIndex As Integer) As Double
            If result Is Nothing OrElse result.ScoreModel Is Nothing OrElse result.ScoreModel.Scores Is Nothing Then Return Double.NaN
            If rowIndex < 0 OrElse rowIndex >= result.ScoreModel.Scores.Length Then Return Double.NaN
            Return result.ScoreModel.Scores(rowIndex)
        End Function

        Private Shared Function GetWeight(weights As Double(), rowIndex As Integer) As Double
            If weights Is Nothing OrElse rowIndex < 0 OrElse rowIndex >= weights.Length Then Return Double.NaN
            Return weights(rowIndex)
        End Function

        Private Shared Function BuildRowStatus(obs As PsmObservation, matchingWeight As Double, balancingWeight As Double, matchCount As Integer) As String
            If obs IsNot Nothing Then
                If Not obs.IncludedByCommonSupport Then Return "Dropped: common support"
                If Not obs.IncludedByTrimming Then Return "Dropped: trimming"
            End If
            If matchCount > 0 Then Return "Matched"
            If AppInfrastructure.IsFinite(matchingWeight) AndAlso matchingWeight > 0.0 Then Return "Positive matching weight"
            If AppInfrastructure.IsFinite(balancingWeight) AndAlso balancingWeight > 0.0 Then Return "Positive balancing weight"
            Return "Unmatched / zero weight"
        End Function
    End Class

End Namespace
