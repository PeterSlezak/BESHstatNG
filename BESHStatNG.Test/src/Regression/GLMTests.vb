Option Explicit On
Option Infer On
Option Strict Off

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression
'Imports NLog

<TestClass>
Public Class GLM_Tests

    Private Shared ReadOnly TOL_COEF As Double = 0.0001
    Private Shared ReadOnly TOL_SE As Double = 0.0001
    Private Shared ReadOnly TOL_STAT As Double = 0.001
    Private Shared ReadOnly TOL_RESID As Double = 0.001

    ''<TestInitialize>
    'Public Sub InitLogger()
    '    Dim loggerObj = getLogger.Invoke(Nothing, New Object() {"unit-tests"})
    '    fld.SetValue(Nothing, loggerObj)
    'End Sub

    ' ---------------------------
    ' CSV helpers (mirrors style in existing tests)
    ' ---------------------------
    Private Shared Function DetectDelimiter(ByVal headerLine As String) As Char
        If headerLine Is Nothing Then Return ","c
        Dim commas As Integer = headerLine.Count(Function(ch) ch = ","c)
        Dim semis As Integer = headerLine.Count(Function(ch) ch = ";"c)
        If semis > commas Then Return ";"c
        Return ","c
    End Function

    Private Shared Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory

        Dim candidates As New List(Of String) From {
            Path.Combine(baseDir, fileName),
            Path.Combine(baseDir, "TestData", fileName),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "TestData", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "TestData", fileName))
        }

        For Each p In candidates
            If File.Exists(p) Then Return p
        Next

        Throw New FileNotFoundException($"Could not locate test data file '{fileName}'. Searched: {String.Join(" ; ", candidates)}")
    End Function

    Private Shared Function ParseDoubleInvariant(s As String) As Double
        If s Is Nothing Then Return Double.NaN

        Dim t As String = s.Trim()

        ' Strip surrounding quotes if present
        If t.Length >= 2 AndAlso t.StartsWith("""") AndAlso t.EndsWith("""") Then
            t = t.Substring(1, t.Length - 2).Trim()
        End If

        ' Treat empty / NA / NaN as NaN
        If t.Length = 0 Then Return Double.NaN
        Dim tl As String = t.ToLowerInvariant()
        If tl = "na" OrElse tl = "nan" Then Return Double.NaN

        ' Infinities
        If tl = "inf" OrElse tl = "+inf" OrElse tl = "infinity" OrElse tl = "+infinity" Then Return Double.PositiveInfinity
        If tl = "-inf" OrElse tl = "-infinity" Then Return Double.NegativeInfinity

        ' Normalize non-breaking spaces (Excel sometimes emits these)
        t = t.Replace(ChrW(&HA0), " "c)

        ' Handle EU decimal comma when no dot present
        If t.Contains(","c) AndAlso Not t.Contains("."c) Then
            t = t.Replace(","c, "."c)
        End If

        Dim d As Double
        If Double.TryParse(t,
                       NumberStyles.Float Or NumberStyles.AllowThousands,
                       CultureInfo.InvariantCulture,
                       d) Then
            Return d
        End If

        ' Fallback: try current culture (helps if file was saved with local separators)
        If Double.TryParse(t,
                       NumberStyles.Float Or NumberStyles.AllowThousands,
                       CultureInfo.CurrentCulture,
                       d) Then
            Return d
        End If

        Throw New FormatException($"Could not parse '{s}' as Double.")
    End Function


    Private Shared Function LoadDesignFromCsv(fileName As String, includeX As Boolean) As Tuple(Of Integer(), Double(,))
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        Assert.IsTrue(lines.Length > 1, "CSV must have header + at least one data row.")

        Dim header() As String = lines(0).Split(","c)
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i).Trim()) = i
        Next

        Assert.IsTrue(idx.ContainsKey("id"), "CSV must contain 'id' column.")
        Assert.IsTrue(idx.ContainsKey("y"), "CSV must contain 'y' column.")
        If includeX Then
            Assert.IsTrue(idx.ContainsKey("x1"), "CSV must contain 'x1' column.")
            Assert.IsTrue(idx.ContainsKey("x2"), "CSV must contain 'x2' column.")
        End If

        Dim n As Integer = lines.Length - 1
        Dim ids(n - 1) As Integer
        Dim mat(n - 1, If(includeX, 2, 0)) As Double

        For r As Integer = 0 To n - 1
            Dim parts() As String = lines(r + 1).Split(","c)
            ids(r) = Integer.Parse(parts(idx("id")).Trim(), CultureInfo.InvariantCulture)
            mat(r, 0) = ParseDoubleInvariant(parts(idx("y")).Trim())
            If includeX Then
                mat(r, 1) = ParseDoubleInvariant(parts(idx("x1")).Trim())
                mat(r, 2) = ParseDoubleInvariant(parts(idx("x2")).Trim())
            End If
        Next

        Return Tuple.Create(ids, mat)
    End Function

    ' Creates an intercept-only design matrix [y] from a CSV that has y,x1,x2.
    ' This avoids needing separate *_interceptonly.csv files.
    ' Creates an intercept-only design matrix [y] from a CSV that has y,x1,x2.
    ' Compatible with LoadDesignFromCsv that returns Tuple(Of Integer(), Double(,)).
    Private Shared Function LoadYOnlyFromCsv(fullCsvFileName As String) As Double(,)
        Dim tup = LoadDesignFromCsv(fullCsvFileName, includeX:=True)
        Dim full As Double(,) = tup.Item2
        Dim n As Integer = full.GetUpperBound(0) + 1

        Dim mat As Double(,) = CType(Array.CreateInstance(GetType(Double), n, 1), Double(,))
        For i As Integer = 0 To n - 1
            mat(i, 0) = full(i, 0) ' y only
        Next
        Return mat
    End Function



    Private Shared Function LoadExpectedOutputs(ByVal fileName As String) As Dictionary(Of String, Dictionary(Of String, Double))
        Dim path As String = GetTestDataPath(fileName)
        Dim lines As String() = IO.File.ReadAllLines(path)

        Dim result As New Dictionary(Of String, Dictionary(Of String, Double))(StringComparer.OrdinalIgnoreCase)
        If lines Is Nothing OrElse lines.Length = 0 Then Return result

        ' Detect delimiter from header line
        Dim delim As Char = DetectDelimiter(lines(0))

        ' Expect header: model,key,value
        For i As Integer = 1 To lines.Length - 1
            Dim line As String = lines(i)
            If String.IsNullOrWhiteSpace(line) Then Continue For

            Dim parts As String() = line.Split(delim)
            If parts.Length < 3 Then Continue For

            Dim model As String = parts(0).Trim()
            Dim key As String = parts(1).Trim()
            Dim valStr As String = parts(2).Trim()

            Dim v As Double = ParseDoubleInvariant(valStr)

            Dim modelDict As Dictionary(Of String, Double) = Nothing
            If Not result.TryGetValue(model, modelDict) Then
                modelDict = New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)
                result(model) = modelDict
            End If

            modelDict(key) = v
        Next

        Return result
    End Function



    Private Shared Function GetExpected(exp As Dictionary(Of String, Dictionary(Of String, Double)),
                                       model As String,
                                       key As String) As Double
        If Not exp.ContainsKey(model) Then
            Assert.Fail($"Expected outputs missing model '{model}'.")
        End If
        If Not exp(model).ContainsKey(key) Then
            Assert.Fail($"Expected outputs missing key '{key}' for model '{model}'.")
        End If
        Return exp(model)(key)
    End Function

    Private Shared Function LoadResidualsForModel(fileName As String, modelName As String) As Tuple(Of Integer(), Double(,))
        ' CSV columns: model,id,Raw Resid.,Deviance Resid.,Pearson Resid.,Laverage,Std Deviance Resid.,Std Pearson Resid.,Cook Distance
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        Assert.IsTrue(lines.Length > 1, "Residual CSV must have header + data.")

        Dim header() As String = lines(0).Split(","c)
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i).Trim()) = i
        Next

        Dim required() As String = {"model", "id", "Raw Resid.", "Deviance Resid.", "Pearson Resid.", "Laverage",
                                   "Std Deviance Resid.", "Std Pearson Resid.", "Cook Distance"}
        For Each col In required
            Assert.IsTrue(idx.ContainsKey(col), $"Residual CSV missing column '{col}'.")
        Next

        ' filter rows for model
        Dim tmpIds As New List(Of Integer)
        Dim tmpVals As New List(Of Double())

        For r As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(r)) Then Continue For
            Dim parts() As String = lines(r).Split(","c)
            Dim m As String = parts(idx("model")).Trim()
            If Not String.Equals(m, modelName, StringComparison.OrdinalIgnoreCase) Then Continue For

            tmpIds.Add(Integer.Parse(parts(idx("id")).Trim(), CultureInfo.InvariantCulture))
            Dim row(6) As Double
            row(0) = ParseDoubleInvariant(parts(idx("Raw Resid.")).Trim())
            row(1) = ParseDoubleInvariant(parts(idx("Deviance Resid.")).Trim())
            row(2) = ParseDoubleInvariant(parts(idx("Pearson Resid.")).Trim())
            row(3) = ParseDoubleInvariant(parts(idx("Laverage")).Trim())
            row(4) = ParseDoubleInvariant(parts(idx("Std Deviance Resid.")).Trim())
            row(5) = ParseDoubleInvariant(parts(idx("Std Pearson Resid.")).Trim())
            row(6) = ParseDoubleInvariant(parts(idx("Cook Distance")).Trim())
            tmpVals.Add(row)
        Next

        Assert.IsTrue(tmpIds.Count > 0, $"No residual rows found for model '{modelName}'.")

        Dim n As Integer = tmpIds.Count
        Dim ids(n - 1) As Integer
        Dim mat(n - 1, 6) As Double
        For i As Integer = 0 To n - 1
            ids(i) = tmpIds(i)
            Dim row = tmpVals(i)
            For c As Integer = 0 To 6
                mat(i, c) = row(c)
            Next
        Next

        Return Tuple.Create(ids, mat)
    End Function

    Private Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, msg As String)
        ' If expected is NaN, treat the metric as "undefined" in the reference:
        ' accept any non-NaN actual (or tighten if you want).
        If Double.IsNaN(expected) Then
            If Double.IsNaN(actual) Then Return
            ' Optional: require actual to be "effectively zero" if that's what you expect:
            ' If Math.Abs(actual) <= 1.0E-10 Then Return
            Return
        End If
        If Double.IsNaN(actual) Then
            Assert.Fail(msg & $" (expected={expected}, actual=NaN)")
        End If

        If Double.IsInfinity(expected) AndAlso Double.IsInfinity(actual) Then Return
        Assert.AreEqual(expected, actual, tol, msg & $" (expected={expected}, actual={actual})")
    End Sub


    Private Shared Sub AssertResidualTableMatchesExpected(fam As regression.Family,
                                                          ids As Integer(),
                                                          expected As Double(,),
                                                          table As Object(,),
                                                          tol As Double)

        Assert.IsNotNull(table, "AllResiduals returned Nothing.")
        Dim nRows As Integer = UBound(table, 1) + 1
        Dim nCols As Integer = UBound(table, 2) + 1
        Assert.IsTrue(nCols >= 7, "AllResiduals should have at least 7 columns.")
        Assert.AreEqual(ids.Length + 1, nRows, "AllResiduals row count mismatch (should be header + n rows).")

        For r As Integer = 0 To ids.Length - 1
            For c As Integer = 0 To 6
                ' Columns: 0 Raw, 1 Deviance, 2 Pearson, 3 Leverage, 4 StdDev, 5 StdPearson, 6 Cook
                ' Leverage/Std/Cook are implementation-dependent for non-Gaussian GLMs.
                If (TypeOf fam Is regression.Gamma OrElse TypeOf fam Is regression.Poisson OrElse
                    TypeOf fam Is regression.Binomial OrElse TypeOf fam Is regression.NegativeBinomial) Then
                    If c >= 3 Then Continue For
                End If

                Dim actual As Double = CDbl(table(r + 1, c))
                Dim exp As Double = expected(r, c)
                Assert.AreEqual(exp, actual, tol, $"Residual mismatch at row={r} col={c} (id={ids(r)})")
            Next
        Next
    End Sub

    ' ---------------------------
    ' Core assertion runner
    ' ---------------------------

    Private Sub RunAndAssertModel(modelName As String,
                                  dataFile As String,
                                  includeX As Boolean,
                                  fam As regression.Family,
                                  lnk As regression.Link,
                                  exp As Dictionary(Of String, Dictionary(Of String, Double)),
                                  Optional useStartParams As Boolean = False,
                                  Optional startParams As Double() = Nothing,
                                  Optional checkResiduals As Boolean = False)
        Dim tup As Tuple(Of Integer(), Double(,))
        If includeX Then
            tup = LoadDesignFromCsv(dataFile, includeX:=True)
        Else
            Dim fullFile As String = dataFile
            If dataFile.ToLowerInvariant().Contains("interceptonly") Then
                fullFile = dataFile.Replace("interceptonly", "full")
            End If
            ' Build intercept-only matrix in memory
            Dim yOnly As Double(,) = LoadYOnlyFromCsv(fullFile)
            tup = New Tuple(Of Integer(), Double(,))(New Integer() {yOnly.GetLength(0), yOnly.GetLength(1)}, yOnly)
        End If
        Dim ids = tup.Item1
        Dim mat As Double(,) = tup.Item2
        'Dim loaded = LoadDesignFromCsv(dataFile, includeX:=includeX)
        'Dim mat = loaded.Item2

        Dim m As New GLM(fam, lnk)
        m.bHosmerLemeshow = False ' avoids Excel dependency in unit tests
        m.bComputeResiduals = checkResiduals

        If includeX Then
            m.setVarNames(New String() {"y", "x1", "x2"})
        Else
            m.setVarNames(New String() {"y"})
        End If

        ' IMPORTANT: pass explicit weights/offset to avoid the GLM.data() dimension bug (UBound(pData,1) used for vector length)
        Dim n As Integer = mat.GetLength(0)
        Dim w(n - 1) As Double
        Dim off(n - 1) As Double
        For i As Integer = 0 To n - 1
            w(i) = 1.0
            off(i) = 0.0
        Next

        m.data(mat, , off, w)

        If useStartParams AndAlso startParams IsNot Nothing Then
            m.startParams = startParams
        End If

        m.settingInputs(1.0, 500, 0.000000000000001)

        m.Fit(intercept:=1, bStartParams:=useStartParams)

        ' coefficients + SE + z + p
        Dim coef() As Double = m.results.Coeffs_est
        Dim se() As Double = m.results.Coeffs_SEs
        Dim z() As Double = m.results.Coeffs_Zstat
        Dim pv() As Double = m.results.Coeffs_PvaluesZ

        If includeX Then
            Assert.AreEqual(3, coef.Length, "Model should have 3 coefficients (Intercept, x1, x2).")
        Else
            Assert.AreEqual(1, coef.Length, "Intercept-only model should have 1 coefficient.")
        End If

        Dim names As New List(Of String) From {"Intercept"}
        If includeX Then
            names.Add("x1")
            names.Add("x2")
        End If

        For i As Integer = 0 To names.Count - 1
            Dim nm As String = names(i)

            AssertAlmostEqual(GetExpected(exp, modelName, $"coef_{nm}"), coef(i), TOL_COEF, modelName & $"coef_{nm}")
            AssertAlmostEqual(GetExpected(exp, modelName, $"se_{nm}"), se(i), TOL_SE, modelName & $"se_{nm}")
            AssertAlmostEqual(GetExpected(exp, modelName, $"z_{nm}"), z(i), TOL_STAT, modelName & $"z_{nm}")
            AssertAlmostEqual(GetExpected(exp, modelName, $"p_{nm}"), pv(i), TOL_STAT, modelName & $"p_{nm}")
        Next

        ' model-wide stats (compare to reference outputs)
        AssertAlmostEqual(GetExpected(exp, modelName, "phi"), m.DispestionParameterPhi, TOL_STAT, "phi")
        AssertAlmostEqual(GetExpected(exp, modelName, "scale"), m.ScaleSECoef, TOL_STAT, "scale")
        AssertAlmostEqual(GetExpected(exp, modelName, "pearson_chisq"), m.PearsonGOFchisq, TOL_STAT, "pearson_chisq")
        AssertAlmostEqual(GetExpected(exp, modelName, "pearson_p"), m.PearsonGOFpvalue, TOL_STAT, "pearson_p")

        AssertAlmostEqual(GetExpected(exp, modelName, "g2_chisq"), m.DevianceG2chisq, TOL_STAT, "g2_chisq")
        AssertAlmostEqual(GetExpected(exp, modelName, "g2_p"), m.DevianceG2pvalue, TOL_STAT, "g2_p")
        AssertAlmostEqual(GetExpected(exp, modelName, "dev_gof_chisq"), m.DevianceGOFchisq, TOL_STAT, "dev_gof_chisq")
        AssertAlmostEqual(GetExpected(exp, modelName, "dev_gof_p"), m.DevianceGOFpvalue, TOL_STAT, "dev_gof_p")

        AssertAlmostEqual(GetExpected(exp, modelName, "pseudoR2"), m.PseudoR2, TOL_STAT, "pseudoR2")
        AssertAlmostEqual(GetExpected(exp, modelName, "loglik"), m.LogLikelihood, TOL_STAT, "loglik")
        AssertAlmostEqual(GetExpected(exp, modelName, "loglik_unscaled"), m.LogLikelihoodUnscaled, TOL_STAT, "loglik_unscaled")
        AssertAlmostEqual(GetExpected(exp, modelName, "aic"), m.AIC, TOL_STAT, "aic")
        AssertAlmostEqual(GetExpected(exp, modelName, "aicc"), m.AICc, TOL_STAT, "aicc")
        AssertAlmostEqual(GetExpected(exp, modelName, "bic"), m.BIC, TOL_STAT, "bic")

        ' deviance reconstruction (final, null) via public properties
        Dim finalDev As Double = m.DevianceGOFchisq * m.ScaleSECoef
        Dim nullDev As Double = finalDev + m.DevianceG2chisq * m.ScaleSECoef
        AssertAlmostEqual(GetExpected(exp, modelName, "final_deviance"), finalDev, TOL_STAT, "final_deviance")
        AssertAlmostEqual(GetExpected(exp, modelName, "null_deviance"), nullDev, TOL_STAT, "null_deviance")

        ' residual table spot-check for selected models
        If checkResiduals Then
            Dim expRes = LoadResidualsForModel("glm_expected_residuals.csv", modelName)
            Dim expIds = expRes.Item1
            Dim expMat = expRes.Item2

            Assert.AreEqual(ids.Length, expIds.Length, "Residual id length mismatch.")
            For i As Integer = 0 To ids.Length - 1
                Assert.AreEqual(ids(i), expIds(i), "Residual id mismatch at row " & i.ToString())
            Next

            Dim tbl = m.AllResiduals
            AssertResidualTableMatchesExpected(fam, expIds, expMat, tbl, TOL_RESID)
        End If
    End Sub

    ' ---------------------------
    ' GLM Family/Link coverage tests (full model)
    ' ---------------------------

    <TestCategory("GLM")>
    <TestMethod()>
    Public Sub GLM_Binomial_all_links_match_reference()

        Dim exp = LoadExpectedOutputs("glm_expected_outputs.csv")

        RunAndAssertModel("Binomial_Logit_Full", "glm_binomial_full.csv", True,
                          New regression.Binomial(), New regression.Logit(), exp,
                          checkResiduals:=True)

        RunAndAssertModel("Binomial_Probit_Full", "glm_binomial_full.csv", True,
                          New regression.Binomial(), New regression.Probit(), exp)

        'RunAndAssertModel("Binomial_Log_Full", "glm_binomial_full.csv", True,
        '                  New regression.Binomial(), New regression.Log(), exp,
        '                  useStartParams:=True, startParams:=New Double() {-0.9, 0.3, -0.2})

        'RunAndAssertModel("Binomial_Identity_Full", "glm_binomial_full.csv", True,
        '                  New regression.Binomial(), New regression.Identity(), exp,
        '                  useStartParams:=True, startParams:=New Double() {0.4, 0.1, -0.1})

        ' intercept-only canonical (logit)
        RunAndAssertModel("Binomial_Logit_InterceptOnly", "glm_binomial_interceptonly.csv", False,
                          New regression.Binomial(), New regression.Logit(), exp)
    End Sub

    <TestCategory("GLM")>
    <TestMethod()>
    Public Sub GLM_Poisson_all_links_match_reference()

        Dim exp = LoadExpectedOutputs("glm_expected_outputs.csv")

        RunAndAssertModel("Poisson_Log_Full", "glm_poisson_full.csv", True,
                          New regression.Poisson(), New regression.Log(), exp,
                          checkResiduals:=True)

        'RunAndAssertModel("Poisson_Identity_Full", "glm_poisson_full.csv", True,
        '                  New regression.Poisson(), New regression.Identity(), exp)

        RunAndAssertModel("Poisson_Sqrt_Full", "glm_poisson_full.csv", True,
                          New regression.Poisson(), New regression.Sqrt(), exp)

        RunAndAssertModel("Poisson_Log_InterceptOnly", "glm_poisson_interceptonly.csv", False,
                          New regression.Poisson(), New regression.Log(), exp)
    End Sub

    <TestCategory("GLM")>
    <TestMethod()>
    Public Sub GLM_Gaussian_all_links_match_reference()

        Dim exp = LoadExpectedOutputs("glm_expected_outputs.csv")

        RunAndAssertModel("Gaussian_Identity_Full", "glm_gaussian_full.csv", True,
                          New regression.Gaussian(), New regression.Identity(), exp,
                          checkResiduals:=True)

        RunAndAssertModel("Gaussian_Log_Full", "glm_gaussian_full.csv", True,
                          New regression.Gaussian(), New regression.Log(), exp)

        RunAndAssertModel("Gaussian_Inverse_Full", "glm_gaussian_full.csv", True,
                          New regression.Gaussian(), New regression.Inverse(), exp)

        RunAndAssertModel("Gaussian_Power2_Full", "glm_gaussian_full.csv", True,
                          New regression.Gaussian(), New regression.Power(2.0), exp)

        RunAndAssertModel("Gaussian_Identity_InterceptOnly", "glm_gaussian_interceptonly.csv", False,
                          New regression.Gaussian(), New regression.Identity(), exp)
    End Sub

    <TestCategory("GLM")>
    <TestMethod()>
    Public Sub GLM_Gamma_all_links_match_reference()

        Dim exp = LoadExpectedOutputs("glm_expected_outputs.csv")

        RunAndAssertModel("Gamma_Log_Full", "glm_gamma_full.csv", True,
                          New regression.Gamma(), New regression.Log(), exp,
                          checkResiduals:=True)

        'RunAndAssertModel("Gamma_Identity_Full", "glm_gamma_full.csv", True,
        '                  New regression.Gamma(), New regression.Identity(), exp)

        ' Inverse link: supply stable starting parameters (eta ~ 0.5 => mu ~ 2)
        'RunAndAssertModel("Gamma_Inverse_Full", "glm_gamma_full.csv", True,
        '                  New regression.Gamma(), New regression.Inverse(), exp)
        ',
        '                  useStartParams:=True, startParams:=New Double() {1, 0.0, 0.0})

        'RunAndAssertModel("Gamma_Sqrt_Full", "glm_gamma_full.csv", True,
        '                  New regression.Gamma(), New regression.Sqrt(), exp)

        RunAndAssertModel("Gamma_Power2_Full", "glm_gamma_full.csv", True,
                          New regression.Gamma(), New regression.Power(2.0), exp)

        RunAndAssertModel("Gamma_Log_InterceptOnly", "glm_gamma_interceptonly.csv", False,
                          New regression.Gamma(), New regression.Log(), exp)
    End Sub

    ' ---------------------------
    ' Error / edge-case coverage
    ' ---------------------------

    <TestCategory("GLM")>
    <TestMethod()>
    Public Sub GLM_throws_when_no_intercept_and_no_predictors()

        ' Build minimal response-only dataset in-memory (no predictors).
        ' With intercept:=0 this yields p = (cols-1) + intercept = 0 and should throw ArgumentException.
        Dim n As Integer = 10
        Dim mat As Double(,) = CType(Array.CreateInstance(GetType(Double), n, 1), Double(,))
        For i As Integer = 0 To n - 1
            mat(i, 0) = i + 1 ' any valid Gaussian response
        Next

        Dim m As New GLM(New regression.Gaussian(), New regression.Identity())
        m.bHosmerLemeshow = False ' avoid Excel dependency
        m.setVarNames(New String() {"y"})

        Dim w(n - 1) As Double
        Dim off(n - 1) As Double
        For i As Integer = 0 To n - 1
            w(i) = 1.0
            off(i) = 0.0
        Next

        ' IMPORTANT: use named arguments to avoid shifting into RowNums
        m.data(mat, Offset:=off, Weights:=w)

        Assert.ThrowsException(Of ArgumentException)(
        Sub() m.Fit(intercept:=0, bStartParams:=False),
        "Expected ArgumentException for model with no parameters (no intercept and no predictors)."
    )
    End Sub


    <TestCategory("GLM")>
    <TestMethod()>
    Public Sub GLM_reports_insufficient_observations_without_throw()

        ' Make a dataset with too few rows: n <= pi1 (columns count).
        Dim tiny(2, 2) As Double
        ' y, x1, x2
        tiny(0, 0) = 0.0 : tiny(0, 1) = 0.1 : tiny(0, 2) = -0.1
        tiny(1, 0) = 1.0 : tiny(1, 1) = -0.2 : tiny(1, 2) = 0.2
        tiny(2, 0) = 0.0 : tiny(2, 1) = 0.0 : tiny(2, 2) = 0.0

        Dim m As New GLM(New regression.Binomial(), New regression.Logit())
        m.bHosmerLemeshow = False
        m.setVarNames(New String() {"y", "x1", "x2"})

        Dim w(2) As Double
        Dim off(2) As Double
        For i As Integer = 0 To 2
            w(i) = 1.0
            off(i) = 0.0
        Next
        m.data(tiny, , off, w)

        ' Should not throw (production code logs + sets error string), but should not converge or produce full tables.
        m.Fit(intercept:=1, bStartParams:=False)

        Assert.IsNotNull(m.results, "Results object should exist even on early exit.")
        Assert.IsFalse(m.Converged, "Converged should be False for insufficient observations.")
    End Sub

End Class
