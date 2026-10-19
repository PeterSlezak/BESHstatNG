Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class OutputLayoutPlannerTests
    Public Property TestContext As TestContext

    <TestMethod>
    Public Sub C6FourTables_HaveExactRectanglesGapsOffsetsAndRepeatablePreview()
        Dim document As OutputDocument = FourTables()
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(document, C6())
        Assert.AreEqual(4, plan.Placements.Count)
        Dim expected As Integer(,) = {{6, 3, 7, 5}, {9, 3, 11, 4}, {13, 3, 13, 6}, {15, 3, 16, 3}}
        For i As Integer = 0 To 3
            Dim block As OutputBlockPlacement = plan.Placements(i)
            Assert.AreSame(document.Artifacts(i), block.Artifact)
            Assert.AreEqual(i, block.Index)
            Assert.AreEqual("table-" & ChrW(97 + i), block.ArtifactId)
            Assert.AreEqual(OutputArtifactKind.Table, block.Kind)
            AssertRectangle(block.Bounds, expected(i, 0), expected(i, 1), expected(i, 2), expected(i, 3))
            Assert.AreEqual(expected(i, 0) - 6, block.RowOffset.Value)
            Assert.AreEqual(0, block.ColumnOffset.Value)
            Assert.AreEqual(If(i = 0, 0, 1), block.BlankRowsBefore)
        Next
        AssertRectangle(plan.UsedBounds, 6, 3, 16, 6)
        For Each row As Integer In New Integer() {8, 12, 14, 17}
            Assert.IsFalse(plan.Placements.Any(Function(b) b.Bounds.Contains(row, 3)))
        Next
        Assert.IsFalse(plan.Placements.Any(Function(b) b.Bounds.Contains(16, 6)), "Envelope is not a filled range.")
        AssertFixture("four-tables-c6", plan)
        Assert.AreEqual(Project(plan), Project(OutputLayoutPlanner.Create(document, C6())))
        Dim alternate As New SpreadsheetOutputTarget With {.Destination = SpreadsheetOutputDestination.NewWorksheet, .WorksheetName = "Alternate", .StartRow = 2, .StartColumn = 5}
        Dim other As OutputLayoutPlan = OutputLayoutPlanner.Create(document, alternate, New OutputLayoutOptions(2))
        CollectionAssert.AreEqual(New Integer() {2, 6, 11, 14}, other.Placements.Select(Function(b) b.Bounds.StartRow).ToArray())
        AssertRectangle(other.UsedBounds, 2, 5, 15, 8)
        Assert.AreEqual(Project(other), Project(OutputLayoutPlanner.Create(document, alternate, New OutputLayoutOptions(2))))
        TestContext.WriteLine(Project(plan))
    End Sub

    <DataTestMethod>
    <DataRow(0, 3)>
    <DataRow(1, 4)>
    <DataRow(2, 5)>
    Public Sub DefaultA1_AndExplicitGapsUseWholeBlankRows(gap As Integer, nextRow As Integer)
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(New OutputDocument({Table("a", 2, 3), Table("b", 1, 1)}), options:=New OutputLayoutOptions(gap))
        Assert.AreEqual(SpreadsheetOutputDestination.NewWorksheet, plan.Destination)
        Assert.AreEqual("BESHStat_Result", plan.WorksheetName)
        Assert.AreEqual(1, plan.StartRow)
        Assert.AreEqual(1, plan.StartColumn)
        AssertRectangle(plan.Placements(0).Bounds, 1, 1, 2, 3)
        AssertRectangle(plan.Placements(1).Bounds, nextRow, 1, nextRow, 1)
        AssertRectangle(plan.UsedBounds, 1, 1, nextRow, 3)
        Assert.AreEqual(1048576, plan.Options.MaxRows)
        Assert.AreEqual(16384, plan.Options.MaxColumns)
        Assert.AreEqual(1, New OutputLayoutOptions().BlankRowsBetweenTables)
    End Sub

    <DataTestMethod>
    <DataRow(1, 1)>
    <DataRow(1, 5)>
    <DataRow(5, 1)>
    Public Sub OneTable_UsesExactlyItsShapeWithoutTrailingGap(rows As Integer, columns As Integer)
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(New OutputDocument({Table("only", rows, columns)}), C6(), New OutputLayoutOptions(100))
        AssertRectangle(plan.UsedBounds, 6, 3, 5 + rows, 2 + columns)
        Assert.AreEqual(0, plan.Placements(0).BlankRowsBefore)
    End Sub

    <TestMethod>
    Public Sub AssembledSectionsAndSemanticCells_TranslateLocalOffsetsExactlyOnce()
        Dim model As New ResultTableOutputModel(New Object(,) {
            {"Title one", Nothing, Nothing}, {"Title two", Nothing, Nothing},
            {Nothing, "Estimate", "P"}, {Nothing, "Second", "P"},
            {"A", 2, 0.04}, {"B", 3, 0.1}, {"Foot one", Nothing, Nothing}, {"Foot two", Nothing, Nothing}},
            titleRows:=2, headerTopRows:=2, headerLeftColumns:=1, footerRows:=2, isResultTable:=False,
            pvalueColumns:=New Integer() {2}, pvalueCells:=New ResultTableCellAddress() {New ResultTableCellAddress(1, 2)})
        Dim tableArtifact As New OutputTableArtifact("sections", model, "Caption consumes no row")
        Dim document As New OutputDocument({tableArtifact})
        Dim semantics As OutputSemanticPlan = OutputSemanticPlanner.Create(document, Nothing, 0.05, "")
        Dim before As String = JsonSerializer.Serialize(semantics)
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(document, C6(), semanticPlan:=semantics)
        AssertRectangle(plan.Placements(0).Bounds, 6, 3, 13, 5)
        AssertAlignment(plan, semantics)
        Dim pvalue As OutputCellStyle = semantics.Artifacts(0).Cells.Single(Function(c) c.Row = 4 AndAlso c.Column = 2)
        Assert.IsTrue(pvalue.IsPValue)
        Assert.AreEqual(10, plan.Placements(0).Bounds.StartRow + pvalue.Row)
        Assert.AreEqual(5, plan.Placements(0).Bounds.StartColumn + pvalue.Column)
        Assert.AreEqual(0.04, pvalue.Cell.BinaryValue.Value)
        Assert.AreEqual(before, JsonSerializer.Serialize(semantics))
        Assert.IsFalse(tableArtifact.Model.IsResultTable)
        Assert.AreEqual(1, tableArtifact.Model.PvalueCells(0).BodyRow)
        Assert.AreEqual(2, tableArtifact.Model.PvalueCells(0).BodyColumn)
        Assert.AreEqual(2, tableArtifact.Model.PvalueColumns(0))
        Assert.AreEqual(2, model.TitleRows)
    End Sub

    <TestMethod>
    Public Sub MixedArtifacts_RetainMetadataOrderAndNoInventedCoordinates()
        Dim artifacts As OutputArtifact() = {Table("first", 2, 3), Warning("warn"),
            New OutputPlotArtifact("plot", "logical", "preview", "caption", "description"),
            Table("second", 1, 2), New OutputDiagnosticArtifact("fault", OutputDiagnosticSeverity.Fault, "test.fault", "Fault", "field")}
        Dim document As New OutputDocument(artifacts)
        Dim semantics As OutputSemanticPlan = OutputSemanticPlanner.Create(document, Nothing, 0.05)
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(document, C6(), semanticPlan:=semantics)
        Assert.AreEqual(5, plan.Placements.Count)
        For i As Integer = 0 To artifacts.Length - 1
            Assert.AreSame(artifacts(i), plan.Placements(i).Artifact)
            Assert.AreEqual(i, plan.Placements(i).Index)
            Assert.AreEqual(artifacts(i).Kind, plan.Placements(i).Kind)
        Next
        For Each i As Integer In New Integer() {1, 2, 4}
            AssertUnplaced(plan.Placements(i))
            Assert.AreEqual(0, plan.Placements(i).RowCount)
            Assert.AreEqual(0, plan.Placements(i).ColumnCount)
        Next
        AssertRectangle(plan.Placements(3).Bounds, 9, 3, 9, 4)
        AssertRectangle(plan.UsedBounds, 6, 3, 9, 5)
        Assert.AreEqual("logical", DirectCast(plan.Placements(2).Artifact, OutputPlotArtifact).ReferenceKey)
        Assert.AreEqual("field", DirectCast(plan.Placements(4).Artifact, OutputDiagnosticArtifact).FieldPath)
        AssertAlignment(plan, semantics)
    End Sub

    <DataTestMethod>
    <DataRow(0, 0)>
    <DataRow(0, 3)>
    <DataRow(3, 0)>
    Public Sub ZeroAreaTables_HaveNoBoundsOrSeparators(rows As Integer, columns As Integer)
        Dim empty As OutputTableArtifact = Table("empty", rows, columns)
        Dim only As OutputLayoutPlan = OutputLayoutPlanner.Create(New OutputDocument({empty}), C6())
        Assert.IsNull(only.UsedBounds)
        AssertUnplaced(only.Placements(0))
        Assert.AreEqual(rows, only.Placements(0).RowCount)
        Assert.AreEqual(columns, only.Placements(0).ColumnCount)
        Dim mixed As New OutputDocument({empty, Warning("w"), Table("a", 2, 1), Table("empty2", rows, columns),
            New OutputPlotArtifact("p", "ref", "purpose"), Table("b", 1, 1), Table("empty3", rows, columns)})
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(mixed, C6())
        AssertRectangle(plan.Placements(2).Bounds, 6, 3, 7, 3)
        Assert.AreEqual(0, plan.Placements(2).BlankRowsBefore)
        AssertRectangle(plan.Placements(5).Bounds, 9, 3, 9, 3)
        AssertRectangle(plan.UsedBounds, 6, 3, 9, 3)
        Assert.AreEqual(2, plan.Placements.Where(Function(b) b.IsPlaced).Count())
    End Sub

    <TestMethod>
    Public Sub EmptyAndNonspatialDocuments_HaveExplicitEmptyBounds()
        Dim empty As OutputLayoutPlan = OutputLayoutPlanner.Create(New OutputDocument(Array.Empty(Of OutputArtifact)()))
        Assert.IsNull(empty.UsedBounds)
        Assert.AreEqual(0, empty.Placements.Count)
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(New OutputDocument({Warning("w"), New OutputPlotArtifact("p", "ref", "purpose")}))
        Assert.IsNull(plan.UsedBounds)
        For Each block As OutputBlockPlacement In plan.Placements
            AssertUnplaced(block)
        Next
    End Sub

    <DataTestMethod>
    <DataRow(SpreadsheetOutputDestination.CurrentWorksheet)>
    <DataRow(SpreadsheetOutputDestination.NewWorksheet)>
    <DataRow(SpreadsheetOutputDestination.NewWorkbook)>
    Public Sub TargetAndReservationSnapshots_AreIsolatedAndDestinationIsOnlyIntent(destination As SpreadsheetOutputDestination)
        Dim target As SpreadsheetOutputTarget = C6()
        target.Destination = destination
        target.WorksheetName = " Requested Name "
        Dim options As New OutputLayoutOptions(2, 100, 20)
        Dim reservations As New List(Of OutputRectangle) From {New OutputRectangle(1, 1, 2, 2)}
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(FourTables(), target, options, reservations)
        Dim before As String = Project(plan)
        target.Destination = SpreadsheetOutputDestination.CurrentWorksheet
        target.WorksheetName = "Changed"
        target.StartRow = 99
        target.StartColumn = 99
        options = New OutputLayoutOptions(0)
        reservations.Clear()
        reservations.Add(New OutputRectangle(6, 3, 7, 5))
        Assert.AreEqual(before, Project(plan))
        Assert.AreEqual(destination, plan.Destination)
        Assert.AreEqual(" Requested Name ", plan.WorksheetName)
        Assert.AreEqual(2, plan.Options.BlankRowsBetweenTables)
        Assert.AreEqual(1, plan.Reservations.Count)
        AssertRectangle(plan.Reservations(0), 1, 1, 2, 2)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(plan.Placements, IList(Of OutputBlockPlacement)).Clear())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(plan.Reservations, IList(Of OutputRectangle)).Clear())
        For Each type As Type In New Type() {GetType(OutputLayoutOptions), GetType(OutputLayoutPlan), GetType(OutputRectangle), GetType(OutputBlockPlacement)}
            Assert.IsTrue(type.IsSealed)
            Assert.IsTrue(type.GetProperties().All(Function(p) Not p.CanWrite), type.Name)
        Next
    End Sub

    <DataTestMethod>
    <DataRow(Nothing)>
    <DataRow("")>
    <DataRow("  ")>
    Public Sub BlankNames_DefaultWithoutMutatingCaller(name As String)
        Dim target As SpreadsheetOutputTarget = C6()
        target.WorksheetName = name
        Assert.AreEqual("BESHStat_Result", OutputLayoutPlanner.Create(FourTables(), target).WorksheetName)
        Assert.AreEqual(name, target.WorksheetName)
    End Sub

    <DataTestMethod>
    <DataRow(0, 1)>
    <DataRow(-1, 1)>
    <DataRow(1, 0)>
    <DataRow(1, -1)>
    <DataRow(Integer.MaxValue, 1)>
    <DataRow(1, Integer.MaxValue)>
    Public Sub ExplicitInvalidStarts_FailWithoutNormalizingCaller(row As Integer, column As Integer)
        Dim target As New SpreadsheetOutputTarget With {.StartRow = row, .StartColumn = column}
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(FourTables(), target))
        Assert.AreEqual(row, target.StartRow)
        Assert.AreEqual(column, target.StartColumn)
    End Sub

    <TestMethod>
    Public Sub InvalidInputs_FailClosedEvenForEmptyDocuments()
        Dim empty As New OutputDocument(Array.Empty(Of OutputArtifact)())
        Assert.ThrowsException(Of ArgumentNullException)(Function() OutputLayoutPlanner.Create(Nothing))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(empty, New SpreadsheetOutputTarget With {.Destination = CType(99, SpreadsheetOutputDestination)}))
        Assert.ThrowsException(Of ArgumentException)(Function() OutputLayoutPlanner.Create(empty, occupiedRectangles:=New OutputRectangle() {Nothing}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(empty, occupiedRectangles:={New OutputRectangle(1, 1, 1048577, 1)}))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputLayoutOptions(-1))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputLayoutOptions(maxRows:=0))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputLayoutOptions(maxColumns:=-1))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputLayoutOptions(10, 10, 10))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputLayoutOptions(Integer.MaxValue))
    End Sub

    <DataTestMethod>
    <DataRow(2, 2, 4, 4, True, True)>
    <DataRow(3, 3, 5, 5, True, False)>
    <DataRow(1, 1, 5, 5, True, False)>
    <DataRow(3, 3, 3, 3, True, True)>
    <DataRow(4, 2, 6, 4, True, False)>
    <DataRow(2, 4, 4, 6, True, False)>
    <DataRow(4, 4, 6, 6, True, False)>
    <DataRow(5, 2, 6, 4, False, False)>
    <DataRow(2, 5, 4, 6, False, False)>
    <DataRow(5, 5, 6, 6, False, False)>
    <DataRow(1, 5, 1, 6, False, False)>
    Public Sub InclusiveGeometry_HandlesBoundaryCellsEnclosureAndDisjointAdjacency(r1 As Integer, c1 As Integer, r2 As Integer, c2 As Integer, intersects As Boolean, contains As Boolean)
        Dim a As New OutputRectangle(2, 2, 4, 4)
        Dim b As New OutputRectangle(r1, c1, r2, c2)
        Assert.AreEqual(intersects, a.Intersects(b))
        Assert.AreEqual(intersects, b.Intersects(a))
        Assert.AreEqual(contains, a.Contains(b))
        Assert.AreEqual(contains, a.Contains(r1, c1) AndAlso a.Contains(r2, c2))
        Assert.IsFalse(a.Contains(0, 0))
    End Sub

    <TestMethod>
    Public Sub InvalidGeometry_RejectsNonpositiveOrReversedBounds()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputRectangle(0, 1, 1, 1))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputRectangle(1, 0, 1, 1))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputRectangle(2, 1, 1, 1))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputRectangle(1, 2, 1, 1))
        Dim rectangle As New OutputRectangle(1, 1, 1, 1)
        Assert.ThrowsException(Of ArgumentNullException)(Function() rectangle.Intersects(Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(Function() rectangle.Contains(Nothing))
    End Sub

    <DataTestMethod>
    <DataRow(6, 3, "table-a", "R6C3:R7C5")>
    <DataRow(16, 3, "table-d", "R15C3:R16C3")>
    Public Sub OccupiedReservations_RejectWholePlanWithArtifactAndRegion(row As Integer, column As Integer, id As String, coordinates As String)
        Dim result As OutputLayoutPlan = Nothing
        Dim ex As InvalidOperationException = Assert.ThrowsException(Of InvalidOperationException)(Sub()
            result = OutputLayoutPlanner.Create(FourTables(), C6(), occupiedRectangles:={New OutputRectangle(row, column, row, column)})
        End Sub)
        Assert.IsNull(result)
        StringAssert.Contains(ex.Message, id)
        StringAssert.Contains(ex.Message, coordinates)
        StringAssert.Contains(ex.Message, "reservation[0]")
        StringAssert.Contains(ex.Message, New OutputRectangle(row, column, row, column).ToString())
    End Sub

    <TestMethod>
    Public Sub GapOnlyAndEnvelopeHoles_DoNotCollideAndSourceBlocksArePairwiseDisjoint()
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(FourTables(), C6(), occupiedRectangles:={
            New OutputRectangle(8, 1, 8, 10), New OutputRectangle(12, 1, 12, 10),
            New OutputRectangle(14, 1, 14, 10), New OutputRectangle(15, 4, 16, 6)})
        For i As Integer = 0 To plan.Placements.Count - 1
            For j As Integer = i + 1 To plan.Placements.Count - 1
                Assert.IsFalse(plan.Placements(i).Bounds.Intersects(plan.Placements(j).Bounds))
            Next
        Next
        Dim adjacent As OutputLayoutPlan = OutputLayoutPlanner.Create(FourTables(), C6(), New OutputLayoutOptions(0))
        Assert.IsFalse(adjacent.Placements(0).Bounds.Intersects(adjacent.Placements(1).Bounds))
    End Sub

    <TestMethod>
    Public Sub GridEdgesAndInt32Overflow_UseInt64BeforeNarrowingWithoutGridAllocation()
        Dim oneCell As New OutputDocument({Table("cell", 1, 1)})
        Dim last As New SpreadsheetOutputTarget With {.StartRow = 1048576, .StartColumn = 16384}
        AssertRectangle(OutputLayoutPlanner.Create(oneCell, last).UsedBounds, 1048576, 16384, 1048576, 16384)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(New OutputDocument({Table("wide", 1, 2)}), last))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(New OutputDocument({Table("tall", 2, 1)}), last))
        Dim small As New OutputLayoutOptions(1, 16, 6)
        AssertRectangle(OutputLayoutPlanner.Create(FourTables(), C6(), small).UsedBounds, 6, 3, 16, 6)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(FourTables(), C6(), New OutputLayoutOptions(2, 16, 6)))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(oneCell, New SpreadsheetOutputTarget With {.StartRow = 17}, small))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(oneCell, occupiedRectangles:={New OutputRectangle(1, 1, 1, 7)}, options:=small))
        Dim huge As New OutputLayoutOptions(0, Integer.MaxValue, Integer.MaxValue)
        Dim corner As New SpreadsheetOutputTarget With {.StartRow = Integer.MaxValue, .StartColumn = Integer.MaxValue}
        AssertRectangle(OutputLayoutPlanner.Create(oneCell, corner, huge).UsedBounds, Integer.MaxValue, Integer.MaxValue, Integer.MaxValue, Integer.MaxValue)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(New OutputDocument({Table("wide", 1, 2)}), corner, huge))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(New OutputDocument({Table("tall", 2, 1)}), corner, huge))
        Dim two As New OutputDocument({Table("a", 1, 1), Table("b", 1, 1)})
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(two, options:=New OutputLayoutOptions(Integer.MaxValue - 1, Integer.MaxValue, 1)))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() OutputLayoutPlanner.Create(two, corner, huge))
    End Sub

    <TestMethod>
    Public Sub OptionalSemanticPlan_RejectsCountIdentityOrderKindAndShapeMismatch()
        Dim document As OutputDocument = FourTables()
        Dim wrongDocuments As New List(Of OutputDocument) From {
            New OutputDocument({Table("table-a", 2, 3)}),
            New OutputDocument({Table("other", 2, 3), Table("table-b", 3, 2), Table("table-c", 1, 4), Table("table-d", 2, 1)}),
            New OutputDocument(document.Artifacts.Reverse()),
            New OutputDocument({Table("table-a", 1, 3), Table("table-b", 3, 2), Table("table-c", 1, 4), Table("table-d", 2, 1)}),
            New OutputDocument({Table("table-a", 2, 4), Table("table-b", 3, 2), Table("table-c", 1, 4), Table("table-d", 2, 1)}),
            New OutputDocument({Warning("table-a"), Table("table-b", 3, 2), Table("table-c", 1, 4), Table("table-d", 2, 1)})}
        For Each wrong As OutputDocument In wrongDocuments
            Dim semantics As OutputSemanticPlan = OutputSemanticPlanner.Create(wrong, Nothing, 0.05)
            Assert.ThrowsException(Of ArgumentException)(Function() OutputLayoutPlanner.Create(document, semanticPlan:=semantics))
        Next
        ' Same ID and zero dimensions still cannot substitute a diagnostic for a plot.
        Dim plotDocument As New OutputDocument({New OutputPlotArtifact("x", "ref", "purpose")})
        Dim diagnosticPlan As OutputSemanticPlan = OutputSemanticPlanner.Create(New OutputDocument({Warning("x")}), Nothing, 0.05)
        Assert.ThrowsException(Of ArgumentException)(Function() OutputLayoutPlanner.Create(plotDocument, semanticPlan:=diagnosticPlan))
    End Sub

    <DataTestMethod>
    <DataRow("anova-base")>
    <DataRow("anova-welch-tukey")>
    <DataRow("anova-all-posthoc")>
    <DataRow("kw-base")>
    <DataRow("kw-dunn")>
    <DataRow("generic-mixed")>
    Public Sub AcceptedGoldenDocuments_KeepSemanticMetadataAndLegacyFactoryWhileAddingLayout(name As String)
        Dim document As OutputDocument = If(name = "generic-mixed", GenericDocument(), LoadAnalysis(name))
        Dim semantics As OutputSemanticPlan = OutputSemanticPlanner.Create(document, Nothing, 0.05, "")
        Dim before As String = JsonSerializer.Serialize(semantics)
        Dim models As ResultTableOutputModel() = document.Artifacts.OfType(Of OutputTableArtifact)().Select(Function(t) t.Model).ToArray()
        Dim target As SpreadsheetOutputTarget = C6()
        Dim requests As List(Of ResultTableWriteRequest) = ResultTableWriteRequestFactory.CreateMany(models, Nothing, 0.05, target)
        Dim requestsBefore As String = JsonSerializer.Serialize(requests)
        Dim plan As OutputLayoutPlan = OutputLayoutPlanner.Create(document, target, semanticPlan:=semantics)
        AssertFixture(name, plan)
        AssertAlignment(plan, semantics)
        Assert.AreEqual(before, JsonSerializer.Serialize(semantics))
        Assert.AreEqual(requestsBefore, JsonSerializer.Serialize(ResultTableWriteRequestFactory.CreateMany(models, Nothing, 0.05, target)))
        Assert.IsTrue(requests.All(Function(r) r.OutputTarget.StartRow = 6 AndAlso r.OutputTarget.StartColumn = 3), "CreateMany still owns no spacing.")
        Assert.AreEqual(6, target.StartRow)
        Assert.AreEqual(3, target.StartColumn)
        Using fixture As JsonDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "OutputSemantics", name & ".json")))
            Dim expected As JsonElement() = fixture.RootElement.GetProperty("artifacts").EnumerateArray().ToArray()
            Assert.AreEqual(expected.Length, plan.Placements.Count)
            For i As Integer = 0 To expected.Length - 1
                Assert.AreEqual(expected(i).GetProperty("id").GetString(), plan.Placements(i).ArtifactId)
                Assert.AreEqual(expected(i).GetProperty("rows").GetInt32(), plan.Placements(i).RowCount)
                Assert.AreEqual(expected(i).GetProperty("columns").GetInt32(), plan.Placements(i).ColumnCount)
            Next
        End Using
        Dim oldCulture As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sk-SK")
            Assert.AreEqual(Project(plan), Project(OutputLayoutPlanner.Create(document, target)))
        Finally
            CultureInfo.CurrentCulture = oldCulture
        End Try
    End Sub

    Private Shared Function Table(id As String, rows As Integer, columns As Integer) As OutputTableArtifact
        Dim values As Object(,) = DirectCast(Array.CreateInstance(GetType(Object), rows, columns), Object(,))
        Return New OutputTableArtifact(id, New ResultTableOutputModel(values))
    End Function

    Private Shared Function Warning(id As String) As OutputDiagnosticArtifact
        Return New OutputDiagnosticArtifact(id, OutputDiagnosticSeverity.Warning, "test.warning", "Warning")
    End Function

    Private Shared Function FourTables() As OutputDocument
        Return New OutputDocument({Table("table-a", 2, 3), Table("table-b", 3, 2), Table("table-c", 1, 4), Table("table-d", 2, 1)})
    End Function

    Private Shared Function C6() As SpreadsheetOutputTarget
        Return New SpreadsheetOutputTarget With {.Destination = SpreadsheetOutputDestination.CurrentWorksheet, .StartRow = 6, .StartColumn = 3}
    End Function

    Private Shared Sub AssertRectangle(bounds As OutputRectangle, row As Integer, column As Integer, endRow As Integer, endColumn As Integer)
        Assert.IsNotNull(bounds)
        Assert.AreEqual(row, bounds.StartRow)
        Assert.AreEqual(column, bounds.StartColumn)
        Assert.AreEqual(endRow, bounds.EndRow)
        Assert.AreEqual(endColumn, bounds.EndColumn)
    End Sub

    Private Shared Sub AssertUnplaced(block As OutputBlockPlacement)
        Assert.IsFalse(block.IsPlaced)
        Assert.IsNull(block.Bounds)
        Assert.IsFalse(block.RowOffset.HasValue)
        Assert.IsFalse(block.ColumnOffset.HasValue)
        Assert.AreEqual(0, block.BlankRowsBefore)
    End Sub

    Private Shared Sub AssertAlignment(layout As OutputLayoutPlan, semantics As OutputSemanticPlan)
        For i As Integer = 0 To layout.Placements.Count - 1
            Dim block As OutputBlockPlacement = layout.Placements(i)
            Dim styled As OutputArtifactStylePlan = semantics.Artifacts(i)
            Assert.AreEqual(styled.ArtifactId, block.ArtifactId)
            Assert.AreEqual(styled.Kind, block.Kind)
            Assert.AreEqual(styled.RowCount, block.RowCount)
            Assert.AreEqual(styled.ColumnCount, block.ColumnCount)
            For Each cell As OutputCellStyle In styled.Cells
                Assert.IsTrue(block.Bounds.Contains(block.Bounds.StartRow + cell.Row, block.Bounds.StartColumn + cell.Column))
                Assert.AreEqual(block.ArtifactId, cell.ArtifactId)
            Next
        Next
    End Sub

    ' Test-only scalar projection, not a production transport format. Stable property/order/culture.
    Private Shared Function Project(plan As OutputLayoutPlan) As String
        Dim lines As New List(Of String) From {
            String.Format(CultureInfo.InvariantCulture, "scope={0}|{1}|R{2}C{3};gap={4};grid={5}x{6};envelope={7}",
                plan.Destination, plan.WorksheetName, plan.StartRow, plan.StartColumn, plan.Options.BlankRowsBetweenTables,
                plan.Options.MaxRows, plan.Options.MaxColumns, If(plan.UsedBounds Is Nothing, "none", plan.UsedBounds.ToString()))}
        For Each b As OutputBlockPlacement In plan.Placements
            lines.Add(String.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}|{3}x{4}|placed={5}|offset={6},{7}|gap={8}|{9}",
                b.Index, b.ArtifactId, b.Kind, b.RowCount, b.ColumnCount, If(b.IsPlaced, "true", "false"),
                If(b.RowOffset.HasValue, b.RowOffset.Value.ToString(CultureInfo.InvariantCulture), "none"),
                If(b.ColumnOffset.HasValue, b.ColumnOffset.Value.ToString(CultureInfo.InvariantCulture), "none"),
                b.BlankRowsBefore, If(b.Bounds Is Nothing, "none", b.Bounds.ToString())))
        Next
        Return String.Join(vbLf, lines)
    End Function

    Private Sub AssertFixture(name As String, plan As OutputLayoutPlan)
        Dim actual As String = Project(plan)
        Dim path As String = IO.Path.Combine(AppContext.BaseDirectory, "TestData", "OutputLayout", name & ".txt")
        TestContext.WriteLine(actual)
        Assert.AreEqual(File.ReadAllText(path).Replace(vbCrLf, vbLf).TrimEnd(), actual, name)
    End Sub

    Private Shared Function GenericDocument() As OutputDocument
        ' Same source as the accepted 02.02 generic-mixed fixture; raw scalar meanings unchanged.
        Return New OutputDocument(New OutputArtifact() {
            New OutputTableArtifact("generic-a", New ResultTableOutputModel(New Object(,) {
                {"Title", Nothing, Nothing}, {Nothing, "Statistic", "P"}, {"A", 9007199254740993L, 0.05},
                {"B", Double.NaN, "0.01"}, {"Footnote", Nothing, Nothing}}, titleRows:=1, headerTopRows:=1,
                headerLeftColumns:=1, footerRows:=1, pvalueColumns:=New Integer() {2, 2, 0},
                pvalueCells:=New ResultTableCellAddress() {New ResultTableCellAddress(1, 2), New ResultTableCellAddress(99, 1)})),
            New OutputDiagnosticArtifact("warning", OutputDiagnosticSeverity.Warning, "preview.warning", "Warning"),
            New OutputTableArtifact("generic-b", New ResultTableOutputModel(New Object(,) {{Nothing, "", "#N/A", "=1+1", False, -0.0R, 0.0001D}})),
            New OutputDiagnosticArtifact("fault", OutputDiagnosticSeverity.Fault, "preview.fault", "Fault"),
            New OutputPlotArtifact("plot", "logical-reference", "Preview only")})
    End Function

    Private Shared Function LoadAnalysis(name As String) As OutputDocument
        Using fixture As JsonDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "Stage304", name & ".json")))
            Dim root As JsonElement = fixture.RootElement
            Dim names As String() = root.GetProperty("groups").GetProperty("names").EnumerateArray().Select(Function(e) e.GetString()).ToArray()
            Dim groups As Double()() = root.GetProperty("groups").GetProperty("values").EnumerateArray().Select(
                Function(g) g.EnumerateArray().Select(Function(e) e.GetDouble()).ToArray()).ToArray()
            Dim raw As JsonElement = root.GetProperty("options")
            Dim options As SpreadsheetAnalysisOptions
            Dim kind As SpreadsheetAnalysisKind = CType(root.GetProperty("legacyEnumValue").GetInt32(), SpreadsheetAnalysisKind)
            If kind = SpreadsheetAnalysisKind.OneWayAnova Then
                options = New OneWayAnovaAnalysisOptions With {.IncludeWelch = Flag(raw, "welch"), .IncludeFisherLsd = Flag(raw, "fisherLsd"),
                    .IncludeBonferroni = Flag(raw, "bonferroni"), .IncludeTukeyKramer = Flag(raw, "tukeyKramer"), .IncludeGamesHowell = Flag(raw, "gamesHowell")}
            Else
                options = New KruskalWallisAnalysisOptions With {.IncludeDunnComparisons = Flag(raw, "dunn")}
            End If
            Return OutputDocument.FromResult(SharedAnalysisApplicationService.Run(New SpreadsheetAnalysisRequest(kind, New GroupedNumericData(names, groups), options)))
        End Using
    End Function

    Private Shared Function Flag(options As JsonElement, name As String) As Boolean
        Dim value As JsonElement
        Return options.TryGetProperty(name, value) AndAlso value.GetBoolean()
    End Function
End Class
