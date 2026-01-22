<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Excel2007RefEdit
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.txtAddress = New System.Windows.Forms.TextBox()
        Me.btnState = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'txtAddress
        '
        Me.txtAddress.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtAddress.Location = New System.Drawing.Point(4, 4)
        Me.txtAddress.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtAddress.Multiline = True
        Me.txtAddress.Name = "txtAddress"
        Me.txtAddress.Size = New System.Drawing.Size(219, 24)
        Me.txtAddress.TabIndex = 0
        '
        'btnState
        '
        Me.btnState.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnState.Image = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.btnState.Location = New System.Drawing.Point(225, 1)
        Me.btnState.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnState.Name = "btnState"
        Me.btnState.Size = New System.Drawing.Size(32, 30)
        Me.btnState.TabIndex = 1
        Me.btnState.UseVisualStyleBackColor = True
        '
        'Excel2007RefEdit
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Transparent
        Me.Controls.Add(Me.btnState)
        Me.Controls.Add(Me.txtAddress)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Name = "Excel2007RefEdit"
        Me.Size = New System.Drawing.Size(260, 32)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtAddress As System.Windows.Forms.TextBox
    Friend WithEvents btnState As System.Windows.Forms.Button

End Class
