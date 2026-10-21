Option Explicit On
Option Strict On

Imports System.Linq
Imports System.Threading.Tasks
Imports BESHStatNG
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PreferenceTests
    <TestMethod>
    Public Sub Defaults_AgreeWithCoreAndKeepVersionBoundariesSeparate()
        Dim snapshot = PreferenceSnapshot.Defaults()
        Dim legacy As New BeshStatNgSettings()
        Assert.AreEqual(1, snapshot.SchemaVersion)
        Assert.AreEqual(3, legacy.Version)
        Assert.AreEqual(legacy.DefaultAlpha, snapshot.DefaultAlpha)
        Assert.AreEqual(legacy.DefaultRandomSeed, snapshot.DefaultRandomSeed)
        Assert.AreEqual(legacy.Diagnostics.TraceExecutionLoggingEnabled, snapshot.TraceEnabled)
        Assert.AreEqual(legacy.PValuePresentation.DecimalPlaces, snapshot.DecimalPlaces)
        Assert.AreEqual(legacy.PValuePresentation.SmallValueDisplay, snapshot.SmallValueDisplay)
        Assert.AreEqual(legacy.PValuePresentation.UseUpperBound, snapshot.UseUpperBound)
        Assert.AreEqual(0, CInt(PValueSmallValueDisplay.LessThanThreshold))
        Assert.AreEqual(1, CInt(PValueSmallValueDisplay.ScientificNotation))
        Assert.AreEqual(2, CInt(PValueSmallValueDisplay.FixedDecimal))
        Assert.AreEqual(PreferenceStatus.Defaulted, PreferenceSnapshot.Normalize(Nothing).Status)
    End Sub

    <DataTestMethod>
    <DataRow(0.0)>
    <DataRow(1.0)>
    <DataRow(-0.01)>
    <DataRow(1.01)>
    <DataRow(Double.NaN)>
    <DataRow(Double.PositiveInfinity)>
    <DataRow(Double.NegativeInfinity)>
    Public Sub InvalidAlpha_UsesFiniteCorePolicy(alpha As Double)
        Dim data = PreferenceSnapshot.Defaults().ToData()
        data.DefaultAlpha = alpha
        Dim result = PreferenceSnapshot.Normalize(data)
        Assert.AreEqual(PreferenceStatus.Normalized, result.Status)
        Assert.AreEqual(0.05, result.Snapshot.DefaultAlpha)
        Assert.AreEqual(New AnalysisDefaultsOptions(alpha).DefaultAlpha, result.Snapshot.DefaultAlpha)
        Assert.IsTrue(result.Codes.Contains("preferences.alpha.defaulted"))
        Assert.IsTrue(Double.IsNaN(alpha) AndAlso Double.IsNaN(data.DefaultAlpha.Value) OrElse alpha = data.DefaultAlpha.Value)
    End Sub

    <TestMethod>
    Public Sub FiniteAlphaEdges_AndExplicitOverridesAreRetained()
        For Each alpha In {Double.Epsilon, Math.BitDecrement(1.0), 0.01}
            Dim data = PreferenceSnapshot.Defaults().ToData()
            data.DefaultAlpha = alpha
            Dim result = PreferenceSnapshot.Normalize(data)
            Assert.AreEqual(PreferenceStatus.Loaded, result.Status)
            Assert.AreEqual(alpha, result.Snapshot.DefaultAlpha)
            Assert.AreEqual(alpha, result.Snapshot.CreateAnovaOptions().Alpha)
            Assert.AreEqual(alpha, result.Snapshot.CreateKruskalWallisOptions().Alpha)
            Assert.AreEqual(0.025, result.Snapshot.CreateAnalysisDefaults().ResolveAlpha(0.025))
            Dim overridden = result.Snapshot.CreateAnovaOptions()
            overridden.Alpha = 0.1
            Assert.AreEqual(alpha, result.Snapshot.DefaultAlpha)
            Assert.AreEqual(alpha, result.Snapshot.CreateAnovaOptions().Alpha)
        Next
    End Sub

    <DataTestMethod>
    <DataRow(1, 8)>
    <DataRow(2, 2)>
    <DataRow(16, 16)>
    <DataRow(17, 8)>
    Public Sub Digits_UsePublishedCoreBounds(requested As Integer, expected As Integer)
        Dim data = PreferenceSnapshot.Defaults().ToData()
        data.PValues.DecimalPlaces = requested
        data.PValues.SmallValueDisplay = CType(99, PValueSmallValueDisplay)
        data.PValues.UseUpperBound = False
        Dim result = PreferenceSnapshot.Normalize(data)
        Assert.AreEqual(expected, result.Snapshot.DecimalPlaces)
        Assert.AreEqual(PValueSmallValueDisplay.LessThanThreshold, result.Snapshot.SmallValueDisplay)
        Assert.IsFalse(result.Snapshot.UseUpperBound)
        Assert.AreEqual(requested, data.PValues.DecimalPlaces.Value)
    End Sub

    <TestMethod>
    Public Sub MissingFields_DefaultIndependentlyWithoutErasingValidValues()
        Dim data As New PreferenceData With {.DefaultAlpha = 0.01, .TraceEnabled = False,
            .PValues = New PValuePreferenceData With {.SmallValueDisplay = PValueSmallValueDisplay.ScientificNotation, .UseUpperBound = False}}
        Dim result = PreferenceSnapshot.Normalize(data)
        Assert.AreEqual(PreferenceStatus.Normalized, result.Status)
        Assert.AreEqual(0.01, result.Snapshot.DefaultAlpha)
        Assert.IsFalse(result.Snapshot.TraceEnabled)
        Assert.AreEqual(Integer.MinValue, result.Snapshot.DefaultRandomSeed)
        Assert.AreEqual(8, result.Snapshot.DecimalPlaces)
        Assert.AreEqual(PValueSmallValueDisplay.ScientificNotation, result.Snapshot.SmallValueDisplay)
        Assert.IsFalse(result.Snapshot.UseUpperBound)
        Assert.AreEqual(PreferenceStatus.Loaded, PreferenceSnapshot.Normalize(result.Snapshot.ToData()).Status)
        data.PValues = Nothing
        Dim noNested = PreferenceSnapshot.Normalize(data).Snapshot
        Assert.AreEqual(0.01, noNested.DefaultAlpha)
        Assert.IsTrue(noNested.UseUpperBound)
    End Sub

    <DataTestMethod>
    <DataRow(0)>
    <DataRow(-1)>
    <DataRow(2)>
    <DataRow(Integer.MaxValue)>
    Public Sub UnknownSchemas_AreNotClaimedAsMigrated(version As Integer)
        Dim data = PreferenceSnapshot.Defaults().ToData()
        data.SchemaVersion = version
        data.DefaultAlpha = 0.01
        Dim result = PreferenceSnapshot.Normalize(data)
        Assert.AreEqual(PreferenceStatus.UnsupportedVersion, result.Status)
        Assert.IsFalse(result.CanUseStoredData)
        Assert.AreEqual(PreferencePersistence.SessionOnly, result.Persistence)
        Assert.AreEqual(version, data.SchemaVersion)
        Assert.AreEqual(0.01, data.DefaultAlpha.Value)
    End Sub

    <DataTestMethod>
    <DataRow(0, Integer.MinValue)>
    <DataRow(Integer.MinValue, Integer.MinValue)>
    <DataRow(12345, 12345)>
    <DataRow(-17, -17)>
    Public Sub Seeds_KeepHistoricalZeroSentinelPolicy(seed As Integer, expected As Integer)
        Dim data = PreferenceSnapshot.Defaults().ToData()
        data.DefaultRandomSeed = seed
        Dim snapshot = PreferenceSnapshot.Normalize(data).Snapshot
        Assert.AreEqual(expected, snapshot.DefaultRandomSeed)
        Assert.AreEqual(expected, snapshot.CreateAnalysisDefaults().ResolveRandomSeed(generateWhenMissing:=False))
    End Sub

    <TestMethod>
    Public Async Function StoreReplay_CopiesMutableInputsAndExportsAndDoesNotApplyGlobals() As Task
        Dim initialRuntime = CoreServices.AnalysisDefaults
        Dim initialTrace = CoreServices.TraceExecutionLoggingEnabled
        Dim store As IPreferenceStore = New InMemoryPreferenceStore()
        Dim empty = Await store.LoadAsync()
        Assert.AreEqual(PreferenceStatus.Defaulted, empty.Status)
        Dim data = PreferenceSnapshot.Defaults().ToData()
        data.DefaultAlpha = 0.01
        data.DefaultRandomSeed = 12345
        data.TraceEnabled = False
        data.PValues.SmallValueDisplay = PValueSmallValueDisplay.FixedDecimal
        Dim snapshot = PreferenceSnapshot.Normalize(data).Snapshot
        Dim saved = Await store.SaveAsync(snapshot)
        Assert.AreEqual(PreferenceStatus.Saved, saved.Status)
        Assert.AreEqual(PreferencePersistence.SessionOnly, saved.Persistence)
        data.DefaultAlpha = 0.2
        data.PValues.DecimalPlaces = 2
        Dim first = Await store.LoadAsync()
        Dim exported = first.Snapshot.ToData()
        exported.DefaultAlpha = 0.3
        exported.PValues.SmallValueDisplay = PValueSmallValueDisplay.ScientificNotation
        first.Snapshot.CreateDisplaySettings().DecimalPlaces = 16
        first.Snapshot.CreateAnalysisDefaults().DefaultAlpha = 0.4
        Dim replay = Await store.LoadAsync()
        Assert.AreEqual(PreferenceStatus.Loaded, replay.Status)
        Assert.AreEqual(0.01, replay.Snapshot.DefaultAlpha)
        Assert.AreEqual(8, replay.Snapshot.DecimalPlaces)
        Assert.AreEqual(PValueSmallValueDisplay.FixedDecimal, replay.Snapshot.SmallValueDisplay)
        Assert.IsFalse(replay.Snapshot.TraceEnabled)
        Assert.AreSame(initialRuntime, CoreServices.AnalysisDefaults)
        Assert.AreEqual(initialTrace, CoreServices.TraceExecutionLoggingEnabled)
    End Function

    <TestMethod>
    Public Sub AlphaAndDisplay_ApplyExplicitlyAndDoNotChangeNumbers()
        Dim previous = CoreServices.AnalysisDefaults
        Dim previousTrace = CoreServices.TraceExecutionLoggingEnabled
        Try
            Dim data = PreferenceSnapshot.Defaults().ToData()
            data.DefaultAlpha = 0.01
            data.DefaultRandomSeed = 12345
            data.TraceEnabled = False
            data.PValues.DecimalPlaces = 6
            data.PValues.SmallValueDisplay = PValueSmallValueDisplay.ScientificNotation
            data.PValues.UseUpperBound = False
            Dim snapshot = PreferenceSnapshot.Normalize(data).Snapshot
            snapshot.ApplyToCoreDefaults()
            Assert.AreEqual(0.01, CoreServices.AnalysisDefaults.DefaultAlpha)
            Assert.AreEqual(12345, CoreServices.AnalysisDefaults.DefaultRandomSeed)
            Assert.IsFalse(CoreServices.TraceExecutionLoggingEnabled)
            Dim table As New OutputTableArtifact("p", New ResultTableOutputModel(New Object(,) {{0.01}, {0.02}, {0.000000001}}, pvalueColumns:={1}))
            Dim plan = OutputSemanticPlanner.Create(table, snapshot.CreateDisplaySettings(), snapshot.DefaultAlpha, "")
            Assert.AreEqual(snapshot.CreateAnovaOptions().Alpha, plan.Alpha)
            Assert.AreEqual(snapshot.CreateKruskalWallisOptions().Alpha, plan.Alpha)
            Assert.IsTrue(plan.Artifacts(0).Cells(0).IsSignificant)
            Assert.IsFalse(plan.Artifacts(0).Cells(1).IsSignificant)
            Assert.AreEqual(0.000000001, plan.Artifacts(0).Cells(2).Cell.BinaryValue.Value)
            Assert.AreEqual(6, plan.Policy.DecimalPlaces)
            Assert.IsFalse(plan.Policy.UseUpperBound)
            Assert.AreEqual(PValueSmallValueDisplay.ScientificNotation, plan.Policy.SmallValueDisplay)
        Finally
            CoreServices.AnalysisDefaults = previous
            CoreServices.TraceExecutionLoggingEnabled = previousTrace
        End Try
    End Sub
End Class
