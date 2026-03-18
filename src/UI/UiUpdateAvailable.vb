Option Explicit On

Imports System
Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure

''' <summary>
''' Simple update prompt dialog used by Update.AutoUpdate.
''' </summary>
Public Class UiUpdateAvailable
    Inherits Form

    Private ReadOnly _newVersion As String
    Friend WithEvents btnRemind As Button
    Friend WithEvents btnDownload As Button
    Friend WithEvents btnIgnore As Button
    Friend WithEvents lblInfo As Label
    Private ReadOnly _downloadUrl As String

    Sub New(newVersion As String, downloadUrl As String)
        ' This call is required by the designer.
        InitializeComponent()

        _newVersion = newVersion
        _downloadUrl = downloadUrl

        Me.Text = $"{AppGlobals.gsAPP_TITLE} update available"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MinimizeBox = False
        Me.MaximizeBox = False
        Me.ShowInTaskbar = False
        Me.AutoSize = True
        Me.AutoSizeMode = AutoSizeMode.GrowAndShrink
        Me.Padding = New Padding(14)

        With Me.lblInfo
            .Text = $"A new version of {AppGlobals.gsAPP_TITLE} is available." & Environment.NewLine &
                    $"Installed: {AppGlobals.gAddinVersion}" & Environment.NewLine &
                    $"Available: {_newVersion}"
        End With

        Dim link As New LinkLabel() With {
            .AutoSize = True,
            .Text = "View download page",
            .TabStop = True
        }

        AddHandler link.LinkClicked, Sub()
                                         Try
                                             Process.Start(_downloadUrl)
                                         Catch
                                         End Try
                                     End Sub

        Me.btnDownload.DialogResult = DialogResult.OK
        Me.btnRemind.DialogResult = DialogResult.Retry
        Me.btnIgnore.DialogResult = DialogResult.Ignore
        Me.AcceptButton = btnDownload
        Me.CancelButton = btnRemind
    End Sub

    Private Sub InitializeComponent()
        Me.btnRemind = New System.Windows.Forms.Button()
        Me.btnDownload = New System.Windows.Forms.Button()
        Me.btnIgnore = New System.Windows.Forms.Button()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnRemind
        '
        Me.btnRemind.AutoSize = True
        Me.btnRemind.Location = New System.Drawing.Point(167, 82)
        Me.btnRemind.Name = "btnRemind"
        Me.btnRemind.Size = New System.Drawing.Size(142, 26)
        Me.btnRemind.TabIndex = 0
        Me.btnRemind.Text = "Remind me in 7 days"
        Me.btnRemind.UseVisualStyleBackColor = True
        '
        'btnDownload
        '
        Me.btnDownload.AutoSize = True
        Me.btnDownload.Location = New System.Drawing.Point(49, 82)
        Me.btnDownload.Name = "btnDownload"
        Me.btnDownload.Size = New System.Drawing.Size(112, 26)
        Me.btnDownload.TabIndex = 1
        Me.btnDownload.Text = "Download Now"
        Me.btnDownload.UseVisualStyleBackColor = True
        '
        'btnIgnore
        '
        Me.btnIgnore.AutoSize = True
        Me.btnIgnore.Location = New System.Drawing.Point(315, 82)
        Me.btnIgnore.Name = "btnIgnore"
        Me.btnIgnore.Size = New System.Drawing.Size(127, 26)
        Me.btnIgnore.TabIndex = 2
        Me.btnIgnore.Text = "Ignore this Version"
        Me.btnIgnore.UseVisualStyleBackColor = True
        '
        'lblInfo
        '
        Me.lblInfo.AutoSize = True
        Me.lblInfo.Location = New System.Drawing.Point(23, 9)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(389, 48)
        Me.lblInfo.TabIndex = 3
        Me.lblInfo.Text = "A new version of {AppGlobals.gsAPP_TITLE} is available. " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Installed: {BESHst" &
    "atGlobals.gAddinVersion}" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Available: {_newVersion}"
        '
        'UiUpdateAvailable
        '
        Me.ClientSize = New System.Drawing.Size(502, 133)
        Me.Controls.Add(Me.lblInfo)
        Me.Controls.Add(Me.btnIgnore)
        Me.Controls.Add(Me.btnDownload)
        Me.Controls.Add(Me.btnRemind)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(520, 180)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(520, 180)
        Me.Name = "UiUpdateAvailable"
        Me.ShowIcon = False
        Me.Text = "Auto Update"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
End Class
