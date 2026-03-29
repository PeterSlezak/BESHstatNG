Imports BESHStatNG.AppInfrastructure

Public Class Ui12GlobalSettings

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Load current session settings into the form.
        Dim settings = AppGlobals.GetCurrentSettings()
        Me.ckLogging.Checked = settings.Diagnostics.TraceExecutionLoggingEnabled
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)

        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub btnOK_Click(sender As Object, e As System.EventArgs) Handles btnOK.Click
        Dim settings = AppGlobals.GetCurrentSettings()

        settings.Diagnostics.TraceExecutionLoggingEnabled = Me.ckLogging.Checked
        settings.DefaultAlpha = CDbl(Me.spinBtnAlpha.Value)
        AppGlobals.ApplySettings(settings)

        Try
            If AppGlobals.gSettingsStore Is Nothing Then
                AppGlobals.gSettingsStore = New AppInfrastructure.BeshStatNgSettingsStore(AppGlobals.gXllPath)
            End If

            AppGlobals.gSettingsStore.Save(settings)
            AppGlobals.BSlogg.Info("Global settings saved.")
            Me.Close()
        Catch ex As Exception
            If AppGlobals.gLogger IsNot Nothing Then
                AppGlobals.gLogger.WriteError(ex, "Failed to save global settings.")
            End If

            MsgBox("Failed to save settings: " & ex.Message, MsgBoxStyle.Exclamation, AppGlobals.gsAPP_TITLE)
        End Try
    End Sub

End Class