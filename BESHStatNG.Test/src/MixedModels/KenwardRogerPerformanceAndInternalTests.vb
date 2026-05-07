Option Explicit On
Option Infer On
Option Strict Off

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression

' -----------------------------------------------------------------------------
' Mixed-model Kenward-Roger tests.
' The former single large KenwardRogerInferenceTests.vb file is split into a
' few focused files to make future maintenance and batch patches easier.
' -----------------------------------------------------------------------------


' -----------------------------------------------------------------------------
' Split from KenwardRogerInferenceTests.vb for maintainability.
' -----------------------------------------------------------------------------

' ===== BEGIN KR performance and numerical hardening tests =====

<TestClass()>
Public Class MixedModelKenwardRogerPerformanceAndNumericalHardeningTests

    <TestMethod()>
    Public Sub NumericalDiagnostics_RankDeficientSymmetricMatrix_UsesSvdPseudoInverse()
        Dim a(1, 1) As Double
        a(0, 0) = 1.0
        a(0, 1) = 1.0
        a(1, 0) = 1.0
        a(1, 1) = 1.0

        Dim inv(,) As Double = Nothing
        Dim msg As String = Nothing
        Dim detail As New MixedModelNumericalInverseResult()

        Assert.IsTrue(MixedModelNumericalDiagnostics.TryInvertSymmetric(a,
                                                                        inv,
                                                                        msg,
                                                                        allowPseudoInverse:=True,
                                                                        inverseResult:=detail), msg)

        Assert.IsNotNull(inv)
        Assert.IsTrue(detail.UsedPseudoInverse, "Rank-deficient symmetric matrix should use SVD pseudoinverse fallback.")
        Assert.AreEqual(1, detail.Rank)
        Assert.IsTrue(Matrix.MatrixIsFinite(inv))
    End Sub

    <TestMethod()>
    Public Sub KrDfScaling_RepeatedContrast_UsesWorkspaceCache()
        Dim res As MixedModelResult = BuildTwoParameterCacheResult()

        Dim l(1, 1) As Double
        l(0, 0) = 1.0
        l(1, 1) = 1.0

        Dim first As MixedModelKenwardRogerDfResult = Nothing
        Dim firstMsg As String = Nothing
        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrDegreesOfFreedomAndScaling(res, l, first, firstMsg), firstMsg)

        Dim second As MixedModelKenwardRogerDfResult = Nothing
        Dim secondMsg As String = Nothing
        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrDegreesOfFreedomAndScaling(res, l, second, secondMsg), secondMsg)

        Assert.IsNotNull(res.KenwardRogerWorkspace.DfScalingCache)
        Assert.AreEqual(1, res.KenwardRogerWorkspace.DfScalingCache.Count)
        Assert.IsTrue(secondMsg.IndexOf("cached", StringComparison.OrdinalIgnoreCase) >= 0,
                      "Second call should report cached KR DF/scaling trace products.")
        Assert.AreEqual(first.DenDF, second.DenDF, 0.000000000001)
        Assert.AreEqual(first.Lambda, second.Lambda, 0.000000000001)
    End Sub

    <TestMethod()>
    Public Sub MMRM_PerformanceDiagnostics_ArePopulatedForKRFit()
        Dim res As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=24,
                                                         visitCount:=4,
                                                         residualStruct:=New DiagonalHeterogeneousR(),
                                                         maxIter:=50)

        Assert.IsNotNull(res, "KR fit should return a result.")
        Assert.IsNotNull(res.PerformanceDiagnostics, "Performance diagnostics should be initialized.")

        Dim d As MixedModelPerformanceDiagnostics = res.PerformanceDiagnostics
        AssertFiniteNonNegativeTiming(d.TotalFitTimeMs, "Total fit time ms")
        AssertFiniteNonNegativeTiming(d.StartingValuesTimeMs, "Starting values time ms")
        AssertFiniteNonNegativeTiming(d.OptimizationTimeMs, "Optimization time ms")
        AssertFiniteNonNegativeTiming(d.FinalEvaluationTimeMs, "Final evaluation time ms")
        AssertFiniteNonNegativeTiming(d.KrWorkspaceBuildTimeMs, "KR workspace build time ms")
        AssertFiniteNonNegativeTiming(d.KrDerivativeBlockTimeMs, "KR derivative blocks time ms")
        AssertFiniteNonNegativeTiming(d.KrPqrMatrixTimeMs, "KR P/Q/R matrices time ms")
        AssertFiniteNonNegativeTiming(d.KrAdjustedVarBetaTimeMs, "KR adjusted Var(beta) time ms")
        Assert.AreEqual(MixedModelCovarianceGradientMode.Auto,
                        d.SelectedCovarianceGradientMode,
                        "Default covariance optimization should report Auto covariance-gradient mode.")
        Assert.AreEqual(MixedModelCovarianceOptimizerMode.AverageInformationReml,
                        d.SelectedCovarianceOptimizerMode,
                        "Default covariance optimization should report the Average Information optimizer mode.")
        Assert.IsTrue(IsAverageInformationOrItsFallback(d.ActualCovarianceOptimizerName),
                      "Default covariance optimization should attempt the Average Information / Fisher-scoring optimizer and may report the projected-BFGS fallback when safeguards require it. Actual=" &
                      If(d.ActualCovarianceOptimizerName, String.Empty))
        Assert.IsTrue(String.Equals("Average Information REML", d.GradientProviderName, StringComparison.Ordinal) OrElse
                      String.Equals("Caller-supplied gradient", d.GradientProviderName, StringComparison.Ordinal),
                      "Default covariance optimization should report either the Average Information provider or the analytic projected-BFGS fallback provider. Actual=" &
                      If(d.GradientProviderName, String.Empty))
        Assert.AreEqual("Analytic score", d.ActualCovarianceGradientProviderName,
                        "Default Average Information covariance optimization should report the analytic score provider for supported MMRM structures.")
        Assert.IsTrue(d.AnalyticGradientUsed,
                      "Default Auto covariance optimization should report analytic-gradient use for supported MMRM structures.")
        Assert.IsFalse(d.AnalyticGradientFallbackUsed,
                       "Default Average Information covariance optimization for supported MMRM structures should not be recorded as fallback.")
        Assert.IsTrue(d.AverageInformationMatrixEvaluationCount > 0,
                       "Default Average Information covariance optimization should report information-matrix evaluations.")
        Assert.IsTrue(d.ObjectiveEvaluationCount > 0,
                      "Optimizer objective-evaluation counter should be populated.")
        Assert.IsTrue(d.GradientEvaluationCount > 0,
                      "Optimizer gradient-evaluation counter should be populated.")
        Assert.AreEqual(0,
                        d.NumericalGradientObjectiveEvaluationCount,
                        "Default Average Information analytic gradients should avoid optimizer finite-difference objective calls for supported MMRM structures.")
        Assert.IsTrue(d.EstimatedNumericalGradientObjectiveEvaluationsAvoided > 0,
                      "Default Average Information analytic gradients should report avoided numerical-gradient objective evaluations.")
        Assert.IsTrue(d.LineSearchEvaluationCount > 0,
                      "Optimizer line-search evaluation counter should be populated.")
        Assert.IsTrue(d.BfgsResetCount >= 0,
                      "Optimizer BFGS reset counter should be non-negative.")

        Dim wrapped As List(Of ResultTable) = res.wrapResults(includeKenwardRogerTermTests:=True)
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Performance diagnostics"),
                      "wrapResults should include a Performance diagnostics table.")
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Optimization time ms"),
                      "Performance diagnostics table should include optimization timing.")
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "KR derivative blocks time ms"),
                      "Performance diagnostics table should include KR derivative block timing.")
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Selected covariance optimizer mode"),
                      "Performance diagnostics table should include the selected covariance optimizer mode.")
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Optimizer gradient provider"),
                      "Performance diagnostics table should include the optimizer gradient provider.")
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Optimizer numerical-gradient objective evaluations"),
                      "Performance diagnostics table should include numerical-gradient objective-call counts.")
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Optimizer line-search evaluations"),
                      "Performance diagnostics table should include line-search objective-call counts.")
        AssertFiniteNonNegativeTiming(res.PerformanceDiagnostics.ResultWrapTimeMs, "Result wrapping time ms")
    End Sub

    <TestMethod()>
    Public Sub MixedModelControl_DefaultCovarianceGradientMode_IsAuto()
        Dim control As MixedModelControl = MixedModelControl.CreateDefault()

        Assert.AreEqual(MixedModelCovarianceGradientMode.Auto,
                        control.CovarianceGradientMode,
                        "Default covariance-gradient mode should automatically use analytic gradients for validated structures and finite differences otherwise.")
        Assert.AreEqual(MixedModelCovarianceOptimizerMode.AverageInformationReml,
                        control.CovarianceOptimizerMode,
                        "Default covariance optimizer should use the SAS PROC MIXED-style Average Information / Fisher-scoring REML path.")
        Assert.IsTrue(control.FallbackToNumericalGradientOnAnalyticFailure,
                      "Analytic-gradient fallback should be enabled by default for safe future opt-in modes.")
        Assert.IsTrue(control.AnalyticGradientValidationTolerance > 0.0,
                      "Analytic-gradient validation tolerance should have a positive default.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_DefaultAutoCovarianceGradientMode_UsesAverageInformationForSupportedStructure()
        Dim res As MixedModelResult = FitSyntheticMmrmForObjectivePatternCache(subjectCount:=18,
                                                                               visitCount:=4,
                                                                               incompletePatterns:=False)

        Assert.IsNotNull(res, "Default MMRM fit should return a result.")
        Assert.AreEqual(MixedModelCovarianceGradientMode.Auto,
                        res.ControlCovarianceGradientMode,
                        "Result should store the selected default Auto gradient mode.")
        Assert.IsNotNull(res.PerformanceDiagnostics, "Performance diagnostics should be initialized.")
        Assert.AreEqual(MixedModelCovarianceGradientMode.Auto,
                        res.PerformanceDiagnostics.SelectedCovarianceGradientMode,
                        "Performance diagnostics should store the selected default Auto gradient mode.")
        Assert.AreEqual(MixedModelCovarianceOptimizerMode.AverageInformationReml,
                        res.PerformanceDiagnostics.SelectedCovarianceOptimizerMode,
                        "Default Auto mode should use the Average Information optimizer for supported REML MMRM structures.")
        Assert.IsTrue(IsAverageInformationOrItsFallback(res.PerformanceDiagnostics.ActualCovarianceOptimizerName),
                      "Default Auto mode should identify Average Information / Fisher scoring, or the projected-BFGS fallback after Average Information safeguards. Actual=" &
                      If(res.PerformanceDiagnostics.ActualCovarianceOptimizerName, String.Empty))
        Assert.IsTrue(String.Equals("Average Information REML", res.PerformanceDiagnostics.GradientProviderName, StringComparison.Ordinal) OrElse
                      String.Equals("Caller-supplied gradient", res.PerformanceDiagnostics.GradientProviderName, StringComparison.Ordinal),
                      "Default Auto mode should report either the Average Information provider or the analytic projected-BFGS fallback provider. Actual=" &
                      If(res.PerformanceDiagnostics.GradientProviderName, String.Empty))
        'Assert.AreEqual("Analytic score",
        '                res.PerformanceDiagnostics.GradientProviderName,
        '               "Default Auto mode should report the Average Information gradient/information provider for supported MMRM structures.")
        Assert.AreEqual("Analytic score",
                        res.PerformanceDiagnostics.ActualCovarianceGradientProviderName,
                        "Default Auto mode should identify the analytic score provider for supported MMRM structures.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientUsed,
                      "Default Auto mode should report analytic-gradient use for supported MMRM structures.")
        Assert.IsFalse(res.PerformanceDiagnostics.AnalyticGradientFallbackUsed,
                       "Default Auto selection for a supported structure should not be recorded as fallback.")
        Assert.IsTrue(res.PerformanceDiagnostics.AverageInformationMatrixEvaluationCount > 0,
                      "Default Auto mode should report Average Information matrix evaluations for supported REML MMRM structures.")
        Assert.AreEqual(0,
                        res.PerformanceDiagnostics.NumericalGradientObjectiveEvaluationCount,
                        "Default Auto analytic gradients should avoid optimizer finite-difference objective calls for supported MMRM structures.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_AnalyticScoreMode_P3UsesAnalyticProviderAndReportsDiagnostics()
        Dim res As MixedModelResult = FitSyntheticMmrmForObjectivePatternCache(subjectCount:=18,
                                                                               visitCount:=4,
                                                                               incompletePatterns:=False,
                                                                               covarianceGradientMode:=MixedModelCovarianceGradientMode.AnalyticScore,
                                                                               fallbackToNumericalGradientOnAnalyticFailure:=True,
                                                                               covarianceOptimizerMode:=MixedModelCovarianceOptimizerMode.ProjectedBfgs)

        Assert.IsNotNull(res, "Analytic-score opt-in should return a result for supported R-side-only MMRM fits in P3.")
        Assert.AreEqual(MixedModelCovarianceGradientMode.AnalyticScore,
                        res.ControlCovarianceGradientMode,
                        "Result should store the requested analytic covariance-gradient mode.")
        Assert.IsNotNull(res.PerformanceDiagnostics, "Performance diagnostics should be initialized.")
        Assert.AreEqual(MixedModelCovarianceGradientMode.AnalyticScore,
                        res.PerformanceDiagnostics.SelectedCovarianceGradientMode,
                        "Performance diagnostics should store the requested analytic covariance-gradient mode.")
        Assert.AreEqual("Caller-supplied gradient",
                        res.PerformanceDiagnostics.GradientProviderName,
                        "Supported P3 analytic score mode should supply a gradient delegate to the optimizer.")
        Assert.AreEqual("Analytic score",
                        res.PerformanceDiagnostics.ActualCovarianceGradientProviderName,
                        "Actual provider should identify the analytic score path.")
        Assert.IsFalse(res.PerformanceDiagnostics.AnalyticGradientFallbackUsed,
                       "Supported R-side-only MMRM analytic mode should not use fallback.")
        Assert.AreEqual(0,
                        res.PerformanceDiagnostics.NumericalGradientObjectiveEvaluationCount,
                        "Analytic score mode should not spend optimizer evaluations inside numerical-gradient objective differences.")

        Dim wrapped As List(Of ResultTable) = res.wrapResults()
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Selected covariance gradient mode"),
                      "Performance diagnostics table should include the selected gradient mode.")
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Analytic gradient fallback used"),
                      "Performance diagnostics table should include the analytic fallback flag.")
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Covariance gradient mode"),
                      "Convergence diagnostics should display the requested control mode.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_ObjectiveVisitPatternCache_ReusesCompleteVisitPattern()
        Dim res As MixedModelResult = FitSyntheticMmrmForObjectivePatternCache(subjectCount:=30,
                                                                               visitCount:=4,
                                                                               incompletePatterns:=False)

        Assert.IsNotNull(res, "MMRM fit should return a result.")
        Assert.IsNotNull(res.PerformanceDiagnostics, "Performance diagnostics should be initialized.")
        Assert.IsNotNull(res.PerformanceDiagnostics.ObjectivePatternCache, "Objective pattern cache diagnostics should be initialized.")

        Dim cache As MixedModelObjectivePatternCacheDiagnostics = res.PerformanceDiagnostics.ObjectivePatternCache
        Assert.IsTrue(cache.Enabled, "Objective visit-pattern cache should be enabled for MMRM fits.")
        Assert.AreEqual(1, cache.PatternCount, "Complete repeated-visit data should have one observed visit pattern.")
        Assert.IsTrue(cache.Hits > 0, "Complete repeated-visit data should reuse cached covariance blocks.")
        Assert.IsTrue(cache.Misses > 0, "At least one cache miss should build the first pattern in each objective evaluation.")
        Assert.AreEqual(0, cache.InvalidBuilds, "Valid synthetic data should not produce invalid cached covariance builds.")

        Dim wrapped As List(Of ResultTable) = res.wrapResults()
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Objective pattern cache hits"),
                      "Performance diagnostics table should include objective cache counters.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_ObjectiveVisitPatternCache_TracksMultipleVisitPatterns()
        Dim res As MixedModelResult = FitSyntheticMmrmForObjectivePatternCache(subjectCount:=36,
                                                                               visitCount:=4,
                                                                               incompletePatterns:=True)

        Assert.IsNotNull(res, "MMRM fit should return a result.")
        Assert.IsNotNull(res.PerformanceDiagnostics, "Performance diagnostics should be initialized.")
        Assert.IsNotNull(res.PerformanceDiagnostics.ObjectivePatternCache, "Objective pattern cache diagnostics should be initialized.")

        Dim cache As MixedModelObjectivePatternCacheDiagnostics = res.PerformanceDiagnostics.ObjectivePatternCache
        Assert.IsTrue(cache.Enabled, "Objective visit-pattern cache should be enabled for MMRM fits.")
        Assert.IsTrue(cache.PatternCount >= 2, "Incomplete repeated-visit data should track multiple observed visit patterns.")
        Assert.IsTrue(cache.Hits > 0, "Repeated incomplete patterns should reuse cached covariance blocks.")
        Assert.IsTrue(cache.Misses > 0, "At least one miss per pattern should occur in each objective evaluation.")
        Assert.AreEqual(0, cache.InvalidBuilds, "Valid synthetic data should not produce invalid cached covariance builds.")
    End Sub


    <TestMethod()>
    Public Sub MMRM_CancellationRequested_ReturnsCancelledResult()
        Dim res As MixedModelResult = FitSyntheticMmrmForCancellationDiagnostics(subjectCount:=12,
                                                                                  visitCount:=3,
                                                                                  residualStruct:=New DiagonalHeterogeneousR(),
                                                                                  maxIter:=20,
                                                                                  cancelImmediately:=True)

        Assert.IsNotNull(res, "Cancelled fit should return a result object.")
        Assert.IsTrue(res.Cancelled, "Cancellation callback should produce a cancelled result.")
        Assert.IsFalse(res.Converged, "Cancelled fit must not report convergence.")
        Assert.IsTrue(res.Message.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0,
                      "Cancelled result should include a clear cancellation message.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_InterruptionRequested_ReturnsLatestIterateResult()
        Dim res As MixedModelResult = FitSyntheticMmrmForCancellationDiagnostics(subjectCount:=12,
                                                                                  visitCount:=3,
                                                                                  residualStruct:=New DiagonalHeterogeneousR(),
                                                                                  maxIter:=20,
                                                                                  cancelImmediately:=False,
                                                                                  interruptImmediately:=True)

        Assert.IsNotNull(res, "Interrupted fit should return a result object.")
        Assert.IsFalse(res.Cancelled, "Interrupt should not mark the result as cancelled.")
        Assert.IsTrue(res.Interrupted, "Interruption callback should mark the result as interrupted.")
        Assert.IsFalse(res.Converged, "Interrupted fit must not report convergence.")
        Assert.IsNotNull(res.Beta, "Interrupted fit should still return fixed-effect estimates from the latest iterate.")
        Assert.AreEqual(3, res.Beta.Length, "Interrupted fit should preserve the fixed-effect coefficient vector.")
        Assert.IsTrue(res.Message.IndexOf("interrupt", StringComparison.OrdinalIgnoreCase) >= 0,
                      "Interrupted result should include a clear interruption message.")
    End Sub

    <TestMethod()>
    Public Sub MixedModelOptimizer_BfgsDirection_ConvergesQuadraticQuickly()
        Dim control As MixedModelControl = MixedModelControl.CreateDefault()
        control.MaxIter = 25
        control.Epsilon = 0.000001
        control.StepTolerance = 0.000001
        control.FunctionTolerance = 0.000000001
        control.Trace = False
        control.UseBfgsCovarianceOptimization = True

        Dim objective As Func(Of Double(), Double) =
            Function(theta() As Double) Math.Pow(theta(0) - 2.0, 2.0) + 0.5 * Math.Pow(theta(1) + 1.0, 2.0)

        Dim trace As String = String.Empty
        Dim state As MixedModelOptimizationState = MixedModelOptimizer.OptimizeProjected(New Double() {8.0, -5.0},
                                                                                         objective,
                                                                                         control,
                                                                                         strTrace:=trace)

        Assert.IsTrue(state.Converged, "BFGS-enabled optimizer should converge on a smooth quadratic.")
        Assert.IsTrue(state.Iterations <= 12, "BFGS direction should avoid many steepest-descent iterations on a scaled quadratic.")
        Assert.AreEqual(2.0, state.Theta(0), 0.001)
        Assert.AreEqual(-1.0, state.Theta(1), 0.001)
        Assert.AreEqual("Numerical finite difference", state.GradientProviderName,
                        "Default optimizer call should report numerical finite differences.")
        Assert.IsTrue(state.ObjectiveEvaluationCount > 0, "Objective evaluation count should be populated.")
        Assert.IsTrue(state.GradientEvaluationCount > 0, "Gradient evaluation count should be populated.")
        Assert.IsTrue(state.NumericalGradientObjectiveEvaluationCount > 0,
                      "Numerical-gradient objective call count should be populated for the default optimizer gradient.")
        Assert.IsTrue(state.LineSearchEvaluationCount > 0, "Line-search evaluation count should be populated.")
    End Sub

    <TestMethod()>
    Public Sub MixedModelOptimizer_CallerSuppliedGradient_ReportsProviderAndSkipsNumericalGradientCounts()
        Dim control As MixedModelControl = MixedModelControl.CreateDefault()
        control.MaxIter = 25
        control.Epsilon = 0.000001
        control.StepTolerance = 0.000001
        control.FunctionTolerance = 0.000000001
        control.Trace = False
        control.UseBfgsCovarianceOptimization = True

        Dim objective As Func(Of Double(), Double) =
            Function(theta() As Double) Math.Pow(theta(0) - 2.0, 2.0) + 0.5 * Math.Pow(theta(1) + 1.0, 2.0)

        Dim gradient As Func(Of Double(), Double()) =
            Function(theta() As Double) New Double() {2.0 * (theta(0) - 2.0), theta(1) + 1.0}

        Dim trace As String = String.Empty
        Dim state As MixedModelOptimizationState = MixedModelOptimizer.OptimizeProjected(New Double() {8.0, -5.0},
                                                                                         objective,
                                                                                         control,
                                                                                         gradient:=gradient,
                                                                                         strTrace:=trace)

        Assert.IsTrue(state.Converged, "Caller-supplied gradient should converge on a smooth quadratic.")
        Assert.AreEqual("Caller-supplied gradient", state.GradientProviderName,
                        "Optimizer should report the explicit gradient provider.")
        Assert.IsTrue(state.ObjectiveEvaluationCount > 0, "Objective evaluation count should be populated.")
        Assert.IsTrue(state.GradientEvaluationCount > 0, "Gradient evaluation count should be populated.")
        Assert.AreEqual(0, state.NumericalGradientObjectiveEvaluationCount,
                        "Caller-supplied gradients should not record numerical-gradient objective evaluations.")
        Assert.IsTrue(state.LineSearchEvaluationCount > 0, "Line-search evaluation count should be populated.")
    End Sub


    <TestMethod()>
    Public Sub MMRM_UNWeakVisitPairSupport_EmitsWarning()
        Dim res As MixedModelResult = FitSyntheticWeakSupportUnstructuredMmrm()

        Assert.IsNotNull(res, "Weak-support UN fit should return a result object.")
        Assert.IsNotNull(res.VisitSupportDiagnostics, "Visit support diagnostics should be available.")
        Assert.IsTrue(res.VisitSupportDiagnostics.Enabled, "MMRM support diagnostics should be enabled for MMRM fits.")
        Assert.IsTrue(res.VisitSupportDiagnostics.MinimumVisitPairCount < res.VisitSupportDiagnostics.WeakPairThreshold,
                      "Synthetic data should have at least one weakly supported visit pair.")
        Assert.IsTrue(res.UserWarnings IsNot Nothing AndAlso res.UserWarnings.Any(Function(w) w.IndexOf("UN covariance warning", StringComparison.OrdinalIgnoreCase) >= 0),
                      "Weak visit-pair support should emit a user-facing UN covariance warning.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_DiagnosticTables_AreControlledByWrapResultsFlag()
        Dim res As MixedModelResult = FitSyntheticMmrmForObjectivePatternCache(subjectCount:=24,
                                                                               visitCount:=4,
                                                                               incompletePatterns:=False)

        Dim withDiagnostics As List(Of ResultTable) = res.wrapResults(includeDiagnostics:=True)
        Dim withoutDiagnostics As List(Of ResultTable) = res.wrapResults(includeDiagnostics:=False)

        Assert.IsTrue(ContainsResultTableTextPerformance(withDiagnostics, "Performance diagnostics"),
                      "Diagnostic output should include performance diagnostics when requested.")
        Assert.IsTrue(ContainsResultTableTextPerformance(withDiagnostics, "MMRM support diagnostics"),
                      "Diagnostic output should include support diagnostics when requested.")
        Assert.IsFalse(ContainsResultTableTextPerformance(withoutDiagnostics, "Performance diagnostics"),
                       "Diagnostic output should suppress performance diagnostics when includeDiagnostics=False.")
        Assert.IsFalse(ContainsResultTableTextPerformance(withoutDiagnostics, "MMRM support diagnostics"),
                       "Diagnostic output should suppress support diagnostics when includeDiagnostics=False.")
    End Sub


    <TestMethod()>
    Public Sub MMRM_KRDerivativePatternCache_ReusesCompleteVisitPattern()
        Dim res As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=40,
                                                         visitCount:=4,
                                                         residualStruct:=New DiagonalHeterogeneousR(),
                                                         maxIter:=60)

        Assert.IsNotNull(res, "MMRM KR fit should return a result.")
        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.DerivativePatternCache, "Derivative pattern cache diagnostics should be available.")

        Dim cache As MixedModelKrDerivativePatternCacheDiagnostics = res.KenwardRogerWorkspace.DerivativePatternCache
        Assert.IsTrue(cache.Enabled, "KR derivative visit-pattern cache should be enabled for MMRM fits.")
        Assert.AreEqual(1, cache.PatternCount, "Complete repeated-visit KR data should have one derivative visit pattern.")
        Assert.IsTrue(cache.VInvHits > 0, "Complete repeated-visit KR data should reuse cached V inverse blocks.")
        Assert.IsTrue(cache.FirstDerivativeHits > 0, "Complete repeated-visit KR data should reuse first-derivative tensors.")
        Assert.IsTrue(cache.SecondDerivativeHits > 0, "Full KR should reuse second-derivative tensors.")
        Assert.AreEqual(0, cache.InvalidBuilds, "Valid synthetic KR data should not produce invalid derivative pattern-cache builds.")

        Dim wrapped As List(Of ResultTable) = res.wrapResults(includeKenwardRogerTermTests:=True)
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "Derivative pattern cache first derivative hits"),
                      "KR finite-difference diagnostics table should include derivative pattern-cache counters.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_KRDerivativePatternCache_TracksMultipleIncompleteVisitPatterns()
        Dim res As MixedModelResult = FitSyntheticKrMmrmWithIncompletePatterns(subjectCount:=45,
                                                                               visitCount:=4,
                                                                               residualStruct:=New DiagonalHeterogeneousR(),
                                                                               maxIter:=60)

        Assert.IsNotNull(res, "Incomplete-pattern MMRM KR fit should return a result.")
        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.DerivativePatternCache, "Derivative pattern cache diagnostics should be available.")

        Dim cache As MixedModelKrDerivativePatternCacheDiagnostics = res.KenwardRogerWorkspace.DerivativePatternCache
        Assert.IsTrue(cache.Enabled, "KR derivative visit-pattern cache should be enabled for MMRM fits.")
        Assert.IsTrue(cache.PatternCount >= 2, "Incomplete repeated-visit KR data should track multiple derivative visit patterns.")
        Assert.IsTrue(cache.VInvHits > 0, "Repeated incomplete KR visit patterns should reuse cached V inverse blocks.")
        Assert.IsTrue(cache.FirstDerivativeHits > 0, "Repeated incomplete KR visit patterns should reuse first-derivative tensors.")
        Assert.AreEqual(0, cache.InvalidBuilds, "Valid incomplete synthetic KR data should not produce invalid derivative pattern-cache builds.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_KRPqrHalfPairOptimization_PopulatesDiagnostics()
        Dim res As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=28,
                                                         visitCount:=4,
                                                         residualStruct:=New DiagonalHeterogeneousR(),
                                                         maxIter:=60)

        Assert.IsNotNull(res, "MMRM KR fit should return a result.")
        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.PqrPairDiagnostics, "KR P/Q/R pair diagnostics should be available.")

        Dim pqr As MixedModelKrPqrPairDiagnostics = res.KenwardRogerWorkspace.PqrPairDiagnostics
        Dim cache As MixedModelKrPqrDesignPatternCacheDiagnostics = res.KenwardRogerWorkspace.PqrDesignPatternCache
        Dim k As Integer = res.KenwardRogerWorkspace.K
        Dim blockCount As Integer = res.KenwardRogerWorkspace.Blocks.Count
        Dim patternCount As Integer = If(cache Is Nothing OrElse Not cache.Enabled, blockCount, cache.PatternCount)
        Dim halfPairsPerPattern As Integer = (k * (k + 1)) \ 2
        Dim symmetryPairsPerPattern As Integer = (k * (k - 1)) \ 2

        Assert.IsTrue(pqr.Enabled, "KR P/Q/R half-pair optimization should be enabled.")
        Assert.IsTrue(pqr.FastFactorizationEnabled, "KR P/Q/R fast factorization should be enabled by default.")
        Assert.AreEqual(k, pqr.ParameterCount, "KR P/Q/R diagnostics should record the covariance-parameter count.")
        Assert.AreEqual(patternCount * halfPairsPerPattern, pqr.QPairMatricesComputed,
                        "Q_hj should be computed once per retained design pattern for h <= j.")
        Assert.AreEqual(patternCount * symmetryPairsPerPattern, pqr.QPairMatricesFilledBySymmetry,
                        "Q_jh should be filled by transposing Q_hj for h < j per retained design pattern.")

        If res.KenwardRogerWorkspace.Rmats IsNot Nothing Then
            Assert.AreEqual(patternCount * halfPairsPerPattern, pqr.RPairMatricesComputed,
                            "R_hj should be computed once per retained design pattern for h <= j when second derivatives are available.")
            Assert.AreEqual(patternCount * symmetryPairsPerPattern, pqr.RPairMatricesFilledBySymmetry,
                            "R_jh should be filled from R_hj for h < j per retained design pattern when second derivatives are available.")
        End If

        Dim wrapped As List(Of ResultTable) = res.wrapResults(includeKenwardRogerTermTests:=True)
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "KR pair matrices filled by symmetry"),
                      "Performance diagnostics should include KR half-pair counters.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_KRPqrDesignPatternCache_ReusesRepeatedFixedDesigns()
        Dim res As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=28,
                                                         visitCount:=4,
                                                         residualStruct:=New DiagonalHeterogeneousR(),
                                                         maxIter:=60)

        Assert.IsNotNull(res, "MMRM KR fit should return a result.")
        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.PqrDesignPatternCache, "KR P/Q/R design-pattern cache diagnostics should be available.")

        Dim cache As MixedModelKrPqrDesignPatternCacheDiagnostics = res.KenwardRogerWorkspace.PqrDesignPatternCache
        Dim blockCount As Integer = res.KenwardRogerWorkspace.Blocks.Count

        Assert.IsTrue(cache.Enabled, "KR P/Q/R design-pattern cache should be enabled by default.")
        Assert.AreEqual(blockCount, cache.BlockCount, "Design-pattern diagnostics should record all KR blocks.")
        Assert.IsTrue(cache.PatternCount > 0 AndAlso cache.PatternCount < blockCount,
                      "Repeated synthetic MMRM profiles should collapse to fewer P/Q/R design patterns than subject blocks.")
        Assert.IsTrue(cache.Hits > 0, "Repeated synthetic MMRM profiles should produce P/Q/R design-pattern cache hits.")
        Assert.AreEqual(cache.PatternCount, cache.Misses, "Each retained P/Q/R design pattern should be a cache miss exactly once.")
        Assert.AreEqual(0, cache.InvalidBuilds, "Valid synthetic data should not produce invalid P/Q/R design-pattern cache builds.")

        Dim wrapped As List(Of ResultTable) = res.wrapResults(includeKenwardRogerTermTests:=True)
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "KR P/Q/R design-pattern hits"),
                      "Performance diagnostics should include KR P/Q/R design-pattern cache counters.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_KRPqrDesignPatternCache_MatchesDisabledPath()
        Dim enabled As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=16,
                                                             visitCount:=4,
                                                             residualStruct:=New DiagonalHeterogeneousR(),
                                                             maxIter:=60,
                                                             useKrPqrDesignPatternCache:=True)
        Dim disabled As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=16,
                                                              visitCount:=4,
                                                              residualStruct:=New DiagonalHeterogeneousR(),
                                                              maxIter:=60,
                                                              useKrPqrDesignPatternCache:=False)

        Assert.IsNotNull(enabled.KenwardRogerWorkspace, "Enabled-cache KR workspace should be available.")
        Assert.IsNotNull(disabled.KenwardRogerWorkspace, "Disabled-cache KR workspace should be available.")
        Assert.IsTrue(enabled.KenwardRogerWorkspace.PqrDesignPatternCache.Enabled, "Enabled run should mark the design-pattern cache as enabled.")
        Assert.IsFalse(disabled.KenwardRogerWorkspace.PqrDesignPatternCache.Enabled, "Disabled run should mark the design-pattern cache as disabled.")
        Assert.IsTrue(enabled.KenwardRogerWorkspace.PqrDesignPatternCache.Hits > 0, "Enabled run should reuse repeated design patterns.")

        AssertArrayAlmostEqual(disabled.KenwardRogerWorkspace.Pmats, enabled.KenwardRogerWorkspace.Pmats, 0.00000001, "P matrices")
        AssertArrayAlmostEqual(disabled.KenwardRogerWorkspace.Qmats, enabled.KenwardRogerWorkspace.Qmats, 0.00000001, "Q matrices")
        AssertArrayAlmostEqual(disabled.KenwardRogerWorkspace.Rmats, enabled.KenwardRogerWorkspace.Rmats, 0.00000001, "R matrices")
        AssertArrayAlmostEqual(disabled.KenwardRogerAdjustedVarBeta, enabled.KenwardRogerAdjustedVarBeta, 0.00000001, "KR adjusted Var(beta)")
    End Sub

    <TestMethod()>
    Public Sub MMRM_KRPqrFastFactorization_MatchesDirectPath()
        Dim fast As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=14,
                                                          visitCount:=4,
                                                          residualStruct:=New DiagonalHeterogeneousR(),
                                                          maxIter:=60,
                                                          useKrPqrDesignPatternCache:=False,
                                                          useKrPqrFastFactorization:=True)
        Dim direct As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=14,
                                                            visitCount:=4,
                                                            residualStruct:=New DiagonalHeterogeneousR(),
                                                            maxIter:=60,
                                                            useKrPqrDesignPatternCache:=False,
                                                            useKrPqrFastFactorization:=False)

        Assert.IsNotNull(fast.KenwardRogerWorkspace, "Fast KR workspace should be available.")
        Assert.IsNotNull(direct.KenwardRogerWorkspace, "Direct KR workspace should be available.")
        Assert.IsTrue(fast.KenwardRogerWorkspace.PqrPairDiagnostics.FastFactorizationEnabled,
                      "Fast run should mark KR P/Q/R fast factorization as enabled.")
        Assert.IsFalse(direct.KenwardRogerWorkspace.PqrPairDiagnostics.FastFactorizationEnabled,
                       "Direct-reference run should mark KR P/Q/R fast factorization as disabled.")

        AssertArrayAlmostEqual(direct.KenwardRogerWorkspace.Pmats, fast.KenwardRogerWorkspace.Pmats, 0.0000001, "P matrices fast factorization")
        AssertArrayAlmostEqual(direct.KenwardRogerWorkspace.Qmats, fast.KenwardRogerWorkspace.Qmats, 0.0000001, "Q matrices fast factorization")
        AssertArrayAlmostEqual(direct.KenwardRogerWorkspace.Rmats, fast.KenwardRogerWorkspace.Rmats, 0.0000001, "R matrices fast factorization")
        AssertArrayAlmostEqual(direct.KenwardRogerAdjustedVarBeta, fast.KenwardRogerAdjustedVarBeta, 0.0000001, "KR adjusted Var(beta) fast factorization")

        Dim wrapped As List(Of ResultTable) = fast.wrapResults(includeKenwardRogerTermTests:=True)
        Assert.IsTrue(ContainsResultTableTextPerformance(wrapped, "KR P/Q/R fast factorization enabled"),
                      "Performance diagnostics should include the KR P/Q/R fast-factorization flag.")
    End Sub


    <TestMethod()>
    Public Sub MMRM_ResultReleaseLargePostEstimationWorkspaces_ClearsHeavyKrReferences()
        Dim res As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=12,
                                                         visitCount:=4,
                                                         residualStruct:=New DiagonalHeterogeneousR(),
                                                         maxIter:=60)

        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should exist before release.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Blocks, "KR block list should exist before release.")
        Assert.IsTrue(res.KenwardRogerWorkspace.Blocks.Count > 0, "KR block list should contain block workspaces before release.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Pmats, "KR P matrices should exist before release.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Qmats, "KR Q matrices should exist before release.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Rmats, "KR R matrices should exist before release.")
        Assert.IsNotNull(res.InferenceWorkspace, "Inference workspace should exist before release.")
        Assert.IsNotNull(res.InferenceWorkspace.KR_P, "Inference KR P matrices should exist before release.")

        Dim adjustedBefore(,) As Double = res.KenwardRogerAdjustedVarBeta
        Assert.IsNotNull(adjustedBefore, "Adjusted Var(beta) should be available before release.")

        res.ReleaseLargePostEstimationWorkspaces()

        Assert.IsNull(res.KenwardRogerWorkspace, "KR workspace should be cleared after release.")
        Assert.IsNull(res.InferenceWorkspace, "Detailed inference workspace should be cleared after release.")
        Assert.IsNull(res.OptimizerTrace, "Optimizer trace should be released after cleanup.")
        Assert.IsNotNull(res.Beta, "Core beta estimates should remain available after release.")
        Assert.IsNotNull(res.VarBeta, "Core Var(beta) should remain available after release.")
        Assert.IsNotNull(res.KenwardRogerAdjustedVarBeta, "KR adjusted Var(beta) summary should remain available after release.")
        AssertArrayAlmostEqual(adjustedBefore, res.KenwardRogerAdjustedVarBeta, 0.0, "KR adjusted Var(beta) retained after release")
    End Sub

    <TestMethod()>
    <TestCategory("Performance")>
    Public Sub KrPerformance_50Subjects4Visits_DiagonalStable()
        Dim sw As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
        Dim res As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=50,
                                                         visitCount:=4,
                                                         residualStruct:=New DiagonalHeterogeneousR(),
                                                         maxIter:=60)
        sw.Stop()

        AssertKrPerformanceResult(res, expectedSubjects:=50, label:="50 subjects x 4 visits diagonal")
        Assert.IsTrue(sw.Elapsed.TotalSeconds < 45.0,
                      "50 subjects x 4 visits diagonal KR smoke test exceeded 45 seconds. Actual seconds=" & sw.Elapsed.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture))
    End Sub

    <TestMethod()>
    <TestCategory("Performance")>
    Public Sub KrPerformance_100Subjects6Visits_HeterogeneousAR1Stable()
        Dim sw As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
        Dim res As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=100,
                                                         visitCount:=6,
                                                         residualStruct:=New HeterogeneousAR1R(),
                                                         maxIter:=70)
        sw.Stop()

        AssertKrPerformanceResult(res, expectedSubjects:=100, label:="100 subjects x 6 visits heterogeneous AR(1)")
        Assert.IsTrue(sw.Elapsed.TotalSeconds < 90.0,
                      "100 subjects x 6 visits heterogeneous AR(1) KR smoke test exceeded 90 seconds. Actual seconds=" & sw.Elapsed.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture))
    End Sub

    <TestMethod()>
    <TestCategory("Performance")>
    Public Sub KrPerformance_200Subjects6Visits_UNStable()
        Dim sw As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
        Dim res As MixedModelResult = FitSyntheticKrMmrm(subjectCount:=200,
                                                         visitCount:=6,
                                                         residualStruct:=New UnstructuredR(),
                                                         maxIter:=80)
        sw.Stop()

        AssertKrPerformanceResult(res, expectedSubjects:=200, label:="200 subjects x 6 visits UN")
        Assert.IsNotNull(res.KenwardRogerWorkspace.DerivativePatternCache, "200 subjects x 6 visits UN: derivative pattern cache diagnostics should be available.")
        Assert.AreEqual(1, res.KenwardRogerWorkspace.DerivativePatternCache.PatternCount, "200 subjects x 6 visits UN: complete synthetic data should have one derivative visit pattern.")
        Assert.IsTrue(res.KenwardRogerWorkspace.DerivativePatternCache.FirstDerivativeHits > 0, "200 subjects x 6 visits UN: derivative pattern cache should report first-derivative hits.")
        Assert.IsTrue(res.KenwardRogerWorkspace.DerivativePatternCache.SecondDerivativeHits > 0, "200 subjects x 6 visits UN: derivative pattern cache should report second-derivative hits.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.PqrDesignPatternCache, "200 subjects x 6 visits UN: P/Q/R design-pattern cache diagnostics should be available.")
        Assert.IsTrue(res.KenwardRogerWorkspace.PqrDesignPatternCache.Enabled, "200 subjects x 6 visits UN: P/Q/R design-pattern cache should be enabled.")
        Assert.IsTrue(res.KenwardRogerWorkspace.PqrDesignPatternCache.PatternCount < res.KenwardRogerWorkspace.PqrDesignPatternCache.BlockCount,
                      "200 subjects x 6 visits UN: repeated fixed designs should reduce P/Q/R design-pattern count.")
        Assert.IsTrue(res.KenwardRogerWorkspace.PqrDesignPatternCache.Hits > 0, "200 subjects x 6 visits UN: P/Q/R design-pattern cache should report hits.")
        Assert.IsTrue(res.KenwardRogerWorkspace.PqrPairDiagnostics.QPairMatricesComputed < res.KenwardRogerWorkspace.PqrDesignPatternCache.BlockCount * ((res.KenwardRogerWorkspace.K * (res.KenwardRogerWorkspace.K + 1)) \ 2),
                      "200 subjects x 6 visits UN: P/Q/R cache should reduce Q pair matrix computations versus one full set per block. Elapsed seconds=" & sw.Elapsed.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture))
    End Sub

    Private Shared Sub AssertKrPerformanceResult(res As MixedModelResult,
                                                 expectedSubjects As Integer,
                                                 label As String)
        Assert.IsNotNull(res, label & ": result should not be Nothing.")
        Assert.AreEqual(expectedSubjects, res.NoSubjects, label & ": subject count.")
        Assert.IsNotNull(res.Beta, label & ": beta vector should be available.")
        Assert.IsTrue(res.Beta.Length > 0, label & ": beta vector should be non-empty.")
        For Each b As Double In res.Beta
            Assert.IsFalse(Double.IsNaN(b), label & ": beta should not contain NaN.")
            Assert.IsFalse(Double.IsInfinity(b), label & ": beta should not contain Infinity.")
        Next

        Assert.IsNotNull(res.KenwardRogerWorkspace, label & ": KR workspace should be available.")
        Assert.AreEqual(expectedSubjects, res.KenwardRogerWorkspace.VinvCachedBlockCount, label & ": cached V^-1 block count.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Pmats, label & ": KR P matrices should be cached.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Qmats, label & ": KR Q matrices should be cached.")
        Assert.IsNotNull(res.KenwardRogerAdjustedVarBeta, label & ": KR adjusted Var(beta) should be available.")
        Assert.IsTrue(Matrix.MatrixIsFinite(res.KenwardRogerAdjustedVarBeta),
                      label & ": KR adjusted Var(beta) should be finite.")

        If res.KenwardRogerWorkspace.NumericalWarnings IsNot Nothing Then
            For Each one As String In res.KenwardRogerWorkspace.NumericalWarnings
                Assert.IsFalse(String.IsNullOrWhiteSpace(one), label & ": numerical warnings should be non-empty text when present.")
            Next
        End If
    End Sub

    Private Shared Sub AssertArrayAlmostEqual(expected(,) As Double,
                                                   actual(,) As Double,
                                                   tolerance As Double,
                                                   label As String)
        Assert.IsNotNull(expected, label & ": expected array should not be Nothing.")
        Assert.IsNotNull(actual, label & ": actual array should not be Nothing.")
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), label & ": first dimension.")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), label & ": second dimension.")

        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(i, j), actual(i, j), tolerance,
                                label & " [" & i.ToString(CultureInfo.InvariantCulture) & "," & j.ToString(CultureInfo.InvariantCulture) & "]")
            Next
        Next
    End Sub

    Private Shared Sub AssertArrayAlmostEqual(expected(,,) As Double,
                                                   actual(,,) As Double,
                                                   tolerance As Double,
                                                   label As String)
        Assert.IsNotNull(expected, label & ": expected array should not be Nothing.")
        Assert.IsNotNull(actual, label & ": actual array should not be Nothing.")
        For d As Integer = 0 To 2
            Assert.AreEqual(expected.GetLength(d), actual.GetLength(d), label & ": dimension " & d.ToString(CultureInfo.InvariantCulture) & ".")
        Next

        For h As Integer = 0 To expected.GetLength(0) - 1
            For i As Integer = 0 To expected.GetLength(1) - 1
                For j As Integer = 0 To expected.GetLength(2) - 1
                    Assert.AreEqual(expected(h, i, j), actual(h, i, j), tolerance,
                                    label & " [" & h.ToString(CultureInfo.InvariantCulture) & "," & i.ToString(CultureInfo.InvariantCulture) & "," & j.ToString(CultureInfo.InvariantCulture) & "]")
                Next
            Next
        Next
    End Sub

    Private Shared Sub AssertArrayAlmostEqual(expected(,,,) As Double,
                                                   actual(,,,) As Double,
                                                   tolerance As Double,
                                                   label As String)
        Assert.IsNotNull(expected, label & ": expected array should not be Nothing.")
        Assert.IsNotNull(actual, label & ": actual array should not be Nothing.")
        For d As Integer = 0 To 3
            Assert.AreEqual(expected.GetLength(d), actual.GetLength(d), label & ": dimension " & d.ToString(CultureInfo.InvariantCulture) & ".")
        Next

        For h As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                For r As Integer = 0 To expected.GetLength(2) - 1
                    For c As Integer = 0 To expected.GetLength(3) - 1
                        Assert.AreEqual(expected(h, j, r, c), actual(h, j, r, c), tolerance,
                                        label & " [" & h.ToString(CultureInfo.InvariantCulture) & "," & j.ToString(CultureInfo.InvariantCulture) & "," & r.ToString(CultureInfo.InvariantCulture) & "," & c.ToString(CultureInfo.InvariantCulture) & "]")
                    Next
                Next
            Next
        Next
    End Sub

    Private Shared Sub AssertFiniteNonNegativeTiming(value As Double,
                                                   label As String)
        Assert.IsFalse(Double.IsNaN(value), label & " should not be NaN.")
        Assert.IsFalse(Double.IsInfinity(value), label & " should not be Infinity.")
        Assert.IsTrue(value >= 0.0, label & " should be non-negative.")
    End Sub

    Private Shared Function ContainsResultTableTextPerformance(tables As List(Of ResultTable),
                                                               expectedText As String) As Boolean
        If tables Is Nothing Then Return False

        For Each t As ResultTable In tables
            If t Is Nothing Then Continue For
            Dim arr(,) As Object = t.returnSelf()
            If arr Is Nothing Then Continue For

            For i As Integer = 0 To arr.GetLength(0) - 1
                For j As Integer = 0 To arr.GetLength(1) - 1
                    Dim cell As String = Convert.ToString(arr(i, j), CultureInfo.InvariantCulture)
                    If String.Equals(cell, expectedText, StringComparison.OrdinalIgnoreCase) Then Return True
                Next
            Next
        Next

        Return False
    End Function

    Private Shared Function FitSyntheticMmrmForObjectivePatternCache(subjectCount As Integer,
                                                                    visitCount As Integer,
                                                                    incompletePatterns As Boolean,
                                                                    Optional covarianceGradientMode As MixedModelCovarianceGradientMode = MixedModelCovarianceGradientMode.Auto,
                                                                    Optional fallbackToNumericalGradientOnAnalyticFailure As Boolean = True,
                                                                    Optional covarianceOptimizerMode As MixedModelCovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml) As MixedModelResult
        Dim yVals As New List(Of Double)()
        Dim subjectVals As New List(Of Object)()
        Dim visitVals As New List(Of Double)()
        Dim treatmentVals As New List(Of Double)()
        Dim visitCenter As Double = (CDbl(visitCount) - 1.0) / 2.0

        For s As Integer = 0 To subjectCount - 1
            Dim treatment As Double = If((s Mod 2) = 0, 0.0, 1.0)
            Dim subjectShift As Double = 0.03 * CDbl(s Mod 9)

            For v As Integer = 1 To visitCount
                If incompletePatterns Then
                    If (s Mod 3) = 1 AndAlso v = visitCount Then Continue For
                    If (s Mod 3) = 2 AndAlso v = 2 Then Continue For
                End If

                subjectVals.Add("S" & s.ToString("0000", CultureInfo.InvariantCulture))
                visitVals.Add(CDbl(v))
                treatmentVals.Add(treatment)

                Dim visitC As Double = CDbl(v - 1) - visitCenter
                Dim deterministicNoise As Double = 0.1 * Math.Sin(0.29 * CDbl(s + 1) + 0.61 * CDbl(v))
                yVals.Add(10.0 + 0.8 * treatment + 0.25 * visitC + subjectShift + deterministicNoise)
            Next
        Next

        Dim n As Integer = yVals.Count
        Dim y(n - 1) As Double
        Dim x(n - 1, 2) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double

        For i As Integer = 0 To n - 1
            y(i) = yVals(i)
            subject(i) = subjectVals(i)
            visit(i) = visitVals(i)
            x(i, 0) = 1.0
            x(i, 1) = treatmentVals(i)
            x(i, 2) = visit(i) - 1.0 - visitCenter
        Next

        Dim blockData As MixedModelBlockData =
            MixedModelBlockData.FromArrays(y:=y,
                                           x:=x,
                                           subjectId:=subject,
                                           visit:=visit)

        Dim req As MixedModelFitRequest =
            MixedModelFitRequest.CreateMMRM(blockData,
                                            residualStruct:=New DiagonalHeterogeneousR(),
                                            fitMethod:=MixedModelFitMethod.REML)

        req.FixedEffectNames = New String() {"(Intercept)", "treatment", "visit_c"}

        Dim control As MixedModelControl = req.Control
        control.MaxIter = 35
        control.Epsilon = 0.000001
        control.StepTolerance = 0.000001
        control.FunctionTolerance = 0.0000001
        control.Trace = False
        control.CovarianceGradientMode = covarianceGradientMode
        control.CovarianceOptimizerMode = covarianceOptimizerMode
        control.FallbackToNumericalGradientOnAnalyticFailure = fallbackToNumericalGradientOnAnalyticFailure
        req.Control = control

        Return (New MMRM(req)).Fit()
    End Function


    Private Shared Function FitSyntheticWeakSupportUnstructuredMmrm() As MixedModelResult
        Dim yVals As New List(Of Double)()
        Dim subjectVals As New List(Of Object)()
        Dim visitVals As New List(Of Double)()
        Dim treatmentVals As New List(Of Double)()
        Dim visitCount As Integer = 4
        Dim subjectCount As Integer = 30
        Dim visitCenter As Double = (CDbl(visitCount) - 1.0) / 2.0

        For s As Integer = 0 To subjectCount - 1
            Dim treatment As Double = If((s Mod 2) = 0, 0.0, 1.0)
            For v As Integer = 1 To visitCount
                If v = visitCount AndAlso s >= 4 Then Continue For

                subjectVals.Add("S" & s.ToString("0000", CultureInfo.InvariantCulture))
                visitVals.Add(CDbl(v))
                treatmentVals.Add(treatment)

                Dim visitC As Double = CDbl(v - 1) - visitCenter
                Dim deterministicNoise As Double = 0.08 * Math.Sin(0.31 * CDbl(s + 1) + 0.67 * CDbl(v))
                yVals.Add(8.0 + 0.7 * treatment + 0.2 * visitC + 0.02 * CDbl(s Mod 7) + deterministicNoise)
            Next
        Next

        Dim n As Integer = yVals.Count
        Dim y(n - 1) As Double
        Dim x(n - 1, 2) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double

        For i As Integer = 0 To n - 1
            y(i) = yVals(i)
            subject(i) = subjectVals(i)
            visit(i) = visitVals(i)
            x(i, 0) = 1.0
            x(i, 1) = treatmentVals(i)
            x(i, 2) = visit(i) - 1.0 - visitCenter
        Next

        Dim blockData As MixedModelBlockData =
            MixedModelBlockData.FromArrays(y:=y,
                                           x:=x,
                                           subjectId:=subject,
                                           visit:=visit)

        Dim req As MixedModelFitRequest =
            MixedModelFitRequest.CreateMMRM(blockData,
                                            residualStruct:=New UnstructuredR(),
                                            fitMethod:=MixedModelFitMethod.REML)

        req.FixedEffectNames = New String() {"(Intercept)", "treatment", "visit_c"}

        Dim control As MixedModelControl = req.Control
        control.MaxIter = 20
        control.Epsilon = 0.000001
        control.StepTolerance = 0.000001
        control.FunctionTolerance = 0.0000001
        control.Trace = False
        req.Control = control

        Return (New MMRM(req)).Fit()
    End Function

    Private Shared Function IsAverageInformationOrItsFallback(actualOptimizerName As String) As Boolean
        If String.IsNullOrWhiteSpace(actualOptimizerName) Then Return False
        If String.Equals(actualOptimizerName, "Average Information REML", StringComparison.Ordinal) Then Return True
        Return actualOptimizerName.IndexOf("Average Information", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
               actualOptimizerName.IndexOf("fallback", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Shared Function FitSyntheticKrMmrmWithIncompletePatterns(subjectCount As Integer,
                                                                     visitCount As Integer,
                                                                     residualStruct As MixedModelRStruct,
                                                                     maxIter As Integer) As MixedModelResult
        Dim yVals As New List(Of Double)()
        Dim subjectVals As New List(Of Object)()
        Dim visitVals As New List(Of Double)()
        Dim treatmentVals As New List(Of Double)()
        Dim visitCenter As Double = (CDbl(visitCount) - 1.0) / 2.0

        For s As Integer = 0 To subjectCount - 1
            Dim treatment As Double = If((s Mod 2) = 0, 0.0, 1.0)
            Dim subjectShift As Double = 0.04 * CDbl(s Mod 11)

            For v As Integer = 1 To visitCount
                If (s Mod 3) = 1 AndAlso v = visitCount Then Continue For
                If (s Mod 3) = 2 AndAlso v = 2 Then Continue For

                subjectVals.Add("S" & s.ToString("0000", CultureInfo.InvariantCulture))
                visitVals.Add(CDbl(v))
                treatmentVals.Add(treatment)

                Dim visitC As Double = CDbl(v - 1) - visitCenter
                Dim deterministicNoise As Double = 0.15 * Math.Sin(0.37 * CDbl(s + 1) + 0.71 * CDbl(v))
                yVals.Add(20.0 + 1.5 * treatment + 0.45 * visitC + subjectShift + deterministicNoise)
            Next
        Next

        Dim n As Integer = yVals.Count
        Dim y(n - 1) As Double
        Dim x(n - 1, 2) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double

        For i As Integer = 0 To n - 1
            y(i) = yVals(i)
            subject(i) = subjectVals(i)
            visit(i) = visitVals(i)
            x(i, 0) = 1.0
            x(i, 1) = treatmentVals(i)
            x(i, 2) = visit(i) - 1.0 - visitCenter
        Next

        Dim blockData As MixedModelBlockData =
            MixedModelBlockData.FromArrays(y:=y,
                                           x:=x,
                                           subjectId:=subject,
                                           visit:=visit)

        Dim req As MixedModelFitRequest =
            MixedModelFitRequest.CreateMMRM(blockData,
                                            residualStruct:=residualStruct,
                                            fitMethod:=MixedModelFitMethod.REML)

        req.FixedEffectNames = New String() {"(Intercept)", "treatment", "visit_c"}
        req.EnableFullKenwardRogerForMmrm()

        Dim control As MixedModelControl = req.Control
        control.MaxIter = maxIter
        control.Epsilon = 0.0000001
        control.StepTolerance = 0.0000001
        control.FunctionTolerance = 0.000000001
        control.Trace = False
        req.Control = control

        Return (New MMRM(req)).Fit()
    End Function

    Private Shared Function FitSyntheticKrMmrm(subjectCount As Integer,
                                               visitCount As Integer,
                                               residualStruct As MixedModelRStruct,
                                               maxIter As Integer,
                                               Optional useKrPqrDesignPatternCache As Boolean = True,
                                               Optional useKrPqrFastFactorization As Boolean = True) As MixedModelResult
        Dim n As Integer = subjectCount * visitCount
        Dim y(n - 1) As Double
        Dim x(n - 1, 2) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double

        Dim idx As Integer = 0
        Dim visitCenter As Double = (CDbl(visitCount) - 1.0) / 2.0

        For s As Integer = 0 To subjectCount - 1
            Dim treatment As Double = If((s Mod 2) = 0, 0.0, 1.0)
            Dim subjectShift As Double = 0.04 * CDbl(s Mod 11)

            For v As Integer = 0 To visitCount - 1
                subject(idx) = "S" & s.ToString("0000", CultureInfo.InvariantCulture)
                visit(idx) = CDbl(v + 1)

                Dim visitC As Double = CDbl(v) - visitCenter
                x(idx, 0) = 1.0
                x(idx, 1) = treatment
                x(idx, 2) = visitC

                Dim deterministicNoise As Double = 0.15 * Math.Sin(0.37 * CDbl(s + 1) + 0.71 * CDbl(v + 1))
                y(idx) = 20.0 + 1.5 * treatment + 0.45 * visitC + subjectShift + deterministicNoise
                idx += 1
            Next
        Next

        Dim blockData As MixedModelBlockData =
            MixedModelBlockData.FromArrays(y:=y,
                                           x:=x,
                                           subjectId:=subject,
                                           visit:=visit)

        Dim req As MixedModelFitRequest =
            MixedModelFitRequest.CreateMMRM(blockData,
                                            residualStruct:=residualStruct,
                                            fitMethod:=MixedModelFitMethod.REML)

        req.FixedEffectNames = New String() {"(Intercept)", "treatment", "visit_c"}
        req.EnableFullKenwardRogerForMmrm()

        Dim control As MixedModelControl = req.Control
        control.MaxIter = maxIter
        control.Epsilon = 0.0000001
        control.StepTolerance = 0.0000001
        control.FunctionTolerance = 0.000000001
        control.Trace = False
        control.UseKrPqrDesignPatternCache = useKrPqrDesignPatternCache
        control.UseKrPqrFastFactorization = useKrPqrFastFactorization
        req.Control = control

        Return (New MMRM(req)).Fit()
    End Function

    Private Shared Function FitSyntheticMmrmForCancellationDiagnostics(subjectCount As Integer,
                                                                        visitCount As Integer,
                                                                        residualStruct As MixedModelRStruct,
                                                                        maxIter As Integer,
                                                                        cancelImmediately As Boolean,
                                                                        Optional interruptImmediately As Boolean = False) As MixedModelResult
        Dim n As Integer = subjectCount * visitCount
        Dim y(n - 1) As Double
        Dim x(n - 1, 2) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double

        Dim idx As Integer = 0
        Dim visitCenter As Double = (CDbl(visitCount) - 1.0) / 2.0

        For s As Integer = 0 To subjectCount - 1
            Dim treatment As Double = If((s Mod 2) = 0, 0.0, 1.0)
            Dim subjectShift As Double = 0.08 * Math.Sin(0.21 * CDbl(s + 1))

            For v As Integer = 0 To visitCount - 1
                subject(idx) = "S" & s.ToString("0000", CultureInfo.InvariantCulture)
                visit(idx) = CDbl(v + 1)

                Dim visitC As Double = CDbl(v) - visitCenter
                x(idx, 0) = 1.0
                x(idx, 1) = treatment
                x(idx, 2) = visitC

                Dim serialNoise As Double = 0.18 * Math.Sin(0.31 * CDbl(s + 1) + 0.53 * CDbl(v + 1))
                y(idx) = 15.0 + 1.1 * treatment + 0.35 * visitC + subjectShift + serialNoise
                idx += 1
            Next
        Next

        Dim blockData As MixedModelBlockData =
            MixedModelBlockData.FromArrays(y:=y,
                                           x:=x,
                                           subjectId:=subject,
                                           visit:=visit)

        Dim req As MixedModelFitRequest =
            MixedModelFitRequest.CreateMMRM(blockData,
                                            residualStruct:=residualStruct,
                                            fitMethod:=MixedModelFitMethod.REML)

        req.FixedEffectNames = New String() {"(Intercept)", "treatment", "visit_c"}
        req.CancellationRequested = Function() cancelImmediately
        req.InterruptionRequested = Function() interruptImmediately

        Dim control As MixedModelControl = req.Control
        control.MaxIter = maxIter
        control.Epsilon = 0.000001
        control.StepTolerance = 0.000001
        control.FunctionTolerance = 0.0000001
        control.Trace = False
        req.Control = control

        Return (New MMRM(req)).Fit()
    End Function


    Private Shared Function BuildTwoParameterCacheResult() As MixedModelResult
        Dim pMats(1, 1, 1) As Double
        pMats(0, 0, 0) = 0.35
        pMats(0, 1, 1) = 0.1
        pMats(1, 0, 0) = 0.05
        pMats(1, 1, 1) = 0.25

        Dim thetaCov(1, 1) As Double
        thetaCov(0, 0) = 0.015
        thetaCov(0, 1) = 0.002
        thetaCov(1, 0) = 0.002
        thetaCov(1, 1) = 0.02

        Dim phi(1, 1) As Double
        phi(0, 0) = 4.0
        phi(0, 1) = 0.4
        phi(1, 0) = 0.4
        phi(1, 1) = 3.0

        Dim adjusted(1, 1) As Double
        adjusted(0, 0) = 4.2
        adjusted(0, 1) = 0.5
        adjusted(1, 0) = 0.5
        adjusted(1, 1) = 3.3

        Dim ws As New MixedModelKrWorkspace With {
            .P = 2,
            .K = 2,
            .VarBeta = phi,
            .ThetaCovariance = thetaCov,
            .Pmats = pMats,
            .AdjustedVarBeta = adjusted,
            .ParameterScale = MixedModelKrParameterScale.MmrmTheta
        }

        Return New MixedModelResult With {
            .P = 2,
            .Beta = New Double() {1.0, 2.0},
            .VarBeta = phi,
            .FixedEffectNames = New String() {"b0", "b1"},
            .KenwardRogerWorkspace = ws,
            .KenwardRogerAdjustedVarBeta = adjusted
        }
    End Function

End Class

' ===== END KR performance and numerical hardening tests =====

' ===== BEGIN MMRM multicovariate missing KR internal ingredient R-reference diagnostics =====

<TestClass()>
Public Class MMRMMulticovariateMissingKRInternalIngredientParityTests

    Public Property TestContext As Microsoft.VisualStudio.TestTools.UnitTesting.TestContext

    Private Const DATA_FILE_INTERNAL As String = "mixedmodel_longitudinal_multicovariate_missing.csv"
    Private Const R_REFERENCE_FILE_INTERNAL As String = "kr_mmrm_multicovariate_missing_internal_reference.csv"
    Private Const STRICT_ENV_INTERNAL As String = "BESHSTAT_KR_STRICT_INTERNAL_REFERENCE"
    Private Const COMPARE_RAW_INTERNAL_ENV As String = "BESHSTAT_KR_COMPARE_RAW_INTERNAL"

    Private Shared ReadOnly TARGET_STRUCTURES_INTERNAL As String() = New String() {
        "Compound Symmetry",
        "Heterogeneous Compound Symmetry",
        "AR(1)",
        "Heterogeneous AR(1)",
        "Unstructured"
    }

    <TestMethod()>
    Public Sub MulticovariateMissingMMRM_KRInternalIngredients_MatchRmmrmReferenceWhenAvailable()
        Dim actualRows As New List(Of KrInternalRow)()

        For Each structureName As String In TARGET_STRUCTURES_INTERNAL
            Dim res As MixedModelResult = FitKrMMRMInternal(structureName)
            AssertUsableKrInternalResult(res, structureName)
            actualRows.AddRange(BuildBeshInternalRows(structureName, res))
        Next

        WriteDiagnosticRows(actualRows, "besh_mmrm_multicovariate_missing_kr_internal_diagnostics.csv")

        Dim referencePath As String = TryFindTestDataPathInternal(R_REFERENCE_FILE_INTERNAL)
        If String.IsNullOrWhiteSpace(referencePath) Then
            Dim message As String = "R mmrm internal KR reference file is not available. " &
                                    "Run R_referenceScripts\mmrm_lmm\kr_mmrm_multicovariate_missing_internal_reference.R from the test project root " &
                                    "to generate TestData\" & R_REFERENCE_FILE_INTERNAL & ". " &
                                    "The BESH diagnostic export was still written for manual diffing."

            If StrictInternalReferenceMode() Then
                Assert.Fail(message)
            End If

            If Me.TestContext IsNot Nothing Then Me.TestContext.WriteLine(message)
            Return
        End If

        Dim referenceRows As List(Of KrInternalRow) = LoadReferenceRows(referencePath)
        Assert.IsTrue(referenceRows.Count > 0, "R mmrm internal KR reference file did not contain any comparable rows.")

        WriteComparisonReports(actualRows, referenceRows)
        AssertRowsAgainstReference(actualRows, referenceRows)
    End Sub


    Private Shared Function FitKrMMRMInternal(structureName As String) As MixedModelResult
        Dim dat As ModelDataInternal = LoadModelDataInternal()

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Y,
                                                                              x:=dat.X,
                                                                              subjectId:=dat.SubjectId,
                                                                              z:=Nothing,
                                                                              visit:=dat.Visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          CreateRStructInternal(structureName),
                                                                          MixedModelFitMethod.REML)

        req.RequestLabel = "R mmrm KR internal validation: " & structureName
        req.ResponseVarName = "distance_mm"
        req.SubjectVarName = "subject_id"
        req.VisitVarName = "visit"
        req.FixedEffectNames = FixedEffectNamesInternal()
        req.Control = ReferenceControlInternal()
        req.EnableFullKenwardRogerForMmrm()

        Dim startTheta() As Double = StartThetaForInternal(structureName)
        If startTheta IsNot Nothing Then
            req.StartThetaR = startTheta
        End If

        Return (New MMRM(req)).Fit()
    End Function


    Private Shared Sub AssertUsableKrInternalResult(result As MixedModelResult,
                                                    structureName As String)
        Assert.IsNotNull(result, structureName & " result should not be Nothing.")
        Assert.AreEqual(7, result.P, structureName & " fixed-effect dimension.")
        Assert.AreEqual(95, result.Nobs, structureName & " observation count after response filtering.")
        Assert.AreEqual(27, result.NoSubjects, structureName & " subject count.")
        Assert.IsNotNull(result.Beta, structureName & " beta should be available.")
        Assert.IsNotNull(result.Theta, structureName & " theta should be available.")
        Assert.IsNotNull(result.VarBeta, structureName & " ordinary Var(beta) should be available.")
        Assert.IsNotNull(result.KenwardRogerWorkspace, structureName & " KR workspace should be available.")
        Assert.IsNotNull(result.KenwardRogerWorkspace.ThetaCovariance, structureName & " theta covariance should be available.")
        Assert.IsNotNull(result.KenwardRogerWorkspace.Pmats, structureName & " KR P matrices should be available.")
        Assert.IsNotNull(result.KenwardRogerWorkspace.Qmats, structureName & " KR Q matrices should be available.")
        Assert.IsNotNull(result.KenwardRogerWorkspace.Rmats, structureName & " full KR R matrices should be available.")
        Assert.AreEqual(MixedModelKrParameterScale.MmrmTheta,
                        result.KenwardRogerWorkspace.ParameterScale,
                        structureName & " KR parameter scale should follow the R mmrm theta path.")
        Assert.IsNotNull(MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(result),
                         structureName & " KR-adjusted Var(beta) should be available.")
    End Sub


    Private Shared Function BuildBeshInternalRows(structureName As String,
                                                  res As MixedModelResult) As List(Of KrInternalRow)
        Dim rows As New List(Of KrInternalRow)()
        Dim ws As MixedModelKrWorkspace = res.KenwardRogerWorkspace
        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)
        Dim betaNames() As String = SafeNames(res.FixedEffectNames, res.P, "beta_")
        Dim thetaNames() As String = SafeNames(ws.CovarianceParameterNames, ws.K, "theta_")
        Dim thetaForDiagnostics() As Double = If(ws.Theta Is Nothing, res.Theta, ws.Theta)

        AddVectorRows(rows, structureName, "beta", "value", String.Empty, -1, -1, res.Beta, betaNames)
        AddVectorRows(rows, structureName, "theta", "value", String.Empty, -1, -1, thetaForDiagnostics, thetaNames)

        AddMatrixRows(rows, structureName, "varbeta_unadjusted", "matrix", String.Empty, -1, -1, res.VarBeta, betaNames, betaNames)
        AddMatrixRows(rows, structureName, "theta_vcov", "matrix", String.Empty, -1, -1, ws.ThetaCovariance, thetaNames, thetaNames)
        AddMatrixRows(rows, structureName, "varbeta_adjusted", "matrix", String.Empty, -1, -1, adjusted, betaNames, betaNames)
        AddMatrixRows(rows, structureName, "varbeta_kr_delta", "matrix", String.Empty, -1, -1, MatrixSubtract(adjusted, res.VarBeta), betaNames, betaNames)
        AddKrAdjustmentDecompositionRows(rows, structureName, ws, adjusted, betaNames)

        AddVectorRows(rows, structureName, "se_ordinary", "value", String.Empty, -1, -1, DiagonalStandardErrors(res.VarBeta), betaNames)
        AddVectorRows(rows, structureName, "se_kr", "value", String.Empty, -1, -1, DiagonalStandardErrors(adjusted), betaNames)
        AddVectorRows(rows, structureName, "se_kr_delta", "value", String.Empty, -1, -1, VectorSubtract(DiagonalStandardErrors(adjusted), DiagonalStandardErrors(res.VarBeta)), betaNames)

        For h As Integer = 0 To ws.K - 1
            AddMatrixRows(rows, structureName, "P", "matrix", String.Empty, h, -1, Slice3D(ws.Pmats, h), betaNames, betaNames)
        Next

        For h As Integer = 0 To ws.K - 1
            For j As Integer = 0 To ws.K - 1
                AddMatrixRows(rows, structureName, "Q", "matrix", String.Empty, h, j, Slice4D(ws.Qmats, h, j), betaNames, betaNames)
                AddMatrixRows(rows, structureName, "R", "matrix", String.Empty, h, j, Slice4D(ws.Rmats, h, j), betaNames, betaNames)
            Next
        Next

        For j As Integer = 0 To res.P - 1
            Dim l(0, res.P - 1) As Double
            l(0, j) = 1.0
            AddDfRows(rows, structureName, res, "coef:" & betaNames(j), l)
        Next

        Dim siteCentralIndex As Integer = IndexOfName(betaNames, "site_central")
        Dim siteSouthIndex As Integer = IndexOfName(betaNames, "site_south")
        If siteCentralIndex >= 0 AndAlso siteSouthIndex >= 0 Then
            Dim lSite(1, res.P - 1) As Double
            lSite(0, siteCentralIndex) = 1.0
            lSite(1, siteSouthIndex) = 1.0
            AddDfRows(rows, structureName, res, "term:clinic_site", lSite)
        End If

        Dim treatmentIndex As Integer = IndexOfName(betaNames, "treatment_active")
        Dim interactionIndex As Integer = IndexOfName(betaNames, "treatment_active:age_centered_8")
        If treatmentIndex >= 0 AndAlso interactionIndex >= 0 Then
            Dim lTreatment(1, res.P - 1) As Double
            lTreatment(0, treatmentIndex) = 1.0
            lTreatment(1, interactionIndex) = 1.0
            AddDfRows(rows, structureName, res, "joint:treatment_active+treatment_active:age_centered_8", lTreatment)
        End If

        If res.P > 1 Then
            Dim lAll(res.P - 2, res.P - 1) As Double
            For j As Integer = 1 To res.P - 1
                lAll(j - 1, j) = 1.0
            Next
            AddDfRows(rows, structureName, res, "joint:all_non_intercept", lAll)
        End If

        Return rows
    End Function


    Private Shared Sub AddDfRows(rows As List(Of KrInternalRow),
                                 structureName As String,
                                 res As MixedModelResult,
                                 label As String,
                                 lMatrix(,) As Double)
        Dim info As MixedModelKenwardRogerDfResult = Nothing
        Dim diagnostic As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrDegreesOfFreedomAndScaling(res,
                                                                                              lMatrix,
                                                                                              info,
                                                                                              diagnostic),
                      structureName & " KR DF/scaling components should be available for " & label & ": " & diagnostic)
        Assert.IsNotNull(info, structureName & " KR DF/scaling info should be returned for " & label & ".")

        AddScalarRow(rows, structureName, "df", "num_df", label, info.NumDF)
        AddScalarRow(rows, structureName, "df", "den_df", label, info.DenDF)
        AddScalarRow(rows, structureName, "df", "lambda", label, info.Lambda)
        AddScalarRow(rows, structureName, "df", "a1", label, info.A1)
        AddScalarRow(rows, structureName, "df", "a2", label, info.A2)
        AddScalarRow(rows, structureName, "df", "b", label, info.B)
        AddScalarRow(rows, structureName, "df", "e_star", label, info.EStar)
        AddScalarRow(rows, structureName, "df", "v_star", label, info.VStar)
        AddScalarRow(rows, structureName, "df", "rho", label, info.Rho)
    End Sub


    Private Shared Sub AddScalarRow(rows As List(Of KrInternalRow),
                                    structureName As String,
                                    kind As String,
                                    subkind As String,
                                    label As String,
                                    value As Double)
        rows.Add(New KrInternalRow With {
            .StructureName = If(structureName, String.Empty),
            .Kind = kind,
            .Subkind = subkind,
            .Label = If(label, String.Empty),
            .H = -1,
            .J = -1,
            .RowIndex = -1,
            .ColIndex = -1,
            .RowName = String.Empty,
            .ColName = String.Empty,
            .Value = value
        })
    End Sub


    Private Shared Sub AddVectorRows(rows As List(Of KrInternalRow),
                                     structureName As String,
                                     kind As String,
                                     subkind As String,
                                     label As String,
                                     h As Integer,
                                     j As Integer,
                                     values() As Double,
                                     names() As String)
        Assert.IsNotNull(values, structureName & " " & kind & " vector should not be Nothing.")
        For r As Integer = 0 To values.Length - 1
            rows.Add(New KrInternalRow With {
                .StructureName = If(structureName, String.Empty),
                .Kind = kind,
                .Subkind = subkind,
                .Label = If(label, String.Empty),
                .H = h,
                .J = j,
                .RowIndex = r,
                .ColIndex = -1,
                .RowName = names(r),
                .ColName = String.Empty,
                .Value = values(r)
            })
        Next
    End Sub


    Private Shared Sub AddMatrixRows(rows As List(Of KrInternalRow),
                                     structureName As String,
                                     kind As String,
                                     subkind As String,
                                     label As String,
                                     h As Integer,
                                     j As Integer,
                                     values(,) As Double,
                                     rowNames() As String,
                                     colNames() As String)
        Assert.IsNotNull(values, structureName & " " & kind & " matrix should not be Nothing.")

        For r As Integer = 0 To values.GetLength(0) - 1
            For c As Integer = 0 To values.GetLength(1) - 1
                rows.Add(New KrInternalRow With {
                    .StructureName = If(structureName, String.Empty),
                    .Kind = kind,
                    .Subkind = subkind,
                    .Label = If(label, String.Empty),
                    .H = h,
                    .J = j,
                    .RowIndex = r,
                    .ColIndex = c,
                    .RowName = rowNames(r),
                    .ColName = colNames(c),
                    .Value = values(r, c)
                })
            Next
        Next
    End Sub


    Private Shared Sub AddKrAdjustmentDecompositionRows(rows As List(Of KrInternalRow),
                                                        structureName As String,
                                                        ws As MixedModelKrWorkspace,
                                                        adjusted(,) As Double,
                                                        betaNames() As String)
        If ws Is Nothing OrElse ws.VarBeta Is Nothing OrElse ws.ThetaCovariance Is Nothing OrElse
           ws.Pmats Is Nothing OrElse ws.Qmats Is Nothing Then
            Return
        End If

        Dim p As Integer = ws.P
        Dim k As Integer = ws.K
        If p <= 0 OrElse k <= 0 Then Return
        If ws.VarBeta.GetLength(0) <> p OrElse ws.VarBeta.GetLength(1) <> p Then Return
        If ws.ThetaCovariance.GetLength(0) <> k OrElse ws.ThetaCovariance.GetLength(1) <> k Then Return
        If ws.Pmats.GetLength(0) <> k OrElse ws.Pmats.GetLength(1) <> p OrElse ws.Pmats.GetLength(2) <> p Then Return
        If ws.Qmats.GetLength(0) <> k OrElse ws.Qmats.GetLength(1) <> k OrElse ws.Qmats.GetLength(2) <> p OrElse ws.Qmats.GetLength(3) <> p Then Return

        Dim hasR As Boolean = ws.Rmats IsNot Nothing AndAlso
                              ws.Rmats.GetLength(0) = k AndAlso
                              ws.Rmats.GetLength(1) = k AndAlso
                              ws.Rmats.GetLength(2) = p AndAlso
                              ws.Rmats.GetLength(3) = p

        Dim linearDelta(p - 1, p - 1) As Double
        Dim secondDelta(p - 1, p - 1) As Double

        For h As Integer = 0 To k - 1
            For j As Integer = 0 To k - 1
                Dim whj As Double = ws.ThetaCovariance(h, j)
                Dim ph(,) As Double = Slice3D(ws.Pmats, h)
                Dim pj(,) As Double = Slice3D(ws.Pmats, j)
                Dim qhj(,) As Double = Slice4D(ws.Qmats, h, j)
                Dim phPhiPj(,) As Double = MatrixMultiply(MatrixMultiply(ph, ws.VarBeta), pj)
                Dim linearMiddle(,) As Double = MatrixSubtract(qhj, phPhiPj)
                Dim linearPairDelta(,) As Double = ScaleMatrix(MatrixMultiply(MatrixMultiply(ws.VarBeta, linearMiddle), ws.VarBeta), 2.0 * whj)

                AddMatrixRows(rows, structureName, "varbeta_kr_delta_linear_pair", "matrix", String.Empty, h, j, linearPairDelta, betaNames, betaNames)
                AddInPlace(linearDelta, linearPairDelta)

                Dim secondPairDelta(p - 1, p - 1) As Double
                If hasR Then
                    Dim rhj(,) As Double = Slice4D(ws.Rmats, h, j)
                    Dim secondMiddle(,) As Double = ScaleMatrix(rhj, -0.25)
                    secondPairDelta = ScaleMatrix(MatrixMultiply(MatrixMultiply(ws.VarBeta, secondMiddle), ws.VarBeta), 2.0 * whj)
                End If

                AddMatrixRows(rows, structureName, "varbeta_kr_delta_second_pair", "matrix", String.Empty, h, j, secondPairDelta, betaNames, betaNames)
                AddInPlace(secondDelta, secondPairDelta)
            Next
        Next

        Dim reconstructedDelta(,) As Double = MatrixAdd(linearDelta, secondDelta)
        Dim reconstructedAdjusted(,) As Double = MatrixAdd(ws.VarBeta, reconstructedDelta)

        AddMatrixRows(rows, structureName, "varbeta_kr_delta_linear", "matrix", String.Empty, -1, -1, linearDelta, betaNames, betaNames)
        AddMatrixRows(rows, structureName, "varbeta_kr_delta_second", "matrix", String.Empty, -1, -1, secondDelta, betaNames, betaNames)
        AddMatrixRows(rows, structureName, "varbeta_kr_delta_reconstructed", "matrix", String.Empty, -1, -1, reconstructedDelta, betaNames, betaNames)
        AddMatrixRows(rows, structureName, "varbeta_adjusted_reconstructed", "matrix", String.Empty, -1, -1, reconstructedAdjusted, betaNames, betaNames)
        AddMatrixRows(rows, structureName, "varbeta_adjusted_reconstruction_error", "matrix", String.Empty, -1, -1, MatrixSubtract(reconstructedAdjusted, adjusted), betaNames, betaNames)
    End Sub


    Private Shared Sub AddInPlace(target(,) As Double,
                                  addend(,) As Double)
        Assert.IsNotNull(target, "target matrix should not be Nothing.")
        Assert.IsNotNull(addend, "addend matrix should not be Nothing.")
        Assert.AreEqual(target.GetLength(0), addend.GetLength(0), "matrix row dimensions should match.")
        Assert.AreEqual(target.GetLength(1), addend.GetLength(1), "matrix column dimensions should match.")

        For r As Integer = 0 To target.GetLength(0) - 1
            For c As Integer = 0 To target.GetLength(1) - 1
                target(r, c) += addend(r, c)
            Next
        Next
    End Sub


    Private Shared Function MatrixMultiply(left(,) As Double,
                                           right(,) As Double) As Double(,)
        Assert.IsNotNull(left, "left matrix should not be Nothing.")
        Assert.IsNotNull(right, "right matrix should not be Nothing.")
        Assert.AreEqual(left.GetLength(1), right.GetLength(0), "matrix inner dimensions should match.")

        Dim output(left.GetLength(0) - 1, right.GetLength(1) - 1) As Double
        For r As Integer = 0 To left.GetLength(0) - 1
            For c As Integer = 0 To right.GetLength(1) - 1
                Dim value As Double = 0.0
                For m As Integer = 0 To left.GetLength(1) - 1
                    value += left(r, m) * right(m, c)
                Next
                output(r, c) = value
            Next
        Next

        Return output
    End Function


    Private Shared Function ScaleMatrix(values(,) As Double,
                                        factor As Double) As Double(,)
        Assert.IsNotNull(values, "matrix should not be Nothing.")
        Dim output(values.GetLength(0) - 1, values.GetLength(1) - 1) As Double
        For r As Integer = 0 To values.GetLength(0) - 1
            For c As Integer = 0 To values.GetLength(1) - 1
                output(r, c) = factor * values(r, c)
            Next
        Next
        Return output
    End Function


    Private Shared Function MatrixAdd(left(,) As Double,
                                      right(,) As Double) As Double(,)
        Assert.IsNotNull(left, "left matrix should not be Nothing.")
        Assert.IsNotNull(right, "right matrix should not be Nothing.")
        Assert.AreEqual(left.GetLength(0), right.GetLength(0), "matrix row dimensions should match.")
        Assert.AreEqual(left.GetLength(1), right.GetLength(1), "matrix column dimensions should match.")

        Dim output(left.GetLength(0) - 1, left.GetLength(1) - 1) As Double
        For r As Integer = 0 To left.GetLength(0) - 1
            For c As Integer = 0 To left.GetLength(1) - 1
                output(r, c) = left(r, c) + right(r, c)
            Next
        Next
        Return output
    End Function


    Private Shared Function MatrixSubtract(left(,) As Double,
                                           right(,) As Double) As Double(,)
        Assert.IsNotNull(left, "left matrix should not be Nothing.")
        Assert.IsNotNull(right, "right matrix should not be Nothing.")
        Assert.AreEqual(left.GetLength(0), right.GetLength(0), "matrix row dimensions should match.")
        Assert.AreEqual(left.GetLength(1), right.GetLength(1), "matrix column dimensions should match.")

        Dim output(left.GetLength(0) - 1, left.GetLength(1) - 1) As Double
        For r As Integer = 0 To left.GetLength(0) - 1
            For c As Integer = 0 To left.GetLength(1) - 1
                output(r, c) = left(r, c) - right(r, c)
            Next
        Next

        Return output
    End Function


    Private Shared Function DiagonalStandardErrors(values(,) As Double) As Double()
        Assert.IsNotNull(values, "variance matrix should not be Nothing.")
        Dim n As Integer = Math.Min(values.GetLength(0), values.GetLength(1))
        Dim output(n - 1) As Double
        For i As Integer = 0 To n - 1
            output(i) = Math.Sqrt(Math.Max(0.0, values(i, i)))
        Next
        Return output
    End Function


    Private Shared Function VectorSubtract(left() As Double,
                                           right() As Double) As Double()
        Assert.IsNotNull(left, "left vector should not be Nothing.")
        Assert.IsNotNull(right, "right vector should not be Nothing.")
        Assert.AreEqual(left.Length, right.Length, "vector dimensions should match.")

        Dim output(left.Length - 1) As Double
        For i As Integer = 0 To left.Length - 1
            output(i) = left(i) - right(i)
        Next
        Return output
    End Function


    Private Sub WriteComparisonReports(actualRows As List(Of KrInternalRow),
                                       referenceRows As List(Of KrInternalRow))
        Try
            Dim details As List(Of KrInternalComparisonRow) = BuildComparisonRows(actualRows, referenceRows)
            WriteComparisonDetails(details, "besh_vs_r_mmrm_kr_internal_comparison_details.csv")
            WriteComparisonSummary(details, "besh_vs_r_mmrm_kr_internal_comparison_summary.csv")
            WriteAdjustmentPairSummary(details, "besh_vs_r_mmrm_kr_adjustment_pair_summary.csv")
        Catch ex As Exception
            If Me.TestContext IsNot Nothing Then
                Me.TestContext.WriteLine("Could not write KR internal comparison reports: " & ex.ToString())
            End If
        End Try
    End Sub


    Private Shared Function BuildComparisonRows(actualRows As List(Of KrInternalRow),
                                                referenceRows As List(Of KrInternalRow)) As List(Of KrInternalComparisonRow)
        Dim actualByKey As New Dictionary(Of String, KrInternalRow)(StringComparer.OrdinalIgnoreCase)
        For Each one As KrInternalRow In actualRows
            If one IsNot Nothing AndAlso Not actualByKey.ContainsKey(one.Key) Then
                actualByKey.Add(one.Key, one)
            End If
        Next

        Dim details As New List(Of KrInternalComparisonRow)()

        For Each expected As KrInternalRow In referenceRows
            If expected Is Nothing OrElse Not IsTargetStructure(expected.StructureName) Then Continue For

            Dim actual As KrInternalRow = Nothing
            Dim hasActual As Boolean = actualByKey.TryGetValue(expected.Key, actual)
            Dim absTol As Double = 0.0
            Dim relTol As Double = 0.0
            GetTolerances(expected, absTol, relTol)

            Dim diff As Double = Double.NaN
            Dim absDiff As Double = Double.NaN
            Dim relDiff As Double = Double.NaN
            Dim allowed As Double = Math.Max(absTol, Math.Abs(expected.Value) * relTol)
            Dim normalized As Double = Double.NaN
            Dim status As String = "missing_actual"

            If hasActual Then
                diff = actual.Value - expected.Value
                absDiff = Math.Abs(diff)
                If Math.Abs(expected.Value) > 0.0 Then relDiff = absDiff / Math.Abs(expected.Value)
                If allowed > 0.0 Then normalized = absDiff / allowed

                If Not IsFinite(expected.Value) Then
                    status = "reference_not_finite"
                ElseIf Not IsFinite(actual.Value) Then
                    status = "actual_not_finite"
                ElseIf IsAssertableReferenceRow(expected, False) Then
                    status = If(absDiff <= allowed, "within_default_tolerance", "outside_default_tolerance")
                ElseIf IsAssertableReferenceRow(expected, True) Then
                    status = "raw_parameterization_sensitive"
                Else
                    status = "not_compared"
                End If
            End If

            details.Add(New KrInternalComparisonRow With {
                .StructureName = expected.StructureName,
                .Kind = expected.Kind,
                .Subkind = expected.Subkind,
                .Label = expected.Label,
                .H = expected.H,
                .J = expected.J,
                .RowIndex = expected.RowIndex,
                .ColIndex = expected.ColIndex,
                .RowName = expected.RowName,
                .ColName = expected.ColName,
                .ExpectedValue = expected.Value,
                .ActualValue = If(hasActual, actual.Value, Double.NaN),
                .Diff = diff,
                .AbsDiff = absDiff,
                .RelDiff = relDiff,
                .AbsTolerance = absTol,
                .RelTolerance = relTol,
                .Allowed = allowed,
                .NormalizedDiff = normalized,
                .AssertedByDefault = IsAssertableReferenceRow(expected, False),
                .RawParameterizationSensitive = (Not IsAssertableReferenceRow(expected, False)) AndAlso IsAssertableReferenceRow(expected, True),
                .Status = status
            })
        Next

        Return details
    End Function


    Private Sub WriteComparisonDetails(details As List(Of KrInternalComparisonRow),
                                       fileName As String)
        Dim outDir As String = GetExportDirectory()
        Dim path As String = System.IO.Path.Combine(outDir, fileName)
        Dim sb As New StringBuilder()

        sb.AppendLine("structure,kind,subkind,label,h,j,row,col,row_name,col_name,expected,actual,diff,abs_diff,rel_diff,abs_tol,rel_tol,allowed,normalized_diff,asserted_by_default,raw_parameterization_sensitive,status")
        For Each one As KrInternalComparisonRow In details.OrderBy(Function(x) x.StructureName, StringComparer.OrdinalIgnoreCase).
                                                        ThenBy(Function(x) x.Kind, StringComparer.OrdinalIgnoreCase).
                                                        ThenBy(Function(x) x.Subkind, StringComparer.OrdinalIgnoreCase).
                                                        ThenByDescending(Function(x) If(Double.IsNaN(x.NormalizedDiff), -1.0, x.NormalizedDiff))
            sb.AppendLine(String.Join(",",
                                      Csv(one.StructureName),
                                      Csv(one.Kind),
                                      Csv(one.Subkind),
                                      Csv(one.Label),
                                      Csv(one.H),
                                      Csv(one.J),
                                      Csv(one.RowIndex),
                                      Csv(one.ColIndex),
                                      Csv(one.RowName),
                                      Csv(one.ColName),
                                      Csv(one.ExpectedValue),
                                      Csv(one.ActualValue),
                                      Csv(one.Diff),
                                      Csv(one.AbsDiff),
                                      Csv(one.RelDiff),
                                      Csv(one.AbsTolerance),
                                      Csv(one.RelTolerance),
                                      Csv(one.Allowed),
                                      Csv(one.NormalizedDiff),
                                      Csv(one.AssertedByDefault),
                                      Csv(one.RawParameterizationSensitive),
                                      Csv(one.Status)))
        Next

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8)
        AttachComparisonReport(path, "Wrote KR internal comparison detail CSV: ")
    End Sub


    Private Sub WriteComparisonSummary(details As List(Of KrInternalComparisonRow),
                                       fileName As String)
        Dim outDir As String = GetExportDirectory()
        Dim path As String = System.IO.Path.Combine(outDir, fileName)
        Dim sb As New StringBuilder()

        sb.AppendLine("structure,layer,kind,subkind,asserted_by_default,raw_parameterization_sensitive,n,max_abs_diff,max_rel_diff,max_normalized_diff,rms_abs_diff,mean_signed_diff,outside_default_tolerance_count,max_diff_key,max_diff_row_name,max_diff_col_name,max_diff_label")

        Dim groups = details.GroupBy(Function(x) New With {
                                      Key .StructureName = x.StructureName,
                                      Key .Kind = x.Kind,
                                      Key .Subkind = x.Subkind,
                                      Key .AssertedByDefault = x.AssertedByDefault,
                                      Key .RawParameterizationSensitive = x.RawParameterizationSensitive})

        For Each grp In groups.OrderBy(Function(g) LayerRank(g.Key.Kind)).
                              ThenBy(Function(g) g.Key.StructureName, StringComparer.OrdinalIgnoreCase).
                              ThenBy(Function(g) g.Key.Kind, StringComparer.OrdinalIgnoreCase).
                              ThenBy(Function(g) g.Key.Subkind, StringComparer.OrdinalIgnoreCase)
            Dim finiteRows As List(Of KrInternalComparisonRow) = grp.Where(Function(x) IsFinite(x.AbsDiff)).ToList()
            If finiteRows.Count = 0 Then Continue For

            Dim maxAbsRow As KrInternalComparisonRow = finiteRows.OrderByDescending(Function(x) x.AbsDiff).First()
            Dim maxRel As Double = finiteRows.Where(Function(x) IsFinite(x.RelDiff)).Select(Function(x) x.RelDiff).DefaultIfEmpty(Double.NaN).Max()
            Dim maxNorm As Double = finiteRows.Where(Function(x) IsFinite(x.NormalizedDiff)).Select(Function(x) x.NormalizedDiff).DefaultIfEmpty(Double.NaN).Max()
            Dim rms As Double = Math.Sqrt(finiteRows.Select(Function(x) x.AbsDiff * x.AbsDiff).Average())
            Dim meanSigned As Double = finiteRows.Select(Function(x) x.Diff).Average()
            Dim outsideCount As Integer = finiteRows.Select(Function(x) String.Equals(x.Status, "outside_default_tolerance", StringComparison.OrdinalIgnoreCase)).Count

            sb.AppendLine(String.Join(",",
                                      Csv(maxAbsRow.StructureName),
                                      Csv(LayerName(maxAbsRow.Kind)),
                                      Csv(maxAbsRow.Kind),
                                      Csv(maxAbsRow.Subkind),
                                      Csv(grp.Key.AssertedByDefault),
                                      Csv(grp.Key.RawParameterizationSensitive),
                                      Csv(finiteRows.Count),
                                      Csv(maxAbsRow.AbsDiff),
                                      Csv(maxRel),
                                      Csv(maxNorm),
                                      Csv(rms),
                                      Csv(meanSigned),
                                      Csv(outsideCount),
                                      Csv(maxAbsRow.Key),
                                      Csv(maxAbsRow.RowName),
                                      Csv(maxAbsRow.ColName),
                                      Csv(maxAbsRow.Label)))
        Next

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8)
        AttachComparisonReport(path, "Wrote KR internal comparison summary CSV: ")
    End Sub


    Private Sub WriteAdjustmentPairSummary(details As List(Of KrInternalComparisonRow),
                                           fileName As String)
        Dim pairRows As List(Of KrInternalComparisonRow) = details.Where(Function(x) IsAdjustmentPairKind(x.Kind) AndAlso IsFinite(x.AbsDiff)).ToList()
        If pairRows.Count = 0 Then Return

        Dim outDir As String = GetExportDirectory()
        Dim path As String = System.IO.Path.Combine(outDir, fileName)
        Dim sb As New StringBuilder()

        sb.AppendLine("structure,kind,h,j,n,max_abs_diff,max_rel_diff,max_normalized_diff,rms_abs_diff,mean_signed_diff,sum_abs_expected,sum_abs_actual,sum_abs_diff,max_diff_key,max_diff_row_name,max_diff_col_name")

        Dim groups = pairRows.GroupBy(Function(x) New With {
                                      Key .StructureName = x.StructureName,
                                      Key .Kind = x.Kind,
                                      Key .H = x.H,
                                      Key .J = x.J})

        For Each grp In groups.OrderBy(Function(g) g.Key.StructureName, StringComparer.OrdinalIgnoreCase).
                              ThenBy(Function(g) If(String.Equals(g.Key.Kind, "varbeta_kr_delta_second_pair", StringComparison.OrdinalIgnoreCase), 0, 1)).
                              ThenByDescending(Function(g) g.Max(Function(x) x.AbsDiff))
            Dim finiteRows As List(Of KrInternalComparisonRow) = grp.ToList()
            Dim maxAbsRow As KrInternalComparisonRow = finiteRows.OrderByDescending(Function(x) x.AbsDiff).First()
            Dim maxRel As Double = finiteRows.Where(Function(x) IsFinite(x.RelDiff)).Select(Function(x) x.RelDiff).DefaultIfEmpty(Double.NaN).Max()
            Dim maxNorm As Double = finiteRows.Where(Function(x) IsFinite(x.NormalizedDiff)).Select(Function(x) x.NormalizedDiff).DefaultIfEmpty(Double.NaN).Max()
            Dim rms As Double = Math.Sqrt(finiteRows.Select(Function(x) x.AbsDiff * x.AbsDiff).Average())
            Dim meanSigned As Double = finiteRows.Select(Function(x) x.Diff).Average()
            Dim sumExpected As Double = finiteRows.Select(Function(x) Math.Abs(x.ExpectedValue)).Sum()
            Dim sumActual As Double = finiteRows.Select(Function(x) Math.Abs(x.ActualValue)).Sum()
            Dim sumDiff As Double = finiteRows.Select(Function(x) Math.Abs(x.Diff)).Sum()

            sb.AppendLine(String.Join(",",
                                      Csv(maxAbsRow.StructureName),
                                      Csv(maxAbsRow.Kind),
                                      Csv(maxAbsRow.H),
                                      Csv(maxAbsRow.J),
                                      Csv(finiteRows.Count),
                                      Csv(maxAbsRow.AbsDiff),
                                      Csv(maxRel),
                                      Csv(maxNorm),
                                      Csv(rms),
                                      Csv(meanSigned),
                                      Csv(sumExpected),
                                      Csv(sumActual),
                                      Csv(sumDiff),
                                      Csv(maxAbsRow.Key),
                                      Csv(maxAbsRow.RowName),
                                      Csv(maxAbsRow.ColName)))
        Next

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8)
        AttachComparisonReport(path, "Wrote KR internal pair-contribution summary CSV: ")
    End Sub


    Private Shared Function IsAdjustmentPairKind(kind As String) As Boolean
        Return String.Equals(kind, "varbeta_kr_delta_linear_pair", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(kind, "varbeta_kr_delta_second_pair", StringComparison.OrdinalIgnoreCase)
    End Function


    Private Sub AttachComparisonReport(path As String,
                                       messagePrefix As String)
        If Me.TestContext Is Nothing Then Return
        Me.TestContext.WriteLine(messagePrefix & path)
        Try
            Me.TestContext.AddResultFile(path)
        Catch ex As Exception
            Me.TestContext.WriteLine("Could not attach comparison report: " & ex.Message)
        End Try
    End Sub


    Private Shared Function LayerName(kind As String) As String
        Select Case If(kind, String.Empty).ToLowerInvariant()
            Case "beta"
                Return "01 fixed effects"
            Case "theta"
                Return "02 covariance parameters raw"
            Case "varbeta_unadjusted", "se_ordinary"
                Return "03 ordinary covariance"
            Case "theta_vcov"
                Return "04 covariance-parameter vcov raw"
            Case "p", "q", "r"
                Return "05 KR derivative matrices raw"
            Case "varbeta_kr_delta", "se_kr_delta"
                Return "06 KR covariance adjustment"
            Case "varbeta_kr_delta_linear", "varbeta_kr_delta_second", "varbeta_kr_delta_linear_pair", "varbeta_kr_delta_second_pair", "varbeta_kr_delta_reconstructed"
                Return "06 KR covariance adjustment raw split"
            Case "varbeta_adjusted", "se_kr"
                Return "07 KR adjusted covariance"
            Case "df"
                Return "08 KR DF/scaling"
            Case Else
                Return "99 other"
        End Select
    End Function


    Private Shared Function LayerRank(kind As String) As Integer
        Select Case If(kind, String.Empty).ToLowerInvariant()
            Case "beta"
                Return 10
            Case "theta"
                Return 20
            Case "varbeta_unadjusted", "se_ordinary"
                Return 30
            Case "theta_vcov"
                Return 40
            Case "p", "q", "r"
                Return 50
            Case "varbeta_kr_delta", "varbeta_kr_delta_linear", "varbeta_kr_delta_second", "varbeta_kr_delta_linear_pair", "varbeta_kr_delta_second_pair", "varbeta_kr_delta_reconstructed", "se_kr_delta"
                Return 60
            Case "varbeta_adjusted", "varbeta_adjusted_reconstructed", "varbeta_adjusted_reconstruction_error", "se_kr"
                Return 70
            Case "df"
                Return 80
            Case Else
                Return 990
        End Select
    End Function


    Private Shared Sub AssertRowsAgainstReference(actualRows As List(Of KrInternalRow),
                                                  referenceRows As List(Of KrInternalRow))
        Dim actualByKey As New Dictionary(Of String, KrInternalRow)(StringComparer.OrdinalIgnoreCase)
        For Each one As KrInternalRow In actualRows
            If Not actualByKey.ContainsKey(one.Key) Then
                actualByKey.Add(one.Key, one)
            End If
        Next

        Dim compareRawInternal As Boolean = CompareRawInternalMode()
        Dim failures As New List(Of String)()
        Dim checked As Integer = 0
        Dim skippedRaw As Integer = 0

        For Each expected As KrInternalRow In referenceRows
            If Not IsTargetStructure(expected.StructureName) Then Continue For

            If Not IsAssertableReferenceRow(expected, compareRawInternal) Then
                skippedRaw += 1
                Continue For
            End If

            Dim actual As KrInternalRow = Nothing
            If Not actualByKey.TryGetValue(expected.Key, actual) Then
                failures.Add("missing actual row for " & expected.Key &
                             " [row_name=" & expected.RowName & ", col_name=" & expected.ColName & "]")
                Continue For
            End If

            checked += 1

            Dim absTol As Double = 0.0
            Dim relTol As Double = 0.0
            GetTolerances(expected, absTol, relTol)

            If Not IsFinite(expected.Value) Then
                failures.Add(expected.Key & " reference value is not finite: " & expected.Value.ToString("G17", CultureInfo.InvariantCulture))
                Continue For
            End If

            If Not IsFinite(actual.Value) Then
                failures.Add(expected.Key & " actual value is not finite. Expected " & expected.Value.ToString("G17", CultureInfo.InvariantCulture))
                Continue For
            End If

            Dim allowed As Double = Math.Max(absTol, Math.Abs(expected.Value) * relTol)
            Dim diff As Double = Math.Abs(expected.Value - actual.Value)
            If diff > allowed Then
                failures.Add(expected.Key &
                             " [row_name=" & expected.RowName & ", col_name=" & expected.ColName & "] expected " &
                             expected.Value.ToString("G17", CultureInfo.InvariantCulture) &
                             ", actual " & actual.Value.ToString("G17", CultureInfo.InvariantCulture) &
                             ", abs diff " & diff.ToString("G17", CultureInfo.InvariantCulture) &
                             " > allowed " & allowed.ToString("G17", CultureInfo.InvariantCulture) &
                             " (abs=" & absTol.ToString("G17", CultureInfo.InvariantCulture) &
                             ", rel=" & relTol.ToString("G17", CultureInfo.InvariantCulture) & ")")
            End If
        Next

        Assert.IsTrue(checked > 0, "No R mmrm internal KR reference rows were checked.")

        If failures.Count > 0 Then
            Dim shown As Integer = Math.Min(35, failures.Count)
            Dim sb As New StringBuilder()
            sb.AppendLine("R mmrm internal KR parity failures: " & failures.Count.ToString(CultureInfo.InvariantCulture))
            sb.AppendLine("Checked rows: " & checked.ToString(CultureInfo.InvariantCulture) &
                          "; skipped raw parameterization-sensitive rows: " & skippedRaw.ToString(CultureInfo.InvariantCulture) & ".")
            sb.AppendLine("Set " & COMPARE_RAW_INTERNAL_ENV & "=1 only when the R reference CSV has been converted to the BESH KR workspace parameter/sign convention.")
            AppendFailureSummary(sb, failures)
            For i As Integer = 0 To shown - 1
                sb.AppendLine("  - " & failures(i))
            Next
            If failures.Count > shown Then
                sb.AppendLine("  ... " & (failures.Count - shown).ToString(CultureInfo.InvariantCulture) & " more")
            End If
            Assert.Fail(sb.ToString())
        End If
    End Sub


    Private Shared Function IsAssertableReferenceRow(row As KrInternalRow,
                                                     compareRawInternal As Boolean) As Boolean
        If row Is Nothing OrElse String.IsNullOrWhiteSpace(row.Kind) Then Return False

        Select Case row.Kind.ToLowerInvariant()
            Case "beta", "varbeta_unadjusted", "varbeta_adjusted", "varbeta_kr_delta", "varbeta_kr_delta_reconstructed", "varbeta_adjusted_reconstructed", "varbeta_adjusted_reconstruction_error", "se_ordinary", "se_kr", "se_kr_delta", "df"
                Return True

            Case "varbeta_kr_delta_linear", "varbeta_kr_delta_second", "varbeta_kr_delta_linear_pair", "varbeta_kr_delta_second_pair"
                ' The split between the first-derivative P/Q contribution and the
                ' second-derivative R contribution is not invariant under a nonlinear
                ' covariance-parameter reparameterization.  The full reconstructed
                ' KR delta is comparable; the split is diagnostic-only unless raw
                ' internals are deliberately requested.
                Return compareRawInternal

            Case "theta", "theta_vcov", "p", "q", "r"
                ' R mmrm exposes these on its own internal theta/sign convention:
                '   P_R = -X' V^-1 (dV/dtheta_R) V^-1 X.
                ' The BESH KR workspace stores positive derivative matrices on the
                ' BESH optimizer/MmrmTheta scale.  For example, for log-variance vs
                ' log-standard-deviation parameters this alone creates the apparent
                ' AR(1) relation actual = -0.5 * R_reference for P and actual = 0.25
                ' * R_reference for Q/R.  Compare these raw rows only after the R
                ' exporter writes BESH-convention rows or when deliberately debugging
                ' the parameterization mapping.
                Return compareRawInternal

            Case Else
                Return False
        End Select
    End Function


    Private Shared Function CompareRawInternalMode() As Boolean
        Dim value As String = Environment.GetEnvironmentVariable(COMPARE_RAW_INTERNAL_ENV)
        If String.IsNullOrWhiteSpace(value) Then Return False
        value = value.Trim()
        Return String.Equals(value, "1", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(value, "true", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
    End Function


    Private Shared Sub AppendFailureSummary(sb As StringBuilder,
                                            failures As List(Of String))
        If sb Is Nothing OrElse failures Is Nothing OrElse failures.Count = 0 Then Return

        Dim counts As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For Each failure As String In failures
            Dim parts() As String = failure.Split("|"c)
            Dim key As String = If(parts.Length >= 2, parts(0) & " / " & parts(1), "other")
            If counts.ContainsKey(key) Then
                counts(key) += 1
            Else
                counts.Add(key, 1)
            End If
        Next

        sb.AppendLine("Failure summary by structure/kind:")
        For Each kvp As KeyValuePair(Of String, Integer) In counts.OrderBy(Function(x) x.Key, StringComparer.OrdinalIgnoreCase)
            sb.AppendLine("  * " & kvp.Key & ": " & kvp.Value.ToString(CultureInfo.InvariantCulture))
        Next
    End Sub


    Private Shared Function IsTargetStructure(structureName As String) As Boolean
        For Each target As String In TARGET_STRUCTURES_INTERNAL
            If String.Equals(target, structureName, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function


    Private Shared Sub GetTolerances(row As KrInternalRow,
                                     ByRef absTol As Double,
                                     ByRef relTol As Double)
        Select Case row.Kind.ToLowerInvariant()
            Case "beta"
                absTol = 0.0002
                relTol = 0.00001
            Case "theta"
                absTol = 0.00075
                relTol = 0.001
            Case "varbeta_unadjusted"
                absTol = 0.00025
                relTol = 0.0005
            Case "theta_vcov"
                absTol = 0.001
                relTol = 0.01
            Case "p"
                absTol = 0.001
                relTol = 0.01
            Case "q"
                absTol = 0.01
                relTol = 0.03
            Case "r"
                absTol = 0.05
                relTol = 0.08
            Case "varbeta_adjusted", "varbeta_adjusted_reconstructed"
                absTol = 0.008
                relTol = 0.01
            Case "varbeta_kr_delta", "varbeta_kr_delta_linear", "varbeta_kr_delta_second", "varbeta_kr_delta_linear_pair", "varbeta_kr_delta_second_pair", "varbeta_kr_delta_reconstructed"
                absTol = 0.008
                relTol = 0.03
            Case "varbeta_adjusted_reconstruction_error"
                absTol = 0.00000001
                relTol = 0.0
            Case "se_ordinary"
                absTol = 0.00015
                relTol = 0.0005
            Case "se_kr"
                absTol = 0.004
                relTol = 0.006
            Case "se_kr_delta"
                absTol = 0.004
                relTol = 0.05
            Case "df"
                Select Case row.Subkind.ToLowerInvariant()
                    Case "den_df"
                        absTol = 0.03
                        relTol = 0.0015
                    Case "lambda"
                        absTol = 0.00075
                        relTol = 0.001
                    Case "num_df"
                        absTol = 0.000000001
                        relTol = 0.0
                    Case Else
                        absTol = 0.004
                        relTol = 0.015
                End Select
            Case Else
                absTol = 0.000001
                relTol = 0.000001
        End Select
    End Sub


    Private Shared Function LoadReferenceRows(path As String) As List(Of KrInternalRow)
        Dim lines() As String = File.ReadAllLines(path)
        Dim rows As New List(Of KrInternalRow)()
        If lines.Length <= 1 Then Return rows

        Dim header() As String = ParseCsvLine(lines(0))
        Dim col As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            col(header(i)) = i
        Next

        Dim required() As String = {"structure", "kind", "subkind", "label", "h", "j", "row", "col", "row_name", "col_name", "value"}
        For Each name As String In required
            If Not col.ContainsKey(name) Then
                Throw New InvalidOperationException("Reference CSV is missing required column: " & name)
            End If
        Next

        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
            Dim parts() As String = ParseCsvLine(lines(i))

            rows.Add(New KrInternalRow With {
                .StructureName = Field(parts, col("structure")),
                .Kind = Field(parts, col("kind")),
                .Subkind = Field(parts, col("subkind")),
                .Label = Field(parts, col("label")),
                .H = ParseInteger(Field(parts, col("h"))),
                .J = ParseInteger(Field(parts, col("j"))),
                .RowIndex = ParseInteger(Field(parts, col("row"))),
                .ColIndex = ParseInteger(Field(parts, col("col"))),
                .RowName = Field(parts, col("row_name")),
                .ColName = Field(parts, col("col_name")),
                .Value = ParseDoubleInternal(Field(parts, col("value")))
            })
        Next

        Return rows
    End Function


    Private Sub WriteDiagnosticRows(rows As List(Of KrInternalRow), fileName As String)
        Try
            Dim outDir As String = GetExportDirectory()
            Dim path As String = System.IO.Path.Combine(outDir, fileName)

            Dim sb As New StringBuilder()
            sb.AppendLine("structure,kind,subkind,label,h,j,row,col,row_name,col_name,value")
            For Each one As KrInternalRow In rows
                sb.AppendLine(String.Join(",",
                                          Csv(one.StructureName),
                                          Csv(one.Kind),
                                          Csv(one.Subkind),
                                          Csv(one.Label),
                                          Csv(one.H),
                                          Csv(one.J),
                                          Csv(one.RowIndex),
                                          Csv(one.ColIndex),
                                          Csv(one.RowName),
                                          Csv(one.ColName),
                                          Csv(one.Value)))
            Next

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8)

            If Me.TestContext IsNot Nothing Then
                Me.TestContext.WriteLine("Wrote BESH internal KR diagnostic CSV: " & path)
                Try
                    Me.TestContext.AddResultFile(path)
                Catch ex As Exception
                    Me.TestContext.WriteLine("Could not attach internal diagnostic CSV: " & ex.Message)
                End Try
            End If
        Catch ex As Exception
            If Me.TestContext IsNot Nothing Then
                Me.TestContext.WriteLine("Could not write BESH internal KR diagnostic CSV: " & ex.ToString())
            End If
        End Try
    End Sub


    Private Function GetExportDirectory() As String
        Dim explicitDir As String = Environment.GetEnvironmentVariable("BESHSTAT_KR_EXPORT_DIR")

        If Not String.IsNullOrWhiteSpace(explicitDir) Then
            Directory.CreateDirectory(explicitDir)
            Return explicitDir
        End If

        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim dir As DirectoryInfo = New DirectoryInfo(baseDir)

        While dir IsNot Nothing
            Dim testDataDir As String = Path.Combine(dir.FullName, "TestData")
            If Directory.Exists(testDataDir) Then
                Dim exportDir As String = Path.Combine(dir.FullName, "KRValidationExports")
                Directory.CreateDirectory(exportDir)
                Return exportDir
            End If
            dir = dir.Parent
        End While

        Dim fallback As String = Path.Combine(Path.GetTempPath(), "BESHStatNG_KRValidationExports")
        Directory.CreateDirectory(fallback)
        Return fallback
    End Function


    Private Shared Function StrictInternalReferenceMode() As Boolean
        Dim raw As String = Environment.GetEnvironmentVariable(STRICT_ENV_INTERNAL)
        Return String.Equals(raw, "1", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(raw, "true", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase)
    End Function


    Private Shared Function TryFindTestDataPathInternal(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim dir As DirectoryInfo = New DirectoryInfo(baseDir)

        While dir IsNot Nothing
            Dim candidate As String = Path.Combine(dir.FullName, "TestData", fileName)
            If File.Exists(candidate) Then Return candidate
            dir = dir.Parent
        End While

        Return String.Empty
    End Function


    Private Shared Function GetTestDataPathInternal(fileName As String) As String
        Dim path As String = TryFindTestDataPathInternal(fileName)
        If Not String.IsNullOrWhiteSpace(path) Then Return path
        Throw New FileNotFoundException("Could not locate TestData file: " & fileName)
    End Function


    Private Shared Function LoadModelDataInternal() As ModelDataInternal
        Dim rows As List(Of Dictionary(Of String, String)) = LoadRowsInternal()
        rows = rows.FindAll(Function(r) Not IsMissingInternal(r("distance_mm")))

        Dim n As Integer = rows.Count
        Dim y(n - 1) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double
        Dim x(n - 1, 6) As Double

        For i As Integer = 0 To n - 1
            Dim r As Dictionary(Of String, String) = rows(i)

            Dim sexCode As Double = ParseDoubleInternal(r("sex_code"))
            Dim active As Double = If(String.Equals(r("treatment_arm"), "Active", StringComparison.OrdinalIgnoreCase), 1.0, 0.0)
            Dim siteCentral As Double = If(String.Equals(r("clinic_site"), "Central", StringComparison.OrdinalIgnoreCase), 1.0, 0.0)
            Dim siteSouth As Double = If(String.Equals(r("clinic_site"), "South", StringComparison.OrdinalIgnoreCase), 1.0, 0.0)
            Dim ageCentered As Double = ParseDoubleInternal(r("age_centered_8"))

            y(i) = ParseDoubleInternal(r("distance_mm"))
            subject(i) = r("subject_id")
            visit(i) = ParseDoubleInternal(r("visit"))

            x(i, 0) = 1.0
            x(i, 1) = sexCode
            x(i, 2) = active
            x(i, 3) = siteCentral
            x(i, 4) = siteSouth
            x(i, 5) = ageCentered
            x(i, 6) = active * ageCentered
        Next

        Return New ModelDataInternal With {
            .Y = y,
            .X = x,
            .SubjectId = subject,
            .Visit = visit
        }
    End Function


    Private Shared Function LoadRowsInternal() As List(Of Dictionary(Of String, String))
        Dim path As String = GetTestDataPathInternal(DATA_FILE_INTERNAL)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then
            Throw New InvalidOperationException(DATA_FILE_INTERNAL & " must contain a header and at least one data row.")
        End If

        Dim header() As String = ParseCsvLine(lines(0))
        Dim rows As New List(Of Dictionary(Of String, String))()

        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For

            Dim parts() As String = ParseCsvLine(lines(i))
            Dim row As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

            For j As Integer = 0 To header.Length - 1
                Dim value As String = String.Empty
                If j < parts.Length Then value = parts(j)
                row(header(j)) = value
            Next

            rows.Add(row)
        Next

        Return rows
    End Function


    Private Shared Function CreateRStructInternal(name As String) As MixedModelRStruct
        Select Case name
            Case "Compound Symmetry"
                Return New CompoundSymmetryR()
            Case "Heterogeneous Compound Symmetry", "Heterogeneous CS"
                Return New HeterogeneousCSR()
            Case "AR(1)"
                Return New AR1R()
            Case "Heterogeneous AR(1)"
                Return New HeterogeneousAR1R()
            Case "Unstructured"
                Return New UnstructuredR()
            Case Else
                Throw New ArgumentException("Unsupported R mmrm KR reference structure: " & name)
        End Select
    End Function


    Private Shared Function StartThetaForInternal(structureName As String) As Double()
        Select Case structureName
            Case "Compound Symmetry"
                Return New Double() {1.6119723529784433, 0.62916274331024713}
            Case "AR(1)"
                Return New Double() {1.5897216989092446, 0.59539636787538386}
            Case "Unstructured"
                Return New Double() {0.87916971566842228,
                                     1.2312552673676571,
                                     0.49750750452199372,
                                     1.381781498068767,
                                     0.2357143321994451,
                                     0.55245928701394587,
                                     1.0820320747663819,
                                     1.1684924986178791,
                                     0.88553914499981556,
                                     0.376140597144046}
            Case Else
                Return Nothing
        End Select
    End Function


    Private Shared Function ReferenceControlInternal() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 400
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function


    Private Shared Function FixedEffectNamesInternal() As String()
        Return New String() {"(Intercept)",
                             "sex_code",
                             "treatment_active",
                             "site_central",
                             "site_south",
                             "age_centered_8",
                             "treatment_active:age_centered_8"}
    End Function


    Private Shared Function SafeNames(input() As String,
                                      count As Integer,
                                      prefix As String) As String()
        Dim out(count - 1) As String
        For i As Integer = 0 To count - 1
            If input IsNot Nothing AndAlso i < input.Length AndAlso Not String.IsNullOrWhiteSpace(input(i)) Then
                out(i) = input(i)
            Else
                out(i) = prefix & (i + 1).ToString(CultureInfo.InvariantCulture)
            End If
        Next
        Return out
    End Function


    Private Shared Function IndexOfName(names() As String,
                                        target As String) As Integer
        For i As Integer = 0 To names.Length - 1
            If String.Equals(names(i), target, StringComparison.OrdinalIgnoreCase) Then Return i
        Next
        Return -1
    End Function


    Private Shared Function Slice3D(source(,,) As Double,
                                    h As Integer) As Double(,)
        Dim p As Integer = source.GetLength(1)
        Dim out(p - 1, p - 1) As Double
        For r As Integer = 0 To p - 1
            For c As Integer = 0 To p - 1
                out(r, c) = source(h, r, c)
            Next
        Next
        Return out
    End Function


    Private Shared Function Slice4D(source(,,,) As Double,
                                    h As Integer,
                                    j As Integer) As Double(,)
        Dim p As Integer = source.GetLength(2)
        Dim out(p - 1, p - 1) As Double
        For r As Integer = 0 To p - 1
            For c As Integer = 0 To p - 1
                out(r, c) = source(h, j, r, c)
            Next
        Next
        Return out
    End Function


    Private Shared Function ParseCsvLine(line As String) As String()
        Dim fields As New List(Of String)()
        Dim sb As New StringBuilder()
        Dim inQuotes As Boolean = False
        Dim i As Integer = 0

        While i < line.Length
            Dim ch As Char = line(i)
            If ch = """"c Then
                If inQuotes AndAlso i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                    sb.Append(""""c)
                    i += 1
                Else
                    inQuotes = Not inQuotes
                End If
            ElseIf ch = ","c AndAlso Not inQuotes Then
                fields.Add(sb.ToString())
                sb.Length = 0
            Else
                sb.Append(ch)
            End If
            i += 1
        End While

        fields.Add(sb.ToString())
        Return fields.ToArray()
    End Function


    Private Shared Function Field(parts() As String,
                                  index As Integer) As String
        If index < 0 OrElse index >= parts.Length Then Return String.Empty
        Return parts(index)
    End Function


    Private Shared Function ParseInteger(value As String) As Integer
        If String.IsNullOrWhiteSpace(value) Then Return -1
        Return Integer.Parse(value, CultureInfo.InvariantCulture)
    End Function


    Private Shared Function ParseDoubleInternal(value As String) As Double
        If String.IsNullOrWhiteSpace(value) Then Return Double.NaN
        Return Double.Parse(value, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture)
    End Function


    Private Shared Function IsMissingInternal(value As String) As Boolean
        Return String.IsNullOrWhiteSpace(value)
    End Function


    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function


    Private Shared Function Csv(value As String) As String
        If value Is Nothing Then value = String.Empty
        If value.IndexOfAny(New Char() {","c, """"c, ControlChars.Cr, ControlChars.Lf}) >= 0 Then
            Return """" & value.Replace("""", """""") & """"
        End If
        Return value
    End Function


    Private Shared Function Csv(value As Integer) As String
        Return value.ToString(CultureInfo.InvariantCulture)
    End Function


    Private Shared Function Csv(value As Double) As String
        Return value.ToString("G17", CultureInfo.InvariantCulture)
    End Function


    Private Shared Function Csv(value As Boolean) As String
        Return If(value, "true", "false")
    End Function


    Private Class KrInternalRow
        Public Property StructureName As String = String.Empty
        Public Property Kind As String = String.Empty
        Public Property Subkind As String = String.Empty
        Public Property Label As String = String.Empty
        Public Property H As Integer = -1
        Public Property J As Integer = -1
        Public Property RowIndex As Integer = -1
        Public Property ColIndex As Integer = -1
        Public Property RowName As String = String.Empty
        Public Property ColName As String = String.Empty
        Public Property Value As Double = Double.NaN

        Public ReadOnly Property Key As String
            Get
                Return String.Join("|",
                                   New String() {If(StructureName, String.Empty),
                                                 If(Kind, String.Empty),
                                                 If(Subkind, String.Empty),
                                                 If(Label, String.Empty),
                                                 H.ToString(CultureInfo.InvariantCulture),
                                                 J.ToString(CultureInfo.InvariantCulture),
                                                 RowIndex.ToString(CultureInfo.InvariantCulture),
                                                 ColIndex.ToString(CultureInfo.InvariantCulture)})
            End Get
        End Property
    End Class


    Private Class KrInternalComparisonRow
        Public Property StructureName As String = String.Empty
        Public Property Kind As String = String.Empty
        Public Property Subkind As String = String.Empty
        Public Property Label As String = String.Empty
        Public Property H As Integer = -1
        Public Property J As Integer = -1
        Public Property RowIndex As Integer = -1
        Public Property ColIndex As Integer = -1
        Public Property RowName As String = String.Empty
        Public Property ColName As String = String.Empty
        Public Property ExpectedValue As Double = Double.NaN
        Public Property ActualValue As Double = Double.NaN
        Public Property Diff As Double = Double.NaN
        Public Property AbsDiff As Double = Double.NaN
        Public Property RelDiff As Double = Double.NaN
        Public Property AbsTolerance As Double = 0.0
        Public Property RelTolerance As Double = 0.0
        Public Property Allowed As Double = 0.0
        Public Property NormalizedDiff As Double = Double.NaN
        Public Property AssertedByDefault As Boolean = False
        Public Property RawParameterizationSensitive As Boolean = False
        Public Property Status As String = String.Empty

        Public ReadOnly Property Key As String
            Get
                Return String.Join("|",
                                   New String() {If(StructureName, String.Empty),
                                                 If(Kind, String.Empty),
                                                 If(Subkind, String.Empty),
                                                 If(Label, String.Empty),
                                                 H.ToString(CultureInfo.InvariantCulture),
                                                 J.ToString(CultureInfo.InvariantCulture),
                                                 RowIndex.ToString(CultureInfo.InvariantCulture),
                                                 ColIndex.ToString(CultureInfo.InvariantCulture)})
            End Get
        End Property
    End Class


    Private Class ModelDataInternal
        Public Property Y As Double()
        Public Property X As Double(,)
        Public Property SubjectId As Object()
        Public Property Visit As Double()
    End Class

End Class

' ===== END MMRM multicovariate missing KR internal ingredient R-reference diagnostics =====
