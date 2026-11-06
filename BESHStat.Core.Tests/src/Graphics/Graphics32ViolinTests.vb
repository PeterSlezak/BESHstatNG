Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch32Fixtures

<TestClass>
Public Class Graphics32ViolinTests
    <DataTestMethod><DataRow(0, 0)><DataRow(0, 1)><DataRow(0, 2)><DataRow(1, 0)><DataRow(1, 1)><DataRow(1, 2)><DataRow(2, 0)><DataRow(2, 1)><DataRow(2, 2)>
    Public Sub EveryBandwidthScaleTrimAndSourcePermutation(bandwidth As Integer, scale As Integer)
        Dim values = {3.0, Double.NaN, 1, 4, 2, 10, 6, 5}, groups As Object() = {" b ", "missing", 1, "1", 1, "", "b", Nothing}
        For Each useTrim As Boolean In {True, False}
            Dim options As New ViolinPlotOptions With {.BandwidthRule = CType(bandwidth, ViolinBandwidthRule), .ScaleMode = CType(scale, ViolinScaleMode), .ManualBandwidth = 1.2, .GridPoints = 32, .Trim = useTrim}
            Dim result = ViolinPlotCore.Compute(values, groups, options), domain As New ViolinPlotDomainData(result)
            Dim old = LegacyDomain32.ResolveLegacyAxis32(result, New LegacyViolinAppearance32())
            Dim expected = {old.XMinimum, old.XMaximum, old.YMinimum, old.YMaximum}, actual = {domain.XMinimum, domain.XMaximum, domain.YMinimum, domain.YMaximum}
            Graphics31Evidence.EqualBits(expected, actual)
            CollectionAssert.AreEqual({0, 6}, result.Series(0).SourceObservationOrdinals)
            CollectionAssert.AreEqual({2, 3, 4}, result.Series(1).SourceObservationOrdinals)
            CollectionAssert.AreEqual({"b", "1"}, domain.Labels)
            Assert.AreEqual(3, result.ExcludedObservationCount)
            Assert.IsFalse(domain.YMajorUnit.HasValue)
            Graphics31Evidence.EqualBits({1, 2}, domain.GroupPositions)
            Graphics31Evidence.EqualBits({old.YMinimum + 0.012 * (old.YMaximum - old.YMinimum), old.YMinimum + 0.012 * (old.YMaximum - old.YMinimum)}, domain.CategoryLabelY)
            For i As Integer = 0 To domain.Series.Length - 1
                Dim source = result.Series(i), geometry = domain.Series(i), expectedX As New Collections.Generic.List(Of Double), expectedY As New Collections.Generic.List(Of Double)
                For j As Integer = 0 To source.EvaluationPoints.Length - 1
                    expectedX.Add(i + 1.0 + source.ScaledHalfWidths(j)) : expectedY.Add(source.EvaluationPoints(j))
                Next
                For j As Integer = source.EvaluationPoints.Length - 1 To 0 Step -1
                    expectedX.Add(i + 1.0 - source.ScaledHalfWidths(j)) : expectedY.Add(source.EvaluationPoints(j))
                Next
                Graphics31Evidence.EqualBits(expectedX.ToArray(), geometry.Outline.X) : Graphics31Evidence.EqualBits(expectedY.ToArray(), geometry.Outline.Y)
                For j As Integer = 0 To source.SampleSize - 1
                    Graphics31Evidence.EqualBits({i + 1.0 + LegacyDomain32.DeterministicJitter(j, 0.12)}, {geometry.ObservationX(j)})
                    Assert.AreEqual(BitConverter.DoubleToInt64Bits(values(geometry.SourceObservationOrdinals(j))), BitConverter.DoubleToInt64Bits(geometry.Observations(j)))
                Next
                Graphics32Tests.MutateArrayGetters(geometry) : Graphics32Tests.MutateArrayGetters(geometry.Outline)
                Graphics32Tests.MutateArrayGetters(source)
                CollectionAssert.AreEqual(result.Series(i).SourceObservationOrdinals, geometry.SourceObservationOrdinals)
            Next
            Graphics32Tests.MutateArrayGetters(domain)
            Dim lowerBounded As Array = Array.CreateInstance(GetType(Object), {groups.Length}, {7})
            For i As Integer = 0 To groups.Length - 1
                lowerBounded.SetValue(groups(i), i + 7)
            Next
            Dim shifted = ViolinPlotCore.Compute(values, lowerBounded, options)
            For i As Integer = 0 To shifted.GroupCount - 1
                CollectionAssert.AreEqual(result.Series(i).SourceObservationOrdinals, shifted.Series(i).SourceObservationOrdinals)
                Graphics31Evidence.EqualBits(result.Series(i).Density, shifted.Series(i).Density)
            Next
            Graphics31Evidence.Record("VIOLIN32-" & bandwidth & "-" & scale & "-" & useTrim, Graphics31Evidence.Bits(values), "bandwidth=" & bandwidth & ";scale=" & scale & ";trim=" & useTrim & ";grid=32", "axis=4;outline=2grid/series;input ordinals available;physical row unavailable", expected, actual)
        Next
    End Sub
    <TestMethod>
    Public Sub AutomaticFallbackExplicitLimitsAndJitterPolicies()
        For Each bounds In New Double()() {New Double() {0, 0}, New Double() {-9, -1}, New Double() {-3, 7}, New Double() {9, 1}, New Double() {4, 4}, New Double() {1.0E-150, 9.0E-150}, New Double() {1.0E+150, 9.0E+150}, New Double() {Double.NaN, Double.PositiveInfinity}}
            Dim low As Double, high As Double
            LegacyDomain32.ResolveAutomaticYLimits(bounds(0), bounds(1), low, high)
            Dim actual = ViolinPlotDomainData.AutomaticLimits(bounds(0), bounds(1))
            Graphics31Evidence.EqualBits({low, high}, actual)
            Graphics31Evidence.Record("AXIS32-" & Graphics31Evidence.Bits(bounds), Graphics31Evidence.Bits(bounds), "violin automatic fallback", "min/max", {low, high}, actual)
        Next
        Dim result = ViolinPlotCore.Compute({2, 2, 2}, {"a", "a", "a"})
        For Each minimum As Double? In {DirectCast(Nothing, Double?), -10.0, 1.0, 100.0}
            For Each maximum As Double? In {DirectCast(Nothing, Double?), 10.0, 3.0}
                Dim old = New LegacyViolinAppearance32 With {.YAxisMinimum = minimum, .YAxisMaximum = maximum}
                Dim expected As LegacyAxis32 = Nothing, actual As ViolinPlotDomainData = Nothing
                Dim a = Graphics30Tests.ExceptionType(Sub() expected = LegacyDomain32.ResolveLegacyAxis32(result, old))
                Dim b = Graphics30Tests.ExceptionType(Sub() actual = New ViolinPlotDomainData(result, minimum, maximum, 0.5))
                Assert.AreEqual(a, b)
                If a Is Nothing Then
                    Graphics31Evidence.EqualBits({expected.YMinimum, expected.YMaximum}, {actual.YMinimum, actual.YMaximum})
                    Assert.AreEqual(0.5, actual.YMajorUnit.Value)
                End If
            Next
        Next
        For Each width In {0.0, -0.1, 0.12, 0.49}
            For i As Integer = 0 To 39
                Graphics31Evidence.EqualBits({LegacyDomain32.DeterministicJitter(i, width)}, {DistributionPlotGeometryCore.DeterministicJitter(i, width)})
            Next
        Next
        CollectionAssert.AreEqual({False, True, True, False}, DistributionPlotGeometryCore.VisibleObservations({-2, -1, 1, 2}, -1, 1))
        Dim span = DistributionPlotGeometryCore.ViolinInnerX(1, {0.1, 0.4}, 0.1, 0.2)
        Graphics31Evidence.EqualBits({0.9, 1.1, 1 - 0.4 * 0.35, 1 + 0.4 * 0.35}, span)
        Assert.ThrowsException(Of ArgumentException)(Sub() DistributionPlotGeometryCore.ViolinOutline(1, {1}, {1}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() ViolinPlotCore.Compute({Double.PositiveInfinity}, {"a"}))
    End Sub
End Class
