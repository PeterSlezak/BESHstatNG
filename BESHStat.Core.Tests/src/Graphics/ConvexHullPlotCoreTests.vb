Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Direct, Excel-independent tests of the portable convex-hull engine and DTOs.</summary>
<TestClass>
Public Class ConvexHullPlotCoreTests
    Private Shared Sub Near(expected As Double, actual As Double, Optional tolerance As Double = 0.0000000001R)
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

    Private Shared Function Square(Optional options As ConvexHullPlotOptions = Nothing) As ConvexHullPlotResult
        Return ConvexHullPlotCore.Compute(New Double() {0, 2, 2, 0},
                                          New Double() {0, 0, 2, 2}, options)
    End Function

    <TestMethod>
    Public Sub Square_HullAreaPerimeterAndClosedCoordinates()
        Dim result As ConvexHullPlotResult = Square()
        Assert.AreEqual(1, result.Groups.Length)
        Assert.IsFalse(result.HasGrouping)
        Assert.AreEqual(4, result.SourceObservationCount)
        Assert.AreEqual(4, result.ValidObservationCount)
        Assert.AreEqual(0, result.OmittedObservationCount)
        Dim group As ConvexHullPlotGroup = result.Groups(0)
        Assert.AreEqual(4, group.Points.Length)
        Assert.AreEqual(4, group.HullVertices.Length)
        Assert.AreEqual(5, group.ClosedHullX.Length)
        Assert.AreEqual(5, group.ClosedHullY.Length)
        Near(group.ClosedHullX(0), group.ClosedHullX(4))
        Near(group.ClosedHullY(0), group.ClosedHullY(4))
        Near(4, group.Area)
        Near(8, group.Perimeter)
        Assert.IsFalse(group.IsDegenerate)
    End Sub

    <TestMethod>
    Public Sub DuplicateAndInteriorPoints_RetainedAsObservationsButNotVertices()
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.Compute(
            New Double() {0, 2, 2, 0, 0, 1},
            New Double() {0, 0, 2, 2, 0, 1})
        Assert.AreEqual(6, result.Groups(0).Points.Length)
        Assert.AreEqual(4, result.Groups(0).HullVertices.Length)
        Assert.AreEqual(4, result.Groups(0).Points(4).SourceIndex)
        Assert.IsFalse(result.Groups(0).Points(4).IsSynthetic)
        Near(4, result.Groups(0).Area)
    End Sub

    <TestMethod>
    Public Sub CollinearPoints_RetainBoundaryByDefault()
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.Compute(
            New Double() {0, 1, 2, 3}, New Double() {5, 5, 5, 5})
        Assert.AreEqual(4, result.Groups(0).HullVertices.Length)
        Near(0, result.Groups(0).Area)
    End Sub

    <TestMethod>
    Public Sub CollinearPoints_ExcludeIntermediateWhenRequested()
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.Compute(
            New Double() {0, 1, 2, 3}, New Double() {5, 5, 5, 5},
            New ConvexHullPlotOptions With {.IncludeCollinearBoundaryPoints = False})
        Assert.AreEqual(2, result.Groups(0).HullVertices.Length)
        Assert.AreEqual(2, result.Groups(0).ClosedHullX.Length)
        Near(6, result.Groups(0).Perimeter)
    End Sub

    <TestMethod>
    Public Sub SinglePoint_IsDegenerateWithZeroMeasurements()
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.Compute(
            New Double() {4}, New Double() {-3})
        Dim group As ConvexHullPlotGroup = result.Groups(0)
        Assert.AreEqual(1, group.HullVertices.Length)
        Assert.AreEqual(1, group.ClosedHullX.Length)
        Near(0, group.Area)
        Near(0, group.Perimeter)
        Assert.IsTrue(group.IsDegenerate)
    End Sub

    <TestMethod>
    Public Sub TwoPoints_PerimeterCountsBothDirections()
        Dim group As ConvexHullPlotGroup = ConvexHullPlotCore.Compute(
            New Double() {0, 3}, New Double() {0, 4}).Groups(0)
        Assert.AreEqual(2, group.HullVertices.Length)
        Near(10, group.Perimeter)
        Near(0, group.Area)
        Assert.IsTrue(group.IsDegenerate)
    End Sub

    <TestMethod>
    Public Sub MissingCoordinates_AreOmittedAndSourceIndicesPreserved()
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.Compute(
            New Double() {0, Double.NaN, 2, 0},
            New Double() {0, 1, 2, Double.NaN})
        Assert.AreEqual(4, result.SourceObservationCount)
        Assert.AreEqual(2, result.ValidObservationCount)
        Assert.AreEqual(2, result.OmittedObservationCount)
        Assert.AreEqual(0, result.Groups(0).Points(0).SourceIndex)
        Assert.AreEqual(2, result.Groups(0).Points(1).SourceIndex)
    End Sub

    <TestMethod>
    Public Sub MissingAllCoordinates_ThrowsArgumentException()
        Assert.ThrowsException(Of ArgumentException)(Sub() ConvexHullPlotCore.Compute(
            New Double() {Double.NaN}, New Double() {1}))
    End Sub

    <TestMethod>
    Public Sub InfiniteCoordinates_AreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() ConvexHullPlotCore.Compute(
            New Double() {Double.PositiveInfinity}, New Double() {1}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() ConvexHullPlotCore.Compute(
            New Double() {1}, New Double() {Double.NegativeInfinity}))
    End Sub

    <TestMethod>
    Public Sub NullEmptyAndMismatchedInputs_AreRejected()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ConvexHullPlotCore.Compute(
            Nothing, New Double() {1}))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ConvexHullPlotCore.Compute(
            New Double() {1}, Nothing))
        Assert.ThrowsException(Of ArgumentException)(Sub() ConvexHullPlotCore.Compute(
            New Double() {}, New Double() {}))
        Assert.ThrowsException(Of ArgumentException)(Sub() ConvexHullPlotCore.Compute(
            New Double() {1}, New Double() {1, 2}))
    End Sub

    <TestMethod>
    Public Sub GroupedInput_PreservesFirstAppearanceAndGroupNames()
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.ComputeGrouped(
            New Double() {0, 1, 2, 3}, New Double() {1, 2, 3, 4},
            New Object() {" B ", "A", "B", "C"})
        Assert.IsTrue(result.HasGrouping)
        Assert.AreEqual(3, result.Groups.Length)
        Assert.AreEqual("B", result.Groups(0).Name)
        Assert.AreEqual("A", result.Groups(1).Name)
        Assert.AreEqual("C", result.Groups(2).Name)
        Assert.AreEqual(2, result.Groups(0).Points.Length)
        Assert.AreEqual(2, result.Groups(0).Points(1).SourceIndex)
    End Sub

    <TestMethod>
    Public Sub GroupedInput_OmitsMissingAndBlankGroupValues()
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.Compute(
            New Double() {0, 1, 2, 3}, New Double() {0, 1, 2, 3},
            groupingValues:=New Object() {"A", Nothing, DBNull.Value, "  "})
        Assert.AreEqual(1, result.ValidObservationCount)
        Assert.AreEqual(3, result.OmittedObservationCount)
        Assert.AreEqual(1, result.Groups.Length)
        Assert.AreEqual("A", result.Groups(0).Name)
    End Sub

    <TestMethod>
    Public Sub GroupedInput_NumberValuesShareCanonicalIdentity()
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.ComputeGrouped(
            New Double() {0, 1, 2, 3, 4}, New Double() {0, 1, 2, 3, 4},
            New Object() {1, 1D, 1.0R, 1.0F, "1"})
        Assert.AreEqual(2, result.Groups.Length)
        Assert.AreEqual(4, result.Groups(0).Points.Length)
        Assert.AreEqual("1", result.Groups(1).Name)
        Assert.AreEqual(1, result.Groups(1).Points.Length)
    End Sub

    <TestMethod>
    Public Sub GroupedInput_OneBasedColumnAndRowArraysSupported()
        Dim column As Array = Array.CreateInstance(GetType(Object),
            New Integer() {3, 1}, New Integer() {2, 7})
        column.SetValue("A", 2, 7)
        column.SetValue("B", 3, 7)
        column.SetValue("A", 4, 7)
        Dim x As Double() = {1, 2, 3}
        Dim y As Double() = {3, 2, 1}
        Dim colResult As ConvexHullPlotResult = ConvexHullPlotCore.ComputeGrouped(x, y, column)
        Assert.AreEqual(2, colResult.Groups.Length)
        Assert.AreEqual(2, colResult.Groups(0).Points.Length)
        Dim row As Array = Array.CreateInstance(GetType(Object),
            New Integer() {1, 3}, New Integer() {3, 5})
        row.SetValue("A", 3, 5)
        row.SetValue("B", 3, 6)
        row.SetValue("A", 3, 7)
        Dim rowResult As ConvexHullPlotResult = ConvexHullPlotCore.ComputeGrouped(x, y, row)
        Assert.AreEqual(2, rowResult.Groups.Length)
        Assert.AreEqual(2, rowResult.Groups(0).Points.Length)
    End Sub

    <TestMethod>
    Public Sub GroupedInput_NonzeroBoundOneDimensionalArraySupported()
        Dim values As Array = Array.CreateInstance(GetType(Object),
            New Integer() {2}, New Integer() {5})
        values.SetValue("X", 5)
        values.SetValue("Y", 6)
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.ComputeGrouped(
            New Double() {1, 2}, New Double() {1, 2}, values)
        Assert.AreEqual("X", result.Groups(0).Name)
        Assert.AreEqual("Y", result.Groups(1).Name)
    End Sub

    <TestMethod>
    Public Sub GroupedInput_BadShapeLengthAndMissingArgumentRejected()
        Dim matrix(1, 1) As Object
        Assert.ThrowsException(Of ArgumentException)(Sub() ConvexHullPlotCore.ComputeGrouped(
            New Double() {1, 2, 3, 4}, New Double() {1, 2, 3, 4}, matrix))
        Assert.ThrowsException(Of ArgumentException)(Sub() ConvexHullPlotCore.ComputeGrouped(
            New Double() {1, 2}, New Double() {1, 2}, New String() {"A"}))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ConvexHullPlotCore.ComputeGrouped(
            New Double() {1}, New Double() {1}, Nothing))
    End Sub

    <TestMethod>
    Public Sub GroupedInput_UnsupportedTypesAreRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() ConvexHullPlotCore.ComputeGrouped(
            New Double() {1}, New Double() {2}, New Object() {True}))
        Assert.ThrowsException(Of ArgumentException)(Sub() ConvexHullPlotCore.ComputeGrouped(
            New Double() {1}, New Double() {2}, New Object() {New DateTime(2025, 1, 1)}))
    End Sub

    <TestMethod>
    Public Sub GroupedInput_InfiniteGroupValuesRejectedAndNaNsOmitted()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() ConvexHullPlotCore.ComputeGrouped(
            New Double() {1}, New Double() {2}, New Object() {Double.PositiveInfinity}))
        Dim result As ConvexHullPlotResult = ConvexHullPlotCore.ComputeGrouped(
            New Double() {1, 2}, New Double() {3, 4}, New Object() {Double.NaN, "A"})
        Assert.AreEqual(1, result.ValidObservationCount)
        Assert.AreEqual(1, result.OmittedObservationCount)
        Assert.AreEqual("A", result.Groups(0).Name)
    End Sub

    <TestMethod>
    Public Sub Padding_IntroducesSyntheticVerticesWithoutChangingSourceObservations()
        Dim options As New ConvexHullPlotOptions With {.PaddingPercentX = 10, .PaddingPercentY = 10}
        Dim original As ConvexHullPlotResult = Square()
        Dim padded As ConvexHullPlotResult = Square(options)
        Assert.AreEqual(4, padded.Groups(0).Points.Length)
        Assert.IsTrue(padded.Groups(0).HullVertices.Any(Function(p) p.IsSynthetic))
        Assert.IsTrue(padded.Groups(0).HullVertices.All(Function(p) p.IsSynthetic OrElse p.SourceIndex >= 0))
        Assert.IsTrue(padded.Groups(0).Area > original.Groups(0).Area)
        Assert.IsTrue(padded.Groups(0).Perimeter > original.Groups(0).Perimeter)
        Assert.IsTrue(padded.Groups(0).HullVertices.Min(Function(p) p.X) < 0)
        Assert.IsTrue(padded.Groups(0).HullVertices.Max(Function(p) p.Y) > 2)
    End Sub

    <TestMethod>
    Public Sub NoPadding_ProducesSameHullForExplicitZeroOptions()
        Dim normal As ConvexHullPlotResult = Square()
        Dim options As New ConvexHullPlotOptions With {.PaddingPercentX = 0, .PaddingPercentY = 0}
        Dim actual As ConvexHullPlotResult = Square(options)
        CollectionAssert.AreEqual(normal.Groups(0).ClosedHullX, actual.Groups(0).ClosedHullX)
        CollectionAssert.AreEqual(normal.Groups(0).ClosedHullY, actual.Groups(0).ClosedHullY)
    End Sub

    <TestMethod>
    Public Sub Tolerance_ControlsNearCollinearBoundaryRetention()
        Dim x As Double() = {0, 1, 2}
        Dim y As Double() = {0, 0.0000000001R, 0}
        Dim exact As ConvexHullPlotResult = ConvexHullPlotCore.Compute(
            x, y, New ConvexHullPlotOptions With {.IncludeCollinearBoundaryPoints = False})
        Dim relaxed As ConvexHullPlotResult = ConvexHullPlotCore.Compute(
            x, y, New ConvexHullPlotOptions With {
                .IncludeCollinearBoundaryPoints = False, .CollinearityTolerance = 0.001R})
        Assert.AreEqual(3, exact.Groups(0).HullVertices.Length)
        Assert.AreEqual(2, relaxed.Groups(0).HullVertices.Length)
    End Sub

    <TestMethod>
    Public Sub InvalidOptions_AreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Square(
            New ConvexHullPlotOptions With {.PaddingPercentX = -1}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Square(
            New ConvexHullPlotOptions With {.PaddingPercentY = Double.NaN}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Square(
            New ConvexHullPlotOptions With {.CollinearityTolerance = Double.PositiveInfinity}))
    End Sub

    <TestMethod>
    Public Sub ExtremeCoordinates_AndOverflowingToleranceRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() ConvexHullPlotCore.Compute(
            New Double() {1.0E+160R}, New Double() {0}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Square(
            New ConvexHullPlotOptions With {.CollinearityTolerance = Double.MaxValue}))
    End Sub

    <TestMethod>
    Public Sub ResultArrays_AreDefensiveCopies()
        Dim result As ConvexHullPlotResult = Square()
        Dim groupList As ConvexHullPlotGroup() = result.Groups
        groupList(0) = Nothing
        Assert.IsNotNull(result.Groups(0))
        Dim vertices As ConvexHullPoint2D() = result.Groups(0).HullVertices
        vertices(0) = Nothing
        Assert.IsNotNull(result.Groups(0).HullVertices(0))
        Dim closedX As Double() = result.Groups(0).ClosedHullX
        closedX(0) = 999
        Assert.AreNotEqual(999.0R, result.Groups(0).ClosedHullX(0))
        Dim points As ConvexHullPoint2D() = result.Groups(0).Points
        points(0) = Nothing
        Assert.IsNotNull(result.Groups(0).Points(0))
    End Sub

    <TestMethod>
    Public Sub ComputeGrouped_MatchesNamedArgumentOverload()
        Dim x As Double() = {0, 1, 2}
        Dim y As Double() = {1, 2, 3}
        Dim categories As String() = {"X", "Y", "X"}
        Dim a As ConvexHullPlotResult = ConvexHullPlotCore.ComputeGrouped(x, y, categories)
        Dim b As ConvexHullPlotResult = ConvexHullPlotCore.Compute(x, y, groupingValues:=categories)
        Assert.AreEqual(a.Groups.Length, b.Groups.Length)
        Assert.AreEqual(a.SourceObservationCount, b.SourceObservationCount)
        Near(a.Groups(0).Perimeter, b.Groups(0).Perimeter)
    End Sub
End Class
