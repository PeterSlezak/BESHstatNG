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

' ===== BEGIN migrated from LMMRandomInterceptKRAgainstRReferenceTests.vb =====



' Compares BESHStatNG internal KR-backend LMM output against hard-coded
' R lme4 + pbkrtest reference values for the random-intercept LMM data.
'
' R reference model:
'
'   fit <- lme4::lmer(y ~ visit + (1 | subject), data = dat, REML = TRUE)
'   vc_kr <- as.matrix(pbkrtest::vcovAdj(fit))
'
' Important implementation note:
'
'   For this balanced random-intercept data set, pbkrtest::vcovAdj(fit) gives
'   KR-adjusted fixed-effect SEs that are effectively equal to the ordinary REML
'   fixed-effect SEs.
'
'   This BESHStatNG validation test therefore uses the current linear KR adjusted
'   covariance path by leaving BuildKenwardRogerSecondDerivatives = False.
'
'   The second-derivative R_hj path is still under validation for LMMs because the
'   current engine finite-differences V(theta) on the optimizer's transformed
'   log-variance scale.  Full KR R_hj terms should ultimately be computed on a
'   validated covariance-parameter scale before being used as a general LMM reference.
'
' This test compares:
'   - fixed-effect estimates,
'   - KR-adjusted standard errors from BESHStatNG adjusted Var(beta).
'
' It intentionally does not compare KR denominator DF, t, or p-values yet.
<TestClass()>
Public Class LMMRandomInterceptKRAgainstRReferenceTests

    Public Property TestContext As Microsoft.VisualStudio.TestTools.UnitTesting.TestContext

    Private Const ESTIMATE_TOL As Double = 0.0005
    Private Const KR_SE_TOL As Double = 0.005

    <TestMethod()>
    Public Sub RandomInterceptLMM_KRAdjustedSE_MatchesHardCodedRReference()
        Dim dat As MixedCsvData = LoadMixedCsv("mixedmodel_lmm_random_intercept_data.csv")
        Dim startRef As Dictionary(Of String, Double) = LoadReferenceCsvPreferCorrected()

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

        req.RequestLabel = "Random-intercept LMM KR against hard-coded R reference"
        req.ResponseVarName = "y"
        req.SubjectVarName = "subject"
        req.VisitVarName = "visit"

        req.FixedEffectNames = {"(Intercept)", "visit"}
        req.RandomEffectNames = {"(Intercept)"}

        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal
        req.BuildKenwardRogerWorkspace = True
        req.BuildKenwardRogerSecondDerivatives = True

        req.Control = TestControl()

        If startRef IsNot Nothing AndAlso startRef.ContainsKey("var_random_intercept") AndAlso startRef.ContainsKey("var_residual") Then
            req.StartThetaG = {Math.Log(startRef("var_random_intercept"))}
            req.StartThetaR = {Math.Log(startRef("var_residual"))}
        End If

        Dim res As MixedModelResult = (New LMM(req)).Fit()

        Assert.IsNotNull(res, "LMM result should not be Nothing.")
        Assert.IsTrue(res.Converged, "Random-intercept LMM should converge.")
        Assert.AreEqual(2, res.P, "Unexpected fixed-effect dimension.")

        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Rmats,
                 "Covariance-scale KR should populate Rmats when second derivatives are requested.")

        Assert.AreEqual(MixedModelKrParameterScale.Covariance, res.KenwardRogerWorkspace.ParameterScale,
                "KR random-intercept LMM should use covariance-parameter scale.")

        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)
        Assert.IsNotNull(adjusted, "KR adjusted Var(beta) should be available.")
        Assert.AreEqual(2, adjusted.GetLength(0), "Adjusted Var(beta) row dimension.")
        Assert.AreEqual(2, adjusted.GetLength(1), "Adjusted Var(beta) column dimension.")

        Dim refs As List(Of RReferenceRow) = GetHardCodedRReferenceRows()

        Dim comparisonCsv As String = BuildComparisonCsv(res, adjusted, refs)
        WriteComparisonCsv(comparisonCsv, "besh_vs_r_lmm_random_intercept_kr_comparison.csv")

        For j As Integer = 0 To refs.Count - 1
            Dim expected As RReferenceRow = refs(j)

            AssertAlmostEqual(expected.Beta,
                              res.Beta(j),
                              ESTIMATE_TOL,
                              expected.Effect & " estimate")

            Dim actualKRSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))

            AssertAlmostEqual(expected.KRAdjustedSE,
                              actualKRSE,
                              KR_SE_TOL,
                              expected.Effect & " KR adjusted SE")
        Next
    End Sub


    Private Shared Function GetHardCodedRReferenceRows() As List(Of RReferenceRow)
        ' Reference from:
        '
        '   fit <- lme4::lmer(y ~ visit + (1 | subject), data = dat, REML = TRUE)
        '   vc_kr <- as.matrix(pbkrtest::vcovAdj(fit))
        '
        ' For this balanced random-intercept dataset, KR-adjusted SEs from
        ' pbkrtest::vcovAdj(fit) are effectively the ordinary REML fixed-effect SEs.
        Return New List(Of RReferenceRow) From {
            New RReferenceRow(effect:="(Intercept)",
                              beta:=10.0230555555556,
                              ordinarySE:=0.29586106,
                              krAdjustedSE:=0.29586106),
            New RReferenceRow(effect:="visit",
                              beta:=1.4325,
                              ordinarySE:=0.04475452,
                              krAdjustedSE:=0.04475452)
        }
    End Function


    Private Shared Sub AssertAlmostEqual(expected As Double,
                                         actual As Double,
                                         tolerance As Double,
                                         label As String)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail(label & ": actual value is not finite. Expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If

        Dim diff As Double = Math.Abs(expected - actual)

        If diff > tolerance Then
            Assert.Fail(label & ": expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " &
                        actual.ToString("G17", CultureInfo.InvariantCulture) &
                        ", abs diff " &
                        diff.ToString("G17", CultureInfo.InvariantCulture) &
                        " > tolerance " &
                        tolerance.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If
    End Sub


    Private Shared Function BuildComparisonCsv(res As MixedModelResult,
                                               adjusted(,) As Double,
                                               refs As List(Of RReferenceRow)) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("effect,r_beta,besh_beta,beta_diff,r_ordinary_se,r_kr_se,besh_ordinary_se,besh_kr_se,kr_se_diff")

        For j As Integer = 0 To refs.Count - 1
            Dim r As RReferenceRow = refs(j)

            Dim beshBeta As Double = res.Beta(j)
            Dim beshKRSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))
            Dim beshOrdinarySE As Double = Math.Sqrt(Math.Max(0.0, res.VarBeta(j, j)))

            sb.AppendLine(String.Join(",",
                                      Csv(r.Effect),
                                      Csv(r.Beta),
                                      Csv(beshBeta),
                                      Csv(beshBeta - r.Beta),
                                      Csv(r.OrdinarySE),
                                      Csv(r.KRAdjustedSE),
                                      Csv(beshOrdinarySE),
                                      Csv(beshKRSE),
                                      Csv(beshKRSE - r.KRAdjustedSE)))
        Next

        Return sb.ToString()
    End Function


    Private Sub WriteComparisonCsv(contents As String,
                                   fileName As String)
        Try
            Dim outDir As String = GetExportDirectory()
            Dim path As String = System.IO.Path.Combine(outDir, fileName)
            File.WriteAllText(path, contents, Encoding.UTF8)

            If Me.TestContext IsNot Nothing Then
                Me.TestContext.WriteLine("Wrote LMM R comparison CSV: " & path)

                Try
                    Me.TestContext.AddResultFile(path)
                Catch ex As Exception
                    Me.TestContext.WriteLine("Could not attach comparison CSV: " & ex.Message)
                End Try
            End If
        Catch ex As Exception
            If Me.TestContext IsNot Nothing Then
                Me.TestContext.WriteLine("Could not write comparison CSV: " & ex.ToString())
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
                Dim stableOut As String = Path.Combine(dir.FullName, "KRValidationExports")
                Directory.CreateDirectory(stableOut)
                Return stableOut
            End If

            dir = dir.Parent
        End While

        Dim fallback As String = Path.Combine(baseDir, "KRValidationExports")
        Directory.CreateDirectory(fallback)
        Return fallback
    End Function


    Private Shared Function LoadMixedCsv(fileName As String) As MixedCsvData
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        Dim header() As String = lines(0).Split(","c)
        Dim col As Dictionary(Of String, Integer) = BuildColumnMap(header)

        RequireColumn(col, "subject", fileName)
        RequireColumn(col, "visit", fileName)
        RequireColumn(col, "y", fileName)

        Dim n As Integer = lines.Length - 1
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double
        Dim y(n - 1) As Double

        For i As Integer = 0 To n - 1
            Dim parts() As String = lines(i + 1).Split(","c)

            subject(i) = parts(col("subject")).Trim()
            visit(i) = ParseD(parts(col("visit")))
            y(i) = ParseD(parts(col("y")))
        Next

        Return New MixedCsvData With {
            .Subject = subject,
            .Visit = visit,
            .Y = y
        }
    End Function


    Private Shared Function LoadReferenceCsvPreferCorrected() As Dictionary(Of String, Double)
        Try
            Return LoadReferenceCsv("mixedmodel_lmm_random_intercept_reference_corrected.csv")
        Catch
            Return LoadReferenceCsv("mixedmodel_lmm_random_intercept_reference.csv")
        End Try
    End Function


    Private Shared Function LoadReferenceCsv(fileName As String) As Dictionary(Of String, Double)
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        Dim out As New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)

        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For

            Dim parts() As String = lines(i).Split(","c)
            If parts.Length >= 2 Then out(parts(0).Trim()) = ParseD(parts(1))
        Next

        Return out
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


    Private Shared Function TestControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 180
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
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


    Private Shared Function BuildColumnMap(header() As String) As Dictionary(Of String, Integer)
        Dim col As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For j As Integer = 0 To header.Length - 1
            col(header(j).Trim()) = j
        Next

        Return col
    End Function


    Private Shared Sub RequireColumn(col As Dictionary(Of String, Integer),
                                     columnName As String,
                                     sourceName As String)
        If Not col.ContainsKey(columnName) Then
            Throw New InvalidOperationException("CSV file '" & sourceName & "' must contain column '" & columnName & "'.")
        End If
    End Sub


    Private Shared Function ParseD(text As String) As Double
        Return Double.Parse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function


    Private Shared Function Csv(value As Object) As String
        If value Is Nothing Then Return String.Empty

        If TypeOf value Is Double Then
            Dim d As Double = CDbl(value)
            If Double.IsNaN(d) Then Return "NaN"
            If Double.IsPositiveInfinity(d) Then Return "Inf"
            If Double.IsNegativeInfinity(d) Then Return "-Inf"
            Return d.ToString("G17", CultureInfo.InvariantCulture)
        End If

        Dim s As String = Convert.ToString(value, CultureInfo.InvariantCulture)
        If s Is Nothing Then Return String.Empty

        If s.Contains(",") OrElse s.Contains("""") OrElse s.Contains(vbCr) OrElse s.Contains(vbLf) Then
            s = """" & s.Replace("""", """""") & """"
        End If

        Return s
    End Function


    Private Class RReferenceRow
        Public ReadOnly Effect As String
        Public ReadOnly Beta As Double
        Public ReadOnly OrdinarySE As Double
        Public ReadOnly KRAdjustedSE As Double

        Public Sub New(effect As String,
                       beta As Double,
                       ordinarySE As Double,
                       krAdjustedSE As Double)
            Me.Effect = effect
            Me.Beta = beta
            Me.OrdinarySE = ordinarySE
            Me.KRAdjustedSE = krAdjustedSE
        End Sub
    End Class


    Private Class MixedCsvData
        Public Subject() As Object
        Public Visit() As Double
        Public Y() As Double
    End Class

End Class

' ===== END migrated from LMMRandomInterceptKRAgainstRReferenceTests.vb =====

' ===== BEGIN migrated from LMMRandomSlopeSleepstudyKRAgainstPbkrtestReferenceTests.vb =====



' Compares BESHStatNG covariance-scale KR LMM output against hard-coded
' R lme4 + pbkrtest reference values for the lme4 sleepstudy random-slope model.
'
' R reference model:
'
'   fit <- lme4::lmer(Reaction ~ Days + (Days | Subject),
'                     data = lme4::sleepstudy,
'                     REML = TRUE)
'
'   vc_kr <- as.matrix(pbkrtest::vcovAdj(fit))
'
' pbkrtest documents this model as a case where vcovAdj(fit) and vcov(fit)
' are identical. Therefore the KR-adjusted SEs below are the lme4 REML
' fixed-effect SEs.
'
' This test compares:
'   - fixed-effect estimates,
'   - KR-adjusted SE from adjusted Var(beta),
'   - covariance-scale KR workspace selection.
'
' It intentionally does not compare KR denominator DF, t, or p-values yet.
<TestClass()>
Public Class LMMRandomSlopeSleepstudyKRAgainstPbkrtestReferenceTests

    Public Property TestContext As Microsoft.VisualStudio.TestTools.UnitTesting.TestContext

    Private Const ESTIMATE_TOL As Double = 0.005
    Private Const KR_SE_TOL As Double = 0.02

    <TestMethod()>
    Public Sub SleepstudyRandomSlopeLMM_KRAdjustedSE_MatchesPbkrtestReference()
        Dim dat As SleepstudyData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_data.csv")

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

        req.RequestLabel = "sleepstudy random-slope LMM KR against pbkrtest reference"
        req.ResponseVarName = "reaction"
        req.SubjectVarName = "subject"
        req.VisitVarName = "days"
        req.FixedEffectNames = {"(Intercept)", "days"}
        req.RandomEffectNames = {"(Intercept)", "days"}

        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal
        req.BuildKenwardRogerWorkspace = True
        req.BuildKenwardRogerSecondDerivatives = True
        req.Control = TestControl()

        ' lme4 REML starts from the published sleepstudy fit:
        ' Subject SD(intercept)=24.7405, SD(days)=5.9221, Corr=0.066;
        ' residual variance=654.941.
        req.StartThetaG = {Math.Log(24.7405), Math.Log(5.9221), Atanh(0.066)}
        req.StartThetaR = {Math.Log(654.941)}

        Dim res As MixedModelResult = (New LMM(req)).Fit()

        Assert.IsNotNull(res, "LMM result should not be Nothing.")
        Assert.IsTrue(res.Converged, "sleepstudy random-slope LMM should converge.")
        Assert.AreEqual(2, res.P, "Unexpected fixed-effect dimension.")
        Assert.AreEqual(180, res.Nobs, "Unexpected observation count.")
        Assert.AreEqual(18, res.NoSubjects, "Unexpected subject count.")

        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        Assert.AreEqual(MixedModelKrParameterScale.Covariance,
                        res.KenwardRogerWorkspace.ParameterScale,
                        "Random-slope LMM should use covariance-parameter KR scale.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Rmats,
                         "Covariance-scale KR should populate Rmats when second derivatives are requested.")

        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)
        Assert.IsNotNull(adjusted, "KR adjusted Var(beta) should be available.")
        Assert.AreEqual(2, adjusted.GetLength(0), "Adjusted Var(beta) row dimension.")
        Assert.AreEqual(2, adjusted.GetLength(1), "Adjusted Var(beta) column dimension.")

        Dim refs As List(Of RReferenceRow) = GetHardCodedRReferenceRows()

        Dim comparisonCsv As String = BuildComparisonCsv(res, adjusted, refs)
        WriteComparisonCsv(comparisonCsv, "besh_vs_r_lmm_sleepstudy_random_slope_kr_comparison.csv")

        For j As Integer = 0 To refs.Count - 1
            Dim expected As RReferenceRow = refs(j)

            AssertAlmostEqual(expected.Beta,
                              res.Beta(j),
                              ESTIMATE_TOL,
                              expected.Effect & " estimate")

            Dim actualKRSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))

            AssertAlmostEqual(expected.KRAdjustedSE,
                              actualKRSE,
                              KR_SE_TOL,
                              expected.Effect & " KR adjusted SE")
        Next

        ' For the balanced sleepstudy example, pbkrtest::vcovAdj(fit) equals vcov(fit).
        ' Because BESHStatNG is using covariance-scale parameters here, the adjusted
        ' covariance should also be extremely close to ordinary Var(beta).
        For j As Integer = 0 To res.P - 1
            Dim ordinarySE As Double = Math.Sqrt(Math.Max(0.0, res.VarBeta(j, j)))
            Dim adjustedSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))
            Assert.AreEqual(ordinarySE, adjustedSE, 0.001,
                            "sleepstudy adjusted SE should match ordinary SE at beta index " & j.ToString(CultureInfo.InvariantCulture))
        Next
    End Sub


    Private Shared Function GetHardCodedRReferenceRows() As List(Of RReferenceRow)
        ' Reference from:
        '
        '   lme4::lmer(Reaction ~ Days + (Days | Subject),
        '              data = lme4::sleepstudy,
        '              REML = TRUE)
        '
        ' and:
        '
        '   pbkrtest::vcovAdj(fit)
        '
        ' For this balanced example, pbkrtest documents vcovAdj(fit) == vcov(fit).
        Return New List(Of RReferenceRow) From {
            New RReferenceRow(effect:="(Intercept)",
                              beta:=251.40510485,
                              ordinarySE:=6.8246,
                              krAdjustedSE:=6.8246),
            New RReferenceRow(effect:="days",
                              beta:=10.46728596,
                              ordinarySE:=1.5458,
                              krAdjustedSE:=1.5458)
        }
    End Function


    Private Shared Sub AssertAlmostEqual(expected As Double,
                                         actual As Double,
                                         tolerance As Double,
                                         label As String)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail(label & ": actual value is not finite. Expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If

        Dim diff As Double = Math.Abs(expected - actual)

        If diff > tolerance Then
            Assert.Fail(label & ": expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " &
                        actual.ToString("G17", CultureInfo.InvariantCulture) &
                        ", abs diff " &
                        diff.ToString("G17", CultureInfo.InvariantCulture) &
                        " > tolerance " &
                        tolerance.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If
    End Sub


    Private Shared Function BuildComparisonCsv(res As MixedModelResult,
                                               adjusted(,) As Double,
                                               refs As List(Of RReferenceRow)) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("effect,r_beta,besh_beta,beta_diff,r_ordinary_se,r_kr_se,besh_ordinary_se,besh_kr_se,kr_se_diff,kr_parameter_scale")

        For j As Integer = 0 To refs.Count - 1
            Dim r As RReferenceRow = refs(j)

            Dim beshBeta As Double = res.Beta(j)
            Dim beshKRSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))
            Dim beshOrdinarySE As Double = Math.Sqrt(Math.Max(0.0, res.VarBeta(j, j)))

            sb.AppendLine(String.Join(",",
                                      Csv(r.Effect),
                                      Csv(r.Beta),
                                      Csv(beshBeta),
                                      Csv(beshBeta - r.Beta),
                                      Csv(r.OrdinarySE),
                                      Csv(r.KRAdjustedSE),
                                      Csv(beshOrdinarySE),
                                      Csv(beshKRSE),
                                      Csv(beshKRSE - r.KRAdjustedSE),
                                      Csv(res.KenwardRogerWorkspace.ParameterScale.ToString())))
        Next

        Return sb.ToString()
    End Function


    Private Sub WriteComparisonCsv(contents As String,
                                   fileName As String)
        Try
            Dim outDir As String = GetExportDirectory()
            Dim path As String = System.IO.Path.Combine(outDir, fileName)
            File.WriteAllText(path, contents, Encoding.UTF8)

            If Me.TestContext IsNot Nothing Then
                Me.TestContext.WriteLine("Wrote sleepstudy random-slope R comparison CSV: " & path)

                Try
                    Me.TestContext.AddResultFile(path)
                Catch ex As Exception
                    Me.TestContext.WriteLine("Could not attach comparison CSV: " & ex.Message)
                End Try
            End If
        Catch ex As Exception
            If Me.TestContext IsNot Nothing Then
                Me.TestContext.WriteLine("Could not write comparison CSV: " & ex.ToString())
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
                Dim stableOut As String = Path.Combine(dir.FullName, "KRValidationExports")
                Directory.CreateDirectory(stableOut)
                Return stableOut
            End If

            dir = dir.Parent
        End While

        Dim fallback As String = Path.Combine(baseDir, "KRValidationExports")
        Directory.CreateDirectory(fallback)
        Return fallback
    End Function


    Private Shared Function TestControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 240
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


    Private Shared Function Csv(value As Object) As String
        If value Is Nothing Then Return String.Empty

        If TypeOf value Is Double Then
            Dim d As Double = CDbl(value)
            If Double.IsNaN(d) Then Return "NaN"
            If Double.IsPositiveInfinity(d) Then Return "Inf"
            If Double.IsNegativeInfinity(d) Then Return "-Inf"
            Return d.ToString("G17", CultureInfo.InvariantCulture)
        End If

        Dim s As String = Convert.ToString(value, CultureInfo.InvariantCulture)
        If s Is Nothing Then Return String.Empty

        If s.Contains(",") OrElse s.Contains("""") OrElse s.Contains(vbCr) OrElse s.Contains(vbLf) Then
            s = """" & s.Replace("""", """""") & """"
        End If

        Return s
    End Function


    Private Class RReferenceRow
        Public ReadOnly Effect As String
        Public ReadOnly Beta As Double
        Public ReadOnly OrdinarySE As Double
        Public ReadOnly KRAdjustedSE As Double

        Public Sub New(effect As String,
                       beta As Double,
                       ordinarySE As Double,
                       krAdjustedSE As Double)
            Me.Effect = effect
            Me.Beta = beta
            Me.OrdinarySE = ordinarySE
            Me.KRAdjustedSE = krAdjustedSE
        End Sub
    End Class


    Private Class SleepstudyData
        Public Reaction() As Double
        Public Days() As Double
        Public Subject() As Object
    End Class

End Class

' ===== END migrated from LMMRandomSlopeSleepstudyKRAgainstPbkrtestReferenceTests.vb =====

' ===== BEGIN migrated from LMMRandomSlopeSleepstudyUnbalancedKRValidationTests.vb =====



' Non-balanced sleepstudy random-slope LMM KR validation harness.
'
' The first test is an always-on internal validation:
'   - fits the non-balanced random-slope LMM,
'   - verifies KR uses covariance scale,
'   - verifies adjusted Var(beta) is finite,
'   - writes a BESH diagnostic CSV.
'
' The second test uses hard-coded R lme4 + pbkrtest reference values generated by:
'
'   kr_lmm_sleepstudy_random_slope_unbalanced_reference.R
'
' It validates the deterministic unbalanced sleepstudy random-slope case without
' requiring R at test runtime.
'
<TestClass()>
Public Class LMMRandomSlopeSleepstudyUnbalancedKRValidationTests

    Public Property TestContext As Microsoft.VisualStudio.TestTools.UnitTesting.TestContext

    Private Const ESTIMATE_TOL As Double = 0.00005
    Private Const KR_SE_TOL As Double = 0.0003

    <TestMethod()>
    Public Sub SleepstudyUnbalancedRandomSlopeLMM_CovarianceScaleKR_IsFiniteAndExportsDiagnostics()
        Dim dat As SleepstudyData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
        Dim res As MixedModelResult = FitSleepstudyRandomSlope(dat)

        AssertUsableUnbalancedFit(res, expectedNobs:=165, expectedSubjects:=18)

        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)
        Assert.IsNotNull(adjusted, "KR adjusted Var(beta) should be available.")
        Assert.AreEqual(2, adjusted.GetLength(0), "Adjusted Var(beta) row dimension.")
        Assert.AreEqual(2, adjusted.GetLength(1), "Adjusted Var(beta) column dimension.")

        For j As Integer = 0 To res.P - 1
            AssertFinite(res.Beta(j), "beta " & j.ToString(CultureInfo.InvariantCulture))
            AssertFinite(res.VarBeta(j, j), "ordinary Var(beta) diag " & j.ToString(CultureInfo.InvariantCulture))
            AssertFinite(adjusted(j, j), "KR adjusted Var(beta) diag " & j.ToString(CultureInfo.InvariantCulture))
            Assert.IsTrue(res.VarBeta(j, j) > 0.0, "ordinary Var(beta) diag should be positive.")
            Assert.IsTrue(adjusted(j, j) > 0.0, "KR adjusted Var(beta) diag should be positive.")
        Next

        For j As Integer = 0 To res.P - 1
            Dim l(res.P - 1) As Double
            l(j) = 1.0

            Dim krInf As MixedModelKenwardRogerUnivariateInference = Nothing
            Dim krMsg As String = Nothing

            Assert.IsTrue(MixedModelKenwardRogerInference.TryUnivariateInference(res,
                                                                         If(j = 0, "(Intercept)", "days"),
                                                                         l,
                                                                         krInf,
                                                                         alpha:=0.05,
                                                                         diagnostic:=krMsg),
                  "KR univariate inference failed for beta index " &
                  j.ToString(CultureInfo.InvariantCulture) &
                  ": " & krMsg)

            Assert.IsNotNull(krInf)
            AssertFinite(krInf.AdjustedStdError, "KR adjusted SE beta " & j.ToString(CultureInfo.InvariantCulture))
            AssertFinite(krInf.Statistic, "KR t statistic beta " & j.ToString(CultureInfo.InvariantCulture))
            AssertFinite(krInf.DF, "KR approximate df beta " & j.ToString(CultureInfo.InvariantCulture))
            Assert.IsTrue(krInf.DF > 0.0, "KR approximate df should be positive.")
        Next

        WriteComparisonCsv(BuildBeshOnlyDiagnosticCsv(res, adjusted),
                           "besh_lmm_sleepstudy_random_slope_unbalanced_kr_diagnostics.csv")
    End Sub


    <TestMethod()>
    Public Sub SleepstudyUnbalancedRandomSlopeLMM_KRAdjustedSE_MatchesPbkrtestReference()
        Dim dat As SleepstudyData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
        Dim res As MixedModelResult = FitSleepstudyRandomSlope(dat)

        AssertUsableUnbalancedFit(res, expectedNobs:=165, expectedSubjects:=18)

        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)
        Assert.IsNotNull(adjusted, "KR adjusted Var(beta) should be available.")

        Dim refs As List(Of RReferenceRow) = GetHardCodedRReferenceRows()

        Dim comparisonCsv As String = BuildComparisonCsv(res, adjusted, refs)
        WriteComparisonCsv(comparisonCsv, "besh_vs_r_lmm_sleepstudy_random_slope_unbalanced_kr_comparison.csv")

        For j As Integer = 0 To refs.Count - 1
            Dim expected As RReferenceRow = refs(j)

            AssertAlmostEqual(expected.Beta,
                              res.Beta(j),
                              ESTIMATE_TOL,
                              expected.Effect & " estimate")

            Dim actualOrdinarySE As Double = Math.Sqrt(Math.Max(0.0, res.VarBeta(j, j)))
            Dim actualKRSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))

            AssertAlmostEqual(expected.OrdinarySE,
                              actualOrdinarySE,
                              KR_SE_TOL,
                              expected.Effect & " ordinary SE")

            AssertAlmostEqual(expected.KRAdjustedSE,
                              actualKRSE,
                              KR_SE_TOL,
                              expected.Effect & " KR adjusted SE")
        Next
    End Sub


    Private Shared Function FitSleepstudyRandomSlope(dat As SleepstudyData) As MixedModelResult
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

        req.RequestLabel = "sleepstudy unbalanced random-slope LMM KR validation"
        req.ResponseVarName = "reaction"
        req.SubjectVarName = "subject"
        req.VisitVarName = "days"
        req.FixedEffectNames = {"(Intercept)", "days"}
        req.RandomEffectNames = {"(Intercept)", "days"}

        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal
        req.BuildKenwardRogerWorkspace = True
        req.BuildKenwardRogerSecondDerivatives = True
        req.Control = TestControl()

        ' Starts from the published balanced sleepstudy REML fit. These are robust
        ' for the deterministic non-balanced subset used by this test.
        req.StartThetaG = {Math.Log(24.7405), Math.Log(5.9221), Atanh(0.066)}
        req.StartThetaR = {Math.Log(654.941)}

        Return (New LMM(req)).Fit()
    End Function


    Private Shared Sub AssertUsableUnbalancedFit(res As MixedModelResult,
                                                 expectedNobs As Integer,
                                                 expectedSubjects As Integer)
        Assert.IsNotNull(res, "LMM result should not be Nothing.")
        Assert.IsTrue(res.Converged, "Unbalanced sleepstudy random-slope LMM should converge.")
        Assert.AreEqual(2, res.P, "Unexpected fixed-effect dimension.")
        Assert.AreEqual(expectedNobs, res.Nobs, "Unexpected observation count.")
        Assert.AreEqual(expectedSubjects, res.NoSubjects, "Unexpected subject count.")

        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        Assert.AreEqual(MixedModelKrParameterScale.Covariance,
                        res.KenwardRogerWorkspace.ParameterScale,
                        "Random-slope LMM should use covariance-parameter KR scale.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Rmats,
                         "Covariance-scale KR should populate Rmats when second derivatives are requested.")
    End Sub


    Private Shared Function GetHardCodedRReferenceRows() As List(Of RReferenceRow)
        ' Fill these constants from:
        '
        '   kr_lmm_sleepstudy_random_slope_unbalanced_reference.R
        '
        ' Replace the NaN values, remove the <Ignore> attribute from the reference
        ' test, and tighten tolerances if the comparison is stable.
        Return New List(Of RReferenceRow) From {
            New RReferenceRow(effect:="(Intercept)",
                              beta:=248.8345407093,
                              ordinarySE:=6.82866557863,
                              krAdjustedSE:=6.83613875675),
            New RReferenceRow(effect:="days",
                              beta:=11.1253591171,
                              ordinarySE:=1.62187982163,
                              krAdjustedSE:=1.62280741609)
        }
    End Function


    Private Shared Sub AssertAlmostEqual(expected As Double,
                                         actual As Double,
                                         tolerance As Double,
                                         label As String)
        If Double.IsNaN(expected) OrElse Double.IsInfinity(expected) Then
            Assert.Fail(label & ": R reference constant has not been filled in.")
        End If

        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail(label & ": actual value is not finite. Expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If

        Dim diff As Double = Math.Abs(expected - actual)

        If diff > tolerance Then
            Assert.Fail(label & ": expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " &
                        actual.ToString("G17", CultureInfo.InvariantCulture) &
                        ", abs diff " &
                        diff.ToString("G17", CultureInfo.InvariantCulture) &
                        " > tolerance " &
                        tolerance.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If
    End Sub


    Private Shared Function BuildBeshOnlyDiagnosticCsv(res As MixedModelResult,
                                                       adjusted(,) As Double) As String
        Dim names() As String = {"(Intercept)", "days"}
        Dim sb As New StringBuilder()
        sb.AppendLine("effect,besh_beta,besh_ordinary_se,besh_kr_se,kr_minus_ordinary_se,kr_parameter_scale")

        For j As Integer = 0 To names.Length - 1
            Dim ordinarySE As Double = Math.Sqrt(Math.Max(0.0, res.VarBeta(j, j)))
            Dim krSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))

            sb.AppendLine(String.Join(",",
                                      Csv(names(j)),
                                      Csv(res.Beta(j)),
                                      Csv(ordinarySE),
                                      Csv(krSE),
                                      Csv(krSE - ordinarySE),
                                      Csv(res.KenwardRogerWorkspace.ParameterScale.ToString())))
        Next

        Return sb.ToString()
    End Function


    Private Shared Function BuildComparisonCsv(res As MixedModelResult,
                                               adjusted(,) As Double,
                                               refs As List(Of RReferenceRow)) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("effect,r_beta,besh_beta,beta_diff,r_ordinary_se,r_kr_se,besh_ordinary_se,besh_kr_se,kr_se_diff,kr_parameter_scale")

        For j As Integer = 0 To refs.Count - 1
            Dim r As RReferenceRow = refs(j)

            Dim beshBeta As Double = res.Beta(j)
            Dim beshKRSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))
            Dim beshOrdinarySE As Double = Math.Sqrt(Math.Max(0.0, res.VarBeta(j, j)))

            sb.AppendLine(String.Join(",",
                                      Csv(r.Effect),
                                      Csv(r.Beta),
                                      Csv(beshBeta),
                                      Csv(beshBeta - r.Beta),
                                      Csv(r.OrdinarySE),
                                      Csv(r.KRAdjustedSE),
                                      Csv(beshOrdinarySE),
                                      Csv(beshKRSE),
                                      Csv(beshKRSE - r.KRAdjustedSE),
                                      Csv(res.KenwardRogerWorkspace.ParameterScale.ToString())))
        Next

        Return sb.ToString()
    End Function


    Private Sub WriteComparisonCsv(contents As String,
                                   fileName As String)
        Try
            Dim outDir As String = GetExportDirectory()
            Dim path As String = System.IO.Path.Combine(outDir, fileName)
            File.WriteAllText(path, contents, Encoding.UTF8)

            If Me.TestContext IsNot Nothing Then
                Me.TestContext.WriteLine("Wrote unbalanced sleepstudy KR CSV: " & path)

                Try
                    Me.TestContext.AddResultFile(path)
                Catch ex As Exception
                    Me.TestContext.WriteLine("Could not attach comparison CSV: " & ex.Message)
                End Try
            End If
        Catch ex As Exception
            If Me.TestContext IsNot Nothing Then
                Me.TestContext.WriteLine("Could not write comparison CSV: " & ex.ToString())
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
                Dim stableOut As String = Path.Combine(dir.FullName, "KRValidationExports")
                Directory.CreateDirectory(stableOut)
                Return stableOut
            End If

            dir = dir.Parent
        End While

        Dim fallback As String = Path.Combine(baseDir, "KRValidationExports")
        Directory.CreateDirectory(fallback)
        Return fallback
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


    Private Shared Function AssertFinite(value As Double,
                                         label As String) As Boolean
        Assert.IsFalse(Double.IsNaN(value), label & " should not be NaN.")
        Assert.IsFalse(Double.IsInfinity(value), label & " should not be infinite.")
        Return True
    End Function


    Private Shared Function Csv(value As Object) As String
        If value Is Nothing Then Return String.Empty

        If TypeOf value Is Double Then
            Dim d As Double = CDbl(value)
            If Double.IsNaN(d) Then Return "NaN"
            If Double.IsPositiveInfinity(d) Then Return "Inf"
            If Double.IsNegativeInfinity(d) Then Return "-Inf"
            Return d.ToString("G17", CultureInfo.InvariantCulture)
        End If

        Dim s As String = Convert.ToString(value, CultureInfo.InvariantCulture)
        If s Is Nothing Then Return String.Empty

        If s.Contains(",") OrElse s.Contains("""") OrElse s.Contains(vbCr) OrElse s.Contains(vbLf) Then
            s = """" & s.Replace("""", """""") & """"
        End If

        Return s
    End Function


    Private Class RReferenceRow
        Public ReadOnly Effect As String
        Public ReadOnly Beta As Double
        Public ReadOnly OrdinarySE As Double
        Public ReadOnly KRAdjustedSE As Double

        Public Sub New(effect As String,
                       beta As Double,
                       ordinarySE As Double,
                       krAdjustedSE As Double)
            Me.Effect = effect
            Me.Beta = beta
            Me.OrdinarySE = ordinarySE
            Me.KRAdjustedSE = krAdjustedSE
        End Sub
    End Class


    Private Class SleepstudyData
        Public Reaction() As Double
        Public Days() As Double
        Public Subject() As Object
    End Class

End Class

' ===== END migrated from LMMRandomSlopeSleepstudyUnbalancedKRValidationTests.vb =====


' ===== BEGIN LMM pbkrtest scalar and F-reference validation harness =====

<TestClass()>
Public Class LMMKenwardRogerPbkrtestScalarAndFReferenceTests

    Private Const ESTIMATE_ABS_TOL As Double = 0.001
    Private Const ORDINARY_SE_ABS_TOL As Double = 0.006
    Private Const KR_SE_ABS_TOL As Double = 0.008
    Private Const DF_ABS_TOL As Double = 0.6
    Private Const T_ABS_TOL As Double = 0.08
    Private Const P_ABS_TOL As Double = 0.015
    Private Const F_ABS_TOL As Double = 0.12
    Private Const F_REL_TOL As Double = 0.002
    Private Const SCALING_ABS_TOL As Double = 0.02

    <TestMethod()>
    Public Sub RandomInterceptLMM_FullKRScalarAndF_MatchesPbkrtestReferences()
        Dim dat As LmmExternalCsvData = LoadRandomInterceptCsv("mixedmodel_lmm_random_intercept_data.csv")
        Dim res As MixedModelResult = FitRandomIntercept(dat)
        AssertUsableFullLmmKrResult(res, expectedP:=2, label:="random-intercept LMM")
        AssertScalarReferences(res, RandomInterceptScalarReferences(), "random-intercept LMM")
        AssertFReferences(res, RandomInterceptFReferences(), "random-intercept LMM")
    End Sub

    <TestMethod()>
    Public Sub UnbalancedSleepstudyRandomSlopeLMM_FullKRScalarAndF_MatchesPbkrtestReferences()
        Dim dat As LmmExternalCsvData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
        Dim res As MixedModelResult = FitRandomInterceptSlope(dat)
        AssertUsableFullLmmKrResult(res, expectedP:=2, label:="unbalanced sleepstudy random-slope LMM")
        AssertScalarReferences(res, UnbalancedSleepstudyScalarReferences(), "unbalanced sleepstudy random-slope LMM")
        AssertFReferences(res, UnbalancedSleepstudyFReferences(), "unbalanced sleepstudy random-slope LMM")
    End Sub

    <TestMethod()>
    Public Sub UnbalancedSleepstudyRandomSlopeLMM_WrapResults_IncludesLmmKrResultTables()
        Dim dat As LmmExternalCsvData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
        Dim res As MixedModelResult = FitRandomInterceptSlope(dat)
        AssertUsableFullLmmKrResult(res, expectedP:=2, label:="unbalanced sleepstudy random-slope LMM")

        Assert.IsNotNull(res.RandomCovarianceUserScale, "LMM result should expose user-scale G covariance matrix.")
        Assert.IsNotNull(res.RandomCorrelationUserScale, "LMM result should expose user-scale G correlation matrix.")
        Assert.IsNotNull(res.RandomEffects, "LMM result should expose BLUP/random-effect dictionary.")
        Assert.IsTrue(res.RandomEffects.Count > 0, "LMM result should contain subject-specific BLUP/random-effect predictions.")

        Dim tables As List(Of ResultTable) = res.wrapResults(alpha:=0.05, includeKenwardRogerTermTests:=True)
        Assert.IsNotNull(tables, "wrapResults should return tables.")

        AssertContainsTableText(tables, "Fixed effects")
        AssertContainsTableText(tables, "Kenward-Roger term-level F tests")
        AssertContainsTableText(tables, "Covariance parameters")
        AssertContainsTableText(tables, "Estimated G covariance matrix")
        AssertContainsTableText(tables, "Estimated G correlation matrix")
        AssertContainsTableText(tables, "Estimated R covariance matrix")
        AssertContainsTableText(tables, "Estimated R correlation matrix")
        AssertContainsTableText(tables, "BLUPs / random effects")
        AssertContainsTableText(tables, "Fit statistics")
        AssertContainsTableText(tables, "Convergence")
    End Sub

    Private Shared Sub AssertUsableFullLmmKrResult(res As MixedModelResult, expectedP As Integer, label As String)
        Assert.IsNotNull(res, label & " result should not be Nothing.")
        Assert.IsTrue(res.Converged, label & " should converge.")
        Assert.AreEqual(expectedP, res.P, label & " fixed-effect dimension.")
        Assert.IsNotNull(res.KenwardRogerWorkspace, label & " should have a KR workspace.")
        Assert.AreEqual(MixedModelKrParameterScale.Covariance, res.KenwardRogerWorkspace.ParameterScale, label & " should use direct covariance-parameter KR scale.")
        Assert.IsNotNull(res.KenwardRogerAdjustedVarBeta, label & " should have KR-adjusted Var(beta).")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Pmats, label & " should have cached P_h matrices.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Rmats, label & " should have conformable R_hj second-derivative matrices, zero for direct covariance scale where appropriate.")
    End Sub

    Private Shared Sub AssertScalarReferences(res As MixedModelResult, refs As List(Of LmmScalarReference), label As String)
        Assert.IsTrue(refs.Count > 0, "Paste scalar pbkrtest references for " & label & ".")
        For Each expected As LmmScalarReference In refs
            Dim l() As Double = BuildRestrictionVector(res.P, expected.CoefficientIndex)
            Dim actual As MixedModelKenwardRogerUnivariateInference = Nothing
            Dim diagnostic As String = Nothing
            Assert.IsTrue(MixedModelKenwardRogerInference.TryUnivariateInference(res, expected.Effect, l, actual, alpha:=0.05, diagnostic:=diagnostic), label & " " & expected.Effect & " KR scalar inference failed: " & diagnostic)
            Assert.IsNotNull(actual, label & " " & expected.Effect & " scalar result should not be Nothing.")
            AssertAlmostEqual(expected.Estimate, actual.Estimate, ESTIMATE_ABS_TOL, label & " " & expected.Effect & " estimate")
            AssertAlmostEqual(expected.OrdinarySE, actual.OrdinaryStdError, ORDINARY_SE_ABS_TOL, label & " " & expected.Effect & " ordinary SE")
            AssertAlmostEqual(expected.KRSE, actual.AdjustedStdError, KR_SE_ABS_TOL, label & " " & expected.Effect & " KR SE")
            AssertAlmostEqual(expected.DF, actual.DF, DF_ABS_TOL, label & " " & expected.Effect & " denominator DF")
            AssertAlmostEqual(expected.TValue, actual.Statistic, T_ABS_TOL, label & " " & expected.Effect & " t statistic")
            AssertAlmostEqual(expected.PValue, actual.PValue, P_ABS_TOL, label & " " & expected.Effect & " p-value")
        Next
    End Sub

    Private Shared Sub AssertFReferences(res As MixedModelResult, refs As List(Of LmmFReference), label As String)
        Assert.IsTrue(refs.Count > 0, "Paste F-test pbkrtest references for " & label & ".")
        For Each expected As LmmFReference In refs
            Dim l(,) As Double = BuildRestrictionMatrix(res.P, expected.CoefficientIndexes)
            Dim actual As MixedModelKenwardRogerMultiDfInference = Nothing
            Dim diagnostic As String = Nothing
            Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(res, expected.Term, l, actual, alpha:=0.05, diagnostic:=diagnostic), label & " " & expected.Term & " KR F test failed: " & diagnostic)
            Assert.IsNotNull(actual, label & " " & expected.Term & " F-test result should not be Nothing.")
            AssertAlmostEqual(expected.NumDF, actual.NumDF, 0.000000001, label & " " & expected.Term & " numerator DF")
            AssertAlmostEqual(expected.DenDF, actual.DenDF, DF_ABS_TOL, label & " " & expected.Term & " denominator DF")
            AssertAlmostEqual(expected.UnscaledF, actual.UnscaledFStatistic, F_ABS_TOL, label & " " & expected.Term & " unscaled F", F_REL_TOL)
            AssertAlmostEqual(expected.Scaling, actual.Scaling, SCALING_ABS_TOL, label & " " & expected.Term & " F scaling")
            AssertAlmostEqual(expected.ScaledF, actual.FStatistic, F_ABS_TOL, label & " " & expected.Term & " scaled F", F_REL_TOL)
            AssertAlmostEqual(expected.PValue, actual.PValue, P_ABS_TOL, label & " " & expected.Term & " p-value")
        Next
    End Sub

    Private Shared Function RandomInterceptScalarReferences() As List(Of LmmScalarReference)
        Return New List(Of LmmScalarReference) From {
            New LmmScalarReference(effect:="(Intercept)", coefficientIndex:=0, estimate:=10.0230555556, ordinarySE:=0.295861169113, krSE:=0.295861169113, df:=5.23561754877, tValue:=33.8775635397, pValue:=0.000000245168941899),
            New LmmScalarReference(effect:="visit", coefficientIndex:=1, estimate:=1.4325, ordinarySE:=0.0447545212409, krSE:=0.0447545212409, df:=11, tValue:=32.0079393161, pValue:=0.00000000000329354172628)
        }
    End Function

    Private Shared Function RandomInterceptFReferences() As List(Of LmmFReference)
        Return New List(Of LmmFReference) From {
            New LmmFReference(term:="visit", coefficientIndexes:=New Integer() {1}, numDF:=1, denDF:=11, unscaledF:=1024.50817926, scaling:=1, scaledF:=1024.50817926, pValue:=0.00000000000329354172628),
            New LmmFReference(term:="all_fixed", coefficientIndexes:=New Integer() {0, 1}, numDF:=2, denDF:=8.47585602611, unscaledF:=1279.40261716, scaling:=0.928086175889, scaledF:=1187.39588239, pValue:=0.0000000000418176755844)
        }
    End Function

    Private Shared Function UnbalancedSleepstudyScalarReferences() As List(Of LmmScalarReference)
        Return New List(Of LmmScalarReference) From {
            New LmmScalarReference(effect:="(Intercept)", coefficientIndex:=0, estimate:=248.834540709, ordinarySE:=6.82866557863, krSE:=6.83613875675, df:=16.8979550501, tValue:=36.3998668786, pValue:=1.7065069458E-17),
            New LmmScalarReference(effect:="days", coefficientIndex:=1, estimate:=11.1253591171, ordinarySE:=1.62187982163, krSE:=1.62280741609, df:=16.9678028731, tValue:=6.85562501553, pValue:=0.00000281948429507)
        }
    End Function

    Private Shared Function UnbalancedSleepstudyFReferences() As List(Of LmmFReference)
        Return New List(Of LmmFReference) From {
            New LmmFReference(term:="days", coefficientIndexes:=New Integer() {1}, numDF:=1, denDF:=16.9678028731, unscaledF:=46.9995943536, scaling:=1, scaledF:=46.9995943536, pValue:=0.00000281948429507),
            New LmmFReference(term:="all_fixed", coefficientIndexes:=New Integer() {0, 1}, numDF:=2, denDF:=15.9353938144, unscaledF:=747.859154449, scaling:=0.941016206185, scaledF:=703.747584281, pValue:=0.000000000000000285255871988)
        }
    End Function

    Private Shared Function FitRandomIntercept(dat As LmmExternalCsvData) As MixedModelResult
        Dim x(dat.Y.Length - 1, 1) As Double
        Dim z(dat.Y.Length - 1, 0) As Double
        For i As Integer = 0 To dat.Y.Length - 1
            x(i, 0) = 1.0
            x(i, 1) = dat.X(i)
            z(i, 0) = 1.0
        Next
        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Y, x:=x, subjectId:=dat.Subject, z:=z, visit:=dat.X, sortWithinSubjectByVisit:=True)
        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateLMM(blockData, New IdentityR(), New RandomIntercept(), MixedModelFitMethod.REML)
        req.RequestLabel = "Random-intercept LMM full KR pbkrtest validation"
        req.ResponseVarName = "y"
        req.SubjectVarName = "subject"
        req.VisitVarName = "visit"
        req.FixedEffectNames = {"(Intercept)", "visit"}
        req.RandomEffectNames = {"(Intercept)"}
        req.EnableFullKenwardRogerForLmm()
        req.Control = TestControl()
        Return (New LMM(req)).Fit()
    End Function

    Private Shared Function FitRandomInterceptSlope(dat As LmmExternalCsvData) As MixedModelResult
        Dim x(dat.Y.Length - 1, 1) As Double
        Dim z(dat.Y.Length - 1, 1) As Double
        For i As Integer = 0 To dat.Y.Length - 1
            x(i, 0) = 1.0
            x(i, 1) = dat.X(i)
            z(i, 0) = 1.0
            z(i, 1) = dat.X(i)
        Next
        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Y, x:=x, subjectId:=dat.Subject, z:=z, visit:=dat.X, sortWithinSubjectByVisit:=True)
        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateLMM(blockData, New IdentityR(), New RandomInterceptSlope(), MixedModelFitMethod.REML)
        req.RequestLabel = "Unbalanced sleepstudy random-slope LMM full KR pbkrtest validation"
        req.ResponseVarName = "reaction"
        req.SubjectVarName = "subject"
        req.VisitVarName = "days"
        req.FixedEffectNames = {"(Intercept)", "days"}
        req.RandomEffectNames = {"(Intercept)", "days"}
        req.EnableFullKenwardRogerForLmm()
        req.Control = TestControl()
        req.StartThetaG = {Math.Log(24.7405), Math.Log(5.9221), Atanh(0.066)}
        req.StartThetaR = {Math.Log(654.941)}
        Return (New LMM(req)).Fit()
    End Function

    Private Shared Function LoadRandomInterceptCsv(fileName As String) As LmmExternalCsvData
        Dim csvPath As String = ResolveTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(csvPath)
        Dim y As New List(Of Double)()
        Dim x As New List(Of Double)()
        Dim subject As New List(Of Object)()
        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
            Dim parts() As String = lines(i).Split(","c)
            subject.Add(parts(0).Trim())
            x.Add(Double.Parse(parts(1), CultureInfo.InvariantCulture))
            y.Add(Double.Parse(parts(2), CultureInfo.InvariantCulture))
        Next
        Return New LmmExternalCsvData With {.Y = y.ToArray(), .X = x.ToArray(), .Subject = subject.ToArray()}
    End Function

    Private Shared Function LoadSleepstudyCsv(fileName As String) As LmmExternalCsvData
        Dim csvPath As String = ResolveTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(csvPath)
        Dim y As New List(Of Double)()
        Dim x As New List(Of Double)()
        Dim subject As New List(Of Object)()
        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
            Dim parts() As String = lines(i).Split(","c)
            y.Add(Double.Parse(parts(0), CultureInfo.InvariantCulture))
            x.Add(Double.Parse(parts(1), CultureInfo.InvariantCulture))
            subject.Add(parts(2).Trim())
        Next
        Return New LmmExternalCsvData With {.Y = y.ToArray(), .X = x.ToArray(), .Subject = subject.ToArray()}
    End Function

    Private Shared Function ResolveTestDataPath(fileName As String) As String
        Dim checkedPaths As New List(Of String)()

        Dim addCandidate As Action(Of String) =
            Sub(path As String)
                If String.IsNullOrWhiteSpace(path) Then Exit Sub
                If Not checkedPaths.Contains(path) Then checkedPaths.Add(path)
            End Sub

        addCandidate(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", fileName))
        addCandidate(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName))

        AddParentDirectoryCandidates(AppDomain.CurrentDomain.BaseDirectory, fileName, checkedPaths)
        AddParentDirectoryCandidates(Directory.GetCurrentDirectory(), fileName, checkedPaths)

        For Each path As String In checkedPaths
            If File.Exists(path) Then Return path
        Next

        Throw New FileNotFoundException("Could not find test data file. Checked: " & String.Join(" | ", checkedPaths.ToArray()), fileName)
    End Function


    Private Shared Sub AddParentDirectoryCandidates(startPath As String,
                                                     fileName As String,
                                                     checkedPaths As List(Of String))
        If String.IsNullOrWhiteSpace(startPath) OrElse checkedPaths Is Nothing Then Exit Sub

        Dim dir As DirectoryInfo = Nothing
        Try
            dir = New DirectoryInfo(startPath)
        Catch
            Return
        End Try

        For depth As Integer = 0 To 8
            If dir Is Nothing Then Exit For

            Dim candidate As String = System.IO.Path.Combine(dir.FullName, "TestData", fileName)
            If Not checkedPaths.Contains(candidate) Then checkedPaths.Add(candidate)

            dir = dir.Parent
        Next
    End Sub


    Private Shared Function BuildRestrictionVector(p As Integer, index As Integer) As Double()
        Dim l(p - 1) As Double
        l(index) = 1.0
        Return l
    End Function

    Private Shared Function BuildRestrictionMatrix(p As Integer, indexes() As Integer) As Double(,)
        Dim l(indexes.Length - 1, p - 1) As Double
        For r As Integer = 0 To indexes.Length - 1
            l(r, indexes(r)) = 1.0
        Next
        Return l
    End Function

    Private Shared Function TestControl() As MixedModelControl
        Return New MixedModelControl With {.MaxIter = 250, .Epsilon = 0.00000001, .StepTolerance = 0.00000001, .FunctionTolerance = 0.0000000001, .Trace = False}
    End Function

    Private Shared Function Atanh(x As Double) As Double
        Return 0.5 * Math.Log((1.0 + x) / (1.0 - x))
    End Function

    Private Shared Sub AssertContainsTableText(tables As List(Of ResultTable), expectedText As String)
        Assert.IsNotNull(tables, "Table list should not be Nothing.")
        For Each table As ResultTable In tables
            If table Is Nothing Then Continue For
            Dim arr(,) As Object = table.returnSelf()
            If TableContainsText(arr, expectedText) Then Exit Sub
        Next
        Assert.Fail("Expected wrapped result tables to contain text: " & expectedText)
    End Sub

    Private Shared Function TableContainsText(table(,) As Object, expectedText As String) As Boolean
        If table Is Nothing Then Return False
        For i As Integer = 0 To table.GetLength(0) - 1
            For j As Integer = 0 To table.GetLength(1) - 1
                Dim s As String = Convert.ToString(table(i, j), CultureInfo.InvariantCulture)
                If String.Equals(s, expectedText, StringComparison.OrdinalIgnoreCase) Then Return True
            Next
        Next
        Return False
    End Function

    Private Shared Sub AssertAlmostEqual(expected As Double,
                                         actual As Double,
                                         absoluteTolerance As Double,
                                         label As String,
                                         Optional relativeTolerance As Double = 0.0)
        If Double.IsNaN(expected) OrElse Double.IsInfinity(expected) Then Assert.Fail(label & ": R reference constant has not been filled in.")
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then Assert.Fail(label & ": actual value is not finite. Expected " & expected.ToString("G17", CultureInfo.InvariantCulture) & ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) & ".")

        Dim diff As Double = Math.Abs(expected - actual)
        Dim allowed As Double = Math.Max(absoluteTolerance, Math.Abs(expected) * relativeTolerance)
        If diff > allowed Then Assert.Fail(label & ": expected " & expected.ToString("G17", CultureInfo.InvariantCulture) & ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) & ", abs diff " & diff.ToString("G17", CultureInfo.InvariantCulture) & " > allowed tolerance " & allowed.ToString("G17", CultureInfo.InvariantCulture) & " (abs=" & absoluteTolerance.ToString("G17", CultureInfo.InvariantCulture) & ", rel=" & relativeTolerance.ToString("G17", CultureInfo.InvariantCulture) & ").")
    End Sub

    Private Class LmmExternalCsvData
        Public Property Y As Double()
        Public Property X As Double()
        Public Property Subject As Object()
    End Class

    Private Class LmmScalarReference
        Public ReadOnly Effect As String
        Public ReadOnly CoefficientIndex As Integer
        Public ReadOnly Estimate As Double
        Public ReadOnly OrdinarySE As Double
        Public ReadOnly KRSE As Double
        Public ReadOnly DF As Double
        Public ReadOnly TValue As Double
        Public ReadOnly PValue As Double
        Public Sub New(effect As String, coefficientIndex As Integer, estimate As Double, ordinarySE As Double, krSE As Double, df As Double, tValue As Double, pValue As Double)
            Me.Effect = effect
            Me.CoefficientIndex = coefficientIndex
            Me.Estimate = estimate
            Me.OrdinarySE = ordinarySE
            Me.KRSE = krSE
            Me.DF = df
            Me.TValue = tValue
            Me.PValue = pValue
        End Sub
    End Class

    Private Class LmmFReference
        Public ReadOnly Term As String
        Public ReadOnly CoefficientIndexes() As Integer
        Public ReadOnly NumDF As Double
        Public ReadOnly DenDF As Double
        Public ReadOnly UnscaledF As Double
        Public ReadOnly Scaling As Double
        Public ReadOnly ScaledF As Double
        Public ReadOnly PValue As Double
        Public Sub New(term As String, coefficientIndexes() As Integer, numDF As Double, denDF As Double, unscaledF As Double, scaling As Double, scaledF As Double, pValue As Double)
            Me.Term = term
            Me.CoefficientIndexes = coefficientIndexes
            Me.NumDF = numDF
            Me.DenDF = denDF
            Me.UnscaledF = unscaledF
            Me.Scaling = scaling
            Me.ScaledF = scaledF
            Me.PValue = pValue
        End Sub
    End Class

End Class

' ===== END LMM pbkrtest scalar and F-reference validation harness =====
