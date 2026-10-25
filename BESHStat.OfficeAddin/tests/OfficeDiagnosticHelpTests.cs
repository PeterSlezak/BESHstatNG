using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BESHStat.OfficeAddin.Tests.OfficePreferenceTests;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class OfficeDiagnosticHelpTests
{
    [TestMethod]
    public async Task BoundedSessionLogIsRedactedAndObserverExceptionsAreIsolated()
    {
        using var log = new OfficeDiagnosticLog(); log.Changed += () => throw new Exception("PRIVATE");
        var emitter = new DiagnosticEmitter(log, () => new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.Zero));
        var run = await emitter.StartAsync("PRIVATE sheet name", true);
        for (int i = 0; i < 1000; i++) await run.ReportAsync("PRIVATE A1:C4", "PRIVATE header", new Dictionary<string, string> {
            ["host"] = "office", ["sheet"] = "PRIVATE", ["url"] = "https://secret", ["category"] = "PRIVATE", ["outcome"] = "PRIVATE" });
        Assert.IsTrue(log.Snapshot().Count <= OfficeDiagnosticLog.MaximumEvents);
        Assert.IsTrue(System.Text.Encoding.UTF8.GetByteCount(log.Export()) <= OfficeDiagnosticLog.MaximumBytes);
        var export = log.Export(); Assert.IsFalse(export.Contains("PRIVATE")); Assert.IsFalse(export.Contains("A1:C4")); Assert.IsFalse(export.Contains("https://"));
        Assert.IsTrue(log.Snapshot().All(r => r.Operation == "redacted" && r.Stage == "redacted" && r.RunId == run.RunId));
        Assert.IsTrue(log.Export(true).Length > 0);
        log.Dispose(); await run.ReportAsync("diagnostic.run.failed", "analysis"); Assert.AreEqual(0, log.Snapshot().Count);
    }
    [TestMethod]
    public async Task TraceOffSuppressesDebugButRetainsSafetyWarningAndEndedRunRejectsLateEvents()
    {
        using var log = new OfficeDiagnosticLog(); var emitter = new DiagnosticEmitter(log);
        var run = await emitter.StartAsync(AnalysisIds.OneWayAnova, false);
        Assert.AreEqual(DiagnosticDelivery.Suppressed, await run.ReportAsync("diagnostic.stage.entered", "analysis"));
        await run.ReportAsync("output.sheet-name-adjusted", "output"); await run.ReportAsync("diagnostic.output.incomplete", "output");
        await run.EndAsync(); var count = log.Snapshot().Count;
        Assert.AreEqual(DiagnosticDelivery.Suppressed, await run.ReportAsync("diagnostic.output.completed", "output")); Assert.AreEqual(count, log.Snapshot().Count);
        Assert.AreEqual(2, log.Snapshot().Count(r => r.Severity == DiagnosticSeverity.Warning));
        Assert.AreEqual(0, log.Snapshot().Count(r => r.Severity == DiagnosticSeverity.Debug));
        var second = await emitter.StartAsync(AnalysisIds.KruskalWallis, true); Assert.AreNotEqual(run.RunId, second.RunId);
        // The optional Validate button also records failures before the first Run.
        using var fixture = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        using var validationLog = new OfficeDiagnosticLog();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft,
            new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(fixture.RootElement)), new AnalysisPilotTests.Host(), diagnostics: new DiagnosticEmitter(validationLog));
        await state.ValidateAsync();
        Assert.IsTrue(validationLog.Snapshot().Any(r => r.EventCode == "diagnostic.validation.rejected"));
        Assert.IsFalse(validationLog.Snapshot().Any(r => r.EventCode == "diagnostic.analysis.completed"));
    }
    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public async Task RealCollisionWarningAndWriteOutcomeAreStructuredWithoutNames(bool uncertain)
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new()); using var log = new OfficeDiagnosticLog();
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement)) { Uncertain = uncertain };
        var host = new AnalysisPilotTests.Host { NameReply = new() { Code = "workbook.ok", RequestedName = "PRIVATE", ResolvedName = "PRIVATE2", WarningCode = "output.sheet-name-adjusted" } };
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, host, diagnostics: new DiagnosticEmitter(log));
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync(); state.SetDestination(SpreadsheetOutputDestination.NewWorksheet, "J1", "PRIVATE");
        await state.WriteResultsAsync();
        Assert.IsTrue(log.Snapshot().Any(r => r.EventCode == "output.sheet-name-adjusted"));
        Assert.IsTrue(log.Snapshot().Any(r => r.EventCode == (uncertain ? "diagnostic.output.incomplete" : "diagnostic.output.completed")));
        Assert.AreEqual(!uncertain, log.Snapshot().Any(r => r.EventCode == "diagnostic.output.completed"));
        Assert.IsTrue(log.Snapshot().All(r => r.RunId == state.RunId));
        Assert.IsFalse(log.Export().Contains("PRIVATE")); Assert.IsFalse(log.Export().Contains("Input")); Assert.IsFalse(log.Export().Contains("J1"));
    }
    private sealed class ThrowingSink : IDiagnosticSink { public Task EmitAsync(DiagnosticRecord record) => throw new Exception("PRIVATE"); }
    [TestMethod]
    public async Task ThrowingSinkCannotMaskRealAnalysisOrWriteOutcome()
    {
        using var f = AnalysisPilotTests.Fixture("anova-base"); using var draft = new AnovaOptionDraft(new());
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(f.RootElement)) { Uncertain = true };
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(AnalysisIds.OneWayAnova), draft, workbook, new AnalysisPilotTests.Host(), diagnostics: new DiagnosticEmitter(new ThrowingSink()));
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync(); Assert.AreEqual("analysis.completed", state.Code);
        await state.WriteResultsAsync(); Assert.AreEqual(WriteCodes.MutationUncertain, state.Code); Assert.IsTrue(state.RequiresRecovery);
    }
    [TestMethod]
    public async Task HelpRoutesUseRealBuildVersionCanonicalMetadataAndPublishedResolver()
    {
        var browser = new Browser(); var help = new OfficeHelp(browser);
        Assert.IsTrue(help.Version.IsAvailable); Assert.AreEqual(typeof(OfficeHelp).Assembly.GetName().Version!.ToString(4), help.Version.ProductVersion);
        foreach (var target in new[] { help.Home, help.Settings, help.Method(AnalysisIds.OneWayAnova), help.Method(AnalysisIds.KruskalWallis) }) {
            Assert.AreEqual(HelpLaunchTarget.ResolverPublished, target.UsesResolver); Assert.AreEqual(target.Url, HelpLaunchTarget.SafeOpenUrl(target.Url));
            Assert.AreEqual("requested", await help.OpenAsync(target)); Assert.AreEqual(target.Url, browser.Url);
        }
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, help.Method("PRIVATE https://evil").Url);
        var fallback = HelpLaunchTarget.Create(HelpDestination.ForPageKey("GlobalSettings"), help.Version, false);
        Assert.IsFalse(fallback.UsesResolver); Assert.AreEqual(fallback.LatestUrl, fallback.Url);
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLaunchTarget.SafeOpenUrl("https://evil.example/"));
    }
}
