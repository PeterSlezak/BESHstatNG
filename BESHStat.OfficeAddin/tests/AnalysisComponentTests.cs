// RenderTree APIs are intentionally confined to this dependency-free test renderer.
#pragma warning disable BL0006
using BESHStat.OfficeAddin.Components;
using BESHStat.OfficeAddin.Components.OptionEditors;
using BESHStat.OfficeAddin.Infrastructure;
using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class AnalysisComponentTests
{
    [TestMethod]
    public async Task RealHtmlRenderContainsDescriptorNavigationAndDisabledHonestRun()
    {
        using var services = Services().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<AnalysisShell>()).ToHtmlString());
        foreach (var category in GeneratedMethodMetadata.All.Select(m => m.Category).Distinct(StringComparer.Ordinal))
        {
            StringAssert.Contains(html, System.Net.WebUtility.HtmlEncode(category));
        }
        StringAssert.Contains(html, "<select id=\"analysis-group\"");
        StringAssert.Contains(html, "<select id=\"analysis-method\"");
        StringAssert.Contains(html, "for=\"analysis-group\"");
        StringAssert.Contains(html, "for=\"analysis-method\"");
        Assert.IsFalse(html.Contains("data-analysis-id"));
        StringAssert.Contains(html, "disabled aria-describedby=\"analysis-run-unavailable\"");
        StringAssert.Contains(html, "not available until 04.04");
        Assert.IsFalse(html.Contains("analysis-alpha"));
    }

    [DataTestMethod]
    [DataRow(HostState.Standalone)] [DataRow(HostState.Failed)] [DataRow(HostState.Excel)]
    public async Task ActualAppEventsSwitchEditorsRetryPreservesAndReopenResetsWithoutWorkbookCalls(HostState status)
    {
        var workbook = new CountingWorkbook();
        var host = new Ready(status);
        using var services = Services().AddSingleton<IWorkbookAdapter>(workbook)
            .AddSingleton<IOfficeHostReadiness>(host).BuildServiceProvider();
        await using (var renderer = new EventRenderer(services))
        {
            await renderer.Mount<App>();
            Assert.AreEqual(1, host.Calls);
            await renderer.Choose(AnalysisIds.OneWayAnova);
            Assert.AreEqual(AnalysisIds.OneWayAnova, renderer.Attribute("id", "analysis-method", "value"));
            Assert.AreEqual("0.05", renderer.Attribute("id", "analysis-alpha", "value"));
            foreach (var id in new[] { "anova-welch", "anova-fisher", "anova-bonferroni", "anova-tukey", "anova-games" })
            {
                Assert.IsTrue(renderer.Has("for", id));
                await renderer.Event("id", id, "onchange", new ChangeEventArgs { Value = true });
                Assert.AreEqual(true, renderer.Attribute("id", id, "checked"));
                await renderer.Event("id", id, "onchange", new ChangeEventArgs { Value = false });
                Assert.AreNotEqual(true, renderer.Attribute("id", id, "checked"));
            }
            await renderer.Event("id", "analysis-alpha", "oninput", new ChangeEventArgs { Value = "0" });
            Assert.AreEqual("true", renderer.Attribute("id", "analysis-alpha", "aria-invalid"));
            Assert.AreEqual("0", renderer.Attribute("id", "analysis-alpha", "value"));
            Assert.IsTrue(renderer.Has("id", "alpha-error"));
            await renderer.Event("id", "analysis-alpha", "oninput", new ChangeEventArgs { Value = ".125" });
            Assert.AreEqual("false", renderer.Attribute("id", "analysis-alpha", "aria-invalid"));
            await renderer.ClickText("Retry initialization");
            Assert.AreEqual(2, host.Calls);
            Assert.AreEqual(".125", renderer.Attribute("id", "analysis-alpha", "value"));
            Assert.AreEqual("Parametric", renderer.Attribute("id", "analysis-group", "value"));
            Assert.AreEqual(AnalysisIds.OneWayAnova, renderer.Attribute("id", "analysis-method", "value"));
            await renderer.Event("id", "analysis-alpha", "oninput", new ChangeEventArgs { Value = "NaN" });
            await renderer.ClickText("Retry initialization");
            Assert.AreEqual("NaN", renderer.Attribute("id", "analysis-alpha", "value"));
            Assert.IsTrue(renderer.Has("id", "alpha-error"));
            Assert.AreEqual(0, workbook.Calls);
            await renderer.Choose(AnalysisIds.KruskalWallis);
            Assert.IsFalse(renderer.Has("id", "anova-welch"));
            Assert.AreEqual(true, renderer.Attribute("id", "kw-dunn", "checked"));
            Assert.IsTrue(renderer.Has("for", "kw-dunn"));
            Assert.AreEqual("0.05", renderer.Attribute("id", "analysis-alpha", "value"));
            await renderer.Event("id", "kw-dunn", "onchange", new ChangeEventArgs { Value = false });
            Assert.AreNotEqual(true, renderer.Attribute("id", "kw-dunn", "checked"));
            await renderer.Choose(AnalysisIds.OneWayAnova);
            Assert.IsFalse(renderer.Has("id", "kw-dunn"));
            Assert.AreEqual("0.05", renderer.Attribute("id", "analysis-alpha", "value"));
            Assert.AreEqual(0, workbook.Calls);
        }
        Assert.AreEqual(1, workbook.Disposals);
        Assert.AreEqual(1, host.Disposals);
        await using var reopened = new EventRenderer(services);
        await reopened.Mount<AnalysisShell>();
        Assert.IsFalse(reopened.Has("id", "analysis-alpha"));
        Assert.AreEqual("", reopened.Attribute("id", "analysis-group", "value"));
        await reopened.Choose(AnalysisIds.KruskalWallis);
        Assert.AreEqual(true, reopened.Attribute("id", "kw-dunn", "checked"));
        Assert.AreEqual(0, workbook.Calls);
    }

    [TestMethod]
    public async Task AlphaAndCheckboxHtmlHaveExplicitLabelsValidationAndFieldset()
    {
        using var services = Services().BuildServiceProvider();
        using var draft = new KruskalWallisOptionDraft(new());
        draft.SetAlpha("NaN");
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<KruskalWallisOptions>(ParameterView.FromDictionary(
                new Dictionary<string, object?> { ["Draft"] = draft }))).ToHtmlString());
        foreach (var expected in new[] { "<fieldset>", "<legend>Statistical options</legend>", "for=\"analysis-alpha\"",
            "aria-invalid=\"true\"", "aria-describedby=\"alpha-hint alpha-error\"", "id=\"alpha-error\"",
            "value=\"NaN\"", "for=\"kw-dunn\"", "type=\"checkbox\"", "Include Dunn comparisons" })
            StringAssert.Contains(html, expected);
    }

    [TestMethod]
    public async Task DisposingAppDuringPendingInitializationDoesNotMutateOrReadAfterCompletion()
    {
        var host = new Ready(HostState.Excel) { Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        var workbook = new CountingWorkbook();
        using var services = Services().AddSingleton<IOfficeHostReadiness>(host)
            .AddSingleton<IWorkbookAdapter>(workbook).BuildServiceProvider();
        var renderer = new EventRenderer(services);
        await renderer.Mount<App>();
        await renderer.Choose(AnalysisIds.OneWayAnova);
        await renderer.DisposeAsync();
        var displays = renderer.Displays;
        host.Pending.SetResult(new(HostState.Excel, "office.excel-ready"));
        await renderer.InitializationSettled();
        Assert.AreEqual(displays, renderer.Displays);
        Assert.AreEqual(0, workbook.Calls);
        Assert.AreEqual(1, workbook.Disposals);
    }

    private static IServiceCollection Services() => new ServiceCollection().AddLogging()
        .AddSingleton<IAnalysisNavigationCatalog, AnalysisNavigationCatalog>().AddSingleton<AnalysisEditorRegistry>()
        .AddSingleton<AnalysisNavigationMenu>()
        .AddSingleton<IFoundationProbe, FoundationProbe>().AddSingleton(new WorkbookDiagnostics(false));

    [TestMethod]
    public async Task ActualDropdownEventsRepresentAllMethodsAndRejectUnsupportedGroupsAndForgedChoices()
    {
        using var services = Services().BuildServiceProvider();
        await using var renderer = new EventRenderer(services);
        await renderer.Mount<AnalysisShell>();
        Assert.AreEqual(13, renderer.Options("analysis-group").Count); // 12 generated groups + placeholder.
        Assert.AreEqual(true, renderer.Attribute("id", "analysis-method", "disabled"));
        int methods = 0;
        foreach (var category in GeneratedMethodMetadata.All.Select(m => m.Category).Distinct(StringComparer.Ordinal))
        {
            await renderer.Change("analysis-group", category);
            var options = renderer.Options("analysis-method").Skip(1).ToArray();
            var metadata = GeneratedMethodMetadata.All.Where(m => m.Category == category)
                .OrderBy(m => m.DisplayName, StringComparer.Ordinal).ThenBy(m => m.Key, StringComparer.Ordinal).ToArray();
            Assert.AreEqual(metadata.Length, options.Length);
            methods += options.Length;
            for (int i = 0; i < options.Length; i++)
            {
                var supported = metadata[i].AnalysisId is AnalysisIds.OneWayAnova or AnalysisIds.KruskalWallis;
                Assert.AreEqual(metadata[i].DisplayName + (supported ? "" : " — Not yet available"), options[i].Label);
                Assert.AreEqual(!supported, options[i].Disabled);
                if (supported) Assert.AreEqual(metadata[i].AnalysisId, options[i].Value);
            }
        }
        Assert.AreEqual(76, methods);
        foreach (var category in new[] { "Multivariate Analysis", "Graphics" })
        {
            await renderer.Change("analysis-group", category);
            Assert.IsFalse(renderer.Has("id", "analysis-alpha"));
            StringAssert.Contains((string)renderer.Attribute("id", "analysis-navigation-status", "text")!, "Not yet available in this Office add-in");
            Assert.IsTrue(renderer.Options("analysis-method").Skip(1).All(o => o.Disabled));
        }
        await renderer.Choose(AnalysisIds.OneWayAnova);
        await renderer.Event("id", "analysis-alpha", "oninput", new ChangeEventArgs { Value = "0" });
        await renderer.Event("id", "anova-welch", "onchange", new ChangeEventArgs { Value = true });
        await renderer.Change("analysis-group", "Parametric");
        await renderer.Change("analysis-method", AnalysisIds.OneWayAnova);
        Assert.AreEqual("0", renderer.Attribute("id", "analysis-alpha", "value"));
        Assert.AreEqual(true, renderer.Attribute("id", "anova-welch", "checked"));
        await renderer.Change("analysis-group", "Nonparametric");
        Assert.IsFalse(renderer.Has("id", "alpha-error"));
        Assert.AreEqual("", renderer.Attribute("id", "analysis-method", "value"));
        await renderer.Change("analysis-method", AnalysisIds.OneWayAnova); // Stale event from old group.
        Assert.IsFalse(renderer.Has("id", "analysis-alpha"));
        StringAssert.Contains((string)renderer.Attribute("id", "analysis-navigation-status", "text")!, "Options are unavailable");
        await renderer.Change("analysis-method", "unavailable:CochranSQTest");
        Assert.IsFalse(renderer.Has("id", "analysis-alpha"));
        await renderer.Change("analysis-group", 123); // Malformed event payload safely clears.
        Assert.AreEqual("", renderer.Attribute("id", "analysis-group", "value"));
        Assert.IsFalse(renderer.Has("id", "analysis-alpha"));
        StringAssert.Contains(renderer.StaticMarkup, "<button type=\"button\" disabled aria-describedby=\"analysis-run-unavailable\"");
    }

    private sealed class Ready(HostState state) : IOfficeHostReadiness
    {
        public int Calls, Disposals;
        public TaskCompletionSource<HostStatus>? Pending;
        public Task<HostStatus> InitializeAsync(CancellationToken cancellationToken = default, bool retry = false)
        {
            Calls++;
            return Pending?.Task ?? Task.FromResult(new HostStatus(state,
                state == HostState.Excel ? "office.excel-ready" : state == HostState.Failed ? "office.timeout" : "office.unavailable"));
        }
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
    }

    private sealed class CountingWorkbook : IWorkbookAdapter
    {
        public int Calls, Disposals;
        private Task<T> Unexpected<T>() { Calls++; throw new AssertFailedException("Unexpected workbook call while editing options."); }
        public Task<WorkbookResult<HostCapabilitySnapshot>> GetCapabilitiesAsync(CancellationToken cancellationToken = default) => Unexpected<WorkbookResult<HostCapabilitySnapshot>>();
        public Task<WorkbookResult<SelectedRangeMetadata>> InspectSelectionAsync(CancellationToken cancellationToken = default) => Unexpected<WorkbookResult<SelectedRangeMetadata>>();
        public Task<WorkbookResult<SpreadsheetRangeData>> ReadSelectionAsync(InputSizeLimits? limits = null, CancellationToken cancellationToken = default) => Unexpected<WorkbookResult<SpreadsheetRangeData>>();
        public Task<WorkbookResult<SpreadsheetRangeData>> ReadRangeAsync(SpreadsheetRangeReference reference, InputSizeLimits? limits = null, CancellationToken cancellationToken = default) => Unexpected<WorkbookResult<SpreadsheetRangeData>>();
        public Task<WriteResult> WriteAsync(OfficeOutputPlan plan, WriteIntent intent, WriteConsent? consent = null, CancellationToken cancellationToken = default) => Unexpected<WriteResult>();
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
    }

    private sealed class EventRenderer(IServiceProvider services) : Renderer(services, services.GetRequiredService<ILoggerFactory>())
    {
        private int root;
        private readonly List<Task> displays = [];
        public int Displays { get; private set; }
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        protected override void HandleException(Exception exception) => throw new AssertFailedException("Component render failed.", exception);
        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) { Displays++; return Task.CompletedTask; }
        public Task Mount<T>() where T : IComponent => Dispatcher.InvokeAsync(() =>
        {
            root = AssignRootComponentId(InstantiateComponent(typeof(T)));
            displays.Add(RenderRootComponentAsync(root));
        });
        public Task InitializationSettled() => Dispatcher.InvokeAsync(async () => { await Task.WhenAll(displays); });
        private IEnumerable<RenderTreeFrame[]> Trees(int id)
        {
            var frames = GetCurrentRenderTreeFrames(id);
            var items = frames.Array.Take(frames.Count).ToArray();
            yield return items;
            foreach (var frame in items.Where(f => f.FrameType == RenderTreeFrameType.Component))
                foreach (var child in Trees(frame.ComponentId)) yield return child;
        }
        private IEnumerable<Dictionary<string, object>> Elements()
        {
            foreach (var tree in Trees(root))
                for (int i = 0; i < tree.Length; i++)
                    if (tree[i].FrameType == RenderTreeFrameType.Element)
                    {
                        var attrs = new Dictionary<string, object>();
                        for (int j = i + 1; j < tree.Length && tree[j].FrameType == RenderTreeFrameType.Attribute; j++)
                        {
                            attrs[tree[j].AttributeName] = tree[j].AttributeValue;
                            if (tree[j].AttributeEventHandlerId != 0) attrs[tree[j].AttributeName + ":event"] = tree[j].AttributeEventHandlerId;
                        }
                        attrs["text"] = string.Concat(tree.Skip(i + 1).Take(tree[i].ElementSubtreeLength - 1)
                            .Where(f => f.FrameType == RenderTreeFrameType.Text).Select(f => f.TextContent));
                        yield return attrs;
                    }
        }
        public bool Has(string attr, string value) => Elements().Any(e => e.TryGetValue(attr, out var actual) && Equals(actual, value));
        public object? Attribute(string attr, string value, string target) =>
            Elements().Single(e => e.TryGetValue(attr, out var actual) && Equals(actual, value)).GetValueOrDefault(target);
        public Task Event(string attr, string value, string name, EventArgs args) => Dispatcher.InvokeAsync(() =>
        {
            var element = Elements().Single(e => e.TryGetValue(attr, out var actual) && Equals(actual, value));
            return DispatchEventAsync((ulong)element[name + ":event"], null, args);
        });
        public Task ClickText(string text) => Event("text", text, "onclick", new MouseEventArgs());
        public string StaticMarkup => string.Concat(Trees(root).SelectMany(t => t)
            .Where(f => f.FrameType == RenderTreeFrameType.Markup).Select(f => f.MarkupContent));
        public IReadOnlyList<(string Value, string Label, bool Disabled)> Options(string id)
        {
            foreach (var tree in Trees(root))
                for (int i = 0; i < tree.Length; i++)
                    if (tree[i].FrameType == RenderTreeFrameType.Element && tree[i].ElementName == "select"
                        && tree.Skip(i + 1).TakeWhile(f => f.FrameType == RenderTreeFrameType.Attribute)
                            .Any(f => f.AttributeName == "id" && Equals(f.AttributeValue, id)))
                    {
                        var options = new List<(string, string, bool)>();
                        for (int j = i + 1; j < i + tree[i].ElementSubtreeLength; j++)
                            if (tree[j].FrameType == RenderTreeFrameType.Element && tree[j].ElementName == "option")
                            {
                                var attrs = tree.Skip(j + 1).TakeWhile(f => f.FrameType == RenderTreeFrameType.Attribute).ToArray();
                                var label = string.Concat(tree.Skip(j + 1).Take(tree[j].ElementSubtreeLength - 1)
                                    .Where(f => f.FrameType == RenderTreeFrameType.Text).Select(f => f.TextContent));
                                // Razor may represent the static empty value attribute as Boolean true.
                                options.Add((attrs.Single(f => f.AttributeName == "value").AttributeValue as string ?? "", label,
                                    attrs.Any(f => f.AttributeName == "disabled" && Equals(f.AttributeValue, true))));
                            }
                        return options;
                    }
            throw new AssertFailedException("Select element not found: " + id);
        }
        public Task Change(string id, object? value) => Event("id", id, "onchange", new ChangeEventArgs { Value = value });
        public async Task Choose(string id)
        {
            await Change("analysis-group", AnalysisCatalog.GetById(id).Category);
            await Change("analysis-method", id);
        }
    }
}
