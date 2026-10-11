Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Direct portable dumbbell engine tests, without Excel or WinForms.</summary>
<TestClass>
Public Class DumbbellPlotCoreTests
    Private Shared Function Basic(Optional options As DumbbellPlotOptions = Nothing) As DumbbellPlotResult
        Return DumbbellPlotCore.Compute(New String() {"B", "A"},
                                        New Double() {5.0R, 3.0R},
                                        New Double() {9.0R, 1.0R}, options)
    End Function

    Private Shared Sub Near(expected As Double, actual As Double)
        Assert.AreEqual(expected, actual, 0.000000001R)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub Endpoints_PreserveOrderAndPrepareConnectorGeometry()
        Dim result As DumbbellPlotResult = Basic()
        Assert.AreEqual(2, result.CategoryCount)
        Assert.AreEqual(2, result.SourceCategoryCount)
        Assert.AreEqual(0, result.OmittedCategoryCount)
        Assert.IsFalse(result.HasAuxiliaryObservations)
        Assert.AreEqual(DumbbellAxisValueKind.Numeric, result.AxisValueKind)
        Near(1.0R, result.MinimumValue)
        Near(9.0R, result.MaximumValue)
        Dim first As DumbbellPlotCategory = result.Categories(0)
        Assert.AreEqual("B", first.CategoryLabel)
        Assert.AreEqual("S:B", first.CategoryKey)
        Assert.AreEqual(0, first.SourceIndex)
        Assert.AreEqual(0, first.DisplayIndex)
        Near(2.0R, first.YPosition)
        Near(5.0R, first.ConnectorStart)
        Near(9.0R, first.ConnectorEnd)
        Near(4.0R, first.ConnectorLength)
        Dim second As DumbbellPlotCategory = result.Categories(1)
        Assert.AreEqual("A", second.CategoryLabel)
        Assert.AreEqual(1, second.SourceIndex)
        Near(1.0R, second.YPosition)
        Near(-2.0R, second.Difference)
        Near(2.0R, second.AbsoluteDifference)
        Near(1.0R, second.ConnectorStart)
        Near(3.0R, second.ConnectorEnd)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub FirstValueSort_IsStableForTies()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(
            New String() {"B", "A", "C"}, New Double() {2, 1, 2},
            New Double() {3, 4, 0},
            New DumbbellPlotOptions With {.SortMode = DumbbellPlotSortMode.FirstAscending})
        Assert.AreEqual(1, result.Categories(0).SourceIndex)
        Assert.AreEqual(0, result.Categories(1).SourceIndex)
        Assert.AreEqual(2, result.Categories(2).SourceIndex)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub DifferenceSort_UsesSignedDifferences()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(
            New String() {"B", "A", "C"}, New Double() {2, 1, 2},
            New Double() {3, 4, 0},
            New DumbbellPlotOptions With {.SortMode = DumbbellPlotSortMode.DifferenceDescending})
        Assert.AreEqual("A", result.Categories(0).CategoryLabel)
        Assert.AreEqual("B", result.Categories(1).CategoryLabel)
        Assert.AreEqual("C", result.Categories(2).CategoryLabel)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub AbsoluteDifferenceSort_UsesMagnitude()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(
            New String() {"B", "A", "C"}, New Double() {2, 1, 2},
            New Double() {3, 4, 0},
            New DumbbellPlotOptions With {.SortMode = DumbbellPlotSortMode.AbsoluteDifferenceDescending})
        Assert.AreEqual("A", result.Categories(0).CategoryLabel)
        Assert.AreEqual("C", result.Categories(1).CategoryLabel)
        Assert.AreEqual("B", result.Categories(2).CategoryLabel)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub CategorySortAndReverse_KeepTopToBottomCoordinates()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(
            New String() {"b", "A", "c"}, New Double() {1, 2, 3},
            New Double() {2, 3, 4},
            New DumbbellPlotOptions With {
                .SortMode = DumbbellPlotSortMode.CategoryAscending,
                .ReverseCategoryOrder = True})
        Assert.AreEqual("c", result.Categories(0).CategoryLabel)
        Assert.AreEqual("b", result.Categories(1).CategoryLabel)
        Assert.AreEqual("A", result.Categories(2).CategoryLabel)
        Near(3.0R, result.Categories(0).YPosition)
        Near(2.0R, result.Categories(1).YPosition)
        Near(1.0R, result.Categories(2).YPosition)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub DateEndpoints_AreConvertedToOADates()
        Dim first As New DateTime(2024, 1, 1)
        Dim second As New DateTime(2024, 1, 15)
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(
            New String() {"Period"}, New Object() {first}, New Object() {second})
        Assert.AreEqual(DumbbellAxisValueKind.DateValue, result.AxisValueKind)
        Near(first.ToOADate(), result.MinimumValue)
        Near(second.ToOADate(), result.MaximumValue)
        Near(14.0R, result.Categories(0).Difference)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub DateTimeOffsetAndNumeric_AreClassifiedMixed()
        Dim point As New DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(
            New String() {"P"}, New Object() {point}, New Object() {45000.0R})
        Assert.AreEqual(DumbbellAxisValueKind.Mixed, result.AxisValueKind)
        Near(point.DateTime.ToOADate(), result.Categories(0).FirstValue)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub NonzeroLowerBoundsAndTwoDimensionalVectors_AreSupported()
        Dim categories As Array = Array.CreateInstance(GetType(Object),
            New Integer() {1, 2}, New Integer() {5, -1})
        categories.SetValue("X", 5, -1)
        categories.SetValue("Y", 5, 0)
        Dim first As Array = Array.CreateInstance(GetType(Object), New Integer() {2}, New Integer() {-2})
        first.SetValue(2.0R, -2)
        first.SetValue(3.0R, -1)
        Dim second As Array = Array.CreateInstance(GetType(Object),
            New Integer() {2, 1}, New Integer() {1, 7})
        second.SetValue(5.0R, 1, 7)
        second.SetValue(8.0R, 2, 7)
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(categories, first, second)
        Assert.AreEqual(2, result.CategoryCount)
        Assert.AreEqual("X", result.Categories(0).CategoryLabel)
        Assert.AreEqual("Y", result.Categories(1).CategoryLabel)
        Near(5.0R, result.Categories(0).SecondValue)
        Near(8.0R, result.Categories(1).SecondValue)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub AuxiliaryObservations_MatchNumericKeysAndPermitDuplicates()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.ComputeWithAuxiliary(
            New Object() {1, 2}, New Double() {2, 3}, New Double() {6, 8},
            New Object() {"1", 2, 2, DBNull.Value, "  "},
            New Object() {4.0R, 5.0R, 5.5R, 6.0R, 7.0R})
        Assert.AreEqual(5, result.SourceAuxiliaryObservationCount)
        Assert.AreEqual(3, result.UsableAuxiliaryObservationCount)
        Assert.AreEqual(2, result.OmittedAuxiliaryObservationCount)
        Assert.AreEqual("N:1", result.AuxiliaryObservations(0).CategoryKey)
        Assert.AreEqual(1, result.AuxiliaryObservations(1).CategoryDisplayIndex)
        Near(2.0R, result.AuxiliaryObservations(0).BaseYPosition)
        Near(0.0R, result.AuxiliaryObservations(0).JitterOffset)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub AuxiliaryObservations_ExpandAxisBounds()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A"}, New Double() {5}, New Double() {7},
            New String() {"A", "A"}, New Double() {-20, 30})
        Near(-20.0R, result.MinimumValue)
        Near(30.0R, result.MaximumValue)
        Assert.AreEqual(2, result.UsableAuxiliaryObservationCount)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub Jitter_IsDeterministicAndDoesNotMoveCategoryCoordinates()
        Dim settings As New DumbbellPlotOptions With {
            .AuxiliaryJitterFraction = 0.25R, .AuxiliaryJitterSeed = 42}
        Dim a As DumbbellPlotResult = DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A"}, New Double() {1}, New Double() {2},
            New String() {"A", "A", "A"}, New Double() {1.2R, 1.4R, 1.6R}, settings)
        Dim b As DumbbellPlotResult = DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A"}, New Double() {1}, New Double() {2},
            New String() {"A", "A", "A"}, New Double() {1.2R, 1.4R, 1.6R}, settings)
        For i As Integer = 0 To 2
            Near(a.AuxiliaryObservations(i).JitterOffset, b.AuxiliaryObservations(i).JitterOffset)
            Assert.IsTrue(Math.Abs(a.AuxiliaryObservations(i).JitterOffset) <= 0.25R)
            Near(a.AuxiliaryObservations(i).BaseYPosition + a.AuxiliaryObservations(i).JitterOffset,
                 a.AuxiliaryObservations(i).YPosition)
        Next
        Near(1.0R, a.Categories(0).YPosition)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub ResultArraysAndOptions_AreDefensiveCopies()
        Dim options As New DumbbellPlotOptions With {
            .SortMode = DumbbellPlotSortMode.FirstDescending, .AuxiliaryJitterFraction = 0.15R}
        Dim result As DumbbellPlotResult = Basic(options)
        options.SortMode = DumbbellPlotSortMode.FirstAscending
        Assert.AreEqual(DumbbellPlotSortMode.FirstDescending, result.Options.SortMode)
        Dim returnedOptions As DumbbellPlotOptions = result.Options
        returnedOptions.AuxiliaryJitterFraction = 0.45R
        Near(0.15R, result.Options.AuxiliaryJitterFraction)
        Dim categories As DumbbellPlotCategory() = result.Categories
        categories(0) = Nothing
        Assert.IsNotNull(result.Categories(0))
        Dim auxiliary As DumbbellPlotResult = DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A"}, New Double() {1}, New Double() {2},
            New String() {"A"}, New Double() {1.5R})
        Dim observations As DumbbellAuxiliaryObservation() = auxiliary.AuxiliaryObservations
        observations(0) = Nothing
        Assert.IsNotNull(auxiliary.AuxiliaryObservations(0))
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub IncompleteEndpointRows_CanBeOmittedWithSourceIndicesPreserved()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(
            New Object() {"A", "B", DBNull.Value, "C"},
            New Object() {1.0R, 2.0R, 3.0R, Double.NaN},
            New Object() {4.0R, Nothing, 6.0R, 7.0R},
            New DumbbellPlotOptions With {.OmitIncompleteRows = True})
        Assert.AreEqual(1, result.CategoryCount)
        Assert.AreEqual(4, result.SourceCategoryCount)
        Assert.AreEqual(3, result.OmittedCategoryCount)
        Assert.AreEqual(0, result.Categories(0).SourceIndex)
        Assert.ThrowsException(Of ArgumentException)(Sub() DumbbellPlotCore.Compute(
            New String() {"A", "B"}, New Object() {1.0R, Nothing}, New Double() {2, 3}))
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub MissingAuxiliaryValues_AreOmittedNotZero()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A"}, New Double() {2}, New Double() {3},
            New Object() {"A", "A", Nothing}, New Object() {DBNull.Value, 2.5R, 6.0R})
        Assert.AreEqual(3, result.SourceAuxiliaryObservationCount)
        Assert.AreEqual(1, result.UsableAuxiliaryObservationCount)
        Assert.AreEqual(2, result.OmittedAuxiliaryObservationCount)
        Assert.AreEqual(1, result.AuxiliaryObservations(0).SourceIndex)
        Near(2.5R, result.AuxiliaryObservations(0).Value)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub DuplicateNormalizedKeys_AreRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() DumbbellPlotCore.Compute(
            New Object() {1, "1"}, New Double() {1, 2}, New Double() {3, 4}))
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub TextKeys_AreCaseSensitive()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(
            New String() {"A", "a"}, New Double() {1, 2}, New Double() {3, 4})
        Assert.AreEqual(2, result.CategoryCount)
        Assert.AreNotEqual(result.Categories(0).CategoryKey, result.Categories(1).CategoryKey)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub UnknownAuxiliaryKey_Throws()
        Assert.ThrowsException(Of ArgumentException)(Sub() DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A"}, New Double() {1}, New Double() {2},
            New String() {"Other"}, New Double() {1.5R}))
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub NullInputsAndMismatchedLengths_AreRejected()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() DumbbellPlotCore.Compute(
            Nothing, New Double() {1}, New Double() {2}))
        Assert.ThrowsException(Of ArgumentException)(Sub() DumbbellPlotCore.Compute(
            New String() {"A", "B"}, New Double() {1}, New Double() {2, 3}))
        Assert.ThrowsException(Of ArgumentException)(Sub() DumbbellPlotCore.Compute(
            Array.Empty(Of String)(), Array.Empty(Of Double)(), Array.Empty(Of Double)()))
        Assert.ThrowsException(Of ArgumentException)(Sub() DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A"}, New Double() {1}, New Double() {2},
            New String() {"A", "A"}, New Double() {2}))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A"}, New Double() {1}, New Double() {2}, Nothing, New Double() {2}))
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub MatricesWithBothDimensionsGreaterThanOne_AreRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() DumbbellPlotCore.Compute(
            New String(,) {{"A", "B"}, {"C", "D"}},
            New Double() {1, 2, 3, 4}, New Double() {5, 6, 7, 8}))
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub InvalidSortAndJitterOptions_AreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Basic(
            New DumbbellPlotOptions With {.SortMode = CType(999, DumbbellPlotSortMode)}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Basic(
            New DumbbellPlotOptions With {.AuxiliaryJitterFraction = 0.46R}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Basic(
            New DumbbellPlotOptions With {.AuxiliaryJitterFraction = Double.NaN}))
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub InfiniteAndBooleanEndpoints_AreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() DumbbellPlotCore.Compute(
            New String() {"A"}, New Object() {Double.PositiveInfinity}, New Double() {2}))
        Assert.ThrowsException(Of ArgumentException)(Sub() DumbbellPlotCore.Compute(
            New String() {"A"}, New Object() {True}, New Double() {2}))
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub NumericTextAndDecimalEndpoints_AreSupported()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.Compute(
            New String() {"A"}, New Object() {"12.5"}, New Object() {15D})
        Assert.AreEqual(DumbbellAxisValueKind.Numeric, result.AxisValueKind)
        Near(12.5R, result.Categories(0).FirstValue)
        Near(15.0R, result.Categories(0).SecondValue)
    End Sub

    <TestMethod>
    <TestCategory("Dumbbell-Core")>
    Public Sub EmptyAuxiliaryVectors_ArePermitted()
        Dim result As DumbbellPlotResult = DumbbellPlotCore.ComputeWithAuxiliary(
            New String() {"A"}, New Double() {1}, New Double() {3},
            Array.Empty(Of Object)(), Array.Empty(Of Object)())
        Assert.AreEqual(0, result.SourceAuxiliaryObservationCount)
        Assert.AreEqual(0, result.UsableAuxiliaryObservationCount)
        Assert.IsFalse(result.HasAuxiliaryObservations)
    End Sub
End Class
