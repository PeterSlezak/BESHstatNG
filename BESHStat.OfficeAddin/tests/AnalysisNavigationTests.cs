using System.Globalization;
using BESHStat.OfficeAddin.Components.OptionEditors;
using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class AnalysisNavigationTests
{
    private static AnalysisNavigationState State() => new(new AnalysisNavigationCatalog(), new());
    private static bool Select(AnalysisNavigationState state, string id)
    {
        state.SelectGroup(AnalysisCatalog.GetById(id).Category);
        return state.Select(id);
    }

    [TestMethod]
    public void CatalogContainsOnlyVerifiedCommonDescriptorsWithExactMetadataAndStableOrder()
    {
        var catalog = new AnalysisNavigationCatalog();
        CollectionAssert.AreEquivalent(new[] { AnalysisIds.OneWayAnova, AnalysisIds.KruskalWallis }, catalog.All.Select(d => d.Id).ToArray());
        foreach (var actual in catalog.All)
        {
            var expected = AnalysisCatalog.GetById(actual.Id);
            Assert.AreSame(expected, actual);
            var metadata = GeneratedMethodMetadata.GetByAnalysisId(actual.Id);
            Assert.AreEqual(metadata.DisplayName, actual.DisplayName);
            Assert.AreEqual(metadata.Category, actual.Category);
            Assert.AreEqual(metadata.Description, actual.Description);
            Assert.AreEqual(metadata.HelpUrl, actual.HelpUrl);
        }
        var reversed = new AnalysisNavigationCatalog(AnalysisCatalog.All.Reverse(), AnalysisHandlerRegistry.All.Reverse());
        CollectionAssert.AreEqual(catalog.All.Select(d => d.Id).ToArray(), reversed.All.Select(d => d.Id).ToArray());
    }

    [DataTestMethod]
    [DataRow(null)] [DataRow("")] [DataRow("ANOVA.ONEWAY")] [DataRow("anova.oneway ")]
    [DataRow("One-Way ANOVA")] [DataRow("anova")] [DataRow("unknown.method")]
    public void UnknownOrNonExactIdentityFailsClosed(string? id)
    {
        using var state = State();
        Assert.IsFalse(state.Select(id));
        Assert.IsNull(state.Selected);
        Assert.IsNull(state.Editor);
        Assert.AreEqual(string.IsNullOrEmpty(id) ? null : AnalysisNavigationState.UnavailableReason, state.Rejection);
    }

    [TestMethod]
    public void MissingOneInputHandlerRejectsMethodWithoutExecutingAnything()
    {
        var handlers = AnalysisHandlerRegistry.All.Where(h => h.AnalysisId != AnalysisIds.OneWayAnova || h.InputType != typeof(GroupedNumericData));
        var catalog = new AnalysisNavigationCatalog(AnalysisCatalog.All, handlers);
        Assert.IsFalse(catalog.TryGet(AnalysisIds.OneWayAnova, out _));
        Assert.IsTrue(catalog.TryGet(AnalysisIds.KruskalWallis, out _));
        Assert.AreEqual(0, new AnalysisNavigationCatalog(AnalysisCatalog.All, Array.Empty<IAnalysisHandler>()).All.Count);
    }

    [DataTestMethod]
    [DataRow("wrong-schema")] [DataRow("wrong-type")] [DataRow("wrong-factory")]
    [DataRow("missing-handler")] [DataRow("wrong-handler-type")] [DataRow("planned")]
    public void UnsupportedDescriptorsAndRegistrationsFailClosed(string defect)
    {
        var source = AnalysisCatalog.GetById(AnalysisIds.OneWayAnova);
        var type = defect == "wrong-type" ? typeof(KruskalWallisAnalysisOptions) : source.OptionsType;
        var descriptor = new AnalysisDescriptor(source.Id, source.DisplayName, source.Category, source.Description, source.HelpUrl,
            source.ContractVersion, source.InputSchemaId, defect == "wrong-schema" ? "future.options.v1" : source.OptionsSchemaId,
            source.InputTypes, type,
            () => defect is "wrong-type" or "wrong-factory" ? new KruskalWallisAnalysisOptions() : new OneWayAnovaAnalysisOptions(),
            source.OutputKinds, defect == "planned" ? AnalysisAvailability.Planned : source.Availability,
            source.EstimatedCost, source.RequiredCapabilities, source.LegacyKind);
        var handlers = defect == "missing-handler" ? Array.Empty<IAnalysisHandler>() :
            source.InputTypes.Select(t => (IAnalysisHandler)new NeverExecute(source.Id, t,
                defect == "wrong-handler-type" ? typeof(KruskalWallisAnalysisOptions) : type)).ToArray();
        using var state = new AnalysisNavigationState(new AnalysisNavigationCatalog(new[] { descriptor }, handlers), new());
        state.SelectGroup(source.Category);
        Assert.IsFalse(state.Select(source.Id));
        Assert.IsNull(state.Editor);
        Assert.AreEqual(AnalysisNavigationState.UnavailableReason, state.Rejection);
    }

    [DataTestMethod]
    [DataRow(AnalysisIds.OneWayAnova, typeof(AnovaOptions), typeof(AnovaOptionDraft))]
    [DataRow(AnalysisIds.KruskalWallis, typeof(KruskalWallisOptions), typeof(KruskalWallisOptionDraft))]
    public void ValidatedSchemaSelectsTypedEditor(string id, Type component, Type draft)
    {
        using var state = State();
        Assert.IsTrue(Select(state, id));
        Assert.AreEqual(id, state.Selected!.Id);
        Assert.AreEqual(component, state.Editor!.ComponentType);
        Assert.AreEqual(draft, state.Editor.Draft.GetType());
    }

    [TestMethod]
    public void DefaultsAndSnapshotCopiesComeFromConcreteCommonOptions()
    {
        using var state = State();
        Select(state, AnalysisIds.OneWayAnova);
        var expected = (OneWayAnovaAnalysisOptions)AnalysisCatalog.GetById(AnalysisIds.OneWayAnova).CreateDefaultOptions();
        var actual = (OneWayAnovaAnalysisOptions)state.Editor!.Draft.Snapshot();
        Assert.AreEqual(0.05, actual.Alpha);
        Assert.AreEqual(expected.Alpha, actual.Alpha);
        CollectionAssert.AreEqual(Flags(expected), Flags(actual));
        actual.IncludeWelch = !actual.IncludeWelch;
        actual.Alpha = .9;
        var unchanged = (OneWayAnovaAnalysisOptions)state.Editor.Draft.Snapshot();
        Assert.AreEqual(expected.Alpha, unchanged.Alpha);
        CollectionAssert.AreEqual(Flags(expected), Flags(unchanged));
        Select(state, AnalysisIds.KruskalWallis);
        var kw = (KruskalWallisAnalysisOptions)state.Editor!.Draft.Snapshot();
        var kwDefault = (KruskalWallisAnalysisOptions)state.Selected!.CreateDefaultOptions();
        Assert.AreEqual(0.05, kw.Alpha);
        Assert.IsTrue(kw.IncludeDunnComparisons);
        Assert.AreEqual(kwDefault.IncludeDunnComparisons, kw.IncludeDunnComparisons);
    }

    [DataTestMethod]
    [DataRow("0.1", .1)] [DataRow(" 0.025 ", .025)] [DataRow("1e-3", .001)]
    [DataRow(".5", .5)] [DataRow("0.999999", .999999)]
    public void ValidAlphaCommitsInvariantValue(string text, double expected)
    {
        using var draft = new AnovaOptionDraft(new());
        draft.SetAlpha(text);
        Assert.IsTrue(draft.IsValid);
        Assert.IsNull(draft.AlphaError);
        Assert.AreEqual(text, draft.AlphaText);
        Assert.AreEqual(expected, ((OneWayAnovaAnalysisOptions)draft.Snapshot()).Alpha);
    }

    [DataTestMethod]
    [DataRow("0")] [DataRow("1")] [DataRow("-0.1")] [DataRow("1.1")] [DataRow("")]
    [DataRow(" ")] [DataRow("NaN")] [DataRow("Infinity")] [DataRow("-Infinity")]
    [DataRow("1e9999")] [DataRow("0,05")] [DataRow("0,500")] [DataRow("abc")]
    public void InvalidAlphaRemainsVisibleAndCannotProduceSnapshot(string text)
    {
        using var draft = new KruskalWallisOptionDraft(new());
        draft.SetAlpha(".2");
        draft.SetAlpha(text);
        Assert.IsFalse(draft.IsValid);
        Assert.AreEqual(text, draft.AlphaText);
        Assert.IsFalse(string.IsNullOrWhiteSpace(draft.AlphaError));
        Assert.ThrowsException<InvalidOperationException>(() => draft.Snapshot());
        draft.SetAlpha(".03");
        Assert.IsTrue(draft.IsValid);
        Assert.AreEqual(.03, ((KruskalWallisAnalysisOptions)draft.Snapshot()).Alpha);
    }

    [TestMethod]
    public void DecimalCommaCultureDoesNotChangeAlphaGrammarOrDefaults()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sk-SK");
            using var draft = new AnovaOptionDraft(new());
            Assert.AreEqual("0.05", draft.AlphaText);
            draft.SetAlpha("0,05");
            Assert.IsFalse(draft.IsValid);
            draft.SetAlpha("0.125");
            Assert.AreEqual(.125, ((OneWayAnovaAnalysisOptions)draft.Snapshot()).Alpha);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [DataTestMethod]
    [DataRow(0)] [DataRow(1)] [DataRow(2)] [DataRow(3)] [DataRow(4)]
    public void AnovaFlagsIndependentlyRoundTrip(int index)
    {
        using var draft = new AnovaOptionDraft(new());
        Action<bool>[] setters = [v => draft.IncludeWelch = v, v => draft.IncludeFisherLsd = v,
            v => draft.IncludeBonferroni = v, v => draft.IncludeTukeyKramer = v, v => draft.IncludeGamesHowell = v];
        var expected = Flags((OneWayAnovaAnalysisOptions)draft.Snapshot());
        expected[index] = !expected[index];
        setters[index](expected[index]);
        CollectionAssert.AreEqual(expected, Flags((OneWayAnovaAnalysisOptions)draft.Snapshot()));
        setters[index](!expected[index]);
        CollectionAssert.AreEqual(Flags(new()), Flags((OneWayAnovaAnalysisOptions)draft.Snapshot()));
    }

    [TestMethod]
    public void DunnRoundTripsWithoutAliasingCommonDefaults()
    {
        var options = new KruskalWallisAnalysisOptions();
        using var draft = new KruskalWallisOptionDraft(options);
        draft.IncludeDunnComparisons = false;
        Assert.IsFalse(((KruskalWallisAnalysisOptions)draft.Snapshot()).IncludeDunnComparisons);
        Assert.IsTrue(options.IncludeDunnComparisons);
        draft.IncludeDunnComparisons = true;
        Assert.IsTrue(((KruskalWallisAnalysisOptions)draft.Snapshot()).IncludeDunnComparisons);
    }

    [TestMethod]
    public void SwitchingResetsDraftsAndClosesOldEditorsAndReopenStartsFresh()
    {
        using var state = State();
        Select(state, AnalysisIds.OneWayAnova);
        var anova = (AnovaOptionDraft)state.Editor!.Draft;
        anova.SetAlpha(".12"); anova.IncludeWelch = true;
        Assert.IsTrue(state.Select(AnalysisIds.OneWayAnova));
        Assert.AreSame(anova, state.Editor.Draft);
        Select(state, AnalysisIds.KruskalWallis);
        Assert.IsFalse(anova.IsValid);
        Assert.ThrowsException<InvalidOperationException>(() => anova.Snapshot());
        var kw = (KruskalWallisOptionDraft)state.Editor!.Draft;
        Assert.AreEqual(.05, ((KruskalWallisAnalysisOptions)kw.Snapshot()).Alpha);
        kw.IncludeDunnComparisons = false;
        kw.SetAlpha("invalid");
        Select(state, AnalysisIds.OneWayAnova);
        Assert.AreNotSame(anova, state.Editor!.Draft);
        Assert.AreEqual(.05, ((OneWayAnovaAnalysisOptions)state.Editor.Draft.Snapshot()).Alpha);
        Assert.IsFalse(((AnovaOptionDraft)state.Editor.Draft).IncludeWelch);
        state.Dispose(); state.Dispose();
        Assert.IsFalse(state.Select(AnalysisIds.KruskalWallis));
        anova.SetAlpha(".8"); anova.IncludeWelch = false;
        Assert.AreEqual(".12", anova.AlphaText);
        Assert.IsTrue(anova.IncludeWelch);
        using var reopened = State();
        Assert.IsNull(reopened.Selected);
        Select(reopened, AnalysisIds.KruskalWallis);
        Assert.IsTrue(((KruskalWallisOptionDraft)reopened.Editor!.Draft).IncludeDunnComparisons);
    }

    private static bool[] Flags(OneWayAnovaAnalysisOptions o) =>
        [o.IncludeWelch, o.IncludeFisherLsd, o.IncludeBonferroni, o.IncludeTukeyKramer, o.IncludeGamesHowell];

    [TestMethod]
    public void DescriptorFactoryValuesAreUsedRatherThanUiDefaults()
    {
        var source = AnalysisCatalog.GetById(AnalysisIds.OneWayAnova);
        var descriptor = new AnalysisDescriptor(source.Id, source.DisplayName, source.Category, source.Description, source.HelpUrl,
            source.ContractVersion, source.InputSchemaId, source.OptionsSchemaId, source.InputTypes, source.OptionsType,
            () => new OneWayAnovaAnalysisOptions { Alpha = .125, IncludeGamesHowell = true, IncludeWelch = true },
            source.OutputKinds, source.Availability, source.EstimatedCost, source.RequiredCapabilities, source.LegacyKind);
        var catalog = new AnalysisNavigationCatalog(new[] { descriptor }, AnalysisHandlerRegistry.All.Where(h => h.AnalysisId == source.Id));
        using var state = new AnalysisNavigationState(catalog, new());
        Assert.IsTrue(Select(state, source.Id));
        var actual = (OneWayAnovaAnalysisOptions)state.Editor!.Draft.Snapshot();
        Assert.AreEqual(.125, actual.Alpha);
        Assert.IsTrue(actual.IncludeGamesHowell);
        Assert.IsTrue(actual.IncludeWelch);
    }

    [TestMethod]
    public void MenuRepresentsEveryGeneratedMethodAndPrimaryCategoryWithOnlyTwoSupportedEditors()
    {
        var catalog = new AnalysisNavigationCatalog();
        var menu = new AnalysisNavigationMenu(catalog, new());
        Assert.AreEqual(12, menu.Groups.Count);
        Assert.AreEqual(76, menu.Groups.Sum(g => g.Methods.Count));
        CollectionAssert.AreEqual(GeneratedMethodMetadata.All.Select(m => m.Category).Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal).ToArray(), menu.Groups.Select(g => g.Category).ToArray());
        foreach (var group in menu.Groups)
        {
            var expected = GeneratedMethodMetadata.All.Where(m => m.Category == group.Category)
                .OrderBy(m => m.DisplayName, StringComparer.Ordinal).ThenBy(m => m.Key, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(expected.Select(m => m.Key).ToArray(), group.Methods.Select(m => m.Key).ToArray());
            CollectionAssert.AreEqual(expected.Select(m => m.DisplayName).ToArray(), group.Methods.Select(m => m.DisplayName).ToArray());
            Assert.AreEqual(group.Methods.Count, group.Methods.Select(m => m.DisplayName).Distinct(StringComparer.Ordinal).Count());
        }
        CollectionAssert.AreEquivalent(new[] { AnalysisIds.OneWayAnova, AnalysisIds.KruskalWallis },
            menu.Groups.SelectMany(g => g.Methods).Where(m => m.CanEdit).Select(m => m.AnalysisId).ToArray());
        var reversed = new AnalysisNavigationMenu(catalog, new(), GeneratedMethodMetadata.All.Reverse());
        CollectionAssert.AreEqual(menu.Groups.SelectMany(g => g.Methods).Select(m => m.Key).ToArray(),
            reversed.Groups.SelectMany(g => g.Methods).Select(m => m.Key).ToArray());
    }

    [DataTestMethod]
    [DataRow("duplicate-key")] [DataRow("duplicate-id")] [DataRow("duplicate-label")]
    [DataRow("missing-key")] [DataRow("missing-category")] [DataRow("missing-name")] [DataRow("null-entry")]
    public void AmbiguousOrIncompleteMetadataIsAnExplicitCatalogDefect(string defect)
    {
        var source = GeneratedMethodMetadata.GetByAnalysisId(AnalysisIds.OneWayAnova);
        var broken = defect == "null-entry" ? null : new AnalysisMethodMetadata(
            defect == "duplicate-key" ? source.Key : defect == "missing-key" ? "" : "new-key",
            defect == "duplicate-id" ? source.AnalysisId : null,
            defect == "missing-name" ? "" : defect == "duplicate-label" ? source.DisplayName : "Other method",
            defect == "missing-category" ? " " : source.Category, source.Description, source.HelpUrl);
        Assert.ThrowsException<ArgumentException>(() =>
            new AnalysisNavigationMenu(new AnalysisNavigationCatalog(), new(), new[] { source, broken! }));
    }

    [DataTestMethod]
    [DataRow(null)] [DataRow("")] [DataRow("unknown.method")]
    public void NullableOrUnknownMetadataIdentityIsRepresentedButNeverSelectable(string? id)
    {
        var entry = new AnalysisMethodMetadata("future-method", id!, "Future method", "Future category", "Description", "https://example.test/help");
        var catalog = new AnalysisNavigationCatalog();
        var menu = new AnalysisNavigationMenu(catalog, new(), new[] { entry });
        Assert.IsFalse(menu.Groups.Single().Methods.Single().CanEdit);
        Assert.IsFalse(menu.Groups.Single().HasSupportedEditor);
        using var state = new AnalysisNavigationState(catalog, new(), menu);
        state.SelectGroup(entry.Category);
        Assert.IsFalse(state.Select(id));
        Assert.IsFalse(state.Select(menu.Groups.Single().Methods.Single().Value));
        Assert.IsNull(state.Editor);
    }

    [TestMethod]
    public void MissingMetadataAndWrongPrimaryCategoryCannotEnableVerifiedDescriptor()
    {
        var source = GeneratedMethodMetadata.GetByAnalysisId(AnalysisIds.OneWayAnova);
        var catalog = new AnalysisNavigationCatalog();
        using var missing = new AnalysisNavigationState(catalog, new(), new(catalog, new(), Array.Empty<AnalysisMethodMetadata>()));
        Assert.IsFalse(missing.SelectGroup(source.Category));
        Assert.IsFalse(missing.Select(source.AnalysisId));
        var mismatch = new AnalysisMethodMetadata(source.Key, source.AnalysisId, source.DisplayName, "Graphics", source.Description, source.HelpUrl);
        var menu = new AnalysisNavigationMenu(catalog, new(), new[] { mismatch });
        using var wrong = new AnalysisNavigationState(catalog, new(), menu);
        wrong.SelectGroup("Graphics");
        Assert.IsFalse(wrong.Select(source.AnalysisId));
        Assert.IsNull(wrong.Editor);
    }

    [DataTestMethod]
    [DataRow("Parametric", AnalysisIds.KruskalWallis)]
    [DataRow("Nonparametric", AnalysisIds.OneWayAnova)]
    [DataRow("Multivariate Analysis", AnalysisIds.OneWayAnova)]
    [DataRow("Graphics", "unavailable:BoxAndWhiskers")]
    [DataRow("Parametric", "anova.oneway ")]
    public void WrongGroupStaleAndUnavailableMethodSelectionsReject(string category, string id)
    {
        using var state = State();
        Select(state, AnalysisIds.OneWayAnova);
        var previous = state.Editor!.Draft;
        previous.SetAlpha("bad");
        Assert.IsTrue(state.SelectGroup(category));
        Assert.IsFalse(state.Select(id));
        Assert.IsFalse(previous.IsValid);
        Assert.IsNull(state.Selected);
        Assert.IsNull(state.Editor);
        Assert.AreEqual(AnalysisNavigationState.UnavailableReason, state.Rejection);
    }

    [DataTestMethod]
    [DataRow(null)] [DataRow("")] [DataRow("parametric")] [DataRow("Parametric ")] [DataRow("<unknown>")]
    public void InvalidGroupClearsOldSelectionAndClosesDraft(string? category)
    {
        using var state = State();
        Select(state, AnalysisIds.OneWayAnova);
        var draft = state.Editor!.Draft;
        Assert.IsFalse(state.SelectGroup(category));
        Assert.IsNull(state.SelectedGroup);
        Assert.IsNull(state.Selected);
        Assert.IsNull(state.Editor);
        Assert.IsFalse(draft.IsValid);
    }

    [TestMethod]
    public void SameGroupAndMethodPreserveInvalidDraftWhileChangedGroupAndPlaceholderDisposeIt()
    {
        using var state = State();
        Select(state, AnalysisIds.OneWayAnova);
        var draft = (AnovaOptionDraft)state.Editor!.Draft;
        draft.IncludeWelch = true; draft.SetAlpha("invalid");
        Assert.IsTrue(state.SelectGroup("Parametric"));
        Assert.IsTrue(state.Select(AnalysisIds.OneWayAnova));
        Assert.AreSame(draft, state.Editor!.Draft);
        Assert.AreEqual("invalid", state.Editor.Draft.AlphaText);
        state.SelectGroup("Nonparametric");
        Assert.IsNull(state.Editor);
        Assert.IsNull(state.Rejection);
        Assert.IsFalse(draft.IsValid);
        Select(state, AnalysisIds.OneWayAnova);
        var reset = (AnovaOptionDraft)state.Editor!.Draft;
        Assert.IsFalse(reset.IncludeWelch);
        Assert.AreEqual("0.05", reset.AlphaText);
        Assert.IsFalse(state.Select(""));
        Assert.IsFalse(reset.IsValid);
        Assert.IsNull(state.Editor);
        Assert.IsNull(state.Rejection);
        state.Dispose();
        Assert.IsFalse(state.SelectGroup("Parametric"));
        Assert.IsNull(state.SelectedGroup);
    }

    private sealed class NeverExecute(string id, Type input, Type options) : IAnalysisHandler
    {
        public string AnalysisId => id;
        public Type InputType => input;
        public Type OptionsType => options;
        public SpreadsheetAnalysisResult Execute(SpreadsheetAnalysisInput input, SpreadsheetAnalysisOptions options) =>
            throw new AssertFailedException("Option navigation must never execute an analysis.");
    }
}
