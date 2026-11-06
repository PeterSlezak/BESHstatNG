Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch32Fixtures

<TestClass>
Public Class Graphics32Tests
    Public Shared Function Inputs() As Double()()
        Return {New Double() {4, 1, 4, 2, 1}, New Double() {-9, -5, -3, -1}, New Double() {-5, -1, 0, 3, 6}, New Double() {0, 0, 0}, New Double() {3}, New Double() {2, 1}, New Double() {}, New Double() {1, 2, 3, 4, 5, 6, 7, 8, 9, 100}, New Double() {Double.NaN, 1, Double.PositiveInfinity}, New Double() {BitConverter.Int64BitsToDouble(Long.MinValue), 0, 1, 1}, New Double() {-100, 1, 2, 3, 4, 5, 6, 7, 8, 9, 100}, New Double() {1.0E-150, 2.0E-150, 3.0E-150}, New Double() {1.0E+150, 2.0E+150, 3.0E+150}}
    End Function
    Private Shared Function BoxVector(g As BoxGroupPlotData) As Double()
        Dim a = g.Aggregate, s = g.Segments
        Return {s.Blank, s.Median, s.Q3, s.Q3Minus, s.MedianMinus, g.UpperWhiskerLength, g.LowerWhiskerLength, a.LQuartile, a.Median, a.UQuartile, a.Mean, a.Minimum, a.Maximum, a.SmallOutliers.Length, a.LargeOutliers.Length}
    End Function
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)><DataRow(10)><DataRow(11)><DataRow(12)>
    Public Sub BoxFrozenSource(fixture As Integer)
        For Each values In {Inputs()(fixture), Inputs()(fixture).Reverse().ToArray()}
            Dim groups As Double()() = {values, New Double() {8, 1, 3, 3, 4}}
            Dim before = groups.Select(Function(g) DirectCast(g.Clone(), Double())).ToArray()
            Dim old As New LegacyBox32(groups), actual As BoxPlotSeriesData = Nothing
            Dim a = Graphics30Tests.ExceptionType(Sub()
                                                     old.Calculate() : old.CalcForPlotting()
                                                 End Sub)
            Dim b = Graphics30Tests.ExceptionType(Sub() actual = BoxPlotDataCore.Compute(groups, {"same", "same"}))
            Assert.AreEqual(a, b)
            Dim expected As New Collections.Generic.List(Of Double), obtained As New Collections.Generic.List(Of Double)
            If a Is Nothing Then
                For i As Integer = 0 To groups.Length - 1
                    Dim groupIndex = i
                    Dim g = actual.Groups(i)
                    Dim e = {old.pPlotBlank(i), old.pPlotMedian(i), old.pPlotQ3(i), old.pPlotQ3Minus(i), old.pPlotMedianMinus(i), old.pWhiskerQ3(i), old.pWhiskerQ1(i), old.pQ1(i), old.pMedians(i), old.pQ3(i), old.pPlotMeans(i), old.pMins(i), old.pMaxs(i), CDbl(old.pArNOutliersSmall(i)), CDbl(old.pArNOutliersBig(i))}
                    Graphics31Evidence.EqualBits(e, BoxVector(g)) : expected.AddRange(e) : obtained.AddRange(BoxVector(g))
                    Graphics31Evidence.EqualBits(Enumerable.Range(0, CInt(old.pArNOutliersSmall(i))).Select(Function(j) old.pArOutliersSmall(j, groupIndex)).ToArray(), g.Aggregate.SmallOutliers)
                    Graphics31Evidence.EqualBits(Enumerable.Range(0, CInt(old.pArNOutliersBig(i))).Select(Function(j) old.pArOutliersBig(j, groupIndex)).ToArray(), g.Aggregate.LargeOutliers)
                    Graphics31Evidence.EqualBits(groups(i), g.Aggregate.RawValues)
                    For j As Integer = 0 To values.Length - 1
                        If i = 0 Then Graphics31Evidence.EqualBits({i + 1.0 + LegacyDomain32.DeterministicJitter(j, 0.12)}, {g.ObservationX(j)})
                    Next
                Next
            End If
            For i As Integer = 0 To groups.Length - 1
                Assert.AreEqual(Graphics31Evidence.Bits(before(i)), Graphics31Evidence.Bits(groups(i)))
            Next
            Graphics31Evidence.Record("BOX32-" & fixture & "-" & Graphics31Evidence.Bits(values), Graphics31Evidence.Bits(values), "standalone DescriptiveStat;halfWidth=.12", "15 scalars/group;raw+sorted identities separate", expected.ToArray(), obtained.ToArray(), a, b)
        Next
    End Sub
    <TestMethod>
    Public Sub BoxCopySafetyFenceIdentityAndMatrixColumns()
        Dim values = {4.0, 1, 4, 2, 1}, labels = {" A ", " A "}
        Dim p = BoxPlotDataCore.Compute({values, New Double() {3, 3, 3}}, labels)
        Dim baseline = BoxVector(p.Groups(0)), raw = p.Groups(0).Aggregate.RawValues
        values(0) = 999 : labels(0) = "changed" : p.Labels(0) = "changed" : p.Groups(0) = Nothing
        Assert.AreEqual(" A ", p.Labels(0)) : Graphics31Evidence.EqualBits(raw, p.Groups(0).Aggregate.RawValues)
        MutateArrayGetters(p) : MutateArrayGetters(p.Groups(0)) : MutateArrayGetters(p.Groups(0).Aggregate)
        Graphics31Evidence.EqualBits(baseline, BoxVector(p.Groups(0)))
        CollectionAssert.AreEqual({1, 4, 3, 0, 2}, p.Groups(0).Aggregate.SortedToInput)
        Dim exact = BoxPlotDataCore.Aggregate({0, 0, 0, 0, 1, 1, 1, 1, 2.5})
        Dim over = BoxPlotDataCore.Aggregate({0, 0, 0, 0, 1, 1, 1, 1, 2.5000000000000004})
        Assert.AreEqual(0, exact.LargeOutliers.Length) : Assert.AreEqual(1, over.LargeOutliers.Length)
        Dim matrix(,) As Double = {{4, -2}, {1, 0}, {3, 1}}
        Dim m = BoxPlotDataCore.Compute(matrix, {"", "duplicate"})
        Graphics31Evidence.EqualBits({4, 1, 3}, m.Groups(0).Aggregate.RawValues)
        Graphics31Evidence.EqualBits({-2, 0, 1}, m.Groups(1).Aggregate.RawValues)
        Graphics31Evidence.EqualBits({1, 2}, m.GroupPositions)
        Assert.AreEqual(0, m.Groups(0).GroupOrdinal)
    End Sub
    <TestMethod>
    Public Sub QuartileMethodIdentityUsesActualSharedCdfPath()
        Dim values = {1.0, 2, 3, 4, 5, 6}
        Dim b = BoxPlotDataCore.Compute({values}, {"a"}).Groups(0).Aggregate
        Dim h = New HistogramTukeyData(values), v = ViolinPlotCore.Compute(values, New String() {"a", "a", "a", "a", "a", "a"}).Series(0)
        Graphics31Evidence.EqualBits({2, 3.5, 5}, {b.LQuartile, b.Median, b.UQuartile})
        Graphics31Evidence.EqualBits({2, 3.5, 5}, {h.Q1, h.Median, h.Q3})
        Graphics31Evidence.EqualBits({2, 3.5, 5}, {v.Q1, v.Median, v.Q3})
        Assert.AreNotEqual(2.25, b.LQuartile) 'Hand-specified type7 counterexample;not an adopted convention.
        Assert.AreNotEqual(4.75, b.UQuartile)
    End Sub
    Public Shared Sub MutateArrayGetters(instance As Object)
        For Each prop In instance.GetType().GetProperties().Where(Function(x) x.PropertyType.IsArray)
            Dim before = DirectCast(prop.GetValue(instance, Nothing), Array)
            If before Is Nothing OrElse before.Length = 0 Then Continue For
            Dim expected = before.Cast(Of Object)().ToArray(), indexes = New Integer(before.Rank - 1) {}
            Dim t = before.GetType().GetElementType(), replacement As Object = Nothing
            If t Is GetType(Double) OrElse t Is GetType(Object) Then replacement = 999.0
            If t Is GetType(Integer) Then replacement = 999
            If t Is GetType(Boolean) Then replacement = Not CBool(before.GetValue(indexes))
            If t Is GetType(String) Then replacement = "changed"
            before.SetValue(replacement, indexes)
            Dim after = DirectCast(prop.GetValue(instance, Nothing), Array)
            CollectionAssert.AreEqual(expected, after.Cast(Of Object)().ToArray(), prop.Name)
        Next
    End Sub

    <DataTestMethod><DataRow("")><DataRow("(Sturges)")><DataRow("(Doane)")><DataRow("(Scott)")><DataRow("(Freedman-Diaconis)")>
    Public Sub HistogramFrozenPayloadAndAxes(rule As String)
        Dim fixtures = Inputs().Concat({New Double() {1, 1 + 2.2204460492503131E-16, 1}, DirectCast(Nothing, Double())}).ToArray()
        For f As Integer = 0 To fixtures.Length - 1
            For Each overlay In {False, True}
                For Each box In {False, True}
                    Dim input = fixtures(f), old As New LegacyHistogram32(input), result As HistogramPlotSeriesData = Nothing
                    Dim bins(,) As Object = Nothing
                    Dim a = Graphics30Tests.ExceptionType(Sub() bins = old.Compute(overlay, rule, box))
                    Dim b = Graphics30Tests.ExceptionType(Sub() result = HistogramPlotDataCore.Compute(input, overlay, rule, box))
                    Assert.AreEqual(a, b, rule & ":" & f)
                    Dim e() As Double = Nothing, actual() As Double = Nothing
                    If a Is Nothing Then
                        CollectionAssert.AreEqual(bins, result.Bins)
                        Graphics31Evidence.EqualBits(old.arBinMidVal, result.Midpoints) : CollectionAssert.AreEqual(old.arFreq, result.Frequencies)
                        Graphics31Evidence.EqualBits(old.arXi, result.NormalX) : Graphics31Evidence.EqualBits(old.arGauss, result.NormalY)
                        Graphics31Evidence.EqualBits(old.arOutliers, result.Outliers)
                        Graphics31Evidence.EqualBits({old.pQ1, old.pMedian, old.pQ3, old.pWhiskerLow, old.pWhiskerHigh}, {result.Q1, result.Median, result.Q3, result.WhiskerLow, result.WhiskerHigh})
                        e = old.Axes(box) : actual = AxisVector(result.Axis)
                        Graphics31Evidence.EqualBits(e, actual)
                        Assert.AreEqual(result.Midpoints.Length + 1, result.ClassificationEdges.Length)
                        MutateArrayGetters(result)
                    End If
                    Graphics31Evidence.Record("HIST32-" & rule & "-" & f & "-" & overlay & "-" & box, Graphics31Evidence.Bits(input), "rule=" & rule & ";overlay=" & overlay & ";box=" & box, "axis=7 scalars;bins=n*2;overlay=100*2 or Nothing", e, actual, a, b)
                Next
            Next
        Next
    End Sub
    Public Shared Function AxisVector(a As HistogramAxisData) As Double()
        Return {a.XMinimum, a.XMaximum, a.YMinimum, a.YMaximum, a.YMajorUnit, a.BoxCentreY, a.BoxHalfHeight}
    End Function
    <TestMethod>
    Public Sub HistogramTrueEdgesBoundaryMembershipOverlayCopiesAndFiniteAxisPolicy()
        Dim input = Enumerable.Range(0, 10).Select(Function(i) CDbl(i)).ToArray(), before = Graphics31Evidence.Bits(input)
        Dim p = HistogramPlotDataCore.Compute(input, True, "(Sturges)", True)
        Graphics31Evidence.EqualBits({0, 5, 10}, p.ClassificationEdges)
        Graphics31Evidence.EqualBits({2.5, 7.5}, p.Midpoints)
        CollectionAssert.AreEqual({5, 5}, p.Frequencies)
        Assert.AreEqual(before, Graphics31Evidence.Bits(input))
        input(0) = 999 : Graphics31Evidence.EqualBits({2.5, 7.5}, p.Midpoints)
        Dim off = HistogramPlotDataCore.Compute({0, 0, 0}, False, "", False)
        Assert.IsNull(off.NormalX) : Assert.IsNull(off.NormalY) : Assert.IsNull(off.Outliers)
        Assert.IsFalse(off.HasBoxSummary) : Assert.IsFalse(off.DrawBoxSummary)
        Dim old As New LegacyHistogram32({0}) With {.arBinMidVal = {0, 1}, .arFreq = {-99, 0}, .arXi = {0}, .arGauss = {Double.NaN, Double.PositiveInfinity, -5, 10}}
        Graphics31Evidence.EqualBits(old.Axes(False), AxisVector(New HistogramAxisData(old.arBinMidVal, old.arFreq, old.arGauss, False)))
        Dim bin = HistogramDataCore.BinsAndEdgesCopy({0}, "")
        MutateArrayGetters(bin) : Graphics31Evidence.EqualBits({-1, 1}, bin.Edges)
    End Sub
    <DataTestMethod>
    <DataRow(0, 0, "0000000000000000|3FF3333333333334|3FC999999999999A")>
    <DataRow(1, 9, "0000000000000000|4024000000000000|4000000000000000")>
    <DataRow(-9, -1, "C024000000000000|0000000000000000|4000000000000000")>
    <DataRow(-3, 7, "C010000000000000|4020000000000000|4000000000000000")>
    <DataRow(7, -3, "C010000000000000|4020000000000000|4000000000000000")>
    <DataRow(4, 4, "400FAE147AE147AE|40103D70A3D70A3E|3F947AE147AE147B")>
    <DataRow(1E-150, 9E-150, "0000000000000000|21005DF0A267BCC9|20DA2FE76A3F9475")>
    <DataRow(1E+150, 9E+150, "0000000000000000|5F48708279E4BC5B|5F238D352E5096AF")>
    Public Sub CrossChartAxisExactBaseline(minimum As Double, maximum As Double, expected As String)
        Dim ordinary = ChartAxisScalingCore.Compute(minimum, maximum)
        Dim boxAxis = BoxPlotDataCore.Axis({minimum}, {maximum})
        Assert.AreEqual(expected, Graphics31Evidence.Bits({ordinary.Min, ordinary.Max, ordinary.Scale}))
        Assert.AreEqual(expected, Graphics31Evidence.Bits({boxAxis.Min, boxAxis.Max, boxAxis.Scale}))
        Graphics31Evidence.Record("CROSS32-" & minimum.ToString("R", CultureInfo.InvariantCulture) & "-" & maximum.ToString("R", CultureInfo.InvariantCulture), Graphics31Evidence.Bits({minimum, maximum}), "expected IEEE from actual pre-edit merged-main assembly;shared axis used without correction", "min/max/major", expected.Split("|"c).Select(Function(x) BitConverter.Int64BitsToDouble(Convert.ToInt64(x, 16))).ToArray(), {boxAxis.Min, boxAxis.Max, boxAxis.Scale})
    End Sub
End Class
