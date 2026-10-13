Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Direct tests of the portable polar coordinate, grid, and grouping engine.</summary>
<TestClass>
Public Class PolarPlotCoreTests
    Private Shared Function Compute(radius As Double(), angle As Double(),
                                    Optional options As PolarPlotOptions = Nothing,
                                    Optional groups As Array = Nothing) As PolarPlotResult
        Return New PolarPlotCore(radius, angle, options, groups).Compute()
    End Function

    Private Shared Sub Close(expected As Double, actual As Double,
                             Optional tolerance As Double = 0.000000001R)
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

    Private Shared Function FixedScale(Optional angleUnit As PolarAngleUnit = PolarAngleUnit.Degrees,
                                       Optional rotation As PolarRotation = PolarRotation.Counterclockwise,
                                       Optional zero As PolarZeroAngle = PolarZeroAngle.East) As PolarPlotOptions
        Return New PolarPlotOptions With {
            .RadialMinimum = 0, .RadialMaximum = 10,
            .AngleUnit = angleUnit, .Rotation = rotation, .ZeroAngle = zero}
    End Function

    <TestMethod>
    Public Sub DefaultOptions_UseMathematicalCoordinatesAndEightSpokes()
        Dim r As PolarPlotResult = Compute({1, 2}, {0, 90})
        Assert.AreEqual(PolarAngleUnit.Degrees, r.AngleUnit)
        Assert.AreEqual(PolarRotation.Counterclockwise, r.Rotation)
        Assert.AreEqual(PolarZeroAngle.East, r.ZeroAngle)
        Assert.IsTrue(r.ConnectPoints)
        Close(0, r.RadialMinimum)
        Close(1, r.Points(0).X)
        Close(0, r.Points(0).Y)
        Close(0, r.Points(1).X)
        Close(2, r.Points(1).Y)
        Assert.AreEqual(8, r.Spokes.Length)
        Assert.AreEqual(8, r.AngularLabels.Length)
        Assert.AreEqual(2, r.ValidPointCount)
        Assert.IsTrue(r.CartesianExtent > r.RadialSpan)
    End Sub

    <TestMethod>
    Public Sub AngleNormalization_WrapsNegativeAndMultipleTurns()
        Dim r As PolarPlotResult = Compute({3, 3, 3}, {-90, 270, 630}, FixedScale())
        For Each pt As PolarPlotPoint In r.Points
            Close(3.0R * Math.PI / 2.0R, pt.NormalizedAngleRadians)
            Close(0, pt.X)
            Close(-3, pt.Y)
        Next
    End Sub

    <TestMethod>
    Public Sub AngleUnits_RadiansAreNotInterpretedAsDegrees()
        Dim r As PolarPlotResult = Compute({2}, {Math.PI / 2.0R}, FixedScale(PolarAngleUnit.Radians))
        Close(0, r.Points(0).X)
        Close(2, r.Points(0).Y)
        Close(Math.PI / 4.0R, r.AngularMajorInterval)
    End Sub

    <TestMethod>
    Public Sub AngleUnits_PercentagesRepresentCompleteTurn()
        Dim r As PolarPlotResult = Compute({4, 4}, {25, 125}, FixedScale(PolarAngleUnit.Percentage))
        Close(0, r.Points(0).X)
        Close(4, r.Points(0).Y)
        Close(r.Points(0).X, r.Points(1).X)
        Close(12.5R, r.AngularTickInterval)
    End Sub

    <TestMethod>
    Public Sub Rotation_ClockwiseReversesPositiveAngle()
        Dim r As PolarPlotResult = Compute({2}, {90}, FixedScale(rotation:=PolarRotation.Clockwise))
        Close(0, r.Points(0).X)
        Close(-2, r.Points(0).Y)
    End Sub

    <TestMethod>
    Public Sub ZeroDirection_NorthEastSouthWestRotateCorrectly()
        Dim expectedX As Double() = {0, 2, 0, -2}
        Dim expectedY As Double() = {2, 0, -2, 0}
        Dim directions As PolarZeroAngle() = {PolarZeroAngle.North, PolarZeroAngle.East,
                                              PolarZeroAngle.South, PolarZeroAngle.West}
        For i As Integer = 0 To 3
            Dim r As PolarPlotResult = Compute({2}, {0}, FixedScale(zero:=directions(i)))
            Close(expectedX(i), r.Points(0).X)
            Close(expectedY(i), r.Points(0).Y)
        Next
    End Sub

    <TestMethod>
    Public Sub MissingRadius_BreaksLineWithoutDroppingSourceRow()
        Dim r As PolarPlotResult = Compute({1, Double.NaN, 3}, {0, 30, 90}, FixedScale())
        Assert.AreEqual(3, r.Points.Length)
        Assert.AreEqual(1, r.MissingPointCount)
        Assert.IsTrue(r.Points(1).IsMissing)
        Assert.IsFalse(r.Points(1).IsPlottable)
        Assert.IsTrue(Double.IsNaN(r.Points(1).X))
        Assert.AreEqual(2, r.DataSegments.Length)
        Assert.AreEqual(2, r.ValidPointCount)
        Assert.AreEqual(2, r.Points(2).SourceIndex)
    End Sub

    <TestMethod>
    Public Sub MissingAngle_IsAlsoMissingObservation()
        Dim r As PolarPlotResult = Compute({1, 2}, {Double.NaN, 90}, FixedScale())
        Assert.AreEqual(1, r.MissingPointCount)
        Assert.AreEqual(1, r.ValidPointCount)
    End Sub

    <TestMethod>
    Public Sub InfinityInRadiusOrAngle_IsRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() Compute({1, Double.PositiveInfinity}, {0, 90}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() Compute({1}, {Double.NegativeInfinity}))
    End Sub

    <TestMethod>
    Public Sub Input_NullEmptyAndMismatchedArraysAreRejected()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() Compute(Nothing, {1}))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() Compute({1}, Nothing))
        Assert.ThrowsException(Of ArgumentException)(Sub() Compute(New Double() {}, New Double() {}))
        Assert.ThrowsException(Of ArgumentException)(Sub() Compute({1, 2}, {0}))
    End Sub

    <TestMethod>
    Public Sub Input_NoCompletePairIsRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() Compute({Double.NaN}, {Double.NaN}))
    End Sub

    <TestMethod>
    Public Sub RadialScale_AutomaticScaleIncludesZeroAndNegativeData()
        Dim r As PolarPlotResult = Compute({-2, 4}, {0, 180})
        Assert.IsTrue(r.RadialMinimum <= -2)
        Assert.IsTrue(r.RadialMaximum >= 4)
        Assert.IsTrue(r.RadialMajorInterval > 0)
        Close(-2 - r.RadialMinimum, r.Points(0).PlotRadius)
    End Sub

    <TestMethod>
    Public Sub RadialScale_ExplicitMinimumMapsToCentre()
        Dim options As New PolarPlotOptions With {.RadialMinimum = -5, .RadialMaximum = 5}
        Dim r As PolarPlotResult = Compute({-5, 5}, {0, 0}, options)
        Close(0, r.Points(0).PlotRadius)
        Close(10, r.Points(1).PlotRadius)
        Close(10, r.RadialSpan)
    End Sub

    <TestMethod>
    Public Sub RadialLimits_OutOfRangePointsAreRetainedButNotRendered()
        Dim r As PolarPlotResult = Compute({-1, 2, 11}, {0, 0, 0}, FixedScale())
        Assert.AreEqual(3, r.Points.Length)
        Assert.AreEqual(2, r.OutsideRadialLimitCount)
        Assert.AreEqual(1, r.ValidPointCount)
        Assert.IsTrue(r.Points(0).IsOutsideRadialLimits)
        Assert.IsTrue(r.Points(2).IsOutsideRadialLimits)
        Assert.IsFalse(r.Points(1).IsOutsideRadialLimits)
        Assert.IsTrue(Double.IsNaN(r.Points(0).X))
    End Sub

    <TestMethod>
    Public Sub RadialLimits_ExcludingAllPointsThrows()
        Assert.ThrowsException(Of ArgumentException)(Sub() Compute({12, 13}, {0, 45}, FixedScale()))
    End Sub

    <TestMethod>
    Public Sub RadialTicks_OuterLimitIncludedWhenNotDivisible()
        Dim options As PolarPlotOptions = FixedScale()
        options.RadialTickInterval = 3
        Dim r As PolarPlotResult = Compute({1, 2}, {0, 90}, options)
        Close(3, r.RadialMajorInterval)
        Close(3, r.RadialTickInterval)
        Close(10, r.RadialLabels(r.RadialLabels.Length - 1).X)
        Assert.AreEqual(5, r.RadialLabels.Length)
        Assert.AreEqual(4, r.GridCircles.Length)
    End Sub

    <TestMethod>
    Public Sub AngularTicks_CustomDegreesGenerateSpokes()
        Dim options As PolarPlotOptions = FixedScale()
        options.AngularTickInterval = 90
        Dim r As PolarPlotResult = Compute({1}, {0}, options)
        Assert.AreEqual(4, r.Spokes.Length)
        Close(90, r.AngularTickInterval)
        Close(Math.PI / 2.0R, r.AngularMajorIntervalRadians)
    End Sub

    <TestMethod>
    Public Sub AngularTicks_CustomPercentageGenerateSpokes()
        Dim options As PolarPlotOptions = FixedScale(PolarAngleUnit.Percentage)
        options.AngularTickInterval = 25
        Dim r As PolarPlotResult = Compute({1}, {0}, options)
        Assert.AreEqual(4, r.Spokes.Length)
        Close(25, r.AngularMajorInterval)
    End Sub

    <TestMethod>
    Public Sub AngularTicks_TinyIntervalIsRejected()
        Dim options As PolarPlotOptions = FixedScale()
        options.AngularTickInterval = 0.00001R
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Compute({1}, {0}, options))
    End Sub

    <TestMethod>
    Public Sub RadialTicks_TinyIntervalIsRejected()
        Dim options As PolarPlotOptions = FixedScale()
        options.RadialTickInterval = 0.00001R
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Compute({1}, {0}, options))
    End Sub

    <TestMethod>
    Public Sub Options_InvalidIntervalsAndLimitsAreRejected()
        Dim options As PolarPlotOptions = FixedScale()
        options.RadialTickInterval = 0
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Compute({1}, {0}, options))
        options.RadialTickInterval = Nothing
        options.AngularTickInterval = 361
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Compute({1}, {0}, options))
        options.AngularTickInterval = Nothing
        options.RadialMaximum = -10
        Assert.ThrowsException(Of ArgumentException)(Sub() Compute({1}, {0}, options))
    End Sub

    <TestMethod>
    Public Sub Options_InvalidEnumMembersAreRejected()
        Dim options As PolarPlotOptions = FixedScale()
        options.AngleUnit = CType(99, PolarAngleUnit)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Compute({1}, {0}, options))
        options = FixedScale()
        options.Rotation = CType(99, PolarRotation)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Compute({1}, {0}, options))
        options = FixedScale()
        options.ZeroAngle = CType(99, PolarZeroAngle)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Compute({1}, {0}, options))
    End Sub

    <TestMethod>
    Public Sub Options_DisableConnectionsIsReflectedInResult()
        Dim options As PolarPlotOptions = FixedScale()
        options.ConnectPoints = False
        Assert.IsFalse(Compute({1, 2}, {0, 90}, options).ConnectPoints)
    End Sub

    <TestMethod>
    Public Sub Grouped_FirstOccurrenceOrderAndSourceIndicesArePreserved()
        Dim r As PolarPlotResult = Compute({1, 2, 3, 4}, {0, 90, 180, 270},
                                           FixedScale(), New Object() {" B ", "A", "B", "A"})
        Assert.IsTrue(r.HasGrouping)
        Assert.AreEqual(2, r.GroupCount)
        Assert.AreEqual("B", r.GroupSeries(0).Name)
        Assert.AreEqual("A", r.GroupSeries(1).Name)
        CollectionAssert.AreEqual(New Integer() {0, 2}, r.GroupSeries(0).SourceIndices)
        CollectionAssert.AreEqual(New Integer() {1, 3}, r.GroupSeries(1).SourceIndices)
    End Sub

    <TestMethod>
    Public Sub Grouped_InterleavedGroupsDoNotSplitLineSegments()
        Dim r As PolarPlotResult = Compute({1, 2, 3, 4, 5}, {0, 0, 0, 0, 0},
                                           FixedScale(), New Object() {"A", "B", "A", "B", "A"})
        Assert.AreEqual(1, r.GroupSeries(0).DataSegments.Length)
        Assert.AreEqual(3, r.GroupSeries(0).DataSegments(0).Count)
        Assert.AreEqual(1, r.GroupSeries(1).DataSegments.Length)
    End Sub

    <TestMethod>
    Public Sub Grouped_FullyMissingRowBreaksEveryGroupLine()
        Dim r As PolarPlotResult = Compute({1, Double.NaN, 2}, {0, Double.NaN, 90},
                                           FixedScale(), New Object() {"A", Nothing, "A"})
        Assert.AreEqual(2, r.GroupSeries(0).DataSegments.Length)
        Assert.AreEqual(1, r.MissingPointCount)
    End Sub

    <TestMethod>
    Public Sub Grouped_MissingGroupIdentifiersAreOmitted()
        Dim r As PolarPlotResult = Compute({1, 2, 3, 4}, {0, 0, 0, 0},
                                           FixedScale(), New Object() {"A", Nothing, "   ", DBNull.Value})
        Assert.AreEqual(3, r.MissingGroupCount)
        Assert.AreEqual(1, r.ValidPointCount)
        Assert.AreEqual(1, r.GroupCount)
    End Sub

    <TestMethod>
    Public Sub Grouped_NumericAndCharacterCategoriesAreSupported()
        Dim r As PolarPlotResult = Compute({1, 2, 3}, {0, 0, 0},
                                           FixedScale(), New Object() {1, 1.0R, "C"c})
        Assert.AreEqual(2, r.GroupCount)
        Assert.AreEqual(2, r.GroupSeries(0).Count)
        Assert.AreEqual("C", r.GroupSeries(1).Name)
    End Sub

    <TestMethod>
    Public Sub Grouped_ArrayWithNonzeroLowerBoundIsAccepted()
        Dim groups As Array = Array.CreateInstance(GetType(Object), New Integer() {2}, New Integer() {7})
        groups.SetValue("First", 7)
        groups.SetValue("Second", 8)
        Dim r As PolarPlotResult = PolarPlotCore.CreateGrouped({2, 3}, {0, 90}, groups, FixedScale()).Compute()
        Assert.AreEqual(2, r.GroupCount)
        Assert.AreEqual("First", r.GroupSeries(0).Name)
    End Sub

    <TestMethod>
    Public Sub Grouped_InvalidArrayRankLengthAndTypesAreRejected()
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() PolarPlotCore.CreateGrouped({1}, {0}, Nothing))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() Compute({1}, {0}, FixedScale(), New Object() {"A", "B"}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() Compute({1}, {0}, FixedScale(), Array.CreateInstance(GetType(Object), 1, 1)))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() Compute({1}, {0}, FixedScale(), New Object() {New Object()}))
    End Sub

    <TestMethod>
    Public Sub Grouped_AllMissingGroupIdentifiersAreRejected()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() Compute({1, 2}, {0, 90}, FixedScale(), New Object() {Nothing, " "}))
    End Sub

    <TestMethod>
    Public Sub Grouped_InfiniteNumericGroupIsRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() Compute({1}, {0}, FixedScale(), New Object() {Double.PositiveInfinity}))
    End Sub

    <TestMethod>
    Public Sub InputsAndOptions_AreCopiedByConstructor()
        Dim radii As Double() = {2, 3}
        Dim angles As Double() = {0, 90}
        Dim groups As Object() = {"A", "B"}
        Dim options As PolarPlotOptions = FixedScale()
        Dim plot As New PolarPlotCore(radii, angles, options, groups)
        radii(0) = 9
        angles(0) = 180
        groups(0) = "X"
        options.RadialMinimum = 5
        Dim r As PolarPlotResult = plot.Compute()
        Close(0, r.RadialMinimum)
        Close(2, r.Points(0).X)
        Assert.AreEqual("A", r.GroupSeries(0).Name)
    End Sub

    <TestMethod>
    Public Sub Results_ExposedArraysAreDefensiveCopies()
        Dim r As PolarPlotResult = Compute({1, 2}, {0, 90}, FixedScale())
        Dim p As PolarPlotPoint() = r.Points
        p(0) = Nothing
        Assert.IsNotNull(r.Points(0))
        Dim xs As Double() = r.MarkerSeries.XValues
        xs(0) = 999
        Close(1, r.MarkerSeries.XValues(0))
        Dim groups As PolarPlotGroupSeries() = r.GroupSeries
        groups(0) = Nothing
        Assert.IsNotNull(r.GroupSeries(0))
    End Sub
End Class
