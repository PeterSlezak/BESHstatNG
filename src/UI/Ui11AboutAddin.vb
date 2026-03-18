Imports System.Drawing
Imports System.Security.Policy
Imports System.Xml
Imports BESHStatNG.AppInfrastructure

Public Class Ui11AboutAddin
    Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.Text = "About " & AppGlobals.gsAPP_TITLE
        lblVersion.Text = $"Version {AppGlobals.gAddinVersion} ({AppGlobals.GetBuildDateIso})"

        Me.tbLicense.ReadOnly = True
        Me.tbLicense.ForeColor = Color.Red
        Me.tbLicense.BackColor = Me.tbLicense.BackColor
    End Sub

    Private Sub btnCheckUpdate_Click(sender As Object, e As System.EventArgs) Handles btnCheckUpdate.Click
        Dim sErr As String = String.Empty, strDownloadUrl As String = String.Empty, strNewVersion As String = String.Empty
        If IsInternetConnected() Then
            If IsThereAnUpdate(sErr, strDownloadUrl, strNewVersion) Then
                If MsgBox($"There is an update for {AppGlobals.gsAPP_TITLE} available." & vbNewLine &
                          $"Do you want to download new version {strNewVersion} now?",
                          vbQuestion + vbYesNo, AppGlobals.gsAPP_TITLE) = vbYes Then
                    Process.Start(strDownloadUrl)
                End If
            Else
                MsgBox(AppGlobals.gsAPP_TITLE & " is up to date.", vbInformation + vbOKOnly, AppGlobals.gsAPP_TITLE)
            End If
        Else
            MsgBox("There seems to be no internet connection.", vbOKOnly, AppGlobals.gsAPP_TITLE)
        End If
    End Sub

    Private Function IsThereAnUpdate(ByRef sError As String, ByRef downloadURL As String, ByRef strNewVersion As String, Optional url As String = "") As Boolean
        Dim sUrl = If(url = String.Empty, AppGlobals.gCheckUpdateUrl, url)
        Dim doc As New Xml.XmlDocument
        Dim bRet As Boolean = False
        Try
            doc.Load(sUrl)
            'Dim root As XmlNode = doc.FirstChild
            Dim currentVer As Xml.XmlNodeList = doc.SelectNodes("root/CurrentVersion/update")
            strNewVersion = currentVer(0).Attributes.GetNamedItem("version").Value
            If Me.IsNewerVersion(AppGlobals.gAddinVersion, strNewVersion) Then
                bRet = True
                downloadURL = currentVer(0).Attributes.GetNamedItem("detailsURL").Value
            End If
        Catch
            sError = "Can't acquire information about current BESH Stat version from the web. Try again later."
            Return bRet
        End Try
        Return bRet
    End Function

    Private Function IsNewerVersion(strCurrentVersion As String, strNewVersion As String) As Boolean
        Dim currentVer() As String = strCurrentVersion.Split(".")
        Dim newVer() As String = strNewVersion.Split(".")
        Dim l As Integer = Math.Min(currentVer.Length, newVer.Length)
        Dim bIsNewer = False
        For i = 0 To l - 1
            If Int(currentVer(i)) = Int(newVer(i)) Then
                Continue For
            ElseIf Int(currentVer(i)) > Int(newVer(i)) Then
                bIsNewer = False
                Exit For
            ElseIf Int(currentVer(i)) < Int(newVer(i)) Then
                bIsNewer = True
                Exit For
            End If
        Next
        Return bIsNewer
    End Function

    Private Function IsInternetConnected() As Boolean
        Dim bConnected As Boolean = False
        If My.Computer.Network.IsAvailable Then
            Try
                bConnected = If(My.Computer.Network.Ping("www.Google.com"), True, False)
            Catch
                bConnected = False
            End Try
        Else
            bConnected = False
        End If
        Return bConnected
    End Function

End Class