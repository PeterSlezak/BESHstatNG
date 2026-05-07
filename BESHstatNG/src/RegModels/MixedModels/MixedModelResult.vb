Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

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
    ''' Visit and visit-pair support diagnostics for MMRM repeated-measures covariance structures.
    ''' </summary>
    Public Class MixedModelVisitSupportDiagnostics
        Public Property Enabled As Boolean = False
        Public Property CovarianceStructureName As String = String.Empty
        Public Property VisitCounts As Dictionary(Of Double, Integer) = New Dictionary(Of Double, Integer)()
        Public Property VisitPairCounts As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        Public Property MinimumVisitCount As Integer = 0
        Public Property MinimumVisitPairCount As Integer = 0
        Public Property WeakPairThreshold As Integer = 0
        Public Property Warnings As List(Of String) = New List(Of String)()

        Public Function Clone() As MixedModelVisitSupportDiagnostics
            Return New MixedModelVisitSupportDiagnostics With {
                .Enabled = Me.Enabled,
                .CovarianceStructureName = Me.CovarianceStructureName,
                .VisitCounts = New Dictionary(Of Double, Integer)(Me.VisitCounts),
                .VisitPairCounts = New Dictionary(Of String, Integer)(Me.VisitPairCounts, StringComparer.Ordinal),
                .MinimumVisitCount = Me.MinimumVisitCount,
                .MinimumVisitPairCount = Me.MinimumVisitPairCount,
                .WeakPairThreshold = Me.WeakPairThreshold,
                .Warnings = New List(Of String)(Me.Warnings)
            }
        End Function
    End Class

    ''' <summary>
    ''' Diagnostics for structured optimizer restart attempts.
    ''' </summary>
    Public Class MixedModelRestartDiagnostics
        Public Property Enabled As Boolean = False
        Public Property StartAttemptCount As Integer = 0
        Public Property SuccessfulStartAttemptName As String = String.Empty
        Public Property FailedStartAttemptMessages As List(Of String) = New List(Of String)()

        Public Function Clone() As MixedModelRestartDiagnostics
            Return New MixedModelRestartDiagnostics With {
                .Enabled = Me.Enabled,
                .StartAttemptCount = Me.StartAttemptCount,
                .SuccessfulStartAttemptName = Me.SuccessfulStartAttemptName,
                .FailedStartAttemptMessages = New List(Of String)(Me.FailedStartAttemptMessages)
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

    ''' <summary>
    ''' Result container returned by <see cref="MixedModelEngine.Fit"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This first result class is intentionally lightweight and engine-facing.  It keeps the
    ''' numerical quantities that are needed for early validation of the Gaussian mixed-model
    ''' likelihood: fixed effects, fixed-effect covariance, covariance parameters, likelihood
    ''' values, convergence diagnostics, fitted values, residuals, and optional BLUPs.
    ''' </para>
    ''' <para>
    ''' A later UI/UDF layer can wrap this object into <c>ResultTable</c> instances in the same
    ''' style as the existing GLM/GEE/LM output.  Keeping the first engine result independent of
    ''' Excel and UI concepts makes it easier to test the likelihood against R <c>lme4</c>,
    ''' <c>nlme</c>, and MMRM reference fits.
    ''' </para>
    ''' </remarks>
    Public Class MixedModelResult

        ''' <summary>True when the optimizer reports convergence.</summary>
        Public Property Converged As Boolean = False

        ''' <summary>True when a fit was stopped by cooperative user cancellation.</summary>
        Public Property Cancelled As Boolean = False

        ''' <summary>True when a fit was interrupted and partial/current estimates were returned.</summary>
        Public Property Interrupted As Boolean = False

        Public Property ControlMaxIter As Integer = 0
        Public Property ControlEpsilon As Double = Double.NaN
        Public Property ControlStepTolerance As Double = Double.NaN
        Public Property ControlFunctionTolerance As Double = Double.NaN
        Public Property ControlUseBfgsCovarianceOptimization As Boolean = False
        Public Property ControlUseKrPqrDesignPatternCache As Boolean = True
        Public Property ControlUseKrPqrFastFactorization As Boolean = True
        Public Property ControlUseAnalyticGradientDerivativePatternCache As Boolean = True
        Public Property ControlCovarianceGradientMode As MixedModelCovarianceGradientMode = MixedModelCovarianceGradientMode.Auto
        Public Property ControlCovarianceOptimizerMode As MixedModelCovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml
        Public Property ControlAnalyticGradientValidationTolerance As Double = Double.NaN
        Public Property ControlFallbackToNumericalGradientOnAnalyticFailure As Boolean = True

        ''' <summary>Human-readable optimizer/status message.</summary>
        Public Property Message As String = String.Empty

        ''' <summary>User-facing warnings generated during fitting or post-estimation.</summary>
        Public Property UserWarnings As List(Of String) = New List(Of String)()

        Public Sub AddUserWarning(message As String)
            MixedModelNumericalDiagnostics.AddUniqueWarning(Me.UserWarnings, message)
        End Sub

        ''' <summary>Elapsed fit execution time in milliseconds. NaN when not recorded.</summary>
        Public Property ExecutionTimeMs As Double = Double.NaN

        ''' <summary>Detailed phase-level timing diagnostics for model fitting and KR calculations.</summary>
        Public Property PerformanceDiagnostics As MixedModelPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()

        ''' <summary>Visit and visit-pair support diagnostics for repeated-measures covariance structures.</summary>
        Public Property VisitSupportDiagnostics As MixedModelVisitSupportDiagnostics = New MixedModelVisitSupportDiagnostics()

        ''' <summary>Structured optimizer restart diagnostics.</summary>
        Public Property RestartDiagnostics As MixedModelRestartDiagnostics = New MixedModelRestartDiagnostics()

        ''' <summary>Optional UTC timestamp when fitting started.</summary>
        Public Property ExecutionStartedUtc As Nullable(Of DateTime) = Nothing

        ''' <summary>Optional UTC timestamp when fitting completed.</summary>
        Public Property ExecutionCompletedUtc As Nullable(Of DateTime) = Nothing

        ''' <summary>Likelihood criterion used for the fit.</summary>
        Public Property FitMethod As MixedModelFitMethod = MixedModelFitMethod.REML

        ''' <summary>Number of observations used by the fit.</summary>
        Public Property Nobs As Integer = 0

        ''' <summary>Number of subject/cluster blocks.</summary>
        Public Property NoSubjects As Integer = 0

        ''' <summary>Number of fixed-effect columns.</summary>
        Public Property P As Integer = 0

        ''' <summary>Number of random-effect columns.</summary>
        Public Property Q As Integer = 0

        ''' <summary>Fixed-effect names aligned with <see cref="Beta"/>.</summary>
        Public Property FixedEffectNames As String() = Array.Empty(Of String)()

        ''' <summary>Estimated fixed effects, profiled at the fitted covariance parameters.</summary>
        Public Property Beta As Double() = Array.Empty(Of Double)()

        ''' <summary>Estimated covariance matrix of the fixed effects, <c>(X'V^-1X)^-1</c>.</summary>
        Public Property VarBeta As Double(,) = Nothing

        ''' <summary>Standard errors of fixed effects.</summary>
        Public Property BetaSE As Double() = Array.Empty(Of Double)()

        ''' <summary>Large-sample Wald z statistics, <c>beta / SE</c>.</summary>
        Public Property BetaZ As Double() = Array.Empty(Of Double)()

        ''' <summary>Large-sample two-sided normal p-values.  Later versions can replace these with Satterthwaite/KR p-values.</summary>
        Public Property BetaP As Double() = Array.Empty(Of Double)()

        ''' <summary>Fixed-effect inference method used for the displayed coefficient tests.</summary>
        Public Property FixedInferenceMethod As MixedModelFixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal

        ''' <summary>Denominator degrees of freedom for fixed-effect tests.  NaN for large-sample normal inference.</summary>
        Public Property BetaDF As Double() = Array.Empty(Of Double)()

        ''' <summary>Displayed fixed-effect test statistic.  This is z for WaldNormal and t for df-based methods.</summary>
        Public Property BetaStatistic As Double() = Array.Empty(Of Double)()

        ''' <summary>Displayed fixed-effect test-statistic label, typically "z" or "t".</summary>
        Public Property BetaStatisticLabel As String = "z"

        ''' <summary>Displayed fixed-effect p-value label, typically "Pr(>|z|)" or "Pr(>|t|)".</summary>
        Public Property BetaPValueLabel As String = "Pr(>|z|)"

        ''' <summary>
        ''' Compatibility alias for older code.  Internally, Satterthwaite covariance-parameter
        ''' covariance is stored in <see cref="InferenceWorkspace"/>.
        ''' </summary>
        Public Property SatterthwaiteThetaCovariance As Double(,)
            Get
                If Me.InferenceWorkspace Is Nothing Then Return Nothing
                Return Me.InferenceWorkspace.ThetaCovariance
            End Get
            Set(value As Double(,))
                EnsureInferenceWorkspaceForCompatibility()
                Me.InferenceWorkspace.ThetaCovariance = value

                If value IsNot Nothing Then
                    Me.InferenceWorkspace.K = value.GetLength(0)
                End If
            End Set
        End Property

        ''' <summary>
        ''' Compatibility alias for older code.  Internally, finite-difference derivatives of
        ''' Var(beta) are stored in <see cref="InferenceWorkspace"/>.
        ''' </summary>
        Public Property SatterthwaiteVarBetaGradient As Double(,,)
            Get
                If Me.InferenceWorkspace Is Nothing Then Return Nothing
                Return Me.InferenceWorkspace.VarBetaGradient
            End Get
            Set(value As Double(,,))
                EnsureInferenceWorkspaceForCompatibility()
                Me.InferenceWorkspace.VarBetaGradient = value

                If value IsNot Nothing Then
                    Me.InferenceWorkspace.K = value.GetLength(0)
                    Me.InferenceWorkspace.P = value.GetLength(1)
                End If
            End Set
        End Property

        ''' <summary>
        ''' Internal universal Kenward-Roger derivative workspace, when requested.
        ''' This is not yet user-facing KR inference.
        ''' </summary>
        Public Property KenwardRogerWorkspace As regression.MixedModelKrWorkspace = Nothing
        Public Property KenwardRogerParameterScale As MixedModelKrParameterScale = MixedModelKrParameterScale.OptimizerInternal
        Public Property KenwardRogerCovarianceParameterNames As String() = Nothing

        Private pKenwardRogerAdjustedVarBeta As Double(,) = Nothing

        ''' <summary>
        ''' Linear Kenward-Roger adjusted fixed-effect covariance matrix, when the internal
        ''' KR workspace was successfully built.  This is stored for validation only until
        ''' full KR denominator-DF/scaling is implemented and validated.
        ''' </summary>
        Public Property KenwardRogerAdjustedVarBeta As Double(,)
            Get
                If Me.InferenceWorkspace IsNot Nothing AndAlso Me.InferenceWorkspace.AdjustedVarBeta IsNot Nothing Then
                    Return Me.InferenceWorkspace.AdjustedVarBeta
                End If

                Return pKenwardRogerAdjustedVarBeta
            End Get
            Set(value As Double(,))
                pKenwardRogerAdjustedVarBeta = value

                If value IsNot Nothing Then
                    EnsureInferenceWorkspaceForCompatibility()
                    Me.InferenceWorkspace.AdjustedVarBeta = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Diagnostic/status message from the internal KR backend.
        ''' </summary>
        Public Property KenwardRogerStatusMessage As String = String.Empty

        ''' <summary>
        ''' Ensures <see cref="InferenceWorkspace"/> exists and has the current basic
        ''' fixed-effect dimensions/state.
        ''' </summary>
        Private Sub EnsureInferenceWorkspaceForCompatibility()
            If Me.InferenceWorkspace Is Nothing Then
                Me.InferenceWorkspace = New regression.MixedModelInferenceWorkspace()
            End If

            If Me.P > 0 Then
                Me.InferenceWorkspace.P = Me.P
            End If

            If Me.VarBeta IsNot Nothing Then
                Me.InferenceWorkspace.VarBeta = Me.VarBeta
            End If
        End Sub

        ''' <summary>
        ''' Universal fixed-effect inference workspace for Satterthwaite/Kenward-Roger style
        ''' linear-combination inference.  This should gradually replace the older direct
        ''' SatterthwaiteThetaCovariance / SatterthwaiteVarBetaGradient fields.
        ''' </summary>
        Public Property InferenceWorkspace As regression.MixedModelInferenceWorkspace = Nothing

        ''' <summary>G-side covariance parameter names.</summary>
        Public Property ThetaGNames As String() = Array.Empty(Of String)()

        ''' <summary>G-side covariance parameters on the internal optimizer scale.</summary>
        Public Property ThetaG As Double() = Array.Empty(Of Double)()

        ''' <summary>R-side covariance parameter names.</summary>
        Public Property ThetaRNames As String() = Array.Empty(Of String)()

        ''' <summary>R-side covariance parameters on the internal optimizer scale.</summary>
        Public Property ThetaR As Double() = Array.Empty(Of Double)()

        ''' <summary>
        ''' Display name of the fitted R-side residual covariance structure.
        ''' </summary>
        Public Property ResidualCovarianceStructureName As String = String.Empty

        ''' <summary>
        ''' Display name of the fitted G-side random-effects covariance structure.
        ''' For MMRM this is typically "None".
        ''' </summary>
        Public Property RandomCovarianceStructureName As String = String.Empty

        ''' <summary>
        ''' Labels used for the estimated random-effects covariance/correlation matrix.
        ''' For LMMs these usually correspond to random-effect columns such as intercept and slope.
        ''' </summary>
        Public Property RandomCovarianceLabels As String() = Array.Empty(Of String)()

        ''' <summary>
        ''' Estimated G-side random-effects covariance matrix on the user/statistical scale.
        ''' Empty for MMRM/no-random-effects fits.
        ''' </summary>
        Public Property RandomCovarianceUserScale As Double(,) = Nothing

        ''' <summary>
        ''' Estimated G-side random-effects correlation matrix derived from <see cref="RandomCovarianceUserScale"/>.
        ''' Empty for MMRM/no-random-effects fits.
        ''' </summary>
        Public Property RandomCorrelationUserScale As Double(,) = Nothing

        ''' <summary>
        ''' Labels used for the estimated residual covariance/correlation matrix.
        ''' For MMRM these usually correspond to visit/time values.
        ''' </summary>
        Public Property ResidualCovarianceVisitLabels As String() = Array.Empty(Of String)()

        ''' <summary>
        ''' Estimated R-side covariance matrix on the user/statistical scale.
        ''' For MMRM/no-random-effects fits this is the marginal within-subject covariance matrix.
        ''' </summary>
        Public Property ResidualCovarianceUserScale As Double(,) = Nothing

        ''' <summary>
        ''' Estimated R-side correlation matrix derived from <see cref="ResidualCovarianceUserScale"/>.
        ''' </summary>
        Public Property ResidualCorrelationUserScale As Double(,) = Nothing

        ''' <summary>Combined internal covariance parameter vector used by the optimizer.</summary>
        Public Property Theta As Double() = Array.Empty(Of Double)()

        ''' <summary>Optimized profiled objective.  This is -2 log-likelihood or -2 restricted log-likelihood including constants.</summary>
        Public Property Objective As Double = Double.NaN

        ''' <summary>Log-likelihood or restricted log-likelihood corresponding to <see cref="Objective"/>.</summary>
        Public Property LogLik As Double = Double.NaN

        ''' <summary>Akaike information criterion using the fitted objective.  For REML this is mainly diagnostic.</summary>
        Public Property AIC As Double = Double.NaN

        ''' <summary>Bayesian information criterion using the fitted objective.  For REML this is mainly diagnostic.</summary>
        Public Property BIC As Double = Double.NaN

        ''' <summary>REML criterion value.  Populated when <see cref="FitMethod"/> is REML.</summary>
        Public Property REMLCriterion As Double = Double.NaN

        ''' <summary>Residual quadratic form at the fitted covariance parameters.</summary>
        Public Property QForm As Double = Double.NaN

        ''' <summary>Sum of log determinants of subject block covariance matrices.</summary>
        Public Property LogDetV As Double = Double.NaN

        ''' <summary>Log determinant of <c>X'V^-1X</c>.</summary>
        Public Property LogDetXtVinvX As Double = Double.NaN

        ''' <summary>Diagnostic residual scale estimate, <c>Q / df</c>.  Covariance parameters remain the primary variance estimates.</summary>
        Public Property Sigma2Profile As Double = Double.NaN

        ''' <summary>Optimizer iterations completed.</summary>
        Public Property Iterations As Integer = 0

        ''' <summary>Projected-gradient norm reported by the optimizer.</summary>
        Public Property GradNorm As Double = Double.NaN

        ''' <summary>Optimizer trace table, if requested by <see cref="MixedModelControl.Trace"/>.</summary>
        Public Property OptimizerTrace As Double(,) = Nothing

        ''' <summary>Marginal fitted values <c>X beta</c> in the original row order when possible.</summary>
        Public Property FittedMarginal As Double() = Array.Empty(Of Double)()

        ''' <summary>Raw marginal residuals <c>y - X beta</c> in the original row order when possible.</summary>
        Public Property ResidualRaw As Double() = Array.Empty(Of Double)()

        ''' <summary>Dictionary of subject-specific BLUP vectors.  Empty for MMRM/no-random-effects fits.</summary>
        Public Property RandomEffects As Dictionary(Of String, Double()) = New Dictionary(Of String, Double())()

        ''' <summary>Full diagnostic trace accumulated by the engine and optimizer.</summary>
        Public Property strTrace As String = String.Empty

        Public Property AdditionalResultTables As List(Of ResultTable) = New List(Of ResultTable)()

        ''' <summary>
        ''' Releases large intermediate post-estimation workspaces that are no longer needed after
        ''' all requested mixed-model result tables have been materialized.
        ''' </summary>
        ''' <param name="clearAdditionalResultTables">
        ''' If True, clears presentation-only additional tables already copied to an output workbook.
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' Kenward-Roger workspaces can contain block-level derivative tensors and P/Q/R matrices that
        ''' are intentionally retained while post-estimation tables are being generated.  GUI callers that
        ''' have already written the workbook output can call this method to make those large arrays
        ''' immediately eligible for garbage collection.
        ''' </para>
        ''' <para>
        ''' Core fitted quantities such as beta, Var(beta), covariance estimates, fitted values,
        ''' residuals, diagnostics, warnings, and the KR adjusted Var(beta) summary are preserved.
        ''' Detailed KR/Satterthwaite derivative workspaces are released and therefore should not be
        ''' required after this method is called.
        ''' </para>
        ''' </remarks>
        Public Sub ReleaseLargePostEstimationWorkspaces(Optional clearAdditionalResultTables As Boolean = True)
            Try
                If pKenwardRogerAdjustedVarBeta Is Nothing AndAlso
                   Me.InferenceWorkspace IsNot Nothing AndAlso
                   Me.InferenceWorkspace.AdjustedVarBeta IsNot Nothing Then
                    pKenwardRogerAdjustedVarBeta = Me.InferenceWorkspace.AdjustedVarBeta
                End If

                If Me.KenwardRogerWorkspace IsNot Nothing Then
                    Me.KenwardRogerWorkspace.ProgressReporter = Nothing
                    Me.KenwardRogerWorkspace.CancellationRequested = Nothing

                    If Me.KenwardRogerWorkspace.Blocks IsNot Nothing Then
                        For Each block As regression.MixedModelKrBlock In Me.KenwardRogerWorkspace.Blocks
                            If block Is Nothing Then Continue For
                            block.X = Nothing
                            block.VInv = Nothing
                            block.DV = Nothing
                            block.D2V = Nothing
                        Next

                        Me.KenwardRogerWorkspace.Blocks.Clear()
                    End If

                    Me.KenwardRogerWorkspace.Pmats = Nothing
                    Me.KenwardRogerWorkspace.Qmats = Nothing
                    Me.KenwardRogerWorkspace.Rmats = Nothing
                    Me.KenwardRogerWorkspace.VarBeta = Nothing
                    Me.KenwardRogerWorkspace.ThetaCovariance = Nothing
                    Me.KenwardRogerWorkspace.Theta = Nothing

                    If Me.KenwardRogerWorkspace.DfScalingCache IsNot Nothing Then
                        Me.KenwardRogerWorkspace.DfScalingCache.Clear()
                    End If

                    Me.KenwardRogerWorkspace.AdjustedVarBeta = Nothing
                    Me.KenwardRogerWorkspace = Nothing
                End If

                If Me.InferenceWorkspace IsNot Nothing Then
                    Me.InferenceWorkspace.KR_P = Nothing
                    Me.InferenceWorkspace.KR_Q = Nothing
                    Me.InferenceWorkspace.KR_R = Nothing
                    Me.InferenceWorkspace.VarBetaGradient = Nothing
                    Me.InferenceWorkspace.ThetaCovariance = Nothing
                    Me.InferenceWorkspace.AdjustedVarBeta = Nothing
                    Me.InferenceWorkspace = Nothing
                End If

                Me.OptimizerTrace = Nothing

                If Me.RandomEffects IsNot Nothing Then Me.RandomEffects.Clear()

                If clearAdditionalResultTables AndAlso Me.AdditionalResultTables IsNot Nothing Then
                    Me.AdditionalResultTables.Clear()
                End If
            Catch
                ' Memory cleanup is opportunistic and must not affect already-created results.
            End Try
        End Sub

#Region "ResultTable wrapping"


        ''' <summary>
        ''' Wraps the mixed-model fit into presentation-ready <see cref="ResultTable"/> objects.
        ''' </summary>
        ''' <param name="alpha">Two-sided confidence level alpha.  For example, 0.05 gives 95% Wald intervals.</param>
        ''' <param name="includeOptimizerTrace">If True and available, includes the numeric optimizer trace table.</param>
        ''' <returns>
        ''' A list of <see cref="ResultTable"/> objects suitable for <see cref="ProcessListofResultTables.writeToSheet"/>.
        ''' </returns>
        Public Function wrapResults(Optional alpha As Double = 0.05, Optional includeOptimizerTrace As Boolean = False,
                            Optional includeKenwardRogerTermTests As Boolean = False,
                            Optional includeDiagnostics As Boolean = True) As List(Of ResultTable)
            Dim wrapStopwatch As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
            Dim out As New List(Of ResultTable)

            out.Add(BuildFixedEffectsTable(alpha))

            If includeKenwardRogerTermTests OrElse Me.FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger Then
                AppendKenwardRogerTermLevelFTestsTable(out, alpha)
            End If

            AppendAdditionalResultTables(out)
            out.Add(BuildCovarianceParameterTable())

            If Me.RandomCovarianceUserScale IsNot Nothing Then out.Add(BuildRandomCovarianceMatrixTable())
            If Me.RandomCorrelationUserScale IsNot Nothing Then out.Add(BuildRandomCorrelationMatrixTable())
            If Me.ResidualCovarianceUserScale IsNot Nothing Then out.Add(BuildResidualCovarianceMatrixTable())
            If Me.ResidualCorrelationUserScale IsNot Nothing Then out.Add(BuildResidualCorrelationMatrixTable())
            If Me.RandomEffects IsNot Nothing AndAlso Me.RandomEffects.Count > 0 Then out.Add(BuildRandomEffectsTable())

            out.Add(BuildFitStatisticsTable())
            If includeDiagnostics Then
                out.Add(BuildPerformanceDiagnosticsTable())
                If Me.VisitSupportDiagnostics IsNot Nothing AndAlso Me.VisitSupportDiagnostics.Enabled Then
                    out.Add(BuildVisitSupportDiagnosticsTable())
                End If
                If Me.RestartDiagnostics IsNot Nothing AndAlso Me.RestartDiagnostics.Enabled Then
                    out.Add(BuildRestartDiagnosticsTable())
                End If
                If Me.KenwardRogerWorkspace IsNot Nothing AndAlso Me.KenwardRogerWorkspace.FiniteDifferenceDiagnostics IsNot Nothing Then
                    out.Add(BuildKrFiniteDifferenceDiagnosticsTable())
                End If
            End If
            out.Add(BuildConvergenceTable())

            If includeOptimizerTrace AndAlso Me.OptimizerTrace IsNot Nothing Then out.Add(BuildOptimizerTraceTable())

            wrapStopwatch.Stop()
            If Me.PerformanceDiagnostics Is Nothing Then Me.PerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            Me.PerformanceDiagnostics.ResultWrapTimeMs = wrapStopwatch.Elapsed.TotalMilliseconds

            Return out
        End Function

        ''' <summary>
        ''' Attempts to compute a row-specific Satterthwaite denominator DF for the linear
        ''' estimate L*beta.
        ''' </summary>
        ''' <param name="l">
        ''' Fixed-effect contrast/LS-mean row L, aligned with Beta and VarBeta.
        ''' </param>
        ''' <param name="df">
        ''' Output denominator degrees of freedom.
        ''' </param>
        ''' <returns>
        ''' True if a finite positive Satterthwaite DF could be computed.
        ''' </returns>
        Public Function TrySatterthwaiteDFForLinearCombination(l() As Double,
                                                       ByRef df As Double) As Boolean
            df = Double.NaN

            If l Is Nothing OrElse l.Length = 0 Then Return False

            If Me.InferenceWorkspace Is Nothing Then Return False

            If Me.InferenceWorkspace.P <= 0 Then
                Me.InferenceWorkspace.P = If(Me.P > 0, Me.P, l.Length)
            End If

            If Me.InferenceWorkspace.VarBeta Is Nothing AndAlso Me.VarBeta IsNot Nothing Then
                Me.InferenceWorkspace.VarBeta = Me.VarBeta
            End If

            Return regression.MixedModelInferenceMath.TrySatterthwaiteDF(l, Me.InferenceWorkspace, df)
        End Function

        ''' <summary>
        ''' Appends the Kenward-Roger term-level multi-df F-test table.
        ''' </summary>
        ''' <remarks>
        ''' This table is part of the Kenward-Roger result path. It is appended
        ''' automatically when the fitted result uses Kenward-Roger fixed-effect
        ''' inference, and it can still be requested explicitly for backend validation
        ''' by setting <c>includeKenwardRogerTermTests:=True</c>.
        ''' </remarks>
        Private Sub AppendKenwardRogerTermLevelFTestsTable(out As List(Of ResultTable),
                                                       alpha As Double)
            If out Is Nothing Then Exit Sub
            If Me.FixedInferenceMethod <> MixedModelFixedInferenceMethod.KenwardRoger Then Exit Sub
            If Me.FixedEffectNames Is Nothing OrElse Me.FixedEffectNames.Length = 0 Then Exit Sub
            If Me.KenwardRogerWorkspace Is Nothing Then Exit Sub
            If regression.MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(Me) Is Nothing Then Exit Sub

            Try
                Dim t As ResultTable = regression.MixedModelHypothesisBuilder.BuildTermMultiDfInferenceTable(
                modelResult:=Me,
                includeIntercept:=False,
                alpha:=alpha,
                title:="Kenward-Roger term-level F tests")

                If t IsNot Nothing Then out.Add(t)

            Catch ex As Exception
                ' This table is optional/internal validation output.  Do not allow it to
                ' block the standard model result tables.
                Dim t As New ResultTable()
                t.AddTitle("Kenward-Roger term-level F tests")
                Dim body(0, 0) As Object
                body(0, 0) = "Not available"
                t.SetBody(body)
                t.AddHeaderTopRow({"Status"})
                t.AddFootnote("KR term-level F-test table could not be created: " & ex.Message)
                out.Add(t)
            End Try
        End Sub

        ''' <summary>
        ''' Builds the fixed-effects coefficient table.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This first mixed-model output layer uses large-sample Wald normal inference:
        ''' <c>z = beta / SE(beta)</c>, with two-sided normal p-values.  The p-value column is
        ''' registered as body column 4 so that <see cref="WriteResults"/> highlights the p-values,
        ''' not the z-statistics.
        ''' </para>
        ''' <para>
        ''' SAS PROC MIXED reports t statistics for fixed effects because it attaches denominator
        ''' degrees of freedom to the Wald statistic.  Do not rename this column to t until a
        ''' denominator-degree-of-freedom method is implemented and the p-values/CIs are computed
        ''' from the Student-t distribution.
        ''' </para>
        ''' </remarks>
        Private Function BuildFixedEffectsTable(alpha As Double) As ResultTable
            Dim t As New ResultTable
            t.AddTitle("Fixed effects")

            Dim p As Integer = If(Me.Beta Is Nothing, 0, Me.Beta.Length)
            Dim alphaUse As Double = alpha
            If Double.IsNaN(alphaUse) OrElse Double.IsInfinity(alphaUse) OrElse alphaUse <= 0.0 OrElse alphaUse >= 1.0 Then alphaUse = 0.05

            Dim levelText As String = Format((1.0 - alphaUse) * 100.0, "0.###") & "% CI"
            Dim hasDF As Boolean = HasFiniteDFVector(Me.BetaDF, p)

            If hasDF Then
                Dim body(Math.Max(0, p - 1), 6) As Object
                Dim rowNames(Math.Max(0, p - 1)) As String

                For j As Integer = 0 To p - 1
                    rowNames(j) = SafeName(Me.FixedEffectNames, j, "b" & CStr(j))

                    Dim est As Double = SafeVectorValue(Me.Beta, j)
                    Dim se As Double = SafeVectorValue(Me.BetaSE, j)
                    Dim df As Double = SafeVectorValue(Me.BetaDF, j)
                    Dim stat As Double = SafeVectorValue(Me.BetaStatistic, j)
                    Dim pv As Double = SafeVectorValue(Me.BetaP, j)
                    Dim lo As Double = Double.NaN
                    Dim hi As Double = Double.NaN

                    If AppInfrastructure.NumericGuards.IsFinite(est) AndAlso AppInfrastructure.NumericGuards.IsFinite(se) AndAlso AppInfrastructure.NumericGuards.IsFinite(df) AndAlso df > 0.0 Then
                        Dim crit As Double = Global.BESHStatNG.distributions.Distributions.T_Inv_2T(alphaUse, df)
                        lo = est - crit * se
                        hi = est + crit * se
                    End If

                    body(j, 0) = est
                    body(j, 1) = se
                    body(j, 2) = df
                    body(j, 3) = stat
                    body(j, 4) = pv
                    body(j, 5) = lo
                    body(j, 6) = hi
                Next

                t.SetBody(body)
                t.AddHeaderTopRow({"Estimate", "Std. Error", "DF", Me.BetaStatisticLabel, Me.BetaPValueLabel, "Lower " & levelText, "Upper " & levelText})
                If p > 0 Then t.AddHeaderLeftRow(rowNames)
                Select Case Me.FixedInferenceMethod
                    Case MixedModelFixedInferenceMethod.Satterthwaite
                        t.AddFootnote("Inference uses a first-order Satterthwaite denominator-df approximation based on finite-difference derivatives of Var(beta) with respect to covariance parameters. Kenward-Roger adjustment is not applied.")

                    Case MixedModelFixedInferenceMethod.BetweenWithin
                        t.AddFootnote("Inference uses R mmrm-style Between-within denominator degrees of freedom. The intercept and within-subject effects use within-subject df; subject-constant effects use between-subject df. Satterthwaite and Kenward-Roger adjustments are not applied.")

                    Case MixedModelFixedInferenceMethod.ResidualDF
                        t.AddFootnote("Inference uses residual denominator degrees of freedom n - p. Satterthwaite and Kenward-Roger adjustments are not applied.")

                    Case MixedModelFixedInferenceMethod.KenwardRoger
                        t.AddFootnote("Inference uses Kenward-Roger adjusted coefficient covariance and denominator degrees of freedom for fixed-effect coefficient tests.")

                    Case Else
                        t.AddFootnote("Inference uses " & Me.FixedInferenceMethod.ToString() & " denominator degrees of freedom.")
                End Select

                If Me.FitMethod = MixedModelFitMethod.REML Then
                    t.AddFootnote("For REML fits, compare models with different fixed effects using ML rather than REML.")
                End If
                ' Body column 5 is p-value when DF column is included.
                t.AddPvalueToFormat(5)
                Return t
            Else
                Dim body(Math.Max(0, p - 1), 5) As Object
                Dim rowNames(Math.Max(0, p - 1)) As String
                Dim zCrit As Double = Global.BESHStatNG.distributions.Distributions.NormSInv(1.0 - alphaUse / 2.0)

                For j As Integer = 0 To p - 1
                    rowNames(j) = SafeName(Me.FixedEffectNames, j, "b" & CStr(j))

                    Dim est As Double = SafeVectorValue(Me.Beta, j)
                    Dim se As Double = SafeVectorValue(Me.BetaSE, j)
                    Dim stat As Double = SafeVectorValue(Me.BetaStatistic, j)
                    If Not AppInfrastructure.NumericGuards.IsFinite(stat) Then stat = SafeVectorValue(Me.BetaZ, j)
                    Dim pv As Double = SafeVectorValue(Me.BetaP, j)
                    Dim lo As Double = Double.NaN
                    Dim hi As Double = Double.NaN

                    If AppInfrastructure.NumericGuards.IsFinite(est) AndAlso AppInfrastructure.NumericGuards.IsFinite(se) Then
                        lo = est - zCrit * se
                        hi = est + zCrit * se
                    End If

                    body(j, 0) = est
                    body(j, 1) = se
                    body(j, 2) = stat
                    body(j, 3) = pv
                    body(j, 4) = lo
                    body(j, 5) = hi
                Next

                t.SetBody(body)
                t.AddHeaderTopRow({"Estimate", "Std. Error", Me.BetaStatisticLabel, Me.BetaPValueLabel, "Lower " & levelText, "Upper " & levelText})
                If p > 0 Then t.AddHeaderLeftRow(rowNames)
                t.AddFootnote("Inference uses large-sample Wald normal approximation. Satterthwaite/Kenward-Roger denominator df are not applied.")
                If Me.FitMethod = MixedModelFitMethod.REML Then
                    t.AddFootnote("For REML fits, compare models with different fixed effects using ML rather than REML.")
                End If
                ' Body column 4 is p-value when no DF column is included.
                t.AddPvalueToFormat(4)
                Return t
            End If
        End Function

        ''' <summary>
        ''' Builds one covariance-parameter table containing G-side and R-side internal-scale estimates.
        ''' </summary>
        Private Function BuildCovarianceParameterTable() As ResultTable
            Dim t As New ResultTable
            t.AddTitle("Covariance parameters")

            Dim gN As Integer = If(Me.ThetaG Is Nothing, 0, Me.ThetaG.Length)
            Dim rN As Integer = If(Me.ThetaR Is Nothing, 0, Me.ThetaR.Length)
            Dim n As Integer = gN + rN

            If n = 0 Then
                Dim body0(0, 2) As Object
                body0(0, 0) = "None"
                body0(0, 1) = ""
                body0(0, 2) = ""
                t.SetBody(body0)
                t.AddHeaderTopRow({"Side", "Parameter", "Internal estimate"})
                t.AddFootnote("No covariance parameters were optimized for this model.")
                Return t
            End If

            Dim body(n - 1, 2) As Object
            Dim row As Integer = 0

            For j As Integer = 0 To gN - 1
                body(row, 0) = "G"
                body(row, 1) = SafeName(Me.ThetaGNames, j, "G" & CStr(j + 1))
                body(row, 2) = Me.ThetaG(j)
                row += 1
            Next

            For j As Integer = 0 To rN - 1
                body(row, 0) = "R"
                body(row, 1) = SafeName(Me.ThetaRNames, j, "R" & CStr(j + 1))
                body(row, 2) = Me.ThetaR(j)
                row += 1
            Next

            t.SetBody(body)
            t.AddHeaderTopRow({"Side", "Parameter", "Internal estimate"})
            t.AddFootnote("Covariance parameters in this table are shown on the internal optimizer scale. User-scale G-side and R-side covariance/correlation matrices are reported in following tables when available.")
            Return t
        End Function

        ''' <summary>
        ''' Builds model-level fit statistics.
        ''' </summary>
        Private Function BuildFitStatisticsTable() As ResultTable
            Dim t As New ResultTable
            t.AddTitle("Fit statistics")

            Dim metrics As New List(Of String)
            Dim values As New List(Of Object)

            metrics.Add("Fit method") : values.Add(Me.FitMethod.ToString())
            metrics.Add("N observations") : values.Add(Me.Nobs)
            metrics.Add("Subjects") : values.Add(Me.NoSubjects)
            If Not Double.IsNaN(Me.ExecutionTimeMs) AndAlso Not Double.IsInfinity(Me.ExecutionTimeMs) AndAlso Me.ExecutionTimeMs >= 0.0 Then
                metrics.Add("Execution time") : values.Add(FormatExecutionTime(Me.ExecutionTimeMs))
            End If
            metrics.Add("Fixed-effect parameters") : values.Add(Me.P)
            metrics.Add("Random-effect columns") : values.Add(Me.Q)
            metrics.Add("Objective") : values.Add(Me.Objective)
            metrics.Add("Log-likelihood") : values.Add(Me.LogLik)
            metrics.Add("AIC") : values.Add(Me.AIC)
            metrics.Add("BIC") : values.Add(Me.BIC)
            If Me.FitMethod = MixedModelFitMethod.REML Then
                metrics.Add("REML criterion") : values.Add(Me.REMLCriterion)
            End If
            metrics.Add("Q form") : values.Add(Me.QForm)
            metrics.Add("log|V|") : values.Add(Me.LogDetV)
            metrics.Add("log|X'V^-1X|") : values.Add(Me.LogDetXtVinvX)
            metrics.Add("Profile scale Q/df") : values.Add(Me.Sigma2Profile)
            If Me.KenwardRogerWorkspace IsNot Nothing Then
                metrics.Add("KR derivative workspace") : values.Add("Available")
                If Me.KenwardRogerAdjustedVarBeta IsNot Nothing Then
                    metrics.Add("KR linear adjusted Var(beta)") : values.Add("Available")
                End If
                If Not String.IsNullOrWhiteSpace(Me.KenwardRogerStatusMessage) Then
                    metrics.Add("KR backend status") : values.Add(Me.KenwardRogerStatusMessage)
                End If
            End If
            If Me.KenwardRogerWorkspace IsNot Nothing AndAlso Me.KenwardRogerWorkspace.Rmats IsNot Nothing Then
                metrics.Add("KR second derivatives") : values.Add("Available")
            End If
            If Me.KenwardRogerWorkspace IsNot Nothing Then
                metrics.Add("KR parameter scale") : values.Add(Me.KenwardRogerParameterScale.ToString())
            End If

            Dim body(metrics.Count - 1, 0) As Object
            For i As Integer = 0 To metrics.Count - 1
                body(i, 0) = values(i)
            Next

            t.SetBody(body)
            t.AddHeaderTopRow({"Value"})
            t.AddHeaderLeftRow(metrics.ToArray())
            If Me.FitMethod = MixedModelFitMethod.REML Then
                t.AddFootnote("REML information criteria are diagnostic and should not be used to compare models with different fixed-effects specifications.")
            End If
            Return t
        End Function

        ''' <summary>
        ''' Builds phase-level timing diagnostics for fit and Kenward-Roger calculations.
        ''' </summary>
        Private Function BuildPerformanceDiagnosticsTable() As ResultTable
            Dim t As New ResultTable
            t.AddTitle("Performance diagnostics")

            Dim d As MixedModelPerformanceDiagnostics = Me.PerformanceDiagnostics
            If d Is Nothing Then d = New MixedModelPerformanceDiagnostics()

            Dim metrics As New List(Of String)
            Dim values As New List(Of Object)

            metrics.Add("Total fit time ms") : values.Add(SafeTimingValue(d.TotalFitTimeMs))
            metrics.Add("Starting values time ms") : values.Add(SafeTimingValue(d.StartingValuesTimeMs))
            metrics.Add("Optimization time ms") : values.Add(SafeTimingValue(d.OptimizationTimeMs))
            metrics.Add("Final evaluation time ms") : values.Add(SafeTimingValue(d.FinalEvaluationTimeMs))
            metrics.Add("KR workspace build time ms") : values.Add(SafeTimingValue(d.KrWorkspaceBuildTimeMs))
            metrics.Add("KR derivative blocks time ms") : values.Add(SafeTimingValue(d.KrDerivativeBlockTimeMs))
            metrics.Add("KR P/Q/R matrices time ms") : values.Add(SafeTimingValue(d.KrPqrMatrixTimeMs))
            metrics.Add("KR adjusted Var(beta) time ms") : values.Add(SafeTimingValue(d.KrAdjustedVarBetaTimeMs))
            metrics.Add("Result wrapping time ms") : values.Add(SafeTimingValue(d.ResultWrapTimeMs))
            metrics.Add("Selected covariance optimizer mode") : values.Add(d.SelectedCovarianceOptimizerMode.ToString())
            metrics.Add("Actual covariance optimizer") : values.Add(If(String.IsNullOrWhiteSpace(d.ActualCovarianceOptimizerName), If(d.GradientProviderName, String.Empty), d.ActualCovarianceOptimizerName))
            metrics.Add("Selected covariance gradient mode") : values.Add(d.SelectedCovarianceGradientMode.ToString())
            If d.SelectedCovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml OrElse d.AverageInformationMatrixEvaluationCount > 0 Then
                metrics.Add("Average Information iterations") : values.Add(d.AverageInformationIterationCount)
                metrics.Add("Average Information step halvings") : values.Add(d.AverageInformationStepHalvingCount)
                metrics.Add("Average Information ridge adjustments") : values.Add(d.AverageInformationRidgeAdjustmentCount)
                metrics.Add("Average Information matrix evaluations") : values.Add(d.AverageInformationMatrixEvaluationCount)
                metrics.Add("Average Information matrix time ms") : values.Add(SafeTimingValue(d.AverageInformationMatrixTimeMs))
            End If
            metrics.Add("Optimizer gradient provider") : values.Add(If(d.GradientProviderName, String.Empty))
            metrics.Add("Actual covariance gradient provider") : values.Add(If(String.IsNullOrWhiteSpace(d.ActualCovarianceGradientProviderName), If(d.GradientProviderName, String.Empty), d.ActualCovarianceGradientProviderName))
            metrics.Add("Analytic gradient used") : values.Add(d.AnalyticGradientUsed)
            metrics.Add("Analytic gradient fallback used") : values.Add(d.AnalyticGradientFallbackUsed)
            If Not String.IsNullOrWhiteSpace(d.AnalyticGradientFailureMessage) Then
                metrics.Add("Analytic gradient failure/fallback message") : values.Add(d.AnalyticGradientFailureMessage)
            End If
            metrics.Add("Analytic gradient validation evaluations") : values.Add(d.AnalyticGradientValidationEvaluationCount)
            If AppInfrastructure.IsFinite(d.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy) Then
                metrics.Add("Analytic gradient max relative FD discrepancy") : values.Add(d.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy)
                metrics.Add("Analytic gradient max discrepancy parameter") : values.Add(d.AnalyticGradientValidationFailedParameterIndex)
            End If
            If Not String.IsNullOrWhiteSpace(d.AnalyticGradientValidationMessage) Then
                metrics.Add("Analytic gradient validation message") : values.Add(d.AnalyticGradientValidationMessage)
            End If
            metrics.Add("Analytic gradient derivative-pattern cache enabled") : values.Add(d.AnalyticGradientDerivativePatternCacheEnabled)
            metrics.Add("Analytic gradient derivative-pattern count") : values.Add(d.AnalyticGradientDerivativePatternCount)
            metrics.Add("Analytic gradient derivative-pattern cache hits") : values.Add(d.AnalyticGradientDerivativePatternCacheHits)
            metrics.Add("Analytic gradient derivative-pattern cache misses") : values.Add(d.AnalyticGradientDerivativePatternCacheMisses)
            metrics.Add("Analytic gradient derivative matrices built") : values.Add(d.AnalyticGradientDerivativeMatricesBuilt)
            metrics.Add("Analytic gradient trace/quadratic contraction time ms") : values.Add(SafeTimingValue(d.AnalyticGradientTraceQuadraticContractionTimeMs))
            metrics.Add("Optimizer objective evaluations") : values.Add(d.ObjectiveEvaluationCount)
            metrics.Add("Optimizer gradient evaluations") : values.Add(d.GradientEvaluationCount)
            metrics.Add("Optimizer numerical-gradient objective evaluations") : values.Add(d.NumericalGradientObjectiveEvaluationCount)
            metrics.Add("Estimated numerical-gradient objective evaluations avoided") : values.Add(d.EstimatedNumericalGradientObjectiveEvaluationsAvoided)
            metrics.Add("Optimizer line-search evaluations") : values.Add(d.LineSearchEvaluationCount)
            metrics.Add("Optimizer BFGS resets") : values.Add(d.BfgsResetCount)
            Dim c As MixedModelObjectivePatternCacheDiagnostics = d.ObjectivePatternCache
            If c IsNot Nothing Then
                metrics.Add("Objective pattern cache enabled") : values.Add(c.Enabled)
                metrics.Add("Objective pattern cache evaluations") : values.Add(c.ObjectiveEvaluations)
                metrics.Add("Objective pattern cache pattern count") : values.Add(c.PatternCount)
                metrics.Add("Objective pattern cache hits") : values.Add(c.Hits)
                metrics.Add("Objective pattern cache misses") : values.Add(c.Misses)
                metrics.Add("Objective pattern cache invalid builds") : values.Add(c.InvalidBuilds)
            End If

            Dim pqrCache As MixedModelKrPqrDesignPatternCacheDiagnostics = Nothing
            If Me.KenwardRogerWorkspace IsNot Nothing Then pqrCache = Me.KenwardRogerWorkspace.PqrDesignPatternCache
            If pqrCache IsNot Nothing Then
                metrics.Add("KR P/Q/R design-pattern cache enabled") : values.Add(pqrCache.Enabled)
                metrics.Add("KR P/Q/R design-pattern block count") : values.Add(pqrCache.BlockCount)
                metrics.Add("KR P/Q/R design-pattern count") : values.Add(pqrCache.PatternCount)
                metrics.Add("KR P/Q/R design-pattern hits") : values.Add(pqrCache.Hits)
                metrics.Add("KR P/Q/R design-pattern misses") : values.Add(pqrCache.Misses)
                metrics.Add("KR P/Q/R design-pattern incompatible key collisions") : values.Add(pqrCache.IncompatibleKeyCollisions)
                metrics.Add("KR P/Q/R design-pattern invalid builds") : values.Add(pqrCache.InvalidBuilds)
            End If

            Dim pqr As MixedModelKrPqrPairDiagnostics = Nothing
            If Me.KenwardRogerWorkspace IsNot Nothing Then pqr = Me.KenwardRogerWorkspace.PqrPairDiagnostics
            If pqr IsNot Nothing Then
                metrics.Add("KR P/Q/R half-pair optimization enabled") : values.Add(pqr.Enabled)
                metrics.Add("KR P/Q/R fast factorization enabled") : values.Add(pqr.FastFactorizationEnabled)
                metrics.Add("KR P/Q/R allocation-reduced aggregation enabled") : values.Add(pqr.AllocationReducedAggregationEnabled)
                metrics.Add("KR P/Q/R parameter count") : values.Add(pqr.ParameterCount)
                metrics.Add("KR pair matrices computed") : values.Add(pqr.PairMatricesComputed)
                metrics.Add("KR pair matrices filled by symmetry") : values.Add(pqr.PairMatricesFilledBySymmetry)
                metrics.Add("KR Q pair matrices computed") : values.Add(pqr.QPairMatricesComputed)
                metrics.Add("KR Q pair matrices filled by symmetry") : values.Add(pqr.QPairMatricesFilledBySymmetry)
                metrics.Add("KR R pair matrices computed") : values.Add(pqr.RPairMatricesComputed)
                metrics.Add("KR R pair matrices filled by symmetry") : values.Add(pqr.RPairMatricesFilledBySymmetry)
            End If

            Dim body(metrics.Count - 1, 0) As Object
            For i As Integer = 0 To metrics.Count - 1
                body(i, 0) = values(i)
            Next

            t.SetBody(body)
            t.AddHeaderTopRow({"Value"})
            t.AddHeaderLeftRow(metrics.ToArray())
            t.AddFootnote("Timings are wall-clock diagnostics for the current fit and may vary by machine/load. They do not affect statistical estimates.")
            Return t
        End Function

        Private Shared Function SafeTimingValue(ms As Double) As Object
            If Double.IsNaN(ms) OrElse Double.IsInfinity(ms) OrElse ms < 0.0 Then Return "Not recorded"
            Return ms
        End Function

        ''' <summary>
        ''' Builds visit and visit-pair support diagnostics for MMRM covariance structures.
        ''' </summary>
        Private Function BuildVisitSupportDiagnosticsTable() As ResultTable
            Dim t As New ResultTable
            t.AddTitle("MMRM support diagnostics")

            Dim d As MixedModelVisitSupportDiagnostics = Me.VisitSupportDiagnostics
            If d Is Nothing Then d = New MixedModelVisitSupportDiagnostics()

            Dim metrics As New List(Of String)
            Dim values As New List(Of Object)

            metrics.Add("Covariance structure") : values.Add(d.CovarianceStructureName)
            metrics.Add("Unique visits") : values.Add(If(d.VisitCounts Is Nothing, 0, d.VisitCounts.Count))
            metrics.Add("Minimum visit count") : values.Add(d.MinimumVisitCount)
            metrics.Add("Minimum visit-pair count") : values.Add(d.MinimumVisitPairCount)
            metrics.Add("Weak visit-pair threshold") : values.Add(d.WeakPairThreshold)
            metrics.Add("Warning count") : values.Add(If(d.Warnings Is Nothing, 0, d.Warnings.Count))

            If d.Warnings IsNot Nothing Then
                For i As Integer = 0 To d.Warnings.Count - 1
                    metrics.Add("Warning " & (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture))
                    values.Add(d.Warnings(i))
                Next
            End If

            If d.VisitCounts IsNot Nothing Then
                Dim visitKeys As New List(Of Double)(d.VisitCounts.Keys)
                visitKeys.Sort()
                For Each visitValue As Double In visitKeys
                    metrics.Add("Visit " & visitValue.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & " subject count")
                    values.Add(d.VisitCounts(visitValue))
                Next
            End If

            If d.VisitPairCounts IsNot Nothing Then
                Dim pairKeys As New List(Of String)(d.VisitPairCounts.Keys)
                pairKeys.Sort(StringComparer.Ordinal)
                For Each pairKey As String In pairKeys
                    metrics.Add("Visit pair " & pairKey & " subject count")
                    values.Add(d.VisitPairCounts(pairKey))
                Next
            End If

            Dim body(metrics.Count - 1, 0) As Object
            For i As Integer = 0 To metrics.Count - 1
                body(i, 0) = values(i)
            Next

            t.SetBody(body)
            t.AddHeaderTopRow({"Value"})
            t.AddHeaderLeftRow(metrics.ToArray())
            t.AddFootnote("Support diagnostics count subjects observed at each visit and visit pair. Low pair support can make UN/HAR1-style covariance estimation unstable.")
            Return t
        End Function

        ''' <summary>
        ''' Builds structured restart diagnostics for covariance-parameter optimization.
        ''' </summary>
        Private Function BuildRestartDiagnosticsTable() As ResultTable
            Dim t As New ResultTable
            t.AddTitle("Optimizer restart diagnostics")

            Dim d As MixedModelRestartDiagnostics = Me.RestartDiagnostics
            If d Is Nothing Then d = New MixedModelRestartDiagnostics()

            Dim metrics As New List(Of String)
            Dim values As New List(Of Object)

            metrics.Add("Structured restarts enabled") : values.Add(d.Enabled)
            metrics.Add("Start attempt count") : values.Add(d.StartAttemptCount)
            metrics.Add("Successful start attempt") : values.Add(If(String.IsNullOrWhiteSpace(d.SuccessfulStartAttemptName), "None", d.SuccessfulStartAttemptName))

            If d.FailedStartAttemptMessages IsNot Nothing Then
                For i As Integer = 0 To d.FailedStartAttemptMessages.Count - 1
                    metrics.Add("Failed attempt " & (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture))
                    values.Add(d.FailedStartAttemptMessages(i))
                Next
            End If

            Dim body(metrics.Count - 1, 0) As Object
            For i As Integer = 0 To metrics.Count - 1
                body(i, 0) = values(i)
            Next

            t.SetBody(body)
            t.AddHeaderTopRow({"Value"})
            t.AddHeaderLeftRow(metrics.ToArray())
            t.AddFootnote("Restart attempts are only used after an initial optimizer failure or non-converged optimizer exit; successful first attempts leave the numerical path unchanged.")
            Return t
        End Function


        ''' <summary>
        ''' Builds diagnostics for the adaptive finite-difference KR derivative builder.
        ''' </summary>
        Private Function BuildKrFiniteDifferenceDiagnosticsTable() As ResultTable
            Dim t As New ResultTable
            t.AddTitle("KR finite-difference diagnostics")

            Dim d As MixedModelKrFiniteDifferenceDiagnostics = Nothing
            If Me.KenwardRogerWorkspace IsNot Nothing Then d = Me.KenwardRogerWorkspace.FiniteDifferenceDiagnostics

            If d Is Nothing Then
                Dim emptyBody(0, 0) As Object
                emptyBody(0, 0) = "No KR finite-difference diagnostics were recorded."
                t.SetBody(emptyBody)
                t.AddHeaderTopRow({"Value"})
                Return t
            End If

            Dim metrics As New List(Of String)
            Dim values As New List(Of Object)

            Dim threshold As Double = 0.25
            Dim opts As MixedModelKenwardRogerFiniteDifferenceOptions = Nothing
            If Me.KenwardRogerWorkspace IsNot Nothing Then
                opts = Me.KenwardRogerWorkspace.FiniteDifferenceOptions
                threshold = Me.KenwardRogerWorkspace.FiniteDifferenceWarningThreshold()
            End If

            metrics.Add("Status") : values.Add(d.QualityStatus(threshold))
            metrics.Add("Summary") : values.Add(d.SummaryText(threshold))
            If opts IsNot Nothing Then
                metrics.Add("First derivative step scale") : values.Add(opts.FirstDerivativeStepScale)
                metrics.Add("Second derivative step scale") : values.Add(opts.SecondDerivativeStepScale)
                metrics.Add("Minimum step") : values.Add(opts.MinimumStep)
                metrics.Add("Maximum step") : values.Add(opts.MaximumStep)
                metrics.Add("Maximum step halvings") : values.Add(opts.MaxStepHalvings)
                metrics.Add("Richardson refinement") : values.Add(opts.UseRichardsonRefinement)
                metrics.Add("One-sided first-derivative fallback allowed") : values.Add(opts.AllowOneSidedFirstDerivativeFallback)
                metrics.Add("Richardson warning relative tolerance") : values.Add(opts.RichardsonWarningRelativeTolerance)
                metrics.Add("Perturbed V cache diagnostics") : values.Add(opts.EmitPerturbedViCacheDiagnostics)
            End If
            metrics.Add("Blocks started") : values.Add(d.BlocksStarted)
            metrics.Add("Blocks completed") : values.Add(d.BlocksCompleted)
            metrics.Add("First derivatives, central") : values.Add(d.FirstDerivativeCentralCount)
            metrics.Add("First derivatives, one-sided fallback") : values.Add(d.FirstDerivativeOneSidedFallbackCount)
            metrics.Add("First derivatives, failed") : values.Add(d.FirstDerivativeFailedCount)
            metrics.Add("Pure second derivatives, central") : values.Add(d.PureSecondDerivativeCentralCount)
            metrics.Add("Mixed second derivatives, central") : values.Add(d.MixedSecondDerivativeCentralCount)
            metrics.Add("Second derivatives, failed") : values.Add(d.SecondDerivativeFailedCount)
            metrics.Add("Maximum step-halving level used") : values.Add(d.MaxStepHalvingUsed)
            metrics.Add("Max Richardson relative change, first derivative") : values.Add(d.MaxFirstDerivativeRichardsonRelativeChange)
            metrics.Add("Max Richardson relative change, second derivative") : values.Add(d.MaxSecondDerivativeRichardsonRelativeChange)
            metrics.Add("Perturbed V cache entries") : values.Add(d.PerturbedViCacheEntries)
            metrics.Add("Perturbed V cache hits") : values.Add(d.PerturbedViCacheHits)
            metrics.Add("Perturbed V cache misses") : values.Add(d.PerturbedViCacheMisses)
            metrics.Add("Perturbed V invalid builds") : values.Add(d.PerturbedViCacheInvalidBuilds)

            Dim pc As MixedModelKrDerivativePatternCacheDiagnostics = Nothing
            If Me.KenwardRogerWorkspace IsNot Nothing Then pc = Me.KenwardRogerWorkspace.DerivativePatternCache
            If pc IsNot Nothing Then
                metrics.Add("Derivative pattern cache enabled") : values.Add(pc.Enabled)
                metrics.Add("Derivative pattern cache pattern count") : values.Add(pc.PatternCount)
                metrics.Add("Derivative pattern cache V inverse hits") : values.Add(pc.VInvHits)
                metrics.Add("Derivative pattern cache V inverse misses") : values.Add(pc.VInvMisses)
                metrics.Add("Derivative pattern cache first derivative hits") : values.Add(pc.FirstDerivativeHits)
                metrics.Add("Derivative pattern cache first derivative misses") : values.Add(pc.FirstDerivativeMisses)
                metrics.Add("Derivative pattern cache second derivative hits") : values.Add(pc.SecondDerivativeHits)
                metrics.Add("Derivative pattern cache second derivative misses") : values.Add(pc.SecondDerivativeMisses)
                metrics.Add("Derivative pattern cache invalid builds") : values.Add(pc.InvalidBuilds)
            End If

            Dim body(metrics.Count - 1, 0) As Object
            For i As Integer = 0 To metrics.Count - 1
                body(i, 0) = values(i)
            Next

            t.SetBody(body)
            t.AddHeaderTopRow({"Value"})
            t.AddHeaderLeftRow(metrics.ToArray())
            t.AddFootnote("These diagnostics describe the adaptive finite-difference construction of KR derivative blocks. Nonzero fallback or failure counts indicate a numerically difficult covariance fit.")
            Return t
        End Function

        ''' <summary>
        ''' Builds convergence/status diagnostics.
        ''' </summary>
        Private Function BuildConvergenceTable() As ResultTable
            Dim t As New ResultTable
            t.AddTitle("Convergence")

            Dim metrics As New List(Of String)
            Dim values As New List(Of Object)

            metrics.Add("Converged") : values.Add(Me.Converged)
            metrics.Add("Cancelled") : values.Add(Me.Cancelled)
            metrics.Add("Interrupted") : values.Add(Me.Interrupted)
            metrics.Add("Message") : values.Add(If(Me.Message, String.Empty))
            metrics.Add("Iterations") : values.Add(Me.Iterations)
            metrics.Add("Gradient norm") : values.Add(Me.GradNorm)
            If Me.ControlMaxIter > 0 Then metrics.Add("Requested maximum iterations") : values.Add(Me.ControlMaxIter)
            If AppInfrastructure.IsFinite(Me.ControlEpsilon) Then metrics.Add("Gradient tolerance") : values.Add(Me.ControlEpsilon)
            If AppInfrastructure.IsFinite(Me.ControlStepTolerance) Then metrics.Add("Step tolerance") : values.Add(Me.ControlStepTolerance)
            If AppInfrastructure.IsFinite(Me.ControlFunctionTolerance) Then metrics.Add("Objective-change tolerance") : values.Add(Me.ControlFunctionTolerance)
            metrics.Add("BFGS covariance optimization") : values.Add(Me.ControlUseBfgsCovarianceOptimization)
            metrics.Add("Covariance optimizer mode") : values.Add(Me.ControlCovarianceOptimizerMode.ToString())
            metrics.Add("Covariance gradient mode") : values.Add(Me.ControlCovarianceGradientMode.ToString())
            If AppInfrastructure.IsFinite(Me.ControlAnalyticGradientValidationTolerance) Then metrics.Add("Analytic gradient validation tolerance") : values.Add(Me.ControlAnalyticGradientValidationTolerance)
            metrics.Add("Fallback to numerical gradient on analytic failure") : values.Add(Me.ControlFallbackToNumericalGradientOnAnalyticFailure)
            metrics.Add("Analytic gradient derivative-pattern cache") : values.Add(Me.ControlUseAnalyticGradientDerivativePatternCache)
            metrics.Add("KR P/Q/R design-pattern cache") : values.Add(Me.ControlUseKrPqrDesignPatternCache)
            metrics.Add("KR P/Q/R fast factorization") : values.Add(Me.ControlUseKrPqrFastFactorization)
            If Me.UserWarnings IsNot Nothing AndAlso Me.UserWarnings.Count > 0 Then
                metrics.Add("Warnings") : values.Add(String.Join(" ", Me.UserWarnings.ToArray()))
            End If

            Dim body(metrics.Count - 1, 0) As Object

            For i As Integer = 0 To metrics.Count - 1
                body(i, 0) = values(i)
            Next

            t.SetBody(body)
            t.AddHeaderTopRow({"Value"})
            t.AddHeaderLeftRow(metrics.ToArray())
            Return t
        End Function

        ''' <summary>
        ''' Builds the numeric optimizer iteration trace table.
        ''' </summary>
        Private Function BuildOptimizerTraceTable() As ResultTable
            Dim t As New ResultTable
            t.AddTitle("Optimizer trace")

            If Me.OptimizerTrace Is Nothing Then
                Dim body0(0, 0) As Object
                body0(0, 0) = "No optimizer trace was stored."
                t.SetBody(body0)
                Return t
            End If

            t.SetBody(Me.OptimizerTrace)
            t.AddHeaderTopRow({"Iteration", "Objective", "Gradient norm", "Step norm", "Step size", "New objective"})
            Return t
        End Function

        Friend Shared Function SafeName(names() As String, index As Integer, fallback As String) As String
            If names IsNot Nothing AndAlso index >= 0 AndAlso index < names.Length AndAlso Not String.IsNullOrWhiteSpace(names(index)) Then
                Return names(index)
            End If
            Return fallback
        End Function

        Private Shared Function SafeVectorValue(values() As Double, index As Integer) As Double
            If values Is Nothing OrElse index < 0 OrElse index >= values.Length Then Return Double.NaN
            Return values(index)
        End Function

        Private Shared Function HasFiniteDFVector(values() As Double, p As Integer) As Boolean
            If values Is Nothing OrElse values.Length <> p OrElse p <= 0 Then Return False
            For i As Integer = 0 To values.Length - 1
                If Not AppInfrastructure.NumericGuards.IsFinite(values(i)) OrElse values(i) <= 0.0 Then Return False
            Next
            Return True
        End Function

        ''' <summary>
        ''' Builds a user-scale table for the fitted G-side random-effects covariance matrix.
        ''' </summary>
        Private Function BuildRandomCovarianceMatrixTable() As ResultTable
            Dim foot As String = "This table shows the fitted G-side random-effects covariance matrix on the user/statistical scale."
            If Not String.IsNullOrWhiteSpace(Me.RandomCovarianceStructureName) Then
                foot &= " Structure: " & Me.RandomCovarianceStructureName & "."
            End If
            Return BuildSquareMatrixResultTable("Estimated G covariance matrix", Me.RandomCovarianceUserScale, Me.RandomCovarianceLabels, foot)
        End Function

        ''' <summary>
        ''' Builds a user-scale table for the fitted G-side random-effects correlation matrix.
        ''' </summary>
        Private Function BuildRandomCorrelationMatrixTable() As ResultTable
            Dim foot As String = "Correlations are derived from the fitted G covariance matrix as G_ij / sqrt(G_ii G_jj)."
            If Not String.IsNullOrWhiteSpace(Me.RandomCovarianceStructureName) Then
                foot &= " Structure: " & Me.RandomCovarianceStructureName & "."
            End If
            Return BuildSquareMatrixResultTable("Estimated G correlation matrix", Me.RandomCorrelationUserScale, Me.RandomCovarianceLabels, foot)
        End Function

        ''' <summary>
        ''' Builds a table of subject-specific BLUP/random-effect predictions.
        ''' </summary>
        Private Function BuildRandomEffectsTable() As ResultTable
            Dim t As New ResultTable
            t.AddTitle("BLUPs / random effects")

            If Me.RandomEffects Is Nothing OrElse Me.RandomEffects.Count = 0 Then
                Dim body0(0, 0) As Object
                body0(0, 0) = "Not available"
                t.SetBody(body0)
                t.AddHeaderTopRow({"Status"})
                Return t
            End If

            Dim subjects As New List(Of String)(Me.RandomEffects.Keys)
            subjects.Sort(StringComparer.Ordinal)

            Dim q As Integer = 0
            For Each key As String In subjects
                Dim b() As Double = Me.RandomEffects(key)
                If b IsNot Nothing Then q = Math.Max(q, b.Length)
            Next

            If q <= 0 Then
                Dim body0(0, 0) As Object
                body0(0, 0) = "No random-effect vectors were stored."
                t.SetBody(body0)
                t.AddHeaderTopRow({"Status"})
                Return t
            End If

            Dim body(subjects.Count - 1, q - 1) As Object
            Dim rowLabels(subjects.Count - 1) As String
            Dim colLabels(q - 1) As String

            For j As Integer = 0 To q - 1
                colLabels(j) = SafeMatrixLabel(Me.RandomCovarianceLabels, j, "b" & CStr(j + 1))
            Next

            For i As Integer = 0 To subjects.Count - 1
                Dim key As String = subjects(i)
                rowLabels(i) = key
                Dim b() As Double = Me.RandomEffects(key)
                For j As Integer = 0 To q - 1
                    If b IsNot Nothing AndAlso j < b.Length Then
                        body(i, j) = b(j)
                    Else
                        body(i, j) = Double.NaN
                    End If
                Next
            Next

            t.SetBody(body)
            t.AddHeaderTopRow(colLabels)
            t.AddHeaderLeftRow(rowLabels)
            t.AddFootnote("Subject-specific conditional modes/BLUPs for the fitted random effects. These predictions are empirical Bayes estimates, not additional fixed-effect coefficients.")
            Return t
        End Function

        ''' <summary>
        ''' Builds a user-scale table for the fitted R-side residual covariance matrix.
        ''' </summary>
        Private Function BuildResidualCovarianceMatrixTable() As ResultTable
            Dim title As String = "Estimated R covariance matrix"
            Dim foot As String

            If String.Equals(Me.RandomCovarianceStructureName, "None", StringComparison.OrdinalIgnoreCase) _
                OrElse String.IsNullOrWhiteSpace(Me.RandomCovarianceStructureName) Then
                foot = "For MMRM/no-random-effects fits, this R matrix is the fitted marginal within-subject covariance matrix."
            Else
                foot = "This table shows the residual R-side covariance matrix.  The marginal V matrix also includes any ZGZ' random-effects contribution."
            End If

            If Not String.IsNullOrWhiteSpace(Me.ResidualCovarianceStructureName) Then
                foot &= "  Structure: " & Me.ResidualCovarianceStructureName & "."
            End If

            Return BuildSquareMatrixResultTable(title, Me.ResidualCovarianceUserScale, Me.ResidualCovarianceVisitLabels, foot)
        End Function

        ''' <summary>
        ''' Builds a user-scale table for the fitted R-side residual correlation matrix.
        ''' </summary>
        Private Function BuildResidualCorrelationMatrixTable() As ResultTable
            Dim title As String = "Estimated R correlation matrix"
            Dim foot As String = "Correlations are derived from the fitted R covariance matrix as R_ij / sqrt(R_ii R_jj)."

            If Not String.IsNullOrWhiteSpace(Me.ResidualCovarianceStructureName) Then
                foot &= "  Structure: " & Me.ResidualCovarianceStructureName & "."
            End If

            Return BuildSquareMatrixResultTable(title, Me.ResidualCorrelationUserScale, Me.ResidualCovarianceVisitLabels, foot)
        End Function

        ''' <summary>
        ''' Builds a square matrix ResultTable with the same labels on rows and columns.
        ''' </summary>
        Private Shared Function BuildSquareMatrixResultTable(title As String,
                                                             mat(,) As Double,
                                                             labels() As String,
                                                             Optional footnote As String = Nothing) As ResultTable
            Dim t As New ResultTable
            t.AddTitle(title)

            If mat Is Nothing Then
                Dim body0(0, 0) As Object
                body0(0, 0) = "Not available"
                t.SetBody(body0)
                Return t
            End If

            Dim nRows As Integer = mat.GetLength(0)
            Dim nCols As Integer = mat.GetLength(1)
            Dim body(nRows - 1, nCols - 1) As Object
            Dim rowLabels(nRows - 1) As String
            Dim colLabels(nCols - 1) As String

            For i As Integer = 0 To nRows - 1
                rowLabels(i) = SafeMatrixLabel(labels, i, "V" & CStr(i + 1))
                For j As Integer = 0 To nCols - 1
                    body(i, j) = mat(i, j)
                Next
            Next

            For j As Integer = 0 To nCols - 1
                colLabels(j) = SafeMatrixLabel(labels, j, "V" & CStr(j + 1))
            Next

            t.SetBody(body)
            t.AddHeaderTopRow(colLabels)
            t.AddHeaderLeftRow(rowLabels)
            If Not String.IsNullOrWhiteSpace(footnote) Then t.AddFootnote(footnote)
            Return t
        End Function

        Private Sub AppendAdditionalResultTables(out As List(Of ResultTable))
            If out Is Nothing Then Exit Sub
            If Me.AdditionalResultTables Is Nothing Then Exit Sub

            For Each t As ResultTable In Me.AdditionalResultTables
                If t IsNot Nothing Then out.Add(t)
            Next
        End Sub

        ''' <summary>
        ''' Returns a safe matrix row/column label.
        ''' </summary>
        Private Shared Function SafeMatrixLabel(labels() As String, index As Integer, fallback As String) As String
            If labels IsNot Nothing AndAlso index >= 0 AndAlso index < labels.Length AndAlso Not String.IsNullOrWhiteSpace(labels(index)) Then
                Return labels(index)
            End If
            Return fallback
        End Function

        Private Shared Function FormatExecutionTime(ms As Double) As String
            If Double.IsNaN(ms) OrElse Double.IsInfinity(ms) OrElse ms < 0.0 Then Return String.Empty

            Dim ts As TimeSpan = TimeSpan.FromMilliseconds(ms)

            If ts.TotalHours >= 1.0 Then
                Return String.Format(Globalization.CultureInfo.InvariantCulture,
                                     "{0}:{1:00}:{2:00}.{3:000}",
                                     CInt(Math.Floor(ts.TotalHours)),
                                     ts.Minutes,
                                     ts.Seconds,
                                     ts.Milliseconds)
            End If

            If ts.TotalMinutes >= 1.0 Then
                Return String.Format(Globalization.CultureInfo.InvariantCulture,
                                     "{0}:{1:00}.{2:000}",
                                     CInt(Math.Floor(ts.TotalMinutes)),
                                     ts.Seconds,
                                     ts.Milliseconds)
            End If

            Return String.Format(Globalization.CultureInfo.InvariantCulture,
                                 "{0:0.000} s",
                                 ts.TotalSeconds)
        End Function

#End Region

    End Class


End Namespace
