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

' ===== BEGIN migrated from MixedModelSmokeTests.vb =====



''' <summary>
''' Early smoke tests for the shared Gaussian mixed-model engine.
''' </summary>
''' <remarks>
''' These tests are intentionally lightweight.  They are meant to verify that the new
''' MixedModelBlockData -> MixedModelFitRequest -> LMM/MMRM wrapper -> MixedModelEngine path
''' is runnable and produces finite core outputs before the UI, formula service, and UDF
''' layers are built on top of it.
'''
''' They are not intended to replace later reference-validation tests against lme4/nlme/mmrm.
''' Once the engine stabilizes, add separate benchmark tests for fixed effects, likelihoods,
''' covariance parameters, BLUPs, and residuals using external reference outputs.
''' </remarks>
<TestClass>
Public Class MixedModelSmokeTests

    Private Const TOL_BETA_EXACT As Double = 0.000001
    Private Const TOL_BETA_LOOSE As Double = 0.75

    ''' <summary>
    ''' MMRM with identity residual covariance should reduce to ordinary Gaussian regression
    ''' for the fixed effects.  This gives a deterministic first smoke test of the R-side-only
    ''' path V_i = R_i.
    ''' </summary>
    <TestMethod>
    Public Sub MMRM_IdentityResidual_MatchesOlsFixedEffects()
        Dim y() As Double = {1.1, 2.9,
                             0.8, 3.2,
                             1.1, 2.9}

        Dim x(5, 1) As Double
        Dim subjectId() As Object = {"S1", "S1", "S2", "S2", "S3", "S3"}
        Dim visit() As Double = {0.0, 1.0, 0.0, 1.0, 0.0, 1.0}

        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
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
        req.RequestLabel = "MMRM identity smoke test"
        req.ResponseVarName = "y"
        req.SubjectVarName = "subject"
        req.VisitVarName = "visit"
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.Control = SmokeControl()

        Dim fit As New MMRM(req)
        Dim res As MixedModelResult = fit.Fit()

        AssertBasicMixedModelResult(res, expectedP:=2, expectedN:=6, requireFiniteObjective:=True)

        ' The residuals were chosen to be orthogonal to the intercept and visit columns,
        ' so the OLS fixed effects are exactly beta0 = 1 and beta_visit = 2.
        Assert.AreEqual(1.0, res.Beta(0), TOL_BETA_EXACT, "MMRM(identity) intercept should match OLS.")
        Assert.AreEqual(2.0, res.Beta(1), TOL_BETA_EXACT, "MMRM(identity) visit slope should match OLS.")
        Assert.AreEqual(6, res.FittedMarginal.Length, "Expected one fitted value per input row.")
        Assert.AreEqual(6, res.ResidualRaw.Length, "Expected one residual per input row.")
    End Sub

    ''' <summary>
    ''' Random-intercept LMM smoke test.  This does not assert reference-quality variance
    ''' components yet; it verifies that the G-side path Z_i G Z_i' + R_i is numerically
    ''' runnable and returns finite fixed effects and subject-level BLUP containers.
    ''' </summary>
    <TestMethod>
    Public Sub LMM_RandomIntercept_RunsAndReturnsFiniteCoreOutputs()
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
        req.RequestLabel = "LMM random-intercept smoke test"
        req.ResponseVarName = "y"
        req.SubjectVarName = "subject"
        req.VisitVarName = "visit"
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.RandomEffectNames = {"Intercept"}
        req.Control = SmokeControl()

        ' Stable, conservative starts on the internal scale.
        req.StartThetaG = {Math.Log(0.5)}
        req.StartThetaR = {Math.Log(0.05)}

        Dim fit As New LMM(req)
        Dim res As MixedModelResult = fit.Fit()

        AssertBasicMixedModelResult(res, expectedP:=2, expectedN:=y.Length, requireFiniteObjective:=True)
        Assert.AreEqual(data.NoSubjects, res.NoSubjects, "Result should report the blocked subject count.")
        Assert.AreEqual(1, res.Q, "Random-intercept smoke test should have one random-effects column.")
        Assert.IsTrue(res.ThetaG IsNot Nothing AndAlso res.ThetaG.Length = 1, "Expected one G-side covariance parameter.")
        Assert.IsTrue(res.ThetaR IsNot Nothing AndAlso res.ThetaR.Length = 1, "Expected one R-side covariance parameter.")

        ' This is a smoke tolerance, not a reference-validation tolerance.  The toy dataset was
        ' generated around y = 10 + 1.5 * visit + subject_random_intercept + small residual.
        Assert.AreEqual(10.0, res.Beta(0), TOL_BETA_LOOSE, "Random-intercept intercept is unexpectedly far from the toy-data generating value.")
        Assert.AreEqual(1.5, res.Beta(1), TOL_BETA_LOOSE, "Random-intercept visit slope is unexpectedly far from the toy-data generating value.")

        Assert.IsNotNull(res.RandomEffects, "RandomEffects dictionary should be initialized.")
        Assert.IsTrue(res.RandomEffects.Count > 0, "Expected at least one subject-level BLUP vector in the random-intercept fit.")

        For Each kvp As KeyValuePair(Of String, Double()) In res.RandomEffects
            Assert.IsNotNull(kvp.Value, "BLUP vector should not be Nothing for subject " & kvp.Key)
            Assert.AreEqual(1, kvp.Value.Length, "Random-intercept BLUP vector should have length 1 for subject " & kvp.Key)
            AssertFinite(kvp.Value(0), "BLUP for subject " & kvp.Key)
        Next
    End Sub

    Private Shared Function SmokeControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 80
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function

    Private Shared Sub BuildBalancedRandomInterceptToyData(ByRef y() As Double,
                                                           ByRef x(,) As Double,
                                                           ByRef z(,) As Double,
                                                           ByRef subjectId() As Object,
                                                           ByRef visit() As Double)
        Dim nSubjects As Integer = 6
        Dim nVisits As Integer = 3
        Dim n As Integer = nSubjects * nVisits

        ReDim y(n - 1)
        ReDim x(n - 1, 1)
        ReDim z(n - 1, 0)
        ReDim subjectId(n - 1)
        ReDim visit(n - 1)

        Dim subjectEffects() As Double = {-1.0, -0.6, -0.2, 0.2, 0.6, 1.0}
        Dim residuals() As Double = {-0.05, 0.04, 0.01,
                                      0.02, -0.03, 0.01,
                                      0.01, 0.02, -0.03,
                                      -0.02, 0.03, -0.01,
                                      0.04, -0.01, -0.03,
                                      -0.01, -0.02, 0.03}

        Dim r As Integer = 0
        For s As Integer = 0 To nSubjects - 1
            For v As Integer = 0 To nVisits - 1
                subjectId(r) = "S" & (s + 1).ToString()
                visit(r) = CDbl(v)
                x(r, 0) = 1.0
                x(r, 1) = CDbl(v)
                z(r, 0) = 1.0
                y(r) = 10.0 + 1.5 * CDbl(v) + subjectEffects(s) + residuals(r)
                r += 1
            Next
        Next
    End Sub

    Private Shared Sub AssertBasicMixedModelResult(res As MixedModelResult,
                                                   expectedP As Integer,
                                                   expectedN As Integer,
                                                   requireFiniteObjective As Boolean)
        Assert.IsNotNull(res, "Mixed-model fit returned Nothing.")
        Assert.AreEqual(expectedP, res.P, "Unexpected number of fixed-effect columns.")
        Assert.AreEqual(expectedN, res.Nobs, "Unexpected number of observations.")
        Assert.IsNotNull(res.Beta, "Beta vector should not be Nothing.")
        Assert.AreEqual(expectedP, res.Beta.Length, "Unexpected beta-vector length.")
        AssertFiniteVector(res.Beta, "Beta")

        Assert.IsNotNull(res.BetaSE, "BetaSE vector should not be Nothing.")
        Assert.AreEqual(expectedP, res.BetaSE.Length, "Unexpected fixed-effect SE-vector length.")

        If requireFiniteObjective Then
            AssertFinite(res.Objective, "Objective")
            AssertFinite(res.LogLik, "LogLik")
        End If
    End Sub

    Private Shared Sub AssertFiniteVector(values() As Double, label As String)
        Assert.IsNotNull(values, label & " vector should not be Nothing.")
        For i As Integer = 0 To values.Length - 1
            AssertFinite(values(i), label & "[" & i.ToString() & "]")
        Next
    End Sub

    Private Shared Sub AssertFinite(value As Double, label As String)
        Assert.IsFalse(Double.IsNaN(value), label & " should not be NaN.")
        Assert.IsFalse(Double.IsInfinity(value), label & " should not be infinite.")
    End Sub

End Class

' ===== END migrated from MixedModelSmokeTests.vb =====

' ===== BEGIN migrated from MixedModelFormulaServiceTests.vb =====



''' <summary>
''' Unit tests for the mixed-model formula/data bridge.
''' </summary>
''' <remarks>
''' These tests intentionally avoid direct calls to DataObj.DataImportRawMatrix from the test
''' project.  DataImportRawMatrix exposes Microsoft.Office.Interop.Excel.Worksheet in its public
''' signature, and the test project does not need an Excel interop reference just to validate the
''' mixed-model formula service.  Instead the tests call MixedModelFormulaService.BuildRequestFromRawMatrix,
''' which keeps the Excel-interop-dependent import detail inside the main BESHStatNG assembly.
''' </remarks>
<TestClass>
Public Class MixedModelFormulaServiceTests

    Private Const TOL_EXACT As Double = 0.0000001
    Private Const TOL_SMOKE As Double = 0.75

    ''' <summary>
    ''' A right-hand-side-only fixed-effects formula should build an MMRM/identity request when
    ''' the random-effects structure is set to None.  The response is supplied separately and is
    ''' deliberately not part of the formula text.
    ''' </summary>
    <TestMethod>
    <TestCategory("MixedModelFormula")>
    Public Sub FormulaService_MMRM_RhsOnlyFormula_BuildsRequestAndFitsIdentityModel()
        Dim raw(,) As Object = BuildTwoVisitRawMatrix()
        Dim y() As Double = {1.1, 2.9,
                             0.8, 3.2,
                             1.1, 2.9}

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

        Assert.IsNotNull(req, "Expected mixed-model request.")
        Assert.IsTrue(req.IsMMRM(), "A None random-effects structure should produce an MMRM/R-side-only request.")
        Assert.AreEqual("visit", req.FixedFormulaText, "Unexpected stored fixed formula.")
        Assert.AreEqual(String.Empty, If(req.RandomFormulaText, String.Empty), "MMRM request should not store a random-effects parser formula.")

        AssertStringArrayEqual(New String() {"Intercept", "visit"}, req.FixedEffectNames, "Fixed-effect names.")
        Assert.IsTrue(req.RandomEffectNames Is Nothing OrElse req.RandomEffectNames.Length = 0, "MMRM request should not have random-effect names.")
        Assert.AreEqual(2, req.Data.P, "Unexpected fixed-design column count.")
        Assert.AreEqual(0, req.Data.Q, "Unexpected random-design column count for MMRM.")
        Assert.AreEqual(6, req.Data.Nobs, "Unexpected observation count.")
        Assert.AreEqual(3, req.Data.NoSubjects, "Unexpected subject count.")

        req.Control = FastFormulaTestControl()
        req.StartThetaR = {Math.Log(0.1)}

        Dim fit As New MMRM(req)
        Dim res As MixedModelResult = fit.Fit()

        AssertBasicResult(res, expectedP:=2, expectedN:=y.Length)
        Assert.AreEqual(1.0, res.Beta(0), TOL_EXACT, "MMRM identity fixed intercept should match OLS.")
        Assert.AreEqual(2.0, res.Beta(1), TOL_EXACT, "MMRM identity fixed visit effect should match OLS.")
    End Sub

    ''' <summary>
    ''' A random-intercept formula should build a non-degenerate LMM request with a one-column Z
    ''' matrix.  This validates the service path used later by UI/UDF code before building a full
    ''' dialog surface.
    ''' </summary>
    <TestMethod>
    <TestCategory("MixedModelFormula")>
    Public Sub FormulaService_LMM_RandomInterceptFormula_BuildsRequestAndFits()
        Dim raw(,) As Object = Nothing
        Dim y() As Double = Nothing
        BuildRandomInterceptRawMatrix(raw, y)

        Dim req As MixedModelFitRequest = MixedModelFormulaService.BuildRequestFromRawMatrix(rawInput:=raw,
                                                                                             variableNames:=New String() {"subject", "visit"},
                                                                                             response:=y,
                                                                                             fixedFormulaText:="visit",
                                                                                             subjectKey:="subject",
                                                                                             responseName:="y",
                                                                                             randomFormulaText:="(1 | subject)",
                                                                                             visitKey:="visit",
                                                                                             fitMethod:=MixedModelFitMethod.REML,
                                                                                             residualStructType:="Identity",
                                                                                             randomStructType:="Random Intercept")

        Assert.IsNotNull(req, "Expected mixed-model request.")
        Assert.IsFalse(req.IsMMRM(), "Random-intercept formula should produce an LMM request.")
        AssertStringArrayEqual(New String() {"Intercept", "visit"}, req.FixedEffectNames, "Fixed-effect names.")
        AssertStringArrayEqual(New String() {"Intercept"}, req.RandomEffectNames, "Random-effect names.")
        Assert.AreEqual("visit", req.FixedFormulaText, "Unexpected stored fixed formula.")
        Assert.AreEqual(String.Empty, If(req.RandomFormulaText, String.Empty), "Random intercept has no non-intercept parser terms.")
        Assert.AreEqual(2, req.Data.P, "Unexpected fixed-design column count.")
        Assert.AreEqual(1, req.Data.Q, "Unexpected random-design column count.")
        Assert.AreEqual(18, req.Data.Nobs, "Unexpected observation count.")
        Assert.AreEqual(6, req.Data.NoSubjects, "Unexpected subject count.")

        req.Control = FastFormulaTestControl()
        req.StartThetaG = {Math.Log(0.5)}
        req.StartThetaR = {Math.Log(0.05)}

        Dim fit As New LMM(req)
        Dim res As MixedModelResult = fit.Fit()

        AssertBasicResult(res, expectedP:=2, expectedN:=y.Length)
        Assert.AreEqual(1, res.Q, "Expected one random-effect column.")
        Assert.AreEqual(10.0, res.Beta(0), TOL_SMOKE, "Random-intercept formula-service fit has an unexpected intercept.")
        Assert.AreEqual(1.5, res.Beta(1), TOL_SMOKE, "Random-intercept formula-service fit has an unexpected visit slope.")
        Assert.IsNotNull(res.RandomEffects, "RandomEffects dictionary should be initialized.")
        Assert.IsTrue(res.RandomEffects.Count > 0, "Expected subject-level BLUPs for the random-intercept fit.")
    End Sub

    ''' <summary>
    ''' Mixed-model formulas are intentionally RHS-only because y is supplied as a separate UDF/UI
    ''' argument.  A formula containing a left-hand side should fail early with a clear message.
    ''' </summary>
    <TestMethod>
    <TestCategory("MixedModelFormula")>
    Public Sub FormulaService_FixedFormulaWithLeftHandSide_ReturnsHelpfulError()
        Dim raw(,) As Object = BuildTwoVisitRawMatrix()
        Dim y() As Double = {1.1, 2.9,
                             0.8, 3.2,
                             1.1, 2.9}

        Dim ex As Exception = Nothing

        Try
            Dim req As MixedModelFitRequest = MixedModelFormulaService.BuildRequestFromRawMatrix(rawInput:=raw,
                                                                                                  variableNames:=New String() {"subject", "visit"},
                                                                                                  response:=y,
                                                                                                  fixedFormulaText:="y ~ visit",
                                                                                                  subjectKey:="subject",
                                                                                                  responseName:="y",
                                                                                                  randomFormulaText:=Nothing,
                                                                                                  visitKey:="visit",
                                                                                                  fitMethod:=MixedModelFitMethod.ML,
                                                                                                  residualStructType:="Identity",
                                                                                                  randomStructType:="None")
            Assert.Fail("Formula-service build should reject formulas containing a left-hand side.")
        Catch caught As Exception
            ex = caught
        End Try

        Assert.IsNotNull(ex, "Expected a helpful validation exception.")
        Assert.IsFalse(String.IsNullOrWhiteSpace(ex.Message), "Expected a helpful validation error message.")
        StringAssert.Contains(ex.Message, "right-hand-side only")
        StringAssert.Contains(ex.Message, "Use 'visit'")
    End Sub

    Private Shared Function FastFormulaTestControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 120
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function

    Private Shared Function BuildTwoVisitRawMatrix() As Object(,)
        Dim raw(5, 1) As Object
        Dim subject() As Double = {1, 1, 2, 2, 3, 3}
        Dim visit() As Double = {0, 1, 0, 1, 0, 1}

        For i As Integer = 0 To 5
            raw(i, 0) = subject(i)
            raw(i, 1) = visit(i)
        Next

        Return raw
    End Function

    Private Shared Sub BuildRandomInterceptRawMatrix(ByRef raw(,) As Object,
                                                     ByRef y() As Double)
        Dim nSubjects As Integer = 6
        Dim nVisits As Integer = 3
        Dim n As Integer = nSubjects * nVisits

        ReDim raw(n - 1, 1)
        ReDim y(n - 1)

        Dim subjectEffects() As Double = {-1.0, -0.6, -0.2, 0.2, 0.6, 1.0}
        Dim residuals() As Double = {-0.05, 0.04, 0.01,
                                      0.02, -0.03, 0.01,
                                      0.01, 0.02, -0.03,
                                      -0.02, 0.03, -0.01,
                                      0.04, -0.01, -0.03,
                                      -0.01, -0.02, 0.03}

        Dim r As Integer = 0
        For s As Integer = 0 To nSubjects - 1
            For v As Integer = 0 To nVisits - 1
                raw(r, 0) = CDbl(s + 1)
                raw(r, 1) = CDbl(v)
                y(r) = 10.0 + 1.5 * CDbl(v) + subjectEffects(s) + residuals(r)
                r += 1
            Next
        Next
    End Sub

    Private Shared Sub AssertBasicResult(res As MixedModelResult,
                                         expectedP As Integer,
                                         expectedN As Integer)
        Assert.IsNotNull(res, "Mixed-model fit returned Nothing.")
        Assert.AreEqual(expectedP, res.P, "Unexpected number of fixed-effect columns.")
        Assert.AreEqual(expectedN, res.Nobs, "Unexpected observation count.")
        Assert.IsNotNull(res.Beta, "Beta vector should not be Nothing.")
        Assert.AreEqual(expectedP, res.Beta.Length, "Unexpected beta length.")
        For i As Integer = 0 To res.Beta.Length - 1
            AssertFinite(res.Beta(i), "Beta(" & i.ToString() & ")")
        Next
        AssertFinite(res.Objective, "Objective")
        AssertFinite(res.LogLik, "LogLik")
    End Sub

    Private Shared Sub AssertStringArrayEqual(expected() As String,
                                              actual() As String,
                                              Optional context As String = "")
        Assert.IsNotNull(actual, "Actual string array should not be Nothing. " & context)
        Assert.AreEqual(expected.Length, actual.Length, "String-array length mismatch. " & context)
        For i As Integer = 0 To expected.Length - 1
            Assert.AreEqual(expected(i), actual(i), "String mismatch at index " & i.ToString() & ". " & context)
        Next
    End Sub

    Private Shared Sub AssertFinite(value As Double, label As String)
        Assert.IsFalse(Double.IsNaN(value), label & " should not be NaN.")
        Assert.IsFalse(Double.IsInfinity(value), label & " should not be infinite.")
    End Sub

End Class

' ===== END migrated from MixedModelFormulaServiceTests.vb =====

' ===== BEGIN migrated from MixedModelHypothesisBuilderTests.vb =====



<TestClass()>
Public Class MixedModelHypothesisBuilderTests

    <TestMethod()>
    Public Sub NormalizeTermKey_StripsLevelsAndCanonicalizesInteractions()
        Assert.AreEqual("visit", MixedModelHypothesisBuilder.NormalizeTermKey("visit=2"))
        Assert.AreEqual("visit", MixedModelHypothesisBuilder.NormalizeTermKey("visit[3]"))
        Assert.AreEqual("clinic_site_code", MixedModelHypothesisBuilder.NormalizeTermKey("clinic_site_code=2"))
        Assert.AreEqual("(Intercept)", MixedModelHypothesisBuilder.NormalizeTermKey("Intercept"))
        Assert.AreEqual("(Intercept)", MixedModelHypothesisBuilder.NormalizeTermKey("(Intercept)"))

        Assert.AreEqual("treatment_active:visit",
                        MixedModelHypothesisBuilder.NormalizeTermKey("visit=2:treatment_active"))

        Assert.AreEqual("treatment_active:visit",
                        MixedModelHypothesisBuilder.NormalizeTermKey("treatment_active:visit[3]"))
    End Sub


    <TestMethod()>
    Public Sub BuildTermHypotheses_GroupsCategoricalAndInteractionColumns()
        Dim names() As String = {
            "(Intercept)",
            "visit=2",
            "visit=3",
            "treatment_active",
            "treatment_active:visit=2",
            "treatment_active:visit=3",
            "age"
        }

        Dim hyps As List(Of MixedModelMultiDfHypothesis) =
            MixedModelHypothesisBuilder.BuildTermHypotheses(names, includeIntercept:=False)

        Assert.AreEqual(4, hyps.Count)
        Assert.AreEqual("visit", hyps(0).Label)
        Assert.AreEqual("treatment_active", hyps(1).Label)
        Assert.AreEqual("treatment_active:visit", hyps(2).Label)
        Assert.AreEqual("age", hyps(3).Label)

        Assert.AreEqual(2, hyps(0).L.GetLength(0), "visit should have two dummy rows.")
        Assert.AreEqual(names.Length, hyps(0).L.GetLength(1))
        Assert.AreEqual(1.0, hyps(0).L(0, 1), 0.0)
        Assert.AreEqual(1.0, hyps(0).L(1, 2), 0.0)

        Assert.AreEqual(2, hyps(2).L.GetLength(0), "interaction should have two dummy rows.")
        Assert.AreEqual(1.0, hyps(2).L(0, 4), 0.0)
        Assert.AreEqual(1.0, hyps(2).L(1, 5), 0.0)
    End Sub


    <TestMethod()>
    Public Sub TryBuildTermHypothesis_MatchesInteractionOrderInsensitively()
        Dim names() As String = {
            "(Intercept)",
            "visit=2",
            "visit=3",
            "treatment_active",
            "treatment_active:visit=2",
            "treatment_active:visit=3"
        }

        Dim h As MixedModelMultiDfHypothesis = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelHypothesisBuilder.TryBuildTermHypothesis(names,
                                                                         "visit:treatment_active",
                                                                         h,
                                                                         diagnostic:=msg), msg)

        Assert.IsNotNull(h)
        Assert.AreEqual("treatment_active:visit", h.Label)
        Assert.AreEqual(2, h.L.GetLength(0))
        Assert.AreEqual(1.0, h.L(0, 4), 0.0)
        Assert.AreEqual(1.0, h.L(1, 5), 0.0)
    End Sub


    <TestMethod()>
    Public Sub BuildCoefficientLinearHypothesis_ExactNameBuildsSelectorRow()
        Dim names() As String = {"(Intercept)", "visit=2", "visit=3"}

        Dim h As MixedModelLinearHypothesis = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelHypothesisBuilder.TryBuildCoefficientLinearHypothesis(names,
                                                                                      "visit=3",
                                                                                      h,
                                                                                      msg), msg)

        Assert.IsNotNull(h)
        Assert.AreEqual("visit=3", h.Label)
        Assert.AreEqual(3, h.L.Length)
        Assert.AreEqual(0.0, h.L(0), 0.0)
        Assert.AreEqual(0.0, h.L(1), 0.0)
        Assert.AreEqual(1.0, h.L(2), 0.0)
    End Sub


    <TestMethod()>
    Public Sub TermHypothesis_CanFeedKRMultiDfInference()
        Dim res As MixedModelResult = BuildNamedKrResult()

        Dim h As MixedModelMultiDfHypothesis = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelHypothesisBuilder.TryBuildTermHypothesis(res.FixedEffectNames,
                                                                         "visit",
                                                                         h,
                                                                         diagnostic:=msg), msg)

        Dim fTest As MixedModelKenwardRogerMultiDfInference = Nothing
        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(res,
                                                                          h.Label,
                                                                          h.L,
                                                                          fTest,
                                                                          diagnostic:=msg), msg)

        Assert.IsNotNull(fTest)
        Assert.AreEqual(2.0, fTest.NumDF, 0.0000000001)
        Assert.IsFalse(Double.IsNaN(fTest.DenDF))
        Assert.IsTrue(fTest.DenDF > 0.0)
        Assert.IsFalse(Double.IsNaN(fTest.FStatistic))
        Assert.IsTrue(fTest.PValue >= 0.0 AndAlso fTest.PValue <= 1.0)

        Dim t As ResultTable = MixedModelHypothesisBuilder.BuildTermMultiDfInferenceTable(res)
        Assert.IsNotNull(t)
        Assert.IsTrue(t.PvalColumns.Contains(4), "Term-level F-test p-value column should be marked.")
    End Sub


    Private Shared Function BuildNamedKrResult() As MixedModelResult
        Dim names() As String = {
            "(Intercept)",
            "visit=2",
            "visit=3",
            "treatment_active",
            "treatment_active:visit=2",
            "treatment_active:visit=3"
        }

        Dim p As Integer = names.Length
        Dim k As Integer = 2

        Dim phi(p - 1, p - 1) As Double
        Dim adjusted(p - 1, p - 1) As Double

        For j As Integer = 0 To p - 1
            phi(j, j) = 1.0 + CDbl(j)
            adjusted(j, j) = 1.5 + CDbl(j)
        Next

        Dim thetaCov(k - 1, k - 1) As Double
        thetaCov(0, 0) = 0.02
        thetaCov(1, 1) = 0.04
        thetaCov(0, 1) = 0.005
        thetaCov(1, 0) = 0.005

        Dim pMats(k - 1, p - 1, p - 1) As Double

        For j As Integer = 0 To p - 1
            pMats(0, j, j) = 0.1 + 0.01 * CDbl(j)
            pMats(1, j, j) = 0.05 + 0.005 * CDbl(j)
        Next

        Dim beta() As Double = {10.0, 1.0, 2.0, 3.0, 0.5, 0.75}

        Dim ws As New MixedModelKrWorkspace With {
            .P = p,
            .K = k,
            .VarBeta = phi,
            .ThetaCovariance = thetaCov,
            .Pmats = pMats,
            .AdjustedVarBeta = adjusted,
            .ParameterScale = MixedModelKrParameterScale.Covariance
        }

        Dim inf As New MixedModelInferenceWorkspace With {
            .P = p,
            .K = k,
            .VarBeta = phi,
            .AdjustedVarBeta = adjusted,
            .ThetaCovariance = thetaCov,
            .KR_P = pMats
        }

        Return New MixedModelResult With {
            .P = p,
            .Beta = beta,
            .VarBeta = phi,
            .BetaDF = New Double() {100, 100, 100, 100, 100, 100},
            .FixedEffectNames = names,
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger,
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)",
            .KenwardRogerWorkspace = ws,
            .KenwardRogerAdjustedVarBeta = adjusted,
            .InferenceWorkspace = inf
        }
    End Function

End Class

' ===== END migrated from MixedModelHypothesisBuilderTests.vb =====

' ===== BEGIN migrated from MixedModelInferenceWorkspaceCleanupTests.vb =====



<TestClass()>
Public Class MixedModelInferenceWorkspaceCleanupTests

    <TestMethod()>
    Public Sub LegacySatterthwaiteProperties_AreAliasesToInferenceWorkspace()
        Dim res As New MixedModelResult With {
            .P = 2,
            .Beta = New Double() {5.0, 2.0},
            .VarBeta = New Double(,) {{4.0, 0.0}, {0.0, 1.0}},
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.Satterthwaite,
            .BetaDF = New Double() {25.0, 25.0},
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)"
        }

        Dim thetaCov(,) As Double = {{1.0, 0.0}, {0.0, 1.0}}
        Dim grad(1, 1, 1) As Double
        grad(0, 0, 0) = 0.1
        grad(1, 0, 0) = 0.2

        ' Set through legacy properties.  They should populate InferenceWorkspace.
        res.SatterthwaiteThetaCovariance = thetaCov
        res.SatterthwaiteVarBetaGradient = grad

        Assert.IsNotNull(res.InferenceWorkspace, "InferenceWorkspace should be created by legacy setters.")
        Assert.AreSame(thetaCov, res.InferenceWorkspace.ThetaCovariance)
        Assert.AreSame(grad, res.InferenceWorkspace.VarBetaGradient)
        Assert.AreSame(thetaCov, res.SatterthwaiteThetaCovariance)
        Assert.AreSame(grad, res.SatterthwaiteVarBetaGradient)

        Dim df As Double = Double.NaN
        Assert.IsTrue(res.TrySatterthwaiteDFForLinearCombination(New Double() {1.0, 0.0}, df))

        ' v = 4; grad(v) = (0.1, 0.2); Var(v) = 0.05; df = 2*4^2/0.05 = 640
        Assert.AreEqual(640.0, df, 0.0000001)
    End Sub


    <TestMethod()>
    Public Sub SimpleMMRM_Satterthwaite_UsesUnifiedInferenceWorkspace()
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
        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.Satterthwaite
        req.UseSatterthwaite = True
        req.Control = TestControl()

        Dim res As MixedModelResult = (New MMRM(req)).Fit()

        Assert.IsNotNull(res.InferenceWorkspace, "Satterthwaite should populate InferenceWorkspace.")
        Assert.IsNotNull(res.InferenceWorkspace.ThetaCovariance, "Theta covariance should live in InferenceWorkspace.")
        Assert.IsNotNull(res.InferenceWorkspace.VarBetaGradient, "VarBeta gradient should live in InferenceWorkspace.")

        ' Compatibility getters should still work.
        Assert.AreSame(res.InferenceWorkspace.ThetaCovariance, res.SatterthwaiteThetaCovariance)
        Assert.AreSame(res.InferenceWorkspace.VarBetaGradient, res.SatterthwaiteVarBetaGradient)

        Dim df As Double = MixedModelPostEstimation.ResolveLinearEstimateDF(res, New Double() {1.0, 0.0})
        Assert.IsFalse(Double.IsNaN(df))
        Assert.IsFalse(Double.IsInfinity(df))
        Assert.IsTrue(df > 0.0)
    End Sub


    <TestMethod()>
    Public Sub SimpleMMRM_KRWorkspace_IsMirroredToInferenceWorkspace()
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

        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be populated.")
        Assert.IsNotNull(res.InferenceWorkspace, "KR build should also populate InferenceWorkspace.")
        Assert.IsNotNull(res.InferenceWorkspace.KR_P, "KR P matrices should be mirrored to InferenceWorkspace.")
        Assert.IsNotNull(res.InferenceWorkspace.KR_Q, "KR Q matrices should be mirrored to InferenceWorkspace.")
        Assert.IsNotNull(res.KenwardRogerAdjustedVarBeta, "Adjusted Var(beta) should be populated.")
        Assert.AreSame(res.InferenceWorkspace.AdjustedVarBeta, res.KenwardRogerAdjustedVarBeta)
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

' ===== END migrated from MixedModelInferenceWorkspaceCleanupTests.vb =====

