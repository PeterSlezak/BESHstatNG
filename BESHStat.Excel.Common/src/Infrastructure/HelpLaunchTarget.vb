Option Explicit On
Option Strict On

Imports System

Namespace ExcelCommon
    ''' <summary>Pure launch policy. Candidates are not claims of page publication.
    ''' Actual per-page verification runs only in the documentation-origin browser resolver.</summary>
    Public NotInheritable Class HelpLaunchTarget
        Public Const ResolverUrl As String = GeneratedMethodMetadata.HomeUrl & "help-resolver/"
#If BESH_HELP_RESOLVER_PUBLISHED Then
        Public Const ResolverPublished As Boolean = True
#Else
        Public Const ResolverPublished As Boolean = False
#End If

        Private Sub New(latest As String, route As String, version As ProductVersionDescriptor, published As Boolean)
            LatestUrl = latest
            HomeUrl = GeneratedMethodMetadata.HomeUrl
            Url = latest
            If route Is Nothing OrElse version Is Nothing OrElse Not version.IsAvailable Then Return
            Dim relative = latest.Substring(GeneratedMethodMetadata.HomeUrl.Length)
            VersionCandidateUrl = GeneratedMethodMetadata.SiteBaseUrl & "/v" & version.ProductVersion & "/" & relative
            ResolverCandidateUrl = ResolverUrl & "?version=" & version.ProductVersion & "&route=" & route
            UsesResolver = published
            If published Then Url = ResolverCandidateUrl
        End Sub

        Public ReadOnly Property Url As String
        Public ReadOnly Property LatestUrl As String
        Public ReadOnly Property HomeUrl As String
        Public ReadOnly Property VersionCandidateUrl As String
        Public ReadOnly Property ResolverCandidateUrl As String
        Public ReadOnly Property UsesResolver As Boolean

        Public Shared Function Create(destination As HelpDestination, version As ProductVersionDescriptor,
                                      Optional published As Boolean = ResolverPublished) As HelpLaunchTarget
            If destination Is Nothing OrElse destination.IsFallback Then Return New HelpLaunchTarget(GeneratedMethodMetadata.HomeUrl, Nothing, version, False)
            Dim key = destination.Metadata.Key
            Dim known = HelpDestination.ForMethodKey(key)
            If known.IsFallback Then known = HelpDestination.ForPageKey(key)
            If known.IsFallback Then Return New HelpLaunchTarget(GeneratedMethodMetadata.HomeUrl, Nothing, version, False)
            Return New HelpLaunchTarget(known.Url, key, version, published)
        End Function

        Public Shared Function CreateHome(version As ProductVersionDescriptor,
                                          Optional published As Boolean = ResolverPublished) As HelpLaunchTarget
            Return New HelpLaunchTarget(GeneratedMethodMetadata.HomeUrl, "Home", version, published)
        End Function

        ''' <summary>Exact allowlist, including strictly canonical resolver queries only after activation.
        ''' Never accepts arbitrary HTTPS links, raw version URLs or caller-provided path/domain data.</summary>
        Public Shared Function SafeOpenUrl(value As String, Optional published As Boolean = ResolverPublished) As String
            If String.Equals(value, GeneratedMethodMetadata.HomeUrl, StringComparison.Ordinal) Then Return value
            For Each metadata In GeneratedMethodMetadata.All
                If String.Equals(value, metadata.HelpUrl, StringComparison.Ordinal) Then Return value
            Next
            For Each page In GeneratedMethodMetadata.Pages
                If String.Equals(value, page.HelpUrl, StringComparison.Ordinal) Then Return value
            Next
            Dim prefix = ResolverUrl & "?version="
            If Not published OrElse value Is Nothing OrElse Not value.StartsWith(prefix, StringComparison.Ordinal) Then Return GeneratedMethodMetadata.HomeUrl
            Dim parts = value.Substring(prefix.Length).Split(New String() {"&route="}, StringSplitOptions.None)
            If parts.Length <> 2 Then Return GeneratedMethodMetadata.HomeUrl
            Dim version As ProductVersionDescriptor = Nothing
            If Not ProductVersionDescriptor.TryParse(parts(0), version) OrElse Not String.Equals(parts(0), version.ProductVersion, StringComparison.Ordinal) Then Return GeneratedMethodMetadata.HomeUrl
            Dim target As HelpLaunchTarget
            If String.Equals(parts(1), "Home", StringComparison.Ordinal) Then
                target = CreateHome(version, True)
            Else
                Dim destination = HelpDestination.ForMethodKey(parts(1))
                If destination.IsFallback Then destination = HelpDestination.ForPageKey(parts(1))
                target = Create(destination, version, True)
            End If
            If String.Equals(value, target.ResolverCandidateUrl, StringComparison.Ordinal) Then Return value
            Return GeneratedMethodMetadata.HomeUrl
        End Function
    End Class
End Namespace
