Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports BESHStatNG
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PValuePresentationPlanTests
    Public Property TestContext As TestContext

    <DataTestMethod>
    <DataRow(1, "Fisher's LSD", 4)>
    <DataRow(2, "Bonferroni", 4)>
    <DataRow(3, "Tukey-Kramer", 4)>
    <DataRow(4, "Games-Howell", 5)>
    Public Sub PostHocProbabilities_RemainBinary64ThroughSemanticAndWirePlans(tableIndex As Integer,
                                                                                 procedure As String,
                                                                                 pvalueColumn As Integer)
        ' LoadAnalysis runs the real SharedAnalysisApplicationService against immutable Stage304 input.
        Dim document As OutputDocument = LoadAnalysis("anova-all-posthoc")
        Dim table As OutputTableArtifact = DirectCast(document.Artifacts(tableIndex), OutputTableArtifact)
        StringAssert.Contains(CStr(table.Model.Values(0, 0)), procedure)
        Dim settings As New PValuePresentationSettings With {.DecimalPlaces = 3}
        Dim firstProbability As Double = CDbl(table.Model.Values(1, pvalueColumn))
        For Each alpha As Double In New Double() {0.05, 0.01, firstProbability}
            Dim semantics = OutputSemanticPlanner.Create(document, settings, alpha, "")
            Dim styled = semantics.Artifacts(tableIndex)
            Dim target As New SpreadsheetOutputTarget With {.StartRow = 6, .StartColumn = 3}
            Dim wire = OfficeOutputPlanAdapter.Prepare(document, semantics,
                OutputLayoutPlanner.Create(document, target, semanticPlan:=semantics))
            Dim wireTable = wire.Tables.Single(Function(t) t.DocumentIndex = tableIndex)
            Dim probabilities = styled.Cells.Where(Function(c) c.IsPValue).ToArray()
            Assert.AreEqual(3, probabilities.Length)
            For Each cell As OutputCellStyle In probabilities
                Assert.AreEqual(pvalueColumn, cell.Column)
                Assert.IsInstanceOfType(table.Model.Values(cell.Row, cell.Column), GetType(Double))
                Assert.AreEqual(OutputCellKind.Number, cell.Cell.Kind)
                Assert.AreEqual(OutputNumberKind.Binary64, cell.Cell.NumberKind.Value)
                Dim probability As Double = cell.Cell.BinaryValue.Value
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(CDbl(table.Model.Values(cell.Row, cell.Column))),
                                BitConverter.DoubleToInt64Bits(probability))
                Assert.AreEqual(OutputStyleRole.Body, cell.Role)
                Assert.AreEqual(OutputNumberFormatKind.PValue, cell.NumberFormat.Kind)
                Assert.AreEqual(3, cell.NumberFormat.DecimalPlaces.Value)
                Assert.AreEqual(probability <= alpha, cell.IsSignificant)
                Assert.AreEqual(cell.IsSignificant, cell.StrongEmphasis)
                Assert.AreEqual(If(cell.IsSignificant, "significant", ""), cell.AccessibilityIndicator)
                Assert.AreEqual(PValuePresentation.FormatForDisplay(probability, settings, CultureInfo.InvariantCulture), cell.DisplayPreview)
                Dim projected = wireTable.Cells.Single(Function(c) c.Row = cell.Row AndAlso c.Column = cell.Column)
                Assert.AreEqual("number", projected.Value.Kind)
                Assert.IsNull(projected.Value.TextValue)
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(probability), BitConverter.DoubleToInt64Bits(projected.Value.NumberValue.Value))
                Assert.IsTrue(projected.IsPValue)
                Assert.AreEqual(cell.IsSignificant, projected.IsSignificant)
                Assert.AreEqual(cell.StrongEmphasis, projected.StrongEmphasis)
                Assert.AreEqual(PValuePresentation.BuildExcelNumberFormat(settings), projected.NumberFormat)
            Next
            If alpha = 0.05 Then
                Assert.IsTrue(probabilities.Any(Function(c) c.IsSignificant), "Fixture must exercise significance.")
                Assert.IsTrue(probabilities.Any(Function(c) Not c.IsSignificant), "Fixture must exercise a nonsignificant probability.")
            End If
            ' Comparison, mean/CI, SE/statistic (and Games-Howell DF) remain historical text.
            For Each cell As OutputCellStyle In styled.Cells.Where(Function(c) c.Role = OutputStyleRole.Body AndAlso Not c.IsPValue)
                Assert.AreEqual(OutputCellKind.Text, cell.Cell.Kind)
                Dim projected = wireTable.Cells.Single(Function(c) c.Row = cell.Row AndAlso c.Column = cell.Column)
                Assert.AreEqual("text", projected.Value.Kind)
                Assert.AreEqual(cell.Cell.TextValue, projected.Value.TextValue)
                Assert.IsFalse(projected.IsPValue)
                Assert.AreEqual("General", projected.NumberFormat)
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub Annotations_TranslateOnceDeduplicateAndDiagnoseWithoutMutatingRawLists()
        Dim model As New ResultTableOutputModel(New Object(,) {
            {"Title", Nothing, Nothing}, {"Header", "Value", "P"},
            {"A", 7, 0.04}, {"B", 8, 0.1}, {"Foot", Nothing, Nothing}},
            titleRows:=1, headerTopRows:=1, headerLeftColumns:=1, footerRows:=1,
            pvalueColumns:=New Integer() {2, 2, 0, -1, 3, Integer.MaxValue},
            pvalueCells:=New ResultTableCellAddress() {New ResultTableCellAddress(1, 2), New ResultTableCellAddress(1, 2),
                New ResultTableCellAddress(2, 1), New ResultTableCellAddress(0, 1), New ResultTableCellAddress(1, 0),
                New ResultTableCellAddress(3, 2), New ResultTableCellAddress(1, Integer.MaxValue)})
        Dim artifact As New OutputTableArtifact("annotations", model)
        Dim plan As OutputSemanticPlan = OutputSemanticPlanner.Create(artifact, Nothing, 0.05)
        CollectionAssert.AreEqual(New String() {"2:2", "3:1", "3:2"}, plan.Artifacts(0).Cells.Where(Function(c) c.IsPValue).Select(Function(c) c.LocalId).ToArray())
        Assert.AreEqual(8, plan.Diagnostics.Count)
        CollectionAssert.AreEqual(New Integer() {2, 3, 4, 5, 3, 4, 5, 6}, plan.Diagnostics.Select(Function(d) d.SourceIndex.Value).ToArray())
        Assert.IsTrue(plan.Diagnostics.Take(4).All(Function(d) d.Code = "pvalue.column.invalid"))
        Assert.IsTrue(plan.Diagnostics.Skip(4).All(Function(d) d.Code = "pvalue.cell.invalid"))
        Assert.IsTrue(plan.Diagnostics.All(Function(d) d.ArtifactId = "annotations"))
        CollectionAssert.AreEqual(model.PvalueColumns, artifact.Model.PvalueColumns)
        CollectionAssert.AreEqual(model.PvalueCells, artifact.Model.PvalueCells)
        Assert.IsTrue(plan.Artifacts(0).Cells.Where(Function(c) c.Role <> OutputStyleRole.Body).All(Function(c) Not c.IsPValue))
    End Sub

    <TestMethod>
    Public Sub Significance_UsesOnlyValidTypedProbabilitiesAndIncludesExactAlpha()
        Dim raw As Object() = {0.0R, 0.05R, 0.05000000000000001R, 1.0R, Double.NaN, Double.PositiveInfinity,
            Double.NegativeInfinity, -0.01R, 1.01R, Nothing, False, True, "0.01", "#N/A", "", 0.04D, 0, 1,
            Single.NaN, 1.0000000000000000000000000001D}
        Dim values(raw.Length - 1, 0) As Object
        For i As Integer = 0 To raw.Length - 1
            values(i, 0) = raw(i)
        Next
        Dim table As New OutputTableArtifact("probabilities", New ResultTableOutputModel(values, pvalueColumns:=New Integer() {1}))
        Dim plan As OutputSemanticPlan = OutputSemanticPlanner.Create(table, Nothing, 0.05, "")
        CollectionAssert.AreEqual(New Integer() {0, 1, 15, 16}, plan.Artifacts(0).Cells.Where(Function(c) c.IsSignificant).Select(Function(c) c.Row).ToArray())
        Assert.AreEqual("<0.00000001", plan.Artifacts(0).Cells(0).DisplayPreview)
        For Each index As Integer In New Integer() {4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 18, 19}
            Assert.IsNull(plan.Artifacts(0).Cells(index).DisplayPreview)
        Next
        Assert.AreEqual(0.04D, plan.Artifacts(0).Cells(15).Cell.DecimalValue.Value)
        Assert.AreEqual(OutputCellKind.Text, plan.Artifacts(0).Cells(12).Cell.Kind)
        Assert.AreEqual(OutputStyleRole.Body, plan.Artifacts(0).Cells(1).Role)
    End Sub

    <DataTestMethod>
    <DataRow(0.0)>
    <DataRow(1.0)>
    <DataRow(-0.1)>
    <DataRow(1.1)>
    <DataRow(Double.NaN)>
    <DataRow(Double.PositiveInfinity)>
    <DataRow(Double.NegativeInfinity)>
    Public Sub InvalidAlpha_UsesFactoryCompatibleFallback(alpha As Double)
        Dim table As New OutputTableArtifact("alpha", New ResultTableOutputModel(New Object(,) {{0.05}}, pvalueColumns:=New Integer() {1}))
        Dim plan As OutputSemanticPlan = OutputSemanticPlanner.Create(table, Nothing, alpha)
        Assert.AreEqual(0.05, plan.Alpha)
        Assert.IsTrue(plan.Artifacts(0).Cells(0).IsSignificant)
        Assert.AreEqual("pvalue.alpha.defaulted", plan.Diagnostics.Single().Code)
        Assert.AreEqual(ResultTableWriteRequestFactory.Create(table.Model, Nothing, alpha, Nothing).PvalueHighlightAlpha, plan.Alpha)
    End Sub

    <TestMethod>
    Public Sub Settings_AreCopiedCoreNormalizedAndDoNotMutateGlobalOrCallerState()
        Dim settings As New PValuePresentationSettings With {.DecimalPlaces = 1, .SmallValueDisplay = CType(99, PValueSmallValueDisplay), .UseUpperBound = False}
        Dim table As New OutputTableArtifact("settings", New ResultTableOutputModel(New Object(,) {{0.001}}, pvalueColumns:=New Integer() {1}))
        Dim plan As OutputSemanticPlan = OutputSemanticPlanner.Create(table, settings, Double.NaN, "")
        Assert.AreEqual(8, plan.Policy.DecimalPlaces)
        Assert.AreEqual(PValueSmallValueDisplay.LessThanThreshold, plan.Policy.SmallValueDisplay)
        Assert.IsFalse(plan.Policy.UseUpperBound)
        Assert.AreEqual(1, settings.DecimalPlaces)
        Assert.AreEqual(99, CInt(settings.SmallValueDisplay))
        CollectionAssert.AreEqual(New String() {"pvalue.settings.defaulted", "pvalue.alpha.defaulted"}, plan.Diagnostics.Select(Function(d) d.Code).ToArray())
        settings.DecimalPlaces = 16
        settings.UseUpperBound = True
        Assert.AreEqual(8, plan.Policy.DecimalPlaces)
        Assert.IsFalse(plan.Policy.UseUpperBound)
        Assert.AreEqual("0.00100000", plan.Artifacts(0).Cells(0).DisplayPreview)
        Assert.IsNull(OutputSemanticPlanner.Create(table, settings, 0.05).Artifacts(0).Cells(0).DisplayPreview)
        Assert.ThrowsException(Of ArgumentNullException)(Function() OutputSemanticPlanner.Create(DirectCast(Nothing, OutputDocument), Nothing, 0.05))
        Assert.ThrowsException(Of ArgumentNullException)(Function() OutputSemanticPlanner.Create(DirectCast(Nothing, OutputTableArtifact), Nothing, 0.05))
    End Sub

    <DataTestMethod>
    <DataRow(-1)>
    <DataRow(0)>
    <DataRow(1)>
    <DataRow(2)>
    <DataRow(16)>
    <DataRow(17)>
    <DataRow(Integer.MaxValue)>
    Public Sub DecimalSettingsBounds_UseCoreDefaultsAndRetainValidEndpoints(decimals As Integer)
        Dim settings As New PValuePresentationSettings With {.DecimalPlaces = decimals}
        Dim policy As New OutputPValuePolicy(settings)
        Assert.AreEqual(If(decimals >= 2 AndAlso decimals <= 16, decimals, 8), policy.DecimalPlaces)
        Assert.AreEqual(decimals, settings.DecimalPlaces)
        Assert.AreEqual(Global.BESHStatNG.PValuePresentation.LowerThreshold(settings), policy.LowerThreshold)
        Assert.AreEqual(Global.BESHStatNG.PValuePresentation.UpperThreshold(settings), policy.UpperThreshold)
        Assert.AreEqual(Global.BESHStatNG.PValuePresentation.FormatForDisplay(0, settings, CultureInfo.InvariantCulture), policy.Preview(0, CultureInfo.InvariantCulture))
    End Sub

    <DataTestMethod>
    <DataRow(PValueSmallValueDisplay.LessThanThreshold, False)>
    <DataRow(PValueSmallValueDisplay.LessThanThreshold, True)>
    <DataRow(PValueSmallValueDisplay.ScientificNotation, False)>
    <DataRow(PValueSmallValueDisplay.ScientificNotation, True)>
    <DataRow(PValueSmallValueDisplay.FixedDecimal, False)>
    <DataRow(PValueSmallValueDisplay.FixedDecimal, True)>
    Public Sub DisplayModes_MatchCoreAtStrictThresholdBoundaries(mode As PValueSmallValueDisplay, upper As Boolean)
        Dim settings As New PValuePresentationSettings With {.DecimalPlaces = 3, .SmallValueDisplay = mode, .UseUpperBound = upper}
        Dim policy As New OutputPValuePolicy(settings)
        Assert.AreEqual(0.001, policy.LowerThreshold)
        Assert.AreEqual(0.999, policy.UpperThreshold)
        For Each culture As CultureInfo In New CultureInfo() {CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("sk-SK")}
            For Each value As Double In New Double() {0, 0.0009999999999999, 0.001, 0.0010000000000001, 0.05, 0.9989999999999999, 0.999, 0.9990000000000001, 1}
                Assert.AreEqual(Global.BESHStatNG.PValuePresentation.FormatForDisplay(value, settings, culture), policy.Preview(value, culture))
            Next
        Next
        Assert.AreEqual("0.001", policy.Preview(0.001, CultureInfo.InvariantCulture))
        Assert.AreEqual("0.999", policy.Preview(0.999, CultureInfo.InvariantCulture))
        If mode = PValueSmallValueDisplay.LessThanThreshold Then Assert.AreEqual("<0.001", policy.Preview(0, CultureInfo.InvariantCulture))
        If mode = PValueSmallValueDisplay.ScientificNotation Then Assert.AreEqual("1.000E-004", policy.Preview(0.0001, CultureInfo.InvariantCulture))
        If mode = PValueSmallValueDisplay.FixedDecimal Then Assert.AreEqual("0.000", policy.Preview(0, CultureInfo.InvariantCulture))
        Assert.AreEqual(If(upper, ">0.999", "1.000"), policy.Preview(1, CultureInfo.InvariantCulture))
    End Sub

    <TestMethod>
    Public Sub ExplicitCulture_IsIndependentOfMachineDefaultsAndDoesNotChangeNumbers()
        Dim table As New OutputTableArtifact("locale", New ResultTableOutputModel(New Object(,) {{0.0123}}, pvalueColumns:=New Integer() {1}))
        Dim settings As New PValuePresentationSettings With {.DecimalPlaces = 3}
        Dim original As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sk-SK")
            Dim invariant As OutputSemanticPlan = OutputSemanticPlanner.Create(table, settings, 0.05, "")
            Dim us As OutputSemanticPlan = OutputSemanticPlanner.Create(table, settings, 0.05, "en-US")
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US")
            Dim sk As OutputSemanticPlan = OutputSemanticPlanner.Create(table, settings, 0.05, "sk-SK")
            Assert.AreEqual("0.012", invariant.Artifacts(0).Cells(0).DisplayPreview)
            Assert.AreEqual("0.012", us.Artifacts(0).Cells(0).DisplayPreview)
            Assert.AreEqual("0,012", sk.Artifacts(0).Cells(0).DisplayPreview)
            Assert.AreEqual("sk-SK", sk.PreviewCultureName)
            Assert.AreEqual(0.0123, sk.Artifacts(0).Cells(0).Cell.BinaryValue.Value)
            Assert.AreEqual(Normalize(invariant), Normalize(OutputSemanticPlanner.Create(table, settings, 0.05, "")))
        Finally
            CultureInfo.CurrentCulture = original
        End Try
        Assert.ThrowsException(Of ArgumentNullException)(Function() New OutputPValuePolicy(settings).Preview(0.01, Nothing))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputPValuePolicy(settings).Preview(Double.NaN, CultureInfo.InvariantCulture))
    End Sub

    <DataTestMethod>
    <DataRow("anova-base")>
    <DataRow("anova-welch-tukey")>
    <DataRow("anova-all-posthoc")>
    <DataRow("kw-base")>
    <DataRow("kw-dunn")>
    <DataRow("generic-mixed")>
    Public Sub NormalizedSemanticFixtures_AreStableForBothFutureAdapters(name As String)
        Dim document As OutputDocument = If(name = "generic-mixed", GenericDocument(), LoadAnalysis(name))
        Dim settings As New PValuePresentationSettings With {.DecimalPlaces = 3}
        Dim plan As OutputSemanticPlan = OutputSemanticPlanner.Create(document, settings, 0.05, "")
        Dim normalized As String = Normalize(plan)
        Dim actualDirectory As String = Path.Combine(TestContext.TestRunDirectory, "OutputSemanticActual")
        Directory.CreateDirectory(actualDirectory)
        File.WriteAllText(Path.Combine(actualDirectory, name & ".json"), normalized & Environment.NewLine)
        Dim fixturePath As String = Path.Combine(AppContext.BaseDirectory, "TestData", "OutputSemantics", name & ".json")
        Assert.IsTrue(File.Exists(fixturePath), "Missing reviewed fixture; actual: " & actualDirectory)
        Assert.AreEqual(File.ReadAllText(fixturePath).TrimEnd(), normalized, name)
        Assert.AreEqual(normalized, Normalize(OutputSemanticPlanner.Create(document, settings, 0.05, "")))
        For Each artifact As OutputArtifactStylePlan In plan.Artifacts
            Assert.AreEqual(artifact.Cells.Count, artifact.Cells.Select(Function(c) c.LocalId).Distinct().Count())
            Assert.IsTrue(artifact.Cells.Where(Function(c) c.IsPValue).All(Function(c) c.Role = OutputStyleRole.Body))
            For Each cell As OutputCellStyle In artifact.Cells.Where(Function(c) c.IsPValue)
                TestContext.WriteLine(artifact.ArtifactId & " [" & cell.LocalId & "] " & cell.DisplayPreview & " significant=" & cell.IsSignificant.ToString())
            Next
        Next
    End Sub

    Private Shared Function GenericDocument() As OutputDocument
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

    ' Test-only v1 normalized projection. Exact decimals are invariant strings here to avoid
    ' lossy JS JSON numbers; binary64 uses round-trip text and signed zero gets explicit bits.
    ' This fixture format does not change AnalysisResultDocumentSnapshot v1 or raw 02.01.
    Private Shared Function Normalize(plan As OutputSemanticPlan) As String
        Return JsonSerializer.Serialize(New With {
            .schemaVersion = plan.SchemaVersion, .alpha = plan.Alpha, .culture = plan.PreviewCultureName,
            .policy = New With {.decimals = plan.Policy.DecimalPlaces, .small = plan.Policy.SmallValueDisplay.ToString(),
                .upper = plan.Policy.UseUpperBound, .lowerThreshold = plan.Policy.LowerThreshold, .upperThreshold = plan.Policy.UpperThreshold},
            .artifacts = plan.Artifacts.Select(Function(a) New With {
                .id = a.ArtifactId, .kind = a.Kind.ToString(), .rows = a.RowCount, .columns = a.ColumnCount,
                .diagnosticRole = If(a.DiagnosticRole.HasValue, a.DiagnosticRole.ToString(), Nothing), .indicator = a.AccessibilityIndicator,
                .cells = a.Cells.Select(Function(c) New With {.id = c.LocalId, .row = c.Row, .column = c.Column,
                    .role = c.Role.ToString(), .value = NormalizeCell(c.Cell), .format = c.NumberFormat.Kind.ToString(),
                    .decimals = c.NumberFormat.DecimalPlaces, .pvalue = c.IsPValue, .significant = c.IsSignificant,
                    .strong = c.StrongEmphasis, .indicator = c.AccessibilityIndicator, .preview = c.DisplayPreview}).ToArray()}).ToArray(),
            .diagnostics = plan.Diagnostics.Select(Function(d) New With {.artifact = d.ArtifactId, .code = d.Code, .sourceIndex = d.SourceIndex}).ToArray()})
    End Function

    Private Shared Function NormalizeCell(cell As OutputCell) As Object
        Dim fields As New Dictionary(Of String, Object) From {{"kind", cell.Kind.ToString()}}
        Select Case cell.Kind
            Case OutputCellKind.Number
                fields.Add("representation", cell.NumberKind.Value.ToString())
                If cell.NumberKind.Value = OutputNumberKind.ExactDecimal Then
                    fields.Add("exactDecimal", cell.DecimalValue.Value.ToString(CultureInfo.InvariantCulture))
                Else
                    fields.Add("binary64", cell.BinaryValue.Value.ToString("R", CultureInfo.InvariantCulture))
                    fields.Add("bits", BitConverter.DoubleToInt64Bits(cell.BinaryValue.Value).ToString("X16", CultureInfo.InvariantCulture))
                End If
            Case OutputCellKind.Text : fields.Add("text", cell.TextValue)
            Case OutputCellKind.BooleanValue : fields.Add("boolean", cell.BooleanValue.Value)
            Case OutputCellKind.Error
                fields.Add("errorKind", cell.ErrorKind.Value.ToString())
                fields.Add("code", cell.ErrorCode)
                fields.Add("nativeCode", cell.NativeErrorCode)
                fields.Add("original", cell.OriginalError)
                fields.Add("safeDetail", cell.SafeErrorDetail)
            Case OutputCellKind.Formula
                fields.Add("intent", cell.FormulaExpression)
                fields.Add("language", cell.FormulaLanguage)
        End Select
        Return fields
    End Function
End Class
