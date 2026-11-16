Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Xml.Linq
Imports BESHStatNG.Test.Harness45
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class WindowsHarness45Tests
    Private Shared Function Root() As String
        Return Environment.GetEnvironmentVariable("BESHSTAT_04A02_SOURCE_ROOT")
    End Function
    Private Shared Function Xml() As String
        Return File.ReadAllText(Path.Combine(Root(), "BESHstatNG/Resources/RibbonXML.xml"))
    End Function
    Public Shared Iterator Function Actions() As IEnumerable(Of Object())
        For Each element In XDocument.Parse(Xml()).Descendants().Where(Function(e) e.Attribute("onAction") IsNot Nothing)
            Yield New Object() {CStr(element.Attribute("id")), CStr(element.Attribute("onAction"))}
        Next
    End Function
    <DataTestMethod, DynamicData(NameOf(Actions), DynamicDataSourceType.Method)>
    Public Sub RibbonActualSignatureAndSourceDispatch(controlId As String, callback As String)
        FakeTrace45.Reset()
        Dim actual = CallbackHarness45.Resolve(GetType(Global.BESHStatNG.Ribbon), callback, "onAction")
        Assert.AreEqual("BESHStatNG.Ribbon", actual.DeclaringType.FullName)
        CallbackHarness45.DispatchFake(Xml(), controlId)
        Dim source = File.ReadAllText(Path.Combine(Root(), "BESHstatNG/BESHStatAddIn.vb"))
        Dim body = Regex.Match(source, "(?ms)^    Public Sub " & callback & "\(.*?^    End Sub").Value
        Assert.IsTrue(body.Length > 0)
        Dim destination = Regex.Match(body, "New (Ui\w+)\(")
        If body.Contains("Me.RegStart(") Then
            destination = Regex.Match(source, "New (UiGLM)\(")
            Assert.IsTrue(FakeTrace45.Events.Contains("Populate"))
        End If
        If destination.Success Then
            Dim name = destination.Groups(1).Value
            Assert.AreEqual(1, FakeTrace45.Events.Where(Function(v) v = "Construct:" & name).Count())
            Assert.AreEqual(1, FakeTrace45.Events.Where(Function(v) v = "Show:" & name).Count())
            Assert.IsTrue(FakeTrace45.Events.IndexOf("Construct:" & name) < FakeTrace45.Events.IndexOf("Show:" & name))
        Else
            CollectionAssert.AreEqual(New String() {"Help:synthetic-help/"}, FakeTrace45.Events.ToArray())
        End If
    End Sub

    <TestMethod>
    Public Sub RibbonImageOwnerIsInheritedAndNeverInvoked()
        Dim image = CallbackHarness45.Resolve(GetType(Global.BESHStatNG.Ribbon), "LoadImage", "loadImage")
        Assert.AreEqual("ExcelDna.Integration.CustomUI.ExcelRibbon", image.DeclaringType.FullName)
        Assert.IsTrue(CallbackHarness45.Validate(Xml(), GetType(Global.BESHStatNG.Ribbon)).Count > 1)
    End Sub

    <DataTestMethod>
    <DataRow("duplicate")> <DataRow("unresolved")> <DataRow("wrong-signature")> <DataRow("missing-enabled")> <DataRow("unsupported")> <DataRow("disabled")> <DataRow("placeholder")>
    Public Sub RibbonAdverseControlsFailClosed(kind As String)
        FakeTrace45.Reset()
        Dim documentText = Xml(), first = XDocument.Parse(documentText).Descendants().First(Function(e) e.Attribute("onAction") IsNot Nothing), id = CStr(first.Attribute("id")), callback = CStr(first.Attribute("onAction"))
        Select Case kind
            Case "duplicate"
                documentText = "<r><b id='same'/><b id='same'/></r>"
            Case "unresolved"
                documentText = "<r><b id='b' onAction='DoesNotExist'/></r>"
            Case "wrong-signature"
                documentText = "<r><b id='b' onAction='GetCustomUI'/></r>"
            Case "missing-enabled"
                documentText = "<r><b id='b' getEnabled='UnavailableEnabled'/></r>"
            Case "unsupported"
                documentText = "<r><b id='b' getContent='" & callback & "'/></r>"
            Case "disabled"
                documentText = "<r><b id='b' enabled='false' onAction='" & callback & "'/></r>" : id = "b"
            Case "placeholder"
                documentText = "<r><b id='b'/></r>" : id = "b"
        End Select
        Assert.ThrowsException(Of InvalidOperationException)(Sub() CallbackHarness45.DispatchFake(documentText, id))
        Assert.AreEqual(0, FakeTrace45.Events.Count)
    End Sub

    <DataTestMethod>
    <DataRow(True)> <DataRow(False)>
    Public Sub ChartCallbackQueueAndRejectionOrder(hasCharts As Boolean)
        FakeTrace45.Reset() : FakeForm45.Charts = hasCharts
        CallbackHarness45.DispatchFake(Xml(), "btn00_5ExportChart")
        Dim expected = If(hasCharts, New String() {"Queue", "Charts", "Construct:Ui99ExportChart", "Window", "Show:Ui99ExportChart"}, New String() {"Queue", "Charts", "Message"})
        CollectionAssert.AreEqual(expected, FakeTrace45.Events.ToArray())
    End Sub

    <TestMethod>
    Public Sub CallbackWorkbookCreationAndPopulationPrecedeDisplay()
        FakeTrace45.Reset() : FakeGlobals45.app.Workbooks.Count = 0
        CallbackHarness45.DispatchFake(Xml(), "btmCausal_PSM")
        CollectionAssert.AreEqual(New String() {"Workbooks.Add", "Construct:Ui20PropensityScoreMatching", "Populate", "Show:Ui20PropensityScoreMatching"}, FakeTrace45.Events.ToArray())
    End Sub

    <DataTestMethod>
    <DataRow("Workbooks.Add")> <DataRow("Construct:Ui20PropensityScoreMatching")> <DataRow("Populate")> <DataRow("Show:Ui20PropensityScoreMatching")>
    Public Sub CallbackFailuresHaveNoLaterSideEffects(failAt As String)
        FakeTrace45.Reset() : FakeGlobals45.app.Workbooks.Count = 0 : FakeTrace45.FailAt = failAt
        Dim failure = Assert.ThrowsException(Of TargetInvocationException)(Sub() CallbackHarness45.DispatchFake(Xml(), "btmCausal_PSM"))
        Assert.IsInstanceOfType(failure.InnerException, GetType(InvalidOperationException))
        Assert.AreEqual(failAt, FakeTrace45.Events.Last())
    End Sub

    <DataTestMethod>
    <DataRow("consistent")> <DataRow("zero-pid")> <DataRow("wrong-pid")> <DataRow("recycled-pid")> <DataRow("zero-hwnd")> <DataRow("stale-hwnd")> <DataRow("window-pid")> <DataRow("thread")> <DataRow("ambiguous")> <DataRow("image")> <DataRow("bitness")> <DataRow("session")> <DataRow("desktop")> <DataRow("workbook-path")> <DataRow("workbook-switch")> <DataRow("not-loaded")> <DataRow("forged-path")> <DataRow("xll-sha")> <DataRow("dll-sha")> <DataRow("mvid")> <DataRow("release-debug")> <DataRow("multiple-versions")> <DataRow("callbacks-unproved")> <DataRow("binding-pid")> <DataRow("binding-start")> <DataRow("binding-hwnd")> <DataRow("binding-module")> <DataRow("stale-time")> <DataRow("future-time")> <DataRow("build-sha")> <DataRow("unscoped")> <DataRow("missing")>
    Public Sub AttributionOfflineIdentityChecksNeverGrantNative(kind As String)
        Dim now = New DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero), start = now.AddMinutes(-10), mvid = New Guid("00000000-0000-0000-0000-000000000045")
        Dim build As New ReviewedBuild45 With {.Commit = New String("a"c, 40), .TreeSha = New String("b"c, 64), .DependenciesSha = New String("c"c, 64), .Configuration = "Debug", .Bitness = 64, .AssemblySha = New String("d"c, 64), .AssemblyMvid = mvid, .XllSha = New String("e"c, 64), .XllPath = "<synthetic-root>/build.xll", .ProductVersion = "synthetic45", .BuildUtc = start.AddHours(-1)}
        Dim expected = NewSession(start), observed = NewSession(start)
        Dim loaded As New LoadedCode45 With {.XllPath = build.XllPath, .XllShaAfterLoad = build.XllSha, .AssemblySha = build.AssemblySha, .Mvid = mvid, .Configuration = "Debug", .Bitness = 64, .ProductVersion = build.ProductVersion, .LoadedVersions = 1, .ModuleProof = True, .CallbackProof = True}
        Dim binding As New ObservationBinding45 With {.Pid = 45, .StartedUtc = start, .Hwnd = 450, .Session = "synthetic-session", .WorkbookToken = "synthetic-book", .XllSha = build.XllSha, .Mvid = mvid, .Control = "BESHStatNG.Excel2007RefEdit.btnState", .Obligation = "control:BESHStatNG.Excel2007RefEdit.btnState", .ObservedUtc = now}
        Select Case kind
            Case "zero-pid" : observed.Pid = 0
            Case "wrong-pid" : observed.Pid = 46
            Case "recycled-pid" : observed.StartedUtc = start.AddSeconds(1)
            Case "zero-hwnd" : observed.Hwnd = 0
            Case "stale-hwnd" : observed.Hwnd = 451
            Case "window-pid" : observed.WindowPid = 46
            Case "thread" : observed.WindowThreadId = 0
            Case "ambiguous" : observed.Instances = 2
            Case "image" : observed.Image = "synthetic-other"
            Case "bitness" : observed.Bitness = 0
            Case "session" : observed.Session = "synthetic-other"
            Case "desktop" : observed.Desktop = "synthetic-other"
            Case "workbook-path" : expected.WorkbookToken = "<synthetic-root>/book"
            Case "workbook-switch" : observed.WorkbookToken = "synthetic-other"
            Case "not-loaded" : loaded.ModuleProof = False
            Case "forged-path" : loaded.XllPath = "<synthetic-root>/other.xll"
            Case "xll-sha" : loaded.XllShaAfterLoad = New String("f"c, 64)
            Case "dll-sha" : loaded.AssemblySha = New String("f"c, 64)
            Case "mvid" : loaded.Mvid = Guid.Empty
            Case "release-debug" : loaded.Configuration = "Release"
            Case "multiple-versions" : loaded.LoadedVersions = 2
            Case "callbacks-unproved" : loaded.CallbackProof = False
            Case "binding-pid" : binding.Pid = 46
            Case "binding-start" : binding.StartedUtc = start.AddSeconds(-1)
            Case "binding-hwnd" : binding.Hwnd = 451
            Case "binding-module" : binding.Mvid = Guid.Empty
            Case "stale-time" : binding.ObservedUtc = now.AddMinutes(-6)
            Case "future-time" : binding.ObservedUtc = now.AddSeconds(1)
            Case "build-sha" : build.TreeSha = "claim"
            Case "unscoped" : binding.Obligation = Nothing
            Case "missing" : loaded = Nothing
        End Select
        Dim result = Attribution45.Check(build, expected, observed, loaded, binding, now)
        Assert.AreEqual(If(kind = "consistent", "OFFLINE_CONSISTENT_NOT_VERIFIED", "NOT_VERIFIED"), result.Status)
        Assert.IsFalse(result.MayRunNative)
    End Sub

    Private Shared Function NewSession(start As DateTimeOffset) As SessionIdentity45
        Return New SessionIdentity45 With {.Image = "synthetic-excel-image", .Pid = 45, .StartedUtc = start, .Bitness = 64, .Session = "synthetic-session", .Desktop = "synthetic-desktop", .Hwnd = 450, .WindowPid = 45, .WindowThreadId = 1, .Instances = 1, .WorkbookToken = "synthetic-book"}
    End Function
End Class
