Public Class Ui12GlobalSettings

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.ckLogging.Checked = BESHstatGlobals.gbTraceLogging
    End Sub

    Private Sub btnOK_Click(sender As Object, e As System.EventArgs) Handles btnOK.Click
        BESHstatGlobals.gbTraceLogging = If(Me.ckLogging.Checked, True, False)
        Me.Close()
    End Sub

End Class