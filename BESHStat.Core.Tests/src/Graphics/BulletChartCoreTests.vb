Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Direct portable bullet-chart engine tests; no Excel or Windows host dependency.</summary>
<TestClass>
Public Class BulletChartCoreTests

    Private Shared Function RangesTwoRows() As Double(,)
        Return New Double(,) {{100.0R, 200.0R, 300.0R},
                             {40.0R, 60.0R, 80.0R}}
    End Function

    Private Shared Function SimpleChart(Optional options As BulletChartOptions = Nothing) As BulletChartResult
        Return BulletChartCore.Compute(New String() {"Revenue", "Quality"},
                                       New Double() {230.0R, 70.0R},
                                       New Double() {250.0R, 80.0R},
                                       RangesTwoRows(), options:=options)
    End Function

    Private Shared Sub AssertClose(expected As Double, actual As Double,
                                   Optional tolerance As Double = 0.0000000001R)
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub Compute_PreparesIndependentScalesAndSourceOrder()
        Dim result As BulletChartResult = SimpleChart()
        Assert.AreEqual(2, result.ItemCount)
        Assert.AreEqual(2, result.SourceItemCount)
        Assert.AreEqual(0, result.OmittedItemCount)
        Assert.AreEqual(3, result.MaximumQualitativeRangeCount)
        Dim first As BulletChartItem = result.Items(0)
        Dim second As BulletChartItem = result.Items(1)
        Assert.AreEqual("Revenue", first.MeasureLabel)
        Assert.AreEqual(0, first.SourceIndex)
        Assert.AreEqual(1, second.SourceIndex)
        Assert.AreEqual(1, second.DisplayIndex)
        AssertClose(300.0R, first.ScaleMaximum)
        AssertClose(80.0R, second.ScaleMaximum)
        AssertClose(50.0R, first.MajorInterval)
        AssertClose(10.0R, second.MajorInterval)
        Assert.IsTrue(first.ScaleMaximumWasAutomatic)
        Assert.IsTrue(first.MajorIntervalWasAutomatic)
        AssertClose(230.0R / 300.0R, first.ActualNormalized)
        AssertClose(250.0R / 300.0R, first.TargetNormalized)
        CollectionAssert.AreEqual(New Double() {0.0R, 50.0R, 100.0R, 150.0R, 200.0R, 250.0R, 300.0R}, first.ScaleTicks)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub Compute_BandsUseCumulativeBoundariesAndRanks()
        Dim item As BulletChartItem = SimpleChart().Items(0)
        Dim bands As BulletChartBand() = item.Bands
        Assert.AreEqual(3, bands.Length)
        AssertClose(0.0R, bands(0).LowerBound)
        AssertClose(100.0R, bands(0).UpperBound)
        AssertClose(100.0R, bands(1).LowerBound)
        AssertClose(200.0R, bands(1).UpperBound)
        AssertClose(200.0R, bands(2).LowerBound)
        AssertClose(300.0R, bands(2).UpperBound)
        Assert.AreEqual(0, bands(0).BandIndex)
        Assert.AreEqual(0, bands(0).DesirabilityRank)
        Assert.AreEqual(2, bands(2).DesirabilityRank)
        AssertClose(1.0R / 3.0R, bands(0).NormalizedEnd)
        AssertClose(1.0R, bands(2).NormalizedEnd)
        Assert.IsFalse(bands(2).IsExtendedFinalBand)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub LowerIsBetter_ReversesDesirabilityNotPosition()
        Dim result As BulletChartResult = BulletChartCore.ComputeBasic(
            New String() {"Latency"}, New Double() {12.0R}, New Double() {10.0R},
            New Double(,) {{10.0R, 20.0R, 30.0R}},
            New BulletChartOptions With {.DefaultDirection = BulletChartDirection.LowerIsBetter})
        Dim item As BulletChartItem = result.Items(0)
        Assert.IsFalse(item.HigherIsBetter)
        Assert.AreEqual(BulletChartDirection.LowerIsBetter, item.Direction)
        Assert.AreEqual(2, item.Bands(0).DesirabilityRank)
        Assert.AreEqual(1, item.Bands(1).DesirabilityRank)
        Assert.AreEqual(0, item.Bands(2).DesirabilityRank)
        AssertClose(0.0R, item.Bands(0).NormalizedStart)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub ExplicitScaleAndInterval_ExtendFinalBandAndPreserveTicks()
        Dim result As BulletChartResult = BulletChartCore.Compute(
            New String() {"A"}, New Double() {40.0R}, New Double() {55.0R},
            New Double(,) {{10.0R, 20.0R, 30.0R}},
            scaleMaximums:=New Double() {60.0R},
            majorIntervals:=New Double() {15.0R})
        Dim item As BulletChartItem = result.Items(0)
        AssertClose(60.0R, item.ScaleMaximum)
        AssertClose(15.0R, item.MajorInterval)
        CollectionAssert.AreEqual(New Double() {0.0R, 15.0R, 30.0R, 45.0R, 60.0R}, item.ScaleTicks)
        Assert.IsFalse(item.ScaleMaximumWasAutomatic)
        Assert.IsFalse(item.MajorIntervalWasAutomatic)
        Assert.IsTrue(item.ActualExceedsQualitativeMaximum)
        Assert.IsTrue(item.TargetExceedsQualitativeMaximum)
        Assert.IsTrue(item.ScaleExtendedBeyondQualitativeMaximum)
        Assert.IsTrue(result.HasExtendedScales)
        Assert.IsTrue(item.Bands(2).IsExtendedFinalBand)
        AssertClose(60.0R, item.Bands(2).UpperBound)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub MissingRowsCanBeOmitted_AndDisplayIndicesAreCompacted()
        Dim options As New BulletChartOptions With {.OmitIncompleteRows = True}
        Dim ranges As Object(,) = {{10.0R, 20.0R, Nothing},
                                   {10.0R, 20.0R, 30.0R},
                                   {10.0R, 20.0R, 30.0R}}
        Dim result As BulletChartResult = BulletChartCore.Compute(
            New String() {"A", " ", "C"}, New Object() {11.0R, 15.0R, 21.0R},
            New Object() {15.0R, 18.0R, Double.NaN}, ranges, options:=options)
        Assert.AreEqual(3, result.SourceItemCount)
        Assert.AreEqual(1, result.ItemCount)
        Assert.AreEqual(2, result.OmittedItemCount)
        Assert.AreEqual(0, result.Items(0).SourceIndex)
        Assert.AreEqual(0, result.Items(0).DisplayIndex)
        Assert.AreEqual(2, result.Items(0).QualitativeRangeCount)
        Assert.AreEqual(2, result.MaximumQualitativeRangeCount)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub MissingRequiredRow_ThrowsWhenOmissionDisabled()
        Assert.ThrowsException(Of ArgumentException)(Sub() BulletChartCore.ComputeBasic(
            New String() {" "}, New Double() {1.0R}, New Double() {2.0R},
            New Double(,) {{10.0R, 20.0R}}))
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub OptionsAndPayloadsAreDefensiveCopies()
        Dim options As New BulletChartOptions With {.TargetMajorTickCount = 5}
        Dim result As BulletChartResult = SimpleChart(options)
        options.TargetMajorTickCount = 19
        Assert.AreEqual(5, result.Options.TargetMajorTickCount)
        Dim clonedOptions As BulletChartOptions = result.Options
        clonedOptions.TargetMajorTickCount = 7
        Assert.AreEqual(5, result.Options.TargetMajorTickCount)
        Dim items As BulletChartItem() = result.Items
        items(0) = Nothing
        Assert.IsNotNull(result.Items(0))
        Dim boundaries As Double() = result.Items(0).QualitativeRangeBoundaries
        boundaries(0) = -3.0R
        AssertClose(100.0R, result.Items(0).QualitativeRangeBoundaries(0))
        Dim ticks As Double() = result.Items(0).ScaleTicks
        ticks(0) = -10.0R
        AssertClose(0.0R, result.Items(0).ScaleTicks(0))
        Dim bands As BulletChartBand() = result.Items(0).Bands
        bands(0) = Nothing
        Assert.IsNotNull(result.Items(0).Bands(0))
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub ExcelShapeAndNonZeroLowerBoundArrays_AreAccepted()
        Dim labels As Array = Array.CreateInstance(GetType(Object), New Integer() {2, 1}, New Integer() {1, 1})
        labels.SetValue("One", 1, 1)
        labels.SetValue("Two", 2, 1)
        Dim ranges As Array = Array.CreateInstance(GetType(Object), New Integer() {2, 2}, New Integer() {1, 1})
        ranges.SetValue(10.0R, 1, 1)
        ranges.SetValue(20.0R, 1, 2)
        ranges.SetValue(20.0R, 2, 1)
        ranges.SetValue(40.0R, 2, 2)
        Dim result As BulletChartResult = BulletChartCore.Compute(
            labels, New Double() {8.0R, 14.0R}, New Double() {9.0R, 25.0R}, ranges)
        Assert.AreEqual(2, result.ItemCount)
        Assert.AreEqual("Two", result.Items(1).MeasureLabel)
        AssertClose(40.0R, result.Items(1).QualitativeMaximum)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub SingleRowTwoDimensionalVectors_AreAccepted()
        Dim result As BulletChartResult = BulletChartCore.Compute(
            New Object(,) {{"X", "Y"}}, New Object(,) {{5.0R, 10.0R}},
            New Object(,) {{6.0R, 12.0R}},
            New Double(,) {{10.0R, 20.0R}, {20.0R, 40.0R}})
        Assert.AreEqual(2, result.ItemCount)
        AssertClose(10.0R, result.Items(1).ActualValue)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub DirectionVector_AcceptsBooleanIntegerAndText()
        Dim result As BulletChartResult = BulletChartCore.Compute(
            New String() {"A", "B", "C", "D"},
            New Double() {1.0R, 2.0R, 3.0R, 4.0R},
            New Double() {2.0R, 3.0R, 4.0R, 5.0R},
            New Double(,) {{10.0R, 20.0R}, {10.0R, 20.0R}, {10.0R, 20.0R}, {10.0R, 20.0R}},
            directions:=New Object() {False, 0, "Lower is better", "1"})
        Assert.AreEqual(BulletChartDirection.LowerIsBetter, result.Items(0).Direction)
        Assert.AreEqual(BulletChartDirection.HigherIsBetter, result.Items(1).Direction)
        Assert.AreEqual(BulletChartDirection.LowerIsBetter, result.Items(2).Direction)
        Assert.AreEqual(BulletChartDirection.LowerIsBetter, result.Items(3).Direction)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub NumericStringInputs_AreAccepted()
        Dim result As BulletChartResult = BulletChartCore.ComputeBasic(
            New String() {"A"}, New String() {"12.5"}, New String() {"15"},
            New Object(,) {{"10", "20", "30"}})
        AssertClose(12.5R, result.Items(0).ActualValue)
        AssertClose(15.0R, result.Items(0).TargetValue)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub SubtitlesAndEmptyOptionalCells_AreHandled()
        Dim result As BulletChartResult = BulletChartCore.Compute(
            New String() {"A"}, New Double() {12.0R}, New Double() {13.0R},
            New Double(,) {{10.0R, 20.0R}},
            subtitles:=New String() {" USD "},
            scaleMaximums:=New Object() {DBNull.Value},
            majorIntervals:=New Object() {""})
        Assert.IsTrue(result.HasSubtitles)
        Assert.AreEqual("USD", result.Items(0).Subtitle)
        Assert.IsTrue(result.Items(0).ScaleMaximumWasAutomatic)
        Assert.IsTrue(result.Items(0).MajorIntervalWasAutomatic)
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub InvalidDimensionsAndBoundaries_AreRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() BulletChartCore.ComputeBasic(
            New String() {"A", "B"}, New Double() {1.0R}, New Double() {2.0R, 3.0R},
            New Double(,) {{10.0R, 20.0R}, {10.0R, 20.0R}}))
        Assert.ThrowsException(Of ArgumentException)(Sub() BulletChartCore.ComputeBasic(
            New String() {"A"}, New Double() {1.0R}, New Double() {2.0R},
            New Double(,) {{20.0R, 10.0R}}))
        Assert.ThrowsException(Of ArgumentException)(Sub() BulletChartCore.ComputeBasic(
            New String() {"A"}, New Double() {1.0R}, New Double() {2.0R},
            New Object(,) {{10.0R, Nothing, 20.0R}}))
        Assert.ThrowsException(Of ArgumentException)(Sub() BulletChartCore.ComputeBasic(
            New String() {"A"}, New Double() {1.0R}, New Double() {2.0R},
            New Double() {10.0R, 20.0R}))
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub BooleanAndNonNumericActualValues_AreRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() BulletChartCore.ComputeBasic(
            New String() {"A"}, New Object() {True}, New Double() {2.0R},
            New Double(,) {{10.0R, 20.0R}}))
        Assert.ThrowsException(Of ArgumentException)(Sub() BulletChartCore.ComputeBasic(
            New String() {"A"}, New Object() {"abc"}, New Double() {2.0R},
            New Double(,) {{10.0R, 20.0R}}))
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub NegativeAndInfiniteValues_AreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() BulletChartCore.ComputeBasic(
            New String() {"A"}, New Double() {-1.0R}, New Double() {2.0R},
            New Double(,) {{10.0R, 20.0R}}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() BulletChartCore.ComputeBasic(
            New String() {"A"}, New Double() {Double.PositiveInfinity}, New Double() {2.0R},
            New Double(,) {{10.0R, 20.0R}}))
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub OptionsAndTickCountValidation_ArePreserved()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() SimpleChart(
            New BulletChartOptions With {.TargetMajorTickCount = 2}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() SimpleChart(
            New BulletChartOptions With {.DefaultDirection = CType(12, BulletChartDirection)}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() BulletChartCore.Compute(
            New String() {"A"}, New Double() {12.0R}, New Double() {13.0R},
            New Double(,) {{10.0R, 20.0R}},
            scaleMaximums:=New Double() {100.0R},
            majorIntervals:=New Double() {0.1R}))
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub AllIncompleteRows_AreRejectedEvenWhenOmitted()
        Assert.ThrowsException(Of ArgumentException)(Sub() BulletChartCore.ComputeBasic(
            New String() {" "}, New Double() {1.0R}, New Double() {2.0R},
            New Double(,) {{10.0R, 20.0R}},
            New BulletChartOptions With {.OmitIncompleteRows = True}))
    End Sub

    <TestMethod>
    <TestCategory("Bullet-Core")>
    Public Sub NullInputs_KeepArgumentNullException()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() BulletChartCore.ComputeBasic(
            Nothing, New Double() {1.0R}, New Double() {2.0R},
            New Double(,) {{10.0R, 20.0R}}))
    End Sub

End Class
