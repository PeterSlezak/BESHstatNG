Option Explicit On
Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon

<TestClass>
Public Class SharedOutputPlanTests
    Private Shared Function Target() As SpreadsheetOutputTarget
        Return New SpreadsheetOutputTarget With {.Destination = SpreadsheetOutputDestination.CurrentWorksheet,
            .WorksheetName = "Results", .StartRow = 6, .StartColumn = 3}
    End Function

    Private Shared Function Models() As List(Of ResultTableOutputModel)
        Return New List(Of ResultTableOutputModel) From {
            New ResultTableOutputModel(New Object(1, 2) {}), New ResultTableOutputModel(New Object(2, 1) {}),
            New ResultTableOutputModel(New Object(0, 3) {}), New ResultTableOutputModel(New Object(1, 0) {})}
    End Function

    Private Shared Function Document(models As IEnumerable(Of ResultTableOutputModel)) As OutputDocument
        Return New OutputDocument(models.Select(Function(m, i) New OutputTableArtifact("table-" & i.ToString(CultureInfo.InvariantCulture), m)))
    End Function

    Private Shared Function Requests(models As IEnumerable(Of ResultTableOutputModel), target As SpreadsheetOutputTarget) As List(Of ResultTableWriteRequest)
        Dim result = ResultTableWriteRequestFactory.CreateMany(models, New PValuePresentationSettings With {.DecimalPlaces = 5}, 0.01, target)
        For Each request In result
            request.AutoFit = False
        Next
        Return result
    End Function

    <TestMethod>
    Public Sub FourTablesC6_ExecuteExactRangesAndCursorWithoutWritingSeparators()
        Dim source = Models()
        Dim destination = Target()
        Dim plan = OutputLayoutPlanner.Create(Document(source), destination,
            occupiedRectangles:={New OutputRectangle(8, 3, 8, 6), New OutputRectangle(12, 3, 12, 6), New OutputRectangle(14, 3, 14, 6)})
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(plan, Requests(source, destination), destination, "Results")
        CollectionAssert.AreEqual(New String() {"R6C3:R7C5", "R9C3:R11C4", "R13C3:R13C6", "R15C3:R16C3"},
            prepared.Commands.Select(Function(c) c.Bounds.ToString()).ToArray())
        Assert.AreEqual("R6C3:R16C6", plan.UsedBounds.ToString())
        Dim writer As New RecordingWriter(6, 3)
        prepared.Execute(destination, "Results", Sub(c) writer.Record(c), Sub(row) writer.setRowPointer(row))
        Assert.AreEqual(4, writer.Written.Count)
        Assert.AreEqual(17, writer.RowID)
        Assert.AreEqual(3, writer.ColID)
        CollectionAssert.AreEqual(New Integer() {0, 1, 2, 3}, prepared.Commands.Select(Function(c) c.SourceOrdinal).ToArray())
    End Sub

    <TestMethod>
    Public Sub MixedArtifacts_AndZeroAreaKeepSourceOrdinalWithoutRowsOrGaps()
        Dim source = Models().Take(2).ToList()
        Dim zero As New ResultTableOutputModel(New Object(1, -1) {})
        source.Insert(1, zero)
        Dim doc As New OutputDocument({New OutputTableArtifact("table-0", source(0)),
            New OutputDiagnosticArtifact("warning", OutputDiagnosticSeverity.Warning, "w", "warning"),
            New OutputPlotArtifact("plot", "reference", "purpose"), New OutputTableArtifact("table-1", zero),
            New OutputTableArtifact("table-2", source(2)),
            New OutputDiagnosticArtifact("fault", OutputDiagnosticSeverity.Fault, "f", "fault")})
        Dim destination = Target()
        Dim plan = OutputLayoutPlanner.Create(doc, destination)
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(plan, Requests(source, destination), destination, "Results")
        Assert.AreEqual(6, plan.Placements.Count)
        Assert.AreEqual(2, prepared.Commands.Count)
        CollectionAssert.AreEqual(New Integer() {0, 2}, prepared.Commands.Select(Function(c) c.SourceOrdinal).ToArray())
        CollectionAssert.AreEqual(New Integer() {0, 4}, prepared.Commands.Select(Function(c) c.DocumentIndex).ToArray())
        CollectionAssert.AreEqual(New Integer() {6, 9}, prepared.Commands.Select(Function(c) c.Bounds.StartRow).ToArray())
    End Sub

    <DataTestMethod>
    <DataRow("count")> <DataRow("order")> <DataRow("id")> <DataRow("duplicate-id")>
    <DataRow("null")> <DataRow("shape")> <DataRow("ragged")> <DataRow("values")>
    <DataRow("type")> <DataRow("metadata")> <DataRow("annotations")> <DataRow("null-cell")>
    <DataRow("target")> <DataRow("destination")> <DataRow("sheet")> <DataRow("missing-sheet")>
    <DataRow("null-target")> <DataRow("null-values")> <DataRow("null-annotations")>
    Public Sub InvalidRequests_RefuseBeforeRecorderWrites(reason As String)
        Dim source = Models()
        source(0).Values(0, 0) = 1
        source(1).Values(0, 0) = 2
        Dim destination = Target()
        Dim plan = OutputLayoutPlanner.Create(Document(source), destination)
        Dim payload = Requests(source, destination)
        Dim ids As String() = {"table-0", "table-1", "table-2", "table-3"}
        Dim sheet As String = "Results"
        Select Case reason
            Case "count" : payload.RemoveAt(3)
            Case "order" : Dim first = payload(0) : payload(0) = payload(1) : payload(1) = first
            Case "id" : ids(0) = "wrong"
            Case "duplicate-id" : ids(1) = ids(0)
            Case "null" : payload(3) = Nothing
            Case "shape" : payload(3).Values.Add(New List(Of Object) From {Nothing})
            Case "ragged" : payload(3).Values(0).Add(Nothing)
            Case "values" : payload(0).Values(0)(0) = 999
            Case "type" : payload(0).Values(0)(0) = 1.0R
            Case "metadata" : payload(0).TitleRows = 1
            Case "annotations" : payload(0).PvalueColumns.Add(1)
            Case "null-cell" : payload(0).PvalueCells.Add(Nothing)
            Case "target" : payload(3).OutputTarget.StartRow = 7
            Case "destination" : payload(3).OutputTarget.Destination = SpreadsheetOutputDestination.NewWorkbook
            Case "sheet" : sheet = "Other"
            Case "missing-sheet" : sheet = ""
            Case "null-target" : payload(3).OutputTarget = Nothing
            Case "null-values" : payload(3).Values = Nothing
            Case "null-annotations" : payload(3).PvalueColumns = Nothing
        End Select
        Dim writes As Integer = 0
        Try
            Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(plan, payload, destination, sheet, ids)
            prepared.Execute(destination, sheet, Sub(c) writes += 1, Sub(r) Assert.Fail("Unexpected cursor change"))
            Assert.Fail("Expected validation refusal: " & reason)
        Catch ex As ArgumentException
            Assert.AreEqual(0, writes)
        End Try
    End Sub

    <DataTestMethod>
    <DataRow(0, 3)> <DataRow(-1, 3)> <DataRow(1048577, 3)>
    <DataRow(6, 0)> <DataRow(6, -1)> <DataRow(6, 16385)>
    <DataRow(1048575, 3)> <DataRow(6, 16384)>
    Public Sub InvalidGeometry_RefusesBeforeExecution(row As Integer, column As Integer)
        Dim destination = Target()
        destination.StartRow = row
        destination.StartColumn = column
        Dim writes As Integer = 0
        Try
            Dim source = Models()
            Dim plan = OutputLayoutPlanner.Create(Document(source), destination)
            ExcelDnaSharedOutputPlanAdapter.Prepare(plan, Requests(source, destination), destination, "Results").Execute(
                destination, "Results", Sub(c) writes += 1, Sub(r) Assert.Fail("Unexpected cursor change"))
            Assert.Fail("Expected bounds refusal")
        Catch ex As ArgumentException
            Assert.AreEqual(0, writes)
        End Try
    End Sub

    <TestMethod>
    Public Sub FinalRowPlan_RefusesBeforeWriteAndDoesNotOverflowCursor()
        Dim destination = Target()
        destination.StartRow = 1048576
        Dim source As New List(Of ResultTableOutputModel) From {New ResultTableOutputModel(New Object(,) {{1}})}
        Dim plan = OutputLayoutPlanner.Create(Document(source), destination)
        Assert.ThrowsException(Of ArgumentException)(Sub() ExcelDnaSharedOutputPlanAdapter.Prepare(plan, Requests(source, destination), destination, "Results"))
    End Sub

    <TestMethod>
    Public Sub StaleExecutionTarget_RefusesBeforeWritesOrCursorChanges()
        Dim source = Models()
        Dim destination = Target()
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        destination.StartRow += 1
        Assert.ThrowsException(Of ArgumentException)(Sub() prepared.Execute(destination, "Results", Sub(c) Assert.Fail("write"), Sub(r) Assert.Fail("cursor")))
    End Sub

    <DataTestMethod>
    <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub AllDestinations_PreserveResolvedIdentityAndCustomArtifactIds(destinationKind As Integer)
        Dim destination = Target()
        destination.Destination = CType(destinationKind, SpreadsheetOutputDestination)
        Dim source = Models().Take(2).ToList()
        Dim document As New OutputDocument({New OutputTableArtifact("first", source(0)), New OutputTableArtifact("second", source(1))})
        Dim plan = OutputLayoutPlanner.Create(document, destination)
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(plan, Requests(source, destination), destination, "Results", {"first", "second"})
        Assert.AreEqual(destination.Destination, prepared.Layout.Destination)
        CollectionAssert.AreEqual(New String() {"first", "second"}, prepared.Commands.Select(Function(c) c.ArtifactId).ToArray())
        Assert.AreEqual(9, prepared.Commands(1).Bounds.StartRow)
    End Sub

    <DataTestMethod>
    <DataRow(0, 0)> <DataRow(0, 3)> <DataRow(2, 0)>
    Public Sub ZeroAreaOnly_ProducesNoCommandsAndLeavesCursorUnchanged(rows As Integer, columns As Integer)
        Dim model As New ResultTableOutputModel(DirectCast(Array.CreateInstance(GetType(Object), rows, columns), Object(,)))
        Dim destination = Target()
        Dim plan = OutputLayoutPlanner.Create(OutputDocument.FromModel(model), destination)
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(plan, Requests({model}, destination), destination, "Results")
        Assert.AreEqual(0, prepared.Commands.Count)
        Assert.IsNull(plan.UsedBounds)
        prepared.Execute(destination, "Results", Sub(c) Assert.Fail("zero-area write"), Sub(r) Assert.Fail("zero-area cursor"))
    End Sub

    <TestMethod>
    Public Sub NullInputsAndClosedArtifactFamily_RefuseWithoutHostAccess()
        Dim source = Models()
        Dim destination = Target()
        Dim layout = OutputLayoutPlanner.Create(Document(source), destination)
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ExcelDnaSharedOutputPlanAdapter.Prepare(Nothing, Requests(source, destination), destination, "Results"))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ExcelDnaSharedOutputPlanAdapter.Prepare(layout, Nothing, destination, "Results"))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() ExcelDnaSharedOutputPlanAdapter.Prepare(layout, Requests(source, destination), Nothing, "Results"))
        Assert.ThrowsException(Of ArgumentException)(Sub() ExcelDnaSharedOutputPlanAdapter.Prepare(layout, Requests(source, destination), destination, "Results", {"table-0"}))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputDocument({New OutputTableArtifact("same", source(0)), New OutputTableArtifact("same", source(1))}))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputDocument(New OutputArtifact() {Nothing}))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputTableArtifact("unsupported", New ResultTableOutputModel(New Object(,) {{New Object()}})))
        ' The artifact family is closed; unknown kinds cannot enter a document through its API.
        Dim nonspatial As New OutputDocument({New OutputPlotArtifact("plot", "key", "purpose"),
            New OutputDiagnosticArtifact("warning", OutputDiagnosticSeverity.Warning, "w", "warning")})
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(nonspatial, destination),
            New List(Of ResultTableWriteRequest), destination, "Results")
        prepared.Execute(destination, "Results", Sub(c) Assert.Fail("nonspatial write"), Sub(r) Assert.Fail("nonspatial cursor"))
    End Sub

    <TestMethod>
    Public Sub LaterFailure_RecordsSuccessesAndAttemptedCursorWithoutFalseCompletion()
        Dim source = Models()
        Dim destination = Target()
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        Dim writer As New RecordingWriter(6, 3)
        Dim failure As New InvalidOperationException("second command failed")
        Dim observed = Assert.ThrowsException(Of InvalidOperationException)(Sub() prepared.Execute(destination, "Results",
            Sub(c)
                If c.SourceOrdinal = 1 Then Throw failure
                writer.Record(c)
            End Sub, Sub(row) writer.setRowPointer(row)))
        Assert.AreSame(failure, observed)
        Assert.AreEqual(1, writer.Written.Count)
        Assert.AreEqual(9, writer.RowID)
        Assert.AreEqual(3, writer.ColID)
    End Sub

    <TestMethod>
    Public Sub SingleSharedRequest_StillUsesCurrentCursorAndAdvancesOnlyHeight()
        Dim writer As New RecordingWriter(6, 3)
        Dim request = Requests(Models(), Target())(0)
        request.OutputTarget.StartRow = 100 'Standalone historical API ignores requested placement.
        writer.WriteSharedRequest(request)
        Assert.AreEqual(6, writer.Blocks(0).StartRow)
        Assert.AreEqual(3, writer.Blocks(0).StartColumn)
        Assert.AreEqual(8, writer.RowID)
        Assert.AreEqual(3, writer.ColID)
        writer.write(New Object(,) {{1}, {2}, {3}})
        Assert.AreEqual(8, writer.Blocks(1).StartRow)
        Assert.AreEqual(11, writer.RowID)
    End Sub

    <TestMethod>
    Public Sub Snapshots_PreserveScalarTypesNonfiniteNormalizationAndOverrides()
        Dim source As New List(Of ResultTableOutputModel) From {New ResultTableOutputModel(New Object(,) {
            {"0.01", 0.0123456789012345R, True, Nothing, 0.04D},
            {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, Single.NaN, "#N/A"}},
            pvalueColumns:={1, 2}, pvalueCells:={New ResultTableCellAddress(1, 2)})}
        Dim destination = Target()
        Dim payload = Requests(source, destination)
        payload(0).PvalueNumberFormat = "custom format"
        payload(0).PvalueHighlightAlpha = Double.NaN 'Host fallback remains a rendering responsibility.
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), payload, destination, "Results")
        Dim command = prepared.Commands(0)
        Assert.AreEqual("custom format", command.PvalueNumberFormat)
        Assert.IsTrue(Double.IsNaN(command.PvalueHighlightAlpha))
        Assert.IsFalse(command.AutoFit)
        AssertModelMatchesRequest(payload(0), command.Model)
        CollectionAssert.AreEqual(New Object() {"#N/A", "#Pinf", "#Ninf", "#N/A", "#N/A"},
            Enumerable.Range(0, 5).Select(Function(i) command.Model.Values(1, i)).ToArray())
        payload(0).Values(0)(0) = "changed"
        payload(0).PvalueCells(0).BodyRow = 99
        Dim copy = command.Model
        copy.Values(0, 0) = "changed copy"
        copy.PvalueColumns.Clear()
        Assert.AreEqual("0.01", command.Model.Values(0, 0))
        Assert.AreEqual(1, command.Model.PvalueCells(0).BodyRow)
        Assert.AreEqual(2, command.Model.PvalueColumns.Count)
    End Sub

    <TestMethod>
    Public Sub SupplementaryEnvelope_PreservesAllGapsAndOverwriteDecision()
        Dim source = Models()
        Dim destination = Target()
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        Dim extras As ResultTableOutputModel() = {New ResultTableOutputModel(New Object(1, 6) {}), New ResultTableOutputModel(New Object(2, 1) {})}
        Assert.AreEqual("R6C3:R23C9", ExcelDnaSharedOutputPlanAdapter.PreflightBounds(prepared, extras).ToString())
        Dim prompts As Integer = 0
        Dim writes As Integer = 0
        For Each occupied In New Boolean() {False, True}
            For Each answer In New Boolean() {False, True}
                Dim allowed = ExcelDnaSharedOutputPlanAdapter.ConfirmOverwrite(occupied,
                    Function()
                        prompts += 1
                        Return answer
                    End Function)
                Assert.AreEqual(Not occupied OrElse answer, allowed)
                Dim previous = writes
                If allowed Then prepared.Execute(destination, "Results", Sub(c) writes += 1, Sub(r) Return)
                Assert.AreEqual(If(allowed, 4, 0), writes - previous)
            Next
        Next
        Assert.AreEqual(2, prompts)
    End Sub

    <TestMethod>
    Public Sub SupplementaryOverflow_AndEmptySharedFootprintRefuse()
        Dim destination = Target()
        destination.StartRow = 1048564
        Dim source = Models()
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        Assert.ThrowsException(Of ArgumentException)(Sub() ExcelDnaSharedOutputPlanAdapter.PreflightBounds(prepared, Models()))
        Dim empty As New List(Of ResultTableOutputModel)
        prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(empty), Target()), Requests(empty, Target()), Target(), "Results")
        Dim writes As Integer = 0
        prepared.Execute(Target(), "Results", Sub(c) writes += 1, Sub(r) Assert.Fail("empty cursor"))
        Assert.AreEqual(0, writes)
        Assert.ThrowsException(Of ArgumentException)(Sub() ExcelDnaSharedOutputPlanAdapter.PreflightBounds(prepared, Nothing))
    End Sub

    <DataTestMethod>
    <DataRow("anova-base")> <DataRow("anova-welch-tukey")> <DataRow("anova-all-posthoc")>
    <DataRow("kw-base")> <DataRow("kw-dunn")>
    Public Sub Stage304Models_CompletePayloadTypesMetadataAndSpacingMatchLegacy(name As String)
        Dim serializer As New DataContractJsonSerializer(GetType(GoldenCase))
        Dim fixture As GoldenCase
        Using stream = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Stage304", name & ".json"))
            fixture = DirectCast(serializer.ReadObject(stream), GoldenCase)
        End Using
        Dim input As New GroupedNumericData(fixture.Groups.Names, fixture.Groups.Values)
        Dim options As SpreadsheetAnalysisOptions
        If fixture.Kind = 1 Then
            options = New OneWayAnovaAnalysisOptions With {.IncludeWelch = fixture.Options.Welch,
                .IncludeFisherLsd = fixture.Options.FisherLsd, .IncludeBonferroni = fixture.Options.Bonferroni,
                .IncludeTukeyKramer = fixture.Options.TukeyKramer, .IncludeGamesHowell = fixture.Options.GamesHowell}
        Else
            options = New KruskalWallisAnalysisOptions With {.IncludeDunnComparisons = fixture.Options.Dunn}
        End If
        Dim result = SharedAnalysisApplicationService.Run(New SpreadsheetAnalysisRequest(CType(fixture.Kind, SpreadsheetAnalysisKind), input, options))
        Dim destination = Target()
        Dim payload = Requests(result.Tables, destination)
        For Each cultureName In New String() {"en-US", "sk-SK"}
            Dim previous = CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
                Dim layout = OutputLayoutPlanner.Create(OutputDocument.FromResult(result), destination)
                Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(layout, payload, destination, "Results")
                Assert.AreEqual(fixture.Tables.Length, prepared.Commands.Count)
                Dim nextRow As Integer = 6
                For i As Integer = 0 To prepared.Commands.Count - 1
                    Dim command = prepared.Commands(i)
                    Assert.AreEqual(nextRow, command.Bounds.StartRow)
                    Assert.AreEqual(3, command.Bounds.StartColumn)
                    Assert.AreEqual(i, command.SourceOrdinal)
                    Assert.AreEqual(fixture.Tables(i).Rows, command.Model.RowCount)
                    Assert.AreEqual(fixture.Tables(i).Columns, command.Model.ColumnCount)
                    Assert.AreEqual(fixture.Tables(i).TopLeft, command.Model.Values(0, 0))
                    AssertModelMatchesRequest(payload(i), command.Model)
                    Assert.AreEqual(0.01, command.PvalueHighlightAlpha)
                    Assert.AreEqual(payload(i).PvalueNumberFormat, command.PvalueNumberFormat)
                    Assert.IsFalse(command.AutoFit)
                    nextRow = command.Bounds.EndRow + 2
                Next
            Finally
                CultureInfo.CurrentCulture = previous
            End Try
        Next
    End Sub

    Private Shared Sub AssertModelMatchesRequest(request As ResultTableWriteRequest, model As ResultTableOutputModel)
        Assert.AreEqual(request.TitleRows, model.TitleRows)
        Assert.AreEqual(request.HeaderTopRows, model.HeaderTopRows)
        Assert.AreEqual(request.HeaderLeftColumns, model.HeaderLeftColumns)
        Assert.AreEqual(request.FooterRows, model.FooterRows)
        Assert.AreEqual(request.IsResultTable, model.IsResultTable)
        CollectionAssert.AreEqual(request.PvalueColumns, model.PvalueColumns)
        Assert.AreEqual(request.PvalueCells.Count, model.PvalueCells.Count)
        For i As Integer = 0 To model.PvalueCells.Count - 1
            Assert.AreEqual(request.PvalueCells(i).BodyRow, model.PvalueCells(i).BodyRow)
            Assert.AreEqual(request.PvalueCells(i).BodyColumn, model.PvalueCells(i).BodyColumn)
        Next
        For row As Integer = 0 To model.RowCount - 1
            For column As Integer = 0 To model.ColumnCount - 1
                Dim expected = request.Values(row)(column)
                Dim actual = model.Values(row, column)
                Assert.AreEqual(expected, actual)
                If expected IsNot Nothing Then Assert.AreEqual(expected.GetType(), actual.GetType())
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub SectionsAndBodyPValues_MapOnceFromC6WhileNumericTextRetainsWindowsPolicy()
        Dim model As New ResultTableOutputModel(New Object(,) {
            {"Title", Nothing, Nothing}, {Nothing, "Estimate", "P"},
            {"A", 2.0R, 0.01R}, {"B", 3.0R, "0.01"}, {"Foot", Nothing, Nothing}},
            titleRows:=1, headerTopRows:=1, headerLeftColumns:=1, footerRows:=1,
            pvalueColumns:={2}, pvalueCells:={New ResultTableCellAddress(1, 2)})
        Dim document = OutputDocument.FromModel(model)
        Dim semantic = OutputSemanticPlanner.Create(document, Nothing, 0.05)
        Dim destination = Target()
        Dim layout = OutputLayoutPlanner.Create(document, destination, semanticPlan:=semantic)
        Dim command = ExcelDnaSharedOutputPlanAdapter.Prepare(layout, Requests({model}, destination), destination, "Results").Commands(0)
        Dim styles = semantic.Artifacts(0).Cells
        CollectionAssert.AreEqual(New OutputStyleRole() {OutputStyleRole.Title, OutputStyleRole.TopColumnHeader,
            OutputStyleRole.LeftRowHeader, OutputStyleRole.Footer},
            New OutputStyleRole() {styles(0).Role, styles(3).Role, styles(6).Role, styles(12).Role})
        Dim numericCell = styles.Single(Function(c) c.Row = 2 AndAlso c.Column = 2)
        Dim textCell = styles.Single(Function(c) c.Row = 3 AndAlso c.Column = 2)
        Assert.IsTrue(numericCell.IsSignificant)
        Assert.IsFalse(textCell.IsSignificant, "Shared semantics are numeric-only.")
        Assert.IsTrue(ExcelDnaResultWriter.LegacyPValueIsHighlighted(command.Model.Values(3, 2), 0.05, CultureInfo.GetCultureInfo("en-US")))
        Assert.AreEqual(8, command.Bounds.StartRow + numericCell.Row)
        Assert.AreEqual(5, command.Bounds.StartColumn + numericCell.Column)
        Assert.AreEqual(8, command.Bounds.StartRow + model.TitleRows + model.HeaderTopRows + model.PvalueCells(0).BodyRow - 1)
        Assert.AreEqual(5, command.Bounds.StartColumn + model.HeaderLeftColumns + model.PvalueCells(0).BodyColumn - 1)
        Assert.IsInstanceOfType(command.Model.Values(2, 2), GetType(Double))
        Assert.IsInstanceOfType(command.Model.Values(3, 2), GetType(String))
    End Sub

    <DataTestMethod>
    <DataRow("en-US", "0.01")> <DataRow("sk-SK", "0,01")>
    Public Sub LegacyFormatting_PreservesCultureSettingsAndInvalidOverrideFallback(cultureName As String, text As String)
        Dim culture = CultureInfo.GetCultureInfo(cultureName)
        Assert.IsTrue(ExcelDnaResultWriter.LegacyPValueIsHighlighted(text, 0.05, culture))
        Assert.IsTrue(ExcelDnaResultWriter.LegacyPValueIsHighlighted(0.05R, 0.05, culture))
        For Each raw As Object In New Object() {True, False, Nothing, "#N/A", -0.01R, 0.051R, Double.NaN, Double.PositiveInfinity}
            Assert.IsFalse(ExcelDnaResultWriter.LegacyPValueIsHighlighted(raw, 0.05, culture))
        Next
        Dim configured As Double = AppGlobals.DefaultAlpha
        For Each invalid As Double In New Double() {0, 1, -1, Double.NaN, Double.PositiveInfinity}
            Assert.AreEqual(configured, ExcelDnaResultWriter.ResolvePValueAlpha(invalid, configured))
        Next
        Assert.AreEqual(configured, ExcelDnaResultWriter.ResolvePValueAlpha(Nothing, configured))
        Assert.AreEqual(0.01, ExcelDnaResultWriter.ResolvePValueAlpha(0.01, configured))
        Dim settings As New PValuePresentationSettings With {.DecimalPlaces = 5, .UseUpperBound = True}
        Assert.AreEqual(PValuePresentation.BuildExcelNumberFormat(settings), ExcelDnaResultWriter.ResolvePValueNumberFormat(" ", settings))
        Assert.AreEqual("custom", ExcelDnaResultWriter.ResolvePValueNumberFormat("custom", settings))
        Dim factory = ResultTableWriteRequestFactory.Create(Models()(0), settings, Double.NaN, Target())
        Assert.AreEqual(0.05, factory.PvalueHighlightAlpha, "Factory fallback differs from live invalid-override fallback.")
    End Sub

    Private Class RecordingWriter
        Inherits ExcelDnaResultWriter
        Public ReadOnly Written As New List(Of ExcelDnaSharedWriteCommand)
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Public Sub New(row As Integer, column As Integer)
            MyBase.New(row, column)
        End Sub
        Public Sub Record(command As ExcelDnaSharedWriteCommand)
            Written.Add(command)
        End Sub
        Friend Overrides Sub WriteOutputBlockCore(block As ResultTableOutputBlock, numberFormat As String,
                                                  alpha As Double?, autoFit As Boolean, Optional plannedBounds As OutputRectangle = Nothing)
            Blocks.Add(block)
        End Sub
    End Class

    <DataContract> Private Class GoldenCase
        <DataMember(Name:="legacyEnumValue")> Public Kind As Integer
        <DataMember(Name:="groups")> Public Groups As GoldenGroups
        <DataMember(Name:="options")> Public Options As GoldenOptions
        <DataMember(Name:="tables")> Public Tables As GoldenTable()
    End Class
    <DataContract> Private Class GoldenGroups
        <DataMember(Name:="names")> Public Names As String()
        <DataMember(Name:="values")> Public Values As Double()()
    End Class
    <DataContract> Private Class GoldenOptions
        <DataMember(Name:="welch")> Public Welch As Boolean
        <DataMember(Name:="fisherLsd")> Public FisherLsd As Boolean
        <DataMember(Name:="bonferroni")> Public Bonferroni As Boolean
        <DataMember(Name:="tukeyKramer")> Public TukeyKramer As Boolean
        <DataMember(Name:="gamesHowell")> Public GamesHowell As Boolean
        <DataMember(Name:="dunn")> Public Dunn As Boolean
    End Class
    <DataContract> Private Class GoldenTable
        <DataMember(Name:="rows")> Public Rows As Integer
        <DataMember(Name:="columns")> Public Columns As Integer
        <DataMember(Name:="topLeft")> Public TopLeft As String
    End Class
End Class
