Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Xml.Linq

Namespace Harness45
    ' Windows test assembly only. No native adapter, enumerator, fallback or launch API.
    Friend NotInheritable Class CallbackHarness45
        Friend Shared Function Resolve(owner As Type, callbackName As String, attribute As String) As MethodInfo
            If owner Is Nothing OrElse String.IsNullOrWhiteSpace(callbackName) Then Throw New ArgumentException("Missing callback")
            Dim methods = owner.GetMethods(BindingFlags.Public Or BindingFlags.Instance).Where(Function(m) m.Name = callbackName).ToArray()
            If methods.Length <> 1 Then Throw New InvalidOperationException("Unresolved or ambiguous callback")
            Dim method = methods(0), args = method.GetParameters()
            If attribute = "loadImage" Then
                If args.Length <> 1 OrElse args(0).ParameterType IsNot GetType(String) OrElse method.ReturnType IsNot GetType(Object) Then Throw New InvalidOperationException("Wrong image callback signature")
            Else
                If args.Length <> 1 OrElse args(0).ParameterType.FullName <> "ExcelDna.Integration.CustomUI.IRibbonControl" Then Throw New InvalidOperationException("Wrong control callback signature")
                Dim result = If(attribute = "onAction", GetType(Void), If(attribute = "getEnabled" OrElse attribute = "getVisible", GetType(Boolean), Nothing))
                If result Is Nothing OrElse method.ReturnType IsNot result Then Throw New InvalidOperationException("Unsupported callback contract")
            End If
            Return method
        End Function

        Friend Shared Function Validate(xml As String, owner As Type) As List(Of MethodInfo)
            Dim document = XDocument.Parse(xml), ids As New HashSet(Of String)(StringComparer.Ordinal), result As New List(Of MethodInfo)
            For Each element In document.Root.DescendantsAndSelf()
                Dim id = element.Attribute("id")
                If id IsNot Nothing AndAlso Not ids.Add(id.Value) Then Throw New InvalidOperationException("Duplicate control ID")
                If element.Attribute("enabled") IsNot Nothing AndAlso element.Attribute("enabled").Value = "false" AndAlso element.Attribute("onAction") IsNot Nothing Then
                    ' Disabled commands remain declared but cannot be dispatched below.
                End If
                For Each attribute In element.Attributes()
                    If attribute.Name.LocalName.StartsWith("on", StringComparison.Ordinal) OrElse attribute.Name.LocalName.StartsWith("get", StringComparison.Ordinal) OrElse attribute.Name.LocalName = "loadImage" Then result.Add(Resolve(owner, attribute.Value, attribute.Name.LocalName))
                Next
            Next
            Return result
        End Function

        Friend Shared Sub DispatchFake(xml As String, controlId As String)
            Validate(xml, GetType(Global.BESHStatNG.Ribbon)) ' Metadata only: no Ribbon instance/static initialization.
            Dim control = XDocument.Parse(xml).Descendants().Single(Function(e) CStr(e.Attribute("id")) = controlId)
            If CStr(control.Attribute("enabled")) = "false" Then Throw New InvalidOperationException("Disabled control")
            Dim callbackName = CStr(control.Attribute("onAction"))
            If callbackName Is Nothing Then Throw New InvalidOperationException("Nonexecutable control")
            ' Closed projection type: callers cannot inject a real host/delegate.
            Resolve(GetType(RibbonProjection45), callbackName, "onAction").Invoke(New RibbonProjection45(), New Object() {Nothing})
        End Sub
    End Class

    Friend Class ReviewedBuild45
        Friend Commit As String
        Friend TreeSha As String
        Friend Configuration As String
        Friend Bitness As Integer
        Friend AssemblySha As String
        Friend AssemblyMvid As Guid
        Friend XllSha As String
        Friend XllPath As String
        Friend DependenciesSha As String
        Friend ProductVersion As String
        Friend BuildUtc As DateTimeOffset
    End Class

    Friend Class SessionIdentity45
        Friend Image As String
        Friend Pid As Integer
        Friend StartedUtc As DateTimeOffset
        Friend Bitness As Integer
        Friend Session As String
        Friend Desktop As String
        Friend Hwnd As Long
        Friend WindowPid As Integer
        Friend WindowThreadId As Integer
        Friend Instances As Integer
        Friend WorkbookToken As String
    End Class

    Friend Class LoadedCode45
        Friend XllPath As String
        Friend XllShaAfterLoad As String
        Friend AssemblySha As String
        Friend Mvid As Guid
        Friend Configuration As String
        Friend Bitness As Integer
        Friend ProductVersion As String
        Friend LoadedVersions As Integer
        Friend ModuleProof As Boolean
        Friend CallbackProof As Boolean
    End Class

    Friend Class ObservationBinding45
        Friend Pid As Integer
        Friend StartedUtc As DateTimeOffset
        Friend Hwnd As Long
        Friend Session As String
        Friend WorkbookToken As String
        Friend XllSha As String
        Friend Mvid As Guid
        Friend Control As String
        Friend Obligation As String
        Friend ObservedUtc As DateTimeOffset
    End Class

    Friend Class AttributionResult45
        Friend ReadOnly Reasons As New List(Of String)
        Friend ReadOnly Property Status As String
            Get
                Return If(Reasons.Count = 0, "OFFLINE_CONSISTENT_NOT_VERIFIED", "NOT_VERIFIED")
            End Get
        End Property
        Friend ReadOnly Property MayRunNative As Boolean
            Get
                Return False ' No approval/authentic-observation verifier exists in this batch.
            End Get
        End Property
    End Class

    Friend NotInheritable Class Attribution45
        Private Shared Function Hash(value As String, length As Integer) As Boolean
            Return value IsNot Nothing AndAlso value.Length = length AndAlso value.All(Function(c) "0123456789abcdef".Contains(c))
        End Function
        Friend Shared Function Check(build As ReviewedBuild45, expected As SessionIdentity45, observed As SessionIdentity45, loaded As LoadedCode45, binding As ObservationBinding45, now As DateTimeOffset) As AttributionResult45
            Dim r As New AttributionResult45
            If build Is Nothing OrElse expected Is Nothing OrElse observed Is Nothing OrElse loaded Is Nothing OrElse binding Is Nothing Then
                r.Reasons.Add("MISSING_EVIDENCE") : Return r
            End If
            If Not Hash(build.Commit, 40) OrElse Not Hash(build.TreeSha, 64) OrElse Not Hash(build.DependenciesSha, 64) OrElse Not Hash(build.AssemblySha, 64) OrElse Not Hash(build.XllSha, 64) OrElse build.AssemblyMvid = Guid.Empty OrElse String.IsNullOrWhiteSpace(build.ProductVersion) OrElse build.BuildUtc > now Then r.Reasons.Add("REVIEWED_BUILD")
            If (build.Configuration <> "Debug" AndAlso build.Configuration <> "Release") OrElse (build.Bitness <> 32 AndAlso build.Bitness <> 64) Then r.Reasons.Add("BUILD_CONFIG")
            If observed.Pid <= 0 OrElse expected.Pid <= 0 OrElse expected.Pid <> observed.Pid OrElse expected.StartedUtc <> observed.StartedUtc OrElse observed.StartedUtc > now Then r.Reasons.Add("PID_START")
            If observed.Hwnd = 0 OrElse expected.Hwnd <> observed.Hwnd OrElse observed.WindowPid <> observed.Pid OrElse observed.WindowThreadId <= 0 Then r.Reasons.Add("WINDOW_OWNER")
            If observed.Instances <> 1 OrElse expected.Instances <> 1 Then r.Reasons.Add("AMBIGUOUS_INSTANCE")
            If String.IsNullOrWhiteSpace(observed.Image) OrElse observed.Image <> expected.Image OrElse String.IsNullOrWhiteSpace(observed.Session) OrElse String.IsNullOrWhiteSpace(observed.Desktop) OrElse observed.Session <> expected.Session OrElse observed.Desktop <> expected.Desktop Then r.Reasons.Add("SESSION_IMAGE")
            If observed.Bitness <> build.Bitness OrElse observed.Bitness <> expected.Bitness OrElse loaded.Bitness <> build.Bitness Then r.Reasons.Add("BITNESS")
            If String.IsNullOrWhiteSpace(expected.WorkbookToken) OrElse expected.WorkbookToken.IndexOfAny(New Char() {":"c, "/"c, ChrW(92)}) >= 0 OrElse observed.WorkbookToken <> expected.WorkbookToken Then r.Reasons.Add("WORKBOOK_IDENTITY")
            If String.IsNullOrWhiteSpace(build.XllPath) OrElse Not build.XllPath.StartsWith("<synthetic-root>/", StringComparison.Ordinal) OrElse loaded.XllPath <> build.XllPath OrElse loaded.XllShaAfterLoad <> build.XllSha OrElse Not loaded.ModuleProof Then r.Reasons.Add("LOADED_XLL")
            If loaded.AssemblySha <> build.AssemblySha OrElse loaded.Mvid <> build.AssemblyMvid OrElse loaded.Configuration <> build.Configuration OrElse loaded.ProductVersion <> build.ProductVersion OrElse loaded.LoadedVersions <> 1 OrElse Not loaded.CallbackProof Then r.Reasons.Add("LOADED_MANAGED_CODE")
            If binding.Pid <> observed.Pid OrElse binding.StartedUtc <> observed.StartedUtc OrElse binding.Hwnd <> observed.Hwnd OrElse binding.Session <> observed.Session OrElse binding.WorkbookToken <> observed.WorkbookToken OrElse binding.XllSha <> loaded.XllShaAfterLoad OrElse binding.Mvid <> loaded.Mvid Then r.Reasons.Add("OBSERVATION_BINDING")
            If binding.ObservedUtc < observed.StartedUtc OrElse binding.ObservedUtc > now OrElse now - binding.ObservedUtc > TimeSpan.FromMinutes(5) OrElse String.IsNullOrWhiteSpace(binding.Control) OrElse String.IsNullOrWhiteSpace(binding.Obligation) Then r.Reasons.Add("STALE_OR_UNSCOPED_OBSERVATION")
            Return r
        End Function
    End Class
End Namespace
