Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch30Fixtures

<TestClass>
Public Class Graphics30Tests
    Public Shared Sub EqualValues(expected As Double(), actual As Double())
        Assert.AreEqual(expected.Length, actual.Length)
        For i As Integer = 0 To expected.Length - 1
            If Double.IsNaN(expected(i)) Then
                Assert.IsTrue(Double.IsNaN(actual(i)), "NaN at " & i)
            Else
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected(i)), BitConverter.DoubleToInt64Bits(actual(i)), "bits at " & i)
            End If
        Next
    End Sub
    Public Shared Function Flat(matrix As Double(,)) As Double()
        Return matrix.Cast(Of Double)().ToArray()
    End Function
    Private Shared Function RocGroups(fixture As Integer) As Double()()
        Select Case fixture
            Case 0 : Return {New Double() {4, 8, 6}, New Double() {3, 1, 2, 4}}
            Case 1 : Return {New Double() {1, 1, 1}, New Double() {1, 1}}
            Case 2 : Return {New Double() {1, 2}, New Double() {8, 9, 7}}
            Case 3 : Return {New Double() {9, 8, 7}, New Double() {2, 1}}
            Case Else : Return {New Double() {2, 2, 4, 3}, New Double() {1, 3, 3, 4, 2}}
        End Select
    End Function
    Public Shared Function RocStats(r As RocDataResult) As Double()
        Return {r.Auc, r.StandardError, r.PValue, r.Interval.LowerLimit, r.Interval.UpperLimit,
                r.DeLongStandardError, r.DeLongInterval.Estimate, r.DeLongInterval.LowerLimit, r.DeLongInterval.UpperLimit}
    End Function
    Private Shared Function RocStats(r As LegacyRoc30) As Double()
        Return {r.pAUC, r.pseAUC, r.pPvalue, r.pCI.LowerLimit, r.pCI.UpperLimit,
                r.pdelongSE, r.pdelongCI.Estimate, r.pdelongCI.LowerLimit, r.pdelongCI.UpperLimit}
    End Function
    <DataTestMethod>
    <DataRow(0, 0.05)><DataRow(0, 0.1)><DataRow(1, 0.05)><DataRow(1, 0.1)>
    <DataRow(2, 0.05)><DataRow(2, 0.1)><DataRow(3, 0.05)><DataRow(3, 0.1)>
    <DataRow(4, 0.05)><DataRow(4, 0.1)>
    Public Sub RocFrozenSource(fixture As Integer, alpha As Double)
        Dim groups = RocGroups(fixture), before = groups.Select(Function(x) DirectCast(x.Clone(), Double())).ToArray()
        Dim old As New LegacyRoc30(groups, {" P ", "P"})
        old.compute(alpha)
        Dim result = RocDataCore.Compute(groups, alpha)
        EqualValues(RocStats(old), RocStats(result))
        EqualValues(old.parCutOff, result.Cutoffs) : EqualValues(old.parSensitivity, result.Sensitivity)
        EqualValues(old.parSpecificity, result.Specificity) : EqualValues(old.par1minusSpec, result.FalsePositiveRate)
        Assert.AreEqual(groups.SelectMany(Function(x) x).Distinct().Count() + 1, result.Sensitivity.Length)
        For i As Integer = 0 To groups.Length - 1
            EqualValues(before(i), groups(i))
        Next
        Record("ROC-" & fixture & "-" & alpha.ToString(CultureInfo.InvariantCulture), RocStats(old).Concat(old.parCutOff).Concat(old.parSensitivity).Concat(old.parSpecificity).Concat(old.par1minusSpec).ToArray(),
               RocStats(result).Concat(result.Cutoffs).Concat(result.Sensitivity).Concat(result.Specificity).Concat(result.FalsePositiveRate).ToArray())
        'Legacy repeated-call accumulation is retained through an explicit seed.
        Dim seed = result.Auc
        old.compute(alpha)
        Dim repeated = RocDataCore.ComputeCurve(groups, alpha, seed)
        EqualValues({old.pAUC, old.pseAUC, old.pPvalue}, {repeated.Auc, repeated.StandardError, repeated.PValue})
        Dim changed = result.Cutoffs : changed(0) = 999
        EqualValues(before(0), groups(0))
        Assert.AreNotEqual(999.0, result.Cutoffs(0))
        Dim ci = result.Interval : ci.LowerLimit = 999
        Assert.AreNotEqual(999.0, result.Interval.LowerLimit)
    End Sub

    Public Shared Function ExceptionType(action As Action) As Type
        Try
            action()
            Return Nothing
        Catch ex As Exception
            Return ex.GetType()
        End Try
    End Function
    <TestMethod>
    Public Sub RocInvalidAndMidrankCopies()
        Dim fixtures As Double()()() = {Nothing, New Double()() {Nothing, New Double() {1, 2}}, New Double()() {New Double() {}, New Double() {}}, New Double()() {New Double() {1}, New Double() {2, 3}}}
        For Each groups In fixtures
            Dim old As New LegacyRoc30(groups, Nothing)
            Assert.AreEqual(ExceptionType(Sub() old.compute()), ExceptionType(Sub() RocDataCore.Compute(groups)))
            If groups IsNot Nothing AndAlso groups(0) IsNot Nothing Then
                Dim before = DirectCast(groups(0).Clone(), Double())
                ExceptionType(Sub() RocDataCore.Compute(groups))
                EqualValues(before, groups(0))
            End If
        Next
        Dim values = {3.0, 1.0, 3.0, Double.NaN, Double.PositiveInfinity}
        Dim clone = DirectCast(values.Clone(), Double())
        EqualValues(LegacyRoc30.MidRanks(values), RocDataCore.MidRanks(values))
        EqualValues(clone, values)
    End Sub

    <DataTestMethod>
    <DataRow("Blom", "SPSS")><DataRow("Blom", "OLS")><DataRow("Blom", "R")>
    <DataRow("Rankit", "SPSS")><DataRow("Rankit", "OLS")><DataRow("Rankit", "R")>
    <DataRow("Van Der Waerden", "SPSS")><DataRow("Van Der Waerden", "OLS")><DataRow("Van Der Waerden", "R")>
    Public Sub NormalFrozenSource(method As String, line As String)
        For Each values In New Double()() {New Double() {9, 1, 4, 4, 2, 8}, New Double() {2, 2, 2, 2}, New Double() {2, 1}, New Double() {3}}
            Dim before = DirectCast(values.Clone(), Double()), legacyInput = DirectCast(values.Clone(), Double())
            Dim old As New LegacyNormal30(legacyInput, " A ")
            Dim result = NormalPlotDataCore.Compute(values, method)
            EqualValues(old.compute(method), result.NormalScores)
            EqualValues(old.pData, result.Data) : EqualValues(old.pP, result.Probabilities)
            Dim oldLine As Double(,) = Nothing, newLine As Double(,) = Nothing
            Dim oldError = ExceptionType(Sub() oldLine = old.computeRefLIne(line))
            Assert.AreEqual(oldError, ExceptionType(Sub() newLine = NormalPlotDataCore.ReferenceLine(result, line)))
            If oldError Is Nothing Then
                Assert.AreEqual(2, newLine.GetLength(0)) : Assert.AreEqual(2, newLine.GetLength(1))
                EqualValues(Flat(oldLine), Flat(newLine))
                Record("QQ-" & method & "-" & line & "-" & values.Length, old.pData.Concat(old.pZ).Concat(old.pP).Concat(Flat(oldLine)).ToArray(), result.Data.Concat(result.NormalScores).Concat(result.Probabilities).Concat(Flat(newLine)).ToArray())
            End If
            EqualValues(before, values)
            Dim modified = result.Data : If modified.Length > 0 Then modified(0) = 999
            EqualValues(old.pData, result.Data)
        Next
    End Sub

    <TestMethod>
    Public Sub NormalMutationFallbackEmptyAndNonfinite()
        Dim source = {3.0, 1.0, 2.0}
        Dim old As New LegacyNormal30(source, Nothing)
        EqualValues({1, 2, 3}, source) 'Characterize old constructor mutation explicitly.
        Dim unsorted = {3.0, 1.0, 2.0}, clone = DirectCast(unsorted.Clone(), Double())
        NormalPlotDataCore.Compute(unsorted, "Blom") : EqualValues(clone, unsorted)
        For Each values In New Double()() {New Double() {}, New Double() {Double.NaN, 1, Double.PositiveInfinity, Double.NegativeInfinity}, New Double() {4, 1, 2}}
            For Each method In {"Blom", "blom", "Van der Waerden", "unknown"}
                Dim legacy As New LegacyNormal30(DirectCast(values.Clone(), Double()), Nothing)
                Dim result As NormalScoreData = Nothing
                Assert.AreEqual(ExceptionType(Sub() legacy.compute(method)), ExceptionType(Sub() result = NormalPlotDataCore.Compute(values, method)))
                If result IsNot Nothing Then
                    EqualValues(legacy.pZ, result.NormalScores)
                    For Each line In {"SPSS", "OLS", "R", "unknown"}
                        Dim a As Double(,) = Nothing, b As Double(,) = Nothing
                        Assert.AreEqual(ExceptionType(Sub() a = legacy.computeRefLIne(line)), ExceptionType(Sub() b = NormalPlotDataCore.ReferenceLine(result, line)))
                        If a IsNot Nothing AndAlso b IsNot Nothing Then EqualValues(Flat(a), Flat(b))
                    Next
                End If
            Next
        Next
        Assert.AreEqual(ExceptionType(Sub() Array.Sort(DirectCast(Nothing, Double()))), ExceptionType(Sub() NormalPlotDataCore.SortedCopy(Nothing)))
    End Sub

    <TestMethod>
    Public Sub SignedBoxSegmentsFromWholeFrozenMethod()
        Dim data As Double()() = {New Double() {-9, -5, -3, -1}, New Double() {-5, -1, 0, 3, 6}, New Double() {1, 2, 4, 8}, New Double() {0, 0, 0}, New Double() {4}}
        Dim old As New LegacyBox30 With {.pData = data, .pNoGroups = data.Length, .pNs = data.Select(Function(x) x.Length).ToArray(),
            .pArNOutliersBig = New Long(data.Length - 1) {}, .pArNOutliersSmall = New Long(data.Length - 1) {}}
        For Each group In data
            Dim ds As New DescriptiveStat(DirectCast(group.Clone(), Double()))
            ds.compute(False) : old.pDSCollection.Add(ds)
        Next
        old.CalcForPlotting()
        For i As Integer = 0 To data.Length - 1
            Dim ds = DirectCast(old.pDSCollection.Item(i + 1), DescriptiveStat)
            Dim result = BoxSegmentsCore.Compute(ds.LQuartile, ds.Median, ds.UQuartile)
            Dim expected = {old.pPlotBlank(i), old.pPlotMedian(i), old.pPlotQ3(i), old.pPlotQ3Minus(i), old.pPlotMedianMinus(i)}
            Dim actual = {result.Blank, result.Median, result.Q3, result.Q3Minus, result.MedianMinus}
            EqualValues(expected, actual) : Record("BOX-" & i, expected, actual)
        Next
        Dim nonfinite = BoxSegmentsCore.Compute(Double.NaN, Double.NaN, Double.NaN)
        EqualValues({0, 0, 0, 0, 0}, {nonfinite.Blank, nonfinite.Median, nonfinite.Q3, nonfinite.Q3Minus, nonfinite.MedianMinus})
    End Sub

    <DataTestMethod>
    <DataRow("(Sturges)")><DataRow("(Doane)")><DataRow("(Scott)")><DataRow("(Freedman-Diaconis)")><DataRow("")>
    Public Sub HistogramExistingEngineCopies(rule As String)
        Dim source = {9.0, -4, 3, 2, 0, 8, 6, 7, 1, 5}, before = DirectCast(source.Clone(), Double())
        Dim baseline = HistogramBinningCore.HistogramBinsComputation(DirectCast(source.Clone(), Double()), rule)
        Dim actual = HistogramDataCore.BinsCopy(source, rule)
        CollectionAssert.AreEqual(baseline, actual) : EqualValues(before, source)
        Dim mid = Enumerable.Range(0, actual.GetLength(0)).Select(Function(i) CDbl(actual(i, 0))).ToArray(), midBefore = DirectCast(mid.Clone(), Double())
        Dim overlay = HistogramDataCore.OverlayCopy(source, mid)
        EqualValues(Flat(HistogramBinningCore.GaussOverlayComputation(DirectCast(source.Clone(), Double()), mid)), Flat(overlay))
        EqualValues(before, source) : EqualValues(midBefore, mid)
        Record("HIST-" & rule, baseline.Cast(Of Object)().Select(Function(x) Convert.ToDouble(x)).ToArray(), actual.Cast(Of Object)().Select(Function(x) Convert.ToDouble(x)).ToArray())
        For Each edge In New Double()() {New Double() {}, New Double() {1}, New Double() {2, 2, 2}, New Double() {0, 0, 0, 0, 0}, New Double() {1, Double.NaN}, New Double() {1, Double.PositiveInfinity}}
            Dim original = DirectCast(edge.Clone(), Double())
            Assert.AreEqual(ExceptionType(Sub() HistogramBinningCore.HistogramBinsComputation(DirectCast(edge.Clone(), Double()), rule)), ExceptionType(Sub() HistogramDataCore.BinsCopy(edge, rule)))
            EqualValues(original, edge)
        Next
    End Sub

    <TestMethod>
    Public Sub HistogramFrozenBoundsAndEmptyPrecondition()
        For Each midpoints In New Double()() {New Double() {3}, New Double() {0}, New Double() {-2}, New Double() {1, 3, 5}, New Double() {3, 1}, New Double() {2, 2}, New Double() {Double.NaN}, New Double() {Double.PositiveInfinity}}
            Dim old As New LegacyHistogramBounds30 With {.arBinMidVal = midpoints}
            Dim b = HistogramDataCore.HorizontalBounds(midpoints), expected = old.Compute(), actual = {b.Minimum, b.Maximum, b.Width}
            EqualValues(expected, actual) : Record("BOUNDS-" & midpoints.Length & "-" & BitConverter.DoubleToInt64Bits(midpoints(0)), expected, actual)
        Next
        Assert.ThrowsException(Of ArgumentException)(Sub() HistogramDataCore.HorizontalBounds({}))
    End Sub

    <TestMethod>
    Public Sub ScatterFrozenNormalizationAndXyz()
        For Each data As Double(,) In {New Double(,) {{4, 10}, {1, 4}, {9, 7}}, New Double(,) {{2, 1}, {2, 5}, {2, 8}}}
            Dim before = DirectCast(data.Clone(), Double(,))
            Dim old As New LegacyScatter30 With {.pData = data, .n = data.GetLength(0), .p = data.GetLength(1)}
            old.transformData()
            Dim actual = ScatterMatrixDataCore.Normalize(data, 0.1)
            EqualValues(Flat(old.ScaledData), Flat(actual.Values)) : EqualValues(old.x_var_labels, actual.DiagonalLabelX)
            EqualValues(Flat(before), Flat(data)) : Record("SCATTER-" & data(0, 0), Flat(old.ScaledData), Flat(actual.Values))
            Dim copy = actual.Values : copy(0, 0) = 999
            EqualValues(Flat(old.ScaledData), Flat(actual.Values))
        Next
        For Each direction In {1.0, -1.0}
            For Each span In {4.0, 0.0, -4.0}
                Dim raw = {4.0, 1.0, 3.0}, original = DirectCast(raw.Clone(), Double())
                Dim old As New LegacyXyz30 With {.n = raw.Length, .x_raw = raw, .y_raw = raw, .z_raw = raw,
                    .raw_mins_ = {1, 1, 1}, .raw_ranges_ = {span, span, span}, .axis_dir_ = {direction, direction, direction}}
                old.getnorm()
                Dim actual = XyzDataCore.NormalizeAxis(raw, 1, span, direction, raw.Length)
                EqualValues(old.x_norm, actual) : EqualValues(original, raw)
                Record("XYZ-" & direction & "-" & span, old.x_norm, actual)
            Next
        Next
        Dim ragged = {1.0}
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() XyzDataCore.NormalizeAxis(ragged, 0, 1, 1, 2))
        EqualValues({1}, ragged)
        Dim empty As New LegacyXyz30 With {.n = 0}
        empty.getnorm()
        EqualValues(empty.x_norm, XyzDataCore.NormalizeAxis(Nothing, 0, 0, 1, 0))
    End Sub

    'Optional local evidence export: no default writer and no historical destination.
    Public Shared Sub Record(id As String, expected As Double(), actual As Double())
        Dim folder = Environment.GetEnvironmentVariable("BESHSTAT_04A30_CASES")
        If String.IsNullOrEmpty(folder) Then Return
        Dim values As Func(Of Double(), String) = Function(a) "[" & String.Join(",", a.Select(Function(x) "{""bits"":""" & BitConverter.DoubleToInt64Bits(x).ToString("X16", CultureInfo.InvariantCulture) & """,""nan"":" & If(Double.IsNaN(x), "true", "false") & ",""infinity"":" & If(Double.IsInfinity(x), "true", "false") & "}")) & "]"
        System.IO.Directory.CreateDirectory(folder)
        Dim safe = String.Concat(id.Select(Function(c) If(Char.IsLetterOrDigit(c), c, "_"c)))
        System.IO.File.WriteAllText(System.IO.Path.Combine(folder, safe & ".json"), "{""case"":""" & safe & """,""status"":""SOURCE_COMPATIBILITY_PASS"",""scientific"":""NOT_GOLDEN"",""before"":" & values(expected) & ",""after"":" & values(actual) & "}")
    End Sub
End Class
