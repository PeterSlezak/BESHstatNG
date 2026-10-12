Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

''' <summary>Windows-host API forwarder checks; calculation tests live in Core.Tests.</summary>
<TestClass>
Public Class SankeyPlotFacadeTests
    <TestMethod>
    <TestCategory("Sankey-Compatibility")>
    Public Sub Compute_ReturnsCoreOwnedTypesAndMatchingLayout()
        Dim sources As String() = {"A", "A", "B"}
        Dim targets As String() = {"B", "B", "C"}
        Dim weights As Double() = {2, 3, 1}
        Dim expected As SankeyPlotResult = SankeyPlotCore.Compute(sources, targets, weights)
        Dim actual As SankeyPlotResult = SankeyPlot.Compute(sources, targets, weights)
        Assert.AreEqual(expected.NodeCount, actual.NodeCount)
        Assert.AreEqual(expected.LinkCount, actual.LinkCount)
        Assert.AreEqual(expected.StageCount, actual.StageCount)
        Assert.AreEqual(expected.WeightScale, actual.WeightScale)
        For i As Integer = 0 To actual.Links.Length - 1
            Assert.AreEqual(expected.Links(i).Value, actual.Links(i).Value)
            Assert.AreEqual(expected.Links(i).SourceTop, actual.Links(i).SourceTop)
        Next
    End Sub

    <TestMethod>
    <TestCategory("Sankey-Compatibility")>
    Public Sub ComputeStaged_OptionsAndStagesForwarded()
        Dim stages As Object(,) = {{"A", Nothing, "C"}, {"A", "B", "C"}}
        Dim options As New SankeyPlotOptions With {.ConnectAcrossMissingStages = True,
                                                   .NodeOrder = SankeyNodeOrderMode.FirstAppearance}
        Dim names As String() = {"Before", "Middle", "After"}
        Dim actual As SankeyPlotResult = SankeyPlot.ComputeStaged(stages, stageNames:=names, options:=options)
        Dim expected As SankeyPlotResult = SankeyPlotCore.ComputeStaged(stages, stageNames:=names, options:=options)
        Assert.AreEqual(expected.LinkCount, actual.LinkCount)
        Assert.AreEqual(expected.RawLinkCount, actual.RawLinkCount)
        Assert.AreEqual(expected.UsableObservationCount, actual.UsableObservationCount)
        CollectionAssert.AreEqual(names, actual.StageNames)
    End Sub
End Class
