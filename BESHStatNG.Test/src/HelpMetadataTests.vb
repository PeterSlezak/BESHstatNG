Option Explicit On
Option Strict On
Imports System
Imports System.IO
Imports System.Windows.Forms
Imports BESHStatNG
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class HelpMetadataTests
    <TestMethod>
    Public Sub EveryWindowsTopicRetainsGeneratedIdentityAndLatestRelativePath()
        For Each item In GeneratedMethodMetadata.All
            Dim topic = CType([Enum].Parse(GetType(HelpTopic), item.Key), HelpTopic)
            Assert.AreEqual(item.HelpUrl, HelpLinks.GetUrl(topic))
        Next
        For Each item In GeneratedMethodMetadata.Pages
            Dim topic = CType([Enum].Parse(GetType(HelpTopic), item.Key), HelpTopic)
            Assert.AreEqual(item.HelpUrl, HelpLinks.GetUrl(topic))
        Next
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLinks.GetUrl(HelpTopic.Home))
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLinks.GetUrl(CType(-1, HelpTopic)))
        Assert.AreEqual(GeneratedMethodMetadata.SiteBaseUrl, HelpLinks.BaseUrlRoot)
    End Sub

    <TestMethod>
    Public Sub PilotAndSharedFormContextPreservesTitleAndExplicitTagSemantics()
        ' Lightweight forms exercise generated class/title dispatch without Excel COM or production controls.
        Using form As New ContextFixtures.UibyID()
            form.Text = "One-Way ANOVA"
            Assert.AreEqual(HelpTopic.OneWayANOVA, HelpContext.ResolveTopic(form))
            form.Text = "Kruskal-Wallis Test"
            Assert.AreEqual(HelpTopic.KruskalWallisTest, HelpContext.ResolveTopic(form))
            form.Text = "Box and Whiskers"
            Assert.AreEqual(HelpTopic.BoxAndWhiskers, HelpContext.ResolveTopic(form))
            form.Text = "Unpaired T-test"
            Assert.AreEqual(HelpTopic.UnpairedTwoSampleTTests, HelpContext.ResolveTopic(form))
            form.Text = "Unknown"
            Assert.AreEqual(HelpTopic.Home, HelpContext.ResolveTopic(form))
            form.Tag = HelpTopic.KruskalWallisTest
            Assert.AreEqual(HelpTopic.KruskalWallisTest, HelpContext.ResolveTopic(form))
            form.Tag = "OneWayANOVA"
            Assert.AreEqual(HelpTopic.OneWayANOVA, HelpContext.ResolveTopic(form))
        End Using
        Using form As New ContextFixtures.Ui12SampleSizeTtestSingleProp()
            form.Text = "Sample Size - Unpaired T-test"
            Assert.AreEqual(HelpTopic.UnpairedTTest, HelpContext.ResolveTopic(form))
        End Using
        Using form As New ContextFixtures.Ui99ExportChart()
            form.Text = "Export Chart"
            Assert.AreEqual(HelpTopic.ExportChart, HelpContext.ResolveTopic(form))
        End Using
        Using unknown As New Form With {.Text = "Unpaired T-test"}
            Assert.AreEqual(HelpTopic.Home, HelpContext.ResolveTopic(unknown))
        End Using
        Assert.AreEqual(HelpTopic.Home, HelpContext.ResolveTopic(Nothing))
    End Sub

    <TestMethod>
    Public Sub AboutVersionProjectsActualInstalledAssemblyAndRetainsBuildDate()
        Dim actual = GetType(AppGlobals).Assembly.GetName().Version.ToString(4)
        Assert.AreEqual(actual, AppGlobals.gAddinVersion)
        Assert.AreEqual(actual, AppGlobals.InstalledProductVersion.ProductVersion)
        Assert.AreEqual("Version " & actual, AppGlobals.InstalledProductVersion.DisplayLabel)
        Assert.AreEqual("verified-version-first", AppGlobals.InstalledProductVersion.HelpRoutePolicy)
        Assert.IsFalse(String.IsNullOrWhiteSpace(AppGlobals.GetBuildDateIso()))
    End Sub

    <DataTestMethod>
    <DataRow("1.2.5.0")> <DataRow("1.2.6.0")>
    Public Sub EveryTopicUsesSharedVersionLaunchWithoutChangingGetUrl(value As String)
        Dim version As ProductVersionDescriptor = Nothing
        Assert.IsTrue(ProductVersionDescriptor.TryParse(value, version))
        For Each item In GeneratedMethodMetadata.All
            Dim topic = CType([Enum].Parse(GetType(HelpTopic), item.Key), HelpTopic)
            Assert.AreEqual(item.HelpUrl, HelpLinks.GetUrl(topic))
            Assert.AreEqual(HelpDestination.ForMethodKey(item.Key).GetLaunchTarget(version, True).Url, HelpLinks.GetLaunchUrl(topic, version, True))
            Assert.AreEqual(item.HelpUrl, HelpLinks.GetLaunchUrl(topic, version, False))
        Next
        For Each item In GeneratedMethodMetadata.Pages
            Dim topic = CType([Enum].Parse(GetType(HelpTopic), item.Key), HelpTopic)
            Assert.AreEqual(HelpDestination.ForPageKey(item.Key).GetLaunchTarget(version, True).Url, HelpLinks.GetLaunchUrl(topic, version, True))
        Next
        Assert.AreEqual(HelpLaunchTarget.CreateHome(version, True).Url, HelpLinks.GetLaunchUrl(HelpTopic.Home, version, True))
        Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLinks.GetLaunchUrl(CType(-1, HelpTopic), version, True))
    End Sub

    <TestMethod>
    Public Sub WindowsShellAcceptsOnlyGeneratedDestinationsAndStagedResolverQueries()
        Dim launch = HelpLaunchTarget.CreateHome(AppGlobals.InstalledProductVersion, True)
        Assert.AreEqual(If(HelpLaunchTarget.ResolverPublished, launch.Url, GeneratedMethodMetadata.HomeUrl), HelpLinks.GetSafeOpenUrl(launch.Url))
        For Each hostile In {Nothing, "", "https://evil.test/", "javascript:alert(1)", HelpLaunchTarget.ResolverUrl & "?version=1.2.6.0&route=../", launch.VersionCandidateUrl}
            Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLinks.GetSafeOpenUrl(hostile))
        Next
        Dim root = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
        While root IsNot Nothing AndAlso Not File.Exists(Path.Combine(root.FullName, "AGENTS.md"))
            root = root.Parent
        End While
        Assert.IsNotNull(root)
        Dim source = File.ReadAllText(Path.Combine(root.FullName, "BESHstatNG/src/Help/HelpLinks.vb"))
        StringAssert.Contains(source, "OpenUrl(GetLaunchUrl(topic))")
        StringAssert.Contains(source, "AppInfrastructure.AppGlobals.InstalledProductVersion")
        For Each forbidden In {"HttpWebRequest", "WebRequest", "HttpClient", "GetResponse", "UrlExists", "Thread.Sleep"}
            Assert.IsFalse(source.Contains(forbidden))
        Next
    End Sub

    <TestMethod>
    Public Sub ProductionPilotUiAndExecutionRemainAtAcceptedSourceBoundary()
        Dim root = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
        While root IsNot Nothing AndAlso Not File.Exists(Path.Combine(root.FullName, "AGENTS.md"))
            root = root.Parent
        End While
        Assert.IsNotNull(root)
        Dim source = File.ReadAllText(Path.Combine(root.FullName, "BESHstatNG/src/UI/UibyID.vb"))
        StringAssert.Contains(source, "Me.WireHelp(Me.btnHelp)")
        StringAssert.Contains(source, "ClientSize.Height + 36")
        StringAssert.Contains(source, "ColumnCount = 2, .RowCount = 1")
        StringAssert.Contains(source, "BESHSTAT_DEV_EXECUTION_FIXTURE")
        StringAssert.Contains(source, "ExecutionFormClosing")
        Assert.IsFalse(source.Contains("executionDemo"))
    End Sub
End Class

Namespace ContextFixtures
    Friend Class UibyID
        Inherits Form
    End Class
    Friend Class Ui12SampleSizeTtestSingleProp
        Inherits Form
    End Class
    Friend Class Ui99ExportChart
        Inherits Form
    End Class
End Namespace
