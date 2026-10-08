Option Explicit On
Option Infer On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.regression

<TestClass>
Public Class MixedModelEngineWrappersCoreTests

    Private Const ExactTolerance As Double = 0.000001R
    Private Const SmokeTolerance As Double = 0.75R

    <TestMethod>
    Public Sub MixedModelEngine_MMRMIdentity_MatchesOlsFixedEffects()
        Dim y() As Double = {1.1R, 2.9R,
                             0.8R, 3.2R,
                             1.1R, 2.9R}
        Dim visit() As Double = {0.0R, 1.0R, 0.0R, 1.0R, 0.0R, 1.0R}
        Dim subjectId() As Object = {"S1", "S1", "S2", "S2", "S3", "S3"}
        Dim x(5, 1) As Double

        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0R
            x(i, 1) = visit(i)
        Next

        Dim data As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                         x:=x,
                                                                         subjectId:=subjectId,
                                                                         z:=Nothing,
                                                                         visit:=visit,
                                                                         sortWithinSubjectByVisit:=True)
        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(data,
                                                                          New IdentityR(),
                                                                          MixedModelFitMethod.ML)
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.Control = FastControl()
        req.StartThetaR = {Math.Log(0.1R)}

        Dim engine As New MixedModelEngine(req)
        Dim res As MixedModelResult = engine.Fit()

        AssertBasicResult(res, expectedP:=2, expectedN:=6)
        Assert.AreEqual(1.0R, res.Beta(0), ExactTolerance)
        Assert.AreEqual(2.0R, res.Beta(1), ExactTolerance)
    End Sub

    <TestMethod>
    Public Sub LMM_RandomInterceptWrapper_RunsInCore()
        Dim y() As Double = Nothing
        Dim x(,) As Double = Nothing
        Dim z(,) As Double = Nothing
        Dim subjectId() As Object = Nothing
        Dim visit() As Double = Nothing
        BuildBalancedRandomInterceptToyData(y, x, z, subjectId, visit)

        Dim data As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                         x:=x,
                                                                         subjectId:=subjectId,
                                                                         z:=z,
                                                                         visit:=visit,
                                                                         sortWithinSubjectByVisit:=True)
        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateLMM(data,
                                                                         New IdentityR(),
                                                                         New RandomIntercept(),
                                                                         MixedModelFitMethod.REML)
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.RandomEffectNames = {"Intercept"}
        req.Control = FastControl()
        req.StartThetaG = {Math.Log(0.5R)}
        req.StartThetaR = {Math.Log(0.05R)}

        Dim fit As New LMM(req)
        Dim res As MixedModelResult = fit.Fit()

        AssertBasicResult(res, expectedP:=2, expectedN:=y.Length)
        Assert.AreEqual(1, res.Q)
        Assert.AreEqual(10.0R, res.Beta(0), SmokeTolerance)
        Assert.AreEqual(1.5R, res.Beta(1), SmokeTolerance)
        Assert.IsNotNull(res.RandomEffects)
        Assert.IsTrue(res.RandomEffects.Count > 0)
    End Sub

    <TestMethod>
    Public Sub MMRM_Wrapper_RunsInCore()
        Dim y() As Double = {1.1R, 2.9R,
                             0.8R, 3.2R,
                             1.1R, 2.9R}
        Dim visit() As Double = {0.0R, 1.0R, 0.0R, 1.0R, 0.0R, 1.0R}
        Dim subjectId() As Object = {"S1", "S1", "S2", "S2", "S3", "S3"}
        Dim x(5, 1) As Double

        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0R
            x(i, 1) = visit(i)
        Next

        Dim data As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                         x:=x,
                                                                         subjectId:=subjectId,
                                                                         z:=Nothing,
                                                                         visit:=visit,
                                                                         sortWithinSubjectByVisit:=True)
        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(data,
                                                                          New IdentityR(),
                                                                          MixedModelFitMethod.ML)
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.Control = FastControl()
        req.StartThetaR = {Math.Log(0.1R)}

        Dim fit As New MMRM(req)
        Dim res As MixedModelResult = fit.Fit()

        AssertBasicResult(res, expectedP:=2, expectedN:=6)
        Assert.AreEqual(1.0R, res.Beta(0), ExactTolerance)
        Assert.AreEqual(2.0R, res.Beta(1), ExactTolerance)
    End Sub

    <TestMethod>
    Public Sub MixedModelFormulaService_RhsOnlyFormula_BuildsPortableMmrmRequest()
        Dim raw(5, 1) As Object
        Dim subject() As Double = {1.0R, 1.0R, 2.0R, 2.0R, 3.0R, 3.0R}
        Dim visit() As Double = {0.0R, 1.0R, 0.0R, 1.0R, 0.0R, 1.0R}
        For i As Integer = 0 To 5
            raw(i, 0) = subject(i)
            raw(i, 1) = visit(i)
        Next

        Dim y() As Double = {1.1R, 2.9R,
                             0.8R, 3.2R,
                             1.1R, 2.9R}

        Dim req As MixedModelFitRequest = MixedModelFormulaService.BuildRequestFromRawMatrix(rawInput:=raw,
                                                                                             variableNames:=New String() {"subject", "visit"},
                                                                                             response:=y,
                                                                                             fixedFormulaText:="visit",
                                                                                             subjectKey:="subject",
                                                                                             responseName:="y",
                                                                                             randomFormulaText:=Nothing,
                                                                                             visitKey:="visit",
                                                                                             fitMethod:=MixedModelFitMethod.ML,
                                                                                             residualStructType:="Identity",
                                                                                             randomStructType:="None")

        Assert.IsNotNull(req)
        Assert.IsTrue(req.IsMMRM())
        Assert.AreEqual(2, req.Data.P)
        Assert.AreEqual(0, req.Data.Q)
        Assert.AreEqual(6, req.Data.Nobs)
        Assert.AreEqual(3, req.Data.NoSubjects)
        CollectionAssert.AreEqual(New String() {"Intercept", "visit"}, req.FixedEffectNames)
    End Sub

    Private Shared Function FastControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 120
        ctl.Epsilon = 0.000001R
        ctl.StepTolerance = 0.0000001R
        ctl.FunctionTolerance = 0.0000001R
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function

    Private Shared Sub BuildBalancedRandomInterceptToyData(ByRef y() As Double,
                                                           ByRef x(,) As Double,
                                                           ByRef z(,) As Double,
                                                           ByRef subjectId() As Object,
                                                           ByRef visit() As Double)
        Const nSubjects As Integer = 6
        Const nVisits As Integer = 3
        Dim n As Integer = nSubjects * nVisits

        ReDim y(n - 1)
        ReDim x(n - 1, 1)
        ReDim z(n - 1, 0)
        ReDim subjectId(n - 1)
        ReDim visit(n - 1)

        Dim subjectEffects() As Double = {-1.0R, -0.6R, -0.2R, 0.2R, 0.6R, 1.0R}
        Dim residuals() As Double = {-0.05R, 0.04R, 0.01R,
                                      0.02R, -0.03R, 0.01R,
                                      0.01R, 0.02R, -0.03R,
                                      -0.02R, 0.03R, -0.01R,
                                      0.04R, -0.01R, -0.03R,
                                      -0.01R, -0.02R, 0.03R}

        Dim rowIndex As Integer = 0
        For subjectIndex As Integer = 0 To nSubjects - 1
            For visitIndex As Integer = 0 To nVisits - 1
                subjectId(rowIndex) = "S" & (subjectIndex + 1).ToString(Global.System.Globalization.CultureInfo.InvariantCulture)
                visit(rowIndex) = Convert.ToDouble(visitIndex)
                x(rowIndex, 0) = 1.0R
                x(rowIndex, 1) = Convert.ToDouble(visitIndex)
                z(rowIndex, 0) = 1.0R
                y(rowIndex) = 10.0R + 1.5R * Convert.ToDouble(visitIndex) + subjectEffects(subjectIndex) + residuals(rowIndex)
                rowIndex += 1
            Next
        Next
    End Sub

    Private Shared Sub AssertBasicResult(res As MixedModelResult,
                                         expectedP As Integer,
                                         expectedN As Integer)
        Assert.IsNotNull(res)
        Assert.AreEqual(expectedP, res.P)
        Assert.AreEqual(expectedN, res.Nobs)
        Assert.IsNotNull(res.Beta)
        Assert.AreEqual(expectedP, res.Beta.Length)
        For Each value As Double In res.Beta
            Assert.IsFalse(Double.IsNaN(value))
            Assert.IsFalse(Double.IsInfinity(value))
        Next
        Assert.IsFalse(Double.IsNaN(res.Objective))
        Assert.IsFalse(Double.IsInfinity(res.Objective))
        Assert.IsFalse(Double.IsNaN(res.LogLik))
        Assert.IsFalse(Double.IsInfinity(res.LogLik))
    End Sub

End Class
