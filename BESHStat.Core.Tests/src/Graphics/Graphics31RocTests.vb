Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch30Fixtures

<TestClass>
Public Class Graphics31RocTests
    Public Shared Function Groups(fixture As Integer) As Double()()
        Select Case fixture
            Case 0 : Return {New Double() {9, 8, 7}, New Double() {1, 2, 3}}
            Case 1 : Return {New Double() {1, 2, 3}, New Double() {9, 8, 7}}
            Case 2 : Return {New Double() {5, 1, 3, 3}, New Double() {0, 2, 3}}
            Case 3 : Return {New Double() {2, 2, 2}, New Double() {2, 2}}
            Case 4 : Return {New Double() {4, 2}, New Double() {1, 2, 2, 3, 4, 5, 6}}
            Case 5 : Return {New Double() {1, 1}, New Double() {1, 2}}
            Case 6 : Return {New Double() {BitConverter.Int64BitsToDouble(Long.MinValue), 0, 1, 1}, New Double() {0, 0, -1}}
            Case 7 : Return {New Double() {Double.NaN, 1, 3}, New Double() {1, 2}}
            Case 8 : Return {New Double() {Double.PositiveInfinity, 1}, New Double() {Double.NegativeInfinity, 0}}
            Case Else : Return {New Double() {1, 1 + 2.2204460492503131E-16, 2}, New Double() {1, 2, 2}}
        End Select
    End Function

    <DataTestMethod>
    <DataRow(0, 0.05)><DataRow(1, 0.05)><DataRow(2, 0.05)><DataRow(3, 0.05)><DataRow(4, 0.05)>
    <DataRow(5, 0.1)><DataRow(6, 0.01)><DataRow(7, 0.05)><DataRow(8, 0.05)><DataRow(9, 0.1)>
    Public Sub FullCoordinateAndStatisticsCompatibility(fixture As Integer, alpha As Double)
        Dim groupsData = Groups(fixture), before = groupsData.Select(Function(x) DirectCast(x.Clone(), Double())).ToArray()
        Dim legacy As New LegacyRoc30(groupsData, Nothing), result As RocDataResult = Nothing
        Dim expectedError = Graphics30Tests.ExceptionType(Sub() legacy.compute(alpha))
        Dim actualError = Graphics30Tests.ExceptionType(Sub() result = RocDataCore.Compute(groupsData, alpha))
        Assert.AreEqual(expectedError, actualError)
        Dim input = String.Join(";", before.Select(Function(x) Graphics31Evidence.Bits(x)))
        If expectedError Is Nothing Then
            Dim series As New RocPlotSeriesData(result)
            EqualBits(legacy.par1minusSpec, series.X) : EqualBits(legacy.parSensitivity, series.Y)
            EqualBits(legacy.parCutOff, series.Cutoffs) : EqualBits(legacy.parSpecificity, series.Specificity)
            EqualBits({legacy.pAUC, legacy.pseAUC, legacy.pPvalue, legacy.pdelongSE, legacy.pCI.LowerLimit, legacy.pCI.UpperLimit, legacy.pdelongCI.Estimate, legacy.pdelongCI.LowerLimit, legacy.pdelongCI.UpperLimit},
                      {result.Auc, result.StandardError, result.PValue, result.DeLongStandardError, result.Interval.LowerLimit, result.Interval.UpperLimit, result.DeLongInterval.Estimate, result.DeLongInterval.LowerLimit, result.DeLongInterval.UpperLimit})
            Assert.AreEqual(series.CutoffCount + 1, series.PointCount)
            Assert.AreEqual(series.CutoffCount, series.Specificity.Length)
            Assert.AreEqual(series.PointCount, series.Y.Length)
            CollectionAssert.AreEqual(Enumerable.Range(1, series.CutoffCount).ToArray(), series.CutoffPointIndexes)
            EqualBits({1, 0}, {series.X(0), series.X(series.PointCount - 1)})
            EqualBits({1, 0}, {series.Y(0), series.Y(series.PointCount - 1)})
            For i As Integer = 0 To series.CutoffCount - 2
                EqualBits({1.0 - legacy.parSpecificity(i)}, {series.X(series.CutoffPointIndexes(i))})
            Next
            'Terminal coordinate is forced; do not infer an additional threshold or
            'overwrite exceptional terminal specificity to force numerical agreement.
            EqualBits({0, 1}, series.ReferenceX) : EqualBits({0, 1}, series.ReferenceY)
            EqualBits({0, 1, 0.2, 0}, {series.AxisMinimum, series.AxisMaximum, series.AxisMajorUnit, series.AxisCrossing})
            Record("ROC31-" & fixture & "-" & alpha.ToString(CultureInfo.InvariantCulture), input, "alpha=" & alpha.ToString(CultureInfo.InvariantCulture), "cut/spec=u;X/Y=u+1;diagonal=2", legacy.par1minusSpec.Concat(legacy.parSensitivity).Concat(legacy.parCutOff).Concat(legacy.parSpecificity).ToArray(), series.X.Concat(series.Y).Concat(series.Cutoffs).Concat(series.Specificity).ToArray())
            Record("ROC31-STATS-" & fixture, input, "alpha=" & alpha.ToString(CultureInfo.InvariantCulture), "9 scalars", {legacy.pAUC, legacy.pseAUC, legacy.pPvalue, legacy.pdelongSE, legacy.pCI.LowerLimit, legacy.pCI.UpperLimit, legacy.pdelongCI.Estimate, legacy.pdelongCI.LowerLimit, legacy.pdelongCI.UpperLimit}, {result.Auc, result.StandardError, result.PValue, result.DeLongStandardError, result.Interval.LowerLimit, result.Interval.UpperLimit, result.DeLongInterval.Estimate, result.DeLongInterval.LowerLimit, result.DeLongInterval.UpperLimit})
        Else
            Record("ROC31-ERROR-" & fixture, input, "alpha=" & alpha.ToString(CultureInfo.InvariantCulture), "exception", Nothing, Nothing, expectedError, actualError)
        End If
        For i As Integer = 0 To groupsData.Length - 1
            Assert.AreEqual(Bits(before(i)), Bits(groupsData(i)))
        Next
    End Sub

    <TestMethod>
    Public Sub FrozenTieCreditDirectionAndCutoffExamples()
        For Each fixture In {0, 1, 3}
            Dim expected = If(fixture = 0, 1.0, If(fixture = 1, 0.0, 0.5))
            Assert.AreEqual(expected, RocDataCore.Compute(Groups(fixture)).Auc)
        Next
        Dim r = RocDataCore.Compute(Groups(3)), p As New RocPlotSeriesData(r)
        EqualBits({3}, p.Cutoffs) : EqualBits({1, 0}, p.X) : EqualBits({1, 0}, p.Y)
        Assert.AreEqual(1, p.CutoffPointIndexes(0))
        'Half-credit is existing compatibility, not an independent scientific oracle.
    End Sub

    <TestMethod>
    Public Sub CopiesInputsIntervalsVectorsAndSeedDeterminism()
        Dim g = Groups(2), r = RocDataCore.Compute(g), p As New RocPlotSeriesData(r)
        Dim x = p.X, y = p.Y, cut = p.Cutoffs, spec = p.Specificity
        Dim xbits = Bits(x), ybits = Bits(y), cbits = Bits(cut), sbits = Bits(spec)
        x(0) = 99 : y(0) = 99 : cut(0) = 99 : spec(0) = 99
        p.ReferenceX(0) = 99 : p.ReferenceY(0) = 99 : p.CutoffPointIndexes(0) = 99
        r.FalsePositiveRate(0) = 99 : r.Sensitivity(0) = 99 : r.Specificity(0) = 99 : r.Cutoffs(0) = 99
        Dim originalInterval = r.Interval.LowerLimit, originalDeLong = r.DeLongInterval.UpperLimit
        r.Interval.LowerLimit = 99 : r.DeLongInterval.UpperLimit = 99
        g(0)(0) = 999 : g(1) = {999, 999}
        Assert.AreEqual(xbits, Bits(p.X)) : Assert.AreEqual(ybits, Bits(p.Y))
        Assert.AreEqual(cbits, Bits(p.Cutoffs)) : Assert.AreEqual(sbits, Bits(p.Specificity))
        Assert.AreEqual(originalInterval, r.Interval.LowerLimit) : Assert.AreEqual(originalDeLong, r.DeLongInterval.UpperLimit)
        EqualBits({0, 1}, p.ReferenceX) : EqualBits({0, 1}, p.ReferenceY)
        Assert.AreEqual(1, p.CutoffPointIndexes(0))
        Dim a = RocDataCore.ComputeCurve(Groups(2), 0.1, 0.75), b = RocDataCore.ComputeCurve(Groups(2), 0.1, 0.75)
        EqualBits({a.Auc, a.StandardError, a.PValue}, {b.Auc, b.StandardError, b.PValue})
        EqualBits(a.FalsePositiveRate, b.FalsePositiveRate)
        Dim legacy As New LegacyRoc30(Groups(2), Nothing)
        legacy.pAUC = 0.75 : legacy.compute(0.1)
        EqualBits({legacy.pAUC, legacy.pseAUC, legacy.pPvalue}, {a.Auc, a.StandardError, a.PValue})
    End Sub

    <TestMethod>
    Public Sub PermutationsBoundaryAndExceptionalCompatibility()
        For Each fixture In {0, 2, 3, 4, 6}
            Dim g = Groups(fixture), forward = RocDataCore.Compute(g)
            Dim reversed = RocDataCore.Compute(g.Select(Function(v) v.Reverse().ToArray()).ToArray())
            EqualBits(forward.Cutoffs, reversed.Cutoffs) : EqualBits(forward.FalsePositiveRate, reversed.FalsePositiveRate)
            EqualBits(forward.Sensitivity, reversed.Sensitivity) : EqualBits({forward.Auc}, {reversed.Auc})
        Next
        Dim invalid As Double()()() = {Nothing, New Double()() {}, New Double()() {Nothing, New Double() {1, 2}}, New Double()() {New Double() {}, New Double() {}}, New Double()() {New Double() {1}, New Double() {2, 3}}, New Double()() {New Double() {1, 2}, New Double() {}}}
        For i As Integer = 0 To invalid.Length - 1
            Dim g = invalid(i), old As New LegacyRoc30(g, Nothing)
            Dim a = Graphics30Tests.ExceptionType(Sub() old.compute()), b = Graphics30Tests.ExceptionType(Sub() RocDataCore.Compute(g))
            Assert.AreEqual(a, b)
            Record("ROC31-BOUNDARY-" & i, "see fixture " & i, "alpha=0.05", "exception", Nothing, Nothing, a, b)
        Next
        Assert.ThrowsException(Of ArgumentException)(Sub() RocDataCore.ComputeDeLong({New Double() {1}, New Double() {2, 3}}))
        Dim curveOnly = RocDataCore.ComputeCurve({New Double() {1}, New Double() {2, 3}})
        Assert.AreEqual(4, New RocPlotSeriesData(curveOnly).PointCount)
        Dim pre As New RocPlotSeriesData(DirectCast(Nothing, Double()), Nothing, Nothing, Nothing)
        Assert.IsNull(pre.X) : Assert.AreEqual(0, pre.PointCount)
        Assert.ThrowsException(Of ArgumentNullException)(Sub() GC.KeepAlive(New RocPlotSeriesData(DirectCast(Nothing, RocDataResult))))
    End Sub
End Class
