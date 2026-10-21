using BESHStat.WasmSmoke.Infrastructure;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class HelpMetadataTests
{
    [DataTestMethod]
    [DataRow(SpreadsheetAnalysisKind.OneWayAnova, "one-way-anova/")]
    [DataRow(SpreadsheetAnalysisKind.KruskalWallis, "kruskal-wallis-test/")]
    public void SelectedPilotResolvesCanonicalHelp(SpreadsheetAnalysisKind kind, string slug)
    {
        IAnalysisDescriptor descriptor = null!;
        Assert.IsTrue(AnalysisCatalog.TryGetByLegacyKind(kind, ref descriptor));
        var help = HelpDestination.ForAnalysisId(descriptor.Id);
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl + "methods/" + slug, help.Url);
        Assert.IsFalse(help.IsFallback);
    }

    [TestMethod]
    public async Task HelpLookupCannotApproveConsentOrChangeCompletedWrite()
    {
        using var run = new OfficeExecutionRun();
        var shown = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var writes = 0;
        var task = run.RunAsync(new DiagnosticEmitter(null, null, null), AnalysisIds.OneWayAnova, false, async session =>
        {
            var approved = await session.ConfirmAsync(() => shown.SetResult(true));
            Assert.IsTrue(approved);
            await session.WriteAsync(() => { writes++; return Task.FromResult("Completed"); });
            session.FinishOutput("Completed");
        });
        await shown.Task;
        var before = run.Execution.Snapshot;
        _ = HelpDestination.ForAnalysisId(AnalysisIds.KruskalWallis);
        var unavailable = FeatureAvailabilityQuery.ByMethodKey("Histogram", HostCapabilityProfiles.OfficeJsExcel("Online"), SpreadsheetOutputDestination.NewWorksheet);
        Assert.IsTrue(unavailable.IsDocumented);
        Assert.IsFalse(unavailable.IsRegisteredInCommon);
        Assert.IsFalse(unavailable.IsSupportedOnHost);
        Assert.AreEqual(0, writes);
        Assert.IsTrue(run.AwaitingConsent);
        Assert.AreEqual(before.State, run.Execution.Snapshot.State);
        run.ChooseOverwrite(true);
        await task;
        _ = HelpDestination.ForAnalysisId("javascript:alert(1)");
        run.ChooseOverwrite(true); // Stale choice has no consent to approve.
        Assert.AreEqual(ExecutionState.Completed, run.Execution.Snapshot.State);
        Assert.IsFalse(run.AwaitingConsent);
        Assert.AreEqual(1, writes);
    }

    [TestMethod]
    public async Task HelpLookupPreservesCancellationAndInvalidFallback()
    {
        using var run = new OfficeExecutionRun();
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => run.RunAsync(
            new DiagnosticEmitter(null, null, null), AnalysisIds.OneWayAnova, false, session =>
            {
                session.Cancel();
                var help = HelpDestination.ForAnalysisId(null);
                Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, help.Url);
                Assert.IsTrue(session.Execution.Snapshot.IsCancellationRequested);
                session.Execution.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }));
        Assert.AreEqual(ExecutionState.Cancelled, run.Execution.Snapshot.State);
    }

    [TestMethod]
    public void RazorHelpBindingHasNoExecutionOrConsentEventAndKeepsTaskPane()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "AGENTS.md"))) root = root.Parent;
        Assert.IsNotNull(root);
        var source = File.ReadAllText(Path.Combine(root.FullName, "BESHStat.WasmSmoke/Pages/Home.razor"));
        StringAssert.Contains(source, "href=\"@SelectedAnalysisHelp.GetLaunchTarget(PilotProductVersion).Url\" target=\"_blank\" rel=\"noopener noreferrer\"");
        StringAssert.Contains(source, "AnalysisCatalog.TryGetByLegacyKind(analysisKind, ref descriptor)");
        StringAssert.Contains(source, "HelpDestination.ForAnalysisId(descriptor.Id)");
        var anchor = source.Split("Help for selected analysis</a>")[0].Split("<a ").Last();
        Assert.IsFalse(anchor.Contains("@onclick"));
        StringAssert.Contains(source, "ProductVersionDescriptor.FromVersion(typeof(App).Assembly.GetName().Version)");
        Assert.IsFalse(source.Contains("window.open"));
        Assert.IsFalse(source.Contains("fetch("));
    }

    [DataTestMethod]
    [DataRow(SpreadsheetAnalysisKind.OneWayAnova, "1.2.5.0", "OneWayANOVA", "one-way-anova/")]
    [DataRow(SpreadsheetAnalysisKind.OneWayAnova, "1.2.6.0", "OneWayANOVA", "one-way-anova/")]
    [DataRow(SpreadsheetAnalysisKind.KruskalWallis, "1.2.5.0", "KruskalWallisTest", "kruskal-wallis-test/")]
    [DataRow(SpreadsheetAnalysisKind.KruskalWallis, "1.2.6.0", "KruskalWallisTest", "kruskal-wallis-test/")]
    public void PilotLaunchUsesExactRunningVersionAndSharedActivation(SpreadsheetAnalysisKind kind, string value, string key, string slug)
    {
        IAnalysisDescriptor descriptor = null!;
        Assert.IsTrue(AnalysisCatalog.TryGetByLegacyKind(kind, ref descriptor));
        ProductVersionDescriptor version = null!;
        Assert.IsTrue(ProductVersionDescriptor.TryParse(value, ref version));
        var help = HelpDestination.ForAnalysisId(descriptor.Id);
        var active = help.GetLaunchTarget(version, true);
        Assert.AreEqual(GeneratedMethodMetadata.SiteBaseUrl + "/v" + value + "/methods/" + slug, active.VersionCandidateUrl);
        Assert.AreEqual(HelpLaunchTarget.ResolverUrl + "?version=" + value + "&route=" + key, active.Url);
        Assert.AreEqual(help.Url, help.GetLaunchTarget(version, false).Url);
        Assert.AreEqual(help.Url, help.GetLaunchTarget(ProductVersionDescriptor.FromVersion(null), true).Url);
        Assert.AreEqual(HelpLaunchTarget.ResolverPublished, help.GetLaunchTarget(version).UsesResolver);
    }
}
