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

' ===== BEGIN migrated from MixedModelReferenceTests.vb =====



''' <summary>
''' Reference-validation tests for the first Gaussian mixed-model engine path.
''' </summary>
''' <remarks>
''' <para>
''' These tests are intentionally separate from <c>MixedModelSmokeTests</c>.
''' The smoke tests answer whether the new mixed-model path runs and returns sane objects.
''' This file starts the next layer: deterministic numerical comparisons against values generated
''' by a small R reference script.
''' </para>
''' <para>
''' The companion R script is <c>R_referenceScripts/mixed_model_reference.R</c>.  It reads the CSV
''' files in <c>TestData</c> and regenerates the reference CSV files consumed here.  The first test
''' uses ordinary <c>lm</c> as the reference for the MMRM identity-residual special case; the second
''' uses <c>nlme::lme</c> for a random-intercept LMM.
''' </para>
''' <para>
''' These are early reference tests.  They deliberately use moderate tolerances for the covariance
''' parameters and log-likelihood because the mixed-model optimizer is still young.  Tighten these
''' tolerances after the optimizer and covariance parameterization stabilize.
''' </para>
''' </remarks>
<TestClass>
Public Class MixedModelReferenceTests

    Private Const TOL_MMRM_BETA As Double = 0.0000001
    Private Const TOL_MMRM_SE As Double = 0.000001
    Private Const TOL_MMRM_LOGLIK As Double = 0.000001
    Private Const TOL_MMRM_VAR As Double = 0.000001

    Private Const TOL_LMM_BETA As Double = 0.005
    Private Const TOL_LMM_VAR As Double = 0.01
    Private Const TOL_LMM_LOGLIK As Double = 0.05

    ''' <summary>
    ''' MMRM with identity residual covariance is equivalent to ordinary Gaussian regression
    ''' when fitted by ML.  This test compares fixed effects, ML residual variance, standard
    ''' errors, and log-likelihood against the R <c>lm</c>-based reference calculation.
    ''' </summary>
    <TestMethod>
    Public Sub MMRM_IdentityResidual_MatchesRReference()
        Dim dat As MixedCsvData = LoadMixedCsv("mixedmodel_mmrm_identity_data.csv")
        Dim ref As Dictionary(Of String, Double) = LoadReferenceCsv("mixedmodel_mmrm_identity_reference.csv")

        Dim x(,) As Double = BuildInterceptVisitX(dat.Visit)

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Y,
                                                                              x:=x,
                                                                              subjectId:=dat.Subject,
                                                                              z:=Nothing,
                                                                              visit:=dat.Visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New IdentityR(),
                                                                          MixedModelFitMethod.ML)
        req.RequestLabel = "MMRM identity reference test"
        req.ResponseVarName = "y"
        req.SubjectVarName = "subject"
        req.VisitVarName = "visit"
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.Control = ReferenceControl()
        req.StartThetaR = {Math.Log(ref("sigma2_ml"))}

        Dim fit As New MMRM(req)
        Dim res As MixedModelResult = fit.Fit()

        AssertBasicResult(res, expectedP:=2, expectedN:=dat.Y.Length)
        Assert.AreEqual(ref("beta_intercept"), res.Beta(0), TOL_MMRM_BETA, "MMRM(identity) intercept mismatch.")
        Assert.AreEqual(ref("beta_visit"), res.Beta(1), TOL_MMRM_BETA, "MMRM(identity) visit effect mismatch.")
        Assert.AreEqual(ref("se_intercept"), res.BetaSE(0), TOL_MMRM_SE, "MMRM(identity) intercept SE mismatch.")
        Assert.AreEqual(ref("se_visit"), res.BetaSE(1), TOL_MMRM_SE, "MMRM(identity) visit SE mismatch.")

        Dim sigma2 As Double = BuildIdentityResidualVariance(res, blockData)
        Assert.AreEqual(ref("sigma2_ml"), sigma2, TOL_MMRM_VAR, "MMRM(identity) ML residual variance mismatch.")
        Assert.AreEqual(ref("logLik_ml"), res.LogLik, TOL_MMRM_LOGLIK, "MMRM(identity) log-likelihood mismatch.")
    End Sub

    ''' <summary>
    ''' Random-intercept LMM compared with the companion R <c>nlme::lme</c> reference.
    ''' The dataset is balanced and deterministic so this test should be stable, while still
    ''' validating the G-side path <c>Z_i G Z_i' + R_i</c>.
    ''' </summary>
    <TestMethod>
    Public Sub LMM_RandomIntercept_MatchesNlmeReference()
        Dim dat As MixedCsvData = LoadMixedCsv("mixedmodel_lmm_random_intercept_data.csv")
        Dim ref As Dictionary(Of String, Double) = LoadReferenceCsv("mixedmodel_lmm_random_intercept_reference.csv")

        Dim x(,) As Double = BuildInterceptVisitX(dat.Visit)
        Dim z(,) As Double = BuildRandomInterceptZ(dat.Y.Length)

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Y,
                                                                              x:=x,
                                                                              subjectId:=dat.Subject,
                                                                              z:=z,
                                                                              visit:=dat.Visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateLMM(blockData,
                                                                         New IdentityR(),
                                                                         New RandomIntercept(),
                                                                         MixedModelFitMethod.REML)
        req.RequestLabel = "LMM random-intercept reference test"
        req.ResponseVarName = "y"
        req.SubjectVarName = "subject"
        req.VisitVarName = "visit"
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.RandomEffectNames = {"Intercept"}
        req.Control = ReferenceControl()

        ' Start close to the external reference. This keeps the reference test focused on the
        ' engine/l likelihood path rather than on global-start robustness during early development.
        req.StartThetaG = {Math.Log(ref("var_random_intercept"))}
        req.StartThetaR = {Math.Log(ref("var_residual"))}

        Dim fit As New LMM(req)
        Dim res As MixedModelResult = fit.Fit()

        AssertBasicResult(res, expectedP:=2, expectedN:=dat.Y.Length)
        Assert.AreEqual(1, res.Q, "Random-intercept reference test should have one G-side column.")
        Assert.AreEqual(ref("beta_intercept"), res.Beta(0), TOL_LMM_BETA, "LMM random-intercept beta intercept mismatch.")
        Assert.AreEqual(ref("beta_visit"), res.Beta(1), TOL_LMM_BETA, "LMM random-intercept beta visit mismatch.")

        Dim varB As Double = BuildRandomInterceptVariance(res, blockData)
        Dim varE As Double = BuildIdentityResidualVariance(res, blockData)
        Assert.AreEqual(ref("var_random_intercept"), varB, TOL_LMM_VAR, "LMM random-intercept variance mismatch.")
        Assert.AreEqual(ref("var_residual"), varE, TOL_LMM_VAR, "LMM residual variance mismatch.")
        Assert.AreEqual(ref("logLik_reml"), res.LogLik, TOL_LMM_LOGLIK, "LMM REML log-likelihood mismatch.")
    End Sub

    Private Shared Function ReferenceControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 160
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function

    Private Shared Function BuildInterceptVisitX(visit() As Double) As Double(,)
        Dim n As Integer = visit.Length
        Dim x(n - 1, 1) As Double
        For i As Integer = 0 To n - 1
            x(i, 0) = 1.0
            x(i, 1) = visit(i)
        Next
        Return x
    End Function

    Private Shared Function BuildRandomInterceptZ(n As Integer) As Double(,)
        Dim z(n - 1, 0) As Double
        For i As Integer = 0 To n - 1
            z(i, 0) = 1.0
        Next
        Return z
    End Function

    Private Shared Function BuildIdentityResidualVariance(res As MixedModelResult,
                                                          blockData As MixedModelBlockData) As Double
        Assert.IsNotNull(res.ThetaR, "ThetaR should not be Nothing.")
        Assert.IsTrue(res.ThetaR.Length >= 1, "ThetaR should contain at least one residual parameter.")
        Dim rStruct As New IdentityR()
        Dim rMat(,) As Double = rStruct.BuildRi(res.ThetaR, blockData.GetBlock(0), blockData)
        Return rMat(0, 0)
    End Function

    Private Shared Function BuildRandomInterceptVariance(res As MixedModelResult,
                                                         blockData As MixedModelBlockData) As Double
        Assert.IsNotNull(res.ThetaG, "ThetaG should not be Nothing.")
        Assert.IsTrue(res.ThetaG.Length >= 1, "ThetaG should contain at least one random-effect parameter.")
        Dim gStruct As New RandomIntercept()
        Dim gMat(,) As Double = gStruct.BuildG(res.ThetaG, blockData.Q)
        Return gMat(0, 0)
    End Function

    Private Shared Sub AssertBasicResult(res As MixedModelResult,
                                         expectedP As Integer,
                                         expectedN As Integer)
        Assert.IsNotNull(res, "Mixed-model fit returned Nothing.")
        Assert.AreEqual(expectedP, res.P, "Unexpected fixed-effect dimension.")
        Assert.AreEqual(expectedN, res.Nobs, "Unexpected observation count.")
        Assert.IsNotNull(res.Beta, "Beta vector is Nothing.")
        Assert.AreEqual(expectedP, res.Beta.Length, "Unexpected beta-vector length.")
        AssertFiniteVector(res.Beta, "Beta")
        AssertFiniteVector(res.BetaSE, "BetaSE")
        AssertFinite(res.LogLik, "LogLik")
        AssertFinite(res.Objective, "Objective")
    End Sub

    Private Shared Sub AssertFiniteVector(values() As Double, name As String)
        Assert.IsNotNull(values, name & " vector is Nothing.")
        For i As Integer = 0 To values.Length - 1
            AssertFinite(values(i), name & "[" & i.ToString(CultureInfo.InvariantCulture) & "]")
        Next
    End Sub

    Private Shared Sub AssertFinite(value As Double, name As String)
        Assert.IsFalse(Double.IsNaN(value), name & " is NaN.")
        Assert.IsFalse(Double.IsInfinity(value), name & " is infinite.")
    End Sub

    Private Shared Function LoadMixedCsv(fileName As String) As MixedCsvData
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must contain a header and at least one row: " & fileName)

        Dim header() As String = SplitCsvLine(lines(0))
        Dim col As Dictionary(Of String, Integer) = HeaderIndex(header)
        RequireColumn(col, "subject", fileName)
        RequireColumn(col, "visit", fileName)
        RequireColumn(col, "y", fileName)

        Dim n As Integer = lines.Length - 1
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double
        Dim y(n - 1) As Double

        For i As Integer = 0 To n - 1
            Dim parts() As String = SplitCsvLine(lines(i + 1))
            subject(i) = parts(col("subject")).Trim()
            visit(i) = ParseDouble(parts(col("visit")))
            y(i) = ParseDouble(parts(col("y")))
        Next

        Return New MixedCsvData With {.Subject = subject, .Visit = visit, .Y = y}
    End Function

    Private Shared Function LoadReferenceCsv(fileName As String) As Dictionary(Of String, Double)
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("Reference CSV must contain a header and at least one row: " & fileName)

        Dim out As New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
            Dim parts() As String = SplitCsvLine(lines(i))
            If parts.Length < 2 Then Throw New InvalidOperationException("Invalid reference CSV row: " & lines(i))
            out(parts(0).Trim()) = ParseDouble(parts(1))
        Next
        Return out
    End Function

    Private Shared Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim candidates As String() = {
            Path.Combine(baseDir, fileName),
            Path.Combine(baseDir, "TestData", fileName),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\TestData", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\..\TestData", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\..\..\TestData", fileName))
        }

        For Each candidate As String In candidates
            If File.Exists(candidate) Then Return candidate
        Next

        Throw New FileNotFoundException("Test data file not found", fileName)
    End Function

    Private Shared Function HeaderIndex(header() As String) As Dictionary(Of String, Integer)
        Dim out As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            out(header(i).Trim()) = i
        Next
        Return out
    End Function

    Private Shared Sub RequireColumn(col As Dictionary(Of String, Integer), columnName As String, fileName As String)
        If Not col.ContainsKey(columnName) Then
            Throw New InvalidOperationException("CSV file '" & fileName & "' must contain column '" & columnName & "'.")
        End If
    End Sub

    Private Shared Function SplitCsvLine(line As String) As String()
        ' The supplied test/reference CSVs are intentionally simple and do not contain quoted commas.
        Return line.Split(","c)
    End Function

    Private Shared Function ParseDouble(s As String) As Double
        Return Double.Parse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function

    Private Class MixedCsvData
        Public Subject() As Object
        Public Visit() As Double
        Public Y() As Double
    End Class

End Class

' ===== END migrated from MixedModelReferenceTests.vb =====

