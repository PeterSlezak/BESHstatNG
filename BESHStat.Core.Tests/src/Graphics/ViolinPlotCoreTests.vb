Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Direct, Excel-independent violin KDE and DTO regression checks.</summary>
<TestClass>
Public Class ViolinPlotCoreTests
    Private Shared Function Compute(values As Double(), groups As Object(),
                                    Optional options As ViolinPlotOptions = Nothing) As ViolinPlotResult
        Return ViolinPlotCore.Compute(values, groups, options)
    End Function

    Private Shared Function Sample(Optional options As ViolinPlotOptions = Nothing) As ViolinPlotResult
        Return Compute(New Double() {1, 3, 2, 4, 6, 8},
                       New Object() {"A", "A", "B", "B", "B", "B"}, options)
    End Function

    Private Shared Sub Close(expected As Double, actual As Double,
                             Optional tolerance As Double = 0.000000001R)
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

    <TestMethod>
    Public Sub Basic_GroupOrderingAndObservationCounts()
        Dim result As ViolinPlotResult = Sample()
        Assert.AreEqual(2, result.GroupCount)
        Assert.AreEqual("A", result.Series(0).Name)
        Assert.AreEqual("B", result.Series(1).Name)
        Assert.AreEqual(2, result.Series(0).SampleSize)
        Assert.AreEqual(4, result.Series(1).SampleSize)
        Assert.AreEqual(6, result.SourceObservationCount)
        Assert.AreEqual(6, result.UsableObservationCount)
        Assert.AreEqual(0, result.ExcludedObservationCount)
    End Sub

    <TestMethod>
    Public Sub Basic_DensityGridHasRequestedLengthAndPositiveValues()
        Dim result As ViolinPlotResult = Sample(New ViolinPlotOptions With {.GridPoints = 64})
        For Each series As ViolinPlotSeries In result.Series
            Assert.AreEqual(64, series.EvaluationPoints.Length)
            Assert.AreEqual(64, series.Density.Length)
            Assert.AreEqual(64, series.ScaledHalfWidths.Length)
            Assert.IsTrue(series.Density.All(Function(x) x >= 0 AndAlso Not Double.IsNaN(x)))
            Assert.IsTrue(series.Bandwidth > 0)
        Next
    End Sub

    <TestMethod>
    Public Sub Basic_YExtentEnclosesEveryGrid()
        Dim result As ViolinPlotResult = Sample()
        Close(result.Series.Min(Function(s) s.EvaluationPoints.Min()), result.YMinimum)
        Close(result.Series.Max(Function(s) s.EvaluationPoints.Max()), result.YMaximum)
    End Sub

    <TestMethod>
    Public Sub Missing_NaNAndBlankAndDBNullAndNothingRowsAreExcluded()
        Dim values As Double() = {1, Double.NaN, 3, 4, 5, 6}
        Dim groups As Object() = {"A", "A", Nothing, "   ", DBNull.Value, "B"}
        Dim result As ViolinPlotResult = Compute(values, groups)
        Assert.AreEqual(6, result.SourceObservationCount)
        Assert.AreEqual(2, result.UsableObservationCount)
        Assert.AreEqual(4, result.ExcludedObservationCount)
        Assert.AreEqual(2, result.GroupCount)
    End Sub

    <TestMethod>
    Public Sub Missing_AllRowsFiltered_Throws()
        Assert.ThrowsException(Of ArgumentException)(Sub() Compute(New Double() {Double.NaN},
                                                                     New Object() {"A"}))
    End Sub

    <TestMethod>
    Public Sub Missing_InfinityThrowsInsteadOfBeingOmitted()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Compute(New Double() {1, Double.PositiveInfinity},
                                                                              New Object() {"A", "A"}))
    End Sub

    <TestMethod>
    Public Sub Input_NullValuesAreRejected()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ViolinPlotCore.Compute(Nothing, New Object() {"A"}))
    End Sub

    <TestMethod>
    Public Sub Input_NullGroupsAreRejected()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ViolinPlotCore.Compute(New Double() {1}, Nothing))
    End Sub

    <TestMethod>
    Public Sub Input_EmptyDataAreRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() Compute(New Double() {}, New Object() {}))
    End Sub

    <TestMethod>
    Public Sub Input_LengthMismatchIsRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() Compute(New Double() {1, 2}, New Object() {"A"}))
    End Sub

    <TestMethod>
    Public Sub Input_GroupArrayMustBeOneDimensional()
        Dim wrongRank As Array = New Object(1, 1) {}
        Assert.ThrowsException(Of ArgumentException)(Sub() ViolinPlotCore.Compute(New Double() {1, 2, 3, 4}, wrongRank))
    End Sub

    <TestMethod>
    Public Sub Input_NonZeroGroupLowerBoundIsSupported()
        Dim groups As Array = Array.CreateInstance(GetType(Object), New Integer() {4}, New Integer() {-3})
        groups.SetValue("B", -3)
        groups.SetValue("A", -2)
        groups.SetValue("B", -1)
        groups.SetValue("A", 0)
        Dim result As ViolinPlotResult = ViolinPlotCore.Compute(New Double() {1, 2, 3, 4}, groups)
        Assert.AreEqual("B", result.Series(0).Name)
        Assert.AreEqual("A", result.Series(1).Name)
        Assert.AreEqual(2, result.Series(0).SampleSize)
    End Sub

    <TestMethod>
    Public Sub Groups_FirstAppearanceOrderAndTextTrimming()
        Dim result As ViolinPlotResult = Compute(New Double() {4, 1, 2, 3},
                                                  New Object() {" B ", "A", "B", "A"})
        Assert.AreEqual(2, result.GroupCount)
        Assert.AreEqual("B", result.Series(0).Name)
        Assert.AreEqual("A", result.Series(1).Name)
    End Sub

    <TestMethod>
    Public Sub Groups_NumericValuesAndNumericTextUseSameKey()
        Dim result As ViolinPlotResult = Compute(New Double() {1, 2, 3, 4},
                                                  New Object() {1, "01", 2.0R, "2"})
        Assert.AreEqual(2, result.GroupCount)
        Assert.AreEqual(2, result.Series(0).SampleSize)
        Assert.AreEqual(2, result.Series(1).SampleSize)
    End Sub

    <TestMethod>
    Public Sub Groups_ConventionalDecimalStringIsNumerical()
        Dim before As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US")
            Dim result As ViolinPlotResult = Compute(New Double() {1, 2}, New Object() {1.5R, "1.5"})
            Assert.AreEqual(1, result.GroupCount)
        Finally
            CultureInfo.CurrentCulture = before
        End Try
    End Sub

    <TestMethod>
    Public Sub Groups_LocalizedDecimalTextIsNumerical()
        Dim before As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sk-SK")
            Dim result As ViolinPlotResult = Compute(New Double() {1, 2}, New Object() {1.5R, "1,5"})
            Assert.AreEqual(1, result.GroupCount)
        Finally
            CultureInfo.CurrentCulture = before
        End Try
    End Sub

    <TestMethod>
    Public Sub Bandwidth_ManualValueIsRetained()
        Dim result As ViolinPlotResult = Sample(New ViolinPlotOptions With {
            .BandwidthRule = ViolinBandwidthRule.Manual, .ManualBandwidth = 0.75R})
        For Each s As ViolinPlotSeries In result.Series
            Close(0.75R, s.Bandwidth)
        Next
    End Sub

    <TestMethod>
    Public Sub Bandwidth_ScottProducesPositiveFiniteBandwidth()
        Dim result As ViolinPlotResult = Sample(New ViolinPlotOptions With {.BandwidthRule = ViolinBandwidthRule.Scott})
        Assert.IsTrue(result.Series.All(Function(s) s.Bandwidth > 0 AndAlso Not Double.IsInfinity(s.Bandwidth)))
    End Sub

    <TestMethod>
    Public Sub Bandwidth_AdjustmentMultipliesSelectedRule()
        Dim a As ViolinPlotResult = Sample(New ViolinPlotOptions With {.BandwidthAdjustment = 1.0R})
        Dim b As ViolinPlotResult = Sample(New ViolinPlotOptions With {.BandwidthAdjustment = 2.0R})
        Close(a.Series(0).Bandwidth * 2, b.Series(0).Bandwidth)
        Close(a.Series(1).Bandwidth * 2, b.Series(1).Bandwidth)
    End Sub

    <TestMethod>
    Public Sub Bandwidth_ConstantSingleObservationFallbackProducesPositiveKde()
        Dim result As ViolinPlotResult = Compute(New Double() {7}, New Object() {"constant"})
        Assert.IsTrue(result.Series(0).Bandwidth > 0)
        Assert.IsTrue(result.YMinimum < 7)
        Assert.IsTrue(result.YMaximum > 7)
    End Sub

    <TestMethod>
    Public Sub Grid_TrimmedUsesObservedLimits()
        Dim result As ViolinPlotResult = Sample(New ViolinPlotOptions With {.Trim = True})
        Close(1, result.Series(0).EvaluationPoints(0))
        Close(3, result.Series(0).EvaluationPoints.Last())
    End Sub

    <TestMethod>
    Public Sub Grid_UntrimmedExtendsThreeBandwidths()
        Dim result As ViolinPlotResult = Sample(New ViolinPlotOptions With {.Trim = False})
        Dim s As ViolinPlotSeries = result.Series(0)
        Close(s.Minimum - 3 * s.Bandwidth, s.EvaluationPoints(0))
        Close(s.Maximum + 3 * s.Bandwidth, s.EvaluationPoints.Last())
    End Sub

    <TestMethod>
    Public Sub Scaling_EqualMaximumWidthNormalizesEveryViolin()
        Dim result As ViolinPlotResult = Sample(New ViolinPlotOptions With {
            .ScaleMode = ViolinScaleMode.EqualMaximumWidth, .MaximumHalfWidth = 0.3R})
        For Each s As ViolinPlotSeries In result.Series
            Close(0.3R, s.ScaledHalfWidths.Max())
        Next
    End Sub

    <TestMethod>
    Public Sub Scaling_CountModeRespectsRelativeSampleSize()
        Dim result As ViolinPlotResult = Sample(New ViolinPlotOptions With {.ScaleMode = ViolinScaleMode.Count})
        Close(0.2R, result.Series(0).ScaledHalfWidths.Max())
        Close(0.4R, result.Series(1).ScaledHalfWidths.Max())
    End Sub

    <TestMethod>
    Public Sub Scaling_EqualAreaUsesCommonDensityToWidthFactor()
        Dim result As ViolinPlotResult = Sample(New ViolinPlotOptions With {.ScaleMode = ViolinScaleMode.EqualArea})
        Dim factors As New System.Collections.Generic.List(Of Double)()
        For Each s As ViolinPlotSeries In result.Series
            Dim grid As Double() = s.EvaluationPoints
            Dim density As Double() = s.Density
            Dim area As Double = 0
            For i As Integer = 1 To grid.Length - 1
                area += (grid(i) - grid(i - 1)) * (density(i - 1) + density(i)) / 2
            Next
            factors.Add(s.ScaledHalfWidths.Max() * area / density.Max())
        Next
        Close(factors(0), factors(1))
    End Sub

    <TestMethod>
    Public Sub Summary_StatisticsAndTukeyWhiskersArePreserved()
        Dim result As ViolinPlotResult = Compute(New Double() {1, 2, 3, 4, 100},
                                                  New Object() {"A", "A", "A", "A", "A"})
        Dim s As ViolinPlotSeries = result.Series(0)
        Close(1, s.Minimum)
        Close(100, s.Maximum)
        Close(3, s.Median)
        Close(22, s.Mean)
        Assert.IsTrue(s.WhiskerLow >= s.Minimum)
        Assert.IsTrue(s.WhiskerHigh <= s.Maximum)
        Assert.IsTrue(s.Q1 <= s.Median AndAlso s.Median <= s.Q3)
    End Sub

    <TestMethod>
    Public Sub Contracts_OptionsAreCopiedOnResultCreationAndRead()
        Dim options As New ViolinPlotOptions With {.GridPoints = 64, .MaximumHalfWidth = 0.25R}
        Dim result As ViolinPlotResult = Sample(options)
        options.GridPoints = 128
        Dim outOptions As ViolinPlotOptions = result.Options
        outOptions.MaximumHalfWidth = 0.35R
        Assert.AreEqual(64, result.Options.GridPoints)
        Close(0.25R, result.Options.MaximumHalfWidth)
    End Sub

    <TestMethod>
    Public Sub Contracts_ResultArraysAreDefensivelyCopied()
        Dim result As ViolinPlotResult = Sample()
        Dim series As ViolinPlotSeries() = result.Series
        Dim original As Double = series(0).Density(0)
        Dim density As Double() = series(0).Density
        density(0) = -1
        Dim observations As Double() = series(0).Observations
        observations(0) = -1
        Dim grid As Double() = series(0).EvaluationPoints
        grid(0) = -1
        Dim widths As Double() = series(0).ScaledHalfWidths
        widths(0) = -1
        series(0) = Nothing
        Close(original, result.Series(0).Density(0))
        Assert.IsTrue(result.Series(0).Observations(0) >= 0)
        Assert.IsTrue(result.Series(0).EvaluationPoints(0) >= 0)
        Assert.IsTrue(result.Series(0).ScaledHalfWidths(0) >= 0)
    End Sub

    <TestMethod>
    Public Sub Contracts_ComputeDoesNotModifyInputs()
        Dim values As Double() = {4, 3, 2, 1}
        Dim groups As Object() = {"B", "B", "A", "A"}
        Compute(values, groups)
        CollectionAssert.AreEqual(New Double() {4, 3, 2, 1}, values)
        CollectionAssert.AreEqual(New Object() {"B", "B", "A", "A"}, groups)
    End Sub

    <TestMethod>
    Public Sub Validation_UnknownBandwidthRuleThrows()
        Dim opts As New ViolinPlotOptions With {.BandwidthRule = CType(999, ViolinBandwidthRule)}
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Sample(opts))
    End Sub

    <TestMethod>
    Public Sub Validation_UnknownScaleModeThrows()
        Dim opts As New ViolinPlotOptions With {.ScaleMode = CType(999, ViolinScaleMode)}
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Sample(opts))
    End Sub

    <TestMethod>
    Public Sub Validation_GridLimitsThrow()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Sample(New ViolinPlotOptions With {.GridPoints = 31}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Sample(New ViolinPlotOptions With {.GridPoints = 1025}))
    End Sub

    <TestMethod>
    Public Sub Validation_WidthLimitsThrow()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Sample(New ViolinPlotOptions With {.MaximumHalfWidth = 0}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Sample(New ViolinPlotOptions With {.MaximumHalfWidth = 0.5R}))
    End Sub

    <TestMethod>
    Public Sub Validation_ManualBandwidthIsRequiredAndPositive()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Sample(New ViolinPlotOptions With {.BandwidthRule = ViolinBandwidthRule.Manual}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Sample(New ViolinPlotOptions With {
            .BandwidthRule = ViolinBandwidthRule.Manual, .ManualBandwidth = Double.NaN}))
    End Sub

    <TestMethod>
    Public Sub Validation_BandwidthAdjustmentMustBePositive()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Sample(New ViolinPlotOptions With {.BandwidthAdjustment = 0}))
    End Sub

    <TestMethod>
    Public Sub Numerical_RunIsDeterministic()
        Dim first As ViolinPlotResult = Sample()
        Dim second As ViolinPlotResult = Sample()
        For i As Integer = 0 To first.GroupCount - 1
            CollectionAssert.AreEqual(first.Series(i).Density, second.Series(i).Density)
            CollectionAssert.AreEqual(first.Series(i).ScaledHalfWidths, second.Series(i).ScaledHalfWidths)
        Next
    End Sub
End Class
