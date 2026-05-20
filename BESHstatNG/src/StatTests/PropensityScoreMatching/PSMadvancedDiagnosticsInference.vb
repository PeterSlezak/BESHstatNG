Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.Resampling

Namespace CausalInference

    ''' <summary>
    ''' Target effect estimator used by the non-parametric bootstrap helper.
    ''' </summary>
    Public Enum PsmBootstrapTarget
        WeightedEffect = 0
        MatchedEffect = 1
        SubclassificationEffect = 2
        DoublyRobustAipw = 3
    End Enum

    ''' <summary>
    ''' Compact row used for weight diagnostics by group and overall.
    ''' </summary>
    Public Class PsmWeightDiagnosticsRow
        Public Property SampleName As String = ""
        Public Property GroupName As String = ""
        Public Property N As Integer
        Public Property NonZeroN As Integer
        Public Property SumWeights As Double
        Public Property MeanWeight As Double
        Public Property MinWeight As Double
        Public Property MaxWeight As Double
        Public Property WeightVariance As Double
        Public Property CoefficientOfVariation As Double
        Public Property EffectiveSampleSize As Double
        Public Property EffectiveSampleSizeRatio As Double
        Public Property ExtremeWeightN As Integer
        Public Property ExtremeWeightCutoff As Double
        Public Property Flag As String = "OK"
    End Class

    Public Class PsmOverlapSummaryRow
        Public Property GroupName As String = ""
        Public Property N As Integer
        Public Property MinScore As Double
        Public Property Q1Score As Double
        Public Property MedianScore As Double
        Public Property Q3Score As Double
        Public Property MaxScore As Double
        Public Property MeanScore As Double
        Public Property SdScore As Double
        Public Property BelowOverlapRangeN As Integer
        Public Property AboveOverlapRangeN As Integer
        Public Property ExtremeLowN As Integer
        Public Property ExtremeHighN As Integer
        Public Property Flag As String = "OK"
    End Class

    Public Class PsmOverlapBinRow
        Public Property BinIndex As Integer
        Public Property LowerBound As Double
        Public Property UpperBound As Double
        Public Property TreatedN As Integer
        Public Property ControlN As Integer
        Public Property TreatedWeight As Double
        Public Property ControlWeight As Double
    End Class

    Public Class PsmOverlapDiagnosticsResult
        Public Property Summary As New List(Of PsmOverlapSummaryRow)()
        Public Property Bins As New List(Of PsmOverlapBinRow)()
        Public Property Warnings As New List(Of String)()
    End Class

    Public Class PsmLovePlotRow
        Public Property VariableName As String = ""
        Public Property SmdBefore As Double
        Public Property SmdAfterMatching As Double
        Public Property SmdAfterWeighting As Double
        Public Property MaxAbsAfter As Double
        Public Property Threshold As Double
        Public Property Flag As String = "OK"
    End Class

    Public Class PsmOutcomeModelResult
        Public Property TreatmentLevel As Integer
        Public Property Coefficients As Double()
        Public Property VariableNames As String()
        Public Property ResidualVariance As Double
        Public Property N As Integer
        Public Property Warnings As New List(Of String)()
    End Class

    Public Class PsmDoublyRobustResult
        Public Property Effect As PsmEffectResult
        Public Property TreatedOutcomeModel As PsmOutcomeModelResult
        Public Property ControlOutcomeModel As PsmOutcomeModelResult
        Public Property IndividualContributions As Double()
        Public Property Warnings As New List(Of String)()
    End Class

    Public Class PsmBootstrapResult
        Public Property Target As PsmBootstrapTarget
        Public Property ReplicationsRequested As Integer
        Public Property ReplicationsUsed As Integer
        Public Property EstimateOriginal As Double
        Public Property BootstrapMean As Double
        Public Property BootstrapStandardError As Double
        Public Property PercentileLower95 As Double
        Public Property PercentileUpper95 As Double
        Public Property ReplicateEstimates As Double()
        Public Property Warnings As New List(Of String)()
    End Class

    ''' <summary>
    ''' Post-estimation diagnostics for balancing weights, overlap and Love-plot style balance summaries.
    ''' These helpers are backend-only so GUI and UDF layers can share the same calculations.
    ''' </summary>
    Public NotInheritable Class PsmAdvancedDiagnostics
        Private Sub New()
        End Sub

        Public Shared Function ComputeWeightDiagnostics(input As PsmInputData, weights As Double(), Optional sampleName As String = "Weights", Optional extremeWeightCutoff As Double = 10.0) As List(Of PsmWeightDiagnosticsRow)
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If weights Is Nothing OrElse weights.Length <> input.RowCount Then Throw New ArgumentException("Weights length must match input row count.")
            If Double.IsNaN(extremeWeightCutoff) OrElse extremeWeightCutoff <= 0 Then extremeWeightCutoff = 10.0

            Dim rows As New List(Of PsmWeightDiagnosticsRow)()
            rows.Add(BuildWeightDiagnosticRow(sampleName, "All", input, weights, Nothing, extremeWeightCutoff))
            rows.Add(BuildWeightDiagnosticRow(sampleName, "Treated", input, weights, True, extremeWeightCutoff))
            rows.Add(BuildWeightDiagnosticRow(sampleName, "Control", input, weights, False, extremeWeightCutoff))
            Return rows
        End Function

        Private Shared Function BuildWeightDiagnosticRow(sampleName As String, groupName As String, input As PsmInputData, weights As Double(), treatedGroup As Boolean?, extremeWeightCutoff As Double) As PsmWeightDiagnosticsRow
            Dim groupWeights As New List(Of Double)()
            For i As Integer = 0 To input.RowCount - 1
                Dim isTreated As Boolean = input.Treatment(i) >= 0.5
                If treatedGroup.HasValue AndAlso isTreated <> treatedGroup.Value Then Continue For
                groupWeights.Add(weights(i))
            Next

            Dim values As Double() = groupWeights.ToArray()
            Dim nonZero As Double() = values.Where(Function(w) AppInfrastructure.IsFinite(w) AndAlso w > 0.0).ToArray()
            Dim row As New PsmWeightDiagnosticsRow With {
                .SampleName = sampleName,
                .GroupName = groupName,
                .N = values.Length,
                .NonZeroN = nonZero.Length,
                .ExtremeWeightCutoff = extremeWeightCutoff
            }

            If nonZero.Length = 0 Then
                row.MinWeight = Double.NaN
                row.MaxWeight = Double.NaN
                row.MeanWeight = Double.NaN
                row.WeightVariance = Double.NaN
                row.CoefficientOfVariation = Double.NaN
                row.EffectiveSampleSize = 0.0
                row.EffectiveSampleSizeRatio = 0.0
                row.ExtremeWeightN = 0
                row.Flag = "No non-zero weights"
                Return row
            End If

            row.SumWeights = nonZero.Sum()
            row.MeanWeight = nonZero.Average()
            row.MinWeight = nonZero.Min()
            row.MaxWeight = nonZero.Max()
            row.WeightVariance = If(nonZero.Length > 1, PsmMath.Variance(nonZero, True), 0.0)
            row.CoefficientOfVariation = If(row.MeanWeight > 0.0 AndAlso AppInfrastructure.IsFinite(row.WeightVariance), Math.Sqrt(Math.Max(0.0, row.WeightVariance)) / row.MeanWeight, Double.NaN)
            row.EffectiveSampleSize = PsmMath.EffectiveSampleSize(nonZero)
            row.EffectiveSampleSizeRatio = If(row.N > 0, row.EffectiveSampleSize / CDbl(row.N), Double.NaN)
            row.ExtremeWeightN = System.Linq.Enumerable.Count(nonZero, Function(w) w > extremeWeightCutoff)

            Dim flags As New List(Of String)()
            If row.EffectiveSampleSizeRatio < 0.5 Then flags.Add("Low ESS")
            If row.ExtremeWeightN > 0 Then flags.Add("Extreme weights")
            If AppInfrastructure.IsFinite(row.CoefficientOfVariation) AndAlso row.CoefficientOfVariation > 2.0 Then flags.Add("High CV")
            row.Flag = If(flags.Count = 0, "OK", String.Join(",", flags.ToArray()))
            Return row
        End Function

        Public Shared Function ComputeOverlapDiagnostics(input As PsmInputData, scores As Double(), Optional weights As Double() = Nothing, Optional binCount As Integer = 20, Optional extremeLower As Double = 0.05, Optional extremeUpper As Double = 0.95) As PsmOverlapDiagnosticsResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If scores Is Nothing OrElse scores.Length <> input.RowCount Then Throw New ArgumentException("Scores length must match input row count.")
            If weights IsNot Nothing AndAlso weights.Length <> input.RowCount Then Throw New ArgumentException("Weights length must match input row count.")
            If binCount < 2 Then binCount = 20
            If weights Is Nothing Then weights = Matrix.IdentityVect(input.RowCount - 1)

            Dim result As New PsmOverlapDiagnosticsResult()
            Dim treatedScores = ScoresForGroup(input, scores, True)
            Dim controlScores = ScoresForGroup(input, scores, False)
            If treatedScores.Length = 0 OrElse controlScores.Length = 0 Then
                result.Warnings.Add("Overlap diagnostics require at least one treated and one control observation.")
                Return result
            End If

            Dim overlapLow As Double = Math.Max(treatedScores.Min(), controlScores.Min())
            Dim overlapHigh As Double = Math.Min(treatedScores.Max(), controlScores.Max())
            result.Summary.Add(BuildOverlapSummary("Treated", treatedScores, overlapLow, overlapHigh, extremeLower, extremeUpper))
            result.Summary.Add(BuildOverlapSummary("Control", controlScores, overlapLow, overlapHigh, extremeLower, extremeUpper))
            result.Bins.AddRange(BuildOverlapBins(input, scores, weights, binCount))

            If overlapLow >= overlapHigh Then result.Warnings.Add("Estimated propensity-score ranges do not overlap.")
            If System.Linq.Enumerable.Count(treatedScores, Function(p) p < overlapLow OrElse p > overlapHigh) > 0 OrElse System.Linq.Enumerable.Count(controlScores, Function(p) p < overlapLow OrElse p > overlapHigh) > 0 Then result.Warnings.Add("Some observations are outside the common-support range.")
            If treatedScores.Any(Function(p) p <= extremeLower OrElse p >= extremeUpper) OrElse controlScores.Any(Function(p) p <= extremeLower OrElse p >= extremeUpper) Then result.Warnings.Add("Some propensity scores are close to 0 or 1; weighting and matching stability should be reviewed.")
            Return result
        End Function

        Private Shared Function ScoresForGroup(input As PsmInputData, scores As Double(), treated As Boolean) As Double()
            Dim vals As New List(Of Double)()
            For i As Integer = 0 To input.RowCount - 1
                If (input.Treatment(i) >= 0.5) = treated Then vals.Add(scores(i))
            Next
            Return vals.ToArray()
        End Function

        Private Shared Function BuildOverlapSummary(groupName As String, values As Double(), overlapLow As Double, overlapHigh As Double, extremeLower As Double, extremeUpper As Double) As PsmOverlapSummaryRow
            Dim flags As New List(Of String)()
            Dim below As Integer = System.Linq.Enumerable.Count(values, Function(p) p < overlapLow)
            Dim above As Integer = System.Linq.Enumerable.Count(values, Function(p) p > overlapHigh)
            Dim extremeLow As Integer = System.Linq.Enumerable.Count(values, Function(p) p <= extremeLower)
            Dim extremeHigh As Integer = System.Linq.Enumerable.Count(values, Function(p) p >= extremeUpper)
            If below + above > 0 Then flags.Add("Outside support")
            If extremeLow + extremeHigh > 0 Then flags.Add("Extreme PS")

            Return New PsmOverlapSummaryRow With {
                .GroupName = groupName,
                .N = values.Length,
                .MinScore = values.Min(),
                .Q1Score = PsmMath.Quantile(values, 0.25),
                .MedianScore = PsmMath.Quantile(values, 0.5),
                .Q3Score = PsmMath.Quantile(values, 0.75),
                .MaxScore = values.Max(),
                .MeanScore = values.Average(),
                .SdScore = PsmMath.StandardDeviation(values),
                .BelowOverlapRangeN = below,
                .AboveOverlapRangeN = above,
                .ExtremeLowN = extremeLow,
                .ExtremeHighN = extremeHigh,
                .Flag = If(flags.Count = 0, "OK", String.Join(",", flags.ToArray()))
            }
        End Function

        Private Shared Function BuildOverlapBins(input As PsmInputData, scores As Double(), weights As Double(), binCount As Integer) As List(Of PsmOverlapBinRow)
            Dim rows As New List(Of PsmOverlapBinRow)()
            For b As Integer = 0 To binCount - 1
                Dim lo As Double = CDbl(b) / CDbl(binCount)
                Dim hi As Double = CDbl(b + 1) / CDbl(binCount)
                rows.Add(New PsmOverlapBinRow With {.BinIndex = b + 1, .LowerBound = lo, .UpperBound = hi})
            Next

            For i As Integer = 0 To input.RowCount - 1
                Dim p As Double = PsmMath.Clamp(scores(i), 0.0, 1.0)
                Dim index As Integer = Math.Min(binCount - 1, Math.Max(0, CInt(Math.Floor(p * CDbl(binCount)))))
                If input.Treatment(i) >= 0.5 Then
                    rows(index).TreatedN += 1
                    rows(index).TreatedWeight += weights(i)
                Else
                    rows(index).ControlN += 1
                    rows(index).ControlWeight += weights(i)
                End If
            Next
            Return rows
        End Function

        Public Shared Function BuildLovePlotRows(result As PsmResult, Optional threshold As Double = 0.1) As List(Of PsmLovePlotRow)
            If result Is Nothing OrElse result.Balance Is Nothing Then Return New List(Of PsmLovePlotRow)()
            If result.Options IsNot Nothing Then threshold = result.Options.BalanceSmdThreshold

            Dim rows As New List(Of PsmLovePlotRow)()
            For Each variableGroup In result.Balance.GroupBy(Function(r) r.VariableName)
                Dim before = variableGroup.FirstOrDefault(Function(r) r.Sample = PsmBalanceSample.Before)
                Dim afterMatching = variableGroup.FirstOrDefault(Function(r) r.Sample = PsmBalanceSample.AfterMatching)
                Dim afterWeighting = variableGroup.FirstOrDefault(Function(r) r.Sample = PsmBalanceSample.AfterWeighting)
                Dim smdBefore As Double = If(before Is Nothing, Double.NaN, before.StandardizedMeanDifference)
                Dim smdMatch As Double = If(afterMatching Is Nothing, Double.NaN, afterMatching.StandardizedMeanDifference)
                Dim smdWeight As Double = If(afterWeighting Is Nothing, Double.NaN, afterWeighting.StandardizedMeanDifference)
                Dim maxAfter As Double = MaxFiniteAbs(smdMatch, smdWeight)
                rows.Add(New PsmLovePlotRow With {
                    .VariableName = variableGroup.Key,
                    .SmdBefore = smdBefore,
                    .SmdAfterMatching = smdMatch,
                    .SmdAfterWeighting = smdWeight,
                    .MaxAbsAfter = maxAfter,
                    .Threshold = threshold,
                    .Flag = If(AppInfrastructure.IsFinite(maxAfter) AndAlso maxAfter > threshold, "Review", "OK")
                })
            Next

            Return rows.OrderByDescending(Function(r) If(AppInfrastructure.IsFinite(r.MaxAbsAfter), r.MaxAbsAfter, 0.0)).ThenBy(Function(r) r.VariableName).ToList()
        End Function

        Private Shared Function MaxFiniteAbs(ParamArray values() As Double) As Double
            Dim best As Double = Double.NaN
            For Each v In values
                If AppInfrastructure.IsFinite(v) Then
                    Dim av As Double = Math.Abs(v)
                    If Not AppInfrastructure.IsFinite(best) OrElse av > best Then best = av
                End If
            Next
            Return best
        End Function
    End Class

    ''' <summary>
    ''' Augmented inverse-probability weighted effect estimator.
    ''' Outcome regressions are deliberately simple ridge-stabilised linear models, making the estimator reusable
    ''' without adding Excel/GUI dependencies. For binary outcomes this is a linear-probability outcome model.
    ''' </summary>
    Public NotInheritable Class PsmDoublyRobustEstimator
        Private Sub New()
        End Sub

        Public Shared Function EstimateAipw(input As PsmInputData, scores As Double(), Optional options As PsmOptions = Nothing) As PsmDoublyRobustResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If input.Outcome Is Nothing Then Throw New ArgumentException("Outcome is required for doubly robust estimation.")
            If scores Is Nothing OrElse scores.Length <> input.RowCount Then Throw New ArgumentException("Scores length must match input row count.")
            If options Is Nothing Then options = New PsmOptions()

            Dim result As New PsmDoublyRobustResult()
            Dim treatedModel As PsmOutcomeModelResult = FitOutcomeModel(input, True, options.LogisticRidgePenalty)
            Dim controlModel As PsmOutcomeModelResult = FitOutcomeModel(input, False, options.LogisticRidgePenalty)
            result.TreatedOutcomeModel = treatedModel
            result.ControlOutcomeModel = controlModel
            result.Warnings.AddRange(treatedModel.Warnings)
            result.Warnings.AddRange(controlModel.Warnings)

            Dim m1(input.RowCount - 1) As Double
            Dim m0(input.RowCount - 1) As Double
            For i As Integer = 0 To input.RowCount - 1
                m1(i) = PredictOutcome(input, treatedModel.Coefficients, i)
                m0(i) = PredictOutcome(input, controlModel.Coefficients, i)
            Next

            Dim contributions As New List(Of Double)()
            Select Case options.Estimand
                Case PsmEstimand.ATE
                    For i As Integer = 0 To input.RowCount - 1
                        Dim t As Double = If(input.Treatment(i) >= 0.5, 1.0, 0.0)
                        Dim e As Double = PsmMath.Clamp(scores(i), 0.00000001, 0.99999999)
                        Dim y As Double = input.Outcome(i)
                        contributions.Add(m1(i) - m0(i) + t * (y - m1(i)) / e - (1.0 - t) * (y - m0(i)) / (1.0 - e))
                    Next
                Case PsmEstimand.ATO
                    'Overlap-targeted augmented estimator.  This uses the usual
                    'balancing-weight tilting function h(x)=e(x)*(1-e(x)), so the
                    'estimand is the average treatment effect in the overlap
                    'population rather than an ATE fallback.
                    Dim overlapSum As Double = 0.0
                    Dim numerators(input.RowCount - 1) As Double
                    For i As Integer = 0 To input.RowCount - 1
                        Dim t As Double = If(input.Treatment(i) >= 0.5, 1.0, 0.0)
                        Dim e As Double = PsmMath.Clamp(scores(i), 0.00000001, 0.99999999)
                        Dim y As Double = input.Outcome(i)
                        Dim h As Double = e * (1.0 - e)
                        overlapSum += h
                        numerators(i) = h * (m1(i) - m0(i)) + (1.0 - e) * t * (y - m1(i)) - e * (1.0 - t) * (y - m0(i))
                    Next
                    If overlapSum <= 0 OrElse Not AppInfrastructure.IsFinite(overlapSum) Then Throw New ArgumentException("AIPW ATO could not be estimated because the overlap weights sum to zero.")
                    Dim scale As Double = CDbl(input.RowCount) / overlapSum
                    For i As Integer = 0 To input.RowCount - 1
                        contributions.Add(numerators(i) * scale)
                    Next
                Case PsmEstimand.ATT
                    Dim nT As Integer = System.Linq.Enumerable.Count(input.Treatment, Function(v) v >= 0.5)
                    For i As Integer = 0 To input.RowCount - 1
                        Dim t As Double = If(input.Treatment(i) >= 0.5, 1.0, 0.0)
                        Dim e As Double = PsmMath.Clamp(scores(i), 0.00000001, 0.99999999)
                        Dim y As Double = input.Outcome(i)
                        Dim c As Double = t * (y - m0(i)) - (1.0 - t) * e / (1.0 - e) * (y - m0(i))
                        contributions.Add(c * CDbl(input.RowCount) / CDbl(Math.Max(1, nT)))
                    Next
                Case PsmEstimand.ATC
                    Dim nC As Integer = System.Linq.Enumerable.Count(input.Treatment, Function(v) v < 0.5)
                    For i As Integer = 0 To input.RowCount - 1
                        Dim t As Double = If(input.Treatment(i) >= 0.5, 1.0, 0.0)
                        Dim e As Double = PsmMath.Clamp(scores(i), 0.00000001, 0.99999999)
                        Dim y As Double = input.Outcome(i)
                        Dim c As Double = t * (1.0 - e) / e * (y - m1(i)) - (1.0 - t) * (y - m1(i))
                        contributions.Add(c * CDbl(input.RowCount) / CDbl(Math.Max(1, nC)))
                    Next
            End Select

            Dim vals As Double() = contributions.Where(Function(v) AppInfrastructure.IsFinite(v)).ToArray()
            If vals.Length = 0 Then Return result
            Dim est As Double = vals.Average()
            Dim se As Double = If(vals.Length > 1, Math.Sqrt(PsmMath.Variance(vals, True) / CDbl(vals.Length)), Double.NaN)
            result.IndividualContributions = vals
            result.Effect = New PsmEffectResult With {
                .Estimand = options.Estimand,
                .Method = "Doubly robust AIPW",
                .OutcomeType = PsmOutcomeAnalysis.InferOutcomeType(input.Outcome),
                .Estimate = est,
                .StandardError = se,
                .LowerConfidenceLimit = If(AppInfrastructure.IsFinite(se), est - 1.95996398454005 * se, Double.NaN),
                .UpperConfidenceLimit = If(AppInfrastructure.IsFinite(se), est + 1.95996398454005 * se, Double.NaN),
                .TreatedMean = MeanByGroup(input, True),
                .ControlMean = MeanByGroup(input, False),
                .EffectiveTreatedN = System.Linq.Enumerable.Count(input.Treatment, Function(v) v >= 0.5),
                .EffectiveControlN = System.Linq.Enumerable.Count(input.Treatment, Function(v) v < 0.5),
                .MatchedSets = 0,
                .Warnings = result.Warnings
            }
            Return result
        End Function

        Private Shared Function FitOutcomeModel(input As PsmInputData, treated As Boolean, ridge As Double) As PsmOutcomeModelResult
            Dim p As Integer = input.CovariateCount + 1
            Dim names(p - 1) As String
            names(0) = "Intercept"
            For j As Integer = 0 To input.CovariateCount - 1
                names(j + 1) = input.GetCovariateName(j)
            Next

            Dim xtx(p - 1, p - 1) As Double
            Dim xty(p - 1) As Double
            Dim n As Integer = 0
            For i As Integer = 0 To input.RowCount - 1
                If (input.Treatment(i) >= 0.5) <> treated Then Continue For
                n += 1
                Dim x As Double() = DesignRow(input, i)
                For j As Integer = 0 To p - 1
                    xty(j) += x(j) * input.Outcome(i)
                    For k As Integer = 0 To p - 1
                        xtx(j, k) += x(j) * x(k)
                    Next
                Next
            Next

            If ridge < 0 OrElse Double.IsNaN(ridge) Then ridge = 0.000001
            If ridge = 0.0 Then ridge = 0.000001
            For j As Integer = 1 To p - 1
                xtx(j, j) += ridge
            Next

            Dim coefficients(p - 1) As Double
            Dim warnings As New List(Of String)()
            Try
                Dim fault As Integer = 0
                Dim chol As Double(,) = Matrix.Cholesky(xtx, fault, False)
                If fault = 0 Then
                    coefficients = Matrix.CholSolve(chol, xty)
                Else
                    warnings.Add("Outcome model information matrix was not positive definite; using Matrix.MatInv fallback.")
                    coefficients = Matrix.MatrixVectorMultiply(Matrix.MatInv(xtx, "CHOL"), xty)
                End If
            Catch ex As Exception
                warnings.Add("Outcome model could not be fitted: " & ex.Message)
                For j As Integer = 0 To p - 1
                    coefficients(j) = Double.NaN
                Next
            End Try

            Dim residuals As New List(Of Double)()
            If coefficients IsNot Nothing AndAlso coefficients.Length = p Then
                For i As Integer = 0 To input.RowCount - 1
                    If (input.Treatment(i) >= 0.5) <> treated Then Continue For
                    Dim pred As Double = PredictOutcome(input, coefficients, i)
                    If AppInfrastructure.IsFinite(pred) Then residuals.Add(input.Outcome(i) - pred)
                Next
            End If

            Return New PsmOutcomeModelResult With {
                .TreatmentLevel = If(treated, 1, 0),
                .Coefficients = coefficients,
                .VariableNames = names,
                .ResidualVariance = If(residuals.Count > p, residuals.Select(Function(r) r * r).Sum() / CDbl(residuals.Count - p), Double.NaN),
                .N = n,
                .Warnings = warnings
            }
        End Function

        Private Shared Function DesignRow(input As PsmInputData, rowIndex As Integer) As Double()
            Dim x(input.CovariateCount) As Double
            x(0) = 1.0
            For j As Integer = 0 To input.CovariateCount - 1
                x(j + 1) = input.Covariates(rowIndex, j)
            Next
            Return x
        End Function

        Private Shared Function PredictOutcome(input As PsmInputData, coefficients As Double(), rowIndex As Integer) As Double
            If coefficients Is Nothing OrElse coefficients.Length <> input.CovariateCount + 1 Then Return Double.NaN
            Dim x As Double() = DesignRow(input, rowIndex)
            Dim yhat As Double = 0.0
            For j As Integer = 0 To coefficients.Length - 1
                If Not AppInfrastructure.IsFinite(coefficients(j)) Then Return Double.NaN
                yhat += coefficients(j) * x(j)
            Next
            Return yhat
        End Function

        Private Shared Function MeanByGroup(input As PsmInputData, treated As Boolean) As Double
            Dim vals As New List(Of Double)()
            For i As Integer = 0 To input.RowCount - 1
                If (input.Treatment(i) >= 0.5) = treated Then vals.Add(input.Outcome(i))
            Next
            Return If(vals.Count > 0, vals.Average(), Double.NaN)
        End Function
    End Class

    ''' <summary>
    ''' Simple non-parametric bootstrap wrapper for backend effect estimates.
    ''' The bootstrap refits the score model in each replicate when options.ScoreMethod is logistic regression.
    ''' </summary>
    Public NotInheritable Class PsmBootstrapRunner
        Private Sub New()
        End Sub

        Public Shared Function BootstrapEffect(input As PsmInputData, options As PsmOptions, target As PsmBootstrapTarget, Optional replications As Integer = 200, Optional seed As Integer = 12345) As PsmBootstrapResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If options Is Nothing Then options = New PsmOptions()
            If replications < 2 Then Throw New ArgumentOutOfRangeException("replications", "At least two bootstrap replications are required.")

            Dim original As Double = EstimateTarget(input, options, target)
            Dim rng As New Random(seed)
            Dim estimates As New List(Of Double)()
            Dim warnings As New List(Of String)()

            Dim replicateNumber As Integer = 0
            For Each indices As Integer() In ResamplingBootstrap.BootstrapIndices(input.RowCount, replications, rng)
                replicateNumber += 1
                Try
                    Dim bootInput As PsmInputData = ResampleInput(input, indices)
                    Dim estimate As Double = EstimateTarget(bootInput, options, target)
                    If AppInfrastructure.IsFinite(estimate) Then estimates.Add(estimate)
                Catch ex As Exception
                    If warnings.Count < 10 Then warnings.Add("Bootstrap replicate " & replicateNumber.ToString() & " failed: " & ex.Message)
                End Try
            Next

            Dim arr As Double() = estimates.ToArray()
            Dim result As New PsmBootstrapResult With {
                .Target = target,
                .ReplicationsRequested = replications,
                .ReplicationsUsed = arr.Length,
                .EstimateOriginal = original,
                .ReplicateEstimates = arr,
                .Warnings = warnings
            }

            If arr.Length > 0 Then
                result.BootstrapMean = arr.Average()
                result.BootstrapStandardError = If(arr.Length > 1, Math.Sqrt(PsmMath.Variance(arr, True)), Double.NaN)
                result.PercentileLower95 = PsmMath.Quantile(arr, 0.025)
                result.PercentileUpper95 = PsmMath.Quantile(arr, 0.975)
            Else
                result.BootstrapMean = Double.NaN
                result.BootstrapStandardError = Double.NaN
                result.PercentileLower95 = Double.NaN
                result.PercentileUpper95 = Double.NaN
                result.Warnings.Add("No valid bootstrap estimates were produced.")
            End If
            Return result
        End Function

        Private Shared Function EstimateTarget(input As PsmInputData, options As PsmOptions, target As PsmBootstrapTarget) As Double
            Dim fit As PsmResult = PsmBackend.Fit(input, options)
            Select Case target
                Case PsmBootstrapTarget.WeightedEffect
                    Return If(fit.WeightedEffect Is Nothing, Double.NaN, fit.WeightedEffect.Estimate)
                Case PsmBootstrapTarget.MatchedEffect
                    Return If(fit.MatchedEffect Is Nothing, Double.NaN, fit.MatchedEffect.Estimate)
                Case PsmBootstrapTarget.SubclassificationEffect
                    Return If(fit.SubclassificationEffect Is Nothing, Double.NaN, fit.SubclassificationEffect.Estimate)
                Case PsmBootstrapTarget.DoublyRobustAipw
                    Dim dr As PsmDoublyRobustResult = PsmDoublyRobustEstimator.EstimateAipw(input, fit.ScoreModel.Scores, options)
                    Return If(dr Is Nothing OrElse dr.Effect Is Nothing, Double.NaN, dr.Effect.Estimate)
                Case Else
                    Return Double.NaN
            End Select
        End Function

        Private Shared Function ResampleInput(input As PsmInputData, indices As Integer()) As PsmInputData
            If indices Is Nothing Then Throw New ArgumentNullException("indices")
            Dim n As Integer = indices.Length
            Dim p As Integer = input.CovariateCount
            Dim ids(n - 1) As String
            Dim treatment(n - 1) As Double
            Dim outcome As Double() = Nothing
            If input.Outcome IsNot Nothing Then outcome = New Double(n - 1) {}
            Dim covariates(n - 1, p - 1) As Double
            Dim scores As Double() = Nothing
            If input.SuppliedPropensityScores IsNot Nothing Then scores = New Double(n - 1) {}
            Dim groups As String() = Nothing
            If input.ExactGroupLabels IsNot Nothing Then groups = New String(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim src As Integer = indices(i)
                ids(i) = input.GetId(src) & ":b" & (i + 1).ToString()
                treatment(i) = input.Treatment(src)
                If outcome IsNot Nothing Then outcome(i) = input.Outcome(src)
                For j As Integer = 0 To p - 1
                    covariates(i, j) = input.Covariates(src, j)
                Next
                If scores IsNot Nothing Then scores(i) = input.SuppliedPropensityScores(src)
                If groups IsNot Nothing Then groups(i) = If(input.ExactGroupLabels(src), "")
            Next

            Return New PsmInputData With {
                .Ids = ids,
                .Treatment = treatment,
                .Outcome = outcome,
                .Covariates = covariates,
                .CovariateNames = input.CovariateNames,
                .SuppliedPropensityScores = scores,
                .ExactGroupLabels = groups
            }
        End Function
    End Class

    ''' <summary>
    ''' Advanced backend tables for GUI and UDF front ends.
    ''' </summary>
    Public NotInheritable Class PsmAdvancedTables
        Private Sub New()
        End Sub

        Public Shared Function WeightDiagnosticsTable(rows As List(Of PsmWeightDiagnosticsRow)) As Object(,)
            If rows Is Nothing OrElse rows.Count = 0 Then Return PsmResult.EmptyTable("No weight diagnostics available")
            Dim table(rows.Count, 12) As Object
            table(0, 0) = "Sample" : table(0, 1) = "Group" : table(0, 2) = "N" : table(0, 3) = "Non-zero N" : table(0, 4) = "Sum W" : table(0, 5) = "Mean W" : table(0, 6) = "Min W" : table(0, 7) = "Max W" : table(0, 8) = "CV" : table(0, 9) = "ESS" : table(0, 10) = "ESS/N" : table(0, 11) = "Extreme W N" : table(0, 12) = "Flag"
            For i As Integer = 0 To rows.Count - 1
                Dim r = rows(i)
                table(i + 1, 0) = r.SampleName
                table(i + 1, 1) = r.GroupName
                table(i + 1, 2) = r.N
                table(i + 1, 3) = r.NonZeroN
                table(i + 1, 4) = r.SumWeights
                table(i + 1, 5) = r.MeanWeight
                table(i + 1, 6) = r.MinWeight
                table(i + 1, 7) = r.MaxWeight
                table(i + 1, 8) = r.CoefficientOfVariation
                table(i + 1, 9) = r.EffectiveSampleSize
                table(i + 1, 10) = r.EffectiveSampleSizeRatio
                table(i + 1, 11) = r.ExtremeWeightN
                table(i + 1, 12) = r.Flag
            Next
            Return table
        End Function

        Public Shared Function OverlapSummaryTable(result As PsmOverlapDiagnosticsResult) As Object(,)
            If result Is Nothing OrElse result.Summary Is Nothing OrElse result.Summary.Count = 0 Then Return PsmResult.EmptyTable("No overlap summary available")
            Dim rows = result.Summary
            Dim table(rows.Count, 12) As Object
            table(0, 0) = "Group" : table(0, 1) = "N" : table(0, 2) = "Min" : table(0, 3) = "Q1" : table(0, 4) = "Median" : table(0, 5) = "Q3" : table(0, 6) = "Max" : table(0, 7) = "Mean" : table(0, 8) = "SD" : table(0, 9) = "Below overlap" : table(0, 10) = "Above overlap" : table(0, 11) = "Extreme PS" : table(0, 12) = "Flag"
            For i As Integer = 0 To rows.Count - 1
                Dim r = rows(i)
                table(i + 1, 0) = r.GroupName
                table(i + 1, 1) = r.N
                table(i + 1, 2) = r.MinScore
                table(i + 1, 3) = r.Q1Score
                table(i + 1, 4) = r.MedianScore
                table(i + 1, 5) = r.Q3Score
                table(i + 1, 6) = r.MaxScore
                table(i + 1, 7) = r.MeanScore
                table(i + 1, 8) = r.SdScore
                table(i + 1, 9) = r.BelowOverlapRangeN
                table(i + 1, 10) = r.AboveOverlapRangeN
                table(i + 1, 11) = r.ExtremeLowN + r.ExtremeHighN
                table(i + 1, 12) = r.Flag
            Next
            Return table
        End Function

        Public Shared Function OverlapBinsTable(result As PsmOverlapDiagnosticsResult) As Object(,)
            If result Is Nothing OrElse result.Bins Is Nothing OrElse result.Bins.Count = 0 Then Return PsmResult.EmptyTable("No overlap bins available")
            Dim rows = result.Bins
            Dim table(rows.Count, 6) As Object
            table(0, 0) = "Bin" : table(0, 1) = "Lower PS" : table(0, 2) = "Upper PS" : table(0, 3) = "Treated N" : table(0, 4) = "Control N" : table(0, 5) = "Treated W" : table(0, 6) = "Control W"
            For i As Integer = 0 To rows.Count - 1
                Dim r = rows(i)
                table(i + 1, 0) = r.BinIndex
                table(i + 1, 1) = r.LowerBound
                table(i + 1, 2) = r.UpperBound
                table(i + 1, 3) = r.TreatedN
                table(i + 1, 4) = r.ControlN
                table(i + 1, 5) = r.TreatedWeight
                table(i + 1, 6) = r.ControlWeight
            Next
            Return table
        End Function

        Public Shared Function LovePlotTable(rows As List(Of PsmLovePlotRow)) As Object(,)
            If rows Is Nothing OrElse rows.Count = 0 Then Return PsmResult.EmptyTable("No Love plot data available")
            Dim table(rows.Count, 6) As Object
            table(0, 0) = "Variable" : table(0, 1) = "SMD before" : table(0, 2) = "SMD after matching" : table(0, 3) = "SMD after weighting" : table(0, 4) = "Max |after|" : table(0, 5) = "Threshold" : table(0, 6) = "Flag"
            For i As Integer = 0 To rows.Count - 1
                Dim r = rows(i)
                table(i + 1, 0) = r.VariableName
                table(i + 1, 1) = r.SmdBefore
                table(i + 1, 2) = r.SmdAfterMatching
                table(i + 1, 3) = r.SmdAfterWeighting
                table(i + 1, 4) = r.MaxAbsAfter
                table(i + 1, 5) = r.Threshold
                table(i + 1, 6) = r.Flag
            Next
            Return table
        End Function

        Public Shared Function DoublyRobustEffectTable(result As PsmDoublyRobustResult) As Object(,)
            If result Is Nothing OrElse result.Effect Is Nothing Then Return PsmResult.EmptyTable("No doubly robust effect estimate available")
            Dim table(1, 9) As Object
            table(0, 0) = "Method" : table(0, 1) = "Estimand" : table(0, 2) = "Estimate" : table(0, 3) = "Std. Error" : table(0, 4) = "Lower 95%" : table(0, 5) = "Upper 95%" : table(0, 6) = "Treated mean" : table(0, 7) = "Control mean" : table(0, 8) = "Treated N" : table(0, 9) = "Control N"
            Dim e = result.Effect
            table(1, 0) = e.Method
            table(1, 1) = e.Estimand.ToString()
            table(1, 2) = e.Estimate
            table(1, 3) = e.StandardError
            table(1, 4) = e.LowerConfidenceLimit
            table(1, 5) = e.UpperConfidenceLimit
            table(1, 6) = e.TreatedMean
            table(1, 7) = e.ControlMean
            table(1, 8) = e.EffectiveTreatedN
            table(1, 9) = e.EffectiveControlN
            Return table
        End Function

        Public Shared Function BootstrapTable(result As PsmBootstrapResult) As Object(,)
            If result Is Nothing Then Return PsmResult.EmptyTable("No bootstrap result available")
            Dim table(1, 7) As Object
            table(0, 0) = "Target" : table(0, 1) = "Original estimate" : table(0, 2) = "Bootstrap mean" : table(0, 3) = "Bootstrap SE" : table(0, 4) = "Lower 95%" : table(0, 5) = "Upper 95%" : table(0, 6) = "Replications used" : table(0, 7) = "Replications requested"
            table(1, 0) = result.Target.ToString()
            table(1, 1) = result.EstimateOriginal
            table(1, 2) = result.BootstrapMean
            table(1, 3) = result.BootstrapStandardError
            table(1, 4) = result.PercentileLower95
            table(1, 5) = result.PercentileUpper95
            table(1, 6) = result.ReplicationsUsed
            table(1, 7) = result.ReplicationsRequested
            Return table
        End Function

    End Class

End Namespace
