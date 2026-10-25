using BESHStat.OfficeAddin.Infrastructure;
using BESHStat.OfficeAddin.Services;
using BESHStatNG.AppInfrastructure;
using BESHStatNG.ExcelCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class OfficePreferenceTests
{
    internal static PreferenceSnapshot Snapshot(double alpha = .01, int digits = 4, PValueSmallValueDisplay mode = PValueSmallValueDisplay.ScientificNotation) =>
        PreferenceSnapshot.Normalize(new PreferenceData { DefaultAlpha = alpha, DefaultRandomSeed = 12345, TraceEnabled = false,
            PValues = new() { DecimalPlaces = digits, SmallValueDisplay = mode, UseUpperBound = false } }).Snapshot;

    internal sealed class Storage : IOfficePreferenceStorage
    {
        public PreferenceData? Saved;
        public bool? SavedForceEnglish = true;
        public string LoadStatus = "missing", SaveStatus = "saved";
        public bool Throw;
        public TaskCompletionSource<PreferenceStorageReply>? PendingLoad, PendingSave;
        public int Saves;
        public Task<PreferenceStorageReply> LoadAsync() => Throw ? throw new InvalidOperationException("PRIVATE") : PendingLoad?.Task ?? Task.FromResult(new PreferenceStorageReply { Status = Saved is null ? LoadStatus : "loaded", Data = Saved, ForceEnglish = SavedForceEnglish });
        public Task<PreferenceStorageReply> SaveAsync(PreferenceData data, bool? forceEnglish = null)
        {
            Saves++;
            if (Throw) throw new OperationCanceledException("PRIVATE");
            if (SaveStatus == "saved") { Saved = data; SavedForceEnglish = forceEnglish ?? SavedForceEnglish; }
            return PendingSave?.Task ?? Task.FromResult(new PreferenceStorageReply { Status = SaveStatus, ForceEnglish = SavedForceEnglish });
        }
    }
    internal sealed class Browser(string? locale = "en") : IOfficeWorkspaceBrowser
    {
        public string? Url, Copied;
        public string Result = "requested";
        public Task<string?> DisplayLocaleAsync() => Task.FromResult(locale);
        public Task<string> OpenHelpAsync(string url) { Url = url; return Task.FromResult(Result); }
        public Task<bool> CopyAsync(string log) { Copied = log; return Task.FromResult(true); }
    }

    [TestMethod]
    public void DefaultsMatchWindowsAndExportsAreDetached()
    {
        var p = PreferenceSnapshot.Defaults();
        Assert.AreEqual(.05, p.DefaultAlpha); Assert.AreEqual(int.MinValue, p.DefaultRandomSeed);
        Assert.IsTrue(p.TraceEnabled); Assert.AreEqual(8, p.DecimalPlaces); Assert.IsTrue(p.UseUpperBound);
        Assert.AreEqual(PValueSmallValueDisplay.LessThanThreshold, p.SmallValueDisplay);
        var draft = new GlobalOptionsDraft(); draft.Revert(p);
        Assert.AreEqual("", draft.Seed); Assert.AreEqual("0.05", draft.Alpha);
        p.ToData().PValues.DecimalPlaces = 2; Assert.AreEqual(8, p.DecimalPlaces);
        Assert.AreEqual(12345, Snapshot().CreateAnalysisDefaults().DefaultRandomSeed);
    }

    [DataTestMethod]
    [DataRow("0")] [DataRow("1")] [DataRow("NaN")] [DataRow("Infinity")] [DataRow("-Infinity")]
    [DataRow("0,05")] [DataRow("0.0,5")] [DataRow("1,000")] [DataRow("")]
    public void InvalidAlphaCannotBeSavedInAnyLocale(string alpha)
    {
        var draft = new GlobalOptionsDraft(); draft.Revert(PreferenceSnapshot.Defaults()); draft.Alpha = alpha;
        Assert.IsNull(draft.Validate()); Assert.AreEqual("AlphaInvalid", draft.Errors["alpha"]);
    }
    [DataTestMethod]
    [DataRow("", int.MinValue)] [DataRow("0", int.MinValue)] [DataRow("12345", 12345)]
    [DataRow("-2147483648", int.MinValue)] [DataRow("2147483647", int.MaxValue)] [DataRow("+42", 42)] [DataRow("-1", -1)]
    public void SeedUsesCoreNormalization(string seed, int expected)
    {
        var draft = new GlobalOptionsDraft(); draft.Revert(PreferenceSnapshot.Defaults()); draft.Seed = seed;
        Assert.AreEqual(expected, draft.Validate()!.DefaultRandomSeed);
    }
    [DataTestMethod]
    [DataRow("2147483648")] [DataRow("-2147483649")] [DataRow("1.5")] [DataRow("NaN")] [DataRow("1e3")] [DataRow("1,000")]
    public void InvalidSeedIsFieldValidation(string seed)
    {
        var draft = new GlobalOptionsDraft(); draft.Revert(PreferenceSnapshot.Defaults()); draft.Seed = seed;
        Assert.IsNull(draft.Validate()); Assert.IsTrue(draft.Errors.ContainsKey("seed"));
    }
    [DataTestMethod]
    [DataRow("2", true)] [DataRow("16", true)] [DataRow("1", false)] [DataRow("17", false)] [DataRow("2.5", false)]
    public void DigitsUseCoreBounds(string digits, bool valid)
    {
        var draft = new GlobalOptionsDraft(); draft.Revert(PreferenceSnapshot.Defaults()); draft.Digits = digits;
        Assert.AreEqual(valid, draft.Validate() is not null);
    }

    [TestMethod]
    public async Task PersistentSaveReopenAndSessionFallbackAreTruthful()
    {
        var storage = new Storage(); var first = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        await first.InitializeAsync(); Assert.AreEqual(PreferencePersistence.SessionOnly, first.State.Persistence);
        Assert.IsTrue(await first.SaveAsync(Snapshot())); Assert.AreEqual(1L, first.Revision);
        Assert.AreEqual(PreferencePersistence.Persisted, first.State.Persistence);
        var second = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await second.InitializeAsync();
        Assert.AreEqual(.01, second.Effective.DefaultAlpha); Assert.AreEqual(4, second.Effective.DecimalPlaces);
        storage.SaveStatus = "unavailable";
        Assert.IsFalse(await second.SaveAsync(Snapshot(.02))); Assert.AreEqual(.01, second.Effective.DefaultAlpha); Assert.AreEqual(0L, second.Revision);
        Assert.IsTrue(await second.SaveAsync(Snapshot(.03), sessionOnly: true));
        Assert.AreEqual(.03, second.Effective.DefaultAlpha); Assert.AreEqual(PreferencePersistence.SessionOnly, second.State.Persistence);
        var third = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await third.InitializeAsync(); Assert.AreEqual(.01, third.Effective.DefaultAlpha);
    }

    [DataTestMethod]
    [DataRow("unsupported", PreferenceStatus.UnsupportedVersion)] [DataRow("invalid", PreferenceStatus.InvalidData)]
    [DataRow("unavailable", PreferenceStatus.StorageUnavailable)]
    public async Task StorageFailuresRetainLastGoodAndAllowExplicitSessionOnly(string status, PreferenceStatus expected)
    {
        var storage = new Storage(); var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        await owner.InitializeAsync(); await owner.SaveAsync(Snapshot()); storage.SaveStatus = status;
        Assert.IsFalse(await owner.SaveAsync(Snapshot(.02))); Assert.AreEqual(.01, owner.Effective.DefaultAlpha);
        Assert.AreEqual(expected, owner.State.Status); Assert.AreEqual(1L, owner.Revision);
        Assert.IsTrue(await owner.SaveAsync(Snapshot(.02), true)); Assert.AreEqual(.02, owner.Effective.DefaultAlpha);
    }
    [TestMethod]
    public async Task ThrowingAndCancelledStorageDoesNotReplaceEffectiveValues()
    {
        var storage = new Storage(); var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        await owner.SaveAsync(Snapshot()); storage.Throw = true;
        Assert.IsFalse(await owner.SaveAsync(Snapshot(.02))); Assert.AreEqual(.01, owner.Effective.DefaultAlpha);
    }
    [TestMethod]
    public async Task MissingFieldsNormalizeThroughCommonWithoutClaimingPersistence()
    {
        var storage = new Storage { Saved = new() { DefaultAlpha = .01 } };
        var result = await new OfficePreferenceStore(storage).LoadAsync();
        Assert.AreEqual(.01, result.Snapshot.DefaultAlpha); Assert.AreEqual(PreferenceStatus.Normalized, result.Status);
        Assert.AreEqual(PreferencePersistence.SessionOnly, result.Persistence);
    }
    [TestMethod]
    public async Task LateLoadCannotSupersedeExplicitSaveAndDoubleSaveIsRejected()
    {
        var storage = new Storage { PendingLoad = new() };
        var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        var init = owner.InitializeAsync(); var save = owner.SaveAsync(Snapshot(.02));
        Assert.AreEqual(0, storage.Saves);
        storage.PendingLoad.SetResult(new() { Status = "loaded", Data = Snapshot(.01).ToData() });
        await init; Assert.IsTrue(await save); Assert.AreEqual(.02, owner.Effective.DefaultAlpha);
        storage.PendingSave = new(); var pending = owner.SaveAsync(Snapshot(.03));
        Assert.IsTrue(owner.Applying); Assert.IsFalse(await owner.SaveAsync(Snapshot(.04)));
        storage.PendingSave.SetResult(new() { Status = "saved" }); Assert.IsTrue(await pending);
        Assert.AreEqual(.03, owner.Effective.DefaultAlpha); Assert.AreEqual(2L, owner.Revision);
    }

    [DataTestMethod]
    [DataRow("sk-SK", true, "Options")] [DataRow("sk", true, "Options")] [DataRow("en-US", true, "Options")]
    [DataRow("de-DE", true, "Options")] [DataRow("fr-FR", true, "Options")] [DataRow(null, true, "Options")]
    [DataRow("sk-SK", false, "Možnosti")] [DataRow("sk", false, "Možnosti")] [DataRow("en-US", false, "Options")]
    [DataRow("de-DE", false, "Options")] [DataRow(null, false, "Options")]
    public async Task ResourceLocaleFallsBackDeterministically(string? locale, bool forceEnglish, string expected)
    {
        using var preferences = new OfficePreferenceSession(new OfficePreferenceStore(new Storage { Saved = Snapshot().ToData(), SavedForceEnglish = forceEnglish }));
        using var text = new OfficeStrings(new Browser(locale), preferences); await text.InitializeAsync(); Assert.AreEqual(expected, text["Options"]);
        Assert.AreEqual(expected == "Možnosti" ? "Analýza" : "Analysis", text["Analysis"]);
        Assert.AreEqual(expected == "Možnosti" ? "Uložiť" : "Save", text["Save"]);
        StringAssert.Contains(text["Uncertain"], expected == "Možnosti" ? "Excel mohol" : "Excel may");
        Assert.AreNotEqual("Uncertain", text["Uncertain"]);
        Assert.IsTrue(typeof(OfficeStrings).Assembly.GetManifestResourceNames().Contains("BESHStat.OfficeAddin.Resources.Ui.sk.resources"));
    }
    [TestMethod]
    public async Task FoundationOwnsSingleScopedSessionStoreAndDiagnosticSink()
    {
        var registrations = new ServiceCollection().AddOfficeFoundation().AddOfficeFoundation();
        Assert.AreEqual(1, registrations.Count(d => d.ServiceType == typeof(OfficePreferenceSession)));
        await using var provider = registrations.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using var first = provider.CreateAsyncScope(); await using var second = provider.CreateAsyncScope();
        Assert.AreSame(first.ServiceProvider.GetRequiredService<OfficePreferenceSession>(), first.ServiceProvider.GetRequiredService<OfficePreferenceSession>());
        Assert.AreNotSame(first.ServiceProvider.GetRequiredService<OfficePreferenceSession>(), second.ServiceProvider.GetRequiredService<OfficePreferenceSession>());
        Assert.AreSame(first.ServiceProvider.GetRequiredService<OfficeDiagnosticLog>(), first.ServiceProvider.GetRequiredService<IDiagnosticSink>());
    }
}
