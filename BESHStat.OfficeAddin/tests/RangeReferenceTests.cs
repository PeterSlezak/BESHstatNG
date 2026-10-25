using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class RangeReferenceTests
{
    [DataTestMethod]
    [DataRow("'Study Data'!$B$2:$D$25", "Study Data", "B2:D25")]
    [DataRow("'O''Brien Δ'!a1", "O'Brien Δ", "A1:A1")]
    [DataRow("Data!XFD1048576", "Data", "XFD1048576:XFD1048576")]
    public void SharedCommonGrammarPreservesFullQualifiedReference(string text, string sheet, string address)
    {
        var reference = RangeReferenceState.Parse(text, false);
        Assert.AreEqual(sheet, reference.WorksheetName); Assert.AreEqual(address, reference.Address);
        Assert.AreEqual(reference.QualifiedAddress, RangeReferenceState.Parse(reference.QualifiedAddress, false).QualifiedAddress);
    }
    [DataTestMethod]
    [DataRow("A1")] [DataRow("Sheet!A1,B2")] [DataRow("Sheet!A1;B2")]
    [DataRow("'[Other.xlsx]Sheet'!A1")] [DataRow("Sheet!XFE1")] [DataRow("Sheet!A0")]
    [DataRow("'Unclosed!A1")] [DataRow("Sheet!A1:XFD1048576")]
    public void InvalidOrExternalOrOversizedReferenceRejectsBeforeAnyHostCall(string text)
    {
        try { RangeReferenceState.Parse(text, false); Assert.Fail("Invalid reference was admitted."); }
        catch (ArgumentException) { /* Common may report ArgumentOutOfRangeException for off-grid coordinates. */ }
    }
    [TestMethod]
    public void OutputRequiresExactlyOneCell() => Assert.ThrowsException<ArgumentException>(() => RangeReferenceState.Parse("Input!A1:B2", true));

    [TestMethod]
    public async Task OnlyArmedOwnerAcceptsEventsManualEditDisarmsAndCancelRestoresCommittedText()
    {
        var host = new AnalysisPilotTests.Host(); int edits = 0;
        await using var state = new RangeReferenceState(host, false, () => edits++);
        await state.CaptureAsync(); var committed = state.Text;
        await state.ArmAsync(); Assert.IsTrue(state.Armed);
        host.EmitCapture(new() { Code = "workbook.ok", Owner = "other", Handle = "bad", Reference = SpreadsheetRangeReference.FromAddress("D9", "Other") });
        Assert.AreEqual(committed, state.Text);
        host.EmitCapture(new() { Code = "workbook.ok", Owner = host.Owner, Handle = "changed", Reference = SpreadsheetRangeReference.FromAddress("B2:D5", "O'Brien Δ") });
        Assert.AreEqual("'O''Brien Δ'!B2:D5", state.Text);
        await state.CancelAsync(); Assert.AreEqual(committed, state.Text); Assert.IsFalse(state.Armed);
        await state.ArmAsync(); state.Edit("'Manual Δ'!D5"); Assert.IsFalse(state.Armed);
        host.EmitCapture(new() { Code = "workbook.ok", Owner = host.Owner, Handle = "late", Reference = host.Reference });
        Assert.AreEqual("'Manual Δ'!D5", state.Text); Assert.IsTrue(edits >= 3);
    }
    [TestMethod]
    public async Task CaptureFallbackWorksWhenEventCaptureIsUnavailableAndDisposalIgnoresCallbacks()
    {
        var host = new AnalysisPilotTests.Host { CaptureCode = "range.capture-unavailable" };
        var state = new RangeReferenceState(host, false, () => { });
        await state.ArmAsync(); Assert.IsFalse(state.Armed); Assert.AreEqual("range.capture-unavailable", state.Code);
        host.CaptureCode = "workbook.ok"; await state.CaptureAsync(); Assert.AreEqual("'Input'!A1:C4", state.Text);
        await state.DisposeAsync(); host.EmitCapture(new() { Owner = host.Owner, Code = "workbook.ok", Reference = host.Reference, Handle = "late" });
        Assert.IsFalse(state.Armed); Assert.IsTrue(host.Ends >= 2);
    }
    [TestMethod]
    public async Task FailedOutputCaptureCannotSilentlyInspectPreviouslyValidDestination()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var host = new AnalysisPilotTests.Host(); var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        await state.AcquireAsync(); await state.RunAsync(); await state.InspectAsync(); Assert.IsTrue(state.CanWrite);
        await state.OutputReference.ArmAsync(); host.Reference = SpreadsheetRangeReference.FromAddress("A1:B2", "Input");
        await state.OutputReference.FinishAsync(); Assert.IsFalse(state.CanInspect); Assert.IsFalse(state.CanWrite);
        int inspected = host.Inspections; await state.InspectAsync(); await state.WriteAsync();
        Assert.AreEqual(inspected, host.Inspections); Assert.AreEqual(0, workbook.Writes);
        state.OutputReference.Edit("'Input'!M7"); Assert.IsTrue(state.CanInspect);
        state.OutputReference.Edit(""); Assert.IsFalse(state.CanInspect);
    }

    [TestMethod]
    public async Task LateArmCompletionAfterDisposalRemovesSubscriptionAgainAndCannotRearm()
    {
        var host = new AnalysisPilotTests.Host { PendingCapture = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        var state = new RangeReferenceState(host, false, () => { });
        var arm = state.ArmAsync(); Assert.IsTrue(state.Busy); Assert.IsFalse(state.Armed);
        await state.DisposeAsync(); host.PendingCapture.SetResult(new() { Code = "workbook.ok", Owner = host.Owner });
        await arm; Assert.IsFalse(state.Armed); Assert.IsTrue(host.Ends >= 2);
    }

    [TestMethod]
    public async Task ManualMalformedDraftIsVisibleAndCannotResolve()
    {
        var host = new AnalysisPilotTests.Host(); await using var state = new RangeReferenceState(host, true, () => { });
        state.Edit("Sheet!A1:B2"); Assert.AreEqual("input.invalid-reference", state.Code);
        Assert.IsNull(await state.ResolveAsync(default)); Assert.AreEqual("Sheet!A1:B2", state.Text); Assert.AreEqual(0, host.Resolutions);
    }

    internal static SpreadsheetRangeData Arrangement(SpreadsheetRangeData wide, SpreadsheetGroupedOrientation orientation, SpreadsheetHeaderMode header)
    {
        var result = new SpreadsheetRangeData { WorksheetId = wide.WorksheetId, WorksheetName = wide.WorksheetName, StartRow = 1, StartColumn = 1 };
        if (orientation == SpreadsheetGroupedOrientation.GroupsInRows)
            result.Rows = Enumerable.Range(0, wide.ColumnCount).Select(c => wide.Rows.Select(r => r[c]).ToList()).ToList();
        else if (orientation == SpreadsheetGroupedOrientation.LongFormat)
        {
            result.Rows.Add([SpreadsheetCellValue.TextCell("Group ID"), SpreadsheetCellValue.TextCell("Value")]);
            for (int c = 0; c < wide.ColumnCount; c++) for (int r = 1; r < wide.RowCount; r++)
                result.Rows.Add([wide.Rows[0][c], wide.Rows[r][c]]);
        }
        else result.Rows = wide.Rows.Select(r => r.ToList()).ToList();
        if (header == SpreadsheetHeaderMode.None)
        {
            if (orientation == SpreadsheetGroupedOrientation.GroupsInRows) result.Rows.ForEach(r => r.RemoveAt(0));
            else result.Rows.RemoveAt(0);
        }
        result.RowCount = result.Rows.Count; result.ColumnCount = result.Rows[0].Count;
        result.Address = SpreadsheetRangeAddress.Parse($"A1:{(char)('A' + result.ColumnCount - 1)}{result.RowCount}").Address;
        return result;
    }
    [DataTestMethod]
    [DataRow("anova-base", 0, 0)] [DataRow("anova-base", 1, 0)] [DataRow("anova-base", 2, 0)]
    [DataRow("anova-base", 0, 1)] [DataRow("anova-base", 1, 1)] [DataRow("anova-base", 2, 1)]
    [DataRow("anova-base", 0, 2)] [DataRow("anova-base", 1, 2)] [DataRow("anova-base", 2, 2)]
    [DataRow("kw-dunn", 0, 0)] [DataRow("kw-dunn", 1, 0)] [DataRow("kw-dunn", 2, 0)]
    [DataRow("kw-dunn", 0, 1)] [DataRow("kw-dunn", 1, 1)] [DataRow("kw-dunn", 2, 1)]
    [DataRow("kw-dunn", 0, 2)] [DataRow("kw-dunn", 1, 2)] [DataRow("kw-dunn", 2, 2)]
    public async Task AllArrangementsAndHeadersMatchCommonWideMetricsAndTables(string fixture, int orientation, int header)
    {
        using var f = AnalysisPilotTests.Fixture(fixture); var wide = AnalysisPilotTests.Range(f.RootElement);
        var descriptor = AnalysisCatalog.GetById(f.RootElement.GetProperty("analysisIdProposed").GetString()!);
        using AnalysisOptionDraft draft = fixture.StartsWith("kw") ? new KruskalWallisOptionDraft(new()) : new AnovaOptionDraft(new());
        var baseline = SharedAnalysisApplicationService.RunDocument(descriptor.Id, GroupedNumericRangeParser.Parse(wide), draft.Snapshot());
        var range = Arrangement(wide, (SpreadsheetGroupedOrientation)orientation, (SpreadsheetHeaderMode)header);
        var workbook = new AnalysisPilotTests.Workbook(range);
        await using var state = new AnalysisPilotState(descriptor, draft, workbook, new AnalysisPilotTests.Host());
        state.SetInterpretation((SpreadsheetGroupedOrientation)orientation, (SpreadsheetHeaderMode)header);
        if (orientation == 2) ProductionUxTests.SetLongSources(state, workbook, range);
        else state.InputReference.Edit(range.ToReference().QualifiedAddress);
        await state.RunAsync();
        Assert.AreEqual("analysis.completed", state.Code);
        CollectionAssert.AreEqual(baseline.Metrics.Select(m => m.Value).ToArray(), state.Metrics.Select(m => m.Value).ToArray());
        var tables = OutputDocument.FromAnalysis(baseline).Artifacts.OfType<OutputTableArtifact>().ToArray();
        CollectionAssert.AreEqual(tables.Select(t => (t.RowCount, t.ColumnCount)).ToArray(), state.Tables.Select(t => (t.Rows, t.Columns)).ToArray());
    }
    [DataTestMethod]
    [DataRow("'Input'!E1:E10", "'Input'!E1:E10")]
    [DataRow("'Input'!E1:E10", "'Input'!J2:J11")]
    [DataRow("'Input'!E1:F10", "'Input'!J1:J10")]
    public async Task InvalidLongRolesRejectBeforeReadsAndChangingInterpretationInvalidatesEverything(string category, string value)
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new() { IncludeWelch = true });
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, new AnalysisPilotTests.Host());
        await state.AcquireAsync(); await state.RunAsync(); await state.InspectAsync();
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.Required);
        state.CategoryReference.Edit(category); state.ValueReference.Edit(value);
        Assert.IsFalse(state.Current); Assert.IsNull(state.Inspection); Assert.AreEqual(0, state.Tables.Count); Assert.AreEqual(2, workbook.Reads); Assert.IsTrue(draft.IncludeWelch);
        await state.RunAsync(); Assert.IsFalse(state.Current); Assert.IsTrue(state.ShowPreview);
        Assert.AreEqual(2, workbook.Reads); Assert.AreEqual(0, workbook.Writes);
        Assert.IsTrue(state.Code is "input.long-shape" or "input.long-alignment" or "input.long-header-alignment");
    }
    [TestMethod]
    public async Task AutoRejectsMixedPossibleHeadersWithoutReinterpretingAndManualEditsDoNotRead()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var range = AnalysisPilotTests.Range(f.RootElement); range.Rows[0][0] = SpreadsheetCellValue.NumberCell(12);
        var workbook = new AnalysisPilotTests.Workbook(range);
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, new AnalysisPilotTests.Host());
        state.InputReference.Edit("'Input'!A1:C4"); Assert.AreEqual(0, workbook.Reads);
        state.SetInterpretation(SpreadsheetGroupedOrientation.GroupsInColumns, SpreadsheetHeaderMode.Auto);
        await state.AcquireAsync(); Assert.IsFalse(state.Current); Assert.IsTrue(state.Preview!.Issues.Any(i => i.Code == "header.ambiguous"));
    }
}
