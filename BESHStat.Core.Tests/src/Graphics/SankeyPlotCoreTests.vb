Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Portable Sankey computation and layout regression tests; no Excel dependency.</summary>
<TestClass>
Public Class SankeyPlotCoreTests
    Private Shared Sub Near(expected As Double, actual As Double, Optional tolerance As Double = 0.0000000001R)
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

    Private Shared Function Simple() As SankeyPlotResult
        Return SankeyPlotCore.Compute(New String() {"A", "A"}, New String() {"B", "B"})
    End Function

    <TestMethod>
    Public Sub DefaultWeights_AggregateEqualLinks()
        Dim result As SankeyPlotResult = Simple()
        Assert.AreEqual(2, result.SourceObservationCount)
        Assert.AreEqual(2, result.UsableObservationCount)
        Assert.AreEqual(0, result.ExcludedObservationCount)
        Assert.AreEqual(2, result.RawLinkCount)
        Assert.AreEqual(1, result.LinkCount)
        Assert.AreEqual(2, result.NodeCount)
        Assert.AreEqual(2, result.StageCount)
        Near(2, result.Links(0).Value)
        Near(2, result.ReferenceTotal)
        CollectionAssert.AreEqual(New String() {"", ""}, result.StageNames)
    End Sub

    <TestMethod>
    Public Sub WeightedAggregation_RetainsGraphTotals()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New String() {"A", "A", "B"}, New String() {"B", "B", "C"},
            New Double() {2, 3, 4})
        Assert.AreEqual(3, result.RawLinkCount)
        Assert.AreEqual(2, result.LinkCount)
        Assert.AreEqual(3, result.StageCount)
        Near(5, result.Links(0).Value)
        Near(4, result.Links(1).Value)
        Near(5, result.ReferenceTotal)
        Near(5, result.StageTotals(0))
        Near(5, result.StageTotals(1))
        Near(4, result.StageTotals(2))
    End Sub

    <TestMethod>
    Public Sub NumericTextAndTypedNumbers_ShareCategoryKey()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New Object() {"001", 1, 1.0R}, New Object() {"Target", "Target", "Target"})
        Assert.AreEqual(2, result.NodeCount)
        Assert.AreEqual(1, result.LinkCount)
        Near(3, result.Links(0).Value)
        Assert.AreEqual("N:1", result.Nodes.First(Function(n) n.StageIndex = 0).ColorKey)
    End Sub

    <TestMethod>
    Public Sub TextCategories_AreTrimmedAndKeepStableKeys()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New Object() {"  Alpha ", "Alpha"}, New String() {" Beta", "Beta "})
        Assert.AreEqual(2, result.NodeCount)
        Assert.AreEqual("Alpha", result.Nodes(0).Label)
        Assert.AreEqual("S:Alpha", result.Nodes(0).ColorKey)
        Assert.AreEqual("Beta", result.Nodes(1).Label)
    End Sub

    <TestMethod>
    Public Sub NumericCategories_CultureSpecificDecimalText()
        Dim previous As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR")
            Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
                New Object() {"1,5", 1.5R}, New Object() {"T", "T"})
            Assert.AreEqual(1, result.LinkCount)
            Assert.AreEqual("N:1.5", result.Nodes(0).ColorKey)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub BooleanAndDateCategories_AreNotConvertedToNumbers()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New Object() {True, New DateTime(2020, 1, 1)}, New String() {"A", "B"})
        Assert.AreEqual(2, result.LinkCount)
        Assert.IsTrue(result.Nodes.Any(Function(n) n.ColorKey = "S:True"))
        Assert.IsTrue(result.Nodes.Any(Function(n) n.Label.Length > 4 AndAlso n.ColorKey.StartsWith("S:", StringComparison.Ordinal)))
    End Sub

    <TestMethod>
    Public Sub MissingCategories_ZeroWeights_AndNaNWeightsAreExcluded()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New Object() {"A", Nothing, "C", "D", DBNull.Value, "E"},
            New Object() {"B", "B", "D", "E", "F", " "},
            New Double() {2, 1, 0, Double.NaN, 1, 2})
        Assert.AreEqual(6, result.SourceObservationCount)
        Assert.AreEqual(1, result.UsableObservationCount)
        Assert.AreEqual(5, result.ExcludedObservationCount)
        Assert.AreEqual(1, result.LinkCount)
        Near(2, result.ReferenceTotal)
    End Sub

    <TestMethod>
    Public Sub InferredStages_RightAlignLateSource()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New String() {"A", "B", "X"}, New String() {"B", "C", "C"})
        Assert.AreEqual(3, result.StageCount)
        Dim late As SankeyNode = result.Nodes.Single(Function(n) n.Label = "X")
        Assert.AreEqual(1, late.StageIndex)
        Near(0.5R, late.X)
        Assert.AreEqual(2, result.Nodes.Single(Function(n) n.Label = "C").StageIndex)
    End Sub

    <TestMethod>
    Public Sub ExplicitStages_NormalizeOffsetAndPreserveEmptyColumns()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New String() {"A"}, New String() {"B"},
            sourceStages:=New Integer() {4}, targetStages:=New Integer() {7},
            stageNames:=New String() {"First", "Gap1", "Gap2", "Last"})
        Assert.AreEqual(4, result.StageCount)
        Assert.AreEqual(0, result.Nodes(0).StageIndex)
        Assert.AreEqual(3, result.Nodes(1).StageIndex)
        Near(0, result.StageTotals(1))
        Near(0, result.StageTotals(2))
        Near(1, result.Nodes(1).X)
        Assert.AreEqual("Last", result.StageNames(3))
    End Sub

    <TestMethod>
    Public Sub ExplicitStages_SameLabelCanAppearInDifferentColumns()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New String() {"Repeat"}, New String() {"Repeat"},
            sourceStages:=New Integer() {0}, targetStages:=New Integer() {1})
        Assert.AreEqual(2, result.NodeCount)
        Assert.AreNotEqual(result.Nodes(0).Id, result.Nodes(1).Id)
        Assert.AreEqual(result.Nodes(0).ColorKey, result.Nodes(1).ColorKey)
    End Sub

    <TestMethod>
    Public Sub StagedData_AggregatesRepeatedTransitions()
        Dim stages As Object(,) = {{"A", "B", "C"}, {"A", "B", "D"}}
        Dim result As SankeyPlotResult = SankeyPlotCore.ComputeStaged(stages)
        Assert.AreEqual(2, result.SourceObservationCount)
        Assert.AreEqual(2, result.UsableObservationCount)
        Assert.AreEqual(4, result.RawLinkCount)
        Assert.AreEqual(3, result.LinkCount)
        Assert.AreEqual(3, result.StageCount)
        Near(2, result.ReferenceTotal)
    End Sub

    <TestMethod>
    Public Sub StagedData_MissingIntermediateBreaksFlowByDefault()
        Dim stages As Object(,) = {{"A", Nothing, "C"}, {"A", "B", "C"}}
        Dim result As SankeyPlotResult = SankeyPlotCore.ComputeStaged(stages)
        Assert.AreEqual(1, result.UsableObservationCount)
        Assert.AreEqual(1, result.ExcludedObservationCount)
        Assert.AreEqual(2, result.RawLinkCount)
    End Sub

    <TestMethod>
    Public Sub StagedData_MissingIntermediateCanBridgeStages()
        Dim stages As Object(,) = {{"A", Nothing, "C"}, {"A", "B", "C"}}
        Dim result As SankeyPlotResult = SankeyPlotCore.ComputeStaged(stages,
            options:=New SankeyPlotOptions With {.ConnectAcrossMissingStages = True})
        Assert.AreEqual(2, result.UsableObservationCount)
        Assert.AreEqual(3, result.RawLinkCount)
        Assert.IsTrue(result.Links.Any(Function(l)
                                           Dim sourceNode As SankeyNode = result.Nodes.Single(Function(n) n.Id = l.SourceNodeId)
                                           Dim targetNode As SankeyNode = result.Nodes.Single(Function(n) n.Id = l.TargetNodeId)
                                           Return sourceNode.StageIndex = 0 AndAlso targetNode.StageIndex = 2
                                       End Function))
    End Sub

    <TestMethod>
    Public Sub StagedData_LeadingMissingStageAllowed()
        Dim stages As Object(,) = {{Nothing, "A", "B"}}
        Dim result As SankeyPlotResult = SankeyPlotCore.ComputeStaged(stages)
        Assert.AreEqual(3, result.StageCount)
        Assert.AreEqual(1, result.Nodes.First().StageIndex)
        Near(0, result.StageTotals(0))
    End Sub

    <TestMethod>
    Public Sub StagedData_WeightsAreAppliedToEveryTransition()
        Dim stages As Object(,) = {{"A", "B", "C"}}
        Dim result As SankeyPlotResult = SankeyPlotCore.ComputeStaged(stages, New Double() {2.5R})
        Assert.AreEqual(2, result.LinkCount)
        For Each link As SankeyLink In result.Links
            Near(2.5R, link.Value)
        Next
        Near(2.5R, result.ReferenceTotal)
    End Sub

    <TestMethod>
    Public Sub StagedData_AllMissingTransitionsRejected()
        Dim stages As Object(,) = {{"A", Nothing, "C"}}
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.ComputeStaged(stages))
    End Sub

    <TestMethod>
    Public Sub DirectVectors_SupportNonzeroLowerBounds()
        Dim source As Array = Array.CreateInstance(GetType(Object), New Integer() {2}, New Integer() {3})
        Dim target As Array = Array.CreateInstance(GetType(Object), New Integer() {2}, New Integer() {-2})
        source.SetValue("A", 3)
        source.SetValue("A", 4)
        target.SetValue("B", -2)
        target.SetValue("B", -1)
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(source, target)
        Assert.AreEqual(1, result.LinkCount)
        Near(2, result.Links(0).Value)
    End Sub

    <TestMethod>
    Public Sub ResultArrays_AreDefensiveCopies()
        Dim result As SankeyPlotResult = Simple()
        Dim nodes As SankeyNode() = result.Nodes
        Dim links As SankeyLink() = result.Links
        Dim totals As Double() = result.StageTotals
        Dim names As String() = result.StageNames
        nodes(0) = Nothing
        links(0) = Nothing
        totals(0) = -999
        names(0) = "Modified"
        Assert.IsNotNull(result.Nodes(0))
        Assert.IsNotNull(result.Links(0))
        Near(2, result.StageTotals(0))
        Assert.AreEqual("", result.StageNames(0))
    End Sub

    <TestMethod>
    Public Sub NormalizedGeometry_NodeAndLinkBounds()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New String() {"A", "B", "A"}, New String() {"C", "C", "D"})
        For Each node As SankeyNode In result.Nodes
            Assert.IsTrue(node.X >= 0 AndAlso node.X <= 1)
            Assert.IsTrue(node.Y >= -0.0000000001R)
            Assert.IsTrue(node.Height >= 0)
            Assert.IsTrue(node.Y + node.Height <= 1.0000000001R)
        Next
        For Each link As SankeyLink In result.Links
            Dim src As SankeyNode = result.Nodes.Single(Function(n) n.Id = link.SourceNodeId)
            Dim dst As SankeyNode = result.Nodes.Single(Function(n) n.Id = link.TargetNodeId)
            Assert.IsTrue(dst.StageIndex > src.StageIndex)
            Near(link.Value * result.WeightScale, link.Thickness)
            Assert.IsTrue(link.SourceTop >= src.Y - 0.0000000001R)
            Assert.IsTrue(link.TargetTop >= dst.Y - 0.0000000001R)
            Assert.IsTrue(link.SourceTop + link.Thickness <= src.Y + src.Height + 0.0000000001R)
            Assert.IsTrue(link.TargetTop + link.Thickness <= dst.Y + dst.Height + 0.0000000001R)
        Next
    End Sub

    <TestMethod>
    Public Sub FirstAppearanceAndAlphabeticalOrder_AreDistinct()
        Dim sources As String() = {"Z", "A"}
        Dim targets As String() = {"T", "T"}
        Dim first As SankeyPlotResult = SankeyPlotCore.Compute(sources, targets,
            options:=New SankeyPlotOptions With {.NodeOrder = SankeyNodeOrderMode.FirstAppearance})
        Dim alpha As SankeyPlotResult = SankeyPlotCore.Compute(sources, targets,
            options:=New SankeyPlotOptions With {.NodeOrder = SankeyNodeOrderMode.Alphabetical})
        Assert.AreEqual("Z", first.Nodes.First(Function(n) n.StageIndex = 0).Label)
        Assert.AreEqual("A", alpha.Nodes.First(Function(n) n.StageIndex = 0).Label)
    End Sub

    <TestMethod>
    Public Sub WeightDescending_PlacesHeavierNodeFirst()
        Dim result As SankeyPlotResult = SankeyPlotCore.Compute(
            New String() {"A", "B"}, New String() {"C", "C"},
            weights:=New Double() {1, 4},
            options:=New SankeyPlotOptions With {.NodeOrder = SankeyNodeOrderMode.WeightDescending})
        Assert.AreEqual("B", result.Nodes.First(Function(n) n.StageIndex = 0).Label)
    End Sub

    <TestMethod>
    Public Sub AlignmentChoices_ChangeUnderfilledStagePosition()
        Dim source As String() = {"A", "B"}
        Dim target As String() = {"C", "C"}
        Dim topResult As SankeyPlotResult = SankeyPlotCore.Compute(source, target,
            options:=New SankeyPlotOptions With {.VerticalAlignment = SankeyVerticalAlignment.Top})
        Dim centered As SankeyPlotResult = SankeyPlotCore.Compute(source, target,
            options:=New SankeyPlotOptions With {.VerticalAlignment = SankeyVerticalAlignment.Center})
        Dim bottomResult As SankeyPlotResult = SankeyPlotCore.Compute(source, target,
            options:=New SankeyPlotOptions With {.VerticalAlignment = SankeyVerticalAlignment.Bottom})
        Dim topY As Double = topResult.Nodes.Single(Function(n) n.Label = "C").Y
        Dim centerY As Double = centered.Nodes.Single(Function(n) n.Label = "C").Y
        Dim bottomY As Double = bottomResult.Nodes.Single(Function(n) n.Label = "C").Y
        Near(0, topY)
        Assert.IsTrue(topY < centerY AndAlso centerY < bottomY)
    End Sub

    <TestMethod>
    Public Sub InvalidOptions_AreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() SankeyPlotCore.Compute(
            New String() {"A"}, New String() {"B"},
            options:=New SankeyPlotOptions With {.NodeGapFraction = 0.5R}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() SankeyPlotCore.Compute(
            New String() {"A"}, New String() {"B"},
            options:=New SankeyPlotOptions With {.CrossingReductionPasses = 0}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() SankeyPlotCore.Compute(
            New String() {"A"}, New String() {"B"},
            options:=New SankeyPlotOptions With {.NodeOrder = CType(99, SankeyNodeOrderMode)}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() SankeyPlotCore.Compute(
            New String() {"A"}, New String() {"B"},
            options:=New SankeyPlotOptions With {.VerticalAlignment = CType(99, SankeyVerticalAlignment)}))
    End Sub

    <TestMethod>
    Public Sub InvalidWeights_AndNonfiniteCategoriesAreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() SankeyPlotCore.Compute(
            New String() {"A"}, New String() {"B"}, New Double() {-1}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() SankeyPlotCore.Compute(
            New String() {"A"}, New String() {"B"}, New Double() {Double.PositiveInfinity}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(
            New Object() {Double.PositiveInfinity}, New String() {"B"}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(
            New Object() {Double.NaN}, New String() {"B"}))
    End Sub

    <TestMethod>
    Public Sub InvalidVectorShapesAndStageLengths_Throw()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() SankeyPlotCore.Compute(Nothing, New String() {"B"}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(New String() {}, New String() {}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(New String() {"A"}, New String() {"B", "C"}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(New Object(,) {{"A"}}, New String() {"B"}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(New String() {"A"}, New String() {"B"},
            sourceStages:=New Integer() {0}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(New String() {"A"}, New String() {"B"},
            sourceStages:=New Integer() {0}, targetStages:=New Integer() {0}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(New String() {"A"}, New String() {"B"},
            sourceStages:=New Integer() {0}, targetStages:=New Integer() {1}, stageNames:=New String() {"Only"}))
    End Sub

    <TestMethod>
    Public Sub CyclesAndSelfLinks_AreRejected()
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(
            New String() {"A", "B"}, New String() {"B", "A"}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.Compute(
            New String() {"A"}, New String() {"A"}))
    End Sub

    <TestMethod>
    Public Sub StageData_InvalidShapeAndWeightLengthRejected()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() SankeyPlotCore.ComputeStaged(Nothing))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.ComputeStaged(New Object(,) {{"A"}}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.ComputeStaged(
            New Object(,) {{"A", "B"}}, New Double() {1, 2}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SankeyPlotCore.ComputeStaged(
            New Object(,) {{"A", "B"}}, stageNames:=New String() {"One"}))
    End Sub
End Class
