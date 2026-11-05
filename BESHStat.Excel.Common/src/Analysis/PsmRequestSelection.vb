Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports BESHStatNG.CausalInference

Namespace ExcelCommon
    ''' <summary>Existing dialog defaults; Core's raw ridge default deliberately remains different.</summary>
    Public NotInheritable Class PsmSelectionDefaults
        Private Sub New()
        End Sub
        Public Const LogisticTolerance As Double = 0.0000001R
        Public Const LogisticRidgePenalty As Double = 0.0000001R
        Public Const MaximumIterations As Integer = 100
        Public Const MatchingRatio As Integer = 1
        Public Const Strata As Integer = 5
        Public Const CemBins As Integer = 5
        Public Const OverlapBins As Integer = 20
        Public Const LowerTrim As Double = 0.0R
        Public Const UpperTrim As Double = 1.0R
        Public Const SmdThreshold As Double = 0.1R
        Public Const ExtremeWeightCutoff As Double = 10.0R
        Public Const MahalanobisCaliper As Double = 0.2R
    End Class
    ''' <summary>Detached, already parsed Windows selections. No parsing or validation is added.
    ''' Selected options and effective options are separate fresh Core objects. Matrices are not copied.</summary>
    Public NotInheritable Class PsmRequestSelection
        Private ReadOnly selected As PsmComprehensiveFitOptions
        Public Sub New(options As PsmComprehensiveFitOptions)
            selected = Copy(options)
        End Sub
        Public Function SelectedOptions() As PsmComprehensiveFitOptions
            Return Copy(selected)
        End Function
        Public Function CreateEffectiveOptions() As PsmComprehensiveFitOptions
            Dim fit = Copy(selected)
            Normalize(fit)
            Return fit
        End Function
        Public Shared Function CreateCemSpec(bins As Integer, standard As PsmOptions) As PsmCoarseningSpec
            Return New PsmCoarseningSpec With {.DefaultCovariateBins = bins, .PropensityScoreBins = bins,
                .Estimand = standard.Estimand, .NormalizeWeightsToSampleSize = standard.NormalizeWeightsToSampleSize}
        End Function
        ''' <summary>Preserves Ui20 normalization, including inactive values; Core validates later.</summary>
        Public Shared Sub Normalize(fit As PsmComprehensiveFitOptions)
            If fit.StandardOptions Is Nothing Then Return
            Dim options = fit.StandardOptions
            If fit.RunMethod = PsmBackendRunMethod.OptimalPairMatching Then
                options.MatchingRatio = 1
                options.WithReplacement = False
                options.MatchingOrder = PsmMatchingOrder.AsInput
            End If
            If Not PsmMethodCapabilities.UsesMatchingControls(fit.RunMethod) Then
                options.DistanceMetric = PsmDistanceMetric.PropensityScore
                options.CaliperScale = PsmCaliperScale.None
                options.Caliper = Double.NaN
                options.MatchingRatio = 1
                options.WithReplacement = False
                options.MatchingOrder = PsmMatchingOrder.AsInput
            End If
        End Sub
        Private Shared Function Copy(source As PsmComprehensiveFitOptions) As PsmComprehensiveFitOptions
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            Dim output As New PsmComprehensiveFitOptions With {
                .StandardOptions = If(source.StandardOptions Is Nothing, Nothing, PsmComprehensiveBackend.CloneOptions(source.StandardOptions)),
                .RunMethod = source.RunMethod, .IncludeDoublyRobustEstimate = source.IncludeDoublyRobustEstimate,
                .IncludeOverlapDiagnostics = source.IncludeOverlapDiagnostics, .IncludeWeightDiagnostics = source.IncludeWeightDiagnostics,
                .IncludeLovePlotRows = source.IncludeLovePlotRows, .OverlapBinCount = source.OverlapBinCount,
                .LovePlotThreshold = source.LovePlotThreshold, .ExtremeWeightCutoff = source.ExtremeWeightCutoff}
            Dim spec = source.CoarseningSpec
            If spec IsNot Nothing Then
                output.CoarseningSpec = New PsmCoarseningSpec With {
                    .BinCounts = If(spec.BinCounts Is Nothing, Nothing, CType(spec.BinCounts.Clone(), Integer())),
                    .CutPoints = If(spec.CutPoints Is Nothing, Nothing, spec.CutPoints.Select(Function(a) If(a Is Nothing, Nothing, CType(a.Clone(), Double()))).ToArray()),
                    .IncludePropensityScore = spec.IncludePropensityScore, .PropensityScoreBins = spec.PropensityScoreBins,
                    .DefaultCovariateBins = spec.DefaultCovariateBins, .Estimand = spec.Estimand,
                    .NormalizeWeightsToSampleSize = spec.NormalizeWeightsToSampleSize, .DropUnmatchedStrata = spec.DropUnmatchedStrata}
            End If
            Return output
        End Function
    End Class

    ''' <summary>Capability-derived display intent; WinForms owns events, retained text and mutation timing.</summary>
    Public NotInheritable Class PsmControlIntent
        Public Sub New(method As PsmBackendRunMethod, estimand As PsmEstimand, score As PsmScoreMethod)
            Matching = PsmMethodCapabilities.UsesMatchingControls(method)
            NearestNeighbor = PsmMethodCapabilities.UsesNearestNeighborOptions(method)
            OptimalPair = PsmMethodCapabilities.UsesOptimalPairOptions(method)
            Subclassification = PsmMethodCapabilities.UsesSubclassificationControls(method)
            Cem = PsmMethodCapabilities.UsesCemControls(method)
            DoublyRobust = PsmMethodCapabilities.SupportsDoublyRobust(method, estimand)
            Supplied = score = PsmScoreMethod.Supplied
        End Sub
        Public ReadOnly Property Matching As Boolean
        Public ReadOnly Property NearestNeighbor As Boolean
        Public ReadOnly Property OptimalPair As Boolean
        Public ReadOnly Property Subclassification As Boolean
        Public ReadOnly Property Cem As Boolean
        Public ReadOnly Property DoublyRobust As Boolean
        Public ReadOnly Property Supplied As Boolean
        Public Shared Function Estimands(method As PsmBackendRunMethod) As PsmEstimand()
            Return PsmMethodCapabilities.SupportedEstimands(method)
        End Function
        Public Shared Function NeedsCaliperDefault(distance As PsmDistanceMetric, scale As PsmCaliperScale) As Boolean
            Return distance = PsmDistanceMetric.MahalanobisWithinPropensityCaliper AndAlso scale = PsmCaliperScale.None
        End Function
    End Class
End Namespace
