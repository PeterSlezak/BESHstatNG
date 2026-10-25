using System.Text.Json;
using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class ProductionUxTests
{
    internal static void SetLongSources(AnalysisPilotState state, AnalysisPilotTests.Workbook workbook, SpreadsheetRangeData pair)
    {
        SpreadsheetRangeData Vector(int role, int column) => new() {
            WorksheetId = pair.WorksheetId, WorksheetName = pair.WorksheetName,
            WorkbookId = pair.WorkbookId, WorkbookName = pair.WorkbookName,
            StartRow = pair.StartRow, StartColumn = column, RowCount = pair.RowCount, ColumnCount = 1,
            Address = SpreadsheetRangeAddress.Parse($"{(column == 5 ? "E" : "J")}{pair.StartRow}:{(column == 5 ? "E" : "J")}{pair.StartRow + pair.RowCount - 1}").Address,
            Rows = pair.Rows.Select(row => new List<SpreadsheetCellValue> { row[role] }).ToList()
        };
        var category = Vector(0, 5); var values = Vector(1, 10);
        workbook.ResolveRange = reference => reference.StartColumn == 5 ? category : values;
        state.CategoryReference.Edit(category.ToReference().QualifiedAddress);
        state.ValueReference.Edit(values.ToReference().QualifiedAddress);
    }

    [DataTestMethod]
    [DataRow(0)] [DataRow(1)] [DataRow(2)]
    public async Task NonadjacentLongRunUsesTwoPhysicalSnapshotsAndMatchesAdjacentCommon(int header)
    {
        using var f = AnalysisPilotTests.Fixture("anova-base");
        using var draft = new AnovaOptionDraft(new());
        var pair = RangeReferenceTests.Arrangement(AnalysisPilotTests.Range(f.RootElement), SpreadsheetGroupedOrientation.LongFormat, (SpreadsheetHeaderMode)header);
        pair.StartRow = 7;
        var workbook = new AnalysisPilotTests.Workbook(pair); var host = new AnalysisPilotTests.Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat, (SpreadsheetHeaderMode)header);
        SetLongSources(state, workbook, pair);
        Assert.AreEqual(0, workbook.Reads); Assert.IsTrue(state.CanRun);
        await state.RunAsync();
        Assert.AreEqual("analysis.completed", state.Code); Assert.AreEqual(2, workbook.Reads);
        Assert.IsFalse(state.ShowPreview); Assert.IsTrue(state.Current); Assert.AreEqual(0, workbook.Writes);
        var parsed = GroupedNumericRangeParser.Parse(pair, new() { Orientation = SpreadsheetGroupedOrientation.LongFormat, Header = (SpreadsheetHeaderMode)header });
        var expected = SharedAnalysisApplicationService.RunDocument(AnalysisIds.OneWayAnova, parsed, draft.Snapshot());
        CollectionAssert.AreEqual(expected.Metrics.Select(m => m.Value).ToArray(), state.Metrics.Select(m => m.Value).ToArray());
        Assert.AreEqual(2, host.Sources!.Count);
        CollectionAssert.AreEqual(new[] { 5, 10 }, host.Sources.Select(s => s.Snapshot.StartColumn).ToArray());
        Assert.IsTrue(host.Sources.All(s => s.Snapshot.StartRow == 7 && s.Snapshot.WorksheetId == "sheet-1" && s.Handle.Length > 0));
        await state.InspectAsync(); host.CheckCode = "user.document-changed";
        await state.WriteAsync(); Assert.AreEqual("output.consent-stale", state.Code); Assert.AreEqual(0, workbook.Writes);
    }

    [DataTestMethod]
    [DataRow("'Input'!E1:E10", "'Input'!J2:J11", "input.long-header-alignment")]
    [DataRow("'Input'!E1:E10", "'Other'!J1:J10", "input.long-alignment")]
    [DataRow("'Input'!E1:E10", "'Input'!E1:E10", "input.long-alignment")]
    [DataRow("'Input'!E1:F10", "'Input'!J1:J10", "input.long-shape")]
    [DataRow("'Input'!E1:E10", "'Input'!J11:J19", "input.long-alignment")]
    [DataRow("'Input'!E1:E10", "'Input'!J1,J3", "input.invalid-reference")]
    [DataRow("'Input'!E1:E10", "'[Book.xlsx]Input'!J1:J10", "input.invalid-reference")]
    [DataRow("'Input'!E1:E10", "J1:J10", "input.invalid-reference")]
    [DataRow("'Input'!E1:E6000", "'Input'!J1:J6000", "input.total-cells")]
    public async Task InvalidLongDraftsStopBeforeReadsComputeAndWrite(string category, string values, string code)
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement)); var host = new AnalysisPilotTests.Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.Required);
        state.CategoryReference.Edit(category); state.ValueReference.Edit(values);
        await state.RunAsync();
        Assert.AreEqual(code, state.Code); Assert.IsTrue(state.ShowPreview);
        Assert.AreEqual(0, workbook.Reads); Assert.AreEqual(0, host.Pins); Assert.AreEqual(0, state.Tables.Count); Assert.AreEqual(0, workbook.Writes);
        if (code == "input.invalid-reference") StringAssert.Contains(state.Guidance, "Numeric values range");
    }

    [DataTestMethod]
    [DataRow("workbook")] [DataRow("sheet")] [DataRow("rows")] [DataRow("duplicate")] [DataRow("shape")]
    public async Task PhysicalMismatchCannotBeHiddenByValidManualText(string defect)
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var pair = RangeReferenceTests.Arrangement(AnalysisPilotTests.Range(f.RootElement), SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.Required);
        var workbook = new AnalysisPilotTests.Workbook(pair); var host = new AnalysisPilotTests.Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.Required); SetLongSources(state, workbook, pair);
        var resolve = workbook.ResolveRange!;
        workbook.ResolveRange = r => {
            var s = resolve(r); if (r.StartColumn != 10) return s;
            if (defect == "workbook") s.WorkbookId = "different-book";
            if (defect == "sheet") s.WorksheetId = "replacement";
            if (defect == "rows") s.StartRow++;
            if (defect == "duplicate") s.StartColumn = 5;
            if (defect == "shape") s.Rows[1].Add(SpreadsheetCellValue.BlankCell());
            return s;
        };
        await state.RunAsync(); Assert.AreEqual(defect == "rows" ? "input.long-header-alignment" : "input.long-alignment", state.Code);
        Assert.AreEqual(0, host.Pins); Assert.AreEqual(0, state.Tables.Count); Assert.AreEqual(0, workbook.Writes);
    }

    [DataTestMethod]
    [DataRow(0)] [DataRow(1)]
    public async Task LongIssuesPointToRealColumnAndRowAndBlanksStayPaired(int role)
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var pair = RangeReferenceTests.Arrangement(AnalysisPilotTests.Range(f.RootElement), SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.Required);
        pair.StartRow = 20; pair.Rows[2][role] = SpreadsheetCellValue.ErrorCell("#N/A");
        var workbook = new AnalysisPilotTests.Workbook(pair); var host = new AnalysisPilotTests.Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.Required); SetLongSources(state, workbook, pair);
        await state.RunAsync(); Assert.AreEqual("input.validation-failed", state.Code); Assert.IsTrue(state.ShowPreview);
        Assert.IsTrue(state.Preview!.Issues.Any(i => i.WorksheetRow == 22 && i.WorksheetColumn == (role == 0 ? 5 : 10)));
        Assert.IsFalse(JsonSerializer.Serialize(state.Preview).Contains("#N/A")); Assert.AreEqual(0, host.Pins);
        pair.Rows[2][role] = SpreadsheetCellValue.BlankCell();
        host.ChangeDocument(); SetLongSources(state, workbook, pair); Assert.IsFalse(state.ShowPreview); await state.ValidateAsync();
        var common = GroupedNumericRangeParser.ParseWithDiagnostics(pair, new() { Orientation = SpreadsheetGroupedOrientation.LongFormat });
        Assert.AreEqual(common.Success, state.Preview!.Valid);
        if (common.Success) CollectionAssert.AreEqual(common.Data.GroupCounts, state.Preview.GroupCounts.ToArray());
        Assert.AreEqual(0, state.Tables.Count); Assert.AreEqual(0, workbook.Writes);
    }

    [TestMethod]
    [Timeout(40000)]
    public async Task DirectRunHasOneThirtySecondDeadlineIncludingCompositePinAndNeverComputesOnTimeout()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var pair = RangeReferenceTests.Arrangement(AnalysisPilotTests.Range(f.RootElement), SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.Required);
        var workbook = new AnalysisPilotTests.Workbook(pair); var host = new AnalysisPilotTests.Host { Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.Required); SetLongSources(state, workbook, pair);
        await state.RunAsync();
        Assert.AreEqual("input.time-budget", state.Code); Assert.IsFalse(state.Busy); Assert.IsFalse(state.Current); Assert.IsTrue(state.ShowPreview);
        Assert.AreEqual(2, workbook.Reads); Assert.AreEqual(1, host.Pins); Assert.AreEqual(0, state.Tables.Count); Assert.AreEqual(0, workbook.Writes);
        host.Pending.SetResult(new() { Code = "workbook.ok", SourceToken = "late", SessionIdentity = "late" });
        Assert.IsFalse(state.Current); Assert.AreEqual(0, state.Tables.Count);
    }

    [TestMethod]
    public async Task CombinedBytesRejectBeforeParsingEvenWhenEachVectorFitsIndividually()
    {
        using var draft = new AnovaOptionDraft(new());
        var pair = new SpreadsheetRangeData { WorksheetId = "sheet-1", WorksheetName = "Input", StartRow = 1, RowCount = 510, ColumnCount = 2,
            Rows = Enumerable.Range(0, 510).Select(_ => new List<SpreadsheetCellValue> {
                SpreadsheetCellValue.TextCell(new string('x', 32767)), SpreadsheetCellValue.TextCell(new string('y', 32767)) }).ToList() };
        var workbook = new AnalysisPilotTests.Workbook(pair); var host = new AnalysisPilotTests.Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.None); SetLongSources(state, workbook, pair);
        await state.RunAsync(); Assert.AreEqual("input.estimated-bytes", state.Code);
        Assert.AreEqual(0, host.Pins); Assert.AreEqual(0, state.Tables.Count); Assert.AreEqual(0, workbook.Writes);
    }

    [TestMethod]
    public async Task SwitchingCaptureRolesModeCancelAndResetRetireOldOwnerAndDerivedState()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var host = new AnalysisPilotTests.Host(); var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.Required);
        await state.CategoryReference.ArmAsync(); var oldOwner = host.Owner; Assert.IsTrue(state.CategoryReference.Armed);
        await state.ValueReference.ArmAsync(); Assert.IsFalse(state.CategoryReference.Armed); Assert.IsTrue(state.ValueReference.Armed);
        host.EmitCapture(new() { Owner = oldOwner, Code = "workbook.ok", Handle = "old", Reference = host.Reference });
        Assert.AreEqual("", state.CategoryReference.Text); Assert.AreEqual("", state.ValueReference.Text);
        await state.ValueReference.CancelAsync(); Assert.IsFalse(state.Picking); Assert.IsFalse(state.Current);
        await state.CategoryReference.ArmAsync(); state.SetInterpretation(SpreadsheetGroupedOrientation.GroupsInRows, SpreadsheetHeaderMode.None);
        Assert.IsFalse(state.Picking); Assert.IsTrue(host.Ends > 0); Assert.AreEqual(0, workbook.Reads);
        await state.InputReference.ArmAsync(); state.Reset(); Assert.IsFalse(state.Picking); Assert.IsFalse(state.ShowPreview);
        Assert.AreEqual(SpreadsheetGroupedOrientation.GroupsInColumns, state.Orientation); Assert.AreEqual(0, workbook.Writes);
    }

    [TestMethod]
    public async Task OptionalValidationResetAndRunHaveSeparateBoundariesAndRetainUncertainLatch()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement)); var host = new AnalysisPilotTests.Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host);
        state.InputReference.Edit("'Input'!A1:C4"); await state.ValidateAsync();
        Assert.IsTrue(state.ShowPreview); Assert.AreEqual(0, state.Tables.Count); Assert.AreEqual(0, host.Inspections);
        await state.RunAsync(); Assert.AreEqual(2, workbook.Reads); Assert.IsTrue(state.ShowPreview);
        draft.SetAlpha("0.1"); Assert.IsFalse(state.ShowPreview); Assert.IsNull(state.Preview);
        await state.RunAsync(); Assert.AreEqual("analysis.completed", state.Code); Assert.IsFalse(state.ShowPreview);
        await state.InspectAsync(); workbook.Uncertain = true; await state.WriteAsync();
        var outcome = state.WriteOutcome; state.Reset();
        Assert.AreEqual("", state.InputReference.Text); Assert.AreEqual("", state.CategoryReference.Text); Assert.AreEqual("", state.ValueReference.Text);
        Assert.AreEqual("0.05", draft.AlphaText); Assert.AreEqual("J1", state.StartCell); Assert.AreEqual("BESHStat results", state.NewSheetName);
        Assert.AreEqual(SpreadsheetGroupedOrientation.GroupsInColumns, state.Orientation); Assert.AreEqual(SpreadsheetHeaderMode.Required, state.Header);
        Assert.AreEqual(SpreadsheetOutputDestination.CurrentWorksheet, state.Destination); Assert.IsNull(state.Preview); Assert.IsNull(state.Inspection);
        Assert.IsTrue(state.RequiresRecovery); Assert.AreSame(outcome, state.WriteOutcome); Assert.IsFalse(state.CanRun);
        await state.RunAsync(); Assert.AreEqual(3, workbook.Reads); Assert.AreEqual(1, workbook.Writes);
        using var otherDraft = new KruskalWallisOptionDraft(new());
        await using var other = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.KruskalWallis), otherDraft, workbook, host);
        Assert.IsTrue(other.RequiresRecovery); Assert.AreSame(outcome, other.WriteOutcome); Assert.IsFalse(other.CanRun);
        other.InputReference.Edit("'Input'!A1:C4"); await other.RunAsync();
        Assert.AreEqual(3, workbook.Reads); Assert.AreEqual(1, workbook.Writes);
    }
}
