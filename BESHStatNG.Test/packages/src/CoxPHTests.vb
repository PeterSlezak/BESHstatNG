Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.IO
Imports System.Globalization
Imports System.Linq
Imports System.Collections.Generic
Imports System.Reflection
Imports BESHStatNG

<TestClass()>
Public Class CoxPH_Tests

    Private Const TOL_COEF As Double = 0.0000005
    Private Const TOL_SE As Double = 0.0000008
    Private Const TOL_LL As Double = 0.0000008
    Private Const TOL_STAT As Double = 0.000001

    ' ---------------------------
    ' Paths / IO
    ' ---------------------------
    Private Shared Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory

        Dim c1 As String = Path.Combine(baseDir, fileName)
        If File.Exists(c1) Then Return c1

        Dim c2 As String = Path.Combine(baseDir, "TestData", fileName)
        If File.Exists(c2) Then Return c2

        Dim c3 As String = Path.GetFullPath(Path.Combine(baseDir, "..\..\TestData", fileName))
        If File.Exists(c3) Then Return c3

        Dim c4 As String = Path.GetFullPath(Path.Combine(baseDir, "..\..\..\TestData", fileName))
        If File.Exists(c4) Then Return c4

        Throw New FileNotFoundException("Test data file not found", fileName)
    End Function

    Private Shared Function GetAsDouble(s As String) As Double
        Return Double.Parse(s, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function GetAsInt(s As String) As Integer
        Return Integer.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture)
    End Function

    ' Loads CoxPH CSV with schema:
    ' id,time,status,stratum,x1,x2
    ' status: 1 = event, 0 = censored (matches survival.SurvivalRecord.Censorship)
    Private Shared Function LoadCoxRecords(fileName As String) As List(Of survival.SurvivalRecord)
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must have header + rows.")

        Dim header() As String = lines(0).Split(","c).Select(Function(z) z.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next

        Dim required() As String = {"id", "time", "status", "stratum", "x1", "x2"}
        For Each r In required
            If Not idx.ContainsKey(r) Then Throw New InvalidOperationException("Missing column: " & r)
        Next

        Dim out As New List(Of survival.SurvivalRecord)

        For r As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(r).Split(","c)
            Dim id As Integer = GetAsInt(parts(idx("id")).Trim())
            Dim tm As Double = GetAsDouble(parts(idx("time")).Trim())
            Dim st As Integer = GetAsInt(parts(idx("status")).Trim())
            Dim stratum As String = parts(idx("stratum")).Trim()
            Dim x1 As Double = GetAsDouble(parts(idx("x1")).Trim())
            Dim x2 As Double = GetAsDouble(parts(idx("x2")).Trim())

            Dim rec As New survival.SurvivalRecord With {
                .Time = tm,
                .Censorship = st,
                .Group = 0,
                .strGroup = "0",
                .stratum = stratum,
                .strStratum = stratum,
                .Covariates = New Double() {x1, x2},
                .Index = id
            }
            out.Add(rec)
        Next

        Return out
    End Function

    Private Shared Sub AssertVectorAlmostEqual(expected() As Double, actual() As Double, tol As Double, Optional msg As String = "")
        Assert.IsNotNull(actual, "actual vector is Nothing. " & msg)
        Assert.AreEqual(expected.Length, actual.Length, "Vector length mismatch. " & msg)
        For i As Integer = 0 To expected.Length - 1
            Assert.AreEqual(expected(i), actual(i), tol, $"Mismatch at index {i}. {msg}")
        Next
    End Sub

    Private Shared Function WaldChiSquare(beta() As Double, varcov(,) As Double) As Double
        ' Invert 2x2 varcov explicitly (this test dataset uses 2 covariates)
        Dim a As Double = varcov(0, 0)
        Dim b As Double = varcov(0, 1)
        Dim c As Double = varcov(1, 0)
        Dim d As Double = varcov(1, 1)
        Dim det As Double = a * d - b * c
        Assert.IsTrue(Math.Abs(det) > 0.0, "VarCov not invertible.")

        Dim inv00 As Double = d / det
        Dim inv01 As Double = -b / det
        Dim inv10 As Double = -c / det
        Dim inv11 As Double = a / det

        Return beta(0) * (inv00 * beta(0) + inv01 * beta(1)) + beta(1) * (inv10 * beta(0) + inv11 * beta(1))
    End Function

    Function GetScoreChiSqFromPrivateField(cox As CoxPH) As Double
        ' Prefer cached private field if it exists and is populated; otherwise compute via ComputeScoreTest().
        Dim fi = GetType(CoxPH).GetField("pScoreStat", BindingFlags.Instance Or BindingFlags.NonPublic)
        If fi IsNot Nothing Then
            Dim trObj = fi.GetValue(cox)
            If trObj IsNot Nothing Then
                Return GetTestResultMemberAsDouble(trObj, "TestStatistics1")
            End If
        End If

        ' Fallback: call ComputeScoreTest (Private in current implementation)
        Dim mi = GetType(CoxPH).GetMethod("ComputeScoreTest", BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
        Assert.IsNotNull(mi, "Could not access ComputeScoreTest().")
        Dim tr2 = mi.Invoke(cox, New Object() {})
        Assert.IsNotNull(tr2, "ComputeScoreTest() returned Nothing.")
        Return GetTestResultMemberAsDouble(tr2, "TestStatistics1")
    End Function

    Private Function GetTestResultMemberAsDouble(trObj As Object, memberName As String) As Double
        ' TestResult in this codebase uses PUBLIC FIELDS, but support properties too (future-proof).
        Dim t = trObj.GetType()

        Dim pi = t.GetProperty(memberName, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
        If pi IsNot Nothing Then
            Return CDbl(pi.GetValue(trObj))
        End If

        Dim fi = t.GetField(memberName, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
        Assert.IsNotNull(fi, $"Could not access {memberName} as field or property on {t.FullName}.")
        Return CDbl(fi.GetValue(trObj))
    End Function


    ' ---------------------------
    ' Reference values (computed by coxph_reference.R)
    ' ---------------------------
    Private Shared ReadOnly Expected_Breslow_Coef() As Double = {0.125292966448594, -0.631760060901422}
    Private Shared ReadOnly Expected_Breslow_SE() As Double = {0.300191786071262, 0.458462827724965}
    Private Shared ReadOnly Expected_Breslow_LogLik0 As Double = -26.450952820550143
    Private Shared ReadOnly Expected_Breslow_LogLik As Double = -25.389173632191895
    Private Shared ReadOnly Expected_Breslow_LR As Double = 2.1235583767164949
    Private Shared ReadOnly Expected_Breslow_Score As Double = 2.004352549353511
    Private Shared ReadOnly Expected_Breslow_Wald As Double = 1.9251742950994859

    Private Shared ReadOnly Expected_Efron_Coef() As Double = {0.123744688344717, -0.673495322938158}
    Private Shared ReadOnly Expected_Efron_SE() As Double = {0.297667600590607, 0.463602734412479}
    Private Shared ReadOnly Expected_Efron_LogLik0 As Double = -25.98664721241904
    Private Shared ReadOnly Expected_Efron_LogLik As Double = -24.803912597945786
    Private Shared ReadOnly Expected_Efron_LR As Double = 2.3654692289465089
    Private Shared ReadOnly Expected_Efron_Score As Double = 2.226079848873741
    Private Shared ReadOnly Expected_Efron_Wald As Double = 2.1301892621796532

    Private Shared ReadOnly Expected_Exact_Coef() As Double = {0.139870855937664, -0.685330129652873}
    Private Shared ReadOnly Expected_Exact_SE() As Double = {0.31014309096766, 0.477448787165617}
    Private Shared ReadOnly Expected_Exact_LogLik0 As Double = -23.214058490179262
    Private Shared ReadOnly Expected_Exact_LogLik As Double = -22.060046016513578
    Private Shared ReadOnly Expected_Exact_LR As Double = 2.308024947331369
    Private Shared ReadOnly Expected_Exact_Score As Double = 2.18759387571787
    Private Shared ReadOnly Expected_Exact_Wald As Double = 2.0899950166573649

    ' ---------------------------
    ' Tests
    ' ---------------------------

    <TestCategory("CoxPH")>
    <TestMethod()>
    Public Sub CoxPH_Breslow_matches_reference_coeff_SE_LL_and_tests()
        Dim recs = LoadCoxRecords("coxph_dataset_strata_ties.csv")
        Dim cox As New CoxPH(recs, New String() {"x1", "x2"}, 200, 0.000000000001)
        cox.bReturnCov = True
        cox.bRobustVariance = False
        Dim coxres As CoxResult = cox.Fit(TieMethod.Breslow)

        Assert.IsTrue(coxres.Converged, "Model did not converge (Breslow).")

        AssertVectorAlmostEqual(Expected_Breslow_Coef, coxres.Coefficients, TOL_COEF, "Breslow coefficients")
        Dim se() As Double = {Math.Sqrt(coxres.VarCov(0, 0)), Math.Sqrt(coxres.VarCov(1, 1))}
        AssertVectorAlmostEqual(Expected_Breslow_SE, se, TOL_SE, "Breslow SEs")

        Assert.AreEqual(Expected_Breslow_LogLik0, coxres.LogLikelihoodNull, TOL_LL, "Breslow LogLik0")
        Assert.AreEqual(Expected_Breslow_LogLik, coxres.LogLikelihood, TOL_LL, "Breslow LogLik")

        Dim lr As Double = 2.0 * (coxres.LogLikelihood - coxres.LogLikelihoodNull)
        Assert.AreEqual(Expected_Breslow_LR, lr, TOL_STAT, "Breslow LR ChiSq")

        Dim scoreChi2 As Double = GetScoreChiSqFromPrivateField(cox)
        Assert.AreEqual(Expected_Breslow_Score, scoreChi2, TOL_STAT, "Breslow Score ChiSq")

        Dim waldChi2 As Double = WaldChiSquare(coxres.Coefficients, coxres.VarCov)
        Assert.AreEqual(Expected_Breslow_Wald, waldChi2, TOL_STAT, "Breslow Wald ChiSq")
    End Sub

    <TestCategory("CoxPH")>
    <TestMethod()>
    Public Sub CoxPH_Efron_matches_reference_coeff_SE_LL_and_tests()
        Dim recs = LoadCoxRecords("coxph_dataset_strata_ties.csv")
        Dim cox As New CoxPH(recs, New String() {"x1", "x2"}, 200, 0.000000000001)
        cox.bReturnCov = True
        cox.bRobustVariance = False
        Dim coxres = cox.Fit(TieMethod.Efron)

        Assert.IsTrue(coxres.Converged, "Model did not converge (Efron).")

        AssertVectorAlmostEqual(Expected_Efron_Coef, coxres.Coefficients, TOL_COEF, "Efron coefficients")
        Dim se() As Double = {Math.Sqrt(coxres.VarCov(0, 0)), Math.Sqrt(coxres.VarCov(1, 1))}
        AssertVectorAlmostEqual(Expected_Efron_SE, se, TOL_SE, "Efron SEs")

        Assert.AreEqual(Expected_Efron_LogLik0, coxres.LogLikelihoodNull, TOL_LL, "Efron LogLik0")
        Assert.AreEqual(Expected_Efron_LogLik, coxres.LogLikelihood, TOL_LL, "Efron LogLik")

        Dim lr As Double = 2.0 * (coxres.LogLikelihood - coxres.LogLikelihoodNull)
        Assert.AreEqual(Expected_Efron_LR, lr, TOL_STAT, "Efron LR ChiSq")

        Dim scoreChi2 As Double = GetScoreChiSqFromPrivateField(cox)
        Assert.AreEqual(Expected_Efron_Score, scoreChi2, TOL_STAT, "Efron Score ChiSq")

        Dim waldChi2 As Double = WaldChiSquare(coxres.Coefficients, coxres.VarCov)
        Assert.AreEqual(Expected_Efron_Wald, waldChi2, TOL_STAT, "Efron Wald ChiSq")
    End Sub

    <TestCategory("CoxPH")>
    <TestMethod()>
    Public Sub CoxPH_Exact_matches_reference_coeff_SE_LL_and_tests()
        Dim recs = LoadCoxRecords("coxph_dataset_strata_ties.csv")
        Dim cox As New CoxPH(recs, New String() {"x1", "x2"}, 200, 0.000000000001)
        cox.bReturnCov = True
        cox.bRobustVariance = False
        Dim coxres = cox.Fit(TieMethod.Exact)

        Assert.IsTrue(coxres.Converged, "Model did not converge (Exact).")

        AssertVectorAlmostEqual(Expected_Exact_Coef, coxres.Coefficients, TOL_COEF, "Exact coefficients")
        Dim se() As Double = {Math.Sqrt(coxres.VarCov(0, 0)), Math.Sqrt(coxres.VarCov(1, 1))}
        AssertVectorAlmostEqual(Expected_Exact_SE, se, TOL_SE, "Exact SEs")

        Assert.AreEqual(Expected_Exact_LogLik0, coxres.LogLikelihoodNull, TOL_LL, "Exact LogLik0")
        Assert.AreEqual(Expected_Exact_LogLik, coxres.LogLikelihood, TOL_LL, "Exact LogLik")

        Dim lr As Double = 2.0 * (coxres.LogLikelihood - coxres.LogLikelihoodNull)
        Assert.AreEqual(Expected_Exact_LR, lr, TOL_STAT, "Exact LR ChiSq")

        Dim scoreChi2 As Double = GetScoreChiSqFromPrivateField(cox)
        Assert.AreEqual(Expected_Exact_Score, scoreChi2, TOL_STAT, "Exact Score ChiSq")

        Dim waldChi2 As Double = WaldChiSquare(coxres.Coefficients, coxres.VarCov)
        Assert.AreEqual(Expected_Exact_Wald, waldChi2, TOL_STAT, "Exact Wald ChiSq")
    End Sub


    <TestCategory("CoxPH")>
    <TestMethod()>
    Public Sub CoxPH_Breslow_computes_all_residual_types_with_sanity_checks()

        Dim recs = LoadCoxRecords("coxph_dataset_strata_ties.csv")
        Dim cox As New CoxPH(recs, New String() {"x1", "x2"}, 200, 0.000000000001)
        Dim coxres = cox.Fit(BESHStatNG.TieMethod.Breslow)

        Dim p As Integer = coxres.Coefficients.Length
        Assert.AreEqual(2, p, "Expected 2 covariates in this test dataset.")

        ' Make a quick lookup of event/censoring by id
        Dim isEvent As New Dictionary(Of Integer, Boolean)()
        For Each r In recs
            isEvent(r.Index) = (r.Censorship = 1)
        Next

        ' 1) Score residuals: vector length p, finite; column sums ~ 0
        Dim score = cox.Residuals(BESHStatNG.ResidualType.Score)
        Assert.IsNotNull(score, "Score residuals returned Nothing.")
        Assert.AreEqual(recs.Count, score.Count, "Score residuals count mismatch.")
        Dim sumScore(p - 1) As Double

        For Each kv In score
            Dim v = kv.Value
            Assert.IsNotNull(v, "Score residual vector is Nothing.")
            Assert.AreEqual(p, v.Length, "Score residual vector length mismatch.")
            For kk As Integer = 0 To p - 1
                Assert.IsFalse(Double.IsNaN(v(kk)) OrElse Double.IsInfinity(v(kk)), "Score residual contains NaN/Inf.")
                sumScore(kk) += v(kk)
            Next
        Next
        For kk As Integer = 0 To p - 1
            Assert.IsTrue(Math.Abs(sumScore(kk)) < 0.00001, $"Score residuals should sum ~ 0 for covariate {kk}. Got {sumScore(kk)}")
        Next

        ' 2) Martingale residuals: scalar per subject, finite
        Dim mart = cox.Residuals(BESHStatNG.ResidualType.Martingale)
        Assert.IsNotNull(mart, "Martingale residuals returned Nothing.")
        Assert.AreEqual(recs.Count, mart.Count, "Martingale residuals count mismatch.")
        For Each kv In mart
            Dim v = kv.Value
            Assert.IsNotNull(v)
            Assert.AreEqual(1, v.Length, "Martingale residual should be scalar.")
            Assert.IsFalse(Double.IsNaN(v(0)) OrElse Double.IsInfinity(v(0)), "Martingale residual contains NaN/Inf.")
        Next

        ' 3) Deviance residuals: scalar per subject, finite; sign should generally match martingale sign for events
        Dim dev = cox.Residuals(BESHStatNG.ResidualType.Deviance)
        Assert.IsNotNull(dev, "Deviance residuals returned Nothing.")
        Assert.AreEqual(recs.Count, dev.Count, "Deviance residuals count mismatch.")
        For Each kv In dev
            Dim v = kv.Value
            Assert.IsNotNull(v)
            Assert.AreEqual(1, v.Length, "Deviance residual should be scalar.")
            Assert.IsFalse(Double.IsNaN(v(0)) OrElse Double.IsInfinity(v(0)), "Deviance residual contains NaN/Inf.")

            ' If it's an event, deviance residual sign should match martingale residual sign in standard definitions.
            If isEvent.ContainsKey(kv.Key) AndAlso isEvent(kv.Key) Then
                Dim m As Double = mart(kv.Key)(0)
                If Math.Abs(m) > 0.000000000001 Then
                    Assert.IsTrue(Math.Sign(v(0)) = Math.Sign(m), "Deviance residual sign should match martingale sign for events.")
                End If
            End If
        Next

        ' 4) Schoenfeld residuals: NaN for censored, numeric for events; vector length p
        Dim sch = cox.Residuals(BESHStatNG.ResidualType.Schoenfeld)
        Assert.IsNotNull(sch, "Schoenfeld residuals returned Nothing.")
        Assert.AreEqual(recs.Count, sch.Count, "Schoenfeld residuals count mismatch.")
        For Each kv In sch
            Dim v = kv.Value
            Assert.IsNotNull(v)
            Assert.AreEqual(p, v.Length, "Schoenfeld residual vector length mismatch.")
            If isEvent(kv.Key) Then
                For kk As Integer = 0 To p - 1
                    Assert.IsFalse(Double.IsNaN(v(kk)) OrElse Double.IsInfinity(v(kk)), "Schoenfeld residual for event should be numeric.")
                Next
            Else
                For kk As Integer = 0 To p - 1
                    Assert.IsTrue(Double.IsNaN(v(kk)), "Schoenfeld residual for censored should be NaN.")
                Next
            End If
        Next

        ' 5) Scaled Schoenfeld residuals: same NaN rules; vector length p
        Dim schs = cox.Residuals(BESHStatNG.ResidualType.SchoenfeldScaled)
        Assert.IsNotNull(schs, "Scaled Schoenfeld residuals returned Nothing.")
        Assert.AreEqual(recs.Count, schs.Count, "Scaled Schoenfeld residuals count mismatch.")
        For Each kv In schs
            Dim v = kv.Value
            Assert.IsNotNull(v)
            Assert.AreEqual(p, v.Length, "Scaled Schoenfeld residual vector length mismatch.")
            If isEvent(kv.Key) Then
                For kk As Integer = 0 To p - 1
                    Assert.IsFalse(Double.IsNaN(v(kk)) OrElse Double.IsInfinity(v(kk)), "Scaled Schoenfeld residual for event should be numeric.")
                Next
            Else
                For kk As Integer = 0 To p - 1
                    Assert.IsTrue(Double.IsNaN(v(kk)), "Scaled Schoenfeld residual for censored should be NaN.")
                Next
            End If
        Next

        ' 6) Dfbeta residuals: vector length p, finite
        Dim dfb = cox.Residuals(BESHStatNG.ResidualType.Dfbeta)
        Assert.IsNotNull(dfb, "Dfbeta residuals returned Nothing.")
        Assert.AreEqual(recs.Count, dfb.Count, "Dfbeta residuals count mismatch.")
        For Each kv In dfb
            Dim v = kv.Value
            Assert.IsNotNull(v)
            Assert.AreEqual(p, v.Length, "Dfbeta residual vector length mismatch.")
            For kk As Integer = 0 To p - 1
                Assert.IsFalse(Double.IsNaN(v(kk)) OrElse Double.IsInfinity(v(kk)), "Dfbeta residual contains NaN/Inf.")
            Next
        Next

        ' 7) Dfbetas residuals: vector length p, finite
        Dim dfbs = cox.Residuals(BESHStatNG.ResidualType.Dfbetas)
        Assert.IsNotNull(dfbs, "Dfbetas residuals returned Nothing.")
        Assert.AreEqual(recs.Count, dfbs.Count, "Dfbetas residuals count mismatch.")
        For Each kv In dfbs
            Dim v = kv.Value
            Assert.IsNotNull(v)
            Assert.AreEqual(p, v.Length, "Dfbetas residual vector length mismatch.")
            For kk As Integer = 0 To p - 1
                Assert.IsFalse(Double.IsNaN(v(kk)) OrElse Double.IsInfinity(v(kk)), "Dfbetas residual contains NaN/Inf.")
            Next
        Next

        ' 8) Cox-Snell residuals: scalar per subject, non-negative
        Dim cs = cox.Residuals(BESHStatNG.ResidualType.CoxSnell)
        Assert.IsNotNull(cs, "Cox-Snell residuals returned Nothing.")
        Assert.AreEqual(recs.Count, cs.Count, "Cox-Snell residuals count mismatch.")
        For Each kv In cs
            Dim v = kv.Value
            Assert.IsNotNull(v)
            Assert.AreEqual(1, v.Length, "Cox-Snell residual should be scalar.")
            Assert.IsFalse(Double.IsNaN(v(0)) OrElse Double.IsInfinity(v(0)), "Cox-Snell residual contains NaN/Inf.")
            Assert.IsTrue(v(0) >= -0.000000000001, "Cox-Snell residual should be >= 0.")
        Next

    End Sub

End Class