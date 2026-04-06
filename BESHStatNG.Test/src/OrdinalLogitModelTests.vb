Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.IO
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG

<TestClass()>
Public Class OrdinalLogitModel_Tests

    Private Const TOL_COEF As Double = 0.000001
    Private Const TOL_DIAG As Double = 0.000001

    ' ---------------------------
    ' Helpers (mirrors style in other test files)
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

    Private Shared Function GetAsDouble(o As Object) As Double
        If o Is Nothing Then Return Double.NaN

        If TypeOf o Is Double Then Return CDbl(o)
        If TypeOf o Is Single Then Return CDbl(CSng(o))
        If TypeOf o Is Integer Then Return CDbl(CInt(o))
        If TypeOf o Is Long Then Return CDbl(CLng(o))
        If TypeOf o Is Decimal Then Return CDbl(CDec(o))

        Dim s As String = o.ToString().Trim()
        Dim v As Double
        If Double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, v) Then Return v

        ' Try extracting first numeric token
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
        If token.Length > 0 AndAlso Double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, v) Then Return v

        Return Double.NaN
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

    ''' <summary>
    ''' Loads ordinal CSV with header.
    ''' Expected columns: y[,x1,x2][,offset][,w]
    ''' Returns (dataMatrix, varNames, offset, weights)
    ''' dataMatrix columns are: y + predictors (if present).
    ''' </summary>
    Private Shared Function LoadOrdinalCsv(fileName As String) As Tuple(Of Double(,), String(), Double(), Double())
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
        Dim hasOffset As Boolean = colIndex.ContainsKey("offset")
        Dim hasW As Boolean = colIndex.ContainsKey("w")

        Dim pPredictors As Integer = 0
        If hasX1 Then pPredictors += 1
        If hasX2 Then pPredictors += 1

        Dim data(n - 1, pPredictors) As Double ' y + predictors
        Dim names(pPredictors) As String
        names(0) = "y"
        Dim cName As Integer = 1
        If hasX1 Then
            names(cName) = "x1"
            cName += 1
        End If
        If hasX2 Then
            names(cName) = "x2"
            cName += 1
        End If

        Dim offset() As Double = Nothing
        If hasOffset Then ReDim offset(n - 1)

        Dim w() As Double = Nothing
        If hasW Then ReDim w(n - 1)

        For r As Integer = 0 To n - 1
            Dim parts() As String = lines(r + 1).Split(","c)

            data(r, 0) = Double.Parse(parts(colIndex("y")).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)

            Dim c As Integer = 1
            If hasX1 Then
                data(r, c) = Double.Parse(parts(colIndex("x1")).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
                c += 1
            End If
            If hasX2 Then
                data(r, c) = Double.Parse(parts(colIndex("x2")).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
                c += 1
            End If

            If hasOffset Then
                offset(r) = Double.Parse(parts(colIndex("offset")).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
            End If
            If hasW Then
                w(r) = Double.Parse(parts(colIndex("w")).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
            End If
        Next

        Return Tuple.Create(data, names, offset, w)
    End Function

    Private Shared Function ExpandWeightedRows(data(,) As Double, offset() As Double, w() As Double) As Tuple(Of Double(,), Double())
        ' Expands a weighted dataset into repeated rows (weights -> replication), leaving offset replicated.
        ' This is used to validate that case weights behave like frequency weights.
        Dim n As Integer = UBound(data, 1) + 1
        Dim cols As Integer = UBound(data, 2) + 1

        Dim total As Integer = 0
        For i As Integer = 0 To n - 1
            total += CInt(Math.Round(w(i)))
        Next

        Dim out(total - 1, cols - 1) As Double
        Dim outOff(total - 1) As Double

        Dim idx As Integer = 0
        For i As Integer = 0 To n - 1
            Dim reps As Integer = CInt(Math.Round(w(i)))
            For r As Integer = 1 To reps
                For c As Integer = 0 To cols - 1
                    out(idx, c) = data(i, c)
                Next
                outOff(idx) = If(offset Is Nothing, 0.0, offset(i))
                idx += 1
            Next
        Next

        Return Tuple.Create(out, outOff)
    End Function

    Private Shared Function FilterRowsByPositiveWeights(data(,) As Double,
                                                   offset() As Double,
                                                   weights() As Double) As Tuple(Of Double(,), Double(), Double())
        Dim n As Integer = UBound(data, 1) + 1
        Dim cols As Integer = UBound(data, 2) + 1

        Dim keepCount As Integer = 0
        For i As Integer = 0 To n - 1
            If weights(i) > 0.0 Then keepCount += 1
        Next

        Dim out(keepCount - 1, cols - 1) As Double
        Dim outOff(keepCount - 1) As Double
        Dim outW(keepCount - 1) As Double

        Dim r As Integer = 0
        For i As Integer = 0 To n - 1
            If weights(i) <= 0.0 Then Continue For
            For c As Integer = 0 To cols - 1
                out(r, c) = data(i, c)
            Next
            outOff(r) = If(offset Is Nothing, 0.0, offset(i))
            outW(r) = weights(i)
            r += 1
        Next

        Return Tuple.Create(out, outOff, outW)
    End Function

    ' ---------------------------
    ' Reference values (generated by ordinal_logit_reference.R)
    ' Dataset: ordinal_logit_dataset_basic.csv
    ' Model: proportional-odds cumulative logit with offset + frequency weights
    ' Reference category option: Last
    ' ---------------------------
    Private Shared ReadOnly expCoeffs() As Double = {
        0.89159224465025178,
        -0.59831812067236578,
        -0.39537055623664025,
        0.99788228556982417
    }

    Private Shared ReadOnly expSE() As Double = {
        0.1114019164350504,
        0.17548091605851643,
        0.130189347499541,
        0.13720140915275839
    }

    Private Const expLL As Double = -482.07665969418133
    Private Const expLL0 As Double = -521.56352225997693
    Private Const expAIC As Double = 972.15331938836266
    Private Const expBIC As Double = 988.84846380397039

    Private Const expCoxSnellR2 As Double = 0.15170649952729132
    Private Const expNagelkerkeR2 As Double = 0.17119054372123813
    Private Const expMcFaddenR2 As Double = 0.075708635440407734

    Private Const expModelChi2 As Double = 78.973725131591209
    Private Const expModelDF As Integer = 2

    Private Const expGOF As Double = 0.055689874486915869
    Private Const expGOFDF As Integer = 8

    Private Const expOverallAcc As Double = 0.49791666666666667

    Private Shared ReadOnly expConf(,) As Double = {
        {175.0, 0.0, 44.0},
        {88.0, 0.0, 52.0},
        {57.0, 0.0, 64.0}
    }

    Private Shared ReadOnly expProbs(,) As Double = {
        {0.59777954263, 0.259083295827, 0.143137161543},
        {0.59777954263, 0.259083295827, 0.143137161543},
        {0.59777954263, 0.259083295827, 0.143137161543},
        {0.688805283058, 0.210342633711, 0.100852083231},
        {0.688805283058, 0.210342633711, 0.100852083231},
        {0.688805283058, 0.210342633711, 0.100852083231},
        {0.402425119669, 0.328216886474, 0.269357993857},
        {0.402425119669, 0.328216886474, 0.269357993857},
        {0.402425119669, 0.328216886474, 0.269357993857},
        {0.500736892792, 0.300843369103, 0.198419738105},
        {0.500736892792, 0.300843369103, 0.198419738105},
        {0.500736892792, 0.300843369103, 0.198419738105},
        {0.233802575583, 0.317587816661, 0.448609607756},
        {0.233802575583, 0.317587816661, 0.448609607756},
        {0.233802575583, 0.317587816661, 0.448609607756},
        {0.312459755001, 0.334250119829, 0.35329012517},
        {0.312459755001, 0.334250119829, 0.35329012517},
        {0.312459755001, 0.334250119829, 0.35329012517}
    }

    Private Shared ReadOnly expDevRes() As Double = {
        7.028171316415,
        7.531629139433,
        6.539643936248,
        6.403720183994,
        7.280562782193,
        6.058515158411,
        7.632546046506,
        7.611320101164,
        7.597066149231,
        7.438679905537,
        7.593151199619,
        7.194154475352,
        7.431323701496,
        7.572981283574,
        7.597062688256,
        7.626531308553,
        7.692642508378,
        7.633221948794
    }

    Private Shared ReadOnly expLev() As Double = {
        0.32496705888,
        0.337175519243,
        0.047219097658,
        0.35359060616,
        0.25467224143,
        0.028468920823,
        0.130430240224,
        0.332119267047,
        0.081503137916,
        0.168833815016,
        0.301936320726,
        0.053866396569,
        0.104021640716,
        0.431572599528,
        0.244616252986,
        0.153577461345,
        0.472865900862,
        0.1785635197
    }

    Private Shared ReadOnly expStdDevRes() As Double = {
        8.55420899064,
        9.251020814107,
        6.699734330325,
        7.964866788479,
        8.433178698078,
        6.146641017252,
        8.184972386454,
        9.313448755086,
        7.92696735049,
        8.159281903413,
        9.088129920611,
        7.396112787995,
        7.850863371625,
        10.04452446908,
        8.741015529671,
        8.289597565709,
        10.59534237806,
        8.422109767641
    }

    ' ---------------------------
    ' Core fit matches numeric reference
    ' ---------------------------
    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Fit_weighted_with_offset_matches_reference_basic()

        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        Dim m As New regression.OrdinalLogitModel() With {
            .bComputeResiduals = True,
            .bReturnCov = True,
            .bIterationDetails = True
        }
        m.Data(data, names, offset:=offs, weights:=w)
        m.Fit(regression.ReferenceCategory.Last)

        AssertVectorAlmostEqual(expCoeffs, m.results.Coeffs_est, TOL_COEF, "Coefficients")
        AssertVectorAlmostEqual(expSE, m.results.Coeffs_SEs, 0.00001, "Std. errors")

        ' Model diagnostics table values are stored as strings/doubles in ModelTableVals
        Dim mv As Object(,) = m.results.ModelTableVals
        Assert.AreEqual(expLL0, CDbl(mv(0, 0)), 0.0000001, "Null log-likelihood")
        Assert.AreEqual(expLL, CDbl(mv(1, 0)), 0.0000001, "Final log-likelihood")

        Assert.AreEqual(expModelChi2, CDbl(mv(3, 0)), 0.000001, "LR model chi-square")
        Assert.AreEqual(expModelDF, CInt(mv(3, 1)), "LR df")

        Assert.AreEqual(expGOF, CDbl(mv(4, 0)), 0.000001, "GOF deviance")
        Assert.AreEqual(expGOFDF, CInt(mv(4, 1)), "GOF df")

        Assert.AreEqual(expCoxSnellR2, CDbl(mv(5, 0)), 0.0000001, "Cox-Snell R2")
        Assert.AreEqual(expNagelkerkeR2, CDbl(mv(6, 0)), 0.0000001, "Nagelkerke R2")
        Assert.AreEqual(expMcFaddenR2, CDbl(mv(7, 0)), 0.0000001, "McFadden R2")

        Assert.AreEqual(expAIC, CDbl(mv(8, 0)), 0.000001, "AIC")
        Assert.AreEqual(expBIC, CDbl(mv(9, 0)), 0.000001, "BIC")

        ' P-values are computed via ChiSquareCDF in the library; compare against expected stat/df.
        Dim expModelP As Double = 1.0 - distributions.ChiSquareCDF(expModelChi2, expModelDF)
        Assert.AreEqual(expModelP, CDbl(mv(3, 2)), 0.0000001, "LR p-value")

        Dim expGofP As Double = 1.0 - distributions.ChiSquareCDF(expGOF, expGOFDF)
        Assert.AreEqual(expGofP, CDbl(mv(4, 2)), 0.0000001, "GOF p-value")

        ' Residuals / probabilities matrix match reference
        Assert.IsNotNull(m.Residuals, "Residuals were not computed.")
        AssertMatrixAlmostEqual(expProbs, m.Residuals.Probabilities, 0.000001, "Fitted probabilities")

        AssertVectorAlmostEqual(expDevRes, m.Residuals.DevianceResiduals, 0.00001, "Deviance residuals")
        AssertVectorAlmostEqual(expLev, m.Residuals.Leverage, 0.00001, "Leverage")
        AssertVectorAlmostEqual(expStdDevRes, m.Residuals.StdDevianceResiduals, 0.0001, "Std deviance residuals")

        ' Classification
        Assert.IsNotNull(m.Classification, "Classification table not computed.")
        Assert.AreEqual(expOverallAcc, m.Classification.OverallAccuracy, 0.0000001, "Overall accuracy")
        AssertMatrixAlmostEqual(expConf, m.Classification.Counts, 0.0000001, "Confusion matrix")
    End Sub

    ' ---------------------------
    ' Residual invariants / shapes
    ' ---------------------------
    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Residuals_have_expected_shapes_and_sum_constraints()

        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        Dim m As New regression.OrdinalLogitModel() With {.bComputeResiduals = True}
        m.Data(data, names, offset:=offs, weights:=w)
        m.Fit(regression.ReferenceCategory.Last)

        Dim res As regression.MultinomialResiduals = m.Residuals
        Assert.IsNotNull(res)
        Assert.AreEqual(3, res.Categories.Length, "K categories")

        Dim n As Integer = UBound(data, 1) + 1
        Assert.AreEqual(n, UBound(res.Probabilities, 1) + 1, "Probabilities rows")
        Assert.AreEqual(3, UBound(res.Probabilities, 2) + 1, "Probabilities cols")

        For i As Integer = 0 To n - 1
            Dim sumP As Double = 0.0
            Dim sumObs As Double = 0.0
            Dim sumMu As Double = 0.0
            Dim sumResp As Double = 0.0

            For k As Integer = 0 To 2
                Dim pik As Double = res.Probabilities(i, k)
                Assert.IsTrue(pik >= 0.0 AndAlso pik <= 1.0, $"probability bounds at row {i}, k={k}")
                sumP += pik

                sumObs += res.Observed(i, k)
                sumMu += res.FittedMeans(i, k)
                sumResp += res.ResponseResiduals(i, k)
            Next

            Assert.AreEqual(1.0, sumP, 0.0000001, $"probabilities sum to 1 at row {i}")
            Assert.AreEqual(w(i), sumObs, 0.0000001, $"observed sum equals weight at row {i}")
            Assert.AreEqual(w(i), sumMu, 0.0000001, $"fitted means sum equals weight at row {i}")
            Assert.AreEqual(0.0, sumResp, 0.0000001, $"response residuals sum to 0 at row {i}")
        Next

        ' Column names match category ordering
        Dim probNames() As String = m.GetResidualColumnNames(regression.ResidualColumnType.FittedProbability)
        Assert.AreEqual(3, probNames.Length)
        Assert.IsTrue(probNames(0).IndexOf("cat=1", StringComparison.OrdinalIgnoreCase) >= 0)
        Assert.IsTrue(probNames(1).IndexOf("cat=2", StringComparison.OrdinalIgnoreCase) >= 0)
        Assert.IsTrue(probNames(2).IndexOf("cat=3", StringComparison.OrdinalIgnoreCase) >= 0)

    End Sub

    ' ---------------------------
    ' Weights behave like frequency replication
    ' ---------------------------
    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Frequency_weights_match_expanded_unweighted_fit()

        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_basic.csv")
        Dim dataW(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offsW() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        ' weighted fit
        Dim mW As New regression.OrdinalLogitModel()
        mW.Data(dataW, names, offset:=offsW, weights:=w)
        mW.Fit(regression.ReferenceCategory.Last)

        ' expanded fit (unweighted)
        Dim expanded = ExpandWeightedRows(dataW, offsW, w)
        Dim dataU(,) As Double = expanded.Item1
        Dim offsU() As Double = expanded.Item2

        Dim mU As New regression.OrdinalLogitModel()
        mU.Data(dataU, names, offset:=offsU)
        mU.Fit(regression.ReferenceCategory.Last)

        AssertVectorAlmostEqual(mW.results.Coeffs_est, mU.results.Coeffs_est, 0.00001, "Coeffs weighted vs expanded")
        Assert.AreEqual(mW.results.ModelTableVals(1, 0), mU.results.ModelTableVals(1, 0), 0.00001, "LL weighted vs expanded")
    End Sub

    ' ---------------------------
    ' ReferenceCategory handling: reversing labels + First should match Last
    ' ---------------------------
    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub ReferenceCategory_First_with_reversed_labels_matches_Last()

        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        ' Fit with Last on original labels
        Dim mLast As New regression.OrdinalLogitModel()
        mLast.Data(data, names, offset:=offs, weights:=w)
        mLast.Fit(regression.ReferenceCategory.Last)

        ' Reverse the observed labels (1<->3) and fit with First
        Dim n As Integer = UBound(data, 1) + 1
        Dim dataRev(n - 1, UBound(data, 2)) As Double
        Array.Copy(data, dataRev, data.Length)

        For i As Integer = 0 To n - 1
            Dim y As Integer = CInt(Math.Round(data(i, 0)))
            dataRev(i, 0) = 4 - y
        Next

        Dim mFirst As New regression.OrdinalLogitModel()
        mFirst.Data(dataRev, names, offset:=offs, weights:=w)
        mFirst.Fit(regression.ReferenceCategory.First)

        AssertVectorAlmostEqual(mLast.results.Coeffs_est, mFirst.results.Coeffs_est, 0.00001, "Reversal equivalence")

        ' Categories are reversed for First
        Assert.IsTrue(mFirst.Classification.Categories.SequenceEqual(New Integer() {3, 2, 1}), "Categories should be reversed under ReferenceCategory.First")
    End Sub

    ' ---------------------------
    ' Intercept-only: alpha equals logit cumulative proportions
    ' ---------------------------
    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Intercept_only_thresholds_equal_logit_of_weighted_cumulative_props()

        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_intercept_only.csv")
        Dim raw(,) As Double = loaded.Item1 ' contains y only (no predictors)
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        ' build data matrix with just y column
        Dim n As Integer = UBound(raw, 1) + 1
        Dim data(n - 1, 0) As Double
        For i As Integer = 0 To n - 1
            data(i, 0) = raw(i, 0)
        Next

        Dim m As New regression.OrdinalLogitModel()
        m.Data(data, New String() {"y"}, offset:=offs, weights:=w)
        m.Fit(regression.ReferenceCategory.Last)

        Assert.AreEqual(2, m.results.Coeffs_est.Length, "Intercept-only should return K-1 thresholds")

        Dim total As Double = w.Sum()
        Dim p1 As Double = w(0) / total
        Dim p12 As Double = (w(0) + w(1)) / total

        Dim a1 As Double = Math.Log(p1 / (1.0 - p1))
        Dim a2 As Double = Math.Log(p12 / (1.0 - p12))

        Assert.AreEqual(a1, m.results.Coeffs_est(0), 0.0000001, "alpha1")
        Assert.AreEqual(a2, m.results.Coeffs_est(1), 0.0000001, "alpha2")

        ' In the intercept-only case, LR df is 0 and p-value is NaN by design.
        Dim mv As Object(,) = m.results.ModelTableVals
        Assert.AreEqual(0, CInt(mv(3, 1)), "LR df should be 0")
        Assert.IsTrue(Double.IsNaN(CDbl(mv(3, 2))), "LR p-value should be NaN when df=0")
    End Sub

    ' ---------------------------
    ' Edge cases / exceptions
    ' ---------------------------
    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Calculate_throws_if_Data_not_called()
        Dim m As New regression.OrdinalLogitModel()
        Assert.ThrowsException(Of NullReferenceException)(Sub() m.Fit())
    End Sub

    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Calculate_throws_if_less_than_two_categories()
        Dim data(4, 1) As Double
        For i As Integer = 0 To 4
            data(i, 0) = 1 ' single category
            data(i, 1) = i
        Next
        Dim m As New regression.OrdinalLogitModel()
        m.Data(data, New String() {"y", "x1"})
        Assert.ThrowsException(Of ArgumentException)(Sub() m.Fit())
    End Sub

    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Calculate_throws_if_startParams_length_mismatch()
        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        Dim m As New regression.OrdinalLogitModel()
        m.Data(data, names, offset:=offs, weights:=w)
        m.startParams = New Double() {0.0, 0.0} ' wrong length
        Assert.ThrowsException(Of ArgumentException)(Sub() m.Fit())
    End Sub

    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub GetResidualColumnNames_throws_if_model_not_fit()
        Dim m As New regression.OrdinalLogitModel()
        Assert.ThrowsException(Of InvalidOperationException)(Sub() m.GetResidualColumnNames(regression.ResidualColumnType.FittedProbability))
    End Sub

    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub wrapResiduals_throws_if_residuals_not_computed()
        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        Dim m As New regression.OrdinalLogitModel() With {.bComputeResiduals = False}
        m.Data(data, names, offset:=offs, weights:=w)
        m.Fit(reference:=regression.ReferenceCategory.Last)

        Assert.ThrowsException(Of NullReferenceException)(Sub() m.wrapResiduals())
    End Sub

    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Fit_near_separation_converges_and_extreme_predictions_are_sensible()

        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_near_separation.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        Dim m As New regression.OrdinalLogitModel() With {
        .bComputeResiduals = True,
        .bReturnCov = True
    }

        m.Data(data, names, offset:=offs, weights:=w)
        m.Fit(reference:=regression.ReferenceCategory.Last)

        ' Converged? is stored in ModelTableVals row 12, col 0
        Dim convStr As String = CStr(m.results.ModelTableVals(12, 0))
        Assert.IsTrue(String.Equals(convStr, "True", StringComparison.OrdinalIgnoreCase),
                  "Model did not converge on near-separation dataset.")

        ' There is 1 predictor (?) + 2 thresholds (K-1) => length 3
        Assert.AreEqual(3, m.results.Coeffs_est.Length, "Expected 1 slope + 2 thresholds.")

        Dim beta As Double = m.results.Coeffs_est(0)
        Assert.IsTrue(Math.Abs(beta) > 2.0, $"Expected a fairly large slope under near-separation. beta={beta}")

        ' Thresholds must be strictly increasing
        Assert.IsTrue(m.results.Coeffs_est(1) < m.results.Coeffs_est(2), "Thresholds are not increasing.")

        ' Probability sanity at extremes: x=-4 => mostly lowest category; x=4 => mostly highest category
        Dim res = m.Residuals
        Assert.IsNotNull(res, "Residuals not computed.")
        Assert.AreEqual(3, UBound(res.Probabilities, 2) + 1, "Expected K=3 probability columns.")

        Dim n As Integer = UBound(data, 1) + 1
        Dim minIdx As Integer = 0
        Dim maxIdx As Integer = 0
        Dim minX As Double = Double.PositiveInfinity
        Dim maxX As Double = Double.NegativeInfinity

        For i As Integer = 0 To n - 1
            Dim x As Double = data(i, 1) ' x1 is the only predictor
            If x < minX Then
                minX = x
                minIdx = i
            End If
            If x > maxX Then
                maxX = x
                maxIdx = i
            End If
        Next

        Dim pLowAtMin As Double = res.Probabilities(minIdx, 0) ' lowest category
        Dim pHighAtMax As Double = res.Probabilities(maxIdx, 2) ' highest category

        Assert.IsTrue(pLowAtMin > 0.9, $"Expected P(lowest|x=min) > 0.9 but got {pLowAtMin}")
        Assert.IsTrue(pHighAtMax > 0.9, $"Expected P(highest|x=max) > 0.9 but got {pHighAtMax}")

    End Sub

    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Zero_and_negative_weights_are_ignored_like_row_removal_in_fit()

        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        ' Modify a few weights to be zero / negative
        Dim wMod() As Double = CType(w.Clone(), Double())
        wMod(0) = 0.0
        wMod(1) = -10.0
        wMod(7) = 0.0
        wMod(12) = -1.0

        ' Fit with the modified weights (rows with w<=0 are skipped internally)
        Dim mAll As New regression.OrdinalLogitModel()
        mAll.Data(data, names, offset:=offs, weights:=wMod)
        mAll.Fit(reference:=regression.ReferenceCategory.Last)

        ' Fit on filtered dataset where those rows are physically removed
        Dim filtered = FilterRowsByPositiveWeights(data, offs, wMod)
        Dim dataKeep(,) As Double = filtered.Item1
        Dim offKeep() As Double = filtered.Item2
        Dim wKeep() As Double = filtered.Item3

        Dim mDrop As New regression.OrdinalLogitModel()
        mDrop.Data(dataKeep, names, offset:=offKeep, weights:=wKeep)
        mDrop.Fit(reference:=regression.ReferenceCategory.Last)

        AssertVectorAlmostEqual(mDrop.results.Coeffs_est, mAll.results.Coeffs_est, 0.00001,
                           "Coefficients should match when dropping w<=0 rows.")

        Dim llAll As Double = CDbl(mAll.results.ModelTableVals(1, 0))
        Dim llDrop As Double = CDbl(mDrop.results.ModelTableVals(1, 0))
        Assert.AreEqual(llDrop, llAll, 0.00001, "Log-likelihood should match when dropping w<=0 rows.")

    End Sub

    <TestCategory("OrdinalLogitModel")>
    <TestMethod()>
    Public Sub Residuals_for_nonpositive_weight_rows_are_zero_and_NaN_when_no_covariance()

        Dim loaded = LoadOrdinalCsv("ordinal_logit_dataset_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        ' Put some non-positive weights in
        Dim wMod() As Double = CType(w.Clone(), Double())
        wMod(0) = 0.0
        wMod(5) = -2.0

        Dim m As New regression.OrdinalLogitModel() With {
        .bComputeResiduals = True,
        .bReturnCov = False
    }
        m.Data(data, names, offset:=offs, weights:=wMod)
        m.Fit(reference:=regression.ReferenceCategory.Last)

        Dim res = m.Residuals
        Assert.IsNotNull(res)

        Dim n As Integer = UBound(data, 1) + 1
        Dim K As Integer = UBound(res.Probabilities, 2) + 1

        For i As Integer = 0 To n - 1
            If wMod(i) > 0.0 Then Continue For

            ' Probabilities are still computed
            Dim sumP As Double = 0.0
            For kk As Integer = 0 To K - 1
                sumP += res.Probabilities(i, kk)
                Assert.AreEqual(0.0, res.Observed(i, kk), 0.0, $"Observed should be 0 for w<=0 rows (row {i}, kk={kk})")
                Assert.AreEqual(0.0, res.FittedMeans(i, kk), 0.0, $"FittedMeans should be 0 for w<=0 rows (row {i}, kk={kk})")
                Assert.AreEqual(0.0, res.ResponseResiduals(i, kk), 0.0, $"ResponseResidual should be 0 for w<=0 rows (row {i}, kk={kk})")
            Next
            Assert.AreEqual(1.0, sumP, 0.0000001, $"Probabilities must sum to 1 even for w<=0 rows (row {i})")

            ' Deviance is set to 0 and standardized deviance to NaN (before leverage standardization pass)
            Assert.AreEqual(0.0, res.DevianceResiduals(i), 0.0, $"DevianceResidual should be 0 for w<=0 rows (row {i})")
            Assert.IsTrue(Double.IsNaN(res.StdDevianceResiduals(i)), $"StdDevianceResidual should be NaN for w<=0 rows (row {i})")
            Assert.IsTrue(Double.IsNaN(res.Leverage(i)), $"Leverage should be NaN for w<=0 rows (row {i})")
        Next

    End Sub

End Class
