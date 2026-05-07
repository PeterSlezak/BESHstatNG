Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.IO
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG

<TestClass()>
Public Class LinearModel_Tests

    Private Const TOL_COEF As Double = 0.0000000001
    Private Const TOL_DIAG As Double = 0.000000001

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

    ''' <summary>
    ''' Loads a CSV with header. Returns (dataMatrix, weights).
    ''' Expected columns: y,x1,x2[,w]
    ''' </summary>
    Private Shared Function LoadLmCsv(fileName As String) As Tuple(Of Double(,), Double())
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must have header + at least one data row.")

        Dim header() As String = lines(0).Split(","c)
        Dim colIndex As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            colIndex(header(i).Trim()) = i
        Next

        If Not colIndex.ContainsKey("y") Then Throw New InvalidOperationException("CSV must include column 'y'.")

        Dim n As Integer = lines.Length - 1
        Dim hasX1 As Boolean = colIndex.ContainsKey("x1")
        Dim hasX2 As Boolean = colIndex.ContainsKey("x2")
        Dim hasW As Boolean = colIndex.ContainsKey("w")

        Dim pPredictors As Integer = 0
        If hasX1 Then pPredictors += 1
        If hasX2 Then pPredictors += 1

        Dim data(n - 1, pPredictors) As Double  ' columns = 1 + pPredictors (y + predictors)
        Dim w() As Double = Nothing
        If hasW Then ReDim w(n - 1)

        For r As Integer = 0 To n - 1
            Dim parts() As String = lines(r + 1).Split(","c)
            data(r, 0) = GetAsDouble(parts(colIndex("y")).Trim())

            Dim c As Integer = 1
            If hasX1 Then
                data(r, c) = GetAsDouble(parts(colIndex("x1")).Trim())
                c += 1
            End If
            If hasX2 Then
                data(r, c) = GetAsDouble(parts(colIndex("x2")).Trim())
                c += 1
            End If

            If hasW Then
                w(r) = GetAsDouble(parts(colIndex("w")).Trim())
            End If
        Next

        Return Tuple.Create(data, w)
    End Function

    Private Shared Sub AssertVectorAlmostEqual(expected() As Double, actual() As Double, tol As Double, Optional msg As String = "")
        Assert.IsNotNull(actual, "actual vector is Nothing. " & msg)
        Assert.AreEqual(expected.Length, actual.Length, "Vector length mismatch. " & msg)
        For i As Integer = 0 To expected.Length - 1
            Dim e As Double = expected(i)
            Dim a As Double = actual(i)
            If Double.IsNaN(e) Then
                Assert.IsTrue(Double.IsNaN(a), $"Expected NaN at {i}. {msg}")
            Else
                Assert.AreEqual(e, a, tol, $"Mismatch at index {i}. {msg}")
            End If
        Next
    End Sub

    Private Shared Sub AssertMatrixAlmostEqual(expected(,) As Double, actual(,) As Double, tol As Double, Optional msg As String = "")
        Assert.IsNotNull(actual, "actual matrix is Nothing. " & msg)
        Assert.AreEqual(UBound(expected, 1), UBound(actual, 1), "RowUdfs count differs. " & msg)
        Assert.AreEqual(UBound(expected, 2), UBound(actual, 2), "ColUdfs count differs. " & msg)

        For i As Integer = 0 To UBound(expected, 1)
            For j As Integer = 0 To UBound(expected, 2)
                Assert.AreEqual(expected(i, j), actual(i, j), tol, $"Mismatch at ({i},{j}). {msg}")
            Next
        Next
    End Sub

    Private Shared Function FindRowByLabel(tbl(,) As Object, label As String) As Integer
        For i As Integer = 0 To UBound(tbl, 1)
            For j As Integer = 0 To UBound(tbl, 2)
                If TypeOf tbl(i, j) Is String Then
                    Dim s As String = CType(tbl(i, j), String)
                    If String.Equals(s.Trim(), label, StringComparison.OrdinalIgnoreCase) Then
                        Return i
                    End If
                End If
            Next
        Next
        Return -1
    End Function

    Private Shared Function ParseDoubleInvariant(s As String) As Double
        Dim v As Double
        If TryParseFirstDouble(s, v) Then Return v
        Return Double.Parse(s, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture)
    End Function

    Private Shared Function TryParseFirstDouble(ByVal s As String, ByRef value As Double) As Boolean
        If s Is Nothing Then Return False
        s = s.Trim()
        If s.Length = 0 Then Return False

        ' Extract first numeric token (supports - + . e E)
        Dim token As String = ""
        Dim started As Boolean = False

        For i As Integer = 0 To s.Length - 1
            Dim ch As Char = s(i)
            Dim isNumChar As Boolean =
                (ch >= "0"c AndAlso ch <= "9"c) OrElse ch = "-"c OrElse ch = "+"c OrElse ch = "."c OrElse
                ch = "e"c OrElse ch = "E"c

            If isNumChar Then
                token &= ch
                started = True
            ElseIf started Then
                Exit For
            End If
        Next

        If token.Length = 0 Then Return False

        Return Double.TryParse(token, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, value)
    End Function

    Private Shared Function GetAsDouble(o As Object) As Double
        If o Is Nothing Then Return Double.NaN

        If TypeOf o Is Double Then Return CDbl(o)
        If TypeOf o Is Single Then Return CDbl(CSng(o))
        If TypeOf o Is Integer Then Return CDbl(CInt(o))
        If TypeOf o Is Long Then Return CDbl(CLng(o))
        If TypeOf o Is Decimal Then Return CDbl(CDec(o))

        Dim s As String = o.ToString()
        Dim v As Double
        If TryParseFirstDouble(s, v) Then Return v

        Return Double.NaN
    End Function

    Private Shared Function IsNumericCell(o As Object) As Boolean
        If o Is Nothing OrElse TypeOf o Is String Then Return False

        If TypeOf o Is Double OrElse TypeOf o Is Single OrElse TypeOf o Is Integer OrElse
           TypeOf o Is Long OrElse TypeOf o Is Decimal Then
            Return True
        End If

        Return Not Double.IsNaN(GetAsDouble(o))
    End Function

    Private Shared Function GetNumericByRowLabelAndColumnHeader(tbl(,) As Object, rowLabel As String, columnHeader As String) As Double
        Dim row As Integer = FindRowByLabel(tbl, rowLabel)
        If row < 0 Then
            Throw New InvalidOperationException($"Could Not find row '{rowLabel}' in ResultTable output.")
        End If

        For i As Integer = 0 To row - 1
            For j As Integer = 0 To UBound(tbl, 2)
                If TypeOf tbl(i, j) Is String AndAlso
                   String.Equals(CType(tbl(i, j), String).Trim(), columnHeader, StringComparison.OrdinalIgnoreCase) AndAlso
                   IsNumericCell(tbl(row, j)) Then
                    Return GetAsDouble(tbl(row, j))
                End If
            Next
        Next

        Throw New InvalidOperationException($"Could Not find numeric value for row '{rowLabel}' and column '{columnHeader}' in ResultTable output.")
    End Function

    ' ---------------------------
    ' Reference values (generated by linear_model_reference.R)
    ' Dataset: lm_dataset_basic.csv
    ' ---------------------------
    Private Shared ReadOnly expBetaOLS() As Double = {3.2950000000000026, 2.005, -1.4450000000000034}
    Private Shared ReadOnly expSeOLS() As Double = {0.50141585250454268, 0.078307999956216859, 0.44984521147358497}
    Private Shared ReadOnly expFittedOLS() As Double = {5.3000000000000025, 5.8599999999999994, 9.3100000000000023, 9.8699999999999974, 13.32, 13.879999999999999, 17.330000000000002, 17.889999999999997, 21.34, 21.899999999999995}
    Private Shared ReadOnly expResidOLS() As Double = {0.19999999999999751, -0.85999999999999943, -0.11000000000000298, 1.3300000000000018, -0.620000000000001, 0.120000000000001, 0.46999999999999886, -0.48999999999999844, 0.059999999999998721, -0.099999999999994316}
    Private Shared ReadOnly expLevOLS() As Double = {0.39999999999999997, 0.4, 0.24999999999999997, 0.25, 0.19999999999999998, 0.2, 0.25, 0.25, 0.39999999999999997, 0.39999999999999997}
    Private Shared ReadOnly expStdResOLS() As Double = {0.36864066858585548, -1.5851548749191975, -0.1813472307303749, 2.192652880649022, -0.98968176997664992, 0.19155131031806255, 0.7748472585752153, -0.80781948234437273, 0.11059220057575567, -0.18432033429291955}
    Private Shared ReadOnly expCookOLS() As Double = {0.030199098341205894, 0.55838132832891041, 0.0036540908992862033, 0.534191850557606, 0.081622500485342836, 0.0030576587070472241, 0.066709808235725171, 0.0725080351172367, 0.0027179188507084821, 0.0075497745853008039}
    Private Shared ReadOnly expJackOLS() As Double = {0.33333333333332915, -1.4333333333333325, -0.14666666666667064, 1.7733333333333359, -0.77500000000000124, 0.15000000000000124, 0.62666666666666515, -0.65333333333333121, 0.099999999999997854, -0.16666666666665717}
    Private Shared ReadOnly expCovOLS(,) As Double = {{0.25141785714285725, -0.030660714285714298, -0.0674535714285714}, {-0.030660714285714298, 0.00613214285714286, -0.0061321428571428655}, {-0.067453571428571446, -0.00613214285714286, 0.20236071428571437}}

    Private Const expR2_OLS As Double = 0.98946043827880426
    Private Const expAdjR2_OLS As Double = 0.98644913492989117
    Private Const expF_OLS As Double = 328.58211997670338
    Private Const expP_OLS As Double = 0.00000012019293216258831

    Private Const expLL_OLS As Double = -8.8450886794529211
    Private Const expAIC_OLS As Double = 23.690177358905842
    Private Const expBIC_OLS As Double = 24.597932637887979
    Private Const expSSE_OLS As Double = 3.4340000000000015
    Private Const expSST_OLS As Double = 325.82

    Private Shared ReadOnly expBetaWLS() As Double = {3.3199999999999839, 2.0000000000000031, -1.4400000000000037}
    Private Shared ReadOnly expSeWLS() As Double = {0.601581249707801, 0.085940178529685857, 0.52275369780751257}
    Private Shared ReadOnly expFittedWLS() As Double = {5.319999999999987, 5.8799999999999857, 9.3199999999999932, 9.8799999999999937, 13.32, 13.879999999999999, 17.320000000000004, 17.880000000000003, 21.320000000000011, 21.88000000000001}
    Private Shared ReadOnly expResidWLS() As Double = {0.18000000000001304, -0.87999999999998568, -0.11999999999999389, 1.3200000000000056, -0.620000000000001, 0.120000000000001, 0.47999999999999687, -0.480000000000004, 0.079999999999987637, -0.080000000000008953}
    Private Shared ReadOnly expLevWLS() As Double = {0.33333333333333354, 0.46666666666666695, 0.23333333333333348, 0.26666666666666683, 0.20000000000000007, 0.2, 0.23333333333333334, 0.26666666666666672, 0.33333333333333337, 0.46666666666666684}
    Private Shared ReadOnly expStdResWLS() As Double = {0.23417000222483872, -1.8101369408640748, -0.14557643534685344, 2.3155349549638915, -0.7363085125549389, 0.20154144862179907, 0.58230574138743951, -0.84201271089596386, 0.10407555654434913, -0.16455790371493698}
    Private Shared ReadOnly expCookWLS() As Double = {0.0091392649903301679, 0.95567375886522055, 0.0021499636188126994, 0.64990328820116772, 0.0451791854717389, 0.0033849129593811048, 0.034399417901006209, 0.085937624886105457, 0.0018052869116693354, 0.0078981302385575554}
    Private Shared ReadOnly expJackWLS() As Double = {0.27000000000001961, -1.6499999999999742, -0.15652173913042686, 1.800000000000008, -0.77500000000000135, 0.15000000000000124, 0.62608695652173507, -0.65454545454546, 0.11999999999998145, -0.15000000000001681}
    Private Shared ReadOnly expCovWLS(,) As Double = {{0.36189999999999967, -0.0369285714285714, -0.14032857142857119}, {-0.036928571428571394, 0.0073857142857142767, -0.0073857142857142758}, {-0.14032857142857127, -0.007385714285714268, 0.27327142857142817}}

    Private Const expR2_WLS As Double = 0.987267299151427
    Private Const expAdjR2_WLS As Double = 0.98362938462326321
    Private Const expF_WLS As Double = 271.38276380829615
    Private Const expP_WLS As Double = 0.00000023292809481212373

    Private Const expLL_WLS As Double = -11.802431093647954
    Private Const expAIC_WLS As Double = 29.604862187295907
    Private Const expBIC_WLS As Double = 30.512617466278044
    Private Const expSSE_WLS As Double = 6.20399999999999
    Private Const expSST_WLS As Double = 487.24933333333331

    ' No-intercept OLS reference (same dataset)
    Private Shared ReadOnly expBetaNoIntercept() As Double = {2.4068292682926828, -0.56097560975609873}
    Private Const expR2_NoIntercept As Double = 0.988683362741728
    Private Const expAdjR2_NoIntercept As Double = 0.98585420342716
    Private Const expF_NoIntercept As Double = 349.46189055199568
    Private Const expP_NoIntercept As Double = 0.000000016400971802887909

    Private Const expTypeI_x1_SS_OLS As Double = 317.32412121212127
    Private Const expTypeI_x1_F_OLS As Double = 646.84590812022361
    Private Const expTypeI_x1_P_OLS As Double = 0.000000037104584516001182

    Private Const expTypeI_x2_SS_OLS As Double = 5.0618787878787828
    Private Const expTypeI_x2_F_OLS As Double = 10.318331833183304
    Private Const expTypeI_x2_P_OLS As Double = 0.014812460574519903

    Private Const expTypeIII_x1_SS_OLS As Double = 321.60200000000003
    Private Const expTypeIII_x1_F_OLS As Double = 655.56610366919028
    Private Const expTypeIII_x1_P_OLS As Double = 0.00000003542136128853457

    Private Const expTypeIII_x2_SS_OLS As Double = 5.0618787878787828
    Private Const expTypeIII_x2_F_OLS As Double = 10.318331833183304
    Private Const expTypeIII_x2_P_OLS As Double = 0.014812460574519903

    Private Const expVIF_x1_OLS As Double = 1.0312500000000002
    Private Const expVIF_x2_OLS As Double = 1.03125

    Private Const expTypeIII_x1_SS_WLS As Double = 480.00000000000006
    Private Const expTypeIII_x1_F_WLS As Double = 541.586073500968
    Private Const expTypeIII_x1_P_WLS As Double = 0.000000068641247796819016

    Private Const expTypeIII_x2_SS_WLS As Double = 6.7251891891891793
    Private Const expTypeIII_x2_F_WLS As Double = 7.5880600135919289
    Private Const expTypeIII_x2_P_WLS As Double = 0.028313761278172422

    Private Const expVIF_x1_WLS As Double = 1.0277777777777777
    Private Const expVIF_x2_WLS As Double = 1.0277777777777779
    Private Const expPartialR_x1_WLS As Double = 0.993599478654516
    Private Const expPartialR_x2_WLS As Double = -0.72121808414412869

    ' ---------------------------
    ' OLS fit
    ' ---------------------------
    <TestCategory("LinearModel")>
    <TestMethod()>
    Public Sub Fit_OLS_matches_reference_basic()

        Dim loaded = LoadLmCsv("lm_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1

        Dim lm As New regression.LinearModel()
        lm.Data(data, New String() {"y", "x1", "x2"})
        lm.Fit(includeIntercept:=True, computeTermAnova:=regression.TermSumOfSquaresType.TypeIII)

        AssertVectorAlmostEqual(expBetaOLS, lm.results.Coeffs_est, TOL_COEF, "beta (OLS)")
        AssertVectorAlmostEqual(expSeOLS, lm.results.Coeffs_SEs, 0.000000001, "SE (OLS)")

        AssertVectorAlmostEqual(expFittedOLS, lm.Fitted, TOL_DIAG, "fitted (OLS)")
        AssertVectorAlmostEqual(expResidOLS, lm.Residuals, TOL_DIAG, "residuals (OLS)")

        AssertMatrixAlmostEqual(expCovOLS, lm.Covariance, 0.00000001, "Covariance (OLS)")

        Dim mv As Object(,) = lm.results.ModelTableVals
        Assert.AreEqual(expR2_OLS, CDbl(mv(4, 0)), 0.000000000001, "R2")
        Assert.AreEqual(expAdjR2_OLS, CDbl(mv(5, 0)), 0.000000000001, "AdjR2")
        Assert.AreEqual(expF_OLS, CDbl(mv(6, 0)), 0.0000000001, "Overall F")
        Assert.AreEqual(expP_OLS, CDbl(mv(6, 2)), 0.000000000001, "Overall F p")

        Assert.AreEqual(expLL_OLS, CDbl(mv(7, 0)), 0.0000000001, "logLik")
        Assert.AreEqual(expAIC_OLS, CDbl(mv(8, 0)), 0.0000000001, "AIC")
        Assert.AreEqual(expBIC_OLS, CDbl(mv(9, 0)), 0.0000000001, "BIC")

        ' SSE/SST cross-check
        Dim sseCalc As Double = lm.Residuals.Select(Function(r) r * r).Sum()
        Assert.AreEqual(expSSE_OLS, sseCalc, 0.00000001, "SSE")

        Dim diag(,) As Object = lm.AllResiduals_toPrint
        Assert.IsNotNull(diag, "AllResiduals_toPrint returned Nothing (OLS).")

        ' Detect header row in ResultTable output.
        Dim rowOffset As Integer = 0
        If diag IsNot Nothing AndAlso diag.GetLength(0) > 0 Then
            If TypeOf diag(0, 0) Is String AndAlso CStr(diag(0, 0)).IndexOf("Fitted", StringComparison.OrdinalIgnoreCase) >= 0 Then
                rowOffset = 1
            ElseIf diag.GetLength(1) > 2 AndAlso Double.IsNaN(GetAsDouble(diag(0, 2))) Then
                rowOffset = 1
            End If
        End If

        For i As Integer = 0 To expFittedOLS.Length - 1
            Assert.AreEqual(expLevOLS(i), GetAsDouble(diag(i + rowOffset, 2)), 0.00000001, $"diag leverage row {i}")
            Assert.AreEqual(expStdResOLS(i), GetAsDouble(diag(i + rowOffset, 3)), 0.00000001, $"diag stdres row {i}")
            Assert.AreEqual(expCookOLS(i), GetAsDouble(diag(i + rowOffset, 4)), 0.00000001, $"diag cooks row {i}")
            Assert.AreEqual(expJackOLS(i), GetAsDouble(diag(i + rowOffset, 5)), 0.00000001, $"diag jack row {i}")
        Next
    End Sub

    ' ---------------------------
    ' WLS fit
    ' ---------------------------
    <TestCategory("LinearModel")>
    <TestMethod()>
    Public Sub Fit_WLS_matches_reference_basic()

        Dim loaded = LoadLmCsv("lm_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim w() As Double = loaded.Item2

        Dim lm As New regression.LinearModel()
        lm.Data(data, New String() {"y", "x1", "x2"}, weights:=w)
        lm.Fit(includeIntercept:=True, computeTermAnova:=regression.TermSumOfSquaresType.TypeIII)

        AssertVectorAlmostEqual(expBetaWLS, lm.results.Coeffs_est, TOL_COEF, "beta (WLS)")
        AssertVectorAlmostEqual(expSeWLS, lm.results.Coeffs_SEs, 0.00000001, "SE (WLS)")

        AssertVectorAlmostEqual(expFittedWLS, lm.Fitted, 0.00000001, "fitted (WLS)")
        AssertVectorAlmostEqual(expResidWLS, lm.Residuals, 0.00000001, "residuals (WLS)")

        AssertMatrixAlmostEqual(expCovWLS, lm.Covariance, 0.0000001, "Covariance (WLS)")

        Dim mv As Object(,) = lm.results.ModelTableVals
        Assert.AreEqual(expR2_WLS, CDbl(mv(4, 0)), 0.000000000001, "R2")
        Assert.AreEqual(expAdjR2_WLS, CDbl(mv(5, 0)), 0.000000000001, "AdjR2")
        Assert.AreEqual(expF_WLS, CDbl(mv(6, 0)), 0.0000000001, "Overall F")
        Assert.AreEqual(expP_WLS, CDbl(mv(6, 2)), 0.000000000001, "Overall F p")

        Assert.AreEqual(expLL_WLS, CDbl(mv(7, 0)), 0.0000000001, "logLik")
        Assert.AreEqual(expAIC_WLS, CDbl(mv(8, 0)), 0.0000000001, "AIC")
        Assert.AreEqual(expBIC_WLS, CDbl(mv(9, 0)), 0.0000000001, "BIC")

        ' SSE cross-check (weighted)
        Dim sseCalc As Double = 0.0
        For i As Integer = 0 To w.Length - 1
            sseCalc += w(i) * lm.Residuals(i) * lm.Residuals(i)
        Next
        Assert.AreEqual(expSSE_WLS, sseCalc, 0.00000001, "Weighted SSE")

        Dim diag(,) As Object = lm.AllResiduals_toPrint
        Assert.IsNotNull(diag, "AllResiduals_toPrint returned Nothing (WLS).")

        ' ResultTable.returnSelf() includes a header top row; detect and skip it.
        Dim rowOffset As Integer = 0
        If diag.GetLength(0) > 0 Then
            If TypeOf diag(0, 0) Is String AndAlso CStr(diag(0, 0)).IndexOf("Fitted", StringComparison.OrdinalIgnoreCase) >= 0 Then
                rowOffset = 1
            ElseIf diag.GetLength(1) > 2 AndAlso Double.IsNaN(GetAsDouble(diag(0, 2))) Then
                rowOffset = 1
            End If
        End If

        For i As Integer = 0 To expFittedWLS.Length - 1
            Assert.AreEqual(expLevWLS(i), GetAsDouble(diag(i + rowOffset, 2)), 0.00000001, $"diag leverage row {i} (WLS)")
            Assert.AreEqual(expStdResWLS(i), GetAsDouble(diag(i + rowOffset, 3)), 0.00000001, $"diag stdres row {i} (WLS)")
            Assert.AreEqual(expCookWLS(i), GetAsDouble(diag(i + rowOffset, 4)), 0.00000001, $"diag cooks row {i} (WLS)")
            Assert.AreEqual(expJackWLS(i), GetAsDouble(diag(i + rowOffset, 5)), 0.00000001, $"diag jack row {i} (WLS)")
        Next
    End Sub

    ' ---------------------------
    ' Term-wise ANOVA + VIF (parsing ResultTable.returnSelf() in a layout-tolerant way)
    ' ---------------------------
    <TestCategory("LinearModel")>
    <TestMethod()>
    Public Sub TermAnova_TypeI_matches_reference_OLS()

        Dim loaded = LoadLmCsv("lm_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1

        Dim lm As New regression.LinearModel()
        lm.Data(data, New String() {"y", "x1", "x2"})
        lm.Fit(includeIntercept:=True, computeTermAnova:=regression.TermSumOfSquaresType.TypeI)

        Dim rt As ResultTable = lm.AnovaTypeI_toPrint
        Dim a(,) As Object = rt.returnSelf()

        Dim rowX1 As Integer = FindRowByLabel(a, "x1")
        Dim rowX2 As Integer = FindRowByLabel(a, "x2")
        Assert.IsTrue(rowX1 >= 0 AndAlso rowX2 >= 0, "Could not find x1/x2 rows in Type I ANOVA output.")

        Dim numsX1 As New List(Of Double)
        For j As Integer = 0 To UBound(a, 2)
            If a(rowX1, j) IsNot Nothing AndAlso Not TypeOf a(rowX1, j) Is String Then numsX1.Add(GetAsDouble(a(rowX1, j)))
        Next
        Dim numsX2 As New List(Of Double)
        For j As Integer = 0 To UBound(a, 2)
            If a(rowX2, j) IsNot Nothing AndAlso Not TypeOf a(rowX2, j) Is String Then numsX2.Add(GetAsDouble(a(rowX2, j)))
        Next
        Assert.IsTrue(numsX1.Count >= 5 AndAlso numsX2.Count >= 5, "Unexpected Type I ANOVA numeric layout.")

        Assert.AreEqual(expTypeI_x1_SS_OLS, numsX1(1), 0.00000001, "Type I SS (x1)")
        Assert.AreEqual(expTypeI_x1_F_OLS, numsX1(3), 0.00000001, "Type I F (x1)")
        Assert.AreEqual(expTypeI_x1_P_OLS, numsX1(4), 0.00000001, "Type I p (x1)")

        Assert.AreEqual(expTypeI_x2_SS_OLS, numsX2(1), 0.00000001, "Type I SS (x2)")
        Assert.AreEqual(expTypeI_x2_F_OLS, numsX2(3), 0.00000001, "Type I F (x2)")
        Assert.AreEqual(expTypeI_x2_P_OLS, numsX2(4), 0.00000001, "Type I p (x2)")
    End Sub

    <TestCategory("LinearModel")>
    <TestMethod()>
    Public Sub TermAnova_TypeIII_and_VIF_match_reference_WLS()

        Dim loaded = LoadLmCsv("lm_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim w() As Double = loaded.Item2

        Dim lm As New regression.LinearModel()
        lm.Data(data, New String() {"y", "x1", "x2"}, weights:=w)
        lm.Fit(includeIntercept:=True, computeTermAnova:=regression.TermSumOfSquaresType.TypeIII)

        Dim rt As ResultTable = lm.AnovaTypeIII_toPrint
        Dim a(,) As Object = rt.returnSelf()

        Dim rowX1 As Integer = FindRowByLabel(a, "x1")
        Dim rowX2 As Integer = FindRowByLabel(a, "x2")
        Assert.IsTrue(rowX1 >= 0 AndAlso rowX2 >= 0, "Could not find x1/x2 rows in Type III ANOVA output.")

        Dim numsX1 As New List(Of Double)
        For j As Integer = 0 To UBound(a, 2)
            If a(rowX1, j) IsNot Nothing AndAlso Not TypeOf a(rowX1, j) Is String Then numsX1.Add(GetAsDouble(a(rowX1, j)))
        Next
        Dim numsX2 As New List(Of Double)
        For j As Integer = 0 To UBound(a, 2)
            If a(rowX2, j) IsNot Nothing AndAlso Not TypeOf a(rowX2, j) Is String Then numsX2.Add(GetAsDouble(a(rowX2, j)))
        Next
        Assert.IsTrue(numsX1.Count >= 5 AndAlso numsX2.Count >= 5, "Unexpected Type III ANOVA numeric layout.")

        Assert.AreEqual(expTypeIII_x1_SS_WLS, numsX1(1), 0.0000001, "Type III SS (x1) WLS")
        Assert.AreEqual(expTypeIII_x1_F_WLS, numsX1(3), 0.0000001, "Type III F (x1) WLS")
        Assert.AreEqual(expTypeIII_x1_P_WLS, numsX1(4), 0.0000001, "Type III p (x1) WLS")

        Assert.AreEqual(expTypeIII_x2_SS_WLS, numsX2(1), 0.0000001, "Type III SS (x2) WLS")
        Assert.AreEqual(expTypeIII_x2_F_WLS, numsX2(3), 0.0000001, "Type III F (x2) WLS")
        Assert.AreEqual(expTypeIII_x2_P_WLS, numsX2(4), 0.0000001, "Type III p (x2) WLS")

        Dim vifT As ResultTable = lm.VIF_toPrint
        Dim v(,) As Object = vifT.returnSelf()
        Dim r1 As Integer = FindRowByLabel(v, "x1")
        Dim r2 As Integer = FindRowByLabel(v, "x2")
        Assert.IsTrue(r1 >= 0 AndAlso r2 >= 0, "Could not find x1/x2 rows in VIF output.")

        Dim vif1 As Double = GetNumericByRowLabelAndColumnHeader(v, "x1", "VIF")
        Dim vif2 As Double = GetNumericByRowLabelAndColumnHeader(v, "x2", "VIF")
        Dim partialR1 As Double = GetNumericByRowLabelAndColumnHeader(v, "x1", "Partial r")
        Dim partialR2 As Double = GetNumericByRowLabelAndColumnHeader(v, "x2", "Partial r")

        Assert.AreEqual(expVIF_x1_WLS, vif1, 0.0000001, "VIF x1 WLS")
        Assert.AreEqual(expVIF_x2_WLS, vif2, 0.0000001, "VIF x2 WLS")
        Assert.AreEqual(expPartialR_x1_WLS, partialR1, 0.0000001, "Partial r x1 WLS")
        Assert.AreEqual(expPartialR_x2_WLS, partialR2, 0.0000001, "Partial r x2 WLS")
    End Sub



    <TestCategory("LinearModel")>
    <TestMethod()>
    Public Sub Fit_NoIntercept_matches_reference()

        Dim loaded = LoadLmCsv("lm_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1

        Dim lm As New regression.LinearModel()
        lm.Data(data, New String() {"y", "x1", "x2"})
        lm.Fit(includeIntercept:=False, computeTermAnova:=regression.TermSumOfSquaresType.TypeI)

        AssertVectorAlmostEqual(expBetaNoIntercept, lm.results.Coeffs_est, 0.000000001, "beta (no intercept)")
        Dim mv As Object(,) = lm.results.ModelTableVals
        Assert.AreEqual(expR2_NoIntercept, CDbl(mv(4, 0)), 0.000000000001, "R2 (no intercept)")
        Assert.AreEqual(expAdjR2_NoIntercept, CDbl(mv(5, 0)), 0.000000000001, "AdjR2 (no intercept)")
        Assert.AreEqual(expF_NoIntercept, CDbl(mv(6, 0)), 0.0000000001, "Overall F (no intercept)")
        Assert.AreEqual(expP_NoIntercept, CDbl(mv(6, 2)), 0.000000000001, "Overall F p (no intercept)")
    End Sub
    ' ---------------------------
    ' Edge cases
    ' ---------------------------
    <TestCategory("LinearModel")>
    <TestMethod()>
    Public Sub Fit_throws_if_Data_not_called()
        Dim lm As New regression.LinearModel()
        Assert.ThrowsException(Of InvalidOperationException)(Sub() lm.Fit())
    End Sub

    <TestCategory("LinearModel")>
    <TestMethod()>
    Public Sub Data_throws_if_varNames_length_mismatch()
        Dim loaded = LoadLmCsv("lm_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim lm As New regression.LinearModel()
        Assert.ThrowsException(Of ArgumentException)(Sub() lm.Data(data, New String() {"y", "x1"}))
    End Sub

    <TestCategory("LinearModel")>
    <TestMethod()>
    Public Sub Data_throws_if_weights_length_mismatch()
        Dim loaded = LoadLmCsv("lm_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim lm As New regression.LinearModel()
        Dim badW() As Double = New Double() {1, 1, 1}
        Assert.ThrowsException(Of ArgumentException)(Sub() lm.Data(data, New String() {"y", "x1", "x2"}, weights:=badW))
    End Sub

    <TestCategory("LinearModel")>
    <TestMethod()>
    Public Sub Fit_throws_if_any_weight_nonpositive()
        Dim loaded = LoadLmCsv("lm_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim w() As Double = loaded.Item2
        w(0) = 0.0
        Dim lm As New regression.LinearModel()
        lm.Data(data, New String() {"y", "x1", "x2"}, weights:=w)
        Assert.ThrowsException(Of ArgumentException)(Sub() lm.Fit())
    End Sub

End Class
