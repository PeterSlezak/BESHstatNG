Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()>
Public Class MultinomialLogitModel_Tests

    Private Const TOL_COEF As Double = 0.000001
    Private Const TOL_STAT As Double = 0.000001

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

    Private Shared Function GetAsDouble(s As String) As Double
        Return Double.Parse(s, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' Loads grouped multinomial CSV with header.
    ''' Required columns: y,x1,x2,offset,w
    ''' Returns (dataMatrix, names, offset, weights)
    ''' dataMatrix columns: y, x1, x2
    ''' </summary>
    Private Shared Function LoadMlogitCsv(fileName As String) As Tuple(Of Double(,), String(), Double(), Double())
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must have header + at least one data row.")

        Dim header() As String = lines(0).Split(","c)
        Dim colIndex As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For ii As Integer = 0 To header.Length - 1
            colIndex(header(ii).Trim()) = ii
        Next

        Dim required() As String = {"y", "x1", "x2", "offset", "w"}
        For Each req In required
            If Not colIndex.ContainsKey(req) Then Throw New InvalidOperationException("CSV must include column '" & req & "'.")
        Next

        Dim n As Integer = lines.Length - 1
        Dim data(n - 1, 2) As Double ' y + 2 predictors
        Dim offs(n - 1) As Double
        Dim w(n - 1) As Double

        For r As Integer = 0 To n - 1
            Dim parts() As String = lines(r + 1).Split(","c)

            data(r, 0) = GetAsDouble(parts(colIndex("y")).Trim())
            data(r, 1) = GetAsDouble(parts(colIndex("x1")).Trim())
            data(r, 2) = GetAsDouble(parts(colIndex("x2")).Trim())
            offs(r) = GetAsDouble(parts(colIndex("offset")).Trim())
            w(r) = GetAsDouble(parts(colIndex("w")).Trim())
        Next

        Dim names() As String = {"y", "x1", "x2"}
        Return Tuple.Create(data, names, offs, w)
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

    Private Shared Function ExpandByIntegerWeights(data(,) As Double, offs() As Double, w() As Double) As Tuple(Of Double(,), Double())
        Dim n As Integer = UBound(data, 1) + 1
        Dim cols As Integer = UBound(data, 2) + 1

        Dim total As Integer = 0
        For i As Integer = 0 To n - 1
            Dim wi As Double = w(i)
            If wi <= 0 Then Continue For
            Dim wiInt As Integer = CInt(Math.Round(wi))
            If Math.Abs(wi - wiInt) > 0.0000001 Then
                Throw New InvalidOperationException("Weights must be (near) integers for expansion test. Row " & i & " has w=" & wi.ToString(CultureInfo.InvariantCulture))
            End If
            total += wiInt
        Next

        Dim out(total - 1, cols - 1) As Double
        Dim outOff(total - 1) As Double

        Dim r As Integer = 0
        For i As Integer = 0 To n - 1
            Dim wi As Double = w(i)
            If wi <= 0 Then Continue For
            Dim wiInt As Integer = CInt(Math.Round(wi))
            For rep As Integer = 1 To wiInt
                For c As Integer = 0 To cols - 1
                    out(r, c) = data(i, c)
                Next
                outOff(r) = offs(i)
                r += 1
            Next
        Next

        Return Tuple.Create(out, outOff)
    End Function

    Private Shared Function FilterRowsByPositiveWeights(data(,) As Double, offs() As Double, w() As Double) As Tuple(Of Double(,), Double(), Double())
        Dim n As Integer = UBound(data, 1) + 1
        Dim cols As Integer = UBound(data, 2) + 1

        Dim keepCount As Integer = 0
        For i As Integer = 0 To n - 1
            If w(i) > 0.0 Then keepCount += 1
        Next

        Dim out(keepCount - 1, cols - 1) As Double
        Dim outOff(keepCount - 1) As Double
        Dim outW(keepCount - 1) As Double

        Dim r As Integer = 0
        For i As Integer = 0 To n - 1
            If w(i) <= 0.0 Then Continue For
            For c As Integer = 0 To cols - 1
                out(r, c) = data(i, c)
            Next
            outOff(r) = offs(i)
            outW(r) = w(i)
            r += 1
        Next

        Return Tuple.Create(out, outOff, outW)
    End Function

    Private Shared Function ResidualTableToColumnIndexMap(tbl(,) As Object) As Dictionary(Of String, Integer)
        Dim nCols As Integer = UBound(tbl, 2) + 1
        Dim map As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For c As Integer = 0 To nCols - 1
            Dim v As Object = tbl(0, c)
            If v Is Nothing Then Continue For
            Dim s As String = CStr(v).Trim()
            If s.Length = 0 Then Continue For
            If Not map.ContainsKey(s) Then map.Add(s, c)
        Next
        Return map
    End Function

    Private Shared Function ComputeWeightedAccuracyFromResidualTable(tbl(,) As Object,
                                                                    data(,) As Double,
                                                                    weights() As Double,
                                                                    catValues() As Integer) As Double
        Dim map = ResidualTableToColumnIndexMap(tbl)

        ' Find probability columns by category label
        Dim probCols As New Dictionary(Of Integer, Integer)()
        For Each catVal As Integer In catValues
            Dim key As String = "FittedProb: cat=" & catVal.ToString(CultureInfo.InvariantCulture)
            If Not map.ContainsKey(key) Then Throw New InvalidOperationException("Missing probability column '" & key & "'.")
            probCols(catVal) = map(key)
        Next

        Dim n As Integer = UBound(data, 1) + 1
        Dim totalW As Double = 0.0
        Dim correctW As Double = 0.0

        For i As Integer = 0 To n - 1
            Dim wi As Double = weights(i)
            If wi <= 0.0 Then Continue For

            Dim obs As Integer = CInt(Math.Round(data(i, 0)))
            Dim bestCat As Integer = catValues(0)
            Dim bestP As Double = Double.NegativeInfinity

            ' tie-break to smallest category (matches model)
            For Each catVal As Integer In catValues.OrderBy(Function(z) z)
                Dim p As Double = CDbl(tbl(i + 1, probCols(catVal))) ' +1 because row 0 is header
                If p > bestP Then
                    bestP = p
                    bestCat = catVal
                End If
            Next

            totalW += wi
            If bestCat = obs Then correctW += wi
        Next

        If totalW <= 0.0 Then Return Double.NaN
        Return correctW / totalW
    End Function

    ' ---------------------------
    ' Tests
    ' ---------------------------

    <TestCategory("MultinomialLogitModel")>
    <TestMethod()>
    Public Sub Fit_grouped_basic_matches_reference_outputs()

        Dim loaded = LoadMlogitCsv("mlogit_dataset_grouped_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        Dim m As New MultinomialLogitModel() With {
            .bComputeResiduals = True,
            .bReturnCov = True
        }
        m.settingInputs(0.05, 200, 0.0000000001)
        m.data(data, names, offset:=offs, weights:=w)
        m.Calculate(intercept:=1, reference:=ReferenceCategory.Last)

        ' Expected: categories {1,2,3}, baseline=3 => 2 non-baseline cats, p=3 => q=6
        Dim expectedCoef() As Double = {
            -0.19151751037203915,
            -1.0618081473590739,
            0.47922353515433547,
            -0.25969974885385816,
            -0.57788371456108056,
            0.19098864233688326
        }
        Dim expectedSe() As Double = {
            0.45171686137306849,
            0.40606909687251064,
            0.52126284052154659,
            0.4476594006193988,
            0.34528542907280413,
            0.46706942657431166
        }

        AssertVectorAlmostEqual(expectedCoef, m.results.Coeffs_est, TOL_COEF, "Coefficients mismatch.")
        AssertVectorAlmostEqual(expectedSe, m.results.Coeffs_SEs, 0.00001, "SE mismatch.")

        ' Model table indices per MultinomialLogitModel:
        ' 0 LL0, 1 LL, 3 LR test [chisq, df, p], 4 GOF [chisq, df, p], 8 AIC, 9 BIC
        Dim ll0 As Double = CDbl(m.results.ModelTableVals(0, 0))
        Dim ll1 As Double = CDbl(m.results.ModelTableVals(1, 0))
        Assert.AreEqual(-42.157747411659, ll0, 0.00001, "LL0 mismatch.")
        Assert.AreEqual(-36.964300068514, ll1, 0.00001, "LL mismatch.")

        Dim lrChi2 As Double = CDbl(m.results.ModelTableVals(3, 0))
        Dim lrDf As Integer = CInt(m.results.ModelTableVals(3, 1))
        Dim lrP As Double = CDbl(m.results.ModelTableVals(3, 2))
        Assert.AreEqual(10.386894686289, lrChi2, 0.00001, "LR Chi2 mismatch.")
        Assert.AreEqual(4, lrDf, "LR df mismatch.")
        Assert.AreEqual(0.034391168056059, lrP, 0.0000005, "LR p mismatch.")

        Dim gofChi2 As Double = CDbl(m.results.ModelTableVals(4, 0))
        Dim gofDf As Integer = CInt(m.results.ModelTableVals(4, 1))
        Dim gofP As Double = CDbl(m.results.ModelTableVals(4, 2))
        Assert.AreEqual(3.120050788548, gofChi2, 0.00001, "GOF Chi2 mismatch.")
        Assert.AreEqual(4, gofDf, "GOF df mismatch.")
        Assert.AreEqual(0.537940017757, gofP, 0.0000005, "GOF p mismatch.")

        Dim aic As Double = CDbl(m.results.ModelTableVals(8, 0))
        Dim bic As Double = CDbl(m.results.ModelTableVals(9, 0))
        Assert.AreEqual(85.928600137029, aic, 0.00001, "AIC mismatch.")
        Assert.AreEqual(96.061876861712, bic, 0.00002, "BIC mismatch.")

        Dim cs As Double = CDbl(m.results.ModelTableVals(5, 0))
        Dim nk As Double = CDbl(m.results.ModelTableVals(6, 0))
        Dim mf As Double = CDbl(m.results.ModelTableVals(7, 0))
        Assert.AreEqual(0.228695750985, cs, 0.00001, "CoxSnell R2 mismatch.")
        Assert.AreEqual(0.260323602575, nk, 0.00001, "Nagelkerke R2 mismatch.")
        Assert.AreEqual(0.12319081692, mf, 0.00001, "McFadden R2 mismatch.")

        ' Converged?
        Dim convStr As String = CStr(m.results.ModelTableVals(12, 0))
        Assert.IsTrue(String.Equals(convStr, "True", StringComparison.OrdinalIgnoreCase), "Model did not converge.")

    End Sub

    <TestCategory("MultinomialLogitModel")>
    <TestMethod()>
    Public Sub wrapResiduals_probabilities_sum_to_one_and_accuracy_matches_reference()

        Dim loaded = LoadMlogitCsv("mlogit_dataset_grouped_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        Dim m As New MultinomialLogitModel() With {
            .bComputeResiduals = True,
            .bReturnCov = True
        }
        m.settingInputs(0.05, 200, 0.0000000001)
        m.data(data, names, offset:=offs, weights:=w)
        m.Calculate(intercept:=1, reference:=ReferenceCategory.Last)

        Dim tbl(,) As Object = m.wrapResiduals()
        Assert.IsNotNull(tbl, "wrapResiduals returned Nothing.")

        Dim colMap = ResidualTableToColumnIndexMap(tbl)

        ' Prob columns in original category labels for reference=Last:
        Dim cats() As Integer = {1, 2, 3}
        Dim probCols(2) As Integer
        For idx As Integer = 0 To 2
            Dim key As String = "FittedProb: cat=" & cats(idx).ToString(CultureInfo.InvariantCulture)
            Assert.IsTrue(colMap.ContainsKey(key), "Missing probability column: " & key)
            probCols(idx) = colMap(key)
        Next

        Dim n As Integer = UBound(data, 1) + 1
        For i As Integer = 0 To n - 1
            Dim sumP As Double = 0.0
            For kk As Integer = 0 To 2
                sumP += CDbl(tbl(i + 1, probCols(kk))) ' +1 header row
            Next
            Assert.AreEqual(1.0, sumP, 0.0000001, "Probabilities do not sum to 1 at row " & i)
        Next

        ' Weighted accuracy from residual probability argmax (tie-break to smallest)
        Dim acc As Double = ComputeWeightedAccuracyFromResidualTable(tbl, data, w, cats)
        Assert.AreEqual(0.6, acc, 0.000001, "Accuracy mismatch.")

    End Sub

    <TestCategory("MultinomialLogitModel")>
    <TestMethod()>
    Public Sub ReferenceCategory_First_and_Last_have_identical_probabilities_and_LL()

        Dim loaded = LoadMlogitCsv("mlogit_dataset_grouped_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim w() As Double = loaded.Item4

        ' IMPORTANT:
        ' In this implementation, offset is added ONLY to non-baseline categories.
        ' Changing the reference category changes which category receives no offset.
        ' Therefore, invariance across reference choices holds only when offset is not used
        ' (or is constant/zero for all rows).
        Dim offsNothing() As Double = Nothing

        Dim mLast As New MultinomialLogitModel() With {.bComputeResiduals = True, .bReturnCov = False}
        mLast.settingInputs(0.05, 500, 0.000000000001) ' a bit tighter just for stability
        mLast.data(data, names, offset:=offsNothing, weights:=w)
        mLast.Calculate(intercept:=1, reference:=ReferenceCategory.Last)

        Dim mFirst As New MultinomialLogitModel() With {.bComputeResiduals = True, .bReturnCov = False}
        mFirst.settingInputs(0.05, 500, 0.000000000001)
        mFirst.data(data, names, offset:=offsNothing, weights:=w)
        mFirst.Calculate(intercept:=1, reference:=ReferenceCategory.First)

        Dim llLast As Double = CDbl(mLast.results.ModelTableVals(1, 0))
        Dim llFirst As Double = CDbl(mFirst.results.ModelTableVals(1, 0))
        Assert.AreEqual(llLast, llFirst, 0.000001, "LL should be identical across reference choices when offset is not used.")

        Dim tblLast(,) As Object = mLast.wrapResiduals()
        Dim tblFirst(,) As Object = mFirst.wrapResiduals()

        Dim mapLast = ResidualTableToColumnIndexMap(tblLast)
        Dim mapFirst = ResidualTableToColumnIndexMap(tblFirst)

        Dim cats() As Integer = {1, 2, 3}
        Dim n As Integer = UBound(data, 1) + 1

        For i As Integer = 0 To n - 1
            For Each catVal As Integer In cats
                Dim key As String = "FittedProb: cat=" & catVal.ToString(Globalization.CultureInfo.InvariantCulture)
                Dim pL As Double = CDbl(tblLast(i + 1, mapLast(key)))
                Dim pF As Double = CDbl(tblFirst(i + 1, mapFirst(key)))
                Assert.AreEqual(pL, pF, 0.000001, $"Probability mismatch at row {i}, cat={catVal}")
            Next
        Next

    End Sub


    <TestCategory("MultinomialLogitModel")>
    <TestMethod()>
    Public Sub Grouped_weights_equivalent_to_expanded_unweighted_fit()

        Dim loaded = LoadMlogitCsv("mlogit_dataset_grouped_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        ' Grouped weighted fit
        Dim mW As New MultinomialLogitModel()
        mW.settingInputs(0.05, 200, 0.0000000001)
        mW.data(data, names, offset:=offs, weights:=w)
        mW.Calculate(intercept:=1, reference:=ReferenceCategory.Last)

        ' Expanded unweighted fit (repeat rows by integer weights)
        Dim expanded = ExpandByIntegerWeights(data, offs, w)
        Dim dataExp(,) As Double = expanded.Item1
        Dim offsExp() As Double = expanded.Item2

        Dim mExp As New MultinomialLogitModel()
        mExp.settingInputs(0.05, 200, 0.0000000001)
        mExp.data(dataExp, names, offset:=offsExp, weights:=Nothing)
        mExp.Calculate(intercept:=1, reference:=ReferenceCategory.Last)

        AssertVectorAlmostEqual(mW.results.Coeffs_est, mExp.results.Coeffs_est, 0.00001, "Coefficients should match grouped vs expanded.")
        Dim llW As Double = CDbl(mW.results.ModelTableVals(1, 0))
        Dim llE As Double = CDbl(mExp.results.ModelTableVals(1, 0))
        Assert.AreEqual(llW, llE, 0.00001, "LL should match grouped vs expanded.")

    End Sub

    <TestCategory("MultinomialLogitModel")>
    <TestMethod()>
    Public Sub Zero_and_negative_weights_are_ignored_like_row_removal_in_fit()

        Dim loaded = LoadMlogitCsv("mlogit_dataset_grouped_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        Dim wMod() As Double = CType(w.Clone(), Double())
        wMod(0) = 0.0
        wMod(1) = -5.0
        wMod(5) = 0.0

        Dim mAll As New MultinomialLogitModel()
        mAll.settingInputs(0.05, 200, 0.0000000001)
        mAll.data(data, names, offset:=offs, weights:=wMod)
        mAll.Calculate(intercept:=1, reference:=ReferenceCategory.Last)

        Dim filtered = FilterRowsByPositiveWeights(data, offs, wMod)
        Dim dataKeep(,) As Double = filtered.Item1
        Dim offKeep() As Double = filtered.Item2
        Dim wKeep() As Double = filtered.Item3

        Dim mDrop As New MultinomialLogitModel()
        mDrop.settingInputs(0.05, 200, 0.0000000001)
        mDrop.data(dataKeep, names, offset:=offKeep, weights:=wKeep)
        mDrop.Calculate(intercept:=1, reference:=ReferenceCategory.Last)

        AssertVectorAlmostEqual(mDrop.results.Coeffs_est, mAll.results.Coeffs_est, 0.00001, "Coefficients should match when dropping w<=0 rows.")
        Dim llAll As Double = CDbl(mAll.results.ModelTableVals(1, 0))
        Dim llDrop As Double = CDbl(mDrop.results.ModelTableVals(1, 0))
        Assert.AreEqual(llDrop, llAll, 0.00001, "LL should match when dropping w<=0 rows.")

    End Sub

    <TestCategory("MultinomialLogitModel")>
    <TestMethod()>
    Public Sub Calculate_throws_when_data_not_set()
        Dim m As New MultinomialLogitModel()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() m.Calculate())
    End Sub

    <TestCategory("MultinomialLogitModel")>
    <TestMethod()>
    Public Sub Calculate_throws_when_only_one_category_present()
        Dim data(2, 2) As Double
        ' y all same => K=1
        data(0, 0) = 1 : data(0, 1) = 0 : data(0, 2) = 0
        data(1, 0) = 1 : data(1, 1) = 1 : data(1, 2) = 0
        data(2, 0) = 1 : data(2, 1) = 0 : data(2, 2) = 1
        Dim names() As String = {"y", "x1", "x2"}

        Dim m As New MultinomialLogitModel()
        m.data(data, names)
        Assert.ThrowsException(Of ArgumentException)(Sub() m.Calculate())
    End Sub

    <TestCategory("MultinomialLogitModel")>
    <TestMethod()>
    Public Sub Calculate_throws_when_no_predictors_and_intercept_zero()
        ' cols=1 (Y only), intercept=0 => invalid
        Dim data(4, 0) As Double
        data(0, 0) = 1
        data(1, 0) = 2
        data(2, 0) = 3
        data(3, 0) = 1
        data(4, 0) = 2
        Dim names() As String = {"y"}

        Dim m As New MultinomialLogitModel()
        m.data(data, names)
        Assert.ThrowsException(Of ArgumentException)(Sub() m.Calculate(intercept:=0))
    End Sub

    <TestCategory("MultinomialLogitModel")>
    <TestMethod()>
    Public Sub Calculate_throws_when_startParams_length_mismatch()
        Dim loaded = LoadMlogitCsv("mlogit_dataset_grouped_basic.csv")
        Dim data(,) As Double = loaded.Item1
        Dim names() As String = loaded.Item2
        Dim offs() As Double = loaded.Item3
        Dim w() As Double = loaded.Item4

        Dim m As New MultinomialLogitModel()
        m.settingInputs(0.05, 200, 0.0000000001)
        m.data(data, names, offset:=offs, weights:=w)

        m.startParams = New Double() {0.1, 0.2, 0.3} ' wrong length (should be 6)
        Assert.ThrowsException(Of ArgumentException)(Sub() m.Calculate(intercept:=1, reference:=ReferenceCategory.Last, bStartParams:=True))
    End Sub

End Class
