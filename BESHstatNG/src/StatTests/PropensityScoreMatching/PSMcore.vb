Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace CausalInference

    ''' <summary>
    ''' Target estimand for propensity-score methods.
    ''' ATT/ATC are directly supported by nearest-neighbour matching.
    ''' ATE/ATO are primarily supported through weighting and diagnostics.
    ''' </summary>
    Public Enum PsmEstimand
        ATT = 0
        ATC = 1
        ATE = 2
        ATO = 3
    End Enum

    Public Enum PsmScoreMethod
        Supplied = 0
        LogisticRegression = 1
    End Enum

    Public Enum PsmMatchingMethod
        None = 0
        NearestNeighbor = 1
        Subclassification = 2
    End Enum

    Public Enum PsmDistanceMetric
        PropensityScore = 0
        LogitPropensityScore = 1
        Mahalanobis = 2
        MahalanobisWithinPropensityCaliper = 3
    End Enum

    Public Enum PsmCaliperScale
        None = 0
        RawPropensityScore = 1
        StandardizedPropensityScore = 2
        LogitPropensityScore = 3
        StandardizedLogitPropensityScore = 4
    End Enum

    Public Enum PsmMatchingOrder
        AsInput = 0
        PropensityAscending = 1
        PropensityDescending = 2
        Random = 3
        HardestFirst = 4
    End Enum

    Public Enum PsmCommonSupportMode
        None = 0
        DropOutsideOverlapRange = 1
        DropTreatedOutsideControlRange = 2
        DropControlsOutsideTreatedRange = 3
    End Enum

    Public Enum PsmOutcomeType
        Auto = 0
        Continuous = 1
        Binary = 2
    End Enum

    Public Enum PsmBalanceSample
        Before = 0
        AfterMatching = 1
        AfterWeighting = 2
    End Enum

    ''' <summary>
    ''' Reusable backend options. These options intentionally contain no Excel-DNA or WinForms dependencies.
    ''' </summary>
    Public Class PsmOptions
        Public Property ScoreMethod As PsmScoreMethod = PsmScoreMethod.LogisticRegression
        Public Property MatchingMethod As PsmMatchingMethod = PsmMatchingMethod.NearestNeighbor
        Public Property Estimand As PsmEstimand = PsmEstimand.ATT
        Public Property DistanceMetric As PsmDistanceMetric = PsmDistanceMetric.PropensityScore
        Public Property MatchingRatio As Integer = 1
        Public Property WithReplacement As Boolean = False
        Public Property CaliperScale As PsmCaliperScale = PsmCaliperScale.None
        Public Property Caliper As Double = Double.NaN
        Public Property MatchingOrder As PsmMatchingOrder = PsmMatchingOrder.PropensityDescending
        Public Property CommonSupport As PsmCommonSupportMode = PsmCommonSupportMode.None
        Public Property RandomSeed As Integer = 12345
        Public Property IncludeIntercept As Boolean = True
        Public Property StandardizeCovariates As Boolean = True
        Public Property LogisticMaxIterations As Integer = 100
        Public Property LogisticTolerance As Double = 0.0000001
        Public Property LogisticRidgePenalty As Double = 0.000001
        Public Property BalanceSmdThreshold As Double = 0.1
        Public Property BalanceVarianceRatioLower As Double = 0.5
        Public Property BalanceVarianceRatioUpper As Double = 2.0
        Public Property SubclassificationStrata As Integer = 5
        Public Property NormalizeWeightsToSampleSize As Boolean = True
        Public Property TrimPropensityLower As Double = 0.0
        Public Property TrimPropensityUpper As Double = 1.0

        Public Sub Validate()
            If MatchingRatio < 1 Then Throw New ArgumentOutOfRangeException("MatchingRatio", "Matching ratio must be at least 1.")
            If LogisticMaxIterations < 1 Then Throw New ArgumentOutOfRangeException("LogisticMaxIterations", "Maximum iterations must be positive.")
            If LogisticTolerance <= 0 OrElse Double.IsNaN(LogisticTolerance) Then Throw New ArgumentOutOfRangeException("LogisticTolerance", "Tolerance must be positive.")
            If LogisticRidgePenalty < 0 OrElse Double.IsNaN(LogisticRidgePenalty) Then Throw New ArgumentOutOfRangeException("LogisticRidgePenalty", "Ridge penalty cannot be negative.")
            If SubclassificationStrata < 2 Then Throw New ArgumentOutOfRangeException("SubclassificationStrata", "At least two subclasses are required.")
            If TrimPropensityLower < 0 OrElse TrimPropensityLower >= 0.5 Then Throw New ArgumentOutOfRangeException("TrimPropensityLower", "Lower trimming bound must be in [0, 0.5).")
            If TrimPropensityUpper > 1 OrElse TrimPropensityUpper <= 0.5 Then Throw New ArgumentOutOfRangeException("TrimPropensityUpper", "Upper trimming bound must be in (0.5, 1].")
            If TrimPropensityLower >= TrimPropensityUpper Then Throw New ArgumentException("Lower trimming bound must be less than upper trimming bound.")
            If CaliperScale <> PsmCaliperScale.None Then
                If Double.IsNaN(Caliper) OrElse Caliper < 0 Then Throw New ArgumentOutOfRangeException("Caliper", "Caliper must be non-negative when a caliper scale is selected.")
            End If
            If DistanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper AndAlso CaliperScale = PsmCaliperScale.None Then
                Throw New ArgumentException("Mahalanobis-within-propensity-caliper matching requires a propensity-score or logit-propensity caliper.")
            End If
        End Sub
    End Class

    ''' <summary>
    ''' Backend input structure. Rows in Treatment, Outcome, Covariates, SuppliedPropensityScores, Ids and ExactGroupLabels must align.
    ''' Treatment must be coded 0/1.
    ''' </summary>
    Public Class PsmInputData
        Public Property Ids As String()
        Public Property Treatment As Double()
        Public Property Outcome As Double()
        Public Property Covariates As Double(,)
        Public Property CovariateNames As String()
        Public Property SuppliedPropensityScores As Double()
        Public Property ExactGroupLabels As String()

        Public ReadOnly Property RowCount As Integer
            Get
                If Treatment Is Nothing Then Return 0
                Return Treatment.Length
            End Get
        End Property

        Public ReadOnly Property CovariateCount As Integer
            Get
                If Covariates Is Nothing Then Return 0
                Return Covariates.GetLength(1)
            End Get
        End Property

        Public Sub Validate(options As PsmOptions)
            If options Is Nothing Then Throw New ArgumentNullException("options")
            options.Validate()
            If Treatment Is Nothing OrElse Treatment.Length = 0 Then Throw New ArgumentException("Treatment vector is required.")
            If Covariates Is Nothing Then Throw New ArgumentException("Covariate matrix is required.")
            If Covariates.GetLength(0) <> Treatment.Length Then Throw New ArgumentException("Covariate row count must match treatment length.")
            If Covariates.GetLength(1) < 1 Then Throw New ArgumentException("At least one covariate is required.")
            If Ids IsNot Nothing AndAlso Ids.Length <> Treatment.Length Then Throw New ArgumentException("Ids length must match treatment length.")
            If Outcome IsNot Nothing AndAlso Outcome.Length <> Treatment.Length Then Throw New ArgumentException("Outcome length must match treatment length.")
            If ExactGroupLabels IsNot Nothing AndAlso ExactGroupLabels.Length <> Treatment.Length Then Throw New ArgumentException("Exact group labels length must match treatment length.")
            If CovariateNames IsNot Nothing AndAlso CovariateNames.Length <> Covariates.GetLength(1) Then Throw New ArgumentException("Covariate names length must match covariate column count.")
            If options.ScoreMethod = PsmScoreMethod.Supplied Then
                If SuppliedPropensityScores Is Nothing OrElse SuppliedPropensityScores.Length <> Treatment.Length Then Throw New ArgumentException("Supplied propensity scores are required and must match treatment length.")
            End If

            Dim nTreat As Integer = 0
            Dim nControl As Integer = 0
            For i As Integer = 0 To Treatment.Length - 1
                If Not AppInfrastructure.IsFinite(Treatment(i)) Then Throw New ArgumentException("Treatment contains a non-finite value at row " & (i + 1).ToString() & ".")
                If Math.Abs(Treatment(i) - 1.0) < 0.000000000001 Then
                    nTreat += 1
                ElseIf Math.Abs(Treatment(i)) < 0.000000000001 Then
                    nControl += 1
                Else
                    Throw New ArgumentException("Treatment must be coded 0/1. Invalid value at row " & (i + 1).ToString() & ".")
                End If

                For j As Integer = 0 To Covariates.GetLength(1) - 1
                    If Not AppInfrastructure.IsFinite(Covariates(i, j)) Then Throw New ArgumentException("Covariates contain a non-finite value at row " & (i + 1).ToString() & ", column " & (j + 1).ToString() & ".")
                Next

                If Outcome IsNot Nothing AndAlso Not AppInfrastructure.IsFinite(Outcome(i)) Then Throw New ArgumentException("Outcome contains a non-finite value at row " & (i + 1).ToString() & ".")

                If SuppliedPropensityScores IsNot Nothing Then
                    Dim p As Double = SuppliedPropensityScores(i)
                    If Not AppInfrastructure.IsFinite(p) OrElse p <= 0.0 OrElse p >= 1.0 Then Throw New ArgumentException("Propensity scores must be finite and strictly between 0 and 1. Invalid value at row " & (i + 1).ToString() & ".")
                End If
            Next
            If nTreat = 0 OrElse nControl = 0 Then Throw New ArgumentException("Both treatment groups must contain at least one observation.")
        End Sub

        Public Function GetCovariateName(columnIndex As Integer) As String
            If CovariateNames IsNot Nothing AndAlso columnIndex >= 0 AndAlso columnIndex < CovariateNames.Length AndAlso Not String.IsNullOrWhiteSpace(CovariateNames(columnIndex)) Then Return CovariateNames(columnIndex)
            Return "X" & (columnIndex + 1).ToString()
        End Function

        Public Function GetId(rowIndex As Integer) As String
            If Ids IsNot Nothing AndAlso rowIndex >= 0 AndAlso rowIndex < Ids.Length AndAlso Not String.IsNullOrWhiteSpace(Ids(rowIndex)) Then Return Ids(rowIndex)
            Return (rowIndex + 1).ToString()
        End Function
    End Class

    Public Class PsmObservation
        Public Property RowIndex As Integer
        Public Property Id As String
        Public Property Treated As Boolean
        Public Property Outcome As Double = Double.NaN
        Public Property Covariates As Double()
        Public Property PropensityScore As Double = Double.NaN
        Public Property LogitPropensityScore As Double = Double.NaN
        Public Property ExactGroupLabel As String = ""
        Public Property IncludedByCommonSupport As Boolean = True
        Public Property IncludedByTrimming As Boolean = True
    End Class

    Public Class PsmScoreModelResult
        Public Property Method As PsmScoreMethod
        Public Property Scores As Double()
        Public Property LinearPredictor As Double()
        Public Property Coefficients As Double()
        Public Property StandardErrors As Double()
        Public Property VariableNames As String()
        Public Property Converged As Boolean
        Public Property Iterations As Integer
        Public Property LogLikelihood As Double
        Public Property Warnings As New List(Of String)()
    End Class

    Public Class PsmMatchLink
        Public Property SetId As Integer
        Public Property FocalRowIndex As Integer
        Public Property MatchedRowIndex As Integer
        Public Property TreatedRowIndex As Integer
        Public Property ControlRowIndex As Integer
        Public Property Distance As Double
        Public Property PropensityDistance As Double
        Public Property MahalanobisDistance As Double = Double.NaN
        Public Property ExactGroupLabel As String = ""
        Public Property MatchedWeight As Double = 1.0
    End Class

    Public Class PsmBalanceRow
        Public Property Sample As PsmBalanceSample
        Public Property VariableName As String
        Public Property TreatedMean As Double
        Public Property ControlMean As Double
        Public Property TreatedVariance As Double
        Public Property ControlVariance As Double
        Public Property StandardizedMeanDifference As Double
        Public Property VarianceRatio As Double
        Public Property EcdfMeanDifference As Double
        Public Property EcdfMaxDifference As Double
        Public Property TreatedN As Double
        Public Property ControlN As Double
        Public Property Flag As String
    End Class

    Public Class PsmSampleSizeSummary
        Public Property TotalRows As Integer
        Public Property TreatedRows As Integer
        Public Property ControlRows As Integer
        Public Property EligibleTreatedRows As Integer
        Public Property EligibleControlRows As Integer
        Public Property MatchedTreatedRows As Integer
        Public Property MatchedControlRows As Integer
        Public Property MatchedSets As Integer
        Public Property UnmatchedTreatedRows As Integer
        Public Property UnmatchedControlRows As Integer
        Public Property DroppedByCommonSupport As Integer
        Public Property DroppedByTrimming As Integer
    End Class

    Public Class PsmEffectResult
        Public Property Estimand As PsmEstimand
        Public Property Method As String
        Public Property OutcomeType As PsmOutcomeType
        Public Property Estimate As Double
        Public Property StandardError As Double
        Public Property ConfidenceLevel As Double = 0.95
        Public Property LowerConfidenceLimit As Double
        Public Property UpperConfidenceLimit As Double
        Public Property TreatedMean As Double
        Public Property ControlMean As Double
        Public Property EffectiveTreatedN As Double
        Public Property EffectiveControlN As Double
        Public Property MatchedSets As Integer
        Public Property Warnings As New List(Of String)()
    End Class

    Public Class PsmSubclassRow
        Public Property Stratum As Integer
        Public Property LowerScore As Double
        Public Property UpperScore As Double
        Public Property TreatedN As Integer
        Public Property ControlN As Integer
        Public Property TreatedOutcomeMean As Double
        Public Property ControlOutcomeMean As Double
        Public Property Effect As Double
        Public Property Weight As Double
    End Class

    Public Class PsmResult
        Public Property Options As PsmOptions
        Public Property Observations As List(Of PsmObservation)
        Public Property ScoreModel As PsmScoreModelResult
        Public Property MatchingWeights As Double()
        Public Property BalancingWeights As Double()
        Public Property Matches As New List(Of PsmMatchLink)()
        Public Property Balance As New List(Of PsmBalanceRow)()
        Public Property Subclasses As New List(Of PsmSubclassRow)()
        Public Property MatchedEffect As PsmEffectResult
        Public Property WeightedEffect As PsmEffectResult
        Public Property SubclassificationEffect As PsmEffectResult
        Public Property SampleSize As PsmSampleSizeSummary
        Public Property Warnings As New List(Of String)()

        Public Shared Function EmptyTable(message As String) As Object(,)
            Dim table(0, 0) As Object
            table(0, 0) = message
            Return table
        End Function
    End Class

    ''' <summary>
    ''' Numeric helpers shared by all PSM backend batches.
    ''' </summary>
    Public NotInheritable Class PsmMath
        Private Sub New()
        End Sub

        Public Shared Function Clamp(value As Double, lower As Double, upper As Double) As Double
            If value < lower Then Return lower
            If value > upper Then Return upper
            Return value
        End Function

        Public Shared Function SafeLogit(p As Double) As Double
            Dim q As Double = Clamp(p, 0.000000000001, 0.999999999999)
            Return Math.Log(q / (1.0 - q))
        End Function

        Public Shared Function Variance(values As IEnumerable(Of Double), Optional sampleVariance As Boolean = True) As Double
            If values Is Nothing Then Return Double.NaN
            Dim finiteValues As Double() = values.Where(Function(v) AppInfrastructure.IsFinite(v)).ToArray()
            If finiteValues.Length = 0 Then Return Double.NaN
            Return StatFunc.variance(finiteValues, sampleVariance)
        End Function

        Public Shared Function StandardDeviation(values As IEnumerable(Of Double)) As Double
            If values Is Nothing Then Return Double.NaN
            Dim finiteValues As Double() = values.Where(Function(v) AppInfrastructure.IsFinite(v)).ToArray()
            If finiteValues.Length = 0 Then Return Double.NaN
            Return StatFunc.stDev(finiteValues, True)
        End Function

        Public Shared Function WeightedMean(values As Double(), weights As Double()) As Double
            If values Is Nothing OrElse weights Is Nothing OrElse values.Length <> weights.Length Then Return Double.NaN
            Dim sw As Double = 0.0
            Dim sy As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                If AppInfrastructure.IsFinite(values(i)) AndAlso AppInfrastructure.IsFinite(weights(i)) AndAlso weights(i) > 0 Then
                    sw += weights(i)
                    sy += weights(i) * values(i)
                End If
            Next
            If sw <= 0 Then Return Double.NaN
            Return sy / sw
        End Function

        Public Shared Function WeightedVariance(values As Double(), weights As Double(), Optional sampleVariance As Boolean = True) As Double
            If values Is Nothing OrElse weights Is Nothing OrElse values.Length <> weights.Length Then Return Double.NaN
            Dim mu As Double = WeightedMean(values, weights)
            If Not AppInfrastructure.IsFinite(mu) Then Return Double.NaN
            Dim sw As Double = 0.0
            Dim sw2 As Double = 0.0
            Dim ss As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                If AppInfrastructure.IsFinite(values(i)) AndAlso AppInfrastructure.IsFinite(weights(i)) AndAlso weights(i) > 0 Then
                    sw += weights(i)
                    sw2 += weights(i) * weights(i)
                    Dim d As Double = values(i) - mu
                    ss += weights(i) * d * d
                End If
            Next
            If sw <= 0 Then Return Double.NaN
            If Not sampleVariance Then Return ss / sw
            Dim denom As Double = sw - (sw2 / sw)
            If denom <= 0 Then Return Double.NaN
            Return ss / denom
        End Function

        Public Shared Function EffectiveSampleSize(weights As Double()) As Double
            If weights Is Nothing Then Return 0.0
            Dim sw As Double = 0.0
            Dim sw2 As Double = 0.0
            For Each w In weights
                If AppInfrastructure.IsFinite(w) AndAlso w > 0 Then
                    sw += w
                    sw2 += w * w
                End If
            Next
            If sw2 <= 0 Then Return 0.0
            Return (sw * sw) / sw2
        End Function

        Public Shared Function PooledStandardDeviation(varTreated As Double, varControl As Double) As Double
            If Not AppInfrastructure.IsFinite(varTreated) OrElse Not AppInfrastructure.IsFinite(varControl) Then Return Double.NaN
            If varTreated < 0 OrElse varControl < 0 Then Return Double.NaN
            Return Math.Sqrt((varTreated + varControl) / 2.0)
        End Function

        Public Shared Function StandardizedMeanDifference(meanTreated As Double, meanControl As Double, varTreated As Double, varControl As Double) As Double
            Dim psd As Double = PooledStandardDeviation(varTreated, varControl)
            If Not AppInfrastructure.IsFinite(psd) OrElse psd <= 0 Then Return Double.NaN
            Return (meanTreated - meanControl) / psd
        End Function

        Public Shared Function VarianceRatio(varTreated As Double, varControl As Double) As Double
            If Not AppInfrastructure.IsFinite(varTreated) OrElse Not AppInfrastructure.IsFinite(varControl) OrElse varControl <= 0 Then Return Double.NaN
            Return varTreated / varControl
        End Function

        Public Shared Function Quantile(values As IEnumerable(Of Double), probability As Double) As Double
            Dim list As List(Of Double) = values.Where(Function(v) AppInfrastructure.IsFinite(v)).OrderBy(Function(v) v).ToList()
            If list.Count = 0 Then Return Double.NaN
            Dim p As Double = Clamp(probability, 0.0, 1.0)
            Dim pos As Double = p * CDbl(list.Count - 1)
            Dim lo As Integer = CInt(Math.Floor(pos))
            Dim hi As Integer = CInt(Math.Ceiling(pos))
            If lo = hi Then Return list(lo)
            Dim h As Double = pos - CDbl(lo)
            Return list(lo) * (1.0 - h) + list(hi) * h
        End Function

        Public Shared Function EcdfDifference(valuesTreated As Double(), weightsTreated As Double(), valuesControl As Double(), weightsControl As Double()) As Tuple(Of Double, Double)
            Dim grid As New List(Of Double)()
            If valuesTreated IsNot Nothing Then
                For Each v In valuesTreated
                    If AppInfrastructure.IsFinite(v) Then grid.Add(v)
                Next
            End If
            If valuesControl IsNot Nothing Then
                For Each v In valuesControl
                    If AppInfrastructure.IsFinite(v) Then grid.Add(v)
                Next
            End If
            grid = grid.Distinct().OrderBy(Function(v) v).ToList()
            If grid.Count = 0 Then Return Tuple.Create(Double.NaN, Double.NaN)

            Dim totalT As Double = SumPositiveWeights(weightsTreated, valuesTreated)
            Dim totalC As Double = SumPositiveWeights(weightsControl, valuesControl)
            If totalT <= 0 OrElse totalC <= 0 Then Return Tuple.Create(Double.NaN, Double.NaN)

            Dim sumAbs As Double = 0.0
            Dim maxAbs As Double = 0.0
            For Each g In grid
                Dim ft As Double = WeightedEcdfAt(valuesTreated, weightsTreated, g, totalT)
                Dim fc As Double = WeightedEcdfAt(valuesControl, weightsControl, g, totalC)
                Dim d As Double = Math.Abs(ft - fc)
                sumAbs += d
                If d > maxAbs Then maxAbs = d
            Next
            Return Tuple.Create(sumAbs / CDbl(grid.Count), maxAbs)
        End Function

        Private Shared Function WeightedEcdfAt(values As Double(), weights As Double(), threshold As Double, totalWeight As Double) As Double
            Dim s As Double = 0.0
            If values Is Nothing OrElse weights Is Nothing Then Return Double.NaN
            For i As Integer = 0 To values.Length - 1
                If AppInfrastructure.IsFinite(values(i)) AndAlso values(i) <= threshold AndAlso AppInfrastructure.IsFinite(weights(i)) AndAlso weights(i) > 0 Then s += weights(i)
            Next
            Return s / totalWeight
        End Function

        Private Shared Function SumPositiveWeights(weights As Double(), values As Double()) As Double
            If weights Is Nothing OrElse values Is Nothing Then Return 0.0
            Dim s As Double = 0.0
            For i As Integer = 0 To Math.Min(weights.Length, values.Length) - 1
                If AppInfrastructure.IsFinite(values(i)) AndAlso AppInfrastructure.IsFinite(weights(i)) AndAlso weights(i) > 0 Then s += weights(i)
            Next
            Return s
        End Function

        Public Shared Function CopyVector(values As Double()) As Double()
            If values Is Nothing Then Return Nothing
            Dim output(values.Length - 1) As Double
            Array.Copy(values, output, values.Length)
            Return output
        End Function
    End Class

End Namespace
