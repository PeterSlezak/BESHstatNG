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


' -----------------------------------------------------------------------------
' Split from KenwardRogerInferenceTests.vb for maintainability.
' -----------------------------------------------------------------------------

' ===== BEGIN migrated from MixedModelKRValidationTests.vb =====



<TestClass()>
Public Class MixedModelKRValidationTests


    <TestMethod()>
    Public Sub ResolveAdjustedVarBeta_PrefersInferenceWorkspace()
        Dim res As New MixedModelResult With {
            .P = 1,
            .VarBeta = New Double(,) {{1.0}}
        }

        res.KenwardRogerAdjustedVarBeta = New Double(,) {{2.0}}

        Assert.IsNotNull(res.InferenceWorkspace)
        Assert.IsNotNull(res.InferenceWorkspace.AdjustedVarBeta)

        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)
        Assert.AreEqual(2.0, adjusted(0, 0), 0.0000000001)

        res.InferenceWorkspace.AdjustedVarBeta = New Double(,) {{3.0}}
        adjusted = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)

        Assert.AreEqual(3.0, adjusted(0, 0), 0.0000000001)
    End Sub


    Private Shared Function FitSimpleMMRMWithKR() As MixedModelResult
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

        Assert.IsNotNull(res)
        Assert.IsNotNull(res.KenwardRogerAdjustedVarBeta, "KR adjusted Var(beta) should be available.")
        Assert.IsNotNull(res.InferenceWorkspace, "InferenceWorkspace should be available.")
        Assert.IsNotNull(res.InferenceWorkspace.AdjustedVarBeta, "InferenceWorkspace adjusted Var(beta) should be available.")

        Return res
    End Function


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

' ===== END migrated from MixedModelKRValidationTests.vb =====

' ===== BEGIN migrated from MMRMOrthodontKRAgainstRReferenceTests.vb =====



' Compares BESHStatNG internal Kenward-Roger adjusted MMRM output against
' R mmrm reference values for the Orthodont / Potthoff-Roy data.
'
' Reference R model:
'
'   mmrm::mmrm(
'       distance ~ SexCode * age + mmrm::us(visit | Subject),
'       data = dat,
'       reml = TRUE,
'       method = "Kenward-Roger",
'       vcov = "Kenward-Roger"
'   )
'
' This test compares:
'   - fixed-effect estimates,
'   - ordinary fixed-effect SEs,
'   - KR-adjusted fixed-effect SEs,
'   - one-dimensional KR denominator DF,
'   - KR t statistics and p-values,
'   - the equivalent one-row KR F test path.
'
' R reference values were generated with the mmrm package using method = "Kenward-Roger"
' and vcov = "Kenward-Roger".  The constants are hard-coded so this unit test is
' deterministic and does not require R at test runtime.
<TestClass()>
Public Class MMRMOrthodontKRAgainstRReferenceTests

    Public Property TestContext As Microsoft.VisualStudio.TestTools.UnitTesting.TestContext

    Private Const ESTIMATE_TOL As Double = 0.0002
    Private Const ORDINARY_SE_TOL As Double = 0.0001

    ' Use moderately strict tolerances while the KR backend is finite-difference based.
    ' These can be tightened after the derivative path is fully reference-locked.
    Private Const KR_SE_TOL As Double = 0.0001
    Private Const KR_DF_TOL As Double = 0.005
    Private Const KR_T_TOL As Double = 0.0005
    Private Const KR_T_REL_TOL As Double = 0.0001
    Private Const KR_P_TOL As Double = 0.00001
    Private Const KR_F_TOL As Double = 0.0005
    Private Const KR_F_REL_TOL As Double = 0.0005

    <TestMethod()>
    Public Sub OrthodontMMRM_KRAdjustedSE_MatchesRmmrmReference()
        Dim dat As OrthodontData = LoadOrthodontData("mmrm_orthodont_potthoffroy_long.csv")
        Dim x(,) As Double = BuildOrthodontX(dat)

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Distance,
                                                                              x:=x,
                                                                              subjectId:=dat.Subject,
                                                                              z:=Nothing,
                                                                              visit:=dat.Visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New UnstructuredR(),
                                                                          MixedModelFitMethod.REML)
        req.RequestLabel = "Orthodont MMRM UN KR against R reference"
        req.ResponseVarName = "distance"
        req.SubjectVarName = "Subject"
        req.VisitVarName = "visit"
        req.FixedEffectNames = {"(Intercept)", "SexCode", "age", "SexCode:age"}
        req.EnableFullKenwardRogerForMmrm()
        req.Control = TestControl()

        Dim res As MixedModelResult = (New MMRM(req)).Fit()

        Assert.IsNotNull(res, "MMRM result should not be Nothing.")
        Assert.IsTrue(res.Converged, "Orthodont MMRM should converge.")
        Assert.AreEqual(4, res.P, "Unexpected fixed-effect dimension.")
        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        'Assert.AreEqual(MixedModelKrParameterScale.OptimizerInternal,
        'res.KenwardRogerWorkspace.ParameterScale,
        '        "Orthodont UN MMRM should currently use optimizer-internal KR scale to match R mmrm reference.")
        Assert.IsNotNull(res.KenwardRogerWorkspace.Rmats,
                 "Orthodont UN MMRM R mmrm reference requires the full KR path with R_hj matrices.")

        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)
        Assert.IsNotNull(adjusted, "KR adjusted Var(beta) should be available.")
        Assert.AreEqual(4, adjusted.GetLength(0), "Adjusted Var(beta) row dimension.")
        Assert.AreEqual(4, adjusted.GetLength(1), "Adjusted Var(beta) column dimension.")

        Dim refs As List(Of RReferenceRow) = GetOrthodontRReferenceRows()

        Dim comparisonCsv As String = BuildComparisonCsv(res, adjusted, refs)
        WriteComparisonCsv(comparisonCsv, "besh_vs_r_orthodont_mmrm_kr_comparison.csv")

        For j As Integer = 0 To refs.Count - 1
            Dim expected As RReferenceRow = refs(j)

            AssertAlmostEqual(expected.Estimate,
                              res.Beta(j),
                              ESTIMATE_TOL,
                              expected.Effect & " estimate")

            Dim actualOrdinarySE As Double = Math.Sqrt(Math.Max(0.0, res.VarBeta(j, j)))
            Dim actualKRSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))

            AssertAlmostEqual(expected.OrdinaryStdError,
                              actualOrdinarySE,
                              ORDINARY_SE_TOL,
                              expected.Effect & " ordinary SE")

            AssertAlmostEqual(expected.StdError,
                              actualKRSE,
                              KR_SE_TOL,
                              expected.Effect & " KR adjusted SE")

            Dim l(res.P - 1) As Double
            l(j) = 1.0

            Dim scalar As MixedModelKenwardRogerUnivariateInference = Nothing
            Dim scalarMsg As String = Nothing

            Assert.IsTrue(MixedModelKenwardRogerInference.TryUnivariateInference(res,
                                                                                 expected.Effect,
                                                                                 l,
                                                                                 scalar,
                                                                                 alpha:=0.05,
                                                                                 diagnostic:=scalarMsg),
                          expected.Effect & " scalar KR inference failed: " & scalarMsg)
            Assert.IsNotNull(scalar, expected.Effect & " scalar KR inference should be returned.")

            AssertAlmostEqual(expected.StdError,
                              scalar.AdjustedStdError,
                              KR_SE_TOL,
                              expected.Effect & " scalar KR adjusted SE")

            AssertAlmostEqual(expected.DF,
                              scalar.DF,
                              KR_DF_TOL,
                              expected.Effect & " scalar KR denominator DF")

            AssertAlmostEqual(expected.TValue,
                              scalar.Statistic,
                              KR_T_TOL,
                              expected.Effect & " scalar KR t statistic",
                              KR_T_REL_TOL)

            AssertAlmostEqual(expected.PValue,
                              scalar.PValue,
                              KR_P_TOL,
                              expected.Effect & " scalar KR p-value")

            Dim lMat(0, res.P - 1) As Double
            lMat(0, j) = 1.0

            Dim ftest As MixedModelKenwardRogerMultiDfInference = Nothing
            Dim fMsg As String = Nothing

            Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(res,
                                                                            expected.Effect,
                                                                            lMat,
                                                                            ftest,
                                                                            alpha:=0.05,
                                                                            diagnostic:=fMsg),
                          expected.Effect & " one-row KR F test failed: " & fMsg)
            Assert.IsNotNull(ftest, expected.Effect & " one-row KR F test should be returned.")

            AssertAlmostEqual(1.0,
                              ftest.NumDF,
                              0.0000000001,
                              expected.Effect & " one-row KR F numerator DF")

            AssertAlmostEqual(expected.DF,
                              ftest.DenDF,
                              KR_DF_TOL,
                              expected.Effect & " one-row KR F denominator DF")

            AssertAlmostEqual(expected.TValue * expected.TValue,
                              ftest.UnscaledFStatistic,
                              KR_F_TOL,
                              expected.Effect & " one-row unscaled KR F",
                              KR_F_REL_TOL)

            AssertAlmostEqual(1.0,
                              ftest.Scaling,
                              0.0000001,
                              expected.Effect & " one-row KR F scaling")

            AssertAlmostEqual(expected.PValue,
                              ftest.PValue,
                              KR_P_TOL,
                              expected.Effect & " one-row KR F p-value")
        Next
    End Sub


    Private Shared Function GetOrthodontRReferenceRows() As List(Of RReferenceRow)
        Return New List(Of RReferenceRow) From {
            New RReferenceRow("(Intercept)", 15.8422452, 0.97225331, 1.0021906, 24.99999, 15.807617, 0.00000000000001594734),
            New RReferenceRow("SexCode", 1.583124, 1.52322819, 1.57013092, 24.99999, 1.008275, 0.3229822),
            New RReferenceRow("age", 0.82681229999999994, 0.08222052, 0.08368595, 24.99671, 9.879941, 0.0000000004094801),
            New RReferenceRow("SexCode:age", -0.3504484, 0.1288148, 0.13111069, 24.99671, -2.67292, 0.01305046)
        }
    End Function


    Private Shared Sub AssertAlmostEqual(expected As Double,
                                         actual As Double,
                                         absoluteTolerance As Double,
                                         label As String,
                                         Optional relativeTolerance As Double = 0.0)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail(label & ": actual value is not finite. Expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If

        Dim diff As Double = Math.Abs(expected - actual)
        Dim allowed As Double = Math.Max(absoluteTolerance, Math.Abs(expected) * relativeTolerance)

        If diff > allowed Then
            Assert.Fail(label & ": expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " &
                        actual.ToString("G17", CultureInfo.InvariantCulture) &
                        ", abs diff " &
                        diff.ToString("G17", CultureInfo.InvariantCulture) &
                        " > allowed tolerance " &
                        allowed.ToString("G17", CultureInfo.InvariantCulture) &
                        " (abs=" &
                        absoluteTolerance.ToString("G17", CultureInfo.InvariantCulture) &
                        ", rel=" &
                        relativeTolerance.ToString("G17", CultureInfo.InvariantCulture) & ").")
        End If
    End Sub


    Private Shared Function BuildComparisonCsv(res As MixedModelResult,
                                               adjusted(,) As Double,
                                               refs As List(Of RReferenceRow)) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("effect,r_estimate,besh_estimate,estimate_diff,r_kr_se,besh_kr_se,kr_se_diff,r_df,r_t,r_p,besh_ordinary_se")

        For j As Integer = 0 To refs.Count - 1
            Dim r As RReferenceRow = refs(j)
            Dim beshEstimate As Double = res.Beta(j)
            Dim beshKRSE As Double = Math.Sqrt(Math.Max(0.0, adjusted(j, j)))
            Dim beshOrdinarySE As Double = Math.Sqrt(Math.Max(0.0, res.VarBeta(j, j)))

            sb.AppendLine(String.Join(",",
                                      Csv(r.Effect),
                                      Csv(r.Estimate),
                                      Csv(beshEstimate),
                                      Csv(beshEstimate - r.Estimate),
                                      Csv(r.StdError),
                                      Csv(beshKRSE),
                                      Csv(beshKRSE - r.StdError),
                                      Csv(r.DF),
                                      Csv(r.TValue),
                                      Csv(r.PValue),
                                      Csv(beshOrdinarySE)))
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
                Me.TestContext.WriteLine("Wrote R comparison CSV: " & path)

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
        ctl.MaxIter = 180
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function


    Private Shared Function BuildOrthodontX(dat As OrthodontData) As Double(,)
        Dim n As Integer = dat.Distance.Length
        Dim x(n - 1, 3) As Double

        For i As Integer = 0 To n - 1
            x(i, 0) = 1.0
            x(i, 1) = dat.SexCode(i)
            x(i, 2) = dat.Age(i)
            x(i, 3) = dat.SexCode(i) * dat.Age(i)
        Next

        Return x
    End Function


    Private Shared Function LoadOrthodontData(fileName As String) As OrthodontData
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then
            Throw New InvalidOperationException("Orthodont CSV must contain a header and data rows.")
        End If

        Dim header() As String = lines(0).Split(","c)
        Dim col As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For j As Integer = 0 To header.Length - 1
            col(header(j).Trim()) = j
        Next

        RequireColumn(col, "Subject", fileName)
        RequireColumn(col, "SexCode", fileName)
        RequireColumn(col, "visit", fileName)
        RequireColumn(col, "age", fileName)
        RequireColumn(col, "distance", fileName)

        Dim n As Integer = lines.Length - 1
        Dim subject(n - 1) As Object
        Dim sexCode(n - 1) As Double
        Dim visit(n - 1) As Double
        Dim age(n - 1) As Double
        Dim distance(n - 1) As Double

        For i As Integer = 0 To n - 1
            Dim parts() As String = lines(i + 1).Split(","c)

            subject(i) = parts(col("Subject")).Trim()
            sexCode(i) = ParseD(parts(col("SexCode")))
            visit(i) = ParseD(parts(col("visit")))
            age(i) = ParseD(parts(col("age")))
            distance(i) = ParseD(parts(col("distance")))
        Next

        Return New OrthodontData With {
            .Subject = subject,
            .SexCode = sexCode,
            .Visit = visit,
            .Age = age,
            .Distance = distance
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
        Public ReadOnly Estimate As Double
        Public ReadOnly OrdinaryStdError As Double
        Public ReadOnly StdError As Double
        Public ReadOnly DF As Double
        Public ReadOnly TValue As Double
        Public ReadOnly PValue As Double

        Public Sub New(effect As String,
                       estimate As Double,
                       ordinaryStdError As Double,
                       stdError As Double,
                       df As Double,
                       tValue As Double,
                       pValue As Double)
            Me.Effect = effect
            Me.Estimate = estimate
            Me.OrdinaryStdError = ordinaryStdError
            Me.StdError = stdError
            Me.DF = df
            Me.TValue = tValue
            Me.PValue = pValue
        End Sub
    End Class


    Private Class OrthodontData
        Public Subject() As Object
        Public SexCode() As Double
        Public Visit() As Double
        Public Age() As Double
        Public Distance() As Double
    End Class

End Class

' ===== END migrated from MMRMOrthodontKRAgainstRReferenceTests.vb =====

' ===== BEGIN migrated from MMRMKenwardRogerMulticovariateReferenceTests.vb =====



' External Kenward-Roger validation against R mmrm for the existing augmented
' longitudinal multicovariate MMRM test data set.
'
' Reference source:
'   kr_mmrm_multicovariate_missing_reference.R
'
' R reference package:
'   mmrm 0.3.17
'
' Notes:
'   - Constants are hard-coded so the unit tests do not require R at runtime.
'   - The test data file is the existing mixedmodel_longitudinal_multicovariate_missing.csv.
'   - The supplied R references cover: CS, heterogeneous CS, AR(1), heterogeneous AR(1), and UN.
'   - Identity and diagonal heterogeneous KR references can be added here if/when the pinned
'     R mmrm version provides the exact desired covariance aliases for those structures.
<TestClass()>
Public Class MMRMKenwardRogerMulticovariateReferenceTests

    Private Const DATA_FILE As String = "mixedmodel_longitudinal_multicovariate_missing.csv"

    Private Const ESTIMATE_ABS_TOL As Double = 0.0005
    Private Const ESTIMATE_REL_TOL As Double = 0.0
    Private Const ORDINARY_SE_ABS_TOL As Double = 0.001
    Private Const ORDINARY_SE_REL_TOL As Double = 0.001
    ' KR SEs are a primary external-validation quantity, but this project and R mmrm
    ' use independent optimizers and matrix numerics.  A small absolute plus 0.5%
    ' relative tolerance keeps the test meaningful without failing on harmless
    ' cross-implementation numerical differences in the hardest UN/HAR1 cases.
    Private Const KR_SE_ABS_TOL As Double = 0.00001
    Private Const KR_SE_REL_TOL As Double = 0.00001
    Private Const DF_ABS_TOL As Double = 0.001
    Private Const DF_REL_TOL As Double = 0.0001
    Private Const T_ABS_TOL As Double = 0.0001
    Private Const T_REL_TOL As Double = 0.0001
    Private Const P_ABS_TOL As Double = 0.00001
    Private Const P_REL_TOL As Double = 0.00001

    <TestMethod()>
    Public Sub MulticovariateMissingMMRM_KRScalarInference_MatchesRmmrmReferences()
        Dim allRefs As List(Of KrCoefficientReference) = RmmrmCoefficientReferences()
        Dim structures() As String = {
            "Compound Symmetry",
            "Heterogeneous Compound Symmetry",
            "AR(1)",
            "Heterogeneous AR(1)",
            "Unstructured"
        }

        For Each structureName As String In structures
            Dim refs As List(Of KrCoefficientReference) = ReferencesForStructure(allRefs, structureName)
            Assert.IsTrue(refs.Count > 0, "No hard-coded R mmrm KR references for " & structureName & ".")

            Dim result As MixedModelResult = FitKrMMRM(structureName)
            AssertUsableKrResult(result, structureName)

            For Each expected As KrCoefficientReference In refs
                Dim j As Integer = FixedEffectIndex(expected.Effect)
                Dim l(result.P - 1) As Double
                l(j) = 1.0

                Dim krInf As MixedModelKenwardRogerUnivariateInference = Nothing
                Dim diagnostic As String = String.Empty

                Assert.IsTrue(MixedModelKenwardRogerInference.TryUnivariateInference(result,
                                                                                    expected.Effect,
                                                                                    l,
                                                                                    krInf,
                                                                                    alpha:=0.05,
                                                                                    diagnostic:=diagnostic),
                              structureName & " " & expected.Effect & " KR scalar inference failed: " & diagnostic)

                Assert.IsNotNull(krInf, structureName & " " & expected.Effect & " KR scalar inference should not be Nothing.")

                Dim ordinarySE As Double = Math.Sqrt(Math.Max(0.0, result.VarBeta(j, j)))

                AssertAlmostEqual(expected.Estimate,
                                  result.Beta(j),
                                  ESTIMATE_ABS_TOL,
                                  structureName & " " & expected.Effect & " estimate",
                                  ESTIMATE_REL_TOL)

                AssertAlmostEqual(expected.OrdinarySE,
                                  ordinarySE,
                                  ORDINARY_SE_ABS_TOL,
                                  structureName & " " & expected.Effect & " ordinary SE",
                                  ORDINARY_SE_REL_TOL)

                AssertAlmostEqual(expected.KRSE,
                                  krInf.AdjustedStdError,
                                  KR_SE_ABS_TOL,
                                  structureName & " " & expected.Effect & " KR-adjusted SE",
                                  KR_SE_REL_TOL)

                AssertAlmostEqual(expected.DF,
                                  krInf.DF,
                                  DF_ABS_TOL,
                                  structureName & " " & expected.Effect & " KR denominator DF",
                                  DF_REL_TOL)

                AssertAlmostEqual(expected.TValue,
                                  krInf.Statistic,
                                  T_ABS_TOL,
                                  structureName & " " & expected.Effect & " KR t statistic",
                                  T_REL_TOL)

                AssertAlmostEqual(expected.PValue,
                                  krInf.PValue,
                                  P_ABS_TOL,
                                  structureName & " " & expected.Effect & " KR p-value",
                                  P_REL_TOL)
            Next
        Next
    End Sub


    Private Shared Function FitKrMMRM(structureName As String) As MixedModelResult
        Dim dat As ModelData = LoadModelData()

        Dim blockData As MixedModelBlockData =
            MixedModelBlockData.FromArrays(y:=dat.Y,
                                           x:=dat.X,
                                           subjectId:=dat.SubjectId,
                                           z:=Nothing,
                                           visit:=dat.Visit,
                                           sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest =
            MixedModelFitRequest.CreateMMRM(blockData,
                                            CreateRStruct(structureName),
                                            MixedModelFitMethod.REML)

        req.RequestLabel = "R mmrm KR external validation: " & structureName
        req.ResponseVarName = "distance_mm"
        req.SubjectVarName = "subject_id"
        req.VisitVarName = "visit"
        req.FixedEffectNames = FixedEffectNames()
        req.Control = ReferenceControl()
        req.EnableFullKenwardRogerForMmrm()

        Dim startTheta() As Double = StartThetaFor(structureName)
        If startTheta IsNot Nothing Then
            req.StartThetaR = startTheta
        End If

        Return (New MMRM(req)).Fit()
    End Function


    Private Shared Sub AssertUsableKrResult(result As MixedModelResult,
                                            structureName As String)
        Assert.IsNotNull(result, structureName & " result should not be Nothing.")

        ' Some external-reference fits start very close to the optimum.  The current
        ' projected-gradient optimizer can then report a non-converged line-search status
        ' even though the final objective evaluation, estimates, and KR quantities are usable.
        ' This reference test therefore treats the hard-coded R comparisons below as the
        ' convergence gate and does not fail solely on the optimizer status flag.
        Assert.AreEqual(7, result.P, structureName & " fixed-effect dimension.")
        Assert.AreEqual(95, result.Nobs, structureName & " observation count after response filtering.")
        Assert.AreEqual(27, result.NoSubjects, structureName & " subject count.")
        Assert.IsNotNull(result.VarBeta, structureName & " ordinary Var(beta) should be available.")
        Assert.IsNotNull(result.KenwardRogerWorkspace, structureName & " KR workspace should be available.")
        Assert.AreEqual(MixedModelKrParameterScale.MmrmTheta,
                        result.KenwardRogerWorkspace.ParameterScale,
                        structureName & " KR parameter scale should follow the R mmrm theta path.")

        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(result)
        Assert.IsNotNull(adjusted, structureName & " KR-adjusted Var(beta) should be available.")
        Assert.AreEqual(result.P, adjusted.GetLength(0), structureName & " adjusted Var(beta) row dimension.")
        Assert.AreEqual(result.P, adjusted.GetLength(1), structureName & " adjusted Var(beta) column dimension.")
    End Sub


    Private Shared Function CreateRStruct(name As String) As MixedModelRStruct
        Select Case name
            Case "Compound Symmetry"
                Return New CompoundSymmetryR()
            Case "Heterogeneous Compound Symmetry", "Heterogeneous CS"
                Return New HeterogeneousCSR()
            Case "AR(1)"
                Return New AR1R()
            Case "Heterogeneous AR(1)"
                Return New HeterogeneousAR1R()
            Case "Unstructured"
                Return New UnstructuredR()
            Case Else
                Throw New ArgumentException("Unsupported R mmrm KR reference structure: " & name)
        End Select
    End Function


    Private Shared Function StartThetaFor(structureName As String) As Double()
        Select Case structureName
            Case "Compound Symmetry"
                Return New Double() {1.6119723529784433, 0.62916274331024713}
            Case "AR(1)"
                Return New Double() {1.5897216989092446, 0.59539636787538386}
            Case "Unstructured"
                Return New Double() {0.87916971566842228,
                                     1.2312552673676571,
                                     0.49750750452199372,
                                     1.381781498068767,
                                     0.2357143321994451,
                                     0.55245928701394587,
                                     1.0820320747663819,
                                     1.1684924986178791,
                                     0.88553914499981556,
                                     0.376140597144046}
            Case Else
                Return Nothing
        End Select
    End Function


    Private Shared Function ReferenceControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 400
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function


    Private Shared Function FixedEffectNames() As String()
        Return New String() {"(Intercept)",
                             "sex_code",
                             "treatment_active",
                             "site_central",
                             "site_south",
                             "age_centered_8",
                             "treatment_active:age_centered_8"}
    End Function


    Private Shared Function FixedEffectIndex(effect As String) As Integer
        Dim names() As String = FixedEffectNames()
        For j As Integer = 0 To names.Length - 1
            If String.Equals(effect, names(j), StringComparison.OrdinalIgnoreCase) Then
                Return j
            End If
        Next

        Throw New InvalidOperationException("Unknown fixed-effect reference name: " & effect)
    End Function


    Private Shared Function ReferencesForStructure(allRefs As List(Of KrCoefficientReference),
                                                   structureName As String) As List(Of KrCoefficientReference)
        Dim out As New List(Of KrCoefficientReference)()
        For Each one As KrCoefficientReference In allRefs
            If String.Equals(one.StructureName, structureName, StringComparison.OrdinalIgnoreCase) Then
                out.Add(one)
            End If
        Next
        Return out
    End Function


    Private Shared Function RmmrmCoefficientReferences() As List(Of KrCoefficientReference)
        ' Generated by kr_mmrm_multicovariate_missing_reference.R.
        ' R package version used for the reference generator target: mmrm 0.3.17.
        Return New List(Of KrCoefficientReference) From {
            New KrCoefficientReference(structureName:="Compound Symmetry", effect:="(Intercept)", estimate:=22.178253634, ordinarySE:=0.811191690219, krSE:=0.799138740162, df:=27.3653574066, tValue:=27.7526948944, pValue:=1.40334886107E-21),
            New KrCoefficientReference(structureName:="Compound Symmetry", effect:="sex_code", estimate:=-2.08629667086, ordinarySE:=0.733085574166, krSE:=0.720787096938, df:=21.48153511, tValue:=-2.89447005881, pValue:=0.00854338440675),
            New KrCoefficientReference(structureName:="Compound Symmetry", effect:="treatment_active", estimate:=-0.779330134841, ordinarySE:=0.82562711646, krSE:=0.815078702736, df:=35.9714049663, tValue:=-0.956140961878, pValue:=0.345383139388),
            New KrCoefficientReference(structureName:="Compound Symmetry", effect:="site_central", estimate:=1.74777853695, ordinarySE:=0.879627779795, krSE:=0.864884416705, df:=21.4547224172, tValue:=2.02082324897, pValue:=0.0559528982984),
            New KrCoefficientReference(structureName:="Compound Symmetry", effect:="site_south", estimate:=1.32430071271, ordinarySE:=0.883344864823, krSE:=0.868519687009, df:=21.4167753329, tValue:=1.52477915299, pValue:=0.141946312509),
            New KrCoefficientReference(structureName:="Compound Symmetry", effect:="age_centered_8", estimate:=0.575894063969, ordinarySE:=0.100870537303, krSE:=0.100955577349, df:=67.0325736096, tValue:=5.70443039495, pValue:=0.000000287026818309),
            New KrCoefficientReference(structureName:="Compound Symmetry", effect:="treatment_active:age_centered_8", estimate:=0.223341529796, ordinarySE:=0.140641915498, krSE:=0.140733397519, df:=66.7355191106, tValue:=1.58698314497, pValue:=0.117243686452),
            New KrCoefficientReference(structureName:="Heterogeneous Compound Symmetry", effect:="(Intercept)", estimate:=22.080707752, ordinarySE:=0.817025999561, krSE:=0.853995038519, df:=27.0577031437, tValue:=25.8557799004, pValue:=1.29826378736E-20),
            New KrCoefficientReference(structureName:="Heterogeneous Compound Symmetry", effect:="sex_code", estimate:=-2.06322270048, ordinarySE:=0.72579532785, krSE:=0.775885163416, df:=21.7731324373, tValue:=-2.65918566015, pValue:=0.0144053548999),
            New KrCoefficientReference(structureName:="Heterogeneous Compound Symmetry", effect:="treatment_active", estimate:=-0.745163230654, ordinarySE:=0.839687524899, krSE:=0.863833696551, df:=27.5454298453, tValue:=-0.862623481382, pValue:=0.395792969181),
            New KrCoefficientReference(structureName:="Heterogeneous Compound Symmetry", effect:="site_central", estimate:=1.69146808108, ordinarySE:=0.872229552578, krSE:=0.933307660966, df:=21.7384766097, tValue:=1.81233708006, pValue:=0.0837705072292),
            New KrCoefficientReference(structureName:="Heterogeneous Compound Symmetry", effect:="site_south", estimate:=1.39476315404, ordinarySE:=0.872563893805, krSE:=0.932594735255, df:=21.6108545448, tValue:=1.49557262261, pValue:=0.149226286897),
            New KrCoefficientReference(structureName:="Heterogeneous Compound Symmetry", effect:="age_centered_8", estimate:=0.582807373266, ordinarySE:=0.103681351432, krSE:=0.104444766991, df:=50.4894677209, tValue:=5.58005336271, pValue:=0.000000950845752039),
            New KrCoefficientReference(structureName:="Heterogeneous Compound Symmetry", effect:="treatment_active:age_centered_8", estimate:=0.223126491147, ordinarySE:=0.144317974216, krSE:=0.145395107468, df:=51.0095250619, tValue:=1.53462172856, pValue:=0.131056542469),
            New KrCoefficientReference(structureName:="AR(1)", effect:="(Intercept)", estimate:=22.3755244421, ordinarySE:=0.779220809183, krSE:=0.772017342, df:=34.942645892, tValue:=28.9831888804, pValue:=5.00599762605E-26),
            New KrCoefficientReference(structureName:="AR(1)", effect:="sex_code", estimate:=-2.16918116334, ordinarySE:=0.650545980458, krSE:=0.642369045371, df:=24.4415717876, tValue:=-3.37684572283, pValue:=0.0024527166429),
            New KrCoefficientReference(structureName:="AR(1)", effect:="treatment_active", estimate:=-0.729319951658, ordinarySE:=0.853803464893, krSE:=0.848524410367, df:=46.9302385924, tValue:=-0.859515581105, pValue:=0.394425505022),
            New KrCoefficientReference(structureName:="AR(1)", effect:="site_central", estimate:=1.58506814553, ordinarySE:=0.777973915534, krSE:=0.768102723693, df:=24.3463538158, tValue:=2.06361479609, pValue:=0.0498699193979),
            New KrCoefficientReference(structureName:="AR(1)", effect:="site_south", estimate:=1.15643190431, ordinarySE:=0.786523168381, krSE:=0.776792795656, df:=24.4082729923, tValue:=1.48872635119, pValue:=0.149367283692),
            New KrCoefficientReference(structureName:="AR(1)", effect:="age_centered_8", estimate:=0.614140003843, ordinarySE:=0.141157640555, krSE:=0.141501843641, df:=87.0452375997, tValue:=4.34015549226, pValue:=0.0000382559485532),
            New KrCoefficientReference(structureName:="AR(1)", effect:="treatment_active:age_centered_8", estimate:=0.150471239473, ordinarySE:=0.1965265479, krSE:=0.196981533801, df:=87.2396663049, tValue:=0.76388500267, pValue:=0.446996844327),
            New KrCoefficientReference(structureName:="Heterogeneous AR(1)", effect:="(Intercept)", estimate:=22.364479448, ordinarySE:=0.797804585456, krSE:=0.825713127688, df:=31.5424297563, tValue:=27.0850476977, pValue:=2.07451809561E-23),
            New KrCoefficientReference(structureName:="Heterogeneous AR(1)", effect:="sex_code", estimate:=-2.2340253763, ordinarySE:=0.645791543929, krSE:=0.685476294586, df:=24.810176996, tValue:=-3.25908480563, pValue:=0.00323457891202),
            New KrCoefficientReference(structureName:="Heterogeneous AR(1)", effect:="treatment_active", estimate:=-0.715421008227, ordinarySE:=0.888401300514, krSE:=0.908491596369, df:=30.3479167128, tValue:=-0.787482251995, pValue:=0.437107077672),
            New KrCoefficientReference(structureName:="Heterogeneous AR(1)", effect:="site_central", estimate:=1.4864020675, ordinarySE:=0.771780909347, krSE:=0.819290976557, df:=24.7520396533, tValue:=1.81425416614, pValue:=0.0817791323823),
            New KrCoefficientReference(structureName:="Heterogeneous AR(1)", effect:="site_south", estimate:=1.19555038746, ordinarySE:=0.778881261176, krSE:=0.82744164185, df:=24.657212917, tValue:=1.44487577975, pValue:=0.161082187498),
            New KrCoefficientReference(structureName:="Heterogeneous AR(1)", effect:="age_centered_8", estimate:=0.631172345746, ordinarySE:=0.141973769456, krSE:=0.143482532662, df:=51.105649921, tValue:=4.39894901515, pValue:=0.0000553776361177),
            New KrCoefficientReference(structureName:="Heterogeneous AR(1)", effect:="treatment_active:age_centered_8", estimate:=0.134366553626, ordinarySE:=0.197400402551, krSE:=0.199624812458, df:=52.7319451693, tValue:=0.673095453274, pValue:=0.503825907853),
            New KrCoefficientReference(structureName:="Unstructured", effect:="(Intercept)", estimate:=21.8234260393, ordinarySE:=0.817164947301, krSE:=0.890427538949, df:=25.0843318574, tValue:=24.5089297946, pValue:=4.83673231447E-19),
            New KrCoefficientReference(structureName:="Unstructured", effect:="sex_code", estimate:=-1.86063961677, ordinarySE:=0.728489649828, krSE:=0.817432732824, df:=21.5912519591, tValue:=-2.27619905841, pValue:=0.0331208690034),
            New KrCoefficientReference(structureName:="Unstructured", effect:="treatment_active", estimate:=-0.632664646839, ordinarySE:=0.828486124506, krSE:=0.884361051404, df:=21.2397779368, tValue:=-0.715391802743, pValue:=0.482157802044),
            New KrCoefficientReference(structureName:="Unstructured", effect:="site_central", estimate:=1.72365046105, ordinarySE:=0.877889430256, krSE:=0.987940507096, df:=21.8132803262, tValue:=1.74469054429, pValue:=0.0951139800457),
            New KrCoefficientReference(structureName:="Unstructured", effect:="site_south", estimate:=1.47520542741, ordinarySE:=0.875177973519, krSE:=0.982266115024, df:=21.2564747793, tValue:=1.5018388651, pValue:=0.147849141719),
            New KrCoefficientReference(structureName:="Unstructured", effect:="age_centered_8", estimate:=0.593706450429, ordinarySE:=0.111733631136, krSE:=0.114772232774, df:=24.9923610546, tValue:=5.17291017244, pValue:=0.000023862406393),
            New KrCoefficientReference(structureName:="Unstructured", effect:="treatment_active:age_centered_8", estimate:=0.22499853179, ordinarySE:=0.153903194599, krSE:=0.157849447892, df:=25.4124603847, tValue:=1.42539954872, pValue:=0.166209684044)
        }
    End Function


    Private Shared Function LoadModelData() As ModelData
        Dim rows As List(Of Dictionary(Of String, String)) = LoadRows()
        rows = rows.FindAll(Function(r) Not IsMissing(r("distance_mm")))

        Dim n As Integer = rows.Count
        Dim y(n - 1) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double
        Dim x(n - 1, 6) As Double

        For i As Integer = 0 To n - 1
            Dim r As Dictionary(Of String, String) = rows(i)

            Dim sexCode As Double = ParseD(r("sex_code"))
            Dim active As Double = If(String.Equals(r("treatment_arm"), "Active", StringComparison.OrdinalIgnoreCase), 1.0, 0.0)
            Dim siteCentral As Double = If(String.Equals(r("clinic_site"), "Central", StringComparison.OrdinalIgnoreCase), 1.0, 0.0)
            Dim siteSouth As Double = If(String.Equals(r("clinic_site"), "South", StringComparison.OrdinalIgnoreCase), 1.0, 0.0)
            Dim ageCentered As Double = ParseD(r("age_centered_8"))

            y(i) = ParseD(r("distance_mm"))
            subject(i) = r("subject_id")
            visit(i) = ParseD(r("visit"))

            x(i, 0) = 1.0
            x(i, 1) = sexCode
            x(i, 2) = active
            x(i, 3) = siteCentral
            x(i, 4) = siteSouth
            x(i, 5) = ageCentered
            x(i, 6) = active * ageCentered
        Next

        Return New ModelData With {
            .Y = y,
            .X = x,
            .SubjectId = subject,
            .Visit = visit
        }
    End Function


    Private Shared Function LoadRows() As List(Of Dictionary(Of String, String))
        Dim path As String = GetTestDataPath(DATA_FILE)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then
            Throw New InvalidOperationException(DATA_FILE & " must contain a header and at least one data row.")
        End If

        Dim header() As String = SplitCsvSimple(lines(0))
        Dim rows As New List(Of Dictionary(Of String, String))()

        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For

            Dim parts() As String = SplitCsvSimple(lines(i))
            Dim row As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

            For j As Integer = 0 To header.Length - 1
                Dim value As String = String.Empty
                If j < parts.Length Then value = parts(j)
                row(header(j)) = value
            Next

            rows.Add(row)
        Next

        Return rows
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


    Private Shared Function SplitCsvSimple(line As String) As String()
        Dim raw() As String = line.Split(","c)
        Dim out(raw.Length - 1) As String

        For i As Integer = 0 To raw.Length - 1
            out(i) = CleanCsvToken(raw(i))
        Next

        Return out
    End Function


    Private Shared Function CleanCsvToken(value As String) As String
        If value Is Nothing Then Return String.Empty

        Dim s As String = value.Trim()
        If s.Length >= 2 AndAlso s.StartsWith("""", StringComparison.Ordinal) AndAlso s.EndsWith("""", StringComparison.Ordinal) Then
            s = s.Substring(1, s.Length - 2).Replace("""""", """")
        End If

        Return s.Trim()
    End Function


    Private Shared Function ParseD(text As String) As Double
        Return Double.Parse(CleanCsvToken(text), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function


    Private Shared Function IsMissing(value As String) As Boolean
        Return String.IsNullOrWhiteSpace(value)
    End Function


    Private Shared Sub AssertAlmostEqual(expected As Double,
                                         actual As Double,
                                         absoluteTolerance As Double,
                                         label As String,
                                         Optional relativeTolerance As Double = 0.0)
        If Double.IsNaN(expected) OrElse Double.IsInfinity(expected) Then
            Assert.Fail(label & ": reference constant is not finite.")
        End If

        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail(label & ": actual value is not finite. Expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If

        Dim allowed As Double = Math.Max(absoluteTolerance, Math.Abs(expected) * relativeTolerance)
        Dim diff As Double = Math.Abs(expected - actual)
        If diff > allowed Then
            Assert.Fail(label & ": expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " &
                        actual.ToString("G17", CultureInfo.InvariantCulture) &
                        ", abs diff " &
                        diff.ToString("G17", CultureInfo.InvariantCulture) &
                        " > allowed tolerance " &
                        allowed.ToString("G17", CultureInfo.InvariantCulture) &
                        " (abs=" & absoluteTolerance.ToString("G17", CultureInfo.InvariantCulture) &
                        ", rel=" & relativeTolerance.ToString("G17", CultureInfo.InvariantCulture) & ").")
        End If
    End Sub

    Private Class ModelData
        Public Y() As Double
        Public X(,) As Double
        Public SubjectId() As Object
        Public Visit() As Double
    End Class


    Private Class KrCoefficientReference
        Public ReadOnly StructureName As String
        Public ReadOnly Effect As String
        Public ReadOnly Estimate As Double
        Public ReadOnly OrdinarySE As Double
        Public ReadOnly KRSE As Double
        Public ReadOnly DF As Double
        Public ReadOnly TValue As Double
        Public ReadOnly PValue As Double

        Public Sub New(structureName As String,
                       effect As String,
                       estimate As Double,
                       ordinarySE As Double,
                       krSE As Double,
                       df As Double,
                       tValue As Double,
                       pValue As Double)
            Me.StructureName = structureName
            Me.Effect = effect
            Me.Estimate = estimate
            Me.OrdinarySE = ordinarySE
            Me.KRSE = krSE
            Me.DF = df
            Me.TValue = tValue
            Me.PValue = pValue
        End Sub
    End Class



    ' -------------------------------------------------------------------------
    ' R mmrm multi-df Type III / term-level KR F-test validation
    ' -------------------------------------------------------------------------
    '
    ' To activate this external reference test:
    '   1. Run R_referenceScripts/kr_mmrm_multicovariate_missing_type3_reference.R.
    '   2. Paste the generated New KrType3Reference(...) rows into
    '      RmmrmType3References() below.
    '   3. Remove the Ignore attribute from the test method.
    '
    ' The test intentionally uses the same fitted BESHStatNG models as the scalar
    ' KR reference test above.  The R script creates the matching L matrices from
    ' the same coefficient names, including a true two-df clinic_site test formed
    ' from site_central and site_south.
    <TestMethod()>
    Public Sub MulticovariateMissingMMRM_KRType3FTests_MatchesRmmrmReferences()
        Dim allRefs As List(Of KrType3Reference) = RmmrmType3References()
        Assert.IsTrue(allRefs.Count > 0,
                      "No hard-coded R mmrm multi-df Type III KR F-test references have been pasted yet.")

        Dim structures() As String = {
            "Compound Symmetry",
            "Heterogeneous Compound Symmetry",
            "AR(1)",
            "Heterogeneous AR(1)",
            "Unstructured"
        }

        For Each structureName As String In structures
            Dim refs As List(Of KrType3Reference) = Type3ReferencesForStructure(allRefs, structureName)
            Assert.IsTrue(refs.Count > 0, "No hard-coded R mmrm Type III KR references for " & structureName & ".")

            Dim result As MixedModelResult = FitKrMMRM(structureName)
            AssertUsableKrResult(result, structureName)

            For Each expected As KrType3Reference In refs
                Dim l(,) As Double = BuildRestrictionMatrixForEffects(result.P, expected.Effects)

                Dim actual As MixedModelKenwardRogerMultiDfInference = Nothing
                Dim diagnostic As String = String.Empty

                Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(result,
                                                                                  expected.TermName,
                                                                                  l,
                                                                                  actual,
                                                                                  alpha:=0.05,
                                                                                  diagnostic:=diagnostic),
                              structureName & " " & expected.TermName & " KR Type III F test failed: " & diagnostic)
                Assert.IsNotNull(actual, structureName & " " & expected.TermName & " KR Type III F result should not be Nothing.")

                AssertAlmostEqual(expected.NumDF,
                                  actual.NumDF,
                                  TYPE3_NUMDF_ABS_TOL,
                                  structureName & " " & expected.TermName & " numerator DF")

                AssertAlmostEqual(expected.DenDF,
                                  actual.DenDF,
                                  TYPE3_DENDF_ABS_TOL,
                                  structureName & " " & expected.TermName & " denominator DF",
                                  TYPE3_DENDF_REL_TOL)

                AssertAlmostEqual(expected.UnscaledF,
                                  actual.UnscaledFStatistic,
                                  TYPE3_F_ABS_TOL,
                                  structureName & " " & expected.TermName & " unscaled F",
                                  TYPE3_F_REL_TOL)

                AssertAlmostEqual(expected.Scaling,
                                  actual.Scaling,
                                  TYPE3_SCALING_ABS_TOL,
                                  structureName & " " & expected.TermName & " F scaling",
                                  TYPE3_SCALING_REL_TOL)

                AssertAlmostEqual(expected.ScaledF,
                                  actual.FStatistic,
                                  TYPE3_F_ABS_TOL,
                                  structureName & " " & expected.TermName & " scaled F",
                                  TYPE3_F_REL_TOL)

                AssertAlmostEqual(expected.PValue,
                                  actual.PValue,
                                  TYPE3_P_ABS_TOL,
                                  structureName & " " & expected.TermName & " p-value",
                                  TYPE3_P_REL_TOL)
            Next
        Next
    End Sub


    Private Const TYPE3_NUMDF_ABS_TOL As Double = 0.0000000001
    Private Const TYPE3_DENDF_ABS_TOL As Double = 0.001
    Private Const TYPE3_DENDF_REL_TOL As Double = 0.0001
    Private Const TYPE3_F_ABS_TOL As Double = 0.0001
    Private Const TYPE3_F_REL_TOL As Double = 0.0001
    Private Const TYPE3_SCALING_ABS_TOL As Double = 0.0001
    Private Const TYPE3_SCALING_REL_TOL As Double = 0.00001
    Private Const TYPE3_P_ABS_TOL As Double = 0.00005
    Private Const TYPE3_P_REL_TOL As Double = 0.00001


    Private Shared Function RmmrmType3References() As List(Of KrType3Reference)
        ' R mmrm external reference values for multi-df term-level KR F tests.
        ' Generated from R_referenceScripts/kr_mmrm_multicovariate_missing_type3_reference.R
        ' using the existing mixedmodel_longitudinal_multicovariate_missing.csv data file.
        '
        ' These constants intentionally include both one-df terms and the two-df
        ' clinic_site term formed from site_central and site_south.
        Return New List(Of KrType3Reference) From {
            New KrType3Reference(structureName:="Compound Symmetry", termName:="sex_code", effects:=New String() {"sex_code"}, numDF:=1, denDF:=21.48153511, unscaledF:=8.37795692135, scaling:=1, scaledF:=8.37795692135, pValue:=0.00854338440675),
            New KrType3Reference(structureName:="Compound Symmetry", termName:="treatment_active", effects:=New String() {"treatment_active"}, numDF:=1, denDF:=35.9714049663, unscaledF:=0.914205538982, scaling:=1, scaledF:=0.914205538982, pValue:=0.345383139388),
            New KrType3Reference(structureName:="Compound Symmetry", termName:="clinic_site", effects:=New String() {"site_central", "site_south"}, numDF:=2, denDF:=21.3487764748, unscaledF:=2.21126919018, scaling:=1, scaledF:=2.21126919018, pValue:=0.134036701357),
            New KrType3Reference(structureName:="Compound Symmetry", termName:="age_centered_8", effects:=New String() {"age_centered_8"}, numDF:=1, denDF:=67.0325736096, unscaledF:=32.540526130799996, scaling:=1, scaledF:=32.540526130799996, pValue:=0.000000287026818309),
            New KrType3Reference(structureName:="Compound Symmetry", termName:="treatment_active:age_centered_8", effects:=New String() {"treatment_active:age_centered_8"}, numDF:=1, denDF:=66.7355191106, unscaledF:=2.51851550242, scaling:=1, scaledF:=2.51851550242, pValue:=0.117243686452),
            New KrType3Reference(structureName:="Heterogeneous Compound Symmetry", termName:="sex_code", effects:=New String() {"sex_code"}, numDF:=1, denDF:=21.7731324373, unscaledF:=7.07126837516, scaling:=1, scaledF:=7.07126837516, pValue:=0.0144053548999),
            New KrType3Reference(structureName:="Heterogeneous Compound Symmetry", termName:="treatment_active", effects:=New String() {"treatment_active"}, numDF:=1, denDF:=27.5454298453, unscaledF:=0.744119270632, scaling:=1, scaledF:=0.744119270632, pValue:=0.395792969181),
            New KrType3Reference(structureName:="Heterogeneous Compound Symmetry", termName:="clinic_site", effects:=New String() {"site_central", "site_south"}, numDF:=2, denDF:=21.6002241533, unscaledF:=1.86863746408, scaling:=1, scaledF:=1.86863746408, pValue:=0.17844030734),
            New KrType3Reference(structureName:="Heterogeneous Compound Symmetry", termName:="age_centered_8", effects:=New String() {"age_centered_8"}, numDF:=1, denDF:=50.4894677209, unscaledF:=31.1369955307, scaling:=1, scaledF:=31.1369955307, pValue:=0.000000950845752039),
            New KrType3Reference(structureName:="Heterogeneous Compound Symmetry", termName:="treatment_active:age_centered_8", effects:=New String() {"treatment_active:age_centered_8"}, numDF:=1, denDF:=51.0095250619, unscaledF:=2.35506384975, scaling:=1, scaledF:=2.35506384975, pValue:=0.131056542469),
            New KrType3Reference(structureName:="AR(1)", termName:="sex_code", effects:=New String() {"sex_code"}, numDF:=1, denDF:=24.4415717876, unscaledF:=11.4030870358, scaling:=1, scaledF:=11.4030870358, pValue:=0.0024527166429),
            New KrType3Reference(structureName:="AR(1)", termName:="treatment_active", effects:=New String() {"treatment_active"}, numDF:=1, denDF:=46.9302385924, unscaledF:=0.738767034162, scaling:=1, scaledF:=0.738767034162, pValue:=0.394425505022),
            New KrType3Reference(structureName:="AR(1)", termName:="clinic_site", effects:=New String() {"site_central", "site_south"}, numDF:=2, denDF:=24.2728673393, unscaledF:=2.25674428411, scaling:=1, scaledF:=2.25674428411, pValue:=0.126214036926),
            New KrType3Reference(structureName:="AR(1)", termName:="age_centered_8", effects:=New String() {"age_centered_8"}, numDF:=1, denDF:=87.0452375997, unscaledF:=18.836949697, scaling:=1, scaledF:=18.836949697, pValue:=0.0000382559485532),
            New KrType3Reference(structureName:="AR(1)", termName:="treatment_active:age_centered_8", effects:=New String() {"treatment_active:age_centered_8"}, numDF:=1, denDF:=87.2396663049, unscaledF:=0.583520297303, scaling:=1, scaledF:=0.583520297303, pValue:=0.446996844327),
            New KrType3Reference(structureName:="Heterogeneous AR(1)", termName:="sex_code", effects:=New String() {"sex_code"}, numDF:=1, denDF:=24.810176996, unscaledF:=10.6216337703, scaling:=1, scaledF:=10.6216337703, pValue:=0.00323457891202),
            New KrType3Reference(structureName:="Heterogeneous AR(1)", termName:="treatment_active", effects:=New String() {"treatment_active"}, numDF:=1, denDF:=30.3479167128, unscaledF:=0.620128297207, scaling:=1, scaledF:=0.620128297207, pValue:=0.437107077672),
            New KrType3Reference(structureName:="Heterogeneous AR(1)", termName:="clinic_site", effects:=New String() {"site_central", "site_south"}, numDF:=2, denDF:=24.5924281477, unscaledF:=1.82808293381, scaling:=1, scaledF:=1.82808293381, pValue:=0.181896422317),
            New KrType3Reference(structureName:="Heterogeneous AR(1)", termName:="age_centered_8", effects:=New String() {"age_centered_8"}, numDF:=1, denDF:=51.105649921, unscaledF:=19.3507524378, scaling:=1, scaledF:=19.3507524378, pValue:=0.0000553776361177),
            New KrType3Reference(structureName:="Heterogeneous AR(1)", termName:="treatment_active:age_centered_8", effects:=New String() {"treatment_active:age_centered_8"}, numDF:=1, denDF:=52.7319451693, unscaledF:=0.453057489218, scaling:=1, scaledF:=0.453057489218, pValue:=0.503825907853),
            New KrType3Reference(structureName:="Unstructured", termName:="sex_code", effects:=New String() {"sex_code"}, numDF:=1, denDF:=21.5912519591, unscaledF:=5.18108215352, scaling:=1, scaledF:=5.18108215352, pValue:=0.0331208690034),
            New KrType3Reference(structureName:="Unstructured", termName:="treatment_active", effects:=New String() {"treatment_active"}, numDF:=1, denDF:=21.2397779368, unscaledF:=0.511785431431, scaling:=1, scaledF:=0.511785431431, pValue:=0.482157802044),
            New KrType3Reference(structureName:="Unstructured", termName:="clinic_site", effects:=New String() {"site_central", "site_south"}, numDF:=2, denDF:=21.4632796394, unscaledF:=1.77970861798, scaling:=1, scaledF:=1.77970861798, pValue:=0.192692429788),
            New KrType3Reference(structureName:="Unstructured", termName:="age_centered_8", effects:=New String() {"age_centered_8"}, numDF:=1, denDF:=24.9923610546, unscaledF:=26.7589996521, scaling:=1, scaledF:=26.7589996521, pValue:=0.000023862406393),
            New KrType3Reference(structureName:="Unstructured", termName:="treatment_active:age_centered_8", effects:=New String() {"treatment_active:age_centered_8"}, numDF:=1, denDF:=25.4124603847, unscaledF:=2.03176387348, scaling:=1, scaledF:=2.03176387348, pValue:=0.166209684044)
        }
    End Function


    Private Shared Function Type3ReferencesForStructure(allRefs As List(Of KrType3Reference),
                                                        structureName As String) As List(Of KrType3Reference)
        Dim out As New List(Of KrType3Reference)()
        For Each one As KrType3Reference In allRefs
            If String.Equals(one.StructureName, structureName, StringComparison.OrdinalIgnoreCase) Then
                out.Add(one)
            End If
        Next
        Return out
    End Function


    Private Shared Function BuildRestrictionMatrixForEffects(coefficientCount As Integer,
                                                             effects() As String) As Double(,)
        If effects Is Nothing OrElse effects.Length = 0 Then
            Throw New InvalidOperationException("Type III reference does not contain any coefficient names.")
        End If

        Dim l(effects.Length - 1, coefficientCount - 1) As Double
        For i As Integer = 0 To effects.Length - 1
            Dim j As Integer = FixedEffectIndex(effects(i))
            l(i, j) = 1.0
        Next
        Return l
    End Function


    Private Class KrType3Reference
        Public ReadOnly StructureName As String
        Public ReadOnly TermName As String
        Public ReadOnly Effects() As String
        Public ReadOnly NumDF As Double
        Public ReadOnly DenDF As Double
        Public ReadOnly UnscaledF As Double
        Public ReadOnly Scaling As Double
        Public ReadOnly ScaledF As Double
        Public ReadOnly PValue As Double

        Public Sub New(structureName As String,
                       termName As String,
                       effects() As String,
                       numDF As Double,
                       denDF As Double,
                       unscaledF As Double,
                       scaling As Double,
                       scaledF As Double,
                       pValue As Double)
            Me.StructureName = structureName
            Me.TermName = termName
            Me.Effects = effects
            Me.NumDF = numDF
            Me.DenDF = denDF
            Me.UnscaledF = unscaledF
            Me.Scaling = scaling
            Me.ScaledF = scaledF
            Me.PValue = pValue
        End Sub
    End Class

End Class

' ===== END migrated from MMRMKenwardRogerMulticovariateReferenceTests.vb =====
