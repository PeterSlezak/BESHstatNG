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
Public Class GLM_NB_Tests

    Private Const TOL_COEF As Double = 0.000001
    Private Const TOL_SE As Double = 0.000002
    Private Const TOL_STAT As Double = 0.00001
    Private Const TOL_RES As Double = 0.00001

    ' GLM.Calculate uses Offset/Weights arrays unconditionally. In some production
    ' versions, GLM.data() creates default Offset/Weights with incorrect length.
    ' To keep these tests stable, we provide explicit zero-offset and unit-weights.
    Private Shared Function ZerosVector(n As Integer) As Double()
        Dim v(n - 1) As Double
        Return v
    End Function

    Private Shared Function OnesVector(n As Integer) As Double()
        Dim v(n - 1) As Double
        For i As Integer = 0 To n - 1
            v(i) = 1.0
        Next
        Return v
    End Function

    ' ---------------------------
    ' Helpers
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

    Private Shared Function ParseDoubleInvariant(s As String) As Double
        Return Double.Parse(s, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function LoadDesignFromCsv(fileName As String, includeX As Boolean) As Tuple(Of Integer(), Double(,))
        ' CSV schema:
        '   Full: id,y,x1,x2
        '   InterceptOnly: id,y
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must have header + rows.")

        Dim header() As String = lines(0).Split(","c).Select(Function(xx) xx.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next

        If Not idx.ContainsKey("id") OrElse Not idx.ContainsKey("y") Then
            Throw New InvalidOperationException("CSV must contain id and y columns.")
        End If

        If includeX Then
            If Not idx.ContainsKey("x1") OrElse Not idx.ContainsKey("x2") Then
                Throw New InvalidOperationException("Full CSV must contain x1 and x2 columns.")
            End If
        End If

        Dim n As Integer = lines.Length - 1
        Dim ids(n - 1) As Integer
        Dim p As Integer = If(includeX, 3, 1) ' y + (x1,x2) or y only
        Dim X(n - 1, p - 1) As Double

        For r As Integer = 0 To n - 1
            Dim parts() As String = lines(r + 1).Split(","c)
            ids(r) = Integer.Parse(parts(idx("id")).Trim(), CultureInfo.InvariantCulture)
            X(r, 0) = ParseDoubleInvariant(parts(idx("y")).Trim())
            If includeX Then
                X(r, 1) = ParseDoubleInvariant(parts(idx("x1")).Trim())
                X(r, 2) = ParseDoubleInvariant(parts(idx("x2")).Trim())
            End If
        Next

        Return Tuple.Create(ids, X)
    End Function

    Private Shared Function LoadExpectedOutputs(fileName As String) As Dictionary(Of String, Dictionary(Of String, Double))
        ' CSV schema: model,key,value
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("Expected outputs CSV must have header + rows.")

        Dim dict As New Dictionary(Of String, Dictionary(Of String, Double))(StringComparer.OrdinalIgnoreCase)

        For i As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(i).Split(","c)
            If parts.Length < 3 Then Continue For

            Dim modelName As String = parts(0).Trim()
            Dim key As String = parts(1).Trim()
            Dim valStr As String = parts(2).Trim()

            If Not dict.ContainsKey(modelName) Then
                dict(modelName) = New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)
            End If

            Dim v As Double
            If valStr.Length = 0 OrElse valStr.Equals("NaN", StringComparison.OrdinalIgnoreCase) Then
                v = Double.NaN
            Else
                v = ParseDoubleInvariant(valStr)
            End If

            dict(modelName)(key) = v
        Next

        Return dict
    End Function

    Private Shared Function GetExpected(exp As Dictionary(Of String, Dictionary(Of String, Double)),
                                       modelName As String,
                                       key As String) As Double
        If Not exp.ContainsKey(modelName) Then Throw New KeyNotFoundException("Model not found in expected outputs: " & modelName)
        If Not exp(modelName).ContainsKey(key) Then Throw New KeyNotFoundException("Key not found in expected outputs: " & modelName & " / " & key)
        Return exp(modelName)(key)
    End Function

    Private Shared Function GetPrivateDoubleField(obj As Object, fieldName As String) As Double
        Dim t As Type = obj.GetType()
        Dim fi As FieldInfo = t.GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
        If fi Is Nothing Then
            ' might be on base class
            fi = t.BaseType.GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
        End If
        Assert.IsNotNull(fi, "Field not found: " & fieldName)
        Return CDbl(fi.GetValue(obj))
    End Function

    Private Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, msg As String)
        If Double.IsNaN(expected) Then
            Assert.IsTrue(Double.IsNaN(actual), msg & " expected NaN.")
        Else
            Assert.AreEqual(expected, actual, tol, msg)
        End If
    End Sub

    Private Shared Function LoadExpectedResidualsFull(fileName As String) As Tuple(Of Integer(), Double(,))
        ' CSV schema: id, Raw Resid., Deviance Resid., Pearson Resid., Laverage, Std Deviance Resid., Std Pearson Resid., Cook Distance
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("Residuals CSV must have header + rows.")

        Dim header() As String = lines(0).Split(","c).Select(Function(x) x.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next

        Dim required() As String = {"id", "Raw Resid.", "Deviance Resid.", "Pearson Resid.", "Laverage", "Std Deviance Resid.", "Std Pearson Resid.", "Cook Distance"}
        For Each r In required
            If Not idx.ContainsKey(r) Then Throw New InvalidOperationException("Missing residual column: " & r)
        Next

        Dim n As Integer = lines.Length - 1
        Dim ids(n - 1) As Integer
        Dim mat(n - 1, 6) As Double

        For r As Integer = 0 To n - 1
            Dim parts() As String = lines(r + 1).Split(","c)
            ids(r) = Integer.Parse(parts(idx("id")).Trim(), CultureInfo.InvariantCulture)
            mat(r, 0) = ParseDoubleInvariant(parts(idx("Raw Resid.")).Trim())
            mat(r, 1) = ParseDoubleInvariant(parts(idx("Deviance Resid.")).Trim())
            mat(r, 2) = ParseDoubleInvariant(parts(idx("Pearson Resid.")).Trim())
            mat(r, 3) = ParseDoubleInvariant(parts(idx("Laverage")).Trim())
            mat(r, 4) = ParseDoubleInvariant(parts(idx("Std Deviance Resid.")).Trim())
            mat(r, 5) = ParseDoubleInvariant(parts(idx("Std Pearson Resid.")).Trim())
            mat(r, 6) = ParseDoubleInvariant(parts(idx("Cook Distance")).Trim())
        Next

        Return Tuple.Create(ids, mat)
    End Function

    Private Shared Sub AssertResidualTableMatchesExpected(ids() As Integer,
                                                         expected(,) As Double,
                                                         table(,) As Object,
                                                         tol As Double)
        ' table is ResultTable.returnSelf(): header row then body rows, columns correspond to residuals in order.
        Assert.IsNotNull(table, "AllResiduals returned Nothing.")
        Dim nRows As Integer = UBound(table, 1) + 1
        Dim nCols As Integer = UBound(table, 2) + 1
        Assert.IsTrue(nCols >= 7, "AllResiduals should have at least 7 columns.")
        Assert.AreEqual(ids.Length + 1, nRows, "AllResiduals row count mismatch (should be header + n rows).")

        ' Validate numeric body
        For r As Integer = 0 To ids.Length - 1
            For c As Integer = 0 To 6
                Dim actual As Double = CDbl(table(r + 1, c))
                Dim exp As Double = expected(r, c)
                Assert.AreEqual(exp, actual, tol, $"Residual mismatch at row={r} col={c} (id={ids(r)})")
            Next
        Next
    End Sub

    ' ---------------------------
    ' Tests
    ' ---------------------------

    <TestCategory("GLM_NB2")>
    <TestMethod()>
    Public Sub GLM_NB2_FullModel_matches_reference_and_residuals()

        Dim exp = LoadExpectedOutputs("glm_nb2_expected_outputs.csv")

        Dim loaded = LoadDesignFromCsv("glm_nb2_full.csv", includeX:=True)
        Dim ids = loaded.Item1
        Dim X = loaded.Item2

        Dim m As New GLM_NB(New regression.Log())
        m.setVarNames(New String() {"y", "x1", "x2"})
        m.data(X)
        m.settingInputs(1.0, 200, 0.00000000000001)
        m.bComputeResiduals = True
        m.Fit(intercept:=1, False)

        ' alpha (NB dispersion parameter in this codebase)
        AssertAlmostEqual(GetExpected(exp, "Full", "alpha"), m.NBalpha, TOL_STAT, "alpha mismatch")

        ' coefficients + SE + z + p
        Dim coef() As Double = m.results.Coeffs_est
        Dim se() As Double = m.results.Coeffs_SEs
        Dim z() As Double = m.results.Coeffs_Zstat
        Dim pv() As Double = m.results.Coeffs_PvaluesZ

        Assert.AreEqual(3, coef.Length, "Full model should have 3 coefficients (Intercept, x1, x2).")

        AssertAlmostEqual(GetExpected(exp, "Full", "coef_(Intercept)"), coef(0), TOL_COEF, "Intercept coef")
        AssertAlmostEqual(GetExpected(exp, "Full", "coef_x1"), coef(1), TOL_COEF, "x1 coef")
        AssertAlmostEqual(GetExpected(exp, "Full", "coef_x2"), coef(2), TOL_COEF, "x2 coef")

        AssertAlmostEqual(GetExpected(exp, "Full", "se_(Intercept)"), se(0), TOL_SE, "Intercept SE")
        AssertAlmostEqual(GetExpected(exp, "Full", "se_x1"), se(1), TOL_SE, "x1 SE")
        AssertAlmostEqual(GetExpected(exp, "Full", "se_x2"), se(2), TOL_SE, "x2 SE")

        AssertAlmostEqual(GetExpected(exp, "Full", "z_(Intercept)"), z(0), TOL_STAT, "Intercept z")
        AssertAlmostEqual(GetExpected(exp, "Full", "z_x1"), z(1), TOL_STAT, "x1 z")
        AssertAlmostEqual(GetExpected(exp, "Full", "z_x2"), z(2), TOL_STAT, "x2 z")

        AssertAlmostEqual(GetExpected(exp, "Full", "p_(Intercept)"), pv(0), 0.000000000001, "Intercept p")
        AssertAlmostEqual(GetExpected(exp, "Full", "p_x1"), pv(1), 0.0000000001, "x1 p")
        AssertAlmostEqual(GetExpected(exp, "Full", "p_x2"), pv(2), 0.0000000001, "x2 p")

        ' Deviance / GOF / tests / info criteria
        Dim finalDev As Double = GetPrivateDoubleField(m, "pFinalDeviance")
        Dim nullDev As Double = GetPrivateDoubleField(m, "pNullDeviance")
        AssertAlmostEqual(GetExpected(exp, "Full", "final_deviance"), finalDev, TOL_STAT, "Final deviance")
        AssertAlmostEqual(GetExpected(exp, "Full", "null_deviance"), nullDev, TOL_STAT, "Null deviance")

        AssertAlmostEqual(GetExpected(exp, "Full", "g2chisq"), m.DevianceG2chisq, TOL_STAT, "G2 chisq")
        AssertAlmostEqual(GetExpected(exp, "Full", "g2df"), CDbl(m.DevianceG2df), 0.0, "G2 df")
        AssertAlmostEqual(GetExpected(exp, "Full", "g2p"), m.DevianceG2pvalue, 0.000000000001, "G2 p")

        AssertAlmostEqual(GetExpected(exp, "Full", "deviance_p"), m.DevianceGOFpvalue, 0.0000000001, "Deviance GOF p")

        AssertAlmostEqual(GetExpected(exp, "Full", "pseudoR2"), m.PseudoR2, 0.0000000001, "PseudoR2")

        AssertAlmostEqual(GetExpected(exp, "Full", "AIC"), m.AIC, 0.000000001, "AIC")
        AssertAlmostEqual(GetExpected(exp, "Full", "AICc"), m.AICc, 0.000000001, "AICc")
        AssertAlmostEqual(GetExpected(exp, "Full", "BIC"), m.BIC, 0.000000001, "BIC")

        ' LL derived from AIC identity: AIC = -2*LL + 2*(p+1)
        Dim pCount As Integer = coef.Length
        Dim llFromAIC As Double = -m.AIC / 2.0 + (pCount + 1)
        AssertAlmostEqual(GetExpected(exp, "Full", "ll"), llFromAIC, TOL_STAT, "LogLik (from AIC)")

        ' Residual table matches expected (all columns)
        Dim expRes = LoadExpectedResidualsFull("glm_nb2_expected_residuals_full.csv")
        Assert.AreEqual(ids.Length, expRes.Item1.Length, "Residual expected id length mismatch.")
        ' ensure same order
        For i As Integer = 0 To ids.Length - 1
            Assert.AreEqual(ids(i), expRes.Item1(i), "Residual id ordering mismatch at " & i)
        Next

        Dim tbl = m.AllResiduals
        AssertResidualTableMatchesExpected(ids, expRes.Item2, tbl, TOL_RES)

    End Sub

    <TestCategory("GLM_NB2")>
    <TestMethod()>
    Public Sub GLM_NB2_InterceptOnly_matches_reference()

        Dim exp = LoadExpectedOutputs("glm_nb2_expected_outputs.csv")

        Dim loaded = LoadDesignFromCsv("glm_nb2_interceptonly.csv", includeX:=False)
        Dim ids = loaded.Item1
        Dim X = loaded.Item2

        Dim m As New GLM_NB(New regression.Log())
        m.setVarNames(New String() {"y"})
        m.data(X, Offset:=ZerosVector(X.GetLength(0)), Weights:=OnesVector(X.GetLength(0)))
        m.settingInputs(1.0, 200, 0.000000000001)
        m.bComputeResiduals = False
        m.Fit(intercept:=1)

        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "alpha"), m.NBalpha, TOL_STAT, "alpha mismatch (InterceptOnly)")

        Dim coef() As Double = m.results.Coeffs_est
        Dim se() As Double = m.results.Coeffs_SEs
        Dim z() As Double = m.results.Coeffs_Zstat
        Dim pv() As Double = m.results.Coeffs_PvaluesZ

        Assert.AreEqual(1, coef.Length, "Intercept-only model should have 1 coefficient.")

        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "coef_(Intercept)"), coef(0), TOL_COEF, "Intercept coef")
        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "se_(Intercept)"), se(0), TOL_SE, "Intercept SE")
        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "z_(Intercept)"), z(0), TOL_STAT, "Intercept z")
        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "p_(Intercept)"), pv(0), 0.000000000001, "Intercept p")

        Dim finalDev As Double = GetPrivateDoubleField(m, "pFinalDeviance")
        Dim nullDev As Double = GetPrivateDoubleField(m, "pNullDeviance")
        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "final_deviance"), finalDev, TOL_STAT, "Final deviance")
        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "null_deviance"), m.results.ModelTableVals(3, 0), TOL_STAT, "Null deviance")

        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "AIC"), m.AIC, 0.000000001, "AIC")
        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "AICc"), m.AICc, 0.000000001, "AICc")
        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "BIC"), m.BIC, 0.000000001, "BIC")

        Dim llFromAIC As Double = -m.AIC / 2.0 + (coef.Length + 1)
        AssertAlmostEqual(GetExpected(exp, "InterceptOnly", "ll"), llFromAIC, TOL_STAT, "LogLik (from AIC)")

    End Sub

End Class
