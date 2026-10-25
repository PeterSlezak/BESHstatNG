using BESHStat.OfficeAddin.Services;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BESHStat.OfficeAddin.Tests.OfficePreferenceTests;

namespace BESHStat.OfficeAddin.Tests;

[TestClass]
public sealed class ForceEnglishTests
{
    [DataTestMethod]
    [DataRow("sk-SK")] [DataRow("de-DE")] [DataRow("en-US")] [DataRow("fr-FR")]
    public async Task NoSavedFlagAlwaysStartsEnglishAndKeepsDetectedCulture(string locale)
    {
        using var owner = new OfficePreferenceSession(new OfficePreferenceStore(new Storage()));
        using var text = new OfficeStrings(new Browser(locale), owner); await text.InitializeAsync();
        Assert.IsTrue(owner.ForceEnglish); Assert.AreEqual("Options", text["Options"]); Assert.AreEqual(locale, text.DetectedCulture.Name);
    }

    [DataTestMethod]
    [DataRow("invalid")] [DataRow("unsupported")] [DataRow("unavailable")]
    public async Task UnusableSavedDataCannotSelectHostLanguage(string status)
    {
        using var owner = new OfficePreferenceSession(new OfficePreferenceStore(new Storage { LoadStatus = status, SavedForceEnglish = false }));
        using var text = new OfficeStrings(new Browser("sk-SK"), owner); await text.InitializeAsync();
        Assert.IsTrue(owner.ForceEnglish); Assert.AreEqual("Options", text["Options"]);
    }

    [TestMethod]
    public async Task LegacySnapshotKeepsEveryCommonValueAndForceEnglishDefaultsOn()
    {
        var storage = new Storage { Saved = Snapshot().ToData() }; using var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        await owner.InitializeAsync(); Assert.IsTrue(owner.ForceEnglish);
        Assert.AreEqual(.01, owner.Effective.DefaultAlpha); Assert.AreEqual(12345, owner.Effective.DefaultRandomSeed);
        Assert.AreEqual(4, owner.Effective.DecimalPlaces); Assert.IsFalse(owner.Effective.TraceEnabled); Assert.IsFalse(owner.Effective.UseUpperBound);
        Assert.AreEqual(Snapshot().SmallValueDisplay, owner.Effective.SmallValueDisplay); Assert.AreEqual(0, storage.Saves);
    }

    [TestMethod]
    public async Task SavedFalseAndTrueReloadAndSessionOnlyDoesNotChangeDurableFlag()
    {
        var storage = new Storage(); using var first = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        Assert.IsTrue(await first.SaveAsync(Snapshot(), forceEnglish: false)); Assert.IsFalse(first.ForceEnglish);
        using var second = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await second.InitializeAsync(); Assert.IsFalse(second.ForceEnglish);
        Assert.IsTrue(await second.SaveAsync(Snapshot(), forceEnglish: true));
        using var third = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await third.InitializeAsync(); Assert.IsTrue(third.ForceEnglish);
        var saves = storage.Saves;
        Assert.IsTrue(await third.SaveAsync(Snapshot(.02), sessionOnly: true, forceEnglish: false)); Assert.IsFalse(third.ForceEnglish);
        Assert.AreEqual(PreferencePersistence.SessionOnly, third.State.Persistence); Assert.AreEqual(saves, storage.Saves);
        using var reopened = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await reopened.InitializeAsync();
        Assert.IsTrue(reopened.ForceEnglish); Assert.AreEqual(.01, reopened.Effective.DefaultAlpha);
    }

    [TestMethod]
    public async Task RetirementFailureBeforePersistenceCannotLeakFailedLanguageOrAlphaOnReload()
    {
        var storage = new Storage { Saved = Snapshot().ToData() }; using var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        using var text = new OfficeStrings(new Browser("sk-SK"), owner); await text.InitializeAsync(); var notifications = 0; text.Changed += () => notifications++;
        using var participant = owner.Participate(() => true, () => throw new InvalidOperationException("PRIVATE"));
        Assert.IsFalse(await owner.SaveAsync(Snapshot(.02), forceEnglish: false));
        Assert.AreEqual(0, storage.Saves); Assert.IsTrue(owner.ForceEnglish); Assert.AreEqual(.01, owner.Effective.DefaultAlpha);
        Assert.AreEqual(PreferenceStatus.Failed, owner.State.Status); Assert.AreEqual(0L, owner.Revision); Assert.AreEqual(0, notifications);
        using var reopened = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await reopened.InitializeAsync();
        Assert.IsTrue(reopened.ForceEnglish); Assert.AreEqual(.01, reopened.Effective.DefaultAlpha);
    }

    [TestMethod]
    public async Task PersistenceFailureRetainsLanguageAndExplicitSessionChoiceStillWorks()
    {
        var storage = new Storage { SaveStatus = "unavailable" }; using var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        using var text = new OfficeStrings(new Browser("sk-SK"), owner); await text.InitializeAsync();
        Assert.IsFalse(await owner.SaveAsync(Snapshot(), forceEnglish: false)); Assert.AreEqual("Options", text["Options"]);
        Assert.IsTrue(await owner.SaveAsync(Snapshot(), true, false)); Assert.AreEqual("Možnosti", text["Options"]);
        Assert.IsNull(storage.Saved); Assert.IsTrue(storage.SavedForceEnglish);
    }

    [TestMethod]
    public async Task RetireThenPersistIsSerializedAndLanguageChangesOnceAfterSuccess()
    {
        var storage = new Storage { PendingSave = new() }; using var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        using var text = new OfficeStrings(new Browser("sk-SK"), owner); await text.InitializeAsync();
        var retiring = new TaskCompletionSource(); using var participant = owner.Participate(() => true, () => retiring.Task);
        var notifications = 0; text.Changed += () => throw new Exception("PRIVATE"); text.Changed += () => notifications++;
        var save = owner.SaveAsync(Snapshot(), forceEnglish: false);
        Assert.IsTrue(owner.Applying); Assert.AreEqual(0, storage.Saves); Assert.AreEqual("Options", text["Options"]);
        Assert.IsFalse(await owner.SaveAsync(Snapshot(.02), forceEnglish: true)); retiring.SetResult();
        await Task.Yield(); Assert.AreEqual("Options", text["Options"]);
        storage.PendingSave.SetResult(new() { Status = "saved", ForceEnglish = false }); Assert.IsTrue(await save);
        Assert.AreEqual("Možnosti", text["Options"]); Assert.AreEqual(1L, owner.Revision); Assert.AreEqual(1, notifications);
    }

    [TestMethod]
    public async Task ClosingDuringRetirementPreventsPersistenceAndClosingAfterSubmissionDoesNotReportFalseFailure()
    {
        var storage = new Storage(); var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await owner.InitializeAsync();
        var retiring = new TaskCompletionSource(); using var participant = owner.Participate(() => true, () => retiring.Task);
        var save = owner.SaveAsync(Snapshot(), forceEnglish: false); owner.Dispose(); retiring.SetResult();
        Assert.IsFalse(await save); Assert.AreEqual(0, storage.Saves); Assert.IsTrue(owner.ForceEnglish);

        storage.PendingSave = new(); using var second = new OfficePreferenceSession(new OfficePreferenceStore(storage));
        using var text = new OfficeStrings(new Browser("sk-SK"), second); await text.InitializeAsync(); var changes = 0; text.Changed += () => changes++;
        var submitted = second.SaveAsync(Snapshot(), forceEnglish: false); second.Dispose(); text.Dispose();
        storage.PendingSave.SetResult(new() { Status = "saved", ForceEnglish = false });
        Assert.IsTrue(await submitted); Assert.AreEqual(0, changes); Assert.IsFalse(second.ForceEnglish);
        using var reopened = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await reopened.InitializeAsync(); Assert.IsFalse(reopened.ForceEnglish);
    }

    [TestMethod]
    public async Task InvalidHostFlagReplyFailsClosedWithoutTouchingStoredData()
    {
        var storage = new Storage { Saved = Snapshot().ToData(), SavedForceEnglish = null };
        using var owner = new OfficePreferenceSession(new OfficePreferenceStore(storage)); await owner.InitializeAsync();
        Assert.IsTrue(owner.ForceEnglish); Assert.AreEqual(PreferenceStatus.InvalidData, owner.State.Status); Assert.AreEqual(0, storage.Saves);
    }

    [DataTestMethod]
    [DataRow("anova-all-posthoc")] [DataRow("kw-dunn")]
    public async Task LanguageOnlySaveRetiresConsentAndKeepsNumericValuesFormatsAndSignificance(string fixtureName)
    {
        using var fixture = AnalysisPilotTests.Fixture(fixtureName); using var draft = AnalysisPilotTests.Draft(fixture.RootElement);
        using var owner = new OfficePreferenceSession(new OfficePreferenceStore(new Storage())); await owner.InitializeAsync();
        var workbook = new AnalysisPilotTests.Workbook(AnalysisPilotTests.Range(fixture.RootElement)); var host = new AnalysisPilotTests.Host();
        await using var state = new AnalysisPilotState(AnalysisCatalog.GetById(fixture.RootElement.GetProperty("analysisIdProposed").GetString()!), draft, workbook, host, preferenceSession: owner);
        state.InputReference.Edit("'Input'!A1:C4"); await state.RunAsync(); await state.WriteResultsAsync(); Assert.AreEqual(1, workbook.Writes);
        var original = workbook.Plan!.Tables.SelectMany(t => t.Cells).Where(c => c.IsPValue)
            .Select(c => (c.Value.Kind, c.Value.NumberValue, c.IsSignificant, c.NumberFormat)).ToArray();
        Assert.IsTrue(original.Length > 0);
        host.Occupancy = OutputOccupancy.Occupied; await state.RunAsync(); await state.WriteResultsAsync(); var oldConsent = state.Inspection;
        Assert.IsNotNull(oldConsent); Assert.IsTrue(await owner.SaveAsync(owner.Effective, forceEnglish: false));
        Assert.IsNull(state.Inspection); await state.WriteAsync(true); Assert.AreEqual(1, workbook.Writes);
        await state.RunAsync(); await state.WriteResultsAsync(); Assert.AreNotSame(oldConsent, state.Inspection); Assert.IsTrue(state.Inspection!.ConfirmationRequired);
        await state.WriteAsync(true); Assert.AreEqual(2, workbook.Writes); Assert.AreEqual(OutputWriteState.Completed, state.WriteOutcome!.State);
        var after = workbook.Plan!.Tables.SelectMany(t => t.Cells).Where(c => c.IsPValue)
            .Select(c => (c.Value.Kind, c.Value.NumberValue, c.IsSignificant, c.NumberFormat)).ToArray();
        CollectionAssert.AreEqual(original, after); Assert.AreEqual(.05, owner.Effective.DefaultAlpha); Assert.AreEqual(3, workbook.Reads);
    }
}
