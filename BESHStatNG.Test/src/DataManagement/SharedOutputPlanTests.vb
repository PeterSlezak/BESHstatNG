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

    <DataTestMethod>
    <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub TargetPreflight_AllWindowsDestinationsPreserveC10BeforeHostCreation(destination As Integer)
        Dim requested = Target()
        requested.Destination = CType(destination, SpreadsheetOutputDestination)
        requested.StartRow = 10
        Dim bounds = ExcelDnaOutputTargetAdapter.Preflight(requested, New OutputTargetFacts(), Document(Models()), Nothing)
        Assert.AreEqual(10, bounds.StartRow)
        Assert.AreEqual(3, bounds.StartColumn)
        Assert.AreEqual(20, bounds.EndRow)
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(Models()), requested),
            Requests(Models(), requested), requested, "Results")
        Dim writer As New RecordingWriter(10, 3)
        prepared.Execute(requested, "Results", Sub(c) writer.Record(c), Sub(row) writer.setRowPointer(row))
        Assert.AreEqual(4, writer.Written.Count)
        Assert.AreEqual(21, writer.RowID)
        Assert.AreEqual(3, writer.ColID)
    End Sub

    <DataTestMethod>
    <DataRow("protected")> <DataRow("merged")> <DataRow("missing")> <DataRow("readonly")>
    <DataRow("row")> <DataRow("column")> <DataRow("unknown")> <DataRow("unsupported")>
    <DataRow("geometry")> <DataRow("supplementary")> <DataRow("structure")>
    Public Sub TargetPreflight_KnownInvalidRequestDoesNotReachMutationOrConfirmation(reason As String)
        Dim requested = Target()
        Dim facts As New OutputTargetFacts()
        Dim host = ExcelDnaHostCapabilities.GetSnapshot()
        Dim extras As IEnumerable(Of ResultTableOutputModel) = Nothing
        Select Case reason
            Case "protected" : facts = New OutputTargetFacts(protectedSheet:=True)
            Case "merged" : facts = New OutputTargetFacts(mergedConflict:=True)
            Case "missing" : facts = New OutputTargetFacts(sheetExists:=False)
            Case "readonly" : facts = New OutputTargetFacts(writable:=False)
            Case "row" : requested.StartRow = 0
            Case "column" : requested.StartColumn = 16385
            Case "unknown" : requested.Destination = CType(99, SpreadsheetOutputDestination)
            Case "unsupported" : host.Capabilities.Remove(HostCapabilityIds.CurrentWorksheet)
            Case "geometry" : requested.StartRow = 1048575
            Case "supplementary" : requested.StartRow = 1048550 : extras = {New ResultTableOutputModel(New Object(29, 1) {})}
            Case "structure" : requested.Destination = SpreadsheetOutputDestination.NewWorksheet : facts = New OutputTargetFacts(canCreateWorksheet:=False)
        End Select
        Dim mutations As Integer = 0
        Dim confirmations As Integer = 0
        Dim rejected As Boolean = False
        Try
            ExcelDnaOutputTargetAdapter.Preflight(requested, facts, Document(Models()), extras, host)
            mutations += 1 ' represents GetResultWriter destination creation after the real pure seam
            confirmations += 1
        Catch ex As HostCapabilityException
            Assert.IsFalse(String.IsNullOrWhiteSpace(ex.Code))
            rejected = True
        Catch ex As ArgumentException
            rejected = True
        End Try
        Assert.IsTrue(rejected)
        Assert.AreEqual(0, mutations)
        Assert.AreEqual(0, confirmations)
    End Sub

    <DataTestMethod>
    <DataRow(0, False)> <DataRow(0, True)> <DataRow(1, True)> <DataRow(2, True)>
    Public Sub SafeSharedRoute_ExactPrimarySupplementaryChartOrderAndOnePrompt(destinationKind As Integer, approved As Boolean)
        Dim destination = Target() : destination.Destination = CType(destinationKind, SpreadsheetOutputDestination)
        Dim source = Models(), prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(
            OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        Dim extra As New ResultTable() : extra.SetBody(New Object(,) {{"supplementary", 0.01}, {"literal", "=1+1"}})
        extra.AddPvalueToFormat(2)
        Dim extras As New List(Of ResultTable) From {extra, extra}
        Dim guard As New OutputRectangle(1, 1, 100, 20)
        Dim scope = ExcelDnaSharedOutputPlanAdapter.CreateWriteIntent(prepared, extras.Select(Function(t) t.ToOutputModel()),
            {"boxplot", "posthoc-chart"}, {guard}, "Workbook", "Sheet1")
        CollectionAssert.AreEqual(New String() {"R6C3:R7C5", "R9C3:R11C4", "R13C3:R13C6", "R15C3:R16C3", "R18C3:R19C4", "R21C3:R22C4"},
            scope.Regions.Where(Function(r) r.Bounds IsNot Nothing).Select(Function(r) r.Bounds.ToString()).ToArray())
        Assert.AreEqual(1, scope.Guards.Count) : Assert.AreSame(guard, scope.Guards(0))
        Dim writer As New RecordingWriter(6, 3), chartCalls As Integer = 0, prompts As Integer = 0, probes As Integer = 0
        Dim actions = ExcelDnaSharedOutputPlanAdapter.CreateWriteActions(prepared, writer, extras,
            New Action() {Sub() chartCalls += 1, Sub() chartCalls += 1})
        Dim result = SafeWrite.Execute(scope, Function()
                                                 probes += 1
                                                 Return New WriteInspection(scope, OutputOccupancy.Occupied, "observed-content", {"supplementary-1"}, hasFormulas:=True)
                                             End Function,
            Function(observed)
                prompts += 1
                Return approved
            End Function, actions)
        Assert.AreEqual(1, prompts) : Assert.AreEqual(If(approved, 2, 1), probes)
        Assert.AreEqual(If(approved, OutputWriteState.Completed, OutputWriteState.CancelledByUser), result.State)
        Assert.AreEqual(If(approved, 6, 0), writer.Blocks.Count) : Assert.AreEqual(If(approved, 2, 0), chartCalls)
        Assert.AreEqual(If(approved, 23, 6), writer.RowID) : Assert.AreEqual(3, writer.ColID)
        Assert.IsFalse(result.UndoSupported) : Assert.IsFalse(result.RollbackAttempted)
        If approved Then
            CollectionAssert.AreEqual(scope.Regions.Where(Function(r) r.Bounds IsNot Nothing).Select(Function(r) r.Bounds.StartRow).ToArray(),
                writer.Blocks.Select(Function(b) b.StartRow).ToArray())
            Assert.IsTrue(writer.AutoFits.All(Function(value) Not value))
            CollectionAssert.AreEqual(prepared.Commands.Select(Function(c) c.PvalueNumberFormat).ToArray(), writer.Formats.Take(4).ToArray())
            Assert.AreEqual(0.01, writer.Blocks(4).Model.Values(0, 1))
        End If
    End Sub

    <DataTestMethod>
    <DataRow("protected")> <DataRow("merged")> <DataRow("unknown")> <DataRow("changed")>
    Public Sub SafeSharedRoute_InvalidOrChangedInspectionNeverWrites(reason As String)
        Dim destination = Target(), source = Models()
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        Dim scope = ExcelDnaSharedOutputPlanAdapter.CreateWriteIntent(prepared, Nothing, Nothing, Nothing, "Workbook", "Sheet1")
        Dim writer As New RecordingWriter(6, 3), probes As Integer = 0, prompts As Integer = 0
        Dim result = SafeWrite.Execute(scope, Function()
                                                 probes += 1
                                                 If reason = "protected" OrElse reason = "merged" Then Throw New InvalidOperationException("Read-only target rejection")
                                                 Return New WriteInspection(scope, If(reason = "unknown", OutputOccupancy.Unknown, OutputOccupancy.Empty), probes.ToString(), Nothing)
                                             End Function, Function(observed)
                                                               prompts += 1
                                                               Return True
                                                           End Function,
            ExcelDnaSharedOutputPlanAdapter.CreateWriteActions(prepared, writer, New ResultTable() {}, New Action() {}))
        Assert.AreEqual(0, writer.Blocks.Count) : Assert.AreEqual(0, prompts)
        Assert.IsTrue(result.State = OutputWriteState.FailedBeforeWrite OrElse result.State = OutputWriteState.RejectedBeforeWrite)
        Assert.IsFalse(result.MutationMayHaveOccurred)
    End Sub

    <DataTestMethod>
    <DataRow(1, 0)> <DataRow(2, 1)> <DataRow(5, 4)>
    Public Sub SafeSharedRoute_FailureAfterBlockOrDuringSupplementReportsPartial(failAt As Integer, confirmed As Integer)
        Dim destination = Target(), source = Models()
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        Dim extra As New ResultTable() : extra.SetBody(New Object(,) {{1, 0.01}})
        Dim scope = ExcelDnaSharedOutputPlanAdapter.CreateWriteIntent(prepared, {extra.ToOutputModel()}, Nothing, Nothing, "Workbook", "Sheet1")
        Dim writer As New RecordingWriter(6, 3) With {.FailAt = failAt}
        Dim result = SafeWrite.Execute(scope, Function() New WriteInspection(scope, OutputOccupancy.Empty, "empty", Nothing), Function(o) True,
            ExcelDnaSharedOutputPlanAdapter.CreateWriteActions(prepared, writer, {extra}, New Action() {}))
        Assert.AreEqual(OutputWriteState.PartiallyWritten, result.State) : Assert.AreEqual(WriteCodes.MutationUncertain, result.Code)
        Assert.AreEqual(confirmed, result.ConfirmedWritten.Count) : Assert.IsTrue(result.MutationMayHaveOccurred)
        Assert.IsFalse(result.RollbackSucceeded)
    End Sub

    Private Shared Function FixtureResult(name As String) As SpreadsheetAnalysisResult
        Dim fixture As GoldenCase
        Using stream = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Stage304", name & ".json"))
            fixture = DirectCast(New DataContractJsonSerializer(GetType(GoldenCase)).ReadObject(stream), GoldenCase)
        End Using
        Dim options As SpreadsheetAnalysisOptions
        If fixture.Kind = 1 Then
            options = New OneWayAnovaAnalysisOptions With {.IncludeWelch = fixture.Options.Welch, .IncludeFisherLsd = fixture.Options.FisherLsd,
                .IncludeBonferroni = fixture.Options.Bonferroni, .IncludeTukeyKramer = fixture.Options.TukeyKramer, .IncludeGamesHowell = fixture.Options.GamesHowell}
        Else
            options = New KruskalWallisAnalysisOptions With {.IncludeDunnComparisons = fixture.Options.Dunn}
        End If
        Return SharedAnalysisApplicationService.Run(New SpreadsheetAnalysisRequest(CType(fixture.Kind, SpreadsheetAnalysisKind),
            New GroupedNumericData(fixture.Groups.Names, fixture.Groups.Values), options))
    End Function

    ' Matches the production RunSharedOutput lifecycle. Only native destination/facts,
    ' reads and user interaction are simulated, rather than bypassing intent construction.
    Private Class WorkflowRecordingHost
        Public ReadOnly Analysis As SpreadsheetAnalysisResult
        Public ReadOnly Extras As New List(Of ResultTable)
        Public ReadOnly NativeWriter As RecordingWriter
        Public ReadOnly Host As ExcelDnaSharedOutputHost
        Public ReadOnly Session As New ExcelDnaSharedOutputRun
        Public ReadOnly OriginalFailure As New InvalidOperationException("Injected host failure")
        Public Prompts As Integer, Inspections As Integer, Creations As Integer, ChartCalls As Integer
        Public Approved As Boolean, IncludeCharts As Boolean, ChangeAfterConsent As Boolean
        Public DistantInput As Boolean
        Public ShapePrompts As Integer, ShapeInspections As Integer, ShapeCount As Integer
        Public ShapesApproved As Boolean, ChangeShapesAfterConsent As Boolean, FailShapeInspection As Boolean
        Public ReadOnly CellReads As New List(Of OutputRectangle)
        Public OccupiedId As String
        Public OccupiedValue As Object = "existing"
        Public OccupiedFormula As Object
        Public CreationFailure As Integer = -1
        Public FailInspection As Boolean, FailChart As Boolean

        Public Sub New(name As String, destination As SpreadsheetOutputDestination)
            Analysis = FixtureResult(name)
            Dim requested As New SpreadsheetOutputTarget With {.Destination = destination,
                .WorksheetName = If(destination = SpreadsheetOutputDestination.CurrentWorksheet, "anova-stage304", Nothing),
                .StartRow = If(destination = SpreadsheetOutputDestination.CurrentWorksheet, 11, 1),
                .StartColumn = If(destination = SpreadsheetOutputDestination.CurrentWorksheet, 6, 1)}
            NativeWriter = New RecordingWriter(requested.StartRow, requested.StartColumn)
            Host = New ExcelDnaSharedOutputHost With {
                .Acquire = Function(run)
                               run.RequestedTarget = requested
                               run.Stage = "capability-check"
                               ExcelDnaHostCapabilities.EnsureCanWrite(Analysis.Document.AnalysisId, requested)
                               run.Stage = "preflight"
                               ExcelDnaOutputTargetAdapter.Preflight(requested, New OutputTargetFacts(workbookAvailable:=True,
                                   sheetExists:=True, identityUnambiguous:=True, writable:=True, protectedSheet:=False, mergedConflict:=False, canCreateWorksheet:=True),
                                   OutputDocument.FromResult(Analysis), Extras.Select(Function(t) t.ToOutputModel()))
                               If CreationFailure = 0 Then Throw OriginalFailure
                               If destination <> SpreadsheetOutputDestination.CurrentWorksheet Then
                                   run.Stage = "destination-creation"
                                   run.CreationAttempted = True : Creations += 1
                                   If CreationFailure = 1 Then Throw OriginalFailure
                                   run.DestinationCreated = True
                                   If CreationFailure = 2 Then Throw OriginalFailure
                               End If
                               Return NativeWriter
                           End Function,
                .Target = Function(w) New SpreadsheetOutputTarget With {.Destination = destination, .WorksheetName = "Resolved sheet",
                    .StartRow = w.RowID, .StartColumn = w.ColID},
                .Identities = Function(w) Tuple.Create("native-workbook", "native-sheet"),
                .InspectCharts = Function(w, intent)
                                     ShapeInspections += 1
                                     If FailShapeInspection Then Throw OriginalFailure
                                     Return New ExcelDnaChartSafetySnapshot(ShapeCount, If(ChangeShapesAfterConsent AndAlso ShapeInspections > 1, "changed-shapes", "observed-shapes"))
                                 End Function,
                .ConfirmCharts = Function(observed)
                                     ShapePrompts += 1
                                     Return ShapesApproved
                                 End Function,
                .Charts = Function(w, columns)
                              Dim actions As New List(Of Action)()
                              If IncludeCharts Then
                                  For i As Integer = 1 To 2
                                      actions.Add(Sub()
                                                      ChartCalls += 1
                                                      If FailChart Then Throw OriginalFailure
                                                  End Sub)
                                  Next
                              End If
                              Return actions
                          End Function,
                .Inspect = Function(w, intent)
                               Inspections += 1
                               If FailInspection Then Throw OriginalFailure
                               Return ExcelDnaOutputTargetAdapter.InspectWriteCore(intent, Sub(r)
                                                                                         End Sub,
                                   Function(bounds)
                                       CellReads.Add(bounds)
                                       Dim region = intent.Regions.FirstOrDefault(Function(r) Object.ReferenceEquals(r.Bounds, bounds))
                                       Dim occupied As Boolean = region IsNot Nothing AndAlso region.ArtifactId = OccupiedId
                                       Dim value = If(occupied, OccupiedValue, Nothing)
                                       If DistantInput AndAlso bounds.StartRow = 1 AndAlso bounds.StartColumn = 1 Then value = 1.0
                                       If ChangeAfterConsent AndAlso Inspections = 2 Then value = "changed"
                                       Return NativeSnapshot(bounds, value, If(occupied, OccupiedFormula, Nothing))
                                   End Function)
                           End Function,
                .Confirm = Function(observed)
                               Prompts += 1
                               Return Approved
                           End Function}
        End Sub

        Public Function Execute() As WriteResult
            Return Session.Execute(Analysis, Extras, New PValuePresentationSettings With {.DecimalPlaces = 5}, 0.01, Host)
        End Function
    End Class

    <DataTestMethod>
    <DataRow("anova-all-posthoc", False)> <DataRow("anova-all-posthoc", True)>
    <DataRow("kw-dunn", False)> <DataRow("kw-dunn", True)>
    Public Sub FullSharedOrchestration_DistantInputNeverOccupiesEmptyF11(name As String, charts As Boolean)
        Dim recording As New WorkflowRecordingHost(name, SpreadsheetOutputDestination.CurrentWorksheet) With {
            .DistantInput = True, .IncludeCharts = charts}
        Dim extra As New ResultTable() : extra.SetBody(New Object(,) {{0.01}}) : recording.Extras.Add(extra)
        Dim outcome = recording.Execute()
        Assert.AreEqual(OutputWriteState.Completed, outcome.State)
        Assert.AreEqual(0, recording.Prompts)
        Assert.AreEqual(If(charts, 2, 0), recording.ChartCalls)
        Assert.AreEqual(recording.Analysis.Tables.Count + 1, recording.NativeWriter.Blocks.Count)
        Assert.AreEqual(0, outcome.Intent.Guards.Count)
        Assert.AreEqual(0, recording.ShapePrompts)
        Assert.IsTrue(recording.CellReads.All(Function(b) b.StartRow >= 11 AndAlso b.StartColumn = 6))
    End Sub

    <DataTestMethod>
    <DataRow("anova-all-posthoc", False)> <DataRow("anova-all-posthoc", True)>
    <DataRow("kw-dunn", False)> <DataRow("kw-dunn", True)>
    Public Sub FullSharedOrchestration_ExistingShapesRequireDistinctOverlapDecision(name As String, approved As Boolean)
        Dim recording As New WorkflowRecordingHost(name, SpreadsheetOutputDestination.CurrentWorksheet) With {
            .IncludeCharts = True, .DistantInput = True, .ShapeCount = 1, .ShapesApproved = approved}
        Dim outcome = recording.Execute()
        Assert.AreEqual(If(approved, OutputWriteState.Completed, OutputWriteState.CancelledByUser), outcome.State)
        Assert.AreEqual(1, recording.ShapePrompts) : Assert.AreEqual(0, recording.Prompts)
        Assert.AreEqual(If(approved, 2, 1), recording.ShapeInspections)
        Assert.AreEqual(If(approved, 2, 0), recording.ChartCalls)
        StringAssert.Contains(outcome.Code, If(approved, "chart-overlap-approved", "chart-cancelled"))
        Assert.IsFalse(outcome.Message.Contains("range not empty"))
        If Not approved Then
            Assert.AreEqual(0, recording.NativeWriter.Blocks.Count) : Assert.AreEqual(0, recording.NativeWriter.Formats.Count)
            Assert.IsFalse(outcome.MutationMayHaveOccurred)
        End If
    End Sub

    <DataTestMethod>
    <DataRow(False)> <DataRow(True)>
    Public Sub FullSharedOrchestration_UnknownShapeInventoryFailsClosedBeforeWrites(afterCreation As Boolean)
        Dim recording As New WorkflowRecordingHost("anova-base", If(afterCreation, SpreadsheetOutputDestination.NewWorksheet,
            SpreadsheetOutputDestination.CurrentWorksheet)) With {.IncludeCharts = True, .FailShapeInspection = True}
        Dim outcome = recording.Execute()
        Assert.AreEqual(If(afterCreation, OutputWriteState.PartiallyWritten, OutputWriteState.FailedBeforeWrite), outcome.State)
        Assert.AreEqual(afterCreation, outcome.MutationMayHaveOccurred)
        Assert.AreEqual(0, recording.NativeWriter.Blocks.Count) : Assert.AreEqual(0, recording.Prompts)
        Assert.AreEqual(0, recording.ShapePrompts) : Assert.AreEqual(0, recording.ChartCalls)
        Assert.AreSame(recording.OriginalFailure, recording.Session.Failure)
        Assert.AreEqual("chart-inspection", recording.Session.Stage)
        Assert.IsFalse(outcome.RollbackAttempted)
    End Sub

    <DataTestMethod>
    <DataRow(0)> <DataRow(1)>
    Public Sub FullSharedOrchestration_ChangedCellsOrShapesAfterConsentNeverWrite(change As Integer)
        Dim recording As New WorkflowRecordingHost("kw-dunn", SpreadsheetOutputDestination.CurrentWorksheet) With {
            .IncludeCharts = True, .ShapeCount = 1, .ShapesApproved = True, .OccupiedId = "table-0", .Approved = True,
            .ChangeAfterConsent = change = 0, .ChangeShapesAfterConsent = change = 1}
        Dim outcome = recording.Execute()
        Assert.AreEqual(OutputWriteState.RejectedBeforeWrite, outcome.State)
        Assert.AreEqual(WriteCodes.Changed, outcome.Code)
        Assert.AreEqual(1, recording.Prompts) : Assert.AreEqual(1, recording.ShapePrompts)
        Assert.AreEqual(0, recording.NativeWriter.Blocks.Count) : Assert.AreEqual(0, recording.ChartCalls)
        Assert.IsFalse(outcome.MutationMayHaveOccurred)
    End Sub

    <DataTestMethod>
    <DataRow("anova-all-posthoc", 1)> <DataRow("anova-all-posthoc", 2)>
    <DataRow("kw-dunn", 1)> <DataRow("kw-dunn", 2)>
    Public Sub FullSharedOrchestration_ChartsAndSupplementaryNewDestinationsStillComplete(name As String, destination As Integer)
        Dim recording As New WorkflowRecordingHost(name, CType(destination, SpreadsheetOutputDestination)) With {.IncludeCharts = True}
        Dim extra As New ResultTable() : extra.SetBody(New Object(,) {{0.01}}) : recording.Extras.Add(extra)
        Dim outcome = recording.Execute()
        Assert.AreEqual(OutputWriteState.Completed, outcome.State)
        Assert.AreEqual(1, recording.Creations) : Assert.AreEqual(2, recording.ChartCalls)
        Assert.AreEqual(0, recording.Prompts) : Assert.AreEqual(0, recording.ShapePrompts)
        Assert.AreEqual(recording.Analysis.Tables.Count + 1, recording.NativeWriter.Blocks.Count)
        Assert.IsTrue(recording.NativeWriter.AutoFits.All(Function(v) Not v))
        Assert.AreEqual("native-sheet", outcome.Intent.WorksheetIdentity)
    End Sub

    <DataTestMethod>
    <DataRow("anova-all-posthoc", "table-0", False)> <DataRow("anova-all-posthoc", "table-0", True)>
    <DataRow("anova-all-posthoc", "table-1", False)> <DataRow("anova-all-posthoc", "table-1", True)>
    <DataRow("anova-all-posthoc", "supplementary-1", False)> <DataRow("anova-all-posthoc", "supplementary-1", True)>
    <DataRow("kw-dunn", "table-0", False)> <DataRow("kw-dunn", "table-0", True)>
    <DataRow("kw-dunn", "table-1", False)> <DataRow("kw-dunn", "table-1", True)>
    <DataRow("kw-dunn", "supplementary-1", False)> <DataRow("kw-dunn", "supplementary-1", True)>
    Public Sub FullSharedOrchestration_BlankFormulaInAnyTableWithChartsRetainsCellConsent(name As String, id As String, approved As Boolean)
        Dim recording As New WorkflowRecordingHost(name, SpreadsheetOutputDestination.CurrentWorksheet) With {
            .OccupiedId = id, .OccupiedValue = String.Empty, .OccupiedFormula = "=""""", .Approved = approved, .IncludeCharts = True, .DistantInput = True}
        For index As Integer = 0 To 1
            Dim extra As New ResultTable() : extra.SetBody(New Object(,) {{0.01}}) : recording.Extras.Add(extra)
        Next
        Dim outcome = recording.Execute()
        Assert.AreEqual(If(approved, OutputWriteState.Completed, OutputWriteState.CancelledByUser), outcome.State)
        Assert.AreEqual(1, recording.Prompts) : Assert.AreEqual(0, recording.ShapePrompts)
        Assert.AreEqual(If(approved, recording.Analysis.Tables.Count + 2, 0), recording.NativeWriter.Blocks.Count)
        Assert.AreEqual(If(approved, 2, 0), recording.ChartCalls)
        If Not approved Then
            Assert.AreEqual(0, recording.NativeWriter.Formats.Count) : Assert.AreEqual(0, recording.Creations)
            Assert.IsFalse(outcome.MutationMayHaveOccurred)
        End If
    End Sub

    <DataTestMethod>
    <DataRow("anova-base", 0)> <DataRow("anova-base", 1)> <DataRow("anova-base", 2)>
    <DataRow("anova-welch-tukey", 0)> <DataRow("anova-welch-tukey", 1)> <DataRow("anova-welch-tukey", 2)>
    <DataRow("anova-all-posthoc", 0)> <DataRow("anova-all-posthoc", 1)> <DataRow("anova-all-posthoc", 2)>
    <DataRow("kw-base", 0)> <DataRow("kw-base", 1)> <DataRow("kw-base", 2)>
    <DataRow("kw-dunn", 0)> <DataRow("kw-dunn", 1)> <DataRow("kw-dunn", 2)>
    Public Sub FullSharedOrchestration_RealFixturesAllDestinationsReachWriter(name As String, destination As Integer)
        Dim recording As New WorkflowRecordingHost(name, CType(destination, SpreadsheetOutputDestination))
        Dim outcome = recording.Execute()
        Assert.AreEqual(OutputWriteState.Completed, outcome.State)
        Assert.AreEqual(recording.Analysis.Tables.Count, recording.NativeWriter.Blocks.Count)
        Assert.AreEqual(If(destination = 0, 11, 1), recording.NativeWriter.Blocks(0).StartRow)
        Assert.AreEqual(If(destination = 0, 6, 1), recording.NativeWriter.Blocks(0).StartColumn)
        Assert.AreEqual(If(destination = 0, 0, 1), recording.Creations)
        Assert.AreEqual(0, recording.Prompts) : Assert.AreEqual(2, recording.Inspections)
        Assert.AreEqual("native-sheet", outcome.Intent.WorksheetIdentity, "Physical identity does not depend on a nonempty VBA CodeName.")
        Assert.IsTrue(recording.NativeWriter.AutoFits.All(Function(v) Not v))
        For index As Integer = 0 To recording.Analysis.Tables.Count - 1
            AssertModelMatchesRequest(ResultTableWriteRequestFactory.Create(recording.Analysis.Tables(index), Nothing, 0.01, Nothing), recording.NativeWriter.Blocks(index).Model)
        Next
    End Sub

    <DataTestMethod>
    <DataRow("anova-base", False)> <DataRow("anova-base", True)>
    <DataRow("kw-dunn", False)> <DataRow("kw-dunn", True)>
    Public Sub FullSharedOrchestration_OccupiedF11PromptsOnceWithoutDeclinedWrites(name As String, approved As Boolean)
        Dim recording As New WorkflowRecordingHost(name, SpreadsheetOutputDestination.CurrentWorksheet) With {.OccupiedId = "table-0", .Approved = approved}
        Dim outcome = recording.Execute()
        Assert.AreEqual(If(approved, OutputWriteState.Completed, OutputWriteState.CancelledByUser), outcome.State)
        Assert.AreEqual(1, recording.Prompts) : Assert.AreEqual(If(approved, 2, 1), recording.Inspections)
        Assert.AreEqual(If(approved, recording.Analysis.Tables.Count, 0), recording.NativeWriter.Blocks.Count)
        If Not approved Then
            Assert.AreEqual(0, recording.NativeWriter.Formats.Count) : Assert.AreEqual(0, recording.NativeWriter.AutoFits.Count)
            Assert.AreEqual(11, recording.NativeWriter.RowID) : Assert.AreEqual(0, recording.Creations)
            Assert.IsFalse(outcome.MutationMayHaveOccurred)
        End If
    End Sub

    <DataTestMethod>
    <DataRow("anova-base")> <DataRow("kw-dunn")>
    Public Sub FullSharedOrchestration_SecondSupplementaryTableAndBlankFormulaRequireConsent(name As String)
        Dim recording As New WorkflowRecordingHost(name, SpreadsheetOutputDestination.CurrentWorksheet) With {
            .OccupiedId = "supplementary-1", .OccupiedValue = String.Empty, .OccupiedFormula = "="""""}
        For i As Integer = 0 To 1
            Dim extra As New ResultTable() : extra.SetBody(New Object(,) {{"supplement", 0.01}})
            recording.Extras.Add(extra)
        Next
        Dim outcome = recording.Execute()
        Assert.AreEqual(OutputWriteState.CancelledByUser, outcome.State)
        Assert.AreEqual(1, recording.Prompts) : Assert.AreEqual(0, recording.NativeWriter.Blocks.Count)
        Assert.IsTrue(outcome.ConfirmationRequired.Value)
    End Sub

    <DataTestMethod>
    <DataRow(1, 0)> <DataRow(1, 1)> <DataRow(1, 2)> <DataRow(2, 0)> <DataRow(2, 1)> <DataRow(2, 2)>
    Public Sub FullSharedOrchestration_CreationAttemptVersusConfirmedDestination(destination As Integer, failure As Integer)
        Dim recording As New WorkflowRecordingHost("anova-base", CType(destination, SpreadsheetOutputDestination)) With {.CreationFailure = failure}
        Dim outcome = recording.Execute()
        Assert.AreEqual(If(failure = 0, OutputWriteState.FailedBeforeWrite, OutputWriteState.PartiallyWritten), outcome.State)
        Assert.AreEqual(failure > 0, recording.Session.CreationAttempted)
        Assert.AreEqual(failure = 2, recording.Session.DestinationCreated)
        Assert.AreSame(recording.OriginalFailure, recording.Session.Failure)
        Assert.AreEqual(0, recording.NativeWriter.Blocks.Count)
        Assert.AreEqual(failure > 0, outcome.MutationMayHaveOccurred)
        Assert.IsFalse(outcome.RollbackSucceeded)
    End Sub

    <DataTestMethod>
    <DataRow(False)> <DataRow(True)>
    Public Sub FullSharedOrchestration_ChartsAndSupplementsKeepActionOrderAndPartialReporting(failChart As Boolean)
        Dim recording As New WorkflowRecordingHost("anova-all-posthoc", SpreadsheetOutputDestination.CurrentWorksheet) With {.IncludeCharts = True, .FailChart = failChart}
        Dim extra As New ResultTable() : extra.SetBody(New Object(,) {{0.01}}) : recording.Extras.Add(extra)
        Dim outcome = recording.Execute()
        Assert.AreEqual(If(failChart, OutputWriteState.PartiallyWritten, OutputWriteState.Completed), outcome.State)
        Assert.AreEqual(recording.Analysis.Tables.Count + 1, recording.NativeWriter.Blocks.Count)
        Assert.AreEqual(If(failChart, 1, 2), recording.ChartCalls)
        Assert.AreEqual(recording.Analysis.Tables.Count + 3, outcome.Intent.Regions.Count)
        If failChart Then
            Assert.AreEqual("execution", recording.Session.Stage)
            Assert.AreSame(recording.OriginalFailure, recording.Session.Failure)
        End If
    End Sub

    <TestMethod>
    Public Sub FullSharedOrchestration_InspectionFailureIsLoggedAndStaleConsentNeverWrites()
        Dim recording As New WorkflowRecordingHost("kw-dunn", SpreadsheetOutputDestination.CurrentWorksheet) With {.FailInspection = True}
        Dim outcome = recording.Execute()
        Assert.AreEqual(OutputWriteState.FailedBeforeWrite, outcome.State)
        Assert.AreSame(recording.OriginalFailure, recording.Session.Failure)
        Assert.AreEqual("occupancy-inspection", recording.Session.Stage)
        recording = New WorkflowRecordingHost("kw-dunn", SpreadsheetOutputDestination.CurrentWorksheet) With {.OccupiedId = "table-0", .Approved = True, .ChangeAfterConsent = True}
        outcome = recording.Execute()
        Assert.AreEqual(OutputWriteState.RejectedBeforeWrite, outcome.State)
        Assert.AreEqual(0, recording.NativeWriter.Blocks.Count)
        recording = New WorkflowRecordingHost("anova-base", SpreadsheetOutputDestination.CurrentWorksheet)
        recording.Host.Charts = Function(w, columns) New Action() {Nothing}
        outcome = recording.Execute()
        Assert.AreEqual(OutputWriteState.FailedBeforeWrite, outcome.State)
        Assert.AreEqual("action-validation", recording.Session.Stage)
        Assert.IsInstanceOfType(recording.Session.Failure, GetType(ArgumentException))
        Assert.AreEqual(0, recording.NativeWriter.Blocks.Count)
    End Sub

    <DataTestMethod>
    <DataRow("blank-scalar")> <DataRow("blank-matrix")> <DataRow("formula-scalar")> <DataRow("zero-based-malformed")>
    Public Sub NativeShapeRegression_ObservedScalarAndOneBasedMatrixResponses(kind As String)
        Dim scope As New WriteIntent("native", "target", Target(), {New WriteRegion("cell", New OutputRectangle(11, 6, 11, 6))})
        Dim snapshot As ExcelDnaOccupancySnapshot
        Select Case kind
            Case "blank-scalar" : snapshot = New ExcelDnaOccupancySnapshot(Nothing, String.Empty)
            Case "blank-matrix" : snapshot = NativeSnapshot(scope.Regions(0).Bounds, Nothing, Nothing)
            Case "formula-scalar" : snapshot = New ExcelDnaOccupancySnapshot(String.Empty, "=""""")
            Case Else : snapshot = New ExcelDnaOccupancySnapshot(New Object(0, 0) {}, New Object(0, 0) {})
        End Select
        If kind = "zero-based-malformed" Then
            Assert.ThrowsException(Of InvalidOperationException)(Sub() ExcelDnaOutputTargetAdapter.InspectWriteCore(scope, Sub(r)
                                                                                                                        End Sub, Function(b) snapshot))
        Else
            Dim observed = ExcelDnaOutputTargetAdapter.InspectWriteCore(scope, Sub(r)
                                                                            End Sub, Function(b) snapshot)
            Assert.AreEqual(If(kind = "formula-scalar", OutputOccupancy.Occupied, OutputOccupancy.Empty), observed.State)
        End If
    End Sub

    Private Shared Function NativeSnapshot(bounds As OutputRectangle, value As Object, formula As Object) As ExcelDnaOccupancySnapshot
        Dim shape = {bounds.EndRow - bounds.StartRow + 1, bounds.EndColumn - bounds.StartColumn + 1}
        Dim values = Array.CreateInstance(GetType(Object), shape, {1, 1})
        Dim formulas = Array.CreateInstance(GetType(Object), shape, {1, 1})
        values.SetValue(value, 1, 1) : formulas.SetValue(formula, 1, 1)
        Return New ExcelDnaOccupancySnapshot(values, formulas)
    End Function

    <DataTestMethod>
    <DataRow("empty")> <DataRow("number")> <DataRow("boolean")> <DataRow("error")>
    <DataRow("text")> <DataRow("empty-string")> <DataRow("formula")> <DataRow("blank-formula")>
    <DataRow("dbnull")> <DataRow("shape")>
    Public Sub NativeOccupancySeam_InspectsExactRectanglesAndDetectsBlankFormulas(kind As String)
        Dim destination = Target(), source = Models()
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        Dim scope = ExcelDnaSharedOutputPlanAdapter.CreateWriteIntent(prepared, Nothing, Nothing, Nothing, "Workbook", "Sheet1")
        Dim value As Object = Nothing, formula As Object = Nothing
        Select Case kind
            Case "number" : value = 0
            Case "boolean" : value = False
            Case "error" : value = New Runtime.InteropServices.ErrorWrapper(2023)
            Case "text" : value = "sentinel"
            Case "empty-string" : value = String.Empty
            Case "formula" : value = 2 : formula = "=1+1"
            Case "blank-formula" : value = String.Empty : formula = "="""""
            Case "dbnull" : value = DBNull.Value
        End Select
        Dim checks As New List(Of String), reads As New List(Of String)
        Dim inspect As Func(Of WriteInspection) = Function() ExcelDnaOutputTargetAdapter.InspectWriteCore(scope,
            Sub(region) checks.Add(region.Bounds.ToString()), Function(bounds)
                                                                 reads.Add(bounds.ToString())
                                                                 If bounds.StartRow <> 9 Then Return NativeSnapshot(bounds, Nothing, Nothing)
                                                                 If kind = "shape" Then Return New ExcelDnaOccupancySnapshot(New Object(1, 1) {}, Nothing)
                                                                 Return NativeSnapshot(bounds, value, If(formula, value))
                                                             End Function)
        If kind = "shape" OrElse kind = "dbnull" Then
            Dim prompts As Integer = 0, writes As Integer = 0
            Dim result = SafeWrite.Execute(scope, inspect, Function(o)
                                                              prompts += 1
                                                              Return True
                                                          End Function,
                scope.Regions.Select(Function(r) CType(Sub() writes += 1, Action)).ToArray())
            Assert.AreEqual(OutputWriteState.FailedBeforeWrite, result.State)
            Assert.AreEqual(0, prompts) : Assert.AreEqual(0, writes)
            Return
        End If
        Dim observed = inspect()
        CollectionAssert.AreEqual(scope.Regions.Select(Function(r) r.Bounds.ToString()).ToArray(), checks.ToArray())
        CollectionAssert.AreEqual(checks.ToArray(), reads.ToArray())
        Assert.AreEqual(If(kind = "empty", OutputOccupancy.Empty, OutputOccupancy.Occupied), observed.State)
        Assert.AreEqual(kind <> "empty", observed.ConfirmationRequired)
        Assert.AreEqual(kind = "formula" OrElse kind = "blank-formula", observed.HasFormulas)
        If kind <> "empty" Then CollectionAssert.AreEqual(New String() {"table-1"}, observed.OccupiedRegionIds.ToArray())
        Dim token = observed.ObservationIdentity
        value = "changed"
        Assert.AreNotEqual(token, inspect().ObservationIdentity)
    End Sub

    <TestMethod>
    Public Sub NativeOccupancySeam_RejectsObsoleteBroadChartCellGuards()
        Dim destination = Target(), source = Models()
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        Dim guard As New OutputRectangle(1, 1, 100, 20)
        Dim scope = ExcelDnaSharedOutputPlanAdapter.CreateWriteIntent(prepared, Nothing, {"chart"}, {guard}, "Workbook", "Sheet1")
        Dim reads As New List(Of String)
        Assert.ThrowsException(Of InvalidOperationException)(Sub() ExcelDnaOutputTargetAdapter.InspectWriteCore(scope, Sub(r)
                                                                                                                   End Sub, Function(bounds)
                                                                                                                                reads.Add(bounds.ToString())
                                                                                                                                Return NativeSnapshot(bounds, Nothing, Nothing)
                                                                                                                            End Function))
        Assert.AreEqual(0, reads.Count)
        Assert.IsNull(scope.Regions.Last().Bounds) : Assert.IsTrue(scope.Regions.Last().MayMutate)
        Assert.AreEqual(4, scope.Regions.Where(Function(r) r.Bounds IsNot Nothing).Count())
    End Sub

    <TestMethod>
    Public Sub NativeOccupancySeam_NonCellChartNeverAddsCellReadsOrOccupancy()
        Dim destination = Target(), source = Models()
        Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(OutputLayoutPlanner.Create(Document(source), destination), Requests(source, destination), destination, "Results")
        Dim scope = ExcelDnaSharedOutputPlanAdapter.CreateWriteIntent(prepared, Nothing, {"chart"}, Nothing, "Workbook", "Sheet1")
        Dim reads As New List(Of String)
        Dim observed = ExcelDnaOutputTargetAdapter.InspectWriteCore(scope, Sub(r)
                                                                          End Sub, Function(bounds)
                                                                                       reads.Add(bounds.ToString())
                                                                                       Return NativeSnapshot(bounds, Nothing, Nothing)
                                                                                   End Function)
        Assert.AreEqual(4, reads.Count) : Assert.AreEqual(OutputOccupancy.Empty, observed.State)
        Assert.AreEqual(0, observed.OccupiedRegionIds.Count)
        CollectionAssert.AreEqual(scope.Regions.Where(Function(r) r.Bounds IsNot Nothing).Select(Function(r) r.Bounds.ToString()).ToArray(), reads.ToArray())
    End Sub

    <TestMethod>
    Public Sub NativeOccupancySeam_QuotaRejectsBeforeAnyReadOrPrompt()
        Dim scope As New WriteIntent("plan", "target", Target(), {New WriteRegion("table", New OutputRectangle(6, 3, 100006, 3))})
        Dim observed = ExcelDnaOutputTargetAdapter.InspectWriteCore(scope, Sub(r) Assert.Fail("unexpected validation/read"),
            Function(b)
                Assert.Fail("oversized read")
                Return Nothing
            End Function)
        Assert.AreEqual(OutputOccupancy.Unknown, observed.State) : Assert.IsFalse(observed.ConfirmationRequired)
        StringAssert.Contains(observed.Message, "100,000-cell")
    End Sub

    Private Class RecordingWriter
        Inherits ExcelDnaResultWriter
        Public ReadOnly Written As New List(Of ExcelDnaSharedWriteCommand)
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Public ReadOnly Formats As New List(Of String)
        Public ReadOnly AutoFits As New List(Of Boolean)
        Public Property FailAt As Integer
        Public Sub New(row As Integer, column As Integer)
            MyBase.New(row, column)
        End Sub
        Public Sub Record(command As ExcelDnaSharedWriteCommand)
            Written.Add(command)
        End Sub
        Friend Overrides Sub WriteOutputBlockCore(block As ResultTableOutputBlock, numberFormat As String,
                                                  alpha As Double?, autoFit As Boolean, Optional plannedBounds As OutputRectangle = Nothing)
            Blocks.Add(block)
            Formats.Add(numberFormat) : AutoFits.Add(autoFit)
            If FailAt > 0 AndAlso Blocks.Count = FailAt Then Throw New InvalidOperationException("Injected formatter failure")
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
