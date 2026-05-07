Option Explicit On
Option Infer On
Option Strict Off

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression

' -----------------------------------------------------------------------------
' Consolidated mixed-model test module.
' This file groups previously separate test classes so the MixedModels test
' folder stays below ten compile modules while preserving the existing tests.
' -----------------------------------------------------------------------------

' ===== BEGIN migrated from MixedModelCovarianceParameterScaleTests.vb =====



<TestClass()>
Public Class MixedModelCovarianceParameterScaleTests

    <TestMethod()>
    Public Sub RandomIntercept_OptimizerCovarianceScale_RoundTrips()
        Dim y() As Double = {1.1, 2.9, 0.8, 3.2, 1.1, 2.9, 1.0, 3.1}
        Dim subject() As Object = {"S1", "S1", "S2", "S2", "S3", "S3", "S4", "S4"}
        Dim visit() As Double = {0, 1, 0, 1, 0, 1, 0, 1}

        Dim x(y.Length - 1, 1) As Double
        Dim z(y.Length - 1, 0) As Double

        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
            x(i, 1) = visit(i)
            z(i, 0) = 1.0
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subject,
                                                                              z:=z,
                                                                              visit:=visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateLMM(blockData,
                                                                         New IdentityR(),
                                                                         New RandomIntercept(),
                                                                         MixedModelFitMethod.REML)

        Dim optimizerTheta() As Double = {System.Math.Log(0.5), System.Math.Log(0.25)}
        Dim covTheta() As Double = Nothing
        Dim names() As String = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelCovarianceParameterScale.TryOptimizerToCovarianceTheta(req,
                                                                                       optimizerTheta,
                                                                                       covTheta,
                                                                                       names,
                                                                                       msg), msg)

        Assert.AreEqual(2, covTheta.Length)
        Assert.AreEqual(0.5, covTheta(0), 0.0000000001)
        Assert.AreEqual(0.25, covTheta(1), 0.0000000001)

        Dim optimizerRoundTrip() As Double = Nothing
        Assert.IsTrue(MixedModelCovarianceParameterScale.TryCovarianceToOptimizerTheta(req,
                                                                                       covTheta,
                                                                                       optimizerRoundTrip,
                                                                                       msg), msg)

        Assert.AreEqual(optimizerTheta(0), optimizerRoundTrip(0), 0.0000000001)
        Assert.AreEqual(optimizerTheta(1), optimizerRoundTrip(1), 0.0000000001)
    End Sub


    <TestMethod()>
    Public Sub UnstructuredR_CovarianceScale_RoundTrips()
        Dim y() As Double = {1.0, 2.0, 3.0, 1.5, 2.5, 3.5}
        Dim subject() As Object = {"S1", "S1", "S1", "S2", "S2", "S2"}
        Dim visit() As Double = {1, 2, 3, 1, 2, 3}

        Dim x(y.Length - 1, 0) As Double
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subject,
                                                                              z:=Nothing,
                                                                              visit:=visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New UnstructuredR(),
                                                                          MixedModelFitMethod.REML)

        Dim optimizerTheta() As Double = {System.Math.Log(1.0), 0.2, System.Math.Log(1.1), 0.1, 0.3, System.Math.Log(0.9)}
        Dim covTheta() As Double = Nothing
        Dim names() As String = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelCovarianceParameterScale.TryOptimizerToCovarianceTheta(req,
                                                                                       optimizerTheta,
                                                                                       covTheta,
                                                                                       names,
                                                                                       msg), msg)

        Assert.AreEqual(6, covTheta.Length)

        Dim optimizerRoundTrip() As Double = Nothing
        Assert.IsTrue(MixedModelCovarianceParameterScale.TryCovarianceToOptimizerTheta(req,
                                                                                       covTheta,
                                                                                       optimizerRoundTrip,
                                                                                       msg), msg)

        Assert.AreEqual(optimizerTheta.Length, optimizerRoundTrip.Length)

        For i As Integer = 0 To optimizerTheta.Length - 1
            Assert.AreEqual(optimizerTheta(i), optimizerRoundTrip(i), 0.000001)
        Next
    End Sub

    <TestMethod()>
    Public Sub MMRM_Unstructured_AutomaticKRParameterMap_UsesRmmrmThetaConvention()
        Dim y() As Double = {1.0, 2.0, 3.0, 1.5, 2.5, 3.5}
        Dim subject() As Object = {"S1", "S1", "S1", "S2", "S2", "S2"}
        Dim visit() As Double = {1, 2, 3, 1, 2, 3}

        Dim x(y.Length - 1, 0) As Double
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subject,
                                                                              z:=Nothing,
                                                                              visit:=visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New UnstructuredR(),
                                                                          MixedModelFitMethod.REML)
        req.EnableFullKenwardRogerForMmrm()

        ' BESH optimizer UN order/scale is lower-triangular Cholesky order:
        '   log(L11), L21, log(L22), L31, L32, log(L33).
        Dim optimizerTheta() As Double = {System.Math.Log(1.0), 0.2, System.Math.Log(1.1), 0.1, 0.3, System.Math.Log(0.9)}
        Dim thetaCov(optimizerTheta.Length - 1, optimizerTheta.Length - 1) As Double
        For i As Integer = 0 To optimizerTheta.Length - 1
            thetaCov(i, i) = 0.01
        Next

        Dim map As MixedModelKrParameterMap = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelCovarianceParameterScale.TryCreateParameterMap(req,
                                                                               optimizerTheta,
                                                                               thetaCov,
                                                                               req.KenwardRogerOptions,
                                                                               map,
                                                                               msg), msg)

        Assert.IsNotNull(map)
        Assert.AreEqual(MixedModelKrParameterScale.MmrmTheta, map.ParameterScale)
        Assert.IsTrue(map.RequiresMmrmThetaBackTransform)
        Assert.AreEqual(optimizerTheta.Length, map.KrTheta.Length)

        ' R mmrm-compatible UN theta convention puts all log Cholesky diagonals first,
        ' followed by row-normalized off-diagonals Lij / Lii.
        Dim expectedKrTheta() As Double = {
            System.Math.Log(1.0),
            System.Math.Log(1.1),
            System.Math.Log(0.9),
            0.2 / 1.1,
            0.1 / 0.9,
            0.3 / 0.9
        }

        For i As Integer = 0 To expectedKrTheta.Length - 1
            Assert.AreEqual(expectedKrTheta(i), map.KrTheta(i), 0.000000000001, "KR theta index " & i.ToString(CultureInfo.InvariantCulture))
        Next

        Assert.IsNotNull(map.ParameterNames)
        Assert.AreEqual("R:mmrm_log_chol_diag_1", map.ParameterNames(0))
        Assert.AreEqual("R:mmrm_log_chol_diag_2", map.ParameterNames(1))
        Assert.AreEqual("R:mmrm_log_chol_diag_3", map.ParameterNames(2))
        Assert.AreEqual("R:mmrm_chol_ratio_2_1", map.ParameterNames(3))
        Assert.AreEqual("R:mmrm_chol_ratio_3_1", map.ParameterNames(4))
        Assert.AreEqual("R:mmrm_chol_ratio_3_2", map.ParameterNames(5))

        Dim optimizerRoundTrip() As Double = Nothing
        Assert.IsTrue(MixedModelCovarianceParameterScale.TryMmrmThetaToOptimizerTheta(req,
                                                                                      map.KrTheta,
                                                                                      optimizerRoundTrip,
                                                                                      msg), msg)

        Assert.AreEqual(optimizerTheta.Length, optimizerRoundTrip.Length)
        For i As Integer = 0 To optimizerTheta.Length - 1
            Assert.AreEqual(optimizerTheta(i), optimizerRoundTrip(i), 0.000000000001, "optimizer round-trip index " & i.ToString(CultureInfo.InvariantCulture))
        Next
    End Sub



    <TestMethod()>
    Public Sub MMRM_AR1_AutomaticKRParameterMap_UsesRmmrmThetaConvention()
        Dim req As MixedModelFitRequest = CreateThreeVisitMmrmRequest(New AR1R())
        req.EnableFullKenwardRogerForMmrm()

        Dim rho As Double = 0.5
        Dim optimizerTheta() As Double = {Math.Log(4.0), AtanhForTest(rho)}
        Dim map As MixedModelKrParameterMap = CreateKrMapForTest(req, optimizerTheta)

        Assert.AreEqual(MixedModelKrParameterScale.MmrmTheta, map.ParameterScale)
        Assert.IsTrue(map.RequiresMmrmThetaBackTransform)
        Assert.AreEqual(Math.Log(2.0), map.KrTheta(0), 0.000000000001)
        Assert.AreEqual(rho / Math.Sqrt(1.0 - rho * rho), map.KrTheta(1), 0.000000000001)
        Assert.AreEqual("R:mmrm_log_sd", map.ParameterNames(0))
        Assert.AreEqual("R:mmrm_ar1_rho", map.ParameterNames(1))

        AssertKrMapRoundTrips(req, optimizerTheta, map)
    End Sub

    <TestMethod()>
    Public Sub MMRM_CompoundSymmetry_AutomaticKRParameterMap_UsesRmmrmThetaConvention()
        Dim req As MixedModelFitRequest = CreateThreeVisitMmrmRequest(New CompoundSymmetryR())
        req.EnableFullKenwardRogerForMmrm()

        Dim rho As Double = 0.4
        Dim optimizerTheta() As Double = {Math.Log(9.0), AtanhForTest(rho)}
        Dim map As MixedModelKrParameterMap = CreateKrMapForTest(req, optimizerTheta)
        Dim a As Double = 1.0 / 2.0
        Dim expectedCorrelationTheta As Double = LogitForTest((rho + a) / (1.0 + a))

        Assert.AreEqual(MixedModelKrParameterScale.MmrmTheta, map.ParameterScale)
        Assert.IsTrue(map.RequiresMmrmThetaBackTransform)
        Assert.AreEqual(Math.Log(3.0), map.KrTheta(0), 0.000000000001)
        Assert.AreEqual(expectedCorrelationTheta, map.KrTheta(1), 0.000000000001)
        Assert.AreEqual("R:mmrm_log_sd", map.ParameterNames(0))
        Assert.AreEqual("R:mmrm_cs_rho", map.ParameterNames(1))

        AssertKrMapRoundTrips(req, optimizerTheta, map)
    End Sub

    <TestMethod()>
    Public Sub MMRM_HeterogeneousAR1_AutomaticKRParameterMap_UsesRmmrmThetaConvention()
        Dim req As MixedModelFitRequest = CreateThreeVisitMmrmRequest(New HeterogeneousAR1R())
        req.EnableFullKenwardRogerForMmrm()

        Dim rho As Double = 0.35
        Dim optimizerTheta() As Double = {Math.Log(1.0), Math.Log(4.0), Math.Log(9.0), AtanhForTest(rho)}
        Dim map As MixedModelKrParameterMap = CreateKrMapForTest(req, optimizerTheta)

        Assert.AreEqual(MixedModelKrParameterScale.MmrmTheta, map.ParameterScale)
        Assert.IsTrue(map.RequiresMmrmThetaBackTransform)
        Assert.AreEqual(Math.Log(1.0), map.KrTheta(0), 0.000000000001)
        Assert.AreEqual(Math.Log(2.0), map.KrTheta(1), 0.000000000001)
        Assert.AreEqual(Math.Log(3.0), map.KrTheta(2), 0.000000000001)
        Assert.AreEqual(rho / Math.Sqrt(1.0 - rho * rho), map.KrTheta(3), 0.000000000001)
        Assert.AreEqual("R:mmrm_log_sd_visit1", map.ParameterNames(0))
        Assert.AreEqual("R:mmrm_log_sd_visit2", map.ParameterNames(1))
        Assert.AreEqual("R:mmrm_log_sd_visit3", map.ParameterNames(2))
        Assert.AreEqual("R:mmrm_ar1_rho", map.ParameterNames(3))

        AssertKrMapRoundTrips(req, optimizerTheta, map)
    End Sub

    <TestMethod()>
    Public Sub MMRM_HeterogeneousCS_AutomaticKRParameterMap_UsesRmmrmThetaConvention()
        Dim req As MixedModelFitRequest = CreateThreeVisitMmrmRequest(New HeterogeneousCSR())
        req.EnableFullKenwardRogerForMmrm()

        Dim rho As Double = 0.25
        Dim optimizerTheta() As Double = {Math.Log(1.0), Math.Log(4.0), Math.Log(9.0), AtanhForTest(rho)}
        Dim map As MixedModelKrParameterMap = CreateKrMapForTest(req, optimizerTheta)
        Dim a As Double = 1.0 / 2.0
        Dim expectedCorrelationTheta As Double = LogitForTest((rho + a) / (1.0 + a))

        Assert.AreEqual(MixedModelKrParameterScale.MmrmTheta, map.ParameterScale)
        Assert.IsTrue(map.RequiresMmrmThetaBackTransform)
        Assert.AreEqual(Math.Log(1.0), map.KrTheta(0), 0.000000000001)
        Assert.AreEqual(Math.Log(2.0), map.KrTheta(1), 0.000000000001)
        Assert.AreEqual(Math.Log(3.0), map.KrTheta(2), 0.000000000001)
        Assert.AreEqual(expectedCorrelationTheta, map.KrTheta(3), 0.000000000001)
        Assert.AreEqual("R:mmrm_log_sd_visit1", map.ParameterNames(0))
        Assert.AreEqual("R:mmrm_log_sd_visit2", map.ParameterNames(1))
        Assert.AreEqual("R:mmrm_log_sd_visit3", map.ParameterNames(2))
        Assert.AreEqual("R:mmrm_cs_rho", map.ParameterNames(3))

        AssertKrMapRoundTrips(req, optimizerTheta, map)
    End Sub

    Private Shared Function CreateThreeVisitMmrmRequest(residual As MixedModelRStruct) As MixedModelFitRequest
        Dim y() As Double = {1.0, 2.0, 3.0, 1.5, 2.5, 3.5}
        Dim subject() As Object = {"S1", "S1", "S1", "S2", "S2", "S2"}
        Dim visit() As Double = {1, 2, 3, 1, 2, 3}
        Dim x(y.Length - 1, 0) As Double
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subject,
                                                                              z:=Nothing,
                                                                              visit:=visit,
                                                                              sortWithinSubjectByVisit:=True)
        Return MixedModelFitRequest.CreateMMRM(blockData, residual, MixedModelFitMethod.REML)
    End Function

    Private Shared Function CreateKrMapForTest(req As MixedModelFitRequest,
                                               optimizerTheta() As Double) As MixedModelKrParameterMap
        Dim thetaCov(optimizerTheta.Length - 1, optimizerTheta.Length - 1) As Double
        For i As Integer = 0 To optimizerTheta.Length - 1
            thetaCov(i, i) = 0.01
        Next

        Dim map As MixedModelKrParameterMap = Nothing
        Dim msg As String = Nothing
        Assert.IsTrue(MixedModelCovarianceParameterScale.TryCreateParameterMap(req,
                                                                               optimizerTheta,
                                                                               thetaCov,
                                                                               req.KenwardRogerOptions,
                                                                               map,
                                                                               msg), msg)
        Assert.IsNotNull(map)
        Return map
    End Function

    Private Shared Sub AssertKrMapRoundTrips(req As MixedModelFitRequest,
                                             optimizerTheta() As Double,
                                             map As MixedModelKrParameterMap)
        Dim optimizerRoundTrip() As Double = Nothing
        Dim msg As String = Nothing
        Assert.IsTrue(MixedModelCovarianceParameterScale.TryMmrmThetaToOptimizerTheta(req,
                                                                                      map.KrTheta,
                                                                                      optimizerRoundTrip,
                                                                                      msg), msg)
        Assert.AreEqual(optimizerTheta.Length, optimizerRoundTrip.Length)
        For i As Integer = 0 To optimizerTheta.Length - 1
            Assert.AreEqual(optimizerTheta(i), optimizerRoundTrip(i), 0.000000000001, "optimizer round-trip index " & i.ToString(CultureInfo.InvariantCulture))
        Next
    End Sub

    Private Shared Function AtanhForTest(rho As Double) As Double
        Return 0.5 * Math.Log((1.0 + rho) / (1.0 - rho))
    End Function

    Private Shared Function LogitForTest(p As Double) As Double
        Return Math.Log(p / (1.0 - p))
    End Function

    <TestMethod()>
    Public Sub FullKRRequest_RejectsML_WhenRequireRemlIsTrue()
        Dim y() As Double = {1.0, 2.0, 1.5, 2.5}
        Dim subject() As Object = {"S1", "S1", "S2", "S2"}
        Dim visit() As Double = {1, 2, 1, 2}

        Dim x(y.Length - 1, 0) As Double
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subject,
                                                                              z:=Nothing,
                                                                              visit:=visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New CompoundSymmetryR(),
                                                                          MixedModelFitMethod.ML)
        req.EnableFullKenwardRogerForMmrm()

        Assert.ThrowsException(Of ApplicationException)(Sub() req.Validate())
    End Sub
End Class

' ===== END migrated from MixedModelCovarianceParameterScaleTests.vb =====

' ===== BEGIN migrated from MixedModelKRSecondDerivativeTests.vb =====



<TestClass()>
Public Class MixedModelKRSecondDerivativeTests

    <TestMethod()>
    Public Sub SimpleMMRM_KRSecondDerivatives_PopulateRmatsAndInferenceWorkspace()
        Dim y() As Double = {1.1, 2.9,
                             0.8, 3.2,
                             1.1, 2.9,
                             1.0, 3.1}

        Dim subjectId() As Object = {"S1", "S1", "S2", "S2", "S3", "S3", "S4", "S4"}
        Dim visit() As Double = {0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0}

        Dim x(y.Length - 1, 1) As Double
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
            x(i, 1) = visit(i)
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subjectId,
                                                                              z:=Nothing,
                                                                              visit:=visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New IdentityR(),
                                                                          MixedModelFitMethod.REML)
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal
        req.BuildKenwardRogerWorkspace = True
        req.BuildKenwardRogerSecondDerivatives = True
        req.Control = TestControl()

        Dim res As MixedModelResult = (New MMRM(req)).Fit()

        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be populated.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Rmats, "KR R_hj matrices should be populated when second derivatives are requested.")
        Assert.IsNotNull(res.InferenceWorkspace, "InferenceWorkspace should be populated.")
        Assert.IsNotNull(res.InferenceWorkspace.KR_R, "KR R_hj matrices should be mirrored to InferenceWorkspace.")

        Assert.AreEqual(1, res.KenwardRogerWorkspace.K, "Identity R has one covariance parameter.")
        Assert.AreEqual(2, res.KenwardRogerWorkspace.P, "Fixed design has two columns.")

        Assert.AreEqual(1, res.KenwardRogerWorkspace.Rmats.GetLength(0))
        Assert.AreEqual(1, res.KenwardRogerWorkspace.Rmats.GetLength(1))
        Assert.AreEqual(2, res.KenwardRogerWorkspace.Rmats.GetLength(2))
        Assert.AreEqual(2, res.KenwardRogerWorkspace.Rmats.GetLength(3))

        For r As Integer = 0 To 1
            For c As Integer = 0 To 1
                Dim value As Double = res.KenwardRogerWorkspace.Rmats(0, 0, r, c)
                Assert.IsFalse(Double.IsNaN(value), "R_hj entry should not be NaN.")
                Assert.IsFalse(Double.IsInfinity(value), "R_hj entry should not be infinite.")
            Next
        Next
    End Sub


    Private Shared Function TestControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 100
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function

End Class

' ===== END migrated from MixedModelKRSecondDerivativeTests.vb =====

' ===== BEGIN migrated from MixedModelKRWorkspaceEngineTests.vb =====



<TestClass()>
Public Class MixedModelKRWorkspaceEngineTests

    <TestMethod()>
    Public Sub SimpleMMRM_BuildKenwardRogerWorkspace_PopulatesUniversalBlocks()
        Dim y() As Double = {1.1, 2.9,
                             0.8, 3.2,
                             1.1, 2.9,
                             1.0, 3.1}

        Dim subjectId() As Object = {"S1", "S1", "S2", "S2", "S3", "S3", "S4", "S4"}
        Dim visit() As Double = {0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0}

        Dim x(y.Length - 1, 1) As Double
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
            x(i, 1) = visit(i)
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subjectId,
                                                                              z:=Nothing,
                                                                              visit:=visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New IdentityR(),
                                                                          MixedModelFitMethod.REML)
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal
        req.BuildKenwardRogerWorkspace = True
        req.Control = TestControl()

        Dim res As MixedModelResult = (New MMRM(req)).Fit()

        Assert.IsNotNull(res)
        Assert.IsTrue(res.Converged, "Simple identity MMRM should converge.")
        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be populated when requested.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Blocks, "KR workspace blocks should not be Nothing.")
        Assert.AreEqual(4, res.KenwardRogerWorkspace.Blocks.Count, "Expected one KR block per subject.")

        Assert.AreEqual(2, res.KenwardRogerWorkspace.P)
        Assert.AreEqual(1, res.KenwardRogerWorkspace.K)

        Assert.IsNotNull(res.KenwardRogerWorkspace.Pmats, "P matrices should be populated.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Qmats, "Q matrices should be populated.")
        Assert.IsNotNull(res.KenwardRogerAdjustedVarBeta, "Linear adjusted Var(beta) should be populated.")

        Assert.AreEqual(2, res.KenwardRogerAdjustedVarBeta.GetLength(0))
        Assert.AreEqual(2, res.KenwardRogerAdjustedVarBeta.GetLength(1))

        For r As Integer = 0 To 1
            For c As Integer = 0 To 1
                Assert.IsFalse(Double.IsNaN(res.KenwardRogerAdjustedVarBeta(r, c)))
                Assert.IsFalse(Double.IsInfinity(res.KenwardRogerAdjustedVarBeta(r, c)))
            Next
        Next
    End Sub


    <TestMethod()>
    Public Sub SimpleMMRM_ProgressReporter_ReceivesCompletion()
        Dim y() As Double = {1.1, 2.9,
                             0.8, 3.2,
                             1.1, 2.9,
                             1.0, 3.1}

        Dim subjectId() As Object = {"S1", "S1", "S2", "S2", "S3", "S3", "S4", "S4"}
        Dim visit() As Double = {0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0}

        Dim x(y.Length - 1, 1) As Double
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
            x(i, 1) = visit(i)
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subjectId,
                                                                              z:=Nothing,
                                                                              visit:=visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim seen As New List(Of MixedModelProgressInfo)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New IdentityR(),
                                                                          MixedModelFitMethod.REML)
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.Control = TestControl()
        req.ProgressReporter = Sub(info As MixedModelProgressInfo)
                                   If info IsNot Nothing Then seen.Add(info)
                               End Sub

        Dim res As MixedModelResult = (New MMRM(req)).Fit()

        Assert.IsNotNull(res)
        Assert.IsTrue(seen.Count > 0, "Progress reporter should receive at least one update.")
        Assert.IsTrue(seen.Exists(Function(p) String.Equals(p.Stage, "Completed", StringComparison.OrdinalIgnoreCase)),
                      "Progress reporter should receive a Completed update.")
        Assert.IsTrue(res.ExecutionTimeMs >= 0.0, "Execution time should be recorded.")
    End Sub


    Private Shared Function TestControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 100
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function

End Class

' ===== END migrated from MixedModelKRWorkspaceEngineTests.vb =====

' ===== BEGIN migrated from MixedModelKenwardRogerBackendTests.vb =====



<TestClass()>
Public Class MixedModelKenwardRogerBackendTests

    <TestMethod()>
    Public Sub BuildKrMatrices_OneBlock_ReturnsExpectedDimensions()
        Dim ws As MixedModelKrWorkspace = BuildScalarWorkspace(includeSecondDerivative:=False,
                                                               adjustment:=MixedModelKenwardRogerAdjustmentKind.Linear)

        Dim msg As String = Nothing
        Assert.IsTrue(MixedModelKenwardRogerBackend.TryBuildKrMatrices(ws, msg), msg)
        Assert.IsNotNull(ws.Pmats)
        Assert.IsNotNull(ws.Qmats)

        Assert.AreEqual(1, ws.Pmats.GetLength(0))
        Assert.AreEqual(1, ws.Pmats.GetLength(1))
        Assert.AreEqual(1, ws.Pmats.GetLength(2))

        Assert.AreEqual(2.0, ws.Pmats(0, 0, 0), 0.0000000001)
        Assert.AreEqual(2.0, ws.Qmats(0, 0, 0, 0), 0.0000000001)
    End Sub


    <TestMethod()>
    Public Sub LinearAdjustedVarBeta_OneBlock_ComputesFiniteMatrix()
        Dim ws As MixedModelKrWorkspace = BuildScalarWorkspace(includeSecondDerivative:=False,
                                                               adjustment:=MixedModelKenwardRogerAdjustmentKind.Linear)

        Dim adjusted(,) As Double = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerBackend.TryComputeAdjustedVarBeta(ws, adjusted, msg), msg)
        Assert.IsNotNull(adjusted)
        Assert.AreEqual(1, adjusted.GetLength(0))
        Assert.AreEqual(1, adjusted.GetLength(1))
        Assert.IsFalse(Double.IsNaN(adjusted(0, 0)))
        Assert.IsFalse(Double.IsInfinity(adjusted(0, 0)))
        Assert.AreEqual(MixedModelKenwardRogerAdjustmentKind.Linear, ws.AdjustmentUsed)
        StringAssert.Contains(msg, "Linear KR adjusted Var(beta)")
    End Sub


    <TestMethod()>
    Public Sub FullAdjustedVarBeta_WithoutSecondDerivatives_FailsWhenFallbackDisabled()
        Dim ws As MixedModelKrWorkspace = BuildScalarWorkspace(includeSecondDerivative:=False,
                                                               adjustment:=MixedModelKenwardRogerAdjustmentKind.Full,
                                                               allowLinearFallback:=False)

        Dim adjusted(,) As Double = Nothing
        Dim msg As String = Nothing

        Assert.IsFalse(MixedModelKenwardRogerBackend.TryComputeAdjustedVarBeta(ws, adjusted, msg), "Full KR should fail when R_hj matrices are unavailable.")
        Assert.IsNull(adjusted)
        Assert.AreEqual(MixedModelKenwardRogerAdjustmentKind.None, ws.AdjustmentUsed)
        StringAssert.Contains(msg, "requires conformable R_hj")
    End Sub


    <TestMethod()>
    Public Sub FullAdjustedVarBeta_WithoutSecondDerivatives_CanUseExplicitLinearFallback()
        Dim ws As MixedModelKrWorkspace = BuildScalarWorkspace(includeSecondDerivative:=False,
                                                               adjustment:=MixedModelKenwardRogerAdjustmentKind.Full,
                                                               allowLinearFallback:=True)

        Dim adjusted(,) As Double = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerBackend.TryComputeAdjustedVarBeta(ws, adjusted, msg), msg)
        Assert.IsNotNull(adjusted)
        Assert.AreEqual(MixedModelKenwardRogerAdjustmentKind.Linear, ws.AdjustmentUsed)
        StringAssert.Contains(msg, "explicit linear fallback")
    End Sub


    <TestMethod()>
    Public Sub FullAdjustedVarBeta_WithSecondDerivatives_ComputesFullAdjustment()
        Dim ws As MixedModelKrWorkspace = BuildScalarWorkspace(includeSecondDerivative:=True,
                                                               adjustment:=MixedModelKenwardRogerAdjustmentKind.Full,
                                                               allowLinearFallback:=False)

        Dim adjusted(,) As Double = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerBackend.TryComputeAdjustedVarBeta(ws, adjusted, msg), msg)
        Assert.IsNotNull(adjusted)
        Assert.AreEqual(1, adjusted.GetLength(0))
        Assert.AreEqual(1, adjusted.GetLength(1))
        Assert.IsFalse(Double.IsNaN(adjusted(0, 0)))
        Assert.IsFalse(Double.IsInfinity(adjusted(0, 0)))
        Assert.AreEqual(MixedModelKenwardRogerAdjustmentKind.Full, ws.AdjustmentUsed)
        StringAssert.Contains(msg, "Full KR adjusted Var(beta)")
    End Sub


    Private Shared Function BuildScalarWorkspace(includeSecondDerivative As Boolean,
                                                 adjustment As MixedModelKenwardRogerAdjustmentKind,
                                                 Optional allowLinearFallback As Boolean = False) As MixedModelKrWorkspace
        Dim x(,) As Double = {{1.0}, {1.0}}
        Dim vinv(,) As Double = {{1.0, 0.0}, {0.0, 1.0}}

        Dim dv(0, 1, 1) As Double
        dv(0, 0, 0) = 1.0
        dv(0, 1, 1) = 1.0

        Dim block As New MixedModelKrBlock With {
            .X = x,
            .VInv = vinv,
            .DV = dv
        }

        If includeSecondDerivative Then
            Dim d2v(0, 0, 1, 1) As Double
            block.D2V = d2v
        End If

        Return New MixedModelKrWorkspace With {
            .P = 1,
            .K = 1,
            .VarBeta = New Double(,) {{1.0}},
            .ThetaCovariance = New Double(,) {{0.01}},
            .Blocks = New List(Of MixedModelKrBlock) From {block},
            .AdjustmentKind = adjustment,
            .AllowLinearFallback = allowLinearFallback
        }
    End Function

End Class

' ===== END migrated from MixedModelKenwardRogerBackendTests.vb =====


' ===== BEGIN adaptive KR finite-difference derivative validation tests =====

<TestClass()>
Public Class MixedModelKRFiniteDifferenceDerivativeValidationTests

    <TestMethod()>
    Public Sub MMRM_KRFiniteDifferenceDerivativeBlocks_AreFiniteSymmetricAndConformable()
        Dim structureNames() As String = {
            "Compound Symmetry",
            "Heterogeneous Compound Symmetry",
            "AR(1)",
            "Heterogeneous AR(1)",
            "Unstructured"
        }

        For Each structureName As String In structureNames
            Dim res As MixedModelResult = FitSyntheticMmrmForDerivativeValidation(structureName,
                                                                                 subjectCount:=28,
                                                                                 visitCount:=4,
                                                                                 incompleteVisits:=False)

            AssertDerivativeWorkspaceUsable(res, structureName)
            AssertKrDerivativeBlocksFiniteSymmetricAndConformable(res.KenwardRogerWorkspace, structureName)
            AssertKrMatricesFiniteSymmetricAndConformable(res.KenwardRogerWorkspace, structureName)
            AssertAdjustedVarBetaFiniteAndSymmetric(res, structureName)
            AssertFiniteDifferenceDiagnosticsAvailable(res, structureName)
            AssertNoFiniteDifferenceFallbackWarnings(res, structureName)
        Next
    End Sub


    <TestMethod()>
    Public Sub MMRM_KRFiniteDifferenceDerivativeBlocks_RemainFiniteWithIncompleteVisits()
        Dim structureNames() As String = {
            "Compound Symmetry",
            "AR(1)",
            "Unstructured"
        }

        For Each structureName As String In structureNames
            Dim res As MixedModelResult = FitSyntheticMmrmForDerivativeValidation(structureName,
                                                                                 subjectCount:=30,
                                                                                 visitCount:=4,
                                                                                 incompleteVisits:=True)

            AssertDerivativeWorkspaceUsable(res, structureName & " incomplete visits")
            AssertKrDerivativeBlocksFiniteSymmetricAndConformable(res.KenwardRogerWorkspace, structureName & " incomplete visits")
            AssertKrMatricesFiniteSymmetricAndConformable(res.KenwardRogerWorkspace, structureName & " incomplete visits")
            AssertAdjustedVarBetaFiniteAndSymmetric(res, structureName & " incomplete visits")
            AssertFiniteDifferenceDiagnosticsAvailable(res, structureName & " incomplete visits")
            AssertNoFiniteDifferenceFallbackWarnings(res, structureName & " incomplete visits")
        Next
    End Sub


    <TestMethod()>
    Public Sub MMRM_KRFiniteDifferenceDerivatives_ProduceUsableScalarAndMultiDfInference()
        ' Use a well-conditioned CS model for the scalar/multi-df inference smoke test.
        ' The structure-specific derivative tests above already cover UN/HAR1/AR1 derivative
        ' construction. This test confirms that the derivative pipeline can feed scalar
        ' and multi-df inference without making the denominator-DF smoke test depend on
        ' the most numerically demanding covariance structure.
        Dim res As MixedModelResult = FitSyntheticMmrmForDerivativeValidation("Compound Symmetry",
                                                                             subjectCount:=48,
                                                                             visitCount:=4,
                                                                             incompleteVisits:=False)
        AssertDerivativeWorkspaceUsable(res, "Compound Symmetry inference")

        Dim lScalar(res.P - 1) As Double
        lScalar(1) = 1.0

        Dim scalarEstimate As Double = Double.NaN
        Dim scalarSE As Double = Double.NaN
        Dim scalarDF As Double = Double.NaN
        Dim scalarStatistic As Double = Double.NaN
        Dim scalarP As Double = Double.NaN
        Dim scalarLower As Double = Double.NaN
        Dim scalarUpper As Double = Double.NaN
        Dim scalarDiagnostic As String = Nothing

        Assert.IsTrue(MixedModelPostEstimation.TryLinearInference(res,
                                                                  "Treatment",
                                                                  lScalar,
                                                                  0.05,
                                                                  scalarEstimate,
                                                                  scalarSE,
                                                                  scalarDF,
                                                                  scalarStatistic,
                                                                  scalarP,
                                                                  scalarLower,
                                                                  scalarUpper,
                                                                  scalarDiagnostic),
                      scalarDiagnostic)
        AssertFinite(scalarEstimate, "scalar estimate")
        AssertFinite(scalarSE, "scalar KR SE")
        AssertFinite(scalarDF, "scalar KR DF")
        AssertFinite(scalarStatistic, "scalar KR statistic")
        AssertFinite(scalarP, "scalar KR p-value")

        Dim lMulti(1, res.P - 1) As Double
        lMulti(0, 1) = 1.0
        lMulti(1, 2) = 1.0

        Dim multi As MixedModelKenwardRogerMultiDfInference = Nothing
        Dim multiDiagnostic As String = Nothing
        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(res,
                                                                        "Visit and TreatmentByVisit",
                                                                        lMulti,
                                                                        multi,
                                                                        0.05,
                                                                        multiDiagnostic),
                      multiDiagnostic)
        Assert.IsNotNull(multi)
        Assert.AreEqual(2, multi.NumDF, 0.0000000001)
        AssertFinite(multi.DenDF, "multi-df KR denominator DF")
        AssertFinite(multi.UnscaledFStatistic, "multi-df KR unscaled F")
        AssertFinite(multi.FStatistic, "multi-df KR scaled F")
        AssertFinite(multi.Scaling, "multi-df KR scaling")
        AssertFinite(multi.PValue, "multi-df KR p-value")
        Assert.IsTrue(multi.DenDF > 0.0, "multi-df denominator DF should be positive.")
        Assert.IsTrue(multi.FStatistic >= 0.0, "multi-df F statistic should be non-negative.")
    End Sub


    Private Shared Function FitSyntheticMmrmForDerivativeValidation(structureName As String,
                                                                    subjectCount As Integer,
                                                                    visitCount As Integer,
                                                                    incompleteVisits As Boolean) As MixedModelResult
        Dim yList As New List(Of Double)()
        Dim subjectList As New List(Of Object)()
        Dim visitList As New List(Of Double)()
        Dim treatmentList As New List(Of Double)()
        Dim visitCenteredList As New List(Of Double)()

        Dim visitCenter As Double = (CDbl(visitCount) + 1.0) / 2.0

        For s As Integer = 1 To subjectCount
            Dim trt As Double = If(s Mod 2 = 0, 1.0, 0.0)
            Dim subjectShift As Double = 0.08 * CDbl((s Mod 7) - 3)

            For v As Integer = 1 To visitCount
                If incompleteVisits AndAlso ((s Mod 5 = 0 AndAlso v = visitCount) OrElse (s Mod 7 = 0 AndAlso v = 2)) Then
                    Continue For
                End If

                Dim visitCentered As Double = CDbl(v) - visitCenter
                Dim deterministicNoise As Double = 0.04 * Math.Sin(0.31 * CDbl(s) + 0.73 * CDbl(v))

                Dim y As Double = 18.0 +
                                  1.15 * trt +
                                  0.55 * visitCentered +
                                  0.28 * trt * visitCentered +
                                  subjectShift +
                                  deterministicNoise

                yList.Add(y)
                subjectList.Add("S" & s.ToString("000", CultureInfo.InvariantCulture))
                visitList.Add(CDbl(v))
                treatmentList.Add(trt)
                visitCenteredList.Add(visitCentered)
            Next
        Next

        Dim n As Integer = yList.Count
        Dim x(n - 1, 3) As Double

        For i As Integer = 0 To n - 1
            x(i, 0) = 1.0
            x(i, 1) = treatmentList(i)
            x(i, 2) = visitCenteredList(i)
            x(i, 3) = treatmentList(i) * visitCenteredList(i)
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=yList.ToArray(),
                                                                              x:=x,
                                                                              subjectId:=subjectList.ToArray(),
                                                                              z:=Nothing,
                                                                              visit:=visitList.ToArray(),
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          CreateDerivativeValidationRStruct(structureName),
                                                                          MixedModelFitMethod.REML)
        req.FixedEffectNames = New String() {"Intercept", "Treatment", "VisitCentered", "TreatmentByVisit"}
        req.EnableFullKenwardRogerForMmrm()
        req.Control = DerivativeValidationControl()

        Return (New MMRM(req)).Fit()
    End Function


    Private Shared Function CreateDerivativeValidationRStruct(structureName As String) As MixedModelRStruct
        Select Case structureName.Trim().ToLowerInvariant()
            Case "compound symmetry"
                Return New CompoundSymmetryR()
            Case "heterogeneous compound symmetry"
                Return New HeterogeneousCSR()
            Case "ar(1)"
                Return New AR1R()
            Case "heterogeneous ar(1)"
                Return New HeterogeneousAR1R()
            Case "unstructured"
                Return New UnstructuredR()
            Case Else
                Throw New ArgumentException("Unsupported derivative-validation covariance structure: " & structureName)
        End Select
    End Function


    Private Shared Function DerivativeValidationControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 160
        ctl.Epsilon = 0.0000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.000000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function


    Private Shared Sub AssertDerivativeWorkspaceUsable(res As MixedModelResult,
                                                       label As String)
        Assert.IsNotNull(res, label & ": result should not be Nothing.")
        Assert.IsNotNull(res.KenwardRogerWorkspace, label & ": KR workspace should be populated.")
        Assert.AreEqual(MixedModelKrParameterScale.MmrmTheta,
                        res.KenwardRogerWorkspace.ParameterScale,
                        label & ": MMRM KR should use the mmrm-theta parameter scale.")
        Assert.AreEqual(MixedModelKenwardRogerAdjustmentKind.Full,
                        res.KenwardRogerWorkspace.AdjustmentKind,
                        label & ": full KR adjustment should be requested.")
        Assert.AreEqual(MixedModelKenwardRogerAdjustmentKind.Full,
                        res.KenwardRogerWorkspace.AdjustmentUsed,
                        label & ": full KR adjustment should be used.")
        Assert.IsTrue(res.KenwardRogerWorkspace.K > 0, label & ": covariance-parameter count should be positive.")
        Assert.IsTrue(res.KenwardRogerWorkspace.P > 0, label & ": fixed-effect dimension should be positive.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Blocks, label & ": KR blocks should be populated.")
        Assert.IsTrue(res.KenwardRogerWorkspace.Blocks.Count > 0, label & ": at least one KR block should exist.")
    End Sub


    Private Shared Sub AssertKrDerivativeBlocksFiniteSymmetricAndConformable(ws As MixedModelKrWorkspace,
                                                                             label As String)
        For blockIndex As Integer = 0 To ws.Blocks.Count - 1
            Dim block As MixedModelKrBlock = ws.Blocks(blockIndex)
            Dim msg As String = Nothing
            Assert.IsTrue(block.Validate(ws.P, ws.K, msg), label & ": KR block " & blockIndex & " failed validation: " & msg)

            AssertSymmetricFiniteMatrix(block.VInv, label & ": block " & blockIndex & " V inverse")
            AssertFiniteSymmetricTensor3(block.DV, label & ": block " & blockIndex & " first derivative tensor")
            Assert.IsNotNull(block.D2V, label & ": block " & blockIndex & " second derivative tensor should be populated for full KR.")
            AssertFiniteSymmetricTensor4(block.D2V, label & ": block " & blockIndex & " second derivative tensor")
            AssertD2ParameterPairSymmetry(block.D2V, label & ": block " & blockIndex & " second derivative parameter-pair symmetry")
        Next
    End Sub


    Private Shared Sub AssertKrMatricesFiniteSymmetricAndConformable(ws As MixedModelKrWorkspace,
                                                                     label As String)
        Assert.IsNotNull(ws.Pmats, label & ": P matrices should be populated.")
        Assert.IsNotNull(ws.Qmats, label & ": Q matrices should be populated.")
        Assert.IsNotNull(ws.Rmats, label & ": R matrices should be populated for full KR.")

        Assert.AreEqual(ws.K, ws.Pmats.GetLength(0), label & ": P matrix K dimension.")
        Assert.AreEqual(ws.P, ws.Pmats.GetLength(1), label & ": P matrix row dimension.")
        Assert.AreEqual(ws.P, ws.Pmats.GetLength(2), label & ": P matrix column dimension.")

        For h As Integer = 0 To ws.K - 1
            AssertSymmetricFiniteSlice3(ws.Pmats, h, label & ": P matrix " & h)
        Next

        Assert.AreEqual(ws.K, ws.Qmats.GetLength(0), label & ": Q matrix h dimension.")
        Assert.AreEqual(ws.K, ws.Qmats.GetLength(1), label & ": Q matrix j dimension.")
        Assert.AreEqual(ws.P, ws.Qmats.GetLength(2), label & ": Q matrix row dimension.")
        Assert.AreEqual(ws.P, ws.Qmats.GetLength(3), label & ": Q matrix column dimension.")

        Assert.AreEqual(ws.K, ws.Rmats.GetLength(0), label & ": R matrix h dimension.")
        Assert.AreEqual(ws.K, ws.Rmats.GetLength(1), label & ": R matrix j dimension.")
        Assert.AreEqual(ws.P, ws.Rmats.GetLength(2), label & ": R matrix row dimension.")
        Assert.AreEqual(ws.P, ws.Rmats.GetLength(3), label & ": R matrix column dimension.")

        For h As Integer = 0 To ws.K - 1
            For j As Integer = 0 To ws.K - 1
                ' Q_hj = X' P V_h P V_j P X is not generally symmetric when h <> j.
                ' The correct identity is Q_hj = transpose(Q_jh). Only diagonal
                ' Q_hh slices are individually symmetric.
                AssertFiniteSlice4(ws.Qmats, h, j, label & ": Q matrix " & h & "," & j)
                If h = j Then
                    AssertSymmetricFiniteSlice4(ws.Qmats, h, j, label & ": Q matrix " & h & "," & j)
                Else
                    AssertTransposePair4(ws.Qmats, h, j, label & ": Q matrix pair " & h & "," & j)
                End If

                AssertSymmetricFiniteSlice4(ws.Rmats, h, j, label & ": R matrix " & h & "," & j)
            Next
        Next
    End Sub


    Private Shared Sub AssertAdjustedVarBetaFiniteAndSymmetric(res As MixedModelResult,
                                                               label As String)
        Assert.IsNotNull(res.KenwardRogerAdjustedVarBeta, label & ": adjusted Var(beta) should be populated.")
        AssertSymmetricFiniteMatrix(res.KenwardRogerAdjustedVarBeta, label & ": adjusted Var(beta)")
        Assert.IsNotNull(res.VarBeta, label & ": unadjusted Var(beta) should be populated.")
        AssertSymmetricFiniteMatrix(res.VarBeta, label & ": unadjusted Var(beta)")
    End Sub


    Private Shared Sub AssertFiniteDifferenceDiagnosticsAvailable(res As MixedModelResult,
                                                                  label As String)
        Assert.IsNotNull(res.KenwardRogerWorkspace, label & ": KR workspace should be available.")

        Dim d As MixedModelKrFiniteDifferenceDiagnostics = res.KenwardRogerWorkspace.FiniteDifferenceDiagnostics
        Assert.IsNotNull(d, label & ": finite-difference diagnostics should be recorded.")

        Assert.IsTrue(d.BlocksStarted >= d.BlocksCompleted, label & ": blocks started should be >= blocks completed.")
        Assert.IsTrue(d.BlocksCompleted > 0, label & ": at least one derivative block should complete.")
        Assert.IsTrue(d.FirstDerivativeCentralCount > 0, label & ": central first derivatives should be counted.")
        Assert.AreEqual(0, d.FirstDerivativeOneSidedFallbackCount, label & ": stable synthetic data should not use one-sided fallback.")
        Assert.AreEqual(0, d.FirstDerivativeFailedCount, label & ": stable synthetic data should not fail first derivatives.")
        Assert.IsTrue(d.PureSecondDerivativeCentralCount > 0, label & ": pure second derivatives should be counted.")
        If res.KenwardRogerWorkspace.K > 1 Then
            Assert.IsTrue(d.MixedSecondDerivativeCentralCount > 0, label & ": mixed second derivatives should be counted.")
        End If
        Assert.AreEqual(0, d.SecondDerivativeFailedCount, label & ": stable synthetic data should not fail second derivatives.")
        Assert.IsTrue(d.MaxStepHalvingUsed >= 0, label & ": max step-halving count should be non-negative.")
        AssertFinite(d.MaxFirstDerivativeRichardsonRelativeChange, label & ": max first-derivative Richardson change")
        AssertFinite(d.MaxSecondDerivativeRichardsonRelativeChange, label & ": max second-derivative Richardson change")
        Assert.AreEqual("OK", d.QualityStatus(res.KenwardRogerWorkspace.FiniteDifferenceWarningThreshold()),
                        label & ": stable synthetic derivative diagnostics should be OK.")
        Assert.AreEqual(String.Empty, d.WarningSummary(res.KenwardRogerWorkspace.FiniteDifferenceWarningThreshold()),
                        label & ": stable synthetic derivative diagnostics should not produce warning text.")
        Assert.IsFalse(String.IsNullOrWhiteSpace(d.SummaryText(res.KenwardRogerWorkspace.FiniteDifferenceWarningThreshold())),
                       label & ": finite-difference diagnostics should produce a compact summary.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.FiniteDifferenceOptions,
                         label & ": finite-difference options snapshot should be stored on the workspace.")
        Assert.IsTrue(res.KenwardRogerWorkspace.FiniteDifferenceOptions.FirstDerivativeStepScale > 0.0,
                      label & ": finite-difference option snapshot should be valid.")
        Assert.IsTrue(d.PerturbedViCacheEntries > 0, label & ": perturbed V cache entries should be counted.")
        Assert.IsTrue(d.PerturbedViCacheMisses > 0, label & ": perturbed V cache misses should be counted.")
        Assert.AreEqual(0, d.PerturbedViCacheInvalidBuilds, label & ": stable synthetic data should not have invalid perturbed V builds.")

        Dim wrapped As List(Of ResultTable) = res.wrapResults(includeKenwardRogerTermTests:=True)
        Assert.IsTrue(ContainsResultTableText(wrapped, "KR finite-difference diagnostics"),
                      label & ": wrapResults should include KR finite-difference diagnostics table.")
    End Sub


    Private Shared Function ContainsResultTableText(tables As List(Of ResultTable),
                                                    expectedText As String) As Boolean
        If tables Is Nothing Then Return False

        For Each t As ResultTable In tables
            If t Is Nothing Then Continue For
            Dim arr(,) As Object = t.returnSelf()
            If ArrayContainsText(arr, expectedText) Then Return True
        Next

        Return False
    End Function


    Private Shared Function ArrayContainsText(arr(,) As Object,
                                              expectedText As String) As Boolean
        If arr Is Nothing Then Return False

        For i As Integer = 0 To arr.GetLength(0) - 1
            For j As Integer = 0 To arr.GetLength(1) - 1
                Dim cell As String = Convert.ToString(arr(i, j), CultureInfo.InvariantCulture)
                If String.Equals(cell, expectedText, StringComparison.OrdinalIgnoreCase) Then Return True
            Next
        Next

        Return False
    End Function


    Private Shared Sub AssertNoFiniteDifferenceFallbackWarnings(res As MixedModelResult,
                                                                label As String)
        Dim traceText As String = If(res.strTrace, String.Empty)
        Assert.IsFalse(traceText.IndexOf("one-sided finite difference", StringComparison.OrdinalIgnoreCase) >= 0,
                       label & ": stable synthetic data should not require one-sided KR finite-difference fallback. Trace: " & traceText)
        Assert.IsFalse(traceText.IndexOf("could not compute", StringComparison.OrdinalIgnoreCase) >= 0,
                       label & ": KR finite-difference derivative calculation should not report failed derivative computation. Trace: " & traceText)
    End Sub


    Private Shared Sub AssertFiniteSymmetricTensor3(tensor(,,) As Double,
                                                    label As String)
        Assert.IsNotNull(tensor, label & " should not be Nothing.")
        For h As Integer = 0 To tensor.GetLength(0) - 1
            AssertSymmetricFiniteSlice3(tensor, h, label & " slice " & h)
        Next
    End Sub


    Private Shared Sub AssertFiniteSymmetricTensor4(tensor(,,,) As Double,
                                                    label As String)
        Assert.IsNotNull(tensor, label & " should not be Nothing.")
        For h As Integer = 0 To tensor.GetLength(0) - 1
            For j As Integer = 0 To tensor.GetLength(1) - 1
                AssertSymmetricFiniteSlice4(tensor, h, j, label & " slice " & h & "," & j)
            Next
        Next
    End Sub


    Private Shared Sub AssertD2ParameterPairSymmetry(tensor(,,,) As Double,
                                                     label As String)
        For h As Integer = 0 To tensor.GetLength(0) - 1
            For j As Integer = 0 To tensor.GetLength(1) - 1
                For r As Integer = 0 To tensor.GetLength(2) - 1
                    For c As Integer = 0 To tensor.GetLength(3) - 1
                        Assert.AreEqual(tensor(h, j, r, c),
                                        tensor(j, h, r, c),
                                        0.000001,
                                        label & ": D2V(h,j) should equal D2V(j,h).")
                    Next
                Next
            Next
        Next
    End Sub


    Private Shared Sub AssertSymmetricFiniteSlice3(tensor(,,) As Double,
                                                   h As Integer,
                                                   label As String)
        Dim n As Integer = tensor.GetLength(1)
        For r As Integer = 0 To n - 1
            For c As Integer = 0 To n - 1
                AssertFinite(tensor(h, r, c), label & " entry " & r & "," & c)
                AssertAlmostEqualRelative(tensor(h, r, c), tensor(h, c, r), 0.000001, 0.0000000001, label & " should be symmetric.")
            Next
        Next
    End Sub


    Private Shared Sub AssertFiniteSlice4(tensor(,,,) As Double,
                                           h As Integer,
                                           j As Integer,
                                           label As String)
        Dim n As Integer = tensor.GetLength(2)
        For r As Integer = 0 To n - 1
            For c As Integer = 0 To n - 1
                AssertFinite(tensor(h, j, r, c), label & " entry " & r & "," & c)
            Next
        Next
    End Sub


    Private Shared Sub AssertTransposePair4(tensor(,,,) As Double,
                                            h As Integer,
                                            j As Integer,
                                            label As String)
        Dim n As Integer = tensor.GetLength(2)
        For r As Integer = 0 To n - 1
            For c As Integer = 0 To n - 1
                AssertFinite(tensor(h, j, r, c), label & " entry " & r & "," & c)
                AssertFinite(tensor(j, h, c, r), label & " transpose entry " & c & "," & r)
                AssertAlmostEqualRelative(tensor(h, j, r, c),
                                          tensor(j, h, c, r),
                                          0.000001,
                                          0.00000001,
                                          label & " should equal transpose of opposite parameter pair.")
            Next
        Next
    End Sub


    Private Shared Sub AssertAlmostEqualRelative(expected As Double,
                                                 actual As Double,
                                                 absoluteTolerance As Double,
                                                 relativeTolerance As Double,
                                                 label As String)
        Dim diff As Double = Math.Abs(expected - actual)
        Dim allowed As Double = Math.Max(absoluteTolerance,
                                         relativeTolerance * Math.Max(Math.Abs(expected), Math.Abs(actual)))
        Assert.IsTrue(diff <= allowed,
                      label & " Expected " & expected.ToString("G17", CultureInfo.InvariantCulture) &
                      ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) &
                      ", abs diff " & diff.ToString("G17", CultureInfo.InvariantCulture) &
                      " > allowed tolerance " & allowed.ToString("G17", CultureInfo.InvariantCulture) & ".")
    End Sub


    Private Shared Sub AssertSymmetricFiniteSlice4(tensor(,,,) As Double,
                                                   h As Integer,
                                                   j As Integer,
                                                   label As String)
        Dim n As Integer = tensor.GetLength(2)
        For r As Integer = 0 To n - 1
            For c As Integer = 0 To n - 1
                AssertFinite(tensor(h, j, r, c), label & " entry " & r & "," & c)
                AssertAlmostEqualRelative(tensor(h, j, r, c), tensor(h, j, c, r), 0.000001, 0.0000000001, label & " should be symmetric.")
            Next
        Next
    End Sub


    Private Shared Sub AssertSymmetricFiniteMatrix(a(,) As Double,
                                                   label As String)
        Assert.IsNotNull(a, label & " should not be Nothing.")
        Assert.AreEqual(a.GetLength(0), a.GetLength(1), label & " should be square.")

        For r As Integer = 0 To a.GetLength(0) - 1
            For c As Integer = 0 To a.GetLength(1) - 1
                AssertFinite(a(r, c), label & " entry " & r & "," & c)
                AssertAlmostEqualRelative(a(r, c), a(c, r), 0.000001, 0.0000000001, label & " should be symmetric.")
            Next
        Next
    End Sub


    Private Shared Sub AssertFinite(value As Double,
                                    label As String)
        Assert.IsFalse(Double.IsNaN(value), label & " should not be NaN.")
        Assert.IsFalse(Double.IsInfinity(value), label & " should not be infinite.")
    End Sub

End Class

' ===== END adaptive KR finite-difference derivative validation tests =====


' ===== BEGIN KR finite-difference option contract tests =====

<TestClass()>
Public Class MixedModelKRFiniteDifferenceOptionContractTests

    <TestMethod()>
    Public Sub KenwardRogerFiniteDifferenceOptions_ClonePreservesAllValues()
        Dim opts As New MixedModelKenwardRogerFiniteDifferenceOptions With {
            .FirstDerivativeStepScale = 0.0002,
            .SecondDerivativeStepScale = 0.0004,
            .MinimumStep = 0.00000005,
            .MaximumStep = 0.02,
            .MaxStepHalvings = 11,
            .UseRichardsonRefinement = False,
            .AllowOneSidedFirstDerivativeFallback = False,
            .RichardsonWarningRelativeTolerance = 0.125,
            .EmitPerturbedViCacheDiagnostics = False
        }

        Dim clone As MixedModelKenwardRogerFiniteDifferenceOptions = opts.Clone()

        Assert.AreNotSame(opts, clone)
        Assert.AreEqual(opts.FirstDerivativeStepScale, clone.FirstDerivativeStepScale, 0.0)
        Assert.AreEqual(opts.SecondDerivativeStepScale, clone.SecondDerivativeStepScale, 0.0)
        Assert.AreEqual(opts.MinimumStep, clone.MinimumStep, 0.0)
        Assert.AreEqual(opts.MaximumStep, clone.MaximumStep, 0.0)
        Assert.AreEqual(opts.MaxStepHalvings, clone.MaxStepHalvings)
        Assert.AreEqual(opts.UseRichardsonRefinement, clone.UseRichardsonRefinement)
        Assert.AreEqual(opts.AllowOneSidedFirstDerivativeFallback, clone.AllowOneSidedFirstDerivativeFallback)
        Assert.AreEqual(opts.RichardsonWarningRelativeTolerance, clone.RichardsonWarningRelativeTolerance, 0.0)
        Assert.AreEqual(opts.EmitPerturbedViCacheDiagnostics, clone.EmitPerturbedViCacheDiagnostics)
    End Sub


    <TestMethod()>
    Public Sub KenwardRogerOptions_CloneDeepCopiesFiniteDifferenceOptions()
        Dim kr As MixedModelKenwardRogerOptions = MixedModelKenwardRogerOptions.CreateFullMmrm()
        kr.FiniteDifferenceOptions.FirstDerivativeStepScale = 0.0003
        kr.FiniteDifferenceOptions.UseRichardsonRefinement = False

        Dim clone As MixedModelKenwardRogerOptions = kr.Clone()

        Assert.AreNotSame(kr, clone)
        Assert.AreNotSame(kr.FiniteDifferenceOptions, clone.FiniteDifferenceOptions)
        Assert.AreEqual(0.0003, clone.FiniteDifferenceOptions.FirstDerivativeStepScale, 0.0)
        Assert.IsFalse(clone.FiniteDifferenceOptions.UseRichardsonRefinement)

        clone.FiniteDifferenceOptions.FirstDerivativeStepScale = 0.0009
        Assert.AreEqual(0.0003, kr.FiniteDifferenceOptions.FirstDerivativeStepScale, 0.0,
                        "Changing cloned finite-difference options should not mutate the source KR options.")
    End Sub


    <TestMethod()>
    Public Sub KenwardRogerFiniteDifferenceOptions_ValidateRepairsInvalidValues()
        Dim opts As New MixedModelKenwardRogerFiniteDifferenceOptions With {
            .FirstDerivativeStepScale = Double.NaN,
            .SecondDerivativeStepScale = Double.PositiveInfinity,
            .MinimumStep = -1.0,
            .MaximumStep = 0.0,
            .MaxStepHalvings = 100,
            .RichardsonWarningRelativeTolerance = Double.NaN
        }

        opts.Validate()

        Assert.AreEqual(0.0001, opts.FirstDerivativeStepScale, 0.0)
        Assert.AreEqual(0.00025, opts.SecondDerivativeStepScale, 0.0)
        Assert.AreEqual(0.0000001, opts.MinimumStep, 0.0)
        Assert.IsTrue(opts.MaximumStep >= opts.MinimumStep)
        Assert.AreEqual(20, opts.MaxStepHalvings)
        Assert.AreEqual(0.25, opts.RichardsonWarningRelativeTolerance, 0.0)
    End Sub


    <TestMethod()>
    Public Sub KenwardRogerFiniteDifferenceDiagnostics_StatusAndWarningSummary_AreDeterministic()
        Dim d As New MixedModelKrFiniteDifferenceDiagnostics()

        Assert.AreEqual("OK", d.QualityStatus())
        Assert.AreEqual(String.Empty, d.WarningSummary())
        Assert.IsTrue(d.SummaryText().Contains("status=OK"))

        d.FirstDerivativeOneSidedFallbackCount = 2
        Assert.AreEqual("Warning", d.QualityStatus())
        Assert.IsTrue(d.WarningSummary().Contains("one-sided first-derivative fallbacks=2"))

        d.SecondDerivativeFailedCount = 1
        Assert.AreEqual("Failed", d.QualityStatus())
        Assert.IsTrue(d.WarningSummary().Contains("second derivative failures=1"))

        d.SecondDerivativeFailedCount = 0
        d.FirstDerivativeOneSidedFallbackCount = 0
        d.MaxSecondDerivativeRichardsonRelativeChange = 0.5
        Assert.AreEqual("Warning", d.QualityStatus(0.25))
        Assert.IsTrue(d.WarningSummary(0.25).Contains("large Richardson change"))
    End Sub


    <TestMethod()>
    Public Sub FullMmrmAndFullLmmOptions_CreateUsableFiniteDifferenceContracts()
        Dim mmrm As MixedModelKenwardRogerOptions = MixedModelKenwardRogerOptions.CreateFullMmrm()
        Dim lmm As MixedModelKenwardRogerOptions = MixedModelKenwardRogerOptions.CreateFullLmm()

        Assert.IsNotNull(mmrm.FiniteDifferenceOptions)
        Assert.IsNotNull(lmm.FiniteDifferenceOptions)
        Assert.AreNotSame(mmrm.FiniteDifferenceOptions, lmm.FiniteDifferenceOptions)

        mmrm.FiniteDifferenceOptions.Validate()
        lmm.FiniteDifferenceOptions.Validate()

        Assert.IsTrue(mmrm.FiniteDifferenceOptions.FirstDerivativeStepScale > 0.0)
        Assert.IsTrue(mmrm.FiniteDifferenceOptions.SecondDerivativeStepScale > 0.0)
        Assert.IsTrue(lmm.FiniteDifferenceOptions.MinimumStep > 0.0)
        Assert.IsTrue(lmm.FiniteDifferenceOptions.MaximumStep >= lmm.FiniteDifferenceOptions.MinimumStep)
    End Sub

End Class

' ===== END KR finite-difference option contract tests =====
