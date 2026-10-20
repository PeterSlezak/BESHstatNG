Option Explicit On
Option Strict On

Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AppInfrastructure

<TestClass>
Public Class CompositeOutputPlanTests
    Private Shared Function Plan() As CompositeOutputPlan
        Return CompositeOutputExample.Create(TargetPlanner.Create(New SpreadsheetOutputTarget With {
            .Destination = SpreadsheetOutputDestination.NewWorksheet, .WorksheetName = "Composite", .StartRow = 10, .StartColumn = 3},
            HostCapabilityProfiles.WindowsExcelDna()))
    End Function

    <TestMethod>
    Public Sub CompositeRecording_UsesCanonicalOrderAndAllSixRectanglesIncludingShiftedTable()
        Dim source = Plan()
        Dim prepared = ExcelDnaCompositeOutputAdapter.Prepare(source, OutputSemanticPlanner.Create(source.SourceDocument, Nothing, 0.05))
        Dim trace As New List(Of String)()
        Dim writer As New RecordingCompositeWriter(trace)
        prepared.Execute(writer, Sub(c) trace.Add(c.Placement.Item.Kind.ToString() & "|" & c.Placement.Bounds.ToString()))
        CollectionAssert.AreEqual(New String() {"Table|R10C3:R13C8", "Diagnostic|R15C3:R15C8", "PlotPlaceholder|R17C3:R24C8",
            "Note|R26C3:R26C8", "Link|R28C3:R28C8", "Table|R30C3:R31C5"}, trace.ToArray())
        Assert.AreEqual(2, writer.Blocks.Count)
        Assert.AreEqual(4, writer.Blocks(0).Model.RowCount) : Assert.AreEqual(6, writer.Blocks(0).Model.ColumnCount)
        Assert.AreEqual(30, writer.Blocks(1).StartRow) : Assert.AreEqual(3, writer.Blocks(1).StartColumn)
        CollectionAssert.AreEqual(Enumerable.Range(0, 6).ToArray(), prepared.Commands.Select(Function(c) c.Placement.Index).ToArray())
    End Sub

    <TestMethod>
    Public Sub CompositeTablesReuseLegacyFormatterBoundaryAndOriginalMetadata()
        Dim source = Plan(), settings As New PValuePresentationSettings With {.DecimalPlaces = 5}
        Dim semantics = OutputSemanticPlanner.Create(source.SourceDocument, settings, 0.01)
        Dim prepared = ExcelDnaCompositeOutputAdapter.Prepare(source, semantics)
        Dim writer As New RecordingCompositeWriter(New List(Of String)())
        prepared.Execute(writer, Sub(c) Assert.IsNotNull(c.Text))
        Assert.AreEqual(1, writer.Blocks(0).Model.TitleRows)
        Assert.AreEqual(1, writer.Blocks(0).Model.HeaderTopRows)
        Assert.AreEqual(1, writer.Blocks(0).Model.FooterRows)
        CollectionAssert.AreEqual(New Integer() {6}, writer.Blocks(0).Model.PvalueColumns)
        Assert.AreEqual("=1+1", writer.Blocks(0).Model.Values(2, 4))
        Assert.AreEqual(GetType(Integer), writer.Blocks(0).Model.Values(2, 1).GetType())
        Assert.AreEqual(GetType(Double), writer.Blocks(0).Model.Values(2, 2).GetType())
        Assert.AreEqual(GetType(Boolean), writer.Blocks(0).Model.Values(2, 3).GetType())
        Assert.IsTrue(writer.Formats.All(Function(f) f = PValuePresentation.BuildExcelNumberFormat(settings)))
        Assert.IsTrue(writer.Alphas.All(Function(a) a = 0.01))
    End Sub

    <TestMethod>
    Public Sub TextCommandsExplicitlyDegradeLinksAndChartsAndKeepSeverityCues()
        Dim source = Plan(), prepared = ExcelDnaCompositeOutputAdapter.Prepare(source, OutputSemanticPlanner.Create(source.SourceDocument, Nothing, 0.05))
        StringAssert.StartsWith(prepared.Commands(1).Text, "WARNING [demo.warning]")
        StringAssert.Contains(prepared.Commands(1).Text, "demo.input")
        StringAssert.StartsWith(prepared.Commands(2).Text, "CHART PLACEHOLDER (not rendered)")
        StringAssert.Contains(prepared.Commands(2).Text, "Logical reference: demo-distribution")
        Assert.AreEqual("linked-text-only", prepared.Commands(4).Placement.State)
        Assert.AreEqual("Example report [link not active: https://example.org/report]", prepared.Commands(4).Text)
    End Sub

    <TestMethod>
    Public Sub PreparedCommandSnapshotsAndCollectionAreImmutable()
        Dim source = Plan(), prepared = ExcelDnaCompositeOutputAdapter.Prepare(source, OutputSemanticPlanner.Create(source.SourceDocument, Nothing, 0.05))
        prepared.Commands(0).Model.Values(0, 0) = "mutated"
        prepared.Commands(0).Model.PvalueColumns.Clear()
        Assert.AreEqual("Composite demo", prepared.Commands(0).Model.Values(0, 0))
        Assert.AreEqual(1, prepared.Commands(0).Model.PvalueColumns.Count)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(prepared.Commands, IList(Of ExcelDnaCompositeCommand)).Clear())
    End Sub

    <TestMethod>
    Public Sub StaleSemanticsAndCurrentSheetAreRejectedBeforeExecution()
        Dim source = Plan(), other = Plan()
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaCompositeOutputAdapter.Prepare(source, OutputSemanticPlanner.Create(other.SourceDocument, Nothing, 0.05)))
        Dim current = CompositeOutputExample.Create(TargetPlanner.Create(New SpreadsheetOutputTarget With {
            .Destination = SpreadsheetOutputDestination.CurrentWorksheet}, HostCapabilityProfiles.WindowsExcelDna()))
        Assert.ThrowsException(Of ArgumentException)(Function() ExcelDnaCompositeOutputAdapter.Prepare(current, OutputSemanticPlanner.Create(current.SourceDocument, Nothing, 0.05)))
    End Sub

    <TestMethod>
    Public Sub NonspatialPlotRemainsAnOrderedCommandButDoesNotCallTheInsertionSink()
        Dim plot As New OutputPlotArtifact("plot", "opaque", "purpose")
        Dim doc As New OutputDocument({plot})
        Dim plan = CompositeOutputPlanner.Create("id", doc, {CompositeOutputItem.FromArtifact(plot)}, CompositeOutputPlanTests.Plan().Target)
        Dim prepared = ExcelDnaCompositeOutputAdapter.Prepare(plan, OutputSemanticPlanner.Create(doc, Nothing, 0.05))
        Assert.AreEqual(1, prepared.Commands.Count)
        Assert.AreEqual("nonspatial", prepared.Commands(0).Placement.State)
        Dim writer As New RecordingCompositeWriter(New List(Of String)())
        prepared.Execute(writer, Sub(c) Assert.Fail("Nonspatial plot reached text sink."))
        Assert.AreEqual(0, writer.Blocks.Count)
    End Sub

    <TestMethod>
    Public Sub SinkFailurePropagatesWithEarlierTableRetained_NoRollbackClaim()
        Dim source = Plan(), prepared = ExcelDnaCompositeOutputAdapter.Prepare(source, OutputSemanticPlanner.Create(source.SourceDocument, Nothing, 0.05))
        Dim writer As New RecordingCompositeWriter(New List(Of String)())
        Assert.ThrowsException(Of InvalidOperationException)(Sub() prepared.Execute(writer, Sub(c) Throw New InvalidOperationException("sink failed")))
        Assert.AreEqual(1, writer.Blocks.Count)
    End Sub

    Private Class RecordingCompositeWriter
        Inherits ExcelDnaResultWriter
        Private ReadOnly trace As List(Of String)
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)()
        Public ReadOnly Formats As New List(Of String)()
        Public ReadOnly Alphas As New List(Of Double)()
        Public Sub New(trace As List(Of String))
            Me.trace = trace
        End Sub
        Friend Overrides Sub WriteOutputBlockCore(block As ResultTableOutputBlock, format As String, alpha As Double?,
                                                  autoFit As Boolean, Optional bounds As OutputRectangle = Nothing)
            Assert.IsFalse(autoFit) : Assert.IsNotNull(bounds)
            Assert.AreEqual(block.StartRow, bounds.StartRow) : Assert.AreEqual(block.EndRow, bounds.EndRow)
            trace.Add("Table|" & bounds.ToString())
            Blocks.Add(block) : Formats.Add(format) : Alphas.Add(alpha.Value)
        End Sub
    End Class
End Class
