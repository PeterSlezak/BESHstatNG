Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace CausalInference
    Public NotInheritable Class PsmBackend
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Main backend entry point. GUI and UDF front ends should call this facade rather than duplicating logic.
        ''' </summary>
        Public Shared Function Fit(input As PsmInputData, Optional options As PsmOptions = Nothing) As PsmResult
            If options Is Nothing Then options = New PsmOptions()
            ValidateStandaloneOptions(options)
            input.Validate(options)

            Dim result As New PsmResult With {.Options = options}
            result.ScoreModel = PsmPropensityEstimator.Estimate(input, options)
            If result.ScoreModel.Warnings IsNot Nothing Then result.Warnings.AddRange(result.ScoreModel.Warnings)

            result.Observations = PsmMatchingEngine.BuildObservations(input, result.ScoreModel, options)
            result.BalancingWeights = PsmWeightEngine.ComputeBalancingWeights(input, result.ScoreModel.Scores, options)
            result.Balance.AddRange(PsmBalanceDiagnostics.ComputeBalance(input, Matrix.MatrixArithmeticCore.ConstantVector(input.RowCount - 1), PsmBalanceSample.Before, options))
            result.Balance.AddRange(PsmBalanceDiagnostics.ComputeBalance(input, result.BalancingWeights, PsmBalanceSample.AfterWeighting, options))

            If options.MatchingMethod = PsmMatchingMethod.NearestNeighbor Then
                result.Matches = PsmMatchingEngine.MatchNearestNeighbors(input, result.Observations, options)
                result.MatchingWeights = PsmMatchingEngine.ComputeMatchingWeights(input, result.Matches, options)
                result.Balance.AddRange(PsmBalanceDiagnostics.ComputeBalance(input, result.MatchingWeights, PsmBalanceSample.AfterMatching, options))
                result.MatchedEffect = PsmOutcomeAnalysis.EstimateMatchedEffect(input, result.Matches, options)
            Else
                result.MatchingWeights = New Double(input.RowCount - 1) {}
            End If

            If options.MatchingMethod = PsmMatchingMethod.Subclassification Then
                result.Subclasses = PsmSubclassificationEngine.BuildSubclasses(input, result.ScoreModel.Scores, options)
                result.SubclassificationEffect = PsmSubclassificationEngine.EstimateEffect(result.Subclasses, options)
            End If

            result.WeightedEffect = PsmOutcomeAnalysis.EstimateWeightedEffect(input, result.BalancingWeights, options)
            result.SampleSize = BuildSampleSummary(input, result)
            Return result
        End Function

        Private Shared Sub ValidateStandaloneOptions(options As PsmOptions)
            If options Is Nothing Then Throw New ArgumentNullException("options")
            options.Validate()

            If options.MatchingMethod = PsmMatchingMethod.NearestNeighbor AndAlso
               options.Estimand <> PsmEstimand.ATT AndAlso
               options.Estimand <> PsmEstimand.ATC Then
                Throw New ArgumentException("Nearest-neighbour matching supports ATT and ATC only. Use weighting, subclassification, or the comprehensive backend for ATE/ATO.")
            End If
        End Sub

        Private Shared Function BuildSampleSummary(input As PsmInputData, result As PsmResult) As PsmSampleSizeSummary
            Dim obs As List(Of PsmObservation) = result.Observations
            Dim matchedT = result.Matches.Select(Function(m) m.TreatedRowIndex).Distinct().Count()
            Dim matchedC = result.Matches.Select(Function(m) m.ControlRowIndex).Distinct().Count()
            Return New PsmSampleSizeSummary With {
                .TotalRows = input.RowCount,
                .TreatedRows = input.Treatment.Count(Function(v) v >= 0.5),
                .ControlRows = input.Treatment.Count(Function(v) v < 0.5),
                .EligibleTreatedRows = System.Linq.Enumerable.Count(obs, Function(o) o.Treated AndAlso o.IncludedByCommonSupport AndAlso o.IncludedByTrimming),
                .EligibleControlRows = System.Linq.Enumerable.Count(obs, Function(o) Not o.Treated AndAlso o.IncludedByCommonSupport AndAlso o.IncludedByTrimming),
                .MatchedTreatedRows = matchedT,
                .MatchedControlRows = matchedC,
                .MatchedSets = result.Matches.Select(Function(m) m.SetId).Distinct().Count(),
                .UnmatchedTreatedRows = input.Treatment.Count(Function(v) v >= 0.5) - matchedT,
                .UnmatchedControlRows = input.Treatment.Count(Function(v) v < 0.5) - matchedC,
                .DroppedByCommonSupport = System.Linq.Enumerable.Count(obs, Function(o) Not o.IncludedByCommonSupport),
                .DroppedByTrimming = System.Linq.Enumerable.Count(obs, Function(o) Not o.IncludedByTrimming)
            }
        End Function
    End Class

    ''' <summary>
    ''' Table builders are kept in the backend so the GUI and UDF layers can share one representation.
    ''' </summary>
    Public NotInheritable Class PsmBackendTables
        Private Sub New()
        End Sub

        Public Shared Function ScoreModelTable(result As PsmResult) As Object(,)
            If result Is Nothing OrElse result.ScoreModel Is Nothing OrElse result.ScoreModel.Scores Is Nothing Then Return PsmResult.EmptyTable("No score model available")
            Dim p As Integer = If(result.ScoreModel.Coefficients Is Nothing, 0, result.ScoreModel.Coefficients.Length)
            Dim rows As Integer = Math.Max(1, p)
            Dim table(rows, 3) As Object
            table(0, 0) = "Term" : table(0, 1) = "Estimate" : table(0, 2) = "Std. Error" : table(0, 3) = "Method"
            If p = 0 Then
                table(1, 0) = "Supplied scores" : table(1, 1) = "" : table(1, 2) = "" : table(1, 3) = result.ScoreModel.Method.ToString()
            Else
                For j As Integer = 0 To p - 1
                    table(j + 1, 0) = result.ScoreModel.VariableNames(j)
                    table(j + 1, 1) = result.ScoreModel.Coefficients(j)
                    table(j + 1, 2) = result.ScoreModel.StandardErrors(j)
                    table(j + 1, 3) = result.ScoreModel.Method.ToString()
                Next
            End If
            Return table
        End Function

        Public Shared Function BalanceTable(result As PsmResult) As Object(,)
            If result Is Nothing OrElse result.Balance Is Nothing OrElse result.Balance.Count = 0 Then Return PsmResult.EmptyTable("No balance diagnostics available")
            Dim table(result.Balance.Count, 10) As Object
            table(0, 0) = "Sample" : table(0, 1) = "Variable" : table(0, 2) = "Treated Mean" : table(0, 3) = "Control Mean" : table(0, 4) = "Treated Var" : table(0, 5) = "Control Var" : table(0, 6) = "SMD" : table(0, 7) = "Variance Ratio" : table(0, 8) = "eCDF Mean" : table(0, 9) = "eCDF Max" : table(0, 10) = "Flag"
            For i As Integer = 0 To result.Balance.Count - 1
                Dim r = result.Balance(i)
                table(i + 1, 0) = r.Sample.ToString()
                table(i + 1, 1) = r.VariableName
                table(i + 1, 2) = r.TreatedMean
                table(i + 1, 3) = r.ControlMean
                table(i + 1, 4) = r.TreatedVariance
                table(i + 1, 5) = r.ControlVariance
                table(i + 1, 6) = r.StandardizedMeanDifference
                table(i + 1, 7) = r.VarianceRatio
                table(i + 1, 8) = r.EcdfMeanDifference
                table(i + 1, 9) = r.EcdfMaxDifference
                table(i + 1, 10) = r.Flag
            Next
            Return table
        End Function

        Public Shared Function MatchesTable(result As PsmResult, input As PsmInputData) As Object(,)
            If result Is Nothing OrElse result.Matches Is Nothing OrElse result.Matches.Count = 0 Then Return PsmResult.EmptyTable("No matches available")
            Dim table(result.Matches.Count, 8) As Object
            table(0, 0) = "Set" : table(0, 1) = "Treated Row" : table(0, 2) = "Treated ID" : table(0, 3) = "Control Row" : table(0, 4) = "Control ID" : table(0, 5) = "Distance" : table(0, 6) = "PS Distance" : table(0, 7) = "Mahalanobis" : table(0, 8) = "Exact Group"
            For i As Integer = 0 To result.Matches.Count - 1
                Dim m = result.Matches(i)
                table(i + 1, 0) = m.SetId
                table(i + 1, 1) = m.TreatedRowIndex + 1
                table(i + 1, 2) = input.GetId(m.TreatedRowIndex)
                table(i + 1, 3) = m.ControlRowIndex + 1
                table(i + 1, 4) = input.GetId(m.ControlRowIndex)
                table(i + 1, 5) = m.Distance
                table(i + 1, 6) = m.PropensityDistance
                table(i + 1, 7) = m.MahalanobisDistance
                table(i + 1, 8) = m.ExactGroupLabel
            Next
            Return table
        End Function

        Public Shared Function EffectTable(result As PsmResult) As Object(,)
            Dim effects As New List(Of PsmEffectResult)()
            If result IsNot Nothing Then
                If result.MatchedEffect IsNot Nothing Then effects.Add(result.MatchedEffect)
                If result.WeightedEffect IsNot Nothing Then effects.Add(result.WeightedEffect)
                If result.SubclassificationEffect IsNot Nothing Then effects.Add(result.SubclassificationEffect)
            End If
            If effects.Count = 0 Then Return PsmResult.EmptyTable("No effect estimates available")
            Dim table(effects.Count, 9) As Object
            table(0, 0) = "Method" : table(0, 1) = "Estimand" : table(0, 2) = "Estimate" : table(0, 3) = "Std. Error" : table(0, 4) = "Lower 95%" : table(0, 5) = "Upper 95%" : table(0, 6) = "Treated Mean" : table(0, 7) = "Control Mean" : table(0, 8) = "Eff. Treated N" : table(0, 9) = "Eff. Control N"
            For i As Integer = 0 To effects.Count - 1
                Dim e = effects(i)
                table(i + 1, 0) = e.Method
                table(i + 1, 1) = e.Estimand.ToString()
                table(i + 1, 2) = e.Estimate
                table(i + 1, 3) = e.StandardError
                table(i + 1, 4) = e.LowerConfidenceLimit
                table(i + 1, 5) = e.UpperConfidenceLimit
                table(i + 1, 6) = e.TreatedMean
                table(i + 1, 7) = e.ControlMean
                table(i + 1, 8) = e.EffectiveTreatedN
                table(i + 1, 9) = e.EffectiveControlN
            Next
            Return table
        End Function
    End Class

End Namespace
