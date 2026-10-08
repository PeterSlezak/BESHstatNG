Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.regression

<TestClass>
Public Class MixedModelFitRequestOptimizerCoreTests

    <TestMethod>
    Public Sub FitRequest_CreateMmrm_UsesPortableDefaultsAndValidates()
        Dim data As MixedModelBlockData = CreateMmrmData()
        Dim request As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(data,
                                                                              New IdentityR(),
                                                                              MixedModelFitMethod.REML)
        request.ResponseVarName = "y"
        request.SubjectVarName = "subject"
        request.VisitVarName = "visit"
        request.FixedEffectNames = New String() {"Intercept", "Visit"}

        request.Validate()

        Assert.IsTrue(request.IsMMRM())
        Assert.IsFalse(request.HasRandomEffects())
        Assert.AreEqual(MixedModelFixedInferenceMethod.BetweenWithin, request.FixedInferenceMethod)
        Assert.IsTrue(request.Describe().Contains("nSubjects=2", StringComparison.Ordinal))
    End Sub

    <TestMethod>
    Public Sub FitRequest_CreateLmm_ValidatesStartVectorLengths()
        Dim data As MixedModelBlockData = CreateRandomInterceptData()
        Dim request As MixedModelFitRequest = MixedModelFitRequest.CreateLMM(data,
                                                                             New IdentityR(),
                                                                             New RandomIntercept(),
                                                                             MixedModelFitMethod.REML)
        request.FixedEffectNames = New String() {"Intercept", "Visit"}
        request.RandomEffectNames = New String() {"Intercept"}
        request.StartThetaG = New Double() {0.0R, 1.0R}

        Assert.ThrowsException(Of ApplicationException)(Sub() request.Validate())
    End Sub

    <TestMethod>
    Public Sub FitRequest_EnableFullKrContracts_SelectExpectedParameterScales()
        Dim mmrm As New MixedModelFitRequest()
        mmrm.EnableFullKenwardRogerForMmrm()
        Assert.IsTrue(mmrm.UseKenwardRoger)
        Assert.IsTrue(mmrm.BuildKenwardRogerWorkspace)
        Assert.IsTrue(mmrm.BuildKenwardRogerSecondDerivatives)
        Assert.AreEqual(MixedModelFixedInferenceMethod.KenwardRoger, mmrm.FixedInferenceMethod)
        Assert.AreEqual(MixedModelKenwardRogerAdjustmentKind.Full, mmrm.KenwardRogerOptions.Adjustment)
        Assert.AreEqual(MixedModelKenwardRogerParameterScalePreference.MmrmTheta, mmrm.KenwardRogerOptions.ParameterScalePreference)

        Dim lmm As New MixedModelFitRequest()
        lmm.EnableFullKenwardRogerForLmm()
        Assert.AreEqual(MixedModelKenwardRogerParameterScalePreference.Covariance, lmm.KenwardRogerOptions.ParameterScalePreference)
        Assert.IsTrue(lmm.KenwardRogerOptions.StrictValidation)
    End Sub

    <TestMethod>
    Public Sub KenwardRogerOptions_Clone_IsIndependentAndFiniteDifferenceValidationNormalizes()
        Dim original As MixedModelKenwardRogerOptions = MixedModelKenwardRogerOptions.CreateFullMmrm()
        Dim clone As MixedModelKenwardRogerOptions = original.Clone()

        Assert.AreNotSame(original, clone)
        Assert.AreNotSame(original.FiniteDifferenceOptions, clone.FiniteDifferenceOptions)

        clone.FiniteDifferenceOptions.FirstDerivativeStepScale = Double.NaN
        clone.FiniteDifferenceOptions.SecondDerivativeStepScale = -1.0R
        clone.FiniteDifferenceOptions.MinimumStep = 0.01R
        clone.FiniteDifferenceOptions.MaximumStep = 0.001R
        clone.FiniteDifferenceOptions.MaxStepHalvings = 99
        clone.FiniteDifferenceOptions.Validate()
        clone.FiniteDifferenceOptions.UseRichardsonRefinement = False

        Assert.AreEqual(0.0001R, clone.FiniteDifferenceOptions.FirstDerivativeStepScale, 0.0R)
        Assert.AreEqual(0.00025R, clone.FiniteDifferenceOptions.SecondDerivativeStepScale, 0.0R)
        Assert.AreEqual(clone.FiniteDifferenceOptions.MinimumStep, clone.FiniteDifferenceOptions.MaximumStep, 0.0R)
        Assert.AreEqual(20, clone.FiniteDifferenceOptions.MaxStepHalvings)
        Assert.IsTrue(original.FiniteDifferenceOptions.UseRichardsonRefinement,
                      "Changing the clone must not mutate the original finite-difference options.")
    End Sub

    <TestMethod>
    Public Sub Optimizer_NumericalGradient_MatchesQuadraticDerivative()
        Dim objective As Func(Of Double(), Double) =
            Function(theta() As Double) Math.Pow(theta(0) - 2.0R, 2.0R) + 0.5R * Math.Pow(theta(1) + 1.0R, 2.0R)

        Dim gradient() As Double = MixedModelOptimizer.NumericalGradient(objective,
                                                                         New Double() {4.0R, 3.0R},
                                                                         epsilon:=0.000001R)

        Assert.AreEqual(4.0R, gradient(0), 0.00001R)
        Assert.AreEqual(4.0R, gradient(1), 0.00001R)
    End Sub

    <TestMethod>
    Public Sub Optimizer_Bfgs_ConvergesQuadratic()
        Dim control As MixedModelControl = MixedModelControl.CreateDefault()
        control.MaxIter = 25
        control.Epsilon = 0.000001R
        control.StepTolerance = 0.000001R
        control.FunctionTolerance = 0.000000001R
        control.Trace = True
        control.UseBfgsCovarianceOptimization = True

        Dim objective As Func(Of Double(), Double) =
            Function(theta() As Double) Math.Pow(theta(0) - 2.0R, 2.0R) + 0.5R * Math.Pow(theta(1) + 1.0R, 2.0R)

        Dim trace As String = String.Empty
        Dim state As MixedModelOptimizationState = MixedModelOptimizer.OptimizeProjected(New Double() {8.0R, -5.0R},
                                                                                         objective,
                                                                                         control,
                                                                                         strTrace:=trace)

        Assert.IsTrue(state.Converged, state.Message)
        Assert.AreEqual(2.0R, state.Theta(0), 0.001R)
        Assert.AreEqual(-1.0R, state.Theta(1), 0.001R)
        Assert.AreEqual("Numerical finite difference", state.GradientProviderName)
        Assert.IsTrue(state.ObjectiveEvaluationCount > 0)
        Assert.IsTrue(state.GradientEvaluationCount > 0)
        Assert.IsTrue(state.NumericalGradientObjectiveEvaluationCount > 0)
        Assert.IsTrue(state.LineSearchEvaluationCount > 0)
        Assert.IsFalse(String.IsNullOrWhiteSpace(trace))
    End Sub

    <TestMethod>
    Public Sub Optimizer_CallerGradient_SkipsNumericalGradientEvaluations()
        Dim control As MixedModelControl = MixedModelControl.CreateDefault()
        control.MaxIter = 25
        control.Epsilon = 0.000001R
        control.StepTolerance = 0.000001R
        control.FunctionTolerance = 0.000000001R
        control.UseBfgsCovarianceOptimization = True

        Dim objective As Func(Of Double(), Double) =
            Function(theta() As Double) Math.Pow(theta(0) - 2.0R, 2.0R) + 0.5R * Math.Pow(theta(1) + 1.0R, 2.0R)
        Dim gradient As Func(Of Double(), Double()) =
            Function(theta() As Double) New Double() {2.0R * (theta(0) - 2.0R), theta(1) + 1.0R}

        Dim state As MixedModelOptimizationState = MixedModelOptimizer.OptimizeProjected(New Double() {8.0R, -5.0R},
                                                                                         objective,
                                                                                         control,
                                                                                         gradient:=gradient)

        Assert.IsTrue(state.Converged, state.Message)
        Assert.AreEqual("Caller-supplied gradient", state.GradientProviderName)
        Assert.AreEqual(0, state.NumericalGradientObjectiveEvaluationCount)
        Assert.IsTrue(state.GradientEvaluationCount > 0)
    End Sub

    <TestMethod>
    Public Sub Optimizer_Interruption_ReturnsLatestAcceptedIterate()
        Dim objective As Func(Of Double(), Double) = Function(theta() As Double) theta(0) * theta(0)
        Dim state As MixedModelOptimizationState = MixedModelOptimizer.OptimizeProjected(New Double() {3.0R},
                                                                                         objective,
                                                                                         interruptionRequested:=Function() True)

        Assert.AreEqual(MixedModelOptimizationStatus.Interrupted, state.Status)
        Assert.IsFalse(state.Converged)
        Assert.AreEqual(3.0R, state.Theta(0), 0.0R)
        Assert.IsTrue(state.Message.IndexOf("interrupt", StringComparison.OrdinalIgnoreCase) >= 0)
    End Sub

    Private Shared Function CreateMmrmData() As MixedModelBlockData
        Dim y() As Double = {1.0R, 2.0R, 1.5R, 2.5R}
        Dim x(3, 1) As Double
        Dim subjectId() As Object = {"S1", "S1", "S2", "S2"}
        Dim visit() As Double = {0.0R, 1.0R, 0.0R, 1.0R}
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0R
            x(i, 1) = visit(i)
        Next
        Return MixedModelBlockData.FromArrays(y:=y,
                                              x:=x,
                                              subjectId:=subjectId,
                                              z:=Nothing,
                                              visit:=visit,
                                              sortWithinSubjectByVisit:=True)
    End Function

    Private Shared Function CreateRandomInterceptData() As MixedModelBlockData
        Dim y() As Double = {1.0R, 2.0R, 1.4R, 2.4R}
        Dim x(3, 1) As Double
        Dim z(3, 0) As Double
        Dim subjectId() As Object = {"S1", "S1", "S2", "S2"}
        Dim visit() As Double = {0.0R, 1.0R, 0.0R, 1.0R}
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0R
            x(i, 1) = visit(i)
            z(i, 0) = 1.0R
        Next
        Return MixedModelBlockData.FromArrays(y:=y,
                                              x:=x,
                                              subjectId:=subjectId,
                                              z:=z,
                                              visit:=visit,
                                              sortWithinSubjectByVisit:=True)
    End Function

End Class
