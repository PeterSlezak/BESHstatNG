<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui12SampleSizeTtestSingleProp
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
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.lblSettings = New System.Windows.Forms.Label()
        Me.lblBeta = New System.Windows.Forms.Label()
        Me.spinBtnBeta = New System.Windows.Forms.NumericUpDown()
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        Me.tbKappa = New System.Windows.Forms.TextBox()
        Me.lblKappa = New System.Windows.Forms.Label()
        Me.tbSD = New System.Windows.Forms.TextBox()
        Me.lblSD = New System.Windows.Forms.Label()
        Me.tbMeanDiff = New System.Windows.Forms.TextBox()
        Me.lblMeanDiff = New System.Windows.Forms.Label()
        Me.tbOutput = New System.Windows.Forms.TextBox()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.btnSaveToSheet = New System.Windows.Forms.Button()
        Me.grpInput.SuspendLayout()
        CType(Me.spinBtnBeta, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.lblSettings)
        Me.grpInput.Controls.Add(Me.lblBeta)
        Me.grpInput.Controls.Add(Me.spinBtnBeta)
        Me.grpInput.Controls.Add(Me.lblAlpha)
        Me.grpInput.Controls.Add(Me.spinBtnAlpha)
        Me.grpInput.Controls.Add(Me.tbKappa)
        Me.grpInput.Controls.Add(Me.lblKappa)
        Me.grpInput.Controls.Add(Me.tbSD)
        Me.grpInput.Controls.Add(Me.lblSD)
        Me.grpInput.Controls.Add(Me.tbMeanDiff)
        Me.grpInput.Controls.Add(Me.lblMeanDiff)
        Me.grpInput.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpInput.Location = New System.Drawing.Point(12, 9)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(535, 169)
        Me.grpInput.TabIndex = 0
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'lblSettings
        '
        Me.lblSettings.AutoSize = True
        Me.lblSettings.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSettings.Location = New System.Drawing.Point(14, 141)
        Me.lblSettings.Name = "lblSettings"
        Me.lblSettings.Size = New System.Drawing.Size(48, 16)
        Me.lblSettings.TabIndex = 10
        Me.lblSettings.Text = "Label1"
        '
        'lblBeta
        '
        Me.lblBeta.AutoSize = True
        Me.lblBeta.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBeta.Location = New System.Drawing.Point(395, 64)
        Me.lblBeta.Name = "lblBeta"
        Me.lblBeta.Size = New System.Drawing.Size(35, 16)
        Me.lblBeta.TabIndex = 9
        Me.lblBeta.Text = "Beta"
        '
        'spinBtnBeta
        '
        Me.spinBtnBeta.DecimalPlaces = 3
        Me.spinBtnBeta.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnBeta.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnBeta.Location = New System.Drawing.Point(443, 62)
        Me.spinBtnBeta.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnBeta.Name = "spinBtnBeta"
        Me.spinBtnBeta.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnBeta.TabIndex = 8
        Me.spinBtnBeta.Value = New Decimal(New Integer() {20, 0, 0, 131072})
        '
        'lblAlpha
        '
        Me.lblAlpha.AutoSize = True
        Me.lblAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlpha.Location = New System.Drawing.Point(395, 36)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(42, 16)
        Me.lblAlpha.TabIndex = 7
        Me.lblAlpha.Text = "Alpha"
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(443, 34)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 6
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'tbKappa
        '
        Me.tbKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbKappa.Location = New System.Drawing.Point(276, 93)
        Me.tbKappa.Name = "tbKappa"
        Me.tbKappa.Size = New System.Drawing.Size(72, 22)
        Me.tbKappa.TabIndex = 5
        Me.tbKappa.Text = "1"
        '
        'lblKappa
        '
        Me.lblKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblKappa.Location = New System.Drawing.Point(11, 90)
        Me.lblKappa.Name = "lblKappa"
        Me.lblKappa.Size = New System.Drawing.Size(248, 25)
        Me.lblKappa.TabIndex = 4
        Me.lblKappa.Text = "Ratio of control to experimental subjects"
        '
        'tbSD
        '
        Me.tbSD.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbSD.Location = New System.Drawing.Point(276, 65)
        Me.tbSD.Name = "tbSD"
        Me.tbSD.Size = New System.Drawing.Size(72, 22)
        Me.tbSD.TabIndex = 3
        Me.tbSD.Text = "10"
        '
        'lblSD
        '
        Me.lblSD.AutoSize = True
        Me.lblSD.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSD.Location = New System.Drawing.Point(78, 68)
        Me.lblSD.Name = "lblSD"
        Me.lblSD.Size = New System.Drawing.Size(122, 16)
        Me.lblSD.TabIndex = 2
        Me.lblSD.Text = "Standard Deviation"
        '
        'tbMeanDiff
        '
        Me.tbMeanDiff.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMeanDiff.Location = New System.Drawing.Point(276, 37)
        Me.tbMeanDiff.Name = "tbMeanDiff"
        Me.tbMeanDiff.Size = New System.Drawing.Size(72, 22)
        Me.tbMeanDiff.TabIndex = 1
        Me.tbMeanDiff.Text = "5"
        '
        'lblMeanDiff
        '
        Me.lblMeanDiff.AutoSize = True
        Me.lblMeanDiff.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMeanDiff.Location = New System.Drawing.Point(78, 40)
        Me.lblMeanDiff.Name = "lblMeanDiff"
        Me.lblMeanDiff.Size = New System.Drawing.Size(105, 16)
        Me.lblMeanDiff.TabIndex = 0
        Me.lblMeanDiff.Text = "Mean Difference"
        '
        'tbOutput
        '
        Me.tbOutput.Location = New System.Drawing.Point(10, 184)
        Me.tbOutput.Multiline = True
        Me.tbOutput.Name = "tbOutput"
        Me.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.tbOutput.Size = New System.Drawing.Size(537, 193)
        Me.tbOutput.TabIndex = 1
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(391, 383)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 4
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(472, 383)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 3
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'btnSaveToSheet
        '
        Me.btnSaveToSheet.Location = New System.Drawing.Point(310, 383)
        Me.btnSaveToSheet.Name = "btnSaveToSheet"
        Me.btnSaveToSheet.Size = New System.Drawing.Size(75, 23)
        Me.btnSaveToSheet.TabIndex = 5
        Me.btnSaveToSheet.Text = "Save"
        Me.btnSaveToSheet.UseVisualStyleBackColor = True
        '
        'Ui12SampleSizeTtestSingleProp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(559, 413)
        Me.Controls.Add(Me.btnSaveToSheet)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.tbOutput)
        Me.Controls.Add(Me.grpInput)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(577, 407)
        Me.Name = "Ui12SampleSizeTtestSingleProp"
        Me.ShowIcon = False
        Me.Text = "Ui12SampleSizeTtestSingleProp"
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        CType(Me.spinBtnBeta, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents tbMeanDiff As Windows.Forms.TextBox
    Friend WithEvents lblMeanDiff As Windows.Forms.Label
    Friend WithEvents tbKappa As Windows.Forms.TextBox
    Friend WithEvents lblKappa As Windows.Forms.Label
    Friend WithEvents tbSD As Windows.Forms.TextBox
    Friend WithEvents lblSD As Windows.Forms.Label
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents lblBeta As Windows.Forms.Label
    Friend WithEvents spinBtnBeta As Windows.Forms.NumericUpDown
    Friend WithEvents tbOutput As Windows.Forms.TextBox
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents btnSaveToSheet As Windows.Forms.Button
    Friend WithEvents lblSettings As Windows.Forms.Label
End Class
