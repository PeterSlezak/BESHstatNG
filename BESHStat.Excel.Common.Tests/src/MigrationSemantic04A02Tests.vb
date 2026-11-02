Option Explicit On
Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Semantic assertions come from source, never from self-generated numerical references.
<TestClass>
Public Class MigrationSemantic04A02Tests
    Private Shared Function RunCase(id As String) As ResultTableOutputModel
        Dim fixture = MigrationFixture04A02.Load(id)
        MigrationFixture04A02.Validate(fixture)
        Dim grouped = MigrationFixture04A02.Grouped(fixture)
        If id = "C30-SYN-ANOVA" Then
            Dim result = OneWayAnovaApplicationService.Run(grouped, New OneWayAnovaAnalysisOptions With {
                .Alpha = 0.05, .IncludeWelch = False, .IncludeFisherLsd = False,
                .IncludeBonferroni = False, .IncludeTukeyKramer = False, .IncludeGamesHowell = False})
            Assert.IsFalse(result.HasWelch)
            Assert.IsNull(result.WelchResult)
            Assert.IsTrue(Double.IsNaN(result.WelchPValue))
            Assert.IsTrue(Double.IsNaN(result.WelchFStatistic))
            Assert.IsNull(result.FisherLsdPlotData)
            Assert.IsNull(result.BonferroniPlotData)
            Assert.IsNull(result.TukeyKramerPlotData)
            Assert.IsNull(result.GamesHowellPlotData)
            Assert.AreEqual(1, result.Tables.Count)
            CollectionAssert.AreEqual(New String() {"A", "B", "C"}, result.GroupNames)
            CollectionAssert.AreEqual(New Integer() {3, 3, 3}, result.GroupCounts)
            Assert.AreEqual(result.FStatistic, CDbl(result.Table.Values(1, 4)))
            Assert.AreEqual(result.PValue, CDbl(result.Table.Values(1, 5)))
            Return result.Table
        End If
        Dim kw = KruskalWallisApplicationService.Run(grouped, New KruskalWallisAnalysisOptions With {
            .Alpha = 0.05, .IncludeDunnComparisons = False})
        Assert.AreEqual(1, kw.Tables.Count)
        CollectionAssert.AreEqual(New String() {"A", "B", "C"}, kw.GroupNames)
        CollectionAssert.AreEqual(New Integer() {3, 3, 3}, kw.GroupCounts)
        Assert.AreEqual(kw.HStatistic, CDbl(kw.Table.Values(3, 1)))
        Assert.AreEqual(kw.PValue, CDbl(kw.Table.Values(4, 1)))
        Assert.AreEqual(kw.TieCorrectedHStatistic, CDbl(kw.Table.Values(5, 1)))
        Assert.AreEqual(kw.TieCorrectedPValue, CDbl(kw.Table.Values(6, 1)))
        Assert.AreNotEqual(kw.HStatistic, kw.TieCorrectedHStatistic, "The frozen ties must not lose the two fields")
        Return kw.Table
    End Function

    Private Shared Sub ValidateModel(model As ResultTableOutputModel, id As String)
        Dim spec = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "04A02", "semantic-table-source-spec-v1.json")))
        Dim expected = spec("cases").AsArray().Single(Function(c) c("input_case_id").GetValue(Of String)() = id)("tables")(0)
        Assert.AreEqual(expected("rows").GetValue(Of Integer)(), model.RowCount)
        Assert.AreEqual(expected("columns").GetValue(Of Integer)(), model.ColumnCount)
        Assert.AreEqual(expected("header_left_columns").GetValue(Of Integer)(), model.HeaderLeftColumns)
        Assert.AreEqual(1, model.HeaderTopRows)
        Assert.AreEqual(0, model.TitleRows)
        Assert.AreEqual(0, model.FooterRows)
        Assert.IsTrue(model.IsResultTable)
        For col = 0 To model.ColumnCount - 1
            Assert.AreEqual(expected("header")(col).GetValue(Of String)(), CStr(model.Values(0, col)))
        Next
        For row = 1 To model.RowCount - 1
            Assert.AreEqual(expected("body_row_labels")(row - 1).GetValue(Of String)(), CStr(model.Values(row, 0)))
            For col = model.HeaderLeftColumns To model.ColumnCount - 1
                Dim value = model.Values(row, col)
                Assert.AreEqual(expected("cell_types")(row - 1)(col - model.HeaderLeftColumns).GetValue(Of String)(),
                    If(value Is Nothing, "null", value.GetType().FullName), $"cell {row},{col}")
            Next
        Next
        CollectionAssert.AreEqual(expected("pvalue_columns").AsArray().Select(Function(n) n.GetValue(Of Integer)()).ToArray(), model.PvalueColumns.ToArray())
        CollectionAssert.AreEqual(expected("pvalue_cells").AsArray().Select(Function(n) n(0).ToString() & ":" & n(1).ToString()).ToArray(),
            model.PvalueCells.Select(Function(c) c.BodyRow.ToString(CultureInfo.InvariantCulture) & ":" & c.BodyColumn.ToString(CultureInfo.InvariantCulture)).ToArray())
        If id = "C30-SYN-KW" Then
            Assert.AreEqual("ties un-corrected", CStr(model.Values(4, 2)))
            Assert.AreEqual("ties corrected", CStr(model.Values(6, 2)))
            Assert.AreEqual("", CStr(model.Values(3, 2)))
        End If
    End Sub

    <DataTestMethod>
    <DataRow("C30-SYN-ANOVA", "")> <DataRow("C30-SYN-ANOVA", "sk-SK")>
    <DataRow("C30-SYN-KW", "")> <DataRow("C30-SYN-KW", "sk-SK")>
    Public Sub RealServiceDocumentAndLayout_AreSemanticCandidates(id As String, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Dim savedUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture)
            Dim model = RunCase(id)
            ValidateModel(model, id)
            Dim table As New OutputTableArtifact("04A02-" & id & "-semantic", model, "CANDIDATE_ONLY")
            Dim warning As New OutputDiagnosticArtifact("warning", OutputDiagnosticSeverity.Warning, "04A02.mock.warning", "Test-only warning")
            Dim fault As New OutputDiagnosticArtifact("fault", OutputDiagnosticSeverity.Fault, "04A02.mock.fault", "Test-only fault")
            Dim document As New OutputDocument(New OutputArtifact() {table, warning, fault})
            Assert.AreEqual(OutputArtifactKind.Table, document.Artifacts(0).Kind)
            Assert.AreEqual("CANDIDATE_ONLY", document.Artifacts(0).Caption)
            Dim semantics = OutputSemanticPlanner.Create(document, Nothing, 0.05, culture)
            Assert.AreEqual(OutputStyleRole.DiagnosticWarning, semantics.Artifacts(1).DiagnosticRole.Value)
            Assert.AreEqual(OutputStyleRole.DiagnosticFault, semantics.Artifacts(2).DiagnosticRole.Value)
            Assert.AreEqual(model.RowCount * model.ColumnCount, semantics.Artifacts(0).Cells.Count)
            For Each cell In semantics.Artifacts(0).Cells
                Assert.AreEqual(If(cell.Row = 0, OutputStyleRole.TopColumnHeader,
                    If(cell.Column < model.HeaderLeftColumns, OutputStyleRole.LeftRowHeader, OutputStyleRole.Body)), cell.Role)
            Next
            Dim target As New SpreadsheetOutputTarget With {.StartRow = 6, .StartColumn = 3}
            Dim layout = OutputLayoutPlanner.Create(document, target, semanticPlan:=semantics)
            Assert.AreEqual(6, layout.Placements(0).Bounds.StartRow)
            Assert.AreEqual(3, layout.Placements(0).Bounds.StartColumn)
            Assert.AreEqual(5 + model.RowCount, layout.Placements(0).Bounds.EndRow)
            Assert.AreEqual(2 + model.ColumnCount, layout.Placements(0).Bounds.EndColumn)
            Assert.IsFalse(layout.Placements(1).IsPlaced)
            Assert.IsFalse(layout.Placements(2).IsPlaced)
            Dim copied = table.Model
            copied.Values(0, 0) = "mutated"
            ValidateModel(table.Model, id)
            Dim observation = New JsonObject From {
                {"schema", "04A02-G3-execution-candidate-v1"}, {"case_id", id},
                {"parent_case_id", "04A02-TABLE-SEMANTIC-01"}, {"culture", culture},
                {"classification", "candidate"}, {"status", "UNVERIFIED_AGAINST_INDEPENDENT_ORACLE"},
                {"service", MigrationFixture04A02.Observe(MigrationFixture04A02.Load(id))},
                {"semantics", JsonSerializer.SerializeToNode(semantics)},
                {"layout", JsonSerializer.SerializeToNode(layout)}}
            Dim text = MigrationFixture04A02.Canonical(observation)
            Assert.AreEqual(text, MigrationFixture04A02.Canonical(JsonNode.Parse(text)))
            Dim output = Environment.GetEnvironmentVariable("BESHSTAT_04A02_OBSERVATIONS")
            If Not String.IsNullOrEmpty(output) Then
                Dim path = System.IO.Path.Combine(output, "C05-G3-" & id & "-" & If(culture = "", "invariant", culture) & ".json")
                Directory.CreateDirectory(output)
                Using stream As New FileStream(path, FileMode.CreateNew, FileAccess.Write)
                    Using writer As New StreamWriter(stream, New System.Text.UTF8Encoding(False))
                        writer.Write(text & vbLf)
                    End Using
                End Using
            End If
        Finally
            CultureInfo.CurrentCulture = saved
            CultureInfo.CurrentUICulture = savedUi
        End Try
    End Sub

    <DataTestMethod>
    <DataRow("header")> <DataRow("row")> <DataRow("type")> <DataRow("null")>
    <DataRow("p-column")> <DataRow("p-cell")> <DataRow("annotation")> <DataRow("role")>
    Public Sub ExecutedTableMutation_IsRejected(defect As String)
        Dim id = If({"p-cell", "annotation"}.Contains(defect), "C30-SYN-KW", "C30-SYN-ANOVA")
        Dim model = RunCase(id)
        Select Case defect
            Case "header" : model.Values(0, 1) = "df"
            Case "row" : model.Values(1, 0) = "Within Groups"
            Case "type" : model.Values(1, 2) = "2"
            Case "null" : model.Values(2, 4) = ""
            Case "p-column" : model.PvalueColumns.Clear()
            Case "p-cell" : model.PvalueCells.Reverse()
            Case "annotation" : model.Values(4, 2) = "ties corrected"
            Case "role" : model = New ResultTableOutputModel(model.Values, headerTopRows:=1, headerLeftColumns:=0)
        End Select
        Assert.ThrowsException(Of AssertFailedException)(Sub() ValidateModel(model, id))
    End Sub

    <DataTestMethod>
    <DataRow("")> <DataRow("sk-SK")>
    Public Sub RealCoreTable_NullErrorsAndPDisplayAreAuthoredPolicies(culture As String)
        Dim core As New ResultTable()
        core.AddHeaderTopRow({"kind", "value"})
        core.SetBody(New Object(,) {{"null", Nothing}, {"empty", ""}, {"nan", Double.NaN},
            {"pos", Double.PositiveInfinity}, {"neg", Double.NegativeInfinity}, {"p", 0.00001R}})
        core.AddPvalueCellToFormat(6, 2)
        Dim model = core.ToOutputModel()
        Dim request = ResultTableWriteRequestFactory.Create(model, Nothing, 0.05, Nothing)
        Assert.IsNull(request.Values(1)(1))
        Assert.AreEqual("", request.Values(2)(1))
        Assert.AreEqual("#N/A", request.Values(3)(1))
        Assert.AreEqual("#Pinf", request.Values(4)(1))
        Assert.AreEqual("#Ninf", request.Values(5)(1))
        Assert.AreEqual(GetType(Double), request.Values(6)(1).GetType())
        Dim plan = OutputSemanticPlanner.Create(OutputDocument.FromTable(core), Nothing, 0.05, culture)
        Dim p = plan.Artifacts(0).Cells.Single(Function(c) c.Row = 6 AndAlso c.Column = 1)
        Assert.IsTrue(p.IsPValue)
        Assert.IsTrue(p.IsSignificant)
        Assert.AreEqual(OutputNumberFormatKind.PValue, p.NumberFormat.Kind)
        Assert.AreEqual(plan.Policy.Preview(0.00001R, CultureInfo.GetCultureInfo(culture)), p.DisplayPreview)
    End Sub

    <TestMethod>
    Public Sub RealDocumentRejectsDuplicateIdentity()
        Dim table = New OutputTableArtifact("same", RunCase("C30-SYN-KW"))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputDocument({table, table}))
    End Sub
End Class
