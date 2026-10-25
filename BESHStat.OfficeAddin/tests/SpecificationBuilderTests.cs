using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class SpecificationBuilderTests
{
    internal static SpreadsheetRangeData Source(params object?[][] values) => new() {
        WorkbookId = "workbook-fixture", WorksheetId = "sheet-1", WorksheetName = "Hárok 1", SelectionRevision = "revision-1",
        StartRow = 3, StartColumn = 2, RowCount = values.Length, ColumnCount = values[0].Length,
        Address = $"B3:{BESHStatNG.RegressionVariableCatalog.NumberToLetters(values[0].Length + 1)}{values.Length + 2}",
        DateSystem = SpreadsheetDateSystem.Excel1900,
        Rows = values.Select(row => row.Select(v => v switch {
            null => SpreadsheetCellValue.BlankCell(), SpreadsheetCellValue cell => cell,
            string text => SpreadsheetCellValue.TextCell(text), bool flag => SpreadsheetCellValue.BooleanCell(flag),
            _ => SpreadsheetCellValue.NumberCell(Convert.ToDouble(v, System.Globalization.CultureInfo.InvariantCulture))
        }).ToList()).ToList()
    };
    internal static SpreadsheetRangeData Model() => Source(
        ["Response", "Dose", "Treatment"], [2, 1, "Control"], [5, 2, "Active"], [6, 3, "Control"],
        [9, 4, "Active"], [11, 5, "Control"], [14, 6, "Active"], [15, 7, "Control"], [18, 8, "Active"]);
    internal static SpreadsheetRangeData Repeated() => Source(
        ["Subject", "Visit", "Response", "Treatment", "Dose"],
        ["S1", 1, 10, "Control", 1], ["S1", 2, 11, "Control", 2],
        ["S2", 1, 12, "Active", 1], ["S2", 2, 13, "Active", 2],
        ["S3", 1, 14, "Control", 1], ["S3", 2, 15, "Control", 2]);
    internal static SpecificationDraft ModelDraft()
    {
        var draft = new SpecificationDraft(); draft.SetPredictors([1, 2]); draft.SetFactors([2]); draft.SetReference(2, 1);
        return draft;
    }
    private static SpecificationDraft RepeatedDraft(bool mixed = false) => new() {
        Family = mixed ? SpecificationFamily.Mixed : SpecificationFamily.Repeated,
        Subject = 0, Visit = 1, Response = 2, Group = 3, AscendingVisits = true
    };
    private static void Has(SpecificationPreviewData preview, string code)
    { Assert.IsFalse(preview.Valid); Assert.IsTrue(preview.Issues.Any(i => i.Code == code), string.Join(" | ", preview.Issues.Select(i => i.Code + ": " + i.Message))); }

    [DataTestMethod]
    [DataRow(false, false)] [DataRow(true, false)] [DataRow(true, true)]
    public void ModelVisualTermsMatchCommonExactMatricesAndProvenance(bool interaction, bool polynomial)
    {
        var source = Model(); var draft = ModelDraft();
        draft.AddTerm(new(SpecificationTermKind.Main, 1)); draft.AddTerm(new(SpecificationTermKind.Main, 2));
        if (interaction) draft.AddTerm(new(SpecificationTermKind.Interaction, 1, 2));
        if (polynomial) draft.AddTerm(new(SpecificationTermKind.Polynomial, 1, Degree: 2));
        var actual = SpecificationCompilation.Compile(source, draft);
        Assert.IsTrue(actual.Valid, string.Join(" | ", actual.Issues.Select(i => i.Message)));
        var expected = SpreadsheetModelInputCompiler.Compile(source, draft.ModelOptions(actual.Columns)).Data;
        var model = (SpreadsheetModelInput)actual.Specification!;
        CollectionAssert.AreEqual(expected.DesignMatrix, model.DesignMatrix);
        CollectionAssert.AreEqual(expected.Response, model.Response);
        CollectionAssert.AreEqual(expected.PredictorNames.ToArray(), model.PredictorNames.ToArray());
        Assert.AreEqual(3 + (interaction ? 1 : 0) + (polynomial ? 1 : 0), actual.DesignWidth);
        Assert.AreEqual(8, actual.Observations); Assert.AreEqual(4, model.SourceRows[0].WorksheetRow);
        Assert.AreEqual(2, model.SourceRows[0].SelectedRow); Assert.AreEqual("sheet-1", actual.WorksheetId);
        Assert.AreEqual(expected.NormalizedFormula, actual.NormalizedFormula);
        Assert.AreEqual("Control", model.CategoricalColumns[0].Levels[0].Value.TextValue);
        Assert.AreEqual(0d, model.DesignMatrix[0, 2]); Assert.AreEqual(1d, model.DesignMatrix[1, 2]);
        if (interaction) Assert.AreEqual(2d, model.DesignMatrix[1, 3]);
        if (polynomial) Assert.AreEqual(9d, model.DesignMatrix[2, 4]);
        source.Rows[1][1].NumberValue = 999; var matrix = model.DesignMatrix; matrix[0, 1] = 999;
        Assert.AreEqual(1d, model.DesignMatrix[0, 1]);
    }

    [DataTestMethod]
    [DataRow("'Dose' + factor('Treatment', ref=1)", true, 3)]
    [DataRow("'Dose' + factor('Treatment', ref=2)", false, 3)]
    [DataRow("'Dose' + factor('Treatment') + 'Dose':factor('Treatment') + 'Dose'^2", true, 5)]
    public void ManualGrammarRemainsCoreAuthority(string rhs, bool intercept, int width)
    {
        var draft = ModelDraft(); draft.ManualFormula = true; draft.FormulaText = rhs; draft.IncludeIntercept = intercept;
        var preview = SpecificationCompilation.Compile(Model(), draft);
        Assert.IsTrue(preview.Valid, string.Join(" | ", preview.Issues.Select(i => i.Message)));
        Assert.AreEqual(width, preview.DesignWidth);
        Assert.AreEqual(intercept, ((SpreadsheetModelInput)preview.Specification!).IncludesIntercept);
    }

    [DataTestMethod]
    [DataRow("Response ~ Dose", "model.rhs-only")]
    [DataRow("Dose + (", "model.formula")]
    [DataRow("factor(Dose)^2", "model.formula")]
    [DataRow("unknown", "model.formula")]
    [DataRow("'Dose'^2147483647", "spec.polynomial-limit")]
    [DataRow("factor('Treatment', ref=99)", "model.design")]
    public void MalformedOrUnsupportedFormulaIsAnError(string formula, string code)
    { var draft = ModelDraft(); draft.ManualFormula = true; draft.FormulaText = formula; Has(SpecificationCompilation.Compile(Model(), draft), code); }

    [DataTestMethod]
    [DataRow("overlap", "model.predictor-role")] [DataRow("duplicate", "model.predictor-role")]
    [DataRow("factor", "model.category-role")] [DataRow("response", "model.response-role")]
    [DataRow("removed-term", "spec.term-role")]
    public void RolesNeverSilentlyDropInvalidAssignments(string defect, string code)
    {
        var draft = ModelDraft();
        switch (defect) {
            case "overlap": draft.SetPredictors([0, 1]); break;
            case "duplicate": draft.SetPredictors([1, 1]); break;
            case "factor": draft.SetFactors([0]); break;
            case "response": draft.Response = 9; break;
            case "removed-term": draft.AddTerm(new(SpecificationTermKind.Main, 2)); draft.SetPredictors([1]); break;
        }
        Has(SpecificationCompilation.Compile(Model(), draft), code);
    }

    [TestMethod]
    public void RotatedResponseAndQuotedCaptionRoundTripWithoutInjection()
    {
        var source = Source(["Dose' + factor(nope)", "Response", "Treatment"], [1, 2, "A"], [2, 4, "B"], [3, 5, "A"], [4, 7, "B"]);
        var draft = ModelDraft(); draft.Response = 1; draft.SetPredictors([0, 2]);
        var preview = SpecificationCompilation.Compile(source, draft);
        Assert.IsTrue(preview.Valid, string.Join(" | ", preview.Issues.Select(i => i.Message)));
        StringAssert.Contains(preview.NormalizedFormula, "'Dose'' + factor(nope)'");
        Assert.AreEqual(3, preview.DesignWidth);
    }

    [TestMethod]
    public void ExpansionBudgetRejectsLargeCategoricalInteractionBeforeMaterialization()
    {
        var source = Source(Enumerable.Range(0, 31).Select(i => i == 0 ? new object?[] { "Response", "Left", "Right" } : [i, "L" + i, "R" + i]).ToArray());
        var draft = ModelDraft(); draft.SetFactors([1, 2]); draft.ManualFormula = true; draft.FormulaText = "factor('Left'):factor('Right')";
        Has(SpecificationCompilation.Compile(source, draft), "spec.design-budget");
    }

    [TestMethod]
    public void ReferenceTypedIdentityCannotSilentlyChangeWhenCategoryEncodingChanges()
    {
        var source = Model(); var draft = ModelDraft();
        var baseline = (SpreadsheetModelInput)SpecificationCompilation.Compile(source, draft).Specification!;
        draft.SetReference(2, 1, baseline.CategoricalColumns[0].Levels[0].Value.Key);
        source.Rows[1][2] = SpreadsheetCellValue.TextCell("NewControl");
        Has(SpecificationCompilation.Compile(source, draft), "spec.reference-stale");
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public void RepeatedAndMixedMapDirectlyToExistingParsers(bool mixed)
    {
        var source = Repeated(); var draft = RepeatedDraft(mixed);
        if (mixed) { draft.SetFixedEffects([4]); draft.SetRandomGroups([0]); }
        var preview = SpecificationCompilation.Compile(source, draft);
        Assert.IsTrue(preview.Valid, string.Join(" | ", preview.Issues.Select(i => i.Message)));
        Assert.AreEqual(6, preview.Observations); Assert.AreEqual(0, preview.DesignWidth);
        var rows = mixed ? ((SpreadsheetMixedModelInput)preview.Specification!).Rows.Select(r => r.Repeated).ToArray()
            : ((SpreadsheetRepeatedSubjectInput)preview.Specification!).Observations.ToArray();
        var expected = LongitudinalParsers.ParseRepeatedSubjects(source, draft.RepeatedOptions()).Data;
        CollectionAssert.AreEqual(expected.Observations.Select(r => r.Subject.Key).ToArray(), rows.Select(r => r.Subject.Key).ToArray());
        CollectionAssert.AreEqual(expected.Observations.Select(r => r.WorksheetRow).ToArray(), rows.Select(r => r.WorksheetRow).ToArray());
        Assert.AreEqual(4, rows[0].WorksheetRow); Assert.AreEqual(2, rows[0].SelectedRow);
        Assert.AreNotEqual(rows[0].Subject.Key, rows[0].TreatmentGroup.Key);
        if (mixed) {
            var expectedMixed = LongitudinalParsers.ParseMixedModel(source, draft.RepeatedOptions()).Data;
            CollectionAssert.AreEqual(expectedMixed.CovariateNames.ToArray(), ((SpreadsheetMixedModelInput)preview.Specification!).CovariateNames.ToArray());
        }
    }

    [DataTestMethod]
    [DataRow("duplicate", "repeat.duplicate-visit")] [DataRow("missing", "repeat.missing")]
    [DataRow("order", "repeat.visit-order")] [DataRow("time", "repeat.visit-type")]
    [DataRow("date", "repeat.date-origin")] [DataRow("same-role", "repeat.options")]
    [DataRow("group-as-subject", "repeat.duplicate-visit")]
    public void RepeatedInvalidRowsAndSubjectGroupMixupKeepCommonDiagnostics(string defect, string code)
    {
        var source = Repeated(); var draft = RepeatedDraft();
        switch (defect) {
            case "duplicate": source.Rows[2][1] = SpreadsheetCellValue.NumberCell(1); break;
            case "missing": source.Rows[2][1] = SpreadsheetCellValue.BlankCell(); break;
            case "order": source.Rows[2][1] = SpreadsheetCellValue.NumberCell(0); break;
            case "time": source.Rows[2][1] = SpreadsheetCellValue.TextCell("yesterday"); break;
            case "date": draft.DateVisits = true; source.DateSystem = SpreadsheetDateSystem.Unknown; break;
            case "same-role": draft.Subject = draft.Group; break;
            case "group-as-subject": draft.Subject = 3; draft.Group = -1; break;
        }
        Has(SpecificationCompilation.Compile(source, draft), code);
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public void TypedCategoryOrderCaseAndMissingPoliciesMatchCommon(bool ignoreCase)
    {
        var source = Source(["Category"], ["1"], [1], [true], ["A"], ["a"], [null], ["1"]);
        var draft = new SpecificationDraft { Family = SpecificationFamily.Categorical, IgnoreCase = ignoreCase, Missing = SpreadsheetIncompleteObservationPolicy.Skip };
        var actual = SpecificationCompilation.Compile(source, draft);
        var expected = RectangularCategoricalParsers.ParseCategoricalVector(source, draft.CategoryOptions());
        Assert.IsTrue(actual.Valid); var categories = (SpreadsheetCategoricalVectorInput)actual.Specification!;
        CollectionAssert.AreEqual(expected.Data.Levels.Select(l => l.Key).ToArray(), categories.Levels.Select(l => l.Key).ToArray());
        Assert.AreEqual(ignoreCase ? 4 : 5, categories.Levels.Count);
        Assert.AreEqual(SpreadsheetCellKind.Text, categories.Levels[0].Kind);
        Assert.AreEqual(SpreadsheetCellKind.Number, categories.Levels[1].Kind);
        Assert.IsTrue(actual.Issues.Any(i => i.Severity == SpreadsheetValidationSeverity.Warning));
    }

    [DataTestMethod]
    [DataRow("duplicate", "header.duplicate")] [DataRow("empty-header", "header.invalid")]
    [DataRow("native-error", "cell.excel-error")] [DataRow("nonfinite", "cell.invalid-value")]
    [DataRow("blank", "model.incomplete")] [DataRow("auto", "header.ambiguous")]
    public void HeaderAndCellDiagnosticsKeepCoordinates(string defect, string code)
    {
        var source = Model(); var draft = ModelDraft();
        switch (defect) {
            case "duplicate": source.Rows[0][1] = SpreadsheetCellValue.TextCell("response"); break;
            case "empty-header": source.Rows[0][1] = SpreadsheetCellValue.TextCell(""); break;
            case "native-error": draft.Family = SpecificationFamily.Categorical; draft.Category = 2; source.Rows[2][2] = SpreadsheetCellValue.ErrorCell("#N/A", "native-42"); break;
            case "nonfinite": source.Rows[2][1] = SpreadsheetCellValue.NumberCell(double.NaN); break;
            case "blank": source.Rows[2][1] = SpreadsheetCellValue.BlankCell(); break;
            case "auto": draft.Header = SpreadsheetHeaderMode.Auto; break;
        }
        var preview = SpecificationCompilation.Compile(source, draft); Has(preview, code);
        var issue = preview.Issues.First(i => i.Code == code);
        if (defect != "auto") { Assert.IsTrue(issue.WorksheetRow >= 3); Assert.IsTrue(issue.WorksheetColumn >= 2); }
    }

    private static async Task<(SpecificationBuilderState State, AnalysisPilotTests.Workbook Workbook, AnalysisPilotTests.Host Host)> State()
    {
        var source=WorksheetDiscoveryTests.AtRowOne(Model());var host=new AnalysisPilotTests.Host();
        var workbook=new AnalysisPilotTests.Workbook(source);host.ConfigureWorksheet(source);
        var state=new SpecificationBuilderState(workbook,host);await state.InitializeAsync();
        state.Draft.Response=0;state.Draft.SetPredictors([1,2]);state.Draft.SetFactors([2]);return(state,workbook,host);
    }

    [TestMethod]
    public async Task LocalRoleAndTermEditingHasNoAdditionalReadsWritesOrCompiledPreview()
    {
        var f=await State();await using var state=f.State;var reads=f.Workbook.Reads;var pins=f.Host.Pins;
        state.Draft.AddTerm(new(SpecificationTermKind.Main,1));state.Draft.IncludeIntercept=false;
        state.Draft.ManualFormula=true;state.Draft.FormulaText="'Dose' + factor('Treatment')";
        Assert.AreEqual(reads,f.Workbook.Reads);Assert.AreEqual(pins,f.Host.Pins);Assert.AreEqual(0,f.Workbook.Writes);Assert.IsNull(state.Preview);
        await state.PreviewAsync();Assert.AreEqual(SpecificationStatus.Ready,state.Status,state.Code);
        Assert.AreEqual(reads+1,f.Workbook.Reads);Assert.AreEqual(0,f.Host.Inspections);
        state.Draft.IncludeIntercept=true;Assert.IsNull(state.Preview);Assert.AreEqual(SpecificationStatus.DraftChanged,state.Status);
        Assert.AreEqual(reads+1,f.Workbook.Reads);await state.PreviewAsync();Assert.AreEqual(reads+2,f.Workbook.Reads);Assert.AreEqual(SpecificationStatus.Ready,state.Status);
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task SourceAndDocumentInvalidationAllowNextExplicitPreviewWithoutReload(bool retry)
    {
        var f=await State();await using var state=f.State;await state.PreviewAsync();
        if(retry){state.DocumentChanged();await state.InitializeAsync();state.Draft.Response=0;state.Draft.SetPredictors([1,2]);state.Draft.SetFactors([2]);}
        else f.Host.ChangeDocument();
        Assert.IsNull(state.Preview);Assert.AreNotEqual(SpecificationStatus.Ready,state.Status);
        await state.PreviewAsync();Assert.AreEqual(SpecificationStatus.Ready,state.Status);
        f.Host.Catalog.Worksheets=[new("other-sheet","Other","Visible")];f.Host.Catalog.ActiveWorksheetId="other-sheet";
        await state.RefreshAsync();Assert.AreEqual("user.document-changed",state.Code);Assert.IsNull(state.Preview);Assert.IsFalse(state.Busy);
    }

    [DataTestMethod]
    [DataRow("user.document-changed")] [DataRow("office.unsupported-api")] [DataRow("input.range-cells")]
    public async Task HostRejectsDeletedStaleOrUnsupportedWorksheetBeforePayload(string code)
    {
        var f=await State();await using var state=f.State;var reads=f.Workbook.Reads;
        f.Host.Catalog.Code=code;await state.PreviewAsync();
        Assert.AreEqual(code,state.Code);Assert.IsNull(state.Preview);Assert.AreEqual(reads,f.Workbook.Reads);Assert.AreEqual(0,f.Workbook.Writes);
        f.Host.Catalog.Code="workbook.ok";await state.PreviewAsync();Assert.AreEqual(SpecificationStatus.Ready,state.Status);
    }

    [TestMethod]
    public async Task WorksheetPreviewReadsOnlyTheRealBoundedSource()
    {
        var f=await State();await using var state=f.State;
        f.Workbook.ResolveRange=reference=>{
            Assert.AreEqual("sheet-1",reference.WorksheetId);Assert.AreEqual(1,reference.StartRow);
            Assert.AreEqual(f.Workbook.Range.RowCount,reference.RowCount);Assert.IsTrue(reference.RowCount<1048576);return f.Workbook.Range;
        };
        await state.PreviewAsync();Assert.AreEqual(SpecificationStatus.Ready,state.Status);
        Assert.AreEqual(f.Workbook.Range.Address,state.Preview!.Address);Assert.AreEqual(0,f.Workbook.Writes);
    }

    [TestMethod]
    public async Task QueuedRetirementCannotBeOvertakenByFreshPreviewAndCancelRecovers()
    {
        var f=await State();await using var state=f.State;await state.PreviewAsync();var pins=f.Host.Pins;
        var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);f.Host.Retire=()=>release.Task;
        state.Draft.IncludeIntercept=false;var preview=state.PreviewAsync();Assert.AreEqual(pins,f.Host.Pins);Assert.IsTrue(state.Busy);
        state.Cancel();await preview;Assert.IsFalse(state.Busy);Assert.IsNull(state.Preview);
        release.SetResult();f.Host.Retire=null;await state.PreviewAsync();Assert.AreEqual(SpecificationStatus.Ready,state.Status);Assert.AreEqual(pins+1,f.Host.Pins);
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task LatePinAfterCancelOrDisposeCannotPublishReady(bool dispose)
    {
        var f=await State();var state=f.State;f.Host.Pending=new(TaskCreationOptions.RunContinuationsAsynchronously);
        var preview=state.PreviewAsync();if(dispose)await state.DisposeAsync();else state.Cancel();await preview;
        f.Host.Pending.SetResult(new(){Code="workbook.ok",SourceToken="late",SessionIdentity="late"});
        Assert.IsNull(state.Preview);Assert.IsFalse(state.Busy);Assert.AreEqual(0,f.Workbook.Writes);await state.DisposeAsync();
    }

    [TestMethod]
    public async Task InterruptedDiscoveryLateCallbacksAndDisposalDoNotLockNextOwner()
    {
        var f=await State();var state=f.State;f.Host.PendingCatalog=new(TaskCreationOptions.RunContinuationsAsynchronously);
        var refresh=state.RefreshAsync();state.Cancel();await refresh;f.Host.PendingCatalog.SetResult(f.Host.Catalog);f.Host.PendingCatalog=null;
        f.Host.EmitWorksheet("wrong-owner");await state.RefreshAsync();Assert.AreEqual("spec.variables-ready",state.Code);Assert.IsFalse(state.Busy);Assert.AreEqual(3,state.Columns.Count);
        await state.PreviewAsync();Assert.AreEqual(SpecificationStatus.Ready,state.Status);
        await state.DisposeAsync();f.Host.ChangeDocument();Assert.AreEqual(SpecificationStatus.Disposed,state.Status);
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public void NumericAndMixedFactorLevelsRetainCoreReferenceSemantics(bool mixed)
    {
        var source = Source(["Response", "Dose", "Treatment"], [1, 1, mixed ? "1" : 1], [2, 2, 2], [3, 3, mixed ? "1" : 1], [4, 4, 2]);
        var draft = ModelDraft(); draft.SetReference(2, 2);
        var preview = SpecificationCompilation.Compile(source, draft); Assert.IsTrue(preview.Valid);
        var model = (SpreadsheetModelInput)preview.Specification!;
        Assert.AreEqual(mixed ? SpreadsheetCellKind.Text : SpreadsheetCellKind.Number, model.CategoricalColumns[0].Levels[0].Value.Kind);
        Assert.AreEqual(1d, model.DesignMatrix[0, 2]); Assert.AreEqual(0d, model.DesignMatrix[1, 2]);
    }

    [TestMethod]
    public void ExplicitSkipReportsDroppedRowsAndNoneHeaderUsesCommonGeneratedNames()
    {
        var source = Model(); source.Rows[2][1] = SpreadsheetCellValue.BlankCell();
        var draft = ModelDraft(); draft.Missing = SpreadsheetIncompleteObservationPolicy.Skip;
        var preview = SpecificationCompilation.Compile(source, draft); Assert.IsTrue(preview.Valid);
        Assert.AreEqual(7, preview.Observations); Assert.IsTrue(preview.Issues.Any(i => i.Code == "model.incomplete-skipped" && i.WorksheetRow == 5));
        Assert.AreEqual(6, ((SpreadsheetModelInput)preview.Specification!).SourceRows[1].WorksheetRow);
        source.Rows.RemoveAt(0); source.RowCount--; source.StartRow++; source.Address = "B4:D11";
        draft.Header = SpreadsheetHeaderMode.None;
        preview = SpecificationCompilation.Compile(source, draft); Assert.IsTrue(preview.Valid);
        StringAssert.Contains(preview.NormalizedFormula, "'Column 2'");
        Assert.AreEqual(4, ((SpreadsheetModelInput)preview.Specification!).SourceRows[0].WorksheetRow);
    }

    [TestMethod]
    public void TooFewCompleteModelRowsReturnsCommonError()
    {
        var source = Source(["Response", "Dose", "Treatment"], [1, 1, "A"]);
        Has(SpecificationCompilation.Compile(source, ModelDraft()), "model.too-few");
    }

    [TestMethod]
    public async Task ChangedSourceAfterCompileCannotPublishReadyAndReadFailureCanRecover()
    {
        var f = await State(); await using var state = f.State;
        f.Host.CheckCode = "user.document-changed";
        await state.PreviewAsync(); Assert.IsNull(state.Preview); Assert.AreEqual(SpecificationStatus.DocumentChanged, state.Status);
        f.Host.CheckCode = "workbook.ok"; f.Workbook.ReadCode = "input.time-budget";
        await state.PreviewAsync(); Assert.AreEqual("input.time-budget", state.Code); Assert.IsNull(state.Preview);
        f.Workbook.ReadCode = "workbook.ok"; await state.PreviewAsync();
        Assert.AreEqual(SpecificationStatus.Ready, state.Status); Assert.IsFalse(string.IsNullOrEmpty(state.Preview!.HostSessionIdentity));
        Assert.AreEqual(0, f.Workbook.Writes);
    }

    [TestMethod]
    public async Task UncertainPilotWriteRemainsLatchedForSpecificationBuilder()
    {
        var f = await State(); await using var state = f.State;
        var intent = new WriteIntent("plan", "target", new() { StartRow = 1, StartColumn = 10, WorksheetName = "Input" },
            [new WriteRegion("table", new OutputRectangle(1, 10, 2, 11))]);
        f.Host.RecordWriteOutcome(new(OutputWriteState.PartiallyWritten, "output.mutation-uncertain", "Uncertain", intent, true));
        var reads=f.Workbook.Reads;Assert.IsFalse(state.CanPreview); await state.PreviewAsync(); Assert.AreEqual(reads, f.Workbook.Reads);
        state.DocumentChanged(); Assert.IsTrue(state.RequiresRecovery); Assert.AreEqual(0, f.Workbook.Writes);
    }
}
