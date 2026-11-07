Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression
Imports BESHStatNG.StatisticalProcessControl
Imports BESHStatNG.TestFixtures34

<TestClass>
Public Class Graphics34Tests
    Public Shared Function Inputs(index As Integer) As Double()
        Return New Double()() {Nothing, New Double() {}, New Double() {1}, New Double() {-4, -1, 2, 9}, New Double() {2, 2, 2, 2}, New Double() {-0.0, 0, 1, 1, 2}, New Double() {Double.NaN, 1, Double.PositiveInfinity}, New Double() {Double.NegativeInfinity, Double.PositiveInfinity}, New Double() {1.0E+150, 2.0E+150}, New Double() {1.0E-150, 2.0E-150}}(index)
    End Function
    Public Shared Function ErrorIdentity(action As Action) As String
        Try
            action()
            Return "None"
        Catch ex As Exception
            Dim arg = TryCast(ex, ArgumentException)
            Return ex.GetType().FullName & ":" & If(arg Is Nothing, "", arg.ParamName)
        End Try
    End Function
    Public Shared Sub EqualBits(expected As Double(), actual As Double())
        If expected Is Nothing Then
            Assert.IsNull(actual)
            Return
        End If
        Assert.IsNotNull(actual)
        Assert.AreEqual(expected.Length, actual.Length)
        For i As Integer = 0 To expected.Length - 1
            If Double.IsNaN(expected(i)) Then
                Assert.IsTrue(Double.IsNaN(actual(i)), "NaN classification at " & i)
            Else
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected(i)), BitConverter.DoubleToInt64Bits(actual(i)), "IEEE bits at " & i)
            End If
        Next
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)>
    Public Sub LadderFrozenLabels(index As Integer)
        For Each gap In New Double() {-1, 0, 0.5, 20, Double.NaN, Double.PositiveInfinity}
            Dim expected As Double() = Nothing, actual As Double() = Nothing
            Dim oldError = ErrorIdentity(Sub() expected = LegacySpecialty34.SpreadLabelPositions(Inputs(index), -2, 4, gap))
            Dim newError = ErrorIdentity(Sub() actual = LadderLabelGeometryCore.SpreadLabelPositions(Inputs(index), -2, 4, gap))
            Assert.AreEqual(oldError, newError)
            If oldError = "None" Then EqualBits(expected, actual)
        Next
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)>
    Public Sub SymmetryFrozenOrientationAndErrors(index As Integer)
        Dim expected As Double()() = Nothing, actual As SymmetrySeriesData = Nothing
        Dim data = Inputs(index)
        Dim oldError = ErrorIdentity(Sub() expected = LegacySpecialty34.Symmetry(Inputs(index)))
        Dim newError = ErrorIdentity(Sub()
                                        Dim medianValue = StatFunc.Median(data)
                                        Array.Sort(data)
                                        actual = SymmetrySeriesData.FromSorted(data, medianValue)
                                    End Sub)
        Assert.AreEqual(oldError, newError)
        If oldError <> "None" Then Return
        EqualBits(expected(0), actual.XValues)
        EqualBits(expected(1), actual.YValues)
        EqualBits(expected(2), actual.ReferenceX)
        EqualBits(expected(3), actual.ReferenceY)
        EqualBits(expected(5), New Double() {actual.Axis.Min, actual.Axis.Max, actual.Axis.Scale})
        Dim xs = actual.XValues, reference = actual.ReferenceX
        If xs.Length > 0 Then xs(0) = 12345
        reference(0) = 12345
        data(0) = 54321
        EqualBits(expected(0), actual.XValues)
        EqualBits(expected(2), actual.ReferenceX)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)>
    Public Sub EcdfFrozenExtensionsAndCopyBoundaries(index As Integer)
        For mode As Integer = 0 To 3
            Dim x = Inputs(index), y = If(x Is Nothing, Nothing, x.Reverse().ToArray())
            If mode = 3 Then y = New Double() {2, 3}
            Dim minimum As Double = If(mode = 0, -10.0R, 0.0R), maximum As Double = If(mode = 1, 10.0R, 2.0R)
            Dim ex As Double() = Nothing, ey As Double() = Nothing
            LegacySpecialty34.ExtendStepToAxisBounds(x, y, minimum, maximum, ex, ey)
            Dim actual As New CumulativeStepSeriesData(x, y, minimum, maximum)
            EqualBits(ex, actual.XValues)
            EqualBits(ey, actual.YValues)
            Dim frozenX = actual.XValues, frozenY = actual.YValues
            If x IsNot Nothing AndAlso x.Length > 0 Then x(0) = 888
            If y IsNot Nothing AndAlso y.Length > 0 Then y(0) = 999
            Dim returned = actual.XValues
            If returned IsNot Nothing AndAlso returned.Length > 0 Then returned(0) = 123
            EqualBits(frozenX, actual.XValues)
            EqualBits(frozenY, actual.YValues)
        Next
    End Sub

    <TestMethod>
    Public Sub CalibrationBinAdmissionOrderAndSnapshots()
        Dim rows As New List(Of CalibrationBinSummary)
        For Each value In New Double() {-0.0, 0.0, 0.5, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}
            rows.Add(New CalibrationBinSummary With {.MeanPredicted = value, .ObservedRate = value})
        Next
        Dim expected = LegacySpecialty34.Calibration(rows)
        Dim actual As New CalibrationSeriesData(rows)
        EqualBits(expected(0), actual.XValues)
        EqualBits(expected(1), actual.YValues)
        CollectionAssert.AreEqual(New Integer() {0, 1, 2, 4, 5}, actual.SourceOrdinals)
        Assert.AreEqual(5, actual.Count)
        rows.Clear()
        Dim x = actual.XValues, y = actual.YValues, ordinals = actual.SourceOrdinals
        x(0) = 999 : y(0) = 999 : ordinals(0) = 999
        EqualBits(expected(0), actual.XValues)
        EqualBits(expected(1), actual.YValues)
        Assert.AreEqual(0, actual.SourceOrdinals(0))
        EqualBits(New Double() {0, 1}, actual.Reference.XValues)
        EqualBits(actual.Reference.XValues, actual.Reference.YValues)
        EqualBits(New Double() {0, 1, 0.2}, New Double() {actual.Axis.Min, actual.Axis.Max, actual.Axis.Scale})
        Assert.AreEqual(0, New CalibrationSeriesData(Nothing).Count)
        rows.Add(New CalibrationBinSummary With {.MeanPredicted = Double.NaN, .ObservedRate = 0.5})
        Assert.AreEqual(0, New CalibrationSeriesData(rows).Count)
    End Sub

    <DataTestMethod><DataRow(Integer.MinValue)><DataRow(-1)><DataRow(0)><DataRow(1)><DataRow(5)><DataRow(10)><DataRow(15)><DataRow(25)><DataRow(100)><DataRow(Integer.MaxValue)>
    Public Sub FrameDelayFrozenRounding(milliseconds As Integer)
        Assert.AreEqual(LegacySpecialty34.MsToCentiseconds(milliseconds), AnimationFrameTimingCore.MsToCentiseconds(milliseconds))
    End Sub

    <TestMethod>
    Public Sub ExplicitMeasuredMapsRetainDistinctClippingRules()
        For Each v In New Double() {-2, -0.0, 0, 0.5, 1, 3, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}
            EqualBits(New Double() {LegacySpecialty34.BulletX(v, 15, 200)}, New Double() {BulletMeasuredGeometryCore.MapX(v, 15, 200)})
            EqualBits(New Double() {LegacySpecialty34.SankeyX(v, 10, 400)}, New Double() {SankeyMeasuredGeometryCore.MapX(v, 10, 400)})
            EqualBits(New Double() {LegacySpecialty34.SankeyY(v, 20, 300)}, New Double() {SankeyMeasuredGeometryCore.MapY(v, 20, 300)})
        Next
        For Each count In New Integer() {-2, -1, 0, 1, 2, 5}
            Dim expected As Double() = Nothing, actual As Double() = Nothing
            Assert.AreEqual(ErrorIdentity(Sub() expected = LegacySpecialty34.StageCentres(count, 10, 400)), ErrorIdentity(Sub() actual = SankeyMeasuredGeometryCore.StageCentres(count, 10, 400)))
            If expected IsNot Nothing Then EqualBits(expected, actual)
        Next
    End Sub

    <TestMethod>
    Public Sub ReferenceEndpointBitsAndIndependentInvariants()
        For Each lowerEndpoint As Double In New Double() {-0.0, 0, 1, Double.NaN, Double.PositiveInfinity}
            Dim expected = LegacyReference34.FitLine(lowerEndpoint, 2, 3, -2)
            Dim actual = ChartReferenceSeries.Affine(lowerEndpoint, 2, 3, -2)
            EqualBits(expected(0), actual.XValues)
            EqualBits(expected(1), actual.YValues)
            Dim returned = actual.XValues : returned(0) = 999
            EqualBits(expected(0), actual.XValues)
        Next
        Dim identity = ChartReferenceSeries.Identity(-3, 7)
        EqualBits(identity.XValues, identity.YValues)
        EqualBits(New Double() {0, 0}, ChartReferenceSeries.Vertical(0, 0.5, 4.5).XValues)
        EqualBits(New Double() {-2, -2}, ChartReferenceSeries.Horizontal(1, 3, -2).YValues)
        Dim bounds = ComparisonCoordinatesCore.IncludeZeroBounds(2, 5)
        Assert.AreEqual(0.0R, bounds.Min)
        For count As Integer = 1 To 7
            Dim v = Enumerable.Range(1, count).Select(Function(i) CDbl(i)).ToArray()
            Dim expected = LegacyReference34.ComparisonBasis(v, v, v)
            EqualBits(expected(0), ComparisonCoordinatesCore.OrdinalY(count))
            bounds = ComparisonCoordinatesCore.IncludeZeroBounds(v.Min(), v.Max())
            EqualBits(expected(1), New Double() {bounds.Min, bounds.Max})
        Next
    End Sub

    <TestMethod>
    Public Sub SpcFrozenPaddingClampsAndAllMissing()
        For Each panel As SpcPanelType In [Enum].GetValues(GetType(SpcPanelType))
            For Each lo In New Double() {Double.PositiveInfinity, -1, 0, 0.95, 2, Double.NaN}
                For Each hi In New Double() {Double.NegativeInfinity, 0, 1, 2, Double.NaN}
                    Dim expected = LegacyReference34.SpcBounds(lo, hi, panel)
                    Dim actual = SpcAxisBoundsCore.FromExtents(lo, hi, panel)
                    EqualBits(New Double() {expected.Min, expected.Max}, New Double() {actual.Min, actual.Max})
                Next
            Next
        Next
        Dim missing = SpcAxisBoundsCore.FromExtents(Double.PositiveInfinity, Double.NegativeInfinity, SpcPanelType.IndividualValue)
        Assert.AreEqual(0.0R, missing.Min) : Assert.AreEqual(1.0R, missing.Max)
        Dim proportion = SpcAxisBoundsCore.FromExtents(0, 1, SpcPanelType.Proportion)
        Assert.AreEqual(0.0R, proportion.Min) : Assert.AreEqual(1.0R, proportion.Max)
    End Sub
End Class
