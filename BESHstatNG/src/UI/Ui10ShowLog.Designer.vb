<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui10ShowLog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.tbErrorLog = New System.Windows.Forms.TextBox()
        Me.btnErrors = New System.Windows.Forms.Button()
        Me.btnWarn = New System.Windows.Forms.Button()
        Me.btnTrace = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'tbErrorLog
        '
        Me.tbErrorLog.Location = New System.Drawing.Point(16, 12)
        Me.tbErrorLog.Multiline = True
        Me.tbErrorLog.Name = "tbErrorLog"
        Me.tbErrorLog.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.tbErrorLog.Size = New System.Drawing.Size(772, 377)
        Me.tbErrorLog.TabIndex = 0
        '
        'btnErrors
        '
        Me.btnErrors.Location = New System.Drawing.Point(699, 405)
        Me.btnErrors.Name = "btnErrors"
        Me.btnErrors.Size = New System.Drawing.Size(89, 23)
        Me.btnErrors.TabIndex = 5
        Me.btnErrors.Text = "Errors"
        Me.btnErrors.UseVisualStyleBackColor = True
        '
        'btnWarn
        '
        Me.btnWarn.Location = New System.Drawing.Point(604, 405)
        Me.btnWarn.Name = "btnWarn"
        Me.btnWarn.Size = New System.Drawing.Size(89, 23)
        Me.btnWarn.TabIndex = 6
        Me.btnWarn.Text = "Warnings"
        Me.btnWarn.UseVisualStyleBackColor = True
        '
        'btnTrace
        '
        Me.btnTrace.Location = New System.Drawing.Point(509, 405)
        Me.btnTrace.Name = "btnTrace"
        Me.btnTrace.Size = New System.Drawing.Size(89, 23)
        Me.btnTrace.TabIndex = 7
        Me.btnTrace.Text = "All"
        Me.btnTrace.UseVisualStyleBackColor = True
        '
        'Ui10ShowLog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 442)
        Me.Controls.Add(Me.btnTrace)
        Me.Controls.Add(Me.btnWarn)
        Me.Controls.Add(Me.btnErrors)
        Me.Controls.Add(Me.tbErrorLog)
        Me.Name = "Ui10ShowLog"
        Me.ShowIcon = False
        Me.Text = "Log"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents tbErrorLog As Windows.Forms.TextBox
    Friend WithEvents btnErrors As Windows.Forms.Button
    Friend WithEvents btnWarn As Windows.Forms.Button
    Friend WithEvents btnTrace As Windows.Forms.Button
End Class
