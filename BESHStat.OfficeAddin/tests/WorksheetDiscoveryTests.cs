using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class WorksheetDiscoveryTests
{
    internal static SpreadsheetRangeData AtRowOne(SpreadsheetRangeData source,int? column=null)
    {
        var first=column??source.StartColumn;
        return new() {WorkbookId=source.WorkbookId,WorksheetId=source.WorksheetId,WorksheetName=source.WorksheetName,
            SelectionRevision=source.SelectionRevision,DateSystem=source.DateSystem,DateSerialOrigin=source.DateSerialOrigin,
            StartRow=1,StartColumn=first,RowCount=source.Rows.Count,ColumnCount=source.ColumnCount,
            Address=$"{BESHStatNG.RegressionVariableCatalog.NumberToLetters(first)}1:{BESHStatNG.RegressionVariableCatalog.NumberToLetters(first+source.ColumnCount-1)}{source.RowCount}",
            Rows=source.Rows.Select(r=>r.Select(c=>new SpreadsheetCellValue {Kind=c.Kind,NumberValue=c.NumberValue,TextValue=c.TextValue,
                BooleanValue=c.BooleanValue,NumberFormat=c.NumberFormat,NativeErrorCode=c.NativeErrorCode}).ToList()).ToList()};
    }
    internal sealed class Workbook(params SpreadsheetRangeData[] sources) : IWorkbookAdapter
    {
        public readonly Dictionary<string,SpreadsheetRangeData> Sheets=sources.ToDictionary(s=>s.WorksheetId);
        public int Reads,Writes;
        public List<SpreadsheetRangeReference> References=[];
        public string ReadCode="workbook.ok";
        public Task<WorkbookResult<SpreadsheetRangeData>> ReadRangeAsync(SpreadsheetRangeReference reference,InputSizeLimits? limits=null,CancellationToken cancellationToken=default)
        {Reads++;References.Add(reference);return Task.FromResult(new WorkbookResult<SpreadsheetRangeData>(ReadCode,ReadCode=="workbook.ok"?AtRowOne(Sheets[reference.WorksheetId]):null));}
        public Task<WorkbookResult<SpreadsheetRangeData>> ReadSelectionAsync(InputSizeLimits? limits=null,CancellationToken cancellationToken=default)=>throw new AssertFailedException("Worksheet discovery must not use selected cells.");
        public Task<WorkbookResult<SelectedRangeMetadata>> InspectSelectionAsync(CancellationToken cancellationToken=default)=>throw new AssertFailedException();
        public Task<WorkbookResult<HostCapabilitySnapshot>> GetCapabilitiesAsync(CancellationToken cancellationToken=default)=>throw new AssertFailedException();
        public Task<WriteResult> WriteAsync(OfficeOutputPlan plan,WriteIntent intent,WriteConsent? consent=null,CancellationToken cancellationToken=default){Writes++;throw new AssertFailedException("Specification never writes.");}
        public ValueTask DisposeAsync()=>ValueTask.CompletedTask;
    }
    internal static AnalysisPilotTests.Host Host(Workbook workbook,string? active=null)
    {
        var host=new AnalysisPilotTests.Host();
        host.Catalog=new(){Code="workbook.ok",DocumentIdentity="document-1",ActiveWorksheetId=active??workbook.Sheets.Keys.First(),
            Worksheets=workbook.Sheets.Values.Select(s=>new SourceWorksheet(s.WorksheetId,s.WorksheetName,"Visible")).ToList()};
        host.BoundsReply=id=> {
            if(!workbook.Sheets.TryGetValue(id,out var source))return new(){Code="user.document-changed"};
            var reference=SpreadsheetRangeReference.FromAddress(source.Address,source.WorksheetName);reference.WorksheetId=id;
            return new(){Code="workbook.ok",DocumentIdentity=host.Catalog.DocumentIdentity,Empty=false,Reference=reference};
        };
        return host;
    }
    private static SpreadsheetRangeData Data(params object?[][] rows)=>AtRowOne(SpecificationBuilderTests.Source(rows),1);
    private static SpecificationDraft ModelDraft()=>SpecificationBuilderTests.ModelDraft();
    private static WorksheetColumnCatalog Catalog(SpreadsheetRangeData source)=>new("document-1",source);

    [TestMethod]
    public async Task ActiveWorksheetDiscoversSixOwnerCaptionsWithoutPreviewOrSelection()
    {
        var data=Data(["y","trtn","age","base","subject","period"],[1,"T",20,0,"S1",1],[2,"C",30,1,"S2",2]);
        var workbook=new Workbook(data);var host=Host(workbook);
        await using var state=new SpecificationBuilderState(workbook,host);await state.InitializeAsync();
        CollectionAssert.AreEqual(new[]{"y | VarA","trtn | VarB","age | VarC","base | VarD","subject | VarE","period | VarF"},state.Columns.Select(c=>c.DisplayText).ToArray());
        Assert.AreEqual(data.WorksheetId,state.SelectedWorksheetId);Assert.AreEqual(1,workbook.Reads);Assert.AreEqual(0,workbook.Writes);
        Assert.IsNull(state.Preview);Assert.AreEqual("spec.variables-ready",state.Code);
        Assert.AreEqual(-1,state.Draft.Response);Assert.IsTrue(state.Columns.All(c=>c.WorksheetId==data.WorksheetId && c.DocumentIdentity=="document-1"));
    }

    [TestMethod]
    public void SparseProjectionHasNoSyntheticAddressAndRetainsPhysicalColumns()
    {
        var data=Data(["Response",null,"Dose",null,"Treatment"],[2,null,1,null,"Control"],[5,null,2,null,"Active"],[6,null,3,null,"Control"],[9,null,4,null,"Active"],[11,null,5,null,"Control"],[14,null,6,null,"Active"]);
        var catalog=Catalog(data);var draft=ModelDraft();
        CollectionAssert.AreEqual(new[]{1,3,5},catalog.Columns.Select(c=>c.WorksheetColumn).ToArray());
        CollectionAssert.AreEqual(new[]{0,1,2},catalog.Columns.Select(c=>c.Index).ToArray());
        Assert.AreEqual("",catalog.ParserCopy().Address);Assert.AreEqual(0,catalog.ParserCopy().StartColumn);
        for(int stage=0;stage<3;stage++)
        {
            if(stage==1){draft.AddTerm(new(SpecificationTermKind.Main,1));draft.AddTerm(new(SpecificationTermKind.Main,2));draft.AddTerm(new(SpecificationTermKind.Interaction,1,2));}
            if(stage==2)draft.AddTerm(new(SpecificationTermKind.Polynomial,1,Degree:2));
            var preview=SpecificationCompilation.Compile(catalog,draft);Assert.IsTrue(preview.Valid,string.Join(";",preview.Issues.Select(i=>i.Message)));
            var expected=SpreadsheetModelInputCompiler.Compile(catalog.ParserCopy(),draft.ModelOptions(preview.Columns)).Data;
            var model=(SpreadsheetModelInput)preview.Specification!;
            Assert.AreEqual(stage+3,model.DesignColumnCount);CollectionAssert.AreEqual(expected.DesignMatrix,model.DesignMatrix);
            Assert.AreEqual(5,model.CategoricalColumns[0].SelectedColumn);Assert.AreEqual("A1:E7",preview.Address);
            Assert.AreEqual(2,model.SourceRows[0].SelectedRow);Assert.AreEqual(2,model.SourceRows[0].WorksheetRow);
            Assert.AreEqual(data.WorksheetId,model.WorksheetId);StringAssert.Contains(preview.NormalizedFormula,"'Dose'");
        }
    }

    [DataTestMethod]
    [DataRow("duplicate","header.duplicate",4)] [DataRow("blank","header.invalid",1)]
    public void DistinctCaptionsDoNotWeakenRequiredHeaderValidation(string kind,string code,int physicalColumn)
    {
        var data=kind=="duplicate"?Data(["Age",null,null,"Age"],[1,null,null,2],[3,null,null,4]):Data([null,null,"Dose"],[1,null,2],[3,null,4]);
        var catalog=Catalog(data);var draft=new SpecificationDraft();draft.SetPredictors([1]);
        if(kind=="duplicate")CollectionAssert.AreEqual(new[]{"Age | VarA","Age | VarD"},catalog.Columns.Select(c=>c.DisplayText).ToArray());
        else Assert.AreEqual("VarA",catalog.Columns[0].DisplayText);
        var preview=SpecificationCompilation.Compile(catalog,draft);Assert.IsFalse(preview.Valid);
        var issue=preview.Issues.First(i=>i.Code==code);Assert.AreEqual(physicalColumn,issue.WorksheetColumn);Assert.AreEqual(physicalColumn,issue.SelectedColumn);
    }

    [DataTestMethod]
    [DataRow(SpreadsheetHeaderMode.None)] [DataRow(SpreadsheetHeaderMode.Auto)]
    public void NumericRowOneRemainsAnObservationWhenCommonChoosesNoHeader(SpreadsheetHeaderMode mode)
    {
        var data=Data([1,null,2],[3,null,4],[5,null,6]);var draft=new SpecificationDraft{Header=mode};draft.SetPredictors([1]);
        var preview=SpecificationCompilation.Compile(Catalog(data),draft);Assert.IsTrue(preview.Valid);
        var model=(SpreadsheetModelInput)preview.Specification!;Assert.AreEqual(3,model.ObservationCount);
        Assert.AreEqual(1,model.SourceRows[0].SelectedRow);Assert.AreEqual(1,model.SourceRows[0].WorksheetRow);Assert.AreEqual(1d,model.Response[0]);
    }

    [TestMethod]
    public void AutoAmbiguityIsNotHiddenByCaptionDiscovery()
    {
        var data=Data(["Response",null,"Group"],[1,null,"A"],[2,null,"B"]);var draft=new SpecificationDraft{Header=SpreadsheetHeaderMode.Auto};draft.SetPredictors([1]);draft.SetFactors([1]);
        var preview=SpecificationCompilation.Compile(Catalog(data),draft);Assert.IsFalse(preview.Valid);Assert.IsTrue(preview.Issues.Any(i=>i.Code=="header.ambiguous"));
    }

    [TestMethod]
    public void HeaderOnlyBooleanErrorAndTextColumnsAreNotDiscarded()
    {
        var data=Data(["Header only",null,null,null],[null,true,SpreadsheetCellValue.ErrorCell("#N/A","native-42"),"text"]);
        var catalog=Catalog(data);Assert.AreEqual(4,catalog.Columns.Count);
        Assert.AreEqual("Header only | VarA",catalog.Columns[0].DisplayText);Assert.AreEqual("VarC",catalog.Columns[2].DisplayText);
        Assert.AreEqual("native-42",catalog.ParserCopy().Rows[1][2].NativeErrorCode);
        var draft=new SpecificationDraft{Family=SpecificationFamily.Categorical,Header=SpreadsheetHeaderMode.None,Category=2};
        var preview=SpecificationCompilation.Compile(catalog,draft);var issue=preview.Issues.First(i=>i.Code=="cell.excel-error");
        Assert.AreEqual(3,issue.WorksheetColumn);Assert.AreEqual(2,issue.WorksheetRow);
    }

    [TestMethod]
    public void HeaderOnlySelectionStillFailsCommonObservationChecks()
    {
        var data=Data(["Response","Dose"]);var catalog=Catalog(data);Assert.AreEqual(2,catalog.Columns.Count);
        var draft=new SpecificationDraft();draft.SetPredictors([1]);var preview=SpecificationCompilation.Compile(catalog,draft);
        Assert.IsFalse(preview.Valid);Assert.IsTrue(preview.Issues.Any(i=>i.Code=="frame.no-data"));
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public void SparseRepeatedAndMixedCoordinatesReferToPhysicalCells(bool mixed)
    {
        var original=AtRowOne(SpecificationBuilderTests.Repeated(),1);
        var data=Data(original.Rows.Select(r=>new object?[]{r[0],null,r[1],null,r[2],null,r[3],null,r[4]}).ToArray());
        var draft=new SpecificationDraft{Family=mixed?SpecificationFamily.Mixed:SpecificationFamily.Repeated,Subject=0,Visit=1,Response=2,Group=3,AscendingVisits=true};
        draft.SetFixedEffects([4]);draft.SetRandomGroups([0]);
        var preview=SpecificationCompilation.Compile(Catalog(data),draft);Assert.IsTrue(preview.Valid);Assert.AreEqual(6,preview.Observations);
        var observation=mixed?((SpreadsheetMixedModelInput)preview.Specification!).Rows[0].Repeated:((SpreadsheetRepeatedSubjectInput)preview.Specification!).Observations[0];
        Assert.AreEqual(5,observation.Response.WorksheetColumn);Assert.AreEqual(5,observation.Response.SelectedColumn);Assert.AreEqual(2,observation.WorksheetRow);
        if(mixed)Assert.AreEqual(9,((SpreadsheetMixedModelInput)preview.Specification!).Rows[0].Covariates[0].WorksheetColumn);
        data.Rows[2][2]=SpreadsheetCellValue.NumberCell(1);
        preview=SpecificationCompilation.Compile(Catalog(data),draft);var issue=preview.Issues.First(i=>i.Code=="repeat.duplicate-visit");
        Assert.AreEqual(3,issue.WorksheetColumn);Assert.AreEqual(3,issue.SelectedColumn);Assert.AreEqual(3,issue.WorksheetRow);
    }

    [TestMethod]
    public void ApostropheAndFormulaLookingHeadersAreEscapedWithoutUsingDisplayCaptionAsToken()
    {
        var data=Data(["Response","Dose' + factor(nope)","Treatment"],[1,1,"A"],[2,2,"B"],[3,3,"A"],[4,4,"B"]);
        var catalog=Catalog(data);var draft=ModelDraft();var preview=SpecificationCompilation.Compile(catalog,draft);
        Assert.IsTrue(preview.Valid);StringAssert.Contains(preview.NormalizedFormula,"'Dose'' + factor(nope)'");Assert.IsFalse(preview.NormalizedFormula.Contains(" | Var"));
        StringAssert.Contains(draft.TermCaption(new(SpecificationTermKind.Main,1),catalog.Columns)," | VarB");
    }

    [TestMethod]
    public async Task SwitchingSheetsResetsRolesTermsReferencesAndUsesSelectedSheetId()
    {
        var first=AtRowOne(SpecificationBuilderTests.Model(),1);var second=Data(["Other","Predictor"],[1,2],[3,4]);second.WorksheetId="sheet-2";second.WorksheetName="Hárok2";
        var workbook=new Workbook(first,second);var host=Host(workbook);await using var state=new SpecificationBuilderState(workbook,host);
        await state.InitializeAsync();state.Draft.Response=0;state.Draft.SetPredictors([1,2]);state.Draft.SetFactors([2]);state.Draft.SetReference(2,1);state.Draft.AddTerm(new(SpecificationTermKind.Main,1));
        await state.PreviewAsync();Assert.AreEqual(SpecificationStatus.Ready,state.Status);
        await state.SelectWorksheetAsync("sheet-2");Assert.AreEqual("Other | VarA",state.Columns[0].DisplayText);
        Assert.IsNull(state.Preview);Assert.AreEqual(-1,state.Draft.Response);Assert.AreEqual(0,state.Draft.Predictors.Count);Assert.AreEqual(0,state.Draft.Terms.Count);Assert.IsNull(state.Draft.Reference(2));
        state.Draft.Response=0;state.Draft.SetPredictors([1]);await state.PreviewAsync();Assert.AreEqual(SpecificationStatus.Ready,state.Status);
        Assert.AreEqual("sheet-2",workbook.References[^1].WorksheetId);Assert.AreEqual(0,workbook.Writes);
    }

    [TestMethod]
    public async Task NewDocumentWithSameSheetIdCannotReuseDraftOrObservations()
    {
        var data=AtRowOne(SpecificationBuilderTests.Model(),1);var workbook=new Workbook(data);var host=Host(workbook);
        await using var state=new SpecificationBuilderState(workbook,host);await state.InitializeAsync();state.Draft.Response=0;state.Draft.SetPredictors([1]);
        await state.PreviewAsync();Assert.IsNotNull(state.Preview);
        host.Catalog.DocumentIdentity="another-document";await state.RefreshAsync();
        Assert.IsNull(state.Preview);Assert.AreEqual(-1,state.Draft.Response);Assert.AreEqual("another-document",state.Columns[0].DocumentIdentity);
    }

    [TestMethod]
    public async Task AutomaticSourceEventRefreshesColumnsRetiresReadyAndIgnoresLateOwnerCallbacks()
    {
        var data=AtRowOne(SpecificationBuilderTests.Model(),1);var workbook=new Workbook(data);var host=Host(workbook);
        var state=new SpecificationBuilderState(workbook,host);await state.InitializeAsync();state.Draft.Response=0;state.Draft.SetPredictors([1]);await state.PreviewAsync();
        data.Rows[0][1]=SpreadsheetCellValue.TextCell("Changed");host.EmitWorksheet();
        await state.RefreshAsync();Assert.IsNull(state.Preview);Assert.AreEqual("Changed | VarB",state.Columns[1].DisplayText);Assert.AreEqual(-1,state.Draft.Response);
        var oldOwner=host.DiscoveryOwner;await state.DisposeAsync();var reads=workbook.Reads;
        host.EmitWorksheet(oldOwner);Assert.AreEqual(reads,workbook.Reads);
    }

    [TestMethod]
    public async Task FastRefreshCancelAndNextExplicitRefreshDoNotPermanentlyLock()
    {
        var data=AtRowOne(SpecificationBuilderTests.Model(),1);var workbook=new Workbook(data);var host=Host(workbook);
        await using var state=new SpecificationBuilderState(workbook,host);host.PendingCatalog=new(TaskCreationOptions.RunContinuationsAsynchronously);
        var first=state.InitializeAsync();state.Cancel();await first;Assert.IsFalse(state.Busy);
        host.PendingCatalog.SetResult(host.Catalog);host.PendingCatalog=null;
        await state.RefreshAsync();Assert.AreEqual(3,state.Columns.Count);Assert.IsFalse(state.Busy);Assert.AreEqual(1,workbook.Reads);
    }

    [DataTestMethod]
    [DataRow("office.unsupported-api")] [DataRow("user.document-changed")] [DataRow("input.range-cells")] [DataRow("spec.column-limit")]
    public async Task BoundsFailureNeverReadsOrWritesAndExplicitRefreshCanRecover(string code)
    {
        var data=AtRowOne(SpecificationBuilderTests.Model(),1);var workbook=new Workbook(data);var host=Host(workbook);var normal=host.BoundsReply;
        host.BoundsReply=_=>new(){Code=code};await using var state=new SpecificationBuilderState(workbook,host);
        await state.InitializeAsync();Assert.AreEqual(code,state.Code);Assert.AreEqual(0,workbook.Reads);Assert.AreEqual(0,workbook.Writes);
        host.BoundsReply=normal;await state.RefreshAsync();Assert.AreEqual(3,state.Columns.Count);
    }

    [TestMethod]
    public async Task EmptyWorksheetHasNoVariablesAndRemainsRefreshable()
    {
        var data=Data(new object?[]{null});var workbook=new Workbook(data);var host=Host(workbook);host.BoundsReply=_=>new(){Code="workbook.ok",DocumentIdentity="document-1",Empty=true};
        await using var state=new SpecificationBuilderState(workbook,host);await state.InitializeAsync();
        Assert.AreEqual("spec.empty-sheet",state.Code);Assert.AreEqual(0,state.Columns.Count);Assert.AreEqual(0,workbook.Reads);Assert.IsFalse(state.CanPreview);
    }

    [DataTestMethod]
    [DataRow("identity")] [DataRow("duplicate")] [DataRow("visibility")] [DataRow("count")]
    public void MalformedWorksheetCatalogsFailClosed(string defect)
    {
        var reply=new WorksheetCatalogReply{Code="workbook.ok",DocumentIdentity="document",ActiveWorksheetId="id",Worksheets=[new("id","Hárok 1","Visible")]};
        switch(defect){case "identity":reply.DocumentIdentity="";break;case "duplicate":reply.Worksheets.Add(new("id","Other","Visible"));break;case "visibility":reply.Worksheets=[new("id","Data","Unknown")];break;case "count":reply.Worksheets=Enumerable.Range(0,257).Select(i=>new SourceWorksheet("id"+i,"Sheet"+i,"Visible")).ToList();break;}
        Assert.ThrowsException<InvalidDataException>(()=>WorksheetDiscoveryValidation.Catalog(reply));
    }

    [TestMethod]
    public async Task RapidWorksheetSwitchSkipsSupersededPayloadAndRejectsOldDocumentEvent()
    {
        var one=AtRowOne(SpecificationBuilderTests.Model(),1);var two=AtRowOne(one);two.WorksheetId="two";two.WorksheetName="Hárok2";
        var three=AtRowOne(one);three.WorksheetId="three";three.WorksheetName="Visit's data";
        var workbook=new Workbook(one,two,three);var host=Host(workbook);await using var state=new SpecificationBuilderState(workbook,host);
        await state.InitializeAsync();var reads=workbook.Reads;host.PendingCatalog=new(TaskCreationOptions.RunContinuationsAsynchronously);
        var first=state.SelectWorksheetAsync("two");var last=state.SelectWorksheetAsync("three");host.PendingCatalog.SetResult(host.Catalog);await Task.WhenAll(first,last);host.PendingCatalog=null;
        Assert.AreEqual("three",state.SelectedWorksheetId);Assert.AreEqual(reads+1,workbook.Reads);Assert.AreEqual("three",workbook.References[^1].WorksheetId);
        await state.SelectWorksheetAsync("two","old-document");Assert.AreEqual("three",state.SelectedWorksheetId);Assert.AreEqual("user.document-changed",state.Code);
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task RenameOrDocumentChangeBetweenReadAndFinalBoundsCannotPublishPreview(bool documentChange)
    {
        var data=AtRowOne(SpecificationBuilderTests.Model(),1);var workbook=new Workbook(data);var host=Host(workbook);await using var state=new SpecificationBuilderState(workbook,host);
        await state.InitializeAsync();state.Draft.Response=0;state.Draft.SetPredictors([1]);
        var original=host.BoundsReply!;var calls=0;
        host.BoundsReply=id=> {calls++;var reply=original(id);if(calls==2){if(documentChange)reply.DocumentIdentity="replaced-document";else reply.Reference!.WorksheetName="Renamed";}return reply;};
        await state.PreviewAsync();Assert.IsNull(state.Preview);Assert.AreEqual("user.document-changed",state.Code);Assert.IsFalse(state.Busy);Assert.AreEqual(0,workbook.Writes);
    }

    [TestMethod]
    public void PhysicalSnapshotAndNativeErrorMetadataAreDefensivelyCopied()
    {
        var data=Data(["y",null,"error"],[1,null,SpreadsheetCellValue.ErrorCell("#N/A","native-42")]);var catalog=Catalog(data);
        data.Rows[1][2].NativeErrorCode="changed";var parser=catalog.ParserCopy();Assert.AreEqual("native-42",parser.Rows[1][1].NativeErrorCode);
        parser.Rows[1][1].NativeErrorCode="also-changed";Assert.AreEqual("native-42",catalog.ParserCopy().Rows[1][1].NativeErrorCode);
        Assert.AreEqual("error | VarC",catalog.Columns[1].DisplayText);
    }
}
