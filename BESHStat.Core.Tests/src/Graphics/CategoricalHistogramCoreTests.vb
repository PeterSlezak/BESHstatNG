Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Direct Core tests for grouped histogram binning and numerical DTOs.</summary>
<TestClass>
Public Class CategoricalHistogramCoreTests
    Private Shared Function Run(values As Double(), groups As Array,
                                Optional plot As CategoricalHistogramPlotType = CategoricalHistogramPlotType.BarsWithLegend,
                                Optional rule As CategoricalHistogramBinningRule = CategoricalHistogramBinningRule.Sturges) As CategoricalHistogramResult
        Return CategoricalHistogramCore.Compute(values, groups,
            New CategoricalHistogramOptions With {.PlotType = plot, .BinningRule = rule})
    End Function

    Private Shared Function Sample(Optional plot As CategoricalHistogramPlotType = CategoricalHistogramPlotType.BarsWithLegend) As CategoricalHistogramResult
        Return Run(New Double() {0, 1, 2, 3, 4, 5, 6, 7},
                   New Object() {"A", "B", "A", "B", "A", "B", "A", "B"}, plot)
    End Function

    Private Shared Sub Close(expected As Double, actual As Double, Optional tolerance As Double = 0.000000001R)
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

    Private Shared Function Sum(items As Double()) As Double
        Return items.Sum()
    End Function

    <TestMethod>
    Public Sub DefaultOptionsAndGroupOrder()
        Dim r = CategoricalHistogramCore.Compute(New Double() {0, 1, 2, 3}, New Object() {"B", "A", "B", "A"})
        Assert.AreEqual(CategoricalHistogramPlotType.BarsWithLegend, r.Options.PlotType)
        Assert.AreEqual(CategoricalHistogramBinningRule.Sturges, r.Options.BinningRule)
        Assert.AreEqual(2, r.GroupCount)
        Assert.AreEqual("B", r.Series(0).Name)
        Assert.AreEqual("A", r.Series(1).Name)
        Assert.AreEqual(4, r.SourceObservationCount)
        Assert.AreEqual(4, r.UsableObservationCount)
        Assert.AreEqual(0, r.ExcludedObservationCount)
    End Sub

    <TestMethod>
    Public Sub UsesExistingCoreBinsWithSameMidpoints()
        Dim r = Sample()
        Dim bins = HistogramBinningCore.HistogramBinsComputation(New Double() {0, 1, 2, 3, 4, 5, 6, 7}, "(Sturges)")
        Assert.AreEqual(bins.GetLength(0), r.BinCount)
        For i As Integer = 0 To r.BinCount - 1
            Close(Convert.ToDouble(bins(i, 0)), r.BinMidpoints(i))
        Next
    End Sub

    <TestMethod>
    Public Sub RawCountsPreserveAllObservationsAndRightmostEdge()
        Dim r = Sample(CategoricalHistogramPlotType.DifferentSampleSizes)
        Close(8.0R, r.Series.Sum(Function(s) Sum(s.Counts)))
        Close(4.0R, Sum(r.Series(0).Counts))
        Close(4.0R, Sum(r.Series(1).Counts))
        Close(r.Series(0).Counts(r.BinCount - 1), r.Series(0).PlotValues(r.BinCount - 1))
        Close(r.Series(1).Counts(r.BinCount - 1), r.Series(1).PlotValues(r.BinCount - 1))
    End Sub

    <TestMethod>
    Public Sub DensityForEachGroupIntegratesToOne()
        Dim r = Sample()
        For Each s As CategoricalHistogramSeries In r.Series
            Close(1.0R, Sum(s.PerGroupDensity) * r.BinWidth)
            CollectionAssert.AreEqual(s.PerGroupDensity, s.PlotValues)
        Next
    End Sub

    <TestMethod>
    Public Sub StackedPooledContributionsIntegrateToOne()
        Dim r = Sample(CategoricalHistogramPlotType.StackedBar)
        Close(1.0R, r.Series.Sum(Function(s) Sum(s.PooledDensityContribution)) * r.BinWidth)
        For Each s In r.Series
            CollectionAssert.AreEqual(s.PooledDensityContribution, s.PlotValues)
        Next
    End Sub

    <TestMethod>
    Public Sub UnequalGroupsUsePooledCountsForStackedAndPerGroupForDensity()
        Dim values As Double() = {0, 1, 2, 3, 4, 5, 6, 7}
        Dim groups As Object() = {"A", "A", "A", "B", "B", "B", "B", "B"}
        Dim r = Run(values, groups, CategoricalHistogramPlotType.StackedBar)
        Assert.AreEqual(3, r.Series(0).SampleSize)
        Assert.AreEqual(5, r.Series(1).SampleSize)
        Close(3.0R / 8.0R, Sum(r.Series(0).PooledDensityContribution) * r.BinWidth)
        Close(5.0R / 8.0R, Sum(r.Series(1).PooledDensityContribution) * r.BinWidth)
        Close(1.0R, Sum(r.Series(0).PerGroupDensity) * r.BinWidth)
        Close(1.0R, Sum(r.Series(1).PerGroupDensity) * r.BinWidth)
    End Sub

    <TestMethod>
    Public Sub ComparativePercentagesForEachGroupSumToOneHundred()
        Dim r = Sample(CategoricalHistogramPlotType.Comparative)
        For Each s In r.Series
            Close(100.0R, Sum(s.PerGroupPercent))
            CollectionAssert.AreEqual(s.PerGroupPercent, s.PlotValues)
            Assert.IsTrue(s.PlotValues.All(Function(x) x >= 0.0R))
        Next
    End Sub

    <TestMethod>
    Public Sub ComparativeOrientationChangesOnlyRenderingOptions()
        Dim opt As New CategoricalHistogramOptions With {.PlotType = CategoricalHistogramPlotType.Comparative}
        Dim values As Double() = {0, 1, 2, 3, 4, 5, 6, 7}
        Dim groups As Object() = {"A", "B", "A", "B", "A", "B", "A", "B"}
        Dim vertical = CategoricalHistogramCore.Compute(values, groups, opt)
        opt.ComparativeOrientation = CategoricalHistogramComparativeOrientation.Horizontal
        Dim horizontal = CategoricalHistogramCore.Compute(values, groups, opt)
        Assert.AreEqual(CategoricalHistogramComparativeOrientation.Vertical, vertical.Options.ComparativeOrientation)
        Assert.AreEqual(CategoricalHistogramComparativeOrientation.Horizontal, horizontal.Options.ComparativeOrientation)
        CollectionAssert.AreEqual(vertical.Series(0).PlotValues, horizontal.Series(0).PlotValues)
        CollectionAssert.AreEqual(vertical.Series(1).PlotValues, horizontal.Series(1).PlotValues)
    End Sub

    <TestMethod>
    Public Sub AllFourBinningRulesUseSharedCoreEngine()
        Dim values As Double() = {1, 2, 2, 3, 4, 4, 5, 7, 8, 9, 10, 13, 15, 21}
        Dim groups As Object() = {"A", "B", "A", "B", "A", "B", "A", "B", "A", "B", "A", "B", "A", "B"}
        For Each rule As CategoricalHistogramBinningRule In New CategoricalHistogramBinningRule() {
            CategoricalHistogramBinningRule.Sturges, CategoricalHistogramBinningRule.Doane,
            CategoricalHistogramBinningRule.Scott, CategoricalHistogramBinningRule.FreedmanDiaconis}
            Dim r = Run(values, groups, CategoricalHistogramPlotType.DifferentSampleSizes, rule)
            Assert.IsTrue(r.BinCount > 0)
            Assert.IsTrue(r.BinWidth > 0)
            Close(values.Length, r.Series.Sum(Function(s) Sum(s.Counts)))
        Next
    End Sub

    <TestMethod>
    Public Sub ResultBinExtentMatchesWidthAndMidpoints()
        Dim r = Sample()
        Assert.IsTrue(r.BinWidth > 0.0R)
        Close(r.BinMidpoints(0) - r.BinWidth / 2.0R, r.BinMinimum)
        Close(r.BinMidpoints(r.BinCount - 1) + r.BinWidth / 2.0R, r.BinMaximum)
    End Sub

    <TestMethod>
    Public Sub ConstantSampleStillProducesFinitePositiveBinWidth()
        Dim r = Run(New Double() {4, 4, 4, 4, 4, 4}, New Object() {"A", "A", "B", "A", "B", "B"})
        Assert.IsTrue(r.BinWidth > 0.0R)
        Assert.IsFalse(Double.IsInfinity(r.BinWidth))
        Close(6.0R, r.Series.Sum(Function(s) Sum(s.Counts)))
    End Sub

    <TestMethod>
    Public Sub MissingValuesAndGroupsCountOnlyPairedObservations()
        Dim r = Run(New Double() {1, Double.NaN, 3, 4, 5, 6},
                    New Object() {"A", "A", Nothing, "  ", DBNull.Value, "B"})
        Assert.AreEqual(6, r.SourceObservationCount)
        Assert.AreEqual(2, r.UsableObservationCount)
        Assert.AreEqual(4, r.ExcludedObservationCount)
        Assert.AreEqual(2, r.GroupCount)
    End Sub

    <TestMethod>
    Public Sub GroupsWithNonZeroLowerBoundAreAlignedCorrectly()
        Dim labels As Array = Array.CreateInstance(GetType(Object), New Integer() {4}, New Integer() {-2})
        labels.SetValue("First", -2)
        labels.SetValue("Second", -1)
        labels.SetValue("First", 0)
        labels.SetValue("Second", 1)
        Dim r = Run(New Double() {0, 1, 2, 3}, labels)
        Assert.AreEqual(2, r.GroupCount)
        Assert.AreEqual("First", r.Series(0).Name)
        Assert.AreEqual("Second", r.Series(1).Name)
        Assert.AreEqual(2, r.Series(0).SampleSize)
        Assert.AreEqual(2, r.Series(1).SampleSize)
    End Sub

    <TestMethod>
    Public Sub CLRAndTextNumericCategoriesShareOneGroup()
        Dim r = Run(New Double() {1, 2, 3, 4, 5}, New Object() {1, 1.0R, "1", 1D, "A"})
        Assert.AreEqual(2, r.GroupCount)
        Assert.AreEqual(4, r.Series(0).SampleSize)
        Assert.AreEqual("1", r.Series(0).Name)
        Assert.AreEqual("A", r.Series(1).Name)
    End Sub

    <TestMethod>
    Public Sub InvariantDecimalGroupTextAndNumericValueAreEquivalent()
        Dim previous As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US")
            Dim r = Run(New Double() {1, 2, 3}, New Object() {1.5R, "1.5", "other"})
            Assert.AreEqual(2, r.GroupCount)
            Assert.AreEqual(2, r.Series(0).SampleSize)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub TextAndBooleanRemainDistinctCategories()
        Dim r = Run(New Double() {1, 2, 3}, New Object() {"A", True, "A"})
        Assert.AreEqual(2, r.GroupCount)
        Assert.AreEqual(2, r.Series(0).SampleSize)
        Assert.AreEqual(1, r.Series(1).SampleSize)
    End Sub

    <TestMethod>
    Public Sub DefensiveCopiesForResultArraysAndOptions()
        Dim options As New CategoricalHistogramOptions With {.PlotType = CategoricalHistogramPlotType.DifferentSampleSizes}
        Dim r = CategoricalHistogramCore.Compute(New Double() {1, 2, 3, 4}, New Object() {"A", "B", "A", "B"}, options)
        options.PlotType = CategoricalHistogramPlotType.StackedBar
        Assert.AreEqual(CategoricalHistogramPlotType.DifferentSampleSizes, r.Options.PlotType)
        Dim returnedOptions = r.Options
        returnedOptions.PlotType = CategoricalHistogramPlotType.Comparative
        Assert.AreEqual(CategoricalHistogramPlotType.DifferentSampleSizes, r.Options.PlotType)
        Dim midd As Double() = r.BinMidpoints
        Dim first As Double = midd(0)
        midd(0) = -999.0R
        Close(first, r.BinMidpoints(0))
        Dim series = r.Series
        Dim counts = series(0).Counts
        Dim firstCount = counts(0)
        counts(0) = -999.0R
        Close(firstCount, r.Series(0).Counts(0))
        series(0) = Nothing
        Assert.IsNotNull(r.Series(0))
    End Sub

    <TestMethod>
    Public Sub NullContinuousValuesRejected()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() CategoricalHistogramCore.Compute(Nothing, New Object() {"A"}))
    End Sub

    <TestMethod>
    Public Sub NullCategoriesRejected()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() CategoricalHistogramCore.Compute(New Double() {1}, Nothing))
    End Sub

    <TestMethod>
    Public Sub MismatchedRowsRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() Run(New Double() {1, 2}, New Object() {"A"}))
    End Sub

    <TestMethod>
    Public Sub MultidimensionalGroupArrayRejected()
        Dim categories As Array = Array.CreateInstance(GetType(Object), 1, 2)
        Assert.ThrowsException(Of ArgumentException)(Sub() Run(New Double() {1, 2}, categories))
    End Sub

    <TestMethod>
    Public Sub EmptyContinuousArrayRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() Run(Array.Empty(Of Double)(), Array.Empty(Of Object)()))
    End Sub

    <TestMethod>
    Public Sub FullyMissingPairsRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() Run(New Double() {Double.NaN, 2}, New Object() {"A", Nothing}))
    End Sub

    <TestMethod>
    Public Sub InfiniteContinuousValueRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Run(New Double() {1, Double.PositiveInfinity}, New Object() {"A", "A"}))
    End Sub

    <TestMethod>
    Public Sub ComparativeRequiresExactlyTwoUsableGroups()
        Assert.ThrowsException(Of ArgumentException)(Sub() Run(New Double() {1, 2}, New Object() {"A", "A"}, CategoricalHistogramPlotType.Comparative))
        Assert.ThrowsException(Of ArgumentException)(Sub() Run(New Double() {1, 2, 3}, New Object() {"A", "B", "C"}, CategoricalHistogramPlotType.Comparative))
    End Sub

    <TestMethod>
    Public Sub InvalidPlotTypeRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Run(New Double() {1, 2}, New Object() {"A", "B"}, CType(999, CategoricalHistogramPlotType)))
    End Sub

    <TestMethod>
    Public Sub InvalidBinningRuleRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Run(New Double() {1, 2}, New Object() {"A", "B"}, CategoricalHistogramPlotType.BarsWithLegend, CType(999, CategoricalHistogramBinningRule)))
    End Sub

    <TestMethod>
    Public Sub InvalidComparativeOrientationRejected()
        Dim options As New CategoricalHistogramOptions With {.ComparativeOrientation = CType(999, CategoricalHistogramComparativeOrientation)}
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() CategoricalHistogramCore.Compute(New Double() {1, 2}, New Object() {"A", "B"}, options))
    End Sub
End Class
