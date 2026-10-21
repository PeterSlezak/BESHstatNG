Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class HelpLaunchTargetTests
    <DataTestMethod>
    <DataRow("OneWayANOVA", "methods/one-way-anova/", False)>
    <DataRow("KruskalWallisTest", "methods/kruskal-wallis-test/", False)>
    <DataRow("Histogram", "methods/histogram/", False)>
    <DataRow("GlobalSettings", "global-settings/", True)>
    <DataRow("ExportChart", "export-chart/", True)>
    Public Sub ExactVersionCandidatesPreserveAllAuthoredPaths(key As String, relative As String, page As Boolean)
        Dim help = If(page, HelpDestination.ForPageKey(key), HelpDestination.ForMethodKey(key))
        Dim previous = CultureInfo.CurrentCulture
        Try
            For Each culture In {"en-US", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                For Each value In {"1.2.5.0", "1.2.6.0"}
                    Dim version As ProductVersionDescriptor = Nothing
                    Assert.IsTrue(ProductVersionDescriptor.TryParse(value, version))
                    Dim launch = help.GetLaunchTarget(version, True)
                    Assert.AreEqual(GeneratedMethodMetadata.SiteBaseUrl & "/v" & value & "/" & relative, launch.VersionCandidateUrl)
                    Assert.AreEqual(help.Url, launch.LatestUrl)
                    Assert.AreEqual(GeneratedMethodMetadata.HomeUrl & relative, launch.LatestUrl)
                    Assert.AreEqual(HelpLaunchTarget.ResolverUrl & "?version=" & value & "&route=" & key, launch.Url)
                    Assert.IsTrue(launch.UsesResolver)
                    Assert.AreEqual(launch.Url, HelpLaunchTarget.SafeOpenUrl(launch.Url, True))
                    Assert.AreEqual(help.Url, help.GetLaunchTarget(version, False).Url)
                    Assert.AreEqual(HelpLaunchTarget.ResolverPublished, help.GetLaunchTarget(version).UsesResolver)
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <DataTestMethod> <DataRow("1.2.5.0")> <DataRow("1.2.6.0")>
    Public Sub HomeIsExplicitAndUnknownMethodsDoNotBecomeVersionedHome(value As String)
        Dim version As ProductVersionDescriptor = Nothing
        Assert.IsTrue(ProductVersionDescriptor.TryParse(value, version))
        Dim home = HelpLaunchTarget.CreateHome(version, True)
        Assert.AreEqual(GeneratedMethodMetadata.SiteBaseUrl & "/v" & value & "/", home.VersionCandidateUrl)
        Assert.AreEqual(HelpLaunchTarget.ResolverUrl & "?version=" & value & "&route=Home", home.Url)
        Dim unknown = HelpDestination.ForAnalysisId("unknown").GetLaunchTarget(version, True)
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, unknown.Url)
        Assert.IsNull(unknown.VersionCandidateUrl)
        Assert.IsNull(unknown.ResolverCandidateUrl)
    End Sub

    <DataTestMethod>
    <DataRow(Nothing)> <DataRow("")> <DataRow("0.0.0.0")> <DataRow("1.2.6")>
    <DataRow("v1.2.6.0")> <DataRow("1.2.6.65536")> <DataRow("1.2.6.0 ")>
    <DataRow("1.2.-6.0")> <DataRow("１.2.6.0")>
    Public Sub InvalidVersionNeverProducesAnInventedCandidate(value As String)
        Dim version As ProductVersionDescriptor = Nothing
        Assert.IsFalse(ProductVersionDescriptor.TryParse(value, version))
        For Each item In GeneratedMethodMetadata.All.Concat(GeneratedMethodMetadata.Pages)
            Dim destination = HelpDestination.ForMethodKey(item.Key)
            If destination.IsFallback Then destination = HelpDestination.ForPageKey(item.Key)
            Dim launch = destination.GetLaunchTarget(version, True)
            Assert.AreEqual(item.HelpUrl, launch.Url)
            Assert.IsFalse(launch.UsesResolver)
            Assert.IsNull(launch.VersionCandidateUrl)
        Next
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLaunchTarget.CreateHome(version, True).Url)
    End Sub

    <DataTestMethod>
    <DataRow(Nothing)> <DataRow("")> <DataRow("../")> <DataRow("javascript:alert(1)")>
    <DataRow("https://evil.test/")> <DataRow("OneWayANOVA%2f")> <DataRow("anova.oneway#x")>
    Public Sub HostileIdentityHasOnlyLatestHome(value As String)
        Dim version = ProductVersionDescriptor.FromVersion(New Version(1, 2, 6, 0))
        For Each destination In {HelpDestination.ForMethodKey(value), HelpDestination.ForAnalysisId(value), HelpDestination.ForPageKey(value)}
            Dim launch = destination.GetLaunchTarget(version, True)
            Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, launch.Url)
            Assert.IsFalse(launch.UsesResolver)
            Assert.IsNull(launch.VersionCandidateUrl)
        Next
    End Sub

    <DataTestMethod>
    <DataRow("?version=1.2.6.0&route=../")> <DataRow("?version=1.2.6.0&route=OneWayANOVA%2f")>
    <DataRow("?version=1.2.6.0&route=OneWayANOVA#x")> <DataRow("?version=01.2.6.0&route=OneWayANOVA")>
    <DataRow("?version=1.2.6.0&route=Home&next=https://evil.test")> <DataRow("?version=0.0.0.0&route=Home")>
    <DataRow("?version=1.2.6.0&route=constructor")> <DataRow("?route=Home&version=1.2.6.0")>
    <DataRow("?version=1.2.6.0&route=methods/one-way-anova/")> <DataRow("?version=1.2.6.0&route=Home\evil")>
    Public Sub LaunchAllowlistRejectsMalformedQueries(query As String)
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLaunchTarget.SafeOpenUrl(HelpLaunchTarget.ResolverUrl & query, True))
    End Sub

    <TestMethod>
    Public Sub ActivationIsExplicitImmutableAndDoesNotChangeRegistration()
        Dim version = ProductVersionDescriptor.FromVersion(New Version(1, 2, 6, 0))
        Dim target = HelpDestination.ForMethodKey("Histogram").GetLaunchTarget(version, True)
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLaunchTarget.SafeOpenUrl(target.Url, False))
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLaunchTarget.SafeOpenUrl(target.VersionCandidateUrl, True))
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLaunchTarget.SafeOpenUrl("https://evil.test/", True))
        Assert.AreEqual(2, AnalysisCatalog.All.Count)
        Assert.IsFalse(FeatureAvailabilityQuery.ByMethodKey("Histogram", HostCapabilityProfiles.WindowsExcelDna(), SpreadsheetOutputDestination.NewWorksheet).IsRegisteredInCommon)
        Assert.IsFalse(GetType(HelpLaunchTarget).GetProperties().Any(Function(p) p.CanWrite))
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLaunchTarget.Create(Nothing, version, True).Url)
    End Sub
End Class
