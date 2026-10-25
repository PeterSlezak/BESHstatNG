using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using System.Xml.Linq;
using static BESHStat.OfficeAddin.Tests.OfficePreferenceTests;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class OfficeCommandTests
{
    internal sealed class Bridge : IOfficeCommandBridge
    {
        public Func<string,Task<string>>? Dispatch;
        public OfficeCommandRuntime Runtime = new(true,true,true,true);
        public Task<OfficeCommandRuntime> BindAsync(Func<string,Task<string>> dispatch) { Dispatch=dispatch;return Task.FromResult(Runtime); }
        public Task ReadyAsync()=>Task.CompletedTask;
        public Task ShowAboutAsync()=>Task.CompletedTask;
        public ValueTask DisposeAsync(){Dispatch=null;return ValueTask.CompletedTask;}
    }
    private sealed class Ready : BESHStat.OfficeAddin.Infrastructure.IOfficeHostReadiness
    {
        public Task<BESHStat.OfficeAddin.Infrastructure.HostStatus> InitializeAsync(CancellationToken cancellationToken=default,bool retry=false) => Task.FromResult(new BESHStat.OfficeAddin.Infrastructure.HostStatus(BESHStat.OfficeAddin.Infrastructure.HostState.Excel,"office.excel-ready","OfficeOnline"));
        public ValueTask DisposeAsync()=>ValueTask.CompletedTask;
    }
    [DataTestMethod]
    [DataRow("sk-SK",true,"Export Chart","About BESHStatNG","Chart export is not yet implemented in this Office add-in.")]
    [DataRow("sk-SK",false,"Export grafu","O BESHStatNG","Export grafov v tomto doplnku Office zatiaľ nie je implementovaný.")]
    [DataRow("de-DE",false,"Export Chart","About BESHStatNG","Chart export is not yet implemented in this Office add-in.")]
    [DataRow("en-US",true,"Export Chart","About BESHStatNG","Chart export is not yet implemented in this Office add-in.")]
    public async Task HeaderAndExportPageUseLocalizedAccessibleLinksWithoutRedundantControls(string locale,bool forceEnglish,string exportLabel,string aboutLabel,string placeholder)
    {
        var browser=new Browser(locale);var storage=new Storage{Saved=PreferenceSnapshot.Defaults().ToData(),SavedForceEnglish=forceEnglish};
        var bridge=new Bridge();var workbook=new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(AnalysisPilotTests.Fixture("anova-base").RootElement));
        await using var services=AnalysisComponentTests.Services().AddSingleton<IOfficeCommandBridge>(bridge).AddSingleton<IOfficeWorkspaceBrowser>(browser)
            .AddSingleton<IOfficePreferenceStorage>(storage).AddSingleton<BESHStat.OfficeAddin.Infrastructure.IOfficeHostReadiness>(new Ready())
            .AddSingleton<IWorkbookAdapter>(workbook).BuildServiceProvider();
        await using var ui=new AnalysisComponentTests.EventRenderer(services);await ui.Mount<App>();
        var help=services.GetRequiredService<OfficeHelp>();
        Assert.AreEqual(4,ui.Count("role","tab"));Assert.AreEqual(exportLabel,ui.Attribute("id","export-tab","text"));
        Assert.AreEqual(aboutLabel,ui.Attribute("id","header-about","text"));Assert.AreEqual("dialog",ui.Attribute("id","header-about","aria-haspopup"));
        Assert.AreEqual(help.Home.Url,ui.Attribute("id","header-help","href"));Assert.AreEqual(help.Page("ExportChart").Url,ui.Attribute("id","export-help","href"));
        foreach(var id in new[]{"workspace-commands","command-analysis","command-options","command-log","command-about","command-help","command-export"})Assert.IsFalse(ui.Has("id",id));
        await ui.Event("id","export-tab","onclick",new MouseEventArgs());Assert.AreEqual("true",ui.Attribute("id","export-tab","aria-selected"));
        Assert.AreNotEqual(true,ui.Attribute("id","export-panel","hidden"));Assert.AreEqual(placeholder,ui.Attribute("id","export-placeholder","text"));
        await ui.Event("id","export-help","onclick",new MouseEventArgs());Assert.AreEqual(help.Page("ExportChart").Url,browser.Url);
        await ui.Event("id","header-help","onclick",new MouseEventArgs());Assert.AreEqual(help.Home.Url,browser.Url);
        await ui.Event("id","header-about","onclick",new MouseEventArgs());Assert.IsTrue(ui.Has("id","about-panel"));
        Assert.IsTrue(ui.Has("text",help.Version.DisplayLabel));StringAssert.Contains(ui.StaticMarkup,"BESHStatNG · ");Assert.IsTrue(ui.Has("text",services.GetRequiredService<OfficeStrings>()["OfficeWorkspace"]));
        StringAssert.Contains((string)ui.Attribute("id","about-license","text")!,"Apache License 2.0");
        await ui.Event("id","about-close","onclick",new MouseEventArgs());Assert.IsFalse(ui.Has("id","about-panel"));
        Assert.AreEqual("true",ui.Attribute("id","export-tab","aria-selected"));
        Assert.AreEqual("command.unavailable",await services.GetRequiredService<OfficeCommandCoordinator>().DispatchAsync("export",true));
        Assert.AreEqual("true",ui.Attribute("id","export-tab","aria-selected"));
        Assert.AreEqual(0,workbook.Reads);Assert.AreEqual(0,workbook.Writes);Assert.AreEqual(0,storage.Saves);
        browser.Result="blocked";await ui.Event("id","export-help","onclick",new MouseEventArgs());
        Assert.IsTrue(ui.Has("text",services.GetRequiredService<OfficeStrings>()["HelpBlocked"]));
    }
    [TestMethod]
    public async Task LedgerAndRegistryRetainAllOccurrencesButOnlyActualEditorsExecute()
    {
        await using var services=AnalysisComponentTests.Services().BuildServiceProvider();var registry=services.GetRequiredService<OfficeCommandRegistry>();
        Assert.AreEqual(GeneratedMethodMetadata.All.Count+6,registry.All.Count);
        Assert.AreEqual(84,GeneratedRibbonCommands.All.Count);Assert.AreEqual(12,GeneratedRibbonCommands.All.Where(r=>r.CommandId.StartsWith("method.")).Select(r=>r.Category).Distinct().Count());
        var editors=registry.All.Where(c=>c.Capability==OfficeCommandCapability.MethodEditor).Select(c=>c.AnalysisId).Order().ToArray();
        CollectionAssert.AreEqual(new[]{AnalysisIds.OneWayAnova,AnalysisIds.KruskalWallis}.Order().ToArray(),editors);
        foreach(var m in GeneratedMethodMetadata.All){Assert.IsTrue(registry.TryGet("method."+m.Key,out var command));Assert.IsTrue(command!.Sources.Count>0);}
        foreach(var group in GeneratedRibbonCommands.All.GroupBy(s=>s.WindowsId))Assert.AreEqual(1,group.Count());
    }
    [DataTestMethod]
    [DataRow("export")] [DataRow("method.LinearRegression")] [DataRow("https://evil.example/")] [DataRow("OPTIONS")]
    public async Task UnsupportedForgedAndUnavailableCommandsCannotNavigate(string id)
    {
        await using var services=AnalysisComponentTests.Services().BuildServiceProvider();var commands=services.GetRequiredService<OfficeCommandCoordinator>();commands.Runtime=new(true,true,true,true);
        var calls=0;using var owner=commands.Participate(()=>true,c=>{calls++;return Task.FromResult(true);});
        Assert.AreNotEqual("command.navigated",await commands.DispatchAsync(id,true));Assert.AreEqual(0,calls);
    }
    [TestMethod]
    public async Task BusyRapidDisposedAndUnsupportedHostRejectionsNeverReplay()
    {
        await using var services=AnalysisComponentTests.Services().BuildServiceProvider();var commands=services.GetRequiredService<OfficeCommandCoordinator>();
        var ready=false;var calls=0;var release=new TaskCompletionSource<bool>();
        using var owner=commands.Participate(()=>ready,c=>{calls++;return release.Task;});
        Assert.AreEqual("command.busy",await commands.DispatchAsync("options"));ready=true;
        Assert.AreEqual("command.host-unsupported",await commands.DispatchAsync("options",true));
        var first=commands.DispatchAsync("options");Assert.AreEqual("command.busy",await commands.DispatchAsync("about"));release.SetResult(true);Assert.AreEqual("command.navigated",await first);Assert.AreEqual(1,calls);
        commands.Dispose();Assert.AreEqual("command.disposed",await commands.DispatchAsync("about"));
    }
    [TestMethod]
    public async Task RealAppCallbacksSelectEditorsUtilitiesAboutAndRetainSameMethodDraft()
    {
        var bridge=new Bridge();var browser=new Browser();var workbook=new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(AnalysisPilotTests.Fixture("anova-base").RootElement));
        await using var services=AnalysisComponentTests.Services().AddSingleton<IOfficeCommandBridge>(bridge).AddSingleton<IOfficeWorkspaceBrowser>(browser)
            .AddSingleton<BESHStat.OfficeAddin.Infrastructure.IOfficeHostReadiness>(new Ready()).AddSingleton<IWorkbookAdapter>(workbook).AddSingleton<IPilotHost>(new AnalysisPilotTests.Host()).BuildServiceProvider();
        await using var ui=new AnalysisComponentTests.EventRenderer(services);await ui.Mount<App>();
        async Task<string> Dispatch(string id){var result="";await ui.Dispatcher.InvokeAsync(async()=>result=await bridge.Dispatch!(id));return result;}
        Assert.AreEqual("command.navigated",await Dispatch("method.OneWayANOVA"));Assert.AreEqual(AnalysisIds.OneWayAnova,ui.Attribute("id","analysis-method","value"));
        await ui.Event("id","analysis-alpha","oninput",new ChangeEventArgs{Value="0.125"});await Dispatch("method.OneWayANOVA");Assert.AreEqual("0.125",ui.Attribute("id","analysis-alpha","value"));
        await Dispatch("options");Assert.AreEqual("true",ui.Attribute("id","options-tab","aria-selected"));
        await Dispatch("diagnostics");Assert.AreEqual("true",ui.Attribute("id","diagnostics-tab","aria-selected"));
        await Dispatch("about");Assert.IsTrue(ui.Has("role","dialog"));await ui.Event("id","about-close","onclick",new MouseEventArgs());Assert.IsFalse(ui.Has("role","dialog"));
        await Dispatch("help");Assert.AreEqual(services.GetRequiredService<OfficeHelp>().Home.Url,browser.Url);
        await Dispatch("method.KruskalWallisTest");Assert.AreEqual(AnalysisIds.KruskalWallis,ui.Attribute("id","analysis-method","value"));Assert.AreEqual(0,workbook.Reads);Assert.AreEqual(0,workbook.Writes);
        await ui.ClickText("Retry initialization");Assert.AreEqual(AnalysisIds.KruskalWallis,ui.Attribute("id","analysis-method","value"));
        Assert.IsTrue(services.GetRequiredService<OfficePreferenceSession>().ForceEnglish);
    }
    [TestMethod]
    public void ManifestResourcesDisabledItemsAndLedgerAreExact()
    {
        var root=new DirectoryInfo(AppContext.BaseDirectory);while(root is not null&&!File.Exists(Path.Combine(root.FullName,"BESHStatNG.sln")))root=root.Parent;
        var xml=XDocument.Load(Path.Combine(root!.FullName,"BESHStat.OfficeAddin/manifest-dev.xml"));XNamespace ov="http://schemas.microsoft.com/office/taskpaneappversionoverrides",bt="http://schemas.microsoft.com/office/officeappbasictypes/1.0",xsi="http://www.w3.org/2001/XMLSchema-instance";
        var controls=xml.Descendants().Where(e=>e.Name.LocalName is "Control" or "Item").ToArray();Assert.AreEqual(controls.Length,controls.Select(c=>(string?)c.Attribute("id")).Distinct().Count());
        Assert.AreEqual(12,controls.Count(c=>(string?)c.Attribute(xsi+"type")=="Menu"));Assert.IsFalse(xml.Descendants(ov+"Item").Any(i=>i.Descendants(ov+"Items").Any()));
        Assert.AreEqual(1,xml.Descendants(ov+"Runtime").Count());Assert.AreEqual("long",(string?)xml.Descendants(ov+"Runtime").Single().Attribute("lifetime"));Assert.IsFalse(xml.Descendants(ov+"TaskpaneID").Any());
        foreach(var source in GeneratedRibbonCommands.All){var control=controls.Single(c=>(string?)c.Attribute("id")==source.ManifestId);var enabled=control.Element(ov+"Enabled")!.Value=="true";
            Assert.AreEqual(source.CommandId is "method.OneWayANOVA" or "method.KruskalWallisTest" or "about" or "help" or "options" or "diagnostics",enabled,source.ManifestId);}
        var resources=xml.Descendants().Where(e=>e.Parent?.Name.LocalName is "Images" or "Urls" or "ShortStrings" or "LongStrings").Select(e=>(string?)e.Attribute("id")).ToHashSet();
        foreach(var attr in xml.Descendants().Attributes("resid")){Assert.IsTrue(resources.Contains(attr.Value),attr.Value);Assert.IsTrue(attr.Value.Length<=32);}
        foreach(var str in xml.Descendants(bt+"String"))Assert.IsTrue(((string?)str.Attribute("DefaultValue"))!.Length<=(str.Parent!.Name.LocalName=="ShortStrings"?125:250));
        using var ledger=JsonDocument.Parse(File.ReadAllText(Path.Combine(root.FullName,"docs/architecture/batch-04-07-ribbon-parity.json")));Assert.AreEqual(84,ledger.RootElement.GetProperty("occurrences").GetArrayLength());Assert.AreEqual(0,ledger.RootElement.GetProperty("omissions").GetArrayLength());
    }

    [TestMethod]
    public async Task CommandsRetireMethodConsentRejectSubmittedWritesAndKeepUncertaintyAcrossMethodSwitch()
    {
        using var fixture=AnalysisPilotTests.Fixture("anova-base");var workbook=new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(fixture.RootElement));
        var host=new AnalysisPilotTests.Host{Occupancy=OutputOccupancy.Occupied};var bridge=new Bridge();
        await using var services=AnalysisComponentTests.Services().AddSingleton<IOfficeCommandBridge>(bridge).AddSingleton<IOfficeWorkspaceBrowser>(new Browser())
            .AddSingleton<BESHStat.OfficeAddin.Infrastructure.IOfficeHostReadiness>(new Ready()).AddSingleton<IWorkbookAdapter>(workbook).AddSingleton<IPilotHost>(host).BuildServiceProvider();
        await using var ui=new AnalysisComponentTests.EventRenderer(services);await ui.Mount<App>();
        async Task<string> Dispatch(string id){var code="";await ui.Dispatcher.InvokeAsync(async()=>code=await bridge.Dispatch!(id));return code;}
        await Dispatch("method.OneWayANOVA");await ui.Event("id","input-reference","oninput",new ChangeEventArgs{Value="'Input'!A1:C4"});
        await ui.ClickText("Run analysis");await ui.ClickText("Write results");Assert.IsTrue(ui.Has("text","Yes, overwrite inspected output"));
        await ui.Event("id","export-tab","onclick",new MouseEventArgs());Assert.AreEqual("true",ui.Attribute("id","export-tab","aria-selected"));Assert.IsTrue(ui.Has("text","Yes, overwrite inspected output"));
        await Dispatch("options");Assert.IsTrue(ui.Has("text","Yes, overwrite inspected output"));Assert.AreEqual("'Input'!A1:C4",ui.Attribute("id","input-reference","value"));
        await Dispatch("method.KruskalWallisTest");Assert.IsFalse(ui.Has("text","Yes, overwrite inspected output"));Assert.AreEqual(0,workbook.Writes);
        host.Occupancy=OutputOccupancy.Empty;await ui.Event("id","input-reference","oninput",new ChangeEventArgs{Value="'Input'!A1:C4"});await ui.ClickText("Run analysis");
        workbook.PendingWrite=new();var writing=ui.ClickText("Write results");await ui.Dispatcher.InvokeAsync(()=>Task.CompletedTask);
        Assert.AreEqual("command.busy",await Dispatch("method.OneWayANOVA"));Assert.AreEqual(AnalysisIds.KruskalWallis,ui.Attribute("id","analysis-method","value"));
        workbook.PendingWrite.SetResult(new WriteResult(OutputWriteState.PartiallyWritten,WriteCodes.MutationUncertain,"Unverified",workbook.Intent!,true));await writing;
        Assert.AreEqual("command.navigated",await Dispatch("method.OneWayANOVA"));
        Assert.AreEqual(true,ui.Attribute("text","Run analysis","disabled"));Assert.IsTrue(host.RequiresRecovery);Assert.AreEqual(1,workbook.Writes);
        var log=services.GetRequiredService<OfficeDiagnosticLog>().Export();Assert.IsFalse(log.Contains("Input"));Assert.IsFalse(log.Contains("A1:C4"));
        StringAssert.Contains(log,"diagnostic.command.rejected");Assert.IsFalse(log.Contains("diagnostic.output.completed"));
    }
}
