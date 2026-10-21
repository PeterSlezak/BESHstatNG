Option Explicit On
Option Strict On

Imports System
Imports System.Globalization

Namespace ExcelCommon
    ''' <summary>Offline, ordinal lookup of compiled, validated documentation metadata.</summary>
    Public NotInheritable Class HelpDestination
        Private Sub New(metadata As AnalysisMethodMetadata)
            Me.Metadata = metadata
            Url = If(metadata Is Nothing, GeneratedMethodMetadata.HomeUrl, metadata.HelpUrl)
        End Sub

        Public ReadOnly Property Metadata As AnalysisMethodMetadata
        Public ReadOnly Property Url As String
        Public Function GetLaunchTarget(version As ProductVersionDescriptor,
                                        Optional published As Boolean = HelpLaunchTarget.ResolverPublished) As HelpLaunchTarget
            Return HelpLaunchTarget.Create(Me, version, published)
        End Function
        Public ReadOnly Property IsFallback As Boolean
            Get
                Return Metadata Is Nothing
            End Get
        End Property

        Public Shared Function ForMethodKey(key As String) As HelpDestination
            Dim metadata As AnalysisMethodMetadata = Nothing
            GeneratedMethodMetadata.TryGetByKey(key, metadata)
            Return New HelpDestination(metadata)
        End Function

        Public Shared Function ForAnalysisId(id As String) As HelpDestination
            Dim metadata As AnalysisMethodMetadata = Nothing
            GeneratedMethodMetadata.TryGetByAnalysisId(id, metadata)
            Return New HelpDestination(metadata)
        End Function

        ''' <summary>Utility keys are separate from analysis-method identities.</summary>
        Public Shared Function ForPageKey(key As String) As HelpDestination
            For Each page As AnalysisMethodMetadata In GeneratedMethodMetadata.Pages
                If String.Equals(page.Key, key, StringComparison.Ordinal) Then Return New HelpDestination(page)
            Next
            Return New HelpDestination(Nothing)
        End Function

        Public Shared Function TryMethodKey(key As String, ByRef result As HelpDestination) As Boolean
            result = ForMethodKey(key)
            Return Not result.IsFallback
        End Function

        Public Shared Function TryAnalysisId(id As String, ByRef result As HelpDestination) As Boolean
            result = ForAnalysisId(id)
            Return Not result.IsFallback
        End Function
    End Class

    ''' <summary>Installed assembly version supplied by a host; unrelated to metadata source hash.
    ''' Version-first launch requires an explicitly published resolver; version alone is not deployment evidence.</summary>
    Public NotInheritable Class ProductVersionDescriptor
        Private Sub New(value As Version)
            IsAvailable = value IsNot Nothing
            ProductVersion = If(IsAvailable, value.ToString(4), String.Empty)
            DisplayLabel = If(IsAvailable, "Version " & ProductVersion, "Version unavailable")
        End Sub

        Public ReadOnly Property IsAvailable As Boolean
        Public ReadOnly Property ProductVersion As String
        Public ReadOnly Property DisplayLabel As String
        Public ReadOnly Property HelpRoutePolicy As String
            Get
                Return If(IsAvailable, "verified-version-first", "latest")
            End Get
        End Property

        Public Shared Function FromVersion(value As Version) As ProductVersionDescriptor
            Dim result As ProductVersionDescriptor = Nothing
            TryParse(If(value Is Nothing, Nothing, value.ToString()), result)
            Return result
        End Function

        ''' <summary>Four ASCII unsigned components, 0..65535; all-zero is unavailable.
        ''' Reject whitespace, signs, revision suffixes and incomplete versions.</summary>
        Public Shared Function TryParse(value As String, ByRef result As ProductVersionDescriptor) As Boolean
            result = New ProductVersionDescriptor(Nothing)
            If String.IsNullOrEmpty(value) Then Return False
            Dim parts As String() = value.Split("."c)
            If parts.Length <> 4 Then Return False
            Dim numbers(3) As Integer
            For i As Integer = 0 To 3
                If parts(i).Length = 0 Then Return False
                For Each ch As Char In parts(i)
                    If ch < "0"c OrElse ch > "9"c Then Return False
                Next
                If Not Integer.TryParse(parts(i), NumberStyles.None, CultureInfo.InvariantCulture, numbers(i)) OrElse numbers(i) > 65535 Then Return False
            Next
            If numbers(0) = 0 AndAlso numbers(1) = 0 AndAlso numbers(2) = 0 AndAlso numbers(3) = 0 Then Return False
            result = New ProductVersionDescriptor(New Version(numbers(0), numbers(1), numbers(2), numbers(3)))
            Return True
        End Function
    End Class
End Namespace
