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
Public Class ZeroInflatedPoisson_Tests

    ' Tolerances: ZIP uses iterative EM/IRLS; allow slightly looser tolerances than pure closed-form tests
    Private Const TOL_COEF As Double = 0.00001
    Private Const TOL_SE As Double = 0.00002
    Private Const TOL_STAT As Double = 0.00005
    Private Const TOL_PRED As Double = 0.00001
    Private Const TOL_RES As Double = 0.00002

    ' ---------------------------
    ' Helpers (mostly copied from existing test style)
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
        If s Is Nothing Then Return Double.NaN
        s = s.Trim()
        If s = "" Then Return Double.NaN
        Return Double.Parse(s, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function LoadExpectedOutputs(fileName As String) As Dictionary(Of String, Dictionary(Of String, Double))
        ' CSV schema: model,key,value
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("Expected outputs CSV must have header + rows.")

        Dim out As New Dictionary(Of String, Dictionary(Of String, Double))(StringComparer.OrdinalIgnoreCase)

        For i As Integer = 1 To lines.Length - 1
            Dim ln As String = lines(i).Trim()
            If ln = "" Then Continue For
            Dim parts() As String = ln.Split(","c)
            If parts.Length < 3 Then Throw New InvalidOperationException("Bad expected outputs row: " & ln)

            Dim model As String = parts(0).Trim()
            Dim key As String = parts(1).Trim()
            Dim value As Double = ParseDoubleInvariant(parts(2))

            If Not out.ContainsKey(model) Then
                out(model) = New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)
            End If
            out(model)(key) = value
        Next

        Return out
    End Function

    Private Shared Function GetExpected(exp As Dictionary(Of String, Dictionary(Of String, Double)),
                                       model As String, key As String) As Double
        If Not exp.ContainsKey(model) Then
            Throw New KeyNotFoundException("Model not found in expected outputs: " & model)
        End If
        If Not exp(model).ContainsKey(key) Then
            Throw New KeyNotFoundException("Key not found in expected outputs for model '" & model & "': " & key)
        End If
        Return exp(model)(key)
    End Function

    Private Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, msg As String)
        If Double.IsNaN(expected) Then
            Assert.IsTrue(Double.IsNaN(actual), msg & " expected NaN.")
        Else
            Assert.AreEqual(expected, actual, tol, msg)
        End If
    End Sub

    Private Shared Function GetPrivateDoubleField(obj As Object, fieldName As String) As Double
        Dim t As Type = obj.GetType()
        Dim fi As FieldInfo = t.GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
        If fi Is Nothing Then
            fi = t.BaseType.GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
        End If
        Assert.IsNotNull(fi, "Field not found: " & fieldName)
        Return CDbl(fi.GetValue(obj))
    End Function

    Private Shared Function GetPrivateBooleanField(obj As Object, fieldName As String) As Boolean
        Dim t As Type = obj.GetType()
        Dim fi As FieldInfo = t.GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
        If fi Is Nothing Then
            fi = t.BaseType.GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
        End If
        Assert.IsNotNull(fi, "Field not found: " & fieldName)
        Return CBool(fi.GetValue(obj))
    End Function

    Private Shared Function LoadZipDesignFromCsv(countFile As String, zeroFile As String) As Tuple(Of Integer(), Double(,), Double(,))
        ' count CSV schema: id,y,x1,x2
        ' zero  CSV schema: id,y,z1,z2
        Dim pCount As String = GetTestDataPath(countFile)
        Dim pZero As String = GetTestDataPath(zeroFile)

        Dim lc() As String = File.ReadAllLines(pCount)
        Dim lz() As String = File.ReadAllLines(pZero)
        If lc.Length < 2 Then Throw New InvalidOperationException("Count CSV must have header + rows.")
        If lz.Length < 2 Then Throw New InvalidOperationException("Zero CSV must have header + rows.")

        ' parse headers
        Dim hc() As String = lc(0).Split(","c).Select(Function(x) x.Trim()).ToArray()
        Dim hz() As String = lz(0).Split(","c).Select(Function(x) x.Trim()).ToArray()

        Dim idxC As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To hc.Length - 1 : idxC(hc(i)) = i : Next
        Dim idxZ As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To hz.Length - 1 : idxZ(hz(i)) = i : Next

        Dim reqC() As String = {"id", "y", "x1", "x2"}
        Dim reqZ() As String = {"id", "y", "z1", "z2"}
        For Each r In reqC
            If Not idxC.ContainsKey(r) Then Throw New InvalidOperationException("Missing count column: " & r)
        Next
        For Each r In reqZ
            If Not idxZ.ContainsKey(r) Then Throw New InvalidOperationException("Missing zero column: " & r)
        Next

        ' read data
        Dim n As Integer = lc.Length - 1
        Assert.AreEqual(n, lz.Length - 1, "Count/Zero CSV length mismatch.")

        Dim ids(n - 1) As Integer
        Dim dCount(n - 1, 2) As Double ' y, x1, x2
        Dim dZero(n - 1, 2) As Double  ' y, z1, z2

        For i As Integer = 1 To lc.Length - 1
            Dim pc() As String = lc(i).Split(","c)
            Dim pz() As String = lz(i).Split(","c)

            Dim idC As Integer = Integer.Parse(pc(idxC("id")).Trim(), CultureInfo.InvariantCulture)
            Dim idZ As Integer = Integer.Parse(pz(idxZ("id")).Trim(), CultureInfo.InvariantCulture)
            Assert.AreEqual(idC, idZ, "ID mismatch between count and zero CSV at row " & i)

            Dim yC As Double = ParseDoubleInvariant(pc(idxC("y")))
            Dim yZ As Double = ParseDoubleInvariant(pz(idxZ("y")))
            Assert.AreEqual(yC, yZ, "y mismatch between count and zero CSV at id " & idC)

            ids(i - 1) = idC

            dCount(i - 1, 0) = yC
            dCount(i - 1, 1) = ParseDoubleInvariant(pc(idxC("x1")))
            dCount(i - 1, 2) = ParseDoubleInvariant(pc(idxC("x2")))

            dZero(i - 1, 0) = yZ
            dZero(i - 1, 1) = ParseDoubleInvariant(pz(idxZ("z1")))
            dZero(i - 1, 2) = ParseDoubleInvariant(pz(idxZ("z2")))
        Next

        Return Tuple.Create(ids, dCount, dZero)
    End Function

    Private Shared Function LoadExpectedPredictions(fileName As String) As Tuple(Of Integer(), Double())
        ' CSV schema: id, mu, pi, predicted_mean
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("Predictions CSV must have header + rows.")

        Dim header() As String = lines(0).Split(","c).Select(Function(x) x.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next
        If Not idx.ContainsKey("id") OrElse Not idx.ContainsKey("predicted_mean") Then
            Throw New InvalidOperationException("Predictions CSV must include id and predicted_mean columns.")
        End If

        Dim n As Integer = lines.Length - 1
        Dim ids(n - 1) As Integer
        Dim pred(n - 1) As Double

        For i As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(i).Split(","c)
            ids(i - 1) = Integer.Parse(parts(idx("id")).Trim(), CultureInfo.InvariantCulture)
            pred(i - 1) = ParseDoubleInvariant(parts(idx("predicted_mean")))
        Next

        Return Tuple.Create(ids, pred)
    End Function

    Private Shared Function LoadExpectedResiduals(fileName As String) As Tuple(Of Integer(), Double(,))
        ' CSV schema: id, Raw Resid., Pearson Resid.
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("Residuals CSV must have header + rows.")

        Dim header() As String = lines(0).Split(","c).Select(Function(x) x.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next
        Dim required() As String = {"id", "Raw Resid.", "Pearson Resid."}
        For Each r In required
            If Not idx.ContainsKey(r) Then Throw New InvalidOperationException("Missing residual column: " & r)
        Next

        Dim n As Integer = lines.Length - 1
        Dim ids(n - 1) As Integer
        Dim vals(n - 1, 1) As Double ' raw, pearson

        For i As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(i).Split(","c)
            ids(i - 1) = Integer.Parse(parts(idx("id")).Trim(), CultureInfo.InvariantCulture)
            vals(i - 1, 0) = ParseDoubleInvariant(parts(idx("Raw Resid.")))
            vals(i - 1, 1) = ParseDoubleInvariant(parts(idx("Pearson Resid.")))
        Next

        Return Tuple.Create(ids, vals)
    End Function

    Private Shared Sub AssertResidualTableMatchesExpected(expectedVals As Double(,), actualTable As Object(,), tol As Double)
        Assert.IsNotNull(actualTable, "AllResiduals returned Nothing.")
        Dim nRows As Integer = actualTable.GetLength(0)
        Dim nCols As Integer = actualTable.GetLength(1)
        Assert.AreEqual(expectedVals.GetLength(0) + 1, nRows, "Residuals table row count mismatch (header + n).")
        Assert.AreEqual(2, nCols, "Residuals table should have exactly 2 columns (Raw, Pearson).")

        ' header
        Assert.IsTrue(CStr(actualTable(0, 0)).ToLower().Contains("raw"), "Residual header col0 should be Raw Resid.")
        Assert.IsTrue(CStr(actualTable(0, 1)).ToLower().Contains("pearson"), "Residual header col1 should be Pearson Resid.")

        For i As Integer = 0 To expectedVals.GetLength(0) - 1
            Dim raw As Double = CDbl(actualTable(i + 1, 0))
            Dim pear As Double = CDbl(actualTable(i + 1, 1))
            AssertAlmostEqual(expectedVals(i, 0), raw, tol, "Raw residual mismatch at row " & i)
            AssertAlmostEqual(expectedVals(i, 1), pear, tol, "Pearson residual mismatch at row " & i)
        Next
    End Sub

    ' ---------------------------
    ' Tests
    ' ---------------------------

    <TestCategory("ZIP")>
    <TestMethod()>
    Public Sub ZIP_BasicModel_matches_reference_outputs_predictions_and_residuals()

        Dim exp = LoadExpectedOutputs("zip_expected_outputs.csv")

        Dim loaded = LoadZipDesignFromCsv("zip_dataset_basic_count.csv", "zip_dataset_basic_zero.csv")
        Dim ids = loaded.Item1
        Dim dCount = loaded.Item2
        Dim dZero = loaded.Item3

        Dim zip As New ZeroInflatedPoisson()
        zip.dataInputs(dCount, dZero,
                       New String() {"y", "x1", "x2"},
                       New String() {"y", "z1", "z2"},
                       ids)

        zip.settingInputs(0.05, 200, 200, 0.000000000001)
        zip.bComputeResiduals = True
        zip.bIterationDetails = True
        zip.bReturnCov = True

        zip.Fit(interceptPois:=1, interceptLog:=1)

        ' --- Coefficients: Poisson/count part
        Dim coefC() As Double = zip.resultsPoisson.Coeffs_est
        Dim seC() As Double = zip.resultsPoisson.Coeffs_SEs
        Dim zC() As Double = zip.resultsPoisson.Coeffs_Zstat
        Dim pC() As Double = zip.resultsPoisson.Coeffs_PvaluesZ

        Assert.AreEqual(3, coefC.Length, "Poisson coefficient length")
        Assert.AreEqual(3, seC.Length, "Poisson SE length")

        Dim namesC() As String = {"(Intercept)", "x1", "x2"}
        For i As Integer = 0 To namesC.Length - 1
            Dim nm As String = namesC(i)
            AssertAlmostEqual(GetExpected(exp, "Basic", "count_coef_" & nm), coefC(i), TOL_COEF, "Count coef " & nm)
            AssertAlmostEqual(GetExpected(exp, "Basic", "count_se_" & nm), seC(i), TOL_SE, "Count SE " & nm)
            AssertAlmostEqual(GetExpected(exp, "Basic", "count_z_" & nm), zC(i), TOL_STAT, "Count Z " & nm)
            AssertAlmostEqual(GetExpected(exp, "Basic", "count_p_" & nm), pC(i), TOL_STAT, "Count P " & nm)
        Next

        ' --- Coefficients: Logistic/zero part
        Dim coefZ() As Double = zip.resultsLogistic.Coeffs_est
        Dim seZ() As Double = zip.resultsLogistic.Coeffs_SEs
        Dim zZ() As Double = zip.resultsLogistic.Coeffs_Zstat
        Dim pZ() As Double = zip.resultsLogistic.Coeffs_PvaluesZ

        Assert.AreEqual(3, coefZ.Length, "Zero coefficient length")
        Assert.AreEqual(3, seZ.Length, "Zero SE length")

        Dim namesZ() As String = {"(Intercept)", "z1", "z2"}
        For i As Integer = 0 To namesZ.Length - 1
            Dim nm As String = namesZ(i)
            AssertAlmostEqual(GetExpected(exp, "Basic", "zero_coef_" & nm), coefZ(i), TOL_COEF, "Zero coef " & nm)
            AssertAlmostEqual(GetExpected(exp, "Basic", "zero_se_" & nm), seZ(i), TOL_SE, "Zero SE " & nm)
            AssertAlmostEqual(GetExpected(exp, "Basic", "zero_z_" & nm), zZ(i), TOL_STAT, "Zero Z " & nm)
            AssertAlmostEqual(GetExpected(exp, "Basic", "zero_p_" & nm), pZ(i), TOL_STAT, "Zero P " & nm)
        Next

        ' --- Diagnostics
        Dim ll As Double = GetPrivateDoubleField(zip, "pLogLikelihood")
        Dim dev As Double = GetPrivateDoubleField(zip, "pFinalDeviance")
        AssertAlmostEqual(GetExpected(exp, "Basic", "loglik"), ll, TOL_STAT, "LogLikelihood")
        AssertAlmostEqual(GetExpected(exp, "Basic", "deviance"), dev, TOL_STAT, "Deviance")

        AssertAlmostEqual(GetExpected(exp, "Basic", "AIC"), zip.AIC, TOL_STAT, "AIC")
        AssertAlmostEqual(GetExpected(exp, "Basic", "AICc"), zip.AICc, TOL_STAT, "AICc")
        AssertAlmostEqual(GetExpected(exp, "Basic", "BIC"), zip.BIC, TOL_STAT, "BIC")

        ' converged flag
        Dim conv As Boolean = GetPrivateBooleanField(zip, "pConverged")
        Assert.IsTrue(conv, "ZIP did not converge for the basic dataset.")

        ' --- Predictions
        Dim expPred = LoadExpectedPredictions("zip_expected_predictions.csv")
        For i As Integer = 0 To ids.Length - 1
            Assert.AreEqual(ids(i), expPred.Item1(i), "Prediction id ordering mismatch at row " & i)
        Next

        Dim pred() As Double = zip.Predicted
        Assert.AreEqual(expPred.Item2.Length, pred.Length, "Predicted length mismatch")
        For i As Integer = 0 To pred.Length - 1
            AssertAlmostEqual(expPred.Item2(i), pred(i), TOL_PRED, "Predicted mismatch at row " & i)
        Next

        ' --- Residuals (Raw, Pearson)
        Dim expRes = LoadExpectedResiduals("zip_expected_residuals_full.csv")
        For i As Integer = 0 To ids.Length - 1
            Assert.AreEqual(ids(i), expRes.Item1(i), "Residual id ordering mismatch at row " & i)
        Next
        Dim tbl = zip.AllResiduals
        AssertResidualTableMatchesExpected(expRes.Item2, tbl, TOL_RES)

        ' --- wrapResults includes iteration info and covariance when flags set
        Dim tables = zip.wrapResults()
        Assert.IsNotNull(tables, "wrapResults returned Nothing")
        Assert.IsTrue(tables.Count >= 5, "wrapResults should return at least 5 tables when iteration details and covariance are enabled.")

    End Sub

    <TestCategory("ZIP")>
    <TestMethod()>
    Public Sub ZIP_AllNonZero_throws_argument_exception()

        ' y has no zeros -> should throw
        Dim n As Integer = 8
        Dim ids(n - 1) As Integer
        Dim dCount(n - 1, 1) As Double
        Dim dZero(n - 1, 1) As Double

        For i As Integer = 0 To n - 1
            ids(i) = i + 1
            dCount(i, 0) = 2.0
            dCount(i, 1) = i
            dZero(i, 0) = 2.0
            dZero(i, 1) = 0.5 * i
        Next

        Dim zip As New ZeroInflatedPoisson()
        zip.dataInputs(dCount, dZero,
                       New String() {"y", "x1"},
                       New String() {"y", "z1"},
                       ids)
        zip.settingInputs(0.05, 50, 50, 0.000000001)

        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                zip.Fit(interceptPois:=1, interceptLog:=1)
            End Sub)
    End Sub

    <TestCategory("ZIP")>
    <TestMethod()>
    Public Sub ZIP_AllZero_throws_argument_exception()

        ' y all zeros -> should throw
        Dim n As Integer = 8
        Dim ids(n - 1) As Integer
        Dim dCount(n - 1, 1) As Double
        Dim dZero(n - 1, 1) As Double

        For i As Integer = 0 To n - 1
            ids(i) = i + 1
            dCount(i, 0) = 0.0
            dCount(i, 1) = i
            dZero(i, 0) = 0.0
            dZero(i, 1) = 0.5 * i
        Next

        Dim zip As New ZeroInflatedPoisson()
        zip.dataInputs(dCount, dZero,
                       New String() {"y", "x1"},
                       New String() {"y", "z1"},
                       ids)
        zip.settingInputs(0.05, 50, 50, 0.000000001)

        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                zip.Fit(interceptPois:=1, interceptLog:=1)
            End Sub)
    End Sub

    <TestCategory("ZIP")>
    <TestMethod()>
    Public Sub ZIP_PredictPoissonLogLink_with_bad_offset_length_throws()

        ' Invoke the private helper to ensure it validates offsets
        Dim zip As New ZeroInflatedPoisson()

        Dim X(1, 1) As Double
        X(0, 0) = 1.0 : X(0, 1) = 0.0
        X(1, 0) = 1.0 : X(1, 1) = 1.0

        Dim beta() As Double = {0.0, 0.0}
        Dim badOffset() As Double = {1.0} ' wrong length (should be 2)

        Dim mi As MethodInfo = GetType(ZeroInflatedPoisson).GetMethod("PredictPoissonLogLink",
                                                                     BindingFlags.Instance Or BindingFlags.NonPublic)
        Assert.IsNotNull(mi, "PredictPoissonLogLink not found via reflection.")

        Dim ex As TargetInvocationException =
            Assert.ThrowsException(Of TargetInvocationException)(
                Sub()
                    mi.Invoke(zip, New Object() {X, beta, badOffset, True})
                End Sub)

        Assert.IsNotNull(ex.InnerException, "Expected inner exception.")
        Assert.IsTrue(TypeOf ex.InnerException Is ArgumentException, "Expected ArgumentException as inner exception.")
    End Sub

End Class
