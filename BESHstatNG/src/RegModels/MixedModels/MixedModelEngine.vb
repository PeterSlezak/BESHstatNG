Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Internal profiled-likelihood evaluation used by <see cref="MixedModelEngine"/>.
    ''' </summary>
    Friend Structure MixedModelProfileEvaluation
        Public Success As Boolean
        Public Message As String
        Public Criterion As Double
        Public LogLik As Double
        Public Beta() As Double
        Public VarBeta(,) As Double
        Public XtVinvX(,) As Double
        Public XtVinvY() As Double
        Public QForm As Double
        Public LogDetV As Double
        Public LogDetXtVinvX As Double
        Public Sigma2Profile As Double
    End Structure

    ''' <summary>
    ''' Gaussian subject-block likelihood engine for linear mixed models and MMRM.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This class is the numerical engine that combines the added mixed-model building blocks:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><see cref="MixedModelBlockData"/> provides subject-level blocks <c>y_i, X_i, Z_i</c>.</description></item>
    ''' <item><description><see cref="MixedModelGStruct"/> builds random-effects covariance matrices <c>G</c>.</description></item>
    ''' <item><description><see cref="MixedModelRStruct"/> builds residual/within-subject covariance matrices <c>R_i</c>.</description></item>
    ''' <item><description><see cref="MixedModelCovariance"/> assembles and solves subject-level marginal covariance matrices.</description></item>
    ''' <item><description><see cref="MixedModelOptimizer"/> optimizes the covariance parameters.</description></item>
    ''' </list>
    ''' <para>
    ''' The engine uses the marginal Gaussian covariance representation
    ''' </para>
    ''' <para><c>V_i = Z_i G Z_i' + R_i</c></para>
    ''' <para>
    ''' and profiles the fixed effects for each covariance-parameter proposal.  This is deliberately
    ''' MMRM-ready: when the request has no random-effects design and no active G-side structure,
    ''' <c>V_i = R_i</c>, which is the standard MMRM covariance path.
    ''' </para>
    ''' <para>
    ''' The implemented profiled objectives are:
    ''' </para>
    ''' <para><c>-2 log L_ML = log|V| + Q + n log(2π)</c></para>
    ''' <para><c>-2 log L_REML = log|V| + log|X'V^-1X| + Q + (n-p) log(2π)</c></para>
    ''' <para>
    ''' where <c>Q = (y - X beta)' V^-1 (y - X beta)</c> after profiling <c>beta</c>.
    ''' </para>
    ''' <para>
    ''' Implementation decisions:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>All matrix solves use the existing project Cholesky helpers through <c>Matrix.vb</c>.</description></item>
    ''' <item><description>Invalid covariance proposals are converted to a large penalty during optimization.</description></item>
    ''' <item><description>Fixed effects are profiled out at every covariance proposal.</description></item>
    ''' <item><description>Logging is emitted both to <see cref="CoreServices.logger"/> and to an in-memory trace string for future UI exposure.</description></item>
    ''' <item><description>Satterthwaite and Kenward-Roger inference are not implemented here; the result currently reports large-sample Wald z diagnostics.</description></item>
    ''' </list>
    ''' </remarks>
    Partial Public Class MixedModelEngine

        Private Const TwoPi As Double = 6.2831853071795862R
        Private Const PenaltyObjective As Double = 1.0E+100

        Private ReadOnly pRequest As MixedModelFitRequest
        Private pResult As MixedModelResult = Nothing
        Private pStrTrace As String = String.Empty
        Private pPerformanceDiagnostics As MixedModelPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
        Private pRestartDiagnostics As MixedModelRestartDiagnostics = New MixedModelRestartDiagnostics()
        Private pFitStopwatch As System.Diagnostics.Stopwatch = Nothing

        Private Class CachedObjectiveCovarianceBlock
            Public Property Chol As Double(,)
            Public Property LogDet As Double
        End Class

        Private Class MixedModelStartAttempt
            Public Property Name As String = String.Empty
            Public Property Theta As Double() = Array.Empty(Of Double)()
        End Class

        ''' <summary>
        ''' Creates a new engine instance for one mixed-model fit request.
        ''' </summary>
        Public Sub New(req As MixedModelFitRequest)
            If req Is Nothing Then Throw New ArgumentNullException(NameOf(req))
            pRequest = req
            AppendTrace("MixedModelEngine.New initialized. " & req.Describe())
        End Sub

        ''' <summary>
        ''' Last fit result produced by <see cref="Fit"/>.
        ''' </summary>
        Public ReadOnly Property Result As MixedModelResult
            Get
                Return pResult
            End Get
        End Property

        ''' <summary>
        ''' Fits the Gaussian LMM/MMRM by optimizing profiled ML or REML over covariance parameters.
        ''' </summary>
        ''' <returns>A <see cref="MixedModelResult"/> containing numerical estimates and diagnostics.</returns>
        Public Function Fit() As MixedModelResult
            Dim sw As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
            pFitStopwatch = sw
            Dim startedUtc As DateTime = DateTime.UtcNow
            pPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            pPerformanceDiagnostics.SelectedCovarianceGradientMode = pRequest.Control.CovarianceGradientMode
            pPerformanceDiagnostics.SelectedCovarianceOptimizerMode = pRequest.Control.CovarianceOptimizerMode
            pPerformanceDiagnostics.ActualCovarianceOptimizerName = pRequest.Control.CovarianceOptimizerMode.ToString()
            pRestartDiagnostics = New MixedModelRestartDiagnostics()

            Try
                Dim startingStopwatch As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()

                ReportProgress("Validating request", 0)
                AppendInfo("MixedModelEngine.Fit start. " & pRequest.Describe())
                pRequest.Validate()
                ThrowIfCancellationRequested()
                ReportProgress("Preparing starting values", 10)

                Dim olsBeta() As Double = Nothing
                Dim olsResidualVar As Double = EstimateOLSResidualVariance(olsBeta)
                AppendDebug($"MixedModelEngine.Fit OLS start scale={olsResidualVar}; p={pRequest.Data.P}.")

                Dim startThetaG() As Double = GetStartThetaG(olsResidualVar)
                Dim startThetaR() As Double = GetStartThetaR(olsResidualVar)
                Dim startTheta() As Double = PackTheta(startThetaG, startThetaR)
                AppendDebug($"MixedModelEngine.Fit start theta length={startTheta.Length}; g={startThetaG.Length}; r={startThetaR.Length}.")
                startingStopwatch.Stop()
                pPerformanceDiagnostics.StartingValuesTimeMs = startingStopwatch.Elapsed.TotalMilliseconds

                Dim optState As MixedModelOptimizationState
                Dim optimizationStopwatch As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()

                If startTheta.Length = 0 Then
                    ReportProgress("Evaluating model", 40)

                    AppendTrace("MixedModelEngine.Fit zero covariance-parameter case; optimization skipped.")
                    Dim eval0 As MixedModelProfileEvaluation = EvaluateProfileCriterion(startTheta, throwOnFailure:=True)
                    optState = New MixedModelOptimizationState With {
                            .Theta = Array.Empty(Of Double)(),
                            .Objective = eval0.Criterion,
                            .Iterations = 0,
                            .Converged = True,
                            .GradNorm = 0.0,
                            .Status = MixedModelOptimizationStatus.NotStarted,
                            .GradientProviderName = "No covariance parameters",
                            .Message = "No covariance parameters to optimize; profiled criterion evaluated once."
                        }
                Else
                    ReportProgress("Starting covariance optimization", 20, maxIterations:=pRequest.Control.MaxIter)

                    optState = OptimizeCovarianceParametersWithRestarts(startTheta, olsResidualVar)
                End If

                optimizationStopwatch.Stop()
                pPerformanceDiagnostics.OptimizationTimeMs = optimizationStopwatch.Elapsed.TotalMilliseconds

                If optState.Status = MixedModelOptimizationStatus.Cancelled Then
                    pResult = BuildCancelledResult(optState, startedUtc, sw.Elapsed.TotalMilliseconds, optState.Message)
                    ReportProgress("Cancelled", 100, iteration:=pResult.Iterations, maxIterations:=pRequest.Control.MaxIter, objective:=pResult.Objective, message:=pResult.Message)
                    Return pResult
                End If

                If optState.Status = MixedModelOptimizationStatus.Interrupted Then
                    ReportProgress("Building interrupted result", 95, iteration:=optState.Iterations, maxIterations:=pRequest.Control.MaxIter, objective:=optState.Objective, message:=optState.Message)
                    pResult = BuildResult(optState, skipKenwardRoger:=True)
                    MarkInterruptedResult(pResult)
                    pResult.strTrace = MergeTraces(pResult.strTrace, pStrTrace)

                    sw.Stop()
                    pResult.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds
                    If pResult.PerformanceDiagnostics Is Nothing Then pResult.PerformanceDiagnostics = pPerformanceDiagnostics
                    pResult.PerformanceDiagnostics.TotalFitTimeMs = sw.Elapsed.TotalMilliseconds
                    pResult.ExecutionStartedUtc = startedUtc
                    pResult.ExecutionCompletedUtc = DateTime.UtcNow

                    ReportProgress("Interrupted", 100, iteration:=pResult.Iterations, maxIterations:=pRequest.Control.MaxIter, objective:=pResult.Objective, message:=pResult.Message)
                    Return pResult
                End If

                ThrowIfCancellationRequested()
                ReportProgress("Building result", 95)

                pResult = BuildResult(optState)
                pResult.strTrace = MergeTraces(pResult.strTrace, pStrTrace)

                sw.Stop()
                pResult.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds
                If pResult.PerformanceDiagnostics Is Nothing Then pResult.PerformanceDiagnostics = pPerformanceDiagnostics
                pResult.PerformanceDiagnostics.TotalFitTimeMs = sw.Elapsed.TotalMilliseconds
                pResult.ExecutionStartedUtc = startedUtc
                pResult.ExecutionCompletedUtc = DateTime.UtcNow

                ReportProgress("Completed", 100, iteration:=pResult.Iterations, maxIterations:=pRequest.Control.MaxIter, objective:=pResult.Objective)

                AppendInfo($"MixedModelEngine.Fit completed. converged={pResult.Converged}; objective={pResult.Objective}; iter={pResult.Iterations}; elapsedMs={pResult.ExecutionTimeMs}.")
                pResult.strTrace = MergeTraces(pResult.strTrace, pStrTrace)
                Return pResult

            Catch ex As OperationCanceledException
                sw.Stop()
                pResult = BuildCancelledResult(Nothing, startedUtc, sw.Elapsed.TotalMilliseconds, ex.Message)
                ReportProgress("Cancelled", 100, message:=pResult.Message)
                Return pResult

            Catch
                sw.Stop()
                ReportProgress("Failed", 100, message:="Model fitting failed after " & sw.Elapsed.TotalSeconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) & " s.")
                Throw
            End Try
        End Function

        ''' <summary>
        ''' Evaluates the profiled objective and returns only the scalar criterion.
        ''' </summary>
        Private Function SafeObjective(theta() As Double) As Double
            Try
                ThrowIfCancellationRequested()
                ' Do not collect block-level trace during numerical optimization.
                Dim ev As MixedModelProfileEvaluation = EvaluateProfileCriterion(theta,
                                                                         throwOnFailure:=False,
                                                                         collectTrace:=False)
                If Not ev.Success OrElse Not IsFinite(ev.Criterion) Then
                    Return PenaltyObjective
                End If
                Return ev.Criterion
            Catch ex As OperationCanceledException
                Throw
            Catch ex As Exception
                ' Keep this short; optimizer can call this many times.
                Return PenaltyObjective
            End Try
        End Function

        ''' <summary>
        ''' Evaluates the profiled ML or REML criterion for a covariance-parameter vector.
        ''' </summary>
        ''' <param name="theta">Combined internal covariance-parameter vector: G parameters followed by R parameters.</param>
        ''' <param name="throwOnFailure">If True, numerical failures are thrown.  If False, a failed evaluation is returned.</param>
        Private Function EvaluateProfileCriterion(theta() As Double,
                                          Optional throwOnFailure As Boolean = False,
                                          Optional collectTrace As Boolean = False) As MixedModelProfileEvaluation

            Dim ev As New MixedModelProfileEvaluation With {
                .Success = False,
                .Message = String.Empty,
                .Criterion = PenaltyObjective,
                .LogLik = Double.NaN,
                .QForm = Double.NaN,
                .LogDetV = Double.NaN,
                .LogDetXtVinvX = Double.NaN,
                .Sigma2Profile = Double.NaN
            }

            ' Local low-level trace sink.  If collectTrace=False this remains Nothing,
            ' and the patched covariance/G/R LogTrace functions will do nothing.
            Dim evalTrace As String = Nothing

            Try
                ThrowIfCancellationRequested()
                Dim thetaG() As Double = Nothing
                Dim thetaR() As Double = Nothing
                UnpackTheta(theta, thetaG, thetaR)

                Dim p As Integer = pRequest.Data.P
                Dim n As Integer = pRequest.Data.Nobs
                Dim xtVinvX(p - 1, p - 1) As Double
                Dim xtVinvY(p - 1) As Double
                Dim yVinvY As Double = 0.0
                Dim logDetV As Double = 0.0

                Dim useObjectivePatternCache As Boolean = ShouldUseObjectiveVisitPatternCache()
                Dim objectiveBlockCache As Dictionary(Of String, CachedObjectiveCovarianceBlock) = Nothing
                Dim objectiveCacheDiag As MixedModelObjectivePatternCacheDiagnostics = Nothing

                If useObjectivePatternCache Then
                    objectiveBlockCache = New Dictionary(Of String, CachedObjectiveCovarianceBlock)(StringComparer.Ordinal)
                    objectiveCacheDiag = EnsureObjectivePatternCacheDiagnostics()
                    objectiveCacheDiag.Enabled = True
                    objectiveCacheDiag.ObjectiveEvaluations += 1
                End If

                For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                    ThrowIfCancellationRequested()
                    Dim chol(,) As Double = Nothing
                    Dim blockLogDet As Double = 0.0

                    If useObjectivePatternCache Then
                        Dim patternKey As String = BuildObjectiveVisitPatternKey(block)
                        Dim cached As CachedObjectiveCovarianceBlock = Nothing

                        If objectiveBlockCache.TryGetValue(patternKey, cached) Then
                            objectiveCacheDiag.Hits += 1
                            chol = cached.Chol
                            blockLogDet = cached.LogDet
                        Else
                            objectiveCacheDiag.Misses += 1

                            Dim vi(,) As Double = MixedModelCovariance.BuildVi(block, pRequest.Data, ActiveGStruct(),
                                                                       pRequest.ResidualStruct, thetaG, thetaR, evalTrace)

                            If Not MixedModelCovariance.TryCholesky(vi, chol, evalTrace) Then
                                objectiveCacheDiag.InvalidBuilds += 1
                                ev.Message = $"V_i was Not positive definite for subject '{block.SubjectKey}'."
                                If throwOnFailure Then Throw New ApplicationException(ev.Message)
                                Return ev
                            End If

                            blockLogDet = MixedModelCovariance.LogDetFromCholesky(chol)
                            objectiveBlockCache(patternKey) = New CachedObjectiveCovarianceBlock With {
                                    .Chol = chol,
                                    .LogDet = blockLogDet
                                }
                        End If
                    Else
                        Dim vi(,) As Double = MixedModelCovariance.BuildVi(block,
                                                                   pRequest.Data,
                                                                   ActiveGStruct(),
                                                                   pRequest.ResidualStruct,
                                                                   thetaG,
                                                                   thetaR,
                                                                   evalTrace)

                        If Not MixedModelCovariance.TryCholesky(vi, chol, evalTrace) Then
                            ev.Message = $"V_i was Not positive definite for subject '{block.SubjectKey}'."
                            If throwOnFailure Then Throw New ApplicationException(ev.Message)
                            Return ev
                        End If

                        blockLogDet = MixedModelCovariance.LogDetFromCholesky(chol)
                    End If

                    logDetV += blockLogDet
                    MixedModelCovariance.AccumulateProfileCrossProducts(block, chol, xtVinvX, xtVinvY, yVinvY, evalTrace)
                Next

                If useObjectivePatternCache AndAlso objectiveCacheDiag IsNot Nothing AndAlso objectiveBlockCache IsNot Nothing Then
                    objectiveCacheDiag.PatternCount = Math.Max(objectiveCacheDiag.PatternCount, objectiveBlockCache.Count)
                End If

                Dim cholX(,) As Double = Nothing
                If Not MixedModelCovariance.TryCholesky(xtVinvX, cholX, evalTrace) Then
                    ev.Message = "X'V^-1X was not positive definite.  The fixed-effects design may be rank deficient."
                    If throwOnFailure Then Throw New ApplicationException(ev.Message)
                    Return ev
                End If

                Dim beta() As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(cholX, xtVinvY)
                Dim betaDot As Double = Global.BESHStatNG.Matrix.Matrix.DotProduct(beta, xtVinvY)
                Dim qForm As Double = Math.Max(0.0, yVinvY - betaDot)
                Dim logDetX As Double = MixedModelCovariance.LogDetFromCholesky(cholX)
                Dim varBeta(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholInv(cholX)

                Dim df As Integer = If(pRequest.FitMethod = MixedModelFitMethod.REML, n - p, n)
                If df <= 0 Then
                    ev.Message = "Mixed-model likelihood has non-positive residual degrees of freedom."
                    If throwOnFailure Then Throw New ApplicationException(ev.Message)
                    Return ev
                End If

                Dim criterion As Double
                If pRequest.FitMethod = MixedModelFitMethod.REML Then
                    criterion = logDetV + logDetX + qForm + CDbl(n - p) * Math.Log(TwoPi)
                Else
                    criterion = logDetV + qForm + CDbl(n) * Math.Log(TwoPi)
                End If

                If Not IsFinite(criterion) Then
                    ev.Message = "Profile criterion evaluated to a non-finite value."
                    If throwOnFailure Then Throw New ApplicationException(ev.Message)
                    Return ev
                End If

                ev.Success = True
                ev.Message = "OK"
                ev.Criterion = criterion
                ev.LogLik = -0.5 * criterion
                ev.Beta = beta
                ev.VarBeta = varBeta
                ev.XtVinvX = xtVinvX
                ev.XtVinvY = xtVinvY
                ev.QForm = qForm
                ev.LogDetV = logDetV
                ev.LogDetXtVinvX = logDetX
                ev.Sigma2Profile = qForm / CDbl(df)

                If collectTrace AndAlso Not String.IsNullOrEmpty(evalTrace) Then
                    pStrTrace = MergeTraces(pStrTrace, evalTrace)
                End If

                Return ev
            Catch ex As OperationCanceledException
                Throw

            Catch ex As Exception
                ev.Success = False
                ev.Message = ex.Message
                ev.Criterion = PenaltyObjective
                If throwOnFailure Then Throw
                Return ev
            End Try
        End Function

        ''' <summary>
        ''' Builds the final result object from the optimizer state and one final full likelihood evaluation.
        ''' </summary>
        Private Function BuildResult(optState As MixedModelOptimizationState, Optional skipKenwardRoger As Boolean = False) As MixedModelResult
            ThrowIfCancellationRequested()
            Dim res As New MixedModelResult
            res.PerformanceDiagnostics = pPerformanceDiagnostics
            If res.PerformanceDiagnostics Is Nothing Then res.PerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            CopyOptimizerDiagnosticsToPerformance(optState, res.PerformanceDiagnostics)
            res.RestartDiagnostics = If(pRestartDiagnostics Is Nothing, New MixedModelRestartDiagnostics(), pRestartDiagnostics.Clone())
            res.VisitSupportDiagnostics = BuildVisitSupportDiagnostics()
            AppendVisitSupportWarnings(res)
            res.FitMethod = pRequest.FitMethod
            res.Nobs = pRequest.Data.Nobs
            res.NoSubjects = pRequest.Data.NoSubjects
            res.P = pRequest.Data.P
            res.Q = pRequest.Data.Q
            res.ControlMaxIter = pRequest.Control.MaxIter
            res.ControlEpsilon = pRequest.Control.Epsilon
            res.ControlStepTolerance = pRequest.Control.StepTolerance
            res.ControlFunctionTolerance = pRequest.Control.FunctionTolerance
            res.ControlUseBfgsCovarianceOptimization = pRequest.Control.UseBfgsCovarianceOptimization
            res.ControlCovarianceGradientMode = pRequest.Control.CovarianceGradientMode
            res.ControlCovarianceOptimizerMode = pRequest.Control.CovarianceOptimizerMode
            res.ControlAnalyticGradientValidationTolerance = pRequest.Control.AnalyticGradientValidationTolerance
            res.ControlFallbackToNumericalGradientOnAnalyticFailure = pRequest.Control.FallbackToNumericalGradientOnAnalyticFailure
            res.ControlUseKrPqrDesignPatternCache = pRequest.Control.UseKrPqrDesignPatternCache
            res.ControlUseKrPqrFastFactorization = pRequest.Control.UseKrPqrFastFactorization
            res.ControlUseAnalyticGradientDerivativePatternCache = pRequest.Control.UseAnalyticGradientDerivativePatternCache
            res.FixedEffectNames = GetFixedEffectNames()
            res.Iterations = optState.Iterations
            res.GradNorm = optState.GradNorm
            res.OptimizerTrace = optState.TraceTable
            res.Converged = optState.Converged
            res.Message = optState.Message
            res.Theta = If(optState.Theta Is Nothing, Array.Empty(Of Double)(), CType(optState.Theta.Clone(), Double()))
            res.Objective = optState.Objective

            Dim finalEvaluationStopwatch As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
            Dim ev As MixedModelProfileEvaluation = EvaluateProfileCriterion(res.Theta, throwOnFailure:=False)
            finalEvaluationStopwatch.Stop()
            EnsurePerformanceDiagnostics(res).FinalEvaluationTimeMs = finalEvaluationStopwatch.Elapsed.TotalMilliseconds
            If Not ev.Success Then
                res.Converged = False
                res.Message = If(String.IsNullOrWhiteSpace(res.Message), ev.Message, res.Message & " Final evaluation failed: " & ev.Message)
                AppendWarn("MixedModelEngine.BuildResult final evaluation failed: " & ev.Message)
                res.strTrace = pStrTrace
                Return res
            End If

            res.Objective = ev.Criterion
            res.LogLik = ev.LogLik
            res.Beta = If(ev.Beta Is Nothing, Array.Empty(Of Double)(), CType(ev.Beta.Clone(), Double()))
            res.VarBeta = If(ev.VarBeta Is Nothing, Nothing, CType(ev.VarBeta.Clone(), Double(,)))
            If res.VarBeta IsNot Nothing Then
                Dim vbCondition As Double = MixedModelNumericalDiagnostics.EstimateConditionNumberBySvd(res.VarBeta)
                Dim vbWarning As String = MixedModelNumericalDiagnostics.WarningForConditionNumber("Fixed-effect covariance Var(beta)", vbCondition)
                If Not String.IsNullOrWhiteSpace(vbWarning) Then res.AddUserWarning(vbWarning)
            End If
            res.QForm = ev.QForm
            res.LogDetV = ev.LogDetV
            res.LogDetXtVinvX = ev.LogDetXtVinvX
            res.Sigma2Profile = ev.Sigma2Profile
            If pRequest.FitMethod = MixedModelFitMethod.REML Then res.REMLCriterion = ev.Criterion

            Dim thetaG() As Double = Nothing, thetaR() As Double = Nothing
            UnpackTheta(res.Theta, thetaG, thetaR)
            res.ThetaG = thetaG
            res.ThetaR = thetaR
            res.ThetaGNames = GetThetaGNames()
            res.ThetaRNames = GetThetaRNames()
            res.ResidualCovarianceStructureName = If(pRequest Is Nothing, String.Empty, pRequest.ResidualStructName())
            res.RandomCovarianceStructureName = If(pRequest Is Nothing, String.Empty, pRequest.RandomStructName())

            PopulateUserScaleRandomCovariance(res)
            PopulateUserScaleResidualCovariance(res)

            Dim needsKrBeforeFixedDiagnostics As Boolean = Not skipKenwardRoger AndAlso (pRequest.BuildKenwardRogerWorkspace OrElse
                                                            pRequest.FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger)
            If needsKrBeforeFixedDiagnostics Then
                ReportProgress("Building KR derivative workspace", 97)
                PopulateKenwardRogerWorkspace(res)
            ElseIf skipKenwardRoger AndAlso (pRequest.BuildKenwardRogerWorkspace OrElse pRequest.FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger) Then
                res.AddUserWarning("MMRM calculation was interrupted by the user; KR post-estimation was skipped and returned estimates are from the latest accepted covariance-parameter iterate.")
            End If

            PopulateFixedEffectDiagnostics(res)
            PopulateFittedResidualsAndBLUPs(res)
            PopulateInformationCriteria(res)

            res.strTrace = pStrTrace
            Return res
        End Function

        ''' <summary>
        ''' Computes fixed-effect standard errors, test statistics, p-values, and optional
        ''' denominator degrees of freedom.
        ''' </summary>
        Private Sub PopulateFixedEffectDiagnostics(res As MixedModelResult)
            Dim p As Integer = res.P
            If res.Beta Is Nothing OrElse res.Beta.Length <> p OrElse res.VarBeta Is Nothing Then Exit Sub

            Dim inference As MixedModelFixedInferenceMethod = ResolveFixedInferenceMethod()

            If inference = MixedModelFixedInferenceMethod.KenwardRoger AndAlso (res.KenwardRogerWorkspace Is Nothing OrElse
                    MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res) Is Nothing) Then

                AppendWarn("Kenward-Roger fixed-effect inference requested, but KR adjusted Var(beta) is unavailable; falling back to Wald normal inference.")
                inference = MixedModelFixedInferenceMethod.WaldNormal
            End If

            Dim se(p - 1) As Double
            Dim stat(p - 1) As Double
            Dim z(p - 1) As Double
            Dim pv(p - 1) As Double
            Dim df() As Double = Nothing

            If inference = MixedModelFixedInferenceMethod.KenwardRoger Then
                df = MakeNaNVector(p)

                For j As Integer = 0 To p - 1
                    Dim l(p - 1) As Double
                    l(j) = 1.0

                    Dim krInf As MixedModelKenwardRogerUnivariateInference = Nothing
                    Dim krMsg As String = Nothing

                    If MixedModelKenwardRogerInference.TryUnivariateInference(res,
                                                                      regression.MixedModelResult.SafeName(res.FixedEffectNames, j, "b" & CStr(j)),
                                                                      l,
                                                                      krInf,
                                                                      alpha:=0.05,
                                                                      diagnostic:=krMsg) Then
                        se(j) = krInf.AdjustedStdError
                        stat(j) = krInf.Statistic
                        z(j) = stat(j)
                        pv(j) = krInf.PValue
                        df(j) = krInf.DF
                    Else
                        AppendWarn("Kenward-Roger coefficient inference failed for beta index " & j.ToString() & ": " & krMsg)

                        Dim v As Double = res.VarBeta(j, j)
                        If v >= 0.0 AndAlso IsFinite(v) Then
                            se(j) = Math.Sqrt(v)
                        Else
                            se(j) = Double.NaN
                        End If

                        If se(j) > 0.0 AndAlso IsFinite(se(j)) Then
                            stat(j) = res.Beta(j) / se(j)
                            z(j) = stat(j)
                            pv(j) = 2.0 * (1.0 - Global.BESHStatNG.distributions.Distributions.PNorm(Math.Abs(stat(j))))
                            If pv(j) < 0.0 Then pv(j) = 0.0
                            If pv(j) > 1.0 Then pv(j) = 1.0
                        Else
                            stat(j) = Double.NaN
                            z(j) = Double.NaN
                            pv(j) = Double.NaN
                        End If
                    End If
                Next

            Else
                df = ComputeFixedEffectDenominatorDFs(res)

                For j As Integer = 0 To p - 1
                    Dim v As Double = res.VarBeta(j, j)
                    If v >= 0.0 AndAlso IsFinite(v) Then
                        se(j) = Math.Sqrt(v)
                    Else
                        se(j) = Double.NaN
                    End If

                    If se(j) > 0.0 AndAlso IsFinite(se(j)) Then
                        stat(j) = res.Beta(j) / se(j)
                        z(j) = stat(j)

                        If inference = MixedModelFixedInferenceMethod.WaldNormal OrElse df Is Nothing OrElse j >= df.Length OrElse Not IsFinite(df(j)) OrElse df(j) <= 0.0 Then
                            pv(j) = 2.0 * (1.0 - Global.BESHStatNG.distributions.Distributions.PNorm(Math.Abs(stat(j))))
                        Else
                            pv(j) = Global.BESHStatNG.distributions.Distributions.T_2T(Math.Abs(stat(j)), df(j))
                        End If

                        If pv(j) < 0.0 Then pv(j) = 0.0
                        If pv(j) > 1.0 Then pv(j) = 1.0
                    Else
                        stat(j) = Double.NaN
                        z(j) = Double.NaN
                        pv(j) = Double.NaN
                    End If
                Next
            End If

            res.FixedInferenceMethod = inference
            res.BetaSE = se
            res.BetaZ = z                 ' Keep populated for backward compatibility and tests.
            res.BetaStatistic = stat
            res.BetaP = pv

            If inference = MixedModelFixedInferenceMethod.WaldNormal Then
                res.BetaDF = MakeNaNVector(p)
                res.BetaStatisticLabel = "z"
                res.BetaPValueLabel = "Pr(>|z|)"
            Else
                res.BetaDF = df
                res.BetaStatisticLabel = "t"
                res.BetaPValueLabel = "Pr(>|t|)"
            End If
        End Sub

        ''' <summary>
        ''' Computes marginal fitted values, residuals, and optional subject-level BLUPs.
        ''' </summary>
        Private Sub PopulateFittedResidualsAndBLUPs(res As MixedModelResult)
            Dim n As Integer = pRequest.Data.Nobs
            Dim fitted(n - 1) As Double
            Dim residual(n - 1) As Double
            Dim useOriginalOrder As Boolean = CanUseOriginalRowOrder()
            Dim seq As Integer = 0

            Dim thetaG() As Double = res.ThetaG
            Dim thetaR() As Double = res.ThetaR
            Dim activeG As MixedModelGStruct = ActiveGStruct()
            Dim gMat(,) As Double = Nothing
            If activeG IsNot Nothing AndAlso Not activeG.IsDegenerateZeroG() AndAlso pRequest.Data.Q > 0 Then
                gMat = activeG.BuildG(thetaG, pRequest.Data.Q, pStrTrace)
            End If

            For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                ThrowIfCancellationRequested()
                Dim mean() As Double = MixedModelCovariance.ComputeMarginalMean(block, res.Beta)
                Dim resid() As Double = MixedModelCovariance.ComputeMarginalResidual(block, res.Beta)
                Dim rows() As Integer = block.RowIndices

                For i As Integer = 0 To block.Nobs - 1
                    Dim target As Integer = If(useOriginalOrder, rows(i), seq)
                    fitted(target) = mean(i)
                    residual(target) = resid(i)
                    seq += 1
                Next

                If gMat IsNot Nothing AndAlso block.HasRandomEffectsDesign() Then
                    Dim vi(,) As Double = MixedModelCovariance.BuildVi(block, pRequest.Data, activeG,
                                                                       pRequest.ResidualStruct, thetaG, thetaR, pStrTrace)
                    Dim vInv(,) As Double = MixedModelCovariance.InverseSPD(vi, pStrTrace)
                    Dim bHat() As Double = MixedModelCovariance.ComputeBLUP(block, res.Beta, gMat, vInv, pStrTrace)
                    If bHat IsNot Nothing Then res.RandomEffects(block.SubjectKey) = bHat
                End If
            Next

            res.FittedMarginal = fitted
            res.ResidualRaw = residual
        End Sub

        ''' <summary>
        ''' Populates user-scale G-side random-effects covariance and correlation matrices in the result object.
        ''' </summary>
        ''' <remarks>
        ''' The optimizer stores G-side parameters on an internal constrained scale.  For LMM output,
        ''' users need the random-effects covariance matrix itself.  This method asks the selected
        ''' random-effects structure to rebuild G at the fitted parameter values, derives the matching
        ''' correlation matrix, and stores display labels aligned with the random-effects columns.
        ''' </remarks>
        Private Sub PopulateUserScaleRandomCovariance(res As MixedModelResult)
            Try
                If res Is Nothing OrElse pRequest Is Nothing OrElse pRequest.Data Is Nothing Then Exit Sub
                If pRequest.Data.Q <= 0 Then Exit Sub

                Dim gStruct As MixedModelGStruct = ActiveGStruct()
                If gStruct Is Nothing OrElse gStruct.IsDegenerateZeroG() Then Exit Sub
                If res.ThetaG Is Nothing Then Exit Sub

                Dim gMat(,) As Double = gStruct.BuildG(res.ThetaG, pRequest.Data.Q, pStrTrace)
                If gMat Is Nothing Then Exit Sub

                res.RandomCovarianceUserScale = gMat
                res.RandomCorrelationUserScale = ConvertCovarianceToCorrelation(gMat)
                res.RandomCovarianceLabels = BuildRandomCovarianceLabels(pRequest.Data.Q)

                AppendDebug("MixedModelEngine.PopulateUserScaleRandomCovariance populated G matrix dim=" & gMat.GetLength(0).ToString() & ".")
            Catch ex As Exception
                AppendWarn("MixedModelEngine.PopulateUserScaleRandomCovariance failed: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Builds display labels for rows/columns of the fitted G random-effects matrix.
        ''' </summary>
        Private Function BuildRandomCovarianceLabels(q As Integer) As String()
            If q <= 0 Then Return Array.Empty(Of String)()

            Dim labels(q - 1) As String
            For j As Integer = 0 To q - 1
                If pRequest IsNot Nothing AndAlso pRequest.RandomEffectNames IsNot Nothing AndAlso j < pRequest.RandomEffectNames.Length AndAlso Not String.IsNullOrWhiteSpace(pRequest.RandomEffectNames(j)) Then
                    labels(j) = pRequest.RandomEffectNames(j)
                ElseIf j = 0 Then
                    labels(j) = "(Intercept)"
                Else
                    labels(j) = "Random " & CStr(j + 1)
                End If
            Next

            Return labels
        End Function

        ''' <summary>
        ''' Populates user-scale R-side covariance and correlation matrices in the result object.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Optimizer parameters are intentionally stored on an internal scale, for example log-variances,
        ''' unconstrained correlations, or Cholesky factors.  Those values are useful for debugging but not for
        ''' end users.  This method asks the selected residual covariance structure to build a representative
        ''' full-visit R matrix at the fitted theta values and stores that matrix on the result.
        ''' </para>
        ''' <para>
        ''' For MMRM/no-random-effects fits, this R matrix is the fitted marginal within-subject covariance.
        ''' For ordinary LMMs, it is only the residual R-side matrix; the marginal V also includes ZGZ'.
        ''' </para>
        ''' </remarks>
        Private Sub PopulateUserScaleResidualCovariance(res As MixedModelResult)
            Try
                If res Is Nothing OrElse pRequest Is Nothing OrElse pRequest.Data Is Nothing OrElse pRequest.ResidualStruct Is Nothing Then Exit Sub
                If res.ThetaR Is Nothing Then Exit Sub

                Dim proto As MixedModelSubjectBlock = BuildResidualCovariancePrototypeBlock()
                If proto Is Nothing Then Exit Sub

                Dim rMat(,) As Double = pRequest.ResidualStruct.BuildRi(res.ThetaR, proto, pRequest.Data)
                res.ResidualCovarianceUserScale = rMat
                res.ResidualCorrelationUserScale = ConvertCovarianceToCorrelation(rMat)
                res.ResidualCovarianceVisitLabels = BuildResidualCovarianceLabels(proto)

                AppendDebug("MixedModelEngine.PopulateUserScaleResidualCovariance populated R matrix dim=" & rMat.GetLength(0).ToString() & ".")
            Catch ex As Exception
                AppendWarn("MixedModelEngine.PopulateUserScaleResidualCovariance failed: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Builds a synthetic full-visit subject block used only to request the fitted R matrix from the R structure.
        ''' </summary>
        Private Function BuildResidualCovariancePrototypeBlock() As MixedModelSubjectBlock
            If pRequest Is Nothing OrElse pRequest.Data Is Nothing Then Return Nothing

            Dim k As Integer
            Dim visits() As Double = Nothing
            Dim visitIndex() As Integer = Nothing

            If pRequest.Data.HasVisit AndAlso pRequest.Data.UniqueVisitValues IsNot Nothing AndAlso pRequest.Data.UniqueVisitValues.Length > 0 Then
                visits = pRequest.Data.UniqueVisitValues
                k = visits.Length
            Else
                k = Math.Max(1, pRequest.Data.MaxClusterSize())
                ReDim visits(k - 1)
                For i As Integer = 0 To k - 1
                    visits(i) = CDbl(i + 1)
                Next
            End If

            ReDim visitIndex(k - 1)
            For i As Integer = 0 To k - 1
                visitIndex(i) = i
            Next

            Dim y(k - 1) As Double
            Dim p As Integer = Math.Max(1, pRequest.Data.P)
            Dim x(k - 1, p - 1) As Double
            Dim rows(k - 1) As Integer

            For i As Integer = 0 To k - 1
                rows(i) = i
                If p > 0 Then x(i, 0) = 1.0
            Next

            Return New MixedModelSubjectBlock("__R_MATRIX__", rows, y, x, Nothing, visits, visitIndex)
        End Function

        ''' <summary>
        ''' Builds display labels for rows/columns of the representative R matrix.
        ''' </summary>
        Private Function BuildResidualCovarianceLabels(proto As MixedModelSubjectBlock) As String()
            If proto Is Nothing Then Return Array.Empty(Of String)()

            Dim k As Integer = proto.Nobs
            Dim labels(k - 1) As String
            Dim visits() As Double = proto.Visit

            For i As Integer = 0 To k - 1
                If visits IsNot Nothing AndAlso i < visits.Length Then
                    labels(i) = "Visit " & Convert.ToString(visits(i), System.Globalization.CultureInfo.InvariantCulture)
                Else
                    labels(i) = "Position " & CStr(i + 1)
                End If
            Next

            Return labels
        End Function

        ''' <summary>
        ''' Converts a covariance matrix to its corresponding correlation matrix.
        ''' </summary>
        Private Shared Function ConvertCovarianceToCorrelation(cov(,) As Double) As Double(,)
            If cov Is Nothing Then Return Nothing

            Dim n As Integer = cov.GetLength(0)
            Dim m As Integer = cov.GetLength(1)
            Dim out(n - 1, m - 1) As Double

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To m - 1
                    Dim vi As Double = cov(i, i)
                    Dim vj As Double = cov(j, j)

                    If IsFinite(vi) AndAlso IsFinite(vj) AndAlso vi > 0.0 AndAlso vj > 0.0 Then
                        out(i, j) = cov(i, j) / Math.Sqrt(vi * vj)
                    Else
                        out(i, j) = Double.NaN
                    End If
                Next
            Next

            Return out
        End Function

        ''' <summary>
        ''' Computes AIC/BIC-style diagnostics from the fitted criterion.
        ''' </summary>
        Private Sub PopulateInformationCriteria(res As MixedModelResult)
            If Not IsFinite(res.LogLik) Then Exit Sub

            Dim k As Integer = res.Theta.Length
            If pRequest.FitMethod = MixedModelFitMethod.ML Then
                k += res.P
            End If

            res.AIC = -2.0 * res.LogLik + 2.0 * CDbl(k)
            If res.Nobs > 0 Then
                res.BIC = -2.0 * res.LogLik + Math.Log(CDbl(res.Nobs)) * CDbl(k)
            End If
        End Sub

        Private Function ActiveGStruct() As MixedModelGStruct
            If pRequest.RandomStruct Is Nothing Then Return Nothing
            If pRequest.Data Is Nothing OrElse pRequest.Data.Q <= 0 Then Return Nothing
            If pRequest.RandomStruct.IsDegenerateZeroG() Then Return Nothing
            Return pRequest.RandomStruct
        End Function

        Private Function GetStartThetaG(olsResidualVar As Double) As Double()
            If pRequest.StartThetaG IsNot Nothing Then Return CType(pRequest.StartThetaG.Clone(), Double())
            Dim activeG As MixedModelGStruct = ActiveGStruct()
            If activeG Is Nothing Then Return Array.Empty(Of Double)()
            Return activeG.StartParams(pRequest.Data, olsResidualVar)
        End Function

        Private Function GetStartThetaR(olsResidualVar As Double) As Double()
            If pRequest.StartThetaR IsNot Nothing Then Return CType(pRequest.StartThetaR.Clone(), Double())
            Return pRequest.ResidualStruct.StartParams(pRequest.Data, olsResidualVar)
        End Function

        Private Function GetDefaultStartThetaG(olsResidualVar As Double) As Double()
            Dim activeG As MixedModelGStruct = ActiveGStruct()
            If activeG Is Nothing Then Return Array.Empty(Of Double)()
            Return activeG.StartParams(pRequest.Data, olsResidualVar)
        End Function

        Private Function GetDefaultStartThetaR(olsResidualVar As Double) As Double()
            Return pRequest.ResidualStruct.StartParams(pRequest.Data, olsResidualVar)
        End Function

        Private Function OptimizeCovarianceParametersWithRestarts(startTheta() As Double,
                                                                  olsResidualVar As Double) As MixedModelOptimizationState
            Dim restartDiag As MixedModelRestartDiagnostics = EnsureRestartDiagnostics()
            restartDiag.Enabled = pRequest.Control.EnableStructuredRestarts
            If pPerformanceDiagnostics Is Nothing Then pPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            pPerformanceDiagnostics.SelectedCovarianceGradientMode = pRequest.Control.CovarianceGradientMode
            pPerformanceDiagnostics.SelectedCovarianceOptimizerMode = pRequest.Control.CovarianceOptimizerMode
            pPerformanceDiagnostics.ActualCovarianceOptimizerName = If(pRequest.Control.CovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml,
                                                                       MixedModelAverageInformationOptimizer.OptimizerName,
                                                                       "Projected BFGS")

            If pRequest.Control.CovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml Then
                Dim aiDiagnostics As MixedModelAverageInformationDiagnostics = Nothing
                Dim aiTrace As String = pStrTrace
                Dim aiState As MixedModelOptimizationState = MixedModelAverageInformationOptimizer.TryOptimize(pRequest,
                                                                                                               ActiveGStruct(),
                                                                                                               startTheta,
                                                                                                               Function(theta() As Double) EvaluateProfileCriterion(theta, throwOnFailure:=False, collectTrace:=False),
                                                                                                               AddressOf IsCancellationRequested,
                                                                                                               AddressOf IsInterruptionRequested,
                                                                                                               aiDiagnostics,
                                                                                                               aiTrace)
                pStrTrace = aiTrace
                RecordAverageInformationDiagnostics(aiDiagnostics)
                If aiState.Status = MixedModelOptimizationStatus.Cancelled OrElse aiState.Status = MixedModelOptimizationStatus.Interrupted Then Return aiState
                If IsUsableOptimizationState(aiState) Then Return aiState

                Dim aiFallbackMessage As String = "Average Information optimizer did not produce a usable solution; falling back to projected BFGS. " & If(aiState.Message, String.Empty)
                AppendWarn(aiFallbackMessage)
                pPerformanceDiagnostics.ActualCovarianceOptimizerName = "Projected BFGS fallback after Average Information"
                If Not pRequest.Control.FallbackToNumericalGradientOnAnalyticFailure Then Return aiState
            End If

            Dim gradientDelegate As Func(Of Double(), Double()) = Nothing
            Dim analyticGradientMessage As String = String.Empty
            If Not TryPrepareCovarianceGradientDelegate(gradientDelegate, analyticGradientMessage) Then
                If pRequest.Control.CovarianceGradientMode <> MixedModelCovarianceGradientMode.NumericalFiniteDifference AndAlso
                   Not pRequest.Control.FallbackToNumericalGradientOnAnalyticFailure Then
                    Dim failedState As MixedModelOptimizationState = CreateRestartFallbackState(startTheta)
                    failedState.Status = MixedModelOptimizationStatus.InvalidInput
                    failedState.Message = analyticGradientMessage
                    failedState.GradientProviderName = MixedModelAnalyticGradient.AnalyticProviderName
                    pPerformanceDiagnostics.ActualCovarianceGradientProviderName = MixedModelAnalyticGradient.AnalyticProviderName
                    pPerformanceDiagnostics.AnalyticGradientUsed = False
                    pPerformanceDiagnostics.AnalyticGradientFailureMessage = analyticGradientMessage
                    Return failedState
                End If
            End If

            Dim attempts As List(Of MixedModelStartAttempt) = BuildStructuredStartAttempts(startTheta, olsResidualVar)
            Dim bestState As MixedModelOptimizationState = CreateRestartFallbackState(startTheta)
            Dim bestObjective As Double = Double.PositiveInfinity

            For attemptIndex As Integer = 0 To attempts.Count - 1
                ThrowIfCancellationRequested()

                Dim attempt As MixedModelStartAttempt = attempts(attemptIndex)
                restartDiag.StartAttemptCount += 1
                If attemptIndex > 0 Then
                    ReportProgress("Restarting covariance optimization", 20, maxIterations:=pRequest.Control.MaxIter, message:=attempt.Name)
                    AppendWarn("MixedModelEngine.Fit retrying covariance optimization with structured restart: " & attempt.Name)
                End If

                If pRequest.Control.CovarianceGradientMode = MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation AndAlso
                   gradientDelegate IsNot Nothing Then
                    If Not ValidateAnalyticGradientAtTheta(attempt.Theta, attempt.Name & " start") Then
                        If pRequest.Control.FallbackToNumericalGradientOnAnalyticFailure Then
                            Dim validationFallbackMessage As String = "Analytic covariance-gradient validation failed badly at " & attempt.Name & " start; falling back to numerical finite differences for optimization."
                            MarkAnalyticGradientFallback(validationFallbackMessage)
                            gradientDelegate = Nothing
                        Else
                            Dim failedValidationState As MixedModelOptimizationState = CreateRestartFallbackState(attempt.Theta)
                            failedValidationState.Status = MixedModelOptimizationStatus.InvalidInput
                            failedValidationState.Message = "Analytic covariance-gradient validation failed badly and fallback to numerical gradients is disabled."
                            failedValidationState.GradientProviderName = MixedModelAnalyticGradient.AnalyticProviderName
                            Return failedValidationState
                        End If
                    End If
                End If

                Dim objective As Func(Of Double(), Double) = Function(theta() As Double) SafeObjective(theta)
                Dim state As MixedModelOptimizationState = MixedModelOptimizer.OptimizeProjected(attempt.Theta,
                                                                                                 objective,
                                                                                                 pRequest.Control,
                                                                                                 gradient:=gradientDelegate,
                                                                                                 lower:=Nothing,
                                                                                                 upper:=Nothing,
                                                                                                 strTrace:=pStrTrace,
                                                                                                 iterationCallback:=AddressOf ReportOptimizerProgress,
                                                                                                 cancellationRequested:=AddressOf IsCancellationRequested,
                                                                                                 interruptionRequested:=AddressOf IsInterruptionRequested)

                If pRequest.Control.CovarianceGradientMode = MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation AndAlso
                   gradientDelegate IsNot Nothing AndAlso state.Theta IsNot Nothing Then
                    ValidateAnalyticGradientAtTheta(state.Theta, attempt.Name & " final")
                End If

                If IsFinite(state.Objective) AndAlso state.Objective < bestObjective Then
                    bestObjective = state.Objective
                    bestState = state
                End If

                If state.Status = MixedModelOptimizationStatus.Cancelled OrElse
                   state.Status = MixedModelOptimizationStatus.Interrupted Then Return state

                If IsUsableOptimizationState(state) Then
                    restartDiag.SuccessfulStartAttemptName = attempt.Name
                    If attemptIndex > 0 Then
                        state.Message = "Converged after restart using " & attempt.Name & ". " & state.Message
                    End If
                    Return state
                End If

                Dim failedMessage As String = attempt.Name & ": " & state.Status.ToString() & " - " & If(state.Message, String.Empty)
                restartDiag.FailedStartAttemptMessages.Add(failedMessage)

                If Not pRequest.Control.EnableStructuredRestarts OrElse Not ShouldTryStructuredRestart(state) Then
                    Return state
                End If
            Next

            If restartDiag.FailedStartAttemptMessages.Count > 0 Then
                bestState.Message = If(String.IsNullOrWhiteSpace(bestState.Message),
                                       "Covariance optimization did not converge after structured restart attempts.",
                                       bestState.Message & " Structured restart attempts failed.")
            End If
            Return bestState
        End Function

        Private Function TryPrepareCovarianceGradientDelegate(ByRef gradientDelegate As Func(Of Double(), Double()),
                                                                  ByRef analyticGradientMessage As String) As Boolean
            gradientDelegate = Nothing
            analyticGradientMessage = String.Empty

            If pPerformanceDiagnostics Is Nothing Then pPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            pPerformanceDiagnostics.SelectedCovarianceGradientMode = pRequest.Control.CovarianceGradientMode
            pPerformanceDiagnostics.SelectedCovarianceOptimizerMode = pRequest.Control.CovarianceOptimizerMode
            pPerformanceDiagnostics.ActualCovarianceGradientProviderName = MixedModelAnalyticGradient.NumericalProviderName
            pPerformanceDiagnostics.AnalyticGradientUsed = False
            pPerformanceDiagnostics.AnalyticGradientFallbackUsed = False
            pPerformanceDiagnostics.AnalyticGradientFailureMessage = String.Empty
            pPerformanceDiagnostics.EstimatedNumericalGradientObjectiveEvaluationsAvoided = 0

            Dim effectiveGradientMode As MixedModelCovarianceGradientMode = pRequest.Control.CovarianceGradientMode
            If pRequest.Control.CovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.ProjectedBfgsAnalyticGradient AndAlso
               effectiveGradientMode = MixedModelCovarianceGradientMode.NumericalFiniteDifference Then
                effectiveGradientMode = MixedModelCovarianceGradientMode.AnalyticScore
            End If

            If effectiveGradientMode = MixedModelCovarianceGradientMode.NumericalFiniteDifference Then Return True

            Dim requestedGradient As Func(Of Double(), Double()) = Nothing
            Dim providerMessage As String = Nothing
            Dim gradientTrace As String = pStrTrace
            Dim created As Boolean = MixedModelAnalyticGradient.TryCreateGradientDelegate(pRequest,
                                                                                           ActiveGStruct(),
                                                                                           Function(theta() As Double) EvaluateProfileCriterion(theta, throwOnFailure:=False, collectTrace:=False),
                                                                                           AddressOf IsCancellationRequested,
                                                                                           requestedGradient,
                                                                                           providerMessage,
                                                                                           gradientTrace,
                                                                                           AddressOf RecordAnalyticGradientEvaluationDiagnostics)
            pStrTrace = gradientTrace

            If created AndAlso requestedGradient IsNot Nothing Then
                gradientDelegate = requestedGradient
                pPerformanceDiagnostics.ActualCovarianceGradientProviderName = MixedModelAnalyticGradient.AnalyticProviderName
                pPerformanceDiagnostics.AnalyticGradientUsed = True
                Return True
            End If

            analyticGradientMessage = If(String.IsNullOrWhiteSpace(providerMessage),
                                         "Analytic covariance gradient provider is unavailable.",
                                         providerMessage)

            If pRequest.Control.CovarianceGradientMode = MixedModelCovarianceGradientMode.Auto Then
                pPerformanceDiagnostics.AnalyticGradientUsed = False
                pPerformanceDiagnostics.AnalyticGradientFallbackUsed = False
                pPerformanceDiagnostics.ActualCovarianceGradientProviderName = MixedModelAnalyticGradient.NumericalProviderName
                pPerformanceDiagnostics.AnalyticGradientFailureMessage = String.Empty
                Return True
            End If

            pPerformanceDiagnostics.AnalyticGradientFailureMessage = analyticGradientMessage

            If effectiveGradientMode = MixedModelCovarianceGradientMode.Auto Then
                pPerformanceDiagnostics.AnalyticGradientUsed = False
                pPerformanceDiagnostics.AnalyticGradientFallbackUsed = True
                pPerformanceDiagnostics.ActualCovarianceGradientProviderName = MixedModelAnalyticGradient.NumericalProviderName
                AppendWarn(analyticGradientMessage)
                Return False
            End If

            pPerformanceDiagnostics.ActualCovarianceGradientProviderName = MixedModelAnalyticGradient.AnalyticProviderName
            pPerformanceDiagnostics.AnalyticGradientUsed = False
            AppendWarn("Analytic covariance gradient provider is unavailable and fallback to numerical gradients is disabled. " & analyticGradientMessage)
            Return False
        End Function

        Private Function ValidateAnalyticGradientAtTheta(theta() As Double,
                                                          label As String) As Boolean
            If pRequest Is Nothing OrElse pRequest.Control.CovarianceGradientMode <> MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation Then Return True
            If theta Is Nothing OrElse theta.Length = 0 Then Return True

            Dim evalResult As MixedModelAnalyticGradientEvaluation = Nothing
            Dim validationTrace As String = pStrTrace
            Dim ok As Boolean = MixedModelAnalyticGradient.TryEvaluateGradient(pRequest,
                                                                               theta,
                                                                               ActiveGStruct(),
                                                                               Function(candidate() As Double) EvaluateProfileCriterion(candidate, throwOnFailure:=False, collectTrace:=False),
                                                                               AddressOf IsCancellationRequested,
                                                                               evalResult,
                                                                               validationTrace,
                                                                               validateAgainstFiniteDifference:=True)
            pStrTrace = validationTrace
            Return RecordAnalyticGradientValidation(evalResult, label, ok)
        End Function

        Private Sub RecordAverageInformationDiagnostics(aiDiagnostics As MixedModelAverageInformationDiagnostics)
            If pPerformanceDiagnostics Is Nothing Then pPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            pPerformanceDiagnostics.SelectedCovarianceOptimizerMode = pRequest.Control.CovarianceOptimizerMode
            pPerformanceDiagnostics.ActualCovarianceOptimizerName = MixedModelAverageInformationOptimizer.OptimizerName
            pPerformanceDiagnostics.AverageInformationIterationCount = aiDiagnostics.IterationCount
            pPerformanceDiagnostics.AverageInformationStepHalvingCount = aiDiagnostics.StepHalvingCount
            pPerformanceDiagnostics.AverageInformationRidgeAdjustmentCount = aiDiagnostics.RidgeAdjustmentCount
            pPerformanceDiagnostics.AverageInformationMatrixEvaluationCount = aiDiagnostics.InformationMatrixEvaluationCount
            pPerformanceDiagnostics.AverageInformationMatrixTimeMs = aiDiagnostics.InformationMatrixTimeMs
            pPerformanceDiagnostics.ActualCovarianceGradientProviderName = MixedModelAnalyticGradient.AnalyticProviderName
            pPerformanceDiagnostics.AnalyticGradientUsed = True
            pPerformanceDiagnostics.AnalyticGradientFallbackUsed = False
        End Sub

        Private Sub RecordAnalyticGradientEvaluationDiagnostics(evalResult As MixedModelAnalyticGradientEvaluation)
            If pPerformanceDiagnostics Is Nothing Then pPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()

            pPerformanceDiagnostics.AnalyticGradientDerivativePatternCacheEnabled = pPerformanceDiagnostics.AnalyticGradientDerivativePatternCacheEnabled OrElse evalResult.AnalyticDerivativePatternCacheEnabled
            If evalResult.AnalyticDerivativePatternCount > pPerformanceDiagnostics.AnalyticGradientDerivativePatternCount Then
                pPerformanceDiagnostics.AnalyticGradientDerivativePatternCount = evalResult.AnalyticDerivativePatternCount
            End If
            pPerformanceDiagnostics.AnalyticGradientDerivativePatternCacheHits += Math.Max(0, evalResult.AnalyticDerivativePatternCacheHits)
            pPerformanceDiagnostics.AnalyticGradientDerivativePatternCacheMisses += Math.Max(0, evalResult.AnalyticDerivativePatternCacheMisses)
            pPerformanceDiagnostics.AnalyticGradientDerivativeMatricesBuilt += Math.Max(0, evalResult.AnalyticDerivativeMatricesBuilt)

            If IsFinite(evalResult.AnalyticTraceQuadraticContractionTimeMs) Then
                If Not IsFinite(pPerformanceDiagnostics.AnalyticGradientTraceQuadraticContractionTimeMs) Then
                    pPerformanceDiagnostics.AnalyticGradientTraceQuadraticContractionTimeMs = 0.0
                End If
                pPerformanceDiagnostics.AnalyticGradientTraceQuadraticContractionTimeMs += evalResult.AnalyticTraceQuadraticContractionTimeMs
            End If
        End Sub

        Private Function RecordAnalyticGradientValidation(evalResult As MixedModelAnalyticGradientEvaluation,
                                                           label As String,
                                                           succeeded As Boolean) As Boolean
            If pPerformanceDiagnostics Is Nothing Then pPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            RecordAnalyticGradientEvaluationDiagnostics(evalResult)
            pPerformanceDiagnostics.AnalyticGradientValidationEvaluationCount += 1
            Dim validationAcceptable As Boolean = succeeded

            Dim message As String = If(String.IsNullOrWhiteSpace(label), "analytic-gradient validation", label)
            If Not String.IsNullOrWhiteSpace(evalResult.Message) Then
                message &= ": " & evalResult.Message
            ElseIf Not succeeded Then
                message &= ": validation failed."
            End If

            If IsFinite(evalResult.MaxRelativeFiniteDifferenceDiscrepancy) Then
                If Not IsFinite(pPerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy) OrElse
                   evalResult.MaxRelativeFiniteDifferenceDiscrepancy > pPerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy Then
                    pPerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy = evalResult.MaxRelativeFiniteDifferenceDiscrepancy
                    pPerformanceDiagnostics.AnalyticGradientValidationFailedParameterIndex = evalResult.FailedParameterIndex
                End If

                Dim tolerance As Double = pRequest.Control.AnalyticGradientValidationTolerance
                If tolerance <= 0.0 OrElse Not IsFinite(tolerance) Then tolerance = 0.0001
                If evalResult.MaxRelativeFiniteDifferenceDiscrepancy > tolerance Then
                    AppendWarn("Analytic covariance-gradient validation discrepancy exceeded tolerance at " & If(label, String.Empty) & ": maxRelative=" & evalResult.MaxRelativeFiniteDifferenceDiscrepancy.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & ", tolerance=" & tolerance.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & ".")
                End If

                Dim severeTolerance As Double = SevereAnalyticGradientValidationTolerance(tolerance)
                If evalResult.MaxRelativeFiniteDifferenceDiscrepancy > severeTolerance Then
                    validationAcceptable = False
                    message &= " Severe validation threshold " & severeTolerance.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & " was exceeded."
                End If
            End If

            If String.IsNullOrWhiteSpace(pPerformanceDiagnostics.AnalyticGradientValidationMessage) Then
                pPerformanceDiagnostics.AnalyticGradientValidationMessage = message
            Else
                pPerformanceDiagnostics.AnalyticGradientValidationMessage &= " | " & message
            End If

            Return validationAcceptable
        End Function

        Private Function SevereAnalyticGradientValidationTolerance(tolerance As Double) As Double
            If tolerance <= 0.0 OrElse Not IsFinite(tolerance) Then tolerance = 0.0001
            Return Math.Max(0.0000001, 10.0 * tolerance)
        End Function

        Private Sub MarkAnalyticGradientFallback(message As String)
            If pPerformanceDiagnostics Is Nothing Then pPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            pPerformanceDiagnostics.AnalyticGradientUsed = False
            pPerformanceDiagnostics.AnalyticGradientFallbackUsed = True
            pPerformanceDiagnostics.ActualCovarianceGradientProviderName = MixedModelAnalyticGradient.NumericalProviderName
            If String.IsNullOrWhiteSpace(pPerformanceDiagnostics.AnalyticGradientFailureMessage) Then
                pPerformanceDiagnostics.AnalyticGradientFailureMessage = If(message, String.Empty)
            ElseIf Not String.IsNullOrWhiteSpace(message) AndAlso pPerformanceDiagnostics.AnalyticGradientFailureMessage.IndexOf(message, StringComparison.OrdinalIgnoreCase) < 0 Then
                pPerformanceDiagnostics.AnalyticGradientFailureMessage &= " " & message
            End If
            AppendWarn(message)
        End Sub

        Private Function BuildStructuredStartAttempts(startTheta() As Double,
                                                      olsResidualVar As Double) As List(Of MixedModelStartAttempt)
            Dim attempts As New List(Of MixedModelStartAttempt)()
            AddUniqueStartAttempt(attempts, "initial start", startTheta)

            If pRequest Is Nothing OrElse Not pRequest.Control.EnableStructuredRestarts Then Return attempts
            If startTheta Is Nothing OrElse startTheta.Length = 0 Then Return attempts

            Dim baseScale As Double = olsResidualVar
            If Not IsFinite(baseScale) OrElse baseScale <= 0.0 Then baseScale = 1.0

            ' First restart uses the legacy structure default, ignoring any user-supplied start vector.
            AddUniqueStartAttempt(attempts,
                                  "legacy zero-correlation default start",
                                  PackTheta(GetDefaultStartThetaG(baseScale), GetDefaultStartThetaR(baseScale)))

            ' Second restart keeps the legacy zero-correlation shape but inflates variance scale.
            AddUniqueStartAttempt(attempts,
                                  "inflated diagonal start",
                                  PackTheta(GetDefaultStartThetaG(baseScale * 4.0), GetDefaultStartThetaR(baseScale * 4.0)))

            Return attempts
        End Function

        Private Shared Sub AddUniqueStartAttempt(attempts As List(Of MixedModelStartAttempt),
                                                 name As String,
                                                 theta() As Double)
            If attempts Is Nothing Then Exit Sub
            Dim candidate() As Double = If(theta Is Nothing, Array.Empty(Of Double)(), CType(theta.Clone(), Double()))

            For Each existing As MixedModelStartAttempt In attempts
                If SameThetaSignature(existing.Theta, candidate) Then Return
            Next

            attempts.Add(New MixedModelStartAttempt With {.Name = name, .Theta = candidate})
        End Sub

        Private Shared Function SameThetaSignature(a() As Double, b() As Double) As Boolean
            If a Is Nothing Then a = Array.Empty(Of Double)()
            If b Is Nothing Then b = Array.Empty(Of Double)()
            If a.Length <> b.Length Then Return False
            For i As Integer = 0 To a.Length - 1
                If Math.Abs(a(i) - b(i)) > 0.000000000001 Then Return False
            Next
            Return True
        End Function

        Private Function EnsureRestartDiagnostics() As MixedModelRestartDiagnostics
            If pRestartDiagnostics Is Nothing Then pRestartDiagnostics = New MixedModelRestartDiagnostics()
            Return pRestartDiagnostics
        End Function

        Private Shared Function IsUsableOptimizationState(state As MixedModelOptimizationState) As Boolean
            Return state.Converged AndAlso state.Status <> MixedModelOptimizationStatus.Cancelled AndAlso
                   state.Status <> MixedModelOptimizationStatus.Interrupted AndAlso
                   Not Double.IsNaN(state.Objective) AndAlso Not Double.IsInfinity(state.Objective)
        End Function

        Private Shared Function ShouldTryStructuredRestart(state As MixedModelOptimizationState) As Boolean
            Select Case state.Status
                Case MixedModelOptimizationStatus.NonFiniteObjective,
                     MixedModelOptimizationStatus.LineSearchFailed,
                     MixedModelOptimizationStatus.InvalidInput,
                     MixedModelOptimizationStatus.IterationLimit
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Private Shared Function CreateRestartFallbackState(startTheta() As Double) As MixedModelOptimizationState
            Return New MixedModelOptimizationState With {
                .Theta = If(startTheta Is Nothing, Array.Empty(Of Double)(), CType(startTheta.Clone(), Double())),
                .Objective = Double.NaN,
                .Iterations = 0,
                .Converged = False,
                .GradNorm = Double.NaN,
                .Status = MixedModelOptimizationStatus.NotStarted,
                .Message = "Covariance optimization was not started."
            }
        End Function

        Private Function PackTheta(thetaG() As Double, thetaR() As Double) As Double()
            Dim gLen As Integer = If(thetaG Is Nothing, 0, thetaG.Length)
            Dim rLen As Integer = If(thetaR Is Nothing, 0, thetaR.Length)
            If gLen + rLen = 0 Then Return Array.Empty(Of Double)()

            Dim out(gLen + rLen - 1) As Double
            Dim k As Integer = 0
            If thetaG IsNot Nothing Then
                For i As Integer = 0 To thetaG.Length - 1
                    out(k) = thetaG(i)
                    k += 1
                Next
            End If
            If thetaR IsNot Nothing Then
                For i As Integer = 0 To thetaR.Length - 1
                    out(k) = thetaR(i)
                    k += 1
                Next
            End If
            Return out
        End Function

        Private Sub UnpackTheta(theta() As Double, ByRef thetaG() As Double, ByRef thetaR() As Double)
            If theta Is Nothing Then theta = Array.Empty(Of Double)()

            Dim gCount As Integer = 0
            Dim activeG As MixedModelGStruct = ActiveGStruct()
            If activeG IsNot Nothing Then gCount = activeG.ParamCount(pRequest.Data.Q)
            Dim rCount As Integer = pRequest.ResidualStruct.ParamCount(pRequest.Data)
            If theta.Length <> gCount + rCount Then
                Throw New ApplicationException($"MixedModelEngine.UnpackTheta length mismatch. Expected {gCount + rCount}, got {theta.Length}.")
            End If

            If gCount = 0 Then
                thetaG = Array.Empty(Of Double)()
            Else
                ReDim thetaG(gCount - 1)
                Array.Copy(theta, 0, thetaG, 0, gCount)
            End If

            If rCount = 0 Then
                thetaR = Array.Empty(Of Double)()
            Else
                ReDim thetaR(rCount - 1)
                Array.Copy(theta, gCount, thetaR, 0, rCount)
            End If
        End Sub

        Private Function GetThetaGNames() As String()
            Dim activeG As MixedModelGStruct = ActiveGStruct()
            If activeG Is Nothing Then Return Array.Empty(Of String)()
            Return activeG.ParamNames(pRequest.Data.Q, pRequest.RandomEffectNames)
        End Function

        Private Function GetThetaRNames() As String()
            If pRequest.ResidualStruct Is Nothing Then Return Array.Empty(Of String)()
            Return pRequest.ResidualStruct.ParamNames(pRequest.Data)
        End Function

        Private Function GetFixedEffectNames() As String()
            If pRequest.FixedEffectNames IsNot Nothing AndAlso pRequest.FixedEffectNames.Length = pRequest.Data.P Then
                Return CType(pRequest.FixedEffectNames.Clone(), String())
            End If

            Dim out(pRequest.Data.P - 1) As String
            For j As Integer = 0 To out.Length - 1
                out(j) = "beta" & (j + 1).ToString()
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes a simple OLS fit on the stacked fixed-effects design to obtain starting scale information.
        ''' </summary>
        Private Function EstimateOLSResidualVariance(ByRef beta() As Double) As Double
            Dim p As Integer = pRequest.Data.P
            Dim xtx(p - 1, p - 1) As Double
            Dim xty(p - 1) As Double
            Dim yty As Double = 0.0

            For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                ThrowIfCancellationRequested()
                Dim x(,) As Double = block.X
                Dim y() As Double = block.Y
                Dim xt(,) As Double = Matrix.trans(x)
                Dim blockXtx(,) As Double = Matrix.MatrixMult(xt, x)
                Dim blockXty() As Double = Matrix.MatrixVectorMultiply(xt, y)

                For r As Integer = 0 To p - 1
                    xty(r) += blockXty(r)
                    For c As Integer = 0 To p - 1
                        xtx(r, c) += blockXtx(r, c)
                    Next
                Next
                yty += Matrix.DotProduct(y, y)
            Next

            Dim iErr As Integer = 0
            Dim chol(,) As Double = Matrix.Cholesky(CType(xtx.Clone(), Double(,)), iErr, False)
            If iErr = 0 Then
                beta = Matrix.CholSolve(chol, xty)
            Else
                AppendWarn("OLS start X'X was not SPD; zero beta start will be used for residual-scale initialization.")
                ReDim beta(p - 1)
            End If

            Dim betaDot As Double = Matrix.DotProduct(beta, xty)
            Dim rss As Double = Math.Max(0.0, yty - betaDot)
            Dim df As Integer = Math.Max(1, pRequest.Data.Nobs - p)
            Dim s2 As Double = rss / CDbl(df)
            If Not IsFinite(s2) OrElse s2 <= 0.0 Then s2 = 1.0
            Return s2
        End Function

        Private Function CanUseOriginalRowOrder() As Boolean
            If pRequest.Data Is Nothing Then Return False
            Dim seen(pRequest.Data.Nobs - 1) As Boolean
            For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                ThrowIfCancellationRequested()
                For Each r As Integer In block.RowIndices
                    If r < 0 OrElse r >= pRequest.Data.Nobs Then Return False
                    If seen(r) Then Return False
                    seen(r) = True
                Next
            Next
            For i As Integer = 0 To seen.Length - 1
                If Not seen(i) Then Return False
            Next
            Return True
        End Function

        ' Inference and denominator-degree-of-freedom helpers are implemented in
        ' MixedModelEngine.Inference.vb as part of the same partial class.
        Private Function BuildVisitSupportDiagnostics() As MixedModelVisitSupportDiagnostics
            Dim d As New MixedModelVisitSupportDiagnostics()
            If pRequest Is Nothing OrElse pRequest.Data Is Nothing OrElse Not pRequest.IsMMRM() Then Return d

            d.Enabled = True
            d.CovarianceStructureName = pRequest.ResidualStructName()

            Dim visits() As Double = pRequest.Data.UniqueVisitValues
            If visits Is Nothing Then visits = Array.Empty(Of Double)()
            Array.Sort(visits)

            For Each visitValue As Double In visits
                If Not d.VisitCounts.ContainsKey(visitValue) Then d.VisitCounts(visitValue) = 0
            Next

            For i As Integer = 0 To visits.Length - 1
                For j As Integer = i + 1 To visits.Length - 1
                    Dim pairKey As String = BuildVisitPairKey(visits(i), visits(j))
                    If Not d.VisitPairCounts.ContainsKey(pairKey) Then d.VisitPairCounts(pairKey) = 0
                Next
            Next

            For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                Dim observedVisits As New List(Of Double)()
                Dim blockVisits() As Double = block.Visit

                If blockVisits IsNot Nothing AndAlso blockVisits.Length = block.Nobs Then
                    For Each v As Double In blockVisits
                        If Not observedVisits.Contains(v) Then observedVisits.Add(v)
                    Next
                Else
                    For i As Integer = 0 To block.Nobs - 1
                        Dim pseudoVisit As Double = CDbl(i + 1)
                        If Not observedVisits.Contains(pseudoVisit) Then observedVisits.Add(pseudoVisit)
                    Next
                End If

                observedVisits.Sort()

                For Each v As Double In observedVisits
                    If Not d.VisitCounts.ContainsKey(v) Then d.VisitCounts(v) = 0
                    d.VisitCounts(v) += 1
                Next

                For i As Integer = 0 To observedVisits.Count - 1
                    For j As Integer = i + 1 To observedVisits.Count - 1
                        Dim pairKey As String = BuildVisitPairKey(observedVisits(i), observedVisits(j))
                        If Not d.VisitPairCounts.ContainsKey(pairKey) Then d.VisitPairCounts(pairKey) = 0
                        d.VisitPairCounts(pairKey) += 1
                    Next
                Next
            Next

            d.MinimumVisitCount = MinimumDictionaryValue(d.VisitCounts)
            d.MinimumVisitPairCount = MinimumDictionaryValue(d.VisitPairCounts)
            d.WeakPairThreshold = WeakVisitPairThreshold(visits.Length)

            AddVisitSupportWarnings(d, visits.Length)
            Return d
        End Function

        Private Sub AddVisitSupportWarnings(d As MixedModelVisitSupportDiagnostics,
                                            visitCount As Integer)
            If d Is Nothing OrElse d.Warnings Is Nothing Then Exit Sub
            If pRequest Is Nothing OrElse pRequest.Data Is Nothing Then Exit Sub
            If Not IsUnstructuredMMRMResidual() Then Exit Sub

            Dim m As Integer = Math.Max(0, visitCount)
            Dim k As Integer = (m * (m + 1)) \ 2

            If k > 0 AndAlso pRequest.Data.NoSubjects <= 2 * k Then
                d.Warnings.Add("UN covariance warning: subject count " & pRequest.Data.NoSubjects.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                               " is <= 2 * covariance parameter count " & (2 * k).ToString(System.Globalization.CultureInfo.InvariantCulture) &
                               "; covariance estimates may be unstable.")
            End If

            Dim threshold As Integer = d.WeakPairThreshold
            If d.VisitPairCounts IsNot Nothing Then
                For Each kvp As KeyValuePair(Of String, Integer) In d.VisitPairCounts
                    If kvp.Value < threshold Then
                        d.Warnings.Add("UN covariance warning: visit pair " & kvp.Key & " has only " &
                                       kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                                       " complete subject pairs; covariance estimate may be unstable.")
                    End If
                Next
            End If
        End Sub

        Private Sub AppendVisitSupportWarnings(res As MixedModelResult)
            If res Is Nothing OrElse res.VisitSupportDiagnostics Is Nothing Then Exit Sub
            If res.VisitSupportDiagnostics.Warnings Is Nothing Then Exit Sub

            For Each warning As String In res.VisitSupportDiagnostics.Warnings
                If Not String.IsNullOrWhiteSpace(warning) Then res.AddUserWarning(warning)
            Next
        End Sub

        Private Function WeakVisitPairThreshold(visitCount As Integer) As Integer
            Dim configured As Integer = If(pRequest Is Nothing, 5, pRequest.Control.WeakSupportMinimumPairCount)
            If configured < 1 Then configured = 5

            If visitCount <= 1 Then Return configured
            Dim k As Integer = (visitCount * (visitCount + 1)) \ 2
            Return Math.Max(configured, CInt(Math.Ceiling(CDbl(k) / 2.0)))
        End Function

        Private Shared Function BuildVisitPairKey(a As Double, b As Double) As String
            Dim lo As Double = Math.Min(a, b)
            Dim hi As Double = Math.Max(a, b)
            Return lo.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & "-" &
                   hi.ToString("G17", System.Globalization.CultureInfo.InvariantCulture)
        End Function

        Private Shared Function MinimumDictionaryValue(Of TKey)(dict As Dictionary(Of TKey, Integer)) As Integer
            If dict Is Nothing OrElse dict.Count = 0 Then Return 0
            Dim minValue As Integer = Integer.MaxValue
            For Each value As Integer In dict.Values
                If value < minValue Then minValue = value
            Next
            If minValue = Integer.MaxValue Then Return 0
            Return minValue
        End Function

        Private Function ShouldUseObjectiveVisitPatternCache() As Boolean
            If pRequest Is Nothing OrElse pRequest.Data Is Nothing Then Return False
            If Not pRequest.IsMMRM() Then Return False
            If pRequest.ResidualStruct Is Nothing Then Return False

            ' Safe first implementation: the cache is local to one profiled objective evaluation,
            ' so it is valid at the current theta without including theta in the key.
            Return True
        End Function

        Private Function EnsureObjectivePatternCacheDiagnostics() As MixedModelObjectivePatternCacheDiagnostics
            If pPerformanceDiagnostics Is Nothing Then pPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            If pPerformanceDiagnostics.ObjectivePatternCache Is Nothing Then
                pPerformanceDiagnostics.ObjectivePatternCache = New MixedModelObjectivePatternCacheDiagnostics()
            End If
            Return pPerformanceDiagnostics.ObjectivePatternCache
        End Function

        Private Shared Sub CopyOptimizerDiagnosticsToPerformance(optState As MixedModelOptimizationState,
                                                                diagnostics As MixedModelPerformanceDiagnostics)
            If diagnostics Is Nothing Then Exit Sub

            diagnostics.ObjectiveEvaluationCount = optState.ObjectiveEvaluationCount
            diagnostics.GradientEvaluationCount = optState.GradientEvaluationCount
            diagnostics.NumericalGradientObjectiveEvaluationCount = optState.NumericalGradientObjectiveEvaluationCount
            diagnostics.LineSearchEvaluationCount = optState.LineSearchEvaluationCount
            diagnostics.BfgsResetCount = optState.BfgsResetCount
            diagnostics.GradientProviderName = If(optState.GradientProviderName, String.Empty)
            If String.IsNullOrWhiteSpace(diagnostics.ActualCovarianceOptimizerName) Then
                diagnostics.ActualCovarianceOptimizerName = diagnostics.GradientProviderName
            End If
            If String.IsNullOrWhiteSpace(diagnostics.ActualCovarianceGradientProviderName) Then
                diagnostics.ActualCovarianceGradientProviderName = diagnostics.GradientProviderName
            End If
            diagnostics.AnalyticGradientUsed = String.Equals(diagnostics.ActualCovarianceGradientProviderName,
                                                             MixedModelAnalyticGradient.AnalyticProviderName,
                                                             StringComparison.OrdinalIgnoreCase) AndAlso
                                                            (String.Equals(diagnostics.GradientProviderName,
                                                            "Caller-supplied gradient",
                                                            StringComparison.OrdinalIgnoreCase) OrElse
                                                            String.Equals(diagnostics.GradientProviderName,
                                                            MixedModelAverageInformationOptimizer.OptimizerName,
                                                            StringComparison.OrdinalIgnoreCase))
            diagnostics.EstimatedNumericalGradientObjectiveEvaluationsAvoided = EstimateNumericalGradientObjectiveEvaluationsAvoided(optState)
        End Sub

        Private Shared Function EstimateNumericalGradientObjectiveEvaluationsAvoided(optState As MixedModelOptimizationState) As Long
            If optState.Theta Is Nothing OrElse optState.Theta.Length = 0 Then Return 0
            If optState.GradientEvaluationCount <= 0 Then Return 0
            If String.Equals(If(optState.GradientProviderName, String.Empty), "Numerical finite difference", StringComparison.OrdinalIgnoreCase) Then Return 0

            Dim expectedCentralDifferenceCalls As Long = CLng(2 * optState.Theta.Length) * CLng(optState.GradientEvaluationCount)
            Dim observedNumericalCalls As Long = CLng(Math.Max(0, optState.NumericalGradientObjectiveEvaluationCount))
            Return Math.Max(0L, expectedCentralDifferenceCalls - observedNumericalCalls)
        End Function

        Private Shared Function BuildObjectiveVisitPatternKey(block As MixedModelSubjectBlock) As String
            If block Is Nothing Then Return String.Empty

            Dim visitIndex() As Integer = block.VisitIndex
            If visitIndex IsNot Nothing AndAlso visitIndex.Length = block.Nobs Then
                Dim parts(visitIndex.Length - 1) As String
                For i As Integer = 0 To visitIndex.Length - 1
                    parts(i) = visitIndex(i).ToString(System.Globalization.CultureInfo.InvariantCulture)
                Next
                Return "idx:" & String.Join("|", parts)
            End If

            Dim visit() As Double = block.Visit
            If visit IsNot Nothing AndAlso visit.Length = block.Nobs Then
                Dim parts(visit.Length - 1) As String
                For i As Integer = 0 To visit.Length - 1
                    parts(i) = visit(i).ToString("G17", System.Globalization.CultureInfo.InvariantCulture)
                Next
                Return "visit:" & String.Join("|", parts)
            End If

            ' No visit metadata: cache by within-subject row order and block dimension only.
            ' This matches the residual-structure fallback that uses sequential pseudo-visits.
            Return "seq:n=" & block.Nobs.ToString(System.Globalization.CultureInfo.InvariantCulture)
        End Function

        Private Sub ReportProgress(stage As String,
                           percent As Integer,
                           Optional iteration As Integer = -1,
                           Optional maxIterations As Integer = -1,
                           Optional objective As Double = Double.NaN,
                           Optional functionChange As Double = Double.NaN,
                           Optional gradNorm As Double = Double.NaN,
                           Optional stepNorm As Double = Double.NaN,
                           Optional message As String = "")
            Try
                If pRequest Is Nothing Then Exit Sub

                Dim reporter As Action(Of MixedModelProgressInfo) = pRequest.ProgressReporter
                If reporter Is Nothing Then Exit Sub

                If percent < 0 Then percent = 0
                If percent > 100 Then percent = 100

                Dim info As New MixedModelProgressInfo With {
                        .Stage = If(stage, String.Empty),
                        .Message = If(message, String.Empty),
                        .Percent = percent,
                        .Iteration = iteration,
                        .MaxIterations = maxIterations,
                        .Objective = objective,
                        .FunctionChange = functionChange,
                        .GradNorm = gradNorm,
                        .StepNorm = stepNorm,
                        .ElapsedTimeMs = If(pFitStopwatch Is Nothing, Double.NaN, pFitStopwatch.Elapsed.TotalMilliseconds)
                    }

                reporter.Invoke(info)

            Catch
                ' Progress reporting must never interrupt fitting.
            End Try
        End Sub

        Private Sub ReportOptimizerProgress(state As MixedModelOptimizationState)
            Dim maxIter As Integer = If(pRequest Is Nothing, 100, Math.Max(1, pRequest.Control.MaxIter))
            Dim pct As Integer = 20 + CInt(Math.Min(70.0, 70.0 * CDbl(Math.Max(0, state.Iterations)) / CDbl(maxIter)))

            ReportProgress(stage:="Optimizing covariance parameters",
                   percent:=pct,
                   iteration:=state.Iterations,
                   maxIterations:=maxIter,
                   objective:=state.Objective,
                   functionChange:=state.FunctionChange,
                   gradNorm:=state.GradNorm,
                   stepNorm:=state.StepNorm,
                   message:=state.Message)
        End Sub

        Private Function IsCancellationRequested() As Boolean
            If pRequest Is Nothing OrElse pRequest.CancellationRequested Is Nothing Then Return False

            Try
                Return pRequest.CancellationRequested.Invoke()
            Catch
                Return False
            End Try
        End Function

        Private Sub ThrowIfCancellationRequested()
            If IsCancellationRequested() Then Throw New OperationCanceledException("MMRM calculation cancelled by user.")
        End Sub

        Private Function BuildCancelledResult(optState As Nullable(Of MixedModelOptimizationState),
                                              startedUtc As DateTime,
                                              elapsedMs As Double,
                                              message As String) As MixedModelResult
            Dim res As New MixedModelResult
            res.PerformanceDiagnostics = pPerformanceDiagnostics
            res.RestartDiagnostics = If(pRestartDiagnostics Is Nothing, New MixedModelRestartDiagnostics(), pRestartDiagnostics.Clone())
            res.VisitSupportDiagnostics = BuildVisitSupportDiagnostics()
            AppendVisitSupportWarnings(res)
            res.Cancelled = True
            res.Converged = False
            res.Message = If(String.IsNullOrWhiteSpace(message), "MMRM calculation cancelled by user.", message)
            If pRequest IsNot Nothing Then
                res.FitMethod = pRequest.FitMethod
                If pRequest.Data IsNot Nothing Then
                    res.Nobs = pRequest.Data.Nobs
                    res.NoSubjects = pRequest.Data.NoSubjects
                    res.P = pRequest.Data.P
                    res.Q = pRequest.Data.Q
                End If
                res.ControlMaxIter = pRequest.Control.MaxIter
                res.ControlEpsilon = pRequest.Control.Epsilon
                res.ControlStepTolerance = pRequest.Control.StepTolerance
                res.ControlFunctionTolerance = pRequest.Control.FunctionTolerance
                res.ControlUseBfgsCovarianceOptimization = pRequest.Control.UseBfgsCovarianceOptimization
                res.ControlCovarianceGradientMode = pRequest.Control.CovarianceGradientMode
                res.ControlCovarianceOptimizerMode = pRequest.Control.CovarianceOptimizerMode
                res.ControlAnalyticGradientValidationTolerance = pRequest.Control.AnalyticGradientValidationTolerance
                res.ControlFallbackToNumericalGradientOnAnalyticFailure = pRequest.Control.FallbackToNumericalGradientOnAnalyticFailure
                res.ControlUseKrPqrDesignPatternCache = pRequest.Control.UseKrPqrDesignPatternCache
                res.ControlUseKrPqrFastFactorization = pRequest.Control.UseKrPqrFastFactorization
                res.ControlUseAnalyticGradientDerivativePatternCache = pRequest.Control.UseAnalyticGradientDerivativePatternCache
                If pRequest.Data IsNot Nothing Then res.FixedEffectNames = GetFixedEffectNames()
            End If

            If optState.HasValue Then
                Dim st As MixedModelOptimizationState = optState.Value
                res.Iterations = st.Iterations
                res.GradNorm = st.GradNorm
                res.Objective = st.Objective
                res.Theta = If(st.Theta Is Nothing, Array.Empty(Of Double)(), CType(st.Theta.Clone(), Double()))
                If res.PerformanceDiagnostics Is Nothing Then res.PerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
                CopyOptimizerDiagnosticsToPerformance(st, res.PerformanceDiagnostics)
            End If

            res.ExecutionStartedUtc = startedUtc
            res.ExecutionCompletedUtc = DateTime.UtcNow
            res.ExecutionTimeMs = elapsedMs
            If res.PerformanceDiagnostics Is Nothing Then res.PerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            res.PerformanceDiagnostics.TotalFitTimeMs = elapsedMs
            res.strTrace = pStrTrace
            Return res
        End Function

        Private Function IsInterruptionRequested() As Boolean
            If pRequest Is Nothing OrElse pRequest.InterruptionRequested Is Nothing Then Return False

            Try
                Return pRequest.InterruptionRequested.Invoke()
            Catch
                Return False
            End Try
        End Function

        Private Sub MarkInterruptedResult(res As MixedModelResult)
            If res Is Nothing Then Exit Sub
            res.Interrupted = True
            res.Cancelled = False
            res.Converged = False
            If String.IsNullOrWhiteSpace(res.Message) Then
                res.Message = "MMRM calculation interrupted by user; latest accepted covariance-parameter iterate returned."
            ElseIf res.Message.IndexOf("interrupt", StringComparison.OrdinalIgnoreCase) < 0 Then
                res.Message = "MMRM calculation interrupted by user; latest accepted covariance-parameter iterate returned. " & res.Message
            End If
            res.AddUserWarning("MMRM calculation was interrupted by the user; current estimates are based on the latest accepted optimizer iterate and should be treated as provisional.")
        End Sub

        Private Sub AppendInfo(message As String)
            AppendLogCore("INFO", message)
            CoreServices.Logger.Info(message)
        End Sub

        Private Sub AppendWarn(message As String)
            AppendLogCore("WARN", message)
            CoreServices.Logger.Warn(message)
        End Sub

        Private Sub AppendDebug(message As String)
            AppendLogCore("DEBUG", message)
            CoreServices.Logger.Debug(message)
        End Sub

        Private Sub AppendTrace(message As String)
            AppendLogCore("TRACE", message)
            CoreServices.Logger.Trace(message)
        End Sub

        Private Sub AppendLogCore(level As String, message As String)
            Dim line As String = Date.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") & "|" & level & "|MixedModelEngine|" & If(message, String.Empty)
            If String.IsNullOrEmpty(pStrTrace) Then
                pStrTrace = line
            Else
                pStrTrace &= vbNewLine & line
            End If
        End Sub

        Private Function MergeTraces(a As String, b As String) As String
            If String.IsNullOrEmpty(a) Then Return If(b, String.Empty)
            If String.IsNullOrEmpty(b) Then Return a
            If a.Contains(b) Then Return a
            Return a & vbNewLine & b
        End Function

    End Class

End Namespace
