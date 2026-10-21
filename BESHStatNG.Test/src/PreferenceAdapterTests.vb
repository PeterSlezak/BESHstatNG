Option Explicit On
Option Strict On

Imports System.IO
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PreferenceAdapterTests
    Private _directory As String
    Private _store As BeshStatNgSettingsStore
    Private _previousSettings As BeshStatNgSettings
    Private _previousStore As BeshStatNgSettingsStore
    Private _previousRuntime As AnalysisDefaultsOptions
    Private _previousTrace As Boolean
    Private _previousState As PreferenceResult

    <TestInitialize>
    Public Sub Initialize()
        _directory = Path.Combine(Path.GetTempPath(), "BESHStat_Preferences_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(_directory)
        _store = New BeshStatNgSettingsStore(_directory)
        _previousSettings = AppGlobals.gSettings
        _previousStore = AppGlobals.gSettingsStore
        _previousRuntime = CoreServices.AnalysisDefaults
        _previousTrace = CoreServices.TraceExecutionLoggingEnabled
        _previousState = AppGlobals.PreferenceState
    End Sub

    <TestCleanup>
    Public Sub Cleanup()
        AppGlobals.gSettings = _previousSettings
        AppGlobals.gSettingsStore = _previousStore
        AppGlobals.PreferenceState = _previousState
        CoreServices.AnalysisDefaults = _previousRuntime
        CoreServices.TraceExecutionLoggingEnabled = _previousTrace
        Directory.Delete(_directory, True)
    End Sub

    <TestMethod>
    Public Sub MissingFile_KeepsXllAdjacentFilenameAndCreatesHistoricalDefaults()
        Assert.AreEqual(Path.Combine(_directory, "BESHStatNG.settings.xml"), _store.SettingsPath)
        Dim loaded = New LegacyPreferenceStore(_store).Load()
        Assert.AreEqual(PreferenceStatus.Defaulted, loaded.Status)
        Assert.AreEqual(PreferencePersistence.Persisted, loaded.Persistence)
        Assert.AreEqual(0.05, loaded.Snapshot.DefaultAlpha)
        Assert.AreEqual(Integer.MinValue, loaded.Snapshot.DefaultRandomSeed)
        Assert.IsTrue(File.ReadAllText(_store.SettingsPath).Contains("version=""3"""))
        Assert.AreEqual(1, Directory.GetFiles(_directory).Length)
    End Sub

    ' Source-backed fixtures: XmlSerializer uses constructor defaults for omitted
    ' fields, then EnsureDefaults upgrades any historical version below 3.
    <DataTestMethod>
    <DataRow(1)>
    <DataRow(2)>
    <DataRow(3)>
    Public Sub HistoricalXml_UsesExistingSerializerAndMigrationIsIdempotent(version As Integer)
        Dim xml = "<BeshStatNgSettings version=""" & version.ToString() & """><DefaultAlpha>0.01</DefaultAlpha>" &
            "<DefaultRandomSeed>0</DefaultRandomSeed><PValuePresentation><DecimalPlaces>6</DecimalPlaces>" &
            "<SmallValueDisplay>ScientificNotation</SmallValueDisplay><UseUpperBound>false</UseUpperBound>" &
            "</PValuePresentation></BeshStatNgSettings>"
        File.WriteAllText(_store.SettingsPath, xml)
        Dim host As New LegacyPreferenceStore(_store)
        Dim first = host.Load()
        Assert.AreEqual(If(version < 3, PreferenceStatus.Migrated, PreferenceStatus.Normalized), first.Status)
        Assert.AreEqual(0.01, first.Snapshot.DefaultAlpha)
        Assert.AreEqual(Integer.MinValue, first.Snapshot.DefaultRandomSeed)
        Assert.AreEqual(6, first.Snapshot.DecimalPlaces)
        Assert.AreEqual(PValueSmallValueDisplay.ScientificNotation, first.Snapshot.SmallValueDisplay)
        Assert.IsFalse(first.Snapshot.UseUpperBound)
        Assert.IsTrue(first.Snapshot.TraceEnabled)
        Assert.AreEqual(xml, File.ReadAllText(_store.SettingsPath)) ' Reads do not rewrite historical data.
        Dim save = host.Save(first.Snapshot)
        Assert.AreEqual(PreferenceStatus.Saved, save.Status)
        Dim migratedXml = File.ReadAllText(_store.SettingsPath)
        Dim second = host.Load()
        Assert.AreEqual(PreferenceStatus.Loaded, second.Status)
        Assert.AreEqual(PreferencePersistence.Persisted, second.Persistence)
        Assert.AreEqual(first.Snapshot.DefaultAlpha, second.Snapshot.DefaultAlpha)
        Assert.AreEqual(migratedXml, File.ReadAllText(_store.SettingsPath))
    End Sub

    <TestMethod>
    Public Sub V3Roundtrip_SaveApplyAndFreshSessionAgreeWithoutExcel()
        AppGlobals.gSettingsStore = _store
        AppGlobals.LoadPreferences()
        Dim candidate = LegacyPreferenceStore.ToLegacy(PreferenceSnapshot.Defaults())
        candidate.DefaultAlpha = 0.01
        candidate.DefaultRandomSeed = 12345
        candidate.Diagnostics.TraceExecutionLoggingEnabled = False
        candidate.PValuePresentation.DecimalPlaces = 6
        candidate.PValuePresentation.SmallValueDisplay = PValueSmallValueDisplay.ScientificNotation
        candidate.PValuePresentation.UseUpperBound = False
        Dim saved = AppGlobals.SavePreferences(candidate)
        Assert.AreEqual(PreferenceStatus.Saved, saved.Status)
        candidate.DefaultAlpha = 0.3
        candidate.PValuePresentation.DecimalPlaces = 2
        AssertEffective()
        Assert.AreEqual(0.01D, AppGlobals.GetDefaultAlphaDecimal(0.0001D, 0.9999D)) ' UibyID initialization path.
        CoreServices.AnalysisDefaults = New AnalysisDefaultsOptions(0.2, 777)
        AppGlobals.gSettings = Nothing
        AppGlobals.LoadPreferences()
        AssertEffective()
        Dim fresh = New LegacyPreferenceStore(New BeshStatNgSettingsStore(_directory)).Load()
        Assert.AreEqual(PreferenceStatus.Loaded, fresh.Status)
        Assert.AreEqual(3, _store.Load().Version)
        Assert.AreEqual(0.01, fresh.Snapshot.CreateAnovaOptions().Alpha)
        Assert.AreEqual(0.01, fresh.Snapshot.CreateKruskalWallisOptions().Alpha)
    End Sub

    Private Shared Sub AssertEffective()
        Assert.AreEqual(0.01, AppGlobals.DefaultAlpha)
        Assert.AreEqual(12345, AppGlobals.DefaultRandomSeed)
        Assert.IsFalse(AppGlobals.TraceExecutionLoggingEnabled)
        Assert.IsFalse(CoreServices.TraceExecutionLoggingEnabled)
        Assert.AreEqual(6, AppGlobals.PValueDecimalPlaces)
        Assert.AreEqual(PValueSmallValueDisplay.ScientificNotation, AppGlobals.PValueSmallValueDisplay)
        Assert.IsFalse(AppGlobals.PValueUseUpperBound)
    End Sub

    <TestMethod>
    Public Sub FailedSave_AndDetachedCancelledEdit_LeaveSessionAndXmlUnchanged()
        AppGlobals.gSettingsStore = _store
        AppGlobals.LoadPreferences()
        Dim original = File.ReadAllText(_store.SettingsPath)
        Dim candidate = LegacyPreferenceStore.ToLegacy(LegacyPreferenceStore.FromLegacy(AppGlobals.GetCurrentSettings()).Snapshot)
        candidate.DefaultAlpha = 0.01
        candidate.PValuePresentation.DecimalPlaces = 6
        Assert.AreEqual(0.05, AppGlobals.DefaultAlpha) ' Editing/cancelling detached candidate.
        Using blocked As New FileStream(_store.SettingsPath & ".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.None)
            Dim result = AppGlobals.SavePreferences(candidate)
            Assert.AreEqual(PreferenceStatus.StorageUnavailable, result.Status)
            Assert.AreEqual(PreferencePersistence.Unavailable, result.Persistence)
        End Using
        Assert.AreEqual(0.05, AppGlobals.DefaultAlpha)
        Assert.AreEqual(8, AppGlobals.PValueDecimalPlaces)
        Assert.AreEqual(original, File.ReadAllText(_store.SettingsPath))
    End Sub

    <DataTestMethod>
    <DataRow(4)>
    <DataRow(0)>
    Public Sub UnsupportedXml_ReadAndSavePreserveExactBytes(version As Integer)
        Dim xml = "<BeshStatNgSettings version=""" & version.ToString() & """><FutureRequired>preserve</FutureRequired></BeshStatNgSettings>"
        File.WriteAllText(_store.SettingsPath, xml)
        Dim bytes = File.ReadAllBytes(_store.SettingsPath)
        AppGlobals.gSettingsStore = _store
        AppGlobals.LoadPreferences()
        Assert.AreEqual(PreferenceStatus.UnsupportedVersion, AppGlobals.PreferenceState.Status)
        Assert.AreEqual(0.05, AppGlobals.DefaultAlpha)
        Dim result = AppGlobals.SavePreferences(New BeshStatNgSettings With {.DefaultAlpha = 0.01})
        Assert.AreEqual(PreferenceStatus.UnsupportedVersion, result.Status)
        CollectionAssert.AreEqual(bytes, File.ReadAllBytes(_store.SettingsPath))
        Assert.IsFalse(File.Exists(_store.SettingsPath & ".tmp"))
    End Sub

    <TestMethod>
    Public Sub UnrepresentableVersion_AndFutureVersionWithMalformedBody_ArePreserved()
        Dim host As New LegacyPreferenceStore(_store)
        For Each xml As String In {"<BeshStatNgSettings version=""999999999999999"" />", "<BeshStatNgSettings version=""4""><broken"}
            File.WriteAllText(_store.SettingsPath, xml)
            Assert.AreEqual(PreferenceStatus.UnsupportedVersion, host.Load().Status)
            Assert.AreEqual(PreferenceStatus.UnsupportedVersion, host.Save(PreferenceSnapshot.Defaults()).Status)
            Assert.AreEqual(xml, File.ReadAllText(_store.SettingsPath))
        Next
    End Sub

    <TestMethod>
    Public Sub MalformedXml_RetainsLegacyRecovery_AndInaccessibleStoreIsTruthful()
        Dim host As New LegacyPreferenceStore(_store)
        For Each malformed In {"<broken", "<BeshStatNgSettings version=""3""><DefaultAlpha>invalid</DefaultAlpha></BeshStatNgSettings>"}
            File.WriteAllText(_store.SettingsPath, malformed)
            Dim recovered = host.Load()
            Assert.AreEqual(PreferenceStatus.Defaulted, recovered.Status)
            Assert.AreEqual(PreferencePersistence.Persisted, recovered.Persistence)
            Assert.AreEqual(0.05, recovered.Snapshot.DefaultAlpha)
        Next
        ' An existing file in place of a directory cannot be used as a store.
        Dim unavailable = New LegacyPreferenceStore(New BeshStatNgSettingsStore(_store.SettingsPath)).Load()
        Assert.AreEqual(PreferenceStatus.StorageUnavailable, unavailable.Status)
        Assert.AreEqual(PreferencePersistence.Unavailable, unavailable.Persistence)
    End Sub

    <TestMethod>
    Public Sub OldXmlWithOmittedDisplayAndDiagnostics_UsesSerializerConstructorDefaults()
        For Each version In {1, 2}
            File.WriteAllText(_store.SettingsPath, "<BeshStatNgSettings version=""" & version.ToString() & """><DefaultAlpha>0.01</DefaultAlpha></BeshStatNgSettings>")
            Dim result = New LegacyPreferenceStore(_store).Load()
            Assert.AreEqual(PreferenceStatus.Migrated, result.Status)
            Assert.AreEqual(0.01, result.Snapshot.DefaultAlpha)
            Assert.AreEqual(Integer.MinValue, result.Snapshot.DefaultRandomSeed)
            Assert.AreEqual(8, result.Snapshot.DecimalPlaces)
            Assert.AreEqual(PValueSmallValueDisplay.LessThanThreshold, result.Snapshot.SmallValueDisplay)
            Assert.IsTrue(result.Snapshot.TraceEnabled)
            Assert.IsTrue(result.Snapshot.UseUpperBound)
        Next
    End Sub

    <TestMethod>
    Public Sub NaNXml_IsSafeInSessionAndNotRewrittenOnRead()
        Dim xml = "<BeshStatNgSettings version=""3""><DefaultAlpha>NaN</DefaultAlpha></BeshStatNgSettings>"
        File.WriteAllText(_store.SettingsPath, xml)
        AppGlobals.gSettingsStore = _store
        AppGlobals.LoadPreferences()
        Assert.AreEqual(PreferenceStatus.Normalized, AppGlobals.PreferenceState.Status)
        Assert.AreEqual(PreferencePersistence.SessionOnly, AppGlobals.PreferenceState.Persistence)
        Assert.AreEqual(0.05, AppGlobals.DefaultAlpha)
        Assert.AreEqual(xml, File.ReadAllText(_store.SettingsPath))
    End Sub

    <TestMethod>
    Public Sub NonfiniteAndMissingNestedLegacySettings_NormalizeWithoutMutatingCaller()
        Dim legacy As New BeshStatNgSettings With {.DefaultAlpha = Double.NaN, .DefaultRandomSeed = 0,
            .Diagnostics = Nothing, .PValuePresentation = Nothing}
        legacy.EnsureDefaults()
        Assert.IsTrue(Double.IsNaN(legacy.DefaultAlpha)) ' Historical policy remains unchanged.
        legacy.Diagnostics = Nothing
        legacy.PValuePresentation = Nothing
        Dim result = LegacyPreferenceStore.FromLegacy(legacy)
        Assert.AreEqual(PreferenceStatus.Normalized, result.Status)
        Assert.AreEqual(0.05, result.Snapshot.DefaultAlpha)
        Assert.AreEqual(Integer.MinValue, result.Snapshot.DefaultRandomSeed)
        Assert.IsTrue(result.Snapshot.TraceEnabled)
        Assert.IsTrue(result.Snapshot.UseUpperBound)
        Assert.AreEqual(8, result.Snapshot.DecimalPlaces)
        Assert.IsTrue(Double.IsNaN(legacy.DefaultAlpha))
        Assert.IsNull(legacy.Diagnostics)
        AppGlobals.ApplySettings(legacy)
        Assert.AreEqual(0.05, AppGlobals.DefaultAlpha)
        Assert.IsFalse(Double.IsNaN(AppGlobals.GetCurrentSettings().DefaultAlpha))
    End Sub
End Class
