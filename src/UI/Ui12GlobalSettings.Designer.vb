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
        Me.lblAlphaOutliers = New System.Windows.Forms.Label()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        CType(Me.spinBtnPvalueDP, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ckLogging
        '
        Me.ckLogging.AutoSize = True
        Me.ckLogging.Location = New System.Drawing.Point(24, 24)
        Me.ckLogging.Name = "ckLogging"
        Me.ckLogging.Size = New System.Drawing.Size(249, 20)
        Me.ckLogging.TabIndex = 0
        Me.ckLogging.Text = "Trace Execution During This Session"
        Me.ckLogging.UseVisualStyleBackColor = True
        '
        'btnOK
        '
        Me.btnOK.Location = New System.Drawing.Point(409, 153)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(75, 23)
        Me.btnOK.TabIndex = 1
        Me.btnOK.Text = "Save"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'spinBtnPvalueDP
        '
        Me.spinBtnPvalueDP.Enabled = False
        Me.spinBtnPvalueDP.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnPvalueDP.Location = New System.Drawing.Point(263, 61)
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
        Me.lblPvalueDecimalPlaces.Enabled = False
        Me.lblPvalueDecimalPlaces.Location = New System.Drawing.Point(21, 63)
        Me.lblPvalueDecimalPlaces.Name = "lblPvalueDecimalPlaces"
        Me.lblPvalueDecimalPlaces.Size = New System.Drawing.Size(236, 16)
        Me.lblPvalueDecimalPlaces.TabIndex = 8
        Me.lblPvalueDecimalPlaces.Text = "Decimal Places for P-value Presenting"
        '
        'lblAlphaOutliers
        '
        Me.lblAlphaOutliers.AutoSize = True
        Me.lblAlphaOutliers.Enabled = False
        Me.lblAlphaOutliers.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlphaOutliers.Location = New System.Drawing.Point(215, 96)
        Me.lblAlphaOutliers.Name = "lblAlphaOutliers"
        Me.lblAlphaOutliers.Size = New System.Drawing.Size(42, 16)
        Me.lblAlphaOutliers.TabIndex = 10
        Me.lblAlphaOutliers.Text = "Alpha"
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Enabled = False
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(263, 94)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 9
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'Ui12GlobalSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(494, 186)
        Me.Controls.Add(Me.lblAlphaOutliers)
        Me.Controls.Add(Me.spinBtnAlpha)
        Me.Controls.Add(Me.lblPvalueDecimalPlaces)
        Me.Controls.Add(Me.spinBtnPvalueDP)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.ckLogging)
        Me.MaximizeBox = False
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
    Friend WithEvents lblAlphaOutliers As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
End Class
