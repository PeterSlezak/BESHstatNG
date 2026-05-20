Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace CausalInference

    ''' <summary>
    ''' Central capability contract for GUI/UDF/back-end orchestration.  A front end
    ''' should expose only the estimands and controls that this class says are
    ''' supported, and the back end calls the same validator to prevent silent
    ''' fallbacks when callers construct options manually.
    ''' </summary>
    Public NotInheritable Class PsmMethodCapabilities
        Private Sub New()
        End Sub

        Public Shared Function SupportedEstimands(runMethod As PsmBackendRunMethod) As PsmEstimand()
            Select Case runMethod
                Case PsmBackendRunMethod.StandardNearestNeighbor, PsmBackendRunMethod.OptimalPairMatching
                    Return New PsmEstimand() {PsmEstimand.ATT, PsmEstimand.ATC}
                Case PsmBackendRunMethod.StandardSubclassification, PsmBackendRunMethod.WeightingOnly, PsmBackendRunMethod.CoarsenedExactMatching
                    Return New PsmEstimand() {PsmEstimand.ATT, PsmEstimand.ATC, PsmEstimand.ATE, PsmEstimand.ATO}
                Case Else
                    Throw New ArgumentOutOfRangeException("runMethod", "Unsupported PSM backend run method.")
            End Select
        End Function

        Public Shared Function SupportsEstimand(runMethod As PsmBackendRunMethod, estimand As PsmEstimand) As Boolean
            Return SupportedEstimands(runMethod).Contains(estimand)
        End Function

        Public Shared Function UsesMatchingControls(runMethod As PsmBackendRunMethod) As Boolean
            Return runMethod = PsmBackendRunMethod.StandardNearestNeighbor OrElse runMethod = PsmBackendRunMethod.OptimalPairMatching
        End Function

        Public Shared Function UsesNearestNeighborOptions(runMethod As PsmBackendRunMethod) As Boolean
            Return runMethod = PsmBackendRunMethod.StandardNearestNeighbor
        End Function

        Public Shared Function UsesOptimalPairOptions(runMethod As PsmBackendRunMethod) As Boolean
            Return runMethod = PsmBackendRunMethod.OptimalPairMatching
        End Function

        Public Shared Function UsesSubclassificationControls(runMethod As PsmBackendRunMethod) As Boolean
            Return runMethod = PsmBackendRunMethod.StandardSubclassification
        End Function

        Public Shared Function UsesCemControls(runMethod As PsmBackendRunMethod) As Boolean
            Return runMethod = PsmBackendRunMethod.CoarsenedExactMatching
        End Function

        Public Shared Function UsesDistanceMetric(runMethod As PsmBackendRunMethod) As Boolean
            Return UsesMatchingControls(runMethod)
        End Function

        Public Shared Function UsesCaliper(runMethod As PsmBackendRunMethod) As Boolean
            Return UsesMatchingControls(runMethod)
        End Function

        Public Shared Function SupportsDoublyRobust(runMethod As PsmBackendRunMethod, estimand As PsmEstimand) As Boolean
            'AIPW is now implemented for ATT, ATC, ATE and ATO.  It is a diagnostic
            'effect estimate and can accompany all complete PSM back-end runs.
            Return SupportsEstimand(runMethod, estimand)
        End Function

        Public Shared Sub ValidateFitOptions(fitOptions As PsmComprehensiveFitOptions)
            If fitOptions Is Nothing Then Throw New ArgumentNullException("fitOptions")
            If fitOptions.StandardOptions Is Nothing Then Throw New ArgumentException("Standard PSM options are required.")
            ValidateRunOptions(fitOptions.RunMethod, fitOptions.StandardOptions, fitOptions.CoarseningSpec)
            If fitOptions.IncludeDoublyRobustEstimate AndAlso Not SupportsDoublyRobust(fitOptions.RunMethod, fitOptions.StandardOptions.Estimand) Then
                Throw New ArgumentException("The selected run method and estimand do not support the doubly robust AIPW output.")
            End If
        End Sub

        Public Shared Sub ValidateRunOptions(runMethod As PsmBackendRunMethod, options As PsmOptions, Optional coarseningSpec As PsmCoarseningSpec = Nothing)
            If options Is Nothing Then Throw New ArgumentNullException("options")
            options.Validate()

            If Not SupportsEstimand(runMethod, options.Estimand) Then
                Throw New ArgumentException(runMethod.ToString() & " supports " & String.Join("/", SupportedEstimands(runMethod).Select(Function(e) e.ToString()).ToArray()) & " only. The selected estimand was " & options.Estimand.ToString() & ".")
            End If

            If runMethod = PsmBackendRunMethod.OptimalPairMatching Then
                If options.WithReplacement Then Throw New ArgumentException("Optimal pair matching is no-replacement by definition.")
                If options.MatchingRatio <> 1 Then Throw New ArgumentException("Optimal pair matching is 1:1 by definition. Set the matching ratio to 1.")
            End If

            If options.DistanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper AndAlso options.CaliperScale = PsmCaliperScale.None Then
                Throw New ArgumentException("Mahalanobis-within-propensity-caliper matching requires a propensity-score or logit-propensity caliper.")
            End If

            If runMethod = PsmBackendRunMethod.CoarsenedExactMatching AndAlso coarseningSpec IsNot Nothing Then
                If coarseningSpec.DefaultCovariateBins < 2 Then Throw New ArgumentOutOfRangeException("DefaultCovariateBins", "CEM covariate bins must be at least 2.")
                If coarseningSpec.PropensityScoreBins < 2 Then Throw New ArgumentOutOfRangeException("PropensityScoreBins", "CEM propensity-score bins must be at least 2.")
            End If
        End Sub
    End Class
End Namespace
