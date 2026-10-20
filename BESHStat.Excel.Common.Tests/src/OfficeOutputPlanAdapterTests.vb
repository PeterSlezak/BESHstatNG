Option Explicit On
Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Text.Json
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class OfficeOutputPlanAdapterTests
    Private Shared ReadOnly WireOptions As New JsonSerializerOptions With {.PropertyNamingPolicy = JsonNamingPolicy.CamelCase, .WriteIndented = True}

    Private Shared Function Target() As SpreadsheetOutputTarget
        Return New SpreadsheetOutputTarget With {.Destination = SpreadsheetOutputDestination.CurrentWorksheet, .StartRow = 6, .StartColumn = 3}
    End Function

    Private Shared Function Prepare(document As OutputDocument, Optional autoFit As Boolean = True, Optional gap As Integer = 1,
                                    Optional culture As String = Nothing) As OfficeOutputPlan
        Dim semantics = OutputSemanticPlanner.Create(document, Nothing, 0.05, culture)
        Return OfficeOutputPlanAdapter.Prepare(document, semantics,
            OutputLayoutPlanner.Create(document, Target(), New OutputLayoutOptions(gap), semanticPlan:=semantics), autoFit)
    End Function

    <DataTestMethod>
    <DataRow("anova-base")> <DataRow("anova-welch-tukey")> <DataRow("anova-all-posthoc")>
    <DataRow("kw-base")> <DataRow("kw-dunn")> <DataRow("four-tables-c6")>
    <DataRow("mixed-zero")> <DataRow("typed-roles")>
    Public Sub ActualInteropJson_MatchesReviewedWireAndCommonPlans(name As String)
        Dim document = FixtureDocument(name)
        Dim semantics = OutputSemanticPlanner.Create(document, Nothing, 0.05)
        Dim layout = OutputLayoutPlanner.Create(document, Target(), semanticPlan:=semantics)
        Dim prepared = OfficeOutputPlanAdapter.Prepare(document, semantics, layout)
        Dim json As String = JsonSerializer.Serialize(prepared, WireOptions)
        Dim actualDirectory = Path.Combine(AppContext.BaseDirectory, "../../../../TestResults/Codex/Batch0205/Wire")
        Directory.CreateDirectory(actualDirectory)
        File.WriteAllText(Path.Combine(actualDirectory, name & ".json"), json & Environment.NewLine)
        Dim expectedPath = Path.Combine(AppContext.BaseDirectory, "TestData/OfficeOutput", name & ".json")
        Assert.IsTrue(File.Exists(expectedPath), "New wire reference missing; inspect generated actual: " & actualDirectory)
        Assert.AreEqual(File.ReadAllText(expectedPath).TrimEnd().Replace(vbCrLf, vbLf), json.Replace(vbCrLf, vbLf))
        Using parsed = JsonDocument.Parse(json)
            Assert.AreEqual(1, parsed.RootElement.GetProperty("schemaVersion").GetInt32())
            Assert.AreEqual(JsonValueKind.Number, parsed.RootElement.GetProperty("target").GetProperty("destination").ValueKind)
            Assert.IsFalse(parsed.RootElement.TryGetProperty("SchemaVersion", Nothing))
        End Using
        Assert.AreEqual(layout.Placements.Count, prepared.ArtifactCount)
        For Each table In prepared.Tables
            Dim block = layout.Placements(table.DocumentIndex)
            Assert.AreSame(block.Bounds, table.Bounds)
            Dim source = semantics.Artifacts(table.DocumentIndex)
            Assert.AreEqual(source.Cells.Count, table.Cells.Count)
            For index As Integer = 0 To table.Cells.Count - 1
                Dim expected = source.Cells(index)
                Dim actual = table.Cells(index)
                Assert.AreEqual(expected.Role.ToString(), actual.Role)
                Assert.AreEqual(expected.IsSignificant, actual.IsSignificant)
                Assert.AreEqual(expected.IsPValue, actual.IsPValue)
                Assert.AreEqual(expected.StrongEmphasis, actual.StrongEmphasis)
                Assert.AreEqual(expected.Row, actual.Row)
                Assert.AreEqual(expected.Column, actual.Column)
                Assert.AreEqual(JsonSerializer.Serialize(OfficeOutputValue.FromCell(expected.Cell)), JsonSerializer.Serialize(actual.Value))
            Next
        Next
        ' Preview culture cannot affect wire values, positions, flags, or resolved Excel formats.
        Assert.AreEqual(json, JsonSerializer.Serialize(Prepare(document, culture:="en-US"), WireOptions))
        Assert.AreEqual(json, JsonSerializer.Serialize(Prepare(document, culture:="sk-SK"), WireOptions))
        If name.StartsWith("anova", StringComparison.Ordinal) OrElse name.StartsWith("kw", StringComparison.Ordinal) Then
            Using accepted = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData/OutputSemantics", name & ".json")))
                Dim artifacts = accepted.RootElement.GetProperty("artifacts").EnumerateArray().ToArray()
                For Each table In prepared.Tables
                    Dim expectedCells = artifacts(table.DocumentIndex).GetProperty("cells").EnumerateArray().ToArray()
                    For i As Integer = 0 To table.Cells.Count - 1
                        Assert.AreEqual(expectedCells(i).GetProperty("role").GetString(), table.Cells(i).Role)
                        Assert.AreEqual(expectedCells(i).GetProperty("significant").GetBoolean(), table.Cells(i).IsSignificant)
                    Next
                Next
            End Using
        End If
    End Sub

    <TestMethod>
    Public Sub SourceProvenance_RejectsStaleSemanticsAndLayoutEvenWithIdenticalIdsShapeAndValues()
        Dim original = FixtureDocument("typed-roles")
        Dim other = FixtureDocument("typed-roles")
        Dim semantics = OutputSemanticPlanner.Create(original, Nothing, 0.05)
        Dim layout = OutputLayoutPlanner.Create(original)
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeOutputPlanAdapter.Prepare(other, semantics, layout))
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeOutputPlanAdapter.Prepare(original, semantics, OutputLayoutPlanner.Create(other)))
        Assert.ThrowsException(Of ArgumentNullException)(Function() OfficeOutputPlanAdapter.Prepare(original, Nothing, layout))
    End Sub

    <TestMethod>
    Public Sub ZeroAreaAndNonspatial_KeepIdentityWithoutGapsAndAllowExplicitSpacing()
        Dim prepared = Prepare(FixtureDocument("mixed-zero"), autoFit:=False, gap:=3)
        Assert.AreEqual(7, prepared.ArtifactCount)
        Assert.AreEqual(4, prepared.SourceTableCount)
        Assert.AreEqual(2, prepared.TableCount)
        CollectionAssert.AreEqual(New Integer() {1, 3}, prepared.Tables.Select(Function(t) t.TableOrdinal).ToArray())
        CollectionAssert.AreEqual(New Integer() {2, 6}, prepared.Tables.Select(Function(t) t.DocumentIndex).ToArray())
        CollectionAssert.AreEqual(New Integer() {6, 11}, prepared.Tables.Select(Function(t) t.Bounds.StartRow).ToArray())
        Assert.IsTrue(prepared.Tables.All(Function(t) Not t.AutoFit))
        Dim empty = Prepare(New OutputDocument({New OutputPlotArtifact("p", "ref", "purpose")}))
        Assert.AreEqual(0, empty.TableCount)
        Assert.IsNull(empty.UsedBounds)
    End Sub

    <TestMethod>
    Public Sub WireIsImmutableAndCopiedFromMutableSourceAndTarget()
        Dim raw As New ResultTableOutputModel(New Object(,) {{1, "=1+1", "", Nothing, False}})
        Dim table As New OutputTableArtifact("t", raw)
        Dim doc As New OutputDocument({table})
        Dim semantic = OutputSemanticPlanner.Create(doc, Nothing, 0.05)
        Dim targetValue = Target()
        Dim layout = OutputLayoutPlanner.Create(doc, targetValue)
        Dim prepared = OfficeOutputPlanAdapter.Prepare(doc, semantic, layout)
        Dim before = JsonSerializer.Serialize(prepared, WireOptions)
        raw.Values(0, 0) = 999
        targetValue.StartRow = 900
        table.Model.Values(0, 0) = 333
        Assert.AreEqual(before, JsonSerializer.Serialize(prepared, WireOptions))
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(prepared.Tables, IList(Of OfficeOutputTable)).Clear())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(prepared.Tables(0).Cells, IList(Of OfficeOutputCell)).Clear())
        For Each type In New Type() {GetType(OfficeOutputPlan), GetType(OfficeOutputTarget), GetType(OfficeOutputTable), GetType(OfficeOutputCell), GetType(OfficeOutputValue)}
            Assert.IsTrue(type.GetProperties().All(Function(p) Not p.CanWrite))
        Next
    End Sub

    <DataTestMethod>
    <DataRow("0.050000000000000003")> <DataRow("9007199254740993")> <DataRow("18446744073709551615")>
    <DataRow("0.1")> <DataRow("79228162514264337593543950335")>
    Public Sub InexactDecimalTransport_ExplicitlyRefusesWithoutRounding(value As String)
        Dim exact = Decimal.Parse(value, CultureInfo.InvariantCulture)
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeOutputValue.FromCell(OutputCell.DecimalCell(exact)))
        Assert.ThrowsException(Of ArgumentException)(Function() Prepare(New OutputDocument({New OutputTableArtifact("t", New ResultTableOutputModel(New Object(,) {{exact}}))})))
    End Sub

    <TestMethod>
    Public Sub ExactDecimalImmediatelyAboveAlpha_RetainsCommonDecisionThenRefusesLossyTransport()
        Dim exact = OutputCell.DecimalCell(0.050000000000000003D)
        Assert.IsFalse(exact.IsSignificantAt(0.05))
        Assert.IsTrue(CDbl(exact.DecimalValue.Value) <= 0.05, "This is the lossy transport trap.")
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeOutputValue.FromCell(exact))
        For Each value As Decimal In New Decimal() {0D, -1D, 0.5D, 9007199254740992D, 9223372036854775808D}
            Assert.AreEqual(CDbl(value), OfficeOutputValue.FromCell(OutputCell.DecimalCell(value)).NumberValue.Value)
        Next
    End Sub

    <TestMethod>
    Public Sub FiniteBinary64Transport_PreservesBitsThroughActualJsonIncludingSignedZero()
        For Each value As Double In New Double() {0.0, -0.0R, Double.Epsilon, Double.MaxValue, -Double.MaxValue, 0.05}
            Dim projected = OfficeOutputValue.FromCell(OutputCell.NumberCell(value))
            Using parsed = JsonDocument.Parse(JsonSerializer.Serialize(projected, WireOptions))
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(value), BitConverter.DoubleToInt64Bits(parsed.RootElement.GetProperty("numberValue").GetDouble()))
            End Using
        Next
    End Sub

    <DataTestMethod>
    <DataRow("nan")> <DataRow("infinity")> <DataRow("negative-infinity")>
    <DataRow("standard-error")> <DataRow("unknown-error")> <DataRow("excel-formula")> <DataRow("unknown-formula")>
    Public Sub UnsupportedErrorAndFormulaIntent_IsDeterministicallyRejected(kind As String)
        Dim cell As OutputCell
        Select Case kind
            Case "nan" : cell = OutputCell.FromRaw(Double.NaN)
            Case "infinity" : cell = OutputCell.FromRaw(Double.PositiveInfinity)
            Case "negative-infinity" : cell = OutputCell.FromRaw(Double.NegativeInfinity)
            Case "standard-error" : cell = OutputCell.SpreadsheetError("#N/A")
            Case "unknown-error" : cell = OutputCell.SpreadsheetError("#FUTURE!")
            Case "excel-formula" : cell = OutputCell.TrustedFormula("=1+1", "excel-a1")
            Case Else : cell = OutputCell.TrustedFormula("=1+1", "unknown")
        End Select
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeOutputValue.FromCell(cell))
    End Sub

    <TestMethod>
    Public Sub BoundaryAndResourceGuards_RefuseUnsupportedOutput()
        Dim document = FixtureDocument("four-tables-c6")
        Dim semantics = OutputSemanticPlanner.Create(document, Nothing, 0.05)
        Dim targetValue = Target()
        targetValue.Destination = SpreadsheetOutputDestination.NewWorkbook
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeOutputPlanAdapter.Prepare(document, semantics, OutputLayoutPlanner.Create(document, targetValue)))
        targetValue.Destination = SpreadsheetOutputDestination.CurrentWorksheet
        targetValue.StartRow = 1048577
        Dim custom = OutputLayoutPlanner.Create(document, targetValue, New OutputLayoutOptions(maxRows:=2000000))
        Assert.ThrowsException(Of ArgumentException)(Function() OfficeOutputPlanAdapter.Prepare(document, semantics, custom))
        Dim huge = New OutputDocument({New OutputTableArtifact("big", New ResultTableOutputModel(New Object(10000, 0) {}))})
        Assert.ThrowsException(Of ArgumentException)(Function() Prepare(huge))
        Dim longText = New OutputDocument({New OutputTableArtifact("text", New ResultTableOutputModel(New Object(,) {{New String("x"c, 32768)}}))})
        Assert.ThrowsException(Of ArgumentException)(Function() Prepare(longText))
    End Sub

    Private Shared Function FixtureDocument(name As String) As OutputDocument
        If name = "four-tables-c6" Then
            Return New OutputDocument({MakeTable("table-a", 2, 3), MakeTable("table-b", 3, 2), MakeTable("table-c", 1, 4), MakeTable("table-d", 2, 1)})
        End If
        If name = "mixed-zero" Then
            Return New OutputDocument({MakeTable("zero-first", 0, 3), New OutputDiagnosticArtifact("warn", OutputDiagnosticSeverity.Warning, "w", "Warning"),
                MakeTable("first", 2, 3), New OutputPlotArtifact("plot", "ref", "purpose"), MakeTable("zero-between", 1, 0),
                New OutputDiagnosticArtifact("fault", OutputDiagnosticSeverity.Fault, "f", "Fault"), MakeTable("last", 3, 2)})
        End If
        If name = "typed-roles" Then
            Dim model As New ResultTableOutputModel(New Object(,) {
                {"Title", Nothing, Nothing, Nothing}, {"Group", "P", "Other", "Literal"},
                {"A", 0.05, True, "=1+1"}, {"B", 0.049, "", "#N/A"},
                {"C", 0.051, Nothing, "0.002"}, {"D", "0.002", 0.5D, "+SUM(A1)"},
                {"Footer", Nothing, Nothing, Nothing}}, titleRows:=1, headerTopRows:=1, headerLeftColumns:=1, footerRows:=1,
                pvalueColumns:={1, 1}, pvalueCells:={New ResultTableCellAddress(1, 1), New ResultTableCellAddress(2, 1)})
            Return New OutputDocument({New OutputTableArtifact("typed", model)})
        End If
        Using fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData/Stage304", name & ".json")))
            Dim root = fixture.RootElement
            Dim names = root.GetProperty("groups").GetProperty("names").EnumerateArray().Select(Function(e) e.GetString()).ToArray()
            Dim groups = root.GetProperty("groups").GetProperty("values").EnumerateArray().Select(Function(g) g.EnumerateArray().Select(Function(e) e.GetDouble()).ToArray()).ToArray()
            Dim raw = root.GetProperty("options")
            Dim options As SpreadsheetAnalysisOptions
            Dim kind = CType(root.GetProperty("legacyEnumValue").GetInt32(), SpreadsheetAnalysisKind)
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

    Private Shared Function MakeTable(id As String, rows As Integer, columns As Integer) As OutputTableArtifact
        Return New OutputTableArtifact(id, New ResultTableOutputModel(DirectCast(Array.CreateInstance(GetType(Object), rows, columns), Object(,))))
    End Function
End Class
