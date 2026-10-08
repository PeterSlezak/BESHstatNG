Option Explicit On
Option Strict On

Imports System

Namespace regression

    ''' <summary>
    ''' Diagnostics for the per-objective MMRM visit-pattern covariance cache.
    ''' </summary>
    ''' <remarks>
    ''' Counts are accumulated across profiled objective evaluations in one fit.
    ''' <c>PatternCount</c> is the maximum number of distinct observed visit patterns seen in
    ''' any single objective evaluation.
    ''' </remarks>
    Public Class MixedModelObjectivePatternCacheDiagnostics
        Public Property Enabled As Boolean = False
        Public Property ObjectiveEvaluations As Integer = 0
        Public Property PatternCount As Integer = 0
        Public Property Hits As Integer = 0
        Public Property Misses As Integer = 0
        Public Property InvalidBuilds As Integer = 0

        Public Function Clone() As MixedModelObjectivePatternCacheDiagnostics
            Return New MixedModelObjectivePatternCacheDiagnostics With {
                .Enabled = Me.Enabled,
                .ObjectiveEvaluations = Me.ObjectiveEvaluations,
                .PatternCount = Me.PatternCount,
                .Hits = Me.Hits,
                .Misses = Me.Misses,
                .InvalidBuilds = Me.InvalidBuilds
            }
        End Function
    End Class

    ''' <summary>
    ''' Wall-clock timing diagnostics collected during mixed-model fitting and KR post-processing.
    ''' </summary>
    ''' <remarks>
    ''' Timings are diagnostic only.  They are populated opportunistically so that expensive MMRM/LMM
    ''' phases can be measured without changing fitted estimates or inference calculations.
    ''' </remarks>
    Public Class MixedModelPerformanceDiagnostics
        Public Property TotalFitTimeMs As Double = Double.NaN
        Public Property StartingValuesTimeMs As Double = Double.NaN
        Public Property OptimizationTimeMs As Double = Double.NaN
        Public Property FinalEvaluationTimeMs As Double = Double.NaN
        Public Property KrWorkspaceBuildTimeMs As Double = Double.NaN
        Public Property KrDerivativeBlockTimeMs As Double = Double.NaN
        Public Property KrPqrMatrixTimeMs As Double = Double.NaN
        Public Property KrAdjustedVarBetaTimeMs As Double = Double.NaN
        Public Property ResultWrapTimeMs As Double = Double.NaN
        Public Property ObjectiveEvaluationCount As Integer = 0
        Public Property GradientEvaluationCount As Integer = 0
        Public Property NumericalGradientObjectiveEvaluationCount As Integer = 0
        Public Property LineSearchEvaluationCount As Integer = 0
        Public Property BfgsResetCount As Integer = 0
        Public Property GradientProviderName As String = String.Empty
        Public Property SelectedCovarianceGradientMode As MixedModelCovarianceGradientMode = MixedModelCovarianceGradientMode.Auto
        Public Property SelectedCovarianceOptimizerMode As MixedModelCovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml
        Public Property ActualCovarianceOptimizerName As String = String.Empty
        Public Property AverageInformationIterationCount As Integer = 0
        Public Property AverageInformationStepHalvingCount As Integer = 0
        Public Property AverageInformationRidgeAdjustmentCount As Integer = 0
        Public Property AverageInformationMatrixEvaluationCount As Integer = 0
        Public Property AverageInformationMatrixTimeMs As Double = Double.NaN
        Public Property ActualCovarianceGradientProviderName As String = String.Empty
        Public Property AnalyticGradientUsed As Boolean = False
        Public Property AnalyticGradientFallbackUsed As Boolean = False
        Public Property AnalyticGradientFailureMessage As String = String.Empty
        Public Property EstimatedNumericalGradientObjectiveEvaluationsAvoided As Long = 0
        Public Property AnalyticGradientValidationEvaluationCount As Integer = 0
        Public Property AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy As Double = Double.NaN
        Public Property AnalyticGradientValidationFailedParameterIndex As Integer = -1
        Public Property AnalyticGradientValidationMessage As String = String.Empty
        Public Property AnalyticGradientDerivativePatternCacheEnabled As Boolean = False
        Public Property AnalyticGradientDerivativePatternCount As Integer = 0
        Public Property AnalyticGradientDerivativePatternCacheHits As Long = 0
        Public Property AnalyticGradientDerivativePatternCacheMisses As Long = 0
        Public Property AnalyticGradientDerivativeMatricesBuilt As Long = 0
        Public Property AnalyticGradientTraceQuadraticContractionTimeMs As Double = Double.NaN
        Public Property ObjectivePatternCache As MixedModelObjectivePatternCacheDiagnostics = New MixedModelObjectivePatternCacheDiagnostics()

        Public Function Clone() As MixedModelPerformanceDiagnostics
            Dim clonedObjectivePatternCache As MixedModelObjectivePatternCacheDiagnostics = Nothing
            If Me.ObjectivePatternCache IsNot Nothing Then clonedObjectivePatternCache = Me.ObjectivePatternCache.Clone()

            Return New MixedModelPerformanceDiagnostics With {
                .TotalFitTimeMs = Me.TotalFitTimeMs,
                .StartingValuesTimeMs = Me.StartingValuesTimeMs,
                .OptimizationTimeMs = Me.OptimizationTimeMs,
                .FinalEvaluationTimeMs = Me.FinalEvaluationTimeMs,
                .KrWorkspaceBuildTimeMs = Me.KrWorkspaceBuildTimeMs,
                .KrDerivativeBlockTimeMs = Me.KrDerivativeBlockTimeMs,
                .KrPqrMatrixTimeMs = Me.KrPqrMatrixTimeMs,
                .KrAdjustedVarBetaTimeMs = Me.KrAdjustedVarBetaTimeMs,
                .ResultWrapTimeMs = Me.ResultWrapTimeMs,
                .ObjectiveEvaluationCount = Me.ObjectiveEvaluationCount,
                .GradientEvaluationCount = Me.GradientEvaluationCount,
                .NumericalGradientObjectiveEvaluationCount = Me.NumericalGradientObjectiveEvaluationCount,
                .LineSearchEvaluationCount = Me.LineSearchEvaluationCount,
                .BfgsResetCount = Me.BfgsResetCount,
                .GradientProviderName = Me.GradientProviderName,
                .SelectedCovarianceGradientMode = Me.SelectedCovarianceGradientMode,
                .SelectedCovarianceOptimizerMode = Me.SelectedCovarianceOptimizerMode,
                .ActualCovarianceOptimizerName = Me.ActualCovarianceOptimizerName,
                .AverageInformationIterationCount = Me.AverageInformationIterationCount,
                .AverageInformationStepHalvingCount = Me.AverageInformationStepHalvingCount,
                .AverageInformationRidgeAdjustmentCount = Me.AverageInformationRidgeAdjustmentCount,
                .AverageInformationMatrixEvaluationCount = Me.AverageInformationMatrixEvaluationCount,
                .AverageInformationMatrixTimeMs = Me.AverageInformationMatrixTimeMs,
                .ActualCovarianceGradientProviderName = Me.ActualCovarianceGradientProviderName,
                .AnalyticGradientUsed = Me.AnalyticGradientUsed,
                .AnalyticGradientFallbackUsed = Me.AnalyticGradientFallbackUsed,
                .AnalyticGradientFailureMessage = Me.AnalyticGradientFailureMessage,
                .EstimatedNumericalGradientObjectiveEvaluationsAvoided = Me.EstimatedNumericalGradientObjectiveEvaluationsAvoided,
                .AnalyticGradientValidationEvaluationCount = Me.AnalyticGradientValidationEvaluationCount,
                .AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy = Me.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy,
                .AnalyticGradientValidationFailedParameterIndex = Me.AnalyticGradientValidationFailedParameterIndex,
                .AnalyticGradientValidationMessage = Me.AnalyticGradientValidationMessage,
                .AnalyticGradientDerivativePatternCacheEnabled = Me.AnalyticGradientDerivativePatternCacheEnabled,
                .AnalyticGradientDerivativePatternCount = Me.AnalyticGradientDerivativePatternCount,
                .AnalyticGradientDerivativePatternCacheHits = Me.AnalyticGradientDerivativePatternCacheHits,
                .AnalyticGradientDerivativePatternCacheMisses = Me.AnalyticGradientDerivativePatternCacheMisses,
                .AnalyticGradientDerivativeMatricesBuilt = Me.AnalyticGradientDerivativeMatricesBuilt,
                .AnalyticGradientTraceQuadraticContractionTimeMs = Me.AnalyticGradientTraceQuadraticContractionTimeMs,
                .ObjectivePatternCache = clonedObjectivePatternCache
            }
        End Function
    End Class

End Namespace
