Option Explicit On
Option Strict On

Imports System.IO
Imports System.Text.Json
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class CompositeOutputTests
    Private Shared ReadOnly WireOptions As New JsonSerializerOptions With {.PropertyNamingPolicy = JsonNamingPolicy.CamelCase, .WriteIndented = True}
    Private Shared Function Decision(Optional row As Integer = 10, Optional column As Integer = 3,
                                     Optional destination As SpreadsheetOutputDestination = SpreadsheetOutputDestination.NewWorksheet) As OutputTargetDecision
        Return TargetPlanner.Create(New SpreadsheetOutputTarget With {.Destination = destination,
            .WorksheetName = "Composite", .StartRow = row, .StartColumn = column}, HostCapabilityProfiles.OfficeJsExcel("OfficeOnline"))
    End Function
    Private Shared Function Example() As CompositeOutputPlan
        Return CompositeOutputExample.Create(Decision())
    End Function
    Private Shared Function Replan(plan As CompositeOutputPlan, items As IEnumerable(Of CompositeOutputItem),
                                   Optional reservations As IEnumerable(Of OutputRectangle) = Nothing,
                                   Optional options As OutputLayoutOptions = Nothing) As CompositeOutputPlan
        Return CompositeOutputPlanner.Create(plan.DocumentId, plan.SourceDocument, items, plan.Target, options, reservations)
    End Function

    <TestMethod>
    Public Sub ExactC10_AllKindsAndSubsequentTableShift_EnvelopeAndOwnedCells()
        Dim plan = Example()
        CollectionAssert.AreEqual(New String() {"R10C3:R13C8", "R15C3:R15C8", "R17C3:R24C8", "R26C3:R26C8", "R28C3:R28C8", "R30C3:R31C5"},
            plan.Items.Select(Function(p) p.Bounds.ToString()).ToArray())
        CollectionAssert.AreEqual(New Integer() {1, 2, 3, 4, 5, 1}, plan.Items.Select(Function(p) CInt(p.Item.Kind)).ToArray())
        Assert.AreEqual("R10C3:R31C8", plan.UsedBounds.ToString())
        Assert.AreEqual(6, plan.OccupiedRectangles.Count)
        Assert.AreEqual("placeholder-not-rendered", plan.Items(2).State)
        Assert.AreEqual("linked-text-only", plan.Items(4).State)
        For i As Integer = 0 To plan.Items.Count - 1
            Assert.AreEqual(i, plan.Items(i).Index)
            For j As Integer = 0 To i - 1
                Assert.IsFalse(plan.Items(i).Bounds.Intersects(plan.Items(j).Bounds))
            Next
        Next
        Assert.AreEqual("R10C3:R28C8", Replan(New CompositeOutputPlanForTest().WithoutSecond(plan),
            plan.Items.Take(5).Select(Function(p) p.Item)).UsedBounds.ToString())
    End Sub

    ' A separate source document is required when deliberately omitting the second table.
    Private Class CompositeOutputPlanForTest
        Public Function WithoutSecond(plan As CompositeOutputPlan) As CompositeOutputPlan
            Return CompositeOutputPlanner.Create(plan.DocumentId,
                New OutputDocument(plan.SourceDocument.Artifacts.Take(3)), plan.Items.Take(5).Select(Function(p) p.Item), plan.Target)
        End Function
    End Class

    <TestMethod>
    Public Sub WireFixture_IsActualDotNetProjection_WithUnchangedTableWireAndSemanticFlags()
        Dim plan = Example()
        Dim semantics = OutputSemanticPlanner.Create(plan.SourceDocument, Nothing, 0.05)
        Dim wire = OfficeCompositeOutputPlanAdapter.Prepare(plan, semantics)
        AssertWireFixture(wire, "composite-c10")
        Assert.AreEqual(1, wire.TablePlan.SchemaVersion)
        Assert.AreEqual(4, wire.TablePlan.ArtifactCount)
        Assert.AreEqual(2, wire.TablePlan.Tables.Count)
        Assert.AreEqual(3, wire.TablePlan.Tables(1).DocumentIndex)
        Assert.AreEqual("R30C3:R31C5", wire.TablePlan.Tables(1).Bounds.ToString())
        Assert.AreEqual("Warning", wire.Items(1).Severity)
        Assert.AreEqual("demo.warning", wire.Items(1).Code)
        Assert.AreEqual("demo.input", wire.Items(1).FieldPath)
        Assert.AreEqual("demo-distribution", wire.Items(2).ReferenceKey)
        Assert.AreEqual("Chart preview", wire.Items(2).Caption)
        Assert.AreEqual("main", wire.Items(3).RelatedArtifactId)
        Assert.AreEqual("https://example.org/report", wire.Items(4).LinkTarget)
        For Each table In wire.TablePlan.Tables
            Dim styled = semantics.Artifacts(table.DocumentIndex)
            For i As Integer = 0 To table.Cells.Count - 1
                Dim cell = table.Cells(i)
                Assert.AreEqual(styled.Cells(i).Role.ToString(), cell.Role)
                Assert.AreEqual(styled.Cells(i).IsPValue, cell.IsPValue)
                Assert.AreEqual(styled.Cells(i).IsSignificant, cell.IsSignificant)
                Assert.AreEqual(JsonSerializer.Serialize(OfficeOutputValue.FromCell(styled.Cells(i).Cell)), JsonSerializer.Serialize(cell.Value))
            Next
        Next
    End Sub

    Private Shared Sub AssertWireFixture(wire As OfficeCompositeOutputPlan, name As String)
        Dim json = JsonSerializer.Serialize(wire, WireOptions)
        Dim actual = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../TestResults/Codex/Batch0207"))
        Directory.CreateDirectory(actual)
        File.WriteAllText(Path.Combine(actual, name & ".json"), json & Environment.NewLine)
        Dim expected = Path.Combine(AppContext.BaseDirectory, "TestData/CompositeOutput/" & name & ".json")
        Assert.IsTrue(File.Exists(expected), "Inspect actual projection: " & actual)
        Assert.AreEqual(File.ReadAllText(expected).TrimEnd().Replace(vbCrLf, vbLf), json.Replace(vbCrLf, vbLf))
    End Sub

    <TestMethod>
    Public Sub SourceMatricesAndMetadataRemainExact_IncludingEmbeddedFooterAndBodyPvalue()
        Dim plan = Example()
        Dim source = DirectCast(plan.SourceDocument.Artifacts(0), OutputTableArtifact).Model
        Dim original = OutputLayoutPlanner.Create(plan.SourceDocument, plan.Target.ToTarget())
        Dim semantics = OutputSemanticPlanner.Create(plan.SourceDocument, Nothing, 0.05)
        Dim oldWire = OfficeOutputPlanAdapter.Prepare(plan.SourceDocument, semantics, original)
        Dim newWire = OfficeCompositeOutputPlanAdapter.Prepare(plan, semantics).TablePlan
        For t As Integer = 0 To newWire.Tables.Count - 1
            Assert.AreEqual(JsonSerializer.Serialize(oldWire.Tables(t).Cells), JsonSerializer.Serialize(newWire.Tables(t).Cells))
        Next
        Assert.AreEqual(1, source.TitleRows) : Assert.AreEqual(1, source.HeaderTopRows) : Assert.AreEqual(1, source.FooterRows)
        Assert.AreEqual(GetType(Integer), source.Values(2, 1).GetType())
        Assert.AreEqual(GetType(Double), source.Values(2, 2).GetType())
        Assert.AreEqual(GetType(Boolean), source.Values(2, 3).GetType())
        Assert.AreEqual("=1+1", source.Values(2, 4))
        Assert.IsTrue(newWire.Tables(0).Cells(17).IsPValue)
        Assert.IsTrue(newWire.Tables(0).Cells(17).IsSignificant)
        Assert.AreEqual(6, plan.Items.Count, "Embedded footer never becomes a seventh composite item.")
    End Sub

    <DataTestMethod>
    <DataRow("duplicate")> <DataRow("null")> <DataRow("missing")> <DataRow("reordered")>
    <DataRow("anchor")> <DataRow("stale")> <DataRow("id")>
    Public Sub InvalidIdentityAndReferencesFailBeforeProjection(reason As String)
        Dim plan = Example(), items = plan.Items.Select(Function(p) p.Item).ToList()
        Select Case reason
            Case "duplicate" : items.Insert(1, items(0))
            Case "null" : items(0) = Nothing
            Case "missing" : items.RemoveAt(1)
            Case "reordered" : items.Reverse()
            Case "anchor" : items(3) = CompositeOutputItem.Note("note", "text", 1, 1, "absent")
            Case "stale" : items(1) = CompositeOutputItem.FromArtifact(New OutputDiagnosticArtifact("warning", OutputDiagnosticSeverity.Warning, "x", "different"), 1, 6)
            Case "id" : items(3) = CompositeOutputItem.Note("main", "text", 1, 1)
        End Select
        Assert.ThrowsException(Of ArgumentException)(Function() Replan(plan, items))
    End Sub

    <DataTestMethod>
    <DataRow("javascript:alert(1)")> <DataRow("data:text/plain,hello")> <DataRow("file:///tmp/a")>
    <DataRow("http://example.org")> <DataRow("=HYPERLINK(x)")> <DataRow(" https://example.org")>
    <DataRow("https://user:pass@example.org")> <DataRow("https://example.org:8443/x")>
    <DataRow("https://example.org/a b")> <DataRow("https://example.org\report")>
    <DataRow("https://example.org:443/report")> <DataRow("https://.")> <DataRow("https://-bad.example/report")>
    Public Sub UnsafeLinkTargetsRejected(target As String)
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputItem.Link("link", "literal", target, 1, 6))
    End Sub

    <DataTestMethod>
    <DataRow("https://example.org/report")> <DataRow("https://example.org/report?view=summary#note")>
    Public Sub SafeLinksHaveExplicitDisplayAndInactiveIntent(target As String)
        Dim link = CompositeOutputItem.Link("link", "=literal label", target, 1, 6, "main")
        Assert.AreEqual(target, link.LinkTarget)
        Assert.AreEqual("=literal label", link.Text)
        Assert.AreEqual("main", link.RelatedArtifactId)
    End Sub

    <TestMethod>
    Public Sub TextLimitsAndDimensionsAreExplicit_NoTruncationOrFormulaCoercion()
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputItem.Note("note", "", 1, 1))
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputItem.Note("note", New String("x"c, 32768), 1, 1))
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputItem.Note("note", "x", -1, 1))
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputItem.Note("note", "x", 0, 1))
        Dim plot As New OutputPlotArtifact("plot", "javascript:opaque-logical-key", "purpose", "caption", "description")
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputItem.FromArtifact(plot, 1, 0))
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputItem.FromArtifact(plot, 0, 1))
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputItem.Link("link", "label", "https://example.org" & vbLf, 1, 6))
        Dim note = CompositeOutputItem.Note("note", "=1+1" & vbLf & "#N/A" & vbLf & "0012", 3, 6)
        Assert.AreEqual("=1+1" & vbLf & "#N/A" & vbLf & "0012", note.Text)
        StringAssert.Contains(CompositeOutputItem.FromArtifact(plot, 8, 6).Text, plot.ReferenceKey)
        Assert.AreEqual(3, [Enum].GetValues(GetType(OutputArtifactKind)).Length)
    End Sub

    <TestMethod>
    Public Sub SnapshotsDefensivelyCaptureItemsTargetsReservationsAndMatrices()
        Dim values As Object(,) = {{"old"}}, table As New OutputTableArtifact("table", New ResultTableOutputModel(values))
        Dim doc As New OutputDocument({table})
        Dim items As New List(Of CompositeOutputItem) From {CompositeOutputItem.FromArtifact(table)}
        Dim reservations As New List(Of OutputRectangle) From {New OutputRectangle(20, 1, 20, 1)}
        Dim plan = CompositeOutputPlanner.Create("id", doc, items, Decision(), reservations:=reservations)
        items.Clear() : reservations.Clear() : values(0, 0) = "changed"
        plan.Target.ToTarget().StartRow = 999
        table.Model.Values(0, 0) = "changed copy"
        Assert.AreEqual("old", table.Model.Values(0, 0))
        Assert.AreEqual(10, plan.Target.StartRow) : Assert.AreEqual(1, plan.Items.Count) : Assert.AreEqual(1, plan.Reservations.Count)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(plan.Items, IList(Of CompositeOutputPlacement)).Clear())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(plan.OccupiedRectangles, IList(Of OutputRectangle)).Clear())
    End Sub

    <TestMethod>
    Public Sub SecondFixture_NoChartOptionalLinkZeroTableAndNonspatialPlotPreserveOrderedEntries()
        Dim first As New OutputTableArtifact("first", New ResultTableOutputModel(New Object(1, 2) {}))
        Dim zero As New OutputTableArtifact("zero", New ResultTableOutputModel(New Object(-1, 2) {}))
        Dim plot As New OutputPlotArtifact("plot", "opaque", "not requested")
        Dim fault As New OutputDiagnosticArtifact("fault", OutputDiagnosticSeverity.Fault, "bad", "line one" & vbLf & "line two")
        Dim doc As New OutputDocument({first, zero, plot, fault})
        Dim items = {CompositeOutputItem.FromArtifact(first), CompositeOutputItem.FromArtifact(zero), CompositeOutputItem.FromArtifact(plot), CompositeOutputItem.FromArtifact(fault, 2, 5)}
        Dim reserved = {New OutputRectangle(12, 3, 12, 10)}
        Dim plan = CompositeOutputPlanner.Create("second", doc, items, Decision(), reservations:=reserved)
        Assert.AreEqual("R10C3:R11C5", plan.Items(0).Bounds.ToString())
        Assert.IsNull(plan.Items(1).Bounds) : Assert.IsNull(plan.Items(2).Bounds)
        Assert.AreEqual("nonspatial", plan.Items(2).State)
        Assert.AreEqual("R13C3:R14C7", plan.Items(3).Bounds.ToString())
        StringAssert.StartsWith(plan.Items(3).Item.Text, "FAULT [bad]")
        Dim wire = OfficeCompositeOutputPlanAdapter.Prepare(plan, OutputSemanticPlanner.Create(doc, Nothing, 0.05))
        Assert.AreEqual(2, wire.TablePlan.SourceTableCount) : Assert.AreEqual(1, wire.TablePlan.TableCount)
        Assert.AreEqual("nonspatial", wire.Items(2).State)
        AssertWireFixture(wire, "composite-secondary")
        Dim withLink = CompositeOutputPlanner.Create("second-link", doc, items.Concat({CompositeOutputItem.Link("link", "Example", "https://example.org/report", 1, 3)}), Decision(), reservations:=reserved)
        Assert.AreEqual("R16C3:R16C5", withLink.Items(4).Bounds.ToString())
        AssertWireFixture(OfficeCompositeOutputPlanAdapter.Prepare(withLink, OutputSemanticPlanner.Create(doc, Nothing, 0.05)), "composite-secondary-link")
    End Sub

    <DataTestMethod>
    <DataRow(14, False)> <DataRow(16, False)> <DataRow(25, False)> <DataRow(29, False)>
    <DataRow(13, True)> <DataRow(15, True)> <DataRow(17, True)> <DataRow(24, True)> <DataRow(31, True)>
    Public Sub InclusiveOccupiedEdgesAndSeparatorReservations(row As Integer, collision As Boolean)
        Dim plan = Example()
        Dim reservation = {New OutputRectangle(row, 3, row, 8)}
        If collision Then
            Assert.ThrowsException(Of InvalidOperationException)(Function() Replan(plan, plan.Items.Select(Function(p) p.Item), reservation))
        Else
            Dim result = Replan(plan, plan.Items.Select(Function(p) p.Item), reservation)
            Assert.AreEqual(plan.UsedBounds.ToString(), result.UsedBounds.ToString())
        End If
    End Sub

    <DataTestMethod>
    <DataRow(0)> <DataRow(2)> <DataRow(5)>
    Public Sub ConfigurableGapAdvancesAllVisibleKinds(gap As Integer)
        Dim plan = Example()
        Dim result = Replan(plan, plan.Items.Select(Function(p) p.Item), options:=New OutputLayoutOptions(gap))
        For i As Integer = 1 To result.Items.Count - 1
            Assert.AreEqual(result.Items(i - 1).Bounds.EndRow + 1 + gap, result.Items(i).Bounds.StartRow)
        Next
    End Sub

    <TestMethod>
    Public Sub LastCellAndOverflowPathsAreRejectedBeforeAnyAllocationOrWrite()
        Dim doc As New OutputDocument(New OutputArtifact() {})
        Dim last = Decision(1048576, 16384)
        Dim note = CompositeOutputItem.Note("note", "x", 1, 1)
        Dim plan = CompositeOutputPlanner.Create("last", doc, {note}, last)
        Assert.AreEqual("R1048576C16384:R1048576C16384", plan.UsedBounds.ToString())
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() CompositeOutputPlanner.Create("overflow", doc, {CompositeOutputItem.Note("x", "text", Integer.MaxValue, Integer.MaxValue)}, last))
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputPlanner.Create("grid", doc, {note}, last, New OutputLayoutOptions(maxRows:=Integer.MaxValue)))
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputPlanner.Create("small", doc, {note}, last, New OutputLayoutOptions(maxColumns:=16383)))
        Assert.ThrowsException(Of ArgumentException)(Function() CompositeOutputPlanner.Create("reservation", doc, {note}, last, reservations:={New OutputRectangle(1, 1, 1048577, 1)}))
    End Sub

    <TestMethod>
    Public Sub ProjectionRejectsCurrentSheetStaleSemanticsAndHugeVisibleBlocks()
        Dim plan = Example()
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeCompositeOutputPlanAdapter.Prepare(plan, OutputSemanticPlanner.Create(New OutputDocument(plan.SourceDocument.Artifacts), Nothing, 0.05)))
        Dim current = CompositeOutputExample.Create(Decision(destination:=SpreadsheetOutputDestination.CurrentWorksheet))
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeCompositeOutputPlanAdapter.Prepare(current, OutputSemanticPlanner.Create(current.SourceDocument, Nothing, 0.05)))
        Dim doc As New OutputDocument(New OutputArtifact() {})
        Dim huge = CompositeOutputPlanner.Create("huge", doc, {CompositeOutputItem.Note("note", "x", 10001, 1)}, Decision())
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeCompositeOutputPlanAdapter.Prepare(huge, OutputSemanticPlanner.Create(doc, Nothing, 0.05)))
    End Sub

    <TestMethod>
    Public Sub LeadingDiagnosticProjectsTableCoordinatesWithoutInventingATableAtTheTarget()
        Dim diagnostic As New OutputDiagnosticArtifact("warning", OutputDiagnosticSeverity.Warning, "w", "warning")
        Dim table As New OutputTableArtifact("table", New ResultTableOutputModel(New Object(0, 0) {}))
        Dim doc As New OutputDocument({diagnostic, table})
        Dim plan = CompositeOutputPlanner.Create("leading", doc, {CompositeOutputItem.FromArtifact(diagnostic, 2, 3), CompositeOutputItem.FromArtifact(table)}, Decision())
        Dim wire = OfficeCompositeOutputPlanAdapter.Prepare(plan, OutputSemanticPlanner.Create(doc, Nothing, 0.05))
        Assert.AreEqual(10, wire.Target.StartRow)
        Assert.AreEqual(13, wire.TablePlan.Target.StartRow)
        Assert.AreEqual("R13C3:R13C3", wire.TablePlan.Tables(0).Bounds.ToString())
    End Sub

    <TestMethod>
    Public Sub LargeTableTextIsBoundedBeforeHostProjection()
        Dim values(1099, 0) As Object
        For row As Integer = 0 To 1099
            values(row, 0) = New String("x"c, 32767)
        Next
        Dim table As New OutputTableArtifact("table", New ResultTableOutputModel(values))
        Dim doc As New OutputDocument({table})
        Dim plan = CompositeOutputPlanner.Create("large", doc, {CompositeOutputItem.FromArtifact(table)}, Decision())
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeCompositeOutputPlanAdapter.Prepare(plan, OutputSemanticPlanner.Create(doc, Nothing, 0.05)))
    End Sub

    <TestMethod>
    Public Sub AggregateTableAndAnnotationBytesAreBoundedEvenWhenEachPartFits()
        Dim values(989, 0) As Object
        Dim text As String = New String("x"c, 32767)
        For row As Integer = 0 To 989
            values(row, 0) = text
        Next
        Dim table As New OutputTableArtifact("table", New ResultTableOutputModel(values))
        Dim doc As New OutputDocument({table}), semantics = OutputSemanticPlanner.Create(doc, Nothing, 0.05)
        Assert.AreEqual(1, OfficeOutputPlanAdapter.Prepare(doc, semantics, OutputLayoutPlanner.Create(doc, Decision().ToTarget())).TableCount)
        Dim items As New List(Of CompositeOutputItem) From {CompositeOutputItem.FromArtifact(table)}
        For index As Integer = 0 To 19
            items.Add(CompositeOutputItem.Note("note-" & index.ToString(Globalization.CultureInfo.InvariantCulture), text, 1, 1))
        Next
        Dim plan = CompositeOutputPlanner.Create("aggregate", doc, items, Decision())
        Dim failure = Assert.ThrowsException(Of ArgumentException)(Function() OfficeCompositeOutputPlanAdapter.Prepare(plan, semantics))
        StringAssert.Contains(failure.Message, "aggregate byte limit")
    End Sub
End Class
