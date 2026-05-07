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

' ===== BEGIN migrated from MixedModelKenwardRogerInferenceTests.vb =====



<TestClass()>
Public Class MixedModelKenwardRogerInferenceTests

    <TestMethod()>
    Public Sub ApproximateDenominatorDF_ScalarWorkspace_UsesKrMomentMatching()
        Dim res As MixedModelResult = BuildScalarKrResult()

        Dim l() As Double = {1.0}
        Dim adjustedVariance As Double = 4.5
        Dim df As Double = Double.NaN
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeUnivariateDenominatorDF(res,
                                                                                  l,
                                                                                  adjustedVariance,
                                                                                  df,
                                                                                  msg), msg)

        ' For q=1 the KR moment-matching formula gives A1=A2=0.04,
        ' E*=1/(1-A2)=1.041666..., rho=1.065217..., and df=50.
        Assert.AreEqual(50.0, df, 0.0000000001)
    End Sub



    <TestMethod()>
    Public Sub KrDegreesOfFreedomAndScaling_ScalarWorkspace_ReturnsMmrmComponents()
        Dim res As MixedModelResult = BuildScalarKrResult()

        Dim l(0, 0) As Double
        l(0, 0) = 1.0

        Dim info As MixedModelKenwardRogerDfResult = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrDegreesOfFreedomAndScaling(res,
                                                                                              l,
                                                                                              info,
                                                                                              msg), msg)

        Assert.IsNotNull(info)
        Assert.AreEqual(1, info.NumDF)
        Assert.AreEqual(0.04, info.A1, 0.0000000001)
        Assert.AreEqual(0.04, info.A2, 0.0000000001)
        Assert.AreEqual(50.0, info.DenDF, 0.0000000001)
        Assert.AreEqual(1.0, info.Lambda, 0.0000000001)
    End Sub


    <TestMethod()>
    Public Sub UnivariateInference_ScalarWorkspace_ComputesFiniteTAndP()
        Dim res As MixedModelResult = BuildScalarKrResult()

        Dim out As MixedModelKenwardRogerUnivariateInference = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryUnivariateInference(res,
                                                                             "intercept",
                                                                             New Double() {1.0},
                                                                             out,
                                                                             alpha:=0.05,
                                                                             diagnostic:=msg), msg)

        Assert.IsNotNull(out)
        Assert.AreEqual(10.0, out.Estimate, 0.0000000001)
        Assert.AreEqual(2.0, out.OrdinaryStdError, 0.0000000001)
        Assert.AreEqual(System.Math.Sqrt(4.5), out.AdjustedStdError, 0.0000000001)
        Assert.AreEqual(4.0, out.OrdinaryVariance, 0.0000000001)
        Assert.AreEqual(4.5, out.AdjustedVariance, 0.0000000001)
        Assert.AreEqual(50.0, out.DF, 0.0000000001)
        Assert.AreEqual(1.0, out.Lambda, 0.0000000001)
        Assert.IsFalse(Double.IsNaN(out.PValue))
        Assert.IsFalse(Double.IsInfinity(out.PValue))
        Assert.IsTrue(out.PValue >= 0.0 AndAlso out.PValue <= 1.0)
    End Sub


    <TestMethod()>
    Public Sub UnivariateInferenceTable_ScalarWorkspace_BuildsResultTable()
        Dim res As MixedModelResult = BuildScalarKrResult()

        Dim hyps As New List(Of MixedModelLinearHypothesis) From {
            New MixedModelLinearHypothesis("intercept", New Double() {1.0})
        }

        Dim t As ResultTable = MixedModelKenwardRogerInference.BuildUnivariateInferenceTable(res, hyps)

        Assert.IsNotNull(t)
        Assert.IsTrue(t.PvalColumns.Contains(6), "P-value column should be marked for formatting.")
    End Sub


    Private Shared Function BuildScalarKrResult() As MixedModelResult
        Dim pMats(0, 0, 0) As Double
        pMats(0, 0, 0) = 0.5

        Dim thetaCov(0, 0) As Double
        thetaCov(0, 0) = 0.01

        Dim phi(0, 0) As Double
        phi(0, 0) = 4.0

        Dim adjusted(0, 0) As Double
        adjusted(0, 0) = 4.5

        Dim ws As New MixedModelKrWorkspace With {
            .P = 1,
            .K = 1,
            .VarBeta = phi,
            .ThetaCovariance = thetaCov,
            .Pmats = pMats,
            .AdjustedVarBeta = adjusted,
            .ParameterScale = MixedModelKrParameterScale.Covariance
        }

        Dim inf As New MixedModelInferenceWorkspace With {
            .P = 1,
            .K = 1,
            .VarBeta = phi,
            .AdjustedVarBeta = adjusted,
            .ThetaCovariance = thetaCov,
            .KR_P = pMats
        }

        Return New MixedModelResult With {
            .P = 1,
            .Beta = New Double() {10.0},
            .VarBeta = phi,
            .FixedEffectNames = New String() {"intercept"},
            .KenwardRogerWorkspace = ws,
            .KenwardRogerAdjustedVarBeta = adjusted,
            .InferenceWorkspace = inf
        }
    End Function

End Class

' ===== END migrated from MixedModelKenwardRogerInferenceTests.vb =====

' ===== BEGIN migrated from MixedModelKenwardRogerMultiDfInferenceTests.vb =====



<TestClass()>
Public Class MixedModelKenwardRogerMultiDfInferenceTests

    <TestMethod()>
    Public Sub MultiDfInference_OneRow_EqualsUnivariateTTestSquared()
        Dim res As MixedModelResult = BuildScalarKrResult()

        Dim uni As MixedModelKenwardRogerUnivariateInference = Nothing
        Dim uniMsg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryUnivariateInference(res,
                                                                             "intercept",
                                                                             New Double() {1.0},
                                                                             uni,
                                                                             alpha:=0.05,
                                                                             diagnostic:=uniMsg), uniMsg)

        Dim l(0, 0) As Double
        l(0, 0) = 1.0

        Dim multi As MixedModelKenwardRogerMultiDfInference = Nothing
        Dim multiMsg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(res,
                                                                          "intercept",
                                                                          l,
                                                                          multi,
                                                                          alpha:=0.05,
                                                                          diagnostic:=multiMsg), multiMsg)

        Assert.IsNotNull(multi)
        Assert.AreEqual(1.0, multi.NumDF, 0.0000000001)
        Assert.AreEqual(uni.DF, multi.DenDF, 0.0000000001)
        Assert.AreEqual(uni.Statistic * uni.Statistic, multi.UnscaledFStatistic, 0.0000000001)
        Assert.AreEqual(multi.Scaling * multi.UnscaledFStatistic, multi.FStatistic, 0.0000000001)
        Assert.AreEqual(uni.PValue, multi.PValue, 0.000000001)
        Assert.AreEqual(1.0, multi.Scaling, 0.0000000001)
    End Sub


    <TestMethod()>
    Public Sub MultiDfInference_TwoRows_ComputesFiniteFAndP()
        Dim res As MixedModelResult = BuildTwoCoefficientKrResult()

        Dim l(1, 1) As Double
        l(0, 0) = 1.0
        l(1, 1) = 1.0

        Dim multi As MixedModelKenwardRogerMultiDfInference = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(res,
                                                                          "joint beta",
                                                                          l,
                                                                          multi,
                                                                          alpha:=0.05,
                                                                          diagnostic:=msg), msg)

        Assert.IsNotNull(multi)
        Assert.AreEqual(2.0, multi.NumDF, 0.0000000001)

        ' beta = [2, 4], KR cov = [[4, 1], [1, 9]]
        ' inverse = (1/35) * [[9, -1], [-1, 4]]
        ' Wald quadratic = 2.4; unscaled F = 2.4 / 2 = 1.2.
        Assert.AreEqual(1.2, multi.UnscaledFStatistic, 0.0000000001)
        Assert.AreEqual(multi.Scaling * multi.UnscaledFStatistic, multi.FStatistic, 0.0000000001)
        Assert.AreEqual(0.987810857366038, multi.Scaling, 0.000000000001)
        Assert.AreEqual(44.0634311641056, multi.DenDF, 0.0000000001)
        Assert.IsTrue(multi.A1 > 0.0)
        Assert.IsTrue(multi.A2 > 0.0)
        Assert.IsTrue(multi.EStar > 0.0)
        Assert.IsTrue(multi.VStar > 0.0)
        Assert.IsTrue(multi.Rho > 0.0)

        Assert.IsFalse(Double.IsNaN(multi.DenDF))
        Assert.IsFalse(Double.IsInfinity(multi.DenDF))
        Assert.IsTrue(multi.DenDF > 0.0)

        Assert.IsFalse(Double.IsNaN(multi.PValue))
        Assert.IsFalse(Double.IsInfinity(multi.PValue))
        Assert.IsTrue(multi.PValue >= 0.0 AndAlso multi.PValue <= 1.0)
    End Sub


    <TestMethod()>
    Public Sub MultiDfInference_RankDeficientRows_UsesEffectiveNumeratorDf()
        Dim res As MixedModelResult = BuildTwoCoefficientKrResult()

        Dim l(1, 1) As Double
        l(0, 0) = 1.0
        l(1, 0) = 1.0

        Dim multi As MixedModelKenwardRogerMultiDfInference = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(res,
                                                                          "duplicated beta0",
                                                                          l,
                                                                          multi,
                                                                          alpha:=0.05,
                                                                          diagnostic:=msg), msg)

        Assert.IsNotNull(multi)
        Assert.AreEqual(2.0, multi.RequestedNumDF, 0.0000000001)
        Assert.AreEqual(1.0, multi.NumDF, 0.0000000001)
        Assert.AreEqual(1, multi.Rank)
        Assert.IsTrue(multi.RankReduced)
        Assert.IsNotNull(multi.RequestedL)
        Assert.IsNotNull(multi.EffectiveL)
        Assert.AreEqual(1, multi.EffectiveL.GetLength(0))
        Assert.AreEqual(2, multi.EffectiveL.GetLength(1))
        Assert.IsFalse(Double.IsNaN(multi.FStatistic))
        Assert.IsFalse(Double.IsInfinity(multi.FStatistic))
        Assert.IsTrue(multi.DiagnosticMessage.IndexOf("rank-reduced", StringComparison.OrdinalIgnoreCase) >= 0)
    End Sub


    <TestMethod()>
    Public Sub KrDegreesOfFreedomAndScaling_RankDeficientRows_UsesEffectiveRank()
        Dim res As MixedModelResult = BuildTwoCoefficientKrResult()

        Dim l(1, 1) As Double
        l(0, 0) = 1.0
        l(1, 0) = 1.0

        Dim info As MixedModelKenwardRogerDfResult = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrDegreesOfFreedomAndScaling(res,
                                                                                              l,
                                                                                              info,
                                                                                              msg), msg)

        Assert.IsNotNull(info)
        Assert.AreEqual(1, info.NumDF)
        Assert.IsTrue(info.DenDF > 0.0)
        Assert.IsTrue(info.Lambda > 0.0)
    End Sub


    <TestMethod()>
    Public Sub MultiDfInferenceTable_TwoRows_BuildsResultTable()
        Dim res As MixedModelResult = BuildTwoCoefficientKrResult()

        Dim l(1, 1) As Double
        l(0, 0) = 1.0
        l(1, 1) = 1.0

        Dim hyps As New List(Of MixedModelMultiDfHypothesis) From {
            New MixedModelMultiDfHypothesis("joint beta", l)
        }

        Dim t As ResultTable = MixedModelKenwardRogerInference.BuildMultiDfInferenceTable(res, hyps)

        Assert.IsNotNull(t)
        Assert.IsTrue(t.PvalColumns.Contains(4), "F-test p-value column should be marked for formatting.")

        Dim arr(,) As Object = t.returnSelf()
        Assert.IsTrue(ArrayContainsText(arr, "Unscaled F"), "Multi-df KR table should include unscaled F.")
        Assert.IsTrue(ArrayContainsText(arr, "F scaling"), "Multi-df KR table should include the F scaling factor.")
        Assert.IsTrue(ArrayContainsText(arr, "Requested Num DF"), "Multi-df KR table should include requested numerator DF.")
        Assert.IsTrue(ArrayContainsText(arr, "Rank reduced"), "Multi-df KR table should include rank-reduction diagnostics.")
    End Sub


    Private Shared Function ArrayContainsText(arr(,) As Object,
                                              text As String) As Boolean
        If arr Is Nothing Then Return False

        For i As Integer = 0 To arr.GetLength(0) - 1
            For j As Integer = 0 To arr.GetLength(1) - 1
                If arr(i, j) Is Nothing Then Continue For

                If String.Equals(CStr(arr(i, j)),
                                 text,
                                 StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Next
        Next

        Return False
    End Function


    Private Shared Function BuildScalarKrResult() As MixedModelResult
        Dim pMats(0, 0, 0) As Double
        pMats(0, 0, 0) = 0.5

        Dim thetaCov(0, 0) As Double
        thetaCov(0, 0) = 0.01

        Dim phi(0, 0) As Double
        phi(0, 0) = 4.0

        Dim adjusted(0, 0) As Double
        adjusted(0, 0) = 4.5

        Dim ws As New MixedModelKrWorkspace With {
            .P = 1,
            .K = 1,
            .VarBeta = phi,
            .ThetaCovariance = thetaCov,
            .Pmats = pMats,
            .AdjustedVarBeta = adjusted,
            .ParameterScale = MixedModelKrParameterScale.Covariance
        }

        Dim inf As New MixedModelInferenceWorkspace With {
            .P = 1,
            .K = 1,
            .VarBeta = phi,
            .AdjustedVarBeta = adjusted,
            .ThetaCovariance = thetaCov,
            .KR_P = pMats
        }

        Return New MixedModelResult With {
            .P = 1,
            .Beta = New Double() {10.0},
            .VarBeta = phi,
            .FixedEffectNames = New String() {"intercept"},
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger,
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)",
            .KenwardRogerWorkspace = ws,
            .KenwardRogerAdjustedVarBeta = adjusted,
            .InferenceWorkspace = inf
        }
    End Function


    Private Shared Function BuildTwoCoefficientKrResult() As MixedModelResult
        Dim phi(1, 1) As Double
        phi(0, 0) = 1.0
        phi(1, 1) = 4.0

        Dim adjusted(1, 1) As Double
        adjusted(0, 0) = 4.0
        adjusted(0, 1) = 1.0
        adjusted(1, 0) = 1.0
        adjusted(1, 1) = 9.0

        Dim thetaCov(1, 1) As Double
        thetaCov(0, 0) = 0.02
        thetaCov(1, 1) = 0.04
        thetaCov(0, 1) = 0.005
        thetaCov(1, 0) = 0.005

        Dim pMats(1, 1, 1) As Double
        pMats(0, 0, 0) = 0.3
        pMats(0, 1, 1) = 0.1
        pMats(1, 0, 0) = 0.05
        pMats(1, 1, 1) = 0.4

        Dim ws As New MixedModelKrWorkspace With {
            .P = 2,
            .K = 2,
            .VarBeta = phi,
            .ThetaCovariance = thetaCov,
            .Pmats = pMats,
            .AdjustedVarBeta = adjusted,
            .ParameterScale = MixedModelKrParameterScale.Covariance
        }

        Dim inf As New MixedModelInferenceWorkspace With {
            .P = 2,
            .K = 2,
            .VarBeta = phi,
            .AdjustedVarBeta = adjusted,
            .ThetaCovariance = thetaCov,
            .KR_P = pMats
        }

        Return New MixedModelResult With {
            .P = 2,
            .Beta = New Double() {2.0, 4.0},
            .VarBeta = phi,
            .BetaDF = New Double() {100.0, 100.0},
            .FixedEffectNames = New String() {"Intercept", "x"},
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger,
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)",
            .KenwardRogerWorkspace = ws,
            .KenwardRogerAdjustedVarBeta = adjusted,
            .InferenceWorkspace = inf
        }
    End Function

End Class

' ===== END migrated from MixedModelKenwardRogerMultiDfInferenceTests.vb =====

' ===== BEGIN migrated from MixedModelKenwardRogerFixedEffectInferenceIntegrationTests.vb =====



<TestClass()>
Public Class MixedModelKenwardRogerFixedEffectInferenceIntegrationTests

    <TestMethod()>
    Public Sub SleepstudyUnbalanced_KRFixedInference_UsesAdjustedSEAndTLabels()
        Dim dat As SleepstudyData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
        Dim res As MixedModelResult = FitSleepstudyRandomSlopeWithKRFixedInference(dat)

        Assert.IsNotNull(res, "LMM result should not be Nothing.")
        Assert.IsTrue(res.Converged, "Unbalanced sleepstudy random-slope LMM should converge.")
        Assert.AreEqual(MixedModelFixedInferenceMethod.KenwardRoger,
                        res.FixedInferenceMethod,
                        "Result should report KenwardRoger fixed-effect inference.")
        Assert.AreEqual("t", res.BetaStatisticLabel)
        Assert.AreEqual("Pr(>|t|)", res.BetaPValueLabel)

        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        Assert.AreEqual(MixedModelKrParameterScale.Covariance,
                        res.KenwardRogerWorkspace.ParameterScale,
                        "Random-slope LMM should use covariance-parameter KR scale.")

        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)
        Assert.IsNotNull(adjusted, "KR adjusted Var(beta) should be available.")

        For j As Integer = 0 To res.P - 1
            Dim expectedSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))

            Assert.AreEqual(expectedSE,
                            res.BetaSE(j),
                            0.000000001,
                            "Fixed-effect SE should use KR adjusted Var(beta) at beta index " &
                            j.ToString(CultureInfo.InvariantCulture))

            Assert.IsFalse(Double.IsNaN(res.BetaDF(j)), "KR coefficient DF should be finite.")
            Assert.IsFalse(Double.IsInfinity(res.BetaDF(j)), "KR coefficient DF should be finite.")
            Assert.IsTrue(res.BetaDF(j) > 0.0, "KR coefficient DF should be positive.")

            Assert.IsFalse(Double.IsNaN(res.BetaStatistic(j)), "KR t statistic should be finite.")
            Assert.IsFalse(Double.IsInfinity(res.BetaStatistic(j)), "KR t statistic should be finite.")
            Assert.IsFalse(Double.IsNaN(res.BetaP(j)), "KR p-value should be finite.")
            Assert.IsFalse(Double.IsInfinity(res.BetaP(j)), "KR p-value should be finite.")
            Assert.IsTrue(res.BetaP(j) >= 0.0 AndAlso res.BetaP(j) <= 1.0, "KR p-value should be in [0,1].")
        Next
    End Sub


    Private Shared Function FitSleepstudyRandomSlopeWithKRFixedInference(dat As SleepstudyData) As MixedModelResult
        Dim x(,) As Double = BuildFixedDesign(dat)
        Dim z(,) As Double = BuildRandomSlopeDesign(dat)

        Dim blockData As MixedModelBlockData =
            MixedModelBlockData.FromArrays(y:=dat.Reaction,
                                           x:=x,
                                           subjectId:=dat.Subject,
                                           z:=z,
                                           visit:=dat.Days,
                                           sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest =
            MixedModelFitRequest.CreateLMM(blockData,
                                           New IdentityR(),
                                           New RandomInterceptSlope(),
                                           MixedModelFitMethod.REML)

        req.RequestLabel = "sleepstudy unbalanced KR fixed-effect inference integration"
        req.ResponseVarName = "reaction"
        req.SubjectVarName = "subject"
        req.VisitVarName = "days"
        req.FixedEffectNames = {"(Intercept)", "days"}
        req.RandomEffectNames = {"(Intercept)", "days"}

        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger
        req.BuildKenwardRogerWorkspace = True
        req.BuildKenwardRogerSecondDerivatives = True
        req.Control = TestControl()

        req.StartThetaG = {Math.Log(24.7405), Math.Log(5.9221), Atanh(0.066)}
        req.StartThetaR = {Math.Log(654.941)}

        Return (New LMM(req)).Fit()
    End Function


    Private Shared Function TestControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 280
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function


    Private Shared Function BuildFixedDesign(dat As SleepstudyData) As Double(,)
        Dim n As Integer = dat.Reaction.Length
        Dim x(n - 1, 1) As Double

        For i As Integer = 0 To n - 1
            x(i, 0) = 1.0
            x(i, 1) = dat.Days(i)
        Next

        Return x
    End Function


    Private Shared Function BuildRandomSlopeDesign(dat As SleepstudyData) As Double(,)
        Dim n As Integer = dat.Reaction.Length
        Dim z(n - 1, 1) As Double

        For i As Integer = 0 To n - 1
            z(i, 0) = 1.0
            z(i, 1) = dat.Days(i)
        Next

        Return z
    End Function


    Private Shared Function LoadSleepstudyCsv(fileName As String) As SleepstudyData
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then
            Throw New InvalidOperationException("sleepstudy CSV must contain a header and data rows.")
        End If

        Dim header() As String = lines(0).Split(","c)
        Dim col As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For j As Integer = 0 To header.Length - 1
            col(header(j).Trim()) = j
        Next

        RequireColumn(col, "reaction", fileName)
        RequireColumn(col, "days", fileName)
        RequireColumn(col, "subject", fileName)

        Dim n As Integer = lines.Length - 1
        Dim reaction(n - 1) As Double
        Dim days(n - 1) As Double
        Dim subject(n - 1) As Object

        For i As Integer = 0 To n - 1
            Dim parts() As String = lines(i + 1).Split(","c)

            reaction(i) = ParseD(parts(col("reaction")))
            days(i) = ParseD(parts(col("days")))
            subject(i) = parts(col("subject")).Trim()
        Next

        Return New SleepstudyData With {
            .Reaction = reaction,
            .Days = days,
            .Subject = subject
        }
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

        Throw New FileNotFoundException("Could not locate test data file.", fileName)
    End Function


    Private Shared Sub RequireColumn(col As Dictionary(Of String, Integer),
                                     columnName As String,
                                     fileName As String)
        If Not col.ContainsKey(columnName) Then
            Throw New InvalidOperationException("CSV file '" & fileName & "' must contain column '" & columnName & "'.")
        End If
    End Sub


    Private Shared Function ParseD(text As String) As Double
        Return Double.Parse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function


    Private Shared Function Atanh(x As Double) As Double
        Return 0.5 * Math.Log((1.0 + x) / (1.0 - x))
    End Function


    Private Class SleepstudyData
        Public Reaction() As Double
        Public Days() As Double
        Public Subject() As Object
    End Class

End Class

' ===== END migrated from MixedModelKenwardRogerFixedEffectInferenceIntegrationTests.vb =====
