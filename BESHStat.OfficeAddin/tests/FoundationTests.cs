using BESHStat.OfficeAddin.Infrastructure;
using BESHStatNG.ExcelCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.Xml.Linq;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class FoundationTests
{
    private static HostReply Excel() => new() { State = "excel", Code = "office.excel-ready",
        Platform = "OfficeOnline", OfficeReady = true, ExcelApiSupported = true };

    [TestMethod]
    public async Task ProductionRegistrationsResolveAndAreScopedAndIdempotent()
    {
        var services = new ServiceCollection().AddSingleton<IJSRuntime>(new FakeRuntime());
        services.AddOfficeFoundation().AddOfficeFoundation();
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IOfficeHostReadiness)));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using var first = provider.CreateAsyncScope();
        await using var second = provider.CreateAsyncScope();
        var host = first.ServiceProvider.GetRequiredService<IOfficeHostReadiness>();
        Assert.AreSame(host, first.ServiceProvider.GetRequiredService<IOfficeHostReadiness>());
        Assert.AreNotSame(host, second.ServiceProvider.GetRequiredService<IOfficeHostReadiness>());
        Assert.IsTrue(first.ServiceProvider.GetRequiredService<IFoundationProbe>().Run().CommonPassed);
    }

    [TestMethod]
    public void ProbeExecutesRealCoreAndChecksCompleteCommonRegistration()
    {
        var result = new FoundationProbe().Run();
        Assert.IsTrue(result.CorePassed);
        Assert.AreEqual(0.5, result.NormalCdf, 1e-14);
        Assert.IsTrue(result.CommonPassed);
        Assert.AreEqual(AnalysisCatalog.All.Count, result.DescriptorCount);
        Assert.AreEqual(AnalysisHandlerRegistry.All.Count, result.HandlerCount);
        Assert.AreEqual(2, result.DescriptorCount);
        Assert.AreEqual(4, result.HandlerCount);
        Assert.AreEqual("probe.passed", result.Code);
    }

    [TestMethod]
    public async Task RealExcelReplyRequiresInitializationAndApiSupport()
    {
        var js = new FakeRuntime { Reply = Excel() };
        await using var host = new OfficeHostReadiness(js);
        var result = await host.InitializeAsync();
        Assert.IsTrue(result.IsExcel);
        Assert.AreEqual("OfficeOnline", result.Platform);
        CollectionAssert.AreEqual(new[] { "import", "create", "initialize" }, js.Calls.ToArray());
    }

    [DataTestMethod]
    [DataRow("standalone", "office.unavailable", false, false, HostState.Standalone)]
    [DataRow("failed", "office.timeout", false, false, HostState.Failed)]
    [DataRow("failed", "office.initialization-failed", false, false, HostState.Failed)]
    [DataRow("unsupported", "office.unsupported-host", true, false, HostState.Unsupported)]
    [DataRow("unsupported", "office.unsupported-api", true, false, HostState.Unsupported)]
    [DataRow("excel", "office.excel-ready", false, true, HostState.Failed)]
    [DataRow("excel", "office.excel-ready", true, false, HostState.Failed)]
    [DataRow("excel", "secret workbook address", true, true, HostState.Failed)]
    [DataRow("unknown", "office.excel-ready", true, true, HostState.Failed)]
    [DataRow("standalone", "office.unavailable", true, true, HostState.Failed)]
    public async Task ProjectedStatesFailClosed(string state, string code, bool ready, bool api, HostState expected)
    {
        var js = new FakeRuntime { Reply = new() { State = state, Code = code, OfficeReady = ready,
            ExcelApiSupported = api, Platform = "private workbook URL" } };
        await using var host = new OfficeHostReadiness(js);
        var result = await host.InitializeAsync();
        Assert.AreEqual(expected, result.State);
        Assert.IsFalse(result.IsExcel);
        Assert.AreEqual("", result.Platform);
        Assert.IsFalse(result.ToString().Contains("private"));
        Assert.IsFalse(result.ToString().Contains("secret"));
    }

    [TestMethod]
    public async Task NullInteropReplyIsFailure()
    {
        await using var host = new OfficeHostReadiness(new FakeRuntime());
        Assert.AreEqual(HostState.Failed, (await host.InitializeAsync()).State);
    }

    [DataTestMethod]
    [DataRow("import")]
    [DataRow("create")]
    [DataRow("initialize")]
    public async Task InteropExceptionsHaveFixedSafeProjection(string failure)
    {
        var js = new FakeRuntime { ThrowAt = failure };
        await using var host = new OfficeHostReadiness(js);
        var result = await host.InitializeAsync();
        Assert.AreEqual(HostStatus.Failed, result);
        Assert.IsFalse(result.Message.Contains("SECRET"));
        Assert.AreEqual($"bootstrap.interop-{(failure == "initialize" ? "invoke" : failure)}-failed", js.Diagnostic);
    }

    [TestMethod]
    public async Task DiagnosticFailureCannotReplaceTheSafeInteropFailure()
    {
        await using var host = new OfficeHostReadiness(new FakeRuntime { ThrowAt = "create", ThrowDiagnostic = true });
        Assert.AreEqual(HostStatus.Failed, await host.InitializeAsync());
    }

    [TestMethod]
    public void BrowserCamelCaseReplyDeserializesWithoutChangingProjection()
    {
        const string json = """{"state":"excel","code":"office.excel-ready","platform":"OfficeOnline","officeReady":true,"excelApiSupported":true}""";
        var reply = JsonSerializer.Deserialize<HostReply>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.AreEqual(new HostStatus(HostState.Excel, "office.excel-ready", "OfficeOnline"), HostStatus.Project(reply));
    }

    [TestMethod]
    public async Task ConcurrentAndRepeatedStartupCoalesceAndRetryReusesAdapter()
    {
        var js = new FakeRuntime { Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        await using var host = new OfficeHostReadiness(js);
        var first = host.InitializeAsync();
        var second = host.InitializeAsync(retry: true);
        Assert.AreEqual(1, js.Calls.Count(x => x == "initialize"));
        js.Pending.SetResult(Excel());
        Assert.AreEqual(await first, await second);
        await host.InitializeAsync();
        Assert.AreEqual(1, js.Calls.Count(x => x == "initialize"));
        js.Pending = null;
        js.Reply = Excel();
        Assert.IsTrue((await host.InitializeAsync(retry: true)).IsExcel);
        Assert.AreEqual(2, js.Calls.Count(x => x == "initialize"));
        Assert.AreEqual(1, js.Calls.Count(x => x == "create"));
    }

    [TestMethod]
    public async Task CallerCancellationDoesNotPoisonOtherWaiters()
    {
        var js = new FakeRuntime { Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        await using var host = new OfficeHostReadiness(js);
        using var cancelled = new CancellationTokenSource();
        var first = host.InitializeAsync(cancelled.Token);
        var second = host.InitializeAsync();
        cancelled.Cancel();
        Assert.AreEqual(HostState.Disposed, (await first).State);
        js.Pending.SetResult(Excel());
        Assert.IsTrue((await second).IsExcel);
    }

    [TestMethod]
    public async Task PreCancelledStartupDoesNotTouchJs()
    {
        var js = new FakeRuntime();
        await using var host = new OfficeHostReadiness(js);
        Assert.AreEqual(HostState.Disposed, (await host.InitializeAsync(new CancellationToken(true))).State);
        Assert.AreEqual(0, js.Calls.Count);
    }

    [DataTestMethod]
    [DataRow("import")]
    [DataRow("create")]
    [DataRow("initialize")]
    public async Task DisposalDuringInteropCancelsAndReleasesAcquiredReferences(string stage)
    {
        var js = new FakeRuntime { BlockAt = stage };
        var host = new OfficeHostReadiness(js);
        var pending = host.InitializeAsync();
        await host.DisposeAsync();
        Assert.AreEqual(HostState.Disposed, (await pending).State);
        Assert.AreEqual(HostState.Disposed, (await host.InitializeAsync(retry: true)).State);
        await host.DisposeAsync();
        Assert.AreEqual(stage == "import" ? 0 : stage == "create" ? 1 : 2, js.Disposals);
        Assert.AreEqual(stage == "initialize" ? 1 : 0, js.Calls.Count(x => x == "dispose"));
    }

    [TestMethod]
    public async Task CleanupExceptionsAreSafeAndDisposalRemainsIdempotent()
    {
        var js = new FakeRuntime { Reply = Excel(), ThrowAt = "dispose" };
        var host = new OfficeHostReadiness(js);
        await host.InitializeAsync();
        await host.DisposeAsync();
        await host.DisposeAsync();
        Assert.AreEqual(2, js.Disposals);
    }

    [TestMethod]
    public async Task RetryAfterFailedImportRecoversWithoutUsingCachedFailure()
    {
        var js = new FakeRuntime { ThrowAt = "import", Reply = Excel() };
        await using var host = new OfficeHostReadiness(js);
        Assert.AreEqual(HostState.Failed, (await host.InitializeAsync()).State);
        js.ThrowAt = null;
        Assert.AreEqual(HostState.Failed, (await host.InitializeAsync()).State);
        Assert.IsTrue((await host.InitializeAsync(retry: true)).IsExcel);
        Assert.AreEqual(2, js.Calls.Count(x => x == "import"));
    }

    [TestMethod]
    public async Task ExcelReplyDiscardsUnrecognizedPlatformMetadata()
    {
        var reply = Excel();
        reply.Platform = "SECRET workbook path";
        await using var host = new OfficeHostReadiness(new FakeRuntime { Reply = reply });
        var result = await host.InitializeAsync();
        Assert.IsTrue(result.IsExcel);
        Assert.AreEqual("", result.Platform);
        Assert.IsFalse(result.ToString().Contains("SECRET"));
    }

    [TestMethod]
    public void ManifestAndLaunchProfileAreDistinctLocalWorkbookTaskPane()
    {
        string root = FindRoot();
        XNamespace ns = "http://schemas.microsoft.com/office/appforoffice/1.1";
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
        var app = XDocument.Load(Path.Combine(root, "BESHStat.OfficeAddin/manifest-dev.xml")).Root!;
        var old = XDocument.Load(Path.Combine(root, "BESHStat.WasmSmoke/office-manifest.xml")).Root!;
        Assert.AreEqual(ns + "OfficeApp", app.Name);
        Assert.AreEqual("TaskPaneApp", (string?)app.Attribute(xsi + "type"));
        Assert.AreNotEqual(Guid.Parse(app.Element(ns + "Id")!.Value), Guid.Parse(old.Element(ns + "Id")!.Value));
        Assert.AreNotEqual((string?)app.Element(ns + "DisplayName")!.Attribute("DefaultValue"),
            (string?)old.Element(ns + "DisplayName")!.Attribute("DefaultValue"));
        Assert.AreEqual("Workbook", (string?)app.Element(ns + "Hosts")!.Element(ns + "Host")!.Attribute("Name"));
        var set = app.Element(ns + "Requirements")!.Element(ns + "Sets")!.Element(ns + "Set")!;
        Assert.AreEqual("ExcelApi", (string?)set.Attribute("Name"));
        Assert.AreEqual("1.1", (string?)set.Attribute("MinVersion"));
        Assert.AreEqual("ReadWriteDocument", app.Element(ns + "Permissions")!.Value);
        Assert.AreEqual(1, app.Descendants().Count(x => x.Name.LocalName == "VersionOverrides"));
        var url = new Uri((string)app.Element(ns + "DefaultSettings")!.Element(ns + "SourceLocation")!.Attribute("DefaultValue")!);
        using var profile = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "BESHStat.OfficeAddin/Properties/launchSettings.json")));
        var launchUrl = new Uri(profile.RootElement.GetProperty("profiles").GetProperty("https").GetProperty("applicationUrl").GetString()!);
        Assert.AreEqual(launchUrl, url);
        Assert.AreEqual("https", url.Scheme);
        Assert.AreEqual("localhost", url.Host);
        Assert.AreEqual(7141, url.Port);
        Assert.AreEqual("https://localhost:7131/", (string?)old.Element(ns + "DefaultSettings")!.Element(ns + "SourceLocation")!.Attribute("DefaultValue"));
        CollectionAssert.AreEqual(new[] { "Id", "Version", "ProviderName", "DefaultLocale", "DisplayName", "Description", "IconUrl", "HighResolutionIconUrl", "SupportUrl", "Hosts", "Requirements", "DefaultSettings", "Permissions", "VersionOverrides" },
            app.Elements().Select(x => x.Name.LocalName).ToArray());
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BESHStatNG.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new AssertFailedException("Repository root not found.");
    }

    private sealed class FakeRuntime : IJSRuntime, IJSObjectReference
    {
        public List<string> Calls { get; } = [];
        public HostReply? Reply { get; set; }
        public TaskCompletionSource<HostReply>? Pending { get; set; }
        public string? ThrowAt { get; set; }
        public string? BlockAt { get; set; }
        public string? Diagnostic { get; private set; }
        public bool ThrowDiagnostic { get; set; }
        public int Disposals { get; private set; }
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, default, args);
        public async ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Calls.Add(identifier);
            if (identifier == "__beshOfficeDebug.record")
            {
                Diagnostic = args?[0] as string;
                if (ThrowDiagnostic) throw new JSException("SECRET");
            }
            if (identifier == ThrowAt) throw new JSException("SECRET document URL, cell contents, response body");
            if (identifier == BlockAt) await Task.Delay(Timeout.Infinite, cancellationToken);
            if (identifier is "import" or "create") return (T)(object)this;
            if (identifier == "initialize") return (T)(object?)(Pending is null ? Reply : await Pending.Task.WaitAsync(cancellationToken))!;
            return default!;
        }
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
    }
}
