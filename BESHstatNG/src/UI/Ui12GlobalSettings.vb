Imports BESHStatNG.AppInfrastructure

Public Class Ui12GlobalSettings

    Public Sub New(tagn As Integer)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Tag = tagn
        ' Load current session settings into the form.
        Dim settings = AppGlobals.GetCurrentSettings()
        Me.ckLogging.Checked = settings.Diagnostics.TraceExecutionLoggingEnabled
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)
        Me.tbDefaultRandomSeed.Text = AppGlobals.GetDefaultRandomSeedText()

        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub btnOK_Click(sender As Object, e As System.EventArgs) Handles btnOK.Click
        Dim settings = AppGlobals.GetCurrentSettings()

        settings.Diagnostics.TraceExecutionLoggingEnabled = Me.ckLogging.Checked
        settings.DefaultAlpha = CDbl(Me.spinBtnAlpha.Value)

        Dim seedText = Me.tbDefaultRandomSeed.Text.Trim()
        If seedText = String.Empty Then
            settings.DefaultRandomSeed = Integer.MinValue
        Else
            Dim parsedSeed As Integer
            If Not Integer.TryParse(seedText, Globalization.NumberStyles.Integer, Globalization.CultureInfo.InvariantCulture, parsedSeed) Then
                Throw New ArgumentException("Default random seed must be a valid 32-bit integer or left blank.")
            End If
            settings.DefaultRandomSeed = parsedSeed
        End If

        AppGlobals.ApplySettings(settings)

        Try
            If AppGlobals.gSettingsStore Is Nothing Then
                AppGlobals.gSettingsStore = New AppInfrastructure.BeshStatNgSettingsStore(AppGlobals.gXllPath)
            End If

            AppGlobals.gSettingsStore.Save(settings)
            CoreServices.Logger.Info("Global settings saved.")
            Me.Close()
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True, "Failed to save global settings")
        End Try
    End Sub

End Class