Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports BESHStatNG
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class OutputDocumentTests
    Public Property TestContext As TestContext

    <TestMethod>
    Public Sub CoreBridge_PreservesCompleteAssembledTable()
        Dim table As ResultTable = CompositeTable()
        Dim document As OutputDocument = OutputDocument.FromTable(table)
        Assert.AreEqual(1, document.Artifacts.Count)
        Dim artifact As OutputTableArtifact = DirectCast(document.Artifacts(0), OutputTableArtifact)
        Assert.AreEqual("table-0", artifact.Id)
        Assert.AreEqual(OutputArtifactKind.Table, artifact.Kind)
        Assert.AreEqual(String.Empty, artifact.Caption)
        AssertModelsEqual(table.ToOutputModel(), artifact.Model)
        Dim expected As Object(,) = {
            {"Title one", Nothing, Nothing},
            {"Title two", Nothing, Nothing},
            {Nothing, "Estimate", "P"},
            {Nothing, Nothing, "Second"},
            {"A", 0, 0.04R},
            {"B", Nothing, True},
            {"Footnote one", Nothing, Nothing},
            {"Footnote two", Nothing, Nothing}}
        AssertMatrixEqual(expected, artifact.Model.Values)
        Assert.AreEqual(2, artifact.Model.TitleRows)
        Assert.AreEqual(2, artifact.Model.HeaderTopRows)
        Assert.AreEqual(1, artifact.Model.HeaderLeftColumns)
        Assert.AreEqual(2, artifact.Model.FooterRows)
        Assert.AreEqual(8, artifact.RowCount)
        Assert.AreEqual(3, artifact.ColumnCount)
    End Sub

    <TestMethod>
    Public Sub CoreBridge_PreservesEmptyMinimalAndHeaderOnlyTables()
        Dim empty As New ResultTable()
        AssertMatrixEqual(New Object(,) {{Nothing}}, DirectCast(OutputDocument.FromTable(empty).Artifacts(0), OutputTableArtifact).Model.Values)
        Dim minimal As New ResultTable()
        minimal.SetBody(New Object(,) {{0}})
        AssertMatrixEqual(New Object(,) {{0}}, OutputTableArtifact.FromTable("minimal", minimal).Model.Values)
        Dim headerOnly As New ResultTable()
        headerOnly.AddTitle("Empty report")
        headerOnly.AddHeaderTopRow(New String() {"A", "B"})
        headerOnly.AddFootnote("No observations")
        Dim actual As ResultTableOutputModel = OutputTableArtifact.FromTable("header", headerOnly).Model
        AssertMatrixEqual(New Object(,) {{"Empty report", Nothing}, {"A", "B"}, {"No observations", Nothing}}, actual.Values)
        AssertModelsEqual(headerOnly.ToOutputModel(), actual)
    End Sub

    <TestMethod>
    Public Sub ModelBridge_PreservesZeroLengthDimensionsAndNonResultFlag()
        For Each shape As Integer() In New Integer()() {New Integer() {0, 0}, New Integer() {0, 3}, New Integer() {2, 0}}
            Dim values As Object(,) = DirectCast(Array.CreateInstance(GetType(Object), shape), Object(,))
            Dim model As New ResultTableOutputModel(values, isResultTable:=False)
            Dim actual As OutputTableArtifact = DirectCast(OutputDocument.FromModel(model).Artifacts(0), OutputTableArtifact)
            AssertModelsEqual(model, actual.Model)
            Assert.AreEqual(shape(0), actual.RowCount)
            Assert.AreEqual(shape(1), actual.ColumnCount)
            Assert.IsFalse(actual.Model.IsResultTable)
        Next
    End Sub

    <TestMethod>
    Public Sub CoreBridge_KeepsOneBasedBodyAnnotationsAndCoreFiltering()
        Dim table As ResultTable = CompositeTable()
        table.AddPvalueToFormat(2)
        table.AddPvalueToFormat(0)
        table.AddPvalueToFormat(3)
        table.AddPvalueCellToFormat(1, 2)
        table.AddPvalueCellToFormat(0, 1)
        table.AddPvalueCellToFormat(3, 2)
        Dim model As ResultTableOutputModel = OutputTableArtifact.FromTable("p", table).Model
        CollectionAssert.AreEqual(New Integer() {2}, model.PvalueColumns)
        CollectionAssert.AreEqual(New ResultTableCellAddress() {New ResultTableCellAddress(1, 2)}, model.PvalueCells)
        Assert.AreEqual(0.04R, model.Values(4, 2)) ' Annotation (1,2) stays body-relative.
    End Sub

    <TestMethod>
    Public Sub ModelBridge_PreservesUnvalidatedDtoAnnotationsExactly()
        ' Core DTO has no bounds/deduplication policy. Do not silently apply ResultTable's policy.
        Dim model As New ResultTableOutputModel(New Object(,) {{0.01R}},
            pvalueColumns:=New Integer() {1, 1, 0, -1, 99},
            pvalueCells:=New ResultTableCellAddress() {
                New ResultTableCellAddress(1, 1), New ResultTableCellAddress(1, 1),
                New ResultTableCellAddress(0, -2), New ResultTableCellAddress(99, 99)})
        AssertModelsEqual(model, New OutputTableArtifact("raw", model).Model)
    End Sub

    <TestMethod>
    Public Sub TableSnapshot_IsolatesInputReturnedMatricesAndMetadata()
        Dim model As ResultTableOutputModel = CompositeTable().ToOutputModel()
        Dim expected As ResultTableOutputModel = CompositeTable().ToOutputModel()
        Dim artifact As New OutputTableArtifact("captured", model, "Document caption")
        model.Values(0, 0) = "changed"
        model.PvalueColumns.Clear()
        model.PvalueCells.Clear()
        AssertModelsEqual(expected, artifact.Model)
        Dim returned As ResultTableOutputModel = artifact.Model
        returned.Values(0, 0) = "changed copy"
        returned.PvalueColumns.Add(999)
        returned.PvalueCells.Add(New ResultTableCellAddress(999, 999))
        AssertModelsEqual(expected, artifact.Model)
        Assert.AreEqual("Document caption", artifact.Caption)
        Assert.AreEqual("Title one", artifact.Model.Values(0, 0))
    End Sub

    <TestMethod>
    Public Sub CoreBridge_IsolatesLaterCoreBodyAndAnnotationMutations()
        Dim body As Object(,) = {{2.0R, 0.01R}}
        Dim table As New ResultTable()
        table.SetBody(body)
        table.AddPvalueToFormat(2)
        table.AddPvalueCellToFormat(1, 2)
        Dim artifact As OutputTableArtifact = OutputTableArtifact.FromTable("core", table)
        body(0, 0) = -100
        table.PvalColumns.Clear()
        table.PvalCells.Clear()
        Assert.AreEqual(2.0R, artifact.Model.Values(0, 0))
        CollectionAssert.AreEqual(New Integer() {2}, artifact.Model.PvalueColumns)
        Assert.AreEqual(1, artifact.Model.PvalueCells.Count)
    End Sub

    <TestMethod>
    Public Sub Cells_PreserveRawScalarTypesTextBlanksAndNonfiniteNumbers()
        Dim values As Object(,) = {{Nothing, String.Empty, "=1+1", "#N/A", True, False,
            CSByte(-1), CByte(1), CShort(-2), CUShort(2), 0, 3UI, -4L, 4UL,
            0.0F, 0.0R, 1.25D, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity,
            Single.NaN, Single.PositiveInfinity, Single.NegativeInfinity}}
        Dim model As New ResultTableOutputModel(values)
        AssertModelsEqual(model, New OutputTableArtifact("values", model).Model)
        Dim request As ResultTableWriteRequest = ResultTableWriteRequestFactory.Create(
            model, New PValuePresentationSettings(), 0.05, New SpreadsheetOutputTarget())
        Assert.AreEqual("#N/A", request.Values(0)(17)) ' Existing writer normalization remains separate.
        Assert.IsTrue(Double.IsNaN(DirectCast(New OutputTableArtifact("raw", model).Model.Values(0, 17), Double)))
    End Sub

    <TestMethod>
    Public Sub Cells_RejectUnsupportedObjectsWithoutStringCoercion()
        For Each value As Object In New Object() {
            New Object(), New List(Of Integer) From {1}, New Integer() {1},
            DBNull.Value, New DateTime(2026, 1, 1), "x"c, OutputArtifactKind.Table, New NoStringConversion()}
            Dim model As New ResultTableOutputModel(New Object(,) {{value}})
            Dim ex As ArgumentException = Assert.ThrowsException(Of ArgumentException)(Function() New OutputTableArtifact("invalid", model))
            StringAssert.Contains(ex.Message, value.GetType().FullName)
        Next
    End Sub

    <TestMethod>
    Public Sub Tables_RejectNullValuesMalformedSectionsAndNonzeroArrayBounds()
        Assert.ThrowsException(Of ArgumentNullException)(Function() New OutputTableArtifact("null", Nothing))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputTableArtifact("null-values", New ResultTableOutputModel(Nothing)))
        For Each model As ResultTableOutputModel In New ResultTableOutputModel() {
            New ResultTableOutputModel(New Object(,) {{1}}, titleRows:=-1),
            New ResultTableOutputModel(New Object(,) {{1}}, headerTopRows:=-1),
            New ResultTableOutputModel(New Object(,) {{1}}, headerLeftColumns:=-1),
            New ResultTableOutputModel(New Object(,) {{1}}, footerRows:=-1),
            New ResultTableOutputModel(New Object(,) {{1}}, titleRows:=1, headerTopRows:=1),
            New ResultTableOutputModel(New Object(,) {{1}}, headerLeftColumns:=2),
            New ResultTableOutputModel(New Object(,) {{1}}, titleRows:=Integer.MaxValue, footerRows:=Integer.MaxValue)}
            Assert.ThrowsException(Of ArgumentException)(Function() New OutputTableArtifact("invalid", model))
        Next
        Dim nonzero As Object(,) = DirectCast(Array.CreateInstance(GetType(Object), New Integer() {1, 1}, New Integer() {1, 1}), Object(,))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputTableArtifact("bounds", New ResultTableOutputModel(nonzero)))
    End Sub

    <TestMethod>
    Public Sub Document_RejectsNullsDuplicateIdsAndInvalidArtifacts()
        Assert.ThrowsException(Of ArgumentNullException)(Function() New OutputDocument(Nothing))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputDocument(New OutputArtifact() {Nothing}))
        Dim plot As New OutputPlotArtifact("plot", "reference", "Purpose")
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputDocument(New OutputArtifact() {plot, plot}))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputPlotArtifact(" ", "reference", "Purpose"))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputPlotArtifact("p", "", "Purpose"))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputPlotArtifact("p", "ref", ""))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputDiagnosticArtifact("d", OutputDiagnosticSeverity.Warning, "", "Message"))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputDiagnosticArtifact("d", OutputDiagnosticSeverity.Fault, "code", ""))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputDiagnosticArtifact("d", CType(999, OutputDiagnosticSeverity), "code", "Message"))
        Assert.ThrowsException(Of ArgumentNullException)(Function() OutputDocument.FromTable(Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(Function() OutputDocument.FromModel(Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(Function() OutputDocument.FromResult(Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(Function() OutputDocument.FromAnalysis(Nothing))
    End Sub

    <TestMethod>
    Public Sub AnalysisBridge_OrdersTablesWarningsFaultsAndExplicitPlot()
        Dim source As New AnalysisResultDocument("generic.report",
            New ResultTableOutputModel() {CompositeTable().ToOutputModel(), New ResultTableOutputModel(New Object(,) {{"Second"}})},
            New Dictionary(Of String, Double) From {{"metric", 2.0R}}, AnalysisDetailArtifact.FromValue(New NoStringConversion()),
            New AnalysisWarning() {New AnalysisWarning("w1", "Warning one", "input.a"), New AnalysisWarning("w2", "Warning two", "input.b")},
            New AnalysisFault() {New AnalysisFault("f1", "Fault one", "model.a"), New AnalysisFault("f2", "Fault two", "model.b")})
        Dim plot As New OutputPlotArtifact("plot", "residual-reference", "Diagnostic reference", "Residuals", "Placeholder only")
        Dim document As OutputDocument = OutputDocument.FromAnalysis(source, New OutputArtifact() {plot})
        CollectionAssert.AreEqual(New String() {"table-0", "table-1", "warning-0", "warning-1", "fault-0", "fault-1", "plot"},
                                  document.Artifacts.Select(Function(a) a.Id).ToArray())
        For i As Integer = 0 To 1
            Dim warning As OutputDiagnosticArtifact = DirectCast(document.Artifacts(2 + i), OutputDiagnosticArtifact)
            Assert.AreEqual(OutputDiagnosticSeverity.Warning, warning.Severity)
            Assert.AreEqual(source.Warnings(i).Code, warning.Code)
            Assert.AreEqual(source.Warnings(i).Message, warning.Message)
            Assert.AreEqual(source.Warnings(i).FieldPath, warning.FieldPath)
            Dim fault As OutputDiagnosticArtifact = DirectCast(document.Artifacts(4 + i), OutputDiagnosticArtifact)
            Assert.AreEqual(OutputDiagnosticSeverity.Fault, fault.Severity)
            Assert.AreEqual(source.Faults(i).Code, fault.Code)
            Assert.AreEqual(source.Faults(i).Message, fault.Message)
            Assert.AreEqual(source.Faults(i).FieldPath, fault.FieldPath)
        Next
        Assert.AreSame(plot, document.Artifacts(6))
        Assert.AreEqual("residual-reference", plot.ReferenceKey)
        Assert.AreEqual("Diagnostic reference", plot.Purpose)
        Assert.AreEqual("Residuals", plot.Caption)
        Assert.AreEqual("Placeholder only", plot.Description)
        Assert.ThrowsException(Of ArgumentException)(Function() OutputDocument.FromAnalysis(source, New OutputArtifact() {
            New OutputPlotArtifact("table-0", "ref", "purpose")}))
        Assert.ThrowsException(Of ArgumentException)(Function() OutputDocument.FromAnalysis(source, New OutputArtifact() {Nothing}))
    End Sub

    <TestMethod>
    Public Sub GenericComposite_PreservesExplicitInterleavingAndReadOnlyCollection()
        Dim items As New List(Of OutputArtifact)()
        For i As Integer = 0 To 6
            Dim key As String = i.ToString(CultureInfo.InvariantCulture)
            items.Add(New OutputTableArtifact("section-" & key, New ResultTableOutputModel(New Object(,) {{"Section " & key, i}}), "Caption " & key))
            items.Add(New OutputDiagnosticArtifact("note-" & key, If(i Mod 2 = 0, OutputDiagnosticSeverity.Warning, OutputDiagnosticSeverity.Fault), "report.note", "Message " & key, "section." & key))
            items.Add(New OutputPlotArtifact("graphic-" & key, "reference-" & key, "Supplementary description"))
        Next
        Dim expected As OutputArtifact() = items.ToArray()
        Dim document As New OutputDocument(items)
        items.Clear()
        CollectionAssert.AreEqual(expected, document.Artifacts.ToArray())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(document.Artifacts, IList(Of OutputArtifact)).Clear())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(document.Artifacts, IList(Of OutputArtifact))(0) = expected(1))
        Assert.AreEqual(21, document.Artifacts.Count)
    End Sub

    <TestMethod>
    Public Sub EmptyFailureOnlyAndPlotOnlyDocuments_AreSupported()
        Assert.AreEqual(0, New OutputDocument(Array.Empty(Of OutputArtifact)()).Artifacts.Count)
        Dim failure As AnalysisResultDocument = AnalysisResultDocument.Failure("generic.failed", New AnalysisFault("failed", "No result", "input"))
        Dim document As OutputDocument = OutputDocument.FromAnalysis(failure)
        Assert.AreEqual(1, document.Artifacts.Count)
        Assert.AreEqual(OutputDiagnosticSeverity.Fault, DirectCast(document.Artifacts(0), OutputDiagnosticArtifact).Severity)
        Assert.AreEqual("No result", DirectCast(document.Artifacts(0), OutputDiagnosticArtifact).Message)
        Dim plot As New OutputPlotArtifact("only-plot", "logical-reference", "Purpose")
        Assert.AreEqual(1, New OutputDocument(New OutputArtifact() {plot}).Artifacts.Count)
        Dim empty As New AnalysisResultDocument("empty", Array.Empty(Of ResultTableOutputModel)(), New Dictionary(Of String, Double)(), Nothing)
        Assert.AreEqual(0, OutputDocument.FromAnalysis(empty).Artifacts.Count)
    End Sub

    <DataTestMethod>
    <DataRow("anova-base")>
    <DataRow("anova-welch-tukey")>
    <DataRow("anova-all-posthoc")>
    <DataRow("kw-base")>
    <DataRow("kw-dunn")>
    Public Sub FiveGoldenResults_PreserveEveryCellMetadataAndSourceOrder(name As String)
        Dim result As SpreadsheetAnalysisResult = RunFixture(name)
        Dim before As String = JsonSerializer.Serialize(result.Document.ToContractSnapshot())
        Dim document As OutputDocument = OutputDocument.FromResult(result)
        Assert.AreEqual(result.Tables.Count, document.Artifacts.Count, name)
        For i As Integer = 0 To result.Tables.Count - 1
            Dim artifact As OutputTableArtifact = DirectCast(document.Artifacts(i), OutputTableArtifact)
            Assert.AreEqual("table-" & i.ToString(CultureInfo.InvariantCulture), artifact.Id)
            AssertModelsEqual(result.Tables(i), artifact.Model)
        Next
        Assert.AreEqual(before, JsonSerializer.Serialize(result.Document.ToContractSnapshot()))
        Assert.AreEqual(1, result.Document.ToContractSnapshot().SchemaVersion)
        Assert.IsFalse(before.Contains("Values", StringComparison.Ordinal))
    End Sub

    <TestMethod>
    Public Sub ResultBridge_UsesExistingEnvelopeAndOriginalFourArgumentConstructor()
        Dim tables As New List(Of ResultTableOutputModel) From {New ResultTableOutputModel(New Object(,) {{"original"}})}
        Dim metrics As New Dictionary(Of String, Double) From {{"custom", 1.0R}}
        Dim detail As New Object()
        Dim result As New SpreadsheetAnalysisResult(SpreadsheetAnalysisKind.OneWayAnova, tables, metrics, detail)
        Assert.AreSame(tables, result.Tables)
        Assert.AreSame(metrics, result.Metrics)
        Assert.AreSame(detail, result.Detail)
        Dim envelope As AnalysisResultDocument = result.Document
        tables(0).Values(0, 0) = "later legacy edit"
        Assert.AreEqual("original", DirectCast(OutputDocument.FromResult(result).Artifacts(0), OutputTableArtifact).Model.Values(0, 0))
        Assert.AreSame(envelope, result.Document)
    End Sub

    <TestMethod>
    Public Sub PublicBoundary_UsesPortableDependenciesAndNoWritableArtifactProperties()
        Dim assembly As Reflection.Assembly = GetType(OutputDocument).Assembly
        For Each reference As Reflection.AssemblyName In assembly.GetReferencedAssemblies()
            Assert.IsFalse(New String() {"ExcelDna", "Microsoft.Office", "System.Windows.Forms", "System.Web", "Microsoft.JSInterop"}.
                Any(Function(prefix) reference.Name.StartsWith(prefix, StringComparison.Ordinal)), reference.FullName)
        Next
        For Each contract As Type In New Type() {GetType(OutputDocument), GetType(OutputArtifact), GetType(OutputTableArtifact), GetType(OutputDiagnosticArtifact), GetType(OutputPlotArtifact)}
            For Each prop As Reflection.PropertyInfo In contract.GetProperties()
                Assert.IsFalse(prop.CanWrite, contract.Name & "." & prop.Name)
            Next
            For Each method As Reflection.MethodInfo In contract.GetMethods(Reflection.BindingFlags.Public Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Static Or Reflection.BindingFlags.DeclaredOnly)
                For Each parameter As Reflection.ParameterInfo In method.GetParameters()
                    Assert.IsFalse(parameter.ParameterType.FullName.Contains("Microsoft.Office", StringComparison.Ordinal))
                Next
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub AnovaMultiTablePreview_IsDeterministicAndIncludesActualTitleFooterCells()
        Dim result As SpreadsheetAnalysisResult = RunFixture("anova-all-posthoc")
        Dim source As New AnalysisResultDocument(result.Document.AnalysisId, result.Tables, result.Metrics, Nothing,
            warnings:=New AnalysisWarning() {New AnalysisWarning("preview.example", "Read-only preview diagnostic", "preview")})
        Dim document As OutputDocument = OutputDocument.FromAnalysis(source, New OutputArtifact() {
            New OutputPlotArtifact("preview-plot", "anova-plot-reference", "Illustrative placeholder", "Plot placeholder")})
        Dim preview As String = BuildPreview(document)
        Assert.AreEqual(preview, BuildPreview(OutputDocument.FromAnalysis(source, New OutputArtifact() {
            New OutputPlotArtifact("preview-plot", "anova-plot-reference", "Illustrative placeholder", "Plot placeholder")})))
        Assert.AreEqual(result.Tables.Count, document.Artifacts.OfType(Of OutputTableArtifact)().Count())
        ' Existing ANOVA captions are top-header cells, not Core TitleRows. Preserve that distinction.
        Assert.IsTrue(result.Tables.All(Function(t) t.TitleRows = 0))
        StringAssert.Contains(preview, "header[0]=Source of Variation")
        StringAssert.Contains(BuildPreview(OutputDocument.FromTable(CompositeTable())), "title[0]=Title one")
        Assert.IsTrue(preview.Contains("footer[", StringComparison.Ordinal))
        StringAssert.Contains(preview, "Warning preview.example: Read-only preview diagnostic (preview)")
        StringAssert.Contains(preview, "Plot placeholder | reference=anova-plot-reference")
        TestContext.WriteLine(preview)
    End Sub

    Private Shared Function CompositeTable() As ResultTable
        Dim table As New ResultTable()
        table.SetBody(New Object(,) {{0, 0.04R}, {Nothing, True}})
        table.AddTitle("Title one")
        table.AddTitle("Title two")
        table.AddHeaderTopRow(New String() {"Estimate", "P"})
        table.AddHeaderTopRow(New String() {"Second"})
        table.AddHeaderLeftRow(New String() {"A", "B"})
        table.AddFootnote("Footnote one")
        table.AddFootnote("Footnote two")
        table.AddPvalueToFormat(2)
        table.AddPvalueCellToFormat(1, 2)
        Return table
    End Function

    Private Shared Sub AssertModelsEqual(expected As ResultTableOutputModel, actual As ResultTableOutputModel)
        AssertMatrixEqual(expected.Values, actual.Values)
        Assert.AreEqual(expected.RowCount, actual.RowCount)
        Assert.AreEqual(expected.ColumnCount, actual.ColumnCount)
        Assert.AreEqual(expected.TitleRows, actual.TitleRows)
        Assert.AreEqual(expected.HeaderTopRows, actual.HeaderTopRows)
        Assert.AreEqual(expected.HeaderLeftColumns, actual.HeaderLeftColumns)
        Assert.AreEqual(expected.FooterRows, actual.FooterRows)
        Assert.AreEqual(expected.IsResultTable, actual.IsResultTable)
        CollectionAssert.AreEqual(expected.PvalueColumns, actual.PvalueColumns)
        CollectionAssert.AreEqual(expected.PvalueCells, actual.PvalueCells)
    End Sub

    Private Shared Sub AssertMatrixEqual(expected As Object(,), actual As Object(,))
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0))
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1))
        For r As Integer = 0 To expected.GetLength(0) - 1
            For c As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(r, c), actual(r, c), "Cell " & r.ToString(CultureInfo.InvariantCulture) & "," & c.ToString(CultureInfo.InvariantCulture))
                If expected(r, c) IsNot Nothing Then Assert.AreEqual(expected(r, c).GetType(), actual(r, c).GetType())
            Next
        Next
    End Sub

    Private Shared Function RunFixture(name As String) As SpreadsheetAnalysisResult
        Using json As JsonDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "Stage304", name & ".json")))
            Dim fixture As JsonElement = json.RootElement
            Dim group As JsonElement = fixture.GetProperty("groups")
            Dim names As String() = group.GetProperty("names").EnumerateArray().Select(Function(v) v.GetString()).ToArray()
            Dim values As Double()() = group.GetProperty("values").EnumerateArray().Select(Function(g) g.EnumerateArray().Select(Function(v) v.GetDouble()).ToArray()).ToArray()
            Dim input As New GroupedNumericData(names, values)
            Dim id As String = fixture.GetProperty("analysisIdProposed").GetString()
            Dim raw As JsonElement = fixture.GetProperty("options")
            Dim options As SpreadsheetAnalysisOptions
            If id = AnalysisIds.OneWayAnova Then
                options = New OneWayAnovaAnalysisOptions With {
                    .IncludeWelch = Flag(raw, "welch"), .IncludeFisherLsd = Flag(raw, "fisherLsd"),
                    .IncludeBonferroni = Flag(raw, "bonferroni"), .IncludeTukeyKramer = Flag(raw, "tukeyKramer"),
                    .IncludeGamesHowell = Flag(raw, "gamesHowell")}
            Else
                options = New KruskalWallisAnalysisOptions With {.IncludeDunnComparisons = Flag(raw, "dunn")}
            End If
            Return SharedAnalysisApplicationService.Run(id, input, options)
        End Using
    End Function

    Private Shared Function Flag(raw As JsonElement, name As String) As Boolean
        Dim value As JsonElement
        Return raw.TryGetProperty(name, value) AndAlso value.GetBoolean()
    End Function

    ' Test-only read-only diagnostic text, not a production renderer or serialized document schema.
    Private Shared Function BuildPreview(document As OutputDocument) As String
        Dim text As New StringBuilder()
        For Each artifact As OutputArtifact In document.Artifacts
            text.Append(artifact.Id).Append(" | ").Append(artifact.Kind.ToString()).Append(" | ")
            If TypeOf artifact Is OutputTableArtifact Then
                Dim table As OutputTableArtifact = DirectCast(artifact, OutputTableArtifact)
                Dim model As ResultTableOutputModel = table.Model
                text.Append(table.Caption).Append(" | ").Append(table.RowCount.ToString(CultureInfo.InvariantCulture)).Append("x").Append(table.ColumnCount.ToString(CultureInfo.InvariantCulture)).Append(vbLf)
                For r As Integer = 0 To model.TitleRows - 1
                    text.Append("  title[").Append(r.ToString(CultureInfo.InvariantCulture)).Append("]=").Append(RowText(model.Values, r)).Append(vbLf)
                Next
                For r As Integer = model.TitleRows To model.TitleRows + model.HeaderTopRows - 1
                    text.Append("  header[").Append(r.ToString(CultureInfo.InvariantCulture)).Append("]=").Append(RowText(model.Values, r)).Append(vbLf)
                Next
                For r As Integer = model.RowCount - model.FooterRows To model.RowCount - 1
                    text.Append("  footer[").Append(r.ToString(CultureInfo.InvariantCulture)).Append("]=").Append(RowText(model.Values, r)).Append(vbLf)
                Next
            ElseIf TypeOf artifact Is OutputDiagnosticArtifact Then
                Dim diagnostic As OutputDiagnosticArtifact = DirectCast(artifact, OutputDiagnosticArtifact)
                text.Append(diagnostic.Severity.ToString()).Append(" ").Append(diagnostic.Code).Append(": ").Append(diagnostic.Message).Append(" (").Append(diagnostic.FieldPath).Append(")").Append(vbLf)
            Else
                Dim plot As OutputPlotArtifact = DirectCast(artifact, OutputPlotArtifact)
                text.Append(plot.Caption).Append(" | reference=").Append(plot.ReferenceKey).Append(" | purpose=").Append(plot.Purpose).Append(vbLf)
            End If
        Next
        Return text.ToString()
    End Function

    Private Shared Function RowText(values As Object(,), row As Integer) As String
        Dim cells As New List(Of String)()
        For c As Integer = 0 To values.GetLength(1) - 1
            cells.Add(If(values(row, c) Is Nothing, "<blank>", Convert.ToString(values(row, c), CultureInfo.InvariantCulture)))
        Next
        Return String.Join(" | ", cells)
    End Function

    Private NotInheritable Class NoStringConversion
        Public Overrides Function ToString() As String
            Throw New InvalidOperationException("Must not stringify arbitrary objects.")
        End Function
    End Class
End Class
