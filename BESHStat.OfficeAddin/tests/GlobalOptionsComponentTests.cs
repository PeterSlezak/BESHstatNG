using BESHStat.OfficeAddin.Infrastructure;
using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BESHStat.OfficeAddin.Tests.OfficePreferenceTests;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class GlobalOptionsComponentTests
{
    private sealed class Ready : IOfficeHostReadiness
    {
        public Task<HostStatus> InitializeAsync(CancellationToken cancellationToken = default, bool retry = false) => Task.FromResult(new HostStatus(HostState.Excel, "office.excel-ready"));
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
    private static IServiceCollection Services(Storage storage, Browser? browser = null) => AnalysisComponentTests.Services()
        .AddSingleton<IOfficePreferenceStorage>(storage).AddSingleton<IOfficeWorkspaceBrowser>(browser ?? new())
        .AddSingleton<IOfficeHostReadiness>(new Ready());

    [TestMethod]
    public async Task RealCallbacksSaveDiscardDefaultsAndReopenUseSameEffectiveOwner()
    {
        var storage = new Storage();
        await using var services = Services(storage).BuildServiceProvider();
        await using (var ui = new AnalysisComponentTests.EventRenderer(services)) {
            await ui.Mount<App>(); await ui.Event("id", "options-tab", "onclick", new MouseEventArgs());
            foreach (var id in new[] { "global-alpha", "global-seed", "global-trace", "global-digits", "global-mode", "global-upper" }) Assert.IsTrue(ui.Labelled(id), id);
            await ui.Event("id", "global-alpha", "oninput", new ChangeEventArgs { Value = "0,01" });
            Assert.AreEqual("true", ui.Attribute("id", "global-alpha", "aria-invalid"));
            Assert.AreEqual(true, ui.Attribute("id", "global-save", "disabled"));
            await ui.Event("id", "global-discard", "onclick", new MouseEventArgs());
            Assert.AreEqual("0.05", ui.Attribute("id", "global-alpha", "value"));
            await ui.Event("id", "global-alpha", "oninput", new ChangeEventArgs { Value = "0.01" });
            await ui.Event("id", "global-seed", "oninput", new ChangeEventArgs { Value = "12345" });
            await ui.Event("id", "global-digits", "oninput", new ChangeEventArgs { Value = "4" });
            await ui.Event("id", "global-trace", "onchange", new ChangeEventArgs { Value = false });
            await ui.Change("global-mode", "1"); await ui.Event("id", "global-upper", "onchange", new ChangeEventArgs { Value = false });
            Assert.AreEqual(0, storage.Saves);
            await ui.Event("id", "global-save", "onclick", new MouseEventArgs());
            Assert.AreEqual(1, storage.Saves); Assert.AreEqual(1L, services.GetRequiredService<OfficePreferenceSession>().Revision);
            StringAssert.Contains((string)ui.Attribute("id", "preference-state", "text")!, "Saved persistently");
            await ui.Event("id", "global-defaults", "onclick", new MouseEventArgs());
            Assert.AreEqual("0.05", ui.Attribute("id", "global-alpha", "value")); Assert.AreEqual(.01, services.GetRequiredService<OfficePreferenceSession>().Effective.DefaultAlpha);
            await ui.Event("id", "global-discard", "onclick", new MouseEventArgs()); Assert.AreEqual("0.01", ui.Attribute("id", "global-alpha", "value"));
        }
        await using var reopened = Services(storage).BuildServiceProvider();
        await using var pane = new AnalysisComponentTests.EventRenderer(reopened); await pane.Mount<App>();
        Assert.AreEqual("0.01", pane.Attribute("id", "global-alpha", "value")); Assert.AreEqual("12345", pane.Attribute("id", "global-seed", "value"));
        Assert.AreEqual("4", pane.Attribute("id", "global-digits", "value")); Assert.AreEqual("1", pane.Attribute("id", "global-mode", "value"));
        Assert.AreNotEqual(true, pane.Attribute("id", "global-trace", "checked")); Assert.AreNotEqual(true, pane.Attribute("id", "global-upper", "checked"));
    }
    [TestMethod]
    public async Task FailureRequiresSeparateSessionOnlyActionAndPreservesAnalysisOverride()
    {
        var storage = new Storage { SaveStatus = "unavailable" };
        await using var services = Services(storage).BuildServiceProvider();
        await using var ui = new AnalysisComponentTests.EventRenderer(services); await ui.Mount<App>(); await ui.Choose(AnalysisIds.OneWayAnova);
        await ui.Event("id", "analysis-alpha", "oninput", new ChangeEventArgs { Value = "0.125" });
        await ui.Event("id", "global-alpha", "oninput", new ChangeEventArgs { Value = "0.01" });
        await ui.Event("id", "global-save", "onclick", new MouseEventArgs());
        Assert.AreEqual(.05, services.GetRequiredService<OfficePreferenceSession>().Effective.DefaultAlpha);
        Assert.IsTrue(ui.Has("id", "global-session")); Assert.AreEqual("0.01", ui.Attribute("id", "global-alpha", "value"));
        await ui.Event("id", "global-session", "onclick", new MouseEventArgs());
        Assert.AreEqual(.01, services.GetRequiredService<OfficePreferenceSession>().Effective.DefaultAlpha);
        Assert.AreEqual("0.125", ui.Attribute("id", "analysis-alpha", "value"));
        await ui.Choose(AnalysisIds.KruskalWallis); Assert.AreEqual("0.01", ui.Attribute("id", "analysis-alpha", "value"));
    }
    [TestMethod]
    public async Task CompletedPreviewAndOccupiedConsentRetireAndFreshRunKeepsInputAndMethod()
    {
        using var fixture = AnalysisPilotTests.Fixture("anova-base");
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(fixture.RootElement)); var host = new AnalysisPilotTests.Host { Occupancy = OutputOccupancy.Occupied };
        await using var services = Services(new()).AddSingleton<IWorkbookAdapter>(workbook).AddSingleton<IPilotHost>(host).BuildServiceProvider();
        await using var ui = new AnalysisComponentTests.EventRenderer(services); await ui.Mount<App>(); await ui.Choose(AnalysisIds.OneWayAnova);
        await ui.Event("id", "input-reference", "oninput", new ChangeEventArgs { Value = "'Input'!A1:C4" });
        await ui.ClickText("Run analysis"); await ui.ClickText("Write results"); Assert.IsTrue(ui.Has("text", "Yes, overwrite inspected output"));
        await ui.Event("id", "global-alpha", "oninput", new ChangeEventArgs { Value = "0.01" }); await ui.Event("id", "global-save", "onclick", new MouseEventArgs());
        Assert.IsFalse(ui.Has("text", "Yes, overwrite inspected output")); Assert.IsFalse(ui.Has("id", "output-start"));
        Assert.AreEqual("'Input'!A1:C4", ui.Attribute("id", "input-reference", "value")); Assert.AreEqual("0.01", ui.Attribute("id", "analysis-alpha", "value"));
        await ui.ClickText("Run analysis"); await ui.ClickText("Write results"); Assert.IsTrue(ui.Has("text", "Yes, overwrite inspected output"));
        Assert.AreEqual(2, workbook.Reads); Assert.AreEqual(0, workbook.Writes); Assert.AreEqual(2, host.Inspections);
    }
    [TestMethod]
    public async Task ThreeTabsRoveWrapAndRetainDraftsWithScreenReaderLinks()
    {
        await using var services = Services(new()).BuildServiceProvider(); await using var ui = new AnalysisComponentTests.EventRenderer(services); await ui.Mount<App>();
        Assert.AreEqual(3, ui.Count("role", "tab")); Assert.AreEqual("true", ui.Attribute("id", "analysis-tab", "aria-selected"));
        foreach (var tuple in new[] { ("analysis-tab", "ArrowRight", "options-tab"), ("options-tab", "ArrowRight", "diagnostics-tab"), ("diagnostics-tab", "ArrowRight", "analysis-tab"), ("analysis-tab", "ArrowLeft", "diagnostics-tab"), ("diagnostics-tab", "Home", "analysis-tab"), ("analysis-tab", "End", "diagnostics-tab") }) {
            await ui.Event("id", tuple.Item1, "onkeydown", new KeyboardEventArgs { Key = tuple.Item2 });
            Assert.AreEqual("true", ui.Attribute("id", tuple.Item3, "aria-selected")); Assert.AreEqual("0", ui.Attribute("id", tuple.Item3, "tabindex"));
        }
        Assert.AreEqual("options-panel", ui.Attribute("id", "options-tab", "aria-controls"));
        Assert.AreEqual("options-tab", ui.Attribute("id", "options-panel", "aria-labelledby"));
        Assert.IsFalse(ui.Has("text", "Read selected range (read-only)"));
    }
    [TestMethod]
    public async Task HelpAndLogCallbacksAreLocalAndSlovakResourcesRender()
    {
        var browser = new Browser("sk-SK") { Result = "blocked" };
        await using var services = Services(new Storage { Saved = Snapshot().ToData(), SavedForceEnglish = false }, browser).BuildServiceProvider(); await using var ui = new AnalysisComponentTests.EventRenderer(services); await ui.Mount<App>();
        Assert.IsTrue(ui.Has("text", "Možnosti")); await ui.ClickText("Pomocník ku globálnym nastaveniam");
        Assert.IsNotNull(browser.Url); StringAssert.Contains(browser.Url, "route=GlobalSettings");
        Assert.IsTrue(ui.Has("text", "Pomocníka sa nepodarilo otvoriť. Použite odkaz na dokumentáciu."));
        await ui.Choose(AnalysisIds.OneWayAnova); await ui.ClickText("Pomocník k vybranej metóde");
        StringAssert.Contains(browser.Url!, "route=" + HelpDestination.ForAnalysisId(AnalysisIds.OneWayAnova).Metadata.Key);
        await ui.Choose(AnalysisIds.KruskalWallis); await ui.ClickText("Pomocník k vybranej metóde");
        StringAssert.Contains(browser.Url!, "route=" + HelpDestination.ForAnalysisId(AnalysisIds.KruskalWallis).Metadata.Key);
        await ui.ClickText("Kopírovať anonymizovaný záznam"); Assert.IsNotNull(browser.Copied); Assert.IsFalse(browser.Copied.Contains("Input"));
    }

    [TestMethod]
    public async Task ForceEnglishDraftSaveDiscardDefaultsAndRenderedMessagesFollowEffectiveChoice()
    {
        var storage = new Storage(); var browser = new Browser("sk-SK") { Result = "blocked" };
        await using var services = Services(storage, browser).BuildServiceProvider();
        await using var ui = new AnalysisComponentTests.EventRenderer(services); await ui.Mount<App>();
        var owner = services.GetRequiredService<OfficePreferenceSession>();
        Assert.IsTrue(ui.Labelled("global-force-english")); Assert.AreEqual(true, ui.Attribute("id", "global-force-english", "checked"));
        Assert.IsTrue(ui.Has("text", "Options"));
        await ui.Event("id", "options-tab", "onclick", new MouseEventArgs());
        await ui.ClickText("Global settings Help"); await ui.ClickText("Copy sanitized log");
        await ui.Event("id", "global-force-english", "onchange", new ChangeEventArgs { Value = false });
        Assert.IsTrue(owner.ForceEnglish); Assert.IsTrue(ui.Has("text", "Options")); Assert.AreEqual(0, storage.Saves);
        await ui.Event("id", "global-discard", "onclick", new MouseEventArgs()); Assert.AreEqual(true, ui.Attribute("id", "global-force-english", "checked"));
        await ui.Event("id", "global-force-english", "onchange", new ChangeEventArgs { Value = false });
        await ui.Event("id", "global-save", "onclick", new MouseEventArgs());
        Assert.IsFalse(owner.ForceEnglish); Assert.IsTrue(ui.Has("text", "Možnosti")); Assert.IsTrue(ui.Has("text", "Analýza"));
        Assert.IsTrue(ui.Has("text", "Diagnostika / Záznam")); Assert.IsTrue(ui.Has("text", "Uložiť")); Assert.IsTrue(ui.Has("text", "Záznam bol skopírovaný."));
        Assert.IsTrue(ui.Has("text", "Pomocníka sa nepodarilo otvoriť. Použite odkaz na dokumentáciu."));
        Assert.AreEqual("true", ui.Attribute("id", "options-tab", "aria-selected"));
        Assert.AreEqual("Slovenčina", ui.Attribute("id", "effective-interface-language", "text"));
        await ui.Event("id", "global-defaults", "onclick", new MouseEventArgs());
        Assert.AreEqual(true, ui.Attribute("id", "global-force-english", "checked")); Assert.IsFalse(owner.ForceEnglish);
        await ui.Event("id", "global-discard", "onclick", new MouseEventArgs()); Assert.AreNotEqual(true, ui.Attribute("id", "global-force-english", "checked"));
        await ui.Event("id", "global-force-english", "onchange", new ChangeEventArgs { Value = true });
        await ui.Event("id", "global-save", "onclick", new MouseEventArgs());
        Assert.IsTrue(owner.ForceEnglish); Assert.IsTrue(ui.Has("text", "Options")); Assert.IsTrue(ui.Has("text", "Log copied.")); Assert.AreEqual(true, storage.SavedForceEnglish);
    }

    [TestMethod]
    public async Task ChildLogAndHelpUpdateWithoutAnAppParentRerenderAndUnsubscribeOnDisposal()
    {
        await using var services = Services(new(), new Browser("sk-SK") { Result = "blocked" }).BuildServiceProvider();
        var text = services.GetRequiredService<OfficeStrings>(); var owner = services.GetRequiredService<OfficePreferenceSession>(); await text.InitializeAsync();
        await using (var log = new AnalysisComponentTests.EventRenderer(services))
        await using (var help = new AnalysisComponentTests.EventRenderer(services)) {
            await log.Mount<BESHStat.OfficeAddin.Components.DiagnosticLog>(); await help.Mount<BESHStat.OfficeAddin.Components.HelpAction>();
            await help.ClickText("Help"); await log.ClickText("Copy sanitized log");
            await log.Dispatcher.InvokeAsync(() => owner.SaveAsync(Snapshot(), sessionOnly: true, forceEnglish: false));
            await help.Dispatcher.InvokeAsync(() => Task.CompletedTask);
            Assert.IsTrue(log.Has("text", "Diagnostický záznam relácie")); Assert.IsTrue(log.Has("text", "Záznam bol skopírovaný."));
            Assert.IsTrue(help.Has("text", "Pomocník")); Assert.IsTrue(help.Has("text", "Pomocníka sa nepodarilo otvoriť. Použite odkaz na dokumentáciu."));
        }
        Assert.IsTrue(await owner.SaveAsync(Snapshot(), true, true)); Assert.AreEqual("Options", text["Options"]);
    }

    [TestMethod]
    public async Task ReopenedPaneUsesSavedFalseAndSessionLanguageSaveLeavesPersistentEnglish()
    {
        var storage = new Storage { Saved = Snapshot().ToData(), SavedForceEnglish = false };
        await using (var services = Services(storage, new Browser("sk-SK")).BuildServiceProvider())
        await using (var ui = new AnalysisComponentTests.EventRenderer(services)) {
            await ui.Mount<App>(); Assert.IsTrue(ui.Has("text", "Možnosti"));
            await ui.Event("id", "global-force-english", "onchange", new ChangeEventArgs { Value = true });
            await ui.Event("id", "global-save", "onclick", new MouseEventArgs()); Assert.IsTrue(ui.Has("text", "Options"));
        }
        storage.SaveStatus = "unavailable";
        await using (var services = Services(storage, new Browser("sk-SK")).BuildServiceProvider())
        await using (var ui = new AnalysisComponentTests.EventRenderer(services)) {
            await ui.Mount<App>(); Assert.IsTrue(ui.Has("text", "Options"));
            await ui.Event("id", "global-force-english", "onchange", new ChangeEventArgs { Value = false });
            await ui.Event("id", "global-save", "onclick", new MouseEventArgs()); Assert.IsTrue(ui.Has("text", "Options"));
            await ui.Event("id", "global-session", "onclick", new MouseEventArgs()); Assert.IsTrue(ui.Has("text", "Možnosti")); Assert.AreEqual(true, storage.SavedForceEnglish);
        }
        await using var reopened = Services(storage, new Browser("sk-SK")).BuildServiceProvider();
        await using var pane = new AnalysisComponentTests.EventRenderer(reopened); await pane.Mount<App>(); Assert.IsTrue(pane.Has("text", "Options"));
    }
}
