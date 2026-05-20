Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace CausalInference

    ''' <summary>
    ''' Higher-level backend run modes intended for GUI/UDF orchestration. This file does not replace the
    ''' low-level engines; it coordinates them into complete analysis runs that can be selected from front ends.
    ''' </summary>
    Public Enum PsmBackendRunMethod
        StandardNearestNeighbor = 0
        StandardSubclassification = 1
        WeightingOnly = 2
        OptimalPairMatching = 3
        CoarsenedExactMatching = 4
    End Enum

    Public Class PsmComprehensiveFitOptions
        Public Property StandardOptions As PsmOptions = New PsmOptions()
        Public Property RunMethod As PsmBackendRunMethod = PsmBackendRunMethod.StandardNearestNeighbor
        Public Property CoarseningSpec As PsmCoarseningSpec
        Public Property IncludeDoublyRobustEstimate As Boolean = True
        Public Property IncludeOverlapDiagnostics As Boolean = True
        Public Property IncludeWeightDiagnostics As Boolean = True
        Public Property IncludeLovePlotRows As Boolean = True
        Public Property OverlapBinCount As Integer = 20
        Public Property LovePlotThreshold As Double = 0.1
        Public Property ExtremeWeightCutoff As Double = 10.0
    End Class

    Public Class PsmComprehensiveResult
        Public Property RunMethod As PsmBackendRunMethod
        Public Property Result As PsmResult
        Public Property CoarsenedExactResult As PsmCoarsenedExactResult
        Public Property DoublyRobustResult As PsmDoublyRobustResult
        Public Property OverlapDiagnostics As PsmOverlapDiagnosticsResult
        Public Property WeightDiagnostics As New List(Of PsmWeightDiagnosticsRow)()
        Public Property LovePlotRows As New List(Of PsmLovePlotRow)()
        Public Property Warnings As New List(Of String)()
    End Class

    Public Class PsmCaliperTuningRow
        Public Property Caliper As Double
        Public Property CaliperScale As PsmCaliperScale
        Public Property MatchedSets As Integer
        Public Property MatchedTreatedRows As Integer
        Public Property MatchedControlRows As Integer
        Public Property MatchedFraction As Double
        Public Property MeanAbsoluteSmd As Double
        Public Property MaxAbsoluteSmd As Double
        Public Property BalanceFlags As Integer
        Public Property Accepted As Boolean
        Public Property Warning As String = ""
    End Class

    Public Class PsmCaliperTuningResult
        Public Property Rows As New List(Of PsmCaliperTuningRow)()
        Public Property SelectedRow As PsmCaliperTuningRow
        Public Property SelectedOptions As PsmOptions
        Public Property Warnings As New List(Of String)()
    End Class

    Public NotInheritable Class PsmComprehensiveBackend
        Private Sub New()
        End Sub

        Public Shared Function Fit(input As PsmInputData, Optional fitOptions As PsmComprehensiveFitOptions = Nothing) As PsmComprehensiveResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If fitOptions Is Nothing Then fitOptions = New PsmComprehensiveFitOptions()
            If fitOptions.StandardOptions Is Nothing Then fitOptions.StandardOptions = New PsmOptions()

            Dim options As PsmOptions = CloneOptions(fitOptions.StandardOptions)
            PsmMethodCapabilities.ValidateFitOptions(fitOptions)
            input.Validate(options)

            Dim output As New PsmComprehensiveResult With {.RunMethod = fitOptions.RunMethod}
            Select Case fitOptions.RunMethod
                Case PsmBackendRunMethod.StandardNearestNeighbor
                    options.MatchingMethod = PsmMatchingMethod.NearestNeighbor
                    output.Result = PsmBackend.Fit(input, options)
                Case PsmBackendRunMethod.StandardSubclassification
                    options.MatchingMethod = PsmMatchingMethod.Subclassification
                    output.Result = PsmBackend.Fit(input, options)
                Case PsmBackendRunMethod.WeightingOnly
                    options.MatchingMethod = PsmMatchingMethod.None
                    output.Result = PsmBackend.Fit(input, options)
                Case PsmBackendRunMethod.OptimalPairMatching
                    output.Result = FitOptimalPairMatching(input, options)
                Case PsmBackendRunMethod.CoarsenedExactMatching
                    output.Result = FitCoarsenedExactMatching(input, options, fitOptions.CoarseningSpec, output)
                Case Else
                    Throw New ArgumentOutOfRangeException("RunMethod", "Unsupported PSM backend run method.")
            End Select

            If output.Result IsNot Nothing AndAlso output.Result.Warnings IsNot Nothing Then output.Warnings.AddRange(output.Result.Warnings)
            AddRequestedDiagnostics(input, fitOptions, output)
            Return output
        End Function

        Private Shared Function FitOptimalPairMatching(input As PsmInputData, options As PsmOptions) As PsmResult
            If options.Estimand <> PsmEstimand.ATT AndAlso options.Estimand <> PsmEstimand.ATC Then Throw New ArgumentException("Optimal pair matching supports ATT and ATC only.")

            Dim result As New PsmResult With {.Options = options}
            result.ScoreModel = PsmPropensityEstimator.Estimate(input, options)
            If result.ScoreModel.Warnings IsNot Nothing Then result.Warnings.AddRange(result.ScoreModel.Warnings)
            result.Observations = PsmMatchingEngine.BuildObservations(input, result.ScoreModel, options)
            result.BalancingWeights = PsmWeightEngine.ComputeBalancingWeights(input, result.ScoreModel.Scores, options)
            result.Balance.AddRange(PsmBalanceDiagnostics.ComputeBalance(input, Matrix.IdentityVect(input.RowCount - 1), PsmBalanceSample.Before, options))
            result.Balance.AddRange(PsmBalanceDiagnostics.ComputeBalance(input, result.BalancingWeights, PsmBalanceSample.AfterWeighting, options))
            result.Matches = PsmAdvancedMatching.MatchOptimalPairs(input, result.Observations, options)
            result.MatchingWeights = PsmMatchingEngine.ComputeMatchingWeights(input, result.Matches, options)
            result.Balance.AddRange(PsmBalanceDiagnostics.ComputeBalance(input, result.MatchingWeights, PsmBalanceSample.AfterMatching, options))
            result.MatchedEffect = PsmOutcomeAnalysis.EstimateMatchedEffect(input, result.Matches, options)
            result.WeightedEffect = PsmOutcomeAnalysis.EstimateWeightedEffect(input, result.BalancingWeights, options)
            result.SampleSize = BuildSampleSummary(input, result)
            If result.Matches.Count = 0 Then result.Warnings.Add("Optimal pair matching found no eligible pairs. Check common support, exact groups, and caliper settings.")
            Return result
        End Function

        Private Shared Function FitCoarsenedExactMatching(input As PsmInputData, options As PsmOptions, spec As PsmCoarseningSpec, output As PsmComprehensiveResult) As PsmResult
            If spec Is Nothing Then spec = New PsmCoarseningSpec With {.Estimand = options.Estimand, .NormalizeWeightsToSampleSize = options.NormalizeWeightsToSampleSize}
            spec.Estimand = options.Estimand
            spec.NormalizeWeightsToSampleSize = options.NormalizeWeightsToSampleSize

            Dim result As New PsmResult With {.Options = options}
            result.ScoreModel = PsmPropensityEstimator.Estimate(input, options)
            If result.ScoreModel.Warnings IsNot Nothing Then result.Warnings.AddRange(result.ScoreModel.Warnings)
            result.Observations = PsmMatchingEngine.BuildObservations(input, result.ScoreModel, options)
            output.CoarsenedExactResult = PsmAdvancedMatching.BuildCoarsenedExactWeights(input, result.ScoreModel.Scores, spec)
            If output.CoarsenedExactResult.Warnings IsNot Nothing Then result.Warnings.AddRange(output.CoarsenedExactResult.Warnings)

            result.BalancingWeights = output.CoarsenedExactResult.Weights
            result.MatchingWeights = output.CoarsenedExactResult.Weights
            result.Balance.AddRange(PsmBalanceDiagnostics.ComputeBalance(input, Matrix.IdentityVect(input.RowCount - 1), PsmBalanceSample.Before, options))
            result.Balance.AddRange(PsmBalanceDiagnostics.ComputeBalance(input, result.BalancingWeights, PsmBalanceSample.AfterWeighting, options))
            result.WeightedEffect = PsmAdvancedMatching.EstimateCoarsenedExactEffect(input, output.CoarsenedExactResult, options.Estimand)
            result.SampleSize = BuildSampleSummary(input, result)
            Return result
        End Function

        Private Shared Sub AddRequestedDiagnostics(input As PsmInputData, fitOptions As PsmComprehensiveFitOptions, output As PsmComprehensiveResult)
            If output Is Nothing OrElse output.Result Is Nothing OrElse output.Result.ScoreModel Is Nothing Then Return
            Dim result As PsmResult = output.Result
            Dim options As PsmOptions = result.Options

            If fitOptions.IncludeDoublyRobustEstimate AndAlso input.Outcome IsNot Nothing AndAlso result.ScoreModel.Scores IsNot Nothing Then
                Try
                    output.DoublyRobustResult = PsmDoublyRobustEstimator.EstimateAipw(input, result.ScoreModel.Scores, options)
                Catch ex As Exception
                    output.Warnings.Add("Doubly robust estimate could not be computed: " & ex.Message)
                End Try
            End If

            If fitOptions.IncludeOverlapDiagnostics AndAlso result.ScoreModel.Scores IsNot Nothing Then
                Try
                    output.OverlapDiagnostics = PsmAdvancedDiagnostics.ComputeOverlapDiagnostics(input, result.ScoreModel.Scores, result.BalancingWeights, fitOptions.OverlapBinCount)
                Catch ex As Exception
                    output.Warnings.Add("Overlap diagnostics could not be computed: " & ex.Message)
                End Try
            End If

            If fitOptions.IncludeWeightDiagnostics Then
                Try
                    Dim weights As Double() = If(result.BalancingWeights, result.MatchingWeights)
                    If weights IsNot Nothing Then output.WeightDiagnostics = PsmAdvancedDiagnostics.ComputeWeightDiagnostics(input, weights, output.RunMethod.ToString(), fitOptions.ExtremeWeightCutoff)
                Catch ex As Exception
                    output.Warnings.Add("Weight diagnostics could not be computed: " & ex.Message)
                End Try
            End If

            If fitOptions.IncludeLovePlotRows Then
                Try
                    output.LovePlotRows = PsmAdvancedDiagnostics.BuildLovePlotRows(result, fitOptions.LovePlotThreshold)
                Catch ex As Exception
                    output.Warnings.Add("Love plot rows could not be computed: " & ex.Message)
                End Try
            End If
        End Sub

        Private Shared Function BuildSampleSummary(input As PsmInputData, result As PsmResult) As PsmSampleSizeSummary
            Dim obs As List(Of PsmObservation) = If(result.Observations, New List(Of PsmObservation)())
            Dim matchedT As Integer = If(result.Matches Is Nothing, 0, result.Matches.Select(Function(m) m.TreatedRowIndex).Distinct().Count())
            Dim matchedC As Integer = If(result.Matches Is Nothing, 0, result.Matches.Select(Function(m) m.ControlRowIndex).Distinct().Count())
            Dim treatedRows As Integer = CountTreatment(input, True)
            Dim controlRows As Integer = CountTreatment(input, False)
            Dim weightedT As Integer = CountPositiveWeights(input, result.BalancingWeights, True)
            Dim weightedC As Integer = CountPositiveWeights(input, result.BalancingWeights, False)
            If matchedT = 0 AndAlso weightedT > 0 Then matchedT = weightedT
            If matchedC = 0 AndAlso weightedC > 0 Then matchedC = weightedC

            Return New PsmSampleSizeSummary With {
                .TotalRows = input.RowCount,
                .TreatedRows = treatedRows,
                .ControlRows = controlRows,
                .EligibleTreatedRows = System.Linq.Enumerable.Count(obs, Function(o) o.Treated AndAlso o.IncludedByCommonSupport AndAlso o.IncludedByTrimming),
                .EligibleControlRows = System.Linq.Enumerable.Count(obs, Function(o) Not o.Treated AndAlso o.IncludedByCommonSupport AndAlso o.IncludedByTrimming),
                .MatchedTreatedRows = matchedT,
                .MatchedControlRows = matchedC,
                .MatchedSets = If(result.Matches Is Nothing, 0, result.Matches.Select(Function(m) m.SetId).Distinct().Count()),
                .UnmatchedTreatedRows = treatedRows - matchedT,
                .UnmatchedControlRows = controlRows - matchedC,
                .DroppedByCommonSupport = System.Linq.Enumerable.Count(obs, Function(o) Not o.IncludedByCommonSupport),
                .DroppedByTrimming = System.Linq.Enumerable.Count(obs, Function(o) Not o.IncludedByTrimming)
            }
        End Function

        Private Shared Function CountTreatment(input As PsmInputData, treated As Boolean) As Integer
            Dim n As Integer = 0
            For i As Integer = 0 To input.RowCount - 1
                If (input.Treatment(i) >= 0.5) = treated Then n += 1
            Next
            Return n
        End Function

        Private Shared Function CountPositiveWeights(input As PsmInputData, weights As Double(), treated As Boolean) As Integer
            If weights Is Nothing OrElse weights.Length <> input.RowCount Then Return 0
            Dim n As Integer = 0
            For i As Integer = 0 To input.RowCount - 1
                If (input.Treatment(i) >= 0.5) = treated AndAlso AppInfrastructure.IsFinite(weights(i)) AndAlso weights(i) > 0 Then n += 1
            Next
            Return n
        End Function

        Public Shared Function CloneOptions(options As PsmOptions) As PsmOptions
            If options Is Nothing Then Return New PsmOptions()
            Return New PsmOptions With {
                .ScoreMethod = options.ScoreMethod,
                .MatchingMethod = options.MatchingMethod,
                .Estimand = options.Estimand,
                .DistanceMetric = options.DistanceMetric,
                .MatchingRatio = options.MatchingRatio,
                .WithReplacement = options.WithReplacement,
                .CaliperScale = options.CaliperScale,
                .Caliper = options.Caliper,
                .MatchingOrder = options.MatchingOrder,
                .CommonSupport = options.CommonSupport,
                .RandomSeed = options.RandomSeed,
                .IncludeIntercept = options.IncludeIntercept,
                .StandardizeCovariates = options.StandardizeCovariates,
                .LogisticMaxIterations = options.LogisticMaxIterations,
                .LogisticTolerance = options.LogisticTolerance,
                .LogisticRidgePenalty = options.LogisticRidgePenalty,
                .BalanceSmdThreshold = options.BalanceSmdThreshold,
                .BalanceVarianceRatioLower = options.BalanceVarianceRatioLower,
                .BalanceVarianceRatioUpper = options.BalanceVarianceRatioUpper,
                .SubclassificationStrata = options.SubclassificationStrata,
                .NormalizeWeightsToSampleSize = options.NormalizeWeightsToSampleSize,
                .TrimPropensityLower = options.TrimPropensityLower,
                .TrimPropensityUpper = options.TrimPropensityUpper
            }
        End Function
    End Class

    Public NotInheritable Class PsmCaliperTuner
        Private Sub New()
        End Sub

        Public Shared Function TuneNearestNeighbor(input As PsmInputData,
                                                   baseOptions As PsmOptions,
                                                   calipers As Double(),
                                                   Optional caliperScale As PsmCaliperScale = PsmCaliperScale.StandardizedLogitPropensityScore,
                                                   Optional minimumMatchedFraction As Double = 0.5) As PsmCaliperTuningResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If baseOptions Is Nothing Then baseOptions = New PsmOptions()
            If calipers Is Nothing OrElse calipers.Length = 0 Then calipers = New Double() {0.05, 0.1, 0.15, 0.2, 0.25}
            If minimumMatchedFraction < 0.0 OrElse minimumMatchedFraction > 1.0 Then Throw New ArgumentOutOfRangeException("minimumMatchedFraction", "Minimum matched fraction must be between 0 and 1.")

            Dim tuning As New PsmCaliperTuningResult()
            For Each cal In calipers
                Dim row As New PsmCaliperTuningRow With {.Caliper = cal, .CaliperScale = caliperScale}
                If Double.IsNaN(cal) OrElse Double.IsInfinity(cal) OrElse cal < 0 Then
                    row.Warning = "Invalid caliper."
                    row.Accepted = False
                    tuning.Rows.Add(row)
                    Continue For
                End If

                Try
                    Dim opt As PsmOptions = PsmComprehensiveBackend.CloneOptions(baseOptions)
                    opt.MatchingMethod = PsmMatchingMethod.NearestNeighbor
                    opt.CaliperScale = caliperScale
                    opt.Caliper = cal
                    Dim fit As PsmResult = PsmBackend.Fit(input, opt)
                    FillTuningRow(row, input, fit, minimumMatchedFraction)
                Catch ex As Exception
                    row.Warning = ex.Message
                    row.Accepted = False
                End Try
                tuning.Rows.Add(row)
            Next

            Dim candidates As List(Of PsmCaliperTuningRow) = tuning.Rows.Where(Function(r) r.Accepted).OrderBy(Function(r) r.MaxAbsoluteSmd).ThenBy(Function(r) r.MeanAbsoluteSmd).ThenByDescending(Function(r) r.MatchedFraction).ToList()
            If candidates.Count > 0 Then
                tuning.SelectedRow = candidates(0)
                tuning.SelectedOptions = PsmComprehensiveBackend.CloneOptions(baseOptions)
                tuning.SelectedOptions.MatchingMethod = PsmMatchingMethod.NearestNeighbor
                tuning.SelectedOptions.CaliperScale = tuning.SelectedRow.CaliperScale
                tuning.SelectedOptions.Caliper = tuning.SelectedRow.Caliper
            Else
                tuning.Warnings.Add("No candidate caliper satisfied the requested minimum matched fraction.")
            End If
            Return tuning
        End Function

        Private Shared Sub FillTuningRow(row As PsmCaliperTuningRow, input As PsmInputData, fit As PsmResult, minimumMatchedFraction As Double)
            If fit Is Nothing Then
                row.Warning = "No fit returned."
                Return
            End If

            Dim focalTotal As Integer = CountFocalRows(input, fit.Options)
            row.MatchedSets = If(fit.Matches Is Nothing, 0, fit.Matches.Select(Function(m) m.SetId).Distinct().Count())
            row.MatchedTreatedRows = If(fit.Matches Is Nothing, 0, fit.Matches.Select(Function(m) m.TreatedRowIndex).Distinct().Count())
            row.MatchedControlRows = If(fit.Matches Is Nothing, 0, fit.Matches.Select(Function(m) m.ControlRowIndex).Distinct().Count())
            Dim matchedFocal As Integer = If(fit.Options.Estimand = PsmEstimand.ATC, row.MatchedControlRows, row.MatchedTreatedRows)
            row.MatchedFraction = If(focalTotal > 0, CDbl(matchedFocal) / CDbl(focalTotal), 0.0)

            Dim afterRows As List(Of PsmBalanceRow) = fit.Balance.Where(Function(b) b.Sample = PsmBalanceSample.AfterMatching AndAlso AppInfrastructure.IsFinite(b.StandardizedMeanDifference)).ToList()
            If afterRows.Count > 0 Then
                row.MeanAbsoluteSmd = afterRows.Select(Function(b) Math.Abs(b.StandardizedMeanDifference)).Average()
                row.MaxAbsoluteSmd = afterRows.Select(Function(b) Math.Abs(b.StandardizedMeanDifference)).Max()
                row.BalanceFlags = System.Linq.Enumerable.Count(afterRows, Function(b) Not String.Equals(b.Flag, "OK", StringComparison.OrdinalIgnoreCase))
            Else
                row.MeanAbsoluteSmd = Double.NaN
                row.MaxAbsoluteSmd = Double.NaN
                row.BalanceFlags = 0
            End If

            row.Accepted = (row.MatchedFraction >= minimumMatchedFraction AndAlso fit.Matches IsNot Nothing AndAlso fit.Matches.Count > 0)
        End Sub

        Private Shared Function CountFocalRows(input As PsmInputData, options As PsmOptions) As Integer
            Dim treatedFocal As Boolean = (options.Estimand <> PsmEstimand.ATC)
            Dim n As Integer = 0
            For i As Integer = 0 To input.RowCount - 1
                If (input.Treatment(i) >= 0.5) = treatedFocal Then n += 1
            Next
            Return n
        End Function
    End Class

    Public NotInheritable Class PsmComprehensiveTables
        Private Sub New()
        End Sub

        Public Shared Function RunSummaryTable(result As PsmComprehensiveResult) As Object(,)
            If result Is Nothing OrElse result.Result Is Nothing Then Return PsmResult.EmptyTable("No comprehensive PSM result available")
            Dim rows As New List(Of Object())()
            rows.Add(New Object() {"Run Method", result.RunMethod.ToString()})
            If result.Result.ScoreModel IsNot Nothing Then
                rows.Add(New Object() {"Score Method", result.Result.ScoreModel.Method.ToString()})
                rows.Add(New Object() {"Score Model Converged", result.Result.ScoreModel.Converged})
                rows.Add(New Object() {"Score Model Iterations", result.Result.ScoreModel.Iterations})
            End If
            If result.Result.SampleSize IsNot Nothing Then
                rows.Add(New Object() {"Total Rows", result.Result.SampleSize.TotalRows})
                rows.Add(New Object() {"Treated Rows", result.Result.SampleSize.TreatedRows})
                rows.Add(New Object() {"Control Rows", result.Result.SampleSize.ControlRows})
                rows.Add(New Object() {"Matched Sets", result.Result.SampleSize.MatchedSets})
                rows.Add(New Object() {"Dropped by Common Support", result.Result.SampleSize.DroppedByCommonSupport})
                rows.Add(New Object() {"Dropped by Trimming", result.Result.SampleSize.DroppedByTrimming})
            End If
            If result.CoarsenedExactResult IsNot Nothing Then
                rows.Add(New Object() {"CEM Retained Rows", result.CoarsenedExactResult.RetainedRows})
                rows.Add(New Object() {"CEM Dropped Rows", result.CoarsenedExactResult.DroppedRows})
            End If
            rows.Add(New Object() {"Warnings", If(result.Warnings Is Nothing, 0, result.Warnings.Count)})

            Dim table(rows.Count, 1) As Object
            table(0, 0) = "Item" : table(0, 1) = "Value"
            For i As Integer = 0 To rows.Count - 1
                table(i + 1, 0) = rows(i)(0)
                table(i + 1, 1) = rows(i)(1)
            Next
            Return table
        End Function

        Public Shared Function WarningsTable(result As PsmComprehensiveResult) As Object(,)
            If result Is Nothing OrElse result.Warnings Is Nothing OrElse result.Warnings.Count = 0 Then Return PsmResult.EmptyTable("No warnings")
            Dim table(result.Warnings.Count, 0) As Object
            table(0, 0) = "Warning"
            For i As Integer = 0 To result.Warnings.Count - 1
                table(i + 1, 0) = result.Warnings(i)
            Next
            Return table
        End Function

        Public Shared Function CaliperTuningTable(result As PsmCaliperTuningResult) As Object(,)
            If result Is Nothing OrElse result.Rows Is Nothing OrElse result.Rows.Count = 0 Then Return PsmResult.EmptyTable("No caliper tuning results")
            Dim table(result.Rows.Count, 9) As Object
            table(0, 0) = "Caliper" : table(0, 1) = "Scale" : table(0, 2) = "Matched Sets" : table(0, 3) = "Matched Treated" : table(0, 4) = "Matched Control" : table(0, 5) = "Matched Fraction" : table(0, 6) = "Mean |SMD|" : table(0, 7) = "Max |SMD|" : table(0, 8) = "Balance Flags" : table(0, 9) = "Accepted"
            For i As Integer = 0 To result.Rows.Count - 1
                Dim r = result.Rows(i)
                table(i + 1, 0) = r.Caliper
                table(i + 1, 1) = r.CaliperScale.ToString()
                table(i + 1, 2) = r.MatchedSets
                table(i + 1, 3) = r.MatchedTreatedRows
                table(i + 1, 4) = r.MatchedControlRows
                table(i + 1, 5) = r.MatchedFraction
                table(i + 1, 6) = r.MeanAbsoluteSmd
                table(i + 1, 7) = r.MaxAbsoluteSmd
                table(i + 1, 8) = r.BalanceFlags
                table(i + 1, 9) = r.Accepted
            Next
            Return table
        End Function
    End Class

End Namespace
