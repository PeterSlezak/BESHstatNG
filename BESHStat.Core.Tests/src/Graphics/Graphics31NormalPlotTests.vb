Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch30Fixtures

<TestClass>
Public Class Graphics31NormalPlotTests
    Public Shared Function Inputs() As Double()()
        Return {New Double() {9, 1, 4, 4, 2, 8}, New Double() {1, 2, 4, 4, 8, 9}, New Double() {9, 8, 4, 4, 2, 1},
                New Double() {2, 2, 2, 2}, New Double() {3}, New Double() {2, 1}, New Double() {1.0E-150, -2.0E-150, 0, 4.0E-150},
                New Double() {1.0E+150, -2.0E+150, 0, 4.0E+150}, New Double() {}}
    End Function

    <DataTestMethod>
    <DataRow("Blom", "SPSS")><DataRow("Blom", "OLS")><DataRow("Blom", "R")>
    <DataRow("Rankit", "SPSS")><DataRow("Rankit", "OLS")><DataRow("Rankit", "R")>
    <DataRow("Van Der Waerden", "SPSS")><DataRow("Van Der Waerden", "OLS")><DataRow("Van Der Waerden", "R")>
    Public Sub NineCombinations(scoreMethod As String, lineMethod As String)
        Dim fixtures = Inputs()
        For fixture As Integer = 0 To fixtures.Length - 1
            Compare(fixtures(fixture), scoreMethod, lineMethod, "QQ31-" & fixture & "-" & scoreMethod & "-" & lineMethod)
        Next
    End Sub

    Private Shared Sub Compare(input() As Double, method As String, line As String, id As String)
        Dim before = DirectCast(input.Clone(), Double()), legacy As New LegacyNormal30(DirectCast(input.Clone(), Double()), Nothing)
        Dim result As NormalPlotSeriesData = Nothing, expectedLine As Double(,) = Nothing
        Dim a = Graphics30Tests.ExceptionType(Sub()
                                                 legacy.compute(method)
                                                 expectedLine = legacy.computeRefLIne(line)
                                             End Sub)
        Dim b = Graphics30Tests.ExceptionType(Sub() result = NormalPlotSeriesData.Compute(input, method, line))
        Assert.AreEqual(a, b, id)
        Assert.AreEqual(Bits(before), Bits(input))
        If a Is Nothing Then
            Dim sorted = DirectCast(before.Clone(), Double()) : Array.Sort(sorted)
            EqualBits(sorted, result.X) : EqualBits(legacy.pZ, result.Y) : EqualBits(legacy.pP, result.Probabilities)
            EqualBits(Graphics30Tests.Flat(expectedLine), Graphics30Tests.Flat(result.ReferenceLine))
            EqualBits({expectedLine(0, 0), expectedLine(1, 0)}, result.ReferenceX)
            EqualBits({expectedLine(0, 1), expectedLine(1, 1)}, result.ReferenceY)
            Assert.AreEqual(2, result.ReferenceLine.GetLength(0)) : Assert.AreEqual(2, result.ReferenceLine.GetLength(1))
            Assert.AreEqual(method, result.ScoreMethod) : Assert.AreEqual(line, result.LineMethod)
            Assert.AreEqual(input.Length, result.PointCount) : Assert.AreEqual(input.Length, result.Probabilities.Length)
            Assert.AreEqual(input.Length, result.Y.Length)
            VerifyPermutation(before, result)
            Dim again = NormalPlotSeriesData.Compute(before, method, line)
            EqualBits(result.X, again.X) : EqualBits(result.Y, again.Y)
            CollectionAssert.AreEqual(result.SortedToSource, again.SortedToSource)
            Record(id, Bits(before), "score=" & If(method, "Nothing") & ";line=" & If(line, "Nothing"), "X/Y/P=n;reference=2x2 rows=endpoints cols=X/Y", sorted.Concat(legacy.pZ).Concat(legacy.pP).Concat(Graphics30Tests.Flat(expectedLine)).ToArray(), result.X.Concat(result.Y).Concat(result.Probabilities).Concat(Graphics30Tests.Flat(result.ReferenceLine)).ToArray())
        Else
            Record(id, Bits(before), "score=" & If(method, "Nothing") & ";line=" & If(line, "Nothing"), "exception", Nothing, Nothing, a, b)
        End If
    End Sub

    Private Shared Sub VerifyPermutation(input() As Double, result As NormalPlotSeriesData)
        Dim perm = result.SortedToSource, inverse = result.SourceToSorted
        CollectionAssert.AreEquivalent(Enumerable.Range(0, input.Length).ToArray(), perm)
        For i As Integer = 0 To input.Length - 1
            Assert.AreEqual(i, inverse(perm(i)))
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(input(perm(i))), BitConverter.DoubleToInt64Bits(result.X(i)))
            If i > 0 AndAlso BitConverter.DoubleToInt64Bits(result.X(i)) = BitConverter.DoubleToInt64Bits(result.X(i - 1)) Then
                Assert.IsTrue(perm(i - 1) < perm(i))
            End If
        Next
    End Sub

    <TestMethod>
    Public Sub StableTiesInverseAndIeeeDistinguishableSlots()
        Dim input = {4.0, 1, 4, 2, 1}, order As New NormalPlotObservationOrder(input)
        CollectionAssert.AreEqual({1, 4, 3, 0, 2}, order.SortedToSource)
        CollectionAssert.AreEqual({3, 0, 4, 2, 1}, order.SourceToSorted)
        Dim special = Enumerable.Range(0, 40).Select(Function(i) If(i Mod 3 = 0, BitConverter.Int64BitsToDouble(Long.MinValue), If(i Mod 3 = 1, 0.0, BitConverter.Int64BitsToDouble(&H7FF8000000000001L)))).ToArray()
        Dim sorted = NormalPlotDataCore.SortedCopy(special), projection As New NormalPlotObservationOrder(special)
        Assert.AreEqual(Bits(sorted), Bits(projection.Data))
        For i As Integer = 0 To sorted.Length - 1
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(sorted(i)), BitConverter.DoubleToInt64Bits(special(projection.SortedToSource(i))))
        Next
        For Each bitGroup In Enumerable.Range(0, sorted.Length).GroupBy(Function(i) BitConverter.DoubleToInt64Bits(sorted(i)))
            Dim mapped = bitGroup.Select(Function(i) projection.SortedToSource(i)).ToArray()
            CollectionAssert.AreEqual(mapped.OrderBy(Function(i) i).ToArray(), mapped)
        Next
        input(0) = 999 : order.Data(0) = 999 : order.SortedToSource(0) = 999 : order.SourceToSorted(0) = 999
        EqualBits({1, 1, 2, 4, 4}, order.Data)
        CollectionAssert.AreEqual({1, 4, 3, 0, 2}, order.SortedToSource)
    End Sub

    <TestMethod>
    Public Sub AllReturnedVectorsAndReferenceSnapshotsAreIndependent()
        Dim input = {4.0, 1, 4, 2, 1}, before = DirectCast(input.Clone(), Double())
        Dim result = NormalPlotSeriesData.Compute(input, "Blom", "OLS")
        Dim x = result.X, y = result.Y, p = result.Probabilities, matrix = result.ReferenceLine
        Dim xbits = Bits(x), ybits = Bits(y), pbits = Bits(p), mbits = Bits(Graphics30Tests.Flat(matrix))
        input(0) = 999 : x(0) = 999 : y(0) = 999 : p(0) = 999 : matrix(0, 0) = 999
        result.ReferenceX(0) = 999 : result.ReferenceY(0) = 999 : result.SortedToSource(0) = 999 : result.SourceToSorted(0) = 999
        Assert.AreEqual(xbits, Bits(result.X)) : Assert.AreEqual(ybits, Bits(result.Y))
        Assert.AreEqual(pbits, Bits(result.Probabilities)) : Assert.AreEqual(mbits, Bits(Graphics30Tests.Flat(result.ReferenceLine)))
        CollectionAssert.AreEqual({1, 4, 3, 0, 2}, result.SortedToSource)
        Dim again = NormalPlotSeriesData.Compute(before, "Blom", "OLS")
        Assert.AreEqual(Bits(again.X), Bits(result.X)) : Assert.AreEqual(Bits(again.Y), Bits(result.Y))
        Dim order As New NormalPlotObservationOrder(before), probs = result.Probabilities, scores = result.Y, line = result.ReferenceLine
        Dim projected = NormalPlotSeriesData.Project(order, probs, scores, "Blom", line, "OLS")
        probs(0) = 88 : scores(0) = 88 : line(0, 0) = 88
        Assert.AreEqual(pbits, Bits(projected.Probabilities)) : Assert.AreEqual(ybits, Bits(projected.Y))
        Assert.AreEqual(mbits, Bits(Graphics30Tests.Flat(projected.ReferenceLine)))
    End Sub

    <TestMethod>
    Public Sub CaseSensitiveFallbackNonfiniteEmptyAndMissing()
        Dim fixtures As Double()() = {New Double() {3, 1, 2}, New Double() {}, New Double() {Double.NaN, 1, Double.PositiveInfinity, Double.NegativeInfinity}}
        For f As Integer = 0 To fixtures.Length - 1
            For Each method In {"Blom", "blom", "Rankit", "Van Der Waerden", "Van der Waerden", "unknown", Nothing}
                For Each line In {"SPSS", "OLS", "R", "r", "unknown", Nothing}
                    Compare(fixtures(f), method, line, "QQ31-EDGE-" & f & "-" & If(method, "Nothing") & "-" & If(line, "Nothing"))
                Next
            Next
        Next
        Assert.AreEqual(Graphics30Tests.ExceptionType(Sub() NormalPlotDataCore.SortedCopy(Nothing)), Graphics30Tests.ExceptionType(Sub() GC.KeepAlive(New NormalPlotObservationOrder(Nothing))))
        Assert.AreEqual(Graphics30Tests.ExceptionType(Sub() NormalPlotDataCore.Compute(Nothing, "Blom")), Graphics30Tests.ExceptionType(Sub() NormalPlotSeriesData.Compute(Nothing, "Blom", "OLS")))
        Dim zero = NormalPlotSeriesData.Compute({3, 1, 2}, "blom", "unknown")
        EqualBits({0, 0, 0}, zero.Y) : EqualBits({0, 0, 0}, zero.Probabilities)
        EqualBits({0, 0}, zero.ReferenceY)
    End Sub

    <TestMethod>
    Public Sub PreScoringProjectionDoesNotScoreOrFit()
        Dim order As New NormalPlotObservationOrder({3, 1, 2}), line(1, 1) As Double
        Dim result = NormalPlotSeriesData.Project(order, Nothing, Nothing, Nothing, line)
        EqualBits({1, 2, 3}, result.X) : Assert.IsNull(result.Y) : Assert.IsNull(result.Probabilities)
        EqualBits({0, 0}, result.ReferenceX) : EqualBits({0, 0}, result.ReferenceY)
        Assert.AreEqual(3, result.PointCount)
        Dim noLine = NormalPlotSeriesData.Project(order, Nothing, Nothing, Nothing, Nothing)
        Assert.IsNull(noLine.ReferenceLine) : Assert.IsNull(noLine.ReferenceX) : Assert.IsNull(noLine.ReferenceY)
        Assert.ThrowsException(Of ArgumentNullException)(Sub() NormalPlotSeriesData.Project(Nothing, Nothing, Nothing, Nothing, Nothing))
    End Sub
End Class
