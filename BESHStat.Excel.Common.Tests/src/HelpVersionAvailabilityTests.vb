Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class HelpVersionAvailabilityTests
    <DataTestMethod>
    <DataRow("OneWayANOVA", "anova.oneway", "One-Way ANOVA", "Parametric", "one-way-anova")>
    <DataRow("KruskalWallisTest", "nonparametric.kruskal-wallis", "Kruskal-Wallis Test", "Nonparametric", "kruskal-wallis-test")>
    Public Sub PilotIdentityAndUrlsMatchExistingCatalog(key As String, id As String, title As String, category As String, slug As String)
        Dim byKey As HelpDestination = Nothing, byId As HelpDestination = Nothing
        Assert.IsTrue(HelpDestination.TryMethodKey(key, byKey))
        Assert.IsTrue(HelpDestination.TryAnalysisId(id, byId))
        Assert.AreSame(byKey.Metadata, byId.Metadata)
        Assert.AreEqual(title, byId.Metadata.DisplayName)
        Assert.AreEqual(category, byId.Metadata.Category)
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl & "methods/" & slug & "/", byId.Url)
        Assert.AreEqual(AnalysisCatalog.GetById(id).HelpUrl, byId.Url)
        Assert.AreSame(GeneratedMethodMetadata.GetByKey(key), byId.Metadata)
    End Sub

    <DataTestMethod>
    <DataRow(Nothing)> <DataRow("")> <DataRow(" ")> <DataRow("../")> <DataRow("?next=https://evil.test")>
    <DataRow("#anova")> <DataRow("javascript:alert(1)")> <DataRow("https://evil.test/")>
    <DataRow("//evil.test")> <DataRow(" anova.oneway")> <DataRow("anova.oneway ")>
    <DataRow("ANOVA.ONEWAY")> <DataRow("аnova.oneway")> <DataRow("OneWayANOVA/")>
    Public Sub InvalidIdentifiersNeverComposeLinks(value As String)
        Dim result As HelpDestination = Nothing
        Assert.IsFalse(HelpDestination.TryMethodKey(value, result))
        Assert.IsTrue(result.IsFallback)
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, result.Url)
        Assert.IsFalse(HelpDestination.TryAnalysisId(value, result))
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, result.Url)
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpDestination.ForPageKey(value).Url)
        Assert.IsFalse(FeatureAvailabilityQuery.ByAnalysisId(value, HostCapabilityProfiles.WindowsExcelDna(), SpreadsheetOutputDestination.NewWorksheet).IsSupportedOnHost)
    End Sub

    <TestMethod>
    Public Sub AllMethodsAndUtilitiesAreSafeDistinctImmutableAndSeparate()
        Dim urls As New HashSet(Of String)(StringComparer.Ordinal)
        For Each entry In GeneratedMethodMetadata.All.Concat(GeneratedMethodMetadata.Pages)
            Dim url As New Uri(entry.HelpUrl)
            Assert.AreEqual("https", url.Scheme)
            Assert.AreEqual("www.beshstat.eu", url.Host)
            Assert.AreEqual(String.Empty, url.Query)
            Assert.AreEqual(String.Empty, url.Fragment)
            Assert.AreEqual(String.Empty, url.UserInfo)
            Assert.IsTrue(url.AbsolutePath.StartsWith("/beshstatng/help/latest/", StringComparison.Ordinal))
            Assert.IsTrue(url.AbsolutePath.EndsWith("/", StringComparison.Ordinal))
            Assert.IsTrue(urls.Add(entry.HelpUrl))
        Next
        Assert.AreEqual(78, urls.Count)
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl & "global-settings/", HelpDestination.ForPageKey("GlobalSettings").Url)
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl & "export-chart/", HelpDestination.ForPageKey("ExportChart").Url)
        Assert.IsTrue(HelpDestination.ForMethodKey("GlobalSettings").IsFallback)
        Assert.IsTrue(HelpDestination.ForAnalysisId("OneWayANOVA").IsFallback)
        Assert.IsTrue(HelpDestination.ForMethodKey("anova.oneway").IsFallback)
        Assert.IsTrue(HelpDestination.ForPageKey("OneWayANOVA").IsFallback)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(GeneratedMethodMetadata.Pages, IList(Of AnalysisMethodMetadata)).Clear())
        Assert.AreEqual(64, GeneratedMethodMetadata.SourceRevision.Length)
    End Sub

    <DataTestMethod>
    <DataRow(Nothing)> <DataRow("")> <DataRow("0.0.0.0")> <DataRow("1.2.3")> <DataRow("1.2")>
    <DataRow("1.2.3.4.5")> <DataRow("1.2.-1.4")> <DataRow("1.2.+3.4")> <DataRow("1.2..4")>
    <DataRow("1.2.3.65536")> <DataRow("2147483648.2.3.4")> <DataRow("1.2.3.4 ")>
    <DataRow(" 1.2.3.4")> <DataRow("１.2.3.4")> <DataRow("1.2.3.4-beta")>
    Public Sub InvalidVersionsReturnExplicitFallback(value As String)
        Dim version As ProductVersionDescriptor = Nothing
        Assert.IsFalse(ProductVersionDescriptor.TryParse(value, version))
        Assert.IsFalse(version.IsAvailable)
        Assert.AreEqual("Version unavailable", version.DisplayLabel)
        Assert.AreEqual(String.Empty, version.ProductVersion)
        Assert.AreEqual("latest", version.HelpRoutePolicy)
    End Sub

    <TestMethod>
    Public Sub VersionParsingIsFourPartCultureIndependentAndRequiresVerifiedVersionedHelp()
        Dim previous = CultureInfo.CurrentCulture
        Try
            For Each culture In {"en-US", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim version As ProductVersionDescriptor = Nothing
                Assert.IsTrue(ProductVersionDescriptor.TryParse("1.2.6.0", version))
                Assert.AreEqual("1.2.6.0", version.ProductVersion)
                Assert.AreEqual("Version 1.2.6.0", version.DisplayLabel)
                Assert.AreEqual("verified-version-first", version.HelpRoutePolicy)
                Dim staged = HelpDestination.ForAnalysisId(AnalysisIds.OneWayAnova).GetLaunchTarget(version, False)
                Assert.AreEqual(HelpDestination.ForAnalysisId(AnalysisIds.OneWayAnova).Url, staged.Url)
                Assert.IsFalse(staged.UsesResolver)
                Assert.IsTrue(ProductVersionDescriptor.TryParse("65535.65535.65535.65535", version))
                Assert.IsTrue(ProductVersionDescriptor.TryParse("0.0.0.1", version))
                Assert.IsTrue(ProductVersionDescriptor.TryParse("01.02.03.04", version))
                Assert.AreEqual("1.2.3.4", version.ProductVersion)
            Next
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
        Assert.IsFalse(ProductVersionDescriptor.FromVersion(Nothing).IsAvailable)
        Assert.IsFalse(ProductVersionDescriptor.FromVersion(New Version(1, 2)).IsAvailable)
        Assert.AreEqual("1.2.3.4", ProductVersionDescriptor.FromVersion(New Version(1, 2, 3, 4)).ProductVersion)
    End Sub

    <TestMethod>
    Public Sub AvailabilityRequiresRegistrationAndExistingHostPreflightWithoutExecution()
        For Each descriptor In AnalysisCatalog.All
            Dim host = HostCapabilityProfiles.OfficeJsExcel("Online")
            Dim info = FeatureAvailabilityQuery.ByAnalysisId(descriptor.Id, host, SpreadsheetOutputDestination.NewWorksheet)
            Assert.IsTrue(info.IsDocumented)
            Assert.IsTrue(info.IsRegisteredInCommon)
            Assert.IsTrue(info.IsSupportedOnHost)
            Assert.AreEqual(String.Empty, info.Reason)
            Dim byKey = FeatureAvailabilityQuery.ByMethodKey(info.Help.Metadata.Key, host, SpreadsheetOutputDestination.NewWorksheet)
            Assert.IsTrue(byKey.IsSupportedOnHost)
            host.Capabilities.Clear()
            Assert.IsTrue(info.IsSupportedOnHost, "Result must not retain mutable host state.")
            Dim missing = FeatureAvailabilityQuery.ByAnalysisId(descriptor.Id, host, SpreadsheetOutputDestination.NewWorksheet)
            Assert.AreEqual(CapabilityPreflightCodes.MissingCapability, missing.ReasonCode)
            Assert.IsFalse(missing.IsSupportedOnHost)
            Assert.AreEqual(CapabilityPreflightCodes.HostUnavailable, FeatureAvailabilityQuery.ByAnalysisId(descriptor.Id, Nothing, SpreadsheetOutputDestination.NewWorksheet).ReasonCode)
            host = HostCapabilityProfiles.OfficeJsExcel("Online")
            host.Capabilities.Remove(HostCapabilityIds.NewWorksheet)
            Assert.AreEqual(CapabilityPreflightCodes.UnsupportedDestination, FeatureAvailabilityQuery.ByAnalysisId(descriptor.Id, host, SpreadsheetOutputDestination.NewWorksheet).ReasonCode)
            host.Capabilities.Add(HostCapabilityIds.NewWorkbook)
            Assert.AreEqual(CapabilityPreflightCodes.UnsupportedDestination, FeatureAvailabilityQuery.ByAnalysisId(descriptor.Id, host, SpreadsheetOutputDestination.NewWorkbook).ReasonCode)
            Assert.AreEqual(CapabilityPreflightCodes.InvalidTarget, FeatureAvailabilityQuery.ByAnalysisId(descriptor.Id, host, CType(999, SpreadsheetOutputDestination)).ReasonCode)
            Assert.IsTrue(FeatureAvailabilityQuery.ByAnalysisId(descriptor.Id, HostCapabilityProfiles.WindowsExcelDna(), SpreadsheetOutputDestination.NewWorkbook).IsSupportedOnHost)
        Next
    End Sub

    <TestMethod>
    Public Sub PlannedDescriptorCannotAcquireRegistrationOrExecuteThroughDiscovery()
        Dim pilot = AnalysisCatalog.GetById(AnalysisIds.OneWayAnova)
        Dim planned As New AnalysisDescriptor("planned.analysis", "Planned", "Tests", "Test only", pilot.HelpUrl,
            pilot.ContractVersion, pilot.InputSchemaId, pilot.OptionsSchemaId, pilot.InputTypes, pilot.OptionsType,
            Function() ThrowIfExecuted(), pilot.OutputKinds, AnalysisAvailability.Planned, pilot.EstimatedCost,
            pilot.RequiredCapabilities, Nothing)
        ' Existing registration validation rejects a handler for a planned descriptor.
        Dim handler As New PlannedHandler()
        Assert.ThrowsException(Of ArgumentException)(Sub() AnalysisHandlerRegistry.ValidateRegistrations({planned}, {handler}))
        Dim info = FeatureAvailabilityQuery.ByAnalysisId(planned.Id, HostCapabilityProfiles.WindowsExcelDna(), SpreadsheetOutputDestination.NewWorksheet)
        Assert.IsFalse(info.IsRegisteredInCommon)
        Assert.IsFalse(info.IsSupportedOnHost)
        Assert.AreEqual(CapabilityPreflightCodes.UnknownAnalysis, info.ReasonCode)
    End Sub

    Private Shared Function ThrowIfExecuted() As SpreadsheetAnalysisOptions
        Throw New AssertFailedException("Discovery must not create options or execute an analysis.")
    End Function

    Private Class PlannedHandler
        Implements IAnalysisHandler
        Public ReadOnly Property AnalysisId As String = "planned.analysis" Implements IAnalysisHandler.AnalysisId
        Public ReadOnly Property InputType As Type = GetType(GroupedNumericData) Implements IAnalysisHandler.InputType
        Public ReadOnly Property OptionsType As Type = GetType(OneWayAnovaAnalysisOptions) Implements IAnalysisHandler.OptionsType
        Public Function Execute(input As SpreadsheetAnalysisInput, options As SpreadsheetAnalysisOptions) As SpreadsheetAnalysisResult Implements IAnalysisHandler.Execute
            Throw New AssertFailedException("Discovery executed a handler.")
        End Function
    End Class

    <TestMethod>
    Public Sub DocumentationDoesNotAdvertiseExecutableOrWindowsSupport()
        For Each method In GeneratedMethodMetadata.All
            Dim info = FeatureAvailabilityQuery.ByMethodKey(method.Key, HostCapabilityProfiles.WindowsExcelDna(), SpreadsheetOutputDestination.NewWorksheet)
            Assert.IsTrue(info.IsDocumented)
            Assert.AreEqual(method.AnalysisId IsNot Nothing, info.IsRegisteredInCommon)
            If method.AnalysisId Is Nothing Then
                Assert.IsFalse(info.IsSupportedOnHost)
                Assert.AreEqual(CapabilityPreflightCodes.UnsupportedAnalysis, info.ReasonCode)
                Assert.IsFalse(String.IsNullOrWhiteSpace(info.Reason))
            End If
        Next
        Assert.AreEqual(CapabilityPreflightCodes.UnknownAnalysis, FeatureAvailabilityQuery.ByAnalysisId("planned.analysis", Nothing, SpreadsheetOutputDestination.NewWorksheet).ReasonCode)
        For Each page In GeneratedMethodMetadata.Pages
            Assert.IsFalse(FeatureAvailabilityQuery.ByMethodKey(page.Key, Nothing, SpreadsheetOutputDestination.NewWorksheet).IsRegisteredInCommon)
        Next
        For Each type In {GetType(FeatureAvailabilityInfo), GetType(HelpDestination), GetType(ProductVersionDescriptor)}
            Assert.IsFalse(type.GetProperties().Any(Function(p) p.CanWrite))
        Next
    End Sub
End Class
