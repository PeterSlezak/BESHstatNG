Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Direct tests of the portable kite-chart geometry and result contracts.</summary>
<TestClass>
Public Class KiteChartCoreTests
    Private Shared Sub Close(expected As Double, actual As Double,
                             Optional tolerance As Double = 0.0000000001R)
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

    Private Shared Function Sample() As Double(,)
        Return New Double(,) {{2.0R, 4.0R}, {4.0R, 2.0R}}
    End Function

    <TestMethod>
    Public Sub DefaultOptions_ProduceSymmetricKitesInSourceOrder()
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample())
        Assert.AreEqual(2, r.Series.Length)
        Assert.AreEqual(2, r.SourceCategoryCount)
        Assert.AreEqual(4, r.PlottedCategoryCount)
        Close(0.4R, r.MaximumHalfWidth)
        Close(2.0R, r.Series(0).CenterLine)
        Close(1.0R, r.Series(1).CenterLine)
        Close(0.2R, r.Series(0).HalfWidths(1))
        Close(0.4R, r.Series(0).HalfWidths(2))
        Close(0.4R, r.Series(1).HalfWidths(1))
        Close(2.2R, r.Series(0).UpperBoundary(1))
        Close(1.8R, r.Series(0).LowerBoundary(1))
        Close(0.0R, r.AxisMinimum)
        Close(3.0R, r.AxisMaximum)
    End Sub

    <TestMethod>
    Public Sub ZeroEndpoints_CloseBothEndsOfEachKite()
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample())
        For Each s As KiteChartSeries In r.Series
            Close(0.0R, s.HalfWidths(0))
            Close(0.0R, s.HalfWidths(s.HalfWidths.Length - 1))
            Close(s.CenterLine, s.UpperBoundary(0))
            Close(s.CenterLine, s.LowerBoundary(s.LowerBoundary.Length - 1))
        Next
        Assert.AreEqual(String.Empty, r.Categories(0))
        Assert.AreEqual(String.Empty, r.Categories(3))
    End Sub

    <TestMethod>
    Public Sub AddZeroEndpointsFalse_KeepsOriginalCategoryCount()
        Dim opt As New KiteChartOptions With {.AddZeroEndpoints = False}
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample(), options:=opt)
        Assert.AreEqual(2, r.PlottedCategoryCount)
        Close(0.2R, r.Series(0).HalfWidths(0))
        Close(0.4R, r.Series(0).HalfWidths(1))
    End Sub

    <TestMethod>
    Public Sub CommonMaximum_ScalesAllColumnsAgainstGlobalMaximum()
        Dim r As KiteChartResult = KiteChartCore.Compute(New Double(,) {{1, 2}, {4, 8}},
                   options:=New KiteChartOptions With {.AddZeroEndpoints = False})
        Close(0.2R, r.Series(0).HalfWidths(1))
        Close(0.4R, r.Series(1).HalfWidths(1))
        Close(8.0R, r.GlobalSourceMaximum)
    End Sub

    <TestMethod>
    Public Sub PerSeriesMaximum_ScalesEachColumnIndependently()
        Dim opt As New KiteChartOptions With {.ScaleMode = KiteScaleMode.PerSeriesMaximum,
                                             .AddZeroEndpoints = False}
        Dim r As KiteChartResult = KiteChartCore.Compute(New Double(,) {{1, 2}, {4, 8}}, options:=opt)
        Close(0.4R, r.Series(0).HalfWidths(1))
        Close(0.4R, r.Series(1).HalfWidths(1))
        Close(0.1R, r.Series(0).HalfWidths(0))
    End Sub

    <TestMethod>
    Public Sub SquareRootTransform_IsAppliedBeforeNormalization()
        Dim opt As New KiteChartOptions With {.ValueTransform = KiteValueTransform.SquareRoot,
                                             .AddZeroEndpoints = False}
        Dim r As KiteChartResult = KiteChartCore.Compute(New Double(,) {{1}, {9}}, options:=opt)
        Close(3, r.Series(0).TransformedValues(1))
        Close(0.4R / 3.0R, r.Series(0).HalfWidths(0))
        Close(9, r.Series(0).SourceMaximum)
        Close(3, r.GlobalTransformedMaximum)
    End Sub

    <TestMethod>
    Public Sub LogOnePlusTransform_UsesNaturalLogarithm()
        Dim opt As New KiteChartOptions With {.ValueTransform = KiteValueTransform.LogOnePlus}
        Dim r As KiteChartResult = KiteChartCore.Compute(New Double(,) {{0}, {9}}, options:=opt)
        Close(Math.Log(10.0R), r.Series(0).TransformedValues(1))
        Close(0, r.Series(0).TransformedValues(0))
        Close(0.4R, r.Series(0).HalfWidths(2))
    End Sub

    <TestMethod>
    Public Sub MissingGap_PreservesNaNsAndPerSeriesCounts()
        Dim r As KiteChartResult = KiteChartCore.Compute(New Double(,) {{1, Double.NaN}, {Double.NaN, 2}})
        Assert.AreEqual(2, r.MissingObservationCount)
        Assert.AreEqual(2, r.ValidObservationCount)
        Assert.AreEqual(1, r.Series(0).MissingObservationCount)
        Assert.IsTrue(Double.IsNaN(r.Series(0).HalfWidths(2)))
        Assert.IsTrue(Double.IsNaN(r.Series(1).UpperBoundary(1)))
    End Sub

    <TestMethod>
    Public Sub MissingZero_PreservesMissingSourceButDrawsZeroWidth()
        Dim opt As New KiteChartOptions With {.MissingValueMode = KiteMissingValueMode.Zero}
        Dim r As KiteChartResult = KiteChartCore.Compute(New Double(,) {{1}, {Double.NaN}}, options:=opt)
        Assert.IsTrue(Double.IsNaN(r.Series(0).SourceValues(1)))
        Close(0, r.Series(0).HalfWidths(2))
        Close(r.Series(0).CenterLine, r.Series(0).UpperBoundary(2))
        Assert.AreEqual(1, r.MissingObservationCount)
    End Sub

    <TestMethod>
    Public Sub LaneSpacingAndWidthFraction_ControlGeometry()
        Dim opt As New KiteChartOptions With {.LaneSpacing = 3, .MaximumWidthFraction = 0.5R}
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample(), options:=opt)
        Close(0.75R, r.MaximumHalfWidth)
        Close(6, r.Series(0).CenterLine)
        Close(3, r.Series(1).CenterLine)
        Close(9, r.AxisMaximum)
    End Sub

    <TestMethod>
    Public Sub SeriesNames_AreTrimmedAndMissingOnesGenerated()
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample(), New String() {" First ", " "})
        Assert.AreEqual("First", r.Series(0).Name)
        Assert.AreEqual("Series 2", r.Series(1).Name)
        Assert.AreEqual(0, r.Series(0).SourceIndex)
        Assert.AreEqual(1, r.Series(1).SourceIndex)
    End Sub

    <TestMethod>
    Public Sub DefaultCategories_StartAtOne()
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample(),
            options:=New KiteChartOptions With {.AddZeroEndpoints = False})
        Assert.AreEqual(1, r.Categories(0))
        Assert.AreEqual(2, r.Categories(1))
    End Sub

    <TestMethod>
    Public Sub ExplicitCategories_PreserveMixedTypesAndOrder()
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample(), categoryLabels:=New Object() {"A", 5})
        Assert.AreEqual("A", r.Categories(1))
        Assert.AreEqual(5, r.Categories(2))
    End Sub

    <TestMethod>
    Public Sub CategoryLabels_WithNonzeroLowerBounds_AreReadPositionally()
        Dim labels As Array = Array.CreateInstance(GetType(Object), New Integer() {2}, New Integer() {1})
        labels.SetValue("left", 1)
        labels.SetValue("right", 2)
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample(), categoryLabels:=labels)
        Assert.AreEqual("left", r.Categories(1))
        Assert.AreEqual("right", r.Categories(2))
    End Sub

    <TestMethod>
    Public Sub NullCategoryLabels_FallBackToRowIndices()
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample(),
            categoryLabels:=New Object() {Nothing, DBNull.Value})
        Assert.AreEqual(1, r.Categories(1))
        Assert.AreEqual(2, r.Categories(2))
    End Sub

    <TestMethod>
    Public Sub Results_DefensivelyCopyCategoriesAndSourceArrays()
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample())
        Dim c As Object() = r.Categories
        c(1) = "changed"
        Assert.AreEqual(1, r.Categories(1))
        Dim w As Double() = r.Series(0).HalfWidths
        w(1) = 99.0R
        Close(0.2R, r.Series(0).HalfWidths(1))
        Dim s As KiteChartSeries() = r.Series
        s(0) = Nothing
        Assert.IsNotNull(r.Series(0))
    End Sub

    <TestMethod>
    Public Sub Options_AreCopiedAtComputeTimeAndOnRead()
        Dim opt As New KiteChartOptions With {.LaneSpacing = 2}
        Dim r As KiteChartResult = KiteChartCore.Compute(Sample(), options:=opt)
        opt.LaneSpacing = 50
        Close(2, r.Options.LaneSpacing)
        Dim returned As KiteChartOptions = r.Options
        returned.LaneSpacing = 100
        Close(2, r.Options.LaneSpacing)
    End Sub

    <TestMethod>
    Public Sub AllMissingObservations_AreRejected()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() KiteChartCore.Compute(New Double(,) {{Double.NaN}, {Double.NaN}}))
    End Sub

    <TestMethod>
    Public Sub AllZeroObservations_AreRejected()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() KiteChartCore.Compute(New Double(,) {{0.0R}, {0.0R}}))
    End Sub

    <TestMethod>
    Public Sub NegativeAndInfiniteValues_AreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() KiteChartCore.Compute(New Double(,) {{1}, {-1}}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() KiteChartCore.Compute(New Double(,) {{1}, {Double.PositiveInfinity}}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() KiteChartCore.Compute(New Double(,) {{1}, {Double.NegativeInfinity}}))
    End Sub

    <TestMethod>
    Public Sub MatrixMustHaveAtLeastTwoRows()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() KiteChartCore.Compute(New Double(,) {{1.0R, 2.0R}}))
    End Sub

    <TestMethod>
    Public Sub NothingMatrix_IsRejected()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() KiteChartCore.Compute(Nothing))
    End Sub

    <TestMethod>
    Public Sub InvalidNamesAndLabels_AreRejected()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() KiteChartCore.Compute(Sample(), seriesNames:=New String() {"only one"}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() KiteChartCore.Compute(Sample(), categoryLabels:=New String() {"A"}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() KiteChartCore.Compute(Sample(), categoryLabels:=New String(,) {{"A", "B"}}))
    End Sub

    <TestMethod>
    Public Sub UndefinedEnumValues_AreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() KiteChartCore.Compute(Sample(), options:=New KiteChartOptions With {
                .ScaleMode = CType(999, KiteScaleMode)}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() KiteChartCore.Compute(Sample(), options:=New KiteChartOptions With {
                .ValueTransform = CType(999, KiteValueTransform)}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() KiteChartCore.Compute(Sample(), options:=New KiteChartOptions With {
                .MissingValueMode = CType(999, KiteMissingValueMode)}))
    End Sub

    <TestMethod>
    Public Sub InvalidLaneSpacing_IsRejected()
        For Each v As Double In New Double() {0, -1, Double.NaN, Double.PositiveInfinity}
            Dim invalid As Double = v
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub() KiteChartCore.Compute(Sample(), options:=New KiteChartOptions With {.LaneSpacing = invalid}))
        Next
    End Sub

    <TestMethod>
    Public Sub InvalidWidthFraction_IsRejected()
        For Each v As Double In New Double() {0, -1, 1.01R, Double.NaN, Double.PositiveInfinity}
            Dim invalid As Double = v
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(
                Sub() KiteChartCore.Compute(Sample(), options:=New KiteChartOptions With {.MaximumWidthFraction = invalid}))
        Next
    End Sub

    <TestMethod>
    Public Sub CountsAndExtrema_ReflectSourceMatrix()
        Dim r As KiteChartResult = KiteChartCore.Compute(New Double(,) {{2, 1}, {3, 5}, {Double.NaN, 0}})
        Assert.AreEqual(6, r.SourceObservationCount)
        Assert.AreEqual(5, r.ValidObservationCount)
        Assert.AreEqual(1, r.MissingObservationCount)
        Assert.AreEqual(2, r.Series(0).ValidObservationCount)
        Close(3, r.Series(0).SourceMaximum)
        Close(5, r.GlobalSourceMaximum)
    End Sub
End Class
