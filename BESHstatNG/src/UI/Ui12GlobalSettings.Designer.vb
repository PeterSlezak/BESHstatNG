<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui12GlobalSettings
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
        Me.ckLogging = New System.Windows.Forms.CheckBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.spinBtnPvalueDP = New System.Windows.Forms.NumericUpDown()
        Me.lblPvalueDecimalPlaces = New System.Windows.Forms.Label()
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.lblDefaultRandomSeed = New System.Windows.Forms.Label()
        Me.tbDefaultRandomSeed = New System.Windows.Forms.TextBox()
        Me.cbPvalueSmallValueDisplay = New System.Windows.Forms.ComboBox()
        Me.lblPvalueSmallValueDisplay = New System.Windows.Forms.Label()
        Me.ckPvalueUpperBound = New System.Windows.Forms.CheckBox()
        CType(Me.spinBtnPvalueDP, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ckLogging
        '
        Me.ckLogging.AutoSize = True
        Me.ckLogging.Location = New System.Drawing.Point(38, 12)
        Me.ckLogging.Name = "ckLogging"
        Me.ckLogging.Size = New System.Drawing.Size(181, 20)
        Me.ckLogging.TabIndex = 0
        Me.ckLogging.Text = "Trace Program Execution"
        Me.ckLogging.UseVisualStyleBackColor = True
        '
        'btnOK
        '
        Me.btnOK.Location = New System.Drawing.Point(403, 188)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(75, 23)
        Me.btnOK.TabIndex = 1
        Me.btnOK.Text = "Save"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'spinBtnPvalueDP
        '
        Me.spinBtnPvalueDP.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnPvalueDP.Location = New System.Drawing.Point(223, 72)
        Me.spinBtnPvalueDP.Maximum = New Decimal(New Integer() {16, 0, 0, 0})
        Me.spinBtnPvalueDP.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinBtnPvalueDP.Name = "spinBtnPvalueDP"
        Me.spinBtnPvalueDP.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnPvalueDP.TabIndex = 7
        Me.spinBtnPvalueDP.Value = New Decimal(New Integer() {8, 0, 0, 0})
        '
        'lblPvalueDecimalPlaces
        '
        Me.lblPvalueDecimalPlaces.AutoSize = True
        Me.lblPvalueDecimalPlaces.Location = New System.Drawing.Point(43, 74)
        Me.lblPvalueDecimalPlaces.Name = "lblPvalueDecimalPlaces"
        Me.lblPvalueDecimalPlaces.Size = New System.Drawing.Size(174, 16)
        Me.lblPvalueDecimalPlaces.TabIndex = 8
        Me.lblPvalueDecimalPlaces.Text = "Decimal places for p-values"
        '
        'lblAlpha
        '
        Me.lblAlpha.AutoSize = True
        Me.lblAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlpha.Location = New System.Drawing.Point(133, 49)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(86, 16)
        Me.lblAlpha.TabIndex = 10
        Me.lblAlpha.Text = "Default alpha"
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(223, 47)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 9
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(322, 188)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 11
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'lblDefaultRandomSeed
        '
        Me.lblDefaultRandomSeed.AutoSize = True
        Me.lblDefaultRandomSeed.Location = New System.Drawing.Point(77, 163)
        Me.lblDefaultRandomSeed.Name = "lblDefaultRandomSeed"
        Me.lblDefaultRandomSeed.Size = New System.Drawing.Size(140, 16)
        Me.lblDefaultRandomSeed.TabIndex = 12
        Me.lblDefaultRandomSeed.Text = "Default Random Seed"
        '
        'tbDefaultRandomSeed
        '
        Me.tbDefaultRandomSeed.Location = New System.Drawing.Point(223, 160)
        Me.tbDefaultRandomSeed.Name = "tbDefaultRandomSeed"
        Me.tbDefaultRandomSeed.Size = New System.Drawing.Size(218, 22)
        Me.tbDefaultRandomSeed.TabIndex = 13
        '
        'cbPvalueSmallValueDisplay
        '
        Me.cbPvalueSmallValueDisplay.FormattingEnabled = True
        Me.cbPvalueSmallValueDisplay.Location = New System.Drawing.Point(223, 100)
        Me.cbPvalueSmallValueDisplay.Name = "cbPvalueSmallValueDisplay"
        Me.cbPvalueSmallValueDisplay.Size = New System.Drawing.Size(218, 24)
        Me.cbPvalueSmallValueDisplay.TabIndex = 17
        '
        'lblPvalueSmallValueDisplay
        '
        Me.lblPvalueSmallValueDisplay.AutoSize = True
        Me.lblPvalueSmallValueDisplay.Location = New System.Drawing.Point(92, 103)
        Me.lblPvalueSmallValueDisplay.Name = "lblPvalueSmallValueDisplay"
        Me.lblPvalueSmallValueDisplay.Size = New System.Drawing.Size(125, 16)
        Me.lblPvalueSmallValueDisplay.TabIndex = 16
        Me.lblPvalueSmallValueDisplay.Text = "Very small p-values"
        '
        'ckPvalueUpperBound
        '
        Me.ckPvalueUpperBound.AutoSize = True
        Me.ckPvalueUpperBound.Location = New System.Drawing.Point(223, 128)
        Me.ckPvalueUpperBound.Name = "ckPvalueUpperBound"
        Me.ckPvalueUpperBound.Size = New System.Drawing.Size(255, 20)
        Me.ckPvalueUpperBound.TabIndex = 18
        Me.ckPvalueUpperBound.Text = "Use > threshold for p-values close to 1"
        Me.ckPvalueUpperBound.UseVisualStyleBackColor = True
        '
        'Ui12GlobalSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(494, 218)
        Me.Controls.Add(Me.ckPvalueUpperBound)
        Me.Controls.Add(Me.cbPvalueSmallValueDisplay)
        Me.Controls.Add(Me.lblPvalueSmallValueDisplay)
        Me.Controls.Add(Me.tbDefaultRandomSeed)
        Me.Controls.Add(Me.lblDefaultRandomSeed)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.lblAlpha)
        Me.Controls.Add(Me.spinBtnAlpha)
        Me.Controls.Add(Me.lblPvalueDecimalPlaces)
        Me.Controls.Add(Me.spinBtnPvalueDP)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.ckLogging)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(512, 265)
        Me.MinimumSize = New System.Drawing.Size(512, 265)
        Me.Name = "Ui12GlobalSettings"
        Me.ShowIcon = False
        Me.Text = "Global Settings"
        CType(Me.spinBtnPvalueDP, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ckLogging As Windows.Forms.CheckBox
    Friend WithEvents btnOK As Windows.Forms.Button
    Friend WithEvents spinBtnPvalueDP As Windows.Forms.NumericUpDown
    Friend WithEvents lblPvalueDecimalPlaces As Windows.Forms.Label
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents lblDefaultRandomSeed As Windows.Forms.Label
    Friend WithEvents tbDefaultRandomSeed As Windows.Forms.TextBox
    Friend WithEvents cbPvalueSmallValueDisplay As Windows.Forms.ComboBox
    Friend WithEvents lblPvalueSmallValueDisplay As Windows.Forms.Label
    Friend WithEvents ckPvalueUpperBound As Windows.Forms.CheckBox
End Class
