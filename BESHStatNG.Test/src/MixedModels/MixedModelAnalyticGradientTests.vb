Option Explicit On
Option Infer On
Option Strict Off

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression

<TestClass()>
Public Class MixedModelAnalyticGradientTests

    <TestMethod()>
    Public Sub MMRM_AnalyticScoreAndNumericalGradientFits_MatchForMLAndREML()
        For Each method As MixedModelFitMethod In New MixedModelFitMethod() {MixedModelFitMethod.ML, MixedModelFitMethod.REML}
            Dim numerical As MixedModelResult = FitDeterministicMmrm(method,
                                                                     MixedModelCovarianceGradientMode.NumericalFiniteDifference)
            Dim analytic As MixedModelResult = FitDeterministicMmrm(method,
                                                                    MixedModelCovarianceGradientMode.AnalyticScore)

            Assert.IsTrue(numerical.Converged, method.ToString() & ": numerical-gradient fit should converge. " & numerical.Message)
            Assert.IsTrue(analytic.Converged, method.ToString() & ": analytic-gradient fit should converge. " & analytic.Message)
            Assert.AreEqual("Numerical finite difference",
                            numerical.PerformanceDiagnostics.GradientProviderName,
                            method.ToString() & ": baseline should use numerical finite differences.")
            Assert.AreEqual("Caller-supplied gradient",
                            analytic.PerformanceDiagnostics.GradientProviderName,
                            method.ToString() & ": analytic opt-in should supply the optimizer gradient.")
            Assert.AreEqual("Analytic score",
                            analytic.PerformanceDiagnostics.ActualCovarianceGradientProviderName,
                            method.ToString() & ": actual provider should be analytic.")
            Assert.IsTrue(analytic.PerformanceDiagnostics.AnalyticGradientUsed,
                          method.ToString() & ": supported MMRM analytic fit should report that the analytic gradient was used.")
            Assert.IsFalse(analytic.PerformanceDiagnostics.AnalyticGradientFallbackUsed,
                           method.ToString() & ": supported MMRM analytic fit should not fall back.")
            Assert.IsTrue(numerical.PerformanceDiagnostics.NumericalGradientObjectiveEvaluationCount > 0,
                          method.ToString() & ": numerical-gradient baseline should record finite-difference objective calls.")
            Assert.AreEqual(0,
                            analytic.PerformanceDiagnostics.NumericalGradientObjectiveEvaluationCount,
                            method.ToString() & ": analytic optimizer gradients should not use numerical-gradient objective calls.")
            Assert.IsTrue(analytic.PerformanceDiagnostics.EstimatedNumericalGradientObjectiveEvaluationsAvoided > 0,
                          method.ToString() & ": analytic mode should report avoided numerical-gradient objective evaluations.")
            Assert.IsTrue(numerical.PerformanceDiagnostics.NumericalGradientObjectiveEvaluationCount > analytic.PerformanceDiagnostics.NumericalGradientObjectiveEvaluationCount,
                          method.ToString() & ": analytic mode should greatly reduce numerical-gradient objective calls.")

            Assert.AreEqual(numerical.Objective, analytic.Objective, 0.01,
                            method.ToString() & ": profiled objective should match the numerical-gradient optimum.")
            AssertVectorClose(numerical.Beta, analytic.Beta, 0.005,
                              method.ToString() & ": fixed effects should match the numerical-gradient fit.")
            AssertVectorClose(numerical.Theta, analytic.Theta, 0.02,
                              method.ToString() & ": covariance parameters should match the numerical-gradient fit.")
            AssertMatrixClose(numerical.VarBeta, analytic.VarBeta, 0.005,
                              method.ToString() & ": Var(beta) should match the numerical-gradient fit.")
        Next
    End Sub

    <TestMethod()>
    Public Sub MMRM_AnalyticGradientValidationMode_ReportsFiniteDifferenceDiscrepancy()
        Dim res As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                           MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation,
                                                           validationTolerance:=0.01)

        Assert.IsTrue(res.Converged, "Validation-mode analytic fit should converge. " & res.Message)
        Assert.AreEqual(MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation,
                        res.PerformanceDiagnostics.SelectedCovarianceGradientMode)
        Assert.AreEqual("Analytic score",
                        res.PerformanceDiagnostics.ActualCovarianceGradientProviderName)
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientUsed,
                      "Validation mode should still use the analytic optimizer gradient when validation is acceptable.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientValidationEvaluationCount >= 2,
                      "Validation mode should compare analytic and finite-difference gradients at optimization start and final theta.")
        Assert.IsTrue(Not Double.IsNaN(res.PerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy) AndAlso Not Double.IsInfinity(res.PerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy),
                      "Validation mode should report a finite maximum relative discrepancy.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy <= 0.01,
                      "Analytic gradient should agree with finite differences within the requested validation tolerance. maxRel=" &
                      res.PerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy.ToString("G17", CultureInfo.InvariantCulture))

        Dim wrapped As List(Of ResultTable) = res.wrapResults()
        Assert.IsTrue(ContainsResultTableText(wrapped, "Analytic gradient used"),
                      "Performance diagnostics table should identify whether analytic gradients were used.")
        Assert.IsTrue(ContainsResultTableText(wrapped, "Analytic gradient validation evaluations"),
                      "Performance diagnostics table should include validation evaluation count.")
        Assert.IsTrue(ContainsResultTableText(wrapped, "Analytic gradient max relative FD discrepancy"),
                      "Performance diagnostics table should include the maximum validation discrepancy.")
        Assert.IsTrue(ContainsResultTableText(wrapped, "Estimated numerical-gradient objective evaluations avoided"),
                      "Performance diagnostics table should include the deterministic objective-evaluation reduction estimate.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_AnalyticScoreAndNumericalGradientFits_MatchSatterthwaiteAndKenwardRogerOutputs()
        Dim satNumerical As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                                    MixedModelCovarianceGradientMode.NumericalFiniteDifference,
                                                                    fixedInferenceMethod:=MixedModelFixedInferenceMethod.Satterthwaite)
        Dim satAnalytic As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                                   MixedModelCovarianceGradientMode.AnalyticScore,
                                                                   fixedInferenceMethod:=MixedModelFixedInferenceMethod.Satterthwaite)

        Assert.IsTrue(satNumerical.Converged, "Satterthwaite numerical-gradient fit should converge. " & satNumerical.Message)
        Assert.IsTrue(satAnalytic.Converged, "Satterthwaite analytic-gradient fit should converge. " & satAnalytic.Message)
        Assert.AreEqual(MixedModelFixedInferenceMethod.Satterthwaite, satAnalytic.FixedInferenceMethod)
        AssertVectorClose(satNumerical.BetaDF, satAnalytic.BetaDF, 0.1,
                          "Satterthwaite coefficient denominator DF should match.")
        AssertVectorClose(satNumerical.BetaStatistic, satAnalytic.BetaStatistic, 0.01,
                          "Satterthwaite coefficient test statistics should match.")
        AssertVectorClose(satNumerical.BetaP, satAnalytic.BetaP, 0.01,
                          "Satterthwaite coefficient p-values should match.")

        Dim krNumerical As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                                   MixedModelCovarianceGradientMode.NumericalFiniteDifference,
                                                                   enableFullKenwardRoger:=True)
        Dim krAnalytic As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                                  MixedModelCovarianceGradientMode.AnalyticScore,
                                                                  enableFullKenwardRoger:=True)

        Assert.IsTrue(krNumerical.Converged, "KR numerical-gradient fit should converge. " & krNumerical.Message)
        Assert.IsTrue(krAnalytic.Converged, "KR analytic-gradient fit should converge. " & krAnalytic.Message)
        Assert.AreEqual(MixedModelFixedInferenceMethod.KenwardRoger, krAnalytic.FixedInferenceMethod)
        AssertMatrixClose(krNumerical.KenwardRogerAdjustedVarBeta, krAnalytic.KenwardRogerAdjustedVarBeta, 0.01,
                          "KR adjusted Var(beta) should match.")
        AssertVectorClose(krNumerical.BetaDF, krAnalytic.BetaDF, 0.1,
                          "KR coefficient denominator DF should match.")
        AssertVectorClose(krNumerical.BetaStatistic, krAnalytic.BetaStatistic, 0.01,
                          "KR coefficient test statistics should match.")
        AssertVectorClose(krNumerical.BetaP, krAnalytic.BetaP, 0.01,
                          "KR coefficient p-values should match.")
        AssertKrTermFTestClose(krNumerical, krAnalytic, "treatment", 0.01, 0.1)
        AssertKrTermFTestClose(krNumerical, krAnalytic, "visit_c", 0.01, 0.1)
    End Sub

    <TestMethod()>
    Public Sub LMM_AnalyticScoreMode_RandomIntercept_UsesAnalyticAndMatchesNumerical()
        For Each method As MixedModelFitMethod In New MixedModelFitMethod() {MixedModelFitMethod.ML, MixedModelFitMethod.REML}
            Dim numerical As MixedModelResult = FitDeterministicLmm(method,
                                                                    MixedModelCovarianceGradientMode.NumericalFiniteDifference,
                                                                    New RandomIntercept())
            Dim analytic As MixedModelResult = FitDeterministicLmm(method,
                                                                   MixedModelCovarianceGradientMode.AnalyticScore,
                                                                   New RandomIntercept())

            AssertSupportedLmmAnalyticFitMatchesNumerical(numerical, analytic, method.ToString() & " random-intercept LMM")
        Next
    End Sub

    <TestMethod()>
    Public Sub LMM_AnalyticScoreMode_RandomInterceptSlope_UsesAnalyticAndMatchesNumerical()
        Dim numerical As MixedModelResult = FitDeterministicLmm(MixedModelFitMethod.REML,
                                                                MixedModelCovarianceGradientMode.NumericalFiniteDifference,
                                                                New RandomInterceptSlope())
        Dim analytic As MixedModelResult = FitDeterministicLmm(MixedModelFitMethod.REML,
                                                               MixedModelCovarianceGradientMode.AnalyticScore,
                                                               New RandomInterceptSlope())

        AssertSupportedLmmAnalyticFitMatchesNumerical(numerical, analytic, "REML random-intercept/slope LMM", thetaTolerance:=0.08)
    End Sub

    <TestMethod()>
    Public Sub LMM_AnalyticGradientValidationMode_RandomIntercept_ReportsFiniteDifferenceDiscrepancy()
        Dim res As MixedModelResult = FitDeterministicLmm(MixedModelFitMethod.REML,
                                                          MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation,
                                                          New RandomIntercept(),
                                                          validationTolerance:=0.02)

        Assert.IsTrue(res.Converged, "Validation-mode random-intercept LMM should converge. " & res.Message)
        Assert.AreEqual("Analytic score",
                        res.PerformanceDiagnostics.ActualCovarianceGradientProviderName,
                        "Supported G-side LMM analytic mode should use the analytic provider.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientUsed,
                      "Supported G-side LMM validation mode should use analytic optimizer gradients.")
        Assert.IsFalse(res.PerformanceDiagnostics.AnalyticGradientFallbackUsed,
                       "Supported G-side LMM validation mode should not fall back.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientValidationEvaluationCount >= 2,
                      "Validation mode should compare analytic and finite-difference gradients at start and final theta.")
        Assert.IsTrue(Not Double.IsNaN(res.PerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy) AndAlso
                      Not Double.IsInfinity(res.PerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy),
                      "LMM validation mode should report a finite maximum relative discrepancy.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy <= 0.02,
                      "LMM analytic gradient should agree with finite differences within tolerance. maxRel=" &
                      res.PerformanceDiagnostics.AnalyticGradientMaxRelativeFiniteDifferenceDiscrepancy.ToString("G17", CultureInfo.InvariantCulture))
    End Sub

    <TestMethod()>
    Public Sub LMM_AutoCovarianceGradientMode_RandomIntercept_UsesAnalyticAndMatchesNumerical()
        Dim numerical As MixedModelResult = FitDeterministicLmm(MixedModelFitMethod.REML,
                                                                MixedModelCovarianceGradientMode.NumericalFiniteDifference,
                                                                New RandomIntercept())
        Dim autoFit As MixedModelResult = FitDeterministicLmm(MixedModelFitMethod.REML,
                                                             MixedModelCovarianceGradientMode.Auto,
                                                             New RandomIntercept())

        AssertSupportedLmmAnalyticFitMatchesNumerical(numerical, autoFit, "REML random-intercept LMM Auto mode")
        Assert.AreEqual(MixedModelCovarianceGradientMode.Auto,
                        autoFit.PerformanceDiagnostics.SelectedCovarianceGradientMode,
                        "Auto mode should be preserved in performance diagnostics.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_AverageInformationReml_MatchesProjectedBfgsAnalyticFit()
        Dim bfgs As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                            MixedModelCovarianceGradientMode.AnalyticScore,
                                                            optimizerMode:=MixedModelCovarianceOptimizerMode.ProjectedBfgs)
        Dim ai As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                          MixedModelCovarianceGradientMode.AnalyticScore,
                                                          optimizerMode:=MixedModelCovarianceOptimizerMode.AverageInformationReml)

        Assert.IsTrue(bfgs.Converged, "Projected-BFGS analytic REML fit should converge. " & bfgs.Message)
        Assert.IsTrue(ai.Converged, "Average Information REML fit should converge. " & ai.Message)
        Assert.AreEqual(MixedModelCovarianceOptimizerMode.AverageInformationReml,
                        ai.ControlCovarianceOptimizerMode,
                        "Result should preserve the requested Average Information optimizer mode.")
        Assert.AreEqual(MixedModelCovarianceOptimizerMode.AverageInformationReml,
                        ai.PerformanceDiagnostics.SelectedCovarianceOptimizerMode,
                        "Performance diagnostics should preserve the selected Average Information optimizer mode.")
        Assert.AreEqual("Average Information REML",
                        ai.PerformanceDiagnostics.ActualCovarianceOptimizerName,
                        "Average Information fit should identify the optimizer used.")
        Assert.AreEqual("Average Information REML",
                        ai.PerformanceDiagnostics.GradientProviderName,
                        "Average Information optimizer should identify its own gradient/information provider.")
        Assert.AreEqual("Analytic score",
                        ai.PerformanceDiagnostics.ActualCovarianceGradientProviderName,
                        "Average Information optimizer should reuse the analytic score provider.")
        Assert.IsTrue(ai.PerformanceDiagnostics.AnalyticGradientUsed,
                      "Average Information optimizer should report analytic-gradient use.")
        Assert.IsTrue(ai.PerformanceDiagnostics.AverageInformationMatrixEvaluationCount > 0,
                      "Average Information diagnostics should report information-matrix evaluations.")
        Assert.IsTrue(Not Double.IsNaN(ai.PerformanceDiagnostics.AverageInformationMatrixTimeMs),
                      "Average Information diagnostics should report information-matrix timing.")
        Assert.AreEqual(0,
                        ai.PerformanceDiagnostics.NumericalGradientObjectiveEvaluationCount,
                        "Average Information optimizer should not use optimizer finite-difference gradients.")

        Assert.AreEqual(bfgs.Objective, ai.Objective, 0.15,
                        "Average Information objective should match projected-BFGS analytic optimum.")
        AssertVectorClose(bfgs.Beta, ai.Beta, 0.03,
                          "Average Information fixed effects should match projected-BFGS analytic fit.")
        AssertMatrixClose(bfgs.VarBeta, ai.VarBeta, 0.03,
                          "Average Information Var(beta) should match projected-BFGS analytic fit.")

        Dim wrapped As List(Of ResultTable) = ai.wrapResults()
        Assert.IsTrue(ContainsResultTableText(wrapped, "Average Information matrix evaluations"),
                      "Performance diagnostics should display Average Information counters.")
        Assert.IsTrue(ContainsResultTableText(wrapped, "Covariance optimizer mode"),
                      "Convergence diagnostics should display the selected covariance optimizer mode.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_AnalyticGradientDerivativePatternCache_RepeatedDesignGetsHits()
        Dim res As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                           MixedModelCovarianceGradientMode.AnalyticScore,
                                                           useAnalyticGradientDerivativePatternCache:=True)

        Assert.IsTrue(res.Converged, "Cached analytic-gradient fit should converge. " & res.Message)
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientDerivativePatternCacheEnabled,
                      "Analytic derivative-pattern cache should be enabled for this fit.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientDerivativePatternCount > 0,
                      "Repeated MMRM design should produce at least one derivative pattern.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientDerivativePatternCacheHits > 0,
                      "Repeated visit/design patterns should produce derivative-pattern cache hits.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientDerivativePatternCacheMisses > 0,
                      "The first block for each pattern should produce a cache miss.")
        Assert.IsTrue(res.PerformanceDiagnostics.AnalyticGradientDerivativeMatricesBuilt > 0,
                      "Derivative matrix build count should be populated.")
        Assert.IsTrue(Not Double.IsNaN(res.PerformanceDiagnostics.AnalyticGradientTraceQuadraticContractionTimeMs),
                      "Trace/quadratic contraction time should be populated.")

        Dim wrapped As List(Of ResultTable) = res.wrapResults()
        Assert.IsTrue(ContainsResultTableText(wrapped, "Analytic gradient derivative-pattern cache hits"),
                      "Performance diagnostics should display analytic derivative-pattern cache hits.")
        Assert.IsTrue(ContainsResultTableText(wrapped, "Analytic gradient derivative matrices built"),
                      "Performance diagnostics should display analytic derivative matrix build count.")
    End Sub

    <TestMethod()>
    Public Sub MMRM_AnalyticGradientDerivativePatternCache_CachedAndUncachedFitsMatch()
        Dim cached As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                             MixedModelCovarianceGradientMode.AnalyticScore,
                                                             useAnalyticGradientDerivativePatternCache:=True)
        Dim uncached As MixedModelResult = FitDeterministicMmrm(MixedModelFitMethod.REML,
                                                               MixedModelCovarianceGradientMode.AnalyticScore,
                                                               useAnalyticGradientDerivativePatternCache:=False)

        Assert.IsTrue(cached.Converged, "Cached analytic-gradient fit should converge. " & cached.Message)
        Assert.IsTrue(uncached.Converged, "Uncached analytic-gradient fit should converge. " & uncached.Message)
        Assert.IsTrue(cached.PerformanceDiagnostics.AnalyticGradientDerivativePatternCacheHits > 0,
                      "Cached fit should report derivative-pattern cache hits.")
        Assert.IsFalse(uncached.PerformanceDiagnostics.AnalyticGradientDerivativePatternCacheEnabled,
                       "Uncached fit should report the derivative-pattern cache as disabled.")
        Assert.AreEqual(0L, uncached.PerformanceDiagnostics.AnalyticGradientDerivativePatternCacheHits,
                        "Uncached fit should not report cache hits.")
        Assert.IsTrue(cached.PerformanceDiagnostics.AnalyticGradientDerivativeMatricesBuilt < uncached.PerformanceDiagnostics.AnalyticGradientDerivativeMatricesBuilt,
                      "Cached fit should build fewer derivative matrices than the uncached fit.")

        Assert.AreEqual(uncached.Objective, cached.Objective, 0.000001,
                        "Cached and uncached analytic-gradient objectives should match.")
        AssertVectorClose(uncached.Beta, cached.Beta, 0.000001,
                          "Cached and uncached fixed effects should match.")
        AssertVectorClose(uncached.Theta, cached.Theta, 0.000001,
                          "Cached and uncached covariance parameters should match.")
        AssertMatrixClose(uncached.VarBeta, cached.VarBeta, 0.000001,
                          "Cached and uncached Var(beta) should match.")
    End Sub

    Private Shared Function FitDeterministicMmrm(method As MixedModelFitMethod,
                                                 mode As MixedModelCovarianceGradientMode,
                                                 Optional validationTolerance As Double = 0.0001,
                                                 Optional fixedInferenceMethod As MixedModelFixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal,
                                                 Optional enableFullKenwardRoger As Boolean = False,
                                                 Optional useAnalyticGradientDerivativePatternCache As Boolean = True,
                                                 Optional optimizerMode As MixedModelCovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.ProjectedBfgs) As MixedModelResult
        Dim subjectCount As Integer = 30
        Dim visitCount As Integer = 4
        Dim yVals As New List(Of Double)()
        Dim subjectVals As New List(Of Object)()
        Dim visitVals As New List(Of Double)()
        Dim treatmentVals As New List(Of Double)()
        Dim visitCenter As Double = (CDbl(visitCount) - 1.0) / 2.0

        For s As Integer = 0 To subjectCount - 1
            Dim treatment As Double = If((s Mod 2) = 0, 0.0, 1.0)
            Dim subjectShift As Double = 0.04 * CDbl(s Mod 7)
            For v As Integer = 1 To visitCount
                subjectVals.Add("S" & s.ToString("0000", CultureInfo.InvariantCulture))
                visitVals.Add(CDbl(v))
                treatmentVals.Add(treatment)
                Dim visitC As Double = CDbl(v - 1) - visitCenter
                Dim deterministicNoise As Double = 0.08 * Math.Sin(0.31 * CDbl(s + 1) + 0.47 * CDbl(v))
                yVals.Add(7.5 + 0.65 * treatment + 0.18 * visitC + subjectShift + deterministicNoise)
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

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subject,
                                                                              visit:=visit)
        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          residualStruct:=New DiagonalHeterogeneousR(),
                                                                          fitMethod:=method)
        req.FixedEffectNames = New String() {"(Intercept)", "treatment", "visit_c"}
        If enableFullKenwardRoger Then
            req.EnableFullKenwardRogerForMmrm()
        Else
            req.FixedInferenceMethod = fixedInferenceMethod
        End If
        Dim control As MixedModelControl = req.Control
        control.MaxIter = 50
        control.Epsilon = 0.000001
        control.StepTolerance = 0.000001
        control.FunctionTolerance = 0.0000001
        control.CovarianceGradientMode = mode
        control.CovarianceOptimizerMode = optimizerMode
        control.AnalyticGradientValidationTolerance = validationTolerance
        control.FallbackToNumericalGradientOnAnalyticFailure = True
        control.UseAnalyticGradientDerivativePatternCache = useAnalyticGradientDerivativePatternCache
        req.Control = control

        Return (New MMRM(req)).Fit()
    End Function

    Private Shared Function FitDeterministicLmm(method As MixedModelFitMethod,
                                                    mode As MixedModelCovarianceGradientMode,
                                                    randomStruct As MixedModelGStruct,
                                                    Optional validationTolerance As Double = 0.0001,
                                                    Optional optimizerMode As MixedModelCovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.ProjectedBfgs) As MixedModelResult
        Dim subjectCount As Integer = 18
        Dim visitCount As Integer = 4
        Dim q As Integer = If(TypeOf randomStruct Is RandomInterceptSlope, 2, 1)
        Dim n As Integer = subjectCount * visitCount
        Dim y(n - 1) As Double
        Dim x(n - 1, 1) As Double
        Dim z(n - 1, q - 1) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double

        Dim row As Integer = 0
        For s As Integer = 0 To subjectCount - 1
            Dim subjectIntercept As Double = 0.12 * CDbl((s Mod 5) - 2)
            Dim subjectSlope As Double = 0.025 * CDbl((s Mod 4) - 1)
            For v As Integer = 1 To visitCount
                Dim time As Double = CDbl(v - 1) - 1.5
                subject(row) = "L" & s.ToString("000", CultureInfo.InvariantCulture)
                visit(row) = CDbl(v)
                x(row, 0) = 1.0
                x(row, 1) = time
                z(row, 0) = 1.0
                If q >= 2 Then z(row, 1) = time
                y(row) = 4.0 + 0.35 * time + subjectIntercept + subjectSlope * time + 0.035 * Math.Cos(0.73 * CDbl(row + 1))
                row += 1
            Next
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subject,
                                                                              z:=z,
                                                                              visit:=visit)
        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateLMM(blockData,
                                                                         New IdentityR(),
                                                                         randomStruct,
                                                                         method)
        req.FixedEffectNames = New String() {"(Intercept)", "time"}
        req.RandomEffectNames = If(q = 1, New String() {"(Intercept)"}, New String() {"(Intercept)", "time"})
        Dim control As MixedModelControl = req.Control
        control.MaxIter = 60
        control.Epsilon = 0.000001
        control.StepTolerance = 0.000001
        control.FunctionTolerance = 0.0000001
        control.CovarianceGradientMode = mode
        control.CovarianceOptimizerMode = optimizerMode
        control.AnalyticGradientValidationTolerance = validationTolerance
        control.FallbackToNumericalGradientOnAnalyticFailure = True
        req.Control = control

        Return (New LMM(req)).Fit()
    End Function

    Private Shared Sub AssertSupportedLmmAnalyticFitMatchesNumerical(numerical As MixedModelResult,
                                                                    analytic As MixedModelResult,
                                                                    label As String,
                                                                    Optional thetaTolerance As Double = 0.05)
        Assert.IsTrue(numerical.Converged, label & ": numerical-gradient fit should converge. " & numerical.Message)
        Assert.IsTrue(analytic.Converged, label & ": analytic-gradient fit should converge. " & analytic.Message)
        Assert.AreEqual("Caller-supplied gradient",
                        analytic.PerformanceDiagnostics.GradientProviderName,
                        label & ": analytic opt-in should supply the optimizer gradient.")
        Assert.AreEqual("Analytic score",
                        analytic.PerformanceDiagnostics.ActualCovarianceGradientProviderName,
                        label & ": supported G-side LMM should use the analytic provider.")
        Assert.IsTrue(analytic.PerformanceDiagnostics.AnalyticGradientUsed,
                      label & ": supported G-side LMM should report analytic-gradient use.")
        Assert.IsFalse(analytic.PerformanceDiagnostics.AnalyticGradientFallbackUsed,
                       label & ": supported G-side LMM should not fall back.")
        Assert.AreEqual(0,
                        analytic.PerformanceDiagnostics.NumericalGradientObjectiveEvaluationCount,
                        label & ": analytic optimizer gradients should not use numerical-gradient objective calls.")
        Assert.IsTrue(numerical.PerformanceDiagnostics.NumericalGradientObjectiveEvaluationCount > 0,
                      label & ": numerical-gradient baseline should record finite-difference objective calls.")
        Assert.IsTrue(analytic.PerformanceDiagnostics.EstimatedNumericalGradientObjectiveEvaluationsAvoided > 0,
                      label & ": analytic mode should report avoided numerical-gradient objective evaluations.")

        Assert.AreEqual(numerical.Objective, analytic.Objective, 0.05,
                        label & ": profiled objective should match the numerical-gradient optimum.")
        AssertVectorClose(numerical.Beta, analytic.Beta, 0.02,
                          label & ": fixed effects should match the numerical-gradient fit.")
        AssertVectorClose(numerical.Theta, analytic.Theta, thetaTolerance,
                          label & ": covariance parameters should match the numerical-gradient fit.")
        AssertMatrixClose(numerical.VarBeta, analytic.VarBeta, 0.03,
                          label & ": Var(beta) should match the numerical-gradient fit.")
    End Sub

    Private Shared Sub AssertVectorClose(expected() As Double,
                                         actual() As Double,
                                         tolerance As Double,
                                         label As String)
        Assert.IsNotNull(expected, label & ": expected vector missing.")
        Assert.IsNotNull(actual, label & ": actual vector missing.")
        Assert.AreEqual(expected.Length, actual.Length, label & ": vector length.")
        For i As Integer = 0 To expected.Length - 1
            Assert.AreEqual(expected(i), actual(i), tolerance,
                            label & ": index " & i.ToString(CultureInfo.InvariantCulture))
        Next
    End Sub

    Private Shared Sub AssertMatrixClose(expected(,) As Double,
                                         actual(,) As Double,
                                         tolerance As Double,
                                         label As String)
        Assert.IsNotNull(expected, label & ": expected matrix missing.")
        Assert.IsNotNull(actual, label & ": actual matrix missing.")
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), label & ": row count.")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), label & ": column count.")
        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(i, j), actual(i, j), tolerance,
                                label & ": element (" & i.ToString(CultureInfo.InvariantCulture) & "," & j.ToString(CultureInfo.InvariantCulture) & ").")
            Next
        Next
    End Sub

    Private Shared Sub AssertKrTermFTestClose(expected As MixedModelResult,
                                                  actual As MixedModelResult,
                                                  termName As String,
                                                  tolerance As Double,
                                                  Optional denDfTolerance As Double = -1.0)
        Dim expectedHypothesis As MixedModelMultiDfHypothesis = Nothing
        Dim expectedDiagnostic As String = String.Empty
        Assert.IsTrue(MixedModelHypothesisBuilder.TryBuildTermHypothesis(expected.FixedEffectNames,
                                                                         termName,
                                                                         expectedHypothesis,
                                                                         diagnostic:=expectedDiagnostic),
                      termName & ": expected term hypothesis should be buildable. " & expectedDiagnostic)

        Dim actualHypothesis As MixedModelMultiDfHypothesis = Nothing
        Dim actualDiagnostic As String = String.Empty
        Assert.IsTrue(MixedModelHypothesisBuilder.TryBuildTermHypothesis(actual.FixedEffectNames,
                                                                         termName,
                                                                         actualHypothesis,
                                                                         diagnostic:=actualDiagnostic),
                      termName & ": actual term hypothesis should be buildable. " & actualDiagnostic)

        Dim expectedInference As MixedModelKenwardRogerMultiDfInference = Nothing
        Dim expectedMessage As String = String.Empty
        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(expected,
                                                                        termName,
                                                                        expectedHypothesis.L,
                                                                        expectedInference,
                                                                        diagnostic:=expectedMessage),
                      termName & ": expected KR term F-test should be computable. " & expectedMessage)

        Dim actualInference As MixedModelKenwardRogerMultiDfInference = Nothing
        Dim actualMessage As String = String.Empty
        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(actual,
                                                                        termName,
                                                                        actualHypothesis.L,
                                                                        actualInference,
                                                                        diagnostic:=actualMessage),
                      termName & ": actual KR term F-test should be computable. " & actualMessage)

        Dim effectiveDenDfTolerance As Double = If(denDfTolerance >= 0.0, denDfTolerance, tolerance)

        Assert.AreEqual(expectedInference.NumDF, actualInference.NumDF, 0.0000000001,
                        termName & ": KR Type III numerator DF should match.")
        Assert.AreEqual(expectedInference.DenDF, actualInference.DenDF, effectiveDenDfTolerance,
                        termName & ": KR Type III denominator DF should match.")
        Assert.AreEqual(expectedInference.FStatistic, actualInference.FStatistic, tolerance,
                        termName & ": KR Type III F statistic should match.")
        Assert.AreEqual(expectedInference.PValue, actualInference.PValue, tolerance,
                        termName & ": KR Type III p-value should match.")
    End Sub

    Private Shared Function ContainsResultTableText(tables As List(Of ResultTable),
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

End Class
