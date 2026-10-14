Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text.Json
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Batch 00.02: stable IDs, descriptor metadata, default factories and enum parity.</summary>
<TestClass>
Public Class AnalysisCatalogTests

    <TestMethod>
    Public Sub Catalog_ExposesExactlyTwoStableUniquePublishedIds()
        Assert.AreEqual("anova.oneway", AnalysisIds.OneWayAnova)
        Assert.AreEqual("nonparametric.kruskal-wallis", AnalysisIds.KruskalWallis)
        Assert.AreEqual(2, AnalysisCatalog.All.Count)
        Assert.AreEqual(2, AnalysisCatalog.All.Select(Function(d) d.Id).Distinct(StringComparer.Ordinal).Count())
        Assert.AreEqual(2, AnalysisCatalog.All.Select(Function(d) d.LegacyKind.Value).Distinct().Count())
        AnalysisCatalog.ValidateDescriptors(AnalysisCatalog.All)
        CollectionAssert.AreEqual(
            New String() {AnalysisIds.OneWayAnova, AnalysisIds.KruskalWallis},
            AnalysisCatalog.All.Select(Function(d) d.Id).ToArray())
    End Sub

    <TestMethod>
    Public Sub Catalog_RoundTripsLegacyEnums_AndRejectsUnknownKeys()
        Assert.AreEqual(1, CInt(SpreadsheetAnalysisKind.OneWayAnova))
        Assert.AreEqual(2, CInt(SpreadsheetAnalysisKind.KruskalWallis))

        For Each descriptor As IAnalysisDescriptor In AnalysisCatalog.All
            Assert.IsTrue(descriptor.LegacyKind.HasValue)
            Assert.AreSame(descriptor, AnalysisCatalog.GetById(descriptor.Id))
            Assert.AreSame(descriptor, AnalysisCatalog.GetByLegacyKind(descriptor.LegacyKind.Value))
            Dim kind As SpreadsheetAnalysisKind = CType(0, SpreadsheetAnalysisKind)
            Assert.IsTrue(AnalysisCatalog.TryGetLegacyKind(descriptor.Id, kind))
            Assert.AreEqual(descriptor.LegacyKind.Value, kind)
        Next

        Dim found As IAnalysisDescriptor = Nothing
        Assert.IsFalse(AnalysisCatalog.TryGetById(Nothing, found))
        Assert.IsNull(found)
        Assert.IsFalse(AnalysisCatalog.TryGetById("", found))
        Assert.IsFalse(AnalysisCatalog.TryGetById("ANOVA.ONEWAY", found), "IDs are ordinal/case-sensitive")
        Assert.IsFalse(AnalysisCatalog.TryGetById("anova.unknown", found))
        Assert.IsFalse(AnalysisCatalog.TryGetByLegacyKind(CType(999, SpreadsheetAnalysisKind), found))
        Assert.IsNull(found)
        Dim kindOut As SpreadsheetAnalysisKind = SpreadsheetAnalysisKind.OneWayAnova
        Assert.IsFalse(AnalysisCatalog.TryGetLegacyKind("bad.id", kindOut))
        Assert.AreEqual(0, CInt(kindOut))
        Assert.ThrowsException(Of KeyNotFoundException)(Sub() AnalysisCatalog.GetById("bad.id"))
        Assert.ThrowsException(Of KeyNotFoundException)(Sub() AnalysisCatalog.GetByLegacyKind(CType(999, SpreadsheetAnalysisKind)))
    End Sub

    <TestMethod>
    Public Sub Descriptors_HaveSchemaHelpTypesAndOutputMetadata()
        Dim anova As IAnalysisDescriptor = AnalysisCatalog.GetById(AnalysisIds.OneWayAnova)
        Dim kw As IAnalysisDescriptor = AnalysisCatalog.GetById(AnalysisIds.KruskalWallis)

        Assert.AreEqual("One-Way ANOVA", anova.DisplayName)
        Assert.AreEqual("Kruskal-Wallis Test", kw.DisplayName)
        Assert.AreEqual("Parametric", anova.Category)
        Assert.AreEqual("Nonparametric", kw.Category)
        Assert.AreEqual("anova.oneway.options.v1", anova.OptionsSchemaId)
        Assert.AreEqual("nonparametric.kruskal-wallis.options.v1", kw.OptionsSchemaId)
        Assert.AreEqual("grouped-numeric.wide.v1", anova.InputSchemaId)
        Assert.AreEqual(anova.InputSchemaId, kw.InputSchemaId)
        Assert.AreEqual(GetType(OneWayAnovaAnalysisOptions), anova.OptionsType)
        Assert.AreEqual(GetType(KruskalWallisAnalysisOptions), kw.OptionsType)
        Assert.AreEqual(AnalysisOutputKinds.Tables Or AnalysisOutputKinds.Metrics Or AnalysisOutputKinds.TypedDetail, anova.OutputKinds)
        Assert.AreEqual(anova.OutputKinds, kw.OutputKinds)

        For Each d As IAnalysisDescriptor In AnalysisCatalog.All
            Assert.AreEqual(1, d.ContractVersion)
            Assert.AreEqual(AnalysisAvailability.ImplementedInCommon, d.Availability)
            Assert.AreEqual(AnalysisExecutionCost.Low, d.EstimatedCost)
            Assert.AreEqual(0, d.RequiredCapabilities.Count, "00.06 will add host-specific requirements")
            Assert.IsTrue(d.InputTypes.Contains(GetType(SpreadsheetRangeData)))
            Assert.IsTrue(d.InputTypes.Contains(GetType(GroupedNumericData)))
            Assert.AreEqual(2, d.InputTypes.Count)
            Assert.IsTrue(d.HelpUrl.StartsWith("https://", StringComparison.Ordinal))
            Assert.IsTrue(d.HelpUrl.EndsWith("/", StringComparison.Ordinal))
            Assert.IsFalse(String.IsNullOrWhiteSpace(d.Description))
        Next
        StringAssert.Contains(anova.HelpUrl, "/methods/one-way-anova/")
        StringAssert.Contains(kw.HelpUrl, "/methods/kruskal-wallis-test/")
    End Sub

    <TestMethod>
    Public Sub DescriptorDefaults_ReturnIndependentObjects_WithLegacyDefaults()
        Dim a As IAnalysisDescriptor = AnalysisCatalog.GetById(AnalysisIds.OneWayAnova)
        Dim first As OneWayAnovaAnalysisOptions = DirectCast(a.CreateDefaultOptions(), OneWayAnovaAnalysisOptions)
        Dim second As OneWayAnovaAnalysisOptions = DirectCast(a.CreateDefaultOptions(), OneWayAnovaAnalysisOptions)
        Assert.AreNotSame(first, second)
        Assert.AreEqual(0.05, first.Alpha, 0.0)
        Assert.IsFalse(first.IncludeWelch)
        Assert.IsFalse(first.IncludeFisherLsd)
        Assert.IsFalse(first.IncludeBonferroni)
        Assert.IsFalse(first.IncludeTukeyKramer)
        Assert.IsFalse(first.IncludeGamesHowell)
        first.IncludeWelch = True
        first.Alpha = 0.01
        Assert.IsFalse(second.IncludeWelch)
        Assert.AreEqual(0.05, second.Alpha, 0.0)

        Dim k As IAnalysisDescriptor = AnalysisCatalog.GetById(AnalysisIds.KruskalWallis)
        Dim firstK As KruskalWallisAnalysisOptions = DirectCast(k.CreateDefaultOptions(), KruskalWallisAnalysisOptions)
        Dim secondK As KruskalWallisAnalysisOptions = DirectCast(k.CreateDefaultOptions(), KruskalWallisAnalysisOptions)
        Assert.AreNotSame(firstK, secondK)
        Assert.AreEqual(0.05, firstK.Alpha, 0.0)
        Assert.IsTrue(firstK.IncludeDunnComparisons)
        firstK.IncludeDunnComparisons = False
        Assert.IsTrue(secondK.IncludeDunnComparisons)
    End Sub

    <TestMethod>
    Public Sub Stage304GoldenFixtures_UseSameStableIdsAndEnumsAsCatalog()
        For Each name As String In New String() {
            "anova-base", "anova-welch-tukey", "anova-all-posthoc", "kw-base", "kw-dunn"
        }
            Dim path As String = System.IO.Path.Combine(AppContext.BaseDirectory, "TestData", "Stage304", name & ".json")
            Assert.IsTrue(File.Exists(path), name)
            Using doc As JsonDocument = JsonDocument.Parse(File.ReadAllText(path))
                Dim id As String = doc.RootElement.GetProperty("analysisIdProposed").GetString()
                Dim legacy As Integer = doc.RootElement.GetProperty("legacyEnumValue").GetInt32()
                Dim descriptor As IAnalysisDescriptor = AnalysisCatalog.GetById(id)
                Assert.IsTrue(descriptor.LegacyKind.HasValue, name)
                Assert.AreEqual(legacy, CInt(descriptor.LegacyKind.Value), name)
            End Using
        Next
    End Sub

    <TestMethod>
    Public Sub Catalog_CollectionsAreReadOnly()
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() DirectCast(AnalysisCatalog.All, IList(Of IAnalysisDescriptor)).Clear())
        Dim descriptor As IAnalysisDescriptor = AnalysisCatalog.All(0)
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() DirectCast(descriptor.InputTypes, IList(Of Type)).Clear())
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() DirectCast(descriptor.RequiredCapabilities, IList(Of String)).Add("write.newWorkbook"))
    End Sub

    <TestMethod>
    Public Sub Catalog_RejectsDuplicateIdsAndLegacyEnums()
        Dim one As IAnalysisDescriptor = MakeDescriptor("test.one", SpreadsheetAnalysisKind.OneWayAnova)
        Dim duplicateId As IAnalysisDescriptor = MakeDescriptor("test.one", SpreadsheetAnalysisKind.KruskalWallis)
        Dim duplicateKind As IAnalysisDescriptor = MakeDescriptor("test.two", SpreadsheetAnalysisKind.OneWayAnova)
        Assert.ThrowsException(Of ArgumentException)(
            Sub() AnalysisCatalog.ValidateDescriptors(New IAnalysisDescriptor() {one, duplicateId}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() AnalysisCatalog.ValidateDescriptors(New IAnalysisDescriptor() {one, duplicateKind}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() AnalysisCatalog.ValidateDescriptors(New IAnalysisDescriptor() {Nothing}))
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() AnalysisCatalog.ValidateDescriptors(Nothing))
    End Sub

    <TestMethod>
    Public Sub Descriptor_RejectsInvalidIdAndInvalidOptionsFactoryResult()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() MakeDescriptor("Upper.Case", SpreadsheetAnalysisKind.OneWayAnova))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() MakeDescriptor("bad id", SpreadsheetAnalysisKind.OneWayAnova))
        Dim invalid As IAnalysisDescriptor = New AnalysisDescriptor(
            "test.factory", "Test", "Tests", "Test factory", "https://example.org/help/", 1,
            "test.input.v1", "test.options.v1",
            New Type() {GetType(GroupedNumericData)},
            GetType(OneWayAnovaAnalysisOptions),
            Function() CType(New KruskalWallisAnalysisOptions(), SpreadsheetAnalysisOptions),
            AnalysisOutputKinds.Tables, AnalysisAvailability.Planned,
            AnalysisExecutionCost.Low, Array.Empty(Of String)(), Nothing)
        Assert.ThrowsException(Of InvalidOperationException)(Sub() invalid.CreateDefaultOptions())
    End Sub

    Private Shared Function MakeDescriptor(id As String, kind As SpreadsheetAnalysisKind) As IAnalysisDescriptor
        Return New AnalysisDescriptor(
            id, "Test", "Tests", "Testing catalogue", "https://example.org/help/", 1,
            "test.input.v1", "test.options.v1",
            New Type() {GetType(GroupedNumericData)},
            GetType(OneWayAnovaAnalysisOptions),
            Function() CType(New OneWayAnovaAnalysisOptions(), SpreadsheetAnalysisOptions),
            AnalysisOutputKinds.Tables, AnalysisAvailability.Planned,
            AnalysisExecutionCost.Low, Array.Empty(Of String)(), kind)
    End Function
End Class
