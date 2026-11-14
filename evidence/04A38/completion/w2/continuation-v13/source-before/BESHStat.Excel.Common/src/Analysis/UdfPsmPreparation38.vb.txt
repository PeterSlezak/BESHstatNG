Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon.PropensityScoreUDFsOptions38

Namespace ExcelCommon
    Public NotInheritable Class UdfPsmPreparation38
        Private Sub New()
        End Sub
        Public Shared Sub NormalizeFitOptions(fitOptions As PsmComprehensiveFitOptions)
            Dim o As PsmOptions = fitOptions.StandardOptions
            If fitOptions.RunMethod = PsmBackendRunMethod.OptimalPairMatching Then
                o.MatchingRatio = 1
                o.WithReplacement = False
            End If
            If Not PsmMethodCapabilities.UsesMatchingControls(fitOptions.RunMethod) Then
                o.MatchingRatio = 1
                o.WithReplacement = False
                o.DistanceMetric = PsmDistanceMetric.PropensityScore
                o.CaliperScale = PsmCaliperScale.None
                o.Caliper = Double.NaN
            End If
            If fitOptions.RunMethod = PsmBackendRunMethod.CoarsenedExactMatching AndAlso fitOptions.CoarseningSpec Is Nothing Then
                fitOptions.CoarseningSpec = New PsmCoarseningSpec With {
                    .Estimand = o.Estimand,
                    .NormalizeWeightsToSampleSize = o.NormalizeWeightsToSampleSize
                }
            End If
        End Sub

        Public Shared Sub ApplyOptionText(fitOptions As PsmComprehensiveFitOptions, optionText As String)
            If String.IsNullOrWhiteSpace(optionText) Then Return
            Dim parts As String() = optionText.Split({";"c, Convert.ToChar(10), Convert.ToChar(13)}, StringSplitOptions.RemoveEmptyEntries)
            For Each raw As String In parts
                Dim p As String = raw.Trim()
                If p.Length = 0 Then Continue For
                Dim eq As Integer = p.IndexOf("="c)
                Dim key As String = NormalizeOptionKey(If(eq >= 0, p.Substring(0, eq), p))
                Dim value As String = If(eq >= 0, p.Substring(eq + 1).Trim(), "true")
                ApplyOption(fitOptions, key, value)
            Next
        End Sub

        Public Shared Sub ApplyOption(fitOptions As PsmComprehensiveFitOptions, key As String, value As String)
            Dim o As PsmOptions = fitOptions.StandardOptions
            Select Case LegacyUdfText38.CaseToken(key, New String() {"method", "runmethod", "estimand", "scoremethod", "score", "distance", "distancemetric", "ratio", "matchingratio", "k", "replacement", "withreplacement", "caliperscale", "calipertype", "caliper", "order", "matchingorder", "support", "commonsupport", "seed", "randomseed", "intercept", "includeintercept", "standardize", "standardizecovariates", "maxiter", "logisticmaxiterations", "tol", "tolerance", "logistictolerance", "ridge", "ridgepenalty", "logisticridgepenalty", "smd", "smdthreshold", "balancesmdthreshold", "vrlower", "varianceratiolower", "vrupper", "varianceratioupper", "strata", "subclasses", "subclassificationstrata", "normalize", "normalizeweights", "normalizeweightstosamplesize", "trimlower", "trimpropensitylower", "trimupper", "trimpropensityupper", "dr", "aipw", "doublyrobust", "overlap", "overlapdiagnostics", "weightdiagnostics", "loveplot", "loveplotrows", "overlapbins", "overlapbincount", "extremeweight", "extremeweightcutoff", "cembins", "bins", "defaultcovariatebins", "psbins", "propensityscorebins", "includeps", "includepropensityscore"})
                Case "method", "runmethod"
                    fitOptions.RunMethod = ParseRunMethod(value, fitOptions.RunMethod)
                Case "estimand"
                    o.Estimand = ParseEstimand(value, o.Estimand)
                Case "scoremethod", "score"
                    o.ScoreMethod = ParseScoreMethod(value, o.ScoreMethod)
                Case "distance", "distancemetric"
                    o.DistanceMetric = ParseDistance(value, o.DistanceMetric)
                Case "ratio", "matchingratio", "k"
                    o.MatchingRatio = Math.Max(1, ParseInteger(value, o.MatchingRatio))
                Case "replacement", "withreplacement"
                    o.WithReplacement = ParseBoolean(value, o.WithReplacement)
                Case "caliperscale", "calipertype"
                    o.CaliperScale = ParseCaliperScale(value, o.CaliperScale)
                Case "caliper"
                    o.Caliper = ParseDouble(value, o.Caliper)
                Case "order", "matchingorder"
                    o.MatchingOrder = ParseOrder(value, o.MatchingOrder)
                Case "support", "commonsupport"
                    o.CommonSupport = ParseSupport(value, o.CommonSupport)
                Case "seed", "randomseed"
                    o.RandomSeed = ParseInteger(value, o.RandomSeed)
                Case "intercept", "includeintercept"
                    o.IncludeIntercept = ParseBoolean(value, o.IncludeIntercept)
                Case "standardize", "standardizecovariates"
                    o.StandardizeCovariates = ParseBoolean(value, o.StandardizeCovariates)
                Case "maxiter", "logisticmaxiterations"
                    o.LogisticMaxIterations = Math.Max(1, ParseInteger(value, o.LogisticMaxIterations))
                Case "tol", "tolerance", "logistictolerance"
                    o.LogisticTolerance = ParseDouble(value, o.LogisticTolerance)
                Case "ridge", "ridgepenalty", "logisticridgepenalty"
                    o.LogisticRidgePenalty = ParseDouble(value, o.LogisticRidgePenalty)
                Case "smd", "smdthreshold", "balancesmdthreshold"
                    o.BalanceSmdThreshold = ParseDouble(value, o.BalanceSmdThreshold)
                    fitOptions.LovePlotThreshold = o.BalanceSmdThreshold
                Case "vrlower", "varianceratiolower"
                    o.BalanceVarianceRatioLower = ParseDouble(value, o.BalanceVarianceRatioLower)
                Case "vrupper", "varianceratioupper"
                    o.BalanceVarianceRatioUpper = ParseDouble(value, o.BalanceVarianceRatioUpper)
                Case "strata", "subclasses", "subclassificationstrata"
                    o.SubclassificationStrata = Math.Max(2, ParseInteger(value, o.SubclassificationStrata))
                Case "normalize", "normalizeweights", "normalizeweightstosamplesize"
                    o.NormalizeWeightsToSampleSize = ParseBoolean(value, o.NormalizeWeightsToSampleSize)
                Case "trimlower", "trimpropensitylower"
                    o.TrimPropensityLower = ParseDouble(value, o.TrimPropensityLower)
                Case "trimupper", "trimpropensityupper"
                    o.TrimPropensityUpper = ParseDouble(value, o.TrimPropensityUpper)
                Case "dr", "aipw", "doublyrobust"
                    fitOptions.IncludeDoublyRobustEstimate = ParseBoolean(value, fitOptions.IncludeDoublyRobustEstimate)
                Case "overlap", "overlapdiagnostics"
                    fitOptions.IncludeOverlapDiagnostics = ParseBoolean(value, fitOptions.IncludeOverlapDiagnostics)
                Case "weightdiagnostics"
                    fitOptions.IncludeWeightDiagnostics = ParseBoolean(value, fitOptions.IncludeWeightDiagnostics)
                Case "loveplot", "loveplotrows"
                    fitOptions.IncludeLovePlotRows = ParseBoolean(value, fitOptions.IncludeLovePlotRows)
                Case "overlapbins", "overlapbincount"
                    fitOptions.OverlapBinCount = Math.Max(2, ParseInteger(value, fitOptions.OverlapBinCount))
                Case "extremeweight", "extremeweightcutoff"
                    fitOptions.ExtremeWeightCutoff = ParseDouble(value, fitOptions.ExtremeWeightCutoff)
                Case "cembins", "bins", "defaultcovariatebins"
                    EnsureCoarseningSpec(fitOptions)
                    fitOptions.CoarseningSpec.DefaultCovariateBins = Math.Max(2, ParseInteger(value, fitOptions.CoarseningSpec.DefaultCovariateBins))
                    fitOptions.CoarseningSpec.PropensityScoreBins = fitOptions.CoarseningSpec.DefaultCovariateBins
                Case "psbins", "propensityscorebins"
                    EnsureCoarseningSpec(fitOptions)
                    fitOptions.CoarseningSpec.PropensityScoreBins = Math.Max(2, ParseInteger(value, fitOptions.CoarseningSpec.PropensityScoreBins))
                Case "includeps", "includepropensityscore"
                    EnsureCoarseningSpec(fitOptions)
                    fitOptions.CoarseningSpec.IncludePropensityScore = ParseBoolean(value, fitOptions.CoarseningSpec.IncludePropensityScore)
            End Select
        End Sub

        Public Shared Sub EnsureCoarseningSpec(fitOptions As PsmComprehensiveFitOptions)
            If fitOptions.CoarseningSpec Is Nothing Then fitOptions.CoarseningSpec = New PsmCoarseningSpec()
            fitOptions.CoarseningSpec.Estimand = fitOptions.StandardOptions.Estimand
            fitOptions.CoarseningSpec.NormalizeWeightsToSampleSize = fitOptions.StandardOptions.NormalizeWeightsToSampleSize
        End Sub

        Public Shared Function ParseEstimand(value As String, defaultValue As PsmEstimand) As PsmEstimand
            If String.IsNullOrWhiteSpace(value) Then Return defaultValue
            Try
                Return CType([Enum].Parse(GetType(PsmEstimand), value.Trim(), True), PsmEstimand)
            Catch
                Return defaultValue
            End Try
        End Function
    End Class
End Namespace
