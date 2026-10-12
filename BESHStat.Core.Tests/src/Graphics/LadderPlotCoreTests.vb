Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Direct .NET Standard Ladder Plot calculation and DTO tests; no Excel dependencies.</summary>
<TestClass>
Public Class LadderPlotCoreTests
    Private Shared Function Basic(Optional options As LadderPlotOptions = Nothing) As LadderPlotResult
        Return LadderPlotCore.Compute(New Double() {2, 5}, New Double() {7, 3}, options)
    End Function

    Private Shared Sub Near(expected As Double, actual As Double)
        Assert.AreEqual(expected, actual, 0.0000000001R)
    End Sub

    <TestMethod>
    Public Sub PairedValues_OrderAndDifferences()
        Dim result As LadderPlotResult = Basic()
        Assert.AreEqual(2, result.ObservationCount)
        Assert.AreEqual(2, result.SourceObservationCount)
        Assert.AreEqual(0, result.OmittedObservationCount)
        Near(2, result.MinimumValue)
        Near(7, result.MaximumValue)
        Near(5, result.Observations(0).Difference)
        Near(5, result.Observations(0).AbsoluteDifference)
        Near(-2, result.Observations(1).Difference)
        Assert.AreEqual(0, result.Observations(0).SourceIndex)
        Assert.AreEqual(1, result.Observations(1).DisplayIndex)
    End Sub

    <TestMethod>
    Public Sub Coordinates_WithoutJitterUseFixedColumns()
        Dim result As LadderPlotResult = Basic()
        Assert.AreEqual(1.0R, LadderPlotCore.FirstBaseXPosition)
        Assert.AreEqual(2.0R, LadderPlotCore.SecondBaseXPosition)
        For Each item As LadderPlotObservation In result.Observations
            Near(1, item.FirstXPosition)
            Near(2, item.SecondXPosition)
            Assert.IsFalse(item.IsFirstJittered)
            Assert.IsFalse(item.IsSecondJittered)
        Next
        Assert.IsFalse(result.HasHorizontalJitter)
    End Sub

    <TestMethod>
    Public Sub Labels_FallbackSourceIndexAndExplicitFlags()
        Dim result As LadderPlotResult = LadderPlotCore.ComputeWithLabels(
            New Double() {1, 2, 3}, New Double() {4, 5, 6},
            New Object() {"A", Nothing, "  "})
        Assert.IsTrue(result.RecordLabelsSupplied)
        Assert.AreEqual("A", result.Observations(0).RecordLabel)
        Assert.IsTrue(result.Observations(0).HasExplicitRecordLabel)
        Assert.AreEqual("2", result.Observations(1).RecordLabel)
        Assert.IsFalse(result.Observations(1).HasExplicitRecordLabel)
        Assert.AreEqual("3", result.Observations(2).RecordLabel)
    End Sub

    <TestMethod>
    Public Sub Groups_PreserveFirstAppearanceAndAssignIndices()
        Dim result As LadderPlotResult = LadderPlotCore.ComputeWithGroups(
            New Double() {1, 2, 3, 4}, New Double() {2, 3, 4, 5},
            New String() {"a", "b", "c", "d"}, New String() {"Z", "A", "Z", "B"})
        Assert.IsTrue(result.HasGroups)
        Assert.IsTrue(result.GroupingSupplied)
        Assert.AreEqual(3, result.GroupCount)
        Assert.AreEqual("S:Z", result.GroupLevels(0).GroupKey)
        Assert.AreEqual("A", result.GroupLevels(1).GroupLabel)
        Assert.AreEqual(0, result.Observations(2).GroupLevelIndex)
        Assert.AreEqual(2, result.Observations(3).GroupLevelIndex)
    End Sub

    <TestMethod>
    Public Sub GroupIdentifiers_DatesAndBooleansKeepTypeKeys()
        Dim day As New DateTime(2024, 1, 1)
        Dim result As LadderPlotResult = LadderPlotCore.Compute(
            New Double() {1, 2, 3}, New Double() {2, 3, 4}, Nothing,
            New Object() {day, True, "hello"})
        Assert.IsTrue(result.GroupLevels(0).GroupKey.StartsWith("D:", StringComparison.Ordinal))
        Assert.AreEqual("B:1", result.GroupLevels(1).GroupKey)
        Assert.AreEqual("S:hello", result.GroupLevels(2).GroupKey)
    End Sub

    <TestMethod>
    Public Sub NumericLikeGroupStrings_NormalizeAsNumericLegacyKeys()
        Dim result As LadderPlotResult = LadderPlotCore.Compute(
            New Double() {1, 2}, New Double() {3, 4}, Nothing,
            New Object() {1, "1"})
        Assert.AreEqual(1, result.GroupCount)
        Assert.AreEqual("N:1", result.GroupLevels(0).GroupKey)
    End Sub

    <TestMethod>
    Public Sub OmitIncompleteRows_TracksSourceAndDisplayIndices()
        Dim result As LadderPlotResult = LadderPlotCore.Compute(
            New Object() {1.0R, DBNull.Value, 3.0R},
            New Object() {2.0R, 9.0R, 6.0R}, Nothing, Nothing,
            New LadderPlotOptions With {.OmitIncompleteRows = True})
        Assert.AreEqual(3, result.SourceObservationCount)
        Assert.AreEqual(1, result.OmittedObservationCount)
        Assert.AreEqual(2, result.ObservationCount)
        Assert.AreEqual(2, result.Observations(1).SourceIndex)
        Assert.AreEqual(1, result.Observations(1).DisplayIndex)
        Assert.AreEqual("3", result.Observations(1).RecordLabel)
    End Sub

    <TestMethod>
    Public Sub MissingGroup_OmittedOnlyWhenRequested()
        Dim first As Double() = {1, 2}
        Dim second As Double() = {3, 4}
        Dim groups As Object() = {"A", Nothing}
        Assert.ThrowsException(Of ArgumentException)(
            Sub() LadderPlotCore.Compute(first, second, Nothing, groups))
        Dim result As LadderPlotResult = LadderPlotCore.Compute(first, second, Nothing, groups,
            New LadderPlotOptions With {.OmitIncompleteRows = True})
        Assert.AreEqual(1, result.ObservationCount)
        Assert.AreEqual(1, result.OmittedObservationCount)
    End Sub

    <TestMethod>
    Public Sub MissingNumeric_RejectsWithoutOmission()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() LadderPlotCore.Compute(New Object() {1, Nothing}, New Object() {2, 3}))
    End Sub

    <TestMethod>
    Public Sub MissingAllRows_RejectsEvenWithOmission()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() LadderPlotCore.Compute(New Object() {DBNull.Value}, New Object() {Nothing},
                New LadderPlotOptions With {.OmitIncompleteRows = True}))
    End Sub

    <TestMethod>
    Public Sub NumericStrings_InvariantAndCurrentCulture()
        Dim original As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR")
            Dim result As LadderPlotResult = LadderPlotCore.Compute(
                New Object() {"1,5", "2.25"}, New Object() {"3,5", "4.75"})
            Near(1.5R, result.Observations(0).FirstValue)
            Near(3.5R, result.Observations(0).SecondValue)
            Near(2.25R, result.Observations(1).FirstValue)
            Near(4.75R, result.Observations(1).SecondValue)
        Finally
            CultureInfo.CurrentCulture = original
        End Try
    End Sub

    <TestMethod>
    Public Sub NumericPrimitiveTypes_AreAccepted()
        Dim result As LadderPlotResult = LadderPlotCore.Compute(
            New Object() {CByte(2), 3D, 4.0F},
            New Object() {CShort(5), 6L, 7.0R})
        Near(2, result.MinimumValue)
        Near(7, result.MaximumValue)
        Near(3, result.Observations(0).Difference)
    End Sub

    <TestMethod>
    Public Sub BooleanAndNonNumericValues_AreRejected()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() LadderPlotCore.Compute(New Object() {True}, New Double() {1}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() LadderPlotCore.Compute(New Object() {"oops"}, New Double() {1}))
    End Sub

    <TestMethod>
    Public Sub InfiniteValues_AreRejectedAndNaNOmitted()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() LadderPlotCore.Compute(New Double() {Double.PositiveInfinity}, New Double() {1}))
        Dim result As LadderPlotResult = LadderPlotCore.Compute(
            New Double() {Double.NaN, 2}, New Double() {1, 4},
            New LadderPlotOptions With {.OmitIncompleteRows = True})
        Assert.AreEqual(1, result.ObservationCount)
        Assert.AreEqual(1, result.OmittedObservationCount)
    End Sub

    <TestMethod>
    Public Sub VectorShapes_HandleSingleColumnWithNonzeroBounds()
        Dim first As Array = Array.CreateInstance(GetType(Object),
            New Integer() {2, 1}, New Integer() {5, -1})
        first.SetValue(1, 5, -1)
        first.SetValue(2, 6, -1)
        Dim second As Array = Array.CreateInstance(GetType(Object),
            New Integer() {2}, New Integer() {-3})
        second.SetValue(4, -3)
        second.SetValue(5, -2)
        Dim result As LadderPlotResult = LadderPlotCore.Compute(first, second)
        Assert.AreEqual(2, result.ObservationCount)
        Near(3, result.Observations(0).Difference)
    End Sub

    <TestMethod>
    Public Sub VectorShapes_HandleSingleRowWithNonzeroBounds()
        Dim first As Array = Array.CreateInstance(GetType(Object),
            New Integer() {1, 2}, New Integer() {-1, 7})
        first.SetValue(1, -1, 7)
        first.SetValue(2, -1, 8)
        Dim result As LadderPlotResult = LadderPlotCore.Compute(first, New Double() {4, 6})
        Assert.AreEqual(2, result.ObservationCount)
        Near(4, result.Observations(1).Difference)
    End Sub

    <TestMethod>
    Public Sub VectorShapes_RejectMatrixAndMismatchedLengths()
        Dim matrix As Object(,) = {{1, 2}, {3, 4}}
        Assert.ThrowsException(Of ArgumentException)(
            Sub() LadderPlotCore.Compute(matrix, New Double() {1, 2, 3, 4}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() LadderPlotCore.Compute(New Double() {1, 2}, New Double() {3}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() LadderPlotCore.ComputeWithLabels(New Double() {1}, New Double() {2}, New String() {"A", "B"}))
    End Sub

    <TestMethod>
    Public Sub EmptyAndNullInputs_Throw()
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() LadderPlotCore.Compute(Nothing, New Double() {1}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() LadderPlotCore.Compute(Array.Empty(Of Double)(), Array.Empty(Of Double)()))
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() LadderPlotCore.ComputeWithGroups(New Double() {1}, New Double() {2}, Nothing, Nothing))
    End Sub

    <TestMethod>
    Public Sub FirstSideCollision_JittersFirstOnly()
        Dim result As LadderPlotResult = LadderPlotCore.Compute(
            New Double() {1, 1, 3}, New Double() {3, 4, 5},
            New LadderPlotOptions With {.HorizontalJitterFraction = 0.1R})
        Assert.AreEqual(1, result.FirstCollisionGroupCount)
        Assert.AreEqual(2, result.FirstCollisionPointCount)
        Assert.AreEqual(2, result.FirstJitteredPointCount)
        Assert.AreEqual(0, result.SecondCollisionGroupCount)
        Near(0.9R, result.Observations(0).FirstXPosition)
        Near(1.1R, result.Observations(1).FirstXPosition)
        Near(1.0R, result.Observations(2).FirstXPosition)
        Near(2.0R, result.Observations(0).SecondXPosition)
    End Sub

    <TestMethod>
    Public Sub SecondSideCollision_JittersSecondOnly()
        Dim result As LadderPlotResult = LadderPlotCore.Compute(
            New Double() {1, 2}, New Double() {5, 5},
            New LadderPlotOptions With {.HorizontalJitterFraction = 0.2R})
        Assert.AreEqual(0, result.FirstCollisionGroupCount)
        Assert.AreEqual(1, result.SecondCollisionGroupCount)
        Near(1.8R, result.Observations(0).SecondXPosition)
        Near(2.2R, result.Observations(1).SecondXPosition)
    End Sub

    <TestMethod>
    Public Sub CollisionWithoutJitter_StillTracksDiagnostics()
        Dim result As LadderPlotResult = LadderPlotCore.Compute(
            New Double() {1, 1}, New Double() {2, 3})
        Assert.IsTrue(result.HasCollisions)
        Assert.IsFalse(result.HasHorizontalJitter)
        Assert.AreEqual(2, result.FirstCollisionPointCount)
        Assert.AreEqual(0, result.FirstJitteredPointCount)
    End Sub

    <TestMethod>
    Public Sub Tolerance_MergesNearbyValuesAndKeepsOtherSideIndependent()
        Dim result As LadderPlotResult = LadderPlotCore.Compute(
            New Double() {1, 1.005R, 2}, New Double() {3, 4, 5},
            New LadderPlotOptions With {.CollisionTolerance = 0.01R, .HorizontalJitterFraction = 0.1R})
        Assert.AreEqual(1, result.FirstCollisionGroupCount)
        Assert.AreEqual(0, result.SecondCollisionGroupCount)
    End Sub

    <TestMethod>
    Public Sub ThreeWayJitter_HasUnmovedCenterAndDeterministicOffsets()
        Dim options As New LadderPlotOptions With {.HorizontalJitterFraction = 0.12R}
        Dim first As Double() = {4, 4, 4}
        Dim second As Double() = {8, 9, 10}
        Dim a As LadderPlotResult = LadderPlotCore.Compute(first, second, options)
        Dim b As LadderPlotResult = LadderPlotCore.Compute(first, second, options)
        Near(-0.12R, a.Observations(0).FirstJitterOffset)
        Near(0, a.Observations(1).FirstJitterOffset)
        Near(0.12R, a.Observations(2).FirstJitterOffset)
        Assert.AreEqual(2, a.FirstJitteredPointCount)
        For i As Integer = 0 To 2
            Near(a.Observations(i).FirstJitterOffset, b.Observations(i).FirstJitterOffset)
        Next
    End Sub

    <TestMethod>
    Public Sub Options_Validation()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() LadderPlotCore.Compute(New Double() {1}, New Double() {2},
                New LadderPlotOptions With {.HorizontalJitterFraction = 0.26R}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() LadderPlotCore.Compute(New Double() {1}, New Double() {2},
                New LadderPlotOptions With {.CollisionTolerance = Double.NaN}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() LadderPlotCore.Compute(New Double() {1}, New Double() {2},
                New LadderPlotOptions With {.CollisionTolerance = -1}))
    End Sub

    <TestMethod>
    Public Sub ResultCollections_ReturnClones()
        Dim result As LadderPlotResult = Basic()
        Dim original As LadderPlotObservation = result.Observations(0)
        Dim copied As LadderPlotObservation() = result.Observations
        copied(0) = Nothing
        Assert.AreSame(original, result.Observations(0))
        Dim groups As LadderPlotResult = LadderPlotCore.Compute(
            New Double() {1}, New Double() {2}, Nothing, New String() {"A"})
        Dim copiedGroups As LadderPlotGroupLevel() = groups.GroupLevels
        copiedGroups(0) = Nothing
        Assert.IsNotNull(groups.GroupLevels(0))
    End Sub

    <TestMethod>
    Public Sub ResultOptions_AreSnapshotsAndReturnCopies()
        Dim options As New LadderPlotOptions With {.HorizontalJitterFraction = 0.1R}
        Dim result As LadderPlotResult = LadderPlotCore.Compute(New Double() {1}, New Double() {2}, options)
        options.HorizontalJitterFraction = 0.2R
        Near(0.1R, result.Options.HorizontalJitterFraction)
        Dim returned As LadderPlotOptions = result.Options
        returned.HorizontalJitterFraction = 0.24R
        Near(0.1R, result.Options.HorizontalJitterFraction)
    End Sub

    <TestMethod>
    Public Sub ConvenienceOverloads_RejectNullRequiredVectors()
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() LadderPlotCore.ComputeWithLabels(New Double() {1}, New Double() {2}, Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() LadderPlotCore.ComputeWithGroups(New Double() {1}, New Double() {2},
                New String() {"A"}, Nothing))
    End Sub
End Class
