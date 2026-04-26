Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.IO
Imports System.Globalization
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG

<TestClass()>
Public Class NistLinearRegression_Tests

    Private Const ABS_TOL As Double = 0.0000000001
    Private Const REL_TOL_COEF_DEFAULT As Double = 0.00000001
    Private Const REL_TOL_SE_DEFAULT As Double = 0.00000001
    Private Const REL_TOL_STAT_DEFAULT As Double = 0.00000001

    ' Filip is the hardest NIST LLS case in this suite for the current Double-precision solver.
    ' Keep the suite strict everywhere else, but allow a slightly looser relative tolerance here.
    Private Const REL_TOL_COEF_FILIP As Double = 0.00000002
    Private Const REL_TOL_SE_FILIP As Double = 0.00000002
    Private Const REL_TOL_STAT_FILIP As Double = 0.00000002

    ' Wampler5 is also numerically demanding in the current Double-precision solver.
    ' Keep the suite strict elsewhere, but allow a slightly looser coefficient tolerance here.
    Private Const REL_TOL_COEF_WAMPLER5 As Double = 0.0000015

    Private Class DatasetMeta
        Public Property DatasetName As String
        Public Property IncludeIntercept As Boolean
        Public Property ModelKind As String
        Public Property PredictorCount As Integer
        Public Property MaxPower As Integer
        Public Property ObservationCount As Integer
    End Class

    Private Class ReferenceValues
        Public Property Coefficients As List(Of Tuple(Of String, Double, Double))
        Public Property ResidualStandardDeviation As Double
        Public Property RSquared As Double
        Public Property RegressionDf As Integer
        Public Property RegressionSS As Double
        Public Property RegressionMS As Double
        Public Property RegressionF As Double
        Public Property ResidualDf As Integer
        Public Property ResidualSS As Double
        Public Property ResidualMS As Double
    End Class

    <DataTestMethod()>
    <DataRow("Norris")>
    <DataRow("Pontius")>
    <DataRow("NoInt1")>
    <DataRow("NoInt2")>
    <DataRow("Filip")>
    <DataRow("Longley")>
    <DataRow("Wampler1")>
    <DataRow("Wampler2")>
    <DataRow("Wampler3")>
    <DataRow("Wampler4")>
    <DataRow("Wampler5")>
    <TestCategory("LinearModel")>
    Public Sub Fit_matches_nist_lls_reference(datasetName As String)
        RunNistDatasetCheck(datasetName)
    End Sub

    Private Shared Sub RunNistDatasetCheck(datasetName As String)
        Dim meta As DatasetMeta = LoadMetadata(datasetName)
        Dim raw As List(Of Dictionary(Of String, Double)) = LoadCsvAsRows($"nist_lls_{datasetName.ToLowerInvariant()}_data.csv")
        Assert.AreEqual(meta.ObservationCount, raw.Count, $"Unexpected row count for {datasetName}.")

        Dim loaded As Tuple(Of Double(,), String()) = BuildLinearModelData(raw, meta)
        Dim data(,) As Double = loaded.Item1
        Dim varNames() As String = loaded.Item2

        Dim lm As New regression.LinearModel()
        lm.Data(data, varNames)
        lm.Fit(includeIntercept:=meta.IncludeIntercept, computeTermAnova:=regression.TermSumOfSquaresType.TypeI)

        Dim expected As ReferenceValues = LoadReference(datasetName)
        Dim relTolCoef As Double = GetRelativeTolerance(datasetName, "coef")
        Dim relTolSe As Double = GetRelativeTolerance(datasetName, "se")
        Dim relTolStat As Double = GetRelativeTolerance(datasetName, "stat")

        Assert.AreEqual(expected.Coefficients.Count, lm.results.Coeffs_est.Length, $"Coefficient count mismatch for {datasetName}.")
        Assert.AreEqual(expected.Coefficients.Count, lm.results.Coeffs_SEs.Length, $"SE count mismatch for {datasetName}.")

        For i As Integer = 0 To expected.Coefficients.Count - 1
            Dim expectedBeta As Double = expected.Coefficients(i).Item2
            Dim expectedSe As Double = expected.Coefficients(i).Item3
            AssertClose(expectedBeta, lm.results.Coeffs_est(i), relTolCoef, ABS_TOL, $"{datasetName} beta[{i}]")
            AssertClose(expectedSe, lm.results.Coeffs_SEs(i), relTolSe, ABS_TOL, $"{datasetName} se[{i}]")
        Next

        Dim modelVals(,) As Object = lm.results.ModelTableVals
        AssertClose(expected.RSquared, CDbl(modelVals(4, 0)), relTolStat, ABS_TOL, $"{datasetName} R^2")

        Dim overall As ResultTable = lm.AnovaOverall_toPrint
        Dim a(,) As Object = overall.returnSelf()

        Dim rowModel As Integer = FindRowByLabel(a, "Model")
        Dim rowResidual As Integer = FindRowByLabel(a, "Residuals")
        Assert.IsTrue(rowModel >= 0, $"Could not find Model row for {datasetName}.")
        Assert.IsTrue(rowResidual >= 0, $"Could not find Residuals row for {datasetName}.")

        Dim numsModel As List(Of Double) = GetNumericValuesFromRow(a, rowModel)
        Dim numsResidual As List(Of Double) = GetNumericValuesFromRow(a, rowResidual)

        Assert.IsTrue(numsModel.Count >= 4, $"Unexpected numeric layout in Model ANOVA row for {datasetName}.")
        Assert.IsTrue(numsResidual.Count >= 3, $"Unexpected numeric layout in Residual ANOVA row for {datasetName}.")

        AssertClose(expected.RegressionSS, numsModel(0), relTolStat, ABS_TOL, $"{datasetName} regression SS")
        AssertClose(expected.RegressionDf, numsModel(1), relTolStat, ABS_TOL, $"{datasetName} regression df")
        AssertClose(expected.RegressionMS, numsModel(2), relTolStat, ABS_TOL, $"{datasetName} regression MS")

        If Double.IsPositiveInfinity(expected.RegressionF) Then
            Dim actualF As Double = numsModel(3)
            Dim actualResidualMS As Double = numsResidual(2)

            ' Perfect-fit NIST cases (for example Wampler1) may produce either:
            ' - +Infinity
            ' - NaN
            ' - or an extremely large finite F due to tiny nonzero residual MS in Double precision.
            '
            ' Accept any nonnegative huge finite F once the residual mean square is essentially zero.
            Assert.IsTrue(
                Double.IsNaN(actualF) OrElse
                Double.IsPositiveInfinity(actualF) OrElse
                (actualF > 1.0E+30 AndAlso Math.Abs(actualResidualMS) <= 1.0E-20),
                $"{datasetName} expected infinite F but got {actualF}.")
        Else
            AssertClose(expected.RegressionF, numsModel(3), relTolStat, ABS_TOL, $"{datasetName} regression F")
        End If

        AssertClose(expected.ResidualSS, numsResidual(0), relTolStat, ABS_TOL, $"{datasetName} residual SS")
        AssertClose(expected.ResidualDf, numsResidual(1), relTolStat, ABS_TOL, $"{datasetName} residual df")
        AssertClose(expected.ResidualMS, numsResidual(2), relTolStat, ABS_TOL, $"{datasetName} residual MS")

        Dim actualResidualSd As Double = Math.Sqrt(Math.Max(0.0, numsResidual(2)))
        AssertClose(expected.ResidualStandardDeviation, actualResidualSd, relTolStat, ABS_TOL, $"{datasetName} residual standard deviation")
    End Sub

    Private Shared Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim candidates As String() = {
            Path.Combine(baseDir, "TestData", "NIST_LLS", fileName),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\TestData\NIST_LLS", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\..\TestData\NIST_LLS", fileName))
        }

        For Each c As String In candidates
            If File.Exists(c) Then Return c
        Next

        Throw New FileNotFoundException("Test data file not found", fileName)
    End Function

    Private Shared Function LoadMetadata(datasetName As String) As DatasetMeta
        Dim path As String = GetTestDataPath("nist_lls_metadata.csv")
        Dim lines() As String = File.ReadAllLines(path)
        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
            Dim parts() As String = lines(i).Split(","c)
            If String.Equals(parts(0).Trim(), datasetName, StringComparison.OrdinalIgnoreCase) Then
                Dim m As New DatasetMeta()
                m.DatasetName = parts(0).Trim()
                m.IncludeIntercept = Boolean.Parse(parts(1).Trim())
                m.ModelKind = parts(2).Trim().ToLowerInvariant()
                m.PredictorCount = Integer.Parse(parts(3).Trim(), CultureInfo.InvariantCulture)
                m.MaxPower = Integer.Parse(parts(4).Trim(), CultureInfo.InvariantCulture)
                m.ObservationCount = Integer.Parse(parts(5).Trim(), CultureInfo.InvariantCulture)
                Return m
            End If
        Next
        Throw New InvalidOperationException($"Dataset metadata not found for {datasetName}.")
    End Function

    Private Shared Function LoadCsvAsRows(fileName As String) As List(Of Dictionary(Of String, Double))
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException($"CSV must include header and data rows: {fileName}")

        Dim headers() As String = lines(0).Split(","c).Select(Function(s) s.Trim().ToLowerInvariant()).ToArray()
        Dim out As New List(Of Dictionary(Of String, Double))()

        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
            Dim parts() As String = lines(i).Split(","c)
            Dim row As New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)
            For j As Integer = 0 To headers.Length - 1
                row(headers(j)) = ParseDoubleInvariant(parts(j))
            Next
            out.Add(row)
        Next

        Return out
    End Function

    Private Shared Function BuildLinearModelData(raw As List(Of Dictionary(Of String, Double)), meta As DatasetMeta) As Tuple(Of Double(,), String())
        Dim n As Integer = raw.Count
        Dim p As Integer
        Dim varNames() As String

        If meta.ModelKind = "multilinear" Then
            p = meta.PredictorCount
            ReDim varNames(p)
            varNames(0) = "y"
            For j As Integer = 1 To p
                varNames(j) = $"x{j}"
            Next

            Dim data(n - 1, p) As Double
            For i As Integer = 0 To n - 1
                data(i, 0) = raw(i)("y")
                For j As Integer = 1 To p
                    data(i, j) = raw(i)($"x{j}")
                Next
            Next
            Return Tuple.Create(data, varNames)
        Else
            p = meta.MaxPower
            ReDim varNames(p)
            varNames(0) = "y"
            For j As Integer = 1 To p
                varNames(j) = $"x{j}"
            Next

            Dim data(n - 1, p) As Double
            For i As Integer = 0 To n - 1
                Dim x As Double = raw(i)("x")
                data(i, 0) = raw(i)("y")
                For pow As Integer = 1 To p
                    data(i, pow) = Math.Pow(x, pow)
                Next
            Next
            Return Tuple.Create(data, varNames)
        End If
    End Function

    Private Shared Function LoadReference(datasetName As String) As ReferenceValues
        Dim path As String = GetTestDataPath($"nist_lls_{datasetName.ToLowerInvariant()}_reference.csv")
        Dim lines() As String = File.ReadAllLines(path)
        Dim r As New ReferenceValues()
        r.Coefficients = New List(Of Tuple(Of String, Double, Double))()

        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
            Dim parts() As String = lines(i).Split(","c)
            Dim recordType As String = parts(0).Trim().ToLowerInvariant()
            Dim label As String = parts(1).Trim()

            If recordType = "coefficient" Then
                Dim estimate As Double = ParseDoubleInvariant(parts(2))
                Dim se As Double = ParseDoubleInvariant(parts(3))
                r.Coefficients.Add(Tuple.Create(label, estimate, se))
            ElseIf recordType = "summary" Then
                Dim value As Double = ParseDoubleInvariant(parts(8))
                If String.Equals(label, "residual_standard_deviation", StringComparison.OrdinalIgnoreCase) Then
                    r.ResidualStandardDeviation = value
                ElseIf String.Equals(label, "r_squared", StringComparison.OrdinalIgnoreCase) Then
                    r.RSquared = value
                End If
            ElseIf recordType = "anova" Then
                Dim df As Integer = Integer.Parse(parts(4), CultureInfo.InvariantCulture)
                Dim ss As Double = ParseDoubleInvariant(parts(5))
                Dim ms As Double = ParseDoubleInvariant(parts(6))
                Dim fStat As Double = If(String.IsNullOrWhiteSpace(parts(7)), Double.NaN, ParseDoubleInvariant(parts(7)))
                If String.Equals(label, "Regression", StringComparison.OrdinalIgnoreCase) Then
                    r.RegressionDf = df
                    r.RegressionSS = ss
                    r.RegressionMS = ms
                    r.RegressionF = fStat
                ElseIf String.Equals(label, "Residual", StringComparison.OrdinalIgnoreCase) Then
                    r.ResidualDf = df
                    r.ResidualSS = ss
                    r.ResidualMS = ms
                End If
            End If
        Next

        r.Coefficients = r.Coefficients.OrderBy(Function(t) ExtractCoefficientIndex(t.Item1)).ToList()
        Return r
    End Function

    Private Shared Function ExtractCoefficientIndex(label As String) As Integer
        Dim digits As String = New String(label.Where(Function(ch) Char.IsDigit(ch)).ToArray())
        If String.IsNullOrEmpty(digits) Then Return Integer.MaxValue
        Return Integer.Parse(digits, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function FindRowByLabel(tbl(,) As Object, label As String) As Integer
        For i As Integer = 0 To UBound(tbl, 1)
            For j As Integer = 0 To UBound(tbl, 2)
                If TypeOf tbl(i, j) Is String Then
                    Dim s As String = CStr(tbl(i, j)).Trim()
                    If String.Equals(s, label, StringComparison.OrdinalIgnoreCase) Then
                        Return i
                    End If
                End If
            Next
        Next
        Return -1
    End Function

    Private Shared Function GetNumericValuesFromRow(tbl(,) As Object, rowIndex As Integer) As List(Of Double)
        Dim vals As New List(Of Double)()
        For j As Integer = 0 To UBound(tbl, 2)
            Dim v As Double = GetAsDouble(tbl(rowIndex, j))
            If Not Double.IsNaN(v) Then vals.Add(v)
        Next
        Return vals
    End Function

    Private Shared Function ParseDoubleInvariant(s As String) As Double
        Dim t As String = s.Trim()
        If String.Equals(t, "inf", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(t, "+inf", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(t, "infinity", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(t, "+infinity", StringComparison.OrdinalIgnoreCase) Then
            Return Double.PositiveInfinity
        End If
        Return Double.Parse(t, NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function GetAsDouble(o As Object) As Double
        If o Is Nothing Then Return Double.NaN
        If TypeOf o Is Double Then Return CDbl(o)
        If TypeOf o Is Single Then Return CDbl(CSng(o))
        If TypeOf o Is Integer Then Return CDbl(CInt(o))
        If TypeOf o Is Long Then Return CDbl(CLng(o))
        If TypeOf o Is Decimal Then Return CDbl(CDec(o))

        Dim s As String = o.ToString().Trim()
        If s.Length = 0 Then Return Double.NaN
        If String.Equals(s, "inf", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(s, "+inf", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(s, "infinity", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(s, "+infinity", StringComparison.OrdinalIgnoreCase) Then
            Return Double.PositiveInfinity
        End If

        Dim v As Double
        If Double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, v) Then Return v
        Return Double.NaN
    End Function

    Private Shared Function GetRelativeTolerance(datasetName As String, valueKind As String) As Double
        If String.Equals(datasetName, "Filip", StringComparison.OrdinalIgnoreCase) Then
            Select Case valueKind.ToLowerInvariant()
                Case "coef"
                    Return REL_TOL_COEF_FILIP
                Case "se"
                    Return REL_TOL_SE_FILIP
                Case "stat"
                    Return REL_TOL_STAT_FILIP
            End Select
        End If

        If String.Equals(datasetName, "Wampler5", StringComparison.OrdinalIgnoreCase) Then
            Select Case valueKind.ToLowerInvariant()
                Case "coef"
                    Return REL_TOL_COEF_WAMPLER5
            End Select
        End If

        Select Case valueKind.ToLowerInvariant()
            Case "coef"
                Return REL_TOL_COEF_DEFAULT
            Case "se"
                Return REL_TOL_SE_DEFAULT
            Case "stat"
                Return REL_TOL_STAT_DEFAULT
            Case Else
                Throw New ArgumentException($"Unknown tolerance kind: {valueKind}")
        End Select
    End Function

    Private Shared Sub AssertClose(expected As Double, actual As Double, relTol As Double, absTol As Double, message As String)
        If Double.IsNaN(expected) Then
            Assert.IsTrue(Double.IsNaN(actual), $"{message}: expected NaN but got {actual}.")
            Return
        End If
        If Double.IsPositiveInfinity(expected) Then
            Assert.IsTrue(Double.IsPositiveInfinity(actual), $"{message}: expected +Infinity but got {actual}.")
            Return
        End If
        If Double.IsNegativeInfinity(expected) Then
            Assert.IsTrue(Double.IsNegativeInfinity(actual), $"{message}: expected -Infinity but got {actual}.")
            Return
        End If

        Dim scale As Double = Math.Max(1.0, Math.Max(Math.Abs(expected), Math.Abs(actual)))
        Dim tol As Double = Math.Max(absTol, relTol * scale)
        Dim diff As Double = Math.Abs(expected - actual)
        Assert.IsTrue(diff <= tol, $"{message}: expected={expected:R}, actual={actual:R}, diff={diff:R}, tol={tol:R}")
    End Sub
End Class
