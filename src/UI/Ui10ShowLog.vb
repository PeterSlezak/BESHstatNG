Imports System.IO
Imports BESHStatNG.AppInfrastructure

Public Class Ui10ShowLog
    Private ex As Exception
    Sub New(Optional e As Exception = Nothing)
        ' This call is required by the designer.
        InitializeComponent()

        Me.tbErrorLog.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                               Windows.Forms.AnchorStyles.Top Or
                               Windows.Forms.AnchorStyles.Left Or
                               Windows.Forms.AnchorStyles.Right
        Me.btnWarn.Anchor = Windows.Forms.AnchorStyles.Bottom Or Windows.Forms.AnchorStyles.Right
        Me.btnTrace.Anchor = Windows.Forms.AnchorStyles.Bottom Or Windows.Forms.AnchorStyles.Right
        Me.btnErrors.Anchor = Windows.Forms.AnchorStyles.Bottom Or Windows.Forms.AnchorStyles.Right

        Me.LoadFileToTextbox(AppGlobals.gLogFile)
        Me.Text = "Trace Log"
    End Sub

    Private Sub LoadFileToTextbox(path As String)
        Dim LogFileStream As FileStream
        Dim LogFileReader As StreamReader
        Dim strRowText As String

        LogFileStream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        LogFileReader = New StreamReader(LogFileStream)
        strRowText = LogFileReader.ReadToEnd()
        Me.tbErrorLog.Clear()
        Me.tbErrorLog.AppendText(strRowText)
        LogFileReader.Dispose()
        LogFileStream.Dispose()
    End Sub

    Private Sub btnTrace_Click(sender As Object, e As System.EventArgs) Handles btnTrace.Click
        Me.LoadFileToTextbox(AppGlobals.gLogFile)
        Me.Text = "Trace Log"
    End Sub

    Private Sub btnErrors_Click(sender As Object, e As System.EventArgs) Handles btnErrors.Click
        Dim dir = Path.GetDirectoryName(AppGlobals.gLogFile)
        Me.LoadFileToTextbox(Path.Combine(dir, "err.log"))
        Me.Text = "Errors Log"
    End Sub

    Private Sub btnWarn_Click(sender As Object, e As System.EventArgs) Handles btnWarn.Click
        Dim dir = Path.GetDirectoryName(AppGlobals.gLogFile)
        Me.LoadFileToTextbox(Path.Combine(dir, "errwarn.log"))
        Me.Text = "Errors and Warnings Log"
    End Sub
End Class