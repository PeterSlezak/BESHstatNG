using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using BESHStatNG.AppInfrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class FormatInputAmendmentTests
{
    private static SpreadsheetRangeData Vector(int col, int start, IEnumerable<SpreadsheetCellValue> cells)
    {
        var rows = cells.Select(c => new List<SpreadsheetCellValue> { c }).ToList();
        var address = $"{(char)(64 + col)}{start}:{(char)(64 + col)}{start + rows.Count - 1}";
        return new() {WorksheetId="sheet-1",WorksheetName="Input",StartRow=start,StartColumn=col,ColumnCount=1,RowCount=rows.Count,Address=address,Rows=rows};
    }
    private static (SpreadsheetRangeData Category, SpreadsheetRangeData Values) Long(bool headers = true, bool populated = false)
    {
        var categories = Enumerable.Range(0,9).Select(i=>SpreadsheetCellValue.TextCell(((char)('A'+i/3)).ToString())).ToList();
        var values = Enumerable.Range(1,9).Select(i=>SpreadsheetCellValue.NumberCell(i)).ToList();
        if(headers){categories.Insert(0,SpreadsheetCellValue.TextCell("Group"));values.Insert(0,SpreadsheetCellValue.TextCell("Value"));}
        values.Add(SpreadsheetCellValue.BlankCell());values.Add(populated?SpreadsheetCellValue.NumberCell(0):SpreadsheetCellValue.BlankCell());
        return (Vector(6,headers?1:2,categories),Vector(5,headers?1:2,values));
    }
    [DataTestMethod]
    [DataRow(0,false)] [DataRow(1,false)] [DataRow(2,false)] [DataRow(0,true)] [DataRow(1,true)] [DataRow(2,true)]
    public async Task OwnerUnequalRangesUseMatchingRowsAndRetainBothOriginalOverhangs(int header, bool populated)
    {
        var (category, values)=Long(header!=1,populated);var host=new AnalysisPilotTests.Host();
        using var draft=new AnovaOptionDraft(new());var workbook=new AnalysisPilotTests.Workbook(category) {ResolveRange=r=>r.StartColumn==6?category:values};
        await using var state=new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova),draft,workbook,host);
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat,(SpreadsheetHeaderMode)header);
        state.CategoryReference.Edit(category.ToReference().QualifiedAddress);state.ValueReference.Edit(values.ToReference().QualifiedAddress);
        await state.RunAsync();Assert.AreEqual("analysis.completed",state.Code);Assert.IsFalse(state.ShowPreview);
        CollectionAssert.AreEqual(new[]{3,3,3},state.Preview!.GroupCounts.ToArray());
        var notice=state.InputNotices.Single();Assert.AreEqual("input.long-overhang",notice.Code);Assert.AreEqual(2,notice.Count);Assert.AreEqual(populated?1:0,notice.Populated);
        Assert.AreEqual(values.RowCount,host.Sources![1].Snapshot.RowCount);Assert.AreEqual(2,workbook.Reads);
        await state.WriteResultsAsync();Assert.AreEqual(1,workbook.Writes);Assert.AreEqual(OutputWriteState.Completed,state.WriteOutcome!.State);
    }
    [TestMethod]
    public void DifferentStartsWithoutHeadersPairByWorksheetRowAndMapActualIssuePositions()
    {
        var (category,values)=Long(false); category.StartRow=3;category.Rows.RemoveAt(category.Rows.Count-1);category.RowCount--;
        var policy=new GroupedInputPolicy(SpreadsheetGroupedOrientation.LongFormat,SpreadsheetHeaderMode.None,new());
        var parsed=GroupedInput.Parse([category,values],policy);
        Assert.IsTrue(parsed.Valid);Assert.AreEqual(3,parsed.View.FirstRow);Assert.AreEqual(8,parsed.ParserInput.RowCount);
        Assert.AreEqual(2,parsed.ParserInput.Rows[0][1].NumberValue); // Physical row 3, never value 1 from row 2.
        values.Rows[3][0]=SpreadsheetCellValue.ErrorCell("#N/A");
        parsed=GroupedInput.Parse([category,values],policy);
        Assert.IsFalse(parsed.Valid);Assert.IsTrue(parsed.Issues.Any(i=>i.WorksheetRow==5&&i.WorksheetColumn==5&&i.SelectedRow==4));
    }
    [DataTestMethod]
    [DataRow(0)] [DataRow(2)]
    public void DifferentHeaderStartsRejectWithoutInventingAHeader(int header)
    {
        var (category,values)=Long();category.StartRow=2;
        var error=Assert.ThrowsException<GroupedInputException>(()=>GroupedInput.Parse([category,values],new(SpreadsheetGroupedOrientation.LongFormat,(SpreadsheetHeaderMode)header,new())));
        Assert.AreEqual("input.long-header-alignment",error.Code);
    }
    [DataTestMethod]
    [DataRow(0)] [DataRow(1)] [DataRow(2)]
    public void FutureTwoGroupAndMatchedBlockPoliciesReuseTheSameLayoutAndCommonParser(int header)
    {
        using var f=AnalysisPilotTests.Fixture("anova-base");var wide=RangeReferenceTests.Arrangement(AnalysisPilotTests.Range(f.RootElement),SpreadsheetGroupedOrientation.GroupsInColumns,(SpreadsheetHeaderMode)header);
        var exactlyTwo=new GroupedInputPolicy(SpreadsheetGroupedOrientation.GroupsInColumns,(SpreadsheetHeaderMode)header,new(MaximumGroups:2));
        Assert.IsTrue(GroupedInput.Parse([wide],exactlyTwo).Issues.Any(i=>i.Code=="input.group-count"));
        wide.ColumnCount=2;wide.Rows.ForEach(r=>r.RemoveAt(2));Assert.IsTrue(GroupedInput.Parse([wide],exactlyTwo).Valid);
        var blocks=exactlyTwo with {Constraints=new(RequireMatchedBlocks:true)};
        Assert.IsTrue(GroupedInput.Parse([wide],blocks).Valid);
        // Equal group counts alone do not prove matched blocks: these blanks occur in different rows.
        int first=header==1?0:1;wide.Rows[first][0]=SpreadsheetCellValue.BlankCell();wide.Rows[first+1][1]=SpreadsheetCellValue.BlankCell();
        Assert.IsTrue(GroupedInput.Parse([wide],exactlyTwo).Valid);
        Assert.IsTrue(GroupedInput.Parse([wide],blocks).Issues.Any(i=>i.Code=="input.incomplete-blocks"));
        var rows=new SpreadsheetRangeData {WorksheetId="sheet-1",WorksheetName="Input",StartRow=1,StartColumn=1,RowCount=wide.ColumnCount,ColumnCount=wide.RowCount,
            Rows=Enumerable.Range(0,wide.ColumnCount).Select(c=>wide.Rows.Select(r=>r[c]).ToList()).ToList()};
        Assert.IsTrue(GroupedInput.Parse([rows],blocks with {Orientation=SpreadsheetGroupedOrientation.GroupsInRows}).Issues.Any(i=>i.Code=="input.incomplete-blocks"));
        // This is input completeness only; no Mann-Whitney or Friedman handler is registered/enabled here.
    }
    [DataTestMethod]
    [DataRow("'H\u00e1rok 1'!$F:$F",6,1)] [DataRow("'Data'!$A:$C",1,3)] [DataRow("'O''Brien'!XFD:XFD",16384,1)]
    public void EntireColumnShorthandUsesCommonQualifiedIdentityAndFiniteNativeBounds(string text,int col,int count)
    {
        var r=RangeReferenceState.Parse(text,false);Assert.IsTrue(RangeReferenceState.WholeColumns(r));Assert.AreEqual(col,r.StartColumn);Assert.AreEqual(count,r.ColumnCount);
        Assert.ThrowsException<ArgumentException>(()=>RangeReferenceState.Parse(text,true));
    }
    [DataTestMethod]
    [DataRow("Data!1:3")] [DataRow("Data!A:XFD")] [DataRow("'[Other.xlsx]Data'!F:F")] [DataRow("Data!F:F,H:H")]
    public void UnboundedUnsupportedSyntaxCannotBypassCommonValidation(string text)
    {
        try {RangeReferenceState.Parse(text,false);Assert.Fail("Unsupported input admitted");}catch(ArgumentException){}
    }
    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task OneClickWriteInspectsOncePausesForConsentOrWritesAndRejectsStaleConsent(bool occupied)
    {
        using var f=AnalysisPilotTests.Fixture("anova-base");using var draft=new AnovaOptionDraft(new());
        var workbook=new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));var host=new AnalysisPilotTests.Host {Occupancy=occupied?OutputOccupancy.Occupied:OutputOccupancy.Empty};
        await using var state=new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova),draft,workbook,host);
        state.InputReference.Edit("'Input'!A1:C4");await state.RunAsync();await state.WriteResultsAsync();
        Assert.AreEqual(1,host.Inspections);Assert.AreEqual(1,workbook.Reads);
        if(occupied){Assert.AreEqual(0,workbook.Writes);Assert.IsTrue(state.Inspection!.ConfirmationRequired);state.SetDestination(SpreadsheetOutputDestination.NewWorksheet,"A1","Other");await state.WriteAsync(true);Assert.AreEqual(0,workbook.Writes);}
        else{Assert.AreEqual(OutputWriteState.Completed,state.WriteOutcome!.State);await state.WriteResultsAsync();Assert.AreEqual(1,workbook.Writes);}
    }
    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task OccupiedOneClickWriteUsesExplicitNoOrYesAndNeverRepeatsMutation(bool approve)
    {
        using var f=AnalysisPilotTests.Fixture("anova-base");using var draft=new AnovaOptionDraft(new());
        var workbook=new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));var host=new AnalysisPilotTests.Host {Occupancy=OutputOccupancy.Occupied};
        await using var state=new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova),draft,workbook,host);
        state.InputReference.Edit("'Input'!A1:C4");await state.RunAsync();await state.WriteResultsAsync();Assert.AreEqual(0,workbook.Writes);
        await state.WriteAsync(approve);Assert.AreEqual(approve?OutputWriteState.Completed:OutputWriteState.CancelledByUser,state.WriteOutcome!.State);
        Assert.AreEqual(approve,workbook.Consent!.Approved);Assert.IsFalse(state.CanWrite);Assert.AreEqual(!approve,state.Current);
        await state.WriteAsync(approve);Assert.AreEqual(1,workbook.Writes);Assert.AreEqual(1,workbook.Reads);
        if(!approve){await state.WriteResultsAsync();Assert.AreEqual(2,host.Inspections);Assert.AreEqual(1,workbook.Reads);}
    }
    [TestMethod]
    public async Task NormalizedWholeColumnsRetainEditableOriginalScopeAndReadOnlyBoundedEffectiveRows()
    {
        using var f=AnalysisPilotTests.Fixture("anova-base");using var draft=new AnovaOptionDraft(new());
        var wide=AnalysisPilotTests.Range(f.RootElement);wide.StartRow=7;wide.Address="A7:C10";
        var workbook=new AnalysisPilotTests.Workbook(wide){ResolveRange=r=>{Assert.AreEqual(7,r.StartRow);Assert.AreEqual(4,r.RowCount);return wide;}};
        var host=new AnalysisPilotTests.Host {ResolveReply=original=>new(){Code="workbook.ok",Handle="normalized",OriginalReference=original,Reference=wide.ToReference()}};
        await using var state=new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova),draft,workbook,host);
        state.InputReference.Edit("'Input'!$A:$C");Assert.AreEqual(0,workbook.Reads);await state.RunAsync();
        Assert.AreEqual("analysis.completed",state.Code);Assert.AreEqual("'Input'!A:C",state.InputReference.Text);Assert.AreEqual(1,workbook.Reads);
        Assert.AreEqual("input.columns-narrowed",state.InputNotices.Single().Code);Assert.AreEqual(4,state.InputNotices.Single().Count);
    }
    [TestMethod]
    public async Task BothWholeColumnEffectiveSizesArePreflightedBeforeAnyPayloadRead()
    {
        using var f=AnalysisPilotTests.Fixture("anova-base");using var draft=new AnovaOptionDraft(new());var workbook=new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        var host=new AnalysisPilotTests.Host {ResolveReply=original=>{
            var effective=SpreadsheetRangeReference.FromAddress(original.StartColumn==5?"E1:E6000":"J1:J6000","Input");effective.WorksheetId="sheet-1";
            return new(){Code="workbook.ok",Handle="normalized",OriginalReference=original,Reference=effective};}};
        await using var state=new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova),draft,workbook,host);
        state.SetInterpretation(SpreadsheetGroupedOrientation.LongFormat,SpreadsheetHeaderMode.Required);state.CategoryReference.Edit("'Input'!E:E");state.ValueReference.Edit("'Input'!J:J");
        await state.RunAsync();Assert.AreEqual("input.total-cells",state.Code);Assert.AreEqual(0,workbook.Reads);Assert.AreEqual(0,host.Pins);Assert.AreEqual(0,workbook.Writes);
    }
    [DataTestMethod]
    [DataRow("columns")] [DataRow("sheet")]
    public async Task MalformedNormalizationCannotDropSelectedColumnsOrChangeThePhysicalSheet(string defect)
    {
        using var f=AnalysisPilotTests.Fixture("anova-base");using var draft=new AnovaOptionDraft(new());var workbook=new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement));
        var host=new AnalysisPilotTests.Host {ResolveReply=original=>{
            var effective=SpreadsheetRangeReference.FromAddress(defect=="columns"?"A1:B4":"A1:C4",defect=="sheet"?"Other":"Input");effective.WorksheetId="sheet-1";
            return new(){Code="workbook.ok",Handle="wrong",OriginalReference=original,Reference=effective};}};
        await using var state=new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova),draft,workbook,host);state.InputReference.Edit("'Input'!A:C");
        await state.RunAsync();Assert.AreEqual("input.invalid-reference",state.Code);Assert.AreEqual(0,workbook.Reads);Assert.AreEqual(0,host.Pins);Assert.AreEqual(0,workbook.Writes);
    }
    private static PreferenceSnapshot Preferences(int mode,bool upper)=>PreferenceSnapshot.Normalize(new() {SchemaVersion=1,DefaultAlpha=.05,DefaultRandomSeed=int.MinValue,TraceEnabled=false,
        PValues=new(){DecimalPlaces=4,SmallValueDisplay=(PValueSmallValueDisplay)mode,UseUpperBound=upper}}).Snapshot;
    [DataTestMethod]
    [DataRow("anova-base",0,false)] [DataRow("anova-welch-tukey",1,true)] [DataRow("anova-all-posthoc",2,true)] [DataRow("kw-base",0,true)] [DataRow("kw-dunn",1,false)]
    public void PresentationPreservesGoldenCellsGeometryAndUsesExistingEffectivePreferencePolicy(string fixture,int mode,bool upper)
    {
        using var f=AnalysisPilotTests.Fixture(fixture);var root=f.RootElement;using var draft=AnalysisPilotTests.Draft(root);
        var result=SharedAnalysisApplicationService.RunDocument(root.GetProperty("analysisIdProposed").GetString()!,GroupedNumericRangeParser.Parse(AnalysisPilotTests.Range(root)),draft.Snapshot());
        var prefs=Preferences(mode,upper);var wire=OutputPlanPresentation.Prepare(OutputDocument.FromAnalysis(result),new(){WorksheetName="Output",StartRow=7,StartColumn=4},.05,prefs);
        Assert.AreEqual(BESHStatNG.PValuePresentation.BuildExcelNumberFormat(prefs.CreateDisplaySettings()),wire.PvalueNumberFormat);
        Assert.AreEqual(7,wire.Tables[0].Bounds.StartRow);Assert.AreEqual(4,wire.Tables[0].Bounds.StartColumn);
        foreach(var expected in root.GetProperty("cells").EnumerateArray()){
            if(!expected.TryGetProperty("expectedNumber",out var number))continue;
            var cell=wire.Tables[expected.GetProperty("table").GetInt32()].Cells.Single(c=>c.Row==expected.GetProperty("row").GetInt32()&&c.Column==expected.GetProperty("column").GetInt32());
            Assert.AreEqual(number.GetDouble(),cell.Value.NumberValue!.Value,expected.GetProperty("tolerance").GetDouble());
        }
        Assert.IsTrue(wire.Tables.All(t=>!t.AutoFit));
        Assert.IsTrue(wire.Tables.SelectMany(t=>t.Cells).Where(c=>c.IsPValue).All(c=>c.NumberFormat==wire.PvalueNumberFormat));
        Assert.IsTrue(wire.Tables.SelectMany(t=>t.Cells).Where(c=>c.Role=="TopColumnHeader").All(c=>c.StrongEmphasis));
        for(int i=1;i<wire.Tables.Count;i++)Assert.IsTrue(wire.Tables[i].Bounds.StartRow>wire.Tables[i-1].Bounds.EndRow);
    }
    [DataTestMethod]
    [DataRow(0,false)] [DataRow(0,true)] [DataRow(1,false)] [DataRow(1,true)] [DataRow(2,false)] [DataRow(2,true)]
    public void SmallPValuesNearAlphaAndBlanksRetainNumericStorageAndFrozenSignificance(int mode,bool upper)
    {
        double alpha=.05;object?[,] values={{"Title",null},{"Label","p"},{"Small",1e-12},{"Below",Math.BitDecrement(alpha)},{"Equal",alpha},{"Above",Math.BitIncrement(alpha)},{"High",.9999999},{"Blank",null},{"Footer",null}};
        var model=new BESHStatNG.ResultTableOutputModel(values,headerTopRows:1,headerLeftColumns:1,footerRows:1,pvalueColumns:new[]{1},titleRows:1);
        var wire=OutputPlanPresentation.Prepare(new(new OutputArtifact[]{new OutputTableArtifact("table",model)}),new(){WorksheetName="Output",StartRow=1,StartColumn=1},alpha,Preferences(mode,upper));
        var p=wire.Tables[0].Cells.Where(c=>c.IsPValue).ToArray();
        CollectionAssert.AreEqual(new[]{true,true,true,false,false,false},p.Select(c=>c.IsSignificant).ToArray());
        Assert.AreEqual(1e-12,p[0].Value.NumberValue);Assert.AreEqual("number",p[0].Value.Kind);Assert.AreEqual("blank",p[^1].Value.Kind);
        Assert.IsFalse(wire.Tables[0].Cells[0].StrongEmphasis);Assert.AreEqual("Footer",wire.Tables[0].Cells[^1].Role);
    }
}
