<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui11AboutAddin
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui11AboutAddin))
        Me.tbLicense = New System.Windows.Forms.TextBox()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.btnCheckUpdate = New System.Windows.Forms.Button()
        Me.lblAutor = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'tbLicense
        '
        Me.tbLicense.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbLicense.ForeColor = System.Drawing.Color.Red
        Me.tbLicense.Location = New System.Drawing.Point(7, 129)
        Me.tbLicense.Multiline = True
        Me.tbLicense.Name = "tbLicense"
        Me.tbLicense.ReadOnly = True
        Me.tbLicense.Size = New System.Drawing.Size(454, 218)
        Me.tbLicense.TabIndex = 1
        Me.tbLicense.TabStop = False
        Me.tbLicense.Text = resources.GetString("tbLicense.Text")
        Me.tbLicense.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblVersion
        '
        Me.lblVersion.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.2!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblVersion.Location = New System.Drawing.Point(7, 9)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(454, 25)
        Me.lblVersion.TabIndex = 1
        Me.lblVersion.Text = "Version"
        Me.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'btnCheckUpdate
        '
        Me.btnCheckUpdate.Location = New System.Drawing.Point(325, 357)
        Me.btnCheckUpdate.Name = "btnCheckUpdate"
        Me.btnCheckUpdate.Size = New System.Drawing.Size(128, 23)
        Me.btnCheckUpdate.TabIndex = 0
        Me.btnCheckUpdate.Text = "Check for Update"
        Me.btnCheckUpdate.UseVisualStyleBackColor = True
        '
        'lblAutor
        '
        Me.lblAutor.Location = New System.Drawing.Point(7, 34)
        Me.lblAutor.Name = "lblAutor"
        Me.lblAutor.Size = New System.Drawing.Size(454, 37)
        Me.lblAutor.TabIndex = 3
        Me.lblAutor.Text = "Created by Peter Slezak " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "www.beshstat.eu"
        Me.lblAutor.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(7, 71)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(454, 41)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "E-mail: peter.slezak5(at)gmail.com," & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "beshstat(at)beshstat.eu"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'Ui11AboutAddin
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(465, 392)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblAutor)
        Me.Controls.Add(Me.btnCheckUpdate)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.tbLicense)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(483, 439)
        Me.MinimumSize = New System.Drawing.Size(483, 439)
        Me.Name = "Ui11AboutAddin"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "About"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents tbLicense As Windows.Forms.TextBox
    Friend WithEvents lblVersion As Windows.Forms.Label
    Friend WithEvents btnCheckUpdate As Windows.Forms.Button
    Friend WithEvents lblAutor As Windows.Forms.Label
    Friend WithEvents Label1 As Windows.Forms.Label
End Class
