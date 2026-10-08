Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.regression

<TestClass>
Public Class MixedModelKenwardRogerCoreTests

    <TestMethod>
    Public Sub CovarianceParameterScale_RandomIntercept_RoundTrips()
        Dim y() As Double = {1.1R, 2.9R, 0.8R, 3.2R, 1.1R, 2.9R, 1.0R, 3.1R}
        Dim subject() As Object = {"S1", "S1", "S2", "S2", "S3", "S3", "S4", "S4"}
        Dim visit() As Double = {0.0R, 1.0R, 0.0R, 1.0R, 0.0R, 1.0R, 0.0R, 1.0R}
        Dim x(y.Length - 1, 1) As Double
        Dim z(y.Length - 1, 0) As Double
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0R
            x(i, 1) = visit(i)
            z(i, 0) = 1.0R
        Next

        Dim data As MixedModelBlockData = MixedModelBlockData.FromArrays(
            y:=y, x:=x, subjectId:=subject, z:=z, visit:=visit, sortWithinSubjectByVisit:=True)
        Dim request As MixedModelFitRequest = MixedModelFitRequest.CreateLMM(
            data, New IdentityR(), New RandomIntercept(), MixedModelFitMethod.REML)

        Dim optimizerTheta() As Double = {Math.Log(0.5R), Math.Log(0.25R)}
        Dim covarianceTheta() As Double = Nothing
        Dim names() As String = Nothing
        Dim diagnostic As String = Nothing
        Assert.IsTrue(MixedModelCovarianceParameterScale.TryOptimizerToCovarianceTheta(
            request, optimizerTheta, covarianceTheta, names, diagnostic), diagnostic)
        Assert.AreEqual(0.5R, covarianceTheta(0), 0.0000000001R)
        Assert.AreEqual(0.25R, covarianceTheta(1), 0.0000000001R)

        Dim roundTrip() As Double = Nothing
        Assert.IsTrue(MixedModelCovarianceParameterScale.TryCovarianceToOptimizerTheta(
            request, covarianceTheta, roundTrip, diagnostic), diagnostic)
        Assert.AreEqual(optimizerTheta(0), roundTrip(0), 0.0000000001R)
        Assert.AreEqual(optimizerTheta(1), roundTrip(1), 0.0000000001R)
    End Sub

    <TestMethod>
    Public Sub InferenceWorkspace_Satterthwaite_UsesThetaCovarianceAndGradient()
        Dim gradient(0, 0, 0) As Double
        gradient(0, 0, 0) = 0.2R
        Dim workspace As New MixedModelInferenceWorkspace With {
            .P = 1, .K = 1,
            .VarBeta = New Double(,) {{1.0R}},
            .ThetaCovariance = New Double(,) {{0.01R}},
            .VarBetaGradient = gradient
        }
        Dim df As Double = Double.NaN
        Assert.IsTrue(MixedModelInferenceMath.TrySatterthwaiteDF({1.0R}, workspace, df))
        Assert.AreEqual(5000.0R, df, 0.000001R)
    End Sub

    <TestMethod>
    Public Sub KenwardRogerBackend_ScalarLinearAdjustment_IsFinite()
        Dim workspace As MixedModelKrWorkspace = BuildScalarWorkspace()
        Dim diagnostic As String = Nothing
        Assert.IsTrue(MixedModelKenwardRogerBackend.TryBuildKrMatrices(workspace, diagnostic), diagnostic)
        Dim adjusted(,) As Double = Nothing
        Assert.IsTrue(MixedModelKenwardRogerBackend.TryComputeAdjustedVarBeta(workspace, adjusted, diagnostic), diagnostic)
        Assert.IsNotNull(adjusted)
        Assert.IsFalse(Double.IsNaN(adjusted(0, 0)))
        Assert.IsFalse(Double.IsInfinity(adjusted(0, 0)))
        Assert.AreEqual(MixedModelKenwardRogerAdjustmentKind.Linear, workspace.AdjustmentUsed)
    End Sub

    <TestMethod>
    Public Sub KenwardRogerInferenceCore_DfScaling_IsCachedAndRankReduces()
        Dim workspace As MixedModelKrWorkspace = BuildScalarWorkspace()
        Dim diagnostic As String = Nothing
        Assert.IsTrue(MixedModelKenwardRogerBackend.TryBuildKrMatrices(workspace, diagnostic), diagnostic)

        Dim redundant(,) As Double = {{1.0R}, {2.0R}}
        Dim first As MixedModelKenwardRogerDfResult = Nothing
        Assert.IsTrue(MixedModelKenwardRogerInferenceCore.TryComputeDegreesOfFreedomAndScaling(
            workspace, redundant, first, diagnostic), diagnostic)
        Assert.AreEqual(1, first.NumDF)
        Assert.IsTrue(first.DenDF > 0.0R)
        Assert.IsTrue(first.Lambda > 0.0R)

        Dim second As MixedModelKenwardRogerDfResult = Nothing
        Dim secondDiagnostic As String = Nothing
        Assert.IsTrue(MixedModelKenwardRogerInferenceCore.TryComputeDegreesOfFreedomAndScaling(
            workspace, redundant, second, secondDiagnostic), secondDiagnostic)
        Assert.AreEqual(first.DenDF, second.DenDF, 0.000000000001R)
        Assert.AreEqual(first.Lambda, second.Lambda, 0.000000000001R)
        StringAssert.Contains(secondDiagnostic, "Reused cached KR DF/scaling")
    End Sub

    Private Shared Function BuildScalarWorkspace() As MixedModelKrWorkspace
        Dim dv(0, 1, 1) As Double
        dv(0, 0, 0) = 1.0R
        dv(0, 1, 1) = 1.0R
        Dim block As New MixedModelKrBlock With {
            .X = New Double(,) {{1.0R}, {1.0R}},
            .VInv = New Double(,) {{1.0R, 0.0R}, {0.0R, 1.0R}},
            .dv = dv
        }
        Return New MixedModelKrWorkspace With {
            .P = 1, .K = 1,
            .VarBeta = New Double(,) {{1.0R}},
            .ThetaCovariance = New Double(,) {{0.01R}},
            .Blocks = New List(Of MixedModelKrBlock) From {block},
            .AdjustmentKind = MixedModelKenwardRogerAdjustmentKind.Linear,
            .AllowLinearFallback = False
        }
    End Function

End Class
